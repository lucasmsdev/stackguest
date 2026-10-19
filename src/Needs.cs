using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace DockerQuest
{
    // Fome e sede, moedas, sala do jogador e patinete.
    // Fome e sede caem com o tempo; abaixo de 30% a visão (e o texto dos desafios) começa a embaçar.
    // Comida e bebida são compradas com as moedas ganhas nos desafios.
    partial class GameView
    {
        const float HungerMinutes = 24, ThirstMinutes = 16;   // de 100% a 0% (andando normal)
        int lastHungerWarn = 100, lastThirstWarn = 100;
        string roomFloorSig;
        float sofaCooldown;

        public float Hunger { get { return gs.Prog.Hunger; } }
        public float Thirst { get { return gs.Prog.Thirst; } }

        // 0 = visão normal; 1 = muito embaçada (fome ou sede em 0%)
        public float NeedBlur
        {
            get
            {
                float lo = Math.Min(gs.Prog.Hunger, gs.Prog.Thirst);
                return Math.Max(0, Math.Min(1, (30 - lo) / 30f));
            }
        }

        bool Riding { get { return gs.Prog.Equipped("ride") == "patinete" && gs.Prog.Owned.Contains("patinete"); } }

        void UpdateNeeds(float dt)
        {
            bool active = System.Windows.Forms.Form.ActiveForm != null;
#if DEMO
            active |= ForceActive;
            if (FreezeNeeds) return;
#endif
            if (!active) return;   // jogo minimizado ou em segundo plano: ninguém sente fome
            if (sofaCooldown > 0) sofaCooldown -= dt;
            float k = PlayerRoom.NeedsDrain(gs.Prog);
            bool running = moving && (keys.Contains(System.Windows.Forms.Keys.ShiftKey) || ModifierKeys.HasFlag(System.Windows.Forms.Keys.Shift)) && !Riding;
            if (running) k *= 1.5f;
            gs.Prog.Hunger = Math.Max(0, gs.Prog.Hunger - dt * 100f / (HungerMinutes * 60) * k);
            gs.Prog.Thirst = Math.Max(0, gs.Prog.Thirst - dt * 100f / (ThirstMinutes * 60) * k);
            WarnNeed(gs.Prog.Hunger, ref lastHungerWarn, "fome", "Você está com fome! Coma algo no Refeitório (corredor sul) ou use um lanche (teclas 1–8).", "Muita fome: sua visão está embaçando. Vá ao Refeitório!", "Sem energia de tanta fome! Tudo fica embaçado e você anda mais devagar.");
            WarnNeed(gs.Prog.Thirst, ref lastThirstWarn, "sede", "Você está com sede! Beba água no bebedouro ou na máquina de café.", "Muita sede: sua visão está embaçando. Beba água!", "Desidratado(a)! Tudo fica embaçado e você anda mais devagar.");
        }

        void WarnNeed(float v, ref int last, string what, string m30, string m15, string m0)
        {
            int level = v <= 0 ? 0 : v < 15 ? 15 : v < 30 ? 30 : 100;
            if (level < last)
            {
                Sfx.Play("hint");
                Toast(Fmt(level == 0 ? m0 : level == 15 ? m15 : m30), level == 30 ? Art.Hx(0xF0B429) : Art.Hx(0xF85149));
            }
            last = level;
        }

        void Restore(float hunger, float thirst)
        {
            gs.Prog.Hunger = Math.Min(100, gs.Prog.Hunger + hunger);
            gs.Prog.Thirst = Math.Min(100, gs.Prog.Thirst + thirst);
            if (gs.Prog.Hunger >= 30) lastHungerWarn = 100;
            if (gs.Prog.Thirst >= 30) lastThirstWarn = 100;
            var parts = new List<string>();
            if (hunger > 0) parts.Add("fome " + (int)gs.Prog.Hunger + "%");
            if (thirst > 0) parts.Add("sede " + (int)gs.Prog.Thirst + "%");
            if (parts.Count > 0) Toast("Satisfeito(a): " + string.Join(" · ", parts.ToArray()), Art.Hx(0x3FB950));
        }

        // paga com as moedas do saldo; avisa se não der
        bool Pay(int price, string what)
        {
            if (gs.Prog.Wallet < price)
            {
                Sfx.Play("err");
                SayIcon(what, "coffee", "Moedas insuficientes: custa " + price + " e você tem " + Math.Max(0, gs.Prog.Wallet) + ". Resolva desafios nos terminais para ganhar moedas!");
                return false;
            }
            gs.Prog.Spent += price;
            gs.Prog.Purchases++;
            gs.Prog.Save();
            Sfx.Play("buy");
            return true;
        }

        // ── sala do jogador
        public void RefreshRoom()
        {
            PlayerRoom.Apply(World, gs.Prog);
            var floor = PlayerRoom.Active(gs.Prog, "piso");
            string sig = floor == null ? "cimento" : floor.Id;
            bool floorChanged = roomFloorSig != null && sig != roomFloorSig;
            roomFloorSig = sig;
            if (floorChanged)
            {
                // blocos de chão (visão de cima) e minimapa com o piso novo
                foreach (var key in floorChunks.Keys.ToList())
                {
                    int cx = key / 1000, cy = key % 1000;
                    if (cx * ChunkTiles > PlayerRoom.X1 || cx * ChunkTiles + ChunkTiles <= PlayerRoom.X0 || cy * ChunkTiles > PlayerRoom.Y1 || cy * ChunkTiles + ChunkTiles <= PlayerRoom.Y0) continue;
                    floorChunks[key].Dispose(); floorChunks.Remove(key); chunkLru.Remove(key);
                }
                mini.Dispose(); miniBig.Dispose();
                mini = Scene.RenderMini(World, miniTile);
                miniBig = Scene.RenderMini(World, miniTileBig);
                if (w3dIso != null) w3dIso.InvalidateFloor(PlayerRoom.X0, PlayerRoom.Y0, PlayerRoom.X1, PlayerRoom.Y1);
            }
            if (w3dIso != null) w3dIso.RebuildRoom(World.RoomFurniture);
            renderSerial = -1;
        }

        readonly Dictionary<string, Bitmap> roomIcons = new Dictionary<string, Bitmap>();

        // ícone de um item da sala para a lojinha: o próprio modelo 3D
        public Bitmap RoomIcon(string id, int size)
        {
            var it = PlayerRoom.Get(id);
            if (it == null) return null;
            Bitmap b;
            string key = id + "|" + size;
            if (roomIcons.TryGetValue(key, out b)) return b;
            if (it.Kind == "floor") b = FloorIcon(it, size);
            else if (w3dIso != null)
            {
                var f = new Furn { Kind = it.Group == "pc" ? "pc" : it.Kind, X = 0, Y = 0, W = it.W, H = it.H, C = Art.Hx(it.Color == 0 ? 0x8D6E63 : it.Color), Var = it.Var, Text = "0" };
                try { b = w3dIso.FurnIcon(f, size); } catch { b = null; }
            }
            if (b != null) roomIcons[key] = b;
            return b;
        }

        static Bitmap FloorIcon(RoomItem it, int size)
        {
            var b = new Bitmap(size, size);
            using (var g = Graphics.FromImage(b))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Color c = Art.Hx(it.Color);
                float cx = size / 2f, cy = size / 2f, hw = size * 0.44f, hh = hw / 2;
                var pts = new[] { new PointF(cx, cy - hh), new PointF(cx + hw, cy), new PointF(cx, cy + hh), new PointF(cx - hw, cy) };
                g.FillPolygon(Art.B(c), pts);
                g.FillPolygon(Art.B(Art.Dk(c, 0.25f)), new[] { pts[3], pts[2], new PointF(cx, cy + hh + size * 0.06f), new PointF(cx - hw, cy + size * 0.06f) });
                g.FillPolygon(Art.B(Art.Dk(c, 0.4f)), new[] { pts[2], pts[1], new PointF(cx + hw, cy + size * 0.06f), new PointF(cx, cy + hh + size * 0.06f) });
                var line = new Pen(Art.A(Art.Dk(c, 0.3f), 160), 1);
                for (int i = 1; i < 4; i++)
                {
                    float t = i / 4f;
                    if (it.Var != 2 && it.Var != 4) g.DrawLine(line, Lerp(pts[0], pts[3], t), Lerp(pts[1], pts[2], t));
                    if (it.Var == 0 || it.Var == 5) g.DrawLine(line, Lerp(pts[0], pts[1], t), Lerp(pts[3], pts[2], t));
                }
                if (it.Var == 4)
                {
                    var r = new Random(3);
                    for (int i = 0; i < 60; i++) { float u = (float)r.NextDouble(), v = (float)r.NextDouble(); var p = Lerp(Lerp(pts[0], pts[1], u), Lerp(pts[3], pts[2], u), v); g.DrawLine(new Pen(Art.Dk(c, 0.3f)), p.X, p.Y, p.X + 1, p.Y - 3); }
                }
                line.Dispose();
            }
            return b;
        }

        static PointF Lerp(PointF a, PointF b, float t) { return new PointF(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t); }

        // interações dentro da sala do jogador
        void RoomInteract(Item it)
        {
            switch (it.Text)
            {
                case "pc":
                    {
                        var pc = PlayerRoom.Active(gs.Prog, "pc");
                        var areas = Challenges.Cats.Where(c => gs.Prog.Met.Contains(c.Room)).ToList();
                        if (areas.Count == 0) { SayIcon(pc.Name, "board", "Você ainda não conhece nenhuma área. Fale com o líder de uma área primeiro; depois dá para abrir o terminal dela daqui, do seu computador."); return; }
                        var opts = areas.Take(8).Select(c => new KeyValuePair<string, Action>("Terminal de " + c.Name, () =>
                        {
                            keys.Clear();
                            Sfx.Play("open");
                            if (TerminalRequested != null) TerminalRequested(c.Room);
                        })).ToList();
                        opts.Add(new KeyValuePair<string, Action>("Agora não", null));
                        string bonus = PlayerRoom.XpBonus(gs.Prog) > 0 ? " Ele também te dá +" + (int)Math.Round(PlayerRoom.XpBonus(gs.Prog) * 100) + "% de XP em todos os desafios." : "";
                        pages.Enqueue(new DialogPage { Title = pc.Name, Icon = "board", Text = Fmt("Seu computador abre o terminal de qualquer área que você já conhece." + bonus + " Qual vai ser?"), Options = opts });
                        break;
                    }
                case "frigobar":
                    pages.Enqueue(new DialogPage
                    {
                        Title = "Frigobar", Icon = "water", Text = Fmt("Seu frigobar está abastecido. Fome " + (int)gs.Prog.Hunger + "% · sede " + (int)gs.Prog.Thirst + "% · saldo " + gs.Prog.Wallet.ToString("N0") + " moedas."),
                        Options = new List<KeyValuePair<string, Action>>
                        {
                            new KeyValuePair<string, Action>("Garrafa de água (4 moedas): sede +35", () => { if (Pay(4, "Frigobar")) { Sfx.Play("coffee"); Restore(0, 35); gs.Prog.Waters++; } }),
                            new KeyValuePair<string, Action>("Suco de caixinha (10 moedas): sede +45, fome +5", () => { if (Pay(10, "Frigobar")) { Sfx.Play("coffee"); Restore(5, 45); } }),
                            new KeyValuePair<string, Action>("Iogurte (12 moedas): fome +20", () => { if (Pay(12, "Frigobar")) { Sfx.Play("use"); Restore(20, 0); } }),
                            new KeyValuePair<string, Action>("Agora não", null),
                        }
                    });
                    break;
                case "cafe":
                    if (!Pay(6, "Máquina de café")) return;
                    gs.Prog.Coffees++;
                    Sfx.Play("coffee");
                    Restore(0, 15);
                    if (gs.Prog.Owned.Contains("cafeteira")) gs.SpeedBuff = Math.Max(gs.SpeedBuff, 45);
                    SayIcon("Máquina de café", "coffee", "Cafezinho da sua sala, por 6 moedas. Sede +15.");
                    break;
                case "arcade":
                    keys.Clear();
                    Sfx.Play("open");
                    if (MiniGameRequested != null) MiniGameRequested("invaders");
                    return;
                case "badges":
                    keys.Clear();
                    Sfx.Play("open");
                    if (BadgesRequested != null) BadgesRequested();
                    return;
                case "sofa":
                    if (sofaCooldown > 0) { SayIcon("Sofá", "board", "Você acabou de descansar. Bora voltar aos desafios!"); return; }
                    sofaCooldown = 90;
                    gs.SpeedBuff = Math.Max(gs.SpeedBuff, 60);
                    gs.Prog.AddStat("rests");
                    Sfx.Play("coffee");
                    SayIcon("Sofá", "board", "Você se jogou no sofá por uns minutinhos. Cabeça renovada! (+40% de velocidade por 60 s)");
                    break;
            }
        }

        string RoomVerb(Item it)
        {
            switch (it.Text)
            {
                case "pc": return "Usar o seu computador";
                case "frigobar": return "Abrir o frigobar";
                case "cafe": return "Tomar café (6 moedas)";
                case "arcade": return "Jogar Invasores de Bugs";
                case "sofa": return "Descansar no sofá";
                case "badges": return "Ver suas badges";
            }
            return it.Title;
        }

        void ToggleRide()
        {
            if (!gs.Prog.Owned.Contains("patinete")) { Toast("Você ainda não tem o patinete elétrico. Ele está na Lojinha (aba Utilidades).", Art.Hx(0x8B949E)); return; }
            if (Riding) gs.Prog.Equip.Remove("ride"); else gs.Prog.Equip["ride"] = "patinete";
            gs.Prog.Save();
            RefreshLook();
            Sfx.Play("use");
            Toast(Riding ? "Você subiu no patinete (+50% de velocidade). P para descer." : "Você desceu do patinete. P para subir de novo.", Art.Hx(0x2496ED));
        }

        // fome e sede no canto da tela
        void DrawNeeds(Graphics g, int vw, int vh, float bottom)
        {
            float w = S(250), h = S(66), x = S(14), y = bottom - h - S(8);
            int hv = (int)Math.Round(gs.Prog.Hunger), tv = (int)Math.Round(gs.Prog.Thirst);
            bool low = hv < 15 || tv < 15;
            float pulse = low ? ((int)(time * 3) % 2 == 0 ? 1f : 0f) : 1f;
            // só redesenha quando os números (ou o piscar) mudam
            CachedHud(g, "necessidades", hv + "|" + tv + "|" + pulse, x, y, w, h, gg => DrawNeedsCard(gg, x, y, w, h, pulse));
        }

        void DrawNeedsCard(Graphics g, float x, float y, float w, float h, float pulse)
        {
            Card(g, x, y, w, h);
            for (int i = 0; i < 2; i++)
            {
                float v = i == 0 ? gs.Prog.Hunger : gs.Prog.Thirst;
                float ry = y + S(10) + i * S(26);
                float ix = x + S(12);
                if (i == 0)
                {
                    // hambúrguer
                    Art.E(g, Art.Hx(0xE9A23B), ix, ry, S(18), S(9));
                    Art.R(g, Art.Hx(0x3FB950), ix, ry + S(8), S(18), S(3));
                    Art.RR(g, Art.Hx(0x8B4513), ix, ry + S(11), S(18), S(5), S(2));
                }
                else
                {
                    // gota
                    g.FillPolygon(Art.B(Art.Hx(0x58A6FF)), new[] { new PointF(ix + S(9), ry - S(1)), new PointF(ix + S(15), ry + S(9)), new PointF(ix + S(3), ry + S(9)) });
                    Art.E(g, Art.Hx(0x58A6FF), ix + S(3), ry + S(5), S(12), S(12));
                }
                g.DrawString(i == 0 ? "Fome" : "Sede", fSmall, Art.B(Art.Hx(0xC9D1D9)), ix + S(24), ry);
                float bx = ix + S(70), bw = w - S(70) - S(70), bh = S(10);
                Color c = v >= 50 ? Art.Hx(0x3FB950) : v >= 30 ? Art.Hx(0xF0B429) : Art.Hx(0xF85149);
                if (v < 15) c = Art.A(c, (int)(150 + 105 * pulse));
                Art.RR(g, Art.Hx(0x30363D), bx, ry + S(4), bw, bh, S(5));
                if (v > 0.5f) Art.RR(g, c, bx, ry + S(4), Math.Max(S(6), bw * v / 100f), bh, S(5));
                g.DrawString((int)Math.Round(v) + "%", fSmall, Art.B(v < 30 ? Art.Hx(0xFF7B72) : Art.Hx(0xC9D1D9)), bx + bw + S(8), ry);
            }
        }
    }
}
