using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace DockerQuest
{
    // Base dos minijogos da Sala de Jogos: tela de baixa resolução ampliada (pixel art), laço de ~60 quadros/s,
    // teclado e uma fonte de pixels 3×5.
    abstract class PixelGameForm : Form
    {
        protected readonly int LW, LH;
        protected readonly Bitmap Low;
        protected readonly HashSet<Keys> Held = new HashSet<Keys>();
        protected readonly Random Rng = new Random();
        readonly Timer timer;
        readonly Stopwatch clock = Stopwatch.StartNew();
        double last;
        readonly GamePanel panel;
        protected float MouseY = -1, MouseX = -1;
        protected bool Finished;

        class GamePanel : Panel { public GamePanel() { DoubleBuffered = true; ResizeRedraw = true; } }

        protected PixelGameForm(string title, int lw, int lh, float scale)
        {
            LW = lw; LH = lh;
            Low = new Bitmap(lw, lh, PixelFormat.Format32bppPArgb);
            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            BackColor = Color.Black;
            KeyPreview = true;
            var wa = Screen.PrimaryScreen.WorkingArea;
            int k = Math.Max(2, Math.Min((wa.Width - 80) / lw, (wa.Height - 120) / lh));
            ClientSize = new Size(lw * k, lh * k);
            panel = new GamePanel { Dock = DockStyle.Fill, BackColor = Color.Black };
            panel.Paint += (s, e) =>
            {
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                int kk = Math.Max(1, Math.Min(panel.ClientSize.Width / LW, panel.ClientSize.Height / LH));
                int ox = (panel.ClientSize.Width - LW * kk) / 2, oy = (panel.ClientSize.Height - LH * kk) / 2;
                e.Graphics.DrawImage(Low, ox, oy, LW * kk, LH * kk);
            };
            panel.MouseMove += (s, e) =>
            {
                int kk = Math.Max(1, Math.Min(panel.ClientSize.Width / LW, panel.ClientSize.Height / LH));
                int ox = (panel.ClientSize.Width - LW * kk) / 2, oy = (panel.ClientSize.Height - LH * kk) / 2;
                MouseX = (e.X - ox) / (float)kk; MouseY = (e.Y - oy) / (float)kk;
            };
            panel.MouseDown += (s, e) => OnClick();
            Controls.Add(panel);
            timer = new Timer { Interval = 15 };
            timer.Tick += (s, e) =>
            {
                double now = clock.Elapsed.TotalSeconds;
                float dt = (float)Math.Min(0.05, now - last);
                last = now;
                Step(dt);
                using (var g = Graphics.FromImage(Low))
                {
                    g.SmoothingMode = SmoothingMode.None;
                    g.PixelOffsetMode = PixelOffsetMode.None;
                    Draw(g);
                }
                panel.Invalidate();
            };
            Shown += (s, e) => { last = clock.Elapsed.TotalSeconds; timer.Start(); };
            FormClosed += (s, e) => { timer.Stop(); timer.Dispose(); Low.Dispose(); };
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            Held.Add(e.KeyCode);
            if (e.KeyCode == System.Windows.Forms.Keys.Escape) { Close(); return; }
            OnPress(e.KeyCode);
            e.Handled = true;
        }
        protected override void OnKeyUp(KeyEventArgs e) { Held.Remove(e.KeyCode); }
        protected override bool ProcessDialogKey(Keys keyData)
        {
            // setas e espaço vão para o jogo (não trocam o foco)
            var k = keyData & System.Windows.Forms.Keys.KeyCode;
            if (k == System.Windows.Forms.Keys.Up || k == System.Windows.Forms.Keys.Down || k == System.Windows.Forms.Keys.Left || k == System.Windows.Forms.Keys.Right || k == System.Windows.Forms.Keys.Space || k == System.Windows.Forms.Keys.Enter) return false;
            return base.ProcessDialogKey(keyData);
        }
        protected override bool IsInputKey(Keys keyData) { return true; }

#if DEMO
        public bool AutoPlay;
        public Bitmap DebugShot() { return (Bitmap)Low.Clone(); }
        public void DebugPress(Keys k) { OnPress(k); }
#endif
        protected abstract void Step(float dt);
        protected abstract void Draw(Graphics g);
        protected virtual void OnPress(Keys k) { }
        protected virtual void OnClick() { }

        protected bool Down(params Keys[] ks) { foreach (var k in ks) if (Held.Contains(k)) return true; return false; }

        protected static void Beep(string name, params double[] spec) { Audio.Tone(name, spec); }

        // ── desenho em pixels
        protected static readonly Dictionary<int, SolidBrush> brushes = new Dictionary<int, SolidBrush>();
        protected static SolidBrush B(Color c) { SolidBrush b; if (!brushes.TryGetValue(c.ToArgb(), out b)) { b = new SolidBrush(c); brushes[c.ToArgb()] = b; } return b; }
        protected static void Px(Graphics g, Color c, float x, float y, float w, float h) { g.FillRectangle(B(c), (int)Math.Round(x), (int)Math.Round(y), (int)Math.Round(w), (int)Math.Round(h)); }

        // sprite a partir de linhas de texto ('#' ou letra de cor; '.' vazio)
        protected static void Sprite(Graphics g, string[] rows, int x, int y, Color c, Dictionary<char, Color> pal = null)
        {
            for (int r = 0; r < rows.Length; r++)
                for (int k = 0; k < rows[r].Length; k++)
                {
                    char ch = rows[r][k];
                    if (ch == '.' || ch == ' ') continue;
                    Color cc = c;
                    if (pal != null && pal.ContainsKey(ch)) cc = pal[ch];
                    g.FillRectangle(B(cc), x + k, y + r, 1, 1);
                }
        }

        // fonte de pixels 3×5
        static readonly Dictionary<char, string> PixFont = new Dictionary<char, string>
        {
            {'A',"010101111101101"},{'B',"110101110101110"},{'C',"011100100100011"},{'D',"110101101101110"},{'E',"111100110100111"},
            {'F',"111100110100100"},{'G',"011100101101011"},{'H',"101101111101101"},{'I',"111010010010111"},{'J',"001001001101010"},
            {'K',"101101110101101"},{'L',"100100100100111"},{'M',"101111111101101"},{'N',"110101101101101"},{'O',"010101101101010"},
            {'P',"110101110100100"},{'Q',"010101101111011"},{'R',"110101110101101"},{'S',"011100010001110"},{'T',"111010010010010"},
            {'U',"101101101101111"},{'V',"101101101101010"},{'W',"101101111111101"},{'X',"101101010101101"},{'Y',"101101010010010"},
            {'Z',"111001010100111"},{'0',"111101101101111"},{'1',"010110010010111"},{'2',"110001010100111"},{'3',"110001010001110"},
            {'4',"101101111001001"},{'5',"111100110001110"},{'6',"011100111101111"},{'7',"111001010010010"},{'8',"111101111101111"},
            {'9',"111101111001110"},{':',"000010000010000"},{'!',"010010010000010"},{'-',"000000111000000"},{'.',"000000000000010"},
            {'/',"001001010100100"},{'?',"110001010000010"},{'+',"000010111010000"},{'×',"000101010101000"},{'\'',"010010000000000"},
            {',',"000000000010100"},{'<',"001010100010001"},{'>',"100010001010100"},{'(',"010100100100010"},{')',"010001001001010"}
        };

        protected static string Plain(string s)
        {
            var n = s.ToUpperInvariant().Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var ch in n) if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) != System.Globalization.UnicodeCategory.NonSpacingMark) sb.Append(ch);
            return sb.ToString();
        }

        protected static int TextW(string s, int k = 1) { return Plain(s).Length * 4 * k - k; }

        protected static void PText(Graphics g, string s, int x, int y, Color c, int k = 1)
        {
            s = Plain(s);
            for (int i = 0; i < s.Length; i++)
            {
                string gl;
                if (PixFont.TryGetValue(s[i], out gl))
                    for (int p = 0; p < 15; p++)
                        if (gl[p] == '1') g.FillRectangle(B(c), x + i * 4 * k + (p % 3) * k, y + (p / 3) * k, k, k);
            }
        }

        protected static void TextC(Graphics g, string s, int cx, int y, Color c, int k = 1) { PText(g, s, cx - TextW(s, k) / 2, y, c, k); }
        protected static void TextShadow(Graphics g, string s, int cx, int y, Color c, int k = 1) { TextC(g, s, cx + k, y + k, Color.FromArgb(160, 0, 0, 0), k); TextC(g, s, cx, y, c, k); }
    }

    // ───────────────────────── PINGUE-PONGUE contra o Vini ─────────────────────────
    class PingPongForm : PixelGameForm
    {
        const int Goal = 7;
        public bool Won;
        public int PlayerScore, CpuScore;
        public bool Played;

        readonly int tx0 = 18, ty0 = 26, tx1 = 302, ty1 = 172;   // mesa
        float py, cy, bx, by, vx, vy, serveT, cpuErr, flash;
        readonly float ph, ch = 26;
        int state;   // 0 título, 1 jogando, 2 fim
        int rally, best;
        readonly string opponent;

        public PingPongForm(float scale, bool bigPaddle, string opponent) : base("Pingue-pongue — Sala de Jogos", 320, 180, scale)
        {
            this.opponent = opponent;
            ph = bigPaddle ? 32 : 26;
            py = cy = (ty0 + ty1) / 2f;
            Reset(true);
        }

        void Reset(bool towardPlayer)
        {
            bx = (tx0 + tx1) / 2f; by = (ty0 + ty1) / 2f;
            float sp = 155;
            double ang = (Rng.NextDouble() - 0.5) * 0.9;
            vx = (towardPlayer ? -1 : 1) * sp * (float)Math.Cos(ang);
            vy = sp * (float)Math.Sin(ang);
            serveT = 0.9f;
            rally = 0;
            cpuErr = (float)(Rng.NextDouble() - 0.5) * 18;
        }

        protected override void OnPress(Keys k)
        {
            if (state == 0 && (k == System.Windows.Forms.Keys.Enter || k == System.Windows.Forms.Keys.Space || k == System.Windows.Forms.Keys.E)) { state = 1; Played = true; Beep("pp_go", 660, 0.06, 880, 0.08); }
            else if (state == 2 && (k == System.Windows.Forms.Keys.Enter || k == System.Windows.Forms.Keys.Space || k == System.Windows.Forms.Keys.E)) Close();
        }
        protected override void OnClick() { OnPress(System.Windows.Forms.Keys.Enter); }

        protected override void Step(float dt)
        {
            if (flash > 0) flash -= dt;
            if (state != 1) return;
            // jogador: teclado ou mouse
            float pv = 0;
            if (Down(System.Windows.Forms.Keys.W, System.Windows.Forms.Keys.Up)) pv -= 1;
            if (Down(System.Windows.Forms.Keys.S, System.Windows.Forms.Keys.Down)) pv += 1;
#if DEMO
            if (AutoPlay) { MouseY = by; }
#endif
            if (pv != 0) { py += pv * 200 * dt; MouseY = -1; }
            else if (MouseY >= 0) py += Math.Max(-260 * dt, Math.Min(260 * dt, MouseY - py));
            py = Math.Max(ty0 + ph / 2, Math.Min(ty1 - ph / 2, py));
            // Vini: segue a bola com um pequeno erro e velocidade limitada (dá para ganhar!)
            float target = vx > 0 ? by + cpuErr : (ty0 + ty1) / 2f;
            float cpuSpeed = 128 + rally * 3;
            cy += Math.Max(-cpuSpeed * dt, Math.Min(cpuSpeed * dt, target - cy));
            cy = Math.Max(ty0 + ch / 2, Math.Min(ty1 - ch / 2, cy));

            if (serveT > 0) { serveT -= dt; return; }
            bx += vx * dt; by += vy * dt;
            if (by < ty0 + 2) { by = ty0 + 2; vy = Math.Abs(vy); Beep("pp_wall", 320, 0.03); }
            if (by > ty1 - 2) { by = ty1 - 2; vy = -Math.Abs(vy); Beep("pp_wall", 320, 0.03); }
            // raquete do jogador (esquerda)
            float pxp = tx0 + 8;
            if (vx < 0 && bx - 2 <= pxp + 2 && bx > pxp - 4 && Math.Abs(by - py) <= ph / 2 + 2)
            {
                Hit(by - py, ph, 1);
                bx = pxp + 4;
            }
            float cxp = tx1 - 8;
            if (vx > 0 && bx + 2 >= cxp - 2 && bx < cxp + 4 && Math.Abs(by - cy) <= ch / 2 + 2)
            {
                Hit(by - cy, ch, -1);
                bx = cxp - 4;
                cpuErr = (float)(Rng.NextDouble() - 0.5) * (22 + rally * 1.5f);
            }
            if (bx < tx0 - 6) Point(false);
            else if (bx > tx1 + 6) Point(true);
        }

        void Hit(float off, float size, int dir)
        {
            rally++;
            best = Math.Max(best, rally);
            float sp = Math.Min(340, (float)Math.Sqrt(vx * vx + vy * vy) * 1.08f);
            double ang = Math.Max(-1, Math.Min(1, off / (size / 2))) * 0.95;   // onde bate na raquete muda o ângulo
            vx = dir * sp * (float)Math.Cos(ang);
            vy = sp * (float)Math.Sin(ang);
            Beep("pp_hit" + (dir > 0 ? "a" : "b"), dir > 0 ? 740 : 620, 0.035);
        }

        void Point(bool player)
        {
            if (player) PlayerScore++; else CpuScore++;
            flash = 0.6f;
            Beep(player ? "pp_pt" : "pp_lose", player ? 880 : 220, 0.08, player ? 1175 : 165, 0.12);
            if (PlayerScore >= Goal || CpuScore >= Goal)
            {
                state = 2;
                Won = PlayerScore > CpuScore;
                Beep(Won ? "pp_win" : "pp_over", Won ? 523 : 392, 0.1, Won ? 659 : 330, 0.1, Won ? 784 : 262, 0.1, Won ? 1047 : 196, 0.3);
                return;
            }
            Reset(!player);
        }

        static readonly Color Floor = Color.FromArgb(38, 44, 58), FloorDk = Color.FromArgb(30, 35, 47);
        static readonly Color Table = Color.FromArgb(31, 122, 80), TableDk = Color.FromArgb(22, 92, 60), Line = Color.FromArgb(236, 242, 246);

        protected override void Draw(Graphics g)
        {
            // chão em xadrez e a mesa vista de cima
            for (int y = 0; y < LH; y += 10)
                for (int x = 0; x < LW; x += 10)
                    Px(g, ((x + y) / 10) % 2 == 0 ? Floor : FloorDk, x, y, 10, 10);
            Px(g, Color.FromArgb(18, 20, 28), tx0 + 3, ty1 + 1, tx1 - tx0, 4);
            Px(g, Table, tx0, ty0, tx1 - tx0, ty1 - ty0);
            Px(g, TableDk, tx0, ty1 - 3, tx1 - tx0, 3);
            Px(g, Line, tx0, ty0, tx1 - tx0, 2); Px(g, Line, tx0, ty1 - 2, tx1 - tx0, 2);
            Px(g, Line, tx0, ty0, 2, ty1 - ty0); Px(g, Line, tx1 - 2, ty0, 2, ty1 - ty0);
            Px(g, Color.FromArgb(150, Line), tx0, (ty0 + ty1) / 2, tx1 - tx0, 1);
            // rede
            int nx = (tx0 + tx1) / 2;
            Px(g, Color.FromArgb(70, 76, 90), nx - 1, ty0 - 4, 3, ty1 - ty0 + 8);
            for (int y = ty0 - 3; y < ty1 + 4; y += 3) Px(g, Color.FromArgb(200, 210, 220), nx, y, 1, 1);
            // raquetes
            Px(g, Color.FromArgb(214, 40, 40), tx0 + 6, py - ph / 2, 4, ph);
            Px(g, Color.FromArgb(255, 120, 110), tx0 + 6, py - ph / 2, 1, ph);
            Px(g, Color.FromArgb(30, 30, 36), tx1 - 10, cy - ch / 2, 4, ch);
            Px(g, Color.FromArgb(90, 90, 100), tx1 - 10, cy - ch / 2, 1, ch);
            // bola com sombra
            if (state == 1)
            {
                Px(g, Color.FromArgb(90, 0, 0, 0), bx - 1, by + 1, 4, 3);
                Px(g, Color.FromArgb(255, 170, 70), bx - 2, by - 2, 4, 4);
                Px(g, Color.White, bx - 2, by - 2, 2, 1);
            }
            // placar
            Px(g, Color.FromArgb(14, 17, 23), 0, 0, LW, 18);
            PText(g, "VOCE", 8, 6, Color.FromArgb(255, 140, 130));
            TextC(g, PlayerScore + " - " + CpuScore, LW / 2, 3, flash > 0 && (int)(flash * 10) % 2 == 0 ? Color.FromArgb(255, 220, 90) : Color.White, 2);
            PText(g, Plain(opponent), LW - 8 - TextW(opponent), 6, Color.FromArgb(170, 180, 200));
            if (state == 1 && serveT > 0) TextShadow(g, "SAQUE!", LW / 2, 88, Color.FromArgb(255, 220, 90), 2);
            if (state == 0) Panel(g, "PINGUE-PONGUE", "VOCE CONTRA " + Plain(opponent), "W/S, SETAS OU MOUSE", "PRIMEIRO A " + Goal + " PONTOS", "ENTER PARA COMECAR");
            if (state == 2) Panel(g, Won ? "VOCE VENCEU!" : Plain(opponent) + " VENCEU!", PlayerScore + " A " + CpuScore, "MAIOR SEQUENCIA: " + best + " TOQUES", Won ? "PEDIU REVANCHE..." : "TREINA E VOLTA!", "ENTER PARA SAIR");
        }

        void Panel(Graphics g, string t, string a, string b, string c, string d)
        {
            Px(g, Color.FromArgb(220, 10, 12, 18), 60, 44, 200, 100);
            Px(g, Color.FromArgb(255, 220, 90), 60, 44, 200, 2);
            TextShadow(g, t, LW / 2, 54, Color.FromArgb(255, 220, 90), 2);
            TextC(g, a, LW / 2, 74, Color.White);
            TextC(g, b, LW / 2, 86, Color.FromArgb(170, 180, 200));
            TextC(g, c, LW / 2, 98, Color.FromArgb(170, 180, 200));
            if ((int)(Environment.TickCount / 400) % 2 == 0) TextC(g, d, LW / 2, 124, Color.FromArgb(120, 230, 140));
        }
    }

    // ───────────────────────── INVASORES DE BUGS (estilo Space Invaders) ─────────────────────────
    class InvadersForm : PixelGameForm
    {
        public int Score, Wave = 1;
        public bool Played;
        readonly int best;

        class Bug { public float X, Y; public int Kind; public bool Alive = true; }
        class Shot { public float X, Y, V; }
        readonly List<Bug> bugs = new List<Bug>();
        readonly List<Shot> enemyShots = new List<Shot>();
        readonly List<KeyValuePair<PointF, float>> booms = new List<KeyValuePair<PointF, float>>();
        Shot shot;
        float shipX, stepT, fireT, ufoX = -40, ufoT = 12, dead, waveBanner;
        int dir = 1, frame, lives = 3, state;   // 0 título, 1 jogando, 2 fim
        bool[,] shields;
        const int ShipY = 228, ShieldY = 198;

        static readonly string[][] BugArt =
        {
            new[] { "..##..##..", "...####...", "..######..", ".##.##.##.", "##########", "#.######.#", "#.#....#.#", "...#..#..." },   // bug verde
            new[] { "...####...", ".########.", "##.####.##", "##########", ".##.##.##.", "..##..##..", ".##....##.", "##......##" },   // vírus vermelho
            new[] { "....##....", "...####...", "..######..", ".##.##.##.", ".########.", "...#..#...", "..#.##.#..", ".#.#..#.#." },   // malware roxo
        };
        static readonly string[][] BugArt2 =
        {
            new[] { "..##..##..", "...####...", "..######..", ".##.##.##.", "##########", "..######..", ".#......#.", "#........#" },
            new[] { "...####...", ".########.", "##.####.##", "##########", ".##.##.##.", "..##..##..", "..##..##..", ".##....##." },
            new[] { "....##....", "...####...", "..######..", ".##.##.##.", ".########.", "..#.##.#..", ".#......#.", "#........#" },
        };
        static readonly Color[] BugColor = { Color.FromArgb(120, 230, 140), Color.FromArgb(255, 110, 100), Color.FromArgb(200, 140, 255) };
        static readonly int[] BugPts = { 10, 20, 30 };
        static readonly string[] Ship = { "......#......", ".....###.....", ".....###.....", ".###########.", "#############", "#.#.#.#.#.#.#", "#############", "#############" };
        static readonly string[] Ufo = { "....######....", "..##########..", ".##.##.##.##.#", "##############", "..###....###..", "...#......#..." };

        public InvadersForm(float scale, int best) : base("Fliperama — Invasores de Bugs", 240, 256, scale)
        {
            this.best = best;
            shipX = LW / 2f;
            NewWave();
            ResetShields();
        }

        void NewWave()
        {
            bugs.Clear();
            int top = 42 + Math.Min(4, Wave - 1) * 6;
            for (int r = 0; r < 5; r++)
                for (int c = 0; c < 9; c++)
                    bugs.Add(new Bug { X = 22 + c * 20, Y = top + r * 14, Kind = r == 0 ? 2 : r < 3 ? 1 : 0 });
            dir = 1; stepT = 0;
            enemyShots.Clear();
            waveBanner = 1.6f;
        }

        void ResetShields()
        {
            shields = new bool[LW, 14];
            for (int s = 0; s < 4; s++)
            {
                int x0 = 26 + s * 54;
                for (int x = 0; x < 22; x++)
                    for (int y = 0; y < 14; y++)
                    {
                        bool notch = y > 9 && x > 6 && x < 15;
                        bool corner = (y < 3 && (x < 3 - y || x > 18 + y));
                        if (!notch && !corner) shields[x0 + x, y] = true;
                    }
            }
        }

        bool HitShield(float x, float y)
        {
            int sx = (int)x, sy = (int)y - ShieldY;
            if (sy < 0 || sy >= 14 || sx < 0 || sx >= LW || !shields[sx, sy]) return false;
            for (int dx = -2; dx <= 2; dx++)
                for (int dy = -2; dy <= 2; dy++)
                {
                    int ax = sx + dx, ay = sy + dy;
                    if (ax >= 0 && ax < LW && ay >= 0 && ay < 14 && Rng.Next(3) > 0) shields[ax, ay] = false;
                }
            return true;
        }

        protected override void OnPress(Keys k)
        {
            if (state == 0 && (k == System.Windows.Forms.Keys.Enter || k == System.Windows.Forms.Keys.Space)) { state = 1; Played = true; Beep("inv_go", 523, 0.06, 784, 0.1); return; }
            if (state == 2 && (k == System.Windows.Forms.Keys.Enter || k == System.Windows.Forms.Keys.E)) { Close(); return; }
            if (state == 1 && (k == System.Windows.Forms.Keys.Space || k == System.Windows.Forms.Keys.W || k == System.Windows.Forms.Keys.Up)) Fire();
        }
        protected override void OnClick() { if (state == 1) Fire(); else OnPress(System.Windows.Forms.Keys.Enter); }

        void Fire()
        {
            if (shot != null || dead > 0) return;
            shot = new Shot { X = shipX, Y = ShipY - 2, V = -230 };
            Beep("inv_shot", 1200, 0.02, 900, 0.03);
        }

        protected override void Step(float dt)
        {
            for (int i = booms.Count - 1; i >= 0; i--) { var b = booms[i]; if (b.Value - dt <= 0) booms.RemoveAt(i); else booms[i] = new KeyValuePair<PointF, float>(b.Key, b.Value - dt); }
            if (state != 1) return;
            if (waveBanner > 0) waveBanner -= dt;
            if (dead > 0)
            {
                dead -= dt;
                if (dead <= 0 && lives <= 0) End();
                return;
            }
            // nave (servidor)
            float mv = 0;
            if (Down(System.Windows.Forms.Keys.A, System.Windows.Forms.Keys.Left)) mv -= 1;
            if (Down(System.Windows.Forms.Keys.D, System.Windows.Forms.Keys.Right)) mv += 1;
#if DEMO
            if (AutoPlay)
            {
                var tgt = bugs.Where(b => b.Alive).OrderByDescending(b => b.Y).FirstOrDefault();
                if (tgt != null) MouseX = tgt.X + 5;
                if (Rng.Next(8) == 0) Fire();
            }
#endif
            if (mv != 0) { shipX += mv * 95 * dt; MouseX = -1; }
            else if (MouseX >= 0) shipX += Math.Max(-110 * dt, Math.Min(110 * dt, MouseX - shipX));
            shipX = Math.Max(8, Math.Min(LW - 8, shipX));

            // tiro do jogador
            if (shot != null)
            {
                shot.Y += shot.V * dt;
                if (shot.Y < 14) shot = null;
                else if (HitShield(shot.X, shot.Y)) shot = null;
                else
                {
                    foreach (var b in bugs)
                        if (b.Alive && shot.X >= b.X - 1 && shot.X <= b.X + 10 && shot.Y >= b.Y && shot.Y <= b.Y + 8)
                        {
                            b.Alive = false; shot = null;
                            Score += BugPts[b.Kind];
                            booms.Add(new KeyValuePair<PointF, float>(new PointF(b.X + 5, b.Y + 4), 0.25f));
                            Beep("inv_kill", 300, 0.03, 180, 0.05);
                            break;
                        }
                    if (shot != null && ufoX > -30 && ufoX < LW + 30 && shot.Y < 30 && shot.Y > 18 && Math.Abs(shot.X - (ufoX + 7)) < 8)
                    {
                        int bonus = 50 * (1 + Rng.Next(4));
                        Score += bonus; booms.Add(new KeyValuePair<PointF, float>(new PointF(ufoX + 7, 24), 0.5f));
                        ufoX = -60; ufoT = 15 + (float)Rng.NextDouble() * 10; shot = null;
                        Beep("inv_ufo", 880, 0.05, 1320, 0.05, 1760, 0.1);
                    }
                }
            }

            // marcha dos bugs: quanto menos sobram, mais rápido
            var alive = bugs.Where(b => b.Alive).ToList();
            if (alive.Count == 0) { Wave++; Score += 100; NewWave(); Beep("inv_wave", 523, 0.08, 659, 0.08, 784, 0.15); return; }
            float interval = Math.Max(0.06f, 0.55f * alive.Count / 45f + 0.04f - (Wave - 1) * 0.04f);
            stepT += dt;
            if (stepT >= interval)
            {
                stepT = 0;
                frame ^= 1;
                float minX = alive.Min(b => b.X), maxX = alive.Max(b => b.X) + 10;
                if ((dir > 0 && maxX + 3 > LW - 4) || (dir < 0 && minX - 3 < 4))
                {
                    foreach (var b in alive) b.Y += 6;
                    dir = -dir;
                }
                else foreach (var b in alive) b.X += 3 * dir;
                Beep("inv_step" + frame, frame == 0 ? 98 : 87, 0.05);
            }
            if (alive.Any(b => b.Y + 8 >= ShipY - 2)) { lives = 0; Die(); return; }
            // bugs que chegam aos escudos destroem o escudo
            foreach (var b in alive) for (int x = 0; x < 10; x++) for (int y = 0; y < 8; y++) { int sy = (int)b.Y + y - ShieldY, sx = (int)b.X + x; if (sy >= 0 && sy < 14 && sx >= 0 && sx < LW) shields[sx, sy] = false; }

            // tiros dos bugs
            fireT -= dt;
            if (fireT <= 0)
            {
                fireT = Math.Max(0.35f, 1.0f - Wave * 0.08f) * (0.6f + (float)Rng.NextDouble() * 0.8f);
                var cols = alive.GroupBy(b => (int)Math.Round(b.X)).Select(gg => gg.OrderByDescending(b => b.Y).First()).ToList();
                var sh = cols[Rng.Next(cols.Count)];
                enemyShots.Add(new Shot { X = sh.X + 5, Y = sh.Y + 8, V = 75 + Wave * 6 });
            }
            for (int i = enemyShots.Count - 1; i >= 0; i--)
            {
                var s = enemyShots[i];
                s.Y += s.V * dt;
                if (s.Y > LH) { enemyShots.RemoveAt(i); continue; }
                if (HitShield(s.X, s.Y)) { enemyShots.RemoveAt(i); continue; }
                if (shot != null && Math.Abs(s.X - shot.X) < 2 && Math.Abs(s.Y - shot.Y) < 4) { enemyShots.RemoveAt(i); shot = null; continue; }
                if (s.Y >= ShipY && s.Y <= ShipY + 8 && Math.Abs(s.X - shipX) <= 6) { enemyShots.RemoveAt(i); lives--; Die(); return; }
            }
            // disco voador (trojan) de bônus
            ufoT -= dt;
            if (ufoT <= 0 && ufoX < -30) { ufoX = -20; }
            if (ufoX >= -20 && ufoT <= 0)
            {
                ufoX += 40 * dt;
                if (ufoX > LW + 20) { ufoX = -60; ufoT = 15 + (float)Rng.NextDouble() * 10; }
            }
        }

        void Die()
        {
            dead = 1.3f;
            booms.Add(new KeyValuePair<PointF, float>(new PointF(shipX, ShipY + 4), 1.0f));
            enemyShots.Clear(); shot = null;
            Beep("inv_die", 220, 0.1, 165, 0.1, 110, 0.3);
        }

        void End()
        {
            state = 2;
            Beep("inv_over", 392, 0.12, 330, 0.12, 262, 0.12, 196, 0.4);
        }

        protected override void Draw(Graphics g)
        {
            g.Clear(Color.FromArgb(6, 8, 14));
            // estrelas / "pacotes" ao fundo
            var r = new Random(7);
            for (int i = 0; i < 40; i++) Px(g, Color.FromArgb(40 + r.Next(60), 120, 160, 255), r.Next(LW), r.Next(LH - 30), 1, 1);
            // placar
            PText(g, "PONTOS " + Score, 4, 3, Color.White);
            TextC(g, "RECORDE " + Math.Max(best, Score), LW / 2 + 20, 3, Color.FromArgb(255, 220, 90));
            PText(g, "ONDA " + Wave, LW - 4 - TextW("ONDA " + Wave), 3, Color.FromArgb(120, 200, 255));
            Px(g, Color.FromArgb(60, 70, 90), 0, 11, LW, 1);
            // trojan
            if (ufoX > -30 && ufoX < LW + 20) Sprite(g, Ufo, (int)ufoX, 20, Color.FromArgb(255, 90, 200));
            // bugs
            foreach (var b in bugs)
                if (b.Alive) Sprite(g, frame == 0 ? BugArt[b.Kind] : BugArt2[b.Kind], (int)b.X, (int)b.Y, BugColor[b.Kind]);
            // escudos (firewall)
            for (int x = 0; x < LW; x++)
                for (int y = 0; y < 14; y++)
                    if (shields[x, y]) g.FillRectangle(B(y < 2 ? Color.FromArgb(255, 160, 80) : Color.FromArgb(232, 110, 50)), x, ShieldY + y, 1, 1);
            // tiros
            if (shot != null) Px(g, Color.FromArgb(120, 230, 255), shot.X, shot.Y - 3, 1, 4);
            foreach (var s in enemyShots)
            {
                bool zig = ((int)(s.Y / 3)) % 2 == 0;
                Px(g, Color.FromArgb(255, 230, 120), s.X + (zig ? 0 : 1), s.Y - 4, 1, 2);
                Px(g, Color.FromArgb(255, 230, 120), s.X + (zig ? 1 : 0), s.Y - 2, 1, 2);
            }
            // nave
            if (dead <= 0) Sprite(g, Ship, (int)shipX - 6, ShipY, Color.FromArgb(36, 150, 237));
            // explosões
            foreach (var b in booms)
            {
                int n = (int)(b.Value * 30);
                var rr = new Random((int)(b.Key.X * 13 + b.Key.Y));
                for (int i = 0; i < 10; i++)
                    Px(g, i % 2 == 0 ? Color.FromArgb(255, 220, 90) : Color.FromArgb(255, 120, 80), b.Key.X + rr.Next(-n, n + 1) / 3f, b.Key.Y + rr.Next(-n, n + 1) / 3f, 1, 1);
            }
            // chão e vidas
            Px(g, Color.FromArgb(36, 150, 237), 0, LH - 14, LW, 1);
            PText(g, "VIDAS", 4, LH - 10, Color.FromArgb(170, 180, 200));
            for (int i = 0; i < Math.Max(0, lives - 1); i++) Sprite(g, Ship, 28 + i * 16, LH - 11, Color.FromArgb(36, 150, 237));
            PText(g, "ESPACO ATIRA", LW - 4 - TextW("ESPACO ATIRA"), LH - 10, Color.FromArgb(110, 120, 140));

            if (state == 1 && waveBanner > 0) TextShadow(g, "ONDA " + Wave, LW / 2, 120, Color.FromArgb(120, 200, 255), 2);
            if (state == 0)
            {
                Box(g);
                TextShadow(g, "INVASORES", LW / 2, 64, Color.FromArgb(120, 230, 140), 3);
                TextShadow(g, "DE BUGS", LW / 2, 84, Color.FromArgb(120, 230, 140), 3);
                TextC(g, "PROTEJA O SERVIDOR!", LW / 2, 110, Color.White);
                for (int k = 0; k < 3; k++)
                {
                    Sprite(g, BugArt[2 - k], LW / 2 - 40, 124 + k * 13, BugColor[2 - k]);
                    PText(g, "= " + BugPts[2 - k] + " PONTOS", LW / 2 - 24, 126 + k * 13, Color.FromArgb(200, 210, 220));
                }
                TextC(g, "SETAS/AD MOVE  ESPACO ATIRA", LW / 2, 168, Color.FromArgb(170, 180, 200));
                if ((Environment.TickCount / 400) % 2 == 0) TextC(g, "ENTER PARA COMECAR", LW / 2, 180, Color.FromArgb(255, 220, 90));
            }
            if (state == 2)
            {
                Box(g);
                TextShadow(g, "FIM DE JOGO", LW / 2, 74, Color.FromArgb(255, 110, 100), 2);
                TextC(g, "PONTOS: " + Score, LW / 2, 100, Color.White);
                TextC(g, "ONDA: " + Wave, LW / 2, 112, Color.FromArgb(120, 200, 255));
                if (Score > best) TextC(g, "NOVO RECORDE!", LW / 2, 128, Color.FromArgb(255, 220, 90));
                if ((Environment.TickCount / 400) % 2 == 0) TextC(g, "ENTER PARA SAIR", LW / 2, 160, Color.FromArgb(120, 230, 140));
            }
        }

        void Box(Graphics g)
        {
            Px(g, Color.FromArgb(225, 8, 10, 16), 16, 54, LW - 32, 140);
            Px(g, Color.FromArgb(120, 230, 140), 16, 54, LW - 32, 1);
            Px(g, Color.FromArgb(120, 230, 140), 16, 193, LW - 32, 1);
        }
    }
}
