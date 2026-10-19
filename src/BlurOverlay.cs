using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DockerQuest
{
    // Embaçado por cima da estação (terminal, IDE ou console AWS) quando o jogador está com fome ou sede.
    // É uma janela "fantasma": não recebe cliques nem foco, só mostra uma cópia borrada do que está embaixo.
    class BlurOverlay : Form
    {
        [DllImport("user32.dll")] static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
        Bitmap shot;
        public Bitmap Shot { get { return shot; } }

        public BlurOverlay()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            BackColor = Color.Black;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override bool ShowWithoutActivation { get { return true; } }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80000 | 0x20 | 0x08000000 | 0x80;   // LAYERED | TRANSPARENT (cliques passam) | NOACTIVATE | TOOLWINDOW
                return cp;
            }
        }

        public void ShowBlur(Form owner, Rectangle screenRect, float level)
        {
            if (screenRect.Width < 10 || screenRect.Height < 10) return;
            // copia a janela do jogo (esta janela não aparece na cópia) e recorta a área da estação
            var client = owner.RectangleToScreen(owner.ClientRectangle);
            var win = owner.Bounds;
            using (var full = new Bitmap(win.Width, win.Height, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(full))
                {
                    var hdc = g.GetHdc();
                    try { PrintWindow(owner.Handle, hdc, 2); } finally { g.ReleaseHdc(hdc); }
                }
                int ox = screenRect.X - win.X, oy = screenRect.Y - win.Y;
                // borrão: reduz e amplia de novo (quanto maior a fome/sede, menor a cópia)
                float k = 1.6f + level * 5.5f;
                int sw = Math.Max(8, (int)(screenRect.Width / k)), sh = Math.Max(8, (int)(screenRect.Height / k));
                var nb = new Bitmap(screenRect.Width, screenRect.Height, PixelFormat.Format32bppArgb);
                using (var small = new Bitmap(sw, sh, PixelFormat.Format32bppArgb))
                {
                    using (var g = Graphics.FromImage(small))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                        g.DrawImage(full, new Rectangle(0, 0, sw, sh), new Rectangle(ox, oy, screenRect.Width, screenRect.Height), GraphicsUnit.Pixel);
                    }
                    using (var g = Graphics.FromImage(nb))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                        g.PixelOffsetMode = PixelOffsetMode.Half;
                        g.DrawImage(small, new Rectangle(0, 0, screenRect.Width, screenRect.Height));
                        using (var b = new SolidBrush(Color.FromArgb((int)(70 * level), 30, 22, 14))) g.FillRectangle(b, 0, 0, screenRect.Width, screenRect.Height);
                    }
                }
                var old = shot;
                shot = nb;
                if (old != null) old.Dispose();
            }
            // pouco embaçado: dá para ler através; muito embaçado: quase só o borrão
            Opacity = 0.35 + 0.6 * Math.Min(1, level);
            if (Bounds != screenRect) Bounds = screenRect;
            if (!Visible) Show(owner);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (shot != null) e.Graphics.DrawImageUnscaled(shot, 0, 0);
        }
    }
}
