using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DockerQuest
{
    class ShopItem
    {
        public string Id, Name, Slot, Desc;
        public int Price;
    }

    class Achievement
    {
        public string Id, Name, Desc;
        public int Reward, Goal;
        public Func<GameState, World, int> Progress;
    }

    // Catálogo de aparência, itens da lojinha e objetivos (conquistas).
    static class Catalog
    {
        public static readonly int[] Skins = { Story.Sk1, Story.Sk2, Story.Sk3, Story.Sk4, Story.Sk5, Story.Sk6 };
        public static readonly string[] HairStyles = { "Curto", "Longo", "Coque", "Raspado", "Black power", "Rabo de cavalo", "Tranças", "Chanel" };
        public static readonly int[] HairStyleIds = { 0, 1, 2, 3, 4, 6, 7, 8 };   // 5 = boné (só dos colegas)
        public static readonly int[] HairColors = { Story.HBlack, Story.HBrown, Story.HBlond, Story.HRed, Story.HGray, Story.HBlue, Story.HPink, 0x2E8B57 };
        public static readonly int[] Shirts = { 0x2496ED, 0xE63946, 0x2A9D8F, 0xF4A261, 0x8E6CC4, 0x264653, 0xFFB703, 0xEF476F, 0x3FB950, 0xF1F5F9 };
        public static readonly int[] Pants = { 0x2B2D42, 0x1D3557, 0x5A3A22, 0x6C757D, 0x264653, 0x111827 };

        public static readonly string[] Slots = { "hat", "acc", "outfit", "pet", "use", "util", "sala" };
        public static readonly string[] SlotNames = { "Chapéus", "Acessórios", "Roupas", "Mascotes", "Consumíveis", "Utilidades", "Minha sala" };

        // ordem da barra de atalhos (teclas 1–8)
        public static readonly string[] Hotbar = { "cafe_expresso", "pao_queijo", "energetico", "pizza", "borracha", "teleporte", "cha_verde", "bolo" };

        public static readonly ShopItem[] Items = new ShopItem[]
        {
            new ShopItem { Id = "bone", Slot = "hat", Price = 150, Name = "Boné Docker", Desc = "Boné azul com a baleia do Docker. Clássico!" },
            new ShopItem { Id = "gorro", Slot = "hat", Price = 120, Name = "Gorro de lã", Desc = "Para o ar-condicionado gelado do Data Center." },
            new ShopItem { Id = "capacete", Slot = "hat", Price = 200, Name = "Capacete de obra", Desc = "Para quem vive construindo imagens (docker build)." },
            new ShopItem { Id = "cowboy", Slot = "hat", Price = 250, Name = "Chapéu de cowboy", Desc = "Laçando containers desgarrados desde 2013." },
            new ShopItem { Id = "coroa", Slot = "hat", Price = 800, Name = "Coroa de ouro", Desc = "Reservada à realeza dos containers." },

            new ShopItem { Id = "oculos_redondos", Slot = "acc", Price = 100, Name = "Óculos redondos", Desc = "Visual de quem lê a documentação inteira." },
            new ShopItem { Id = "oculos_escuros", Slot = "acc", Price = 150, Name = "Óculos escuros", Desc = "Deploy na sexta-feira? Sem medo." },
            new ShopItem { Id = "gravata", Slot = "acc", Price = 120, Name = "Gravata-borboleta", Desc = "Elegância para as reuniões com a diretoria." },
            new ShopItem { Id = "headset", Slot = "acc", Price = 180, Name = "Headset", Desc = "Igual ao do pessoal do plantão no NOC." },
            new ShopItem { Id = "cracha", Slot = "acc", Price = 300, Name = "Crachá dourado", Desc = "Mostra para todo mundo que você é VIP." },

            new ShopItem { Id = "moletom", Slot = "outfit", Price = 200, Name = "Moletom Docker", Desc = "Moletom azul com capuz e a baleia no peito." },
            new ShopItem { Id = "jaleco", Slot = "outfit", Price = 250, Name = "Jaleco de laboratório", Desc = "Perfeito para os experimentos do Laboratório de Containers." },
            new ShopItem { Id = "havaiana", Slot = "outfit", Price = 300, Name = "Camisa havaiana", Desc = "Sexta-feira casual o ano inteiro." },
            new ShopItem { Id = "terno", Slot = "outfit", Price = 400, Name = "Terno executivo", Desc = "Com gravata vermelha. Pronto(a) para virar CTO." },

            new ShopItem { Id = "pato", Slot = "pet", Price = 250, Name = "Patinho de borracha", Desc = "O melhor parceiro de debug: explique o problema para ele!" },
            new ShopItem { Id = "gato", Slot = "pet", Price = 350, Name = "Gatinho", Desc = "Um gatinho laranja que te segue pelo escritório." },
            new ShopItem { Id = "robo", Slot = "pet", Price = 450, Name = "Robozinho", Desc = "Um pequeno robô flutuante com antena piscante." },
            new ShopItem { Id = "moby", Slot = "pet", Price = 600, Name = "Moby, a baleiazinha", Desc = "O mascote oficial do Docker, carregando containers nas costas." },

            new ShopItem { Id = "chef", Slot = "hat", Price = 180, Name = "Chapéu de chef", Desc = "Para quem cozinha Dockerfiles caprichados." },
            new ShopItem { Id = "mochila", Slot = "acc", Price = 160, Name = "Mochila", Desc = "Leva o notebook, o carregador e três cafés de reserva." },
            new ShopItem { Id = "polo_noc", Slot = "outfit", Price = 220, Name = "Polo do NOC", Desc = "Polo preta oficial do pessoal de plantão." },
            new ShopItem { Id = "cachorro", Slot = "pet", Price = 400, Name = "Cachorrinho", Desc = "Um cachorro caramelo, o melhor amigo do DevOps." },

            new ShopItem { Id = "bone_react", Slot = "hat", Price = 170, Name = "Boné React", Desc = "Boné ciano com o átomo do React. Re-renderiza o seu estilo." },
            new ShopItem { Id = "gorro_python", Slot = "hat", Price = 140, Name = "Gorro Python", Desc = "Azul e amarelo, nas cores da cobrinha mais famosa da programação." },
            new ShopItem { Id = "boina", Slot = "hat", Price = 160, Name = "Boina de designer", Desc = "Obrigatória para discutir paleta de cores no Estúdio Frontend." },
            new ShopItem { Id = "camiseta_python", Slot = "outfit", Price = 180, Name = "Camiseta Python", Desc = "import estilo. Azul com o logo amarelo." },
            new ShopItem { Id = "camiseta_php", Slot = "outfit", Price = 180, Name = "Camiseta PHP", Desc = "Roxinha, com o elePHPant no peito. Roda em 77% da web!" },
            new ShopItem { Id = "jaqueta", Slot = "outfit", Price = 350, Name = "Jaqueta de couro", Desc = "Visual de quem resolve incidente às 3 da manhã sem suar." },
            new ShopItem { Id = "moletom_mic", Slot = "outfit", Price = 260, Name = "Moletom Micorosoft", Desc = "Cinza, com capuz e os quatro quadradinhos da empresa." },
            new ShopItem { Id = "pinguim", Slot = "pet", Price = 380, Name = "Pinguim", Desc = "Um pinguinzinho que roda Linux. Bamboleia atrás de você." },
            new ShopItem { Id = "cobra", Slot = "pet", Price = 330, Name = "Cobrinha Python", Desc = "Inofensiva e indentada: anda sempre alinhada com 4 espaços." },
            new ShopItem { Id = "elefante", Slot = "pet", Price = 420, Name = "elePHPant", Desc = "O elefantinho azul do PHP. Nunca esquece um $." },
            new ShopItem { Id = "cafe_expresso", Slot = "use", Price = 15, Name = "Café expresso", Desc = "Tecla 1. +40% de velocidade por 90 segundos." },
            new ShopItem { Id = "pao_queijo", Slot = "use", Price = 25, Name = "Pão de queijo", Desc = "Tecla 2. O seu próximo acerto vale +15 pontos." },
            new ShopItem { Id = "energetico", Slot = "use", Price = 40, Name = "Energético", Desc = "Tecla 3. +45 segundos no próximo desafio relâmpago (ou no atual)." },
            new ShopItem { Id = "pizza", Slot = "use", Price = 80, Name = "Pizza para o time", Desc = "Tecla 4. O time comemora e o próximo desafio relâmpago vale +50 pontos." },
            new ShopItem { Id = "borracha", Slot = "use", Price = 35, Name = "Borracha mágica", Desc = "Tecla 5. Apaga o seu próximo erro no terminal (não perde pontos)." },
            new ShopItem { Id = "teleporte", Slot = "use", Price = 50, Name = "Crachá de teletransporte", Desc = "Tecla 6. Leva você direto até o seu objetivo atual." },
            new ShopItem { Id = "cha_verde", Slot = "use", Price = 30, Name = "Chá verde", Desc = "Tecla 7. Você divide um chá com o time: +4 de reputação." },
            new ShopItem { Id = "bolo", Slot = "use", Price = 90, Name = "Bolo para o time", Desc = "Tecla 8. Festa no escritório! +8 de reputação e todo mundo comemora." },
            new ShopItem { Id = "vale", Slot = "util", Price = 20, Name = "Vale-dica", Desc = "Troque com um colega por uma dica SEM perder os 25 pontos do desafio. Acumulável." },
            new ShopItem { Id = "tenis", Slot = "util", Price = 300, Name = "Tênis de corrida", Desc = "Ande 30% mais rápido pelo escritório." },
            new ShopItem { Id = "mapa", Slot = "util", Price = 80, Name = "Minimapa ampliado", Desc = "Deixa o minimapa do canto da tela bem maior." },
            new ShopItem { Id = "patinete", Slot = "util", Price = 500, Name = "Patinete elétrico", Desc = "Suba no patinete e ande 50% mais rápido (melhor que o tênis). Na Lojinha ou com a tecla P você sobe e desce dele." },
            new ShopItem { Id = "fone_ruido", Slot = "util", Price = 250, Name = "Fone com cancelamento de ruído", Desc = "Mais foco: todo desafio relâmpago dura +30 segundos." },
            new ShopItem { Id = "cafeteira", Slot = "util", Price = 300, Name = "Cafeteira pessoal", Desc = "O café da máquina do corredor também te deixa 40% mais rápido por 45 s." },
            new ShopItem { Id = "teclado", Slot = "util", Price = 450, Name = "Teclado mecânico", Desc = "Clack-clack! Você rende mais: +10% de XP em todos os desafios." },
            new ShopItem { Id = "monitor", Slot = "util", Price = 350, Name = "Monitor ultrawide", Desc = "Enxerga o problema inteiro: desafios relâmpago duram +20 segundos." },
            new ShopItem { Id = "livro_etica", Slot = "util", Price = 200, Name = "Livro \"Ética na TI\"", Desc = "Boas escolhas rendem 50% mais reputação nas situações do dia a dia." },
        }.Concat(PlayerRoom.ShopItems()).ToArray();

        public static ShopItem Item(string id) { return Items.FirstOrDefault(i => i.Id == id); }

        public static Look PlayerLook(Progress p) { return MakeLook(p.Visual, p.Equip, null); }

        public static Look MakeLook(int[] v, Dictionary<string, string> equip, ShopItem preview)
        {
            Func<string, string> eq = s => { string id; return equip.TryGetValue(s, out id) ? id : null; };
            var l = new Look
            {
                Skin = Art.Hx(Skins[v[0] % Skins.Length]), HairStyle = HairStyleIds[v[1] % HairStyleIds.Length],
                Female = v.Length > 5 && v[5] == 1, Bottom = v.Length > 6 ? v[6] : 0, Hair = Art.Hx(HairColors[v[2] % HairColors.Length]),
                Shirt = Art.Hx(Shirts[v[3] % Shirts.Length]), Pants = Art.Hx(Pants[v[4] % Pants.Length]), Badge = true,
                Hat = eq("hat"), Outfit = eq("outfit"), Pet = eq("pet"), Acc = eq("acc"), Ride = eq("ride")
            };
            l.Accent = l.Shirt;
            if (preview != null)
            {
                if (preview.Slot == "hat") l.Hat = preview.Id;
                if (preview.Slot == "acc") l.Acc = preview.Id;
                if (preview.Slot == "outfit") l.Outfit = preview.Id;
                if (preview.Slot == "pet") l.Pet = preview.Id;
                if (preview.Id == "patinete") l.Ride = "patinete";
            }
            l.Glasses = l.Acc == "oculos_redondos";
            l.Headset = l.Acc == "headset";
            return l;
        }

        static int Boards(World w) { return w.Items.Count(i => i.Kind == "board"); }
        public static string BoardKey(Item it) { return it.Title + "@" + it.X + "," + it.Y; }

        public static readonly List<Achievement> Achievements = new List<Achievement>
        {
new Achievement { Id = "primeiro", Name = "Primeiro desafio", Desc = "Conclua seu primeiro desafio.", Reward = 20, Goal = 1, Progress = (gs, w) => gs.Prog.Completed },
            new Achievement { Id = "sem_rodinhas", Name = "Sem rodinhas", Desc = "Conclua 5 desafios sem usar nenhuma dica.", Reward = 50, Goal = 5, Progress = (gs, w) => gs.Prog.NoHint.Count },
            new Achievement { Id = "perfeicao", Name = "Perfeccionista", Desc = "Tire 100 pontos em 10 desafios (sem erros e sem dicas).", Reward = 100, Goal = 10, Progress = (gs, w) => gs.Prog.Points.Count(x => x >= 100) },
            new Achievement { Id = "persistente", Name = "Persistente", Desc = "Acerte um desafio depois de errar 3 vezes ou mais.", Reward = 20, Goal = 1, Progress = (gs, w) => gs.Prog.Persist },
            new Achievement { Id = "setor_impecavel", Name = "Nível impecável", Desc = "Tire 100 pontos em todos os desafios de um mesmo nível de uma área.", Reward = 150, Goal = 1,
                Progress = (gs, w) => Challenges.Cats.Any(ct => Enumerable.Range(0, 3).Any(t => gs.TierLevels(ct.Id, t).All(i => gs.Prog.Points[i] >= 100))) ? 1 : 0 },
            new Achievement { Id = "primeira_badge", Name = "Primeira badge", Desc = "Conquiste sua primeira badge.", Reward = 30, Goal = 1, Progress = (gs, w) => gs.BadgeCount },
            new Achievement { Id = "multiarea", Name = "Multitarefa", Desc = "Conquiste badges de 4 áreas diferentes.", Reward = 120, Goal = 4,
                Progress = (gs, w) => Challenges.Cats.Count(ct => gs.Prog.BadgeDates.ContainsKey(ct.Id + ":0")) },
            new Achievement { Id = "ouro", Name = "Padrão ouro", Desc = "Conquiste uma badge de nível Profissional.", Reward = 150, Goal = 1,
                Progress = (gs, w) => Challenges.Cats.Count(ct => gs.Prog.BadgeDates.ContainsKey(ct.Id + ":2")) },
            new Achievement { Id = "colecao_badges", Name = "Parede de badges", Desc = "Conquiste 12 badges.", Reward = 200, Goal = 12, Progress = (gs, w) => gs.BadgeCount },
            new Achievement { Id = "metade", Name = "Meio caminho", Desc = "Conclua 50 desafios.", Reward = 100, Goal = 50, Progress = (gs, w) => gs.Prog.Completed },
            new Achievement { Id = "jornada", Name = "Lenda da Micorosoft", Desc = "Conclua todos os desafios de todas as áreas.", Reward = 500, Goal = 0, Progress = (gs, w) => gs.Prog.Completed },
            new Achievement { Id = "promovido", Name = "Promovido(a)!", Desc = "Saia do estágio: chegue a Júnior.", Reward = 40, Goal = 1, Progress = (gs, w) => gs.Prog.LastSeniority },
            new Achievement { Id = "pleno", Name = "Pleno(a)", Desc = "Chegue ao cargo de Pleno.", Reward = 120, Goal = 2, Progress = (gs, w) => gs.Prog.LastSeniority },
            new Achievement { Id = "etico", Name = "Ética em dia", Desc = "Faça 5 escolhas éticas nas situações do dia a dia.", Reward = 60, Goal = 5, Progress = (gs, w) => gs.Prog.GoodChoices },
            new Achievement { Id = "querido", Name = "Querido(a) da firma", Desc = "Alcance 85 de reputação.", Reward = 80, Goal = 85, Progress = (gs, w) => gs.Prog.Reputation },
            new Achievement { Id = "ouvinte", Name = "Ouvinte assíduo(a)", Desc = "Conclua 3 aulas no telão do Auditório.", Reward = 60, Goal = 3, Progress = (gs, w) => Lessons.DoneCount(gs.Prog) },
            new Achievement { Id = "todas_aulas", Name = "Formatura no Auditório", Desc = "Conclua todas as aulas do Auditório.", Reward = 150, Goal = Lessons.All.Length, Progress = (gs, w) => Lessons.DoneCount(gs.Prog) },
            new Achievement { Id = "phishing", Name = "Caçador(a) de phishing", Desc = "Desmascare 5 e-mails no SOC de Segurança.", Reward = 60, Goal = 5, Progress = (gs, w) => gs.Prog.Stat("phish") },
            new Achievement { Id = "suporte", Name = "Herói do suporte", Desc = "Resolva 5 chamados no Help Desk.", Reward = 50, Goal = 5, Progress = (gs, w) => gs.Prog.Stat("tickets") },
            new Achievement { Id = "plateia", Name = "Plateia nota 10", Desc = "Acerte 5 perguntas do Tech Talk no Auditório.", Reward = 50, Goal = 5, Progress = (gs, w) => gs.Prog.Stat("quiz") },
            new Achievement { Id = "cacabug", Name = "Caça-bugs", Desc = "Encontre 5 bugs com a Bel na Sala de Jogos.", Reward = 50, Goal = 5, Progress = (gs, w) => gs.Prog.Stat("bugs") },
            new Achievement { Id = "ia_etica", Name = "IA com responsabilidade", Desc = "Tome 3 decisões responsáveis no Laboratório de Inovação.", Reward = 60, Goal = 3, Progress = (gs, w) => gs.Prog.Stat("ai_good") },
            new Achievement { Id = "jardineiro", Name = "Mão verde", Desc = "Complete a missão do jardim do Seu Raimundo.", Reward = 30, Goal = 1, Progress = (gs, w) => gs.Prog.Stat("garden_done") },
            new Achievement { Id = "networking", Name = "Networking", Desc = "Converse sobre carreira com 3 colegas (Refeitório e Auditório).", Reward = 30, Goal = 3, Progress = (gs, w) => gs.Prog.Stat("network") },
            new Achievement { Id = "invasores", Name = "Defensor do servidor", Desc = "Faça 1.000 pontos no fliperama Invasores de Bugs da Sala de Jogos.", Reward = 40, Goal = 1000, Progress = (gs, w) => gs.Prog.Stat("invaders_best") },
            new Achievement { Id = "explorador", Name = "Explorador(a)", Desc = "Visite todas as " + Story.RoomCount + " salas do campus da Micorosoft.", Reward = 60, Goal = Story.RoomCount, Progress = (gs, w) => gs.Prog.Visited.Count },
            new Achievement { Id = "sociavel", Name = "Sociável", Desc = "Converse com todos os funcionários da Micorosoft.", Reward = 60, Goal = 0,
                Progress = (gs, w) => w.Npcs.Count(n => gs.Prog.Talked.Contains(n.Name)) },
            new Achievement { Id = "leitor", Name = "Rato de documentação", Desc = "Leia todos os quadros, pôsteres e painéis.", Reward = 40, Goal = 0,
                Progress = (gs, w) => w.Items.Count(i => i.Kind == "board" && gs.Prog.Read.Contains(BoardKey(i))) },
            new Achievement { Id = "pesquisador", Name = "Pesquisador(a)", Desc = "Consulte 10 páginas da documentação na Biblioteca.", Reward = 40, Goal = 10, Progress = (gs, w) => gs.Prog.DocsRead.Count },
            new Achievement { Id = "cafe", Name = "Movido(a) a café", Desc = "Tome 5 cafés na máquina do corredor.", Reward = 25, Goal = 5, Progress = (gs, w) => gs.Prog.Coffees },
            new Achievement { Id = "agua", Name = "Hidratado(a)", Desc = "Beba água no bebedouro 3 vezes.", Reward = 15, Goal = 3, Progress = (gs, w) => gs.Prog.Waters },
            new Achievement { Id = "maratona", Name = "Maratonista", Desc = "Ande 1.500 metros pelo escritório.", Reward = 30, Goal = 1500, Progress = (gs, w) => (int)gs.Prog.Walked },
            new Achievement { Id = "bombeiro", Name = "Bombeiro(a) de plantão", Desc = "Resolva um desafio relâmpago.", Reward = 30, Goal = 1, Progress = (gs, w) => gs.Prog.Incidents },
            new Achievement { Id = "apaga_incendio", Name = "Apaga-incêndio", Desc = "Resolva 5 desafios relâmpago.", Reward = 100, Goal = 5, Progress = (gs, w) => gs.Prog.Incidents },
            new Achievement { Id = "relampago", Name = "Relâmpago", Desc = "Resolva um desafio relâmpago com mais de 2/3 do tempo sobrando.", Reward = 60, Goal = 1, Progress = (gs, w) => gs.Prog.FastIncidents },
            new Achievement { Id = "gourmet", Name = "Gourmet", Desc = "Use 5 consumíveis da lojinha.", Reward = 25, Goal = 5, Progress = (gs, w) => gs.Prog.Consumed },
            new Achievement { Id = "cliente", Name = "Cliente da lojinha", Desc = "Compre seu primeiro item na lojinha.", Reward = 10, Goal = 1, Progress = (gs, w) => gs.Prog.Purchases },
            new Achievement { Id = "estiloso", Name = "Estiloso(a)", Desc = "Use chapéu, acessório e mascote ao mesmo tempo.", Reward = 30, Goal = 3,
                Progress = (gs, w) => new[] { "hat", "acc", "pet" }.Count(s => gs.Prog.Equipped(s) != null) },
            new Achievement { Id = "colecionador", Name = "Colecionador(a)", Desc = "Tenha 8 itens visuais da lojinha.", Reward = 80, Goal = 8,
                Progress = (gs, w) => gs.Prog.Owned.Count(id => { var it = Item(id); return it != null && it.Slot != "util"; }) },
        };

        // Metas que dependem do mapa (quantidade de funcionários e quadros).
        static int totalChallenges;
        public static int GoalOf(Achievement a, World w)
        {
            if (a.Id == "sociavel") return w.Npcs.Count;
            if (a.Id == "leitor") return Boards(w);
            if (a.Id == "jornada") { if (totalChallenges == 0) totalChallenges = Challenges.All().Count; return totalChallenges; }
            return a.Goal;
        }
    }
}
