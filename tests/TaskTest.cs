#if DEMO
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DockerQuest
{
    // Valida TODAS as tarefas pela interface: digita a resposta tecla por tecla no campo do terminal / editor da IDE /
    // terminal mysql / CloudShell, envia com ENTER (ou F5 na IDE) e confere se a tarefa foi concluída.
    // Também confere, em vários tamanhos de janela, se o campo e o botão de enviar estão visíveis.
    static class TaskTest
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr SendMessage(IntPtr h, int msg, IntPtr w, IntPtr l);
        const int WM_KEYDOWN = 0x100, WM_KEYUP = 0x101, WM_CHAR = 0x102;

        static void Key(Control c, Keys k)
        {
            SendMessage(c.Handle, WM_KEYDOWN, (IntPtr)(int)k, (IntPtr)1);
            SendMessage(c.Handle, WM_KEYUP, (IntPtr)(int)k, unchecked((IntPtr)(int)0xC0000001));
        }
        // F11 passa pelo laço de mensagens (PostMessage) para chegar ao ProcessCmdKey da janela, como uma tecla de verdade
        [DllImport("user32.dll")] static extern bool PostMessage(IntPtr h, int msg, IntPtr w, IntPtr l);
        static void SendF11(Control c)
        {
            PostMessage(c.Handle, WM_KEYDOWN, (IntPtr)(int)Keys.F11, (IntPtr)1);
            PostMessage(c.Handle, WM_KEYUP, (IntPtr)(int)Keys.F11, unchecked((IntPtr)(int)0xC0000001));
            Pump(3);
        }
        static void Char(Control c, char ch) { SendMessage(c.Handle, WM_CHAR, (IntPtr)ch, (IntPtr)1); }
        static void Back(Control c)
        {
            SendMessage(c.Handle, WM_KEYDOWN, (IntPtr)(int)Keys.Back, (IntPtr)1);
            SendMessage(c.Handle, WM_CHAR, (IntPtr)8, (IntPtr)1);
            SendMessage(c.Handle, WM_KEYUP, (IntPtr)(int)Keys.Back, unchecked((IntPtr)(int)0xC0000001));
        }
        static void Pump(int n) { for (int i = 0; i < n; i++) Application.DoEvents(); }

        static int CurIndent(RichTextBox r)
        {
            string t = r.Text; int caret = r.SelectionStart;
            int ls = caret == 0 ? 0 : t.LastIndexOf('\n', caret - 1) + 1;
            int n = 0; while (ls + n < caret && t[ls + n] == ' ') n++;
            return n;
        }

        // digita como uma pessoa: na IDE, o ENTER já indenta; ela apaga ou completa a indentação de cada linha
        static void Type(RichTextBox c, string s, bool editor)
        {
            s = s.Replace("\r\n", "\n").Replace("\t", "    ");
            var lines = s.Split('\n');
            for (int li = 0; li < lines.Length; li++)
            {
                string ln = lines[li];
                if (li > 0)
                {
                    Key(c, Keys.Return);
                    int want = ln.Length - ln.TrimStart(' ').Length;
                    int have = CurIndent(c);
                    while (have > want) { Back(c); have--; }
                    ln = ln.Substring(have);
                }
                foreach (char ch in ln) Char(c, ch);
            }
        }

        static string Norm(string s) { return (s ?? "").Replace("\r\n", "\n").Replace("\r", "\n").Replace("\t", "    ").Trim(); }

        public static void Run(Form f, GameView v, TerminalPanel t, GameState gs, string dir, Action<string> shot)
        {
            Action<string> log = s => File.AppendAllText(Path.Combine(dir, "tasks.txt"), s + Environment.NewLine);
            if (Environment.GetEnvironmentVariable("DQ_FS") == "1")
            {
                // tela cheia (F11) no escritório e no terminal, e a volta para a janela
                var mf = (MainForm)f;
                var before = f.Bounds;
                Action<int> wait = ms => { var sw = System.Diagnostics.Stopwatch.StartNew(); while (sw.ElapsedMilliseconds < ms) { Application.DoEvents(); System.Threading.Thread.Sleep(10); } };
                f.Activate(); v.Focus(); wait(3000);
                SendF11(v);   // pela tecla, como o jogador wait(1500);
                var scr = Screen.FromControl(f).Bounds;
                log("tela cheia: " + mf.DebugFullScreen + " janela=" + f.Bounds + " tela=" + scr + " borda=" + f.FormBorderStyle + " jogo=" + v.ClientSize);
                var gl = v.DebugGlShot(); if (gl != null) { gl.Save(Path.Combine(dir, "telacheia_jogo.png")); log("captura do jogo em tela cheia: " + gl.Size); gl.Dispose(); }
                t.Open(Challenges.Cat("docker").Room); wait(500);
                log("terminal em tela cheia: " + t.Bounds + " campo=" + (Vis(f, t, t.DebugEntry) ?? "ok") + " botão=" + (Vis(f, t, t.DebugSend) ?? "ok"));
                shot("telacheia_terminal");
                SendF11(t.DebugEntry); wait(800);
                log("voltou: " + !mf.DebugFullScreen + " janela=" + f.Bounds + " (antes " + before + ") borda=" + f.FormBorderStyle + " terminal=" + t.Bounds);
                t.DebugClose(); wait(300);
            }
            v.FreezeNeeds = true; v.Paused = true; v.Visible = false; v.DebugStopLoop();
            gs.IncCooldown = 99999; gs.Incident = null;
            gs.Prog.Erasers = 0;
            f.MinimumSize = Size.Empty;
            // badges e promoções abrem janelas modais: fecha sozinho para o teste seguir
            var closer = new Timer { Interval = 300 };
            closer.Tick += (s, e) => { foreach (Form o in Application.OpenForms.Cast<Form>().ToList()) if (o != f && o.Modal) { log("(janela fechada: " + o.Text + ")"); o.Close(); } };
            closer.Start();
            int total = 0, okCount = 0, falhas = 0, tested = 0;
            var problemas = new List<string>();

            // ── 1) layout: campo e botão de enviar visíveis em vários tamanhos de janela
            var sizes = new[] { new Size(1440, 900), new Size(1280, 720), new Size(1093, 582), new Size(1049, 559), new Size(983, 522), new Size(900, 500) };
            var modes = new List<int>();
            foreach (var cat in Challenges.Cats)
            {
                foreach (var kind in gs.LevelList.Where(l => l.Cat == cat.Id).Select(l => l.Kind).Distinct())
                    modes.Add(Enumerable.Range(0, gs.LevelList.Count).First(i => gs.LevelList[i].Cat == cat.Id && gs.LevelList[i].Kind == kind));
            }
            string onlyCat = Environment.GetEnvironmentVariable("DQ_CAT");
            if (Environment.GetEnvironmentVariable("DQ_NOLAYOUT") == "1") sizes = new Size[0];
            foreach (var sz in sizes)
            {
                f.WindowState = FormWindowState.Normal;
                f.ClientSize = sz; Pump(3);
                foreach (int i in modes)
                {
                    var lv = gs.LevelList[i];
                    t.Open(Challenges.Cat(lv.Cat).Room); t.DebugLoad(i); Pump(3);
                    string a = Vis(f, t, t.DebugEntry), b = Vis(f, t, t.DebugSend), m = Vis(f, t, t.DebugMission);
                    string tag = sz.Width + "x" + sz.Height + " " + lv.Cat + "/" + lv.Kind;
                    if (a != null || b != null || m != null)
                    {
                        string msg = "LAYOUT " + tag + ": campo=" + (a ?? "ok") + " botão=" + (b ?? "ok") + " missão=" + (m ?? "ok");
                        log(msg); problemas.Add(msg);
                        shot("layout_" + sz.Width + "_" + lv.Cat + "_" + lv.Kind);
                    }
                    else log("layout ok " + tag);
                    if (sz.Width == 1049 && Environment.GetEnvironmentVariable("DQ_SHOTS") == "1")
                    {
                        if (lv.Cat == "aws") { t.DebugAws("ec2", null); Pump(3); }
                        shot("pequena_" + lv.Cat + "_" + lv.Kind);
                    }
                    t.DebugClose(); Pump(2);
                }
            }
            f.ClientSize = new Size(1440, 900); Pump(3);

            // ── 2) todas as tarefas, todas as respostas aceitas, digitadas e enviadas pela interface
            string lastCat = null;
            for (int i = 0; i < gs.LevelList.Count; i++)
            {
                var lv = gs.LevelList[i];
                if (!string.IsNullOrEmpty(onlyCat) && !onlyCat.Split(',').Contains(lv.Cat)) continue;
                total++;
                File.WriteAllText(Path.Combine(dir, "progresso.txt"), i + " " + lv.Cat + " '" + lv.Title + "'");
                if (lv.Cat != lastCat) { if (lastCat != null) t.DebugClose(); t.Open(Challenges.Cat(lv.Cat).Room); lastCat = lv.Cat; Pump(2); }
                var answers = new List<string[]>();
                if (lv.Kind == "aws")
                {
                    if (lv.Answers.Length > 1 && lv.Answers[1].StartsWith("aws "))
                        answers.Add(lv.Answers[1].Split(new[] { "  +  " }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray());
                    else { log("AWS só pelo console (sem CLI): '" + lv.Title + "'"); okCount++; continue; }
                }
                else
                {
                    // respostas geradas por combinação podem ser centenas: digita as 4 primeiras e mais 2 do meio/fim
                    var idx = new SortedSet<int>(Enumerable.Range(0, Math.Min(4, lv.Answers.Length)));
                    idx.Add(lv.Answers.Length / 2); idx.Add(lv.Answers.Length - 1);
                    foreach (int k in idx) answers.Add(new[] { lv.Answers[k] });
                }
                bool allOk = true;
                foreach (var steps in answers)
                {
                    tested++;
                    t.DebugLoad(i); Pump(2);
                    if (t.DebugSolved) { log("JÁ RESOLVIDA ao carregar?! " + lv.Title); }
                    string why = null;
                    foreach (var step in steps)
                    {
                        var entry = t.DebugEntry;
                        bool code = t.DebugCode;
                        entry.Focus();
                        Type(entry, step, code);
                        Pump(1);
                        if (entry.IsDisposed)
                        {
                            string chain = "";
                            for (Control pc = entry; pc != null; pc = pc.Parent) chain += pc.GetType().Name + (pc.IsDisposed ? "(X)" : "") + " < ";
                            log("CAMPO DESCARTADO em '" + lv.Title + "': " + chain + " | atual descartado? " + t.DebugEntry.IsDisposed + " mesmo objeto? " + (entry == t.DebugEntry) + " terminal descartado? " + t.IsDisposed + " form? " + f.IsDisposed);
                            problemas.Add("campo descartado");
                            entry = t.DebugEntry;
                        }
                        string typed = Norm(entry.Text), want = Norm(step);
                        if (typed != want) why = "texto digitado ficou diferente: [" + typed.Replace("\n", "⏎") + "] esperado [" + want.Replace("\n", "⏎") + "]";
                        Key(entry, code ? Keys.F5 : Keys.Return);
                        Pump(4);
                        if (t.DebugSolved) break;
                    }
                    if (!t.DebugSolved)
                    {
                        allOk = false;
                        string msg = "FALHA " + lv.Cat + " t" + lv.Tier + " [" + lv.Kind + (t.DebugCode ? "/IDE" : "") + "] '" + lv.Title + "' resposta: " + string.Join(" + ", steps).Replace("\n", "⏎") +
                                     "\n    " + (why ?? "") + "\n    status: " + t.DebugStatus + "\n    saída: " + t.DebugOutTail.Replace("\n", " | ");
                        log(msg); problemas.Add(msg);
                        shot("falha_" + i);
                    }
                }
                if (allOk) okCount++; else falhas++;
            }
            t.DebugClose();

            // ── 3) tarefas AWS que só se fazem pelo console: cliques e digitação nos controles do console
            if (string.IsNullOrEmpty(onlyCat) || onlyCat.Split(',').Contains("aws"))
            {
                Func<string, int> idxOf = title => Enumerable.Range(0, gs.LevelList.Count).First(k => gs.LevelList[k].Cat == "aws" && gs.LevelList[k].Title == title);
                Action<string, Action<Control>> console = (title, steps) =>
                {
                    int k = idxOf(title);
                    t.Open(Challenges.Cat("aws").Room); t.DebugLoad(k); Pump(3);
                    var aws = t.DebugAwsView;
                    string err = null;
                    try { steps(aws); } catch (Exception ex) { err = ex.Message; }
                    Pump(4);
                    total++; tested++;
                    if (t.DebugSolved) { okCount++; log("console ok: '" + title + "'"); }
                    else
                    {
                        falhas++;
                        string msg = "FALHA console AWS '" + title + "': " + (err ?? "") + " status: " + t.DebugStatus;
                        log(msg); problemas.Add(msg); shot("falha_console_" + k);
                    }
                    t.DebugClose(); Pump(2);
                };
                console("Novo servidor", aws =>
                {
                    var search = ((AwsConsoleView)aws).DebugSearch;
                    if (search.Visible) { search.Focus(); foreach (char ch in "EC2") Char(search, ch); Key(search, Keys.Return); Pump(3); }
                    else t.DebugAws("ec2", null);
                    Click(Need<Button>(aws, b => b.Text == "Executar instâncias" && b.Visible, "botão Executar instâncias")); Pump(3);
                    var name = Need<TextBox>(aws, x => x != search && x.Visible && !x.Multiline, "campo Nome");
                    name.Focus(); foreach (char ch in "api-cafe") Char(name, ch);
                    var type = Need<ComboBox>(aws, c => c.Visible && c.Items.Count > 2 && c.Items[0].ToString().StartsWith("t"), "lista de tipos");
                    for (int j = 0; j < type.Items.Count; j++) if (type.Items[j].ToString().StartsWith("t3.micro")) type.SelectedIndex = j;
                    Click(Need<Button>(aws, b => b.Text == "Executar instância" && b.Visible, "botão Executar instância"));
                });
                console("MFA para todos", aws =>
                {
                    t.DebugAws("iam-user", "rodrigo"); Pump(3);
                    Click(Need<Button>(aws, b => b.Text == "Atribuir dispositivo MFA" && b.Visible, "botão Atribuir MFA")); Pump(3);
                    var boxes = All<TextBox>(aws).Where(x => x != ((AwsConsoleView)aws).DebugSearch && x.Visible && !x.Multiline).ToList();
                    var codes = boxes.Skip(Math.Max(0, boxes.Count - 2)).ToList();
                    if (codes.Count < 2) throw new Exception("campos de código MFA não encontrados");
                    codes[0].Focus(); foreach (char ch in "482913") Char(codes[0], ch);
                    codes[1].Focus(); foreach (char ch in "105662") Char(codes[1], ch);
                    Click(Need<Button>(aws, b => b.Text == "Adicionar MFA" && b.Visible, "botão Adicionar MFA"));
                });
            }
            // ── 4) desafios relâmpago: os do Docker e um sorteado de cada área (caminho de envio próprio do terminal)
            if (string.IsNullOrEmpty(onlyCat) || onlyCat.Split(',').Contains("relampago"))
            {
                var incs = new List<Incident>(Incidents.All);
                foreach (var cat in Challenges.Cats)
                    foreach (var li in gs.CatLevels(cat.Id).Where(k => gs.LevelList[k].Kind != "aws" || (gs.LevelList[k].Answers.Length > 1 && gs.LevelList[k].Answers[1].StartsWith("aws "))).Take(3))
                    {
                        var lv = gs.LevelList[li];
                        incs.Add(new Incident { Id = "flash_" + li, Cat = cat.Id, Room = cat.Room, Tier = lv.Tier, Level = lv, LevelIndex = li, Reporter = "Teste", Title = lv.Title, Alert = "URGENTE! " + lv.Context });
                    }
                foreach (var inc in incs)
                {
                    total++; tested++;
                    gs.Incident = inc; gs.IncTotal = gs.IncTime = 120; gs.IncBonus = 0; gs.IncWrong = 0;
                    t.Open(inc.Room); Pump(3);
                    string ans = inc.Level.Kind == "aws" ? inc.Level.Answers[1] : inc.Level.Answers[0];
                    var entry = t.DebugEntry; bool code = t.DebugCode;
                    entry.Focus(); Type(entry, ans, code); Pump(1);
                    Key(entry, code ? Keys.F5 : Keys.Return); Pump(4);
                    if (t.DebugSolved) okCount++;
                    else
                    {
                        falhas++;
                        string msg = "FALHA relâmpago " + inc.Cat + " '" + inc.Title + "': " + ans.Replace("\n", "⏎") + "\n    status: " + t.DebugStatus + "\n    saída: " + t.DebugOutTail.Replace("\n", " | ");
                        log(msg); problemas.Add(msg); shot("falha_relampago_" + inc.Id);
                    }
                    gs.Incident = null;
                    t.DebugClose(); Pump(2);
                }
                log("relâmpagos testados: " + incs.Count);
            }
            log("");
            log("RESUMO: " + total + " tarefas, " + tested + " respostas enviadas pela interface, " + okCount + " ok, " + falhas + " com falha, " + problemas.Count + " problemas no total");
            gs.Prog.Save();
            Application.Exit();
        }

        const int WM_LBUTTONDOWN = 0x201, WM_LBUTTONUP = 0x202;
        static void Click(Control c)
        {
            var lp = (IntPtr)(((c.Height / 2) << 16) | (c.Width / 2));
            SendMessage(c.Handle, WM_LBUTTONDOWN, (IntPtr)1, lp);
            SendMessage(c.Handle, WM_LBUTTONUP, IntPtr.Zero, lp);
            Pump(2);
        }
        static IEnumerable<T> All<T>(Control root) where T : Control
        {
            foreach (Control c in root.Controls)
            {
                if (c is T) yield return (T)c;
                foreach (var d in All<T>(c)) yield return d;
            }
        }
        static T Find<T>(Control root, Func<T, bool> pred) where T : Control { return All<T>(root).FirstOrDefault(pred); }
        static T Need<T>(Control root, Func<T, bool> pred, string what) where T : Control
        {
            var c = Find(root, pred);
            if (c == null) throw new Exception(what + " não encontrado");
            return c;
        }

        // o controle está visível de verdade (sem cortes e sem nada por cima)?
        static string Vis(Form f, TerminalPanel t, Control c)
        {
            if (!c.Visible) return "invisível";
            var r = c.RectangleToScreen(c.ClientRectangle);
            if (r.Width < 40 || r.Height < 14) return "pequeno demais " + r.Size;
            var vr = r;
            for (var p = c.Parent; p != null; p = p.Parent) vr.Intersect(p.RectangleToScreen(p.ClientRectangle));
            if (vr.Width < r.Width * 0.9 || vr.Height < r.Height * 0.9) return "cortado (aparece " + vr.Width + "x" + vr.Height + " de " + r.Width + "x" + r.Height + ")";
            var pt = new Point(vr.X + vr.Width / 2, vr.Y + vr.Height / 2);
            Control hit = f;
            while (true)
            {
                var ch = hit.GetChildAtPoint(hit.PointToClient(pt), GetChildAtPointSkip.Invisible);
                if (ch == null) break;
                hit = ch;
            }
            for (var h = hit; h != null; h = h.Parent) if (h == c) return null;
            return "coberto por " + hit.GetType().Name + " '" + hit.Name + "'";
        }
    }
}
#endif
