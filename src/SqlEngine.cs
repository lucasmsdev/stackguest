using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DockerQuest
{
    // Mini banco de dados SQL (dialeto MySQL) para o terminal da área de SQL.
    // Executa de verdade: SELECT (JOIN, GROUP BY, HAVING, subconsultas, ORDER BY, LIMIT), INSERT, UPDATE, DELETE,
    // CREATE/DROP TABLE, CREATE INDEX, ALTER TABLE, SHOW, DESCRIBE, EXPLAIN e transações.
    // As mensagens de erro imitam as do MySQL e trazem uma explicação em português (Hint).

    class SqlError : Exception
    {
        public int Code;
        public string State, Hint;
        public SqlError(int code, string state, string msg, string hint) : base(msg) { Code = code; State = state; Hint = hint; }
        public string Mysql { get { return "ERROR " + Code + " (" + State + "): " + Message; } }
    }

    class SqlCol
    {
        public string Name, Type = "INT", RefTable, RefCol;
        public string OnDelete, FkName;      // ação da chave estrangeira ao apagar o "pai" (CASCADE, SET NULL, RESTRICT...) e nome da restrição
        public string InlineRef;             // "REFERENCES t(c)" escrito na própria coluna: o MySQL 8 aceita, mas ignora
        public Ex CheckEx;                   // CHECK declarado na própria coluna (vai para a tabela no CREATE)
        public int Len = -1, Scale = -1;
        public bool Pk, Auto, NotNull, Unique;
        public object Default;
        public SqlCol Copy() { return (SqlCol)MemberwiseClone(); }
        public bool IsInt { get { return Type == "INT" || Type == "BIGINT" || Type == "SMALLINT" || Type == "TINYINT" || Type == "BOOLEAN"; } }
        public bool IsNum { get { return IsInt || Type == "DECIMAL" || Type == "FLOAT" || Type == "DOUBLE"; } }
        public string TypeText
        {
            get
            {
                string t = Type.ToLowerInvariant();
                if (Type == "BOOLEAN") return "tinyint(1)";
                if (Type == "DECIMAL") return "decimal(" + (Len > 0 ? Len : 10) + "," + Math.Max(0, Scale) + ")";
                if ((Type == "VARCHAR" || Type == "CHAR") && Len > 0) return t + "(" + Len + ")";
                return t;
            }
        }
    }

    class SqlIndex
    {
        public string Name;
        public List<string> Cols = new List<string>();
        public bool Unique;
    }

    class SqlTable
    {
        public string Name;
        public List<SqlCol> Cols = new List<SqlCol>();
        public List<object[]> Rows = new List<object[]>();
        public List<SqlIndex> Indexes = new List<SqlIndex>();
        public List<KeyValuePair<string, Ex>> Checks = new List<KeyValuePair<string, Ex>>();   // restrições CHECK (nome, condição)
        public int NextId = 1;

        public int ColIndex(string n) { return Cols.FindIndex(c => string.Equals(c.Name, n, StringComparison.OrdinalIgnoreCase)); }

        public SqlTable Clone()
        {
            var t = new SqlTable { Name = Name, NextId = NextId };
            t.Cols = Cols.Select(c => c.Copy()).ToList();
            t.Rows = Rows.Select(r => (object[])r.Clone()).ToList();
            t.Indexes = Indexes.Select(i => new SqlIndex { Name = i.Name, Unique = i.Unique, Cols = new List<string>(i.Cols) }).ToList();
            t.Checks = new List<KeyValuePair<string, Ex>>(Checks);
            return t;
        }
    }

    // VIEW: uma consulta guardada com nome (é executada de novo toda vez que alguém lê a view)
    class SqlView
    {
        public string Name, Sql;
        public List<string> Cols;            // nomes de colunas opcionais: CREATE VIEW v (a, b) AS ...
    }

    // valor de INTERVAL n UNIDADE (DATE_ADD, DATE_SUB, data + INTERVAL ...)
    class SqlInterval
    {
        public decimal N;
        public string Unit;
        public override string ToString() { return "INTERVAL " + N.ToString(CultureInfo.InvariantCulture) + " " + Unit; }
    }

    class SqlResult
    {
        public string Kind;                  // select, insert, update, delete, create, drop, alter, index, show, use, tx, explain
        public List<string> Headers;
        public List<object[]> Rows;
        public int Affected, Matched;
        public bool NoWhere, Modified, Star, Simple;   // Simple: SELECT * FROM tabela sem cláusulas (exploração)
        public string Message, Table;
        public string Warn;                  // erro clássico detectado (JOIN sem ON, = NULL...): explica o resultado estranho
        public string Tx;                    // begin, commit, rollback
    }

    // ───────────────────────────── tokens ─────────────────────────────
    enum SqlTT { Word, Quoted, Num, Str, Sym, End }

    class SqlTok
    {
        public SqlTT T;
        public string S;
        public int Pos, EndPos;
        public string U { get { return T == SqlTT.Word ? S.ToUpperInvariant() : S; } }
    }

    // ───────────────────────────── árvore de expressões ─────────────────────────────
    abstract class Ex { public string Text; }
    class ELit : Ex { public object V; }
    class ECol : Ex { public string Tab, Name; }
    class EStar : Ex { public string Tab; }
    class EBin : Ex { public string Op; public Ex L, R; }
    class EUn : Ex { public string Op; public Ex E; }
    class EIsNull : Ex { public Ex E; public bool Not; }
    class ELike : Ex { public Ex E, Pat; public bool Not; }
    class EIn : Ex { public Ex E; public List<Ex> List; public SSelect Sub; public bool Not; }
    class EBetween : Ex { public Ex E, Lo, Hi; public bool Not; }
    class EFunc : Ex { public string Name; public List<Ex> Args = new List<Ex>(); public bool Star, Distinct; }
    class ESub : Ex { public SSelect Q; }
    class EExists : Ex { public SSelect Q; public bool Not; }
    class ECase : Ex { public Ex Base, Else; public List<Ex> Whens = new List<Ex>(), Thens = new List<Ex>(); }

    class SItem { public Ex E; public string Alias; }
    class SJoin { public string Kind, Table, Alias; public Ex On; }
    class SSelect
    {
        public bool Distinct;
        public List<SItem> Items = new List<SItem>();
        public string From, FromAlias;
        public List<SJoin> Joins = new List<SJoin>();
        public Ex Where, Having;
        public List<Ex> GroupBy = new List<Ex>();
        public List<KeyValuePair<Ex, bool>> Order = new List<KeyValuePair<Ex, bool>>();   // expressão, descendente?
        public int Limit = -1, Offset;
        public bool Paren;                                 // veio entre parênteses
        public List<SSelect> Union;                        // UNION: as partes (este objeto guarda só o ORDER BY / LIMIT do conjunto)
        public List<bool> UnionAll;                        // para cada parte depois da primeira: UNION ALL?
    }

    class SqlSrc { public string Alias; public SqlTable T; public object[] Row; }

    class SqlCtx
    {
        public List<SqlSrc> B;                 // linha atual (uma fonte por tabela do FROM/JOIN)
        public List<List<SqlSrc>> G;           // grupo atual (agregações)
        public List<SqlSrc> Schema;            // tabelas visíveis (para resolver colunas mesmo sem linhas)
        public Dictionary<string, object> Alias;   // apelidos do SELECT (ORDER BY / HAVING)
        public SqlCtx Outer;                   // consulta externa (subconsultas correlacionadas)
    }

    // ───────────────────────────── banco ─────────────────────────────
    class SqlDb
    {
        public const string DbName = "cafe365";
        public Dictionary<string, SqlTable> Tables = new Dictionary<string, SqlTable>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, SqlView> Views = new Dictionary<string, SqlView>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, SqlTable> txSnap;
        Dictionary<string, SqlTable> lastTxStart;   // como o banco estava antes da última transação confirmada (COMMIT)
        int viewDepth;
        public static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        static readonly CompareInfo Cmp = CultureInfo.InvariantCulture.CompareInfo;
        const CompareOptions CI = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

        public SqlDb Clone()
        {
            var d = new SqlDb();
            foreach (var kv in Tables) d.Tables[kv.Key] = kv.Value.Clone();
            foreach (var kv in Views) d.Views[kv.Key] = new SqlView { Name = kv.Value.Name, Sql = kv.Value.Sql, Cols = kv.Value.Cols == null ? null : new List<string>(kv.Value.Cols) };
            d.txSnap = CloneTables(txSnap);
            d.lastTxStart = CloneTables(lastTxStart);
            return d;
        }

        static Dictionary<string, SqlTable> CloneTables(Dictionary<string, SqlTable> src)
        {
            if (src == null) return null;
            var s = new Dictionary<string, SqlTable>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in src) s[kv.Key] = kv.Value.Clone();
            return s;
        }

        Dictionary<string, SqlTable> Snapshot() { return CloneTables(Tables); }

        public bool InTransaction { get { return txSnap != null; } }

        // desfaz a última transação confirmada (usado pelo treino quando o COMMIT não deixou o banco como o pedido)
        public bool UndoLastCommit()
        {
            if (lastTxStart == null) return false;
            Tables = lastTxStart; lastTxStart = null; txSnap = null;
            return true;
        }

        // ── erros no estilo do MySQL, com explicação
        static SqlError Syntax(string src, int pos)
        {
            string near = pos >= 0 && pos < src.Length ? src.Substring(pos) : "";
            if (near.Length > 60) near = near.Substring(0, 60);
            return new SqlError(1064, "42000", "You have an error in your SQL syntax; check the manual that corresponds to your MySQL server version for the right syntax to use near '" + near + "' at line 1",
                near.Length > 0 ? "Erro de sintaxe perto de: " + near.Trim() + "  —  confira vírgulas, aspas, parênteses, nomes e a ordem das cláusulas (SELECT … FROM … WHERE … GROUP BY … HAVING … ORDER BY … LIMIT)."
                                : "O comando terminou antes da hora. Faltou alguma parte (um valor, um parêntese, o nome da tabela...)?");
        }

        static SqlError NoTable(string t)
        {
            return new SqlError(1146, "42S02", "Table '" + DbName + "." + t + "' doesn't exist", "A tabela '" + t + "' não existe. Veja as tabelas com SHOW TABLES;");
        }

        static SqlError UnknownCol(string c, string where)
        {
            return new SqlError(1054, "42S22", "Unknown column '" + c + "' in '" + where + "'", "A coluna '" + c + "' não existe aqui. Confira o nome com DESCRIBE nome_da_tabela;");
        }

        public SqlTable Table(string name)
        {
            SqlTable t;
            if (!Tables.TryGetValue(name, out t))
            {
                if (Views.ContainsKey(name))
                    throw new SqlError(1288, "HY000", "The target table " + name + " of the statement is not updatable",
                        "'" + name + "' é uma VIEW (uma consulta guardada), não uma tabela. Para mudar dados ou estrutura, use a tabela de origem.");
                throw NoTable(name);
            }
            return t;
        }

        // fonte de dados do FROM/JOIN: uma tabela ou uma VIEW (que é executada na hora e vira uma tabela temporária)
        SqlTable Source(string name)
        {
            SqlTable t;
            if (Tables.TryGetValue(name, out t)) return t;
            SqlView v;
            if (!Views.TryGetValue(name, out v)) throw NoTable(name);
            if (viewDepth > 8) throw new SqlError(1462, "HY000", "`" + DbName + "`.`" + name + "` contains view recursion", "A view usa a si mesma (direta ou indiretamente).");
            SqlResult r;
            viewDepth++;
            try
            {
                var p = new Parser(v.Sql, Lex(v.Sql));
                r = Select(p.ParseSelect(), null, p);
            }
            finally { viewDepth--; }
            return Materialize(v.Name, r, v.Cols);
        }

        // transforma o resultado de um SELECT numa tabela (VIEW e CREATE TABLE ... AS SELECT)
        static SqlTable Materialize(string name, SqlResult r, List<string> names)
        {
            var t = new SqlTable { Name = name };
            for (int k = 0; k < r.Headers.Count; k++)
            {
                var vals = r.Rows.Select(x => x[k]).Where(x => x != null).ToList();
                var c = new SqlCol { Name = names != null && k < names.Count ? names[k] : r.Headers[k] };
                if (vals.Count > 0 && vals.All(x => x is decimal))
                {
                    int scale = vals.Max(x => (int)((decimal.GetBits((decimal)x)[3] >> 16) & 0xFF));
                    if (scale == 0) c.Type = "INT"; else { c.Type = "DECIMAL"; c.Len = 10; c.Scale = scale; }
                }
                else if (vals.Count > 0 && vals.All(x => Regex.IsMatch(Str(x), @"^\d{4}-\d{2}-\d{2}$"))) c.Type = "DATE";
                else if (vals.Count > 0 && vals.All(x => Regex.IsMatch(Str(x), @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$"))) c.Type = "DATETIME";
                else { c.Type = "VARCHAR"; c.Len = Math.Max(1, vals.Count == 0 ? 1 : vals.Max(x => Str(x).Length)); }
                t.Cols.Add(c);
            }
            foreach (var row in r.Rows) t.Rows.Add((object[])row.Clone());
            return t;
        }

        bool NameTaken(string n) { return Tables.ContainsKey(n) || Views.ContainsKey(n); }

        // ───────────────────────────── execução ─────────────────────────────
        public SqlResult Execute(string sql)
        {
            var toks = Lex(sql);
            var p = new Parser(sql, toks);
            if (p.AtEnd) throw new SqlError(1065, "42000", "Query was empty", "Comando vazio.");
            var snap = Snapshot();
            try
            {
                var r = Dispatch(p);
                p.AcceptSym(";");
                if (!p.AtEnd) throw Syntax(sql, p.Peek.Pos);
                return r;
            }
            catch (SqlError) { Tables = snap; throw; }
        }

        // divide um texto com vários comandos separados por ';' (respeitando aspas)
        public static List<string> Split(string text)
        {
            var res = new List<string>();
            var sb = new StringBuilder();
            char q = '\0';
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (q != '\0') { sb.Append(c); if (c == q) q = '\0'; continue; }
                if (c == '\'' || c == '"' || c == '`') { q = c; sb.Append(c); continue; }
                if (c == ';') { if (sb.ToString().Trim().Length > 0) res.Add(sb.ToString().Trim()); sb.Clear(); continue; }
                sb.Append(c);
            }
            if (sb.ToString().Trim().Length > 0) res.Add(sb.ToString().Trim());
            return res;
        }

        SqlResult Dispatch(Parser p)
        {
            string k = p.Peek.U;
            switch (k)
            {
                case "SELECT": case "(": return Select(p.ParseSelect(), null, p);
                case "INSERT": return Insert(p);
                case "UPDATE": return Update(p);
                case "DELETE": return Delete(p);
                // comandos de estrutura (DDL) confirmam sozinhos a transação aberta (COMMIT implícito), como no MySQL
                case "CREATE": ImplicitCommit(); return Create(p);
                case "DROP": ImplicitCommit(); return Drop(p);
                case "ALTER": ImplicitCommit(); return Alter(p);
                case "TRUNCATE": ImplicitCommit(); return Truncate(p);
                case "SHOW": return Show(p);
                case "DESCRIBE": case "DESC": p.Next(); return Describe(p.Ident());
                case "EXPLAIN": p.Next(); if (p.Peek.U == "SELECT" || p.IsSym("(")) return Explain(p.ParseSelect()); return Describe(p.Ident());
                case "USE":
                    {
                        p.Next();
                        string db = p.Ident();
                        if (!db.Equals(DbName, StringComparison.OrdinalIgnoreCase)) throw new SqlError(1049, "42000", "Unknown database '" + db + "'", "Neste servidor o banco de dados do Café 365 se chama " + DbName + ".");
                        return new SqlResult { Kind = "use", Message = "Database changed" };
                    }
                case "BEGIN": case "START":
                    p.Next(); if (k == "START") p.Expect("TRANSACTION"); else p.Accept("WORK");
                    ImplicitCommit();
                    txSnap = Snapshot();
                    return new SqlResult { Kind = "tx", Tx = "begin", Message = "Query OK, 0 rows affected (0.00 sec)" };
                case "COMMIT":
                    p.Next(); p.Accept("WORK");
                    lastTxStart = txSnap; txSnap = null;
                    return new SqlResult { Kind = "tx", Tx = "commit", Message = "Query OK, 0 rows affected (0.00 sec)" };
                case "ROLLBACK":
                    {
                        p.Next(); p.Accept("WORK");
                        bool had = txSnap != null;
                        if (had) { Tables = txSnap; txSnap = null; }
                        return new SqlResult { Kind = "tx", Tx = "rollback", Modified = had, Message = "Query OK, 0 rows affected (0.00 sec)" };
                    }
                default:
                    throw Syntax(p.Src, p.Peek.Pos);
            }
        }

        void ImplicitCommit() { if (txSnap != null) { lastTxStart = txSnap; txSnap = null; } }

        // ───────────────────────────── SELECT ─────────────────────────────
        static bool IsAgg(string n) { return n == "COUNT" || n == "SUM" || n == "AVG" || n == "MIN" || n == "MAX" || n == "GROUP_CONCAT"; }

        static void Walk(Ex e, Action<Ex> f)
        {
            if (e == null) return;
            f(e);
            var b = e as EBin; if (b != null) { Walk(b.L, f); Walk(b.R, f); return; }
            var u = e as EUn; if (u != null) { Walk(u.E, f); return; }
            var n = e as EIsNull; if (n != null) { Walk(n.E, f); return; }
            var l = e as ELike; if (l != null) { Walk(l.E, f); Walk(l.Pat, f); return; }
            var i = e as EIn; if (i != null) { Walk(i.E, f); if (i.List != null) foreach (var x in i.List) Walk(x, f); return; }
            var bt = e as EBetween; if (bt != null) { Walk(bt.E, f); Walk(bt.Lo, f); Walk(bt.Hi, f); return; }
            var fn = e as EFunc; if (fn != null) { foreach (var x in fn.Args) Walk(x, f); return; }
            var c = e as ECase; if (c != null) { Walk(c.Base, f); foreach (var x in c.Whens) Walk(x, f); foreach (var x in c.Thens) Walk(x, f); Walk(c.Else, f); }
        }

        static bool HasAgg(Ex e)
        {
            bool has = false;
            Walk(e, x => { var f = x as EFunc; if (f != null && IsAgg(f.Name)) has = true; });
            return has;
        }

        // procura a coluna nas tabelas visíveis: devolve (fonte, coluna) ou lança erro
        static bool Find(List<SqlSrc> schema, ECol c, out int si, out int ci, string clause)
        {
            si = -1; ci = -1;
            for (int s = 0; s < schema.Count; s++)
            {
                if (c.Tab != null && !schema[s].Alias.Equals(c.Tab, StringComparison.OrdinalIgnoreCase)) continue;
                int k = schema[s].T.ColIndex(c.Name);
                if (k < 0) continue;
                if (si >= 0)
                    throw new SqlError(1052, "23000", "Column '" + c.Name + "' in " + clause + " is ambiguous",
                        "Mais de uma tabela tem a coluna '" + c.Name + "'. Diga de qual é: tabela.coluna (ou apelido.coluna, ex.: p." + c.Name + ").");
                si = s; ci = k;
            }
            if (si < 0 && c.Tab != null && !schema.Any(s => s.Alias.Equals(c.Tab, StringComparison.OrdinalIgnoreCase)))
                return false;
            return si >= 0;
        }

        object ColValue(ECol c, SqlCtx ctx, string clause)
        {
            if (ctx.Alias != null && c.Tab == null)
            {
                object v;
                if (ctx.Alias.TryGetValue(c.Name, out v)) return v;
            }
            for (var x = ctx; x != null; x = x.Outer)
            {
                int si, ci;
                if (x.Schema != null && Find(x.Schema, c, out si, out ci, clause))
                {
                    if (x.B == null || x.B[si].Row == null) return null;
                    return x.B[si].Row[ci];
                }
            }
            throw UnknownCol(c.Tab != null ? c.Tab + "." + c.Name : c.Name, clause);
        }

        void Validate(Ex e, SqlCtx ctx, string clause)
        {
            Walk(e, x =>
            {
                var c = x as ECol;
                if (c == null) return;
                if (ctx.Alias != null && c.Tab == null && ctx.Alias.ContainsKey(c.Name)) return;
                for (var o = ctx; o != null; o = o.Outer) { int si, ci; if (o.Schema != null && Find(o.Schema, c, out si, out ci, clause)) return; }
                throw UnknownCol(c.Tab != null ? c.Tab + "." + c.Name : c.Name, clause);
            });
        }

        // UNION / UNION ALL: junta os resultados de vários SELECTs (mesma quantidade de colunas)
        SqlResult SelectUnion(SSelect q, SqlCtx outer)
        {
            var parts = q.Union.Select(s => Select(s, outer, null)).ToList();
            var headers = parts[0].Headers;
            for (int i = 1; i < parts.Count; i++)
                if (parts[i].Headers.Count != headers.Count)
                    throw new SqlError(1222, "21000", "The used SELECT statements have a different number of columns",
                        "Num UNION, todos os SELECTs precisam trazer a MESMA quantidade de colunas (e na mesma ordem de significado).");
            var rows = new List<object[]>(parts[0].Rows);
            string warn = parts.Select(x => x.Warn).FirstOrDefault(x => x != null);
            for (int i = 1; i < parts.Count; i++)
            {
                rows.AddRange(parts[i].Rows);
                if (!q.UnionAll[i - 1])
                {
                    var seen = new HashSet<string>();
                    rows = rows.Where(r => seen.Add(string.Join("\u0001", r.Select(Key)))).ToList();
                }
            }
            if (q.Order.Count > 0)
            {
                var keys = new List<object[]>();
                foreach (var r in rows)
                {
                    var al = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    for (int k = 0; k < headers.Count; k++) if (!al.ContainsKey(headers[k])) al[headers[k]] = r[k];
                    var ctx = new SqlCtx { Alias = al, Schema = new List<SqlSrc>(), Outer = outer };
                    var kv = new object[q.Order.Count];
                    for (int k = 0; k < q.Order.Count; k++)
                    {
                        var e = q.Order[k].Key;
                        var lit = e as ELit;
                        if (lit != null && lit.V is decimal)
                        {
                            int pos = (int)(decimal)lit.V;
                            if (pos < 1 || pos > headers.Count) throw UnknownCol(pos.ToString(), "order clause");
                            kv[k] = r[pos - 1];
                        }
                        else
                        {
                            var c = e as ECol;
                            if (c != null && (c.Tab != null || !al.ContainsKey(c.Name)))
                                throw new SqlError(1054, "42S22", "Unknown column '" + c.Text + "' in 'order clause'",
                                    "Depois de um UNION, o ORDER BY só enxerga as colunas do RESULTADO (pelo nome da primeira consulta ou pela posição: ORDER BY 1).");
                            kv[k] = Eval(e, ctx);
                        }
                    }
                    keys.Add(kv);
                }
                var desc = q.Order.Select(o => o.Value).ToArray();
                var idx = Enumerable.Range(0, rows.Count).OrderBy(i => i, Comparer<int>.Create((a, b) =>
                {
                    for (int k = 0; k < desc.Length; k++)
                    {
                        int c = CompareNullsFirst(keys[a][k], keys[b][k]);
                        if (c != 0) return desc[k] ? -c : c;
                    }
                    return 0;
                })).ToList();
                rows = idx.Select(i => rows[i]).ToList();
            }
            if (q.Offset > 0) rows = rows.Skip(q.Offset).ToList();
            if (q.Limit >= 0) rows = rows.Take(q.Limit).ToList();
            return new SqlResult { Kind = "select", Headers = new List<string>(headers), Rows = rows, Warn = warn };
        }

        // resolve a qual fonte (apelido) uma coluna pertence; null se não achar ou se for ambígua
        static int SourceOf(List<SqlSrc> schema, ECol c)
        {
            int si, ci;
            try { return Find(schema, c, out si, out ci, "on clause") ? si : -1; }
            catch (SqlError) { return -1; }
        }

        // erros clássicos que não dão erro de sintaxe, mas trazem um resultado errado
        static string LintNull(Ex e)
        {
            string w = null;
            Walk(e, x =>
            {
                var b = x as EBin;
                if (w != null || b == null || (b.Op != "=" && b.Op != "<>" && b.Op != "!=")) return;
                var l1 = b.L as ELit; var l2 = b.R as ELit;
                if ((l1 != null && l1.V == null && l1.Text == "NULL") || (l2 != null && l2.V == null && l2.Text == "NULL"))
                    w = "Atenção: '" + b.Text + "' nunca é verdadeiro — NULL não é igual (nem diferente) de nada. Para testar valor vazio use IS NULL (ou IS NOT NULL).";
            });
            return w;
        }

        string LintSelect(SSelect q, List<SqlSrc> schema)
        {
            for (int n = 0; n < q.Joins.Count; n++)
            {
                var j = q.Joins[n];
                int me = n + 1;
                if (j.On == null)
                {
                    bool linked = false;
                    Walk(q.Where, x =>
                    {
                        var b = x as EBin;
                        if (b == null || b.Op != "=" || !(b.L is ECol) || !(b.R is ECol)) return;
                        int a = SourceOf(schema, (ECol)b.L), c = SourceOf(schema, (ECol)b.R);
                        if (a >= 0 && c >= 0 && a != c && (a == me || c == me)) linked = true;
                    });
                    if (!linked)
                    {
                        int total = schema.Take(me + 1).Aggregate(1, (acc, s) => acc * Math.Max(1, s.T.Rows.Count));
                        return "Atenção: a tabela " + j.Table + " entrou na consulta SEM condição de ligação (faltou o ON). Cada linha foi combinada com TODAS as linhas da outra tabela " +
                               "(produto cartesiano: " + total + " combinações). Ligue as tabelas pela chave: JOIN " + j.Table + " " + (j.Alias ?? "x") + " ON chave_estrangeira = chave_primária.";
                    }
                    continue;
                }
                var eq = j.On as EBin;
                if (eq != null && eq.Op == "=" && eq.L is ECol && eq.R is ECol)
                {
                    int a = SourceOf(schema, (ECol)eq.L), c = SourceOf(schema, (ECol)eq.R);
                    if (a >= 0 && a == c)
                        return "Atenção: o ON do JOIN com " + j.Table + " compara duas colunas da MESMA tabela (" + eq.Text + "). A condição precisa ligar uma tabela à outra.";
                    if (a >= 0 && c >= 0 && schema[a].T != schema[c].T)
                    {
                        var ca = schema[a].T.Cols[schema[a].T.ColIndex(((ECol)eq.L).Name)];
                        var cc = schema[c].T.Cols[schema[c].T.ColIndex(((ECol)eq.R).Name)];
                        if (ca.Pk && cc.Pk)
                            return "Atenção: o JOIN ligou a chave primária de uma tabela com a chave primária da outra (" + eq.Text + "): a linha 3 de uma tabela não tem nada a ver com a linha 3 da outra. " +
                                   "Ligue a chave estrangeira com a chave primária (ex.: pedidos.cliente_id = clientes.id).";
                    }
                }
            }
            if (q.Joins.Any(j => j.Kind == "LEFT") && q.Items.Any(i => { bool s = false; Walk(i.E, x => { var f = x as EFunc; if (f != null && f.Name == "COUNT" && f.Star) s = true; }); return s; }))
                return "Atenção: com LEFT JOIN, COUNT(*) conta 1 até para quem não tem par (a linha com NULLs também é uma linha). Para contar só os pares, conte uma coluna da tabela da direita: COUNT(p.id).";
            return LintNull(q.Where) ?? LintNull(q.Having) ?? q.Joins.Select(j => LintNull(j.On)).FirstOrDefault(w => w != null);
        }

        SqlResult Select(SSelect q, SqlCtx outer, Parser p)
        {
            if (q.Union != null) return SelectUnion(q, outer);
            var schema = new List<SqlSrc>();
            var rows = new List<List<SqlSrc>>();
            if (q.From == null) rows.Add(new List<SqlSrc>());
            else
            {
                var t0 = Source(q.From);
                string a0 = q.FromAlias ?? t0.Name;
                schema.Add(new SqlSrc { Alias = a0, T = t0 });
                foreach (var r in t0.Rows) rows.Add(new List<SqlSrc> { new SqlSrc { Alias = a0, T = t0, Row = r } });
                foreach (var j in q.Joins)
                {
                    var tj = Source(j.Table);
                    string aj = j.Alias ?? tj.Name;
                    if (schema.Any(s => s.Alias.Equals(aj, StringComparison.OrdinalIgnoreCase)))
                        throw new SqlError(1066, "42000", "Not unique table/alias: '" + aj + "'", "A tabela '" + aj + "' aparece duas vezes. Dê um apelido diferente para cada uma (ex.: JOIN clientes c).");
                    schema.Add(new SqlSrc { Alias = aj, T = tj });
                    if (j.On != null) Validate(j.On, new SqlCtx { Schema = schema, Outer = outer }, "on clause");
                    var next = new List<List<SqlSrc>>();
                    var usedRight = new HashSet<object[]>();
                    foreach (var combo in rows)
                    {
                        bool any = false;
                        foreach (var r in tj.Rows)
                        {
                            var c2 = new List<SqlSrc>(combo) { new SqlSrc { Alias = aj, T = tj, Row = r } };
                            if (j.On == null || Truthy(Eval(j.On, new SqlCtx { B = c2, Schema = schema, Outer = outer }))) { next.Add(c2); any = true; usedRight.Add(r); }
                        }
                        if (!any && j.Kind == "LEFT") next.Add(new List<SqlSrc>(combo) { new SqlSrc { Alias = aj, T = tj } });
                    }
                    if (j.Kind == "RIGHT")
                        foreach (var r in tj.Rows)
                            if (!usedRight.Contains(r))
                            {
                                var c2 = schema.Take(schema.Count - 1).Select(s => new SqlSrc { Alias = s.Alias, T = s.T }).ToList();
                                c2.Add(new SqlSrc { Alias = aj, T = tj, Row = r });
                                next.Add(c2);
                            }
                    rows = next;
                }
            }

            var baseCtx = new SqlCtx { Schema = schema, Outer = outer };
            if (q.Where != null)
            {
                if (HasAgg(q.Where)) throw new SqlError(1111, "HY000", "Invalid use of group function", "COUNT, SUM, AVG, MIN e MAX não podem ir no WHERE. Para filtrar pelo resultado de uma agregação, use HAVING depois do GROUP BY.");
                Validate(q.Where, baseCtx, "where clause");
                rows = rows.Where(r => Truthy(Eval(q.Where, new SqlCtx { B = r, Schema = schema, Outer = outer }))).ToList();
            }

            // colunas e cabeçalhos do resultado
            var headers = new List<string>();
            foreach (var it in q.Items)
            {
                var st = it.E as EStar;
                if (st != null)
                {
                    if (schema.Count == 0) throw new SqlError(1096, "HY000", "No tables used", "SELECT * precisa de uma tabela: SELECT * FROM nome_da_tabela;");
                    bool found = false;
                    foreach (var s in schema)
                    {
                        if (st.Tab != null && !s.Alias.Equals(st.Tab, StringComparison.OrdinalIgnoreCase)) continue;
                        found = true;
                        foreach (var c in s.T.Cols) headers.Add(c.Name);
                    }
                    if (!found) throw new SqlError(1051, "42S02", "Unknown table '" + st.Tab + "'", "Não há nenhuma tabela (ou apelido) '" + st.Tab + "' no FROM desta consulta.");
                    continue;
                }
                Validate(it.E, baseCtx, "field list");
                string h = it.Alias ?? (it.E is ECol ? ((ECol)it.E).Name : it.E.Text);
                headers.Add(h);
            }

            bool agg = q.GroupBy.Count > 0 || q.Items.Any(i => !(i.E is EStar) && HasAgg(i.E)) || (q.Having != null && HasAgg(q.Having)) || q.Order.Any(o => HasAgg(o.Key));
            var ctxs = new List<SqlCtx>();
            if (agg)
            {
                var gexprs = q.GroupBy.Select(g => GroupExpr(g, q, schema, outer)).ToList();
                CheckFullGroup(q, schema, gexprs);
                if (gexprs.Count == 0) ctxs.Add(new SqlCtx { B = rows.Count > 0 ? rows[0] : null, G = rows, Schema = schema, Outer = outer });
                else
                {
                    var map = new Dictionary<string, List<List<SqlSrc>>>();
                    var order = new List<string>();
                    foreach (var r in rows)
                    {
                        var rc = new SqlCtx { B = r, Schema = schema, Outer = outer };
                        string key = string.Join("\u0001", gexprs.Select(g => Key(Eval(g, rc))));
                        List<List<SqlSrc>> lst;
                        if (!map.TryGetValue(key, out lst)) { lst = new List<List<SqlSrc>>(); map[key] = lst; order.Add(key); }
                        lst.Add(r);
                    }
                    foreach (var key in order) ctxs.Add(new SqlCtx { B = map[key][0], G = map[key], Schema = schema, Outer = outer });
                }
            }
            else foreach (var r in rows) ctxs.Add(new SqlCtx { B = r, Schema = schema, Outer = outer });

            // projeção
            var outRows = new List<KeyValuePair<object[], SqlCtx>>();
            foreach (var c in ctxs)
            {
                var vals = new List<object>();
                foreach (var it in q.Items)
                {
                    var st = it.E as EStar;
                    if (st != null)
                    {
                        for (int s = 0; s < schema.Count; s++)
                        {
                            if (st.Tab != null && !schema[s].Alias.Equals(st.Tab, StringComparison.OrdinalIgnoreCase)) continue;
                            for (int k = 0; k < schema[s].T.Cols.Count; k++) vals.Add(c.B == null || c.B[s].Row == null ? null : c.B[s].Row[k]);
                        }
                        continue;
                    }
                    vals.Add(Eval(it.E, c));
                }
                var arr = vals.ToArray();
                var al = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                for (int k = 0; k < headers.Count && k < arr.Length; k++)
                    if (!al.ContainsKey(headers[k])) al[headers[k]] = arr[k];
                c.Alias = al;
                outRows.Add(new KeyValuePair<object[], SqlCtx>(arr, c));
            }

            if (q.Having != null)
            {
                foreach (var c in ctxs) { Validate(q.Having, c, "having clause"); break; }
                outRows = outRows.Where(o => Truthy(Eval(q.Having, o.Value))).ToList();
            }

            if (q.Distinct)
            {
                var seen = new HashSet<string>();
                outRows = outRows.Where(o => seen.Add(string.Join("\u0001", o.Key.Select(Key)))).ToList();
            }

            if (q.Order.Count > 0)
            {
                var keys = new List<object[]>();
                foreach (var o in outRows)
                {
                    var kv = new object[q.Order.Count];
                    for (int k = 0; k < q.Order.Count; k++)
                    {
                        var e = q.Order[k].Key;
                        var lit = e as ELit;
                        if (lit != null && lit.V is decimal)
                        {
                            int pos = (int)(decimal)lit.V;
                            if (pos < 1 || pos > o.Key.Length) throw UnknownCol(pos.ToString(), "order clause");
                            kv[k] = o.Key[pos - 1];
                        }
                        else
                        {
                            Validate(e, o.Value, "order clause");
                            kv[k] = Eval(e, o.Value);
                        }
                    }
                    keys.Add(kv);
                }
                var idx = Enumerable.Range(0, outRows.Count).ToList();
                var desc = q.Order.Select(o => o.Value).ToArray();
                idx = idx.OrderBy(i => i, Comparer<int>.Create((a, b) =>
                {
                    for (int k = 0; k < desc.Length; k++)
                    {
                        int c = CompareNullsFirst(keys[a][k], keys[b][k]);
                        if (c != 0) return desc[k] ? -c : c;
                    }
                    return 0;
                })).ToList();
                outRows = idx.Select(i => outRows[i]).ToList();
            }

            if (q.Offset > 0) outRows = outRows.Skip(q.Offset).ToList();
            if (q.Limit >= 0) outRows = outRows.Take(q.Limit).ToList();

            bool simple = q.From != null && q.Joins.Count == 0 && q.Where == null && q.GroupBy.Count == 0 && q.Having == null && q.Order.Count == 0 &&
                          q.Limit < 0 && !q.Distinct && q.Items.Count == 1 && q.Items[0].E is EStar;
            return new SqlResult { Kind = "select", Headers = headers, Rows = outRows.Select(o => o.Key).ToList(), Simple = simple, Table = q.From, Warn = LintSelect(q, schema) };
        }

        // GROUP BY aceita número da coluna (GROUP BY 1) ou apelido do SELECT
        Ex GroupExpr(Ex g, SSelect q, List<SqlSrc> schema, SqlCtx outer)
        {
            var lit = g as ELit;
            if (lit != null && lit.V is decimal)
            {
                int pos = (int)(decimal)lit.V;
                if (pos < 1 || pos > q.Items.Count || q.Items[pos - 1].E is EStar) throw UnknownCol(pos.ToString(), "group statement");
                return q.Items[pos - 1].E;
            }
            var c = g as ECol;
            if (c != null && c.Tab == null)
            {
                int si, ci;
                bool inSchema = false;
                try { inSchema = Find(schema, c, out si, out ci, "group statement"); } catch (SqlError) { throw; }
                if (!inSchema)
                {
                    var it = q.Items.FirstOrDefault(i => i.Alias != null && i.Alias.Equals(c.Name, StringComparison.OrdinalIgnoreCase));
                    if (it != null) return it.E;
                }
            }
            if (HasAgg(g)) throw new SqlError(1056, "42000", "Can't group on '" + g.Text + "'", "Não dá para agrupar por uma agregação. Agrupe por uma coluna (ex.: GROUP BY categoria).");
            Validate(g, new SqlCtx { Schema = schema, Outer = outer }, "group statement");
            return g;
        }

        // ONLY_FULL_GROUP_BY (padrão do MySQL 8): colunas fora de agregações precisam estar no GROUP BY
        void CheckFullGroup(SSelect q, List<SqlSrc> schema, List<Ex> gexprs)
        {
            var grouped = new List<KeyValuePair<int, int>>();
            if (gexprs != null)
                foreach (var g in gexprs)
                {
                    var gc = g as ECol;
                    int si, ci;
                    if (gc != null && Find(schema, gc, out si, out ci, "group statement")) grouped.Add(new KeyValuePair<int, int>(si, ci));
                }
            var gtexts = gexprs == null ? new HashSet<string>() : new HashSet<string>(gexprs.Select(g => Norm(g.Text)));
            for (int n = 0; n < q.Items.Count; n++)
            {
                var it = q.Items[n];
                string bad = null;
                if (it.E is EStar)
                {
                    if (schema.Count > 0) bad = schema[0].T.Name + "." + schema[0].T.Cols[0].Name;
                    if (gexprs != null && gexprs.Count > 0)
                    {
                        // SELECT * com GROUP BY só passa se agrupar pela chave primária de todas as tabelas
                        bool ok = schema.All(s => grouped.Any(g => schema[g.Key] == s && s.T.Cols[g.Value].Pk));
                        if (ok) bad = null;
                    }
                }
                else if (!gtexts.Contains(Norm(it.E.Text)))
                {
                    WalkNoAgg(it.E, x =>
                    {
                        if (bad != null) return;
                        var c = x as ECol;
                        int si, ci;
                        if (c == null || !Find(schema, c, out si, out ci, "field list")) return;
                        bool ok = grouped.Any(g => g.Key == si && g.Value == ci) || grouped.Any(g => g.Key == si && schema[si].T.Cols[g.Value].Pk);
                        if (!ok) bad = DbName + "." + schema[si].T.Name + "." + schema[si].T.Cols[ci].Name;
                    });
                }
                if (bad == null) continue;
                if (gexprs == null || gexprs.Count == 0)
                    throw new SqlError(1140, "42000", "In aggregated query without GROUP BY, expression #" + (n + 1) + " of SELECT list contains nonaggregated column '" + bad + "'; this is incompatible with sql_mode=only_full_group_by",
                        "Você misturou uma agregação (COUNT, SUM...) com uma coluna comum sem GROUP BY. Ou tire a coluna, ou agrupe por ela: ... GROUP BY coluna.");
                throw new SqlError(1055, "42000", "Expression #" + (n + 1) + " of SELECT list is not in GROUP BY clause and contains nonaggregated column '" + bad + "' which is not functionally dependent on columns in GROUP BY clause; this is incompatible with sql_mode=only_full_group_by",
                    "Toda coluna do SELECT que não está dentro de COUNT/SUM/AVG/MIN/MAX precisa aparecer no GROUP BY.");
            }
        }

        static void WalkNoAgg(Ex e, Action<Ex> f)
        {
            if (e == null) return;
            var fn = e as EFunc;
            if (fn != null && IsAgg(fn.Name)) return;
            f(e);
            var b = e as EBin; if (b != null) { WalkNoAgg(b.L, f); WalkNoAgg(b.R, f); return; }
            var u = e as EUn; if (u != null) { WalkNoAgg(u.E, f); return; }
            var n = e as EIsNull; if (n != null) { WalkNoAgg(n.E, f); return; }
            var l = e as ELike; if (l != null) { WalkNoAgg(l.E, f); WalkNoAgg(l.Pat, f); return; }
            var i = e as EIn; if (i != null) { WalkNoAgg(i.E, f); if (i.List != null) foreach (var x in i.List) WalkNoAgg(x, f); return; }
            var bt = e as EBetween; if (bt != null) { WalkNoAgg(bt.E, f); WalkNoAgg(bt.Lo, f); WalkNoAgg(bt.Hi, f); return; }
            if (fn != null) { foreach (var x in fn.Args) WalkNoAgg(x, f); return; }
            var c = e as ECase; if (c != null) { WalkNoAgg(c.Base, f); foreach (var x in c.Whens) WalkNoAgg(x, f); foreach (var x in c.Thens) WalkNoAgg(x, f); WalkNoAgg(c.Else, f); }
        }

        static string Norm(string s) { return new string((s ?? "").ToLowerInvariant().Where(c => !char.IsWhiteSpace(c)).ToArray()); }

        // ───────────────────────────── avaliação ─────────────────────────────
        public static bool Truthy(object v)
        {
            if (v == null) return false;
            if (v is decimal) return (decimal)v != 0;
            return NumPrefix(v.ToString()) != 0;
        }

        static object B(bool b) { return b ? 1m : 0m; }

        static decimal NumPrefix(string s)
        {
            var m = Regex.Match(s.Trim(), @"^[+-]?(\d+(\.\d*)?|\.\d+)");
            decimal d;
            return m.Success && decimal.TryParse(m.Value, NumberStyles.Float, Inv, out d) ? d : 0m;
        }

        static decimal? Dec(object v)
        {
            if (v == null) return null;
            if (v is decimal) return (decimal)v;
            return NumPrefix(v.ToString());
        }

        static string Fold(string s)
        {
            var d = s.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in d) if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            return sb.ToString();
        }

        // comparação no estilo MySQL (collation sem diferenciar maiúsculas e acentos)
        public static int Compare(object a, object b)
        {
            if (a is decimal && b is decimal) return ((decimal)a).CompareTo((decimal)b);
            if (a is decimal || b is decimal) return Dec(a).Value.CompareTo(Dec(b).Value);
            string sa = a.ToString(), sb = b.ToString();
            if (DateLike.IsMatch(sa) && DateLike.IsMatch(sb)) return Math.Sign(string.CompareOrdinal(sa, sb));   // datas: compara como texto 'AAAA-MM-DD hh:mm:ss'
            return Math.Sign(Cmp.Compare(sa, sb, CI));
        }

        static int CompareNullsFirst(object a, object b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;
            return Compare(a, b);
        }

        public static decimal NormDec(decimal d) { return d / 1.000000000000000000000000000000000m; }

        public static string Key(object v)
        {
            if (v == null) return "\0N";
            if (v is decimal) return "d" + NormDec((decimal)v).ToString(Inv);
            return "s" + Fold(v.ToString()).TrimEnd();
        }

        object Eval(Ex e, SqlCtx c)
        {
            var lit = e as ELit; if (lit != null) return lit.V;
            var col = e as ECol; if (col != null) return ColValue(col, c, c.Alias != null ? "order clause" : "field list");
            var bin = e as EBin;
            if (bin != null)
            {
                if (bin.Op == "AND")
                {
                    var l = Eval(bin.L, c);
                    if (l != null && !Truthy(l)) return 0m;
                    var r = Eval(bin.R, c);
                    if (r != null && !Truthy(r)) return 0m;
                    return l == null || r == null ? null : (object)1m;
                }
                if (bin.Op == "OR")
                {
                    var l = Eval(bin.L, c);
                    if (l != null && Truthy(l)) return 1m;
                    var r = Eval(bin.R, c);
                    if (r != null && Truthy(r)) return 1m;
                    return l == null || r == null ? null : (object)0m;
                }
                var a = Eval(bin.L, c);
                var b = Eval(bin.R, c);
                if (a == null || b == null) return null;
                if (b is SqlInterval && (bin.Op == "+" || bin.Op == "-")) return DateAdd(a, (SqlInterval)b, bin.Op == "-" ? -1 : 1);
                if (a is SqlInterval && bin.Op == "+") return DateAdd(b, (SqlInterval)a, 1);
                switch (bin.Op)
                {
                    case "=": return B(Compare(a, b) == 0);
                    case "<>": case "!=": return B(Compare(a, b) != 0);
                    case "<": return B(Compare(a, b) < 0);
                    case "<=": return B(Compare(a, b) <= 0);
                    case ">": return B(Compare(a, b) > 0);
                    case ">=": return B(Compare(a, b) >= 0);
                }
                decimal x = Dec(a).Value, y = Dec(b).Value;
                switch (bin.Op)
                {
                    case "+": return x + y;
                    case "-": return x - y;
                    case "*": return x * y;
                    case "/": return y == 0 ? null : (object)Math.Round(x / y, 4);
                    case "DIV": return y == 0 ? null : (object)decimal.Truncate(x / y);
                    case "%": case "MOD": return y == 0 ? null : (object)(x % y);
                }
                throw new SqlError(1064, "42000", "Unsupported operator " + bin.Op, "Operador não suportado.");
            }
            var un = e as EUn;
            if (un != null)
            {
                var v = Eval(un.E, c);
                if (v == null) return null;
                if (un.Op == "NOT") return B(!Truthy(v));
                return -Dec(v).Value;
            }
            var isn = e as EIsNull; if (isn != null) { bool n = Eval(isn.E, c) == null; return B(isn.Not ? !n : n); }
            var like = e as ELike;
            if (like != null)
            {
                var v = Eval(like.E, c); var p = Eval(like.Pat, c);
                if (v == null || p == null) return null;
                bool m = LikeMatch(Str(v), Str(p));
                return B(like.Not ? !m : m);
            }
            var inn = e as EIn;
            if (inn != null)
            {
                var v = Eval(inn.E, c);
                if (v == null) return null;
                List<object> vals;
                if (inn.Sub != null)
                {
                    var r = Select(inn.Sub, c, null);
                    if (r.Headers.Count != 1) throw new SqlError(1241, "21000", "Operand should contain 1 column(s)", "A subconsulta do IN precisa devolver uma coluna só.");
                    vals = r.Rows.Select(x => x[0]).ToList();
                }
                else vals = inn.List.Select(x => Eval(x, c)).ToList();
                bool found = vals.Any(x => x != null && Compare(v, x) == 0), hasNull = vals.Any(x => x == null);
                if (found) return B(!inn.Not);
                if (hasNull) return null;
                return B(inn.Not);
            }
            var bt = e as EBetween;
            if (bt != null)
            {
                var v = Eval(bt.E, c); var lo = Eval(bt.Lo, c); var hi = Eval(bt.Hi, c);
                if (v == null || lo == null || hi == null) return null;
                bool m = Compare(v, lo) >= 0 && Compare(v, hi) <= 0;
                return B(bt.Not ? !m : m);
            }
            var fn = e as EFunc; if (fn != null) return Func(fn, c);
            var sub = e as ESub;
            if (sub != null)
            {
                var r = Select(sub.Q, c, null);
                if (r.Headers.Count != 1) throw new SqlError(1241, "21000", "Operand should contain 1 column(s)", "A subconsulta precisa devolver uma coluna só.");
                if (r.Rows.Count > 1) throw new SqlError(1242, "21000", "Subquery returns more than 1 row", "A subconsulta devolveu várias linhas, mas aqui cabe um valor só. Use IN (...) ou uma agregação (AVG, MAX...).");
                return r.Rows.Count == 0 ? null : r.Rows[0][0];
            }
            var ex = e as EExists;
            if (ex != null) { var r = Select(ex.Q, c, null); return B(ex.Not ? r.Rows.Count == 0 : r.Rows.Count > 0); }
            var cs = e as ECase;
            if (cs != null)
            {
                var bv = cs.Base != null ? Eval(cs.Base, c) : null;
                for (int k = 0; k < cs.Whens.Count; k++)
                {
                    var w = Eval(cs.Whens[k], c);
                    bool hit = cs.Base != null ? (bv != null && w != null && Compare(bv, w) == 0) : Truthy(w);
                    if (hit) return Eval(cs.Thens[k], c);
                }
                return cs.Else != null ? Eval(cs.Else, c) : null;
            }
            throw new SqlError(1064, "42000", "Unsupported expression", "Expressão não suportada pelo simulador.");
        }

        static string Str(object v) { return v is decimal ? ((decimal)v).ToString(Inv) : v.ToString(); }

        static bool LikeMatch(string v, string pat)
        {
            var sb = new StringBuilder("^");
            for (int i = 0; i < pat.Length; i++)
            {
                char ch = pat[i];
                if (ch == '\\' && i + 1 < pat.Length) { sb.Append(Regex.Escape(pat[++i].ToString())); continue; }
                if (ch == '%') sb.Append(".*");
                else if (ch == '_') sb.Append('.');
                else sb.Append(Regex.Escape(ch.ToString()));
            }
            sb.Append('$');
            return Regex.IsMatch(Fold(v), Fold(sb.ToString()), RegexOptions.Singleline);
        }

        object Func(EFunc f, SqlCtx c)
        {
            if (IsAgg(f.Name))
            {
                if (c.G == null) throw new SqlError(1111, "HY000", "Invalid use of group function", "Funções de agregação (COUNT, SUM, AVG, MIN, MAX) só podem ser usadas no SELECT, no HAVING ou no ORDER BY — nunca no WHERE.");
                if (f.Name == "COUNT" && f.Star) return (decimal)c.G.Count;
                if (f.Args.Count != 1 && f.Name != "GROUP_CONCAT") throw new SqlError(1064, "42000", "Wrong number of arguments for " + f.Name, f.Name + " recebe um argumento: " + f.Name + "(coluna).");
                var vals = new List<object>();
                foreach (var m in c.G)
                {
                    var v = Eval(f.Args[0], new SqlCtx { B = m, Schema = c.Schema, Outer = c.Outer });
                    if (v != null) vals.Add(v);
                }
                if (f.Distinct) { var seen = new HashSet<string>(); vals = vals.Where(v => seen.Add(Key(v))).ToList(); }
                switch (f.Name)
                {
                    case "COUNT": return (decimal)vals.Count;
                    case "SUM": return vals.Count == 0 ? null : (object)vals.Sum(v => Dec(v).Value);
                    case "AVG": return vals.Count == 0 ? null : (object)Math.Round(vals.Sum(v => Dec(v).Value) / vals.Count, 4);
                    case "MIN": return vals.Count == 0 ? null : vals.Aggregate((a, b) => Compare(a, b) <= 0 ? a : b);
                    case "MAX": return vals.Count == 0 ? null : vals.Aggregate((a, b) => Compare(a, b) >= 0 ? a : b);
                    case "GROUP_CONCAT": return vals.Count == 0 ? null : string.Join(",", vals.Select(Str));
                }
            }
            var args = f.Args.Select(a => Eval(a, c)).ToList();
            Func<int, object> A = i => i < args.Count ? args[i] : null;
            switch (f.Name)
            {
                case "UPPER": case "UCASE": return A(0) == null ? null : Str(A(0)).ToUpperInvariant();
                case "LOWER": case "LCASE": return A(0) == null ? null : Str(A(0)).ToLowerInvariant();
                // LENGTH conta BYTES (em UTF-8, 'ã' ocupa 2); CHAR_LENGTH conta caracteres
                case "LENGTH": case "OCTET_LENGTH": return A(0) == null ? null : (object)(decimal)Encoding.UTF8.GetByteCount(Str(A(0)));
                case "CHAR_LENGTH": case "CHARACTER_LENGTH": return A(0) == null ? null : (object)(decimal)Str(A(0)).Length;
                case "TRIM": return A(0) == null ? null : Str(A(0)).Trim(' ');
                case "LTRIM": return A(0) == null ? null : Str(A(0)).TrimStart(' ');
                case "RTRIM": return A(0) == null ? null : Str(A(0)).TrimEnd(' ');
                case "CONCAT": return args.Any(a => a == null) ? null : string.Concat(args.Select(Str));
                case "CONCAT_WS": return A(0) == null ? null : string.Join(Str(A(0)), args.Skip(1).Where(a => a != null).Select(Str));
                case "COALESCE": case "IFNULL": return args.FirstOrDefault(a => a != null);
                case "NULLIF": return A(0) != null && A(1) != null && Compare(A(0), A(1)) == 0 ? null : A(0);
                case "IF": return Truthy(A(0)) ? A(1) : A(2);
                case "ABS": return A(0) == null ? null : (object)Math.Abs(Dec(A(0)).Value);
                case "FLOOR": return A(0) == null ? null : (object)Math.Floor(Dec(A(0)).Value);
                case "CEIL": case "CEILING": return A(0) == null ? null : (object)Math.Ceiling(Dec(A(0)).Value);
                case "TRUNCATE":
                    {
                        if (A(0) == null || A(1) == null) return null;
                        decimal fac = 1; for (int i = 0; i < (int)Dec(A(1)).Value; i++) fac *= 10;
                        return decimal.Truncate(Dec(A(0)).Value * fac) / fac;
                    }
                case "MOD": return A(0) == null || A(1) == null || Dec(A(1)).Value == 0 ? null : (object)(Dec(A(0)).Value % Dec(A(1)).Value);
                case "POWER": case "POW": return A(0) == null || A(1) == null ? null : (object)(decimal)Math.Pow((double)Dec(A(0)).Value, (double)Dec(A(1)).Value);
                case "SQRT": return A(0) == null || Dec(A(0)).Value < 0 ? null : (object)Math.Round((decimal)Math.Sqrt((double)Dec(A(0)).Value), 6);
                case "GREATEST": return args.Any(a => a == null) || args.Count == 0 ? null : args.Aggregate((a, b) => Compare(a, b) >= 0 ? a : b);
                case "LEAST": return args.Any(a => a == null) || args.Count == 0 ? null : args.Aggregate((a, b) => Compare(a, b) <= 0 ? a : b);
                case "LEFT":
                    {
                        if (A(0) == null || A(1) == null) return null;
                        string s = Str(A(0)); int n = Math.Max(0, (int)Dec(A(1)).Value);
                        return s.Substring(0, Math.Min(n, s.Length));
                    }
                case "RIGHT":
                    {
                        if (A(0) == null || A(1) == null) return null;
                        string s = Str(A(0)); int n = Math.Max(0, (int)Dec(A(1)).Value);
                        return s.Substring(s.Length - Math.Min(n, s.Length));
                    }
                case "REPLACE": return A(0) == null || A(1) == null || A(2) == null ? null : (Str(A(1)).Length == 0 ? Str(A(0)) : Str(A(0)).Replace(Str(A(1)), Str(A(2))));
                case "REVERSE": return A(0) == null ? null : new string(Str(A(0)).Reverse().ToArray());
                case "LPAD": case "RPAD":
                    {
                        if (A(0) == null || A(1) == null || A(2) == null) return null;
                        string s = Str(A(0)), pad = Str(A(2)); int n = (int)Dec(A(1)).Value;
                        if (n < 0) return null;
                        if (s.Length >= n) return s.Substring(0, n);
                        if (pad.Length == 0) return null;
                        var sb = new StringBuilder();
                        while (sb.Length < n - s.Length) sb.Append(pad);
                        string fill = sb.ToString().Substring(0, n - s.Length);
                        return f.Name == "LPAD" ? fill + s : s + fill;
                    }
                case "LOCATE": case "INSTR": case "POSITION":
                    {
                        // LOCATE(trecho, texto) e INSTR(texto, trecho): posição a partir de 1 (0 = não achou)
                        var sub = f.Name == "INSTR" ? A(1) : A(0); var str = f.Name == "INSTR" ? A(0) : A(1);
                        if (sub == null || str == null) return null;
                        int start = f.Name == "LOCATE" && args.Count > 2 && A(2) != null ? Math.Max(1, (int)Dec(A(2)).Value) - 1 : 0;
                        string hs = Fold(Str(str)), nd = Fold(Str(sub));
                        if (start > hs.Length) return 0m;
                        return (decimal)(hs.IndexOf(nd, start, StringComparison.Ordinal) + 1);
                    }
                case "SUBSTRING_INDEX":
                    {
                        if (A(0) == null || A(1) == null || A(2) == null) return null;
                        string s = Str(A(0)), d = Str(A(1)); int n = (int)Dec(A(2)).Value;
                        if (n == 0 || d.Length == 0) return "";
                        var parts = s.Split(new[] { d }, StringSplitOptions.None);
                        if (Math.Abs(n) >= parts.Length) return s;
                        return n > 0 ? string.Join(d, parts.Take(n)) : string.Join(d, parts.Skip(parts.Length + n));
                    }
                case "INTERVAL":
                    {
                        if (A(0) == null) return null;
                        return new SqlInterval { N = Dec(A(0)).Value, Unit = Str(A(1)) };
                    }
                case "DATE_ADD": case "ADDDATE": case "DATE_SUB": case "SUBDATE":
                    {
                        if (A(0) == null || A(1) == null) return null;
                        var iv = A(1) as SqlInterval ?? new SqlInterval { N = Dec(A(1)).Value, Unit = "DAY" };
                        return DateAdd(A(0), iv, f.Name == "DATE_SUB" || f.Name == "SUBDATE" ? -1 : 1);
                    }
                case "DATEDIFF":
                    {
                        DateTime d1, d2;
                        if (!ToDate(A(0), out d1) || !ToDate(A(1), out d2)) return null;
                        return (decimal)(d1.Date - d2.Date).Days;
                    }
                case "TIMESTAMPDIFF":
                    {
                        DateTime d1, d2;
                        if (A(0) == null || !ToDate(A(1), out d1) || !ToDate(A(2), out d2)) return null;
                        switch (Str(A(0)).ToUpperInvariant())
                        {
                            case "SECOND": return (decimal)Math.Truncate((d2 - d1).TotalSeconds);
                            case "MINUTE": return (decimal)Math.Truncate((d2 - d1).TotalMinutes);
                            case "HOUR": return (decimal)Math.Truncate((d2 - d1).TotalHours);
                            case "DAY": return (decimal)Math.Truncate((d2 - d1).TotalDays);
                            case "WEEK": return (decimal)Math.Truncate((d2 - d1).TotalDays / 7);
                            case "MONTH": case "YEAR":
                                {
                                    int m = (d2.Year - d1.Year) * 12 + d2.Month - d1.Month;
                                    if (m > 0 && d2 < d1.AddMonths(m)) m--;
                                    if (m < 0 && d2 > d1.AddMonths(m)) m++;
                                    return Str(A(0)).ToUpperInvariant() == "MONTH" ? (decimal)m : (decimal)(m / 12);
                                }
                        }
                        throw new SqlError(1064, "42000", "Unknown unit " + Str(A(0)), "Unidades do TIMESTAMPDIFF: SECOND, MINUTE, HOUR, DAY, WEEK, MONTH, YEAR.");
                    }
                case "DATE":
                    {
                        DateTime d;
                        return ToDate(A(0), out d) ? d.ToString("yyyy-MM-dd", Inv) : null;
                    }
                case "HOUR": case "MINUTE": case "SECOND":
                    {
                        DateTime d;
                        if (!ToDate(A(0), out d)) return null;
                        return (decimal)(f.Name == "HOUR" ? d.Hour : f.Name == "MINUTE" ? d.Minute : d.Second);
                    }
                case "DAYOFWEEK": { DateTime d; return ToDate(A(0), out d) ? (object)(decimal)((int)d.DayOfWeek + 1) : null; }
                case "LAST_DAY": { DateTime d; return ToDate(A(0), out d) ? new DateTime(d.Year, d.Month, DateTime.DaysInMonth(d.Year, d.Month)).ToString("yyyy-MM-dd", Inv) : null; }
                case "DATE_FORMAT":
                    {
                        DateTime d;
                        if (A(1) == null || !ToDate(A(0), out d)) return null;
                        string fmt = Str(A(1));
                        var sb = new StringBuilder();
                        for (int i = 0; i < fmt.Length; i++)
                        {
                            if (fmt[i] != '%' || i + 1 >= fmt.Length) { sb.Append(fmt[i]); continue; }
                            char ch = fmt[++i];
                            switch (ch)
                            {
                                case 'Y': sb.Append(d.Year.ToString("0000")); break;
                                case 'y': sb.Append((d.Year % 100).ToString("00")); break;
                                case 'm': sb.Append(d.Month.ToString("00")); break;
                                case 'c': sb.Append(d.Month); break;
                                case 'd': sb.Append(d.Day.ToString("00")); break;
                                case 'e': sb.Append(d.Day); break;
                                case 'H': sb.Append(d.Hour.ToString("00")); break;
                                case 'i': sb.Append(d.Minute.ToString("00")); break;
                                case 's': case 'S': sb.Append(d.Second.ToString("00")); break;
                                case 'M': sb.Append(d.ToString("MMMM", Inv)); break;
                                case 'b': sb.Append(d.ToString("MMM", Inv)); break;
                                case 'W': sb.Append(d.DayOfWeek.ToString()); break;
                                default: sb.Append(ch); break;
                            }
                        }
                        return sb.ToString();
                    }
                case "ROUND":
                    {
                        if (A(0) == null) return null;
                        int d = args.Count > 1 && A(1) != null ? (int)Dec(A(1)).Value : 0;
                        return Math.Round(Dec(A(0)).Value, Math.Max(0, Math.Min(10, d)), MidpointRounding.AwayFromZero);
                    }
                case "SUBSTRING": case "SUBSTR":
                    {
                        if (A(0) == null || A(1) == null) return null;
                        string s = Str(A(0));
                        int st = Math.Max(1, (int)Dec(A(1)).Value) - 1;
                        if (st >= s.Length) return "";
                        int len = args.Count > 2 && A(2) != null ? (int)Dec(A(2)).Value : s.Length - st;
                        return s.Substring(st, Math.Max(0, Math.Min(len, s.Length - st)));
                    }
                case "NOW": case "CURRENT_TIMESTAMP": return "2026-10-07 10:30:00";
                case "CURDATE": case "CURRENT_DATE": return "2026-10-07";
                case "YEAR": case "MONTH": case "DAY":
                    {
                        if (A(0) == null) return null;
                        var parts = Str(A(0)).Split('-', ' ');
                        int k = f.Name == "YEAR" ? 0 : f.Name == "MONTH" ? 1 : 2;
                        decimal d;
                        return parts.Length > k && decimal.TryParse(parts[k], NumberStyles.Integer, Inv, out d) ? (object)d : null;
                    }
                case "DATABASE": return DbName;
                case "VERSION": return "8.0.36";
            }
            throw new SqlError(1305, "42000", "FUNCTION " + DbName + "." + f.Name.ToLowerInvariant() + " does not exist",
                "A função " + f.Name + " não existe. Confira o nome (as mais usadas: COUNT, SUM, AVG, MIN, MAX, UPPER, LOWER, ROUND, CONCAT).");
        }

        static readonly Regex DateLike = new Regex(@"^\d{4}-\d{2}-\d{2}( \d{2}:\d{2}:\d{2})?$");

        static bool ToDate(object v, out DateTime d)
        {
            d = DateTime.MinValue;
            if (v == null) return false;
            return DateTime.TryParseExact(Str(v).Trim(), new[] { "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd H:mm:ss", "yyyy-MM-dd HH:mm" }, Inv, DateTimeStyles.None, out d);
        }

        // data + INTERVAL n UNIDADE (devolve 'AAAA-MM-DD', ou com hora quando a data tem hora ou a unidade é de tempo)
        static object DateAdd(object v, SqlInterval iv, int sign)
        {
            DateTime d;
            if (!ToDate(v, out d)) return null;
            string unit = (iv.Unit ?? "DAY").ToUpperInvariant();
            int n = (int)iv.N * sign;
            switch (unit)
            {
                case "SECOND": d = d.AddSeconds(n); break;
                case "MINUTE": d = d.AddMinutes(n); break;
                case "HOUR": d = d.AddHours(n); break;
                case "DAY": d = d.AddDays(n); break;
                case "WEEK": d = d.AddDays(7 * n); break;
                case "MONTH": d = d.AddMonths(n); break;
                case "YEAR": d = d.AddYears(n); break;
                default: throw new SqlError(1064, "42000", "Unknown interval unit " + unit, "Unidades do INTERVAL: SECOND, MINUTE, HOUR, DAY, WEEK, MONTH, YEAR.");
            }
            bool time = Str(v).Trim().Length > 10 || unit == "SECOND" || unit == "MINUTE" || unit == "HOUR";
            return d.ToString(time ? "yyyy-MM-dd HH:mm:ss" : "yyyy-MM-dd", Inv);
        }

        // ───────────────────────────── tipos ─────────────────────────────
        static object Coerce(SqlCol col, object v, int row)
        {
            if (v == null) return null;
            if (col.IsNum)
            {
                decimal d;
                if (v is decimal) d = (decimal)v;
                else if (!decimal.TryParse(v.ToString().Trim(), NumberStyles.Float, Inv, out d))
                    throw new SqlError(1366, "HY000", "Incorrect " + (col.IsInt ? "integer" : "decimal") + " value: '" + v + "' for column '" + col.Name + "' at row " + row,
                        "A coluna '" + col.Name + "' guarda números, mas recebeu '" + v + "'. Números vão sem aspas e com ponto decimal (ex.: 7.50).");
                if (col.IsInt) return decimal.Truncate(Math.Round(d, 0, MidpointRounding.AwayFromZero));
                if (col.Type == "DECIMAL")
                {
                    int s = Math.Max(0, col.Scale);
                    return Math.Round(d, s, MidpointRounding.AwayFromZero) + new decimal(0, 0, 0, false, (byte)s);
                }
                return d;
            }
            string str = Str(v);
            if ((col.Type == "VARCHAR" || col.Type == "CHAR") && col.Len > 0 && str.Length > col.Len)
                throw new SqlError(1406, "22001", "Data too long for column '" + col.Name + "' at row " + row, "O texto é maior do que o limite da coluna '" + col.Name + "' (" + col.TypeText + ").");
            if (col.Type == "DATE" && !Regex.IsMatch(str, @"^\d{4}-\d{2}-\d{2}$"))
                throw new SqlError(1292, "22007", "Incorrect date value: '" + str + "' for column '" + col.Name + "' at row " + row, "Datas no MySQL usam o formato 'AAAA-MM-DD', entre aspas (ex.: '2026-10-07').");
            if (col.Type == "DATETIME" || col.Type == "TIMESTAMP")
            {
                if (Regex.IsMatch(str, @"^\d{4}-\d{2}-\d{2}$")) str += " 00:00:00";
                if (!Regex.IsMatch(str, @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$"))
                    throw new SqlError(1292, "22007", "Incorrect datetime value: '" + str + "' for column '" + col.Name + "' at row " + row, "Data e hora no MySQL usam o formato 'AAAA-MM-DD hh:mm:ss', entre aspas (ex.: '2026-10-07 14:30:00').");
            }
            return str;
        }

        void CheckRow(SqlTable t, object[] row, object[] skip, int rowNum)
        {
            for (int k = 0; k < t.Cols.Count; k++)
            {
                var c = t.Cols[k];
                if (row[k] == null && (c.NotNull || c.Pk))
                    throw new SqlError(1048, "23000", "Column '" + c.Name + "' cannot be null", "A coluna '" + c.Name + "' é obrigatória (NOT NULL): informe um valor para ela.");
                if (row[k] != null && (c.Pk || c.Unique))
                    foreach (var o in t.Rows)
                        if (o != skip && o != row && o[k] != null && Compare(o[k], row[k]) == 0)
                            throw new SqlError(1062, "23000", "Duplicate entry '" + Str(row[k]) + "' for key '" + t.Name + "." + (c.Pk ? "PRIMARY" : c.Name) + "'",
                                "Já existe uma linha com " + c.Name + " = " + Str(row[k]) + ", e essa coluna não aceita repetidos" + (c.Pk ? " (é a chave primária)." : " (UNIQUE)."));
                if (row[k] != null && c.RefTable != null)
                {
                    SqlTable pt;
                    if (Tables.TryGetValue(c.RefTable, out pt))
                    {
                        int pk = pt.ColIndex(c.RefCol);
                        if (pk >= 0 && !pt.Rows.Any(r => r[pk] != null && Compare(r[pk], row[k]) == 0))
                            throw new SqlError(1452, "23000", "Cannot add or update a child row: a foreign key constraint fails (`" + DbName + "`.`" + t.Name + "`, CONSTRAINT `" + t.Name + "_ibfk_1` FOREIGN KEY (`" + c.Name + "`) REFERENCES `" + pt.Name + "` (`" + c.RefCol + "`))",
                                "Não existe em " + pt.Name + " uma linha com " + c.RefCol + " = " + Str(row[k]) + ". A chave estrangeira (FOREIGN KEY) impede registros 'órfãos'.");
                    }
                }
            }
            foreach (var ix in t.Indexes.Where(i => i.Unique))
            {
                var ks = ix.Cols.Select(t.ColIndex).ToList();
                if (ks.Any(k => row[k] == null)) continue;
                foreach (var o in t.Rows)
                    if (o != skip && o != row && ks.All(k => o[k] != null && Compare(o[k], row[k]) == 0))
                        throw new SqlError(1062, "23000", "Duplicate entry '" + string.Join("-", ks.Select(k => Str(row[k]))) + "' for key '" + t.Name + "." + ix.Name + "'", "O índice único " + ix.Name + " não aceita valores repetidos.");
            }
            CheckConstraints(t, row);
        }

        // CHECK: a linha só entra se a condição não for FALSA (NULL passa, como no MySQL)
        void CheckConstraints(SqlTable t, object[] row)
        {
            foreach (var ck in t.Checks)
            {
                var ctx = new SqlCtx { B = new List<SqlSrc> { new SqlSrc { Alias = t.Name, T = t, Row = row } }, Schema = new List<SqlSrc> { new SqlSrc { Alias = t.Name, T = t } } };
                var v = Eval(ck.Value, ctx);
                if (v != null && !Truthy(v))
                    throw new SqlError(3819, "HY000", "Check constraint '" + ck.Key + "' is violated.",
                        "O valor não passa na regra CHECK (" + ck.Value.Text + ") da tabela " + t.Name + ". O banco recusou a linha inteira.");
            }
        }

        // linhas de outras tabelas que apontam para esta (impede apagar o "pai")
        void CheckParent(SqlTable t, IEnumerable<object[]> leaving) { CheckParent(t, leaving, false, 0); }

        // deleting = true: aplica ON DELETE CASCADE / SET NULL; sem ação (RESTRICT), recusa com o erro 1451
        void CheckParent(SqlTable t, IEnumerable<object[]> leaving, bool deleting, int depth)
        {
            var gone = leaving.ToList();
            if (gone.Count == 0 || depth > 20) return;
            foreach (var child in Tables.Values.ToList())
                foreach (var cc in child.Cols.Where(q => q.RefTable != null && q.RefTable.Equals(t.Name, StringComparison.OrdinalIgnoreCase)).ToList())
                {
                    int pk = t.ColIndex(cc.RefCol), ck = child.ColIndex(cc.Name);
                    if (pk < 0 || ck < 0) continue;
                    var deps = child.Rows.Where(r => !gone.Contains(r) && r[ck] != null && gone.Any(g => g[pk] != null && Compare(r[ck], g[pk]) == 0)).ToList();
                    if (deps.Count == 0) continue;
                    if (deleting && cc.OnDelete == "CASCADE")
                    {
                        CheckParent(child, deps, true, depth + 1);
                        child.Rows.RemoveAll(r => deps.Contains(r));
                        continue;
                    }
                    if (deleting && cc.OnDelete == "SET NULL")
                    {
                        foreach (var r in deps) r[ck] = null;
                        continue;
                    }
                    throw new SqlError(1451, "23000", "Cannot delete or update a parent row: a foreign key constraint fails (`" + DbName + "`.`" + child.Name + "`, CONSTRAINT `" + (cc.FkName ?? child.Name + "_ibfk_1") + "` FOREIGN KEY (`" + cc.Name + "`) REFERENCES `" + t.Name + "` (`" + cc.RefCol + "`))",
                        "Existem linhas em " + child.Name + " ligadas a esta linha de " + t.Name + " (chave estrangeira). Apague (ou altere) primeiro os registros dependentes.");
                }
        }

        // ───────────────────────────── INSERT / UPDATE / DELETE ─────────────────────────────
        SqlResult Insert(Parser p)
        {
            p.Expect("INSERT"); p.Accept("INTO");
            var t = Table(p.Ident());
            var cols = new List<int>();
            if (p.AcceptSym("("))
            {
                do
                {
                    string cn = p.Ident();
                    int k = t.ColIndex(cn);
                    if (k < 0) throw UnknownCol(cn, "field list");
                    cols.Add(k);
                } while (p.AcceptSym(","));
                p.ExpectSym(")");
            }
            else cols = Enumerable.Range(0, t.Cols.Count).ToList();

            var data = new List<List<object>>();
            if (p.Peek.U == "SELECT")
            {
                var r = Select(p.ParseSelect(), null, p);
                data = r.Rows.Select(x => x.ToList()).ToList();
            }
            else
            {
                if (!p.Accept("VALUES") && !p.Accept("VALUE")) throw Syntax(p.Src, p.Peek.Pos);
                do
                {
                    p.ExpectSym("(");
                    var vals = new List<object>();
                    if (!p.IsSym(")"))
                        do vals.Add(p.Accept("DEFAULT") ? DefaultMark : Eval(p.ParseExpr(), new SqlCtx())); while (p.AcceptSym(","));
                    p.ExpectSym(")");
                    data.Add(vals);
                } while (p.AcceptSym(","));
            }
            int n = 0;
            foreach (var vals in data)
            {
                n++;
                if (vals.Count != cols.Count)
                    throw new SqlError(1136, "21S01", "Column count doesn't match value count at row " + n,
                        "A quantidade de valores (" + vals.Count + ") é diferente da quantidade de colunas (" + cols.Count + "). Liste as colunas: INSERT INTO tabela (col1, col2) VALUES (v1, v2);");
                var row = new object[t.Cols.Count];
                for (int k = 0; k < t.Cols.Count; k++) row[k] = t.Cols[k].Default;
                for (int k = 0; k < cols.Count; k++)
                    row[cols[k]] = vals[k] == DefaultMark ? t.Cols[cols[k]].Default : Coerce(t.Cols[cols[k]], vals[k], n);
                for (int k = 0; k < t.Cols.Count; k++)
                {
                    var c = t.Cols[k];
                    if (c.Auto)
                    {
                        if (row[k] == null || (decimal)row[k] == 0) row[k] = (decimal)t.NextId;
                        t.NextId = Math.Max(t.NextId, (int)(decimal)row[k] + 1);
                    }
                    else if (row[k] == null && c.NotNull && !cols.Contains(k))
                        throw new SqlError(1364, "HY000", "Field '" + c.Name + "' doesn't have a default value", "A coluna '" + c.Name + "' é obrigatória e não tem valor padrão: inclua-a na lista de colunas do INSERT.");
                }
                CheckRow(t, row, null, n);
                t.Rows.Add(row);
            }
            return new SqlResult { Kind = "insert", Modified = n > 0, Affected = n, Table = t.Name,
                Message = "Query OK, " + n + (n == 1 ? " row" : " rows") + " affected (0.01 sec)" + (n > 1 ? "\nRecords: " + n + "  Duplicates: 0  Warnings: 0" : "") };
        }

        static readonly object DefaultMark = new object();

        SqlResult Update(Parser p)
        {
            p.Expect("UPDATE");
            var t = Table(p.Ident());
            string alias = p.OptAlias() ?? t.Name;
            p.Expect("SET");
            var sets = new List<KeyValuePair<int, Ex>>();
            do
            {
                var target = p.ParseColRef();
                int k = t.ColIndex(target.Name);
                if (k < 0 || (target.Tab != null && !target.Tab.Equals(alias, StringComparison.OrdinalIgnoreCase))) throw UnknownCol(target.Name, "field list");
                p.ExpectSym("=");
                sets.Add(new KeyValuePair<int, Ex>(k, p.ParseExpr()));
            } while (p.AcceptSym(","));
            Ex where = p.Accept("WHERE") ? p.ParseExpr() : null;
            int limit = p.Accept("LIMIT") ? p.Int() : -1;
            var schema = new List<SqlSrc> { new SqlSrc { Alias = alias, T = t } };
            if (where != null) Validate(where, new SqlCtx { Schema = schema }, "where clause");
            foreach (var s in sets) Validate(s.Value, new SqlCtx { Schema = schema }, "field list");
            int matched = 0, changed = 0;
            var olds = new List<object[]>();
            foreach (var row in t.Rows.ToList())
            {
                if (limit >= 0 && matched >= limit) break;
                var ctx = new SqlCtx { B = new List<SqlSrc> { new SqlSrc { Alias = alias, T = t, Row = row } }, Schema = schema };
                if (where != null && !Truthy(Eval(where, ctx))) continue;
                matched++;
                var before = (object[])row.Clone();
                foreach (var s in sets) row[s.Key] = Coerce(t.Cols[s.Key], Eval(s.Value, ctx), matched);
                bool diff = false;
                for (int k = 0; k < row.Length; k++) if (Key(row[k]) != Key(before[k]) || (row[k] is decimal && before[k] is decimal && row[k].ToString() != before[k].ToString())) diff = true;
                if (diff) { changed++; olds.Add(before); CheckRow(t, row, row, matched); }
            }
            if (olds.Count > 0)
            {
                // se mudou a chave referenciada por outra tabela, confere as dependências
                var pkCols = Enumerable.Range(0, t.Cols.Count).Where(k => t.Cols[k].Pk || t.Cols[k].Unique).ToList();
                var changedKeys = olds.Where(o => !t.Rows.Any(r => pkCols.All(k => Key(r[k]) == Key(o[k])))).ToList();
                if (changedKeys.Count > 0) CheckParent(t, changedKeys);
            }
            return new SqlResult { Kind = "update", Modified = changed > 0, Affected = changed, Matched = matched, NoWhere = where == null, Table = t.Name, Warn = LintNull(where),
                Message = "Query OK, " + changed + (changed == 1 ? " row" : " rows") + " affected (0.01 sec)\nRows matched: " + matched + "  Changed: " + changed + "  Warnings: 0" };
        }

        SqlResult Delete(Parser p)
        {
            p.Expect("DELETE"); p.Expect("FROM");
            var t = Table(p.Ident());
            string alias = p.OptAlias() ?? t.Name;
            Ex where = p.Accept("WHERE") ? p.ParseExpr() : null;
            int limit = p.Accept("LIMIT") ? p.Int() : -1;
            var schema = new List<SqlSrc> { new SqlSrc { Alias = alias, T = t } };
            if (where != null) Validate(where, new SqlCtx { Schema = schema }, "where clause");
            var gone = new List<object[]>();
            foreach (var row in t.Rows)
            {
                if (limit >= 0 && gone.Count >= limit) break;
                if (where == null || Truthy(Eval(where, new SqlCtx { B = new List<SqlSrc> { new SqlSrc { Alias = alias, T = t, Row = row } }, Schema = schema }))) gone.Add(row);
            }
            CheckParent(t, gone, true, 0);
            t.Rows.RemoveAll(r => gone.Contains(r));
            int n = gone.Count;
            return new SqlResult { Kind = "delete", Modified = n > 0, Affected = n, Matched = n, NoWhere = where == null, Table = t.Name, Warn = LintNull(where),
                Message = "Query OK, " + n + (n == 1 ? " row" : " rows") + " affected (0.01 sec)" };
        }

        SqlResult Truncate(Parser p)
        {
            p.Expect("TRUNCATE"); p.Accept("TABLE");
            var t = Table(p.Ident());
            if (Tables.Values.Any(o => o != t && o.Cols.Any(c => c.RefTable != null && c.RefTable.Equals(t.Name, StringComparison.OrdinalIgnoreCase))))
                throw new SqlError(1701, "42000", "Cannot truncate a table referenced in a foreign key constraint", "Outra tabela depende desta (chave estrangeira), então ela não pode ser esvaziada de uma vez.");
            int n = t.Rows.Count;
            t.Rows.Clear(); t.NextId = 1;
            return new SqlResult { Kind = "delete", Modified = n > 0, Affected = n, NoWhere = true, Table = t.Name, Message = "Query OK, 0 rows affected (0.02 sec)" };
        }

        // ───────────────────────────── DDL ─────────────────────────────
        SqlResult Create(Parser p)
        {
            p.Expect("CREATE");
            bool orReplace = false;
            if (p.Accept("OR")) { p.Expect("REPLACE"); orReplace = true; }
            if (p.Accept("VIEW")) return CreateView(p, orReplace);
            if (orReplace) throw Syntax(p.Src, p.Peek.Pos);
            bool unique = p.Accept("UNIQUE");
            if (p.Accept("INDEX") || p.Accept("KEY"))
            {
                string name = p.Ident();
                p.Expect("ON");
                var t = Table(p.Ident());
                var cols = p.ColList();
                AddIndex(t, name, cols, unique);
                return new SqlResult { Kind = "index", Modified = true, Table = t.Name, Message = "Query OK, 0 rows affected (0.03 sec)\nRecords: 0  Duplicates: 0  Warnings: 0" };
            }
            if (unique) throw Syntax(p.Src, p.Peek.Pos);
            if (p.Accept("DATABASE") || p.Accept("SCHEMA"))
                throw new SqlError(1044, "42000", "Access denied for user 'estagio'@'%' to database '" + p.Ident() + "'", "Estagiários não podem criar bancos de dados novos neste servidor. Use o banco " + DbName + ".");
            p.Expect("TABLE");
            bool ifNot = false;
            if (p.Accept("IF")) { p.Expect("NOT"); p.Expect("EXISTS"); ifNot = true; }
            string tn = p.Ident();
            if (NameTaken(tn))
            {
                if (ifNot) { p.SkipRest(); return new SqlResult { Kind = "create", Message = "Query OK, 0 rows affected, 1 warning (0.00 sec)" }; }
                throw new SqlError(1050, "42S01", "Table '" + tn + "' already exists", "Já existe uma tabela (ou view) chamada '" + tn + "'. Veja a estrutura dela com DESCRIBE " + tn + ";");
            }
            // CREATE TABLE nova LIKE outra: copia só a estrutura
            if (p.Accept("LIKE"))
            {
                var src = Table(p.Ident());
                var copy = src.Clone();
                copy.Name = tn; copy.Rows.Clear(); copy.NextId = 1;
                Tables[tn] = copy;
                return new SqlResult { Kind = "create", Modified = true, Table = tn, Message = "Query OK, 0 rows affected (0.02 sec)" };
            }
            // CREATE TABLE nova AS SELECT ...: cria a tabela com o resultado (cópia/backup). Chaves e índices NÃO são copiados.
            if (p.Accept("AS") || p.Is("SELECT") || (p.IsSym("(") && p.PeekAt(1).U == "SELECT"))
            {
                var q = p.ParseSelect();
                var r = Select(q, null, p);
                if (r.Headers.Distinct(StringComparer.OrdinalIgnoreCase).Count() != r.Headers.Count)
                    throw new SqlError(1060, "42S21", "Duplicate column name", "Duas colunas do SELECT têm o mesmo nome: dê apelidos diferentes com AS.");
                var nt2 = Materialize(tn, r, null);
                // colunas copiadas direto de uma tabela mantêm o tipo, o NOT NULL e o DEFAULT de lá
                var srcs = new List<SqlTable>();
                if (q.Union == null && q.From != null) { srcs.Add(Source(q.From)); foreach (var j in q.Joins) srcs.Add(Source(j.Table)); }
                for (int k = 0; k < nt2.Cols.Count; k++)
                {
                    var it = q.Union == null && k < q.Items.Count && !(q.Items.Any(i => i.E is EStar)) ? q.Items[k] : null;
                    string srcName = it == null ? r.Headers[k] : (it.E is ECol ? ((ECol)it.E).Name : null);
                    if (srcName == null) continue;
                    var orig = srcs.Select(s => s.ColIndex(srcName) >= 0 ? s.Cols[s.ColIndex(srcName)] : null).FirstOrDefault(c => c != null);
                    if (orig == null) continue;
                    var c2 = orig.Copy();
                    c2.Name = nt2.Cols[k].Name; c2.Pk = false; c2.Auto = false; c2.Unique = false; c2.RefTable = null; c2.RefCol = null; c2.OnDelete = null; c2.FkName = null; c2.CheckEx = null;
                    if (orig.Pk) c2.NotNull = true;
                    if (orig.Auto) c2.Default = 0m;
                    nt2.Cols[k] = c2;
                }
                Tables[tn] = nt2;
                int n = nt2.Rows.Count;
                return new SqlResult { Kind = "create", Modified = true, Table = tn, Affected = n,
                    Message = "Query OK, " + n + (n == 1 ? " row" : " rows") + " affected (0.02 sec)\nRecords: " + n + "  Duplicates: 0  Warnings: 0" };
            }
            var nt = new SqlTable { Name = tn };
            var checks = new List<KeyValuePair<string, Ex>>();
            p.ExpectSym("(");
            do
            {
                string w = p.Peek.U;
                string cname = null;
                if (w == "CONSTRAINT")
                {
                    p.Next();
                    if ((p.Peek.T == SqlTT.Word || p.Peek.T == SqlTT.Quoted) && p.Peek.U != "PRIMARY" && p.Peek.U != "FOREIGN" && p.Peek.U != "UNIQUE" && p.Peek.U != "CHECK") cname = p.Next().S;
                    w = p.Peek.U;
                }
                if (w == "PRIMARY")
                {
                    p.Next(); p.Expect("KEY");
                    foreach (var cn in p.ColList())
                    {
                        int k = nt.ColIndex(cn);
                        if (k < 0) throw new SqlError(1072, "42000", "Key column '" + cn + "' doesn't exist in table", "A coluna '" + cn + "' da chave primária não foi declarada.");
                        if (nt.Cols.Any(c => c.Pk) && !nt.Cols[k].Pk) throw new SqlError(1068, "42000", "Multiple primary key defined", "Uma tabela só pode ter UMA chave primária.");
                        nt.Cols[k].Pk = true; nt.Cols[k].NotNull = true;
                    }
                }
                else if (w == "FOREIGN")
                {
                    p.Next(); p.Expect("KEY");
                    if (!p.IsSym("(")) cname = cname ?? p.Ident();
                    var cs = p.ColList();
                    p.Expect("REFERENCES");
                    string rt = p.Ident();
                    var rc = p.ColList();
                    string act = FkActions(p);
                    bool self = rt.Equals(tn, StringComparison.OrdinalIgnoreCase);
                    if (!Tables.ContainsKey(rt) && !self) throw new SqlError(1824, "HY000", "Failed to open the referenced table '" + rt + "'", "A tabela referenciada '" + rt + "' não existe.");
                    int k = nt.ColIndex(cs[0]);
                    if (k < 0) throw new SqlError(1072, "42000", "Key column '" + cs[0] + "' doesn't exist in table", "A coluna '" + cs[0] + "' não foi declarada.");
                    if (act == "SET NULL" && nt.Cols[k].NotNull)
                        throw new SqlError(1830, "HY000", "Column '" + cs[0] + "' cannot be NOT NULL: needed in a foreign key constraint SET NULL", "ON DELETE SET NULL precisa de uma coluna que aceite NULL (tire o NOT NULL).");
                    nt.Cols[k].RefTable = self ? tn : Tables[rt].Name; nt.Cols[k].RefCol = rc[0]; nt.Cols[k].OnDelete = act; nt.Cols[k].FkName = cname;
                }
                else if (w == "UNIQUE" || w == "INDEX" || w == "KEY")
                {
                    p.Next();
                    bool u = w == "UNIQUE";
                    if (u) { if (!p.Accept("KEY")) p.Accept("INDEX"); }
                    string iname = p.IsSym("(") ? cname : p.Ident();
                    var cs = p.ColList();
                    if (u && cs.Count == 1 && iname == null) { int k = nt.ColIndex(cs[0]); if (k >= 0) nt.Cols[k].Unique = true; }
                    else nt.Indexes.Add(new SqlIndex { Name = iname ?? cs[0], Cols = cs, Unique = u });
                }
                else if (w == "CHECK")
                {
                    p.Next(); p.ExpectSym("(");
                    var e = p.ParseExpr();
                    p.ExpectSym(")");
                    checks.Add(new KeyValuePair<string, Ex>(cname, e));
                }
                else
                {
                    var col = ColumnDef(p);
                    if (nt.ColIndex(col.Name) >= 0) throw new SqlError(1060, "42S21", "Duplicate column name '" + col.Name + "'", "A coluna '" + col.Name + "' foi declarada duas vezes.");
                    if (col.Pk && nt.Cols.Any(c => c.Pk)) throw new SqlError(1068, "42000", "Multiple primary key defined", "Uma tabela só pode ter UMA chave primária.");
                    if (col.RefTable != null)
                    {
                        if (col.RefTable.Equals(tn, StringComparison.OrdinalIgnoreCase)) col.RefTable = tn;
                        else if (!Tables.ContainsKey(col.RefTable)) throw new SqlError(1824, "HY000", "Failed to open the referenced table '" + col.RefTable + "'", "A tabela referenciada '" + col.RefTable + "' não existe.");
                        else col.RefTable = Tables[col.RefTable].Name;
                    }
                    if (col.CheckEx != null) { checks.Add(new KeyValuePair<string, Ex>(null, col.CheckEx)); col.CheckEx = null; }
                    nt.Cols.Add(col);
                }
            } while (p.AcceptSym(","));
            p.ExpectSym(")");
            p.SkipRest();
            if (nt.Cols.Count(c => c.Auto) > 1 || nt.Cols.Any(c => c.Auto && !c.Pk && !c.Unique))
                throw new SqlError(1075, "42000", "Incorrect table definition; there can be only one auto column and it must be defined as a key",
                    "AUTO_INCREMENT só funciona numa coluna que é chave (normalmente a PRIMARY KEY), e só pode haver uma.");
            var vctx = new SqlCtx { Schema = new List<SqlSrc> { new SqlSrc { Alias = tn, T = nt } } };
            int nchk = 0;
            foreach (var ck in checks)
            {
                Validate(ck.Value, vctx, "check constraint");
                nchk++;
                nt.Checks.Add(new KeyValuePair<string, Ex>(ck.Key ?? tn + "_chk_" + nchk, ck.Value));
            }
            Tables[tn] = nt;
            return new SqlResult { Kind = "create", Modified = true, Table = tn, Message = "Query OK, 0 rows affected (0.02 sec)" };
        }

        // CREATE [OR REPLACE] VIEW nome [(colunas)] AS SELECT ...
        SqlResult CreateView(Parser p, bool orReplace)
        {
            string vn = p.Ident();
            List<string> cols = null;
            if (p.IsSym("(")) cols = p.ColList();
            p.Expect("AS");
            int start = p.Peek.Pos;
            var q = p.ParseSelect();
            int end = p.Peek.Pos;
            string sql = p.Src.Substring(start, Math.Max(0, end - start)).Trim();
            if (Tables.ContainsKey(vn)) throw new SqlError(1050, "42S01", "Table '" + vn + "' already exists", "Já existe uma TABELA chamada '" + vn + "'. Escolha outro nome para a view (é comum começar com vw_).");
            if (Views.ContainsKey(vn) && !orReplace) throw new SqlError(1050, "42S01", "Table '" + vn + "' already exists", "A view '" + vn + "' já existe. Para trocar a consulta dela, use CREATE OR REPLACE VIEW.");
            var r = Select(q, null, p);   // valida a consulta agora (colunas, tabelas...)
            if (cols != null && cols.Count != r.Headers.Count)
                throw new SqlError(1353, "HY000", "In definition of view, derived table or common table expression, SELECT list and column names list have different column counts",
                    "A lista de nomes da view tem " + cols.Count + " colunas, mas o SELECT traz " + r.Headers.Count + ".");
            var names = cols ?? r.Headers;
            if (names.Distinct(StringComparer.OrdinalIgnoreCase).Count() != names.Count)
                throw new SqlError(1060, "42S21", "Duplicate column name", "A view teria duas colunas com o mesmo nome: dê apelidos diferentes com AS.");
            Views[vn] = new SqlView { Name = vn, Sql = sql, Cols = cols };
            return new SqlResult { Kind = "create", Modified = true, Table = vn, Message = "Query OK, 0 rows affected (0.01 sec)" };
        }

        // ON DELETE / ON UPDATE: devolve a ação do ON DELETE (CASCADE, SET NULL, RESTRICT, NO ACTION) ou null
        static string FkActions(Parser p)
        {
            string del = null;
            while (p.Peek.U == "ON")
            {
                p.Next();
                bool isDel = p.Accept("DELETE");
                if (!isDel) p.Expect("UPDATE");
                string act;
                if (p.Accept("SET")) { if (p.Accept("NULL")) act = "SET NULL"; else { p.Expect("DEFAULT"); act = "SET DEFAULT"; } }
                else if (p.Accept("NO")) { p.Expect("ACTION"); act = "NO ACTION"; }
                else if (p.Accept("CASCADE")) act = "CASCADE";
                else { p.Expect("RESTRICT"); act = "RESTRICT"; }
                if (isDel) del = act;
            }
            return del;
        }

        static readonly Dictionary<string, string> TypeNames = new Dictionary<string, string>
        {
            { "INT", "INT" }, { "INTEGER", "INT" }, { "BIGINT", "BIGINT" }, { "SMALLINT", "SMALLINT" }, { "TINYINT", "TINYINT" }, { "MEDIUMINT", "INT" },
            { "DECIMAL", "DECIMAL" }, { "NUMERIC", "DECIMAL" }, { "FLOAT", "FLOAT" }, { "DOUBLE", "DOUBLE" }, { "REAL", "DOUBLE" },
            { "VARCHAR", "VARCHAR" }, { "CHAR", "CHAR" }, { "TEXT", "TEXT" }, { "DATE", "DATE" }, { "DATETIME", "DATETIME" }, { "TIMESTAMP", "TIMESTAMP" },
            { "BOOLEAN", "BOOLEAN" }, { "BOOL", "BOOLEAN" }
        };

        SqlCol ColumnDef(Parser p)
        {
            var c = new SqlCol { Name = p.Ident() };
            var tt = p.Next();
            string tn;
            if (tt.T != SqlTT.Word || !TypeNames.TryGetValue(tt.U, out tn)) throw Syntax(p.Src, tt.Pos);
            c.Type = tn;
            if (p.AcceptSym("("))
            {
                c.Len = p.Int();
                if (p.AcceptSym(",")) c.Scale = p.Int();
                p.ExpectSym(")");
            }
            if (c.Type == "DECIMAL") { if (c.Len < 0) c.Len = 10; if (c.Scale < 0) c.Scale = 0; }
            if (c.Type == "VARCHAR" && c.Len < 0) throw Syntax(p.Src, p.Peek.Pos);
            p.Accept("UNSIGNED");
            while (true)
            {
                if (p.Accept("NOT")) { p.Expect("NULL"); c.NotNull = true; }
                else if (p.Accept("NULL")) { }
                else if (p.Accept("PRIMARY")) { p.Expect("KEY"); c.Pk = true; c.NotNull = true; }
                else if (p.Accept("AUTO_INCREMENT")) c.Auto = true;
                else if (p.Accept("UNIQUE")) { p.Accept("KEY"); c.Unique = true; }
                else if (p.Accept("DEFAULT"))
                {
                    if (p.Accept("CURRENT_TIMESTAMP") || p.Accept("NOW")) { if (p.AcceptSym("(")) p.ExpectSym(")"); c.Default = "2026-10-07 10:30:00"; }
                    else c.Default = Coerce(c, Eval(p.Unary(), new SqlCtx()), 1);
                }
                // como no MySQL 8: REFERENCES escrito na própria coluna é aceito, mas IGNORADO (só FOREIGN KEY (...) REFERENCES ... cria a regra)
                else if (p.Accept("REFERENCES")) { string rt = p.Ident(); var rc = p.ColList(); FkActions(p); c.InlineRef = rt + "(" + rc[0] + ")"; }
                else if (p.Accept("CHECK")) { p.ExpectSym("("); c.CheckEx = p.ParseExpr(); p.ExpectSym(")"); }
                else if (p.Accept("COMMENT")) p.Next();
                else break;
            }
            return c;
        }

        void AddIndex(SqlTable t, string name, List<string> cols, bool unique)
        {
            if (t.Indexes.Any(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) || name.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase))
                throw new SqlError(1061, "42000", "Duplicate key name '" + name + "'", "Já existe um índice chamado '" + name + "' nesta tabela. Veja com SHOW INDEX FROM " + t.Name + ";");
            foreach (var cn in cols)
                if (t.ColIndex(cn) < 0) throw new SqlError(1072, "42000", "Key column '" + cn + "' doesn't exist in table", "A coluna '" + cn + "' não existe em " + t.Name + ". Confira com DESCRIBE " + t.Name + ";");
            var ix = new SqlIndex { Name = name, Unique = unique, Cols = cols.Select(cn => t.Cols[t.ColIndex(cn)].Name).ToList() };
            if (unique)
            {
                var seen = new HashSet<string>();
                foreach (var r in t.Rows)
                {
                    string k = string.Join("\u0001", ix.Cols.Select(cn => Key(r[t.ColIndex(cn)])));
                    if (!seen.Add(k)) throw new SqlError(1062, "23000", "Duplicate entry for key '" + t.Name + "." + name + "'", "Já existem valores repetidos nessa coluna, então o índice não pode ser UNIQUE.");
                }
            }
            t.Indexes.Add(ix);
        }

        SqlResult Drop(Parser p)
        {
            p.Expect("DROP");
            if (p.Accept("INDEX"))
            {
                string name = p.Ident();
                p.Expect("ON");
                var t = Table(p.Ident());
                int n = t.Indexes.RemoveAll(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (n == 0) throw new SqlError(1091, "42000", "Can't DROP '" + name + "'; check that column/key exists", "Não existe índice '" + name + "' em " + t.Name + ".");
                return new SqlResult { Kind = "index", Modified = true, Table = t.Name, Message = "Query OK, 0 rows affected (0.01 sec)" };
            }
            if (p.Accept("DATABASE") || p.Accept("SCHEMA"))
                throw new SqlError(1044, "42000", "Access denied for user 'estagio'@'%' to database '" + p.Ident() + "'", "Ainda bem: estagiários não têm permissão para apagar o banco de dados inteiro!");
            if (p.Accept("VIEW"))
            {
                bool ifv = false;
                if (p.Accept("IF")) { p.Expect("EXISTS"); ifv = true; }
                var vnames = new List<string>();
                do vnames.Add(p.Ident()); while (p.AcceptSym(","));
                foreach (var vn in vnames)
                    if (!Views.ContainsKey(vn) && !ifv)
                        throw new SqlError(1051, "42S02", "Unknown table '" + DbName + "." + vn + "'", Tables.ContainsKey(vn) ? "'" + vn + "' é uma TABELA, não uma view." : "A view '" + vn + "' não existe.");
                foreach (var vn in vnames) Views.Remove(vn);
                return new SqlResult { Kind = "dropview", Modified = true, Table = vnames[0], Message = "Query OK, 0 rows affected (0.01 sec)" };
            }
            p.Expect("TABLE");
            bool ifEx = false;
            if (p.Accept("IF")) { p.Expect("EXISTS"); ifEx = true; }
            var names = new List<string>();
            do names.Add(p.Ident()); while (p.AcceptSym(","));
            string last = null;
            foreach (var n in names)
            {
                SqlTable t;
                if (!Tables.TryGetValue(n, out t))
                {
                    if (ifEx) continue;
                    throw new SqlError(1051, "42S02", "Unknown table '" + DbName + "." + n + "'", "A tabela '" + n + "' não existe.");
                }
                var dep = Tables.Values.FirstOrDefault(o => o != t && !names.Contains(o.Name, StringComparer.OrdinalIgnoreCase) && o.Cols.Any(c => c.RefTable != null && c.RefTable.Equals(t.Name, StringComparison.OrdinalIgnoreCase)));
                if (dep != null)
                    throw new SqlError(3730, "HY000", "Cannot drop table '" + t.Name + "' referenced by a foreign key constraint '" + dep.Name + "_ibfk_1' on table '" + dep.Name + "'.",
                        "A tabela " + dep.Name + " depende de " + t.Name + " (chave estrangeira), então o MySQL não deixa apagá-la.");
                Tables.Remove(n);
                last = t.Name;
            }
            return new SqlResult { Kind = "drop", Modified = last != null, Table = last, Message = "Query OK, 0 rows affected (0.02 sec)" };
        }

        // ALTER TABLE t ação [, ação ...]
        SqlResult Alter(Parser p)
        {
            p.Expect("ALTER"); p.Expect("TABLE");
            var t = Table(p.Ident());
            SqlResult last = null;
            bool allIndex = true;
            do
            {
                last = AlterAction(p, t);
                if (last.Kind != "index") allIndex = false;
            } while (p.AcceptSym(","));
            if (!allIndex) last.Kind = "alter";
            return last;
        }

        SqlResult AlterOk(SqlTable t, string kind)
        {
            return new SqlResult { Kind = kind, Modified = true, Table = t.Name, Message = "Query OK, 0 rows affected (0.02 sec)\nRecords: 0  Duplicates: 0  Warnings: 0" };
        }

        SqlResult AlterAction(Parser p, SqlTable t)
        {
            if (p.Accept("ADD"))
            {
                string cname = null;
                if (p.Accept("CONSTRAINT"))
                {
                    if ((p.Peek.T == SqlTT.Word || p.Peek.T == SqlTT.Quoted) && p.Peek.U != "PRIMARY" && p.Peek.U != "FOREIGN" && p.Peek.U != "UNIQUE" && p.Peek.U != "CHECK") cname = p.Next().S;
                }
                bool unique = p.Accept("UNIQUE");
                if (p.Accept("INDEX") || p.Accept("KEY") || unique)
                {
                    if (unique) { if (!p.Accept("INDEX")) p.Accept("KEY"); }
                    string name = p.IsSym("(") ? cname : p.Ident();
                    var cols = p.ColList();
                    AddIndex(t, name ?? cols[0], cols, unique);
                    return AlterOk(t, "index");
                }
                if (p.Accept("PRIMARY"))
                {
                    p.Expect("KEY");
                    var cols = p.ColList();
                    if (t.Cols.Any(c => c.Pk)) throw new SqlError(1068, "42000", "Multiple primary key defined", "A tabela " + t.Name + " já tem chave primária.");
                    if (cols.Count != 1) throw new SqlError(1235, "42000", "Composite primary keys are not supported by this simulator", "Neste treino a chave primária é de uma coluna só.");
                    int k = t.ColIndex(cols[0]);
                    if (k < 0) throw new SqlError(1072, "42000", "Key column '" + cols[0] + "' doesn't exist in table", "A coluna '" + cols[0] + "' não existe em " + t.Name + ".");
                    if (t.Rows.Any(r => r[k] == null)) throw new SqlError(1138, "22004", "Invalid use of NULL value", "Há linhas com " + cols[0] + " vazio (NULL): a chave primária não aceita NULL.");
                    var seen = new HashSet<string>();
                    foreach (var r in t.Rows) if (!seen.Add(Key(r[k]))) throw new SqlError(1062, "23000", "Duplicate entry '" + Str(r[k]) + "' for key '" + t.Name + ".PRIMARY'", "Há valores repetidos em " + cols[0] + ": a chave primária não aceita repetidos.");
                    t.Cols[k].Pk = true; t.Cols[k].NotNull = true;
                    return AlterOk(t, "alter");
                }
                if (p.Accept("FOREIGN"))
                {
                    p.Expect("KEY");
                    if (!p.IsSym("(")) cname = cname ?? p.Ident();
                    var cs = p.ColList();
                    p.Expect("REFERENCES");
                    string rt = p.Ident();
                    var rc = p.ColList();
                    string act = FkActions(p);
                    SqlTable pt;
                    if (!Tables.TryGetValue(rt, out pt)) throw new SqlError(1824, "HY000", "Failed to open the referenced table '" + rt + "'", "A tabela referenciada '" + rt + "' não existe.");
                    int k = t.ColIndex(cs[0]);
                    if (k < 0) throw new SqlError(1072, "42000", "Key column '" + cs[0] + "' doesn't exist in table", "A coluna '" + cs[0] + "' não existe em " + t.Name + ".");
                    int pk = pt.ColIndex(rc[0]);
                    if (pk < 0) throw new SqlError(1822, "HY000", "Failed to add the foreign key constraint. Missing index for constraint in the referenced table '" + pt.Name + "'", "A coluna '" + rc[0] + "' não existe em " + pt.Name + " (normalmente a referência é a chave primária: id).");
                    if (act == "SET NULL" && t.Cols[k].NotNull)
                        throw new SqlError(1830, "HY000", "Column '" + cs[0] + "' cannot be NOT NULL: needed in a foreign key constraint SET NULL", "ON DELETE SET NULL precisa de uma coluna que aceite NULL.");
                    if (cname != null && t.Cols.Any(c => c.FkName != null && c.FkName.Equals(cname, StringComparison.OrdinalIgnoreCase)))
                        throw new SqlError(1826, "HY000", "Duplicate foreign key constraint name '" + cname + "'", "Já existe uma restrição com o nome " + cname + ".");
                    // as linhas que já existem também precisam respeitar a nova chave
                    foreach (var r in t.Rows)
                        if (r[k] != null && !pt.Rows.Any(x => x[pk] != null && Compare(x[pk], r[k]) == 0))
                            throw new SqlError(1452, "23000", "Cannot add or update a child row: a foreign key constraint fails (`" + DbName + "`.`#sql-1`, CONSTRAINT `" + (cname ?? t.Name + "_ibfk_1") + "` FOREIGN KEY (`" + cs[0] + "`) REFERENCES `" + pt.Name + "` (`" + rc[0] + "`))",
                                "Já existe em " + t.Name + " uma linha com " + cs[0] + " = " + Str(r[k]) + ", que não existe em " + pt.Name + ". Corrija esses dados 'órfãos' antes de criar a chave.");
                    var col = t.Cols[k];
                    col.RefTable = pt.Name; col.RefCol = pt.Cols[pk].Name; col.OnDelete = act; col.FkName = cname;
                    return AlterOk(t, "alter");
                }
                if (p.Accept("CHECK"))
                {
                    p.ExpectSym("(");
                    var e = p.ParseExpr();
                    p.ExpectSym(")");
                    Validate(e, new SqlCtx { Schema = new List<SqlSrc> { new SqlSrc { Alias = t.Name, T = t } } }, "check constraint");
                    string name = cname ?? t.Name + "_chk_" + (t.Checks.Count + 1);
                    if (t.Checks.Any(c => c.Key.Equals(name, StringComparison.OrdinalIgnoreCase)))
                        throw new SqlError(3822, "HY000", "Duplicate check constraint name '" + name + "'.", "Já existe uma restrição CHECK com o nome " + name + ".");
                    var ck = new KeyValuePair<string, Ex>(name, e);
                    t.Checks.Add(ck);
                    try { foreach (var r in t.Rows) CheckConstraints(t, r); }
                    catch (SqlError) { t.Checks.Remove(ck); throw new SqlError(3819, "HY000", "Check constraint '" + name + "' is violated.", "Alguma linha que JÁ existe em " + t.Name + " não passa nessa regra. Corrija os dados antes de criar o CHECK."); }
                    return AlterOk(t, "alter");
                }
                if (cname != null) throw Syntax(p.Src, p.Peek.Pos);
                p.Accept("COLUMN");
                var ncol = ColumnDef(p);
                if (t.ColIndex(ncol.Name) >= 0) throw new SqlError(1060, "42S21", "Duplicate column name '" + ncol.Name + "'", "A tabela " + t.Name + " já tem a coluna '" + ncol.Name + "'.");
                if (ncol.Pk || ncol.Auto) throw new SqlError(1068, "42000", "Multiple primary key defined", "A tabela já tem chave primária.");
                if (ncol.RefTable != null)
                {
                    if (!Tables.ContainsKey(ncol.RefTable)) throw new SqlError(1824, "HY000", "Failed to open the referenced table '" + ncol.RefTable + "'", "A tabela referenciada '" + ncol.RefTable + "' não existe.");
                    ncol.RefTable = Tables[ncol.RefTable].Name;
                }
                if (ncol.NotNull && ncol.Default == null && t.Rows.Count > 0 && !ncol.IsNum)
                    ncol.Default = "";
                var chk = ncol.CheckEx; ncol.CheckEx = null;
                t.Cols.Add(ncol);
                object fill = ncol.Default ?? (ncol.NotNull && ncol.IsNum ? (object)0m : null);
                for (int i = 0; i < t.Rows.Count; i++)
                {
                    var r = t.Rows[i];
                    Array.Resize(ref r, t.Cols.Count);
                    r[t.Cols.Count - 1] = fill;
                    t.Rows[i] = r;
                }
                if (chk != null)
                {
                    var ck = new KeyValuePair<string, Ex>(t.Name + "_chk_" + (t.Checks.Count + 1), chk);
                    t.Checks.Add(ck);
                    foreach (var r in t.Rows) CheckConstraints(t, r);
                }
                return AlterOk(t, "alter");
            }
            if (p.Accept("DROP"))
            {
                if (p.Accept("INDEX") || p.Accept("KEY"))
                {
                    string name = p.Ident();
                    if (t.Indexes.RemoveAll(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) == 0)
                    {
                        int uk = t.ColIndex(name);
                        if (uk >= 0 && t.Cols[uk].Unique && !t.Cols[uk].Pk) t.Cols[uk].Unique = false;
                        else throw new SqlError(1091, "42000", "Can't DROP '" + name + "'; check that column/key exists", "Não existe índice '" + name + "' em " + t.Name + ".");
                    }
                    return AlterOk(t, "index");
                }
                if (p.Accept("FOREIGN"))
                {
                    p.Expect("KEY");
                    string name = p.Ident();
                    var col = t.Cols.FirstOrDefault(c => c.RefTable != null && ((c.FkName != null && c.FkName.Equals(name, StringComparison.OrdinalIgnoreCase)) ||
                                                                               (t.Name + "_ibfk_1").Equals(name, StringComparison.OrdinalIgnoreCase)));
                    if (col == null) throw new SqlError(1091, "42000", "Can't DROP '" + name + "'; check that column/key exists", "Não existe chave estrangeira '" + name + "' em " + t.Name + ". Veja os nomes com SHOW CREATE TABLE " + t.Name + ";");
                    col.RefTable = null; col.RefCol = null; col.OnDelete = null; col.FkName = null;
                    return AlterOk(t, "alter");
                }
                if (p.Accept("CHECK") || p.Accept("CONSTRAINT"))
                {
                    string name = p.Ident();
                    int n = t.Checks.RemoveAll(c => c.Key.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (n == 0)
                    {
                        var col = t.Cols.FirstOrDefault(c => c.FkName != null && c.FkName.Equals(name, StringComparison.OrdinalIgnoreCase));
                        if (col == null) throw new SqlError(3940, "HY000", "Constraint '" + name + "' does not exist.", "Não existe restrição '" + name + "' em " + t.Name + ". Veja com SHOW CREATE TABLE " + t.Name + ";");
                        col.RefTable = null; col.RefCol = null; col.OnDelete = null; col.FkName = null;
                    }
                    return AlterOk(t, "alter");
                }
                if (p.Accept("PRIMARY"))
                {
                    p.Expect("KEY");
                    var pkc = t.Cols.FirstOrDefault(c => c.Pk);
                    if (pkc == null) throw new SqlError(1091, "42000", "Can't DROP 'PRIMARY'; check that column/key exists", "A tabela não tem chave primária.");
                    if (pkc.Auto) throw new SqlError(1075, "42000", "Incorrect table definition; there can be only one auto column and it must be defined as a key", "A coluna AUTO_INCREMENT precisa continuar sendo chave.");
                    pkc.Pk = false;
                    return AlterOk(t, "alter");
                }
                p.Accept("COLUMN");
                string cn = p.Ident();
                int k = t.ColIndex(cn);
                if (k < 0) throw new SqlError(1091, "42000", "Can't DROP '" + cn + "'; check that column/key exists", "A coluna '" + cn + "' não existe em " + t.Name + ".");
                if (t.Cols[k].Pk) throw new SqlError(1829, "HY000", "Cannot drop column '" + cn + "': needed in a foreign key constraint", "Não dá para apagar a chave primária da tabela.");
                ColNotInCheck(t, t.Cols[k].Name);
                t.Cols.RemoveAt(k);
                for (int i = 0; i < t.Rows.Count; i++) t.Rows[i] = t.Rows[i].Where((v, j) => j != k).ToArray();
                foreach (var ix in t.Indexes) ix.Cols.RemoveAll(c => c.Equals(cn, StringComparison.OrdinalIgnoreCase));
                t.Indexes.RemoveAll(ix => ix.Cols.Count == 0);
                return AlterOk(t, "alter");
            }
            // MODIFY coluna nova_definição  /  CHANGE coluna_antiga coluna_nova nova_definição
            bool modify = p.Accept("MODIFY");
            if (modify || p.Accept("CHANGE"))
            {
                p.Accept("COLUMN");
                string oldName = modify ? null : p.Ident();
                var ncol = ColumnDef(p);
                if (modify) oldName = ncol.Name;
                int k = t.ColIndex(oldName);
                if (k < 0) throw UnknownCol(oldName, t.Name);
                if (!oldName.Equals(ncol.Name, StringComparison.OrdinalIgnoreCase) && t.ColIndex(ncol.Name) >= 0)
                    throw new SqlError(1060, "42S21", "Duplicate column name '" + ncol.Name + "'", "A tabela " + t.Name + " já tem a coluna '" + ncol.Name + "'.");
                var old = t.Cols[k];
                // a chave primária, os índices e as chaves estrangeiras são restrições da TABELA: continuam valendo.
                // Já NOT NULL, DEFAULT e AUTO_INCREMENT fazem parte da definição da coluna: se não forem repetidos, somem!
                ncol.Pk = ncol.Pk || old.Pk; if (ncol.Pk) ncol.NotNull = true;
                ncol.Unique = ncol.Unique || old.Unique;
                if (ncol.RefTable == null) { ncol.RefTable = old.RefTable; ncol.RefCol = old.RefCol; ncol.OnDelete = old.OnDelete; ncol.FkName = old.FkName; }
                if (ncol.Auto && !ncol.Pk && !ncol.Unique) throw new SqlError(1075, "42000", "Incorrect table definition; there can be only one auto column and it must be defined as a key", "AUTO_INCREMENT só funciona numa coluna que é chave.");
                ncol.CheckEx = null;
                var vals = new object[t.Rows.Count];
                for (int i = 0; i < t.Rows.Count; i++)
                {
                    var v = t.Rows[i][k];
                    if (v == null && ncol.NotNull) throw new SqlError(1138, "22004", "Invalid use of NULL value", "Há linhas com " + oldName + " vazio (NULL): a coluna não pode virar NOT NULL antes de preencher esses valores.");
                    try { vals[i] = Coerce(ncol, v, i + 1); }
                    catch (SqlError e)
                    {
                        if (e.Code == 1406) throw new SqlError(1265, "01000", "Data truncated for column '" + ncol.Name + "' at row " + (i + 1), "Algum valor que já existe não cabe no novo tipo (" + ncol.TypeText + "). O MySQL recusou a mudança para não perder dados.");
                        throw;
                    }
                }
                for (int i = 0; i < t.Rows.Count; i++) t.Rows[i][k] = vals[i];
                if (!old.Name.Equals(ncol.Name, StringComparison.Ordinal)) RenameColRefs(t, old.Name, ncol.Name);
                t.Cols[k] = ncol;
                return AlterOk(t, "alter");
            }
            // ALTER [COLUMN] c SET DEFAULT v / DROP DEFAULT
            if (p.Accept("ALTER"))
            {
                p.Accept("COLUMN");
                string cn = p.Ident();
                int k = t.ColIndex(cn);
                if (k < 0) throw UnknownCol(cn, t.Name);
                if (p.Accept("SET")) { p.Expect("DEFAULT"); t.Cols[k].Default = Coerce(t.Cols[k], Eval(p.Unary(), new SqlCtx()), 1); }
                else { p.Expect("DROP"); p.Expect("DEFAULT"); t.Cols[k].Default = null; }
                return AlterOk(t, "alter");
            }
            if (p.Accept("RENAME"))
            {
                if (p.Accept("COLUMN"))
                {
                    string a = p.Ident();
                    p.Expect("TO");
                    string b = p.Ident();
                    int k = t.ColIndex(a);
                    if (k < 0) throw UnknownCol(a, t.Name);
                    if (t.ColIndex(b) >= 0 && !a.Equals(b, StringComparison.OrdinalIgnoreCase)) throw new SqlError(1060, "42S21", "Duplicate column name '" + b + "'", "A tabela " + t.Name + " já tem a coluna '" + b + "'.");
                    RenameColRefs(t, t.Cols[k].Name, b);
                    t.Cols[k].Name = b;
                    return AlterOk(t, "alter");
                }
                if (p.Accept("INDEX") || p.Accept("KEY"))
                {
                    string a = p.Ident();
                    p.Expect("TO");
                    string b = p.Ident();
                    var ix = t.Indexes.FirstOrDefault(i => i.Name.Equals(a, StringComparison.OrdinalIgnoreCase));
                    if (ix == null) throw new SqlError(1176, "42000", "Key '" + a + "' doesn't exist in table '" + t.Name + "'", "Não existe índice '" + a + "' em " + t.Name + ".");
                    if (t.Indexes.Any(i => i != ix && i.Name.Equals(b, StringComparison.OrdinalIgnoreCase))) throw new SqlError(1061, "42000", "Duplicate key name '" + b + "'", "Já existe um índice chamado '" + b + "'.");
                    ix.Name = b;
                    return AlterOk(t, "index");
                }
                if (!p.Accept("TO")) p.Accept("AS");
                string nn = p.Ident();
                if (NameTaken(nn)) throw new SqlError(1050, "42S01", "Table '" + nn + "' already exists", "Já existe uma tabela com esse nome.");
                string oldName2 = t.Name;
                Tables.Remove(t.Name); t.Name = nn; Tables[nn] = t;
                foreach (var o in Tables.Values) foreach (var c in o.Cols) if (c.RefTable != null && c.RefTable.Equals(oldName2, StringComparison.OrdinalIgnoreCase)) c.RefTable = nn;
                var r2 = AlterOk(t, "alter"); r2.Message = "Query OK, 0 rows affected (0.01 sec)";
                return r2;
            }
            throw Syntax(p.Src, p.Peek.Pos);
        }

        // ao renomear uma coluna: índices desta tabela e chaves estrangeiras de outras tabelas que apontam para ela
        void RenameColRefs(SqlTable t, string oldName, string newName)
        {
            ColNotInCheck(t, oldName);
            foreach (var ix in t.Indexes)
                for (int i = 0; i < ix.Cols.Count; i++) if (ix.Cols[i].Equals(oldName, StringComparison.OrdinalIgnoreCase)) ix.Cols[i] = newName;
            foreach (var o in Tables.Values)
                foreach (var c in o.Cols)
                    if (c.RefTable != null && c.RefTable.Equals(t.Name, StringComparison.OrdinalIgnoreCase) && c.RefCol != null && c.RefCol.Equals(oldName, StringComparison.OrdinalIgnoreCase)) c.RefCol = newName;
        }

        // como no MySQL 8: uma coluna usada num CHECK não pode ser renomeada nem apagada (apague o CHECK antes)
        static void ColNotInCheck(SqlTable t, string oldName)
        {
            foreach (var ck in t.Checks)
            {
                bool uses = false;
                Walk(ck.Value, x => { var c = x as ECol; if (c != null && c.Name.Equals(oldName, StringComparison.OrdinalIgnoreCase)) uses = true; });
                if (uses) throw new SqlError(3959, "HY000", "Check constraint '" + ck.Key + "' uses column '" + oldName + "', hence column cannot be dropped or renamed.",
                    "A regra CHECK " + ck.Key + " usa a coluna " + oldName + ". Apague o CHECK (ALTER TABLE " + t.Name + " DROP CHECK " + ck.Key + ";) antes de mexer nessa coluna.");
            }
        }

        // ───────────────────────────── SHOW / DESCRIBE / EXPLAIN ─────────────────────────────
        SqlResult Show(Parser p)
        {
            p.Expect("SHOW");
            bool full = p.Accept("FULL");
            if (p.Accept("TABLES"))
            {
                var names = Tables.Values.Select(t => new object[] { t.Name, "BASE TABLE" }).Concat(Views.Values.Select(v => new object[] { v.Name, "VIEW" }))
                                  .OrderBy(r => (string)r[0], StringComparer.OrdinalIgnoreCase).ToList();
                if (full) return new SqlResult { Kind = "show", Headers = new List<string> { "Tables_in_" + DbName, "Table_type" }, Rows = names };
                return new SqlResult { Kind = "show", Headers = new List<string> { "Tables_in_" + DbName }, Rows = names.Select(r => new object[] { r[0] }).ToList() };
            }
            if (p.Accept("DATABASES") || p.Accept("SCHEMAS"))
                return new SqlResult { Kind = "show", Headers = new List<string> { "Database" }, Rows = new[] { DbName, "information_schema", "mysql", "performance_schema", "sys" }.OrderBy(n => n).Select(n => new object[] { n }).ToList() };
            if (p.Accept("COLUMNS") || p.Accept("FIELDS")) { if (!p.Accept("FROM")) p.Expect("IN"); return Describe(p.Ident()); }
            if (p.Accept("INDEX") || p.Accept("INDEXES") || p.Accept("KEYS"))
            {
                if (!p.Accept("FROM")) p.Expect("IN");
                var t = Table(p.Ident());
                var rows = new List<object[]>();
                var pk = t.Cols.FirstOrDefault(c => c.Pk);
                if (pk != null) rows.Add(new object[] { t.Name, 0m, "PRIMARY", 1m, pk.Name });
                foreach (var c in t.Cols.Where(q => q.Unique && !q.Pk)) rows.Add(new object[] { t.Name, 0m, c.Name, 1m, c.Name });
                foreach (var c in t.Cols.Where(q => q.RefTable != null && !t.Indexes.Any(i => i.Cols[0].Equals(q.Name, StringComparison.OrdinalIgnoreCase)))) rows.Add(new object[] { t.Name, 1m, c.FkName ?? c.Name, 1m, c.Name });
                foreach (var ix in t.Indexes) for (int k = 0; k < ix.Cols.Count; k++) rows.Add(new object[] { t.Name, ix.Unique ? 0m : 1m, ix.Name, (decimal)(k + 1), ix.Cols[k] });
                return new SqlResult { Kind = "show", Headers = new List<string> { "Table", "Non_unique", "Key_name", "Seq_in_index", "Column_name" }, Rows = rows };
            }
            if (p.Accept("CREATE"))
            {
                if (p.Accept("VIEW"))
                {
                    string vn = p.Ident();
                    SqlView v;
                    if (!Views.TryGetValue(vn, out v)) throw new SqlError(1146, "42S02", "Table '" + DbName + "." + vn + "' doesn't exist", "A view '" + vn + "' não existe. Veja com SHOW FULL TABLES;");
                    return new SqlResult { Kind = "show", Headers = new List<string> { "View", "Create View" },
                        Rows = new List<object[]> { new object[] { v.Name, "CREATE VIEW `" + v.Name + "`" + (v.Cols != null ? " (" + string.Join(", ", v.Cols) + ")" : "") + " AS " + v.Sql } } };
                }
                p.Expect("TABLE");
                var t = Table(p.Ident());
                return new SqlResult { Kind = "show", Headers = new List<string> { "Table", "Create Table" }, Rows = new List<object[]> { new object[] { t.Name, CreateText(t) } } };
            }
            throw Syntax(p.Src, p.Peek.Pos);
        }

        static string CreateText(SqlTable t)
        {
            var parts = new List<string>();
            foreach (var c in t.Cols)
                parts.Add("  `" + c.Name + "` " + c.TypeText + (c.NotNull ? " NOT NULL" : "") + (c.Auto ? " AUTO_INCREMENT" : "") +
                          (c.Default != null ? " DEFAULT " + (c.Default is decimal ? Str(c.Default) : "'" + c.Default + "'") : ""));
            var pk = t.Cols.FirstOrDefault(c => c.Pk);
            if (pk != null) parts.Add("  PRIMARY KEY (`" + pk.Name + "`)");
            foreach (var c in t.Cols.Where(q => q.Unique && !q.Pk)) parts.Add("  UNIQUE KEY `" + c.Name + "` (`" + c.Name + "`)");
            foreach (var ix in t.Indexes) parts.Add("  " + (ix.Unique ? "UNIQUE " : "") + "KEY `" + ix.Name + "` (" + string.Join(",", ix.Cols.Select(c => "`" + c + "`")) + ")");
            int n = 0;
            foreach (var c in t.Cols.Where(q => q.RefTable != null))
                parts.Add("  CONSTRAINT `" + (c.FkName ?? t.Name + "_ibfk_" + (++n)) + "` FOREIGN KEY (`" + c.Name + "`) REFERENCES `" + c.RefTable + "` (`" + c.RefCol + "`)" +
                          (c.OnDelete != null && c.OnDelete != "RESTRICT" && c.OnDelete != "NO ACTION" ? " ON DELETE " + c.OnDelete : ""));
            foreach (var ck in t.Checks) parts.Add("  CONSTRAINT `" + ck.Key + "` CHECK (" + ck.Value.Text + ")");
            return "CREATE TABLE `" + t.Name + "` (\n" + string.Join(",\n", parts) + "\n) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4";
        }

        SqlResult Describe(string name)
        {
            var t = Source(name);
            var rows = new List<object[]>();
            foreach (var c in t.Cols)
            {
                string key = c.Pk ? "PRI" : c.Unique ? "UNI" : (c.RefTable != null || t.Indexes.Any(i => i.Cols[0].Equals(c.Name, StringComparison.OrdinalIgnoreCase))) ? "MUL" : "";
                rows.Add(new object[] { c.Name, c.TypeText, c.NotNull ? "NO" : "YES", key, c.Default == null ? null : Str(c.Default), c.Auto ? "auto_increment" : "" });
            }
            return new SqlResult { Kind = "show", Headers = new List<string> { "Field", "Type", "Null", "Key", "Default", "Extra" }, Rows = rows, Table = t.Name };
        }

        // plano de execução simplificado: mostra se a consulta usa índice ou varre a tabela toda
        SqlResult Explain(SSelect q)
        {
            Select(q, null, null);   // valida a consulta
            if (q.Union != null) q = q.Union[0];
            if (q.From == null) return new SqlResult { Kind = "explain", Headers = new List<string> { "id", "select_type", "table", "type", "possible_keys", "key", "rows", "Extra" }, Rows = new List<object[]> { new object[] { 1m, "SIMPLE", null, null, null, null, null, "No tables used" } } };
            var t = Source(q.From);
            var eqCols = new List<string>();
            Walk(q.Where, x =>
            {
                var b = x as EBin;
                if (b == null || b.Op != "=") return;
                var c = (b.L as ECol) ?? (b.R as ECol);
                if (c != null && (b.L is ELit || b.R is ELit)) eqCols.Add(c.Name);
            });
            string key = null, type = "ALL";
            var pk = t.Cols.FirstOrDefault(c => c.Pk);
            if (pk != null && eqCols.Any(c => c.Equals(pk.Name, StringComparison.OrdinalIgnoreCase))) { key = "PRIMARY"; type = "const"; }
            else
            {
                // índice composto: vale a regra do "prefixo da esquerda" (a 1ª coluna do índice precisa estar no filtro)
                var ix = t.Indexes.Where(i => eqCols.Any(c => c.Equals(i.Cols[0], StringComparison.OrdinalIgnoreCase)))
                                  .OrderByDescending(i => i.Cols.TakeWhile(ic => eqCols.Any(c => c.Equals(ic, StringComparison.OrdinalIgnoreCase))).Count()).FirstOrDefault();
                if (ix != null) { key = ix.Name; type = "ref"; }
                else
                {
                    var fk = t.Cols.FirstOrDefault(c => (c.Unique || c.RefTable != null) && eqCols.Any(e => e.Equals(c.Name, StringComparison.OrdinalIgnoreCase)));
                    if (fk != null) { key = fk.Name; type = "ref"; }
                }
            }
            int rows = type == "ALL" ? t.Rows.Count : type == "const" ? 1 : Math.Max(1, t.Rows.Count / 4);
            return new SqlResult { Kind = "explain", Headers = new List<string> { "id", "select_type", "table", "type", "possible_keys", "key", "rows", "Extra" },
                Rows = new List<object[]> { new object[] { 1m, "SIMPLE", t.Name, type, key, key, (decimal)rows, q.Where == null ? null : type == "ALL" ? "Using where" : null } } };
        }

        // ───────────────────────────── saída no estilo do cliente mysql ─────────────────────────────
        public static string Cell(object v) { return v == null ? "NULL" : Str(v).Replace("\n", " "); }

        public static string Format(SqlResult r)
        {
            if (r.Headers == null) return r.Message ?? "Query OK";
            var w = r.Headers.Select(h => h.Length).ToArray();
            var numeric = new bool[w.Length];
            for (int k = 0; k < w.Length; k++) numeric[k] = r.Rows.Count > 0 && r.Rows.All(x => x[k] == null || x[k] is decimal);
            foreach (var row in r.Rows) for (int k = 0; k < w.Length; k++) w[k] = Math.Max(w[k], Cell(row[k]).Length);
            var sb = new StringBuilder();
            string sep = "+" + string.Join("+", w.Select(x => new string('-', x + 2))) + "+";
            if (r.Rows.Count == 0) return "Empty set (0.00 sec)";
            sb.AppendLine(sep);
            sb.AppendLine("| " + string.Join(" | ", r.Headers.Select((h, k) => h.PadRight(w[k]))) + " |");
            sb.AppendLine(sep);
            foreach (var row in r.Rows)
                sb.AppendLine("| " + string.Join(" | ", row.Select((v, k) => numeric[k] ? Cell(v).PadLeft(w[k]) : Cell(v).PadRight(w[k]))) + " |");
            sb.AppendLine(sep);
            sb.Append(r.Rows.Count + (r.Rows.Count == 1 ? " row" : " rows") + " in set (0.00 sec)");
            return sb.ToString();
        }

        // ───────────────────────────── léxico ─────────────────────────────
        public static List<SqlTok> Lex(string s)
        {
            var res = new List<SqlTok>();
            int i = 0;
            while (i < s.Length)
            {
                char c = s[i];
                if (char.IsWhiteSpace(c)) { i++; continue; }
                if (c == '-' && i + 1 < s.Length && s[i + 1] == '-') { while (i < s.Length && s[i] != '\n') i++; continue; }
                if (c == '#') { while (i < s.Length && s[i] != '\n') i++; continue; }
                if (c == '/' && i + 1 < s.Length && s[i + 1] == '*') { int e = s.IndexOf("*/", i + 2); i = e < 0 ? s.Length : e + 2; continue; }
                int st = i;
                if (c == '\'' || c == '"')
                {
                    var sb = new StringBuilder();
                    i++;
                    bool closed = false;
                    while (i < s.Length)
                    {
                        if (s[i] == '\\' && i + 1 < s.Length) { char n = s[i + 1]; sb.Append(n == 'n' ? '\n' : n == 't' ? '\t' : n); i += 2; continue; }
                        if (s[i] == c) { if (i + 1 < s.Length && s[i + 1] == c) { sb.Append(c); i += 2; continue; } closed = true; i++; break; }
                        sb.Append(s[i++]);
                    }
                    if (!closed) throw Syntax(s, st);
                    res.Add(new SqlTok { T = SqlTT.Str, S = sb.ToString(), Pos = st, EndPos = i });
                    continue;
                }
                if (c == '`')
                {
                    int e = s.IndexOf('`', i + 1);
                    if (e < 0) throw Syntax(s, st);
                    res.Add(new SqlTok { T = SqlTT.Quoted, S = s.Substring(i + 1, e - i - 1), Pos = st, EndPos = e + 1 });
                    i = e + 1;
                    continue;
                }
                if (char.IsDigit(c) || (c == '.' && i + 1 < s.Length && char.IsDigit(s[i + 1])))
                {
                    while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.')) i++;
                    if (i < s.Length && (char.IsLetter(s[i]) || s[i] == '_'))
                    {
                        while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '_')) i++;
                        res.Add(new SqlTok { T = SqlTT.Word, S = s.Substring(st, i - st), Pos = st, EndPos = i });
                        continue;
                    }
                    res.Add(new SqlTok { T = SqlTT.Num, S = s.Substring(st, i - st), Pos = st, EndPos = i });
                    continue;
                }
                if (char.IsLetter(c) || c == '_' || c == '@')
                {
                    while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '_' || s[i] == '$' || s[i] == '@')) i++;
                    res.Add(new SqlTok { T = SqlTT.Word, S = s.Substring(st, i - st), Pos = st, EndPos = i });
                    continue;
                }
                string two = i + 1 < s.Length ? s.Substring(i, 2) : "";
                if (two == "<=" || two == ">=" || two == "<>" || two == "!=" || two == "||" || two == "&&")
                {
                    res.Add(new SqlTok { T = SqlTT.Sym, S = two, Pos = st, EndPos = i + 2 });
                    i += 2;
                    continue;
                }
                if ("(),.;*+-/%=<>".IndexOf(c) >= 0) { res.Add(new SqlTok { T = SqlTT.Sym, S = c.ToString(), Pos = st, EndPos = i + 1 }); i++; continue; }
                throw Syntax(s, st);
            }
            res.Add(new SqlTok { T = SqlTT.End, S = "", Pos = s.Length, EndPos = s.Length });
            return res;
        }

        // ───────────────────────────── sintaxe ─────────────────────────────
        public static readonly HashSet<string> Reserved = new HashSet<string>
        {
            "SELECT", "FROM", "WHERE", "GROUP", "BY", "HAVING", "ORDER", "LIMIT", "OFFSET", "JOIN", "INNER", "LEFT", "RIGHT", "OUTER", "CROSS", "ON", "AS",
            "AND", "OR", "NOT", "NULL", "IS", "IN", "LIKE", "BETWEEN", "DISTINCT", "INSERT", "INTO", "VALUES", "UPDATE", "SET", "DELETE", "CREATE", "TABLE",
            "INDEX", "DROP", "ALTER", "ADD", "UNION", "ASC", "DESC", "CASE", "WHEN", "THEN", "ELSE", "END", "EXISTS", "PRIMARY", "KEY", "DEFAULT", "UNIQUE",
            "REFERENCES", "FOREIGN", "TRUE", "FALSE", "USING", "NATURAL", "DIV", "MOD", "XOR", "ALL", "ANY"
        };

        class Parser
        {
            public readonly string Src;
            readonly List<SqlTok> t;
            int p;
            public Parser(string src, List<SqlTok> toks) { Src = src; t = toks; }

            public SqlTok Peek { get { return t[p]; } }
            public SqlTok PeekAt(int k) { return t[Math.Min(t.Count - 1, p + k)]; }
            public bool AtEnd { get { return t[p].T == SqlTT.End; } }
            public SqlTok Next() { var x = t[p]; if (p < t.Count - 1) p++; return x; }
            public bool Is(string w) { return t[p].T == SqlTT.Word && t[p].U == w; }
            public bool IsSym(string s) { return t[p].T == SqlTT.Sym && t[p].S == s; }
            public bool Accept(string w) { if (Is(w)) { p++; return true; } return false; }
            public bool AcceptSym(string s) { if (IsSym(s)) { p++; return true; } return false; }
            public void Expect(string w) { if (!Accept(w)) throw Syntax(Src, t[p].Pos); }
            public void ExpectSym(string s) { if (!AcceptSym(s)) throw Syntax(Src, t[p].Pos); }
            public void SkipRest() { while (!AtEnd && !IsSym(";")) p++; }

            public string Ident()
            {
                var x = t[p];
                if (x.T == SqlTT.Quoted || (x.T == SqlTT.Word && !Reserved.Contains(x.U))) { p++; return x.S; }
                throw Syntax(Src, x.Pos);
            }

            public int Int()
            {
                var x = t[p];
                int v;
                if (x.T != SqlTT.Num || !int.TryParse(x.S, NumberStyles.Integer, Inv, out v)) throw Syntax(Src, x.Pos);
                p++;
                return v;
            }

            public List<string> ColList()
            {
                ExpectSym("(");
                var l = new List<string>();
                do { l.Add(Ident()); if (AcceptSym("(")) { Int(); ExpectSym(")"); } Accept("ASC"); Accept("DESC"); } while (AcceptSym(","));
                ExpectSym(")");
                return l;
            }

            public string OptAlias()
            {
                if (Accept("AS")) return Ident();
                var x = t[p];
                if (x.T == SqlTT.Quoted || (x.T == SqlTT.Word && !Reserved.Contains(x.U))) { p++; return x.S; }
                return null;
            }

            public ECol ParseColRef()
            {
                int st = t[p].Pos;
                string a = Ident();
                if (AcceptSym(".")) { string b = Ident(); return new ECol { Tab = a, Name = b, Text = Src.Substring(st, t[p - 1].EndPos - st) }; }
                return new ECol { Name = a, Text = a };
            }

            public SSelect ParseSelect()
            {
                var first = ParseSelectTerm();
                if (!Is("UNION")) return first;
                var u = new SSelect { Union = new List<SSelect> { first }, UnionAll = new List<bool>() };
                while (Accept("UNION"))
                {
                    bool all = Accept("ALL");
                    if (!all) Accept("DISTINCT");
                    u.Union.Add(ParseSelectTerm());
                    u.UnionAll.Add(all);
                }
                // ORDER BY / LIMIT escritos depois da última parte (sem parênteses) valem para o resultado inteiro
                var last = u.Union[u.Union.Count - 1];
                if (!last.Paren)
                {
                    u.Order = last.Order; u.Limit = last.Limit; u.Offset = last.Offset;
                    last.Order = new List<KeyValuePair<Ex, bool>>(); last.Limit = -1; last.Offset = 0;
                }
                else ParseOrderLimit(u);
                for (int i = 0; i < u.Union.Count - 1; i++)
                    if (!u.Union[i].Paren && (u.Union[i].Order.Count > 0 || u.Union[i].Limit >= 0))
                        throw new SqlError(1221, "HY000", "Incorrect usage of UNION and ORDER BY",
                            "Num UNION, o ORDER BY / LIMIT vai uma vez só, no FIM (vale para o resultado inteiro). Para ordenar uma parte, coloque-a entre parênteses.");
                return u;
            }

            void ParseOrderLimit(SSelect q)
            {
                if (Accept("ORDER"))
                {
                    Expect("BY");
                    do
                    {
                        var e = ParseExpr();
                        bool desc = false;
                        if (Accept("DESC")) desc = true; else Accept("ASC");
                        q.Order.Add(new KeyValuePair<Ex, bool>(e, desc));
                    } while (AcceptSym(","));
                }
                if (Accept("LIMIT"))
                {
                    int a = Int();
                    if (AcceptSym(",")) { q.Offset = a; q.Limit = Int(); }
                    else { q.Limit = a; if (Accept("OFFSET")) q.Offset = Int(); }
                }
            }

            SSelect ParseSelectTerm()
            {
                if (AcceptSym("(")) { var inner = ParseSelect(); ExpectSym(")"); inner.Paren = true; return inner; }
                Expect("SELECT");
                var q = new SSelect();
                if (Accept("DISTINCT")) q.Distinct = true; else Accept("ALL");
                do
                {
                    if (IsSym("*")) { Next(); q.Items.Add(new SItem { E = new EStar { Text = "*" } }); continue; }
                    if ((t[p].T == SqlTT.Word || t[p].T == SqlTT.Quoted) && PeekAt(1).S == "." && PeekAt(2).S == "*" && PeekAt(1).T == SqlTT.Sym)
                    {
                        string tab = Next().S; Next(); Next();
                        q.Items.Add(new SItem { E = new EStar { Tab = tab, Text = tab + ".*" } });
                        continue;
                    }
                    var e = ParseExpr();
                    string alias = null;
                    if (Accept("AS")) alias = t[p].T == SqlTT.Str ? Next().S : Ident();
                    else if (t[p].T == SqlTT.Str) alias = Next().S;
                    else if (t[p].T == SqlTT.Quoted || (t[p].T == SqlTT.Word && !Reserved.Contains(t[p].U))) alias = Next().S;
                    q.Items.Add(new SItem { E = e, Alias = alias });
                } while (AcceptSym(","));
                if (Accept("FROM"))
                {
                    q.From = Ident();
                    q.FromAlias = OptAlias();
                    while (true)
                    {
                        string kind = null;
                        if (AcceptSym(",")) kind = "CROSS";
                        else if (Accept("JOIN")) kind = "INNER";
                        else if (Accept("INNER")) { Expect("JOIN"); kind = "INNER"; }
                        else if (Accept("CROSS")) { Expect("JOIN"); kind = "CROSS"; }
                        else if (Accept("LEFT")) { Accept("OUTER"); Expect("JOIN"); kind = "LEFT"; }
                        else if (Accept("RIGHT")) { Accept("OUTER"); Expect("JOIN"); kind = "RIGHT"; }
                        if (kind == null) break;
                        var j = new SJoin { Kind = kind, Table = Ident() };
                        j.Alias = OptAlias();
                        if (Accept("ON")) j.On = ParseExpr();
                        else if (Accept("USING"))
                        {
                            var cols = ColList();
                            string left = q.Joins.Count == 0 ? (q.FromAlias ?? q.From) : (q.Joins[q.Joins.Count - 1].Alias ?? q.Joins[q.Joins.Count - 1].Table);
                            string right = j.Alias ?? j.Table;
                            foreach (var c in cols)
                            {
                                Ex eq = new EBin { Op = "=", L = new ECol { Tab = left, Name = c, Text = c }, R = new ECol { Tab = right, Name = c, Text = c }, Text = c };
                                j.On = j.On == null ? eq : new EBin { Op = "AND", L = j.On, R = eq, Text = "" };
                            }
                        }
                        else if (kind == "LEFT" || kind == "RIGHT") throw Syntax(Src, t[p].Pos);
                        q.Joins.Add(j);
                    }
                }
                if (Accept("WHERE")) q.Where = ParseExpr();
                if (Accept("GROUP")) { Expect("BY"); do q.GroupBy.Add(ParseExpr()); while (AcceptSym(",")); }
                if (Accept("HAVING")) q.Having = ParseExpr();
                ParseOrderLimit(q);
                return q;
            }

            // precedência: OR < XOR < AND < NOT < comparação/IS/IN/LIKE/BETWEEN < + - < * / % < unário
            public Ex ParseExpr() { return Or(); }

            Ex Wrap(Ex e, int st) { e.Text = Src.Substring(st, Math.Max(0, t[Math.Max(0, p - 1)].EndPos - st)).Trim(); return e; }

            Ex Or()
            {
                int st = t[p].Pos;
                var l = And();
                while (Is("OR") || IsSym("||")) { Next(); var r = And(); l = Wrap(new EBin { Op = "OR", L = l, R = r }, st); }
                return l;
            }

            Ex And()
            {
                int st = t[p].Pos;
                var l = Not();
                while (Is("AND") || IsSym("&&")) { Next(); var r = Not(); l = Wrap(new EBin { Op = "AND", L = l, R = r }, st); }
                return l;
            }

            Ex Not()
            {
                int st = t[p].Pos;
                if (Is("NOT") && PeekAt(1).U != "EXISTS") { Next(); return Wrap(new EUn { Op = "NOT", E = Not() }, st); }
                return Comparison();
            }

            Ex Comparison()
            {
                int st = t[p].Pos;
                var l = Additive();
                while (true)
                {
                    if (t[p].T == SqlTT.Sym && (t[p].S == "=" || t[p].S == "<>" || t[p].S == "!=" || t[p].S == "<" || t[p].S == "<=" || t[p].S == ">" || t[p].S == ">="))
                    {
                        string op = Next().S;
                        Ex r;
                        if ((Is("ANY") || Is("ALL") || Is("SOME")) && PeekAt(1).S == "(") throw new SqlError(1235, "42000", "This version of MySQL doesn't yet support 'ANY/ALL' in this simulator", "Use IN (...) ou uma subconsulta com MAX/MIN.");
                        r = Additive();
                        l = Wrap(new EBin { Op = op, L = l, R = r }, st);
                        continue;
                    }
                    if (Is("IS"))
                    {
                        Next();
                        bool not = Accept("NOT");
                        if (Accept("NULL")) { l = Wrap(new EIsNull { E = l, Not = not }, st); continue; }
                        if (Accept("TRUE")) { l = Wrap(not ? (Ex)new EUn { Op = "NOT", E = l } : new EBin { Op = "<>", L = l, R = new ELit { V = 0m } }, st); continue; }
                        if (Accept("FALSE")) { l = Wrap(not ? (Ex)new EBin { Op = "<>", L = l, R = new ELit { V = 0m } } : new EUn { Op = "NOT", E = l }, st); continue; }
                        throw Syntax(Src, t[p].Pos);
                    }
                    bool neg = false;
                    if (Is("NOT") && (PeekAt(1).U == "LIKE" || PeekAt(1).U == "IN" || PeekAt(1).U == "BETWEEN")) { Next(); neg = true; }
                    if (Accept("LIKE")) { var pat = Additive(); l = Wrap(new ELike { E = l, Pat = pat, Not = neg }, st); continue; }
                    if (Accept("IN"))
                    {
                        ExpectSym("(");
                        var e = new EIn { E = l, Not = neg };
                        if (Is("SELECT")) e.Sub = ParseSelect();
                        else { e.List = new List<Ex>(); do e.List.Add(ParseExpr()); while (AcceptSym(",")); }
                        ExpectSym(")");
                        l = Wrap(e, st);
                        continue;
                    }
                    if (Accept("BETWEEN"))
                    {
                        var lo = Additive(); Expect("AND"); var hi = Additive();
                        l = Wrap(new EBetween { E = l, Lo = lo, Hi = hi, Not = neg }, st);
                        continue;
                    }
                    if (neg) throw Syntax(Src, t[p].Pos);
                    return l;
                }
            }

            Ex Additive()
            {
                int st = t[p].Pos;
                var l = Mult();
                while (IsSym("+") || IsSym("-")) { string op = Next().S; var r = Mult(); l = Wrap(new EBin { Op = op, L = l, R = r }, st); }
                return l;
            }

            Ex Mult()
            {
                int st = t[p].Pos;
                var l = Unary();
                while (IsSym("*") || IsSym("/") || IsSym("%") || Is("DIV") || Is("MOD")) { string op = Next().U; var r = Unary(); l = Wrap(new EBin { Op = op, L = l, R = r }, st); }
                return l;
            }

            public Ex Unary()
            {
                int st = t[p].Pos;
                if (AcceptSym("-")) return Wrap(new EUn { Op = "-", E = Unary() }, st);
                if (AcceptSym("+")) return Unary();
                return ParsePrimary();
            }

            public Ex ParsePrimary()
            {
                int st = t[p].Pos;
                var x = t[p];
                if (x.T == SqlTT.Num)
                {
                    p++;
                    decimal d;
                    if (!decimal.TryParse(x.S, NumberStyles.Float, Inv, out d)) throw Syntax(Src, x.Pos);
                    return new ELit { V = d, Text = x.S };
                }
                if (x.T == SqlTT.Str) { p++; return new ELit { V = x.S, Text = Src.Substring(x.Pos, x.EndPos - x.Pos) }; }
                if (AcceptSym("("))
                {
                    if (Is("SELECT")) { var q = ParseSelect(); ExpectSym(")"); return Wrap(new ESub { Q = q }, st); }
                    var e = ParseExpr();
                    ExpectSym(")");
                    return Wrap(e is ELit || e is ECol ? e : e, st);
                }
                if (x.T == SqlTT.Word)
                {
                    string u = x.U;
                    if (u == "NULL") { p++; return new ELit { V = null, Text = "NULL" }; }
                    if (u == "TRUE") { p++; return new ELit { V = 1m, Text = "TRUE" }; }
                    if (u == "FALSE") { p++; return new ELit { V = 0m, Text = "FALSE" }; }
                    if (u == "EXISTS" || (u == "NOT" && PeekAt(1).U == "EXISTS"))
                    {
                        bool not = Accept("NOT");
                        Expect("EXISTS"); ExpectSym("(");
                        var q = ParseSelect();
                        ExpectSym(")");
                        return Wrap(new EExists { Q = q, Not = not }, st);
                    }
                    if (u == "CASE")
                    {
                        p++;
                        var c = new ECase();
                        if (!Is("WHEN")) c.Base = ParseExpr();
                        while (Accept("WHEN")) { c.Whens.Add(ParseExpr()); Expect("THEN"); c.Thens.Add(ParseExpr()); }
                        if (c.Whens.Count == 0) throw Syntax(Src, t[p].Pos);
                        if (Accept("ELSE")) c.Else = ParseExpr();
                        Expect("END");
                        return Wrap(c, st);
                    }
                    if ((u == "CURRENT_DATE" || u == "CURRENT_TIMESTAMP") && PeekAt(1).S != "(") { p++; return new EFunc { Name = u, Text = x.S }; }
                    // INTERVAL 6 MONTH  (usado em DATE_ADD / DATE_SUB e em data + INTERVAL ...)
                    if (u == "INTERVAL" && !(PeekAt(1).T == SqlTT.Sym && PeekAt(1).S == "("))
                    {
                        p++;
                        var n = Unary();
                        var ut = Next();
                        string unit = ut.T == SqlTT.Word ? ut.U : "";
                        if (new[] { "SECOND", "MINUTE", "HOUR", "DAY", "WEEK", "MONTH", "YEAR" }.All(z => z != unit))
                            throw new SqlError(1064, "42000", "You have an error in your SQL syntax near '" + ut.S + "'", "Depois de INTERVAL vem o número e a unidade no singular: INTERVAL 7 DAY, INTERVAL 6 MONTH, INTERVAL 1 YEAR, INTERVAL 30 MINUTE.");
                        var iv = new EFunc { Name = "INTERVAL" };
                        iv.Args.Add(n); iv.Args.Add(new ELit { V = unit, Text = unit });
                        return Wrap(iv, st);
                    }
                    bool fnWord = !Reserved.Contains(u) || u == "LEFT" || u == "RIGHT" || u == "MOD";
                    if (PeekAt(1).T == SqlTT.Sym && PeekAt(1).S == "(" && fnWord)
                    {
                        p += 2;
                        var f = new EFunc { Name = u };
                        if ((u == "TIMESTAMPDIFF" || u == "TIMESTAMPADD") && t[p].T == SqlTT.Word)
                        {
                            // o 1º argumento é a unidade (MINUTE, HOUR, DAY...), não uma coluna
                            var ut = Next();
                            f.Args.Add(new ELit { V = ut.U, Text = ut.S });
                            ExpectSym(",");
                        }
                        if (AcceptSym("*")) f.Star = true;
                        else if (!IsSym(")"))
                        {
                            if (Accept("DISTINCT")) f.Distinct = true;
                            do f.Args.Add(ParseExpr()); while (AcceptSym(","));
                        }
                        ExpectSym(")");
                        if (f.Star && f.Name != "COUNT") throw Syntax(Src, x.Pos);
                        return Wrap(f, st);
                    }
                }
                if (x.T == SqlTT.Quoted || (x.T == SqlTT.Word && !Reserved.Contains(x.U))) return ParseColRef();
                throw Syntax(Src, x.Pos);
            }
        }
    }

    // ───────────────────────────── banco de exemplo: Café 365 ─────────────────────────────
    static class SqlSample
    {
        static SqlDb template;

        public static readonly string[] Script =
        {
            "CREATE TABLE clientes (id INT PRIMARY KEY AUTO_INCREMENT, nome VARCHAR(80) NOT NULL, email VARCHAR(120) UNIQUE, cidade VARCHAR(60), criado_em DATE)",
            "CREATE TABLE produtos (id INT PRIMARY KEY AUTO_INCREMENT, nome VARCHAR(80) NOT NULL, categoria VARCHAR(30) NOT NULL, preco DECIMAL(10,2) NOT NULL, estoque INT NOT NULL DEFAULT 0)",
            "CREATE TABLE pedidos (id INT PRIMARY KEY AUTO_INCREMENT, cliente_id INT NOT NULL, total DECIMAL(10,2) NOT NULL, status VARCHAR(20) NOT NULL DEFAULT 'pendente', data DATE, FOREIGN KEY (cliente_id) REFERENCES clientes(id))",
            "INSERT INTO clientes (nome, email, cidade, criado_em) VALUES " +
                "('Ana Souza', 'ana.souza@email.com', 'São Paulo', '2026-01-15'), ('Bruno Lima', 'bruno.lima@email.com', 'Rio de Janeiro', '2026-02-03'), " +
                "('Carla Dias', 'carla.dias@email.com', 'Belo Horizonte', '2026-02-20'), ('Marcos Rocha', 'marcos.rocha@email.com', 'São Paulo', '2026-03-11'), " +
                "('Mariana Alves', 'mariana.alves@email.com', 'Recife', '2026-04-02'), ('Paulo Nunes', NULL, 'Curitiba', '2026-05-18'), " +
                "('Marta Gomes', 'marta.gomes@email.com', 'São Paulo', '2026-06-09'), ('Rafael Costa', 'rafael.costa@email.com', 'Porto Alegre', '2026-07-21')",
            "INSERT INTO produtos (nome, categoria, preco, estoque) VALUES " +
                "('Café expresso', 'bebida', 6.50, 120), ('Cappuccino', 'bebida', 9.90, 80), ('Pão de queijo', 'salgado', 5.00, 200), ('Croissant', 'salgado', 8.50, 45), " +
                "('Bolo de cenoura', 'doce', 7.00, 30), ('Brigadeiro', 'doce', 3.50, 150), ('Suco de laranja', 'bebida', 8.00, 60), ('Coxinha', 'salgado', 6.00, 0)",
            "INSERT INTO pedidos (cliente_id, total, status, data) VALUES " +
                "(1, 16.40, 'entregue', '2026-09-01'), (2, 9.90, 'entregue', '2026-09-01'), (1, 13.00, 'entregue', '2026-09-02'), (3, 25.40, 'cancelado', '2026-09-03'), " +
                "(4, 6.50, 'entregue', '2026-09-03'), (1, 18.00, 'pendente', '2026-09-04'), (5, 11.50, 'cancelado', '2026-09-05'), (2, 21.40, 'entregue', '2026-09-05'), " +
                "(8, 7.00, 'pendente', '2026-09-06'), (4, 12.00, 'entregue', '2026-09-06')",
            // equipe do café (gerente_id aponta para a própria tabela: quem é o gerente de cada um)
            "CREATE TABLE funcionarios (id INT PRIMARY KEY AUTO_INCREMENT, nome VARCHAR(80) NOT NULL, cargo VARCHAR(30) NOT NULL, salario DECIMAL(10,2) NOT NULL, gerente_id INT, admissao DATE, " +
                "FOREIGN KEY (gerente_id) REFERENCES funcionarios(id))",
            "INSERT INTO funcionarios (nome, cargo, salario, gerente_id, admissao) VALUES " +
                "('Helena Prado', 'gerente', 6500.00, NULL, '2022-03-01'), ('Diego Martins', 'barista', 2800.00, 1, '2024-01-10'), " +
                "('Lúcia Ferraz', 'barista', 3000.00, 1, '2023-06-15'), ('Otávio Reis', 'confeiteiro', 3400.00, 1, '2023-11-20'), " +
                "('Marta Gomes', 'atendente', 2300.00, 2, '2025-08-04'), ('Igor Teles', 'entregador', 2100.00, 1, '2025-02-12'), " +
                "('Caio Mendes', 'entregador', 2100.00, 1, '2025-11-03'), ('Renan Duarte', 'entregador', 2100.00, 1, '2026-09-20')",
            // entregas dos pedidos (o pedido 5 foi retirado no balcão; a entrega do pedido 9 ainda está a caminho: chegada NULL).
            // Obs.: entregador_id ficou SEM chave estrangeira de propósito (desafio de integridade).
            "CREATE TABLE entregas (id INT PRIMARY KEY AUTO_INCREMENT, pedido_id INT NOT NULL, entregador_id INT, saida DATETIME NOT NULL, chegada DATETIME, " +
                "taxa DECIMAL(10,2) NOT NULL DEFAULT 0, FOREIGN KEY (pedido_id) REFERENCES pedidos(id))",
            "INSERT INTO entregas (pedido_id, entregador_id, saida, chegada, taxa) VALUES " +
                "(1, 6, '2026-09-01 10:00:00', '2026-09-01 10:25:00', 5.00), (2, 7, '2026-09-01 11:10:00', '2026-09-01 11:50:00', 7.00), " +
                "(3, 6, '2026-09-02 09:30:00', '2026-09-02 09:45:00', 5.00), (8, 7, '2026-09-05 14:00:00', '2026-09-05 14:30:00', 7.00), " +
                "(10, 6, '2026-09-06 16:20:00', '2026-09-06 16:40:00', 5.00), (9, 7, '2026-09-06 18:05:00', NULL, 8.00)"
        };

        public static SqlDb Fresh()
        {
            if (template == null)
            {
                var db = new SqlDb();
                foreach (var s in Script) db.Execute(s);
                template = db;
            }
            return template.Clone();
        }
    }

    // ───────────────────────────── tarefas da área de SQL ─────────────────────────────
    class SqlTask
    {
        public string Id, Expected, Mode = "query";   // query: compara o resultado; state: compara as tabelas; schema: confere a estrutura
        public bool Ordered;
        public bool Headers;                         // modo query: os nomes das colunas (apelidos com AS) também precisam bater
        public string[] Tables = new string[0];
        public Func<SqlDb, string> Check;            // modo schema: null = ok, SqlTasks.Pending = ainda no meio (neutro), senão a mensagem do que falta
        public bool NeedTx;                          // modo state: as alterações precisam estar numa transação (avaliadas no COMMIT)
    }

    static class SqlTasks
    {
        public const string OK = "OK";
        public const string Pending = "\u0001PENDENTE";   // resposta da função Check: "faltam passos" (não conta erro)
        public static readonly Dictionary<string, SqlTask> All = new Dictionary<string, SqlTask>();

        static string Plural(int n, string um, string varios) { return n + " " + (n == 1 ? um : varios); }

        static List<string> Cells(object[] r) { return r.Select(v => v is decimal ? SqlDb.NormDec(Math.Round((decimal)v, 2)).ToString(SqlDb.Inv) : v == null ? "\0N" : v.ToString()).ToList(); }

        // reordena as colunas do aluno pelos nomes quando elas são as mesmas do esperado
        static List<List<string>> Align(SqlResult got, SqlResult exp)
        {
            var gh = got.Headers.Select(h => h.ToLowerInvariant()).ToList();
            var eh = exp.Headers.Select(h => h.ToLowerInvariant()).ToList();
            var rows = got.Rows.Select(Cells).ToList();
            if (gh.Count == eh.Count && !gh.SequenceEqual(eh) && new HashSet<string>(gh).SetEquals(eh) && gh.Distinct().Count() == gh.Count)
            {
                var map = eh.Select(h => gh.IndexOf(h)).ToList();
                rows = rows.Select(r => map.Select(i => r[i]).ToList()).ToList();
            }
            return rows;
        }

        public static string CompareResult(SqlResult got, SqlResult exp, bool ordered)
        {
            if (got.Headers.Count != exp.Headers.Count)
                return "A consulta rodou, mas trouxe " + Plural(got.Headers.Count, "coluna", "colunas") + " e o pedido são " + Plural(exp.Headers.Count, "coluna", "colunas") +
                       " (" + string.Join(", ", exp.Headers) + "). Releia o objetivo: quais colunas ele pede?";
            var g = Align(got, exp);
            var e = exp.Rows.Select(Cells).ToList();
            Func<List<string>, string> J = r => string.Join("\u0001", r);
            if (g.Count != e.Count)
                return "A consulta rodou, mas trouxe " + Plural(g.Count, "linha", "linhas") + " e o esperado são " + Plural(e.Count, "linha", "linhas") +
                       ". Confira o filtro (WHERE / HAVING / JOIN).";
            var gs = g.Select(J).OrderBy(x => x, StringComparer.Ordinal).ToList();
            var es = e.Select(J).OrderBy(x => x, StringComparer.Ordinal).ToList();
            if (!gs.SequenceEqual(es))
            {
                var gh = new HashSet<string>(got.Headers.Select(h => h.ToLowerInvariant()));
                if (!gh.SetEquals(exp.Headers.Select(h => h.ToLowerInvariant())) && !exp.Headers.Any(h => h.Contains("(")))
                    return "A quantidade de linhas está certa, mas as colunas não são as pedidas (esperado: " + string.Join(", ", exp.Headers) + ").";
                return "A quantidade de linhas está certa, mas os valores não batem com o pedido. Confira as colunas e as condições.";
            }
            if (ordered && !g.Select(J).SequenceEqual(e.Select(J)))
                return "As linhas estão certas, mas a ORDEM não. Confira o ORDER BY (ASC = crescente, DESC = decrescente).";
            return null;
        }

        // nomes das colunas do resultado (quando o desafio pede apelidos com AS)
        public static string CompareHeaders(SqlResult got, SqlResult exp)
        {
            var gh = got.Headers.Select(h => h.ToLowerInvariant()).ToList();
            var eh = exp.Headers.Select(h => h.ToLowerInvariant()).ToList();
            if (new HashSet<string>(gh).SetEquals(eh)) return null;
            var faltam = exp.Headers.Where(h => !gh.Contains(h.ToLowerInvariant())).ToList();
            return "Os valores estão certos, mas os NOMES das colunas não: o pedido é " + string.Join(", ", exp.Headers) +
                   ". Faltou: " + string.Join(", ", faltam) + ". Dê o nome com AS (ex.: AVG(nota) AS media).";
        }

        static List<string> RowKey(SqlTable t, object[] r)
        {
            var l = new List<string>();
            for (int k = 0; k < t.Cols.Count; k++)
                if (!t.Cols[k].Auto) l.Add(r[k] is decimal ? SqlDb.NormDec((decimal)r[k]).ToString(SqlDb.Inv) : SqlDb.Key(r[k]));
            return l;
        }

        public static string CompareState(SqlDb got, SqlDb exp, string[] tables)
        {
            foreach (var tn in tables)
            {
                SqlTable gt, et;
                if (!got.Tables.TryGetValue(tn, out gt)) return "A tabela " + tn + " sumiu!";
                et = exp.Tables[tn];
                if (gt.Cols.Count != et.Cols.Count) return "A estrutura da tabela " + tn + " mudou (colunas diferentes). Este desafio só pede alteração de DADOS.";
                if (gt.Rows.Count != et.Rows.Count)
                    return "A tabela " + tn + " ficou com " + Plural(gt.Rows.Count, "linha", "linhas") + ", mas deveria ficar com " + Plural(et.Rows.Count, "linha", "linhas") + ". Confira o comando e o WHERE.";
                var a = gt.Rows.Select(r => string.Join("\u0001", RowKey(gt, r))).OrderBy(x => x, StringComparer.Ordinal).ToList();
                var b = et.Rows.Select(r => string.Join("\u0001", RowKey(et, r))).OrderBy(x => x, StringComparer.Ordinal).ToList();
                if (!a.SequenceEqual(b)) return "Os dados da tabela " + tn + " não ficaram como o pedido. Confira nomes, valores (números com ponto: 5.50) e o WHERE.";
            }
            return null;
        }

        static readonly HashSet<string> ReadKinds = new HashSet<string> { "select", "show", "use", "tx", "explain" };

        // veredito para um comando já executado: null = neutro (exploração), OK, ou a mensagem do problema
        // banco esperado: o banco de exemplo depois do(s) comando(s) de referência
        public static SqlDb ExpectedDb(SqlTask task)
        {
            var exp = SqlSample.Fresh();
            foreach (var st in SqlDb.Split(task.Expected)) exp.Execute(st);
            return exp;
        }

        static string StateVerdict(SqlTask task, SqlDb db, SqlResult res)
        {
            var exp = ExpectedDb(task);
            var msg = CompareState(db, exp, task.Tables);
            if (msg == null)
            {
                foreach (var other in db.Tables.Keys)
                    if (!task.Tables.Contains(other, StringComparer.OrdinalIgnoreCase) && exp.Tables.ContainsKey(other) && CompareState(db, exp, new[] { other }) != null)
                        return "Você alterou a tabela " + other + ", que não fazia parte do pedido.";
                return OK;
            }
            if ((res.Kind == "update" || res.Kind == "delete") && res.Affected == 0)
                msg = res.Kind == "update" && res.Matched > 0
                    ? "O UPDATE encontrou " + res.Matched + (res.Matched == 1 ? " linha" : " linhas") + ", mas não mudou nada (os valores já eram esses). Confira o SET."
                    : "O comando rodou, mas o WHERE não encontrou NENHUMA linha (0 rows affected). Confira o valor do filtro, as aspas e o nome da coluna.";
            if (res.Warn != null) msg = res.Warn + " " + msg;
            return msg;
        }

        public static string Evaluate(SqlTask task, SqlDb db, SqlResult res, ref SqlResult expectedCache)
        {
            if (res.Kind == "drop")
                return "DROP TABLE apaga a tabela INTEIRA, com todos os dados! Em produção isso derruba o sistema. (Calma: neste treino a tabela foi restaurada.)";
            if ((res.Kind == "update" || res.Kind == "delete") && res.NoWhere && res.Affected > 1 && task.Mode != "query")
                return (res.Kind == "update" ? "UPDATE" : "DELETE") + " SEM WHERE " + (res.Kind == "update" ? "passou por TODAS as " + Math.Max(res.Matched, res.Affected) : "apagou TODAS as " + res.Affected) +
                       " linhas da tabela " + res.Table + "! Esse é um dos erros mais perigosos (e famosos) de SQL. Sempre filtre com WHERE. (A alteração foi desfeita.)";
            if (task.Mode == "query")
            {
                if (res.Kind == "select")
                {
                    if (expectedCache == null) expectedCache = SqlSample.Fresh().Execute(task.Expected);
                    var msg = CompareResult(res, expectedCache, task.Ordered);
                    if (msg == null && task.Headers) msg = CompareHeaders(res, expectedCache);
                    if (msg == null) return OK;
                    if (res.Simple || res.Headers.Count == 0) return null;
                    return res.Warn != null ? res.Warn + " " + msg : msg;
                }
                if (ReadKinds.Contains(res.Kind)) return null;
                return "Este desafio pede só uma CONSULTA (SELECT): nada de alterar o banco. A alteração foi desfeita.";
            }
            if (task.Mode == "state" && task.NeedTx)
            {
                // transação: os comandos ficam pendentes até o COMMIT, e só então o resultado é conferido
                if (res.Kind == "insert" || res.Kind == "update" || res.Kind == "delete")
                {
                    if (!db.InTransaction)
                        return "Este desafio pede as alterações DENTRO de uma transação: comece com START TRANSACTION; (ou BEGIN;), rode os comandos e termine com COMMIT;. (A alteração foi desfeita.)";
                    if (res.Warn != null) return res.Warn + " (O comando foi desfeito; a transação continua aberta.)";
                    return null;
                }
                if (res.Kind == "tx" && res.Tx == "commit")
                {
                    var v = StateVerdict(task, db, res);
                    if (v == OK) return OK;
                    bool undone = db.UndoLastCommit();
                    return "A transação foi confirmada, mas o banco não ficou como o pedido: " + v + (undone ? " (O treino desfez a transação inteira para você tentar de novo.)" : "");
                }
                if (ReadKinds.Contains(res.Kind)) return null;
            }
            if (ReadKinds.Contains(res.Kind)) return null;
            if (task.Mode == "state")
            {
                var v = StateVerdict(task, db, res);
                return v == OK ? OK : v + " (A alteração foi desfeita para você tentar de novo.)";
            }
            var cm = task.Check(db);
            if (cm == Pending) return null;
            return cm == null ? OK : cm + " (A alteração foi desfeita para você tentar de novo.)";
        }
    }
}
