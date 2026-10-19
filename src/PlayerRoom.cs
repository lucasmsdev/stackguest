using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DockerQuest
{
    // Móvel/decoração da sala do jogador (comprado com moedas na aba "Minha sala" da lojinha)
    class RoomItem
    {
        public string Id, Name, Desc, Group, Kind, Text;
        public int Price, Tier, Color, Var;
        public float X, Y, W, H;
        public bool Solid = true;
        public string Use;   // interação: "pc", "frigobar", "cafe", "arcade", "sofa"
    }

    // A sala do jogador: fica no corredor sul, ao lado do Centro de Dados SQL. Começa vazia (piso de cimento)
    // e vai sendo mobiliada. Itens do mesmo grupo (computador, mesa, cadeira, piso) substituem o anterior.
    static class PlayerRoom
    {
        public const int X0 = 31, Y0 = 47, X1 = 44, Y1 = 57;   // área interna (porta em x 37–38, vinda do corredor sul)

        public static readonly RoomItem[] Items =
        {
            // mesas (o computador precisa de uma mesa)
            new RoomItem { Id = "sala_mesa1", Group = "mesa", Tier = 1, Price = 80, Name = "Mesa simples", Desc = "Uma mesa de madeira básica. Já dá para trabalhar!", Kind = "mydesk", Var = 1, Color = 0xB08A5B, X = 40, Y = 47.2f, W = 3, H = 1 },
            new RoomItem { Id = "sala_mesa2", Group = "mesa", Tier = 2, Price = 300, Name = "Mesa grande de carvalho", Desc = "Mais espaço para o computador, o café e as anotações.", Kind = "mydesk", Var = 2, Color = 0x8B5A2B, X = 40, Y = 47.2f, W = 3, H = 1 },
            new RoomItem { Id = "sala_mesa3", Group = "mesa", Tier = 3, Price = 650, Name = "Mesa gamer com LED", Desc = "Mesa preta com fita de LED colorida por baixo.", Kind = "mydesk", Var = 3, Color = 0x23262E, X = 40, Y = 47.2f, W = 3, H = 1 },
            // computadores (vantagem: mais XP nos desafios)
            new RoomItem { Id = "sala_pc1", Group = "pc", Tier = 1, Price = 60, Name = "Notebook velho", Desc = "Esquenta, mas funciona. Na sua sala, ele abre o terminal de qualquer área que você já conhece.", Kind = "pc", Var = 1, X = 41, Y = 47.2f, W = 1, H = 1, Solid = false, Use = "pc" },
            new RoomItem { Id = "sala_pc2", Group = "pc", Tier = 2, Price = 250, Name = "PC com monitor", Desc = "Computador de mesa com um monitor. +3% de XP em todos os desafios.", Kind = "pc", Var = 2, X = 41, Y = 47.2f, W = 1, H = 1, Solid = false, Use = "pc" },
            new RoomItem { Id = "sala_pc3", Group = "pc", Tier = 3, Price = 600, Name = "Setup com dois monitores", Desc = "Código de um lado, documentação do outro. +6% de XP.", Kind = "pc", Var = 3, X = 41, Y = 47.2f, W = 1, H = 1, Solid = false, Use = "pc" },
            new RoomItem { Id = "sala_pc4", Group = "pc", Tier = 4, Price = 1200, Name = "Setup gamer RGB", Desc = "Três monitores e gabinete com luzes RGB. +10% de XP.", Kind = "pc", Var = 4, X = 41, Y = 47.2f, W = 1, H = 1, Solid = false, Use = "pc" },
            // cadeiras (vantagem: fome e sede caem mais devagar)
            new RoomItem { Id = "sala_cad1", Group = "cadeira", Tier = 1, Price = 40, Name = "Banquinho", Desc = "Sem encosto. Melhor que trabalhar de pé.", Kind = "stool", Color = 0x8B5E3C, X = 41.1f, Y = 48.3f, W = 0.8f, H = 0.8f },
            new RoomItem { Id = "sala_cad2", Group = "cadeira", Tier = 2, Price = 200, Name = "Cadeira de escritório", Desc = "Confortável: fome e sede caem 10% mais devagar.", Kind = "chair_office", Color = 0x2B2D42, X = 41.1f, Y = 48.3f, W = 0.8f, H = 0.8f },
            new RoomItem { Id = "sala_cad3", Group = "cadeira", Tier = 3, Price = 500, Name = "Cadeira gamer", Desc = "Ergonômica e estilosa: fome e sede caem 20% mais devagar.", Kind = "chair_gamer", Color = 0xD62828, X = 41.1f, Y = 48.3f, W = 0.8f, H = 0.8f },
            // pisos
            new RoomItem { Id = "sala_piso1", Group = "piso", Tier = 1, Price = 150, Name = "Carpete azul", Desc = "Troca o piso de cimento da sala por um carpete macio.", Kind = "floor", Var = 2, Color = 0x3B5B8C },
            new RoomItem { Id = "sala_piso2", Group = "piso", Tier = 2, Price = 250, Name = "Piso de madeira", Desc = "Tábuas de madeira clara, aconchegante.", Kind = "floor", Var = 1, Color = 0xC89B6D },
            new RoomItem { Id = "sala_piso3", Group = "piso", Tier = 3, Price = 300, Name = "Porcelanato branco", Desc = "Piso claro e brilhante, de escritório moderno.", Kind = "floor", Var = 0, Color = 0xEEF1F5 },
            new RoomItem { Id = "sala_piso4", Group = "piso", Tier = 4, Price = 350, Name = "Grama sintética", Desc = "Para quem sente falta do Jardim.", Kind = "floor", Var = 4, Color = 0x6BAF5F },
            // decoração (cada uma tem o seu lugar na sala)
            new RoomItem { Id = "sala_planta", Price = 30, Name = "Vaso de planta", Desc = "Um verdinho ao lado da porta.", Kind = "plant", X = 36, Y = 47.1f, W = 1, H = 1 },
            new RoomItem { Id = "sala_vaso", Price = 90, Name = "Árvore no vaso", Desc = "Uma árvore de interior no canto da sala.", Kind = "bigplant", X = 43.9f, Y = 56, W = 1, H = 1 },
            new RoomItem { Id = "sala_estante", Price = 150, Name = "Estante de livros", Desc = "Livros de programação (e alguns gibis).", Kind = "shelf", Color = 0x6B4A2F, X = 31, Y = 47.1f, W = 2, H = 1 },
            new RoomItem { Id = "sala_tv", Price = 450, Name = "TV grande", Desc = "Para assistir palestras de tecnologia (e campeonatos).", Kind = "tv", X = 33.2f, Y = 47.1f, W = 2.6f, H = 0.6f },
            new RoomItem { Id = "sala_neon", Price = 220, Name = "Letreiro neon", Desc = "Um </> brilhante na parede.", Kind = "neon", X = 33.3f, Y = 47.0f, W = 2.4f, H = 0.2f, Solid = false },
            new RoomItem { Id = "sala_quadro", Price = 120, Name = "Quadro na parede", Desc = "Uma pintura para decorar a parede.", Kind = "frame", X = 39.2f, Y = 47.0f, W = 0.8f, H = 0.2f, Solid = false },
            new RoomItem { Id = "sala_tapete", Price = 100, Name = "Tapete redondo", Desc = "Um tapete colorido no meio da sala.", Kind = "carpet", Color = 0xE76F51, X = 32, Y = 50, W = 4, H = 3, Solid = false },
            new RoomItem { Id = "sala_sofa", Price = 350, Name = "Sofá", Desc = "Na sua sala, ele serve para uma pausa rápida (+40% de velocidade por 60 s).", Kind = "sofa", Var = 7, Color = 0x2A9D8F, X = 32, Y = 51.3f, W = 3, H = 1, Use = "sofa" },
            new RoomItem { Id = "sala_pufe", Price = 90, Name = "Pufe", Desc = "Para sentar no chão com estilo.", Kind = "beanbag", Color = 0xF4A261, X = 35.4f, Y = 50.2f, W = 1, H = 1 },
            new RoomItem { Id = "sala_luminaria", Price = 80, Name = "Luminária de chão", Desc = "Luz amarelinha para os dias de foco.", Kind = "lamp", X = 31.1f, Y = 49.2f, W = 0.8f, H = 0.8f },
            new RoomItem { Id = "sala_frigobar", Price = 260, Name = "Frigobar", Desc = "Água e suco geladinhos na sua sala, mais baratos que no refeitório.", Kind = "fridge", X = 43.9f, Y = 50.2f, W = 1, H = 1, Use = "frigobar" },
            new RoomItem { Id = "sala_cafeteira", Price = 200, Name = "Máquina de café", Desc = "Cafezinho na sua sala (mata um pouco da sede).", Kind = "coffee", X = 43.9f, Y = 52.2f, W = 1, H = 1, Use = "cafe" },
            new RoomItem { Id = "sala_fliperama", Price = 900, Name = "Fliperama particular", Desc = "Invasores de Bugs na sua sala, sem fila!", Kind = "arcade", Color = 0x6A4C93, Var = 3, X = 43.9f, Y = 54.1f, W = 1, H = 1, Use = "arcade" },
            new RoomItem { Id = "sala_aquario", Price = 400, Name = "Aquário", Desc = "Peixinhos coloridos nadando. Muito relaxante.", Kind = "aquarium", X = 31.1f, Y = 55.6f, W = 2, H = 0.9f },
            new RoomItem { Id = "sala_trofeu", Price = 150, Name = "Troféu de ouro", Desc = "\"Melhor estagiário(a) do mês\". Merecido!", Kind = "trophy", X = 35.5f, Y = 56.4f, W = 0.8f, H = 0.8f },
            new RoomItem { Id = "sala_som", Price = 180, Name = "Caixas de som", Desc = "Para ouvir lo-fi enquanto programa.", Kind = "speaker", X = 38.6f, Y = 56.4f, W = 0.9f, H = 0.6f },
            new RoomItem { Id = "sala_robo", Price = 500, Name = "Robozinho assistente", Desc = "Um robô flutuante que fica de olho na sua sala.", Kind = "robot", Var = 5, X = 39.6f, Y = 53.5f, W = 1, H = 1, Solid = false },
        };

        public static RoomItem Get(string id) { return Items.FirstOrDefault(i => i.Id == id); }

        public static IEnumerable<ShopItem> ShopItems()
        {
            return Items.Select(i => new ShopItem { Id = i.Id, Slot = "sala", Price = i.Price, Name = i.Name, Desc = i.Desc });
        }

        // item do grupo em uso (o de maior nível comprado)
        public static RoomItem Active(Progress p, string group)
        {
            return Items.Where(i => i.Group == group && p.Owned.Contains(i.Id)).OrderByDescending(i => i.Tier).FirstOrDefault();
        }

        // o que já está na sala
        public static List<RoomItem> Placed(Progress p)
        {
            var list = new List<RoomItem>();
            foreach (var it in Items)
            {
                if (!p.Owned.Contains(it.Id)) continue;
                if (it.Group != null && Active(p, it.Group) != it) continue;
                list.Add(it);
            }
            return list;
        }

        public static float XpBonus(Progress p)
        {
            var pc = Active(p, "pc");
            return pc == null ? 0 : pc.Tier == 2 ? 0.03f : pc.Tier == 3 ? 0.06f : pc.Tier == 4 ? 0.10f : 0;
        }

        // cadeira boa: fome e sede caem mais devagar
        public static float NeedsDrain(Progress p)
        {
            var c = Active(p, "cadeira");
            return c == null ? 1f : c.Tier == 2 ? 0.9f : c.Tier == 3 ? 0.8f : 1f;
        }

        // por que não dá para comprar (ou null se dá)
        public static string BlockReason(Progress p, string id)
        {
            var it = Get(id);
            if (it == null) return null;
            if (it.Group == "pc" && Active(p, "mesa") == null) return "Compre uma mesa primeiro: o computador fica em cima dela.";
            if (it.Group != null)
            {
                var cur = Active(p, it.Group);
                if (cur != null && cur.Tier >= it.Tier) return cur == it ? null : "Você já tem algo melhor: " + cur.Name + ".";
            }
            return null;
        }

        // aplica a sala no mapa: piso, móveis (com colisão) e pontos de interação
        public static void Apply(World w, Progress p)
        {
            var sec = Story.Sectors[Story.PlayerRoom];
            var floor = Active(p, "piso");
            if (floor == null) { sec.Style = 5; sec.F1 = Art.Hx(0xB4B8BE); sec.F2 = Art.Hx(0xACB0B6); }
            else { sec.Style = floor.Var; sec.F1 = Art.Hx(floor.Color); sec.F2 = Art.Dk(Art.Hx(floor.Color), 0.06f); }

            for (int x = X0; x <= X1; x++) for (int y = Y0; y <= Y1; y++) w.Solid[x, y] = false;
            w.RoomFurniture.Clear();
            w.Items.RemoveAll(i => i.Chapter == Story.PlayerRoom && i.Kind != "board");
            var desk = Active(p, "mesa");
            var pc = Active(p, "pc");
            foreach (var it in Placed(p))
            {
                if (it.Kind == "floor" || it.Group == "pc") continue;
                var f = new Furn { Kind = it.Kind, X = it.X, Y = it.Y, W = it.W, H = it.H, C = Art.Hx(it.Color == 0 ? 0x8D6E63 : it.Color), Var = it.Var };
                if (it.Group == "mesa") f.Text = pc != null ? pc.Var.ToString() : "0";
                w.RoomFurniture.Add(f);
                if (it.Solid)
                    for (int tx = (int)Math.Floor(it.X); tx < (int)Math.Ceiling(it.X + it.W); tx++)
                        for (int ty = (int)Math.Floor(it.Y); ty < (int)Math.Ceiling(it.Y + it.H); ty++)
                            if (w.InMap(tx, ty)) w.Solid[tx, ty] = true;
                if (it.Use != null && it.Use != "pc")
                    w.Items.Add(new Item { Kind = "sala", Text = it.Use, X = it.X + it.W / 2, Y = it.Y + it.H + 0.25f, Chapter = Story.PlayerRoom, Title = it.Name });
            }
            if (pc != null && desk != null)
                w.Items.Add(new Item { Kind = "sala", Text = "pc", X = 41.5f, Y = 48.6f, Chapter = Story.PlayerRoom, Title = pc.Name });
            // parede da fama: cada badge ganha vira um quadro na parede oeste (de alvenaria)
            w.RoomBadges.Clear();
            foreach (var cat in Challenges.Cats)
                for (int t = 0; t < 3; t++)
                    if (p.BadgeDates.ContainsKey(cat.Id + ":" + t)) w.RoomBadges.Add(new KeyValuePair<string, int>(cat.Id, t));
            if (w.RoomBadges.Count > 0)
                w.Items.Add(new Item { Kind = "sala", Text = "badges", X = BadgeWallX + 0.6f, Y = 53.9f, Chapter = Story.PlayerRoom, Title = "Parede da fama" });
        }

        // parede da fama: uma coluna por área (na ordem das áreas) e uma fileira por nível (Profissional em cima)
        public const float BadgeWallX = 31f, BadgeSize = 0.64f;
        public static float BadgeY(int catIndex) { return 47.95f + catIndex * 1.2f; }
        public static float BadgeZ(int tier) { return 0.28f + tier * 0.68f; }
    }
}
