using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace DockerQuest
{
    // Toda a arte do jogo é desenhada por código (sem arquivos de imagem).
    static class Art
    {
        public static readonly Color Void = Hx(0x161B26);
        public static readonly Color WallCap = Hx(0x4A5266), WallFace = Hx(0xCBD3DF), WallHi = Hx(0xE4E9F0), WallBase = Hx(0x8E99AB);
        public static readonly Color Docker = Hx(0x2496ED);
        public static bool DrawShadows = true;

        static readonly Dictionary<int, SolidBrush> brushes = new Dictionary<int, SolidBrush>();
        static readonly Dictionary<long, Pen> pens = new Dictionary<long, Pen>();

        public static Color Hx(int rgb) { return Color.FromArgb(255, (rgb >> 16) & 255, (rgb >> 8) & 255, rgb & 255); }
        public static Color A(Color c, int a) { return Color.FromArgb(Math.Max(0, Math.Min(255, a)), c); }
        public static Color Dk(Color c, float f) { return Color.FromArgb(c.A, (int)(c.R * (1 - f)), (int)(c.G * (1 - f)), (int)(c.B * (1 - f))); }
        public static Color Lt(Color c, float f) { return Color.FromArgb(c.A, (int)(c.R + (255 - c.R) * f), (int)(c.G + (255 - c.G) * f), (int)(c.B + (255 - c.B) * f)); }

        public static SolidBrush B(Color c)
        {
            int k = c.ToArgb();
            SolidBrush b;
            if (!brushes.TryGetValue(k, out b)) { b = new SolidBrush(c); brushes[k] = b; }
            return b;
        }

        public static Pen P(Color c, float w)
        {
            long k = ((long)(uint)c.ToArgb() << 20) ^ (long)(w * 64);
            Pen p;
            if (!pens.TryGetValue(k, out p)) { p = new Pen(c, w); p.StartCap = p.EndCap = LineCap.Round; pens[k] = p; }
            return p;
        }

        public static void R(Graphics g, Color c, float x, float y, float w, float h) { g.FillRectangle(B(c), x, y, w, h); }
        public static void E(Graphics g, Color c, float x, float y, float w, float h) { g.FillEllipse(B(c), x, y, w, h); }

        public static GraphicsPath RoundPath(float x, float y, float w, float h, float r)
        {
            var p = new GraphicsPath();
            r = Math.Max(0.5f, Math.Min(r, Math.Min(w, h) / 2));
            float d = r * 2;
            p.AddArc(x, y, d, d, 180, 90);
            p.AddArc(x + w - d, y, d, d, 270, 90);
            p.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            p.AddArc(x, y + h - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static void RR(Graphics g, Color c, float x, float y, float w, float h, float r)
        {
            if (w <= 0 || h <= 0) return;
            using (var p = RoundPath(x, y, w, h, r)) g.FillPath(B(c), p);
        }

        public static void RRO(Graphics g, Color c, float pw, float x, float y, float w, float h, float r)
        {
            using (var p = RoundPath(x, y, w, h, r)) g.DrawPath(P(c, pw), p);
        }

        public static void Text(Graphics g, string s, Font f, Color c, RectangleF r, StringAlignment h = StringAlignment.Center, StringAlignment v = StringAlignment.Center)
        {
            using (var sf = new StringFormat { Alignment = h, LineAlignment = v, Trimming = StringTrimming.EllipsisCharacter })
                g.DrawString(s, f, B(c), r, sf);
        }

        // ───────────────────────── logo ─────────────────────────
        // Logo do Stack Quest: três camadas isométricas empilhadas (a "stack") com um brilho de conquista.
        public const string GameName = "Stack Quest";

        public static void Logo(Graphics g, float x, float y, float s, bool background = true)
        {
            var oldSm = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (background)
            {
                using (var path = RoundPath(x, y, s, s, s * 0.22f))
                using (var lg = new LinearGradientBrush(new RectangleF(x, y, s, s), Hx(0x5B2EFF), Hx(0x00B4D8), 45f))
                {
                    g.FillPath(lg, path);
                    using (var hi = new LinearGradientBrush(new RectangleF(x, y - 1, s, s * 0.56f), A(Color.White, 55), A(Color.White, 0), 90f))
                    {
                        var clip = g.Clip;
                        g.SetClip(path, CombineMode.Intersect);
                        g.FillRectangle(hi, x, y, s, s * 0.5f);
                        g.Clip = clip;
                    }
                }
            }
            float cx = x + s * 0.47f, hw = s * 0.31f, hh = s * 0.155f, th = s * 0.085f;
            var tops = new[] { 0xFF9F1C, 0xF72585, 0x4CC9F0 };
            for (int i = 0; i < 3; i++)
            {
                float cy = y + s * 0.67f - i * s * 0.165f;
                Color top = Hx(tops[i]);
                var left = new[] { new PointF(cx - hw, cy), new PointF(cx, cy + hh), new PointF(cx, cy + hh + th), new PointF(cx - hw, cy + th) };
                var right = new[] { new PointF(cx, cy + hh), new PointF(cx + hw, cy), new PointF(cx + hw, cy + th), new PointF(cx, cy + hh + th) };
                var face = new[] { new PointF(cx, cy - hh), new PointF(cx + hw, cy), new PointF(cx, cy + hh), new PointF(cx - hw, cy) };
                g.FillPolygon(B(Dk(top, 0.25f)), left);
                g.FillPolygon(B(Dk(top, 0.42f)), right);
                g.FillPolygon(B(top), face);
                if (s >= 40)
                    using (var p = new Pen(A(Color.White, 120), Math.Max(1, s * 0.012f)))
                        g.DrawLine(p, cx - hw * 0.62f, cy - hh * 0.1f, cx - hw * 0.05f, cy - hh * 0.72f);
            }
            if (s >= 40)
            {
                // "</>" na camada de cima
                float ty = y + s * 0.67f - 2 * s * 0.165f;
                using (var p = new Pen(Hx(0x0B2545), Math.Max(1.5f, s * 0.028f)) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
                {
                    float u = s * 0.055f;
                    g.DrawLines(p, new[] { new PointF(cx - u * 1.2f, ty - u * 0.55f), new PointF(cx - u * 2.2f, ty), new PointF(cx - u * 1.2f, ty + u * 0.55f) });
                    g.DrawLines(p, new[] { new PointF(cx + u * 1.2f, ty - u * 0.55f), new PointF(cx + u * 2.2f, ty), new PointF(cx + u * 1.2f, ty + u * 0.55f) });
                    g.DrawLine(p, cx + u * 0.4f, ty - u * 0.7f, cx - u * 0.4f, ty + u * 0.7f);
                }
            }
            // brilho (estrela de 4 pontas)
            float sx = x + s * 0.8f, sy = y + s * 0.21f, r = s * 0.13f, ri = r * 0.28f;
            g.FillPolygon(B(Hx(0xFFE066)), new[] { new PointF(sx, sy - r), new PointF(sx + ri, sy - ri), new PointF(sx + r, sy), new PointF(sx + ri, sy + ri),
                                                   new PointF(sx, sy + r), new PointF(sx - ri, sy + ri), new PointF(sx - r, sy), new PointF(sx - ri, sy - ri) });
            g.SmoothingMode = oldSm;
        }

        // Logo + nome do jogo lado a lado. Retorna a largura ocupada.
        public static float Wordmark(Graphics g, float x, float y, float h, Color text, bool subtitle = true)
        {
            Logo(g, x, y, h);
            using (var f = new Font("Segoe UI Black", h * (subtitle ? 0.42f : 0.55f), GraphicsUnit.Pixel))
            using (var fs = new Font("Segoe UI Semibold", h * 0.2f, GraphicsUnit.Pixel))
            {
                var oldT = g.TextRenderingHint;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                float tx = x + h * 1.18f;
                var sz = g.MeasureString("STACK QUEST", f);
                g.DrawString("STACK QUEST", f, B(text), tx, y + (subtitle ? h * 0.04f : (h - sz.Height) / 2));
                float w = sz.Width;
                if (subtitle)
                {
                    g.DrawString("carreira tech na Micorosoft", fs, B(A(text, 170)), tx + h * 0.04f, y + h * 0.6f);
                    w = Math.Max(w, g.MeasureString("carreira tech na Micorosoft", fs).Width);
                }
                g.TextRenderingHint = oldT;
                return h * 1.18f + w;
            }
        }

        public static void Whale(Graphics g, float cx, float cy, float s, Color c)
        {
            float b = s * 0.14f, gap = s * 0.03f, baseY = cy - s * 0.02f;
            float row = 3 * b + 2 * gap, x0 = cx - row / 2 + s * 0.06f;
            for (int i = 0; i < 3; i++) R(g, c, x0 + i * (b + gap), baseY - b, b, b);
            for (int i = 0; i < 2; i++) R(g, c, x0 + (b + gap) * 0.5f + i * (b + gap), baseY - 2 * b - gap, b, b);
            R(g, c, x0 + (b + gap), baseY - 3 * b - 2 * gap, b, b);
            RR(g, c, cx - s * 0.42f, baseY + gap, s * 0.86f, s * 0.26f, s * 0.13f);
            g.FillPolygon(B(c), new[] {
                new PointF(cx - s * 0.36f, baseY + s * 0.06f), new PointF(cx - s * 0.56f, baseY - s * 0.12f), new PointF(cx - s * 0.46f, baseY + s * 0.16f)
            });
        }

        // ───────────────────────── mapa estático ─────────────────────────
        public static Bitmap RenderStatic(World w, float T)
        {
            var bmp = new Bitmap((int)Math.Ceiling(World.W * T), (int)Math.Ceiling(World.H * T), PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Void);
                g.SmoothingMode = SmoothingMode.None;
                for (int x = 0; x < World.W; x++)
                    for (int y = 0; y < World.H; y++)
                    {
                        int t = w.Tile[x, y];
                        if (t == World.Floor || t == World.Door) FloorTile(g, w, x, y, T);
                    }
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                foreach (var r in w.Rugs) Rug(g, r, T);
                g.SmoothingMode = SmoothingMode.None;
                for (int x = 0; x < World.W; x++)
                    for (int y = 0; y < World.H; y++)
                        if (w.Tile[x, y] == World.Wall)
                        {
                            bool face = y + 1 < World.H && (w.Tile[x, y + 1] == World.Floor || w.Tile[x, y + 1] == World.Door);
                            WallTile(g, x, y, T, face);
                        }
                g.SmoothingMode = SmoothingMode.AntiAlias;
                foreach (var d in w.Decor) WallDecor(g, d, T);
            }
            return bmp;
        }

        static void FloorTile(Graphics g, World w, int x, int y, float T)
        {
            int zone = w.Zone[x, y];
            Color a, b; int style;
            if (zone < 0) { a = Story.CorridorA; b = Story.CorridorB; style = 0; }
            else { var s = Story.SectorOf(zone); a = s.F1; b = s.F2; style = s.Style; }
            float X = x * T, Y = y * T;
            switch (style)
            {
                case 1: // madeira
                    for (int k = 0; k < 3; k++)
                    {
                        float py = Y + k * T / 3;
                        R(g, ((y * 3 + k) % 2 == 0) ? a : b, X, py, T, T / 3);
                        R(g, Dk(a, 0.12f), X, py + T / 3 - 1, T, 1);
                        float seam = ((y * 3 + k) * 17 + x * 5) % 3 * T / 3;
                        if (((x + y + k) % 2) == 0) R(g, Dk(a, 0.12f), X + seam, py, 1, T / 3);
                    }
                    break;
                case 2: // carpete
                    R(g, a, X, Y, T, T);
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 3; j++)
                            if ((i + j + x + y) % 2 == 0) R(g, b, X + (i + 0.3f) * T / 3, Y + (j + 0.3f) * T / 3, T / 8, T / 8);
                    break;
                case 3: // piso elevado de data center
                    R(g, Dk(a, 0.1f), X, Y, T, T);
                    R(g, ((x + y) % 2 == 0) ? a : b, X + 1, Y + 1, T - 2, T - 2);
                    for (int i = 1; i < 4; i++)
                        for (int j = 1; j < 4; j++) R(g, Dk(a, 0.18f), X + i * T / 4 - 1, Y + j * T / 4 - 1, 2, 2);
                    break;
                default: // ladrilho
                    R(g, ((x + y) % 2 == 0) ? a : b, X, Y, T, T);
                    R(g, Dk(a, 0.07f), X, Y + T - 1, T, 1);
                    R(g, Dk(a, 0.07f), X + T - 1, Y, 1, T);
                    break;
            }
        }

        static void WallTile(Graphics g, int x, int y, float T, bool face)
        {
            float X = x * T, Y = y * T;
            if (!face) { R(g, WallCap, X, Y, T, T); R(g, Lt(WallCap, 0.08f), X, Y, T, 2); return; }
            R(g, WallCap, X, Y, T, T * 0.22f);
            R(g, WallFace, X, Y + T * 0.22f, T, T * 0.78f);
            R(g, WallHi, X, Y + T * 0.22f, T, T * 0.05f);
            R(g, WallBase, X, Y + T * 0.88f, T, T * 0.12f);
        }

        static void Rug(Graphics g, Furn r, float T)
        {
            float x = r.X * T, y = r.Y * T, w = r.W * T, h = r.H * T;
            RR(g, A(r.C, 230), x, y, w, h, T * 0.3f);
            RRO(g, A(Lt(r.C, 0.4f), 200), T * 0.06f, x + T * 0.15f, y + T * 0.15f, w - T * 0.3f, h - T * 0.3f, T * 0.22f);
            if (r.Var == 1)
            {
                Whale(g, x + w / 2, y + h / 2 + T * 0.1f, Math.Min(w, h) * 0.6f, A(Color.White, 230));
            }
        }

        static void WallDecor(Graphics g, Furn d, float T)
        {
            float x = d.X * T + T * 0.08f, w = d.W * T - T * 0.16f, y = d.Y * T + T * 0.3f, h = T * 0.52f;
            switch (d.Kind)
            {
                case "window":
                    R(g, Hx(0xAEB9C7), x, y - T * 0.04f, w, h + T * 0.08f);
                    R(g, Hx(0x8CCBF2), x + T * 0.05f, y, w - T * 0.1f, h);
                    g.FillPolygon(B(A(Color.White, 90)), new[] { new PointF(x + w * 0.2f, y), new PointF(x + w * 0.4f, y), new PointF(x + w * 0.2f, y + h), new PointF(x + T * 0.05f, y + h) });
                    R(g, Hx(0xAEB9C7), x + w / 2 - 1.5f, y, 3, h);
                    break;
                case "board":
                    R(g, Hx(0x6D4C2F), x, y - T * 0.03f, w, h + T * 0.06f);
                    R(g, d.C, x + T * 0.04f, y, w - T * 0.08f, h);
                    var notes = new[] { 0xFFFFFF, 0xFFE66D, 0xFF8FAB, 0xA0E7E5, 0xFFFFFF, 0xC3F584 };
                    for (int i = 0; i < (int)(d.W * 3); i++)
                        R(g, Hx(notes[i % notes.Length]), x + T * 0.1f + i * T * 0.3f, y + T * (0.06f + (i % 2) * 0.18f), T * 0.22f, T * 0.2f);
                    break;
                case "whiteboard":
                    R(g, Hx(0x9AA4B1), x, y - T * 0.04f, w, h + T * 0.08f);
                    R(g, Hx(0xF7F9FB), x + T * 0.04f, y, w - T * 0.08f, h);
                    using (var f = new Font("Consolas", T * 0.16f, FontStyle.Bold, GraphicsUnit.Pixel))
                        Text(g, d.Text ?? "", f, Hx(0x1D4ED8), new RectangleF(x, y, w, h * 0.55f));
                    g.DrawLine(P(Hx(0xE63946), T * 0.03f), x + w * 0.15f, y + h * 0.65f, x + w * 0.55f, y + h * 0.65f);
                    g.DrawLine(P(Hx(0x2A9D8F), T * 0.03f), x + w * 0.15f, y + h * 0.82f, x + w * 0.75f, y + h * 0.82f);
                    break;
                case "screen":
                case "dashboard":
                    R(g, Hx(0x2A3443), x, y - T * 0.05f, w, h + T * 0.1f);
                    R(g, Hx(0x0D1520), x + T * 0.04f, y, w - T * 0.08f, h);
                    int panels = Math.Max(1, (int)(d.W / 1.5f));
                    float pw = (w - T * 0.08f) / panels;
                    var rnd = new Random((int)(d.X * 31));
                    for (int p = 0; p < panels; p++)
                    {
                        float px = x + T * 0.04f + p * pw;
                        var pts = new PointF[8];
                        for (int i = 0; i < 8; i++) pts[i] = new PointF(px + T * 0.06f + i * (pw - T * 0.12f) / 7, y + h * (0.25f + (float)rnd.NextDouble() * 0.55f));
                        g.DrawLines(P(p % 2 == 0 ? Hx(0x3FB950) : Docker, T * 0.03f), pts);
                        R(g, rnd.Next(5) == 0 ? Hx(0xF85149) : Hx(0x3FB950), px + T * 0.06f, y + T * 0.05f, T * 0.06f, T * 0.06f);
                    }
                    break;
                case "kanban":
                    R(g, Hx(0x9AA4B1), x, y - T * 0.04f, w, h + T * 0.08f);
                    R(g, Hx(0xF7F9FB), x + T * 0.04f, y, w - T * 0.08f, h);
                    var cols = new[] { 0xE63946, 0xF4A261, 0x2A9D8F };
                    float cw = (w - T * 0.08f) / 3;
                    for (int c = 0; c < 3; c++)
                    {
                        R(g, Hx(cols[c]), x + T * 0.06f + c * cw, y + T * 0.03f, cw - T * 0.06f, T * 0.06f);
                        for (int k = 0; k < 3 - c % 2; k++)
                            R(g, Hx(new[] { 0xFFE66D, 0xFF8FAB, 0xA0E7E5 }[(c + k) % 3]), x + T * 0.1f + c * cw + (k % 2) * T * 0.25f, y + T * (0.14f + (k / 2) * 0.17f + (k % 2) * 0.08f), T * 0.2f, T * 0.14f);
                    }
                    break;
                case "sign":
                    RR(g, d.C, x, y + T * 0.04f, w, h - T * 0.08f, T * 0.08f);
                    using (var f = new Font("Segoe UI Black", T * 0.22f, GraphicsUnit.Pixel))
                        Text(g, d.Text ?? "", f, Color.White, new RectangleF(x, y, w, h));
                    break;
                case "poster":
                    R(g, Hx(0xEDF2F7), x + T * 0.08f, y - T * 0.04f, w - T * 0.16f, h + T * 0.08f);
                    R(g, d.C, x + T * 0.13f, y, w - T * 0.26f, h);
                    Whale(g, x + w / 2, y + h * 0.58f, T * 0.42f, Color.White);
                    break;
            }
        }

        public static Bitmap RenderMini(World w, float m)
        {
            var bmp = new Bitmap((int)(World.W * m), (int)(World.H * m), PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                for (int x = 0; x < World.W; x++)
                    for (int y = 0; y < World.H; y++)
                    {
                        int t = w.Tile[x, y];
                        Color c;
                        if (t == World.Wall) c = Hx(0x2E3546);
                        else if (t == World.Void) continue;
                        else if (w.Zone[x, y] < 0) c = Hx(0x8D96A5);
                        else c = Story.SectorOf(w.Zone[x, y]).F1;
                        R(g, c, x * m, y * m, m, m);
                    }
            }
            return bmp;
        }

        // ───────────────────────── portas e placas ─────────────────────────
        public static void DoorAndPlate(Graphics g, DoorInfo d, bool locked, bool done, float T, float t, Font f)
        {
            float x = d.Tiles[0].X * T, y = d.Tiles[0].Y * T;
            float w = d.Vertical ? T : 2 * T, h = d.Vertical ? 2 * T : T;
            Color led = locked ? Hx(0xF85149) : Hx(0x3FB950);
            if (locked)
            {
                R(g, Hx(0x7B8496), x, y, w, h);
                if (d.Vertical)
                {
                    R(g, Hx(0x9AA5B6), x + T * 0.3f, y, T * 0.4f, h);
                    R(g, Hx(0x6B7486), x + T * 0.3f, y + h / 2 - 1, T * 0.4f, 2);
                }
                else
                {
                    R(g, Hx(0x9AA5B6), x, y + T * 0.15f, w, T * 0.75f);
                    R(g, Hx(0x6B7486), x + w / 2 - 1, y + T * 0.15f, 2, T * 0.75f);
                }
                // cadeado
                float cx = x + w / 2, cy = y + h / 2;
                g.DrawArc(P(Hx(0x3A3F4B), T * 0.06f), cx - T * 0.11f, cy - T * 0.22f, T * 0.22f, T * 0.24f, 180, 180);
                RR(g, Hx(0xF4A261), cx - T * 0.16f, cy - T * 0.1f, T * 0.32f, T * 0.26f, T * 0.05f);
                E(g, Hx(0x3A3F4B), cx - T * 0.035f, cy - T * 0.02f, T * 0.07f, T * 0.07f);
            }
            else
            {
                if (d.Vertical) { R(g, Hx(0x8E99AB), x, y - T * 0.1f, T, T * 0.1f); R(g, Hx(0x8E99AB), x, y + h, T, T * 0.1f); }
                else { R(g, Hx(0x8E99AB), x - T * 0.1f, y, T * 0.1f, T); R(g, Hx(0x8E99AB), x + w, y, T * 0.1f, T); }
                R(g, A(Color.Black, 25), x, y, w, h);
            }
            float pulse = 0.6f + 0.4f * (float)Math.Sin(t * 4);
            float lx = d.Vertical ? x + T * 0.5f : x + w + T * 0.2f, ly = d.Vertical ? y - T * 0.2f : y + T * 0.2f;
            E(g, A(led, (int)(90 * pulse)), lx - T * 0.12f, ly - T * 0.12f, T * 0.24f, T * 0.24f);
            E(g, led, lx - T * 0.06f, ly - T * 0.06f, T * 0.12f, T * 0.12f);

            // placa com o nome do setor
            var pr = d.Plate;
            float px = pr.X * T, py = pr.Y * T, pw = pr.Width * T, ph = pr.Height * T;
            Color bg = d.Chapter == Story.Library ? Hx(0x6B3E26) : locked ? Hx(0x5C2430) : done ? Hx(0x1F6F43) : Hx(0x1F5F9E);
            RR(g, A(Color.Black, 60), px + 2, py + 3, pw, ph, T * 0.1f);
            RR(g, bg, px, py, pw, ph, T * 0.1f);
            RRO(g, Lt(bg, 0.35f), Math.Max(1, T * 0.03f), px, py, pw, ph, T * 0.1f);
            string label = d.Chapter == Story.Library ? "BIBLIOTECA · DOCUMENTAÇÃO" : (done ? "✔ " : "") + Story.SectorOf(d.Chapter).Short + (d.Vertical ? "  ►" : "");
            if (locked)
            {
                Icon(g, "lock", new RectangleF(px + ph * 0.15f, py + ph * 0.12f, ph * 0.76f, ph * 0.76f));
                Text(g, label, f, Color.White, new RectangleF(px + ph * 0.7f, py, pw - ph * 0.7f, ph));
            }
            else Text(g, label, f, Color.White, new RectangleF(px, py, pw, ph));
        }

        // Ícones dos itens da lojinha (consumíveis e utilidades).
        public static void ItemIcon(Graphics g, string id, RectangleF r)
        {
            float x = r.X, y = r.Y, w = r.Width, h = r.Height;
            var edge = P(Hx(0x1A1D26), Math.Max(1, w * 0.04f));
            switch (id)
            {
                case "cafe_expresso":
                    E(g, Hx(0xC9D1D9), x + w * 0.12f, y + h * 0.78f, w * 0.76f, h * 0.12f);
                    RR(g, Color.White, x + w * 0.25f, y + h * 0.42f, w * 0.42f, h * 0.38f, w * 0.08f);
                    g.DrawArc(P(Color.White, w * 0.07f), x + w * 0.58f, y + h * 0.48f, w * 0.2f, h * 0.2f, -90, 180);
                    E(g, Hx(0x6F4E37), x + w * 0.28f, y + h * 0.42f, w * 0.36f, h * 0.1f);
                    for (int i = 0; i < 2; i++) g.DrawBezier(P(A(Color.White, 160), w * 0.04f), x + w * (0.38f + i * 0.14f), y + h * 0.36f, x + w * (0.3f + i * 0.14f), y + h * 0.26f, x + w * (0.48f + i * 0.14f), y + h * 0.2f, x + w * (0.4f + i * 0.14f), y + h * 0.1f);
                    break;
                case "pao_queijo":
                    foreach (var p in new[] { new PointF(0.18f, 0.45f), new PointF(0.48f, 0.4f), new PointF(0.32f, 0.2f) })
                    {
                        E(g, Hx(0xE9B44C), x + w * p.X, y + h * p.Y, w * 0.38f, h * 0.36f);
                        E(g, Hx(0xF6D58E), x + w * (p.X + 0.08f), y + h * (p.Y + 0.06f), w * 0.15f, h * 0.12f);
                        g.DrawEllipse(edge, x + w * p.X, y + h * p.Y, w * 0.38f, h * 0.36f);
                    }
                    break;
                case "energetico":
                    RR(g, Hx(0x2EA043), x + w * 0.3f, y + h * 0.12f, w * 0.4f, h * 0.76f, w * 0.08f);
                    R(g, Hx(0xB8C0CC), x + w * 0.3f, y + h * 0.12f, w * 0.4f, h * 0.08f);
                    g.FillPolygon(B(Hx(0xF5C542)), new[] { new PointF(x + w * 0.54f, y + h * 0.28f), new PointF(x + w * 0.38f, y + h * 0.55f), new PointF(x + w * 0.5f, y + h * 0.55f),
                        new PointF(x + w * 0.44f, y + h * 0.78f), new PointF(x + w * 0.62f, y + h * 0.48f), new PointF(x + w * 0.5f, y + h * 0.48f) });
                    break;
                case "pizza":
                    var tri = new[] { new PointF(x + w * 0.5f, y + h * 0.88f), new PointF(x + w * 0.12f, y + h * 0.22f), new PointF(x + w * 0.88f, y + h * 0.22f) };
                    g.FillPolygon(B(Hx(0xF5C542)), tri);
                    RR(g, Hx(0xC98A06), x + w * 0.1f, y + h * 0.14f, w * 0.8f, h * 0.14f, h * 0.06f);
                    foreach (var p in new[] { new PointF(0.38f, 0.36f), new PointF(0.56f, 0.4f), new PointF(0.47f, 0.58f) }) E(g, Hx(0xD62828), x + w * p.X, y + h * p.Y, w * 0.12f, h * 0.12f);
                    g.DrawPolygon(edge, tri);
                    break;
                case "borracha":
                    {
                        var st = g.Save();
                        g.TranslateTransform(x + w / 2, y + h / 2);
                        g.RotateTransform(-25);
                        R(g, Hx(0xFF8FAB), -w * 0.36f, -h * 0.16f, w * 0.4f, h * 0.32f);
                        R(g, Hx(0x58A6FF), w * 0.04f, -h * 0.16f, w * 0.32f, h * 0.32f);
                        g.DrawRectangle(edge, -w * 0.36f, -h * 0.16f, w * 0.72f, h * 0.32f);
                        g.Restore(st);
                        for (int i = 0; i < 3; i++) E(g, Hx(0xF5C542), x + w * (0.15f + i * 0.3f), y + h * (0.12f + (i % 2) * 0.08f), w * 0.09f, w * 0.09f);
                        break;
                    }
                case "teleporte":
                    RR(g, Hx(0x8E6CC4), x + w * 0.22f, y + h * 0.12f, w * 0.56f, h * 0.76f, w * 0.08f);
                    R(g, Color.White, x + w * 0.3f, y + h * 0.22f, w * 0.4f, h * 0.24f);
                    g.FillPolygon(B(Hx(0xF5C542)), new[] { new PointF(x + w * 0.56f, y + h * 0.5f), new PointF(x + w * 0.4f, y + h * 0.7f), new PointF(x + w * 0.5f, y + h * 0.7f),
                        new PointF(x + w * 0.45f, y + h * 0.86f), new PointF(x + w * 0.62f, y + h * 0.64f), new PointF(x + w * 0.52f, y + h * 0.64f) });
                    break;
                case "tenis":
                    RR(g, Hx(0xE63946), x + w * 0.12f, y + h * 0.42f, w * 0.72f, h * 0.3f, w * 0.12f);
                    R(g, Color.White, x + w * 0.12f, y + h * 0.66f, w * 0.76f, h * 0.1f);
                    R(g, Color.White, x + w * 0.3f, y + h * 0.48f, w * 0.2f, h * 0.04f);
                    break;
                case "patinete":
                    g.DrawLine(P(Hx(0x8B949E), w * 0.07f), x + w * 0.66f, y + h * 0.15f, x + w * 0.72f, y + h * 0.72f);
                    g.DrawLine(P(Hx(0x8B949E), w * 0.07f), x + w * 0.56f, y + h * 0.15f, x + w * 0.76f, y + h * 0.15f);
                    R(g, Hx(0x2496ED), x + w * 0.2f, y + h * 0.68f, w * 0.55f, h * 0.08f);
                    E(g, Hx(0x1A1D26), x + w * 0.12f, y + h * 0.7f, w * 0.18f, w * 0.18f);
                    E(g, Hx(0x1A1D26), x + w * 0.66f, y + h * 0.7f, w * 0.18f, w * 0.18f);
                    break;
                case "fone_ruido":
                    g.DrawArc(P(Hx(0x2B2D42), w * 0.09f), x + w * 0.18f, y + h * 0.15f, w * 0.64f, h * 0.7f, 180, 180);
                    RR(g, Hx(0x2496ED), x + w * 0.1f, y + h * 0.45f, w * 0.22f, h * 0.34f, w * 0.06f);
                    RR(g, Hx(0x2496ED), x + w * 0.68f, y + h * 0.45f, w * 0.22f, h * 0.34f, w * 0.06f);
                    break;
                case "cafeteira":
                    RR(g, Hx(0x59606B), x + w * 0.22f, y + h * 0.1f, w * 0.56f, h * 0.8f, w * 0.06f);
                    R(g, Hx(0x1F2329), x + w * 0.3f, y + h * 0.18f, w * 0.4f, h * 0.2f);
                    R(g, Hx(0xF85149), x + w * 0.6f, y + h * 0.22f, w * 0.06f, h * 0.06f);
                    RR(g, Color.White, x + w * 0.4f, y + h * 0.62f, w * 0.2f, h * 0.18f, w * 0.04f);
                    break;
                case "mapa":
                    g.FillPolygon(B(Hx(0xE9D8A6)), new[] { new PointF(x + w * 0.1f, y + h * 0.2f), new PointF(x + w * 0.38f, y + h * 0.12f), new PointF(x + w * 0.62f, y + h * 0.2f),
                        new PointF(x + w * 0.9f, y + h * 0.12f), new PointF(x + w * 0.9f, y + h * 0.8f), new PointF(x + w * 0.62f, y + h * 0.88f), new PointF(x + w * 0.38f, y + h * 0.8f), new PointF(x + w * 0.1f, y + h * 0.88f) });
                    g.DrawLine(P(Hx(0xE63946), w * 0.04f), x + w * 0.25f, y + h * 0.6f, x + w * 0.7f, y + h * 0.35f);
                    E(g, Hx(0xE63946), x + w * 0.66f, y + h * 0.3f, w * 0.1f, w * 0.1f);
                    break;
                case "cha_verde":
                    E(g, Hx(0xC9D1D9), x + w * 0.14f, y + h * 0.8f, w * 0.72f, h * 0.1f);
                    RR(g, Hx(0xF1F5F9), x + w * 0.22f, y + h * 0.4f, w * 0.48f, h * 0.42f, w * 0.12f);
                    R(g, Hx(0x7FB069), x + w * 0.26f, y + h * 0.43f, w * 0.4f, h * 0.08f);
                    g.DrawArc(P(Hx(0xF1F5F9), w * 0.07f), x + w * 0.62f, y + h * 0.48f, w * 0.2f, h * 0.2f, -90, 180);
                    g.DrawLine(P(Hx(0xC9D1D9), w * 0.02f), x + w * 0.5f, y + h * 0.42f, x + w * 0.56f, y + h * 0.16f);
                    R(g, Hx(0x2A9D8F), x + w * 0.5f, y + h * 0.1f, w * 0.14f, h * 0.1f);
                    break;
                case "bolo":
                    E(g, Hx(0xC9D1D9), x + w * 0.08f, y + h * 0.78f, w * 0.84f, h * 0.14f);
                    RR(g, Hx(0x8B5A2B), x + w * 0.16f, y + h * 0.5f, w * 0.68f, h * 0.32f, w * 0.06f);
                    RR(g, Hx(0xFF8FAB), x + w * 0.14f, y + h * 0.42f, w * 0.72f, h * 0.14f, w * 0.06f);
                    for (int i = 0; i < 3; i++)
                    {
                        R(g, Hx(0xF1F5F9), x + w * (0.3f + i * 0.17f), y + h * 0.24f, w * 0.05f, h * 0.18f);
                        E(g, Hx(0xF5C542), x + w * (0.29f + i * 0.17f), y + h * 0.13f, w * 0.07f, h * 0.1f);
                    }
                    break;
                case "teclado":
                    RR(g, Hx(0x2B2D42), x + w * 0.06f, y + h * 0.32f, w * 0.88f, h * 0.4f, w * 0.06f);
                    for (int r2 = 0; r2 < 3; r2++)
                        for (int k = 0; k < 7; k++)
                            R(g, (r2 + k) % 5 == 0 ? Hx(0xF0B429) : Hx(0xC9D1D9), x + w * (0.11f + k * 0.115f), y + h * (0.37f + r2 * 0.105f), w * 0.085f, h * 0.075f);
                    break;
                case "monitor":
                    RR(g, Hx(0x2B2D42), x + w * 0.04f, y + h * 0.18f, w * 0.92f, h * 0.48f, w * 0.05f);
                    R(g, Hx(0x1F5F9E), x + w * 0.08f, y + h * 0.22f, w * 0.84f, h * 0.4f);
                    R(g, Hx(0x58A6FF), x + w * 0.12f, y + h * 0.28f, w * 0.3f, h * 0.05f);
                    R(g, Hx(0x3FB950), x + w * 0.12f, y + h * 0.38f, w * 0.5f, h * 0.05f);
                    R(g, Hx(0x59606B), x + w * 0.45f, y + h * 0.66f, w * 0.1f, h * 0.14f);
                    R(g, Hx(0x59606B), x + w * 0.3f, y + h * 0.8f, w * 0.4f, h * 0.06f);
                    break;
                case "livro_etica":
                    RR(g, Hx(0x1D3557), x + w * 0.2f, y + h * 0.1f, w * 0.6f, h * 0.8f, w * 0.05f);
                    R(g, Hx(0xF1F5F9), x + w * 0.74f, y + h * 0.14f, w * 0.06f, h * 0.72f);
                    R(g, Hx(0xF5C542), x + w * 0.3f, y + h * 0.25f, w * 0.38f, h * 0.05f);
                    g.DrawLine(P(Hx(0xF5C542), w * 0.04f), x + w * 0.49f, y + h * 0.38f, x + w * 0.49f, y + h * 0.72f);
                    g.DrawLine(P(Hx(0xF5C542), w * 0.04f), x + w * 0.33f, y + h * 0.45f, x + w * 0.65f, y + h * 0.45f);
                    g.DrawArc(P(Hx(0xF5C542), w * 0.03f), x + w * 0.28f, y + h * 0.48f, w * 0.12f, h * 0.1f, 0, 180);
                    g.DrawArc(P(Hx(0xF5C542), w * 0.03f), x + w * 0.58f, y + h * 0.48f, w * 0.12f, h * 0.1f, 0, 180);
                    break;
                default:
                    Icon(g, "board", r);
                    break;
            }
        }

        // Ícones simples desenhados à mão (emojis não renderizam bem no GDI+).
        public static void Icon(Graphics g, string kind, RectangleF r)
        {
            float x = r.X, y = r.Y, w = r.Width, h = r.Height;
            switch (kind)
            {
                case "lock":
                    g.DrawArc(P(Hx(0xD0D7DE), w * 0.12f), x + w * 0.28f, y + h * 0.05f, w * 0.44f, h * 0.55f, 180, 180);
                    RR(g, Hx(0xF0B429), x + w * 0.15f, y + h * 0.38f, w * 0.7f, h * 0.55f, w * 0.1f);
                    E(g, Hx(0x5C4410), x + w * 0.43f, y + h * 0.52f, w * 0.14f, h * 0.14f);
                    R(g, Hx(0x5C4410), x + w * 0.47f, y + h * 0.6f, w * 0.06f, h * 0.16f);
                    break;
                case "coffee":
                    for (int i = 0; i < 3; i++)
                        g.DrawBezier(P(A(Color.White, 170), w * 0.04f), x + w * (0.3f + i * 0.15f), y + h * 0.42f, x + w * (0.22f + i * 0.15f), y + h * 0.3f,
                            x + w * (0.38f + i * 0.15f), y + h * 0.22f, x + w * (0.3f + i * 0.15f), y + h * 0.1f);
                    g.DrawArc(P(Hx(0xF1F5F9), w * 0.07f), x + w * 0.62f, y + h * 0.5f, w * 0.22f, h * 0.22f, -90, 180);
                    RR(g, Hx(0xF1F5F9), x + w * 0.2f, y + h * 0.45f, w * 0.48f, h * 0.42f, w * 0.1f);
                    R(g, Hx(0x6F4E37), x + w * 0.24f, y + h * 0.47f, w * 0.4f, h * 0.07f);
                    E(g, Hx(0xC9D1D9), x + w * 0.1f, y + h * 0.84f, w * 0.68f, h * 0.1f);
                    break;
                case "water":
                    g.FillPolygon(B(Hx(0x58A6FF)), new[] { new PointF(x + w * 0.5f, y + h * 0.08f), new PointF(x + w * 0.75f, y + h * 0.55f), new PointF(x + w * 0.25f, y + h * 0.55f) });
                    E(g, Hx(0x58A6FF), x + w * 0.24f, y + h * 0.38f, w * 0.52f, h * 0.52f);
                    E(g, A(Color.White, 150), x + w * 0.34f, y + h * 0.5f, w * 0.1f, h * 0.18f);
                    break;
                default: // quadro / prancheta
                    RR(g, Hx(0x8D6E63), x + w * 0.18f, y + h * 0.1f, w * 0.64f, h * 0.82f, w * 0.06f);
                    R(g, Hx(0xF7F9FB), x + w * 0.25f, y + h * 0.2f, w * 0.5f, h * 0.64f);
                    RR(g, Hx(0x9AA4B1), x + w * 0.38f, y + h * 0.05f, w * 0.24f, h * 0.1f, w * 0.03f);
                    for (int i = 0; i < 4; i++) R(g, Hx(i == 0 ? 0x2496ED : 0x8B949E), x + w * 0.31f, y + h * (0.3f + i * 0.13f), w * (i % 2 == 0 ? 0.38f : 0.28f), h * 0.05f);
                    break;
            }
        }

        public static void Glow(Graphics g, Furn desk, float T, float t, bool strong)
        {
            float cx = (desk.X + desk.W / 2) * T, cy = (desk.Y + 0.6f) * T;
            float pulse = 0.5f + 0.5f * (float)Math.Sin(t * 3);
            int a = strong ? (int)(50 + 70 * pulse) : 30;
            float r = T * (strong ? 1.5f + 0.15f * pulse : 1.3f);
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(cx - r, cy - r * 0.7f, 2 * r, 1.4f * r);
                using (var pb = new PathGradientBrush(path) { CenterColor = A(Docker, a), SurroundColors = new[] { A(Docker, 0) } })
                    g.FillPath(pb, path);
            }
        }

        // ───────────────────────── móveis ─────────────────────────
        public static void Furniture(Graphics g, Furn f, float T, float t)
        {
            float x = f.X * T, y = f.Y * T, w = f.W * T, h = f.H * T;
            switch (f.Kind)
            {
                case "desk": Desk(g, x, y, w, h, T, f.C, f.Var); break;
                case "terminal": TerminalDesk(g, x, y, w, h, T, t); break;
                case "counter": Counter(g, x, y, w, h, T); break;
                case "plant": Plant(g, x, y, T); break;
                case "sofa": Sofa(g, x, y, w, h, T, f.C); break;
                case "bench": Sofa(g, x, y + T * 0.2f, w, h - T * 0.2f, T, f.C); break;
                case "crate": Crate(g, x, y, w, h, T, f.C); break;
                case "rack": Rack(g, x, y, T, t, f.Var); break;
                case "table": Table(g, x, y, w, h, T, f.C); break;
                case "bigdesk": Table(g, x, y, w, h, T, f.C); Laptop(g, x + w * 0.5f, y + T * 0.3f, T); E(g, Hx(0xFFE08A), x + w * 0.15f, y + T * 0.05f, T * 0.3f, T * 0.3f); break;
                case "labtable": LabTable(g, x, y, w, h, T); break;
                case "beanbag": Beanbag(g, x, y, T, f.C); break;
                case "pingpong": PingPong(g, x, y, w, h, T); break;
                case "shelf": Shelf(g, x, y, w, h, T, f.C); break;
                case "coffee": Coffee(g, x, y, T, t); break;
                case "cooler": Cooler(g, x, y, T); break;
                case "ac": AirCon(g, x, y, w, h, T); break;
                case "kiosk": Kiosk(g, x, y, w, h, T, t); break;
            }
        }

        static void Shadow(Graphics g, float x, float y, float w, float T)
        {
            E(g, A(Color.Black, 45), x - T * 0.02f, y - T * 0.08f, w + T * 0.04f, T * 0.18f);
        }

        static void Monitor(Graphics g, float cx, float baseY, float T, Color scr)
        {
            R(g, Hx(0x2A2F3A), cx - T * 0.05f, baseY - T * 0.06f, T * 0.1f, T * 0.12f);
            RR(g, Hx(0x2A2F3A), cx - T * 0.32f, baseY - T * 0.46f, T * 0.64f, T * 0.42f, T * 0.04f);
            R(g, scr, cx - T * 0.27f, baseY - T * 0.42f, T * 0.54f, T * 0.32f);
            R(g, A(Color.White, 70), cx - T * 0.22f, baseY - T * 0.36f, T * 0.3f, T * 0.03f);
            R(g, A(Color.White, 50), cx - T * 0.22f, baseY - T * 0.29f, T * 0.4f, T * 0.03f);
            R(g, A(Color.White, 50), cx - T * 0.22f, baseY - T * 0.22f, T * 0.2f, T * 0.03f);
        }

        static void Desk(Graphics g, float x, float y, float w, float h, float T, Color top, int var)
        {
            Shadow(g, x, y + h, w, T);
            RR(g, top, x + T * 0.04f, y + T * 0.1f, w - T * 0.08f, h - T * 0.3f, T * 0.06f);
            R(g, Dk(top, 0.3f), x + T * 0.04f, y + h - T * 0.24f, w - T * 0.08f, T * 0.2f);
            if (var == 2) { Monitor(g, x + w * 0.3f, y + T * 0.42f, T, Hx(0x1F6FEB)); Monitor(g, x + w * 0.72f, y + T * 0.42f, T, Hx(0x2EA043)); }
            else if (var == 1) Monitor(g, x + w / 2, y + T * 0.42f, T, Hx(0x3B82C4));
            R(g, Hx(0xD9DEE5), x + w / 2 - T * 0.2f, y + T * 0.48f, T * 0.4f, T * 0.09f);
        }

        static void TerminalDesk(Graphics g, float x, float y, float w, float h, float T, float t)
        {
            Shadow(g, x, y + h, w, T);
            RR(g, Hx(0x3A4150), x + T * 0.04f, y + T * 0.1f, w - T * 0.08f, h - T * 0.3f, T * 0.06f);
            R(g, Hx(0x262B36), x + T * 0.04f, y + h - T * 0.24f, w - T * 0.08f, T * 0.2f);
            R(g, Docker, x + T * 0.04f, y + h - T * 0.24f, w - T * 0.08f, T * 0.04f);
            float cx = x + w / 2;
            R(g, Hx(0x1E222B), cx - T * 0.06f, y + T * 0.2f, T * 0.12f, T * 0.25f);
            RR(g, Hx(0x1E222B), cx - T * 0.6f, y - T * 0.55f, T * 1.2f, T * 0.8f, T * 0.06f);
            R(g, Hx(0x0B2A4A), cx - T * 0.54f, y - T * 0.5f, T * 1.08f, T * 0.66f);
            using (var f = new Font("Consolas", T * 0.17f, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                g.DrawString("$ docker", f, B(Hx(0x7EE787)), cx - T * 0.5f, y - T * 0.47f);
                if ((int)(t * 2) % 2 == 0) R(g, Hx(0x7EE787), cx - T * 0.5f, y - T * 0.15f, T * 0.1f, T * 0.04f);
            }
            Whale(g, cx + T * 0.32f, y - T * 0.12f, T * 0.3f, A(Docker, 230));
            R(g, Hx(0xD9DEE5), cx - T * 0.25f, y + T * 0.5f, T * 0.5f, T * 0.1f);
        }

        static void Counter(Graphics g, float x, float y, float w, float h, float T)
        {
            Shadow(g, x, y + h, w, T);
            RR(g, Hx(0xCFD8E3), x, y - T * 0.05f, w, T * 0.45f, T * 0.08f);
            R(g, Hx(0xF1F5F9), x, y + T * 0.35f, w, h - T * 0.38f);
            R(g, Docker, x, y + T * 0.55f, w, T * 0.12f);
            Monitor(g, x + w * 0.35f, y + T * 0.25f, T, Hx(0x3B82C4));
            Whale(g, x + w * 0.75f, y + T * 0.82f, T * 0.3f, Docker);
        }

        static void Plant(Graphics g, float x, float y, float T)
        {
            Shadow(g, x + T * 0.2f, y + T, T * 0.6f, T);
            g.FillPolygon(B(Hx(0xB5653A)), new[] {
                new PointF(x + T * 0.27f, y + T * 0.55f), new PointF(x + T * 0.73f, y + T * 0.55f),
                new PointF(x + T * 0.65f, y + T * 0.97f), new PointF(x + T * 0.35f, y + T * 0.97f) });
            R(g, Hx(0x8E4A28), x + T * 0.27f, y + T * 0.55f, T * 0.46f, T * 0.07f);
            E(g, Hx(0x2E7D4F), x + T * 0.08f, y + T * 0.05f, T * 0.5f, T * 0.55f);
            E(g, Hx(0x2E7D4F), x + T * 0.42f, y + T * 0.0f, T * 0.5f, T * 0.6f);
            E(g, Hx(0x3DA266), x + T * 0.2f, y - T * 0.32f, T * 0.6f, T * 0.65f);
            E(g, Hx(0x52B97A), x + T * 0.32f, y - T * 0.2f, T * 0.3f, T * 0.3f);
        }

        static void Sofa(Graphics g, float x, float y, float w, float h, float T, Color c)
        {
            Shadow(g, x, y + h, w, T);
            RR(g, Dk(c, 0.2f), x + T * 0.05f, y - T * 0.05f, w - T * 0.1f, T * 0.5f, T * 0.12f);
            RR(g, c, x + T * 0.12f, y + T * 0.3f, w - T * 0.24f, h - T * 0.35f, T * 0.1f);
            RR(g, Dk(c, 0.28f), x, y + T * 0.15f, T * 0.22f, h - T * 0.15f, T * 0.08f);
            RR(g, Dk(c, 0.28f), x + w - T * 0.22f, y + T * 0.15f, T * 0.22f, h - T * 0.15f, T * 0.08f);
            for (float k = x + T; k < x + w - T * 0.5f; k += T) R(g, Dk(c, 0.15f), k, y + T * 0.35f, 1.5f, h - T * 0.45f);
        }

        static void Crate(Graphics g, float x, float y, float w, float h, float T, Color c)
        {
            Shadow(g, x, y + h, w, T);
            R(g, Lt(c, 0.25f), x + T * 0.03f, y - T * 0.38f, w - T * 0.06f, T * 0.2f);
            R(g, c, x + T * 0.03f, y - T * 0.18f, w - T * 0.06f, h + T * 0.12f);
            for (float k = x + T * 0.15f; k < x + w - T * 0.1f; k += T * 0.17f) R(g, Dk(c, 0.18f), k, y - T * 0.15f, T * 0.04f, h + T * 0.05f);
            R(g, Dk(c, 0.3f), x + w - T * 0.35f, y - T * 0.12f, T * 0.04f, h);
            R(g, Dk(c, 0.3f), x + w - T * 0.25f, y - T * 0.12f, T * 0.04f, h);
            RR(g, A(Color.White, 220), x + T * 0.2f, y + T * 0.05f, T * 0.5f, T * 0.25f, T * 0.04f);
        }

        static void Rack(Graphics g, float x, float y, float T, float t, int seed)
        {
            Shadow(g, x + T * 0.05f, y + T, T * 0.9f, T);
            R(g, Hx(0x3B4558), x + T * 0.06f, y - T * 0.95f, T * 0.88f, T * 1.9f);
            R(g, Hx(0x1C2230), x + T * 0.1f, y - T * 0.88f, T * 0.8f, T * 1.78f);
            for (int i = 0; i < 7; i++)
            {
                float sy = y - T * 0.82f + i * T * 0.25f;
                R(g, Hx(0x2B3446), x + T * 0.14f, sy, T * 0.72f, T * 0.18f);
                bool on = ((int)(t * 3 + seed * 7 + i * 13) % 5) != 0;
                Color led = (i + seed) % 3 == 0 ? Hx(0x58A6FF) : Hx(0x3FB950);
                if (on) R(g, led, x + T * 0.2f, sy + T * 0.06f, T * 0.06f, T * 0.06f);
                if ((int)(t * 5 + seed + i) % 3 == 0) R(g, Hx(0xF0B429), x + T * 0.3f, sy + T * 0.06f, T * 0.05f, T * 0.06f);
                R(g, Hx(0x11151D), x + T * 0.45f, sy + T * 0.05f, T * 0.36f, T * 0.08f);
            }
        }

        static void Table(Graphics g, float x, float y, float w, float h, float T, Color c)
        {
            Shadow(g, x, y + h, w, T);
            RR(g, c, x + T * 0.05f, y + T * 0.05f, w - T * 0.1f, h - T * 0.25f, T * 0.12f);
            RR(g, Lt(c, 0.12f), x + T * 0.15f, y + T * 0.12f, w - T * 0.3f, h - T * 0.42f, T * 0.1f);
            R(g, Dk(c, 0.3f), x + T * 0.1f, y + h - T * 0.22f, w - T * 0.2f, T * 0.18f);
            if (w > 3 * T)
                for (float k = x + T * 0.7f; k < x + w - T * 0.5f; k += T * 1.3f) Laptop(g, k, y + T * 0.45f, T);
        }

        static void Laptop(Graphics g, float cx, float cy, float T)
        {
            R(g, Hx(0x3A3F4B), cx - T * 0.22f, cy - T * 0.25f, T * 0.44f, T * 0.28f);
            R(g, Hx(0x58A6FF), cx - T * 0.18f, cy - T * 0.22f, T * 0.36f, T * 0.2f);
            R(g, Hx(0xB8C0CC), cx - T * 0.25f, cy + T * 0.03f, T * 0.5f, T * 0.1f);
        }

        static void LabTable(Graphics g, float x, float y, float w, float h, float T)
        {
            Shadow(g, x, y + h, w, T);
            R(g, Hx(0xE6EDF3), x + T * 0.04f, y + T * 0.08f, w - T * 0.08f, h - T * 0.28f);
            R(g, Hx(0x9AA7B5), x + T * 0.04f, y + h - T * 0.22f, w - T * 0.08f, T * 0.18f);
            var cols = new[] { 0x39C5CF, 0xD65AA0, 0x3FB950, 0xF0B429 };
            for (int i = 0; i < (int)(w / T) * 2 - 1; i++)
            {
                float bx = x + T * 0.3f + i * T * 0.45f;
                R(g, A(Color.White, 200), bx + T * 0.06f, y - T * 0.12f, T * 0.08f, T * 0.15f);
                E(g, A(Hx(cols[i % 4]), 210), bx, y, T * 0.2f, T * 0.24f);
            }
        }

        static void Beanbag(Graphics g, float x, float y, float T, Color c)
        {
            Shadow(g, x + T * 0.05f, y + T, T * 0.9f, T);
            E(g, c, x + T * 0.05f, y + T * 0.15f, T * 0.9f, T * 0.8f);
            E(g, Lt(c, 0.25f), x + T * 0.2f, y + T * 0.22f, T * 0.4f, T * 0.3f);
            E(g, Dk(c, 0.2f), x + T * 0.35f, y + T * 0.45f, T * 0.4f, T * 0.25f);
        }

        static void PingPong(Graphics g, float x, float y, float w, float h, float T)
        {
            Shadow(g, x, y + h, w, T);
            R(g, Hx(0x1F7A4D), x + T * 0.05f, y + T * 0.05f, w - T * 0.1f, h - T * 0.3f);
            g.DrawRectangle(P(Color.White, T * 0.03f), x + T * 0.1f, y + T * 0.1f, w - T * 0.2f, h - T * 0.4f);
            g.DrawLine(P(Color.White, T * 0.02f), x + T * 0.1f, y + (h - T * 0.25f) / 2, x + w - T * 0.1f, y + (h - T * 0.25f) / 2);
            R(g, Hx(0x2B2D42), x + w / 2 - T * 0.03f, y - T * 0.05f, T * 0.06f, h - T * 0.15f);
            R(g, Hx(0x155C39), x + T * 0.05f, y + h - T * 0.25f, w - T * 0.1f, T * 0.18f);
            E(g, Hx(0xFF9F1C), x + w * 0.3f, y + T * 0.5f, T * 0.1f, T * 0.1f);
        }

        static void Shelf(Graphics g, float x, float y, float w, float h, float T, Color c)
        {
            Shadow(g, x, y + h, w, T);
            R(g, c, x + T * 0.05f, y - T * 0.75f, w - T * 0.1f, h + T * 0.7f);
            var books = new[] { 0xE63946, 0x457B9D, 0xF4A261, 0x2A9D8F, 0xE9C46A, 0x8E6CC4 };
            int rows = (int)((h + T * 0.6f) / (T * 0.45f));
            for (int r = 0; r < rows; r++)
            {
                float ry = y - T * 0.68f + r * T * 0.45f;
                R(g, Dk(c, 0.35f), x + T * 0.1f, ry, w - T * 0.2f, T * 0.38f);
                for (int b = 0; b < 5; b++)
                    R(g, Hx(books[(r * 3 + b) % books.Length]), x + T * 0.13f + b * (w - T * 0.26f) / 5, ry + T * (0.06f + (b % 2) * 0.04f), (w - T * 0.3f) / 5, T * (0.32f - (b % 2) * 0.04f));
            }
        }

        static void Coffee(Graphics g, float x, float y, float T, float t)
        {
            Shadow(g, x + T * 0.1f, y + T, T * 0.8f, T);
            RR(g, Hx(0x59606B), x + T * 0.12f, y - T * 0.75f, T * 0.76f, T * 1.68f, T * 0.08f);
            R(g, Hx(0x1F2329), x + T * 0.2f, y - T * 0.62f, T * 0.6f, T * 0.45f);
            R(g, Hx(0x7EE787), x + T * 0.26f, y - T * 0.56f, T * 0.3f, T * 0.1f);
            E(g, Hx(0xF85149), x + T * 0.62f, y - T * 0.56f, T * 0.1f, T * 0.1f);
            R(g, Hx(0x2B2F36), x + T * 0.28f, y - T * 0.05f, T * 0.44f, T * 0.45f);
            RR(g, Color.White, x + T * 0.4f, y + T * 0.18f, T * 0.2f, T * 0.2f, T * 0.04f);
            for (int i = 0; i < 2; i++)
            {
                float sx = x + T * (0.45f + i * 0.08f), ph = (float)Math.Sin(t * 3 + i);
                g.DrawBezier(P(A(Color.White, 140), T * 0.025f), sx, y + T * 0.15f, sx + ph * T * 0.06f, y, sx - ph * T * 0.06f, y - T * 0.08f, sx, y - T * 0.2f);
            }
            using (var f = new Font("Segoe UI Black", T * 0.14f, GraphicsUnit.Pixel))
                Text(g, "CAFÉ", f, Hx(0xF4A261), new RectangleF(x + T * 0.12f, y - T * 0.15f, T * 0.76f, T * 0.14f));
        }

        static void Cooler(Graphics g, float x, float y, float T)
        {
            Shadow(g, x + T * 0.2f, y + T, T * 0.6f, T);
            R(g, Hx(0xE9EEF3), x + T * 0.25f, y + T * 0.05f, T * 0.5f, T * 0.9f);
            R(g, Hx(0xC5CED9), x + T * 0.25f, y + T * 0.05f, T * 0.5f, T * 0.06f);
            E(g, A(Hx(0x8CCBF2), 220), x + T * 0.26f, y - T * 0.5f, T * 0.48f, T * 0.62f);
            E(g, A(Color.White, 120), x + T * 0.33f, y - T * 0.4f, T * 0.12f, T * 0.3f);
            R(g, Hx(0x2496ED), x + T * 0.36f, y + T * 0.3f, T * 0.08f, T * 0.08f);
            R(g, Hx(0xF85149), x + T * 0.56f, y + T * 0.3f, T * 0.08f, T * 0.08f);
        }

        static void AirCon(Graphics g, float x, float y, float w, float h, float T)
        {
            Shadow(g, x, y + h, w, T);
            R(g, Hx(0xE9EEF3), x + T * 0.05f, y - T * 0.5f, w - T * 0.1f, h + T * 0.45f);
            for (float k = y - T * 0.35f; k < y + h - T * 0.2f; k += T * 0.15f) R(g, Hx(0xAEB9C7), x + T * 0.15f, k, w - T * 0.3f, T * 0.05f);
            E(g, Hx(0x58A6FF), x + w / 2 - T * 0.05f, y - T * 0.45f, T * 0.1f, T * 0.1f);
        }

        // ───────────────────────── personagens ─────────────────────────
        // (x, y) = posição dos pés. facing: 0 baixo, 1 esquerda, 2 direita, 3 cima.
        // ───────────────────────── personagem (proporções realistas) ─────────────────────────
        // (x, y) = posição dos pés; altura total = u * 1.5. facing: 0 frente, 1 esquerda, 2 direita, 3 costas.
        static void Quad(Graphics g, Color c, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            g.FillPolygon(B(c), new[] { new PointF(x1, y1), new PointF(x2, y2), new PointF(x3, y3), new PointF(x4, y4) });
        }

        // membro (perna/braço) como um quadrilátero que afina, com faixa de sombra de um lado
        static void Limb(Graphics g, float ax, float ay, float bx, float by, float wa, float wb, Color c, Color shade)
        {
            Quad(g, c, ax - wa / 2, ay, ax + wa / 2, ay, bx + wb / 2, by, bx - wb / 2, by);
            Quad(g, shade, ax + wa * 0.18f, ay, ax + wa / 2, ay, bx + wb / 2, by, bx + wb * 0.18f, by);
        }

        public static void Character(Graphics g, float x, float y, float u, Look L, int facing, float walk, bool moving, float t)
        {
            float H = u * 1.5f;
            float sw = moving ? (float)Math.Sin(walk) : 0f;
            float bob = moving ? Math.Abs((float)Math.Cos(walk)) * H * 0.03f : 0f;   // corpo sobe na passagem e desce no apoio
            bool side = facing == 1 || facing == 2, back = facing == 3;
            float dir = facing == 1 ? -1f : 1f;
            string outfit = L.Outfit ?? "", acc = L.Acc ?? "";

            Color skin = L.Skin, skinDk = Dk(skin, 0.2f);
            Color top = L.Shirt;
            switch (outfit)
            {
                case "moletom": top = Docker; break;
                case "jaleco": top = Hx(0xF1F5F9); break;
                case "terno": top = Hx(0x1F2937); break;
                case "havaiana": top = Hx(0xF4A261); break;
                case "polo_noc": top = Hx(0x1B1F24); break;
                case "camiseta_python": top = Hx(0x3776AB); break;
                case "camiseta_php": top = Hx(0x777BB4); break;
                case "jaqueta": top = Hx(0x3B2A20); break;
                case "moletom_mic": top = Hx(0x8A929E); break;
            }
            Color topDk = Dk(top, 0.24f), topLt = Lt(top, 0.14f);
            Color pants = L.Pants, pantsDk = Dk(pants, 0.3f);
            Color shoe = Hx(0x2A2522), sole = Hx(0x7A7068);
            Color hair = L.Hair, hairDk = Dk(hair, 0.32f), hairLt = Lt(hair, 0.2f);
            bool shortSleeve = outfit == "havaiana" || outfit == "polo_noc" || outfit == "camiseta_python" || outfit == "camiseta_php";
            bool hoodie = outfit == "moletom" || outfit == "moletom_mic";

            if (DrawShadows) E(g, A(Color.Black, 60), x - H * 0.17f, y - H * 0.035f, H * 0.34f, H * 0.07f);

            float yAnkle = y - H * 0.04f;
            float yHip = y - H * 0.50f - bob;
            float ySh = y - H * 0.815f - bob;
            float headH = H * 0.17f, headW = H * 0.142f;
            float hTop = y - H - bob, hcx = x + (side ? dir * H * 0.012f : 0), hcy = hTop + headH * 0.52f;
            bool fem = L.Female, skirt = L.Bottom == 1;
            float W1 = side ? (fem ? H * 0.155f : H * 0.17f) : (fem ? H * 0.265f : H * 0.31f), W2 = side ? (fem ? H * 0.16f : H * 0.15f) : (fem ? H * 0.25f : H * 0.235f);
            Color legC = skirt ? skin : pants, legD = skirt ? skinDk : pantsDk;   // de saia, as pernas aparecem abaixo dela
            float tcx = side ? x - dir * H * 0.008f : x;

            // ── mochila (atrás do corpo de lado; nas costas)
            Color bag = Hx(0x8B5E3C);
            if (acc == "mochila" && side) RR(g, bag, tcx - dir * (W1 / 2 + H * 0.07f) - H * 0.05f, ySh + H * 0.03f, H * 0.1f, H * 0.22f, H * 0.03f);

            // ── pernas
            if (!side)
            {
                float lw = H * (fem ? 0.072f : 0.082f), gap = H * 0.014f;
                float lift1 = Math.Max(0, sw) * H * 0.13f, lift2 = Math.Max(0, -sw) * H * 0.13f;   // pé que avança sobe bem visível
                float lx = x - gap / 2 - lw / 2, rx = x + gap / 2 + lw / 2;
                Limb(g, lx, yHip, lx, yAnkle - lift1, lw, lw * 0.82f, legC, legD);
                Limb(g, rx, yHip, rx, yAnkle - lift2, lw, lw * 0.82f, legC, legD);
                foreach (var f in new[] { new PointF(lx, yAnkle - lift1), new PointF(rx, yAnkle - lift2) })
                {
                    RR(g, shoe, f.X - lw * 0.62f, f.Y - H * 0.012f, lw * 1.24f, H * 0.048f, H * 0.018f);
                    R(g, sole, f.X - lw * 0.6f, f.Y + H * 0.03f, lw * 1.2f, Math.Max(1, H * 0.008f));
                }
            }
            else
            {
                float lw = H * (fem ? 0.075f : 0.085f), stride = sw * H * 0.16f;
                float backFoot = x - dir * stride, frontFoot = x + dir * stride;
                Limb(g, x - dir * H * 0.01f, yHip, backFoot, yAnkle - Math.Max(0, sw) * H * 0.06f, lw, lw * 0.8f, legD, Dk(legD, 0.2f));
                RR(g, Dk(shoe, 0.1f), backFoot - lw * 0.45f + (dir > 0 ? 0 : -lw * 0.45f), yAnkle - Math.Max(0, sw) * H * 0.06f - H * 0.012f, lw * 1.45f, H * 0.046f, H * 0.018f);
                Limb(g, x + dir * H * 0.01f, yHip, frontFoot, yAnkle - Math.Max(0, -sw) * H * 0.06f, lw, lw * 0.8f, legC, legD);
                RR(g, shoe, frontFoot - lw * 0.45f + (dir > 0 ? 0 : -lw * 0.45f), yAnkle - Math.Max(0, -sw) * H * 0.06f - H * 0.012f, lw * 1.45f, H * 0.046f, H * 0.018f);
            }

            // ── braço de trás (de lado)
            float armW = H * 0.064f, handY = ySh + H * 0.335f;
            Action<float, float, bool, float> arm = (shx, hx, dark, dy) =>
            {
                Color sc = dark ? topDk : top, sd = dark ? Dk(topDk, 0.15f) : topDk;
                float hy = handY + dy;
                if (shortSleeve)
                {
                    float mx = shx + (hx - shx) * 0.42f, my = ySh + H * 0.02f + (hy - ySh) * 0.42f;
                    Limb(g, shx, ySh + H * 0.02f, mx, my, armW, armW * 0.95f, sc, sd);
                    Limb(g, mx, my, hx, hy, armW * 0.78f, armW * 0.7f, dark ? skinDk : skin, Dk(skin, 0.3f));
                }
                else Limb(g, shx, ySh + H * 0.02f, hx, hy, armW, armW * 0.82f, sc, sd);
                E(g, dark ? skinDk : skin, hx - armW * 0.5f, hy - H * 0.01f, armW, H * 0.065f);
            };
            if (side) arm(tcx, tcx + dir * sw * H * 0.15f, true, -Math.Abs(sw) * H * 0.02f);

            // ── cabelo longo/black power atrás do corpo (frente e lado): tronco e pescoço cobrem o meio
            if (!back)
            {
                if (L.HairStyle == 1) RR(g, hairDk, hcx - headW * 0.62f, hcy - headH * 0.4f, headW * 1.24f, headH * 1.45f, headW * 0.3f);
                if (L.HairStyle == 4) E(g, hair, hcx - headW * 0.85f, hTop - headH * 0.22f, headW * 1.7f, headH * 1.2f);
                if (L.HairStyle == 8) RR(g, hairDk, hcx - headW * 0.62f, hcy - headH * 0.42f, headW * 1.24f, headH * 1.0f, headW * 0.32f);
                if (L.HairStyle == 6 && side)
                {
                    // rabo de cavalo balança um pouco ao andar
                    float sway = sw * headW * 0.08f;
                    RR(g, hair, hcx - dir * headW * 0.6f - headW * 0.13f + sway, hcy - headH * 0.32f, headW * 0.26f, headH * 0.95f, headW * 0.13f);
                    R(g, L.Accent, hcx - dir * headW * 0.52f - headW * 0.1f, hcy - headH * 0.34f, headW * 0.2f, Math.Max(1, headH * 0.07f));
                }
                if (L.HairStyle == 7 && side) Braid(g, hair, hairDk, hcx - dir * headW * 0.38f, hcy - headH * 0.05f, headW * 0.2f, headH * 0.24f, 4, L.Accent);
            }

            // ── capuz (moletom) atrás do pescoço
            if (hoodie && !back) E(g, Dk(top, 0.3f), x - H * 0.1f, ySh - H * 0.055f, H * 0.2f, H * 0.12f);

            // ── tronco
            RR(g, top, tcx - W1 / 2, ySh, W1, H * 0.09f, H * 0.035f);
            if (fem && !side)
            {
                float yW = ySh + (yHip - ySh) * 0.62f, Ww = H * 0.205f;
                Quad(g, top, tcx - W1 / 2, ySh + H * 0.04f, tcx + W1 / 2, ySh + H * 0.04f, tcx + Ww / 2, yW, tcx - Ww / 2, yW);
                Quad(g, top, tcx - Ww / 2, yW, tcx + Ww / 2, yW, tcx + W2 / 2, yHip, tcx - W2 / 2, yHip);
            }
            else Quad(g, top, tcx - W1 / 2, ySh + H * 0.04f, tcx + W1 / 2, ySh + H * 0.04f, tcx + W2 / 2, yHip, tcx - W2 / 2, yHip);
            if (!side) Quad(g, topDk, tcx + W1 * 0.28f, ySh + H * 0.04f, tcx + W1 / 2, ySh + H * 0.04f, tcx + W2 / 2, yHip, tcx + W2 * 0.28f, yHip);
            else Quad(g, topDk, tcx - dir * W1 / 2, ySh + H * 0.04f, tcx - dir * W1 * 0.2f, ySh + H * 0.04f, tcx - dir * W2 * 0.2f, yHip, tcx - dir * W2 / 2, yHip);
            if (!side) R(g, topLt, tcx - W1 * 0.42f, ySh + H * 0.03f, W1 * 0.3f, H * 0.02f);
            // cinto
            if (outfit != "jaleco" && !hoodie && outfit != "jaqueta")
            {
                R(g, Dk(pants, 0.45f), tcx - W2 / 2, yHip - H * 0.018f, W2, H * 0.022f);
                if (!side && !back) R(g, Hx(0xC9A227), tcx - H * 0.012f, yHip - H * 0.017f, H * 0.024f, H * 0.02f);
            }
            // saia (a cor é a mesma escolhida para a calça)
            if (skirt)
            {
                float sTop = yHip - H * 0.03f, sBot = y - H * 0.25f;
                if (!side)
                {
                    Quad(g, pants, tcx - W2 / 2 - H * 0.004f, sTop, tcx + W2 / 2 + H * 0.004f, sTop, tcx + W2 * 0.74f, sBot, tcx - W2 * 0.74f, sBot);
                    Quad(g, pantsDk, tcx + W2 * 0.18f, sTop, tcx + W2 / 2 + H * 0.004f, sTop, tcx + W2 * 0.74f, sBot, tcx + W2 * 0.3f, sBot);
                    R(g, Dk(pants, 0.45f), tcx - W2 * 0.74f, sBot - Math.Max(1, H * 0.012f), W2 * 1.48f, Math.Max(1, H * 0.012f));
                }
                else
                {
                    float fl = sw * H * 0.03f;
                    Quad(g, pants, tcx - W2 / 2, sTop, tcx + W2 / 2, sTop, tcx + W2 * 0.78f + fl, sBot, tcx - W2 * 0.78f + fl, sBot);
                    Quad(g, pantsDk, tcx - dir * W2 / 2, sTop, tcx - dir * W2 * 0.1f, sTop, tcx - dir * W2 * 0.2f + fl, sBot, tcx - dir * W2 * 0.78f + fl, sBot);
                }
            }
            // jaleco: abas sobre as coxas
            if (outfit == "jaleco")
            {
                if (!side)
                {
                    Quad(g, top, tcx - W2 / 2 - H * 0.01f, yHip - H * 0.02f, tcx - H * 0.02f, yHip - H * 0.02f, tcx - H * 0.025f, y - H * 0.24f, tcx - W2 / 2 - H * 0.025f, y - H * 0.24f);
                    Quad(g, Dk(top, 0.1f), tcx + H * 0.02f, yHip - H * 0.02f, tcx + W2 / 2 + H * 0.01f, yHip - H * 0.02f, tcx + W2 / 2 + H * 0.025f, y - H * 0.24f, tcx + H * 0.025f, y - H * 0.24f);
                }
                else Quad(g, top, tcx - W2 / 2, yHip - H * 0.02f, tcx + W2 / 2, yHip - H * 0.02f, tcx + W2 / 2 + dir * H * 0.01f, y - H * 0.24f, tcx - W2 / 2, y - H * 0.24f);
            }

            // ── detalhes da frente da roupa
            if (!back && !side)
            {
                switch (outfit)
                {
                    case "jaleco":
                        Quad(g, L.Shirt, tcx - H * 0.035f, ySh + H * 0.01f, tcx + H * 0.035f, ySh + H * 0.01f, tcx + H * 0.025f, yHip, tcx - H * 0.025f, yHip);
                        R(g, Hx(0xCBD5E1), tcx + W1 * 0.18f, ySh + H * 0.14f, H * 0.05f, H * 0.045f);
                        R(g, Hx(0x2496ED), tcx + W1 * 0.2f, ySh + H * 0.125f, H * 0.008f, H * 0.03f);
                        break;
                    case "terno":
                        g.FillPolygon(B(Color.White), new[] { new PointF(tcx - H * 0.06f, ySh), new PointF(tcx + H * 0.06f, ySh), new PointF(tcx, ySh + H * 0.17f) });
                        g.FillPolygon(B(Hx(0xD62828)), new[] { new PointF(tcx - H * 0.015f, ySh + H * 0.01f), new PointF(tcx + H * 0.015f, ySh + H * 0.01f),
                            new PointF(tcx + H * 0.022f, ySh + H * 0.15f), new PointF(tcx, ySh + H * 0.18f), new PointF(tcx - H * 0.022f, ySh + H * 0.15f) });
                        E(g, Hx(0x9AA5B6), tcx + H * 0.03f, ySh + H * 0.2f, H * 0.018f, H * 0.018f);
                        E(g, Hx(0x9AA5B6), tcx + H * 0.03f, ySh + H * 0.25f, H * 0.018f, H * 0.018f);
                        break;
                    case "havaiana":
                        var fl = new[] { 0xFFFFFF, 0xFF70A6, 0xFFE66D, 0x2A9D8F };
                        for (int i = 0; i < 6; i++)
                            E(g, Hx(fl[i % 4]), tcx - W1 * 0.36f + (i % 3) * W1 * 0.3f, ySh + H * (0.06f + (i / 3) * 0.13f), H * 0.035f, H * 0.035f);
                        g.FillPolygon(B(skin), new[] { new PointF(tcx - H * 0.04f, ySh), new PointF(tcx + H * 0.04f, ySh), new PointF(tcx, ySh + H * 0.06f) });
                        break;
                    case "polo_noc":
                        g.FillPolygon(B(Hx(0x2D333B)), new[] { new PointF(tcx - H * 0.06f, ySh), new PointF(tcx, ySh + H * 0.05f), new PointF(tcx - H * 0.02f, ySh) });
                        g.FillPolygon(B(Hx(0x2D333B)), new[] { new PointF(tcx + H * 0.06f, ySh), new PointF(tcx, ySh + H * 0.05f), new PointF(tcx + H * 0.02f, ySh) });
                        R(g, Hx(0x3FB950), tcx - W1 * 0.32f, ySh + H * 0.08f, H * 0.04f, H * 0.03f);
                        break;
                    case "camiseta_python":
                        g.FillPolygon(B(skin), new[] { new PointF(tcx - H * 0.035f, ySh), new PointF(tcx + H * 0.035f, ySh), new PointF(tcx, ySh + H * 0.035f) });
                        RR(g, Hx(0xFFD43B), tcx - H * 0.045f, ySh + H * 0.08f, H * 0.05f, H * 0.05f, H * 0.015f);
                        RR(g, Hx(0xF1F5F9), tcx - H * 0.005f, ySh + H * 0.115f, H * 0.05f, H * 0.05f, H * 0.015f);
                        break;
                    case "camiseta_php":
                        g.FillPolygon(B(skin), new[] { new PointF(tcx - H * 0.035f, ySh), new PointF(tcx + H * 0.035f, ySh), new PointF(tcx, ySh + H * 0.035f) });
                        E(g, Hx(0x4F5B93), tcx - H * 0.06f, ySh + H * 0.08f, H * 0.12f, H * 0.075f);
                        E(g, Hx(0xF1F5F9), tcx - H * 0.035f, ySh + H * 0.095f, H * 0.015f, H * 0.015f);
                        break;
                    case "jaqueta":
                        R(g, L.Shirt, tcx - H * 0.04f, ySh + H * 0.01f, H * 0.08f, yHip - ySh - H * 0.01f);
                        g.FillPolygon(B(Hx(0x2A1D16)), new[] { new PointF(tcx - H * 0.04f, ySh), new PointF(tcx - H * 0.1f, ySh), new PointF(tcx - H * 0.04f, ySh + H * 0.12f) });
                        g.FillPolygon(B(Hx(0x2A1D16)), new[] { new PointF(tcx + H * 0.04f, ySh), new PointF(tcx + H * 0.1f, ySh), new PointF(tcx + H * 0.04f, ySh + H * 0.12f) });
                        R(g, Hx(0xB8C0CC), tcx + H * 0.042f, ySh + H * 0.13f, Math.Max(1, H * 0.006f), yHip - ySh - H * 0.14f);
                        break;
                    case "moletom_mic":
                        RR(g, Dk(top, 0.15f), tcx - W2 * 0.38f, ySh + H * 0.19f, W2 * 0.76f, H * 0.08f, H * 0.02f);
                        R(g, Color.White, tcx - H * 0.03f, ySh + H * 0.01f, Math.Max(1, H * 0.008f), H * 0.08f);
                        R(g, Color.White, tcx + H * 0.025f, ySh + H * 0.01f, Math.Max(1, H * 0.008f), H * 0.08f);
                        {
                            float q = H * 0.024f, qx = tcx - q - H * 0.002f, qy = ySh + H * 0.095f;
                            R(g, Hx(0xF25022), qx, qy, q, q); R(g, Hx(0x7FBA00), qx + q + H * 0.004f, qy, q, q);
                            R(g, Hx(0x00A4EF), qx, qy + q + H * 0.004f, q, q); R(g, Hx(0xFFB900), qx + q + H * 0.004f, qy + q + H * 0.004f, q, q);
                        }
                        break;
                    case "moletom":
                        RR(g, Dk(Docker, 0.15f), tcx - W2 * 0.38f, ySh + H * 0.19f, W2 * 0.76f, H * 0.08f, H * 0.02f);
                        R(g, Color.White, tcx - H * 0.03f, ySh + H * 0.01f, Math.Max(1, H * 0.008f), H * 0.08f);
                        R(g, Color.White, tcx + H * 0.025f, ySh + H * 0.01f, Math.Max(1, H * 0.008f), H * 0.08f);
                        Whale(g, tcx, ySh + H * 0.14f, H * 0.11f, Color.White);
                        break;
                    default:
                        // gola
                        g.FillPolygon(B(skin), new[] { new PointF(tcx - H * 0.035f, ySh), new PointF(tcx + H * 0.035f, ySh), new PointF(tcx, ySh + H * 0.035f) });
                        if (!L.Badge && L.Accent != L.Shirt) R(g, L.Accent, tcx - W1 / 2 + H * 0.01f, ySh + H * 0.11f, W1 - H * 0.02f, H * 0.03f);
                        break;
                }
                if (L.Badge && acc != "cracha" && outfit != "terno")
                {
                    R(g, Color.White, tcx - W1 * 0.33f, ySh + H * 0.08f, H * 0.045f, H * 0.055f);
                    R(g, Docker, tcx - W1 * 0.33f, ySh + H * 0.08f, H * 0.045f, H * 0.016f);
                }
                if (acc == "cracha")
                {
                    R(g, Hx(0xF5C542), tcx - W1 * 0.34f, ySh + H * 0.075f, H * 0.05f, H * 0.062f);
                    R(g, Color.White, tcx - W1 * 0.32f, ySh + H * 0.11f, H * 0.035f, Math.Max(1, H * 0.01f));
                }
                if (acc == "gravata")
                {
                    float gy = ySh + H * 0.012f;
                    g.FillPolygon(B(Hx(0xD62828)), new[] { new PointF(tcx, gy), new PointF(tcx - H * 0.045f, gy - H * 0.022f), new PointF(tcx - H * 0.045f, gy + H * 0.022f) });
                    g.FillPolygon(B(Hx(0xD62828)), new[] { new PointF(tcx, gy), new PointF(tcx + H * 0.045f, gy - H * 0.022f), new PointF(tcx + H * 0.045f, gy + H * 0.022f) });
                    E(g, Hx(0x9B1C1C), tcx - H * 0.012f, gy - H * 0.012f, H * 0.024f, H * 0.024f);
                }
                if (acc == "mochila")
                {
                    R(g, Dk(bag, 0.25f), tcx - W1 * 0.36f, ySh + H * 0.01f, H * 0.025f, H * 0.22f);
                    R(g, Dk(bag, 0.25f), tcx + W1 * 0.36f - H * 0.025f, ySh + H * 0.01f, H * 0.025f, H * 0.22f);
                }
            }
            if (back && acc == "mochila")
            {
                RR(g, bag, tcx - W1 * 0.36f, ySh + H * 0.03f, W1 * 0.72f, H * 0.26f, H * 0.035f);
                RR(g, Dk(bag, 0.15f), tcx - W1 * 0.26f, ySh + H * 0.16f, W1 * 0.52f, H * 0.1f, H * 0.02f);
            }

            // ── braços (frente/costas) e braço da frente (lado)
            if (!side)
            {
                // braços balançam ao contrário das pernas: o que vai para a frente encurta (mão sobe)
                arm(tcx - W1 / 2 + armW * 0.35f, tcx - W1 / 2 - H * 0.012f + Math.Max(0, -sw) * H * 0.012f, false, -Math.Max(0, -sw) * H * 0.07f + Math.Max(0, sw) * H * 0.015f);
                arm(tcx + W1 / 2 - armW * 0.35f, tcx + W1 / 2 + H * 0.012f - Math.Max(0, sw) * H * 0.012f, false, -Math.Max(0, sw) * H * 0.07f + Math.Max(0, -sw) * H * 0.015f);
            }
            else arm(tcx, tcx - dir * sw * H * 0.15f, false, -Math.Abs(sw) * H * 0.02f);

            // ── pescoço
            R(g, skinDk, hcx - H * 0.032f, ySh - H * 0.04f, H * 0.064f, H * 0.055f);

            // ── cabelo de trás
            if (back && L.HairStyle == 1) RR(g, hair, hcx - headW * 0.62f, hcy - headH * 0.4f, headW * 1.24f, headH * 1.55f, headW * 0.3f);
            if (back && L.HairStyle == 4) E(g, hair, hcx - headW * 0.85f, hTop - headH * 0.22f, headW * 1.7f, headH * 1.2f);

            // ── cabeça
            if (!side)
            {
                E(g, skinDk, hcx - headW * 0.6f, hcy - headH * 0.08f, headW * 0.2f, headH * 0.26f);
                E(g, skinDk, hcx + headW * 0.4f, hcy - headH * 0.08f, headW * 0.2f, headH * 0.26f);
            }
            E(g, skin, hcx - headW / 2, hTop, headW, headH);
            if (!back)
            {
                // sombra suave só na lateral do rosto (luz vindo da esquerda)
                using (var fp = new GraphicsPath())
                {
                    fp.AddEllipse(hcx - headW / 2, hTop, headW, headH);
                    var st = g.Save();
                    g.SetClip(fp, CombineMode.Intersect);
                    float sx = side ? (dir > 0 ? hcx - headW * 0.5f : hcx + headW * 0.32f) : hcx + headW * 0.3f;
                    R(g, A(skinDk, 80), sx, hTop, headW * 0.2f, headH);
                    g.Restore(st);
                }
            }
            if (side) E(g, skinDk, hcx - dir * headW * 0.12f - headW * 0.1f, hcy - headH * 0.08f, headW * 0.2f, headH * 0.26f);

            if (back)
            {
                E(g, hair, hcx - headW * 0.53f, hTop - headH * 0.05f, headW * 1.06f, headH * (L.HairStyle == 3 ? 0.75f : 1.0f));
                if (L.HairStyle == 2) E(g, hair, hcx - headW * 0.22f, hTop - headH * 0.12f, headW * 0.44f, headH * 0.36f);
                if (L.HairStyle == 8) RR(g, hair, hcx - headW * 0.6f, hTop + headH * 0.2f, headW * 1.2f, headH * 0.95f, headW * 0.3f);
                if (L.HairStyle == 6)
                {
                    RR(g, hair, hcx - headW * 0.13f + sw * headW * 0.06f, hcy - headH * 0.05f, headW * 0.26f, headH * 1.0f, headW * 0.13f);
                    R(g, L.Accent, hcx - headW * 0.12f, hcy - headH * 0.08f, headW * 0.24f, Math.Max(1, headH * 0.07f));
                }
                if (L.HairStyle == 7)
                    foreach (float sx in new[] { -1f, 1f }) Braid(g, hair, hairDk, hcx + sx * headW * 0.26f, hcy + headH * 0.05f, headW * 0.2f, headH * 0.24f, 4, L.Accent);
                if (L.HairStyle == 5) { using (var p = new GraphicsPath()) { p.AddPie(hcx - headW * 0.56f, hTop - headH * 0.1f, headW * 1.12f, headH * 1.0f, 180, 180); g.FillPath(B(L.Accent), p); } }
            }
            else
            {
                // rosto
                float ey = hcy + headH * 0.06f;
                if (!side)
                {
                    float ex = headW * 0.22f;
                    foreach (float s in new[] { -1f, 1f })
                    {
                        E(g, Color.White, hcx + s * ex - headW * 0.1f, ey - headH * 0.045f, headW * 0.2f, headH * 0.11f);
                        E(g, Hx(0x2B1B12), hcx + s * ex - headW * 0.05f, ey - headH * 0.04f, headW * 0.1f, headH * 0.1f);
                        R(g, hairDk, hcx + s * ex - headW * 0.11f, ey - headH * 0.15f, headW * 0.22f, Math.Max(1, headH * (fem ? 0.03f : 0.04f)));
                        if (fem)
                        {
                            R(g, Hx(0x1A1210), hcx + s * ex - headW * 0.11f, ey - headH * 0.065f, headW * 0.22f, Math.Max(1, headH * 0.035f));
                            R(g, Hx(0x1A1210), hcx + s * ex + (s > 0 ? headW * 0.08f : -headW * 0.13f), ey - headH * 0.095f, headW * 0.05f, Math.Max(1, headH * 0.035f));
                        }
                    }
                    R(g, Dk(skin, 0.28f), hcx - headW * 0.03f, ey + headH * 0.08f, headW * 0.07f, headH * 0.08f);
                    if (fem) RR(g, Hx(0xC2456B), hcx - headW * 0.11f, ey + headH * 0.225f, headW * 0.22f, Math.Max(2, headH * 0.06f), headH * 0.03f);
                    else R(g, Hx(0x9C4A4A), hcx - headW * 0.12f, ey + headH * 0.24f, headW * 0.24f, Math.Max(1, headH * 0.04f));
                }
                else
                {
                    E(g, skin, hcx + dir * headW * 0.38f - headW * 0.08f, ey - headH * 0.02f, headW * 0.18f, headH * 0.16f);
                    E(g, Color.White, hcx + dir * headW * 0.24f - headW * 0.08f, ey - headH * 0.045f, headW * 0.16f, headH * 0.11f);
                    E(g, Hx(0x2B1B12), hcx + dir * headW * 0.29f - headW * 0.045f, ey - headH * 0.04f, headW * 0.09f, headH * 0.1f);
                    R(g, hairDk, hcx + dir * headW * 0.24f - headW * 0.1f, ey - headH * 0.15f, headW * 0.2f, Math.Max(1, headH * 0.04f));
                    R(g, fem ? Hx(0xC2456B) : Hx(0x9C4A4A), hcx + dir * headW * 0.3f - (dir > 0 ? 0 : headW * 0.14f), ey + headH * 0.24f, headW * 0.14f, Math.Max(1, headH * (fem ? 0.055f : 0.04f)));
                    if (fem) R(g, Hx(0x1A1210), hcx + dir * headW * 0.24f - headW * 0.1f, ey - headH * 0.065f, headW * 0.2f, Math.Max(1, headH * 0.035f));
                }

                // cabelo da frente
                switch (L.HairStyle)
                {
                    case 3:
                        using (var p = new GraphicsPath()) { p.AddPie(hcx - headW * 0.52f, hTop - headH * 0.02f, headW * 1.04f, headH * 0.62f, 180, 180); g.FillPath(B(A(hair, 200)), p); }
                        break;
                    case 5:
                        using (var p = new GraphicsPath()) { p.AddPie(hcx - headW * 0.56f, hTop - headH * 0.1f, headW * 1.12f, headH * 1.0f, 180, 180); g.FillPath(B(L.Accent), p); }
                        if (!side) RR(g, Dk(L.Accent, 0.25f), hcx - headW * 0.5f, hTop + headH * 0.3f, headW * 1.0f, headH * 0.14f, headH * 0.06f);
                        else RR(g, Dk(L.Accent, 0.25f), hcx + (dir > 0 ? headW * 0.1f : -headW * 0.95f), hTop + headH * 0.3f, headW * 0.85f, headH * 0.13f, headH * 0.06f);
                        break;
                    default:
                        float hw = L.HairStyle == 4 ? 1.3f : L.HairStyle == 8 ? 1.2f : 1.1f;
                        using (var p = new GraphicsPath()) { p.AddPie(hcx - headW * hw / 2, hTop - headH * 0.07f, headW * hw, headH * 0.85f, 180, 180); g.FillPath(B(hair), p); }
                        if (!side)
                        {
                            Quad(g, hair, hcx - headW * 0.52f, hTop + headH * 0.3f, hcx + headW * 0.15f, hTop + headH * 0.3f, hcx - headW * 0.05f, hTop + headH * 0.42f, hcx - headW * 0.5f, hTop + headH * 0.5f);
                            R(g, hair, hcx - headW * 0.52f, hTop + headH * 0.3f, headW * 0.1f, headH * 0.32f);
                            R(g, hair, hcx + headW * 0.42f, hTop + headH * 0.3f, headW * 0.1f, headH * 0.32f);
                        }
                        else
                        {
                            Quad(g, hair, hcx - dir * headW * 0.55f, hTop + headH * 0.25f, hcx + dir * headW * 0.1f, hTop + headH * 0.25f, hcx - dir * headW * 0.05f, hcy + headH * 0.1f, hcx - dir * headW * 0.52f, hcy + headH * 0.25f);
                        }
                        E(g, A(hairLt, 150), hcx - headW * 0.36f, hTop + headH * 0.02f, headW * 0.42f, headH * 0.13f);
                        E(g, A(Lt(hair, 0.45f), 120), hcx - headW * 0.27f, hTop + headH * 0.045f, headW * 0.2f, headH * 0.06f);
                        if (L.HairStyle == 1)
                        {
                            if (!side) { R(g, hair, hcx - headW * 0.58f, hcy - headH * 0.15f, headW * 0.14f, headH * 0.72f); R(g, hair, hcx + headW * 0.44f, hcy - headH * 0.15f, headW * 0.14f, headH * 0.72f); }
                            else R(g, hair, hcx - dir * headW * 0.6f - (dir > 0 ? 0 : headW * 0.0f), hcy - headH * 0.2f, headW * 0.25f, headH * 0.9f);
                        }
                        if (L.HairStyle == 2) E(g, hair, hcx - dir * (side ? headW * 0.35f : 0) - headW * 0.22f, hTop - headH * 0.18f, headW * 0.44f, headH * 0.36f);
                        if (L.HairStyle == 8)
                        {
                            // chanel: cabelo na altura do queixo, cobrindo as laterais do rosto
                            if (!side) { RR(g, hair, hcx - headW * 0.62f, hcy - headH * 0.22f, headW * 0.2f, headH * 0.62f, headW * 0.08f); RR(g, hair, hcx + headW * 0.42f, hcy - headH * 0.22f, headW * 0.2f, headH * 0.62f, headW * 0.08f); }
                            else RR(g, hair, hcx - dir * headW * 0.6f - headW * 0.16f, hcy - headH * 0.28f, headW * 0.32f, headH * 0.68f, headW * 0.1f);
                        }
                        if (L.HairStyle == 7 && !side)
                            foreach (float sx in new[] { -1f, 1f }) Braid(g, hair, hairDk, hcx + sx * headW * 0.5f, hcy + headH * 0.12f, headW * 0.2f, headH * 0.24f, 4, L.Accent);
                        break;
                }

                // óculos
                if (L.Glasses || acc == "oculos_escuros")
                {
                    bool dark = acc == "oculos_escuros";
                    var gp = P(Hx(0x1B1B1F), Math.Max(1, H * 0.012f));
                    if (!side)
                        foreach (float s in new[] { -1f, 1f })
                        {
                            var rc = new RectangleF(hcx + s * headW * 0.22f - headW * 0.15f, ey - headH * 0.09f, headW * 0.3f, headH * 0.17f);
                            if (dark) g.FillRectangle(B(Hx(0x111318)), rc); else g.DrawRectangle(gp, rc.X, rc.Y, rc.Width, rc.Height);
                        }
                    else
                    {
                        var rc = new RectangleF(hcx + dir * headW * 0.26f - headW * 0.14f, ey - headH * 0.09f, headW * 0.28f, headH * 0.17f);
                        if (dark) g.FillRectangle(B(Hx(0x111318)), rc); else g.DrawRectangle(gp, rc.X, rc.Y, rc.Width, rc.Height);
                        g.DrawLine(gp, hcx + dir * headW * 0.12f, ey - headH * 0.04f, hcx - dir * headW * 0.15f, ey - headH * 0.06f);
                    }
                    if (!side) g.DrawLine(gp, hcx - headW * 0.07f, ey - headH * 0.02f, hcx + headW * 0.07f, ey - headH * 0.02f);
                }
            }

            // headset
            if (L.Headset)
            {
                g.DrawArc(P(Hx(0x2B2D42), Math.Max(1, H * 0.018f)), hcx - headW * 0.58f, hTop - headH * 0.08f, headW * 1.16f, headH * 1.0f, 180, 180);
                if (!side || dir < 0) RR(g, Hx(0x2B2D42), hcx - headW * 0.66f, hcy - headH * 0.12f, headW * 0.22f, headH * 0.32f, headW * 0.06f);
                if (!side || dir > 0) RR(g, Hx(0x2B2D42), hcx + headW * 0.44f, hcy - headH * 0.12f, headW * 0.22f, headH * 0.32f, headW * 0.06f);
                if (facing == 0) g.DrawLine(P(Hx(0x2B2D42), Math.Max(1, H * 0.01f)), hcx - headW * 0.55f, hcy + headH * 0.15f, hcx - headW * 0.18f, hcy + headH * 0.32f);
            }
            if (!string.IsNullOrEmpty(L.Hat)) Hat(g, L.Hat, hcx, hTop, headW, headH, facing);
        }

        static void Hat(Graphics g, string kind, float hcx, float hTop, float hw, float hh, int facing)
        {
            bool side = facing == 1 || facing == 2;
            float dir = facing == 1 ? -1 : 1;
            switch (kind)
            {
                case "bone":
                    using (var p = new GraphicsPath()) { p.AddPie(hcx - hw * 0.6f, hTop - hh * 0.12f, hw * 1.2f, hh * 1.0f, 180, 180); g.FillPath(B(Docker), p); }
                    if (facing == 0) { E(g, Dk(Docker, 0.3f), hcx - hw * 0.52f, hTop + hh * 0.28f, hw * 1.04f, hh * 0.2f); Whale(g, hcx, hTop + hh * 0.12f, hw * 0.45f, Color.White); }
                    else if (side) RR(g, Dk(Docker, 0.3f), hcx + (dir > 0 ? hw * 0.2f : -hw * 0.95f), hTop + hh * 0.3f, hw * 0.75f, hh * 0.14f, hh * 0.06f);
                    break;
                case "gorro":
                    using (var p = new GraphicsPath()) { p.AddPie(hcx - hw * 0.6f, hTop - hh * 0.18f, hw * 1.2f, hh * 1.15f, 180, 180); g.FillPath(B(Hx(0xE63946)), p); }
                    for (int i = 0; i < 4; i++) R(g, Hx(0xC1121F), hcx - hw * 0.4f + i * hw * 0.25f, hTop - hh * 0.06f, hw * 0.07f, hh * 0.4f);
                    RR(g, Hx(0xF1F5F9), hcx - hw * 0.6f, hTop + hh * 0.26f, hw * 1.2f, hh * 0.16f, hh * 0.06f);
                    E(g, Hx(0xF1F5F9), hcx - hw * 0.16f, hTop - hh * 0.32f, hw * 0.32f, hw * 0.32f);
                    break;
                case "capacete":
                    using (var p = new GraphicsPath()) { p.AddPie(hcx - hw * 0.66f, hTop - hh * 0.2f, hw * 1.32f, hh * 1.2f, 180, 180); g.FillPath(B(Hx(0xF0B429)), p); }
                    R(g, Hx(0xFFD166), hcx - hw * 0.06f, hTop - hh * 0.18f, hw * 0.12f, hh * 0.55f);
                    RR(g, Hx(0xC98A06), hcx - hw * 0.75f, hTop + hh * 0.34f, hw * 1.5f, hh * 0.12f, hh * 0.05f);
                    break;
                case "cowboy":
                    E(g, Hx(0x7A4A24), hcx - hw * 1.1f, hTop + hh * 0.12f, hw * 2.2f, hh * 0.38f);
                    RR(g, Hx(0x8B5A2B), hcx - hw * 0.5f, hTop - hh * 0.35f, hw * 1.0f, hh * 0.62f, hw * 0.2f);
                    R(g, Hx(0x3E2716), hcx - hw * 0.5f, hTop + hh * 0.12f, hw * 1.0f, hh * 0.1f);
                    break;
                case "coroa":
                    var pts = new[] {
                        new PointF(hcx - hw * 0.5f, hTop + hh * 0.18f), new PointF(hcx - hw * 0.55f, hTop - hh * 0.32f), new PointF(hcx - hw * 0.27f, hTop - hh * 0.08f),
                        new PointF(hcx, hTop - hh * 0.42f), new PointF(hcx + hw * 0.27f, hTop - hh * 0.08f), new PointF(hcx + hw * 0.55f, hTop - hh * 0.32f),
                        new PointF(hcx + hw * 0.5f, hTop + hh * 0.18f) };
                    g.FillPolygon(B(Hx(0xF5C542)), pts);
                    E(g, Hx(0xE63946), hcx - hw * 0.08f, hTop - hh * 0.04f, hw * 0.16f, hw * 0.16f);
                    E(g, Hx(0x2496ED), hcx - hw * 0.38f, hTop - hh * 0.0f, hw * 0.12f, hw * 0.12f);
                    E(g, Hx(0x2A9D8F), hcx + hw * 0.26f, hTop - hh * 0.0f, hw * 0.12f, hw * 0.12f);
                    break;
                case "bone_react":
                    using (var p = new GraphicsPath()) { p.AddPie(hcx - hw * 0.6f, hTop - hh * 0.12f, hw * 1.2f, hh * 1.0f, 180, 180); g.FillPath(B(Hx(0x20232A)), p); }
                    if (facing == 0)
                    {
                        E(g, Hx(0x61DAFB), hcx - hw * 0.52f, hTop + hh * 0.28f, hw * 1.04f, hh * 0.2f);
                        using (var pen = new Pen(Hx(0x61DAFB), Math.Max(1, hw * 0.05f)))   // caneta própria: as de P() ficam em cache e não podem ser descartadas
                        {
                            g.DrawEllipse(pen, hcx - hw * 0.2f, hTop + hh * 0.04f, hw * 0.4f, hh * 0.14f);
                            g.DrawLine(pen, hcx - hw * 0.12f, hTop - hh * 0.02f, hcx + hw * 0.12f, hTop + hh * 0.24f);
                            g.DrawLine(pen, hcx + hw * 0.12f, hTop - hh * 0.02f, hcx - hw * 0.12f, hTop + hh * 0.24f);
                        }
                    }
                    else if (side) RR(g, Hx(0x61DAFB), hcx + (dir > 0 ? hw * 0.2f : -hw * 0.95f), hTop + hh * 0.3f, hw * 0.75f, hh * 0.14f, hh * 0.06f);
                    break;
                case "gorro_python":
                    using (var p = new GraphicsPath()) { p.AddPie(hcx - hw * 0.6f, hTop - hh * 0.18f, hw * 1.2f, hh * 1.15f, 180, 180); g.FillPath(B(Hx(0x3776AB)), p); }
                    for (int i = 0; i < 4; i++) R(g, Hx(0xFFD43B), hcx - hw * 0.4f + i * hw * 0.25f, hTop - hh * 0.04f, hw * 0.07f, hh * 0.36f);
                    RR(g, Hx(0xFFD43B), hcx - hw * 0.6f, hTop + hh * 0.26f, hw * 1.2f, hh * 0.16f, hh * 0.06f);
                    E(g, Hx(0xFFD43B), hcx - hw * 0.16f, hTop - hh * 0.32f, hw * 0.32f, hw * 0.32f);
                    break;
                case "boina":
                    E(g, Hx(0xC1121F), hcx - hw * 0.66f + (side ? dir * hw * 0.1f : 0), hTop - hh * 0.12f, hw * 1.32f, hh * 0.46f);
                    R(g, Hx(0x8D0E18), hcx - hw * 0.5f, hTop + hh * 0.22f, hw * 1.0f, hh * 0.08f);
                    R(g, Hx(0x8D0E18), hcx - hw * 0.03f, hTop - hh * 0.22f, hw * 0.06f, hh * 0.12f);
                    break;
                case "chef":
                    RR(g, Hx(0xF8FAFC), hcx - hw * 0.45f, hTop - hh * 0.55f, hw * 0.9f, hh * 0.75f, hw * 0.15f);
                    E(g, Hx(0xF8FAFC), hcx - hw * 0.55f, hTop - hh * 0.8f, hw * 0.55f, hh * 0.45f);
                    E(g, Hx(0xF8FAFC), hcx - hw * 0.05f, hTop - hh * 0.85f, hw * 0.6f, hh * 0.48f);
                    E(g, Hx(0xE2E8F0), hcx - hw * 0.25f, hTop - hh * 0.95f, hw * 0.5f, hh * 0.4f);
                    R(g, Hx(0xCBD5E1), hcx - hw * 0.5f, hTop + hh * 0.1f, hw * 1.0f, hh * 0.12f);
                    break;
            }
        }

        // Mascotes que seguem o jogador. (x, y) = posição no chão.
        public static void Pet(Graphics g, string kind, float x, float y, float u, int facing, float t, bool moving)
        {
            float bob = (float)Math.Abs(Math.Sin(t * (moving ? 9 : 3))) * u * (moving ? 0.05f : 0.02f);
            float d = facing == 1 ? -1 : 1;
            switch (kind)
            {
                case "moby":
                    {
                        float fy = y - u * 0.18f - (float)Math.Sin(t * 3) * u * 0.05f;
                        if (DrawShadows) E(g, A(Color.Black, 45), x - u * 0.22f, y - u * 0.05f, u * 0.44f, u * 0.1f);
                        g.FillPolygon(B(Hx(0x1B7FCC)), new[] { new PointF(x - d * u * 0.22f, fy - u * 0.12f), new PointF(x - d * u * 0.42f, fy - u * 0.28f), new PointF(x - d * u * 0.4f, fy - u * 0.02f) });
                        E(g, Docker, x - u * 0.27f, fy - u * 0.3f, u * 0.54f, u * 0.3f);
                        E(g, Hx(0xBDE3FF), x - u * 0.18f, fy - u * 0.12f, u * 0.36f, u * 0.1f);
                        float b = u * 0.07f;
                        for (int i = 0; i < 3; i++) R(g, Color.White, x - u * 0.13f + i * (b + u * 0.015f), fy - u * 0.4f, b, b);
                        E(g, Hx(0x0D1117), x + d * u * 0.15f - u * 0.025f, fy - u * 0.2f, u * 0.05f, u * 0.05f);
                        if ((int)(t * 1.5f) % 3 == 0)
                            for (int i = -1; i <= 1; i++) E(g, A(Hx(0x8CCBF2), 200), x + i * u * 0.06f - u * 0.025f, fy - u * 0.55f - Math.Abs(i) * u * 0.02f, u * 0.05f, u * 0.05f);
                        break;
                    }
                case "gato":
                    {
                        float by = y - bob;
                        if (DrawShadows) E(g, A(Color.Black, 45), x - u * 0.2f, y - u * 0.05f, u * 0.4f, u * 0.1f);
                        g.DrawBezier(P(Hx(0xE07B39), u * 0.06f), x - d * u * 0.15f, by - u * 0.12f, x - d * u * 0.35f, by - u * 0.15f, x - d * u * 0.3f, by - u * 0.4f, x - d * u * 0.2f, by - u * 0.42f);
                        E(g, Hx(0xF4A261), x - u * 0.18f, by - u * 0.24f, u * 0.36f, u * 0.22f);
                        E(g, Hx(0xF4A261), x + d * u * 0.08f - u * 0.13f, by - u * 0.42f, u * 0.26f, u * 0.24f);
                        float hx = x + d * u * 0.08f;
                        g.FillPolygon(B(Hx(0xE07B39)), new[] { new PointF(hx - u * 0.12f, by - u * 0.36f), new PointF(hx - u * 0.1f, by - u * 0.5f), new PointF(hx - u * 0.02f, by - u * 0.4f) });
                        g.FillPolygon(B(Hx(0xE07B39)), new[] { new PointF(hx + u * 0.12f, by - u * 0.36f), new PointF(hx + u * 0.1f, by - u * 0.5f), new PointF(hx + u * 0.02f, by - u * 0.4f) });
                        E(g, Hx(0x1B1B1F), hx - u * 0.07f, by - u * 0.33f, u * 0.04f, u * 0.05f);
                        E(g, Hx(0x1B1B1F), hx + u * 0.03f, by - u * 0.33f, u * 0.04f, u * 0.05f);
                        break;
                    }
                case "cachorro":
                    {
                        float by = y - bob;
                        Color fur = Hx(0xB07A4A), furDk = Hx(0x7A4F2C);
                        if (DrawShadows) E(g, A(Color.Black, 45), x - u * 0.22f, y - u * 0.05f, u * 0.44f, u * 0.1f);
                        float wag = (float)Math.Sin(t * 14) * u * 0.04f;
                        g.DrawLine(P(furDk, u * 0.05f), x - d * u * 0.2f, by - u * 0.22f, x - d * u * 0.3f + wag, by - u * 0.36f);
                        R(g, furDk, x - u * 0.15f, by - u * 0.12f, u * 0.06f, u * 0.12f);
                        R(g, furDk, x + u * 0.09f, by - u * 0.12f, u * 0.06f, u * 0.12f);
                        RR(g, fur, x - u * 0.22f, by - u * 0.28f, u * 0.44f, u * 0.2f, u * 0.08f);
                        float hx = x + d * u * 0.2f;
                        RR(g, fur, hx - u * 0.12f, by - u * 0.42f, u * 0.24f, u * 0.2f, u * 0.08f);
                        RR(g, furDk, hx - u * 0.15f, by - u * 0.42f, u * 0.08f, u * 0.17f, u * 0.03f);
                        RR(g, furDk, hx + u * 0.07f, by - u * 0.42f, u * 0.08f, u * 0.17f, u * 0.03f);
                        E(g, Hx(0x1B1B1F), hx + d * u * 0.09f - u * 0.025f, by - u * 0.3f, u * 0.05f, u * 0.04f);
                        E(g, Hx(0x1B1B1F), hx - u * 0.04f, by - u * 0.36f, u * 0.035f, u * 0.035f);
                        E(g, Hx(0x1B1B1F), hx + u * 0.02f, by - u * 0.36f, u * 0.035f, u * 0.035f);
                        break;
                    }                case "pinguim":
                    {
                        float by = y - bob, sway = (float)Math.Sin(t * (moving ? 10 : 2)) * u * 0.02f;
                        if (DrawShadows) E(g, A(Color.Black, 45), x - u * 0.16f, y - u * 0.05f, u * 0.32f, u * 0.1f);
                        E(g, Hx(0xF77F00), x - u * 0.13f, by - u * 0.05f, u * 0.12f, u * 0.06f);
                        E(g, Hx(0xF77F00), x + u * 0.01f, by - u * 0.05f, u * 0.12f, u * 0.06f);
                        E(g, Hx(0x1B1F24), x - u * 0.15f + sway, by - u * 0.46f, u * 0.3f, u * 0.44f);
                        E(g, Hx(0xF1F5F9), x - u * 0.1f + sway + d * u * 0.02f, by - u * 0.36f, u * 0.2f, u * 0.32f);
                        E(g, Hx(0x1B1F24), x - u * 0.12f + sway, by - u * 0.62f, u * 0.24f, u * 0.22f);
                        E(g, Hx(0xF1F5F9), x - u * 0.06f + sway + d * u * 0.03f, by - u * 0.57f, u * 0.05f, u * 0.06f);
                        E(g, Hx(0xF1F5F9), x + u * 0.02f + sway + d * u * 0.03f, by - u * 0.57f, u * 0.05f, u * 0.06f);
                        g.FillPolygon(B(Hx(0xF5C542)), new[] { new PointF(x + sway + d * u * 0.02f - u * 0.04f, by - u * 0.5f), new PointF(x + sway + d * u * 0.02f + u * 0.04f, by - u * 0.5f), new PointF(x + sway + d * u * 0.06f, by - u * 0.44f) });
                        break;
                    }
                case "cobra":
                    {
                        float by = y - bob * 0.5f;
                        if (DrawShadows) E(g, A(Color.Black, 40), x - u * 0.24f, y - u * 0.05f, u * 0.48f, u * 0.09f);
                        var pts = new PointF[7];
                        for (int i = 0; i < 7; i++)
                            pts[i] = new PointF(x - d * u * (0.24f - i * 0.07f), by - u * 0.06f - (float)Math.Sin(t * 6 + i * 1.1f) * u * 0.04f);
                        using (var pen = new Pen(Hx(0x3776AB), u * 0.09f) { StartCap = LineCap.Round, EndCap = LineCap.Round }) g.DrawCurve(pen, pts);
                        using (var pen = new Pen(Hx(0xFFD43B), u * 0.03f) { StartCap = LineCap.Round, EndCap = LineCap.Round }) g.DrawCurve(pen, pts);
                        var hd = pts[6];
                        E(g, Hx(0x3776AB), hd.X - u * 0.02f, hd.Y - u * 0.2f, u * 0.14f, u * 0.12f);
                        g.DrawLine(P(Hx(0x3776AB), u * 0.07f), hd.X, hd.Y, hd.X + u * 0.04f, hd.Y - u * 0.12f);
                        E(g, Hx(0x1B1B1F), hd.X + d * u * 0.03f + u * 0.03f, hd.Y - u * 0.17f, u * 0.03f, u * 0.03f);
                        if ((int)(t * 2) % 3 == 0) g.DrawLine(P(Hx(0xE63946), u * 0.015f), hd.X + u * 0.05f + d * u * 0.06f, hd.Y - u * 0.13f, hd.X + u * 0.05f + d * u * 0.12f, hd.Y - u * 0.12f);
                        break;
                    }
                case "elefante":
                    {
                        float by = y - bob;
                        Color ele = Hx(0x7B86C8), eleDk = Hx(0x5A64A6);
                        if (DrawShadows) E(g, A(Color.Black, 45), x - u * 0.24f, y - u * 0.05f, u * 0.48f, u * 0.1f);
                        R(g, eleDk, x - u * 0.16f, by - u * 0.14f, u * 0.08f, u * 0.14f);
                        R(g, eleDk, x + u * 0.08f, by - u * 0.14f, u * 0.08f, u * 0.14f);
                        E(g, ele, x - u * 0.24f, by - u * 0.36f, u * 0.48f, u * 0.3f);
                        float hx = x + d * u * 0.2f;
                        E(g, ele, hx - u * 0.13f, by - u * 0.48f, u * 0.26f, u * 0.24f);
                        E(g, eleDk, hx - d * u * 0.14f - u * 0.08f, by - u * 0.48f, u * 0.16f, u * 0.2f);
                        g.DrawBezier(P(ele, u * 0.07f), hx + d * u * 0.1f, by - u * 0.34f, hx + d * u * 0.2f, by - u * 0.28f, hx + d * u * 0.22f, by - u * 0.18f, hx + d * u * 0.28f, by - u * 0.2f);
                        E(g, Hx(0x1B1B1F), hx + d * u * 0.04f - u * 0.02f, by - u * 0.4f, u * 0.04f, u * 0.04f);
                        break;
                    }
                case "pato":
                    {
                        float by = y - bob;
                        if (DrawShadows) E(g, A(Color.Black, 45), x - u * 0.18f, y - u * 0.05f, u * 0.36f, u * 0.1f);
                        E(g, Hx(0xFFD60A), x - u * 0.2f, by - u * 0.22f, u * 0.4f, u * 0.2f);
                        E(g, Hx(0xFFD60A), x + d * u * 0.06f - u * 0.11f, by - u * 0.4f, u * 0.22f, u * 0.22f);
                        g.FillPolygon(B(Hx(0xF77F00)), new[] { new PointF(x + d * u * 0.15f, by - u * 0.3f), new PointF(x + d * u * 0.26f, by - u * 0.28f), new PointF(x + d * u * 0.15f, by - u * 0.25f) });
                        E(g, Hx(0x1B1B1F), x + d * u * 0.08f - u * 0.02f, by - u * 0.33f, u * 0.04f, u * 0.04f);
                        E(g, Hx(0xFFEA70), x - d * u * 0.08f - u * 0.08f, by - u * 0.2f, u * 0.14f, u * 0.08f);
                        break;
                    }
                case "robo":
                    {
                        float hover = y - u * 0.15f - (float)Math.Sin(t * 4) * u * 0.04f;
                        if (DrawShadows) E(g, A(Color.Black, 40), x - u * 0.16f, y - u * 0.05f, u * 0.32f, u * 0.08f);
                        RR(g, Hx(0x9AA5B6), x - u * 0.16f, hover - u * 0.26f, u * 0.32f, u * 0.24f, u * 0.05f);
                        RR(g, Hx(0xC5CED9), x - u * 0.14f, hover - u * 0.48f, u * 0.28f, u * 0.2f, u * 0.05f);
                        R(g, Hx(0x0D1117), x - u * 0.1f, hover - u * 0.44f, u * 0.2f, u * 0.1f);
                        E(g, Hx(0x39C5CF), x - u * 0.08f + d * u * 0.02f, hover - u * 0.42f, u * 0.05f, u * 0.05f);
                        E(g, Hx(0x39C5CF), x + u * 0.03f + d * u * 0.02f, hover - u * 0.42f, u * 0.05f, u * 0.05f);
                        g.DrawLine(P(Hx(0x6B7486), u * 0.02f), x, hover - u * 0.48f, x, hover - u * 0.58f);
                        E(g, (int)(t * 2) % 2 == 0 ? Hx(0xF85149) : Hx(0x5C2430), x - u * 0.03f, hover - u * 0.62f, u * 0.06f, u * 0.06f);
                        R(g, Hx(0x2496ED), x - u * 0.06f, hover - u * 0.18f, u * 0.12f, u * 0.04f);
                        break;
                    }
            }
        }

        public static void Coin(Graphics g, float x, float y, float d)
        {
            E(g, Hx(0xB8860B), x, y, d, d);
            E(g, Hx(0xF5C542), x + d * 0.08f, y + d * 0.06f, d * 0.84f, d * 0.84f);
            E(g, Hx(0xFFE08A), x + d * 0.25f, y + d * 0.22f, d * 0.5f, d * 0.5f);
            R(g, Hx(0xB8860B), x + d * 0.45f, y + d * 0.3f, d * 0.1f, d * 0.36f);
        }

        static void Kiosk(Graphics g, float x, float y, float w, float h, float T, float t)
        {
            Shadow(g, x, y + h, w, T);
            // prateleira ao fundo com produtos
            R(g, Hx(0x6B4A2F), x + T * 0.1f, y - T * 0.66f, w - T * 0.2f, T * 0.75f);
            var cols = new[] { 0x2496ED, 0xE63946, 0xF0B429, 0x2A9D8F, 0x8E6CC4, 0xF4A261 };
            for (int i = 0; i < 8; i++)
            {
                float ix = x + T * 0.2f + i * (w - T * 0.4f) / 8;
                RR(g, Hx(cols[i % cols.Length]), ix, y - T * (0.56f - (i % 2) * 0.04f), (w - T * 0.5f) / 8, T * 0.22f, T * 0.04f);
                RR(g, Hx(cols[(i + 3) % cols.Length]), ix, y - T * 0.26f, (w - T * 0.5f) / 8, T * 0.2f, T * 0.04f);
            }
            // toldo listrado
            float aw = w + T * 0.2f, ax = x - T * 0.1f, ay = y - T * 0.98f;
            for (int i = 0; i < 8; i++) R(g, i % 2 == 0 ? Hx(0xE63946) : Color.White, ax + i * aw / 8, ay, aw / 8 + 0.5f, T * 0.26f);
            for (int i = 0; i < 8; i++) E(g, i % 2 == 0 ? Hx(0xE63946) : Color.White, ax + i * aw / 8, ay + T * 0.17f, aw / 8, T * 0.18f);
            // balcão
            R(g, Hx(0xD9B98C), x, y + T * 0.15f, w, T * 0.3f);
            R(g, Hx(0xB08A5B), x, y + T * 0.45f, w, h - T * 0.5f);
            RR(g, Hx(0x1D3557), x + w / 2 - T * 0.75f, y + T * 0.5f, T * 1.5f, T * 0.32f, T * 0.06f);
            using (var f = new Font("Segoe UI Black", T * 0.19f, GraphicsUnit.Pixel))
                Text(g, "LOJINHA", f, Color.White, new RectangleF(x + w / 2 - T * 0.75f, y + T * 0.5f, T * 1.5f, T * 0.32f));
            Coin(g, x + T * 0.25f, y + T * 0.0f, T * 0.28f);
            Coin(g, x + w - T * 0.55f, y + T * 0.02f, T * 0.28f);
        }
        // trança: gomos alternados, com elástico na ponta
        static void Braid(Graphics g, Color c, Color dk, float cx, float y, float w, float seg, int n, Color tie)
        {
            for (int i = 0; i < n; i++)
            {
                float yy = y + i * seg * 0.82f, off = (i % 2 == 0 ? -1 : 1) * w * 0.12f;
                E(g, i % 2 == 0 ? c : Dk(c, 0.12f), cx - w / 2 + off, yy, w, seg);
                R(g, dk, cx - w * 0.3f + off, yy + seg * 0.75f, w * 0.6f, Math.Max(1, seg * 0.12f));
            }
            R(g, tie, cx - w * 0.3f, y + n * seg * 0.82f, w * 0.6f, Math.Max(1, seg * 0.2f));
            E(g, c, cx - w * 0.35f, y + n * seg * 0.82f + seg * 0.15f, w * 0.7f, seg * 0.5f);
        }

        public static void Portrait(Graphics g, RectangleF box, Look look, float t)
        {
            var old = g.Clip;
            g.SetClip(box);
            using (var lg = new LinearGradientBrush(box, Lt(look.Shirt, 0.55f), Lt(look.Shirt, 0.25f), 90f))
                g.FillRectangle(lg, box);
            float H = box.Height * 2.0f;
            Character(g, box.X + box.Width / 2, box.Y + box.Height * 0.1f + H, H / 1.5f, look, 0, 0, false, t);
            g.Clip = old;
        }
    }
}
