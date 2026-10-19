using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DockerQuest
{
    // Acesso direto ao OpenGL que já vem no Windows (opengl32.dll): nada para instalar.
    // Usado pela visão isométrica (estilo Habbo).
    static class GL
    {
        const string LIB = "opengl32.dll";
        public const uint TEXTURE_2D = 0x0DE1, BLEND = 0x0BE2, DEPTH_TEST = 0x0B71, ALPHA_TEST = 0x0BC0, FOG = 0x0B60, CULL_FACE = 0x0B44,
            MULTISAMPLE = 0x809D, POLYGON_OFFSET_FILL = 0x8037, LINE_SMOOTH = 0x0B20;
        public const uint SRC_ALPHA = 0x0302, ONE_MINUS_SRC_ALPHA = 0x0303, ONE = 1, ZERO = 0;
        public const uint COLOR_BUFFER_BIT = 0x4000, DEPTH_BUFFER_BIT = 0x0100;
        public const uint PROJECTION = 0x1701, MODELVIEW = 0x1700;
        public const uint TRIANGLES = 0x0004, QUADS = 0x0007;
        public const uint RGBA = 0x1908, BGRA = 0x80E1, UNSIGNED_BYTE = 0x1401, RGBA8 = 0x8058;
        public const uint TEXTURE_MIN_FILTER = 0x2801, TEXTURE_MAG_FILTER = 0x2800, TEXTURE_WRAP_S = 0x2802, TEXTURE_WRAP_T = 0x2803;
        public const int LINEAR = 0x2601, NEAREST = 0x2600, LINEAR_MIPMAP_LINEAR = 0x2703, CLAMP_TO_EDGE = 0x812F, CLAMP = 0x2900, REPEAT = 0x2901;
        public const uint GENERATE_MIPMAP = 0x8191, TEXTURE_MAX_LEVEL = 0x813D, TEXTURE_MAX_ANISOTROPY = 0x84FE, MAX_TEXTURE_MAX_ANISOTROPY = 0x84FF;
        public const uint UNPACK_ALIGNMENT = 0x0CF5, PACK_ALIGNMENT = 0x0D05, MAX_TEXTURE_SIZE = 0x0D33;
        public const uint FOG_MODE = 0x0B65, FOG_START = 0x0B63, FOG_END = 0x0B64, FOG_COLOR = 0x0B66, FOG_HINT = 0x0C54, NICEST = 0x1102;
        public const uint LEQUAL = 0x0203, GREATER = 0x0204, LESS = 0x0201;
        public const uint COMPILE = 0x1300;
        public const uint VENDOR = 0x1F00, RENDERER = 0x1F01, VERSION = 0x1F02, EXTENSIONS = 0x1F03;
        public const uint TEXTURE_ENV = 0x2300, TEXTURE_ENV_MODE = 0x2200, MODULATE = 0x2100;
        public const uint SMOOTH = 0x1D01, PERSPECTIVE_CORRECTION_HINT = 0x0C50;

        [DllImport(LIB, EntryPoint = "glEnable")] public static extern void Enable(uint cap);
        [DllImport(LIB, EntryPoint = "glDisable")] public static extern void Disable(uint cap);
        [DllImport(LIB, EntryPoint = "glBlendFunc")] public static extern void BlendFunc(uint s, uint d);
        [DllImport(LIB, EntryPoint = "glAlphaFunc")] public static extern void AlphaFunc(uint f, float r);
        [DllImport(LIB, EntryPoint = "glDepthFunc")] public static extern void DepthFunc(uint f);
        [DllImport(LIB, EntryPoint = "glDepthMask")] public static extern void DepthMask(bool f);
        [DllImport(LIB, EntryPoint = "glClear")] public static extern void Clear(uint mask);
        [DllImport(LIB, EntryPoint = "glClearColor")] public static extern void ClearColor(float r, float g, float b, float a);
        [DllImport(LIB, EntryPoint = "glViewport")] public static extern void Viewport(int x, int y, int w, int h);
        [DllImport(LIB, EntryPoint = "glMatrixMode")] public static extern void MatrixMode(uint m);
        [DllImport(LIB, EntryPoint = "glLoadIdentity")] public static extern void LoadIdentity();
        [DllImport(LIB, EntryPoint = "glLoadMatrixf")] public static extern void LoadMatrix(float[] m);
        [DllImport(LIB, EntryPoint = "glOrtho")] public static extern void Ortho(double l, double r, double b, double t, double n, double f);
        [DllImport(LIB, EntryPoint = "glBegin")] public static extern void Begin(uint mode);
        [DllImport(LIB, EntryPoint = "glEnd")] public static extern void End();
        [DllImport(LIB, EntryPoint = "glVertex3f")] public static extern void Vertex3(float x, float y, float z);
        [DllImport(LIB, EntryPoint = "glVertex2f")] public static extern void Vertex2(float x, float y);
        [DllImport(LIB, EntryPoint = "glTexCoord2f")] public static extern void TexCoord(float s, float t);
        [DllImport(LIB, EntryPoint = "glColor4f")] public static extern void Color4(float r, float g, float b, float a);
        [DllImport(LIB, EntryPoint = "glColor4ub")] public static extern void Color4ub(byte r, byte g, byte b, byte a);
        [DllImport(LIB, EntryPoint = "glGenTextures")] public static extern void GenTextures(int n, uint[] tex);
        [DllImport(LIB, EntryPoint = "glDeleteTextures")] public static extern void DeleteTextures(int n, uint[] tex);
        [DllImport(LIB, EntryPoint = "glBindTexture")] public static extern void BindTexture(uint target, uint tex);
        [DllImport(LIB, EntryPoint = "glTexImage2D")] public static extern void TexImage2D(uint target, int level, int internalFormat, int w, int h, int border, uint format, uint type, IntPtr data);
        [DllImport(LIB, EntryPoint = "glTexImage2D")] public static extern void TexImage2D(uint target, int level, int internalFormat, int w, int h, int border, uint format, uint type, byte[] data);
        [DllImport(LIB, EntryPoint = "glTexSubImage2D")] public static extern void TexSubImage2D(uint target, int level, int x, int y, int w, int h, uint format, uint type, IntPtr data);
        [DllImport(LIB, EntryPoint = "glTexParameteri")] public static extern void TexParameter(uint target, uint name, int value);
        [DllImport(LIB, EntryPoint = "glTexParameterf")] public static extern void TexParameterf(uint target, uint name, float value);
        [DllImport(LIB, EntryPoint = "glTexEnvi")] public static extern void TexEnv(uint target, uint name, int value);
        [DllImport(LIB, EntryPoint = "glPixelStorei")] public static extern void PixelStore(uint name, int value);
        [DllImport(LIB, EntryPoint = "glGetString")] static extern IntPtr GetStringPtr(uint name);
        [DllImport(LIB, EntryPoint = "glGetIntegerv")] public static extern void GetInteger(uint name, int[] v);
        [DllImport(LIB, EntryPoint = "glGetFloatv")] public static extern void GetFloat(uint name, float[] v);
        [DllImport(LIB, EntryPoint = "glGenLists")] public static extern uint GenLists(int n);
        [DllImport(LIB, EntryPoint = "glNewList")] public static extern void NewList(uint list, uint mode);
        [DllImport(LIB, EntryPoint = "glEndList")] public static extern void EndList();
        [DllImport(LIB, EntryPoint = "glCallList")] public static extern void CallList(uint list);
        [DllImport(LIB, EntryPoint = "glDeleteLists")] public static extern void DeleteLists(uint list, int n);
        [DllImport(LIB, EntryPoint = "glFogi")] public static extern void Fogi(uint name, int v);
        [DllImport(LIB, EntryPoint = "glFogf")] public static extern void Fogf(uint name, float v);
        [DllImport(LIB, EntryPoint = "glFogfv")] public static extern void Fogfv(uint name, float[] v);
        [DllImport(LIB, EntryPoint = "glHint")] public static extern void Hint(uint target, uint mode);
        [DllImport(LIB, EntryPoint = "glShadeModel")] public static extern void ShadeModel(uint mode);
        [DllImport(LIB, EntryPoint = "glPolygonOffset")] public static extern void PolygonOffset(float factor, float units);
        [DllImport(LIB, EntryPoint = "glFlush")] public static extern void Flush();
        [DllImport(LIB, EntryPoint = "glFinish")] public static extern void Finish();
        [DllImport(LIB, EntryPoint = "glReadPixels")] public static extern void ReadPixels(int x, int y, int w, int h, uint format, uint type, IntPtr data);
        [DllImport(LIB, EntryPoint = "glGetError")] public static extern uint GetError();
        [DllImport(LIB, EntryPoint = "glPushMatrix")] public static extern void PushMatrix();
        [DllImport(LIB, EntryPoint = "glPopMatrix")] public static extern void PopMatrix();
        [DllImport(LIB, EntryPoint = "glTranslatef")] public static extern void Translate(float x, float y, float z);
        [DllImport(LIB, EntryPoint = "glRotatef")] public static extern void Rotate(float angle, float x, float y, float z);
        [DllImport(LIB, EntryPoint = "glCopyTexSubImage2D")] public static extern void CopyTexSubImage2D(uint target, int level, int xoff, int yoff, int x, int y, int w, int h);

        [DllImport("glu32.dll", EntryPoint = "gluBuild2DMipmaps")] public static extern int Build2DMipmaps(uint target, int internalFormat, int w, int h, uint format, uint type, byte[] data);

        public static string GetString(uint name)
        {
            var p = GetStringPtr(name);
            return p == IntPtr.Zero ? "" : Marshal.PtrToStringAnsi(p);
        }

        // ── contexto (WGL)
        [StructLayout(LayoutKind.Sequential)]
        public struct PIXELFORMATDESCRIPTOR
        {
            public ushort nSize, nVersion;
            public uint dwFlags;
            public byte iPixelType, cColorBits, cRedBits, cRedShift, cGreenBits, cGreenShift, cBlueBits, cBlueShift, cAlphaBits, cAlphaShift;
            public byte cAccumBits, cAccumRedBits, cAccumGreenBits, cAccumBlueBits, cAccumAlphaBits, cDepthBits, cStencilBits, cAuxBuffers, iLayerType, bReserved;
            public uint dwLayerMask, dwVisibleMask, dwDamageMask;
        }
        [DllImport("kernel32.dll")] static extern IntPtr LoadLibrary(string name);
        [DllImport("user32.dll")] public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")] public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
        [DllImport("gdi32.dll")] static extern int ChoosePixelFormat(IntPtr hdc, ref PIXELFORMATDESCRIPTOR pfd);
        [DllImport("gdi32.dll")] static extern bool SetPixelFormat(IntPtr hdc, int format, ref PIXELFORMATDESCRIPTOR pfd);
        [DllImport("gdi32.dll")] static extern int DescribePixelFormat(IntPtr hdc, int format, uint size, ref PIXELFORMATDESCRIPTOR pfd);
        [DllImport("gdi32.dll")] public static extern bool SwapBuffers(IntPtr hdc);
        [DllImport(LIB)] static extern IntPtr wglCreateContext(IntPtr hdc);
        [DllImport(LIB)] public static extern bool wglMakeCurrent(IntPtr hdc, IntPtr ctx);
        [DllImport(LIB)] public static extern bool wglDeleteContext(IntPtr ctx);
        [DllImport(LIB)] static extern IntPtr wglGetProcAddress(string name);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        delegate bool ChoosePixelFormatARB(IntPtr hdc, int[] iattr, float[] fattr, uint max, int[] formats, out uint count);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        delegate bool SwapIntervalEXT(int interval);

        static PIXELFORMATDESCRIPTOR BasicPfd()
        {
            return new PIXELFORMATDESCRIPTOR
            {
                nSize = (ushort)Marshal.SizeOf(typeof(PIXELFORMATDESCRIPTOR)), nVersion = 1,
                dwFlags = 0x4 | 0x20 | 0x1,   // DRAW_TO_WINDOW | SUPPORT_OPENGL | DOUBLEBUFFER
                iPixelType = 0, cColorBits = 32, cAlphaBits = 8, cDepthBits = 24, cStencilBits = 8, iLayerType = 0
            };
        }

        // procura um formato com antisserrilhamento (MSAA 4x); usa uma janela temporária porque o formato
        // de uma janela só pode ser escolhido uma vez
        static int FindMultisampleFormat(out int samples)
        {
            samples = 0;
            using (var tmp = new Form { ShowInTaskbar = false, FormBorderStyle = FormBorderStyle.None, Size = new System.Drawing.Size(4, 4) })
            {
                IntPtr hwnd = tmp.Handle, hdc = GetDC(hwnd);
                try
                {
                    var pfd = BasicPfd();
                    int f = ChoosePixelFormat(hdc, ref pfd);
                    if (f == 0 || !SetPixelFormat(hdc, f, ref pfd)) return 0;
                    IntPtr ctx = wglCreateContext(hdc);
                    if (ctx == IntPtr.Zero) return 0;
                    try
                    {
                        wglMakeCurrent(hdc, ctx);
                        IntPtr fp = wglGetProcAddress("wglChoosePixelFormatARB");
                        if (fp == IntPtr.Zero) return 0;
                        var choose = (ChoosePixelFormatARB)Marshal.GetDelegateForFunctionPointer(fp, typeof(ChoosePixelFormatARB));
                        foreach (int s in new[] { 8, 4, 2 })
                        {
                            int[] attr = { 0x2001, 1, 0x2010, 1, 0x2011, 1, 0x2003, 0x2027, 0x2013, 0x202B, 0x2014, 32, 0x201B, 8, 0x2022, 24, 0x2023, 8, 0x2041, 1, 0x2042, s, 0 };
                            var formats = new int[4];
                            uint n;
                            if (choose(hdc, attr, null, 4, formats, out n) && n > 0) { samples = s; return formats[0]; }
                        }
                        return 0;
                    }
                    finally { wglMakeCurrent(IntPtr.Zero, IntPtr.Zero); wglDeleteContext(ctx); }
                }
                catch { return 0; }
                finally { ReleaseDC(hwnd, hdc); }
            }
        }

        public static int Samples;

        // cria o contexto OpenGL na janela; devolve IntPtr.Zero se não der
        public static IntPtr CreateContext(IntPtr hwnd, IntPtr hdc)
        {
            LoadLibrary(LIB);
            int samples;
            int fmt = 0;
            try { fmt = FindMultisampleFormat(out samples); } catch { samples = 0; }
            var pfd = BasicPfd();
            if (fmt != 0) DescribePixelFormat(hdc, fmt, (uint)Marshal.SizeOf(typeof(PIXELFORMATDESCRIPTOR)), ref pfd);
            else { fmt = ChoosePixelFormat(hdc, ref pfd); samples = 0; }
            if (fmt == 0 || !SetPixelFormat(hdc, fmt, ref pfd)) return IntPtr.Zero;
            IntPtr ctx = wglCreateContext(hdc);
            if (ctx == IntPtr.Zero) return IntPtr.Zero;
            if (!wglMakeCurrent(hdc, ctx)) { wglDeleteContext(ctx); return IntPtr.Zero; }
            Samples = samples;
            // o ritmo dos quadros já é controlado pelo jogo (sincronia pelo DWM): a troca não espera
            try
            {
                IntPtr sp = wglGetProcAddress("wglSwapIntervalEXT");
                if (sp != IntPtr.Zero) ((SwapIntervalEXT)Marshal.GetDelegateForFunctionPointer(sp, typeof(SwapIntervalEXT)))(0);
            }
            catch { }
            return ctx;
        }
    }

    // janela filha onde o OpenGL desenha (CS_OWNDC: o contexto fica preso a ela)
    class GlHost : Control
    {
        public IntPtr Hdc, Ctx;
        public Action PaintRequested;
        public Action<MouseEventArgs> Mouse;
        public GlHost()
        {
            SetStyle(ControlStyles.Opaque | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.Selectable, false);
            TabStop = false;
        }
        protected override CreateParams CreateParams
        {
            get { var cp = base.CreateParams; cp.ClassStyle |= 0x20; return cp; }   // CS_OWNDC
        }
        public bool Init()
        {
            if (Ctx != IntPtr.Zero) return true;
            IntPtr h = Handle;   // cria a janela mesmo escondida
            Hdc = GL.GetDC(h);
            Ctx = GL.CreateContext(Handle, Hdc);
            return Ctx != IntPtr.Zero;
        }
        protected override void OnPaintBackground(PaintEventArgs e) { }
        protected override void OnPaint(PaintEventArgs e) { if (PaintRequested != null) PaintRequested(); }
        protected override void OnMouseDown(MouseEventArgs e) { if (Mouse != null) Mouse(e); }
        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (Ctx != IntPtr.Zero) { GL.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero); GL.wglDeleteContext(Ctx); Ctx = IntPtr.Zero; }
            base.OnHandleDestroyed(e);
        }
    }
}
