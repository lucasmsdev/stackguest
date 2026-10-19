using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DockerQuest
{
    // Conta AWS simulada usada pelo console e pelo CloudShell da área de AWS.
    // Toda ação (no console ou na CLI) muda o estado e gera um evento; as tarefas dos desafios avaliam esses eventos.
    class AwsSim
    {
        public class Bucket
        {
            public string Name, Region; public bool BlockPublic = true; public List<string> Objects = new List<string>(); public DateTime Created = DateTime.Now; public string Versioning = "Desativado", Encryption = "SSE-S3", KmsKey;
            public Dictionary<string, string> Tags = new Dictionary<string, string>();
            public string IndexDoc, ErrorDoc, Policy, PolicyText;            // site estático; política do bucket (tipo + JSON)
            public List<LcRule> Lifecycle = new List<LcRule>();
            public List<Marker> Markers = new List<Marker>();               // marcadores de exclusão (bucket versionado)
        }
        public class Ec2 { public string Id, Name, Type, State, Region, Ami, Ip, Sg = "sg-0default"; public Dictionary<string, string> Tags = new Dictionary<string, string>(); public bool Protect; public string Imds = "optional", Role, Key = "cafe-key"; }
        public class User { public string Name, Group; public bool Mfa; public DateTime Created = DateTime.Now.AddDays(-30); public List<string> Policies = new List<string>(); }
        public class Volume { public string Id, Type, State, Instance, Region; public int Size; }
        public class Snapshot { public string Id, Volume, Description, Region; public int Size; public DateTime Created = DateTime.Now; }
        public class Rule { public string Proto = "tcp", Source; public int Port; }
        public class SecGroup { public string Id, Name, Description, Region, Vpc; public List<Rule> Inbound = new List<Rule>(); }
        public class Fn { public string Name, Runtime; public string LastResult; public int Timeout = 3, Memory = 128, Concurrency = -1; public Dictionary<string, string> Env = new Dictionary<string, string>(); public List<string[]> Triggers = new List<string[]>(); }
        public class Alarm { public string Name, Metric, Resource, State; public float Threshold; public string Namespace = "AWS/EC2", Comparison = "GreaterThanThreshold", Action; }
        public class Repo { public string Name; public bool ScanOnPush, Immutable; public string LcTag, LcType; public int LcN; }
        public class Stack { public string Name, Status, Template, Reason; }
        // ── recursos da 2ª leva de tarefas
        public class LcRule { public string Id, Prefix = ""; public int Expire, Noncurrent, Glacier; }
        public class Marker { public string Key, VersionId; }
        public class Eip { public string Alloc, Ip, Instance, Region; }
        public class KeyPair { public string Name, Type, Region; public DateTime Created = DateTime.Now.AddDays(-120); }
        public class Image { public string Id, Name, Instance, Region; public DateTime Created = DateTime.Now.AddDays(-5); }
        public class Asg { public string Name; public int Min, Max, Desired; public List<string[]> Policies = new List<string[]>(); }   // nome, tipo, métrica, alvo
        public class Vpc { public string Id, Name, Cidr, Region; }
        public class Subnet { public string Id, Name, Vpc, Cidr, Az, Rtb; }
        public class RouteTable { public string Id, Name, Vpc; public List<string[]> Routes = new List<string[]>(); }   // destino, alvo
        public class Role { public string Name, Trust; public List<string> Policies = new List<string>(); }
        public class AccessKey { public string Id, User, Status; public DateTime Created; }
        public class Topic { public string Name; public bool Fifo; public List<string[]> Subs = new List<string[]>(); }   // protocolo, endpoint, status
        public class Queue { public string Name, Dlq; public bool Fifo, Dedup; public int Visibility = 30, Wait = 0, MaxReceive; }
        public class Table { public string Name, Pk, PkType, Sk, SkType, Billing = "PAY_PER_REQUEST", Ttl; public bool Pitr, Protect; public List<string> Items = new List<string>(); }
        public class Db { public string Id, Engine, Class, Status; public bool Public, MultiAz, Protect; public int Backup = 1; public Dictionary<string, string> Pending = new Dictionary<string, string>(); }
        public class DbSnap { public string Id, Db, Type; public DateTime Created = DateTime.Now; }
        public class DnsRecord { public string Name, Type, Value; public int Ttl; }
        public class Dist { public string Id, Domain, Origin, Viewer = "allow-all", Root = ""; public List<string> Invalidations = new List<string>(); }
        public class Trail { public string Name, Bucket; public bool MultiRegion, Logging, Validation; }
        public class Budget { public string Name; public float Amount; public List<string[]> Alerts = new List<string[]>(); }   // tipo, limite %, e-mail

        public const string AccountId = "1234-5678-9012";
        public static readonly string[][] Regions =
        {
            new[] { "us-east-1", "Leste dos EUA (Norte da Virgínia)" },
            new[] { "us-east-2", "Leste dos EUA (Ohio)" },
            new[] { "sa-east-1", "América do Sul (São Paulo)" },
            new[] { "eu-west-1", "Europa (Irlanda)" },
        };
        public static string RegionName(string r) { var x = Regions.FirstOrDefault(a => a[0] == r); return x == null ? r : x[1]; }

        public static readonly string[] Groups = { "Administradores", "Desenvolvedores", "Leitura" };
        public static readonly string[] SiteFiles = { "index.html", "style.css", "app.js", "logo.png" };
        public static readonly string[] LocalFiles = { "index.html", "style.css", "app.js", "logo.png", "foto-do-gato.jpg", "senhas.txt" };
        // tipo, descrição (vCPU, memória, preço) — usados no assistente de execução e em "Alterar tipo de instância"
        public static readonly string[][] InstanceTypes =
        {
            new[] { "t3.micro", "2 vCPU · 1 GiB · qualificado para o nível gratuito" },
            new[] { "t3.small", "2 vCPU · 2 GiB · US$ 0,02/hora" },
            new[] { "t3.medium", "2 vCPU · 4 GiB · US$ 0,04/hora" },
            new[] { "m5.xlarge", "4 vCPU · 16 GiB · US$ 0,19/hora" },
            new[] { "m5.4xlarge", "16 vCPU · 64 GiB · US$ 0,77/hora" },
        };
        // políticas gerenciadas pela AWS disponíveis para anexar diretamente a usuários
        public static readonly string[][] ManagedPolicies =
        {
            new[] { "AdministratorAccess", "Acesso total a todos os serviços e recursos" },
            new[] { "PowerUserAccess", "Tudo, menos gerenciar usuários e permissões (IAM)" },
            new[] { "ReadOnlyAccess", "Somente leitura em todos os serviços" },
            new[] { "AmazonS3ReadOnlyAccess", "Somente leitura no S3 (listar e baixar objetos)" },
            new[] { "AmazonS3FullAccess", "Acesso total ao S3, inclusive apagar buckets" },
            new[] { "AmazonEC2ContainerRegistryReadOnly", "Somente baixar (pull) imagens do ECR" },
            new[] { "AmazonEC2ContainerRegistryPowerUser", "Enviar (push) e baixar (pull) imagens do ECR, sem apagar repositórios" },
            new[] { "AmazonEC2ContainerRegistryFullAccess", "Acesso total ao ECR, inclusive criar e apagar repositórios" },
            new[] { "CloudWatchReadOnlyAccess", "Somente leitura em métricas, alarmes e logs" },
            new[] { "AmazonDynamoDBReadOnlyAccess", "Somente leitura nas tabelas do DynamoDB" },
            new[] { "AWSLambdaBasicExecutionRole", "Gravar logs no CloudWatch Logs (básico de toda função Lambda)", "service-role/" },
        };
        // ARN completo de uma política gerenciada (algumas ficam no caminho service-role/)
        public static string PolicyArn(string name)
        {
            var p = ManagedPolicies.FirstOrDefault(x => x[0] == name);
            return "arn:aws:iam::aws:policy/" + (p != null && p.Length > 2 ? p[2] : "") + name;
        }
        // nome da política a partir do ARN (null se não for um ARN de política gerenciada)
        public static string PolicyFromArn(string arn)
        {
            const string pre = "arn:aws:iam::aws:policy/";
            if (arn == null || !arn.StartsWith(pre)) return null;
            string n = arn.Substring(pre.Length);
            int sl = n.LastIndexOf('/');
            return sl >= 0 ? n.Substring(sl + 1) : n;
        }
        // ID de cada chave do KMS (as operações de rotação exigem o ID ou o ARN, não o alias)
        public static readonly string[][] KmsIds =
        {
            new[] { "alias/aws/s3", "0d6e3c2a-91f4-4b7e-8c55-aws000000s3a" },
            new[] { "alias/backups", "7c1e5a90-2b4d-4f8e-9a31-b4c4c0ffee01" },
            new[] { "alias/cafe365-app", "3f9d2b17-8e6a-4c02-b5d4-a1cafe365a99" },
        };
        // AMIs públicas da AWS (id, nome)
        public static readonly string[][] BaseAmis =
        {
            new[] { "ami-0al2023a1", "Amazon Linux 2023" },
            new[] { "ami-0ub2404b2", "Ubuntu 24.04" },
            new[] { "ami-0ws2022c3", "Windows Server 2022" },
        };
        public static readonly string[] VolumeTypes = { "gp3", "gp2", "io2", "st1", "sc1" };
        public static readonly int[] RetentionDays = { 1, 3, 5, 7, 14, 30, 60, 90, 120, 150, 180, 365, 400, 545, 731, 1096, 1827, 2192, 2557, 2922, 3288, 3653 };
        public static string RetentionText(int d) { return d <= 0 ? "Nunca expirar" : d == 30 ? "1 mês (30 dias)" : d == 365 ? "1 ano (365 dias)" : d + " dias"; }
        public static readonly string[][] Runtimes =
        {
            new[] { "python3.8", "Python 3.8" }, new[] { "python3.12", "Python 3.12" }, new[] { "python3.13", "Python 3.13" },
            new[] { "nodejs18.x", "Node.js 18.x" }, new[] { "nodejs20.x", "Node.js 20.x" }, new[] { "nodejs22.x", "Node.js 22.x" },
        };
        public static string RuntimeId(string display) { var r = Runtimes.FirstOrDefault(x => x[1] == display); return r != null ? r[0] : display; }
        public const string ZoneId = "Z0CAFE365BR", Zone = "cafe365.com.br", Arn = "123456789012";
        // métricas do CloudWatch por namespace
        public static readonly string[][] Metrics =
        {
            new[] { "AWS/EC2", "CPUUtilization", "NetworkIn", "DiskReadOps", "StatusCheckFailed" },
            new[] { "AWS/SQS", "ApproximateNumberOfMessagesVisible", "ApproximateAgeOfOldestMessage", "NumberOfMessagesSent" },
            new[] { "AWS/RDS", "CPUUtilization", "FreeStorageSpace", "DatabaseConnections" },
        };
        // arquivos JSON da pasta do CloudShell (usados com file://)
        public static readonly string[][] LocalJson =
        {
            new[] { "confianca-ec2.json", "{\"Version\":\"2012-10-17\",\"Statement\":[{\"Effect\":\"Allow\",\"Principal\":{\"Service\":\"ec2.amazonaws.com\"},\"Action\":\"sts:AssumeRole\"}]}" },
            new[] { "confianca-lambda.json", "{\"Version\":\"2012-10-17\",\"Statement\":[{\"Effect\":\"Allow\",\"Principal\":{\"Service\":\"lambda.amazonaws.com\"},\"Action\":\"sts:AssumeRole\"}]}" },
            new[] { "politica-somente-https.json", "{\"Version\":\"2012-10-17\",\"Statement\":[{\"Sid\":\"SomenteHTTPS\",\"Effect\":\"Deny\",\"Principal\":\"*\",\"Action\":\"s3:*\",\"Resource\":[\"arn:aws:s3:::micorosoft-backups\",\"arn:aws:s3:::micorosoft-backups/*\"],\"Condition\":{\"Bool\":{\"aws:SecureTransport\":\"false\"}}}]}" },
            new[] { "politica-cloudfront-site.json", "{\"Version\":\"2012-10-17\",\"Statement\":[{\"Sid\":\"SomenteCloudFront\",\"Effect\":\"Allow\",\"Principal\":{\"Service\":\"cloudfront.amazonaws.com\"},\"Action\":\"s3:GetObject\",\"Resource\":\"arn:aws:s3:::cafe365-site/*\",\"Condition\":{\"StringEquals\":{\"AWS:SourceArn\":\"arn:aws:cloudfront::123456789012:distribution/E1CAFE365SITE\"}}}]}" },
            new[] { "politica-leitura-publica.json", "{\"Version\":\"2012-10-17\",\"Statement\":[{\"Sid\":\"LeituraPublica\",\"Effect\":\"Allow\",\"Principal\":\"*\",\"Action\":\"s3:GetObject\",\"Resource\":\"arn:aws:s3:::cafe365-site/*\"}]}" },
            new[] { "redrive-pedidos.json", "{\"RedrivePolicy\":\"{\\\"deadLetterTargetArn\\\":\\\"arn:aws:sqs:us-east-1:123456789012:pedidos-dlq\\\",\\\"maxReceiveCount\\\":\\\"5\\\"}\"}" },
        };
        public static string JsonFile(string name) { name = (name ?? "").Replace("file://", "").TrimStart('.', '/'); var f = LocalJson.FirstOrDefault(x => x[0] == name); return f == null ? null : f[1]; }
        public static readonly string[][] KmsKeys =
        {
            new[] { "alias/aws/s3", "Chave gerenciada pela AWS (padrão do S3)" },
            new[] { "alias/backups", "Chave gerenciada pelo cliente: backups" },
            new[] { "alias/cafe365-app", "Chave gerenciada pelo cliente: aplicação" },
        };
        // tipos de regra de entrada (nome, porta TCP)
        public static readonly string[][] RuleTypes =
        {
            new[] { "SSH", "22" }, new[] { "HTTP", "80" }, new[] { "HTTPS", "443" }, new[] { "MySQL/Aurora", "3306" }, new[] { "PostgreSQL", "5432" }, new[] { "RDP", "3389" },
        };
        public static string RuleName(int port) { var r = RuleTypes.FirstOrDefault(x => x[1] == port.ToString()); return r == null ? "TCP personalizado" : r[0]; }
        public static readonly string[] LogGroups = { "/aws/lambda/pedidos", "/aws/lambda/notificacoes", "/ecs/cafe365-api" };

        // linhas de log de cada grupo (sem o horário)
        public static string[] LogLines(string group)
        {
            if (group.Contains("pedidos")) return new[] { "START RequestId: 7f3a1c", "[ERROR] KeyError: 'quantidade' (pedido sem campo quantidade)", "END RequestId: 7f3a1c", "START RequestId: 9b02ee", "Pedido 4820 registrado", "END RequestId: 9b02ee" };
            if (group.Contains("notificacoes")) return new[] { "START RequestId: 51c0d2", "Enviando confirmação do pedido 4821 para cliente@exemplo.com", "[ERROR] Falha ao enviar e-mail: tempo esgotado ao conectar em smtp.cafe365.com:587", "END RequestId: 51c0d2" };
            return new[] { "INFO  serviço iniciado", "INFO  GET /health 200" };
        }

        public string Region = "us-east-1";
        public readonly List<Bucket> Buckets = new List<Bucket>();
        public readonly List<Ec2> Instances = new List<Ec2>();
        public readonly List<User> Users = new List<User>();
        public readonly List<Fn> Functions = new List<Fn>();
        public readonly List<Alarm> Alarms = new List<Alarm>();
        public readonly List<Repo> Repos = new List<Repo>();
        public readonly List<Stack> Stacks = new List<Stack>();
        public readonly List<Volume> Volumes = new List<Volume>();
        public readonly List<Snapshot> Snapshots = new List<Snapshot>();
        public readonly List<SecGroup> SecGroups = new List<SecGroup>();
        public int EcsDeployments = 1, EcsRunning = 2;
        int nextId = 0x5f1;
        // ── 2ª leva
        public readonly List<Eip> Eips = new List<Eip>();
        public readonly List<KeyPair> KeyPairs = new List<KeyPair>();
        public readonly List<Image> Images = new List<Image>();
        public readonly List<Asg> Asgs = new List<Asg>();
        public readonly List<Vpc> Vpcs = new List<Vpc>();
        public readonly List<Subnet> Subnets = new List<Subnet>();
        public readonly List<RouteTable> RouteTables = new List<RouteTable>();
        public readonly List<Role> Roles = new List<Role>();
        public readonly List<AccessKey> AccessKeys = new List<AccessKey>();
        public readonly List<Topic> Topics = new List<Topic>();
        public readonly List<Queue> Queues = new List<Queue>();
        public readonly List<Table> Tables = new List<Table>();
        public readonly List<Db> Dbs = new List<Db>();
        public readonly List<DbSnap> DbSnaps = new List<DbSnap>();
        public readonly List<DnsRecord> Records = new List<DnsRecord>();
        public readonly List<Dist> Dists = new List<Dist>();
        public readonly List<Trail> Trails = new List<Trail>();
        public readonly List<Budget> Budgets = new List<Budget>();
        public readonly Dictionary<string, int> LogRetention = new Dictionary<string, int>();
        public readonly Dictionary<string, bool> EbsDefault = new Dictionary<string, bool>();
        public readonly Dictionary<string, bool> KeyRotation = new Dictionary<string, bool>();
        public int PwMin = 8, PwMaxAge = 0; public bool PwUpper, PwLower, PwNumbers, PwSymbols;
        int seq;   // contador determinístico de IDs novos (zerado no Reset)

        public event Action<string> Event;     // evento de uma ação (para as tarefas)
        public event Action Changed;           // estado mudou (para redesenhar o console)

        public AwsSim() { Reset(); }

        public void Reset()
        {
            Region = "us-east-1";
            Buckets.Clear(); Instances.Clear(); Users.Clear(); Functions.Clear(); Alarms.Clear(); Repos.Clear(); Stacks.Clear();
            Volumes.Clear(); Snapshots.Clear(); SecGroups.Clear();
            Buckets.Add(new Bucket { Name = "cafe365-logs", Region = "us-east-1", Objects = { "2026/10/01/acesso.log", "2026/10/02/acesso.log" }, Created = DateTime.Now.AddDays(-90) });
            Buckets.Add(new Bucket { Name = "micorosoft-backups", Region = "sa-east-1", Objects = { "banco-2026-10-01.sql.gz" }, Created = DateTime.Now.AddDays(-200) });
            Instances.Add(new Ec2 { Id = "i-0web001", Name = "web-1", Type = "t3.small", State = "Executando", Region = "us-east-1", Ami = "Amazon Linux 2023", Ip = "54.210.12.8" });
            Instances.Add(new Ec2 { Id = "i-0abc123", Name = "teste-antigo", Type = "m5.xlarge", State = "Executando", Region = "us-east-1", Ami = "Ubuntu 24.04", Ip = "3.88.40.17" });
            Instances.Add(new Ec2 { Id = "i-0br0042", Name = "web-br-1", Type = "t3.small", State = "Executando", Region = "sa-east-1", Ami = "Amazon Linux 2023", Ip = "18.229.5.77", Sg = "sg-0brweb3" });
            Instances.Add(new Ec2 { Id = "i-0int555", Name = "intranet", Type = "t3.small", State = "Interrompida", Region = "us-east-1", Ami = "Ubuntu 24.04", Ip = "-" });
            Instances.Add(new Ec2 { Id = "i-0rel777", Name = "relatorios", Type = "m5.4xlarge", State = "Executando", Region = "us-east-1", Ami = "Amazon Linux 2023", Ip = "3.91.66.120" });
            Instances[0].Sg = "sg-0web111";
            Instances[0].Tags["Ambiente"] = "producao";
            Volumes.Add(new Volume { Id = "vol-0f3a9e1", Size = 8, Type = "gp3", State = "Em uso", Instance = "i-0web001", Region = "us-east-1" });
            Volumes.Add(new Volume { Id = "vol-0b72c44", Size = 100, Type = "gp2", State = "Em uso", Instance = "i-0abc123", Region = "us-east-1" });
            Volumes.Add(new Volume { Id = "vol-0c5d881", Size = 30, Type = "gp3", State = "Em uso", Instance = "i-0int555", Region = "us-east-1" });
            Volumes.Add(new Volume { Id = "vol-0d9e202", Size = 50, Type = "gp3", State = "Em uso", Instance = "i-0rel777", Region = "us-east-1" });
            Volumes.Add(new Volume { Id = "vol-0e81b37", Size = 500, Type = "gp2", State = "Disponível", Region = "us-east-1" });
            Volumes.Add(new Volume { Id = "vol-0c0ffee", Size = 20, Type = "gp3", State = "Disponível", Region = "us-east-1" });
            Volumes.Add(new Volume { Id = "vol-0a6f310", Size = 8, Type = "gp3", State = "Em uso", Instance = "i-0br0042", Region = "sa-east-1" });
            Snapshots.Add(new Snapshot { Id = "snap-0aa1100", Volume = "vol-0f3a9e1", Description = "inicial", Region = "us-east-1", Size = 8, Created = DateTime.Now.AddDays(-60) });
            SecGroups.Add(new SecGroup { Id = "sg-0web111", Name = "web-sg", Description = "Servidores web do Café 365", Region = "us-east-1", Inbound = { new Rule { Port = 80, Source = "0.0.0.0/0" }, new Rule { Port = 443, Source = "0.0.0.0/0" } } });
            SecGroups.Add(new SecGroup { Id = "sg-0db222", Name = "db-sg", Description = "Banco de dados MySQL do Café 365", Region = "us-east-1", Inbound = { new Rule { Port = 3306, Source = "0.0.0.0/0" } } });
            SecGroups.Add(new SecGroup { Id = "sg-0default", Name = "default", Description = "Grupo padrão da VPC", Region = "us-east-1" });
            SecGroups.Add(new SecGroup { Id = "sg-0brweb3", Name = "web-br-sg", Description = "Servidores web do Brasil", Region = "sa-east-1", Inbound = { new Rule { Port = 80, Source = "0.0.0.0/0" } } });
            Users.Add(new User { Name = "helena", Group = "Administradores", Mfa = true });
            Users.Add(new User { Name = "rodrigo", Group = "Administradores", Mfa = false });
            Users.Add(new User { Name = "ana", Group = "Desenvolvedores", Mfa = true });
            Functions.Add(new Fn { Name = "pedidos", Runtime = "Python 3.12", Env = { { "TABELA", "pedidos" } } });
            Functions.Add(new Fn { Name = "notificacoes", Runtime = "Node.js 20.x", Env = { { "SMTP_HOST", "smtp.cafe365.com" } } });
            Alarms.Add(new Alarm { Name = "disco-cheio-banco", Metric = "FreeStorageSpace", Resource = "banco-cafe", Threshold = 10, State = "OK", Namespace = "AWS/RDS", Comparison = "LessThanThreshold" });
            Alarms.Add(new Alarm { Name = "erros-pedidos", Metric = "Errors", Resource = "pedidos", Threshold = 5, State = "Em alarme", Namespace = "AWS/Lambda" });
            Repos.Add(new Repo { Name = "cafe365-web", ScanOnPush = true });
            Stacks.Add(new Stack { Name = "rede-base", Status = "CREATE_COMPLETE", Template = "rede.yaml" });
            EcsDeployments = 1;
            ResetExtra();
            if (Changed != null) Changed();
        }

        // estado inicial dos recursos da 2ª leva (sem disparar eventos)
        void ResetExtra()
        {
            seq = 0;
            Eips.Clear(); KeyPairs.Clear(); Images.Clear(); Asgs.Clear(); Vpcs.Clear(); Subnets.Clear(); RouteTables.Clear(); Roles.Clear(); AccessKeys.Clear();
            Topics.Clear(); Queues.Clear(); Tables.Clear(); Dbs.Clear(); DbSnaps.Clear(); Records.Clear(); Dists.Clear(); Trails.Clear(); Budgets.Clear();
            LogRetention.Clear(); EbsDefault.Clear(); KeyRotation.Clear();
            PwMin = 8; PwMaxAge = 0; PwUpper = PwLower = PwNumbers = PwSymbols = false;
            Buckets.Add(new Bucket { Name = "cafe365-relatorios", Region = "us-east-1", Objects = { "vendas-agosto.pdf", "vendas-setembro.pdf" }, Created = DateTime.Now.AddDays(-70) });
            Buckets.Add(new Bucket { Name = "micorosoft-auditoria", Region = "us-east-1", Created = DateTime.Now.AddDays(-15) });
            Functions.Add(new Fn { Name = "relatorio-vendas", Runtime = "Python 3.8", Timeout = 60, Memory = 128 });
            Functions.Add(new Fn { Name = "importar-estoque", Runtime = "Python 3.12", Timeout = 120, Memory = 256, Env = { { "DB_HOST", "banco-cafe.interno" }, { "DB_USUARIO", "estoque" } } });
            Functions.Add(new Fn { Name = "processar-pedidos", Runtime = "Python 3.12", Timeout = 50, Memory = 256 });
            Alarms.Add(new Alarm { Name = "cpu-servidor-legado", Metric = "CPUUtilization", Resource = "i-0old999 (encerrada em 2025)", Threshold = 90, State = "Dados insuficientes" });
            Users.Add(new User { Name = "joao", Group = "Desenvolvedores", Mfa = true, Created = DateTime.Now.AddDays(-400) });
            Users.Add(new User { Name = "ci-github", Created = DateTime.Now.AddDays(-410), Policies = { "AmazonEC2ContainerRegistryPowerUser" } });
            Users.Add(new User { Name = "app-web", Created = DateTime.Now.AddDays(-200), Policies = { "AmazonS3ReadOnlyAccess" } });
            AccessKeys.Add(new AccessKey { Id = "AKIA5HELENA0000K2QX9", User = "helena", Status = "Active", Created = DateTime.Now.AddDays(-45) });
            AccessKeys.Add(new AccessKey { Id = "AKIA5RODRIGO7Q2XW3LM", User = "rodrigo", Status = "Active", Created = DateTime.Now.AddDays(-20) });
            AccessKeys.Add(new AccessKey { Id = "AKIA5CIGITHUB2024OLD", User = "ci-github", Status = "Active", Created = DateTime.Now.AddDays(-410) });
            AccessKeys.Add(new AccessKey { Id = "AKIA5APPWEB0SRV00001", User = "app-web", Status = "Active", Created = DateTime.Now.AddDays(-200) });
            AccessKeys.Add(new AccessKey { Id = "AKIA5JOAO0000000DEV1", User = "joao", Status = "Active", Created = DateTime.Now.AddDays(-90) });
            Roles.Add(new Role { Name = "app-s3-leitura", Trust = "ec2", Policies = { "AmazonS3ReadOnlyAccess" } });
            Roles.Add(new Role { Name = "lambda-pedidos-role", Trust = "lambda", Policies = { "AWSLambdaBasicExecutionRole" } });
            Eips.Add(new Eip { Alloc = "eipalloc-0web0001", Ip = "54.210.12.8", Instance = "i-0web001", Region = "us-east-1" });
            Eips.Add(new Eip { Alloc = "eipalloc-0f4ee01", Ip = "34.192.7.21", Region = "us-east-1" });
            KeyPairs.Add(new KeyPair { Name = "cafe-key", Type = "rsa", Region = "us-east-1" });
            KeyPairs.Add(new KeyPair { Name = "br-key", Type = "rsa", Region = "sa-east-1" });
            Images.Add(new Image { Id = "ami-0901d365", Name = "web-golden-2026-10", Instance = "i-0web001", Region = "us-east-1" });
            Asgs.Add(new Asg { Name = "asg-web", Min = 1, Max = 4, Desired = 2 });
            Vpcs.Add(new Vpc { Id = "vpc-0cafe365", Name = "cafe365-vpc", Cidr = "10.0.0.0/16", Region = "us-east-1" });
            Vpcs.Add(new Vpc { Id = "vpc-0br3650", Name = "br-vpc", Cidr = "172.31.0.0/16", Region = "sa-east-1" });
            Subnets.Add(new Subnet { Id = "subnet-0pub1a", Name = "publica-a", Vpc = "vpc-0cafe365", Cidr = "10.0.1.0/24", Az = "us-east-1a", Rtb = "rtb-0publica" });
            Subnets.Add(new Subnet { Id = "subnet-0pub1b", Name = "publica-b", Vpc = "vpc-0cafe365", Cidr = "10.0.2.0/24", Az = "us-east-1b", Rtb = "rtb-0publica" });
            Subnets.Add(new Subnet { Id = "subnet-0priv1a", Name = "privada-a", Vpc = "vpc-0cafe365", Cidr = "10.0.10.0/24", Az = "us-east-1a", Rtb = "rtb-0privada" });
            Subnets.Add(new Subnet { Id = "subnet-0br1a", Name = "br-publica-a", Vpc = "vpc-0br3650", Cidr = "172.31.0.0/20", Az = "sa-east-1a", Rtb = "rtb-0br" });
            RouteTables.Add(new RouteTable { Id = "rtb-0publica", Name = "rt-publica", Vpc = "vpc-0cafe365", Routes = { new[] { "10.0.0.0/16", "local" }, new[] { "0.0.0.0/0", "igw-0cafe365" } } });
            RouteTables.Add(new RouteTable { Id = "rtb-0privada", Name = "rt-privada", Vpc = "vpc-0cafe365", Routes = { new[] { "10.0.0.0/16", "local" } } });
            RouteTables.Add(new RouteTable { Id = "rtb-0br", Name = "rt-br", Vpc = "vpc-0br3650", Routes = { new[] { "172.31.0.0/16", "local" }, new[] { "0.0.0.0/0", "igw-0br3650" } } });
            foreach (var g in SecGroups) g.Vpc = g.Region == "sa-east-1" ? "vpc-0br3650" : "vpc-0cafe365";
            Topics.Add(new Topic { Name = "alertas-ti" });
            Topics.Add(new Topic { Name = "deploys", Subs = { new[] { "email", "devs@cafe365.com.br", "Confirmada" } } });
            Queues.Add(new Queue { Name = "pedidos" });
            Queues.Add(new Queue { Name = "pedidos-dlq" });
            Tables.Add(new Table { Name = "cardapio", Pk = "item_id", PkType = "S", Items = { "{ \"item_id\": \"cafe-expresso\", \"nome\": \"Café expresso\", \"preco\": 6.5 }", "{ \"item_id\": \"cappuccino\", \"nome\": \"Cappuccino\", \"preco\": 11.9 }", "{ \"item_id\": \"pao-de-queijo\", \"nome\": \"Pão de queijo\", \"preco\": 5.5 }", "{ \"item_id\": \"bolo-cenoura\", \"nome\": \"Bolo de cenoura\", \"preco\": 8.0 }" } });
            Tables.Add(new Table { Name = "sessoes", Pk = "sessao_id", PkType = "S", Items = { "{ \"sessao_id\": \"a81f\", \"cliente\": \"maria\", \"expira_em\": 1792108800 }" } });
            Tables.Add(new Table { Name = "historico-pedidos", Pk = "cliente_id", PkType = "S", Sk = "data_pedido", SkType = "S", Items = { "{ \"cliente_id\": \"c-102\", \"data_pedido\": \"2026-10-14T09:12\", \"total\": 23.4 }" } });
            Dbs.Add(new Db { Id = "banco-cafe", Engine = "MySQL 8.0.39", Class = "db.t3.medium", Status = "Disponível" });
            Dbs.Add(new Db { Id = "banco-dev", Engine = "MySQL 8.0.39", Class = "db.t3.micro", Status = "Disponível" });
            DbSnaps.Add(new DbSnap { Id = "rds:banco-cafe-2026-10-14", Db = "banco-cafe", Type = "Automatizado", Created = DateTime.Now.AddDays(-1) });
            Records.Add(new DnsRecord { Name = Zone, Type = "A", Value = "54.210.12.8", Ttl = 300 });
            Records.Add(new DnsRecord { Name = Zone, Type = "MX", Value = "10 mail.cafe365.com.br", Ttl = 3600 });
            Records.Add(new DnsRecord { Name = "www." + Zone, Type = "CNAME", Value = Zone, Ttl = 300 });
            Records.Add(new DnsRecord { Name = "api." + Zone, Type = "A", Value = "54.210.12.8", Ttl = 300 });
            Records.Add(new DnsRecord { Name = "cdn." + Zone, Type = "CNAME", Value = "d111abcdef8.cloudfront.net", Ttl = 300 });
            Records.Add(new DnsRecord { Name = "promo." + Zone, Type = "CNAME", Value = "cafe365-blackfriday.s3-website-us-east-1.amazonaws.com", Ttl = 300 });
            Dists.Add(new Dist { Id = "E1CAFE365SITE", Domain = "d111abcdef8.cloudfront.net", Origin = "cafe365-site.s3.us-east-1.amazonaws.com" });
            foreach (var g in LogGroups) LogRetention[g] = g.StartsWith("/ecs") ? 0 : 30;
        }

        // ── utilitários da 2ª leva
        static string Hx(string s, int n)
        {
            uint h = 2166136261;
            foreach (char c in s ?? "") { h ^= c; h *= 16777619; }
            uint h2 = h * 2654435761u + 0x9e37u;
            string x = h.ToString("x8") + h2.ToString("x8");
            return x.Substring(0, Math.Min(n, x.Length));
        }
        string NextId(string prefix, int n) { seq++; return prefix + Hx(prefix + seq, n); }
        public Bucket Bkt(string name) { return Buckets.FirstOrDefault(x => x.Name == name); }
        static string NoBucket(string n) { return "O bucket " + n + " não existe (NoSuchBucket)."; }
        public static string VersionOf(string bucket, string key) { return Hx("v" + bucket + "/" + key, 16).ToUpperInvariant(); }
        public static string TagStr(Dictionary<string, string> d) { return string.Join(",", d.Select(kv => kv.Key + "=" + kv.Value)); }
        public Fn Func(string n) { return Functions.FirstOrDefault(x => x.Name == n); }
        public Queue Q(string n) { return Queues.FirstOrDefault(x => x.Name == n); }
        public Topic Tp(string n) { return Topics.FirstOrDefault(x => x.Name == n); }
        public Table Tb(string n) { return Tables.FirstOrDefault(x => x.Name == n); }
        public Db DbI(string n) { return Dbs.FirstOrDefault(x => x.Id == n); }
        public Role Rl(string n) { return Roles.FirstOrDefault(x => x.Name == n); }
        public Asg Group(string n) { return Asgs.FirstOrDefault(x => x.Name == n); }
        public static string KmsId(string alias) { var k = KmsIds.FirstOrDefault(x => x[0] == alias); return k == null ? null : k[1]; }

        // CIDR → (rede, máscara); false se inválido
        public static bool CidrParts(string c, out uint net, out uint mask)
        {
            net = 0; mask = 0;
            if (!IsCidr(c)) return false;
            var p = c.Split('/');
            int bits = int.Parse(p[1]);
            var o = p[0].Split('.').Select(uint.Parse).ToArray();
            uint ip = (o[0] << 24) | (o[1] << 16) | (o[2] << 8) | o[3];
            mask = bits == 0 ? 0u : uint.MaxValue << (32 - bits);
            net = ip & mask;
            return true;
        }
        public static bool CidrInside(string inner, string outer)
        {
            uint n1, m1, n2, m2;
            if (!CidrParts(inner, out n1, out m1) || !CidrParts(outer, out n2, out m2)) return false;
            return m1 >= m2 && (n1 & m2) == n2;
        }
        public static bool CidrOverlap(string a, string b)
        {
            uint n1, m1, n2, m2;
            if (!CidrParts(a, out n1, out m1) || !CidrParts(b, out n2, out m2)) return false;
            uint m = m1 < m2 ? m1 : m2;
            return (n1 & m) == (n2 & m);
        }

        void Notify(string evt)
        {
            // primeiro a tarefa avalia (e pode desfazer uma ação perigosa); depois o console redesenha já com o estado final
            if (Event != null) Event(evt);
            if (muteChanged) { pendingChanged = true; return; }
            if (Changed != null) Changed();
        }
        bool muteChanged, pendingChanged;   // durante um comando com --region, o console só redesenha no final

        // ───────────── ações (usadas pelo console e pela CLI) ─────────────

        public void Open(string service) { Notify("open:" + service); }
        public void Identity() { Notify("sts"); }
        public void SetRegion(string r) { if (Region == r) return; Region = r; Notify("region:" + r); }

        public static string BucketNameError(string n)
        {
            if (string.IsNullOrWhiteSpace(n)) return "Informe um nome para o bucket.";
            if (n.Length < 3 || n.Length > 63) return "O nome deve ter entre 3 e 63 caracteres.";
            if (n.Any(ch => !(char.IsLower(ch) || char.IsDigit(ch) || ch == '-' || ch == '.')) || n.Any(char.IsUpper)) return "Use só letras minúsculas, números, pontos e hífens (sem espaços nem acentos).";
            if (!char.IsLetterOrDigit(n[0]) || !char.IsLetterOrDigit(n[n.Length - 1])) return "O nome deve começar e terminar com letra ou número.";
            return null;
        }

        public string CreateBucket(string name, string region, bool blockPublic)
        {
            name = (name ?? "").Trim();
            var err = BucketNameError(name);
            if (err != null) return err;
            if (Buckets.Any(b => b.Name == name)) return "Já existe um bucket com esse nome (BucketAlreadyExists). Nomes de bucket são únicos no mundo inteiro.";
            Buckets.Add(new Bucket { Name = name, Region = region, BlockPublic = blockPublic });
            Notify("s3:create:" + name + ":" + (blockPublic ? "priv" : "pub"));
            return null;
        }

        public void DeleteBucket(string name)
        {
            var b = Buckets.FirstOrDefault(x => x.Name == name);
            if (b == null) return;
            Buckets.Remove(b);
            Notify("s3:delete:" + name);
        }

        public string Upload(string bucket, IEnumerable<string> files)
        {
            var b = Buckets.FirstOrDefault(x => x.Name == bucket);
            if (b == null) return "O bucket " + bucket + " não existe (NoSuchBucket).";
            foreach (var f in files.ToList())
            {
                if (!b.Objects.Contains(f)) b.Objects.Add(f);
                Notify("s3:upload:" + bucket + ":" + f);
            }
            return null;
        }

        // em bucket versionado, excluir cria um marcador de exclusão (a versão anterior continua guardada)
        void MarkDeleted(Bucket b, string key)
        {
            if (b.Versioning != "Ativado") return;
            b.Markers.RemoveAll(m => m.Key == key);
            b.Markers.Add(new Marker { Key = key, VersionId = Hx("dm" + b.Name + key + (++seq), 16).ToUpperInvariant() });
        }

        public void DeleteObject(string bucket, string key)
        {
            var b = Buckets.FirstOrDefault(x => x.Name == bucket);
            if (b != null && b.Objects.Remove(key)) { MarkDeleted(b, key); Notify("s3:delobj:" + bucket + ":" + key); }
        }

        // exclusão em lote (aws s3 rm --recursive): um único evento com todas as chaves, separadas por vírgula
        public void DeleteObjects(string bucket, IEnumerable<string> keys)
        {
            var b = Buckets.FirstOrDefault(x => x.Name == bucket);
            if (b == null) return;
            var gone = keys.Where(k => b.Objects.Remove(k)).ToList();
            foreach (var k in gone) MarkDeleted(b, k);
            if (gone.Count == 1) Notify("s3:delobj:" + bucket + ":" + gone[0]);
            else if (gone.Count > 1) Notify("s3:delmany:" + bucket + ":" + string.Join(",", gone));
        }

        public Ec2 Inst(string id) { return Instances.FirstOrDefault(i => i.Id == id); }

        // devolve null ou o motivo de a ação não ter sido feita
        public string SetState(string id, string action)
        {
            var i = Inst(id);
            if (i == null) return "A instância " + id + " não existe (InvalidInstanceID.NotFound).";
            if (action == "terminate" && i.Protect && i.State != "Encerrada")
            {
                Notify("ec2:terminate-denied:" + id);
                return "A instância " + i.Name + " tem a proteção contra encerramento ativada (OperationNotPermitted). Desative a proteção antes, se o encerramento for mesmo intencional.";
            }
            var eip = Eips.FirstOrDefault(x => x.Instance == id);
            if (action == "stop") { if (i.State == "Encerrada") return "A instância já foi encerrada."; i.State = "Interrompida"; i.Ip = eip != null ? eip.Ip : "-"; }
            else if (action == "start") { if (i.State == "Encerrada") return "A instância já foi encerrada."; i.State = "Executando"; i.Ip = eip != null ? eip.Ip : "54.1" + (id.Length % 9) + "." + (id.GetHashCode() & 255) + ".9"; }
            else if (action == "reboot") { }
            else if (action == "terminate") { i.State = "Encerrada"; i.Ip = "-"; if (eip != null) eip.Instance = null; }
            Notify("ec2:" + action + ":" + id);
            return null;
        }

        public string Launch(string name, string ami, string type) { return Launch(name, ami, type, null, null); }
        public string LastLaunched;   // ID da última instância criada (para a saída da CLI)

        public string Launch(string name, string ami, string type, string key, string sg)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Dê um nome para a instância.";
            if (key != null && key.Length > 0 && !KeyPairs.Any(k => k.Name == key && k.Region == Region)) return "O par de chaves " + key + " não existe nesta região (InvalidKeyPair.NotFound).";
            var g = sg == null ? null : SecGroups.FirstOrDefault(x => (x.Id == sg || x.Name == sg) && x.Region == Region);
            if (sg != null && g == null) return "O grupo de segurança " + sg + " não existe nesta região (InvalidGroup.NotFound).";
            seq++;
            var i = new Ec2 { Id = "i-0" + Hx("i" + name + seq, 7), Name = name.Trim(), Ami = ami, Type = type, State = "Executando", Region = Region, Ip = "44.20" + (seq % 7) + ".31.1" + (seq % 90), Key = key ?? "", Sg = g != null ? g.Id : "sg-0default" };
            Instances.Add(i);
            LastLaunched = i.Id;
            Notify("ec2:launch:" + i.Name + ":" + type + ":" + Region + ":" + ami + ":" + (key ?? "") + ":" + i.Sg);
            return null;
        }

        public string CreateUser(string name, string group)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0) return "Informe o nome do usuário.";
            if (Users.Any(u => u.Name == name)) return "O usuário " + name + " já existe (EntityAlreadyExists).";
            Users.Add(new User { Name = name, Group = group, Created = DateTime.Now });
            Notify("iam:create:" + name + ":" + (group ?? ""));
            return null;
        }

        public string AddToGroup(string name, string group)
        {
            var u = Users.FirstOrDefault(x => x.Name == name);
            if (u == null) return "O usuário " + name + " não existe (NoSuchEntity).";
            if (!Groups.Contains(group)) return "O grupo " + group + " não existe (NoSuchEntity).";
            u.Group = group;
            Notify("iam:group:" + name + ":" + group);
            return null;
        }

        public void DeleteUser(string name)
        {
            var u = Users.FirstOrDefault(x => x.Name == name);
            if (u == null) return;
            Users.Remove(u);
            AccessKeys.RemoveAll(k => k.User == name);
            Notify("iam:delete:" + name);
        }

        public void EnableMfa(string name)
        {
            var u = Users.FirstOrDefault(x => x.Name == name);
            if (u == null) return;
            u.Mfa = true;
            Notify("iam:mfa:" + name);
        }

        public string Invoke(string fn)
        {
            var f = Functions.FirstOrDefault(x => x.Name == fn);
            if (f == null) return null;
            f.LastResult = "{\"statusCode\": 200, \"body\": \"Pedido 4821 registrado\"}";
            Notify("lambda:invoke:" + fn);
            return f.LastResult;
        }

        public string CreateRepo(string name, bool scan, bool immutable)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0) return "Informe o nome do repositório.";
            if (name.Any(char.IsUpper) || name.Contains(" ")) return "O nome deve ter só letras minúsculas, números, hífens, barras e pontos.";
            if (Repos.Any(r => r.Name == name)) return "O repositório já existe (RepositoryAlreadyExistsException).";
            Repos.Add(new Repo { Name = name, ScanOnPush = scan, Immutable = immutable });
            Notify("ecr:create:" + name + ":" + (scan ? "scan" : "noscan"));
            return null;
        }

        public void UpdateService(bool force, int desired)
        {
            EcsRunning = Math.Max(0, desired);
            if (force) EcsDeployments++;
            Notify("ecs:update:" + (force ? "force" : "noforce"));
        }

        public string CreateAlarm(string name, string metric, string resource, float threshold) { return CreateAlarm(name, metric, resource, threshold, "AWS/EC2", "GreaterThanThreshold", null); }

        public Stack CreateStack(string name, string template, bool ackIam)
        {
            name = (name ?? "").Trim();
            Stacks.RemoveAll(s => s.Name == name);
            var st = new Stack { Name = name, Template = template };
            bool needsIam = template == "infra.yaml";
            if (needsIam && !ackIam) { st.Status = "ROLLBACK_COMPLETE"; st.Reason = "Requires capabilities: [CAPABILITY_IAM] (InsufficientCapabilities)"; }
            else st.Status = "CREATE_COMPLETE";
            Stacks.Add(st);
            Notify("cfn:create:" + name + ":" + st.Status);
            return st;
        }

        // ── EC2: tags, tipo de instância, volumes, snapshots e grupos de segurança
        public string AddTag(string id, string key, string value)
        {
            key = (key ?? "").Trim(); value = (value ?? "").Trim();
            var i = Inst(id);
            if (i == null) return "O recurso " + id + " não existe (InvalidID.NotFound).";
            if (key.Length == 0) return "Informe a chave da tag.";
            if (key.Contains(":") || value.Contains(":")) return "Neste laboratório, não use ':' em tags.";
            if (key == "Name") i.Name = value; else i.Tags[key] = value;
            Notify("ec2:tag:" + id + ":" + key + ":" + value);
            return null;
        }

        public string ModifyType(string id, string type)
        {
            var i = Inst(id);
            if (i == null) return "A instância " + id + " não existe (InvalidInstanceID.NotFound).";
            type = (type ?? "").Trim();
            if (!InstanceTypes.Any(t => t[0] == type)) return "Tipo de instância inválido: " + type + " (InvalidInstanceAttributeValue).";
            if (i.State != "Interrompida") return "A instância " + i.Name + " precisa estar INTERROMPIDA para trocar o tipo (IncorrectInstanceState).";
            i.Type = type;
            Notify("ec2:modify:" + id + ":" + type);
            return null;
        }

        public Volume Vol(string id) { return Volumes.FirstOrDefault(v => v.Id == id); }
        public SecGroup Sg(string idOrName) { return SecGroups.FirstOrDefault(g => g.Id == idOrName || g.Name == idOrName); }

        public string CreateSnapshot(string vol, string desc)
        {
            var v = Vol(vol);
            if (v == null) return "O volume " + vol + " não existe (InvalidVolume.NotFound).";
            desc = (desc ?? "").Trim().Replace(':', ' ');
            Snapshots.Add(new Snapshot { Id = "snap-0" + (nextId++).ToString("x") + "5a", Volume = vol, Description = desc, Region = v.Region, Size = v.Size });
            Notify("ec2:snapshot:" + vol + ":" + desc);
            return null;
        }

        public string DeleteVolume(string vol)
        {
            var v = Vol(vol);
            if (v == null) return "O volume " + vol + " não existe (InvalidVolume.NotFound).";
            if (v.Instance != null)
            {
                var i = Inst(v.Instance);
                return "O volume " + vol + " está em uso (anexado a " + (i != null ? i.Name : v.Instance) + "): VolumeInUse. Só volumes 'Disponível' podem ser excluídos.";
            }
            Volumes.Remove(v);
            Notify("ec2:delvol:" + vol);
            return null;
        }

        public static bool IsCidr(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            var p = s.Split('/');
            if (p.Length != 2) return false;
            int bits;
            if (!int.TryParse(p[1], out bits) || bits < 0 || bits > 32) return false;
            var o = p[0].Split('.');
            int n;
            return o.Length == 4 && o.All(x => int.TryParse(x, out n) && n >= 0 && n <= 255 && x.Length > 0 && x.All(char.IsDigit));
        }

        public string Authorize(string gid, int port, string source)
        {
            var g = Sg(gid);
            if (g == null) return "O grupo de segurança " + gid + " não existe (InvalidGroup.NotFound).";
            source = (source ?? "").Trim();
            if (source.StartsWith("sg-")) { if (Sg(source) == null) return "O grupo de origem " + source + " não existe (InvalidGroup.NotFound)."; source = Sg(source).Id; }
            else if (!IsCidr(source)) return "Origem inválida: use um bloco CIDR (ex.: 203.0.113.5/32) ou o ID de um grupo de segurança (sg-...).";
            if (port < 1 || port > 65535) return "Porta inválida: " + port + ".";
            if (g.Inbound.Any(r => r.Port == port && r.Source == source)) return "Essa regra já existe (InvalidPermission.Duplicate).";
            g.Inbound.Add(new Rule { Port = port, Source = source });
            Notify("ec2:sg-add:" + g.Id + ":" + port + ":" + source);
            return null;
        }

        public string Revoke(string gid, int port, string source)
        {
            var g = Sg(gid);
            if (g == null) return "O grupo de segurança " + gid + " não existe (InvalidGroup.NotFound).";
            source = (source ?? "").Trim();
            if (source.StartsWith("sg-") && Sg(source) != null) source = Sg(source).Id;
            var r = g.Inbound.FirstOrDefault(x => x.Port == port && x.Source == source);
            if (r == null) return "Regra não encontrada no grupo " + g.Name + ": porta " + port + " de " + source + " (InvalidPermission.NotFound).";
            g.Inbound.Remove(r);
            Notify("ec2:sg-del:" + g.Id + ":" + port + ":" + source);
            return null;
        }

        // ── S3: versionamento e criptografia padrão
        public string SetVersioning(string bucket, bool enable)
        {
            var b = Buckets.FirstOrDefault(x => x.Name == bucket);
            if (b == null) return "O bucket " + bucket + " não existe (NoSuchBucket).";
            if (!enable && b.Versioning == "Desativado") return null;   // nunca ativado: nada a suspender
            b.Versioning = enable ? "Ativado" : "Suspenso";
            Notify("s3:versioning:" + bucket + ":" + (enable ? "Enabled" : "Suspended"));
            return null;
        }

        public string SetEncryption(string bucket, bool kms, string key)
        {
            var b = Buckets.FirstOrDefault(x => x.Name == bucket);
            if (b == null) return "O bucket " + bucket + " não existe (NoSuchBucket).";
            key = (key ?? "").Trim();
            if (kms && key.Length == 0) key = "alias/aws/s3";
            if (kms && !KmsKeys.Any(k => k[0] == key)) return "Chave KMS não encontrada: " + key + " (KMS.NotFoundException).";
            b.Encryption = kms ? "SSE-KMS" : "SSE-S3";
            b.KmsKey = kms ? key : null;
            Notify("s3:encryption:" + bucket + ":" + b.Encryption + ":" + (kms ? key : ""));
            return null;
        }

        // ── IAM: políticas anexadas diretamente ao usuário
        public string AttachPolicy(string user, string policy)
        {
            var u = Users.FirstOrDefault(x => x.Name == user);
            if (u == null) return "O usuário " + user + " não existe (NoSuchEntity).";
            if (!ManagedPolicies.Any(p => p[0] == policy)) return "A política " + policy + " não existe (NoSuchEntity).";
            if (!u.Policies.Contains(policy)) u.Policies.Add(policy);
            Notify("iam:attach:" + user + ":" + policy);
            return null;
        }

        public string DetachPolicy(string user, string policy)
        {
            var u = Users.FirstOrDefault(x => x.Name == user);
            if (u == null) return "O usuário " + user + " não existe (NoSuchEntity).";
            if (!u.Policies.Remove(policy)) return "A política " + policy + " não está anexada a " + user + " (NoSuchEntity).";
            Notify("iam:detach:" + user + ":" + policy);
            return null;
        }

        // ── CloudWatch Logs: abrir/acompanhar um grupo de logs
        public void OpenLogs(string group) { Notify("logs:" + group); }

        // ═════════════ ações da 2ª leva ═════════════

        // ── S3: tags, site estático, ciclo de vida, bloqueio de acesso público, política, URL pré-assinada, versões
        public string SetBucketTags(string bucket, Dictionary<string, string> tags)
        {
            var b = Bkt(bucket);
            if (b == null) return NoBucket(bucket);
            foreach (var kv in tags)
            {
                if (kv.Key.Trim().Length == 0) return "Informe a chave da tag.";
                if ((kv.Key + kv.Value).IndexOfAny(new[] { ':', ',', '=' }) >= 0) return "Neste laboratório, não use ':', ',' ou '=' em tags.";
            }
            b.Tags = tags.ToDictionary(kv => kv.Key.Trim(), kv => kv.Value.Trim());
            Notify("s3:tags:" + bucket + ":" + TagStr(b.Tags));
            return null;
        }

        public string SetWebsite(string bucket, string index, string error)
        {
            var b = Bkt(bucket);
            if (b == null) return NoBucket(bucket);
            index = (index ?? "").Trim(); error = (error ?? "").Trim();
            if (index.Length == 0) { b.IndexDoc = null; b.ErrorDoc = null; Notify("s3:website:" + bucket + "::"); return null; }
            if (index.Contains("/")) return "O documento de índice não pode conter '/' (ex.: index.html).";
            b.IndexDoc = index; b.ErrorDoc = error.Length > 0 ? error : null;
            Notify("s3:website:" + bucket + ":" + index + ":" + error);
            return null;
        }

        public static string WebsiteUrl(Bucket b) { return "http://" + b.Name + ".s3-website-" + b.Region + ".amazonaws.com"; }

        public string PutLifecycle(string bucket, List<LcRule> rules, bool replace)
        {
            var b = Bkt(bucket);
            if (b == null) return NoBucket(bucket);
            foreach (var r in rules)
            {
                if (string.IsNullOrWhiteSpace(r.Id)) return "Informe o nome (ID) da regra.";
                if (r.Id.Contains(":")) return "O nome da regra não pode ter ':'.";
                if (r.Expire <= 0 && r.Noncurrent <= 0 && r.Glacier <= 0) return "Escolha pelo menos uma ação para a regra (expirar versões atuais, excluir versões desatualizadas ou mover para o Glacier) com o número de dias.";
                if (r.Glacier > 0 && r.Expire > 0 && r.Expire <= r.Glacier) return "A expiração precisa acontecer DEPOIS da transição para o Glacier.";
                r.Prefix = (r.Prefix ?? "").Trim();
            }
            if (replace) b.Lifecycle.Clear();
            foreach (var r in rules) { b.Lifecycle.RemoveAll(x => x.Id == r.Id); b.Lifecycle.Add(r); }
            foreach (var r in rules) Notify("s3:lifecycle:" + bucket + ":" + r.Id + ":" + r.Prefix + ":" + r.Expire + ":" + r.Noncurrent + ":" + r.Glacier);
            return null;
        }

        public string DeleteLifecycleRule(string bucket, string id)
        {
            var b = Bkt(bucket);
            if (b == null) return NoBucket(bucket);
            if (b.Lifecycle.RemoveAll(x => x.Id == id) == 0) return "Regra não encontrada: " + id;
            Notify("s3:lcdel:" + bucket + ":" + id);
            return null;
        }

        public string SetBlockPublic(string bucket, bool on)
        {
            var b = Bkt(bucket);
            if (b == null) return NoBucket(bucket);
            b.BlockPublic = on;
            Notify("s3:bpa:" + bucket + ":" + (on ? "on" : "off"));
            return null;
        }

        // classifica a política do bucket: https, cloudfront, publica ou personalizada (erro se for inválida ou citar outro bucket)
        public static string PolicyKind(string json, string bucket, out string err)
        {
            err = null;
            json = json ?? "";
            if (!json.Contains("\"Statement\"") || !json.Contains("\"Effect\"") || json.Count(c => c == '{') != json.Count(c => c == '}'))
            { err = "MalformedPolicy: o documento não é uma política JSON válida (precisa de Version, Statement, Effect...)."; return null; }
            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(json, "arn:aws:s3:::([a-z0-9.\\-]+)"))
                if (m.Groups[1].Value != bucket) { err = "MalformedPolicy: Policy has invalid resource — a política cita o bucket " + m.Groups[1].Value + ", mas está sendo aplicada no " + bucket + "."; return null; }
            string flat = json.Replace(" ", "");
            if (flat.Contains("aws:SecureTransport") && flat.Contains("\"Deny\"")) return "https";
            if (flat.Contains("cloudfront.amazonaws.com")) return "cloudfront";
            if (flat.Contains("\"Allow\"") && (flat.Contains("\"Principal\":\"*\"") || flat.Contains("\"AWS\":\"*\""))) return "publica";
            return "personalizada";
        }

        public string SetPolicy(string bucket, string json)
        {
            var b = Bkt(bucket);
            if (b == null) return NoBucket(bucket);
            if (string.IsNullOrWhiteSpace(json))
            {
                if (b.Policy == null) return "O bucket não tem política (NoSuchBucketPolicy).";
                b.Policy = null; b.PolicyText = null;
                Notify("s3:policy:" + bucket + ":none");
                return null;
            }
            string err;
            string kind = PolicyKind(json, bucket, out err);
            if (err != null) return err;
            if (kind == "publica" && b.BlockPublic) return "AccessDenied: o Bloqueio de acesso público do bucket (BlockPublicPolicy) impede salvar uma política que dá acesso público.";
            b.Policy = kind; b.PolicyText = json.Trim();
            Notify("s3:policy:" + bucket + ":" + kind);
            return null;
        }

        public string Presign(string bucket, string key, int seconds, out string url)
        {
            url = null;
            var b = Bkt(bucket);
            if (b == null) return NoBucket(bucket);
            if (!b.Objects.Contains(key)) return "O objeto " + key + " não existe no bucket " + bucket + " (NoSuchKey).";
            if (seconds < 1 || seconds > 604800) return "A validade de uma URL pré-assinada vai de 1 segundo a 7 dias (604800 segundos).";
            url = "https://" + bucket + ".s3." + b.Region + ".amazonaws.com/" + key + "?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIA5ESTAGIO%2F" + DateTime.Now.ToString("yyyyMMdd") + "%2F" + b.Region + "%2Fs3%2Faws4_request&X-Amz-Expires=" + seconds + "&X-Amz-SignedHeaders=host&X-Amz-Signature=" + Hx(bucket + key + seconds, 16) + Hx(key + seconds, 16);
            Notify("s3:presign:" + bucket + ":" + key + ":" + seconds);
            return null;
        }

        // exclui uma versão: se for um marcador de exclusão, o objeto "volta"
        public string DeleteVersion(string bucket, string key, string versionId)
        {
            var b = Bkt(bucket);
            if (b == null) return NoBucket(bucket);
            var mk = b.Markers.FirstOrDefault(m => m.Key == key && m.VersionId == versionId);
            if (mk != null)
            {
                b.Markers.Remove(mk);
                if (!b.Objects.Contains(key)) b.Objects.Add(key);
                Notify("s3:unmark:" + bucket + ":" + key);
                return null;
            }
            if (b.Objects.Contains(key) && VersionOf(bucket, key) == versionId)
            {
                b.Objects.Remove(key);
                Notify("s3:delver:" + bucket + ":" + key);
                return null;
            }
            return "Versão não encontrada: " + key + " (versionId " + versionId + ") — NoSuchVersion.";
        }

        // ── EC2: pares de chaves, IP elástico, AMI, proteção, metadados, role, volumes, grupos, criptografia padrão
        public string CreateKeyPair(string name, string type)
        {
            name = (name ?? "").Trim(); type = (type ?? "rsa").Trim().ToLowerInvariant();
            if (name.Length == 0) return "Informe o nome do par de chaves.";
            if (name.Contains(" ") || name.Contains(":")) return "O nome não pode ter espaços nem ':'.";
            if (type != "rsa" && type != "ed25519") return "Tipo inválido: " + type + " (use rsa ou ed25519).";
            if (KeyPairs.Any(k => k.Name == name && k.Region == Region)) return "Já existe um par de chaves " + name + " nesta região (InvalidKeyPair.Duplicate).";
            KeyPairs.Add(new KeyPair { Name = name, Type = type, Region = Region, Created = DateTime.Now });
            Notify("ec2:keypair:" + name + ":" + type);
            return null;
        }

        public string DeleteKeyPair(string name)
        {
            var k = KeyPairs.FirstOrDefault(x => x.Name == name && x.Region == Region);
            if (k == null) return "O par de chaves " + name + " não existe nesta região.";
            KeyPairs.Remove(k);
            Notify("ec2:delkeypair:" + name);
            return null;
        }

        public Eip EipOf(string alloc) { return Eips.FirstOrDefault(x => x.Alloc == alloc); }

        public string AllocateEip(out string alloc) { string ip; return AllocateEip(out alloc, out ip); }

        public string AllocateEip(out string alloc, out string ip)
        {
            seq++;
            alloc = "eipalloc-0" + Hx("eip" + seq, 7);
            ip = "3.2" + (seq % 9) + "." + (40 + seq % 50) + "." + (10 + seq % 200);
            Eips.Add(new Eip { Alloc = alloc, Ip = ip, Region = Region });
            Notify("ec2:eip-alloc:" + alloc);
            return null;
        }

        public string AssociateEip(string alloc, string inst, bool reassoc)
        {
            var e = EipOf(alloc);
            if (e == null || e.Region != Region) return "O IP elástico " + alloc + " não existe nesta região (InvalidAllocationID.NotFound).";
            var i = Inst(inst);
            if (i == null || i.Region != Region) return "A instância " + inst + " não existe nesta região (InvalidInstanceID.NotFound).";
            if (i.State == "Encerrada") return "A instância " + i.Name + " foi encerrada.";
            if (e.Instance != null && e.Instance != inst && !reassoc) return "O IP " + e.Ip + " já está associado a " + (Inst(e.Instance) != null ? Inst(e.Instance).Name : e.Instance) + " (Resource.AlreadyAssociated). Para mover, marque 'Permitir reassociação'.";
            if (e.Instance != null && e.Instance != inst) { var old = Inst(e.Instance); if (old != null) old.Ip = old.State == "Executando" ? "54.9" + (seq % 9) + ".1.7" : "-"; }
            var prev = Eips.FirstOrDefault(x => x.Instance == inst && x != e);
            if (prev != null) prev.Instance = null;
            e.Instance = inst;
            i.Ip = e.Ip;
            Notify("ec2:eip-assoc:" + alloc + ":" + inst);
            return null;
        }

        public string DisassociateEip(string alloc)
        {
            var e = EipOf(alloc);
            if (e == null || e.Region != Region) return "O IP elástico " + alloc + " não existe nesta região.";
            if (e.Instance == null) return "O IP " + e.Ip + " não está associado a nada.";
            var i = Inst(e.Instance);
            string inst = e.Instance;
            e.Instance = null;
            if (i != null) i.Ip = i.State == "Executando" ? "54.9" + (seq % 9) + ".2.8" : "-";
            Notify("ec2:eip-disassoc:" + alloc + ":" + inst);
            return null;
        }

        public string ReleaseEip(string alloc)
        {
            var e = EipOf(alloc);
            if (e == null || e.Region != Region) return "O IP elástico " + alloc + " não existe nesta região (InvalidAllocationID.NotFound).";
            if (e.Instance != null) return "O IP " + e.Ip + " está associado a " + (Inst(e.Instance) != null ? Inst(e.Instance).Name : e.Instance) + " (InvalidIPAddress.InUse). Desassocie antes de liberar.";
            Eips.Remove(e);
            Notify("ec2:eip-release:" + alloc);
            return null;
        }

        public string CreateImage(string inst, string name, bool noReboot)
        {
            var i = Inst(inst);
            if (i == null || i.Region != Region) return "A instância " + inst + " não existe nesta região (InvalidInstanceID.NotFound).";
            name = (name ?? "").Trim();
            if (name.Length < 3) return "Informe o nome da imagem (3 a 128 caracteres).";
            if (name.Contains(" ") || name.Contains(":")) return "Neste laboratório, use nome sem espaços nem ':' (ex.: servidor-base).";
            if (Images.Any(x => x.Name == name && x.Region == Region)) return "Já existe uma AMI com o nome " + name + " (InvalidAMIName.Duplicate).";
            var img = new Image { Id = "ami-0" + Hx("ami" + name, 7), Name = name, Instance = inst, Region = Region, Created = DateTime.Now };
            Images.Add(img);
            Notify("ec2:image:" + inst + ":" + name + ":" + (noReboot ? "noreboot" : "reboot"));
            return null;
        }

        public string SetProtection(string inst, bool on)
        {
            var i = Inst(inst);
            if (i == null) return "A instância " + inst + " não existe (InvalidInstanceID.NotFound).";
            i.Protect = on;
            Notify("ec2:protect:" + inst + ":" + (on ? "on" : "off"));
            return null;
        }

        public string SetImds(string inst, string tokens, bool endpoint)
        {
            var i = Inst(inst);
            if (i == null) return "A instância " + inst + " não existe (InvalidInstanceID.NotFound).";
            tokens = (tokens ?? "optional").Trim().ToLowerInvariant();
            if (tokens != "optional" && tokens != "required") return "--http-tokens aceita optional ou required.";
            i.Imds = endpoint ? tokens : "disabled";
            Notify("ec2:imds:" + inst + ":" + tokens + ":" + (endpoint ? "enabled" : "disabled"));
            return null;
        }

        public string AssociateRole(string inst, string role)
        {
            var i = Inst(inst);
            if (i == null) return "A instância " + inst + " não existe (InvalidInstanceID.NotFound).";
            var r = Rl(role);
            if (r == null) return "Perfil de instância não encontrado: " + role + " (NoSuchEntity).";
            if (r.Trust != "ec2") return "A função " + role + " não confia no serviço EC2 (não tem perfil de instância). Só funções com entidade confiável EC2 podem ser usadas em instâncias.";
            if (i.Role != null && i.Role != role) return "A instância já tem a função " + i.Role + " (IncorrectState). Substitua em vez de associar outra.";
            i.Role = role;
            Notify("ec2:role:" + inst + ":" + role);
            return null;
        }

        public string ModifyVolume(string vol, string type, int size)
        {
            var v = Vol(vol);
            if (v == null) return "O volume " + vol + " não existe (InvalidVolume.NotFound).";
            type = (type ?? v.Type).Trim();
            if (!VolumeTypes.Contains(type)) return "Tipo de volume inválido: " + type + " (use gp3, gp2, io2, st1 ou sc1).";
            if (size <= 0) size = v.Size;
            if (size < v.Size) return "Não é possível DIMINUIR um volume EBS (de " + v.Size + " para " + size + " GiB): só aumentar.";
            if (type == v.Type && size == v.Size) return "Nada mudou: escolha outro tipo ou tamanho.";
            v.Type = type; v.Size = size;
            Notify("ec2:modvol:" + vol + ":" + type + ":" + size);
            return null;
        }

        public string CreateSg(string name, string desc, string vpc, out string id)
        {
            id = null;
            name = (name ?? "").Trim(); desc = (desc ?? "").Trim();
            if (name.Length == 0) return "Informe o nome do grupo de segurança.";
            if (name.StartsWith("sg-") || name.Contains(":") || name.Contains(" ")) return "Nome inválido (não comece com sg- e não use espaços).";
            if (desc.Length == 0) return "A descrição é obrigatória.";
            var v = Vpcs.FirstOrDefault(x => x.Id == (vpc ?? "") && x.Region == Region) ?? Vpcs.FirstOrDefault(x => x.Region == Region);
            if (SecGroups.Any(g => g.Name == name && g.Region == Region)) return "Já existe um grupo " + name + " nesta região (InvalidGroup.Duplicate).";
            id = "sg-0" + Hx("sg" + name, 7);
            SecGroups.Add(new SecGroup { Id = id, Name = name, Description = desc.Replace(':', ' '), Region = Region, Vpc = v != null ? v.Id : null });
            Notify("ec2:sg-create:" + id + ":" + name);
            return null;
        }

        public string SetEbsDefault(string region, bool on)
        {
            EbsDefault[region] = on;
            Notify("ec2:ebs-default:" + region + ":" + (on ? "on" : "off"));
            return null;
        }
        public bool EbsDefaultOn(string region) { bool b; return EbsDefault.TryGetValue(region, out b) && b; }

        // ── Auto Scaling
        public string SetCapacity(string name, int min, int max, int desired)
        {
            var a = Group(name);
            if (a == null) return "Grupo do Auto Scaling não encontrado: " + name + " (ValidationError).";
            if (min < 0 || max < 0 || desired < 0) return "Os valores não podem ser negativos.";
            if (min > max) return "A capacidade mínima (" + min + ") não pode ser maior que a máxima (" + max + ") (ValidationError).";
            if (desired < min || desired > max) return "A capacidade desejada (" + desired + ") precisa ficar entre a mínima (" + min + ") e a máxima (" + max + ") (ValidationError).";
            a.Min = min; a.Max = max; a.Desired = desired;
            Notify("asg:capacity:" + name + ":" + min + ":" + max + ":" + desired);
            return null;
        }

        public string PutScalingPolicy(string asg, string name, string type, string metric, float target)
        {
            var a = Group(asg);
            if (a == null) return "Grupo do Auto Scaling não encontrado: " + asg + " (ValidationError).";
            name = (name ?? "").Trim();
            if (name.Length == 0 || name.Contains(":")) return "Informe o nome da política (sem ':').";
            if (type == "TargetTrackingScaling" && (target <= 0 || target > 100 && metric != "ALBRequestCountPerTarget" && metric != "ASGAverageNetworkIn")) return "Valor de destino inválido: " + target + ".";
            a.Policies.RemoveAll(p => p[0] == name);
            string tv = target.ToString(System.Globalization.CultureInfo.InvariantCulture);
            a.Policies.Add(new[] { name, type, metric, tv });
            Notify("asg:policy:" + asg + ":" + name + ":" + type + ":" + metric + ":" + tv);
            return null;
        }

        public string DeleteScalingPolicy(string asg, string name)
        {
            var a = Group(asg);
            if (a == null || a.Policies.RemoveAll(p => p[0] == name) == 0) return "Política não encontrada: " + name;
            Notify("asg:delpolicy:" + asg + ":" + name);
            return null;
        }

        // ── VPC: sub-redes e rotas
        public string CreateSubnet(string vpc, string cidr, string az, string name, out string id)
        {
            id = null;
            var v = Vpcs.FirstOrDefault(x => x.Id == vpc);
            if (v == null || v.Region != Region) return "A VPC " + vpc + " não existe nesta região (InvalidVpcID.NotFound).";
            cidr = (cidr ?? "").Trim(); az = (az ?? "").Trim(); name = (name ?? "").Trim();
            if (!IsCidr(cidr)) return "Bloco CIDR inválido: '" + cidr + "' (ex.: 10.0.5.0/24).";
            uint n, m;
            CidrParts(cidr, out n, out m);
            if (cidr.Split('/')[0] != string.Join(".", new[] { n >> 24, (n >> 16) & 255, (n >> 8) & 255, n & 255 })) return "O endereço " + cidr + " não é o início de um bloco /" + cidr.Split('/')[1] + " (InvalidSubnet.Range).";
            int bits = int.Parse(cidr.Split('/')[1]);
            if (bits < 16 || bits > 28) return "Sub-redes na AWS vão de /16 a /28 (InvalidSubnet.Range).";
            if (!CidrInside(cidr, v.Cidr)) return "O bloco " + cidr + " está fora da VPC (" + v.Cidr + ") (InvalidSubnet.Range).";
            var clash = Subnets.FirstOrDefault(s => s.Vpc == vpc && CidrOverlap(s.Cidr, cidr));
            if (clash != null) return "O bloco " + cidr + " se sobrepõe à sub-rede " + clash.Name + " (" + clash.Cidr + ") (InvalidSubnet.Conflict).";
            if (!az.StartsWith(Region) || az.Length != Region.Length + 1 || "abc".IndexOf(az[az.Length - 1]) < 0) return "Zona de disponibilidade inválida: '" + az + "' (nesta região: " + Region + "a, " + Region + "b ou " + Region + "c).";
            if (name.Contains(":")) return "O nome não pode ter ':'.";
            id = "subnet-0" + Hx("sn" + cidr, 7);
            var main = RouteTables.FirstOrDefault(r => r.Vpc == vpc && r.Name.Contains("privada")) ?? RouteTables.FirstOrDefault(r => r.Vpc == vpc);
            Subnets.Add(new Subnet { Id = id, Name = name, Vpc = vpc, Cidr = cidr, Az = az, Rtb = main != null ? main.Id : null });
            Notify("vpc:subnet:" + id + ":" + vpc + ":" + cidr + ":" + az + ":" + name);
            return null;
        }

        public static readonly string[][] RouteTargets = { new[] { "igw-0cafe365", "Gateway da Internet (igw)" }, new[] { "nat-0cafe01", "Gateway NAT (nat) — na sub-rede publica-a" }, new[] { "local", "local (dentro da VPC)" } };

        public string SetRoute(string rtb, string dest, string target, bool replace)
        {
            var r = RouteTables.FirstOrDefault(x => x.Id == rtb || x.Name == rtb);
            if (r == null) return "A tabela de rotas " + rtb + " não existe (InvalidRouteTableID.NotFound).";
            dest = (dest ?? "").Trim(); target = (target ?? "").Trim();
            if (!IsCidr(dest)) return "Destino inválido: '" + dest + "' (use um bloco CIDR, ex.: 0.0.0.0/0).";
            var vpc = Vpcs.FirstOrDefault(x => x.Id == r.Vpc);
            bool valid = target == "igw-0cafe365" || target == "nat-0cafe01" || (r.Vpc == "vpc-0br3650" && target == "igw-0br3650");
            if (!valid) return "Alvo inválido: '" + target + "' (nesta VPC: igw-0cafe365 ou nat-0cafe01).";
            var ex = r.Routes.FirstOrDefault(x => x[0] == dest);
            if (ex != null && ex[1] == "local") return "A rota local (" + dest + ") não pode ser alterada.";
            if (vpc != null && dest == vpc.Cidr) return "Esse destino é a própria VPC (rota local).";
            if (ex != null && !replace) return "Já existe uma rota para " + dest + " (RouteAlreadyExists). Use replace-route (ou 'Editar rotas' no console) para trocar o alvo.";
            if (ex == null && replace) return "Não existe rota para " + dest + " nessa tabela (InvalidRoute.NotFound).";
            if (ex != null) ex[1] = target; else r.Routes.Add(new[] { dest, target });
            Notify("vpc:route:" + r.Id + ":" + dest + ":" + target);
            return null;
        }

        public string DeleteRoute(string rtb, string dest)
        {
            var r = RouteTables.FirstOrDefault(x => x.Id == rtb || x.Name == rtb);
            if (r == null) return "A tabela de rotas " + rtb + " não existe.";
            var ex = r.Routes.FirstOrDefault(x => x[0] == dest);
            if (ex == null) return "Rota não encontrada: " + dest + " (InvalidRoute.NotFound).";
            if (ex[1] == "local") return "A rota local não pode ser excluída.";
            r.Routes.Remove(ex);
            Notify("vpc:delroute:" + r.Id + ":" + dest + ":" + ex[1]);
            return null;
        }

        // ── IAM: chaves de acesso, roles, política de senha, grupos
        public string SetKeyStatus(string user, string keyId, string status)
        {
            var k = AccessKeys.FirstOrDefault(x => x.Id == keyId);
            if (k == null || k.User != user) return "A chave " + keyId + " não pertence ao usuário " + user + " (NoSuchEntity).";
            if (status != "Active" && status != "Inactive") return "--status aceita Active ou Inactive.";
            k.Status = status;
            Notify("iam:key-status:" + user + ":" + keyId + ":" + status);
            return null;
        }

        public string CreateAccessKey(string user, out string keyId)
        {
            keyId = null;
            if (!Users.Any(u => u.Name == user)) return "O usuário " + user + " não existe (NoSuchEntity).";
            if (AccessKeys.Count(k => k.User == user) >= 2) return "O usuário " + user + " já tem 2 chaves de acesso, o máximo (LimitExceeded). Exclua uma antes.";
            seq++;
            keyId = "AKIA5" + Hx("ak" + user + seq, 15).ToUpperInvariant();
            AccessKeys.Add(new AccessKey { Id = keyId, User = user, Status = "Active", Created = DateTime.Now });
            Notify("iam:key-create:" + user + ":" + keyId);
            return null;
        }

        public string DeleteAccessKey(string user, string keyId)
        {
            var k = AccessKeys.FirstOrDefault(x => x.Id == keyId);
            if (k == null || k.User != user) return "A chave " + keyId + " não pertence ao usuário " + user + " (NoSuchEntity).";
            AccessKeys.Remove(k);
            Notify("iam:key-delete:" + user + ":" + keyId);
            return null;
        }

        public string CreateRole(string name, string trust)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0) return "Informe o nome da função (role).";
            if (name.Contains(" ") || name.Contains(":")) return "O nome não pode ter espaços nem ':'.";
            if (Roles.Any(r => r.Name == name)) return "A função " + name + " já existe (EntityAlreadyExists).";
            if (trust != "ec2" && trust != "lambda" && trust != "ecs-tasks") return "Entidade confiável inválida.";
            Roles.Add(new Role { Name = name, Trust = trust });
            Notify("iam:role-create:" + name + ":" + trust);
            return null;
        }

        public string AttachRolePolicy(string role, string policy)
        {
            var r = Rl(role);
            if (r == null) return "A função " + role + " não existe (NoSuchEntity).";
            if (!ManagedPolicies.Any(p => p[0] == policy)) return "A política " + policy + " não existe (NoSuchEntity).";
            if (!r.Policies.Contains(policy)) r.Policies.Add(policy);
            Notify("iam:role-attach:" + role + ":" + policy);
            return null;
        }

        public string DetachRolePolicy(string role, string policy)
        {
            var r = Rl(role);
            if (r == null) return "A função " + role + " não existe (NoSuchEntity).";
            if (!r.Policies.Remove(policy)) return "A política " + policy + " não está anexada a " + role + ".";
            Notify("iam:role-detach:" + role + ":" + policy);
            return null;
        }

        public string DeleteRole(string role)
        {
            var r = Rl(role);
            if (r == null) return "A função " + role + " não existe (NoSuchEntity).";
            Roles.Remove(r);
            foreach (var i in Instances.Where(x => x.Role == role)) i.Role = null;
            Notify("iam:role-delete:" + role);
            return null;
        }

        public string SetPasswordPolicy(int min, bool upper, bool lower, bool numbers, bool symbols, int maxAge)
        {
            if (min < 6 || min > 128) return "O comprimento mínimo vai de 6 a 128 caracteres.";
            if (maxAge < 0 || maxAge > 1095) return "A expiração vai de 1 a 1095 dias (ou nenhuma).";
            PwMin = min; PwUpper = upper; PwLower = lower; PwNumbers = numbers; PwSymbols = symbols; PwMaxAge = maxAge;
            Notify("iam:pwpolicy:" + min + ":" + (upper ? 1 : 0) + ":" + (lower ? 1 : 0) + ":" + (numbers ? 1 : 0) + ":" + (symbols ? 1 : 0) + ":" + maxAge);
            return null;
        }

        public string RemoveFromGroup(string user, string group)
        {
            var u = Users.FirstOrDefault(x => x.Name == user);
            if (u == null) return "O usuário " + user + " não existe (NoSuchEntity).";
            if (u.Group != group) return "O usuário " + user + " não está no grupo " + group + " (NoSuchEntity).";
            u.Group = null;
            Notify("iam:ungroup:" + user + ":" + group);
            return null;
        }

        // exclusão pela API: falha se ainda houver grupo, chaves ou políticas (o console remove tudo antes)
        public string DeleteUserStrict(string user)
        {
            var u = Users.FirstOrDefault(x => x.Name == user);
            if (u == null) return "O usuário " + user + " não existe (NoSuchEntity).";
            var pend = new List<string>();
            if (u.Group != null) pend.Add("está no grupo " + u.Group + " (remove-user-from-group)");
            if (AccessKeys.Any(k => k.User == user)) pend.Add("tem chave(s) de acesso (delete-access-key)");
            if (u.Policies.Count > 0) pend.Add("tem políticas anexadas (detach-user-policy)");
            if (pend.Count > 0) return "An error occurred (DeleteConflict): Cannot delete entity, must remove dependencies first — o usuário " + string.Join("; ", pend) + ".";
            DeleteUser(user);
            return null;
        }

        // ── Lambda
        public string UpdateFnConfig(string fn, int timeout, int memory, string runtime)
        {
            var f = Func(fn);
            if (f == null) return "Função não encontrada: " + fn + " (ResourceNotFoundException).";
            if (timeout != -1 && (timeout < 1 || timeout > 900)) return "O tempo limite vai de 1 a 900 segundos (15 minutos).";
            if (memory != -1 && (memory < 128 || memory > 10240)) return "A memória vai de 128 MB a 10240 MB.";
            string disp = null;
            if (runtime != null)
            {
                var r = Runtimes.FirstOrDefault(x => x[0] == runtime || x[1] == runtime);
                if (r == null) return "Runtime inválido: " + runtime + " (ex.: python3.12, nodejs20.x).";
                disp = r[1];
            }
            var evts = new List<string>();
            if (timeout != -1 && timeout != f.Timeout) { f.Timeout = timeout; evts.Add("timeout:" + timeout); }
            if (memory != -1 && memory != f.Memory) { f.Memory = memory; evts.Add("memory:" + memory); }
            if (disp != null && disp != f.Runtime) { f.Runtime = disp; evts.Add("runtime:" + RuntimeId(disp)); }
            foreach (var e in evts) Notify("lambda:config:" + fn + ":" + e);
            return null;
        }

        public string SetEnv(string fn, Dictionary<string, string> vars)
        {
            var f = Func(fn);
            if (f == null) return "Função não encontrada: " + fn + " (ResourceNotFoundException).";
            foreach (var kv in vars)
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(kv.Key, "^[A-Za-z][A-Za-z0-9_]*$")) return "Nome de variável inválido: '" + kv.Key + "' (letras, números e _; começa com letra).";
                if (kv.Value.IndexOfAny(new[] { ',', '=', ':' }) >= 0) return "Neste laboratório, os valores não podem ter ',', '=' ou ':'.";
            }
            f.Env = new Dictionary<string, string>(vars);
            Notify("lambda:env:" + fn + ":" + TagStr(f.Env));
            return null;
        }

        public string SetConcurrency(string fn, int n)
        {
            var f = Func(fn);
            if (f == null) return "Função não encontrada: " + fn + " (ResourceNotFoundException).";
            if (n > 900) return "A conta só tem 1000 execuções simultâneas e 100 precisam ficar livres: reserve no máximo 900.";
            f.Concurrency = n < 0 ? -1 : n;
            Notify("lambda:concurrency:" + fn + ":" + f.Concurrency);
            return null;
        }

        public string AddTrigger(string fn, string queue, int batch)
        {
            var f = Func(fn);
            if (f == null) return "Função não encontrada: " + fn + " (ResourceNotFoundException).";
            var q = Q(queue);
            if (q == null) return "A fila " + queue + " não existe (QueueDoesNotExist).";
            if (batch < 1 || batch > 10000) return "O tamanho do lote vai de 1 a 10000.";
            if (q.Fifo && batch > 10) return "Filas FIFO aceitam lote de no máximo 10.";
            if (f.Triggers.Any(t => t[0] == queue)) return "Já existe um gatilho da fila " + queue + " nesta função (ResourceConflictException).";
            f.Triggers.Add(new[] { queue, batch.ToString() });
            Notify("lambda:trigger:" + fn + ":" + queue + ":" + batch);
            return null;
        }

        public string RemoveTrigger(string fn, string queue)
        {
            var f = Func(fn);
            if (f == null || f.Triggers.RemoveAll(t => t[0] == queue) == 0) return "Gatilho não encontrado.";
            Notify("lambda:untrigger:" + fn + ":" + queue);
            return null;
        }

        // ── CloudWatch: alarmes (com namespace e ação), exclusão, retenção e filtro de logs
        public string CreateAlarm(string name, string metric, string resource, float threshold, string ns, string comparison, string action)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0) return "Informe o nome do alarme.";
            if (name.Contains(":")) return "O nome do alarme não pode ter ':'.";
            if (action != null && Tp(action) == null) return "O tópico do SNS " + action + " não existe (InvalidParameterValue).";
            Alarms.RemoveAll(a => a.Name == name);
            Alarms.Add(new Alarm { Name = name, Metric = metric, Resource = resource, Threshold = threshold, State = "Dados insuficientes", Namespace = ns ?? "AWS/EC2", Comparison = comparison ?? "GreaterThanThreshold", Action = action });
            Notify("cw:alarm:" + name + ":" + metric + ":" + resource + ":" + threshold.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":" + (ns ?? "AWS/EC2") + ":" + (comparison ?? "GreaterThanThreshold") + ":" + (action ?? ""));
            return null;
        }

        public string DeleteAlarm(string name)
        {
            var a = Alarms.FirstOrDefault(x => x.Name == name);
            if (a == null) return "Alarme não encontrado: " + name + " (ResourceNotFound).";
            Alarms.Remove(a);
            Notify("cw:delete:" + name);
            return null;
        }

        public int Retention(string group) { int d; return LogRetention.TryGetValue(group, out d) ? d : 0; }

        public string SetRetention(string group, int days)
        {
            if (!LogGroups.Contains(group)) return "O grupo de logs " + group + " não existe (ResourceNotFoundException).";
            if (days != 0 && !RetentionDays.Contains(days)) return "Valor de retenção inválido: " + days + " (InvalidParameterException). Use 1, 3, 5, 7, 14, 30, 60, 90, 120, 150, 180, 365, 400, 545, 731...";
            LogRetention[group] = days;
            Notify("logs:retention:" + group + ":" + days);
            return null;
        }

        public List<string> FilterLogs(string group, string pattern)
        {
            pattern = (pattern ?? "").Trim();
            string term = pattern.Trim('"').TrimStart('?');
            var res = LogLines(group).Where(l => term.Length == 0 || l.Contains(term)).ToList();
            Notify("logs-filter:" + group + ":" + pattern.Replace(":", " "));
            return res;
        }

        // ── SNS
        public string CreateTopic(string name, bool fifo)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0) return "Informe o nome do tópico.";
            if (!System.Text.RegularExpressions.Regex.IsMatch(name, "^[A-Za-z0-9_\\-]+(\\.fifo)?$")) return "Nome inválido: use letras, números, hífens e _ (até 256).";
            if (fifo != name.EndsWith(".fifo")) return fifo ? "Tópicos FIFO precisam terminar com .fifo." : "Só tópicos FIFO terminam com .fifo.";
            if (Tp(name) != null) return "O tópico " + name + " já existe.";
            Topics.Add(new Topic { Name = name, Fifo = fifo });
            Notify("sns:create:" + name + ":" + (fifo ? "fifo" : "std"));
            return null;
        }

        // cria um tópico padrão sem disparar evento (preparação de cenário)
        public void CreateTopicQuiet(string name) { if (Tp(name) == null) Topics.Add(new Topic { Name = name }); }

        public string DeleteTopic(string name)
        {
            var t = Tp(name);
            if (t == null) return "Tópico não encontrado (NotFound).";
            Topics.Remove(t);
            Notify("sns:delete:" + name);
            return null;
        }

        public string Subscribe(string topic, string protocol, string endpoint)
        {
            var t = Tp(topic);
            if (t == null) return "Tópico não encontrado: " + topic + " (NotFound).";
            protocol = (protocol ?? "").Trim().ToLowerInvariant(); endpoint = (endpoint ?? "").Trim();
            var protos = new[] { "email", "sms", "https", "sqs", "lambda" };
            if (!protos.Contains(protocol)) return "Protocolo inválido: " + protocol + " (email, sms, https, sqs, lambda).";
            if (protocol == "email" && !System.Text.RegularExpressions.Regex.IsMatch(endpoint, "^[^@\\s]+@[^@\\s]+\\.[a-z]{2,}$")) return "Endereço de e-mail inválido: " + endpoint;
            if (endpoint.Length == 0) return "Informe o endpoint.";
            if (t.Fifo && protocol == "email") return "Tópicos FIFO não entregam para e-mail (só SQS).";
            if (t.Subs.Any(s => s[0] == protocol && s[1] == endpoint)) return "Essa assinatura já existe.";
            t.Subs.Add(new[] { protocol, endpoint, protocol == "email" || protocol == "https" ? "Confirmação pendente" : "Confirmada" });
            Notify("sns:sub:" + topic + ":" + protocol + ":" + endpoint.Replace(":", "|"));
            return null;
        }

        public string Unsubscribe(string topic, string protocol, string endpoint)
        {
            var t = Tp(topic);
            if (t == null || t.Subs.RemoveAll(s => s[0] == protocol && s[1] == endpoint) == 0) return "Assinatura não encontrada.";
            Notify("sns:unsub:" + topic + ":" + protocol + ":" + endpoint.Replace(":", "|"));
            return null;
        }

        public string Publish(string topic, string message)
        {
            var t = Tp(topic);
            if (t == null) return "Tópico não encontrado: " + topic + " (NotFound).";
            if (string.IsNullOrWhiteSpace(message)) return "A mensagem não pode ficar vazia.";
            Notify("sns:publish:" + topic);
            return null;
        }

        // ── SQS
        public static string QueueUrl(string name) { return "https://sqs.us-east-1.amazonaws.com/" + Arn + "/" + name; }

        public string CreateQueue(string name, bool fifo, bool dedup, int visibility, int wait)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0) return "Informe o nome da fila.";
            if (!System.Text.RegularExpressions.Regex.IsMatch(name, "^[A-Za-z0-9_\\-]+(\\.fifo)?$")) return "Nome inválido: use letras, números, hífens e _ (até 80).";
            if (fifo != name.EndsWith(".fifo")) return fifo ? "O nome de uma fila FIFO precisa terminar com .fifo." : "Só filas FIFO terminam com .fifo (marque o tipo FIFO).";
            if (!fifo && dedup) return "Deduplicação baseada em conteúdo só existe em filas FIFO.";
            if (Q(name) != null) return "A fila " + name + " já existe (QueueAlreadyExists).";
            if (visibility < 0 || visibility > 43200) return "O tempo limite de visibilidade vai de 0 a 43200 segundos (12 horas).";
            if (wait < 0 || wait > 20) return "O tempo de espera de recebimento vai de 0 a 20 segundos.";
            Queues.Add(new Queue { Name = name, Fifo = fifo, Dedup = dedup, Visibility = visibility, Wait = wait });
            Notify("sqs:create:" + name + ":" + (fifo ? "fifo" : "std") + ":" + (dedup ? "dedup" : "nodedup"));
            return null;
        }

        public string DeleteQueue(string name)
        {
            var q = Q(name);
            if (q == null) return "A fila não existe (QueueDoesNotExist).";
            Queues.Remove(q);
            Notify("sqs:delete:" + name);
            return null;
        }

        public string SetQueueAttr(string name, string key, int value)
        {
            var q = Q(name);
            if (q == null) return "A fila " + name + " não existe (QueueDoesNotExist).";
            if (key == "VisibilityTimeout") { if (value < 0 || value > 43200) return "VisibilityTimeout vai de 0 a 43200 segundos."; if (q.Visibility == value) return null; q.Visibility = value; }
            else if (key == "ReceiveMessageWaitTimeSeconds") { if (value < 0 || value > 20) return "ReceiveMessageWaitTimeSeconds vai de 0 a 20 segundos (InvalidAttributeValue)."; if (q.Wait == value) return null; q.Wait = value; }
            else return "Atributo não suportado neste laboratório: " + key + " (use VisibilityTimeout, ReceiveMessageWaitTimeSeconds ou RedrivePolicy).";
            Notify("sqs:attr:" + name + ":" + key + ":" + value);
            return null;
        }

        public string SetRedrive(string name, string dlq, int max)
        {
            var q = Q(name);
            if (q == null) return "A fila " + name + " não existe (QueueDoesNotExist).";
            if (dlq == null) { if (q.Dlq == null) return null; q.Dlq = null; q.MaxReceive = 0; Notify("sqs:redrive:" + name + "::0"); return null; }
            var d = Q(dlq);
            if (d == null) return "A fila de mensagens mortas " + dlq + " não existe (InvalidParameterValue).";
            if (d.Fifo != q.Fifo) return "A DLQ precisa ser do mesmo tipo da fila (padrão com padrão, FIFO com FIFO).";
            if (max < 1 || max > 1000) return "O máximo de recebimentos vai de 1 a 1000.";
            if (q.Dlq == dlq && q.MaxReceive == max) return null;
            q.Dlq = dlq; q.MaxReceive = max;
            Notify("sqs:redrive:" + name + ":" + dlq + ":" + max);
            return null;
        }

        // ── DynamoDB
        public string Scan(string table, out List<string> items)
        {
            items = null;
            var t = Tb(table);
            if (t == null) return "Tabela não encontrada: " + table + " (ResourceNotFoundException).";
            items = t.Items.ToList();
            Notify("ddb:scan:" + table);
            return null;
        }

        public string CreateTable(string name, string pk, string pkType, string sk, string skType, string billing)
        {
            name = (name ?? "").Trim(); pk = (pk ?? "").Trim(); sk = (sk ?? "").Trim();
            if (name.Length < 3) return "O nome da tabela precisa ter de 3 a 255 caracteres.";
            if (!System.Text.RegularExpressions.Regex.IsMatch(name, "^[A-Za-z0-9_.\\-]+$")) return "Nome inválido: use letras, números, _, - e .";
            if (pk.Length == 0) return "Informe a chave de partição.";
            if (Tb(name) != null) return "A tabela " + name + " já existe (ResourceInUseException).";
            var types = new[] { "S", "N", "B" };
            if (!types.Contains(pkType) || (sk.Length > 0 && !types.Contains(skType))) return "Tipo de atributo inválido (S = String, N = Número, B = Binário).";
            if (billing != "PAY_PER_REQUEST" && billing != "PROVISIONED") return "Modo de cobrança inválido (PAY_PER_REQUEST ou PROVISIONED).";
            Tables.Add(new Table { Name = name, Pk = pk, PkType = pkType, Sk = sk.Length > 0 ? sk : null, SkType = sk.Length > 0 ? skType : null, Billing = billing });
            Notify("ddb:create:" + name + ":" + pk + ":" + pkType + ":" + sk + ":" + (sk.Length > 0 ? skType : "") + ":" + billing);
            return null;
        }

        public string SetTtl(string table, string attr)
        {
            var t = Tb(table);
            if (t == null) return "Tabela não encontrada: " + table + " (ResourceNotFoundException).";
            attr = (attr ?? "").Trim();
            if (attr.Length == 0) { if (t.Ttl == null) return "O TTL já está desativado."; t.Ttl = null; Notify("ddb:ttl:" + table + ":"); return null; }
            if (t.Ttl != null && t.Ttl != attr) return "O TTL já está ativo com o atributo " + t.Ttl + ": desative antes de trocar (ValidationException).";
            t.Ttl = attr;
            Notify("ddb:ttl:" + table + ":" + attr);
            return null;
        }

        public string SetPitr(string table, bool on)
        {
            var t = Tb(table);
            if (t == null) return "Tabela não encontrada: " + table + " (TableNotFoundException).";
            t.Pitr = on;
            Notify("ddb:pitr:" + table + ":" + (on ? "on" : "off"));
            return null;
        }

        public string SetTableProtect(string table, bool on)
        {
            var t = Tb(table);
            if (t == null) return "Tabela não encontrada: " + table + " (ResourceNotFoundException).";
            t.Protect = on;
            Notify("ddb:protect:" + table + ":" + (on ? "on" : "off"));
            return null;
        }

        public string DeleteTable(string table)
        {
            var t = Tb(table);
            if (t == null) return "Tabela não encontrada: " + table + " (ResourceNotFoundException).";
            if (t.Protect) return "A tabela " + table + " tem proteção contra exclusão ativada (ValidationException).";
            Tables.Remove(t);
            Notify("ddb:delete:" + table);
            return null;
        }

        // ── RDS
        public string DbAction(string id, string action)
        {
            var d = DbI(id);
            if (d == null) return "Banco não encontrado: " + id + " (DBInstanceNotFound).";
            if (action == "stop") { if (d.Status != "Disponível") return "O banco precisa estar Disponível para ser interrompido (InvalidDBInstanceState)."; d.Status = "Interrompido"; }
            else if (action == "start") { if (d.Status != "Interrompido") return "O banco não está interrompido (InvalidDBInstanceState)."; d.Status = "Disponível"; }
            else if (action == "reboot") { if (d.Status != "Disponível") return "O banco precisa estar Disponível (InvalidDBInstanceState)."; }
            else if (action == "delete")
            {
                if (d.Protect) return "O banco " + id + " tem proteção contra exclusão (InvalidParameterCombination). Desative a proteção antes.";
                Dbs.Remove(d);
            }
            Notify("rds:" + action + ":" + id);
            return null;
        }

        public string DbSnapshot(string id, string snap)
        {
            var d = DbI(id);
            if (d == null) return "Banco não encontrado: " + id + " (DBInstanceNotFound).";
            snap = (snap ?? "").Trim();
            if (!System.Text.RegularExpressions.Regex.IsMatch(snap, "^[a-zA-Z][a-zA-Z0-9\\-]{0,254}$") || snap.Contains("--") || snap.EndsWith("-")) return "Identificador de snapshot inválido: comece com letra; só letras, números e hífens (sem '--' e sem terminar em '-').";
            if (DbSnaps.Any(x => x.Id == snap)) return "Já existe um snapshot " + snap + " (DBSnapshotAlreadyExists).";
            if (d.Status != "Disponível") return "O banco precisa estar Disponível para tirar snapshot (InvalidDBInstanceState).";
            DbSnaps.Add(new DbSnap { Id = snap, Db = id, Type = "Manual" });
            Notify("rds:snapshot:" + id + ":" + snap);
            return null;
        }

        // changes: public, multiaz, protect (true/false), backup (dias), class
        public string ModifyDb(string id, Dictionary<string, string> changes, bool immediate)
        {
            var d = DbI(id);
            if (d == null) return "Banco não encontrado: " + id + " (DBInstanceNotFound).";
            if (changes.Count == 0) return "Nenhuma modificação informada.";
            string bk;
            int bd = 0;
            if (changes.TryGetValue("backup", out bk) && (!int.TryParse(bk, out bd) || bd < 0 || bd > 35)) return "O período de retenção de backup vai de 0 a 35 dias.";
            if (d.Status != "Disponível") return "O banco precisa estar Disponível para ser modificado (InvalidDBInstanceState).";
            var real = changes.Where(kv => DbValue(d, kv.Key) != kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
            if (real.Count == 0) return "Nada mudou: os valores escolhidos já são os atuais.";
            if (immediate) foreach (var kv in real) ApplyDb(d, kv.Key, kv.Value);
            else foreach (var kv in real) d.Pending[kv.Key] = kv.Value;
            Notify("rds:modify:" + id + ":" + string.Join(",", real.Select(kv => kv.Key + "=" + kv.Value)) + ":" + (immediate ? "now" : "window"));
            return null;
        }

        public static string DbValue(Db d, string k)
        {
            switch (k)
            {
                case "public": return d.Public ? "true" : "false";
                case "multiaz": return d.MultiAz ? "true" : "false";
                case "protect": return d.Protect ? "true" : "false";
                case "backup": return d.Backup.ToString();
                case "class": return d.Class;
            }
            return null;
        }

        public static void ApplyDb(Db d, string k, string v)
        {
            switch (k)
            {
                case "public": d.Public = v == "true"; break;
                case "multiaz": d.MultiAz = v == "true"; break;
                case "protect": d.Protect = v == "true"; break;
                case "backup": d.Backup = int.Parse(v); break;
                case "class": d.Class = v; break;
            }
            d.Pending.Remove(k);
        }

        // ── Route 53
        public static string Fqdn(string n)
        {
            n = (n ?? "").Trim().TrimEnd('.').ToLowerInvariant();
            if (n.Length == 0 || n == "@") return Zone;
            return n == Zone || n.EndsWith("." + Zone) ? n : n + "." + Zone;
        }

        public string ChangeRecord(string action, string name, string type, string value, int ttl)
        {
            action = (action ?? "").ToUpperInvariant(); type = (type ?? "").Trim().ToUpperInvariant(); value = (value ?? "").Trim().TrimEnd('.');
            string fq = Fqdn(name);
            var types = new[] { "A", "AAAA", "CNAME", "MX", "TXT" };
            if (!types.Contains(type)) return "Tipo de registro não suportado aqui: " + type + " (A, AAAA, CNAME, MX, TXT).";
            var ex = Records.FirstOrDefault(r => r.Name == fq && r.Type == type);
            if (action == "DELETE")
            {
                if (ex == null) return "InvalidChangeBatch: o registro " + fq + " " + type + " não existe.";
                if (ex.Value != value || ex.Ttl != ttl) return "InvalidChangeBatch: para excluir, informe EXATAMENTE os dados atuais do registro (valor " + ex.Value + ", TTL " + ex.Ttl + ").";
                Records.Remove(ex);
                Notify("r53:delete:" + fq + ":" + type + ":" + value + ":" + ttl);
                return null;
            }
            if (action != "CREATE" && action != "UPSERT") return "Ação inválida: " + action + " (CREATE, UPSERT ou DELETE).";
            if (value.Length == 0) return "Informe o valor do registro.";
            if (ttl < 0 || ttl > 2147483647) return "TTL inválido.";
            if (type == "A") { var o = value.Split('.'); int b; if (o.Length != 4 || !o.All(x => int.TryParse(x, out b) && b >= 0 && b <= 255)) return "Um registro A precisa de um endereço IPv4 (ex.: 203.0.113.10)."; }
            if (type == "CNAME" && fq == Zone) return "Não é permitido CNAME no domínio raiz (use A/alias).";
            if (type == "CNAME" && Records.Any(r => r.Name == fq && r.Type != "CNAME")) return "Já existe outro registro com esse nome: um CNAME não pode coexistir com outros tipos.";
            if (type != "CNAME" && Records.Any(r => r.Name == fq && r.Type == "CNAME")) return "Esse nome já tem um CNAME: não pode ter outros registros.";
            if (action == "CREATE" && ex != null) return "InvalidChangeBatch: o registro " + fq + " " + type + " já existe (use UPSERT para alterar).";
            if (ex == null) { ex = new DnsRecord { Name = fq, Type = type }; Records.Add(ex); }
            ex.Value = value; ex.Ttl = ttl;
            Notify("r53:" + (action == "CREATE" ? "create" : "upsert") + ":" + fq + ":" + type + ":" + value + ":" + ttl);
            return null;
        }

        // ── CloudFront
        public Dist Distr(string id) { return Dists.FirstOrDefault(x => x.Id == id); }

        public string Invalidate(string dist, List<string> paths)
        {
            var d = Distr(dist);
            if (d == null) return "Distribuição não encontrada: " + dist + " (NoSuchDistribution).";
            paths = paths.Select(p => p.Trim()).Where(p => p.Length > 0).ToList();
            if (paths.Count == 0) return "Informe pelo menos um caminho.";
            var bad = paths.FirstOrDefault(p => !p.StartsWith("/"));
            if (bad != null) return "Caminho inválido: '" + bad + "' — todo caminho começa com / (ex.: /index.html) (InvalidArgument).";
            d.Invalidations.Insert(0, string.Join(" ", paths));
            Notify("cf:invalidate:" + dist + ":" + string.Join(",", paths));
            return null;
        }

        public static readonly string[][] ViewerPolicies = { new[] { "allow-all", "HTTP e HTTPS" }, new[] { "redirect-to-https", "Redirecionar HTTP para HTTPS" }, new[] { "https-only", "Somente HTTPS" } };

        public string SetViewerPolicy(string dist, string policy)
        {
            var d = Distr(dist);
            if (d == null) return "Distribuição não encontrada: " + dist;
            if (!ViewerPolicies.Any(v => v[0] == policy)) return "Política inválida: " + policy;
            if (d.Viewer == policy) return null;
            d.Viewer = policy;
            Notify("cf:viewer:" + dist + ":" + policy);
            return null;
        }

        public string SetRootObject(string dist, string obj)
        {
            var d = Distr(dist);
            if (d == null) return "Distribuição não encontrada: " + dist;
            obj = (obj ?? "").Trim();
            if (obj.StartsWith("/")) return "O objeto raiz padrão é só o nome, sem a barra inicial (ex.: index.html).";
            if (d.Root == obj) return null;
            d.Root = obj;
            Notify("cf:root:" + dist + ":" + obj);
            return null;
        }

        // ── CloudTrail
        public string CreateTrail(string name, string bucket, bool multi, bool validation, bool start)
        {
            name = (name ?? "").Trim();
            if (name.Length < 3 || !System.Text.RegularExpressions.Regex.IsMatch(name, "^[A-Za-z0-9._\\-]+$")) return "Nome de trilha inválido (3 a 128 caracteres: letras, números, ., _ e -).";
            if (Trails.Any(t => t.Name == name)) return "A trilha " + name + " já existe (TrailAlreadyExistsException).";
            if (Bkt(bucket) == null) return "O bucket " + bucket + " não existe (S3BucketDoesNotExistException).";
            Trails.Add(new Trail { Name = name, Bucket = bucket, MultiRegion = multi, Validation = validation });
            Notify("ct:create:" + name + ":" + bucket + ":" + (multi ? "multi" : "single") + ":" + (validation ? "valid" : "novalid"));
            if (start) StartLogging(name);
            return null;
        }

        public string StartLogging(string name)
        {
            var t = Trails.FirstOrDefault(x => x.Name == name);
            if (t == null) return "Trilha não encontrada: " + name + " (TrailNotFoundException).";
            t.Logging = true;
            Notify("ct:start:" + name);
            return null;
        }

        public string StopLogging(string name)
        {
            var t = Trails.FirstOrDefault(x => x.Name == name);
            if (t == null) return "Trilha não encontrada: " + name + " (TrailNotFoundException).";
            t.Logging = false;
            Notify("ct:stop:" + name);
            return null;
        }

        public string DeleteTrail(string name)
        {
            var t = Trails.FirstOrDefault(x => x.Name == name);
            if (t == null) return "Trilha não encontrada: " + name + " (TrailNotFoundException).";
            Trails.Remove(t);
            Notify("ct:delete:" + name);
            return null;
        }

        // ── KMS
        public bool Rotation(string alias) { bool b; return KeyRotation.TryGetValue(alias, out b) && b; }

        public string SetRotation(string alias, bool on)
        {
            if (!KmsKeys.Any(k => k[0] == alias)) return "Chave não encontrada: " + alias + " (NotFoundException).";
            if (alias == "alias/aws/s3") return "Chaves gerenciadas pela AWS já são alternadas automaticamente pela AWS: não dá para mudar (UnsupportedOperationException).";
            KeyRotation[alias] = on;
            Notify("kms:rotation:" + alias + ":" + (on ? "on" : "off"));
            return null;
        }

        // ── Budgets
        public string CreateBudget(string name, float amount)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0 || name.Contains(":")) return "Informe o nome do orçamento (sem ':').";
            if (amount <= 0) return "O valor do orçamento precisa ser maior que zero.";
            if (Budgets.Any(b => b.Name == name)) return "Já existe um orçamento " + name + " (DuplicateRecordException).";
            Budgets.Add(new Budget { Name = name, Amount = amount });
            Notify("budget:create:" + name + ":" + amount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return null;
        }

        public string AddBudgetAlert(string budget, string type, float threshold, string email)
        {
            var b = Budgets.FirstOrDefault(x => x.Name == budget);
            if (b == null) return "Orçamento não encontrado: " + budget + " (NotFoundException).";
            type = (type ?? "ACTUAL").ToUpperInvariant();
            if (type != "ACTUAL" && type != "FORECASTED") return "Tipo de alerta inválido (ACTUAL = custo real; FORECASTED = previsto).";
            if (threshold <= 0 || threshold > 1000) return "Limite de alerta inválido (em % do orçamento).";
            email = (email ?? "").Trim();
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, "^[^@\\s]+@[^@\\s]+\\.[a-z]{2,}$")) return "E-mail inválido: " + email;
            string th = threshold.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (b.Alerts.Any(a => a[0] == type && a[1] == th)) return "Já existe um alerta igual (DuplicateRecordException).";
            b.Alerts.Add(new[] { type, th, email });
            Notify("budget:alert:" + budget + ":" + type + ":" + th + ":" + email);
            return null;
        }

        // ── ECR: imutabilidade, verificação e ciclo de vida em repositório existente
        public Repo RepoOf(string n) { return Repos.FirstOrDefault(r => r.Name == n); }

        public string SetImmutable(string repo, bool on)
        {
            var r = RepoOf(repo);
            if (r == null) return "Repositório não encontrado: " + repo + " (RepositoryNotFoundException).";
            r.Immutable = on;
            Notify("ecr:immutable:" + repo + ":" + (on ? "IMMUTABLE" : "MUTABLE"));
            return null;
        }

        public string SetScan(string repo, bool on)
        {
            var r = RepoOf(repo);
            if (r == null) return "Repositório não encontrado: " + repo + " (RepositoryNotFoundException).";
            r.ScanOnPush = on;
            Notify("ecr:scan:" + repo + ":" + (on ? "on" : "off"));
            return null;
        }

        public string SetEcrLifecycle(string repo, string tagStatus, string countType, int n)
        {
            var r = RepoOf(repo);
            if (r == null) return "Repositório não encontrado: " + repo + " (RepositoryNotFoundException).";
            if (tagStatus != "tagged" && tagStatus != "untagged" && tagStatus != "any") return "tagStatus inválido (tagged, untagged ou any).";
            if (countType != "imageCountMoreThan" && countType != "sinceImagePushed") return "countType inválido (imageCountMoreThan ou sinceImagePushed).";
            if (tagStatus == "tagged") return "Para tagStatus tagged é preciso informar tagPrefixList (não usado neste laboratório): use untagged ou any.";
            if (n < 1) return "countNumber precisa ser maior que zero.";
            r.LcTag = tagStatus; r.LcType = countType; r.LcN = n;
            Notify("ecr:lifecycle:" + repo + ":" + tagStatus + ":" + countType + ":" + n);
            return null;
        }

        // ───────────── CloudShell: interpretador simplificado da AWS CLI ─────────────

        static Dictionary<string, string> Flags(List<string> t, int start, List<string> pos)
        {
            var d = new Dictionary<string, string>();
            for (int i = start; i < t.Count; i++)
            {
                string a = t[i];
                if (a.StartsWith("--"))
                {
                    string k = a.Substring(2), v = "";
                    int eq = k.IndexOf('=');
                    if (eq > 0) { v = k.Substring(eq + 1); k = k.Substring(0, eq); }
                    else if (i + 1 < t.Count && !t[i + 1].StartsWith("--")) v = t[++i];
                    d[k] = v;
                }
                else pos.Add(a);
            }
            return d;
        }

        static string Get(Dictionary<string, string> d, string k) { string v; return d.TryGetValue(k, out v) ? v : null; }
        static string S3Name(string uri) { if (uri == null) return null; uri = uri.Trim(); if (uri.StartsWith("s3://")) uri = uri.Substring(5); return uri.TrimEnd('/').Split('/')[0]; }

        // Executa um comando e devolve a saída. 'ok' indica se o comando foi reconhecido e executado.
        public string Run(string cmd, out bool ok)
        {
            ok = false;
            var t = CommandParser.Tokenize(cmd ?? "").ToList();
            if (t.Count == 0) return "";
            if (t[0] == "clear") { ok = true; return ""; }
            if (t[0] == "ls") { ok = true; return string.Join("  ", LocalFiles) + "  site/  infra.yaml  rede.yaml\n" + string.Join("  ", LocalJson.Select(j => j[0])); }
            if (t[0] == "pwd") { ok = true; return "/home/cloudshell-user"; }
            if (t[0] == "cat")
            {
                if (t.Count < 2) return "cat: informe o arquivo (ex.: cat confianca-ec2.json)";
                string c = JsonFile(t[1]);
                if (c != null) { ok = true; return PrettyJson(c); }
                if (LocalFiles.Contains(t[1].TrimStart('.', '/'))) { ok = true; return "(" + t[1] + ": arquivo do site — conteúdo omitido)"; }
                return "cat: " + t[1] + ": Arquivo ou diretório inexistente";
            }
            if (t[0] != "aws") return "bash: " + t[0] + ": comando não encontrado (no CloudShell, use a AWS CLI: comandos começam com 'aws')";
            if (t.Count == 1 || t[1] == "help") return "usage: aws [options] <command> <subcommand> [parameters]\nServiços disponíveis aqui: " + CliServices + "\nOpção global: --region <região> (vale só para aquele comando)\nArquivos JSON da pasta (ls / cat) podem ser passados como file://arquivo.json";
            var pos = new List<string>();
            string svc = t[1], sub = t.Count > 2 ? t[2] : "";
            var fl = Flags(t, 3, pos);
            var sb = new StringBuilder();
            // --region: vale só para este comando (sem mudar a região padrão)
            string rg = Get(fl, "region");
            if (rg != null && !Regions.Any(r => r[0] == rg)) return "Região desconhecida: " + rg + " (ex.: us-east-1, sa-east-1)";
            if (rg == null) rg = Region;
            {
                // recursos da 2ª leva: a região do comando vale durante a execução
                string saved = Region;
                bool handled;
                string extra;
                Region = rg; muteChanged = true; pendingChanged = false;
                try { extra = RunExtra(svc, sub, fl, pos, t, out ok, out handled); }
                finally { Region = saved; muteChanged = false; }
                if (pendingChanged && Changed != null) Changed();
                if (handled) return extra;
            }
            switch (svc)
            {
                case "configure":
                    if (sub == "set" && pos.Count >= 2 && pos[0] == "region")
                    {
                        if (!Regions.Any(r => r[0] == pos[1])) return "Região desconhecida: " + pos[1];
                        ok = true; SetRegion(pos[1]); return "";
                    }
                    if (sub == "list") { ok = true; return "      Name                    Value             Type    Location\n    region                " + Region + "         config-file"; }
                    ok = true;
                    return "No CloudShell as credenciais já vêm configuradas pela sua sessão do console.\nPara trocar a região padrão: aws configure set region <região>";
                case "sts":
                    if (sub != "get-caller-identity") break;
                    ok = true; Identity();
                    return "{\n    \"UserId\": \"AIDA5ESTAGIO2026\",\n    \"Account\": \"" + AccountId.Replace("-", "") + "\",\n    \"Arn\": \"arn:aws:iam::" + AccountId.Replace("-", "") + ":user/estagio\"\n}";
                case "s3":
                    switch (sub)
                    {
                        case "ls":
                            ok = true; Open("s3");
                            if (pos.Count == 0) { foreach (var b in Buckets) sb.AppendLine(b.Created.ToString("yyyy-MM-dd HH:mm:ss") + " " + b.Name); return sb.ToString().TrimEnd(); }
                            {
                                var b = Buckets.FirstOrDefault(x => x.Name == S3Name(pos[0]));
                                if (b == null) { ok = false; return "An error occurred (NoSuchBucket) when calling the ListObjectsV2 operation: The specified bucket does not exist"; }
                                foreach (var o in b.Objects) sb.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "       2048 " + o);
                                return sb.ToString().TrimEnd();
                            }
                        case "mb":
                            {
                                if (pos.Count == 0) return "aws: error: the following arguments are required: path";
                                string n = S3Name(pos[0]);
                                var err = CreateBucket(n, rg, true);
                                if (err != null) return "make_bucket failed: s3://" + n + " " + err;
                                ok = true; return "make_bucket: " + n;
                            }
                        case "rb":
                            {
                                if (pos.Count == 0) return "aws: error: the following arguments are required: path";
                                string n = S3Name(pos[0]);
                                var b = Buckets.FirstOrDefault(x => x.Name == n);
                                if (b == null) return "remove_bucket failed: s3://" + n + " NoSuchBucket";
                                if (b.Objects.Count > 0 && !fl.ContainsKey("force")) return "remove_bucket failed: s3://" + n + " BucketNotEmpty (use --force para apagar os objetos junto)";
                                ok = true; DeleteBucket(n); return "remove_bucket: " + n;
                            }
                        case "rm":
                            {
                                if (pos.Count == 0) return "aws: error: the following arguments are required: paths";
                                string uri = pos[0].Trim();
                                if (!uri.StartsWith("s3://")) return "Informe o objeto no formato s3://bucket/chave";
                                string n = S3Name(uri);
                                string rest = uri.Substring(5);
                                string key = rest.Length > n.Length + 1 ? rest.Substring(n.Length + 1) : "";
                                var b = Buckets.FirstOrDefault(x => x.Name == n);
                                if (b == null) return "delete failed: s3://" + n + " An error occurred (NoSuchBucket)";
                                if (fl.ContainsKey("recursive"))
                                {
                                    var del = b.Objects.Where(o => o.StartsWith(key)).ToList();
                                    foreach (var o in del) sb.AppendLine("delete: s3://" + n + "/" + o);
                                    DeleteObjects(n, del);
                                    ok = true; return del.Count == 0 ? "(nenhum objeto com esse prefixo)" : sb.ToString().TrimEnd();
                                }
                                if (key.Length == 0) return "Informe a chave do objeto: aws s3 rm s3://" + n + "/<arquivo>  (para apagar tudo seria --recursive: cuidado!)";
                                if (!b.Objects.Contains(key)) return "delete failed: s3://" + n + "/" + key + " (NoSuchKey: o objeto não existe)";
                                DeleteObject(n, key);
                                ok = true; return "delete: s3://" + n + "/" + key;
                            }
                        case "cp":
                            {
                                if (pos.Count < 2) return "aws: error: the following arguments are required: paths";
                                string src = pos[0].TrimStart('.', '/'), dst = pos[1];
                                if (!dst.StartsWith("s3://")) return "Neste laboratório, envie arquivos para um bucket: aws s3 cp <arquivo> s3://<bucket>/";
                                if (!LocalFiles.Contains(src)) return "The user-provided path " + pos[0] + " does not exist.";
                                string n = S3Name(dst);
                                var err = Upload(n, new[] { src });
                                if (err != null) return "upload failed: " + src + " to " + dst + " " + err;
                                ok = true; return "upload: ./" + src + " to s3://" + n + "/" + src;
                            }
                        case "sync":
                            {
                                if (pos.Count < 2) return "aws: error: the following arguments are required: paths";
                                string src = pos[0].Trim('.', '/'), dst = pos[1];
                                if (src != "site") return "The user-provided path " + pos[0] + " does not exist. (A pasta do site é ./site)";
                                string n = S3Name(dst);
                                var b = Buckets.FirstOrDefault(x => x.Name == n);
                                if (b == null) return "fatal error: An error occurred (NoSuchBucket) when calling the ListObjectsV2 operation";
                                var news = SiteFiles.Where(f => !b.Objects.Contains(f)).ToList();
                                foreach (var f in news) sb.AppendLine("upload: site/" + f + " to s3://" + n + "/" + f);
                                Upload(n, news);
                                ok = true; return news.Count == 0 ? "(nada a enviar: tudo já está sincronizado)" : sb.ToString().TrimEnd();
                            }
                    }
                    break;
                case "ec2":
                    switch (sub)
                    {
                        case "describe-instances":
                            ok = true; Open("ec2");
                            sb.AppendLine("{\n    \"Reservations\": [");
                            foreach (var i in Instances.Where(x => x.Region == rg))
                                sb.AppendLine("        { \"InstanceId\": \"" + i.Id + "\", \"InstanceType\": \"" + i.Type + "\", \"State\": \"" + StateEn(i.State) + "\", \"SecurityGroup\": \"" + i.Sg + "\", \"Tags\": [" + string.Join(", ", new[] { "{ \"Key\": \"Name\", \"Value\": \"" + i.Name + "\" }" }.Concat(i.Tags.Select(kv => "{ \"Key\": \"" + kv.Key + "\", \"Value\": \"" + kv.Value + "\" }"))) + "] },");
                            sb.Append("    ]\n}");
                            return sb.ToString();
                        case "create-tags":
                            {
                                string res = Get(fl, "resources"), tags = Get(fl, "tags");
                                if (string.IsNullOrEmpty(res) || string.IsNullOrEmpty(tags)) return "aws: error: the following arguments are required: --resources, --tags";
                                var m = System.Text.RegularExpressions.Regex.Match(tags, "Key\"?\\s*[:=]\\s*\"?([^\",}\\s]+)\"?\\s*,\\s*\"?Value\"?\\s*[:=]\\s*\"?([^\",}\\]\\s]*)");
                                if (!m.Success) return "Parameter validation failed: use --tags Key=CHAVE,Value=VALOR";
                                var i = Instances.FirstOrDefault(x => x.Id == res && x.Region == rg);
                                if (i == null) return "An error occurred (InvalidID): The ID '" + res + "' is not valid or does not exist (confira a região: " + rg + ")";
                                var err = AddTag(res, m.Groups[1].Value, m.Groups[2].Value);
                                if (err != null) return "An error occurred: " + err;
                                ok = true; return "";
                            }
                        case "modify-instance-attribute":
                            {
                                string id = Get(fl, "instance-id"), it = Get(fl, "instance-type");
                                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(it)) return "aws: error: the following arguments are required: --instance-id, --instance-type";
                                var m = System.Text.RegularExpressions.Regex.Match(it, "[a-z][0-9][a-z]*\\.[0-9]*[a-z]+");
                                var i = Instances.FirstOrDefault(x => x.Id == id && x.Region == rg);
                                if (i == null) return "An error occurred (InvalidInstanceID.NotFound): The instance ID '" + id + "' does not exist (confira a região: " + rg + ")";
                                var err = ModifyType(id, m.Success ? m.Value : it);
                                if (err != null) return "An error occurred: " + err;
                                ok = true; return "";
                            }
                        case "describe-volumes":
                            ok = true; Open("ec2");
                            foreach (var v in Volumes.Where(x => x.Region == rg))
                                sb.AppendLine("{ \"VolumeId\": \"" + v.Id + "\", \"Size\": " + v.Size + ", \"VolumeType\": \"" + v.Type + "\", \"State\": \"" + (v.Instance != null ? "in-use" : "available") + "\", \"Attachments\": [" + (v.Instance != null ? "{ \"InstanceId\": \"" + v.Instance + "\" }" : "") + "] }");
                            return sb.Length == 0 ? "{ \"Volumes\": [] }" : sb.ToString().TrimEnd();
                        case "describe-snapshots":
                            ok = true;
                            foreach (var sn in Snapshots.Where(x => x.Region == rg))
                                sb.AppendLine("{ \"SnapshotId\": \"" + sn.Id + "\", \"VolumeId\": \"" + sn.Volume + "\", \"Description\": \"" + sn.Description + "\", \"State\": \"completed\" }");
                            return sb.Length == 0 ? "{ \"Snapshots\": [] }" : sb.ToString().TrimEnd();
                        case "create-snapshot":
                            {
                                string vid = Get(fl, "volume-id");
                                if (string.IsNullOrEmpty(vid)) return "aws: error: the following arguments are required: --volume-id";
                                var v = Volumes.FirstOrDefault(x => x.Id == vid && x.Region == rg);
                                if (v == null) return "An error occurred (InvalidVolume.NotFound): The volume '" + vid + "' does not exist (confira a região: " + rg + ")";
                                var err = CreateSnapshot(vid, Get(fl, "description"));
                                if (err != null) return "An error occurred: " + err;
                                var last = Snapshots.Last();
                                ok = true; return "{\n    \"SnapshotId\": \"" + last.Id + "\",\n    \"VolumeId\": \"" + vid + "\",\n    \"Description\": \"" + last.Description + "\",\n    \"State\": \"pending\"\n}";
                            }
                        case "delete-volume":
                            {
                                string vid = Get(fl, "volume-id");
                                if (string.IsNullOrEmpty(vid)) return "aws: error: the following arguments are required: --volume-id";
                                var v = Volumes.FirstOrDefault(x => x.Id == vid && x.Region == rg);
                                if (v == null) return "An error occurred (InvalidVolume.NotFound): The volume '" + vid + "' does not exist (confira a região: " + rg + ")";
                                var err = DeleteVolume(vid);
                                if (err != null) return "An error occurred: " + err;
                                ok = true; return "";
                            }
                        case "describe-security-groups":
                            ok = true; Open("ec2");
                            foreach (var g in SecGroups.Where(x => x.Region == rg))
                                sb.AppendLine("{ \"GroupId\": \"" + g.Id + "\", \"GroupName\": \"" + g.Name + "\", \"IpPermissions\": [" + string.Join(", ", g.Inbound.Select(r => "{ \"IpProtocol\": \"tcp\", \"FromPort\": " + r.Port + ", \"ToPort\": " + r.Port + ", " + (r.Source.StartsWith("sg-") ? "\"UserIdGroupPairs\": [{ \"GroupId\": \"" + r.Source + "\" }]" : "\"IpRanges\": [{ \"CidrIp\": \"" + r.Source + "\" }]") + " }")) + "] }");
                            return sb.ToString().TrimEnd();
                        case "authorize-security-group-ingress":
                        case "revoke-security-group-ingress":
                            {
                                string gid = Get(fl, "group-id") ?? Get(fl, "group-name"), proto = Get(fl, "protocol"), port = Get(fl, "port");
                                string src = Get(fl, "cidr") ?? Get(fl, "source-group");
                                if (string.IsNullOrEmpty(gid) || string.IsNullOrEmpty(proto) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(src))
                                    return "aws: error: the following arguments are required: --group-id, --protocol, --port e (--cidr ou --source-group)";
                                if (proto != "tcp") return "Neste laboratório, as regras são TCP: use --protocol tcp";
                                int pn;
                                if (!int.TryParse(port, out pn)) return "Parameter validation failed: --port deve ser um número (ex.: 443)";
                                var g = SecGroups.FirstOrDefault(x => (x.Id == gid || x.Name == gid) && x.Region == rg);
                                if (g == null) return "An error occurred (InvalidGroup.NotFound): The security group '" + gid + "' does not exist (confira a região: " + rg + ")";
                                if (fl.ContainsKey("cidr") && fl.ContainsKey("source-group")) return "Use --cidr OU --source-group, não os dois.";
                                if (fl.ContainsKey("source-group") && !src.StartsWith("sg-")) return "--source-group espera o ID de um grupo de segurança (sg-...)";
                                if (fl.ContainsKey("cidr") && !IsCidr(src)) return "Parameter validation failed: CIDR inválido '" + src + "' (ex.: 203.0.113.5/32)";
                                var err = sub.StartsWith("authorize") ? Authorize(g.Id, pn, src) : Revoke(g.Id, pn, src);
                                if (err != null) return "An error occurred: " + err;
                                ok = true; return sub.StartsWith("authorize") ? "{\n    \"Return\": true,\n    \"SecurityGroupRules\": [{ \"GroupId\": \"" + g.Id + "\", \"IsEgress\": false, \"IpProtocol\": \"tcp\", \"FromPort\": " + pn + ", \"ToPort\": " + pn + " }]\n}" : "{\n    \"Return\": true\n}";
                            }
                        case "stop-instances":
                        case "start-instances":
                        case "terminate-instances":
                        case "reboot-instances":
                            {
                                string ids = Get(fl, "instance-ids");
                                if (string.IsNullOrEmpty(ids)) return "aws: error: the following arguments are required: --instance-ids";
                                var i = Instances.FirstOrDefault(x => x.Id == ids && x.Region == rg);
                                if (i == null) return "An error occurred (InvalidInstanceID.NotFound): The instance ID '" + ids + "' does not exist (confira a região: " + rg + ")";
                                string act = sub.Split('-')[0];
                                string before = StateEn(i.State);
                                var stErr = SetState(ids, act);
                                if (stErr != null) return "An error occurred (OperationNotPermitted): " + stErr;
                                ok = true;
                                return "{\n    \"Instances\": [{ \"InstanceId\": \"" + ids + "\", \"PreviousState\": \"" + before + "\", \"CurrentState\": \"" + StateEn(i.State) + "\" }]\n}";
                            }
                    }
                    break;
                case "iam":
                    switch (sub)
                    {
                        case "list-users":
                            ok = true; Open("iam");
                            foreach (var u in Users) sb.AppendLine("{ \"UserName\": \"" + u.Name + "\", \"Arn\": \"arn:aws:iam::123456789012:user/" + u.Name + "\" }");
                            return sb.ToString().TrimEnd();
                        case "list-groups":
                            ok = true;
                            return string.Join("\n", Groups.Select(gp => "{ \"GroupName\": \"" + gp + "\" }"));
                        case "create-user":
                            {
                                string n = Get(fl, "user-name");
                                if (string.IsNullOrEmpty(n)) return "aws: error: the following arguments are required: --user-name";
                                var err = CreateUser(n, null);
                                if (err != null) return "An error occurred: " + err;
                                ok = true; return "{ \"User\": { \"UserName\": \"" + n + "\", \"Arn\": \"arn:aws:iam::123456789012:user/" + n + "\" } }";
                            }
                        case "add-user-to-group":
                            {
                                string n = Get(fl, "user-name"), gname = Get(fl, "group-name");
                                if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(gname)) return "aws: error: the following arguments are required: --user-name, --group-name";
                                var err = AddToGroup(n, gname);
                                if (err != null) return "An error occurred: " + err;
                                ok = true; return "";
                            }
                        case "list-policies":
                            ok = true; Open("iam");
                            return string.Join("\n", ManagedPolicies.Select(p => "{ \"PolicyName\": \"" + p[0] + "\", \"Arn\": \"" + PolicyArn(p[0]) + "\" }   # " + p[1]));
                        case "attach-user-policy":
                        case "detach-user-policy":
                            {
                                string n = Get(fl, "user-name"), arn = Get(fl, "policy-arn");
                                if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(arn)) return "aws: error: the following arguments are required: --user-name, --policy-arn";
                                const string pre = "arn:aws:iam::aws:policy/";
                                if (!arn.StartsWith(pre)) return "An error occurred (InvalidInput): --policy-arn precisa ser o ARN completo, ex.: " + pre + "ReadOnlyAccess";
                                var err = sub.StartsWith("attach") ? AttachPolicy(n, PolicyFromArn(arn)) : DetachPolicy(n, PolicyFromArn(arn));
                                if (err != null) return "An error occurred: " + err;
                                ok = true; return "";
                            }
                        case "list-attached-user-policies":
                            {
                                string n = Get(fl, "user-name");
                                var u = Users.FirstOrDefault(x => x.Name == n);
                                if (u == null) return "An error occurred (NoSuchEntity): The user with name " + n + " cannot be found.";
                                ok = true;
                                return "{ \"AttachedPolicies\": [" + string.Join(", ", u.Policies.Select(p => "{ \"PolicyName\": \"" + p + "\" }")) + "] }";
                            }
                    }
                    break;
                case "lambda":
                    if (sub == "list-functions") { ok = true; Open("lambda"); return string.Join("\n", Functions.Select(f => "{ \"FunctionName\": \"" + f.Name + "\", \"Runtime\": \"" + f.Runtime + "\" }")); }
                    if (sub == "invoke")
                    {
                        string n = Get(fl, "function-name");
                        if (string.IsNullOrEmpty(n)) return "aws: error: the following arguments are required: --function-name";
                        if (pos.Count == 0) return "aws: error: the following arguments are required: outfile";
                        var r = Invoke(n);
                        if (r == null) return "An error occurred (ResourceNotFoundException): Function not found: " + n;
                        ok = true; return "{\n    \"StatusCode\": 200,\n    \"ExecutedVersion\": \"$LATEST\"\n}\n(resposta salva em " + pos[0] + ": " + r + ")";
                    }
                    break;
                case "logs":
                    if (sub == "tail" && pos.Count == 0 && !string.IsNullOrEmpty(Get(fl, "follow"))) pos.Add(Get(fl, "follow"));
                    if (sub == "describe-log-groups") { ok = true; return string.Join("\n", LogGroups.Select(gname => "{ \"logGroupName\": \"" + gname + "\"" + (Retention(gname) > 0 ? ", \"retentionInDays\": " + Retention(gname) : "") + " }")); }
                    if (sub == "tail" && pos.Count > 0)
                    {
                        if (!LogGroups.Contains(pos[0])) return "An error occurred (ResourceNotFoundException): The specified log group does not exist: " + pos[0] + "\n(Grupos de logs de funções Lambda: /aws/lambda/<nome-da-função>)";
                        ok = true; OpenLogs(pos[0]);
                        return string.Join("\n", LogLines(pos[0]).Select(l => DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + " " + l));
                    }
                    break;
                case "ecr":
                    if (sub == "describe-repositories") { ok = true; Open("ecr"); return string.Join("\n", Repos.Select(r => "{ \"repositoryName\": \"" + r.Name + "\", \"imageScanningConfiguration\": { \"scanOnPush\": " + (r.ScanOnPush ? "true" : "false") + " } }")); }
                    if (sub == "create-repository")
                    {
                        string n = Get(fl, "repository-name");
                        if (string.IsNullOrEmpty(n)) return "aws: error: the following arguments are required: --repository-name";
                        string scan = Get(fl, "image-scanning-configuration") ?? "";
                        var err = CreateRepo(n, scan.Replace(" ", "").ToLowerInvariant().Contains("scanonpush=true"), (Get(fl, "image-tag-mutability") ?? "").ToUpperInvariant() == "IMMUTABLE");
                        if (err != null) return "An error occurred: " + err;
                        ok = true; return "{ \"repository\": { \"repositoryName\": \"" + n + "\", \"repositoryUri\": \"123456789012.dkr.ecr." + Region + ".amazonaws.com/" + n + "\" } }";
                    }
                    break;
                case "ecs":
                    if (sub == "list-services") { ok = true; Open("ecs"); return "{ \"serviceArns\": [ \"arn:aws:ecs:us-east-1:123456789012:service/cafe365/api\" ] }"; }
                    if (sub == "update-service")
                    {
                        if (Get(fl, "cluster") != "cafe365") return "An error occurred (ClusterNotFoundException): Cluster not found. (o cluster se chama cafe365)";
                        if (Get(fl, "service") != "api") return "An error occurred (ServiceNotFoundException): Service not found. (o serviço se chama api)";
                        string desired = Get(fl, "desired-count");
                        int dc; if (!int.TryParse(desired, out dc)) dc = EcsRunning;
                        UpdateService(fl.ContainsKey("force-new-deployment"), dc);
                        ok = true; return "{ \"service\": { \"serviceName\": \"api\", \"desiredCount\": " + EcsRunning + ", \"deployments\": " + EcsDeployments + " } }";
                    }
                    break;
                case "cloudwatch":
                    if (sub == "describe-alarms")
                    {
                        ok = true; Open("cloudwatch");
                        string sv = Get(fl, "state-value");
                        foreach (var a in Alarms.Where(x => sv == null || StateEn(x.State) == sv)) sb.AppendLine("{ \"AlarmName\": \"" + a.Name + "\", \"StateValue\": \"" + StateEn(a.State) + "\" }");
                        return sb.Length == 0 ? "{ \"MetricAlarms\": [] }" : sb.ToString().TrimEnd();
                    }
                    if (sub == "put-metric-alarm")
                    {
                        string n = Get(fl, "alarm-name"), m = Get(fl, "metric-name"), th = Get(fl, "threshold");
                        if (n == null || m == null || th == null) return "aws: error: the following arguments are required: --alarm-name, --metric-name, --threshold, ...";
                        float tv; float.TryParse(th, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out tv);
                        string dims = Get(fl, "dimensions") ?? "";
                        var inst = Instances.FirstOrDefault(i => dims.Contains(i.Id));
                        CreateAlarm(n, m, inst != null ? inst.Name : "(sem instância)", tv);
                        ok = true; return "";
                    }
                    break;
                case "cloudformation":
                    if (sub == "describe-stacks") { ok = true; Open("cloudformation"); return string.Join("\n", Stacks.Select(s => "{ \"StackName\": \"" + s.Name + "\", \"StackStatus\": \"" + s.Status + "\" }")); }
                    if (sub == "deploy")
                    {
                        string tf = Get(fl, "template-file"), sn = Get(fl, "stack-name");
                        if (tf == null || sn == null) return "aws: error: the following arguments are required: --template-file, --stack-name";
                        tf = tf.TrimStart('.', '/');
                        if (tf != "infra.yaml" && tf != "rede.yaml") return "Invalid template path " + tf;
                        bool ack = (Get(fl, "capabilities") ?? "").Contains("CAPABILITY_IAM") || (Get(fl, "capabilities") ?? "").Contains("CAPABILITY_NAMED_IAM");
                        var st = CreateStack(sn, tf, ack);
                        ok = true;
                        if (st.Status != "CREATE_COMPLETE")
                            return "Waiting for changeset to be created..\nAn error occurred (InsufficientCapabilities) when calling the CreateChangeSet operation: Requires capabilities : [CAPABILITY_IAM]";
                        return "Waiting for changeset to be created..\nWaiting for stack create/update to complete\nSuccessfully created/updated stack - " + sn;
                    }
                    break;
                case "s3api":
                    {
                        string bn = Get(fl, "bucket");
                        if (sub.EndsWith("-bucket-versioning") || sub.EndsWith("-bucket-encryption"))
                        {
                            if (string.IsNullOrEmpty(bn)) return "aws: error: the following arguments are required: --bucket";
                            var b = Buckets.FirstOrDefault(x => x.Name == bn);
                            if (b == null) return "An error occurred (NoSuchBucket) when calling the " + sub + " operation: The specified bucket does not exist";
                            switch (sub)
                            {
                                case "get-bucket-versioning":
                                    ok = true;
                                    return b.Versioning == "Desativado" ? "(sem saída: o versionamento nunca foi ativado neste bucket)" : "{\n    \"Status\": \"" + (b.Versioning == "Ativado" ? "Enabled" : "Suspended") + "\"\n}";
                                case "put-bucket-versioning":
                                    {
                                        string vc = Get(fl, "versioning-configuration");
                                        if (string.IsNullOrEmpty(vc)) return "aws: error: the following arguments are required: --versioning-configuration";
                                        var m = System.Text.RegularExpressions.Regex.Match(vc, "Status\"?\\s*[:=]\\s*\"?(Enabled|Suspended)");
                                        if (!m.Success) return "Parameter validation failed: use --versioning-configuration Status=Enabled (ou Status=Suspended)";
                                        SetVersioning(bn, m.Groups[1].Value == "Enabled");
                                        ok = true; return "";
                                    }
                                case "get-bucket-encryption":
                                    ok = true;
                                    return "{ \"ServerSideEncryptionConfiguration\": { \"Rules\": [{ \"ApplyServerSideEncryptionByDefault\": { \"SSEAlgorithm\": \"" + (b.Encryption == "SSE-KMS" ? "aws:kms\", \"KMSMasterKeyID\": \"" + b.KmsKey : "AES256") + "\" } }] } }";
                                case "put-bucket-encryption":
                                    {
                                        string cfg = Get(fl, "server-side-encryption-configuration");
                                        if (string.IsNullOrEmpty(cfg)) return "aws: error: the following arguments are required: --server-side-encryption-configuration";
                                        var alg = System.Text.RegularExpressions.Regex.Match(cfg, "SSEAlgorithm\"?\\s*[:=]\\s*\"?(aws:kms|AES256)");
                                        if (!alg.Success) return "Parameter validation failed: SSEAlgorithm deve ser AES256 (SSE-S3) ou aws:kms (SSE-KMS)";
                                        bool kms = alg.Groups[1].Value == "aws:kms";
                                        string key = null;
                                        var km = System.Text.RegularExpressions.Regex.Match(cfg, "KMSMasterKeyID\"?\\s*[:=]\\s*\"?([A-Za-z0-9/_:\\-]+)");
                                        if (km.Success) { key = km.Groups[1].Value; int ai = key.IndexOf("alias/"); if (ai > 0) key = key.Substring(ai); }
                                        var err = SetEncryption(bn, kms, key);
                                        if (err != null) return "An error occurred: " + err;
                                        ok = true; return "";
                                    }
                            }
                        }
                        break;
                    }
                default:
                    return "aws: error: argument command: Invalid choice '" + svc + "' (neste laboratório: " + CliServices + ")";
            }
            return "aws: error: argument operation: Invalid choice '" + sub + "' para o serviço " + svc + ". Dica: pesquise na Biblioteca (seção AWS).";
        }

        // ───────────── CLI da 2ª leva ─────────────
        public const string CliServices = "s3, s3api, ec2, autoscaling, iam, sts, lambda, logs, cloudwatch, sns, sqs, dynamodb, rds, route53, cloudfront, cloudtrail, kms, budgets, ecr, ecs, cloudformation, configure";

        static string Rx(string text, string pattern)
        {
            if (text == null) return null;
            var m = System.Text.RegularExpressions.Regex.Match(text, pattern);
            return m.Success ? m.Groups[1].Value : null;
        }
        // valor de uma chave em JSON ou na sintaxe abreviada (Chave=Valor)
        static string Pick(string text, string key) { return Rx(text, "(?<![A-Za-z0-9_])\"?" + key + "\"?\\s*[:=]\\s*\"?([^\",}\\]\\s\\\\]*)"); }
        static bool IsTrue(string v) { return v != null && v.Trim('"').ToLowerInvariant() == "true"; }

        // parâmetro que pode vir como texto ou como file://arquivo.json
        static string Param(Dictionary<string, string> fl, string k, out string err)
        {
            err = null;
            string v = Get(fl, k);
            if (v == null) return null;
            if (v.StartsWith("file://"))
            {
                string c = JsonFile(v);
                if (c == null) err = "Error parsing parameter '--" + k + "': Unable to load paramfile " + v + ": [Errno 2] No such file or directory (veja os arquivos com ls)";
                return c;
            }
            return v;
        }

        // todos os tokens depois de uma opção, até a próxima opção (ex.: --attribute-definitions A=1 B=2)
        static string Multi(List<string> t, string flag)
        {
            int i = t.IndexOf("--" + flag);
            if (i < 0) { var eq = t.FirstOrDefault(x => x.StartsWith("--" + flag + "=")); return eq == null ? null : eq.Substring(flag.Length + 3); }
            var parts = new List<string>();
            for (int k = i + 1; k < t.Count && !t[k].StartsWith("--"); k++) parts.Add(t[k]);
            return string.Join(" ", parts);
        }

        static string Need(params string[] flags) { return "aws: error: the following arguments are required: " + string.Join(", ", flags.Select(f => "--" + f)); }
        static string Invalid(string svc, string sub) { return "aws: error: argument operation: Invalid choice '" + sub + "' para o serviço " + svc + ". Dica: pesquise na Biblioteca (seção AWS)."; }
        static string AwsErr(string e) { return e.StartsWith("An error") || e.StartsWith("aws:") ? e : "An error occurred: " + e; }

        public static string PrettyJson(string j)
        {
            var sb = new StringBuilder();
            int ind = 0; bool str = false;
            for (int i = 0; i < j.Length; i++)
            {
                char c = j[i];
                if (str) { sb.Append(c); if (c == '\\' && i + 1 < j.Length) { sb.Append(j[++i]); } else if (c == '"') str = false; continue; }
                if (c == '"') { str = true; sb.Append(c); }
                else if (c == '{' || c == '[') { sb.Append(c).Append('\n').Append(' ', (++ind) * 2); }
                else if (c == '}' || c == ']') { sb.Append('\n').Append(' ', Math.Max(0, --ind) * 2).Append(c); }
                else if (c == ',') { sb.Append(",\n").Append(' ', ind * 2); }
                else if (c == ':') sb.Append(": ");
                else sb.Append(c);
            }
            return sb.ToString();
        }

        static string TopicFromArn(string arn)
        {
            const string pre = "arn:aws:sns:us-east-1:" + Arn + ":";
            if (arn == null) return null;
            return arn.StartsWith(pre) ? arn.Substring(pre.Length) : null;
        }
        static string QueueFromUrl(string url)
        {
            string pre = "https://sqs.us-east-1.amazonaws.com/" + Arn + "/";
            if (url == null) return null;
            return url.StartsWith(pre) ? url.Substring(pre.Length) : null;
        }
        static string QueueFromArn(string arn)
        {
            const string pre = "arn:aws:sqs:us-east-1:" + Arn + ":";
            if (arn == null) return null;
            return arn.StartsWith(pre) ? arn.Substring(pre.Length) : null;
        }

        string RunExtra(string svc, string sub, Dictionary<string, string> fl, List<string> pos, List<string> t, out bool ok, out bool handled)
        {
            ok = false; handled = true;
            switch (svc)
            {
                case "s3": if (sub == "website" || sub == "presign") return CliS3(sub, fl, pos, out ok); break;
                case "s3api": { string r = CliS3Api(sub, fl, out ok, out handled); if (handled) return r; break; }
                case "ec2": { string r = CliEc2(sub, fl, pos, t, out ok, out handled); if (handled) return r; break; }
                case "iam": { string r = CliIam(sub, fl, out ok, out handled); if (handled) return r; break; }
                case "lambda": { string r = CliLambda(sub, fl, out ok, out handled); if (handled) return r; break; }
                case "logs": { string r = CliLogs(sub, fl, out ok, out handled); if (handled) return r; break; }
                case "cloudwatch": { string r = CliCw(sub, fl, out ok, out handled); if (handled) return r; break; }
                case "ecr": { string r = CliEcr(sub, fl, out ok, out handled); if (handled) return r; break; }
                case "autoscaling": return CliAsg(sub, fl, out ok);
                case "sns": return CliSns(sub, fl, out ok);
                case "sqs": return CliSqs(sub, fl, out ok);
                case "dynamodb": return CliDdb(sub, fl, t, out ok);
                case "rds": return CliRds(sub, fl, out ok);
                case "route53": return CliR53(sub, fl, out ok);
                case "cloudfront": return CliCf(sub, fl, pos, out ok);
                case "cloudtrail": return CliCt(sub, fl, out ok);
                case "kms": return CliKms(sub, fl, out ok);
                case "budgets": return CliBudgets(sub, fl, out ok);
            }
            handled = false;
            return null;
        }

        string CliS3(string sub, Dictionary<string, string> fl, List<string> pos, out bool ok)
        {
            ok = false;
            if (pos.Count == 0) return "aws: error: the following arguments are required: " + (sub == "website" ? "paths (s3://bucket)" : "path (s3://bucket/objeto)");
            string uri = pos[0].Trim();
            if (!uri.StartsWith("s3://")) return "Informe no formato s3://bucket" + (sub == "presign" ? "/objeto" : "");
            string n = S3Name(uri);
            if (sub == "website")
            {
                string idx = Get(fl, "index-document");
                if (string.IsNullOrEmpty(idx)) return "Informe --index-document (ex.: index.html)";
                var e = SetWebsite(n, idx, Get(fl, "error-document"));
                if (e != null) return AwsErr(e);
                ok = true; return "";
            }
            string rest = uri.Substring(5), key = rest.Length > n.Length + 1 ? rest.Substring(n.Length + 1) : "";
            if (key.Length == 0) return "Informe o objeto: aws s3 presign s3://" + n + "/<arquivo> --expires-in <segundos>";
            int secs = 3600;
            string ex = Get(fl, "expires-in");
            if (ex != null && !int.TryParse(ex, out secs)) return "--expires-in deve ser um número de segundos (ex.: 3600 = 1 hora)";
            string url;
            var er = Presign(n, key, secs, out url);
            if (er != null) return AwsErr(er);
            ok = true; return url;
        }

        string CliS3Api(string sub, Dictionary<string, string> fl, out bool ok, out bool handled)
        {
            ok = false; handled = true;
            var news = new[] { "put-bucket-tagging", "get-bucket-tagging", "delete-bucket-tagging", "put-bucket-website", "get-bucket-website", "delete-bucket-website", "put-bucket-lifecycle-configuration", "get-bucket-lifecycle-configuration", "delete-bucket-lifecycle", "put-public-access-block", "get-public-access-block", "delete-public-access-block", "put-bucket-policy", "get-bucket-policy", "delete-bucket-policy", "list-object-versions", "delete-object" };
            if (!news.Contains(sub)) { handled = false; return null; }
            string bn = Get(fl, "bucket");
            if (string.IsNullOrEmpty(bn)) return Need("bucket");
            var b = Bkt(bn);
            if (b == null) return "An error occurred (NoSuchBucket) when calling the " + sub + " operation: The specified bucket does not exist";
            string err;
            switch (sub)
            {
                case "put-bucket-tagging":
                    {
                        string tg = Param(fl, "tagging", out err);
                        if (err != null) return err;
                        if (string.IsNullOrEmpty(tg)) return Need("tagging");
                        var tags = new Dictionary<string, string>();
                        foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(tg, "Key\"?\\s*[:=]\\s*\"?([^\",}\\s]+)\"?\\s*,\\s*\"?Value\"?\\s*[:=]\\s*\"?([^\",}\\]\\s]*)"))
                            tags[m.Groups[1].Value] = m.Groups[2].Value;
                        if (tags.Count == 0) return "Parameter validation failed: use --tagging 'TagSet=[{Key=CHAVE,Value=VALOR}]'";
                        var e = SetBucketTags(bn, tags);
                        if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
                case "get-bucket-tagging":
                    if (b.Tags.Count == 0) return "An error occurred (NoSuchTagSet) when calling the GetBucketTagging operation: The TagSet does not exist";
                    ok = true; return "{ \"TagSet\": [" + string.Join(", ", b.Tags.Select(kv => "{ \"Key\": \"" + kv.Key + "\", \"Value\": \"" + kv.Value + "\" }")) + "] }";
                case "delete-bucket-tagging":
                    { var e = SetBucketTags(bn, new Dictionary<string, string>()); if (e != null) return AwsErr(e); ok = true; return ""; }
                case "put-bucket-website":
                    {
                        string wc = Param(fl, "website-configuration", out err);
                        if (err != null) return err;
                        if (string.IsNullOrEmpty(wc)) return Need("website-configuration");
                        string idx = Pick(wc, "Suffix");
                        if (string.IsNullOrEmpty(idx)) return "Parameter validation failed: informe IndexDocument={Suffix=index.html}";
                        string ed = Rx(wc, "ErrorDocument\"?\\s*[:=]\\s*\\{\\s*\"?Key\"?\\s*[:=]\\s*\"?([^\",}\\s]+)");
                        var e = SetWebsite(bn, idx, ed);
                        if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
                case "get-bucket-website":
                    if (b.IndexDoc == null) return "An error occurred (NoSuchWebsiteConfiguration): The specified bucket does not have a website configuration";
                    ok = true; return "{\n    \"IndexDocument\": { \"Suffix\": \"" + b.IndexDoc + "\" }" + (b.ErrorDoc != null ? ",\n    \"ErrorDocument\": { \"Key\": \"" + b.ErrorDoc + "\" }" : "") + "\n}\n(endpoint: " + WebsiteUrl(b) + ")";
                case "delete-bucket-website":
                    { var e = SetWebsite(bn, null, null); if (e != null) return AwsErr(e); ok = true; return ""; }
                case "put-bucket-lifecycle-configuration":
                    {
                        string lc = Param(fl, "lifecycle-configuration", out err);
                        if (err != null) return err;
                        if (string.IsNullOrEmpty(lc)) return Need("lifecycle-configuration");
                        var rules = new List<LcRule>();
                        var idx = System.Text.RegularExpressions.Regex.Matches(lc, "\"?ID\"?\\s*[:=]").Cast<System.Text.RegularExpressions.Match>().Select(m => m.Index).ToList();
                        if (idx.Count == 0) return "Parameter validation failed: cada regra precisa de \"ID\" (o nome da regra)";
                        for (int k = 0; k < idx.Count; k++)
                        {
                            string seg = lc.Substring(idx[k], (k + 1 < idx.Count ? idx[k + 1] : lc.Length) - idx[k]);
                            var r = new LcRule { Id = Pick(seg, "ID") };
                            string status = Pick(seg, "Status");
                            if (status != "Enabled" && status != "Disabled") return "Parameter validation failed: cada regra precisa de \"Status\": \"Enabled\"";
                            r.Prefix = Rx(seg, "\"?Prefix\"?\\s*[:=]\\s*\"([^\"]*)\"") ?? Pick(seg, "Prefix") ?? "";
                            int d;
                            string exp = Rx(seg, "\"Expiration\"\\s*:\\s*\\{([^}]*)\\}");
                            if (exp != null && int.TryParse(Pick(exp, "Days"), out d)) r.Expire = d;
                            string nc = Rx(seg, "\"NoncurrentVersionExpiration\"\\s*:\\s*\\{([^}]*)\\}");
                            if (nc != null && int.TryParse(Pick(nc, "NoncurrentDays"), out d)) r.Noncurrent = d;
                            string tr = Rx(seg, "\"Transitions\"\\s*:\\s*\\[([^\\]]*)\\]");
                            if (tr != null && int.TryParse(Pick(tr, "Days"), out d)) { string sc = Pick(tr, "StorageClass"); if (sc != "GLACIER") return "Neste laboratório, a transição é para StorageClass GLACIER."; r.Glacier = d; }
                            if (status == "Enabled") rules.Add(r);
                        }
                        var e = PutLifecycle(bn, rules, true);
                        if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
                case "get-bucket-lifecycle-configuration":
                    if (b.Lifecycle.Count == 0) return "An error occurred (NoSuchLifecycleConfiguration): The lifecycle configuration does not exist";
                    ok = true;
                    return "{ \"Rules\": [\n" + string.Join(",\n", b.Lifecycle.Select(r => "    { \"ID\": \"" + r.Id + "\", \"Filter\": { \"Prefix\": \"" + r.Prefix + "\" }, \"Status\": \"Enabled\"" + (r.Expire > 0 ? ", \"Expiration\": { \"Days\": " + r.Expire + " }" : "") + (r.Glacier > 0 ? ", \"Transitions\": [{ \"Days\": " + r.Glacier + ", \"StorageClass\": \"GLACIER\" }]" : "") + (r.Noncurrent > 0 ? ", \"NoncurrentVersionExpiration\": { \"NoncurrentDays\": " + r.Noncurrent + " }" : "") + " }")) + "\n] }";
                case "delete-bucket-lifecycle":
                    { b.Lifecycle.Clear(); Notify("s3:lcdel:" + bn + ":*"); ok = true; return ""; }
                case "put-public-access-block":
                    {
                        string pc = Param(fl, "public-access-block-configuration", out err);
                        if (err != null) return err;
                        if (string.IsNullOrEmpty(pc)) return Need("public-access-block-configuration");
                        var keys = new[] { "BlockPublicAcls", "IgnorePublicAcls", "BlockPublicPolicy", "RestrictPublicBuckets" };
                        bool all = keys.All(k => IsTrue(Pick(pc, k)));
                        var e = SetBlockPublic(bn, all);
                        if (e != null) return AwsErr(e);
                        ok = true; return all ? "" : "(atenção: com alguma opção false, o bucket NÃO fica totalmente bloqueado)";
                    }
                case "get-public-access-block":
                    ok = true;
                    { string v = b.BlockPublic ? "true" : "false"; return "{ \"PublicAccessBlockConfiguration\": { \"BlockPublicAcls\": " + v + ", \"IgnorePublicAcls\": " + v + ", \"BlockPublicPolicy\": " + v + ", \"RestrictPublicBuckets\": " + v + " } }"; }
                case "delete-public-access-block":
                    { var e = SetBlockPublic(bn, false); if (e != null) return AwsErr(e); ok = true; return ""; }
                case "put-bucket-policy":
                    {
                        string pj = Param(fl, "policy", out err);
                        if (err != null) return err;
                        if (string.IsNullOrEmpty(pj)) return Need("policy");
                        var e = SetPolicy(bn, pj);
                        if (e != null) return "An error occurred (" + (e.StartsWith("AccessDenied") ? "AccessDenied" : "MalformedPolicy") + ") when calling the PutBucketPolicy operation: " + e;
                        ok = true; return "";
                    }
                case "get-bucket-policy":
                    if (b.Policy == null) return "An error occurred (NoSuchBucketPolicy) when calling the GetBucketPolicy operation: The bucket policy does not exist";
                    ok = true; return "{ \"Policy\": " + b.PolicyText + " }";
                case "delete-bucket-policy":
                    { var e = SetPolicy(bn, null); if (e != null) return AwsErr(e); ok = true; return ""; }
                case "list-object-versions":
                    {
                        string pre = Get(fl, "prefix") ?? "";
                        ok = true;
                        var sb = new StringBuilder("{\n    \"Versions\": [\n");
                        sb.Append(string.Join(",\n", b.Objects.Where(o => o.StartsWith(pre)).Select(o => "        { \"Key\": \"" + o + "\", \"VersionId\": \"" + VersionOf(bn, o) + "\", \"IsLatest\": true }")));
                        sb.Append("\n    ],\n    \"DeleteMarkers\": [\n");
                        sb.Append(string.Join(",\n", b.Markers.Where(m => m.Key.StartsWith(pre) && !b.Objects.Contains(m.Key)).Select(m => "        { \"Key\": \"" + m.Key + "\", \"VersionId\": \"" + m.VersionId + "\", \"IsLatest\": true }")));
                        sb.Append("\n    ]\n}");
                        if (b.Versioning == "Desativado") sb.Append("\n(versionamento desativado: cada objeto tem uma única versão)");
                        return sb.ToString();
                    }
                case "delete-object":
                    {
                        string key = Get(fl, "key");
                        if (string.IsNullOrEmpty(key)) return Need("key");
                        string vid = Get(fl, "version-id");
                        if (vid == null)
                        {
                            if (!b.Objects.Contains(key)) { ok = true; return ""; }
                            DeleteObject(bn, key);
                            var mk = b.Markers.LastOrDefault(m => m.Key == key);
                            ok = true; return b.Versioning == "Ativado" && mk != null ? "{ \"DeleteMarker\": true, \"VersionId\": \"" + mk.VersionId + "\" }" : "";
                        }
                        var e = DeleteVersion(bn, key, vid);
                        if (e != null) return "An error occurred (NoSuchVersion) when calling the DeleteObject operation: " + e;
                        ok = true; return "{ \"VersionId\": \"" + vid + "\" }";
                    }
            }
            return Invalid("s3api", sub);
        }

        Ec2 InstIn(string id) { return Instances.FirstOrDefault(x => x.Id == id && x.Region == Region); }
        static string NoInst(string id, string rg) { return "An error occurred (InvalidInstanceID.NotFound): The instance ID '" + id + "' does not exist (confira a região: " + rg + ")"; }

        string CliEc2(string sub, Dictionary<string, string> fl, List<string> pos, List<string> t, out bool ok, out bool handled)
        {
            ok = false; handled = true;
            switch (sub)
            {
                case "create-key-pair":
                    {
                        string n = Get(fl, "key-name");
                        if (string.IsNullOrEmpty(n)) return Need("key-name");
                        string kt = (Get(fl, "key-type") ?? "rsa").ToLowerInvariant();
                        var e = CreateKeyPair(n, kt);
                        if (e != null) return AwsErr(e);
                        ok = true;
                        return "{\n    \"KeyName\": \"" + n + "\",\n    \"KeyType\": \"" + kt + "\",\n    \"KeyMaterial\": \"-----BEGIN " + (kt == "ed25519" ? "OPENSSH" : "RSA") + " PRIVATE KEY-----\\n" + Hx(n, 16) + "...\\n-----END " + (kt == "ed25519" ? "OPENSSH" : "RSA") + " PRIVATE KEY-----\"\n}\n(a chave privada só aparece AGORA: guarde-a com segurança; a AWS não guarda cópia)";
                    }
                case "describe-key-pairs":
                    ok = true; Open("ec2");
                    return "{ \"KeyPairs\": [\n" + string.Join(",\n", KeyPairs.Where(k => k.Region == Region).Select(k => "    { \"KeyName\": \"" + k.Name + "\", \"KeyType\": \"" + k.Type + "\" }")) + "\n] }";
                case "delete-key-pair":
                    { string n = Get(fl, "key-name"); if (string.IsNullOrEmpty(n)) return Need("key-name"); var e = DeleteKeyPair(n); if (e != null) return AwsErr(e); ok = true; return ""; }
                case "describe-addresses":
                    ok = true; Open("ec2");
                    return "{ \"Addresses\": [\n" + string.Join(",\n", Eips.Where(x => x.Region == Region).Select(x => "    { \"PublicIp\": \"" + x.Ip + "\", \"AllocationId\": \"" + x.Alloc + "\"" + (x.Instance != null ? ", \"AssociationId\": \"eipassoc-" + x.Alloc.Substring(9) + "\", \"InstanceId\": \"" + x.Instance + "\"" : "") + " }")) + "\n] }";
                case "allocate-address":
                    { string a, ip; AllocateEip(out a, out ip); ok = true; return "{\n    \"PublicIp\": \"" + ip + "\",\n    \"AllocationId\": \"" + a + "\",\n    \"Domain\": \"vpc\"\n}"; }
                case "associate-address":
                    {
                        string id = Get(fl, "instance-id"), al = Get(fl, "allocation-id");
                        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(al)) return Need("instance-id", "allocation-id");
                        var e = AssociateEip(al, id, fl.ContainsKey("allow-reassociation"));
                        if (e != null) return AwsErr(e);
                        ok = true; return "{ \"AssociationId\": \"eipassoc-" + (al.Length > 9 ? al.Substring(9) : al) + "\" }";
                    }
                case "disassociate-address":
                    {
                        string asg = Get(fl, "association-id"), ip = Get(fl, "public-ip");
                        Eip e = null;
                        if (asg != null) e = Eips.FirstOrDefault(x => x.Region == Region && "eipassoc-" + (x.Alloc.Length > 9 ? x.Alloc.Substring(9) : x.Alloc) == asg);
                        else if (ip != null) e = Eips.FirstOrDefault(x => x.Region == Region && x.Ip == ip);
                        else return Need("association-id");
                        if (e == null) return "An error occurred (InvalidAssociationID.NotFound): associação não encontrada";
                        var er = DisassociateEip(e.Alloc);
                        if (er != null) return AwsErr(er);
                        ok = true; return "";
                    }
                case "release-address":
                    { string al = Get(fl, "allocation-id"); if (string.IsNullOrEmpty(al)) return Need("allocation-id"); var e = ReleaseEip(al); if (e != null) return AwsErr(e); ok = true; return ""; }
                case "create-image":
                    {
                        string id = Get(fl, "instance-id"), n = Get(fl, "name");
                        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(n)) return Need("instance-id", "name");
                        var e = CreateImage(id, n, fl.ContainsKey("no-reboot"));
                        if (e != null) return AwsErr(e);
                        ok = true; return "{\n    \"ImageId\": \"ami-0" + Hx("ami" + n.Trim(), 7) + "\"\n}";
                    }
                case "describe-images":
                    ok = true; Open("ec2");
                    return "{ \"Images\": [\n" + string.Join(",\n", Images.Where(x => x.Region == Region).Select(x => "    { \"ImageId\": \"" + x.Id + "\", \"Name\": \"" + x.Name + "\", \"OwnerId\": \"" + Arn + "\", \"State\": \"available\" }")
                        .Concat(fl.ContainsKey("owners") && (Get(fl, "owners") ?? "") == "self" ? new string[0] : BaseAmis.Select(a => "    { \"ImageId\": \"" + a[0] + "\", \"Name\": \"" + a[1] + "\", \"OwnerId\": \"amazon\" }"))) + "\n] }";
                case "modify-instance-attribute":
                    {
                        bool on = fl.ContainsKey("disable-api-termination"), off = fl.ContainsKey("no-disable-api-termination");
                        if (!on && !off) { handled = false; return null; }
                        string id = Get(fl, "instance-id");
                        if (string.IsNullOrEmpty(id)) return Need("instance-id");
                        if (InstIn(id) == null) return NoInst(id, Region);
                        var e = SetProtection(id, on);
                        if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
                case "modify-instance-metadata-options":
                    {
                        string id = Get(fl, "instance-id");
                        if (string.IsNullOrEmpty(id)) return Need("instance-id");
                        var i = InstIn(id);
                        if (i == null) return NoInst(id, Region);
                        string tk = Get(fl, "http-tokens") ?? (i.Imds == "disabled" ? "optional" : i.Imds);
                        string ep = Get(fl, "http-endpoint") ?? "enabled";
                        if (ep != "enabled" && ep != "disabled") return "--http-endpoint aceita enabled ou disabled";
                        var e = SetImds(id, tk, ep == "enabled");
                        if (e != null) return AwsErr(e);
                        ok = true; return "{\n    \"InstanceId\": \"" + id + "\",\n    \"InstanceMetadataOptions\": { \"State\": \"pending\", \"HttpTokens\": \"" + tk + "\", \"HttpEndpoint\": \"" + ep + "\" }\n}";
                    }
                case "associate-iam-instance-profile":
                    {
                        string id = Get(fl, "instance-id"), prof = Get(fl, "iam-instance-profile");
                        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(prof)) return Need("instance-id", "iam-instance-profile");
                        if (InstIn(id) == null) return NoInst(id, Region);
                        string n = Pick(prof, "Name") ?? Rx(prof, "instance-profile/([A-Za-z0-9+=,.@_\\-]+)");
                        if (n == null) return "Parameter validation failed: use --iam-instance-profile Name=NOME_DO_PERFIL";
                        var e = AssociateRole(id, n);
                        if (e != null) return AwsErr(e);
                        ok = true; return "{ \"IamInstanceProfileAssociation\": { \"InstanceId\": \"" + id + "\", \"IamInstanceProfile\": { \"Arn\": \"arn:aws:iam::" + Arn + ":instance-profile/" + n + "\" }, \"State\": \"associating\" } }";
                    }
                case "describe-iam-instance-profile-associations":
                    ok = true;
                    return "{ \"IamInstanceProfileAssociations\": [" + string.Join(", ", Instances.Where(i => i.Role != null && i.Region == Region).Select(i => "{ \"InstanceId\": \"" + i.Id + "\", \"IamInstanceProfile\": \"" + i.Role + "\" }")) + "] }";
                case "modify-volume":
                    {
                        string vid = Get(fl, "volume-id");
                        if (string.IsNullOrEmpty(vid)) return Need("volume-id");
                        var v = Volumes.FirstOrDefault(x => x.Id == vid && x.Region == Region);
                        if (v == null) return "An error occurred (InvalidVolume.NotFound): The volume '" + vid + "' does not exist (confira a região: " + Region + ")";
                        int size = -1;
                        if (Get(fl, "size") != null && !int.TryParse(Get(fl, "size"), out size)) return "--size deve ser um número (GiB)";
                        var e = ModifyVolume(vid, Get(fl, "volume-type") ?? v.Type, size);
                        if (e != null) return AwsErr(e);
                        ok = true; return "{ \"VolumeModification\": { \"VolumeId\": \"" + vid + "\", \"ModificationState\": \"modifying\", \"TargetVolumeType\": \"" + v.Type + "\", \"TargetSize\": " + v.Size + " } }";
                    }
                case "create-security-group":
                    {
                        string n = Get(fl, "group-name"), d = Get(fl, "description");
                        if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(d)) return Need("group-name", "description");
                        string id;
                        var e = CreateSg(n, d, Get(fl, "vpc-id"), out id);
                        if (e != null) return AwsErr(e);
                        ok = true; return "{\n    \"GroupId\": \"" + id + "\"\n}";
                    }
                case "run-instances":
                    {
                        string ami = Get(fl, "image-id"), it = Get(fl, "instance-type");
                        if (string.IsNullOrEmpty(ami) || string.IsNullOrEmpty(it)) return Need("image-id", "instance-type");
                        var baseAmi = BaseAmis.FirstOrDefault(a => a[0] == ami);
                        var own = Images.FirstOrDefault(x => x.Id == ami && x.Region == Region);
                        if (baseAmi == null && own == null) return "An error occurred (InvalidAMIID.NotFound): The image id '[" + ami + "]' does not exist (veja: aws ec2 describe-images)";
                        if (!InstanceTypes.Any(x => x[0] == it)) return "An error occurred (InvalidParameterValue): Invalid value '" + it + "' for InstanceType.";
                        string tags = Multi(t, "tag-specifications") ?? "";
                        string name = Rx(tags, "Key\"?\\s*[:=]\\s*\"?Name\"?\\s*,\\s*\"?Value\"?\\s*[:=]\\s*\"?([^\",}\\]\\s]+)");
                        if (name == null) return "Dê um nome à instância: --tag-specifications 'ResourceType=instance,Tags=[{Key=Name,Value=NOME}]'";
                        string sgs = Get(fl, "security-group-ids");
                        var e = Launch(name, baseAmi != null ? baseAmi[1] : own.Name, it, Get(fl, "key-name"), sgs);
                        if (e != null) return AwsErr(e);
                        ok = true; return "{\n    \"Instances\": [{ \"InstanceId\": \"" + LastLaunched + "\", \"ImageId\": \"" + ami + "\", \"InstanceType\": \"" + it + "\", \"KeyName\": \"" + (Get(fl, "key-name") ?? "") + "\", \"State\": { \"Name\": \"pending\" } }]\n}";
                    }
                case "enable-ebs-encryption-by-default":
                case "disable-ebs-encryption-by-default":
                    { bool on = sub.StartsWith("enable"); SetEbsDefault(Region, on); ok = true; return "{\n    \"EbsEncryptionByDefault\": " + (on ? "true" : "false") + "\n}"; }
                case "get-ebs-encryption-by-default":
                    ok = true; return "{\n    \"EbsEncryptionByDefault\": " + (EbsDefaultOn(Region) ? "true" : "false") + "\n}   # região " + Region;
                case "describe-vpcs":
                    ok = true; Open("vpc");
                    return "{ \"Vpcs\": [" + string.Join(", ", Vpcs.Where(v => v.Region == Region).Select(v => "{ \"VpcId\": \"" + v.Id + "\", \"CidrBlock\": \"" + v.Cidr + "\", \"Tags\": [{ \"Key\": \"Name\", \"Value\": \"" + v.Name + "\" }] }")) + "] }";
                case "describe-subnets":
                    ok = true; Open("vpc");
                    return "{ \"Subnets\": [\n" + string.Join(",\n", Subnets.Where(s => Vpcs.Any(v => v.Id == s.Vpc && v.Region == Region)).Select(s => "    { \"SubnetId\": \"" + s.Id + "\", \"VpcId\": \"" + s.Vpc + "\", \"CidrBlock\": \"" + s.Cidr + "\", \"AvailabilityZone\": \"" + s.Az + "\", \"Tags\": [{ \"Key\": \"Name\", \"Value\": \"" + s.Name + "\" }] }")) + "\n] }";
                case "create-subnet":
                    {
                        string vpc = Get(fl, "vpc-id"), cidr = Get(fl, "cidr-block"), az = Get(fl, "availability-zone");
                        if (string.IsNullOrEmpty(vpc) || string.IsNullOrEmpty(cidr)) return Need("vpc-id", "cidr-block");
                        if (string.IsNullOrEmpty(az)) az = Region + "a";
                        string tags = Multi(t, "tag-specifications") ?? "";
                        string name = Rx(tags, "Key\"?\\s*[:=]\\s*\"?Name\"?\\s*,\\s*\"?Value\"?\\s*[:=]\\s*\"?([^\",}\\]\\s]+)") ?? "";
                        string id;
                        var e = CreateSubnet(vpc, cidr, az, name, out id);
                        if (e != null) return AwsErr(e);
                        ok = true; return "{\n    \"Subnet\": { \"SubnetId\": \"" + id + "\", \"VpcId\": \"" + vpc + "\", \"CidrBlock\": \"" + cidr + "\", \"AvailabilityZone\": \"" + az + "\", \"AvailableIpAddressCount\": 251 }\n}";
                    }
                case "describe-route-tables":
                    ok = true; Open("vpc");
                    return "{ \"RouteTables\": [\n" + string.Join(",\n", RouteTables.Where(r => Vpcs.Any(v => v.Id == r.Vpc && v.Region == Region)).Select(r => "    { \"RouteTableId\": \"" + r.Id + "\", \"Name\": \"" + r.Name + "\", \"Associations\": [" + string.Join(", ", Subnets.Where(s => s.Rtb == r.Id).Select(s => "\"" + s.Id + "\"")) + "], \"Routes\": [" + string.Join(", ", r.Routes.Select(x => "{ \"DestinationCidrBlock\": \"" + x[0] + "\", \"" + (x[1].StartsWith("nat-") ? "NatGatewayId" : "GatewayId") + "\": \"" + x[1] + "\" }")) + "] }")) + "\n] }";
                case "create-route":
                case "replace-route":
                    {
                        string rtb = Get(fl, "route-table-id"), dest = Get(fl, "destination-cidr-block");
                        string target = Get(fl, "nat-gateway-id") ?? Get(fl, "gateway-id");
                        if (string.IsNullOrEmpty(rtb) || string.IsNullOrEmpty(dest) || string.IsNullOrEmpty(target)) return Need("route-table-id", "destination-cidr-block", "gateway-id ou --nat-gateway-id");
                        if (fl.ContainsKey("gateway-id") && target.StartsWith("nat-")) return "Para um gateway NAT use --nat-gateway-id (--gateway-id é para o gateway da Internet).";
                        if (fl.ContainsKey("nat-gateway-id") && !target.StartsWith("nat-")) return "--nat-gateway-id espera um ID nat-...";
                        var e = SetRoute(rtb, dest, target, sub == "replace-route");
                        if (e != null) return AwsErr(e);
                        ok = true; return "{\n    \"Return\": true\n}";
                    }
                case "delete-route":
                    {
                        string rtb = Get(fl, "route-table-id"), dest = Get(fl, "destination-cidr-block");
                        if (string.IsNullOrEmpty(rtb) || string.IsNullOrEmpty(dest)) return Need("route-table-id", "destination-cidr-block");
                        var e = DeleteRoute(rtb, dest);
                        if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
            }
            handled = false;
            return null;
        }

        string CliAsg(string sub, Dictionary<string, string> fl, out bool ok)
        {
            ok = false;
            string n = Get(fl, "auto-scaling-group-name");
            switch (sub)
            {
                case "describe-auto-scaling-groups":
                    ok = true; Open("ec2");
                    return "{ \"AutoScalingGroups\": [" + string.Join(", ", Asgs.Select(a => "{ \"AutoScalingGroupName\": \"" + a.Name + "\", \"MinSize\": " + a.Min + ", \"MaxSize\": " + a.Max + ", \"DesiredCapacity\": " + a.Desired + " }")) + "] }";
                case "set-desired-capacity":
                    {
                        int d;
                        if (string.IsNullOrEmpty(n) || !int.TryParse(Get(fl, "desired-capacity"), out d)) return Need("auto-scaling-group-name", "desired-capacity");
                        var a = Group(n);
                        if (a == null) return "An error occurred (ValidationError): AutoScalingGroup name not found - " + n;
                        if (d > a.Max || d < a.Min) return "An error occurred (ValidationError): New SetDesiredCapacity value " + d + " is " + (d > a.Max ? "above max value " + a.Max : "below min value " + a.Min) + " for the AutoScalingGroup.";
                        var e = SetCapacity(n, a.Min, a.Max, d);
                        if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
                case "update-auto-scaling-group":
                    {
                        if (string.IsNullOrEmpty(n)) return Need("auto-scaling-group-name");
                        var a = Group(n);
                        if (a == null) return "An error occurred (ValidationError): AutoScalingGroup name not found - " + n;
                        int mn = a.Min, mx = a.Max, d = a.Desired;
                        if (Get(fl, "min-size") != null && !int.TryParse(Get(fl, "min-size"), out mn)) return "--min-size deve ser um número";
                        if (Get(fl, "max-size") != null && !int.TryParse(Get(fl, "max-size"), out mx)) return "--max-size deve ser um número";
                        bool dset = Get(fl, "desired-capacity") != null;
                        if (dset && !int.TryParse(Get(fl, "desired-capacity"), out d)) return "--desired-capacity deve ser um número";
                        if (!dset) d = Math.Max(mn, Math.Min(mx, d));   // como na AWS: a desejada acompanha os novos limites
                        var e = SetCapacity(n, mn, mx, d);
                        if (e != null) return "An error occurred (ValidationError): " + e;
                        ok = true; return "";
                    }
                case "put-scaling-policy":
                    {
                        string pn = Get(fl, "policy-name"), pt = Get(fl, "policy-type") ?? "SimpleScaling";
                        if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(pn)) return Need("auto-scaling-group-name", "policy-name");
                        if (pt != "TargetTrackingScaling") return "Neste laboratório, use --policy-type TargetTrackingScaling (rastreamento de destino): escala simples e em etapas precisam de alarmes próprios.";
                        string err;
                        string cfg = Param(fl, "target-tracking-configuration", out err);
                        if (err != null) return err;
                        if (string.IsNullOrEmpty(cfg)) return Need("target-tracking-configuration");
                        string metric = Pick(cfg, "PredefinedMetricType");
                        float tv;
                        if (metric == null || !float.TryParse(Pick(cfg, "TargetValue"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out tv)) return "Parameter validation failed: informe PredefinedMetricSpecification.PredefinedMetricType e TargetValue";
                        var valid = new[] { "ASGAverageCPUUtilization", "ASGAverageNetworkIn", "ASGAverageNetworkOut", "ALBRequestCountPerTarget" };
                        if (!valid.Contains(metric)) return "PredefinedMetricType inválido: " + metric + " (ex.: ASGAverageCPUUtilization)";
                        var e = PutScalingPolicy(n, pn, pt, metric, tv);
                        if (e != null) return AwsErr(e);
                        ok = true; return "{\n    \"PolicyARN\": \"arn:aws:autoscaling:us-east-1:" + Arn + ":scalingPolicy:" + Hx(pn, 8) + ":autoScalingGroupName/" + n + ":policyName/" + pn + "\"\n}";
                    }
                case "describe-policies":
                    ok = true;
                    return "{ \"ScalingPolicies\": [" + string.Join(", ", Asgs.SelectMany(a => a.Policies.Select(p => "{ \"AutoScalingGroupName\": \"" + a.Name + "\", \"PolicyName\": \"" + p[0] + "\", \"PolicyType\": \"" + p[1] + "\", \"PredefinedMetricType\": \"" + p[2] + "\", \"TargetValue\": " + p[3] + " }"))) + "] }";
                case "delete-policy":
                    { string pn = Get(fl, "policy-name"); if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(pn)) return Need("auto-scaling-group-name", "policy-name"); var e = DeleteScalingPolicy(n, pn); if (e != null) return AwsErr(e); ok = true; return ""; }
            }
            return Invalid("autoscaling", sub);
        }

        string CliIam(string sub, Dictionary<string, string> fl, out bool ok, out bool handled)
        {
            ok = false; handled = true;
            string u = Get(fl, "user-name"), rn = Get(fl, "role-name"), err;
            switch (sub)
            {
                case "list-access-keys":
                    {
                        if (string.IsNullOrEmpty(u)) return Need("user-name");
                        if (!Users.Any(x => x.Name == u)) return "An error occurred (NoSuchEntity): The user with name " + u + " cannot be found.";
                        ok = true;
                        return "{ \"AccessKeyMetadata\": [\n" + string.Join(",\n", AccessKeys.Where(k => k.User == u).Select(k => "    { \"UserName\": \"" + u + "\", \"AccessKeyId\": \"" + k.Id + "\", \"Status\": \"" + k.Status + "\", \"CreateDate\": \"" + k.Created.ToString("yyyy-MM-dd") + "\" }")) + "\n] }";
                    }
                case "update-access-key":
                    {
                        string k = Get(fl, "access-key-id"), st = Get(fl, "status");
                        if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(k) || string.IsNullOrEmpty(st)) return Need("user-name", "access-key-id", "status");
                        var e = SetKeyStatus(u, k, st);
                        if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
                case "create-access-key":
                    {
                        if (string.IsNullOrEmpty(u)) return Need("user-name");
                        string id;
                        var e = CreateAccessKey(u, out id);
                        if (e != null) return AwsErr(e);
                        ok = true; return "{\n    \"AccessKey\": {\n        \"UserName\": \"" + u + "\",\n        \"AccessKeyId\": \"" + id + "\",\n        \"Status\": \"Active\",\n        \"SecretAccessKey\": \"" + Hx(id, 16) + Hx(u + id, 16) + "\"\n    }\n}\n(a chave secreta só aparece AGORA: guarde no cofre de segredos do pipeline)";
                    }
                case "delete-access-key":
                    {
                        string k = Get(fl, "access-key-id");
                        if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(k)) return Need("user-name", "access-key-id");
                        var e = DeleteAccessKey(u, k);
                        if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
                case "create-role":
                    {
                        if (string.IsNullOrEmpty(rn)) return Need("role-name", "assume-role-policy-document");
                        string doc = Param(fl, "assume-role-policy-document", out err);
                        if (err != null) return err;
                        if (string.IsNullOrEmpty(doc)) return Need("assume-role-policy-document");
                        string svc = Rx(doc, "\"Service\"\\s*:\\s*\"([a-z0-9\\-]+)\\.amazonaws\\.com\"");
                        if (svc == null || !doc.Contains("sts:AssumeRole")) return "An error occurred (MalformedPolicyDocument): a política de confiança precisa de Principal.Service e Action sts:AssumeRole";
                        var e = CreateRole(rn, svc);
                        if (e != null) return AwsErr(e);
                        ok = true; return "{ \"Role\": { \"RoleName\": \"" + rn + "\", \"Arn\": \"arn:aws:iam::" + Arn + ":role/" + rn + "\" } }";
                    }
                case "attach-role-policy":
                case "detach-role-policy":
                    {
                        string arn = Get(fl, "policy-arn");
                        if (string.IsNullOrEmpty(rn) || string.IsNullOrEmpty(arn)) return Need("role-name", "policy-arn");
                        string p = PolicyFromArn(arn);
                        if (p == null) return "An error occurred (InvalidInput): --policy-arn precisa ser o ARN completo, ex.: arn:aws:iam::aws:policy/ReadOnlyAccess";
                        var mp = ManagedPolicies.FirstOrDefault(x => x[0] == p);
                        if (mp != null && arn != PolicyArn(p)) return "An error occurred (NoSuchEntity): Policy " + arn + " does not exist or is not attachable. (o ARN certo é " + PolicyArn(p) + ")";
                        var e = sub.StartsWith("attach") ? AttachRolePolicy(rn, p) : DetachRolePolicy(rn, p);
                        if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
                case "list-roles":
                    ok = true; Open("iam");
                    return "{ \"Roles\": [\n" + string.Join(",\n", Roles.Select(r => "    { \"RoleName\": \"" + r.Name + "\", \"AssumeRolePolicyDocument\": { \"Principal\": { \"Service\": \"" + r.Trust + ".amazonaws.com\" } } }")) + "\n] }";
                case "list-attached-role-policies":
                    {
                        var r = Rl(rn ?? "");
                        if (r == null) return "An error occurred (NoSuchEntity): The role with name " + rn + " cannot be found.";
                        ok = true; return "{ \"AttachedPolicies\": [" + string.Join(", ", r.Policies.Select(p => "{ \"PolicyName\": \"" + p + "\", \"PolicyArn\": \"" + PolicyArn(p) + "\" }")) + "] }";
                    }
                case "delete-role":
                    {
                        var r = Rl(rn ?? "");
                        if (r == null) return "An error occurred (NoSuchEntity): The role with name " + rn + " cannot be found.";
                        if (r.Policies.Count > 0) return "An error occurred (DeleteConflict): Cannot delete entity, must detach all policies first.";
                        var e = DeleteRole(rn); if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
                case "update-account-password-policy":
                    {
                        int min = 8, age = 0;
                        if (Get(fl, "minimum-password-length") != null && !int.TryParse(Get(fl, "minimum-password-length"), out min)) return "--minimum-password-length deve ser um número";
                        if (Get(fl, "max-password-age") != null && !int.TryParse(Get(fl, "max-password-age"), out age)) return "--max-password-age deve ser um número de dias";
                        // como na AWS: o que não for informado volta ao padrão (desligado)
                        var e = SetPasswordPolicy(min, fl.ContainsKey("require-uppercase-characters"), fl.ContainsKey("require-lowercase-characters"), fl.ContainsKey("require-numbers"), fl.ContainsKey("require-symbols"), age);
                        if (e != null) return "An error occurred (ValidationError): " + e;
                        ok = true; return "";
                    }
                case "get-account-password-policy":
                    ok = true;
                    return "{ \"PasswordPolicy\": { \"MinimumPasswordLength\": " + PwMin + ", \"RequireUppercaseCharacters\": " + (PwUpper ? "true" : "false") + ", \"RequireLowercaseCharacters\": " + (PwLower ? "true" : "false") + ", \"RequireNumbers\": " + (PwNumbers ? "true" : "false") + ", \"RequireSymbols\": " + (PwSymbols ? "true" : "false") + ", \"ExpirePasswords\": " + (PwMaxAge > 0 ? "true, \"MaxPasswordAge\": " + PwMaxAge : "false") + " } }";
                case "remove-user-from-group":
                    {
                        string g = Get(fl, "group-name");
                        if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(g)) return Need("user-name", "group-name");
                        var e = RemoveFromGroup(u, g);
                        if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
                case "delete-user":
                    {
                        if (string.IsNullOrEmpty(u)) return Need("user-name");
                        var e = DeleteUserStrict(u);
                        if (e != null) return e.StartsWith("An error") ? e : AwsErr(e);
                        ok = true; return "";
                    }
                case "get-user":
                    {
                        var x = Users.FirstOrDefault(y => y.Name == u);
                        if (x == null) return "An error occurred (NoSuchEntity): The user with name " + u + " cannot be found.";
                        ok = true; return "{ \"User\": { \"UserName\": \"" + x.Name + "\", \"Arn\": \"arn:aws:iam::" + Arn + ":user/" + x.Name + "\", \"CreateDate\": \"" + x.Created.ToString("yyyy-MM-dd") + "\" } }";
                    }
                case "list-groups-for-user":
                    {
                        var x = Users.FirstOrDefault(y => y.Name == u);
                        if (x == null) return "An error occurred (NoSuchEntity): The user with name " + u + " cannot be found.";
                        ok = true; return "{ \"Groups\": [" + (x.Group != null ? "{ \"GroupName\": \"" + x.Group + "\" }" : "") + "] }";
                    }
            }
            handled = false;
            return null;
        }

        // Variables={A=1,B=2} ou {"Variables":{"A":"1"}}
        static Dictionary<string, string> ParseEnv(string text)
        {
            var d = new Dictionary<string, string>();
            if (text == null) return null;
            string inner = Rx(text, "Variables\"?\\s*[:=]\\s*\\{([^}]*)\\}");
            if (inner == null) return null;
            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(inner, "\"?([A-Za-z_][A-Za-z0-9_]*)\"?\\s*[:=]\\s*\"?([^\",]*)\"?"))
                d[m.Groups[1].Value] = m.Groups[2].Value.Trim();
            return d;
        }

        string CliLambda(string sub, Dictionary<string, string> fl, out bool ok, out bool handled)
        {
            ok = false; handled = true;
            string fn = Get(fl, "function-name"), err;
            switch (sub)
            {
                case "get-function-configuration":
                    {
                        var f = Func(fn ?? "");
                        if (f == null) return "An error occurred (ResourceNotFoundException): Function not found: " + fn;
                        ok = true;
                        return "{\n    \"FunctionName\": \"" + f.Name + "\",\n    \"Runtime\": \"" + RuntimeId(f.Runtime) + "\",\n    \"Timeout\": " + f.Timeout + ",\n    \"MemorySize\": " + f.Memory + ",\n    \"Environment\": { \"Variables\": { " + string.Join(", ", f.Env.Select(kv => "\"" + kv.Key + "\": \"" + kv.Value + "\"")) + " } }\n}";
                    }
                case "update-function-configuration":
                    {
                        if (string.IsNullOrEmpty(fn)) return Need("function-name");
                        var f = Func(fn);
                        if (f == null) return "An error occurred (ResourceNotFoundException): Function not found: " + fn;
                        int to = -1, mem = -1;
                        if (Get(fl, "timeout") != null && !int.TryParse(Get(fl, "timeout"), out to)) return "--timeout deve ser um número de segundos";
                        if (Get(fl, "memory-size") != null && !int.TryParse(Get(fl, "memory-size"), out mem)) return "--memory-size deve ser um número (MB)";
                        string rt = Get(fl, "runtime");
                        Dictionary<string, string> env = null;
                        if (fl.ContainsKey("environment"))
                        {
                            string et = Param(fl, "environment", out err);
                            if (err != null) return err;
                            env = ParseEnv(et);
                            if (env == null) return "Parameter validation failed: use --environment Variables={CHAVE=valor,OUTRA=valor}";
                        }
                        if (to == -1 && mem == -1 && rt == null && env == null) return "Informe o que mudar: --timeout, --memory-size, --runtime ou --environment";
                        var e = UpdateFnConfig(fn, to, mem, rt);
                        if (e != null) return "An error occurred (InvalidParameterValueException): " + e;
                        if (env != null) { e = SetEnv(fn, env); if (e != null) return "An error occurred (InvalidParameterValueException): " + e; }
                        ok = true;
                        return "{ \"FunctionName\": \"" + f.Name + "\", \"Runtime\": \"" + RuntimeId(f.Runtime) + "\", \"Timeout\": " + f.Timeout + ", \"MemorySize\": " + f.Memory + ", \"Environment\": { \"Variables\": { " + string.Join(", ", f.Env.Select(kv => "\"" + kv.Key + "\": \"" + kv.Value + "\"")) + " } }, \"LastUpdateStatus\": \"InProgress\" }";
                    }
                case "put-function-concurrency":
                    {
                        int n;
                        if (string.IsNullOrEmpty(fn) || !int.TryParse(Get(fl, "reserved-concurrent-executions"), out n)) return Need("function-name", "reserved-concurrent-executions");
                        if (n < 0) return "--reserved-concurrent-executions não pode ser negativo";
                        var e = SetConcurrency(fn, n);
                        if (e != null) return AwsErr(e);
                        ok = true; return "{\n    \"ReservedConcurrentExecutions\": " + n + "\n}";
                    }
                case "delete-function-concurrency":
                    { if (string.IsNullOrEmpty(fn)) return Need("function-name"); var e = SetConcurrency(fn, -1); if (e != null) return AwsErr(e); ok = true; return ""; }
                case "create-event-source-mapping":
                    {
                        string src = Get(fl, "event-source-arn");
                        if (string.IsNullOrEmpty(fn) || string.IsNullOrEmpty(src)) return Need("function-name", "event-source-arn");
                        string q = QueueFromArn(src);
                        if (q == null) return "An error occurred (InvalidParameterValueException): --event-source-arn deve ser o ARN da fila: arn:aws:sqs:us-east-1:" + Arn + ":NOME";
                        int batch = 10;
                        if (Get(fl, "batch-size") != null && !int.TryParse(Get(fl, "batch-size"), out batch)) return "--batch-size deve ser um número";
                        var e = AddTrigger(fn, q, batch);
                        if (e != null) return AwsErr(e);
                        ok = true; return "{\n    \"UUID\": \"" + Hx(fn + q, 8) + "-" + Hx(q, 4) + "-4" + Hx(fn, 3) + "-a" + Hx(q + fn, 3) + "-" + Hx(fn + "e", 12) + "\",\n    \"BatchSize\": " + batch + ",\n    \"EventSourceArn\": \"" + src + "\",\n    \"State\": \"Creating\"\n}";
                    }
                case "list-event-source-mappings":
                    ok = true;
                    return "{ \"EventSourceMappings\": [" + string.Join(", ", Functions.Where(f => fn == null || f.Name == fn).SelectMany(f => f.Triggers.Select(tr => "{ \"FunctionArn\": \"" + f.Name + "\", \"EventSourceArn\": \"arn:aws:sqs:us-east-1:" + Arn + ":" + tr[0] + "\", \"BatchSize\": " + tr[1] + " }"))) + "] }";
            }
            handled = false;
            return null;
        }

        string CliLogs(string sub, Dictionary<string, string> fl, out bool ok, out bool handled)
        {
            ok = false; handled = true;
            string g = Get(fl, "log-group-name");
            switch (sub)
            {
                case "filter-log-events":
                    {
                        if (string.IsNullOrEmpty(g)) return Need("log-group-name");
                        if (!LogGroups.Contains(g)) return "An error occurred (ResourceNotFoundException): The specified log group does not exist: " + g;
                        var lines = FilterLogs(g, Get(fl, "filter-pattern"));
                        ok = true;
                        return "{ \"events\": [\n" + string.Join(",\n", lines.Select(l => "    { \"logStreamName\": \"2026/10/15/[$LATEST]" + Hx(g, 8) + "\", \"message\": \"" + l.Replace("\"", "\\\"") + "\" }")) + "\n] }";
                    }
                case "put-retention-policy":
                    {
                        int d;
                        if (string.IsNullOrEmpty(g) || !int.TryParse(Get(fl, "retention-in-days"), out d)) return Need("log-group-name", "retention-in-days");
                        var e = SetRetention(g, d);
                        if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
                case "delete-retention-policy":
                    { if (string.IsNullOrEmpty(g)) return Need("log-group-name"); var e = SetRetention(g, 0); if (e != null) return AwsErr(e); ok = true; return ""; }
            }
            handled = false;
            return null;
        }

        string CliCw(string sub, Dictionary<string, string> fl, out bool ok, out bool handled)
        {
            ok = false; handled = true;
            switch (sub)
            {
                case "delete-alarms":
                    {
                        string n = Get(fl, "alarm-names");
                        if (string.IsNullOrEmpty(n)) return Need("alarm-names");
                        var e = DeleteAlarm(n);
                        if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
                case "put-metric-alarm":
                    {
                        string n = Get(fl, "alarm-name"), m = Get(fl, "metric-name"), th = Get(fl, "threshold");
                        if (n == null || m == null || th == null) return "aws: error: the following arguments are required: --alarm-name, --metric-name, --namespace, --threshold, --comparison-operator, ...";
                        float tv;
                        if (!float.TryParse(th, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out tv)) return "--threshold deve ser um número";
                        string ns = Get(fl, "namespace") ?? "AWS/EC2";
                        var nsm = Metrics.FirstOrDefault(x => x[0] == ns);
                        if (nsm == null) return "Namespace não suportado aqui: " + ns + " (AWS/EC2, AWS/SQS ou AWS/RDS)";
                        if (!nsm.Skip(1).Contains(m)) return "A métrica " + m + " não existe no namespace " + ns + " (opções: " + string.Join(", ", nsm.Skip(1)) + ")";
                        string comp = Get(fl, "comparison-operator") ?? "GreaterThanThreshold";
                        var comps = new[] { "GreaterThanThreshold", "GreaterThanOrEqualToThreshold", "LessThanThreshold", "LessThanOrEqualToThreshold" };
                        if (!comps.Contains(comp)) return "--comparison-operator inválido: " + comp;
                        string dims = Get(fl, "dimensions") ?? "";
                        string res;
                        if (ns == "AWS/EC2") { var inst = Instances.FirstOrDefault(i => dims.Contains(i.Id)); res = inst != null ? inst.Name : "(sem instância)"; }
                        else if (ns == "AWS/SQS") { res = Pick(dims, "Value"); if (res == null || Q(res) == null || !dims.Contains("QueueName")) return "Para o AWS/SQS use --dimensions Name=QueueName,Value=NOME_DA_FILA (fila existente)"; }
                        else { res = Pick(dims, "Value"); if (res == null || DbI(res) == null || !dims.Contains("DBInstanceIdentifier")) return "Para o AWS/RDS use --dimensions Name=DBInstanceIdentifier,Value=ID_DO_BANCO"; }
                        string act = null;
                        string aa = Get(fl, "alarm-actions");
                        if (aa != null) { act = TopicFromArn(aa); if (act == null) return "--alarm-actions espera o ARN do tópico: arn:aws:sns:us-east-1:" + Arn + ":NOME"; }
                        var e = CreateAlarm(n, m, res, tv, ns, comp, act);
                        if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
                case "describe-alarms":
                    {
                        string sv = Get(fl, "state-value");
                        ok = true; Open("cloudwatch");
                        var rows = Alarms.Where(x => sv == null || StateEn(x.State) == sv).Select(a => "{ \"AlarmName\": \"" + a.Name + "\", \"Namespace\": \"" + a.Namespace + "\", \"MetricName\": \"" + a.Metric + "\", \"StateValue\": \"" + StateEn(a.State) + "\", \"AlarmActions\": [" + (a.Action != null ? "\"arn:aws:sns:us-east-1:" + Arn + ":" + a.Action + "\"" : "") + "] }").ToList();
                        return rows.Count == 0 ? "{ \"MetricAlarms\": [] }" : string.Join("\n", rows);
                    }
            }
            handled = false;
            return null;
        }

        string CliEcr(string sub, Dictionary<string, string> fl, out bool ok, out bool handled)
        {
            ok = false; handled = true;
            string rn = Get(fl, "repository-name"), err;
            switch (sub)
            {
                case "put-image-tag-mutability":
                    {
                        string m = Get(fl, "image-tag-mutability");
                        if (string.IsNullOrEmpty(rn) || string.IsNullOrEmpty(m)) return Need("repository-name", "image-tag-mutability");
                        if (m != "IMMUTABLE" && m != "MUTABLE") return "--image-tag-mutability aceita IMMUTABLE ou MUTABLE";
                        var e = SetImmutable(rn, m == "IMMUTABLE");
                        if (e != null) return AwsErr(e);
                        ok = true; return "{ \"repositoryName\": \"" + rn + "\", \"imageTagMutability\": \"" + m + "\" }";
                    }
                case "put-image-scanning-configuration":
                    {
                        string c = Get(fl, "image-scanning-configuration");
                        if (string.IsNullOrEmpty(rn) || string.IsNullOrEmpty(c)) return Need("repository-name", "image-scanning-configuration");
                        var e = SetScan(rn, IsTrue(Pick(c, "scanOnPush")));
                        if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
                case "put-lifecycle-policy":
                    {
                        if (string.IsNullOrEmpty(rn)) return Need("repository-name", "lifecycle-policy-text");
                        string txt = Param(fl, "lifecycle-policy-text", out err);
                        if (err != null) return err;
                        if (string.IsNullOrEmpty(txt)) return Need("lifecycle-policy-text");
                        string ts = Pick(txt, "tagStatus"), ct = Pick(txt, "countType"), cn = Pick(txt, "countNumber"), at = Pick(txt, "type");
                        int n;
                        if (ts == null || ct == null || !int.TryParse(cn, out n)) return "An error occurred (InvalidParameterException): a regra precisa de selection.tagStatus, countType e countNumber";
                        if (at != "expire") return "An error occurred (InvalidParameterException): a ação da regra é {\"type\":\"expire\"}";
                        if (ct == "sinceImagePushed" && Pick(txt, "countUnit") != "days") return "An error occurred (InvalidParameterException): com sinceImagePushed informe \"countUnit\":\"days\"";
                        var e = SetEcrLifecycle(rn, ts, ct, n);
                        if (e != null) return "An error occurred (InvalidParameterException): " + e;
                        ok = true; return "{ \"repositoryName\": \"" + rn + "\", \"lifecyclePolicyText\": \"(salva)\" }";
                    }
                case "get-lifecycle-policy":
                    {
                        var r = RepoOf(rn ?? "");
                        if (r == null) return "An error occurred (RepositoryNotFoundException)";
                        if (r.LcTag == null) return "An error occurred (LifecyclePolicyNotFoundException): Lifecycle policy does not exist for the repository";
                        ok = true; return "{ \"rules\": [{ \"rulePriority\": 1, \"selection\": { \"tagStatus\": \"" + r.LcTag + "\", \"countType\": \"" + r.LcType + "\", " + (r.LcType == "sinceImagePushed" ? "\"countUnit\": \"days\", " : "") + "\"countNumber\": " + r.LcN + " }, \"action\": { \"type\": \"expire\" } }] }";
                    }
            }
            handled = false;
            return null;
        }

        string CliSns(string sub, Dictionary<string, string> fl, out bool ok)
        {
            ok = false;
            string arn = Get(fl, "topic-arn");
            switch (sub)
            {
                case "list-topics":
                    ok = true; Open("sns");
                    return "{ \"Topics\": [\n" + string.Join(",\n", Topics.Select(x => "    { \"TopicArn\": \"arn:aws:sns:us-east-1:" + Arn + ":" + x.Name + "\" }")) + "\n] }";
                case "create-topic":
                    {
                        string n = Get(fl, "name");
                        if (string.IsNullOrEmpty(n)) return Need("name");
                        bool fifo = IsTrue(Pick(Get(fl, "attributes") ?? "", "FifoTopic"));
                        var e = CreateTopic(n, fifo);
                        if (e != null) return AwsErr(e);
                        ok = true; return "{\n    \"TopicArn\": \"arn:aws:sns:us-east-1:" + Arn + ":" + n + "\"\n}";
                    }
                case "delete-topic":
                    { string tn = TopicFromArn(arn); if (tn == null) return Need("topic-arn"); var e = DeleteTopic(tn); if (e != null) return AwsErr(e); ok = true; return ""; }
                case "subscribe":
                    {
                        string p = Get(fl, "protocol"), ep = Get(fl, "notification-endpoint");
                        if (string.IsNullOrEmpty(arn) || string.IsNullOrEmpty(p) || string.IsNullOrEmpty(ep)) return Need("topic-arn", "protocol", "notification-endpoint");
                        string tn = TopicFromArn(arn);
                        if (tn == null) return "An error occurred (InvalidParameter): --topic-arn deve ser arn:aws:sns:us-east-1:" + Arn + ":NOME_DO_TOPICO";
                        var e = Subscribe(tn, p, ep);
                        if (e != null) return "An error occurred (InvalidParameter): " + e;
                        ok = true; return "{\n    \"SubscriptionArn\": \"pending confirmation\"\n}\n(o destinatário precisa clicar no link de confirmação do e-mail)";
                    }
                case "list-subscriptions-by-topic":
                    {
                        string tn = TopicFromArn(arn);
                        var tp = tn == null ? null : Tp(tn);
                        if (tp == null) return "An error occurred (NotFound): Topic does not exist";
                        ok = true;
                        return "{ \"Subscriptions\": [" + string.Join(", ", tp.Subs.Select(s => "{ \"Protocol\": \"" + s[0] + "\", \"Endpoint\": \"" + s[1] + "\", \"SubscriptionArn\": \"" + (s[2] == "Confirmada" ? arn + ":" + Hx(s[1], 8) : "PendingConfirmation") + "\" }")) + "] }";
                    }
                case "publish":
                    {
                        string msg = Get(fl, "message");
                        if (string.IsNullOrEmpty(arn) || msg == null) return Need("topic-arn", "message");
                        string tn = TopicFromArn(arn);
                        if (tn == null) return "An error occurred (InvalidParameter): --topic-arn deve ser arn:aws:sns:us-east-1:" + Arn + ":NOME_DO_TOPICO";
                        var e = Publish(tn, msg);
                        if (e != null) return AwsErr(e);
                        ok = true; return "{\n    \"MessageId\": \"" + Hx(msg + tn, 8) + "-" + Hx(tn, 4) + "-5" + Hx(msg, 3) + "-9" + Hx(tn + "m", 3) + "-" + Hx(msg + "x", 12) + "\"\n}";
                    }
            }
            return Invalid("sns", sub);
        }

        string CliSqs(string sub, Dictionary<string, string> fl, out bool ok)
        {
            ok = false;
            string url = Get(fl, "queue-url"), err;
            switch (sub)
            {
                case "list-queues":
                    ok = true; Open("sqs");
                    return "{ \"QueueUrls\": [\n" + string.Join(",\n", Queues.Select(q => "    \"" + QueueUrl(q.Name) + "\"")) + "\n] }";
                case "get-queue-url":
                    {
                        string n = Get(fl, "queue-name");
                        if (Q(n ?? "") == null) return "An error occurred (AWS.SimpleQueueService.NonExistentQueue): The specified queue does not exist.";
                        ok = true; return "{\n    \"QueueUrl\": \"" + QueueUrl(n) + "\"\n}";
                    }
                case "create-queue":
                    {
                        string n = Get(fl, "queue-name");
                        if (string.IsNullOrEmpty(n)) return Need("queue-name");
                        string at = Param(fl, "attributes", out err) ?? "";
                        if (err != null) return err;
                        int vis = 30, wait = 0;
                        if (Pick(at, "VisibilityTimeout") != null) int.TryParse(Pick(at, "VisibilityTimeout"), out vis);
                        if (Pick(at, "ReceiveMessageWaitTimeSeconds") != null) int.TryParse(Pick(at, "ReceiveMessageWaitTimeSeconds"), out wait);
                        bool fifo = IsTrue(Pick(at, "FifoQueue")), dedup = IsTrue(Pick(at, "ContentBasedDeduplication"));
                        if (n.EndsWith(".fifo") && !fifo) return "An error occurred (InvalidParameterValue): para criar uma fila FIFO informe --attributes FifoQueue=true (e o nome termina com .fifo)";
                        var e = CreateQueue(n, fifo, dedup, vis, wait);
                        if (e != null) return "An error occurred (InvalidParameterValue): " + e;
                        ok = true; return "{\n    \"QueueUrl\": \"" + QueueUrl(n) + "\"\n}";
                    }
                case "delete-queue":
                    { string n = QueueFromUrl(url); if (n == null) return Need("queue-url"); var e = DeleteQueue(n); if (e != null) return AwsErr(e); ok = true; return ""; }
                case "get-queue-attributes":
                    {
                        string n = QueueFromUrl(url);
                        var q = n == null ? null : Q(n);
                        if (q == null) return "An error occurred (AWS.SimpleQueueService.NonExistentQueue): use --queue-url " + QueueUrl("NOME") + " (veja aws sqs list-queues)";
                        ok = true;
                        return "{ \"Attributes\": { \"VisibilityTimeout\": \"" + q.Visibility + "\", \"ReceiveMessageWaitTimeSeconds\": \"" + q.Wait + "\", \"FifoQueue\": \"" + (q.Fifo ? "true" : "false") + "\"" + (q.Fifo ? ", \"ContentBasedDeduplication\": \"" + (q.Dedup ? "true" : "false") + "\"" : "") + (q.Dlq != null ? ", \"RedrivePolicy\": \"{\\\"deadLetterTargetArn\\\":\\\"arn:aws:sqs:us-east-1:" + Arn + ":" + q.Dlq + "\\\",\\\"maxReceiveCount\\\":" + q.MaxReceive + "}\"" : "") + " } }";
                    }
                case "set-queue-attributes":
                    {
                        string n = QueueFromUrl(url);
                        if (string.IsNullOrEmpty(url)) return Need("queue-url", "attributes");
                        if (n == null || Q(n) == null) return "An error occurred (AWS.SimpleQueueService.NonExistentQueue): use a URL completa: " + QueueUrl("NOME") + " (veja aws sqs get-queue-url --queue-name NOME)";
                        string at = Param(fl, "attributes", out err);
                        if (err != null) return err;
                        if (string.IsNullOrEmpty(at)) return Need("attributes");
                        bool any = false;
                        foreach (var k in new[] { "VisibilityTimeout", "ReceiveMessageWaitTimeSeconds" })
                        {
                            string v = Pick(at, k);
                            if (v == null) continue;
                            int iv;
                            if (!int.TryParse(v, out iv)) return "An error occurred (InvalidAttributeValue): " + k + " deve ser um número de segundos";
                            var e = SetQueueAttr(n, k, iv);
                            if (e != null) return "An error occurred (InvalidAttributeValue): " + e;
                            any = true;
                        }
                        if (at.Contains("RedrivePolicy"))
                        {
                            string dlqArn = Rx(at, "deadLetterTargetArn\\W+(arn:aws:sqs:[a-z0-9\\-]+:\\d+:[A-Za-z0-9_\\-.]+)");
                            string mx = Rx(at, "maxReceiveCount\\W+(\\d+)");
                            int mr;
                            if (dlqArn == null || mx == null || !int.TryParse(mx, out mr)) return "An error occurred (InvalidAttributeValue): RedrivePolicy precisa de deadLetterTargetArn e maxReceiveCount";
                            string dq = QueueFromArn(dlqArn);
                            if (dq == null) return "An error occurred (InvalidAttributeValue): ARN da DLQ inválido: " + dlqArn;
                            if (dq == n) return "An error occurred (InvalidAttributeValue): a fila não pode ser a DLQ dela mesma.";
                            var e = SetRedrive(n, dq, mr);
                            if (e != null) return "An error occurred (InvalidAttributeValue): " + e;
                            any = true;
                        }
                        if (!any) return "An error occurred (InvalidAttributeName): neste laboratório use VisibilityTimeout, ReceiveMessageWaitTimeSeconds ou RedrivePolicy";
                        ok = true; return "";
                    }
            }
            return Invalid("sqs", sub);
        }

        string CliDdb(string sub, Dictionary<string, string> fl, List<string> t, out bool ok)
        {
            ok = false;
            string tn = Get(fl, "table-name");
            switch (sub)
            {
                case "list-tables":
                    ok = true; Open("dynamodb");
                    return "{\n    \"TableNames\": [ " + string.Join(", ", Tables.Select(x => "\"" + x.Name + "\"")) + " ]\n}";
                case "describe-table":
                    {
                        var x = Tb(tn ?? "");
                        if (x == null) return "An error occurred (ResourceNotFoundException): Requested resource not found: Table: " + tn + " not found";
                        ok = true;
                        return "{ \"Table\": { \"TableName\": \"" + x.Name + "\", \"KeySchema\": [{ \"AttributeName\": \"" + x.Pk + "\", \"KeyType\": \"HASH\" }" + (x.Sk != null ? ", { \"AttributeName\": \"" + x.Sk + "\", \"KeyType\": \"RANGE\" }" : "") + "], \"BillingModeSummary\": { \"BillingMode\": \"" + x.Billing + "\" }, \"DeletionProtectionEnabled\": " + (x.Protect ? "true" : "false") + ", \"ItemCount\": " + x.Items.Count + " } }";
                    }
                case "scan":
                    {
                        if (string.IsNullOrEmpty(tn)) return Need("table-name");
                        List<string> items;
                        var e = Scan(tn, out items);
                        if (e != null) return "An error occurred (ResourceNotFoundException): " + e;
                        ok = true; return "{ \"Items\": [\n    " + string.Join(",\n    ", items) + "\n], \"Count\": " + items.Count + " }";
                    }
                case "create-table":
                    {
                        if (string.IsNullOrEmpty(tn)) return Need("table-name", "attribute-definitions", "key-schema");
                        string ad = Multi(t, "attribute-definitions"), ks = Multi(t, "key-schema");
                        if (string.IsNullOrEmpty(ad) || string.IsNullOrEmpty(ks)) return Need("attribute-definitions", "key-schema");
                        var types = new Dictionary<string, string>();
                        foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(ad, "AttributeName\"?\\s*[:=]\\s*\"?([A-Za-z0-9_.\\-]+)\"?\\s*,\\s*\"?AttributeType\"?\\s*[:=]\\s*\"?([A-Z]+)"))
                            types[m.Groups[1].Value] = m.Groups[2].Value;
                        string pk = null, sk = null;
                        foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(ks, "AttributeName\"?\\s*[:=]\\s*\"?([A-Za-z0-9_.\\-]+)\"?\\s*,\\s*\"?KeyType\"?\\s*[:=]\\s*\"?([A-Z]+)"))
                        { if (m.Groups[2].Value == "HASH") pk = m.Groups[1].Value; else if (m.Groups[2].Value == "RANGE") sk = m.Groups[1].Value; }
                        if (pk == null) return "An error occurred (ValidationException): a key-schema precisa de uma chave KeyType=HASH (chave de partição)";
                        if (!types.ContainsKey(pk) || (sk != null && !types.ContainsKey(sk))) return "An error occurred (ValidationException): todo atributo da chave precisa estar em --attribute-definitions (com AttributeType S, N ou B)";
                        if (types.Count != (sk == null ? 1 : 2)) return "An error occurred (ValidationException): --attribute-definitions só deve listar os atributos usados na chave (o DynamoDB não tem esquema para os demais)";
                        string bm = Get(fl, "billing-mode");
                        if (bm == null && !fl.ContainsKey("provisioned-throughput")) return "An error occurred (ValidationException): One or more parameter values were invalid: ReadCapacityUnits and WriteCapacityUnits must both be specified when BillingMode is PROVISIONED (use --billing-mode PAY_PER_REQUEST para sob demanda)";
                        if (bm == null) bm = "PROVISIONED";
                        var e = CreateTable(tn, pk, types[pk], sk, sk != null ? types[sk] : null, bm);
                        if (e != null) return "An error occurred (ValidationException): " + e;
                        ok = true; return "{ \"TableDescription\": { \"TableName\": \"" + tn + "\", \"TableStatus\": \"CREATING\", \"BillingModeSummary\": { \"BillingMode\": \"" + bm + "\" } } }";
                    }
                case "update-time-to-live":
                    {
                        string spec = Get(fl, "time-to-live-specification");
                        if (string.IsNullOrEmpty(tn) || string.IsNullOrEmpty(spec)) return Need("table-name", "time-to-live-specification");
                        bool en = IsTrue(Pick(spec, "Enabled"));
                        string at = Pick(spec, "AttributeName");
                        if (string.IsNullOrEmpty(at)) return "Parameter validation failed: use Enabled=true,AttributeName=NOME";
                        var x = Tb(tn);
                        if (!en && x != null && x.Ttl != at) return "An error occurred (ValidationException): o TTL não está ativo com esse atributo";
                        var e = SetTtl(tn, en ? at : null);
                        if (e != null) return "An error occurred (ValidationException): " + e;
                        ok = true; return "{ \"TimeToLiveSpecification\": { \"Enabled\": " + (en ? "true" : "false") + ", \"AttributeName\": \"" + at + "\" } }";
                    }
                case "describe-time-to-live":
                    { var x = Tb(tn ?? ""); if (x == null) return "An error occurred (ResourceNotFoundException)"; ok = true; return "{ \"TimeToLiveDescription\": { \"TimeToLiveStatus\": \"" + (x.Ttl != null ? "ENABLED\", \"AttributeName\": \"" + x.Ttl : "DISABLED") + "\" } }"; }
                case "update-continuous-backups":
                    {
                        string spec = Get(fl, "point-in-time-recovery-specification");
                        if (string.IsNullOrEmpty(tn) || string.IsNullOrEmpty(spec)) return Need("table-name", "point-in-time-recovery-specification");
                        string v = Pick(spec, "PointInTimeRecoveryEnabled");
                        if (v == null) return "Parameter validation failed: use PointInTimeRecoveryEnabled=true";
                        var e = SetPitr(tn, IsTrue(v));
                        if (e != null) return "An error occurred (TableNotFoundException): " + e;
                        ok = true; return "{ \"ContinuousBackupsDescription\": { \"ContinuousBackupsStatus\": \"ENABLED\", \"PointInTimeRecoveryDescription\": { \"PointInTimeRecoveryStatus\": \"" + (IsTrue(v) ? "ENABLED" : "DISABLED") + "\" } } }";
                    }
                case "describe-continuous-backups":
                    { var x = Tb(tn ?? ""); if (x == null) return "An error occurred (TableNotFoundException)"; ok = true; return "{ \"PointInTimeRecoveryStatus\": \"" + (x.Pitr ? "ENABLED" : "DISABLED") + "\" }"; }
                case "update-table":
                    {
                        if (string.IsNullOrEmpty(tn)) return Need("table-name");
                        bool on = fl.ContainsKey("deletion-protection-enabled"), off = fl.ContainsKey("no-deletion-protection-enabled");
                        if (!on && !off) return "Neste laboratório, update-table altera a proteção: --deletion-protection-enabled (ou --no-deletion-protection-enabled)";
                        var e = SetTableProtect(tn, on);
                        if (e != null) return "An error occurred (ResourceNotFoundException): " + e;
                        ok = true; return "{ \"TableDescription\": { \"TableName\": \"" + tn + "\", \"DeletionProtectionEnabled\": " + (on ? "true" : "false") + " } }";
                    }
                case "delete-table":
                    { if (string.IsNullOrEmpty(tn)) return Need("table-name"); var e = DeleteTable(tn); if (e != null) return "An error occurred (ValidationException): " + e; ok = true; return ""; }
            }
            return Invalid("dynamodb", sub);
        }

        string CliRds(string sub, Dictionary<string, string> fl, out bool ok)
        {
            ok = false;
            string id = Get(fl, "db-instance-identifier");
            switch (sub)
            {
                case "describe-db-instances":
                    ok = true; Open("rds");
                    return "{ \"DBInstances\": [\n" + string.Join(",\n", Dbs.Where(d => id == null || d.Id == id).Select(d => "    { \"DBInstanceIdentifier\": \"" + d.Id + "\", \"Engine\": \"" + d.Engine + "\", \"DBInstanceClass\": \"" + d.Class + "\", \"DBInstanceStatus\": \"" + (d.Status == "Disponível" ? "available" : "stopped") + "\", \"MultiAZ\": " + (d.MultiAz ? "true" : "false") + ", \"PubliclyAccessible\": " + (d.Public ? "true" : "false") + ", \"BackupRetentionPeriod\": " + d.Backup + ", \"DeletionProtection\": " + (d.Protect ? "true" : "false") + (d.Pending.Count > 0 ? ", \"PendingModifiedValues\": { " + string.Join(", ", d.Pending.Select(kv => "\"" + kv.Key + "\": \"" + kv.Value + "\"")) + " }" : "") + " }")) + "\n] }";
                case "stop-db-instance":
                case "start-db-instance":
                case "reboot-db-instance":
                    {
                        if (string.IsNullOrEmpty(id)) return Need("db-instance-identifier");
                        var e = DbAction(id, sub.Split('-')[0]);
                        if (e != null) return AwsErr(e);
                        ok = true; return "{ \"DBInstance\": { \"DBInstanceIdentifier\": \"" + id + "\", \"DBInstanceStatus\": \"" + (sub.StartsWith("stop") ? "stopping" : sub.StartsWith("start") ? "starting" : "rebooting") + "\" } }";
                    }
                case "delete-db-instance":
                    {
                        if (string.IsNullOrEmpty(id)) return Need("db-instance-identifier");
                        if (!fl.ContainsKey("skip-final-snapshot") && Get(fl, "final-db-snapshot-identifier") == null) return "An error occurred (InvalidParameterCombination): informe --final-db-snapshot-identifier (ou --skip-final-snapshot)";
                        var e = DbAction(id, "delete");
                        if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
                case "create-db-snapshot":
                    {
                        string sn = Get(fl, "db-snapshot-identifier");
                        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(sn)) return Need("db-instance-identifier", "db-snapshot-identifier");
                        var e = DbSnapshot(id, sn);
                        if (e != null) return AwsErr(e);
                        ok = true; return "{ \"DBSnapshot\": { \"DBSnapshotIdentifier\": \"" + sn + "\", \"DBInstanceIdentifier\": \"" + id + "\", \"Status\": \"creating\", \"SnapshotType\": \"manual\" } }";
                    }
                case "describe-db-snapshots":
                    ok = true;
                    return "{ \"DBSnapshots\": [" + string.Join(", ", DbSnaps.Where(s => id == null || s.Db == id).Select(s => "{ \"DBSnapshotIdentifier\": \"" + s.Id + "\", \"DBInstanceIdentifier\": \"" + s.Db + "\", \"SnapshotType\": \"" + (s.Type == "Manual" ? "manual" : "automated") + "\" }")) + "] }";
                case "modify-db-instance":
                    {
                        if (string.IsNullOrEmpty(id)) return Need("db-instance-identifier");
                        var ch = new Dictionary<string, string>();
                        if (fl.ContainsKey("publicly-accessible")) ch["public"] = "true";
                        if (fl.ContainsKey("no-publicly-accessible")) ch["public"] = "false";
                        if (fl.ContainsKey("multi-az")) ch["multiaz"] = "true";
                        if (fl.ContainsKey("no-multi-az")) ch["multiaz"] = "false";
                        if (fl.ContainsKey("deletion-protection")) ch["protect"] = "true";
                        if (fl.ContainsKey("no-deletion-protection")) ch["protect"] = "false";
                        if (Get(fl, "backup-retention-period") != null) { int b; if (!int.TryParse(Get(fl, "backup-retention-period"), out b)) return "--backup-retention-period deve ser um número de dias"; ch["backup"] = b.ToString(); }
                        if (Get(fl, "db-instance-class") != null) ch["class"] = Get(fl, "db-instance-class");
                        if (ch.Count == 0) return "Informe o que modificar (ex.: --no-publicly-accessible, --multi-az, --deletion-protection, --backup-retention-period N)";
                        var e = ModifyDb(id, ch, fl.ContainsKey("apply-immediately"));
                        if (e != null) return AwsErr(e);
                        var d = DbI(id);
                        ok = true; return "{ \"DBInstance\": { \"DBInstanceIdentifier\": \"" + id + "\", \"PendingModifiedValues\": { " + string.Join(", ", d.Pending.Select(kv => "\"" + kv.Key + "\": \"" + kv.Value + "\"")) + " } } }" + (fl.ContainsKey("apply-immediately") ? "" : "\n(sem --apply-immediately: as mudanças ficam pendentes até a próxima janela de manutenção)");
                    }
            }
            return Invalid("rds", sub);
        }

        string CliR53(string sub, Dictionary<string, string> fl, out bool ok)
        {
            ok = false;
            string zid = (Get(fl, "hosted-zone-id") ?? "").Replace("/hostedzone/", ""), err;
            switch (sub)
            {
                case "list-hosted-zones":
                    ok = true; Open("route53");
                    return "{ \"HostedZones\": [{ \"Id\": \"/hostedzone/" + ZoneId + "\", \"Name\": \"" + Zone + ".\", \"ResourceRecordSetCount\": " + Records.Count + " }] }";
                case "list-resource-record-sets":
                    if (zid != ZoneId) return "An error occurred (NoSuchHostedZone): No hosted zone found with ID: " + zid + " (veja aws route53 list-hosted-zones)";
                    ok = true;
                    return "{ \"ResourceRecordSets\": [\n" + string.Join(",\n", Records.Select(r => "    { \"Name\": \"" + r.Name + ".\", \"Type\": \"" + r.Type + "\", \"TTL\": " + r.Ttl + ", \"ResourceRecords\": [{ \"Value\": \"" + r.Value + "\" }] }")) + "\n] }";
                case "change-resource-record-sets":
                    {
                        if (zid != ZoneId) return string.IsNullOrEmpty(zid) ? Need("hosted-zone-id", "change-batch") : "An error occurred (NoSuchHostedZone): No hosted zone found with ID: " + zid;
                        string cb = Param(fl, "change-batch", out err);
                        if (err != null) return err;
                        if (string.IsNullOrEmpty(cb)) return Need("change-batch");
                        var idx = System.Text.RegularExpressions.Regex.Matches(cb, "\"Action\"\\s*:").Cast<System.Text.RegularExpressions.Match>().Select(m => m.Index).ToList();
                        if (idx.Count == 0) return "An error occurred (InvalidChangeBatch): o change-batch precisa de Changes com Action, Name, Type, TTL e ResourceRecords";
                        for (int k = 0; k < idx.Count; k++)
                        {
                            string seg = cb.Substring(idx[k], (k + 1 < idx.Count ? idx[k + 1] : cb.Length) - idx[k]);
                            string act = Pick(seg, "Action"), name = Pick(seg, "Name"), type = Pick(seg, "Type"), ttl = Pick(seg, "TTL");
                            var vals = System.Text.RegularExpressions.Regex.Matches(seg, "\"Value\"\\s*:\\s*\"([^\"]*)\"").Cast<System.Text.RegularExpressions.Match>().Select(m => m.Groups[1].Value).ToList();
                            int tv;
                            if (act == null || name == null || type == null || !int.TryParse(ttl, out tv) || vals.Count == 0) return "An error occurred (InvalidInput): cada mudança precisa de Action, Name, Type, TTL e ResourceRecords [{\"Value\": ...}]";
                            if (vals.Count > 1) return "Neste laboratório, use um único valor por registro.";
                            var e = ChangeRecord(act, name, type, vals[0], tv);
                            if (e != null) return "An error occurred (InvalidChangeBatch): " + e;
                        }
                        ok = true; return "{\n    \"ChangeInfo\": { \"Id\": \"/change/C" + Hx(cb, 10).ToUpperInvariant() + "\", \"Status\": \"PENDING\" }\n}";
                    }
            }
            return Invalid("route53", sub);
        }

        string CliCf(string sub, Dictionary<string, string> fl, List<string> pos, out bool ok)
        {
            ok = false;
            switch (sub)
            {
                case "list-distributions":
                    ok = true; Open("cloudfront");
                    return "{ \"DistributionList\": { \"Items\": [" + string.Join(", ", Dists.Select(d => "{ \"Id\": \"" + d.Id + "\", \"DomainName\": \"" + d.Domain + "\", \"Origin\": \"" + d.Origin + "\", \"ViewerProtocolPolicy\": \"" + d.Viewer + "\", \"DefaultRootObject\": \"" + d.Root + "\" }")) + "] } }";
                case "get-distribution":
                    {
                        var d = Distr(Get(fl, "id") ?? "");
                        if (d == null) return "An error occurred (NoSuchDistribution): The specified distribution does not exist.";
                        ok = true; return "{ \"Distribution\": { \"Id\": \"" + d.Id + "\", \"DomainName\": \"" + d.Domain + "\", \"DistributionConfig\": { \"DefaultRootObject\": \"" + d.Root + "\", \"DefaultCacheBehavior\": { \"ViewerProtocolPolicy\": \"" + d.Viewer + "\" } } }, \"ETag\": \"E" + Hx(d.Viewer + d.Root, 12).ToUpperInvariant() + "\" }";
                    }
                case "create-invalidation":
                    {
                        string id = Get(fl, "distribution-id"), p = Get(fl, "paths");
                        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(p)) return Need("distribution-id", "paths");
                        var paths = new List<string> { p };
                        paths.AddRange(pos);
                        var e = Invalidate(id, paths);
                        if (e != null) return "An error occurred: " + e;
                        ok = true; return "{\n    \"Invalidation\": { \"Id\": \"I" + Hx(id + p, 12).ToUpperInvariant() + "\", \"Status\": \"InProgress\", \"InvalidationBatch\": { \"Paths\": { \"Quantity\": " + paths.Count + ", \"Items\": [ " + string.Join(", ", paths.Select(x => "\"" + x + "\"")) + " ] } } }\n}";
                    }
                case "list-invalidations":
                    {
                        var d = Distr(Get(fl, "distribution-id") ?? "");
                        if (d == null) return "An error occurred (NoSuchDistribution)";
                        ok = true; return "{ \"InvalidationList\": { \"Items\": [" + string.Join(", ", d.Invalidations.Select(x => "{ \"Paths\": \"" + x + "\", \"Status\": \"Completed\" }")) + "] } }";
                    }
                case "update-distribution":
                    return "Neste laboratório, altere a configuração da distribuição pelo CONSOLE (CloudFront → distribuição → Editar). Pela CLI, update-distribution exige o documento de configuração COMPLETO (get-distribution-config) e o --if-match com o ETag.";
            }
            return Invalid("cloudfront", sub);
        }

        string CliCt(string sub, Dictionary<string, string> fl, out bool ok)
        {
            ok = false;
            string n = Get(fl, "name");
            switch (sub)
            {
                case "describe-trails":
                    ok = true; Open("cloudtrail");
                    return "{ \"trailList\": [" + string.Join(", ", Trails.Select(x => "{ \"Name\": \"" + x.Name + "\", \"S3BucketName\": \"" + x.Bucket + "\", \"IsMultiRegionTrail\": " + (x.MultiRegion ? "true" : "false") + ", \"LogFileValidationEnabled\": " + (x.Validation ? "true" : "false") + " }")) + "] }";
                case "get-trail-status":
                    {
                        var x = Trails.FirstOrDefault(y => y.Name == n);
                        if (x == null) return "An error occurred (TrailNotFoundException): Unknown trail: " + n;
                        ok = true; return "{\n    \"IsLogging\": " + (x.Logging ? "true" : "false") + "\n}";
                    }
                case "create-trail":
                    {
                        string b = Get(fl, "s3-bucket-name");
                        if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(b)) return Need("name", "s3-bucket-name");
                        var e = CreateTrail(n, b, fl.ContainsKey("is-multi-region-trail"), fl.ContainsKey("enable-log-file-validation"), false);
                        if (e != null) return AwsErr(e);
                        ok = true; return "{\n    \"Name\": \"" + n + "\",\n    \"S3BucketName\": \"" + b + "\",\n    \"IsMultiRegionTrail\": " + (fl.ContainsKey("is-multi-region-trail") ? "true" : "false") + ",\n    \"LogFileValidationEnabled\": " + (fl.ContainsKey("enable-log-file-validation") ? "true" : "false") + "\n}\n(trilha criada pela CLI começa SEM registrar: rode start-logging)";
                    }
                case "start-logging":
                case "stop-logging":
                    { if (string.IsNullOrEmpty(n)) return Need("name"); var e = sub == "start-logging" ? StartLogging(n) : StopLogging(n); if (e != null) return AwsErr(e); ok = true; return ""; }
                case "delete-trail":
                    { if (string.IsNullOrEmpty(n)) return Need("name"); var e = DeleteTrail(n); if (e != null) return AwsErr(e); ok = true; return ""; }
            }
            return Invalid("cloudtrail", sub);
        }

        string CliKms(string sub, Dictionary<string, string> fl, out bool ok)
        {
            ok = false;
            string kid = Get(fl, "key-id");
            Func<string, string> aliasOf = k =>
            {
                if (k == null) return null;
                string id = k.StartsWith("arn:aws:kms:") ? Rx(k, "key/([0-9a-z\\-]+)$") : k;
                var r = KmsIds.FirstOrDefault(x => x[1] == id);
                return r == null ? null : r[0];
            };
            switch (sub)
            {
                case "list-keys":
                    ok = true; Open("kms");
                    return "{ \"Keys\": [\n" + string.Join(",\n", KmsIds.Select(k => "    { \"KeyId\": \"" + k[1] + "\", \"KeyArn\": \"arn:aws:kms:us-east-1:" + Arn + ":key/" + k[1] + "\" }")) + "\n] }";
                case "list-aliases":
                    ok = true;
                    return "{ \"Aliases\": [\n" + string.Join(",\n", KmsIds.Select(k => "    { \"AliasName\": \"" + k[0] + "\", \"TargetKeyId\": \"" + k[1] + "\" }")) + "\n] }";
                case "describe-key":
                    {
                        string al = kid != null && kid.StartsWith("alias/") ? (KmsIds.Any(x => x[0] == kid) ? kid : null) : aliasOf(kid);
                        if (al == null) return "An error occurred (NotFoundException): Key '" + kid + "' does not exist";
                        ok = true; return "{ \"KeyMetadata\": { \"KeyId\": \"" + KmsId(al) + "\", \"KeyManager\": \"" + (al == "alias/aws/s3" ? "AWS" : "CUSTOMER") + "\", \"KeyState\": \"Enabled\", \"Description\": \"" + KmsKeys.First(k => k[0] == al)[1] + "\" } }";
                    }
                case "enable-key-rotation":
                case "disable-key-rotation":
                case "get-key-rotation-status":
                    {
                        if (string.IsNullOrEmpty(kid)) return Need("key-id");
                        if (kid.StartsWith("alias/")) return "An error occurred (ValidationException) when calling the " + (sub == "enable-key-rotation" ? "EnableKeyRotation" : sub == "disable-key-rotation" ? "DisableKeyRotation" : "GetKeyRotationStatus") + " operation: esta operação não aceita alias — use o ID da chave (veja aws kms list-aliases).";
                        string al = aliasOf(kid);
                        if (al == null) return "An error occurred (NotFoundException): Key '" + kid + "' does not exist";
                        if (sub == "get-key-rotation-status") { ok = true; return "{\n    \"KeyRotationEnabled\": " + (Rotation(al) || al == "alias/aws/s3" ? "true" : "false") + "\n}"; }
                        var e = SetRotation(al, sub == "enable-key-rotation");
                        if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
            }
            return Invalid("kms", sub);
        }

        string CliBudgets(string sub, Dictionary<string, string> fl, out bool ok)
        {
            ok = false;
            string acc = (Get(fl, "account-id") ?? "").Replace("-", "");
            if (sub != "create-budget" && sub != "create-notification" && sub != "describe-budgets") return Invalid("budgets", sub);
            if (acc.Length == 0) return Need("account-id");
            if (acc != Arn) return "An error occurred (AccessDeniedException): a conta " + acc + " não é a sua (" + Arn + ").";
            string err;
            switch (sub)
            {
                case "describe-budgets":
                    ok = true; Open("budgets");
                    return "{ \"Budgets\": [" + string.Join(", ", Budgets.Select(b => "{ \"BudgetName\": \"" + b.Name + "\", \"BudgetLimit\": { \"Amount\": \"" + b.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\", \"Unit\": \"USD\" }, \"TimeUnit\": \"MONTHLY\", \"BudgetType\": \"COST\" }")) + "] }";
                case "create-budget":
                    {
                        string bt = Param(fl, "budget", out err);
                        if (err != null) return err;
                        if (string.IsNullOrEmpty(bt)) return Need("budget");
                        string name = Pick(bt, "BudgetName"), amount = Pick(bt, "Amount"), unit = Pick(bt, "TimeUnit"), type = Pick(bt, "BudgetType");
                        float a;
                        if (name == null || amount == null || !float.TryParse(amount, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out a)) return "Parameter validation failed: informe BudgetName, BudgetLimit={Amount=VALOR,Unit=USD}, TimeUnit e BudgetType";
                        if (unit != "MONTHLY") return "Neste laboratório, os orçamentos são mensais: TimeUnit=MONTHLY";
                        if (type != "COST") return "Neste laboratório, os orçamentos são de custo: BudgetType=COST";
                        if ((Pick(bt, "Unit") ?? "USD") != "USD") return "A unidade do orçamento é USD.";
                        var e = CreateBudget(name, a);
                        if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
                case "create-notification":
                    {
                        string bn = Get(fl, "budget-name"), nt = Get(fl, "notification"), sub2 = Get(fl, "subscribers");
                        if (string.IsNullOrEmpty(bn) || string.IsNullOrEmpty(nt) || string.IsNullOrEmpty(sub2)) return Need("budget-name", "notification", "subscribers");
                        float th;
                        if (!float.TryParse(Pick(nt, "Threshold"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out th)) return "Parameter validation failed: informe Threshold (ex.: 80)";
                        if ((Pick(nt, "ComparisonOperator") ?? "") != "GREATER_THAN") return "Neste laboratório, use ComparisonOperator=GREATER_THAN";
                        if ((Pick(sub2, "SubscriptionType") ?? "") != "EMAIL") return "Neste laboratório, use SubscriptionType=EMAIL";
                        var e = AddBudgetAlert(bn, Pick(nt, "NotificationType"), th, Pick(sub2, "Address"));
                        if (e != null) return AwsErr(e);
                        ok = true; return "";
                    }
            }
            return Invalid("budgets", sub);
        }

        static string StateEn(string s)
        {
            switch (s)
            {
                case "Executando": return "running";
                case "Interrompida": return "stopped";
                case "Encerrada": return "terminated";
                case "Em alarme": return "ALARM";
                case "OK": return "OK";
                default: return "INSUFFICIENT_DATA";
            }
        }
    }

    // Tarefa de um desafio de AWS: prepara o cenário e avalia cada evento (null = nada; "OK" = concluído; outro = erro explicado).
    class AwsTask
    {
        public Action<AwsSim> Setup;
        public Func<AwsSim, string, string> Eval;
        public Action<AwsSim> Solve;   // resolução "perfeita" (usada nos testes automáticos)
    }

    static class AwsTasks
    {
        public const string OK = "OK";

        static void EnsureSite(AwsSim s) { if (!s.Buckets.Any(b => b.Name == "cafe365-site")) s.Buckets.Add(new AwsSim.Bucket { Name = "cafe365-site", Region = "us-east-1" }); }

        public static readonly Dictionary<string, AwsTask> All = new Dictionary<string, AwsTask>
        {
            { "a1", new AwsTask {
                Setup = s => { },
                Eval = (s, e) => e == "open:s3" ? OK : null,
                Solve = s => s.Open("s3") } },
            { "a2", new AwsTask {
                Setup = s => s.Buckets.RemoveAll(b => b.Name == "cafe365-site"),
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("s3:create:")) return null;
                    var p = e.Split(':');
                    if (p[2] != "cafe365-site") return "Bucket criado com o nome '" + p[2] + "', mas o pedido é cafe365-site.";
                    if (p[3] == "pub") { s.Buckets.RemoveAll(b => b.Name == "cafe365-site"); return "O bucket foi criado com acesso PÚBLICO! A Helena apagou na hora por segurança. Crie de novo mantendo o bloqueio de acesso público."; }
                    return OK;
                },
                Solve = s => s.CreateBucket("cafe365-site", "us-east-1", true) } },
            { "a3", new AwsTask {
                Setup = s => { EnsureSite(s); s.Buckets.First(b => b.Name == "cafe365-site").Objects.Remove("index.html"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("s3:upload:")) return null;
                    var p = e.Split(':');
                    if (p[2] != "cafe365-site") return "Você enviou para o bucket " + p[2] + ". O site fica no cafe365-site.";
                    if (p[3] == "senhas.txt") return "Você enviou senhas.txt para a nuvem! Arquivos sensíveis nunca vão para um bucket de site. Exclua e envie só o index.html.";
                    if (p[3] != "index.html") return "Você enviou " + p[3] + ", mas a página pedida é index.html.";
                    return OK;
                },
                Solve = s => s.Upload("cafe365-site", new[] { "index.html" }) } },
            { "a4", new AwsTask {
                Setup = s => s.Region = "us-east-1",
                Eval = (s, e) => e == "region:sa-east-1" ? OK : null,
                Solve = s => s.SetRegion("sa-east-1") } },
            { "a5", new AwsTask {
                Setup = s => { },
                Eval = (s, e) => e == "sts" ? OK : null,
                Solve = s => s.Identity() } },
            { "b1", new AwsTask {
                Setup = s => { s.Region = "us-east-1"; foreach (var i in s.Instances.Where(x => x.Id == "i-0abc123" || x.Id == "i-0web001")) { i.State = "Executando"; } },
                Eval = (s, e) =>
                {
                    if (e == "ec2:stop:i-0abc123") return OK;
                    if (e == "ec2:terminate:i-0abc123") { s.Inst("i-0abc123").State = "Executando"; return "Você ENCERROU (apagou) a instância! O pedido era só interromper. A Helena restaurou do backup — tente de novo com 'Interromper'."; }
                    if (e.StartsWith("ec2:stop:i-0web001") || e.StartsWith("ec2:terminate:i-0web001")) { s.Inst("i-0web001").State = "Executando"; return "Opa! Você desligou o web-1, que atende os clientes. O site caiu por alguns segundos (já religamos). Selecione o teste-antigo."; }
                    return null;
                },
                Solve = s => s.SetState("i-0abc123", "stop") } },
            { "b2", new AwsTask {
                Setup = s => s.Instances.RemoveAll(i => i.Name == "api-cafe"),
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ec2:launch:")) return null;
                    var p = e.Split(':');
                    if (p[2] != "api-cafe") return "Instância criada com o nome '" + p[2] + "'. O nome pedido é api-cafe.";
                    if (p[3] != "t3.micro") { s.Instances.RemoveAll(i => i.Name == "api-cafe"); return "Tipo " + p[3] + " é maior (e mais caro) do que o necessário. A FinOps encerrou a instância — use t3.micro."; }
                    if (p[4] != "us-east-1") return "A instância foi criada na região " + p[4] + ". O time de API usa Norte da Virgínia (us-east-1).";
                    var inst = s.Instances.Last(i => i.Name == "api-cafe");
                    if (inst.Ami != "Amazon Linux 2023") return "A imagem escolhida foi " + inst.Ami + ". O pedido é Amazon Linux 2023.";
                    return OK;
                },
                Solve = s => { s.Region = "us-east-1"; s.Launch("api-cafe", "Amazon Linux 2023", "t3.micro"); } } },
            { "b3", new AwsTask {
                Setup = s => s.Users.RemoveAll(u => u.Name == "estagiario"),
                Eval = (s, e) =>
                {
                    if (!(e.StartsWith("iam:create:") || e.StartsWith("iam:group:"))) return null;
                    var p = e.Split(':');
                    if (p[2] != "estagiario") return "O usuário pedido é estagiario (sem acento).";
                    if (p[3] == "Leitura") return OK;
                    if (p[3] == "Administradores" || p[3] == "Desenvolvedores") { s.Users.RemoveAll(u => u.Name == "estagiario"); return "Permissão demais! " + p[3] + " pode alterar recursos. A Helena removeu o usuário: crie de novo no grupo Leitura (menor privilégio)."; }
                    return null;   // criado sem grupo: ainda falta adicionar ao grupo
                },
                Solve = s => s.CreateUser("estagiario", "Leitura") } },
            { "b4", new AwsTask {
                Setup = s => { var u = s.Users.FirstOrDefault(x => x.Name == "rodrigo"); if (u != null) u.Mfa = false; },
                Eval = (s, e) => e == "iam:mfa:rodrigo" ? OK : null,
                Solve = s => s.EnableMfa("rodrigo") } },
            { "b5", new AwsTask {
                Setup = s => { EnsureSite(s); var b = s.Buckets.First(x => x.Name == "cafe365-site"); b.Objects.RemoveAll(o => o != "index.html"); },
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("s3:upload:")) return null;
                    var p = e.Split(':');
                    if (p[3] == "senhas.txt") return "senhas.txt não faz parte do site e não deveria ir para a nuvem!";
                    var b = s.Buckets.FirstOrDefault(x => x.Name == "cafe365-site");
                    return b != null && AwsSim.SiteFiles.All(f => b.Objects.Contains(f)) ? OK : null;
                },
                Solve = s => s.Upload("cafe365-site", AwsSim.SiteFiles) } },
            { "c1", new AwsTask {
                Setup = s => s.Functions.ForEach(f => f.LastResult = null),
                Eval = (s, e) => e == "lambda:invoke:pedidos" ? OK : null,
                Solve = s => s.Invoke("pedidos") } },
            { "c2", new AwsTask {
                Setup = s => s.Repos.RemoveAll(r => r.Name == "cafe365-api"),
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("ecr:create:")) return null;
                    var p = e.Split(':');
                    if (p[2] != "cafe365-api") return "Repositório criado como '" + p[2] + "'. O nome pedido é cafe365-api.";
                    if (p[3] != "scan") { s.Repos.RemoveAll(r => r.Name == "cafe365-api"); return "Faltou ativar 'Verificar ao enviar' (scan on push). A segurança removeu o repositório: crie de novo com a verificação."; }
                    return OK;
                },
                Solve = s => s.CreateRepo("cafe365-api", true, false) } },
            { "c3", new AwsTask {
                Setup = s => { s.EcsDeployments = 1; s.EcsRunning = 2; },
                Eval = (s, e) =>
                {
                    if (e == "ecs:update:force") return OK;
                    if (e == "ecs:update:noforce") return "Serviço atualizado, mas SEM forçar nova implantação: como a definição não mudou, o ECS não subiu tarefas novas. Marque 'Forçar nova implantação'.";
                    return null;
                },
                Solve = s => s.UpdateService(true, 2) } },
            { "c4", new AwsTask {
                Setup = s => s.Alarms.RemoveAll(a => a.Name == "cpu-alta-web"),
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("cw:alarm:")) return null;
                    var p = e.Split(':');
                    if (p[2] != "cpu-alta-web") return "Alarme criado com o nome '" + p[2] + "'. O nome pedido é cpu-alta-web.";
                    if (p[3] != "CPUUtilization") return "A métrica escolhida foi " + p[3] + ". O pedido é CPUUtilization (uso de CPU).";
                    if (p[4] != "web-1") return "O alarme monitora " + p[4] + ", mas quem travou foi o web-1.";
                    if (p[5] != "80") return "O limite ficou em " + p[5] + ". O pedido é maior que 80.";
                    return OK;
                },
                Solve = s => s.CreateAlarm("cpu-alta-web", "CPUUtilization", "web-1", 80) } },
            { "c5", new AwsTask {
                Setup = s => s.Stacks.RemoveAll(x => x.Name == "cafe365"),
                Eval = (s, e) =>
                {
                    if (!e.StartsWith("cfn:create:")) return null;
                    var p = e.Split(':');
                    if (p[2] != "cafe365") return "Pilha criada como '" + p[2] + "'. O nome pedido é cafe365.";
                    if (p[3] != "CREATE_COMPLETE") return "A pilha falhou (ROLLBACK): o modelo cria recursos do IAM e isso precisa ser reconhecido (CAPABILITY_IAM).";
                    var st = s.Stacks.First(x => x.Name == "cafe365");
                    if (st.Template != "infra.yaml") return "Você usou o modelo " + st.Template + ". O pedido é infra.yaml.";
                    return OK;
                },
                Solve = s => s.CreateStack("cafe365", "infra.yaml", true) } },
        };
    }
}
