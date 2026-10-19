using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace DockerQuest
{
    // Renderização top-down em 3/4 (estilo Gather/Stardew), em HD: o chão é visto de cima e as paredes e móveis
    // mostram a face da frente. Coordenadas do mundo em tiles (x, y) + altura z em "pixels do mundo"; tudo é desenhado
    // já na escala da tela (Q pixels da tela por pixel do mundo), com bordas suaves, degradês e sombras.
    // Para ficar leve, quase tudo é desenhado uma vez e guardado em cache (texturas de piso, blocos de chão, móveis, sprites).
    static class Scene
    {
        public const int T = 32;                                   // pixels do mundo por tile
        public const float WALL_H = 48, GLASS_H = 40, LOW_H = 12;  // alturas das paredes
        public const float OX = 16, OY = 76;                       // margens (as paredes do topo sobem acima da linha 0)
        public static int Q = 2;                                   // escala de desenho (definida pelo jogo)

        static readonly Color Edge = Art.Hx(0x1E2230);

        public static PointF P(float x, float y, float z) { return new PointF(x * T + OX, y * T - z + OY); }
        public static Size WorldSize { get { return new Size((int)(World.W * T + 2 * OX), (int)(World.H * T + OY + 24)); } }

        static readonly Dictionary<long, Pen> pens = new Dictionary<long, Pen>();
        static Pen Pn(Color c, float w = 1f)
        {
            long k = ((long)c.ToArgb() << 8) ^ (long)(w * 16);
            Pen p;
            if (!pens.TryGetValue(k, out p)) { p = new Pen(c, w) { StartCap = LineCap.Round, EndCap = LineCap.Round }; pens[k] = p; }
            return p;
        }

        static void R(Graphics g, Color c, float x, float y, float w, float h) { if (w > 0 && h > 0) g.FillRectangle(Art.B(c), x, y, w, h); }
        static void Outline(Graphics g, Color c, float x, float y, float w, float h) { g.DrawRectangle(Pn(c, 0.7f), x + 0.35f, y + 0.35f, w - 0.7f, h - 0.7f); }

        // ── utilitários de pintura (degradês e sombras suaves)
        static void GradV(Graphics g, Color top, Color bottom, float x, float y, float w, float h)
        {
            if (w <= 0 || h <= 0) return;
            using (var lg = new LinearGradientBrush(new RectangleF(x, y - 0.5f, w, h + 1f), top, bottom, 90f)) g.FillRectangle(lg, x, y, w, h);
        }

        static void GradH(Graphics g, Color left, Color right, float x, float y, float w, float h)
        {
            if (w <= 0 || h <= 0) return;
            using (var lg = new LinearGradientBrush(new RectangleF(x - 0.5f, y, w + 1f, h), left, right, 0f)) g.FillRectangle(lg, x, y, w, h);
        }

        static void SoftEllipse(Graphics g, float cx, float cy, float rx, float ry, Color c)
        {
            if (rx <= 0 || ry <= 0) return;
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(cx - rx, cy - ry, rx * 2, ry * 2);
                using (var pg = new PathGradientBrush(path))
                {
                    pg.CenterColor = c;
                    pg.SurroundColors = new[] { Color.FromArgb(0, c) };
                    pg.FocusScales = new PointF(0.35f, 0.35f);
                    g.FillPath(pg, path);
                }
            }
        }

        // versão barata para o que é desenhado a cada quadro: camadas de elipses semitransparentes
        static void SoftEllipseFast(Graphics g, float cx, float cy, float rx, float ry, Color c)
        {
            for (int i = 0; i < 3; i++)
            {
                float k = 1f - i * 0.28f;
                g.FillEllipse(Art.B(Color.FromArgb(c.A / 3, c)), cx - rx * k, cy - ry * k, rx * 2 * k, ry * 2 * k);
            }
        }

        // sombra difusa de um retângulo (camadas cada vez maiores e mais transparentes)
        static void SoftRect(Graphics g, RectangleF r, float blur, int alpha)
        {
            const int n = 5;
            for (int i = n; i >= 1; i--)
            {
                float k = blur * i / n;
                using (var p = Art.RoundPath(r.X - k, r.Y - k, r.Width + 2 * k, r.Height + 2 * k, Math.Max(1, k + 1.5f)))
                    g.FillPath(Art.B(Art.A(Color.Black, alpha / n)), p);
            }
        }

        // Bloco em 3/4: base no chão em (x, y) com largura w e profundidade d (tiles), entre as alturas z0 e z1.
        // Tampo com luz suave, face da frente em degradê, realce na quina e contorno discreto. Retorna o tampo.
        public static RectangleF Block(Graphics g, float x, float y, float w, float d, float z0, float z1, Color top, Color front, bool outline = true)
        {
            var o = P(x, y, z1);
            float W = w * T, D = d * T, H = z1 - z0;
            var topR = new RectangleF(o.X, o.Y, W, D);
            if (H > 0) GradV(g, Art.Lt(front, 0.03f), Art.Dk(front, 0.16f), o.X, o.Y + D, W, H);
            if (D > 0)
                using (var lg = new LinearGradientBrush(new RectangleF(o.X - 1, o.Y - 1, W + 2, D + 2), Art.Lt(top, 0.1f), Art.Dk(top, 0.05f), 35f))
                    g.FillRectangle(lg, o.X, o.Y, W, D);
            if (outline)
            {
                if (D > 1) g.DrawLine(Pn(Art.A(Art.Lt(top, 0.55f), 170), 0.8f), o.X + 0.8f, o.Y + 0.6f, o.X + W - 0.8f, o.Y + 0.6f);
                if (H >= 2) g.DrawLine(Pn(Art.A(Art.Dk(front, 0.4f), 210), 0.8f), o.X + 0.4f, o.Y + D, o.X + W - 0.4f, o.Y + D);
                if (H >= 4 && W > 3) g.DrawLine(Pn(Art.A(Color.White, 45), 0.7f), o.X + 0.8f, o.Y + D + 1, o.X + 0.8f, o.Y + D + H - 1);
                Outline(g, Art.A(Edge, 150), o.X, o.Y, W, D + H);
            }
            return topR;
        }

        public static RectangleF Block(Graphics g, float x, float y, float w, float d, float z0, float z1, Color c, bool outline = true)
        {
            return Block(g, x, y, w, d, z0, z1, Art.Lt(c, 0.1f), Art.Dk(c, 0.18f), outline);
        }

        // Retângulo da face da frente de um bloco (para desenhar detalhes nela)
        public static RectangleF Front(float x, float y, float w, float d, float z0, float z1)
        {
            var o = P(x, y + d, z1);
            return new RectangleF(o.X, o.Y, w * T, z1 - z0);
        }

        // sombra de personagem: elipse difusa no chão
        public static void Shadow(Graphics g, float x, float y, float size, int alpha)
        {
            var c = P(x, y, 0);
            float w = T * size * 1.2f, h = T * size * 0.46f;
            SoftEllipseFast(g, c.X, c.Y + 1, w / 2, h / 2, Art.A(Color.Black, alpha));
        }

        // ───────────────────────── chão ─────────────────────────

        static bool IsFloor(World w, int x, int y) { return w.InMap(x, y) && (w.Tile[x, y] == World.Floor || w.Tile[x, y] == World.Door); }

        internal static void FloorStyle(World w, int x, int y, out Color a, out Color b, out int style)
        {
            int zone = w.Zone[x, y];
            if (w.Tile[x, y] == World.Wall)
            {
                // parede: usa o chão do vizinho (aparece através do vidro)
                foreach (var d in new[] { new Point(0, -1), new Point(-1, 0), new Point(1, 0), new Point(0, 1) })
                    if (IsFloor(w, x + d.X, y + d.Y)) { zone = w.Zone[x + d.X, y + d.Y]; break; }
            }
            if (zone < 0) { a = Art.Hx(0xD5D9E0); b = Art.Hx(0xCBD0D8); style = 5; }
            else { var s = Story.SectorOf(zone); a = s.F1; b = s.F2; style = s.Style; }
        }

        static int Hash(int x, int y) { unchecked { int h = x * 73856093 ^ y * 19349663; h ^= h >> 13; h *= 0x5bd1e995; h ^= h >> 15; return h & 0x7FFFFFFF; } }

        // texturas de piso: uma imagem por (estilo, cores, variação), já na escala da tela
        static readonly Dictionary<string, Bitmap> tileTex = new Dictionary<string, Bitmap>();

        static Bitmap TileTex(int style, Color a, Color b, int x, int y)
        {
            int variant, px = 0, py = 0;
            Color main = a;
            switch (style)
            {
                case 0: main = ((x + y) & 1) == 0 ? a : b; variant = Hash(x, y) % 4; break;
                case 5: px = x & 1; py = y & 1; variant = Hash(x >> 1, y >> 1) % 4; break;
                case 1: variant = Hash(x, y) % 8; break;
                case 4: variant = Hash(x, y) % 8; break;
                default: variant = Hash(x, y) % 4; break;
            }
            string key = style + "|" + main.ToArgb() + "|" + b.ToArgb() + "|" + variant + "|" + px + "|" + py;
            Bitmap bmp;
            if (tileTex.TryGetValue(key, out bmp)) return bmp;
            bmp = new Bitmap(T * Q, T * Q, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.ScaleTransform(Q, Q);
                g.SetClip(new RectangleF(0, 0, T, T));
                TileArt(g, style, main, b, variant, px, py);
            }
            tileTex[key] = bmp;
            return bmp;
        }

        static void TileArt(Graphics g, int style, Color a, Color b, int v, int px, int py)
        {
            var rnd = new Random(1000 + v * 97 + style * 13);
            switch (style)
            {
                case 0: // porcelanato: placas com leve brilho, salpicado e rejunte
                    {
                        R(g, a, 0, 0, T, T);
                        for (int i = 0; i < 70; i++)
                        {
                            float s = 0.5f + (float)rnd.NextDouble() * 0.8f;
                            g.FillEllipse(Art.B(Art.A(rnd.Next(2) == 0 ? Art.Lt(a, 0.15f) : Art.Dk(a, 0.1f), 70)), (float)rnd.NextDouble() * T, (float)rnd.NextDouble() * T, s, s);
                        }
                        using (var lg = new LinearGradientBrush(new RectangleF(0, 0, T, T), Art.A(Color.White, 34), Art.A(Color.White, 0), 45f)) g.FillRectangle(lg, 0, 0, T, T);
                        g.DrawLine(Pn(Art.A(Art.Lt(a, 0.35f), 160), 0.8f), 0.6f, 0.6f, T - 0.6f, 0.6f);
                        g.DrawLine(Pn(Art.A(Art.Lt(a, 0.35f), 160), 0.8f), 0.6f, 0.6f, 0.6f, T - 0.6f);
                        R(g, Art.Dk(a, 0.2f), 0, T - 0.8f, T, 0.8f);
                        R(g, Art.Dk(a, 0.2f), T - 0.8f, 0, 0.8f, T);
                        break;
                    }
                case 1: // madeira: tábuas com veios, emendas e variação de tom
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            float y0 = k * 8;
                            float tone = (float)rnd.NextDouble() * 0.12f - 0.06f;
                            Color pc = tone > 0 ? Art.Lt(a, tone) : Art.Dk(a, -tone);
                            GradV(g, Art.Lt(pc, 0.05f), Art.Dk(pc, 0.06f), 0, y0, T, 8);
                            for (int gr = 0; gr < 3; gr++)
                            {
                                float gy = y0 + 1.5f + (float)rnd.NextDouble() * 5f, amp = 0.4f + (float)rnd.NextDouble() * 0.8f;
                                g.DrawBezier(Pn(Art.A(Art.Dk(pc, 0.18f), 90), 0.45f), 0, gy, T * 0.33f, gy - amp, T * 0.66f, gy + amp, T, gy);
                            }
                            float seam = 4 + rnd.Next(24);
                            g.DrawLine(Pn(Art.A(Art.Dk(a, 0.35f), 220), 0.8f), seam, y0 + 0.4f, seam, y0 + 7.4f);
                            g.DrawLine(Pn(Art.A(Art.Lt(pc, 0.25f), 120), 0.6f), seam + 0.8f, y0 + 0.6f, seam + 0.8f, y0 + 7.2f);
                            if (rnd.Next(7) == 0) g.FillEllipse(Art.B(Art.A(Art.Dk(pc, 0.25f), 150)), 6 + rnd.Next(20), y0 + 2.5f, 2.4f, 1.6f);
                            R(g, Art.Dk(a, 0.32f), 0, y0 + 7.3f, T, 0.7f);
                            R(g, Art.A(Art.Lt(pc, 0.3f), 90), 0, y0, T, 0.5f);
                        }
                        break;
                    }
                case 2: // carpete: trama fina e textura
                    {
                        R(g, a, 0, 0, T, T);
                        for (int i = -4; i < 8; i++)
                            g.DrawLine(Pn(Art.A(Art.Dk(a, 0.07f), 50), 0.5f), i * 8, 0, i * 8 + T, T);
                        for (int i = 0; i < 260; i++)
                        {
                            float s = 0.45f + (float)rnd.NextDouble() * 0.5f;
                            g.FillEllipse(Art.B(Art.A(rnd.Next(2) == 0 ? Art.Lt(a, 0.13f) : Art.Dk(a, 0.12f), 85)), (float)rnd.NextDouble() * T, (float)rnd.NextDouble() * T, s, s);
                        }
                        break;
                    }
                case 3: // piso elevado de data center: placas metálicas, algumas perfuradas
                    {
                        GradV(g, Art.Lt(a, 0.08f), Art.Dk(a, 0.06f), 0, 0, T, T);
                        R(g, Art.Dk(a, 0.32f), 0, T - 1, T, 1);
                        R(g, Art.Dk(a, 0.32f), T - 1, 0, 1, T);
                        g.DrawLine(Pn(Art.A(Art.Lt(a, 0.35f), 170), 0.8f), 0.5f, 0.5f, T - 1.5f, 0.5f);
                        g.DrawLine(Pn(Art.A(Art.Lt(a, 0.35f), 170), 0.8f), 0.5f, 0.5f, 0.5f, T - 1.5f);
                        if (v == 0)
                            for (int i = 0; i < 6; i++) for (int j = 0; j < 6; j++)
                                g.FillEllipse(Art.B(Art.A(Art.Dk(a, 0.3f), 180)), 6.2f + i * 4, 6.2f + j * 4, 1.4f, 1.4f);
                        foreach (var c in new[] { new PointF(3, 3), new PointF(T - 5, 3), new PointF(3, T - 5), new PointF(T - 5, T - 5) })
                        {
                            g.FillEllipse(Art.B(Art.Dk(a, 0.25f)), c.X, c.Y, 1.6f, 1.6f);
                            g.FillEllipse(Art.B(Art.A(Color.White, 90)), c.X + 0.3f, c.Y + 0.2f, 0.6f, 0.6f);
                        }
                        break;
                    }
                case 4: // grama: tufos, folhas e algumas flores
                    {
                        R(g, a, 0, 0, T, T);
                        for (int i = 0; i < 6; i++)
                            SoftEllipse(g, (float)rnd.NextDouble() * T, (float)rnd.NextDouble() * T, 5 + (float)rnd.NextDouble() * 6, 3 + (float)rnd.NextDouble() * 4, Art.A(rnd.Next(2) == 0 ? Art.Dk(a, 0.15f) : b, 70));
                        for (int i = 0; i < 70; i++)
                        {
                            float bx = (float)rnd.NextDouble() * T, by = (float)rnd.NextDouble() * (T + 3), len = 1.6f + (float)rnd.NextDouble() * 2.6f, lean = ((float)rnd.NextDouble() - 0.5f) * 1.8f;
                            int c = rnd.Next(3);
                            Color bc = c == 0 ? Art.Dk(a, 0.22f) : c == 1 ? Art.Lt(a, 0.2f) : b;
                            g.DrawLine(Pn(Art.A(bc, 210), 0.55f), bx, by, bx + lean, by - len);
                        }
                        if (v == 1 || v == 5)
                        {
                            Color fc = v == 1 ? Art.Hx(0xFFE066) : Art.Hx(0xFF8FAB);
                            float fx = 8 + rnd.Next(16), fy = 8 + rnd.Next(16);
                            for (int k = 0; k < 5; k++)
                            {
                                double an = k * Math.PI * 2 / 5;
                                g.FillEllipse(Art.B(fc), fx + (float)Math.Cos(an) * 1.2f - 0.9f, fy + (float)Math.Sin(an) * 1.2f - 0.9f, 1.8f, 1.8f);
                            }
                            g.FillEllipse(Art.B(Art.Hx(0xF4A261)), fx - 0.7f, fy - 0.7f, 1.4f, 1.4f);
                        }
                        break;
                    }
                default: // corredor: pedra polida em placas grandes (2×2 tiles), com veios contínuos
                    {
                        R(g, a, 0, 0, T, T);
                        var st = g.Save();
                        g.TranslateTransform(-px * T, -py * T);
                        var vr = new Random(5000 + v * 31);
                        for (int i = 0; i < 4; i++)
                        {
                            float sx = (float)vr.NextDouble() * 64, sy = (float)vr.NextDouble() * 64;
                            g.DrawBezier(Pn(Art.A(i % 2 == 0 ? Art.Lt(a, 0.4f) : Art.Dk(a, 0.08f), 70), 0.5f + (float)vr.NextDouble() * 0.5f),
                                sx, sy, sx + 10 + (float)vr.NextDouble() * 20, sy + 6, sx + 20, sy + 18 + (float)vr.NextDouble() * 10, sx + 34, sy + 26);
                        }
                        using (var lg = new LinearGradientBrush(new RectangleF(0, 0, 64, 64), Art.A(Color.White, 30), Art.A(Color.White, 0), 50f)) g.FillRectangle(lg, 0, 0, 64, 64);
                        g.Restore(st);
                        if (px == 0) { R(g, Art.Dk(a, 0.16f), 0, 0, 0.8f, T); R(g, Art.A(Color.White, 70), 0.8f, 0, 0.6f, T); }
                        if (py == 0) { R(g, Art.Dk(a, 0.16f), 0, 0, T, 0.8f); R(g, Art.A(Color.White, 70), 0, 0.8f, T, 0.6f); }
                        break;
                    }
            }
        }

        // um bloco do chão (n×n tiles a partir de tx0, ty0), com tapetes, sombras e luz das janelas — já na escala da tela
        public static Bitmap FloorChunk(World w, int tx0, int ty0, int n)
        {
            int cs = n * T;
            var bmp = new Bitmap(cs * Q, cs * Q, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Art.Void);
                g.CompositingMode = CompositingMode.SourceCopy;
                for (int x = tx0; x < tx0 + n; x++)
                    for (int y = ty0; y < ty0 + n; y++)
                    {
                        if (!w.InMap(x, y) || w.Tile[x, y] == World.Void) continue;
                        Color a, b; int style;
                        FloorStyle(w, x, y, out a, out b, out style);
                        g.DrawImageUnscaled(TileTex(style, a, b, x, y), (x - tx0) * T * Q, (y - ty0) * T * Q);
                    }
                g.CompositingMode = CompositingMode.SourceOver;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.ScaleTransform(Q, Q);
                g.TranslateTransform(-(OX + tx0 * T), -(OY + ty0 * T));
                var area = new RectangleF(OX + (tx0 - 3) * T, OY + (ty0 - 3) * T, (n + 6) * T, (n + 6) * T);

                foreach (var r in w.Rugs)
                    if (new RectangleF(P(r.X, r.Y, 0), new SizeF(r.W * T, r.H * T)).IntersectsWith(area)) Rug(g, r);

                // sombras dos móveis no chão
                foreach (var f in w.Furniture)
                    if (new RectangleF(P(f.X, f.Y, 0), new SizeF(f.W * T, f.H * T)).IntersectsWith(area)) FurnShadow(g, f);

                // sombra de contato junto às paredes (de cima e dos lados) e luz que entra pelas janelas
                for (int x = tx0 - 1; x <= tx0 + n; x++)
                    for (int y = ty0 - 1; y <= ty0 + n; y++)
                    {
                        if (!w.InMap(x, y) || w.Tile[x, y] != World.Wall) continue;
                        if (IsFloor(w, x, y + 1)) { var o = P(x, y + 1, 0); GradV(g, Art.A(Color.Black, 78), Art.A(Color.Black, 0), o.X, o.Y, T, 16); }
                        if (IsFloor(w, x + 1, y)) { var o = P(x + 1, y, 0); GradH(g, Art.A(Color.Black, 50), Art.A(Color.Black, 0), o.X, o.Y, 9, T); }
                        if (IsFloor(w, x - 1, y)) { var o = P(x, y, 0); GradH(g, Art.A(Color.Black, 0), Art.A(Color.Black, 50), o.X - 9, o.Y, 9, T); }
                    }
                foreach (var d in w.Decor)
                {
                    if (d.Kind != "window") continue;
                    var o = P(d.X, d.Y + 1, 0);
                    if (!new RectangleF(o.X - T, o.Y, (d.W + 2) * T, 3 * T).IntersectsWith(area)) continue;
                    float L = 2.6f * T, sk = 0.9f * T;
                    var poly = new[] { new PointF(o.X + 2, o.Y), new PointF(o.X + d.W * T - 2, o.Y), new PointF(o.X + d.W * T - 2 + sk, o.Y + L), new PointF(o.X + 2 + sk, o.Y + L) };
                    using (var lg = new LinearGradientBrush(new RectangleF(o.X, o.Y - 1, d.W * T + sk, L + 2), Art.A(Art.Hx(0xFFF4D6), 70), Art.A(Art.Hx(0xFFF4D6), 0), 90f))
                        g.FillPolygon(lg, poly);
                }
            }
            return bmp;
        }

        static void FurnShadow(Graphics g, Furn f)
        {
            switch (f.Kind)
            {
                case "plant": { var c = P(f.X + 0.5f, f.Y + 0.75f, 0); SoftEllipse(g, c.X + 1, c.Y + 1, 9, 5, Art.A(Color.Black, 80)); break; }
                case "tree": { var c = P(f.X + 0.5f, f.Y + 0.75f, 0); SoftEllipse(g, c.X + 3, c.Y + 2, 22, 9, Art.A(Color.Black, 85)); break; }
                case "beanbag": { var c = P(f.X + 0.5f, f.Y + 0.65f, 0); SoftEllipse(g, c.X + 1, c.Y + 1, 15, 7, Art.A(Color.Black, 75)); break; }
                case "cooler": { var c = P(f.X + 0.5f, f.Y + 0.8f, 0); SoftEllipse(g, c.X + 1, c.Y + 1, 10, 5, Art.A(Color.Black, 70)); break; }
                case "robot": case "fountain": break;
                default:
                    {
                        var o = P(f.X, f.Y, 0);
                        var r = new RectangleF(o.X + 2.5f, o.Y + f.H * T * 0.25f + 2, f.W * T - 2, f.H * T * 0.8f);
                        SoftRect(g, r, 5, f.Kind == "chairs" || f.Kind == "bench" ? 55 : 70);
                        break;
                    }
            }
        }

        static void Rug(Graphics g, Furn r)
        {
            var o = P(r.X, r.Y, 0);
            float W = r.W * T, H = r.H * T;
            SoftRect(g, new RectangleF(o.X + 1, o.Y + 2, W, H), 3, 45);
            using (var p = Art.RoundPath(o.X, o.Y, W, H, 3)) using (var lg = new LinearGradientBrush(new RectangleF(o.X, o.Y, W, H), Art.Lt(r.C, 0.06f), Art.Dk(r.C, 0.1f), 60f)) g.FillPath(lg, p);
            using (var p = Art.RoundPath(o.X + 5, o.Y + 5, W - 10, H - 10, 2)) g.DrawPath(Pn(Art.A(Art.Lt(r.C, 0.45f), 200), 1f), p);
            using (var p = Art.RoundPath(o.X + 8, o.Y + 8, W - 16, H - 16, 2)) g.DrawPath(Pn(Art.A(Art.Dk(r.C, 0.25f), 160), 0.6f), p);
            var rnd = new Random((int)(r.X * 31 + r.Y * 7));
            for (int i = 0; i < (int)(W * H / 6); i++)
                g.FillEllipse(Art.B(Art.A(rnd.Next(2) == 0 ? Art.Lt(r.C, 0.15f) : Art.Dk(r.C, 0.15f), 60)), o.X + 2 + (float)rnd.NextDouble() * (W - 4), o.Y + 2 + (float)rnd.NextDouble() * (H - 4), 0.8f, 0.8f);
            if (r.Var == 2)
            {
                // quatro quadradinhos (logo da empresa) no tapete da recepção
                float s = Math.Min(W, H) * 0.16f, cx = o.X + W / 2, cy = o.Y + H / 2;
                var lc = new[] { 0xF25022, 0x7FBA00, 0x00A4EF, 0xFFB900 };
                for (int k = 0; k < 4; k++) Art.RR(g, Art.Hx(lc[k]), cx - s - 1 + (k % 2) * (s + 2), cy - s - 1 + (k / 2) * (s + 2), s, s, 1);
            }
        }

        // compatibilidade: o chão inteiro numa imagem (não é mais usado no jogo, só em ferramentas)
        public static Bitmap RenderFloor(World w)
        {
            var sz = WorldSize;
            var bmp = new Bitmap(sz.Width, sz.Height, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(bmp)) g.Clear(Art.Void);
            return bmp;
        }

        // ───────────────────────── paredes ─────────────────────────

        public class WallPiece { public int X, Y; public float H; public bool Glass; public float Key; public Color Face; }

        public static List<WallPiece> BuildWalls(World w)
        {
            var list = new List<WallPiece>();
            Func<int, int, bool> fl = (x, y) => IsFloor(w, x, y);
            Func<int, int, bool> wl = (x, y) => w.InMap(x, y) && w.Tile[x, y] == World.Wall;
            Func<int, int, bool> glassRow = (x, y) => (wl(x, y) && fl(x, y - 1) && fl(x, y + 1)) || (w.InMap(x, y) && w.Tile[x, y] == World.Door);
            var map = new Dictionary<long, WallPiece>();
            for (int x = 0; x < World.W; x++)
                for (int y = 0; y < World.H; y++)
                {
                    if (!wl(x, y)) continue;
                    bool fN = fl(x, y - 1), fS = fl(x, y + 1), fW = fl(x - 1, y), fE = fl(x + 1, y);
                    if (!fN && !fS && !fW && !fE) continue;
                    var p = new WallPiece { X = x, Y = y, Key = y + 1 };
                    if ((fN && fS) || (fW && fE) || glassRow(x - 1, y) || glassRow(x + 1, y)) { p.Glass = true; p.H = GLASS_H; }
                    else if (fN && !fS && !fW && !fE) p.H = LOW_H;
                    else p.H = WALL_H;
                    int zone = fS ? w.Zone[x, y + 1] : fW ? w.Zone[x - 1, y] : fE ? w.Zone[x + 1, y] : -1;
                    p.Face = FaceColor(zone);
                    map[((long)x << 32) | (uint)y] = p;
                    list.Add(p);
                }
            // junções (sem chão ao redor): herdam o tipo dos vizinhos
            for (int x = 0; x < World.W; x++)
                for (int y = 0; y < World.H; y++)
                {
                    if (!wl(x, y) || map.ContainsKey(((long)x << 32) | (uint)y)) continue;
                    WallPiece best = null;
                    foreach (var d in new[] { new Point(0, 1), new Point(-1, 0), new Point(1, 0), new Point(0, -1) })
                    {
                        WallPiece n;
                        if (map.TryGetValue(((long)(x + d.X) << 32) | (uint)(y + d.Y), out n) && (best == null || (best.Glass && !n.Glass) || n.H > best.H)) best = n;
                    }
                    if (best == null) continue;
                    var p = new WallPiece { X = x, Y = y, Key = y + 1, H = Math.Max(best.H, best.Glass ? GLASS_H : WALL_H), Glass = best.Glass, Face = best.Face };
                    list.Add(p);
                }
            return list;
        }

        internal static Color FaceColor(int zone)
        {
            Color baseC = Art.Hx(0xE7EAF0);
            if (zone < 0) return baseC;
            var s = Story.SectorOf(zone);
            return Lerp(baseC, s.F1, 0.18f);
        }

        static Color Lerp(Color a, Color b, float t)
        {
            return Color.FromArgb(255, (int)(a.R + (b.R - a.R) * t), (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
        }

        static readonly Color WallTop = Art.Hx(0x3E4659), Glass = Art.Hx(0xA9D8FF), Alu = Art.Hx(0xB9C3CF), AluDk = Art.Hx(0x7C8899);

        public static void DrawWall(Graphics g, WallPiece p)
        {
            var o = P(p.X, p.Y, p.H);           // canto superior do tampo
            float X = o.X, Ytop = o.Y, Yface = o.Y + T, H = p.H;
            if (!p.Glass)
            {
                // tampo: acabamento escuro com bisel
                GradV(g, Art.Lt(WallTop, 0.12f), WallTop, X, Ytop, T, T);
                R(g, Art.A(Art.Lt(WallTop, 0.4f), 200), X, Ytop, T, 0.9f);
                R(g, Art.Dk(WallTop, 0.25f), X, Ytop + T - 1.2f, T, 1.2f);
                // face pintada: degradê de luz, textura leve de tinta e sombra sob o tampo
                GradV(g, Art.Lt(p.Face, 0.1f), Art.Dk(p.Face, 0.1f), X, Yface, T, H);
                var rnd = new Random(p.Face.ToArgb() ^ (int)H);
                for (int i = 0; i < 30; i++)
                    g.FillEllipse(Art.B(Art.A(rnd.Next(2) == 0 ? Art.Lt(p.Face, 0.2f) : Art.Dk(p.Face, 0.1f), 35)), X + (float)rnd.NextDouble() * T, Yface + (float)rnd.NextDouble() * H, 1, 1);
                GradV(g, Art.A(Color.Black, 55), Art.A(Color.Black, 0), X, Yface, T, Math.Min(6, H));
                if (H > LOW_H)
                {
                    GradV(g, Art.Dk(p.Face, 0.3f), Art.Dk(p.Face, 0.45f), X, Yface + H - 6, T, 6);
                    R(g, Art.A(Art.Lt(p.Face, 0.35f), 200), X, Yface + H - 6.6f, T, 0.8f);
                }
                return;
            }
            // vidro: moldura de alumínio, vidro azulado com reflexos e base sólida
            R(g, Art.A(Glass, 55), X, Ytop, T, T);
            GradV(g, Art.Lt(Alu, 0.3f), Alu, X, Ytop, T, 3);
            GradV(g, Alu, Art.Dk(Alu, 0.15f), X, Ytop + T - 3, T, 3);
            GradV(g, Art.A(Art.Lt(Glass, 0.15f), 150), Art.A(Glass, 70), X, Yface, T, H - 8);
            using (var path = new GraphicsPath())
            {
                path.AddPolygon(new[] { new PointF(X + 5, Yface + 2), new PointF(X + 12, Yface + 2), new PointF(X + 4, Yface + H - 10), new PointF(X - 3, Yface + H - 10) });
                g.SetClip(new RectangleF(X, Yface, T, H - 8), CombineMode.Intersect);
                g.FillPath(Art.B(Art.A(Color.White, 60)), path);
                g.ResetClip();
            }
            g.DrawLine(Pn(Art.A(Color.White, 110), 0.8f), X + 19, Yface + 4, X + 15, Yface + H - 16);
            GradV(g, Art.Hx(0xD3DAE3), Art.Hx(0xA9B4C2), X, Yface + H - 8, T, 8);
            R(g, Art.A(Color.White, 150), X, Yface + H - 8, T, 0.7f);
            GradH(g, Art.Dk(AluDk, 0.1f), AluDk, X, Yface, 1.2f, H);
        }

        // ───────────────────────── portas e placas ─────────────────────────

        public static void Door(Graphics g, DoorInfo d, bool locked, float t)
        {
            var p0 = d.Tiles[0];
            var o = P(p0.X, p0.Y + 1, GLASS_H);
            float X = o.X, Y = o.Y, W = 2 * T;
            // capacho
            var m = P(p0.X + 0.3f, p0.Y + 1.05f, 0);
            Art.RR(g, Art.A(Art.Hx(0x2F3644), 150), m.X, m.Y, 1.4f * T, 6, 2);
            // portas de vidro abertas (recolhidas nas laterais)
            GradV(g, Art.A(Art.Lt(Glass, 0.2f), 150), Art.A(Glass, 90), X + 2, Y + 4, 7, GLASS_H - 4);
            GradV(g, Art.A(Art.Lt(Glass, 0.2f), 150), Art.A(Glass, 90), X + W - 9, Y + 4, 7, GLASS_H - 4);
            g.DrawLine(Pn(Art.A(Color.White, 120), 0.7f), X + 4, Y + 6, X + 4, Y + GLASS_H - 4);
            // batentes e verga
            GradH(g, Art.Lt(AluDk, 0.2f), Art.Dk(AluDk, 0.2f), X, Y, 3, GLASS_H);
            GradH(g, Art.Lt(AluDk, 0.2f), Art.Dk(AluDk, 0.2f), X + W - 3, Y, 3, GLASS_H);
            GradV(g, Art.Lt(Alu, 0.35f), Art.Dk(Alu, 0.1f), X - 1, Y - 5, W + 2, 6);
            g.DrawLine(Pn(Art.A(Edge, 160), 0.7f), X - 1, Y + 1, X + W, Y + 1);
            Color led = locked ? Art.Hx(0xF85149) : Art.Hx(0x3FB950);
            SoftEllipse(g, X + W / 2, Y - 2.5f, 5, 3, Art.A(led, 110));
            Art.RR(g, led, X + W / 2 - 2, Y - 4, 4, 3, 1);
        }

        public static string PlateLabel(DoorInfo d, bool locked, bool done)
        {
            return d.Chapter == Story.Library ? "BIBLIOTECA" : (done ? "✔ " : "") + Story.SectorOf(d.Chapter).Short;
        }

        // retângulo da placa (acima da porta), em pixels do mundo
        public static RectangleF PlateRect(DoorInfo d)
        {
            var p0 = d.Tiles[0];
            var o = P(p0.X + 1, p0.Y + 1, GLASS_H + 4);
            float w = Math.Max(3.2f, d.Plate.Width) * T * 0.92f;
            return new RectangleF(o.X - w / 2, o.Y - 15, w, 13);
        }

        public static void Plate(Graphics g, DoorInfo d, bool locked, bool done)
        {
            var r = PlateRect(d);
            Color bg = d.Chapter == Story.Library ? Art.Hx(0x6B3E26) : locked ? Art.Hx(0x5C2430) : done ? Art.Hx(0x1F6F43) : Art.Hx(0x1F4E8C);
            var cat = Challenges.CatOfRoom(d.Chapter);
            SoftRect(g, new RectangleF(r.X + 1, r.Y + 2, r.Width, r.Height), 2, 70);
            using (var p = Art.RoundPath(r.X, r.Y, r.Width, r.Height, 2))
            using (var lg = new LinearGradientBrush(r, Art.Lt(bg, 0.15f), Art.Dk(bg, 0.15f), 90f))
                g.FillPath(lg, p);
            if (cat != null) Art.RR(g, cat.Color, r.X, r.Y, 4, r.Height, 1.5f);
            R(g, Art.A(Color.White, 60), r.X + 2, r.Y + 0.6f, r.Width - 4, 0.8f);
            R(g, AluDk, r.X + 6, r.Y + r.Height, 1, 3);
            R(g, AluDk, r.X + r.Width - 7, r.Y + r.Height, 1, 3);
        }

        // ───────────────────────── decoração de parede ─────────────────────────

        // retângulo útil da decoração na face da parede
        public static RectangleF DecorRect(Furn d, bool glass)
        {
            float h = glass ? GLASS_H : WALL_H;
            var o = P(d.X, d.Y + 1, h);
            float dh = glass ? 22 : 30;
            return new RectangleF(o.X + 3, o.Y + 6, d.W * T - 6, dh);
        }

        public static void Decor(Graphics g, Furn d, bool glass)
        {
            var r = DecorRect(d, glass);
            float x = r.X, y = r.Y, w = r.Width, h = r.Height;
            switch (d.Kind)
            {
                case "window":
                    GradV(g, Art.Lt(Alu, 0.25f), Art.Dk(Alu, 0.1f), x, y - 2, w, h + 4);
                    GradV(g, Art.Hx(0xCDEBFF), Art.Hx(0x6FAEE0), x + 2, y, w - 4, h);
                    // nuvem e prédios ao longe
                    g.FillEllipse(Art.B(Art.A(Color.White, 170)), x + w * 0.15f, y + 3, w * 0.25f, 5);
                    g.FillEllipse(Art.B(Art.A(Color.White, 150)), x + w * 0.28f, y + 2, w * 0.2f, 5);
                    R(g, Art.A(Art.Hx(0x8FA5BC), 200), x + 4, y + h - 10, 6, 6); R(g, Art.A(Art.Hx(0x7D93AA), 200), x + 11, y + h - 13, 5, 9);
                    R(g, Art.A(Art.Hx(0x8FB996), 230), x + 3, y + h - 5, w - 6, 4);
                    using (var path = new GraphicsPath())
                    {
                        path.AddPolygon(new[] { new PointF(x + 6, y), new PointF(x + 12, y), new PointF(x + 6, y + h), new PointF(x, y + h) });
                        g.SetClip(new RectangleF(x + 2, y, w - 4, h), CombineMode.Intersect);
                        g.FillPath(Art.B(Art.A(Color.White, 70)), path);
                        g.ResetClip();
                    }
                    GradH(g, Art.Lt(Alu, 0.2f), Art.Dk(Alu, 0.15f), x + w / 2 - 1, y, 2, h);
                    break;
                case "board":
                    SoftRect(g, new RectangleF(x + 1, y + 2, w, h), 2, 60);
                    GradV(g, Art.Hx(0x7A5636), Art.Hx(0x5E4129), x, y, w, h);
                    GradV(g, Art.Lt(d.C, 0.08f), Art.Dk(d.C, 0.08f), x + 2, y + 2, w - 4, h - 4);
                    {
                        var notes = new[] { 0xFFFFFF, 0xFFE66D, 0xFF8FAB, 0xA0E7E5 };
                        for (int i = 0; i < (int)(w / 9); i++)
                        {
                            float nx = x + 4 + i * 9, ny = y + 4 + (i % 2) * 8;
                            R(g, Art.A(Color.Black, 40), nx + 0.8f, ny + 0.8f, 7, 7);
                            R(g, Art.Hx(notes[i % 4]), nx, ny, 7, 7);
                            g.FillEllipse(Art.B(Art.Hx(0xE63946)), nx + 2.6f, ny + 0.2f, 1.6f, 1.6f);
                        }
                    }
                    break;
                case "whiteboard":
                    SoftRect(g, new RectangleF(x + 1, y + 2, w, h), 2, 60);
                    GradV(g, Art.Hx(0xB4BDC9), Art.Hx(0x8E99A8), x, y, w, h);
                    GradV(g, Art.Hx(0xFFFFFF), Art.Hx(0xEEF1F5), x + 2, y + 2, w - 4, h - 4);
                    using (var path = new GraphicsPath())
                    {
                        path.AddPolygon(new[] { new PointF(x + w * 0.55f, y + 2), new PointF(x + w * 0.7f, y + 2), new PointF(x + w * 0.5f, y + h - 2), new PointF(x + w * 0.35f, y + h - 2) });
                        g.FillPath(Art.B(Art.A(Color.White, 120)), path);
                    }
                    g.DrawLine(Pn(Art.Hx(0xE63946), 0.8f), x + 6, y + h - 7, x + w * 0.6f, y + h - 7);
                    g.DrawLine(Pn(Art.Hx(0x2A9D8F), 0.8f), x + 6, y + h - 4, x + w * 0.8f, y + h - 4);
                    GradV(g, Art.Hx(0xA7B1BE), Art.Hx(0x7F8A99), x + 4, y + h, w - 8, 2.5f);
                    break;
                case "screen":
                case "dashboard":
                    SoftRect(g, new RectangleF(x + 1, y + 2, w, h), 2, 70);
                    GradV(g, Art.Hx(0x34404F), Art.Hx(0x222B36), x, y, w, h);
                    GradV(g, Art.Hx(0x102033), Art.Hx(0x07101A), x + 2, y + 2, w - 4, h - 4);
                    {
                        var rnd = new Random((int)(d.X * 31 + d.Y));
                        int panels = Math.Max(1, (int)(d.W / 1.5f));
                        float pw = (w - 4) / panels;
                        for (int p = 0; p < panels; p++)
                        {
                            var pts = new PointF[7];
                            for (int i = 0; i < 7; i++) pts[i] = new PointF(x + 2 + p * pw + 3 + i * (pw - 6) / 6, y + 6 + (float)rnd.NextDouble() * (h - 11));
                            Color lc = p % 2 == 0 ? Art.Hx(0x3FB950) : Art.Hx(0x58A6FF);
                            g.DrawLines(Pn(Art.A(lc, 70), 2f), pts);
                            g.DrawLines(Pn(lc, 0.8f), pts);
                            g.FillEllipse(Art.B(rnd.Next(5) == 0 ? Art.Hx(0xF85149) : Art.Hx(0x3FB950)), x + 2 + p * pw + 3, y + 4, 2, 2);
                        }
                    }
                    R(g, Art.A(Color.White, 25), x + 2, y + 2, w - 4, (h - 4) * 0.4f);
                    break;
                case "kanban":
                    SoftRect(g, new RectangleF(x + 1, y + 2, w, h), 2, 60);
                    GradV(g, Art.Hx(0xB4BDC9), Art.Hx(0x8E99A8), x, y, w, h);
                    GradV(g, Art.Hx(0xFFFFFF), Art.Hx(0xEEF1F5), x + 2, y + 2, w - 4, h - 4);
                    {
                        var cols = new[] { 0xE63946, 0xF4A261, 0x2A9D8F };
                        float cw = (w - 4) / 3;
                        for (int c = 0; c < 3; c++)
                        {
                            Art.RR(g, Art.Hx(cols[c]), x + 3 + c * cw, y + 3, cw - 2, 3, 1);
                            for (int k = 0; k < 3; k++)
                            {
                                float kx = x + 5 + c * cw + (k % 2) * 8, ky = y + 8 + k * 6;
                                R(g, Art.A(Color.Black, 35), kx + 0.6f, ky + 0.6f, 7, 5);
                                R(g, Art.Hx(new[] { 0xFFE66D, 0xFF8FAB, 0xA0E7E5 }[(c + k) % 3]), kx, ky, 7, 5);
                            }
                        }
                    }
                    break;
                case "sign":
                    SoftRect(g, new RectangleF(x + 1, y + 6, w, h - 10), 2, 70);
                    using (var p = Art.RoundPath(x, y + 4, w, h - 10, 2)) using (var lg = new LinearGradientBrush(new RectangleF(x, y + 4, w, h - 10), Art.Lt(d.C, 0.18f), Art.Dk(d.C, 0.15f), 90f)) g.FillPath(lg, p);
                    R(g, Art.A(Color.White, 70), x + 2, y + 4.6f, w - 4, 0.8f);
                    break;
                case "poster":
                    {
                        float pw = Math.Min(w, 24), px = x + (w - pw) / 2;
                        SoftRect(g, new RectangleF(px + 1, y + 2, pw, h), 2, 60);
                        R(g, Art.Hx(0xF4F6F9), px, y, pw, h);
                        GradV(g, Art.Lt(d.C, 0.12f), Art.Dk(d.C, 0.12f), px + 2, y + 2, pw - 4, h - 4);
                        R(g, Art.A(Color.White, 220), px + 5, y + 5, pw - 10, 3);
                        g.FillEllipse(Art.B(Art.A(Color.White, 200)), px + pw / 2 - 5, y + h / 2 - 3, 10, 10);
                        R(g, Art.A(Color.White, 160), px + 5, y + h - 7, pw - 10, 2);
                        break;
                    }
            }
        }

        // ───────────────────────── móveis ─────────────────────────

        public static float FurnKey(Furn f) { return f.Y + f.H; }

        static void Monitor(Graphics g, float cx, float y, float zb, float w, float h, Color scr, float t = 0, bool code = false, int mode = 0)
        {
            var f = Front(cx - w / 2, y, w, 0.1f, zb + 4, zb + 4 + h);
            if (mode != 2)
            {
                Block(g, cx - 0.06f, y + 0.12f, 0.12f, 0.08f, zb, zb + 5, Art.Hx(0x2A2F3A), false);
                Block(g, cx - w / 2, y, w, 0.1f, zb + 4, zb + 4 + h, Art.Hx(0x2A2F3A));
                GradV(g, Art.Lt(scr, 0.15f), Art.Dk(scr, 0.2f), f.X + 1.5f, f.Y + 1.5f, f.Width - 3, f.Height - 3);
                if (code)
                {
                    R(g, Art.Hx(0x7EE787), f.X + 4, f.Y + 4, 4, 1);
                    R(g, Art.Hx(0xE6EDF3), f.X + 10, f.Y + 4, f.Width * 0.45f, 1);
                    R(g, Art.Hx(0x58A6FF), f.X + 4, f.Y + 7, f.Width * 0.55f, 1);
                    R(g, Art.Hx(0xFFA657), f.X + 8, f.Y + 10, f.Width * 0.35f, 1);
                }
                else
                {
                    R(g, Art.A(Color.White, 90), f.X + 4, f.Y + 4, f.Width * 0.45f, 1);
                    R(g, Art.A(Color.White, 70), f.X + 4, f.Y + 7, f.Width * 0.6f, 1);
                }
                // reflexo da tela
                using (var path = new GraphicsPath())
                {
                    path.AddPolygon(new[] { new PointF(f.X + f.Width * 0.55f, f.Y + 1.5f), new PointF(f.X + f.Width * 0.75f, f.Y + 1.5f), new PointF(f.X + f.Width * 0.5f, f.Bottom - 1.5f), new PointF(f.X + f.Width * 0.3f, f.Bottom - 1.5f) });
                    g.FillPath(Art.B(Art.A(Color.White, 28)), path);
                }
            }
            if (mode != 1 && code && (int)(t * 2) % 2 == 0) R(g, Art.Hx(0x7EE787), f.X + 4, f.Y + 13, 3, 2);
        }

        static void Legs(Graphics g, float x, float y, float w, float d, float z, Color c)
        {
            float s = 0.08f;
            Block(g, x, y + d - s, s, s, 0, z, c, false);
            Block(g, x + w - s, y + d - s, s, s, 0, z, c, false);
        }

        // forma arredondada com volume (luz vindo de cima, à esquerda)
        static void Blob(Graphics g, float x, float y, float w, float h, Color c)
        {
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(x, y, w, h);
                using (var pg = new PathGradientBrush(path))
                {
                    pg.CenterPoint = new PointF(x + w * 0.38f, y + h * 0.32f);
                    pg.CenterColor = Art.Lt(c, 0.28f);
                    pg.SurroundColors = new[] { Art.Dk(c, 0.18f) };
                    g.FillPath(pg, path);
                }
                g.DrawPath(Pn(Art.A(Art.Dk(c, 0.5f), 150), 0.6f), path);
            }
        }

        // folhagem: vários tufos com volume e alguns brilhos
        static void Foliage(Graphics g, float cx, float cy, float s, Color c, int seed)
        {
            var rnd = new Random(seed);
            for (int i = 0; i < 7; i++)
            {
                float a = (float)(i * Math.PI * 2 / 7 + rnd.NextDouble() * 0.5), d = s * 0.38f;
                float bw = s * (0.42f + (float)rnd.NextDouble() * 0.12f);
                Blob(g, cx + (float)Math.Cos(a) * d - bw / 2, cy + (float)Math.Sin(a) * d * 0.7f - bw / 2, bw, bw * 0.9f, i % 2 == 0 ? Art.Dk(c, 0.08f) : c);
            }
            Blob(g, cx - s * 0.3f, cy - s * 0.32f, s * 0.6f, s * 0.55f, Art.Lt(c, 0.08f));
            for (int i = 0; i < 4; i++)
                g.FillEllipse(Art.B(Art.A(Art.Lt(c, 0.5f), 150)), cx - s * 0.25f + (float)rnd.NextDouble() * s * 0.4f, cy - s * 0.3f + (float)rnd.NextDouble() * s * 0.3f, s * 0.08f, s * 0.05f);
        }

        static void Laptop(Graphics g, float cx, float y, float z)
        {
            Block(g, cx - 0.22f, y, 0.44f, 0.24f, z, z + 1, Art.Hx(0xC4CBD5), false);
            Block(g, cx - 0.22f, y - 0.06f, 0.44f, 0.05f, z + 1, z + 11, Art.Hx(0x3A3F4B), false);
            var f = Front(cx - 0.22f, y - 0.06f, 0.44f, 0.05f, z + 1, z + 11);
            GradV(g, Art.Hx(0x79B8FF), Art.Hx(0x3A80D9), f.X + 1, f.Y + 1, f.Width - 2, f.Height - 2);
        }

        public static void Furniture(Graphics g, Furn f, float t) { Furniture(g, f, t, 0); }

        // mode 0 = tudo, 1 = só a parte fixa (vai para o cache), 2 = só a parte animada (desenhada a cada quadro)
        public static void Furniture(Graphics g, Furn f, float t, int mode)
        {
            float x = f.X, y = f.Y, w = f.W, h = f.H;
            bool st = mode != 2, dyn = mode != 1;
            switch (f.Kind)
            {
                case "desk":
                    {
                        if (!st) return;
                        Legs(g, x + 0.08f, y + 0.12f, w - 0.16f, h - 0.24f, 14, Art.Dk(f.C, 0.35f));
                        Block(g, x + 0.04f, y + 0.08f, w - 0.08f, h - 0.16f, 14, 18, f.C);
                        if (f.Var == 2) { Monitor(g, x + w * 0.3f, y + 0.18f, 18, 0.62f, 15, Art.Hx(0x1F6FEB)); Monitor(g, x + w * 0.72f, y + 0.18f, 18, 0.62f, 15, Art.Hx(0x2EA043)); }
                        else if (f.Var == 1) Monitor(g, x + w / 2, y + 0.18f, 18, 0.62f, 15, Art.Hx(0x3B82C4));
                        var kb = P(x + w / 2 - 0.25f, y + 0.55f, 18);
                        Art.RR(g, Art.Hx(0xDDE2E9), kb.X, kb.Y, 0.5f * T, 4, 1);
                        R(g, Art.Hx(0xAEB6C2), kb.X, kb.Y + 4, 0.5f * T, 1);
                        for (int i = 0; i < 6; i++) R(g, Art.Hx(0xC3CAD4), kb.X + 1.5f + i * 2.5f, kb.Y + 1, 1.6f, 1);
                        if (f.Var != 0) { var mug = P(x + w - 0.3f, y + 0.5f, 18); Art.RR(g, Color.White, mug.X, mug.Y - 4, 4, 5, 1); R(g, Art.Hx(0x6F4E37), mug.X + 0.5f, mug.Y - 4, 3, 1); }
                        break;
                    }
                case "terminal":
                    {
                        if (st)
                        {
                            Block(g, x + 0.04f, y + 0.08f, w - 0.08f, h - 0.16f, 0, 18, Art.Hx(0x313847), Art.Hx(0x262B36));
                            var fr = Front(x + 0.04f, y + 0.08f, w - 0.08f, h - 0.16f, 0, 18);
                            GradH(g, Art.Docker, Art.Lt(Art.Docker, 0.3f), fr.X, fr.Y + 3, fr.Width, 2);
                            var kb = P(x + w / 2 - 0.3f, y + 0.55f, 18);
                            Art.RR(g, Art.Hx(0xDDE2E9), kb.X, kb.Y, 0.6f * T, 4, 1);
                        }
                        Monitor(g, x + w / 2, y + 0.16f, 18, 1.3f, 22, Art.Hx(0x0B2A4A), t, true, mode);
                        break;
                    }
                case "counter":
                    {
                        if (!st) return;
                        Block(g, x, y, w, h, 0, 20, Art.Hx(0xF5F7FA), Art.Hx(0xE2E7EE));
                        var fr = Front(x, y, w, h, 0, 20);
                        GradH(g, Art.Docker, Art.Lt(Art.Docker, 0.3f), fr.X, fr.Y + 6, fr.Width, 3);
                        Monitor(g, x + w * 0.35f, y + 0.25f, 20, 0.6f, 14, Art.Hx(0x3B82C4));
                        break;
                    }
                case "plant":
                    {
                        if (!st) return;
                        Block(g, x + 0.3f, y + 0.45f, 0.4f, 0.35f, 0, 11, Art.Hx(0xC97A48), Art.Hx(0x9C5530));
                        var rim = P(x + 0.3f, y + 0.45f, 11);
                        g.FillEllipse(Art.B(Art.Hx(0x4A3426)), rim.X + 1, rim.Y + 1, 0.4f * T - 2, 0.35f * T - 2);
                        var c = P(x + 0.5f, y + 0.55f, 11);
                        Foliage(g, c.X, c.Y - 14, 22, Art.Hx(0x2F8A55), (int)(x * 7 + y * 3));
                        break;
                    }
                case "sofa":
                    {
                        if (!st) return;
                        Block(g, x, y + 0.05f, w, 0.35f, 0, 22, Art.Dk(f.C, 0.08f), Art.Dk(f.C, 0.22f));
                        Block(g, x + 0.05f, y + 0.35f, w - 0.1f, h - 0.4f, 0, 10, Art.Lt(f.C, 0.08f), Art.Dk(f.C, 0.15f));
                        for (float k = x + 0.12f; k < x + w - 0.6f; k += 1f)
                        {
                            var cs = P(k, y + 0.38f, 10);
                            using (var p = Art.RoundPath(cs.X + 1.5f, cs.Y + 1, T - 5, 0.5f * T, 3))
                            using (var lg = new LinearGradientBrush(new RectangleF(cs.X, cs.Y, T, 0.5f * T), Art.Lt(f.C, 0.18f), Art.Dk(f.C, 0.02f), 70f))
                                g.FillPath(lg, p);
                        }
                        Block(g, x, y + 0.2f, 0.18f, h - 0.25f, 0, 15, f.C);
                        Block(g, x + w - 0.18f, y + 0.2f, 0.18f, h - 0.25f, 0, 15, f.C);
                        break;
                    }
                case "bench":
                    {
                        if (!st) return;
                        if (h > w)
                        {
                            Legs(g, x + 0.3f, y + 0.1f, 0.4f, h - 0.2f, 8, Art.Hx(0x4A4E59));
                            Block(g, x + 0.25f, y + 0.05f, 0.5f, h - 0.1f, 8, 11, f.C);
                        }
                        else
                        {
                            Legs(g, x + 0.1f, y + 0.3f, w - 0.2f, 0.4f, 8, Art.Hx(0x4A4E59));
                            Block(g, x + 0.05f, y + 0.3f, w - 0.1f, 0.45f, 8, 11, f.C);
                            for (float k = x + 0.15f; k < x + w - 0.1f; k += 0.5f) { var s = P(k, y + 0.32f, 11); R(g, Art.A(Art.Dk(f.C, 0.2f), 160), s.X, s.Y, 0.7f, 0.4f * T); }
                        }
                        break;
                    }
                case "crate":
                    {
                        if (!st) return;
                        Block(g, x + 0.04f, y + 0.1f, w - 0.08f, h - 0.2f, 0, 24, f.C);
                        var fr = Front(x + 0.04f, y + 0.1f, w - 0.08f, h - 0.2f, 0, 24);
                        for (float k = fr.X + 5; k < fr.Right - 2; k += 6) R(g, Art.A(Art.Dk(f.C, 0.3f), 160), k, fr.Y + 1, 0.8f, fr.Height - 2);
                        Art.RR(g, Color.White, fr.X + 6, fr.Y + 6, 12, 6, 1);
                        R(g, Art.Dk(f.C, 0.2f), fr.X + 8, fr.Y + 8, 8, 1);
                        break;
                    }
                case "rack":
                    {
                        var fr = Front(x + 0.08f, y + 0.15f, 0.84f, 0.7f, 0, 58);
                        if (st)
                        {
                            Block(g, x + 0.08f, y + 0.15f, 0.84f, 0.7f, 0, 58, Art.Hx(0x2B3446), Art.Hx(0x1C2230));
                            for (int i = 0; i < 8; i++)
                            {
                                float sy = fr.Y + 3 + i * 6.8f;
                                GradV(g, Art.Hx(0x37425A), Art.Hx(0x252D3D), fr.X + 2, sy, fr.Width - 4, 4.6f);
                                for (int k = 0; k < 4; k++) R(g, Art.A(Color.Black, 120), fr.X + 12 + k * 3, sy + 1.2f, 1.6f, 2.2f);
                            }
                        }
                        if (dyn)
                            for (int i = 0; i < 8; i++)
                            {
                                float sy = fr.Y + 3 + i * 6.8f;
                                bool on = ((int)(t * 3 + f.Var * 7 + i * 13) % 5) != 0;
                                Color lc = (i + f.Var) % 3 == 0 ? Art.Hx(0x58A6FF) : Art.Hx(0x3FB950);
                                if (on) { g.FillEllipse(Art.B(Art.A(lc, 60)), fr.X + 2.5f, sy - 0.3f, 5, 4.6f); g.FillEllipse(Art.B(lc), fr.X + 4, sy + 1, 2, 2); }
                                if ((int)(t * 5 + f.Var + i) % 3 == 0) g.FillEllipse(Art.B(Art.Hx(0xF0B429)), fr.X + 8, sy + 1, 2, 2);
                            }
                        break;
                    }
                case "table":
                case "bigdesk":
                    {
                        if (!st) return;
                        Legs(g, x + 0.1f, y + 0.1f, w - 0.2f, h - 0.2f, 15, Art.Dk(f.C, 0.35f));
                        if (f.Kind == "bigdesk") Block(g, x + 0.1f, y + h - 0.3f, w - 0.2f, 0.12f, 1, 15, Art.Dk(f.C, 0.2f), false);
                        Block(g, x + 0.04f, y + 0.04f, w - 0.08f, h - 0.08f, 15, 19, f.C);
                        if (f.Kind == "bigdesk")
                        {
                            Laptop(g, x + w * 0.5f, y + 0.3f, 19);
                            var lamp = P(x + w * 0.16f, y + 0.35f, 19);
                            R(g, Art.Hx(0x3A3F4B), lamp.X, lamp.Y - 10, 2, 10);
                            SoftEllipse(g, lamp.X + 0.5f, lamp.Y - 11, 9, 6, Art.A(Art.Hx(0xFFE08A), 90));
                            Blob(g, lamp.X - 4, lamp.Y - 15, 9, 6, Art.Hx(0xFFE08A));
                            var pp = P(x + w * 0.75f, y + 0.4f, 19);
                            R(g, Color.White, pp.X, pp.Y, 10, 7); R(g, Art.Hx(0xD9DEE5), pp.X + 2, pp.Y + 2, 6, 1);
                        }
                        else if (w > 3) for (float k = x + 0.8f; k < x + w - 0.5f; k += 1.3f) Laptop(g, k, y + 0.35f, 19);
                        break;
                    }
                case "labtable":
                    {
                        if (!st) return;
                        Legs(g, x + 0.1f, y + 0.12f, w - 0.2f, h - 0.24f, 14, Art.Hx(0x9AA7B5));
                        Block(g, x + 0.04f, y + 0.08f, w - 0.08f, h - 0.16f, 14, 17, Art.Hx(0xF1F4F8), Art.Hx(0xC9D2DC));
                        var cols = new[] { 0x39C5CF, 0xD65AA0, 0x3FB950, 0xF0B429 };
                        for (int i = 0; i < (int)(w * 2) - 1; i++)
                        {
                            float bx = x + 0.3f + i * 0.45f;
                            Block(g, bx, y + 0.3f, 0.14f, 0.12f, 17, 25, Art.A(Art.Hx(cols[i % 4]), 230), false);
                            var c = P(bx, y + 0.3f, 25);
                            R(g, Art.A(Color.White, 150), c.X + 1, c.Y + 1, 1, 4);
                        }
                        break;
                    }
                case "beanbag":
                    {
                        if (!st) return;
                        var c = P(x + 0.5f, y + 0.6f, 0);
                        Blob(g, c.X - 13, c.Y - 14, 26, 18, f.C);
                        g.FillEllipse(Art.B(Art.A(Art.Lt(f.C, 0.35f), 150)), c.X - 8, c.Y - 12, 10, 5);
                        break;
                    }
                case "pingpong":
                    {
                        if (!st) return;
                        Legs(g, x + 0.12f, y + 0.12f, w - 0.24f, h - 0.24f, 15, Art.Hx(0x2B2D42));
                        var top = Block(g, x + 0.04f, y + 0.04f, w - 0.08f, h - 0.08f, 15, 18, Art.Hx(0x22875A), Art.Hx(0x175C3A));
                        g.DrawRectangle(Pn(Color.White, 0.8f), top.X + 2, top.Y + 2, top.Width - 4, top.Height - 4);
                        g.DrawLine(Pn(Color.White, 0.6f), top.X + 2, top.Y + top.Height / 2, top.Right - 2, top.Y + top.Height / 2);
                        GradV(g, Art.A(Color.White, 220), Art.A(Color.White, 140), top.X + top.Width / 2 - 1, top.Y - 4, 2, top.Height + 4);
                        g.FillEllipse(Art.B(Art.Hx(0xF4A261)), top.X + top.Width * 0.3f, top.Y + 6, 3, 3);
                        break;
                    }
                case "shelf":
                    {
                        if (!st) return;
                        Block(g, x + 0.04f, y + 0.15f, w - 0.08f, h - 0.2f, 0, 48, f.C);
                        var fr = Front(x + 0.04f, y + 0.15f, w - 0.08f, h - 0.2f, 0, 48);
                        var books = new[] { 0xE63946, 0x457B9D, 0xF4A261, 0x2A9D8F, 0xE9C46A, 0x8E6CC4 };
                        for (int r = 0; r < 4; r++)
                        {
                            float ry = fr.Y + 3 + r * 11;
                            GradV(g, Art.Dk(f.C, 0.5f), Art.Dk(f.C, 0.35f), fr.X + 2, ry, fr.Width - 4, 9);
                            for (int b = 0; b < (int)(fr.Width / 4) - 1; b++)
                            {
                                int hh = Hash((int)(x * 10) + b, r + (int)(y * 7));
                                float bh = 8 - (hh % 3 == 0 ? 2 : 0);
                                Color bc = Art.Hx(books[hh % books.Length]);
                                GradH(g, Art.Lt(bc, 0.15f), Art.Dk(bc, 0.2f), fr.X + 3 + b * 4, ry + 9 - bh, 3.2f, bh);
                                R(g, Art.A(Color.White, 90), fr.X + 3.6f + b * 4, ry + 10.5f - bh, 2, 0.6f);
                            }
                        }
                        break;
                    }
                case "coffee":
                    {
                        var fr = Front(x + 0.12f, y + 0.2f, 0.76f, 0.6f, 0, 44);
                        if (st)
                        {
                            Block(g, x + 0.12f, y + 0.2f, 0.76f, 0.6f, 0, 44, Art.Hx(0x737C8A), Art.Hx(0x4E5561));
                            GradV(g, Art.Hx(0x2A2F37), Art.Hx(0x15181D), fr.X + 3, fr.Y + 3, fr.Width - 6, 10);
                            R(g, Art.Hx(0x7EE787), fr.X + 5, fr.Y + 5, 7, 3);
                            g.FillEllipse(Art.B(Art.Hx(0xF85149)), fr.Right - 8, fr.Y + 5, 3, 3);
                            GradV(g, Art.Hx(0x22262C), Art.Hx(0x34393F), fr.X + 5, fr.Y + 18, fr.Width - 10, 16);
                            Art.RR(g, Color.White, fr.X + fr.Width / 2 - 3, fr.Y + 27, 6, 6, 1);
                            R(g, Art.Hx(0xF4A261), fr.X + 5, fr.Y + 15, fr.Width - 10, 2);
                        }
                        if (dyn)
                            for (int k = 0; k < 3; k++)
                            {
                                float ph = (t * 1.3f + k * 0.33f) % 1f;
                                g.FillEllipse(Art.B(Art.A(Color.White, (int)(110 * (1 - ph)))), fr.X + fr.Width / 2 - 1.5f + (float)Math.Sin(ph * 6 + k) * 1.2f, fr.Y + 25 - ph * 8, 2.4f, 2.4f);
                            }
                        break;
                    }
                case "cooler":
                    {
                        if (!st) return;
                        Block(g, x + 0.28f, y + 0.4f, 0.44f, 0.4f, 0, 20, Art.Hx(0xF4F6FA), Art.Hx(0xD5DCE5));
                        var c = P(x + 0.5f, y + 0.55f, 20);
                        Blob(g, c.X - 6, c.Y - 16, 12, 17, Art.A(Art.Hx(0x8CCBF2), 225));
                        R(g, Art.A(Color.White, 160), c.X - 3, c.Y - 13, 1.6f, 9);
                        var fr = Front(x + 0.28f, y + 0.4f, 0.44f, 0.4f, 0, 20);
                        g.FillEllipse(Art.B(Art.Docker), fr.X + 3, fr.Y + 5, 3, 3);
                        g.FillEllipse(Art.B(Art.Hx(0xF85149)), fr.X + 9, fr.Y + 5, 3, 3);
                        break;
                    }
                case "ac":
                    {
                        if (!st) return;
                        Block(g, x + 0.1f, y + 0.1f, w - 0.2f, h - 0.2f, 0, 44, Art.Hx(0xF1F4F8), Art.Hx(0xD5DCE5));
                        var fr = Front(x + 0.1f, y + 0.1f, w - 0.2f, h - 0.2f, 0, 44);
                        for (int k = 0; k < 7; k++) GradV(g, Art.Hx(0xBCC6D3), Art.Hx(0x9AA6B5), fr.X + 3, fr.Y + 6 + k * 5, fr.Width - 6, 2);
                        break;
                    }
                case "kiosk":
                    {
                        if (!st) return;
                        Block(g, x, y, w, 0.3f, 0, 40, Art.Hx(0x7A5638), Art.Hx(0x6B4A2F));
                        var sf = Front(x, y, w, 0.3f, 0, 40);
                        var cols = new[] { 0x2496ED, 0xE63946, 0xF0B429, 0x2A9D8F, 0x8E6CC4, 0xF4A261 };
                        for (int i = 0; i < (int)(sf.Width / 9); i++)
                        {
                            Art.RR(g, Art.Hx(cols[i % 6]), sf.X + 3 + i * 9, sf.Y + 6 + (i % 2) * 2, 7, 8, 1.5f);
                            Art.RR(g, Art.Hx(cols[(i + 3) % 6]), sf.X + 3 + i * 9, sf.Y + 20, 7, 7, 1.5f);
                        }
                        // toldo listrado com sombra
                        var aw = P(x - 0.05f, y, 50);
                        float aW = (w + 0.1f) * T;
                        for (int i = 0; i < (int)(aW / 8); i++) GradV(g, i % 2 == 0 ? Art.Hx(0xEF4B57) : Color.White, i % 2 == 0 ? Art.Hx(0xC62F3B) : Art.Hx(0xE4E7EC), aw.X + i * 8, aw.Y, 8, 10);
                        GradV(g, Art.Hx(0xB8202B), Art.Hx(0x8E1720), aw.X, aw.Y + 10, aW, 2);
                        GradV(g, Art.A(Color.Black, 60), Art.A(Color.Black, 0), aw.X, aw.Y + 12, aW, 5);
                        Block(g, x, y + 0.45f, w, 0.55f, 0, 18, Art.Hx(0xDDBE92), Art.Hx(0xB08A5B));
                        var fr = Front(x, y + 0.45f, w, 0.55f, 0, 18);
                        Art.RR(g, Art.Hx(0x1D3557), fr.X + fr.Width / 2 - 22, fr.Y + 4, 44, 10, 2);
                        var c1 = P(x + 0.35f, y + 0.6f, 18); Blob(g, c1.X - 3, c1.Y - 4, 7, 7, Art.Hx(0xF5C542));
                        var c2 = P(x + w - 0.35f, y + 0.6f, 18); Blob(g, c2.X - 3, c2.Y - 4, 7, 7, Art.Hx(0xF5C542));
                        break;
                    }
                case "buffet":
                    {
                        if (!st) return;
                        Block(g, x, y + 0.1f, w, h - 0.15f, 0, 18, Art.Hx(0xD7DDE5), Art.Hx(0xB8C0CC));
                        var top = Block(g, x - 0.03f, y + 0.07f, w + 0.06f, h - 0.1f, 18, 20, Art.Hx(0xF1F4F8), Art.Hx(0xC9D2DC));
                        var foods = new[] { 0xE9C46A, 0xE76F51, 0x8AB17D, 0xF4A261, 0xD62828, 0xFFFFFF };
                        for (int i = 0; i < (int)(w * 1.5f); i++)
                        {
                            float bx = top.X + 4 + i * 21;
                            GradV(g, Art.Hx(0xC3CCD8), Art.Hx(0x97A3B3), bx, top.Y + 5, 17, 12);
                            Blob(g, bx + 2, top.Y + 7, 13, 8, Art.Hx(foods[(i + f.Var) % foods.Length]));
                        }
                        GradV(g, Art.A(Art.Hx(0xBFE6FF), 150), Art.A(Art.Hx(0xBFE6FF), 60), top.X, top.Y - 10, top.Width, 3);
                        break;
                    }
                case "lunchtable":
                    {
                        if (!st) return;
                        Legs(g, x + 0.12f, y + 0.12f, w - 0.24f, h - 0.24f, 14, Art.Hx(0x5A6378));
                        var top = Block(g, x + 0.04f, y + 0.04f, w - 0.08f, h - 0.08f, 14, 17, f.C);
                        for (float k = top.X + 10; k < top.Right - 14; k += T)
                        {
                            Art.RR(g, Art.Hx(0xEAF0F6), k, top.Y + 6, 14, 10, 2);
                            Blob(g, k + 3, top.Y + 8, 8, 6, Art.Hx(((int)k / 7) % 2 == 0 ? 0xE9C46A : 0x8AB17D));
                        }
                        Block(g, x + 0.1f, y + h + 0.05f, w - 0.2f, 0.25f, 6, 9, Art.Hx(0x8D6E63), false);
                        break;
                    }
                case "stage":
                    {
                        if (!st) return;
                        Block(g, x, y, w, h, 0, 12, Art.Hx(0x8B5E3C), Art.Hx(0x5E3E25));
                        var top = P(x, y, 12);
                        for (float k = top.X + 6; k < top.X + w * T; k += 12) R(g, Art.A(Art.Hx(0x6A4428), 160), k, top.Y, 0.7f, h * T);
                        Block(g, x + w / 2 - 0.3f, y + 0.5f, 0.6f, 0.4f, 12, 34, Art.Hx(0x2B2D42));
                        var mic = P(x + w / 2, y + 0.5f, 40);
                        Art.RR(g, Art.Hx(0x58A6FF), mic.X - 2, mic.Y, 4, 2, 1);
                        break;
                    }
                case "chairs":
                    {
                        if (!st) return;
                        for (float k = x + 0.1f; k < x + w - 0.3f; k += 0.8f)
                        {
                            Block(g, k, y + 0.15f, 0.6f, 0.18f, 0, 24, Art.Dk(f.C, 0.05f), Art.Dk(f.C, 0.2f));
                            Block(g, k, y + 0.33f, 0.6f, 0.5f, 0, 10, Art.Lt(f.C, 0.1f), Art.Dk(f.C, 0.25f));
                        }
                        break;
                    }
                case "arcade":
                    {
                        var fr = Front(x + 0.12f, y + 0.25f, 0.76f, 0.55f, 0, 54);
                        if (st)
                        {
                            Block(g, x + 0.12f, y + 0.25f, 0.76f, 0.55f, 0, 54, Art.Lt(f.C, 0.05f), f.C);
                            GradV(g, Art.Lt(f.C, 0.4f), Art.Lt(f.C, 0.15f), fr.X + 3, fr.Y + 1, fr.Width - 6, 4);
                            GradV(g, Art.Hx(0x14202E), Art.Hx(0x070B10), fr.X + 3, fr.Y + 8, fr.Width - 6, 18);
                            GradV(g, Art.Hx(0x3A3D55), Art.Hx(0x24263A), fr.X + 3, fr.Y + 30, fr.Width - 6, 6);
                            g.FillEllipse(Art.B(Art.Hx(0xF85149)), fr.X + 7, fr.Y + 31, 3, 3);
                            g.FillEllipse(Art.B(Art.Hx(0x58A6FF)), fr.X + 13, fr.Y + 31, 3, 3);
                        }
                        if (dyn)
                        {
                            bool blink = (int)(t * 3 + f.Var) % 2 == 0;
                            Color c = blink ? Art.Hx(0xF5C542) : Art.Hx(0x3FB950);
                            SoftEllipseFast(g, fr.X + 8 + (f.Var % 3) * 4, fr.Y + 15, 6, 5, Art.A(c, 110));
                            R(g, c, fr.X + 6 + (f.Var % 3) * 4, fr.Y + 13, 4, 4);
                            R(g, Art.Hx(0xF85149), fr.Right - 10, fr.Y + 18, 3, 3);
                        }
                        break;
                    }
                case "tree":
                    {
                        if (!st) return;
                        var c = P(x + 0.5f, y + 0.7f, 0);
                        GradH(g, Art.Hx(0x8A5A33), Art.Hx(0x5E3B20), c.X - 3, c.Y - 26, 6, 26);
                        Foliage(g, c.X, c.Y - 46, 40, Art.Hx(0x2F8A55), f.Var * 13 + (int)x);
                        if (f.Var % 2 == 1) { Blob(g, c.X - 8, c.Y - 44, 3.5f, 3.5f, Art.Hx(0xE63946)); Blob(g, c.X + 7, c.Y - 54, 3.5f, 3.5f, Art.Hx(0xE63946)); Blob(g, c.X, c.Y - 38, 3.5f, 3.5f, Art.Hx(0xE63946)); }
                        break;
                    }
                case "fountain":
                    {
                        var to = P(x + 0.1f, y + 0.1f, 10);
                        var top = new RectangleF(to.X, to.Y, (w - 0.2f) * T, (h - 0.2f) * T);
                        if (st)
                        {
                            Block(g, x + 0.1f, y + 0.1f, w - 0.2f, h - 0.2f, 0, 10, Art.Hx(0xCDD4DE), Art.Hx(0x98A3B3));
                            GradV(g, Art.Hx(0x6CB6F5), Art.Hx(0x3A86D8), top.X + 5, top.Y + 5, top.Width - 10, top.Height - 10);
                            Block(g, x + w / 2 - 0.14f, y + h / 2 - 0.14f, 0.28f, 0.28f, 10, 26, Art.Hx(0xD5DCE5));
                        }
                        if (dyn)
                        {
                            float k = (float)Math.Sin(t * 5) * 2;
                            using (var p = Art.RoundPath(top.X + 12 + k, top.Y + 12 + k, top.Width - 24 - 2 * k, top.Height - 24 - 2 * k, 4)) g.DrawPath(Pn(Art.A(Color.White, 120), 0.8f), p);
                            var c = P(x + w / 2, y + h / 2, 28);
                            g.FillEllipse(Art.B(Art.A(Art.Hx(0xBFE6FF), 210)), c.X - 7, c.Y - 4 + k, 14, 7);
                            R(g, Art.A(Color.White, 190), c.X - 1, c.Y - 10 + k, 2, 7);
                        }
                        break;
                    }
                case "robot":
                    {
                        if (!dyn) return;
                        float hover = (float)Math.Sin(t * 3 + f.Var) * 1.5f;
                        var c = P(x + 0.5f, y + 0.7f, 0);
                        SoftEllipseFast(g, c.X, c.Y, 10, 4, Art.A(Color.Black, 70));
                        Block(g, x + 0.25f, y + 0.45f, 0.5f, 0.3f, 6 + hover, 22 + hover, Art.Hx(0xD5DCE5), Art.Hx(0xB5BFCC));
                        Block(g, x + 0.28f, y + 0.47f, 0.44f, 0.26f, 23 + hover, 34 + hover, Art.Hx(0xF1F4F8), Art.Hx(0xD5DCE5));
                        var fr = Front(x + 0.28f, y + 0.47f, 0.44f, 0.26f, 23 + hover, 34 + hover);
                        Art.RR(g, Art.Hx(0x0D1117), fr.X + 2, fr.Y + 2, fr.Width - 4, 6, 2);
                        g.FillEllipse(Art.B(Art.Hx(0x39C5CF)), fr.X + 4, fr.Y + 4, 2, 2);
                        g.FillEllipse(Art.B(Art.Hx(0x39C5CF)), fr.Right - 6, fr.Y + 4, 2, 2);
                        var ant = P(x + 0.5f, y + 0.6f, 40 + hover);
                        R(g, Art.Hx(0x6B7486), ant.X, ant.Y, 1, 6);
                        g.FillEllipse(Art.B((int)(t * 2 + f.Var) % 2 == 0 ? Art.Hx(0xF85149) : Art.Hx(0x5C2430)), ant.X - 1.2f, ant.Y - 2.2f, 3.4f, 3.4f);
                        break;
                    }
                case "vending":
                    {
                        if (!st) return;
                        Block(g, x + 0.1f, y + 0.25f, 0.8f, 0.55f, 0, 56, Art.Lt(f.C, 0.05f), f.C);
                        var fr = Front(x + 0.1f, y + 0.25f, 0.8f, 0.55f, 0, 56);
                        GradV(g, Art.A(Art.Hx(0xD7EEFF), 220), Art.A(Art.Hx(0x9CC9EE), 220), fr.X + 3, fr.Y + 4, fr.Width - 12, 36);
                        var cols = new[] { 0xE63946, 0xF4A261, 0x3FB950, 0x2496ED, 0xF5C542 };
                        for (int r = 0; r < 4; r++)
                            for (int k = 0; k < 4; k++)
                                Art.RR(g, Art.Hx(cols[(r + k) % 5]), fr.X + 5 + k * 4.5f, fr.Y + 6 + r * 8.5f, 3, 5, 0.8f);
                        R(g, Art.A(Color.White, 60), fr.X + 4, fr.Y + 5, 2, 34);
                        R(g, Art.Hx(0x1F2329), fr.Right - 8, fr.Y + 8, 5, 10);
                        R(g, Art.Hx(0x1F2329), fr.X + 3, fr.Y + 44, fr.Width - 6, 6);
                        break;
                    }
            }
        }

        public static void Glow(Graphics g, Furn desk, float t, bool strong)
        {
            var c = P(desk.X + desk.W / 2, desk.Y + desk.H + 0.25f, 0);
            float pulse = 0.5f + 0.5f * (float)Math.Sin(t * 3);
            int a = strong ? (int)(70 + 70 * pulse) : 40;
            float rw = (strong ? 1.6f + 0.15f * pulse : 1.3f) * T, rh = rw * 0.45f;
            SoftEllipseFast(g, c.X, c.Y, rw / 2, rh / 2, Art.A(Art.Docker, a));
            if (strong) g.DrawEllipse(Pn(Art.A(Art.Hx(0x8CCBF2), (int)(140 * pulse)), 1f), c.X - rw / 2, c.Y - rh / 2, rw, rh);
        }

        // ───────────────────────── sprites (personagens e mascotes), já na escala da tela ─────────────────────────

        static readonly Dictionary<string, Bitmap> sprites = new Dictionary<string, Bitmap>();
        public const int SW = 64, SH = 96, FootX = 32, FootY = 90, CharU = 40;
        public const int PW = 52, PH = 52, PFootX = 26, PFootY = 46;

        public static Bitmap CharSprite(Look look, int facing, bool moving, float walk)
        {
            // ciclo de caminhada com 6 quadros (passo esquerdo, passagem, passo direito...)
            int frame = moving ? 1 + (((int)Math.Floor(walk / (Math.PI / 3)) % 6) + 6) % 6 : 0;
            string key = "c" + look.Id + "|" + facing + "|" + frame + "|" + Q;
            Bitmap b;
            if (sprites.TryGetValue(key, out b)) return b;
            if (sprites.Count > 900) Clear();
            b = RenderChar(look, facing, frame, Q);
            sprites[key] = b;
            return b;
        }

        // quadro de um personagem numa escala qualquer, sem cache (o modo 3D usa em alta resolução)
        public static Bitmap RenderChar(Look look, int facing, int frame, int q)
        {
            var raw = new Bitmap(SW * q, SH * q, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(raw))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.ScaleTransform(q, q);
                Art.DrawShadows = false;
                Art.Character(g, FootX, FootY, CharU, look, facing, frame == 0 ? 0 : (frame - 1) * (float)Math.PI / 3 + (float)Math.PI / 2, frame != 0, 0);
                Art.DrawShadows = true;
            }
            Light(raw);
            var b = OutlineSprite(raw, Art.A(Edge, 170));
            raw.Dispose();
            return b;
        }

        // luz do escritório vindo de cima à esquerda: o lado esquerdo do corpo fica mais claro e os pés um pouco
        // mais escuros (dá volume ao personagem). Feito uma vez por quadro de animação, fica no cache.
        static void Light(Bitmap bmp)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var data = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
            int w = bmp.Width, h = bmp.Height;
            var px = new int[w * h];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, px, 0, px.Length);
            int minX = w, maxX = -1, minY = h, maxY = -1;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    if (((uint)px[y * w + x] >> 24) != 0)
                    {
                        if (x < minX) minX = x; if (x > maxX) maxX = x;
                        if (y < minY) minY = y; if (y > maxY) maxY = y;
                    }
            if (maxX > minX && maxY > minY)
            {
                float iw = 1f / (maxX - minX), ih = 1f / (maxY - minY);
                for (int y = minY; y <= maxY; y++)
                {
                    float v = (y - minY) * ih;
                    for (int x = minX; x <= maxX; x++)
                    {
                        int i = y * w + x;
                        uint c = (uint)px[i];
                        uint a = c >> 24;
                        if (a == 0) continue;
                        float u = (x - minX) * iw;
                        float f = 1.13f - 0.27f * u - 0.12f * v * v;
                        int r = Math.Min((int)a, (int)(((c >> 16) & 255) * f));
                        int gg = Math.Min((int)a, (int)(((c >> 8) & 255) * f));
                        int bb = Math.Min((int)a, (int)((c & 255) * f));
                        px[i] = (int)((a << 24) | ((uint)r << 16) | ((uint)gg << 8) | (uint)bb);
                    }
                }
                System.Runtime.InteropServices.Marshal.Copy(px, 0, data.Scan0, px.Length);
            }
            bmp.UnlockBits(data);
        }

        public static Bitmap PetSprite(string kind, int facing, bool moving, float t)
        {
            int frame = moving ? (int)(t * 8) % 2 : 0;
            string key = "p" + kind + "|" + facing + "|" + frame + "|" + Q;
            Bitmap b;
            if (sprites.TryGetValue(key, out b)) return b;
            b = RenderPet(kind, facing, frame, Q);
            sprites[key] = b;
            return b;
        }

        public static Bitmap RenderPet(string kind, int facing, int frame, int q)
        {
            var raw = new Bitmap(PW * q, PH * q, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(raw))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.ScaleTransform(q, q);
                Art.DrawShadows = false;
                Art.Pet(g, kind, PFootX, PFootY, CharU * 1.05f, facing, frame * 0.2f, frame == 1);
                Art.DrawShadows = true;
            }
            var b = OutlineSprite(raw, Art.A(Edge, 170));
            raw.Dispose();
            return b;
        }

        public static void Clear()
        {
            foreach (var b in sprites.Values) b.Dispose();
            sprites.Clear();
        }

        // contorno fino em volta do sprite (ajuda a destacar o personagem do chão)
        static Bitmap OutlineSprite(Bitmap src, Color c)
        {
            var dst = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(dst))
            using (var ia = new ImageAttributes())
            {
                var cm = new ColorMatrix(new[] {
                    new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 0, 0, 0, 0 },
                    new float[] { 0, 0, 0, c.A / 255f, 0 }, new[] { c.R / 255f, c.G / 255f, c.B / 255f, 0, 1 } });
                ia.SetColorMatrix(cm);
                var r = new Rectangle(0, 0, src.Width, src.Height);
                foreach (var o in new[] { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1) })
                    g.DrawImage(src, new Rectangle(o.X, o.Y, src.Width, src.Height), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, ia);
                g.DrawImage(src, r);
            }
            return dst;
        }

        // ───────────────────────── minimapa ─────────────────────────

        public static Bitmap RenderMini(World w, float m)
        {
            var bmp = new Bitmap((int)(World.W * m) + 4, (int)(World.H * m) + 4, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.None;
                for (int x = 0; x < World.W; x++)
                    for (int y = 0; y < World.H; y++)
                    {
                        int t = w.Tile[x, y];
                        if (t == World.Void) continue;
                        Color c = t == World.Wall ? Art.Hx(0x2E3546) : w.Zone[x, y] < 0 ? Art.Hx(0x9AA3B0) : Story.SectorOf(w.Zone[x, y]).F1;
                        var p = Mini(x, y, m);
                        g.FillRectangle(Art.B(c), p.X, p.Y, (float)Math.Ceiling(m), (float)Math.Ceiling(m));
                    }
            }
            return bmp;
        }

        public static PointF Mini(float x, float y, float m) { return new PointF(x * m + 2, y * m + 2); }
    }
}
