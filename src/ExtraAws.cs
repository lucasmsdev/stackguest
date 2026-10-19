using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerQuest
{
    static partial class Challenges
    {
        // ───────────── AWS (2ª leva): apoio às tarefas e1–e72 ─────────────
        // Para desfazer um erro do aluno, copiamos o estado inicial de uma conta "nova" (AwsSim recém-criada não dispara eventos).

        static AwsSim.Bucket ExtraAwsSite(AwsSim s) { return MaisAwsBucket(s, "cafe365-site"); }

        static void ExtraAwsFn(AwsSim s, string name)
        {
            var src = new AwsSim().Func(name);
            if (src == null) return;
            var f = s.Func(name);
            if (f == null) { f = new AwsSim.Fn { Name = name }; s.Functions.Add(f); }
            f.Runtime = src.Runtime; f.Timeout = src.Timeout; f.Memory = src.Memory; f.Concurrency = -1;
            f.Env = new Dictionary<string, string>(src.Env);
            f.Triggers.Clear();
        }

        static void ExtraAwsQueue(AwsSim s, string name)
        {
            var q = s.Q(name);
            if (q == null) { q = new AwsSim.Queue { Name = name }; s.Queues.Add(q); }
            q.Visibility = 30; q.Wait = 0; q.Dlq = null; q.MaxReceive = 0; q.Fifo = false; q.Dedup = false;
        }

        static AwsSim.Db ExtraAwsDb(AwsSim s, string id)
        {
            var d = s.DbI(id);
            if (d == null) { d = new AwsSim.Db { Id = id }; s.Dbs.Add(d); }
            d.Engine = "MySQL 8.0.39"; d.Class = id == "banco-dev" ? "db.t3.micro" : "db.t3.medium"; d.Status = "Disponível";
            d.Public = false; d.MultiAz = false; d.Protect = false; d.Backup = 1; d.Pending.Clear();
            return d;
        }

        // usuário IAM (e suas chaves) como no início
        static void ExtraAwsUser(AwsSim s, string name)
        {
            var fresh = new AwsSim();
            var src = fresh.Users.FirstOrDefault(u => u.Name == name);
            if (src == null) return;
            s.Users.RemoveAll(u => u.Name == name);
            s.AccessKeys.RemoveAll(k => k.User == name);
            s.Users.Add(new AwsSim.User { Name = src.Name, Group = src.Group, Mfa = src.Mfa, Created = src.Created, Policies = src.Policies.ToList() });
            foreach (var k in fresh.AccessKeys.Where(k => k.User == name)) s.AccessKeys.Add(new AwsSim.AccessKey { Id = k.Id, User = k.User, Status = "Active", Created = k.Created });
        }

        // registros DNS como no início (os que foram excluídos voltam; os alterados voltam ao valor original)
        static void ExtraAwsRecords(AwsSim s, params string[] keep)
        {
            foreach (var r in new AwsSim().Records)
            {
                if (keep.Contains(r.Name)) continue;
                var ex = s.Records.FirstOrDefault(x => x.Name == r.Name && x.Type == r.Type);
                if (ex == null) s.Records.Add(new AwsSim.DnsRecord { Name = r.Name, Type = r.Type, Value = r.Value, Ttl = r.Ttl });
                else { ex.Value = r.Value; ex.Ttl = r.Ttl; }
            }
        }

        static void ExtraAwsAlarm(AwsSim s, string name)
        {
            var a = new AwsSim().Alarms.FirstOrDefault(x => x.Name == name);
            if (a != null && !s.Alarms.Any(x => x.Name == name)) s.Alarms.Add(a);
        }

        static void ExtraAwsVolume(AwsSim s, string id, string type, int size)
        {
            var v = s.Vol(id);
            if (v != null) { v.Type = type; v.Size = size; }
        }

        static void ExtraAwsEips(AwsSim s)
        {
            s.Eips.RemoveAll(x => x.Alloc != "eipalloc-0web0001" && x.Alloc != "eipalloc-0f4ee01");
            var w = s.EipOf("eipalloc-0web0001");
            if (w == null) { w = new AwsSim.Eip { Alloc = "eipalloc-0web0001", Ip = "54.210.12.8", Region = "us-east-1" }; s.Eips.Add(w); }
            w.Instance = "i-0web001";
            var f = s.EipOf("eipalloc-0f4ee01");
            if (f == null) { f = new AwsSim.Eip { Alloc = "eipalloc-0f4ee01", Ip = "34.192.7.21", Region = "us-east-1" }; s.Eips.Add(f); }
            f.Instance = null;
            var web = MaisAwsResetInst(s, "i-0web001");
            web.Ip = "54.210.12.8";
            MaisAwsResetInst(s, "i-0int555");
        }

        static void ExtraAwsRoutes(AwsSim s)
        {
            foreach (var r in new AwsSim().RouteTables)
            {
                var ex = s.RouteTables.FirstOrDefault(x => x.Id == r.Id);
                if (ex == null) s.RouteTables.Add(r); else { ex.Routes.Clear(); ex.Routes.AddRange(r.Routes); }
            }
        }

        static void ExtraAwsWinSg(AwsSim s)
        {
            var g = s.Sg("sg-0win444");
            if (g == null) { g = new AwsSim.SecGroup { Id = "sg-0win444", Name = "windows-sg", Description = "Servidor Windows do financeiro", Region = "us-east-1", Vpc = "vpc-0cafe365" }; s.SecGroups.Add(g); }
            g.Inbound.Clear();
            g.Inbound.Add(new AwsSim.Rule { Port = 3389, Source = "0.0.0.0/0" });
            g.Inbound.Add(new AwsSim.Rule { Port = 3389, Source = "200.155.80.10/32" });
        }

        static void ExtraAwsPromo(AwsSim s)
        {
            var b = s.Bkt("cafe365-promo");
            if (b == null) { b = new AwsSim.Bucket { Name = "cafe365-promo", Region = "us-east-1", Created = DateTime.Now.AddDays(-40) }; s.Buckets.Add(b); }
            b.Objects.Clear(); b.Objects.Add("banner-blackfriday.png"); b.Objects.Add("promo.html");
            b.BlockPublic = false;
            b.Policy = "publica";
            b.PolicyText = "{\"Version\":\"2012-10-17\",\"Statement\":[{\"Sid\":\"LeituraPublica\",\"Effect\":\"Allow\",\"Principal\":\"*\",\"Action\":\"s3:GetObject\",\"Resource\":\"arn:aws:s3:::cafe365-promo/*\"}]}";
        }

        static string ExtraAwsTagsOf(string e, string prefix) { return e.Length > prefix.Length ? e.Substring(prefix.Length) : ""; }

        static Dictionary<string, string> ExtraAwsKv(string s)
        {
            var d = new Dictionary<string, string>();
            foreach (var part in (s ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int i = part.IndexOf('=');
                if (i > 0) d[part.Substring(0, i)] = part.Substring(i + 1);
            }
            return d;
        }

        static void ExtraAwsTasks()
        {
            const string OK = AwsTasks.OK;
            var T = AwsTasks.All;

            // ═══════════════ Iniciante ═══════════════

            // e1 — tag no bucket
            T["e1"] = new AwsTask
            {
                Setup = s => { ExtraAwsSite(s).Tags.Clear(); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("s3:tags:")) return null;
                    var p = e.Split(':');
                    var b = s.Bkt(p[2]);
                    if (p[2] != "cafe365-site") { if (b != null) b.Tags.Clear(); return "Você etiquetou o bucket " + p[2] + " (tags removidas). O pedido é o cafe365-site."; }
                    string v;
                    if (b.Tags.TryGetValue("Projeto", out v))
                    {
                        if (v == "cafe365") return OK;
                        b.Tags.Clear();
                        return "A tag Projeto ficou com o valor '" + v + "'. O relatório espera exatamente cafe365 (tags foram removidas).";
                    }
                    var quase = b.Tags.Keys.FirstOrDefault(k => k.Trim().ToLowerInvariant() == "projeto");
                    b.Tags.Clear();
                    if (quase != null) return "A chave ficou '" + quase + "'. Tags diferenciam maiúsculas e minúsculas: o relatório procura exatamente Projeto (tags removidas).";
                    if (p.Length > 3 && p[3].Length == 0) return null;
                    return "A tag pedida é Projeto = cafe365 (tags removidas).";
                },
                Solve = s => s.SetBucketTags("cafe365-site", new Dictionary<string, string> { { "Projeto", "cafe365" } })
            };

            // e2 — tópico SNS
            T["e2"] = new AwsTask
            {
                Setup = s => { s.Topics.RemoveAll(t => t.Name == "alertas-ti" || t.Name == "alertas-ti.fifo"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("sns:create:")) return null;
                    var p = e.Split(':');
                    if (p[2] == "alertas-ti") return OK;
                    s.Topics.RemoveAll(t => t.Name == p[2]);
                    if (p[2] == "alertas-ti.fifo") return "Tópico FIFO não entrega para e-mail nem SMS (só para filas SQS FIFO). Para alertas da equipe, crie um tópico Padrão chamado alertas-ti (o FIFO foi excluído).";
                    return "Tópico criado como '" + p[2] + "' (excluído). O nome combinado com a equipe é alertas-ti.";
                },
                Solve = s => s.CreateTopic("alertas-ti", false)
            };

            // e3 — fila SQS
            T["e3"] = new AwsTask
            {
                Setup = s => { s.Queues.RemoveAll(q => q.Name.StartsWith("emails-pendentes")); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("sqs:create:")) return null;
                    var p = e.Split(':');
                    if (p[2] == "emails-pendentes") return OK;
                    s.Queues.RemoveAll(q => q.Name == p[2]);
                    if (p[2] == "emails-pendentes.fifo") return "Não precisa de FIFO: a ordem dos e-mails não importa e a fila Padrão tem vazão quase ilimitada. Crie a fila Padrão emails-pendentes (a FIFO foi excluída).";
                    return "Fila criada como '" + p[2] + "' (excluída). O nome pedido é emails-pendentes.";
                },
                Solve = s => s.CreateQueue("emails-pendentes", false, false, 30, 0)
            };

            // e4 — filtrar ERROR nos logs
            T["e4"] = new AwsTask
            {
                Setup = s => { },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("logs-filter:")) return null;
                    string rest = e.Substring("logs-filter:".Length);
                    int i = rest.LastIndexOf(':');
                    string grp = rest.Substring(0, i), pat = rest.Substring(i + 1).Trim();
                    if (pat.Length == 0) return null;
                    if (grp != "/aws/lambda/pedidos") return "Você filtrou o grupo " + grp + ". Quem está dando erro é a função pedidos: o grupo dela é /aws/lambda/pedidos.";
                    if (pat.Contains("ERROR")) return OK;
                    if (pat.ToUpperInvariant().Contains("ERROR")) return "O filtro '" + pat + "' não achou nada: os padrões de filtro do CloudWatch diferenciam maiúsculas de minúsculas. As linhas de erro têm [ERROR].";
                    return "O filtro '" + pat + "' não mostra os erros. Procure pelo termo ERROR.";
                },
                Solve = s => s.FilterLogs("/aws/lambda/pedidos", "ERROR")
            };

            // e5 — explorar itens do DynamoDB
            T["e5"] = new AwsTask
            {
                Setup = s => { },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ddb:scan:")) return null;
                    string t = e.Substring("ddb:scan:".Length);
                    return t == "cardapio" ? OK : "Essa é a tabela " + t + ". Os preços ficam na tabela cardapio.";
                },
                Solve = s => { List<string> it; s.Scan("cardapio", out it); }
            };

            // e6 — interromper o banco de testes
            T["e6"] = new AwsTask
            {
                Setup = s => { ExtraAwsDb(s, "banco-cafe"); ExtraAwsDb(s, "banco-dev"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("rds:")) return null;
                    var p = e.Split(':');
                    if (p.Length < 3) return null;
                    if (p[1] == "stop" && p[2] == "banco-dev") return OK;
                    if (p[2] == "banco-cafe" && (p[1] == "stop" || p[1] == "reboot" || p[1] == "modify")) { ExtraAwsDb(s, "banco-cafe"); return "Você mexeu no banco-cafe, o banco de PRODUÇÃO: os pedidos do Café 365 pararam! (já voltamos). Só o banco-dev deve ser interrompido."; }
                    if (p[1] == "modify" || p[1] == "reboot") { ExtraAwsDb(s, p[2]); return "A tarefa é só interromper o banco-dev (alteração desfeita)."; }
                    return null;
                },
                Solve = s => s.DbAction("banco-dev", "stop")
            };

            // e7 — tempo limite da Lambda
            T["e7"] = new AwsTask
            {
                Setup = s => { foreach (var f in s.Functions.ToList()) ExtraAwsFn(s, f.Name); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("lambda:config:")) return null;
                    var p = e.Split(':');
                    string fn = p[2], field = p[3];
                    if (fn != "notificacoes") { ExtraAwsFn(s, fn); return "Você alterou a função " + fn + " (desfeito). Quem estoura o tempo é a notificacoes."; }
                    if (field != "timeout") { ExtraAwsFn(s, fn); return "Só o tempo limite precisa mudar (" + (field == "memory" ? "memória" : "runtime") + " desfeito)."; }
                    int v = int.Parse(p[4]);
                    if (v == 30) return OK;
                    ExtraAwsFn(s, fn);
                    return v < 30 ? "Com " + v + " s ainda não dá: o servidor de e-mail leva até 20 s para responder. O pedido é 30 segundos (desfeito)."
                                  : "O pedido é 30 segundos. Tempo limite alto demais deixa execuções travadas rodando (e cobrando) por muito tempo (desfeito).";
                },
                Solve = s => s.UpdateFnConfig("notificacoes", 30, -1, null)
            };

            // e8 — proteção contra encerramento
            T["e8"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; foreach (var i in s.Instances) i.Protect = false; MaisAwsResetInst(s, "i-0web001"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:")) return null;
                    var p = e.Split(':');
                    if (p[1] == "protect")
                    {
                        if (p[2] == "i-0web001") return p[3] == "on" ? OK : null;
                        var i = s.Inst(p[2]); if (i != null) i.Protect = false;
                        return "Você mexeu na proteção de " + MaisAwsInstName(s, p[2]) + " (desfeito). O pedido é proteger o web-1 (i-0web001).";
                    }
                    if (p[1] == "terminate" || p[1] == "stop") { string n = MaisAwsInstName(s, p[2]); MaisAwsResetInst(s, p[2]); return "Você desligou " + n + " (já religamos). A tarefa é só ativar a proteção contra encerramento do web-1."; }
                    return null;
                },
                Solve = s => s.SetProtection("i-0web001", true)
            };

            // e9 — par de chaves ED25519
            T["e9"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; s.KeyPairs.RemoveAll(k => k.Name == "rodrigo-ssh"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:keypair:")) return null;
                    var p = e.Split(':');
                    var k = s.KeyPairs.LastOrDefault(x => x.Name == p[2]);
                    string region = k != null ? k.Region : "us-east-1";
                    if (p[2] != "rodrigo-ssh") { s.KeyPairs.Remove(k); return "Par de chaves criado como '" + p[2] + "' (excluído). O nome pedido é rodrigo-ssh."; }
                    if (region != "us-east-1") { s.KeyPairs.Remove(k); return "O par de chaves foi criado em " + region + ". Pares de chaves são REGIONAIS: os servidores do Rodrigo ficam no Norte da Virgínia (us-east-1). Excluído: crie de novo na região certa."; }
                    if (p[3] != "ed25519") { s.KeyPairs.Remove(k); return "O tipo ficou RSA. O pedido é ED25519 (chave menor, mais rápida e moderna). Excluído: crie de novo com o tipo certo."; }
                    return OK;
                },
                Solve = s => s.CreateKeyPair("rodrigo-ssh", "ed25519")
            };

            // e10 — excluir alarme de recurso que não existe mais
            T["e10"] = new AwsTask
            {
                Setup = s => { ExtraAwsAlarm(s, "cpu-servidor-legado"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("cw:delete:")) return null;
                    string n = e.Substring("cw:delete:".Length);
                    if (n == "cpu-servidor-legado") return OK;
                    ExtraAwsAlarm(s, n);
                    return "Você excluiu o alarme " + n + ", que ainda monitora um recurso ativo (restaurado). O alarme fantasma é o cpu-servidor-legado.";
                },
                Solve = s => s.DeleteAlarm("cpu-servidor-legado")
            };

            // e11 — invalidação da CloudFront
            T["e11"] = new AwsTask
            {
                Setup = s => { var d = s.Distr("E1CAFE365SITE"); if (d != null) d.Invalidations.Clear(); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("cf:invalidate:")) return null;
                    var p = e.Split(':');
                    var paths = p[3].Split(',');
                    if (paths.Length == 1 && paths[0] == "/index.html") return OK;
                    if (paths.Any(x => x.EndsWith("*"))) return "Com o curinga (" + paths.First(x => x.EndsWith("*")) + ") TODO o cache foi descartado: as imagens pesadas voltam a ser baixadas da origem por todos os visitantes. O pedido era só /index.html.";
                    if (paths.Contains("/index.html")) return "Além de /index.html você invalidou outros caminhos (" + string.Join(", ", paths.Where(x => x != "/index.html")) + "). Só a página inicial mudou.";
                    return "O arquivo alterado é /index.html (você invalidou " + string.Join(", ", paths) + ").";
                },
                Solve = s => s.Invalidate("E1CAFE365SITE", new List<string> { "/index.html" })
            };

            // e12 — site estático
            T["e12"] = new AwsTask
            {
                Setup = s => { var b = ExtraAwsSite(s); b.IndexDoc = null; b.ErrorDoc = null; },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("s3:website:")) return null;
                    var p = e.Split(':');
                    var b = s.Bkt(p[2]);
                    if (p[2] != "cafe365-site") { if (b != null) { b.IndexDoc = null; b.ErrorDoc = null; } return "Você configurou o site no bucket " + p[2] + " (desfeito). O site fica no cafe365-site."; }
                    if (p[3].Length == 0) return null;
                    if (p[3] != "index.html") { b.IndexDoc = null; b.ErrorDoc = null; return "O documento de índice ficou '" + p[3] + "'. A página inicial do site é o arquivo index.html (desfeito)."; }
                    return OK;
                },
                Solve = s => s.SetWebsite("cafe365-site", "index.html", null)
            };

            // e13 — desativar chave vazada
            T["e13"] = new AwsTask
            {
                Setup = s => { ExtraAwsUser(s, "rodrigo"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("iam:")) return null;
                    var p = e.Split(':');
                    if (p[1] == "key-status")
                    {
                        if (p[2] == "rodrigo" && p[3] == "AKIA5RODRIGO7Q2XW3LM") return p[4] == "Inactive" ? OK : null;
                        var k = s.AccessKeys.FirstOrDefault(x => x.Id == p[3]); if (k != null) k.Status = "Active";
                        return "Você desativou a chave de " + p[2] + ", que não vazou (reativada). A chave do print é a AKIA5RODRIGO7Q2XW3LM, do rodrigo.";
                    }
                    if (p[1] == "key-delete") { ExtraAwsUser(s, p[2]); return "Não exclua ainda: a Helena precisa investigar no CloudTrail o que foi feito com a chave. DESATIVAR corta o acesso na hora e ainda dá para reativar (chave restaurada)."; }
                    if (p[1] == "delete") { ExtraAwsUser(s, p[2]); return "Você excluiu o usuário " + p[2] + " inteiro (restaurado). Basta desativar a chave vazada."; }
                    if (p[1] == "key-create") return null;
                    return null;
                },
                Solve = s => s.SetKeyStatus("rodrigo", "AKIA5RODRIGO7Q2XW3LM", "Inactive")
            };

            // e14 — retenção de logs
            T["e14"] = new AwsTask
            {
                Setup = s => { foreach (var g in AwsSim.LogGroups) s.LogRetention[g] = g.StartsWith("/ecs") ? 0 : 30; },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("logs:retention:")) return null;
                    string rest = e.Substring("logs:retention:".Length);
                    int i = rest.LastIndexOf(':');
                    string grp = rest.Substring(0, i);
                    int d = int.Parse(rest.Substring(i + 1));
                    if (grp != "/ecs/cafe365-api") { s.LogRetention[grp] = 30; return "Você mudou a retenção de " + grp + " (desfeito). O grupo que guarda tudo para sempre é o /ecs/cafe365-api."; }
                    if (d == 30) return OK;
                    if (d == 0) return null;
                    s.LogRetention[grp] = 0;
                    return d < 30 ? "Com " + d + " dia(s), um problema do mês passado não poderia mais ser investigado. O combinado é 30 dias (desfeito)." : "Com " + d + " dias o custo continua alto. O combinado com a Ana é 30 dias (desfeito).";
                },
                Solve = s => s.SetRetention("/ecs/cafe365-api", 30)
            };

            // e15 — memória da Lambda
            T["e15"] = new AwsTask
            {
                Setup = s => { foreach (var f in s.Functions.ToList()) ExtraAwsFn(s, f.Name); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("lambda:config:")) return null;
                    var p = e.Split(':');
                    string fn = p[2], field = p[3];
                    if (fn != "relatorio-vendas") { ExtraAwsFn(s, fn); return "Você alterou a função " + fn + " (desfeito). Quem fica sem memória é a relatorio-vendas."; }
                    if (field != "memory") { ExtraAwsFn(s, fn); return "Só a memória precisa mudar (" + (field == "timeout" ? "tempo limite" : "runtime") + " desfeito)."; }
                    int v = int.Parse(p[4]);
                    if (v == 1024) return OK;
                    ExtraAwsFn(s, fn);
                    return v < 1024 ? "Com " + v + " MB o relatório ainda estoura a memória (ele usa ~800 MB no pico). O pedido é 1024 MB (desfeito)." : "Com " + v + " MB você paga por memória que a função não usa (o preço por milissegundo cresce junto). O pedido é 1024 MB (desfeito).";
                },
                Solve = s => s.UpdateFnConfig("relatorio-vendas", -1, 1024, null)
            };

            // e16 — imutabilidade de tags no ECR
            T["e16"] = new AwsTask
            {
                Setup = s => { var r = s.RepoOf("cafe365-web"); if (r != null) { r.Immutable = false; r.ScanOnPush = true; } },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ecr:")) return null;
                    var p = e.Split(':');
                    var r = s.RepoOf(p[2]);
                    if (p[1] == "immutable")
                    {
                        if (p[2] == "cafe365-web") return p[3] == "IMMUTABLE" ? OK : null;
                        if (r != null) r.Immutable = false;
                        return "Você alterou o repositório " + p[2] + " (desfeito). O pedido é o cafe365-web.";
                    }
                    if (p[1] == "scan" && p[3] == "off") { if (r != null) r.ScanOnPush = true; return "Não desligue a verificação de vulnerabilidades (religada). Só a imutabilidade de tags deve mudar."; }
                    return null;
                },
                Solve = s => s.SetImmutable("cafe365-web", true)
            };

            // e17 — liberar IP elástico sem uso
            T["e17"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; ExtraAwsEips(s); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:eip-")) return null;
                    var p = e.Split(':');
                    if (p[1] == "eip-release") { if (p[2] == "eipalloc-0f4ee01") return OK; ExtraAwsEips(s); return "Esse IP estava em uso (restaurado). O que cobra à toa é o que está 'Sem associação'."; }
                    if (p[1] == "eip-disassoc") { ExtraAwsEips(s); return "Você desassociou o IP do web-1: o DNS do site aponta para 54.210.12.8 e o site saiu do ar! (religado). Libere só o IP sem associação."; }
                    if (p[1] == "eip-assoc") { ExtraAwsEips(s); return "O pedido é LIBERAR (devolver) o IP sem uso, não associá-lo a uma instância (desfeito)."; }
                    if (p[1] == "eip-alloc") { s.Eips.RemoveAll(x => x.Alloc == p[2]); return "Você alocou MAIS um IP elástico (mais custo). Ele foi devolvido: libere o que está sem associação."; }
                    return null;
                },
                Solve = s => s.ReleaseEip("eipalloc-0f4ee01")
            };

            // e18 — criar AMI
            T["e18"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; s.Images.RemoveAll(x => x.Name != "web-golden-2026-10"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:image:")) return null;
                    var p = e.Split(':');
                    if (p[2] != "i-0web001") { s.Images.RemoveAll(x => x.Name == p[3]); return "A imagem foi criada a partir de " + MaisAwsInstName(s, p[2]) + " (excluída). A base é o web-1 (i-0web001)."; }
                    if (p[3] != "web-1-base") { s.Images.RemoveAll(x => x.Name == p[3]); return "A imagem ficou com o nome '" + p[3] + "' (excluída). O nome pedido é web-1-base."; }
                    return OK;
                },
                Solve = s => s.CreateImage("i-0web001", "web-1-base", false)
            };

            // e19 — capacidade desejada do Auto Scaling
            T["e19"] = new AwsTask
            {
                Setup = s => { var a = s.Group("asg-web"); a.Min = 1; a.Max = 4; a.Desired = 2; a.Policies.Clear(); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("asg:capacity:")) return null;
                    var p = e.Split(':');
                    var a = s.Group("asg-web");
                    int mn = int.Parse(p[3]), mx = int.Parse(p[4]), d = int.Parse(p[5]);
                    a.Min = 1; a.Max = 4; a.Desired = 2;
                    if (d != 3) return "A capacidade desejada ficou " + d + ". O pedido é 3 instâncias (voltamos para 2).";
                    if (mn != 1 || mx != 4) return "Você mudou também a mínima/máxima (" + mn + "/" + mx + "). Os limites continuam 1 e 4; só a desejada vai para 3 (desfeito).";
                    a.Desired = 3;
                    return OK;
                },
                Solve = s => s.SetCapacity("asg-web", 1, 4, 3)
            };

            // e20 — rotação de chave KMS
            T["e20"] = new AwsTask
            {
                Setup = s => { s.KeyRotation.Clear(); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("kms:rotation:")) return null;
                    var p = e.Split(':');
                    if (p[3] != "on") return null;
                    if (p[2] == "alias/backups") return OK;
                    s.KeyRotation[p[2]] = false;
                    return "Você ativou a rotação da chave " + p[2] + ", que é do time da aplicação (desfeito). A auditoria pediu a alias/backups.";
                },
                Solve = s => s.SetRotation("alias/backups", true)
            };

            // e21 — orçamento mensal
            T["e21"] = new AwsTask
            {
                Setup = s => { s.Budgets.Clear(); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("budget:create:")) return null;
                    var p = e.Split(':');
                    float v = float.Parse(p[3], System.Globalization.CultureInfo.InvariantCulture);
                    if (p[2] != "custo-mensal") { s.Budgets.RemoveAll(b => b.Name == p[2]); return "Orçamento criado como '" + p[2] + "' (excluído). O nome pedido é custo-mensal."; }
                    if (Math.Abs(v - 300) > 0.001) { s.Budgets.RemoveAll(b => b.Name == p[2]); return "O valor ficou US$ " + p[3] + ". O teto combinado com a Ana é US$ 300 por mês (excluído)."; }
                    return OK;
                },
                Solve = s => s.CreateBudget("custo-mensal", 300)
            };

            // e22 — bloquear acesso público de bucket exposto
            T["e22"] = new AwsTask
            {
                Setup = s => { ExtraAwsPromo(s); },
                Eval = (s, e) =>
                {
                    var p = e.Split(':');
                    if (e.StartsWith("s3:bpa:"))
                    {
                        if (p[2] == "cafe365-promo") { if (p[3] == "on") return OK; return "O bucket NÃO ficou totalmente bloqueado: as quatro opções do bloqueio de acesso público precisam estar ativadas."; }
                        var b = s.Bkt(p[2]); if (b != null) b.BlockPublic = true;
                        return "Você mexeu no bucket " + p[2] + " (desfeito). O exposto é o cafe365-promo.";
                    }
                    if (e == "s3:delete:cafe365-promo") { ExtraAwsPromo(s); return "Apagar não era o pedido: o marketing ainda vai reaproveitar o material (bucket restaurado). Só bloqueie o acesso público."; }
                    if (e.StartsWith("s3:delobj:cafe365-promo") || e.StartsWith("s3:delmany:cafe365-promo")) { ExtraAwsPromo(s); return "Não apague os arquivos (restaurados): só bloqueie o acesso público do bucket."; }
                    return null;
                },
                Solve = s => s.SetBlockPublic("cafe365-promo", true)
            };

            // e23 — RDP aberto para a internet
            T["e23"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; ExtraAwsWinSg(s); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:sg-")) return null;
                    var p = e.Split(':');
                    string op = p[1], gid = p[2], src = p[4];
                    int port = int.Parse(p[3]);
                    if (op == "sg-del" && gid == "sg-0win444" && port == 3389 && src == "0.0.0.0/0") return OK;
                    MaisAwsUndoRule(s, op, gid, port, src);
                    if (op == "sg-del" && gid == "sg-0win444") return "Essa é a regra do escritório (200.155.80.10/32), que o financeiro usa: ela deve ficar (restaurada). Remova só a origem 0.0.0.0/0.";
                    if (gid != "sg-0win444") return "Você alterou o grupo " + (s.Sg(gid) != null ? s.Sg(gid).Name : gid) + " (desfeito). O problema está no windows-sg (sg-0win444).";
                    return "A tarefa é REMOVER a regra aberta para a internet, não adicionar outra (regra desfeita).";
                },
                Solve = s => s.Revoke("sg-0win444", 3389, "0.0.0.0/0")
            };

            // e24 — política de administrador esquecida
            T["e24"] = new AwsTask
            {
                Setup = s => { ExtraAwsUser(s, "ana"); var a = s.Users.First(u => u.Name == "ana"); a.Policies.Add("AdministratorAccess"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("iam:")) return null;
                    var p = e.Split(':');
                    if (p.Length < 3) return null;
                    Action back = () => { ExtraAwsUser(s, "ana"); s.Users.First(u => u.Name == "ana").Policies.Add("AdministratorAccess"); };
                    if (p[1] == "detach")
                    {
                        if (p[2] == "ana" && p[3] == "AdministratorAccess") return OK;
                        var u = s.Users.FirstOrDefault(x => x.Name == p[2]); if (u != null && !u.Policies.Contains(p[3])) u.Policies.Add(p[3]);
                        return "Você removeu " + p[3] + " de " + p[2] + " (restaurada). A permissão esquecida é a AdministratorAccess da ana.";
                    }
                    if (p[2] == "ana" && (p[1] == "delete" || p[1] == "ungroup")) { back(); return "A Ana continua no time e precisa do grupo Desenvolvedores (restaurado). Remova só a política AdministratorAccess anexada diretamente."; }
                    if (p[1] == "attach") { var u = s.Users.FirstOrDefault(x => x.Name == p[2]); if (u != null) u.Policies.Remove(p[3]); return "A tarefa é REMOVER uma permissão, não dar outra (" + p[3] + " removida)."; }
                    return null;
                },
                Solve = s => s.DetachPolicy("ana", "AdministratorAccess")
            };

            // ═══════════════ Intermediário ═══════════════

            // e25 — associar IP elástico
            T["e25"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; ExtraAwsEips(s); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:eip-")) return null;
                    var p = e.Split(':');
                    if (p[1] == "eip-assoc")
                    {
                        if (p[2] == "eipalloc-0f4ee01" && p[3] == "i-0int555") return OK;
                        ExtraAwsEips(s);
                        if (p[2] == "eipalloc-0web0001") return "Você MOVEU o IP do web-1 (54.210.12.8) e o site saiu do ar! (desfeito). Use o IP elástico que está sem associação (eipalloc-0f4ee01).";
                        return "O IP foi associado a " + MaisAwsInstName(s, p[3]) + " (desfeito). Quem precisa de IP fixo é a intranet (i-0int555).";
                    }
                    if (p[1] == "eip-alloc") { s.Eips.RemoveAll(x => x.Alloc == p[2]); return "Já existe um IP elástico livre (34.192.7.21) sendo pago sem uso: use ele em vez de alocar outro (o novo foi devolvido)."; }
                    if (p[1] == "eip-release" || p[1] == "eip-disassoc") { ExtraAwsEips(s); return "Não libere nem desassocie IPs (desfeito): associe o IP livre à intranet."; }
                    return null;
                },
                Solve = s => s.AssociateEip("eipalloc-0f4ee01", "i-0int555", false)
            };

            // e26 — assinatura de e-mail no SNS
            T["e26"] = new AwsTask
            {
                Setup = s => { var t = s.Tp("alertas-ti"); if (t == null) { s.CreateTopicQuiet("alertas-ti"); t = s.Tp("alertas-ti"); } t.Subs.Clear(); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("sns:sub:")) return null;
                    var p = e.Split(':');
                    string topic = p[2], proto = p[3], ep = p[4].Replace("|", ":");
                    var t = s.Tp(topic);
                    Action undo = () => { if (t != null) t.Subs.RemoveAll(x => x[0] == proto && x[1] == ep); };
                    if (topic != "alertas-ti") { undo(); return "A assinatura foi criada no tópico " + topic + " (excluída). Os alertas saem do alertas-ti."; }
                    if (proto != "email") { undo(); return "O protocolo ficou " + proto + ". A equipe quer receber por E-MAIL (assinatura excluída)."; }
                    if (ep != "ti@cafe365.com.br") { undo(); return "O endereço ficou " + ep + ". O pedido é a lista da equipe: ti@cafe365.com.br (assinatura excluída)."; }
                    return OK;
                },
                Solve = s => s.Subscribe("alertas-ti", "email", "ti@cafe365.com.br")
            };

            // e27 — variável de ambiente sem perder as outras
            T["e27"] = new AwsTask
            {
                Setup = s => { foreach (var f in s.Functions.ToList()) ExtraAwsFn(s, f.Name); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("lambda:")) return null;
                    var p = e.Split(':');
                    if (p.Length < 3) return null;
                    if (p[2] != "pedidos" && (p[1] == "env" || p[1] == "config")) { ExtraAwsFn(s, p[2]); return "Você alterou a função " + p[2] + " (desfeito). O pedido é na função pedidos."; }
                    if (p[1] == "config") { ExtraAwsFn(s, "pedidos"); return "Só as variáveis de ambiente devem mudar (configuração desfeita)."; }
                    if (p[1] != "env") return null;
                    var env = s.Func("pedidos").Env;
                    string v;
                    if (!env.TryGetValue("TABELA", out v) || v != "pedidos") { ExtraAwsFn(s, "pedidos"); return "A variável TABELA " + (v == null ? "SUMIU" : "mudou para '" + v + "'") + ": a função não acharia mais a tabela! Atualizar as variáveis SUBSTITUI o conjunto inteiro — mande TABELA=pedidos junto (desfeito)."; }
                    if (env.Keys.Any(k => k != "TABELA" && k != "LOG_LEVEL")) { ExtraAwsFn(s, "pedidos"); return "Apareceu uma variável que não foi pedida (" + env.Keys.First(k => k != "TABELA" && k != "LOG_LEVEL") + "). Deixe só TABELA e LOG_LEVEL (desfeito)."; }
                    if (!env.TryGetValue("LOG_LEVEL", out v)) return null;
                    if (v != "INFO") { ExtraAwsFn(s, "pedidos"); return "LOG_LEVEL ficou '" + v + "'. O pedido é INFO (DEBUG em produção gera log demais e custa caro) — desfeito."; }
                    return OK;
                },
                Solve = s => s.SetEnv("pedidos", new Dictionary<string, string> { { "TABELA", "pedidos" }, { "LOG_LEVEL", "INFO" } })
            };

            // e28 — snapshot manual do RDS
            T["e28"] = new AwsTask
            {
                Setup = s => { ExtraAwsDb(s, "banco-cafe"); ExtraAwsDb(s, "banco-dev"); s.DbSnaps.RemoveAll(x => x.Type == "Manual"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("rds:")) return null;
                    var p = e.Split(':');
                    if (p[1] == "snapshot")
                    {
                        if (p[2] != "banco-cafe") { s.DbSnaps.RemoveAll(x => x.Id == p[3]); return "O snapshot foi do " + p[2] + " (excluído). A migração é no banco de produção, banco-cafe."; }
                        if (p[3] != "antes-da-migracao") { s.DbSnaps.RemoveAll(x => x.Id == p[3]); return "O snapshot ficou com o nome '" + p[3] + "' (excluído). O roteiro da migração procura antes-da-migracao."; }
                        return OK;
                    }
                    if (p[1] == "stop" || p[1] == "modify" || p[1] == "reboot") { ExtraAwsDb(s, p[2]); return "A tarefa é só tirar o snapshot (alteração no " + p[2] + " desfeita)."; }
                    return null;
                },
                Solve = s => s.DbSnapshot("banco-cafe", "antes-da-migracao")
            };

            // e29 — registro A no Route 53
            T["e29"] = new AwsTask
            {
                Setup = s => { ExtraAwsRecords(s); s.Records.RemoveAll(r => r.Name == "api.cafe365.com.br"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("r53:")) return null;
                    var p = e.Split(':');
                    string act = p[1], name = p[2], type = p[3], val = p[4];
                    int ttl = int.Parse(p[5]);
                    Action undo = () => { ExtraAwsRecords(s, "api.cafe365.com.br"); s.Records.RemoveAll(r => r.Name == "api.cafe365.com.br"); };
                    if (name != "api.cafe365.com.br") { undo(); return "Você " + (act == "delete" ? "excluiu" : "criou/alterou") + " o registro " + name + " (desfeito). O pedido é criar api.cafe365.com.br."; }
                    if (act == "delete") return null;
                    if (type != "A") { undo(); return "O tipo ficou " + type + ". Para apontar um nome para um endereço IPv4 o tipo é A (registro excluído)."; }
                    if (val != "54.210.12.8") { undo(); return "O valor ficou " + val + ". A API responde no IP elástico do web-1: 54.210.12.8 (registro excluído)."; }
                    if (ttl != 300) { undo(); return "O TTL ficou " + ttl + " s. O padrão combinado é 300 segundos (registro excluído)."; }
                    return OK;
                },
                Solve = s => s.ChangeRecord("CREATE", "api.cafe365.com.br", "A", "54.210.12.8", 300)
            };

            // e30 — ciclo de vida: expirar logs antigos
            T["e30"] = new AwsTask
            {
                Setup = s => { MaisAwsBucket(s, "cafe365-logs").Lifecycle.Clear(); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("s3:lifecycle:")) return null;
                    var p = e.Split(':');
                    var b = s.Bkt(p[2]);
                    string id = p[3], pre = p[4];
                    int exp = int.Parse(p[5]), nc = int.Parse(p[6]), gl = int.Parse(p[7]);
                    Action undo = () => { if (b != null) b.Lifecycle.RemoveAll(r => r.Id == id); };
                    if (p[2] != "cafe365-logs") { undo(); return "A regra foi criada no bucket " + p[2] + " (excluída). Os logs ficam no cafe365-logs."; }
                    if (id != "expirar-logs") { undo(); return "A regra ficou com o nome '" + id + "' (excluída). O nome pedido é expirar-logs."; }
                    if (pre.Length > 0) { undo(); return "A regra ficou limitada ao prefixo '" + pre + "'. Ela deve valer para o bucket INTEIRO (excluída)."; }
                    if (gl > 0 || nc > 0) { undo(); return "Só a expiração das versões atuais foi pedida (sem Glacier e sem regra de versões) — regra excluída."; }
                    if (exp != 90) { undo(); return "A expiração ficou em " + exp + " dias. A política da empresa guarda logs por 90 dias (regra excluída)."; }
                    return OK;
                },
                Solve = s => s.PutLifecycle("cafe365-logs", new List<AwsSim.LcRule> { new AwsSim.LcRule { Id = "expirar-logs", Expire = 90 } }, false)
            };

            // e31 — gp2 → gp3
            T["e31"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; ExtraAwsVolume(s, "vol-0b72c44", "gp2", 100); ExtraAwsVolume(s, "vol-0d9e202", "gp2", 50); ExtraAwsVolume(s, "vol-0e81b37", "gp2", 500); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:modvol:")) return null;
                    var p = e.Split(':');
                    string vid = p[2], type = p[3];
                    var orig = new AwsSim().Vol(vid);
                    int osz = orig != null ? orig.Size : 0;
                    string otype = vid == "vol-0d9e202" ? "gp2" : orig != null ? orig.Type : "gp3";
                    int size = int.Parse(p[4]);
                    Action undo = () => ExtraAwsVolume(s, vid, otype, osz);
                    if (vid != "vol-0b72c44" && vid != "vol-0d9e202") { undo(); return vid == "vol-0e81b37" ? "Esse volume está 'Disponível' (solto, vai ser apagado): o pedido é converter os que estão EM USO (desfeito)." : "O volume " + vid + " já era gp3 (desfeito)."; }
                    if (size != osz) { undo(); return "Você mudou também o tamanho (" + osz + " → " + size + " GiB): isso aumenta o custo e não dá para diminuir depois. Só o tipo muda (desfeito)."; }
                    if (type != "gp3") { undo(); return "O tipo ficou " + type + (type == "io2" ? " (IOPS provisionadas, muito mais caro)" : type == "st1" || type == "sc1" ? " (HDD: lento para o sistema)" : "") + ". O pedido é gp3 (desfeito)."; }
                    return s.Vol("vol-0b72c44").Type == "gp3" && s.Vol("vol-0d9e202").Type == "gp3" ? OK : null;
                },
                Solve = s => { s.ModifyVolume("vol-0b72c44", "gp3", -1); s.ModifyVolume("vol-0d9e202", "gp3", -1); }
            };

            // e32 — fila FIFO com deduplicação
            T["e32"] = new AwsTask
            {
                Setup = s => { s.Queues.RemoveAll(q => q.Name.StartsWith("pagamentos")); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("sqs:create:")) return null;
                    var p = e.Split(':');
                    Action undo = () => s.Queues.RemoveAll(q => q.Name == p[2]);
                    if (p[2] == "pagamentos") { undo(); return "Fila Padrão pode entregar fora de ordem e em duplicidade: pagamento cobrado duas vezes! Crie a FIFO pagamentos.fifo (a fila foi excluída)."; }
                    if (p[2] != "pagamentos.fifo") { undo(); return "Fila criada como '" + p[2] + "' (excluída). O nome pedido é pagamentos.fifo."; }
                    if (p[4] != "dedup") { undo(); return "Faltou a deduplicação baseada em conteúdo: o app de pagamentos não envia ID de deduplicação, então o mesmo pagamento reenviado seria aceito duas vezes (fila excluída)."; }
                    return OK;
                },
                Solve = s => s.CreateQueue("pagamentos.fifo", true, true, 30, 0)
            };

            // e33 — tabela do DynamoDB
            T["e33"] = new AwsTask
            {
                Setup = s => { s.Tables.RemoveAll(t => t.Name == "carrinhos"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ddb:create:")) return null;
                    var p = e.Split(':');
                    string name = p[2], pk = p[3], pkt = p[4], sk = p[5], bill = p[7];
                    Action undo = () => s.Tables.RemoveAll(t => t.Name == name);
                    if (name != "carrinhos") { undo(); return "Tabela criada como '" + name + "' (excluída). O nome pedido é carrinhos."; }
                    if (pk != "cliente_id") { undo(); return "A chave de partição ficou '" + pk + "'. Cada carrinho é achado pelo cliente: cliente_id (tabela excluída)."; }
                    if (pkt != "S") { undo(); return "O tipo da chave ficou " + (pkt == "N" ? "Número" : "Binário") + ". Os IDs de cliente são texto (ex.: c-102): String (tabela excluída)."; }
                    if (sk.Length > 0) { undo(); return "Não precisa de chave de classificação: é UM carrinho por cliente (tabela excluída)."; }
                    if (bill != "PAY_PER_REQUEST") { undo(); return "Capacidade provisionada cobra mesmo parada e não aguenta picos sem ajuste. O tráfego é imprevisível: use Sob demanda (tabela excluída)."; }
                    return OK;
                },
                Solve = s => s.CreateTable("carrinhos", "cliente_id", "S", null, null, "PAY_PER_REQUEST")
            };

            // e34 — runtime descontinuado
            T["e34"] = new AwsTask
            {
                Setup = s => { foreach (var f in s.Functions.ToList()) ExtraAwsFn(s, f.Name); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("lambda:config:")) return null;
                    var p = e.Split(':');
                    if (p[2] != "relatorio-vendas") { ExtraAwsFn(s, p[2]); return "Você alterou a função " + p[2] + " (desfeito). A do runtime descontinuado é a relatorio-vendas."; }
                    if (p[3] != "runtime") { ExtraAwsFn(s, p[2]); return "Só o tempo de execução (runtime) precisa mudar (desfeito)."; }
                    if (p[4] == "python3.12") return OK;
                    ExtraAwsFn(s, p[2]);
                    if (p[4].StartsWith("nodejs")) return "O código da função é Python: num runtime Node.js ela nem inicia (desfeito).";
                    if (p[4] == "python3.13") return "O time testou o relatório com Python 3.12; pular para outra versão sem testar pode quebrar bibliotecas (desfeito).";
                    return "O pedido é Python 3.12 (python3.12) — desfeito.";
                },
                Solve = s => s.UpdateFnConfig("relatorio-vendas", -1, -1, "python3.12")
            };

            // e35 — URL pré-assinada
            T["e35"] = new AwsTask
            {
                Setup = s => { var b = MaisAwsBucket(s, "cafe365-relatorios"); b.BlockPublic = true; b.Policy = null; b.PolicyText = null; foreach (var o in new[] { "vendas-agosto.pdf", "vendas-setembro.pdf" }) if (!b.Objects.Contains(o)) b.Objects.Add(o); },
                Eval = (s, e) =>
                {
                    var p = e.Split(':');
                    if (e.StartsWith("s3:bpa:") && p[3] == "off") { var b = s.Bkt(p[2]); if (b != null) b.BlockPublic = true; return "Não abra o bucket para o mundo (bloqueio religado): a URL pré-assinada dá acesso a UM arquivo, por tempo limitado."; }
                    if (!e.StartsWith("s3:presign:")) return null;
                    int secs = int.Parse(p[4]);
                    if (p[2] != "cafe365-relatorios" || p[3] != "vendas-setembro.pdf") return "Você gerou o link de " + p[3] + " (" + p[2] + "). O contador precisa do vendas-setembro.pdf do bucket cafe365-relatorios.";
                    if (secs == 3600) return OK;
                    return secs > 3600 ? "O link vale por " + (secs >= 86400 ? secs / 86400 + " dia(s)" : secs / 3600 + " hora(s)") + ": tempo demais se a URL vazar. O combinado é 1 hora (3600 s). Gere outra." : "Com " + secs + " s o link expira antes de o contador abrir o e-mail. O combinado é 1 hora (3600 s). Gere outra.";
                },
                Solve = s => { string u; s.Presign("cafe365-relatorios", "vendas-setembro.pdf", 3600, out u); }
            };

            // e36 — grupo de segurança novo com HTTPS
            T["e36"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; s.SecGroups.RemoveAll(g => g.Name == "api-sg"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:sg-")) return null;
                    var p = e.Split(':');
                    if (p[1] == "sg-create")
                    {
                        var g = s.Sg(p[2]);
                        if (p[3] != "api-sg") { s.SecGroups.Remove(g); return "Grupo criado como '" + p[3] + "' (excluído). O nome pedido é api-sg."; }
                        if (g != null && g.Region != "us-east-1") { s.SecGroups.Remove(g); return "O grupo foi criado em " + g.Region + ". A API roda no Norte da Virgínia (us-east-1): grupos de segurança são da região/VPC (excluído)."; }
                        return null;
                    }
                    string op = p[1], gid = p[2], src = p[4];
                    int port = int.Parse(p[3]);
                    var api = s.SecGroups.FirstOrDefault(x => x.Name == "api-sg" && x.Region == "us-east-1");
                    if (api == null || gid != api.Id) { MaisAwsUndoRule(s, op, gid, port, src); return "Você alterou o grupo " + (s.Sg(gid) != null ? s.Sg(gid).Name : gid) + " (desfeito). A regra vai no api-sg."; }
                    if (op == "sg-del") return null;
                    if (port != 443) { MaisAwsUndoRule(s, op, gid, port, src); return port == 22 ? "SSH (22) aberto não foi pedido — e para 0.0.0.0/0 seria perigoso (regra removida). A API só atende HTTPS (443)." : "A porta " + port + " (" + AwsSim.RuleName(port) + ") não foi pedida: a API só atende HTTPS, porta 443 (regra removida)."; }
                    if (src != "0.0.0.0/0") { MaisAwsUndoRule(s, op, gid, port, src); return "A origem ficou " + src + ". A API é pública (o app dos clientes acessa de qualquer lugar): 0.0.0.0/0 (regra removida)."; }
                    return api.Inbound.Count == 1 ? OK : null;
                },
                Solve = s => { string id; s.CreateSg("api-sg", "API do Cafe 365", "vpc-0cafe365", out id); s.Authorize(id, 443, "0.0.0.0/0"); }
            };

            // e37 — redirecionar HTTP para HTTPS (console)
            T["e37"] = new AwsTask
            {
                Setup = s => { var d = s.Distr("E1CAFE365SITE"); d.Viewer = "allow-all"; },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("cf:viewer:")) return null;
                    var p = e.Split(':');
                    if (p[3] == "redirect-to-https") return OK;
                    if (p[3] == "https-only") { s.Distr(p[2]).Viewer = "allow-all"; return "Com 'Somente HTTPS', quem chega por http:// (links antigos, gente digitando sem https) recebe ERRO 403. O pedido é REDIRECIONAR para HTTPS (desfeito)."; }
                    return null;
                },
                Solve = s => s.SetViewerPolicy("E1CAFE365SITE", "redirect-to-https")
            };

            // e38 — política de senha
            T["e38"] = new AwsTask
            {
                Setup = s => { s.PwMin = 8; s.PwMaxAge = 0; s.PwUpper = s.PwLower = s.PwNumbers = s.PwSymbols = false; },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("iam:pwpolicy:")) return null;
                    var p = e.Split(':');
                    int min = int.Parse(p[2]), age = int.Parse(p[7]);
                    var faltam = new List<string>();
                    if (p[3] != "1") faltam.Add("maiúscula");
                    if (p[4] != "1") faltam.Add("minúscula");
                    if (p[5] != "1") faltam.Add("número");
                    if (p[6] != "1") faltam.Add("símbolo");
                    string err = null;
                    if (min != 14) err = "O comprimento mínimo ficou " + min + ". O pedido da auditoria é 14 caracteres.";
                    else if (faltam.Count > 0) err = "Faltou exigir: " + string.Join(", ", faltam) + ". (Pela CLI, toda opção não informada volta a ficar desligada!)";
                    else if (age > 0) err = "Você ativou a expiração de senha (" + age + " dias). O pedido é SEM troca periódica forçada (recomendação atual do NIST: só trocar quando houver suspeita de vazamento).";
                    if (err == null) return OK;
                    s.PwMin = 8; s.PwMaxAge = 0; s.PwUpper = s.PwLower = s.PwNumbers = s.PwSymbols = false;
                    return err + " (política voltou ao padrão)";
                },
                Solve = s => s.SetPasswordPolicy(14, true, true, true, true, 0)
            };

            // e39 — sondagem longa (long polling)
            T["e39"] = new AwsTask
            {
                Setup = s => { ExtraAwsQueue(s, "pedidos"); ExtraAwsQueue(s, "pedidos-dlq"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("sqs:")) return null;
                    var p = e.Split(':');
                    if (p[1] != "attr" && p[1] != "redrive") return null;
                    if (p[2] != "pedidos") { ExtraAwsQueue(s, p[2]); return "Você alterou a fila " + p[2] + " (desfeito). As chamadas vazias vêm da fila pedidos."; }
                    if (p[1] == "redrive" || p[3] != "ReceiveMessageWaitTimeSeconds") { ExtraAwsQueue(s, "pedidos"); return "Só o tempo de espera de recebimento deve mudar (desfeito)."; }
                    int v = int.Parse(p[4]);
                    if (v == 20) return OK;
                    ExtraAwsQueue(s, "pedidos");
                    return v == 0 ? null : "Com " + v + " s ainda sobram muitas chamadas vazias. O máximo (e mais econômico) é 20 segundos (desfeito).";
                },
                Solve = s => s.SetQueueAttr("pedidos", "ReceiveMessageWaitTimeSeconds", 20)
            };

            // e40 — retenção de backup do RDS
            T["e40"] = new AwsTask
            {
                Setup = s => { ExtraAwsDb(s, "banco-cafe"); ExtraAwsDb(s, "banco-dev"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("rds:")) return null;
                    var p = e.Split(':');
                    if (p[1] == "stop" || p[1] == "reboot") { ExtraAwsDb(s, p[2]); return "A tarefa é só mudar a retenção de backup (o " + p[2] + " voltou a funcionar)."; }
                    if (p[1] != "modify") return null;
                    var ch = ExtraAwsKv(p[3]);
                    if (p[2] != "banco-cafe") { ExtraAwsDb(s, p[2]); return "Você modificou o " + p[2] + " (desfeito). O pedido é no banco de produção, banco-cafe."; }
                    var extra = ch.Keys.Where(k => k != "backup").ToList();
                    if (extra.Count > 0) { ExtraAwsDb(s, "banco-cafe"); return "Você mudou também: " + string.Join(", ", extra) + ". Só a retenção de backup deve mudar (desfeito)."; }
                    string b;
                    if (!ch.TryGetValue("backup", out b)) return null;
                    if (b == "0") { ExtraAwsDb(s, "banco-cafe"); return "Retenção 0 DESATIVA os backups automáticos (e apaga os existentes)! O pedido é 7 dias (desfeito)."; }
                    if (b != "7") { ExtraAwsDb(s, "banco-cafe"); return "A retenção ficou " + b + " dia(s). O pedido é 7 dias (desfeito)."; }
                    if (p[4] == "window") { ExtraAwsDb(s, "banco-cafe"); return "A mudança ficou PENDENTE para a janela de manutenção de domingo. O pedido é aplicar imediatamente (mudar a retenção de 1 para 7 dias não reinicia o banco) — desfeito."; }
                    return OK;
                },
                Solve = s => s.ModifyDb("banco-cafe", new Dictionary<string, string> { { "backup", "7" } }, true)
            };

            // e41 — TTL no DynamoDB
            T["e41"] = new AwsTask
            {
                Setup = s => { foreach (var t in s.Tables) t.Ttl = null; },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ddb:ttl:")) return null;
                    var p = e.Split(':');
                    var t = s.Tb(p[2]);
                    if (p[3].Length == 0) return null;
                    if (p[2] != "sessoes") { if (t != null) t.Ttl = null; return "Você ativou o TTL na tabela " + p[2] + " (desfeito): itens de lá seriam apagados sozinhos! O pedido é na tabela sessoes."; }
                    if (p[3] != "expira_em") { t.Ttl = null; return "O atributo ficou '" + p[3] + "', que os itens não têm (nada expiraria). O horário de expiração das sessões está em expira_em (desfeito)."; }
                    return OK;
                },
                Solve = s => s.SetTtl("sessoes", "expira_em")
            };

            // e42 — alarme com notificação
            T["e42"] = new AwsTask
            {
                Setup = s => { s.Alarms.RemoveAll(a => a.Name == "status-web"); var t = s.Tp("alertas-ti"); if (t == null) { s.CreateTopicQuiet("alertas-ti"); t = s.Tp("alertas-ti"); } t.Subs.Clear(); t.Subs.Add(new[] { "email", "ti@cafe365.com.br", "Confirmada" }); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("cw:alarm:")) return null;
                    var p = e.Split(':');
                    string name = p[2], metric = p[3], res = p[4], th = p[5], comp = p.Length > 7 ? p[7] : "", act = p.Length > 8 ? p[8] : "";
                    Action undo = () => s.Alarms.RemoveAll(a => a.Name == name);
                    if (name != "status-web") { undo(); return "Alarme criado como '" + name + "' (excluído). O nome pedido é status-web."; }
                    if (metric != "StatusCheckFailed") { undo(); return "A métrica ficou " + metric + ". Para saber se a instância parou de responder, use StatusCheckFailed (verificação de status) — alarme excluído."; }
                    if (res != "web-1") { undo(); return "O alarme monitora " + res + ". O pedido é o web-1 (alarme excluído)."; }
                    if (th != "0" || comp.StartsWith("Less")) { undo(); return "StatusCheckFailed vale 0 (tudo bem) ou 1 (falhou): a condição é MAIOR QUE 0 (alarme excluído)."; }
                    if (act.Length == 0) { undo(); return "Sem notificação, o alarme muda de estado e ninguém fica sabendo. Escolha o tópico alertas-ti (alarme excluído)."; }
                    if (act != "alertas-ti") { undo(); return "A notificação foi para o tópico " + act + ". Quem atende incidente é a equipe do alertas-ti (alarme excluído)."; }
                    return OK;
                },
                Solve = s => s.CreateAlarm("status-web", "StatusCheckFailed", "web-1", 0, "AWS/EC2", "GreaterThanThreshold", "alertas-ti")
            };

            // e43 — limites do Auto Scaling
            T["e43"] = new AwsTask
            {
                Setup = s => { var a = s.Group("asg-web"); a.Min = 1; a.Max = 4; a.Desired = 2; a.Policies.Clear(); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("asg:")) return null;
                    var p = e.Split(':');
                    var a = s.Group("asg-web");
                    if (p[1] == "policy") { a.Policies.Clear(); return "Política de escalabilidade não foi pedida agora (excluída). A tarefa é ajustar mínimo e máximo."; }
                    if (p[1] != "capacity") return null;
                    int mn = int.Parse(p[3]), mx = int.Parse(p[4]);
                    if (mn == 2 && mx == 6) return OK;
                    a.Min = 1; a.Max = 4; a.Desired = 2;
                    if (mn != 2) return "A capacidade mínima ficou " + mn + ". O pedido é 2 — uma instância em cada zona, para o site sobreviver à queda de uma zona (desfeito).";
                    return "A capacidade máxima ficou " + mx + ". O teto combinado para a Black Friday é 6 (desfeito).";
                },
                Solve = s => s.SetCapacity("asg-web", 2, 6, 2)
            };

            // e44 — objeto raiz padrão (console)
            T["e44"] = new AwsTask
            {
                Setup = s => { s.Distr("E1CAFE365SITE").Root = ""; },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("cf:root:")) return null;
                    var p = e.Split(':');
                    if (p[3] == "index.html") return OK;
                    if (p[3].Length == 0) return null;
                    s.Distr(p[2]).Root = "";
                    return "O objeto raiz ficou '" + p[3] + "'. A página inicial do site é index.html (desfeito).";
                },
                Solve = s => s.SetRootObject("E1CAFE365SITE", "index.html")
            };

            // e45 — alerta no orçamento
            T["e45"] = new AwsTask
            {
                Setup = s => { s.Budgets.RemoveAll(b => b.Name == "custo-mensal"); s.Budgets.Add(new AwsSim.Budget { Name = "custo-mensal", Amount = 300 }); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("budget:alert:")) return null;
                    var p = e.Split(':');
                    var b = s.Budgets.FirstOrDefault(x => x.Name == p[2]);
                    Action undo = () => { if (b != null) b.Alerts.RemoveAll(a => a[0] == p[3] && a[1] == p[4]); };
                    if (p[2] != "custo-mensal") { undo(); return "O alerta foi no orçamento " + p[2] + " (excluído). O pedido é no custo-mensal."; }
                    if (p[3] != "ACTUAL") { undo(); return "O alerta ficou sobre o custo PREVISTO. A Ana pediu sobre o custo REAL (o que já foi gasto) — alerta excluído."; }
                    if (p[4] != "80") { undo(); return "O limite ficou " + p[4] + "%. O pedido é 80% do orçamento (alerta excluído)."; }
                    if (p[5] != "financeiro@cafe365.com.br") { undo(); return "O e-mail ficou " + p[5] + ". O alerta vai para financeiro@cafe365.com.br (alerta excluído)."; }
                    return OK;
                },
                Solve = s => s.AddBudgetAlert("custo-mensal", "ACTUAL", 80, "financeiro@cafe365.com.br")
            };

            // e46 — gatilho SQS → Lambda
            T["e46"] = new AwsTask
            {
                Setup = s => { ExtraAwsFn(s, "processar-pedidos"); ExtraAwsQueue(s, "pedidos"); ExtraAwsQueue(s, "pedidos-dlq"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("lambda:trigger:")) return null;
                    var p = e.Split(':');
                    var f = s.Func(p[2]);
                    Action undo = () => { if (f != null) f.Triggers.RemoveAll(t => t[0] == p[3]); };
                    if (p[2] != "processar-pedidos") { undo(); return "O gatilho foi na função " + p[2] + " (removido). Quem processa a fila é a processar-pedidos."; }
                    if (p[3] == "pedidos-dlq") { undo(); return "A pedidos-dlq guarda as mensagens que FALHARAM: ligar a função nela reprocessaria os erros sem parar. A fila de entrada é a pedidos (gatilho removido)."; }
                    if (p[3] != "pedidos") { undo(); return "A fila escolhida foi " + p[3] + ". Os pedidos chegam na fila pedidos (gatilho removido)."; }
                    if (p[4] != "10") { undo(); return "O lote ficou " + p[4] + ". O pedido é 10 mensagens por execução (gatilho removido)."; }
                    return OK;
                },
                Solve = s => s.AddTrigger("processar-pedidos", "pedidos", 10)
            };

            // e47 — função (role) para a Lambda
            T["e47"] = new AwsTask
            {
                Setup = s => { s.Roles.RemoveAll(r => r.Name == "lambda-relatorios"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("iam:role-")) return null;
                    var p = e.Split(':');
                    var r = s.Rl(p[2]);
                    if (p[1] == "role-create")
                    {
                        if (p[2] != "lambda-relatorios") { s.Roles.Remove(r); return "Função criada como '" + p[2] + "' (excluída). O nome pedido é lambda-relatorios."; }
                        if (p[3] != "lambda") { s.Roles.Remove(r); return "A entidade confiável ficou " + p[3] + ": só esse serviço poderia assumir a função, e a Lambda não! Use o caso de uso Lambda (na CLI, confianca-lambda.json) — função excluída."; }
                        return null;
                    }
                    if (p[1] != "role-attach" || p[2] != "lambda-relatorios" || r == null) return null;
                    var ok = new[] { "AmazonS3ReadOnlyAccess", "AWSLambdaBasicExecutionRole" };
                    if (!ok.Contains(p[3]))
                    {
                        s.Roles.Remove(r);
                        return p[3] == "AmazonS3FullAccess" || p[3] == "AdministratorAccess" || p[3] == "PowerUserAccess" ? "Permissão demais: " + p[3] + " deixa a função APAGAR dados. Ela só lê o S3 e grava logs (função excluída: crie de novo)." : p[3] + " não é necessária: o relatório só lê o S3 e grava logs (função excluída: crie de novo).";
                    }
                    return ok.All(x => r.Policies.Contains(x)) ? OK : null;
                },
                Solve = s => { s.CreateRole("lambda-relatorios", "lambda"); s.AttachRolePolicy("lambda-relatorios", "AmazonS3ReadOnlyAccess"); s.AttachRolePolicy("lambda-relatorios", "AWSLambdaBasicExecutionRole"); }
            };

            // e48 — executar instância a partir da AMI dourada
            T["e48"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; s.Instances.RemoveAll(i => i.Name == "web-2"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:launch:")) return null;
                    var p = e.Split(':');
                    string name = p[2], type = p[3], region = p[4], ami = p.Length > 5 ? p[5] : "", key = p.Length > 6 ? p[6] : "", sg = p.Length > 7 ? p[7] : "";
                    var inst = s.Instances.LastOrDefault(i => i.Name == name);
                    Action undo = () => s.Instances.Remove(inst);
                    if (name != "web-2") { undo(); return "Instância criada como '" + name + "' (encerrada). O nome pedido é web-2."; }
                    if (region != "us-east-1") { undo(); return "A instância foi criada em " + region + ". A AMI web-golden e o web-1 ficam no Norte da Virgínia (encerrada)."; }
                    if (ami != "web-golden-2026-10") { undo(); return "A imagem usada foi " + ami + ": um servidor zerado, sem o site! Use a AMI web-golden-2026-10 (Minhas AMIs) — instância encerrada."; }
                    if (type != "t3.small") { undo(); return "O tipo ficou " + type + ". O web-1 roda bem em t3.small (instância encerrada)."; }
                    if (sg != "sg-0web111") { undo(); return "O grupo de segurança ficou " + (s.Sg(sg) != null ? s.Sg(sg).Name : sg) + (sg == "sg-0default" ? ", que não libera HTTP/HTTPS: ninguém acessaria o site" : "") + ". Use o web-sg (instância encerrada)."; }
                    if (key != "cafe-key") { undo(); return (key.Length == 0 ? "Sem par de chaves, o Rodrigo não consegue entrar por SSH" : "O par de chaves ficou " + key) + ". Use o cafe-key, o mesmo do web-1 (instância encerrada)."; }
                    return OK;
                },
                Solve = s => s.Launch("web-2", "web-golden-2026-10", "t3.small", "cafe-key", "sg-0web111")
            };

            // ═══════════════ Profissional ═══════════════

            // e49 — banco sem acesso público, imediatamente
            T["e49"] = new AwsTask
            {
                Setup = s => { ExtraAwsDb(s, "banco-dev"); ExtraAwsDb(s, "banco-cafe").Public = true; },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("rds:")) return null;
                    var p = e.Split(':');
                    Action undo = () => { ExtraAwsDb(s, "banco-cafe").Public = true; };
                    if (p[2] != "banco-cafe") { if (p[1] != "snapshot") ExtraAwsDb(s, p[2]); return p[1] == "snapshot" ? null : "Você mexeu no " + p[2] + " (desfeito). O exposto é o banco-cafe."; }
                    if (p[1] == "stop" || p[1] == "reboot") { undo(); return "Derrubar a produção não é a correção (o banco voltou). Tire o acesso público do banco-cafe."; }
                    if (p[1] != "modify") return null;
                    var ch = ExtraAwsKv(p[3]);
                    string pub;
                    if (!ch.TryGetValue("public", out pub) || pub != "false") { undo(); return "O banco continua acessível publicamente (modificação desfeita). Escolha 'Não acessível publicamente'."; }
                    if (ch.Count > 1) { undo(); return "Você mudou também: " + string.Join(", ", ch.Keys.Where(k => k != "public")) + ". Em incidente, mude só o necessário (desfeito)."; }
                    if (p[4] == "window") { undo(); return "A mudança ficou para a janela de manutenção de domingo: até lá o banco continua exposto na internet! Num incidente, aplique IMEDIATAMENTE (desfeito)."; }
                    return OK;
                },
                Solve = s => s.ModifyDb("banco-cafe", new Dictionary<string, string> { { "public", "false" } }, true)
            };

            // e50 — IMDSv2 obrigatório nos dois servidores web
            T["e50"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; foreach (var i in s.Instances) i.Imds = "optional"; },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:imds:")) return null;
                    var p = e.Split(':');
                    var inst = s.Inst(p[2]);
                    if (p[4] == "disabled") { inst.Imds = "optional"; return "Desabilitar o serviço de metadados quebra a aplicação: é por ele que ela recebe as credenciais temporárias da função do IAM (desfeito). Deixe habilitado e exija o V2."; }
                    if (p[2] != "i-0web001" && p[2] != "i-0br0042") { inst.Imds = "optional"; return MaisAwsInstName(s, p[2]) + " não é servidor web (desfeito). O pedido é no web-1 e no web-br-1."; }
                    if (p[3] != "required") return null;
                    return s.Inst("i-0web001").Imds == "required" && s.Inst("i-0br0042").Imds == "required" ? OK : null;
                },
                Solve = s => { s.SetImds("i-0web001", "required", true); s.SetImds("i-0br0042", "required", true); }
            };

            // e51 — política do bucket: só HTTPS
            T["e51"] = new AwsTask
            {
                Setup = s => { var b = MaisAwsBucket(s, "micorosoft-backups"); b.Policy = null; b.PolicyText = null; },
                Eval = (s, e) =>
                {
                    var p = e.Split(':');
                    if (e.StartsWith("s3:bpa:") && p[3] == "off") { var bb = s.Bkt(p[2]); if (bb != null) bb.BlockPublic = true; return "Não desative o bloqueio de acesso público (religado): ele não impede uma política que só NEGA acesso."; }
                    if (!e.StartsWith("s3:policy:")) return null;
                    var b = s.Bkt(p[2]);
                    if (p[2] != "micorosoft-backups") { if (b != null) { b.Policy = null; b.PolicyText = null; } return "Você alterou a política do bucket " + p[2] + " (desfeito). O pedido é no micorosoft-backups."; }
                    if (p[3] == "https") return OK;
                    b.Policy = null; b.PolicyText = null;
                    if (p[3] == "none") return null;
                    return "Essa política não nega as requisições sem criptografia em trânsito. Procure um Deny com a condição aws:SecureTransport = false (política removida).";
                },
                Solve = s => s.SetPolicy("micorosoft-backups", AwsSim.JsonFile("politica-somente-https.json"))
            };

            // e52 — fila de mensagens mortas
            T["e52"] = new AwsTask
            {
                Setup = s => { ExtraAwsQueue(s, "pedidos"); ExtraAwsQueue(s, "pedidos-dlq"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("sqs:")) return null;
                    var p = e.Split(':');
                    if (p[1] != "attr" && p[1] != "redrive") return null;
                    if (p[2] != "pedidos") { ExtraAwsQueue(s, p[2]); return "Você alterou a fila " + p[2] + " (desfeito). A redrive é configurada na fila de ORIGEM, pedidos, apontando para a DLQ."; }
                    if (p[1] == "attr") { ExtraAwsQueue(s, "pedidos"); return "Só a fila de mensagens mortas deve mudar (desfeito)."; }
                    if (p[3].Length == 0) return null;
                    int mx = int.Parse(p[4]);
                    if (p[3] != "pedidos-dlq") { ExtraAwsQueue(s, "pedidos"); return "A DLQ ficou " + p[3] + ". Use a fila criada para isso: pedidos-dlq (desfeito)."; }
                    if (mx != 5) { ExtraAwsQueue(s, "pedidos"); return mx < 5 ? "Com " + mx + " tentativa(s), uma falha passageira (banco lento por 1 segundo) já manda o pedido para a DLQ. O combinado são 5 (desfeito)." : "Com " + mx + " tentativas, a mensagem envenenada fica voltando por muito tempo e atrasa as outras. O combinado são 5 (desfeito)."; }
                    return OK;
                },
                Solve = s => s.SetRedrive("pedidos", "pedidos-dlq", 5)
            };

            // e53 — senha fora das variáveis de ambiente
            Action<AwsSim> e53env = s => { ExtraAwsFn(s, "importar-estoque"); s.Func("importar-estoque").Env["DB_SENHA"] = "Cafe2026!"; };
            T["e53"] = new AwsTask
            {
                Setup = s => e53env(s),
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("lambda:")) return null;
                    var p = e.Split(':');
                    if (p.Length < 3) return null;
                    if (p[2] != "importar-estoque" && (p[1] == "env" || p[1] == "config")) { ExtraAwsFn(s, p[2]); return "Você alterou a função " + p[2] + " (desfeito). A senha exposta está na importar-estoque."; }
                    if (p[1] == "config") { e53env(s); return "Só as variáveis de ambiente devem mudar (desfeito)."; }
                    if (p[1] != "env") return null;
                    var env = s.Func("importar-estoque").Env;
                    string v;
                    if (!env.TryGetValue("DB_HOST", out v) || v != "banco-cafe.interno" || !env.TryGetValue("DB_USUARIO", out v) || v != "estoque") { e53env(s); return "DB_HOST e DB_USUARIO precisam continuar como estavam: sem eles a função não conecta (desfeito — lembre que salvar SUBSTITUI o conjunto inteiro)."; }
                    if (env.TryGetValue("SEGREDO_ID", out v) && v != "prod/banco-cafe") { e53env(s); return "SEGREDO_ID ficou '" + v + "'. O segredo criado no Secrets Manager é prod/banco-cafe (desfeito)."; }
                    if (env.Keys.Any(k => k != "DB_HOST" && k != "DB_USUARIO" && k != "SEGREDO_ID" && k != "DB_SENHA")) { e53env(s); return "Apareceu uma variável não pedida (" + env.Keys.First(k => k != "DB_HOST" && k != "DB_USUARIO" && k != "SEGREDO_ID" && k != "DB_SENHA") + ") — desfeito."; }
                    if (env.ContainsKey("DB_SENHA")) return null;
                    if (!env.ContainsKey("SEGREDO_ID")) { e53env(s); return "Você tirou a senha, mas a função não sabe onde buscá-la: adicione SEGREDO_ID = prod/banco-cafe (desfeito)."; }
                    return OK;
                },
                Solve = s => s.SetEnv("importar-estoque", new Dictionary<string, string> { { "DB_HOST", "banco-cafe.interno" }, { "DB_USUARIO", "estoque" }, { "SEGREDO_ID", "prod/banco-cafe" } })
            };

            // e54 — rotação de chave de acesso sem derrubar o pipeline
            T["e54"] = new AwsTask
            {
                Setup = s => { ExtraAwsUser(s, "ci-github"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("iam:")) return null;
                    var p = e.Split(':');
                    if (p.Length < 3) return null;
                    if (p[2] != "ci-github" && (p[1] == "key-status" || p[1] == "key-delete" || p[1] == "key-create" || p[1] == "delete"))
                    {
                        ExtraAwsUser(s, p[2]);
                        return "Você mexeu nas credenciais de " + p[2] + " (restauradas). A chave antiga é a do usuário ci-github.";
                    }
                    if (p[1] == "delete") { ExtraAwsUser(s, "ci-github"); return "Excluir o usuário derruba todos os deploys (restaurado). Faça a rotação da chave."; }
                    const string old = "AKIA5CIGITHUB2024OLD";
                    if (p[1] == "key-delete" && p[3] == old) { s.AccessKeys.Add(new AwsSim.AccessKey { Id = old, User = "ci-github", Status = "Active", Created = DateTime.Now.AddDays(-410) }); return "Não exclua a chave antiga ainda: primeiro DESATIVE (se algum job esquecido quebrar, dá para reativar); exclua depois de alguns dias sem problemas (chave restaurada)."; }
                    if (p[1] == "key-status")
                    {
                        var k = s.AccessKeys.FirstOrDefault(x => x.Id == p[3]);
                        if (p[3] != old) { if (p[4] == "Inactive" && k != null) { k.Status = "Active"; return "Essa é a chave NOVA, que o pipeline vai usar (reativada). Desative a antiga, " + old + "."; } return null; }
                        if (p[4] != "Inactive") return null;
                        if (s.AccessKeys.Any(x => x.User == "ci-github" && x.Id != old && x.Status == "Active")) return OK;
                        k.Status = "Active";
                        return "O pipeline PAROU: a ci-github ficou sem nenhuma chave ativa (reativada)! Ordem certa: criar a chave nova → atualizar o segredo do GitHub → desativar a antiga.";
                    }
                    return null;
                },
                Solve = s => { string id; s.CreateAccessKey("ci-github", out id); s.SetKeyStatus("ci-github", "AKIA5CIGITHUB2024OLD", "Inactive"); }
            };

            // e55 — registro DNS órfão (subdomain takeover)
            T["e55"] = new AwsTask
            {
                Setup = s => { ExtraAwsRecords(s); s.Buckets.RemoveAll(b => b.Name == "cafe365-blackfriday"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("r53:")) return null;
                    var p = e.Split(':');
                    if (p[1] == "delete" && p[2] == "promo.cafe365.com.br") return OK;
                    ExtraAwsRecords(s);
                    if (p[1] == "delete") return "O registro " + p[2] + " " + p[3] + " está em uso: ele aponta para " + p[4] + ", que existe (restaurado). Procure o que aponta para um bucket que NÃO existe mais.";
                    if (p[2] == "promo.cafe365.com.br") return "Mudar o destino não resolve: a campanha acabou e o subdomínio não deve mais existir. Exclua o registro (desfeito).";
                    return "A tarefa é excluir o registro órfão, não alterar outros (desfeito).";
                },
                Solve = s => s.ChangeRecord("DELETE", "promo.cafe365.com.br", "CNAME", "cafe365-blackfriday.s3-website-us-east-1.amazonaws.com", 300)
            };

            // e56 — PITR + proteção contra exclusão
            T["e56"] = new AwsTask
            {
                Setup = s => { foreach (var t in s.Tables) { t.Pitr = false; t.Protect = false; } },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ddb:pitr:") && !e.StartsWith("ddb:protect:")) return null;
                    var p = e.Split(':');
                    var t = s.Tb(p[2]);
                    if (p[2] != "historico-pedidos") { if (t != null) { t.Pitr = false; t.Protect = false; } return "Você alterou a tabela " + p[2] + " (desfeito). A tabela crítica é a historico-pedidos."; }
                    if (p[3] != "on") return null;
                    return t.Pitr && t.Protect ? OK : null;
                },
                Solve = s => { s.SetPitr("historico-pedidos", true); s.SetTableProtect("historico-pedidos", true); }
            };

            // e57 — simultaneidade reservada
            T["e57"] = new AwsTask
            {
                Setup = s => { foreach (var f in s.Functions.ToList()) ExtraAwsFn(s, f.Name); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("lambda:")) return null;
                    var p = e.Split(':');
                    if (p.Length < 4) return null;
                    if (p[1] == "config" || p[1] == "env") { ExtraAwsFn(s, p[2]); return "A correção é limitar a simultaneidade, não mexer na configuração (desfeito)."; }
                    if (p[1] != "concurrency") return null;
                    var f = s.Func(p[2]);
                    int n = int.Parse(p[3]);
                    if (p[2] != "importar-estoque") { f.Concurrency = -1; return "Você limitou a função " + p[2] + " (desfeito). Quem esgota as conexões do banco é a importar-estoque."; }
                    if (n == 10) return OK;
                    if (n < 0) return null;
                    f.Concurrency = -1;
                    if (n == 0) return "Simultaneidade reservada 0 DESLIGA a função: nenhuma importação roda (desfeito). Faça a conta das conexões que sobram no banco.";
                    return "Com " + n + " execuções simultâneas, " + (n > 10 ? "o banco ainda estoura: são 60 conexões, a API já usa até 50, sobram 10" : "a importação fica mais lenta que o necessário: sobram 10 conexões (60 − 50 da API)") + " (desfeito).";
                },
                Solve = s => s.SetConcurrency("importar-estoque", 10)
            };

            // e58 — sub-rede privada nova
            T["e58"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; var ids = new AwsSim().Subnets.Select(x => x.Id).ToList(); s.Subnets.RemoveAll(x => !ids.Contains(x.Id)); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("vpc:subnet:")) return null;
                    var p = e.Split(':');
                    string id = p[2], cidr = p[4], az = p[5], name = p.Length > 6 ? p[6] : "";
                    Action undo = () => s.Subnets.RemoveAll(x => x.Id == id);
                    if (cidr != "10.0.11.0/24")
                    {
                        undo();
                        if (cidr.StartsWith("10.0.") && cidr.EndsWith("/24")) { int third = int.Parse(cidr.Split('.')[2]); if (third < 10) return "O bloco " + cidr + " está na faixa das sub-redes PÚBLICAS (10.0.1.x, 10.0.2.x...). As privadas seguem 10.0.10.0/24, 10.0.11.0/24... (sub-rede excluída)."; }
                        if (!cidr.EndsWith("/24")) return "O bloco ficou " + cidr + ". O padrão da empresa é /24 por sub-rede (256 endereços), como a privada-a (sub-rede excluída).";
                        return "O bloco ficou " + cidr + ". Seguindo o padrão das privadas, o próximo /24 livre depois de 10.0.10.0/24 é 10.0.11.0/24 (sub-rede excluída).";
                    }
                    if (az != "us-east-1b") { undo(); return "A sub-rede ficou na zona " + az + ". A privada-a já está em us-east-1a: a nova vai em us-east-1b, para os servidores sobreviverem à queda de uma zona (excluída)."; }
                    if (name != "privada-b") { undo(); return (name.Length == 0 ? "A sub-rede ficou sem nome" : "O nome ficou '" + name + "'") + ". A automação da equipe procura privada-b (sub-rede excluída)."; }
                    return OK;
                },
                Solve = s => { string id; s.CreateSubnet("vpc-0cafe365", "10.0.11.0/24", "us-east-1b", "privada-b", out id); }
            };

            // e59 — restaurar objeto excluído (remover o marcador de exclusão)
            const string marker = "QX7ZK2PLM9NDR4TB";
            Action<AwsSim> e59setup = s =>
            {
                var b = ExtraAwsSite(s);
                b.Versioning = "Ativado";
                b.Objects.Remove("index.html");
                b.Markers.Clear();
                b.Markers.Add(new AwsSim.Marker { Key = "index.html", VersionId = marker });
            };
            T["e59"] = new AwsTask
            {
                Setup = s => e59setup(s),
                Eval = (s, e) =>
                {
                    var p = e.Split(':');
                    if (e == "s3:unmark:cafe365-site:index.html") return OK;
                    if (e.StartsWith("s3:upload:cafe365-site:"))
                    {
                        string key = e.Substring("s3:upload:cafe365-site:".Length);
                        if (key == "index.html") { e59setup(s); return "A cópia do seu computador é de 3 meses atrás (sem as promoções novas): ela foi removida. Restaure a versão que estava no bucket, removendo o MARCADOR de exclusão."; }
                        return null;
                    }
                    if (e.StartsWith("s3:delver:") || e.StartsWith("s3:delobj:") || e.StartsWith("s3:delmany:") || e.StartsWith("s3:unmark:"))
                    {
                        e59setup(s);
                        return "Você apagou outra coisa do bucket (restaurado). Só o marcador de exclusão do index.html deve ser removido.";
                    }
                    if (e.StartsWith("s3:versioning:cafe365-site:") && p[3] != "Enabled") { e59setup(s); return "Não suspenda o versionamento: é ele que guarda a versão anterior (reativado)."; }
                    return null;
                },
                Solve = s => s.DeleteVersion("cafe365-site", "index.html", marker)
            };

            // e60 — tempo limite de visibilidade = 6x o da função
            T["e60"] = new AwsTask
            {
                Setup = s => { ExtraAwsQueue(s, "pedidos"); ExtraAwsQueue(s, "pedidos-dlq"); ExtraAwsFn(s, "processar-pedidos"); s.Func("processar-pedidos").Triggers.Add(new[] { "pedidos", "10" }); },
                Eval = (s, e) =>
                {
                    var p = e.Split(':');
                    if (e.StartsWith("lambda:config:processar-pedidos:") || e.StartsWith("lambda:concurrency:processar-pedidos:"))
                    {
                        ExtraAwsFn(s, "processar-pedidos"); s.Func("processar-pedidos").Triggers.Add(new[] { "pedidos", "10" });
                        return "A função precisa mesmo de até 50 s (ela chama o sistema de pagamento). Ajuste a FILA, não a função (desfeito).";
                    }
                    if (!e.StartsWith("sqs:attr:") && !e.StartsWith("sqs:redrive:")) return null;
                    if (p[2] != "pedidos") { ExtraAwsQueue(s, p[2]); return "Você alterou a fila " + p[2] + " (desfeito). Quem dispara a função é a fila pedidos."; }
                    if (p[1] == "redrive" || p[3] != "VisibilityTimeout") { ExtraAwsQueue(s, "pedidos"); return "Só o tempo limite de visibilidade deve mudar (desfeito)."; }
                    int v = int.Parse(p[4]);
                    if (v == 300) return OK;
                    ExtraAwsQueue(s, "pedidos");
                    if (v <= 50) return "Com " + v + " s, a mensagem reaparece enquanto a função ainda processa (até 50 s) e o pedido é cobrado em dobro (desfeito).";
                    return v < 300 ? "Melhorou, mas a AWS recomenda pelo menos 6 vezes o tempo limite da função (para cobrir as novas tentativas e o lote): 6 × 50 = 300 s (desfeito)." : "Com " + v + " s, uma mensagem que falhar demora demais para ser tentada de novo. A recomendação é 6 × o tempo limite da função = 300 s (desfeito).";
                },
                Solve = s => s.SetQueueAttr("pedidos", "VisibilityTimeout", 300)
            };

            // e61 — trocar chave fixa por função do IAM na instância
            T["e61"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; foreach (var i in s.Instances) i.Role = null; ExtraAwsUser(s, "app-web"); },
                Eval = (s, e) =>
                {
                    var p = e.Split(':');
                    const string key = "AKIA5APPWEB0SRV00001";
                    if (e.StartsWith("ec2:role:"))
                    {
                        if (p[2] != "i-0web001") { var i = s.Inst(p[2]); if (i != null) i.Role = null; return "A função foi associada a " + MaisAwsInstName(s, p[2]) + " (desfeito). A aplicação que usa a chave roda no web-1."; }
                        return null;
                    }
                    if (!e.StartsWith("iam:") || p.Length < 3) return null;
                    bool hasRole = s.Inst("i-0web001").Role == "app-s3-leitura";
                    if (p[2] != "app-web" && (p[1] == "key-delete" || p[1] == "key-status" || p[1] == "delete")) { ExtraAwsUser(s, p[2]); return "Você mexeu nas credenciais de " + p[2] + " (restauradas). A chave guardada no servidor é a do app-web."; }
                    if (p[1] == "delete") { ExtraAwsUser(s, "app-web"); return "Exclua só a CHAVE de acesso do app-web, não o usuário (restaurado)."; }
                    if ((p[1] == "key-delete" || (p[1] == "key-status" && p[4] == "Inactive")) && p[3] == key)
                    {
                        if (!hasRole) { ExtraAwsUser(s, "app-web"); return "A aplicação PAROU: ela ainda usava essa chave, porque o web-1 não tinha a função do IAM (chave restaurada). Primeiro associe a função app-s3-leitura ao web-1, depois remova a chave."; }
                        return p[1] == "key-delete" ? OK : null;
                    }
                    return null;
                },
                Solve = s => { s.AssociateRole("i-0web001", "app-s3-leitura"); s.DeleteAccessKey("app-web", "AKIA5APPWEB0SRV00001"); }
            };

            // e62 — trilha do CloudTrail
            T["e62"] = new AwsTask
            {
                Setup = s => { s.Trails.Clear(); MaisAwsBucket(s, "micorosoft-auditoria"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ct:")) return null;
                    var p = e.Split(':');
                    var t = s.Trails.FirstOrDefault(x => x.Name == p[2]);
                    if (p[1] == "create")
                    {
                        Action undo = () => s.Trails.Remove(t);
                        if (p[2] != "auditoria") { undo(); return "Trilha criada como '" + p[2] + "' (excluída). O nome pedido é auditoria."; }
                        if (p[3] != "micorosoft-auditoria") { undo(); return "Os logs iriam para o bucket " + p[3] + ". O bucket próprio para auditoria (com acesso restrito) é o micorosoft-auditoria (trilha excluída)."; }
                        if (p[4] != "multi") { undo(); return "Trilha de uma região só não vê o que acontece nas outras — onde um invasor costuma criar máquinas escondidas. Aplique a todas as regiões (trilha excluída)."; }
                        if (p[5] != "valid") { undo(); return "Sem a validação dos arquivos de log, não dá para provar que ninguém alterou ou apagou um log. Ative-a (trilha excluída)."; }
                        return null;
                    }
                    if (p[1] == "start" && p[2] == "auditoria" && t != null) return OK;
                    return null;
                },
                Solve = s => s.CreateTrail("auditoria", "micorosoft-auditoria", true, true, true)
            };

            // e63 — política de rastreamento de destino
            T["e63"] = new AwsTask
            {
                Setup = s => { var a = s.Group("asg-web"); a.Min = 2; a.Max = 6; a.Desired = 2; a.Policies.Clear(); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("asg:")) return null;
                    var p = e.Split(':');
                    var a = s.Group("asg-web");
                    if (p[1] == "capacity") { a.Min = 2; a.Max = 6; a.Desired = 2; return "Os limites (2 a 6) já estão certos: não mexa neles (desfeito). Crie a política de escalabilidade."; }
                    if (p[1] != "policy") return null;
                    string name = p[3], metric = p[5];
                    float target = float.Parse(p[6], System.Globalization.CultureInfo.InvariantCulture);
                    Action undo = () => a.Policies.RemoveAll(x => x[0] == name);
                    if (name != "cpu-50") { undo(); return "A política ficou com o nome '" + name + "' (excluída). O nome pedido é cpu-50."; }
                    if (metric != "ASGAverageCPUUtilization") { undo(); return "A métrica escolhida não acompanha o gargalo do site, que é CPU. Use a utilização média da CPU (política excluída)."; }
                    if (Math.Abs(target - 50) > 0.01) { undo(); return target > 50 ? "Com destino de " + target + "%, o grupo só escala quando a CPU já está quase no limite: não sobra folga enquanto as instâncias novas sobem. O pedido é 50% (política excluída)." : "Com destino de " + target + "%, o grupo mantém instâncias demais quase ociosas (custo). O pedido é 50% (política excluída)."; }
                    return OK;
                },
                Solve = s => s.PutScalingPolicy("asg-web", "cpu-50", "TargetTrackingScaling", "ASGAverageCPUUtilization", 50)
            };

            // e64 — rota da sub-rede privada pela NAT
            T["e64"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; ExtraAwsRoutes(s); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("vpc:route:") && !e.StartsWith("vpc:delroute:")) return null;
                    var p = e.Split(':');
                    string rtb = p[2], dest = p[3], target = p[4];
                    if (p[1] == "delroute") { ExtraAwsRoutes(s); return "Você removeu a rota " + dest + " → " + target + " de " + rtb + " (restaurada)" + (rtb == "rtb-0publica" ? ": as sub-redes públicas perderiam a internet!" : ".") + " A tarefa é ADICIONAR a saída da privada."; }
                    if (rtb != "rtb-0privada") { ExtraAwsRoutes(s); return "Você alterou a tabela " + rtb + " (desfeito). Quem não tem saída é a rt-privada (rtb-0privada)."; }
                    if (dest != "0.0.0.0/0") { ExtraAwsRoutes(s); return "O destino ficou " + dest + ". Para 'qualquer endereço da internet' o destino é 0.0.0.0/0 (desfeito)."; }
                    if (target.StartsWith("igw-")) { ExtraAwsRoutes(s); return "Rota para o gateway da Internet transforma a sub-rede em PÚBLICA — e os servidores privados nem têm IP público, então nem funcionaria. Saída sem entrada é com o gateway NAT (desfeito)."; }
                    return target == "nat-0cafe01" ? OK : null;
                },
                Solve = s => s.SetRoute("rtb-0privada", "0.0.0.0/0", "nat-0cafe01", false)
            };

            // e65 — site acessível só pela CloudFront (ordem importa)
            Action<AwsSim> e65setup = s =>
            {
                var b = ExtraAwsSite(s);
                b.BlockPublic = false;
                b.Policy = "publica";
                b.PolicyText = AwsSim.JsonFile("politica-leitura-publica.json");
            };
            T["e65"] = new AwsTask
            {
                Setup = s => e65setup(s),
                Eval = (s, e) =>
                {
                    var p = e.Split(':');
                    if (e.StartsWith("s3:bpa:"))
                    {
                        var b = s.Bkt(p[2]);
                        if (p[2] != "cafe365-site") { if (b != null) b.BlockPublic = true; return "Você mexeu no bucket " + p[2] + " (desfeito). O pedido é o cafe365-site."; }
                        if (p[3] != "on") return null;
                        if (b.Policy == "cloudfront") return OK;
                        b.BlockPublic = false;
                        return "O SITE CAIU: a CloudFront lia o bucket pelo acesso público, e você bloqueou antes de dar permissão a ela (bloqueio desfeito). Ordem: primeiro a política que libera a CloudFront, depois o bloqueio.";
                    }
                    if (!e.StartsWith("s3:policy:")) return null;
                    if (p[2] != "cafe365-site") { var bb = s.Bkt(p[2]); if (bb != null) { bb.Policy = null; bb.PolicyText = null; } return "Você alterou a política do bucket " + p[2] + " (desfeito)."; }
                    var site = s.Bkt("cafe365-site");
                    if (p[3] == "cloudfront") return site.BlockPublic ? OK : null;
                    e65setup(s);
                    if (p[3] == "none") return "Sem política nenhuma, nem a CloudFront consegue ler os arquivos: o site caiu (política restaurada). Troque pela política que libera só a CloudFront.";
                    return "Essa política não libera a CloudFront (política restaurada). Use a que tem o principal cloudfront.amazonaws.com e a condição AWS:SourceArn da distribuição.";
                },
                Solve = s => { s.SetPolicy("cafe365-site", AwsSim.JsonFile("politica-cloudfront-site.json")); s.SetBlockPublic("cafe365-site", true); }
            };

            // e66 — reduzir o TTL antes da migração
            T["e66"] = new AwsTask
            {
                Setup = s => { ExtraAwsRecords(s); s.Records.First(r => r.Name == "api.cafe365.com.br").Ttl = 86400; },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("r53:")) return null;
                    var p = e.Split(':');
                    Action undo = () => { ExtraAwsRecords(s); s.Records.First(r => r.Name == "api.cafe365.com.br").Ttl = 86400; };
                    if (p[2] != "api.cafe365.com.br") { undo(); return "Você alterou o registro " + p[2] + " (desfeito). A migração é da API: api.cafe365.com.br."; }
                    if (p[1] == "delete") { undo(); return "Sem o registro, a API sai do ar para todo mundo (restaurado). Só o TTL deve mudar."; }
                    if (p[4] != "54.210.12.8") { undo(); return "Ainda não é hora de trocar o IP: a migração é amanhã, e com TTL de 1 dia metade dos clientes continuaria no IP antigo por horas. Hoje, só reduza o TTL (desfeito)."; }
                    int ttl = int.Parse(p[5]);
                    if (ttl == 60) return OK;
                    undo();
                    return ttl > 60 ? "Com TTL de " + ttl + " s, depois da troca alguns clientes ainda usariam o IP velho por até " + (ttl >= 3600 ? ttl / 3600 + " hora(s)" : ttl / 60 + " minuto(s)") + ". O combinado é 60 s (desfeito)." : "TTL de " + ttl + " s multiplica as consultas DNS sem ganho real. O combinado é 60 s (desfeito).";
                },
                Solve = s => s.ChangeRecord("UPSERT", "api.cafe365.com.br", "A", "54.210.12.8", 60)
            };

            // e67 — alarme na DLQ
            T["e67"] = new AwsTask
            {
                Setup = s => { s.Alarms.RemoveAll(a => a.Name == "dlq-pedidos"); ExtraAwsQueue(s, "pedidos"); ExtraAwsQueue(s, "pedidos-dlq"); var t = s.Tp("alertas-ti"); if (t == null) s.CreateTopicQuiet("alertas-ti"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("cw:alarm:")) return null;
                    var p = e.Split(':');
                    string name = p[2], metric = p[3], res = p[4], th = p[5], ns = p.Length > 6 ? p[6] : "", comp = p.Length > 7 ? p[7] : "", act = p.Length > 8 ? p[8] : "";
                    Action undo = () => s.Alarms.RemoveAll(a => a.Name == name);
                    if (name != "dlq-pedidos") { undo(); return "Alarme criado como '" + name + "' (excluído). O nome pedido é dlq-pedidos."; }
                    if (ns != "AWS/SQS") { undo(); return "O namespace ficou " + ns + ". Métricas de fila ficam em AWS/SQS (alarme excluído)."; }
                    if (res == "pedidos") { undo(); return "A fila PRINCIPAL sempre tem mensagens (é o movimento normal): o alarme tocaria o dia todo. O sinal de problema é mensagem na DLQ, pedidos-dlq (alarme excluído)."; }
                    if (res != "pedidos-dlq") { undo(); return "O alarme monitora a fila " + res + ". O pedido é a pedidos-dlq (alarme excluído)."; }
                    if (metric == "NumberOfMessagesSent") { undo(); return "Mensagens movidas para a DLQ pela redrive NÃO contam em NumberOfMessagesSent (só envios diretos). Use ApproximateNumberOfMessagesVisible (alarme excluído)."; }
                    if (metric != "ApproximateNumberOfMessagesVisible") { undo(); return "A métrica ficou " + metric + ". Para 'tem mensagem parada na DLQ', use ApproximateNumberOfMessagesVisible (alarme excluído)."; }
                    if (th != "0" || comp.StartsWith("Less")) { undo(); return "Qualquer mensagem na DLQ já é problema: a condição é MAIOR QUE 0 (alarme excluído)."; }
                    if (act != "alertas-ti") { undo(); return act.Length == 0 ? "Sem notificação, ninguém fica sabendo. Notifique o tópico alertas-ti (alarme excluído)." : "A notificação foi para " + act + ". Use o tópico da equipe, alertas-ti (alarme excluído)."; }
                    return OK;
                },
                Solve = s => s.CreateAlarm("dlq-pedidos", "ApproximateNumberOfMessagesVisible", "pedidos-dlq", 0, "AWS/SQS", "GreaterThanThreshold", "alertas-ti")
            };

            // e68 — expirar só as versões desatualizadas
            T["e68"] = new AwsTask
            {
                Setup = s => { var b = ExtraAwsSite(s); b.Versioning = "Ativado"; b.Lifecycle.Clear(); },
                Eval = (s, e) =>
                {
                    var p = e.Split(':');
                    if (e.StartsWith("s3:versioning:cafe365-site:") && p[3] != "Enabled") { s.Bkt("cafe365-site").Versioning = "Ativado"; return "Não suspenda o versionamento: ele é a proteção contra sobrescritas (reativado). Controle o custo com uma regra de ciclo de vida."; }
                    if (!e.StartsWith("s3:lifecycle:")) return null;
                    var b = s.Bkt(p[2]);
                    string id = p[3], pre = p[4];
                    int exp = int.Parse(p[5]), nc = int.Parse(p[6]), gl = int.Parse(p[7]);
                    Action undo = () => { if (b != null) b.Lifecycle.RemoveAll(r => r.Id == id); };
                    if (p[2] != "cafe365-site") { undo(); return "A regra foi criada no bucket " + p[2] + " (excluída). As versões acumulam no cafe365-site."; }
                    if (exp > 0) { undo(); return "PERIGO: 'expirar versões atuais' apagaria o SITE NO AR depois de " + exp + " dias! O pedido é só sobre as versões antigas (regra excluída)."; }
                    if (gl > 0) { undo(); return "Mover para o Glacier deixaria o site lento/indisponível. Não foi pedido (regra excluída)."; }
                    if (pre.Length > 0) { undo(); return "A regra ficou limitada ao prefixo '" + pre + "'. Ela deve valer para o bucket inteiro (excluída)."; }
                    if (nc != 30) { undo(); return nc == 0 ? "Marque 'Excluir permanentemente versões desatualizadas' com 30 dias (regra excluída)." : "As versões antigas ficariam " + nc + " dias. O combinado é 30 dias: tempo de perceber um erro e voltar atrás (regra excluída)."; }
                    if (id != "limpar-versoes-antigas") { undo(); return "A regra ficou com o nome '" + id + "' (excluída). O nome pedido é limpar-versoes-antigas."; }
                    return OK;
                },
                Solve = s => s.PutLifecycle("cafe365-site", new List<AwsSim.LcRule> { new AwsSim.LcRule { Id = "limpar-versoes-antigas", Noncurrent = 30 } }, false)
            };

            // e69 — desligamento completo de um usuário
            T["e69"] = new AwsTask
            {
                Setup = s => { ExtraAwsUser(s, "joao"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("iam:")) return null;
                    var p = e.Split(':');
                    if (p.Length < 3) return null;
                    if (p[1] == "delete") { if (p[2] == "joao") return OK; ExtraAwsUser(s, p[2]); return "Você excluiu " + p[2] + ", que continua na empresa (restaurado)! Quem saiu foi o joao."; }
                    if (p[2] != "joao" && (p[1] == "ungroup" || p[1] == "key-delete" || p[1] == "key-status" || p[1] == "detach")) { ExtraAwsUser(s, p[2]); return "Você mexeu no acesso de " + p[2] + " (restaurado). Quem saiu foi o joao."; }
                    return null;
                },
                Solve = s => { s.RemoveFromGroup("joao", "Desenvolvedores"); s.DeleteAccessKey("joao", "AKIA5JOAO0000000DEV1"); s.DeleteUserStrict("joao"); }
            };

            // e70 — Multi-AZ + proteção contra exclusão
            T["e70"] = new AwsTask
            {
                Setup = s => { ExtraAwsDb(s, "banco-cafe"); ExtraAwsDb(s, "banco-dev"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("rds:")) return null;
                    var p = e.Split(':');
                    if (p[1] == "stop" || p[1] == "reboot") { ExtraAwsDb(s, p[2]); return "A tarefa não pede derrubar o banco (" + p[2] + " voltou)."; }
                    if (p[1] != "modify") return null;
                    if (p[2] != "banco-cafe") { ExtraAwsDb(s, p[2]); return p[2] == "banco-dev" ? "O banco de testes não precisa de Multi-AZ (dobraria o custo dele) — desfeito. O pedido é o banco-cafe." : "Você modificou o " + p[2] + " (desfeito)."; }
                    var ch = ExtraAwsKv(p[3]);
                    var d = s.DbI("banco-cafe");
                    string v;
                    if (ch.TryGetValue("class", out v)) { ExtraAwsDb(s, "banco-cafe"); return "Você trocou a classe da instância: isso REINICIA o banco (desfeito). Só Multi-AZ e proteção contra exclusão."; }
                    if (ch.Keys.Any(k => k != "multiaz" && k != "protect")) { ExtraAwsDb(s, "banco-cafe"); return "Você mudou também: " + string.Join(", ", ch.Keys.Where(k => k != "multiaz" && k != "protect")) + ". Só Multi-AZ e proteção contra exclusão (desfeito)."; }
                    if (ch.Values.Any(x => x == "false")) { ExtraAwsDb(s, "banco-cafe"); return "Algo foi DESATIVADO (desfeito). O pedido é ativar Multi-AZ e a proteção contra exclusão."; }
                    if (p[4] == "window") { ExtraAwsDb(s, "banco-cafe"); return "A mudança ficou para a janela de manutenção de domingo, mas a auditoria é amanhã. A conversão para Multi-AZ não derruba o banco: aplique imediatamente (desfeito)."; }
                    return d.MultiAz && d.Protect ? OK : null;
                },
                Solve = s => s.ModifyDb("banco-cafe", new Dictionary<string, string> { { "multiaz", "true" }, { "protect", "true" } }, true)
            };

            // e71 — ciclo de vida do ECR
            T["e71"] = new AwsTask
            {
                Setup = s => { var r = s.RepoOf("cafe365-web"); r.LcTag = null; r.LcType = null; r.LcN = 0; },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ecr:lifecycle:")) return null;
                    var p = e.Split(':');
                    var r = s.RepoOf(p[2]);
                    Action undo = () => { if (r != null) { r.LcTag = null; r.LcType = null; r.LcN = 0; } };
                    if (p[2] != "cafe365-web") { undo(); return "A regra foi criada no repositório " + p[2] + " (excluída). O pedido é o cafe365-web."; }
                    if (p[3] == "any") { undo(); return "Com 'Qualquer', a regra apagaria também as imagens COM TAG — inclusive a que está rodando em produção! O lixo são as imagens SEM tag (regra excluída)."; }
                    if (p[4] != "sinceImagePushed") { undo(); return "O critério ficou por quantidade de imagens. O pedido é por idade: imagens sem tag com mais de 7 dias (regra excluída)."; }
                    if (p[5] != "7") { undo(); return "O prazo ficou " + p[5] + " dia(s). O combinado é 7 dias (regra excluída)."; }
                    return OK;
                },
                Solve = s => s.SetEcrLifecycle("cafe365-web", "untagged", "sinceImagePushed", 7)
            };

            // e72 — criptografia padrão do EBS nas duas regiões
            T["e72"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; s.EbsDefault.Clear(); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:ebs-default:")) return null;
                    var p = e.Split(':');
                    if (p[3] != "on") return null;
                    if (s.EbsDefaultOn("us-east-1") && s.EbsDefaultOn("sa-east-1")) return OK;
                    return null;
                },
                Solve = s => { s.SetEbsDefault("us-east-1", true); s.SetEbsDefault("sa-east-1", true); }
            };
        }

        static partial void ExtraAws(List<Level> list)
        {
            ExtraAwsTasks();

            // ───────────────────────── Iniciante ─────────────────────────
            list.Add(AwsL(0, "e1", "Etiqueta no bucket",
                "A Ana (FinOps) vai separar a fatura do S3 por projeto. O relatório de custos agrupa os buckets pela tag Projeto.",
                "No bucket cafe365-site, adicione a tag Projeto com o valor cafe365.",
                A("Console: S3 → cafe365-site → Propriedades → Tags → Editar tags → Chave Projeto, Valor cafe365 → Salvar alterações", "aws s3api put-bucket-tagging --bucket cafe365-site --tagging 'TagSet=[{Key=Projeto,Value=cafe365}]'"),
                A("As tags de um bucket ficam na aba Propriedades dele.", "Chave Projeto (P maiúsculo), valor cafe365. Na CLI: s3api put-bucket-tagging com TagSet=[{Key=...,Value=...}]."),
                A("Tag de bucket", "Par chave/valor que aparece nos relatórios de custo (depois de ativada como tag de alocação de custos).", "put-bucket-tagging", "SUBSTITUI todas as tags do bucket: se já houver outras, mande-as junto."),
                "Combine um padrão de tags (Projeto, Dono, Ambiente) para todos os recursos: sem tag, ninguém sabe de quem é a conta."));
            list.Add(AwsL(0, "e2", "Tópico de alertas",
                "A Helena quer que os alarmes da conta avisem a equipe de TI. O primeiro passo é ter um 'canal' de notificações no SNS.",
                "No SNS, crie um tópico do tipo Padrão chamado alertas-ti.",
                A("Console: Simple Notification Service → Tópicos → Criar tópico → Tipo Padrão → Nome alertas-ti → Criar tópico", "aws sns create-topic --name alertas-ti"),
                A("SNS = Simple Notification Service: procure por 'tópico' ou 'notificação' na pesquisa.", "Tipo Padrão (o FIFO não entrega e-mail). Na CLI: aws sns create-topic --name ..."),
                A("Tópico do SNS", "Canal que recebe uma mensagem e entrega uma cópia para cada assinante.", "Padrão x FIFO", "FIFO garante a ordem, mas só entrega para filas SQS; alertas por e-mail usam Padrão."),
                "Um tópico por finalidade (alertas, deploys...) permite mudar quem recebe sem mexer nos alarmes."));
            list.Add(AwsL(0, "e3", "Fila de e-mails",
                "Na Black Friday, o envio dos e-mails de confirmação trava o checkout. O Rodrigo quer deixar os e-mails numa fila para serem enviados aos poucos.",
                "No SQS, crie uma fila do tipo Padrão chamada emails-pendentes.",
                A("Console: Simple Queue Service → Filas → Criar fila → Tipo Padrão → Nome emails-pendentes → Criar fila", "aws sqs create-queue --queue-name emails-pendentes"),
                A("SQS = Simple Queue Service (filas de mensagens).", "Tipo Padrão, nome emails-pendentes. Na CLI: aws sqs create-queue --queue-name ..."),
                A("Fila", "O checkout só deixa a mensagem na fila e segue; outro processo envia o e-mail depois.", "Fila Padrão", "Vazão quase ilimitada; pode entregar fora de ordem e, raramente, duas vezes."),
                "Filas desacoplam sistemas: se o servidor de e-mail cair, as mensagens esperam em vez de se perder."));
            list.Add(AwsL(0, "e4", "Caçando o erro",
                "A função pedidos gera milhares de linhas de log por hora. O Rodrigo quer ver só as linhas de erro para entender o problema.",
                "No CloudWatch, filtre os eventos do grupo de logs /aws/lambda/pedidos pelo termo ERROR.",
                A("Console: CloudWatch → Grupos de logs → /aws/lambda/pedidos → campo de filtro: ERROR → Enter (ou Filtrar)", "aws logs filter-log-events --log-group-name /aws/lambda/pedidos --filter-pattern ERROR"),
                A("Abra o grupo de logs da função e use o campo de filtro dos eventos.", "O filtro diferencia maiúsculas: ERROR. Na CLI: aws logs filter-log-events --log-group-name ... --filter-pattern ..."),
                A("Padrão de filtro", "Mostra só os eventos que contêm o termo (maiúsculas importam).", "filter-log-events", "Busca no grupo inteiro; 'logs tail' só acompanha as linhas novas."),
                "Padronize o formato dos logs (ex.: [ERROR], JSON): fica fácil filtrar e até criar métricas e alarmes a partir deles."));
            list.Add(AwsL(0, "e5", "Espiando o cardápio",
                "Um cliente reclamou que o preço do café expresso no app está errado. O Rodrigo pediu para você conferir o que está gravado no banco NoSQL.",
                "No DynamoDB, explore (verifique) os itens da tabela cardapio.",
                A("Console: DynamoDB → Tabelas → cardapio → Explorar itens da tabela", "aws dynamodb scan --table-name cardapio"),
                A("DynamoDB → Tabelas → clique na tabela.", "Use 'Explorar itens da tabela'. Na CLI: aws dynamodb scan --table-name ..."),
                A("Scan (verificação)", "Lê TODOS os itens da tabela: tudo bem para conferir uma tabela pequena.", "Query (consulta)", "Busca pela chave de partição: muito mais barata em tabelas grandes."),
                "Em produção, evite scan em tabelas grandes: ele consome capacidade proporcional à tabela inteira."));
            list.Add(AwsL(0, "e6", "Banco de testes dormindo",
                "A Ana notou que o banco de testes fica ligado de madrugada e nos fins de semana, sem ninguém usando.",
                "No RDS, interrompa temporariamente o banco banco-dev. Não mexa no banco-cafe (produção).",
                A("Console: RDS → Bancos de dados → selecione banco-dev → Ações → Interromper temporariamente → Sim", "aws rds stop-db-instance --db-instance-identifier banco-dev"),
                A("Selecione a linha do banco certo e procure o menu Ações.", "Ações → Interromper temporariamente. Na CLI: aws rds stop-db-instance --db-instance-identifier ..."),
                A("Interromper temporariamente", "Para a cobrança da instância (o armazenamento continua sendo cobrado).", "7 dias", "O RDS religa sozinho depois de 7 dias parado."),
                "Ambientes de teste podem ser desligados fora do expediente com agendamento automático: economia fácil."));
            list.Add(AwsL(0, "e7", "Tempo esgotado",
                "Os e-mails de confirmação falham com 'Task timed out after 3.00 seconds'. O servidor de e-mail leva até 20 segundos para responder.",
                "Aumente o tempo limite da função Lambda notificacoes para 30 segundos.",
                A("Console: Lambda → notificacoes → Configuração → Configuração geral → Editar → Tempo limite 0 min 30 s → Salvar", "aws lambda update-function-configuration --function-name notificacoes --timeout 30"),
                A("O tempo limite fica na aba Configuração da função.", "Configuração geral → Editar → 30 segundos. Na CLI: update-function-configuration --timeout 30."),
                A("Tempo limite (timeout)", "Duração máxima de uma execução; passou disso, a Lambda interrompe (máximo 15 minutos).", "--timeout 30", "Valor em segundos."),
                "Tempo limite com folga, mas não infinito: uma execução travada com limite de 15 min custa caro e segura recursos."));
            list.Add(AwsL(0, "e8", "À prova de engano",
                "Na semana passada, alguém quase encerrou o servidor errado. A Helena quer uma trava no servidor principal do site.",
                "Ative a proteção contra encerramento da instância web-1 (i-0web001).",
                A("Console: EC2 → Instâncias → selecione web-1 → Ações → Configurações da instância → Alterar proteção contra encerramento → Ativar → Salvar", "aws ec2 modify-instance-attribute --instance-id i-0web001 --disable-api-termination"),
                A("É uma configuração da instância, no menu Ações.", "Ações → Configurações da instância → Alterar proteção contra encerramento. Na CLI: modify-instance-attribute --disable-api-termination."),
                A("Proteção contra encerramento", "Bloqueia o 'Encerrar' (console, CLI e API) até alguém desativar a proteção.", "--disable-api-termination", "Nome da opção na API: 'desativar o encerramento pela API'."),
                "Ative em tudo que é produção. Instâncias de Auto Scaling têm uma proteção própria (contra redução de escala)."));
            list.Add(AwsL(0, "e9", "Par de chaves novo",
                "O Rodrigo trocou de notebook e precisa de um par de chaves novo para acessar os servidores por SSH.",
                "No EC2 (Norte da Virgínia), crie o par de chaves rodrigo-ssh do tipo ED25519.",
                A("Console: EC2 → Pares de chaves → Criar par de chaves → nome rodrigo-ssh → tipo ED25519 → formato .pem → Criar par de chaves", "aws ec2 create-key-pair --key-name rodrigo-ssh --key-type ed25519"),
                A("Pares de chaves ficam no menu lateral do EC2 (Rede e segurança).", "Tipo ED25519. Na CLI: aws ec2 create-key-pair --key-name ... --key-type ed25519."),
                A("Par de chaves", "A AWS guarda a chave pública; a privada é baixada UMA única vez.", "ED25519", "Algoritmo moderno: chave curta, rápida e segura (não serve para Windows).", "Regional", "O par de chaves só existe na região em que foi criado."),
                "Guarde a chave privada com permissão restrita (chmod 400) e nunca no Git. Melhor ainda: Session Manager, sem chave nenhuma."));
            list.Add(AwsL(0, "e10", "Alarme fantasma",
                "Todo dia aparece 'dados insuficientes' no alarme cpu-servidor-legado. O servidor que ele monitorava foi encerrado em 2025.",
                "No CloudWatch, exclua o alarme cpu-servidor-legado. Não exclua os outros.",
                A("Console: CloudWatch → Alarmes → selecione cpu-servidor-legado → Excluir → Sim", "aws cloudwatch delete-alarms --alarm-names cpu-servidor-legado"),
                A("Selecione a linha do alarme na lista de Alarmes.", "Botão Excluir. Na CLI: aws cloudwatch delete-alarms --alarm-names NOME."),
                A("Dados insuficientes", "O alarme não recebe dados (recurso parado ou inexistente).", "delete-alarms", "Aceita vários nomes de uma vez."),
                "Alarme que toca à toa ensina a equipe a ignorar alarmes. Ao remover um recurso, remova os alarmes dele."));
            list.Add(AwsL(0, "e11", "Cache desatualizado",
                "O Rodrigo publicou um index.html novo no S3, mas os clientes continuam vendo a página antiga: a CloudFront guardou a versão anterior em cache.",
                "Na distribuição E1CAFE365SITE da CloudFront, crie uma invalidação SÓ do caminho /index.html (as imagens não mudaram).",
                A("Console: CloudFront → E1CAFE365SITE → Invalidações → Criar invalidação → /index.html → Criar invalidação", "aws cloudfront create-invalidation --distribution-id E1CAFE365SITE --paths /index.html"),
                A("Invalidação tira arquivos do cache; ela fica na página da distribuição.", "O caminho começa com barra: /index.html. Na CLI: create-invalidation --distribution-id ... --paths ..."),
                A("Invalidação", "Remove o arquivo do cache: a próxima visita busca a versão nova na origem.", "/*", "Invalida TUDO: carga extra na origem e páginas mais lentas até o cache encher de novo."),
                "Para não depender de invalidação, use nomes versionados (app.3f9a.js) com cache longo e só o HTML com cache curto."));
            list.Add(AwsL(0, "e12", "Site estático no ar",
                "O time do frontend quer testar o site direto pelo endpoint do S3, antes de ligar a CDN.",
                "Ative a hospedagem de site estático no bucket cafe365-site com o documento de índice index.html.",
                A("Console: S3 → cafe365-site → Propriedades → Hospedagem de site estático → Editar → Ativar → Documento de índice index.html → Salvar alterações", "aws s3 website s3://cafe365-site/ --index-document index.html"),
                A("Fica na aba Propriedades do bucket, no fim da página.", "Documento de índice: index.html. Na CLI: aws s3 website s3://bucket/ --index-document ..."),
                A("Hospedagem de site estático", "O S3 passa a responder como servidor web (endpoint s3-website).", "Documento de índice", "Arquivo entregue quando se pede a raiz (/) ou uma 'pasta'.", "Documento de erro", "Página para erros 4XX (opcional)."),
                "O endpoint de site do S3 é só HTTP e exige objetos públicos: em produção, use CloudFront com o bucket privado."));
            list.Add(AwsL(0, "e13", "Chave vazada",
                "Um print do terminal do Rodrigo, com a chave de acesso AKIA5RODRIGO7Q2XW3LM visível, foi parar num grupo de mensagens. A Helena quer cortar o acesso já.",
                "No IAM, DESATIVE (não exclua) a chave de acesso AKIA5RODRIGO7Q2XW3LM do usuário rodrigo.",
                A("Console: IAM → Usuários → rodrigo → Chaves de acesso → selecione a chave → Ações → Desativar", "aws iam update-access-key --user-name rodrigo --access-key-id AKIA5RODRIGO7Q2XW3LM --status Inactive"),
                A("As chaves ficam na página do usuário, seção Chaves de acesso.", "Ações → Desativar. Na CLI: update-access-key ... --status Inactive."),
                A("Desativar", "Corta o acesso na hora, mas dá para reativar se algo legítimo quebrar.", "Excluir", "Definitivo: faça depois da investigação.", "--status Inactive", "Status da chave na API."),
                "Chave vazada: desative, investigue no CloudTrail o que ela fez, crie outra para o dono e só então exclua a antiga."));
            list.Add(AwsL(0, "e14", "Logs eternos",
                "A Ana descobriu que a fatura do CloudWatch Logs dobrou: o grupo /ecs/cafe365-api guarda tudo para sempre (já são 38 GB).",
                "Configure a retenção do grupo de logs /ecs/cafe365-api para 30 dias.",
                A("Console: CloudWatch → Grupos de logs → selecione /ecs/cafe365-api → Ações → Editar configuração de retenção → 1 mês (30 dias) → Salvar", "aws logs put-retention-policy --log-group-name /ecs/cafe365-api --retention-in-days 30"),
                A("A retenção aparece na lista de grupos de logs; o menu Ações altera.", "30 dias. Na CLI: aws logs put-retention-policy --log-group-name ... --retention-in-days 30."),
                A("Retenção", "Eventos mais velhos que isso são apagados automaticamente.", "Nunca expirar", "O padrão: guarda (e cobra) para sempre.", "--retention-in-days", "Só aceita valores fixos: 1, 3, 5, 7, 14, 30, 60, 90..."),
                "Defina retenção em TODO grupo de logs; o que precisa ficar anos (auditoria) pode ser exportado para o S3, bem mais barato."));
            list.Add(AwsL(0, "e15", "Mais memória",
                "A função relatorio-vendas morre com 'Runtime exited with error: signal: killed': ela usa uns 800 MB no pico e tem só 128 MB.",
                "Aumente a memória da função Lambda relatorio-vendas para 1024 MB.",
                A("Console: Lambda → relatorio-vendas → Configuração → Configuração geral → Editar → Memória 1024 → Salvar", "aws lambda update-function-configuration --function-name relatorio-vendas --memory-size 1024"),
                A("A memória fica na Configuração geral da função.", "1024 MB. Na CLI: update-function-configuration --memory-size 1024."),
                A("Memória", "De 128 MB a 10240 MB; a CPU cresce junto com a memória.", "--memory-size", "Valor em MB.", "Custo", "Cobrança = memória × tempo: às vezes mais memória termina mais rápido e sai até mais barato."),
                "Meça o consumo real (o log mostra 'Max Memory Used') e ajuste; o AWS Lambda Power Tuning ajuda a achar o ponto ótimo."));
            list.Add(AwsL(0, "e16", "Tags imutáveis",
                "Alguém fez push de uma imagem quebrada com a tag 1.4.2, que já estava em produção, e ninguém conseguiu voltar atrás.",
                "No ECR, ative a imutabilidade de tags do repositório cafe365-web (sem desligar a verificação de vulnerabilidades).",
                A("Console: ECR → selecione cafe365-web → Ações → Editar → marque Imutabilidade de tags → Salvar", "aws ecr put-image-tag-mutability --repository-name cafe365-web --image-tag-mutability IMMUTABLE"),
                A("É uma configuração do repositório (Ações → Editar).", "Na CLI: put-image-tag-mutability --image-tag-mutability IMMUTABLE."),
                A("Tag imutável", "Uma tag já enviada sempre aponta para a mesma imagem.", "IMMUTABLE / MUTABLE", "Valores da opção na CLI."),
                "Com tags imutáveis, versão nova = tag nova (ex.: o hash do commit): fica fácil auditar e voltar atrás."));
            list.Add(AwsL(0, "e17", "IP elástico parado",
                "A Ana achou um IP elástico (34.192.7.21) sem associação a nenhuma instância — e todo IP público é cobrado por hora, mesmo parado.",
                "No EC2 (Norte da Virgínia), libere o IP elástico sem associação (eipalloc-0f4ee01). O IP do web-1 deve continuar.",
                A("Console: EC2 → IPs elásticos → selecione 34.192.7.21 → Ações → Liberar endereços IP elásticos → Sim", "aws ec2 release-address --allocation-id eipalloc-0f4ee01"),
                A("IPs elásticos ficam no menu lateral do EC2.", "Libere o que está 'Sem associação'. Na CLI: aws ec2 release-address --allocation-id ..."),
                A("IP elástico", "IPv4 público fixo da sua conta.", "Liberar", "Devolve o IP para a AWS (não dá para recuperar o mesmo depois).", "Cobrança", "Todo IPv4 público é cobrado por hora; parado, é desperdício puro."),
                "Revise todo mês IPs sem associação, volumes 'Disponível' e snapshots velhos: são os esquecidos clássicos da fatura."));
            list.Add(AwsL(0, "e18", "Imagem do servidor",
                "O web-1 está configurado do jeito certo depois de dias de ajustes. O Rodrigo quer uma cópia completa (sistema + aplicação) para criar servidores iguais.",
                "Crie uma imagem (AMI) da instância web-1 (i-0web001) com o nome web-1-base.",
                A("Console: EC2 → Instâncias → selecione web-1 → Ações → Imagem e modelos → Criar imagem → nome web-1-base → Criar imagem", "aws ec2 create-image --instance-id i-0web001 --name web-1-base"),
                A("Selecione a instância; criar imagem fica em Ações → Imagem e modelos.", "Na CLI: aws ec2 create-image --instance-id ... --name ..."),
                A("AMI", "Imagem com o disco da instância: novas instâncias nascem iguais.", "Sem reinicialização", "Por padrão a AWS reinicia a instância para o disco ficar consistente; --no-reboot evita (com risco)."),
                "Automatize imagens com o EC2 Image Builder e versione o nome (web-2026-10-15): fica fácil voltar para uma imagem boa."));
            list.Add(AwsL(0, "e19", "Mais um servidor",
                "Começou uma promoção na TV e o grupo do Auto Scaling asg-web está com 2 instâncias no limite. O Rodrigo quer 3 agora.",
                "No grupo do Auto Scaling asg-web, mude a capacidade desejada para 3, mantendo a mínima 1 e a máxima 4.",
                A("Console: EC2 → Grupos do Auto Scaling → asg-web → Editar → Capacidade desejada 3 → Atualizar", "aws autoscaling set-desired-capacity --auto-scaling-group-name asg-web --desired-capacity 3"),
                A("Abra o grupo e edite a capacidade.", "Só a desejada muda: 3. Na CLI: aws autoscaling set-desired-capacity ..."),
                A("Capacidade desejada", "Quantas instâncias o grupo mantém agora.", "Mínima / máxima", "Limites dentro dos quais a desejada (e as políticas) podem variar."),
                "Ajuste manual resolve na hora; o ideal é uma política de escalabilidade fazer isso sozinha."));
            list.Add(AwsL(0, "e20", "Rodízio de chave",
                "A auditoria exige que a chave do KMS usada nos backups tenha rotação automática anual.",
                "No KMS, ative a rotação automática da chave gerenciada pelo cliente alias/backups.",
                A("Console: Key Management Service → Chaves gerenciadas pelo cliente → alias/backups → Rotação de chaves → Editar → marque a rotação automática → Salvar", "aws kms enable-key-rotation --key-id 7c1e5a90-2b4d-4f8e-9a31-b4c4c0ffee01"),
                A("Abra a chave pelo alias e procure 'Rotação de chaves'.", "Na CLI essa operação NÃO aceita alias: use o ID da chave (aws kms list-aliases mostra)."),
                A("Rotação automática", "O KMS gera material criptográfico novo todo ano; ID, alias e permissões continuam.", "--key-id", "Aqui precisa ser o ID (ou o ARN) da chave, não o alias."),
                "Os dados antigos continuam legíveis depois da rotação: o KMS guarda as versões anteriores da chave."));
            list.Add(AwsL(0, "e21", "Orçamento do mês",
                "A Ana quer acompanhar se a conta está dentro do combinado com a diretoria: US$ 300 por mês.",
                "Em Billing e Budgets, crie um orçamento de custo mensal chamado custo-mensal com valor de US$ 300.",
                A("Console: Billing e Budgets → Orçamentos → Criar orçamento → Orçamento de custo → nome custo-mensal → valor 300 → Criar orçamento", "aws budgets create-budget --account-id 123456789012 --budget BudgetName=custo-mensal,BudgetLimit={Amount=300,Unit=USD},TimeUnit=MONTHLY,BudgetType=COST"),
                A("Procure por 'orçamento' ou 'billing' na pesquisa.", "Orçamento de custo, mensal, US$ 300. A CLI precisa do --account-id (sem traços)."),
                A("Orçamento (budget)", "Compara o custo real e o previsto com um valor definido.", "BudgetLimit={Amount=300,Unit=USD}", "Valor e moeda na sintaxe abreviada da CLI.", "TimeUnit=MONTHLY", "Período do orçamento."),
                "Crie o orçamento no primeiro dia da conta — e com alertas, senão ninguém olha."));
            list.Add(AwsL(0, "e22", "Bucket exposto",
                "A campanha da Black Friday acabou, mas o bucket cafe365-promo continua público na internet. O marketing vai reaproveitar o material, mas ninguém de fora deve acessar.",
                "Ative o bloqueio de TODO o acesso público do bucket cafe365-promo (sem apagar nada).",
                A("Console: S3 → cafe365-promo → Permissões → Bloquear acesso público → Editar → marque Bloquear todo o acesso público → Salvar alterações → confirmar", "aws s3api put-public-access-block --bucket cafe365-promo --public-access-block-configuration BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true"),
                A("Fica na aba Permissões do bucket.", "As quatro opções precisam ficar ativas (na CLI, todas =true)."),
                A("Bloquear acesso público", "Trava que ignora e impede ACLs e políticas que deixariam objetos públicos.", "RestrictPublicBuckets", "Anula na hora o efeito de uma política pública que já existe."),
                "Ative o bloqueio de acesso público na CONTA inteira e libere exceções conscientes (de preferência via CloudFront)."));
            list.Add(AwsL(0, "e23", "Porta RDP aberta",
                "O scanner de segurança achou o RDP (3389) do servidor Windows do financeiro aberto para a internet inteira. O financeiro só acessa do escritório.",
                "No grupo de segurança windows-sg (sg-0win444), remova a regra de entrada 3389 com origem 0.0.0.0/0, mantendo a do IP do escritório.",
                A("Console: EC2 → Grupos de segurança → windows-sg → selecione a regra RDP 3389 de 0.0.0.0/0 → Excluir regra", "aws ec2 revoke-security-group-ingress --group-id sg-0win444 --protocol tcp --port 3389 --cidr 0.0.0.0/0"),
                A("Abra o grupo e confira a coluna Origem das regras.", "Exclua só a de 0.0.0.0/0. Na CLI: revoke-security-group-ingress com os mesmos dados da regra."),
                A("RDP (3389)", "Acesso remoto ao Windows: alvo nº 1 de ataques de força bruta.", "revoke-security-group-ingress", "Remove a regra que bate exatamente com porta, protocolo e origem."),
                "Melhor que liberar RDP: acesso pelo Fleet Manager/Session Manager do Systems Manager, sem porta aberta."));
            list.Add(AwsL(0, "e24", "Permissão esquecida",
                "Durante um incidente no mês passado, a Ana recebeu AdministratorAccess 'só por hoje'. A permissão nunca foi removida.",
                "No IAM, remova a política AdministratorAccess anexada diretamente à usuária ana. Ela continua no grupo Desenvolvedores.",
                A("Console: IAM → Usuários → ana → selecione AdministratorAccess → Remover política", "aws iam detach-user-policy --user-name ana --policy-arn arn:aws:iam::aws:policy/AdministratorAccess"),
                A("Na página da usuária, veja 'Políticas anexadas diretamente'.", "Remova só a política. Na CLI: detach-user-policy com o ARN da política."),
                A("Anexada diretamente", "Política no próprio usuário, fora dos grupos — fácil de esquecer.", "detach-user-policy", "Desanexa a política (ela continua existindo para outros)."),
                "Acesso temporário deve expirar sozinho: use roles com sessão limitada ou o IAM Identity Center em vez de 'lembrar de tirar'."));

            // ───────────────────────── Intermediário ─────────────────────────
            list.Add(AwsL(1, "e25", "IP fixo para a intranet",
                "Toda vez que a intranet é ligada, o IP público muda e o firewall do escritório precisa ser atualizado. Existe um IP elástico livre (34.192.7.21) na conta.",
                "Associe o IP elástico eipalloc-0f4ee01 à instância intranet (i-0int555), sem mexer no IP do web-1.",
                A("Console: EC2 → IPs elásticos → selecione 34.192.7.21 → Ações → Associar endereço IP elástico → Instância intranet → Associar", "aws ec2 associate-address --instance-id i-0int555 --allocation-id eipalloc-0f4ee01"),
                A("IPs elásticos → Ações → Associar.", "Escolha a intranet e não use o IP que já está no web-1. Na CLI: associate-address --instance-id ... --allocation-id ..."),
                A("Associar IP elástico", "A instância passa a usar esse IP fixo, mesmo depois de parar e iniciar.", "--allow-reassociation", "Permite MOVER um IP que já está em outra instância (cuidado!)."),
                "Para serviços web, prefira um load balancer ou um nome DNS a depender de IP fixo."));
            list.Add(AwsL(1, "e26", "Assinando os alertas",
                "O tópico alertas-ti existe, mas ninguém recebe nada: ele não tem assinantes.",
                "No SNS, crie uma assinatura do tipo E-mail no tópico alertas-ti para o endereço ti@cafe365.com.br.",
                A("Console: Simple Notification Service → Tópicos → alertas-ti → Criar assinatura → Protocolo E-mail → Endpoint ti@cafe365.com.br → Criar assinatura", "aws sns subscribe --topic-arn arn:aws:sns:us-east-1:123456789012:alertas-ti --protocol email --notification-endpoint ti@cafe365.com.br"),
                A("Abra o tópico e use 'Criar assinatura'.", "Protocolo email; na CLI o tópico vai pelo ARN: arn:aws:sns:us-east-1:123456789012:alertas-ti."),
                A("Assinatura", "Destino de um tópico: e-mail, SMS, fila, função...", "Confirmação pendente", "O e-mail só recebe depois que o dono clica no link de confirmação.", "--notification-endpoint", "O endereço (e-mail, telefone, URL ou ARN)."),
                "Assine uma LISTA da equipe, não o e-mail de uma pessoa: férias e trocas de time não deixam o alerta no vácuo."));
            list.Add(AwsL(1, "e27", "Nível de log",
                "O time quer controlar o nível de log da função pedidos sem publicar código novo. Ela já usa a variável TABELA=pedidos.",
                "Adicione à função Lambda pedidos a variável de ambiente LOG_LEVEL com valor INFO, mantendo TABELA=pedidos.",
                A("Console: Lambda → pedidos → Configuração → Variáveis de ambiente → Editar → adicione LOG_LEVEL = INFO (mantenha TABELA) → Salvar", "aws lambda update-function-configuration --function-name pedidos --environment Variables={TABELA=pedidos,LOG_LEVEL=INFO}"),
                A("Variáveis de ambiente ficam na aba Configuração.", "Atualizar as variáveis SUBSTITUI o conjunto inteiro: na CLI, mande as duas em Variables={...}."),
                A("Variável de ambiente", "Configuração lida pelo código (os.environ / process.env) sem mudar o código.", "Variables={A=1,B=2}", "Sintaxe abreviada da CLI: o conjunto COMPLETO de variáveis."),
                "Configuração em variável de ambiente, segredo no Secrets Manager, código no Git: cada coisa no seu lugar."));
            list.Add(AwsL(1, "e28", "Foto do banco",
                "Hoje à noite o time roda uma migração que altera várias tabelas do banco de produção. Se der errado, precisa haver como voltar.",
                "No RDS, tire um snapshot manual do banco banco-cafe com o nome antes-da-migracao.",
                A("Console: RDS → Bancos de dados → selecione banco-cafe → Ações → Tirar snapshot → nome antes-da-migracao → Tirar snapshot", "aws rds create-db-snapshot --db-instance-identifier banco-cafe --db-snapshot-identifier antes-da-migracao"),
                A("Selecione o banco de produção e procure em Ações.", "Na CLI: aws rds create-db-snapshot --db-instance-identifier ... --db-snapshot-identifier ..."),
                A("Snapshot manual", "Cópia do banco que fica até você excluir (os automáticos somem com a retenção).", "Restaurar", "Um snapshot vira um banco NOVO; depois a aplicação passa a usar ele."),
                "Antes de qualquer mudança arriscada em produção: snapshot com um nome que diga o porquê."));
            list.Add(AwsL(1, "e29", "Endereço da API",
                "O app do Café 365 vai chamar a API por nome, não por IP. A API responde no IP elástico do web-1, 54.210.12.8.",
                "No Route 53, na zona cafe365.com.br, crie o registro A api.cafe365.com.br apontando para 54.210.12.8, com TTL de 300 segundos.",
                A("Console: Route 53 → Zonas hospedadas → cafe365.com.br → Criar registro → nome api → tipo A → valor 54.210.12.8 → TTL 300 → Criar registros", "aws route53 change-resource-record-sets --hosted-zone-id Z0CAFE365BR --change-batch '{\"Changes\":[{\"Action\":\"CREATE\",\"ResourceRecordSet\":{\"Name\":\"api.cafe365.com.br\",\"Type\":\"A\",\"TTL\":300,\"ResourceRecords\":[{\"Value\":\"54.210.12.8\"}]}}]}'"),
                A("Abra a zona hospedada e use 'Criar registro'.", "Tipo A = nome → IPv4. Na CLI, o change-batch é um JSON com Action, Name, Type, TTL e ResourceRecords."),
                A("Registro A", "Liga um nome a um endereço IPv4.", "TTL", "Por quanto tempo os resolvedores guardam a resposta em cache.", "change-batch", "Lista de mudanças (CREATE, UPSERT, DELETE) aplicadas juntas."),
                "Com load balancer ou CloudFront, use registro ALIAS em vez de A com IP fixo: o Route 53 acompanha as mudanças de IP."));
            list.Add(AwsL(1, "e30", "Faxina automática de logs",
                "O bucket cafe365-logs guarda acessos desde 2023. A política da empresa manda manter logs por 90 dias.",
                "No bucket cafe365-logs, crie a regra de ciclo de vida expirar-logs, valendo para o bucket inteiro, que expira (exclui) as versões atuais dos objetos 90 dias após a criação.",
                A("Console: S3 → cafe365-logs → Gerenciamento → Criar regra de ciclo de vida → nome expirar-logs → Aplicar a todos os objetos do bucket → Expirar versões atuais: 90 dias → Criar regra", "aws s3api put-bucket-lifecycle-configuration --bucket cafe365-logs --lifecycle-configuration '{\"Rules\":[{\"ID\":\"expirar-logs\",\"Filter\":{\"Prefix\":\"\"},\"Status\":\"Enabled\",\"Expiration\":{\"Days\":90}}]}'"),
                A("Regras de ciclo de vida ficam na aba Gerenciamento do bucket.", "Escopo: bucket inteiro; ação: expirar versões atuais em 90 dias. Na CLI, o JSON tem ID, Filter, Status e Expiration.Days."),
                A("Ciclo de vida", "Regras automáticas que movem ou apagam objetos pela idade.", "Expiration.Days", "Apaga objetos com mais de N dias.", "put-bucket-lifecycle-configuration", "Substitui TODAS as regras do bucket."),
                "Combine transição (Glacier) e expiração: logs recentes à mão, antigos baratos, velhos demais apagados."));
            list.Add(AwsL(1, "e31", "Disco mais barato",
                "A Ana leu que volumes gp3 custam cerca de 20% menos que gp2, com desempenho igual ou melhor — e dá para converter sem parar as instâncias.",
                "No EC2 (Norte da Virgínia), converta para gp3 os dois volumes gp2 que estão Em uso (vol-0b72c44 e vol-0d9e202), sem mudar o tamanho.",
                A("Console: EC2 → Volumes → selecione o volume → Ações → Modificar volume → Tipo gp3 → Modificar (repita no outro)", "aws ec2 modify-volume --volume-id vol-0b72c44 --volume-type gp3  +  aws ec2 modify-volume --volume-id vol-0d9e202 --volume-type gp3"),
                A("Volumes → Ações → Modificar volume.", "Só o tipo muda. Na CLI: aws ec2 modify-volume --volume-id ... --volume-type gp3."),
                A("Elastic Volumes", "Muda tipo e tamanho com o volume em uso, sem parar a instância.", "gp3", "SSD de uso geral com 3000 IOPS de base, independentemente do tamanho, e mais barato que o gp2.", "Tamanho", "Só aumenta; nunca diminui."),
                "Revise os tipos de volume de tempos em tempos: gp2 → gp3 é uma das economias mais fáceis da AWS."));
            list.Add(AwsL(1, "e32", "Pagamentos em ordem",
                "O sistema de pagamentos precisa processar as mensagens na ordem e nunca duas vezes. O app não envia ID de deduplicação.",
                "No SQS, crie a fila FIFO pagamentos.fifo com a deduplicação baseada em conteúdo ativada.",
                A("Console: Simple Queue Service → Criar fila → Tipo FIFO → nome pagamentos.fifo → marque Deduplicação baseada em conteúdo → Criar fila", "aws sqs create-queue --queue-name pagamentos.fifo --attributes FifoQueue=true,ContentBasedDeduplication=true"),
                A("Ordem garantida = fila FIFO; o nome termina com .fifo.", "Na CLI: --attributes FifoQueue=true,ContentBasedDeduplication=true."),
                A("FIFO", "Ordem garantida e processamento exatamente uma vez.", "Deduplicação por conteúdo", "Mensagens iguais em 5 minutos contam como uma só (hash do corpo).", ".fifo", "Sufixo obrigatório no nome."),
                "FIFO tem vazão menor que a Padrão: use só quando ordem ou duplicidade realmente importam."));
            list.Add(AwsL(1, "e33", "Tabela de carrinhos",
                "O carrinho de compras sai do servidor e vai para o DynamoDB: um carrinho por cliente, achado pelo ID do cliente (ex.: c-102). O tráfego é imprevisível.",
                "No DynamoDB, crie a tabela carrinhos com chave de partição cliente_id do tipo String, sem chave de classificação, no modo de capacidade sob demanda.",
                A("Console: DynamoDB → Criar tabela → nome carrinhos → chave de partição cliente_id (String) → sem chave de classificação → Sob demanda → Criar tabela", "aws dynamodb create-table --table-name carrinhos --attribute-definitions AttributeName=cliente_id,AttributeType=S --key-schema AttributeName=cliente_id,KeyType=HASH --billing-mode PAY_PER_REQUEST"),
                A("Chave de partição = o 'endereço' do item.", "Tipo S (String), KeyType HASH e --billing-mode PAY_PER_REQUEST (sem isso, a CLI cria provisionada)."),
                A("Chave de partição (HASH)", "Define onde o item fica e como ele é achado.", "AttributeType=S", "String (N = número, B = binário).", "PAY_PER_REQUEST", "Sob demanda: paga por requisição, sem planejar capacidade."),
                "Modele o DynamoDB pelos acessos: 'buscar o carrinho do cliente X' → chave = cliente_id."));
            list.Add(AwsL(1, "e34", "Runtime aposentado",
                "A AWS avisou: o Python 3.8 foi descontinuado e não recebe mais correções de segurança. A função relatorio-vendas ainda usa ele; o time já testou o código no Python 3.12.",
                "Mude o tempo de execução (runtime) da função Lambda relatorio-vendas para Python 3.12.",
                A("Console: Lambda → relatorio-vendas → Código → Configurações de tempo de execução → Editar → Python 3.12 → Salvar", "aws lambda update-function-configuration --function-name relatorio-vendas --runtime python3.12"),
                A("Fica em 'Configurações de tempo de execução', na aba Código da função.", "Na CLI, o identificador é python3.12."),
                A("Runtime", "Linguagem e versão em que a função roda.", "Descontinuado", "Sem patches de segurança; depois a AWS bloqueia criar e atualizar funções nele."),
                "Mantenha um inventário de runtimes (o AWS Health avisa) e atualize com testes, bem antes do prazo."));
            list.Add(AwsL(1, "e35", "Link temporário",
                "O contador externo precisa do relatório vendas-setembro.pdf. Ele não tem conta na AWS, e o bucket cafe365-relatorios é (e deve continuar) privado.",
                "Gere uma URL pré-assinada do objeto vendas-setembro.pdf do bucket cafe365-relatorios, válida por 1 hora.",
                A("Console: S3 → cafe365-relatorios → selecione vendas-setembro.pdf → Ações → Compartilhar com URL pré-assinada → 1 hora (ou 60 minutos) → Criar URL pré-assinada", "aws s3 presign s3://cafe365-relatorios/vendas-setembro.pdf --expires-in 3600"),
                A("Selecione o objeto e procure em Ações.", "1 hora = 3600 segundos. Na CLI: aws s3 presign s3://bucket/objeto --expires-in ..."),
                A("URL pré-assinada", "Link com assinatura temporária: quem tem o link baixa o objeto até ele expirar.", "--expires-in", "Validade em segundos (máximo de 7 dias na CLI)."),
                "Compartilhe com URL pré-assinada curta em vez de abrir o bucket — e lembre: quem tiver o link, baixa."));
            list.Add(AwsL(1, "e36", "Firewall da nova API",
                "A nova API do Café 365 vai rodar em servidores próprios no Norte da Virgínia. Ela atende só HTTPS e é usada pelo app dos clientes, de qualquer lugar.",
                "Crie o grupo de segurança api-sg (VPC cafe365-vpc, Norte da Virgínia) com UMA regra de entrada: HTTPS (443) de qualquer lugar (0.0.0.0/0).",
                A("Console: EC2 → Grupos de segurança → Criar grupo de segurança → nome api-sg, descrição, VPC cafe365-vpc → regra HTTPS de 0.0.0.0/0 → Criar grupo de segurança", "aws ec2 create-security-group --group-name api-sg --description \"API do Cafe 365\" --vpc-id vpc-0cafe365  +  aws ec2 authorize-security-group-ingress --group-id sg-08300e47 --protocol tcp --port 443 --cidr 0.0.0.0/0"),
                A("Grupos de segurança → Criar grupo de segurança (a regra pode ir no mesmo formulário).", "HTTPS = TCP 443, origem 0.0.0.0/0. Na CLI: create-security-group (anote o GroupId da saída) e depois authorize-security-group-ingress --group-id com esse ID."),
                A("create-security-group", "Cria o grupo (descrição obrigatória) numa VPC e devolve o GroupId (sg-...), usado nos comandos seguintes.", "Regras de saída", "Por padrão todo o tráfego de saída é liberado.", "0.0.0.0/0 na 443", "Normal para um serviço público; nunca para SSH, RDP ou bancos."),
                "Um grupo por função (web, api, banco) e regras entre grupos: fica claro quem fala com quem."));
            list.Add(AwsL(1, "e37", "Sempre HTTPS",
                "O site pela CDN ainda aceita http:// sem criptografia. Muitos clientes têm links antigos com http, que não podem quebrar.",
                "Na distribuição E1CAFE365SITE da CloudFront, altere a política de protocolo do visualizador do comportamento padrão para Redirecionar HTTP para HTTPS.",
                A("Console: CloudFront → E1CAFE365SITE → Comportamentos → selecione Padrão (*) → Editar → Redirecionar HTTP para HTTPS → Salvar alterações"),
                A("É uma configuração do COMPORTAMENTO (cache behavior) da distribuição.", "Redirecionar (301) mantém os links antigos funcionando; 'Somente HTTPS' devolve erro. Pela CLI, update-distribution exige a configuração completa: faça pelo console."),
                A("Política de protocolo do visualizador", "Como a CDN trata requisições http://.", "Redirecionar HTTP para HTTPS", "Responde 301 apontando para a versão https.", "Somente HTTPS", "Recusa http com erro 403."),
                "Complete com HSTS (cabeçalho de resposta) para o navegador nem tentar http da próxima vez."));
            list.Add(AwsL(1, "e38", "Senhas mais fortes",
                "A auditoria reprovou a política de senha da conta: hoje vale qualquer senha com 8 caracteres.",
                "No IAM, defina uma política de senha personalizada: mínimo de 14 caracteres, exigindo maiúscula, minúscula, número e símbolo, SEM expiração periódica.",
                A("Console: IAM → Configurações da conta → Política de senha → Editar → Personalizada → 14 → marque maiúscula, minúscula, número e caractere não alfanumérico → deixe a expiração desmarcada → Salvar alterações", "aws iam update-account-password-policy --minimum-password-length 14 --require-uppercase-characters --require-lowercase-characters --require-numbers --require-symbols"),
                A("Fica em IAM → Configurações da conta.", "Na CLI, toda exigência que você NÃO informar volta a ficar desligada: passe as quatro opções --require-..."),
                A("Política de senha", "Regras para as senhas de console dos usuários IAM.", "Expiração periódica", "Trocas forçadas sem motivo levam a senhas previsíveis (Cafe2026!, Cafe2027!...).", "update-account-password-policy", "Substitui a política inteira."),
                "Senha forte + MFA obrigatório; melhor ainda, login federado (IAM Identity Center) e menos usuários IAM."));
            list.Add(AwsL(1, "e39", "Menos chamadas vazias",
                "O consumidor da fila pedidos pergunta 'tem mensagem?' sem parar e quase sempre ouve 'não' — e cada pergunta é cobrada.",
                "Ative a sondagem longa (long polling) na fila pedidos: tempo de espera de recebimento de 20 segundos.",
                A("Console: Simple Queue Service → pedidos → Editar → Tempo de espera de recebimento 20 → Salvar", "aws sqs set-queue-attributes --queue-url https://sqs.us-east-1.amazonaws.com/123456789012/pedidos --attributes ReceiveMessageWaitTimeSeconds=20"),
                A("É um atributo da fila: Editar.", "ReceiveMessageWaitTimeSeconds=20 (o máximo). Na CLI a fila é identificada pela URL (aws sqs get-queue-url mostra)."),
                A("Sondagem longa", "O recebimento espera até chegar mensagem (até 20 s), em vez de responder vazio na hora.", "--queue-url", "Comandos do SQS usam a URL da fila, não o nome."),
                "Long polling reduz custo E latência: quase sempre vale ligar."));
            list.Add(AwsL(1, "e40", "Backups por uma semana",
                "Hoje o banco-cafe guarda backups automáticos por só 1 dia. Um erro descoberto na segunda-feira não teria volta.",
                "Altere o período de retenção de backup do banco-cafe para 7 dias, aplicando imediatamente.",
                A("Console: RDS → selecione banco-cafe → Modificar → Backup: período de retenção 7 dias → Aplicar imediatamente → Modificar instância de banco de dados", "aws rds modify-db-instance --db-instance-identifier banco-cafe --backup-retention-period 7 --apply-immediately"),
                A("Modificar → seção Backup.", "Escolha 'Aplicar imediatamente' (sem isso, fica para a janela de manutenção). Na CLI: --backup-retention-period 7 --apply-immediately."),
                A("Retenção de backup", "Quantos dias de backups automáticos (e de restauração para um ponto no tempo) o RDS guarda.", "--apply-immediately", "Aplica agora, sem esperar a janela de manutenção.", "0 dias", "Desativa os backups automáticos!"),
                "Teste a restauração de vez em quando: backup que nunca foi restaurado é só uma esperança."));
            list.Add(AwsL(1, "e41", "Sessões que expiram",
                "A tabela sessoes não para de crescer: ninguém apaga as sessões vencidas. Cada item tem o atributo expira_em com o horário de expiração em segundos (epoch).",
                "Ative o TTL (tempo de vida) na tabela sessoes usando o atributo expira_em.",
                A("Console: DynamoDB → Tabelas → sessoes → Tempo de vida (TTL) → Ativar TTL → atributo expira_em → Ativar TTL", "aws dynamodb update-time-to-live --table-name sessoes --time-to-live-specification Enabled=true,AttributeName=expira_em"),
                A("O TTL é configurado na página da tabela.", "Atributo expira_em. Na CLI: update-time-to-live --time-to-live-specification Enabled=true,AttributeName=..."),
                A("TTL", "O DynamoDB apaga sozinho os itens cujo horário já passou (sem custo de gravação).", "Epoch", "Segundos desde 01/01/1970 (UTC), tipo Número.", "Atraso", "A exclusão pode levar alguns dias: filtre itens vencidos na leitura."),
                "TTL é a faxina grátis do DynamoDB: sessões, tokens, carrinhos abandonados."));
            list.Add(AwsL(1, "e42", "Alarme que avisa",
                "Na terça, o web-1 parou de responder às verificações de status e ninguém soube por duas horas. O tópico alertas-ti já entrega e-mail para a equipe.",
                "No CloudWatch, crie o alarme status-web: métrica StatusCheckFailed (AWS/EC2) do web-1, maior que 0, notificando o tópico do SNS alertas-ti.",
                A("Console: CloudWatch → Alarmes → Criar alarme → namespace AWS/EC2 → StatusCheckFailed → web-1 → Maior que 0 → notificação alertas-ti → nome status-web → Criar alarme", "aws cloudwatch put-metric-alarm --alarm-name status-web --namespace AWS/EC2 --metric-name StatusCheckFailed --dimensions Name=InstanceId,Value=i-0web001 --statistic Maximum --period 60 --evaluation-periods 2 --threshold 0 --comparison-operator GreaterThanThreshold --alarm-actions arn:aws:sns:us-east-1:123456789012:alertas-ti"),
                A("Alarme = métrica + condição + ação.", "StatusCheckFailed vale 0 ou 1: maior que 0. A ação é o ARN do tópico (--alarm-actions)."),
                A("StatusCheckFailed", "Vale 1 quando a instância ou o hardware falham nas verificações.", "--alarm-actions", "O que fazer ao entrar EM ALARME (aqui, publicar no tópico).", "--evaluation-periods 2", "Precisa falhar 2 períodos seguidos (evita alarme por um soluço)."),
                "Para falhas do sistema (StatusCheckFailed_System), dá até para a ação do alarme RECUPERAR a instância sozinha."));
            list.Add(AwsL(1, "e43", "Limites do Auto Scaling",
                "Para a Black Friday, o asg-web nunca pode ficar com menos de 2 instâncias (uma em cada zona) e pode crescer até 6.",
                "No grupo do Auto Scaling asg-web, defina a capacidade mínima 2 e a máxima 6.",
                A("Console: EC2 → Grupos do Auto Scaling → asg-web → Editar → mínima 2, máxima 6 (desejada entre elas) → Atualizar", "aws autoscaling update-auto-scaling-group --auto-scaling-group-name asg-web --min-size 2 --max-size 6"),
                A("Edite a capacidade do grupo.", "A desejada precisa ficar entre a mínima e a máxima. Na CLI: update-auto-scaling-group --min-size 2 --max-size 6."),
                A("Mínima", "Piso: garante disponibilidade (ex.: uma instância por zona).", "Máxima", "Teto: limita o custo se algo escalar sem controle.", "update-auto-scaling-group", "Altera os limites; a desejada se ajusta se ficar fora deles."),
                "Mínimo de 2 instâncias em zonas diferentes é o básico de alta disponibilidade."));
            list.Add(AwsL(1, "e44", "Página inicial da CDN",
                "Quem abre https://d111abcdef8.cloudfront.net/ recebe um XML de AccessDenied: a CloudFront não sabe que a página inicial é o index.html.",
                "Na distribuição E1CAFE365SITE, defina o objeto raiz padrão como index.html.",
                A("Console: CloudFront → E1CAFE365SITE → Geral: Configurações → Editar → Objeto raiz padrão index.html → Salvar alterações"),
                A("É uma configuração geral da distribuição.", "Só o nome, sem barra: index.html. Pela CLI, update-distribution exige a configuração completa: faça pelo console."),
                A("Objeto raiz padrão", "O que entregar quando alguém pede a raiz (/).", "Subpastas", "Não vale para /blog/: para isso use uma CloudFront Function."),
                "Depois de alterar a distribuição, espere a implantação terminar antes de testar."));
            list.Add(AwsL(1, "e45", "Alerta de orçamento",
                "O orçamento custo-mensal (US$ 300) existe, mas ninguém é avisado quando ele está acabando.",
                "No orçamento custo-mensal, adicione um alerta para quando o custo REAL passar de 80% do valor, enviando e-mail para financeiro@cafe365.com.br.",
                A("Console: Billing e Budgets → Orçamentos → custo-mensal → Adicionar limite de alerta → 80 → Real → financeiro@cafe365.com.br → Salvar", "aws budgets create-notification --account-id 123456789012 --budget-name custo-mensal --notification NotificationType=ACTUAL,ComparisonOperator=GREATER_THAN,Threshold=80 --subscribers SubscriptionType=EMAIL,Address=financeiro@cafe365.com.br"),
                A("Abra o orçamento e adicione um limite de alerta.", "Tipo ACTUAL (real), limite 80 (%). Na CLI: create-notification com --notification e --subscribers."),
                A("Real (ACTUAL)", "Dispara com o que já foi gasto.", "Previsto (FORECASTED)", "Dispara quando a PROJEÇÃO do mês passa do limite (avisa antes).", "Threshold=80", "Porcentagem do valor orçado."),
                "Use os dois: previsto (100%) para agir cedo e real (80%) como confirmação."));
            list.Add(AwsL(1, "e46", "Fila dispara a função",
                "Os pedidos chegam na fila pedidos, mas ninguém os processa. A função processar-pedidos está pronta para ler da fila.",
                "Adicione à função Lambda processar-pedidos um gatilho do SQS ligado à fila pedidos, com tamanho de lote 10.",
                A("Console: Lambda → processar-pedidos → Adicionar gatilho → SQS → fila pedidos → Tamanho do lote 10 → Adicionar", "aws lambda create-event-source-mapping --function-name processar-pedidos --event-source-arn arn:aws:sqs:us-east-1:123456789012:pedidos --batch-size 10"),
                A("Na visão geral da função há 'Adicionar gatilho'.", "Na CLI é um mapeamento de origem de eventos: create-event-source-mapping com o ARN da fila."),
                A("Gatilho SQS", "A Lambda busca as mensagens da fila e chama a função com um lote.", "--batch-size", "Quantas mensagens por execução.", "Permissões", "A função de execução precisa de sqs:ReceiveMessage e sqs:DeleteMessage."),
                "Se a função falhar, o lote volta para a fila: informe as falhas por mensagem (ReportBatchItemFailures) para não reprocessar tudo."));
            list.Add(AwsL(1, "e47", "Função para a Lambda",
                "A função relatorio-vendas precisa ler arquivos do S3 e gravar logs. Para isso ela precisa de uma função (role) do IAM própria.",
                "Crie a função (role) lambda-relatorios, que confia no serviço Lambda, com as políticas AmazonS3ReadOnlyAccess e AWSLambdaBasicExecutionRole — e nada além.",
                A("Console: IAM → Funções → Criar função → Serviço da AWS: Lambda → marque AmazonS3ReadOnlyAccess e AWSLambdaBasicExecutionRole → nome lambda-relatorios → Criar função", "aws iam create-role --role-name lambda-relatorios --assume-role-policy-document file://confianca-lambda.json  +  aws iam attach-role-policy --role-name lambda-relatorios --policy-arn arn:aws:iam::aws:policy/AmazonS3ReadOnlyAccess  +  aws iam attach-role-policy --role-name lambda-relatorios --policy-arn arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"),
                A("Funções (roles) ficam no IAM; a 'entidade confiável' diz quem pode assumir a função.", "Caso de uso Lambda (na CLI, o arquivo confianca-lambda.json — veja com cat). O ARN da política básica tem o caminho service-role/."),
                A("Política de confiança", "Quem pode assumir a função (aqui, lambda.amazonaws.com).", "Política de permissões", "O que a função pode fazer.", "AWSLambdaBasicExecutionRole", "Permite criar e gravar os logs no CloudWatch."),
                "Uma role por função Lambda, com o mínimo necessário: se uma for comprometida, as outras não são."));
            list.Add(AwsL(1, "e48", "Clonando o servidor web",
                "O tráfego cresceu e o Rodrigo quer um segundo servidor igualzinho ao web-1. A imagem pronta é a AMI web-golden-2026-10.",
                "No EC2 (Norte da Virgínia), execute a instância web-2 a partir da AMI web-golden-2026-10, tipo t3.small, par de chaves cafe-key e grupo de segurança web-sg.",
                A("Console: EC2 → Executar instâncias → nome web-2 → AMI web-golden-2026-10 (Minhas AMIs) → t3.small → par de chaves cafe-key → grupo de segurança web-sg → Executar instância", "aws ec2 run-instances --image-id ami-0901d365 --instance-type t3.small --key-name cafe-key --security-group-ids sg-0web111 --tag-specifications 'ResourceType=instance,Tags=[{Key=Name,Value=web-2}]'"),
                A("No assistente de execução, as AMIs da conta aparecem junto com as da AWS.", "Na CLI o nome vai numa tag Name (--tag-specifications); o ID da AMI aparece em aws ec2 describe-images --owners self."),
                A("run-instances", "Cria instâncias; o nome é só a tag Name.", "--security-group-ids", "Firewall da instância (o default não libera HTTP).", "AMI dourada", "Imagem com tudo instalado: servidores novos ficam prontos em minutos."),
                "O próximo passo é um modelo de execução + Auto Scaling: servidores novos sobem sozinhos com essa mesma AMI."));

            // ───────────────────────── Profissional ─────────────────────────
            list.Add(AwsL(2, "e49", "Banco escondido",
                "O scanner de segurança achou o banco-cafe acessível publicamente na internet. É um incidente: a Helena quer fechar agora.",
                "Torne o banco banco-cafe NÃO acessível publicamente, aplicando a mudança imediatamente. Não altere mais nada.",
                A("Console: RDS → selecione banco-cafe → Modificar → Conectividade: Não acessível publicamente → Aplicar imediatamente → Modificar instância de banco de dados", "aws rds modify-db-instance --db-instance-identifier banco-cafe --no-publicly-accessible --apply-immediately"),
                A("Acesso público é uma configuração de Conectividade, em Modificar.", "Sem 'aplicar imediatamente', a mudança espera a janela de manutenção. Na CLI: --no-publicly-accessible --apply-immediately."),
                A("Acesso público", "Com 'Sim', o banco ganha endereço público na internet.", "--no-publicly-accessible", "Forma 'negativa' da opção booleana na CLI.", "Janela de manutenção", "Horário semanal em que as mudanças pendentes são aplicadas."),
                "Defesa em camadas: sub-rede privada + sem acesso público + grupo de segurança que só aceita a aplicação."));
            list.Add(AwsL(2, "e50", "Metadados blindados",
                "Um teste de invasão mostrou que, com uma falha de SSRF na aplicação, dá para ler as credenciais da instância pelo IMDSv1. Os servidores web precisam exigir o IMDSv2.",
                "Exija o IMDSv2 (token obrigatório, com o serviço de metadados habilitado) nos dois servidores web: web-1 (Norte da Virgínia) e web-br-1 (São Paulo).",
                A("Console: EC2 → selecione web-1 → Ações → Configurações da instância → Modificar opções de metadados → Habilitar + Somente V2 (token obrigatório) → Salvar; troque a região para São Paulo e repita no web-br-1", "aws ec2 modify-instance-metadata-options --instance-id i-0web001 --http-tokens required --http-endpoint enabled  +  aws ec2 modify-instance-metadata-options --instance-id i-0br0042 --http-tokens required --http-endpoint enabled --region sa-east-1"),
                A("As opções de metadados ficam em Ações → Configurações da instância. Uma das instâncias está em outra região.", "Token obrigatório = --http-tokens required; NÃO desabilite o endpoint (a aplicação usa as credenciais da role por ele)."),
                A("IMDS", "Serviço em 169.254.169.254 que entrega dados e credenciais temporárias à instância.", "IMDSv2", "Exige um token obtido com PUT: um SSRF comum (que só faz GET) não consegue.", "--http-tokens required", "Torna o v2 obrigatório."),
                "Defina o IMDSv2 como padrão da conta e das AMIs, para que toda instância nova já nasça protegida."));
            list.Add(AwsL(2, "e51", "Só por HTTPS",
                "A auditoria exige que os backups nunca trafeguem sem criptografia. Hoje o bucket aceita requisições HTTP simples. Na pasta do CloudShell há políticas prontas (ls para listar; cat para ler).",
                "Aplique ao bucket micorosoft-backups uma política que NEGUE qualquer requisição feita sem criptografia em trânsito (sem HTTPS).",
                A("Console: S3 → micorosoft-backups → Permissões → Política do bucket → Editar → Inserir exemplo: politica-somente-https.json → Salvar alterações", "aws s3api put-bucket-policy --bucket micorosoft-backups --policy file://politica-somente-https.json"),
                A("Leia as políticas disponíveis (cat) e procure a que tem Effect Deny.", "A condição certa é aws:SecureTransport = false. Na CLI: put-bucket-policy --policy file://arquivo.json."),
                A("Deny explícito", "Vence qualquer Allow: nem um administrador acessa sem HTTPS.", "aws:SecureTransport", "Chave de condição: true quando a requisição usa TLS.", "file://", "A CLI lê o valor do parâmetro de um arquivo."),
                "Some à política o bloqueio de acesso público, o versionamento e o Object Lock: backup protegido em várias camadas."));
            list.Add(AwsL(2, "e52", "Fila de mensagens mortas",
                "Um pedido com JSON inválido faz o processador falhar, volta para a fila e falha de novo — há dois dias. A fila pedidos-dlq foi criada para isso.",
                "Configure a fila pedidos para mandar à DLQ pedidos-dlq as mensagens que falharem 5 vezes (máximo de recebimentos 5).",
                A("Console: Simple Queue Service → pedidos → Editar → Fila de mensagens mortas: Ativado → pedidos-dlq → Máximo de recebimentos 5 → Salvar", "aws sqs set-queue-attributes --queue-url https://sqs.us-east-1.amazonaws.com/123456789012/pedidos --attributes file://redrive-pedidos.json"),
                A("A redrive é configurada na fila de ORIGEM, não na DLQ.", "Na CLI, o atributo RedrivePolicy é um JSON dentro de JSON: há um arquivo pronto (cat redrive-pedidos.json)."),
                A("DLQ", "Fila que recebe as mensagens 'envenenadas' para análise.", "maxReceiveCount", "Quantas vezes a mensagem pode ser recebida sem ser apagada antes de ir para a DLQ.", "RedrivePolicy", "Atributo com deadLetterTargetArn e maxReceiveCount."),
                "DLQ sem alarme é cemitério silencioso: monitore a quantidade de mensagens nela."));
            list.Add(AwsL(2, "e53", "Senha fora do código",
                "A senha do banco está em texto puro na variável DB_SENHA da função importar-estoque: qualquer um com leitura na Lambda vê. O segredo prod/banco-cafe já está no Secrets Manager, e o código novo lê o nome dele da variável SEGREDO_ID.",
                "Na função importar-estoque, remova a variável DB_SENHA e adicione SEGREDO_ID com o valor prod/banco-cafe, mantendo DB_HOST e DB_USUARIO.",
                A("Console: Lambda → importar-estoque → Configuração → Variáveis de ambiente → Editar → apague a chave DB_SENHA → adicione SEGREDO_ID = prod/banco-cafe → Salvar", "aws lambda update-function-configuration --function-name importar-estoque --environment Variables={DB_HOST=banco-cafe.interno,DB_USUARIO=estoque,SEGREDO_ID=prod/banco-cafe}"),
                A("Salvar as variáveis SUBSTITUI o conjunto inteiro.", "Na CLI, mande DB_HOST, DB_USUARIO e SEGREDO_ID em Variables={...} — sem DB_SENHA."),
                A("Secrets Manager", "Guarda o segredo criptografado, com controle de acesso, auditoria e rotação.", "SEGREDO_ID", "A função recebe só o NOME do segredo e busca o valor durante a execução."),
                "A senha ficou exposta: troque-a (rotacione) também — remover a variável não 'desvaza' nada."));
            list.Add(AwsL(2, "e54", "Rodízio sem downtime",
                "A chave de acesso do robô ci-github tem 410 dias. A política de segurança manda trocar a cada 90 — sem parar os deploys.",
                "Crie uma chave de acesso nova para ci-github e DESATIVE a antiga (AKIA5CIGITHUB2024OLD), sem excluí-la e sem deixar o usuário sem chave ativa em nenhum momento.",
                A("Console: IAM → Usuários → ci-github → Chaves de acesso → Criar chave de acesso (atualize o segredo no GitHub) → selecione a chave antiga → Ações → Desativar", "aws iam create-access-key --user-name ci-github  +  aws iam update-access-key --user-name ci-github --access-key-id AKIA5CIGITHUB2024OLD --status Inactive"),
                A("Cada usuário pode ter até 2 chaves: é isso que permite trocar sem parar.", "Ordem: criar a nova → atualizar o segredo do pipeline → desativar a antiga → (dias depois) excluir."),
                A("Duas chaves", "Durante a troca, a velha e a nova funcionam ao mesmo tempo.", "Desativar antes de excluir", "Se algum job esquecido quebrar, reative e corrija.", "create-access-key", "A chave secreta só aparece nessa hora."),
                "Melhor que rodar chave: OIDC do GitHub com uma role (credenciais temporárias, nada fixo para vazar)."));
            list.Add(AwsL(2, "e55", "Registro órfão",
                "Um pesquisador de segurança avisou: um subdomínio do Café 365 aponta para um bucket do S3 que não existe mais. Qualquer pessoa pode criar um bucket com esse nome e publicar conteúdo falso no domínio da empresa (subdomain takeover).",
                "Na zona cafe365.com.br, encontre o registro que aponta para um bucket inexistente e exclua só ele.",
                A("Console: Route 53 → cafe365.com.br → selecione promo.cafe365.com.br (CNAME para cafe365-blackfriday.s3-website...) → Excluir registro (confira no S3 que o bucket cafe365-blackfriday não existe)", "aws route53 change-resource-record-sets --hosted-zone-id Z0CAFE365BR --change-batch '{\"Changes\":[{\"Action\":\"DELETE\",\"ResourceRecordSet\":{\"Name\":\"promo.cafe365.com.br\",\"Type\":\"CNAME\",\"TTL\":300,\"ResourceRecords\":[{\"Value\":\"cafe365-blackfriday.s3-website-us-east-1.amazonaws.com\"}]}}]}'"),
                A("Compare os valores dos registros CNAME com a lista de buckets do S3.", "Na CLI, um DELETE precisa repetir EXATAMENTE o registro atual (nome, tipo, TTL e valor)."),
                A("Subdomain takeover", "Um DNS 'pendurado' num recurso apagado deixa outra pessoa assumir o subdomínio.", "DELETE", "Ação do change-batch; os dados precisam bater com o registro atual."),
                "Ao apagar um recurso (bucket, distribuição, IP), apague junto o DNS que aponta para ele — de preferência no mesmo código de infraestrutura."));
            list.Add(AwsL(2, "e56", "Tabela à prova de desastre",
                "A historico-pedidos guarda anos de pedidos e é usada pela contabilidade. Hoje, um DELETE errado ou um 'excluir tabela' por engano seria irreversível.",
                "Na tabela historico-pedidos, ative a recuperação para um ponto no tempo (PITR) e a proteção contra exclusão.",
                A("Console: DynamoDB → historico-pedidos → Backups: Editar → Ativar a recuperação para um ponto no tempo → Salvar alterações; Proteção contra exclusão → Ativar", "aws dynamodb update-continuous-backups --table-name historico-pedidos --point-in-time-recovery-specification PointInTimeRecoveryEnabled=true  +  aws dynamodb update-table --table-name historico-pedidos --deletion-protection-enabled"),
                A("São duas configurações diferentes, na página da tabela.", "Na CLI: update-continuous-backups (PITR) e update-table --deletion-protection-enabled."),
                A("PITR", "Restaura a tabela para qualquer segundo dos últimos 35 dias.", "Proteção contra exclusão", "Impede excluir a tabela até alguém desativar a proteção.", "Restauração", "Cria uma tabela NOVA com os dados daquele momento."),
                "Para retenção longa ou cópia em outra conta/região, combine com o AWS Backup."));
            list.Add(AwsL(2, "e57", "Freio na concorrência",
                "Quando chega um arquivo grande, a importar-estoque dispara centenas de execuções ao mesmo tempo e o banco recusa conexões, derrubando a API. O banco aceita 60 conexões, a API usa até 50 e cada execução da função abre 1.",
                "Configure a simultaneidade reservada da função importar-estoque para que ela nunca use mais conexões do que as que sobram no banco.",
                A("Console: Lambda → importar-estoque → Configuração → Simultaneidade → Editar → Reservar simultaneidade: 10 → Salvar", "aws lambda put-function-concurrency --function-name importar-estoque --reserved-concurrent-executions 10"),
                A("Faça a conta das conexões que sobram para a função.", "60 − 50 = 10. Na CLI: put-function-concurrency --reserved-concurrent-executions N."),
                A("Simultaneidade reservada", "Garante E limita quantas execuções da função rodam ao mesmo tempo.", "Throttling", "Acima do limite, as invocações são limitadas (as assíncronas esperam e são tentadas de novo).", "0", "Reservar 0 desliga a função."),
                "Para bancos relacionais, use também o RDS Proxy: ele reaproveita conexões entre as execuções."));
            list.Add(AwsL(2, "e58", "Sub-rede privada nova",
                "Os servidores internos só existem na zona us-east-1a (privada-a, 10.0.10.0/24): se a zona cair, tudo cai. Padrão de endereços: públicas em 10.0.1.0/24, 10.0.2.0/24...; privadas em 10.0.10.0/24, 10.0.11.0/24...",
                "Na VPC cafe365-vpc, crie a sub-rede privada-b na zona us-east-1b, com o próximo bloco /24 livre do padrão das privadas.",
                A("Console: VPC → Sub-redes → Criar sub-rede → VPC cafe365-vpc → nome privada-b → zona us-east-1b → CIDR 10.0.11.0/24 → Criar sub-rede", "aws ec2 create-subnet --vpc-id vpc-0cafe365 --cidr-block 10.0.11.0/24 --availability-zone us-east-1b --tag-specifications 'ResourceType=subnet,Tags=[{Key=Name,Value=privada-b}]'"),
                A("Sub-rede = pedaço da VPC numa única zona; o bloco não pode sobrepor outro.", "Depois de 10.0.10.0/24 vem 10.0.11.0/24. O nome vai na tag Name."),
                A("Bloco /24", "256 endereços (a AWS reserva 5 em cada sub-rede).", "Zona de disponibilidade", "Data center isolado: espalhe as sub-redes para sobreviver a falhas.", "Sobreposição", "Dois blocos que compartilham endereços: a AWS recusa (InvalidSubnet.Conflict)."),
                "Planeje (e documente) o endereçamento antes: mudar CIDR depois é muito mais difícil que criar certo."));
            list.Add(AwsL(2, "e59", "Ressuscitando o index",
                "Alguém apagou o index.html do bucket cafe365-site e o site mostra erro. O bucket tem versionamento; a cópia no seu computador é de 3 meses atrás.",
                "Restaure a versão do index.html que estava no bucket, removendo o marcador de exclusão. Não envie a cópia local.",
                A("Console: S3 → cafe365-site → Mostrar versões → selecione o 'Marcador de exclusão' de index.html → Excluir → confirmar", "aws s3api delete-object --bucket cafe365-site --key index.html --version-id QX7ZK2PLM9NDR4TB"),
                A("Com versionamento, excluir só coloca um marcador por cima da versão anterior.", "Mostre as versões e exclua o MARCADOR. Na CLI: list-object-versions mostra o VersionId do marcador; delete-object --version-id remove ele."),
                A("Marcador de exclusão", "Versão 'vazia' que esconde o objeto; removê-lo faz a versão anterior voltar a ser a atual.", "list-object-versions", "Lista versões e marcadores com seus VersionId.", "delete-object --version-id", "Remove uma versão específica (numa versão de verdade, é definitivo!)."),
                "Em buckets críticos, use também MFA Delete ou Object Lock: excluir versões passa a exigir mais que um clique."));
            list.Add(AwsL(2, "e60", "Mensagens em dobro",
                "Clientes estão sendo cobrados duas vezes. A processar-pedidos (disparada pela fila pedidos) pode levar quase todo o seu tempo limite, mas a visibilidade da fila é de 30 s: a mensagem reaparece no meio do processamento.",
                "Ajuste o tempo limite de visibilidade da fila pedidos seguindo a recomendação da AWS para filas que disparam Lambda: 6 vezes o tempo limite da função.",
                A("Console: Lambda → processar-pedidos → Configuração (tempo limite 50 s); depois Simple Queue Service → pedidos → Editar → Tempo limite de visibilidade 300 → Salvar", "aws sqs set-queue-attributes --queue-url https://sqs.us-east-1.amazonaws.com/123456789012/pedidos --attributes VisibilityTimeout=300"),
                A("Descubra o tempo limite da função na Lambda (Configuração geral).", "6 × 50 s = 300 s. Na CLI: --attributes VisibilityTimeout=300."),
                A("Tempo limite de visibilidade", "Quanto tempo a mensagem fica invisível depois de entregue, esperando ser apagada.", "6 × timeout", "Dá folga para as novas tentativas da Lambda e para o lote inteiro.", "Idempotência", "O processamento deve aguentar a mesma mensagem duas vezes."),
                "Mesmo com a visibilidade certa, a fila Padrão entrega 'pelo menos uma vez': guarde o ID do pedido e ignore repetidos."));
            list.Add(AwsL(2, "e61", "Sem chaves no servidor",
                "A aplicação do web-1 lê o S3 usando a chave de acesso do usuário app-web, salva num arquivo no servidor. Já existe a função (role) app-s3-leitura com a mesma permissão.",
                "Faça o web-1 usar a função do IAM app-s3-leitura e depois exclua a chave de acesso do app-web (AKIA5APPWEB0SRV00001), sem deixar a aplicação parar.",
                A("Console: EC2 → selecione web-1 → Ações → Segurança → Modificar função do IAM → app-s3-leitura → Atualizar função do IAM; depois IAM → Usuários → app-web → selecione a chave → Ações → Excluir", "aws ec2 associate-iam-instance-profile --instance-id i-0web001 --iam-instance-profile Name=app-s3-leitura  +  aws iam delete-access-key --user-name app-web --access-key-id AKIA5APPWEB0SRV00001"),
                A("A ordem importa: a aplicação precisa de outra fonte de credenciais antes de a chave sumir.", "Primeiro a função na instância (perfil de instância), depois a exclusão da chave. Na CLI: associate-iam-instance-profile e delete-access-key."),
                A("Perfil de instância", "Liga a role à instância; o SDK busca as credenciais temporárias no IMDS sozinho.", "Credenciais temporárias", "Expiram e são renovadas automaticamente: nada fixo para vazar.", "delete-access-key", "Remove a chave de vez."),
                "Regra de ouro: código rodando NA AWS nunca usa chave de acesso fixa — usa role."));
            list.Add(AwsL(2, "e62", "Trilha de auditoria",
                "A conta não guarda registro de quem fez o quê além dos 90 dias do histórico de eventos. A auditoria exige uma trilha permanente, de todas as regiões, à prova de adulteração.",
                "Crie a trilha auditoria do CloudTrail, multirregional, com validação dos arquivos de log, gravando no bucket micorosoft-auditoria — e garanta que ela fique registrando.",
                A("Console: CloudTrail → Trilhas → Criar trilha → nome auditoria → aplicar a todas as regiões → bucket existente micorosoft-auditoria → validação do arquivo de log ativada → Criar trilha", "aws cloudtrail create-trail --name auditoria --s3-bucket-name micorosoft-auditoria --is-multi-region-trail --enable-log-file-validation  +  aws cloudtrail start-logging --name auditoria"),
                A("Pelo console a trilha já começa registrando; pela CLI, não.", "Na CLI: create-trail com --is-multi-region-trail e --enable-log-file-validation, e depois start-logging."),
                A("Trilha multirregional", "Registra as ações de todas as regiões num lugar só.", "Validação de arquivo", "Arquivos de resumo assinados provam que nenhum log foi alterado ou apagado.", "start-logging", "Uma trilha criada pela API nasce parada."),
                "Proteja o bucket da trilha (acesso restrito, Object Lock) e, em empresas maiores, use uma trilha da organização numa conta separada."));
            list.Add(AwsL(2, "e63", "Escala automática por CPU",
                "O asg-web (de 2 a 6 instâncias) ainda é ajustado à mão. O gargalo do site é CPU.",
                "Crie no asg-web a política de escalabilidade de rastreamento de destino cpu-50, que mantenha a utilização média da CPU em 50%.",
                A("Console: EC2 → Grupos do Auto Scaling → asg-web → Criar política de escalabilidade dinâmica → Rastreamento de destino → nome cpu-50 → Utilização média da CPU → 50 → Criar", "aws autoscaling put-scaling-policy --auto-scaling-group-name asg-web --policy-name cpu-50 --policy-type TargetTrackingScaling --target-tracking-configuration '{\"PredefinedMetricSpecification\":{\"PredefinedMetricType\":\"ASGAverageCPUUtilization\"},\"TargetValue\":50.0}'"),
                A("Rastreamento de destino funciona como um termostato.", "Métrica ASGAverageCPUUtilization, TargetValue 50. Na CLI: put-scaling-policy --policy-type TargetTrackingScaling."),
                A("Rastreamento de destino", "O Auto Scaling cria os alarmes e ajusta a capacidade desejada para manter a métrica no alvo.", "50%", "Folga para absorver picos enquanto as instâncias novas sobem.", "Limites", "A política nunca passa da mínima/máxima do grupo."),
                "Para picos com hora marcada (abertura de promoção), some uma ação agendada que aumenta a capacidade antes."));
            list.Add(AwsL(2, "e64", "Saída pela NAT",
                "Os servidores da sub-rede privada-a não conseguem baixar atualizações: a tabela rt-privada só tem a rota local. Existe o gateway NAT nat-0cafe01 na sub-rede pública.",
                "Na tabela de rotas rt-privada (rtb-0privada), adicione a rota para a internet (0.0.0.0/0) pelo gateway NAT nat-0cafe01, sem tornar a sub-rede pública.",
                A("Console: VPC → Tabelas de rotas → rt-privada → Editar rotas → Destino 0.0.0.0/0, Alvo nat-0cafe01 → Salvar alterações", "aws ec2 create-route --route-table-id rtb-0privada --destination-cidr-block 0.0.0.0/0 --nat-gateway-id nat-0cafe01"),
                A("0.0.0.0/0 = 'todo o resto'.", "O alvo é o gateway NAT, não o gateway da Internet. Na CLI: create-route --nat-gateway-id."),
                A("Gateway NAT", "Deixa a sub-rede privada SAIR para a internet, sem que ninguém de fora inicie conexão.", "Gateway da Internet", "Usado pelas sub-redes públicas (entrada e saída).", "Rota mais específica", "10.0.0.0/16 → local continua mantendo o tráfego interno."),
                "NAT cobra por hora e por GB: para falar com S3 e DynamoDB, use VPC endpoints (gateway) e economize."));
            list.Add(AwsL(2, "e65", "Site só pela CDN",
                "O cafe365-site ainda é público no S3, e dá para burlar a CloudFront acessando o bucket direto. A distribuição E1CAFE365SITE já usa OAC (controle de acesso de origem); falta o bucket confiar só nela.",
                "Deixe o bucket cafe365-site acessível SÓ pela CloudFront: troque a política dele pela que libera apenas a distribuição e ative o bloqueio de todo o acesso público — sem derrubar o site.",
                A("Console: S3 → cafe365-site → Permissões → Política do bucket → Editar → Inserir exemplo: politica-cloudfront-site.json → Salvar; depois Bloquear acesso público → Editar → Bloquear todo o acesso público → Salvar", "aws s3api put-bucket-policy --bucket cafe365-site --policy file://politica-cloudfront-site.json  +  aws s3api put-public-access-block --bucket cafe365-site --public-access-block-configuration BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true"),
                A("Se você bloquear primeiro, a CloudFront (que hoje lê pelo acesso público) perde o acesso.", "Primeiro a política com o principal cloudfront.amazonaws.com, depois o bloqueio."),
                A("OAC", "A CloudFront assina as requisições ao S3; a política do bucket confia nessa assinatura.", "AWS:SourceArn", "Condição que limita a permissão à SUA distribuição.", "Ordem das mudanças", "Remova um acesso só depois que o novo caminho já funciona."),
                "Planeje toda mudança de permissão como uma migração: novo caminho primeiro, corte do antigo depois."));
            list.Add(AwsL(2, "e66", "TTL antes da migração",
                "Amanhã a API muda de IP. Hoje o registro api.cafe365.com.br tem TTL de 86400 s (1 dia): depois da troca, muitos clientes continuariam no IP velho por até um dia.",
                "Reduza HOJE o TTL do registro A api.cafe365.com.br para 60 segundos, sem trocar o IP ainda.",
                A("Console: Route 53 → cafe365.com.br → selecione api.cafe365.com.br → Editar registro → TTL 60 → Salvar", "aws route53 change-resource-record-sets --hosted-zone-id Z0CAFE365BR --change-batch '{\"Changes\":[{\"Action\":\"UPSERT\",\"ResourceRecordSet\":{\"Name\":\"api.cafe365.com.br\",\"Type\":\"A\",\"TTL\":60,\"ResourceRecords\":[{\"Value\":\"54.210.12.8\"}]}}]}'"),
                A("Mude só o TTL; o valor continua 54.210.12.8.", "Na CLI, UPSERT cria ou substitui o registro inteiro: repita o valor atual."),
                A("TTL", "Tempo que os resolvedores guardam a resposta.", "Reduzir antes", "Espere passar o TTL ANTIGO (1 dia) para todos pegarem o TTL curto; só então troque o IP.", "UPSERT", "Cria se não existe, substitui se existe."),
                "Com a migração estabilizada, volte o TTL para um valor maior (menos consultas e mais resiliência)."));
            list.Add(AwsL(2, "e67", "Alarme na DLQ",
                "A DLQ pedidos-dlq já recebe os pedidos que falham, mas ninguém percebe: na semana passada 40 pedidos ficaram lá por três dias.",
                "Crie o alarme dlq-pedidos, que dispare quando a quantidade de mensagens visíveis na fila pedidos-dlq for maior que 0, notificando o tópico alertas-ti.",
                A("Console: CloudWatch → Alarmes → Criar alarme → namespace AWS/SQS → ApproximateNumberOfMessagesVisible → QueueName = pedidos-dlq → Maior que 0 → notificação alertas-ti → nome dlq-pedidos → Criar alarme", "aws cloudwatch put-metric-alarm --alarm-name dlq-pedidos --namespace AWS/SQS --metric-name ApproximateNumberOfMessagesVisible --dimensions Name=QueueName,Value=pedidos-dlq --statistic Maximum --period 300 --evaluation-periods 1 --threshold 0 --comparison-operator GreaterThanThreshold --alarm-actions arn:aws:sns:us-east-1:123456789012:alertas-ti"),
                A("Métricas de fila ficam no namespace AWS/SQS.", "Mensagens movidas pela redrive não contam como 'enviadas': use ApproximateNumberOfMessagesVisible, com a dimensão QueueName=pedidos-dlq."),
                A("ApproximateNumberOfMessagesVisible", "Mensagens esperando na fila.", "NumberOfMessagesSent", "Só conta envios diretos (SendMessage), não as mensagens movidas para a DLQ.", "Dimensão QueueName", "Escolhe qual fila o alarme observa."),
                "Alarme na DLQ + painel com a idade da mensagem mais antiga = problema achado em minutos, não em dias."));
            list.Add(AwsL(2, "e68", "Versões que pesam",
                "Com o versionamento do cafe365-site, cada deploy guarda cópias antigas de todos os arquivos — e a fatura do bucket triplicou.",
                "No cafe365-site, crie a regra de ciclo de vida limpar-versoes-antigas, para o bucket inteiro, que exclui permanentemente as versões desatualizadas (não atuais) 30 dias depois de ficarem desatualizadas — sem apagar as versões atuais.",
                A("Console: S3 → cafe365-site → Gerenciamento → Criar regra de ciclo de vida → limpar-versoes-antigas → Aplicar a todos os objetos do bucket → Excluir permanentemente versões desatualizadas: 30 dias → Criar regra", "aws s3api put-bucket-lifecycle-configuration --bucket cafe365-site --lifecycle-configuration '{\"Rules\":[{\"ID\":\"limpar-versoes-antigas\",\"Filter\":{\"Prefix\":\"\"},\"Status\":\"Enabled\",\"NoncurrentVersionExpiration\":{\"NoncurrentDays\":30}}]}'"),
                A("'Expirar versões atuais' apagaria o site no ar!", "A ação certa é para versões DESATUALIZADAS (NoncurrentVersionExpiration.NoncurrentDays = 30)."),
                A("Versão desatualizada", "Versão antiga, que já foi substituída ou escondida por um marcador.", "NoncurrentVersionExpiration", "Apaga versões antigas N dias depois de deixarem de ser atuais.", "Expiration", "Mexe nas versões ATUAIS — cuidado em sites."),
                "Versionamento + regra para versões antigas: proteção contra erro humano com custo controlado."));
            list.Add(AwsL(2, "e69", "Desligamento completo",
                "O João saiu da empresa hoje. Ele está no grupo Desenvolvedores e tem uma chave de acesso ativa. O Rodrigo prefere a CLI, que não apaga nada 'escondido'.",
                "Remova totalmente o usuário IAM joao (grupo, chave de acesso e o próprio usuário), sem mexer em nenhum outro usuário.",
                A("Console: IAM → Usuários → selecione joao → Excluir → confirmar (o console remove o grupo e as chaves junto)", "aws iam remove-user-from-group --user-name joao --group-name Desenvolvedores  +  aws iam delete-access-key --user-name joao --access-key-id AKIA5JOAO0000000DEV1  +  aws iam delete-user --user-name joao"),
                A("Pela CLI, delete-user falha (DeleteConflict) enquanto houver grupo, chaves ou políticas.", "Ordem na CLI: remove-user-from-group → delete-access-key → delete-user (aws iam list-access-keys mostra o ID da chave)."),
                A("DeleteConflict", "A API não apaga um usuário com dependências: é preciso removê-las antes.", "remove-user-from-group", "Tira o usuário do grupo.", "Console", "O botão Excluir do console faz a limpeza das dependências por você."),
                "Desligamento é processo: corte o acesso NO DIA (de preferência no provedor de identidade central) e revise o que a pessoa criou."));
            list.Add(AwsL(2, "e70", "Banco de alta disponibilidade",
                "A auditoria de amanhã exige que o banco de produção sobreviva à queda de uma zona e não possa ser apagado por engano.",
                "No banco-cafe, ative a implantação Multi-AZ e a proteção contra exclusão, aplicando imediatamente. Não mude a classe nem outras configurações.",
                A("Console: RDS → selecione banco-cafe → Modificar → Instância de banco de dados Multi-AZ → marque Ativar a proteção contra exclusão → Aplicar imediatamente → Modificar instância de banco de dados", "aws rds modify-db-instance --db-instance-identifier banco-cafe --multi-az --deletion-protection --apply-immediately"),
                A("As duas opções ficam no mesmo formulário Modificar.", "Na CLI: --multi-az --deletion-protection --apply-immediately (trocar a classe reiniciaria o banco)."),
                A("Multi-AZ", "Réplica síncrona em espera em outra zona, com failover automático.", "Proteção contra exclusão", "Impede excluir o banco até alguém desativar a proteção.", "Conversão", "Ativar Multi-AZ não derruba o banco (pode haver mais latência durante a cópia inicial)."),
                "Multi-AZ é disponibilidade, não backup: um DELETE errado é replicado na hora. Backups e snapshots continuam essenciais."));
            list.Add(AwsL(2, "e71", "Faxina no registry",
                "Cada build do pipeline deixa imagens SEM TAG no repositório cafe365-web (a tag passa para a imagem nova). Já são 2 mil imagens órfãs.",
                "No repositório cafe365-web, crie a regra de ciclo de vida que expira as imagens SEM TAG enviadas há mais de 7 dias (imagens com tag não podem ser afetadas).",
                A("Console: ECR → selecione cafe365-web → Ações → Política de ciclo de vida → Criar regra → Status da imagem: Sem tag → Desde o envio da imagem (dias): 7 → Salvar", "aws ecr put-lifecycle-policy --repository-name cafe365-web --lifecycle-policy-text '{\"rules\":[{\"rulePriority\":1,\"description\":\"remover imagens sem tag\",\"selection\":{\"tagStatus\":\"untagged\",\"countType\":\"sinceImagePushed\",\"countUnit\":\"days\",\"countNumber\":7},\"action\":{\"type\":\"expire\"}}]}'"),
                A("'Qualquer' incluiria as imagens com tag que estão em produção.", "tagStatus untagged, countType sinceImagePushed, countUnit days, countNumber 7."),
                A("Imagem sem tag", "Sobra quando uma tag é movida para outra imagem.", "sinceImagePushed", "Critério por idade (dias desde o push).", "imageCountMoreThan", "Critério por quantidade (mantém as N mais novas)."),
                "Teste regras novas com a prévia do ciclo de vida antes de salvar: apagar imagem de produção dói."));
            list.Add(AwsL(2, "e72", "Discos sempre criptografados",
                "A política de segurança exige que TODO disco novo seja criptografado. A empresa usa as regiões Norte da Virgínia e São Paulo.",
                "Ative a criptografia padrão do EBS (sempre criptografar novos volumes) nas regiões us-east-1 e sa-east-1.",
                A("Console: EC2 → Configurações da conta → Criptografia do EBS → Gerenciar → Ativar → Atualizar criptografia do EBS; troque a região para São Paulo e repita", "aws ec2 enable-ebs-encryption-by-default  +  aws ec2 enable-ebs-encryption-by-default --region sa-east-1"),
                A("A configuração é POR REGIÃO.", "Na CLI: aws ec2 enable-ebs-encryption-by-default (e de novo com --region sa-east-1)."),
                A("Criptografia padrão do EBS", "Todo volume e snapshot NOVO da região nasce criptografado.", "Por região", "Cada região tem a sua configuração.", "Volumes existentes", "Não mudam: para criptografar, crie um snapshot, copie com criptografia e recrie o volume."),
                "Transforme isso em controle preventivo (AWS Config, SCP) para que nenhuma conta ou região nova escape."));
        }
    }

    static partial class Docs
    {
        static partial void DocsExtraAws(List<DocEntry> all)
        {
            const string W = "AWS";

            // ── Console de gerenciamento (telas novas)
            all.Add(X(W, "Console de gerenciamento", "S3: abas do bucket (tags, site, permissões, ciclo de vida, versões)", "A página do bucket tem abas: Objetos, Propriedades, Permissões e Gerenciamento.",
                "Objetos → selecione → Ações → Compartilhar com URL pré-assinada  ·  Mostrar versões\nPropriedades → Tags → Editar tags  ·  Hospedagem de site estático → Editar\nPermissões → Bloquear acesso público → Editar  ·  Política do bucket → Editar\nGerenciamento → Criar regra de ciclo de vida",
                A("Mostrar versões", "Lista versões e marcadores de exclusão (bucket versionado).", "Inserir exemplo", "No editor de política, carrega um JSON da pasta do CloudShell.", "Ciclo de vida", "Expirar versões atuais, excluir versões desatualizadas, mover para o Glacier."),
                A("Propriedades → Tags → Turma = 3A", "etiqueta o bucket boletins-escola", "Gerenciamento → regra 'apagar-rascunhos' com prefixo rascunhos/ e expiração de 15 dias", "faxina só de uma pasta"),
                "Em bucket versionado, 'Excluir' só cria um marcador: a versão anterior continua lá (e cobrando)."));
            all.Add(X(W, "Console de gerenciamento", "EC2: IPs elásticos, pares de chaves, AMIs e configurações", "Itens do menu lateral do EC2 e do menu Ações das instâncias.",
                "IPs elásticos → Alocar / Ações → Associar · Desassociar · Liberar\nPares de chaves → Criar par de chaves (RSA ou ED25519, .pem ou .ppk)\nInstâncias → Ações → Imagem e modelos → Criar imagem  ·  Configurações da instância → Proteção contra encerramento / Opções de metadados  ·  Segurança → Modificar função do IAM\nVolumes → Ações → Modificar volume  ·  Configurações da conta → Criptografia do EBS",
                A("Permitir reassociação", "Move um IP elástico que já está em outra instância.", "Somente V2 (token obrigatório)", "IMDSv2: protege as credenciais da instância contra SSRF.", "Criptografia do EBS", "Configuração por região: vale para volumes novos."),
                A("Associar 3.5.6.7 à instância portal-escola", "IP fixo para o portal", "Volumes → vol-0esc001 → Modificar volume → gp3", "troca o tipo sem parar a máquina"),
                "Instâncias, IPs, pares de chaves e AMIs são regionais: confira a região antes de procurar."));
            all.Add(X(W, "Console de gerenciamento", "EC2: grupos do Auto Scaling", "Mantêm a quantidade certa de instâncias entre a capacidade mínima e a máxima.",
                "EC2 → Grupos do Auto Scaling → (grupo) → Editar: desejada, mínima e máxima\n(grupo) → Criar política de escalabilidade dinâmica → Rastreamento de destino → métrica + valor de destino",
                A("Desejada", "Quantas instâncias rodar agora.", "Mínima / máxima", "Piso de disponibilidade e teto de custo.", "Rastreamento de destino", "Mantém uma métrica (ex.: CPU média) perto de um alvo."),
                A("asg-portal: mínima 1, máxima 3", "limites do portal da escola", "Política 'rede-60': Média de bytes de entrada da rede", "escala pelo tráfego"), null));
            all.Add(X(W, "Console de gerenciamento", "VPC: sub-redes e tabelas de rotas", "A VPC é a rede privada; sub-redes ficam em uma zona; cada uma usa uma tabela de rotas.",
                "VPC → Sub-redes → Criar sub-rede → VPC, nome, zona, bloco CIDR\nVPC → Tabelas de rotas → (tabela) → Editar rotas → destino + alvo → Salvar alterações",
                A("Sub-rede pública", "Rota 0.0.0.0/0 → gateway da Internet (igw-...).", "Sub-rede privada", "Sem rota direta da internet; sai pelo gateway NAT (nat-...).", "local", "Rota da própria VPC; não pode ser alterada."),
                A("Sub-rede 'lab-c' 172.20.3.0/24 em sa-east-1c", "sub-rede numa terceira zona", "rt-laboratorio: 0.0.0.0/0 → nat-0escola", "saída para a internet sem expor os servidores"), null));
            all.Add(X(W, "Console de gerenciamento", "IAM: funções, chaves de acesso e política de senha", "Telas de identidade além de usuários e grupos.",
                "IAM → Funções → Criar função → caso de uso (EC2, Lambda...) → políticas → nome\nIAM → Usuários → (usuário) → Chaves de acesso → Criar · Ações → Desativar / Ativar / Excluir · Remover do grupo\nIAM → Configurações da conta → Política de senha → Editar",
                A("Função (role)", "Identidade sem senha nem chave fixa; serviços a assumem e recebem credenciais temporárias.", "Entidade confiável", "Quem pode assumir a função.", "Duas chaves por usuário", "Permitem trocar a chave sem parar o sistema."),
                A("Função 'ec2-boletins' (EC2) com AmazonS3ReadOnlyAccess", "servidor lê o S3 sem chave fixa", "Desativar a chave do usuário 'robo-notas'", "corta o acesso sem apagar"), null));
            all.Add(X(W, "Console de gerenciamento", "Lambda: abas Código, Testar e Configuração", "Ajustes da função sem publicar código novo.",
                "Lambda → função → Adicionar gatilho (SQS: fila + tamanho do lote)\nCódigo → Configurações de tempo de execução → Editar (runtime)\nConfiguração → Configuração geral (memória, tempo limite) · Variáveis de ambiente · Simultaneidade",
                A("Tempo limite", "Até 15 minutos por execução.", "Variáveis de ambiente", "Salvar substitui o conjunto inteiro.", "Simultaneidade reservada", "Garante e limita as execuções ao mesmo tempo (0 = função desligada)."),
                A("matricula: memória 512 MB, tempo limite 45 s", "função mais folgada", "Variáveis: ESCOLA=centro, MODO=teste", "configuração sem mexer no código"), null));
            all.Add(X(W, "Console de gerenciamento", "CloudWatch: filtro, retenção e alarmes com notificação", "Logs e alarmes além do básico.",
                "Grupos de logs → (grupo) → campo de filtro (diferencia maiúsculas)\nGrupos de logs → selecione → Ações → Editar configuração de retenção\nAlarmes → Criar alarme → namespace (AWS/EC2, AWS/SQS, AWS/RDS) → métrica → recurso → condição → notificação (tópico do SNS) → nome  ·  Alarmes → selecione → Excluir",
                A("Namespace", "Grupo de métricas de um serviço.", "Notificação", "Tópico do SNS avisado quando o alarme entra EM ALARME.", "Retenção", "Dias de logs guardados (o padrão é nunca expirar)."),
                A("Filtro 'Timeout' em /aws/lambda/matricula", "acha execuções estouradas", "Alarme 'fila-presenca': AWS/SQS ApproximateAgeOfOldestMessage > 600", "fila parada há 10 minutos"), null));
            all.Add(X(W, "Console de gerenciamento", "SNS e SQS no console", "Tópicos (notificações para vários assinantes) e filas (mensagens esperando processamento).",
                "SNS → Tópicos → Criar tópico (Padrão ou FIFO) → (tópico) → Criar assinatura (protocolo + endpoint) · Publicar mensagem\nSQS → Criar fila (Padrão ou FIFO, visibilidade, espera, deduplicação, DLQ) → (fila) → Editar",
                A("Tempo limite de visibilidade", "Tempo que a mensagem fica escondida depois de entregue.", "Tempo de espera de recebimento", "1 a 20 s = sondagem longa (menos chamadas vazias).", "Fila de mensagens mortas", "Recebe as mensagens que falharam N vezes."),
                A("Tópico 'avisos-escola' + assinatura e-mail secretaria@escola.edu.br", "avisos por e-mail", "Fila 'presencas' com espera de 10 s", "sondagem longa"), null));
            all.Add(X(W, "Console de gerenciamento", "DynamoDB e RDS no console", "Bancos gerenciados: NoSQL (DynamoDB) e relacional (RDS).",
                "DynamoDB → Criar tabela (chave de partição + tipo, chave de classificação, sob demanda/provisionada) → (tabela) → Explorar itens · TTL · Backups (PITR) · Proteção contra exclusão\nRDS → selecione → Ações (Interromper, Iniciar, Reinicializar, Tirar snapshot) · Modificar (classe, Multi-AZ, acesso público, retenção de backup, proteção) → Aplicar imediatamente ou na janela",
                A("Sob demanda", "Paga por requisição: bom para tráfego imprevisível.", "PITR", "Restauração para qualquer segundo dos últimos 35 dias.", "Janela de manutenção", "Horário em que as mudanças pendentes do RDS são aplicadas."),
                A("Tabela 'alunos': chave matricula (String)", "um item por aluno", "banco-escola → Modificar → retenção de backup 14 dias → Aplicar imediatamente", "mais dias de backup"), null));
            all.Add(X(W, "Console de gerenciamento", "Route 53, CloudFront, CloudTrail, KMS, Budgets e ECR", "Resumo das demais telas usadas no Centro de Nuvem.",
                "Route 53 → zona → Criar registro / Editar registro / Excluir registro\nCloudFront → distribuição → Geral (objeto raiz) · Comportamentos (protocolo do visualizador) · Invalidações\nCloudTrail → Criar trilha  ·  KMS → chave → Rotação de chaves  ·  Billing e Budgets → Orçamentos (+ alertas)\nECR → repositório → Ações → Editar (imutabilidade, verificação) / Política de ciclo de vida",
                A("TTL", "Tempo de cache da resposta DNS.", "Redirecionar HTTP para HTTPS", "301 para a versão segura.", "Trilha multirregional", "Registra as ações de todas as regiões.", "Status da imagem", "Com tag, sem tag ou qualquer (regras do ECR)."),
                A("Registro A 'secretaria' → 203.0.113.40, TTL 300", "nome para o servidor da secretaria", "Invalidação /css/*", "atualiza só os estilos"), null));

            // ── CLI
            all.Add(X(W, "S3", "aws s3api put-bucket-tagging / aws s3 website", "Tags do bucket e hospedagem de site estático.",
                "aws s3api put-bucket-tagging --bucket BUCKET --tagging 'TagSet=[{Key=CHAVE,Value=VALOR}]'\naws s3api get-bucket-tagging --bucket BUCKET\naws s3 website s3://BUCKET/ --index-document index.html [--error-document erro.html]",
                A("TagSet", "Lista completa de tags (substitui as anteriores).", "--index-document", "Página inicial do site.", "--error-document", "Página dos erros 4XX."),
                A("aws s3api put-bucket-tagging --bucket boletins-escola --tagging 'TagSet=[{Key=Turma,Value=3A}]'", "etiqueta o bucket", "aws s3 website s3://portal-escola/ --index-document inicio.html", "site com inicio.html como página inicial"), null));
            all.Add(X(W, "S3", "aws s3api put-public-access-block / put-bucket-policy", "Bloqueio de acesso público e política do bucket (o JSON pode vir de um arquivo com file://).",
                "aws s3api put-public-access-block --bucket BUCKET --public-access-block-configuration BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true\naws s3api put-bucket-policy --bucket BUCKET --policy file://politica.json\naws s3api get-bucket-policy --bucket BUCKET\naws s3api delete-bucket-policy --bucket BUCKET",
                A("As 4 opções =true", "Bloqueio total do acesso público.", "file://", "A CLI lê o parâmetro de um arquivo (veja com ls e cat).", "Deny + aws:SecureTransport=false", "Recusa requisições sem HTTPS."),
                A("aws s3api put-public-access-block --bucket fotos-formatura --public-access-block-configuration BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true", "fecha o bucket", "aws s3api put-bucket-policy --bucket notas-escola --policy file://so-tls-notas.json", "aplica uma política salva em arquivo"),
                "Uma política que cita outro bucket no Resource é recusada (MalformedPolicy)."));
            all.Add(X(W, "S3", "aws s3api put-bucket-lifecycle-configuration", "Regras de ciclo de vida em JSON (substitui todas as regras do bucket).",
                "aws s3api put-bucket-lifecycle-configuration --bucket BUCKET --lifecycle-configuration '{\"Rules\":[{\"ID\":\"NOME\",\"Filter\":{\"Prefix\":\"\"},\"Status\":\"Enabled\",\"Expiration\":{\"Days\":N}}]}'\naws s3api get-bucket-lifecycle-configuration --bucket BUCKET",
                A("Filter.Prefix", "\"\" = bucket inteiro; \"logs/\" = só essa pasta.", "Expiration.Days", "Apaga as versões ATUAIS com mais de N dias.", "NoncurrentVersionExpiration.NoncurrentDays", "Apaga versões antigas N dias depois de ficarem desatualizadas.", "Transitions", "[{\"Days\":30,\"StorageClass\":\"GLACIER\"}] move para o Glacier."),
                A("aws s3api put-bucket-lifecycle-configuration --bucket provas-escola --lifecycle-configuration '{\"Rules\":[{\"ID\":\"apagar-rascunhos\",\"Filter\":{\"Prefix\":\"rascunhos/\"},\"Status\":\"Enabled\",\"Expiration\":{\"Days\":15}}]}'", "apaga rascunhos com mais de 15 dias"),
                "Em sites com versionamento, nunca use Expiration por engano: ela expira o conteúdo no ar."));
            all.Add(X(W, "S3", "aws s3 presign / s3api list-object-versions / delete-object", "Links temporários e versões de objetos.",
                "aws s3 presign s3://BUCKET/OBJETO --expires-in SEGUNDOS\naws s3api list-object-versions --bucket BUCKET [--prefix CHAVE]\naws s3api delete-object --bucket BUCKET --key CHAVE --version-id ID",
                A("--expires-in", "Validade da URL (padrão 3600 s; máximo 7 dias).", "DeleteMarkers", "Marcadores de exclusão listados com o VersionId.", "--version-id", "Apaga uma versão específica: se for o marcador, o objeto 'volta'."),
                A("aws s3 presign s3://boletins-escola/turma-3a.pdf --expires-in 900", "link de 15 minutos", "aws s3api delete-object --bucket provas-escola --key gabarito.pdf --version-id Z9Y8X7W6", "remove o marcador e recupera o arquivo"), null));
            all.Add(X(W, "EC2", "aws ec2 create-key-pair / allocate-address / associate-address / release-address", "Pares de chaves e IPs elásticos.",
                "aws ec2 create-key-pair --key-name NOME --key-type ed25519|rsa\naws ec2 describe-addresses\naws ec2 allocate-address\naws ec2 associate-address --instance-id ID --allocation-id eipalloc-...\naws ec2 release-address --allocation-id eipalloc-...",
                A("KeyMaterial", "A chave privada aparece só na saída do create-key-pair.", "--allow-reassociation", "Move um IP que já está associado.", "release-address", "Só funciona com o IP desassociado (InvalidIPAddress.InUse)."),
                A("aws ec2 create-key-pair --key-name prof-maria --key-type ed25519", "chave nova para a professora", "aws ec2 associate-address --instance-id i-0esc001 --allocation-id eipalloc-0esc9", "IP fixo no servidor da escola"), null));
            all.Add(X(W, "EC2", "aws ec2 create-image / run-instances", "Cria AMIs e executa instâncias a partir delas.",
                "aws ec2 create-image --instance-id ID --name NOME [--no-reboot]\naws ec2 describe-images --owners self\naws ec2 run-instances --image-id ami-... --instance-type TIPO --key-name CHAVE --security-group-ids sg-... --tag-specifications 'ResourceType=instance,Tags=[{Key=Name,Value=NOME}]'",
                A("--owners self", "Lista só as AMIs da sua conta.", "--tag-specifications", "Tags na criação (o nome é a tag Name).", "--security-group-ids", "Grupos de segurança da nova instância."),
                A("aws ec2 create-image --instance-id i-0esc001 --name portal-base-v2", "imagem do portal", "aws ec2 run-instances --image-id ami-0esc777 --instance-type t3.micro --key-name chave-lab --security-group-ids sg-0escola1 --tag-specifications 'ResourceType=instance,Tags=[{Key=Name,Value=lab-2}]'", "clona o servidor do laboratório"), null));
            all.Add(X(W, "EC2", "aws ec2 modify-instance-attribute / modify-instance-metadata-options / associate-iam-instance-profile", "Proteção contra encerramento, IMDSv2 e função do IAM da instância.",
                "aws ec2 modify-instance-attribute --instance-id ID --disable-api-termination   (--no-disable-api-termination desfaz)\naws ec2 modify-instance-metadata-options --instance-id ID --http-tokens required --http-endpoint enabled\naws ec2 associate-iam-instance-profile --instance-id ID --iam-instance-profile Name=PERFIL",
                A("--disable-api-termination", "Liga a proteção contra encerramento.", "--http-tokens required", "Exige IMDSv2 (token de sessão).", "--iam-instance-profile Name=...", "Perfil de instância com o mesmo nome da função."),
                A("aws ec2 modify-instance-metadata-options --instance-id i-0esc002 --http-tokens required --http-endpoint enabled --region eu-west-1", "IMDSv2 numa instância da Irlanda", "aws ec2 associate-iam-instance-profile --instance-id i-0esc001 --iam-instance-profile Name=ec2-boletins", "role no servidor"), null));
            all.Add(X(W, "EC2", "aws ec2 modify-volume / enable-ebs-encryption-by-default / create-security-group", "Volumes, criptografia padrão e grupos novos.",
                "aws ec2 modify-volume --volume-id vol-... --volume-type gp3 [--size GiB]\naws ec2 enable-ebs-encryption-by-default [--region REGIAO]\naws ec2 get-ebs-encryption-by-default\naws ec2 create-security-group --group-name NOME --description \"TEXTO\" --vpc-id vpc-...",
                A("modify-volume", "Elastic Volumes: muda tipo/tamanho sem parar (tamanho só aumenta).", "enable-ebs-encryption-by-default", "Vale só para a região do comando.", "--description", "Obrigatória no create-security-group."),
                A("aws ec2 modify-volume --volume-id vol-0esc123 --volume-type gp3", "volume mais barato", "aws ec2 create-security-group --group-name portal-sg --description \"Portal da escola\" --vpc-id vpc-0escola", "grupo novo"), null));
            all.Add(X(W, "EC2", "aws ec2 create-subnet / create-route", "Sub-redes e rotas da VPC.",
                "aws ec2 describe-subnets  ·  aws ec2 describe-route-tables\naws ec2 create-subnet --vpc-id vpc-... --cidr-block CIDR --availability-zone ZONA --tag-specifications 'ResourceType=subnet,Tags=[{Key=Name,Value=NOME}]'\naws ec2 create-route --route-table-id rtb-... --destination-cidr-block 0.0.0.0/0 --nat-gateway-id nat-...   (ou --gateway-id igw-...)",
                A("InvalidSubnet.Conflict", "O bloco se sobrepõe a outra sub-rede.", "--nat-gateway-id", "Saída para a internet de sub-rede privada.", "--gateway-id igw-...", "Rota de sub-rede pública.", "replace-route", "Troca o alvo de uma rota que já existe."),
                A("aws ec2 create-subnet --vpc-id vpc-0escola --cidr-block 172.20.3.0/24 --availability-zone sa-east-1c", "sub-rede numa terceira zona", "aws ec2 create-route --route-table-id rtb-0interna --destination-cidr-block 0.0.0.0/0 --nat-gateway-id nat-0escola", "saída pela NAT"), null));
            all.Add(X(W, "Infra e monitoramento", "aws autoscaling set-desired-capacity / update-auto-scaling-group / put-scaling-policy", "Capacidade e políticas de um grupo do Auto Scaling.",
                "aws autoscaling set-desired-capacity --auto-scaling-group-name GRUPO --desired-capacity N\naws autoscaling update-auto-scaling-group --auto-scaling-group-name GRUPO --min-size N --max-size N\naws autoscaling put-scaling-policy --auto-scaling-group-name GRUPO --policy-name NOME --policy-type TargetTrackingScaling --target-tracking-configuration '{\"PredefinedMetricSpecification\":{\"PredefinedMetricType\":\"ASGAverageCPUUtilization\"},\"TargetValue\":N}'",
                A("ValidationError", "A desejada precisa ficar entre a mínima e a máxima.", "PredefinedMetricType", "ASGAverageCPUUtilization, ASGAverageNetworkIn, ASGAverageNetworkOut, ALBRequestCountPerTarget.", "TargetValue", "Valor que a política tenta manter."),
                A("aws autoscaling update-auto-scaling-group --auto-scaling-group-name asg-portal --min-size 1 --max-size 3", "limites do portal", "aws autoscaling put-scaling-policy --auto-scaling-group-name asg-portal --policy-name rede-60 --policy-type TargetTrackingScaling --target-tracking-configuration '{\"PredefinedMetricSpecification\":{\"PredefinedMetricType\":\"ASGAverageNetworkIn\"},\"TargetValue\":60000000}'", "escala pelo tráfego de rede"), null));
            all.Add(X(W, "IAM e logs", "aws iam (chaves de acesso)", "Listar, criar, ativar/desativar e excluir chaves de acesso de um usuário.",
                "aws iam list-access-keys --user-name USUARIO\naws iam create-access-key --user-name USUARIO\naws iam update-access-key --user-name USUARIO --access-key-id AKIA... --status Inactive|Active\naws iam delete-access-key --user-name USUARIO --access-key-id AKIA...",
                A("Máximo de 2", "Cada usuário tem no máximo duas chaves (LimitExceeded).", "SecretAccessKey", "Só aparece na criação.", "Inactive", "Corta o acesso sem apagar (dá para reativar)."),
                A("aws iam update-access-key --user-name robo-notas --access-key-id AKIAEXEMPLO123 --status Inactive", "desativa a chave do robô", "aws iam create-access-key --user-name robo-notas", "chave nova para a troca"), null));
            all.Add(X(W, "IAM e logs", "aws iam create-role / attach-role-policy / update-account-password-policy / delete-user", "Funções (roles), política de senha e remoção de usuários.",
                "aws iam create-role --role-name NOME --assume-role-policy-document file://confianca.json\naws iam attach-role-policy --role-name NOME --policy-arn arn:aws:iam::aws:policy/POLITICA\naws iam update-account-password-policy --minimum-password-length N --require-uppercase-characters --require-lowercase-characters --require-numbers --require-symbols\naws iam remove-user-from-group --user-name U --group-name G  ·  aws iam delete-user --user-name U",
                A("Política de confiança", "Quem pode assumir a função (Principal.Service).", "service-role/", "Algumas políticas da AWS ficam nesse caminho do ARN (ex.: AWSLambdaBasicExecutionRole).", "Opções omitidas", "No update-account-password-policy, o que não for informado volta ao padrão (desligado).", "DeleteConflict", "delete-user falha se ainda houver grupo, chaves ou políticas."),
                A("aws iam create-role --role-name ec2-boletins --assume-role-policy-document file://confia-ec2-escola.json", "role para servidores", "aws iam attach-role-policy --role-name ec2-boletins --policy-arn arn:aws:iam::aws:policy/AmazonS3ReadOnlyAccess", "só leitura no S3"), null));
            all.Add(X(W, "Serverless e containers", "aws lambda update-function-configuration / put-function-concurrency / create-event-source-mapping", "Configuração da função, limite de execuções e gatilho de fila.",
                "aws lambda get-function-configuration --function-name FUNCAO\naws lambda update-function-configuration --function-name FUNCAO [--timeout S] [--memory-size MB] [--runtime python3.12] [--environment Variables={K=V,K2=V2}]\naws lambda put-function-concurrency --function-name FUNCAO --reserved-concurrent-executions N\naws lambda create-event-source-mapping --function-name FUNCAO --event-source-arn arn:aws:sqs:REGIAO:CONTA:FILA --batch-size N",
                A("--environment", "Substitui TODAS as variáveis: mande o conjunto completo.", "--reserved-concurrent-executions", "Garante e limita a simultaneidade (0 desliga a função).", "--batch-size", "Mensagens por execução."),
                A("aws lambda update-function-configuration --function-name matricula --environment Variables={ESCOLA=centro,MODO=teste}", "duas variáveis", "aws lambda put-function-concurrency --function-name gerar-boletim --reserved-concurrent-executions 5", "no máximo 5 ao mesmo tempo"), null));
            all.Add(X(W, "Infra e monitoramento", "aws logs filter-log-events / put-retention-policy · aws cloudwatch delete-alarms", "Busca em logs, retenção e limpeza de alarmes.",
                "aws logs filter-log-events --log-group-name GRUPO --filter-pattern TERMO\naws logs put-retention-policy --log-group-name GRUPO --retention-in-days N\naws cloudwatch delete-alarms --alarm-names NOME\naws cloudwatch put-metric-alarm ... --namespace AWS/SQS --dimensions Name=QueueName,Value=FILA --alarm-actions arn:aws:sns:REGIAO:CONTA:TOPICO",
                A("--filter-pattern", "Diferencia maiúsculas.", "--retention-in-days", "1, 3, 5, 7, 14, 30, 60, 90, 120, 150, 180, 365...", "--alarm-actions", "ARN do tópico do SNS a notificar.", "Dimensões", "InstanceId (EC2), QueueName (SQS), DBInstanceIdentifier (RDS)."),
                A("aws logs filter-log-events --log-group-name /aws/lambda/matricula --filter-pattern Timeout", "execuções que estouraram o tempo", "aws logs put-retention-policy --log-group-name /aws/lambda/matricula --retention-in-days 14", "guarda só 2 semanas"), null));
            all.Add(X(W, "Mensageria", "aws sns create-topic / subscribe / publish", "Tópicos do SNS pela CLI (o tópico é identificado pelo ARN).",
                "aws sns create-topic --name NOME\naws sns subscribe --topic-arn arn:aws:sns:REGIAO:CONTA:NOME --protocol email --notification-endpoint EMAIL\naws sns list-subscriptions-by-topic --topic-arn ARN\naws sns publish --topic-arn ARN --message \"TEXTO\"",
                A("--protocol", "email, sms, https, sqs ou lambda.", "pending confirmation", "Assinatura de e-mail espera o clique no link de confirmação."),
                A("aws sns create-topic --name avisos-escola", "tópico novo", "aws sns subscribe --topic-arn arn:aws:sns:sa-east-1:111122223333:avisos-escola --protocol email --notification-endpoint secretaria@escola.edu.br", "secretaria recebe os avisos"), null));
            all.Add(X(W, "Mensageria", "aws sqs create-queue / set-queue-attributes", "Filas do SQS pela CLI (os comandos usam a URL da fila).",
                "aws sqs create-queue --queue-name NOME [--attributes FifoQueue=true,ContentBasedDeduplication=true]\naws sqs get-queue-url --queue-name NOME\naws sqs set-queue-attributes --queue-url URL --attributes VisibilityTimeout=S|ReceiveMessageWaitTimeSeconds=S|file://redrive.json\naws sqs get-queue-attributes --queue-url URL --attribute-names All",
                A("VisibilityTimeout", "0 a 43200 s.", "ReceiveMessageWaitTimeSeconds", "0 a 20 s (sondagem longa).", "RedrivePolicy", "JSON com deadLetterTargetArn e maxReceiveCount.", ".fifo", "Fila FIFO: nome termina com .fifo e FifoQueue=true."),
                A("aws sqs create-queue --queue-name notas.fifo --attributes FifoQueue=true,ContentBasedDeduplication=true", "fila FIFO de notas", "aws sqs set-queue-attributes --queue-url https://sqs.sa-east-1.amazonaws.com/111122223333/presencas --attributes VisibilityTimeout=120", "2 minutos de visibilidade"), null));
            all.Add(X(W, "Bancos de dados", "aws dynamodb scan / create-table / update-time-to-live / update-continuous-backups / update-table", "Tabelas do DynamoDB pela CLI.",
                "aws dynamodb scan --table-name TABELA\naws dynamodb create-table --table-name T --attribute-definitions AttributeName=A,AttributeType=S --key-schema AttributeName=A,KeyType=HASH --billing-mode PAY_PER_REQUEST\naws dynamodb update-time-to-live --table-name T --time-to-live-specification Enabled=true,AttributeName=ATRIBUTO\naws dynamodb update-continuous-backups --table-name T --point-in-time-recovery-specification PointInTimeRecoveryEnabled=true\naws dynamodb update-table --table-name T --deletion-protection-enabled",
                A("KeyType", "HASH = partição; RANGE = classificação.", "AttributeType", "S (string), N (número), B (binário).", "--billing-mode PAY_PER_REQUEST", "Sob demanda (sem isso, a CLI exige capacidade provisionada)."),
                A("aws dynamodb create-table --table-name alunos --attribute-definitions AttributeName=matricula,AttributeType=S --key-schema AttributeName=matricula,KeyType=HASH --billing-mode PAY_PER_REQUEST", "tabela de alunos", "aws dynamodb update-time-to-live --table-name tokens-login --time-to-live-specification Enabled=true,AttributeName=vence_em", "tokens expiram sozinhos"), null));
            all.Add(X(W, "Bancos de dados", "aws rds stop-db-instance / create-db-snapshot / modify-db-instance", "Operações do RDS pela CLI.",
                "aws rds describe-db-instances\naws rds stop-db-instance --db-instance-identifier BANCO\naws rds create-db-snapshot --db-instance-identifier BANCO --db-snapshot-identifier NOME\naws rds modify-db-instance --db-instance-identifier BANCO [--no-publicly-accessible] [--multi-az] [--deletion-protection] [--backup-retention-period N] --apply-immediately",
                A("--apply-immediately", "Sem ela, a mudança fica pendente até a janela de manutenção.", "--no-...", "Forma negativa das opções booleanas.", "PendingModifiedValues", "Mudanças que ainda vão ser aplicadas."),
                A("aws rds create-db-snapshot --db-instance-identifier banco-escola --db-snapshot-identifier antes-da-virada", "foto antes da virada de ano letivo", "aws rds modify-db-instance --db-instance-identifier banco-escola --backup-retention-period 14 --apply-immediately", "14 dias de backup"), null));
            all.Add(X(W, "Redes e DNS", "aws route53 change-resource-record-sets", "Cria, altera ou exclui registros DNS de uma zona (JSON no --change-batch).",
                "aws route53 list-hosted-zones\naws route53 list-resource-record-sets --hosted-zone-id ZONA\naws route53 change-resource-record-sets --hosted-zone-id ZONA --change-batch '{\"Changes\":[{\"Action\":\"UPSERT\",\"ResourceRecordSet\":{\"Name\":\"NOME\",\"Type\":\"A\",\"TTL\":300,\"ResourceRecords\":[{\"Value\":\"IP\"}]}}]}'",
                A("CREATE", "Cria (erro se já existir).", "UPSERT", "Cria ou substitui.", "DELETE", "Exclui: os dados precisam bater EXATAMENTE com o registro atual."),
                A("aws route53 change-resource-record-sets --hosted-zone-id Z0ESCOLA --change-batch '{\"Changes\":[{\"Action\":\"UPSERT\",\"ResourceRecordSet\":{\"Name\":\"secretaria.escola.edu.br\",\"Type\":\"A\",\"TTL\":120,\"ResourceRecords\":[{\"Value\":\"203.0.113.40\"}]}}]}'", "cria/atualiza o registro da secretaria"), null));
            all.Add(X(W, "Redes e DNS", "aws cloudfront create-invalidation", "Tira arquivos do cache da CloudFront.",
                "aws cloudfront list-distributions\naws cloudfront create-invalidation --distribution-id ID --paths /CAMINHO [/OUTRO]\naws cloudfront list-invalidations --distribution-id ID",
                A("--paths", "Cada caminho começa com /; * vale como curinga (/css/*).", "update-distribution", "Exige a configuração completa e o ETag: altere configurações pelo console."),
                A("aws cloudfront create-invalidation --distribution-id E2ESCOLA --paths /css/*", "invalida só os estilos"), null));
            all.Add(X(W, "Básico da CLI", "aws cloudtrail / kms / budgets", "Auditoria, chaves e orçamentos pela CLI.",
                "aws cloudtrail create-trail --name NOME --s3-bucket-name BUCKET --is-multi-region-trail --enable-log-file-validation\naws cloudtrail start-logging --name NOME   (trilha criada pela CLI nasce parada)\naws kms list-aliases  ·  aws kms enable-key-rotation --key-id ID_DA_CHAVE\naws budgets create-budget --account-id CONTA --budget BudgetName=NOME,BudgetLimit={Amount=N,Unit=USD},TimeUnit=MONTHLY,BudgetType=COST\naws budgets create-notification --account-id CONTA --budget-name NOME --notification NotificationType=ACTUAL,ComparisonOperator=GREATER_THAN,Threshold=N --subscribers SubscriptionType=EMAIL,Address=EMAIL",
                A("--key-id", "enable-key-rotation não aceita alias: use o ID (list-aliases mostra).", "get-trail-status", "IsLogging diz se a trilha está registrando.", "ACTUAL / FORECASTED", "Custo real / custo previsto."),
                A("aws cloudtrail create-trail --name registro-escola --s3-bucket-name logs-auditoria-escola --is-multi-region-trail", "trilha da escola", "aws budgets create-budget --account-id 111122223333 --budget BudgetName=verba-escola,BudgetLimit={Amount=120,Unit=USD},TimeUnit=MONTHLY,BudgetType=COST", "orçamento mensal da escola"), null));
            all.Add(X(W, "Serverless e containers", "aws ecr put-image-tag-mutability / put-lifecycle-policy", "Configurações de um repositório que já existe.",
                "aws ecr put-image-tag-mutability --repository-name REPO --image-tag-mutability IMMUTABLE\naws ecr put-image-scanning-configuration --repository-name REPO --image-scanning-configuration scanOnPush=true\naws ecr put-lifecycle-policy --repository-name REPO --lifecycle-policy-text '{\"rules\":[{\"rulePriority\":1,\"selection\":{\"tagStatus\":\"untagged\",\"countType\":\"sinceImagePushed\",\"countUnit\":\"days\",\"countNumber\":N},\"action\":{\"type\":\"expire\"}}]}'",
                A("tagStatus", "untagged (sem tag), tagged (com prefixos) ou any.", "countType", "sinceImagePushed (por idade) ou imageCountMoreThan (por quantidade).", "action.type", "expire (apaga)."),
                A("aws ecr put-lifecycle-policy --repository-name portal-escola --lifecycle-policy-text '{\"rules\":[{\"rulePriority\":1,\"selection\":{\"tagStatus\":\"any\",\"countType\":\"imageCountMoreThan\",\"countNumber\":30},\"action\":{\"type\":\"expire\"}}]}'", "mantém só as 30 imagens mais novas"), null));
        }
    }
}
