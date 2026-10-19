using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;

namespace DockerQuest
{
    struct V3 { public float X, Y, Z; public V3(float x, float y, float z) { X = x; Y = y; Z = z; } }

    // pedaço de uma folha de texturas (atlas)
    class Region { public int Page; public float U0, V0, U1, V1; public bool Emissive; }

    // malha montada na CPU: por folha de textura, vértices (x, y, z, u, v, r, g, b, a)
    class Mesh
    {
        public readonly Dictionary<int, List<float>> Data = new Dictionary<int, List<float>>();
        public bool Empty { get { return Data.Count == 0; } }
        // formas (quads e triângulos) na ordem em que entraram: só guardadas para desenhar ícones
        public bool Track;
        public readonly List<int[]> Shapes = new List<int[]>();
        public void Shape(int page, int verts)
        {
            if (!Track) return;
            List<float> l;
            if (!Data.TryGetValue(page, out l)) { l = new List<float>(); Data[page] = l; }
            Shapes.Add(new[] { page, l.Count, verts });
        }
        public void Vert(int page, V3 p, float u, float v, Color c)
        {
            List<float> l;
            if (!Data.TryGetValue(page, out l)) { l = new List<float>(); Data[page] = l; }
            l.Add(p.X); l.Add(p.Y); l.Add(p.Z); l.Add(u); l.Add(v); l.Add(c.R); l.Add(c.G); l.Add(c.B); l.Add(c.A);
        }
    }

    // personagem desenhado como "cartaz" sempre virado para a câmera
    class Billboard { public string Key; public string[] Fallbacks; public Func<int, Bitmap> Get; public float X, Y, W, H, FX, FY, Shadow; }
    // marca no chão (sombra, anel do objetivo, brilho do terminal)
    class Decal { public float X, Y, RX, RY; public Color C; public bool Ring; }

    class Frame3D
    {
        public float X, Y, Time;
        public int VW, VH;
        public IntPtr Hud;
        public float Blur;   // 0 a 1: fome/sede embaçando a visão
        public float Zoom = 3;
        public float CamX, CamY;
        public List<Avatar3D> Avatars = new List<Avatar3D>();
        public List<Billboard> Sprites = new List<Billboard>();
        public List<Decal> Decals = new List<Decal>();
    }

    // O escritório em 3D para a visão isométrica. Coordenadas do mundo: x leste, y sul (em tiles), z altura (1 tile ≈ 0,93 m).
    // No OpenGL: X = x, Y = z (para cima), Z = y. Tudo o que é fixo vira "display lists" por bloco de 8×8 tiles;
    // o chão usa as mesmas texturas HD da visão de cima (com tapetes, sombras e luz das janelas já pintadas).
    partial class World3D
    {
        public const float WH = 3.0f, HEAD = 2.45f, EyeH = 1.62f;
        public const float FovY = 64f, Near = 0.06f, Far = 70f, FogStart = 13f, FogEnd = 46f;
        const int CS = 8;   // tiles por bloco

        readonly World W;
        readonly Dictionary<int, Scene.WallPiece> wallAt = new Dictionary<int, Scene.WallPiece>();
        public Func<int, int, Bitmap> ChunkSource;
        bool gl12, gl14;
        float aniso = 1;
        public string Renderer = "", Version = "";
        public bool Software;

        class Cell { public Mesh O = new Mesh(), G = new Mesh(); public uint LO, LG, LF; public int CX, CY; public float X0, Y0, X1, Y1; public float Dist; public List<IsoWall> Walls = new List<IsoWall>(); public List<IsoDecor> Decor = new List<IsoDecor>(); }
        Cell[,] cells;
        int CW, CH;
        readonly List<Furn> racks = new List<Furn>(), robots = new List<Furn>();

        public World3D(World w, List<Scene.WallPiece> walls)
        {
            W = w;
            foreach (var p in walls) wallAt[p.X * 1000 + p.Y] = p;
        }

        // ───────────────────────── texturas ─────────────────────────

        class Page { public Bitmap Bmp; public Graphics G; public int X, Y, RowH; public uint Tex; public bool Dirty; }
        readonly List<Page> pages = new List<Page>();
        readonly Dictionary<string, Region> regions = new Dictionary<string, Region>();
        const int PageSize = 1024, Pad = 6;
        Region rNoise, rWhite, rPlaster, rCeil, rLight, rGlass, rShadow, rRing, rCode, rLaptop, rPing, rPlank, rAwning, rRobot;

        Page NewPage()
        {
            var p = new Page { Bmp = new Bitmap(PageSize, PageSize, PixelFormat.Format32bppArgb) };
            p.G = Graphics.FromImage(p.Bmp);
            p.G.Clear(Color.FromArgb(255, 128, 128, 128));
            p.G.CompositingMode = CompositingMode.SourceCopy;
            p.G.InterpolationMode = InterpolationMode.HighQualityBilinear;
            p.G.PixelOffsetMode = PixelOffsetMode.Half;
            pages.Add(p);
            return p;
        }

        Region Add(string key, Bitmap bmp, bool emissive)
        {
            Region r;
            if (regions.TryGetValue(key, out r)) { bmp.Dispose(); return r; }
            int w = bmp.Width, h = bmp.Height, max = PageSize - 2 * Pad;
            if (w > max || h > max) { float sc = Math.Min(max / (float)w, max / (float)h); w = Math.Max(1, (int)(w * sc)); h = Math.Max(1, (int)(h * sc)); }
            Page p = pages.LastOrDefault(q => q.Bmp != null);   // as páginas dos rostos não têm imagem: ficam de fora
            if (p != null && p.X + w + 2 * Pad > PageSize) { p.X = 0; p.Y += p.RowH; p.RowH = 0; }
            if (p == null || p.Y + h + 2 * Pad > PageSize) p = NewPage();
            int x = p.X + Pad, y = p.Y + Pad;
            // borda repetida em volta: os mipmaps não misturam a cor de regiões vizinhas
            using (var ia = new ImageAttributes())
            {
                ia.SetWrapMode(WrapMode.TileFlipXY);
                p.G.DrawImage(bmp, new Rectangle(x - Pad, y - Pad, w + 2 * Pad, h + 2 * Pad), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, ia);
                p.G.DrawImage(bmp, new Rectangle(x, y, w, h), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, ia);
            }
            bmp.Dispose();
            if (p.Tex != 0) p.Dirty = true;   // já estava na placa de vídeo: precisa mandar de novo
            r = new Region { Page = pages.IndexOf(p), U0 = (x + 0.5f) / PageSize, V0 = (y + 0.5f) / PageSize, U1 = (x + w - 0.5f) / PageSize, V1 = (y + h - 0.5f) / PageSize, Emissive = emissive };
            regions[key] = r;
            p.X += w + 2 * Pad;
            p.RowH = Math.Max(p.RowH, h + 2 * Pad);
            return r;
        }

        static Bitmap Bmp(int w, int h) { return new Bitmap(Math.Max(1, w), Math.Max(1, h), PixelFormat.Format32bppArgb); }

        static Graphics Gfx(Bitmap b)
        {
            var g = Graphics.FromImage(b);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            return g;
        }

        // imagem de tons de cinza feita pixel a pixel (rápido)
        static Bitmap Pixels(int w, int h, Func<int, int, Color> f)
        {
            var b = Bmp(w, h);
            var d = b.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var px = new int[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) px[y * w + x] = f(x, y).ToArgb();
            for (int y = 0; y < h; y++) Marshal.Copy(px, y * w, new IntPtr(d.Scan0.ToInt64() + (long)y * d.Stride), w);
            b.UnlockBits(d);
            return b;
        }

        static int Clamp(double v) { return v < 0 ? 0 : v > 255 ? 255 : (int)v; }
        static Color Grey(double v) { int c = Clamp(v); return Color.FromArgb(255, c, c, c); }

        // ruído suave (soma de senos com fase aleatória) para texturas de material
        static Func<int, int, double> Smooth(int seed, double scale)
        {
            var r = new Random(seed);
            var ph = Enumerable.Range(0, 8).Select(i => r.NextDouble() * 6.28).ToArray();
            var fr = Enumerable.Range(0, 8).Select(i => (0.6 + r.NextDouble()) * scale).ToArray();
            return (x, y) =>
            {
                double s = 0;
                for (int i = 0; i < 4; i++) s += Math.Sin(x * fr[i] + ph[i] + Math.Sin(y * fr[i + 4] + ph[i + 4]) * 1.3) * Math.Cos(y * fr[i] * 0.7 + ph[(i + 3) % 8]);
                return s / 4;
            };
        }

        void BaseTextures()
        {
            var rnd = new Random(7);
            var sm = Smooth(3, 0.05);
            rNoise = Add("noise", Pixels(256, 256, (x, y) => Grey(243 + sm(x, y) * 7 + rnd.Next(-5, 6))), false);
            rWhite = Add("white", Pixels(64, 64, (x, y) => Color.White), true);
            // parede pintada: tinta com textura fina, rodapé embaixo e sombra suave perto do teto
            var sp = Smooth(11, 0.08);
            rPlaster = Add("plaster", Pixels(128, 384, (x, y) =>
            {
                double v = y / 383.0;
                if (v > 0.955) return v < 0.959 ? Grey(185) : Grey(118 + rnd.Next(-4, 5) - (v - 0.96) * 300);
                if (v > 0.945) return Grey(205);
                return Grey(249 - v * 8 + sp(x, y) * 4 + rnd.Next(-4, 5) - (v < 0.03 ? (0.03 - v) * 500 : 0));
            }), false);
            // forro de placas acústicas
            rCeil = Add("ceil", Pixels(128, 128, (x, y) =>
            {
                if (x < 2 || y < 2) return Grey(196);
                if (x < 4 || y < 4) return Grey(250);
                return Grey(240 + rnd.Next(-3, 4) - (rnd.Next(14) == 0 ? 18 : 0));
            }), false);
            // luminária de embutir
            rLight = Add("light", Pixels(128, 128, (x, y) =>
            {
                if (x < 6 || y < 6 || x > 121 || y > 121) return Grey(205);
                bool grid = (x - 6) % 19 == 0 || (y - 6) % 19 == 0;
                return grid ? Color.FromArgb(255, 236, 238, 240) : Color.FromArgb(255, 255, 253, 246);
            }), true);
            // vidro: levemente azulado com faixas de reflexo
            rGlass = Add("glass", Pixels(128, 128, (x, y) =>
            {
                double d = (x + y * 0.6) % 128;
                bool streak = (d > 20 && d < 30) || (d > 38 && d < 42);
                return streak ? Color.FromArgb(92, 255, 255, 255) : Color.FromArgb(44, 190, 222, 250);
            }), false);
            rShadow = Add("shadow", Pixels(64, 64, (x, y) =>
            {
                double dx = (x - 31.5) / 31.5, dy = (y - 31.5) / 31.5, r = Math.Sqrt(dx * dx + dy * dy);
                return Color.FromArgb(Clamp(r >= 1 ? 0 : 170 * Math.Pow(1 - r, 1.4)), 0, 0, 0);
            }), false);
            rRing = Add("ring", Pixels(128, 128, (x, y) =>
            {
                double dx = (x - 63.5) / 63.5, dy = (y - 63.5) / 63.5, r = Math.Sqrt(dx * dx + dy * dy);
                double ring = Math.Max(0, 1 - Math.Abs(r - 0.82) / 0.1);
                double fill = r < 0.82 ? 0.22 * (1 - r) : 0;
                return Color.FromArgb(Clamp(255 * Math.Min(1, ring + fill)), 255, 255, 255);
            }), true);
            rCode = Add("code", ScreenBmp(Art.Hx(0x0B2A4A), true, 1), true);
            rLaptop = Add("laptop", ScreenBmp(Art.Hx(0x3A80D9), false, 2), true);
            // mesa de pingue-pongue
            {
                var b = Bmp(256, 144);
                using (var g = Gfx(b))
                {
                    g.Clear(Art.Hx(0x1F7A50));
                    using (var p = new Pen(Color.White, 5)) { g.DrawRectangle(p, 4, 4, 247, 135); }
                    using (var p = new Pen(Color.White, 2)) g.DrawLine(p, 4, 72, 252, 72);
                }
                rPing = Add("ping", b, false);
            }
            // tábuas do palco
            {
                var b = Bmp(128, 128);
                using (var g = Gfx(b))
                {
                    var r2 = new Random(5);
                    for (int i = 0; i < 4; i++)
                    {
                        var c = Art.Lt(Art.Hx(0x8B5E3C), (float)r2.NextDouble() * 0.12f);
                        using (var lg = new LinearGradientBrush(new Rectangle(i * 32, 0, 32, 128), Art.Lt(c, 0.06f), Art.Dk(c, 0.08f), 0f)) g.FillRectangle(lg, i * 32, 0, 32, 128);
                        for (int k = 0; k < 5; k++) using (var p = new Pen(Art.A(Art.Dk(c, 0.25f), 90), 1)) { float gx = i * 32 + 4 + r2.Next(24); g.DrawBezier(p, gx, 0, gx + 3, 40, gx - 3, 90, gx, 128); }
                        g.FillRectangle(Art.B(Art.Dk(c, 0.4f)), i * 32 + 31, 0, 1.5f, 128);
                    }
                }
                rPlank = Add("plank", b, false);
            }
            rAwning = Add("awning", Pixels(256, 64, (x, y) =>
            {
                bool red = (x / 32) % 2 == 0;
                double k = 1 - y / 64.0 * 0.15;
                return red ? Color.FromArgb(255, Clamp(239 * k), Clamp(75 * k), Clamp(87 * k)) : Grey(250 * k);
            }), false);
            {
                var b = Bmp(64, 32);
                using (var g = Gfx(b))
                {
                    g.Clear(Art.Hx(0x0D1117));
                    g.FillEllipse(Art.B(Art.Hx(0x39C5CF)), 14, 10, 10, 10);
                    g.FillEllipse(Art.B(Art.Hx(0x39C5CF)), 40, 10, 10, 10);
                }
                rRobot = Add("robot", b, true);
            }
        }

        static Bitmap ScreenBmp(Color scr, bool code, int seed)
        {
            var b = Bmp(256, 160);
            using (var g = Gfx(b))
            {
                using (var lg = new LinearGradientBrush(new Rectangle(0, 0, 256, 160), Art.Lt(scr, 0.15f), Art.Dk(scr, 0.25f), 90f)) g.FillRectangle(lg, 0, 0, 256, 160);
                var r = new Random(seed);
                var cols = code ? new[] { 0x7EE787, 0xE6EDF3, 0x58A6FF, 0xFFA657, 0xD2A8FF } : new[] { 0xFFFFFF };
                for (int i = 0; i < 11; i++)
                {
                    float ind = code ? (i % 4) * 14 : 0;
                    int segs = 1 + r.Next(3);
                    float x = 14 + ind;
                    for (int s = 0; s < segs; s++)
                    {
                        float w = 16 + r.Next(60);
                        if (x + w > 240) break;
                        g.FillRectangle(Art.B(Art.A(Art.Hx(cols[r.Next(cols.Length)]), code ? 230 : 120)), x, 12 + i * 13, w, 5);
                        x += w + 8;
                    }
                }
                using (var p = new GraphicsPath())
                {
                    p.AddPolygon(new[] { new PointF(150, 0), new PointF(195, 0), new PointF(120, 160), new PointF(75, 160) });
                    g.FillPath(Art.B(Art.A(Color.White, 22)), p);
                }
            }
            return b;
        }

        Region Screen(Color c) { return regions.ContainsKey("scr" + c.ToArgb()) ? regions["scr" + c.ToArgb()] : Add("scr" + c.ToArgb(), ScreenBmp(c, false, c.ToArgb()), true); }

        static readonly Dictionary<int, Font> fonts = new Dictionary<int, Font>();
        static void FitText(Graphics g, string text, RectangleF r, Color c)
        {
            if (string.IsNullOrEmpty(text)) return;
            float size = Math.Max(2, r.Height * 0.72f);
            using (var f0 = new Font("Segoe UI Black", size, GraphicsUnit.Pixel))
            {
                var sz = g.MeasureString(text, f0);
                if (sz.Width > r.Width) size = Math.Max(2, size * r.Width / sz.Width);
            }
            using (var f = new Font("Segoe UI Black", size, GraphicsUnit.Pixel))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap })
                g.DrawString(text, f, Art.B(c), new RectangleF(r.X - 2, r.Y, r.Width + 4, r.Height + 1), sf);
        }

        // frente de um móvel: a mesma arte HD da visão de cima, recortada na face da frente
        Region Front(Furn f, string tag, float bx, float by, float bw, float bd, float z0, float z1)
        {
            string key = "front|" + tag + "|" + f.Kind + "|" + f.W + "|" + f.H + "|" + f.C.ToArgb() + "|" + f.Var + "|" + (f.X - Math.Floor(f.X)) + "|" + (f.Y - Math.Floor(f.Y));
            if (f.Kind == "shelf" || f.Kind == "kiosk") key += "|" + f.X + "|" + f.Y;
            Region r;
            if (regions.TryGetValue(key, out r)) return r;
            var fr = Scene.Front(bx, by, bw, bd, z0, z1);
            const int sc = 5;
            var b = Bmp((int)Math.Ceiling(fr.Width * sc), (int)Math.Ceiling(fr.Height * sc));
            using (var g = Gfx(b))
            {
                g.ScaleTransform(sc, sc);
                g.TranslateTransform(-fr.X, -fr.Y);
                Scene.Furniture(g, f, 0, 1);
                if (f.Kind == "kiosk" && tag == "kc")
                {
                    var kp = Scene.P(f.X + f.W / 2, f.Y + 1f, 18);
                    FitText(g, "LOJINHA", new RectangleF(kp.X - 22, kp.Y + 4, 44, 10), Color.White);
                }
            }
            return Add(key, b, false);
        }

        uint Upload(byte[] rgba, int w, int h, bool mip, int maxLevel, bool repeat)
        {
            var t = new uint[1];
            GL.GenTextures(1, t);
            GL.BindTexture(GL.TEXTURE_2D, t[0]);
            GL.PixelStore(GL.UNPACK_ALIGNMENT, 1);
            int wrap = repeat ? GL.REPEAT : gl12 ? GL.CLAMP_TO_EDGE : GL.CLAMP;
            GL.TexParameter(GL.TEXTURE_2D, GL.TEXTURE_WRAP_S, wrap);
            GL.TexParameter(GL.TEXTURE_2D, GL.TEXTURE_WRAP_T, wrap);
            GL.TexParameter(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.LINEAR);
            if (mip)
            {
                GL.TexParameter(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.LINEAR_MIPMAP_LINEAR);
                if (maxLevel > 0 && gl12) GL.TexParameter(GL.TEXTURE_2D, GL.TEXTURE_MAX_LEVEL, maxLevel);
                if (aniso > 1) GL.TexParameterf(GL.TEXTURE_2D, GL.TEXTURE_MAX_ANISOTROPY, aniso);
                if (gl14)
                {
                    GL.TexParameter(GL.TEXTURE_2D, GL.GENERATE_MIPMAP, 1);
                    GL.TexImage2D(GL.TEXTURE_2D, 0, (int)GL.RGBA8, w, h, 0, GL.RGBA, GL.UNSIGNED_BYTE, rgba);
                }
                else GL.Build2DMipmaps(GL.TEXTURE_2D, 4, w, h, GL.RGBA, GL.UNSIGNED_BYTE, rgba);
            }
            else
            {
                GL.TexParameter(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.LINEAR);
                GL.TexImage2D(GL.TEXTURE_2D, 0, (int)GL.RGBA8, w, h, 0, GL.RGBA, GL.UNSIGNED_BYTE, rgba);
            }
            return t[0];
        }

        // pixels de um Bitmap em RGBA (opcionalmente dentro de uma área maior, potência de 2)
        static byte[] Rgba(Bitmap b, PixelFormat fmt, int tw, int th)
        {
            int w = b.Width, h = b.Height;
            var d = b.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, fmt);
            var bytes = new byte[tw * th * 4];
            for (int y = 0; y < Math.Min(h, th); y++)
                Marshal.Copy(new IntPtr(d.Scan0.ToInt64() + (long)y * d.Stride), bytes, y * tw * 4, Math.Min(w, tw) * 4);
            b.UnlockBits(d);
            for (int i = 0; i < bytes.Length; i += 4) { byte t = bytes[i]; bytes[i] = bytes[i + 2]; bytes[i + 2] = t; }
            return bytes;
        }

        static int Pot(int v) { int p = 1; while (p < v) p <<= 1; return p; }

        // ───────────────────────── montagem da geometria ─────────────────────────

        static readonly V3 LightDir = new V3(0.3f, 0.42f, 0.86f);

        static Color Mul(Color c, float k) { return Color.FromArgb(c.A, Clamp(c.R * k), Clamp(c.G * k), Clamp(c.B * k)); }

        // oclusão de ambiente aproximada: perto do chão (e do teto) fica mais escuro
        static float AO(float z)
        {
            if (z <= 0.001f) return 0.72f;
            if (z < 0.6f) return 0.72f + 0.28f * z / 0.6f;
            if (z > WH - 0.35f) return 1f - (z - (WH - 0.35f)) * 0.3f;
            return 1f;
        }

        static void QuadC(Mesh m, Region r, V3 a, V3 b, V3 c, V3 d, Color ca, Color cb, Color cc, Color cd, float u0, float v0, float u1, float v1)
        {
            float du = r.U1 - r.U0, dv = r.V1 - r.V0;
            float U0 = r.U0 + du * u0, U1 = r.U0 + du * u1, V0 = r.V0 + dv * v0, V1 = r.V0 + dv * v1;
            m.Shape(r.Page, 6);
            m.Vert(r.Page, a, U0, V1, ca); m.Vert(r.Page, b, U1, V1, cb); m.Vert(r.Page, c, U1, V0, cc);
            m.Vert(r.Page, a, U0, V1, ca); m.Vert(r.Page, c, U1, V0, cc); m.Vert(r.Page, d, U0, V0, cd);
        }

        static void Tri(Mesh m, Region r, V3 a, V3 b, V3 c, Color ca, Color cb, Color cc)
        {
            float u = (r.U0 + r.U1) / 2, v = (r.V0 + r.V1) / 2;
            m.Shape(r.Page, 3);
            m.Vert(r.Page, a, u, v, ca); m.Vert(r.Page, b, u, v, cb); m.Vert(r.Page, c, u, v, cc);
        }

        // face plana: cor × luz da direção × oclusão por altura (ou acesa, se a textura emite luz)
        static void Face(Mesh m, Region r, V3 a, V3 b, V3 c, V3 d, Color col, float k, float u0, float v0, float u1, float v1)
        {
            if (r.Emissive) { var w = Color.White; QuadC(m, r, a, b, c, d, w, w, w, w, u0, v0, u1, v1); return; }
            QuadC(m, r, a, b, c, d, Mul(col, k * AO(a.Z)), Mul(col, k * AO(b.Z)), Mul(col, k * AO(c.Z)), Mul(col, k * AO(d.Z)), u0, v0, u1, v1);
        }
        static void Face(Mesh m, Region r, V3 a, V3 b, V3 c, V3 d, Color col, float k) { Face(m, r, a, b, c, d, col, k, 0, 0, 1, 1); }

        const float KTop = 1f, KS = 0.86f, KE = 0.8f, KW = 0.74f, KN = 0.7f, KBottom = 0.55f;
        static float Sp(float s) { return Math.Min(1f, Math.Max(0.06f, s / 1.6f)); }

        // caixa: tampo, quatro lados (a frente sul pode ter textura) e, se pedido, o fundo
        void Box(Mesh m, float x0, float y0, float z0, float x1, float y1, float z1, Color c, Region front = null, Region top = null, bool noTop = false, bool bottom = false)
        {
            if (x1 <= x0 || y1 <= y0 || z1 <= z0) return;
            float w = x1 - x0, d = y1 - y0, h = z1 - z0;
            if (!noTop)
            {
                if (top != null) Face(m, top, new V3(x0, y1, z1), new V3(x1, y1, z1), new V3(x1, y0, z1), new V3(x0, y0, z1), Color.White, KTop);
                else Face(m, rNoise, new V3(x0, y1, z1), new V3(x1, y1, z1), new V3(x1, y0, z1), new V3(x0, y0, z1), c, KTop, 0, 0, Sp(w), Sp(d));
            }
            if (front != null) Face(m, front, new V3(x0, y1, z0), new V3(x1, y1, z0), new V3(x1, y1, z1), new V3(x0, y1, z1), Color.White, 0.95f);
            else Face(m, rNoise, new V3(x0, y1, z0), new V3(x1, y1, z0), new V3(x1, y1, z1), new V3(x0, y1, z1), c, KS, 0, 0, Sp(w), Sp(h));
            Face(m, rNoise, new V3(x1, y0, z0), new V3(x0, y0, z0), new V3(x0, y0, z1), new V3(x1, y0, z1), c, KN, 0, 0, Sp(w), Sp(h));
            Face(m, rNoise, new V3(x1, y1, z0), new V3(x1, y0, z0), new V3(x1, y0, z1), new V3(x1, y1, z1), c, KE, 0, 0, Sp(d), Sp(h));
            Face(m, rNoise, new V3(x0, y0, z0), new V3(x0, y1, z0), new V3(x0, y1, z1), new V3(x0, y0, z1), c, KW, 0, 0, Sp(d), Sp(h));
            if (bottom) Face(m, rNoise, new V3(x0, y0, z0), new V3(x1, y0, z0), new V3(x1, y1, z0), new V3(x0, y1, z0), c, KBottom, 0, 0, Sp(w), Sp(d));
        }

        // cilindro (ou tronco de cone) em pé
        void Cyl(Mesh m, float cx, float cy, float r0, float r1, float z0, float z1, Color c, int seg = 12, bool cap = true, bool emissive = false)
        {
            var reg = emissive ? rWhite : rNoise;
            for (int i = 0; i < seg; i++)
            {
                double a0 = i * Math.PI * 2 / seg, a1 = (i + 1) * Math.PI * 2 / seg, am = (a0 + a1) / 2;
                float k = emissive ? 1 : 0.775f + 0.03f * (float)Math.Cos(am) + 0.09f * (float)Math.Sin(am);
                Color col = emissive ? c : c;
                var A = new V3(cx + r0 * (float)Math.Cos(a0), cy + r0 * (float)Math.Sin(a0), z0);
                var B = new V3(cx + r0 * (float)Math.Cos(a1), cy + r0 * (float)Math.Sin(a1), z0);
                var C = new V3(cx + r1 * (float)Math.Cos(a1), cy + r1 * (float)Math.Sin(a1), z1);
                var D = new V3(cx + r1 * (float)Math.Cos(a0), cy + r1 * (float)Math.Sin(a0), z1);
                if (emissive) QuadC(m, reg, A, B, C, D, c, c, c, c, 0.3f, 0.3f, 0.7f, 0.7f);
                else Face(m, reg, A, B, C, D, col, k, 0, 0, 0.3f, 1);
                if (cap && r1 > 0.001f)
                {
                    var top = new V3(cx, cy, z1);
                    Color tc = emissive ? c : Mul(c, KTop * AO(z1));
                    Tri(m, reg, top, D, C, tc, tc, tc);
                }
            }
        }

        // forma arredondada (folhagem, pufe): elipsoide de poucos polígonos com leve irregularidade
        void Ell(Mesh m, float cx, float cy, float cz, float rx, float ry, float rz, Color c, int seed, float jit = 0.1f, int lat = 5, int lon = 9)
        {
            var rnd = new Random(seed);
            var pts = new V3[lat + 1, lon + 1];
            var cols = new Color[lat + 1, lon + 1];
            var jj = new float[lat + 1, lon];
            for (int i = 0; i <= lat; i++) for (int j = 0; j < lon; j++) jj[i, j] = (i == 0 || i == lat) ? 1 : 1 + ((float)rnd.NextDouble() - 0.5f) * 2 * jit;
            float ll = (float)Math.Sqrt(LightDir.X * LightDir.X + LightDir.Y * LightDir.Y + LightDir.Z * LightDir.Z);
            for (int i = 0; i <= lat; i++)
            {
                double th = Math.PI * i / lat;
                for (int j = 0; j <= lon; j++)
                {
                    double ph = Math.PI * 2 * j / lon;
                    float k = jj[i, j % lon];
                    float nx = (float)(Math.Sin(th) * Math.Cos(ph)), ny = (float)(Math.Sin(th) * Math.Sin(ph)), nz = (float)Math.Cos(th);
                    pts[i, j] = new V3(cx + nx * rx * k, cy + ny * ry * k, cz + nz * rz * k);
                    float dot = (nx * LightDir.X + ny * LightDir.Y + nz * LightDir.Z) / ll;
                    float sh = 0.5f + 0.5f * Math.Max(0, dot) + 0.08f * nz;
                    cols[i, j] = Mul(c, sh * AO(Math.Max(0, pts[i, j].Z)));
                }
            }
            for (int i = 0; i < lat; i++)
                for (int j = 0; j < lon; j++)
                    QuadC(m, rNoise, pts[i + 1, j], pts[i + 1, j + 1], pts[i, j + 1], pts[i, j], cols[i + 1, j], cols[i + 1, j + 1], cols[i, j + 1], cols[i, j], 0.2f, 0.2f, 0.4f, 0.4f);
        }

        // painel vertical virado para o sul (tela, quadro...) no plano y
        static void PanelS(Mesh m, Region r, float x0, float x1, float y, float z0, float z1, Color c, float k)
        {
            Face(m, r, new V3(x0, y, z0), new V3(x1, y, z0), new V3(x1, y, z1), new V3(x0, y, z1), c, k);
        }
        // painel vertical virado para o norte
        static void PanelN(Mesh m, Region r, float x0, float x1, float y, float z0, float z1, Color c, float k)
        {
            Face(m, r, new V3(x1, y, z0), new V3(x0, y, z0), new V3(x0, y, z1), new V3(x1, y, z1), c, k);
        }

        Cell C(float x, float y)
        {
            int cx = Math.Max(0, Math.Min(CW - 1, (int)Math.Floor(x / CS))), cy = Math.Max(0, Math.Min(CH - 1, (int)Math.Floor(y / CS)));
            return cells[cx, cy];
        }

        bool InMap(int x, int y) { return W.InMap(x, y); }
        bool IsFloorT(int x, int y) { return InMap(x, y) && (W.Tile[x, y] == World.Floor || W.Tile[x, y] == World.Door); }
        bool IsWallT(int x, int y) { return InMap(x, y) && W.Tile[x, y] == World.Wall; }
        Scene.WallPiece Piece(int x, int y) { Scene.WallPiece p; return wallAt.TryGetValue(x * 1000 + y, out p) ? p : null; }
        bool GlassT(int x, int y) { var p = Piece(x, y); return p != null && p.Glass && IsWallT(x, y); }

        bool Outdoor(int x, int y)
        {
            if (IsFloorT(x, y))
            {
                Color a, b; int style;
                Scene.FloorStyle(W, x, y, out a, out b, out style);
                return style == 4;
            }
            // vidro: só fica sem teto se todo o chão em volta for ao ar livre
            bool any = false;
            foreach (var d in Dirs)
            {
                int nx = x + d.X, ny = y + d.Y;
                if (!IsFloorT(nx, ny)) continue;
                any = true;
                if (!Outdoor(nx, ny)) return false;
            }
            return any;
        }

        static readonly Point[] Dirs = { new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0) };

        static readonly Color Alu = Art.Hx(0xB9C3CF), AluDk = Art.Hx(0x7C8899), Dark = Art.Hx(0x2A2F3A);

        public void Build()
        {
            var ver = GL.GetString(GL.VERSION);
            Version = ver;
            Renderer = GL.GetString(GL.RENDERER);
            Software = Renderer.IndexOf("GDI Generic", StringComparison.OrdinalIgnoreCase) >= 0;
            int maj = 1, min = 1;
            try
            {
                var parts = ver.Split(' ')[0].Split('.');
                maj = int.Parse(parts[0]); min = int.Parse(parts[1]);
            }
            catch { }
            gl12 = maj > 1 || min >= 2;
            gl14 = maj > 1 || min >= 4;
            var ext = GL.GetString(GL.EXTENSIONS);
            if (ext.Contains("GL_EXT_texture_filter_anisotropic"))
            {
                var mx = new float[1];
                GL.GetFloat(GL.MAX_TEXTURE_MAX_ANISOTROPY, mx);
                aniso = Math.Min(8f, Math.Max(1f, mx[0]));
            }

            BaseTextures();
            RoomTextures();
            CW = (World.W + CS - 1) / CS; CH = (World.H + CS - 1) / CS;
            cells = new Cell[CW, CH];
            for (int cx = 0; cx < CW; cx++)
                for (int cy = 0; cy < CH; cy++)
                    cells[cx, cy] = new Cell { CX = cx, CY = cy, X0 = cx * CS - 2, Y0 = cy * CS - 2, X1 = cx * CS + CS + 2, Y1 = cy * CS + CS + 2 };

            for (int x = 0; x < World.W; x++)
                for (int y = 0; y < World.H; y++)
                {
                    if (IsWallT(x, y)) { if (GlassT(x, y)) IsoGlassWall(x, y); else IsoSolidWall(x, y); }
                }
            foreach (var d in W.AllDoors) IsoDoor(d);
            foreach (var d in W.Decor) IsoDecorPiece(d);
            foreach (var f in W.Furniture) Model(f);

            // folhas de textura para a placa de vídeo
            foreach (var p in pages)
            {
                // a imagem fica guardada: a sala do jogador e os ícones da lojinha ainda podem acrescentar coisas
                p.Tex = Upload(Rgba(p.Bmp, PixelFormat.Format32bppArgb, PageSize, PageSize), PageSize, PageSize, true, 5, false);
                p.Dirty = false;
            }
            // listas prontas (desenho rápido)
            foreach (var c in cells)
            {
                var fm = new Mesh();
                for (int x = c.CX * CS; x < Math.Min(World.W, c.CX * CS + CS); x++)
                    for (int y = c.CY * CS; y < Math.Min(World.H, c.CY * CS + CS); y++)
                    {
                        if (W.Tile[x, y] == World.Void) continue;
                        Color fa, fb; int style;
                        Scene.FloorStyle(W, x, y, out fa, out fb, out style);
                        QuadC(fm, rNoise, new V3(x, y + 1, 0), new V3(x + 1, y + 1, 0), new V3(x + 1, y, 0), new V3(x, y, 0), fa, fa, fa, fa, 0, 0, 0.3f, 0.3f);
                    }
                if (!fm.Empty) { c.LF = GL.GenLists(1); GL.NewList(c.LF, GL.COMPILE); DrawMesh(fm); GL.EndList(); }
                if (!c.O.Empty) { c.LO = GL.GenLists(1); GL.NewList(c.LO, GL.COMPILE); DrawMesh(c.O); GL.EndList(); }
                if (!c.G.Empty) { c.LG = GL.GenLists(1); GL.NewList(c.LG, GL.COMPILE); DrawMesh(c.G); GL.EndList(); }
                c.O = null; c.G = null;
            }
        }

        void DrawMesh(Mesh m)
        {
            foreach (var kv in m.Data)
            {
                GL.BindTexture(GL.TEXTURE_2D, pages[kv.Key].Tex);
                var l = kv.Value;
                GL.Begin(GL.TRIANGLES);
                for (int i = 0; i < l.Count; i += 9)
                {
                    GL.Color4ub((byte)l[i + 5], (byte)l[i + 6], (byte)l[i + 7], (byte)l[i + 8]);
                    GL.TexCoord(l[i + 3], l[i + 4]);
                    GL.Vertex3(l[i], l[i + 2], l[i + 1]);
                }
                GL.End();
            }
        }

        Color SideColor(int nx, int ny)
        {
            if (IsFloorT(nx, ny)) return Scene.FaceColor(W.Zone[nx, ny]);
            var p = Piece(nx, ny);
            return p != null ? p.Face : Scene.FaceColor(-1);
        }

        void Legs4(Mesh m, float x0, float y0, float x1, float y1, float z, Color c)
        {
            float s = 0.05f;
            Box(m, x0, y0, 0, x0 + s, y0 + s, z, c); Box(m, x1 - s, y0, 0, x1, y0 + s, z, c);
            Box(m, x0, y1 - s, 0, x0 + s, y1, z, c); Box(m, x1 - s, y1 - s, 0, x1, y1, z, c);
        }

        void Monitor3(Mesh m, float cx, float y, float zb, float w, Region scr)
        {
            float h = w * 0.58f;
            Box(m, cx - 0.1f, y + 0.04f, zb, cx + 0.1f, y + 0.16f, zb + 0.015f, Dark);
            Box(m, cx - 0.025f, y + 0.07f, zb, cx + 0.025f, y + 0.1f, zb + 0.16f, Dark);
            Box(m, cx - w / 2, y, zb + 0.12f, cx + w / 2, y + 0.05f, zb + 0.12f + h, Dark, null, null, false, true);
            PanelS(m, scr, cx - w / 2 + 0.025f, cx + w / 2 - 0.025f, y + 0.052f, zb + 0.145f, zb + 0.12f + h - 0.025f, Color.White, 1);
        }

        void Laptop3(Mesh m, float cx, float y, float z)
        {
            Box(m, cx - 0.2f, y, z, cx + 0.2f, y + 0.26f, z + 0.015f, Art.Hx(0xC4CBD5));
            var w = Color.White;
            float z0 = z + 0.015f;
            QuadC(m, rLaptop, new V3(cx - 0.19f, y + 0.005f, z0), new V3(cx + 0.19f, y + 0.005f, z0), new V3(cx + 0.19f, y - 0.06f, z0 + 0.25f), new V3(cx - 0.19f, y - 0.06f, z0 + 0.25f), w, w, w, w, 0, 0, 1, 1);
            var dk = Art.Hx(0x3A3F4B);
            QuadC(m, rNoise, new V3(cx - 0.2f, y - 0.012f, z0), new V3(cx + 0.2f, y - 0.012f, z0), new V3(cx + 0.2f, y - 0.077f, z0 + 0.26f), new V3(cx - 0.2f, y - 0.077f, z0 + 0.26f), dk, dk, dk, dk, 0, 0, 0.2f, 0.2f);
        }

        void Model(Furn f)
        {
            float x = f.X, y = f.Y, w = f.W, h = f.H;
            var m = target ?? C(x + w / 2, y + h / 2).O;
            if (f.Kind == "sofa" && f.Var == 7) { SofaNorth(m, f); return; }
            switch (f.Kind)
            {
                case "desk":
                    {
                        Legs4(m, x + 0.08f, y + 0.1f, x + w - 0.08f, y + h - 0.1f, 0.74f, Art.Dk(f.C, 0.35f));
                        Box(m, x + 0.13f, y + 0.12f, 0.3f, x + w - 0.13f, y + 0.16f, 0.74f, Art.Dk(f.C, 0.15f));
                        Box(m, x + 0.04f, y + 0.06f, 0.74f, x + w - 0.04f, y + h - 0.06f, 0.78f, f.C);
                        if (f.Var == 2) { Monitor3(m, x + w * 0.3f, y + 0.14f, 0.78f, 0.6f, Screen(Art.Hx(0x1F6FEB))); Monitor3(m, x + w * 0.72f, y + 0.14f, 0.78f, 0.6f, Screen(Art.Hx(0x2EA043))); }
                        else if (f.Var == 1) Monitor3(m, x + w / 2, y + 0.14f, 0.78f, 0.6f, Screen(Art.Hx(0x3B82C4)));
                        Box(m, x + w / 2 - 0.24f, y + 0.5f, 0.78f, x + w / 2 + 0.24f, y + 0.64f, 0.795f, Art.Hx(0xDDE2E9));
                        if (f.Var != 0) Cyl(m, x + w - 0.3f, y + 0.56f, 0.045f, 0.045f, 0.78f, 0.89f, Color.White, 10);
                        break;
                    }
                case "terminal":
                    {
                        Box(m, x + 0.04f, y + 0.08f, 0, x + w - 0.04f, y + h - 0.08f, 0.8f, Art.Hx(0x313847), Front(f, "cab", x + 0.04f, y + 0.08f, w - 0.08f, h - 0.16f, 0, 18));
                        Monitor3(m, x + w / 2, y + 0.1f, 0.8f, 1.3f, rCode);
                        Box(m, x + w / 2 - 0.3f, y + 0.5f, 0.8f, x + w / 2 + 0.3f, y + 0.66f, 0.82f, Art.Hx(0xDDE2E9));
                        break;
                    }
                case "counter":
                    Box(m, x, y, 0, x + w, y + h, 0.95f, Art.Hx(0xF5F7FA), Front(f, "c", x, y, w, h, 0, 20));
                    Monitor3(m, x + w * 0.35f, y + 0.2f, 0.95f, 0.6f, Screen(Art.Hx(0x3B82C4)));
                    break;
                case "plant":
                    {
                        float cx = x + 0.5f, cy = y + 0.62f;
                        Cyl(m, cx, cy, 0.14f, 0.19f, 0, 0.42f, Art.Hx(0xC97A48), 12, false);
                        Cyl(m, cx, cy, 0.175f, 0.175f, 0.38f, 0.4f, Art.Hx(0x4A3426), 12);
                        var r = new Random((int)(x * 7 + y * 3));
                        for (int i = 0; i < 6; i++)
                        {
                            double a = i * Math.PI * 2 / 6 + r.NextDouble() * 0.5;
                            var gc = i % 2 == 0 ? Art.Hx(0x2F8A55) : Art.Hx(0x267447);
                            Ell(m, cx + (float)Math.Cos(a) * 0.13f, cy + (float)Math.Sin(a) * 0.13f, 0.68f + (float)r.NextDouble() * 0.22f, 0.19f, 0.19f, 0.21f, gc, r.Next(), 0.15f);
                        }
                        Ell(m, cx, cy, 0.98f, 0.17f, 0.17f, 0.2f, Art.Hx(0x3A9B62), r.Next(), 0.15f);
                        break;
                    }
                case "sofa":
                    {
                        Color c = f.C;
                        Box(m, x, y + 0.05f, 0, x + w, y + 0.4f, 0.82f, Art.Dk(c, 0.08f));
                        Box(m, x + 0.05f, y + 0.35f, 0, x + w - 0.05f, y + h - 0.05f, 0.4f, Art.Dk(c, 0.05f));
                        int n = Math.Max(1, (int)Math.Round((w - 0.36f) / 0.9f));
                        float cw = (w - 0.36f) / n;
                        for (int i = 0; i < n; i++)
                        {
                            Box(m, x + 0.18f + i * cw + 0.02f, y + 0.42f, 0.4f, x + 0.18f + (i + 1) * cw - 0.02f, y + h - 0.08f, 0.5f, Art.Lt(c, 0.08f));
                            Box(m, x + 0.18f + i * cw + 0.03f, y + 0.4f, 0.48f, x + 0.18f + (i + 1) * cw - 0.03f, y + 0.52f, 0.78f, Art.Lt(c, 0.04f));
                        }
                        Box(m, x, y + 0.2f, 0, x + 0.18f, y + h - 0.05f, 0.6f, c);
                        Box(m, x + w - 0.18f, y + 0.2f, 0, x + w, y + h - 0.05f, 0.6f, c);
                        break;
                    }
                case "bench":
                    {
                        var lc = Art.Hx(0x4A4E59);
                        if (h > w)
                        {
                            Box(m, x + 0.3f, y + 0.15f, 0, x + 0.7f, y + 0.22f, 0.4f, lc);
                            Box(m, x + 0.3f, y + h - 0.22f, 0, x + 0.7f, y + h - 0.15f, 0.4f, lc);
                            Box(m, x + 0.25f, y + 0.05f, 0.4f, x + 0.75f, y + h - 0.05f, 0.46f, f.C);
                        }
                        else
                        {
                            Box(m, x + 0.15f, y + 0.35f, 0, x + 0.22f, y + 0.7f, 0.4f, lc);
                            Box(m, x + w - 0.22f, y + 0.35f, 0, x + w - 0.15f, y + 0.7f, 0.4f, lc);
                            Box(m, x + 0.05f, y + 0.3f, 0.4f, x + w - 0.05f, y + 0.75f, 0.46f, f.C);
                        }
                        break;
                    }
                case "crate":
                    Box(m, x + 0.04f, y + 0.1f, 0, x + w - 0.04f, y + h - 0.1f, 0.95f, f.C, Front(f, "cr", x + 0.04f, y + 0.1f, w - 0.08f, h - 0.2f, 0, 24));
                    break;
                case "rack":
                    Box(m, x + 0.08f, y + 0.15f, 0, x + 0.92f, y + 0.85f, 2.2f, Art.Hx(0x2B3446), Front(f, "rk", x + 0.08f, y + 0.15f, 0.84f, 0.7f, 0, 58));
                    racks.Add(f);
                    break;
                case "table":
                case "bigdesk":
                    {
                        Legs4(m, x + 0.1f, y + 0.1f, x + w - 0.1f, y + h - 0.1f, 0.74f, Art.Dk(f.C, 0.35f));
                        if (f.Kind == "bigdesk") Box(m, x + 0.1f, y + h - 0.3f, 0.05f, x + w - 0.1f, y + h - 0.18f, 0.74f, Art.Dk(f.C, 0.2f));
                        Box(m, x + 0.04f, y + 0.04f, 0.74f, x + w - 0.04f, y + h - 0.04f, 0.79f, f.C);
                        if (f.Kind == "bigdesk")
                        {
                            Laptop3(m, x + w * 0.5f, y + 0.35f, 0.79f);
                            float lx = x + w * 0.16f, ly = y + 0.35f;
                            Cyl(m, lx, ly, 0.07f, 0.07f, 0.79f, 0.81f, Dark, 10);
                            Cyl(m, lx, ly, 0.012f, 0.012f, 0.81f, 1.15f, Dark, 6, false);
                            Cyl(m, lx, ly, 0.11f, 0.05f, 1.1f, 1.24f, Art.Hx(0xFFE08A), 12, true, true);
                            Box(m, x + w * 0.72f, y + 0.4f, 0.79f, x + w * 0.72f + 0.22f, y + 0.7f, 0.795f, Color.White);
                        }
                        else if (w > 3) for (float k = x + 0.8f; k < x + w - 0.5f; k += 1.3f) Laptop3(m, k, y + 0.4f, 0.79f);
                        break;
                    }
                case "labtable":
                    {
                        Legs4(m, x + 0.1f, y + 0.12f, x + w - 0.1f, y + h - 0.12f, 0.8f, Art.Hx(0x9AA7B5));
                        Box(m, x + 0.04f, y + 0.08f, 0.8f, x + w - 0.04f, y + h - 0.08f, 0.85f, Art.Hx(0xF1F4F8));
                        var cols = new[] { 0x39C5CF, 0xD65AA0, 0x3FB950, 0xF0B429 };
                        for (int i = 0; i < (int)(w * 2) - 1; i++)
                        {
                            float bx = x + 0.37f + i * 0.45f;
                            Cyl(m, bx, y + 0.38f, 0.06f, 0.06f, 0.85f, 1.12f, Art.Hx(cols[i % 4]), 10);
                            Cyl(m, bx, y + 0.38f, 0.025f, 0.025f, 1.12f, 1.2f, Art.Hx(0xE5EAF0), 8);
                        }
                        break;
                    }
                case "beanbag":
                    Ell(m, x + 0.5f, y + 0.6f, 0.2f, 0.42f, 0.38f, 0.27f, f.C, (int)(x * 13 + y), 0.06f, 6, 12);
                    break;
                case "pingpong":
                    Legs4(m, x + 0.12f, y + 0.12f, x + w - 0.12f, y + h - 0.12f, 0.74f, Art.Hx(0x2B2D42));
                    Box(m, x + 0.04f, y + 0.04f, 0.74f, x + w - 0.04f, y + h - 0.04f, 0.78f, Art.Hx(0x175C3A), null, rPing);
                    Box(m, x + w / 2 - 0.012f, y + 0.0f, 0.78f, x + w / 2 + 0.012f, y + h, 0.93f, Art.Hx(0xF1F5F9));
                    Ell(m, x + w * 0.3f, y + 0.45f, 0.81f, 0.025f, 0.025f, 0.025f, Art.Hx(0xF4A261), 3, 0, 4, 6);
                    break;
                case "shelf":
                    Box(m, x + 0.04f, y + 0.15f, 0, x + w - 0.04f, y + h - 0.05f, 2.1f, f.C, Front(f, "sh", x + 0.04f, y + 0.15f, w - 0.08f, h - 0.2f, 0, 48));
                    break;
                case "coffee":
                    Box(m, x + 0.12f, y + 0.2f, 0, x + 0.88f, y + 0.8f, 1.75f, Art.Hx(0x737C8A), Front(f, "cf", x + 0.12f, y + 0.2f, 0.76f, 0.6f, 0, 44));
                    break;
                case "cooler":
                    Box(m, x + 0.28f, y + 0.4f, 0, x + 0.72f, y + 0.8f, 0.95f, Art.Hx(0xF4F6FA), Front(f, "co", x + 0.28f, y + 0.4f, 0.44f, 0.4f, 0, 20));
                    Cyl(m, x + 0.5f, y + 0.6f, 0.15f, 0.15f, 0.95f, 1.38f, Art.Hx(0x8CCBF2), 14, false);
                    Cyl(m, x + 0.5f, y + 0.6f, 0.15f, 0.06f, 1.38f, 1.47f, Art.Hx(0x9CD5F5), 14);
                    break;
                case "ac":
                    Box(m, x + 0.1f, y + 0.1f, 0, x + w - 0.1f, y + h - 0.1f, 2.0f, Art.Hx(0xF1F4F8), Front(f, "ac", x + 0.1f, y + 0.1f, w - 0.2f, h - 0.2f, 0, 44));
                    break;
                case "kiosk":
                    {
                        Box(m, x, y, 0, x + w, y + 0.3f, 1.9f, Art.Hx(0x7A5638), Front(f, "kb", x, y, w, 0.3f, 0, 40));
                        Box(m, x, y + 0.45f, 0, x + w, y + 1.0f, 0.9f, Art.Hx(0xDDBE92), Front(f, "kc", x, y + 0.45f, w, 0.55f, 0, 18));
                        var wc = Color.White;
                        QuadC(m, rAwning, new V3(x - 0.05f, y + 0.6f, 2.05f), new V3(x + w + 0.05f, y + 0.6f, 2.05f), new V3(x + w + 0.05f, y - 0.05f, 2.35f), new V3(x - 0.05f, y - 0.05f, 2.35f), wc, wc, wc, wc, 0, 0, w / 2, 1);
                        Box(m, x - 0.05f, y + 0.58f, 1.9f, x + w + 0.05f, y + 0.62f, 2.05f, Art.Hx(0xB8202B), null, null, false, true);
                        Box(m, x + 0.02f, y + 0.52f, 0.9f, x + 0.07f, y + 0.57f, 2.05f, Art.Hx(0x6B4A2F));
                        Box(m, x + w - 0.07f, y + 0.52f, 0.9f, x + w - 0.02f, y + 0.57f, 2.05f, Art.Hx(0x6B4A2F));
                        break;
                    }
                case "buffet":
                    {
                        Box(m, x, y + 0.1f, 0, x + w, y + h - 0.05f, 0.85f, Art.Hx(0xD7DDE5), Front(f, "bf", x, y + 0.1f, w, h - 0.15f, 0, 18));
                        Box(m, x - 0.03f, y + 0.07f, 0.85f, x + w + 0.03f, y + h - 0.03f, 0.9f, Art.Hx(0xF1F4F8));
                        var foods = new[] { 0xE9C46A, 0xE76F51, 0x8AB17D, 0xF4A261, 0xD62828, 0xFFFFFF };
                        for (int i = 0; i < (int)(w * 1.5f); i++)
                        {
                            float bx = x + 0.12f + i * 0.66f;
                            if (bx + 0.53f > x + w) break;
                            Box(m, bx, y + 0.25f, 0.9f, bx + 0.53f, y + 0.68f, 0.95f, Art.Hx(0xAEB8C6));
                            Ell(m, bx + 0.265f, y + 0.465f, 0.95f, 0.2f, 0.15f, 0.06f, Art.Hx(foods[(i + f.Var) % foods.Length]), i + f.Var, 0.2f, 4, 8);
                        }
                        break;
                    }
                case "lunchtable":
                    {
                        Legs4(m, x + 0.12f, y + 0.12f, x + w - 0.12f, y + h - 0.12f, 0.74f, Art.Hx(0x5A6378));
                        Box(m, x + 0.04f, y + 0.04f, 0.74f, x + w - 0.04f, y + h - 0.04f, 0.78f, f.C);
                        int i = 0;
                        for (float k = x + 0.5f; k < x + w - 0.4f; k += 1f, i++)
                        {
                            Cyl(m, k, y + 0.45f, 0.17f, 0.17f, 0.78f, 0.795f, Art.Hx(0xEAF0F6), 14);
                            Ell(m, k, y + 0.45f, 0.81f, 0.1f, 0.08f, 0.04f, Art.Hx(i % 2 == 0 ? 0xE9C46A : 0x8AB17D), i, 0.2f, 4, 8);
                        }
                        Box(m, x + 0.1f, y + h + 0.05f, 0.4f, x + w - 0.1f, y + h + 0.3f, 0.45f, Art.Hx(0x8D6E63));
                        Box(m, x + 0.2f, y + h + 0.12f, 0, x + 0.27f, y + h + 0.23f, 0.4f, Art.Hx(0x5A6378));
                        Box(m, x + w - 0.27f, y + h + 0.12f, 0, x + w - 0.2f, y + h + 0.23f, 0.4f, Art.Hx(0x5A6378));
                        break;
                    }
                case "stage":
                    {
                        Box(m, x, y, 0, x + w, y + h, 0.5f, Art.Hx(0x6E4A2D), null, null, true);
                        for (float tx = x; tx < x + w - 0.01f; tx += 1)
                            for (float ty = y; ty < y + h - 0.01f; ty += 1)
                            {
                                float ex = Math.Min(x + w, tx + 1), ey = Math.Min(y + h, ty + 1);
                                Face(m, rPlank, new V3(tx, ey, 0.5f), new V3(ex, ey, 0.5f), new V3(ex, ty, 0.5f), new V3(tx, ty, 0.5f), Color.White, 0.95f, 0, 0, ex - tx, ey - ty);
                            }
                        Box(m, x + w / 2 - 0.3f, y + 0.5f, 0.5f, x + w / 2 + 0.3f, y + 0.9f, 1.55f, Art.Hx(0x2B2D42));
                        Cyl(m, x + w / 2, y + 0.75f, 0.01f, 0.01f, 1.55f, 1.75f, Dark, 6, false);
                        Ell(m, x + w / 2, y + 0.78f, 1.77f, 0.03f, 0.03f, 0.04f, Art.Hx(0x58A6FF), 1, 0, 4, 6);
                        break;
                    }
                case "chairs":
                    for (float k = x + 0.1f; k < x + w - 0.3f; k += 0.8f)
                    {
                        Box(m, k, y + 0.15f, 0, k + 0.6f, y + 0.3f, 1.0f, Art.Dk(f.C, 0.05f));
                        Box(m, k, y + 0.3f, 0, k + 0.6f, y + 0.8f, 0.45f, Art.Lt(f.C, 0.1f));
                    }
                    break;
                case "arcade":
                    Box(m, x + 0.12f, y + 0.25f, 0, x + 0.88f, y + 0.8f, 2.0f, f.C, Front(f, "ar", x + 0.12f, y + 0.25f, 0.76f, 0.55f, 0, 54));
                    break;
                case "tree":
                    {
                        float cx = x + 0.5f, cy = y + 0.7f;
                        Cyl(m, cx, cy, 0.11f, 0.08f, 0, 1.8f, Art.Hx(0x6E4527), 10, false);
                        var r = new Random(f.Var * 13 + (int)x);
                        for (int i = 0; i < 7; i++)
                        {
                            double a = i * Math.PI * 2 / 7 + r.NextDouble() * 0.4;
                            var gc = i % 2 == 0 ? Art.Hx(0x2F8A55) : Art.Hx(0x24703F);
                            Ell(m, cx + (float)Math.Cos(a) * 0.38f, cy + (float)Math.Sin(a) * 0.38f, 2.15f + (float)r.NextDouble() * 0.35f, 0.5f, 0.5f, 0.45f, gc, r.Next(), 0.14f, 6, 10);
                        }
                        Ell(m, cx, cy, 2.75f, 0.5f, 0.5f, 0.42f, Art.Hx(0x3A9B62), r.Next(), 0.14f, 6, 10);
                        if (f.Var % 2 == 1)
                            for (int i = 0; i < 7; i++)
                            {
                                double a = i * 2.3;
                                Ell(m, cx + (float)Math.Cos(a) * 0.72f, cy + (float)Math.Sin(a) * 0.72f, 2.0f + (i % 3) * 0.3f, 0.07f, 0.07f, 0.07f, Art.Hx(0xE63946), i, 0, 4, 6);
                            }
                        break;
                    }
                case "fountain":
                    {
                        Box(m, x + 0.1f, y + 0.1f, 0, x + w - 0.1f, y + h - 0.1f, 0.45f, Art.Hx(0xCDD4DE), null, null, true);
                        // borda do lago (4 muretas) e água
                        float t = 0.14f;
                        Box(m, x + 0.1f, y + 0.1f, 0.3f, x + w - 0.1f, y + 0.1f + t, 0.5f, Art.Hx(0xD5DCE5));
                        Box(m, x + 0.1f, y + h - 0.1f - t, 0.3f, x + w - 0.1f, y + h - 0.1f, 0.5f, Art.Hx(0xD5DCE5));
                        Box(m, x + 0.1f, y + 0.1f, 0.3f, x + 0.1f + t, y + h - 0.1f, 0.5f, Art.Hx(0xD5DCE5));
                        Box(m, x + w - 0.1f - t, y + 0.1f, 0.3f, x + w - 0.1f, y + h - 0.1f, 0.5f, Art.Hx(0xD5DCE5));
                        var wc = Art.Hx(0x4A9BE8);
                        Face(m, rNoise, new V3(x + 0.24f, y + h - 0.24f, 0.4f), new V3(x + w - 0.24f, y + h - 0.24f, 0.4f), new V3(x + w - 0.24f, y + 0.24f, 0.4f), new V3(x + 0.24f, y + 0.24f, 0.4f), wc, 1f, 0, 0, 1, 1);
                        float cx = x + w / 2, cy = y + h / 2;
                        Cyl(m, cx, cy, 0.14f, 0.1f, 0.4f, 1.05f, Art.Hx(0xD5DCE5), 12);
                        Cyl(m, cx, cy, 0.1f, 0.32f, 1.0f, 1.12f, Art.Hx(0xCDD4DE), 14);
                        Cyl(m, cx, cy, 0.28f, 0.28f, 1.1f, 1.11f, Art.Hx(0x6CB6F5), 14);
                        Cyl(m, cx, cy, 0.03f, 0.01f, 1.11f, 1.35f, Art.Hx(0xBFE6FF), 8, false, true);
                        break;
                    }
                case "robot":
                    if (iconMode) { RobotMesh(m, f, 0, 0); break; }
                    robots.Add(f);
                    if (target != null) roomRobots.Add(f);
                    break;
                case "vending":
                    Box(m, x + 0.1f, y + 0.25f, 0, x + 0.9f, y + 0.8f, 2.0f, f.C, Front(f, "vd", x + 0.1f, y + 0.25f, 0.8f, 0.55f, 0, 56));
                    break;
                default:
                    if (!RoomModel(m, f)) Box(m, x + 0.05f, y + 0.05f, 0, x + w - 0.05f, y + h - 0.05f, 0.8f, f.C);
                    break;
            }
        }

        // ───────────────────────── câmera ─────────────────────────

        public readonly float[] Proj = new float[16], View = new float[16];

        // ponto do mundo (x, y, altura) → espaço da câmera
        public void ToView(float x, float y, float h, out float vx, out float vy, out float vz)
        {
            vx = View[0] * x + View[4] * h + View[8] * y + View[12];
            vy = View[1] * x + View[5] * h + View[9] * y + View[13];
            vz = View[2] * x + View[6] * h + View[10] * y + View[14];
        }

        // ponto do mundo → tela (câmera ortográfica da visão isométrica)
        public PointF Project(float x, float y, float h, int vw, int vh, out bool front)
        {
            float vx, vy, vz;
            ToView(x, y, h, out vx, out vy, out vz);
            front = true;
            return new PointF(orthoCX + vx * orthoPx, orthoCY - vy * orthoPx);
        }

        // ───────────────────────── chão (texturas HD da visão de cima) ─────────────────────────

        static readonly Dictionary<int, uint> floorTex = new Dictionary<int, uint>();

        void LoadFloor(Cell c)
        {
            var bmp = ChunkSource(c.CX, c.CY);
            if (bmp == null) return;
            const int size = 512;
            byte[] data;
            if (bmp.Width == size && bmp.Height == size) data = Rgba(bmp, PixelFormat.Format32bppPArgb, size, size);
            else
                using (var b2 = new Bitmap(size, size, PixelFormat.Format32bppPArgb))
                {
                    using (var g = Graphics.FromImage(b2)) { g.InterpolationMode = InterpolationMode.HighQualityBilinear; g.PixelOffsetMode = PixelOffsetMode.Half; g.DrawImage(bmp, 0, 0, size, size); }
                    data = Rgba(b2, PixelFormat.Format32bppPArgb, size, size);
                }
            FloorUploads++;
            floorTex[c.CX * 100 + c.CY] = Upload(data, size, size, true, 0, false);
        }

        static float Dist2(Cell c, float px, float py)
        {
            float cx = c.CX * CS + CS / 2f, cy = c.CY * CS + CS / 2f;
            return (cx - px) * (cx - px) + (cy - py) * (cy - py);
        }

        // ───────────────────────── personagens (cartazes) ─────────────────────────

        class STex { public uint Id; public float U, V; public long Bytes; public int Used; }
        readonly Dictionary<string, STex> stex = new Dictionary<string, STex>();
        public int SpriteUploads, FloorUploads;
        long stexBytes;
        int frameNo;
        const long SpriteMemory = 96L * 1024 * 1024;

        // perto: imagem em alta resolução; longe: versão menor (mais rápida de gerar e mais leve)
        static int SpriteQFor(float dist) { return dist < 8 ? 4 : 2; }

        STex MakeSprite(string key, Func<int, Bitmap> get, int q)
        {
            STex t;
            if (stex.TryGetValue(key + "|" + q, out t)) return t;
            var bmp = get(q);
            SpriteUploads++;
            int tw = Pot(bmp.Width), th = Pot(bmp.Height);
            var data = Rgba(bmp, PixelFormat.Format32bppPArgb, tw, th);
            t = new STex { Id = Upload(data, tw, th, true, 0, false), U = bmp.Width / (float)tw, V = bmp.Height / (float)th, Bytes = (long)tw * th * 16 / 3, Used = frameNo };
            bmp.Dispose();   // a imagem já está na placa de vídeo
            stex[key + "|" + q] = t;
            stexBytes += t.Bytes;
            // limite de memória de vídeo: descarta as imagens usadas há mais tempo
            if (stexBytes > SpriteMemory)
            {
                foreach (var kv in stex.Where(k => k.Value.Used < frameNo - 30).OrderBy(k => k.Value.Used).ToList())
                {
                    GL.DeleteTextures(1, new[] { kv.Value.Id });
                    stexBytes -= kv.Value.Bytes;
                    stex.Remove(kv.Key);
                    if (stexBytes < SpriteMemory * 0.8) break;
                }
            }
            return t;
        }

        STex FindSprite(Billboard b, int q)
        {
            STex t;
            int o = q == 4 ? 2 : 4;
            if (stex.TryGetValue(b.Key + "|" + q, out t) || stex.TryGetValue(b.Key + "|" + o, out t)) return t;
            if (b.Fallbacks != null)
                foreach (var k in b.Fallbacks)
                    if (stex.TryGetValue(k + "|" + q, out t) || stex.TryGetValue(k + "|" + o, out t)) return t;
            return null;
        }

        // ───────────────────────── quadro ─────────────────────────

        uint hudTex;
        int hudW, hudH;
        static readonly float[] FogColor = { 0.84f, 0.87f, 0.91f, 1f };

        static void V(float x, float y, float z) { GL.Vertex3(x, z, y); }

        // HUD (desenhado pelo GDI+ numa imagem transparente) por cima de tudo
        void DrawHudOverlay(Frame3D f)
        {
            int vw = f.VW, vh = f.VH;
            GL.Enable(GL.TEXTURE_2D);
            GL.Enable(GL.BLEND);
            GL.Disable(GL.FOG); GL.Disable(GL.DEPTH_TEST);
            if (GL.Samples > 0) GL.Disable(GL.MULTISAMPLE);
            if (f.Hud != IntPtr.Zero)
            {
                int tw = Pot(vw), th = Pot(vh);
                if (hudTex == 0 || tw != hudW || th != hudH)
                {
                    if (hudTex != 0) GL.DeleteTextures(1, new[] { hudTex });
                    var t = new uint[1];
                    GL.GenTextures(1, t);
                    hudTex = t[0]; hudW = tw; hudH = th;
                    GL.BindTexture(GL.TEXTURE_2D, hudTex);
                    GL.TexParameter(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.NEAREST);
                    GL.TexParameter(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.NEAREST);
                    GL.TexImage2D(GL.TEXTURE_2D, 0, (int)GL.RGBA8, tw, th, 0, GL.BGRA, GL.UNSIGNED_BYTE, IntPtr.Zero);
                }
                GL.BindTexture(GL.TEXTURE_2D, hudTex);
                GL.PixelStore(GL.UNPACK_ALIGNMENT, 4);
                GL.TexSubImage2D(GL.TEXTURE_2D, 0, 0, 0, vw, vh, GL.BGRA, GL.UNSIGNED_BYTE, f.Hud);
                GL.MatrixMode(GL.PROJECTION); GL.LoadIdentity(); GL.Ortho(0, vw, vh, 0, -1, 1);
                GL.MatrixMode(GL.MODELVIEW); GL.LoadIdentity();
                GL.BlendFunc(GL.ONE, GL.ONE_MINUS_SRC_ALPHA);
                GL.Color4(1, 1, 1, 1);
                float u = vw / (float)tw, v = vh / (float)th;
                GL.Begin(GL.QUADS);
                GL.TexCoord(0, 0); GL.Vertex2(0, 0);
                GL.TexCoord(u, 0); GL.Vertex2(vw, 0);
                GL.TexCoord(u, v); GL.Vertex2(vw, vh);
                GL.TexCoord(0, v); GL.Vertex2(0, vh);
                GL.End();
            }
            GL.Disable(GL.BLEND);
        }

        // coisas que mexem: luzes dos servidores e os robôs flutuando
        void DrawDynamic(Frame3D f)
        {
            var m = new Mesh();
            float t = f.Time;
            foreach (var r in racks)
            {
                if (Math.Abs(r.X - f.X) > 22 || Math.Abs(r.Y - f.Y) > 22) continue;
                float py = r.Y + 0.85f + 0.004f, x0 = r.X + 0.08f;
                for (int i = 0; i < 8; i++)
                {
                    float z = 2.2f - (3 + i * 6.8f + 2f) / 58f * 2.2f;
                    bool on = ((int)(t * 3 + r.Var * 7 + i * 13) % 5) != 0;
                    Color lc = (i + r.Var) % 3 == 0 ? Art.Hx(0x58A6FF) : Art.Hx(0x3FB950);
                    if (on) QuadC(m, rWhite, new V3(x0 + 0.1f, py, z - 0.025f), new V3(x0 + 0.15f, py, z - 0.025f), new V3(x0 + 0.15f, py, z + 0.025f), new V3(x0 + 0.1f, py, z + 0.025f), lc, lc, lc, lc, 0.3f, 0.3f, 0.7f, 0.7f);
                    if ((int)(t * 5 + r.Var + i) % 3 == 0)
                    {
                        var ac = Art.Hx(0xF0B429);
                        QuadC(m, rWhite, new V3(x0 + 0.24f, py, z - 0.02f), new V3(x0 + 0.29f, py, z - 0.02f), new V3(x0 + 0.29f, py, z + 0.02f), new V3(x0 + 0.24f, py, z + 0.02f), ac, ac, ac, ac, 0.3f, 0.3f, 0.7f, 0.7f);
                    }
                }
            }
            foreach (var r in robots)
            {
                if (Math.Abs(r.X - f.X) > 25 || Math.Abs(r.Y - f.Y) > 25) continue;
                RobotMesh(m, r, (float)Math.Sin(t * 3 + r.Var) * 0.05f, t);
            }
            DrawMesh(m);
            // sombras dos robôs
            GL.Enable(GL.BLEND); GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA); GL.DepthMask(false);
            GL.BindTexture(GL.TEXTURE_2D, pages[rShadow.Page].Tex);
            GL.Color4(1, 1, 1, 0.8f);
            foreach (var r in robots)
            {
                float cx = r.X + 0.5f, cy = r.Y + 0.6f;
                GL.Begin(GL.QUADS);
                GL.TexCoord(rShadow.U0, rShadow.V0); V(cx - 0.35f, cy - 0.25f, 0.004f);
                GL.TexCoord(rShadow.U1, rShadow.V0); V(cx + 0.35f, cy - 0.25f, 0.004f);
                GL.TexCoord(rShadow.U1, rShadow.V1); V(cx + 0.35f, cy + 0.25f, 0.004f);
                GL.TexCoord(rShadow.U0, rShadow.V1); V(cx - 0.35f, cy + 0.25f, 0.004f);
                GL.End();
            }
            GL.DepthMask(true); GL.Disable(GL.BLEND);
        }

        // cópia do quadro (para capturas de tela de teste)
        public Bitmap ReadBack(int vw, int vh)
        {
            // 32bppRgb: o canal alfa do OpenGL é ignorado
            var b = new Bitmap(vw, vh, PixelFormat.Format32bppRgb);
            var d = b.LockBits(new Rectangle(0, 0, vw, vh), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            GL.PixelStore(GL.PACK_ALIGNMENT, 4);
            GL.ReadPixels(0, 0, vw, vh, GL.BGRA, GL.UNSIGNED_BYTE, d.Scan0);
            b.UnlockBits(d);
            b.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return b;
        }
    }
}
