using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerQuest
{
    // Carreira: XP por habilidade (categoria) define senioridade e especialização do título.
    static class Career
    {
        public static readonly int[] SenXp = { 0, 200, 600, 1200, 2000, 3000 };
        public static readonly string[] SenName = { "Estagiário(a)", "Júnior", "Pleno", "Sênior", "Especialista", "Principal" };
        static readonly string[] SenSuffix = { "", "Jr", "Pleno", "Sênior", "Especialista", "Principal" };

        public static int TotalXp(Progress p) { return p.Xp.Values.Sum(); }

        public static int Seniority(int xp)
        {
            int s = 0;
            for (int i = 0; i < SenXp.Length; i++) if (xp >= SenXp[i]) s = i;
            return s;
        }

        public static int NextXp(int xp) { int s = Seniority(xp); return s + 1 < SenXp.Length ? SenXp[s + 1] : SenXp[s]; }

        static int X(Progress p, string c) { int v; return p.Xp.TryGetValue(c, out v) ? v : 0; }

        const int MinSkill = 40;   // XP mínimo numa área para ela contar no título (uns 2 ou 3 desafios)

        // Especialização a partir das habilidades mais fortes
        public static string Specialization(Progress p)
        {
            var cats = Challenges.Cats.Select(c => new KeyValuePair<string, int>(c.Id, X(p, c.Id))).Where(kv => kv.Value >= MinSkill).OrderByDescending(kv => kv.Value).ToList();
            if (cats.Count == 0) return null;
            int top = cats[0].Value;
            var strong = new HashSet<string>(cats.Where(kv => kv.Value >= top * 0.35).Select(kv => kv.Key));
            string first = cats[0].Key;
            bool front = strong.Contains("front") || strong.Contains("react");
            bool back = strong.Contains("python") || strong.Contains("php");
            bool mobile = strong.Contains("flutter");
            bool docker = strong.Contains("docker"), aws = strong.Contains("aws"), redes = strong.Contains("redes"), sql = strong.Contains("sql");

            if (strong.Count >= 5) return "Engenheiro(a) de Software Generalista";
            if (front && back && (docker || aws)) return "Dev Fullstack Cloud";
            if (front && back) return "Dev Fullstack";
            if (mobile && (front || back)) return "Dev Multiplataforma";
            if (mobile) return "Dev Mobile";
            if (sql && strong.Contains("python")) return "Engenheiro(a) de Dados";
            if (sql && aws) return "Engenheiro(a) de Dados Cloud";
            if (docker && aws) return first == "aws" ? "Cloud Engineer" : "Engenheiro(a) DevOps";
            if (docker && redes) return "SRE";
            if (aws && redes) return "Engenheiro(a) de Redes Cloud";
            if (back && (docker || aws)) return "Engenheiro(a) de Plataforma";
            if (front && (docker || aws)) return "Dev Frontend Cloud";
            if (strong.Contains("python") && strong.Contains("php")) return "Dev Backend";
            if (strong.Contains("python")) return "Dev Python";
            if (strong.Contains("php")) return "Dev Backend PHP";
            if (strong.Contains("react")) return strong.Contains("front") ? "Dev Frontend React" : "Dev React";
            if (strong.Contains("front")) return "Dev Frontend";
            if (docker) return "Engenheiro(a) DevOps";
            if (aws) return "Cloud Engineer";
            if (redes) return "Analista de Redes";
            if (sql) return "Analista de Dados";
            return null;
        }

        public static string Title(Progress p) { return Gender.Apply(TitleRaw(p)); }

        static string TitleRaw(Progress p)
        {
            int s = Seniority(TotalXp(p));
            string spec = Specialization(p);
            if (s == 0) return spec == null ? "Estagiário(a)" : "Estagiário(a) · " + spec;
            if (spec == null) spec = "Profissional de TI";
            return spec + " " + SenSuffix[s];
        }

        public static string RepLabel(int rep)
        {
            return rep >= 85 ? "Referência" : rep >= 65 ? "Confiável" : rep >= 40 ? "Neutra" : rep >= 20 ? "Arranhada" : "Queimado(a)";
        }

        public static float XpMultiplier(Progress p)
        {
            float m = 1f;
            if (p.Owned.Contains("teclado")) m += 0.1f;
            m += PlayerRoom.XpBonus(p);   // computador da sua sala
            if (p.Reputation >= 80) m += 0.1f;
            if (p.Reputation < 20) m -= 0.25f;
            return m;
        }
    }

    // Tratamento nos textos: "Estagiário(a)", "Bem-vindo(a)", "o(a) mentor(a)"... viram masculino, feminino ou ficam neutros.
    static class Gender
    {
        public static int Mode = 2;   // 0 masculino, 1 feminino, 2 neutro

        static readonly System.Text.RegularExpressions.Regex Rx =
            new System.Text.RegularExpressions.Regex(@"(\w*)\(([aA])\)", System.Text.RegularExpressions.RegexOptions.Compiled);

        public static string Apply(string s)
        {
            if (string.IsNullOrEmpty(s) || Mode == 2 || s.IndexOf('(') < 0) return s;
            return Rx.Replace(s, m =>
            {
                string w = m.Groups[1].Value, a = m.Groups[2].Value;
                bool upper = a == "A";
                if (w.Length == 0) return m.Value;
                if (Mode == 0) return w;                                        // masculino: só tira o "(a)"
                if (w == "ele" || w == "ELE") return upper ? "ELA" : "ela";
                char last = w[w.Length - 1];
                if (last == 'o' || last == 'O') return w.Substring(0, w.Length - 1) + a;   // estagiário(a) → estagiária
                return w + a;                                                   // explorador(a) → exploradora
            });
        }
    }

    // Eventos de diálogo com escolhas e consequências
    class EventOption
    {
        public string Label, Reply;
        public int Rep, Xp, Pts;
        public string Item;      // item ganho (consumível)
        public bool Good;        // escolha ética/positiva
        public bool Flash;       // dispara um desafio relâmpago
    }

    class DialogEvent
    {
        public string Id, Intro;
        public EventOption[] Options;
    }

    static class Events
    {
        static EventOption O(string label, string reply, int rep, int xp, int pts = 0, bool good = false, string item = null, bool flash = false)
        {
            return new EventOption { Label = label, Reply = reply, Rep = rep, Xp = xp, Pts = pts, Good = good, Item = item, Flash = flash };
        }

        static DialogEvent E(string id, string intro, params EventOption[] opts) { return new DialogEvent { Id = id, Intro = intro, Options = opts }; }

        public static readonly List<DialogEvent> All = new List<DialogEvent>
        {
            E("senha", "Psiu, {nome}! Esqueci minha senha e o servidor de deploy está bloqueado. Me empresta a sua rapidinho? Ninguém vai saber...",
                O("Não compartilho senha. Te levo ao suporte para resetar a sua.", "Poxa, você tem razão. Senha é pessoal e intransferível. Valeu pela ajuda!", 10, 15, 0, true),
                O("Tá bom, anota aí...", "...Dias depois o Vítor, da segurança, descobriu acessos estranhos com a SUA senha. Advertência registrada.", -18, 0),
                O("Agora não posso.", "Ok... vou me virar.", 0, 0)),
            E("credito", "{nome}, na reunião de ontem o Léo apresentou aquela solução que VOCÊ criou como se fosse dele. O que você vai fazer?",
                O("Converso com ele em particular e combino de dar os créditos certos.", "Ele pediu desculpas e corrigiu no canal do time. Atitude madura!", 6, 10, 0, true),
                O("Exponho ele na frente de todo mundo, bem alto.", "Você tinha razão, mas o barraco pegou mal para os dois.", -6, 0),
                O("Deixo pra lá.", "Tudo bem, mas lembre-se: dar visibilidade ao seu trabalho também é importante.", 0, 2)),
            E("sexta", "São 17h55 de sexta e você achou um bug em produção: o checkout falha às vezes. Ninguém mais percebeu...",
                O("Aviso o time agora e ajudo a corrigir com revisão.", "Corrigido com segurança em 30 minutos. O plantão te agradece!", 10, 20, 40, true),
                O("Faço um hotfix sozinho direto em produção, sem revisão.", "O hotfix derrubou o login por 20 minutos. Deploy sem revisão em plena sexta...", -12, 5),
                O("Finjo que não vi. Segunda eu vejo.", "No sábado o bug piorou e descobriram que você sabia. Pegou mal.", -10, 0)),
            E("novato", "Oi... sou novo(a) aqui e não sei nem por onde começar. Você pode me ajudar um pouquinho?",
                O("Claro! Te mostro o escritório e a Biblioteca.", "Muito obrigado(a)! Toma, um pão de queijo de agradecimento.", 8, 10, 0, true, "pao_queijo"),
                O("Estou ocupado(a), pergunta para outra pessoa.", "Ah... tudo bem.", -3, 0)),
            E("cpf", "{nome}, olha isso: os logs da API estão gravando CPF e cartão dos clientes em texto puro!",
                O("Reporto imediatamente para a Segurança (LGPD!).", "O Vítor corrigiu na hora e agradeceu publicamente. Isso é responsabilidade!", 12, 20, 30, true),
                O("Tiro um print para mostrar para os amigos.", "Vazamento de dados é crime. Você foi chamado(a) no RH. Reputação queimada.", -25, 0),
                O("Ignoro, não é problema meu.", "Dados pessoais são problema de todos. Isso podia virar multa milionária.", -6, 0)),
            E("pirata", "Ei, tenho uma licença 'alternativa' daquela IDE caríssima. Quer? É de graça, hehe.",
                O("Não, obrigado(a). Uso a versão gratuita ou peço licença à empresa.", "Boa! Software pirata é ilegal e pode trazer vírus.", 5, 5, 0, true),
                O("Manda aí!", "O antivírus da empresa detectou um malware no instalador. Ops.", -15, 0)),
            E("fofoca", "Você soube? Dizem que a chefe do Squad React vai ser demitida... conta aí o que você acha dela!",
                O("Mudo de assunto educadamente.", "Ok, ok... você é discreto(a).", 2, 2, 0, true),
                O("Entro na fofoca.", "A fofoca era falsa e chegou aos ouvidos dela. Climão.", -8, 0),
                O("Defendo: 'Ela é ótima, não espalha isso'.", "Ela soube depois e ficou agradecida.", 5, 3, 0, true)),
            E("review", "Pode revisar meu Pull Request? Preciso aprovar antes do almoço!",
                O("Reviso com calma e aponto os problemas com gentileza.", "Você achou um bug sério antes de ir para produção. Excelente review!", 6, 15, 0, true),
                O("Aprovo sem ler, ela está com pressa.", "O código tinha uma falha de segurança. Aprovar sem ler também é responsabilidade sua.", -6, 0),
                O("Zoo o código no canal público do time.", "Críticas são no código, não na pessoa — e em particular. Pegou mal.", -12, 0)),
            E("mentor", "Quer uma dica de quem está há 15 anos na área? Leia a documentação oficial antes de pedir ajuda: 80% das respostas estão lá.",
                O("Valeu pela dica!", "De nada. A Biblioteca do escritório tem tudo organizado por tecnologia.", 1, 8)),
            E("hackathon", "Vai ter um hackathon interno no sábado. Topa participar com o meu time?",
                O("Topo! Bora aprender.", "Vocês ficaram em 2º lugar! Muito aprendizado e networking.", 4, 30, 50, true),
                O("Dessa vez não, preciso descansar.", "Tudo certo, descanso também é importante!", 0, 0)),
            E("cliente", "Socorro! Um cliente grande ligou reclamando de um problema AGORA. Você pode olhar?",
                O("Posso! Me passa o problema.", "Valeu! É um desafio relâmpago — corra para a área certa!", 3, 0, 0, true, null, true),
                O("Agora não consigo.", "Tudo bem, vou procurar outra pessoa.", 0, 0)),
            E("plagio", "Me manda sua resposta do teste de certificação? Eu só copio, juro que ninguém percebe.",
                O("Não envio, mas te explico o conceito para você fazer sozinho(a).", "Entendi de verdade agora! Obrigado(a).", 8, 10, 0, true),
                O("Mando sim.", "O teste detectou respostas idênticas. Os dois foram desclassificados.", -10, 0)),
            E("reuniao", "Te chamaram para uma reunião às 17h58 sem pauta nenhuma...",
                O("Pergunto educadamente qual é a pauta.", "Mudaram para segunda, com pauta. Produtivo!", 2, 3, 0, true),
                O("Simplesmente não apareço.", "Ficaram esperando você. Avisar é o mínimo.", -5, 0),
                O("Vou mesmo assim.", "Foi rápida, pelo menos.", 0, 2)),
            E("licenca", "Achei um código pronto no GitHub com licença GPL. Copio inteiro para o produto fechado da Micorosoft?",
                O("Antes, consulto o jurídico sobre a licença.", "Ótimo: a GPL exigiria abrir o código do produto. Evitamos um problemão!", 8, 10, 0, true),
                O("Copio, ninguém vai ver.", "Uma auditoria encontrou o código copiado. Retrabalho e advertência.", -10, 0)),
            E("acess", "A Nina, de acessibilidade, pediu para colocar texto alternativo em todas as imagens do site. Vai dar trabalho...",
                O("Faço! Acessibilidade é para todos.", "Leitores de tela agora descrevem as imagens. Usuários agradeceram!", 5, 12, 0, true),
                O("Deixo para depois (nunca).", "Um cliente com deficiência visual reclamou publicamente. Pegou mal.", -4, 0)),
            E("phishing", "Recebi um e-mail: 'Clique aqui e ganhe um Xbox grátis da Micorosoft!'. Clico?",
                O("Não! Reporto como phishing para a Segurança.", "Era um ataque mirando a empresa toda. Você ajudou a bloquear!", 6, 10, 20, true),
                O("Clico, vai que é verdade.", "Seu computador foi isolado por suspeita de malware. Uma tarde perdida.", -12, 0)),
            E("cafe", "Opa, derramei café no teclado do servidor do Data Center! E agora?!",
                O("Desligo com segurança, ajudo a limpar e chamo o suporte.", "Salvou o equipamento! Vou te dever um café.", 4, 5, 0, true, "cafe_expresso"),
                O("Dou risada e saio de fininho.", "Ninguém gostou da atitude.", -4, 0)),
            E("ceo", "Ouvi falar muito bem de você. Continue assim, {nome}: aqui crescem as pessoas que aprendem rápido e são éticas.",
                O("Obrigado(a)! Vou continuar me esforçando.", "É assim que se faz.", 3, 5)),
        };
    }
}
