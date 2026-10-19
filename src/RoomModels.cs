using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace DockerQuest
{
    // Móveis da sala do jogador (modelos 3D), a lista de desenho da sala (refeita a cada compra),
    // os ícones da lojinha (o próprio modelo 3D desenhado em GDI+) e o embaçado de fome/sede.
    partial class World3D
    {
        Mesh target;                     // quando não é null, os modelos vão para esta malha (sala do jogador / ícones)
        bool iconMode;
        uint roomList;
        readonly List<Furn> roomRobots = new List<Furn>();
        Region rTv, rNeon, rPainting, rAquarium, rSpeaker;

        void RoomTextures()
        {
            foreach (var c in new[] { 0x1F6FEB, 0x2EA043, 0x8E6CC4, 0x3B82C4 }) Screen(Art.Hx(c));
            // TV: um "jogo" colorido na tela
            {
                var b = Bmp(256, 144);
                using (var g = Gfx(b))
                {
                    using (var lg = new LinearGradientBrush(new Rectangle(0, 0, 256, 144), Art.Hx(0x5DADE2), Art.Hx(0xA9DFBF), 90f)) g.FillRectangle(lg, 0, 0, 256, 144);
                    g.FillEllipse(Art.B(Art.Hx(0xF9E79F)), 190, 14, 34, 34);
                    g.FillPolygon(Art.B(Art.Hx(0x52BE80)), new[] { new PointF(0, 110), new PointF(70, 60), new PointF(140, 105), new PointF(200, 70), new PointF(256, 100), new PointF(256, 144), new PointF(0, 144) });
                    g.FillRectangle(Art.B(Art.Hx(0x1E8449)), 0, 120, 256, 24);
                    g.FillRectangle(Art.B(Art.Hx(0xE74C3C)), 60, 96, 18, 24);
                    g.FillRectangle(Art.B(Color.White), 64, 100, 4, 4);
                    using (var f = new Font("Segoe UI Black", 14, GraphicsUnit.Pixel)) g.DrawString("STACK QUEST TV", f, Art.B(Color.White), 8, 6);
                }
                rTv = Add("room_tv", b, true);
            }
            // letreiro neon </>
            {
                var b = Bmp(256, 96);
                using (var g = Gfx(b))
                {
                    g.Clear(Art.Hx(0x14121C));
                    using (var f = new Font("Consolas", 64, FontStyle.Bold, GraphicsUnit.Pixel))
                    {
                        for (int k = 6; k >= 1; k--) using (var p = new Pen(Art.A(Art.Hx(0xFF4FD8), 30), k * 3)) { var path = new GraphicsPath(); path.AddString("</>", f.FontFamily, (int)FontStyle.Bold, 64, new PointF(52, 10), StringFormat.GenericDefault); g.DrawPath(p, path); path.Dispose(); }
                        using (var path = new GraphicsPath())
                        {
                            path.AddString("</>", f.FontFamily, (int)FontStyle.Bold, 64, new PointF(52, 10), StringFormat.GenericDefault);
                            g.FillPath(Art.B(Art.Hx(0xFFC6F3)), path);
                            using (var p = new Pen(Art.Hx(0x39E7FF), 2)) g.DrawPath(p, path);
                        }
                    }
                }
                rNeon = Add("room_neon", b, true);
            }
            // pintura (paisagem)
            {
                var b = Bmp(128, 112);
                using (var g = Gfx(b))
                {
                    g.Clear(Art.Hx(0x6B4226));
                    using (var lg = new LinearGradientBrush(new Rectangle(8, 8, 112, 96), Art.Hx(0xF5B041), Art.Hx(0xF1948A), 90f)) g.FillRectangle(lg, 8, 8, 112, 96);
                    g.FillEllipse(Art.B(Art.Hx(0xFDFEFE)), 70, 20, 26, 26);
                    g.FillPolygon(Art.B(Art.Hx(0x6C3483)), new[] { new PointF(8, 80), new PointF(45, 40), new PointF(80, 78), new PointF(120, 50), new PointF(120, 104), new PointF(8, 104) });
                    g.FillRectangle(Art.B(Art.Hx(0x1B4F72)), 8, 92, 112, 12);
                }
                rPainting = Add("room_painting", b, false);
            }
            // aquário
            {
                var b = Bmp(192, 96);
                using (var g = Gfx(b))
                {
                    using (var lg = new LinearGradientBrush(new Rectangle(0, 0, 192, 96), Art.Hx(0x85C1E9), Art.Hx(0x1F618D), 90f)) g.FillRectangle(lg, 0, 0, 192, 96);
                    g.FillRectangle(Art.B(Art.Hx(0xE5C185)), 0, 84, 192, 12);
                    var r = new Random(4);
                    for (int i = 0; i < 6; i++) { float px = 12 + i * 30; g.FillPolygon(Art.B(Art.Hx(0x27AE60)), new[] { new PointF(px, 86), new PointF(px + 6, 40 + r.Next(25)), new PointF(px + 10, 86) }); }
                    var fish = new[] { 0xF39C12, 0xE74C3C, 0xF4D03F, 0xAF7AC5 };
                    for (int i = 0; i < 5; i++)
                    {
                        float fx = 20 + r.Next(150), fy = 14 + r.Next(55);
                        g.FillEllipse(Art.B(Art.Hx(fish[i % 4])), fx, fy, 16, 9);
                        g.FillPolygon(Art.B(Art.Hx(fish[i % 4])), new[] { new PointF(fx, fy + 4.5f), new PointF(fx - 7, fy), new PointF(fx - 7, fy + 9) });
                        g.FillEllipse(Art.B(Color.Black), fx + 11, fy + 3, 2.5f, 2.5f);
                    }
                    for (int i = 0; i < 8; i++) g.DrawEllipse(Pens.White, 150 + r.Next(30), 6 + i * 9, 4, 4);
                }
                rAquarium = Add("room_aquarium", b, false);
            }
            // caixa de som
            {
                var b = Bmp(64, 128);
                using (var g = Gfx(b))
                {
                    g.Clear(Art.Hx(0x1C1C22));
                    foreach (var c in new[] { new PointF(32, 34), new PointF(32, 88) })
                    {
                        float rr = c.Y < 50 ? 14 : 22;
                        g.FillEllipse(Art.B(Art.Hx(0x3A3A44)), c.X - rr - 3, c.Y - rr - 3, 2 * rr + 6, 2 * rr + 6);
                        g.FillEllipse(Art.B(Art.Hx(0x0E0E12)), c.X - rr, c.Y - rr, 2 * rr, 2 * rr);
                        g.FillEllipse(Art.B(Art.Hx(0x55555F)), c.X - rr * 0.35f, c.Y - rr * 0.35f, rr * 0.7f, rr * 0.7f);
                    }
                }
                rSpeaker = Add("room_speaker", b, false);
            }
        }

        // faixa de luz (LED/RGB): quad aceso, de uma cor
        void Glow(Mesh m, V3 a, V3 b, V3 c, V3 d, Color col) { QuadC(m, rWhite, a, b, c, d, col, col, col, col, 0.3f, 0.3f, 0.7f, 0.7f); }

        // computador por cima da mesa (ou no chão, no ícone da loja)
        void PcOnDesk(Mesh m, float x, float y, float w, float zb, int tier, bool tower)
        {
            float cx = x + w / 2;
            if (tier == 1) { Laptop3(m, cx, y + 0.18f, zb); return; }
            if (tier == 2) Monitor3(m, cx, y + 0.08f, zb, 0.62f, Screen(Art.Hx(0x3B82C4)));
            if (tier == 3) { Monitor3(m, cx - 0.36f, y + 0.08f, zb, 0.62f, Screen(Art.Hx(0x1F6FEB))); Monitor3(m, cx + 0.36f, y + 0.08f, zb, 0.62f, Screen(Art.Hx(0x2EA043))); }
            if (tier == 4)
            {
                Monitor3(m, cx, y + 0.06f, zb, 0.72f, rCode);
                Monitor3(m, cx - 0.74f, y + 0.12f, zb, 0.62f, Screen(Art.Hx(0x8E6CC4)));
                Monitor3(m, cx + 0.74f, y + 0.12f, zb, 0.62f, Screen(Art.Hx(0x1F6FEB)));
            }
            // teclado e mouse
            var kc = tier == 4 ? Art.Hx(0x1C1C22) : Art.Hx(0xDDE2E9);
            Box(m, cx - 0.24f, y + 0.5f, zb, cx + 0.24f, y + 0.64f, zb + 0.018f, kc);
            if (tier == 4) Glow(m, new V3(cx - 0.24f, y + 0.641f, zb), new V3(cx + 0.24f, y + 0.641f, zb), new V3(cx + 0.24f, y + 0.641f, zb + 0.016f), new V3(cx - 0.24f, y + 0.641f, zb + 0.016f), Art.Hx(0xFF4FD8));
            Box(m, cx + 0.34f, y + 0.54f, zb, cx + 0.42f, y + 0.66f, zb + 0.03f, kc);
            if (tower)
            {
                // gabinete gamer com faixas RGB, ao lado da mesa
                float tx = x + w + 0.06f, ty = y + 0.15f;
                Box(m, tx, ty, 0, tx + 0.36f, ty + 0.7f, 0.85f, Art.Hx(0x15161C));
                var cols = new[] { 0xFF4FD8, 0x39E7FF, 0x7CFF6B };
                for (int i = 0; i < 3; i++)
                {
                    float z0 = 0.18f + i * 0.2f;
                    Glow(m, new V3(tx + 0.05f, ty + 0.703f, z0), new V3(tx + 0.31f, ty + 0.703f, z0), new V3(tx + 0.31f, ty + 0.703f, z0 + 0.05f), new V3(tx + 0.05f, ty + 0.703f, z0 + 0.05f), Art.Hx(cols[i]));
                }
            }
        }

        // modelos que só existem na sala do jogador; devolve false se não conhece o tipo
        bool RoomModel(Mesh m, Furn f)
        {
            float x = f.X, y = f.Y, w = f.W, h = f.H;
            switch (f.Kind)
            {
                case "mydesk":
                    {
                        int pc = 0; int.TryParse(f.Text ?? "0", out pc);
                        Color c = f.C, leg = Art.Dk(c, 0.35f);
                        if (f.Var == 1)
                        {
                            Legs4(m, x + 0.08f, y + 0.1f, x + w - 0.08f, y + h - 0.1f, 0.74f, leg);
                            Box(m, x + 0.04f, y + 0.06f, 0.74f, x + w - 0.04f, y + h - 0.06f, 0.78f, c);
                        }
                        else
                        {
                            Box(m, x + w - 0.62f, y + 0.12f, 0, x + w - 0.08f, y + h - 0.12f, 0.72f, Art.Dk(c, 0.12f));
                            Box(m, x + w - 0.6f, y + h - 0.12f, 0.42f, x + w - 0.1f, y + h - 0.1f, 0.44f, Art.Lt(c, 0.25f));
                            Box(m, x + 0.08f, y + 0.12f, 0, x + 0.16f, y + h - 0.12f, 0.72f, leg);
                            Box(m, x + 0.16f, y + 0.12f, 0.3f, x + w - 0.62f, y + 0.18f, 0.72f, Art.Dk(c, 0.18f));
                            Box(m, x + 0.03f, y + 0.05f, 0.72f, x + w - 0.03f, y + h - 0.05f, 0.79f, c);
                            if (f.Var == 3)
                            {
                                // fita de LED por baixo do tampo
                                var cols = new[] { 0xFF4FD8, 0x39E7FF, 0x7CFF6B, 0xFFD93D };
                                int n = 6; float sw = (w - 0.06f) / n;
                                for (int i = 0; i < n; i++)
                                    Glow(m, new V3(x + 0.03f + i * sw, y + h - 0.049f, 0.695f), new V3(x + 0.03f + (i + 1) * sw, y + h - 0.049f, 0.695f), new V3(x + 0.03f + (i + 1) * sw, y + h - 0.049f, 0.72f), new V3(x + 0.03f + i * sw, y + h - 0.049f, 0.72f), Art.Hx(cols[i % 4]));
                            }
                        }
                        float top = f.Var == 1 ? 0.78f : 0.79f;
                        if (pc > 0) PcOnDesk(m, x + 0.4f, y, w - 0.8f, top, pc, pc == 4);
                        Cyl(m, x + 0.25f, y + 0.6f, 0.045f, 0.045f, top, top + 0.11f, Color.White, 10);
                        return true;
                    }
                case "pc":
                    PcOnDesk(m, x - 0.6f, y, 2.2f, 0, f.Var, f.Var == 4);
                    return true;
                case "stool":
                    Cyl(m, x + w / 2, y + h / 2, 0.2f, 0.2f, 0.42f, 0.48f, f.C, 14);
                    foreach (var p in new[] { new PointF(-0.12f, -0.12f), new PointF(0.12f, -0.12f), new PointF(0, 0.14f) })
                        Box(m, x + w / 2 + p.X - 0.025f, y + h / 2 + p.Y - 0.025f, 0, x + w / 2 + p.X + 0.025f, y + h / 2 + p.Y + 0.025f, 0.42f, Art.Dk(f.C, 0.3f));
                    return true;
                case "chair_office":
                case "chair_gamer":
                    {
                        bool gamer = f.Kind == "chair_gamer";
                        float cx = x + w / 2, cy = y + h / 2;
                        Color c = f.C, dk = Art.Hx(0x1C1C22);
                        // base com rodinhas
                        for (int i = 0; i < 5; i++)
                        {
                            double a = i * Math.PI * 2 / 5;
                            float ex = cx + (float)Math.Cos(a) * 0.3f, ey = cy + (float)Math.Sin(a) * 0.3f;
                            Box(m, Math.Min(cx, ex) - 0.025f, Math.Min(cy, ey) - 0.025f, 0.04f, Math.Max(cx, ex) + 0.025f, Math.Max(cy, ey) + 0.025f, 0.08f, dk);
                            Cyl(m, ex, ey, 0.04f, 0.04f, 0, 0.05f, dk, 8);
                        }
                        Cyl(m, cx, cy, 0.04f, 0.04f, 0.08f, 0.42f, Art.Hx(0x8A929E), 8, false);
                        Box(m, cx - 0.27f, cy - 0.25f, 0.42f, cx + 0.27f, cy + 0.22f, 0.52f, gamer ? dk : c);
                        // encosto do lado sul (quem senta olha para a mesa, ao norte)
                        float bz = gamer ? 1.38f : 1.05f;
                        Box(m, cx - 0.25f, cy + 0.22f, 0.5f, cx + 0.25f, cy + 0.32f, bz, gamer ? dk : c);
                        if (gamer)
                        {
                            Box(m, cx - 0.1f, cy + 0.215f, 0.56f, cx + 0.1f, cy + 0.33f, bz - 0.06f, c);
                            Box(m, cx - 0.27f, cy + 0.215f, 0.52f, cx - 0.2f, cy + 0.33f, bz - 0.1f, c);
                            Box(m, cx + 0.2f, cy + 0.215f, 0.52f, cx + 0.27f, cy + 0.33f, bz - 0.1f, c);
                            Box(m, cx - 0.14f, cy + 0.2f, bz - 0.22f, cx + 0.14f, cy + 0.22f, bz - 0.1f, Color.White);
                        }
                        foreach (int s in new[] { -1, 1 }) Box(m, cx + s * 0.27f - 0.03f, cy - 0.15f, 0.52f, cx + s * 0.27f + 0.03f, cy + 0.2f, 0.68f, dk);
                        return true;
                    }
                case "bigplant":
                    {
                        float cx = x + 0.5f, cy = y + 0.55f;
                        Cyl(m, cx, cy, 0.2f, 0.26f, 0, 0.55f, Art.Hx(0xE8E8E8), 14, false);
                        Cyl(m, cx, cy, 0.24f, 0.24f, 0.5f, 0.52f, Art.Hx(0x4A3426), 14);
                        Cyl(m, cx, cy, 0.05f, 0.04f, 0.5f, 1.35f, Art.Hx(0x6E4527), 8, false);
                        var r = new Random(77);
                        for (int i = 0; i < 6; i++)
                        {
                            double a = i * Math.PI * 2 / 6;
                            Ell(m, cx + (float)Math.Cos(a) * 0.22f, cy + (float)Math.Sin(a) * 0.22f, 1.4f + (float)r.NextDouble() * 0.3f, 0.3f, 0.3f, 0.28f, i % 2 == 0 ? Art.Hx(0x2F8A55) : Art.Hx(0x3A9B62), r.Next(), 0.14f);
                        }
                        Ell(m, cx, cy, 1.85f, 0.3f, 0.3f, 0.26f, Art.Hx(0x45A86C), 9, 0.14f);
                        return true;
                    }
                case "tv":
                    Box(m, x, y + 0.1f, 0, x + w, y + h, 0.45f, Art.Hx(0x5C4033));
                    Box(m, x + w / 2 - 0.05f, y + 0.18f, 0.45f, x + w / 2 + 0.05f, y + 0.24f, 0.62f, Art.Hx(0x22252C));
                    Box(m, x + 0.12f, y + 0.12f, 0.6f, x + w - 0.12f, y + 0.2f, 1.62f, Art.Hx(0x15161C), null, null, false, true);
                    PanelS(m, rTv, x + 0.16f, x + w - 0.16f, y + 0.202f, 0.64f, 1.58f, Color.White, 1);
                    return true;
                case "neon":
                    PanelS(m, rNeon, x, x + w, y + 0.03f, 1.62f, 2.22f, Color.White, 1);
                    return true;
                case "frame":
                    Box(m, x, y + 0.01f, 1.15f, x + w, y + 0.06f, 1.88f, Art.Hx(0x6B4226));
                    PanelS(m, rPainting, x + 0.05f, x + w - 0.05f, y + 0.062f, 1.2f, 1.83f, Color.White, 0.97f);
                    return true;
                case "carpet":
                    {
                        float cx = x + w / 2, cy = y + h / 2;
                        Cyl(m, cx, cy, 1.45f, 1.45f, 0.003f, 0.018f, f.C, 28);
                        Cyl(m, cx, cy, 1.0f, 1.0f, 0.018f, 0.02f, Art.Hx(0xF4A261), 28);
                        Cyl(m, cx, cy, 0.5f, 0.5f, 0.02f, 0.022f, Art.Hx(0x2A9D8F), 24);
                        return true;
                    }
                case "lamp":
                    {
                        float cx = x + w / 2, cy = y + h / 2;
                        Cyl(m, cx, cy, 0.18f, 0.16f, 0, 0.05f, Art.Hx(0x2B2D42), 14);
                        Cyl(m, cx, cy, 0.02f, 0.02f, 0.05f, 1.5f, Art.Hx(0x8A929E), 6, false);
                        Cyl(m, cx, cy, 0.24f, 0.15f, 1.42f, 1.75f, Art.Hx(0xFFE7A3), 16, true, true);
                        return true;
                    }
                case "fridge":
                    Box(m, x + 0.12f, y + 0.15f, 0, x + 0.88f, y + 0.85f, 1.05f, Art.Hx(0xDDE3EA));
                    Box(m, x + 0.74f, y + 0.85f, 0.45f, x + 0.79f, y + 0.9f, 0.9f, Art.Hx(0x6B7486));
                    Box(m, x + 0.14f, y + 0.85f, 0.98f, x + 0.86f, y + 0.86f, 0.995f, Art.Hx(0x9AA6B5));
                    Glow(m, new V3(x + 0.2f, y + 0.852f, 0.9f), new V3(x + 0.26f, y + 0.852f, 0.9f), new V3(x + 0.26f, y + 0.852f, 0.94f), new V3(x + 0.2f, y + 0.852f, 0.94f), Art.Hx(0x39E7FF));
                    return true;
                case "aquarium":
                    Box(m, x, y + 0.05f, 0, x + w, y + h, 0.7f, Art.Hx(0x3E2A1E));
                    Box(m, x + 0.03f, y + 0.08f, 0.7f, x + w - 0.03f, y + h - 0.03f, 1.3f, Art.Hx(0x5DADE2), rAquarium);
                    Box(m, x, y + 0.05f, 1.3f, x + w, y + h, 1.36f, Art.Hx(0x2B2D42));
                    return true;
                case "trophy":
                    {
                        float cx = x + w / 2, cy = y + h / 2;
                        Box(m, cx - 0.25f, cy - 0.25f, 0, cx + 0.25f, cy + 0.25f, 0.9f, Art.Hx(0x2B2D42));
                        Box(m, cx - 0.12f, cy - 0.12f, 0.9f, cx + 0.12f, cy + 0.12f, 0.96f, Art.Hx(0xB8860B));
                        Cyl(m, cx, cy, 0.03f, 0.03f, 0.96f, 1.05f, Art.Hx(0xD4AC0D), 8, false);
                        Cyl(m, cx, cy, 0.06f, 0.15f, 1.05f, 1.32f, Art.Hx(0xF5C542), 14);
                        foreach (int s in new[] { -1, 1 }) Box(m, cx + s * 0.15f - 0.025f, cy - 0.02f, 1.12f, cx + s * 0.15f + 0.025f, cy + 0.02f, 1.26f, Art.Hx(0xF5C542));
                        return true;
                    }
                case "speaker":
                    foreach (int s in new[] { 0, 1 })
                    {
                        float sx = x + s * (w - 0.3f);
                        Box(m, sx, y, 0, sx + 0.3f, y + h, 0.95f, Art.Hx(0x1C1C22), rSpeaker);
                    }
                    return true;
            }
            return false;
        }

        // sofá virado para o norte (encosto ao sul), usado na sala do jogador
        void SofaNorth(Mesh m, Furn f)
        {
            float x = f.X, y = f.Y, w = f.W, h = f.H;
            Color c = f.C;
            Box(m, x, y + h - 0.4f, 0, x + w, y + h - 0.05f, 0.82f, Art.Dk(c, 0.08f));
            Box(m, x + 0.05f, y + 0.05f, 0, x + w - 0.05f, y + h - 0.35f, 0.4f, Art.Dk(c, 0.05f));
            int n = Math.Max(1, (int)Math.Round((w - 0.36f) / 0.9f));
            float cw = (w - 0.36f) / n;
            for (int i = 0; i < n; i++)
            {
                Box(m, x + 0.18f + i * cw + 0.02f, y + 0.08f, 0.4f, x + 0.18f + (i + 1) * cw - 0.02f, y + h - 0.42f, 0.5f, Art.Lt(c, 0.08f));
                Box(m, x + 0.18f + i * cw + 0.03f, y + h - 0.52f, 0.48f, x + 0.18f + (i + 1) * cw - 0.03f, y + h - 0.4f, 0.78f, Art.Lt(c, 0.04f));
            }
            Box(m, x, y + 0.05f, 0, x + 0.18f, y + h - 0.2f, 0.6f, c);
            Box(m, x + w - 0.18f, y + 0.05f, 0, x + w, y + h - 0.2f, 0.6f, c);
        }

        void RobotMesh(Mesh m, Furn r, float hv, float t)
        {
            float x = r.X, y = r.Y;
            Box(m, x + 0.27f, y + 0.45f, 0.3f + hv, x + 0.73f, y + 0.75f, 0.85f + hv, Art.Hx(0xD5DCE5), null, null, false, true);
            Box(m, x + 0.29f, y + 0.47f, 0.88f + hv, x + 0.71f, y + 0.73f, 1.22f + hv, Art.Hx(0xF1F4F8), null, null, false, true);
            PanelS(m, rRobot, x + 0.32f, x + 0.68f, y + 0.733f, 0.98f + hv, 1.15f + hv, Color.White, 1);
            Cyl(m, x + 0.5f, y + 0.6f, 0.01f, 0.01f, 1.22f + hv, 1.42f + hv, Art.Hx(0x6B7486), 6, false);
            bool blink = (int)(t * 2 + r.Var) % 2 == 0;
            Ell(m, x + 0.5f, y + 0.6f, 1.44f + hv, 0.04f, 0.04f, 0.04f, blink ? Art.Hx(0xF85149) : Art.Hx(0x5C2430), 1, 0, 4, 6);
        }

        // refaz a lista de desenho da sala do jogador (depois de cada compra)
        class BadgeFrame { public Region R; public float Y0, Y1, Z0, Z1; }
        readonly List<BadgeFrame> badgeFrames = new List<BadgeFrame>();

        // quadro de uma badge: moldura de madeira, fundo creme e a medalha no meio
        Region BadgeRegion(Category cat, int tier)
        {
            string key = "badgeframe|" + cat.Id + "|" + tier;
            Region r;
            if (regions.TryGetValue(key, out r)) return r;
            const int S = 160;
            var b = Bmp(S, S);
            using (var g = Gfx(b))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var lg = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, S, S), Art.Hx(0x8A5A32), Art.Hx(0x4E2F17), 45f))
                    g.FillRectangle(lg, 0, 0, S, S);
                g.DrawRectangle(new Pen(Art.Hx(0x2E1A0C), 3), 1, 1, S - 3, S - 3);
                g.DrawRectangle(new Pen(Art.Hx(0xB5875A), 2), 7, 7, S - 15, S - 15);
                // fundo (passe-partout) com a cor da área bem clarinha
                var mat = Art.Lt(Art.Hx(0xF3ECDD), 0.1f);
                g.FillRectangle(new SolidBrush(mat), 12, 12, S - 24, S - 24);
                g.FillRectangle(new SolidBrush(Art.A(cat.Color, 40)), 12, 12, S - 24, S - 24);
                g.DrawRectangle(new Pen(Art.A(Color.Black, 60), 2), 12, 12, S - 25, S - 25);
                BadgeForm.DrawBadge(g, S / 2f, S * 0.42f, S * 0.27f, cat, tier, null, true);
            }
            return Add(key, b, false);
        }

        void BuildBadgeFrames()
        {
            badgeFrames.Clear();
            foreach (var kv in W.RoomBadges)
            {
                int ci = Array.FindIndex(Challenges.Cats, c => c.Id == kv.Key);
                if (ci < 0) continue;
                float yc = PlayerRoom.BadgeY(ci), z0 = PlayerRoom.BadgeZ(kv.Value), h = PlayerRoom.BadgeSize;
                badgeFrames.Add(new BadgeFrame { R = BadgeRegion(Challenges.Cats[ci], kv.Value), Y0 = yc - h / 2, Y1 = yc + h / 2, Z0 = z0, Z1 = z0 + h });
            }
        }

        // quadros na face leste da parede oeste da sala; somem quando a parede é cortada perto do jogador
        void DrawBadgeFrames(Mesh m)
        {
            int wx = (int)PlayerRoom.BadgeWallX - 1;
            float x = PlayerRoom.BadgeWallX + 0.012f;
            foreach (var f in badgeFrames)
            {
                bool show = true;
                for (int ty = (int)Math.Floor(f.Y0); ty <= (int)Math.Floor(f.Y1); ty++)
                {
                    float wh = WallHAt(wx, ty);
                    if (wh >= 0 && wh < f.Z1 - 0.02f) { show = false; break; }
                }
                if (!show) continue;
                // virado para o leste: da ponta sul (u = 0) para a norte (u = 1)
                Face(m, f.R, new V3(x, f.Y1, f.Z0), new V3(x, f.Y0, f.Z0), new V3(x, f.Y0, f.Z1), new V3(x, f.Y1, f.Z1), Color.White, 0.97f);
            }
        }

        public void RebuildRoom(List<Furn> items)
        {
            if (cells == null) return;
            BuildBadgeFrames();
            robots.RemoveAll(r => roomRobots.Contains(r));
            roomRobots.Clear();
            var m = new Mesh();
            target = m;
            try { foreach (var f in items) Model(f); }
            finally { target = null; }
            ReUploadDirty();
            if (roomList != 0) GL.DeleteLists(roomList, 1);
            roomList = 0;
            if (!m.Empty) { roomList = GL.GenLists(1); GL.NewList(roomList, GL.COMPILE); DrawMesh(m); GL.EndList(); }
        }

        // piso trocado: descarta as texturas e as listas de cor dos blocos de chão dessa área
        public void InvalidateFloor(int x0, int y0, int x1, int y1)
        {
            foreach (var c in cells)
            {
                if (c.CX * CS > x1 || c.CX * CS + CS <= x0 || c.CY * CS > y1 || c.CY * CS + CS <= y0) continue;
                uint tex;
                if (floorTex.TryGetValue(c.CX * 100 + c.CY, out tex)) { GL.DeleteTextures(1, new[] { tex }); floorTex.Remove(c.CX * 100 + c.CY); }
                if (c.LF != 0) { GL.DeleteLists(c.LF, 1); c.LF = 0; }
                BuildFallbackFloor(c);
            }
        }

        void BuildFallbackFloor(Cell c)
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
        }

        // folhas de textura que ganharam imagens novas depois da montagem: manda de novo para a placa de vídeo
        void ReUploadDirty()
        {
            foreach (var p in pages)
            {
                if (!p.Dirty || p.Bmp == null || p.Tex == 0) continue;
                p.Dirty = false;
                var data = Rgba(p.Bmp, PixelFormat.Format32bppArgb, PageSize, PageSize);
                GL.BindTexture(GL.TEXTURE_2D, p.Tex);
                GL.PixelStore(GL.UNPACK_ALIGNMENT, 1);
                if (gl14) GL.TexImage2D(GL.TEXTURE_2D, 0, (int)GL.RGBA8, PageSize, PageSize, 0, GL.RGBA, GL.UNSIGNED_BYTE, data);
                else GL.Build2DMipmaps(GL.TEXTURE_2D, 4, PageSize, PageSize, GL.RGBA, GL.UNSIGNED_BYTE, data);
            }
        }

        // ── ícone da lojinha: o próprio modelo 3D, desenhado no GDI+ em projeção isométrica
        public Bitmap FurnIcon(Furn f, int size)
        {
            var m = new Mesh { Track = true };
            target = m; iconMode = true;
            try
            {
                if (f.Kind == "robot") RobotMesh(m, f, 0, 0);
                else Model(f);
            }
            finally { target = null; iconMode = false; }
            const double yaw = -3 * Math.PI / 4, pit = Math.PI / 6;
            float fxh = (float)Math.Cos(yaw), fyh = (float)Math.Sin(yaw), rx = -fyh, ry = fxh, cp = (float)Math.Cos(pit), sp = (float)Math.Sin(pit);
            Func<float, float, float, PointF> proj = (x, y, z) => new PointF(x * rx + y * ry, -(z * cp + (x * fxh + y * fyh) * sp));
            Func<float, float, float, float> depth = (x, y, z) => (x * fxh + y * fyh) * cp - z * sp;
            var shapes = new List<KeyValuePair<float, int[]>>();
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (var s in m.Shapes)
            {
                var l = m.Data[s[0]];
                float d = 0;
                for (int k = 0; k < s[2]; k++)
                {
                    int i = s[1] + k * 9;
                    var pt = proj(l[i], l[i + 1], l[i + 2]);
                    minX = Math.Min(minX, pt.X); maxX = Math.Max(maxX, pt.X); minY = Math.Min(minY, pt.Y); maxY = Math.Max(maxY, pt.Y);
                    d += depth(l[i], l[i + 1], l[i + 2]);
                }
                shapes.Add(new KeyValuePair<float, int[]>(d / s[2], s));
            }
            var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            if (shapes.Count == 0) return bmp;
            float sc = (size * 0.86f) / Math.Max(maxX - minX, maxY - minY);
            float ox = size / 2f - (minX + maxX) / 2 * sc, oy = size / 2f - (minY + maxY) / 2 * sc;
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                foreach (var kv in shapes.OrderByDescending(k => k.Key))
                {
                    var s = kv.Value;
                    var l = m.Data[s[0]];
                    var pts = new PointF[s[2] == 6 ? 4 : 3];
                    int[] idx = s[2] == 6 ? new[] { 0, 1, 2, 5 } : new[] { 0, 1, 2 };
                    int rr = 0, gg = 0, bb = 0;
                    for (int k = 0; k < pts.Length; k++)
                    {
                        int i = s[1] + idx[k] * 9;
                        var pt = proj(l[i], l[i + 1], l[i + 2]);
                        pts[k] = new PointF(ox + pt.X * sc, oy + pt.Y * sc);
                        rr += (int)l[i + 5]; gg += (int)l[i + 6]; bb += (int)l[i + 7];
                    }
                    var col = Color.FromArgb(255, rr / pts.Length, gg / pts.Length, bb / pts.Length);
                    var page = pages[s[0]];
                    float u0 = l[s[1] + 3], v1 = l[s[1] + 4];
                    bool solid = s[2] != 6 || page.Bmp == null || InRegion(rNoise, u0, v1) || InRegion(rWhite, u0, v1);
                    if (solid)
                    {
                        g.FillPolygon(Art.B(col), pts);
                        using (var pe = new Pen(col, 1f)) g.DrawPolygon(pe, pts);
                    }
                    else
                    {
                        // quad com textura: canto d (u0,v0) em cima à esquerda, c (u1,v0) em cima à direita, a (u0,v1) embaixo à esquerda
                        int ic = s[1] + 2 * 9;
                        float u1 = l[ic + 3], v0 = l[ic + 4];
                        var src = RectangleF.FromLTRB(Math.Min(u0, u1) * PageSize, Math.Min(v0, v1) * PageSize, Math.Max(u0, u1) * PageSize, Math.Max(v0, v1) * PageSize);
                        PointF a = pts[0], c = pts[2], d = pts[3];
                        var dest = u1 >= u0 ? new[] { d, c, a } : new[] { c, d, pts[1] };
                        float kr = col.R / 255f, kg = col.G / 255f, kb = col.B / 255f;
                        using (var ia = new ImageAttributes())
                        {
                            ia.SetColorMatrix(new ColorMatrix(new[] { new[] { kr, 0, 0, 0, 0f }, new[] { 0, kg, 0, 0, 0f }, new[] { 0, 0, kb, 0, 0f }, new[] { 0, 0, 0, 1f, 0 }, new[] { 0, 0, 0, 0, 1f } }));
                            g.DrawImage(page.Bmp, dest, src, GraphicsUnit.Pixel, ia);
                        }
                    }
                }
            }
            return bmp;
        }

        static bool InRegion(Region r, float u, float v) { return u >= r.U0 - 0.002f && u <= r.U1 + 0.002f && v >= r.V0 - 0.002f && v <= r.V1 + 0.002f; }

        // ── embaçado (fome/sede): a cena é copiada e redesenhada várias vezes deslocada, em média
        uint blurTex;
        int blurW, blurH;

        void BlurScene(int vw, int vh, float level)
        {
            int tw = Pot(vw), th = Pot(vh);
            if (blurTex == 0 || tw != blurW || th != blurH)
            {
                if (blurTex != 0) GL.DeleteTextures(1, new[] { blurTex });
                var t = new uint[1];
                GL.GenTextures(1, t);
                blurTex = t[0]; blurW = tw; blurH = th;
                GL.BindTexture(GL.TEXTURE_2D, blurTex);
                GL.TexParameter(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.LINEAR);
                GL.TexParameter(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.LINEAR);
                GL.TexParameter(GL.TEXTURE_2D, GL.TEXTURE_WRAP_S, gl12 ? GL.CLAMP_TO_EDGE : GL.CLAMP);
                GL.TexParameter(GL.TEXTURE_2D, GL.TEXTURE_WRAP_T, gl12 ? GL.CLAMP_TO_EDGE : GL.CLAMP);
                GL.TexImage2D(GL.TEXTURE_2D, 0, (int)GL.RGBA8, tw, th, 0, GL.RGBA, GL.UNSIGNED_BYTE, IntPtr.Zero);
            }
            GL.Disable(GL.DEPTH_TEST);
            GL.Enable(GL.TEXTURE_2D);
            GL.BindTexture(GL.TEXTURE_2D, blurTex);
            GL.MatrixMode(GL.PROJECTION); GL.LoadIdentity(); GL.Ortho(0, vw, vh, 0, -1, 1);
            GL.MatrixMode(GL.MODELVIEW); GL.LoadIdentity();
            GL.Enable(GL.BLEND); GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
            float u = vw / (float)tw, v = vh / (float)th;
            float radius = 2 + level * 16;
            const int taps = 12;
            for (int pass = 0; pass < 2; pass++)
            {
                // cada passada lê o resultado da anterior: o borrão fica mais suave
                GL.CopyTexSubImage2D(GL.TEXTURE_2D, 0, 0, 0, 0, 0, vw, vh);
                float rad = pass == 0 ? radius : radius * 0.5f;
                for (int i = 0; i < taps; i++)
                {
                    double a = i * Math.PI * 2 / taps + pass * 0.26;
                    float dx = (float)Math.Cos(a) * rad * (i % 2 == 0 ? 1 : 0.6f), dy = (float)Math.Sin(a) * rad * (i % 2 == 0 ? 1 : 0.6f);
                    GL.Color4(1, 1, 1, 1f / (i + 2));   // média: a camada nova pesa igual às anteriores
                    GL.Begin(GL.QUADS);
                    GL.TexCoord(0, v); GL.Vertex2(dx, dy);
                    GL.TexCoord(u, v); GL.Vertex2(vw + dx, dy);
                    GL.TexCoord(u, 0); GL.Vertex2(vw + dx, vh + dy);
                    GL.TexCoord(0, 0); GL.Vertex2(dx, vh + dy);
                    GL.End();
                }
            }
            // a visão escurece e perde a cor aos poucos
            GL.Disable(GL.TEXTURE_2D);
            GL.Color4(0.12f, 0.1f, 0.08f, 0.32f * level);
            GL.Begin(GL.QUADS); GL.Vertex2(0, 0); GL.Vertex2(vw, 0); GL.Vertex2(vw, vh); GL.Vertex2(0, vh); GL.End();
            GL.Enable(GL.TEXTURE_2D);
            GL.Disable(GL.BLEND);
        }
    }
}
