using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace DockerQuest
{
    // Boneco de blocos (estilo Habbo): a mesma montagem serve para o OpenGL (jogo) e para o GDI+
    // (prévia na criação de personagem e no guarda-roupa).
    static class BlockAvatar
    {
        // face de um bloco: 4 cantos (de baixo à esquerda, no sentido anti-horário visto de fora), cor já com a luz,
        // textura do rosto (-1 nenhuma, 0 frente, 1 lado, 2 nuca, 3 topo) e se o lado é espelhado
        public class BQuad { public V3 A, B, C, D; public Color Col; public int Tex = -1; public bool Mirror; public int Part; }
        static int curPart;
        static float zLift;   // em cima do patinete o corpo sobe um pouco   // 0 corpo, 1/2 pernas, 3/4 braços (o jogo anima cada parte)

        static Color Mul(Color c, float k) { return Color.FromArgb(c.A, Math.Min(255, (int)(c.R * k)), Math.Min(255, (int)(c.G * k)), Math.Min(255, (int)(c.B * k))); }

        public static Color TopColor(Look L)
        {
            switch (L.Outfit ?? "")
            {
                case "moletom": return Art.Docker;
                case "jaleco": return Art.Hx(0xF1F5F9);
                case "terno": return Art.Hx(0x1F2937);
                case "havaiana": return Art.Hx(0xF4A261);
                case "polo_noc": return Art.Hx(0x1B1F24);
                case "camiseta_python": return Art.Hx(0x3776AB);
                case "camiseta_php": return Art.Hx(0x777BB4);
                case "jaqueta": return Art.Hx(0x3B2A20);
                case "moletom_mic": return Art.Hx(0x8A929E);
            }
            return L.Shirt;
        }

        public const int FaceS = 32;   // cada lado da cabeça é uma imagem de 32×32

        // rosto, lados, nuca e topo da cabeça desenhados com traços suaves (uma tira de 128×32)
        public static Bitmap FaceBitmap(Look L)
        {
            const int S = FaceS;
            var b = new Bitmap(S * 4, S, PixelFormat.Format32bppArgb);
            Color skin = L.Skin, skinDk = Art.Dk(skin, 0.14f), hair = L.Hair, hairDk = Art.Dk(hair, 0.28f), hairLt = Art.Lt(hair, 0.22f);
            Color eye = Art.Hx(0x24170F), lip = L.Female ? Art.Hx(0xC2456B) : Art.Hx(0xA0524F);
            int st = L.HairStyle;
            bool cap = st == 5, fem = L.Female, longSides = st == 1 || st == 4 || st == 8, dark = L.Acc == "oculos_escuros";
            Color capC = L.Accent, capDk = Art.Dk(capC, 0.25f), hc = cap ? capC : hair;
            using (var g = Graphics.FromImage(b))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.FillRectangle(Art.B(skin), 0, 0, S * 4, S);
                // ── frente
                using (var lg = new LinearGradientBrush(new Rectangle(0, 15, S, 18), Color.FromArgb(0, 0, 0, 0), Color.FromArgb(46, 0, 0, 0), 90f)) g.FillRectangle(lg, 0, 16, S, 16);
                if (st == 3) g.FillRectangle(Art.B(hc), 0, 0, S, 3);
                else
                {
                    g.FillRectangle(Art.B(hc), 0, 0, S, 7);
                    if (cap) g.FillRectangle(Art.B(capDk), 0, 5, S, 3);
                    else
                    {
                        g.FillPolygon(Art.B(hc), new[] { new PointF(-1, 6), new PointF(20, 6), new PointF(9, 11.5f), new PointF(-1, 12.5f) });
                        g.FillRectangle(Art.B(Art.A(hairLt, 170)), 3, 2, 11, 2);
                    }
                }
                if (longSides) { g.FillRectangle(Art.B(hair), 0, 5, 4, 20); g.FillRectangle(Art.B(hair), S - 4, 5, 4, 20); }
                if (st == 7) { g.FillRectangle(Art.B(hair), 0, 5, 3, 9); g.FillRectangle(Art.B(hair), S - 3, 5, 3, 9); }
                if (dark)
                {
                    Art.RR(g, Art.Hx(0x111318), 5, 12, 10, 6, 2); Art.RR(g, Art.Hx(0x111318), 17, 12, 10, 6, 2);
                    g.FillRectangle(Art.B(Art.Hx(0x111318)), 14, 13, 4, 1.5f);
                    g.FillRectangle(Art.B(Art.A(Color.White, 90)), 7, 13, 3, 1.2f);
                }
                else
                {
                    if (!cap && st != 3) { g.FillRectangle(Art.B(hairDk), 7, 10, 6, 1.6f); g.FillRectangle(Art.B(hairDk), 19, 10, 6, 1.6f); }
                    g.FillEllipse(Art.B(Color.White), 7.5f, 12.5f, 5, 5.5f); g.FillEllipse(Art.B(Color.White), 19.5f, 12.5f, 5, 5.5f);
                    g.FillEllipse(Art.B(eye), 8.6f, 13.2f, 3.6f, 4.6f); g.FillEllipse(Art.B(eye), 20.6f, 13.2f, 3.6f, 4.6f);
                    g.FillEllipse(Art.B(Color.White), 9.4f, 13.8f, 1.4f, 1.4f); g.FillEllipse(Art.B(Color.White), 21.4f, 13.8f, 1.4f, 1.4f);
                    if (fem)
                    {
                        using (var pe = new Pen(eye, 1.1f)) { g.DrawLine(pe, 7f, 13.2f, 5.6f, 12f); g.DrawLine(pe, 25.2f, 13.2f, 26.6f, 12f); }
                        g.FillEllipse(Art.B(Art.A(Art.Hx(0xF08A8A), 110)), 3.5f, 18.5f, 6, 3.2f); g.FillEllipse(Art.B(Art.A(Art.Hx(0xF08A8A), 110)), 22.5f, 18.5f, 6, 3.2f);
                    }
                    if (L.Glasses)
                        using (var pg = new Pen(Art.Hx(0x1B1B1F), 1.3f))
                        {
                            using (var p1 = Art.RoundPath(6, 11.5f, 8.5f, 7, 2)) g.DrawPath(pg, p1);
                            using (var p2 = Art.RoundPath(18, 11.5f, 8.5f, 7, 2)) g.DrawPath(pg, p2);
                            g.DrawLine(pg, 14.5f, 14, 17.5f, 14);
                        }
                }
                g.FillEllipse(Art.B(skinDk), 14.5f, 18.5f, 3, 2.6f);
                if (fem) g.FillEllipse(Art.B(lip), 12.5f, 22.4f, 7, 3);
                else { g.FillEllipse(Art.B(lip), 12.5f, 22.8f, 7, 2.2f); }
                // ── lado (a frente do rosto fica à esquerda)
                int ox = S;
                using (var lg = new LinearGradientBrush(new Rectangle(ox, 15, S, 18), Color.FromArgb(0, 0, 0, 0), Color.FromArgb(46, 0, 0, 0), 90f)) g.FillRectangle(lg, ox, 16, S, 16);
                int backTo = longSides ? 26 : st == 3 ? 6 : st == 6 || st == 7 ? 20 : 18;
                g.FillRectangle(Art.B(hc), ox, 0, S, st == 3 ? 3 : 7);
                if (!cap) g.FillPolygon(Art.B(hair), new[] { new PointF(ox + 13, 6), new PointF(ox + S, 6), new PointF(ox + S, backTo), new PointF(ox + 17, backTo), new PointF(ox + 15, 12) });
                else { g.FillRectangle(Art.B(capDk), ox, 5, S, 3); g.FillRectangle(Art.B(hair), ox + 18, 8, S - 18, 10); }
                // lado da cabeça: só a orelha (olhos e boca ficam apenas na frente do rosto, senão o boneco parece ter olhos dos lados)
                g.FillEllipse(Art.B(skinDk), ox + 14, 12, 5, 8);
                g.FillEllipse(Art.B(Art.Dk(skin, 0.25f)), ox + 15.5f, 14, 2, 4);
                if (L.Glasses || dark) using (var pg = new Pen(Art.Hx(0x1B1B1F), 1.3f)) g.DrawLine(pg, ox, 13.5f, ox + 14, 13f);
                // ── nuca
                ox = 2 * S;
                int hb = longSides || st == 6 || st == 7 ? S : st == 3 ? 6 : 22;
                g.FillRectangle(Art.B(hair), ox, 0, S, hb);
                if (cap) g.FillRectangle(Art.B(capC), ox, 0, S, 7);
                g.FillRectangle(Art.B(Art.A(hairLt, 120)), ox + 4, 2, 12, 2);
                if (hb < S) using (var lg = new LinearGradientBrush(new Rectangle(ox, hb - 1, S, S - hb + 2), Color.FromArgb(50, 0, 0, 0), Color.FromArgb(0, 0, 0, 0), 90f)) g.FillRectangle(lg, ox, hb, S, S - hb);
                // ── topo
                ox = 3 * S;
                g.FillRectangle(Art.B(hc), ox, 0, S, S);
                if (!cap)
                    using (var ph = new Pen(Art.A(hairDk, 120), 1f))
                        for (int i = 0; i < 6; i++) g.DrawLine(ph, ox + 3 + i * 5, 2, ox + 1 + i * 5, S - 3);
                g.FillRectangle(Art.B(Art.A(hairLt, 110)), ox + 6, 6, 10, 3);
            }
            return b;
        }

        // bloco com rotação em torno do eixo x (pernas e braços balançando); tex: 1 topo, 2 frente, 4 nuca, 8 lados
        static void Cube(List<BQuad> m, float x0, float y0, float z0, float x1, float y1, float z1, Color c, float pivotZ = 0, float ang = 0, int tex = 0)
        {
            var p = new V3[8];
            float ca = (float)Math.Cos(ang), sa = (float)Math.Sin(ang);
            for (int i = 0; i < 8; i++)
            {
                float x = (i & 1) == 0 ? x0 : x1, y = (i & 2) == 0 ? y0 : y1, z = (i & 4) == 0 ? z0 : z1;
                float dz = z - pivotZ;
                p[i] = new V3(x, y * ca - dz * sa, pivotZ + y * sa + dz * ca + (curPart == 5 ? 0 : zLift));
            }
            Action<V3, V3, V3, V3, float, int, bool> face = (a, b, cc, d, k, t, mirror) =>
                m.Add(new BQuad { A = a, B = b, C = cc, D = d, Col = Mul(t >= 0 ? Color.White : c, k), Tex = t, Mirror = mirror, Part = curPart });
            face(p[6], p[7], p[5], p[4], 1f, (tex & 1) != 0 ? 3 : -1, false);
            face(p[2], p[3], p[7], p[6], 0.9f, (tex & 2) != 0 ? 0 : -1, false);
            face(p[1], p[0], p[4], p[5], 0.7f, (tex & 4) != 0 ? 2 : -1, false);
            face(p[3], p[1], p[5], p[7], 0.8f, (tex & 8) != 0 ? 1 : -1, false);
            face(p[0], p[2], p[6], p[4], 0.74f, (tex & 8) != 0 ? 1 : -1, true);
            face(p[0], p[1], p[3], p[2], 0.5f, -1, false);
        }

        public static List<BQuad> Build(Look L, int phase) { return Build(L, phase == 0 ? 0 : (phase - 1) / 8.0 * Math.PI * 2, phase != 0); }

        public const float Hip = 0.66f, Shoulder = 1.16f;

        // a = fase da caminhada (radianos); parado, braços e pernas ficam retos
        public static List<BQuad> Build(Look L, double a, bool moving)
        {
            var m = new List<BQuad>();
            curPart = 0;
            bool ride = L.Ride == "patinete";
            if (ride) moving = false;   // no patinete: pés parados no deque e mãos no guidão
            float leg = moving ? (float)Math.Sin(a) * 0.5f : 0, arm = -leg * 0.8f;
            float bob = moving ? Math.Abs((float)Math.Cos(a)) * 0.035f : 0;
            zLift = 0;
            if (ride)
            {
                curPart = 5;
                Scooter(m);
                zLift = RideLift;
            }
            string outfit = L.Outfit ?? "", acc = L.Acc ?? "";
            Color skin = L.Skin, top = TopColor(L), pants = L.Pants, shoe = Art.Hx(0x2A2522), hair = L.Hair;
            bool fem = L.Female, skirt = L.Bottom == 1;
            bool shortSleeve = outfit == "havaiana" || outfit == "polo_noc" || outfit == "camiseta_python" || outfit == "camiseta_php";
            Color legC = skirt ? skin : pants;
            float hip = 0.66f;
            // pernas e sapatos (balançam em volta do quadril)
            foreach (int s in new[] { -1, 1 })
            {
                curPart = s < 0 ? 1 : 2;
                float ang = s < 0 ? leg : -leg;
                float x0 = s < 0 ? -0.19f : 0.03f, x1 = s < 0 ? -0.03f : 0.19f;
                Cube(m, x0, -0.08f, 0.1f, x1, 0.08f, hip, legC, hip, ang);
                Cube(m, x0 - 0.005f, -0.09f, 0, x1 + 0.005f, 0.15f, 0.1f, shoe, hip, ang);
            }
            curPart = 0;
            float tw = fem ? 0.19f : 0.21f;
            // tronco
            if (outfit == "jaleco") Cube(m, -tw - 0.01f, -0.13f, 0.48f + bob, tw + 0.01f, 0.13f, 1.2f + bob, top);
            else Cube(m, -tw, -0.12f, 0.62f + bob, tw, 0.12f, 1.2f + bob, top);
            if (!skirt && outfit != "jaleco") Cube(m, -tw - 0.005f, -0.125f, 0.6f + bob, tw + 0.005f, 0.125f, 0.68f + bob, Art.Dk(pants, 0.25f));
            if (skirt) Cube(m, -0.25f, -0.16f, 0.42f + bob, 0.25f, 0.16f, 0.78f + bob, pants);
            // detalhes da frente da roupa
            if (outfit == "terno") { Cube(m, -0.06f, 0.12f, 0.9f + bob, 0.06f, 0.128f, 1.2f + bob, Color.White); Cube(m, -0.025f, 0.128f, 0.88f + bob, 0.025f, 0.135f, 1.17f + bob, Art.Hx(0xD62828)); }
            if (acc == "gravata") Cube(m, -0.03f, 0.12f, 0.86f + bob, 0.03f, 0.135f, 1.18f + bob, Art.Hx(0xD62828));
            if (acc == "cracha") Cube(m, -0.16f, 0.12f, 0.98f + bob, -0.07f, 0.132f, 1.08f + bob, Art.Hx(0xF5C542));
            if (L.Badge && acc != "cracha") Cube(m, 0.07f, 0.12f, 1.0f + bob, 0.15f, 0.13f, 1.09f + bob, Color.White);
            if (outfit == "moletom" || outfit == "moletom_mic") Cube(m, -0.16f, -0.2f, 1.1f + bob, 0.16f, -0.12f, 1.28f + bob, Art.Dk(top, 0.25f));
            if (acc == "mochila") Cube(m, -0.17f, -0.25f, 0.78f + bob, 0.17f, -0.12f, 1.15f + bob, Art.Hx(0x8B5E3C));
            // braços (balançam ao contrário das pernas)
            foreach (int s in new[] { -1, 1 })
            {
                curPart = s < 0 ? 3 : 4;
                float ang = ride ? RideArm : s < 0 ? arm : -arm;
                float x0 = s < 0 ? -tw - 0.12f : tw, x1 = s < 0 ? -tw : tw + 0.12f;
                float pz = 1.16f + bob;
                if (shortSleeve)
                {
                    Cube(m, x0, -0.065f, 0.98f + bob, x1, 0.065f, 1.19f + bob, top, pz, ang);
                    Cube(m, x0 + 0.01f, -0.055f, 0.7f + bob, x1 - 0.01f, 0.055f, 0.98f + bob, skin, pz, ang);
                }
                else Cube(m, x0, -0.065f, 0.72f + bob, x1, 0.065f, 1.19f + bob, top, pz, ang);
                Cube(m, x0 + 0.01f, -0.055f, 0.6f + bob, x1 - 0.01f, 0.055f, 0.72f + bob, skin, pz, ang);
            }
            curPart = 0;
            // pescoço e cabeça
            Cube(m, -0.06f, -0.05f, 1.19f + bob, 0.06f, 0.05f, 1.25f + bob, Art.Dk(skin, 0.15f));
            float hz0 = 1.24f + bob, hz1 = 1.64f + bob;
            Cube(m, -0.2f, -0.19f, hz0, 0.2f, 0.21f, hz1, skin, 0, 0, 0x0F);
            // cabelo
            int st = L.HairStyle;
            switch (st)
            {
                case 1: Cube(m, -0.21f, -0.22f, 0.98f + bob, 0.21f, -0.17f, hz1, hair); break;
                case 2: Cube(m, -0.08f, -0.22f, hz1 - 0.04f, 0.08f, -0.07f, hz1 + 0.12f, hair); break;
                case 4: Cube(m, -0.27f, -0.29f, hz1 - 0.1f, 0.27f, 0.18f, hz1 + 0.2f, hair); Cube(m, -0.27f, -0.29f, hz0 + 0.04f, 0.27f, -0.12f, hz1 - 0.1f, hair); foreach (int s4 in new[] { -1, 1 }) Cube(m, s4 < 0 ? -0.27f : 0.2f, -0.12f, hz0 + 0.12f, s4 < 0 ? -0.2f : 0.27f, 0.1f, hz1 - 0.1f, hair); break;
                case 5: Cube(m, -0.16f, 0.21f, hz1 - 0.07f, 0.16f, 0.36f, hz1 - 0.04f, Art.Dk(L.Accent, 0.25f)); break;
                case 6: Cube(m, -0.05f, -0.28f, 1.02f + bob, 0.05f, -0.19f, hz1 - 0.08f, hair); Cube(m, -0.06f, -0.29f, hz1 - 0.16f, 0.06f, -0.19f, hz1 - 0.1f, L.Accent); break;
                case 7: foreach (int s in new[] { -1, 1 }) Cube(m, s < 0 ? -0.27f : 0.2f, -0.05f, 0.96f + bob, s < 0 ? -0.2f : 0.27f, 0.05f, hz0 + 0.22f, hair); break;
                case 8: foreach (int s in new[] { -1, 1 }) Cube(m, s < 0 ? -0.24f : 0.2f, -0.2f, hz0 + 0.06f, s < 0 ? -0.2f : 0.24f, 0.16f, hz1, hair); Cube(m, -0.22f, -0.23f, hz0 + 0.06f, 0.22f, -0.19f, hz1, hair); break;
            }
            if (st != 3 && st != 4 && st != 5 && string.IsNullOrEmpty(L.Hat)) Cube(m, -0.21f, -0.2f, hz1 - 0.02f, 0.21f, 0.17f, hz1 + 0.05f, hair, 0, 0, 0x01);
            if (L.Headset) foreach (int s in new[] { -1, 1 }) Cube(m, s < 0 ? -0.24f : 0.2f, -0.05f, hz0 + 0.12f, s < 0 ? -0.2f : 0.24f, 0.07f, hz0 + 0.26f, Art.Hx(0x2B2D42));
            // chapéus da lojinha
            switch (L.Hat ?? "")
            {
                case "bone": Cube(m, -0.215f, -0.2f, hz1 - 0.06f, 0.215f, 0.21f, hz1 + 0.07f, Art.Docker); Cube(m, -0.16f, 0.21f, hz1 - 0.06f, 0.16f, 0.37f, hz1 - 0.03f, Art.Dk(Art.Docker, 0.3f)); break;
                case "bone_react": Cube(m, -0.215f, -0.2f, hz1 - 0.06f, 0.215f, 0.21f, hz1 + 0.07f, Art.Hx(0x20232A)); Cube(m, -0.16f, 0.21f, hz1 - 0.06f, 0.16f, 0.37f, hz1 - 0.03f, Art.Hx(0x61DAFB)); break;
                case "gorro": Cube(m, -0.22f, -0.21f, hz1 - 0.1f, 0.22f, 0.22f, hz1 + 0.12f, Art.Hx(0xE63946)); Cube(m, -0.225f, -0.215f, hz1 - 0.1f, 0.225f, 0.225f, hz1 - 0.04f, Art.Hx(0xF1F5F9)); Cube(m, -0.05f, -0.05f, hz1 + 0.12f, 0.05f, 0.05f, hz1 + 0.2f, Art.Hx(0xF1F5F9)); break;
                case "gorro_python": Cube(m, -0.22f, -0.21f, hz1 - 0.1f, 0.22f, 0.22f, hz1 + 0.12f, Art.Hx(0x3776AB)); Cube(m, -0.225f, -0.215f, hz1 - 0.1f, 0.225f, 0.225f, hz1 - 0.04f, Art.Hx(0xFFD43B)); break;
                case "capacete": Cube(m, -0.24f, -0.23f, hz1 - 0.08f, 0.24f, 0.24f, hz1 + 0.1f, Art.Hx(0xF0B429)); Cube(m, -0.2f, 0.24f, hz1 - 0.08f, 0.2f, 0.32f, hz1 - 0.05f, Art.Hx(0xC98A06)); break;
                case "cowboy": Cube(m, -0.36f, -0.34f, hz1 - 0.02f, 0.36f, 0.38f, hz1 + 0.01f, Art.Hx(0x7A4A24)); Cube(m, -0.18f, -0.16f, hz1 + 0.01f, 0.18f, 0.18f, hz1 + 0.2f, Art.Hx(0x8B5A2B)); break;
                case "coroa": Cube(m, -0.2f, -0.19f, hz1, 0.2f, 0.21f, hz1 + 0.08f, Art.Hx(0xF5C542)); foreach (float sx in new[] { -0.15f, 0, 0.15f }) Cube(m, sx - 0.03f, 0.17f, hz1 + 0.08f, sx + 0.03f, 0.21f, hz1 + 0.15f, Art.Hx(0xF5C542)); break;
                case "boina": Cube(m, -0.24f, -0.22f, hz1 - 0.02f, 0.24f, 0.24f, hz1 + 0.06f, Art.Hx(0xC1121F)); break;
                case "chef": Cube(m, -0.19f, -0.18f, hz1 - 0.04f, 0.19f, 0.2f, hz1 + 0.3f, Art.Hx(0xF8FAFC)); break;
            }
            zLift = 0;
            return m;
        }

        public const float RideLift = 0.13f, RideArm = 0.85f;

        // patinete elétrico: deque, rodas, coluna e guidão (frente para +y)
        static void Scooter(List<BQuad> m)
        {
            Color deck = Art.Hx(0x2496ED), dark = Art.Hx(0x1C1C22), metal = Art.Hx(0xB8C0CC);
            Cube(m, -0.13f, -0.38f, 0.07f, 0.13f, 0.34f, 0.12f, deck);
            Cube(m, -0.11f, -0.36f, 0.12f, 0.11f, 0.3f, 0.125f, dark);
            Cube(m, -0.035f, 0.3f, 0, 0.035f, 0.46f, 0.14f, dark);
            Cube(m, -0.035f, -0.46f, 0, 0.035f, -0.3f, 0.14f, dark);
            Cube(m, -0.03f, 0.36f, 0.12f, 0.03f, 0.42f, 1.07f, metal);
            Cube(m, -0.26f, 0.35f, 1.02f, 0.26f, 0.43f, 1.07f, metal);
            Cube(m, -0.3f, 0.35f, 1.01f, -0.22f, 0.43f, 1.08f, dark);
            Cube(m, 0.22f, 0.35f, 1.01f, 0.3f, 0.43f, 1.08f, dark);
            Cube(m, -0.05f, 0.42f, 0.85f, 0.05f, 0.45f, 0.92f, Art.Hx(0xF5C542));
        }

        // ── desenho no GDI+ (prévia): projeção isométrica igual à do jogo, faces de trás para a frente
        class dynamicQuad { public BQuad Q; public V3 A, B, C, D; }
        static readonly Dictionary<string, Bitmap> faceCache = new Dictionary<string, Bitmap>();

        public static void Draw(Graphics g, Look L, float cx, float fy, float scale, float angle, int phase)
        {
            Draw(g, L, cx, fy, scale, angle, phase == 0 ? 0 : (phase - 1) / 8.0 * Math.PI * 2, phase != 0);
        }

        public static void Draw(Graphics g, Look L, float cx, float fy, float scale, float angle, double walk, bool moving)
        {
            Bitmap face;
            string fk = L.Id.ToString();
            if (!faceCache.TryGetValue(fk, out face)) { if (faceCache.Count > 40) { foreach (var b in faceCache.Values) b.Dispose(); faceCache.Clear(); } face = FaceBitmap(L); faceCache[fk] = face; }
            double yaw = -3 * Math.PI / 4, pit = Math.PI / 6;
            float fxh = (float)Math.Cos(yaw), fyh = (float)Math.Sin(yaw);
            float rx = -fyh, ry = fxh;
            float cp = (float)Math.Cos(pit), sp = (float)Math.Sin(pit);
            double phi = angle - Math.PI / 2;
            float cr = (float)Math.Cos(phi), sr = (float)Math.Sin(phi);
            Func<V3, V3> world = v => new V3(v.X * cr - v.Y * sr, v.X * sr + v.Y * cr, v.Z);
            Func<V3, PointF> scr = v => new PointF(cx + (v.X * rx + v.Y * ry) * scale, fy - (v.Z * cp + (v.X * fxh + v.Y * fyh) * sp) * scale);
            var toCam = new V3(-fxh * cp, -fyh * cp, sp);
            // cada bloco tem 6 faces seguidas: ordena os blocos do mais longe para o mais perto e,
            // dentro de cada bloco, desenha só as faces viradas para a câmera (bloco é convexo: não se sobrepõem)
            var all = Build(L, walk, moving);
            var groups = new List<KeyValuePair<float, List<V3[]>>>();
            var groupQuads = new List<List<BQuad>>();
            for (int i = 0; i + 5 < all.Count; i += 6)
            {
                float depth = 0;
                var faces = new List<BQuad>();
                for (int k = 0; k < 6; k++)
                {
                    var q = all[i + k];
                    foreach (var v in new[] { q.A, q.B, q.C, q.D }) { var w = world(v); depth += (w.X * fxh + w.Y * fyh) * cp - w.Z * sp; }
                    faces.Add(q);
                }
                groups.Add(new KeyValuePair<float, List<V3[]>>(depth / 24, null));
                groupQuads.Add(faces);
            }
            var order = Enumerable.Range(0, groups.Count).OrderByDescending(i => groups[i].Key).ToList();
            var vis = new List<dynamicQuad>();
            foreach (int gi in order)
                foreach (var q in groupQuads[gi])
                {
                    V3 A = world(q.A), B = world(q.B), C = world(q.C), D = world(q.D);
                    // normal para fora = (D - A) × (B - A)
                    float ux = D.X - A.X, uy = D.Y - A.Y, uz = D.Z - A.Z, vx = B.X - A.X, vy = B.Y - A.Y, vz = B.Z - A.Z;
                    float nx = uy * vz - uz * vy, ny = uz * vx - ux * vz, nz = ux * vy - uy * vx;
                    if (nx * toCam.X + ny * toCam.Y + nz * toCam.Z <= 1e-6f) continue;
                    vis.Add(new dynamicQuad { Q = q, A = A, B = B, C = C, D = D });
                }
            var old = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            foreach (var q in vis)
            {
                PointF a = scr(q.A), b = scr(q.B), c = scr(q.C), d = scr(q.D);
                if (q.Q.Tex >= 0)
                {
                    int sx = q.Q.Tex * FaceS;
                    var src = new Rectangle(sx, 0, FaceS, FaceS);
                    float k = q.Q.Col.R / 255f;
                    using (var ia = new ImageAttributes())
                    {
                        ia.SetColorMatrix(new ColorMatrix(new[] { new[] { k, 0, 0, 0, 0f }, new[] { 0, k, 0, 0, 0f }, new[] { 0, 0, k, 0, 0f }, new[] { 0, 0, 0, 1f, 0 }, new[] { 0, 0, 0, 0, 1f } }));
                        ia.SetWrapMode(WrapMode.TileFlipXY);
                        // espelhado: troca os cantos de cima (o lado esquerdo da imagem vai para a direita)
                        g.DrawImage(face, q.Q.Mirror ? new[] { c, d, b } : new[] { d, c, a }, src, GraphicsUnit.Pixel, ia);
                    }
                }
                else
                {
                    // contorno da mesma cor fecha as frestas entre faces vizinhas (bordas suaves sem riscos)
                    var pts = new[] { a, b, c, d };
                    g.FillPolygon(Art.B(q.Q.Col), pts);
                    using (var pe = new Pen(q.Q.Col, 1f) { LineJoin = LineJoin.Round }) g.DrawPolygon(pe, pts);
                }
            }
            g.SmoothingMode = old;
        }
    }
}
