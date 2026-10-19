using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DockerQuest
{
    static class Program
    {
        [DllImport("user32.dll")]
        static extern bool SetProcessDPIAware();

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        static extern int SetWindowTheme(IntPtr hwnd, string app, string idList);

        // Barras de rolagem escuras (Windows 10 1809+); em versões antigas simplesmente não faz nada.
        public static void DarkScroll(Control c)
        {
            c.HandleCreated += (s, e) => { try { SetWindowTheme(c.Handle, "DarkMode_Explorer", null); } catch { } };
        }

        [STAThread]
        static void Main()
        {
            try { SetProcessDPIAware(); } catch { }
            // erros inesperados vão para um log (%APPDATA%\StackQuest\erros.txt) em vez de abrir uma caixa para o aluno
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
            {
                try
                {
                    string dir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StackQuest");
                    System.IO.Directory.CreateDirectory(dir);
                    System.IO.File.AppendAllText(System.IO.Path.Combine(dir, "erros.txt"), DateTime.Now + "  " + e.Exception + Environment.NewLine + Environment.NewLine);
                }
                catch { }
            };
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Audio.Start();
            Application.AddMessageFilter(new KeyClickFilter());
            Application.Run(new MainForm());
        }
    }

    // barulho de teclado quando o aluno digita em qualquer campo de texto (terminal, IDE, CloudShell, pesquisa...)
    class KeyClickFilter : IMessageFilter
    {
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg != 0x0100) return false;                                   // WM_KEYDOWN
            if (((long)m.LParam & 0x40000000) != 0) return false;               // tecla segurada (repetição): o teclado real não estala de novo
            var tb = Control.FromHandle(m.HWnd) as TextBoxBase;
            if (tb == null || tb.ReadOnly || !tb.Enabled) return false;
            var k = (Keys)(int)(long)m.WParam;
            if (k == Keys.ShiftKey || k == Keys.ControlKey || k == Keys.Menu || k == Keys.Capital || k == Keys.LWin || k == Keys.RWin) return false;
            Audio.Key(k == Keys.Space ? 1 : k == Keys.Enter ? 2 : k == Keys.Back || k == Keys.Delete ? 3 : 0);
            return false;
        }
    }

    static class Theme
    {
        public static readonly Color Bg = Color.FromArgb(13, 17, 23);
        public static readonly Color Panel = Color.FromArgb(22, 27, 34);
        public static readonly Color Term = Color.FromArgb(1, 4, 9);
        public static readonly Color Border = Color.FromArgb(48, 54, 61);
        public static readonly Color Text = Color.FromArgb(201, 209, 217);
        public static readonly Color Bright = Color.FromArgb(240, 246, 252);
        public static readonly Color Muted = Color.FromArgb(125, 133, 144);
        public static readonly Color Green = Color.FromArgb(63, 185, 80);
        public static readonly Color Red = Color.FromArgb(248, 81, 73);
        public static readonly Color Yellow = Color.FromArgb(227, 179, 65);
        public static readonly Color Cyan = Color.FromArgb(57, 197, 207);
        public static readonly Color Docker = Color.FromArgb(36, 150, 237);
        public static readonly Color Selected = Color.FromArgb(31, 58, 95);
    }

    class MainForm : Form
    {
        readonly GameState gs;
        readonly GameView view;
        readonly TerminalPanel term;
        readonly float scale;

        int S(float v) { return (int)Math.Round(v * scale); }

        public MainForm()
        {
            using (var g = CreateGraphics()) scale = g.DpiX / 96f;
            Text = "Stack Quest — carreira tech na Micorosoft";
            BackColor = Theme.Bg;
            StartPosition = FormStartPosition.CenterScreen;
            var wa = Screen.PrimaryScreen.WorkingArea;
            int w = Math.Min(S(1440), (int)(wa.Width * 0.96)), h = Math.Min(S(900), (int)(wa.Height * 0.96));
            Size = new Size(w, h);
            MinimumSize = new Size(Math.Min(S(1000), w), Math.Min(S(640), h));
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            gs = new GameState();
            view = new GameView(gs, scale) { Dock = DockStyle.Fill };
            term = new TerminalPanel(gs, view.World, scale);
            Controls.Add(term);
            Controls.Add(view);

            view.TerminalRequested += ch =>
            {
                view.Paused = true;
                LayoutTerminal();
                term.Open(ch);
            };
            term.Closed += (ch, done) =>
            {
                view.Paused = false;
                view.Focus();
                view.OnTerminalClosed(ch, done);
            };
            term.Solved += (lv, pts) => view.OnSolved(lv, pts, false);
            term.IncidentSolved += pts => view.SolveIncident(pts);
            view.BadgeEarned += (cat, tier) => { view.ClearKeys(); using (var f = new BadgeForm(gs, scale, cat, tier)) f.ShowDialog(this); view.Focus(); };
            view.BadgesRequested += () => { view.ClearKeys(); using (var f = new BadgeForm(gs, scale, null, -1)) f.ShowDialog(this); view.Focus(); };
            view.HelpPressed += ShowHelp;
            view.ResetRequested += ResetProgress;
            view.DocsRequested += cat => { using (var f = new DocsForm(gs, scale, cat)) f.ShowDialog(this); AfterDialog(); };
            view.ShopRequested += () => { using (var f = new ShopForm(gs, scale, (id, sz) => view.RoomIcon(id, sz))) f.ShowDialog(this); AfterDialog(); view.RefreshRoom(); };
            view.LessonsRequested += () => { view.ClearKeys(); using (var f = new LessonForm(gs, scale)) { f.ShowDialog(this); view.LessonDone(f.CompletedId, f.Hits, f.Total); } AfterDialog(); };
            view.MiniGameRequested += game =>
            {
                view.ClearKeys();
                if (game == "pingpong")
                    using (var f = new PingPongForm(scale, gs.Prog.Owned.Contains("tenis"), "Vini")) { f.ShowDialog(this); view.MiniGameDone(game, f.Played, f.PlayerScore, f.CpuScore, f.Won); }
                else
                    using (var f = new InvadersForm(scale, gs.Prog.Stat("invaders_best"))) { f.ShowDialog(this); view.MiniGameDone(game, f.Played, f.Score, f.Wave, false); }
                AfterDialog();
            };
            view.WardrobeRequested += () => { using (var f = new CharacterForm(gs, scale, false)) f.ShowDialog(this); AfterDialog(); };
            Resize += (s, e) => LayoutTerminal();
            // fome e sede embaçam também o terminal / IDE / console AWS
            blur = new BlurOverlay();
            var blurTimer = new Timer { Interval = 140 };
            blurTimer.Tick += (s, e) => UpdateBlur();
            blurTimer.Start();
            Deactivate += (s, e) => view.ClearKeys();

#if DEMO
            if (Demo.Active) { StartPosition = FormStartPosition.Manual; Location = Demo.Perf ? new Point(40, 40) : new Point(-5000, 40); ShowInTaskbar = false; gs.Prog.PlayerName = "Alex"; gs.Prog.HasVisual = true; }
#endif
            Shown += (s, e) =>
            {
                if (string.IsNullOrEmpty(gs.Prog.PlayerName) || !gs.Prog.HasVisual) CreateCharacter();
                view.Focus();
                if (gs.Prog.Met.Count == 0 && gs.Prog.Completed == 0) view.StartIntro();
#if DEMO
                if (Demo.Active) Demo.Run(this, view, term, gs);
#endif
            };
        }

        BlurOverlay blur;

        // ── tela cheia (F11): funciona no escritório, no terminal, na IDE e no console AWS
        bool fullScreen;
        Rectangle normalBounds;
        FormWindowState normalState;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F11 || keyData == (Keys.Alt | Keys.Enter)) { ToggleFullScreen(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void ToggleFullScreen()
        {
            SuspendLayout();
            if (!fullScreen)
            {
                normalState = WindowState == FormWindowState.Minimized ? FormWindowState.Normal : WindowState;
                normalBounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
                var scr = Screen.FromControl(this).Bounds;
                WindowState = FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.None;
                Bounds = scr;
                fullScreen = true;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                Bounds = normalBounds;
                WindowState = normalState;
                fullScreen = false;
            }
            ResumeLayout(true);
            LayoutTerminal();
            view.Toast(fullScreen ? "Tela cheia  ·  F11 volta para a janela" : "Modo janela  ·  F11 para tela cheia", Art.Hx(0x58A6FF));
        }
#if DEMO
        public bool DebugFullScreen { get { return fullScreen; } }
        public Bitmap DebugBlurShot() { UpdateBlur(); return blur.Visible && blur.Shot != null ? (Bitmap)blur.Shot.Clone() : null; }
        public float DebugBlurOpacity { get { return blur.Visible ? (float)blur.Opacity : 0; } }
#endif

        void UpdateBlur()
        {
            float level = view.NeedBlur;
            bool active = Form.ActiveForm == this;
#if DEMO
            active |= Demo.Active;
#endif
            bool show = level > 0.01f && term.Visible && WindowState != FormWindowState.Minimized && active;
            if (!show) { if (blur.Visible) blur.Hide(); return; }
            var r = term.RectangleToScreen(term.ClientRectangle);
            blur.ShowBlur(this, r, level);
        }

        void LayoutTerminal()
        {
            // a estação (terminal, IDE ou console AWS) ocupa praticamente a janela inteira
            int m = S(6);
            int w = ClientSize.Width - 2 * m, h = ClientSize.Height - 2 * m;
            term.Bounds = new Rectangle(m, m, Math.Max(100, w), Math.Max(100, h));
        }

        void CreateCharacter()
        {
            using (var f = new CharacterForm(gs, scale, true))
                if (f.ShowDialog(this) != DialogResult.OK)
                {
                    if (string.IsNullOrEmpty(gs.Prog.PlayerName)) gs.Prog.PlayerName = "Estagiário(a)";
                    gs.Prog.HasVisual = true;
                    gs.Prog.Save();
                }
            view.RefreshLook();
        }

        void AfterDialog()
        {
            view.ClearKeys();
            view.RefreshLook();
            view.Focus();
        }
        void ResetProgress()
        {
            var ok = MessageBox.Show(this, "Apagar todo o progresso, badges e pontuação e recomeçar do primeiro dia na Micorosoft?", "Reiniciar progresso",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (ok != DialogResult.Yes) { view.Focus(); return; }
            gs.ResetAll();
            CreateCharacter();
            view.PlacePlayer();
            view.Focus();
            view.StartIntro();
        }

        void ShowHelp()
        {
            MessageBox.Show(this,
                "STACK QUEST — carreira tech na Micorosoft\n\n" +
                "Você acaba de entrar como estagiário(a) na Micorosoft. O campus tem 19 salas: 9 áreas de tecnologia, Diretoria,\nBiblioteca, Refeitório, Auditório, Jardim, Sala de Jogos, SOC de Segurança, Help Desk e Laboratório de Inovação.\nAs áreas são:\n" +
                "Docker, Redes, Python, PHP, Frontend (HTML/CSS/JS), React, Flutter, AWS e SQL. Nada é trancado: ande à vontade!\n\n" +
                "COMO JOGAR\n" +
                "•  O escritório aparece em VISÃO ISOMÉTRICA (estilo Habbo), com bonecos de blocos. Ande com WASD ou setas (W sobe na tela,\n   D vai para a direita; Shift corre) e siga a seta amarela. A rodinha do mouse aproxima e afasta.\n" +
                "•  SALA DE JOGOS: a mesa de pingue-pongue é um jogo de verdade contra o Vini (W/S, setas ou mouse; primeiro a 7),\n   e os fliperamas rodam INVASORES DE BUGS (setas movem, Espaço atira). Vitórias e recordes rendem pontos.\n" +
                "•  Converse com o(a) MENTOR(A) de cada área (ícone !) para liberar o terminal da sala.\n" +
                "•  Docker e Redes usam o TERMINAL da sala: digite o comando exato pedido.\n" +
                "•  SQL usa o terminal MYSQL do Centro de Dados (descendo pelo corredor sul): os comandos rodam num banco de verdade\n   e o jogo confere o RESULTADO. Explorar com SELECT, SHOW TABLES e DESCRIBE é grátis.\n" +
                "•  AWS abre um CONSOLE DA AWS simulado (S3, EC2, IAM, Lambda, CloudWatch, ECR, ECS, CloudFormation):\n   faça a tarefa clicando, como na AWS de verdade, ou pela AWS CLI no CloudShell embaixo.\n" +
                "•  Frontend, React, Flutter, Python e PHP usam a IDE Micorosoft Code (estilo VS Code): escreva o código no editor\n   e aperte Ctrl+Enter (ou F5). Comandos como npm, flutter e pip vão no terminal integrado da IDE.\n" +
                "•  Ctrl+B esconde a lista de desafios e deixa o terminal / a IDE / o console ainda maiores.\n" +
                "•  Acertou? Veja a explicação de cada parte da resposta.\n" +
                "•  Cada área tem 3 níveis: INICIANTE → INTERMEDIÁRIO → PROFISSIONAL. Conclua um nível para liberar o próximo.\n" +
                "•  Cada nível concluído dá uma BADGE (bronze, prata, ouro) com seu nome — dá para baixar como imagem (tecla B).\n" +
                "•  Não sabe? Vá à BIBLIOTECA (porta no fundo da Recepção) e pesquise na documentação — é grátis.\n" +
                "•  No AUDITÓRIO, o telão tem AULAS (Git, internet, segurança, IA, bancos de dados e carreira) com quiz no final.\n" +
                "•  As salas sem desafios têm ATIVIDADES (ícone + roxo): Tech Talk com quiz no Auditório, detector de phishing\n   no SOC, chamados no Help Desk, caça ao bug e fliperama na Sala de Jogos, dilemas de IA na Inovação,\n   missão do jardim, cardápio do Refeitório e networking. Rendem pontos, XP e reputação.\n" +
                "•  Prefere um atalho? Peça dica no terminal ou aos colegas da sala (ícone ?) — custa 25 pontos.\n\n" +
                "CARREIRA\n" +
                "•  Cada desafio dá XP na área dele. Com XP você sobe de cargo: Estagiário(a) → Júnior → Pleno → Sênior → Especialista → Principal.\n" +
                "•  Seu TÍTULO depende da combinação de áreas: Docker + AWS = DevOps/Cloud Engineer, Python + PHP = Dev Backend,\n   Frontend + React = Dev Frontend, Frontend + backend = Fullstack, Flutter = Dev Mobile, Redes + Docker = SRE, SQL + Python = Engenheiro(a) de Dados...\n" +
                "•  DESAFIOS RELÂMPAGO de qualquer área aparecem a qualquer momento. Aceitar é opcional: resolva no tempo\n   para ganhar mais pontos e XP em dobro.\n" +
                "•  Colegas às vezes chegam com SITUAÇÕES (ícone …). Suas escolhas mexem na sua REPUTAÇÃO: ética e\n   colaboração abrem portas; atalhos e fofoca queimam seu filme. Reputação alta dá +10% de XP.\n" +
                "•  Efetivação: chegue a Pleno com 6 badges e fale com a CEO Valéria na Diretoria.\n\n" +
                "FOME, SEDE E MOEDAS\n" +
                "•  Os desafios rendem MOEDAS (o saldo ao lado da estrela). Tudo se compra com elas: comida, bebida, roupas e móveis.\n" +
                "•  FOME e SEDE (canto inferior esquerdo) caem com o tempo. Abaixo de 30% a tela e o texto dos desafios EMBAÇAM;\n   em 0% você também anda mais devagar. Coma no REFEITÓRIO (corredor sul), beba no bebedouro ou na máquina de café,\n   ou use os consumíveis (teclas 1 a 8).\n" +
                "•  SUA SALA (corredor sul, ao lado do Centro de Dados SQL) começa vazia. Na Lojinha, aba \"Minha sala\", compre\n   mesa, computador (abre os terminais e dá mais XP), cadeira (menos fome e sede), piso e decoração.\n" +
                "   Cada badge que você ganha vira um QUADRO na parede da sua sala (a parede da fama).\n" +
                "•  PATINETE (Lojinha, Utilidades): tecla P sobe e desce dele; com ele você anda 50% mais rápido.\n\n" +
                "LOJA, ITENS E OBJETIVOS\n" +
                "•  LOJINHA (banca no corredor): chapéus, roupas, mascotes, vales-dica, equipamentos, lanches e móveis da sua sala.\n   Gastar não diminui sua pontuação do ranking.\n" +
                "•  CONSUMÍVEIS (teclas 1 a 8)  ·  OBJETIVOS (tecla O)  ·  GUARDA-ROUPA (tecla C)\n\n" +
                "PONTUAÇÃO\n" +
                "•  100 pontos por desafio  ·  −25 por dica  ·  −5 por erro  (mínimo 10)  ·  revelar a resposta (após 5 erros) vale 0\n\n" +
                "TECLAS\n" +
                "•  E / Espaço: conversar e interagir   ·   1–9: escolher opção no diálogo   ·   TAB: carreira   ·   B: badges\n" +
                "•  O: objetivos   ·   C: visual   ·   M: som   ·   rodinha: zoom   ·   F1: ajuda   ·   F11: tela cheia   ·   Ctrl+R: reiniciar progresso\n" +
                "•  No terminal: ↑/↓ recuperam comandos, Esc volta ao escritório\n\n" +
                "Em comandos, a ordem das opções não importa. Em código, espaços extras, aspas simples/duplas e ';' finais também não.",
                "Ajuda — Stack Quest", MessageBoxButtons.OK, MessageBoxIcon.Information);
            view.Focus();
        }
    }
}
