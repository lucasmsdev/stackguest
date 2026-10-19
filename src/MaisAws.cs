using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerQuest
{
    static partial class Challenges
    {
        // ───────────── AWS: apoio às tarefas novas (restaurar o cenário sem disparar eventos) ─────────────

        // id, nome, tipo, estado, região, AMI, IP, grupo de segurança — estado inicial das instâncias da conta simulada
        static readonly string[][] MaisAwsInst =
        {
            new[] { "i-0web001", "web-1", "t3.small", "Executando", "us-east-1", "Amazon Linux 2023", "54.210.12.8", "sg-0web111" },
            new[] { "i-0abc123", "teste-antigo", "m5.xlarge", "Executando", "us-east-1", "Ubuntu 24.04", "3.88.40.17", "sg-0default" },
            new[] { "i-0br0042", "web-br-1", "t3.small", "Executando", "sa-east-1", "Amazon Linux 2023", "18.229.5.77", "sg-0brweb3" },
            new[] { "i-0int555", "intranet", "t3.small", "Interrompida", "us-east-1", "Ubuntu 24.04", "-", "sg-0default" },
            new[] { "i-0rel777", "relatorios", "m5.4xlarge", "Executando", "us-east-1", "Amazon Linux 2023", "3.91.66.120", "sg-0default" },
        };

        // volta a instância ao estado inicial (devolve null se não for uma das instâncias conhecidas)
        static AwsSim.Ec2 MaisAwsResetInst(AwsSim s, string id)
        {
            var d = MaisAwsInst.FirstOrDefault(x => x[0] == id);
            if (d == null) return s.Inst(id);
            var i = s.Inst(id);
            if (i == null) { i = new AwsSim.Ec2 { Id = id }; s.Instances.Add(i); }
            i.Name = d[1]; i.Type = d[2]; i.State = d[3]; i.Region = d[4]; i.Ami = d[5]; i.Ip = d[6]; i.Sg = d[7];
            return i;
        }

        static string MaisAwsInstName(AwsSim s, string id) { var i = s.Inst(id); return i != null ? i.Name + " (" + id + ")" : id; }

        static void MaisAwsVol(AwsSim s, string id, int size, string type, string inst, string region)
        {
            var v = s.Vol(id);
            if (v == null) { v = new AwsSim.Volume { Id = id }; s.Volumes.Add(v); }
            v.Size = size; v.Type = type; v.Instance = inst; v.Region = region; v.State = inst != null ? "Em uso" : "Disponível";
        }

        static void MaisAwsRestoreVol(AwsSim s, string id)
        {
            if (id == "vol-0e81b37") MaisAwsVol(s, id, 500, "gp2", null, "us-east-1");
            else if (id == "vol-0c0ffee") MaisAwsVol(s, id, 20, "gp3", null, "us-east-1");
        }

        // regras de entrada iniciais de cada grupo de segurança
        static void MaisAwsResetSg(AwsSim s, string id)
        {
            var g = s.Sg(id);
            if (g == null)
            {
                g = new AwsSim.SecGroup { Id = id, Region = "us-east-1" };
                if (id == "sg-0web111") { g.Name = "web-sg"; g.Description = "Servidores web do Café 365"; }
                else if (id == "sg-0db222") { g.Name = "db-sg"; g.Description = "Banco de dados MySQL do Café 365"; }
                else { g.Name = id; g.Description = "-"; }
                s.SecGroups.Add(g);
            }
            g.Inbound.Clear();
            if (id == "sg-0web111") { g.Inbound.Add(new AwsSim.Rule { Port = 80, Source = "0.0.0.0/0" }); g.Inbound.Add(new AwsSim.Rule { Port = 443, Source = "0.0.0.0/0" }); }
            if (id == "sg-0db222") g.Inbound.Add(new AwsSim.Rule { Port = 3306, Source = "0.0.0.0/0" });
        }

        // desfaz uma alteração de regra (evento ec2:sg-add / ec2:sg-del)
        static void MaisAwsUndoRule(AwsSim s, string op, string gid, int port, string src)
        {
            var g = s.Sg(gid);
            if (g == null) return;
            if (op == "sg-add") g.Inbound.RemoveAll(r => r.Port == port && r.Source == src);
            else if (!g.Inbound.Any(r => r.Port == port && r.Source == src)) g.Inbound.Add(new AwsSim.Rule { Port = port, Source = src });
        }

        // garante o bucket com os objetos iniciais (sem disparar eventos)
        static AwsSim.Bucket MaisAwsBucket(AwsSim s, string name)
        {
            var b = s.Buckets.FirstOrDefault(x => x.Name == name);
            if (b == null)
            {
                string region = name == "micorosoft-backups" || name == "cafe365-fotos" ? "sa-east-1" : "us-east-1";
                b = new AwsSim.Bucket { Name = name, Region = region, Created = DateTime.Now.AddDays(-30) };
                s.Buckets.Add(b);
            }
            b.BlockPublic = true;
            if (name == "cafe365-logs") foreach (var o in new[] { "2026/10/01/acesso.log", "2026/10/02/acesso.log" }) if (!b.Objects.Contains(o)) b.Objects.Add(o);
            if (name == "micorosoft-backups" && !b.Objects.Contains("banco-2026-10-01.sql.gz")) b.Objects.Add("banco-2026-10-01.sql.gz");
            if (name == "cafe365-site") foreach (var o in AwsSim.SiteFiles) if (!b.Objects.Contains(o)) b.Objects.Add(o);
            return b;
        }

        static void MaisAwsTasks()
        {
            const string OK = AwsTasks.OK;
            var T = AwsTasks.All;

            // ── Iniciante
            T["d1"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; MaisAwsResetInst(s, "i-0int555"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:")) return null;
                    var p = e.Split(':');
                    if (p.Length < 3) return null;
                    if (p[1] == "start" && p[2] == "i-0int555") return OK;
                    if (p[1] == "terminate" && p[2] == "i-0int555") { MaisAwsResetInst(s, "i-0int555"); return "Você ENCERROU (apagou) a intranet! O pedido era ligá-la. A Helena restaurou do backup: use 'Estado da instância' → Iniciar."; }
                    if ((p[1] == "stop" || p[1] == "terminate") && p[2] != "i-0int555") { string n = MaisAwsInstName(s, p[2]); MaisAwsResetInst(s, p[2]); return "Você desligou " + n + ", que estava funcionando (já religamos). Só a intranet precisa ser ligada."; }
                    return null;
                },
                Solve = s => s.SetState("i-0int555", "start")
            };
            T["d2"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; MaisAwsResetInst(s, "i-0web001"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:")) return null;
                    var p = e.Split(':');
                    if (p.Length < 3) return null;
                    if (p[1] == "reboot" && p[2] == "i-0web001") return OK;
                    if ((p[1] == "stop" || p[1] == "terminate") && p[2] == "i-0web001") { MaisAwsResetInst(s, "i-0web001"); return "Você " + (p[1] == "stop" ? "INTERROMPEU" : "ENCERROU") + " o web-1: o site saiu do ar (e interromper ainda troca o IP público). Já religamos. O pedido é Reinicializar."; }
                    if (p[1] == "reboot") return "Você reinicializou " + MaisAwsInstName(s, p[2]) + ". Quem está lento é o web-1.";
                    if (p[1] == "stop" || p[1] == "terminate") { string n = MaisAwsInstName(s, p[2]); MaisAwsResetInst(s, p[2]); return "Você desligou " + n + " (já religamos). A tarefa é só reinicializar o web-1."; }
                    return null;
                },
                Solve = s => s.SetState("i-0web001", "reboot")
            };
            T["d3"] = new AwsTask
            {
                Setup = s => { var b = MaisAwsBucket(s, "cafe365-site"); if (!b.Objects.Contains("senhas.txt")) b.Objects.Add("senhas.txt"); },
                Eval = (s, e) =>
                {
                    var p = e.Split(':');
                    if (e.StartsWith("s3:delobj:"))
                    {
                        string bucket = p[2], key = e.Substring(("s3:delobj:" + bucket + ":").Length);
                        if (bucket == "cafe365-site" && key == "senhas.txt") return OK;
                        var b = s.Buckets.FirstOrDefault(x => x.Name == bucket);
                        if (b != null && !b.Objects.Contains(key)) b.Objects.Add(key);
                        return "Você apagou " + key + " do bucket " + bucket + " (já restauramos). Apague SÓ o senhas.txt do cafe365-site.";
                    }
                    if (e.StartsWith("s3:delmany:"))
                    {
                        string bucket = p[2];
                        var keys = e.Substring(("s3:delmany:" + bucket + ":").Length).Split(',');
                        var b = s.Buckets.FirstOrDefault(x => x.Name == bucket);
                        if (b != null) foreach (var k in keys) if (!b.Objects.Contains(k)) b.Objects.Add(k);
                        return "O --recursive apagou " + keys.Length + " objetos de " + bucket + ", inclusive as páginas do site! A Helena restaurou tudo (até o senhas.txt). Apague SÓ o objeto senhas.txt.";
                    }
                    if (e.StartsWith("s3:delete:"))
                    {
                        var b = MaisAwsBucket(s, p[2]);
                        if (p[2] == "cafe365-site" && !b.Objects.Contains("senhas.txt")) b.Objects.Add("senhas.txt");
                        return "Você apagou o bucket " + p[2] + " inteiro! A Helena restaurou do backup. Apague só o objeto senhas.txt dentro do cafe365-site.";
                    }
                    return null;
                },
                Solve = s => s.DeleteObject("cafe365-site", "senhas.txt")
            };
            T["d4"] = new AwsTask
            {
                Setup = s => { },
                Eval = (s, e) => e == "logs:/aws/lambda/notificacoes" ? OK : null,
                Solve = s => s.OpenLogs("/aws/lambda/notificacoes")
            };
            T["d5"] = new AwsTask
            {
                Setup = s => { s.Buckets.RemoveAll(b => b.Name == "cafe365-fotos"); s.Region = "us-east-1"; },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("s3:create:")) return null;
                    var p = e.Split(':');
                    if (p[2] != "cafe365-fotos") return "Bucket criado com o nome '" + p[2] + "'. O nome pedido é cafe365-fotos.";
                    var b = s.Buckets.First(x => x.Name == "cafe365-fotos");
                    if (p[3] == "pub") { s.Buckets.Remove(b); return "O bucket foi criado com acesso PÚBLICO! Fotos de clientes são dados pessoais. A Helena apagou: crie de novo com o acesso público bloqueado."; }
                    if (b.Region != "sa-east-1") { s.Buckets.Remove(b); return "O bucket foi criado em " + AwsSim.RegionName(b.Region) + " (" + b.Region + "). As fotos precisam ficar no Brasil: São Paulo (sa-east-1). O bucket foi apagado; crie de novo na região certa."; }
                    return OK;
                },
                Solve = s => s.CreateBucket("cafe365-fotos", "sa-east-1", true)
            };

            // ── Intermediário
            T["d6"] = new AwsTask
            {
                Setup = s => { s.EcsRunning = 2; s.EcsDeployments = 1; },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ecs:update:")) return null;
                    if (s.EcsRunning == 4) return OK;
                    if (s.EcsRunning == 0) { s.EcsRunning = 2; return "Zero tarefas = API fora do ar em plena Black Friday! Voltamos para 2. O pedido é 4 tarefas."; }
                    return "O serviço ficou com " + s.EcsRunning + " tarefa(s). O pedido é exatamente 4 (" + (s.EcsRunning > 4 ? "mais que isso é custo sem necessidade" : "menos que isso não aguenta o pico") + ").";
                },
                Solve = s => s.UpdateService(false, 4)
            };
            T["d7"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; foreach (var i in s.Instances) i.Tags.Remove("CentroDeCusto"); MaisAwsResetInst(s, "i-0web001"); MaisAwsResetInst(s, "i-0br0042"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:tag:")) return null;
                    var p = e.Split(':');
                    string id = p[2], key = p[3], val = p.Length > 4 ? p[4] : "";
                    var inst = s.Inst(id);
                    if (key == "Name") { MaisAwsResetInst(s, id); return "Você trocou a tag Name (o nome da instância)! Já desfizemos. A tag pedida é CentroDeCusto."; }
                    if (id != "i-0web001" && id != "i-0br0042") { if (inst != null) inst.Tags.Remove(key); return MaisAwsInstName(s, id) + " não é servidor web do Café 365 (os web são web-1 e web-br-1). A tag foi removida."; }
                    if (key != "CentroDeCusto") { inst.Tags.Remove(key); return "A chave ficou '" + key + "'. O relatório da Ana procura exatamente CentroDeCusto (tags diferenciam maiúsculas e minúsculas). A tag foi removida."; }
                    if (val != "cafe365") { inst.Tags.Remove(key); return "O valor ficou '" + val + "'. O pedido é cafe365. A tag foi removida."; }
                    Func<string, bool> has = x => { var i = s.Inst(x); string v; return i != null && i.Tags.TryGetValue("CentroDeCusto", out v) && v == "cafe365"; };
                    return has("i-0web001") && has("i-0br0042") ? OK : null;
                },
                Solve = s => { s.AddTag("i-0web001", "CentroDeCusto", "cafe365"); s.AddTag("i-0br0042", "CentroDeCusto", "cafe365"); }
            };
            T["d8"] = new AwsTask
            {
                Setup = s => { MaisAwsBucket(s, "cafe365-site").Versioning = "Desativado"; },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("s3:versioning:")) return null;
                    var p = e.Split(':');
                    if (p[2] != "cafe365-site") return "Você alterou o versionamento do bucket " + p[2] + ". O pedido é o cafe365-site.";
                    if (p[3] != "Enabled") return "O versionamento ficou SUSPENSO. O pedido é ATIVAR.";
                    return OK;
                },
                Solve = s => s.SetVersioning("cafe365-site", true)
            };
            T["d9"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; MaisAwsResetInst(s, "i-0web001"); MaisAwsVol(s, "vol-0f3a9e1", 8, "gp3", "i-0web001", "us-east-1"); s.Snapshots.RemoveAll(x => x.Description == "antes-da-atualizacao"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:snapshot:")) return null;
                    var p = e.Split(':');
                    string vol = p[2], desc = p.Length > 3 ? p[3] : "";
                    if (vol != "vol-0f3a9e1")
                    {
                        var v = s.Vol(vol);
                        string dono = v != null && v.Instance != null ? "do " + MaisAwsInstName(s, v.Instance) : "que não está anexado a nenhuma instância";
                        return "O volume " + vol + " é " + dono + ". O disco do web-1 é outro: confira a coluna 'Anexado a' em Volumes.";
                    }
                    if (desc != "antes-da-atualizacao") return "Snapshot criado com a descrição '" + desc + "'. O pedido é antes-da-atualizacao (crie outro com a descrição certa).";
                    return OK;
                },
                Solve = s => s.CreateSnapshot("vol-0f3a9e1", "antes-da-atualizacao")
            };

            // ── Profissional
            T["d10"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; MaisAwsResetSg(s, "sg-0web111"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:sg-")) return null;
                    var p = e.Split(':');
                    string op = p[1], gid = p[2], src = p[4];
                    int port = int.Parse(p[3]);
                    var g = s.Sg(gid);
                    string gname = g != null ? g.Name : gid;
                    MaisAwsUndoRule(s, op, gid, port, src);   // desfaz; se estiver certo, refaz abaixo
                    if (op == "sg-del")
                    {
                        if (gid == "sg-0web111" && (port == 80 || port == 443)) return "Você removeu a regra " + AwsSim.RuleName(port) + " do web-sg e o site ficou inacessível para os clientes (regra restaurada). A tarefa é só ADICIONAR o SSH.";
                        return "Você removeu uma regra do grupo " + gname + " (regra restaurada). A tarefa é só adicionar o SSH no web-sg.";
                    }
                    if (gid != "sg-0web111") return "A regra foi criada no grupo " + gname + " (e removida). O web-1 usa o web-sg (sg-0web111).";
                    if (port != 22) return "Você liberou a porta " + port + " (" + AwsSim.RuleName(port) + "). Acesso remoto a servidor Linux é SSH, porta 22. A regra foi removida.";
                    if (src == "0.0.0.0/0") return "SSH aberto para 0.0.0.0/0 = qualquer pessoa da internet pode tentar entrar (robôs testam senhas o dia todo)! A Helena removeu a regra. Libere só o IP do escritório.";
                    if (src != "200.155.80.10/32") return "A origem ficou " + src + ". Libere apenas o IP do escritório, 200.155.80.10, como um único endereço. A regra foi removida.";
                    g.Inbound.Add(new AwsSim.Rule { Port = port, Source = src });
                    return OK;
                },
                Solve = s => s.Authorize("sg-0web111", 22, "200.155.80.10/32")
            };
            T["d11"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; MaisAwsResetInst(s, "i-0rel777"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:")) return null;
                    var p = e.Split(':');
                    if (p.Length < 3) return null;
                    string act = p[1], id = p[2];
                    if (act != "stop" && act != "start" && act != "terminate" && act != "modify" && act != "reboot") return null;
                    if (id != "i-0rel777")
                    {
                        if (act == "stop" || act == "terminate" || act == "modify") { string n = MaisAwsInstName(s, id); MaisAwsResetInst(s, id); return "Você mexeu em " + n + " (já desfizemos). O servidor superdimensionado é o relatorios (i-0rel777)."; }
                        return null;
                    }
                    var inst = s.Inst(id);
                    if (act == "terminate") { MaisAwsResetInst(s, id); return "Você ENCERROU o relatorios: servidor e disco seriam perdidos! A Helena restaurou do backup. Para trocar o tamanho: interromper → alterar tipo → iniciar."; }
                    if (act == "modify")
                    {
                        string t = p[3];
                        if (t == "t3.medium") return null;
                        inst.Type = "m5.4xlarge";
                        if (t == "t3.micro" || t == "t3.small") return "O tipo " + t + " tem pouca memória: o relatório precisa de 3 GiB e travaria. Voltamos para m5.4xlarge; escolha o mais barato que atenda 2 vCPU e 3 GiB.";
                        return "O tipo " + t + " ainda é muito maior que o necessário (custo!). Voltamos para m5.4xlarge; escolha o mais barato que atenda 2 vCPU e 3 GiB.";
                    }
                    if (act == "start" && inst.Type == "t3.medium") return OK;
                    return null;
                },
                Solve = s => { s.SetState("i-0rel777", "stop"); s.ModifyType("i-0rel777", "t3.medium"); s.SetState("i-0rel777", "start"); }
            };
            T["d12"] = new AwsTask
            {
                Setup = s =>
                {
                    s.Region = "us-east-1";
                    MaisAwsVol(s, "vol-0e81b37", 500, "gp2", null, "us-east-1");
                    MaisAwsVol(s, "vol-0c0ffee", 20, "gp3", null, "us-east-1");
                    s.Snapshots.RemoveAll(x => x.Volume == "vol-0e81b37");
                },
                Eval = (s, e) =>
                {
                    var p = e.Split(':');
                    if (e.StartsWith("ec2:snapshot:"))
                    {
                        if (p[2] != "vol-0e81b37") return null;
                        string desc = p.Length > 3 ? p[3] : "";
                        if (desc != "backup-final") return "Snapshot criado com a descrição '" + desc + "'. O pedido é backup-final: crie outro com a descrição certa ANTES de excluir o volume.";
                        return null;
                    }
                    if (!e.StartsWith("ec2:delvol:")) return null;
                    if (p[2] != "vol-0e81b37")
                    {
                        MaisAwsRestoreVol(s, p[2]);
                        return "Você excluiu o volume " + p[2] + " (restaurado). O esquecido é o de 500 GiB que não está anexado a nenhuma instância.";
                    }
                    if (s.Snapshots.Any(x => x.Volume == "vol-0e81b37" && x.Description == "backup-final")) return OK;
                    MaisAwsRestoreVol(s, "vol-0e81b37");
                    return "Você excluiu o volume SEM criar o snapshot antes: se alguém precisasse daqueles dados, já era! (restaurado). Primeiro o snapshot backup-final, depois a exclusão.";
                },
                Solve = s => { s.CreateSnapshot("vol-0e81b37", "backup-final"); s.DeleteVolume("vol-0e81b37"); }
            };
            T["d13"] = new AwsTask
            {
                Setup = s => s.Users.RemoveAll(u => u.Name == "ci-deploy"),
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("iam:")) return null;
                    var p = e.Split(':');
                    if (p.Length < 3) return null;
                    string act = p[1], user = p[2];
                    var u = s.Users.FirstOrDefault(x => x.Name == user);
                    if (act == "create")
                    {
                        if (user != "ci-deploy") return "Usuário criado como '" + user + "'. O nome pedido é ci-deploy.";
                        if (p.Length > 3 && p[3].Length > 0) { s.Users.RemoveAll(x => x.Name == "ci-deploy"); return "Nenhum grupo serve para o robô: " + p[3] + (p[3] == "Leitura" ? " não deixa enviar imagens" : " dá permissão demais") + ". O usuário foi removido: crie SEM grupo e anexe a política diretamente."; }
                        return null;
                    }
                    if (act == "group" && user == "ci-deploy") { if (u != null) u.Group = null; return "O robô não deve entrar em grupo nenhum (" + p[3] + "): anexe só a política do ECR diretamente. Grupo removido."; }
                    if (act != "attach" || p.Length < 4) return null;
                    string pol = p[3];
                    if (user != "ci-deploy") { if (u != null) u.Policies.Remove(pol); return "Você anexou " + pol + " ao usuário " + user + " (removida). A permissão é para o ci-deploy."; }
                    if (pol == "AmazonEC2ContainerRegistryPowerUser") return u != null && u.Group == null && u.Policies.Count == 1 ? OK : null;
                    if (u != null) u.Policies.Remove(pol);
                    if (pol == "AdministratorAccess" || pol == "PowerUserAccess" || pol == "AmazonEC2ContainerRegistryFullAccess" || pol == "AmazonS3FullAccess")
                        return "Permissão demais: " + pol + (pol == "AmazonEC2ContainerRegistryFullAccess" ? " deixa até APAGAR repositórios" : " vai muito além do ECR") + ". Se a chave do robô vazar, o estrago é enorme. Menor privilégio: só enviar e baixar imagens. (política removida)";
                    return pol + " não permite ENVIAR (push) imagens ao ECR: o deploy falharia. (política removida)";
                },
                Solve = s => { s.CreateUser("ci-deploy", null); s.AttachPolicy("ci-deploy", "AmazonEC2ContainerRegistryPowerUser"); }
            };
            T["d14"] = new AwsTask
            {
                Setup = s => { var b = MaisAwsBucket(s, "micorosoft-backups"); b.Versioning = "Desativado"; b.Encryption = "SSE-S3"; b.KmsKey = null; },
                Eval = (s, e) =>
                {
                    var p = e.Split(':');
                    if (e == "s3:delete:micorosoft-backups") { var nb = MaisAwsBucket(s, "micorosoft-backups"); nb.Versioning = "Desativado"; return "Você apagou o bucket de backups!! A Helena restaurou de uma cópia em outra conta. Proteja o bucket, não apague."; }
                    if (e.StartsWith("s3:delobj:micorosoft-backups:") || e.StartsWith("s3:delmany:micorosoft-backups:")) { MaisAwsBucket(s, "micorosoft-backups"); return "Você apagou um backup (restaurado). A tarefa é proteger os backups, não apagar."; }
                    if (!e.StartsWith("s3:versioning:") && !e.StartsWith("s3:encryption:")) return null;
                    if (p[2] != "micorosoft-backups") return "Você alterou o bucket " + p[2] + ". O pedido é o micorosoft-backups.";
                    if (p[1] == "versioning" && p[3] != "Enabled") return "Você SUSPENDEU o versionamento. Sem versões, um ransomware que sobrescreve os backups não deixa como voltar. Ative.";
                    if (p[1] == "encryption")
                    {
                        if (p[3] == "SSE-S3") return "SSE-S3 usa chaves gerenciadas pelo próprio S3: vocês não controlam quem usa a chave. O pedido é SSE-KMS com a chave alias/backups.";
                        string k = p.Length > 4 ? p[4] : "";
                        if (k != "alias/backups") return "A chave escolhida foi " + k + (k == "alias/aws/s3" ? ", gerenciada pela AWS: vocês não controlam a política nem a rotação dela" : ", que é da aplicação: misturar chaves dá acesso aos backups a quem só deveria ver a aplicação") + ". O pedido é a chave gerenciada pelo cliente alias/backups.";
                    }
                    var b = s.Buckets.FirstOrDefault(x => x.Name == "micorosoft-backups");
                    return b != null && b.Versioning == "Ativado" && b.Encryption == "SSE-KMS" && b.KmsKey == "alias/backups" ? OK : null;
                },
                Solve = s => { s.SetVersioning("micorosoft-backups", true); s.SetEncryption("micorosoft-backups", true, "alias/backups"); }
            };
            T["d15"] = new AwsTask
            {
                Setup = s => { s.Region = "us-east-1"; MaisAwsResetSg(s, "sg-0db222"); MaisAwsResetSg(s, "sg-0web111"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:sg-")) return null;
                    var p = e.Split(':');
                    string op = p[1], gid = p[2], src = p[4];
                    int port = int.Parse(p[3]);
                    var g = s.Sg(gid);
                    if (gid != "sg-0db222") { MaisAwsUndoRule(s, op, gid, port, src); return "Você alterou o grupo " + (g != null ? g.Name : gid) + " (alteração desfeita). A correção é no db-sg (sg-0db222), o grupo do banco."; }
                    if (op == "sg-add")
                    {
                        string erro = null;
                        if (port != 3306) erro = "Você liberou a porta " + port + " (" + AwsSim.RuleName(port) + "). O MySQL usa a porta 3306.";
                        else if (src == "0.0.0.0/0") erro = "Isso é justamente o problema: banco aberto para a internet inteira!";
                        else if (!src.StartsWith("sg-")) erro = "Você liberou por endereço (" + src + "). IPs mudam quando as instâncias são recriadas ou escalam; o pedido é usar o grupo de segurança dos servidores web como origem.";
                        else if (src != "sg-0web111") erro = "A origem ficou o grupo " + (s.Sg(src) != null ? s.Sg(src).Name : src) + ". Quem acessa o banco são os servidores web: web-sg (sg-0web111).";
                        if (erro != null) { MaisAwsUndoRule(s, op, gid, port, src); return erro + " (regra removida)"; }
                    }
                    bool aberto = g.Inbound.Any(r => r.Source == "0.0.0.0/0");
                    bool api = g.Inbound.Any(r => r.Port == 3306 && r.Source == "sg-0web111");
                    return !aberto && api ? OK : null;
                },
                Solve = s => { s.Revoke("sg-0db222", 3306, "0.0.0.0/0"); s.Authorize("sg-0db222", 3306, "sg-0web111"); }
            };
        }

        static partial void MaisAws(List<Level> list)
        {
            MaisAwsTasks();

            // rebalanceamento: testar a Lambda é só abrir a função e clicar em Testar (Intermediário, não Profissional)
            var lambda = list.First(l => l.Cat == "aws" && l.Title == "Testando a Lambda");
            lambda.Tier = 1; lambda.Chapter = Cat("aws").Name + " · " + TierNames[1];

            // ───────────── Iniciante ─────────────
            list.Add(AwsL(0, "d1", "Ligando a intranet",
                "Segunda-feira, 8h: ninguém consegue abrir a intranet. O Rodrigo descobriu que a instância dela ficou desligada desde sexta.",
                "No EC2 (Norte da Virgínia), INICIE a instância intranet (i-0int555). Não mexa nas outras.",
                A("Console: EC2 → Instâncias → selecione intranet → Estado da instância → Iniciar instância", "aws ec2 start-instances --instance-ids i-0int555"),
                A("Ligar uma instância parada é uma ação do menu 'Estado da instância'.", "Selecione a linha da intranet e use Estado da instância → Iniciar (nunca Encerrar!)."),
                A("Iniciar", "Liga de novo uma instância interrompida; o disco continua o mesmo.", "IP público", "Muda a cada vez que a instância é ligada (para um IP fixo existe o IP elástico)."),
                "Antes de culpar o código, confira o óbvio: a instância está 'Executando'? Está na região certa?"));
            list.Add(AwsL(0, "d2", "Reinicializando o web",
                "Depois de uma atualização, o web-1 ficou lento. O Rodrigo pediu um reboot rápido, sem trocar o IP e sem perder nada.",
                "No EC2 (Norte da Virgínia), REINICIALIZE a instância web-1 (i-0web001). Não interrompa nem encerre.",
                A("Console: EC2 → Instâncias → selecione web-1 → Estado da instância → Reinicializar instância", "aws ec2 reboot-instances --instance-ids i-0web001"),
                A("Reiniciar é diferente de interromper: a máquina não sai do ar por muito tempo.", "Estado da instância → Reinicializar instância."),
                A("Reinicializar (reboot)", "Reinicia o sistema operacional; mantém IP público, disco e tudo mais.", "Interromper", "Desliga de verdade: libera o IP público e a máquina pode voltar em outro hardware."),
                "Reboot é o 'desligar e ligar' rápido. Se o problema voltar, investigue os logs e as métricas, não só reinicie."));
            list.Add(AwsL(0, "d3", "Arquivo indevido",
                "Alguém enviou por engano o arquivo senhas.txt para o bucket do site. Ele está exposto junto com as páginas!",
                "No S3, exclua SOMENTE o objeto senhas.txt do bucket cafe365-site. As páginas do site devem continuar lá.",
                A("Console: S3 → cafe365-site → selecione senhas.txt → Excluir", "aws s3 rm s3://cafe365-site/senhas.txt"),
                A("Abra o bucket e selecione só o objeto errado.", "No console: selecione senhas.txt → Excluir. Na CLI: aws s3 rm s3://bucket/arquivo."),
                A("Excluir objeto", "Remove um arquivo do bucket (sem versionamento, não tem volta).", "aws s3 rm", "Apaga um objeto; com --recursive apaga TUDO o que estiver no caminho. Cuidado!"),
                "Senha que vazou deve ser TROCADA, não só apagada: alguém pode ter copiado antes. E use um .gitignore/lista de exclusão no deploy."));
            list.Add(AwsL(0, "d4", "Logs da função",
                "Os clientes reclamam que não recebem o e-mail de confirmação do pedido. Quem manda esses e-mails é a função Lambda notificacoes.",
                "No CloudWatch, abra o grupo de logs da função Lambda notificacoes para ver o erro (ou acompanhe esse grupo de logs pelo CloudShell).",
                A("Console: CloudWatch → Grupos de logs → /aws/lambda/notificacoes", "aws logs tail /aws/lambda/notificacoes"),
                A("Os logs ficam no CloudWatch, em 'Grupos de logs'.", "Toda função Lambda grava no grupo /aws/lambda/<nome-da-função>."),
                A("Grupo de logs", "Conjunto de logs de uma aplicação; a Lambda cria o seu automaticamente.", "/aws/lambda/notificacoes", "Padrão de nome: /aws/lambda/ + nome da função."),
                "O log mostra o erro real (aqui, o servidor de e-mail não responde). Ler o log antes de mexer no código economiza horas."));
            list.Add(AwsL(0, "d5", "Bucket no Brasil",
                "As fotos dos clientes (dados pessoais) vão para um bucket novo. A Helena lembrou: pela política da empresa, esses dados ficam no Brasil.",
                "Crie o bucket cafe365-fotos na região América do Sul (São Paulo) — sa-east-1 — com o acesso público BLOQUEADO.",
                A("Console: S3 → Criar bucket → nome cafe365-fotos → Região América do Sul (São Paulo) sa-east-1 → mantenha o bloqueio de acesso público → Criar bucket", "aws s3 mb s3://cafe365-fotos --region sa-east-1"),
                A("A região do bucket é escolhida na criação e não muda depois.", "No formulário, troque 'Região da AWS' para São Paulo. Na CLI: --region sa-east-1."),
                A("Região do bucket", "Onde os dados ficam guardados fisicamente.", "--region sa-east-1", "Na CLI, vale só para aquele comando (sem mudar a região padrão)."),
                "Dados pessoais: pense em LGPD (onde ficam e quem acessa) desde a criação do recurso."));

            // ───────────── Intermediário ─────────────
            list.Add(AwsL(1, "d6", "Escalando a API",
                "A Black Friday começa à meia-noite e a Ana liberou orçamento: a API precisa dobrar de tamanho até lá.",
                "No ECS, altere o serviço api do cluster cafe365 para rodar 4 tarefas (quantidade desejada).",
                A("Console: ECS → cafe365 → selecione o serviço api → Atualizar serviço → Tarefas desejadas 4 → Atualizar", "aws ecs update-service --cluster cafe365 --service api --desired-count 4"),
                A("O número de cópias de um serviço é a 'quantidade desejada' (desired count).", "Atualizar serviço → Tarefas desejadas: 4. Na CLI: --desired-count 4."),
                A("Tarefas desejadas", "Quantas cópias do container o ECS mantém rodando; se uma cair, ele sobe outra.", "--desired-count 4", "Muda a quantidade sem precisar de uma nova implantação."),
                "Para não fazer isso à mão, configure Auto Scaling do serviço (ex.: manter a CPU média em 60%)."));
            list.Add(AwsL(1, "d7", "Etiquetas de custo",
                "A Ana (FinOps) quer saber quanto os servidores web custam. O relatório de custos agrupa os recursos pela tag CentroDeCusto.",
                "Adicione a tag CentroDeCusto com valor cafe365 nas DUAS instâncias web: web-1 (Norte da Virgínia) e web-br-1 (São Paulo).",
                A("Console: EC2 → selecione web-1 → Ações → Gerenciar tags → chave CentroDeCusto, valor cafe365 → Salvar; troque a região para São Paulo e repita no web-br-1", "aws ec2 create-tags --resources i-0web001 --tags Key=CentroDeCusto,Value=cafe365  +  aws ec2 create-tags --resources i-0br0042 --tags Key=CentroDeCusto,Value=cafe365 --region sa-east-1"),
                A("Tags são pares chave=valor; a chave diferencia maiúsculas e minúsculas.", "O web-br-1 só aparece com a região São Paulo selecionada (na CLI: --region sa-east-1). IDs: i-0web001 e i-0br0042."),
                A("Tag", "Etiqueta chave/valor em um recurso: dono, projeto, ambiente, centro de custo.", "create-tags --resources ... --tags Key=...,Value=...", "Marca um ou mais recursos pela CLI.", "Duas regiões", "Cada instância é regional: troque a região para achar a outra."),
                "Defina um padrão de tags para a empresa toda (e ative as 'tags de alocação de custos' no Billing). Sem tag, ninguém sabe de quem é a conta."));
            list.Add(AwsL(1, "d8", "Versionando o site",
                "Ontem alguém sobrescreveu o index.html com uma versão quebrada e não havia como voltar atrás.",
                "Ative o versionamento do bucket cafe365-site.",
                A("Console: S3 → cafe365-site → Propriedades → Editar versionamento → Ativar → Salvar alterações", "aws s3api put-bucket-versioning --bucket cafe365-site --versioning-configuration Status=Enabled"),
                A("Fica nas propriedades do bucket.", "Na CLI, configurações finas do S3 ficam no s3api: put-bucket-versioning com Status=Enabled."),
                A("Versionamento", "Cada alteração cria uma versão nova; dá para restaurar a anterior.", "s3api", "Comandos de baixo nível do S3 (configurações do bucket).", "Status=Enabled", "Ativa (Suspended suspende; depois de ativado, nunca volta a 'desativado')."),
                "Versões antigas também são cobradas: combine o versionamento com uma regra de ciclo de vida que apaga versões antigas depois de X dias."));
            list.Add(AwsL(1, "d9", "Snapshot de segurança",
                "Hoje à noite o Rodrigo vai atualizar o sistema operacional do web-1. Se der errado, precisa haver como voltar.",
                "Crie um snapshot do volume (disco EBS) anexado ao web-1, com a descrição antes-da-atualizacao.",
                A("Console: EC2 → Volumes → selecione o volume anexado ao web-1 (vol-0f3a9e1) → Ações → Criar snapshot → descrição antes-da-atualizacao → Criar snapshot", "aws ec2 create-snapshot --volume-id vol-0f3a9e1 --description antes-da-atualizacao"),
                A("O disco de uma instância é um volume EBS; descubra qual é pela coluna 'Anexado a' em Volumes.", "Volumes → volume do web-1 → Ações → Criar snapshot. Na CLI: create-snapshot --volume-id ... --description ..."),
                A("Volume EBS", "O 'HD' da instância, separado dela.", "Snapshot", "Cópia do volume num momento; dá para criar um volume novo a partir dela.", "Descrição", "Diz do que é o backup (ajuda muito na hora do aperto)."),
                "Automatize: o Amazon Data Lifecycle Manager (ou o AWS Backup) cria e apaga snapshots sozinho, por agenda."));

            // ───────────── Profissional ─────────────
            list.Add(AwsL(2, "d10", "SSH só do escritório",
                "O Rodrigo precisa acessar o web-1 por SSH. O grupo de segurança web-sg só libera HTTP e HTTPS. O IP fixo do escritório é 200.155.80.10.",
                "No grupo de segurança web-sg (sg-0web111), adicione uma regra de entrada SSH que aceite conexões SOMENTE desse único IP do escritório.",
                A("Console: EC2 → Grupos de segurança → web-sg → Adicionar regra de entrada → Tipo SSH (TCP 22) → Origem 200.155.80.10/32 → Adicionar regra", "aws ec2 authorize-security-group-ingress --group-id sg-0web111 --protocol tcp --port 22 --cidr 200.155.80.10/32"),
                A("A origem de uma regra é um bloco CIDR: o número depois da barra diz quantos bits são fixos.", "Um único IP = /32. SSH = TCP 22. Nunca 0.0.0.0/0 para SSH."),
                A("Regra de entrada", "Libera tráfego de uma ORIGEM para uma PORTA; o resto é bloqueado.", "TCP 22", "Porta do SSH.", "200.155.80.10/32", "Exatamente um endereço IP.", "0.0.0.0/0", "Qualquer endereço da internet: só para portas públicas como 80/443."),
                "Melhor ainda: dispense a porta 22 e use o AWS Systems Manager Session Manager (acesso pelo console, com log e sem chave SSH)."));
            list.Add(AwsL(2, "d11", "Servidor no tamanho certo",
                "A Ana viu que o relatorios (m5.4xlarge, US$ 0,77/hora) usa no pico 2 vCPU e 3 GiB de memória. Está pagando por 16 vCPU e 64 GiB!",
                "Mude o tipo da instância relatorios (i-0rel777) para o tipo MAIS BARATO da lista que ainda atenda 2 vCPU e 3 GiB, e deixe-a executando no final. Não encerre a instância.",
                A("Console: EC2 → relatorios → Estado da instância → Interromper → Ações → Alterar tipo de instância → t3.medium → Aplicar → Estado da instância → Iniciar", "aws ec2 stop-instances --instance-ids i-0rel777  +  aws ec2 modify-instance-attribute --instance-id i-0rel777 --instance-type Value=t3.medium  +  aws ec2 start-instances --instance-ids i-0rel777"),
                A("Compare vCPU e memória de cada tipo: o menor que cabe 3 GiB. E só dá para trocar o tipo com a instância interrompida.", "t3.small tem só 2 GiB; t3.medium tem 4 GiB. Ordem: interromper → alterar tipo → iniciar."),
                A("Rightsizing", "Ajustar o tamanho ao uso real medido (CloudWatch).", "Interromper antes", "O tipo só muda com a instância parada; o disco e os dados continuam.", "t3.medium", "2 vCPU e 4 GiB por ~US$ 0,04/hora: cerca de 19 vezes mais barato."),
                "Revise o uso todo mês (o AWS Compute Optimizer sugere tamanhos). Encerrar para 'recriar menor' perde o disco: nunca faça isso sem backup."));
            list.Add(AwsL(2, "d12", "Disco esquecido",
                "A Ana achou um volume de 500 GiB que não está anexado a nenhuma instância há 8 meses — cobrado todo mês. Ninguém sabe se os dados ainda servem.",
                "Exclua o volume de 500 GiB que está 'Disponível', mas ANTES crie um snapshot dele com a descrição backup-final. Não toque em nenhum outro volume.",
                A("Console: EC2 → Volumes → selecione o volume de 500 GiB (vol-0e81b37) → Ações → Criar snapshot (descrição backup-final) → depois Volumes → mesmo volume → Ações → Excluir volume", "aws ec2 create-snapshot --volume-id vol-0e81b37 --description backup-final  +  aws ec2 delete-volume --volume-id vol-0e81b37"),
                A("Volume 'Disponível' = não anexado. Há mais de um assim: confira o tamanho.", "Ordem: snapshot backup-final → excluir volume. Na CLI: create-snapshot e depois delete-volume."),
                A("Volume órfão", "Disco solto que continua cobrando por GiB todo mês.", "Snapshot antes de excluir", "Backup barato (incremental) caso alguém precise dos dados.", "delete-volume", "Só funciona com volume 'Disponível' (não anexado)."),
                "Faça a faxina com processo: tag de dono, aviso ao time, snapshot, exclusão — e uma regra para apagar o snapshot depois de alguns meses."));
            list.Add(AwsL(2, "d13", "Robô de deploy",
                "O pipeline do GitHub Actions vai publicar as imagens Docker no ECR sozinho. Ele precisa de uma identidade própria na conta.",
                "No IAM, crie o usuário ci-deploy SEM grupo e anexe diretamente a política gerenciada da AWS que permite enviar (push) e baixar imagens do ECR, mas NÃO apagar repositórios. Nada além disso.",
                A("Console: IAM → Criar usuário → ci-deploy → Nenhum grupo → Criar usuário → abra ci-deploy → Adicionar permissões → AmazonEC2ContainerRegistryPowerUser → Adicionar permissões", "aws iam create-user --user-name ci-deploy  +  aws iam attach-user-policy --user-name ci-deploy --policy-arn arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryPowerUser"),
                A("Compare as políticas do ECR em IAM → Políticas: ReadOnly, PowerUser e FullAccess.", "ReadOnly não envia; FullAccess apaga repositórios. Na CLI, o ARN é arn:aws:iam::aws:policy/NOME."),
                A("Política gerenciada pela AWS", "Permissões prontas, mantidas pela AWS.", "AmazonEC2ContainerRegistryPowerUser", "Push e pull de imagens, sem apagar repositórios.", "attach-user-policy --policy-arn", "Anexa a política ao usuário pelo ARN."),
                "Ainda melhor: em vez de usuário com chave fixa, use uma role com OIDC do GitHub (credenciais temporárias, nada para vazar)."));
            list.Add(AwsL(2, "d14", "Backups blindados",
                "A auditoria de segurança exige que os backups do banco resistam a ransomware e a exclusões acidentais, e que a chave de criptografia seja controlada pela empresa.",
                "No bucket micorosoft-backups, ative o versionamento E configure a criptografia padrão SSE-KMS com a chave gerenciada pelo cliente alias/backups.",
                A("Console: S3 → micorosoft-backups → Propriedades → Editar versionamento → Ativar → Salvar; Editar criptografia → SSE-KMS → chave alias/backups → Salvar alterações", "aws s3api put-bucket-versioning --bucket micorosoft-backups --versioning-configuration Status=Enabled  +  aws s3api put-bucket-encryption --bucket micorosoft-backups --server-side-encryption-configuration '{\"Rules\":[{\"ApplyServerSideEncryptionByDefault\":{\"SSEAlgorithm\":\"aws:kms\",\"KMSMasterKeyID\":\"alias/backups\"}}]}'"),
                A("São duas propriedades do bucket: versionamento e criptografia padrão.", "Criptografia: SSE-KMS (não SSE-S3) e a chave alias/backups (não a alias/aws/s3). Na CLI: SSEAlgorithm aws:kms + KMSMasterKeyID."),
                A("Versionamento", "Sobrescrever ou apagar não destrói a versão anterior.", "SSE-KMS", "Criptografia com chave do KMS: acesso à chave controlado por política e registrado no CloudTrail.", "alias/backups", "Chave gerenciada pelo cliente (a empresa decide quem usa e a rotação)."),
                "Para backups críticos, vá além: S3 Object Lock (modo compliance) e cópia para outra conta/região."));
            list.Add(AwsL(2, "d15", "Banco só para a API",
                "O scanner de segurança achou o grupo db-sg com a porta 3306 (MySQL) aberta para 0.0.0.0/0. Só os servidores web, que usam o grupo web-sg (sg-0web111), precisam falar com o banco.",
                "No grupo db-sg (sg-0db222), remova a regra que abre a 3306 para a internet e libere a 3306 SOMENTE para o grupo de segurança web-sg como origem.",
                A("Console: EC2 → Grupos de segurança → db-sg → selecione a regra 3306 de 0.0.0.0/0 → Excluir regra → Adicionar regra: MySQL/Aurora (3306), origem sg-0web111 → Adicionar regra", "aws ec2 revoke-security-group-ingress --group-id sg-0db222 --protocol tcp --port 3306 --cidr 0.0.0.0/0  +  aws ec2 authorize-security-group-ingress --group-id sg-0db222 --protocol tcp --port 3306 --source-group sg-0web111"),
                A("A origem de uma regra pode ser outro grupo de segurança, em vez de um IP.", "Revogue 3306 de 0.0.0.0/0 e autorize 3306 com origem sg-0web111 (na CLI: --source-group)."),
                A("revoke-security-group-ingress", "Remove uma regra de entrada (precisa dos mesmos dados da regra).", "Origem = grupo de segurança", "Libera qualquer instância que use aquele grupo, mesmo que o IP mude.", "Porta 3306", "MySQL/Aurora."),
                "Banco nunca fica em sub-rede pública: use sub-rede privada + grupo de segurança por referência + senha no Secrets Manager."));
        }
    }

    static partial class Docs
    {
        static partial void DocsMaisAws(List<DocEntry> all)
        {
            const string W = "AWS";

            // ── Console
            all.Add(X(W, "Console de gerenciamento", "EC2: iniciar, reinicializar, tags e tipo", "Ações do dia a dia sobre instâncias. Selecione a linha da instância antes de usar os menus.",
                "EC2 → Instâncias → selecione → Estado da instância → Iniciar / Interromper / Reinicializar / Encerrar\nEC2 → Instâncias → selecione → Ações → Gerenciar tags da instância → chave + valor → Salvar\nEC2 → Instâncias → selecione → Ações → Alterar tipo de instância (só com a instância INTERROMPIDA)",
                A("Iniciar", "Liga uma instância interrompida (o IP público muda).", "Reinicializar", "Reboot rápido: mantém IP e disco.", "Interromper", "Desliga; disco mantido, IP público liberado.", "Encerrar", "APAGA a instância.", "Tag", "Par chave/valor; a chave diferencia maiúsculas."),
                A("Reinicializar 'site-escola'", "reboot sem perder o IP", "Gerenciar tags → Chave: Turma, Valor: 3A", "etiqueta para relatórios", "Interromper → Alterar tipo para t3.large → Iniciar", "aumenta uma máquina"),
                "Recursos são regionais: para achar uma instância de outra região, troque a região no canto superior direito."));
            all.Add(X(W, "Console de gerenciamento", "Tipos de instância (tamanho e preço)", "O tipo define CPU, memória e preço por hora. Escolha o MENOR que atenda o uso medido (rightsizing).",
                "t3.micro   2 vCPU · 1 GiB  · nível gratuito\nt3.small   2 vCPU · 2 GiB  · ~US$ 0,02/h\nt3.medium  2 vCPU · 4 GiB  · ~US$ 0,04/h\nm5.xlarge  4 vCPU · 16 GiB · ~US$ 0,19/h\nm5.4xlarge 16 vCPU · 64 GiB · ~US$ 0,77/h",
                A("Família t3", "Uso geral com 'créditos' de CPU: ótima para carga baixa/variável.", "Família m5", "Uso geral com CPU constante, mais cara.", "Trocar o tipo", "Interromper → Alterar tipo → Iniciar (o disco é mantido)."),
                A("Uso medido: 1 vCPU e 1,5 GiB → t3.small (2 GiB)", "o menor que cabe", "Uso medido: 3 vCPU e 10 GiB → m5.xlarge", "precisa de mais memória"),
                "Meça antes (CloudWatch, Compute Optimizer) e reduza depois: máquina grande 'por garantia' é dinheiro jogado fora."));
            all.Add(X(W, "Console de gerenciamento", "Volumes EBS e snapshots", "Volume EBS é o disco de uma instância. Snapshot é uma cópia (backup) do volume num momento.",
                "EC2 → Volumes → coluna 'Anexado a' mostra de qual instância é cada disco\nEC2 → Volumes → selecione → Ações → Criar snapshot → descrição → Criar snapshot\nEC2 → Volumes → selecione um volume 'Disponível' → Ações → Excluir volume\nEC2 → Snapshots → lista os backups",
                A("Em uso", "Anexado a uma instância (não pode ser excluído).", "Disponível", "Solto, sem instância — mas continua cobrando por GiB.", "Snapshot", "Incremental e mais barato que o volume; dá para recriar o disco a partir dele."),
                A("Snapshot do vol-0a1b2c3 com descrição 'antes-da-prova'", "backup antes de mexer", "Excluir vol-09f8e7d (Disponível)", "faxina de custos, depois do snapshot"),
                "Antes de excluir qualquer disco, faça um snapshot. Sem ele, não tem volta."));
            all.Add(X(W, "Console de gerenciamento", "Grupos de segurança (firewall)", "Firewall virtual das instâncias. Regras de ENTRADA dizem qual origem pode acessar qual porta; o resto é bloqueado.",
                "EC2 → Grupos de segurança → clique no nome → Regras de entrada\nAdicionar: Tipo (SSH 22, HTTP 80, HTTPS 443, MySQL/Aurora 3306, PostgreSQL 5432, RDP 3389) + Origem → Adicionar regra\nExcluir: selecione a regra → Excluir regra",
                A("Origem CIDR x.x.x.x/32", "Um único IP.", "Origem x.x.x.0/24", "256 endereços (uma rede pequena).", "Origem 0.0.0.0/0", "Qualquer lugar da internet: só para portas públicas (80/443).", "Origem sg-...", "Qualquer instância que use aquele outro grupo de segurança (não depende de IP)."),
                A("Grupo 'escola-sg': RDP 3389 de 198.51.100.7/32", "acesso remoto só da secretaria", "Grupo 'notas-db-sg': PostgreSQL 5432 de sg-0app999", "banco aceita só os servidores da aplicação"),
                "Nunca abra SSH (22), RDP (3389) ou portas de banco para 0.0.0.0/0: robôs varrem a internet o tempo todo."));
            all.Add(X(W, "Console de gerenciamento", "S3: versionamento e criptografia padrão", "Propriedades de proteção do bucket.",
                "S3 → bucket → Propriedades → Versionamento de bucket → Editar → Ativar → Salvar alterações\nS3 → bucket → Propriedades → Criptografia padrão → Editar → SSE-S3 ou SSE-KMS (+ chave) → Salvar alterações\nS3 → bucket → selecione um objeto → Excluir",
                A("Versionamento", "Guarda versões antigas; sobrescrever/apagar não destrói o dado.", "SSE-S3", "Criptografia com chave gerenciada pelo próprio S3 (padrão).", "SSE-KMS", "Criptografia com chave do AWS KMS: controle de acesso à chave e registro de uso.", "alias/aws/s3", "Chave do KMS gerenciada pela AWS.", "alias/<nome>", "Chave gerenciada pelo cliente (a empresa controla)."),
                A("Bucket 'boletins-escola' → Ativar versionamento", "protege contra sobrescrita", "Criptografia → SSE-KMS → alias/notas-escola", "dados sensíveis com chave própria"),
                "Versionamento + SSE-KMS com chave própria + bloqueio de acesso público = o básico para dados importantes."));
            all.Add(X(W, "Console de gerenciamento", "IAM: políticas anexadas ao usuário", "Além de grupos, um usuário pode receber políticas diretamente. Escolha sempre a MENOR que resolve.",
                "IAM → Políticas → lista as políticas gerenciadas pela AWS\nIAM → Usuários → (usuário) → Adicionar permissões → escolha a política → Adicionar permissões\nIAM → Usuários → (usuário) → selecione a política → Remover política",
                A("AdministratorAccess", "Tudo.", "PowerUserAccess", "Tudo menos IAM.", "ReadOnlyAccess", "Só leitura em tudo.", "AmazonS3ReadOnlyAccess / AmazonS3FullAccess", "S3: só ler / tudo (inclusive apagar).", "AmazonEC2ContainerRegistryReadOnly", "ECR: só baixar imagens (pull).", "AmazonEC2ContainerRegistryPowerUser", "ECR: enviar e baixar imagens, sem apagar repositórios.", "AmazonEC2ContainerRegistryFullAccess", "ECR: tudo, inclusive apagar repositórios.", "CloudWatchReadOnlyAccess", "Só ler métricas, alarmes e logs."),
                A("Usuário 'boletim-bot' + AmazonS3ReadOnlyAccess", "robô que só lê arquivos do S3"),
                "Robôs (CI/CD, scripts) recebem só o que usam. Se a chave vazar, o estrago fica limitado."));
            all.Add(X(W, "Console de gerenciamento", "CloudWatch: grupos de logs", "Logs das aplicações, organizados em grupos. Cada função Lambda grava num grupo com nome padrão.",
                "CloudWatch → Grupos de logs → clique no grupo → eventos de log\nLambda: /aws/lambda/<nome-da-função>\nECS: /ecs/<nome-do-serviço> (padrão comum)",
                A("Grupo de logs", "Conjunto de logs de uma aplicação.", "Retenção", "Por quantos dias os logs são guardados (e cobrados)."),
                A("/aws/lambda/matricula", "logs da função matricula"), "Procure por [ERROR], Exception e Traceback nos eventos."));

            // ── CLI
            all.Add(X(W, "EC2", "aws ec2 reboot-instances / --region", "Reinicializa instâncias. A opção global --region executa o comando em outra região sem mudar a padrão.",
                "aws ec2 reboot-instances --instance-ids ID\naws ec2 start-instances --instance-ids ID --region REGIAO",
                A("--region", "Vale para qualquer comando (ex.: sa-east-1, us-east-1).", "reboot-instances", "Reinicia sem trocar IP."),
                A("aws ec2 reboot-instances --instance-ids i-0esc001", "reinicia a instância", "aws ec2 describe-instances --region eu-west-1", "lista as instâncias da Irlanda"), null));
            all.Add(X(W, "EC2", "aws ec2 create-tags", "Adiciona (ou altera) tags em recursos.",
                "aws ec2 create-tags --resources ID --tags Key=CHAVE,Value=VALOR",
                A("--resources", "ID do recurso (instância, volume...).", "--tags", "Key=...,Value=... (a chave diferencia maiúsculas)."),
                A("aws ec2 create-tags --resources i-0esc001 --tags Key=Turma,Value=3A", "etiqueta a instância", "aws ec2 create-tags --resources i-0esc002 --tags Key=Turma,Value=3A --region eu-west-1", "instância em outra região"), null));
            all.Add(X(W, "EC2", "aws ec2 modify-instance-attribute", "Altera atributos de uma instância, como o tipo. A instância precisa estar interrompida.",
                "aws ec2 stop-instances --instance-ids ID\naws ec2 modify-instance-attribute --instance-id ID --instance-type Value=TIPO\naws ec2 start-instances --instance-ids ID",
                A("--instance-type Value=TIPO", "Novo tipo (também aceita JSON: '{\"Value\": \"TIPO\"}')."),
                A("aws ec2 modify-instance-attribute --instance-id i-0esc001 --instance-type Value=t3.large", "muda o tipo (com a instância parada)"),
                "Se a instância estiver executando, o erro é IncorrectInstanceState."));
            all.Add(X(W, "EC2", "aws ec2 describe-volumes / create-snapshot / delete-volume", "Discos EBS e seus backups.",
                "aws ec2 describe-volumes\naws ec2 create-snapshot --volume-id VOL --description TEXTO\naws ec2 describe-snapshots\naws ec2 delete-volume --volume-id VOL",
                A("Attachments", "Em describe-volumes, mostra a instância do volume.", "State", "in-use (anexado) ou available (solto).", "--description", "Texto do snapshot."),
                A("aws ec2 create-snapshot --volume-id vol-0a1b2c3 --description antes-da-prova", "backup do disco", "aws ec2 delete-volume --volume-id vol-09f8e7d", "apaga um volume solto"),
                "delete-volume só funciona com volume 'available' — e não tem volta."));
            all.Add(X(W, "EC2", "aws ec2 authorize / revoke-security-group-ingress", "Adiciona ou remove regras de entrada de um grupo de segurança.",
                "aws ec2 authorize-security-group-ingress --group-id SG --protocol tcp --port PORTA --cidr ORIGEM/BITS\naws ec2 authorize-security-group-ingress --group-id SG --protocol tcp --port PORTA --source-group SG_ORIGEM\naws ec2 revoke-security-group-ingress --group-id SG --protocol tcp --port PORTA --cidr ORIGEM/BITS\naws ec2 describe-security-groups",
                A("--cidr", "Origem por endereço (x.x.x.x/32 = um IP).", "--source-group", "Origem = outro grupo de segurança.", "revoke", "Remove: informe exatamente os mesmos dados da regra."),
                A("aws ec2 authorize-security-group-ingress --group-id sg-0escola1 --protocol tcp --port 3389 --cidr 198.51.100.7/32", "RDP só para um IP", "aws ec2 authorize-security-group-ingress --group-id sg-0notas2 --protocol tcp --port 5432 --source-group sg-0app999", "banco só para a aplicação", "aws ec2 revoke-security-group-ingress --group-id sg-0escola1 --protocol tcp --port 80 --cidr 0.0.0.0/0", "fecha o HTTP"), null));
            all.Add(X(W, "S3", "aws s3 rm / aws s3 mb --region", "Apaga objetos e cria buckets em uma região específica.",
                "aws s3 rm s3://BUCKET/ARQUIVO\naws s3 rm s3://BUCKET/PASTA --recursive\naws s3 mb s3://BUCKET --region REGIAO",
                A("--recursive", "(rm) apaga tudo o que estiver no caminho. Muito cuidado!", "--region", "(mb) região onde o bucket será criado."),
                A("aws s3 rm s3://fotos-turma/rascunho.txt", "apaga um objeto", "aws s3 mb s3://notas-escola --region eu-west-1", "cria o bucket na Irlanda"), null));
            all.Add(X(W, "S3", "aws s3api put-bucket-versioning", "Ativa ou suspende o versionamento de um bucket.",
                "aws s3api put-bucket-versioning --bucket BUCKET --versioning-configuration Status=Enabled\naws s3api get-bucket-versioning --bucket BUCKET",
                A("Status=Enabled", "Ativa.", "Status=Suspended", "Suspende (as versões antigas continuam)."),
                A("aws s3api put-bucket-versioning --bucket boletins-escola --versioning-configuration Status=Enabled", "ativa no bucket boletins-escola"), null));
            all.Add(X(W, "S3", "aws s3api put-bucket-encryption", "Define a criptografia padrão do bucket (SSE-S3 ou SSE-KMS).",
                "aws s3api put-bucket-encryption --bucket BUCKET --server-side-encryption-configuration 'JSON'\naws s3api get-bucket-encryption --bucket BUCKET",
                A("SSEAlgorithm AES256", "SSE-S3 (chave do S3).", "SSEAlgorithm aws:kms", "SSE-KMS (chave do KMS).", "KMSMasterKeyID", "Alias (alias/nome) ou ARN da chave KMS."),
                A("aws s3api put-bucket-encryption --bucket boletins-escola --server-side-encryption-configuration '{\"Rules\":[{\"ApplyServerSideEncryptionByDefault\":{\"SSEAlgorithm\":\"aws:kms\",\"KMSMasterKeyID\":\"alias/notas-escola\"}}]}'", "SSE-KMS com a chave alias/notas-escola"),
                "Use aspas simples em volta do JSON para o shell não desmontar as aspas duplas."));
            all.Add(X(W, "Básico da CLI", "aws iam attach-user-policy", "Anexa (ou remove) uma política gerenciada diretamente a um usuário.",
                "aws iam list-policies\naws iam attach-user-policy --user-name NOME --policy-arn arn:aws:iam::aws:policy/POLITICA\naws iam list-attached-user-policies --user-name NOME\naws iam detach-user-policy --user-name NOME --policy-arn ARN",
                A("--policy-arn", "ARN completo; políticas da AWS começam com arn:aws:iam::aws:policy/."),
                A("aws iam attach-user-policy --user-name boletim-bot --policy-arn arn:aws:iam::aws:policy/AmazonS3ReadOnlyAccess", "robô só lê o S3"), null));
            all.Add(X(W, "Serverless e containers", "aws ecs update-service --desired-count", "Muda quantas tarefas (cópias) um serviço ECS mantém rodando.",
                "aws ecs update-service --cluster CLUSTER --service SERVICO --desired-count N\nConsole: ECS → cluster → selecione o serviço → Atualizar serviço → Tarefas desejadas → Atualizar",
                A("--desired-count", "Quantidade de tarefas; 0 tira o serviço do ar."),
                A("aws ecs update-service --cluster escola --service matriculas --desired-count 3", "3 cópias do serviço matriculas"), null));
            all.Add(X(W, "IAM e logs", "aws logs tail (Lambda)", "Mostra os eventos de um grupo de logs. Funções Lambda gravam em /aws/lambda/<função>.",
                "aws logs describe-log-groups\naws logs tail /aws/lambda/FUNCAO [--follow]", null,
                A("aws logs tail /aws/lambda/matricula", "logs da função matricula"), null));
        }
    }
}
