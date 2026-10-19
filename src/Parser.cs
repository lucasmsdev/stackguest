using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DockerQuest
{
    class ParsedCommand
    {
        public List<string> Path = new List<string>();
        public List<KeyValuePair<string, string>> Flags = new List<KeyValuePair<string, string>>();
        public List<string> Args = new List<string>();
        public string Error;

        public List<string> FlagStrings(HashSet<string> ignore)
        {
            var list = Flags.Where(f => !ignore.Contains(f.Key))
                            .Select(f => f.Value == null ? f.Key : f.Key + "=" + f.Value)
                            .ToList();
            list.Sort(StringComparer.Ordinal);
            return list;
        }
    }

    class MatchResult
    {
        public bool Ok;
        public string Message;
    }

    // Interpreta um comando docker digitado e o compara de forma "inteligente" com as respostas
    // aceitas: ordem das flags, flags combinadas (-it), --flag=valor, formas longas/curtas,
    // aliases (docker container ls = docker ps) e ":latest" implícito são tratados como equivalentes.
    static class CommandParser
    {
        static readonly HashSet<string> Groups = new HashSet<string> {
            "container", "image", "volume", "network", "compose", "system", "builder", "buildx",
            "context", "plugin", "swarm", "service", "stack", "node", "secret", "config",
            "manifest", "trust", "scout"
        };

        static readonly HashSet<string> KnownTop = new HashSet<string> {
            "attach", "build", "builder", "buildx", "commit", "compose", "config", "container", "context",
            "cp", "create", "diff", "events", "exec", "export", "history", "image", "images", "import",
            "info", "init", "inspect", "kill", "load", "login", "logout", "logs", "manifest", "network",
            "node", "pause", "plugin", "port", "ps", "pull", "push", "rename", "restart", "rm", "rmi",
            "run", "save", "scout", "search", "secret", "service", "stack", "start", "stats", "stop",
            "swarm", "system", "tag", "top", "trust", "unpause", "update", "version", "volume", "wait"
        };

        static readonly HashSet<string> LongWithValue = new HashSet<string> {
            "--name", "--publish", "--volume", "--env", "--env-file", "--network", "--net", "--restart",
            "--memory", "--memory-swap", "--cpus", "--cpu-shares", "--tag", "--file", "--tail", "--filter",
            "--format", "--workdir", "--user", "--mount", "--entrypoint", "--output", "--input", "--driver",
            "--hostname", "--label", "--project-name", "--ip", "--link", "--add-host", "--platform",
            "--build-arg", "--target", "--network-alias", "--author", "--message", "--scale", "--since",
            "--until", "--timeout", "--username", "--subnet", "--gateway", "--expose", "--health-cmd",
            "--profile", "--env-file", "--log-driver", "--device", "--cap-add", "--cap-drop",
            // segunda leva (ExtraDocker): opções que recebem valor
            "--alias", "--log-opt", "--pids-limit", "--secret", "--docker", "--wait-timeout"
        };

        static readonly Dictionary<string, string> FlagAlias = new Dictionary<string, string> {
            { "--detach", "-d" }, { "--interactive", "-i" }, { "--tty", "-t" }, { "--tag", "-t" },
            { "--publish", "-p" }, { "--publish-all", "-P" }, { "--volume", "-v" }, { "--volumes", "-v" },
            { "--env", "-e" }, { "--all", "-a" }, { "--force", "-f" }, { "--follow", "-f" },
            { "--file", "-f" }, { "--filter", "-f" }, { "--quiet", "-q" }, { "--tail", "-n" },
            { "--workdir", "-w" }, { "--user", "-u" }, { "--memory", "-m" }, { "--output", "-o" },
            { "--input", "-i" }, { "--net", "--network" }, { "--timeout", "-t" }, { "--username", "-u" },
            { "--message", "-m" }, { "--author", "-a" }, { "--project-name", "-p" },
            { "--timestamps", "-t" }, { "--no-tty", "-T" }
        };

        static readonly HashSet<string> ContainerCmds = new HashSet<string> {
            "run", "rm", "start", "stop", "restart", "logs", "exec", "inspect", "stats", "cp", "commit",
            "kill", "create", "top", "rename", "pause", "unpause", "attach", "port", "diff", "export",
            "wait", "update"
        };

        static readonly HashSet<string> ImageCmds = new HashSet<string> {
            "pull", "push", "build", "tag", "history", "save", "load", "inspect", "import"
        };

        public static List<string> Tokenize(string s)
        {
            var list = new List<string>();
            var sb = new StringBuilder();
            bool inS = false, inD = false, has = false;
            foreach (char c in s)
            {
                if (inS) { if (c == '\'') inS = false; else sb.Append(c); continue; }
                if (inD) { if (c == '"') inD = false; else sb.Append(c); continue; }
                if (c == '\'') { inS = true; has = true; }
                else if (c == '"') { inD = true; has = true; }
                else if (char.IsWhiteSpace(c))
                {
                    if (sb.Length > 0 || has) { list.Add(sb.ToString()); sb.Clear(); has = false; }
                }
                else sb.Append(c);
            }
            if (sb.Length > 0 || has) list.Add(sb.ToString());
            return list;
        }

        static bool IsNumber(string t)
        {
            double d;
            return double.TryParse(t, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out d);
        }

        static bool LongTakesValue(string name) { return LongWithValue.Contains(name); }

        // Algumas letras mudam de significado conforme o subcomando (ex.: -t no build = tag, no run = tty).
        static bool ShortTakesValue(string f, List<string> raw)
        {
            string last = raw.Count > 0 ? raw[raw.Count - 1] : "";
            switch (f)
            {
                case "-t": return raw.Contains("build") || last == "stop" || last == "restart";
                case "-f":
                    if (raw.Contains("build")) return true;
                    if (raw.Count == 1 && raw[0] == "compose") return true;
                    return last == "ps" || last == "images" || last == "ls" || last == "events";
                case "-v": return raw.Contains("run") || raw.Contains("create");
                case "-i": return last == "load";
                case "-a": return last == "commit";
                case "-m": case "-e": case "-n": case "-o": case "-w": case "-u": case "-p": case "-l": case "-c":
                    return true;
                default: return false;
            }
        }

        static bool StopsAtFirstArg(List<string> raw)
        {
            if (raw.Count == 0) return false;
            string last = raw[raw.Count - 1];
            // "docker context create" / "docker buildx create" não rodam nada dentro de um container
            if (raw[0] == "context" || raw[0] == "buildx") return false;
            return last == "run" || last == "exec" || last == "create";
        }

        static List<string> NormalizePath(List<string> raw)
        {
            if (raw.Count >= 2)
            {
                string g = raw[0], c = raw[1];
                if (g == "container")
                {
                    if (c == "ls" || c == "list" || c == "ps") return new List<string> { "ps" };
                    if (c == "remove") return new List<string> { "rm" };
                    if (ContainerCmds.Contains(c)) return new List<string> { c };
                }
                if (g == "image")
                {
                    if (c == "ls" || c == "list") return new List<string> { "images" };
                    if (c == "rm" || c == "remove") return new List<string> { "rmi" };
                    if (ImageCmds.Contains(c)) return new List<string> { c };
                }
                if ((g == "builder" || g == "buildx") && c == "build") return new List<string> { "build" };
                if ((g == "volume" || g == "network") && c == "list") return new List<string> { g, "ls" };
                if ((g == "volume" || g == "network") && c == "remove") return new List<string> { g, "rm" };
            }
            return new List<string>(raw);
        }

        static string NormVal(string v)
        {
            v = v.Trim().ToLowerInvariant();
            if (v.EndsWith(":latest")) v = v.Substring(0, v.Length - 7);
            if (v.Length > 1 && v.EndsWith("/")) v = v.TrimEnd('/');
            if (v == "") v = ".";
            return v;
        }

        public static ParsedCommand Parse(string input)
        {
            var pc = new ParsedCommand();
            var t = Tokenize((input ?? "").Trim());
            if (t.Count == 0) { pc.Error = "Digite um comando."; return pc; }
            if (t[0].ToLowerInvariant() == "sudo") t.RemoveAt(0);
            if (t.Count > 0 && t[0].ToLowerInvariant() == "docker-compose") { t[0] = "docker"; t.Insert(1, "compose"); }
            if (t.Count == 0 || t[0].ToLowerInvariant() != "docker")
            {
                pc.Error = "Todo comando deste jogo começa com 'docker'.";
                return pc;
            }

            var raw = new List<string>();
            bool pathDone = false, literal = false;
            for (int i = 1; i < t.Count; i++)
            {
                string tok = t[i];
                if (literal) { pc.Args.Add(tok); continue; }
                if (tok == "--") { literal = true; continue; }

                if (tok.StartsWith("--") && tok.Length > 2)
                {
                    string name = tok, val = null;
                    int eq = tok.IndexOf('=');
                    if (eq > 0) { name = tok.Substring(0, eq); val = tok.Substring(eq + 1); }
                    name = name.ToLowerInvariant();
                    if (val == null && LongTakesValue(name) && i + 1 < t.Count) val = t[++i];
                    pc.Flags.Add(new KeyValuePair<string, string>(name, val));
                }
                else if (tok.StartsWith("-") && tok.Length > 1 && !IsNumber(tok))
                {
                    for (int j = 1; j < tok.Length; j++)
                    {
                        string f = "-" + tok[j];
                        if (ShortTakesValue(f, raw))
                        {
                            string val = j + 1 < tok.Length ? tok.Substring(j + 1) : (i + 1 < t.Count ? t[++i] : null);
                            if (val != null && val.StartsWith("=")) val = val.Substring(1);
                            pc.Flags.Add(new KeyValuePair<string, string>(f, val));
                            break;
                        }
                        pc.Flags.Add(new KeyValuePair<string, string>(f, null));
                    }
                }
                else if (!pathDone)
                {
                    string low = tok.ToLowerInvariant();
                    raw.Add(low);
                    if (!(raw.Count == 1 && Groups.Contains(low))) pathDone = true;
                }
                else
                {
                    pc.Args.Add(tok);
                    if (pc.Args.Count == 1 && StopsAtFirstArg(raw)) literal = true;
                }
            }

            pc.Path = NormalizePath(raw);
            var flags = new List<KeyValuePair<string, string>>();
            foreach (var f in pc.Flags)
            {
                string key = f.Key;
                string alias;
                if (FlagAlias.TryGetValue(key, out alias)) key = alias;
                if (pc.Path.Count == 0 && key == "-v") key = "--version";
                flags.Add(new KeyValuePair<string, string>(key, f.Value == null ? null : NormVal(f.Value)));
            }
            pc.Flags = flags;
            pc.Args = pc.Args.Select(NormVal).ToList();
            return pc;
        }

        // ───────────── destaque de sintaxe e auto-completar (usados pelo terminal) ─────────────

        public enum TokKind { Docker, Path, Flag, Value, Arg, Inner }

        public class HTok
        {
            public int Start, Length;
            public TokKind Kind;
        }

        static List<KeyValuePair<int, string>> RawTokens(string s)
        {
            var list = new List<KeyValuePair<int, string>>();
            int i = 0;
            while (i < s.Length)
            {
                while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
                if (i >= s.Length) break;
                int st = i;
                char q = '\0';
                while (i < s.Length && (q != '\0' || !char.IsWhiteSpace(s[i])))
                {
                    if (q == '\0' && (s[i] == '"' || s[i] == '\'')) q = s[i];
                    else if (q != '\0' && s[i] == q) q = '\0';
                    i++;
                }
                list.Add(new KeyValuePair<int, string>(st, s.Substring(st, i - st)));
            }
            return list;
        }

        public static List<HTok> Highlight(string s)
        {
            var res = new List<HTok>();
            var toks = RawTokens(s ?? "");
            var raw = new List<string>();
            bool pathDone = false, literal = false, wantValue = false, started = false;
            int args = 0;
            Action<int, int, TokKind> add = (st, len, k) => { if (len > 0) res.Add(new HTok { Start = st, Length = len, Kind = k }); };
            foreach (var kv in toks)
            {
                int st = kv.Key;
                string tok = kv.Value, low = tok.ToLowerInvariant().Trim('"', '\'');
                if (!started)
                {
                    if (low == "sudo") { add(st, tok.Length, TokKind.Docker); continue; }
                    started = true;
                    if (low == "docker") { add(st, tok.Length, TokKind.Docker); continue; }
                    if (low == "docker-compose") { add(st, tok.Length, TokKind.Docker); raw.Add("compose"); continue; }
                    add(st, tok.Length, TokKind.Arg);
                    continue;
                }
                if (literal) { add(st, tok.Length, TokKind.Inner); continue; }
                if (wantValue) { add(st, tok.Length, TokKind.Value); wantValue = false; continue; }
                if (tok == "--") { add(st, 2, TokKind.Flag); literal = true; continue; }
                if (tok.StartsWith("--") && tok.Length > 2)
                {
                    int eq = tok.IndexOf('=');
                    if (eq > 0) { add(st, eq, TokKind.Flag); add(st + eq + 1, tok.Length - eq - 1, TokKind.Value); }
                    else { add(st, tok.Length, TokKind.Flag); wantValue = LongTakesValue(low); }
                    continue;
                }
                if (tok.StartsWith("-") && tok.Length > 1 && !IsNumber(tok))
                {
                    int j = 1;
                    for (; j < tok.Length; j++)
                        if (ShortTakesValue("-" + tok[j], raw))
                        {
                            if (j + 1 < tok.Length) { add(st, j + 1, TokKind.Flag); add(st + j + 1, tok.Length - j - 1, TokKind.Value); }
                            else { add(st, tok.Length, TokKind.Flag); wantValue = true; }
                            break;
                        }
                    if (j >= tok.Length) add(st, tok.Length, TokKind.Flag);
                    continue;
                }
                if (!pathDone)
                {
                    raw.Add(low);
                    if (!(raw.Count == 1 && Groups.Contains(low))) pathDone = true;
                    add(st, tok.Length, TokKind.Path);
                    continue;
                }
                args++;
                add(st, tok.Length, TokKind.Arg);
                if (args == 1 && StopsAtFirstArg(raw)) literal = true;
            }
            return res;
        }

        static readonly Dictionary<string, string[]> GroupSubs = new Dictionary<string, string[]>
        {
            { "compose", new[] { "build", "config", "down", "exec", "logs", "ps", "pull", "restart", "run", "start", "stop", "up" } },
            { "volume", new[] { "create", "inspect", "ls", "prune", "rm" } },
            { "network", new[] { "connect", "create", "disconnect", "inspect", "ls", "prune", "rm" } },
            { "system", new[] { "df", "events", "info", "prune" } },
            { "container", new[] { "attach", "commit", "cp", "create", "exec", "inspect", "kill", "logs", "ls", "pause", "port", "prune", "rename", "restart", "rm", "run", "start", "stats", "stop", "top", "unpause" } },
            { "image", new[] { "build", "history", "inspect", "load", "ls", "prune", "pull", "push", "rm", "save", "tag" } },
            { "builder", new[] { "build", "prune" } },
            { "buildx", new[] { "build", "create", "ls", "prune", "use" } },
            { "context", new[] { "create", "inspect", "ls", "rm", "use" } },
        };

        // Sugestões para a palavra sendo digitada (apenas nomes de comandos/subcomandos).
        public static List<string> Complete(string input, out int start)
        {
            start = input.Length;
            var toks = RawTokens(input);
            bool endsSpace = input.Length > 0 && char.IsWhiteSpace(input[input.Length - 1]);
            string partial = "";
            var before = toks.Select(t => t.Value.ToLowerInvariant()).ToList();
            if (!endsSpace && toks.Count > 0)
            {
                partial = toks[toks.Count - 1].Value.ToLowerInvariant();
                start = toks[toks.Count - 1].Key;
                before.RemoveAt(before.Count - 1);
            }
            IEnumerable<string> cands;
            if (before.Count == 0) cands = new[] { "docker" };
            else if (before[0] != "docker" && before[0] != "sudo") return new List<string>();
            else
            {
                var rest = before.Where(b => b != "sudo" && b != "docker").ToList();
                if (rest.Count == 0) cands = KnownTop.OrderBy(x => x);
                else if (rest.Count == 1 && GroupSubs.ContainsKey(rest[0])) cands = GroupSubs[rest[0]];
                else return new List<string>();
            }
            if (partial.StartsWith("-")) return new List<string>();
            return cands.Where(c => c.StartsWith(partial)).ToList();
        }

        public static bool IsKnownCommand(string word) { return KnownTop.Contains(word); }

        static bool Same(ParsedCommand a, ParsedCommand b, HashSet<string> ignore)
        {
            return a.Path.SequenceEqual(b.Path)
                && a.FlagStrings(ignore).SequenceEqual(b.FlagStrings(ignore))
                && a.Args.SequenceEqual(b.Args);
        }

        public static MatchResult Check(string input, Level lv)
        {
            var p = Parse(input);
            if (p.Error != null) return new MatchResult { Ok = false, Message = p.Error };

            var ignore = new HashSet<string>(lv.Ignore ?? new string[0]);
            var cands = lv.Answers.Select(Parse).ToList();
            if (cands.Any(c => Same(p, c, ignore))) return new MatchResult { Ok = true };

            var samePath = cands.Where(c => c.Path.SequenceEqual(p.Path)).ToList();
            if (samePath.Count == 0)
            {
                string msg;
                if (p.Path.Count == 0 && p.Flags.Count == 0)
                    msg = "Você digitou só 'docker'. Falta o comando que executa a ação.";
                else if (p.Path.Count > 0 && !KnownTop.Contains(p.Path[0]))
                    msg = "'docker " + p.Path[0] + "' não é um comando do Docker. Confira a grafia ou pesquise na documentação.";
                else if (p.Path.Count == 1 && Groups.Contains(p.Path[0]))
                    msg = "'docker " + p.Path[0] + "' precisa de um subcomando (ex.: docker " + p.Path[0] + " <ação>).";
                else
                    msg = "Esse comando existe, mas não é o adequado para esta tarefa. Pense em qual ação o Docker precisa executar.";
                return new MatchResult { Ok = false, Message = msg };
            }

            // Escolhe a resposta aceita mais próxima para dar um retorno útil.
            ParsedCommand best = null;
            int bestScore = int.MaxValue;
            List<string> bMissing = null, bExtra = null, bWrongVal = null;
            foreach (var c in samePath)
            {
                var exp = c.Flags.Where(f => !ignore.Contains(f.Key)).ToList();
                var got = p.Flags.Where(f => !ignore.Contains(f.Key)).ToList();
                var expKeys = exp.Select(f => f.Key).Distinct().ToList();
                var gotKeys = got.Select(f => f.Key).Distinct().ToList();
                var missing = expKeys.Where(k => !gotKeys.Contains(k)).ToList();
                var extra = gotKeys.Where(k => !expKeys.Contains(k)).ToList();
                var wrongVal = expKeys.Where(k => gotKeys.Contains(k) &&
                    !exp.Where(f => f.Key == k).Select(f => f.Value ?? "").OrderBy(x => x)
                        .SequenceEqual(got.Where(f => f.Key == k).Select(f => f.Value ?? "").OrderBy(x => x))).ToList();
                int argDiff = c.Args.SequenceEqual(p.Args) ? 0 : 1;
                int score = missing.Count * 3 + extra.Count * 3 + wrongVal.Count * 2 + argDiff;
                if (score < bestScore)
                {
                    bestScore = score; best = c;
                    bMissing = missing; bExtra = extra; bWrongVal = wrongVal;
                }
            }

            var parts = new List<string>();
            parts.Add("Comando certo (docker " + string.Join(" ", p.Path) + ")!");
            bool flagsOk = bMissing.Count == 0 && bExtra.Count == 0 && bWrongVal.Count == 0;
            if (bMissing.Count > 0)
                parts.Add(bMissing.Count == 1 ? "Mas falta 1 opção/flag." : "Mas faltam " + bMissing.Count + " opções/flags.");
            if (bExtra.Count > 0)
                parts.Add("Opção desnecessária ou incorreta aqui: " + string.Join(", ", bExtra) + ".");
            if (bWrongVal.Count > 0)
                parts.Add("Valor incorreto para: " + string.Join(", ", bWrongVal) + ".");
            if (flagsOk)
            {
                if (best.Args.Count != p.Args.Count)
                    parts.Add("As opções estão certas, mas a quantidade de argumentos (imagem, nome, caminho, comando...) está diferente do esperado.");
                else
                    parts.Add("As opções estão certas! Confira os argumentos: nome do container/imagem, tag, caminho ou comando.");
            }
            return new MatchResult { Ok = false, Message = string.Join(" ", parts) };
        }
    }
}
