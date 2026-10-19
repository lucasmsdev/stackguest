using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace DockerQuest
{
    // Visão principal: isométrica em pixel (estilo Habbo), desenhada pelo OpenGL em baixa resolução.
    // Se o OpenGL não funcionar no computador, o jogo continua na visão de cima em HD.
    partial class GameView
    {
        bool isoMode, isoFailed;
        GlHost glHost;
        IntPtr frameBits;   // pixels do quadro (HUD) que vão por cima da cena do OpenGL
        int isoPending = Environment.GetEnvironmentVariable("STACKQUEST_2D") == "1" ? 0 : 2;
        float isoZoomMul = 1, isoZoomShown = 1;   // zoom pedido pela rodinha e o zoom mostrado (vai suavemente até ele)
        bool isoLoading;
        float isoLoadProgress;
        World3D w3dIso;
        float isoCamX = float.NaN, isoCamY;

        public bool IsoMode { get { return isoMode; } }
        public bool IsoFailed { get { return isoFailed; } }

        void EnsureGlHost()
        {
            if (glHost != null) return;
            glHost = new GlHost { Dock = DockStyle.Fill };
            glHost.PaintRequested = () => { if (isoMode) { RenderFrame(); GL.SwapBuffers(glHost.Hdc); } };
            glHost.Mouse = e => Focus();
            Controls.Add(glHost);
        }

        void EnterIso()
        {
            try
            {
                EnsureGlHost();
                // a janela do OpenGL só aparece quando o chão terminar de carregar (até lá, a barra de progresso)
                glHost.Visible = false;
                if (!glHost.Init()) throw new Exception("OpenGL indisponível");
                if (w3dIso == null)
                {
                    w3dIso = new World3D(World, walls) { ChunkSource = (cx, cy) => Chunk(cx, cy) };
                    w3dIso.Build();
                }
                // o chão inteiro é preparado antes de entrar (com barra de progresso): andando, nada engasga
                isoLoading = true;
                isoLoadProgress = 0;
                Focus();
            }
            catch (Exception ex)
            {
                isoFailed = true;
                isoMode = false;
                if (glHost != null) glHost.Visible = false;
                try
                {
                    string dir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StackQuest");
                    System.IO.Directory.CreateDirectory(dir);
                    System.IO.File.AppendAllText(System.IO.Path.Combine(dir, "erros.txt"), DateTime.Now + "  visão isométrica indisponível: " + ex + Environment.NewLine + Environment.NewLine);
                }
                catch { }
            }
        }

        // continua a preparação do chão (chamado a cada quadro enquanto carrega)
        void IsoLoadStep()
        {
            if (!isoLoading || w3dIso == null) return;
            try { isoLoadProgress = w3dIso.PreloadStep(px, py, 45); }
            catch (Exception) { isoLoadProgress = 1; }
            if (isoLoadProgress >= 1)
            {
                isoLoading = false;
                isoMode = true;
                w3dIso.RebuildRoom(World.RoomFurniture);
                if (glHost != null) glHost.Visible = true;
                isoCamX = float.NaN;
                renderSerial = -1;
            }
        }

        void DrawIsoLoading(Graphics g, int vw, int vh)
        {
            if (!isoLoading) return;
            float w = Math.Min(S(460), vw - S(40)), h = S(86), x = (vw - w) / 2, y = vh * 0.62f;
            Card(g, x, y, w, h);
            g.DrawString("Montando o escritório...", fHudBold, Art.B(Color.White), x + S(20), y + S(14));
            float bw = w - S(40);
            Art.RR(g, Art.Hx(0x30363D), x + S(20), y + S(50), bw, S(12), S(6));
            Art.RR(g, Art.Docker, x + S(20), y + S(50), Math.Max(S(12), bw * isoLoadProgress), S(12), S(6));
        }

        // zoom: pixels por tile em relação à altura da janela (a rodinha do mouse aproxima e afasta)
        float IsoZoom()
        {
            float vh = Math.Max(1, ClientSize.Height);
            return Math.Max(1.4f, vh / 330f) * isoZoomShown;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!isoMode) return;
            isoZoomMul *= e.Delta > 0 ? 1.15f : 1 / 1.15f;
            isoZoomMul = Math.Max(0.55f, Math.Min(2.2f, isoZoomMul));
        }

        // teclas relativas à tela: W sobe na tela (noroeste no mapa), D vai para a direita (nordeste)...
        void IsoInput(ref float ix, ref float iy)
        {
            float sx = 0, sy = 0;
            if (keys.Contains(Keys.A) || keys.Contains(Keys.Left)) sx -= 1;
            if (keys.Contains(Keys.D) || keys.Contains(Keys.Right)) sx += 1;
            if (keys.Contains(Keys.W) || keys.Contains(Keys.Up)) sy -= 1;
            if (keys.Contains(Keys.S) || keys.Contains(Keys.Down)) sy += 1;
            ix = (sx + sy) * 0.70710678f;
            iy = (sy - sx) * 0.70710678f;
        }

        static float FacingAngle(int f) { return f == 0 ? (float)Math.PI / 2 : f == 1 ? (float)Math.PI : f == 2 ? 0 : -(float)Math.PI / 2; }
        static int AvatarPhase(bool moving, float walk) { return moving ? 1 + (((int)Math.Floor(walk / (Math.PI * 2) * 8) % 8) + 8) % 8 : 0; }

        Frame3D BuildIsoFrame(int vw, int vh)
        {
            // câmera suave atrás do jogador
            float cdt = Math.Min(0.1f, camDt);
            camDt = 0;
            if (float.IsNaN(isoCamX) || Math.Abs(isoCamX - px) + Math.Abs(isoCamY - py) > 12) { isoCamX = px; isoCamY = py; }
            else
            {
                float k = 1f - (float)Math.Exp(-cdt * 11);
                isoCamX += (px - isoCamX) * k; isoCamY += (py - isoCamY) * k;
            }
            isoZoomShown += (isoZoomMul - isoZoomShown) * (1f - (float)Math.Exp(-cdt * 12));
            camMoving = Math.Abs(px - isoCamX) + Math.Abs(py - isoCamY) > 0.01f || Math.Abs(isoZoomMul - isoZoomShown) > 0.002f;
            var fr = new Frame3D { X = px, Y = py, Time = time, VW = vw, VH = vh, Zoom = IsoZoom(), CamX = isoCamX, CamY = isoCamY, Blur = NeedBlur };
            foreach (var n in World.Npcs)
            {
                if (Math.Abs(n.X - px) > 40 || Math.Abs(n.Y - py) > 40) continue;
                fr.Avatars.Add(new Avatar3D { Look = n.Look, X = n.X, Y = n.Y, Angle = FacingAngle(n.Facing), Walk = n.Walk, Moving = n.Moving });
            }
            float pa = (Math.Abs(dirX) + Math.Abs(dirY) > 0.01f) ? (float)Math.Atan2(dirY, dirX) : FacingAngle(facing);
            fr.Avatars.Add(new Avatar3D { Look = playerLook, X = px, Y = py, Angle = pa, Walk = walk, Moving = moving });
            if (playerLook.Pet != null && petInit)
            {
                int frame = petMoving ? (int)(time * 8) % 2 : 0;
                string kind = playerLook.Pet;
                int pf = petFacing;
                fr.Sprites.Add(new Billboard
                {
                    Key = "p" + kind + "|" + pf + "|" + frame,
                    Get = q => Scene.RenderPet(kind, pf, frame, q),
                    X = petX, Y = petY, W = Scene.PW / (float)Scene.T, H = Scene.PH / (float)Scene.T,
                    FX = Scene.PFootX / (float)Scene.PW, FY = Scene.PFootY / (float)Scene.PH, Shadow = 0.24f
                });
            }
            int curRoom = World.ZoneAt(px, py);
            float pulse = 0.5f + 0.5f * (float)Math.Sin(time * 3);
            for (int r = 0; r < Story.RoomCount; r++)
            {
                var dsk = World.TerminalDesks[r];
                if (dsk == null || Math.Abs(dsk.X - px) > 30 || Math.Abs(dsk.Y - py) > 30) continue;
                bool strong = (gs.Incident != null && gs.Incident.Room == r) || (gs.Incident == null && r == curRoom && gs.Prog.Met.Contains(r));
                float rad = strong ? 0.82f + 0.08f * pulse : 0.7f;
                fr.Decals.Add(new Decal { X = dsk.X + dsk.W / 2, Y = dsk.Y + dsk.H + 0.3f, RX = rad, RY = rad * 0.62f, Ring = true, C = Color.FromArgb(strong ? (int)(150 + 90 * pulse) : 90, Art.Docker) });
            }
            if (target.HasValue && page == null)
            {
                float p2 = 0.75f + 0.25f * (float)Math.Sin(time * 5);
                fr.Decals.Add(new Decal { X = target.Value.X, Y = target.Value.Y, RX = 0.55f * p2, RY = 0.55f * p2, Ring = true, C = Color.FromArgb(235, Art.Hx(0xF0B429)) });
            }
            return fr;
        }

        void RenderFrameIso()
        {
            int vw = ClientSize.Width, vh = ClientSize.Height;
            if (vw <= 0 || vh <= 0) return;
            renderSerial = tickSerial;
            if (frameBmp == null || frameBmp.Width != vw || frameBmp.Height != vh) CreateFrameSurface(vw, vh);
#if DEMO
            var isw = System.Diagnostics.Stopwatch.StartNew();
#endif
            var fr = BuildIsoFrame(vw, vh);
            w3dIso.SetupIso(fr);   // o HUD usa a mesma câmera para pôr nomes e setas no lugar certo
            using (var g = Graphics.FromImage(frameBmp))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.Clear(Color.Transparent);
                g.CompositingMode = CompositingMode.SourceOver;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                DrawOverlays(g, vw, vh);
                DrawHud(g, vw, vh, 0, 0);
                DrawSplash(g, vw, vh);
                if (Paused) Art.R(g, Art.A(Color.Black, 110), 0, 0, vw, vh);
            }
            GdiFlush();
            fr.Hud = frameBits;
#if DEMO
            Prof[4] += isw.Elapsed.TotalMilliseconds; isw.Restart();
            w3dIso.RenderIso(fr);
            GL.Finish();
            Prof[5] += isw.Elapsed.TotalMilliseconds;
#else
            w3dIso.RenderIso(fr);
#endif
        }

#if DEMO
        public string DebugWall(int x, int y) { return w3dIso == null ? "" : w3dIso.DebugWallInfo(x, y); }
        public PointF DebugProject(float x, float y, float h) { bool fr; return w3dIso == null ? PointF.Empty : w3dIso.Project(x, y, h, 0, 0, out fr); }
        public Bitmap DebugGlShot()
        {
            if (!isoMode || glHost == null) return null;
            RenderFrame();
            return w3dIso.ReadBack(ClientSize.Width, ClientSize.Height);
        }
        public PointF DebugXY { get { return new PointF(px, py); } }
        public void DebugIsoZoom(int z) { isoZoomMul = isoZoomShown = (float)Math.Pow(1.15, z * 2); }
#endif
    }
}
