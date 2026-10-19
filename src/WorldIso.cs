using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace DockerQuest
{
    // boneco de blocos (estilo Habbo) para desenhar na visão isométrica
    class Avatar3D { public Look Look; public float X, Y, Angle, Walk; public bool Moving; }

    // Visão isométrica em pixel (estilo Habbo): câmera ortográfica na diagonal, cena desenhada numa resolução baixa
    // e ampliada sem suavizar (pixels nítidos). Paredes do fundo de cada sala ficam altas; as da frente e as que
    // tapariam o jogador são cortadas. Os personagens são bonecos de blocos com rosto em pixel art.
    partial class World3D
    {
        public const float HI = 2.3f, LOWH = 0.3f, GH = 1.35f;   // GH: divisórias de vidro mais baixas (deixam ver as salas)
        const float IsoPxPerUnit = 22.627417f;   // 32 px de largura por tile em losango (proporção 2:1)


        float orthoPx, orthoCX, orthoCY, orthoHalfW, orthoHalfH;

        public class IsoWall { public int X, Y; public Color CS, CE; public bool OpenS, OpenE, Back; public float H = -1; public List<int> Segs = new List<int>(); }
        // porta numa parede de alvenaria: a moldura acompanha a altura da parede (é desenhada a cada quadro)
        class IsoDoorDyn { public int X0, Y, N; public float Ya, Yb, Gy, Pcx, Hx, Hy, Pw, Ph; public Region Plate; }
        readonly List<IsoDoorDyn> isoDoors = new List<IsoDoorDyn>();
        public class IsoDecor { public Region R; public float X0, X1, Y, Z0, Z1; public int WY, WX0, WX1; public bool OnGlass; }
        readonly Dictionary<int, IsoWall> isoWallAt = new Dictionary<int, IsoWall>();
        static readonly Color WallTopC = Art.Hx(0x485166), Exterior = Art.Hx(0xC3CAD4);

        // ───────────────────────── montagem ─────────────────────────

        Color IsoSide(int nx, int ny)
        {
            if (IsFloorT(nx, ny) || GlassT(nx, ny)) return SideColor(nx, ny);
            return Exterior;
        }

        bool SolidT(int x, int y) { return IsWallT(x, y) && !GlassT(x, y); }

        void IsoSolidWall(int x, int y)
        {
            var cell = C(x + 0.5f, y + 0.5f);
            var w = new IsoWall
            {
                X = x, Y = y,
                OpenS = !SolidT(x, y + 1), OpenE = !SolidT(x + 1, y),
                CS = IsoSide(x, y + 1), CE = IsoSide(x + 1, y),
                // parede do fundo de alguma sala (o chão fica na frente dela): pode ficar alta.
                // Encostar numa divisória de vidro não conta: senão o canto vira um pilar alto solto no meio das paredes baixas.
                Back = IsFloorT(x, y + 1) || IsFloorT(x + 1, y)
            };
            cell.Walls.Add(w);
            isoWallAt[x * 1000 + y] = w;
            float v0 = 1 - LOWH / HI;
            if (w.OpenS) Face(cell.O, rPlaster, new V3(x, y + 1, 0), new V3(x + 1, y + 1, 0), new V3(x + 1, y + 1, LOWH), new V3(x, y + 1, LOWH), w.CS, KS, 0, v0, 1, 1);
            if (w.OpenE) Face(cell.O, rPlaster, new V3(x + 1, y + 1, 0), new V3(x + 1, y, 0), new V3(x + 1, y, LOWH), new V3(x + 1, y + 1, LOWH), w.CE, KE, 0, v0, 1, 1);
        }

        void IsoGlassWall(int x, int y)
        {
            var cell = C(x + 0.5f, y + 0.5f);
            var conn = new List<Point>();
            foreach (var d in Dirs)
            {
                int nx = x + d.X, ny = y + d.Y;
                if (IsWallT(nx, ny) || (InMap(nx, ny) && W.Tile[nx, ny] == World.Door)) conn.Add(d);
            }
            if (conn.Count == 0) { conn.Add(new Point(1, 0)); conn.Add(new Point(-1, 0)); }
            else if (conn.Count == 1) conn.Add(new Point(-conn[0].X, -conn[0].Y));
            float cx = x + 0.5f, cy = y + 0.5f, t = 0.06f;
            var wc = Color.White;
            foreach (var d in conn)
            {
                float x0, x1, y0, y1;
                if (d.X != 0) { x0 = d.X > 0 ? cx : x; x1 = d.X > 0 ? x + 1 : cx; y0 = cy - t; y1 = cy + t; }
                else { y0 = d.Y > 0 ? cy : y; y1 = d.Y > 0 ? y + 1 : cy; x0 = cx - t; x1 = cx + t; }
                Box(cell.O, x0, y0, 0, x1, y1, 0.12f, Alu);
                Box(cell.O, x0, y0, GH - 0.06f, x1, y1, GH, Alu);
                if (d.X != 0) QuadC(cell.G, rGlass, new V3(x0, cy, 0.12f), new V3(x1, cy, 0.12f), new V3(x1, cy, GH - 0.06f), new V3(x0, cy, GH - 0.06f), wc, wc, wc, wc, (x0 - x) * 0.5f, 0, (x1 - x) * 0.5f, 1);
                else QuadC(cell.G, rGlass, new V3(cx, y0, 0.12f), new V3(cx, y1, 0.12f), new V3(cx, y1, GH - 0.06f), new V3(cx, y0, GH - 0.06f), wc, wc, wc, wc, (y0 - y) * 0.5f, 0, (y1 - y) * 0.5f, 1);
            }
            Box(cell.O, cx - 0.035f, cy - 0.07f, 0, cx + 0.035f, cy + 0.07f, GH, AluDk);
        }

        void IsoDoor(DoorInfo d)
        {
            int x0 = d.Tiles.Min(t => t.X), y = d.Tiles[0].Y, n = d.Tiles.Count;
            var cell = C(x0 + 1, y + 0.5f);
            float ya = y + 0.4f, yb = y + 0.6f;
            float DH = GlassT(x0 - 1, y) || GlassT(x0 + n, y) ? GH : HI;
            if (DH == HI)
            {
                // na alvenaria a parede pode ser cortada perto do jogador: a moldura acompanha (sem "pilares" soltos)
                var dpr = Scene.PlateRect(d);
                float dpw = dpr.Width / Scene.T * 0.62f;
                isoDoors.Add(new IsoDoorDyn
                {
                    X0 = x0, Y = y, N = n, Ya = ya, Yb = yb, Gy = y + 0.5f, Pcx = x0 + n / 2f,
                    Hx = 0.7071f * dpw / 2, Hy = -0.7071f * dpw / 2, Pw = dpw, Ph = dpr.Height / Scene.T * 0.62f / 0.866f,
                    Plate = PlateRegion(d, dpr, Scene.PlateLabel(d, false, false))
                });
                return;
            }
            Box(cell.O, x0, ya, 0, x0 + 0.08f, yb, DH, AluDk);
            Box(cell.O, x0 + n - 0.08f, ya, 0, x0 + n, yb, DH, AluDk);
            Box(cell.O, x0, ya, DH - 0.08f, x0 + n, yb, DH, Alu, null, null, false, true);
            var wc = Color.White;
            float gy = y + 0.5f;
            foreach (var side in new[] { 0, 1 })
            {
                float a = side == 0 ? x0 + 0.08f : x0 + n - 0.5f, b = side == 0 ? x0 + 0.5f : x0 + n - 0.08f;
                QuadC(cell.G, rGlass, new V3(a, gy, 0.02f), new V3(b, gy, 0.02f), new V3(b, gy, DH - 0.08f), new V3(a, gy, DH - 0.08f), wc, wc, wc, wc, 0, 0, 0.4f, 1);
            }
            // placa com o nome da área em cima da porta, virada para a câmera
            var pr = Scene.PlateRect(d);
            string label = Scene.PlateLabel(d, false, false);
            var reg = PlateRegion(d, pr, label);
            float pw = pr.Width / Scene.T * 0.62f, ph = pr.Height / Scene.T * 0.62f / 0.866f, pcx = x0 + n / 2f, z0 = DH + 0.06f;
            // a placa fica de frente para a câmera (perpendicular à diagonal da visão): o texto não sai torto
            float hx = 0.7071f * pw / 2, hy = -0.7071f * pw / 2;
            var white = Color.White;
            QuadC(cell.O, reg, new V3(pcx - hx, gy - hy, z0), new V3(pcx + hx, gy + hy, z0), new V3(pcx + hx, gy + hy, z0 + ph), new V3(pcx - hx, gy - hy, z0 + ph), white, white, white, white, 0, 0, 1, 1);
            Box(cell.O, pcx - 0.03f, gy - 0.03f, DH, pcx + 0.03f, gy + 0.03f, z0 + 0.02f, AluDk);
        }

        Region PlateRegion(DoorInfo d, RectangleF pr, string label)
        {
            var key = "plate|" + label + "|" + d.Chapter;
            Region reg;
            if (regions.TryGetValue(key, out reg)) return reg;
            const int sc = 6;
            var b = Bmp((int)(pr.Width * sc), (int)(pr.Height * sc));
            using (var g = Gfx(b))
            {
                g.ScaleTransform(sc, sc);
                g.TranslateTransform(-pr.X, -pr.Y);
                Scene.Plate(g, d, false, false);
                FitText(g, label, new RectangleF(pr.X + 4, pr.Y, pr.Width - 6, pr.Height), Color.White);
            }
            return Add(key, b, false);
        }

        void IsoDecorPiece(Furn d)
        {
            int wx = (int)Math.Floor(d.X + d.W / 2), wy = (int)d.Y;
            bool glass = GlassT(wx, wy);
            var rect = Scene.DecorRect(d, glass);
            string key = "decor|" + d.Kind + "|" + d.X + "|" + d.Y + "|" + d.W + "|" + glass;
            const int sc = 5;
            var b = Bmp((int)(rect.Width * sc), (int)(rect.Height * sc));
            using (var g = Gfx(b))
            {
                g.ScaleTransform(sc, sc);
                g.TranslateTransform(-rect.X, -rect.Y);
                Scene.Decor(g, d, glass);
                if (d.Kind == "sign") FitText(g, d.Text, new RectangleF(rect.X, rect.Y + 4, rect.Width, rect.Height - 10), Color.White);
                else if (d.Kind == "whiteboard" && d.Text != null) FitText(g, d.Text, new RectangleF(rect.X + 3, rect.Y + 3, rect.Width - 6, 9), Art.Hx(0x1D4ED8));
            }
            bool lit = d.Kind == "window" || d.Kind == "screen" || d.Kind == "dashboard";
            var reg = Add(key, b, lit);
            float h = rect.Height / Scene.T;
            var dec = new IsoDecor
            {
                R = reg, X0 = d.X + 3f / Scene.T, X1 = d.X + d.W - 3f / Scene.T,
                Y = glass ? wy + 0.5f + 0.09f : wy + 1 + 0.012f,
                Z0 = glass ? GH - 0.08f - h : 1.35f - h / 2, Z1 = glass ? GH - 0.08f : 1.35f + h / 2,
                WY = wy, WX0 = (int)Math.Floor(d.X), WX1 = (int)Math.Ceiling(d.X + d.W) - 1, OnGlass = glass
            };
            if (d.Kind == "window") { dec.Z0 = 0.75f; dec.Z1 = 2.0f; }
            C(d.X + d.W / 2, wy + 0.5f).Decor.Add(dec);
        }

        // carrega os blocos de chão (mais perto primeiro) até o tempo limite; devolve quanto já foi (0 a 1)
        public float PreloadStep(float px, float py, int ms)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var todo = cells.Cast<Cell>().Where(c => !floorTex.ContainsKey(c.CX * 100 + c.CY)).OrderBy(c => Dist2(c, px, py)).ToList();
            foreach (var c in todo)
            {
                if (sw.ElapsedMilliseconds >= ms) break;
                LoadFloor(c);
            }
            int total = CW * CH, done = cells.Cast<Cell>().Count(c => floorTex.ContainsKey(c.CX * 100 + c.CY));
            return done / (float)total;
        }

        // ───────────────────────── corte das paredes ─────────────────────────

        float cutX, cutY;

        // a parede está na faixa entre a câmera e o jogador?
        // Só corta a parede que fica de verdade entre o jogador e a câmera (que olha de sudeste).
        // Em coordenadas s = x + y (profundidade, cresce em direção à câmera) e q = x − y (lado), o quadrado
        // da parede é um losango |s − sc| + |q − qc| ≤ 1 e o boneco ocupa |q − qp| ≤ 0,5. A parede tapa o
        // jogador se alguma parte dela, na faixa lateral do boneco, estiver na frente dele (e perto o bastante:
        // uma parede de 2,3 só esconde o que está a até ~4 tiles atrás dela). Paredes ao lado ou atrás
        // (como a parede da fama da sua sala) continuam em pé.
        bool InCut(IsoWall w)
        {
            float sc = w.X + w.Y + 1, qc = w.X - w.Y;
            float sp = cutX + cutY, qp = cutX - cutY;
            float qb = Math.Max(qp - 0.5f, Math.Min(qp + 0.5f, qc));   // ponto da faixa do boneco mais perto do centro da parede
            float reach = 1 - Math.Abs(qb - qc);
            if (reach < 0) return false;                              // a parede não cruza a linha de visão do boneco
            float front = sc + reach - sp;                            // quanto a parte mais à frente passa do jogador
            return front > 0.2f && sc - 1 - sp < 7f;
        }

        // trechos retos de parede de fundo (de uma junção até a outra). O corte vale para o trecho INTEIRO:
        // assim não sobram pedaços altos soltos ("pilares") nem degraus na borda da faixa de corte.
        List<List<IsoWall>> wallSegs;
        bool[] segCut;

        void BuildWallSegs()
        {
            wallSegs = new List<List<IsoWall>>();
            foreach (var w in isoWallAt.Values) w.Segs.Clear();
            for (int pass = 0; pass < 2; pass++)
            {
                bool horiz = pass == 0;
                int outer = horiz ? World.H : World.W, inner = horiz ? World.W : World.H;
                for (int a = 0; a < outer; a++)
                {
                    var run = new List<IsoWall>();
                    for (int b = 0; b <= inner; b++)
                    {
                        int x = horiz ? b : a, y = horiz ? a : b;
                        IsoWall w = null;
                        if (b < inner) isoWallAt.TryGetValue(x * 1000 + y, out w);
                        if (w == null || !w.Back) { CloseSeg(run); run = new List<IsoWall>(); continue; }
                        run.Add(w);
                        // junção com uma parede perpendicular: termina este trecho e começa outro (a junção fica nos dois)
                        bool junction = horiz ? (SolidT(x, y - 1) || SolidT(x, y + 1)) : (SolidT(x - 1, y) || SolidT(x + 1, y));
                        if (junction && run.Count > 1) { CloseSeg(run); run = new List<IsoWall> { w }; }
                    }
                }
            }
            segCut = new bool[wallSegs.Count];
        }

        void CloseSeg(List<IsoWall> run)
        {
            if (run.Count < 2) return;
            foreach (var w in run) w.Segs.Add(wallSegs.Count);
            wallSegs.Add(run);
        }

        // parede alta, a não ser que esteja num trecho que tapa o jogador (ou seja só a frente de uma sala)
        bool Tall(IsoWall w)
        {
            if (!w.Back) return false;
            if (w.Segs.Count == 0 || segCut == null) return !InCut(w);
            foreach (int i in w.Segs) if (!segCut[i]) return true;
            return false;
        }

#if DEMO
        public string DebugWallInfo(int x, int y)
        {
            IsoWall w; string r = "(" + x + "," + y + ") tile=" + (InMap(x, y) ? W.Tile[x, y].ToString() : "fora") + " vidro=" + GlassT(x, y);
            if (isoWallAt.TryGetValue(x * 1000 + y, out w)) r += " parede back=" + w.Back + " h=" + w.H.ToString("0.00") + " segs=" + w.Segs.Count + " corte=" + InCut(w) + " d=" + ((w.X + w.Y + 1) - (cutX + cutY)).ToString("0.0") + " lat=" + ((w.X - w.Y) - (cutX - cutY)).ToString("0.0") + " cam=" + cutX.ToString("0.0") + "," + cutY.ToString("0.0");
            foreach (var f in W.Furniture) if (Math.Abs(f.X + f.W / 2 - (x + 0.5f)) < 1.2f && Math.Abs(f.Y + f.H / 2 - (y + 0.5f)) < 1.2f) r += " movel=" + f.Kind + "@" + f.X + "," + f.Y + " " + f.W + "x" + f.H;
            foreach (var d in W.AllDoors) foreach (var t in d.Tiles) if (Math.Abs(t.X - x) <= 1 && Math.Abs(t.Y - y) <= 1) r += " porta@" + t.X + "," + t.Y;
            return r;
        }
#endif
        float WallH(IsoWall w) { return w.H < 0 ? (Tall(w) ? HI : LOWH) : w.H; }
        float WallHAt(int x, int y) { IsoWall w; return isoWallAt.TryGetValue(x * 1000 + y, out w) ? WallH(w) : -1; }

        readonly System.Diagnostics.Stopwatch wallClock = System.Diagnostics.Stopwatch.StartNew();
        double lastWallT;

        // decide o corte e anima a altura das paredes (descem e sobem em ~0,2 s, sem "piscar")
        void UpdateWallHeights()
        {
            if (wallSegs == null) BuildWallSegs();
            for (int i = 0; i < wallSegs.Count; i++)
            {
                bool cut = false;
                foreach (var w in wallSegs[i]) if (InCut(w)) { cut = true; break; }
                segCut[i] = cut;
            }
            double now = wallClock.Elapsed.TotalSeconds;
            float dt = (float)Math.Min(0.1, Math.Max(0, now - lastWallT));
            lastWallT = now;
            float step = (HI - LOWH) / 0.2f * dt;
            foreach (var w in isoWallAt.Values)
            {
                float target = Tall(w) ? HI : LOWH;
                if (w.H < 0) w.H = target;
                else if (w.H < target) w.H = Math.Min(target, w.H + step);
                else if (w.H > target) w.H = Math.Max(target, w.H - step);
            }
        }

        Mesh doorGlass = new Mesh();

        void DrawIsoWalls(List<Cell> vis)
        {
            UpdateWallHeights();
            var m = new Mesh();
            var top = WallTopC;
            foreach (var c in vis)
            {
                foreach (var w in c.Walls)
                {
                    float h = WallH(w);
                    int x = w.X, y = w.Y;
                    // tampo (a "espessura" escura da parede, como no Habbo)
                    QuadC(m, rNoise, new V3(x, y + 1, h), new V3(x + 1, y + 1, h), new V3(x + 1, y, h), new V3(x, y, h), top, top, top, top, 0, 0, 0.2f, 0.2f);
                    if (h <= LOWH + 0.001f) continue;
                    // faces sul e leste: do chão da sala (ou do topo da parede vizinha, se ela estiver mais baixa) até o topo
                    float zs = w.OpenS ? LOWH : Math.Max(LOWH, WallHAt(x, y + 1)), ze = w.OpenE ? LOWH : Math.Max(LOWH, WallHAt(x + 1, y));
                    Color cs = w.OpenS ? w.CS : Exterior, ce = w.OpenE ? w.CE : Exterior;
                    if (zs < h - 0.001f) Face(m, rPlaster, new V3(x, y + 1, zs), new V3(x + 1, y + 1, zs), new V3(x + 1, y + 1, h), new V3(x, y + 1, h), cs, KS, 0, 1 - h / HI, 1, 1 - zs / HI);
                    if (ze < h - 0.001f) Face(m, rPlaster, new V3(x + 1, y + 1, ze), new V3(x + 1, y, ze), new V3(x + 1, y, h), new V3(x + 1, y + 1, h), ce, KE, 0, 1 - h / HI, 1, 1 - ze / HI);
                }
                foreach (var d in c.Decor)
                {
                    bool show = d.OnGlass;
                    if (!show)
                    {
                        // quadros e placas só aparecem enquanto a parede for alta o bastante para segurá-los
                        show = true;
                        for (int x = d.WX0; x <= d.WX1; x++) { float wh = WallHAt(x, d.WY); if (wh >= 0 && wh < d.Z1 - 0.02f) { show = false; break; } }
                    }
                    if (show) PanelS(m, d.R, d.X0, d.X1, d.Y, d.Z0, d.Z1, Color.White, 0.95f);
                }
            }
            DrawBadgeFrames(m);
            // portas da alvenaria: a moldura desce junto com a parede
            doorGlass = new Mesh();
            var wc = Color.White;
            foreach (var d in isoDoors)
            {
                if (!VisibleOrtho(d.X0 - 1, d.Y, d.X0 + d.N + 1, d.Y + 1, 0, HI + 1)) continue;
                float hl = WallHAt(d.X0 - 1, d.Y), hr = WallHAt(d.X0 + d.N, d.Y);
                float DH = Math.Max(LOWH, Math.Max(hl < 0 ? HI : hl, hr < 0 ? HI : hr));
                int x0 = d.X0, n = d.N;
                Box(m, x0, d.Ya, 0, x0 + 0.08f, d.Yb, DH, AluDk);
                Box(m, x0 + n - 0.08f, d.Ya, 0, x0 + n, d.Yb, DH, AluDk);
                if (DH < 1.2f) continue;   // parede cortada: só os batentes baixos (sem travessa no meio da passagem)
                Box(m, x0, d.Ya, DH - 0.08f, x0 + n, d.Yb, DH, Alu, null, null, false, true);
                foreach (var side in new[] { 0, 1 })
                {
                    float a = side == 0 ? x0 + 0.08f : x0 + n - 0.5f, b = side == 0 ? x0 + 0.5f : x0 + n - 0.08f;
                    QuadC(doorGlass, rGlass, new V3(a, d.Gy, 0.02f), new V3(b, d.Gy, 0.02f), new V3(b, d.Gy, DH - 0.08f), new V3(a, d.Gy, DH - 0.08f), wc, wc, wc, wc, 0, 0, 0.4f, 1);
                }
                float z0 = DH + 0.06f;
                QuadC(m, d.Plate, new V3(d.Pcx - d.Hx, d.Gy - d.Hy, z0), new V3(d.Pcx + d.Hx, d.Gy + d.Hy, z0), new V3(d.Pcx + d.Hx, d.Gy + d.Hy, z0 + d.Ph), new V3(d.Pcx - d.Hx, d.Gy - d.Hy, z0 + d.Ph), wc, wc, wc, wc, 0, 0, 1, 1);
                Box(m, d.Pcx - 0.03f, d.Gy - 0.03f, DH, d.Pcx + 0.03f, d.Gy + 0.03f, z0 + 0.02f, AluDk);
            }
            DrawMesh(m);
        }

        // ───────────────────────── bonecos de blocos ─────────────────────────

        class LookTex { public Region Front, Side, Back, Top; }
        readonly Dictionary<int, LookTex> lookTex = new Dictionary<int, LookTex>();
        readonly Dictionary<int, uint[]> avatarParts = new Dictionary<int, uint[]>();

        // rosto, lados, nuca e topo da cabeça (imagem suave, com mipmaps)
        LookTex FaceTex(Look L)
        {
            LookTex lt;
            if (lookTex.TryGetValue(L.Id, out lt)) return lt;
            int S = BlockAvatar.FaceS;
            var b = BlockAvatar.FaceBitmap(L);
            var data = Rgba(b, PixelFormat.Format32bppArgb, S * 4, S);
            b.Dispose();
            uint tex = Upload(data, S * 4, S, true, 3, false);
            pages.Add(new Page { Tex = tex });
            int pg = pages.Count - 1;
            float h = 0.5f / (S * 4), hv = 0.5f / S;
            Func<int, Region> reg = i => new Region { Page = pg, U0 = i * 0.25f + h, U1 = (i + 1) * 0.25f - h, V0 = hv, V1 = 1 - hv };
            lt = new LookTex { Front = reg(0), Side = reg(1), Back = reg(2), Top = reg(3) };
            lookTex[L.Id] = lt;
            return lt;
        }

        // boneco em 5 partes (corpo, pernas, braços): o jogo gira pernas e braços a cada quadro (animação contínua)
        uint[] AvatarParts(Look L)
        {
            uint[] lists;
            if (avatarParts.TryGetValue(L.Id, out lists)) return lists;
            var lt = FaceTex(L);
            var meshes = new Mesh[6];
            for (int i = 0; i < 6; i++) meshes[i] = new Mesh();
            foreach (var q in BlockAvatar.Build(L, 0, false))
            {
                var m = meshes[q.Part];
                var r = q.Tex == 0 ? lt.Front : q.Tex == 1 ? lt.Side : q.Tex == 2 ? lt.Back : q.Tex == 3 ? lt.Top : rNoise;
                if (q.Tex < 0) QuadC(m, rNoise, q.A, q.B, q.C, q.D, q.Col, q.Col, q.Col, q.Col, 0, 0, 0.15f, 0.15f);
                else QuadC(m, r, q.A, q.B, q.C, q.D, q.Col, q.Col, q.Col, q.Col, q.Mirror ? 1 : 0, 0, q.Mirror ? 0 : 1, 1);
            }
            lists = new uint[6];
            for (int i = 0; i < 6; i++)
            {
                lists[i] = GL.GenLists(1);
                GL.NewList(lists[i], GL.COMPILE);
                DrawMesh(meshes[i]);
                GL.EndList();
            }
            avatarParts[L.Id] = lists;
            return lists;
        }

        void DrawAvatar(Avatar3D a)
        {
            var parts = AvatarParts(a.Look);
            bool ride = a.Look.Ride == "patinete";
            float leg = a.Moving && !ride ? (float)Math.Sin(a.Walk) * 0.5f : 0, arm = -leg * 0.8f;
            float bob = a.Moving && !ride ? Math.Abs((float)Math.Cos(a.Walk)) * 0.035f : 0;
            const float R2D = 180f / (float)Math.PI;
            GL.PushMatrix();
            GL.Translate(a.X, 0, a.Y);
            GL.Rotate(90 - a.Angle * R2D, 0, 1, 0);
            if (ride) GL.CallList(parts[5]);
            for (int s = 0; s < 2; s++)
            {
                float ang = s == 0 ? leg : -leg;
                GL.PushMatrix();
                GL.Translate(0, BlockAvatar.Hip, 0); GL.Rotate(-ang * R2D, 1, 0, 0); GL.Translate(0, -BlockAvatar.Hip, 0);
                GL.CallList(parts[1 + s]);
                GL.PopMatrix();
            }
            GL.Translate(0, bob, 0);
            GL.CallList(parts[0]);
            for (int s = 0; s < 2; s++)
            {
                float ang = s == 0 ? arm : -arm;
                GL.PushMatrix();
                GL.Translate(0, BlockAvatar.Shoulder, 0); GL.Rotate(-ang * R2D, 1, 0, 0); GL.Translate(0, -BlockAvatar.Shoulder, 0);
                GL.CallList(parts[3 + s]);
                GL.PopMatrix();
            }
            GL.PopMatrix();
        }

        // ───────────────────────── câmera isométrica ─────────────────────────

        public void SetupIso(Frame3D f)
        {

            float s = IsoPxPerUnit * Math.Max(0.5f, f.Zoom);   // pixels da tela por tile
            orthoHalfW = f.VW / (2 * s); orthoHalfH = f.VH / (2 * s);
            orthoPx = s;
            orthoCX = f.VW / 2f; orthoCY = f.VH / 2f;
            double yaw = -3 * Math.PI / 4, pit = -Math.PI / 6;
            float fx = (float)(Math.Cos(pit) * Math.Cos(yaw)), fz = (float)(Math.Cos(pit) * Math.Sin(yaw)), fy = (float)Math.Sin(pit);
            float sl = (float)Math.Sqrt(fx * fx + fz * fz);
            float sx = -fz / sl, sy = 0, sz = fx / sl;
            float ux = sy * fz - sz * fy, uy = sz * fx - sx * fz, uz = sx * fy - sy * fx;
            float tx = f.CamX, ty = 0.9f, tz = f.CamY;
            float ex = tx - fx * 60, ey = ty - fy * 60, ez = tz - fz * 60;
            View[0] = sx; View[4] = sy; View[8] = sz; View[12] = -(sx * ex + sy * ey + sz * ez);
            View[1] = ux; View[5] = uy; View[9] = uz; View[13] = -(ux * ex + uy * ey + uz * ez);
            View[2] = -fx; View[6] = -fy; View[10] = -fz; View[14] = fx * ex + fy * ey + fz * ez;
            View[3] = 0; View[7] = 0; View[11] = 0; View[15] = 1;
            // câmera alinhada aos pixels da tela (as texturas não "cintilam" quando ela anda)
            View[12] = (float)Math.Round(View[12] * s) / s;
            View[13] = (float)Math.Round(View[13] * s) / s;
            Array.Clear(Proj, 0, 16);
            float n = 1, fa = 200;
            Proj[0] = 1 / orthoHalfW; Proj[5] = 1 / orthoHalfH;
            Proj[10] = -2 / (fa - n); Proj[14] = -(fa + n) / (fa - n); Proj[15] = 1;
            cutX = f.X; cutY = f.Y;
        }

        bool VisibleOrtho(float x0, float y0, float x1, float y1, float z0, float z1)
        {
            int left = 0, right = 0, up = 0, down = 0;
            float mx = orthoHalfW + 1, my = orthoHalfH + 1;
            for (int i = 0; i < 8; i++)
            {
                float x = (i & 1) == 0 ? x0 : x1, y = (i & 2) == 0 ? y0 : y1, z = (i & 4) == 0 ? z0 : z1;
                float vx, vy, vz;
                ToView(x, y, z, out vx, out vy, out vz);
                if (vx > mx) right++; if (vx < -mx) left++;
                if (vy > my) up++; if (vy < -my) down++;
            }
            return left < 8 && right < 8 && up < 8 && down < 8;
        }

        public void RenderIso(Frame3D f)
        {
            frameNo++;
            ReUploadDirty();
            int vw = f.VW, vh = f.VH;
            SetupIso(f);
            GL.Viewport(0, 0, vw, vh);
            GL.ClearColor(0.07f, 0.08f, 0.11f, 1);
            GL.Clear(GL.COLOR_BUFFER_BIT | GL.DEPTH_BUFFER_BIT);
            GL.Disable(GL.FOG); GL.Disable(GL.BLEND); GL.Disable(GL.ALPHA_TEST);
            if (GL.Samples > 0) GL.Enable(GL.MULTISAMPLE);
            GL.MatrixMode(GL.PROJECTION); GL.LoadMatrix(Proj);
            GL.MatrixMode(GL.MODELVIEW); GL.LoadMatrix(View);
            GL.Enable(GL.DEPTH_TEST); GL.DepthFunc(GL.LEQUAL); GL.DepthMask(true);
            GL.Enable(GL.TEXTURE_2D);
            GL.TexEnv(GL.TEXTURE_ENV, GL.TEXTURE_ENV_MODE, (int)GL.MODULATE);
            GL.ShadeModel(GL.SMOOTH);
            GL.Hint(GL.PERSPECTIVE_CORRECTION_HINT, GL.NICEST);

            var vis = new List<Cell>();
            foreach (var c in cells)
            {
                c.Dist = (float)Math.Sqrt(Dist2(c, f.X, f.Y));
                if (VisibleOrtho(c.X0, c.Y0, c.X1, c.Y1, 0, HI + 1)) vis.Add(c);
            }
            vis.Sort((a, b) => a.Dist.CompareTo(b.Dist));
            if (frameNo % 2 == 0)
                foreach (var c in vis)
                {
                    if (c.Dist > 30) break;
                    if (!floorTex.ContainsKey(c.CX * 100 + c.CY)) { LoadFloor(c); break; }
                }
            foreach (var c in vis)
            {
                uint tex;
                float x0 = c.CX * CS, y0 = c.CY * CS, x1 = x0 + CS, y1 = y0 + CS;
                if (floorTex.TryGetValue(c.CX * 100 + c.CY, out tex)) { GL.BindTexture(GL.TEXTURE_2D, tex); GL.Color4(1, 1, 1, 1); }
                else { if (c.LF != 0) GL.CallList(c.LF); continue; }
                GL.Begin(GL.QUADS);
                GL.TexCoord(0, 0); V(x0, y0, 0); GL.TexCoord(1, 0); V(x1, y0, 0);
                GL.TexCoord(1, 1); V(x1, y1, 0); GL.TexCoord(0, 1); V(x0, y1, 0);
                GL.End();
            }
            foreach (var c in vis) if (c.LO != 0) GL.CallList(c.LO);
            if (roomList != 0) GL.CallList(roomList);
            DrawIsoWalls(vis);
            DrawDynamic(f);

            // bonecos
            foreach (var a in f.Avatars)
                if (VisibleOrtho(a.X - 0.5f, a.Y - 0.5f, a.X + 0.5f, a.Y + 0.5f, 0, 2)) DrawAvatar(a);

            // sombras e marcas no chão
            GL.Enable(GL.BLEND); GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
            GL.DepthMask(false);
            GL.Enable(GL.POLYGON_OFFSET_FILL); GL.PolygonOffset(-1, -2);
            var decals = new List<Decal>(f.Decals);
            foreach (var a in f.Avatars) decals.Add(new Decal { X = a.X, Y = a.Y, RX = 0.36f, RY = 0.36f, C = Color.FromArgb(200, 0, 0, 0) });
            foreach (var b in f.Sprites) if (b.Shadow > 0) decals.Add(new Decal { X = b.X, Y = b.Y, RX = b.Shadow, RY = b.Shadow, C = Color.FromArgb(200, 0, 0, 0) });
            foreach (var d in decals)
            {
                var r = d.Ring ? rRing : rShadow;
                GL.BindTexture(GL.TEXTURE_2D, pages[r.Page].Tex);
                GL.Color4ub(d.C.R, d.C.G, d.C.B, d.C.A);
                GL.Begin(GL.QUADS);
                GL.TexCoord(r.U0, r.V0); V(d.X - d.RX, d.Y - d.RY, 0.004f);
                GL.TexCoord(r.U1, r.V0); V(d.X + d.RX, d.Y - d.RY, 0.004f);
                GL.TexCoord(r.U1, r.V1); V(d.X + d.RX, d.Y + d.RY, 0.004f);
                GL.TexCoord(r.U0, r.V1); V(d.X - d.RX, d.Y + d.RY, 0.004f);
                GL.End();
            }
            GL.Disable(GL.POLYGON_OFFSET_FILL);
            GL.DepthMask(true);

            // mascote (imagem virada para a câmera)
            GL.BlendFunc(GL.ONE, GL.ONE_MINUS_SRC_ALPHA);
            GL.Enable(GL.ALPHA_TEST); GL.AlphaFunc(GL.GREATER, 0.05f);
            float yaw = -3 * (float)Math.PI / 4;
            float rx = -(float)Math.Sin(yaw), ry = (float)Math.Cos(yaw);
            foreach (var b in f.Sprites)
            {
                if (!stex.ContainsKey(b.Key + "|4")) MakeSprite(b.Key, b.Get, 4);
                var t = FindSprite(b, 4);
                if (t == null) continue;
                t.Used = frameNo;
                GL.BindTexture(GL.TEXTURE_2D, t.Id);
                GL.Color4(1, 1, 1, 1);
                float lx = b.X - rx * b.W * b.FX, ly = b.Y - ry * b.W * b.FX, hx = b.X + rx * b.W * (1 - b.FX), hy = b.Y + ry * b.W * (1 - b.FX);
                float zb = -b.H * (1 - b.FY), zt = b.H * b.FY;
                GL.Begin(GL.QUADS);
                GL.TexCoord(0, t.V); V(lx, ly, zb);
                GL.TexCoord(t.U, t.V); V(hx, hy, zb);
                GL.TexCoord(t.U, 0); V(hx, hy, zt);
                GL.TexCoord(0, 0); V(lx, ly, zt);
                GL.End();
            }
            GL.Disable(GL.ALPHA_TEST);

            // vidros
            GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
            GL.DepthMask(false);
            for (int i = vis.Count - 1; i >= 0; i--) if (vis[i].LG != 0) GL.CallList(vis[i].LG);
            DrawMesh(doorGlass);
            GL.DepthMask(true);
            GL.Disable(GL.BLEND);

            if (f.Blur > 0.01f) BlurScene(vw, vh, f.Blur);
            DrawHudOverlay(f);
        }
    }
}
