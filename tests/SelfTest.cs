using System.Collections.Generic;
using System;
using System.Linq;

namespace DockerQuest
{
    static class SelfTest
    {
        static int fails = 0;

        static void Expect(int level, string input, bool ok)
        {
            var lv = Levels.All()[level - 1];
            var r = CommandParser.Check(input, lv);
            if (r.Ok != ok)
            {
                fails++;
                Console.WriteLine("FALHA fase " + level + ": '" + input + "' esperado " + ok + " -> " + r.Message);
            }
        }

        static int Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var all = Levels.All();
            for (int i = 0; i < all.Count; i++)
            {
                foreach (var a in all[i].Answers) Expect(i + 1, a, true);
                if (all[i].Explain.Length % 2 != 0) { fails++; Console.WriteLine("Explain ímpar na fase " + (i + 1)); }
                if (all[i].Hints.Length < 2) { fails++; Console.WriteLine("Poucas dicas na fase " + (i + 1)); }
            }

            foreach (var inc in Incidents.All)
            {
                foreach (var a in inc.Level.Answers)
                    if (!CommandParser.Check(a, inc.Level).Ok) { fails++; Console.WriteLine("FALHA incidente " + inc.Id + ": " + a); }
                if (inc.Level.Explain.Length % 2 != 0) { fails++; Console.WriteLine("Explain ímpar no incidente " + inc.Id); }
            }

            // todas as categorias: respostas oficiais passam no verificador geral
            var lista = Challenges.All();
            var porCat = lista.GroupBy(l => l.Cat + ":" + l.Tier).ToDictionary(g => g.Key, g => g.Count());
            foreach (var cat in Challenges.Cats)
                for (int t = 0; t < 3; t++)
                {
                    int n; porCat.TryGetValue(cat.Id + ":" + t, out n);
                    if (n == 0) { fails++; Console.WriteLine("Nível vazio: " + cat.Id + " " + t); }
                }
            foreach (var grp in lista.GroupBy(l => l.Cat))
                foreach (var dup in grp.GroupBy(l => Checker.Fold(l.Title)).Where(g => g.Count() > 1))
                { fails++; Console.WriteLine("Título repetido em " + grp.Key + ": " + dup.Key); }
            for (int i = 0; i < lista.Count; i++)
            {
                var lv = lista[i];
                if (string.IsNullOrEmpty(lv.Title) || string.IsNullOrEmpty(lv.Objective) || string.IsNullOrEmpty(lv.Context) || string.IsNullOrEmpty(lv.Summary) || lv.Answers == null || lv.Answers.Length == 0)
                { fails++; Console.WriteLine("Campos vazios: " + lv.Cat + " '" + lv.Title + "'"); }
                if (lv.Tier < 0 || lv.Tier > 2) { fails++; Console.WriteLine("Nível inválido: " + lv.Cat + " '" + lv.Title + "'"); }
                foreach (var a in lv.Answers)
                    if (lv.Kind != "aws" && lv.Kind != "sql" && !Checker.Check(a, lv).Ok) { fails++; Console.WriteLine("FALHA " + lv.Cat + " '" + lv.Title + "': " + a); }
                if (lv.Explain.Length % 2 != 0) { fails++; Console.WriteLine("Explain ímpar: " + lv.Cat + " " + lv.Title); }
                if (lv.Hints.Length < 2) { fails++; Console.WriteLine("Poucas dicas: " + lv.Cat + " " + lv.Title); }
                if (Checker.Check("xyz qualquer coisa", lv).Ok) { fails++; Console.WriteLine("Aceitou lixo: " + lv.Cat + " " + lv.Title); }
            }
            Func<string, string, Level> L = (cat, title) => lista.First(l => l.Cat == cat && l.Title == title);
            Action<string, string, string, bool> E = (cat, title, input, ok) =>
            {
                var r = Checker.Check(input, L(cat, title));
                if (r.Ok != ok) { fails++; Console.WriteLine("FALHA " + cat + " '" + title + "': '" + input + "' esperado " + ok + " -> " + r.Message); }
            };
            E("front", "Parágrafos azuis", "p{color:blue}", true);
            E("front", "Parágrafos azuis", "p { color: red; }", false);
            E("front", "Imagem acessível", "<img src='logo.png' alt='Logo' />", true);
            E("front", "O título da vitrine", "<H1>Café 365</H1>", true);
            E("python", "Primeiro print", "print('Olá, Micorosoft!')", true);
            E("python", "Primeiro print", "print(\"Ola, Micorosoft\")", false);
            E("php", "Variável de preço", "$preco=9.90", true);
            E("redes", "A porta do cadeado", " 443 ", true);
            E("redes", "A agenda da internet", "dns", true);
            E("redes", "Portas abertas", "netstat -na", true);
            E("react", "Instalando dependências", "NPM INSTALL", true);
            E("flutter", "Em coluna", "Column(children:[a,b])", true);
            // código em várias linhas, como escrito no editor da IDE
            E("python", "Função dobro", "def dobro(n):\n    return n * 2", true);
            E("python", "Função dobro", "def dobro(n):\n    return n * 3", false);
            E("php", "Percorrendo itens", "foreach ($itens as $item) {\n    echo $item;\n}", true);
            E("front", "Galeria em grid", ".galeria {\n    display: grid;\n    grid-template-columns: repeat(3, 1fr);\n}", true);
            E("front", "Arrow function", "const dobro = (n) => {\n    return n * 2\n}", true);
            E("react", "Carregar ao montar", "useEffect(() => {\n    carregar()\n}, [])", true);
            E("flutter", "Função assíncrona", "Future<void> carregar() async {\n}", true);
            Console.WriteLine("  [react/Botão +1 quase] -> " + Checker.Check("<button onClick={() => setContador(contador + 2)}>+1</button>", L("react", "Botão +1")).Message);
            Console.WriteLine("  categorias: " + string.Join(", ", porCat.OrderBy(p => p.Key).Select(p => p.Key + "=" + p.Value)));

            // AWS: cada tarefa precisa ser concluída pelo console (Solve) e, quando houver, pela CLI do CloudShell
            foreach (var lv in lista.Where(l => l.Kind == "aws"))
            {
                AwsTask task;
                if (lv.Task == null || !AwsTasks.All.TryGetValue(lv.Task, out task)) { fails++; Console.WriteLine("Tarefa AWS ausente: " + lv.Title); continue; }
                var sim = new AwsSim(); task.Setup(sim);
                string got = null;
                sim.Event += e => { var r = task.Eval(sim, e); if (r != null && got != AwsTasks.OK) got = r; };
                task.Solve(sim);
                if (got != AwsTasks.OK) { fails++; Console.WriteLine("FALHA AWS (console) '" + lv.Title + "': " + (got ?? "nenhum evento")); }
                if (lv.Answers.Length > 1 && lv.Answers[1].StartsWith("aws "))
                {
                    var sim2 = new AwsSim(); task.Setup(sim2);
                    string got2 = null;
                    sim2.Event += e => { var r = task.Eval(sim2, e); if (r != null && got2 != AwsTasks.OK) got2 = r; };
                    foreach (var part in lv.Answers[1].Split(new[] { "  +  " }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        bool ok;
                        string outp = sim2.Run(part.Trim(), out ok);
                        if (!ok) { fails++; Console.WriteLine("FALHA AWS (CLI) '" + lv.Title + "': " + part + " -> " + outp); }
                    }
                    if (got2 != AwsTasks.OK) { fails++; Console.WriteLine("FALHA AWS (CLI) '" + lv.Title + "': " + (got2 ?? "nenhum evento")); }
                }
            }
            // AWS: erros que precisam ser apontados
            {
                Func<string, Action<AwsSim>, string> Wrong = (taskId, act) =>
                {
                    var tk = AwsTasks.All[taskId]; var sm = new AwsSim(); tk.Setup(sm);
                    string res = null;
                    sm.Event += e => { var r = tk.Eval(sm, e); if (r != null) res = r; };
                    act(sm);
                    return res;
                };
                var w1 = Wrong("a2", s => s.CreateBucket("cafe365-site", "us-east-1", false));
                var w2 = Wrong("b1", s => s.SetState("i-0abc123", "terminate"));
                var w3 = Wrong("b3", s => s.CreateUser("estagiario", "Administradores"));
                var w4 = Wrong("c3", s => s.UpdateService(false, 2));
                var w5 = Wrong("c5", s => s.CreateStack("cafe365", "infra.yaml", false));
                foreach (var w in new[] { w1, w2, w3, w4, w5 })
                    if (w == null || w == AwsTasks.OK) { fails++; Console.WriteLine("FALHA AWS: um erro esperado não foi apontado"); }
                bool okx;
                Console.WriteLine("  [aws CLI inválida] -> " + new AwsSim().Run("aws s3 voar", out okx));
            }

            // SQL: a resposta de referência (e variações equivalentes) precisa concluir a tarefa; erros clássicos precisam ser apontados
            {
                Func<string, string, string> Sq = (taskId, sql) =>
                {
                    var tk = SqlTasks.All[taskId];
                    var db = SqlSample.Fresh();
                    SqlResult cache = null;
                    string last = null;
                    foreach (var st in SqlDb.Split(sql))
                    {
                        SqlResult res;
                        try { res = db.Execute(st); }
                        catch (SqlError e) { return "ERRO " + e.Mysql; }
                        last = SqlTasks.Evaluate(tk, db, res, ref cache);
                        if (last != null) return last;
                    }
                    return last;
                };
                Action<string, string, bool> Q = (taskId, sql, ok) =>
                {
                    string r = Sq(taskId, sql);
                    if ((r == SqlTasks.OK) != ok) { fails++; Console.WriteLine("FALHA SQL " + taskId + ": '" + sql + "' esperado " + (ok ? "OK" : "erro") + " -> " + (r ?? "neutro")); }
                };
                foreach (var lv in lista.Where(l => l.Kind == "sql"))
                {
                    if (lv.Task == null || !SqlTasks.All.ContainsKey(lv.Task)) { fails++; Console.WriteLine("Tarefa SQL ausente: " + lv.Title); continue; }
                    Q(lv.Task, lv.Answers[0], true);
                }
                Q("s1", "select * from PRODUTOS", true);
                Q("s2", "SELECT preco, nome FROM produtos", true);
                Q("s2", "SELECT nome FROM produtos", false);
                Q("s3", "SELECT nome, preco FROM produtos WHERE categoria = 'Bebida';", true);
                Q("s3", "SELECT p.nome, p.preco FROM produtos p WHERE p.categoria IN ('bebida')", true);
                Q("s3", "SELECT nome, preco FROM produtos WHERE categoria = 'doce'", false);
                Q("s4", "SELECT nome, preco FROM produtos ORDER BY preco", false);
                Q("s4", "SELECT nome, preco FROM produtos ORDER BY 2 DESC", true);
                Q("s5", "SELECT COUNT(id) AS total FROM clientes", true);
                Q("s6", "SELECT nome FROM clientes WHERE nome LIKE 'ma%'", true);
                Q("s6", "SELECT nome FROM clientes WHERE nome LIKE '%ma%'", false);
                Q("s7", "INSERT INTO produtos VALUES (NULL, 'Chá gelado', 'bebida', 7.5, 40)", true);
                Q("s7", "INSERT INTO produtos (nome, categoria, preco, estoque) VALUES ('Chá gelado', 'bebida', 75, 40)", false);
                Q("s8", "UPDATE produtos SET preco = 5.50 WHERE id = 3", true);
                Q("s8", "UPDATE produtos SET preco = 5.50", false);
                Q("s8", "SELECT * FROM produtos WHERE id = 3; UPDATE produtos SET preco = 5.5 WHERE nome = 'pão de queijo'", true);
                Q("s9", "DELETE FROM pedidos", false);
                Q("s9", "DELETE FROM pedidos WHERE status <> 'entregue' AND status <> 'pendente'", true);
                Q("s10", "SELECT categoria, COUNT(*) AS qtd FROM produtos GROUP BY 1", true);
                Q("s11", "SELECT clientes.nome, pedidos.total FROM clientes INNER JOIN pedidos ON pedidos.cliente_id = clientes.id", true);
                Q("s11", "SELECT nome, total FROM pedidos, clientes WHERE pedidos.cliente_id = clientes.id", true);
                Q("s12", "SELECT nome FROM clientes WHERE id NOT IN (SELECT cliente_id FROM pedidos)", true);
                Q("s12", "SELECT nome FROM clientes c WHERE NOT EXISTS (SELECT 1 FROM pedidos p WHERE p.cliente_id = c.id)", true);
                Q("s13", "SELECT cliente_id, COUNT(*) FROM pedidos GROUP BY cliente_id HAVING COUNT(*) >= 2", true);
                Q("s13", "SELECT cliente_id, COUNT(*) FROM pedidos GROUP BY cliente_id", false);
                Q("s14", "create table avaliacoes (id int auto_increment, produto_id int, nota int, primary key (id))", true);
                Q("s14", "CREATE TABLE avaliacoes (id INT PRIMARY KEY, produto_id INT, nota INT)", false);
                Q("s15", "ALTER TABLE produtos ADD INDEX idx_produtos_categoria (categoria)", true);
                Q("s15", "CREATE INDEX idx_categoria ON produtos (categoria)", false);
                Q("s15", "CREATE INDEX idx_produtos_categoria ON produtos (nome)", false);
                // erros do MySQL que precisam aparecer
                Func<string, int> Err = sql => { try { SqlSample.Fresh().Execute(sql); return 0; } catch (SqlError e) { return e.Code; } };
                var errs = new[]
                {
                    new KeyValuePair<string, int>("SELECT nome, COUNT(*) FROM produtos", 1140),
                    new KeyValuePair<string, int>("SELECT categoria, nome, COUNT(*) FROM produtos GROUP BY categoria", 1055),
                    new KeyValuePair<string, int>("SELECT * FROM produto", 1146),
                    new KeyValuePair<string, int>("SELECT nme FROM produtos", 1054),
                    new KeyValuePair<string, int>("SELECT id FROM pedidos JOIN clientes ON cliente_id = clientes.id", 1052),
                    new KeyValuePair<string, int>("SELECT * FROM produtos WHERE COUNT(*) > 1", 1111),
                    new KeyValuePair<string, int>("SELEC * FROM produtos", 1064),
                    new KeyValuePair<string, int>("DELETE FROM clientes WHERE id = 1", 1451),
                    new KeyValuePair<string, int>("INSERT INTO pedidos (cliente_id, total) VALUES (99, 10)", 1452),
                    new KeyValuePair<string, int>("INSERT INTO produtos (nome) VALUES ('x')", 1364),
                    new KeyValuePair<string, int>("INSERT INTO produtos (id, nome, categoria, preco) VALUES (1, 'x', 'y', 1)", 1062),
                    new KeyValuePair<string, int>("DROP TABLE clientes", 3730),
                };
                foreach (var kv in errs)
                {
                    int code = Err(kv.Key);
                    if (code != kv.Value) { fails++; Console.WriteLine("FALHA SQL erro: '" + kv.Key + "' esperado " + kv.Value + " -> " + code); }
                }
                var demo = SqlSample.Fresh();
                Console.WriteLine(SqlDb.Format(demo.Execute("SELECT categoria, COUNT(*) AS qtd, AVG(preco) FROM produtos GROUP BY categoria ORDER BY qtd DESC, categoria")));
                Console.WriteLine(SqlDb.Format(demo.Execute("EXPLAIN SELECT * FROM pedidos WHERE cliente_id = 1")));
            }

            // variações equivalentes            Expect(1, "docker -v", true);
            Expect(1, "docker version", false);
            Expect(3, "docker pull nginx:latest", true);
            Expect(3, "docker image pull nginx", true);
            Expect(3, "sudo docker pull nginx", true);
            Expect(4, "docker image list", true);
            Expect(6, "docker container ps", true);
            Expect(7, "docker ps --all", true);
            Expect(7, "docker ps", false);
            Expect(8, "docker run --name web -d nginx", true);
            Expect(8, "docker run --name=web --detach nginx:latest", true);
            Expect(8, "docker container run -d --name web nginx", true);
            Expect(8, "docker run -d nginx --name web", false);
            Expect(9, "docker run -dp 8080:80 --name site nginx", true);
            Expect(9, "docker run --name site -p8080:80 -d nginx", true);
            Expect(9, "docker run -d -p 80:8080 --name site nginx", false);
            Expect(10, "docker run -ti ubuntu bash", true);
            Expect(10, "docker run --interactive --tty ubuntu /bin/bash", true);
            Expect(10, "docker run -it ubuntu", false);
            Expect(11, "docker run --rm alpine echo \"ola\"", true);
            Expect(12, "docker run -d -e MYSQL_ROOT_PASSWORD=segredo --name banco mysql", true);
            Expect(12, "docker run -d --env=MYSQL_ROOT_PASSWORD=segredo --name banco mysql", true);
            Expect(14, "docker run -d --name leve -m 256m --cpus=0.5 nginx", true);
            Expect(17, "docker container restart site", true);
            Expect(19, "docker logs --follow web", true);
            Expect(20, "docker logs site --tail 20", true);
            Expect(20, "docker logs --tail=20 site", true);
            Expect(21, "docker exec -ti web bash", true);
            Expect(21, "docker exec -it web sh", false);
            Expect(22, "docker exec web ls /usr/share/nginx/html/", true);
            Expect(26, "docker cp web:/etc/nginx/nginx.conf ./", true);
            Expect(29, "docker rm --force loja", true);
            Expect(29, "docker container rm -f loja", true);
            Expect(30, "docker build . -t cafeapp:1.0", true);
            Expect(30, "docker build --tag cafeapp:1.0 .", true);
            Expect(30, "docker build -t cafeapp:1.0", false);
            Expect(31, "docker build -t cafeapp:dev -f Dockerfile.dev .", true);
            Expect(38, "docker save cafeapp:1.0 -o cafeapp.tar", true);
            Expect(38, "docker save --output=cafeapp.tar cafeapp:1.0", true);
            Expect(39, "docker load --input cafeapp.tar", true);
            Expect(40, "docker image rm cafeapp:dev", true);
            Expect(43, "docker run -d --name db -v dados:/var/lib/postgresql/data -e POSTGRES_PASSWORD=senha postgres", true);
            Expect(43, "docker run -d --name db --volume dados:/var/lib/postgresql/data/ -e POSTGRES_PASSWORD=senha postgres", true);
            Expect(46, "docker network list", true);
            Expect(47, "docker run -d --name cache --net cafe-net redis", true);
            Expect(47, "docker run -d --name cache --network=cafe-net redis", true);
            Expect(50, "docker-compose up -d", true);
            Expect(50, "docker compose up --detach", true);
            Expect(50, "docker compose up", false);
            Expect(52, "docker compose logs --follow api", true);
            Expect(53, "docker compose up --build -d", true);
            Expect(55, "docker compose up -d --scale=api=3", true);
            Expect(56, "docker compose --file docker-compose.prod.yml up -d", true);
            Expect(56, "docker compose -f docker-compose.prod.yml up", false);
            Expect(57, "docker compose down --volumes", true);
            Expect(59, "docker container prune -f", true);
            Expect(62, "docker system prune --all --force", true);
            Expect(63, "docker run --restart=always -e NODE_ENV=production -p 80:3000 --network cafe-net --name cafe-prod -d micorosoft/cafeapp:1.0", true);

            // Mensagens de diagnóstico
            var lv9 = all[8];
            foreach (var s in new[] { "docker run -d --name site nginx", "docker start site", "docker fly", "docker",
                                      "docker run -d -p 8080:80 --name site httpd", "podman run x", "docker volume" })
                Console.WriteLine("  [" + s + "] -> " + CommandParser.Check(s, lv9).Message);

            // desenho de todos os itens, personagens, móveis e paredes (duas vezes, para pegar objetos em cache descartados)
            try
            {
                var world = new World();
                using (var bmp = new System.Drawing.Bitmap(400, 400))
                using (var g = System.Drawing.Graphics.FromImage(bmp))
                {
                    for (int pass = 0; pass < 2; pass++)
                    {
                        foreach (var it in Catalog.Items)
                        {
                            var equip = new System.Collections.Generic.Dictionary<string, string>();
                            if (it.Slot == "hat" || it.Slot == "acc" || it.Slot == "outfit" || it.Slot == "pet") equip[it.Slot] = it.Id;
                            var look = Catalog.MakeLook(new[] { 2, 1, 1, 0, 0 }, equip, null);
                            for (int fc = 0; fc < 4; fc++)
                            {
                                Art.Character(g, 100, 200, 60, look, fc, fc * 0.7f, fc % 2 == 1, pass);
                                if (look.Pet != null) Art.Pet(g, look.Pet, 200, 200, 60, fc, pass, fc % 2 == 1);
                            }
                            Art.ItemIcon(g, it.Id, new System.Drawing.RectangleF(10, 10, 48, 48));
                        }
                        foreach (var cat in Challenges.Cats)
                            for (int t = 0; t < 3; t++) BadgeForm.DrawBadge(g, 200, 200, 80, cat, t, "Teste", t != 1);
                        foreach (var fu in world.Furniture) Scene.Furniture(g, fu, pass);
                        foreach (var w in Scene.BuildWalls(world)) Scene.DrawWall(g, w);
                        foreach (var d in world.Decor) Scene.Decor(g, d, false);
                        foreach (var d in world.AllDoors) { Scene.Door(g, d, false, 0); Scene.Plate(g, d, false, false); }
                    }
                }
                Console.WriteLine("  desenho: itens, personagens, badges, móveis e paredes ok");
            }
            catch (Exception ex) { fails++; Console.WriteLine("FALHA no desenho: " + ex); }

            Console.WriteLine(fails == 0 ? "OK - " + all.Count + " fases Docker + " + Challenges.All().Count + " desafios no total validados" : fails + " falha(s)");
            return fails == 0 ? 0 : 1;
        }
    }
}
