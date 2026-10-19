using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace DockerQuest
{
    // Motor de áudio: mixer em tempo real (waveOut do Windows) que toca vários sons ao mesmo tempo.
    // Tudo é sintetizado na hora (sem arquivos): teclas, passos, vozes, murmúrio do escritório e ambiente.
    static class Audio
    {
        public const int Rate = 22050;
        const int BufSamples = 352, BufCount = 4;   // ~16 ms por bloco, ~64 ms de fila

        class Voice { public float[] Data; public double Pos, Step = 1; public float Vol; }

        static readonly object sync = new object();
        static readonly List<Voice> voices = new List<Voice>();
        static IntPtr hwo;
        static IntPtr[] hdrs, datas;
        static AutoResetEvent ev;
        static volatile bool running, ready;
        static int hdrSize, flagsOff;
        public static bool Available { get { return ready; } }

        public static volatile bool Muted;
        public static float Master = 0.9f;

        // ambiente (definido pelo jogo a cada quadro; suavizado no mixer)
        static volatile float murmurTarget, roomTarget, keysRate, ambTarget = 1f;
        static float murmurGain, roomGain, ambGain;
        static float[] murmurLoop, roomLoop;
        static int murmurPos, roomPos;
        static readonly Random rng = new Random();
        static double keyTimer = 1, burstGap;
        static int burstLeft;

        // bancos de sons
        static float[][] keys, spaces, enters, backs, syllables;
        static float[][][] steps;   // [piso][variação]
        static readonly Dictionary<string, float[]> tones = new Dictionary<string, float[]>();

        [StructLayout(LayoutKind.Sequential)]
        struct WAVEFORMATEX { public short wFormatTag, nChannels; public int nSamplesPerSec, nAvgBytesPerSec; public short nBlockAlign, wBitsPerSample, cbSize; }
        [StructLayout(LayoutKind.Sequential)]
        struct WAVEHDR { public IntPtr lpData; public int dwBufferLength, dwBytesRecorded; public IntPtr dwUser; public int dwFlags, dwLoops; public IntPtr lpNext, reserved; }

        [DllImport("winmm.dll")] static extern int waveOutOpen(out IntPtr h, int dev, ref WAVEFORMATEX fmt, IntPtr cb, IntPtr inst, int flags);
        [DllImport("winmm.dll")] static extern int waveOutPrepareHeader(IntPtr h, IntPtr hdr, int size);
        [DllImport("winmm.dll")] static extern int waveOutWrite(IntPtr h, IntPtr hdr, int size);
        const int WAVE_MAPPER = -1, CALLBACK_EVENT = 0x50000, WHDR_DONE = 1;

        // abre o dispositivo e começa a tocar numa thread própria (os sons são sintetizados lá, sem travar a janela)
        public static void Start()
        {
            if (running) return;
            running = true;
            var th = new Thread(() =>
            {
                try
                {
                    BuildBanks();
                    var fmt = new WAVEFORMATEX { wFormatTag = 1, nChannels = 1, nSamplesPerSec = Rate, nAvgBytesPerSec = Rate * 2, nBlockAlign = 2, wBitsPerSample = 16 };
                    ev = new AutoResetEvent(false);
                    if (waveOutOpen(out hwo, WAVE_MAPPER, ref fmt, ev.SafeWaitHandle.DangerousGetHandle(), IntPtr.Zero, CALLBACK_EVENT) != 0) { running = false; return; }
                    hdrSize = Marshal.SizeOf(typeof(WAVEHDR));
                    flagsOff = (int)Marshal.OffsetOf(typeof(WAVEHDR), "dwFlags");
                    hdrs = new IntPtr[BufCount]; datas = new IntPtr[BufCount];
                    for (int i = 0; i < BufCount; i++)
                    {
                        datas[i] = Marshal.AllocHGlobal(BufSamples * 2);
                        hdrs[i] = Marshal.AllocHGlobal(hdrSize);
                        var h = new WAVEHDR { lpData = datas[i], dwBufferLength = BufSamples * 2 };
                        Marshal.StructureToPtr(h, hdrs[i], false);
                        waveOutPrepareHeader(hwo, hdrs[i], hdrSize);
                    }
                    ready = true;
                    Loop();
                }
                catch { ready = false; running = false; }
            }) { IsBackground = true, Priority = ThreadPriority.AboveNormal, Name = "áudio" };
            th.Start();
        }

        static void Loop()
        {
            var mix = new float[BufSamples];
            var pcm = new short[BufSamples];
            for (int i = 0; i < BufCount; i++) { Fill(mix, pcm); Marshal.Copy(pcm, 0, datas[i], BufSamples); waveOutWrite(hwo, hdrs[i], hdrSize); }
            int next = 0;
            while (running)
            {
                ev.WaitOne(15);
                // reabastece os blocos na ordem em que terminaram
                while ((Marshal.ReadInt32(hdrs[next], flagsOff) & WHDR_DONE) != 0)
                {
                    Fill(mix, pcm);
                    Marshal.Copy(pcm, 0, datas[next], BufSamples);
                    waveOutWrite(hwo, hdrs[next], hdrSize);
                    next = (next + 1) % BufCount;
                }
            }
        }

        static void Fill(float[] mix, short[] pcm)
        {
            Array.Clear(mix, 0, mix.Length);
            lock (sync)
            {
                for (int v = voices.Count - 1; v >= 0; v--)
                {
                    var vo = voices[v];
                    var d = vo.Data;
                    for (int i = 0; i < mix.Length; i++)
                    {
                        if (vo.Pos < 0) { vo.Pos += vo.Step; continue; }   // começa com atraso
                        int k = (int)vo.Pos;
                        if (k >= d.Length - 1) { vo.Pos = d.Length; break; }
                        float f = (float)(vo.Pos - k);
                        mix[i] += (d[k] + (d[k + 1] - d[k]) * f) * vo.Vol;
                        vo.Pos += vo.Step;
                    }
                    if (vo.Pos >= d.Length - 1) voices.RemoveAt(v);
                }
            }
            // ambiente: murmúrio, ruído da sala e teclados distantes
            if (murmurLoop != null)
                for (int i = 0; i < mix.Length; i++)
                {
                    ambGain += (ambTarget - ambGain) * 0.0004f;
                    murmurGain += (murmurTarget - murmurGain) * 0.0003f;
                    roomGain += (roomTarget - roomGain) * 0.0003f;
                    mix[i] += (murmurLoop[murmurPos] * murmurGain + roomLoop[roomPos] * roomGain) * ambGain;
                    if (++murmurPos >= murmurLoop.Length) murmurPos = 0;
                    if (++roomPos >= roomLoop.Length) roomPos = 0;
                }
            double dtb = (double)BufSamples / Rate;
            float kr = keysRate * ambGain;
            if (burstLeft <= 0)
            {
                keyTimer -= dtb;
                if (kr > 0.01f && keyTimer <= 0)
                {
                    burstLeft = 3 + rng.Next(14);
                    burstGap = 0;
                    keyTimer = 0.4 + (-Math.Log(1 - rng.NextDouble()) / kr);
                }
            }
            else
            {
                burstGap -= dtb;
                if (burstGap <= 0 && keys != null)
                {
                    float vol = (0.035f + (float)rng.NextDouble() * 0.04f) * Math.Min(1f, 0.5f + kr * 0.3f) * ambGain;
                    var src = rng.Next(9) == 0 ? spaces : keys;
                    AddVoice(src[rng.Next(src.Length)], vol, 0.78f + (float)rng.NextDouble() * 0.12f, 0);
                    burstLeft--;
                    burstGap = 0.06 + rng.NextDouble() * 0.11;
                }
            }
            float m = Muted ? 0f : Master;
            for (int i = 0; i < mix.Length; i++)
            {
                float x = mix[i] * m;
                // limitador suave: evita estalos quando muitos sons tocam juntos
                if (x > 0.7f) x = 0.7f + (x - 0.7f) / (1f + (x - 0.7f) * 3f);
                else if (x < -0.7f) x = -0.7f + (x + 0.7f) / (1f - (x + 0.7f) * 3f);
                if (x > 1f) x = 1f; else if (x < -1f) x = -1f;
                pcm[i] = (short)(x * 32000);
            }
        }

        static void AddVoice(float[] data, float vol, float pitch, double delaySec)
        {
            if (data == null || vol <= 0.001f) return;
            lock (sync)
            {
                if (voices.Count > 48) voices.RemoveAt(0);
                voices.Add(new Voice { Data = data, Vol = vol, Step = pitch, Pos = -delaySec * Rate * pitch });
            }
        }

        // ───────────────────────── API do jogo ─────────────────────────
        public static void Play(float[] data, float vol, float pitch = 1f, double delay = 0) { if (ready && !Muted) AddVoice(data, vol, pitch, delay); }

        public static void Tone(string name, double[] spec)
        {
            if (!ready || Muted) return;
            float[] d;
            lock (tones) { if (!tones.TryGetValue(name, out d)) { d = Synth.Tone(spec); tones[name] = d; } }
            AddVoice(d, 0.55f, 1f, 0);
        }

        // tecla digitada: 0 comum, 1 espaço, 2 Enter, 3 apagar
        public static void Key(int kind)
        {
            if (!ready || Muted) return;
            var bank = kind == 1 ? spaces : kind == 2 ? enters : kind == 3 ? backs : keys;
            AddVoice(bank[rng.Next(bank.Length)], kind == 0 ? 0.42f : 0.5f, 0.94f + (float)rng.NextDouble() * 0.12f, 0);
        }

        // passo: piso 0 piso frio, 1 madeira, 2 carpete, 3 piso elevado, 4 grama
        public static void Step(int floor, float vol)
        {
            if (!ready || Muted || steps == null) return;
            var bank = steps[Math.Max(0, Math.Min(steps.Length - 1, floor))];
            AddVoice(bank[rng.Next(bank.Length)], vol, 0.92f + (float)rng.NextDouble() * 0.16f, 0);
        }

        // sílaba de "voz" (fala dos personagens, no estilo dos jogos): o tom muda de pessoa para pessoa
        public static void Syllable(float pitch, float vol, double delay = 0)
        {
            if (!ready || Muted || syllables == null) return;
            AddVoice(syllables[rng.Next(syllables.Length)], vol, pitch * (0.93f + (float)rng.NextDouble() * 0.14f), delay);
        }

        public static void Chatter(float pitch, float vol, int count)
        {
            double t = 0;
            for (int i = 0; i < count; i++) { Syllable(pitch, vol, t); t += 0.07 + rng.NextDouble() * 0.06; }
        }

        public static void SetAmbience(float murmur, float typingRate, float room, float master)
        {
            murmurTarget = murmur; keysRate = typingRate; roomTarget = room; ambTarget = master;
        }

        static void BuildBanks()
        {
            var r = new Random(1234);
            keys = new float[8][];
            for (int i = 0; i < keys.Length; i++) keys[i] = Synth.KeyClick(r, 230, 0.005, 0.07, 1.0);
            spaces = new float[3][];
            for (int i = 0; i < spaces.Length; i++) spaces[i] = Synth.KeyClick(r, 140, 0.011, 0.09, 0.7);
            enters = new float[3][];
            for (int i = 0; i < enters.Length; i++) enters[i] = Synth.KeyClick(r, 170, 0.009, 0.09, 1.0);
            backs = new float[3][];
            for (int i = 0; i < backs.Length; i++) backs[i] = Synth.KeyClick(r, 280, 0.005, 0.07, 0.9);
            steps = new float[5][][];
            for (int f = 0; f < 5; f++)
            {
                steps[f] = new float[5][];
                for (int i = 0; i < 5; i++) steps[f][i] = Synth.Footstep(r, f);
            }
            syllables = new float[12][];
            for (int i = 0; i < syllables.Length; i++) syllables[i] = Synth.Syllable(r);
            roomLoop = Synth.RoomTone(r);
            murmurLoop = Synth.Murmur(r);
        }
    }

    // Síntese dos sons (ruído filtrado, ressonâncias, formantes de vogais)
    static class Synth
    {
        const int Rate = Audio.Rate;
        const double TwoPi = Math.PI * 2;

        static void Normalize(float[] d, float peak)
        {
            float m = 0.0001f;
            foreach (var x in d) m = Math.Max(m, Math.Abs(x));
            float k = peak / m;
            for (int i = 0; i < d.Length; i++) d[i] *= k;
        }

        public static float[] Tone(double[] spec)
        {
            var samples = new List<float>();
            for (int n = 0; n + 1 < spec.Length; n += 2)
            {
                double f = spec[n];
                int count = (int)(Rate * spec[n + 1]);
                for (int i = 0; i < count; i++)
                {
                    double t = (double)i / Rate;
                    double env = Math.Min(1.0, i / 90.0) * Math.Exp(-3.0 * i / count);
                    double s = 0.75 * Math.Sin(TwoPi * f * t) + 0.25 * Math.Sin(TwoPi * f * 2 * t) * 0.5 + 0.12 * Math.Sign(Math.Sin(TwoPi * f * t));
                    samples.Add((float)(s * env * 0.6));
                }
            }
            var d = samples.ToArray();
            Normalize(d, 0.55f);
            return d;
        }

        // tecla mecânica: estalo agudo + "toc" grave + clique de retorno da tecla
        public static float[] KeyClick(Random r, double baseF, double thockTau, double len, double clickAmp)
        {
            int n = (int)(Rate * len);
            var d = new float[n];
            double f = baseF * (0.9 + r.NextDouble() * 0.2);
            double prev = 0, lp = 0;
            int upAt = (int)(Rate * (0.03 + r.NextDouble() * 0.015));
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / Rate;
                double noise = r.NextDouble() * 2 - 1;
                double hp = noise - prev; prev = noise;
                lp += (noise - lp) * 0.22;
                double click = hp * Math.Exp(-t / 0.0016) * clickAmp;
                double thock = Math.Sin(TwoPi * f * t) * Math.Exp(-t / thockTau) * 0.45;
                double body = lp * Math.Exp(-t / 0.007) * 0.55;
                double up = 0;
                if (i >= upAt)
                {
                    double tu = (double)(i - upAt) / Rate;
                    up = hp * Math.Exp(-tu / 0.0011) * 0.22 + Math.Sin(TwoPi * f * 1.7 * tu) * Math.Exp(-tu / 0.006) * 0.12;
                }
                d[i] = (float)(click + thock + body + up);
            }
            Normalize(d, 0.8f);
            return d;
        }

        // passo: batida abafada do calcanhar + ponta do pé, com o timbre do piso
        public static float[] Footstep(Random r, int floor)
        {
            int n = (int)(Rate * 0.2);
            var d = new float[n];
            double alpha = floor == 2 ? 0.035 : floor == 4 ? 0.25 : floor == 1 ? 0.08 : 0.12;
            double tau = floor == 2 ? 0.035 : floor == 4 ? 0.03 : 0.022;
            double knockF = floor == 1 ? 150 + r.NextDouble() * 40 : floor == 3 ? 240 + r.NextDouble() * 50 : 95 + r.NextDouble() * 20;
            double toe = 0.03 + r.NextDouble() * 0.02;
            double lp = 0, lp2 = 0, prev = 0;
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / Rate;
                double noise = r.NextDouble() * 2 - 1;
                double hp = noise - prev; prev = noise;
                lp += (noise - lp) * alpha;
                lp2 += (lp - lp2) * 0.5;
                double env = Math.Min(1, t / 0.002) * Math.Exp(-t / tau);
                double t2 = t - toe;
                double env2 = t2 > 0 ? Math.Min(1, t2 / 0.002) * Math.Exp(-t2 / (tau * 0.8)) * 0.55 : 0;
                double s = lp2 * (env + env2) * 2.2;
                s += Math.Sin(TwoPi * knockF * t) * Math.Exp(-t / (floor == 3 ? 0.05 : 0.025)) * (floor == 2 ? 0.15 : floor == 4 ? 0.05 : 0.45);
                if (floor == 0) s += hp * Math.Exp(-t / 0.0025) * 0.35 + (t2 > 0 ? hp * Math.Exp(-t2 / 0.002) * 0.2 : 0);   // estalo do sapato no piso
                if (floor == 4) s += (noise - lp) * Math.Exp(-t / 0.06) * Math.Min(1, t / 0.01) * 0.35;                     // folhas/grama
                d[i] = (float)s;
            }
            // volume conforme o piso: carpete e grama são bem mais macios
            Normalize(d, floor == 2 ? 0.38f : floor == 4 ? 0.42f : floor == 1 ? 0.7f : floor == 3 ? 0.68f : 0.62f);
            return d;
        }

        static readonly double[][] Vowels =
        {
            new double[] { 800, 1200 }, new double[] { 450, 1900 }, new double[] { 300, 2300 }, new double[] { 520, 850 },
            new double[] { 330, 750 }, new double[] { 620, 1700 }, new double[] { 700, 1100 }
        };

        // filtro passa-faixa (ressonância de um formante)
        class Bp
        {
            double b0, b2, a1, a2, x1, x2, y1, y2;
            public void Set(double f, double q)
            {
                double w = TwoPi * f / Rate, al = Math.Sin(w) / (2 * q), a0 = 1 + al;
                b0 = al / a0; b2 = -al / a0; a1 = -2 * Math.Cos(w) / a0; a2 = (1 - al) / a0;
            }
            public double Run(double x)
            {
                double y = b0 * x + b2 * x2 - a1 * y1 - a2 * y2;
                x2 = x1; x1 = x; y2 = y1; y1 = y;
                return y;
            }
        }

        // uma voz falando "nada com nada" (sílabas com vogais e entonação) — base do murmúrio e das falas
        static void Speak(Random r, float[] outBuf, int start, int len, double f0, double gain, bool muffled, bool phrase)
        {
            var b1 = new Bp(); var b2 = new Bp(); var b3 = new Bp();
            double phase = 0, lp = 0;
            int pos = start, end = Math.Min(outBuf.Length, start + len);
            int vPrev = r.Next(Vowels.Length);
            double vib = r.NextDouble() * TwoPi;
            while (pos < end)
            {
                int syl = (int)(Rate * (phrase ? 0.1 + r.NextDouble() * 0.15 : 0.085 + r.NextDouble() * 0.04));
                int vNext = r.Next(Vowels.Length);
                double pitchJump = 1 + (r.NextDouble() - 0.5) * 0.25;
                for (int k = 0; k < syl && pos < end; k++, pos++)
                {
                    double u = (double)k / syl;
                    double glide = Math.Min(1, u / 0.35);
                    double f1 = Vowels[vPrev][0] + (Vowels[vNext][0] - Vowels[vPrev][0]) * glide;
                    double f2 = Vowels[vPrev][1] + (Vowels[vNext][1] - Vowels[vPrev][1]) * glide;
                    if ((k & 31) == 0) { b1.Set(f1, 6); b2.Set(f2, 8); b3.Set(2700, 9); }
                    double t = (double)pos / Rate;
                    double f = f0 * pitchJump * (1 + 0.05 * Math.Sin(TwoPi * 0.6 * t + vib) - 0.08 * u);
                    phase += f / Rate;
                    phase -= Math.Floor(phase);
                    double src = 2 * phase - 1 - (r.NextDouble() - 0.5) * 0.15;
                    double env = Math.Pow(Math.Sin(Math.PI * Math.Min(1, u * 1.05)), 0.7);
                    if (k < syl * 0.12 && r.Next(3) == 0) env *= 0.6;   // consoante
                    double s = (b1.Run(src) + b2.Run(src) * 0.7 + b3.Run(src) * 0.25) * env;
                    if (muffled) { lp += (s - lp) * 0.3; s = lp; }
                    outBuf[pos] += (float)(s * gain);
                }
                vPrev = vNext;
                if (phrase && r.Next(6) == 0) pos += (int)(Rate * (0.25 + r.NextDouble() * 0.6));   // pausa entre frases
            }
        }

        public static float[] Syllable(Random r)
        {
            var d = new float[(int)(Rate * 0.11)];
            Speak(r, d, 0, d.Length, 175, 1, false, false);
            // corta qualquer resto depois da primeira sílaba e suaviza o fim
            for (int i = 0; i < d.Length; i++) { double u = (double)i / d.Length; d[i] *= (float)Math.Max(0, 1 - Math.Pow(u, 6)); }
            Normalize(d, 0.6f);
            return d;
        }

        // várias pessoas conversando longe: laço de 8 s (com as pontas emendadas)
        public static float[] Murmur(Random r)
        {
            int loop = Rate * 8, fade = Rate / 2;
            var d = new float[loop + fade];
            for (int v = 0; v < 6; v++)
            {
                double f0 = r.Next(2) == 0 ? 95 + r.NextDouble() * 45 : 165 + r.NextDouble() * 70;
                Speak(r, d, (int)(r.NextDouble() * Rate), d.Length, f0, 0.5 + r.NextDouble() * 0.5, true, true);
            }
            // mais abafado (distância) e um leve ar de sala
            double lp = 0;
            for (int i = 0; i < d.Length; i++) { lp += (d[i] - lp) * 0.25; d[i] = (float)(lp + (r.NextDouble() - 0.5) * 0.004); }
            var o = new float[loop];
            for (int i = 0; i < loop; i++)
            {
                o[i] = d[i];
                if (i < fade) { float w = (float)i / fade; o[i] = d[i] * w + d[loop + i] * (1 - w); }
            }
            Normalize(o, 0.45f);
            return o;
        }

        // ruído grave da sala (ar-condicionado, computadores)
        public static float[] RoomTone(Random r)
        {
            int loop = Rate * 3, fade = Rate / 3;
            var d = new float[loop + fade];
            double lp = 0, lp2 = 0;
            for (int i = 0; i < d.Length; i++)
            {
                double n = r.NextDouble() * 2 - 1;
                lp += (n - lp) * 0.03;
                lp2 += (lp - lp2) * 0.08;
                d[i] = (float)(lp2 + Math.Sin(TwoPi * 120 * i / Rate) * 0.002);
            }
            var o = new float[loop];
            for (int i = 0; i < loop; i++)
            {
                o[i] = d[i];
                if (i < fade) { float w = (float)i / fade; o[i] = d[i] * w + d[loop + i] * (1 - w); }
            }
            Normalize(o, 0.35f);
            return o;
        }
    }
}
