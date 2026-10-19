using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DockerQuest
{
    // Estação de trabalho que abre por cima do escritório. Dois visuais:
    //  • Terminal (Docker, Redes, AWS): [desafios] [terminal + linha de comando] [cartão do desafio]
    //  • IDE "Micorosoft Code" (Frontend, React, Flutter, Python, PHP), no estilo do VS Code:
    //    [barra de atividades + explorador] [abas + editor com números de linha / painel SAÍDA-TERMINAL / barra de status] [cartão]
    //    Desafios de código são escritos no editor; os de comando (npm, flutter, pip...) usam o terminal integrado.
    class TerminalPanel : Panel
    {
        public event Action<int, bool> Closed;      // sala, nível concluído agora
        public event Action<int, int> Solved;       // desafio, pontos
        public event Action<int> IncidentSolved;    // pontos do desafio relâmpago

        bool incidentMode;
        Incident incShown;
        Timer incTimer;
        static readonly Color CRed = Art.Hx(0xFF7B72);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr h, int msg, IntPtr w, IntPtr l);
        const int WM_SETREDRAW = 0x0B;

        static readonly Color TermBg = Color.FromArgb(12, 14, 18), Chrome = Color.FromArgb(32, 34, 40), InputBg = Art.Hx(0x161B22);
        static readonly Color CDocker = Art.Hx(0x58A6FF), CPath = Art.Hx(0x39C5CF), CFlag = Art.Hx(0xE3B341), CValue = Art.Hx(0xFFA657),
                              CArg = Art.Hx(0xF0F6FC), CInner = Art.Hx(0xA5D6FF), CPromptUser = Art.Hx(0x3FB950), CPromptPath = Art.Hx(0x58A6FF);
        static readonly Color[] TierColor = { Art.Hx(0xCD7F32), Art.Hx(0xC0C0C0), Art.Hx(0xF5C542) };

        // cores da IDE (tema escuro no estilo do VS Code)
        static readonly Color EdBg = Art.Hx(0x1F1F1F), EdSide = Art.Hx(0x181818), EdBorder = Art.Hx(0x2B2B2B), EdText = Art.Hx(0xCCCCCC),
                              EdMuted = Art.Hx(0x8B8B8B), EdBlue = Art.Hx(0x0078D4), EdStatus = Art.Hx(0x007ACC), EdSel = Art.Hx(0x37373D),
                              EdComment = Art.Hx(0x6A9955), EdLineNo = Art.Hx(0x6E7681);
        static readonly HashSet<string> IdeCats = new HashSet<string> { "front", "react", "flutter", "python", "php" };
        static readonly HashSet<string> ControlWords = new HashSet<string> { "return", "import", "from", "if", "else", "elif", "for", "while", "foreach", "try", "catch", "except", "await", "async", "yield", "export", "default", "require", "include", "as", "in", "with" };

        readonly GameState gs;
        readonly World world;
        readonly float scale;
        int ch, cur;                 // ch = sala da empresa; cur = índice do desafio
        string cat = "docker";
        Category catInfo = Challenges.Cats[0];
        bool ideCat, codeMode;       // ideCat: área usa a IDE; codeMode: desafio atual é escrito no editor
        bool awsMode;                // área de AWS: console de gerenciamento simulado + CloudShell
        readonly AwsSim awsSim = new AwsSim();
        AwsConsoleView awsView;
        bool sqlMode;                // área de SQL: terminal mysql conectado a um banco simulado (cafe365)
        SqlDb sqlDb;
        SqlResult sqlExpected;       // resultado da consulta de referência (calculado uma vez por desafio)
        bool listHidden;             // Ctrl+B: esconde a lista de desafios para o terminal/IDE ficar maior
        RectangleF toggleRect;
        const int TierRow = -10;     // itens da lista: -1 = relâmpago, -10..-12 = cabeçalho de nível
        bool solved, completedNow, navLock, revealedShown, recoloring;
        readonly List<string> history = new List<string>();
        int histPos;

        Panel header, promptBox, preview, valueBar, line, promptLineRef, leftRef, rightRef, centerRef, inputAreaRef, outWrap;
        Panel activity, ideTabs, editorWrap, editorHost, gutter, panelHeader, ideStatus;
        Label lblStatus, lblMentor, lblRole, lblMission, navHeaderRef, editorCue;
        Button btnRun, btnReveal, btnNext, btnHint, btnIdeRun;
        ListBox nav;
        PictureBox portrait;
        RichTextBox rtbMission, rtbOut, input, editor;
        Font fMono, fMonoBold, fMonoBig, fMonoBigBold, fUi, fUiBold, fUiSmall, fH1, fSym, fBlack, fObj, fBig, fChip, fSymBig, fEditor, fTab, fTiny;

        // modelo da linha "o Docker entende:"
        readonly List<KeyValuePair<string, string>> chips = new List<KeyValuePair<string, string>>();
        string previewHint = "";

        int S(float v) { return (int)Math.Round(v * scale); }

        Level CurLv { get { return incidentMode && incShown != null ? incShown.Level : gs.LevelList[cur]; } }
        string CurKind { get { return CurLv.Kind ?? "docker"; } }

        public TerminalPanel(GameState gs, World world, float scale)
        {
            this.gs = gs; this.world = world; this.scale = scale;
            BackColor = Chrome;
            ForeColor = Theme.Text;
            Padding = new Padding(S(2));
            Visible = false;
            DoubleBuffered = true;

            fMono = new Font("Consolas", 12f);
            fMonoBold = new Font("Consolas", 12f, FontStyle.Bold);
            fMonoBig = new Font("Consolas", 14.5f);
            fMonoBigBold = new Font("Consolas", 14.5f, FontStyle.Bold);
            fUi = new Font("Segoe UI", 10.5f);
            fUiBold = new Font("Segoe UI Semibold", 10f);
            fUiSmall = new Font("Segoe UI", 9f);
            fH1 = new Font("Segoe UI Semibold", 15f);
            fSym = new Font("Segoe UI Symbol", 10f);
            fBlack = new Font("Segoe UI Black", 10f);
            fObj = new Font("Segoe UI Semibold", 11.5f);
            fBig = new Font("Consolas", 15f, FontStyle.Bold);
            fChip = new Font("Consolas", 9.5f, FontStyle.Bold);
            fSymBig = new Font("Segoe UI Symbol", 14f, FontStyle.Bold);
            fEditor = new Font("Consolas", 14f);
            fTab = new Font("Segoe UI", 9.5f);
            fTiny = new Font("Segoe UI Semibold", 7.5f);
            BuildUi();
            incTimer = new Timer { Interval = 250 };
            incTimer.Tick += (s, e) =>
            {
                if (!Visible || !incidentMode) return;
                valueBar.Invalidate();
                nav.Invalidate();
                if (gs.Incident == null && !solved) ExpireIncident();
            };
            incTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var p = new Pen(Art.Hx(0x484F58), Math.Max(1, S(1.5f)))) e.Graphics.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape) { CloseTerminal(); return true; }
            if (keyData == (Keys.Control | Keys.L)) { ClearScreen(); return true; }
            if (keyData == (Keys.Control | Keys.B)) { ToggleList(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        Button MkBtn(string text, Color bg, Color fg)
        {
            var b = new Button
            {
                Text = text, FlatStyle = FlatStyle.Flat, BackColor = bg, ForeColor = fg, Font = fUiBold,
                Cursor = Cursors.Hand, UseVisualStyleBackColor = false, TabStop = false
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bg, 0.25f);
            b.EnabledChanged += (s, e) => b.BackColor = b.Enabled ? bg : Theme.Border;
            return b;
        }

        Panel Gap(int h, DockStyle d) { return new Panel { Dock = d, Height = S(h), Width = S(h), BackColor = Color.Transparent }; }

        class DbPanel : Panel { public DbPanel() { DoubleBuffered = true; ResizeRedraw = true; } }

        void BuildUi()
        {
            // ── barra da janela
            header = new DbPanel { Dock = DockStyle.Top, Height = S(40), BackColor = Chrome };
            header.Paint += PaintHeader;
            header.MouseClick += (s, e) => { if (e.X < S(80)) CloseTerminal(); else if (toggleRect.Contains(e.Location)) ToggleList(); };
            header.MouseMove += (s, e) => header.Cursor = e.X < S(80) || toggleRect.Contains(e.Location) ? Cursors.Hand : Cursors.Default;

            // ── lista de desafios / explorador
            var left = new Panel { Dock = DockStyle.Left, Width = S(244), BackColor = Theme.Panel, Padding = new Padding(S(6), S(10), S(4), S(10)) };
            leftRef = left;
            activity = new DbPanel { Dock = DockStyle.Left, Width = S(46), BackColor = EdSide, Visible = false };
            activity.Paint += PaintActivity;
            var navHeader = new Label { Text = "DESAFIOS DA ÁREA", Dock = DockStyle.Top, Height = S(28), Font = fUiBold, ForeColor = Theme.Muted, Padding = new Padding(S(8), 0, 0, 0), TextAlign = ContentAlignment.MiddleLeft, UseMnemonic = false };
            navHeaderRef = navHeader;
            nav = new ListBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, BackColor = Theme.Panel, ForeColor = Theme.Text, DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = S(30), IntegralHeight = false, Font = fUi };
            nav.DrawItem += NavDraw;
            nav.SelectedIndexChanged += NavSelect;
            Program.DarkScroll(nav);
            var navHolder = new Panel { Dock = DockStyle.Fill };
            navHolder.Controls.Add(nav);
            navHolder.Controls.Add(navHeader);
            left.Controls.Add(navHolder);
            left.Controls.Add(activity);

            // ── cartão do desafio (direita)
            var right = new Panel { Dock = DockStyle.Right, Width = S(360), BackColor = Theme.Panel, Padding = new Padding(S(16), S(14), S(16), S(14)) };
            rightRef = right;
            var who = new Panel { Dock = DockStyle.Top, Height = S(88) };
            portrait = new PictureBox { Location = new Point(0, 0), Size = new Size(S(74), S(74)) };
            lblMentor = new Label { Location = new Point(S(86), S(2)), AutoSize = true, Font = new Font("Segoe UI Semibold", 12f), ForeColor = Theme.Yellow, UseMnemonic = false };
            lblRole = new Label { Location = new Point(S(86), S(27)), AutoSize = true, Font = fUiSmall, ForeColor = Art.Hx(0xD9B45A), UseMnemonic = false };
            lblMission = new Label { Location = new Point(S(86), S(47)), AutoSize = true, Font = fUiSmall, ForeColor = Theme.Muted, UseMnemonic = false };
            who.Controls.AddRange(new Control[] { portrait, lblMentor, lblRole, lblMission });

            rtbMission = new RichTextBox
            {
                Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = Theme.Panel, ForeColor = Theme.Text,
                Font = fUi, ScrollBars = RichTextBoxScrollBars.Vertical, TabStop = false, DetectUrls = false, Cursor = Cursors.Default
            };
            Program.DarkScroll(rtbMission);

            var actions = new Panel { Dock = DockStyle.Bottom, Height = S(46 + 8 + 40 + 6 + 40 + 10 + 50) };
            valueBar = new DbPanel { Dock = DockStyle.Top, Height = S(46) };
            valueBar.Paint += PaintValue;
            btnHint = MkBtn("Pedir dica", Color.FromArgb(70, 56, 20), Theme.Yellow);
            btnHint.Dock = DockStyle.Top; btnHint.Height = S(40);
            btnHint.Click += (s, e) => AskHint();
            btnReveal = MkBtn("Revelar resposta", Color.FromArgb(75, 30, 30), Color.FromArgb(255, 160, 150));
            btnReveal.Dock = DockStyle.Top; btnReveal.Height = S(40);
            btnReveal.Click += (s, e) => Reveal();
            btnNext = MkBtn("Próximo desafio  ►", Theme.Docker, Color.White);
            btnNext.Font = new Font("Segoe UI Semibold", 11.5f);
            btnNext.Dock = DockStyle.Bottom; btnNext.Height = S(50);
            btnNext.Click += (s, e) => GoNext();
            actions.Controls.Add(btnNext);
            actions.Controls.Add(Gap(10, DockStyle.Top));
            actions.Controls.Add(btnReveal);
            actions.Controls.Add(Gap(6, DockStyle.Top));
            actions.Controls.Add(btnHint);
            actions.Controls.Add(Gap(8, DockStyle.Top));
            actions.Controls.Add(valueBar);

            right.Controls.Add(rtbMission);
            right.Controls.Add(Gap(10, DockStyle.Top));
            right.Controls.Add(actions);
            right.Controls.Add(who);

            // ── centro: terminal ou IDE
            var center = new Panel { Dock = DockStyle.Fill, BackColor = TermBg, Padding = new Padding(S(16), S(10), S(12), S(10)) };
            centerRef = center;
            rtbOut = new RichTextBox
            {
                Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = TermBg, ForeColor = Theme.Text,
                Font = fMono, ScrollBars = RichTextBoxScrollBars.Vertical, TabStop = false, DetectUrls = false, WordWrap = true
            };
            Program.DarkScroll(rtbOut);
            rtbOut.Click += (s, e) => { if (rtbOut.SelectionLength == 0) FocusInput(); };
            outWrap = new Panel { BackColor = TermBg };
            outWrap.Controls.Add(rtbOut);

            var inputArea = new Panel { Height = S(24 + 52 + 34 + 26), BackColor = TermBg };
            inputAreaRef = inputArea;
            var promptLine = new Panel { Dock = DockStyle.Top, Height = S(24), BackColor = TermBg };
            promptLine.Paint += (s, e) =>
            {
                int x = S(2);
                foreach (var p in new[] { new KeyValuePair<string, Color>(PromptUser, CPromptUser), new KeyValuePair<string, Color>(":", Theme.Text),
                                          new KeyValuePair<string, Color>(PromptPath, CPromptPath), new KeyValuePair<string, Color>("  — " + InputHint, Theme.Muted) })
                {
                    var f = p.Value == Theme.Muted ? fUiSmall : fMonoBold;
                    TextRenderer.DrawText(e.Graphics, p.Key, f, new Point(x, S(4)), p.Value, promptLine.BackColor, TextFormatFlags.NoPadding);
                    x += TextRenderer.MeasureText(e.Graphics, p.Key, f, Size.Empty, TextFormatFlags.NoPadding).Width;
                }
            };
            line = new Panel { Dock = DockStyle.Top, Height = S(52), BackColor = InputBg, Padding = new Padding(S(10), 0, S(6), 0) };
            line.Paint += (s, e) =>
            {
                using (var pen = new Pen(input.Focused ? Theme.Docker : Theme.Border, Math.Max(1, S(2))))
                    e.Graphics.DrawRectangle(pen, 0, 0, line.Width - 1, line.Height - 1);
            };
            promptBox = new Panel { Dock = DockStyle.Left, Width = S(10), BackColor = InputBg };
            promptBox.Paint += PaintPrompt;
            input = new RichTextBox
            {
                Dock = DockStyle.Fill, Multiline = false, BorderStyle = BorderStyle.None, BackColor = InputBg, ForeColor = CArg,
                Font = fMonoBig, DetectUrls = false, ScrollBars = RichTextBoxScrollBars.None
            };
            input.TextChanged += (s, e) => Recolor();
            input.KeyDown += InputKeyDown;
            input.PreviewKeyDown += (s, e) => { if (e.KeyCode == Keys.Tab) e.IsInputKey = true; };
            input.GotFocus += (s, e) => line.Invalidate();
            input.LostFocus += (s, e) => line.Invalidate();
            var inputHolder = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, S(14), 0, 0), BackColor = InputBg };
            inputHolder.Controls.Add(input);
            btnRun = MkBtn("Executar  ⏎", Theme.Green, Color.White);
            btnRun.Dock = DockStyle.Fill;
            btnRun.Click += (s, e) => Submit();
            var runHolder = new Panel { Dock = DockStyle.Right, Width = S(132), Padding = new Padding(S(6), S(8), 0, S(8)), BackColor = InputBg };
            runHolder.Controls.Add(btnRun);
            line.Controls.Add(inputHolder);
            line.Controls.Add(runHolder);
            line.Controls.Add(promptBox);

            preview = new DbPanel { Dock = DockStyle.Top, Height = S(34), BackColor = TermBg };
            preview.Paint += PaintPreview;
            lblStatus = new Label { Dock = DockStyle.Fill, Font = fUiSmall, ForeColor = Theme.Muted, TextAlign = ContentAlignment.MiddleLeft, UseMnemonic = false };
            inputArea.Controls.Add(lblStatus);
            inputArea.Controls.Add(preview);
            inputArea.Controls.Add(line);
            inputArea.Controls.Add(promptLine);
            promptLineRef = promptLine;

            // ── peças da IDE
            ideTabs = new DbPanel { Height = S(62), BackColor = EdSide, Visible = false };
            ideTabs.Paint += PaintTabs;
            btnIdeRun = new Button
            {
                Text = "▶  Executar   Ctrl+Enter", FlatStyle = FlatStyle.Flat, BackColor = Art.Hx(0x2D7D46), ForeColor = Color.White, Font = fUiBold,
                Cursor = Cursors.Hand, UseVisualStyleBackColor = false, TabStop = false, Size = new Size(S(196), S(27)), Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnIdeRun.FlatAppearance.BorderSize = 0;
            btnIdeRun.FlatAppearance.MouseOverBackColor = Art.Hx(0x369152);
            btnIdeRun.Click += (s, e) => { if (solved) GoNext(); else Submit(); };
            ideTabs.Controls.Add(btnIdeRun);
            ideTabs.Resize += (s, e) => btnIdeRun.Location = new Point(ideTabs.Width - btnIdeRun.Width - S(8), S(4));

            editorWrap = new Panel { Height = S(300), BackColor = EdBg, Visible = false };
            gutter = new DbPanel { Dock = DockStyle.Left, Width = S(54), BackColor = EdBg };
            gutter.Paint += PaintGutter;
            editorHost = new Panel { Dock = DockStyle.Fill, BackColor = EdBg, Padding = new Padding(S(6), S(6), 0, 0) };
            editor = new RichTextBox
            {
                Dock = DockStyle.Fill, Multiline = true, BorderStyle = BorderStyle.None, BackColor = EdBg, ForeColor = EdText, Font = fEditor,
                WordWrap = false, ScrollBars = RichTextBoxScrollBars.Both, AcceptsTab = true, DetectUrls = false
            };
            Program.DarkScroll(editor);
            editor.TextChanged += (s, e) => { RecolorEditor(); editorCue.Visible = editor.TextLength == 0; gutter.Invalidate(); };
            editor.VScroll += (s, e) => gutter.Invalidate();
            editor.SelectionChanged += (s, e) => { if (!recoloring) { gutter.Invalidate(); ideStatus.Invalidate(); } };
            editor.KeyDown += EditorKeyDown;
            editor.PreviewKeyDown += (s, e) => { if (e.KeyCode == Keys.Tab) e.IsInputKey = true; };
            editorCue = new Label { AutoSize = true, ForeColor = EdComment, BackColor = EdBg, Font = fEditor, Location = new Point(S(8), S(6)), Cursor = Cursors.IBeam, UseMnemonic = false };
            editorCue.Click += (s, e) => editor.Focus();
            editorHost.Controls.Add(editor);
            editorHost.Controls.Add(editorCue);
            editorCue.BringToFront();
            editorWrap.Controls.Add(editorHost);
            editorWrap.Controls.Add(gutter);

            panelHeader = new DbPanel { Height = S(34), BackColor = EdSide, Visible = false };
            panelHeader.Paint += PaintPanelHeader;
            ideStatus = new DbPanel { Height = S(24), BackColor = EdStatus, Visible = false };
            ideStatus.Paint += PaintStatus;

            // posicionamento manual (ordem fixa de cima para baixo: abas, editor, cabeçalho do painel, saída, entrada, status)
            awsView = new AwsConsoleView(awsSim, scale) { Visible = false };
            awsSim.Event += OnAwsEvent;
            center.Controls.AddRange(new Control[] { outWrap, panelHeader, editorWrap, ideTabs, inputArea, ideStatus, awsView });
            center.Layout += (s, e) => LayoutCenter();

            Controls.Add(center);
            Controls.Add(right);
            Controls.Add(left);
            Controls.Add(header);
            Resize += (s, e) => ApplySideWidths();
        }

        // larguras das colunas laterais proporcionais à janela: o centro (terminal, IDE ou console) fica com o resto
        void ApplySideWidths()
        {
            if (leftRef == null) return;
            int lw = Math.Max(S(190), Math.Min(S(250), (int)(Width * 0.15f)));
            int rw = Math.Max(S(280), Math.Min(S(360), (int)(Width * 0.22f)));
            // telas pequenas (notebook com zoom do Windows): o centro, onde se digita e envia, nunca fica espremido
            int minCenter = S(480), act = ideCat ? S(46) : 0;
            int center = Width - (listHidden ? 0 : lw) - rw - act;
            if (center < minCenter) { rw = Math.Max(S(220), rw - (minCenter - center)); center = Width - (listHidden ? 0 : lw) - rw - act; }
            bool hideList = listHidden || center < minCenter;
            leftRef.Visible = !hideList || ideCat;
            leftRef.Width = (hideList ? 0 : lw) + act;
            if (rightRef.Width != rw) rightRef.Width = rw;
        }

        void ToggleList()
        {
            listHidden = !listHidden;
            ApplySideWidths();
            header.Invalidate();
            SetStatus(listHidden ? "Lista de desafios escondida (Ctrl+B mostra de novo)." : "Lista de desafios visível (Ctrl+B esconde para ganhar espaço).", Theme.Muted);
            FocusInput();
        }

        // banco novo para cada desafio de SQL (o aluno pode explorar e errar à vontade)
        void PrepareSql(Level lv)
        {
            if (lv.Kind != "sql") return;
            sqlDb = SqlSample.Fresh();
            sqlExpected = null;
        }

        void FocusInput() { if (codeMode) editor.Focus(); else input.Focus(); }

        // prepara a conta simulada para o desafio de AWS (cria/remove o que for preciso) e volta à página inicial do console
        void PrepareAws(Level lv)
        {
            if (lv.Kind != "aws" || lv.Task == null) return;
            AwsTask task;
            awsSim.Reset();   // conta limpa: o que foi feito em outra tarefa não atrapalha esta
            if (AwsTasks.All.TryGetValue(lv.Task, out task)) task.Setup(awsSim);
            awsView.Navigate("home", null, false);
            panelHeader.Invalidate();
        }

        void LayoutCenter()
        {
            var p = centerRef.Padding;
            int x = p.Left, w = Math.Max(10, centerRef.ClientSize.Width - p.Left - p.Right);
            int y0 = p.Top, y1 = centerRef.ClientSize.Height - p.Bottom;
            if (ideStatus.Visible) { ideStatus.SetBounds(x, y1 - S(24), w, S(24)); y1 -= S(24); }
            int ih = inputAreaRef.Height;
            inputAreaRef.SetBounds(x, y1 - ih, w, ih);
            y1 -= ih + (ideCat ? 0 : S(8));
            if (awsView.Visible)
            {
                // o console ocupa 70%, mas sempre sobra espaço para a saída do CloudShell (o campo de digitar fica embaixo)
                int ah = Math.Max(Math.Min(S(260), y1 - y0 - S(34) - S(70)), (int)((y1 - y0 - S(34)) * 0.70f));
                awsView.SetBounds(x, y0, w, ah);
                y0 += ah;
            }
            if (ideTabs.Visible) { ideTabs.SetBounds(x, y0, w, S(62)); y0 += S(62); }
            int ph = panelHeader.Visible ? S(34) : 0;
            if (editorWrap.Visible)
            {
                int eh = Math.Max(S(140), (int)((y1 - y0 - ph) * 0.58f));
                editorWrap.SetBounds(x, y0, w, eh);
                y0 += eh;
            }
            if (panelHeader.Visible) { panelHeader.SetBounds(x, y0, w, ph); y0 += ph; }
            outWrap.SetBounds(x, y0, w, Math.Max(S(40), y1 - y0));
        }

        void LayoutEditor() { LayoutCenter(); }

        // ─────────────────────────── modo terminal × IDE ───────────────────────────

        void ApplyMode()
        {
            codeMode = ideCat && (CurKind == "code" || CurKind == "html");
            awsMode = cat == "aws";
            sqlMode = cat == "sql";
            awsView.Visible = awsMode;
            SuspendLayout();
            centerRef.SuspendLayout();
            activity.Visible = ideCat;
            ApplySideWidths();
            leftRef.Padding = ideCat ? new Padding(0, 0, 0, 0) : new Padding(S(6), S(10), S(4), S(10));
            Color sideBg = ideCat ? EdSide : Theme.Panel;
            leftRef.BackColor = nav.BackColor = sideBg;
            navHeaderRef.Text = ideCat ? "EXPLORADOR:  " + ProjectName.ToUpperInvariant() : "DESAFIOS DA ÁREA";
            navHeaderRef.ForeColor = ideCat ? EdMuted : Theme.Muted;
            navHeaderRef.Font = ideCat ? fTiny : fUiBold;
            navHeaderRef.Padding = new Padding(ideCat ? S(12) : S(8), 0, 0, 0);
            rightRef.BackColor = rtbMission.BackColor = sideBg;
            ideTabs.Visible = editorWrap.Visible = codeMode;
            panelHeader.Visible = ideCat || awsMode;
            ideStatus.Visible = ideCat;
            line.Visible = promptLineRef.Visible = !codeMode;
            inputAreaRef.Height = codeMode ? S(34 + 26) : S(24 + 52 + 34 + 26);
            Color outBg = ideCat ? EdSide : TermBg;
            centerRef.Padding = ideCat || awsMode ? Padding.Empty : new Padding(S(16), S(10), S(12), S(10));
            centerRef.BackColor = outBg;
            outWrap.Padding = ideCat || awsMode ? new Padding(S(16), S(8), S(6), S(2)) : Padding.Empty;
            outWrap.BackColor = rtbOut.BackColor = outBg;
            inputAreaRef.Padding = ideCat || awsMode ? new Padding(S(16), 0, S(12), S(2)) : Padding.Empty;
            if (awsMode) inputAreaRef.Height = S(24 + 52 + 26);
            inputAreaRef.BackColor = preview.BackColor = promptLineRef.BackColor = lblStatus.BackColor = outBg;
            header.BackColor = ideCat ? EdSide : Chrome;
            BackColor = ideCat ? EdBorder : Chrome;
            centerRef.ResumeLayout(true);
            ResumeLayout(true);
            LayoutEditor();
            foreach (Control c in new Control[] { header, ideTabs, panelHeader, ideStatus, nav, promptBox, promptLineRef, activity, gutter }) c.Invalidate();
        }

        // ── nomes de projeto/arquivos usados na IDE
        string ProjectName
        {
            get
            {
                switch (cat)
                {
                    case "front": return "cafe365-web";
                    case "react": return "cafe365-painel";
                    case "flutter": return "cafe_app";
                    case "python": return "dados-vendas";
                    case "php": return "api-pedidos";
                    default: return cat;
                }
            }
        }

        static bool IsCss(string a)
        {
            a = a.Trim();
            if (a.StartsWith("@media")) return true;
            int b = a.IndexOf('{');
            if (b < 0) return false;
            string pre = a.Substring(0, b);
            return pre.IndexOf('(') < 0 && pre.IndexOf('=') < 0 && !pre.Contains("function");
        }

        static string Ext(Level lv)
        {
            if (lv.Kind == "cmd" || lv.Kind == "docker") return "sh";
            if (lv.Kind == "text") return "md";
            switch (lv.Cat)
            {
                case "front": return lv.Kind == "html" ? "html" : IsCss(lv.Answers[0]) ? "css" : "js";
                case "react": return "jsx";
                case "flutter": return "dart";
                case "python": return "py";
                case "php": return "php";
                default: return "txt";
            }
        }

        static string FileName(Level lv)
        {
            var words = new List<string>();
            var sb = new System.Text.StringBuilder();
            foreach (char c in Checker.Fold(lv.Title) + " ")
            {
                if (char.IsLetterOrDigit(c)) sb.Append(c);
                else if (sb.Length > 0) { words.Add(sb.ToString()); sb.Clear(); }
            }
            words = words.Where(w => w.Length > 2 || words.Count <= 2).Take(4).ToList();
            if (words.Count == 0) words.Add("desafio");
            string ext = Ext(lv), name;
            if (lv.Cat == "react" && ext == "jsx") name = string.Concat(words.Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1)));
            else if (lv.Cat == "python" || lv.Cat == "flutter") name = string.Join("_", words);
            else name = string.Join("-", words);
            if (name.Length > 24) name = name.Substring(0, 24).TrimEnd('-', '_');
            return name + "." + ext;
        }

        string Folder(Level lv)
        {
            string ext = Ext(lv);
            switch (cat)
            {
                case "front": return ext == "css" ? "css" : ext == "js" ? "js" : "";
                case "react": return "src › components";
                case "flutter": return "lib";
                case "python": return "scripts";
                case "php": return "src";
                default: return "";
            }
        }

        static string LangName(string ext)
        {
            switch (ext)
            {
                case "py": return "Python";
                case "js": return "JavaScript";
                case "css": return "CSS";
                case "html": return "HTML";
                case "jsx": return "JavaScript React";
                case "dart": return "Dart";
                case "php": return "PHP";
                case "md": return "Markdown";
                default: return "Shell";
            }
        }

        static string CommentFor(string ext, string text)
        {
            switch (ext)
            {
                case "py": return "# " + text;
                case "html": return "<!-- " + text + " -->";
                case "css": return "/* " + text + " */";
                default: return "// " + text;
            }
        }

        string RunLine(Level lv)
        {
            string ext = Ext(lv), f = FileName(lv), dir = Folder(lv).Replace(" › ", "/");
            string path = dir.Length > 0 ? dir + "/" + f : f;
            switch (ext)
            {
                case "py": return "python " + path;
                case "js": return "node " + path;
                case "jsx": return "npm test -- " + f;
                case "dart": return "flutter test " + path;
                case "php": return "php " + path;
                case "css": return "npx stylelint " + path;
                case "html": return "npx html-validate " + path;
                default: return path;
            }
        }

        static Color ExtColor(string ext)
        {
            switch (ext)
            {
                case "html": return Art.Hx(0xE44D26);
                case "css": return Art.Hx(0x42A5F5);
                case "js": return Art.Hx(0xF1E05A);
                case "jsx": return Art.Hx(0x61DAFB);
                case "dart": return Art.Hx(0x40C4FF);
                case "py": return Art.Hx(0xFFD43B);
                case "php": return Art.Hx(0x8892BF);
                case "md": return Art.Hx(0x519ABA);
                default: return Art.Hx(0x89E051);
            }
        }

        static string ExtLabel(string ext)
        {
            switch (ext)
            {
                case "html": return "<>";
                case "css": return "#";
                case "js": return "JS";
                case "jsx": return "JSX";
                case "dart": return "◆";
                case "py": return "py";
                case "php": return "php";
                case "md": return "i";
                default: return ">_";
            }
        }

        void ExtBadge(Graphics g, string ext, float x, float cy)
        {
            string l = ExtLabel(ext);
            var f = ext == "dart" ? fSym : fChip;
            var sz = g.MeasureString(l, f);
            g.DrawString(l, f, Art.B(ExtColor(ext)), x + (S(22) - sz.Width) / 2, cy - sz.Height / 2);
        }

        // ─────────────────────────── pintura customizada ───────────────────────────

        void PaintHeader(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            var colors = new[] { 0xFF5F57, 0xFEBC2E, 0x28C840 };
            for (int i = 0; i < 3; i++) g.FillEllipse(Art.B(Art.Hx(colors[i])), S(14) + i * S(22), S(13), S(14), S(14));
            string hint = "Esc · voltar ao escritório";
            var sz = g.MeasureString(hint, fUiSmall);
            float hx = header.Width - sz.Width - S(14);
            g.DrawString(hint, fUiSmall, Art.B(Theme.Muted), hx, (header.Height - sz.Height) / 2);
            string tg = (listHidden ? "▸ mostrar lista" : "◂ esconder lista") + "  Ctrl+B";
            var tsz0 = g.MeasureString(tg, fUiSmall);
            toggleRect = new RectangleF(hx - tsz0.Width - S(26), S(7), tsz0.Width + S(12), header.Height - S(14));
            Art.RR(g, Art.A(Color.White, 18), toggleRect.X, toggleRect.Y, toggleRect.Width, toggleRect.Height, S(5));
            g.DrawString(tg, fUiSmall, Art.B(Theme.Text), toggleRect.X + S(6), (header.Height - tsz0.Height) / 2);
            if (awsMode)
            {
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString("Console de Gerenciamento da AWS (simulado)  —  " + Story.SectorOf(ch).Name + "  ·  CloudShell embaixo", fUiBold, Art.B(Theme.Text), new RectangleF(0, 0, header.Width, header.Height), sf);
                return;
            }
            if (!ideCat)
            {
                string title = sqlMode ? "mysql -u estagio -p " + SqlDb.DbName + "  —  " + Story.SectorOf(ch).Name + "  ·  MySQL 8.0"
                                       : "estagio@micorosoft: " + PromptPath + "  —  " + Story.SectorOf(ch).Name + "  ·  " + catInfo.Name;
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(title, fUiBold, Art.B(Theme.Text), new RectangleF(0, 0, header.Width, header.Height), sf);
                return;
            }
            // barra de título da IDE: ícone, menus e título centralizado
            float x = S(88);
            Art.RR(g, EdBlue, x, S(11), S(18), S(18), S(4));
            g.DrawString("</>", fTiny, Art.B(Color.White), x + S(0.5f), S(13.5f));
            x += S(28);
            foreach (var m in new[] { "Arquivo", "Editar", "Seleção", "Ver", "Executar", "Terminal", "Ajuda" })
            {
                g.DrawString(m, fUiSmall, Art.B(EdText), x, S(11));
                x += g.MeasureString(m, fUiSmall).Width + S(6);
            }
            string file = codeMode ? FileName(CurLv) : "Terminal";
            string t2 = file + " — " + ProjectName + " — Micorosoft Code";
            var tsz = g.MeasureString(t2, fUiSmall);
            float tx = Math.Max(x + S(16), (header.Width - tsz.Width) / 2);
            Art.RR(g, Art.Hx(0x2A2A2A), tx - S(14), S(8), tsz.Width + S(28), S(24), S(6));
            g.DrawString(t2, fUiSmall, Art.B(EdText), tx, S(11));
        }

        void PaintActivity(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color dim = Art.Hx(0x858585);
            float cx = activity.Width / 2f, y = S(14), u = S(22);
            // arquivos (ativo)
            g.FillRectangle(Art.B(EdBlue), 0, y - S(4), S(2), u + S(8));
            using (var p = new Pen(Color.White, Math.Max(1, S(1.6f))))
            {
                g.DrawRectangle(p, cx - u * 0.32f, y + u * 0.08f, u * 0.5f, u * 0.7f);
                g.DrawRectangle(p, cx - u * 0.16f, y - u * 0.08f + u * 0.08f, u * 0.5f, u * 0.7f);
            }
            using (var p = new Pen(dim, Math.Max(1, S(1.6f))))
            {
                // pesquisa
                y += S(48);
                g.DrawEllipse(p, cx - u * 0.36f, y, u * 0.5f, u * 0.5f);
                g.DrawLine(p, cx + u * 0.08f, y + u * 0.46f, cx + u * 0.34f, y + u * 0.74f);
                // controle de versão
                y += S(48);
                g.DrawEllipse(p, cx - u * 0.32f, y, u * 0.2f, u * 0.2f);
                g.DrawEllipse(p, cx - u * 0.32f, y + u * 0.6f, u * 0.2f, u * 0.2f);
                g.DrawEllipse(p, cx + u * 0.14f, y + u * 0.18f, u * 0.2f, u * 0.2f);
                g.DrawLine(p, cx - u * 0.22f, y + u * 0.2f, cx - u * 0.22f, y + u * 0.6f);
                g.DrawBezier(p, cx + u * 0.24f, y + u * 0.38f, cx + u * 0.24f, y + u * 0.55f, cx - u * 0.22f, y + u * 0.45f, cx - u * 0.22f, y + u * 0.6f);
                // executar e depurar
                y += S(48);
                g.DrawPolygon(p, new[] { new PointF(cx - u * 0.28f, y), new PointF(cx + u * 0.34f, y + u * 0.38f), new PointF(cx - u * 0.28f, y + u * 0.76f) });
                // extensões
                y += S(48);
                float b = u * 0.3f;
                g.DrawRectangle(p, cx - b - S(1), y + b + S(2), b, b);
                g.DrawRectangle(p, cx + S(1), y + b + S(2), b, b);
                g.DrawRectangle(p, cx - b - S(1), y, b, b);
                g.DrawRectangle(p, cx + S(4), y - S(3), b, b);
                // conta e configurações (embaixo)
                float by = activity.Height - S(84);
                g.DrawEllipse(p, cx - u * 0.18f, by, u * 0.36f, u * 0.36f);
                g.DrawArc(p, cx - u * 0.36f, by + u * 0.42f, u * 0.72f, u * 0.6f, 180, 180);
                by += S(44);
                g.DrawEllipse(p, cx - u * 0.3f, by, u * 0.6f, u * 0.6f);
                g.DrawEllipse(p, cx - u * 0.1f, by + u * 0.2f, u * 0.2f, u * 0.2f);
            }
        }

        void PaintTabs(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            if (!codeMode) return;
            var lv = CurLv;
            string ext = Ext(lv), file = FileName(lv);
            float th = S(35);
            g.FillRectangle(Art.B(EdBg), 0, th, ideTabs.Width, ideTabs.Height - th);
            g.DrawLine(Art.P(EdBorder, 1), 0, th - 1, ideTabs.Width, th - 1);
            // aba ativa
            var fsz = g.MeasureString(file, fTab);
            float tw = fsz.Width + S(70);
            g.FillRectangle(Art.B(EdBg), 0, 0, tw, th);
            g.FillRectangle(Art.B(incidentMode ? Art.Hx(0xF85149) : EdBlue), 0, 0, tw, S(2));
            g.DrawLine(Art.P(EdBorder, 1), tw, 0, tw, th);
            ExtBadge(g, ext, S(8), th / 2);
            g.DrawString(file, fTab, Art.B(Color.White), S(32), (th - fsz.Height) / 2);
            bool dirty = editor.TextLength > 0 && !solved;
            if (dirty) g.FillEllipse(Art.B(EdText), tw - S(24), th / 2 - S(4), S(8), S(8));
            else g.DrawString("×", fUi, Art.B(EdMuted), tw - S(27), th / 2 - S(11));
            // aba inativa
            string other = incidentMode ? "ALERTA.md" : "LEIA-ME.md";
            var osz = g.MeasureString(other, fTab);
            ExtBadge(g, "md", tw + S(8), th / 2);
            g.DrawString(other, fTab, Art.B(EdMuted), tw + S(32), (th - osz.Height) / 2);
            g.DrawLine(Art.P(EdBorder, 1), tw + osz.Width + S(60), 0, tw + osz.Width + S(60), th);
            // caminho (breadcrumb)
            string folder = Folder(lv);
            string crumb = ProjectName + "  ›  " + (folder.Length > 0 ? folder.Replace(" › ", "  ›  ") + "  ›  " : "");
            float cx = S(16), cy = th + S(5);
            g.DrawString(crumb, fUiSmall, Art.B(EdMuted), cx, cy);
            cx += g.MeasureString(crumb, fUiSmall).Width;
            ExtBadge(g, ext, cx - S(4), cy + S(9));
            g.DrawString(file, fUiSmall, Art.B(EdText), cx + S(18), cy);
        }

        void PaintGutter(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            int lines = Math.Max(1, editor.Lines.Length + (editor.Text.EndsWith("\n") ? 1 : 0));
            int curLine = editor.GetLineFromCharIndex(editor.SelectionStart);
            int top = editorHost.Padding.Top;
            for (int i = 0; i < lines; i++)
            {
                int idx = editor.GetFirstCharIndexFromLine(i);
                int y = idx >= 0 ? editor.GetPositionFromCharIndex(idx).Y : i * fEditor.Height;
                if (idx < 0 && i > 0) { int prev = editor.GetFirstCharIndexFromLine(i - 1); if (prev >= 0) y = editor.GetPositionFromCharIndex(prev).Y + fEditor.Height; }
                y += top;
                if (y < -fEditor.Height || y > gutter.Height) continue;
                string n = (i + 1).ToString();
                var sz = TextRenderer.MeasureText(n, fEditor);
                TextRenderer.DrawText(g, n, fEditor, new Point(gutter.Width - sz.Width - S(6), y), i == curLine ? Art.Hx(0xC6C6C6) : EdLineNo, EdBg);
            }
        }

        void PaintPanelHeader(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.DrawLine(Art.P(EdBorder, 1), 0, 0, panelHeader.Width, 0);
            if (awsMode)
            {
                g.FillRectangle(Art.B(Art.Hx(0x232F3E)), 0, 0, panelHeader.Width, panelHeader.Height);
                var cs = g.MeasureString("CloudShell", fUiBold);
                g.DrawString(">_", fUiBold, Art.B(Art.Hx(0xFF9900)), S(14), (panelHeader.Height - cs.Height) / 2);
                g.DrawString("CloudShell", fUiBold, Art.B(Color.White), S(36), (panelHeader.Height - cs.Height) / 2);
                g.DrawString(awsSim.Region + "  ·  AWS CLI pronta (opcional: tudo também pode ser feito pelo console acima)", fUiSmall, Art.B(Art.Hx(0xAAB7B8)), S(48) + cs.Width, (panelHeader.Height - cs.Height) / 2 + S(1));
                return;
            }
            string active = codeMode ? "SAÍDA" : "TERMINAL";
            int probs = incidentMode ? gs.IncWrong : gs.WrongOf(cur);
            float x = S(16);
            foreach (var t in new[] { "PROBLEMAS", "SAÍDA", "CONSOLE DE DEPURAÇÃO", "TERMINAL" })
            {
                bool on = t == active;
                var sz = g.MeasureString(t, fTiny);
                g.DrawString(t, fTiny, Art.B(on ? Color.White : EdMuted), x, (panelHeader.Height - sz.Height) / 2);
                if (on) g.FillRectangle(Art.B(Color.White), x, panelHeader.Height - S(5), sz.Width, S(1.5f));
                x += sz.Width;
                if (t == "PROBLEMAS" && probs > 0)
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    Art.RR(g, Art.Hx(0x5A5A5A), x + S(3), panelHeader.Height / 2 - S(8), S(18), S(16), S(8));
                    var ns = g.MeasureString(probs.ToString(), fTiny);
                    g.DrawString(probs.ToString(), fTiny, Art.B(Color.White), x + S(12) - ns.Width / 2, panelHeader.Height / 2 - ns.Height / 2);
                    x += S(22);
                }
                x += S(22);
            }
        }

        void PaintStatus(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            if (incidentMode) g.Clear(Art.Hx(0xC5383F));
            float y = (ideStatus.Height - g.MeasureString("X", fUiSmall).Height) / 2;
            // ramo do git
            float x = S(10);
            using (var p = new Pen(Color.White, Math.Max(1, S(1.2f))))
            {
                g.DrawEllipse(p, x, S(5), S(4), S(4));
                g.DrawEllipse(p, x, S(15), S(4), S(4));
                g.DrawEllipse(p, x + S(7), S(7), S(4), S(4));
                g.DrawLine(p, x + S(2), S(9), x + S(2), S(15));
            }
            g.DrawString("main", fUiSmall, Art.B(Color.White), x + S(14), y);
            x += S(14) + g.MeasureString("main", fUiSmall).Width + S(14);
            int probs = incidentMode ? gs.IncWrong : gs.WrongOf(cur);
            string left = "⊗ " + probs + "   ⚠ 0" + (incidentMode ? "      ▲ DESAFIO RELÂMPAGO" : solved ? (codeMode ? "      ✔ testes passaram" : "      ✔ comando ok") : "");
            g.DrawString(left, fUiSmall, Art.B(Color.White), x, y);
            string lang = codeMode ? LangName(Ext(CurLv)) : "Shell";
            string pos = "";
            if (codeMode)
            {
                int ln = editor.GetLineFromCharIndex(editor.SelectionStart);
                int col = editor.SelectionStart - Math.Max(0, editor.GetFirstCharIndexFromLine(ln)) + 1;
                pos = "Ln " + (ln + 1) + ", Col " + col + "      Espaços: 4      UTF-8      ";
            }
            string right = pos + lang + "      Micorosoft Code";
            var rs = g.MeasureString(right, fUiSmall);
            g.DrawString(right, fUiSmall, Art.B(Color.White), ideStatus.Width - rs.Width - S(12), y);
        }

        string PromptPath { get { return awsMode ? "~" : sqlMode ? SqlDb.DbName : "~/" + (ideCat ? ProjectName : cat); } }
        string PromptUser { get { return awsMode ? "cloudshell-user@ip-10-0-12-34" : sqlMode ? "mysql" : "estagio@micorosoft"; } }
        string PromptSymbol { get { string k = CurKind; return k == "docker" || k == "cmd" || k == "aws" ? "$" : catInfo.Prompt; } }

        string InputHint
        {
            get
            {
                switch (CurKind)
                {
                    case "docker": return "digite seu comando docker abaixo";
                    case "aws": return "CloudShell: comandos aws (opcional) — ou faça a tarefa pelo console acima";
                    case "sql": return "digite um comando SQL abaixo (ENTER executa)";
                    case "cmd": return ideCat ? "terminal integrado: digite o comando abaixo" : "digite o comando completo abaixo";
                    case "html": return "escreva a tag HTML abaixo";
                    case "text": return "responda abaixo";
                    default: return "escreva a linha de código abaixo";
                }
            }
        }

        void PaintPrompt(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            int x = 0, y = S(13);
            var parts = new[] { new KeyValuePair<string, Color>(PromptSymbol, CPromptUser) };
            foreach (var p in parts)
            {
                TextRenderer.DrawText(g, p.Key, fMonoBigBold, new Point(x, y), p.Value, InputBg, TextFormatFlags.NoPadding);
                x += TextRenderer.MeasureText(g, p.Key, fMonoBigBold, Size.Empty, TextFormatFlags.NoPadding).Width;
            }
            int w = x + S(12);
            if (promptBox.Width != w) promptBox.Width = w;
        }

        void PaintPreview(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            float x = S(2), h = S(24), y = (preview.Height - h) / 2;
            if (chips.Count == 0)
            {
                g.DrawString(previewHint, fUiSmall, Art.B(Theme.Muted), x, y + S(4));
                return;
            }
            g.DrawString("o Docker entende:", fUiSmall, Art.B(Theme.Muted), x, y + S(4));
            x += g.MeasureString("o Docker entende:", fUiSmall).Width + S(6);
            foreach (var c in chips)
            {
                Color col = c.Key == "COMANDO" ? CPath : c.Key == "OPÇÕES" ? CFlag : c.Key == "ERRO" ? Theme.Red : c.Key == "DENTRO DO CONTAINER" ? CInner : CArg;
                string label = c.Key + " ", val = c.Value;
                var ls = g.MeasureString(label, fUiSmall);
                float room = preview.Width - S(4) - x - ls.Width - S(12);
                if (room < S(40)) { g.DrawString("…", fChip, Art.B(Theme.Muted), x, y + S(3)); break; }
                while (val.Length > 1 && g.MeasureString(val, fChip).Width > room) val = val.Substring(0, val.Length - 2) + "…";
                var vs = g.MeasureString(val, fChip);
                float w = ls.Width + vs.Width + S(12);
                Art.RR(g, Art.A(col, 30), x, y, w, h, S(5));
                Art.RRO(g, Art.A(col, 120), 1, x, y, w, h, S(5));
                g.DrawString(label, fUiSmall, Art.B(Art.A(col, 190)), x + S(6), y + S(4));
                g.DrawString(val, fChip, Art.B(col), x + S(6) + ls.Width, y + S(4));
                x += w + S(6);
            }
        }

        void PaintValue(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            if (incidentMode)
            {
                float bwi = valueBar.Width, byi = S(22);
                if (solved || gs.Incident == null)
                {
                    g.DrawString("DESAFIO RELÂMPAGO RESOLVIDO", fUiSmall, Art.B(Theme.Green), 0, 0);
                    Art.RR(g, Theme.Green, 0, byi, bwi, S(9), S(4));
                    return;
                }
                int s = (int)Math.Ceiling(Math.Max(0, gs.IncTime));
                g.DrawString("TEMPO RESTANTE  " + (s / 60) + ":" + (s % 60).ToString("00"), fUiBold, Art.B(s <= 20 ? CRed : Theme.Text), 0, 0);
                string iv = "vale " + gs.IncidentValue() + " pts";
                var ivs = g.MeasureString(iv, fUiBold);
                g.DrawString(iv, fUiBold, Art.B(Theme.Yellow), bwi - ivs.Width, 0);
                Art.RR(g, Art.Hx(0x5C2430), 0, byi, bwi, S(9), S(4));
                Art.RR(g, Art.Hx(0xF85149), 0, byi, Math.Max(S(9), bwi * gs.IncTime / Math.Max(1, gs.IncTotal)), S(9), S(4));
                g.DrawString("−" + Incidents.WrongCost + " por erro · XP em dobro · bônus por rapidez", fUiSmall, Art.B(Theme.Muted), 0, byi + S(10));
                return;
            }
            int v = solved ? (gs.Prog.Done(cur) ? gs.Prog.Points[cur] : 0) : gs.CurrentValue(cur);
            string label = solved ? "DESAFIO CONCLUÍDO" : "VALENDO AGORA";
            g.DrawString(label, fUiSmall, Art.B(Theme.Muted), 0, 0);
            string val = v + " / " + GameState.MaxPoints + " pts";
            var vs = g.MeasureString(val, fUiBold);
            g.DrawString(val, fUiBold, Art.B(Theme.Yellow), valueBar.Width - vs.Width, 0);
            float bw = valueBar.Width, by = S(22);
            Art.RR(g, Theme.Border, 0, by, bw, S(9), S(4));
            Color bc = v >= 80 ? Theme.Green : v >= 50 ? Theme.Yellow : Theme.Red;
            Art.RR(g, bc, 0, by, Math.Max(S(9), bw * v / GameState.MaxPoints), S(9), S(4));
            if (!solved) g.DrawString("−" + GameState.WrongCost + " por erro · −" + GameState.HintCost + " por dica · Biblioteca é grátis", fUiSmall, Art.B(Theme.Muted), 0, by + S(10));
        }

        // ─────────────────────────── abrir / fechar ───────────────────────────

        public void Open(int room)
        {
            ch = room;
            catInfo = Challenges.CatOfRoom(room) ?? Challenges.Cats[0];
            cat = catInfo.Id;
            ideCat = IdeCats.Contains(cat);
            completedNow = false;
            incidentMode = false;
            bool incHere = gs.Incident != null && gs.Incident.Room == ch;
            RebuildNav(incHere);
            Visible = true;
            BringToFront();
            PerformLayout();

            var mentor = world.Mentors[ch] ?? world.Mentors[0];
            lblMentor.Text = mentor.Name;
            lblRole.Text = mentor.Role;
            var bmp = new Bitmap(Math.Max(1, portrait.Width), Math.Max(1, portrait.Height));
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(ideCat ? EdSide : Theme.Panel);
                using (var path = Art.RoundPath(0, 0, bmp.Width, bmp.Height, S(10)))
                {
                    g.SetClip(path);
                    Art.Portrait(g, new RectangleF(0, 0, bmp.Width, bmp.Height), mentor.Look, 0);
                }
            }
            var old = portrait.Image;
            portrait.Image = bmp;
            if (old != null) old.Dispose();

            int lv;
            if (!gs.TerminalLevel.TryGetValue(cat, out lv) || gs.LevelList[lv].Cat != cat) lv = gs.DefaultLevel(cat);
            if ((gs.Prog.Done(lv) && !gs.CatDone(cat)) || !gs.Unlocked(lv)) lv = gs.DefaultLevel(cat);
            cur = lv;
            ApplyMode();
            rtbOut.Clear();
            Banner();
            if (incHere) LoadIncident();
            else
            {
                if (gs.Incident != null)
                    Put("▲ Há um DESAFIO RELÂMPAGO de " + Challenges.Cat(gs.Incident.Cat).Name + " esperando na área " + Story.SectorOf(gs.Incident.Room).Name +
                        "! Ele só pode ser resolvido lá. Corra!\n\n", CRed, fMonoBold);
                LoadLevel(lv);
            }
            FocusInput();
        }

        void RebuildNav(bool withIncident)
        {
            navLock = true;
            nav.Items.Clear();
            if (withIncident) nav.Items.Add(-1);
            for (int t = 0; t < 3; t++)
            {
                nav.Items.Add(TierRow - t);
                foreach (var i in gs.TierLevels(cat, t)) nav.Items.Add(i);
            }
            navLock = false;
        }

        // ─────────────────────────── desafio relâmpago ───────────────────────────

        void LoadIncident()
        {
            if (gs.Incident == null) return;
            incidentMode = true;
            solved = false;
            incShown = gs.Incident;
            ApplyMode();
            PrepareAws(incShown.Level);
            PrepareSql(incShown.Level);
            editor.ReadOnly = false;
            editor.Text = "";
            UpdateCue();
            RenderIncident();
            IncidentHeader();
            SetInput("");
            Recolor();
            promptBox.Invalidate();
            promptLineRef.Invalidate();
            SetStatus("Rápido! Cada erro custa " + Incidents.WrongCost + " pontos e o bônus de tempo diminui a cada segundo.", CRed);
            UpdateButtons();
            SyncNav();
            FocusInput();
        }

        void RenderIncident()
        {
            var inc = incShown;
            lblMission.Text = "Relâmpago · " + Challenges.Cat(inc.Cat).Name + " · " + Challenges.TierNames[inc.Tier] + "\nreportado por " + inc.Reporter;
            rtbMission.Clear();
            Put(rtbMission, "DESAFIO RELÂMPAGO\n", CRed, fBlack);
            Put(rtbMission, inc.Title + "\n", Theme.Bright, fH1);
            Put(rtbMission, inc.Reporter + ": \"" + inc.Alert + "\"\n\n", Theme.Text, fUi);
            Put(rtbMission, "► ", Theme.Yellow, fSym);
            Put(rtbMission, "OBJETIVO\n", Theme.Yellow, fBlack);
            Put(rtbMission, inc.Level.Objective + "\n\n", Theme.Bright, fObj);
            Put(rtbMission, "Aqui não há dicas: confie no que você aprendeu (ou corra até a Biblioteca, se der tempo!). Acertar vale XP em dobro.", Theme.Muted, fUiSmall);
            rtbMission.SelectionStart = 0;
            rtbMission.ScrollToCaret();
        }

        void IncidentHeader()
        {
            Put("══════════════════════════════════════════════════════════\n", CRed, fMono);
            Put(" ▲ ", CRed, fSym); Put("DESAFIO RELÂMPAGO · " + incShown.Title + "\n", CRed, fMonoBold);
            Put("══════════════════════════════════════════════════════════\n", CRed, fMono);
            if (codeMode) Put("▸ ALERTA.md\n", EdMuted, fMono);
            else if (sqlMode) Put("-- ALERTA\n", EdComment, fMono);
            else { PutPrompt(); Put("cat alerta.txt\n", CArg, fMono); }
            Put(incShown.Level.Objective + "\n\n", Art.Hx(0xFFB4AB), fMono);
            rtbOut.SelectionStart = rtbOut.TextLength;
            rtbOut.ScrollToCaret();
        }

        void ExpireIncident()
        {
            Put("\n⏰ Tempo esgotado! Outra pessoa resolveu o desafio relâmpago dessa vez.\n\n", CRed, fMonoBold);
            LeaveIncident();
        }

        void LeaveIncident()
        {
            incidentMode = false;
            navLock = true;
            nav.Items.Remove(-1);
            navLock = false;
            promptBox.Invalidate();
            promptLineRef.Invalidate();
            LoadLevel(gs.CatDone(cat) ? cur : gs.DefaultLevel(cat));
        }

#if DEMO
        public void DebugType(string s) { if (codeMode) editor.Text = s; else input.Text = s; Submit(); }
        public void DebugTypeOnly(string s) { if (codeMode) { editor.Text = s; editor.SelectionStart = s.Length; } else { input.Text = s; input.SelectionStart = s.Length; } }
        public void DebugClose() { CloseTerminal(); }
        public void DebugAws(string page, string arg) { awsView.Navigate(page, arg); }
        public void DebugNext() { GoNext(); }
        public void DebugToggleList() { ToggleList(); }
        public void DebugLoad(int i) { LoadLevel(i); }
        public bool DebugSolved { get { return solved; } }
        public bool DebugCode { get { return codeMode; } }
        public RichTextBox DebugEntry { get { return codeMode ? editor : input; } }
        public Button DebugSend { get { return codeMode ? btnIdeRun : btnRun; } }
        public Control DebugMission { get { return rtbMission; } }
        public string DebugStatus { get { return lblStatus.Text; } }
        public Control DebugAwsView { get { return awsView; } }
        public string DebugOutTail { get { string s = rtbOut.Text; return s.Length > 400 ? s.Substring(s.Length - 400) : s; } }
#endif

        void CloseTerminal()
        {
            if (!Visible) return;
            Visible = false;
            bool done = completedNow;
            completedNow = false;
            if (Closed != null) Closed(ch, done);
        }

        // ─────────────────────────── lista lateral ───────────────────────────

        void NavDraw(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            int i = (int)nav.Items[e.Index];
            var g = e.Graphics;
            var r = e.Bounds;
            g.FillRectangle(Art.B(nav.BackColor), r);
            if (i == -1)
            {
                bool sel = incidentMode;
                g.FillRectangle(Art.B(sel ? Art.Hx(0x5C2430) : Art.Hx(0x3A0D0D)), r.X + S(2), r.Y + S(1), r.Width - S(4), r.Height - S(2));
                g.FillRectangle(Art.B(Art.Hx(0xF85149)), r.X + S(2), r.Y + S(1), S(3), r.Height - S(2));
                string clock = "";
                if (gs.Incident != null) { int s = (int)Math.Ceiling(Math.Max(0, gs.IncTime)); clock = (s / 60) + ":" + (s % 60).ToString("00"); }
                TextRenderer.DrawText(g, "RELÂMPAGO!", fUiBold, new Rectangle(r.X + S(14), r.Y, r.Width - S(70), r.Height), CRed, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                TextRenderer.DrawText(g, clock, fUiBold, new Rectangle(r.Right - S(60), r.Y, S(52), r.Height), Color.White, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                return;
            }
            if (ideCat) { NavDrawIde(g, r, i); return; }
            if (i <= TierRow)
            {
                int t = TierRow - i;
                bool open = gs.TierUnlocked(cat, t), all = gs.TierDone(cat, t);
                int n = gs.TierLevels(cat, t).Count, k = gs.TierCount(cat, t);
                g.FillRectangle(Art.B(Art.Hx(0x0D1117)), r.X + S(2), r.Y + S(3), r.Width - S(4), r.Height - S(4));
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Color mc = TierColor[t];
                g.FillEllipse(Art.B(open ? mc : Theme.Border), r.X + S(8), r.Y + r.Height / 2 - S(6), S(12), S(12));
                g.SmoothingMode = SmoothingMode.None;
                string name = Challenges.TierNames[t].ToUpperInvariant();
                TextRenderer.DrawText(g, name, fUiBold, new Rectangle(r.X + S(26), r.Y + S(2), r.Width - S(90), r.Height), open ? mc : Theme.Muted, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                string rightTxt = all ? "✔ badge" : open ? k + "/" + n : "🔒";
                TextRenderer.DrawText(g, rightTxt, all || !open ? fSym : fUiSmall, new Rectangle(r.Right - S(80), r.Y + S(2), S(72), r.Height), all ? Theme.Green : Theme.Muted,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                return;
            }
            bool unlocked = gs.Unlocked(i), done = gs.Prog.Done(i), current = i == cur && !incidentMode;
            if (current)
            {
                g.FillRectangle(Art.B(Theme.Selected), r.X + S(2), r.Y + S(1), r.Width - S(4), r.Height - S(2));
                g.FillRectangle(Art.B(catInfo.Color), r.X + S(2), r.Y + S(1), S(3), r.Height - S(2));
            }
            string sym = done ? "✔" : unlocked ? "▸" : "·";
            Color sc = done ? Theme.Green : unlocked ? catInfo.Color : Theme.Border;
            TextRenderer.DrawText(g, sym, fSym, new Rectangle(r.X + S(8), r.Y, S(24), r.Height), sc, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            int ptsW = done ? S(36) : 0;
            Color tc = !unlocked ? Theme.Muted : current ? Theme.Bright : Theme.Text;
            var tl = gs.TierLevels(cat, gs.LevelList[i].Tier);
            TextRenderer.DrawText(g, (tl.IndexOf(i) + 1) + ". " + gs.LevelList[i].Title, fUi, new Rectangle(r.X + S(34), r.Y, r.Width - S(38) - ptsW, r.Height), tc,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            if (done)
                TextRenderer.DrawText(g, gs.Prog.Points[i].ToString(), fUiSmall, new Rectangle(r.Right - ptsW - S(6), r.Y, ptsW, r.Height), Theme.Yellow,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        // explorador de arquivos no estilo da IDE
        void NavDrawIde(Graphics g, Rectangle r, int i)
        {
            if (i <= TierRow)
            {
                int t = TierRow - i;
                bool open = gs.TierUnlocked(cat, t), all = gs.TierDone(cat, t);
                int n = gs.TierLevels(cat, t).Count, k = gs.TierCount(cat, t);
                float cy = r.Y + r.Height / 2f;
                TextRenderer.DrawText(g, open ? "⌄" : "›", fUi, new Rectangle(r.X + S(6), r.Y - S(2), S(14), r.Height), EdMuted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                // pasta
                Color fc = open ? TierColor[t] : Art.Hx(0x6E6E6E);
                g.FillRectangle(Art.B(Art.Dk(fc, 0.15f)), r.X + S(22), cy - S(6), S(7), S(3));
                g.FillRectangle(Art.B(fc), r.X + S(22), cy - S(4), S(16), S(11));
                string name = (t + 1) + "-" + Checker.Fold(Challenges.TierNames[t]);
                TextRenderer.DrawText(g, name, fUiBold, new Rectangle(r.X + S(44), r.Y, r.Width - S(100), r.Height), open ? EdText : EdMuted, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                string rightTxt = all ? "✔ badge" : open ? k + "/" + n : "🔒";
                TextRenderer.DrawText(g, rightTxt, all || !open ? fSym : fUiSmall, new Rectangle(r.Right - S(80), r.Y, S(70), r.Height), all ? Art.Hx(0x73C991) : EdMuted,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                return;
            }
            var lv = gs.LevelList[i];
            bool unlocked = gs.Unlocked(i), done = gs.Prog.Done(i), current = i == cur && !incidentMode;
            if (current)
            {
                g.FillRectangle(Art.B(EdSel), r);
                using (var p = new Pen(EdBlue)) g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
            }
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            string ext = Ext(lv);
            ExtBadge(g, ext, r.X + S(36), r.Y + r.Height / 2f);
            string file = FileName(lv);
            Color tc = !unlocked ? Art.Hx(0x5A5A5A) : done ? Art.Hx(0x73C991) : current ? Color.White : EdText;
            int rightW = done ? S(46) : S(14);
            TextRenderer.DrawText(g, file, fTab, new Rectangle(r.X + S(60), r.Y, r.Width - S(64) - rightW, r.Height), tc, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            if (done)
                TextRenderer.DrawText(g, "✔ " + gs.Prog.Points[i], fUiSmall, new Rectangle(r.Right - rightW - S(4), r.Y, rightW, r.Height), Art.Hx(0x73C991), TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            else if (unlocked && !current)
                TextRenderer.DrawText(g, "U", fTiny, new Rectangle(r.Right - S(18), r.Y, S(12), r.Height), Art.Hx(0x73C991), TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        void NavSelect(object sender, EventArgs e)
        {
            if (navLock || nav.SelectedIndex < 0) return;
            int i = (int)nav.Items[nav.SelectedIndex];
            if (i == -1)
            {
                if (gs.Incident != null && !incidentMode) LoadIncident();
                SyncNav();
                FocusInput();
                return;
            }
            if (i <= TierRow)
            {
                int t = TierRow - i;
                if (!gs.TierUnlocked(cat, t))
                    SetStatus("Nível " + Challenges.TierNames[t] + " bloqueado: conclua todos os desafios do nível " + Challenges.TierNames[t - 1] + " primeiro.", Theme.Red);
                SyncNav();
                FocusInput();
                return;
            }
            if (i != cur || incidentMode)
            {
                if (gs.Unlocked(i))
                {
                    if (incidentMode) { incidentMode = false; promptBox.Invalidate(); promptLineRef.Invalidate(); }
                    LoadLevel(i);
                }
                else
                {
                    int t = gs.LevelList[i].Tier;
                    SetStatus("Desafio bloqueado: conclua todo o nível " + Challenges.TierNames[t - 1] + " para liberar o " + Challenges.TierNames[t] + ".", Theme.Red);
                }
            }
            SyncNav();
            FocusInput();
        }

        void SyncNav()
        {
            navLock = true;
            nav.SelectedIndex = nav.Items.IndexOf(incidentMode ? -1 : cur);
            navLock = false;
            nav.Invalidate();
        }

        // ─────────────────────────── texto ───────────────────────────

        void Put(RichTextBox r, string text, Color c, Font f)
        {
            r.SelectionStart = r.TextLength;
            r.SelectionLength = 0;
            r.SelectionColor = c;
            r.SelectionFont = f;
            r.SelectedText = text;
        }

        void Put(string text, Color c) { Put(rtbOut, text, c, fMono); }
        void Put(string text, Color c, Font f) { Put(rtbOut, text, c, f); }

        static Color KindColor(CommandParser.TokKind k)
        {
            switch (k)
            {
                case CommandParser.TokKind.Docker: return CDocker;
                case CommandParser.TokKind.Path: return CPath;
                case CommandParser.TokKind.Flag: return CFlag;
                case CommandParser.TokKind.Value: return CValue;
                case CommandParser.TokKind.Inner: return CInner;
                default: return CArg;
            }
        }

        // cor do token: paleta do terminal ou da IDE (para código)
        Color TokColor(CommandParser.HTok t, string src, string kind, string ext)
        {
            bool ide = ideCat && (kind == "code" || kind == "html");
            if (!ide) return KindColor(t.Kind);
            string w = src.Substring(t.Start, t.Length);
            switch (t.Kind)
            {
                case CommandParser.TokKind.Docker: return ControlWords.Contains(w) ? Art.Hx(0xC586C0) : Art.Hx(0x569CD6);
                case CommandParser.TokKind.Path:
                    if (w.StartsWith("<"))
                    {
                        int k = w.Length > 1 && w[1] == '/' ? 2 : 1;
                        return k < w.Length && char.IsUpper(w[k]) ? Art.Hx(0x4EC9B0) : Art.Hx(0x569CD6);
                    }
                    return Art.Hx(0xDCDCAA);
                case CommandParser.TokKind.Flag: return Art.Hx(0x9CDCFE);
                case CommandParser.TokKind.Value: return char.IsDigit(w[0]) ? Art.Hx(0xB5CEA8) : Art.Hx(0xCE9178);
                default: return ext == "css" ? Art.Hx(0xD7BA7D) : ext == "html" ? Art.Hx(0xD4D4D4) : Art.Hx(0x9CDCFE);
            }
        }

        static readonly HashSet<string> Keywords = new HashSet<string>
        {
            "const", "let", "var", "function", "return", "if", "else", "for", "while", "def", "class", "import", "from", "in", "print", "echo",
            "new", "async", "await", "true", "false", "null", "None", "True", "False", "public", "private", "final", "static", "void", "this",
            "extends", "use", "namespace", "foreach", "as", "lambda", "with", "export", "default", "try", "catch", "except", "not", "and", "or",
            "fn", "require", "include", "match", "yield", "elif", "self", "super", "interface", "implements", "abstract", "readonly"
        };

        // destaque genérico para comandos de terminal e linhas de código
        static List<CommandParser.HTok> CodeHighlight(string s, string kind)
        {
            var res = new List<CommandParser.HTok>();
            int i = 0;
            bool first = true;
            while (i < s.Length)
            {
                char c = s[i];
                if (char.IsWhiteSpace(c)) { i++; if (c == '\n') first = true; continue; }
                int st = i;
                var k = CommandParser.TokKind.Arg;
                if (c == '"' || c == '\'' || c == '`')
                {
                    i++;
                    while (i < s.Length && s[i] != c && s[i] != '\n') i++;
                    if (i < s.Length && s[i] == c) i++;
                    k = CommandParser.TokKind.Value;
                }
                else if (kind == "cmd")
                {
                    while (i < s.Length && !char.IsWhiteSpace(s[i])) i++;
                    string w = s.Substring(st, i - st);
                    k = first ? CommandParser.TokKind.Docker : w.StartsWith("-") ? CommandParser.TokKind.Flag :
                        char.IsDigit(w[0]) ? CommandParser.TokKind.Value : CommandParser.TokKind.Arg;
                }
                else if (c == '<')
                {
                    i++;
                    if (i < s.Length && s[i] == '/') i++;
                    while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '-' || s[i] == '.')) i++;
                    k = CommandParser.TokKind.Path;
                }
                else if (char.IsLetter(c) || c == '_' || c == '$' || c == '@' || c == '#')
                {
                    i++;
                    while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '_' || s[i] == '-' || s[i] == '$')) i++;
                    string w = s.Substring(st, i - st);
                    int j = i;
                    while (j < s.Length && s[j] == ' ') j++;
                    if (Keywords.Contains(w)) k = CommandParser.TokKind.Docker;
                    else if (j < s.Length && s[j] == '(') k = CommandParser.TokKind.Path;
                    else if (j < s.Length && s[j] == '=' && kind == "html") k = CommandParser.TokKind.Flag;
                    else if (j < s.Length && s[j] == ':' && kind == "code") k = CommandParser.TokKind.Flag;
                    else k = CommandParser.TokKind.Arg;
                }
                else if (char.IsDigit(c))
                {
                    while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '.')) i++;
                    k = CommandParser.TokKind.Value;
                }
                else { i++; continue; }
                res.Add(new CommandParser.HTok { Start = st, Length = i - st, Kind = k });
                first = false;
            }
            return res;
        }

        List<CommandParser.HTok> Hl(string s, string kind)
        {
            if (kind == "sql") return SqlHighlight(s);
            return kind == "docker" ? CommandParser.Highlight(s) : CodeHighlight(s, kind == "aws" ? "cmd" : kind);
        }

        static readonly HashSet<string> SqlKw = new HashSet<string>
        {
            "SELECT", "FROM", "WHERE", "AND", "OR", "NOT", "NULL", "IS", "IN", "LIKE", "BETWEEN", "ORDER", "BY", "GROUP", "HAVING", "LIMIT", "OFFSET", "ASC", "DESC",
            "DISTINCT", "AS", "JOIN", "INNER", "LEFT", "RIGHT", "OUTER", "ON", "INSERT", "INTO", "VALUES", "UPDATE", "SET", "DELETE", "CREATE", "TABLE", "INDEX",
            "DROP", "ALTER", "ADD", "COLUMN", "PRIMARY", "KEY", "FOREIGN", "REFERENCES", "AUTO_INCREMENT", "DEFAULT", "UNIQUE", "SHOW", "TABLES", "DESCRIBE",
            "EXPLAIN", "USE", "INT", "INTEGER", "VARCHAR", "DECIMAL", "DATE", "TEXT", "BOOLEAN", "CHAR", "TRUNCATE", "BEGIN", "START", "TRANSACTION", "COMMIT",
            "ROLLBACK", "CASE", "WHEN", "THEN", "ELSE", "END", "EXISTS", "UNION", "DATABASES", "COLUMNS", "TRUE", "FALSE", "IF", "CROSS", "USING"
        };

        // destaque de sintaxe do SQL: palavras-chave, funções, textos e números
        static List<CommandParser.HTok> SqlHighlight(string s)
        {
            var res = new List<CommandParser.HTok>();
            int i = 0;
            while (i < s.Length)
            {
                char c = s[i];
                if (char.IsWhiteSpace(c)) { i++; continue; }
                int st = i;
                var k = CommandParser.TokKind.Arg;
                if (c == '-' && i + 1 < s.Length && s[i + 1] == '-')
                {
                    while (i < s.Length && s[i] != '\n') i++;
                    k = CommandParser.TokKind.Inner;
                }
                else if (c == '\'' || c == '"' || c == '`')
                {
                    i++;
                    while (i < s.Length && s[i] != c) i++;
                    if (i < s.Length) i++;
                    k = c == '`' ? CommandParser.TokKind.Arg : CommandParser.TokKind.Value;
                }
                else if (char.IsDigit(c)) { while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.')) i++; k = CommandParser.TokKind.Value; }
                else if (char.IsLetter(c) || c == '_')
                {
                    while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '_')) i++;
                    string w = s.Substring(st, i - st).ToUpperInvariant();
                    int j = i;
                    while (j < s.Length && s[j] == ' ') j++;
                    k = SqlKw.Contains(w) ? CommandParser.TokKind.Docker : j < s.Length && s[j] == '(' ? CommandParser.TokKind.Path : CommandParser.TokKind.Arg;
                }
                else if (c == '*') { i++; k = CommandParser.TokKind.Flag; }
                else { i++; continue; }
                res.Add(new CommandParser.HTok { Start = st, Length = i - st, Kind = k });
            }
            return res;
        }

        void PutColored(string cmd, string kind) { PutColored(cmd, kind, ""); }

        void PutColored(string cmd, string kind, string ext)
        {
            int pos = 0;
            bool ide = ideCat && (kind == "code" || kind == "html");
            Color plain = ide ? Art.Hx(0xD4D4D4) : CArg;
            foreach (var t in Hl(cmd, kind))
            {
                if (t.Start > pos) Put(cmd.Substring(pos, t.Start - pos), plain, fMono);
                bool bold = !ide && (t.Kind == CommandParser.TokKind.Docker || t.Kind == CommandParser.TokKind.Path);
                Put(cmd.Substring(t.Start, t.Length), TokColor(t, cmd, kind, ext), bold ? fMonoBold : fMono);
                pos = t.Start + t.Length;
            }
            if (pos < cmd.Length) Put(cmd.Substring(pos), plain, fMono);
        }

        void PutColored(Level lv, string code) { PutColored(code, lv.Kind ?? "docker", Ext(lv)); }

        void PutPrompt()
        {
            if (sqlMode) { Put("mysql> ", CPromptUser, fMonoBold); return; }
            Put(PromptUser, CPromptUser, fMonoBold);
            Put(":", Theme.Text, fMonoBold);
            Put(PromptPath, CPromptPath, fMonoBold);
            string sym = PromptSymbol;
            Put((sym == "$" ? "$" : " " + sym) + " ", Theme.Text, fMonoBold);
        }

        void Banner()
        {
            if (awsMode)
            {
                Put("AWS CloudShell", Art.Hx(0xFF9900), fMonoBold);
                Put("  ·  região " + awsSim.Region + "  ·  conta " + AwsSim.AccountId + " (simulada)\n", EdMuted);
                Put("Faça as tarefas pelo console acima (cliques) OU pela AWS CLI aqui. 'ls' mostra seus arquivos locais.\n", EdMuted);
                Put("Não sabe? A BIBLIOTECA (porta no fundo da Recepção) tem a seção AWS — grátis!\n\n", Art.Hx(0xD7BA7D));
                return;
            }
            if (sqlMode)
            {
                Put("Monitor MySQL", CPromptUser, fMonoBold);
                Put("  ·  servidor 8.0.36 (simulado para treino)  ·  os comandos terminam com ;\n", Theme.Muted);
                Put("Banco ", Theme.Muted); Put(SqlDb.DbName, CPath, fMonoBold);
                Put("  ·  tabelas: clientes, produtos, pedidos  ·  usuário estagio\n", Theme.Muted);
                Put("Explorar é grátis e não conta erro: SHOW TABLES;  DESCRIBE produtos;  SELECT * FROM produtos;\n", Theme.Muted);
                Put("help mostra os comandos · clear limpa · ↑↓ histórico · TAB completa nomes · Esc volta\n", Theme.Muted);
                Put("Não sabe? A BIBLIOTECA (porta no fundo da Recepção) tem a seção SQL — grátis!\n\n", CFlag);
                return;
            }
            if (ideCat)
            {
                Put("Micorosoft Code", Art.Hx(0x3794FF), fMonoBold);
                Put("  ·  projeto ", EdMuted);
                Put(ProjectName, Color.White, fMonoBold);
                Put("  ·  " + catInfo.Name + "  ·  " + Story.SectorOf(ch).Name + "\n", EdMuted);
                Put("Código: escreva no editor e aperte Ctrl+Enter (ou F5). Comandos: use o terminal integrado.\n", EdMuted);
                Put("Não sabe? A BIBLIOTECA (porta no fundo da Recepção) tem a documentação — grátis!\n\n", Art.Hx(0xD7BA7D));
                return;
            }
            // logo de quatro quadrados (qualquer semelhança é mera paródia)
            var cols = new[] { Art.Hx(0xF25022), Art.Hx(0x7FBA00), Art.Hx(0x00A4EF), Art.Hx(0xFFB900) };
            for (int row = 0; row < 4; row++)
            {
                int a = row < 2 ? 0 : 2;
                Put("   ███████", cols[a], fMonoBold);
                Put(" ███████", cols[a + 1], fMonoBold);
                if (row == 1) Put("    M I C O R O S O F T", Theme.Bright, fMonoBold);
                if (row == 2) Put("    Programa Jovem Talento", Theme.Muted, fMono);
                Put("\n", Theme.Text);
            }
            Put("\n  Terminal da área ", Theme.Muted);
            Put(catInfo.Name, catInfo.Color, fMonoBold);
            Put("  ·  " + Story.SectorOf(ch).Name + "\n", Theme.Muted);
            Put("  ENTER executa · ↑↓ histórico · Ctrl+L limpa · Esc volta" + (cat == "docker" ? " · TAB completa comandos" : "") + "\n", Theme.Muted);
            Put("  Não sabe? A BIBLIOTECA (porta no fundo da Recepção) tem a documentação de todas as áreas — grátis!\n\n", CFlag);
        }

        void ClearScreen()
        {
            rtbOut.Clear();
            Banner();
            if (incidentMode) IncidentHeader();
            else
            {
                MissionHeader();
                if (gs.Revealed.Contains(cur)) { revealedShown = false; ShowRevealed(); }
            }
            FocusInput();
        }

        void MissionHeader()
        {
            var lv = gs.LevelList[cur];
            var ls = gs.TierLevels(cat, lv.Tier);
            Put("──────────────────────────────────────────────────────────\n", ideCat ? EdBorder : Theme.Border, fMono);
            Put(" " + catInfo.Name.ToUpperInvariant() + " · ", catInfo.Color, fMonoBold);
            Put(Challenges.TierNames[lv.Tier].ToUpperInvariant(), TierColor[lv.Tier], fMonoBold);
            Put(" · desafio " + (ls.IndexOf(cur) + 1) + "/" + ls.Count + " · " + lv.Title + "\n", Theme.Bright, fMonoBold);
            Put("──────────────────────────────────────────────────────────\n", ideCat ? EdBorder : Theme.Border, fMono);
            if (codeMode) Put("▸ LEIA-ME.md   (arquivo aberto no editor: " + FileName(lv) + ")\n", EdMuted, fMono);
            else if (sqlMode) Put("-- OBJETIVO\n", EdComment, fMono);
            else { PutPrompt(); Put("cat objetivo.txt\n", CArg, fMono); }
            Put(lv.Objective + "\n\n", Art.Hx(0xD2A8FF), fMono);
            rtbOut.SelectionStart = rtbOut.TextLength;
            rtbOut.ScrollToCaret();
        }

        // ─────────────────────────── entrada com destaque ───────────────────────────

        void Recolor()
        {
            if (recoloring) return;
            recoloring = true;
            try
            {
                int ss = input.SelectionStart, sl = input.SelectionLength;
                SendMessage(input.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
                input.SelectAll();
                input.SelectionColor = CArg;
                input.SelectionFont = fMonoBig;
                foreach (var t in Hl(input.Text, CurKind))
                {
                    input.Select(t.Start, t.Length);
                    input.SelectionColor = KindColor(t.Kind);
                    if (t.Kind == CommandParser.TokKind.Docker || t.Kind == CommandParser.TokKind.Path) input.SelectionFont = fMonoBigBold;
                }
                input.Select(ss, sl);
                SendMessage(input.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
                input.Invalidate();
            }
            finally { recoloring = false; }
            UpdatePreview();
        }

        void RecolorEditor()
        {
            if (recoloring || !codeMode) return;
            recoloring = true;
            try
            {
                var lv = CurLv;
                string kind = lv.Kind ?? "code", ext = Ext(lv), text = editor.Text;
                int ss = editor.SelectionStart, sl = editor.SelectionLength;
                SendMessage(editor.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
                editor.SelectAll();
                editor.SelectionColor = Art.Hx(0xD4D4D4);
                editor.SelectionFont = fEditor;
                foreach (var t in CodeHighlight(text, kind))
                {
                    editor.Select(t.Start, t.Length);
                    editor.SelectionColor = TokColor(t, text, kind, ext);
                }
                editor.Select(ss, sl);
                SendMessage(editor.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
                editor.Invalidate();
            }
            finally { recoloring = false; }
            ideTabs.Invalidate();
            ideStatus.Invalidate();
        }

        void UpdateCue()
        {
            if (!codeMode) return;
            string ext = Ext(CurLv);
            editorCue.Text = CommentFor(ext, "escreva aqui o código pedido no objetivo") + "\n" + CommentFor(ext, "Ctrl+Enter (ou F5) executa");
            editorCue.Visible = editor.TextLength == 0;
        }

        void UpdatePreview()
        {
            chips.Clear();
            string text = input.Text.Trim();
            string kind = CurKind;
            if (codeMode)
            {
                previewHint = "Ctrl+Enter ou F5 executa  ·  Enter quebra linha  ·  espaços extras, aspas simples/duplas e ';' no final não importam";
                preview.Invalidate();
                return;
            }
            if (kind != "docker")
            {
                switch (kind)
                {
                    case "cmd": previewHint = "Digite o comando completo, com subcomando e opções.  A ordem das opções não importa.  ↑↓ histórico"; break;
                    case "aws": previewHint = "Faça a tarefa no console (cliques) OU pela AWS CLI aqui no CloudShell.  Ex.: aws s3 ls   ·   ls mostra seus arquivos   ·   ↑↓ histórico"; break;
                    case "sql": previewHint = "Explorar é grátis: SHOW TABLES;   DESCRIBE tabela;   SELECT * FROM tabela;    ·   TAB completa nomes   ·   ↑↓ histórico   ·   help"; break;
                    case "html": previewHint = "Escreva a tag completa (abrindo e fechando, se tiver).  Aspas simples ou duplas valem."; break;
                    case "text": previewHint = "Responda com uma palavra ou expressão curta.  Maiúsculas e acentos não importam."; break;
                    default: previewHint = "Escreva uma linha de " + catInfo.Name + ".  Espaços extras, aspas simples/duplas e ';' no final não importam."; break;
                }
                preview.Invalidate();
                return;
            }
            if (text.Length == 0)
            {
                previewHint = "Digite um comando começando com 'docker'.   TAB completa nomes de comandos  ·  ↑↓ histórico";
                preview.Invalidate();
                return;
            }
            var p = CommandParser.Parse(text);
            if (p.Error != null)
                chips.Add(new KeyValuePair<string, string>("ERRO", p.Error));
            else
            {
                if (p.Path.Count > 0 && !CommandParser.IsKnownCommand(p.Path[0]))
                    chips.Add(new KeyValuePair<string, string>("ERRO", "'" + p.Path[0] + "' não é comando do Docker"));
                else if (p.Path.Count > 0)
                    chips.Add(new KeyValuePair<string, string>("COMANDO", "docker " + string.Join(" ", p.Path)));
                var hl = CommandParser.Highlight(text);
                var opts = new List<string>();
                for (int i = 0; i < hl.Count; i++)
                {
                    if (hl[i].Kind != CommandParser.TokKind.Flag) continue;
                    string o = text.Substring(hl[i].Start, hl[i].Length);
                    if (i + 1 < hl.Count && hl[i + 1].Kind == CommandParser.TokKind.Value) o += " " + text.Substring(hl[i + 1].Start, hl[i + 1].Length);
                    opts.Add(o);
                }
                if (opts.Count > 0) chips.Add(new KeyValuePair<string, string>("OPÇÕES", string.Join("  ", opts)));
                var argToks = hl.Where(t => t.Kind == CommandParser.TokKind.Arg).Select(t => text.Substring(t.Start, t.Length)).ToList();
                var inner = hl.Where(t => t.Kind == CommandParser.TokKind.Inner).Select(t => text.Substring(t.Start, t.Length)).ToList();
                if (argToks.Count > 0) chips.Add(new KeyValuePair<string, string>("ARGUMENTOS", string.Join(" ", argToks)));
                if (inner.Count > 0) chips.Add(new KeyValuePair<string, string>("DENTRO DO CONTAINER", string.Join(" ", inner)));
            }
            preview.Invalidate();
        }

        void InputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                if (solved) GoNext(); else Submit();
            }
            else if (e.KeyCode == Keys.Tab)
            {
                e.SuppressKeyPress = true;
                TabComplete();
            }
            else if (e.KeyCode == Keys.Up && history.Count > 0)
            {
                e.SuppressKeyPress = true;
                histPos = Math.Max(0, histPos - 1);
                SetInput(history[histPos]);
            }
            else if (e.KeyCode == Keys.Down && history.Count > 0)
            {
                e.SuppressKeyPress = true;
                histPos = Math.Min(history.Count, histPos + 1);
                SetInput(histPos < history.Count ? history[histPos] : "");
            }
        }

        void EditorKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Enter && e.Control) || e.KeyCode == Keys.F5)
            {
                e.SuppressKeyPress = true;
                if (solved) GoNext(); else Submit();
                return;
            }
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                if (solved) { GoNext(); return; }
                // quebra de linha com indentação automática
                int ln = editor.GetLineFromCharIndex(editor.SelectionStart);
                int start = Math.Max(0, editor.GetFirstCharIndexFromLine(ln));
                string before = editor.Text.Substring(start, Math.Max(0, editor.SelectionStart - start));
                string indent = new string(before.TakeWhile(c => c == ' ' || c == '\t').ToArray());
                string tr = before.TrimEnd();
                if (tr.EndsWith(":") || tr.EndsWith("{") || tr.EndsWith("(") || tr.EndsWith("[")) indent += "    ";
                editor.SelectedText = "\n" + indent;
                return;
            }
            if (e.KeyCode == Keys.Tab)
            {
                e.SuppressKeyPress = true;
                if (!editor.ReadOnly) editor.SelectedText = "    ";
                return;
            }
            if (e.Control && e.KeyCode == Keys.V)
            {
                e.SuppressKeyPress = true;
                if (!editor.ReadOnly && Clipboard.ContainsText()) editor.SelectedText = Clipboard.GetText().Replace("\r\n", "\n").Replace("\t", "    ");
            }
        }

        void SetInput(string s)
        {
            input.Text = s;
            input.SelectionStart = input.TextLength;
            input.SelectionLength = 0;
        }

        void TabComplete()
        {
            if (CurKind == "sql") { SqlComplete(); return; }
            if (CurKind != "docker") { SetStatus("TAB completa apenas comandos do Docker. Aqui, pesquise a sintaxe na Biblioteca!", Theme.Muted); return; }
            int caret = input.SelectionStart;
            string text = input.Text.Substring(0, caret), after = input.Text.Substring(caret);
            int start;
            var c = CommandParser.Complete(text, out start);
            if (c.Count == 0) { SetStatus("TAB completa só nomes de comandos (ex.: docker con → container). As opções você pesquisa na Biblioteca!", Theme.Muted); return; }
            if (c.Count == 1)
            {
                string done = text.Substring(0, start) + c[0] + " ";
                input.Text = done + after;
                input.SelectionStart = done.Length;
                SetStatus("", Theme.Muted);
                return;
            }
            string pre = c[0];
            foreach (var s in c) while (!s.StartsWith(pre)) pre = pre.Substring(0, pre.Length - 1);
            if (pre.Length > text.Length - start)
            {
                string done = text.Substring(0, start) + pre;
                input.Text = done + after;
                input.SelectionStart = done.Length;
            }
            SetStatus("TAB:  " + string.Join("  ·  ", c.Take(16)) + (c.Count > 16 ? "  …" : ""), CPath);
        }

        // TAB no terminal mysql: completa palavras-chave e nomes de tabelas/colunas do banco
        void SqlComplete()
        {
            int caret = input.SelectionStart;
            string text = input.Text.Substring(0, caret), after = input.Text.Substring(caret);
            int start = caret;
            while (start > 0 && (char.IsLetterOrDigit(text[start - 1]) || text[start - 1] == '_')) start--;
            string pre = text.Substring(start);
            if (pre.Length == 0) { SetStatus("TAB completa palavras-chave e nomes de tabelas/colunas: digite o começo e aperte TAB.", Theme.Muted); return; }
            bool upper = pre.ToUpperInvariant() == pre;
            var names = new List<string>();
            if (sqlDb != null)
                foreach (var t in sqlDb.Tables.Values) { names.Add(t.Name); names.AddRange(t.Cols.Select(c => c.Name)); }
            var cands = names.Concat(SqlKw.Select(k => upper ? k : k.ToLowerInvariant()))
                             .Where(w => w.StartsWith(pre, StringComparison.OrdinalIgnoreCase)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(w => w.Length).ThenBy(w => w).ToList();
            if (cands.Count == 0) { SetStatus("Nada começa com '" + pre + "'. DESCRIBE tabela; mostra as colunas.", Theme.Muted); return; }
            string common = cands[0];
            foreach (var c in cands) while (!c.StartsWith(common, StringComparison.OrdinalIgnoreCase)) common = common.Substring(0, common.Length - 1);
            if (cands.Count == 1) common += " ";
            if (common.Length >= pre.Length)
            {
                string done = text.Substring(0, start) + common;
                input.Text = done + after;
                input.SelectionStart = done.Length;
            }
            SetStatus(cands.Count == 1 ? "" : "TAB:  " + string.Join("  ·  ", cands.Take(14)) + (cands.Count > 14 ? "  …" : ""), CPath);
        }

        void SetStatus(string msg, Color c) { lblStatus.Text = msg; lblStatus.ForeColor = c; }

        // ─────────────────────────── fluxo ───────────────────────────

        void LoadLevel(int i)
        {
            incidentMode = false;
            cur = i; solved = false; revealedShown = false;
            gs.TerminalLevel[cat] = i;
            ApplyMode();
            PrepareAws(gs.LevelList[i]);
            PrepareSql(gs.LevelList[i]);
            editor.ReadOnly = false;
            editor.Text = "";
            UpdateCue();
            promptBox.Invalidate();
            promptLineRef.Invalidate();
            RenderMission();
            MissionHeader();
            if (gs.Revealed.Contains(i)) ShowRevealed();
            SetInput("");
            Recolor();
            SetStatus(gs.Prog.Done(i) ? "Desafio já concluído (" + gs.Prog.Points[i] + " pts). Refaça para tentar melhorar." : "", Theme.Muted);
            UpdateButtons();
            SyncNav();
            FocusInput();
        }

        Npc HelperFor(int idx)
        {
            if (ch < 0 || ch >= Story.RoomCount) return null;
            return world.Helpers[ch, Math.Min(1, idx)] ?? world.Helpers[ch, 0];
        }

        void RenderMission()
        {
            var lv = gs.LevelList[cur];
            var ls = gs.TierLevels(cat, lv.Tier);
            lblMission.Text = catInfo.Name + " · " + Challenges.TierNames[lv.Tier] + "\ndesafio " + (ls.IndexOf(cur) + 1) + " de " + ls.Count + " do nível";
            rtbMission.Clear();
            Put(rtbMission, lv.Title + "\n", Theme.Bright, fH1);
            Put(rtbMission, "\"" + lv.Context + "\"\n\n", Theme.Text, fUi);
            Put(rtbMission, "► ", Theme.Yellow, fSym);
            Put(rtbMission, "OBJETIVO\n", Theme.Yellow, fBlack);
            Put(rtbMission, lv.Objective + "\n", Theme.Bright, fObj);
            if (codeMode) Put(rtbMission, "\nArquivo: " + FileName(lv) + "  ·  escreva no editor e aperte Ctrl+Enter.\n", Art.Hx(0x4FC1FF), fUiSmall);
            int used = gs.HintsOf(cur);
            for (int h = 0; h < used && h < lv.Hints.Length; h++)
            {
                Put(rtbMission, "\n💡 ", Theme.Yellow, fSym);
                Put(rtbMission, "Dica " + (h + 1) + ": ", Theme.Yellow, fUiBold);
                Put(rtbMission, lv.Hints[h], Theme.Yellow, fUi);
            }
            if (used < lv.Hints.Length && !solved)
            {
                var hp = HelperFor(used);
                Put(rtbMission, "\n\nTravou? Pesquise de graça na Biblioteca (seção " + catInfo.Name + "), ou peça uma dica" +
                    (hp != null ? " (aqui ou com " + hp.Name + " na sala)." : "."), Theme.Muted, fUiSmall);
            }
            rtbMission.SelectionStart = 0;
            rtbMission.ScrollToCaret();
        }

        void UpdateButtons()
        {
            if (incidentMode)
            {
                btnHint.Enabled = false;
                btnHint.Text = "Sem dicas no relâmpago";
                btnReveal.Enabled = false;
                btnReveal.Text = "Sem revelar no relâmpago";
                btnNext.Enabled = solved;
                btnNext.Text = "Continuar os desafios  ►";
                btnRun.Enabled = !solved;
                btnIdeRun.Text = solved ? "Continuar  ►" : "▶  Executar   Ctrl+Enter";
                valueBar.Invalidate();
                return;
            }
            int wrong = gs.WrongOf(cur);
            bool revealed = gs.Revealed.Contains(cur);
            int hintsUsed = gs.HintsOf(cur), hintsMax = gs.LevelList[cur].Hints.Length;
            btnHint.Enabled = !solved && hintsUsed < hintsMax;
            btnHint.Text = hintsUsed < hintsMax ? "💡  Pedir dica " + (hintsUsed + 1) + "/" + hintsMax + "   (−" + GameState.HintCost + " pts)" : "💡  Sem mais dicas";
            btnReveal.Enabled = !solved && !revealed && wrong >= GameState.RevealAfter;
            btnReveal.Text = revealed ? "Resposta revelada" :
                wrong >= GameState.RevealAfter ? "Revelar resposta (0 pts)" : "Revelar após " + GameState.RevealAfter + " erros (" + wrong + "/" + GameState.RevealAfter + ")";
            int next = NextInCat();
            btnNext.Enabled = solved || gs.Prog.Done(cur);
            btnNext.Text = completedNow ? "Receber a badge  ★" : next >= 0 ? "Próximo desafio  ►" : "Voltar ao escritório";
            btnRun.Enabled = !solved;
            btnIdeRun.Text = solved ? (completedNow ? "Receber a badge  ★" : "Próximo  ►   Ctrl+Enter") : "▶  Executar   Ctrl+Enter";
            valueBar.Invalidate();
            panelHeader.Invalidate();
            ideStatus.Invalidate();
            ideTabs.Invalidate();
        }

        // próximo desafio liberado da área (primeiro os não concluídos)
        int NextInCat()
        {
            var l = gs.CatLevels(cat);
            int pos = l.IndexOf(cur);
            for (int k = pos + 1; k < l.Count; k++) if (gs.Unlocked(l[k]) && !gs.Prog.Done(l[k])) return l[k];
            for (int k = 0; k < l.Count; k++) if (l[k] != cur && gs.Unlocked(l[k]) && !gs.Prog.Done(l[k])) return l[k];
            return -1;
        }

        // eco do que foi "executado" na saída da IDE: comando de execução + código com números de linha
        void PutRun(Level lv, string code)
        {
            Put("> ", EdMuted, fMonoBold);
            Put(RunLine(lv) + "\n", Art.Hx(0x4FC1FF), fMono);
            var lines = code.Replace("\r\n", "\n").Split('\n');
            for (int k = 0; k < lines.Length && k < 12; k++)
            {
                Put((k + 1).ToString().PadLeft(3) + " │ ", EdLineNo, fMono);
                PutColored(lv, lines[k]);
                Put("\n", EdText);
            }
            if (lines.Length > 12) Put("    │ …\n", EdLineNo, fMono);
        }

        void Submit()
        {
            if (solved) return;
            string cmd = (codeMode ? editor.Text : input.Text).Trim();
            if (cmd.Length == 0) { FocusInput(); return; }
            if (!codeMode)
            {
                if (history.Count == 0 || history[history.Count - 1] != cmd) history.Add(cmd);
                histPos = history.Count;
                SetInput("");
            }

            var lv = CurLv;
            if (lv.Kind == "aws") { RunCloudShell(cmd); return; }
            if (lv.Kind == "sql") { RunSql(cmd); return; }
            var r = Checker.Check(cmd, lv);
            if (codeMode) PutRun(lv, cmd);
            else
            {
                PutPrompt();
                PutColored(cmd, lv.Kind);
                Put("\n", Theme.Text);
            }
            if (!r.Ok) { Fail(r.Message); return; }
            Succeed(cmd, lv);
        }

        // erro: desconta (ou usa a borracha) e explica
        void Fail(string message)
        {
            bool erased = gs.Prog.Erasers > 0;
            if (erased) { gs.Prog.Erasers--; gs.Prog.Save(); }
            else if (incidentMode) gs.IncWrong++;
            else gs.Wrong[cur] = gs.WrongOf(cur) + 1;
            if (codeMode) { Put("✖ ", Theme.Red, fMonoBold); Put("PROBLEMA: ", Art.Hx(0xF48771), fMonoBold); }
            else Put("✖ ", Theme.Red, fMonoBold);
            Put(message + "\n", Art.Hx(0xFF7B72), fMono);
            if (erased) { Put("  ✎ ", Art.Hx(0xFF8FAB), fSym); Put("A borracha mágica apagou este erro: ele não conta!\n", Art.Hx(0xFF8FAB), fMonoBold); }
            Put("\n", Theme.Text);
            rtbOut.ScrollToCaret();
            SetStatus(codeMode ? "✖  Ainda não. Leia o problema na SAÍDA, corrija o código no editor e execute de novo." :
                      awsMode ? "✖  Ainda não. Leia a mensagem (em vermelho no console e no CloudShell), corrija e tente de novo." :
                      sqlMode ? "✖  Ainda não. Leia a mensagem acima, ajuste o comando e tente de novo (explorar com SELECT, SHOW e DESCRIBE é grátis)." :
                      "✖  Ainda não. Leia a mensagem acima, ajuste e tente de novo.", Theme.Red);
            Sfx.Play("err");
            UpdateButtons();
            FocusInput();
        }

        // acerto: pontua, explica e libera o próximo
        void Succeed(string typed, Level lv)
        {
            string go = codeMode ? "Ctrl+Enter" : "ENTER";
            if (incidentMode)
            {
                if (gs.Incident == null) { ExpireIncident(); return; }
                solved = true;
                editor.ReadOnly = true;
                int ipts = gs.IncidentValue() + TakeBreadBonus();
                ShowExplanation(typed, lv, ipts, -1, false, true);
                if (IncidentSolved != null) IncidentSolved(ipts);
                SetStatus("✔  Desafio relâmpago resolvido! +" + ipts + " pontos — " + go + " para continuar", Theme.Green);
                Sfx.Play("unlock");
                UpdateButtons();
                nav.Invalidate();
                FocusInput();
                return;
            }

            solved = true;
            editor.ReadOnly = true;
            bool wasComplete = gs.TierDone(cat, lv.Tier);
            bool revealed = gs.Revealed.Contains(cur);
            int pts = gs.CurrentValue(cur);
            int before = gs.Prog.Points[cur];
            if (pts > before) gs.Prog.Points[cur] = pts;
            if (gs.HintsOf(cur) == 0 && !revealed) gs.Prog.NoHint.Add(cur);
            if (gs.WrongOf(cur) >= 3) gs.Prog.Persist++;
            int bread = TakeBreadBonus();
            if (bread > 0) gs.Prog.ExtraPts += bread;
            gs.Prog.Save();
            gs.ClearSession(cur);
            if (!wasComplete && gs.TierDone(cat, lv.Tier)) completedNow = true;
            if (!gs.CatDone(cat)) gs.TerminalLevel[cat] = gs.DefaultLevel(cat);

            ShowExplanation(typed, lv, pts, before, revealed, false);
            if (bread > 0) { Put("Pão de queijo: +" + bread + " pontos extras!\n\n", Theme.Yellow, fMonoBold); rtbOut.ScrollToCaret(); }
            SetStatus(revealed ? "✔  Correto! (resposta revelada: 0 pts) — " + go + " para continuar" : "✔  Correto! +" + pts + " pontos — " + go + " para continuar", Theme.Green);
            Sfx.Play(completedNow ? "unlock" : "ok");
            RenderMission();
            UpdateButtons();
            nav.Invalidate();
            if (Solved != null) Solved(cur, pts);
            FocusInput();
        }

        // ── AWS: CloudShell e eventos do console
        string lastAwsInput;

        void RunCloudShell(string cmd)
        {
            PutPrompt();
            PutColored(cmd, "cmd");
            Put("\n", Theme.Text);
            if (cmd == "clear") { ClearScreen(); return; }
            lastAwsInput = cmd;
            bool ok = true;
            var parts = SplitShell(cmd);
            for (int k = 0; k < parts.Count; k++)
            {
                bool okk;
                if (parts.Count > 1) Put("$ " + parts[k] + "\n", Theme.Muted, fMono);
                string output = awsSim.Run(parts[k], out okk);   // pode disparar OnAwsEvent (acerto ou erro da tarefa)
                if (!string.IsNullOrEmpty(output)) Put(output + "\n", okk ? Art.Hx(0xC9D1D9) : Art.Hx(0xFF7B72), fMono);
                ok &= okk;
                if (!okk) break;   // como no bash com &&: parou no primeiro erro
            }
            Put("\n", Theme.Text);
            BeginInvoke((Action)(() => lastAwsInput = null));   // depois do veredito (eventos são avaliados antes, na fila)
            rtbOut.ScrollToCaret();
            var clv = CurLv;
            bool consoleOnly = clv.Kind == "aws" && !(clv.Answers.Length > 1 && clv.Answers[1].StartsWith("aws "));
            if (!ok && !solved) SetStatus(consoleOnly ? "Esta tarefa se faz pelo CONSOLE da AWS (a parte de cima da tela): o CloudShell daqui não tem esse comando." :
                                          "O comando falhou. Confira a sintaxe (a Biblioteca tem a seção AWS) — ou faça pelo console.", Theme.Yellow);
            FocusInput();
        }

        // separa "cmd1 && cmd2", "cmd1; cmd2" e "cmd1  +  cmd2" (fora de aspas)
        static List<string> SplitShell(string cmd)
        {
            var res = new List<string>();
            var sb = new StringBuilder();
            char q = '\0';
            for (int i = 0; i < cmd.Length; i++)
            {
                char c = cmd[i];
                if (q != '\0') { if (c == q) q = '\0'; sb.Append(c); continue; }
                if (c == '\'' || c == '"') { q = c; sb.Append(c); continue; }
                int skip = 0;
                if (c == ';') skip = 1;
                else if (c == '&' && i + 1 < cmd.Length && cmd[i + 1] == '&') skip = 2;
                else if (c == '+' && i > 0 && cmd[i - 1] == ' ' && i + 1 < cmd.Length && cmd[i + 1] == ' ') skip = 1;
                if (skip > 0)
                {
                    if (sb.ToString().Trim().Length > 0) res.Add(sb.ToString().Trim());
                    sb.Clear(); i += skip - 1; continue;
                }
                sb.Append(c);
            }
            if (sb.ToString().Trim().Length > 0) res.Add(sb.ToString().Trim());
            if (res.Count == 0) res.Add(cmd.Trim());
            return res;
        }

        // ── SQL: executa no banco simulado, mostra a tabela de resultado e confere a tarefa
        void RunSql(string text)
        {
            PutPrompt();
            PutColored(text, "sql");
            Put("\n", Theme.Text);
            string low = text.Trim().TrimEnd(';').Trim().ToLowerInvariant();
            if (low == "clear" || low == "cls" || low == "\\c") { ClearScreen(); return; }
            if (low == "exit" || low == "quit" || low == "\\q") { Put("Bye\n\n", Theme.Muted); CloseTerminal(); return; }
            if (low == "help" || low == "\\h" || low == "?" || low == "ajuda") { SqlHelp(); return; }
            if (sqlDb == null) sqlDb = SqlSample.Fresh();
            var lv = CurLv;
            SqlTask task = null;
            if (lv.Task != null) SqlTasks.All.TryGetValue(lv.Task, out task);
            foreach (var stmt in SqlDb.Split(text))
            {
                var before = sqlDb.Clone();
                SqlResult res;
                try { res = sqlDb.Execute(stmt); }
                catch (SqlError e)
                {
                    Put(e.Mysql + "\n", Art.Hx(0xFF7B72), fMono);
                    string first = SqlDb.Lex(stmt).Select(t => t.U).FirstOrDefault() ?? "";
                    bool explore = first == "SHOW" || first == "DESCRIBE" || first == "DESC" || first == "EXPLAIN" || first == "USE" || (first == "SELECT" && task != null && task.Mode != "query");
                    if (explore || task == null)
                    {
                        Put("  ↳ " + e.Hint + "\n\n", Art.Hx(0xD7BA7D), fMono);
                        SetStatus("O comando deu erro, mas não conta como tentativa (era só exploração). Leia a dica em amarelo.", Theme.Yellow);
                        rtbOut.ScrollToCaret();
                        FocusInput();
                        return;
                    }
                    Fail(e.Hint);
                    return;
                }
                Put(SqlDb.Format(res) + "\n\n", Art.Hx(0xC9D1D9), fMono);
                if (task == null) continue;
                string verdict = SqlTasks.Evaluate(task, sqlDb, res, ref sqlExpected);
                if (verdict == null) continue;
                if (verdict == SqlTasks.OK) { Succeed(stmt, lv); return; }
                if (res.Modified || res.Kind == "drop")
                {
                    sqlDb = before;
                    Put("(ROLLBACK automático do treino: o banco voltou a ficar como estava antes desse comando.)\n", Theme.Muted, fMono);
                }
                Fail(verdict);
                return;
            }
            rtbOut.ScrollToCaret();
            FocusInput();
        }

        void SqlHelp()
        {
            Put("Comandos úteis no terminal mysql:\n", Theme.Bright, fMonoBold);
            var lines = new[]
            {
                new[] { "SHOW TABLES;", "lista as tabelas do banco " + SqlDb.DbName },
                new[] { "DESCRIBE produtos;", "mostra as colunas (e os tipos) de uma tabela" },
                new[] { "SELECT * FROM produtos;", "mostra todos os dados de uma tabela" },
                new[] { "SHOW INDEX FROM pedidos;", "mostra os índices de uma tabela" },
                new[] { "EXPLAIN SELECT ...;", "mostra se a consulta usa um índice" },
                new[] { "clear", "limpa a tela  ·  Esc volta ao escritório" },
            };
            foreach (var l in lines)
            {
                Put("  " + l[0].PadRight(28), CDocker, fMonoBold);
                Put(l[1] + "\n", Theme.Muted, fMono);
            }
            Put("Explorar (SELECT, SHOW, DESCRIBE) não conta erro. A sintaxe de cada comando está na Biblioteca, seção SQL.\n\n", Art.Hx(0xD7BA7D), fMono);
            rtbOut.ScrollToCaret();
            FocusInput();
        }

        void OnAwsEvent(string evt)
        {
            if (solved || !awsMode || !Visible) return;
            var lv = CurLv;
            if (lv.Kind != "aws" || lv.Task == null) return;
            AwsTask task;
            if (!AwsTasks.All.TryGetValue(lv.Task, out task)) return;
            string res = task.Eval(awsSim, evt);
            if (res == null) return;
            // adia um instante: deixa o console terminar a ação (e a mensagem de sucesso dele) antes do veredito
            BeginInvoke((Action)(() =>
            {
                if (solved) return;
                if (res == AwsTasks.OK)
                {
                    awsView.Flash("✔ Tarefa concluída: " + lv.Title, Art.Hx(0x037F0C));
                    Succeed(lastAwsInput ?? "(feito pelo console)", lv);
                    lastAwsInput = null;
                }
                else
                {
                    awsView.Flash("✖ " + res, Art.Hx(0xD13212));
                    Fail(res);
                }
            }));
        }
        int TakeBreadBonus()
        {
            int b = gs.Prog.NextSolveBonus;
            gs.Prog.NextSolveBonus = 0;
            return b;
        }

        static string Same(string s, string kind)
        {
            if (kind == "docker" || kind == "cmd") return string.Join(" ", CommandParser.Tokenize(s)).ToLowerInvariant();
            return Checker.NormCode(s);
        }

        void ShowExplanation(string typed, Level lv, int pts, int before, bool revealed, bool incident)
        {
            string kind = lv.Kind ?? "docker";
            bool isCmd = kind == "docker" || kind == "cmd";
            bool ide = ideCat && !isCmd && kind != "text";
            Color frame = ide ? Art.Hx(0x3794FF) : Theme.Docker;
            if (!string.IsNullOrEmpty(lv.Output)) Put(lv.Output.Replace("\r\n", "\n") + "\n", Art.Hx(0x8B949E), fMono);
            Put("\n", Theme.Text);
            bool aws = kind == "aws", sql = kind == "sql";
            string head = incident ? "DESAFIO RELÂMPAGO RESOLVIDO!" : aws ? "TAREFA CONCLUÍDA NA AWS!" : sql ? "SQL CORRETO: O BANCO RESPONDEU CERTO!" : ide ? "TESTES PASSARAM!" : isCmd ? "COMANDO CORRETO!" : "RESPOSTA CORRETA!";
            Put("✔ ", Theme.Green, fSymBig); Put(head, Theme.Green, fBig);
            string ptsTxt = revealed ? "   +0 pontos (resposta revelada)" : "   +" + pts + " pontos";
            if (before >= 0) ptsTxt += "   (melhor anterior: " + before + ")";
            Put(ptsTxt + "\n", Theme.Yellow, fMonoBold);
            if (ide) Put("  ✓ " + FileName(lv) + " · sintaxe ok · resultado esperado\n", Art.Hx(0x73C991), fMono);
            if (completedNow && !incident)
            {
                Put("★ ", Theme.Cyan, fSym);
                Put("NÍVEL " + Challenges.TierNames[lv.Tier].ToUpperInvariant() + " DE " + catInfo.Name.ToUpperInvariant() + " CONCLUÍDO! Badge desbloqueada" +
                    (lv.Tier < 2 ? " e próximo nível liberado." : ". Você domina esta área!") + "\n", Theme.Cyan, fMonoBold);
            }

            Put("\n╭─ COMO FUNCIONA ─────────────────────────────────────────\n", frame, fMonoBold);
            for (int k = 0; k + 1 < lv.Explain.Length; k += 2)
            {
                Put("│ ", frame, fMonoBold);
                Put(lv.Explain[k] + "\n", ide ? Art.Hx(0xDCDCAA) : CPath, fMonoBold);
                Put("│     ", frame, fMonoBold);
                Put(lv.Explain[k + 1] + "\n", Theme.Text, fMono);
            }
            Put("├─ NA PRÁTICA ────────────────────────────────────────────\n", frame, fMonoBold);
            Put("│ ", frame, fMonoBold);
            Put(lv.Summary + "\n", Theme.Text, fMono);
            if (aws)
            {
                Put("├─ COMO FAZER ────────────────────────────────────────────\n", frame, fMonoBold);
                Put("│ ", frame, fMonoBold); Put("Pelo console: ", CPath, fMonoBold); Put(lv.Answers[0].Split('|')[0].Replace("Console:", "").Trim() + "\n", Theme.Text, fMono);
                if (lv.Answers.Length > 1) { Put("│ ", frame, fMonoBold); Put("Pela CLI: ", CPath, fMonoBold); PutColored(lv.Answers[1], "cmd"); Put("\n", Theme.Text); }
            }
            else if (Same(typed, kind) != Same(lv.Answers[0], kind))
            {
                Put("├─ FORMA DE REFERÊNCIA ───────────────────────────────────\n", frame, fMonoBold);
                foreach (var refLine in lv.Answers[0].Replace("\r\n", "\n").Split('\n'))
                {
                    Put("│ ", frame, fMonoBold);
                    PutColored(lv, refLine);
                    Put("\n", Theme.Text);
                }
                Put("│ ", frame, fMonoBold);
                Put(sql ? "(o seu SQL é diferente, mas equivalente: o que vale é o resultado no banco)\n" :
                    isCmd ? "(o seu comando é equivalente: a ordem das opções e as formas longas/curtas não importam)\n"
                          : "(a sua resposta é equivalente: espaços, quebras de linha, aspas e ';' finais não importam)\n", Theme.Muted, fMono);
            }
            Put("╰──────────────────────────────────────────────────────────\n\n", frame, fMonoBold);
            rtbOut.SelectionStart = rtbOut.TextLength;
            rtbOut.ScrollToCaret();
        }

        void AskHint()
        {
            if (incidentMode) return;
            var lv = gs.LevelList[cur];
            int used = gs.HintsOf(cur);
            if (solved || used >= lv.Hints.Length) return;
            var ok = MessageBox.Show(this, "Pedir uma dica desconta " + GameState.HintCost + " pontos deste desafio.\n\n" +
                "Alternativa grátis: pesquisar na Biblioteca (porta no fundo da Recepção).\n\nQuer a dica mesmo assim?",
                "Pedir dica", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (ok != DialogResult.Yes) { FocusInput(); return; }
            gs.Hints[cur] = used + 1;
            Sfx.Play("hint");
            RenderMission();
            Put("💡 ", Theme.Yellow, fSym); Put("Dica " + (used + 1) + ": ", Theme.Yellow, fMonoBold);
            Put(lv.Hints[used] + "\n\n", Theme.Yellow, fMono);
            rtbOut.ScrollToCaret();
            SetStatus("Dica " + (used + 1) + " anotada (−" + GameState.HintCost + " pontos).", Theme.Yellow);
            UpdateButtons();
            FocusInput();
        }

        void Reveal()
        {
            if (incidentMode) return;
            if (solved || gs.Revealed.Contains(cur) || gs.WrongOf(cur) < GameState.RevealAfter) return;
            var ok = MessageBox.Show(this, "Revelar a resposta faz este desafio valer 0 pontos.\n\nVocê ainda vai precisar digitar a resposta para avançar. Continuar?",
                "Revelar resposta", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (ok != DialogResult.Yes) return;
            gs.Revealed.Add(cur);
            ShowRevealed();
            SetStatus("Resposta revelada na saída. Digite-a para avançar.", Theme.Yellow);
            UpdateButtons();
            FocusInput();
        }

        void ShowRevealed()
        {
            if (revealedShown) return;
            revealedShown = true;
            var rl = gs.LevelList[cur];
            if (rl.Kind == "aws")
            {
                Put("Como fazer: ", Theme.Yellow, fMonoBold);
                Put(rl.Answers[0] + "\n", Theme.Yellow, fMono);
                if (rl.Answers.Length > 1)
                {
                    var cmds = rl.Answers[1].Split(new[] { "  +  " }, StringSplitOptions.RemoveEmptyEntries);
                    Put(cmds.Length > 1 ? "Ou pela CLI (um comando por vez):\n" : "Ou pela CLI: ", Theme.Yellow, fMonoBold);
                    for (int k = 0; k < cmds.Length; k++) { if (cmds.Length > 1) Put("  ", Theme.Text); PutColored(cmds[k].Trim(), "cmd"); if (k < cmds.Length - 1) Put("\n", Theme.Text); }
                }
                Put("\nFaça os passos acima no console (ou o comando no CloudShell) para concluir.\n\n", Theme.Yellow, fMono);
                rtbOut.ScrollToCaret();
                return;
            }
            Put("Resposta: ", Theme.Yellow, fMonoBold);
            PutColored(gs.LevelList[cur], gs.LevelList[cur].Answers[0]);
            Put("\nDigite a resposta acima para continuar (praticar a digitação ajuda a memorizar!).\n\n", Theme.Yellow, fMono);
            rtbOut.ScrollToCaret();
        }

        void GoNext()
        {
            if (incidentMode) { if (solved || gs.Incident == null) LeaveIncident(); return; }
            if (!(solved || gs.Prog.Done(cur))) return;
            int next = NextInCat();
            if (completedNow || next < 0) { CloseTerminal(); return; }
            LoadLevel(next);
        }
    }
}
