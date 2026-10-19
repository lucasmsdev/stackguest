using System;
using System.Collections.Generic;
using System.Drawing;

namespace DockerQuest
{
    class Furn
    {
        public string Kind, Text;
        public float X, Y, W, H;
        public Color C;
        public int Var;
    }

    class Item
    {
        public string Kind, Title, Text;
        public float X, Y;
        public int Chapter = -1;   // sala
    }

    class DoorInfo
    {
        public int Chapter;        // sala
        public readonly List<Point> Tiles = new List<Point>();
        public bool Vertical;
        public PointF Outside, Inside;
        public RectangleF Plate;
    }

    // Campus da Micorosoft (coordenadas em tiles):
    //   [Frontend 5][React 6][Flutter 7][AWS 8][Diretoria 9][Jogos 14][SOC 15]
    //   ============================ corredor principal ============================ ‖
    //   [Recepção 0][Docker 1][Redes 2][Python 3][PHP 4][Help Desk 16][Inovação 17]  ‖ corredor
    //   [Biblioteca 10][   Refeitório 11   ][   Auditório 12   ][   Jardim 13   ]   ‖ leste
    //   ================================ corredor sul ===============================
    //                                            [ Centro de Dados SQL 18 ]
    class World
    {
        public const int W = 111, H = 59;
        public const int Void = 0, Wall = 1, Floor = 2, Door = 3;

        public readonly int[,] Tile = new int[W, H];
        public readonly int[,] Zone = new int[W, H];
        public readonly int[,] DoorCh = new int[W, H];
        public readonly bool[,] Solid = new bool[W, H];

        public readonly List<Furn> Furniture = new List<Furn>();
        public readonly List<Furn> Decor = new List<Furn>();
        public readonly List<Furn> Rugs = new List<Furn>();
        public readonly List<Furn> RoomFurniture = new List<Furn>();   // móveis da sala do jogador (mudam durante o jogo)
        public readonly List<KeyValuePair<string, int>> RoomBadges = new List<KeyValuePair<string, int>>();   // badges já ganhas (quadros na parede da sala)
        public readonly List<Npc> Npcs = new List<Npc>();
        public readonly List<Item> Items = new List<Item>();
        public readonly DoorInfo[] Doors = new DoorInfo[Story.RoomCount];       // porta principal de cada sala
        public readonly List<DoorInfo> AllDoors = new List<DoorInfo>();         // todas as portas (inclui as secundárias)
        public readonly Rectangle[] Rooms = new Rectangle[Story.RoomCount];
        public readonly Npc[] Mentors = new Npc[Story.RoomCount];
        public readonly Npc[,] Helpers = new Npc[Story.RoomCount, 2];
        public readonly Furn[] TerminalDesks = new Furn[Story.RoomCount];
        public PointF Spawn = new PointF(7.5f, 26.4f);

        public DoorInfo LibraryDoor { get { return Doors[Story.Library]; } }

        public World() { Build(); }

        static Color Hx(int c) { return Art.Hx(c); }

        public bool InMap(int x, int y) { return x >= 0 && y >= 0 && x < W && y < H; }

        public int ZoneAt(float x, float y)
        {
            int tx = (int)Math.Floor(x), ty = (int)Math.Floor(y);
            return InMap(tx, ty) ? Zone[tx, ty] : -2;
        }

        public DoorInfo DoorOfZone(int zone) { return Doors[zone]; }

        void Carve(Rectangle r, int zone)
        {
            for (int x = r.Left; x < r.Right; x++)
                for (int y = r.Top; y < r.Bottom; y++) { Tile[x, y] = Floor; Zone[x, y] = zone; }
        }

        void AddDoor(int room, Point a, Point b, PointF outside, PointF inside, RectangleF plate, bool main = true)
        {
            var d = new DoorInfo { Chapter = room, Outside = outside, Inside = inside, Plate = plate };
            foreach (var p in new[] { a, b })
            {
                d.Tiles.Add(p);
                Tile[p.X, p.Y] = Door; Zone[p.X, p.Y] = -1; DoorCh[p.X, p.Y] = -1;
            }
            if (main) Doors[room] = d;
            AllDoors.Add(d);
        }

        // porta na parede de baixo da sala, abrindo para o corredor sul
        void SouthDoor(int room, int x, bool main)
        {
            AddDoor(room, new Point(x, 41), new Point(x + 1, 41), new PointF(x + 1, 42.8f), new PointF(x + 1, 39.9f), new RectangleF(x - 5.8f, 41.25f, 5.2f, 0.6f), main);
        }

        Furn F(string kind, float x, float y) { return F(kind, x, y, 1, 1); }

        Furn F(string kind, float x, float y, float w, float h, int color = 0x8D6E63, int var = 0, bool solid = true, string text = null)
        {
            var f = new Furn { Kind = kind, X = x, Y = y, W = w, H = h, C = Hx(color), Var = var, Text = text };
            Furniture.Add(f);
            if (solid)
                for (int tx = (int)Math.Floor(x); tx < (int)Math.Ceiling(x + w); tx++)
                    for (int ty = (int)Math.Floor(y); ty < (int)Math.Ceiling(y + h); ty++)
                        if (InMap(tx, ty)) Solid[tx, ty] = true;
            return f;
        }

        void WallDecor(string kind, float x, int y, float w, int color = 0, string text = null)
        {
            Decor.Add(new Furn { Kind = kind, X = x, Y = y, W = w, H = 1, C = Hx(color), Text = text });
        }

        void Rug(float x, float y, float w, float h, int color, int var = 0)
        {
            Rugs.Add(new Furn { Kind = "rug", X = x, Y = y, W = w, H = h, C = Hx(color), Var = var });
        }

        void It(string kind, float x, float y, int room, string title, string text)
        {
            Items.Add(new Item { Kind = kind, X = x, Y = y, Chapter = room, Title = title, Text = text });
        }

        static readonly HashSet<string> FemaleNames = new HashSet<string>
        {
            "Gabi", "Bia", "Lia", "Sandra", "Clara", "Yara", "Dani", "Fê", "Camila", "Yuki", "Nina", "Mari", "Paula", "Helena", "Ana",
            "Valéria", "Dona Célia", "Tati", "Mel", "Dona Lurdes", "Sofia", "Lu", "Dra. Ada", "Renata", "Lara", "Bel", "Kátia", "Jéssica",
            "Rita", "Rô", "Duda", "Glória", "Tânia"
        };

        Npc N(string name, string role, float x, float y, int facing, Look look, int room, NpcKind kind)
        {
            var n = new Npc
            {
                Name = name, Role = role, X = x, Y = y, HomeX = x, HomeY = y, TX = x, TY = y, Facing = facing,
                DefaultFacing = facing, Look = look, Chapter = room, Kind = kind
            };
            if (FemaleNames.Contains(name))
            {
                look.Female = true;
                int h = 0;
                foreach (char c in name) h += c;
                look.Bottom = h % 3 == 0 ? 1 : 0;   // algumas de saia, outras de calça
            }
            Npcs.Add(n);
            if (kind == NpcKind.Mentor) Mentors[room] = n;
            return n;
        }

        Npc Helper(int room, int idx, Npc n, string greeting, string prefix, params string[] small)
        {
            n.HelperIdx = idx; n.Greeting = greeting; n.HintPrefix = prefix; n.Lines = small;
            Helpers[room, idx] = n;
            return n;
        }

        Npc Flavor(Npc n, params string[] lines) { n.Lines = lines; return n; }

        void Terminal(int room, float x, float y)
        {
            TerminalDesks[room] = F("terminal", x, y, 2, 1, 0x2496ED, room);
            It("terminal", x + 1f, y + 1.4f, room, "Terminal", null);
        }

        static Look L(int skin, int hair, int style, int shirt, int pants, bool glasses = false, bool headset = false) { return Look.Make(skin, hair, style, shirt, pants, glasses, headset); }

        void Build()
        {
            for (int x = 0; x < W; x++)
                for (int y = 0; y < H; y++) { Tile[x, y] = Wall; Zone[x, y] = -2; DoorCh[x, y] = -1; }

            int[] bottom = { 0, 1, 2, 3, 4, Story.HelpDesk, Story.Innovation }, top = { 5, 6, 7, 8, 9, Story.GameRoom, Story.Soc };
            for (int c = 0; c < bottom.Length; c++)
            {
                int x0 = 1 + 15 * c;
                Rooms[bottom[c]] = new Rectangle(x0, 18, 14, 11);
                Rooms[top[c]] = new Rectangle(x0, 1, 14, 11);
            }
            Rooms[Story.Library] = new Rectangle(1, 30, 14, 11);
            Rooms[Story.Cafeteria] = new Rectangle(16, 30, 29, 11);
            Rooms[Story.Auditorium] = new Rectangle(46, 30, 29, 11);
            Rooms[Story.Garden] = new Rectangle(76, 30, 29, 11);
            Rooms[Story.SqlRoom] = new Rectangle(46, 47, 14, 11);   // abaixo do corredor sul
            Rooms[Story.PlayerRoom] = new Rectangle(PlayerRoom.X0, PlayerRoom.Y0, 14, 11);   // sala do jogador, ao lado do SQL
            for (int r = 0; r < Story.RoomCount; r++) Carve(Rooms[r], r);
            Carve(new Rectangle(1, 13, 109, 4), -1);    // corredor principal
            Carve(new Rectangle(1, 42, 109, 4), -1);    // corredor sul
            Carve(new Rectangle(106, 13, 4, 33), -1);   // corredor leste (liga os dois)

            for (int c = 0; c < bottom.Length; c++)
            {
                int x0 = 1 + 15 * c;
                AddDoor(bottom[c], new Point(x0 + 6, 17), new Point(x0 + 7, 17), new PointF(x0 + 7, 16.2f), new PointF(x0 + 7, 18.9f), new RectangleF(x0 + 1.2f, 17.22f, 4.6f, 0.6f));
                AddDoor(top[c], new Point(x0 + 6, 12), new Point(x0 + 7, 12), new PointF(x0 + 7, 13.9f), new PointF(x0 + 7, 10.9f), new RectangleF(x0 + 1.2f, 12.25f, 4.6f, 0.6f));
            }
            AddDoor(Story.Library, new Point(7, 29), new Point(8, 29), new PointF(8, 28.3f), new PointF(8, 30.9f), new RectangleF(1.6f, 29.25f, 5.0f, 0.6f));
            SouthDoor(Story.Library, 9, false);
            SouthDoor(Story.Cafeteria, 29, true);
            SouthDoor(Story.Auditorium, 59, true);
            SouthDoor(Story.Garden, 89, true);
            AddDoor(Story.SqlRoom, new Point(52, 46), new Point(53, 46), new PointF(53, 45.2f), new PointF(53, 47.9f), new RectangleF(47.2f, 46.22f, 4.6f, 0.6f));
            AddDoor(Story.PlayerRoom, new Point(37, 46), new Point(38, 46), new PointF(38, 45.2f), new PointF(38, 47.9f), new RectangleF(32.2f, 46.22f, 4.6f, 0.6f));
            It("board", 38f, 47.6f, Story.PlayerRoom, "Sua sala", "Esta sala é SUA, {nome}! Ela começa vazia: na Lojinha do corredor, aba \"Minha sala\", você compra mesa, computador, cadeira, piso e decoração com as moedas que ganha nos desafios. Computador e cadeira melhores também ajudam: mais XP e menos fome e sede.");

            BuildCorridor();
            BuildEastAndSouthCorridors();
            BuildCafeteria();
            BuildAuditorium();
            BuildGarden();
            BuildGameRoom();
            BuildSoc();
            BuildHelpDesk();
            BuildInnovation();
            BuildActivities();
            BuildReception();
            BuildDocker();
            BuildNetwork();
            BuildPython();
            BuildPhp();
            BuildFrontend();
            BuildReact();
            BuildFlutter();
            BuildAws();
            BuildSql();
            BuildDirectors();
            BuildLibrary();

            // paredes que não encostam em piso viram fundo
            var voids = new List<Point>();
            for (int x = 0; x < W; x++)
                for (int y = 0; y < H; y++)
                {
                    if (Tile[x, y] != Wall) continue;
                    bool near = false;
                    for (int dx = -1; dx <= 1 && !near; dx++)
                        for (int dy = -1; dy <= 1 && !near; dy++)
                            if (InMap(x + dx, y + dy) && (Tile[x + dx, y + dy] == Floor || Tile[x + dx, y + dy] == Door)) near = true;
                    if (!near) voids.Add(new Point(x, y));
                }
            foreach (var p in voids) Tile[p.X, p.Y] = Void;
        }

        // ───────────────────────── corredor ─────────────────────────
        void BuildCorridor()
        {
            F("coffee", 1, 13);
            It("coffee", 1.5f, 14.4f, -1, "Máquina de café", null);
            F("kiosk", 9, 13, 3, 1);
            It("shop", 10.5f, 14.4f, -1, "Lojinha", null);
            F("cooler", 13, 13);
            It("cooler", 13.5f, 14.4f, -1, "Bebedouro", "Você bebeu um copo d'água geladinha. Hidratação também é importante para quem programa!");
            F("plant", 15, 13);
            F("sofa", 25, 13, 3, 1, 0xC1666B); F("plant", 29, 13);
            F("bench", 40, 13, 3, 1, 0x7D8597); F("plant", 44, 13);
            F("plant", 55, 13); F("sofa", 57, 13, 3, 1, 0x3D405B);
            F("plant", 70, 13); F("plant", 74, 13);
            F("plant", 1, 16); F("bench", 11, 16, 3, 1, 0x7D8597); F("plant", 15, 16);
            F("bench", 26, 16, 3, 1, 0x7D8597); F("plant", 30, 16); F("plant", 41, 16);
            F("plant", 56, 16); F("bench", 57, 16, 3, 1, 0x7D8597); F("plant", 74, 16);

            WallDecor("poster", 14, 12, 1, 0x2496ED);
            It("board", 14.5f, 13.8f, -1, "Pôster: valores da Micorosoft", "VALORES DA MICOROSOFT: 1) Aprender sempre. 2) Colaborar sem ego. 3) Ética acima de tudo: dados de clientes, senhas e licenças são sagrados.");
            WallDecor("poster", 27, 12, 1, 0xE76F51);
            It("board", 27.5f, 13.8f, -1, "Pôster: carreira", "TRILHA DE CARREIRA: Estagiário(a) → Júnior → Pleno → Sênior → Especialista → Principal. O seu título mostra a sua especialidade: quem resolve Docker + AWS vira DevOps/Cloud, Python + PHP vira Backend, Frontend + Backend vira Fullstack...");
            WallDecor("poster", 42, 12, 1, 0x8E6CC4);
            It("board", 42.5f, 13.8f, -1, "Pôster: desafios relâmpago", "DESAFIOS RELÂMPAGO: problemas urgentes podem surgir em qualquer área. Aceitar é opcional, mas vale até 250 pontos e XP em dobro!");
            WallDecor("poster", 57, 12, 1, 0x3FB950);
            It("board", 57.5f, 13.8f, -1, "Pôster: badges", "BADGES: conclua todos os desafios de um nível (Iniciante, Intermediário ou Profissional) de uma área e ganhe uma badge com o seu nome. Tecla B mostra a coleção.");
            WallDecor("sign", 70.2f, 12, 3.6f, 0x1D3557, "MICOROSOFT");

            Flavor(N("Seu Zé", "Barista", 2.4f, 14.3f, 1, L(Story.Sk4, Story.HGray, 3, 0x6D4C41, 0x3E2723), -1, NpcKind.Flavor)).Trivia = true;
            var ro = N("Rô", "Vendedora da lojinha", 12.6f, 14.4f, 1, L(Story.Sk3, Story.HRed, 2, 0xE63946, 0x2B2D42), -1, NpcKind.Flavor);
            ro.Shop = true;
            ro.Lines = new[] { "Bem-vindo(a) à Lojinha da Micorosoft! Troque seus pontos por roupas, mascotes, lanches e ferramentas." };
            var leo = Flavor(N("Léo", "Marketing", 34, 14.8f, 0, L(Story.Sk2, Story.HRed, 4, 0xF4A261, 0x264653), -1, NpcKind.Flavor),
                "Eu tentei rodar 'docker run' no micro-ondas. Não funcionou.",
                "Dizem que a frase 'funciona na minha máquina' morreu quando inventaram os containers.",
                "A Micorosoft tem mais áreas do que eu tenho plantas. E eu tenho MUITAS plantas.");
            leo.Wander = true; leo.Range = 5;
            var dudu = Flavor(N("Duda", "Designer", 49, 15.2f, 0, L(Story.Sk1, Story.HPink, 1, 0x9B5DE5, 0x2B2D42, true), -1, NpcKind.Flavor),
                "Eu desenhei os pôsteres do corredor. Tem dicas boas neles!",
                "Todas as áreas estão abertas. Explore e escolha o que você mais curte.",
                "Dica de ouro: a documentação oficial é sua melhor amiga — e a Biblioteca resume tudo.");
            dudu.Wander = true; dudu.Range = 6;
            Flavor(N("Marcos", "Estagiário de Segurança", 62, 15.4f, 2, L(Story.Sk5, Story.HBlack, 0, 0x343A40, 0x212529), -1, NpcKind.Flavor),
                "Senha forte é longa: use frases. 'cafe-com-pao-de-queijo-2026' é melhor que 'Abc@123'.",
                "Nunca clique em links estranhos, mesmo que pareçam ser da Micorosoft.");
        }

        // ───────────────────────── recepção ─────────────────────────
        void BuildReception()
        {
            Rug(4, 21.4f, 7, 4, 0x2496ED, 2);
            F("counter", 2, 20, 4, 1, 0xE8EEF5, 1);
            F("plant", 1, 18); F("plant", 14, 18); F("plant", 1, 28); F("plant", 14, 28);
            F("sofa", 2, 27, 3, 1, 0x2A9D8F); F("sofa", 10, 27, 3, 1, 0x2A9D8F);
            F("shelf", 14, 23, 1, 2, 0x6B4A2F);
            WallDecor("board", 9, 17, 2, 0xB07D4F);
            It("board", 10f, 18.7f, 0, "Mural do RH", "MURAL DO RH: Programa Jovem Talento. Meta para efetivação: chegar a Pleno com pelo menos 6 badges. Converse com os líderes de cada área para começar!");
            WallDecor("sign", 11.2f, 17, 2.6f, 0x1D3557, "Micorosoft");

            var gabi = N("Gabi", "Recursos Humanos", 9.8f, 25.2f, 1, L(Story.Sk5, Story.HBlack, 4, 0xFFB703, 0x023047), 0, NpcKind.Flavor);
            gabi.Lines = new[] {
                "WASD anda, E interage, TAB mostra sua carreira, B suas badges, O seus objetivos, 1–8 usam itens.",
                "Lembre: reputação também conta na hora da efetivação!",
                "Não sabe por onde começar? Escolha uma área e fale com o líder dela."
            };
            Mentors[0] = gabi;
            Flavor(N("Paulo", "Suporte de TI", 3.6f, 19.8f, 0, L(Story.Sk2, Story.HBrown, 0, 0x457B9D, 0x1D3557, false, true), 0, NpcKind.Flavor),
                "Se o computador travar, já tentou desligar e ligar de novo?",
                "Senha esquecida? Eu reseto. Só não empreste a sua para ninguém!");
            Flavor(N("Bia", "Estagiária veterana", 5.6f, 27.4f, 2, L(Story.Sk1, Story.HBlond, 2, 0xE76F51, 0x3D405B), 0, NpcKind.Flavor),
                "Seis meses atrás eu estava no seu lugar. Hoje sou Dev Frontend Jr!",
                "Dica: começar pelo nível Iniciante de várias áreas ajuda a descobrir do que você gosta.");
        }

        // ───────────────────────── áreas ─────────────────────────
        void BuildDocker()
        {
            const int R = 1;
            F("labtable", 18, 21, 4, 1, 0xE6EDF3); F("labtable", 18, 24, 4, 1, 0xE6EDF3);
            Terminal(R, 26, 20);
            F("crate", 17, 27, 3, 1, 0xE76F51); F("crate", 21, 27, 3, 1, 0x2496ED);
            F("crate", 26, 25, 3, 1, 0x2A9D8F); F("crate", 26, 27, 3, 1, 0x8E6CC4);
            F("plant", 16, 18); F("plant", 29, 18);
            WallDecor("whiteboard", 24, 17, 3, 0, "docker run");
            It("board", 25.5f, 18.7f, R, "Quadro branco", "docker run [OPÇÕES] IMAGEM [COMANDO] — as opções vêm SEMPRE antes da imagem.");
            WallDecor("poster", 28, 17, 1, 0x2A9D8F);
            N("Rafael", "Líder de DevOps", 28.6f, 21.3f, 0, L(Story.Sk3, Story.HBlack, 0, 0x264653, 0x2B2D42), R, NpcKind.Mentor);
            Helper(R, 0, N("Lia", "Analista de QA", 20f, 22.7f, 3, L(Story.Sk2, Story.HRed, 1, 0x8ECAE6, 0x023047, true), R, NpcKind.Helper),
                "Testando, testando...", "Pelo que eu testei aqui: ", "Container que não sobe? Primeira coisa: docker logs.");
            Helper(R, 1, N("Jorge", "SRE", 24.6f, 26.1f, 1, L(Story.Sk5, Story.HBlack, 3, 0x6A4C93, 0x1B263B), R, NpcKind.Helper),
                "Opa, beleza?", "Faz assim: ", "Eu rodava tudo direto na máquina. Nunca mais.");
        }

        void BuildNetwork()
        {
            const int R = 2;
            for (int x = 32; x <= 36; x++) { F("rack", x, 21, 1, 1, 0x1C2230, x * 7); F("rack", x, 25, 1, 1, 0x1C2230, x * 11); }
            Terminal(R, 41, 20);
            F("ac", 43, 25, 1, 2, 0xE9EEF3);
            F("plant", 31, 28); F("plant", 44, 28);
            WallDecor("screen", 39, 17, 5);
            It("board", 41.5f, 18.7f, R, "Mapa da rede", "PORTAS FAMOSAS: 22 SSH · 53 DNS · 80 HTTP · 443 HTTPS · 3306 MySQL · 5432 PostgreSQL. MÁSCARAS: /24 = 255.255.255.0 · /16 = 255.255.0.0 · /8 = 255.0.0.0.");
            N("Sandra", "Líder de Infra e Redes", 43.6f, 21.3f, 0, L(Story.Sk3, Story.HGray, 2, 0x4361EE, 0x1B263B, true), R, NpcKind.Mentor);
            Helper(R, 0, N("Beto", "Analista de Redes", 34f, 23.8f, 3, L(Story.Sk2, Story.HBrown, 5, 0xFB8500, 0x023047), R, NpcKind.Helper),
                "Opa! Cuidado com o cabo azul.", "Em redes a regra é: ", "Já perdi um dia inteiro por causa de um cabo mal encaixado.").Look.Accent = Hx(0x219EBC);
            Helper(R, 1, N("Clara", "Segurança de Redes", 38.5f, 27f, 1, L(Story.Sk4, Story.HRed, 1, 0x8338EC, 0x2B2D42), R, NpcKind.Helper),
                "Olá! Firewall em dia?", "Pensa assim: ", "Bloqueie tudo e libere só o necessário.");
        }

        void BuildPython()
        {
            const int R = 3;
            F("desk", 47, 21, 3, 1, 0x5D6B82, 2); F("desk", 47, 24, 3, 1, 0x5D6B82, 2); F("desk", 51, 24, 3, 1, 0x5D6B82, 2);
            Terminal(R, 56, 20);
            F("sofa", 55, 27, 3, 1, 0x3D405B);
            F("plant", 46, 28); F("plant", 59, 28); F("shelf", 59, 23, 1, 2, 0x4A4E69);
            WallDecor("dashboard", 54, 17, 5);
            It("board", 56.5f, 18.7f, R, "Dashboard de vendas", "PYTHON NO DIA A DIA: listas [1, 2], dicionários {\"a\": 1}, funções def, pandas para tabelas (read_csv, groupby) e Flask para APIs.");
            N("Yara", "Cientista de Dados", 58.6f, 21.3f, 0, L(Story.Sk5, Story.HBlack, 1, 0x3776AB, 0x1B263B, true), R, NpcKind.Mentor);
            Helper(R, 0, N("Otávio", "Dev Python", 48.5f, 22.7f, 3, L(Story.Sk4, Story.HGray, 0, 0x588157, 0x344E41, true), R, NpcKind.Helper),
                "Hum? Ah, oi.", "Em Python é assim: ", "Indentação não é enfeite em Python: é sintaxe!");
            Helper(R, 1, N("Dani", "Engenheira de Dados", 52.5f, 25.7f, 3, L(Story.Sk3, Story.HBlue, 0, 0xFFD43B, 0x1D3557, false, true), R, NpcKind.Helper),
                "Dados, dados, dados!", "Do jeito que eu faço: ", "90% de ciência de dados é limpar dados. Sério.");
        }

        void BuildPhp()
        {
            const int R = 4;
            F("desk", 62, 21, 2, 1, 0xB0805A, 1); F("desk", 65, 21, 2, 1, 0xB0805A, 1);
            F("desk", 62, 24, 2, 1, 0xB0805A, 1); F("desk", 65, 24, 2, 1, 0xB0805A, 1);
            Terminal(R, 71, 20);
            F("beanbag", 62, 27, 1, 1, 0x777BB4); F("beanbag", 64, 27, 1, 1, 0xF4A261);
            F("plant", 74, 28); F("shelf", 74, 23, 1, 2, 0x6B4A2F);
            WallDecor("kanban", 69, 17, 4);
            It("board", 71f, 18.7f, R, "Kanban do backend", "BOAS PRÁTICAS PHP: variáveis com $, instruções terminam com ;, SQL sempre com prepared statements e senhas com password_hash.");
            N("Igor", "Líder de Backend PHP", 73.6f, 21.3f, 0, L(Story.Sk5, Story.HBlack, 3, 0x777BB4, 0x14213D), R, NpcKind.Mentor);
            Helper(R, 0, N("Fê", "Dev PHP", 63f, 22.6f, 3, L(Story.Sk2, Story.HBlond, 1, 0xEF476F, 0x26547C), R, NpcKind.Helper),
                "Oi! PHP é amor.", "Em PHP: ", "Nunca confie em dados do usuário. Valide tudo!");
            Helper(R, 1, N("Rui", "DBA", 66f, 25.6f, 3, L(Story.Sk3, Story.HBrown, 4, 0x118AB2, 0x073B4C), R, NpcKind.Helper),
                "Banco de dados em ordem.", "O DBA recomenda: ", "SQL concatenado com texto do usuário? Isso é SQL Injection esperando acontecer.");
        }

        void BuildFrontend()
        {
            const int R = 5;
            WallDecor("window", 2, 0, 2);
            WallDecor("whiteboard", 5, 0, 4, 0, "<html>");
            It("board", 7f, 1.7f, R, "Quadro de wireframes", "HTML estrutura, CSS estiliza, JS dá vida. Lembre: toda imagem precisa de alt, e o site precisa funcionar no celular.");
            WallDecor("poster", 11, 0, 1, 0xE34F26);
            F("desk", 2, 3, 2, 1, 0xF1F5F9, 1); F("desk", 5, 3, 2, 1, 0xF1F5F9, 1);
            Terminal(R, 11, 7);
            F("beanbag", 2, 8, 1, 1, 0xE63946); F("beanbag", 4, 9, 1, 1, 0xF4A261);
            F("plant", 1, 11); F("plant", 14, 11); F("shelf", 14, 3, 1, 2, 0x6B4A2F);
            N("Camila", "Líder de Frontend", 13.6f, 8.3f, 0, L(Story.Sk1, Story.HBrown, 1, 0xE34F26, 0x2B2D42), R, NpcKind.Mentor);
            Helper(R, 0, N("Yuki", "Dev Frontend", 2.6f, 4.7f, 3, L(Story.Sk1, Story.HBlack, 1, 0xFF70A6, 0x3D405B), R, NpcKind.Helper),
                "Oiê!", "No front eu lembro assim: ", "CSS é fácil de aprender e difícil de dominar. Flexbox salvou minha vida.");
            Helper(R, 1, N("Nina", "Especialista em Acessibilidade", 7.5f, 8.6f, 2, L(Story.Sk5, Story.HBlack, 4, 0x06D6A0, 0x073B4C), R, NpcKind.Helper),
                "Olá! Acessível para todos?", "Anota aí: ", "Teste seu site só com o teclado. Se não der para usar, tem problema!");
        }

        void BuildReact()
        {
            const int R = 6;
            WallDecor("whiteboard", 18, 0, 4, 0, "React");
            It("board", 20f, 1.7f, R, "Lousa do React", "REACT: componentes são funções que retornam JSX. Props entram, estado (useState) muda, efeitos (useEffect) rodam depois de renderizar.");
            WallDecor("window", 24, 0, 2);
            WallDecor("poster", 27, 0, 1, 0x149ECA);
            F("table", 18, 4, 5, 2, 0x9C6644);
            Terminal(R, 26, 7);
            F("plant", 16, 1); F("plant", 29, 1); F("plant", 16, 11); F("sofa", 18, 9, 3, 1, 0x6D597A);
            N("Thiago", "Líder do Squad React", 28.6f, 8.3f, 0, L(Story.Sk2, Story.HBlack, 5, 0x1D3557, 0x2B2D42), R, NpcKind.Mentor).Look.Accent = Hx(0x149ECA);
            Helper(R, 0, N("Caio", "Dev React", 20.5f, 7.1f, 3, L(Story.Sk3, Story.HBrown, 0, 0x149ECA, 0x1B263B), R, NpcKind.Helper),
                "Fala!", "Em React: ", "Componentes pequenos são mais fáceis de testar e reutilizar.");
            Helper(R, 1, N("Mari", "Dev React", 23.5f, 9.3f, 1, L(Story.Sk4, Story.HPink, 2, 0x2B2D42, 0x111827, true), R, NpcKind.Helper),
                "Oi! Hooks?", "Dica de hooks: ", "Nunca chame hooks dentro de if ou for.");
        }

        void BuildFlutter()
        {
            const int R = 7;
            WallDecor("screen", 33, 0, 4);
            It("board", 35f, 1.7f, R, "Painel de testes", "FLUTTER: tudo é widget. Text, Column, Row, Padding, botões... Estado muda com setState. Pacotes vêm do pub.dev.");
            WallDecor("window", 39, 0, 2);
            F("desk", 32, 3, 2, 1, 0xDDE3EA, 1); F("desk", 35, 3, 2, 1, 0xDDE3EA, 1); F("desk", 38, 3, 2, 1, 0xDDE3EA, 1);
            Terminal(R, 41, 7);
            F("beanbag", 32, 9, 1, 1, 0x02569B); F("sofa", 34, 9, 3, 1, 0x3D405B);
            F("plant", 31, 1); F("plant", 44, 1);
            N("Bruno", "Líder de Mobile", 43.6f, 8.3f, 0, L(Story.Sk2, Story.HBlond, 0, 0x02569B, 0x111827), R, NpcKind.Mentor);
            Helper(R, 0, N("Kenji", "Dev Flutter", 33f, 4.6f, 3, L(Story.Sk1, Story.HBlack, 0, 0x13B9FD, 0x1B263B, true), R, NpcKind.Helper),
                "Yo!", "No Flutter: ", "Hot reload é a melhor invenção da humanidade.");
            Helper(R, 1, N("Paula", "QA Mobile", 39.5f, 8.6f, 1, L(Story.Sk3, Story.HRed, 1, 0xF72585, 0x3D405B), R, NpcKind.Helper),
                "Testei em 37 celulares hoje.", "Do que eu vejo nos testes: ", "Tela pequena, internet lenta, bateria fraca: teste em tudo isso!");
        }

        void BuildAws()
        {
            const int R = 8;
            WallDecor("dashboard", 49, 0, 6);
            It("board", 52f, 1.7f, R, "Painel da nuvem", "AWS NA PRÁTICA: S3 guarda arquivos, EC2 são servidores, IAM controla quem pode o quê, Lambda roda funções, ECS roda containers. Segurança e custo em primeiro lugar!");
            for (int x = 47; x <= 51; x++) { F("rack", x, 3, 1, 1, 0x1C2230, x * 5); F("rack", x, 6, 1, 1, 0x1C2230, x * 3); }
            Terminal(R, 56, 7);
            F("crate", 54, 3, 3, 1, 0xFF9900);
            F("plant", 59, 1); F("plant", 46, 11);
            N("Helena", "Arquiteta de Nuvem", 58.6f, 8.3f, 0, L(Story.Sk4, Story.HGray, 2, 0x111827, 0x111827, true), R, NpcKind.Mentor).Look.Accent = Hx(0xFF9900);
            Helper(R, 0, N("Rodrigo", "Cloud Engineer", 49f, 4.6f, 3, L(Story.Sk2, Story.HBrown, 3, 0xFF9900, 0x232F3E), R, NpcKind.Helper),
                "Nuvem limpa, céu azul.", "Na AWS: ", "Esqueceu servidor ligado? A fatura não esquece.");
            Helper(R, 1, N("Ana", "FinOps", 53.5f, 8.6f, 1, L(Story.Sk3, Story.HBlack, 1, 0x2A9D8F, 0x264653), R, NpcKind.Helper),
                "Cada centavo conta!", "Pensando em custo e segurança: ", "Tag em tudo: sem tag, ninguém sabe de quem é o recurso.");
        }

        void BuildSql()
        {
            const int R = Story.SqlRoom;   // sala x 46..59, y 47..57, porta no alto (vinda do corredor sul)
            for (int x = 47; x <= 51; x++) { F("rack", x, 50, 1, 1, 0x1C2230, x * 13); F("rack", x, 54, 1, 1, 0x1C2230, x * 17); }
            Terminal(R, 56, 49);
            F("desk", 53, 53, 3, 1, 0x5D6B82, 2);
            F("plant", 46, 57); F("plant", 59, 57); F("shelf", 59, 52, 1, 2, 0x4A4E69); F("cooler", 46, 47);
            It("cooler", 46.5f, 48.4f, R, "Bebedouro", "Água gelada no Centro de Dados. Servidor gosta de frio, DBA gosta de água!");
            WallDecor("whiteboard", 55, 46, 4, 0, "SELECT");
            It("board", 57f, 47.7f, R, "Quadro do DBA", "SQL NA PRÁTICA: SELECT lê · INSERT cria · UPDATE altera · DELETE apaga. UPDATE e DELETE SEMPRE com WHERE! JOIN junta tabelas pela chave: pedidos.cliente_id = clientes.id.");
            N("Glória", "Líder de Dados e DBA", 58.6f, 50.3f, 0, L(Story.Sk5, Story.HGray, 4, 0x00838F, 0x1B263B, true), R, NpcKind.Mentor);
            Helper(R, 0, N("Fábio", "Analista de BI", 49f, 52.2f, 3, L(Story.Sk2, Story.HBrown, 0, 0x4DB6AC, 0x263238, true), R, NpcKind.Helper),
                "Dados contam histórias!", "Em SQL, eu faria assim: ", "Um bom relatório começa com uma boa pergunta — e um bom SELECT.");
            Helper(R, 1, N("Tânia", "DBA", 54.5f, 54.6f, 1, L(Story.Sk3, Story.HBlack, 1, 0x37474F, 0x212121), R, NpcKind.Helper),
                "Backup em dia?", "Conselho de DBA: ", "Antes de um UPDATE, faço um SELECT com o mesmo WHERE. Sempre.");
        }

        void BuildDirectors()
        {
            const int R = 9;
            WallDecor("window", 63, 0, 2); WallDecor("sign", 66, 0, 3, 0x7A2E3B, "Diretoria"); WallDecor("window", 70, 0, 2);
            Rug(63, 3.5f, 9, 6, 0x7A2E3B);
            F("bigdesk", 65, 5, 4, 1, 0x5B3A29);
            F("shelf", 61, 2, 1, 3, 0x5B3A29); F("shelf", 74, 2, 1, 3, 0x5B3A29);
            F("sofa", 66, 9, 3, 1, 0x3D405B);
            F("plant", 61, 11); F("plant", 74, 11); F("plant", 61, 1); F("plant", 74, 1);
            var ceo = N("Valéria", "CEO da Micorosoft", 67f, 4.4f, 0, L(Story.Sk4, Story.HBlack, 2, 0x7A2E3B, 0x111827, true), R, NpcKind.Flavor);
            ceo.Ceo = true;
            Mentors[R] = ceo;
            Flavor(N("Vítor", "CISO · Segurança", 72f, 8f, 1, L(Story.Sk1, Story.HBlack, 0, 0x343A40, 0x212529, true), R, NpcKind.Flavor),
                "Segurança não é produto, é processo.",
                "Nunca coloque senha direto no código. Variáveis de ambiente e cofres de segredos existem por um motivo.",
                "Viu algo estranho? Reporte. Ninguém é punido por avisar de boa-fé.");
        }

        void BuildLibrary()
        {
            const int R = Story.Library;
            Rug(5.3f, 30.2f, 5.4f, 10.6f, 0x7A2E3B);
            F("plant", 1, 30); F("plant", 14, 30); F("plant", 1, 40); F("plant", 14, 40);
            var spots = new[] { new[] { 2, 31 }, new[] { 2, 34 }, new[] { 2, 37 }, new[] { 12, 31 }, new[] { 12, 34 }, new[] { 12, 37 }, new[] { 2, 40 }, new[] { 5, 40 }, new[] { 11, 40 } };
            for (int i = 0; i < spots.Length && i < Challenges.Cats.Length; i++)
            {
                int sx = spots[i][0], sy = spots[i][1];
                F("shelf", sx, sy, 2, 1, 0x5B3A29);
                var cat = Challenges.Cats[i];
                Items.Add(new Item { Kind = "docs", X = sx + 1f, Y = sy + (sy == 40 ? -0.6f : 1.45f), Title = "Estante: " + cat.Name, Text = cat.Name, Chapter = R });
            }
            F("desk", 6, 34, 4, 1, 0x8D6E63, 2); F("desk", 6, 38, 4, 1, 0x8D6E63, 2);
            Items.Add(new Item { Kind = "docs", X = 8f, Y = 35.4f, Title = "Computador de pesquisa", Chapter = R });
            Items.Add(new Item { Kind = "docs", X = 8f, Y = 39.4f, Title = "Computador de pesquisa", Chapter = R });
            Flavor(N("Dona Célia", "Bibliotecária", 10.2f, 31.2f, 1, L(Story.Sk2, Story.HGray, 2, 0x6D597A, 0x2B2D42, true), R, NpcKind.Flavor),
                "Bem-vindo(a) à Biblioteca! Aqui tem documentação de todas as áreas da Micorosoft — e pesquisar não custa nenhum ponto.",
                "Cada estante é de uma tecnologia. Os computadores do centro pesquisam em tudo.",
                "Os exemplos usam outros nomes de propósito: entenda a sintaxe e adapte para o seu desafio.");
            Flavor(N("Hugo", "Estagiário de Dados", 4.6f, 36.0f, 2, L(Story.Sk5, Story.HBlack, 0, 0x3A86FF, 0x1D3557, true), R, NpcKind.Flavor),
                "Eu sempre começo pela documentação.",
                "Pesquisar é de graça, dica custa pontos. Faço as contas e venho sempre para cá!");
        }

        // ───────────────────────── atividades das salas sem desafios ─────────────────────────
        void BuildActivities()
        {
            var acts = new Dictionary<string, string>
            {
                { "Dra. Ada", "talk" }, { "Kátia", "phishing" }, { "Nando", "ticket" }, { "Jéssica", "ticket" }, { "Bel", "bugs" },
                { "Dr. Lúcio", "ia" }, { "Seu Raimundo", "garden" }, { "Sofia", "network" }, { "Henrique", "network" }, { "Renata", "network" },
                { "Dona Lurdes", "meal" }
            };
            foreach (var n in Npcs) { string a; if (acts.TryGetValue(n.Name, out a)) n.Activity = a; }
            // jardim: árvores para regar e lugares de pausa
            Items.Add(new Item { Kind = "activity", X = 78.5f, Y = 32.4f, Chapter = Story.Garden, Title = "Árvore do jardim", Text = "water:0" });
            Items.Add(new Item { Kind = "activity", X = 100.5f, Y = 32.4f, Chapter = Story.Garden, Title = "Árvore do jardim", Text = "water:1" });
            Items.Add(new Item { Kind = "activity", X = 102.5f, Y = 38.4f, Chapter = Story.Garden, Title = "Árvore do jardim", Text = "water:2" });
            Items.Add(new Item { Kind = "activity", X = 85.5f, Y = 38.4f, Chapter = Story.Garden, Title = "Banco do jardim", Text = "rest" });
            Items.Add(new Item { Kind = "activity", X = 89.5f, Y = 36.4f, Chapter = Story.Garden, Title = "Fonte", Text = "rest" });
            // sala de jogos e inovação
            Items.Add(new Item { Kind = "activity", X = 79.5f, Y = 6.4f, Chapter = Story.GameRoom, Title = "Mesa de pingue-pongue", Text = "pingpong" });
            Items.Add(new Item { Kind = "activity", X = 92.5f, Y = 21.4f, Chapter = Story.Innovation, Title = "Robô de entregas", Text = "robot" });
            Items.Add(new Item { Kind = "activity", X = 94.5f, Y = 21.4f, Chapter = Story.Innovation, Title = "Robô de entregas", Text = "robot" });
        }

        // ───────────────────────── ala leste e corredor sul ─────────────────────────
        void BuildEastAndSouthCorridors()
        {
            // continuação do corredor principal
            F("plant", 76, 13); F("coffee", 88, 13);
            It("coffee", 88.5f, 14.4f, -1, "Máquina de café", null);
            F("vending", 89, 13, 1, 1, 0x1D3557);
            It("vending", 89.5f, 14.4f, -1, "Máquina de lanches", null);
            F("sofa", 91, 13, 3, 1, 0x2A9D8F); F("plant", 95, 13); F("plant", 101, 13); F("bench", 102, 13, 3, 1, 0x7D8597); F("plant", 109, 13);
            F("plant", 76, 16); F("bench", 87, 16, 3, 1, 0x7D8597); F("plant", 92, 16); F("plant", 104, 16);
            WallDecor("poster", 79, 12, 1, 0xF72585);
            It("board", 79.5f, 13.8f, -1, "Pôster: bem-estar", "BEM-ESTAR NA MICOROSOFT: pausas curtas aumentam o foco. Beba água, alongue-se, almoce no Refeitório e dê uma volta no Jardim. Ninguém programa bem cansado.");
            WallDecor("poster", 100, 12, 1, 0x06D6A0);
            It("board", 100.5f, 13.8f, -1, "Pôster: mapa do campus", "MAPA DO CAMPUS: corredor de cima = áreas de tecnologia + Diretoria, Sala de Jogos e SOC. Corredor de baixo = Recepção, áreas, Help Desk e Inovação. Pelo corredor leste você chega ao corredor sul: Biblioteca, Refeitório, Auditório, Jardim e, descendo por ele, o Centro de Dados SQL.");

            // corredor leste
            F("plant", 109, 18); F("bench", 109, 21, 1, 3, 0x7D8597); F("cooler", 109, 26);
            It("cooler", 108.3f, 26.5f, -1, "Bebedouro", "Água geladinha no meio do caminho. Hidratação em dia!");
            F("plant", 106, 30); F("plant", 109, 33); F("bench", 109, 35, 1, 3, 0x7D8597); F("plant", 106, 39);

            // corredor sul
            F("plant", 1, 42); F("plant", 14, 42); F("bench", 18, 42, 3, 1, 0x7D8597); F("plant", 25, 42);
            F("cooler", 34, 42);
            It("cooler", 34.5f, 43.4f, -1, "Bebedouro", "Você bebeu água. O corredor sul é o caminho mais fresquinho da empresa!");
            F("plant", 44, 42); F("sofa", 48, 42, 3, 1, 0xC1666B); F("plant", 54, 42);
            F("vending", 66, 42, 1, 1, 0xE63946);
            It("vending", 66.5f, 43.4f, -1, "Máquina de lanches", null);
            F("plant", 74, 42); F("bench", 78, 42, 3, 1, 0x7D8597); F("plant", 84, 42); F("plant", 95, 42); F("bench", 98, 42, 3, 1, 0x7D8597); F("plant", 104, 42);
            F("plant", 1, 45); F("plant", 20, 45); F("plant", 40, 45); F("plant", 64, 45); F("plant", 86, 45); F("plant", 109, 45);
            WallDecor("poster", 38, 41, 1, 0xF4A261);
            It("board", 38.5f, 42.8f, -1, "Pôster: cardápio da semana", "CARDÁPIO: segunda, feijoada leve · terça, strogonoff · quarta, massa · quinta, frango grelhado · sexta, peixe. Todo dia tem salada e pão de queijo!");
            WallDecor("poster", 68, 41, 1, 0x8E6CC4);
            It("board", 68.5f, 42.8f, -1, "Pôster: agenda do auditório", "TECH TALKS: toda sexta no Auditório. Temas do mês: segurança no dia a dia, IA responsável, carreira em tecnologia e acessibilidade.");
            WallDecor("poster", 98, 41, 1, 0x2E7D4F);

            var tati = Flavor(N("Tati", "Analista de RH", 50, 43.6f, 0, L(Story.Sk3, Story.HBrown, 1, 0xFFB703, 0x3D405B), -1, NpcKind.Flavor),
                "Já conheceu o Jardim? É o melhor lugar para pensar num problema difícil.",
                "No Refeitório todo mundo conversa com todo mundo. Ótimo para fazer networking!");
            tati.Wander = true; tati.Range = 8;
            var gui = Flavor(N("Gui", "Facilities", 84, 44, 1, L(Story.Sk4, Story.HBlack, 0, 0x6C757D, 0x343A40, false, true), -1, NpcKind.Flavor),
                "Esse prédio tem mais de 40 salas e corredores. Eu conheço cada lâmpada!",
                "Se algo quebrar no prédio, abre um chamado no Help Desk, ala leste.");
            gui.Wander = true; gui.Range = 10;
            var mel = Flavor(N("Mel", "Dev Fullstack", 107.5f, 30, 2, L(Story.Sk1, Story.HRed, 1, 0x3A86FF, 0x1D3557), -1, NpcKind.Flavor),
                "Eu subo e desço esse corredor leste umas 30 vezes por dia. É meu cardio!",
                "Fullstack é isso: um pouco de tudo. Comece pelo que você gosta e vá ampliando.");
            mel.Wander = true; mel.Range = 6;
            Flavor(N("Otto", "Dev Júnior", 93, 15.2f, 1, L(Story.Sk2, Story.HBlond, 0, 0x8338EC, 0x2B2D42, true), -1, NpcKind.Flavor),
                "Fui efetivado mês passado! O segredo foi não ter vergonha de perguntar.",
                "A Sala de Jogos fica aqui em cima. Uma partida de pingue-pongue destrava qualquer bug.");
        }

        void BuildCafeteria()
        {
            const int R = Story.Cafeteria;
            F("buffet", 18, 30, 6, 1, 0xB8C0CC, 1);
            It("lunch", 21f, 31.4f, R, "Bandejão", null);
            F("buffet", 34, 30, 4, 1, 0xB8C0CC, 3);
            It("lunch", 36f, 31.4f, R, "Bandejão", null);
            F("coffee", 41, 30);
            It("coffee", 41.5f, 31.4f, R, "Máquina de café", null);
            F("cooler", 43, 30);
            It("cooler", 43.5f, 31.4f, R, "Bebedouro", "Suco natural no bebedouro do refeitório? Hoje é dia de limonada!");
            foreach (var tx in new[] { 17, 23, 32, 38 })
                foreach (var ty in new[] { 33, 36 })
                    F("lunchtable", tx, ty, 4, 1, 0xE8D5B5);
            F("plant", 16, 40); F("plant", 44, 40); F("plant", 26, 30); F("plant", 44, 33);
            WallDecor("board", 27, 29, 2, 0xB07D4F);
            It("board", 28f, 30.7f, R, "Cardápio do dia", "PRATO DO DIA: arroz, feijão, frango grelhado, salada e pudim. Opção vegana: grão-de-bico ao curry. Comer bem também é produtividade!");

            Flavor(N("Dona Lurdes", "Cozinheira", 25f, 31.4f, 1, L(Story.Sk4, Story.HGray, 2, 0xF1F5F9, 0x6C757D), R, NpcKind.Flavor),
                "Pega a bandeja, meu bem! Saco vazio não para em pé, e programador com fome escreve bug.",
                "Hoje tem pudim de sobremesa. Mas só depois da salada!",
                "Trinta anos de cozinha e eu nunca vi um time tão animado quanto esse da Micorosoft.");
            Flavor(N("Sofia", "Dev Backend", 19.5f, 34.5f, 3, L(Story.Sk2, Story.HBlack, 1, 0x777BB4, 0x14213D), R, NpcKind.Flavor),
                "Almoço com o time é sagrado. É aqui que as melhores ideias aparecem.",
                "Já fez os desafios de PHP? O Igor é exigente, mas ensina muito.");
            Flavor(N("Henrique", "Product Manager", 34f, 34.6f, 3, L(Story.Sk5, Story.HBlack, 3, 0x264653, 0x2B2D42), R, NpcKind.Flavor),
                "Como PM eu não programo, mas preciso entender de tudo um pouco para conversar com o time.",
                "Um bom estagiário pergunta 'por quê?' antes de perguntar 'como?'.");
            var lu = Flavor(N("Lu", "Designer de Produto", 30f, 38.6f, 0, L(Story.Sk1, Story.HPink, 2, 0xF72585, 0x3D405B, true), R, NpcKind.Flavor),
                "Design não é só bonito: é fácil de usar. E acessível!",
                "Hoje eu desenhei 12 telas do app. Agora mereço esse pão de queijo.");
            lu.Wander = true; lu.Range = 5;
        }

        void BuildAuditorium()
        {
            const int R = Story.Auditorium;
            F("stage", 52, 30, 12, 3, 0x6B4A2F);
            WallDecor("screen", 53, 29, 10);
            It("lesson", 62.5f, 33.6f, R, "Telão do Auditório", null);
            It("board", 50.5f, 33.6f, R, "Agenda de palestras", "TECH TALK DE HOJE: \"IA responsável: quando a tecnologia precisa de ética\". Próximas: segurança da informação, acessibilidade digital e como montar um portfólio.");
            foreach (var ty in new[] { 35, 37, 39 })
            {
                F("chairs", 48, ty, 10, 1, 0x8E2C48);
                F("chairs", 63, ty, 10, 1, 0x8E2C48);
            }
            F("plant", 46, 30); F("plant", 74, 30); F("plant", 46, 40); F("plant", 74, 40);
            var ada = Flavor(N("Dra. Ada", "Palestrante convidada", 58f, 33.8f, 0, L(Story.Sk5, Story.HGray, 4, 0x6A4C93, 0x1B263B, true), R, NpcKind.Flavor),
                "Bem-vindo(a) ao Tech Talk! Hoje o tema é IA responsável: tecnologia boa é a que respeita as pessoas.",
                "Pergunta para a plateia: quem aqui já leu os termos de uso de um app até o fim? Pois é...",
                "Dados pessoais não são 'só dados'. Por trás de cada linha do banco tem uma pessoa.");
            ada.Trivia = true;
            Flavor(N("Caco", "Estagiário de Dados", 60.5f, 36f, 2, L(Story.Sk3, Story.HBrown, 0, 0x2A9D8F, 0x264653), R, NpcKind.Flavor),
                "Eu anoto tudo das palestras. Já tenho um caderno inteiro!",
                "Dica: assistir palestra e depois explicar para alguém é o melhor jeito de aprender.");
            var rena = Flavor(N("Renata", "Tech Recruiter", 47f, 36f, 3, L(Story.Sk2, Story.HRed, 2, 0xE76F51, 0x3D405B), R, NpcKind.Flavor),
                "Recrutador olha portfólio e atitude. Badges e projetos contam muito!",
                "Numa entrevista, dizer 'não sei, mas eu pesquisaria assim...' é uma ótima resposta.");
            rena.Wander = true; rena.Range = 3;
        }

        void BuildGarden()
        {
            const int R = Story.Garden;
            F("tree", 78, 31, 1, 1, 0, 0); F("tree", 82, 34, 1, 1, 0, 1); F("tree", 100, 31, 1, 1, 0, 0);
            F("tree", 102, 37, 1, 1, 0, 1); F("tree", 79, 38, 1, 1, 0, 1); F("tree", 96, 39, 1, 1, 0, 0);
            F("fountain", 88, 33, 3, 3, 0xAEB9C7);
            F("bench", 84, 37, 3, 1, 0x8D6E63); F("bench", 93, 32, 3, 1, 0x8D6E63); F("bench", 96, 35, 3, 1, 0x8D6E63);
            F("plant", 76, 30); F("plant", 104, 30); F("plant", 104, 40); F("plant", 76, 35);
            WallDecor("poster", 86, 29, 1, 0x2E7D4F);
            It("board", 86.5f, 30.7f, R, "Placa do jardim", "PAUSA ATIVA: 5 minutos longe da tela a cada hora ajudam os olhos, a postura e a cabeça. Respire, alongue e volte com a ideia nova.");
            var rai = Flavor(N("Seu Raimundo", "Jardineiro", 92f, 37.5f, 1, L(Story.Sk4, Story.HGray, 3, 0x588157, 0x5A3A22), R, NpcKind.Flavor),
                "Planta é igual a código: precisa de cuidado todo dia, senão vira mato.",
                "Essa fonte aqui foi ideia da CEO. Barulhinho de água acalma qualquer deploy de sexta.");
            rai.Wander = true; rai.Range = 6;
            Flavor(N("Lara", "Dev Mobile", 94.5f, 33.6f, 0, L(Story.Sk3, Story.HBlack, 1, 0x02569B, 0x111827), R, NpcKind.Flavor),
                "Quando travo num bug, venho aqui e penso no problema longe da tela. Funciona!",
                "Saúde mental também é coisa séria. Se estiver sobrecarregado(a), fale com alguém.");
            Flavor(N("Diego", "UX Researcher", 85.5f, 38.6f, 0, L(Story.Sk2, Story.HBrown, 5, 0xFFB703, 0x2B2D42), R, NpcKind.Flavor),
                "Eu entrevisto usuários do Café 365. Tem gente que usa o app com leitor de tela: acessibilidade importa!",
                "A melhor pesquisa começa com uma boa pergunta.");
        }

        void BuildGameRoom()
        {
            const int R = Story.GameRoom;
            Rug(77f, 3f, 6f, 5f, 0x3A86FF);
            F("pingpong", 78, 4, 3, 2);
            F("arcade", 86, 1, 1, 1, 0x8E2C48, 1);
            It("arcade", 86.5f, 2.4f, R, "Fliperama", null);
            F("arcade", 88, 1, 1, 1, 0x1D3557, 2);
            It("arcade", 88.5f, 2.4f, R, "Fliperama", null);
            F("beanbag", 77, 9, 1, 1, 0xF72585); F("beanbag", 79, 10, 1, 1, 0x06D6A0);
            F("sofa", 86, 8, 3, 1, 0x6D597A); F("plant", 76, 1); F("plant", 89, 10);
            WallDecor("poster", 80, 0, 1, 0xF72585); WallDecor("window", 83, 0, 2);
            var vini = Flavor(N("Vini", "Dev Frontend", 80f, 6.6f, 3, L(Story.Sk2, Story.HBlack, 5, 0xE34F26, 0x2B2D42), R, NpcKind.Flavor),
                "Pingue-pongue rápido? Perdeu, paga o café!",
                "Descansar faz parte. Mas não esquece de voltar pro trabalho, hein?");
            vini.Wander = true; vini.Range = 2;
            Flavor(N("Bel", "QA", 87.5f, 4f, 1, L(Story.Sk1, Story.HBlond, 1, 0x06D6A0, 0x073B4C), R, NpcKind.Flavor),
                "Meu recorde no fliperama é 999.999. O jogo tem um bug de overflow... já reportei!",
                "QA é isso: até jogando eu acho bug.");
        }

        void BuildSoc()
        {
            const int R = Story.Soc;
            F("desk", 92, 4, 3, 1, 0x2B2D42, 2); F("desk", 96, 4, 3, 1, 0x2B2D42, 2); F("desk", 100, 4, 3, 1, 0x2B2D42, 2);
            F("desk", 92, 7, 3, 1, 0x2B2D42, 2); F("desk", 101, 7, 3, 1, 0x2B2D42, 2);
            F("plant", 91, 10); F("plant", 104, 10);
            WallDecor("dashboard", 92, 0, 6); WallDecor("screen", 99, 0, 5);
            It("board", 95f, 1.7f, R, "Painel de ameaças", "SOC (Centro de Operações de Segurança): monitora 24 horas tentativas de invasão, phishing e vazamentos. Regra número 1: na dúvida, reporte. Ninguém é punido por avisar de boa-fé.");
            Flavor(N("Kátia", "Analista de SOC", 94f, 5.6f, 3, L(Story.Sk5, Story.HBlack, 4, 0x343A40, 0x212529, false, true), R, NpcKind.Flavor),
                "Hoje bloqueamos 312 e-mails de phishing. Um deles imitava a própria CEO!",
                "Link estranho? Não clica. Encaminha para o SOC.",
                "Senha forte + autenticação em dois fatores = 99% dos ataques barrados.");
            Flavor(N("Edu", "Pentester", 102f, 9f, 1, L(Story.Sk1, Story.HBlack, 0, 0x111827, 0x111827, true), R, NpcKind.Flavor),
                "Eu sou pago para invadir... os sistemas da própria empresa, com autorização por escrito. Isso é hacking ético.",
                "Invadir sem autorização é crime, mesmo 'só para testar'. Sempre peça permissão.");
        }

        void BuildHelpDesk()
        {
            const int R = Story.HelpDesk;
            F("labtable", 78, 21, 4, 1, 0xE6EDF3); F("labtable", 78, 24, 4, 1, 0xE6EDF3);
            F("desk", 85, 20, 3, 1, 0xB0805A, 1);
            F("shelf", 88, 22, 1, 3, 0x4A4E69);
            F("crate", 84, 26, 3, 1, 0x59606B); F("crate", 84, 24, 2, 1, 0x3A86FF);
            F("plant", 76, 28); F("plant", 89, 28);
            WallDecor("kanban", 85, 17, 3);
            It("board", 86.5f, 18.7f, R, "Fila de chamados", "CHAMADOS DE HOJE: #4521 mouse sem pilha · #4522 'a internet sumiu' (cabo solto) · #4523 senha expirada · #4524 café derramado no teclado (de novo).");
            Flavor(N("Jéssica", "Técnica de Hardware", 80f, 22.7f, 3, L(Story.Sk3, Story.HRed, 2, 0x457B9D, 0x1D3557, true), R, NpcKind.Flavor),
                "Já montou um computador? Processador, memória, disco... é tipo Lego caro.",
                "Antes de chamar o Help Desk: o cabo está conectado? Sério, 30% dos chamados são isso.");
            Flavor(N("Nando", "Suporte N1", 86.5f, 21.6f, 0, L(Story.Sk2, Story.HBrown, 0, 0x2A9D8F, 0x264653, false, true), R, NpcKind.Flavor),
                "Suporte é a porta de entrada de muita gente na TI. Eu comecei aqui e hoje estudo Redes!",
                "Paciência e empatia resolvem metade dos chamados. A outra metade é reiniciar.");
        }

        void BuildInnovation()
        {
            const int R = Story.Innovation;
            F("robot", 92, 20, 1, 1, 0, 1); F("robot", 94, 20, 1, 1, 0, 2); F("robot", 103, 25, 1, 1, 0, 3);
            for (int x = 100; x <= 103; x++) F("rack", x, 19, 1, 1, 0x1C2230, x * 9);
            F("table", 92, 23, 5, 2, 0x9C6644);
            F("plant", 91, 28); F("plant", 104, 28);
            WallDecor("whiteboard", 100, 17, 4, 0, "IA");
            It("board", 102f, 18.7f, R, "Lousa de IA", "IA RESPONSÁVEL: 1) Dados com consentimento. 2) Testar vieses. 3) Explicar decisões. 4) Humano no controle. Tecnologia poderosa exige responsabilidade.");
            Flavor(N("Dr. Lúcio", "Pesquisador de IA", 96f, 26.2f, 0, L(Story.Sk4, Story.HGray, 3, 0xF1F5F9, 0x2B2D42, true), R, NpcKind.Flavor),
                "IA não substitui quem aprende a usá-la bem. Quem programa com IA precisa entender o código que ela gera.",
                "Um modelo treinado com dados enviesados toma decisões enviesadas. Por isso testamos tudo.",
                "Python é a língua oficial da IA. Já fez os desafios do Laboratório de Dados?");
            Flavor(N("Rita", "Engenheira de Robótica", 93.5f, 21.8f, 3, L(Story.Sk5, Story.HBlack, 4, 0xFF9900, 0x232F3E), R, NpcKind.Flavor),
                "Esses robozinhos entregam café pelo prédio. Às vezes erram a sala... ainda estamos debugando.",
                "Robótica junta tudo: hardware, redes, código e muita paciência.");
        }
    }
}
