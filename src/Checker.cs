using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DockerQuest
{
    // Verifica respostas de qualquer categoria. Docker usa o parser especializado;
    // as demais usam normalização (espaços, aspas, maiúsculas, ponto e vírgula).
    static class Checker
    {
        public static MatchResult Check(string input, Level lv)
        {
            if (lv.Kind == "docker") return CommandParser.Check(input, lv);
            if (lv.Kind == "sql") return Fail("Este desafio é feito no terminal MySQL, que executa o comando no banco e confere o resultado.");
            if (lv.Kind == "aws") return Fail("Este desafio é feito no Console da AWS (ou no CloudShell), que avalia o que você fez na conta.");
            string a = (input ?? "").Trim();
            if (a.Length == 0) return Fail("Digite uma resposta.");
            switch (lv.Kind)
            {
                case "cmd": return CheckCmd(a, lv);
                case "text": return CheckText(a, lv);
                default: return CheckCode(a, lv);
            }
        }

        static MatchResult Fail(string m) { return new MatchResult { Ok = false, Message = m }; }
        static MatchResult Ok() { return new MatchResult { Ok = true }; }

        public static string Fold(string s)
        {
            var d = s.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in d) if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            return sb.ToString();
        }

        // ── respostas curtas
        static string NormText(string s) { return new string(Fold(s).Where(c => !char.IsWhiteSpace(c)).ToArray()).TrimEnd('.', '!', '?'); }

        static MatchResult CheckText(string a, Level lv)
        {
            string n = NormText(a);
            if (lv.Answers.Any(x => NormText(x) == n)) return Ok();
            return Fail("Resposta incorreta. Releia a pergunta com calma — e, se precisar, pesquise na Biblioteca (seção " + Challenges.Cat(lv.Cat).Name + ").");
        }

        // ── comandos de terminal
        static List<string> Toks(string s) { return CommandParser.Tokenize(s).Select(t => ShortFlags(t.ToLowerInvariant().TrimEnd('/'))).ToList(); }

        // "-na" e "-an" são a mesma coisa: ordena as letras de opções curtas agrupadas
        static string ShortFlags(string t)
        {
            if (t.Length < 3 || t[0] != '-' || t[1] == '-' || !t.Skip(1).All(char.IsLetter)) return t;
            var l = t.Substring(1).ToCharArray();
            Array.Sort(l);
            return "-" + new string(l);
        }

        // cabeça (antes da primeira opção) + conjunto de grupos "opção valores..."
        static string Groups(List<string> t)
        {
            var head = new List<string>();
            var groups = new List<string>();
            int i = 0;
            while (i < t.Count && !t[i].StartsWith("-")) head.Add(t[i++]);
            while (i < t.Count)
            {
                var g = new List<string> { t[i++] };
                while (i < t.Count && !t[i].StartsWith("-")) g.Add(t[i++]);
                groups.Add(string.Join(" ", g));
            }
            groups.Sort(StringComparer.Ordinal);
            return string.Join(" ", head) + " | " + string.Join(" ; ", groups);
        }

        static MatchResult CheckCmd(string a, Level lv)
        {
            var t = Toks(a);
            string seq = string.Join(" ", t), grp = Groups(t);
            foreach (var ans in lv.Answers)
            {
                var at = Toks(ans);
                if (string.Join(" ", at) == seq || Groups(at) == grp) return Ok();
            }
            var first = lv.Answers.Select(x => Toks(x)).ToList();
            if (!first.Any(f => f.Count > 0 && t.Count > 0 && f[0] == t[0]))
                return Fail("O comando começa diferente do esperado. Qual programa/ferramenta resolve isso? Pesquise na Biblioteca (seção " + Challenges.Cat(lv.Cat).Name + ").");
            if (first.Any(f => f.Count > 1 && t.Count > 1 && f[1] == t[1]))
                return Fail("Começo certo! Confira o resto: subcomando, opções e valores (nomes, caminhos, números).");
            return Fail("A ferramenta está certa, mas o subcomando/ação não é esse. Pesquise as ações disponíveis.");
        }

        // ── linhas de código / HTML
        public static string NormCode(string s)
        {
            s = s.Trim().Replace('\'', '"').Replace("“", "\"").Replace("”", "\"").ToLowerInvariant();
            var sb = new StringBuilder();
            bool pend = false;
            foreach (char c in s)
            {
                if (char.IsWhiteSpace(c)) { pend = true; continue; }
                if (c == ';') continue;
                if (pend && sb.Length > 0 && IsWord(sb[sb.Length - 1]) && IsWord(c)) sb.Append(' ');
                pend = false;
                sb.Append(c);
            }
            return sb.ToString().Replace("/>", ">").TrimEnd(',');
        }

        static bool IsWord(char c) { return char.IsLetterOrDigit(c) || c == '_' || c == '$' || c == '"'; }

        static MatchResult CheckCode(string a, Level lv)
        {
            string n = NormCode(a);
            double best = 0;
            foreach (var ans in lv.Answers)
            {
                string x = NormCode(ans);
                if (x == n) return Ok();
                best = Math.Max(best, Similarity(x, n));
            }
            if (best >= 0.88) return Fail("Muito perto! Falta um detalhe: confira aspas, parênteses, chaves, nomes e símbolos.");
            if (best >= 0.6) return Fail("Você está no caminho, mas algo importante está diferente. Releia o objetivo: nomes e valores precisam ser exatamente os pedidos.");
            return Fail("Ainda não. Releia o objetivo e pesquise a sintaxe na Biblioteca (seção " + Challenges.Cat(lv.Cat).Name + ").");
        }

        static double Similarity(string a, string b)
        {
            if (a.Length == 0 && b.Length == 0) return 1;
            var d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) d[0, j] = j;
            for (int i = 1; i <= a.Length; i++)
                for (int j = 1; j <= b.Length; j++)
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
            return 1.0 - (double)d[a.Length, b.Length] / Math.Max(a.Length, b.Length);
        }
    }
}
