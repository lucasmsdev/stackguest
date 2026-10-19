using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace DockerQuest
{
    // Prévia animada do personagem (gira e anda no lugar).
    class PreviewBox : Control
    {
        public Look Look;
        public Image Picture;   // quando definido, mostra esta imagem (ex.: móvel da sala) em vez do personagem
        readonly Timer timer;
        float t;

        public PreviewBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            timer = new Timer { Interval = 30 };
            timer.Tick += (s, e) => { t += 0.03f; Invalidate(); };
            timer.Start();
        }

        protected override void Dispose(bool disposing) { if (disposing) timer.Dispose(); base.Dispose(disposing); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = ClientRectangle;
            using (var lg = new LinearGradientBrush(r, Art.Hx(0x1F2A3C), Art.Hx(0x0D1117), 90f)) g.FillRectangle(lg, r);
            if (Picture != null)
            {
                float side = Math.Min(r.Width, r.Height) * 0.92f;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(Picture, (r.Width - side) / 2, (r.Height - side) / 2, side, side);
                return;
            }
            if (Look == null) return;
            float u = Math.Min(r.Height * 0.48f, r.Width * 0.62f);
            float cx = r.Width * (Look.Pet != null ? 0.44f : 0.5f), fy = r.Height * 0.84f;
            Art.E(g, Art.A(Art.Docker, 50), cx - u * 0.6f, fy - u * 0.12f, u * 1.2f, u * 0.26f);
            // o mesmo boneco de blocos do jogo, de frente para quem olha, virando um pouco e andando no lugar
            float angle = (float)(Math.PI / 4 + Math.Sin(t * 0.8) * 1.1);
            BlockAvatar.Draw(g, Look, cx, fy, u * 0.62f, angle, t * 7, true);
            int facing = 2;
            if (Look.Pet != null) Art.Pet(g, Look.Pet, cx + u * 0.62f, fy + u * 0.02f, u, facing == 1 ? 1 : 2, t, true);
        }
    }

    // Botão quadradinho de cor ou de texto.
    class Swatch : Control
    {
        public Color Fill = Theme.Border;
        public bool Selected;
        public string Label;

        public Swatch()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI Semibold", 9.5f);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(Parent != null ? Parent.BackColor : Theme.Panel);
            float pad = 3, rad = Math.Min(Width, Height) * 0.22f;
            Art.RR(g, Selected ? Theme.Docker : Art.A(Color.White, 25), 0, 0, Width, Height, rad + 2);
            Art.RR(g, Fill, pad, pad, Width - 2 * pad, Height - 2 * pad, rad);
            if (Label != null)
                Art.Text(g, Label, Font, Selected ? Color.White : Theme.Text, new RectangleF(0, 0, Width, Height));
        }
    }

    // Criação do personagem (primeiro acesso) e guarda-roupa (tecla C).
    class CharacterForm : Form
    {
        readonly GameState gs;
        readonly float scale;
        readonly int[] visual;
        readonly Dictionary<string, string> equip;
        readonly PreviewBox preview;
        readonly TextBox name;
        readonly Panel options;
        readonly List<Action> refreshers = new List<Action>();
        int S(float v) { return (int)Math.Round(v * scale); }

        public CharacterForm(GameState gs, float scale, bool creating)
        {
            this.gs = gs; this.scale = scale;
            visual = new int[8];
            var cur = gs.Prog.Visual;
            for (int i = 0; i < visual.Length; i++) visual[i] = i < cur.Length ? cur[i] : (i == 7 ? 2 : 0);
            equip = new Dictionary<string, string>(gs.Prog.Equip);

            Text = creating ? "Stack Quest — crie seu personagem" : "Stack Quest — guarda-roupa";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            BackColor = Theme.Panel;
            ForeColor = Theme.Text;
            var wa = Screen.PrimaryScreen.WorkingArea;
            ClientSize = new Size(Math.Min(S(940), wa.Width - 40), Math.Min(S(680), wa.Height - 60));

            var title = new Label
            {
                Text = creating ? "Boas-vindas à Micorosoft! Crie seu personagem" : "Guarda-roupa",
                Font = new Font("Segoe UI Black", 15f), ForeColor = Theme.Docker, AutoSize = true, Location = new Point(S(22), S(16))
            };
            var sub = new Label
            {
                Text = creating ? "É seu primeiro dia de estágio na Micorosoft. Escolha seu visual — dá para mudar depois com a tecla C."
                                : "Mude seu visual e escolha quais itens da lojinha usar.",
                Font = new Font("Segoe UI", 10f), ForeColor = Theme.Muted, AutoSize = true, Location = new Point(S(24), S(52))
            };

            preview = new PreviewBox { Location = new Point(S(22), S(88)), Size = new Size(S(300), ClientSize.Height - S(88) - S(130)) };
            var lblName = new Label { Text = "Seu nome:", Font = new Font("Segoe UI Semibold", 10f), AutoSize = true, Location = new Point(S(22), preview.Bottom + S(12)) };
            name = new TextBox
            {
                Font = new Font("Segoe UI", 13f), MaxLength = 18, Location = new Point(S(22), lblName.Bottom + S(4)), Width = S(300),
                BackColor = Theme.Term, ForeColor = Theme.Bright, BorderStyle = BorderStyle.FixedSingle, Text = gs.Prog.PlayerName
            };

            options = new Panel { Location = new Point(S(344), S(88)), Size = new Size(ClientSize.Width - S(344) - S(16), ClientSize.Height - S(88) - S(76)), AutoScroll = true, BackColor = Theme.Panel };
            Program.DarkScroll(options);

            var ok = new Button
            {
                Text = creating ? "Começar  ►" : "Salvar  ✔", Font = new Font("Segoe UI Semibold", 11.5f), FlatStyle = FlatStyle.Flat,
                BackColor = Theme.Docker, ForeColor = Color.White, Size = new Size(S(190), S(46)),
                Location = new Point(ClientSize.Width - S(206), ClientSize.Height - S(62)), Cursor = Cursors.Hand
            };
            ok.FlatAppearance.BorderSize = 0;
            ok.Click += (s, e) => Confirm();
            Controls.AddRange(new Control[] { title, sub, preview, lblName, name, options, ok });
            if (creating)
            {
                var logo = new Panel { Size = new Size(S(250), S(54)), Location = new Point(ClientSize.Width - S(266), S(14)) };
                logo.Paint += (s, e) => { e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; Art.Wordmark(e.Graphics, 0, 0, S(50), Color.White); };
                Controls.Add(logo);
            }
            AcceptButton = ok;
            if (!creating)
            {
                var cancel = new Button
                {
                    Text = "Cancelar", Font = new Font("Segoe UI Semibold", 11f), FlatStyle = FlatStyle.Flat, BackColor = Theme.Border, ForeColor = Theme.Text,
                    Size = new Size(S(140), S(46)), Location = new Point(ok.Left - S(152), ok.Top), DialogResult = DialogResult.Cancel, Cursor = Cursors.Hand
                };
                cancel.FlatAppearance.BorderSize = 0;
                Controls.Add(cancel);
                CancelButton = cancel;
            }
            BuildOptions();
            UpdatePreview();
        }

        void UpdatePreview()
        {
            preview.Look = Catalog.MakeLook(visual, equip, null);
            foreach (var r in refreshers) r();
        }

        int Section(int y, string title)
        {
            options.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI Semibold", 10.5f), ForeColor = Theme.Bright, AutoSize = true, Location = new Point(S(2), y) });
            return y + S(28);
        }

        int ColorRow(int y, string title, int[] colors, int idx)
        {
            y = Section(y, title);
            var list = new List<Swatch>();
            for (int i = 0; i < colors.Length; i++)
            {
                int k = i;
                var sw = new Swatch { Fill = Art.Hx(colors[i]), Size = new Size(S(38), S(38)), Location = new Point(S(2) + i * S(44), y) };
                sw.Click += (s, e) => { visual[idx] = k; UpdatePreview(); };
                list.Add(sw);
                options.Controls.Add(sw);
            }
            refreshers.Add(() => { for (int i = 0; i < list.Count; i++) { list[i].Selected = visual[idx] == i; list[i].Invalidate(); } });
            return y + S(52);
        }

        int TextRow(int y, string title, string[] labels, Func<int> get, Action<int> set)
        {
            y = Section(y, title);
            var list = new List<Swatch>();
            int x = S(2);
            using (var g = CreateGraphics())
                for (int i = 0; i < labels.Length; i++)
                {
                    int k = i;
                    var sw = new Swatch { Label = labels[i], Fill = Theme.Term };
                    int w = (int)g.MeasureString(labels[i], sw.Font).Width + S(26);
                    if (x + w > options.Width - S(24)) { x = S(2); y += S(44); }
                    sw.Size = new Size(w, S(36));
                    sw.Location = new Point(x, y);
                    x += w + S(6);
                    sw.Click += (s, e) => { set(k); UpdatePreview(); };
                    list.Add(sw);
                    options.Controls.Add(sw);
                }
            refreshers.Add(() => { for (int i = 0; i < list.Count; i++) { list[i].Selected = get() == i; list[i].Invalidate(); } });
            return y + S(50);
        }

        void BuildOptions()
        {
            int y = 0;
            y = ColorRow(y, "Tom de pele", Catalog.Skins, 0);
            y = TextRow(y, "Corpo", new[] { "Masculino", "Feminino" }, () => visual[5], v => { visual[5] = v; visual[7] = v; if (v == 0) visual[6] = 0; });
            y = TextRow(y, "Cabelo", Catalog.HairStyles, () => visual[1], v => visual[1] = v);
            y = ColorRow(y, "Cor do cabelo", Catalog.HairColors, 2);
            y = ColorRow(y, "Cor da camisa", Catalog.Shirts, 3);
            y = TextRow(y, "Parte de baixo", new[] { "Calça", "Saia" }, () => visual[6], v => visual[6] = v);
            y = ColorRow(y, "Cor da calça / saia", Catalog.Pants, 4);
            y = TextRow(y, "Como os textos falam com você", new[] { "No masculino (o)", "No feminino (a)", "Neutro (o/a)" }, () => visual[7], v => visual[7] = v);

            for (int si = 0; si < 4; si++)
            {
                string slot = Catalog.Slots[si];
                var owned = Catalog.Items.Where(it => it.Slot == slot && gs.Prog.Owned.Contains(it.Id)).ToList();
                var labels = new List<string> { "Nenhum" };
                labels.AddRange(owned.Select(it => it.Name));
                if (owned.Count == 0)
                {
                    y = Section(y, Catalog.SlotNames[si] + " da lojinha");
                    options.Controls.Add(new Label { Text = "Você ainda não tem itens deste tipo. Compre na Lojinha do corredor!", Font = new Font("Segoe UI", 9.5f), ForeColor = Theme.Muted, AutoSize = true, Location = new Point(S(4), y) });
                    y += S(34);
                    continue;
                }
                y = TextRow(y, Catalog.SlotNames[si] + " da lojinha", labels.ToArray(),
                    () => { string id; if (!equip.TryGetValue(slot, out id)) return 0; int ix = owned.FindIndex(it => it.Id == id); return ix + 1; },
                    v => { if (v == 0) equip.Remove(slot); else equip[slot] = owned[v - 1].Id; });
            }
        }

        void Confirm()
        {
            string n = name.Text.Trim();
            Gender.Mode = visual[7];
            gs.Prog.PlayerName = n.Length > 0 ? n : Gender.Apply("Estagiário(a)");
            gs.Prog.Visual = visual;
            gs.Prog.HasVisual = true;
            gs.Prog.Equip.Clear();
            foreach (var kv in equip) gs.Prog.Equip[kv.Key] = kv.Value;
            gs.Prog.Save();
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    // Badges: uma por nível (Iniciante/Intermediário/Profissional) de cada área, com o nome do aluno.
    // Modo prêmio (badge recém-conquistada) ou galeria (todas as 24); dá para baixar como PNG.
    class BadgeForm : Form
    {
        readonly GameState gs;
        readonly float scale;
        readonly Panel canvas;
        readonly Button btnSave, btnToggle, btnOk;
        string selCat;
        int selTier;
        bool gallery;
        float t;
        int S(float v) { return (int)Math.Round(v * scale); }

        public BadgeForm(GameState gs, float scale, string cat, int tier)
        {
            this.gs = gs; this.scale = scale;
            selCat = cat; selTier = tier;
            gallery = cat == null;
            Text = "Stack Quest — badges";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            BackColor = Theme.Bg;
            var wa = Screen.PrimaryScreen.WorkingArea;
            ClientSize = new Size(Math.Min(S(1000), wa.Width - 40), Math.Min(S(720), wa.Height - 60));

            canvas = new DoubleBufferedPanel { Location = new Point(0, 0), Size = new Size(ClientSize.Width, ClientSize.Height - S(70)), Cursor = Cursors.Default };
            canvas.Paint += (s, e) => Paint2(e.Graphics, canvas.ClientRectangle);
            canvas.MouseClick += CanvasClick;
            canvas.MouseMove += (s, e) => canvas.Cursor = gallery && HitCell(e.Location) != null ? Cursors.Hand : Cursors.Default;
            var timer = new Timer { Interval = 40 };
            timer.Tick += (s, e) => { t += 0.04f; if (!gallery) canvas.Invalidate(); };
            timer.Start();
            FormClosed += (s, e) => timer.Dispose();

            btnOk = new Button
            {
                Text = "Continuar  ►", Font = new Font("Segoe UI Semibold", 11.5f), FlatStyle = FlatStyle.Flat,
                BackColor = Theme.Docker, ForeColor = Color.White, Size = new Size(S(200), S(46)), Cursor = Cursors.Hand,
                Location = new Point(ClientSize.Width - S(220), ClientSize.Height - S(60)), DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnSave = new Button
            {
                Text = "Baixar badge (PNG)", Font = new Font("Segoe UI Semibold", 10.5f), FlatStyle = FlatStyle.Flat,
                BackColor = Art.Hx(0x238636), ForeColor = Color.White, Size = new Size(S(220), S(46)), Cursor = Cursors.Hand,
                Location = new Point(S(20), ClientSize.Height - S(60))
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => SavePng();
            btnToggle = new Button
            {
                Text = "Ver todas as badges", Font = new Font("Segoe UI Semibold", 10.5f), FlatStyle = FlatStyle.Flat,
                BackColor = Theme.Border, ForeColor = Theme.Text, Size = new Size(S(220), S(46)), Cursor = Cursors.Hand,
                Location = new Point(S(252), ClientSize.Height - S(60))
            };
            btnToggle.FlatAppearance.BorderSize = 0;
            btnToggle.Click += (s, e) => { gallery = !gallery; Sync(); };
            Controls.AddRange(new Control[] { canvas, btnOk, btnSave, btnToggle });
            AcceptButton = btnOk;
            Sync();
            if (!gallery) Sfx.Play("ach");
        }

        void Sync()
        {
            btnSave.Visible = !gallery;
            btnToggle.Visible = !gallery || selCat != null;
            btnToggle.Location = new Point(gallery ? S(20) : S(252), btnToggle.Top);
            btnToggle.Text = gallery ? "Voltar à badge selecionada" : "Ver todas as badges";
            btnOk.Text = gallery ? "Fechar" : "Continuar  ►";
            canvas.Invalidate();
        }

        class DoubleBufferedPanel : Panel { public DoubleBufferedPanel() { DoubleBuffered = true; } }

        static readonly Dictionary<string, Font> fonts = new Dictionary<string, Font>();
        static Font PF(string n, float px, FontStyle st = FontStyle.Regular)
        {
            px = Math.Max(4, (float)Math.Round(px * 2) / 2f);
            string k = n + px + st;
            Font f;
            if (!fonts.TryGetValue(k, out f)) { f = new Font(n, px, st, GraphicsUnit.Pixel); fonts[k] = f; }
            return f;
        }

        string StudentName { get { return string.IsNullOrEmpty(gs.Prog.PlayerName) ? "Estagiário(a)" : gs.Prog.PlayerName; } }

        string DateOf(string cat, int tier)
        {
            string d;
            return gs.Prog.BadgeDates.TryGetValue(cat + ":" + tier, out d) ? d : DateTime.Now.ToString("dd/MM/yyyy");
        }

        // ─────────────────────────── desenho ───────────────────────────

        void Paint2(Graphics g, Rectangle r)
        {
            if (gallery) DrawGallery(g, r);
            else DrawCard(g, r, Challenges.Cat(selCat), selTier, StudentName, DateOf(selCat, selTier), t);
        }

        RectangleF CellRect(Rectangle r, int ci, int tier)
        {
            float top = S(84), cw = (r.Width - S(40)) / (float)Challenges.Cats.Length, chh = (r.Height - top - S(16)) / 3f;
            return new RectangleF(S(20) + ci * cw, top + tier * chh, cw, chh);
        }

        KeyValuePair<string, int>? HitCell(Point p)
        {
            var r = canvas.ClientRectangle;
            for (int ci = 0; ci < Challenges.Cats.Length; ci++)
                for (int tier = 0; tier < 3; tier++)
                    if (CellRect(r, ci, tier).Contains(p) && gs.Prog.BadgeDates.ContainsKey(Challenges.Cats[ci].Id + ":" + tier))
                        return new KeyValuePair<string, int>(Challenges.Cats[ci].Id, tier);
            return null;
        }

        void CanvasClick(object sender, MouseEventArgs e)
        {
            if (!gallery) return;
            var h = HitCell(e.Location);
            if (h == null) return;
            selCat = h.Value.Key; selTier = h.Value.Value;
            gallery = false;
            Sfx.Play("ok");
            Sync();
        }

        void DrawGallery(Graphics g, Rectangle r)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            using (var lg = new LinearGradientBrush(r, Art.Hx(0x1B2433), Art.Hx(0x0D1117), 90f)) g.FillRectangle(lg, r);
            int n = gs.Prog.BadgeDates.Count;
            g.DrawString("SUAS BADGES", PF("Segoe UI Black", S(24)), Art.B(Art.Hx(0xF5C542)), S(20), S(16));
            g.DrawString(n + " de " + Challenges.Cats.Length * 3 + " conquistadas  ·  " + StudentName + "  ·  " + Career.Title(gs.Prog), PF("Segoe UI", S(14)), Art.B(Theme.Text), S(22), S(52));
            string hint = "Clique numa badge conquistada para ver e baixar";
            var hs = g.MeasureString(hint, PF("Segoe UI", S(13)));
            g.DrawString(hint, PF("Segoe UI", S(13)), Art.B(Theme.Muted), r.Width - S(20) - hs.Width, S(54));
            for (int ci = 0; ci < Challenges.Cats.Length; ci++)
            {
                var cat = Challenges.Cats[ci];
                for (int tier = 0; tier < 3; tier++)
                {
                    var cr = CellRect(r, ci, tier);
                    bool has = gs.Prog.BadgeDates.ContainsKey(cat.Id + ":" + tier);
                    float R = Math.Min(cr.Width * 0.4f, cr.Height * 0.3f);
                    float cx = cr.X + cr.Width / 2, cy = cr.Y + R + S(6);
                    DrawBadge(g, cx, cy, R, cat, tier, null, has);
                    string cap = has ? DateOf(cat.Id, tier) : gs.TierCount(cat.Id, tier) + "/" + gs.TierLevels(cat.Id, tier).Count;
                    var f = PF("Segoe UI", S(11));
                    var cs = g.MeasureString(cap, f);
                    g.DrawString(cap, f, Art.B(has ? Theme.Text : Theme.Muted), cx - cs.Width / 2, cy + R * 1.45f);
                }
            }
        }

        static readonly int[][] Metal = { new[] { 0xF3C08E, 0xCD7F32, 0x6E4219 }, new[] { 0xFFFFFF, 0xC3C8D0, 0x666D78 }, new[] { 0xFFF4B8, 0xF5C542, 0x8F6406 } };
        public static Color MetalColor(int tier) { return Art.Hx(Metal[Math.Max(0, Math.Min(2, tier))][1]); }

        static Color Gray(Color c)
        {
            int l = (int)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);
            int v = (int)(l * 0.35 + 34);
            return Color.FromArgb(c.A, v, v, v + 4);
        }

        static PointF[] Star(float cx, float cy, float R, int n, float ratio)
        {
            var pts = new PointF[n * 2];
            for (int k = 0; k < n * 2; k++)
            {
                double a = -Math.PI / 2 + k * Math.PI / n;
                float rr = k % 2 == 0 ? R : R * ratio;
                pts[k] = new PointF(cx + (float)Math.Cos(a) * rr, cy + (float)Math.Sin(a) * rr);
            }
            return pts;
        }

        static void CenterText(Graphics g, string s, Font f, Color c, float cx, float cy)
        {
            var sz = g.MeasureString(s, f);
            g.DrawString(s, f, Art.B(c), cx - sz.Width / 2, cy - sz.Height / 2);
        }

        public static void DrawBadge(Graphics g, float cx, float cy, float R, Category cat, int tier, string name, bool earned)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            Func<Color, Color> col = c => earned ? c : Gray(c);
            Color light = col(Art.Hx(Metal[tier][0])), mid = col(Art.Hx(Metal[tier][1])), dark = col(Art.Hx(Metal[tier][2]));
            Color cc = col(cat.Color), ccDark = col(Art.Dk(cat.Color, 0.45f)), ccLight = col(Art.Lt(cat.Color, 0.25f));

            // fitas
            float tw = R * 0.42f, y0 = cy + R * 0.45f, y1 = cy + R * 1.42f;
            for (int s = -1; s <= 1; s += 2)
            {
                float a = s < 0 ? cx - R * 0.08f - tw : cx + R * 0.08f, dx = s * R * 0.28f;
                var rib = new[] { new PointF(a, y0), new PointF(a + tw, y0), new PointF(a + tw + dx, y1), new PointF(a + tw / 2 + dx, y1 - tw * 0.45f), new PointF(a + dx, y1) };
                g.FillPolygon(Art.B(ccDark), rib);
                using (var p = new Pen(Art.A(Color.Black, 60), Math.Max(1, R * 0.015f))) g.DrawPolygon(p, rib);
                g.FillRectangle(Art.B(Art.A(light, 140)), Math.Min(a, a + dx) + tw * 0.35f + (s < 0 ? dx * 0.2f : dx * 0.6f), y0, tw * 0.14f, (y1 - y0) * 0.75f);
            }

            // sombra
            g.FillEllipse(Art.B(Art.A(Color.Black, 70)), cx - R, cy - R + R * 0.08f, R * 2, R * 2);
            // borda metálica (mais pontas = nível mais alto)
            var outer = tier == 0 ? Star(cx, cy, R, 36, 0.96f) : tier == 1 ? Star(cx, cy, R, 16, 0.9f) : Star(cx, cy, R * 1.04f, 12, 0.8f);
            using (var lg = new LinearGradientBrush(new RectangleF(cx - R, cy - R, R * 2, R * 2), light, dark, 55f))
            {
                lg.SetBlendTriangularShape(0.35f, 1f);
                g.FillPolygon(lg, outer);
            }
            using (var p = new Pen(dark, Math.Max(1, R * 0.02f))) g.DrawPolygon(p, outer);
            float r2 = R * 0.82f;
            using (var lg = new LinearGradientBrush(new RectangleF(cx - r2, cy - r2, r2 * 2, r2 * 2), dark, light, 55f))
                g.FillEllipse(lg, cx - r2, cy - r2, r2 * 2, r2 * 2);
            float r3 = R * 0.72f;
            using (var lg = new LinearGradientBrush(new RectangleF(cx - r3, cy - r3, r3 * 2, r3 * 2), ccLight, ccDark, 90f))
                g.FillEllipse(lg, cx - r3, cy - r3, r3 * 2, r3 * 2);
            using (var p = new Pen(Art.A(Color.White, earned ? 90 : 40), Math.Max(1, R * 0.012f)))
                g.DrawEllipse(p, cx - r3 * 0.92f, cy - r3 * 0.92f, r3 * 1.84f, r3 * 1.84f);
            // brilho
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(cx - r3, cy - r3, r3 * 2, r3 * 2);
                var clip = g.Clip;
                g.SetClip(path, CombineMode.Intersect);
                g.FillEllipse(Art.B(Art.A(Color.White, earned ? 28 : 10)), cx - r3 * 1.4f, cy - r3 * 1.9f, r3 * 2.4f, r3 * 1.8f);
                g.Clip = clip;
            }

            Color ink = earned ? Color.White : Art.Hx(0x8B949E);
            CenterText(g, Challenges.TierNames[tier].ToUpperInvariant(), PF("Segoe UI Black", R * 0.1f), earned ? light : ink, cx, cy - R * 0.5f);
            DrawIcon(g, cat.Id, cx, cy - R * 0.1f, R * 0.34f, ink, cc);
            CenterText(g, cat.Name, PF("Segoe UI Black", R * (cat.Name.Length > 8 ? 0.11f : 0.14f)), ink, cx, cy + R * 0.3f);
            string stars = new string('★', tier + 1);
            CenterText(g, stars, PF("Segoe UI Symbol", R * 0.12f, FontStyle.Bold), earned ? light : ink, cx, cy + R * 0.48f);

            if (name != null)
            {
                float bw = R * 2.1f, bh = R * 0.34f, by = cy + R * 0.78f, bx = cx - bw / 2, ear = bh * 0.55f;
                var left = new[] { new PointF(bx - ear, by + bh * 0.2f), new PointF(bx + ear, by + bh * 0.2f), new PointF(bx + ear, by + bh * 1.2f), new PointF(bx - ear, by + bh * 1.2f), new PointF(bx - ear * 0.45f, by + bh * 0.7f) };
                var right = new[] { new PointF(bx + bw + ear, by + bh * 0.2f), new PointF(bx + bw - ear, by + bh * 0.2f), new PointF(bx + bw - ear, by + bh * 1.2f), new PointF(bx + bw + ear, by + bh * 1.2f), new PointF(bx + bw + ear * 0.45f, by + bh * 0.7f) };
                g.FillPolygon(Art.B(Art.Dk(ccDark, 0.3f)), left);
                g.FillPolygon(Art.B(Art.Dk(ccDark, 0.3f)), right);
                using (var lg = new LinearGradientBrush(new RectangleF(bx, by, bw, bh), cc, ccDark, 90f)) g.FillRectangle(lg, bx, by, bw, bh);
                using (var p = new Pen(mid, Math.Max(1, R * 0.02f))) g.DrawRectangle(p, bx, by, bw, bh);
                float fs = R * 0.17f;
                Font nf = PF("Segoe UI Black", fs);
                while (fs > 6 && g.MeasureString(name, nf).Width > bw - R * 0.12f) { fs -= 1; nf = PF("Segoe UI Black", fs); }
                CenterText(g, name, nf, Color.White, cx, by + bh / 2 + R * 0.005f);
            }
        }

        static void DrawIcon(Graphics g, string id, float cx, float cy, float s, Color ink, Color catColor)
        {
            switch (id)
            {
                case "docker":
                    Art.Whale(g, cx, cy, s * 1.5f, ink);
                    break;
                case "react":
                    using (var p = new Pen(ink, Math.Max(1, s * 0.07f)))
                    {
                        var st = g.Save();
                        g.TranslateTransform(cx, cy);
                        for (int k = 0; k < 3; k++)
                        {
                            g.DrawEllipse(p, -s * 0.85f, -s * 0.32f, s * 1.7f, s * 0.64f);
                            g.RotateTransform(60);
                        }
                        g.Restore(st);
                    }
                    g.FillEllipse(Art.B(ink), cx - s * 0.13f, cy - s * 0.13f, s * 0.26f, s * 0.26f);
                    break;
                case "redes":
                    {
                        var pts = new[] { new PointF(cx, cy - s * 0.6f), new PointF(cx - s * 0.7f, cy + s * 0.45f), new PointF(cx + s * 0.7f, cy + s * 0.45f), new PointF(cx, cy + s * 0.05f) };
                        using (var p = new Pen(ink, Math.Max(1, s * 0.08f)))
                        {
                            for (int k = 0; k < 3; k++) g.DrawLine(p, pts[k], pts[3]);
                            g.DrawLine(p, pts[0], pts[1]); g.DrawLine(p, pts[1], pts[2]); g.DrawLine(p, pts[2], pts[0]);
                        }
                        foreach (var q in pts) g.FillEllipse(Art.B(ink), q.X - s * 0.17f, q.Y - s * 0.17f, s * 0.34f, s * 0.34f);
                        g.FillEllipse(Art.B(catColor), pts[3].X - s * 0.08f, pts[3].Y - s * 0.08f, s * 0.16f, s * 0.16f);
                    }
                    break;
                case "flutter":
                    {
                        var a = new[] { new PointF(cx + s * 0.25f, cy - s * 0.75f), new PointF(cx + s * 0.7f, cy - s * 0.75f), new PointF(cx - s * 0.25f, cy + s * 0.2f), new PointF(cx - s * 0.48f, cy - s * 0.03f) };
                        var b = new[] { new PointF(cx + s * 0.25f, cy - s * 0.05f), new PointF(cx + s * 0.7f, cy - s * 0.05f), new PointF(cx + s * 0.15f, cy + s * 0.5f), new PointF(cx + s * 0.7f, cy + s * 1.0f) };
                        g.FillPolygon(Art.B(ink), a);
                        g.FillPolygon(Art.B(ink), new[] { b[0], b[1], new PointF(cx - s * 0.05f, cy + s * 0.7f), new PointF(cx - s * 0.28f, cy + s * 0.47f) });
                        g.FillPolygon(Art.B(Art.A(ink, 170)), new[] { new PointF(cx - s * 0.05f, cy + s * 0.7f), new PointF(cx + s * 0.18f, cy + s * 0.47f), b[3], new PointF(cx + s * 0.25f, cy + s * 1.0f) });
                    }
                    break;
                case "aws":
                    CenterText(g, "aws", PF("Segoe UI Black", s * 0.75f), ink, cx, cy - s * 0.12f);
                    using (var p = new Pen(ink, Math.Max(1, s * 0.09f)) { StartCap = LineCap.Round, EndCap = LineCap.ArrowAnchor })
                        g.DrawArc(p, cx - s * 0.6f, cy - s * 0.25f, s * 1.2f, s * 0.6f, 25, 130);
                    break;
                case "sql":
                    {
                        // banco de dados: cilindro com faixas
                        float w = s * 1.15f, eh = s * 0.36f, top = cy - s * 0.62f, bot = cy + s * 0.5f;
                        using (var p = new Pen(ink, Math.Max(1, s * 0.09f)))
                        {
                            g.DrawEllipse(p, cx - w / 2, top, w, eh);
                            g.DrawLine(p, cx - w / 2, top + eh / 2, cx - w / 2, bot);
                            g.DrawLine(p, cx + w / 2, top + eh / 2, cx + w / 2, bot);
                            g.DrawArc(p, cx - w / 2, bot - eh / 2, w, eh, 0, 180);
                            g.DrawArc(p, cx - w / 2, top + (bot - top) * 0.36f, w, eh, 0, 180);
                            g.DrawArc(p, cx - w / 2, top + (bot - top) * 0.68f, w, eh, 0, 180);
                        }
                    }
                    break;
                case "python":
                    CenterText(g, "Py", PF("Segoe UI Black", s * 0.95f), ink, cx, cy);
                    break;
                case "php":
                    CenterText(g, "php", PF("Segoe UI Black", s * 0.8f, FontStyle.Italic), ink, cx, cy);
                    break;
                default:
                    CenterText(g, "</>", PF("Consolas", s * 0.85f, FontStyle.Bold), ink, cx, cy);
                    break;
            }
        }

        // cartão completo: badge + texto (tela de prêmio e PNG baixado)
        public static void DrawCard(Graphics g, Rectangle r, Category cat, int tier, string name, string date, float anim)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            Color metal = MetalColor(tier), light = Art.Hx(Metal[tier][0]);
            using (var lg = new LinearGradientBrush(r, Art.Hx(0x1B2433), Art.Hx(0x0D1117), 90f)) g.FillRectangle(lg, r);
            using (var rg = new GraphicsPath())
            {
                float gr = Math.Min(r.Width, r.Height) * 0.55f, gcx = r.X + r.Width / 2f, gcy = r.Y + r.Height * 0.4f;
                rg.AddEllipse(gcx - gr, gcy - gr, gr * 2, gr * 2);
                using (var pb = new PathGradientBrush(rg) { CenterColor = Art.A(cat.Color, 70), SurroundColors = new[] { Art.A(cat.Color, 0) } })
                    g.FillPath(pb, rg);
            }
            float u = Math.Min(r.Width, r.Height) / 100f;
            if (anim >= 0)
            {
                var rnd = new Random(11);
                var cols = new[] { 0xF5C542, 0x2496ED, 0x3FB950, 0xE63946, 0x8E6CC4, 0xFF9900 };
                for (int i = 0; i < 46; i++)
                {
                    float x = r.X + (float)rnd.NextDouble() * r.Width, sp = 20 + (float)rnd.NextDouble() * 40;
                    float y = r.Y + ((float)rnd.NextDouble() * r.Height + anim * sp * u) % r.Height;
                    Art.R(g, Art.A(Art.Hx(cols[i % cols.Length]), 160), x, y, u * 1.1f, u * 0.55f);
                }
            }
            Art.RRO(g, metal, Math.Max(2, u * 0.5f), r.X + u * 2, r.Y + u * 2, r.Width - u * 4, r.Height - u * 4, u * 2);
            Art.RRO(g, Art.A(metal, 110), Math.Max(1, u * 0.2f), r.X + u * 3.4f, r.Y + u * 3.4f, r.Width - u * 6.8f, r.Height - u * 6.8f, u * 1.4f);

            float cx = r.X + r.Width / 2f;
            // logo do jogo
            float lh = u * 5.2f;
            var lf = PF("Segoe UI Black", u * 3.4f);
            float lw = lh * 1.2f + g.MeasureString("STACK QUEST", lf).Width;
            float lx = cx - lw / 2, ly = r.Y + u * 5.2f;
            Art.Logo(g, lx, ly, lh);
            g.DrawString("STACK QUEST", lf, Art.B(Color.White), lx + lh * 1.2f, ly + (lh - g.MeasureString("STACK QUEST", lf).Height) / 2);
            CenterText(g, "BADGE OFICIAL", PF("Segoe UI Black", u * 2.1f), light, cx, r.Y + u * 13.5f);

            float R = Math.Min(r.Width * 0.2f, r.Height * 0.2f);
            float by = r.Y + r.Height * 0.4f;
            float bob = anim >= 0 ? (float)Math.Sin(anim * 2) * u * 0.6f : 0;
            DrawBadge(g, cx, by + bob, R, cat, tier, name, true);

            float ty = by + R * 1.55f;
            CenterText(g, "concedida a", PF("Segoe UI", u * 2.6f), Theme.Muted, cx, ty);
            float fs = u * 6.5f;
            Font nf = PF("Segoe UI Black", fs);
            while (fs > 8 && g.MeasureString(name, nf).Width > r.Width - u * 12) { fs -= 1; nf = PF("Segoe UI Black", fs); }
            CenterText(g, name, nf, Color.White, cx, ty + u * 5.5f);
            CenterText(g, "por concluir o nível " + Challenges.TierNames[tier] + " de " + cat.Name, PF("Segoe UI Semibold", u * 3f), Theme.Text, cx, ty + u * 11.5f);
            CenterText(g, "Stack Quest · emitida em " + date, PF("Segoe UI", u * 2.4f), Theme.Muted, cx, ty + u * 16f);
        }

        void SavePng()
        {
            if (selCat == null) return;
            var cat = Challenges.Cat(selCat);
            string safe = new string(Checker.Fold(StudentName).Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray()).Trim('-');
            string file = "badge-" + cat.Id + "-" + Challenges.TierNames[selTier].ToLowerInvariant() + (safe.Length > 0 ? "-" + safe : "") + ".png";
            file = new string(Checker.Fold(file).Select(ch => char.IsLetterOrDigit(ch) || ch == '.' || ch == '-' ? ch : '-').ToArray());
            using (var dlg = new SaveFileDialog { Filter = "Imagem PNG|*.png", FileName = file })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                using (var bmp = new Bitmap(900, 1000))
                {
                    using (var g = Graphics.FromImage(bmp)) DrawCard(g, new Rectangle(0, 0, bmp.Width, bmp.Height), cat, selTier, StudentName, DateOf(selCat, selTier), -1);
                    bmp.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
                }
                MessageBox.Show(this, "Badge salva em:\n" + dlg.FileName, "Stack Quest", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

#if DEMO
        public static void ExportDemo(string path, string cat, int tier, string name)
        {
            using (var bmp = new Bitmap(900, 1000))
            {
                using (var g = Graphics.FromImage(bmp)) DrawCard(g, new Rectangle(0, 0, bmp.Width, bmp.Height), Challenges.Cat(cat), tier, name, "02/10/2026", -1);
                bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
#endif
    }

    // Lojinha: compra e equipa itens com o saldo de pontos.
    class ShopForm : Form
    {
        readonly GameState gs;
        readonly float scale;
        readonly PreviewBox preview;
        readonly ListBox list;
        readonly Label lblWallet, lblName, lblDesc, lblInfo;
        readonly Button btnAction;
        readonly List<Swatch> tabs = new List<Swatch>();
        readonly Font fName, fDesc, fPrice;
        string slot = "hat";
        readonly Func<string, int, Bitmap> roomIcon;   // ícone 3D dos móveis da sala (vem do jogo)
        int S(float v) { return (int)Math.Round(v * scale); }

        public ShopForm(GameState gs, float scale, Func<string, int, Bitmap> roomIcon = null)
        {
            this.gs = gs; this.scale = scale; this.roomIcon = roomIcon;
            Text = "Stack Quest — Lojinha do corredor";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            BackColor = Theme.Panel;
            ForeColor = Theme.Text;
            var wa = Screen.PrimaryScreen.WorkingArea;
            ClientSize = new Size(Math.Min(S(1000), wa.Width - 40), Math.Min(S(660), wa.Height - 60));
            fName = new Font("Segoe UI Semibold", 11f);
            fDesc = new Font("Segoe UI", 9f);
            fPrice = new Font("Segoe UI Black", 10.5f);

            var title = new Label { Text = "LOJINHA DA MICOROSOFT", Font = new Font("Segoe UI Black", 16f), ForeColor = Theme.Docker, AutoSize = true, Location = new Point(S(22), S(14)) };
            lblWallet = new Label { Font = new Font("Segoe UI Black", 14f), ForeColor = Theme.Yellow, AutoSize = true, Location = new Point(S(22), S(54)) };
            var note = new Label
            {
                Text = "As moedas vêm dos desafios e dos bônus dos objetivos. Gastar NÃO diminui sua pontuação do ranking.",
                Font = new Font("Segoe UI", 9.5f), ForeColor = Theme.Muted, AutoSize = true, Location = new Point(S(24), S(88))
            };

            int x = S(22);
            for (int i = 0; i < Catalog.Slots.Length; i++)
            {
                string s = Catalog.Slots[i];
                var tab = new Swatch { Label = Catalog.SlotNames[i], Fill = Theme.Term, Size = new Size(S(116), S(38)), Location = new Point(x, S(118)) };
                tab.Click += (o, e) => { slot = s; Fill(); };
                tabs.Add(tab);
                x += S(120);
            }

            int top = S(168);
            list = new ListBox
            {
                Location = new Point(S(22), top), Size = new Size(S(560), ClientSize.Height - top - S(20)), BorderStyle = BorderStyle.None,
                BackColor = Theme.Bg, ForeColor = Theme.Text, DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = S(76), IntegralHeight = false
            };
            Program.DarkScroll(list);
            list.DrawItem += DrawItem;
            list.SelectedIndexChanged += (s, e) => ShowSelected();

            int rx = list.Right + S(18), rw = ClientSize.Width - rx - S(20);
            preview = new PreviewBox { Location = new Point(rx, top), Size = new Size(rw, S(230)) };
            lblName = new Label { Location = new Point(rx, preview.Bottom + S(10)), Size = new Size(rw, S(30)), Font = new Font("Segoe UI Semibold", 13f), ForeColor = Theme.Bright };
            lblDesc = new Label { Location = new Point(rx, lblName.Bottom), Size = new Size(rw, S(64)), Font = new Font("Segoe UI", 10f), ForeColor = Theme.Text };
            btnAction = new Button
            {
                Location = new Point(rx, lblDesc.Bottom + S(6)), Size = new Size(rw, S(46)), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 11.5f),
                BackColor = Theme.Green, ForeColor = Color.White, Cursor = Cursors.Hand
            };
            btnAction.FlatAppearance.BorderSize = 0;
            btnAction.Click += (s, e) => DoAction();
            lblInfo = new Label { Location = new Point(rx, btnAction.Bottom + S(6)), Size = new Size(rw, S(40)), Font = new Font("Segoe UI", 9.5f), ForeColor = Theme.Muted };

            Controls.AddRange(new Control[] { title, lblWallet, note, list, preview, lblName, lblDesc, btnAction, lblInfo });
            Controls.AddRange(tabs.ToArray());
            Fill();
        }

        ShopItem Selected { get { return list.SelectedItem as ShopItem; } }
#if DEMO
        public void DebugSlot(string s) { slot = s; Fill(); }
        public void DebugSelect(string id) { for (int i = 0; i < list.Items.Count; i++) if (((ShopItem)list.Items[i]).Id == id) list.SelectedIndex = i; }
#endif

        void Fill()
        {
            for (int i = 0; i < tabs.Count; i++) { tabs[i].Selected = Catalog.Slots[i] == slot; tabs[i].Invalidate(); }
            list.Items.Clear();
            foreach (var it in Catalog.Items.Where(i => i.Slot == slot)) list.Items.Add(it);
            if (list.Items.Count > 0) list.SelectedIndex = 0;
            RefreshWallet();
        }

        void RefreshWallet() { lblWallet.Text = "Saldo: " + gs.Prog.Wallet.ToString("N0") + " moedas"; }

        string Status(ShopItem it)
        {
            if (it.Id == "vale") return gs.Prog.Vouchers > 0 ? "Você tem " + gs.Prog.Vouchers : null;
            if (it.Slot == "use") return gs.Prog.Count(it.Id) > 0 ? "Você tem " + gs.Prog.Count(it.Id) : null;
            if (it.Slot == "sala")
            {
                var ri = PlayerRoom.Get(it.Id);
                if (!gs.Prog.Owned.Contains(it.Id)) return null;
                return ri.Group == null || PlayerRoom.Active(gs.Prog, ri.Group) == ri ? "NA SALA" : "TROCADO";
            }
            if (!gs.Prog.Owned.Contains(it.Id)) return null;
            if (it.Id == "patinete") return gs.Prog.Equipped("ride") == "patinete" ? "EM USO" : "COMPRADO";
            if (it.Slot == "util") return "COMPRADO";
            return gs.Prog.Equipped(it.Slot) == it.Id ? "EQUIPADO" : "COMPRADO";
        }

        void DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var it = (ShopItem)list.Items[e.Index];
            var g = e.Graphics;
            var r = e.Bounds;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            bool sel = (e.State & DrawItemState.Selected) != 0;
            g.FillRectangle(Art.B(Theme.Bg), r);
            Art.RR(g, sel ? Theme.Selected : Theme.Panel, r.X + S(2), r.Y + S(3), r.Width - S(4), r.Height - S(6), S(8));
            if (sel) Art.RR(g, Theme.Docker, r.X + S(2), r.Y + S(3), S(4), r.Height - S(6), S(2));

            var ib = new RectangleF(r.X + S(12), r.Y + S(9), r.Height - S(18), r.Height - S(18));
            using (var path = Art.RoundPath(ib.X, ib.Y, ib.Width, ib.Height, S(8)))
            {
                var st = g.Save();
                g.SetClip(path);
                if (it.Slot == "pet")
                {
                    g.FillRectangle(Art.B(Art.Hx(0x1F2A3C)), ib);
                    Art.Pet(g, it.Id, ib.X + ib.Width / 2, ib.Bottom - ib.Height * 0.15f, ib.Height * 1.3f, 0, 0, false);
                }
                else if (it.Slot == "sala" && roomIcon != null && roomIcon(it.Id, 128) != null)
                {
                    g.FillRectangle(Art.B(Art.Hx(0x1F2A3C)), ib);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(roomIcon(it.Id, 128), ib);
                }
                else if (it.Slot == "util" || it.Slot == "use" || it.Slot == "sala")
                {
                    g.FillRectangle(Art.B(Art.Hx(0x1F2A3C)), ib);
                    Art.ItemIcon(g, it.Id, new RectangleF(ib.X + ib.Width * 0.1f, ib.Y + ib.Height * 0.1f, ib.Width * 0.8f, ib.Height * 0.8f));
                }
                else Art.Portrait(g, ib, Catalog.MakeLook(gs.Prog.Visual, new Dictionary<string, string>(), it), 0);
                g.Restore(st);
            }

            float tx = ib.Right + S(14);
            string status = Status(it);
            g.DrawString(it.Name, fName, Art.B(Theme.Bright), tx, r.Y + S(10));
            g.DrawString(it.Desc, fDesc, Art.B(Theme.Muted), new RectangleF(tx, r.Y + S(36), r.Right - tx - S(110), S(34)));
            string right = status ?? it.Price.ToString("N0");
            var rs = g.MeasureString(right, fPrice);
            Color rc = status == "EQUIPADO" || status == "NA SALA" || status == "EM USO" ? Theme.Green : status != null ? Theme.Cyan : gs.Prog.Wallet >= it.Price ? Theme.Yellow : Theme.Red;
            float px = r.Right - rs.Width - S(16);
            g.DrawString(right, fPrice, Art.B(rc), px, r.Y + (r.Height - rs.Height) / 2);
            if (status == null) Art.Coin(g, px - S(24), r.Y + r.Height / 2f - S(9), S(18));
        }

        void ShowSelected()
        {
            var it = Selected;
            if (it == null) return;
            preview.Look = Catalog.MakeLook(gs.Prog.Visual, gs.Prog.Equip, (it.Slot == "util" && it.Id != "patinete") || it.Slot == "use" || it.Slot == "sala" ? null : it);
            preview.Picture = it.Slot == "sala" && roomIcon != null ? roomIcon(it.Id, 512) : null;
            preview.Invalidate();
            lblName.Text = it.Name;
            lblDesc.Text = it.Desc;
            bool owned = gs.Prog.Owned.Contains(it.Id);
            bool canPay = gs.Prog.Wallet >= it.Price;
            lblInfo.Text = "";
            bool repeat = it.Id == "vale" || it.Slot == "use";
            string block = it.Slot == "sala" && !owned ? PlayerRoom.BlockReason(gs.Prog, it.Id) : null;
            if (it.Slot == "sala" && owned)
            {
                var ri = PlayerRoom.Get(it.Id);
                bool active = ri.Group == null || PlayerRoom.Active(gs.Prog, ri.Group) == ri;
                btnAction.Text = active ? "Na sua sala ✔" : "Trocado por um melhor";
                btnAction.Enabled = false;
                btnAction.BackColor = Theme.Border;
                lblInfo.Text = "Sua sala fica no corredor sul, ao lado do Centro de Dados SQL.";
            }
            else if (block != null)
            {
                btnAction.Text = "Indisponível";
                btnAction.Enabled = false;
                btnAction.BackColor = Theme.Border;
                lblInfo.Text = block;
            }
            else if (repeat || !owned)
            {
                btnAction.Text = "Comprar por " + it.Price.ToString("N0") + " moedas";
                btnAction.Enabled = canPay;
                btnAction.BackColor = canPay ? Theme.Green : Theme.Border;
                if (!canPay) lblInfo.Text = "Faltam " + (it.Price - gs.Prog.Wallet).ToString("N0") + " moedas. Resolva desafios e objetivos para ganhar mais!";
                if (it.Slot == "sala" && canPay) lblInfo.Text = "Vai direto para a sua sala (corredor sul, ao lado do Centro de Dados SQL).";
                if (it.Id == "vale") lblInfo.Text = (lblInfo.Text + "  Você tem " + gs.Prog.Vouchers + " vale(s)-dica.").Trim();
                if (it.Slot == "use") lblInfo.Text = (lblInfo.Text + "  Você tem " + gs.Prog.Count(it.Id) + ". Use no escritório com a tecla indicada.").Trim();
            }
            else if (it.Id == "patinete")
            {
                bool riding = gs.Prog.Equipped("ride") == "patinete";
                btnAction.Text = riding ? "Descer do patinete" : "Subir no patinete";
                btnAction.Enabled = true;
                btnAction.BackColor = riding ? Theme.Border : Theme.Docker;
                lblInfo.Text = "No escritório, a tecla P também sobe e desce do patinete.";
            }
            else if (it.Slot == "util")
            {
                btnAction.Text = "Comprado ✔";
                btnAction.Enabled = false;
                btnAction.BackColor = Theme.Border;
            }
            else
            {
                bool eq = gs.Prog.Equipped(it.Slot) == it.Id;
                btnAction.Text = eq ? "Desequipar" : "Equipar";
                btnAction.Enabled = true;
                btnAction.BackColor = eq ? Theme.Border : Theme.Docker;
            }
        }

        void DoAction()
        {
            var it = Selected;
            if (it == null) return;
            bool owned = gs.Prog.Owned.Contains(it.Id);
            if (it.Id == "vale" || it.Slot == "use" || !owned)
            {
                if (gs.Prog.Wallet < it.Price) return;
                gs.Prog.Spent += it.Price;
                gs.Prog.Purchases++;
                if (it.Id == "vale") gs.Prog.Vouchers++;
                else if (it.Slot == "use") gs.Prog.Inv[it.Id] = gs.Prog.Count(it.Id) + 1;
                else
                {
                    gs.Prog.Owned.Add(it.Id);
                    if (it.Slot != "util" && it.Slot != "sala") gs.Prog.Equip[it.Slot] = it.Id;
                    if (it.Id == "patinete") gs.Prog.Equip["ride"] = "patinete";
                }
                Sfx.Play("buy");
            }
            else if (it.Id == "patinete")
            {
                if (gs.Prog.Equipped("ride") == "patinete") gs.Prog.Equip.Remove("ride"); else gs.Prog.Equip["ride"] = "patinete";
            }
            else if (it.Slot != "util" && it.Slot != "sala")
            {
                if (gs.Prog.Equipped(it.Slot) == it.Id) gs.Prog.Equip.Remove(it.Slot);
                else gs.Prog.Equip[it.Slot] = it.Id;
            }
            gs.Prog.Save();
            RefreshWallet();
            list.Invalidate();
            ShowSelected();
        }
    }
}
