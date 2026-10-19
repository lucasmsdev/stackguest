#if DEMO
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DockerQuest
{
    // Roteiro automático usado só no build de teste: joga algumas cenas e salva prints em DOCKERQUEST_DEMO.
    static class Demo
    {
        [DllImport("user32.dll")]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);

        static string Dir { get { return Environment.GetEnvironmentVariable("DOCKERQUEST_DEMO"); } }
        public static bool Active { get { return !string.IsNullOrEmpty(Dir); } }
        public static bool Perf { get { return Environment.GetEnvironmentVariable("DQ_PERF") == "1"; } }

        static T Off<T>(T f) where T : Form { f.StartPosition = FormStartPosition.Manual; f.Location = new Point(-5000, 40); f.Show(); return f; }

        public static void Run(Form f, GameView v, TerminalPanel t, GameState gs)
        {
            Form target = f;
            if (!Perf) Audio.Master = 0f;   // capturas de tela em silêncio
            float scale; using (var gg = f.CreateGraphics()) scale = gg.DpiX / 96f;
            Action<string> shot = name =>
            {
                using (var bmp = new Bitmap(target.Width, target.Height))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        var hdc = g.GetHdc();
                        PrintWindow(target.Handle, hdc, 2);
                        g.ReleaseHdc(hdc);
                    }
                    bmp.Save(Path.Combine(Dir, name + ".png"));
                }
            };
            Action<string> log = s => File.AppendAllText(Path.Combine(Dir, "log.txt"), s + Environment.NewLine);
            Func<string, int, int> first = (cat, tier) => gs.TierLevels(cat, tier)[0];
            if (Environment.GetEnvironmentVariable("DQ_TOUR") == "1")
            {
                // passeio por todas as salas (prints do OpenGL) para caçar defeitos de desenho
                var tourSteps = new List<Action> { () => { v.DebugFinishDialog(); v.ForceActive = true; v.FreezeNeeds = true; gs.IncCooldown = 99999; f.ClientSize = new Size(1600, 900); } };
                // DQ_ROOMS: salas (padrão: todas); DQ_SPOTS: centro, dentro e fora da porta; DQ_ZOOMS: níveis de zoom
                string roomsEnv = Environment.GetEnvironmentVariable("DQ_ROOMS"), spotsEnv = Environment.GetEnvironmentVariable("DQ_SPOTS") ?? "c", zoomsEnv = Environment.GetEnvironmentVariable("DQ_ZOOMS") ?? "0,1";
                var roomList = string.IsNullOrEmpty(roomsEnv) ? Enumerable.Range(0, Story.RoomCount).ToList() : roomsEnv.Split(',').Select(int.Parse).ToList();
                foreach (int r in roomList)
                {
                    int rr = r;
                    var rc = v.World.Rooms[r];
                    if (rc.Width == 0) continue;
                    var door = v.World.Doors[r];
                    foreach (var spot in spotsEnv.Split(','))
                    {
                        PointF p;
                        if (spot == "i" && door != null) p = door.Inside;
                        else if (spot == "o" && door != null) p = door.Outside;
                        else if (spot == "c") p = new PointF(rc.X + rc.Width / 2f, rc.Y + rc.Height / 2f + 0.5f);
                        else if (spot == "e") p = new PointF(rc.X + 3.5f, rc.Y + rc.Height / 2f + 0.5f);
                        else if (spot == "w") p = new PointF(rc.X + 0.6f, rc.Y + rc.Height / 2f + 0.5f);
                        else if (spot == "sw") p = new PointF(rc.X + 0.6f, rc.Bottom - 0.6f);
                        else continue;
                        string sp = spot;
                        foreach (var zs in zoomsEnv.Split(','))
                        {
                            int zz = int.Parse(zs);
                            var pp = p;
                            tourSteps.Add(() => { v.Teleport(pp.X, pp.Y, 0); v.DebugIsoZoom(zz); });
                            tourSteps.Add(() => { });
                            tourSteps.Add(() =>
                            {
                                var b = v.DebugGlShot();
                                if (b != null) { b.Save(Path.Combine(Dir, "tour_" + rr.ToString("00") + "_" + sp + "_z" + zz + ".png")); b.Dispose(); }
                                if (Environment.GetEnvironmentVariable("DQ_PROBE") == "1")
                                    foreach (var q in new[] { new Point(rc.X - 1, rc.Y - 1), new Point(rc.Right, rc.Y - 1), new Point(rc.X - 1, rc.Bottom), new Point(rc.Right, rc.Bottom) })
                                    {
                                        var sc = v.DebugProject(q.X + 0.5f, q.Y + 0.5f, 2.3f);
                                        log("sala " + rr + " " + sp + " parede (" + q.X + "," + q.Y + ") topo na tela " + sc.X.ToString("0") + "," + sc.Y.ToString("0"));
                                    }
                                if (Environment.GetEnvironmentVariable("DQ_PROBE") == "1")
                                    for (int qx = rc.X - 2; qx <= rc.Right + 1; qx++)
                                        for (int qy = rc.Y - 2; qy <= rc.Bottom + 1; qy++)
                                        {
                                            if (qx > rc.X && qx < rc.Right - 1 && qy > rc.Y && qy < rc.Bottom - 1) continue;
                                            var sc2 = v.DebugProject(qx + 0.5f, qy + 0.5f, 2.3f);
                                            log("  " + v.DebugWall(qx, qy) + "  tela " + sc2.X.ToString("0") + "," + sc2.Y.ToString("0"));
                                        }
                            });
                        }
                    }
                }
                tourSteps.Add(() => Application.Exit());
                int sk = 0;
                var st = new Timer { Interval = 500 };
                st.Tick += (s, e) => { if (sk < tourSteps.Count) { try { tourSteps[sk++](); } catch (Exception ex) { log("ERRO passeio " + (sk - 1) + ": " + ex); } } };
                st.Start();
                return;
            }
            if (Environment.GetEnvironmentVariable("DQ_TASKS") == "1")
            {
                var tt = new Timer { Interval = 1500 };
                tt.Tick += (s, e) =>
                {
                    tt.Stop();
                    try { TaskTest.Run(f, v, t, gs, Dir, shot); }
                    catch (Exception ex) { log("ERRO tarefas: " + ex); Application.Exit(); }
                };
                tt.Start();
                return;
            }
            if (Environment.GetEnvironmentVariable("DQ_ROOM") == "1")
            {
                // roteiro da sala do jogador, fome/sede, refeitório pago e patinete
                Action<string> rshot = name =>
                {
                    var b = v.DebugGlShot();
                    if (b != null) { b.Save(Path.Combine(Dir, name + ".png")); b.Dispose(); }
                    else shot(name);
                };
                var mf = (MainForm)f;
                Action<string> own = id => gs.Prog.Owned.Add(id);
                Func<string> needs = () => "fome " + v.Hunger.ToString("0") + "% · sede " + v.Thirst.ToString("0") + "% · moedas " + gs.Prog.Wallet;
                PointF sp0 = PointF.Empty; int spT = 0;
                ShopForm shop = null;
                var rs = new List<Action>
                {
                    () => { v.DebugFinishDialog(); v.ForceActive = true; v.FreezeNeeds = true; v.DebugNeeds(100, 100); gs.IncCooldown = 99999; },
                    () => { log("isométrico: " + v.IsoMode + " · " + needs()); v.Teleport(38f, 50.5f, 3); },
                    () => rshot("sala01_vazia"),
                    () => { own("sala_mesa1"); own("sala_pc1"); own("sala_cad1"); v.RefreshRoom(); },
                    () => rshot("sala02_basica"),
                    () =>
                    {
                        foreach (var it in PlayerRoom.Items) own(it.Id);
                        gs.Prog.Owned.Remove("sala_piso4"); gs.Prog.Owned.Remove("sala_piso3");
                        v.RefreshRoom();
                        log("móveis na sala: " + PlayerRoom.Placed(gs.Prog).Count + " · XP bônus " + PlayerRoom.XpBonus(gs.Prog) + " · fome/sede x" + PlayerRoom.NeedsDrain(gs.Prog));
                    },
                    () => { }, () => { },
                    () => rshot("sala03_completa"),
                    () => { v.DebugIsoZoom(1); v.Teleport(41.5f, 49.9f, 3); },
                    () => rshot("sala04_computador_zoom"),
                    () => { v.DebugIsoZoom(0); v.DebugAct(); },
                    () => rshot("sala05_computador_menu"),
                    () => { v.DebugCloseDialog(); log("menu do computador fechado; pausado? " + v.DebugPaused); v.DebugNeeds(60, 40); v.Teleport(44.4f, 51.7f, 3); v.DebugAct(); },
                    () => { rshot("sala06_frigobar"); var w0 = gs.Prog.Wallet; v.DebugKey(Keys.D1, true); v.DebugKey(Keys.D1, false); v.DebugKey(Keys.D1, true); v.DebugKey(Keys.D1, false); log("frigobar água: " + needs() + " (gastou " + (w0 - gs.Prog.Wallet) + ")"); },
                    () => { v.DebugCloseDialog(); v.Teleport(25f, 32.6f, 3); log("foco no refeitório: " + v.DebugFocus); v.DebugAct(); },
                    () => { rshot("sala07_refeitorio_cardapio"); var w0 = gs.Prog.Wallet; v.DebugKey(Keys.D1, true); v.DebugKey(Keys.D1, false); v.DebugKey(Keys.D1, true); v.DebugKey(Keys.D1, false); log("prato feito: " + needs() + " (gastou " + (w0 - gs.Prog.Wallet) + ")"); },
                    () => { v.DebugCloseDialog(); gs.Prog.Inv["pao_queijo"] = 2; v.DebugNeeds(50, 50); v.DebugKey(Keys.D2, true); v.DebugKey(Keys.D2, false); log("pão de queijo (tecla 2): " + needs()); },
                    () =>
                    {
                        // sem moedas: o refeitório não vende
                        int spent = gs.Prog.Spent; gs.Prog.Spent = gs.Prog.Wallet + spent + 5;
                        v.Teleport(25f, 32.6f, 3); v.DebugAct(); v.DebugKey(Keys.D1, true); v.DebugKey(Keys.D1, false); v.DebugKey(Keys.D1, true); v.DebugKey(Keys.D1, false);
                        log("sem moedas, comprar prato: " + needs());
                        rshot("sala08_sem_moedas");
                        gs.Prog.Spent = spent;
                    },
                    () => { v.DebugFinishDialog(); own("patinete"); gs.Prog.Equip["ride"] = "patinete"; v.RefreshLook(); v.Teleport(37.5f, 44f, 2); },
                    () => rshot("sala09_patinete"),
                    () => { v.DebugFinishDialog(); gs.SpeedBuff = 0; sp0 = v.DebugXY; spT = Environment.TickCount; v.DebugKey(Keys.D, true); v.DebugKey(Keys.S, true); },
                    () =>
                    {
                        var p = v.DebugXY; float sec = (Environment.TickCount - spT) / 1000f;
                        v.DebugKey(Keys.D, false); v.DebugKey(Keys.S, false);
                        log("velocidade de patinete: " + (Math.Sqrt((p.X - sp0.X) * (p.X - sp0.X) + (p.Y - sp0.Y) * (p.Y - sp0.Y)) / sec).ToString("0.0") + " tiles/s");
                        rshot("sala10_patinete_andando");
                        v.DebugFinishDialog(); v.DebugKey(Keys.P, true); v.DebugKey(Keys.P, false);
                        log("tecla P desce do patinete: " + (gs.Prog.Equipped("ride") == null));
                        v.Teleport(14f, 15.2f, 2);
                    },
                    () => { v.DebugFinishDialog(); gs.SpeedBuff = 0; sp0 = v.DebugXY; spT = Environment.TickCount; v.DebugKey(Keys.D, true); v.DebugKey(Keys.S, true); },
                    () =>
                    {
                        var p = v.DebugXY; float sec = (Environment.TickCount - spT) / 1000f;
                        v.DebugKey(Keys.D, false); v.DebugKey(Keys.S, false);
                        log("velocidade andando: " + (Math.Sqrt((p.X - sp0.X) * (p.X - sp0.X) + (p.Y - sp0.Y) * (p.Y - sp0.Y)) / sec).ToString("0.0") + " tiles/s");
                        v.Teleport(14f, 15.2f, 2);
                    },
                    () => { v.DebugFinishDialog(); gs.SpeedBuff = 0; sp0 = v.DebugXY; spT = Environment.TickCount; v.DebugKey(Keys.ShiftKey, true); v.DebugKey(Keys.D, true); v.DebugKey(Keys.S, true); },
                    () =>
                    {
                        var p = v.DebugXY; float sec = (Environment.TickCount - spT) / 1000f;
                        v.DebugKey(Keys.D, false); v.DebugKey(Keys.S, false); v.DebugKey(Keys.ShiftKey, false);
                        log("velocidade correndo: " + (Math.Sqrt((p.X - sp0.X) * (p.X - sp0.X) + (p.Y - sp0.Y) * (p.Y - sp0.Y)) / sec).ToString("0.0") + " tiles/s");
                    },
                    () => { shop = new ShopForm(gs, scale, (id, sz) => v.RoomIcon(id, sz)); shop.StartPosition = FormStartPosition.Manual; shop.Location = new Point(-5000, 40); shop.Show(); shop.DebugSlot("sala"); target = shop; },
                    () => shot("sala11_loja_minha_sala"),
                    () => { shop.DebugSlot("util"); shop.DebugSelect("patinete"); },
                    () => { shot("sala12_loja_patinete"); shop.Close(); target = f; },
                    // fome e sede: visão embaçada no escritório e no terminal
                    () => { v.Teleport(20f, 15f, 0); v.DebugNeeds(25, 60); },
                    () => { rshot("sala13_fome_25"); log("embaçado com fome 25%: " + v.NeedBlur.ToString("0.00")); v.DebugNeeds(70, 4); },
                    () => { rshot("sala14_sede_4"); log("embaçado com sede 4%: " + v.NeedBlur.ToString("0.00")); },
                    () => { gs.Prog.Met.Add(1); v.Teleport(26.6f, 21.6f, 3); v.DebugAct(); },
                    () => { },
                    () =>
                    {
                        var b = mf.DebugBlurShot();
                        log("terminal aberto? " + t.Visible + " · embaçado sobre o terminal: " + (b != null) + " · opacidade " + mf.DebugBlurOpacity.ToString("0.00"));
                        if (b != null) { b.Save(Path.Combine(Dir, "sala15_terminal_embacado.png")); b.Dispose(); }
                        shot("sala16_terminal_por_baixo");
                    },
                    () => { t.DebugClose(); v.DebugNeeds(100, 100); v.FreezeNeeds = false; },
                    () => { log("sem fome/sede: " + v.NeedBlur.ToString("0.00") + " · " + needs()); },
                    () => Application.Exit()
                };
                int rk = 0;
                var rt = new Timer { Interval = 700 };
                rt.Tick += (s, e) =>
                {
                    if (rk >= rs.Count) return;
                    try { rs[rk++](); }
                    catch (Exception ex) { log("ERRO sala passo " + (rk - 1) + ": " + ex); }
                };
                rt.Start();
                f.Activate();
                return;
            }
            if (Environment.GetEnvironmentVariable("DQ_ISO") == "1")
            {
                // roteiro da visão isométrica: capturas pelo próprio OpenGL
                Action<string> gshot = name =>
                {
                    var b = v.DebugGlShot();
                    if (b != null) { b.Save(Path.Combine(Dir, name + ".png")); b.Dispose(); }
                    else log("sem OpenGL em " + name);
                };
                PointF spd0 = PointF.Empty; int spdT = 0;
                var isoSteps = new List<Action>
                {
                    () => { v.DebugFinishDialog(); v.ForceActive = true; },
                    () => { log("isométrico ativo? " + v.IsoMode); v.Teleport(20f, 15f, 0); },
                    () => gshot("iso01_corredor"),
                    () => { v.Teleport(28.6f, 24.4f, 3); },
                    () => gshot("iso02_lab_docker"),
                    () => { v.DebugAct(); gshot("iso03_conversa"); },
                    () => { v.DebugFinishDialog(); v.Teleport(53f, 46.4f, 0); },
                    () => gshot("iso04_sql"),
                    () => { v.Teleport(60f, 38.8f, 3); },
                    () => gshot("iso05_auditorio"),
                    () => { v.Teleport(82f, 8f, 3); },
                    () => gshot("iso06_sala_jogos"),
                    () => { v.Teleport(97.5f, 25.5f, 3); },
                    () => gshot("iso07_inovacao"),
                    () => { v.Teleport(90f, 39.2f, 3); },
                    () => gshot("iso08_area"),
                    () => { v.Teleport(14f, 15.2f, 2); v.DebugIsoZoom(0); },
                    () => { spd0 = v.DebugXY; spdT = Environment.TickCount; v.DebugKey(Keys.D, true); v.DebugKey(Keys.S, true); },
                    () =>
                    {
                        var p = v.DebugXY; float sec = (Environment.TickCount - spdT) / 1000f;
                        v.DebugKey(Keys.D, false); v.DebugKey(Keys.S, false);
                        log("velocidade andando: " + (Math.Sqrt((p.X - spd0.X) * (p.X - spd0.X) + (p.Y - spd0.Y) * (p.Y - spd0.Y)) / sec).ToString("0.0") + " tiles/s");
                        v.Teleport(14f, 15.2f, 2);
                    },
                    () => { spd0 = v.DebugXY; spdT = Environment.TickCount; v.DebugKey(Keys.ShiftKey, true); v.DebugKey(Keys.D, true); v.DebugKey(Keys.S, true); },
                    () =>
                    {
                        var p = v.DebugXY; float sec = (Environment.TickCount - spdT) / 1000f;
                        v.DebugKey(Keys.D, false); v.DebugKey(Keys.S, false); v.DebugKey(Keys.ShiftKey, false);
                        log("velocidade correndo: " + (Math.Sqrt((p.X - spd0.X) * (p.X - spd0.X) + (p.Y - spd0.Y) * (p.Y - spd0.Y)) / sec).ToString("0.0") + " tiles/s");
                        gshot("iso08b_depois_de_correr");
                    },
                    () => { v.DebugKey(Keys.V, true); v.DebugKey(Keys.V, false); log("tecla V não muda a visão: " + v.IsoMode); },
                    () => { v.Teleport(8f, 30.5f, 0); v.DebugIsoZoom(1); },
                    () => gshot("iso09_recepcao_zoom"),
                    () =>
                    {
                        v.DebugIsoZoom(0);
                        v.Teleport(20f, 15f, 2);
                        for (int q = 0; q < v.Prof.Length; q++) v.Prof[q] = 0; v.ProfFrames = 0; v.ProfTotal = 0; v.ProfTick = 0;
                        v.FrameLog.Clear();
                        v.DebugKey(Keys.D, true);
                    },
                    () => { }, () => { },
                    () =>
                    {
                        v.DebugKey(Keys.D, false);
                        int n = Math.Max(1, v.ProfFrames);
                        var fl = v.FrameLog.Skip(3).ToList();
                        log("PERF iso andando: quadros=" + v.ProfFrames + " · HUD " + (v.Prof[4] / n).ToString("0.0") + " ms · OpenGL " + (v.Prof[5] / n).ToString("0.0") + " ms · quadro " + (v.ProfTotal / n).ToString("0.0") + " ms · intervalo médio " + (fl.Count > 0 ? fl.Average() : 0).ToString("0.0") + " ms · máx " + (fl.Count > 0 ? fl.Max() : 0).ToString("0.0"));
                        gshot("iso10_andando");
                    },
                    () => { shot("iso13_printwindow"); },
                    () => Application.Exit()
                };
                int ik = 0;
                var it = new Timer { Interval = 700 };
                it.Tick += (s, e) =>
                {
                    if (ik >= isoSteps.Count) return;
                    try { isoSteps[ik++](); }
                    catch (Exception ex) { log("ERRO iso passo " + (ik - 1) + ": " + ex); }
                };
                it.Start();
                f.Activate();
                return;
            }
            if (Perf)
            {
                var ps = new List<Action>
                {
                    () => { v.DebugFinishDialog(); v.Teleport(20f, 15f, 0); v.ForceFps = 60; v.ForceActive = true; v.UseGdiPlusScale = Environment.GetEnvironmentVariable("DQ_GDIP") == "1"; },
                    () => { }, () => { },
                    () => { for (int q = 0; q < v.Prof.Length; q++) v.Prof[q] = 0; v.ProfFrames = 0; v.ProfTotal = 0; v.ProfTick = 0; v.ProfItems = 0; },
                    () => { }, () => { }, () => { }, () => { }, () => { }, () => { },
                    () =>
                    {
                        int n = Math.Max(1, v.ProfFrames);
                        log("PERF parado: quadros=" + v.ProfFrames + " em ~3,9 s · paint " + (v.ProfTotal / n).ToString("0.0") + " ms [mundo " + (v.Prof[0] / n).ToString("0.0") + " · ampliar " + (v.Prof[1] / n).ToString("0.0") + " · textos " + (v.Prof[2] / n).ToString("0.0") + " · hud " + (v.Prof[3] / n).ToString("0.0") + "] tick " + (v.ProfTick / n).ToString("0.0") + " · janela " + f.ClientSize + " " + f.WindowState + " ativa=" + (Form.ActiveForm != null));
                        for (int q = 0; q < v.Prof.Length; q++) v.Prof[q] = 0; v.ProfFrames = 0; v.ProfTotal = 0; v.ProfTick = 0; v.ProfItems = 0;
                        v.ForceFps = 0;
                        v.FrameLog.Clear(); v.PresentLag.Clear();
                        v.DebugKey(Keys.D, true);
                    },
                    () => { }, () => { }, () => { },
                    // primeira ida (prepara caches), volta, e mede a segunda ida
                    () => { v.DebugKey(Keys.D, false); v.DebugKey(Keys.A, true); },
                    () => { }, () => { }, () => { },
                    () => { v.DebugKey(Keys.A, false); for (int q = 0; q < v.Prof.Length; q++) v.Prof[q] = 0; v.ProfFrames = 0; v.ProfTotal = 0; v.ProfTick = 0; v.ProfItems = 0; v.FrameLog.Clear(); v.PresentLag.Clear(); v.DebugKey(Keys.D, true); },
                    () => { }, () => { }, () => { },
                    () =>
                    {
                        v.DebugKey(Keys.D, false);
                        int n = Math.Max(1, v.ProfFrames);
                        log("DETALHE: paredes " + (v.Prof[4] / n).ToString("0.00") + " · móveis " + (v.Prof[5] / n).ToString("0.00") + " · animados " + (v.Prof[6] / n).ToString("0.00") + " · personagens " + (v.Prof[7] / n).ToString("0.00") + " · decoração/portas " + (v.Prof[9] / n).ToString("0.00") + " · aquecer " + (v.Prof[8] / n).ToString("0.00") + " · itens/quadro " + (v.ProfItems / n));
                        log("PERF andando: quadros=" + v.ProfFrames + " em ~2,6 s · paint " + (v.ProfTotal / n).ToString("0.0") + " ms [mundo " + (v.Prof[0] / n).ToString("0.0") + " · ampliar " + (v.Prof[1] / n).ToString("0.0") + " · textos " + (v.Prof[2] / n).ToString("0.0") + " · hud " + (v.Prof[3] / n).ToString("0.0") + "] tick " + (v.ProfTick / n).ToString("0.0"));
                        var fl = v.FrameLog.Skip(3).ToList();
                        if (fl.Count > 2)
                        {
                            double avg = fl.Average(), sd = Math.Sqrt(fl.Sum(x => (x - avg) * (x - avg)) / fl.Count);
                            log("INTERVALOS andando: " + fl.Count + " quadros · média " + avg.ToString("0.00") + " ms · desvio " + sd.ToString("0.00") + " · mín " + fl.Min().ToString("0.0") + " · máx " + fl.Max().ToString("0.0") +
                                " · fora de ±4 ms da média: " + fl.Count(x => Math.Abs(x - avg) > 4) + " · vsync " + v.VsyncPeriod.ToString("0.00") + " ms x" + v.FramesPerRequest + " · desenho " + v.RenderMs.ToString("0.0") + " ms");
                            var pl = v.PresentLag.Skip(3).ToList();
                            if (pl.Count > 0) log("ENTREGA após a atualização: " + pl.Count + " · média " + pl.Average().ToString("0.00") + " ms · máx " + pl.Max().ToString("0.00") + " · acima do período: " + pl.Count(x => x > v.VsyncPeriod) );
                        }
                    },
                    () => Application.Exit()
                };
                int pk = 0;
                var pt = new Timer { Interval = 650 };
                pt.Tick += (s, e) => { if (pk < ps.Count) ps[pk++](); };
                pt.Start();
                f.Activate();
                return;
            }
            var steps = new List<Action>
            {
                () => shot("01_intro_gabi"),
                () => { v.DebugFinishDialog(); v.Teleport(20f, 15f, 2); },
                () => { },
                () => shot("02_corredor"),
                () => { v.Teleport(28.6f, 22.4f, 3); v.DebugAct(); },
                () => shot("03_mentor_docker"),
                () => { v.DebugFinishDialog(); v.Teleport(26.6f, 21.6f, 3); v.DebugAct(); log("terminal docker aberto? " + t.Visible); },
                () => t.DebugType("docker version"),
                () => t.DebugType("docker --version"),
                () => shot("04_terminal_docker"),
                () => { t.DebugClose(); gs.Prog.Met.Add(5); v.Teleport(11.6f, 8.6f, 3); v.DebugAct(); },
                () => shot("05_ide_frontend_vazio"),
                () => t.DebugType("<h1>Cafe 365</h1>"),
                () => t.DebugTypeOnly("<h1>Café 365</h1>"),
                () => shot("06_ide_frontend_erro"),
                () => t.DebugType("<h1>Café 365</h1>"),
                () => shot("07_ide_frontend_acerto"),
                () => { t.DebugClose(); gs.Prog.Met.Add(3); foreach (var i in gs.TierLevels("python", 0)) gs.Prog.Points[i] = 100; gs.Prog.BadgeDates["python:0"] = "05/10/2026"; v.Teleport(56.6f, 21.6f, 3); v.DebugAct(); },
                () => t.DebugTypeOnly("def dobro(n):\n    return n * 2"),
                () => shot("08_ide_python_multilinha"),
                () => t.DebugType("def dobro(n):\n    return n * 2"),
                () => shot("09_ide_python_acerto"),
                () => { t.DebugClose(); gs.Prog.Met.Add(6); v.Teleport(26.6f, 8.6f, 3); v.DebugAct(); },
                () => t.DebugType("npm install"),
                () => shot("10_ide_react_terminal_integrado"),
                () => t.DebugType("<h2>Olá, {nome}</h2>"),
                () => { },
                () => { t.DebugClose(); gs.Prog.Met.Add(8); v.Teleport(56.6f, 8.6f, 3); v.DebugAct(); },
                () => shot("50_aws_console_home"),
                () => t.DebugAws("s3", null),
                () => shot("51_aws_tarefa_concluida"),
                () => { t.DebugNext(); t.DebugAws("s3-create", null); },
                () => shot("52_aws_criar_bucket"),
                () => t.DebugType("aws s3 mb s3://cafe365-site"),
                () => shot("53_aws_cloudshell"),
                () => t.DebugAws("ec2", null),
                () => shot("54_aws_ec2"),
                () => t.DebugAws("iam-mfa", "rodrigo"),
                () => shot("55_aws_mfa"),
                () => { t.DebugClose(); gs.Prog.Met.Add(Story.SqlRoom); v.Teleport(53f, 44.4f, 0); },
                () => { },
                () => shot("60_corredor_sul_porta_sql"),
                () => v.Teleport(52.5f, 52.5f, 0),
                () => { },
                () => shot("61_sala_sql"),
                () => { v.Teleport(56.4f, 50.6f, 3); v.DebugAct(); log("terminal sql aberto? " + t.Visible); },
                () => t.DebugType("SHOW TABLES;"),
                () => t.DebugType("DESCRIBE produtos;"),
                () => shot("62_sql_explorando"),
                () => t.DebugType("SELECT * FROM produtos;"),
                () => shot("63_sql_acerto"),
                () => { t.DebugNext(); t.DebugType("SELECT nome FROM produtos;"); },
                () => shot("64_sql_erro"),
                () => t.DebugType("UPDATE produtos SET preco = 1;"),
                () => shot("65_sql_rollback"),
                () =>
                {
                    t.DebugClose();
                    foreach (var tier in new[] { 0, 1 }) foreach (var i in gs.TierLevels("sql", tier)) gs.Prog.Points[i] = 90;
                    gs.Prog.BadgeDates["sql:0"] = "07/10/2026"; gs.Prog.BadgeDates["sql:1"] = "07/10/2026";
                    v.Teleport(56.4f, 50.6f, 3); v.DebugAct();
                },
                () => t.DebugType("SELECT c.nome, p.total FROM pedidos p JOIN clientes c ON p.cliente_id = c.id;"),
                () => shot("66_sql_join"),
                () => { t.DebugNext(); t.DebugToggleList(); t.DebugType("SELECT c.nome FROM clientes c LEFT JOIN pedidos p ON p.cliente_id = c.id WHERE p.id IS NULL;"); },
                () => shot("67_sql_lista_escondida"),
                () => { t.DebugToggleList(); t.DebugClose(); v.Teleport(82.5f, 6.8f, 3); },
                () => { },
                () => shot("11_sala_de_jogos"),
                () => v.Teleport(97.5f, 9.6f, 3),
                () => shot("11b_soc"),
                () => v.Teleport(30f, 38.8f, 3),
                () => shot("11c_refeitorio"),
                () => v.Teleport(60f, 38.8f, 3),
                () => shot("11d_auditorio"),
                () => v.Teleport(90f, 39.2f, 3),
                () => shot("11e_jardim"),
                () => v.Teleport(107.5f, 28f, 3),
                () => shot("11f_corredor_leste"),
                () => v.Teleport(97.5f, 25.5f, 3),
                () => shot("11g_inovacao"),
                () => v.Teleport(82.5f, 22.5f, 3),
                () => shot("11h_helpdesk"),
                () => v.Teleport(40f, 43.5f, 2),
                () => shot("11i_corredor_sul"),
                () => v.Teleport(8f, 33.5f, 0),
                () => shot("11j_biblioteca"),
                () => { v.Teleport(94f, 6.8f, 3); v.DebugAct(); },
                () => shot("40_soc_kátia"),
                () => v.DebugAct(),
                () => shot("41_atividade_phishing"),
                () => { v.DebugKey(Keys.D1, true); v.DebugKey(Keys.D1, false); v.DebugKey(Keys.D1, true); v.DebugKey(Keys.D1, false); },
                () => shot("42_phishing_resposta"),
                () => { v.DebugCloseDialog(); v.Teleport(25f, 32.6f, 3); log("foco no refeitório: " + v.DebugFocus); v.DebugAct(); },
                () => shot("43_atividade_cardapio"),
                () => { v.DebugKey(Keys.D3, true); v.DebugKey(Keys.D3, false); v.DebugKey(Keys.D3, true); v.DebugKey(Keys.D3, false); v.DebugFinishDialog(); v.Teleport(86.5f, 3.3f, 3); v.DebugAct(); },
                // fliperama aberto pelo próprio jogo (E na máquina): joga sozinho, fecha e confere o resultado
                () =>
                {
                    var inv = Application.OpenForms.OfType<InvadersForm>().FirstOrDefault();
                    log("fliperama aberto pelo jogo? " + (inv != null));
                    if (inv != null) { inv.AutoPlay = true; inv.DebugPress(Keys.Enter); target = inv; }
                },
                () => { }, () => { }, () => { },
                () => shot("44_fliperama_invasores"),
                () => { var inv = target as InvadersForm; if (inv != null) { log("invasores: " + inv.Score + " pontos, onda " + inv.Wave); inv.Close(); } target = f; },
                () => shot("45_fliperama_resultado"),
                () => { v.DebugFinishDialog(); v.Teleport(79.5f, 7.6f, 3); v.DebugAct(); },
                () =>
                {
                    var pp = Application.OpenForms.OfType<PingPongForm>().FirstOrDefault();
                    log("pingue-pongue aberto pelo jogo? " + (pp != null));
                    if (pp != null) { pp.AutoPlay = true; pp.DebugPress(Keys.Enter); target = pp; }
                },
                () => { }, () => { }, () => { },
                () => shot("45b_pingue_pongue"),
                () => { var pp = target as PingPongForm; if (pp != null) { log("pingue-pongue: " + pp.PlayerScore + " x " + pp.CpuScore); pp.Close(); } target = f; },
                () => shot("45c_pingue_pongue_resultado"),
                () => { v.DebugFinishDialog(); v.Teleport(58f, 35f, 3); v.DebugAct(); },
                () => shot("46_tech_talk"),
                () => { v.DebugAct(); v.DebugAct(); },
                () => shot("47_tech_talk_quiz"),
                () => { v.DebugKey(Keys.D2, true); v.DebugKey(Keys.D2, false); v.DebugKey(Keys.D2, true); v.DebugKey(Keys.D2, false); v.DebugFinishDialog(); },
                () =>
                {
                    foreach (var pos in new[] { new PointF(20f, 15f), new PointF(60f, 38f), new PointF(107.5f, 28f) })
                    {
                        v.Teleport(pos.X, pos.Y, 0);
                        v.Refresh();
                        for (int q = 0; q < 4; q++) v.Prof[q] = 0;
                        var sw = Stopwatch.StartNew();
                        for (int fr = 0; fr < 40; fr++) v.Refresh();
                        log("vis=" + v.Visible + " tam=" + v.ClientSize + " form=" + f.WindowState + " ms por quadro em " + pos + ": " + (sw.ElapsedMilliseconds / 40.0).ToString("0.0") + "  [mundo " + (v.Prof[0] / 40).ToString("0.0") + " · ampliar " + (v.Prof[1] / 40).ToString("0.0") + " · textos " + (v.Prof[2] / 40).ToString("0.0") + " · hud " + (v.Prof[3] / 40).ToString("0.0") + "]");
                    }
                    log("memoria do processo (MB): " + (Process.GetCurrentProcess().WorkingSet64 / 1048576));
                    v.FrameLog.Clear();
                    v.DebugKey(Keys.D, true);
                },
                () => { },
                () =>
                {
                    v.DebugKey(Keys.D, false);
                    var fl = v.FrameLog.Skip(2).ToList();
                    if (fl.Count > 0)
                        log("quadros andando: " + fl.Count + " em ~1,3 s · média " + fl.Average().ToString("0.0") + " ms · mín " + fl.Min().ToString("0.0") + " · máx " + fl.Max().ToString("0.0") + " · acima de 20 ms: " + fl.Count(x => x > 20) + "");
                },
                () => { v.Teleport(29f, 23.5f, 3); v.DebugEvent("senha"); },
                () => shot("12_dilema_etico"),
                () => { v.DebugAct(); },
                () => shot("12b_dilema_opcoes"),
                () => { v.DebugKey(Keys.D1, true); v.DebugKey(Keys.D1, false); v.DebugKey(Keys.D1, true); v.DebugKey(Keys.D1, false); },
                () => shot("13_dilema_resultado"),
                () =>
                {
                    v.DebugFinishDialog();
                    // simula progresso: Docker e AWS avançados, um pouco de Python e Front
                    foreach (var c in new[] { "docker", "aws" }) for (int tier = 0; tier < 2; tier++) foreach (var i in gs.TierLevels(c, tier)) gs.Prog.Points[i] = 90;
                    foreach (var i in gs.TierLevels("python", 0)) gs.Prog.Points[i] = 100;
                    foreach (var i in gs.TierLevels("front", 0)) gs.Prog.Points[i] = 80;
                    gs.Prog.Xp["docker"] = 520; gs.Prog.Xp["aws"] = 410; gs.Prog.Xp["python"] = 90; gs.Prog.Xp["front"] = 70;
                    foreach (var bk in new[] { "docker:0", "docker:1", "aws:0", "aws:1", "python:0", "front:0" }) gs.Prog.BadgeDates[bk] = "02/10/2026";
                    gs.Prog.LastSeniority = 2; gs.Prog.Reputation = 78;
                    for (int r = 0; r < 9; r++) gs.Prog.Met.Add(r);
                    v.Teleport(44f, 15f, 2);
                },
                () => { },
                () => shot("14_hud_carreira"),
                () => v.DebugTab(),
                () => shot("15_painel_carreira"),
                () => { v.DebugTab(); BadgeForm.ExportDemo(Path.Combine(Dir, "badge_aws_intermediario.png"), "aws", 1, "Alex Souza"); BadgeForm.ExportDemo(Path.Combine(Dir, "badge_docker_profissional.png"), "docker", 2, "Alex Souza");
                        BadgeForm.ExportDemo(Path.Combine(Dir, "badge_python_iniciante.png"), "python", 0, "Alex Souza"); target = Off(new BadgeForm(gs, scale, "aws", 1)); },
                () => shot("16_badge_conquistada"),
                () => { target.Close(); target = Off(new BadgeForm(gs, scale, null, -1)); },
                () => shot("17_galeria_badges"),
                () => { target.Close(); target = f; v.Teleport(26.6f, 21.6f, 3); v.DebugAct(); },
                () => shot("18_terminal_niveis"),
                () => { t.DebugClose(); v.Teleport(20f, 15f, 0); foreach (var id in Catalog.Hotbar) gs.Prog.Inv[id] = 2; v.DebugStartIncident(); v.DebugAct(); },
                () => shot("19_relampago_convite"),
                () => { v.DebugKey(Keys.D1, true); v.DebugKey(Keys.D1, false); v.DebugKey(Keys.D1, true); v.DebugKey(Keys.D1, false); v.DebugFinishDialog(); log("relampago: " + (gs.Incident != null ? gs.Incident.Cat + " sala " + gs.Incident.Room : "nenhum")); },
                () => shot("20_relampago_aceito"),
                () =>
                {
                    if (gs.Incident == null) return;
                    gs.Prog.Met.Add(gs.Incident.Room);
                    var dk = v.World.TerminalDesks[gs.Incident.Room];
                    v.Teleport(dk.X + dk.W / 2 - 0.4f, dk.Y + 1.6f, 3);
                    v.DebugAct();
                },
                () => shot("21_terminal_relampago"),
                () => { if (gs.Incident != null) t.DebugType(gs.Incident.Level.Answers[0]); },
                () => shot("22_relampago_resolvido"),
                () => t.DebugClose(),
                () => shot("23_comemoracao"),
                () =>
                {
                    v.DebugFinishDialog();
                    foreach (var id in new[] { "bone_react", "moletom_mic", "pinguim", "oculos_escuros" }) gs.Prog.Owned.Add(id);
                    gs.Prog.Equip["hat"] = "bone_react"; gs.Prog.Equip["outfit"] = "moletom_mic"; gs.Prog.Equip["pet"] = "pinguim"; gs.Prog.Equip["acc"] = "oculos_escuros";
                    v.RefreshLook(); v.Teleport(11f, 15.2f, 0);
                },
                () => { },
                () => shot("24_visual_novo"),
                () => { gs.Prog.Equip["hat"] = "gorro_python"; gs.Prog.Equip["outfit"] = "camiseta_php"; gs.Prog.Equip["pet"] = "elefante"; gs.Prog.Equip.Remove("acc"); v.RefreshLook(); },
                () => { },
                () => shot("25_visual_novo2"),
                () => { gs.Prog.Equip["hat"] = "boina"; gs.Prog.Equip["outfit"] = "jaqueta"; gs.Prog.Equip["pet"] = "cobra"; v.RefreshLook(); },
                () => { },
                () => shot("26_visual_novo3"),
                () => { target = Off(new ShopForm(gs, scale)); },
                () => shot("27_loja"),
                () => { ((ShopForm)target).DebugSlot("use"); },
                () => shot("28_loja_consumiveis"),
                () => { ((ShopForm)target).DebugSlot("util"); },
                () => shot("29_loja_utilidades"),
                () => { target.Close(); target = Off(new DocsForm(gs, scale, "Python")); },
                () => shot("30_docs_python"),
                () => { target.Close(); target = Off(new DocsForm(gs, scale, "React")); },
                () => shot("31_docs_react"),
                () => { target.Close(); var keep = gs.Prog.Visual; gs.Prog.Visual = new[] { 3, 6, 1, 2, 1, 1, 1, 1 }; target = Off(new CharacterForm(gs, scale, true)); gs.Prog.Visual = keep; },
                () => shot("76_criar_personagem_feminina"),
                () => { target.Close(); target = Off(new LessonForm(gs, scale)); },
                () => shot("70_aula_slide1"),
                () => { var lf = (LessonForm)target; lf.DebugNext(); },
                () => shot("71_aula_slide_codigo"),
                () => { var lf = (LessonForm)target; for (int q = 0; q < 6; q++) lf.DebugNext(); },
                () => shot("72_aula_quiz"),
                () => { var lf = (LessonForm)target; lf.DebugAnswer(0); },
                () => shot("73_aula_quiz_resposta"),
                () => { var lf = (LessonForm)target; for (int q = 0; q < 3; q++) { lf.DebugAnswer(0); lf.DebugNext(); } },
                () => shot("74_aula_concluida"),
                () => { target.Close(); target = f; v.Teleport(62.5f, 34.6f, 3); },
                () => { },
                () => shot("75_auditorio_telao"),
                () => { target = f; },
                () => Application.Exit()
            };
            int k = 0;
            var timer = new Timer { Interval = 650 };
            timer.Tick += (s, e) =>
            {
                if (k >= steps.Count) { timer.Stop(); return; }
                try { steps[k++](); }
                catch (Exception ex) { File.WriteAllText(Path.Combine(Dir, "erro.txt"), "passo " + k + ": " + ex); Application.Exit(); }
            };
            timer.Start();
        }
    }
}
#endif
