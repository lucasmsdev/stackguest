using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace DockerQuest
{
    // Gera src\icon.ico (várias resoluções, PNG dentro do ICO) e logo.png a partir de Art.Logo.
    // Compile com /main:DockerQuest.MakeIcon junto com src\*.cs e rode na pasta docker-quest.
    static class MakeIcon
    {
        static byte[] Png(int size)
        {
            using (var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    float pad = size >= 32 ? size * 0.03f : 0;
                    Art.Logo(g, pad, pad, size - 2 * pad);
                }
                using (var ms = new MemoryStream()) { bmp.Save(ms, ImageFormat.Png); return ms.ToArray(); }
            }
        }

        static int Main()
        {
            var sizes = new[] { 16, 20, 24, 32, 40, 48, 64, 128, 256 };
            var imgs = new List<byte[]>();
            foreach (var s in sizes) imgs.Add(Png(s));
            using (var fs = new FileStream(Path.Combine("src", "icon.ico"), FileMode.Create))
            using (var w = new BinaryWriter(fs))
            {
                w.Write((short)0); w.Write((short)1); w.Write((short)sizes.Length);
                int offset = 6 + 16 * sizes.Length;
                for (int i = 0; i < sizes.Length; i++)
                {
                    w.Write((byte)(sizes[i] >= 256 ? 0 : sizes[i])); w.Write((byte)(sizes[i] >= 256 ? 0 : sizes[i]));
                    w.Write((byte)0); w.Write((byte)0); w.Write((short)1); w.Write((short)32);
                    w.Write(imgs[i].Length); w.Write(offset);
                    offset += imgs[i].Length;
                }
                foreach (var b in imgs) w.Write(b);
            }
            File.WriteAllBytes("logo.png", Png(512));
            using (var bmp = new Bitmap(900, 260, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Art.Hx(0x0D1117));
                    Art.Wordmark(g, 40, 40, 180, Color.White);
                }
                bmp.Save("logo-wordmark.png", ImageFormat.Png);
            }
            Console.WriteLine("ok");
            return 0;
        }
    }
}
