using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace DockerQuest
{
    class DialogPage
    {
        public Npc Npc;
        public string Title, Icon, Text;
        public bool Choice;
        public Action OnYes, OnNo, Run;
        public List<KeyValuePair<string, Action>> Options;   // escolhas múltiplas (teclas 1–3)
    }

    class Toast { public string Text; public float Life; public Color Color; }

    class FocusTarget
    {
        public Npc Npc;
        public Item Item;
        public DoorInfo Door;
        public string Label;
    }

    partial class GameView : Control
    {
        public event Action<int> TerminalRequested;
        public event Action HelpPressed, ResetRequested, ShopRequested, WardrobeRequested, LessonsRequested;
        public event Action<string> DocsRequested, MiniGameRequested;
        public event Action<string, int> BadgeEarned;
        public event Action BadgesRequested;

        // balões de fala dos NPCs (pânico/comemoração) e estado dos incidentes
        readonly Dictionary<Npc, KeyValuePair<string, float>> bubbles = new Dictionary<Npc, KeyValuePair<string, float>>();
        float bubbleTimer, celebrateTime;
        Incident pendingThanks;
        int pendingThanksPts;
        bool pendingFail;
        Look playerLook;
        float petX, petY, achTimer, saveTimer;
        int petFacing = 2;
        bool petInit, petMoving, showObjectives;
        public readonly World World;
        public bool Paused;

        readonly GameState gs;
        readonly float T, scale;
        Bitmap mini, miniBig;
        Bitmap low;
        readonly List<Scene.WallPiece> walls;
        // paredes vizinhas da mesma fileira viram uma imagem só (menos cópias por quadro)
        class WallRun { public int X0, Y; public float Key; public List<Scene.WallPiece> Pieces = new List<Scene.WallPiece>(); }
        readonly List<WallRun> wallRuns = new List<WallRun>();
        readonly int pix;
        int camXi, camYi;
        float dirX = 0, dirY = 1, vx, vy;
        readonly float miniTile, miniTileBig;
        readonly HashSet<Keys> keys = new HashSet<Keys>();
        [System.Runtime.InteropServices.DllImport("winmm.dll")]
        static extern uint timeBeginPeriod(uint period);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        static extern int DwmFlush();
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        static extern int DwmIsCompositionEnabled(out bool enabled);
        internal volatile int FramesPerRequest = 1;
        internal double VsyncPeriod = 1000.0 / 60;

        // Laço do jogo: uma thread marca o ritmo (60 quadros/s, precisão de 1 ms) e pede cada quadro à
        // interface. Mais estável que o Timer do WinForms, que oscila entre 15 e 31 ms e deixa o movimento travado.
        volatile bool framePending, loopRunning;
        volatile float frameInterval = 1000f / 60;   // ritmo adaptativo: 60 fps em movimento, menos quando parado

        void StartLoop()
        {
            try { timeBeginPeriod(1); } catch { }
            loopRunning = true;
            bool dwm = false;
            try { DwmIsCompositionEnabled(out dwm); } catch { dwm = false; }
            var th = new System.Threading.Thread(() =>
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                double next = 0;
                // sincronia vertical: DwmFlush espera a próxima atualização da tela (vsync). Assim cada quadro
                // aparece exatamente uma vez por atualização do monitor — sem engasgos de "quadro repetido".
                if (dwm)
                {
                    double lastFlush = 0, period = 1000.0 / 60;
                    int skip = 0, sinceReq = 0;
                    while (loopRunning)
                    {
                        int hr;
                        try { hr = DwmFlush(); } catch { hr = -1; }
                        if (hr != 0) { dwm = false; vsyncActive = false; break; }
                        vsyncActive = true;
                        LastFlushMs = clock.Elapsed.TotalMilliseconds;
                        if (presentPending)
                        {
                            presentPending = false;
                            try { BeginInvoke((Action)Present); } catch { }
                        }
                        double now = sw.Elapsed.TotalMilliseconds;
                        double d = now - lastFlush;
                        lastFlush = now;
                        if (d > 2 && d < 40) period = period * 0.95 + d * 0.05;   // período medido do monitor (60, 75, 144 Hz...)
                        VsyncPeriod = period;
                        // monitores rápidos: pula atualizações para ficar perto do ritmo desejado (60 fps em movimento)
                        skip = Math.Max(1, (int)Math.Round(frameInterval / period));
                        // o quadro precisa caber no intervalo (desenho + cópia + folga); senão, espera mais uma atualização
                        if (skip > 1 || period < 12) skip = Math.Max(skip, (int)Math.Ceiling((RenderMs + 3) / period));
                        if (++sinceReq < skip) continue;
                        sinceReq = 0;
                        FramesPerRequest = skip;
                        if (!framePending && IsHandleCreated)
                        {
                            framePending = true;
                            try { BeginInvoke((Action)Frame); } catch { framePending = false; }
                        }
                    }
                }
                while (loopRunning)
                {
                    double now = sw.Elapsed.TotalMilliseconds;
                    if (now >= next)
                    {
                        double interval = frameInterval;
                        next = Math.Max(next + interval, now - interval);
                        if (!framePending && IsHandleCreated)
                        {
                            framePending = true;
                            try { BeginInvoke((Action)Frame); } catch { framePending = false; }
                        }
                    }
                    else System.Threading.Thread.Sleep(1);
                }
            }) { IsBackground = true, Name = "ritmo do jogo" };
            th.Start();
        }

#if DEMO
        public readonly List<double> FrameLog = new List<double>();
#endif
        void Frame()
        {
            framePending = false;
            if (IsDisposed) return;
            double now = clock.Elapsed.TotalSeconds;
            float dt = (float)Math.Min(0.05, now - last);
            // passo de tempo constante, múltiplo do período da tela: o movimento avança sempre a mesma
            // distância por quadro mostrado (o tempo medido oscila 1–2 ms e isso aparece como tremido)
            double step = VsyncPeriod * FramesPerRequest / 1000.0;
            if (step > 0.004 && step < 0.05 && last > 0)
            {
                int n = Math.Max(1, (int)Math.Round((now - last) / step));
                dt = (float)Math.Min(0.05, n * step);
            }
#if DEMO
            FrameLog.Add((now - last) * 1000);
#endif
            last = now;
#if DEMO
            var fsw = System.Diagnostics.Stopwatch.StartNew();
            Tick(dt); camDt += dt; tickSerial++;
            ProfTick += fsw.Elapsed.TotalMilliseconds; fsw.Restart();
            RenderAndQueue();
            ProfTotal += fsw.Elapsed.TotalMilliseconds; ProfFrames++;
#else
            Tick(dt); camDt += dt; tickSerial++;
            RenderAndQueue();
#endif
            // decide o ritmo do próximo quadro (economiza CPU quando nada se move)
#if DEMO
            bool active = Form.ActiveForm != null || ForceActive;
#else
            bool active = Form.ActiveForm != null;
#endif
            bool motion = moving || Math.Abs(vx) + Math.Abs(vy) > 0.05f || petMoving || camMoving || splash > 0;
            frameInterval = !active || Paused ? 1000f / 15 : motion ? 1000f / 60 : 1000f / 30;
#if DEMO
            if (ForceFps > 0) frameInterval = 1000f / ForceFps;
#endif
        }
        // desenha o quadro e decide quando ele vai para a tela
        void RenderAndQueue()
        {
            var rsw = Stopwatch.StartNew();
            RenderFrame();
            RenderMs = RenderMs * 0.9 + rsw.Elapsed.TotalMilliseconds * 0.1;
            // com sincronia: a cópia acontece logo depois da próxima atualização do monitor (momento fixo);
            // sem sincronia (ou 1 atualização por quadro), copia já
            if (vsyncActive && FramesPerRequest > 1) presentPending = true;
            else Present();
        }
        volatile bool vsyncActive;

        readonly Stopwatch clock = Stopwatch.StartNew();
        readonly Random rng = new Random();
        double last;
        float time;

        float px, py, walk;
        int facing;
        bool moving;

        readonly Queue<DialogPage> pages = new Queue<DialogPage>();
        DialogPage page;
        float typed;
        Npc talking;

        bool showMissions;
        string banner;
        float bannerLife, splash;
        int lastZone = -99;
        readonly List<Toast> toasts = new List<Toast>();
        FocusTarget focus;
        string objective = "";
        PointF? target;

        readonly Font fSmall, fHud, fHudBold, fBanner, fName, fDialog, fTag, fScore, fSign, fIcon, fKey, fLogo, fTitle, fMark;

        public GameView(GameState gs, float scale)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.Selectable | ControlStyles.ResizeRedraw, true);
            TabStop = true;
            BackColor = Art.Void;
            this.gs = gs;
            this.scale = scale;
            T = (float)Math.Round(42 * scale);
            World = new World();
            // patinete de saves antigos: já comprado vira "em uso" uma vez
            if (gs.Prog.Owned.Contains("patinete") && gs.Prog.Stat("ride_init") == 0) { gs.Prog.Stats["ride_init"] = 1; gs.Prog.Equip["ride"] = "patinete"; }
            PlayerRoom.Apply(World, gs.Prog);
            walls = Scene.BuildWalls(World);
            foreach (var wp in walls) wallAt[wp.X * 1000 + wp.Y] = wp;
            WallRun run = null;
            foreach (var wp in walls.OrderBy(q => q.Y).ThenBy(q => q.X))
            {
                if (run == null || wp.Y != run.Y || wp.X != run.X0 + run.Pieces.Count || run.Pieces.Count >= 6 || wp.Key != run.Key)
                {
                    run = new WallRun { X0 = wp.X, Y = wp.Y, Key = wp.Key };
                    wallRuns.Add(run);
                }
                run.Pieces.Add(wp);
            }
            pix = Math.Max(2, (int)Math.Round(2 * scale));
            Scene.Q = pix;   // tudo (chão, móveis, personagens) é desenhado já na escala da tela
            miniTile = Math.Max(2f, 2.2f * scale);
            mini = Scene.RenderMini(World, miniTile);
            miniTileBig = miniTile * 1.5f;
            miniBig = Scene.RenderMini(World, miniTileBig);
            playerLook = Catalog.PlayerLook(gs.Prog);

            fSmall = new Font("Segoe UI", 9f);
            fHud = new Font("Segoe UI", 10f);
            fHudBold = new Font("Segoe UI Semibold", 11f);
            fBanner = new Font("Segoe UI Black", 22f);
            fName = new Font("Segoe UI Semibold", 13f);
            fDialog = new Font("Segoe UI", 12.5f);
            fTag = new Font("Segoe UI Semibold", T * 0.24f, GraphicsUnit.Pixel);
            fScore = new Font("Segoe UI Black", 17f);
            fSign = new Font("Segoe UI Semibold", T * 0.24f, GraphicsUnit.Pixel);
            fIcon = new Font("Segoe UI Symbol", 30f);
            fKey = new Font("Consolas", 10f, FontStyle.Bold);
            fLogo = new Font("Segoe UI Black", 11f);
            fTitle = new Font("Segoe UI Black", 16f);
            fMark = new Font("Segoe UI Black", T * 0.28f, GraphicsUnit.Pixel);

            PlacePlayer();
            // laço do jogo: 60 quadros por segundo com ritmo estável (o Timer do WinForms oscila entre 15 e 31 ms)
            HandleCreated += (s, e) => { if (!loopRunning) StartLoop(); };
            HandleDestroyed += (s, e) => loopRunning = false;
        }

        // ─────────────────────────── estado / API pública ───────────────────────────

        public void PlacePlayer()
        {
            px = World.Spawn.X; py = World.Spawn.Y; facing = 0;
            lastZone = -99;
        }

        public void ClearKeys() { keys.Clear(); }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);
        bool simulatedKeys;   // roteiros de teste apertam teclas "de mentira"

        // Às vezes o Windows não avisa que a tecla foi solta (ela foi solta com outra janela, campo ou diálogo em foco,
        // ou durante um travamento). Sem isso o jogo acha que W/A/S/D continua apertada e o boneco anda sozinho.
        // A cada quadro, confere no teclado de verdade e solta o que não está mais pressionado.
        void ReleaseStuckKeys()
        {
            if (keys.Count == 0 || simulatedKeys) return;
            var form = FindForm();
            if (form == null || Form.ActiveForm != form || !ContainsFocus) { keys.Clear(); return; }
            List<Keys> up = null;
            foreach (var k in keys)
                if ((GetAsyncKeyState((int)k) & 0x8000) == 0) (up ?? (up = new List<Keys>())).Add(k);
            if (up != null) foreach (var k in up) keys.Remove(k);
        }

#if DEMO
        public void Teleport(float x, float y, int f)
        {
            px = x; py = y; facing = f;
            dirX = f == 1 ? -1 : f == 2 ? 1 : 0; dirY = f == 3 ? -1 : f == 0 ? 1 : 0;
            vx = vy = 0;
            Tick(0.016f);
        }
        public void DebugAct(bool yes = true) { if (page != null) typed = page.Text.Length; Act(yes); Tick(0.016f); }
        public void DebugFinishDialog() { int guard = 60; while (page != null && guard-- > 0) { typed = page.Text.Length; Act(true); } Tick(0.016f); }
        public void DebugTab() { showMissions = !showMissions; }
        public void DebugObjectives() { showObjectives = !showObjectives; showMissions = false; }
        public void DebugStartIncident() { StartIncident(); }
        public void DebugKey(Keys k, bool down) { simulatedKeys = true; if (down) OnKeyDown(new KeyEventArgs(k)); else OnKeyUp(new KeyEventArgs(k)); }
        public void DebugStopLoop() { loopRunning = false; }
        public void DebugEvent(string id)
        {
            var n = World.Npcs.Where(x => x.Kind == NpcKind.Helper).OrderBy(x => Math.Abs(x.X - px) + Math.Abs(x.Y - py)).First();
            n.Event = DockerQuest.Events.All.First(e => e.Id == id);
            talking = n;
            RunEvent(n);
            StartDialog();
        }
        public readonly double[] Prof = new double[10];
        public bool ForceActive, FreezeNeeds;
        public void DebugNeeds(float hunger, float thirst) { gs.Prog.Hunger = hunger; gs.Prog.Thirst = thirst; }
        public void DebugCloseDialog() { pages.Clear(); page = null; }
        public string DebugFocus { get { var f = FindFocus(); return f == null ? "(nada)" : f.Label; } }
        public bool DebugPaused { get { return Paused; } }
        public int ProfItems;
        public int ProfFrames, ForceFps; public double ProfTotal, ProfTick;
        public bool UseGdiPlusScale;
        public string DebugPos() { return px.ToString("0.00") + "," + py.ToString("0.00") + " zona=" + World.ZoneAt(px, py); }
#endif

        string Fmt(string s) { return Gender.Apply(s.Replace("{nome}", string.IsNullOrEmpty(gs.Prog.PlayerName) ? "estagiário(a)" : gs.Prog.PlayerName)); }

        void Say(Npc n, string text) { pages.Enqueue(new DialogPage { Npc = n, Text = Fmt(text) }); }
        void SayIcon(string title, string icon, string text) { pages.Enqueue(new DialogPage { Title = title, Icon = icon, Text = Fmt(text) }); }
        void Ask(Npc n, string text, Action yes, Action no) { pages.Enqueue(new DialogPage { Npc = n, Text = Fmt(text), Choice = true, OnYes = yes, OnNo = no }); }
        void Then(Action a) { pages.Enqueue(new DialogPage { Run = a }); }

        void StartDialog() { if (page == null) NextPage(); }

        void NextPage()
        {
            page = null;
            while (pages.Count > 0)
            {
                var p = pages.Dequeue();
                if (p.Run != null) { p.Run(); continue; }
                page = p; typed = 0;
                if (p.Npc == null || !Audio.Available) Sfx.Play("talk");
                return;
            }
            talking = null;
        }

        public void Toast(string text, Color c) { toasts.Add(new Toast { Text = text, Life = 3.2f, Color = c }); }

        public void StartIntro()
        {
            splash = 5f;
            var gabi = World.Mentors[0];
            talking = gabi;
            foreach (var l in Story.Intro) Say(gabi, l);
            StartDialog();
        }

        readonly List<KeyValuePair<string, int>> pendingBadges = new List<KeyValuePair<string, int>>();
        int pendingPromotion = -1;

        // chamado pelo terminal ao acertar um desafio (normal ou relâmpago)
        public void OnSolved(int level, int pts, bool flash)
        {
            var lv = gs.LevelList[level];
            int xp = gs.AwardXp(lv, pts, flash);
            Toast((pts > 0 ? "+" + pts + " pontos" : "Concluído (0 pts)") + "  ·  +" + xp + " XP em " + Challenges.Cat(lv.Cat).Name, Art.Hx(0xF0B429));
            CheckBadges();
            CheckPromotion();
            gs.Prog.Save();
        }

        bool roomBadgesDirty;

        void CheckBadges()
        {
            foreach (var cat in Challenges.Cats)
                for (int t = 0; t < 3; t++)
                {
                    string key = cat.Id + ":" + t;
                    if (gs.Prog.BadgeDates.ContainsKey(key) || !gs.TierDone(cat.Id, t)) continue;
                    gs.Prog.BadgeDates[key] = DateTime.Now.ToString("dd/MM/yyyy");
                    pendingBadges.Add(new KeyValuePair<string, int>(cat.Id, t));
                    Toast("Nova badge: " + cat.Name + " · " + Challenges.TierNames[t] + "!", Art.Hx(0xF5C542));
                    Sfx.Play("ach");
                    roomBadgesDirty = true;
                }
            // a badge nova já vira quadro na parede da sua sala
            if (roomBadgesDirty) { roomBadgesDirty = false; RefreshRoom(); }
            if (!Paused) FlushPending();
        }

        void CheckPromotion()
        {
            int s = Career.Seniority(Career.TotalXp(gs.Prog));
            if (s > gs.Prog.LastSeniority)
            {
                gs.Prog.LastSeniority = s;
                pendingPromotion = s;
                gs.Prog.Save();
                if (!Paused) FlushPending();
            }
        }

        void FlushPending()
        {
            foreach (var b in pendingBadges.ToList())
            {
                pendingBadges.Remove(b);
                if (BadgeEarned != null) BadgeEarned(b.Key, b.Value);
            }
            if (pendingPromotion >= 0)
            {
                int s = pendingPromotion;
                pendingPromotion = -1;
                Sfx.Play("unlock");
                banner = "PROMOÇÃO: " + Career.SenName[s].ToUpperInvariant() + "!";
                bannerLife = 3f;
                var gabi = World.Mentors[0];
                talking = gabi;
                Say(gabi, "Parabéns, {nome}! Você foi promovido(a): agora seu cargo é " + Career.Title(gs.Prog) + "!");
                Say(gabi, "Seu título acompanha o que você mais pratica. Quer mudar de especialidade? Resolva desafios de outras áreas!");
                StartDialog();
            }
        }

        public void OnTerminalClosed(int room, bool unused)
        {
            keys.Clear();
            if (pendingThanks != null)
            {
                var rep = NpcNamed(pendingThanks.Reporter);
                talking = rep;
                Say(rep, "Valeu, {nome}! Você salvou o dia! \"" + pendingThanks.Title + "\" resolvido. +" + pendingThanksPts + " pontos!");
                pendingThanks = null;
                StartDialog();
            }
            if (pendingFail)
            {
                pendingFail = false;
                Say(World.Mentors[0], "O tempo do desafio relâmpago acabou... outra pessoa resolveu dessa vez. Fica para a próxima, {nome}!");
                StartDialog();
            }
            FlushPending();
        }
        // ─────────────────────────── entrada ───────────────────────────

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Up: case Keys.Down: case Keys.Left: case Keys.Right: case Keys.Tab: case Keys.Space: case Keys.Enter:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnMouseDown(MouseEventArgs e) { Focus(); base.OnMouseDown(e); }
        protected override void OnLostFocus(EventArgs e) { keys.Clear(); base.OnLostFocus(e); }
        protected override void OnKeyUp(KeyEventArgs e) { keys.Remove(e.KeyCode); }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            e.Handled = true;
            if (Paused) return;
            keys.Add(e.KeyCode);
            if (page != null && page.Options != null)
            {
                int k = e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D9 ? e.KeyCode - Keys.D1 : e.KeyCode >= Keys.NumPad1 && e.KeyCode <= Keys.NumPad9 ? e.KeyCode - Keys.NumPad1 : -1;
                if (typed < page.Text.Length && (k >= 0 || e.KeyCode == Keys.E || e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)) { typed = page.Text.Length; return; }
                if (k >= 0 && k < page.Options.Count)
                {
                    var act = page.Options[k].Value;
                    page = null;
                    if (act != null) act();
                    NextPage();
                }
                return;
            }
            switch (e.KeyCode)
            {
                case Keys.E: case Keys.Space: case Keys.Enter:
                    Act(true); break;
                case Keys.Escape: case Keys.N:
                    if (page != null) Act(false);
                    else { showMissions = false; showObjectives = false; }
                    break;
                case Keys.Tab:
                    if (page == null) { showMissions = !showMissions; showObjectives = false; } break;
                case Keys.O:
                    if (page == null) { showObjectives = !showObjectives; showMissions = false; } break;
                case Keys.C:
                    if (page == null) { keys.Clear(); showMissions = showObjectives = false; if (WardrobeRequested != null) WardrobeRequested(); } break;
                case Keys.D1: case Keys.D2: case Keys.D3: case Keys.D4: case Keys.D5: case Keys.D6: case Keys.D7: case Keys.D8:
                    if (page == null && e.KeyCode - Keys.D1 < Catalog.Hotbar.Length) UseItem(Catalog.Hotbar[e.KeyCode - Keys.D1]);
                    break;
                case Keys.NumPad1: case Keys.NumPad2: case Keys.NumPad3: case Keys.NumPad4: case Keys.NumPad5: case Keys.NumPad6: case Keys.NumPad7: case Keys.NumPad8:
                    if (page == null && e.KeyCode - Keys.NumPad1 < Catalog.Hotbar.Length) UseItem(Catalog.Hotbar[e.KeyCode - Keys.NumPad1]);
                    break;
                case Keys.B:
                    if (page == null) { keys.Clear(); showMissions = showObjectives = false; if (BadgesRequested != null) BadgesRequested(); }
                    break;
                case Keys.P:
                    if (page == null) ToggleRide();
                    break;
                case Keys.M:
                    Sfx.Muted = !Sfx.Muted;
                    Toast(Sfx.Muted ? "Som desligado" : "Som ligado", Art.Hx(0x8B949E));
                    break;
                case Keys.F1:
                    keys.Clear();
                    if (HelpPressed != null) HelpPressed();
                    break;
                case Keys.R:
                    if (e.Control) { keys.Clear(); if (ResetRequested != null) ResetRequested(); }
                    break;
            }
        }

        void Act(bool yes)
        {
            if (page != null)
            {
                if (typed < page.Text.Length) { typed = page.Text.Length; return; }
                if (page.Options != null) return;
                if (page.Choice)
                {
                    var a = yes ? page.OnYes : page.OnNo;
                    page = null;
                    if (a != null) a();
                }
                NextPage();
                return;
            }
            if (!yes) return;
            if (showMissions || showObjectives) { showMissions = showObjectives = false; return; }
            if (focus != null) Interact(focus);
        }

        // ─────────────────────────── interações ───────────────────────────

        void Interact(FocusTarget f)
        {
            if (f.Npc != null)
            {
                var n = f.Npc;
                talking = n;
                gs.Prog.Talked.Add(n.Name);
                float dx = px - n.X, dy = py - n.Y;
                n.Facing = ScreenFacing(dx, dy);
                if (n.Event != null) RunEvent(n);
                else if (n.Activity != null) RunActivity(n);
                else if (n.Kind == NpcKind.Mentor) TalkMentor(n.Chapter, n);
                else if (n.Kind == NpcKind.Helper) TalkHelper(n.Chapter, n);
                else if (n.Ceo) TalkCeo(n);
                else if (n.Shop)
                {
                    Say(n, n.Lines[0] + " Seu saldo agora: " + gs.Prog.Wallet.ToString("N0") + " pontos.");
                    Then(OpenShop);
                }
                else TalkFlavor(n);
            }
            else if (f.Item != null)
            {
                var it = f.Item;
                switch (it.Kind)
                {
                    case "terminal":
                        if (!gs.Prog.Met.Contains(it.Chapter))
                        {
                            var m = World.Mentors[it.Chapter];
                            talking = m;
                            Say(m, "Ei, {nome}! Antes de usar o terminal da área, venha falar comigo — estou aqui do lado.");
                        }
                        else
                        {
                            Sfx.Play("open");
                            keys.Clear();
                            if (TerminalRequested != null) TerminalRequested(it.Chapter);
                            return;
                        }
                        break;
                    case "shop":
                        OpenShop();
                        return;
                    case "docs":
                        keys.Clear();
                        Sfx.Play("open");
                        if (DocsRequested != null) DocsRequested(it.Text);
                        return;
                    case "lesson":
                        keys.Clear();
                        Sfx.Play("open");
                        if (LessonsRequested != null) LessonsRequested();
                        return;
                    case "lunch":
                        MealMenu(null);
                        break;
                    case "sala":
                        RoomInteract(it);
                        break;
                    case "arcade":
                        keys.Clear();
                        Sfx.Play("open");
                        if (MiniGameRequested != null) MiniGameRequested("invaders");
                        return;
                    case "activity":
                        RunItemActivity(it);
                        break;
                    case "vending":
                        if (!Pay(20, it.Title)) break;
                        Sfx.Play("use");
                        Restore(18, 0);
                        SayIcon(it.Title, "coffee", "Você comprou um lanchinho (" + Snacks[rng.Next(Snacks.Length)] + ") por 20 moedas. Fome +18.");
                        break;
                    case "coffee":
                        if (!Pay(15, "Máquina de café")) break;
                        Restore(0, 15);
                        gs.Prog.Coffees++;
                        Sfx.Play("coffee");
                        if (gs.Prog.Owned.Contains("cafeteira")) { gs.SpeedBuff = Math.Max(gs.SpeedBuff, 45); Toast("Cafeteira pessoal: +40% de velocidade por 45 s!", Art.Hx(0xF4A261)); }
                        SayIcon("Máquina de café", "coffee", "Café fresquinho (15 moedas, sede +15)! Enquanto você toma, lê um cartaz colado na máquina: \"" + Story.Trivia[rng.Next(Story.Trivia.Length)] + "\"");
                        break;
                    default:
                        if (it.Kind == "cooler")
                        {
                            if (!Pay(8, it.Title)) break;
                            gs.Prog.Waters++;
                            Restore(0, 35);
                            SayIcon(it.Title, "water", it.Text + " (Copo d'água: 8 moedas, sede +35.)");
                            break;
                        }
                        gs.Prog.Read.Add(Catalog.BoardKey(it));
                        SayIcon(it.Title, "board", it.Text);
                        break;
                }
            }
            StartDialog();
        }

        bool lunched;

        string CorridorName() { return py >= 41.5f ? "Corredor sul" : px >= 105.5f ? "Corredor leste" : "Corredor principal"; }
        static readonly string[] FunLines =
        {
            "Você jogou uma partida rápida de Space Invaders e fez 4.200 pontos.",
            "Game over na fase 3! Mas a pausa de 2 minutos refrescou a cabeça.",
            "Você bateu o recorde do Vini no Pac-Man. Ele não vai gostar nada disso.",
            "Partida de Tetris: as peças encaixaram melhor que o seu último CSS."
        };
        static readonly string[] Snacks = { "barra de cereal", "maçã", "pacotinho de castanhas", "suco de uva", "biscoito de polvilho" };

        void OpenShop()
        {
            keys.Clear();
            if (ShopRequested != null) ShopRequested();
        }

        string TierSummary(string cat)
        {
            var parts = new List<string>();
            for (int t = 0; t < 3; t++)
                parts.Add(Challenges.TierNames[t] + " " + gs.TierCount(cat, t) + "/" + gs.TierLevels(cat, t).Count + (gs.TierUnlocked(cat, t) ? "" : " (bloqueado)"));
            return string.Join(" · ", parts);
        }

        void TalkMentor(int room, Npc m)
        {
            var cat = Challenges.CatOfRoom(room);
            if (!gs.Prog.Met.Contains(room))
            {
                foreach (var l in Story.Sectors[room].Intro) Say(m, l);
                Then(() =>
                {
                    gs.Prog.Met.Add(room);
                    gs.Prog.Save();
                    Toast("Terminal de " + cat.Name + " liberado!", Art.Docker);
                });
                return;
            }
            Say(m, "Seu progresso em " + cat.Name + ": " + TierSummary(cat.Id) + ".");
            if (gs.CatDone(cat.Id))
            {
                Say(m, "Você concluiu TODOS os níveis de " + cat.Name + "! Que orgulho. Que tal explorar outra área para ampliar seu perfil?");
                return;
            }
            int lv = gs.DefaultLevel(cat.Id);
            var L = gs.LevelList[lv];
            Say(m, "Próximo desafio (" + Challenges.TierNames[L.Tier] + "): " + L.Title + ". " + L.Objective);
            Say(m, "Para liberar o próximo nível, conclua todos os desafios do nível atual. Dicas: " + World.Helpers[room, 0].Name + " e " + World.Helpers[room, 1].Name + " (custam pontos) ou a Biblioteca (grátis).");
        }

        void TalkHelper(int room, Npc h)
        {
            var cat = Challenges.CatOfRoom(room);
            var mentor = World.Mentors[room];
            var other = World.Helpers[room, 1 - h.HelperIdx];
            if (!gs.Prog.Met.Contains(room))
            {
                Say(h, h.Greeting + " Fale primeiro com " + mentor.Name + " (" + mentor.Role + "), que lidera esta área. Depois eu te ajudo!");
                return;
            }
            if (gs.Prog.Reputation < 20)
            {
                Say(h, "Hmm... ouvi umas coisas sobre você pela empresa. Prefiro não ajudar agora. Melhore sua reputação, {nome}.");
                return;
            }
            if (gs.CatDone(cat.Id))
            {
                Say(h, h.Lines[h.LineIdx++ % h.Lines.Length]);
                return;
            }
            int lv;
            if (!gs.TerminalLevel.TryGetValue(cat.Id, out lv) || gs.Prog.Done(lv)) lv = gs.DefaultLevel(cat.Id);
            var L = gs.LevelList[lv];
            if (L.Hints.Length == 0) { Say(h, h.Lines[h.LineIdx++ % h.Lines.Length]); return; }
            int used = gs.HintsOf(lv);
            int idx = h.HelperIdx;
            if (idx == used)
            {
                Action<bool> give = free =>
                {
                    gs.Hints[lv] = used + 1;
                    if (free)
                    {
                        gs.FreeHints[lv] = gs.FreeOf(lv) + 1;
                        gs.Prog.Vouchers--;
                        gs.Prog.Save();
                    }
                    Sfx.Play("hint");
                    Say(h, h.HintPrefix + L.Hints[idx]);
                    if (idx == 0) Say(h, "Se ainda ficar difícil, fale com " + other.Name + " — dá pra complementar.");
                    Then(() => Toast(free ? "Vale-dica usado: dica anotada sem perder pontos" : "Dica anotada no terminal", Art.Hx(0xF0B429)));
                };
                Action refuse = () => Say(h, "Beleza! Tenta aí — qualquer coisa, estou por aqui.");
                string ask = h.Greeting + " Quer uma dica para \"" + L.Title + "\"?";
                if (gs.Prog.Vouchers > 0)
                    Ask(h, ask + " Você tem " + gs.Prog.Vouchers + " vale(s)-dica: quer usar um? Assim você NÃO perde pontos.",
                        () => give(true),
                        () => Ask(h, "Tudo bem. Quer a dica pagando " + GameState.HintCost + " pontos, então?", () => give(false), refuse));
                else
                    Ask(h, ask + " Ela custa " + GameState.HintCost + " pontos do desafio. (Vales-dica da lojinha evitam esse desconto.)", () => give(false), refuse);
            }
            else if (idx > used)
                Say(h, h.Greeting + " Para esse desafio, fale primeiro com " + other.Name + ". Depois eu complemento!");
            else
                Say(h, "A dica " + (idx + 1) + " já está anotada no seu terminal: \"" + L.Hints[idx] + "\"" + (idx == 0 && used < 2 ? " Se precisar de mais, fale com " + other.Name + "." : " Agora é com você!"));
        }

        void TalkCeo(Npc n)
        {
            if (!gs.Prog.Met.Contains(Story.Directors))
            {
                foreach (var l in Story.CeoLines) Say(n, l);
                Then(() => { gs.Prog.Met.Add(Story.Directors); gs.Prog.Save(); });
                return;
            }
            int sen = Career.Seniority(Career.TotalXp(gs.Prog));
            Say(n, "Olá, " + Career.Title(gs.Prog) + "! Você tem " + gs.BadgeCount + " badge(s) e reputação " + gs.Prog.Reputation + " (" + Career.RepLabel(gs.Prog.Reputation) + ").");
            if (sen >= 2 && gs.BadgeCount >= 6 && gs.Prog.Reputation >= 40)
                Say(n, "Pleno, " + gs.BadgeCount + " badges e boa reputação... Está decidido: você está EFETIVADO(A) na Micorosoft! Parabéns, {nome}! Continue crescendo — o céu é o limite.");
            else
            {
                var falta = new List<string>();
                if (sen < 2) falta.Add("chegar a Pleno (" + Career.TotalXp(gs.Prog) + "/" + Career.SenXp[2] + " XP)");
                if (gs.BadgeCount < 6) falta.Add("ter 6 badges (" + gs.BadgeCount + "/6)");
                if (gs.Prog.Reputation < 40) falta.Add("recuperar sua reputação");
                Say(n, "Para a efetivação ainda falta: " + string.Join(", ", falta) + ". Estou torcendo por você!");
            }
        }

        // ─────────────────────────── eventos de diálogo ───────────────────────────

        float eventTimer = 40;

        void UpdateEvents(float dt)
        {
            if (Paused || page != null) return;
            eventTimer -= dt;
            if (eventTimer > 0) return;
            eventTimer = 45 + rng.Next(45);
            if (World.Npcs.Count(n => n.Event != null) >= 2) return;
            var pool = DockerQuest.Events.All.Where(ev => !gs.Prog.EventsDone.Contains(ev.Id) && World.Npcs.All(n => n.Event != ev)).ToList();
            if (pool.Count == 0) pool = DockerQuest.Events.All.Where(ev => ev.Id == "mentor" || ev.Id == "cliente" || ev.Id == "ceo").ToList();
            var ev2 = pool[rng.Next(pool.Count)];
            Npc who;
            if (ev2.Id == "ceo") who = World.Mentors[Story.Directors];
            else
            {
                var cand = World.Npcs.Where(n => n.Event == null && !n.Shop && !n.Ceo && n.Kind != NpcKind.Mentor && Math.Abs(n.X - px) + Math.Abs(n.Y - py) < 30).ToList();
                if (cand.Count == 0) return;
                who = cand[rng.Next(cand.Count)];
            }
            who.Event = ev2;
            bubbles[who] = new KeyValuePair<string, float>("Psiu, " + (gs.Prog.PlayerName.Length > 0 ? gs.Prog.PlayerName : "você") + "!", 4f);
        }

        void RunEvent(Npc n)
        {
            var ev = n.Event;
            n.Event = null;
            Say(n, ev.Intro);
            var opts = new List<KeyValuePair<string, Action>>();
            foreach (var o in ev.Options)
            {
                var opt = o;
                opts.Add(new KeyValuePair<string, Action>(opt.Label, () => ApplyChoice(n, ev, opt)));
            }
            pages.Enqueue(new DialogPage { Npc = n, Text = Fmt("O que você faz?"), Options = opts });
        }

        void ApplyChoice(Npc n, DialogEvent ev, EventOption o)
        {
            gs.Prog.EventsDone.Add(ev.Id);
            int rep = o.Rep;
            if (rep > 0 && gs.Prog.Owned.Contains("livro_etica")) rep = (int)Math.Round(rep * 1.5);
            gs.Prog.Reputation = Math.Max(0, Math.Min(100, gs.Prog.Reputation + rep));
            if (o.Xp > 0) gs.Prog.AddXp("soft", (int)Math.Round(o.Xp * Career.XpMultiplier(gs.Prog)));
            if (o.Pts != 0) gs.Prog.ExtraPts += o.Pts;
            if (o.Good) gs.Prog.GoodChoices++;
            if (rep < 0) gs.Prog.BadChoices++;
            if (o.Item != null) gs.Prog.Inv[o.Item] = gs.Prog.Count(o.Item) + 1;
            gs.Prog.Save();
            Say(n, o.Reply);
            var fx = new List<string>();
            if (rep != 0) fx.Add((rep > 0 ? "+" : "") + rep + " reputação");
            if (o.Xp > 0) fx.Add("+" + o.Xp + " XP (habilidades interpessoais)");
            if (o.Pts > 0) fx.Add("+" + o.Pts + " pontos");
            if (o.Item != null) fx.Add("ganhou: " + Catalog.Item(o.Item).Name);
            Color c = rep > 0 ? Art.Hx(0x3FB950) : rep < 0 ? Art.Hx(0xF85149) : Art.Hx(0x8B949E);
            Then(() =>
            {
                if (fx.Count > 0) Toast(string.Join("  ·  ", fx), c);
                Sfx.Play(rep < 0 ? "err" : rep > 0 ? "ach" : "talk");
                if (gs.Prog.Reputation < 20) Toast("Sua reputação está queimada: colegas vão evitar te ajudar.", Art.Hx(0xF85149));
                CheckPromotion();
                if (o.Flash) { gs.IncCooldown = 0; StartIncident(true); }
            });
        }
        void TalkFlavor(Npc n)
        {
            if (n.Trivia)
            {
                Say(n, "Bom dia! Quer um cafezinho? Pega ali na máquina. Ah, e sabia dessa?");
                Say(n, Story.Trivia[n.LineIdx++ % Story.Trivia.Length]);
                return;
            }
            Say(n, n.Lines[n.LineIdx++ % n.Lines.Length]);
        }

        // ─────────────────────────── atualização ───────────────────────────

        bool DoorLocked(int ch) { return false; }   // todas as áreas são abertas

        bool BlockedTile(int tx, int ty)
        {
            if (!World.InMap(tx, ty)) return true;
            int t = World.Tile[tx, ty];
            if (t == World.Wall || t == World.Void) return true;
            if (World.Solid[tx, ty]) return true;
            if (t == World.Door && DoorLocked(World.DoorCh[tx, ty])) return true;
            return false;
        }

        bool Blocked(float x, float y, Npc self)
        {
            for (int tx = (int)Math.Floor(x - 0.28f); tx <= (int)Math.Floor(x + 0.28f); tx++)
                for (int ty = (int)Math.Floor(y - 0.22f); ty <= (int)Math.Floor(y + 0.02f); ty++)
                    if (BlockedTile(tx, ty)) return true;
            if (self != null) return false;
            foreach (var n in World.Npcs)
            {
                float d2 = (n.X - x) * (n.X - x) + (n.Y - y) * (n.Y - y);
                float o2 = (n.X - px) * (n.X - px) + (n.Y - py) * (n.Y - py);
                if (d2 < 0.36f && d2 < o2) return true;
            }
            return false;
        }

        void Tick(float dt)
        {
            time += dt;
            if (isoPending > 0 && --isoPending == 0) EnterIso();
            if (isoLoading) IsoLoadStep();
            if (page != null && typed < page.Text.Length)
            {
                // a fala aparece letra a letra, com a "voz" de quem está falando
                int before = (int)typed / 4;
                typed += dt * 60;
                int ti = (int)typed;
                if (page.Npc != null && ti / 4 != before && ti < page.Text.Length && !char.IsWhiteSpace(page.Text[ti]))
                    Audio.Syllable(NpcPitch(page.Npc), 0.26f);
            }
            for (int i = toasts.Count - 1; i >= 0; i--) { toasts[i].Life -= dt; if (toasts[i].Life <= 0) toasts.RemoveAt(i); }
            if (bannerLife > 0) bannerLife -= dt;
            if (splash > 0) splash -= dt;

            UpdateNpcs(dt);
            UpdateNeeds(dt);
            UpdateAmbience(dt);
            UpdateIncident(dt);
            UpdateEvents(dt);
            if (gs.SpeedBuff > 0) gs.SpeedBuff -= dt;
            foreach (var n in bubbles.Keys.ToList())
            {
                var b = bubbles[n];
                if (b.Value - dt <= 0) bubbles.Remove(n); else bubbles[n] = new KeyValuePair<string, float>(b.Key, b.Value - dt);
            }

            moving = false;
            ReleaseStuckKeys();
            if (!Paused && page == null)
            {
                // W sobe, S desce, A esquerda, D direita (na tela), com aceleração suave
                float ix = 0, iy = 0;
                if (isoMode) IsoInput(ref ix, ref iy);
                else
                {
                    if (keys.Contains(Keys.A) || keys.Contains(Keys.Left)) ix -= 1;
                    if (keys.Contains(Keys.D) || keys.Contains(Keys.Right)) ix += 1;
                    if (keys.Contains(Keys.W) || keys.Contains(Keys.Up)) iy -= 1;
                    if (keys.Contains(Keys.S) || keys.Contains(Keys.Down)) iy += 1;
                }
                float maxSp = keys.Contains(Keys.ShiftKey) || ModifierKeys.HasFlag(Keys.Shift) ? 7.4f : 4.6f;
                if (Riding) maxSp *= 1.5f;
                else if (gs.Prog.Owned.Contains("tenis")) maxSp *= 1.25f;
                if (gs.Prog.Hunger <= 0 || gs.Prog.Thirst <= 0) maxSp *= 0.75f;   // sem energia
                if (gs.SpeedBuff > 0) maxSp *= 1.4f;
                float tvx = 0, tvy = 0;
                if (ix != 0 || iy != 0)
                {
                    float len = (float)Math.Sqrt(ix * ix + iy * iy);
                    tvx = ix / len * maxSp; tvy = iy / len * maxSp;
                    facing = Math.Abs(ix) > 0 && iy == 0 ? (ix < 0 ? 1 : 2) : iy != 0 && ix == 0 ? (iy < 0 ? 3 : 0) : (facing == 1 || facing == 2 ? (ix < 0 ? 1 : 2) : (iy < 0 ? 3 : 0));
                    dirX = ix / len; dirY = iy / len;
                }
                float accel = (ix != 0 || iy != 0) ? 16f : 22f;
                float kk = 1f - (float)Math.Exp(-accel * dt);
                vx += (tvx - vx) * kk; vy += (tvy - vy) * kk;
                if (Math.Abs(vx) < 0.02f && tvx == 0) vx = 0;
                if (Math.Abs(vy) < 0.02f && tvy == 0) vy = 0;
                float mdx = vx * dt, mdy = vy * dt;
                float ox = px, oy = py;
                if (mdx != 0)
                {
                    if (!Blocked(px + mdx, py, null)) px += mdx;
                    else
                    {
                        // desliza nos cantos (ajuda a passar por portas)
                        bool slid = false;
                        foreach (var off in new[] { 0.12f, -0.12f, 0.24f, -0.24f })
                            if (tvy == 0 && !Blocked(px + mdx, py + off, null) && !Blocked(px, py + Math.Sign(off) * Math.Min(Math.Abs(off), Math.Abs(mdx) + 0.02f), null))
                            { py += Math.Sign(off) * Math.Min(Math.Abs(off), Math.Abs(mdx) + 0.02f); slid = true; break; }
                        if (!slid) vx = 0;
                    }
                }
                if (mdy != 0)
                {
                    if (!Blocked(px, py + mdy, null)) py += mdy;
                    else
                    {
                        bool slid = false;
                        foreach (var off in new[] { 0.12f, -0.12f, 0.24f, -0.24f })
                            if (tvx == 0 && !Blocked(px + off, py + mdy, null) && !Blocked(px + Math.Sign(off) * Math.Min(Math.Abs(off), Math.Abs(mdy) + 0.02f), py, null))
                            { px += Math.Sign(off) * Math.Min(Math.Abs(off), Math.Abs(mdy) + 0.02f); slid = true; break; }
                        if (!slid) vy = 0;
                    }
                }
                float moved = (float)Math.Sqrt((px - ox) * (px - ox) + (py - oy) * (py - oy));
                gs.Prog.Walked += moved;
                if (moved > 0.15f * dt)
                {
                    moving = true;
                    walk += moved * 2.5f;
                }
                if (!Riding) PlayerSteps(keys.Contains(Keys.ShiftKey) || ModifierKeys.HasFlag(Keys.Shift));
            }
            else { vx = vy = 0; }
            UpdatePet(dt);

            int zone = World.ZoneAt(px, py);
            if (zone != lastZone && zone >= -1)
            {
                if (lastZone != -99)
                {
                    banner = zone == Story.Library ? "BIBLIOTECA · DOCUMENTAÇÃO" : zone >= 0 ? Story.Sectors[zone].Name.ToUpperInvariant() : CorridorName().ToUpperInvariant();
                    bannerLife = 2.4f;
                }
                lastZone = zone;
                if (zone >= 0) gs.Prog.Visited.Add(zone);
                if (zone == Story.PlayerRoom && PlayerRoom.Placed(gs.Prog).Count == 0)
                    Toast("Esta é a SUA sala! Ela está vazia: mobilie na Lojinha, aba \"Minha sala\".", Art.Hx(0xF0B429));
            }

            achTimer -= dt;
            if (achTimer <= 0) { achTimer = 0.5f; CheckAchievements(); }

            focus = Paused || page != null ? null : FindFocus();
            ComputeObjective(zone);
        }

        // ─────────────────────────── mascote e objetivos ───────────────────────────

        // ─────────────────────────── incidentes relâmpago ───────────────────────────

        void UpdateIncident(float dt)
        {
            if (gs.Incident != null)
            {
                gs.IncTime -= dt;
                bubbleTimer -= dt;
                if (bubbleTimer <= 0) { bubbleTimer = 1.0f; RandomBubble(Incidents.Panic, 3.2f); }
                if (gs.IncTime <= 0) FailIncident();
                return;
            }
            if (celebrateTime > 0)
            {
                celebrateTime -= dt;
                bubbleTimer -= dt;
                if (bubbleTimer <= 0) { bubbleTimer = 0.6f; RandomBubble(Incidents.Celebrate, 2.6f); }
            }
            if (!Paused && page == null && gs.Prog.Met.Count >= 1)
            {
                gs.IncCooldown -= dt;
                if (gs.IncCooldown <= 0) StartIncident();
            }
        }

        void RandomBubble(string[] lines, float life)
        {
            var near = World.Npcs.Where(n => n != talking && !bubbles.ContainsKey(n) && Math.Abs(n.X - px) + Math.Abs(n.Y - py) < 14).ToList();
            if (near.Count == 0) return;
            var n2 = near[rng.Next(near.Count)];
            bubbles[n2] = new KeyValuePair<string, float>(Fmt(lines[rng.Next(lines.Length)]), life);
            NpcChatter(n2, 3 + rng.Next(4));
        }

        Npc NpcNamed(string name) { return World.Npcs.FirstOrDefault(n => n.Name == name) ?? World.Mentors[0]; }

        // Cria um desafio relâmpago de qualquer categoria, no nível que o jogador já alcançou.
        Incident MakeFlash()
        {
            var cat = Challenges.Cats[rng.Next(Challenges.Cats.Length)];
            int maxT = gs.MaxTier(cat.Id);
            if (cat.Id == "docker" && rng.Next(2) == 0)
            {
                var pool = Incidents.All.Where(i => i.Tier <= maxT && i.Id != gs.LastIncidentId).ToList();
                if (pool.Count > 0) return pool[rng.Next(pool.Count)];
            }
            var levels = gs.CatLevels(cat.Id).Where(i => gs.LevelList[i].Tier <= maxT).ToList();
            var fresh = levels.Where(i => !gs.Prog.Done(i)).ToList();
            var choose = fresh.Count > 0 && rng.Next(3) > 0 ? fresh : levels;
            int li = choose[rng.Next(choose.Count)];
            var lv = gs.LevelList[li];
            var rep = World.Helpers[cat.Room, rng.Next(2)] ?? World.Mentors[cat.Room];
            return new Incident
            {
                Id = "flash_" + li, Cat = cat.Id, Room = cat.Room, Tier = lv.Tier, Level = lv, LevelIndex = li, Reporter = rep.Name,
                Title = lv.Title, Alert = "URGENTE na área de " + cat.Name + "! " + lv.Context
            };
        }

        void StartIncident(bool forced = false)
        {
            var inc = MakeFlash();
            gs.LastIncidentId = inc.Id;
            var rep = NpcNamed(inc.Reporter);
            float total = Incidents.BaseTime + (gs.Prog.Owned.Contains("fone_ruido") ? 30 : 0) + (gs.Prog.Owned.Contains("monitor") ? 20 : 0) + gs.Prog.NextIncidentTime;
            int maxPts = Incidents.BasePoints + Incidents.TimeBonus + gs.Prog.NextIncidentBonus;
            talking = rep;
            Sfx.Play("alarm");
            Say(rep, inc.Alert);
            pages.Enqueue(new DialogPage
            {
                Npc = rep,
                Text = Fmt("DESAFIO RELÂMPAGO de " + Challenges.Cat(inc.Cat).Name + " (" + Challenges.TierNames[inc.Tier] + "): você teria " + (int)total +
                           " s para resolver no terminal da área " + Story.Sectors[inc.Room].Name + ". Vale até " + maxPts + " pontos e XP em dobro. Aceita?"),
                Options = new List<KeyValuePair<string, Action>>
                {
                    new KeyValuePair<string, Action>("Aceito! Vou agora.", () =>
                    {
                        gs.Incident = inc;
                        gs.IncTotal = total;
                        gs.IncTime = total;
                        gs.IncBonus = gs.Prog.NextIncidentBonus;
                        gs.IncWrong = 0;
                        gs.Prog.NextIncidentTime = 0;
                        gs.Prog.NextIncidentBonus = 0;
                        gs.Prog.Met.Add(inc.Room);
                        gs.Prog.Save();
                        bubbles[rep] = new KeyValuePair<string, float>("Corre!", 6f);
                        banner = "DESAFIO RELÂMPAGO!";
                        bannerLife = 2.4f;
                    }),
                    new KeyValuePair<string, Action>("Agora não, estou focado(a) em outra coisa.", () =>
                    {
                        gs.IncCooldown = 120 + rng.Next(90);
                        Say(rep, "Tudo bem, vou chamar outra pessoa. Fica para a próxima!");
                    })
                }
            });
            StartDialog();
        }

        void FailIncident()
        {
            var inc = gs.Incident;
            gs.Incident = null;
            gs.IncCooldown = 180 + rng.Next(120);
            bubbles.Clear();
            for (int i = 0; i < 4; i++) RandomBubble(Incidents.Sad, 3f);
            Sfx.Play("err");
            Toast("Tempo esgotado: " + inc.Title, Art.Hx(0xF85149));
            if (Paused) { pendingFail = true; return; }
            var rep = NpcNamed(inc.Reporter);
            talking = rep;
            Say(rep, "O tempo acabou... outra pessoa resolveu dessa vez. Fica para a próxima, {nome}!");
            StartDialog();
        }

        public void SolveIncident(int pts)
        {
            var inc = gs.Incident;
            if (inc == null) return;
            bool fast = gs.IncTime > gs.IncTotal * 2f / 3f;
            gs.Prog.ExtraPts += pts;
            gs.Prog.Incidents++;
            if (fast) gs.Prog.FastIncidents++;
            if (inc.LevelIndex >= 0 && !gs.Prog.Done(inc.LevelIndex)) gs.Prog.Points[inc.LevelIndex] = GameState.MaxPoints;
            int xp = gs.AwardXp(inc.Level, pts, true);
            gs.Prog.Save();
            gs.Incident = null;
            gs.IncCooldown = 200 + rng.Next(120);
            bubbles.Clear();
            celebrateTime = 7;
            bubbleTimer = 0;
            pendingThanks = inc;
            pendingThanksPts = pts;
            Toast("Desafio relâmpago resolvido! +" + pts + " pontos · +" + xp + " XP" + (fast ? "  (relâmpago!)" : ""), Art.Hx(0x3FB950));
            CheckBadges();
            CheckPromotion();
        }
        // ─────────────────────────── consumíveis (teclas 1–6) ───────────────────────────

        void UseItem(string id)
        {
            var item = Catalog.Item(id);
            if (gs.Prog.Count(id) <= 0) { Toast("Você não tem " + item.Name + ". Compre na Lojinha do corredor!", Art.Hx(0x8B949E)); return; }
            switch (id)
            {
                case "cafe_expresso":
                    gs.SpeedBuff = 90;
                    Toast("Café expresso: +40% de velocidade por 90 s!", Art.Hx(0xF4A261));
                    Restore(0, 20);
                    break;
                case "pao_queijo":
                    gs.Prog.NextSolveBonus += 15;
                    Restore(25, 0);
                    Toast("Pão de queijo: seu próximo acerto vale +" + gs.Prog.NextSolveBonus + " pontos extras", Art.Hx(0xF5C542));
                    break;
                case "energetico":
                    Restore(0, 25);
                    if (gs.Incident != null) { gs.IncTime += 45; gs.IncTotal += 45; Toast("Energético: +45 s no incidente!", Art.Hx(0x3FB950)); }
                    else { gs.Prog.NextIncidentTime += 45; Toast("Energético: +45 s no próximo incidente", Art.Hx(0x3FB950)); }
                    break;
                case "pizza":
                    if (gs.Incident != null) gs.IncBonus += 50; else gs.Prog.NextIncidentBonus += 50;
                    Restore(45, 0);
                    celebrateTime = 4;
                    for (int i = 0; i < 5; i++) RandomBubble(new[] { "Oba, pizza!", "Valeu pela pizza, {nome}!", "Pizza no escritório!", "Melhor estagiário(a)!" }, 3f);
                    Toast("Pizza para o time: o " + (gs.Incident != null ? "incidente atual" : "próximo incidente") + " vale +50 pontos!", Art.Hx(0xE63946));
                    break;
                case "borracha":
                    gs.Prog.Erasers++;
                    Toast("Borracha mágica pronta: seu próximo erro no terminal não conta", Art.Hx(0xFF8FAB));
                    break;
                case "teleporte":
                    if (!target.HasValue) { Toast("Sem objetivo para teletransportar agora.", Art.Hx(0x8B949E)); return; }
                    if (!TeleportNear(target.Value)) { Toast("Não achei um lugar livre perto do objetivo.", Art.Hx(0x8B949E)); return; }
                    Toast("Teletransporte! Zap!", Art.Hx(0x8E6CC4));
                    break;
                case "cha_verde":
                    gs.Prog.Reputation = Math.Min(100, gs.Prog.Reputation + 4);
                    Restore(0, 30);
                    for (int i = 0; i < 2; i++) RandomBubble(new[] { "Obrigado pelo chá, {nome}!", "Que gentileza!", "Chazinho, que delícia." }, 3f);
                    Toast("Chá verde dividido com o time: +4 de reputação", Art.Hx(0x7FB069));
                    break;
                case "bolo":
                    gs.Prog.Reputation = Math.Min(100, gs.Prog.Reputation + 8);
                    Restore(35, 0);
                    celebrateTime = 5;
                    for (int i = 0; i < 6; i++) RandomBubble(new[] { "BOLO!!!", "Parabéns pra quem?", "Valeu pelo bolo, {nome}!", "Tem de chocolate?", "Melhor dia na Micorosoft!" }, 3f);
                    Toast("Bolo para o time: +8 de reputação!", Art.Hx(0xEF476F));
                    break;
            }
            gs.Prog.Inv[id] = gs.Prog.Count(id) - 1;
            gs.Prog.Consumed++;
            gs.Prog.Save();
            Sfx.Play("use");
        }

        bool TeleportNear(PointF p)
        {
            var offs = new[] { new PointF(0, 0.7f), new PointF(0, 0), new PointF(0.7f, 0.4f), new PointF(-0.7f, 0.4f), new PointF(0, 1.2f), new PointF(1, 1), new PointF(-1, 1), new PointF(0.7f, -0.6f), new PointF(-0.7f, -0.6f) };
            foreach (var o in offs)
                if (!Blocked(p.X + o.X, p.Y + o.Y, null)) { px = p.X + o.X; py = p.Y + o.Y; petInit = false; return true; }
            return false;
        }

        void UpdatePet(float dt)
        {
            if (playerLook.Pet == null) { petInit = false; return; }
            float tx = px - dirX * 0.9f + (dirY != 0 ? 0.45f : 0), ty = py - dirY * 0.9f + (dirX != 0 ? 0.15f : 0);
            if (!petInit) { petX = tx; petY = ty; petInit = true; }
            float dx = tx - petX, dy = ty - petY;
            float k = Math.Min(1, dt * 5);
            petX += dx * k; petY += dy * k;
            petMoving = Math.Abs(dx) + Math.Abs(dy) > 0.08f;
            if (Math.Abs(dx) > 0.05f) petFacing = dx < 0 ? 1 : 2;
        }

        public void RefreshLook()
        {
            playerLook = Catalog.PlayerLook(gs.Prog);
            CheckAchievements();
        }

        void CheckAchievements()
        {
            bool changed = false;
            foreach (var a in Catalog.Achievements)
            {
                if (gs.Prog.Achieved.Contains(a.Id)) continue;
                if (a.Progress(gs, World) < Catalog.GoalOf(a, World)) continue;
                gs.Prog.Achieved.Add(a.Id);
                gs.Prog.Bonus += a.Reward;
                Toast("Objetivo concluído: " + a.Name + "  (+" + a.Reward + " para a lojinha)", Art.Hx(0xF5C542));
                Sfx.Play("ach");
                changed = true;
            }
            saveTimer += 0.5f;
            if (changed || saveTimer > 10) { saveTimer = 0; gs.Prog.Save(); }
        }

        // converte uma direção em tiles na pose do personagem (visão de cima)
        static int ScreenFacing(float dx, float dy)
        {
            if (Math.Abs(dx) > Math.Abs(dy) * 1.1f) return dx < 0 ? 1 : 2;
            return dy < 0 ? 3 : 0;
        }

        void UpdateNpcs(float dt)
        {
            foreach (var n in World.Npcs)
            {
                n.Moving = false;
                if (n == talking) continue;
                if (n.Wander)
                {
                    if (n.Wait > 0) { n.Wait -= dt; continue; }
                    float dx = n.TX - n.X, dy = n.TY - n.Y, d = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (d < 0.1f)
                    {
                        n.TX = n.HomeX + (float)(rng.NextDouble() * 2 - 1) * n.Range;
                        n.TY = n.HomeY + (float)(rng.NextDouble() * 2 - 1) * 0.9f;
                        n.Wait = 1.5f + (float)rng.NextDouble() * 3;
                        continue;
                    }
                    float sp = Math.Min(d, (gs.Incident != null ? 3.6f : 1.7f) * dt);
                    float nx = n.X + dx / d * sp, ny = n.Y + dy / d * sp;
                    float pd = (nx - px) * (nx - px) + (ny - py) * (ny - py);
                    if (Blocked(nx, ny, n) || pd < 0.5f) { n.TX = n.X; n.TY = n.Y; continue; }
                    n.X = nx; n.Y = ny; n.Moving = true; n.Walk += sp * 2.5f; NpcStep(n);
                    n.Facing = ScreenFacing(dx, dy);
                }
                else if (n.Stroll) UpdateStroll(n, dt);
                else
                {
                    n.Wait -= dt;
                    if (n.Wait <= 0)
                    {
                        n.Wait = 2 + (float)rng.NextDouble() * 5;
                        // de vez em quando a pessoa dá uma voltinha pela sala (e depois volta para o lugar dela)
                        if (!n.Shop && !n.Trivia && rng.Next(3) == 0 && PickStroll(n)) { n.Stroll = true; n.Wait = 0.2f; }
                        else n.Facing = rng.Next(3) == 0 ? rng.Next(4) : n.DefaultFacing;
                    }
                }
            }
        }

        // escolhe um ponto livre perto do lugar da pessoa, na mesma sala, com caminho reto desimpedido
        bool PickStroll(Npc n)
        {
            int zone = World.ZoneAt(n.HomeX, n.HomeY);
            for (int tries = 0; tries < 8; tries++)
            {
                float tx = n.HomeX + (float)(rng.NextDouble() * 2 - 1) * 2.8f, ty = n.HomeY + (float)(rng.NextDouble() * 2 - 1) * 1.8f;
                if (World.ZoneAt(tx, ty) != zone || Math.Abs(tx - n.HomeX) + Math.Abs(ty - n.HomeY) < 1.2f) continue;
                if (!PathFree(n.HomeX, n.HomeY, tx, ty, n)) continue;
                n.TX = tx; n.TY = ty;
                return true;
            }
            return false;
        }

        bool PathFree(float x0, float y0, float x1, float y1, Npc n)
        {
            float d = (float)Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
            int steps = Math.Max(1, (int)(d / 0.2f));
            for (int k = 1; k <= steps; k++)
                if (Blocked(x0 + (x1 - x0) * k / steps, y0 + (y1 - y0) * k / steps, n)) return false;
            return true;
        }

        void UpdateStroll(Npc n, float dt)
        {
            if (n.Wait > 0) { n.Wait -= dt; return; }
            float dx = n.TX - n.X, dy = n.TY - n.Y, d = (float)Math.Sqrt(dx * dx + dy * dy);
            bool goingHome = Math.Abs(n.TX - n.HomeX) < 0.01f && Math.Abs(n.TY - n.HomeY) < 0.01f;
            if (d < 0.08f)
            {
                if (goingHome)
                {
                    n.X = n.HomeX; n.Y = n.HomeY; n.Stroll = false; n.Stuck = 0;
                    n.Facing = n.DefaultFacing;
                    n.Wait = 10 + (float)rng.NextDouble() * 16;
                }
                else
                {
                    n.TX = n.HomeX; n.TY = n.HomeY;
                    n.Facing = rng.Next(4);
                    n.Wait = 1.5f + (float)rng.NextDouble() * 3;
                }
                return;
            }
            float sp = Math.Min(d, (gs.Incident != null ? 3f : 1.5f) * dt);
            float nx = n.X + dx / d * sp, ny = n.Y + dy / d * sp;
            float pd = (nx - px) * (nx - px) + (ny - py) * (ny - py);
            if (Blocked(nx, ny, n) || pd < 0.5f)
            {
                // alguém no caminho: espera um pouco; se ficar travado, volta para casa
                n.Stuck += dt;
                if (!goingHome) { n.TX = n.HomeX; n.TY = n.HomeY; }
                else if (n.Stuck > 4f && pd > 9f) { n.X = n.HomeX; n.Y = n.HomeY; }
                n.Wait = 0.4f;
                return;
            }
            n.Stuck = 0;
            n.X = nx; n.Y = ny; n.Moving = true; n.Walk += sp * 2.5f; NpcStep(n);
            n.Facing = ScreenFacing(dx, dy);
        }

        FocusTarget FindFocus()
        {
            FocusTarget best = null;
            float bestScore = float.MaxValue;
            float fx = dirX, fy = dirY;
            Action<float, float, float, FocusTarget> consider = (x, y, range, f) =>
            {
                float dx = x - px, dy = y - (py - 0.3f), d = (float)Math.Sqrt(dx * dx + dy * dy);
                if (d > range) return;
                float dot = d > 0.01f ? (dx * fx + dy * fy) / d : 1;
                float score = d - dot * 0.5f;
                if (score < bestScore) { bestScore = score; best = f; }
            };
            foreach (var n in World.Npcs)
                consider(n.X, n.Y - 0.3f, 1.7f, new FocusTarget { Npc = n, Label = "Falar com " + n.Name + (ActivityLabel(n) != null ? "  ·  " + ActivityLabel(n) : "") });
            foreach (var it in World.Items)
            {
                string label = it.Kind == "terminal" ? TerminalVerb(it.Chapter) : it.Kind == "shop" ? "Abrir lojinha" : it.Kind == "lesson" ? "Assistir a uma aula no telão" : it.Kind == "docs" ? (it.Text == null ? "Pesquisar na documentação" : "Consultar " + it.Title.ToLowerInvariant()) : it.Kind == "coffee" ? "Tomar café (15 moedas)" : it.Kind == "cooler" ? "Beber água (8 moedas)" : it.Kind == "lunch" ? "Almoçar" : it.Kind == "arcade" ? "Jogar Invasores de Bugs" : it.Kind == "vending" ? "Comprar um lanche (20 moedas)" : it.Kind == "sala" ? RoomVerb(it) : it.Kind == "activity" ? ItemVerb(it) : "Ler " + it.Title.ToLowerInvariant();
                consider(it.X, it.Y, 1.5f, new FocusTarget { Item = it, Label = label });
            }
            for (int ch = 1; ch < 7; ch++)
            {
                var d = World.Doors[ch];
                if (!DoorLocked(ch)) continue;
                var c = d.Vertical ? new PointF(d.Tiles[0].X + 0.5f, 15f) : new PointF(d.Tiles[0].X + 1f, d.Tiles[0].Y + 0.5f);
                consider(c.X, c.Y, 1.8f, new FocusTarget { Door = d, Label = "Porta trancada" });
            }
            return best;
        }

        void ComputeObjective(int zone)
        {
            if (gs.Incident != null)
            {
                int s = (int)Math.Ceiling(gs.IncTime);
                var dsk = World.TerminalDesks[gs.Incident.Room];
                objective = "RELÂMPAGO: " + gs.Incident.Title + " — terminal de " + Story.Sectors[gs.Incident.Room].Name + " (" + (s / 60) + ":" + (s % 60).ToString("00") + ")";
                target = new PointF(dsk.X + dsk.W / 2, dsk.Y + 1.3f);
                return;
            }
            var cat = zone >= 0 ? Challenges.CatOfRoom(zone) : null;
            if (cat != null)
            {
                var mentor = World.Mentors[zone];
                if (!gs.Prog.Met.Contains(zone))
                {
                    objective = "Fale com " + mentor.Name + " (" + mentor.Role + ") para liberar o terminal de " + cat.Name;
                    target = new PointF(mentor.X, mentor.Y);
                    return;
                }
                var dsk = World.TerminalDesks[zone];
                int maxT = gs.MaxTier(cat.Id);
                objective = gs.CatDone(cat.Id) ? cat.Name + ": todos os níveis concluídos! Explore outra área." :
                    cat.Name + " · " + Challenges.TierNames[maxT] + " " + gs.TierCount(cat.Id, maxT) + "/" + gs.TierLevels(cat.Id, maxT).Count + " — use o terminal da área";
                target = gs.CatDone(cat.Id) ? (PointF?)null : new PointF(dsk.X + dsk.W / 2, dsk.Y + 1.3f);
                return;
            }
            target = null;
            int met = Challenges.Cats.Count(c => gs.Prog.Met.Contains(c.Room));
            if (met == 0) objective = "Explore o campus: escolha uma área e fale com o líder dela (todas estão abertas!)";
            else
            {
                int xp = Career.TotalXp(gs.Prog), next = Career.NextXp(xp);
                objective = Career.Title(gs.Prog) + " — " + (next > xp ? (next - xp) + " XP para a próxima promoção" : "topo da carreira!") + " · " + gs.BadgeCount + "/" + Challenges.Cats.Length * 3 + " badges";
            }
        }
        // ─────────────────────────── desenho ───────────────────────────

        int S(float v) { return (int)Math.Round(v * scale); }

        class PlateItem { public DoorInfo Door; }
        // texto desenhado em alta resolução sobre a pixel art (X0/YPlane = canto em pixels do mundo)
        class WallText { public string Text; public float X0, YPlane, W, H; public Color C; }
        readonly List<WallText> wallTexts = new List<WallText>();
        class DecorItem { public Furn F; public bool Divider; }

        // ── superfície de baixa resolução: uma DIB compartilhada entre GDI+ (desenho) e GDI (ampliação rápida)
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        struct BITMAPINFOHEADER { public int biSize, biWidth, biHeight; public short biPlanes, biBitCount; public int biCompression, biSizeImage, biXPelsPerMeter, biYPelsPerMeter, biClrUsed, biClrImportant; }
        [System.Runtime.InteropServices.DllImport("gdi32.dll")] static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFOHEADER bmi, uint usage, out IntPtr bits, IntPtr section, uint offset);
        [System.Runtime.InteropServices.DllImport("gdi32.dll")] static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [System.Runtime.InteropServices.DllImport("gdi32.dll")] static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);
        [System.Runtime.InteropServices.DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr obj);
        [System.Runtime.InteropServices.DllImport("gdi32.dll")] static extern bool DeleteDC(IntPtr hdc);
        [System.Runtime.InteropServices.DllImport("gdi32.dll")] static extern int SetStretchBltMode(IntPtr hdc, int mode);
        [System.Runtime.InteropServices.DllImport("gdi32.dll")] static extern bool StretchBlt(IntPtr dst, int x, int y, int w, int h, IntPtr src, int sx, int sy, int sw, int sh, uint rop);
        [System.Runtime.InteropServices.DllImport("gdi32.dll")] static extern bool GdiFlush();
        IntPtr lowDC, lowDib, lowOld;

        void CreateLowSurface(int lw, int lh)
        {
            if (low != null) low.Dispose();
            if (lowDC != IntPtr.Zero) { SelectObject(lowDC, lowOld); DeleteObject(lowDib); DeleteDC(lowDC); }
            var bi = new BITMAPINFOHEADER { biSize = 40, biWidth = lw, biHeight = -lh, biPlanes = 1, biBitCount = 32 };
            IntPtr bits;
            lowDib = CreateDIBSection(IntPtr.Zero, ref bi, 0, out bits, IntPtr.Zero, 0);
            lowDC = CreateCompatibleDC(IntPtr.Zero);
            lowOld = SelectObject(lowDC, lowDib);
            low = new Bitmap(lw, lh, lw * 4, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, bits);
        }

        // o mundo cobre a tela inteira: não precisa pintar o fundo (economiza uma limpeza por quadro)
        protected override void OnPaintBackground(PaintEventArgs e) { }

        // câmera suave (posição em pixels do mundo, com fração para rolagem sem tremor)
        float camX = float.NaN, camY, camFX, camFY;
        bool camMoving;
        float camDt;

        // ── quadro pronto (DIB do tamanho da janela). O quadro é desenhado aqui e só depois copiado para a
        //    tela num momento fixo do ciclo do monitor: cada quadro fica exatamente o mesmo tempo na tela.
        [System.Runtime.InteropServices.DllImport("gdi32.dll")] static extern bool BitBlt(IntPtr dst, int x, int y, int w, int h, IntPtr src, int sx, int sy, uint rop);
        IntPtr frameDC, frameDib, frameOld;
        Bitmap frameBmp;
        int tickSerial, renderSerial = -1;
        internal volatile bool presentPending;
        internal double RenderMs = 10;

        void CreateFrameSurface(int w, int h)
        {
            if (frameBmp != null) frameBmp.Dispose();
            if (frameDC != IntPtr.Zero) { SelectObject(frameDC, frameOld); DeleteObject(frameDib); DeleteDC(frameDC); }
            var bi = new BITMAPINFOHEADER { biSize = 40, biWidth = w, biHeight = -h, biPlanes = 1, biBitCount = 32 };
            IntPtr bits;
            frameDib = CreateDIBSection(IntPtr.Zero, ref bi, 0, out bits, IntPtr.Zero, 0);
            frameDC = CreateCompatibleDC(IntPtr.Zero);
            frameOld = SelectObject(frameDC, frameDib);
            frameBmp = new Bitmap(w, h, w * 4, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, bits);
            frameBits = bits;
        }

        // copia o quadro pronto para a tela (rápido: uma cópia só)
        protected override void OnPaint(PaintEventArgs e)
        {
            int vw = ClientSize.Width, vh = ClientSize.Height;
            if (vw <= 0 || vh <= 0) return;
            if (frameBmp == null || frameBmp.Width != vw || frameBmp.Height != vh || renderSerial != tickSerial) RenderFrame();
            if (frameBmp == null) return;
            IntPtr hdc = e.Graphics.GetHdc();
            try { BitBlt(hdc, 0, 0, vw, vh, frameDC, 0, 0, 0x00CC0020); }
            finally { e.Graphics.ReleaseHdc(hdc); }
        }

        void Present()
        {
            if (IsDisposed || !IsHandleCreated) return;
            if (isoMode && glHost != null && glHost.Ctx != IntPtr.Zero) GL.SwapBuffers(glHost.Hdc);
            else { Invalidate(); Update(); }
#if DEMO
            PresentLag.Add(clock.Elapsed.TotalMilliseconds - LastFlushMs);
#endif
        }
        internal double LastFlushMs;
#if DEMO
        public readonly List<double> PresentLag = new List<double>();
#endif

        // ── Renderização HD: o mundo é desenhado direto na resolução da tela, com bordas suaves.
        //    Para continuar leve, quase tudo vem pronto de caches: o chão em blocos de 8×8 tiles e cada
        //    parede, móvel e personagem já na escala da tela. A cada quadro o jogo só copia essas imagens
        //    e desenha as poucas coisas animadas (luzes dos servidores, telas, vapor do café...).
        int offX, offY;   // deslocamento da câmera em pixels da tela (inteiro: tudo alinhado, sem emendas)

        void RenderFrame()
        {
            if (isoMode && w3dIso != null) { RenderFrameIso(); return; }
            int vw = ClientSize.Width, vh = ClientSize.Height;
            if (vw <= 0 || vh <= 0) return;
            renderSerial = tickSerial;
            if (frameBmp == null || frameBmp.Width != vw || frameBmp.Height != vh) CreateFrameSurface(vw, vh);
            var ws = Scene.WorldSize;
            var pp = Scene.P(px, py, 0);
            float vwl = vw / (float)pix, vhl = vh / (float)pix;
            float padTop = S(110) / (float)pix, padB = S(90) / (float)pix, padX = S(60) / (float)pix;
            float tx = ws.Width + 2 * padX <= vwl ? (ws.Width - vwl) / 2f : Math.Max(-padX, Math.Min(ws.Width - vwl + padX, pp.X - vwl / 2f));
            float ty = ws.Height + padTop + padB <= vhl ? (ws.Height - vhl) / 2f : Math.Max(-padTop, Math.Min(ws.Height - vhl + padB, pp.Y - 30 - vhl / 2f));
            // a câmera avança com o mesmo passo de tempo do quadro (pinturas extras, como redimensionar, não a movem)
            float cdt = Math.Min(0.1f, camDt);
            camDt = 0;
            if (float.IsNaN(camX) || Math.Abs(tx - camX) > vwl || Math.Abs(ty - camY) > vhl) { camX = tx; camY = ty; }
            else
            {
                float k = 1f - (float)Math.Exp(-cdt * 12);
                camX += (tx - camX) * k; camY += (ty - camY) * k;
            }
            camMoving = Math.Abs(tx - camX) + Math.Abs(ty - camY) > 0.3f;
            camXi = (int)Math.Floor(camX); camYi = (int)Math.Floor(camY);
            camFX = camX - camXi; camFY = camY - camYi;
            offX = -(int)Math.Round(camX * pix); offY = -(int)Math.Round(camY * pix);

#if DEMO
            var psw = System.Diagnostics.Stopwatch.StartNew();
#endif
            using (var g = Graphics.FromImage(frameBmp))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                DrawFloorChunks(g, vw, vh);
#if DEMO
                Prof[0] += psw.Elapsed.TotalMilliseconds; psw.Restart();
                try { DrawWorld(g, new RectangleF(camX, camY, vwl, vhl)); }
                catch (Exception ex) { System.IO.File.AppendAllText(System.IO.Path.Combine(Environment.GetEnvironmentVariable("DOCKERQUEST_DEMO") ?? ".", "paint_erro.txt"), ex + "\n"); }
                Prof[1] += psw.Elapsed.TotalMilliseconds; psw.Restart();
#else
                DrawWorld(g, new RectangleF(camX, camY, vwl, vhl));
#endif
                g.ResetTransform();
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.PixelOffsetMode = PixelOffsetMode.Default;
                DrawOverlays(g, vw, vh);
#if DEMO
                Prof[2] += psw.Elapsed.TotalMilliseconds; psw.Restart();
#endif
                DrawHud(g, vw, vh, camXi, camYi);
#if DEMO
                Prof[3] += psw.Elapsed.TotalMilliseconds;
#endif
                DrawSplash(g, vw, vh);
                DrawIsoLoading(g, vw, vh);
                if (Paused) Art.R(g, Art.A(Color.Black, 110), 0, 0, vw, vh);
            }
            if (isoLoading) return;   // enquanto o chão do isométrico carrega, não disputa tempo com ele
#if DEMO
            var wsw = System.Diagnostics.Stopwatch.StartNew();
            WarmCaches();
            Prof[8] += wsw.Elapsed.TotalMilliseconds;
#else
            WarmCaches();
#endif
        }

        // posição na tela (alta resolução) de um ponto do mundo
        PointF Scr(float x, float y, float z)
        {
            if (isoMode && w3dIso != null) { bool front; return w3dIso.Project(x, y, z / Scene.T, ClientSize.Width, ClientSize.Height, out front); }
            var p = Scene.P(x, y, z);
            return new PointF((p.X - camX) * pix, (p.Y - camY) * pix);
        }

        // ── chão em blocos (cache LRU) ──
        const int ChunkTiles = 8;
        readonly Dictionary<int, Bitmap> floorChunks = new Dictionary<int, Bitmap>();
        readonly LinkedList<int> chunkLru = new LinkedList<int>();
        int ChunkPx { get { return ChunkTiles * Scene.T; } }

        void DrawFloorChunks(Graphics g, int vw, int vh)
        {
            g.ResetTransform();
            g.CompositingMode = CompositingMode.SourceCopy;
            g.Clear(Art.Void);
            int cs = ChunkPx;
            int c0x = (int)Math.Floor((camX - Scene.OX) / cs), c1x = (int)Math.Floor((camX + vw / (float)pix - Scene.OX) / cs);
            int c0y = (int)Math.Floor((camY - Scene.OY) / cs), c1y = (int)Math.Floor((camY + vh / (float)pix - Scene.OY) / cs);
            int maxX = (World.W + ChunkTiles - 1) / ChunkTiles, maxY = (World.H + ChunkTiles - 1) / ChunkTiles;
            for (int cy = Math.Max(0, c0y); cy <= Math.Min(maxY - 1, c1y); cy++)
                for (int cx = Math.Max(0, c0x); cx <= Math.Min(maxX - 1, c1x); cx++)
                    g.DrawImageUnscaled(Chunk(cx, cy), ((int)Scene.OX + cx * cs) * pix + offX, ((int)Scene.OY + cy * cs) * pix + offY);
            g.CompositingMode = CompositingMode.SourceOver;
        }

        Bitmap Chunk(int cx, int cy)
        {
            int key = cx * 1000 + cy;
            Bitmap b;
            if (floorChunks.TryGetValue(key, out b)) { chunkLru.Remove(key); chunkLru.AddFirst(key); return b; }
            b = Scene.FloorChunk(World, cx * ChunkTiles, cy * ChunkTiles, ChunkTiles);
            floorChunks[key] = b;
            chunkLru.AddFirst(key);
            int max = Math.Max(24, (int)(72L * 1024 * 1024 / ((long)ChunkPx * pix * ChunkPx * pix * 4)));
            while (chunkLru.Count > max) { int old = chunkLru.Last.Value; chunkLru.RemoveLast(); floorChunks[old].Dispose(); floorChunks.Remove(old); }
            return b;
        }

        // aproveita a folga do quadro para preparar os blocos de chão e os móveis ao redor (sem engasgo ao andar)
        void WarmCaches()
        {
            var sw = Stopwatch.StartNew();
            int cs = ChunkPx;
            int pcx = (int)Math.Floor((Scene.P(px, py, 0).X - Scene.OX) / cs), pcy = (int)Math.Floor((Scene.P(px, py, 0).Y - Scene.OY) / cs);
            int maxX = (World.W + ChunkTiles - 1) / ChunkTiles, maxY = (World.H + ChunkTiles - 1) / ChunkTiles;
            for (int r = 0; r <= 3 && sw.ElapsedMilliseconds < 3; r++)
                for (int cy = pcy - r; cy <= pcy + r && sw.ElapsedMilliseconds < 3; cy++)
                    for (int cx = pcx - r - 1; cx <= pcx + r + 1 && sw.ElapsedMilliseconds < 3; cx++)
                        if (cx >= 0 && cy >= 0 && cx < maxX && cy < maxY && !floorChunks.ContainsKey(cx * 1000 + cy)) Chunk(cx, cy);
            if (sw.ElapsedMilliseconds >= 3) return;
            // quadros de caminhada: do jogador e dos colegas por perto
            var looks = new List<Look> { playerLook };
            foreach (var n in World.Npcs)
                if (Math.Abs(n.X - px) <= 12 && Math.Abs(n.Y - py) <= 8) looks.Add(n.Look);
            foreach (var lk in looks)
                for (int fc = 0; fc < 4; fc++)
                    for (int fr = 0; fr <= 6; fr++)
                    {
                        if (sw.ElapsedMilliseconds >= 3) return;
                        Scene.CharSprite(lk, fc, fr > 0, fr == 0 ? 0 : (fr - 1) * (float)(Math.PI / 3) + 0.01f);
                    }
            if (sw.ElapsedMilliseconds >= 3) return;
            // móveis e paredes próximos (até ~1,5 tela de distância)
            if (warmList == null)
            {
                warmList = new List<object>();
                warmList.AddRange(wallRuns.Cast<object>());
                warmList.AddRange(World.Furniture.Cast<object>());
                warmList.AddRange(World.Decor.Select(d => (object)new DecorItem { F = d, Divider = GlassAt((int)(d.X + d.W / 2), (int)d.Y) }));
            }
            for (int i = 0; i < warmList.Count && sw.ElapsedMilliseconds < 3; i++)
            {
                var o = warmList[i];
                float ox, oy;
                var wp = o as WallRun; var fu = o as Furn; var di = o as DecorItem;
                if (wp != null) { ox = wp.X0; oy = wp.Y; } else if (fu != null) { ox = fu.X; oy = fu.Y; } else { ox = di.F.X; oy = di.F.Y; }
                if (Math.Abs(ox - px) > 30 || Math.Abs(oy - py) > 18) continue;
                if (StaticReady(o)) { warmList.RemoveAt(i); i--; continue; }
                using (var dummy = Graphics.FromImage(warmBmp)) DrawItem(dummy, o, true);
                warmList.RemoveAt(i); i--;
            }
        }
        List<object> warmList;
        readonly Bitmap warmBmp = new Bitmap(1, 1);

        // ── cache de peças estáticas, compartilhado entre peças iguais (ex.: todos os vasos de planta iguais)
        class StaticSprite { public Bitmap Bmp; public float SX, SY; public bool Opaque; }
        readonly Dictionary<string, StaticSprite> statics = new Dictionary<string, StaticSprite>();
        static readonly HashSet<string> AnimatedFurniture = new HashSet<string> { "terminal", "rack", "coffee", "fountain", "robot", "arcade" };

        static RectangleF TopBounds(float x, float y, float w, float h, float zmax)
        {
            var a = Scene.P(x, y, zmax);
            var b = Scene.P(x + w, y + h, 0);
            return RectangleF.FromLTRB(a.X, a.Y, b.X, b.Y);
        }

        static string FurnKey(Furn f)
        {
            // peças iguais na mesma posição dentro do tile compartilham a imagem; estantes e quiosques variam pela posição
            string k = "F" + f.Kind + "|" + f.W + "|" + f.H + "|" + f.C.ToArgb() + "|" + f.Var + "|" + (f.X - (float)Math.Floor(f.X)) + "|" + (f.Y - (float)Math.Floor(f.Y));
            if (f.Kind == "shelf" || f.Kind == "kiosk" || f.Kind == "tree") k += "|" + f.X + "|" + f.Y;
            return k;
        }

        string StaticKey(object o)
        {
            var wr = o as WallRun;
            if (wr != null) return WallRunKey(wr);
            var fu = o as Furn;
            if (fu != null) return FurnKey(fu);
            var di = o as DecorItem;
            if (di != null) return "D" + di.F.Kind + "|" + di.F.X + "|" + di.F.Y + "|" + di.F.W + "|" + di.Divider;
            return null;
        }

        static string WallRunKey(WallRun r)
        {
            var sb = new System.Text.StringBuilder("W");
            foreach (var p in r.Pieces) sb.Append(p.Glass ? 'g' : 'w').Append(p.H).Append(':').Append(p.Face.ToArgb()).Append('|');
            return sb.ToString();
        }

        bool StaticReady(object o) { string k = StaticKey(o); return k == null || statics.ContainsKey(k); }

        // anchor = ponto de referência da peça no mundo (pixels do mundo); a imagem fica guardada relativa a ele
        void DrawStatic(Graphics g, string key, PointF anchor, RectangleF b, Action<Graphics> draw)
        {
            StaticSprite s;
            if (!statics.TryGetValue(key, out s))
            {
                int x0 = (int)Math.Floor(b.X) - 4, y0 = (int)Math.Floor(b.Y) - 4;
                int w = Math.Max(1, (int)Math.Ceiling(b.Right) + 4 - x0), h = Math.Max(1, (int)Math.Ceiling(b.Bottom) + 4 - y0);
                var bmp = new Bitmap(w * pix, h * pix, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                using (var gg = Graphics.FromImage(bmp))
                {
                    gg.SmoothingMode = SmoothingMode.AntiAlias;
                    gg.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    gg.ScaleTransform(pix, pix);
                    gg.TranslateTransform(-x0, -y0);
                    draw(gg);
                }
                int tx, ty; bool opaque;
                bmp = TrimBitmap(bmp, out tx, out ty, out opaque);   // sem margens transparentes: cada quadro mistura menos pixels
                s = new StaticSprite { Bmp = bmp, SX = (x0 - anchor.X) * pix + tx, SY = (y0 - anchor.Y) * pix + ty, Opaque = opaque };
                statics[key] = s;
            }
            if (g.VisibleClipBounds.Width <= 1) return;   // só aquecendo o cache
            g.ResetTransform();
            // peças totalmente opacas (paredes de alvenaria) são copiadas direto, sem cálculo de transparência
            if (s.Opaque) g.CompositingMode = CompositingMode.SourceCopy;
            g.DrawImageUnscaled(s.Bmp, (int)Math.Round(anchor.X * pix + s.SX) + offX, (int)Math.Round(anchor.Y * pix + s.SY) + offY);
            if (s.Opaque) g.CompositingMode = CompositingMode.SourceOver;
        }

        static Bitmap TrimBitmap(Bitmap b, out int ox, out int oy, out bool opaque)
        {
            ox = oy = 0; opaque = false;
            var rect = new Rectangle(0, 0, b.Width, b.Height);
            var data = b.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            var px = new int[b.Width * b.Height];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, px, 0, px.Length);
            b.UnlockBits(data);
            int minX = b.Width, minY = b.Height, maxX = -1, maxY = -1;
            for (int y = 0; y < b.Height; y++)
                for (int x = 0; x < b.Width; x++)
                    if ((px[y * b.Width + x] >> 24) != 0)
                    {
                        if (x < minX) minX = x; if (x > maxX) maxX = x;
                        if (y < minY) minY = y; if (y > maxY) maxY = y;
                    }
            if (maxX < 0) { b.Dispose(); return new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppPArgb); }
            opaque = true;
            for (int y = minY; y <= maxY && opaque; y++)
                for (int x = minX; x <= maxX; x++)
                    if (((uint)px[y * b.Width + x] >> 24) != 255) { opaque = false; break; }
            if (minX == 0 && minY == 0 && maxX == b.Width - 1 && maxY == b.Height - 1) return b;
            var crop = b.Clone(new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            b.Dispose();
            ox = minX; oy = minY;
            return crop;
        }

        // desenho vetorial direto no mundo (coisas animadas, sombras, marcadores)
        void WorldSpace(Graphics g)
        {
            g.ResetTransform();
            g.TranslateTransform(offX, offY);
            g.ScaleTransform(pix, pix);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        }

        readonly Dictionary<int, Scene.WallPiece> wallAt = new Dictionary<int, Scene.WallPiece>();
        bool GlassAt(int x, int y) { Scene.WallPiece w; return wallAt.TryGetValue(x * 1000 + y, out w) && w.Glass; }

        void DrawWorld(Graphics g, RectangleF view)
        {
            wallTexts.Clear();
            int curRoom = World.ZoneAt(px, py);
            Func<float, float, float, bool> inViewW = (wx, wy, hw) => { var q = Scene.P(wx, wy, 0); float m = 56 + hw * Scene.T; return q.X > view.Left - m && q.X < view.Right + m && q.Y > view.Top - 24 && q.Y < view.Bottom + 100; };
            Func<float, float, bool> inView = (wx, wy) => inViewW(wx, wy, 0);
            WorldSpace(g);
            for (int r = 0; r < Story.RoomCount; r++)
                if (World.TerminalDesks[r] != null && inView(World.TerminalDesks[r].X, World.TerminalDesks[r].Y))
                    Scene.Glow(g, World.TerminalDesks[r], time, (gs.Incident != null && gs.Incident.Room == r) || (gs.Incident == null && r == curRoom && gs.Prog.Met.Contains(r)));

            // anel no chão marcando o objetivo
            if (target.HasValue && page == null)
            {
                var c = Scene.P(target.Value.X, target.Value.Y, 0);
                float pulse = 0.75f + 0.25f * (float)Math.Sin(time * 5);
                float rw = Scene.T * 1.1f * pulse, rh = rw * 0.45f;
                using (var pen = new Pen(Art.Hx(0xF0B429), 1.6f)) g.DrawEllipse(pen, c.X - rw / 2, c.Y - rh / 2, rw, rh);
                using (var pen = new Pen(Art.A(Art.Hx(0xF0B429), 110), 1f)) g.DrawEllipse(pen, c.X - rw / 2 - 2, c.Y - rh / 2 - 1, rw + 4, rh + 2);
            }

            var draw = new List<KeyValuePair<float, object>>(800);
            foreach (var w in wallRuns)
                if (inViewW(w.X0 + w.Pieces.Count / 2f, w.Y, w.Pieces.Count * 0.5f)) draw.Add(new KeyValuePair<float, object>(w.Key, w));
            foreach (var d in World.AllDoors)
            {
                if (!inView(d.Tiles[0].X, d.Tiles[0].Y)) continue;
                draw.Add(new KeyValuePair<float, object>(d.Tiles[0].Y + 1.02f, d));
                draw.Add(new KeyValuePair<float, object>(d.Tiles[0].Y + 1.03f, new PlateItem { Door = d }));
            }
            foreach (var d in World.Decor)
            {
                if (!inView(d.X + d.W / 2, d.Y)) continue;
                draw.Add(new KeyValuePair<float, object>(d.Y + 1.01f, new DecorItem { F = d, Divider = GlassAt((int)(d.X + d.W / 2), (int)d.Y) }));
            }
            foreach (var f in World.Furniture)
                if (inViewW(f.X + f.W / 2, f.Y + f.H, f.W / 2)) draw.Add(new KeyValuePair<float, object>(Scene.FurnKey(f), f));
            // personagens entram na mesma ordenação por profundidade (quem está mais abaixo fica na frente)
            foreach (var n in World.Npcs)
                if (inView(n.X, n.Y)) draw.Add(new KeyValuePair<float, object>(n.Y, n));
            draw.Add(new KeyValuePair<float, object>(py, this));
            if (playerLook.Pet != null && petInit) draw.Add(new KeyValuePair<float, object>(petY, "pet"));
            draw.Sort((a, b) => a.Key.CompareTo(b.Key));

#if DEMO
            var isw = new System.Diagnostics.Stopwatch();
            foreach (var kv in draw)
            {
                isw.Restart();
                DrawItem(g, kv.Value, false);
                int slot = kv.Value is WallRun ? 4 : kv.Value is Furn ? (AnimatedFurniture.Contains(((Furn)kv.Value).Kind) ? 6 : 5) : (kv.Value is DecorItem || kv.Value is DoorInfo || kv.Value is PlateItem) ? 9 : 7;
                Prof[slot] += isw.Elapsed.TotalMilliseconds;
            }
            ProfItems += draw.Count;
#else
            foreach (var kv in draw) DrawItem(g, kv.Value, false);
#endif
        }

        // desenha um objeto do cenário (ou só prepara o cache, com warm = true)
        void DrawItem(Graphics g, object o, bool warm)
        {
            if (o is WallRun)
            {
                var wr = (WallRun)o;
                DrawStatic(g, StaticKey(wr), Scene.P(wr.X0, wr.Y, 0), TopBounds(wr.X0, wr.Y, wr.Pieces.Count, 1, Scene.WALL_H + 4),
                    gg => { foreach (var wp in wr.Pieces) Scene.DrawWall(gg, wp); });
            }
            else if (o is DoorInfo)
            {
                var dr = (DoorInfo)o;
                DrawStatic(g, "door|" + dr.Tiles[0].X + "|" + dr.Tiles[0].Y, Scene.P(dr.Tiles[0].X, dr.Tiles[0].Y, 0),
                    TopBounds(dr.Tiles[0].X - 0.3f, dr.Tiles[0].Y, 2.6f, 1.4f, Scene.GLASS_H + 10), gg => Scene.Door(gg, dr, false, 0));
            }
            else if (o is PlateItem)
            {
                if (warm) return;
                var d = ((PlateItem)o).Door;
                var pc = Challenges.CatOfRoom(d.Chapter);
                bool lk = false, dn = pc != null && gs.CatDone(pc.Id);
                var prr = Scene.PlateRect(d);
                prr.Inflate(4, 4);
                DrawStatic(g, "plate|" + d.Tiles[0].X + "|" + d.Tiles[0].Y + "|" + dn, Scene.P(d.Tiles[0].X, d.Tiles[0].Y, 0), prr, gg => Scene.Plate(gg, d, lk, dn));
                var pr = Scene.PlateRect(d);
                wallTexts.Add(new WallText { Text = Scene.PlateLabel(d, lk, dn), X0 = pr.X + 3, YPlane = pr.Y, W = pr.Width - 4, H = pr.Height, C = Color.White });
            }
            else if (o is DecorItem)
            {
                var di = (DecorItem)o;
                DrawStatic(g, StaticKey(di), Scene.P(di.F.X, di.F.Y, 0), TopBounds(di.F.X, di.F.Y, di.F.W, 1, Scene.WALL_H + 4), gg => Scene.Decor(gg, di.F, di.Divider));
                if (warm) return;
                if (di.F.Kind == "sign" || (di.F.Kind == "whiteboard" && di.F.Text != null))
                {
                    var r = Scene.DecorRect(di.F, di.Divider);
                    if (di.F.Kind == "sign") wallTexts.Add(new WallText { Text = di.F.Text, X0 = r.X, YPlane = r.Y + 4, W = r.Width, H = r.Height - 10, C = Color.White });
                    else wallTexts.Add(new WallText { Text = di.F.Text, X0 = r.X + 3, YPlane = r.Y + 3, W = r.Width - 6, H = 9, C = Art.Hx(0x1D4ED8) });
                }
            }
            else if (o is Furn)
            {
                var fu = (Furn)o;
                var anchor = Scene.P((float)Math.Floor(fu.X), (float)Math.Floor(fu.Y), 0);
                DrawStatic(g, FurnKey(fu), anchor, TopBounds(fu.X - 0.6f, fu.Y - 0.2f, fu.W + 1.2f, fu.H + 1.2f, 80), gg => Scene.Furniture(gg, fu, 0, 1));
                if (warm) return;
                if (AnimatedFurniture.Contains(fu.Kind)) { WorldSpace(g); Scene.Furniture(g, fu, time, 2); }
                if (fu.Kind == "kiosk")
                {
                    var kp = Scene.P(fu.X + fu.W / 2, fu.Y + 1f, 18);
                    wallTexts.Add(new WallText { Text = "LOJINHA", X0 = kp.X - 22, YPlane = kp.Y + 4, W = 44, H = 10, C = Color.White });
                }
            }
            else if (!warm) DrawActor(g, o);
        }

        // personagens: sombra suave + sprite pronto (já na escala da tela)
        void DrawActor(Graphics g, object o)
        {
            Bitmap spr; int fx, fy; float x, y;
            WorldSpace(g);
            if (o is Npc)
            {
                var n = (Npc)o; x = n.X; y = n.Y;
                Scene.Shadow(g, x, y, 0.55f, 80);
                spr = Scene.CharSprite(n.Look, n.Facing, n.Moving, n.Walk); fx = Scene.FootX; fy = Scene.FootY;
            }
            else if (o is string)
            {
                x = petX; y = petY;
                Scene.Shadow(g, x, y, 0.35f, 70);
                spr = Scene.PetSprite(playerLook.Pet, petFacing, petMoving, time); fx = Scene.PFootX; fy = Scene.PFootY;
            }
            else
            {
                x = px; y = py;
                Scene.Shadow(g, x, y, 0.55f, 90);
                spr = Scene.CharSprite(playerLook, facing, moving, walk); fx = Scene.FootX; fy = Scene.FootY;
            }
            var p = Scene.P(x, y, 0);
            g.ResetTransform();
            g.DrawImageUnscaled(spr, (int)Math.Round((p.X - fx) * pix) + offX, (int)Math.Round((p.Y - fy) * pix) + offY);
        }

        void DrawSplash(Graphics g, int vw, int vh)
        {
            if (splash <= 0) return;
            float h = Math.Min(S(120), vw / 7f), w = h * 4.6f + S(60), x = (vw - w) / 2, y = vh * 0.16f - (splash < 0.5f ? (0.5f - splash) * S(300) : 0);
            Card(g, x, y, w, h + S(48));
            Art.Wordmark(g, x + S(30), y + S(24), h, Color.White);
        }

        readonly Dictionary<int, Font> wallFonts = new Dictionary<int, Font>();

        void DrawOverlays(Graphics g, int vw, int vh)
        {
            // textos das placas e quadros: desenhados em alta resolução por cima da pixel art
            if (!isoMode)
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap, Trimming = StringTrimming.None })
                foreach (var wt in wallTexts)
                {
                    float sx = (wt.X0 - camX) * pix, sy = (wt.YPlane - camY) * pix, sw = wt.W * pix, sh = wt.H * pix;
                    if (sx + sw < -20 || sy + sh < -20 || sx > vw + 20 || sy > vh + 20) continue;
                    int size = Math.Max(6, (int)(sh * 0.72f));
                    Font f;
                    if (!wallFonts.TryGetValue(size, out f)) { f = new Font("Segoe UI Black", size, GraphicsUnit.Pixel); wallFonts[size] = f; }
                    var tsz = g.MeasureString(wt.Text, f);
                    if (tsz.Width > sw)
                    {
                        int s2 = Math.Max(6, (int)(size * sw / tsz.Width));
                        if (!wallFonts.TryGetValue(s2, out f)) { f = new Font("Segoe UI Black", s2, GraphicsUnit.Pixel); wallFonts[s2] = f; }
                    }
                    g.DrawString(wt.Text, f, Art.B(wt.C), new RectangleF(sx, sy, sw, sh + 2), sf);
                }
            float head = Scene.CharU * 1.55f + 4;
            foreach (var n in World.Npcs)
            {
                var hp = Scr(n.X, n.Y, head);
                if (hp.X < -100 || hp.Y < -100 || hp.X > vw + 100 || hp.Y > vh + 100) continue;
                string mark = NpcMark(n);
                float dist = (float)Math.Sqrt((n.X - px) * (n.X - px) + (n.Y - py) * (n.Y - py));
                float top = hp.Y;
                if (dist < 3.2f)
                {
                    var sz = g.MeasureString(n.Name, fTag);
                    float tw = sz.Width + S(12), th = sz.Height + S(2);
                    float ty = top - th;
                    Art.RR(g, Art.A(Color.White, 235), hp.X - tw / 2, ty, tw, th, S(4));
                    Art.RRO(g, Art.Hx(0x1A1D26), Math.Max(1, S(1.5f)), hp.X - tw / 2, ty, tw, th, S(4));
                    Art.Text(g, n.Name, fTag, n.Kind == NpcKind.Mentor ? Art.Hx(0xB45309) : Art.Hx(0x1A1D26), new RectangleF(hp.X - tw / 2, ty, tw, th));
                    top = ty - S(3);
                }
                if (mark != null)
                {
                    float b = (float)Math.Sin(time * 4) * S(3), d = S(24), my = top - d + b;
                    Color mc = mark == "!!" ? Art.Hx(0xF85149) : mark == "!" ? Art.Hx(0xF0B429) : mark == "+" ? Art.Hx(0x8E6CC4) : Art.Hx(0x39C5CF);
                    top = my;
                    Art.RR(g, Art.Hx(0x1A1D26), hp.X - d / 2 - S(2), my - S(2), d + S(4), d + S(4), S(6));
                    Art.RR(g, mc, hp.X - d / 2, my, d, d, S(5));
                    Art.Text(g, mark, fMark, Color.White, new RectangleF(hp.X - d / 2, my, d, d));
                }
                // balão de fala (pânico / comemoração)
                KeyValuePair<string, float> bub;
                if (bubbles.TryGetValue(n, out bub) && page == null)
                {
                    var bs = g.MeasureString(bub.Key, fHudBold);
                    float bw = bs.Width + S(18), bh = bs.Height + S(10), bx = hp.X - bw / 2, by = top - bh - S(14);
                    bool alarm = gs.Incident != null;
                    Color bg = alarm ? Art.Hx(0xFFF1F0) : Color.White, fg = alarm ? Art.Hx(0xB42318) : Art.Hx(0x1A1D26);
                    Art.RR(g, Art.Hx(0x1A1D26), bx - S(2), by - S(2), bw + S(4), bh + S(4), S(9));
                    Art.RR(g, bg, bx, by, bw, bh, S(8));
                    g.FillPolygon(Art.B(Art.Hx(0x1A1D26)), new[] { new PointF(hp.X - S(7), by + bh), new PointF(hp.X + S(7), by + bh), new PointF(hp.X, by + bh + S(11)) });
                    g.FillPolygon(Art.B(bg), new[] { new PointF(hp.X - S(5), by + bh - 1), new PointF(hp.X + S(5), by + bh - 1), new PointF(hp.X, by + bh + S(7)) });
                    g.DrawString(bub.Key, fHudBold, Art.B(fg), bx + S(9), by + S(5));
                }
            }

            // seta amarela sobre o alvo
            if (target.HasValue && page == null)
            {
                var tp = Scr(target.Value.X, target.Value.Y, 70);
                tp.Y += (float)Math.Sin(time * 5) * S(6);
                if (tp.X > 0 && tp.Y > 0 && tp.X < vw && tp.Y < vh)
                {
                    float a = S(13);
                    var tri = new[] { new PointF(tp.X - a, tp.Y - a), new PointF(tp.X + a, tp.Y - a), new PointF(tp.X, tp.Y + a * 0.6f) };
                    g.FillPolygon(Art.B(Art.Hx(0x1A1D26)), tri.Select(p => new PointF(p.X, p.Y + S(3))).ToArray());
                    g.FillPolygon(Art.B(Art.Hx(0xF0B429)), tri);
                    g.DrawPolygon(Art.P(Art.Hx(0x1A1D26), Math.Max(1, S(1.5f))), tri);
                }
            }
        }
        static string TerminalVerb(int room)
        {
            var cat = Challenges.CatOfRoom(room);
            if (cat == null) return "Usar terminal";
            if (cat.Id == "aws") return "Abrir o Console AWS";
            if (cat.Id == "sql") return "Abrir o terminal MySQL";
            if (cat.Id == "front" || cat.Id == "react" || cat.Id == "flutter" || cat.Id == "python" || cat.Id == "php") return "Abrir o Micorosoft Code";
            return "Usar terminal";
        }

        static string ItemVerb(Item it)
        {
            string a = it.Text ?? "";
            if (a.StartsWith("water:")) return "Regar a árvore";
            switch (a) { case "rest": return "Fazer uma pausa"; case "pingpong": return "Jogar pingue-pongue"; case "robot": return "Falar com o robô"; default: return it.Title; }
        }

        bool ActivityFresh(Npc n)
        {
            switch (n.Activity)
            {
                case "talk": return Enumerable.Range(0, ActivityBank.Talk.Length).Any(i => gs.Prog.Stat("ok:talk:" + i) == 0);
                case "phishing": return Enumerable.Range(0, ActivityBank.Phishing.Length).Any(i => gs.Prog.Stat("ok:phish:" + i) == 0);
                case "ticket": return Enumerable.Range(0, ActivityBank.Tickets.Length).Any(i => gs.Prog.Stat("ok:ticket:" + i) == 0);
                case "bugs": return Enumerable.Range(0, ActivityBank.Bugs.Length).Any(i => gs.Prog.Stat("ok:bugs:" + i) == 0);
                case "ia": return Enumerable.Range(0, ActivityBank.Ai.Length).Any(i => gs.Prog.Stat("ai:" + i) == 0);
                case "garden": return gs.Prog.Stat("garden_done") == 0;
                case "network": return gs.Prog.Stat("net:" + n.Name) == 0;
                case "meal": return !lunched;
                default: return false;
            }
        }

        string NpcMark(Npc n)
        {
            if (gs.Incident != null && n.Name == gs.Incident.Reporter) return "!!";
            if (n.Event != null) return "…";
            if (n.Activity != null && ActivityFresh(n)) return "+";
            if (n.Ceo && !gs.Prog.Met.Contains(Story.Directors)) return "!";
            if (n.Chapter < 0 || Challenges.CatOfRoom(n.Chapter) == null) return null;
            var cat = Challenges.CatOfRoom(n.Chapter);
            bool met = gs.Prog.Met.Contains(n.Chapter);
            if (n.Kind == NpcKind.Mentor) return !met ? "!" : null;
            if (n.Kind == NpcKind.Helper && met && !gs.CatDone(cat.Id))
            {
                int lv;
                if (!gs.TerminalLevel.TryGetValue(cat.Id, out lv) || gs.Prog.Done(lv)) lv = gs.DefaultLevel(cat.Id);
                return gs.HintsOf(lv) == n.HelperIdx && gs.LevelList[lv].Hints.Length > n.HelperIdx ? "?" : null;
            }
            return null;
        }
        void Card(Graphics g, float x, float y, float w, float h)
        {
            Art.RR(g, Art.A(Color.Black, 70), x + 3, y + 4, w, h, S(10));
            Art.RR(g, Art.A(Art.Hx(0x0D1117), 225), x, y, w, h, S(10));
            Art.RRO(g, Art.A(Art.Hx(0x30363D), 255), 1, x, y, w, h, S(10));
        }

        class HudPiece { public string Key; public Bitmap Bmp; }
        readonly Dictionary<string, HudPiece> hudCache = new Dictionary<string, HudPiece>();

        // desenha um pedaço do HUD numa imagem e só redesenha quando o conteúdo (key) muda
        void CachedHud(Graphics g, string slot, string key, float fx, float fy, float fw, float fh, Action<Graphics> draw)
        {
            int x = (int)Math.Floor(fx), y = (int)Math.Floor(fy), w = (int)Math.Ceiling(fw) + 6, h = (int)Math.Ceiling(fh) + 6;
            HudPiece p;
            if (!hudCache.TryGetValue(slot, out p) || p.Key != key || p.Bmp.Width != w || p.Bmp.Height != h)
            {
                if (p != null) p.Bmp.Dispose();
                var bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                using (var gg = Graphics.FromImage(bmp))
                {
                    gg.SmoothingMode = SmoothingMode.AntiAlias;
                    gg.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    gg.TranslateTransform(-x, -y);
                    draw(gg);
                }
                p = new HudPiece { Key = key, Bmp = bmp };
                hudCache[slot] = p;
            }
            g.DrawImageUnscaled(p.Bmp, x, y);
        }

        void DrawHud(Graphics g, int vw, int vh, int cx, int cy)
        {
            int zone = World.ZoneAt(px, py);
            // objetivo
            float cw = Math.Min(S(560), vw - S(330));
            var osz = g.MeasureString("► " + objective, fHudBold, (int)(cw - S(40)));
            string where = zone == Story.Library ? "Biblioteca & Documentação" : zone >= 0 ? Story.Sectors[zone].Name : CorridorName();
            CachedHud(g, "objetivo", objective + "|" + where + "|" + cw, S(14), S(14), cw, S(52) + osz.Height, gg =>
            {
                Card(gg, S(14), S(14), cw, S(52) + osz.Height);
                Art.Logo(gg, S(26), S(19), S(24));
                gg.DrawString("STACK QUEST", fLogo, Art.B(Color.White), S(56), S(22));
                float wx = S(62) + gg.MeasureString("STACK QUEST", fLogo).Width;
                gg.DrawString(where, fHud, Art.B(Art.Hx(0x8B949E)), wx, S(24));
                gg.DrawString("► " + objective, fHudBold, Art.B(Color.White), new RectangleF(S(28), S(50), cw - S(40), osz.Height + S(4)));
            });

            // alarme do incidente: borda vermelha pulsando + cartão com contagem regressiva
            if (gs.Incident != null)
            {
                float pulse = 0.5f + 0.5f * (float)Math.Sin(time * 6);
                for (int i = 0; i < 6; i++)
                {
                    int a = (int)((60 - i * 9) * pulse);
                    var pen = Art.P(Art.A(Art.Hx(0xF85149), a), S(6));
                    g.DrawRectangle(pen, i * S(6), i * S(6), vw - i * S(12) - 1, vh - i * S(12) - 1);
                }
                float iy = S(28) + osz.Height + S(52), iw = cw, ih = S(104);
                Art.RR(g, Art.A(Color.Black, 70), S(14) + 3, iy + 4, iw, ih, S(10));
                Art.RR(g, Art.Hx(0x3A0D0D), S(14), iy, iw, ih, S(10));
                Art.RRO(g, Art.A(Art.Hx(0xF85149), 150 + (int)(100 * pulse)), Math.Max(1, S(2)), S(14), iy, iw, ih, S(10));
                g.DrawString("INCIDENTE RELÂMPAGO", fLogo, Art.B(Art.Hx(0xFF7B72)), S(28), iy + S(10));
                int s = (int)Math.Ceiling(Math.Max(0, gs.IncTime));
                string clock = (s / 60) + ":" + (s % 60).ToString("00");
                var cs = g.MeasureString(clock, fScore);
                g.DrawString(clock, fScore, Art.B(s <= 20 && pulse > 0.5f ? Art.Hx(0xFF7B72) : Color.White), S(14) + iw - cs.Width - S(14), iy + S(4));
                g.DrawString(gs.Incident.Title, fHudBold, Art.B(Color.White), new RectangleF(S(28), iy + S(34), iw - S(150), S(26)));
                g.DrawString("Terminal de " + Story.SectorOf(gs.Incident.Room).Name + "  ·  vale agora " + gs.IncidentValue() + " pts", fSmall, Art.B(Art.Hx(0xFFB4AB)), S(28), iy + S(62));
                float bw = iw - S(28);
                Art.RR(g, Art.Hx(0x5C2430), S(28), iy + S(86), bw, S(8), S(4));
                Art.RR(g, Art.Hx(0xF85149), S(28), iy + S(86), Math.Max(S(6), bw * gs.IncTime / Math.Max(1, gs.IncTotal)), S(8), S(4));
            }

            // barra de atalhos (consumíveis, teclas 1–6)
            {
                float slot = S(50), gap = S(6), hbw = Catalog.Hotbar.Length * (slot + gap) - gap;
                float hx = (vw - hbw) / 2, hy = vh - slot - S(18);
                string hkey = string.Join(",", Catalog.Hotbar.Select(id => gs.Prog.Count(id).ToString()).ToArray());
                CachedHud(g, "itens", hkey, hx - S(8), hy - S(8), hbw + S(16), slot + S(16), g2 =>
                {
                var g0 = g; g = g2;
                Card(g, hx - S(8), hy - S(8), hbw + S(16), slot + S(16));
                for (int i = 0; i < Catalog.Hotbar.Length; i++)
                {
                    string id = Catalog.Hotbar[i];
                    int n = gs.Prog.Count(id);
                    float bx = hx + i * (slot + gap);
                    Art.RR(g, n > 0 ? Art.Hx(0x1F2A3C) : Art.Hx(0x161B22), bx, hy, slot, slot, S(7));
                    Art.RRO(g, n > 0 ? Art.Hx(0x484F58) : Art.Hx(0x30363D), 1, bx, hy, slot, slot, S(7));
                    var st = g.Save();
                    if (n == 0) g.SetClip(new RectangleF(bx, hy, slot, slot));
                    Art.ItemIcon(g, id, new RectangleF(bx + S(8), hy + S(6), slot - S(16), slot - S(16)));
                    g.Restore(st);
                    if (n == 0) Art.RR(g, Art.A(Art.Hx(0x0D1117), 150), bx, hy, slot, slot, S(7));
                    g.DrawString((i + 1).ToString(), fSmall, Art.B(Art.Hx(0x8B949E)), bx + S(3), hy + S(1));
                    if (n > 0)
                    {
                        string cnt = "x" + n;
                        var ns = g.MeasureString(cnt, fSmall);
                        g.DrawString(cnt, fSmall, Art.B(Color.White), bx + slot - ns.Width - S(2), hy + slot - ns.Height);
                    }
                }
                g = g0;
                });
                var buffs = new List<string>();
                if (gs.SpeedBuff > 0) buffs.Add("café " + (int)gs.SpeedBuff + "s");
                if (gs.Prog.NextSolveBonus > 0) buffs.Add("pão de queijo +" + gs.Prog.NextSolveBonus);
                if (gs.Prog.Erasers > 0) buffs.Add("borracha x" + gs.Prog.Erasers);
                if (gs.Prog.NextIncidentTime > 0) buffs.Add("energético +" + gs.Prog.NextIncidentTime + "s");
                if (gs.Prog.NextIncidentBonus > 0) buffs.Add("pizza +" + gs.Prog.NextIncidentBonus);
                if (buffs.Count > 0)
                {
                    string bt = "ativos: " + string.Join("  ·  ", buffs);
                    var bsz = g.MeasureString(bt, fSmall);
                    g.DrawString(bt, fSmall, Art.B(Art.Hx(0x3FB950)), (vw - bsz.Width) / 2, hy - S(30));
                }
            }

            // carreira: pontos, título, XP, reputação, saldo e badges
            float sw = S(340), sx = vw - sw - S(14), sh = S(168);
            string ckey = gs.Prog.Total + "|" + gs.Prog.Wallet + "|" + Career.Title(gs.Prog) + "|" + Career.TotalXp(gs.Prog) + "|" + gs.Prog.Reputation + "|" + string.Join(",", gs.Prog.BadgeDates.Keys.ToArray());
            CachedHud(g, "carreira", ckey, sx, S(14), sw, sh, g2 =>
            {
            var g0 = g; g = g2;
            Card(g, sx, S(14), sw, sh);
            g.DrawString("★ " + gs.Prog.Total.ToString("N0") + " pts", fScore, Art.B(Art.Hx(0xF0B429)), sx + S(14), S(18));
            string wal = gs.Prog.Wallet.ToString("N0");
            var wsz = g.MeasureString(wal, fHud);
            g.DrawString(wal, fHud, Art.B(Art.Hx(0xF5C542)), sx + sw - S(14) - wsz.Width, S(28));
            Art.Coin(g, sx + sw - S(14) - wsz.Width - S(20), S(30), S(17));
            Art.Text(g, Career.Title(gs.Prog), fHudBold, Color.White, new RectangleF(sx + S(14), S(54), sw - S(28), S(24)), StringAlignment.Near, StringAlignment.Center);
            int txp = Career.TotalXp(gs.Prog), sen = Career.Seniority(txp), nxt = Career.NextXp(txp), cur0 = Career.SenXp[sen];
            float xb = sw - S(28), frac = nxt > cur0 ? (float)(txp - cur0) / (nxt - cur0) : 1f;
            Art.RR(g, Art.Hx(0x30363D), sx + S(14), S(84), xb, S(10), S(5));
            Art.RR(g, Art.Hx(0x8E6CC4), sx + S(14), S(84), Math.Max(S(10), xb * frac), S(10), S(5));
            string xpTxt = txp + " XP" + (sen + 1 < Career.SenName.Length ? "  ·  próximo: " + Career.SenName[sen + 1] + " (" + nxt + ")" : "  ·  topo da carreira");
            g.DrawString(xpTxt, fSmall, Art.B(Art.Hx(0xC9D1D9)), sx + S(14), S(97));
            int rep = gs.Prog.Reputation;
            Color rc = rep >= 65 ? Art.Hx(0x3FB950) : rep >= 40 ? Art.Hx(0xF0B429) : Art.Hx(0xF85149);
            g.DrawString("Reputação " + rep + " · " + Career.RepLabel(rep), fSmall, Art.B(rc), sx + S(14), S(117));
            // badges por categoria
            int nc = Challenges.Cats.Length;
            float bdw = (sw - S(28) - (nc - 1) * S(4)) / nc;
            for (int i = 0; i < Challenges.Cats.Length; i++)
            {
                var cat = Challenges.Cats[i];
                float bx = sx + S(14) + i * (bdw + S(4)), byy = S(140);
                Art.RR(g, Art.A(cat.Color, 60), bx, byy, bdw, S(30), S(5));
                for (int t = 0; t < 3; t++)
                {
                    bool has = gs.Prog.BadgeDates.ContainsKey(cat.Id + ":" + t);
                    Color mc = t == 0 ? Art.Hx(0xCD7F32) : t == 1 ? Art.Hx(0xC0C0C0) : Art.Hx(0xF5C542);
                    Art.E(g, has ? mc : Art.Hx(0x30363D), bx + S(3) + t * (bdw - S(6)) / 3f, byy + S(16), S(8), S(8));
                }
                Art.Text(g, cat.Short.Split(' ')[0].Substring(0, Math.Min(4, cat.Short.Split(' ')[0].Length)), fTag, Color.White, new RectangleF(bx, byy + S(1), bdw, S(14)));
            }
            g = g0;
            });

            // controles
            string ctl = isoMode ? "WASD andar · Shift correr · E interagir · rodinha zoom · 1–8 itens · P patinete\nTAB carreira · B badges · O objetivos · C visual · M som · F1 ajuda · F11 tela cheia"
                                 : "WASD andar · Shift correr · E interagir · 1–8 itens · P patinete\nTAB carreira · B badges · O objetivos · C visual · M som · F1 ajuda · F11 tela cheia";
            var csz = g.MeasureString(ctl, fSmall);
            CachedHud(g, "controles", ctl, S(14), vh - csz.Height - S(30), csz.Width + S(20), csz.Height + S(14), g2 =>
            {
                Card(g2, S(14), vh - csz.Height - S(30), csz.Width + S(20), csz.Height + S(14));
                g2.DrawString(ctl, fSmall, Art.B(Art.Hx(0x8B949E)), S(24), vh - csz.Height - S(23));
            });
            DrawNeeds(g, vw, vh, vh - csz.Height - S(30));

            // minimapa (maior com o item da lojinha)
            bool big = gs.Prog.Owned.Contains("mapa");
            var mbmp = big ? miniBig : mini;
            float mt = big ? miniTileBig : miniTile;
            float mw = mbmp.Width, mh = mbmp.Height, mx = vw - mw - S(24), my = vh - mh - S(24);
            Card(g, mx - S(10), my - S(10), mw + S(20), mh + S(20));
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(mbmp, mx, my, mw, mh);
            foreach (var cat in Challenges.Cats)
                foreach (var p in World.Doors[cat.Room].Tiles)
                {
                    var mp = Scene.Mini(p.X + 0.5f, p.Y + 0.5f, mt);
                    Art.R(g, cat.Color, mx + mp.X - mt / 2, my + mp.Y - mt / 2, mt, mt);
                }
            float ks = mt / Scene.T;
            int lwv = vw / pix, lhv = vh / pix;
            if (!isoMode) g.DrawRectangle(Art.P(Art.A(Color.White, 140), 1), mx + (camX - Scene.OX) * ks + 2, my + (camY - Scene.OY) * ks + 2, lwv * ks, lhv * ks);
            if (target.HasValue && (int)(time * 3) % 2 == 0)
            {
                var tp = Scene.Mini(target.Value.X, target.Value.Y, mt);
                Art.E(g, Art.Hx(0xF0B429), mx + tp.X - S(4), my + tp.Y - S(4), S(8), S(8));
            }
            var shop = World.Items.First(it => it.Kind == "shop");
            var sp = Scene.Mini(shop.X, shop.Y - 1, mt);
            Art.Coin(g, mx + sp.X - S(5), my + sp.Y - S(5), S(10));
            var me = Scene.Mini(px, py, mt);
            Art.E(g, Color.White, mx + me.X - S(4), my + me.Y - S(4), S(8), S(8));
            Art.E(g, Art.Docker, mx + me.X - S(2.5f), my + me.Y - S(2.5f), S(5), S(5));

            // seta na borda da tela quando o alvo está fora de vista
            if (target.HasValue && page == null)
            {
                var tsp = Scr(target.Value.X, target.Value.Y, 30);
                float tx = tsp.X, ty = tsp.Y;
                float m = S(70);
                if (tx < m || ty < m || tx > vw - m || ty > vh - m)
                {
                    float ccx = vw / 2f, ccy = vh / 2f, dx = tx - ccx, dy = ty - ccy;
                    float k = Math.Min(Math.Abs((vw / 2f - m) / (dx == 0 ? 0.001f : dx)), Math.Abs((vh / 2f - m) / (dy == 0 ? 0.001f : dy)));
                    float ex = ccx + dx * k, ey = ccy + dy * k;
                    float ang = (float)Math.Atan2(dy, dx);
                    var st = g.Save();
                    g.TranslateTransform(ex, ey);
                    g.RotateTransform(ang * 180f / (float)Math.PI);
                    float a = S(18) + (float)Math.Sin(time * 6) * S(3);
                    var tri = new[] { new PointF(a, 0), new PointF(-a * 0.7f, -a * 0.75f), new PointF(-a * 0.7f, a * 0.75f) };
                    g.FillPolygon(Art.B(Art.Hx(0x1A1D26)), tri.Select(p => new PointF(p.X + 2, p.Y + 3)).ToArray());
                    g.FillPolygon(Art.B(Art.Hx(0xF0B429)), tri);
                    g.DrawPolygon(Art.P(Art.Hx(0x1A1D26), Math.Max(1, S(1.5f))), tri);
                    g.Restore(st);
                }
            }

            // prompt de interação
            if (focus != null && page == null && !Paused && !showMissions && !showObjectives)
            {
                string txt = focus.Label;
                var sz = g.MeasureString(txt, fHudBold);
                float kw = S(26), w = sz.Width + kw + S(26), h = S(34);
                var hp = Scr(px, py, Scene.CharU * 1.6f + 10);
                float bx = hp.X - w / 2, by = hp.Y - h;
                Art.RR(g, Art.Hx(0x1A1D26), bx - S(2), by - S(2), w + S(4), h + S(4), S(8));
                Art.RR(g, Color.White, bx, by, w, h, S(7));
                Art.RR(g, Art.Docker, bx + S(6), by + S(6), kw, h - S(12), S(4));
                Art.Text(g, "E", fKey, Color.White, new RectangleF(bx + S(6), by + S(6), kw, h - S(12)));
                g.DrawString(txt, fHudBold, Art.B(Art.Hx(0x1A1D26)), bx + kw + S(14), by + (h - sz.Height) / 2);
            }
            // banner de setor
            if (bannerLife > 0 && banner != null)
            {
                int a = (int)(255 * Math.Min(1, Math.Min(bannerLife, 2.4f - bannerLife) * 3));
                var sz = g.MeasureString(banner, fBanner);
                float bx = (vw - sz.Width) / 2, by = Math.Max(S(240), vh * 0.3f);
                Art.RR(g, Art.A(Art.Hx(0x0D1117), a * 200 / 255), bx - S(24), by - S(8), sz.Width + S(48), sz.Height + S(16), S(12));
                g.DrawString(banner, fBanner, Art.B(Art.A(Color.White, a)), bx, by);
            }

            // toasts
            for (int i = 0; i < toasts.Count; i++)
            {
                var t = toasts[i];
                int a = (int)(255 * Math.Min(1, t.Life * 2));
                var sz = g.MeasureString(t.Text, fHudBold);
                float bx = (vw - sz.Width) / 2 - S(16), by = S(124) + i * S(44);
                Art.RR(g, Art.A(Art.Hx(0x0D1117), a * 230 / 255), bx, by, sz.Width + S(32), sz.Height + S(14), S(10));
                Art.RR(g, Art.A(t.Color, a), bx, by, S(5), sz.Height + S(14), S(2));
                g.DrawString(t.Text, fHudBold, Art.B(Art.A(t.Color, a)), bx + S(16), by + S(7));
            }

            if (showMissions) DrawMissions(g, vw, vh);
            if (showObjectives) DrawObjectives(g, vw, vh);
            if (page != null) DrawDialog(g, vw, vh);
        }

        void DrawObjectives(Graphics g, int vw, int vh)
        {
            var list = Catalog.Achievements;
            int rows = (list.Count + 1) / 2;
            float w = Math.Min(S(1060), vw - S(40)), rowH = S(62), h = S(112) + rows * rowH, x = (vw - w) / 2, y = Math.Max(S(10), (vh - h) / 2);
            Card(g, x, y, w, h);
            g.DrawString("OBJETIVOS", fTitle, Art.B(Art.Hx(0xF5C542)), x + S(24), y + S(14));
            int done = list.Count(a => gs.Prog.Achieved.Contains(a.Id));
            g.DrawString(done + " de " + list.Count + " concluídos  ·  cada objetivo dá pontos extras para gastar na lojinha", fHud, Art.B(Art.Hx(0x8B949E)), x + S(26), y + S(50));
            float cw = (w - S(48)) / 2;
            for (int i = 0; i < list.Count; i++)
            {
                var a = list[i];
                float cx = x + S(16) + (i % 2) * (cw + S(16)), cy = y + S(82) + (i / 2) * rowH;
                bool ok = gs.Prog.Achieved.Contains(a.Id);
                int goal = Catalog.GoalOf(a, World), prog = Math.Min(goal, a.Progress(gs, World));
                Color c = ok ? Art.Hx(0x3FB950) : Art.Hx(0x484F58);
                Art.RR(g, Art.A(c, ok ? 45 : 30), cx, cy, cw, rowH - S(8), S(8));
                Art.RR(g, ok ? Art.Hx(0xF5C542) : Art.Hx(0x30363D), cx + S(8), cy + S(9), S(36), S(36), S(18));
                Art.Text(g, ok ? "★" : "☆", fHudBold, ok ? Art.Hx(0x5C4410) : Art.Hx(0x8B949E), new RectangleF(cx + S(8), cy + S(9), S(36), S(36)));
                g.DrawString(a.Name, fHudBold, Art.B(ok ? Color.White : Art.Hx(0xC9D1D9)), cx + S(54), cy + S(4));
                g.DrawString(a.Desc, fSmall, Art.B(Art.Hx(0x8B949E)), new RectangleF(cx + S(54), cy + S(27), cw - S(150), S(26)));
                string right = ok ? "✔ +" + a.Reward : "+" + a.Reward;
                var rs = g.MeasureString(right, fHudBold);
                g.DrawString(right, fHudBold, Art.B(ok ? Art.Hx(0x3FB950) : Art.Hx(0xF5C542)), cx + cw - rs.Width - S(10), cy + S(4));
                if (!ok)
                {
                    float bw = S(80), bx = cx + cw - bw - S(10), byy = cy + S(34);
                    Art.RR(g, Art.Hx(0x30363D), bx, byy, bw, S(8), S(4));
                    Art.RR(g, Art.Docker, bx, byy, Math.Max(S(4), bw * prog / Math.Max(1, goal)), S(8), S(4));
                    string pt = prog.ToString("N0") + "/" + goal.ToString("N0");
                    var ps = g.MeasureString(pt, fSmall);
                    g.DrawString(pt, fSmall, Art.B(Art.Hx(0x8B949E)), bx - ps.Width - S(4), byy - S(5));
                }
            }
            g.DrawString("O fechar  ·  TAB carreira  ·  C visual", fSmall, Art.B(Art.Hx(0x8B949E)), x + S(24), y + h - S(26));
        }
        void DrawMissions(Graphics g, int vw, int vh)
        {
            float w = Math.Min(S(900), vw - S(40)), rowH = S(50), h = S(150) + Challenges.Cats.Length * rowH, x = (vw - w) / 2, y = Math.Max(S(10), (vh - h) / 2);
            Card(g, x, y, w, h);
            g.DrawString("CARREIRA", fTitle, Art.B(Art.Docker), x + S(24), y + S(14));
            g.DrawString(Career.Title(gs.Prog), fHudBold, Art.B(Color.White), x + S(170), y + S(20));
            int txp = Career.TotalXp(gs.Prog);
            int soft; gs.Prog.Xp.TryGetValue("soft", out soft);
            g.DrawString(txp + " XP total · " + soft + " XP de habilidades interpessoais · reputação " + gs.Prog.Reputation + " (" + Career.RepLabel(gs.Prog.Reputation) + ") · " +
                         gs.BadgeCount + "/" + Challenges.Cats.Length * 3 + " badges · " + gs.Prog.GoodChoices + " boas escolhas", fSmall, Art.B(Art.Hx(0x8B949E)), x + S(26), y + S(54));
            float colX = x + S(300), colW = (w - S(320)) / 3f;
            for (int t = 0; t < 3; t++)
                g.DrawString(Challenges.TierNames[t].ToUpperInvariant(), fSmall, Art.B(Art.Hx(0x8B949E)), colX + t * colW + S(6), y + S(84));
            for (int i = 0; i < Challenges.Cats.Length; i++)
            {
                var cat = Challenges.Cats[i];
                float ry = y + S(104) + i * rowH;
                int cx; gs.Prog.Xp.TryGetValue(cat.Id, out cx);
                Art.RR(g, Art.A(cat.Color, 35), x + S(16), ry, w - S(32), rowH - S(6), S(8));
                Art.RR(g, cat.Color, x + S(16), ry, S(6), rowH - S(6), S(3));
                g.DrawString(cat.Name, fHudBold, Art.B(Color.White), x + S(32), ry + S(3));
                g.DrawString(Story.Sectors[cat.Room].Name + " · " + cx + " XP", fSmall, Art.B(Art.Hx(0x8B949E)), x + S(32), ry + S(24));
                for (int t = 0; t < 3; t++)
                {
                    int done = gs.TierCount(cat.Id, t), tot = gs.TierLevels(cat.Id, t).Count;
                    bool un = gs.TierUnlocked(cat.Id, t), badge = gs.Prog.BadgeDates.ContainsKey(cat.Id + ":" + t);
                    float bx = colX + t * colW + S(6), bw2 = colW - S(16);
                    Art.RR(g, Art.Hx(0x30363D), bx, ry + S(18), bw2, S(10), S(5));
                    if (tot > 0) Art.RR(g, badge ? Art.Hx(0xF5C542) : cat.Color, bx, ry + S(18), Math.Max(S(6), bw2 * done / tot), S(10), S(5));
                    string lab = !un ? "bloqueado" : badge ? "★ badge" : done + "/" + tot;
                    g.DrawString(lab, fSmall, Art.B(badge ? Art.Hx(0xF5C542) : un ? Art.Hx(0xC9D1D9) : Art.Hx(0x6E7681)), bx, ry + S(1));
                }
            }
            g.DrawString("TAB fechar  ·  B badges  ·  O objetivos  ·  C visual  ·  Ctrl+R reiniciar progresso  ·  F1 ajuda", fSmall, Art.B(Art.Hx(0x8B949E)), x + S(24), y + h - S(28));
        }
        void DrawDialog(Graphics g, int vw, int vh)
        {
            int nopt = page.Options != null ? page.Options.Count : 0;
            float w = Math.Min(S(1000), vw - S(40)), h0 = S(168), h = h0 + nopt * S(34), x = (vw - w) / 2, y = vh - h - S(24);
            Card(g, x, y, w, h);
            Art.RRO(g, nopt > 0 ? Art.Hx(0x8E6CC4) : Art.Docker, Math.Max(1, S(2)), x, y, w, h, S(10));
            var pbox = new RectangleF(x + S(16), y + S(16), h0 - S(32), h0 - S(32));
            if (nopt > 0 && typed >= page.Text.Length)
            {
                float ox = pbox.Right + S(20), oy = y + h0 - S(52);
                for (int i = 0; i < nopt; i++)
                {
                    float ry = oy + i * S(34);
                    Art.RR(g, Art.Hx(0x21262D), ox, ry, w - (ox - x) - S(20), S(30), S(6));
                    Art.RR(g, Art.Hx(0x8E6CC4), ox + S(4), ry + S(4), S(26), S(22), S(4));
                    Art.Text(g, (i + 1).ToString(), fKey, Color.White, new RectangleF(ox + S(4), ry + S(4), S(26), S(22)));
                    g.DrawString(page.Options[i].Key, fHudBold, Art.B(Color.White), ox + S(40), ry + S(5));
                }
            }
            if (page.Npc != null)
            {
                using (var path = Art.RoundPath(pbox.X, pbox.Y, pbox.Width, pbox.Height, S(10)))
                {
                    var st = g.Save();
                    g.SetClip(path);
                    Art.Portrait(g, pbox, page.Npc.Look, time);
                    g.Restore(st);
                }
            }
            else
            {
                Art.RR(g, Art.Hx(0x1F2937), pbox.X, pbox.Y, pbox.Width, pbox.Height, S(10));
                Art.Icon(g, page.Icon ?? "board", new RectangleF(pbox.X + pbox.Width * 0.15f, pbox.Y + pbox.Height * 0.15f, pbox.Width * 0.7f, pbox.Height * 0.7f));
            }
            float tx = pbox.Right + S(20), tw = x + w - tx - S(20);
            string name = page.Npc != null ? page.Npc.Name : page.Title;
            g.DrawString(name, fName, Art.B(Art.Hx(0xF0B429)), tx, y + S(14));
            if (page.Npc != null)
            {
                var ns = g.MeasureString(name, fName);
                g.DrawString(page.Npc.Role, fHud, Art.B(Art.Hx(0x8B949E)), tx + ns.Width + S(4), y + S(18));
            }
            string shown = page.Text.Substring(0, Math.Min(page.Text.Length, (int)typed));
            g.DrawString(shown, fDialog, Art.B(Art.Hx(0xE6EDF3)), new RectangleF(tx, y + S(46), tw, h0 - S(70)));

            bool full = typed >= page.Text.Length;
            string hint = nopt > 0 ? "Escolha com as teclas 1 a " + nopt : page.Choice ? "[E] Sim     [Esc] Não" : full ? (pages.Count(p => p.Run == null) > 0 ? "[E] continuar ▼" : "[E] fechar") : "[E] pular";
            var hs = g.MeasureString(hint, fHudBold);
            Color hc = page.Choice ? Art.Hx(0xF0B429) : Art.Hx(0x58A6FF);
            if (!page.Choice && full && (int)(time * 2.5f) % 2 == 1) hc = Art.A(hc, 120);
            g.DrawString(hint, fHudBold, Art.B(hc), x + w - hs.Width - S(20), y + h - hs.Height - S(12));
        }
    }
}
