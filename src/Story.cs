using System.Drawing;

namespace DockerQuest
{
    class Look
    {
        public Color Skin, Hair, Shirt, Pants, Accent;
        public int HairStyle;          // 0 curto, 1 longo, 2 coque, 3 raspado, 4 black power, 5 boné
        public bool Glasses, Headset, Badge;
        public bool Female;            // corpo feminino (ombros mais estreitos, cílios, lábios)
        public int Bottom;             // 0 calça, 1 saia
        public string Hat, Acc, Outfit, Pet;   // itens da lojinha (só o jogador usa)
        public string Ride;                    // "patinete" quando o jogador está em cima do patinete
        static int counter;
        public readonly int Id = System.Threading.Interlocked.Increment(ref counter);

        public static Look Make(int skin, int hair, int style, int shirt, int pants, bool glasses = false, bool headset = false)
        {
            return new Look
            {
                Skin = Art.Hx(skin), Hair = Art.Hx(hair), HairStyle = style, Shirt = Art.Hx(shirt), Pants = Art.Hx(pants),
                Accent = Art.Hx(shirt), Glasses = glasses, Headset = headset
            };
        }
    }

    enum NpcKind { Mentor, Helper, Flavor }

    class Npc
    {
        public string Name, Role, Greeting, HintPrefix;
        public string[] Lines;
        public int LineIdx;
        public Look Look;
        public float X, Y, HomeX, HomeY, TX, TY, Wait, Walk, Range;
        public int Facing, DefaultFacing, Chapter = -1, HelperIdx;   // Chapter = sala (área) do NPC
        public NpcKind Kind;
        public bool Wander, Moving, Trivia, Shop, Ceo;
        public bool Stroll;          // dando uma voltinha pela sala (volta sozinho para o lugar)
        public float Stuck;
        public int LastStep;         // último passo tocado (som de passos)
        public DialogEvent Event;   // evento de diálogo pendente (marcador "…")
        public string Activity;     // atividade da sala (quiz, phishing, chamados...), marcador "+"
    }

    class Sector
    {
        public string Name, Short, Cat;
        public Color F1, F2;
        public int Style;          // 0 piso, 1 madeira, 2 carpete, 3 piso elevado
        public string[] Intro;
    }


    static class Story
    {
        public const string Company = "Micorosoft";

        // Cores de pele / cabelo reutilizadas
        public const int Sk1 = 0xFFDBB4, Sk2 = 0xF1C27D, Sk3 = 0xE0AC69, Sk4 = 0xC68642, Sk5 = 0x8D5524, Sk6 = 0x5C3A21;
        public const int HBlack = 0x2B1B12, HBrown = 0x5A3A22, HBlond = 0xD9B45A, HRed = 0xA5402A, HGray = 0xA7ADB5, HBlue = 0x3B6FD8, HPink = 0xD65AA0;

        public static readonly Color CorridorA = Art.Hx(0xC9CED6), CorridorB = Art.Hx(0xC1C7D0);

        public const int Reception = 0, Directors = 9, Library = 10, Cafeteria = 11, Auditorium = 12, Garden = 13, GameRoom = 14, Soc = 15, HelpDesk = 16, Innovation = 17, SqlRoom = 18, PlayerRoom = 19, RoomCount = 20;

        static Sector S(string name, string shortName, string cat, int f1, int f2, int style, params string[] intro)
        {
            return new Sector { Name = name, Short = shortName, Cat = cat, F1 = Art.Hx(f1), F2 = Art.Hx(f2), Style = style, Intro = intro };
        }

        // Salas: 0 Recepção, 1 Docker, 2 Redes, 3 Python, 4 PHP, 5 Frontend, 6 React, 7 Flutter, 8 AWS, 9 Diretoria, 10 Biblioteca,
        //        11 Refeitório, 12 Auditório, 13 Jardim, 14 Sala de Jogos, 15 SOC, 16 Help Desk, 17 Lab de Inovação, 18 Centro de Dados SQL
        public static readonly Sector[] Sectors =
        {
            S("Recepção & RH", "RECEPÇÃO", null, 0xD8B98E, 0xCFAE82, 1),
            S("Laboratório de Containers", "CONTAINERS · DOCKER", "docker", 0xE3E9F0, 0xD5DDE7, 0,
                "Bem-vindo(a) ao Laboratório de Containers, {nome}! Eu sou o Rafael, líder de DevOps da Micorosoft.",
                "Todo o Café 365 roda em containers Docker: site, APIs, bancos. Aqui você aprende a criar, rodar e cuidar deles.",
                "Os desafios vão do nível Iniciante ao Profissional. Cada nível concluído vale uma badge com o seu nome!",
                "O terminal fica ali do lado. A Lia e o Jorge dão dicas, mas cada dica custa pontos. A Biblioteca é grátis."),
            S("Data Center & Redes", "REDES · DATA CENTER", "redes", 0xB9C1CC, 0xAEB7C3, 3,
                "Cuidado com os cabos! Eu sou a Sandra, líder de Infraestrutura e Redes.",
                "Se a rede cai, NADA funciona: nem o site, nem o app, nem a nuvem. Aqui você aprende IP, DNS, portas, SSH e firewall.",
                "Os desafios mais avançados caem em entrevistas de emprego. Use o terminal da sala quando estiver pronto(a)."),
            S("Laboratório de Dados", "DADOS · PYTHON", "python", 0x46506A, 0x414A62, 2,
                "Oi, {nome}! Yara, cientista de dados. Aqui o Python transforma milhões de pedidos em decisões.",
                "Você vai do primeiro print até pandas, Flask e classes.",
                "O Otávio e a Dani te ajudam se travar. O terminal aceita Python linha a linha."),
            S("Squad Backend PHP", "BACKEND · PHP", "php", 0x5E9C9F, 0x58959A, 2,
                "E aí! Igor, líder do Backend PHP. Metade da internet roda PHP — e o site legado da Micorosoft também.",
                "Aqui você aprende de echo até PDO seguro contra SQL Injection e Laravel.",
                "A Fê e o Rui conhecem cada linha desse sistema. Bora?"),
            S("Estúdio Frontend", "FRONTEND · HTML/CSS/JS", "front", 0xF2E3D5, 0xEAD8C6, 1,
                "Olá! Camila, líder do Frontend. Tudo que o cliente VÊ do Café 365 nasce aqui: HTML, CSS e JavaScript.",
                "Acessibilidade e bom código são regra da casa. A Nina é nossa especialista em acessibilidade.",
                "O terminal aceita uma linha de HTML, CSS ou JS por vez. Capricha!"),
            S("Squad React", "REACT", "react", 0x7E6BA8, 0x7764A0, 2,
                "Fala, {nome}! Thiago, líder do Squad React. O painel web do Café 365 é todo em React.",
                "Componentes, props, estado, hooks e contexto: você vai dominar tudo.",
                "Caio e Mari estão por aqui para ajudar."),
            S("Mobile Lab", "MOBILE · FLUTTER", "flutter", 0xDCE8F5, 0xD0DFEF, 0,
                "Bem-vindo(a) ao Mobile Lab! Bruno, líder de Mobile. O app Café 365 para Android e iOS é feito em Flutter.",
                "Um código só, dois sistemas. Você vai criar telas, botões, navegação e chamadas de API.",
                "O Kenji e a Paula testam o app em dezenas de celulares. Pergunte a eles se precisar."),
            S("Centro de Nuvem", "CLOUD · AWS", "aws", 0x343C4C, 0x2F3646, 0,
                "Bem-vindo(a) ao Centro de Nuvem! Helena, arquiteta de nuvem. Toda a Micorosoft roda na AWS.",
                "S3, EC2, IAM, Lambda, ECS, CloudFormation... e o mais importante: segurança e custos.",
                "Rodrigo e Ana estão de olho em tudo por aqui. O terminal tem a AWS CLI configurada."),
            S("Diretoria", "DIRETORIA", null, 0x8A3B3B, 0x7F3535, 2),
            S("Biblioteca & Documentação", "BIBLIOTECA", null, 0x8C5A3C, 0x83533A, 1),
            S("Refeitório", "REFEITÓRIO", null, 0xF3E9D8, 0xE9DCC6, 0),
            S("Auditório", "AUDITÓRIO", null, 0x5B3A4A, 0x543546, 2),
            S("Jardim de Inverno", "JARDIM", null, 0x7CB66A, 0x73AD61, 4),
            S("Sala de Jogos", "SALA DE JOGOS", null, 0x3A3F7A, 0x353A72, 2),
            S("SOC · Centro de Segurança", "SEGURANÇA · SOC", null, 0x2B3140, 0x272C3A, 3),
            S("Help Desk & Hardware", "HELP DESK", null, 0xD9DEE6, 0xCDD3DD, 0),
            S("Laboratório de Inovação", "INOVAÇÃO · IA", null, 0xE8E4F5, 0xDDD8EE, 0),
            S("Centro de Dados SQL", "DADOS · SQL", "sql", 0x2F5D62, 0x2B575C, 2,
                "Bem-vindo(a) ao Centro de Dados, {nome}! Eu sou a Glória, líder de Dados e DBA da Micorosoft.",
                "Cada pedido, cliente e produto do Café 365 mora num banco de dados MySQL. Quem sabe SQL sabe responder qualquer pergunta sobre o negócio.",
                "O terminal da sala abre o cliente mysql conectado ao banco cafe365, com dados de verdade (de teste!). Explorar é grátis: SHOW TABLES, DESCRIBE, SELECT * ...",
                "Só uma regra de ouro: UPDATE e DELETE sempre com WHERE. O Fábio e a Tânia ajudam se você travar."),
            // a sala do jogador: o piso muda conforme o que ele compra (PlayerRoom.Apply)
            S("Sua Sala", "SUA SALA", null, 0xB4B8BE, 0xACB0B6, 5),
        };

        public static Sector SectorOf(int zone) { return zone >= 0 && zone < Sectors.Length ? Sectors[zone] : Sectors[0]; }

        public static readonly string[] Intro =
        {
            "Olá, {nome}! Eu sou a Gabi, do RH. Seja muito bem-vindo(a) à Micorosoft — e ao Programa Jovem Talento!",
            "Estamos construindo o Café 365: site, app, APIs e infraestrutura para milhões de clientes. E precisamos de gente que aprenda rápido.",
            "Aqui todas as áreas estão abertas: Containers, Redes, Dados (Python), Backend PHP, Frontend, React, Mobile (Flutter), Nuvem (AWS) e Banco de Dados (SQL).",
            "Cada área tem desafios de nível Iniciante, Intermediário e Profissional. Cada nível concluído vale uma BADGE com o seu nome.",
            "Você começa como Estagiário(a). Resolvendo desafios, ganha XP e sobe de cargo. O seu título depende do que você mais pratica!",
            "De vez em quando alguém vai pedir ajuda com um problema urgente: é um DESAFIO RELÂMPAGO. Aceitar é opcional, mas vale muitos pontos.",
            "E atenção: suas atitudes contam. Ética, colaboração e responsabilidade aumentam sua REPUTAÇÃO. Atalhos perigosos te queimam.",
            "Não sabe algo? A Biblioteca (porta no fundo desta sala) tem a documentação de tudo, de graça. Controles: WASD anda, E interage, TAB mostra sua carreira, B suas badges.",
            "O campus é grande! Na ala leste ficam a Sala de Jogos, o SOC de Segurança, o Help Desk e o Laboratório de Inovação; no corredor sul, o Refeitório, o Auditório e o Jardim — e, descendo por ele, o Centro de Dados SQL. Pausas também fazem parte do trabalho.",
            "A Diretora, Valéria, fica na última sala do corredor de cima. Ela acompanha os talentos de perto. Boa sorte!"
        };

        public static readonly string[] Trivia =
        {
            "O Docker foi apresentado em 2013, numa palestra de 5 minutos na PyCon.",
            "O primeiro site da história (1991) ainda está no ar, no CERN — puro HTML.",
            "O JavaScript foi criado em apenas 10 dias, em 1995.",
            "O nome Python vem do grupo de comédia Monty Python, não da cobra.",
            "O PHP nasceu como 'Personal Home Page Tools' em 1994.",
            "O React foi criado no Facebook e lançado como open source em 2013.",
            "Flutter usa a linguagem Dart e desenha cada pixel da tela por conta própria.",
            "A AWS começou a vender nuvem pública em 2006, com o S3 e o EC2.",
            "O SQL nasceu na IBM nos anos 1970 e se chamava SEQUEL. Até hoje tem gente que pronuncia 'síquel'.",
            "O endereço 127.0.0.1 é sempre a sua própria máquina (localhost).",
            "O IPv6 tem 2^128 endereços: dá para dar trilhões para cada grão de areia da Terra.",
            "'Funciona na minha máquina' foi o problema que os containers vieram resolver.",
            "O primeiro 'bug' registrado foi uma mariposa presa num computador em 1947."
        };

        public static readonly string[] CeoLines =
        {
            "Bem-vindo(a) à Diretoria, {nome}. Eu sou a Valéria, CEO da Micorosoft.",
            "Aqui valorizamos três coisas: aprender sem parar, colaborar com o time e agir com ética.",
            "No Programa Jovem Talento, quem chega a Pleno com pelo menos 6 badges é efetivado(a). Estou de olho!"
        };
    }
}
