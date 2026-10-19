using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;

namespace DockerQuest
{
    class Progress
    {
        public int[] Points;
        public bool FirstRun, HasVisual;
        public string PlayerName = "";
        public readonly HashSet<int> Met = new HashSet<int>();

        // perfil do jogador
        // pele, estilo de cabelo, cor do cabelo, camisa, calça (ou saia), corpo (0 masc., 1 fem.), parte de baixo (0 calça, 1 saia),
        // tratamento nos textos (0 masculino, 1 feminino, 2 neutro "o(a)"). Saves antigos (5 números) ficam com corpo masc. e texto neutro.
        public int[] Visual = { 2, 0, 1, 0, 0, 0, 0, 2 };
        public readonly HashSet<string> Owned = new HashSet<string>();
        public readonly Dictionary<string, string> Equip = new Dictionary<string, string>();
        public int Spent, Bonus, Vouchers, Purchases;

        // estatísticas e objetivos
        public readonly HashSet<string> Achieved = new HashSet<string>();
        public readonly HashSet<string> Talked = new HashSet<string>();
        public readonly HashSet<string> Read = new HashSet<string>();
        public readonly HashSet<string> DocsRead = new HashSet<string>();
        public readonly HashSet<int> Visited = new HashSet<int>();
        public readonly HashSet<int> NoHint = new HashSet<int>();
        public int Coffees, Waters, Persist;
        // incidentes, consumíveis e efeitos guardados
        public int ExtraPts, Incidents, FastIncidents, NextSolveBonus, NextIncidentTime, NextIncidentBonus, Erasers, Consumed;
        public readonly Dictionary<string, int> Inv = new Dictionary<string, int>();
        public int Count(string id) { int v; return Inv.TryGetValue(id, out v) ? v : 0; }
        public float Walked;
        public float Hunger = 100, Thirst = 100;   // 100 = satisfeito; caem com o tempo

        // carreira
        public readonly Dictionary<string, int> Xp = new Dictionary<string, int>();          // XP por categoria + "soft"
        public readonly Dictionary<string, string> BadgeDates = new Dictionary<string, string>(); // "cat:tier" -> data
        public readonly HashSet<string> EventsDone = new HashSet<string>();
        public int Reputation = 50, GoodChoices, BadChoices, LastSeniority;
        public void AddXp(string cat, int v) { int c; Xp.TryGetValue(cat, out c); Xp[cat] = Math.Max(0, c + v); }

        // contadores das atividades das salas (quiz, phishing, chamados, IA, jardim, fliperama...)
        public readonly Dictionary<string, int> Stats = new Dictionary<string, int>();
        public int Stat(string k) { int v; return Stats.TryGetValue(k, out v) ? v : 0; }
        public void AddStat(string k, int v = 1) { Stats[k] = Stat(k) + v; }

        readonly string file;
        readonly string mirror;          // cópia extra em Documentos\StackQuest (sobrevive se a pasta AppData for limpa)
        readonly string[] keys;          // identidade estável de cada desafio: área + título (não depende da posição na lista)

        // chave estável de um desafio no save: "área:título" sem acentos/maiúsculas (e sem '=' / '|', que o save usa)
        public static string KeyOf(Level l)
        {
            return (l.Cat + ":" + Checker.Fold(l.Title ?? "").Trim()).Replace('=', '_').Replace('|', '_');
        }

        public Progress(List<Level> levels)
        {
            int n = levels.Count;
            Points = Enumerable.Repeat(-1, n).ToArray();
            keys = levels.Select(KeyOf).ToArray();
            file = Environment.GetEnvironmentVariable("STACKQUEST_SAVE");
            if (string.IsNullOrEmpty(file)) file = Environment.GetEnvironmentVariable("DOCKERQUEST_SAVE");
            if (string.IsNullOrEmpty(file))
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                file = Path.Combine(appData, "StackQuest", "progresso.txt");
                try
                {
                    string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    if (!string.IsNullOrEmpty(docs)) mirror = Path.Combine(docs, "StackQuest", "progresso.txt");
                }
                catch { }
                // migra o progresso da versão antiga (Docker Quest)
                string old = Path.Combine(appData, "DockerQuest", "progresso.txt");
                try
                {
                    if (!File.Exists(file) && File.Exists(old))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(file));
                        File.Copy(old, file);
                    }
                }
                catch { }
            }
            // lê o save principal; se ele faltar ou estiver estragado, usa o backup (.bak) ou a cópia em Documentos
            string[] lines = null;
            foreach (var cand in new[] { file, file + ".bak", mirror })
            {
                if (string.IsNullOrEmpty(cand)) continue;
                try
                {
                    if (!File.Exists(cand)) continue;
                    var ls = File.ReadAllLines(cand, Encoding.UTF8);
                    if (ls.Any(l => l.StartsWith("nome=") || l.StartsWith("setores="))) { lines = ls; break; }
                }
                catch { }
            }
            FirstRun = lines == null;
            if (FirstRun) return;
            var byKey = new Dictionary<string, int>();
            var noHintKeys = new List<string>();
            foreach (var line in lines)
            {
              try
              {
                    int eq = line.IndexOf('=');
                    if (eq < 0) continue;
                    string k = line.Substring(0, eq), v = line.Substring(eq + 1);
                    int i, p;
                    if (k.StartsWith("d:")) { if (int.TryParse(v, out p)) byKey[k.Substring(2)] = p; continue; }
                    switch (k)
                    {
                        case "nome": PlayerName = v.Trim(); break;
                        case "setores": foreach (var s in Split(v)) if (int.TryParse(s, out i)) Met.Add(i); break;
                        case "visitou": foreach (var s in Split(v)) if (int.TryParse(s, out i)) Visited.Add(i); break;
                        case "semdica": foreach (var s in Split(v)) if (int.TryParse(s, out i) && i >= 0 && i < n) NoHint.Add(i); break;
                        case "semdicak": noHintKeys.AddRange(v.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)); break;
                        case "visual":
                            var parts = Split(v);
                            for (int j = 0; j < Visual.Length && j < parts.Length; j++) int.TryParse(parts[j], out Visual[j]);
                            HasVisual = true;
                            break;
                        case "itens": foreach (var s in Split(v)) Owned.Add(s); break;
                        case "equip":
                            foreach (var s in Split(v)) { var kv = s.Split(':'); if (kv.Length == 2) Equip[kv[0]] = kv[1]; }
                            break;
                        case "conq": foreach (var s in Split(v)) Achieved.Add(s); break;
                        case "falou": foreach (var s in Split(v)) Talked.Add(s); break;
                        case "leu": foreach (var s in Split(v)) Read.Add(s); break;
                        case "docs": foreach (var s in v.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)) DocsRead.Add(s); break;
                        case "gasto": int.TryParse(v, out Spent); break;
                        case "fome": float.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out Hunger); break;
                        case "sede": float.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out Thirst); break;
                        case "bonus": int.TryParse(v, out Bonus); break;
                        case "vales": int.TryParse(v, out Vouchers); break;
                        case "compras": int.TryParse(v, out Purchases); break;
                        case "cafes": int.TryParse(v, out Coffees); break;
                        case "aguas": int.TryParse(v, out Waters); break;
                        case "persist": int.TryParse(v, out Persist); break;
                        case "extra": int.TryParse(v, out ExtraPts); break;
                        case "incid": int.TryParse(v, out Incidents); break;
                        case "incfast": int.TryParse(v, out FastIncidents); break;
                        case "nsolve": int.TryParse(v, out NextSolveBonus); break;
                        case "ninct": int.TryParse(v, out NextIncidentTime); break;
                        case "nincb": int.TryParse(v, out NextIncidentBonus); break;
                        case "borr": int.TryParse(v, out Erasers); break;
                        case "consum": int.TryParse(v, out Consumed); break;
                        case "rep": int.TryParse(v, out Reputation); break;
                        case "bons": int.TryParse(v, out GoodChoices); break;
                        case "maus": int.TryParse(v, out BadChoices); break;
                        case "sen": int.TryParse(v, out LastSeniority); break;
                        case "eventos": foreach (var s in Split(v)) EventsDone.Add(s); break;
                        case "xp":
                            foreach (var s in Split(v))
                            {
                                var kv = s.Split(':');
                                int qx;
                                if (kv.Length == 2 && int.TryParse(kv[1], out qx)) Xp[kv[0]] = qx;
                            }
                            break;
                        case "badges":
                            foreach (var s in v.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var kv = s.Split('=');
                                if (kv.Length == 2) BadgeDates[kv[0]] = kv[1];
                            }
                            break;
                        case "inv":
                            foreach (var s in Split(v))
                            {
                                var kv = s.Split(':');
                                int qty;
                                if (kv.Length == 2 && int.TryParse(kv[1], out qty)) Inv[kv[0]] = qty;
                            }
                            break;
                        case "stats":
                            foreach (var s in Split(v))
                            {
                                var kv = s.Split(':');
                                int sv;
                                if (kv.Length == 2 && int.TryParse(kv[1], out sv)) Stats[kv[0]] = sv;
                            }
                            break;
                        case "andou":
                            float.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out Walked);
                            break;
                        default:
                            if (int.TryParse(k, out i) && int.TryParse(v, out p) && i >= 0 && i < n) Points[i] = p;
                            break;
                    }
              }
              catch { }   // uma linha estranha não derruba o resto do save
            }
            // saves novos guardam cada desafio pela área + título: valem mesmo se a ordem dos desafios mudar numa versão nova
            if (byKey.Count > 0)
            {
                var pos = new Dictionary<string, int>();
                for (int j = 0; j < n; j++) if (!pos.ContainsKey(keys[j])) pos[keys[j]] = j;
                for (int j = 0; j < n; j++) Points[j] = -1;
                foreach (var kv in byKey) { int j; if (pos.TryGetValue(kv.Key, out j)) Points[j] = kv.Value; }
                NoHint.Clear();
                foreach (var nk in noHintKeys) { int j; if (pos.TryGetValue(nk, out j)) NoHint.Add(j); }
            }
        }

        static string[] Split(string v) { return v.Split(new[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries); }
        static string J<T>(IEnumerable<T> items) { return string.Join("|", items); }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                var lines = new List<string>
                {
                    "nome=" + PlayerName,
                    "setores=" + J(Met.OrderBy(x => x)),
                    "visitou=" + J(Visited.OrderBy(x => x)),
                    "semdica=" + J(NoHint.OrderBy(x => x)),
                    "itens=" + J(Owned),
                    "equip=" + J(Equip.Select(kv => kv.Key + ":" + kv.Value)),
                    "conq=" + J(Achieved),
                    "falou=" + J(Talked),
                    "leu=" + J(Read),
                    "docs=" + J(DocsRead),
                    "gasto=" + Spent, "bonus=" + Bonus, "vales=" + Vouchers, "compras=" + Purchases,
                    "cafes=" + Coffees, "aguas=" + Waters, "persist=" + Persist,
                    "extra=" + ExtraPts, "incid=" + Incidents, "incfast=" + FastIncidents, "nsolve=" + NextSolveBonus, "ninct=" + NextIncidentTime,
                    "nincb=" + NextIncidentBonus, "borr=" + Erasers, "consum=" + Consumed,
                    "inv=" + J(Inv.Where(kv => kv.Value > 0).Select(kv => kv.Key + ":" + kv.Value)),
                    "rep=" + Reputation, "bons=" + GoodChoices, "maus=" + BadChoices, "sen=" + LastSeniority,
                    "eventos=" + J(EventsDone),
                    "xp=" + J(Xp.Select(kv => kv.Key + ":" + kv.Value)),
                    "badges=" + J(BadgeDates.Select(kv => kv.Key + "=" + kv.Value)),
                    "stats=" + J(Stats.Select(kv => kv.Key + ":" + kv.Value)),
                    "andou=" + Walked.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture),
                    "fome=" + Hunger.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture),
                    "sede=" + Thirst.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)
                };
                if (HasVisual) lines.Insert(1, "visual=" + J(Visual));
                lines.Add("semdicak=" + J(NoHint.Where(i => i >= 0 && i < keys.Length).Select(i => keys[i])));
                // cada desafio pela área + título (vale entre versões) ...
                for (int i = 0; i < Points.Length; i++) if (Points[i] >= 0) lines.Add("d:" + keys[i] + "=" + Points[i]);
                // ... e também pela posição, para quem voltar a abrir uma versão antiga
                lines.AddRange(Points.Select((p, i) => i + "=" + p));
                WriteSafe(file, lines);
                if (!string.IsNullOrEmpty(mirror)) WriteSafe(mirror, lines);
            }
            catch { }
        }

        // grava num arquivo temporário e só então troca: se o PC desligar no meio, o save anterior continua inteiro (e fica um .bak)
        static void WriteSafe(string path, List<string> lines)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                string tmp = path + ".tmp";
                File.WriteAllLines(tmp, lines, Encoding.UTF8);
                if (File.Exists(path))
                {
                    try { File.Replace(tmp, path, path + ".bak", true); return; }
                    catch { File.Copy(path, path + ".bak", true); }
                }
                File.Copy(tmp, path, true);
                File.Delete(tmp);
            }
            catch { }
        }

        public bool Done(int i) { return Points[i] >= 0; }
        public bool Unlocked(int i) { return i == 0 || Done(i) || Done(i - 1); }
        public int MissionTotal { get { return Points.Where(p => p > 0).Sum(); } }
        public int Total { get { return MissionTotal + ExtraPts; } }
        public int Completed { get { return Points.Count(p => p >= 0); } }

        // Saldo para a lojinha: pontos das missões + bônus dos objetivos − o que já foi gasto.
        public int Wallet { get { return Total + Bonus - Spent; } }

        public string Equipped(string slot) { string v; return Equip.TryGetValue(slot, out v) ? v : null; }

        // Zera tudo (inclusive nome e visual) para o próximo aluno.
        public void Reset()
        {
            for (int i = 0; i < Points.Length; i++) Points[i] = -1;
            Met.Clear(); Owned.Clear(); Equip.Clear(); Achieved.Clear(); Talked.Clear(); Read.Clear(); DocsRead.Clear(); Visited.Clear(); NoHint.Clear();
            Spent = Bonus = Vouchers = Purchases = Coffees = Waters = Persist = 0;
            ExtraPts = Incidents = FastIncidents = NextSolveBonus = NextIncidentTime = NextIncidentBonus = Erasers = Consumed = 0;
            Inv.Clear();
            Xp.Clear(); BadgeDates.Clear(); EventsDone.Clear(); Stats.Clear();
            Reputation = 50; GoodChoices = BadChoices = LastSeniority = 0;
            Walked = 0;
            Hunger = Thirst = 100;
            PlayerName = "";
            Visual = new[] { 2, 0, 1, 0, 0, 0, 0, 2 };
            Gender.Mode = 2;
            HasVisual = false;
            Save();
        }
    }
    class GameState
    {
        public const int MaxPoints = 100, HintCost = 25, WrongCost = 5, MinPoints = 10, RevealAfter = 5;

        public readonly List<Level> LevelList;
        public readonly Progress Prog;
        public readonly Dictionary<string, int> TerminalLevel = new Dictionary<string, int>();

        // penalidades da tentativa atual de cada desafio (valem enquanto o jogo está aberto)
        public readonly Dictionary<int, int> Hints = new Dictionary<int, int>();
        public readonly Dictionary<int, int> Wrong = new Dictionary<int, int>();
        public readonly HashSet<int> Revealed = new HashSet<int>();
        public readonly Dictionary<int, int> FreeHints = new Dictionary<int, int>();   // dicas pagas com vale-dica

        // desafio relâmpago em andamento
        public Incident Incident;
        public float IncTime, IncTotal, IncCooldown = 150, SpeedBuff;
        public int IncWrong, IncBonus;
        public string LastIncidentId;

        public GameState()
        {
            LevelList = Challenges.All();
            Prog = new Progress(LevelList);
            Gender.Mode = Prog.Visual.Length > 7 ? Prog.Visual[7] : 2;
            foreach (var c in Challenges.Cats) TerminalLevel[c.Id] = DefaultLevel(c.Id);
        }

        public Level Lv(int i) { return LevelList[i]; }

        public List<int> CatLevels(string cat)
        {
            var l = new List<int>();
            for (int i = 0; i < LevelList.Count; i++) if (LevelList[i].Cat == cat) l.Add(i);
            // ordem de jogo: nível, depois Order, depois posição
            return l.OrderBy(i => LevelList[i].Tier).ThenBy(i => LevelList[i].Order).ThenBy(i => i).ToList();
        }

        public List<int> TierLevels(string cat, int tier)
        {
            var l = new List<int>();
            for (int i = 0; i < LevelList.Count; i++) if (LevelList[i].Cat == cat && LevelList[i].Tier == tier) l.Add(i);
            return l.OrderBy(i => LevelList[i].Order).ThenBy(i => i).ToList();
        }

        public bool TierDone(string cat, int tier) { return TierLevels(cat, tier).All(Prog.Done); }
        public int TierCount(string cat, int tier) { return TierLevels(cat, tier).Count(Prog.Done); }
        // quem já ganhou a badge do nível anterior não perde o acesso quando novos desafios entram nele
        public bool TierUnlocked(string cat, int tier) { return tier <= 0 || TierDone(cat, tier - 1) || Prog.BadgeDates.ContainsKey(cat + ":" + (tier - 1)); }
        public bool Unlocked(int i) { return TierUnlocked(LevelList[i].Cat, LevelList[i].Tier); }
        public int MaxTier(string cat) { int t = 0; while (t < 2 && TierUnlocked(cat, t + 1)) t++; return t; }
        public bool CatDone(string cat) { return CatLevels(cat).All(Prog.Done); }
        public int BadgeCount { get { return Prog.BadgeDates.Count; } }

        public int DefaultLevel(string cat)
        {
            var l = CatLevels(cat);
            foreach (var i in l) if (!Prog.Done(i) && Unlocked(i)) return i;
            return l[0];
        }

        public int HintsOf(int lv) { int v; return Hints.TryGetValue(lv, out v) ? v : 0; }
        public int WrongOf(int lv) { int v; return Wrong.TryGetValue(lv, out v) ? v : 0; }
        public int FreeOf(int lv) { int v; return FreeHints.TryGetValue(lv, out v) ? v : 0; }

        public int CurrentValue(int lv)
        {
            if (Revealed.Contains(lv)) return 0;
            return Math.Max(MinPoints, MaxPoints - HintCost * (HintsOf(lv) - FreeOf(lv)) - WrongCost * WrongOf(lv));
        }

        public void ClearSession(int lv) { Hints.Remove(lv); Wrong.Remove(lv); Revealed.Remove(lv); FreeHints.Remove(lv); }

        public int IncidentValue()
        {
            if (Incident == null) return 0;
            int v = DockerQuest.Incidents.BasePoints + (int)(DockerQuest.Incidents.TimeBonus * Math.Max(0, IncTime) / Math.Max(1, IncTotal))
                    - DockerQuest.Incidents.WrongCost * IncWrong + IncBonus;
            return Math.Max(DockerQuest.Incidents.MinPoints, v);
        }

        // XP ganho ao resolver: base do nível × aproveitamento × multiplicadores
        public int AwardXp(Level lv, int pts, bool flash)
        {
            int baseXp = Challenges.TierXp[Math.Max(0, Math.Min(2, lv.Tier))];
            float f = flash ? 2f : Math.Max(0.3f, pts / 100f);
            int xp = (int)Math.Round(baseXp * f * Career.XpMultiplier(Prog));
            Prog.AddXp(lv.Cat, xp);
            return xp;
        }

        public void ResetAll()
        {
            Prog.Reset();
            Hints.Clear(); Wrong.Clear(); Revealed.Clear(); FreeHints.Clear();
            Incident = null; IncCooldown = 150;
            foreach (var c in Challenges.Cats) TerminalLevel[c.Id] = DefaultLevel(c.Id);
        }

        public string Rank() { return Career.Title(Prog); }
    }

    // Efeitos sonoros sintetizados em memória (sem arquivos externos).
    static class Sfx
    {
        public static bool Muted { get { return Audio.Muted; } set { Audio.Muted = value; } }
        static readonly Dictionary<string, byte[]> cache = new Dictionary<string, byte[]>();
        static SoundPlayer player;

        public static void Play(string name)
        {
            if (Muted) return;
            // com o mixer ligado, os efeitos se somam aos passos, vozes e ambiente (sem cortar um ao outro)
            if (Audio.Available) { Audio.Tone(name, Spec(name)); return; }
            try
            {
                byte[] data;
                if (!cache.TryGetValue(name, out data)) { data = Build(Spec(name)); cache[name] = data; }
                if (player != null) player.Stop();
                player = new SoundPlayer(new MemoryStream(data));
                player.Play();
            }
            catch { }
        }

        static double[] Spec(string name)
        {
            switch (name)
            {
                case "ok": return new double[] { 523, 0.08, 659, 0.08, 784, 0.08, 1047, 0.25 };
                case "err": return new double[] { 196, 0.09, 147, 0.18 };
                case "unlock": return new double[] { 392, 0.08, 523, 0.08, 659, 0.08, 784, 0.08, 1047, 0.35 };
                case "hint": return new double[] { 880, 0.06, 1175, 0.14 };
                case "talk": return new double[] { 740, 0.03, 988, 0.03 };
                case "step": return new double[] { 120, 0.04 };
                case "open": return new double[] { 660, 0.05, 990, 0.08 };
                case "locked": return new double[] { 220, 0.08, 185, 0.12 };
                case "coffee": return new double[] { 587, 0.07, 784, 0.07, 659, 0.12 };
                case "ach": return new double[] { 784, 0.07, 988, 0.07, 1175, 0.07, 1568, 0.3 };
                case "buy": return new double[] { 1047, 0.05, 1319, 0.05, 1568, 0.12 };
                case "alarm": return new double[] { 880, 0.12, 660, 0.12, 880, 0.12, 660, 0.12, 880, 0.12, 660, 0.18 };
                case "use": return new double[] { 523, 0.05, 784, 0.1 };
                default: return new double[] { 660, 0.05 };
            }
        }

        static byte[] Build(double[] spec)
        {
            const int rate = 22050;
            var samples = new List<short>();
            for (int n = 0; n + 1 < spec.Length; n += 2)
            {
                double f = spec[n];
                int count = (int)(rate * spec[n + 1]);
                for (int i = 0; i < count; i++)
                {
                    double t = (double)i / rate;
                    double env = Math.Min(1.0, i / 90.0) * Math.Exp(-3.0 * i / count);
                    double s = 0.7 * Math.Sin(2 * Math.PI * f * t) + 0.3 * Math.Sign(Math.Sin(2 * Math.PI * f * t));
                    samples.Add((short)(s * env * 0.2 * short.MaxValue));
                }
            }
            var ms = new MemoryStream();
            var w = new BinaryWriter(ms);
            int len = samples.Count * 2;
            w.Write(Encoding.ASCII.GetBytes("RIFF")); w.Write(36 + len); w.Write(Encoding.ASCII.GetBytes("WAVE"));
            w.Write(Encoding.ASCII.GetBytes("fmt ")); w.Write(16); w.Write((short)1); w.Write((short)1);
            w.Write(rate); w.Write(rate * 2); w.Write((short)2); w.Write((short)16);
            w.Write(Encoding.ASCII.GetBytes("data")); w.Write(len);
            foreach (var s in samples) w.Write(s);
            return ms.ToArray();
        }
    }
}
