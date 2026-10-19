using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace DockerQuest
{
    // Aulas do Auditório: slides sobre um tema específico, dados por alguém da empresa, com quiz no final.
    class Slide
    {
        public string Title, Code, Speech;
        public string[] Bullets;
    }

    class Lesson
    {
        public string Id, Title, Teacher, Role, Summary;
        public Color Color;
        public Slide[] Slides;
        public Quiz[] Quiz;
    }

    static class Lessons
    {
        public const int BasePts = 40, PtsPerHit = 20, BaseXp = 10, XpPerHit = 6;

        static Slide S(string title, string speech, string code, params string[] bullets) { return new Slide { Title = title, Speech = speech, Code = code, Bullets = bullets }; }
        static Quiz Q(string prompt, string explain, params string[] options) { return new Quiz(prompt, explain, options); }

        public static readonly Lesson[] All =
        {
            new Lesson
            {
                Id = "git", Title = "Git e GitHub do zero", Teacher = "Rafael", Role = "Líder de DevOps", Color = Art.Hx(0xF05032),
                Summary = "Commits, branches, conflitos e Pull Requests: como times de verdade trabalham no mesmo código.",
                Slides = new[]
                {
                    S("Por que controlar versões?", "Quem nunca salvou um 'trabalho_final_AGORA_VAI_v3.zip'? O Git resolve isso.", null,
                      "Sem Git: cópias de pastas, e-mails com anexos e ninguém sabe qual é a versão certa.",
                      "Com Git: cada mudança vira um commit, com autor, data e uma mensagem explicando o porquê.",
                      "Dá para voltar no tempo, comparar versões e trabalhar em equipe sem apagar o trabalho dos outros."),
                    S("Os três lugares do Git", "Pense numa foto: você arruma a cena (add) e depois tira a foto (commit).",
                      "git status\ngit add index.html\ngit commit -m \"Adiciona página inicial\"",
                      "Diretório de trabalho: onde você edita os arquivos.",
                      "Área de preparo (staging): o que vai entrar no próximo commit — git add.",
                      "Repositório: o histórico de commits — git commit."),
                    S("Commits que ajudam o time", "Um commit bom conta uma história curta e clara.", null,
                      "Pequenos e com um objetivo só: fica fácil revisar e desfazer.",
                      "Mensagem no imperativo e específica: 'Corrige cálculo do frete', não 'mudanças'.",
                      "NUNCA commite senhas ou chaves: use .gitignore e variáveis de ambiente."),
                    S("Branches: linhas paralelas", "Cada tarefa ganha a sua branch. A main fica sempre funcionando.",
                      "git switch -c feature/carrinho\n# ...edita e faz commits...\ngit switch main\ngit merge feature/carrinho",
                      "Uma branch é uma linha de desenvolvimento independente.",
                      "main (ou master) = o que está em produção.",
                      "Terminou a tarefa? Junte a branch na main (merge)."),
                    S("Trabalhando com o GitHub", "O GitHub guarda uma cópia do repositório na nuvem para o time todo.",
                      "git clone https://github.com/micorosoft/cafe365.git\ngit pull\ngit push",
                      "git clone: copia o repositório remoto para a sua máquina.",
                      "git pull: traz as novidades (fetch + merge). git push: envia os seus commits.",
                      "Conflito = duas pessoas mudaram a mesma linha. O Git marca com <<<<<<< e você decide o que fica."),
                    S("Pull Request e revisão", "Na Micorosoft nada vai para a main sem revisão. Nem o meu código!", null,
                      "Pull Request (PR): pedido para juntar a sua branch na main.",
                      "Colegas revisam, comentam e aprovam; testes automáticos (CI) rodam antes do merge.",
                      "Revisão é sobre o código, nunca sobre a pessoa — e seja gentil nos comentários."),
                },
                Quiz = new[]
                {
                    Q("Qual comando coloca uma alteração na área de preparo (staging)?", "git add prepara; git commit grava; git push envia; git status só mostra a situação.",
                      "git add", "git commit", "git push", "git status"),
                    Q("Você e uma colega mudaram a MESMA linha do mesmo arquivo em branches diferentes. Ao juntar, o que acontece?", "É um conflito: o Git não adivinha, alguém precisa escolher o conteúdo certo e fazer o commit.",
                      "Um conflito, que alguém precisa resolver escolhendo o conteúdo certo", "O Git apaga as duas versões", "Vale sempre a mudança mais recente", "O GitHub escolhe sozinho a melhor versão"),
                    Q("Onde NÃO se deve guardar a senha do banco de dados?", "Tudo que é commitado fica no histórico (mesmo se apagar depois). Segredos vão em variáveis de ambiente ou cofres.",
                      "Num arquivo commitado no repositório", "Numa variável de ambiente do servidor", "Num cofre de segredos", "No gerenciador de segredos da nuvem"),
                }
            },
            new Lesson
            {
                Id = "internet", Title = "Como a internet funciona", Teacher = "Sandra", Role = "Líder de Infra e Redes", Color = Art.Hx(0x2A9D8F),
                Summary = "O que acontece entre o clique e a página aparecer: IP, DNS, portas, HTTP e HTTPS.",
                Slides = new[]
                {
                    S("O caminho de um clique", "Você digita cafe365.com e aperta Enter. Muita coisa acontece em menos de um segundo!", null,
                      "1) DNS descobre o endereço IP do site.",
                      "2) TCP abre uma conexão com o servidor.  3) TLS cria um canal criptografado.",
                      "4) HTTP pede a página, e o navegador desenha o que recebeu."),
                    S("Endereços IP", "Todo dispositivo conectado tem um endereço, como uma casa tem CEP e número.", null,
                      "IPv4: quatro números de 0 a 255 (ex.: 192.168.0.15). IPv6 é bem maior.",
                      "IPs privados (192.168.x.x, 10.x.x.x, 172.16–31.x.x) só valem dentro da rede local.",
                      "O roteador faz NAT: vários computadores saem para a internet com um IP público só."),
                    S("DNS: a agenda da internet", "Ninguém decora IP. O DNS traduz nomes em números.",
                      "nslookup cafe365.com\ndig cafe365.com A +short",
                      "Registro A: nome → IPv4. AAAA: nome → IPv6.",
                      "CNAME: apelido para outro nome. MX: servidor de e-mail do domínio.",
                      "Se o site abre pelo IP mas não pelo nome, o problema é DNS."),
                    S("Portas e protocolos", "O IP acha o computador; a porta acha o programa dentro dele.", null,
                      "80 HTTP · 443 HTTPS · 22 SSH · 53 DNS · 3306 MySQL · 5432 PostgreSQL.",
                      "TCP: garante que tudo chega, em ordem (páginas, e-mails, arquivos).",
                      "UDP: mais rápido e sem garantia (chamadas de vídeo, jogos, DNS)."),
                    S("HTTP por dentro", "HTTP é uma conversa de texto: pedido e resposta.",
                      "GET /cardapio HTTP/1.1\nHost: cafe365.com\n\nHTTP/1.1 200 OK\nContent-Type: application/json",
                      "Pedido: método (GET, POST, PUT, DELETE), caminho, cabeçalhos e corpo.",
                      "Resposta: código de status (200 OK, 301 mudou, 404 não achou, 500 erro no servidor).",
                      "Cabeçalhos carregam informações extras: tipo do conteúdo, cookies, cache..."),
                    S("HTTPS e o cadeado", "O cadeado protege a conversa — mas não garante que o site é honesto.", null,
                      "TLS criptografa tudo entre você e o servidor: ninguém no caminho consegue ler.",
                      "O certificado prova que o servidor é mesmo dono daquele domínio.",
                      "Golpistas também têm certificado! Cadeado ≠ site confiável: confira o endereço."),
                },
                Quiz = new[]
                {
                    Q("O que o DNS faz?", "DNS = Domain Name System: traduz nomes de domínio em endereços IP.",
                      "Traduz nomes (como cafe365.com) em endereços IP", "Criptografa a conexão", "Guarda as páginas do site", "Escolhe a rota mais rápida até o servidor"),
                    Q("Qual porta o HTTPS usa por padrão?", "443 é HTTPS; 80 é HTTP sem criptografia; 22 é SSH; 3306 é MySQL.",
                      "443", "80", "22", "3306"),
                    Q("Um site com cadeado (HTTPS) é sempre confiável?", "O cadeado só garante a criptografia da conexão. Sites de golpe também usam HTTPS.",
                      "Não: o cadeado só garante que a conexão é criptografada", "Sim, o cadeado garante que não é golpe", "Sim, desde que o site seja bonito", "Só se o domínio terminar em .com.br"),
                }
            },
            new Lesson
            {
                Id = "seguranca", Title = "Segurança no dia a dia", Teacher = "Vítor", Role = "CISO · Segurança", Color = Art.Hx(0xD62828),
                Summary = "Senhas, 2FA, phishing, atualizações e backups: o básico que evita a maioria dos ataques.",
                Slides = new[]
                {
                    S("O elo mais fraco", "A maioria dos ataques não começa com um hacker de filme, e sim com alguém enganado.", null,
                      "Engenharia social: o golpista explora urgência, autoridade, medo e curiosidade.",
                      "'Sou da TI, me passa sua senha' — nenhuma equipe séria pede isso.",
                      "Desconfiar não é falta de educação: é profissionalismo."),
                    S("Senhas fortes de verdade", "'Abc@123' cai em segundos. 'cafe-com-pao-de-queijo-na-sexta' resiste anos.", null,
                      "Tamanho vale mais que símbolos: use frases longas.",
                      "Uma senha diferente para cada serviço — vazou uma, as outras continuam seguras.",
                      "Use um gerenciador de senhas: ele lembra por você."),
                    S("Autenticação em dois fatores (2FA)", "Mesmo que roubem sua senha, falta o segundo fator.", null,
                      "Algo que você sabe (senha) + algo que você tem (celular, chave física).",
                      "App autenticador e chave física são mais seguros que SMS.",
                      "Chegou um código que você NÃO pediu? Alguém tem sua senha: troque já e avise a segurança."),
                    S("Reconhecendo phishing", "Phishing é a isca. Não morda!", null,
                      "Confira o remetente e o domínio — leia da direita para a esquerda: micorosoft.com.golpe.ru é golpe.",
                      "Passe o mouse sobre o link antes de clicar e veja o destino real.",
                      "Pedido de senha, pagamento urgente ou segredo = pare e confirme por outro canal."),
                    S("Atualizações e backups", "Atualizar e ter backup é chato... até o dia em que salva a empresa.", null,
                      "Atualizações corrigem falhas conhecidas: adiar é deixar a porta destrancada.",
                      "Regra 3-2-1: 3 cópias, em 2 mídias diferentes, 1 fora do local.",
                      "Teste a restauração: backup que não volta não é backup."),
                    S("Viu algo estranho? Reporte!", "Aqui ninguém é punido por avisar de boa-fé. Quem esconde, sim.", null,
                      "Reportar rápido diminui o estrago: minutos fazem diferença.",
                      "Ransomware (arquivos travados e pedido de resgate): isole a máquina da rede e chame o SOC.",
                      "Nunca tente 'investigar sozinho' um ataque: preserve as evidências."),
                },
                Quiz = new[]
                {
                    Q("O que deixa uma senha mais forte?", "Comprimento é o que mais pesa. E reutilizar senha transforma um vazamento em vários.",
                      "Ser longa (uma frase) e única para cada serviço", "Trocar a letra 'a' por '@'", "Usar o nome do pet com o ano", "Usar a mesma senha forte em todos os sites"),
                    Q("Chegou um código de verificação que você não pediu. O que isso indica?", "Alguém digitou a sua senha e caiu no segundo fator. Troque a senha e avise a segurança.",
                      "Alguém tem sua senha e está tentando entrar", "O sistema está com defeito", "É um brinde da empresa", "Nada, pode ignorar sempre"),
                    Q("A regra de backup 3-2-1 significa...", "3 cópias dos dados, em 2 tipos de mídia, sendo 1 fora do local (outro prédio ou nuvem).",
                      "3 cópias, em 2 mídias diferentes, 1 fora do local", "3 backups por dia, 2 por semana e 1 por mês", "3 senhas, 2 e-mails e 1 celular", "3 discos, 2 servidores e 1 pendrive"),
                }
            },
            new Lesson
            {
                Id = "ia", Title = "IA generativa com responsabilidade", Teacher = "Dra. Ada", Role = "Palestrante convidada", Color = Art.Hx(0x8E6CC4),
                Summary = "Como funcionam os modelos de linguagem, alucinações, bons prompts, privacidade e vieses.",
                Slides = new[]
                {
                    S("O que é um modelo de linguagem", "Ele não 'pensa' como nós: ele prevê texto muito, muito bem.", null,
                      "Aprende padrões em uma quantidade enorme de textos.",
                      "Gera a resposta palavra por palavra, escolhendo continuações prováveis.",
                      "É ótimo com linguagem; pode errar fatos, contas e detalhes."),
                    S("Alucinação", "A IA pode inventar uma lei, um livro ou uma função — com a maior confiança do mundo.", null,
                      "Alucinação = informação inventada com aparência de verdade.",
                      "Confira dados importantes em fontes confiáveis (documentação, sites oficiais).",
                      "Pediu fontes? Verifique se elas existem e dizem mesmo aquilo."),
                    S("Bons prompts", "Quanto melhor a pergunta, melhor a resposta.",
                      "Sou estagiário de dados. Explique groupby do pandas\npara iniciantes, com um exemplo de vendas por loja,\nem até 10 linhas de código.",
                      "Dê contexto: quem você é e para que precisa.",
                      "Diga o formato: lista, tabela, código, tamanho máximo.",
                      "Itere: refine a pergunta com base na primeira resposta."),
                    S("IA para programar", "Use como colega experiente que às vezes se engana — não como oráculo.", null,
                      "Ótima para explicar código, sugerir testes e destravar ideias.",
                      "Todo código gerado precisa ser lido, entendido e testado por você.",
                      "Atenção à segurança: ela pode sugerir SQL concatenado, senha no código, dependências velhas."),
                    S("Dados e privacidade", "O que você cola numa ferramenta pública pode sair do controle da empresa.", null,
                      "Nunca cole dados pessoais de clientes nem segredos da empresa em ferramentas não aprovadas.",
                      "Siga a política da empresa e a LGPD.",
                      "Precisa usar dados reais? Anonimize antes."),
                    S("Vieses e transparência", "Tecnologia boa é a que trata todo mundo de forma justa.", null,
                      "Modelos aprendem preconceitos presentes nos dados.",
                      "Teste os resultados com pessoas e situações diversas.",
                      "Seja transparente: diga quando usou IA em trabalhos e relatórios."),
                },
                Quiz = new[]
                {
                    Q("O que é 'alucinação' de uma IA?", "É quando o modelo gera algo falso com aparência de verdadeiro: fatos, fontes ou código inexistentes.",
                      "Quando ela inventa informações com aparência de verdade", "Quando ela fica lenta", "Quando ela se recusa a responder", "Quando ela gera imagens coloridas demais"),
                    Q("Você vai usar IA para resumir reclamações de clientes. O que fazer primeiro?", "Dados pessoais exigem cuidado (LGPD): anonimize e use só ferramentas aprovadas pela empresa.",
                      "Remover os dados pessoais e seguir a política da empresa", "Colar tudo como está, é mais rápido", "Pedir para a IA guardar os CPFs", "Usar sua conta pessoal para ninguém ver"),
                    Q("A IA gerou uma função de login para o app. Qual é a atitude certa?", "Código de IA é código como qualquer outro: você é responsável por ler, entender e testar.",
                      "Ler, entender e testar antes de usar", "Colar direto em produção", "Confiar, IA não erra código", "Pedir para outra IA aprovar e pronto"),
                }
            },
            new Lesson
            {
                Id = "bancos", Title = "Bancos de dados: do zero ao JOIN", Teacher = "Glória", Role = "Líder de Dados e DBA", Color = Art.Hx(0x00838F),
                Summary = "Tabelas, chaves, relacionamentos, CRUD, JOIN, índices e SQL Injection.",
                Slides = new[]
                {
                    S("Por que um banco de dados?", "Planilha é ótima... até 50 pessoas editarem ao mesmo tempo.", null,
                      "Guarda dados com segurança, sem duplicar e sem perder.",
                      "Muitos usuários ao mesmo tempo, com regras que impedem bagunça.",
                      "Consultas rápidas mesmo com milhões de registros."),
                    S("Tabelas, linhas e colunas", "Cada tabela fala de um assunto só.", null,
                      "Tabela = um assunto (clientes, produtos, pedidos).",
                      "Linha = um registro; coluna = uma característica (nome, preço...).",
                      "Chave primária (PRIMARY KEY) identifica cada linha de forma única."),
                    S("Relacionamentos", "Em vez de repetir o nome do cliente em cada pedido, guardamos só o id dele.",
                      "pedidos.cliente_id  →  clientes.id",
                      "Chave estrangeira (FOREIGN KEY) liga uma tabela à outra.",
                      "Um cliente tem muitos pedidos (relação 1:N).",
                      "Sem repetição, mudar o nome do cliente é mudar um lugar só (normalização)."),
                    S("CRUD em SQL", "Quatro operações fazem quase tudo.",
                      "INSERT INTO ...                -- Create\nSELECT ... FROM ...            -- Read\nUPDATE ... SET ... WHERE ...   -- Update\nDELETE FROM ... WHERE ...      -- Delete",
                      "SELECT lê, INSERT cria, UPDATE altera e DELETE apaga.",
                      "UPDATE e DELETE sem WHERE afetam TODAS as linhas!",
                      "Antes de alterar, rode um SELECT com o mesmo WHERE para conferir."),
                    S("Juntando tabelas com JOIN", "O JOIN reconstrói a informação completa a partir das tabelas separadas.",
                      "SELECT c.nome, p.total\nFROM pedidos p\nJOIN clientes c ON p.cliente_id = c.id;",
                      "JOIN combina linhas relacionadas pela chave.",
                      "LEFT JOIN mantém todas as linhas da esquerda, mesmo sem par (o lado direito vem NULL).",
                      "Apelidos (p, c) deixam a consulta curta e sem ambiguidade."),
                    S("Índices e segurança", "Rapidez e segurança andam juntas num bom banco.", null,
                      "Índice acelera buscas, como o índice de um livro (mas deixa escritas um pouco mais lentas).",
                      "SQL Injection: NUNCA junte texto digitado pelo usuário dentro do SQL.",
                      "Use consultas preparadas (prepared statements): os valores vão separados do comando."),
                },
                Quiz = new[]
                {
                    Q("Para que serve a chave primária?", "Ela identifica cada linha de forma única; por isso não pode repetir nem ser nula.",
                      "Identificar cada linha de forma única", "Criptografar a tabela", "Ordenar a tabela por nome", "Ligar o banco à internet"),
                    Q("O que acontece com o comando  DELETE FROM pedidos;", "Sem WHERE, o DELETE apaga TODAS as linhas da tabela.",
                      "Apaga TODOS os pedidos", "Apaga só o último pedido", "Dá erro por falta de WHERE", "Não faz nada sem COMMIT"),
                    Q("Como evitar SQL Injection?", "Com consultas preparadas, o banco trata o que o usuário digitou como valor, nunca como comando.",
                      "Usando consultas preparadas, com os valores separados do SQL", "Escondendo o nome das tabelas", "Escrevendo o SQL só em maiúsculas", "Colocando o banco em outro servidor"),
                }
            },
            new Lesson
            {
                Id = "carreira", Title = "Carreira em tecnologia", Teacher = "Renata", Role = "Tech Recruiter", Color = Art.Hx(0xE76F51),
                Summary = "Portas de entrada, portfólio, currículo, entrevistas e as habilidades que fazem diferença.",
                Slides = new[]
                {
                    S("Muitas portas de entrada", "Eu entrevisto gente todo dia. Ninguém começa sabendo tudo!", null,
                      "Suporte, QA, dados, desenvolvimento, infraestrutura, segurança, UX...",
                      "Comece por uma área e vá ampliando: o seu título aqui no jogo mostra isso.",
                      "Estágios e programas de jovem talento são ótimas portas de entrada."),
                    S("Portfólio fala mais que currículo", "Quero ver o que você construiu, mesmo que seja pequeno.", null,
                      "Projetos pequenos e COMPLETOS no GitHub.",
                      "README explicando o problema, como rodar e o que você aprendeu.",
                      "Um projeto bem feito vale mais que dez pela metade."),
                    S("Currículo e LinkedIn", "Recrutador lê um currículo em segundos. Facilite!", null,
                      "Uma página, objetivo claro e links para os projetos.",
                      "Mostre resultados com números: 'reduzi o tempo de carregamento em 40%'.",
                      "Mantenha atualizado e participe de comunidades e eventos."),
                    S("Entrevista técnica", "Eu quero entender como você pensa, não ver você decorar.", null,
                      "Pense em voz alta: o raciocínio importa tanto quanto a resposta.",
                      "'Não sei, mas eu pesquisaria assim...' é uma ótima resposta.",
                      "Treine explicar seus projetos: por que escolheu cada coisa?"),
                    S("Habilidades comportamentais", "Código se aprende rápido. Atitude é o que faz a pessoa crescer.", null,
                      "Comunicação, colaboração e responsabilidade.",
                      "Pedir ajuda DEPOIS de tentar sozinho é sinal de maturidade.",
                      "Feedback é presente: ouça sem se defender e agradeça."),
                    S("Aprender sempre", "A tecnologia muda rápido. Quem aprende sempre, nunca fica para trás.", null,
                      "Crie o hábito de estudar um pouco toda semana.",
                      "A documentação oficial é sua melhor amiga (a Biblioteca daqui também!).",
                      "Ensinar o que você aprendeu é o melhor jeito de fixar."),
                },
                Quiz = new[]
                {
                    Q("O que mais chama a atenção de recrutadores em quem está começando?", "Projetos práticos e bem documentados mostram o que você sabe fazer de verdade.",
                      "Projetos práticos bem documentados", "Uma lista enorme de tecnologias, sem projetos", "Um currículo de cinco páginas", "Fotos profissionais caras"),
                    Q("Numa entrevista, você não sabe a resposta. Qual é a melhor atitude?", "Honestidade + raciocínio. Inventar costuma ser percebido na hora.",
                      "Ser honesto e explicar como pesquisaria ou resolveria", "Inventar uma resposta com confiança", "Mudar de assunto", "Dizer que a pergunta está errada"),
                    Q("Na descrição de um projeto, qual frase é mais forte?", "Resultados concretos e medidos mostram impacto.",
                      "Reduzi o tempo de carregamento da página em 40%", "Trabalhei com várias coisas", "Sou muito esforçado", "Usei o computador no projeto"),
                }
            },
        };

        public static Lesson Get(string id) { return All.FirstOrDefault(l => l.Id == id); }
        public static int DoneCount(Progress p) { return All.Count(l => p.Stat("aula:" + l.Id) > 0); }
    }

    // Janela da aula: lista de aulas à esquerda, telão com os slides à direita e quiz no final.
    class LessonForm : Form
    {
        readonly GameState gs;
        readonly float scale;
        readonly ListBox list;
        readonly Panel screen;
        readonly Button btnPrev, btnNext;
        readonly List<Button> optButtons = new List<Button>();
        readonly Font fTitle, fBullet, fCode, fSpeech, fSmall, fBig, fItem, fItemSmall, fOpt;

        Lesson lesson;
        int slide;                 // índice do slide; Slides.Length.. = quiz
        int qIndex = -1, hits, answered = -1;
        List<int> order;           // ordem embaralhada das opções da pergunta atual
        bool finished;
        readonly Random rng = new Random();

        // resultado para o jogo dar o prêmio
        public string CompletedId;
        public int Hits, Total;

        int S(float v) { return (int)Math.Round(v * scale); }

        class DbPanel : Panel { public DbPanel() { DoubleBuffered = true; ResizeRedraw = true; } }

        public LessonForm(GameState gs, float scale)
        {
            this.gs = gs; this.scale = scale;
            Text = "Stack Quest — Auditório: aulas";
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            BackColor = Theme.Panel;
            ForeColor = Theme.Text;
            KeyPreview = true;
            var wa = Screen.PrimaryScreen.WorkingArea;
            ClientSize = new Size(Math.Min(S(1280), wa.Width - 40), Math.Min(S(780), wa.Height - 60));
            MinimumSize = new Size(S(900), S(560));
            fTitle = new Font("Segoe UI Black", 22f);
            fBullet = new Font("Segoe UI", 13.5f);
            fCode = new Font("Consolas", 12.5f);
            fSpeech = new Font("Segoe UI", 11.5f, FontStyle.Italic);
            fSmall = new Font("Segoe UI Semibold", 9.5f);
            fBig = new Font("Segoe UI Semibold", 15f);
            fItem = new Font("Segoe UI Semibold", 11f);
            fItemSmall = new Font("Segoe UI", 9f);
            fOpt = new Font("Segoe UI", 12f);

            var header = new Panel { Dock = DockStyle.Top, Height = S(64), Padding = new Padding(S(20), S(10), S(20), 0) };
            header.Controls.Add(new Label { Text = "AUDITÓRIO · AULAS DA MICOROSOFT", Font = new Font("Segoe UI Black", 14f), ForeColor = Art.Hx(0xD9A4F5), AutoSize = true, Location = new Point(S(20), S(8)), UseMnemonic = false });
            header.Controls.Add(new Label { Text = "Escolha uma aula, assista aos slides (← →) e responda o quiz no final. A primeira vez que você conclui uma aula vale pontos e XP.", Font = new Font("Segoe UI", 9.5f), ForeColor = Theme.Muted, AutoSize = true, Location = new Point(S(22), S(38)), UseMnemonic = false });

            var left = new Panel { Dock = DockStyle.Left, Width = S(330), Padding = new Padding(S(14), S(4), S(8), S(14)) };
            list = new ListBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, BackColor = Theme.Bg, ForeColor = Theme.Text, DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = S(74), IntegralHeight = false };
            Program.DarkScroll(list);
            foreach (var l in Lessons.All) list.Items.Add(l);
            list.DrawItem += DrawItem;
            list.SelectedIndexChanged += (s, e) => { var l = list.SelectedItem as Lesson; if (l != null && l != lesson) Start(l); };
            left.Controls.Add(list);

            var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(S(8), S(4), S(18), S(14)) };
            var bottom = new Panel { Dock = DockStyle.Bottom, Height = S(56), Padding = new Padding(0, S(10), 0, 0) };
            btnPrev = MkBtn("◄  Anterior", Theme.Border);
            btnPrev.Dock = DockStyle.Left; btnPrev.Width = S(160);
            btnPrev.Click += (s, e) => Prev();
            btnNext = MkBtn("Próximo  ►", Art.Hx(0x8E6CC4));
            btnNext.Dock = DockStyle.Right; btnNext.Width = S(260);
            btnNext.Click += (s, e) => Next();
            bottom.Controls.Add(btnPrev);
            bottom.Controls.Add(btnNext);
            screen = new DbPanel { Dock = DockStyle.Fill, BackColor = Art.Hx(0x0B1020) };
            screen.Paint += PaintScreen;
            screen.Resize += (s, e) => LayoutOptions();
            for (int i = 0; i < 4; i++)
            {
                int k = i;
                var b = MkBtn("", Art.Hx(0x1F2A44));
                b.Font = fOpt;
                b.TextAlign = ContentAlignment.MiddleLeft;
                b.Padding = new Padding(S(14), 0, S(10), 0);
                b.Visible = false;
                b.Click += (s, e) => Answer(k);
                optButtons.Add(b);
                screen.Controls.Add(b);
            }
            right.Controls.Add(screen);
            right.Controls.Add(bottom);

            Controls.Add(right);
            Controls.Add(left);
            Controls.Add(header);
            KeyDown += OnKey;
            Shown += (s, e) => { list.SelectedIndex = Math.Max(0, Array.FindIndex(Lessons.All, l => gs.Prog.Stat("aula:" + l.Id) == 0)); btnNext.Focus(); };
        }

        Button MkBtn(string text, Color bg)
        {
            var b = new Button { Text = text, FlatStyle = FlatStyle.Flat, BackColor = bg, ForeColor = Color.White, Font = fItem, Cursor = Cursors.Hand, UseVisualStyleBackColor = false, TabStop = false, UseMnemonic = false };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bg, 0.25f);
            return b;
        }

        void OnKey(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { Close(); return; }
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) { Next(); e.Handled = true; e.SuppressKeyPress = true; return; }
            if (e.KeyCode == Keys.Left) { Prev(); e.Handled = true; e.SuppressKeyPress = true; return; }
            if (InQuiz && answered < 0 && e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D4) { Answer(e.KeyCode - Keys.D1); e.Handled = true; }
        }

        bool InQuiz { get { return lesson != null && slide >= lesson.Slides.Length && !finished; } }
#if DEMO
        public void DebugNext() { if (lesson == null) Start(Lessons.All[0]); else Next(); }
        public void DebugAnswer(int k) { Answer(k); }
#endif

        void Start(Lesson l)
        {
            lesson = l; slide = 0; qIndex = -1; hits = 0; answered = -1; finished = false;
            Sfx.Play("open");
            Sync();
        }

        void Prev()
        {
            if (lesson == null || slide == 0 || InQuiz || finished) return;
            slide--;
            Sync();
        }

        void Next()
        {
            if (lesson == null) return;
            if (finished) { Close(); return; }
            if (slide < lesson.Slides.Length - 1) { slide++; Sync(); return; }
            if (slide == lesson.Slides.Length - 1) { slide++; NextQuestion(); return; }
            // no quiz: só avança depois de responder
            if (answered < 0) return;
            if (qIndex + 1 < lesson.Quiz.Length) NextQuestion();
            else Finish();
        }

        void NextQuestion()
        {
            qIndex++;
            answered = -1;
            order = Enumerable.Range(0, lesson.Quiz[qIndex].Options.Length).OrderBy(x => rng.Next()).ToList();
            Sync();
        }

        void Answer(int k)
        {
            if (!InQuiz || answered >= 0 || k >= order.Count) return;
            answered = k;
            bool ok = order[k] == 0;
            if (ok) hits++;
            Sfx.Play(ok ? "ok" : "err");
            Sync();
        }

        void Finish()
        {
            finished = true;
            CompletedId = lesson.Id; Hits = hits; Total = lesson.Quiz.Length;
            Sfx.Play("unlock");
            Sync();
        }

        void Sync()
        {
            bool quiz = InQuiz;
            btnPrev.Enabled = lesson != null && slide > 0 && !quiz && !finished;
            btnPrev.BackColor = btnPrev.Enabled ? Theme.Border : Theme.Bg;
            if (lesson == null) btnNext.Text = "Escolha uma aula";
            else if (finished) btnNext.Text = "Concluir  ✔";
            else if (slide < lesson.Slides.Length - 1) btnNext.Text = "Próximo  ►";
            else if (slide == lesson.Slides.Length - 1) btnNext.Text = "Quiz da aula  ►";
            else btnNext.Text = answered < 0 ? "Responda (teclas 1 a 4)" : qIndex + 1 < lesson.Quiz.Length ? "Próxima pergunta  ►" : "Ver resultado  ►";
            for (int i = 0; i < optButtons.Count; i++)
            {
                var b = optButtons[i];
                b.Visible = quiz && i < order.Count;
                if (!b.Visible) continue;
                var q = lesson.Quiz[qIndex];
                b.Text = (i + 1) + ")  " + q.Options[order[i]];
                Color bg = Art.Hx(0x1F2A44);
                if (answered >= 0)
                {
                    if (order[i] == 0) bg = Art.Hx(0x1E6B3A);
                    else if (i == answered) bg = Art.Hx(0x8B2C2C);
                }
                b.BackColor = bg;
                b.FlatAppearance.MouseOverBackColor = answered >= 0 ? bg : ControlPaint.Light(bg, 0.25f);
                b.Cursor = answered >= 0 ? Cursors.Default : Cursors.Hand;
            }
            LayoutOptions();
            list.Invalidate();
            screen.Invalidate();
        }

        void LayoutOptions()
        {
            int w = screen.ClientSize.Width - S(80), y = (int)(screen.ClientSize.Height * 0.36f), h = S(48);
            for (int i = 0; i < optButtons.Count; i++)
                optButtons[i].SetBounds(S(40), y + i * (h + S(10)), Math.Max(S(200), w), h);
        }

        void DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var l = (Lesson)list.Items[e.Index];
            var g = e.Graphics;
            var r = e.Bounds;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillRectangle(Art.B(Theme.Bg), r);
            bool sel = l == lesson;
            Art.RR(g, sel ? Art.Hx(0x1F2A44) : Art.Hx(0x161B22), r.X + S(2), r.Y + S(3), r.Width - S(4), r.Height - S(6), S(8));
            Art.RR(g, l.Color, r.X + S(2), r.Y + S(3), S(5), r.Height - S(6), S(3));
            bool done = gs.Prog.Stat("aula:" + l.Id) > 0;
            TextRenderer.DrawText(g, l.Title, fItem, new Rectangle(r.X + S(16), r.Y + S(9), r.Width - S(56), S(24)), Theme.Bright, TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g, "Prof. " + l.Teacher + " · " + l.Role, fItemSmall, new Rectangle(r.X + S(16), r.Y + S(33), r.Width - S(30), S(18)), l.Color, TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g, done ? "✔ concluída · " + gs.Prog.Stat("aula:" + l.Id + ":acertos") + "/" + l.Quiz.Length + " no quiz" : l.Slides.Length + " slides + quiz", fItemSmall,
                new Rectangle(r.X + S(16), r.Y + S(51), r.Width - S(30), S(18)), done ? Theme.Green : Theme.Muted, TextFormatFlags.Left);
        }

        void PaintScreen(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            int W = screen.ClientSize.Width, H = screen.ClientSize.Height;
            using (var lg = new LinearGradientBrush(new Rectangle(0, 0, W, H), Art.Hx(0x141B33), Art.Hx(0x0B1020), 90f)) g.FillRectangle(lg, 0, 0, W, H);
            if (lesson == null) return;
            // faixa superior: aula, professor e progresso
            g.FillRectangle(Art.B(lesson.Color), 0, 0, W, S(6));
            string top = lesson.Title.ToUpperInvariant() + "   ·   Prof. " + lesson.Teacher;
            g.DrawString(top, fSmall, Art.B(Art.Lt(lesson.Color, 0.35f)), S(40), S(22));
            int total = lesson.Slides.Length + 1;
            int cur = Math.Min(slide, lesson.Slides.Length);
            for (int i = 0; i < total; i++)
            {
                float cx = W - S(40) - (total - 1 - i) * S(18);
                bool on = i <= cur;
                g.FillEllipse(Art.B(on ? lesson.Color : Art.Hx(0x2A3350)), cx - S(5), S(26), S(10), S(10));
            }

            if (finished) { PaintResult(g, W, H); return; }
            if (InQuiz) { PaintQuiz(g, W, H); return; }

            var sl = lesson.Slides[slide];
            float y = S(62);
            g.DrawString(sl.Title, fTitle, Art.B(Color.White), new RectangleF(S(40), y, W - S(80), S(60)));
            y += S(66);
            g.FillRectangle(Art.B(lesson.Color), S(42), y - S(6), S(70), S(4));
            y += S(16);
            foreach (var b in sl.Bullets)
            {
                var sz = g.MeasureString(b, fBullet, W - S(120));
                g.FillEllipse(Art.B(lesson.Color), S(46), y + S(10), S(10), S(10));
                g.DrawString(b, fBullet, Art.B(Theme.Text), new RectangleF(S(68), y, W - S(120), sz.Height + 2));
                y += sz.Height + S(14);
            }
            if (sl.Code != null)
            {
                var lines = sl.Code.Split('\n');
                float lh = fCode.GetHeight(g), ch = lines.Length * lh + S(24);
                Art.RR(g, Art.Hx(0x05070D), S(46), y + S(4), W - S(92), ch, S(8));
                Art.RRO(g, Art.Hx(0x2A3350), 1, S(46), y + S(4), W - S(92), ch, S(8));
                for (int i = 0; i < lines.Length; i++)
                {
                    string ln = lines[i];
                    Color c = ln.TrimStart().StartsWith("#") || ln.Contains("--") && ln.IndexOf("--") > 10 ? Art.Hx(0x6A9955) : Art.Hx(0x9CDCFE);
                    g.DrawString(ln, fCode, Art.B(c), S(62), y + S(16) + i * lh);
                }
                y += ch + S(16);
            }
            // fala do professor (rodapé do slide)
            if (!string.IsNullOrEmpty(sl.Speech))
            {
                string sp = lesson.Teacher + ": “" + sl.Speech + "”";
                var ssz = g.MeasureString(sp, fSpeech, W - S(120));
                float sy = Math.Max(y + S(10), H - ssz.Height - S(30));
                Art.RR(g, Art.A(lesson.Color, 40), S(40), sy - S(10), W - S(80), ssz.Height + S(20), S(10));
                g.DrawString(sp, fSpeech, Art.B(Art.Lt(lesson.Color, 0.5f)), new RectangleF(S(56), sy, W - S(112), ssz.Height + 2));
            }
            g.DrawString("slide " + (slide + 1) + " de " + lesson.Slides.Length + "   ·   ← → navegam", fItemSmall, Art.B(Theme.Muted), S(40), H - S(22));
        }

        void PaintQuiz(Graphics g, int W, int H)
        {
            var q = lesson.Quiz[qIndex];
            g.DrawString("QUIZ DA AULA · PERGUNTA " + (qIndex + 1) + " DE " + lesson.Quiz.Length, fSmall, Art.B(Theme.Yellow), S(40), S(56));
            g.DrawString(q.Prompt, fBig, Art.B(Color.White), new RectangleF(S(40), S(84), W - S(80), H * 0.36f - S(90)));
            if (answered >= 0)
            {
                bool ok = order[answered] == 0;
                float y = H * 0.36f + 4 * S(58) + S(6);
                string t = (ok ? "✔ Isso mesmo! " : "✖ Não foi dessa vez. ") + q.Explain;
                g.DrawString(t, fBullet, Art.B(ok ? Theme.Green : Art.Hx(0xFF8A80)), new RectangleF(S(40), y, W - S(80), H - y - S(10)));
            }
        }

        void PaintResult(Graphics g, int W, int H)
        {
            bool first = gs.Prog.Stat("aula:" + lesson.Id) == 0;
            g.DrawString("AULA CONCLUÍDA!", fTitle, Art.B(Theme.Green), S(40), S(70));
            g.DrawString(lesson.Title, fBig, Art.B(Color.White), S(42), S(122));
            g.DrawString("Você acertou " + hits + " de " + lesson.Quiz.Length + " perguntas do quiz.", fBullet, Art.B(Theme.Text), S(42), S(170));
            int pts = Lessons.BasePts + Lessons.PtsPerHit * hits, xp = Lessons.BaseXp + Lessons.XpPerHit * hits;
            string prize = first ? "Prêmio: +" + pts + " pontos e +" + xp + " XP (entregue ao fechar a aula)." : "Você já tinha concluído esta aula: rever não dá prêmio, mas fixa o conteúdo!";
            g.DrawString(prize, fBullet, Art.B(Theme.Yellow), new RectangleF(S(42), S(206), W - S(84), S(60)));
            g.DrawString("Resumo: " + lesson.Summary, fBullet, Art.B(Theme.Muted), new RectangleF(S(42), S(270), W - S(84), S(80)));
            int done = Lessons.DoneCount(gs.Prog) + (first ? 1 : 0);
            g.DrawString("Aulas concluídas: " + done + " de " + Lessons.All.Length, fSmall, Art.B(Theme.Muted), S(42), H - S(40));
        }
    }
}
