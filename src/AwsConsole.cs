using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace DockerQuest
{
    // Console de gerenciamento da AWS simulado (área de AWS). Navegação por serviços, listas, formulários
    // e ações reais sobre a conta simulada (AwsSim). O CloudShell fica embaixo, no próprio TerminalPanel.
    class AwsConsoleView : Panel
    {
        public readonly AwsSim Sim;
        readonly float scale;
        int S(float v) { return (int)Math.Round(v * scale); }

        static readonly Color Nav = Art.Hx(0x232F3E), NavHover = Art.Hx(0x37475A), Link = Art.Hx(0x0972D3), Primary = Art.Hx(0xEC7211),
                              Ink = Art.Hx(0x16191F), Muted = Art.Hx(0x5F6B7A), Line = Art.Hx(0xD5DBDB), Body = Art.Hx(0xF2F3F3),
                              OkGreen = Art.Hx(0x037F0C), ErrRed = Art.Hx(0xD13212), InfoBlue = Art.Hx(0x0972D3);
        readonly Font fUi, fBold, fH1, fH2, fSmall, fMono, fNav;

        Panel chrome, navbar, flash, body, side, main, servicesMenu, accountMenu;
        Label flashText;
        TextBox search;
#if DEMO
        public TextBox DebugSearch { get { return search; } }
#endif
        ComboBox region;
        string page = "home", arg;
        bool building, ready;   // ready: interface montada (antes disso, redimensionar não navega)

        public string PageTitle { get; private set; }

        class DbPanel : Panel { public DbPanel() { DoubleBuffered = true; ResizeRedraw = true; } }

        public AwsConsoleView(AwsSim sim, float scale)
        {
            Sim = sim; this.scale = scale;
            BackColor = Body;
            fUi = new Font("Segoe UI", 9.75f);
            fBold = new Font("Segoe UI Semibold", 9.75f);
            fH1 = new Font("Segoe UI Semibold", 16f);
            fH2 = new Font("Segoe UI Semibold", 12f);
            fSmall = new Font("Segoe UI", 8.5f);
            fMono = new Font("Consolas", 9.5f);
            fNav = new Font("Segoe UI Semibold", 9.5f);
            Build();
            Sim.Changed += () => { if (ready && !building && Visible && !IsForm(page)) Navigate(page, arg, false); };
        }

        static bool IsForm(string p) { return p.EndsWith("-create") || p.EndsWith("-launch") || p.EndsWith("-upload") || p.EndsWith("-mfa") || p.EndsWith("-update") || p.EndsWith("-edit") || p.EndsWith("-attach"); }

        // abas de páginas de detalhe (bucket, função Lambda)
        string s3Tab = "Objetos", lambdaTab = "Código";

        // ───────────────────────── estrutura ─────────────────────────

        void Build()
        {
            chrome = new DbPanel { Dock = DockStyle.Top, Height = S(34), BackColor = Art.Hx(0xDEE1E6) };
            chrome.Paint += PaintChrome;
            navbar = new DbPanel { Dock = DockStyle.Top, Height = S(42), BackColor = Nav };
            navbar.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                using (var f = new Font("Arial Black", 13f, GraphicsUnit.Point))
                    g.DrawString("aws", f, Brushes.White, S(12), S(6));
                using (var p = new Pen(Art.Hx(0xFF9900), Math.Max(2, S(2.2f))) { StartCap = LineCap.Round, EndCap = LineCap.ArrowAnchor })
                    g.DrawArc(p, S(14), S(14), S(40), S(16), 25, 120);
            };
            var logoHit = new Label { Location = new Point(0, 0), Size = new Size(S(64), S(42)), BackColor = Color.Transparent, Cursor = Cursors.Hand };
            logoHit.Click += (s, e) => Navigate("home", null);
            var services = NavLabel("Serviços  ▾", S(70));
            services.Click += (s, e) => { accountMenu.Visible = false; servicesMenu.Visible = !servicesMenu.Visible; if (servicesMenu.Visible) servicesMenu.BringToFront(); };
            search = new TextBox { Location = new Point(S(170), S(9)), Width = S(300), Font = fUi, BackColor = Art.Hx(0x0F1B2A), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            SetCue(search, "Pesquisar serviços (ex.: S3, EC2, IAM)   [Enter]");
            search.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; DoSearch(search.Text); } };
            region = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = fSmall, Width = S(180), DropDownWidth = S(260), FlatStyle = FlatStyle.Flat, BackColor = Nav, ForeColor = Color.White };
            foreach (var r in AwsSim.Regions)
            {
                int a = r[1].LastIndexOf('('), b = r[1].LastIndexOf(')');
                region.Items.Add(r[0] + "  (" + (a >= 0 && b > a ? r[1].Substring(a + 1, b - a - 1) : r[1]) + ")");
            }
            region.SelectedIndexChanged += (s, e) =>
            {
                if (building || region.SelectedIndex < 0) return;
                Sim.SetRegion(AwsSim.Regions[region.SelectedIndex][0]);
                Flash("Região alterada para " + AwsSim.RegionName(Sim.Region) + " (" + Sim.Region + ").", InfoBlue);
                Navigate(page, arg, false);
            };
            var account = NavLabel("estagio @ " + AwsSim.AccountId + "  ▾", 0);
            account.Click += (s, e) => { servicesMenu.Visible = false; accountMenu.Visible = !accountMenu.Visible; if (accountMenu.Visible) { accountMenu.BringToFront(); Sim.Identity(); } };
            navbar.Controls.AddRange(new Control[] { logoHit, services, search, region, account });
            navbar.Resize += (s, e) =>
            {
                // em janelas estreitas a conta vira só "estagio ▾" para sobrar espaço à pesquisa
                account.Text = navbar.Width < S(820) ? "estagio  ▾" : "estagio @ " + AwsSim.AccountId + "  ▾";
                account.Location = new Point(navbar.Width - account.Width - S(10), (navbar.Height - account.Height) / 2);
                region.Location = new Point(account.Left - region.Width - S(12), S(10));
                int room = region.Left - search.Left - S(14);
                search.Width = Math.Max(S(90), Math.Min(S(320), room));
                search.Visible = room >= S(90);
                region.Visible = region.Left >= services.Right + S(8);
            };

            flash = new DbPanel { Dock = DockStyle.Top, Height = S(40), Visible = false };
            flashText = new Label { Dock = DockStyle.Fill, Font = fBold, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(S(16), 0, S(40), 0), UseMnemonic = false };
            var flashClose = new Label { Dock = DockStyle.Right, Width = S(36), Text = "✕", Font = fBold, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };
            flashClose.Click += (s, e) => flash.Visible = false;
            flash.Controls.Add(flashText);
            flash.Controls.Add(flashClose);

            body = new Panel { Dock = DockStyle.Fill, BackColor = Body };
            side = new Panel { Dock = DockStyle.Left, Width = S(200), BackColor = Color.White, AutoScroll = true };
            body.Resize += (s, e) => side.Width = body.Width < S(760) ? S(170) : S(200);
            side.Paint += (s, e) => e.Graphics.DrawLine(Art.P(Line, 1), side.Width - 1, 0, side.Width - 1, side.Height);
            main = new Panel { Dock = DockStyle.Fill, BackColor = Body, AutoScroll = true };
            main.Resize += (s, e) => { if (ready && !building && Visible && !IsForm(page)) Navigate(page, arg, false); };
            body.Controls.Add(main);
            body.Controls.Add(side);

            Controls.Add(body);
            Controls.Add(flash);
            Controls.Add(navbar);
            Controls.Add(chrome);

            BuildServicesMenu();
            BuildAccountMenu();
            ready = true;
            Navigate("home", null, false);
        }

        Label NavLabel(string text, int x)
        {
            var l = new Label { Text = text, Font = fNav, ForeColor = Color.White, AutoSize = true, Location = new Point(x, S(12)), Cursor = Cursors.Hand, UseMnemonic = false, BackColor = Color.Transparent };
            l.MouseEnter += (s, e) => l.ForeColor = Art.Hx(0xFF9900);
            l.MouseLeave += (s, e) => l.ForeColor = Color.White;
            return l;
        }

        static void SetCue(TextBox t, string cue)
        {
            t.Text = cue; t.ForeColor = Art.Hx(0x8D99A8);
            t.GotFocus += (s, e) => { if (t.Text == cue) { t.Text = ""; t.ForeColor = Color.White; } };
            t.LostFocus += (s, e) => { if (t.Text.Length == 0) { t.Text = cue; t.ForeColor = Art.Hx(0x8D99A8); } };
        }

        void PaintChrome(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            // aba do navegador
            Art.RR(g, Color.White, S(8), S(5), S(230), S(29), S(8));
            g.FillRectangle(Brushes.White, S(8), S(20), S(230), S(14));
            Art.RR(g, Art.Hx(0xFF9900), S(16), S(12), S(14), S(14), S(3));
            g.DrawString(PageTitle + " | Console AWS", fSmall, Art.B(Ink), new RectangleF(S(36), S(11), S(180), S(18)));
            // barra de endereço
            float ax = S(250);
            Art.RR(g, Color.White, ax, S(5), chrome.Width - ax - S(12), S(24), S(12));
            g.DrawString("🔒", new Font("Segoe UI Symbol", 8f), Art.B(Muted), ax + S(8), S(9));
            g.DrawString("https://" + Sim.Region + ".console.aws.amazon.com/" + UrlPath() + "   (simulado)", fSmall, Art.B(Ink), ax + S(28), S(9));
        }

        string UrlPath()
        {
            string svc = ServiceOf(page);
            if (svc == null) return "console/home?region=" + Sim.Region;
            return svc + "/home?region=" + Sim.Region + "#" + page + (arg != null ? "/" + arg : "");
        }

        static string ServiceOf(string p)
        {
            if (p == "home") return null;
            if (p.StartsWith("s3")) return "s3";
            if (p.StartsWith("ec2")) return "ec2";
            if (p.StartsWith("iam")) return "iam";
            if (p.StartsWith("lambda")) return "lambda";
            if (p.StartsWith("cw") || p == "cloudwatch") return "cloudwatch";
            if (p.StartsWith("ecr")) return "ecr";
            if (p.StartsWith("ecs")) return "ecs";
            if (p.StartsWith("cfn") || p == "cloudformation") return "cloudformation";
            if (p.StartsWith("vpc")) return "vpc";
            if (p.StartsWith("sns")) return "sns";
            if (p.StartsWith("sqs")) return "sqs";
            if (p.StartsWith("ddb") || p == "dynamodb") return "dynamodb";
            if (p.StartsWith("rds")) return "rds";
            if (p.StartsWith("r53") || p == "route53") return "route53";
            if (p.StartsWith("cdn") || p == "cloudfront") return "cloudfront";
            if (p.StartsWith("trail") || p == "cloudtrail") return "cloudtrail";
            if (p.StartsWith("kms")) return "kms";
            if (p.StartsWith("budget")) return "budgets";
            return null;
        }

        static readonly string[][] ServiceList =
        {
            new[] { "s3", "S3", "Armazenamento escalável na nuvem", "Armazenamento" },
            new[] { "ec2", "EC2", "Servidores virtuais na nuvem", "Computação" },
            new[] { "lambda", "Lambda", "Executar código sem servidores", "Computação" },
            new[] { "ecs", "Elastic Container Service", "Executar containers", "Containers" },
            new[] { "ecr", "Elastic Container Registry", "Repositórios de imagens Docker", "Containers" },
            new[] { "rds", "RDS", "Bancos relacionais gerenciados", "Banco de dados" },
            new[] { "dynamodb", "DynamoDB", "Banco NoSQL gerenciado", "Banco de dados" },
            new[] { "vpc", "VPC", "Redes virtuais isoladas", "Redes e entrega de conteúdo" },
            new[] { "cloudfront", "CloudFront", "Rede de entrega de conteúdo (CDN)", "Redes e entrega de conteúdo" },
            new[] { "route53", "Route 53", "DNS e domínios", "Redes e entrega de conteúdo" },
            new[] { "sns", "Simple Notification Service", "Notificações", "Integração de aplicações" },
            new[] { "sqs", "Simple Queue Service", "Filas de mensagens", "Integração de aplicações" },
            new[] { "iam", "IAM", "Usuários, grupos e permissões", "Segurança, identidade e conformidade" },
            new[] { "kms", "Key Management Service", "Chaves de criptografia", "Segurança, identidade e conformidade" },
            new[] { "cloudwatch", "CloudWatch", "Métricas, alarmes e logs", "Gerenciamento e governança" },
            new[] { "cloudformation", "CloudFormation", "Infraestrutura como código", "Gerenciamento e governança" },
            new[] { "cloudtrail", "CloudTrail", "Auditoria das ações na conta", "Gerenciamento e governança" },
            new[] { "budgets", "Billing e Budgets", "Faturamento e orçamentos", "Gerenciamento de custos" },
        };
        static readonly string[] RootPages = ServiceList.Select(x => x[0]).ToArray();

        void BuildServicesMenu()
        {
            servicesMenu = new Panel { BackColor = Color.White, Visible = false, BorderStyle = BorderStyle.FixedSingle, AutoScroll = true };
            Controls.Add(servicesMenu);
            LayoutServicesMenu();
            Resize += (s, e) =>
            {
                LayoutServicesMenu();
                servicesMenu.Location = new Point(S(60), chrome.Height + navbar.Height);
                accountMenu.Location = new Point(Width - accountMenu.Width - S(10), chrome.Height + navbar.Height);
            };
        }

        // menu de serviços em uma ou duas colunas, com rolagem quando a janela é baixa
        int menuLayoutW = -1, menuLayoutH = -1;
        void LayoutServicesMenu()
        {
            if (servicesMenu == null || (menuLayoutW == Width && menuLayoutH == Height)) return;
            menuLayoutW = Width; menuLayoutH = Height;
            foreach (Control c in servicesMenu.Controls.Cast<Control>().ToList()) c.Dispose();
            servicesMenu.Controls.Clear();
            var groups = ServiceList.GroupBy(x => x[3]).ToList();
            int colW = Math.Max(S(300), ServiceList.Max(sv => TextRenderer.MeasureText(sv[1] + "  —  " + sv[2], fUi).Width) + S(36));
            bool two = Width >= S(60) + 2 * colW + S(40);
            var hs = groups.Select(g => S(22) + g.Count() * S(24) + S(6)).ToList();
            int split = groups.Count;
            if (two) { int acc = 0; for (int i = 0; i < groups.Count; i++) { acc += hs[i]; if (acc * 2 >= hs.Sum()) { split = i + 1; break; } } }
            int maxY = 0;
            for (int col = 0; col < (two ? 2 : 1); col++)
            {
                int y = S(12), x0 = S(16) + col * colW;
                for (int gi = col == 0 ? 0 : split; gi < (col == 0 ? split : groups.Count); gi++)
                {
                    var grp = groups[gi];
                    servicesMenu.Controls.Add(new Label { Text = grp.Key.ToUpperInvariant(), Font = fSmall, ForeColor = Muted, Location = new Point(x0, y), AutoSize = true });
                    y += S(22);
                    foreach (var sv in grp)
                    {
                        var svc = sv[0];
                        var l = new Label { Text = sv[1] + "  —  " + sv[2], Font = fUi, ForeColor = Link, Location = new Point(x0 + S(8), y), AutoSize = true, Cursor = Cursors.Hand, UseMnemonic = false };
                        l.Click += (s, e) => { servicesMenu.Visible = false; Navigate(svc, null); };
                        servicesMenu.Controls.Add(l);
                        y += S(24);
                    }
                    y += S(6);
                }
                maxY = Math.Max(maxY, y);
            }
            int avail = Math.Max(S(150), Height - S(34) - S(42) - S(10));
            int contentH = maxY + S(6);
            servicesMenu.Size = new Size(Math.Max(S(200), Math.Min(Width - S(70), (two ? 2 : 1) * colW + S(30))), Math.Min(contentH, avail));
        }

        void BuildAccountMenu()
        {
            accountMenu = new Panel { BackColor = Color.White, Visible = false, BorderStyle = BorderStyle.FixedSingle, Size = new Size(S(280), S(120)) };
            accountMenu.Controls.Add(new Label { Text = "ID da conta: " + AwsSim.AccountId, Font = fBold, ForeColor = Ink, Location = new Point(S(14), S(12)), AutoSize = true });
            accountMenu.Controls.Add(new Label { Text = "Usuário IAM: estagio", Font = fUi, ForeColor = Ink, Location = new Point(S(14), S(38)), AutoSize = true });
            accountMenu.Controls.Add(new Label { Text = "Organização: Micorosoft (simulado)", Font = fUi, ForeColor = Muted, Location = new Point(S(14), S(60)), AutoSize = true });
            var close = new Label { Text = "Fechar", Font = fUi, ForeColor = Link, Location = new Point(S(14), S(88)), AutoSize = true, Cursor = Cursors.Hand };
            close.Click += (s, e) => accountMenu.Visible = false;
            accountMenu.Controls.Add(close);
            Controls.Add(accountMenu);
        }

        void DoSearch(string q)
        {
            string f = Checker.Fold(q ?? "").Trim();
            if (f.Length == 0 || f.StartsWith("pesquisar")) return;
            var hit = ServiceList.FirstOrDefault(sv => Checker.Fold(sv[1]).Contains(f) || sv[0].Contains(f) || Checker.Fold(sv[2]).Contains(f));
            // palavras-chave → serviço (a primeira que bater vale)
            var keys = new[]
            {
                new[] { "s3", "bucket", "armazen", "objeto" },
                new[] { "ec2", "servidor", "instanc", "volume", "snapshot", "grupo de seguranca", "security group", "ip elastico", "elastic ip", "par de chaves", "key pair", "ami", "auto scaling", "autoscaling" },
                new[] { "kms", "criptograf", "rotacao" },
                new[] { "iam", "usuario", "permiss", "politica", "role", "funcao do iam", "chave de acesso", "access key", "senha" },
                new[] { "cloudtrail", "trilha", "auditoria" },
                new[] { "cloudwatch", "alarme", "log", "metrica" },
                new[] { "vpc", "sub-rede", "subrede", "subnet", "tabela de rotas", "rota ", "gateway nat" },
                new[] { "route53", "dns", "dominio", "registro" },
                new[] { "cloudfront", "cdn", "cache", "distribui", "invalida" },
                new[] { "dynamodb", "dynamo", "nosql" },
                new[] { "rds", "banco", "mysql", "postgres" },
                new[] { "sqs", "fila", "queue" },
                new[] { "sns", "topico", "notifica", "assinatura" },
                new[] { "budgets", "orcamento", "custo", "fatura", "billing", "budget" },
            };
            var kw = keys.FirstOrDefault(k => k.Skip(1).Any(w => f.Contains(w)));
            if (kw != null) hit = ServiceList.First(sv => sv[0] == kw[0]);
            if (hit == null) { Flash("Nenhum serviço encontrado para \"" + q + "\". Tente S3, EC2, IAM, VPC, RDS, DynamoDB, Lambda, SQS, SNS, CloudWatch, Route 53, CloudFront...", ErrRed); return; }
            search.Text = "";
            Navigate(hit[0], null);
        }

        public void Flash(string text, Color c)
        {
            flash.BackColor = c;
            flashText.Text = text;
            flash.Visible = true;
        }

        // ───────────────────────── navegação ─────────────────────────

        public void Navigate(string p, string a, bool fromUser = true)
        {
            if (building || !ready) return;
            building = true;
            try
            {
                bool changedService = ServiceOf(p) != ServiceOf(page) || p == "home";
                if (p == "s3-bucket" && page != "s3-bucket" && !page.StartsWith("s3-")) s3Tab = "Objetos";
                if (p == "s3-bucket" && page == "s3") s3Tab = "Objetos";
                if (p == "lambda-fn" && (page == "lambda" || !page.StartsWith("lambda"))) lambdaTab = "Código";
                if (fromUser && p == "cw-log") logFilter = null;
                page = p; arg = a;
                if (fromUser && changedService) flash.Visible = false;
                servicesMenu.Visible = false;
                int ri = Array.FindIndex(AwsSim.Regions, r => r[0] == Sim.Region);
                if (region.SelectedIndex != ri) region.SelectedIndex = ri;
                main.SuspendLayout();
                int scrollY = fromUser ? 0 : -main.AutoScrollPosition.Y;
                foreach (Control c in main.Controls.Cast<Control>().ToList()) c.Dispose();
                main.Controls.Clear();
                BuildSide();
                var pg = new Flow(this, main, S(24), S(18), Math.Max(S(340), main.ClientSize.Width - S(48)));
                switch (p)
                {
                    case "home": PageHome(pg); break;
                    case "s3": PageS3(pg); break;
                    case "s3-create": PageS3Create(pg); break;
                    case "s3-bucket": PageBucket(pg); break;
                    case "s3-upload": PageUpload(pg); break;
                    case "ec2": PageEc2(pg); break;
                    case "ec2-launch": PageLaunch(pg); break;
                    case "ec2-tags-edit": PageEc2Tags(pg); break;
                    case "ec2-type-edit": PageEc2Type(pg); break;
                    case "ec2-volumes": PageVolumes(pg); break;
                    case "ec2-snap-create": PageSnapCreate(pg); break;
                    case "ec2-snapshots": PageSnapshots(pg); break;
                    case "ec2-sg": PageSgs(pg); break;
                    case "ec2-sg-rules": PageSgRules(pg); break;
                    case "s3-versioning-edit": PageVersioning(pg); break;
                    case "s3-encryption-edit": PageEncryption(pg); break;
                    case "iam-policies": PageIamPolicies(pg); break;
                    case "iam-attach": PageIamAttach(pg); break;
                    case "iam": PageIam(pg); break;
                    case "iam-groups": PageIamGroups(pg); break;
                    case "iam-create": PageIamCreate(pg); break;
                    case "iam-user": PageIamUser(pg); break;
                    case "iam-mfa": PageMfa(pg); break;
                    case "lambda": PageLambda(pg); break;
                    case "lambda-fn": PageLambdaFn(pg); break;
                    case "cloudwatch": PageAlarms(pg); break;
                    case "cw-create": PageAlarmCreate(pg); break;
                    case "cw-logs": PageLogs(pg); break;
                    case "cw-log": PageLog(pg); break;
                    case "ecr": PageEcr(pg); break;
                    case "ecr-create": PageEcrCreate(pg); break;
                    case "ecs": PageEcs(pg); break;
                    case "ecs-cluster": PageEcsCluster(pg); break;
                    case "ecs-update": PageEcsUpdate(pg); break;
                    case "cloudformation": PageCfn(pg); break;
                    case "cfn-create": PageCfnCreate(pg); break;
                    default: if (!PageExtra(p, pg)) PageHome(pg); break;
                }
                pg.Spacer(S(30));
                main.ResumeLayout();
                if (scrollY > 0) main.AutoScrollPosition = new Point(0, scrollY);
                chrome.Invalidate();
            }
            finally { building = false; }
            if (fromUser && RootPages.Contains(p))
                Sim.Open(p);
            if (fromUser && p == "cw-log" && a != null) Sim.OpenLogs(a);
            if (fromUser && p == "ddb-items" && a != null) { List<string> items; Sim.Scan(a, out items); }
        }

        void BuildSide()
        {
            foreach (Control c in side.Controls.Cast<Control>().ToList()) c.Dispose();
            side.Controls.Clear();
            string svc = ServiceOf(page);
            var items = new List<string[]>();   // texto, página
            string title;
            switch (svc)
            {
                case "s3": title = "Amazon S3"; items.Add(new[] { "Buckets", "s3" }); items.Add(new[] { "Pontos de acesso", "" }); items.Add(new[] { "Lente de armazenamento", "" }); break;
                case "ec2": title = "EC2"; items.Add(new[] { "Instâncias", "ec2" }); items.Add(new[] { "Executar instância", "ec2-launch" }); items.Add(new[] { "AMIs", "ec2-amis" }); items.Add(new[] { "Volumes (EBS)", "ec2-volumes" }); items.Add(new[] { "Snapshots", "ec2-snapshots" }); items.Add(new[] { "Grupos de segurança", "ec2-sg" }); items.Add(new[] { "IPs elásticos", "ec2-eips" }); items.Add(new[] { "Pares de chaves", "ec2-keys" }); items.Add(new[] { "Grupos do Auto Scaling", "ec2-asg" }); items.Add(new[] { "Configurações da conta", "ec2-settings" }); break;
                case "iam": title = "Identity and Access Management (IAM)"; items.Add(new[] { "Usuários", "iam" }); items.Add(new[] { "Grupos de usuários", "iam-groups" }); items.Add(new[] { "Funções (roles)", "iam-roles" }); items.Add(new[] { "Políticas", "iam-policies" }); items.Add(new[] { "Configurações da conta", "iam-settings" }); break;
                case "vpc": title = "VPC"; items.Add(new[] { "Suas VPCs", "vpc" }); items.Add(new[] { "Sub-redes", "vpc-subnets" }); items.Add(new[] { "Tabelas de rotas", "vpc-rtbs" }); items.Add(new[] { "Gateways da Internet", "" }); items.Add(new[] { "Gateways NAT", "" }); break;
                case "sns": title = "Amazon SNS"; items.Add(new[] { "Tópicos", "sns" }); items.Add(new[] { "Assinaturas", "" }); break;
                case "sqs": title = "Amazon SQS"; items.Add(new[] { "Filas", "sqs" }); break;
                case "dynamodb": title = "DynamoDB"; items.Add(new[] { "Tabelas", "dynamodb" }); items.Add(new[] { "Explorar itens", "ddb-items" }); items.Add(new[] { "Backups", "" }); break;
                case "rds": title = "Amazon RDS"; items.Add(new[] { "Bancos de dados", "rds" }); items.Add(new[] { "Snapshots", "rds-snapshots" }); break;
                case "route53": title = "Route 53"; items.Add(new[] { "Zonas hospedadas", "route53" }); items.Add(new[] { "Domínios registrados", "" }); break;
                case "cloudfront": title = "CloudFront"; items.Add(new[] { "Distribuições", "cloudfront" }); items.Add(new[] { "Políticas", "" }); break;
                case "cloudtrail": title = "CloudTrail"; items.Add(new[] { "Trilhas", "cloudtrail" }); items.Add(new[] { "Histórico de eventos", "" }); break;
                case "kms": title = "Key Management Service"; items.Add(new[] { "Chaves gerenciadas pelo cliente", "kms" }); items.Add(new[] { "Chaves gerenciadas pela AWS", "" }); break;
                case "budgets": title = "Billing e Gerenciamento de Custos"; items.Add(new[] { "Orçamentos", "budgets" }); items.Add(new[] { "Cost Explorer", "" }); break;
                case "lambda": title = "AWS Lambda"; items.Add(new[] { "Funções", "lambda" }); items.Add(new[] { "Camadas", "" }); break;
                case "cloudwatch": title = "CloudWatch"; items.Add(new[] { "Alarmes", "cloudwatch" }); items.Add(new[] { "Grupos de logs", "cw-logs" }); items.Add(new[] { "Métricas", "" }); break;
                case "ecr": title = "Amazon ECR"; items.Add(new[] { "Repositórios privados", "ecr" }); break;
                case "ecs": title = "Amazon ECS"; items.Add(new[] { "Clusters", "ecs" }); items.Add(new[] { "Definições de tarefa", "" }); break;
                case "cloudformation": title = "CloudFormation"; items.Add(new[] { "Pilhas", "cloudformation" }); items.Add(new[] { "Designer", "" }); break;
                default: title = "Console AWS"; foreach (var sv in ServiceList) items.Add(new[] { sv[1], sv[0] }); break;
            }
            int y = S(16);
            side.AutoScrollPosition = new Point(0, 0);
            // largura sem a barra de rolagem vertical: nomes longos quebram linha em vez de criar rolagem horizontal
            int lw = Math.Max(S(80), side.Width - S(20) - SystemInformation.VerticalScrollBarWidth);
            side.Controls.Add(new Label { Text = title, Font = fH2, ForeColor = Ink, Location = new Point(S(16), y), Size = new Size(lw, S(44)), UseMnemonic = false });
            y += S(50);
            // item ativo: o de destino mais longo que é prefixo da página atual (ex.: ec2-asg-edit → Grupos do Auto Scaling)
            string best = items.Select(x => x[1]).Where(x => x.Length > 0 && (page == x || page.StartsWith(x + "-"))).OrderByDescending(x => x.Length).FirstOrDefault();
            foreach (var it in items)
            {
                var target = it[1];
                bool active = target == best;
                var lf = active ? fBold : fUi;
                int lh = TextRenderer.MeasureText(it[0], lf, new Size(lw, 0), TextFormatFlags.WordBreak).Height;
                var l = new Label { Text = it[0], Font = lf, ForeColor = target.Length == 0 ? Muted : active ? Art.Hx(0xEC7211) : Ink, Location = new Point(S(16), y), Size = new Size(lw, lh + S(2)), Cursor = Cursors.Hand, UseMnemonic = false };
                l.Click += (s, e) =>
                {
                    if (target.Length == 0) Flash("Esse recurso existe na AWS de verdade, mas não é usado neste laboratório.", InfoBlue);
                    else Navigate(target, null);
                };
                side.Controls.Add(l);
                y += Math.Max(S(28), lh + S(10));
            }
        }

        // ───────────────────────── construtor de páginas ─────────────────────────

        class Flow
        {
            readonly AwsConsoleView v;
            public readonly Control Host;
            public int X, Y, W;
            public Flow(AwsConsoleView v, Control host, int x, int y, int w) { this.v = v; Host = host; X = x; Y = y; W = w; }
            int S(float f) { return v.S(f); }

            public Label Text(string t, Font f, Color c, int indent = 0)
            {
                var sz = TextRenderer.MeasureText(t, f, new Size(W - indent, 0), TextFormatFlags.WordBreak);
                var l = new Label { Text = t, Font = f, ForeColor = c, Location = new Point(X + indent, Y), Size = new Size(W - indent, sz.Height + S(2)), BackColor = Color.Transparent, UseMnemonic = false };
                Host.Controls.Add(l);
                Y += l.Height + S(6);
                return l;
            }

            public void Spacer(int h) { var p = new Panel { Location = new Point(X, Y), Size = new Size(1, h), BackColor = Color.Transparent }; Host.Controls.Add(p); Y += h; }

            public void Crumbs(params string[] parts)
            {
                int x = X;
                for (int i = 0; i < parts.Length; i++)
                {
                    bool last = i == parts.Length - 1;
                    var l = new Label { Text = parts[i], Font = v.fSmall, ForeColor = last ? Muted : Link, AutoSize = true, Location = new Point(x, Y), UseMnemonic = false };
                    Host.Controls.Add(l);
                    x += TextRenderer.MeasureText(parts[i], v.fSmall).Width + S(2);
                    if (!last) { Host.Controls.Add(new Label { Text = "›", Font = v.fSmall, ForeColor = Muted, AutoSize = true, Location = new Point(x, Y) }); x += S(14); }
                }
                Y += S(24);
            }

            // título com botões à direita
            public void Header(string title, string info, params Button[] buttons) { HeaderCore(title, v.fH1, S(40), info, buttons); }

            // título de seção (menor) com botões à direita — quebra em outra linha quando falta espaço
            public void SubHeader(string title, params Button[] buttons) { HeaderCore(title, v.fH2, S(38), null, buttons); }

            // abas (ex.: Objetos | Propriedades | Permissões | Gerenciamento)
            public void Tabs(string[] tabs, string current, Action<string> pick)
            {
                int x = X;
                foreach (var tb in tabs)
                {
                    string tname = tb;
                    bool on = tb == current;
                    var f = on ? v.fBold : v.fUi;
                    int w = TextRenderer.MeasureText(tb, f).Width + S(4);
                    if (x > X && x + w > X + W) { x = X; Y += S(30); }
                    var l = new Label { Text = tb, Font = f, ForeColor = on ? Primary : Link, Location = new Point(x, Y), Size = new Size(w, S(26)), Cursor = Cursors.Hand, UseMnemonic = false, BackColor = Color.Transparent };
                    if (on) l.Paint += (s, e) => e.Graphics.FillRectangle(Art.B(Primary), 0, l.Height - S(3), l.Width, S(3));
                    l.Click += (s, e) => pick(tname);
                    Host.Controls.Add(l);
                    x += w + S(22);
                }
                Y += S(28);
                Host.Controls.Add(new Panel { Location = new Point(X, Y), Size = new Size(W, 1), BackColor = Line });
                Y += S(14);
            }

            void HeaderCore(string title, Font font, int rowH, string info, Button[] buttons)
            {
                var t = new Label { Text = title, Font = font, ForeColor = Ink, AutoSize = true, Location = new Point(X, Y), UseMnemonic = false };
                Host.Controls.Add(t);
                int need = buttons.Sum(b => b.Width) + S(8) * Math.Max(0, buttons.Length - 1);
                if (buttons.Length > 0 && X + t.PreferredWidth + S(16) + need > X + W)
                {
                    // não cabe ao lado do título: botões na(s) linha(s) de baixo, alinhados à esquerda
                    Y += rowH;
                    int bx = X;
                    foreach (var b in buttons)
                    {
                        if (bx > X && bx + b.Width > X + W) { bx = X; Y += b.Height + S(8); }
                        b.Location = new Point(bx, Y);
                        Host.Controls.Add(b);
                        b.BringToFront();
                        bx += b.Width + S(8);
                    }
                    Y += buttons.Max(b => b.Height) + S(10);
                }
                else
                {
                    int bx = X + W;
                    foreach (var b in buttons.Reverse())
                    {
                        bx -= b.Width;
                        b.Location = new Point(bx, Y + S(2));
                        Host.Controls.Add(b);
                        b.BringToFront();
                        bx -= S(8);
                    }
                    Y += rowH;
                }
                if (info != null) Text(info, v.fSmall, Muted);
            }

            public Panel Box(int height)
            {
                var p = new Panel { Location = new Point(X, Y), Size = new Size(W, height), BackColor = Color.White };
                p.Paint += (s, e) => e.Graphics.DrawRectangle(Art.P(Line, 1), 0, 0, p.Width - 1, p.Height - 1);
                Host.Controls.Add(p);
                Y += height + S(16);
                return p;
            }

            // container branco que cresce conforme o conteúdo
            public Flow Section(string title, out Panel box)
            {
                box = new Panel { Location = new Point(X, Y), Size = new Size(W, S(60)), BackColor = Color.White };
                var bx = box;
                bx.Paint += (s, e) => e.Graphics.DrawRectangle(Art.P(Line, 1), 0, 0, bx.Width - 1, bx.Height - 1);
                Host.Controls.Add(bx);
                var inner = new Flow(v, bx, S(20), S(14), W - S(40));
                if (title != null) { inner.Text(title, v.fH2, Ink); }
                return inner;
            }

            public void EndSection(Panel box, Flow inner)
            {
                box.Height = inner.Y + S(10);
                Y += box.Height + S(16);
            }

            public TextBox Field(string label, string hint, string value = "")
            {
                Text(label, v.fBold, Ink);
                if (hint != null) Text(hint, v.fSmall, Muted);
                var t = new TextBox { Location = new Point(X, Y), Width = Math.Min(W, S(460)), Font = v.fUi, Text = value };
                Host.Controls.Add(t);
                Y += t.Height + S(12);
                return t;
            }

            public ComboBox Select(string label, string[] items, int selected)
            {
                Text(label, v.fBold, Ink);
                var c = new ComboBox { Location = new Point(X, Y), Width = Math.Min(W, S(460)), Font = v.fUi, DropDownStyle = ComboBoxStyle.DropDownList };
                c.Items.AddRange(items);
                if (selected >= 0 && selected < items.Length) c.SelectedIndex = selected;
                Host.Controls.Add(c);
                Y += c.Height + S(12);
                return c;
            }

            public CheckBox Check(string text, bool on, string hint = null)
            {
                var c = new CheckBox { Text = text, Checked = on, Font = v.fBold, ForeColor = Ink, Location = new Point(X, Y), AutoSize = true, UseMnemonic = false };
                Host.Controls.Add(c);
                Y += S(26);
                if (hint != null) Text(hint, v.fSmall, Muted, S(20));
                return c;
            }

            public List<RadioButton> Radios(string label, string[] options, int selected)
            {
                if (label != null) Text(label, v.fBold, Ink);
                var list = new List<RadioButton>();
                var holder = new Panel { Location = new Point(X, Y), Size = new Size(W, options.Length * S(26)), BackColor = Color.Transparent };
                for (int i = 0; i < options.Length; i++)
                {
                    var r = new RadioButton { Text = options[i], Font = v.fUi, ForeColor = Ink, Location = new Point(S(4), i * S(26)), AutoSize = true, Checked = i == selected, UseMnemonic = false };
                    holder.Controls.Add(r);
                    list.Add(r);
                }
                Host.Controls.Add(holder);
                Y += holder.Height + S(8);
                return list;
            }

            public void Buttons(params Button[] buttons)
            {
                int need = buttons.Sum(x => x.Width) + S(8) * Math.Max(0, buttons.Length - 1);
                if (need > W)
                {
                    // não cabe numa linha: alinhados à esquerda, quebrando
                    int lx = X;
                    foreach (var b in buttons)
                    {
                        if (lx > X && lx + b.Width > X + W) { lx = X; Y += b.Height + S(8); }
                        b.Location = new Point(lx, Y);
                        Host.Controls.Add(b);
                        lx += b.Width + S(8);
                    }
                    Y += S(44);
                    return;
                }
                int bx = X + W;
                foreach (var b in buttons.Reverse())
                {
                    bx -= b.Width;
                    b.Location = new Point(bx, Y);
                    Host.Controls.Add(b);
                    bx -= S(8);
                }
                Y += S(44);
            }

            public Label Error()
            {
                var l = new Label { Font = v.fBold, ForeColor = ErrRed, Location = new Point(X, Y), Size = new Size(W, S(36)), UseMnemonic = false };
                Host.Controls.Add(l);
                Y += S(36);
                return l;
            }

            // tabela: colunas (título, peso); linhas; primeira coluna pode ser um link
            public ListView Table(string[] cols, float[] weights, List<string[]> rows, Action<int> onLink, bool selectable = true)
            {
                var lv = new ListView
                {
                    View = View.Details, FullRowSelect = true, MultiSelect = false, HideSelection = false, GridLines = false,
                    Location = new Point(X, Y), Width = W, Font = v.fUi, BorderStyle = BorderStyle.FixedSingle, HeaderStyle = ColumnHeaderStyle.Nonclickable,
                    ForeColor = Ink
                };
                float tw = weights.Sum();
                for (int i = 0; i < cols.Length; i++) lv.Columns.Add(cols[i], (int)((W - S(6)) * weights[i] / tw));
                foreach (var r in rows)
                {
                    var it = new ListViewItem(r[0]) { UseItemStyleForSubItems = false };
                    if (onLink != null) { it.ForeColor = Link; it.Font = new Font(v.fUi, FontStyle.Underline); }
                    for (int i = 1; i < r.Length; i++)
                    {
                        var sub = it.SubItems.Add(r[i]);
                        if (r[i].StartsWith("●")) sub.ForeColor = r[i].Contains("Executando") || r[i].Contains("OK") || r[i].Contains("COMPLETE") || r[i].Contains("Ativado") || r[i].Contains("Em uso") || r[i].Contains("Concluído") ? OkGreen : r[i].Contains("alarme") || r[i].Contains("ROLLBACK") || r[i].Contains("Desativado") || r[i].Contains("Encerrada") ? ErrRed : Muted;
                    }
                    lv.Items.Add(it);
                }
                int rowH = TextRenderer.MeasureText("Ay", v.fUi).Height + S(8);
                lv.Height = S(30) + Math.Max(1, rows.Count) * rowH + S(4);
                if (onLink != null)
                    lv.MouseClick += (s, e) =>
                    {
                        var hit = lv.HitTest(e.Location);
                        if (hit.Item != null && hit.Item.SubItems.IndexOf(hit.SubItem) == 0) onLink(hit.Item.Index);
                    };
                if (!selectable) lv.SelectedIndexChanged += (s, e) => { };
                Host.Controls.Add(lv);
                Y += lv.Height + S(10);
                if (rows.Count == 0) Text("Nenhum recurso encontrado nesta região.", v.fSmall, Muted);
                return lv;
            }
        }

        Button Btn(string text, bool primary, Action click)
        {
            var b = new Button
            {
                Text = text, Font = fBold, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, UseVisualStyleBackColor = false, TabStop = true,
                BackColor = primary ? Primary : Color.White, ForeColor = primary ? Color.White : Art.Hx(0x0F141A), UseMnemonic = false
            };
            b.FlatAppearance.BorderColor = primary ? Primary : Art.Hx(0x7D8998);
            b.FlatAppearance.BorderSize = primary ? 0 : 1;
            b.FlatAppearance.MouseOverBackColor = primary ? Art.Hx(0xEB5F07) : Art.Hx(0xF2F8FD);
            var sz = TextRenderer.MeasureText(text, fBold);
            b.Size = new Size(sz.Width + S(32), S(32));
            b.Click += (s, e) => click();
            return b;
        }

        // ───────────────────────── páginas ─────────────────────────

        void PageHome(Flow pg)
        {
            PageTitle = "Página inicial do console";
            pg.Header("Página inicial do console", "Bem-vindo(a) ao Console de Gerenciamento da AWS (simulado). Use o menu Serviços, a pesquisa ou os atalhos abaixo.");
            Panel box;
            var f = pg.Section("Visitados recentemente", out box);
            int col = 0, rowY = f.Y;
            int cw = (f.W - S(20)) / 2;
            foreach (var sv in ServiceList)
            {
                var svc = sv[0];
                var l = new Label { Text = sv[1], Font = fBold, ForeColor = Link, Location = new Point(f.X + col * (cw + S(20)), rowY), AutoSize = true, Cursor = Cursors.Hand, UseMnemonic = false };
                l.Click += (s, e) => Navigate(svc, null);
                box.Controls.Add(l);
                box.Controls.Add(new Label { Text = sv[2], Font = fSmall, ForeColor = Muted, Location = new Point(f.X + col * (cw + S(20)), rowY + S(20)), AutoSize = true });
                col++;
                if (col == 2) { col = 0; rowY += S(50); }
            }
            f.Y = rowY + (col > 0 ? S(50) : 0);
            pg.EndSection(box, f);
            var g = pg.Section("Integridade da AWS", out box);
            g.Text("● Nenhum problema nos serviços da região " + AwsSim.RegionName(Sim.Region) + ".", fUi, OkGreen);
            g.Text("Região atual: " + Sim.Region + ". A região é escolhida no canto superior direito.", fSmall, Muted);
            pg.EndSection(box, g);
        }

        // ── S3
        void PageS3(Flow pg)
        {
            PageTitle = "S3";
            pg.Crumbs("Amazon S3", "Buckets");
            var lvRef = new ListView[1];
            pg.Header("Buckets (" + Sim.Buckets.Count + ")", "Buckets são contêineres de dados armazenados no S3. Nomes de bucket são únicos no mundo inteiro.",
                Btn("Excluir", false, () =>
                {
                    var lv = lvRef[0];
                    if (lv == null || lv.SelectedIndices.Count == 0) { Flash("Selecione um bucket na lista primeiro.", InfoBlue); return; }
                    var b = Sim.Buckets[lv.SelectedIndices[0]];
                    if (MessageBox.Show(FindForm(), "Excluir o bucket " + b.Name + " e todos os objetos dele?", "Excluir bucket", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                    Sim.DeleteBucket(b.Name);
                    Flash("Bucket " + b.Name + " excluído.", OkGreen);
                }),
                Btn("Criar bucket", true, () => Navigate("s3-create", null)));
            var rows = Sim.Buckets.Select(b => new[] { b.Name, AwsSim.RegionName(b.Region) + " " + b.Region, b.BlockPublic ? "● Bucket e objetos não públicos" : "● Público", b.Created.ToString("dd/MM/yyyy HH:mm") }).ToList();
            lvRef[0] = pg.Table(new[] { "Nome", "Região da AWS", "Acesso", "Data de criação" }, new[] { 3f, 3f, 2.5f, 1.8f }, rows, i => Navigate("s3-bucket", Sim.Buckets[i].Name));
        }

        void PageS3Create(Flow pg)
        {
            PageTitle = "Criar bucket - S3";
            pg.Crumbs("Amazon S3", "Buckets", "Criar bucket");
            pg.Header("Criar bucket", "Os buckets são contêineres de dados armazenados no S3.");
            Panel box;
            var f = pg.Section("Configuração geral", out box);
            var name = f.Field("Nome do bucket", "Exclusivo no mundo. Só letras minúsculas, números, pontos e hífens.");
            var reg = f.Select("Região da AWS", AwsSim.Regions.Select(r => r[1] + " " + r[0]).ToArray(), Array.FindIndex(AwsSim.Regions, r => r[0] == Sim.Region));
            pg.EndSection(box, f);
            var g = pg.Section("Configurações de bloqueio do acesso público deste bucket", out box);
            g.Text("O acesso público é concedido por políticas e listas de controle de acesso. Recomendamos bloquear todo o acesso público.", fSmall, Muted);
            var block = g.Check("Bloquear todo o acesso público", true, "Ativar essa configuração é o mesmo que ativar as quatro configurações de bloqueio abaixo dela.");
            pg.EndSection(box, g);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("s3", null)), Btn("Criar bucket", true, () =>
            {
                if (!block.Checked && MessageBox.Show(FindForm(), "Desativar o bloqueio pode deixar os objetos públicos na internet. Continuar?", "Aviso", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                string r = AwsSim.Regions[Math.Max(0, reg.SelectedIndex)][0];
                var e = Sim.CreateBucket(name.Text, r, block.Checked);
                if (e != null) { err.Text = "✖ " + e; return; }
                string n = name.Text.Trim();
                Navigate("s3", null, false);
                if (Sim.Buckets.Any(b => b.Name == n)) Flash("Bucket \"" + n + "\" criado com sucesso.", OkGreen);
            }));
        }

        void PageBucket(Flow pg)
        {
            var b = Sim.Buckets.FirstOrDefault(x => x.Name == arg);
            if (b == null) { Navigate("s3", null, false); return; }
            PageTitle = b.Name + " - S3";
            pg.Crumbs("Amazon S3", "Buckets", b.Name);
            string bn = b.Name;
            pg.Header(b.Name, AwsSim.RegionName(b.Region) + " · " + (b.BlockPublic ? "Acesso público bloqueado" : "Objetos podem ser públicos"));
            pg.Tabs(new[] { "Objetos", "Propriedades", "Permissões", "Gerenciamento" }, s3Tab, tb => { s3Tab = tb; Navigate("s3-bucket", bn, false); });
            Panel box;
            if (s3Tab == "Propriedades")
            {
                var o = pg.Section("Visão geral do bucket", out box);
                o.Text("Região da AWS: " + AwsSim.RegionName(b.Region) + " " + b.Region, fUi, Ink);
                o.Text("ARN: arn:aws:s3:::" + b.Name + "   ·   Criado em " + b.Created.ToString("dd/MM/yyyy"), fSmall, Muted);
                pg.EndSection(box, o);
                var f = pg.Section("Versionamento de bucket", out box);
                f.Text("Versionamento de bucket: " + b.Versioning, fBold, b.Versioning == "Ativado" ? OkGreen : Ink);
                f.Text("Guarda versões anteriores de cada objeto: dá para recuperar um arquivo sobrescrito ou excluído por engano.", fSmall, Muted);
                f.Buttons(Btn("Editar versionamento", false, () => Navigate("s3-versioning-edit", bn)));
                pg.EndSection(box, f);
                var tg = pg.Section("Tags (" + b.Tags.Count + ")", out box);
                tg.Text("Pares chave/valor para organizar e rastrear custos. A chave diferencia maiúsculas de minúsculas.", fSmall, Muted);
                if (b.Tags.Count == 0) tg.Text("Nenhuma tag.", fUi, Muted);
                else tg.Table(new[] { "Chave", "Valor" }, new[] { 1f, 1f }, b.Tags.Select(kv => new[] { kv.Key, kv.Value }).ToList(), null);
                tg.Buttons(Btn("Editar tags", false, () => Navigate("s3-tags-edit", bn)));
                pg.EndSection(box, tg);
                var c = pg.Section("Criptografia padrão", out box);
                c.Text("Criptografia padrão: " + (b.Encryption == "SSE-KMS" ? "SSE-KMS (chave do AWS KMS: " + b.KmsKey + ")" : "SSE-S3 (chave gerenciada pelo Amazon S3)"), fBold, Ink);
                c.Text("Todo objeto novo é criptografado em repouso com esta configuração.", fSmall, Muted);
                c.Buttons(Btn("Editar criptografia", false, () => Navigate("s3-encryption-edit", bn)));
                pg.EndSection(box, c);
                var w = pg.Section("Hospedagem de site estático", out box);
                if (b.IndexDoc == null) w.Text("Hospedagem de site estático: Desativada", fBold, Ink);
                else
                {
                    w.Text("Hospedagem de site estático: Ativada · documento de índice " + b.IndexDoc + (b.ErrorDoc != null ? " · documento de erro " + b.ErrorDoc : ""), fBold, OkGreen);
                    w.Text("Endpoint do site do bucket: " + AwsSim.WebsiteUrl(b), fSmall, Muted);
                }
                w.Text("Para o endpoint do S3 abrir o site, os objetos precisam ser públicos. Em produção, prefira a CloudFront na frente de um bucket privado.", fSmall, Muted);
                w.Buttons(Btn("Editar", false, () => Navigate("s3-website-edit", bn)));
                pg.EndSection(box, w);
                return;
            }
            if (s3Tab == "Permissões")
            {
                var f = pg.Section("Bloquear acesso público (configurações do bucket)", out box);
                f.Text("Bloquear todo o acesso público: " + (b.BlockPublic ? "● Ativado" : "● Desativado"), fBold, b.BlockPublic ? OkGreen : ErrRed);
                f.Text("Quando ativado, bloqueia ACLs e políticas que dariam acesso público aos objetos — mesmo que alguém tente liberar por engano.", fSmall, Muted);
                f.Buttons(Btn("Editar", false, () => Navigate("s3-bpa-edit", bn)));
                pg.EndSection(box, f);
                var p = pg.Section("Política do bucket", out box);
                p.Text("Documento JSON que permite ou nega ações neste bucket (ex.: exigir HTTPS, liberar só a CloudFront).", fSmall, Muted);
                if (b.Policy == null) p.Text("Nenhuma política.", fUi, Muted);
                else
                {
                    var tb = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Font = fMono, Location = new Point(p.X, p.Y), Width = p.W, Height = S(150), Text = AwsSim.PrettyJson(b.PolicyText).Replace("\n", "\r\n"), BackColor = Art.Hx(0xF8F8F8) };
                    box.Controls.Add(tb);
                    p.Y += tb.Height + S(10);
                }
                var pb = new List<Button>();
                if (b.Policy != null) pb.Add(Btn("Excluir", false, () =>
                {
                    if (MessageBox.Show(FindForm(), "Excluir a política do bucket " + bn + "?", "Excluir política", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                    var er = Sim.SetPolicy(bn, null);
                    if (er != null) Flash("✖ " + er, ErrRed); else Flash("Política do bucket excluída.", OkGreen);
                }));
                pb.Add(Btn("Editar", false, () => Navigate("s3-policy-edit", bn)));
                p.Buttons(pb.ToArray());
                pg.EndSection(box, p);
                return;
            }
            if (s3Tab == "Gerenciamento")
            {
                var rules = b.Lifecycle.ToList();
                var lvR = new ListView[1];
                pg.SubHeader("Regras de ciclo de vida (" + rules.Count + ")",
                    Btn("Excluir regra", false, () =>
                    {
                        var lv = lvR[0];
                        if (lv == null || lv.SelectedIndices.Count == 0) { Flash("Selecione uma regra primeiro.", InfoBlue); return; }
                        var r = rules[lv.SelectedIndices[0]];
                        var er = Sim.DeleteLifecycleRule(bn, r.Id);
                        if (er != null) Flash("✖ " + er, ErrRed); else Flash("Regra " + r.Id + " excluída.", OkGreen);
                    }),
                    Btn("Criar regra de ciclo de vida", true, () => Navigate("s3-lifecycle-create", bn)));
                pg.Text("Regras movem objetos para classes mais baratas ou apagam objetos e versões antigas automaticamente.", fSmall, Muted);
                lvR[0] = pg.Table(new[] { "Nome da regra", "Escopo", "Ações" }, new[] { 1.4f, 1.4f, 3.4f }, rules.Select(r => new[] { r.Id, r.Prefix.Length == 0 ? "Bucket inteiro" : "Prefixo " + r.Prefix, LcText(r) }).ToList(), null);
                return;
            }
            // Objetos
            var lvRef = new ListView[1];
            var objs = b.Objects.ToList();
            var menu = new ContextMenuStrip { Font = fUi };
            menu.Items.Add("Compartilhar com URL pré-assinada", null, (s, e) =>
            {
                var lv = lvRef[0];
                if (lv == null || lv.SelectedIndices.Count == 0) { Flash("Selecione um objeto primeiro.", InfoBlue); return; }
                Navigate("s3-presign-create", bn + "|" + objs[lv.SelectedIndices[0]]);
            });
            Button actBtn = null;
            actBtn = Btn("Ações  ▾", false, () => ShowMenu(menu, actBtn));
            pg.SubHeader("Objetos (" + objs.Count + ")",
                Btn("Mostrar versões", false, () => Navigate("s3-versions", bn)),
                actBtn,
                Btn("Excluir", false, () =>
                {
                    var lv = lvRef[0];
                    if (lv == null || lv.SelectedIndices.Count == 0) { Flash("Selecione um objeto primeiro.", InfoBlue); return; }
                    string key = objs[lv.SelectedIndices[0]];
                    Sim.DeleteObject(bn, key);
                    Flash("Objeto " + key + " excluído.", OkGreen);
                }),
                Btn("Carregar", true, () => Navigate("s3-upload", bn)));
            var rows = objs.Select(o => new[] { o, System.IO.Path.GetExtension(o).TrimStart('.'), (1 + (o.Length * 37 % 90)) + " KB", DateTime.Now.ToString("dd/MM/yyyy HH:mm") }).ToList();
            lvRef[0] = pg.Table(new[] { "Nome", "Tipo", "Tamanho", "Última modificação" }, new[] { 3f, 1f, 1f, 2f }, rows, null);
            if (b.Versioning == "Ativado" && b.Markers.Any(m => !b.Objects.Contains(m.Key))) pg.Text("Há objetos excluídos com versões anteriores guardadas: use 'Mostrar versões'.", fSmall, Muted);
        }

        static string LcText(AwsSim.LcRule r)
        {
            var a = new List<string>();
            if (r.Glacier > 0) a.Add("mover para o Glacier após " + r.Glacier + " dia(s)");
            if (r.Expire > 0) a.Add("expirar versões atuais após " + r.Expire + " dia(s)");
            if (r.Noncurrent > 0) a.Add("excluir versões desatualizadas após " + r.Noncurrent + " dia(s)");
            return string.Join("; ", a);
        }

        void PageVersioning(Flow pg)
        {
            var b = Sim.Buckets.FirstOrDefault(x => x.Name == arg);
            if (b == null) { Navigate("s3", null, false); return; }
            PageTitle = "Editar versionamento - S3";
            pg.Crumbs("Amazon S3", "Buckets", b.Name, "Editar versionamento de bucket");
            pg.Header("Editar versionamento de bucket", "Status atual: " + b.Versioning);
            Panel box;
            var f = pg.Section("Versionamento de bucket", out box);
            f.Text("Com o versionamento, cada alteração cria uma nova versão do objeto e uma exclusão vira um 'marcador de exclusão': as versões antigas continuam recuperáveis (e cobradas).", fSmall, Muted);
            var opt = f.Radios(null, new[] { "Suspender", "Ativar" }, b.Versioning == "Ativado" ? 1 : 0);
            pg.EndSection(box, f);
            var err = pg.Error();
            string bn = b.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("s3-bucket", bn)), Btn("Salvar alterações", true, () =>
            {
                var e = Sim.SetVersioning(bn, opt[1].Checked);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("s3-bucket", bn, false);
                Flash("Versionamento de " + bn + ": " + Sim.Buckets.First(x => x.Name == bn).Versioning + ".", OkGreen);
            }));
        }

        void PageEncryption(Flow pg)
        {
            var b = Sim.Buckets.FirstOrDefault(x => x.Name == arg);
            if (b == null) { Navigate("s3", null, false); return; }
            PageTitle = "Editar criptografia - S3";
            pg.Crumbs("Amazon S3", "Buckets", b.Name, "Editar criptografia padrão");
            pg.Header("Editar criptografia padrão", null);
            Panel box;
            var f = pg.Section("Criptografia padrão", out box);
            var type = f.Radios("Tipo de criptografia", new[] { "SSE-S3: criptografia com chaves gerenciadas pelo Amazon S3", "SSE-KMS: criptografia com chaves do AWS Key Management Service (KMS)" }, b.Encryption == "SSE-KMS" ? 1 : 0);
            f.Text("Com SSE-KMS e uma chave gerenciada pelo cliente, vocês controlam quem pode usar a chave (política da chave), a rotação, e cada uso fica registrado no CloudTrail.", fSmall, Muted);
            var keys = f.Select("Chave do AWS KMS (só para SSE-KMS)", AwsSim.KmsKeys.Select(k => k[0] + "  —  " + k[1]).ToArray(), b.KmsKey == null ? -1 : Array.FindIndex(AwsSim.KmsKeys, k => k[0] == b.KmsKey));
            pg.EndSection(box, f);
            var err = pg.Error();
            string bn = b.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("s3-bucket", bn)), Btn("Salvar alterações", true, () =>
            {
                bool kms = type[1].Checked;
                if (kms && keys.SelectedIndex < 0) { err.Text = "✖ Escolha a chave do KMS."; return; }
                var e = Sim.SetEncryption(bn, kms, kms ? AwsSim.KmsKeys[keys.SelectedIndex][0] : null);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("s3-bucket", bn, false);
                Flash("Criptografia padrão de " + bn + " atualizada.", OkGreen);
            }));
        }

        void PageUpload(Flow pg)
        {
            PageTitle = "Carregar - S3";
            pg.Crumbs("Amazon S3", "Buckets", arg, "Carregar");
            pg.Header("Carregar", "Escolha os arquivos do seu computador para enviar ao bucket " + arg + ".");
            Panel box;
            var f = pg.Section("Arquivos e pastas (computador local: /home/estagio)", out box);
            var checks = new List<CheckBox>();
            foreach (var file in AwsSim.LocalFiles) checks.Add(f.Check(file, false));
            var folder = f.Check("📁 Adicionar a pasta site/ inteira (index.html, style.css, app.js, logo.png)", false);
            pg.EndSection(box, f);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("s3-bucket", arg)), Btn("Carregar", true, () =>
            {
                var files = new List<string>();
                for (int i = 0; i < checks.Count; i++) if (checks[i].Checked) files.Add(AwsSim.LocalFiles[i]);
                if (folder.Checked) foreach (var sf in AwsSim.SiteFiles) if (!files.Contains(sf)) files.Add(sf);
                if (files.Count == 0) { err.Text = "✖ Selecione pelo menos um arquivo."; return; }
                string bucket = arg;
                var e = Sim.Upload(bucket, files);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("s3-bucket", bucket, false);
                Flash("Carregamento concluído: " + files.Count + " arquivo(s) enviado(s) para " + bucket + ".", OkGreen);
            }));
        }

        // ── EC2
        void PageEc2(Flow pg)
        {
            PageTitle = "Instâncias - EC2";
            pg.Crumbs("EC2", "Instâncias");
            var list = Sim.Instances.Where(i => i.Region == Sim.Region).ToList();
            var lvRef = new ListView[1];
            var stateMenu = new ContextMenuStrip { Font = fUi };
            Action<string, string> act = (key, label) =>
            {
                var lv = lvRef[0];
                if (lv == null || lv.SelectedIndices.Count == 0) { Flash("Selecione uma instância na lista primeiro.", InfoBlue); return; }
                var inst = list[lv.SelectedIndices[0]];
                if (key == "terminate" && MessageBox.Show(FindForm(), "Encerrar APAGA a instância " + inst.Name + " (" + inst.Id + ") e o disco dela. Continuar?", "Encerrar instância", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                var er = Sim.SetState(inst.Id, key);
                if (er != null) { Flash("✖ " + er, ErrRed); return; }
                Flash(label + ": " + inst.Name + " (" + inst.Id + ").", key == "terminate" ? ErrRed : OkGreen);
            };
            stateMenu.Items.Add("Interromper instância", null, (s, e) => act("stop", "Instância interrompida"));
            stateMenu.Items.Add("Iniciar instância", null, (s, e) => act("start", "Instância iniciada"));
            stateMenu.Items.Add("Reinicializar instância", null, (s, e) => act("reboot", "Instância reinicializada"));
            stateMenu.Items.Add(new ToolStripSeparator());
            stateMenu.Items.Add("Encerrar (excluir) instância", null, (s, e) => act("terminate", "Instância encerrada"));
            Button stateBtn = null;
            stateBtn = Btn("Estado da instância  ▾", false, () => ShowMenu(stateMenu, stateBtn));
            var actMenu = new ContextMenuStrip { Font = fUi };
            Action<string> open = target =>
            {
                var lv = lvRef[0];
                if (lv == null || lv.SelectedIndices.Count == 0) { Flash("Selecione uma instância na lista primeiro.", InfoBlue); return; }
                Navigate(target, list[lv.SelectedIndices[0]].Id);
            };
            actMenu.Items.Add("Gerenciar tags da instância", null, (s, e) => open("ec2-tags-edit"));
            actMenu.Items.Add("Configurações da instância → Alterar tipo de instância", null, (s, e) => open("ec2-type-edit"));
            actMenu.Items.Add("Configurações da instância → Alterar proteção contra encerramento", null, (s, e) => open("ec2-protect-edit"));
            actMenu.Items.Add("Configurações da instância → Modificar opções de metadados da instância", null, (s, e) => open("ec2-imds-edit"));
            actMenu.Items.Add("Segurança → Modificar função do IAM", null, (s, e) => open("ec2-role-edit"));
            actMenu.Items.Add("Imagem e modelos → Criar imagem", null, (s, e) => open("ec2-image-create"));
            Button actBtn = null;
            actBtn = Btn("Ações  ▾", false, () => ShowMenu(actMenu, actBtn));
            pg.Header("Instâncias (" + list.Count + ")", "Região: " + AwsSim.RegionName(Sim.Region) + ". Instâncias de outras regiões não aparecem aqui.",
                actBtn, stateBtn, Btn("Executar instâncias", true, () => Navigate("ec2-launch", null)));
            var rows = list.Select(i => new[] { i.Name, i.Id, "● " + i.State, i.Type, i.Region + "a", i.Ip, i.Role ?? "-" }).ToList();
            lvRef[0] = pg.Table(new[] { "Nome", "ID da instância", "Estado", "Tipo", "Zona", "IP público", "Função do IAM" }, new[] { 1.8f, 1.8f, 1.6f, 1.2f, 1.2f, 1.5f, 1.5f }, rows, null);
            pg.Text("Dica: selecione a linha da instância e use 'Estado da instância' (ligar/desligar) ou 'Ações' (tags, tipo, proteção, metadados, função do IAM, imagem).", fSmall, Muted);
        }

        void PageEc2Tags(Flow pg)
        {
            var inst = Sim.Inst(arg);
            if (inst == null) { Navigate("ec2", null, false); return; }
            PageTitle = "Gerenciar tags - EC2";
            pg.Crumbs("EC2", "Instâncias", inst.Id, "Gerenciar tags");
            pg.Header("Gerenciar tags", inst.Name + " (" + inst.Id + ") · " + AwsSim.RegionName(inst.Region));
            Panel box;
            var f = pg.Section("Tags atuais", out box);
            var rows = new List<string[]> { new[] { "Name", inst.Name } };
            rows.AddRange(inst.Tags.Select(kv => new[] { kv.Key, kv.Value }));
            f.Table(new[] { "Chave", "Valor" }, new[] { 1f, 1f }, rows, null);
            pg.EndSection(box, f);
            var g = pg.Section("Adicionar nova tag", out box);
            g.Text("Tags são pares chave/valor. A chave diferencia maiúsculas de minúsculas: 'Projeto' e 'projeto' são tags diferentes.", fSmall, Muted);
            var key = g.Field("Chave", null);
            var val = g.Field("Valor (opcional)", null);
            pg.EndSection(box, g);
            var err = pg.Error();
            string id = inst.Id;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ec2", null)), Btn("Salvar", true, () =>
            {
                var e = Sim.AddTag(id, key.Text, val.Text);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("ec2", null, false);
                Flash("Tag " + key.Text.Trim() + " salva em " + id + ".", OkGreen);
            }));
        }

        void PageEc2Type(Flow pg)
        {
            var inst = Sim.Inst(arg);
            if (inst == null) { Navigate("ec2", null, false); return; }
            PageTitle = "Alterar tipo de instância - EC2";
            pg.Crumbs("EC2", "Instâncias", inst.Id, "Alterar tipo de instância");
            pg.Header("Alterar tipo de instância", inst.Name + " (" + inst.Id + ") · estado: " + inst.State);
            Panel box;
            var f = pg.Section("Configuração do tipo de instância", out box);
            f.Text("Tipo atual: " + inst.Type, fUi, Ink);
            f.Text("A instância precisa estar INTERROMPIDA para trocar o tipo. O disco (EBS) e os dados são mantidos.", fSmall, inst.State == "Interrompida" ? Muted : ErrRed);
            var types = AwsSim.InstanceTypes.Select(t => t[0] + "   · " + t[1]).ToArray();
            var sel = f.Select("Novo tipo de instância", types, Array.FindIndex(AwsSim.InstanceTypes, t => t[0] == inst.Type));
            pg.EndSection(box, f);
            var err = pg.Error();
            string id = inst.Id;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ec2", null)), Btn("Aplicar", true, () =>
            {
                if (sel.SelectedIndex < 0) { err.Text = "✖ Escolha o tipo."; return; }
                var e = Sim.ModifyType(id, AwsSim.InstanceTypes[sel.SelectedIndex][0]);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("ec2", null, false);
                Flash("Tipo de " + id + " alterado para " + AwsSim.InstanceTypes[sel.SelectedIndex][0] + ".", OkGreen);
            }));
        }

        void PageVolumes(Flow pg)
        {
            PageTitle = "Volumes - EC2";
            pg.Crumbs("EC2", "Elastic Block Store", "Volumes");
            var list = Sim.Volumes.Where(v => v.Region == Sim.Region).ToList();
            var lvRef = new ListView[1];
            var menu = new ContextMenuStrip { Font = fUi };
            Func<AwsSim.Volume> selected = () =>
            {
                var lv = lvRef[0];
                if (lv == null || lv.SelectedIndices.Count == 0) { Flash("Selecione um volume na lista primeiro.", InfoBlue); return null; }
                return list[lv.SelectedIndices[0]];
            };
            menu.Items.Add("Criar snapshot", null, (s, e) => { var v = selected(); if (v != null) Navigate("ec2-snap-create", v.Id); });
            menu.Items.Add("Modificar volume", null, (s, e) => { var v = selected(); if (v != null) Navigate("ec2-vol-edit", v.Id); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Excluir volume", null, (s, e) =>
            {
                var v = selected();
                if (v == null) return;
                if (MessageBox.Show(FindForm(), "Excluir o volume " + v.Id + " (" + v.Size + " GiB)? Os dados serão perdidos para sempre, a menos que exista um snapshot.", "Excluir volume", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                var er = Sim.DeleteVolume(v.Id);
                if (er != null) Flash("✖ " + er, ErrRed); else Flash("Volume " + v.Id + " excluído.", OkGreen);
            });
            Button actBtn = null;
            actBtn = Btn("Ações  ▾", false, () => ShowMenu(menu, actBtn));
            pg.Header("Volumes (" + list.Count + ")", "Discos (EBS) das instâncias. Volume 'Disponível' não está anexado a nada, mas continua sendo cobrado por GiB.", actBtn);
            var rows = list.Select(v =>
            {
                var i = v.Instance != null ? Sim.Inst(v.Instance) : null;
                return new[] { v.Id, v.Size + " GiB", v.Type, "● " + (v.Instance != null ? "Em uso" : "Disponível"), i != null ? i.Name + " (" + i.Id + ")" : "-" };
            }).ToList();
            lvRef[0] = pg.Table(new[] { "ID do volume", "Tamanho", "Tipo", "Estado", "Anexado a" }, new[] { 1.6f, 1f, 0.8f, 1.2f, 2f }, rows, null);
            pg.Text("Selecione o volume e use 'Ações'.", fSmall, Muted);
        }

        void PageSnapCreate(Flow pg)
        {
            var v = Sim.Vol(arg);
            if (v == null) { Navigate("ec2-volumes", null, false); return; }
            PageTitle = "Criar snapshot - EC2";
            pg.Crumbs("EC2", "Volumes", v.Id, "Criar snapshot");
            pg.Header("Criar snapshot", "Cópia de segurança (backup) do volume, guardada no S3 pela AWS.");
            Panel box;
            var f = pg.Section("Detalhes", out box);
            var inst = v.Instance != null ? Sim.Inst(v.Instance) : null;
            f.Text("Volume de origem: " + v.Id + " · " + v.Size + " GiB · " + (inst != null ? "anexado a " + inst.Name : "não anexado"), fUi, Ink);
            var desc = f.Field("Descrição", "Até 255 caracteres. Ajuda a saber depois do que é o backup.");
            pg.EndSection(box, f);
            var err = pg.Error();
            string vid = v.Id;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ec2-volumes", null)), Btn("Criar snapshot", true, () =>
            {
                var e = Sim.CreateSnapshot(vid, desc.Text);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("ec2-snapshots", null, false);
                Flash("Snapshot do volume " + vid + " criado.", OkGreen);
            }));
        }

        void PageSnapshots(Flow pg)
        {
            PageTitle = "Snapshots - EC2";
            pg.Crumbs("EC2", "Elastic Block Store", "Snapshots");
            var list = Sim.Snapshots.Where(x => x.Region == Sim.Region).ToList();
            pg.Header("Snapshots (" + list.Count + ")", "Para criar: Volumes → selecione o volume → Ações → Criar snapshot.");
            var rows = list.Select(x => new[] { x.Id, x.Volume, x.Description.Length == 0 ? "-" : x.Description, x.Size + " GiB", "● Concluído", x.Created.ToString("dd/MM/yyyy HH:mm") }).ToList();
            pg.Table(new[] { "ID do snapshot", "Volume", "Descrição", "Tamanho", "Estado", "Criado em" }, new[] { 1.5f, 1.4f, 2f, 0.9f, 1.1f, 1.4f }, rows, null);
        }

        void PageSgs(Flow pg)
        {
            PageTitle = "Grupos de segurança - EC2";
            pg.Crumbs("EC2", "Rede e segurança", "Grupos de segurança");
            var list = Sim.SecGroups.Where(x => x.Region == Sim.Region).ToList();
            pg.Header("Grupos de segurança (" + list.Count + ")", "Um grupo de segurança é um firewall virtual: as regras de entrada dizem quem pode acessar quais portas. Tudo que não está liberado é bloqueado.",
                Btn("Criar grupo de segurança", true, () => Navigate("ec2-sg-create", null)));
            var rows = list.Select(x => new[] { x.Name, x.Id, x.Description, x.Inbound.Count + " regra(s)" }).ToList();
            pg.Table(new[] { "Nome", "ID do grupo", "Descrição", "Regras de entrada" }, new[] { 1.2f, 1.3f, 2.6f, 1.1f }, rows, i => Navigate("ec2-sg-rules", list[i].Id));
            pg.Text("Clique no nome do grupo para ver e editar as regras de entrada.", fSmall, Muted);
        }

        void PageSgRules(Flow pg)
        {
            var sg = Sim.Sg(arg);
            if (sg == null) { Navigate("ec2-sg", null, false); return; }
            PageTitle = sg.Name + " - EC2";
            pg.Crumbs("EC2", "Grupos de segurança", sg.Id);
            var lvRef = new ListView[1];
            var rules = sg.Inbound.ToList();
            string gid = sg.Id;
            pg.Header(sg.Name + " (" + sg.Id + ")", sg.Description + " · " + AwsSim.RegionName(sg.Region),
                Btn("Excluir regra", false, () =>
                {
                    var lv = lvRef[0];
                    if (lv == null || lv.SelectedIndices.Count == 0) { Flash("Selecione uma regra na lista primeiro.", InfoBlue); return; }
                    var r = rules[lv.SelectedIndices[0]];
                    var e = Sim.Revoke(gid, r.Port, r.Source);
                    if (e != null) Flash("✖ " + e, ErrRed); else Flash("Regra de entrada removida: porta " + r.Port + " de " + r.Source + ".", OkGreen);
                }));
            var users = Sim.Instances.Where(i => i.Sg == sg.Id && i.State != "Encerrada").Select(i => i.Name).ToList();
            pg.Text("Instâncias que usam este grupo: " + (users.Count == 0 ? "nenhuma" : string.Join(", ", users)), fSmall, Muted);
            pg.Text("Regras de entrada (" + rules.Count + ")", fH2, Ink);
            var rows = rules.Select(r =>
            {
                var src = r.Source.StartsWith("sg-") && Sim.Sg(r.Source) != null ? r.Source + " (" + Sim.Sg(r.Source).Name + ")" : r.Source;
                return new[] { AwsSim.RuleName(r.Port), "TCP", r.Port.ToString(), src };
            }).ToList();
            lvRef[0] = pg.Table(new[] { "Tipo", "Protocolo", "Porta", "Origem" }, new[] { 1.4f, 0.8f, 0.8f, 2.4f }, rows, null);
            if (rules.Any(r => r.Source == "0.0.0.0/0" && r.Port != 80 && r.Port != 443))
                pg.Text("⚠ Há porta administrativa ou de banco de dados aberta para a internet inteira (0.0.0.0/0).", fBold, ErrRed);
            Panel box;
            var f = pg.Section("Adicionar regra de entrada", out box);
            var type = f.Select("Tipo", AwsSim.RuleTypes.Select(r => r[0] + "  ·  TCP " + r[1]).ToArray(), -1);
            var src2 = f.Field("Origem", "Bloco CIDR (ex.: 203.0.113.25/32 = um único IP; 0.0.0.0/0 = qualquer lugar da internet) ou ID de outro grupo de segurança (sg-...).");
            var err = f.Error();
            f.Buttons(Btn("Adicionar regra", true, () =>
            {
                if (type.SelectedIndex < 0) { err.Text = "✖ Escolha o tipo da regra."; return; }
                int port = int.Parse(AwsSim.RuleTypes[type.SelectedIndex][1]);
                string origem = src2.Text.Trim();
                var e = Sim.Authorize(gid, port, origem);
                if (e != null) { err.Text = "✖ " + e; return; }
                Flash("Regra de entrada adicionada: " + AwsSim.RuleTypes[Array.FindIndex(AwsSim.RuleTypes, r => r[1] == port.ToString())][0] + " (porta " + port + ") de " + origem + ".", OkGreen);
            }));
            pg.EndSection(box, f);
        }

        void PageLaunch(Flow pg)
        {
            PageTitle = "Executar instância - EC2";
            pg.Crumbs("EC2", "Instâncias", "Executar uma instância");
            pg.Header("Executar uma instância", "Região: " + AwsSim.RegionName(Sim.Region) + " (" + Sim.Region + ")");
            Panel box;
            var f = pg.Section("Nome e tags", out box);
            var name = f.Field("Nome", null);
            pg.EndSection(box, f);
            var own = Sim.Images.Where(x => x.Region == Sim.Region).ToList();
            var amis = AwsSim.BaseAmis.Select(x => x[1]).Concat(own.Select(x => x.Name)).ToArray();
            var amiText = AwsSim.BaseAmis.Select(x => x[1] + "   (" + x[0] + ", da AWS)").Concat(own.Select(x => x.Name + "   (" + x.Id + ", Minhas AMIs)")).ToArray();
            var g = pg.Section("Imagens de aplicação e sistema operacional (AMI)", out box);
            var ami = g.Radios(null, amiText, 0);
            pg.EndSection(box, g);
            var types = AwsSim.InstanceTypes.Select(x => x[0].PadRight(10) + " · " + x[1]).ToArray();
            var h = pg.Section("Tipo de instância", out box);
            var type = h.Select("Tipo", types, 1);
            pg.EndSection(box, h);
            var keys = Sim.KeyPairs.Where(x => x.Region == Sim.Region).Select(x => x.Name).ToList();
            var k = pg.Section("Par de chaves (login)", out box);
            var keySel = k.Select("Par de chaves", keys.Concat(new[] { "Prosseguir sem um par de chaves (não recomendado)" }).ToArray(), 0);
            pg.EndSection(box, k);
            var sgs = Sim.SecGroups.Where(x => x.Region == Sim.Region).ToList();
            var n = pg.Section("Configurações de rede: firewall (grupos de segurança)", out box);
            var sgSel = n.Select("Selecionar grupo de segurança existente", sgs.Select(x => x.Name + "  (" + x.Id + ")").ToArray(), sgs.FindIndex(x => x.Name == "default"));
            pg.EndSection(box, n);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ec2", null)), Btn("Executar instância", true, () =>
            {
                string a = amis[Math.Max(0, ami.FindIndex(r => r.Checked))];
                string t = types[Math.Max(0, type.SelectedIndex)].Split(' ')[0];
                string kp = keySel.SelectedIndex >= 0 && keySel.SelectedIndex < keys.Count ? keys[keySel.SelectedIndex] : "";
                string sg = sgSel.SelectedIndex >= 0 ? sgs[sgSel.SelectedIndex].Id : null;
                var e = Sim.Launch(name.Text, a, t, kp, sg);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("ec2", null, false);
                Flash("Instância \"" + name.Text.Trim() + "\" iniciada com sucesso.", OkGreen);
            }));
        }

        // ── IAM
        void PageIam(Flow pg)
        {
            PageTitle = "Usuários - IAM";
            pg.Crumbs("IAM", "Usuários");
            var lvRef = new ListView[1];
            pg.Header("Usuários (" + Sim.Users.Count + ")", "Um usuário IAM é uma identidade com credenciais de longo prazo usada para interagir com a AWS.",
                Btn("Excluir", false, () =>
                {
                    var lv = lvRef[0];
                    if (lv == null || lv.SelectedIndices.Count == 0) { Flash("Selecione um usuário primeiro.", InfoBlue); return; }
                    var u = Sim.Users[lv.SelectedIndices[0]];
                    if (MessageBox.Show(FindForm(), "Excluir o usuário " + u.Name + "?", "Excluir usuário", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                    Sim.DeleteUser(u.Name);
                    Flash("Usuário " + u.Name + " excluído.", OkGreen);
                }),
                Btn("Criar usuário", true, () => Navigate("iam-create", null)));
            var rows = Sim.Users.Select(u => new[] { u.Name, u.Group ?? "Nenhum", u.Mfa ? "● Ativado" : "● Desativado", u.Created.ToString("dd/MM/yyyy") }).ToList();
            lvRef[0] = pg.Table(new[] { "Nome do usuário", "Grupos", "MFA", "Criado em" }, new[] { 2f, 2f, 1.5f, 1.5f }, rows, i => Navigate("iam-user", Sim.Users[i].Name));
        }

        void PageIamGroups(Flow pg)
        {
            PageTitle = "Grupos - IAM";
            pg.Crumbs("IAM", "Grupos de usuários");
            pg.Header("Grupos de usuários (3)", "Grupos aplicam as mesmas permissões a vários usuários.");
            var pol = new Dictionary<string, string> { { "Administradores", "AdministratorAccess (acesso total)" }, { "Desenvolvedores", "PowerUserAccess (tudo, menos IAM)" }, { "Leitura", "ReadOnlyAccess (somente leitura)" } };
            var rows = AwsSim.Groups.Select(gp => new[] { gp, Sim.Users.Count(u => u.Group == gp).ToString(), pol[gp] }).ToList();
            pg.Table(new[] { "Nome do grupo", "Usuários", "Política de permissões" }, new[] { 2f, 1f, 3f }, rows, null);
        }

        void PageIamCreate(Flow pg)
        {
            PageTitle = "Criar usuário - IAM";
            pg.Crumbs("IAM", "Usuários", "Criar usuário");
            pg.Header("Criar usuário", null);
            Panel box;
            var f = pg.Section("Detalhes do usuário", out box);
            var name = f.Field("Nome do usuário", "Até 64 caracteres: letras, números e + = , . @ _ -");
            pg.EndSection(box, f);
            var g = pg.Section("Definir permissões: adicionar usuário a um grupo", out box);
            var opts = new[] { "Administradores  —  AdministratorAccess (acesso total)", "Desenvolvedores  —  PowerUserAccess", "Leitura  —  ReadOnlyAccess (somente leitura)", "Nenhum grupo agora" };
            var rad = g.Radios(null, opts, 3);
            pg.EndSection(box, g);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("iam", null)), Btn("Criar usuário", true, () =>
            {
                int gi = rad.FindIndex(r => r.Checked);
                string group = gi >= 0 && gi < 3 ? AwsSim.Groups[gi] : null;
                var e = Sim.CreateUser(name.Text, group);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("iam", null, false);
                Flash("Usuário " + name.Text.Trim() + " criado com sucesso.", OkGreen);
            }));
        }

        void PageIamUser(Flow pg)
        {
            var u = Sim.Users.FirstOrDefault(x => x.Name == arg);
            if (u == null) { Navigate("iam", null, false); return; }
            PageTitle = u.Name + " - IAM";
            pg.Crumbs("IAM", "Usuários", u.Name);
            pg.Header(u.Name, "ARN: arn:aws:iam::123456789012:user/" + u.Name);
            Panel box;
            var f = pg.Section("Permissões", out box);
            f.Text("Grupo: " + (u.Group ?? "nenhum"), fUi, Ink);
            if (u.Group != null)
            {
                string gname = u.Group, un = u.Name;
                f.Buttons(Btn("Remover do grupo", false, () =>
                {
                    if (MessageBox.Show(FindForm(), "Remover " + un + " do grupo " + gname + "?", "Remover do grupo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                    var er = Sim.RemoveFromGroup(un, gname);
                    if (er != null) Flash("✖ " + er, ErrRed); else Flash(un + " removido do grupo " + gname + ".", OkGreen);
                }));
            }
            if (u.Group == null)
            {
                var sel = f.Select("Adicionar a um grupo", AwsSim.Groups, -1);
                var holder = f;
                f.Buttons(Btn("Adicionar ao grupo", true, () =>
                {
                    if (sel.SelectedIndex < 0) { Flash("Escolha um grupo.", InfoBlue); return; }
                    Sim.AddToGroup(u.Name, AwsSim.Groups[sel.SelectedIndex]);
                    Navigate("iam-user", u.Name, false);
                    Flash("Usuário adicionado ao grupo " + AwsSim.Groups[sel.SelectedIndex] + ".", OkGreen);
                }));
            }
            string uname = u.Name;
            f.Text("Políticas anexadas diretamente (" + u.Policies.Count + ")", fBold, Ink);
            var polRef = new ListView[1];
            var pols = u.Policies.ToList();
            if (pols.Count == 0) f.Text("Nenhuma política anexada diretamente" + (u.Group != null ? " (as permissões vêm do grupo " + u.Group + ")." : "."), fSmall, Muted);
            else polRef[0] = f.Table(new[] { "Nome da política", "Descrição" }, new[] { 1.4f, 2.6f }, pols.Select(p => new[] { p, (AwsSim.ManagedPolicies.FirstOrDefault(x => x[0] == p) ?? new[] { p, "" })[1] }).ToList(), null);
            f.Buttons(Btn("Remover política", false, () =>
            {
                var lv = polRef[0];
                if (lv == null || lv.SelectedIndices.Count == 0) { Flash("Selecione uma política anexada primeiro.", InfoBlue); return; }
                string p = pols[lv.SelectedIndices[0]];
                Sim.DetachPolicy(uname, p);
                Flash("Política " + p + " removida de " + uname + ".", OkGreen);
            }), Btn("Adicionar permissões", true, () => Navigate("iam-attach", uname)));
            pg.EndSection(box, f);
            var g = pg.Section("Credenciais de segurança", out box);
            g.Text("Autenticação multifator (MFA): " + (u.Mfa ? "● Ativada (aplicativo autenticador)" : "● Nenhum dispositivo atribuído"), fUi, u.Mfa ? OkGreen : ErrRed);
            if (!u.Mfa) g.Buttons(Btn("Atribuir dispositivo MFA", true, () => Navigate("iam-mfa", u.Name)));
            pg.EndSection(box, g);
            // chaves de acesso (credenciais de longo prazo para CLI/SDK)
            var keys = Sim.AccessKeys.Where(k => k.User == uname).ToList();
            var kRef = new ListView[1];
            var h = pg.Section("Chaves de acesso (" + keys.Count + ")", out box);
            h.Text("Usadas pela CLI e por programas. No máximo 2 por usuário. A chave secreta só aparece na criação. Prefira funções (roles) para aplicações na AWS.", fSmall, Muted);
            if (keys.Count == 0) h.Text("Nenhuma chave de acesso.", fUi, Muted);
            else kRef[0] = h.Table(new[] { "ID da chave de acesso", "Status", "Criada em", "Idade" }, new[] { 2.4f, 1f, 1.2f, 1f }, keys.Select(k => new[] { k.Id, k.Status == "Active" ? "● Ativa" : "● Inativa", k.Created.ToString("dd/MM/yyyy"), (int)(DateTime.Now - k.Created).TotalDays + " dias" }).ToList(), null);
            var kmenu = new ContextMenuStrip { Font = fUi };
            Func<AwsSim.AccessKey> selKey = () =>
            {
                var lv = kRef[0];
                if (lv == null || lv.SelectedIndices.Count == 0) { Flash("Selecione uma chave de acesso primeiro.", InfoBlue); return null; }
                return keys[lv.SelectedIndices[0]];
            };
            kmenu.Items.Add("Desativar", null, (s, e) => { var k = selKey(); if (k == null) return; var er = Sim.SetKeyStatus(uname, k.Id, "Inactive"); if (er != null) Flash("✖ " + er, ErrRed); else Flash("Chave " + k.Id + " desativada.", OkGreen); });
            kmenu.Items.Add("Ativar", null, (s, e) => { var k = selKey(); if (k == null) return; var er = Sim.SetKeyStatus(uname, k.Id, "Active"); if (er != null) Flash("✖ " + er, ErrRed); else Flash("Chave " + k.Id + " ativada.", OkGreen); });
            kmenu.Items.Add(new ToolStripSeparator());
            kmenu.Items.Add("Excluir", null, (s, e) =>
            {
                var k = selKey(); if (k == null) return;
                if (MessageBox.Show(FindForm(), "Excluir a chave " + k.Id + "? Quem usa essa chave perde o acesso na hora, e não tem volta.", "Excluir chave de acesso", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                var er = Sim.DeleteAccessKey(uname, k.Id); if (er != null) Flash("✖ " + er, ErrRed); else Flash("Chave " + k.Id + " excluída.", OkGreen);
            });
            Button kBtn = null;
            kBtn = Btn("Ações  ▾", false, () => ShowMenu(kmenu, kBtn));
            h.Buttons(kBtn, Btn("Criar chave de acesso", true, () =>
            {
                string id;
                var er = Sim.CreateAccessKey(uname, out id);
                if (er != null) { Flash("✖ " + er, ErrRed); return; }
                Flash("Chave " + id + " criada. Chave secreta (aparece só agora): " + id.Substring(5, 8).ToLowerInvariant() + "/" + "kq" + id.Substring(12) + "...  — guarde no cofre de segredos.", OkGreen);
            }));
            pg.EndSection(box, h);
        }

        void PageIamPolicies(Flow pg)
        {
            PageTitle = "Políticas - IAM";
            pg.Crumbs("IAM", "Políticas");
            pg.Header("Políticas (" + AwsSim.ManagedPolicies.Length + ")", "Uma política é um documento JSON que permite (ou nega) ações em recursos. As 'gerenciadas pela AWS' já vêm prontas.");
            var rows = AwsSim.ManagedPolicies.Select(p => new[] { p[0], "Gerenciada pela AWS", p[1] }).ToList();
            pg.Table(new[] { "Nome da política", "Tipo", "Descrição" }, new[] { 2.2f, 1.2f, 3f }, rows, null);
            pg.Text("Para dar uma política a alguém: Usuários → (usuário) → Adicionar permissões.", fSmall, Muted);
        }

        void PageIamAttach(Flow pg)
        {
            var u = Sim.Users.FirstOrDefault(x => x.Name == arg);
            if (u == null) { Navigate("iam", null, false); return; }
            PageTitle = "Adicionar permissões - IAM";
            pg.Crumbs("IAM", "Usuários", u.Name, "Adicionar permissões");
            pg.Header("Adicionar permissões", "Usuário: " + u.Name + " · Opção: anexar políticas diretamente");
            Panel box;
            var f = pg.Section("Políticas de permissões (gerenciadas pela AWS)", out box);
            var rad = f.Radios(null, AwsSim.ManagedPolicies.Select(p => p[0] + "  —  " + p[1]).ToArray(), -1);
            pg.EndSection(box, f);
            var err = pg.Error();
            string uname = u.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("iam-user", uname)), Btn("Adicionar permissões", true, () =>
            {
                int i = rad.FindIndex(r => r.Checked);
                if (i < 0) { err.Text = "✖ Escolha uma política."; return; }
                var e = Sim.AttachPolicy(uname, AwsSim.ManagedPolicies[i][0]);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("iam-user", uname, false);
                Flash("Política " + AwsSim.ManagedPolicies[i][0] + " anexada a " + uname + ".", OkGreen);
            }));
        }

        void PageMfa(Flow pg)
        {
            PageTitle = "Atribuir MFA - IAM";
            pg.Crumbs("IAM", "Usuários", arg, "Atribuir dispositivo MFA");
            pg.Header("Selecionar dispositivo MFA", "Usuário: " + arg);
            Panel box;
            var f = pg.Section("Dispositivo", out box);
            var dev = f.Radios(null, new[] { "Aplicativo autenticador (no celular)", "Chave de segurança física", "Token TOTP de hardware" }, 0);
            pg.EndSection(box, f);
            var g = pg.Section("Configurar o aplicativo", out box);
            g.Text("1) Abra o aplicativo autenticador no celular e escaneie o código QR.", fUi, Ink);
            var qr = new Panel { Location = new Point(g.X, g.Y), Size = new Size(S(110), S(110)), BackColor = Color.White };
            qr.Paint += (s, e) =>
            {
                var rnd = new Random(42);
                int cell = qr.Width / 11;
                for (int i = 0; i < 11; i++) for (int j = 0; j < 11; j++) if (rnd.Next(2) == 0 || (i < 3 && j < 3) || (i > 7 && j < 3) || (i < 3 && j > 7)) e.Graphics.FillRectangle(Brushes.Black, i * cell, j * cell, cell, cell);
            };
            box.Controls.Add(qr);
            var phone = new Label { Text = "📱 Seu celular mostra:\n\nCódigo agora:  482913\nPróximo código:  105662", Font = new Font("Segoe UI Semibold", 10.5f), ForeColor = Ink, Location = new Point(g.X + S(130), g.Y + S(6)), AutoSize = true, BackColor = Art.Hx(0xF2F8FD), Padding = new Padding(S(10)) };
            box.Controls.Add(phone);
            g.Y += S(122);
            g.Text("2) Digite dois códigos consecutivos do aplicativo.", fUi, Ink);
            var c1 = g.Field("Código MFA 1", null);
            var c2 = g.Field("Código MFA 2", null);
            pg.EndSection(box, g);
            var err = pg.Error();
            string user = arg;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("iam-user", user)), Btn("Adicionar MFA", true, () =>
            {
                if (dev.FindIndex(r => r.Checked) != 0) { err.Text = "✖ Neste laboratório, use o aplicativo autenticador."; return; }
                if (c1.Text.Trim() != "482913" || c2.Text.Trim() != "105662") { err.Text = "✖ Códigos inválidos. Digite os dois códigos mostrados no aplicativo, na ordem."; return; }
                Sim.EnableMfa(user);
                Navigate("iam-user", user, false);
                Flash("Dispositivo MFA atribuído a " + user + ".", OkGreen);
            }));
        }

        // ── Lambda
        void PageLambda(Flow pg)
        {
            PageTitle = "Funções - Lambda";
            pg.Crumbs("Lambda", "Funções");
            pg.Header("Funções (" + Sim.Functions.Count + ")", "Execute código sem provisionar ou gerenciar servidores.");
            var rows = Sim.Functions.Select(fn => new[] { fn.Name, fn.Runtime, "Zip", DateTime.Now.AddDays(-2).ToString("dd/MM/yyyy") }).ToList();
            pg.Table(new[] { "Nome da função", "Tempo de execução", "Tipo de pacote", "Última modificação" }, new[] { 2f, 1.6f, 1f, 1.5f }, rows, i => Navigate("lambda-fn", Sim.Functions[i].Name));
        }

        void PageLambdaFn(Flow pg)
        {
            var fn = Sim.Functions.FirstOrDefault(x => x.Name == arg);
            if (fn == null) { Navigate("lambda", null, false); return; }
            PageTitle = fn.Name + " - Lambda";
            pg.Crumbs("Lambda", "Funções", fn.Name);
            bool node = fn.Runtime.StartsWith("Node");
            pg.Header(fn.Name, "Tempo de execução: " + fn.Runtime + " · Manipulador: " + (node ? "index.handler" : "lambda_function.lambda_handler"));
            Panel box;
            string fnm = fn.Name;
            var ov = pg.Section("Visão geral da função", out box);
            ov.Text("Gatilhos: " + (fn.Triggers.Count == 0 ? "nenhum" : string.Join(", ", fn.Triggers.Select(t => "SQS " + t[0] + " (lote " + t[1] + ")"))), fUi, Ink);
            ov.Text("Função de execução (role): " + fn.Name + "-role · Memória " + fn.Memory + " MB · Tempo limite " + FmtTimeout(fn.Timeout), fSmall, Muted);
            ov.Buttons(Btn("Adicionar gatilho", false, () => Navigate("lambda-trigger-create", fnm)));
            pg.EndSection(box, ov);
            pg.Tabs(new[] { "Código", "Testar", "Configuração" }, lambdaTab, tb => { lambdaTab = tb; Navigate("lambda-fn", fnm, false); });
            if (lambdaTab == "Configuração") { PageLambdaConfig(pg, fn); return; }
            if (lambdaTab == "Código")
            {
                var f = pg.Section("Código-fonte (" + (node ? "index.mjs" : "lambda_function.py") + ")", out box);
                var code = new TextBox
                {
                    Multiline = true, ReadOnly = true, Font = fMono, BackColor = Art.Hx(0x0F1B2A), ForeColor = Art.Hx(0xD4D4D4), Location = new Point(f.X, f.Y), Width = f.W, Height = S(130), BorderStyle = BorderStyle.None,
                    Text = node ? "export const handler = async (event) => {\r\n  // envia o e-mail de confirmação do pedido via SMTP\r\n  await enviarEmail(process.env.SMTP_HOST, event.pedido);\r\n  return { statusCode: 200 };\r\n};"
                         : fn.Name == "pedidos" ? "import json\r\n\r\ndef lambda_handler(event, context):\r\n    pedido = event.get(\"pedido\", {})\r\n    qtd = pedido.get(\"quantidade\", 1)   # corrigido: antes dava KeyError\r\n    return {\"statusCode\": 200, \"body\": f\"Pedido {pedido.get('id')} registrado\"}"
                         : "import os, json\r\n\r\ndef lambda_handler(event, context):\r\n    # " + fn.Name + ": processa o evento recebido\r\n    registros = event.get(\"Records\", [])\r\n    return {\"statusCode\": 200, \"body\": json.dumps({\"processados\": len(registros)})}"
                };
                box.Controls.Add(code);
                f.Y += code.Height + S(10);
                pg.EndSection(box, f);
                var rt = pg.Section("Configurações de tempo de execução", out box);
                rt.Text("Tempo de execução: " + fn.Runtime + (fn.Runtime == "Python 3.8" ? "   ⚠ descontinuado (sem atualizações de segurança)" : ""), fBold, fn.Runtime == "Python 3.8" ? ErrRed : Ink);
                rt.Text("Manipulador: " + (node ? "index.handler" : "lambda_function.lambda_handler") + " · Arquitetura: x86_64", fSmall, Muted);
                rt.Buttons(Btn("Editar", false, () => Navigate("lambda-runtime-edit", fnm)));
                pg.EndSection(box, rt);
                return;
            }
            var g = pg.Section("Testar", out box);
            var ev = g.Field("Nome do evento", null, "pedido-teste");
            g.Text("JSON do evento:", fBold, Ink);
            var json = new TextBox { Multiline = true, Font = fMono, Location = new Point(g.X, g.Y), Width = Math.Min(g.W, S(460)), Height = S(60), Text = "{ \"pedido\": { \"id\": 4821, \"produto\": \"cafe\", \"quantidade\": 2 } }" };
            box.Controls.Add(json);
            g.Y += json.Height + S(10);
            Label result = null;
            string fname = fn.Name;
            g.Buttons(Btn("Testar", true, () =>
            {
                if (string.IsNullOrWhiteSpace(ev.Text)) { Flash("Dê um nome ao evento de teste.", InfoBlue); return; }
                var r = Sim.Invoke(fname);
                result.Text = "✔ Execução da função: bem-sucedida\nResposta: " + r + "\nDuração: 112 ms · Memória usada: 41 MB de " + fn.Memory + " MB";
                result.ForeColor = OkGreen;
            }));
            result = g.Text(fn.LastResult != null ? "Último resultado: " + fn.LastResult : "Ainda não testada.", fUi, Muted);
            result.Height = S(64);
            g.Y += S(30);
            pg.EndSection(box, g);
        }

        // ── CloudWatch
        void PageAlarms(Flow pg)
        {
            PageTitle = "Alarmes - CloudWatch";
            pg.Crumbs("CloudWatch", "Alarmes");
            var lvRef = new ListView[1];
            var list = Sim.Alarms.ToList();
            pg.Header("Alarmes (" + list.Count + ")", "Alarmes observam uma métrica e mudam de estado quando a condição é atingida; a ação (tópico do SNS) avisa quem precisa agir.",
                Btn("Excluir", false, () =>
                {
                    var lv = lvRef[0];
                    if (lv == null || lv.SelectedIndices.Count == 0) { Flash("Selecione um alarme na lista primeiro.", InfoBlue); return; }
                    var a = list[lv.SelectedIndices[0]];
                    if (MessageBox.Show(FindForm(), "Excluir o alarme " + a.Name + "?", "Excluir alarme", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                    var er = Sim.DeleteAlarm(a.Name);
                    if (er != null) Flash("✖ " + er, ErrRed); else Flash("Alarme " + a.Name + " excluído.", OkGreen);
                }),
                Btn("Criar alarme", true, () => Navigate("cw-create", null)));
            var rows = list.Select(a => new[] { a.Name, "● " + a.State, a.Namespace + " " + a.Metric + (a.Comparison.StartsWith("Less") ? " < " : " > ") + a.Threshold + " (" + a.Resource + ")", a.Action != null ? "SNS: " + a.Action : "Nenhuma" }).ToList();
            lvRef[0] = pg.Table(new[] { "Nome", "Estado", "Condições", "Ações" }, new[] { 1.8f, 1.3f, 3.2f, 1.3f }, rows, null);
        }

        void PageAlarmCreate(Flow pg)
        {
            PageTitle = "Criar alarme - CloudWatch";
            pg.Crumbs("CloudWatch", "Alarmes", "Criar alarme");
            pg.Header("Criar alarme", null);
            Panel box;
            var f = pg.Section("Etapa 1: métrica", out box);
            var nsSel = f.Select("Namespace", AwsSim.Metrics.Select(m => m[0] + (m[0] == "AWS/EC2" ? "  (instâncias)" : m[0] == "AWS/SQS" ? "  (filas)" : "  (bancos RDS)")).ToArray(), 0);
            var metric = f.Select("Métrica", AwsSim.Metrics[0].Skip(1).ToArray(), -1);
            var resLabel = f.Text("Recurso (instância, fila ou banco)", fBold, Ink);
            var inst = new ComboBox { Location = new Point(f.X, f.Y), Width = Math.Min(f.W, S(460)), Font = fUi, DropDownStyle = ComboBoxStyle.DropDownList };
            box.Controls.Add(inst);
            f.Y += inst.Height + S(12);
            var resources = new List<string>();
            Action fill = () =>
            {
                string ns = AwsSim.Metrics[Math.Max(0, nsSel.SelectedIndex)][0];
                metric.Items.Clear(); metric.Items.AddRange(AwsSim.Metrics[Math.Max(0, nsSel.SelectedIndex)].Skip(1).ToArray());
                inst.Items.Clear(); resources.Clear();
                if (ns == "AWS/EC2") foreach (var i in Sim.Instances.Where(i => i.State != "Encerrada")) { inst.Items.Add(i.Name + " (" + i.Id + ")"); resources.Add(i.Name); }
                else if (ns == "AWS/SQS") foreach (var q in Sim.Queues) { inst.Items.Add("QueueName = " + q.Name); resources.Add(q.Name); }
                else foreach (var d in Sim.Dbs) { inst.Items.Add("DBInstanceIdentifier = " + d.Id); resources.Add(d.Id); }
            };
            fill();
            nsSel.SelectedIndexChanged += (s, e) => fill();
            pg.EndSection(box, f);
            var g = pg.Section("Etapa 2: condições", out box);
            var cond = g.Radios("Sempre que a métrica for...", new[] { "Maior que", "Menor que" }, 0);
            var th = g.Field("... do que (limite)", "Para CPU, em porcentagem (0 a 100). Para StatusCheckFailed, 0 = tudo bem e 1 = falhou. Para filas, quantidade de mensagens.");
            pg.EndSection(box, g);
            var a2 = pg.Section("Etapa 3: notificação (ação do alarme)", out box);
            var topics = Sim.Topics.Select(t => t.Name).ToList();
            var act = a2.Select("Quando entrar EM ALARME, enviar notificação para o tópico do SNS", new[] { "Nenhuma notificação" }.Concat(topics.Select(t => t + "  (" + Sim.Tp(t).Subs.Count + " assinatura(s))")).ToArray(), 0);
            pg.EndSection(box, a2);
            var h = pg.Section("Etapa 4: nome", out box);
            var name = h.Field("Nome do alarme", null);
            pg.EndSection(box, h);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("cloudwatch", null)), Btn("Criar alarme", true, () =>
            {
                if (metric.SelectedIndex < 0 || inst.SelectedIndex < 0) { err.Text = "✖ Escolha a métrica e o recurso."; return; }
                float v;
                if (!float.TryParse(th.Text.Trim().Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v)) { err.Text = "✖ O limite precisa ser um número."; return; }
                string m = metric.Items[metric.SelectedIndex].ToString();
                string ns = AwsSim.Metrics[Math.Max(0, nsSel.SelectedIndex)][0];
                bool less = cond.FindIndex(r => r.Checked) == 1;
                if (less && m == "CPUUtilization") { err.Text = "✖ Para CPU alta, a condição deve ser 'Maior que'."; return; }
                var e = Sim.CreateAlarm(name.Text, m, resources[inst.SelectedIndex], v, ns, less ? "LessThanThreshold" : "GreaterThanThreshold", act.SelectedIndex > 0 ? topics[act.SelectedIndex - 1] : null);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("cloudwatch", null, false);
                Flash("Alarme \"" + name.Text.Trim() + "\" criado.", OkGreen);
            }));
        }

        void PageLogs(Flow pg)
        {
            PageTitle = "Grupos de logs - CloudWatch";
            pg.Crumbs("CloudWatch", "Grupos de logs");
            var groups = AwsSim.LogGroups;
            var lvRef = new ListView[1];
            var menu = new ContextMenuStrip { Font = fUi };
            menu.Items.Add("Editar configuração de retenção", null, (s, e) =>
            {
                var lv = lvRef[0];
                if (lv == null || lv.SelectedIndices.Count == 0) { Flash("Selecione um grupo de logs na lista primeiro (clique fora do nome).", InfoBlue); return; }
                Navigate("cw-retention-edit", groups[lv.SelectedIndices[0]]);
            });
            Button actBtn = null;
            actBtn = Btn("Ações  ▾", false, () => ShowMenu(menu, actBtn));
            pg.Header("Grupos de logs (" + groups.Length + ")", "Logs com retenção 'Nunca expirar' ficam guardados (e cobrados) para sempre.", actBtn);
            var rows = groups.Select(x => new[] { x, AwsSim.RetentionText(Sim.Retention(x)), x.StartsWith("/ecs") ? "38,2 GB" : "12,4 MB" }).ToList();
            lvRef[0] = pg.Table(new[] { "Grupo de logs", "Retenção", "Bytes armazenados" }, new[] { 3f, 1.4f, 1f }, rows, i => Navigate("cw-log", groups[i]));
            pg.Text("Clique no nome para ver os eventos. Para mudar a retenção: selecione a linha → Ações.", fSmall, Muted);
        }

        string logFilter;   // padrão de filtro do grupo de logs aberto

        void PageLog(Flow pg)
        {
            if (arg == null) { PageLogs(pg); return; }
            PageTitle = "Eventos de log - CloudWatch";
            pg.Crumbs("CloudWatch", "Grupos de logs", arg);
            pg.Header(arg, "Retenção: " + AwsSim.RetentionText(Sim.Retention(arg)));
            var t = DateTime.Now;
            var src = AwsSim.LogLines(arg ?? "");
            string flt = logFilter;
            string term = (flt ?? "").Trim().Trim('"').TrimStart('?');
            var lines = src.Select((l, i) => new { l, i }).Where(x => term.Length == 0 || x.l.Contains(term)).Select(x => t.AddMinutes(-(src.Length - x.i) * 2).ToString("HH:mm:ss") + "  " + x.l).ToArray();
            Panel box;
            var f = pg.Section("Eventos de log", out box);
            var fbox = new TextBox { Location = new Point(f.X, f.Y), Width = Math.Min(f.W - S(100), S(360)), Font = fUi, Text = flt ?? "" };
            box.Controls.Add(fbox);
            string grp = arg;
            Action apply = () => { logFilter = fbox.Text.Trim(); Sim.FilterLogs(grp, logFilter); Navigate("cw-log", grp, false); };
            fbox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; apply(); } };
            var fb = Btn("Filtrar", false, () => apply());
            fb.Location = new Point(fbox.Right + S(8), f.Y - S(3));
            box.Controls.Add(fb);
            f.Y += S(34);
            f.Text("Filtrar eventos: digite um termo (diferencia maiúsculas de minúsculas, ex.: ERROR) e aperte Enter. Vazio = todos.", fSmall, Muted);
            if (lines.Length == 0) f.Text("Nenhum evento corresponde ao filtro.", fUi, Muted);
            foreach (var l in lines) f.Text(l, fMono, l.Contains("ERROR") ? ErrRed : Ink);
            pg.EndSection(box, f);
        }

        // ── ECR
        void PageEcr(Flow pg)
        {
            PageTitle = "Repositórios - ECR";
            pg.Crumbs("Amazon ECR", "Repositórios privados");
            var list = Sim.Repos.ToList();
            var lvRef = new ListView[1];
            var menu = new ContextMenuStrip { Font = fUi };
            Action<string> open = target =>
            {
                var lv = lvRef[0];
                if (lv == null || lv.SelectedIndices.Count == 0) { Flash("Selecione um repositório na lista primeiro.", InfoBlue); return; }
                Navigate(target, list[lv.SelectedIndices[0]].Name);
            };
            menu.Items.Add("Editar (imutabilidade de tags e verificação)", null, (s, e) => open("ecr-edit"));
            menu.Items.Add("Política de ciclo de vida → Criar regra", null, (s, e) => open("ecr-lifecycle-create"));
            Button actBtn = null;
            actBtn = Btn("Ações  ▾", false, () => ShowMenu(menu, actBtn));
            pg.Header("Repositórios privados (" + list.Count + ")", null, actBtn, Btn("Criar repositório", true, () => Navigate("ecr-create", null)));
            var rows = list.Select(r => new[] { r.Name, "123456789012.dkr.ecr." + Sim.Region + ".amazonaws.com/" + r.Name, r.ScanOnPush ? "● Ativado" : "● Desativado", r.Immutable ? "Imutável" : "Mutável", r.LcTag == null ? "Nenhuma" : EcrRuleText(r) }).ToList();
            lvRef[0] = pg.Table(new[] { "Nome do repositório", "URI", "Verificar ao enviar", "Tags", "Ciclo de vida" }, new[] { 1.5f, 3.2f, 1.3f, 1f, 2f }, rows, null);
            pg.Text("Selecione o repositório e use 'Ações'.", fSmall, Muted);
        }

        static string EcrRuleText(AwsSim.Repo r)
        {
            string who = r.LcTag == "untagged" ? "imagens sem tag" : "qualquer imagem";
            return r.LcType == "sinceImagePushed" ? "expirar " + who + " com mais de " + r.LcN + " dia(s)" : "manter só " + r.LcN + " (" + who + ")";
        }

        void PageEcrCreate(Flow pg)
        {
            PageTitle = "Criar repositório - ECR";
            pg.Crumbs("Amazon ECR", "Repositórios privados", "Criar repositório");
            pg.Header("Criar repositório", null);
            Panel box;
            var f = pg.Section("Configurações gerais", out box);
            var vis = f.Radios("Visibilidade", new[] { "Privado", "Público" }, 0);
            var name = f.Field("Nome do repositório", "123456789012.dkr.ecr." + Sim.Region + ".amazonaws.com/<nome>");
            var imm = f.Check("Imutabilidade de tags", false, "Impede sobrescrever uma tag de imagem já enviada.");
            pg.EndSection(box, f);
            var g = pg.Section("Configurações de verificação de imagens", out box);
            var scan = g.Check("Verificar ao enviar (scan on push)", false, "Procura vulnerabilidades conhecidas (CVEs) em cada imagem enviada.");
            pg.EndSection(box, g);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ecr", null)), Btn("Criar repositório", true, () =>
            {
                if (vis.FindIndex(r => r.Checked) != 0) { err.Text = "✖ As imagens da API são internas: use um repositório privado."; return; }
                var e = Sim.CreateRepo(name.Text, scan.Checked, imm.Checked);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("ecr", null, false);
                Flash("Repositório " + name.Text.Trim() + " criado.", OkGreen);
            }));
        }

        // ── ECS
        void PageEcs(Flow pg)
        {
            PageTitle = "Clusters - ECS";
            pg.Crumbs("Amazon ECS", "Clusters");
            pg.Header("Clusters (1)", null);
            var rows = new List<string[]> { new[] { "cafe365", "1", Sim.EcsRunning.ToString(), "Fargate" } };
            pg.Table(new[] { "Cluster", "Serviços", "Tarefas em execução", "Infraestrutura" }, new[] { 2f, 1f, 1.5f, 1.2f }, rows, i => Navigate("ecs-cluster", "cafe365"));
        }

        void PageEcsCluster(Flow pg)
        {
            PageTitle = "cafe365 - ECS";
            pg.Crumbs("Amazon ECS", "Clusters", "cafe365");
            var lvRef = new ListView[1];
            pg.Header("cafe365", "Cluster Fargate · " + AwsSim.RegionName("us-east-1"),
                Btn("Atualizar serviço", true, () =>
                {
                    var lv = lvRef[0];
                    if (lv == null || lv.SelectedIndices.Count == 0) { Flash("Selecione o serviço api na lista primeiro.", InfoBlue); return; }
                    Navigate("ecs-update", "api");
                }));
            var rows = new List<string[]> { new[] { "api", "● Ativo", Sim.EcsRunning + "/" + Sim.EcsRunning + " tarefas", "cafe365-api:7", Sim.EcsDeployments + " implantação(ões)" } };
            lvRef[0] = pg.Table(new[] { "Serviço", "Status", "Tarefas", "Definição de tarefa", "Implantações" }, new[] { 1f, 1f, 1.3f, 1.6f, 1.6f }, rows, null);
            pg.Text("Selecione a linha do serviço e clique em 'Atualizar serviço'.", fSmall, Muted);
        }

        void PageEcsUpdate(Flow pg)
        {
            PageTitle = "Atualizar serviço - ECS";
            pg.Crumbs("Amazon ECS", "Clusters", "cafe365", "api", "Atualizar");
            pg.Header("Atualizar serviço api", "A definição de tarefa continua cafe365-api:7 (a imagem nova usa a mesma tag).");
            Panel box;
            var f = pg.Section("Configuração da implantação", out box);
            var desired = f.Field("Tarefas desejadas", null, Sim.EcsRunning.ToString());
            var force = f.Check("Forçar nova implantação", false, "Sobe tarefas novas mesmo sem mudar a definição (útil quando a imagem foi atualizada com a mesma tag).");
            pg.EndSection(box, f);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ecs-cluster", "cafe365")), Btn("Atualizar", true, () =>
            {
                int d;
                if (!int.TryParse(desired.Text.Trim(), out d) || d < 1) { err.Text = "✖ Informe um número de tarefas maior que zero."; return; }
                Sim.UpdateService(force.Checked, d);
                Navigate("ecs-cluster", "cafe365", false);
                Flash(force.Checked ? "Serviço api atualizado: nova implantação em andamento (rolling update)." : "Serviço api atualizado (sem nova implantação).", force.Checked ? OkGreen : InfoBlue);
            }));
        }

        // ── CloudFormation
        void PageCfn(Flow pg)
        {
            PageTitle = "Pilhas - CloudFormation";
            pg.Crumbs("CloudFormation", "Pilhas");
            pg.Header("Pilhas (" + Sim.Stacks.Count + ")", null, Btn("Criar pilha", true, () => Navigate("cfn-create", null)));
            var rows = Sim.Stacks.Select(s => new[] { s.Name, "● " + s.Status, s.Template, s.Reason ?? "-" }).ToList();
            pg.Table(new[] { "Nome da pilha", "Status", "Modelo", "Motivo do status" }, new[] { 1.4f, 1.8f, 1f, 3f }, rows, null);
        }

        void PageCfnCreate(Flow pg)
        {
            PageTitle = "Criar pilha - CloudFormation";
            pg.Crumbs("CloudFormation", "Pilhas", "Criar pilha");
            pg.Header("Criar pilha", null);
            Panel box;
            var f = pg.Section("Especificar modelo", out box);
            var tpl = f.Select("Carregar um arquivo de modelo", new[] { "infra.yaml  (rede, API, banco e role do IAM)", "rede.yaml  (só a rede)" }, -1);
            pg.EndSection(box, f);
            var g = pg.Section("Detalhes da pilha", out box);
            var name = g.Field("Nome da pilha", null);
            pg.EndSection(box, g);
            var h = pg.Section("Capacidades", out box);
            h.Text("O modelo contém recursos do IAM que podem dar acesso à sua conta. Revise antes de continuar.", fSmall, Muted);
            var ack = h.Check("Eu reconheço que o AWS CloudFormation pode criar recursos do IAM.", false);
            pg.EndSection(box, h);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("cloudformation", null)), Btn("Enviar", true, () =>
            {
                if (tpl.SelectedIndex < 0) { err.Text = "✖ Escolha o arquivo de modelo."; return; }
                if (string.IsNullOrWhiteSpace(name.Text)) { err.Text = "✖ Informe o nome da pilha."; return; }
                var st = Sim.CreateStack(name.Text, tpl.SelectedIndex == 0 ? "infra.yaml" : "rede.yaml", ack.Checked);
                Navigate("cloudformation", null, false);
                if (st.Status == "CREATE_COMPLETE") Flash("Pilha " + st.Name + " criada: CREATE_COMPLETE.", OkGreen);
                else Flash("Falha na pilha " + st.Name + ": " + st.Reason, ErrRed);
            }));
        }

        // ═════════════════════════ páginas da 2ª leva ═════════════════════════

        static string FmtTimeout(int s) { return (s / 60) + " min " + (s % 60) + " s"; }

        int SelIdx(ListView[] r, string what)
        {
            var lv = r[0];
            if (lv == null || lv.SelectedIndices.Count == 0) { Flash("Selecione " + what + " na lista primeiro.", InfoBlue); return -1; }
            return lv.SelectedIndices[0];
        }

        // botão com menu suspenso: pares (texto, ação)
        Button MenuBtn(string text, params object[] items)
        {
            var menu = new ContextMenuStrip { Font = fUi };
            for (int i = 0; i + 1 < items.Length; i += 2)
            {
                var act = (Action)items[i + 1];
                menu.Items.Add((string)items[i], null, (s, e) => act());
            }
            Button b = null;
            b = Btn(text, false, () => ShowMenu(menu, b));
            return b;
        }

        // abre um menu suspenso embaixo do botão (o último aberto fica acessível para testes automáticos)
        public ContextMenuStrip LastMenu { get; private set; }
        void ShowMenu(ContextMenuStrip m, Control under) { LastMenu = m; m.Show(under, new Point(0, under.Height)); }

        bool Confirm(string text, string title) { return MessageBox.Show(FindForm(), text, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes; }
        void Done(string err, string ok) { if (err != null) Flash("✖ " + err, ErrRed); else Flash(ok, OkGreen); }

        // linhas chave/valor editáveis (tags, variáveis de ambiente)
        List<TextBox[]> KvEditor(Flow f, Panel box, IEnumerable<KeyValuePair<string, string>> existing, int extra, string keyLabel, string valLabel)
        {
            var rows = new List<TextBox[]>();
            int w = Math.Min(f.W, S(560));
            int kw = (w - S(16)) * 2 / 5, vw = w - S(16) - kw;
            box.Controls.Add(new Label { Text = keyLabel, Font = fBold, ForeColor = Ink, Location = new Point(f.X, f.Y), AutoSize = true });
            box.Controls.Add(new Label { Text = valLabel, Font = fBold, ForeColor = Ink, Location = new Point(f.X + kw + S(16), f.Y), AutoSize = true });
            f.Y += S(24);
            foreach (var kv in existing.Concat(Enumerable.Range(0, extra).Select(i => new KeyValuePair<string, string>("", ""))))
            {
                var k = new TextBox { Location = new Point(f.X, f.Y), Width = kw, Font = fUi, Text = kv.Key };
                var v = new TextBox { Location = new Point(f.X + kw + S(16), f.Y), Width = vw, Font = fUi, Text = kv.Value };
                box.Controls.Add(k); box.Controls.Add(v);
                rows.Add(new[] { k, v });
                f.Y += k.Height + S(8);
            }
            f.Text("Para remover um item, apague a chave da linha. Linhas com a chave vazia são ignoradas.", fSmall, Muted);
            return rows;
        }

        static Dictionary<string, string> KvRead(List<TextBox[]> rows, out string err)
        {
            err = null;
            var d = new Dictionary<string, string>();
            foreach (var r in rows)
            {
                string k = r[0].Text.Trim();
                if (k.Length == 0) continue;
                if (d.ContainsKey(k)) { err = "Chave repetida: " + k; return null; }
                d[k] = r[1].Text.Trim();
            }
            return d;
        }

        static int ToInt(string s, int def) { int v; return int.TryParse((s ?? "").Trim(), out v) ? v : def; }

        bool PageExtra(string p, Flow pg)
        {
            switch (p)
            {
                case "ec2-protect-edit": PageEc2Protect(pg); return true;
                case "ec2-imds-edit": PageEc2Imds(pg); return true;
                case "ec2-role-edit": PageEc2Role(pg); return true;
                case "ec2-image-create": PageEc2Image(pg); return true;
                case "ec2-amis": PageAmis(pg); return true;
                case "ec2-eips": PageEips(pg); return true;
                case "ec2-eip-assoc-edit": PageEipAssoc(pg); return true;
                case "ec2-keys": PageKeys(pg); return true;
                case "ec2-key-create": PageKeyCreate(pg); return true;
                case "ec2-vol-edit": PageVolEdit(pg); return true;
                case "ec2-sg-create": PageSgCreate(pg); return true;
                case "ec2-asg": PageAsgs(pg); return true;
                case "ec2-asg-detail": PageAsg(pg); return true;
                case "ec2-asg-edit": PageAsgEdit(pg); return true;
                case "ec2-asg-policy-create": PageAsgPolicy(pg); return true;
                case "ec2-settings": PageEc2Settings(pg); return true;
                case "ec2-ebsenc-edit": PageEbsEnc(pg); return true;
                case "s3-tags-edit": PageS3Tags(pg); return true;
                case "s3-website-edit": PageS3Website(pg); return true;
                case "s3-bpa-edit": PageS3Bpa(pg); return true;
                case "s3-policy-edit": PageS3Policy(pg); return true;
                case "s3-lifecycle-create": PageS3Lifecycle(pg); return true;
                case "s3-presign-create": PageS3Presign(pg); return true;
                case "s3-versions": PageS3Versions(pg); return true;
                case "iam-roles": PageRoles(pg); return true;
                case "iam-role": PageRole(pg); return true;
                case "iam-role-create": PageRoleCreate(pg); return true;
                case "iam-role-attach": PageRoleAttach(pg); return true;
                case "iam-settings": PageIamSettings(pg); return true;
                case "iam-pwpolicy-edit": PagePwPolicy(pg); return true;
                case "lambda-general-edit": PageLambdaGeneral(pg); return true;
                case "lambda-runtime-edit": PageLambdaRuntime(pg); return true;
                case "lambda-env-edit": PageLambdaEnv(pg); return true;
                case "lambda-concurrency-edit": PageLambdaConcurrency(pg); return true;
                case "lambda-trigger-create": PageLambdaTrigger(pg); return true;
                case "cw-retention-edit": PageRetention(pg); return true;
                case "ecr-edit": PageEcrEdit(pg); return true;
                case "ecr-lifecycle-create": PageEcrLifecycle(pg); return true;
                case "vpc": PageVpcs(pg); return true;
                case "vpc-subnets": PageSubnets(pg); return true;
                case "vpc-subnet-create": PageSubnetCreate(pg); return true;
                case "vpc-rtbs": PageRtbs(pg); return true;
                case "vpc-rtb": PageRtb(pg); return true;
                case "vpc-routes-edit": PageRoutesEdit(pg); return true;
                case "sns": PageTopics(pg); return true;
                case "sns-create": PageTopicCreate(pg); return true;
                case "sns-topic": PageTopic(pg); return true;
                case "sns-sub-create": PageSubCreate(pg); return true;
                case "sns-publish-edit": PagePublish(pg); return true;
                case "sqs": PageQueues(pg); return true;
                case "sqs-create": PageQueueForm(pg, null); return true;
                case "sqs-queue": PageQueue(pg); return true;
                case "sqs-edit": PageQueueForm(pg, Sim.Q(arg ?? "")); return true;
                case "dynamodb": PageTables(pg); return true;
                case "ddb-create": PageTableCreate(pg); return true;
                case "ddb-table": PageTable(pg); return true;
                case "ddb-items": PageItems(pg); return true;
                case "ddb-ttl-edit": PageTtl(pg); return true;
                case "ddb-pitr-edit": PagePitr(pg); return true;
                case "rds": PageDbs(pg); return true;
                case "rds-db": PageDb(pg); return true;
                case "rds-modify-edit": PageDbModify(pg); return true;
                case "rds-snap-create": PageDbSnap(pg); return true;
                case "rds-snapshots": PageDbSnaps(pg); return true;
                case "route53": PageZones(pg); return true;
                case "r53-zone": PageZone(pg); return true;
                case "r53-record-create": PageRecordForm(pg, false); return true;
                case "r53-record-edit": PageRecordForm(pg, true); return true;
                case "cloudfront": PageDists(pg); return true;
                case "cdn-dist": PageDist(pg); return true;
                case "cdn-general-edit": PageDistGeneral(pg); return true;
                case "cdn-behavior-edit": PageDistBehavior(pg); return true;
                case "cdn-inval-create": PageInvalidation(pg); return true;
                case "cloudtrail": PageTrails(pg); return true;
                case "trail-create": PageTrailCreate(pg); return true;
                case "kms": PageKeysKms(pg); return true;
                case "kms-key": PageKmsKey(pg); return true;
                case "kms-rotation-edit": PageKmsRotation(pg); return true;
                case "budgets": PageBudgets(pg); return true;
                case "budget-create": PageBudgetCreate(pg); return true;
                case "budget-detail": PageBudget(pg); return true;
                case "budget-alert-create": PageBudgetAlert(pg); return true;
            }
            return false;
        }

        // ── EC2
        AwsSim.Ec2 InstPage(Flow pg, string title, string crumb)
        {
            var inst = Sim.Inst(arg);
            if (inst == null) { Navigate("ec2", null, false); return null; }
            PageTitle = title + " - EC2";
            pg.Crumbs("EC2", "Instâncias", inst.Id, crumb);
            pg.Header(title, inst.Name + " (" + inst.Id + ") · " + AwsSim.RegionName(inst.Region));
            return inst;
        }

        void PageEc2Protect(Flow pg)
        {
            var inst = InstPage(pg, "Alterar proteção contra encerramento", "Proteção contra encerramento");
            if (inst == null) return;
            Panel box;
            var f = pg.Section("Proteção contra encerramento", out box);
            f.Text("Com a proteção ativada, a instância não pode ser encerrada (apagada) pelo console, pela CLI ou pela API até alguém desativar a proteção.", fSmall, Muted);
            var on = f.Check("Ativar", inst.Protect);
            pg.EndSection(box, f);
            var err = pg.Error();
            string id = inst.Id;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ec2", null)), Btn("Salvar", true, () =>
            {
                var e = Sim.SetProtection(id, on.Checked);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("ec2", null, false);
                Flash("Proteção contra encerramento de " + id + ": " + (on.Checked ? "ativada" : "desativada") + ".", OkGreen);
            }));
        }

        void PageEc2Imds(Flow pg)
        {
            var inst = InstPage(pg, "Modificar opções de metadados da instância", "Opções de metadados");
            if (inst == null) return;
            Panel box;
            var f = pg.Section("Serviço de metadados da instância (IMDS)", out box);
            f.Text("O IMDS (http://169.254.169.254) entrega à aplicação dados da instância e as credenciais temporárias da função do IAM. Com o IMDSv2 obrigatório, toda leitura exige um token de sessão — isso barra ataques de SSRF que tentam roubar essas credenciais.", fSmall, Muted);
            var ep = f.Radios("Serviço de metadados", new[] { "Habilitar", "Desabilitar" }, inst.Imds == "disabled" ? 1 : 0);
            var ver = f.Radios("Versão dos metadados", new[] { "V1 e V2 (token opcional)", "Somente V2 (token obrigatório)" }, inst.Imds == "required" ? 1 : 0);
            pg.EndSection(box, f);
            var err = pg.Error();
            string id = inst.Id;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ec2", null)), Btn("Salvar", true, () =>
            {
                var e = Sim.SetImds(id, ver[1].Checked ? "required" : "optional", ep[0].Checked);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("ec2", null, false);
                Flash("Opções de metadados de " + id + " atualizadas.", OkGreen);
            }));
        }

        void PageEc2Role(Flow pg)
        {
            var inst = InstPage(pg, "Modificar função do IAM", "Modificar função do IAM");
            if (inst == null) return;
            var roles = Sim.Roles.Where(r => r.Trust == "ec2").ToList();
            Panel box;
            var f = pg.Section("Função do IAM", out box);
            f.Text("A aplicação na instância recebe credenciais TEMPORÁRIAS da função, renovadas automaticamente — sem chave de acesso fixa guardada no servidor. Só aparecem funções que confiam no serviço EC2.", fSmall, Muted);
            f.Text("Função atual: " + (inst.Role ?? "nenhuma"), fUi, Ink);
            var sel = f.Select("Escolher função do IAM", roles.Select(r => r.Name + "   (" + string.Join(", ", r.Policies) + ")").ToArray(), roles.FindIndex(r => r.Name == inst.Role));
            pg.EndSection(box, f);
            var err = pg.Error();
            string id = inst.Id;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ec2", null)), Btn("Atualizar função do IAM", true, () =>
            {
                if (sel.SelectedIndex < 0) { err.Text = "✖ Escolha a função."; return; }
                var e = Sim.AssociateRole(id, roles[sel.SelectedIndex].Name);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("ec2", null, false);
                Flash("Função " + roles[sel.SelectedIndex].Name + " associada a " + id + ".", OkGreen);
            }));
        }

        void PageEc2Image(Flow pg)
        {
            var inst = InstPage(pg, "Criar imagem", "Criar imagem");
            if (inst == null) return;
            Panel box;
            var f = pg.Section("Detalhes da imagem (AMI)", out box);
            var name = f.Field("Nome da imagem", "3 a 128 caracteres; neste laboratório, sem espaços (ex.: servidor-base).");
            f.Field("Descrição da imagem - opcional", null);
            var nr = f.Check("Sem reinicialização", false, "Por padrão a AWS reinicializa a instância para garantir que o disco fique consistente na imagem. Marque só se aceitar esse risco.");
            pg.EndSection(box, f);
            var err = pg.Error();
            string id = inst.Id;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ec2", null)), Btn("Criar imagem", true, () =>
            {
                var e = Sim.CreateImage(id, name.Text, nr.Checked);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("ec2-amis", null, false);
                Flash("Imagem " + name.Text.Trim() + " em criação a partir de " + id + ".", OkGreen);
            }));
        }

        void PageAmis(Flow pg)
        {
            PageTitle = "AMIs - EC2";
            pg.Crumbs("EC2", "Imagens", "AMIs");
            var list = Sim.Images.Where(x => x.Region == Sim.Region).ToList();
            pg.Header("Minhas AMIs (" + list.Count + ")", "Imagens criadas a partir das suas instâncias (sistema + aplicação). Para usar: Executar instância → escolha a AMI.", Btn("Executar instância", true, () => Navigate("ec2-launch", null)));
            pg.Table(new[] { "Nome da AMI", "ID da AMI", "Origem", "Criada em" }, new[] { 2f, 1.4f, 1.6f, 1.2f }, list.Select(x => new[] { x.Name, x.Id, x.Instance, x.Created.ToString("dd/MM/yyyy") }).ToList(), null);
            pg.Text("AMIs públicas da AWS: " + string.Join(" · ", AwsSim.BaseAmis.Select(a => a[1] + " (" + a[0] + ")")), fSmall, Muted);
            pg.Text("Para criar uma AMI: Instâncias → selecione → Ações → Imagem e modelos → Criar imagem.", fSmall, Muted);
        }

        void PageEips(Flow pg)
        {
            PageTitle = "IPs elásticos - EC2";
            pg.Crumbs("EC2", "Rede e segurança", "IPs elásticos");
            var list = Sim.Eips.Where(x => x.Region == Sim.Region).ToList();
            var lvRef = new ListView[1];
            var act = MenuBtn("Ações  ▾",
                "Associar endereço IP elástico", (Action)(() => { int i = SelIdx(lvRef, "um IP elástico"); if (i >= 0) Navigate("ec2-eip-assoc-edit", list[i].Alloc); }),
                "Desassociar endereço IP elástico", (Action)(() =>
                {
                    int i = SelIdx(lvRef, "um IP elástico"); if (i < 0) return;
                    if (!Confirm("Desassociar " + list[i].Ip + "? A instância perde esse IP (e ganha outro, temporário).", "Desassociar")) return;
                    Done(Sim.DisassociateEip(list[i].Alloc), "IP " + list[i].Ip + " desassociado.");
                }),
                "Liberar endereços IP elásticos", (Action)(() =>
                {
                    int i = SelIdx(lvRef, "um IP elástico"); if (i < 0) return;
                    if (!Confirm("Liberar " + list[i].Ip + "? O IP volta para a AWS e você não consegue recuperar esse mesmo endereço depois.", "Liberar IP elástico")) return;
                    Done(Sim.ReleaseEip(list[i].Alloc), "IP " + list[i].Ip + " liberado.");
                }));
            pg.Header("IPs elásticos (" + list.Count + ")", "Endereço IPv4 público fixo, que não muda ao parar/iniciar a instância. É cobrado por hora — inclusive quando NÃO está associado a nada.",
                act, Btn("Alocar endereço IP elástico", true, () => { string a; Sim.AllocateEip(out a); Flash("Endereço IP elástico alocado: " + a + ".", OkGreen); }));
            lvRef[0] = pg.Table(new[] { "Endereço IPv4", "ID de alocação", "Instância associada", "Estado" }, new[] { 1.3f, 1.6f, 2f, 1.3f },
                list.Select(x => new[] { x.Ip, x.Alloc, x.Instance != null ? (Sim.Inst(x.Instance) != null ? Sim.Inst(x.Instance).Name + " (" + x.Instance + ")" : x.Instance) : "-", x.Instance != null ? "● Em uso" : "● Sem associação (cobrando)" }).ToList(), null);
            pg.Text("Selecione o IP e use 'Ações'.", fSmall, Muted);
        }

        void PageEipAssoc(Flow pg)
        {
            var e = Sim.EipOf(arg ?? "");
            if (e == null) { Navigate("ec2-eips", null, false); return; }
            PageTitle = "Associar IP elástico - EC2";
            pg.Crumbs("EC2", "IPs elásticos", e.Alloc, "Associar");
            pg.Header("Associar endereço IP elástico", e.Ip + " (" + e.Alloc + ")");
            var insts = Sim.Instances.Where(i => i.Region == Sim.Region && i.State != "Encerrada").ToList();
            Panel box;
            var f = pg.Section("Associar a uma instância", out box);
            if (e.Instance != null) f.Text("⚠ Este IP já está associado a " + (Sim.Inst(e.Instance) != null ? Sim.Inst(e.Instance).Name : e.Instance) + ".", fBold, ErrRed);
            var sel = f.Select("Instância", insts.Select(i => i.Name + " (" + i.Id + ") · " + i.State).ToArray(), -1);
            var re = f.Check("Permitir que este endereço IP elástico seja reassociado", false, "Necessário quando o IP já está em outra instância: ele é MOVIDO (a outra perde o IP).");
            pg.EndSection(box, f);
            var err = pg.Error();
            string al = e.Alloc;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ec2-eips", null)), Btn("Associar", true, () =>
            {
                if (sel.SelectedIndex < 0) { err.Text = "✖ Escolha a instância."; return; }
                var er = Sim.AssociateEip(al, insts[sel.SelectedIndex].Id, re.Checked);
                if (er != null) { err.Text = "✖ " + er; return; }
                Navigate("ec2-eips", null, false);
                Flash("IP elástico associado a " + insts[sel.SelectedIndex].Name + ".", OkGreen);
            }));
        }

        void PageKeys(Flow pg)
        {
            PageTitle = "Pares de chaves - EC2";
            pg.Crumbs("EC2", "Rede e segurança", "Pares de chaves");
            var list = Sim.KeyPairs.Where(k => k.Region == Sim.Region).ToList();
            var lvRef = new ListView[1];
            pg.Header("Pares de chaves (" + list.Count + ")", "A AWS guarda só a chave PÚBLICA; a privada é baixada uma única vez, na criação.",
                Btn("Excluir", false, () =>
                {
                    int i = SelIdx(lvRef, "um par de chaves"); if (i < 0) return;
                    if (!Confirm("Excluir o par de chaves " + list[i].Name + "? Instâncias que já usam a chave continuam com ela, mas novas instâncias não poderão usá-la.", "Excluir par de chaves")) return;
                    Done(Sim.DeleteKeyPair(list[i].Name), "Par de chaves " + list[i].Name + " excluído.");
                }),
                Btn("Criar par de chaves", true, () => Navigate("ec2-key-create", null)));
            lvRef[0] = pg.Table(new[] { "Nome", "Tipo", "Criado em" }, new[] { 2f, 1f, 1.4f }, list.Select(k => new[] { k.Name, k.Type.ToUpperInvariant(), k.Created.ToString("dd/MM/yyyy") }).ToList(), null);
        }

        void PageKeyCreate(Flow pg)
        {
            PageTitle = "Criar par de chaves - EC2";
            pg.Crumbs("EC2", "Pares de chaves", "Criar par de chaves");
            pg.Header("Criar par de chaves", "Região: " + AwsSim.RegionName(Sim.Region));
            Panel box;
            var f = pg.Section("Par de chaves", out box);
            var name = f.Field("Nome", "Até 255 caracteres, sem espaços.");
            var type = f.Radios("Tipo de par de chaves", new[] { "RSA", "ED25519" }, 0);
            f.Text("ED25519 é mais moderno: chaves menores e mais rápidas, com segurança equivalente ou maior que RSA de 3072 bits (não funciona em instâncias Windows).", fSmall, Muted);
            f.Radios("Formato de arquivo de chave privada", new[] { ".pem (OpenSSH)", ".ppk (PuTTY)" }, 0);
            pg.EndSection(box, f);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ec2-keys", null)), Btn("Criar par de chaves", true, () =>
            {
                var e = Sim.CreateKeyPair(name.Text, type[1].Checked ? "ed25519" : "rsa");
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("ec2-keys", null, false);
                Flash("Par de chaves criado. O arquivo " + name.Text.Trim() + ".pem foi baixado — guarde-o com chmod 400; a AWS não guarda cópia.", OkGreen);
            }));
        }

        void PageVolEdit(Flow pg)
        {
            var v = Sim.Vol(arg ?? "");
            if (v == null) { Navigate("ec2-volumes", null, false); return; }
            PageTitle = "Modificar volume - EC2";
            pg.Crumbs("EC2", "Volumes", v.Id, "Modificar volume");
            var inst = v.Instance != null ? Sim.Inst(v.Instance) : null;
            pg.Header("Modificar volume", v.Id + " · " + v.Size + " GiB " + v.Type + " · " + (inst != null ? "anexado a " + inst.Name : "não anexado"));
            Panel box;
            var f = pg.Section("Detalhes do volume", out box);
            f.Text("Com o Elastic Volumes, tipo e tamanho mudam com o volume EM USO, sem parar a instância. O tamanho só pode aumentar.", fSmall, Muted);
            var desc = new Dictionary<string, string> { { "gp3", "SSD de uso geral (gp3) — ~20% mais barato que o gp2, 3000 IOPS de base" }, { "gp2", "SSD de uso geral (gp2) — geração anterior" }, { "io2", "SSD de IOPS provisionadas (io2) — bancos críticos, bem mais caro" }, { "st1", "HDD otimizado para throughput (st1)" }, { "sc1", "HDD frio (sc1) — acesso raro" } };
            var type = f.Select("Tipo de volume", AwsSim.VolumeTypes.Select(t => desc[t]).ToArray(), Array.IndexOf(AwsSim.VolumeTypes, v.Type));
            var size = f.Field("Tamanho (GiB)", null, v.Size.ToString());
            pg.EndSection(box, f);
            var err = pg.Error();
            string vid = v.Id;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ec2-volumes", null)), Btn("Modificar", true, () =>
            {
                int sz;
                if (!int.TryParse(size.Text.Trim(), out sz)) { err.Text = "✖ O tamanho precisa ser um número (GiB)."; return; }
                var e = Sim.ModifyVolume(vid, AwsSim.VolumeTypes[Math.Max(0, type.SelectedIndex)], sz);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("ec2-volumes", null, false);
                Flash("Volume " + vid + " em modificação (otimizando).", OkGreen);
            }));
        }

        void PageSgCreate(Flow pg)
        {
            PageTitle = "Criar grupo de segurança - EC2";
            pg.Crumbs("EC2", "Grupos de segurança", "Criar grupo de segurança");
            pg.Header("Criar grupo de segurança", "Região: " + AwsSim.RegionName(Sim.Region));
            var vpcs = Sim.Vpcs.Where(x => x.Region == Sim.Region).ToList();
            Panel box;
            var f = pg.Section("Detalhes básicos", out box);
            var name = f.Field("Nome do grupo de segurança", "Não pode ser alterado depois.");
            var desc = f.Field("Descrição", "Obrigatória: diga para que serve o grupo.");
            var vpc = f.Select("VPC", vpcs.Select(x => x.Name + " (" + x.Id + ")").ToArray(), 0);
            pg.EndSection(box, f);
            var g = pg.Section("Regras de entrada (opcional: dá para adicionar depois)", out box);
            var rt = g.Select("Tipo", new[] { "(nenhuma regra agora)" }.Concat(AwsSim.RuleTypes.Select(r => r[0] + "  ·  TCP " + r[1])).ToArray(), 0);
            var src = g.Field("Origem", "Bloco CIDR (0.0.0.0/0 = qualquer lugar; x.x.x.x/32 = um IP) ou ID de outro grupo (sg-...).");
            g.Text("Regras de saída: por padrão, todo o tráfego de saída é permitido.", fSmall, Muted);
            pg.EndSection(box, g);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ec2-sg", null)), Btn("Criar grupo de segurança", true, () =>
            {
                if (rt.SelectedIndex > 0 && src.Text.Trim().Length == 0) { err.Text = "✖ Informe a origem da regra (ou escolha 'nenhuma regra agora')."; return; }
                string id;
                var e = Sim.CreateSg(name.Text, desc.Text, vpcs.Count > 0 ? vpcs[Math.Max(0, vpc.SelectedIndex)].Id : null, out id);
                if (e != null) { err.Text = "✖ " + e; return; }
                string extra = "";
                if (rt.SelectedIndex > 0)
                {
                    var er = Sim.Authorize(id, int.Parse(AwsSim.RuleTypes[rt.SelectedIndex - 1][1]), src.Text.Trim());
                    if (er != null) extra = " (regra NÃO criada: " + er + ")";
                }
                Navigate("ec2-sg-rules", id, false);
                Flash("Grupo de segurança " + name.Text.Trim() + " criado (" + id + ")." + extra, extra.Length > 0 ? ErrRed : OkGreen);
            }));
        }

        void PageAsgs(Flow pg)
        {
            PageTitle = "Grupos do Auto Scaling - EC2";
            pg.Crumbs("EC2", "Auto Scaling", "Grupos do Auto Scaling");
            var list = Sim.Asgs.ToList();
            pg.Header("Grupos do Auto Scaling (" + list.Count + ")", "Mantêm a quantidade certa de instâncias: repõem as que falham e escalam conforme a carga (entre a capacidade mínima e a máxima).");
            pg.Table(new[] { "Nome", "Instâncias", "Desejada", "Mín.", "Máx.", "Políticas" }, new[] { 1.6f, 1f, 1f, 0.8f, 0.8f, 1.6f },
                list.Select(a => new[] { a.Name, a.Desired.ToString(), a.Desired.ToString(), a.Min.ToString(), a.Max.ToString(), a.Policies.Count == 0 ? "nenhuma" : string.Join(", ", a.Policies.Select(p => p[0])) }).ToList(),
                i => Navigate("ec2-asg-detail", list[i].Name));
            pg.Text("Clique no nome do grupo para ver detalhes e editar.", fSmall, Muted);
        }

        void PageAsg(Flow pg)
        {
            var a = Sim.Group(arg ?? "");
            if (a == null) { Navigate("ec2-asg", null, false); return; }
            PageTitle = a.Name + " - Auto Scaling";
            pg.Crumbs("EC2", "Grupos do Auto Scaling", a.Name);
            pg.Header(a.Name, "Modelo de execução: web-cafe365 (AMI web-golden-2026-10, t3.small) · zonas us-east-1a e us-east-1b");
            string an = a.Name;
            Panel box;
            var f = pg.Section("Detalhes do grupo: capacidade", out box);
            f.Text("Capacidade desejada: " + a.Desired + "   ·   Mínima: " + a.Min + "   ·   Máxima: " + a.Max, fBold, Ink);
            f.Text("Desejada = quantas instâncias rodar agora. As políticas de escalabilidade mexem na desejada, sempre entre a mínima e a máxima.", fSmall, Muted);
            f.Buttons(Btn("Editar", false, () => Navigate("ec2-asg-edit", an)));
            pg.EndSection(box, f);
            var pols = a.Policies.ToList();
            var lvRef = new ListView[1];
            pg.SubHeader("Escalabilidade automática: políticas dinâmicas (" + pols.Count + ")",
                Btn("Excluir", false, () =>
                {
                    int i = SelIdx(lvRef, "uma política"); if (i < 0) return;
                    Done(Sim.DeleteScalingPolicy(an, pols[i][0]), "Política " + pols[i][0] + " excluída.");
                }),
                Btn("Criar política de escalabilidade dinâmica", true, () => Navigate("ec2-asg-policy-create", an)));
            lvRef[0] = pg.Table(new[] { "Nome da política", "Tipo", "Métrica", "Valor de destino" }, new[] { 1.3f, 1.6f, 2f, 1f }, pols.Select(p => new[] { p[0], p[1] == "TargetTrackingScaling" ? "Rastreamento de destino" : p[1], MetricName(p[2]), p[3] }).ToList(), null);
        }

        static readonly string[][] AsgMetrics =
        {
            new[] { "ASGAverageCPUUtilization", "Utilização média da CPU (%)" },
            new[] { "ASGAverageNetworkIn", "Média de bytes de entrada da rede" },
            new[] { "ASGAverageNetworkOut", "Média de bytes de saída da rede" },
            new[] { "ALBRequestCountPerTarget", "Contagem de solicitações do Application Load Balancer por destino" },
        };
        static string MetricName(string id) { var m = AsgMetrics.FirstOrDefault(x => x[0] == id); return m != null ? m[1] : id; }

        void PageAsgEdit(Flow pg)
        {
            var a = Sim.Group(arg ?? "");
            if (a == null) { Navigate("ec2-asg", null, false); return; }
            PageTitle = "Editar " + a.Name + " - Auto Scaling";
            pg.Crumbs("EC2", "Grupos do Auto Scaling", a.Name, "Editar");
            pg.Header("Editar " + a.Name, null);
            Panel box;
            var f = pg.Section("Tamanho do grupo", out box);
            var d = f.Field("Capacidade desejada", null, a.Desired.ToString());
            var mn = f.Field("Capacidade mínima", "O grupo nunca fica com menos instâncias que isso.", a.Min.ToString());
            var mx = f.Field("Capacidade máxima", "Teto de instâncias (e de custo).", a.Max.ToString());
            pg.EndSection(box, f);
            var err = pg.Error();
            string an = a.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ec2-asg-detail", an)), Btn("Atualizar", true, () =>
            {
                int dv, mnv, mxv;
                if (!int.TryParse(d.Text.Trim(), out dv) || !int.TryParse(mn.Text.Trim(), out mnv) || !int.TryParse(mx.Text.Trim(), out mxv)) { err.Text = "✖ Use números inteiros."; return; }
                var e = Sim.SetCapacity(an, mnv, mxv, dv);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("ec2-asg-detail", an, false);
                Flash("Grupo " + an + " atualizado: desejada " + dv + ", mínima " + mnv + ", máxima " + mxv + ".", OkGreen);
            }));
        }

        void PageAsgPolicy(Flow pg)
        {
            var a = Sim.Group(arg ?? "");
            if (a == null) { Navigate("ec2-asg", null, false); return; }
            PageTitle = "Criar política - Auto Scaling";
            pg.Crumbs("EC2", "Grupos do Auto Scaling", a.Name, "Criar política de escalabilidade dinâmica");
            pg.Header("Criar política de escalabilidade dinâmica", "Grupo: " + a.Name);
            Panel box;
            var f = pg.Section("Política", out box);
            var type = f.Radios("Tipo de política", new[] { "Rastreamento de destino", "Escalabilidade em etapas", "Escalabilidade simples" }, 0);
            f.Text("Rastreamento de destino funciona como um termostato: você escolhe a métrica e o valor que quer manter, e o Auto Scaling cria os alarmes e ajusta a capacidade sozinho.", fSmall, Muted);
            var name = f.Field("Nome da política", null);
            var metric = f.Select("Tipo de métrica", AsgMetrics.Select(m => m[1]).ToArray(), 0);
            var target = f.Field("Valor de destino", "Para CPU, em % (ex.: 50 deixa folga para picos enquanto as novas instâncias sobem).");
            pg.EndSection(box, f);
            var err = pg.Error();
            string an = a.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ec2-asg-detail", an)), Btn("Criar", true, () =>
            {
                if (!type[0].Checked) { err.Text = "✖ Neste laboratório use 'Rastreamento de destino' (as outras exigem criar os alarmes do CloudWatch à mão)."; return; }
                float tv;
                if (!float.TryParse(target.Text.Trim().Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out tv)) { err.Text = "✖ O valor de destino precisa ser um número."; return; }
                var e = Sim.PutScalingPolicy(an, name.Text, "TargetTrackingScaling", AsgMetrics[Math.Max(0, metric.SelectedIndex)][0], tv);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("ec2-asg-detail", an, false);
                Flash("Política " + name.Text.Trim() + " criada.", OkGreen);
            }));
        }

        void PageEc2Settings(Flow pg)
        {
            PageTitle = "Configurações da conta - EC2";
            pg.Crumbs("EC2", "Painel", "Configurações da conta");
            pg.Header("Configurações da conta (EC2)", "Valem para a região selecionada: " + AwsSim.RegionName(Sim.Region) + " (" + Sim.Region + ").");
            Panel box;
            var f = pg.Section("Proteção e segurança de dados: criptografia do EBS", out box);
            bool on = Sim.EbsDefaultOn(Sim.Region);
            f.Text("Sempre criptografar novos volumes do EBS: " + (on ? "● Ativado" : "● Desativado") + "   (região " + Sim.Region + ")", fBold, on ? OkGreen : ErrRed);
            f.Text("Vale só para volumes e snapshots NOVOS desta região; os que já existem não mudam. Chave padrão: alias/aws/ebs.", fSmall, Muted);
            f.Buttons(Btn("Gerenciar", false, () => Navigate("ec2-ebsenc-edit", null)));
            pg.EndSection(box, f);
        }

        void PageEbsEnc(Flow pg)
        {
            PageTitle = "Criptografia do EBS - EC2";
            pg.Crumbs("EC2", "Configurações da conta", "Gerenciar criptografia do EBS");
            pg.Header("Gerenciar a criptografia padrão do EBS", "Região: " + AwsSim.RegionName(Sim.Region) + " (" + Sim.Region + ")");
            Panel box;
            var f = pg.Section("Criptografia do EBS", out box);
            var on = f.Check("Sempre criptografar novos volumes do EBS: Ativar", Sim.EbsDefaultOn(Sim.Region), "A configuração é por região: repita em cada região usada pela empresa.");
            f.Text("Chave KMS padrão: alias/aws/ebs (gerenciada pela AWS).", fSmall, Muted);
            pg.EndSection(box, f);
            var err = pg.Error();
            string rg = Sim.Region;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ec2-settings", null)), Btn("Atualizar criptografia do EBS", true, () =>
            {
                var e = Sim.SetEbsDefault(rg, on.Checked);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("ec2-settings", null, false);
                Flash("Criptografia padrão do EBS em " + rg + ": " + (on.Checked ? "ativada" : "desativada") + ".", OkGreen);
            }));
        }

        // ── S3
        AwsSim.Bucket BucketPage(Flow pg, string title)
        {
            var b = Sim.Bkt(arg ?? "");
            if (b == null) { Navigate("s3", null, false); return null; }
            PageTitle = title + " - S3";
            pg.Crumbs("Amazon S3", "Buckets", b.Name, title);
            pg.Header(title, "Bucket: " + b.Name);
            return b;
        }

        void PageS3Tags(Flow pg)
        {
            var b = BucketPage(pg, "Editar tags do bucket");
            if (b == null) return;
            Panel box;
            var f = pg.Section("Tags", out box);
            var rows = KvEditor(f, box, b.Tags.ToList(), 2, "Chave", "Valor - opcional");
            pg.EndSection(box, f);
            var err = pg.Error();
            string bn = b.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("s3-bucket", bn)), Btn("Salvar alterações", true, () =>
            {
                string er;
                var d = KvRead(rows, out er);
                if (er != null) { err.Text = "✖ " + er; return; }
                var e = Sim.SetBucketTags(bn, d);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("s3-bucket", bn, false);
                Flash("Tags do bucket " + bn + " salvas.", OkGreen);
            }));
        }

        void PageS3Website(Flow pg)
        {
            var b = BucketPage(pg, "Editar hospedagem de site estático");
            if (b == null) return;
            Panel box;
            var f = pg.Section("Hospedagem de site estático", out box);
            var on = f.Radios(null, new[] { "Desativar", "Ativar" }, b.IndexDoc != null ? 1 : 0);
            var idx = f.Field("Documento de índice", "Página inicial (padrão) do site, ex.: index.html.", b.IndexDoc ?? "");
            var er2 = f.Field("Documento de erro - opcional", "Página mostrada em caso de erro 4XX, ex.: erro.html.", b.ErrorDoc ?? "");
            pg.EndSection(box, f);
            var err = pg.Error();
            string bn = b.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("s3-bucket", bn)), Btn("Salvar alterações", true, () =>
            {
                if (on[1].Checked && idx.Text.Trim().Length == 0) { err.Text = "✖ Informe o documento de índice."; return; }
                var e = Sim.SetWebsite(bn, on[1].Checked ? idx.Text : null, er2.Text);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("s3-bucket", bn, false);
                Flash("Hospedagem de site estático " + (on[1].Checked ? "ativada" : "desativada") + " em " + bn + ".", OkGreen);
            }));
        }

        void PageS3Bpa(Flow pg)
        {
            var b = BucketPage(pg, "Editar bloqueio de acesso público");
            if (b == null) return;
            Panel box;
            var f = pg.Section("Bloquear acesso público (configurações do bucket)", out box);
            var on = f.Check("Bloquear todo o acesso público", b.BlockPublic, "Ativar essa configuração é o mesmo que ativar as quatro abaixo: bloquear ACLs públicas novas e existentes, e políticas públicas novas e existentes.");
            f.Text("   ☐ Bloquear ACLs novas  ·  ☐ Ignorar ACLs existentes  ·  ☐ Bloquear políticas públicas novas  ·  ☐ Restringir acesso de políticas públicas existentes", fSmall, Muted);
            pg.EndSection(box, f);
            var err = pg.Error();
            string bn = b.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("s3-bucket", bn)), Btn("Salvar alterações", true, () =>
            {
                if (!Confirm((on.Checked ? "Ativar" : "DESATIVAR") + " o bloqueio de acesso público do bucket " + bn + "?" + (on.Checked ? " Aplicações que dependem de acesso público a este bucket podem parar de funcionar." : " Os objetos poderão ficar públicos na internet."), "Confirmar")) return;
                var e = Sim.SetBlockPublic(bn, on.Checked);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("s3-bucket", bn, false);
                Flash("Bloqueio de acesso público " + (on.Checked ? "ativado" : "desativado") + " em " + bn + ".", OkGreen);
            }));
        }

        void PageS3Policy(Flow pg)
        {
            var b = BucketPage(pg, "Editar política do bucket");
            if (b == null) return;
            Panel box;
            var f = pg.Section("Política", out box);
            f.Text("ARN do bucket: arn:aws:s3:::" + b.Name, fSmall, Muted);
            var files = AwsSim.LocalJson.Where(j => j[0].StartsWith("politica-")).ToList();
            var ex = f.Select("Inserir exemplo (arquivos do CloudShell)", new[] { "(escolha para carregar no editor)" }.Concat(files.Select(j => j[0])).ToArray(), 0);
            var tb = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Font = fMono, Location = new Point(f.X, f.Y), Width = f.W, Height = S(190), AcceptsReturn = true, Text = b.PolicyText != null ? AwsSim.PrettyJson(b.PolicyText).Replace("\n", "\r\n") : "" };
            box.Controls.Add(tb);
            f.Y += tb.Height + S(10);
            ex.SelectedIndexChanged += (s, e) => { if (ex.SelectedIndex > 0) tb.Text = AwsSim.PrettyJson(files[ex.SelectedIndex - 1][1]).Replace("\n", "\r\n"); };
            f.Text("A política inteira é substituída ao salvar. Leia o JSON antes: Effect (Allow/Deny), Principal (quem), Action (o quê), Resource (onde) e Condition.", fSmall, Muted);
            pg.EndSection(box, f);
            var err = pg.Error();
            string bn = b.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("s3-bucket", bn)), Btn("Salvar alterações", true, () =>
            {
                if (tb.Text.Trim().Length == 0) { err.Text = "✖ O editor está vazio. (Para remover a política, use 'Excluir' na aba Permissões.)"; return; }
                var e = Sim.SetPolicy(bn, tb.Text.Replace("\r\n", "\n"));
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("s3-bucket", bn, false);
                Flash("Política do bucket " + bn + " salva.", OkGreen);
            }));
        }

        void PageS3Lifecycle(Flow pg)
        {
            var b = BucketPage(pg, "Criar regra de ciclo de vida");
            if (b == null) return;
            Panel box;
            var f = pg.Section("Configuração da regra de ciclo de vida", out box);
            var name = f.Field("Nome da regra de ciclo de vida", "Até 255 caracteres.");
            var scope = f.Radios("Escolher o escopo da regra", new[] { "Limitar o escopo desta regra usando um filtro (prefixo)", "Aplicar a todos os objetos do bucket" }, 0);
            var pre = f.Field("Prefixo", "Ex.: logs/ — só objetos cujo nome começa com isso.");
            pg.EndSection(box, f);
            var g = pg.Section("Ações da regra de ciclo de vida", out box);
            var gl = g.Check("Mover versões atuais dos objetos para o Glacier Flexible Retrieval", false);
            var glD = g.Field("   Dias após a criação do objeto", null);
            var ex = g.Check("Expirar versões atuais dos objetos", false, "Expirar = excluir. Em bucket versionado, cria um marcador de exclusão (a versão vira 'desatualizada').");
            var exD = g.Field("   Dias após a criação do objeto", null);
            var nc = g.Check("Excluir permanentemente versões desatualizadas (não atuais) dos objetos", false, "Só faz sentido com versionamento: apaga as versões antigas, mantendo a atual.");
            var ncD = g.Field("   Dias após os objetos se tornarem desatualizados", null);
            pg.EndSection(box, g);
            var err = pg.Error();
            string bn = b.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("s3-bucket", bn)), Btn("Criar regra", true, () =>
            {
                if (scope[0].Checked && pre.Text.Trim().Length == 0) { err.Text = "✖ Informe o prefixo (ou escolha 'Aplicar a todos os objetos do bucket')."; return; }
                var r = new AwsSim.LcRule { Id = name.Text.Trim(), Prefix = scope[0].Checked ? pre.Text.Trim() : "" };
                if (gl.Checked) { r.Glacier = ToInt(glD.Text, -1); if (r.Glacier < 0) { err.Text = "✖ Informe os dias para mover ao Glacier."; return; } }
                if (ex.Checked) { r.Expire = ToInt(exD.Text, -1); if (r.Expire < 1) { err.Text = "✖ Informe os dias para expirar (1 ou mais)."; return; } }
                if (nc.Checked) { r.Noncurrent = ToInt(ncD.Text, -1); if (r.Noncurrent < 1) { err.Text = "✖ Informe os dias para excluir as versões desatualizadas (1 ou mais)."; return; } }
                var e = Sim.PutLifecycle(bn, new List<AwsSim.LcRule> { r }, false);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("s3-bucket", bn, false);
                Flash("Regra de ciclo de vida " + r.Id + " criada.", OkGreen);
            }));
        }

        void PageS3Presign(Flow pg)
        {
            var parts = (arg ?? "").Split('|');
            var b = parts.Length == 2 ? Sim.Bkt(parts[0]) : null;
            if (b == null) { Navigate("s3", null, false); return; }
            string key = parts[1], bn = b.Name;
            PageTitle = "Compartilhar com URL pré-assinada - S3";
            pg.Crumbs("Amazon S3", "Buckets", bn, key, "URL pré-assinada");
            pg.Header("Compartilhar " + key + " com uma URL pré-assinada", "Qualquer pessoa com a URL baixa o objeto até ela expirar — sem conta na AWS e sem deixar o bucket público.");
            Panel box;
            var f = pg.Section("Intervalo de tempo até a URL pré-assinada expirar", out box);
            var n = f.Field("Número", null);
            var unit = f.Radios("Unidade", new[] { "Minutos", "Horas" }, 0);
            f.Text("Máximo: 12 horas pelo console (7 dias pela CLI). Quanto menor, menor o risco se a URL vazar.", fSmall, Muted);
            pg.EndSection(box, f);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("s3-bucket", bn)), Btn("Criar URL pré-assinada", true, () =>
            {
                int v = ToInt(n.Text, -1);
                if (v < 1) { err.Text = "✖ Informe um número inteiro maior que zero."; return; }
                int secs = v * (unit[1].Checked ? 3600 : 60);
                if (secs > 12 * 3600) { err.Text = "✖ Pelo console, o máximo é 12 horas."; return; }
                string url;
                var e = Sim.Presign(bn, key, secs, out url);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("s3-bucket", bn, false);
                Flash("URL pré-assinada (válida por " + v + " " + (unit[1].Checked ? "hora(s)" : "minuto(s)") + "): " + url, OkGreen);
            }));
        }

        void PageS3Versions(Flow pg)
        {
            var b = Sim.Bkt(arg ?? "");
            if (b == null) { Navigate("s3", null, false); return; }
            string bn = b.Name;
            PageTitle = b.Name + " (versões) - S3";
            pg.Crumbs("Amazon S3", "Buckets", bn, "Versões");
            pg.Header(bn + " — mostrando versões", "Versionamento: " + b.Versioning + ". Um objeto excluído num bucket versionado ganha um 'marcador de exclusão'; excluir o MARCADOR traz o objeto de volta.");
            var rows = new List<string[]>();
            var refs = new List<string[]>();   // chave, versão, tipo
            foreach (var o in b.Objects) { rows.Add(new[] { o, "Objeto (versão atual)", AwsSim.VersionOf(bn, o) }); refs.Add(new[] { o, AwsSim.VersionOf(bn, o), "obj" }); }
            foreach (var m in b.Markers.Where(m => !b.Objects.Contains(m.Key))) { rows.Add(new[] { m.Key, "Marcador de exclusão", m.VersionId }); refs.Add(new[] { m.Key, m.VersionId, "marker" }); }
            var lvRef = new ListView[1];
            pg.SubHeader("Objetos e versões (" + rows.Count + ")",
                Btn("Ocultar versões", false, () => { s3Tab = "Objetos"; Navigate("s3-bucket", bn); }),
                Btn("Excluir", false, () =>
                {
                    int i = SelIdx(lvRef, "uma versão"); if (i < 0) return;
                    var r = refs[i];
                    string msg = r[2] == "marker" ? "Excluir o marcador de exclusão de " + r[0] + "? A versão anterior volta a ser a atual (o objeto reaparece)." : "Excluir PERMANENTEMENTE a versão " + r[1] + " de " + r[0] + "? Isso não pode ser desfeito.";
                    if (!Confirm(msg, "Excluir versão")) return;
                    Done(Sim.DeleteVersion(bn, r[0], r[1]), r[2] == "marker" ? "Marcador removido: " + r[0] + " restaurado." : "Versão excluída permanentemente.");
                }));
            lvRef[0] = pg.Table(new[] { "Nome", "Tipo", "ID da versão" }, new[] { 2f, 1.6f, 2f }, rows, null);
        }

        // ── IAM: funções (roles), configurações da conta
        static string TrustText(string t) { return t == "ec2" ? "Serviço da AWS: ec2" : t == "lambda" ? "Serviço da AWS: lambda" : "Serviço da AWS: ecs-tasks"; }

        void PageRoles(Flow pg)
        {
            PageTitle = "Funções - IAM";
            pg.Crumbs("IAM", "Funções");
            var list = Sim.Roles.ToList();
            var lvRef = new ListView[1];
            pg.Header("Funções (" + list.Count + ")", "Uma função (role) é uma identidade SEM senha nem chave fixa: serviços (EC2, Lambda...) assumem a função e recebem credenciais temporárias.",
                Btn("Excluir", false, () =>
                {
                    int i = SelIdx(lvRef, "uma função"); if (i < 0) return;
                    if (!Confirm("Excluir a função " + list[i].Name + "? Quem usa a função perde as permissões.", "Excluir função")) return;
                    Done(Sim.DeleteRole(list[i].Name), "Função " + list[i].Name + " excluída.");
                }),
                Btn("Criar função", true, () => Navigate("iam-role-create", null)));
            lvRef[0] = pg.Table(new[] { "Nome da função", "Entidades confiáveis", "Políticas" }, new[] { 1.6f, 1.6f, 3f }, list.Select(r => new[] { r.Name, TrustText(r.Trust), r.Policies.Count == 0 ? "-" : string.Join(", ", r.Policies) }).ToList(), i => Navigate("iam-role", list[i].Name));
        }

        void PageRole(Flow pg)
        {
            var r = Sim.Rl(arg ?? "");
            if (r == null) { Navigate("iam-roles", null, false); return; }
            PageTitle = r.Name + " - IAM";
            pg.Crumbs("IAM", "Funções", r.Name);
            pg.Header(r.Name, "ARN: arn:aws:iam::123456789012:role/" + r.Name);
            Panel box;
            string rn = r.Name;
            var t = pg.Section("Relações de confiança", out box);
            t.Text("Entidade confiável: " + TrustText(r.Trust) + " (" + r.Trust + ".amazonaws.com pode assumir esta função — sts:AssumeRole)", fUi, Ink);
            pg.EndSection(box, t);
            var pols = r.Policies.ToList();
            var lvRef = new ListView[1];
            var f = pg.Section("Políticas de permissões (" + pols.Count + ")", out box);
            if (pols.Count == 0) f.Text("Nenhuma política anexada.", fUi, Muted);
            else lvRef[0] = f.Table(new[] { "Nome da política", "Descrição" }, new[] { 1.4f, 2.6f }, pols.Select(p => new[] { p, (AwsSim.ManagedPolicies.FirstOrDefault(x => x[0] == p) ?? new[] { p, "" })[1] }).ToList(), null);
            f.Buttons(Btn("Remover", false, () => { int i = SelIdx(lvRef, "uma política"); if (i >= 0) Done(Sim.DetachRolePolicy(rn, pols[i]), "Política " + pols[i] + " removida de " + rn + "."); }),
                      Btn("Adicionar permissões", true, () => Navigate("iam-role-attach", rn)));
            pg.EndSection(box, f);
        }

        void PageRoleCreate(Flow pg)
        {
            PageTitle = "Criar função - IAM";
            pg.Crumbs("IAM", "Funções", "Criar função");
            pg.Header("Criar função", null);
            Panel box;
            var f = pg.Section("Etapa 1: selecionar entidade confiável", out box);
            f.Radios("Tipo de entidade confiável", new[] { "Serviço da AWS" }, 0);
            var uc = f.Select("Caso de uso (quem vai assumir a função)", new[] { "EC2 — permite que instâncias EC2 chamem serviços da AWS", "Lambda — permite que funções Lambda chamem serviços da AWS", "Elastic Container Service Task — permite que tarefas do ECS chamem serviços da AWS" }, -1);
            pg.EndSection(box, f);
            var g = pg.Section("Etapa 2: adicionar permissões (políticas gerenciadas pela AWS)", out box);
            var checks = AwsSim.ManagedPolicies.Select(p => g.Check(p[0] + "  —  " + p[1], false)).ToList();
            pg.EndSection(box, g);
            var h = pg.Section("Etapa 3: nomear, revisar e criar", out box);
            var name = h.Field("Nome da função", "Até 64 caracteres: letras, números e + = , . @ _ -");
            pg.EndSection(box, h);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("iam-roles", null)), Btn("Criar função", true, () =>
            {
                if (uc.SelectedIndex < 0) { err.Text = "✖ Escolha o caso de uso (quem vai assumir a função)."; return; }
                string trust = new[] { "ec2", "lambda", "ecs-tasks" }[uc.SelectedIndex];
                var e = Sim.CreateRole(name.Text, trust);
                if (e != null) { err.Text = "✖ " + e; return; }
                string rn = name.Text.Trim();
                for (int i = 0; i < checks.Count; i++) if (checks[i].Checked) Sim.AttachRolePolicy(rn, AwsSim.ManagedPolicies[i][0]);
                Navigate("iam-roles", null, false);
                Flash("Função " + rn + " criada.", OkGreen);
            }));
        }

        void PageRoleAttach(Flow pg)
        {
            var r = Sim.Rl(arg ?? "");
            if (r == null) { Navigate("iam-roles", null, false); return; }
            PageTitle = "Adicionar permissões - IAM";
            pg.Crumbs("IAM", "Funções", r.Name, "Adicionar permissões");
            pg.Header("Adicionar permissões", "Função: " + r.Name);
            Panel box;
            var f = pg.Section("Políticas de permissões (gerenciadas pela AWS)", out box);
            var rad = f.Radios(null, AwsSim.ManagedPolicies.Select(p => p[0] + "  —  " + p[1]).ToArray(), -1);
            pg.EndSection(box, f);
            var err = pg.Error();
            string rn = r.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("iam-role", rn)), Btn("Adicionar permissões", true, () =>
            {
                int i = rad.FindIndex(x => x.Checked);
                if (i < 0) { err.Text = "✖ Escolha uma política."; return; }
                var e = Sim.AttachRolePolicy(rn, AwsSim.ManagedPolicies[i][0]);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("iam-role", rn, false);
                Flash("Política " + AwsSim.ManagedPolicies[i][0] + " anexada a " + rn + ".", OkGreen);
            }));
        }

        void PageIamSettings(Flow pg)
        {
            PageTitle = "Configurações da conta - IAM";
            pg.Crumbs("IAM", "Configurações da conta");
            pg.Header("Configurações da conta", null);
            Panel box;
            var f = pg.Section("Política de senha", out box);
            bool custom = Sim.PwMin != 8 || Sim.PwUpper || Sim.PwLower || Sim.PwNumbers || Sim.PwSymbols || Sim.PwMaxAge > 0;
            f.Text(custom ? "Política personalizada" : "Padrão do IAM (mínimo 8 caracteres, sem outras exigências)", fBold, Ink);
            f.Text("Comprimento mínimo: " + Sim.PwMin + " · Maiúscula: " + (Sim.PwUpper ? "sim" : "não") + " · Minúscula: " + (Sim.PwLower ? "sim" : "não") + " · Número: " + (Sim.PwNumbers ? "sim" : "não") + " · Símbolo: " + (Sim.PwSymbols ? "sim" : "não") + " · Expiração: " + (Sim.PwMaxAge > 0 ? Sim.PwMaxAge + " dias" : "nunca"), fUi, Ink);
            f.Text("Vale para as senhas de login no console dos usuários IAM.", fSmall, Muted);
            f.Buttons(Btn("Editar", false, () => Navigate("iam-pwpolicy-edit", null)));
            pg.EndSection(box, f);
        }

        void PagePwPolicy(Flow pg)
        {
            PageTitle = "Editar política de senha - IAM";
            pg.Crumbs("IAM", "Configurações da conta", "Editar política de senha");
            pg.Header("Editar política de senha", null);
            Panel box;
            var f = pg.Section("Política de senha", out box);
            var mode = f.Radios(null, new[] { "Padrão do IAM", "Personalizada" }, 1);
            var min = f.Field("Comprimento mínimo da senha", "Entre 6 e 128 caracteres.", Sim.PwMin.ToString());
            var up = f.Check("Exigir pelo menos uma letra maiúscula (A–Z)", Sim.PwUpper);
            var low = f.Check("Exigir pelo menos uma letra minúscula (a–z)", Sim.PwLower);
            var num = f.Check("Exigir pelo menos um número (0–9)", Sim.PwNumbers);
            var sym = f.Check("Exigir pelo menos um caractere não alfanumérico (! @ # $ % ...)", Sim.PwSymbols);
            var exp = f.Check("Ativar a expiração de senha", Sim.PwMaxAge > 0, "Força a troca periódica (recomendações atuais, como a do NIST, desaconselham trocas forçadas sem motivo).");
            var days = f.Field("Expirar senhas em (dias)", null, Sim.PwMaxAge > 0 ? Sim.PwMaxAge.ToString() : "90");
            pg.EndSection(box, f);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("iam-settings", null)), Btn("Salvar alterações", true, () =>
            {
                string e;
                if (mode[0].Checked) e = Sim.SetPasswordPolicy(8, false, false, false, false, 0);
                else
                {
                    int m = ToInt(min.Text, -1), d = exp.Checked ? ToInt(days.Text, -1) : 0;
                    if (m < 0 || d < 0) { err.Text = "✖ Use números inteiros."; return; }
                    e = Sim.SetPasswordPolicy(m, up.Checked, low.Checked, num.Checked, sym.Checked, d);
                }
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("iam-settings", null, false);
                Flash("Política de senha atualizada.", OkGreen);
            }));
        }

        // ── Lambda: aba Configuração e formulários
        void PageLambdaConfig(Flow pg, AwsSim.Fn fn)
        {
            Panel box;
            string fnm = fn.Name;
            var g = pg.Section("Configuração geral", out box);
            g.Text("Memória: " + fn.Memory + " MB   ·   Tempo limite: " + FmtTimeout(fn.Timeout) + "   ·   Armazenamento temporário: 512 MB", fBold, Ink);
            g.Text("A CPU é proporcional à memória. Se a execução passar do tempo limite, a Lambda é interrompida (Task timed out).", fSmall, Muted);
            g.Buttons(Btn("Editar", false, () => Navigate("lambda-general-edit", fnm)));
            pg.EndSection(box, g);
            var e = pg.Section("Variáveis de ambiente (" + fn.Env.Count + ")", out box);
            if (fn.Env.Count == 0) e.Text("Nenhuma variável de ambiente.", fUi, Muted);
            else e.Table(new[] { "Chave", "Valor" }, new[] { 1f, 1.6f }, fn.Env.Select(kv => new[] { kv.Key, kv.Value }).ToList(), null);
            e.Text("Ficam visíveis para quem lê a configuração da função: segredos vão no Secrets Manager (guarde aqui só o ID do segredo).", fSmall, Muted);
            e.Buttons(Btn("Editar", false, () => Navigate("lambda-env-edit", fnm)));
            pg.EndSection(box, e);
            var c = pg.Section("Simultaneidade", out box);
            c.Text(fn.Concurrency < 0 ? "Usando a simultaneidade não reservada da conta (até 1000 execuções ao mesmo tempo, divididas com as outras funções)." : "Simultaneidade reservada: " + fn.Concurrency + " (no máximo " + fn.Concurrency + " execuções ao mesmo tempo; as demais são limitadas — throttling)", fBold, Ink);
            c.Buttons(Btn("Editar", false, () => Navigate("lambda-concurrency-edit", fnm)));
            pg.EndSection(box, c);
            var t = pg.Section("Gatilhos (" + fn.Triggers.Count + ")", out box);
            if (fn.Triggers.Count == 0) t.Text("Nenhum gatilho.", fUi, Muted);
            else t.Table(new[] { "Origem", "Fila", "Tamanho do lote" }, new[] { 1f, 2f, 1f }, fn.Triggers.Select(x => new[] { "SQS", x[0], x[1] }).ToList(), null);
            t.Buttons(Btn("Adicionar gatilho", false, () => Navigate("lambda-trigger-create", fnm)));
            pg.EndSection(box, t);
        }

        AwsSim.Fn FnPage(Flow pg, string title)
        {
            var fn = Sim.Func(arg ?? "");
            if (fn == null) { Navigate("lambda", null, false); return null; }
            PageTitle = title + " - Lambda";
            pg.Crumbs("Lambda", "Funções", fn.Name, title);
            pg.Header(title, "Função: " + fn.Name);
            return fn;
        }

        void PageLambdaGeneral(Flow pg)
        {
            var fn = FnPage(pg, "Editar configurações básicas");
            if (fn == null) return;
            Panel box;
            var f = pg.Section("Configurações básicas", out box);
            var mem = f.Field("Memória (MB)", "De 128 MB a 10240 MB. Mais memória = mais CPU (e preço por milissegundo maior).", fn.Memory.ToString());
            var min = f.Field("Tempo limite: minutos", null, (fn.Timeout / 60).ToString());
            var sec = f.Field("Tempo limite: segundos", "Máximo 15 min (900 s).", (fn.Timeout % 60).ToString());
            pg.EndSection(box, f);
            var err = pg.Error();
            string fnm = fn.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("lambda-fn", fnm)), Btn("Salvar", true, () =>
            {
                int m = ToInt(mem.Text, -2), mi = ToInt(min.Text, -2), s = ToInt(sec.Text, -2);
                if (m < 0 || mi < 0 || s < 0 || s > 59) { err.Text = "✖ Use números inteiros (segundos de 0 a 59)."; return; }
                var e = Sim.UpdateFnConfig(fnm, mi * 60 + s, m, null);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("lambda-fn", fnm, false);
                Flash("Configurações de " + fnm + " atualizadas.", OkGreen);
            }));
        }

        void PageLambdaRuntime(Flow pg)
        {
            var fn = FnPage(pg, "Editar configurações de tempo de execução");
            if (fn == null) return;
            Panel box;
            var f = pg.Section("Configurações de tempo de execução", out box);
            var rt = f.Select("Tempo de execução", AwsSim.Runtimes.Select(r => r[1] + "   (" + r[0] + ")" + (r[0] == "python3.8" || r[0] == "nodejs18.x" ? " — descontinuado" : "")).ToArray(), Array.FindIndex(AwsSim.Runtimes, r => r[1] == fn.Runtime));
            f.Text("Runtimes descontinuados não recebem correções de segurança. Troque para a versão que o time testou e rode os testes depois.", fSmall, Muted);
            pg.EndSection(box, f);
            var err = pg.Error();
            string fnm = fn.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("lambda-fn", fnm)), Btn("Salvar", true, () =>
            {
                if (rt.SelectedIndex < 0) { err.Text = "✖ Escolha o tempo de execução."; return; }
                var e = Sim.UpdateFnConfig(fnm, -1, -1, AwsSim.Runtimes[rt.SelectedIndex][0]);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("lambda-fn", fnm, false);
                Flash("Tempo de execução de " + fnm + ": " + AwsSim.Runtimes[rt.SelectedIndex][1] + ".", OkGreen);
            }));
        }

        void PageLambdaEnv(Flow pg)
        {
            var fn = FnPage(pg, "Editar variáveis de ambiente");
            if (fn == null) return;
            Panel box;
            var f = pg.Section("Variáveis de ambiente", out box);
            var rows = KvEditor(f, box, fn.Env.ToList(), 2, "Chave", "Valor");
            pg.EndSection(box, f);
            var err = pg.Error();
            string fnm = fn.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("lambda-fn", fnm)), Btn("Salvar", true, () =>
            {
                string er;
                var d = KvRead(rows, out er);
                if (er != null) { err.Text = "✖ " + er; return; }
                var e = Sim.SetEnv(fnm, d);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("lambda-fn", fnm, false);
                Flash("Variáveis de ambiente de " + fnm + " salvas.", OkGreen);
            }));
        }

        void PageLambdaConcurrency(Flow pg)
        {
            var fn = FnPage(pg, "Editar simultaneidade");
            if (fn == null) return;
            Panel box;
            var f = pg.Section("Simultaneidade", out box);
            var mode = f.Radios(null, new[] { "Usar simultaneidade não reservada", "Reservar simultaneidade" }, fn.Concurrency < 0 ? 0 : 1);
            var n = f.Field("Simultaneidade reservada", "Garante E limita: a função nunca passa desse número de execuções simultâneas. 0 = a função para de rodar.", fn.Concurrency < 0 ? "" : fn.Concurrency.ToString());
            pg.EndSection(box, f);
            var err = pg.Error();
            string fnm = fn.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("lambda-fn", fnm)), Btn("Salvar", true, () =>
            {
                int v = mode[0].Checked ? -1 : ToInt(n.Text, -2);
                if (v == -2) { err.Text = "✖ Informe um número inteiro."; return; }
                var e = Sim.SetConcurrency(fnm, v);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("lambda-fn", fnm, false);
                Flash("Simultaneidade de " + fnm + " atualizada.", OkGreen);
            }));
        }

        void PageLambdaTrigger(Flow pg)
        {
            var fn = FnPage(pg, "Adicionar gatilho");
            if (fn == null) return;
            var qs = Sim.Queues.ToList();
            Panel box;
            var f = pg.Section("Configuração do gatilho", out box);
            f.Select("Origem", new[] { "SQS (Amazon Simple Queue Service)" }, 0);
            var q = f.Select("Fila do SQS", qs.Select(x => x.Name + "   (arn:aws:sqs:us-east-1:123456789012:" + x.Name + ")").ToArray(), -1);
            var batch = f.Field("Tamanho do lote", "Quantas mensagens a função recebe por execução (1 a 10000; FIFO até 10).", "10");
            f.Text("A função de execução precisa de permissão para ler e apagar mensagens da fila (sqs:ReceiveMessage, sqs:DeleteMessage...).", fSmall, Muted);
            pg.EndSection(box, f);
            var err = pg.Error();
            string fnm = fn.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("lambda-fn", fnm)), Btn("Adicionar", true, () =>
            {
                if (q.SelectedIndex < 0) { err.Text = "✖ Escolha a fila."; return; }
                int b = ToInt(batch.Text, -1);
                if (b < 1) { err.Text = "✖ O tamanho do lote precisa ser um número maior que zero."; return; }
                var e = Sim.AddTrigger(fnm, qs[q.SelectedIndex].Name, b);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("lambda-fn", fnm, false);
                Flash("Gatilho SQS " + qs[q.SelectedIndex].Name + " adicionado a " + fnm + ".", OkGreen);
            }));
        }

        // ── CloudWatch: retenção
        void PageRetention(Flow pg)
        {
            string grp = arg;
            if (grp == null || !AwsSim.LogGroups.Contains(grp)) { Navigate("cw-logs", null, false); return; }
            PageTitle = "Editar retenção - CloudWatch";
            pg.Crumbs("CloudWatch", "Grupos de logs", grp, "Editar configuração de retenção");
            pg.Header("Editar configuração de retenção", "Grupo de logs: " + grp);
            Panel box;
            var f = pg.Section("Retenção", out box);
            var opts = new[] { 0 }.Concat(AwsSim.RetentionDays).ToArray();
            var sel = f.Select("Configuração de retenção", opts.Select(AwsSim.RetentionText).ToArray(), Array.IndexOf(opts, Sim.Retention(grp)));
            f.Text("Eventos mais antigos que a retenção são apagados automaticamente. Logs guardados também são cobrados por GB/mês.", fSmall, Muted);
            pg.EndSection(box, f);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("cw-logs", null)), Btn("Salvar", true, () =>
            {
                if (sel.SelectedIndex < 0) { err.Text = "✖ Escolha a retenção."; return; }
                var e = Sim.SetRetention(grp, opts[sel.SelectedIndex]);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("cw-logs", null, false);
                Flash("Retenção de " + grp + ": " + AwsSim.RetentionText(opts[sel.SelectedIndex]) + ".", OkGreen);
            }));
        }

        // ── ECR
        void PageEcrEdit(Flow pg)
        {
            var r = Sim.RepoOf(arg ?? "");
            if (r == null) { Navigate("ecr", null, false); return; }
            PageTitle = "Editar repositório - ECR";
            pg.Crumbs("Amazon ECR", "Repositórios privados", r.Name, "Editar");
            pg.Header("Editar repositório: " + r.Name, null);
            Panel box;
            var f = pg.Section("Configurações", out box);
            var imm = f.Check("Imutabilidade de tags", r.Immutable, "Impede que um push sobrescreva uma tag que já existe (ex.: 1.4.2 sempre aponta para a mesma imagem).");
            var scan = f.Check("Verificar ao enviar (scan on push)", r.ScanOnPush, "Procura vulnerabilidades conhecidas (CVEs) em cada imagem enviada.");
            pg.EndSection(box, f);
            var err = pg.Error();
            string rn = r.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ecr", null)), Btn("Salvar", true, () =>
            {
                var rr = Sim.RepoOf(rn);
                string e = null;
                if (rr.Immutable != imm.Checked) e = Sim.SetImmutable(rn, imm.Checked);
                if (e == null && rr.ScanOnPush != scan.Checked) e = Sim.SetScan(rn, scan.Checked);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("ecr", null, false);
                Flash("Repositório " + rn + " atualizado.", OkGreen);
            }));
        }

        void PageEcrLifecycle(Flow pg)
        {
            var r = Sim.RepoOf(arg ?? "");
            if (r == null) { Navigate("ecr", null, false); return; }
            PageTitle = "Política de ciclo de vida - ECR";
            pg.Crumbs("Amazon ECR", "Repositórios privados", r.Name, "Política de ciclo de vida", "Criar regra");
            pg.Header("Criar regra de ciclo de vida", "Repositório: " + r.Name + (r.LcTag != null ? " · regra atual: " + EcrRuleText(r) + " (será substituída)" : ""));
            Panel box;
            var f = pg.Section("Regra", out box);
            f.Field("Prioridade da regra", null, "1");
            f.Field("Descrição da regra", null);
            var ts = f.Radios("Status da imagem", new[] { "Com tag (curinga ou prefixos)", "Sem tag", "Qualquer" }, 1);
            var ct = f.Radios("Critérios de correspondência", new[] { "Desde o envio da imagem (dias)", "Contagem de imagens maior que" }, 0);
            var n = f.Field("Número", "Dias (desde o envio) ou quantidade de imagens a manter.");
            f.Text("Ação da regra: expirar (apagar as imagens que correspondem).", fSmall, Muted);
            pg.EndSection(box, f);
            var err = pg.Error();
            string rn = r.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ecr", null)), Btn("Salvar", true, () =>
            {
                int v = ToInt(n.Text, -1);
                if (v < 1) { err.Text = "✖ Informe um número maior que zero."; return; }
                var e = Sim.SetEcrLifecycle(rn, ts[0].Checked ? "tagged" : ts[1].Checked ? "untagged" : "any", ct[0].Checked ? "sinceImagePushed" : "imageCountMoreThan", v);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("ecr", null, false);
                Flash("Política de ciclo de vida de " + rn + " salva.", OkGreen);
            }));
        }

        // ── VPC
        void PageVpcs(Flow pg)
        {
            PageTitle = "VPCs - VPC";
            pg.Crumbs("VPC", "Suas VPCs");
            var list = Sim.Vpcs.Where(v => v.Region == Sim.Region).ToList();
            pg.Header("Suas VPCs (" + list.Count + ")", "Uma VPC é a sua rede privada na AWS, dividida em sub-redes (públicas e privadas) em várias zonas de disponibilidade.");
            pg.Table(new[] { "Nome", "ID da VPC", "CIDR IPv4", "Sub-redes" }, new[] { 1.4f, 1.4f, 1.2f, 1f }, list.Select(v => new[] { v.Name, v.Id, v.Cidr, Sim.Subnets.Count(s => s.Vpc == v.Id).ToString() }).ToList(), null);
            pg.Text("Gateway da Internet: igw-0cafe365 · Gateway NAT: nat-0cafe01 (fica na sub-rede publica-a) — região " + Sim.Region + ".", fSmall, Muted);
        }

        void PageSubnets(Flow pg)
        {
            PageTitle = "Sub-redes - VPC";
            pg.Crumbs("VPC", "Sub-redes");
            var list = Sim.Subnets.Where(s => Sim.Vpcs.Any(v => v.Id == s.Vpc && v.Region == Sim.Region)).ToList();
            pg.Header("Sub-redes (" + list.Count + ")", "Pública = a tabela de rotas manda 0.0.0.0/0 para o gateway da Internet. Privada = sem rota direta da/para a internet.",
                Btn("Criar sub-rede", true, () => Navigate("vpc-subnet-create", null)));
            pg.Table(new[] { "Nome", "ID da sub-rede", "VPC", "CIDR IPv4", "Zona", "Tabela de rotas" }, new[] { 1.2f, 1.4f, 1.2f, 1.1f, 1f, 1.2f },
                list.Select(s => { var r = Sim.RouteTables.FirstOrDefault(x => x.Id == s.Rtb); return new[] { s.Name.Length == 0 ? "-" : s.Name, s.Id, s.Vpc, s.Cidr, s.Az, r != null ? r.Name : "-" }; }).ToList(), null);
        }

        void PageSubnetCreate(Flow pg)
        {
            PageTitle = "Criar sub-rede - VPC";
            pg.Crumbs("VPC", "Sub-redes", "Criar sub-rede");
            pg.Header("Criar sub-rede", null);
            var vpcs = Sim.Vpcs.Where(v => v.Region == Sim.Region).ToList();
            Panel box;
            var f = pg.Section("Configurações da sub-rede", out box);
            var vpc = f.Select("ID da VPC", vpcs.Select(v => v.Id + " (" + v.Name + ") · " + v.Cidr).ToArray(), 0);
            var name = f.Field("Nome da sub-rede", "Cria a tag Name.");
            var az = f.Select("Zona de disponibilidade", new[] { "a", "b", "c" }.Select(x => Sim.Region + x).ToArray(), -1);
            var cidr = f.Field("Bloco CIDR IPv4 da sub-rede", "Dentro do bloco da VPC e sem sobrepor outra sub-rede (ex.: 10.0.5.0/24 = 256 endereços, 251 utilizáveis).");
            pg.EndSection(box, f);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("vpc-subnets", null)), Btn("Criar sub-rede", true, () =>
            {
                if (vpcs.Count == 0) { err.Text = "✖ Não há VPC nesta região."; return; }
                if (az.SelectedIndex < 0) { err.Text = "✖ Escolha a zona de disponibilidade."; return; }
                string id;
                var e = Sim.CreateSubnet(vpcs[Math.Max(0, vpc.SelectedIndex)].Id, cidr.Text, az.Items[az.SelectedIndex].ToString(), name.Text, out id);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("vpc-subnets", null, false);
                Flash("Sub-rede " + id + " criada.", OkGreen);
            }));
        }

        void PageRtbs(Flow pg)
        {
            PageTitle = "Tabelas de rotas - VPC";
            pg.Crumbs("VPC", "Tabelas de rotas");
            var list = Sim.RouteTables.Where(r => Sim.Vpcs.Any(v => v.Id == r.Vpc && v.Region == Sim.Region)).ToList();
            pg.Header("Tabelas de rotas (" + list.Count + ")", "Cada sub-rede usa uma tabela de rotas: ela diz para onde vai o tráfego de cada destino.");
            pg.Table(new[] { "Nome", "ID da tabela", "VPC", "Sub-redes associadas", "Rotas" }, new[] { 1.1f, 1.2f, 1.1f, 1.6f, 2.2f },
                list.Select(r => new[] { r.Name, r.Id, r.Vpc, string.Join(", ", Sim.Subnets.Where(s => s.Rtb == r.Id).Select(s => s.Name.Length > 0 ? s.Name : s.Id)), string.Join("; ", r.Routes.Select(x => x[0] + " → " + x[1])) }).ToList(),
                i => Navigate("vpc-rtb", list[i].Id));
            pg.Text("Clique no nome da tabela para ver e editar as rotas.", fSmall, Muted);
        }

        void PageRtb(Flow pg)
        {
            var r = Sim.RouteTables.FirstOrDefault(x => x.Id == arg);
            if (r == null) { Navigate("vpc-rtbs", null, false); return; }
            PageTitle = r.Name + " - VPC";
            pg.Crumbs("VPC", "Tabelas de rotas", r.Id);
            string rid = r.Id;
            pg.Header(r.Name + " (" + r.Id + ")", "Sub-redes associadas: " + string.Join(", ", Sim.Subnets.Where(s => s.Rtb == r.Id).Select(s => (s.Name.Length > 0 ? s.Name : s.Id) + " " + s.Cidr)));
            pg.SubHeader("Rotas (" + r.Routes.Count + ")", Btn("Editar rotas", true, () => Navigate("vpc-routes-edit", rid)));
            pg.Table(new[] { "Destino", "Alvo", "Status" }, new[] { 1.4f, 2f, 1f }, r.Routes.Select(x => new[] { x[0], x[1] + (x[1].StartsWith("igw-") ? " (gateway da Internet)" : x[1].StartsWith("nat-") ? " (gateway NAT)" : ""), "● Ativo" }).ToList(), null);
            pg.Text("A rota mais específica vence: 10.0.0.0/16 → local mantém o tráfego interno; 0.0.0.0/0 pega todo o resto (internet).", fSmall, Muted);
        }

        void PageRoutesEdit(Flow pg)
        {
            var r = Sim.RouteTables.FirstOrDefault(x => x.Id == arg);
            if (r == null) { Navigate("vpc-rtbs", null, false); return; }
            PageTitle = "Editar rotas - VPC";
            pg.Crumbs("VPC", "Tabelas de rotas", r.Id, "Editar rotas");
            pg.Header("Editar rotas", r.Name + " (" + r.Id + ")");
            var targets = r.Vpc == "vpc-0br3650" ? new[] { new[] { "igw-0br3650", "Gateway da Internet (igw)" } } : AwsSim.RouteTargets.Where(t => t[0] != "local").ToArray();
            Panel box;
            var f = pg.Section("Rotas atuais", out box);
            var existing = r.Routes.ToList();
            var removes = new List<CheckBox>();
            var sels = new List<ComboBox>();
            foreach (var x in existing)
            {
                if (x[1] == "local") { f.Text(x[0] + " → local (não pode ser alterada)", fUi, Muted); removes.Add(null); sels.Add(null); continue; }
                f.Text("Destino " + x[0], fBold, Ink);
                var sel = new ComboBox { Location = new Point(f.X, f.Y), Width = Math.Min(f.W - S(120), S(340)), Font = fUi, DropDownStyle = ComboBoxStyle.DropDownList };
                sel.Items.AddRange(targets.Select(t => t[0] + " — " + t[1]).ToArray());
                sel.SelectedIndex = Array.FindIndex(targets, t => t[0] == x[1]);
                box.Controls.Add(sel);
                var rm = new CheckBox { Text = "Remover", Font = fUi, Location = new Point(sel.Right + S(12), f.Y + S(2)), AutoSize = true };
                box.Controls.Add(rm);
                f.Y += sel.Height + S(12);
                removes.Add(rm); sels.Add(sel);
            }
            pg.EndSection(box, f);
            var g = pg.Section("Adicionar rota", out box);
            var dest = g.Field("Destino", "Bloco CIDR (0.0.0.0/0 = todo o resto, ou seja, a internet).");
            var tgt = g.Select("Alvo", targets.Select(t => t[0] + " — " + t[1]).ToArray(), -1);
            g.Text("Sub-rede privada que precisa SAIR para a internet (atualizações, APIs) usa o gateway NAT; ninguém de fora consegue iniciar conexão com ela.", fSmall, Muted);
            pg.EndSection(box, g);
            var err = pg.Error();
            string rid = r.Id;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("vpc-rtb", rid)), Btn("Salvar alterações", true, () =>
            {
                if (dest.Text.Trim().Length > 0 && tgt.SelectedIndex < 0) { err.Text = "✖ Escolha o alvo da nova rota."; return; }
                for (int i = 0; i < existing.Count; i++)
                {
                    if (removes[i] == null) continue;
                    string e = null;
                    if (removes[i].Checked) e = Sim.DeleteRoute(rid, existing[i][0]);
                    else if (sels[i].SelectedIndex >= 0 && targets[sels[i].SelectedIndex][0] != existing[i][1]) e = Sim.SetRoute(rid, existing[i][0], targets[sels[i].SelectedIndex][0], true);
                    if (e != null) { err.Text = "✖ " + e; return; }
                }
                if (dest.Text.Trim().Length > 0)
                {
                    var e = Sim.SetRoute(rid, dest.Text.Trim(), targets[tgt.SelectedIndex][0], false);
                    if (e != null) { err.Text = "✖ " + e; return; }
                }
                Navigate("vpc-rtb", rid, false);
                Flash("Rotas de " + rid + " salvas.", OkGreen);
            }));
        }

        // ── SNS
        static string TopicArn(string n) { return "arn:aws:sns:us-east-1:123456789012:" + n; }

        void PageTopics(Flow pg)
        {
            PageTitle = "Tópicos - SNS";
            pg.Crumbs("Amazon SNS", "Tópicos");
            var list = Sim.Topics.ToList();
            pg.Header("Tópicos (" + list.Count + ")", "Um tópico recebe uma mensagem e entrega uma cópia para cada assinante (e-mail, SMS, fila, função...).", Btn("Criar tópico", true, () => Navigate("sns-create", null)));
            pg.Table(new[] { "Nome", "Tipo", "Assinaturas", "ARN" }, new[] { 1.2f, 0.8f, 1f, 3f }, list.Select(t => new[] { t.Name, t.Fifo ? "FIFO" : "Padrão", t.Subs.Count.ToString(), TopicArn(t.Name) }).ToList(), i => Navigate("sns-topic", list[i].Name));
        }

        void PageTopicCreate(Flow pg)
        {
            PageTitle = "Criar tópico - SNS";
            pg.Crumbs("Amazon SNS", "Tópicos", "Criar tópico");
            pg.Header("Criar tópico", null);
            Panel box;
            var f = pg.Section("Detalhes", out box);
            var type = f.Radios("Tipo", new[] { "FIFO (primeiro a entrar, primeiro a sair) — ordem garantida, só entrega para filas SQS", "Padrão — entrega para e-mail, SMS, HTTP, SQS, Lambda..." }, 1);
            var name = f.Field("Nome", "Letras, números, hífens e _. Tópicos FIFO terminam com .fifo.");
            pg.EndSection(box, f);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("sns", null)), Btn("Criar tópico", true, () =>
            {
                var e = Sim.CreateTopic(name.Text, type[0].Checked);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("sns-topic", name.Text.Trim(), false);
                Flash("Tópico " + name.Text.Trim() + " criado.", OkGreen);
            }));
        }

        void PageTopic(Flow pg)
        {
            var t = Sim.Tp(arg ?? "");
            if (t == null) { Navigate("sns", null, false); return; }
            PageTitle = t.Name + " - SNS";
            pg.Crumbs("Amazon SNS", "Tópicos", t.Name);
            string tn = t.Name;
            pg.Header(t.Name, "ARN: " + TopicArn(t.Name) + " · Tipo: " + (t.Fifo ? "FIFO" : "Padrão"),
                Btn("Publicar mensagem", false, () => Navigate("sns-publish-edit", tn)));
            var subs = t.Subs.ToList();
            var lvRef = new ListView[1];
            pg.SubHeader("Assinaturas (" + subs.Count + ")",
                Btn("Excluir", false, () => { int i = SelIdx(lvRef, "uma assinatura"); if (i >= 0 && Confirm("Excluir a assinatura " + subs[i][1] + "?", "Excluir assinatura")) Done(Sim.Unsubscribe(tn, subs[i][0], subs[i][1]), "Assinatura excluída."); }),
                Btn("Criar assinatura", true, () => Navigate("sns-sub-create", tn)));
            lvRef[0] = pg.Table(new[] { "Protocolo", "Endpoint", "Status" }, new[] { 1f, 2.4f, 1.4f }, subs.Select(s => new[] { s[0], s[1], "● " + s[2] }).ToList(), null);
            pg.Text("Assinatura de e-mail fica 'Confirmação pendente' até o destinatário clicar no link que a AWS envia.", fSmall, Muted);
        }

        void PageSubCreate(Flow pg)
        {
            var t = Sim.Tp(arg ?? "");
            if (t == null) { Navigate("sns", null, false); return; }
            PageTitle = "Criar assinatura - SNS";
            pg.Crumbs("Amazon SNS", "Tópicos", t.Name, "Criar assinatura");
            pg.Header("Criar assinatura", null);
            var protos = new[] { new[] { "email", "E-mail" }, new[] { "sms", "SMS" }, new[] { "https", "HTTPS" }, new[] { "sqs", "Amazon SQS" }, new[] { "lambda", "AWS Lambda" } };
            Panel box;
            var f = pg.Section("Detalhes", out box);
            f.Select("ARN do tópico", new[] { TopicArn(t.Name) }, 0);
            var proto = f.Select("Protocolo", protos.Select(p => p[1]).ToArray(), -1);
            var ep = f.Field("Endpoint", "E-mail: o endereço; SMS: o telefone; HTTPS: a URL; SQS/Lambda: o ARN.");
            pg.EndSection(box, f);
            var err = pg.Error();
            string tn = t.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("sns-topic", tn)), Btn("Criar assinatura", true, () =>
            {
                if (proto.SelectedIndex < 0) { err.Text = "✖ Escolha o protocolo."; return; }
                var e = Sim.Subscribe(tn, protos[proto.SelectedIndex][0], ep.Text);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("sns-topic", tn, false);
                Flash("Assinatura criada. " + (proto.SelectedIndex == 0 ? "A AWS enviou um e-mail de confirmação para " + ep.Text.Trim() + "." : ""), OkGreen);
            }));
        }

        void PagePublish(Flow pg)
        {
            var t = Sim.Tp(arg ?? "");
            if (t == null) { Navigate("sns", null, false); return; }
            PageTitle = "Publicar mensagem - SNS";
            pg.Crumbs("Amazon SNS", "Tópicos", t.Name, "Publicar mensagem");
            pg.Header("Publicar mensagem no tópico", t.Name + " · " + t.Subs.Count + " assinatura(s)");
            Panel box;
            var f = pg.Section("Detalhes da mensagem", out box);
            f.Field("Assunto - opcional", "Vira o assunto do e-mail.");
            var body = f.Field("Corpo da mensagem", null);
            pg.EndSection(box, f);
            var err = pg.Error();
            string tn = t.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("sns-topic", tn)), Btn("Publicar mensagem", true, () =>
            {
                var e = Sim.Publish(tn, body.Text);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("sns-topic", tn, false);
                Flash("Mensagem publicada em " + tn + ".", OkGreen);
            }));
        }

        // ── SQS
        void PageQueues(Flow pg)
        {
            PageTitle = "Filas - SQS";
            pg.Crumbs("Amazon SQS", "Filas");
            var list = Sim.Queues.ToList();
            pg.Header("Filas (" + list.Count + ")", "Filas desacoplam quem produz de quem processa: as mensagens esperam até alguém buscar.", Btn("Criar fila", true, () => Navigate("sqs-create", null)));
            pg.Table(new[] { "Nome", "Tipo", "Visibilidade", "Espera", "Fila de mensagens mortas" }, new[] { 1.4f, 0.8f, 1f, 0.8f, 1.8f },
                list.Select(q => new[] { q.Name, q.Fifo ? "FIFO" : "Padrão", q.Visibility + " s", q.Wait + " s", q.Dlq != null ? q.Dlq + " (máx. " + q.MaxReceive + ")" : "-" }).ToList(), i => Navigate("sqs-queue", list[i].Name));
        }

        void PageQueue(Flow pg)
        {
            var q = Sim.Q(arg ?? "");
            if (q == null) { Navigate("sqs", null, false); return; }
            PageTitle = q.Name + " - SQS";
            pg.Crumbs("Amazon SQS", "Filas", q.Name);
            string qn = q.Name;
            pg.Header(q.Name, "URL: " + AwsSim.QueueUrl(q.Name),
                Btn("Excluir", false, () => { if (Confirm("Excluir a fila " + qn + " e todas as mensagens?", "Excluir fila")) { var e = Sim.DeleteQueue(qn); if (e != null) Flash("✖ " + e, ErrRed); else { Navigate("sqs", null, false); Flash("Fila " + qn + " excluída.", OkGreen); } } }),
                Btn("Editar", true, () => Navigate("sqs-edit", qn)));
            Panel box;
            var f = pg.Section("Detalhes", out box);
            f.Text("ARN: arn:aws:sqs:us-east-1:123456789012:" + q.Name + " · Tipo: " + (q.Fifo ? "FIFO" + (q.Dedup ? " (deduplicação por conteúdo)" : "") : "Padrão"), fUi, Ink);
            f.Text("Tempo limite de visibilidade: " + q.Visibility + " s   ·   Tempo de espera de recebimento: " + q.Wait + " s" + (q.Wait == 0 ? " (sondagem curta)" : " (sondagem longa)"), fBold, Ink);
            f.Text("Fila de mensagens mortas (DLQ): " + (q.Dlq != null ? q.Dlq + " após " + q.MaxReceive + " recebimento(s) sem sucesso" : "desativada"), fBold, Ink);
            var users = Sim.Functions.Where(fn => fn.Triggers.Any(t => t[0] == qn)).Select(fn => fn.Name + " (tempo limite " + fn.Timeout + " s)").ToList();
            f.Text("Consumidores (gatilhos Lambda): " + (users.Count == 0 ? "nenhum" : string.Join(", ", users)), fSmall, Muted);
            pg.EndSection(box, f);
        }

        void PageQueueForm(Flow pg, AwsSim.Queue q)
        {
            bool edit = q != null;
            if (!edit && page == "sqs-edit") { Navigate("sqs", null, false); return; }
            PageTitle = (edit ? "Editar fila" : "Criar fila") + " - SQS";
            if (edit) pg.Crumbs("Amazon SQS", "Filas", q.Name, "Editar"); else pg.Crumbs("Amazon SQS", "Filas", "Criar fila");
            pg.Header(edit ? "Editar " + q.Name : "Criar fila", null);
            Panel box;
            var f = pg.Section("Detalhes", out box);
            List<RadioButton> type = null;
            TextBox name = null;
            if (!edit)
            {
                type = f.Radios("Tipo", new[] { "Padrão — entrega pelo menos uma vez, ordem aproximada, vazão quase ilimitada", "FIFO — ordem garantida e processamento exatamente uma vez" }, 0);
                name = f.Field("Nome", "FIFO precisa terminar com .fifo.");
            }
            else f.Text("Tipo: " + (q.Fifo ? "FIFO" : "Padrão") + " (não muda depois de criada)", fUi, Ink);
            pg.EndSection(box, f);
            var g = pg.Section("Configuração", out box);
            var vis = g.Field("Tempo limite de visibilidade (segundos)", "Quanto tempo a mensagem fica invisível depois de recebida, esperando ser apagada. Se o processamento demorar mais que isso, ela reaparece e é processada de novo.", edit ? q.Visibility.ToString() : "30");
            var wait = g.Field("Tempo de espera de recebimento (segundos)", "0 = sondagem curta (responde na hora, mesmo vazia). 1 a 20 = sondagem longa (espera chegar mensagem: menos chamadas vazias, menos custo).", edit ? q.Wait.ToString() : "0");
            CheckBox dedup = null;
            if (!edit) dedup = g.Check("Deduplicação baseada em conteúdo (só FIFO)", false, "Mensagens com o mesmo conteúdo enviadas em 5 minutos contam como uma só.");
            pg.EndSection(box, g);
            var others = Sim.Queues.Where(x => !edit || x.Name != q.Name).ToList();
            var h = pg.Section("Fila de mensagens mortas (DLQ)", out box);
            h.Text("Mensagens que falham várias vezes vão para a DLQ, em vez de ficar voltando para sempre.", fSmall, Muted);
            var dl = h.Radios(null, new[] { "Desativado", "Ativado" }, edit && q.Dlq != null ? 1 : 0);
            var dq = h.Select("Fila (DLQ)", others.Select(x => x.Name).ToArray(), edit && q.Dlq != null ? others.FindIndex(x => x.Name == q.Dlq) : -1);
            var mx = h.Field("Máximo de recebimentos", "Quantas tentativas antes de mover para a DLQ (1 a 1000).", edit && q.Dlq != null ? q.MaxReceive.ToString() : "");
            pg.EndSection(box, h);
            var err = pg.Error();
            string orig = edit ? q.Name : null;
            pg.Buttons(Btn("Cancelar", false, () => Navigate(edit ? "sqs-queue" : "sqs", orig)), Btn(edit ? "Salvar" : "Criar fila", true, () =>
            {
                int v = ToInt(vis.Text, -1), w = ToInt(wait.Text, -1);
                if (v < 0 || w < 0) { err.Text = "✖ Use números inteiros de segundos."; return; }
                if (dl[1].Checked && (dq.SelectedIndex < 0 || ToInt(mx.Text, -1) < 1)) { err.Text = "✖ Escolha a fila de mensagens mortas e o máximo de recebimentos."; return; }
                string qn = edit ? orig : name.Text.Trim();
                string e;
                if (!edit) e = Sim.CreateQueue(name.Text, type[1].Checked, dedup.Checked, v, w);
                else
                {
                    e = Sim.SetQueueAttr(qn, "VisibilityTimeout", v);
                    if (e == null) e = Sim.SetQueueAttr(qn, "ReceiveMessageWaitTimeSeconds", w);
                }
                if (e == null) e = dl[1].Checked ? Sim.SetRedrive(qn, others[dq.SelectedIndex].Name, ToInt(mx.Text, 1)) : (edit ? Sim.SetRedrive(qn, null, 0) : null);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("sqs-queue", qn, false);
                Flash("Fila " + qn + (edit ? " atualizada." : " criada."), OkGreen);
            }));
        }

        // ── DynamoDB
        static string DdbType(string t) { return t == "S" ? "String" : t == "N" ? "Número" : t == "B" ? "Binário" : "-"; }

        void PageTables(Flow pg)
        {
            PageTitle = "Tabelas - DynamoDB";
            pg.Crumbs("DynamoDB", "Tabelas");
            var list = Sim.Tables.ToList();
            pg.Header("Tabelas (" + list.Count + ")", "Banco NoSQL chave-valor: cada item é achado pela chave de partição (e, se houver, pela chave de classificação).", Btn("Criar tabela", true, () => Navigate("ddb-create", null)));
            pg.Table(new[] { "Nome", "Chave de partição", "Chave de classificação", "Modo de capacidade", "Proteção contra exclusão" }, new[] { 1.4f, 1.4f, 1.4f, 1.2f, 1.2f },
                list.Select(t => new[] { t.Name, t.Pk + " (" + t.PkType + ")", t.Sk != null ? t.Sk + " (" + t.SkType + ")" : "-", t.Billing == "PAY_PER_REQUEST" ? "Sob demanda" : "Provisionada", t.Protect ? "● Ativado" : "● Desativado" }).ToList(), i => Navigate("ddb-table", list[i].Name));
        }

        void PageTableCreate(Flow pg)
        {
            PageTitle = "Criar tabela - DynamoDB";
            pg.Crumbs("DynamoDB", "Tabelas", "Criar tabela");
            pg.Header("Criar tabela", null);
            var types = new[] { "String", "Número", "Binário" };
            var codes = new[] { "S", "N", "B" };
            Panel box;
            var f = pg.Section("Detalhes da tabela", out box);
            var name = f.Field("Nome da tabela", "3 a 255 caracteres: letras, números, _, - e .");
            var pk = f.Field("Chave de partição", "Identifica o item (ex.: cliente_id). Escolha um atributo com muitos valores diferentes.");
            var pkt = f.Select("Tipo da chave de partição", types, 0);
            var sk = f.Field("Chave de classificação - opcional", "Ordena itens com a mesma chave de partição (ex.: data).");
            var skt = f.Select("Tipo da chave de classificação", types, 0);
            pg.EndSection(box, f);
            var g = pg.Section("Configurações da tabela: capacidade de leitura/gravação", out box);
            var cap = g.Radios(null, new[] { "Sob demanda — paga por requisição; ideal para tráfego imprevisível ou com picos", "Provisionada — você define as unidades de leitura/gravação (paga mesmo sem uso)" }, 1);
            pg.EndSection(box, g);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("dynamodb", null)), Btn("Criar tabela", true, () =>
            {
                var e = Sim.CreateTable(name.Text, pk.Text, codes[Math.Max(0, pkt.SelectedIndex)], sk.Text, codes[Math.Max(0, skt.SelectedIndex)], cap[0].Checked ? "PAY_PER_REQUEST" : "PROVISIONED");
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("dynamodb", null, false);
                Flash("Tabela " + name.Text.Trim() + " sendo criada.", OkGreen);
            }));
        }

        void PageTable(Flow pg)
        {
            var t = Sim.Tb(arg ?? "");
            if (t == null) { Navigate("dynamodb", null, false); return; }
            PageTitle = t.Name + " - DynamoDB";
            pg.Crumbs("DynamoDB", "Tabelas", t.Name);
            string tn = t.Name;
            pg.Header(t.Name, "Chave de partição: " + t.Pk + " (" + DdbType(t.PkType) + ")" + (t.Sk != null ? " · Chave de classificação: " + t.Sk + " (" + DdbType(t.SkType) + ")" : "") + " · " + (t.Billing == "PAY_PER_REQUEST" ? "Sob demanda" : "Provisionada"),
                Btn("Explorar itens da tabela", true, () => Navigate("ddb-items", tn)));
            Panel box;
            var b = pg.Section("Backups: recuperação para um ponto no tempo (PITR)", out box);
            b.Text("PITR: " + (t.Pitr ? "● Ativado" : "● Desativado"), fBold, t.Pitr ? OkGreen : ErrRed);
            b.Text("Backup contínuo: restaura a tabela para qualquer segundo dos últimos 35 dias (ex.: logo antes de um DELETE errado).", fSmall, Muted);
            b.Buttons(Btn("Editar", false, () => Navigate("ddb-pitr-edit", tn)));
            pg.EndSection(box, b);
            var l = pg.Section("Tempo de vida (TTL)", out box);
            l.Text("TTL: " + (t.Ttl != null ? "● Ativado (atributo " + t.Ttl + ")" : "● Desativado"), fBold, t.Ttl != null ? OkGreen : Ink);
            l.Text("O DynamoDB apaga sozinho (e sem custo de gravação) os itens cujo atributo TTL, um horário em segundos (epoch), já passou.", fSmall, Muted);
            l.Buttons(Btn(t.Ttl != null ? "Desativar TTL" : "Ativar TTL", false, () =>
            {
                if (Sim.Tb(tn).Ttl == null) { Navigate("ddb-ttl-edit", tn); return; }
                if (Confirm("Desativar o TTL da tabela " + tn + "?", "Desativar TTL")) Done(Sim.SetTtl(tn, null), "TTL desativado.");
            }));
            pg.EndSection(box, l);
            var p = pg.Section("Proteção contra exclusão", out box);
            p.Text("Proteção contra exclusão: " + (t.Protect ? "● Ativado" : "● Desativado"), fBold, t.Protect ? OkGreen : ErrRed);
            p.Text("Com a proteção ativada, ninguém consegue excluir a tabela (nem por engano) sem antes desativá-la.", fSmall, Muted);
            p.Buttons(Btn(t.Protect ? "Desativar a proteção contra exclusão" : "Ativar a proteção contra exclusão", false, () =>
            {
                bool on = !Sim.Tb(tn).Protect;
                if (Confirm((on ? "Ativar" : "Desativar") + " a proteção contra exclusão da tabela " + tn + "?", "Proteção contra exclusão")) Done(Sim.SetTableProtect(tn, on), "Proteção contra exclusão " + (on ? "ativada" : "desativada") + ".");
            }));
            pg.EndSection(box, p);
        }

        void PageItems(Flow pg)
        {
            PageTitle = "Explorar itens - DynamoDB";
            pg.Crumbs("DynamoDB", "Explorar itens");
            var tables = Sim.Tables.ToList();
            var t = Sim.Tb(arg ?? "");
            pg.Header("Explorar itens" + (t != null ? ": " + t.Name : ""), "Escolha a tabela e execute uma verificação (scan) para listar os itens.");
            Panel box;
            var f = pg.Section("Verificar ou consultar itens", out box);
            var sel = f.Select("Tabela", tables.Select(x => x.Name).ToArray(), t != null ? tables.FindIndex(x => x.Name == t.Name) : -1);
            f.Buttons(Btn("Executar", true, () =>
            {
                if (sel.SelectedIndex < 0) { Flash("Escolha a tabela.", InfoBlue); return; }
                string tn = tables[sel.SelectedIndex].Name;
                List<string> its;
                Sim.Scan(tn, out its);
                Navigate("ddb-items", tn, false);
            }));
            pg.EndSection(box, f);
            if (t != null)
            {
                pg.Text("Itens retornados (" + t.Items.Count + ")", fH2, Ink);
                var g = pg.Section(null, out box);
                foreach (var it in t.Items) g.Text(it, fMono, Ink);
                if (t.Items.Count == 0) g.Text("A tabela está vazia.", fUi, Muted);
                pg.EndSection(box, g);
            }
        }

        void PageTtl(Flow pg)
        {
            var t = Sim.Tb(arg ?? "");
            if (t == null) { Navigate("dynamodb", null, false); return; }
            PageTitle = "Ativar TTL - DynamoDB";
            pg.Crumbs("DynamoDB", "Tabelas", t.Name, "Ativar o tempo de vida (TTL)");
            pg.Header("Ativar o tempo de vida (TTL)", "Tabela: " + t.Name);
            Panel box;
            var f = pg.Section("Configurações do TTL", out box);
            var at = f.Field("Nome do atributo TTL", "Atributo dos itens com o horário de expiração em segundos desde 1970 (epoch), tipo Número.");
            f.Text("Exemplo de item atual: " + (t.Items.Count > 0 ? t.Items[0] : "(vazia)"), fSmall, Muted);
            pg.EndSection(box, f);
            var err = pg.Error();
            string tn = t.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ddb-table", tn)), Btn("Ativar TTL", true, () =>
            {
                if (at.Text.Trim().Length == 0) { err.Text = "✖ Informe o nome do atributo."; return; }
                var e = Sim.SetTtl(tn, at.Text);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("ddb-table", tn, false);
                Flash("TTL ativado em " + tn + " com o atributo " + at.Text.Trim() + ".", OkGreen);
            }));
        }

        void PagePitr(Flow pg)
        {
            var t = Sim.Tb(arg ?? "");
            if (t == null) { Navigate("dynamodb", null, false); return; }
            PageTitle = "Editar PITR - DynamoDB";
            pg.Crumbs("DynamoDB", "Tabelas", t.Name, "Editar recuperação para um ponto no tempo");
            pg.Header("Editar recuperação para um ponto no tempo", "Tabela: " + t.Name);
            Panel box;
            var f = pg.Section("Recuperação para um ponto no tempo", out box);
            var on = f.Check("Ativar a recuperação para um ponto no tempo", t.Pitr, "Período de recuperação: 35 dias. Custo proporcional ao tamanho da tabela.");
            pg.EndSection(box, f);
            var err = pg.Error();
            string tn = t.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("ddb-table", tn)), Btn("Salvar alterações", true, () =>
            {
                var e = Sim.SetPitr(tn, on.Checked);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("ddb-table", tn, false);
                Flash("Recuperação para um ponto no tempo " + (on.Checked ? "ativada" : "desativada") + " em " + tn + ".", OkGreen);
            }));
        }

        // ── RDS
        void PageDbs(Flow pg)
        {
            PageTitle = "Bancos de dados - RDS";
            pg.Crumbs("Amazon RDS", "Bancos de dados");
            var list = Sim.Dbs.ToList();
            var lvRef = new ListView[1];
            Action<string> run = act =>
            {
                int i = SelIdx(lvRef, "um banco de dados"); if (i < 0) return;
                var d = list[i];
                string label = act == "stop" ? "Interromper temporariamente" : act == "start" ? "Iniciar" : "Reinicializar";
                if (!Confirm(label + " o banco " + d.Id + "?" + (act == "stop" ? " Ele fica parado por até 7 dias e então é reiniciado automaticamente pela AWS." : ""), label)) return;
                Done(Sim.DbAction(d.Id, act), label + ": " + d.Id + ".");
            };
            var act2 = MenuBtn("Ações  ▾",
                "Interromper temporariamente", (Action)(() => run("stop")),
                "Iniciar", (Action)(() => run("start")),
                "Reinicializar", (Action)(() => run("reboot")),
                "Tirar snapshot", (Action)(() => { int i = SelIdx(lvRef, "um banco de dados"); if (i >= 0) Navigate("rds-snap-create", list[i].Id); }));
            pg.Header("Bancos de dados (" + list.Count + ")", "Bancos relacionais gerenciados: a AWS cuida de backups, atualizações e failover.", act2,
                Btn("Modificar", false, () => { int i = SelIdx(lvRef, "um banco de dados"); if (i >= 0) Navigate("rds-modify-edit", list[i].Id); }));
            lvRef[0] = pg.Table(new[] { "Identificador", "Status", "Mecanismo", "Classe", "Multi-AZ", "Acesso público" }, new[] { 1.4f, 1.2f, 1.3f, 1.2f, 0.9f, 1.1f },
                list.Select(d => new[] { d.Id, "● " + d.Status, d.Engine, d.Class, d.MultiAz ? "Sim" : "Não", d.Public ? "Sim" : "Não" }).ToList(), i => Navigate("rds-db", list[i].Id));
            pg.Text("Clique no nome para ver os detalhes, ou selecione a linha e use 'Ações' / 'Modificar'.", fSmall, Muted);
        }

        void PageDb(Flow pg)
        {
            var d = Sim.DbI(arg ?? "");
            if (d == null) { Navigate("rds", null, false); return; }
            PageTitle = d.Id + " - RDS";
            pg.Crumbs("Amazon RDS", "Bancos de dados", d.Id);
            string id = d.Id;
            pg.Header(d.Id, d.Engine + " · " + d.Class + " · " + d.Status,
                Btn("Tirar snapshot", false, () => Navigate("rds-snap-create", id)),
                Btn("Modificar", true, () => Navigate("rds-modify-edit", id)));
            Panel box;
            var c = pg.Section("Conectividade e segurança", out box);
            c.Text("Endpoint: " + d.Id + ".c9x2k1.us-east-1.rds.amazonaws.com · porta 3306", fUi, Ink);
            c.Text("Acessível publicamente: " + (d.Public ? "● Sim" : "● Não"), fBold, d.Public ? ErrRed : OkGreen);
            c.Text("Com 'Sim', o banco ganha endereço público e pode ser alcançado da internet (se o grupo de segurança deixar).", fSmall, Muted);
            pg.EndSection(box, c);
            var g = pg.Section("Configuração", out box);
            g.Text("Multi-AZ: " + (d.MultiAz ? "Sim (réplica em espera em outra zona, failover automático)" : "Não") + " · Período de retenção de backup: " + d.Backup + " dia(s) · Proteção contra exclusão: " + (d.Protect ? "ativada" : "desativada"), fUi, Ink);
            if (d.Pending.Count > 0) g.Text("Modificações pendentes (próxima janela de manutenção, dom 03:00 UTC): " + string.Join(", ", d.Pending.Select(kv => kv.Key + "=" + kv.Value)), fBold, InfoBlue);
            pg.EndSection(box, g);
        }

        void PageDbModify(Flow pg)
        {
            var d = Sim.DbI(arg ?? "");
            if (d == null) { Navigate("rds", null, false); return; }
            PageTitle = "Modificar " + d.Id + " - RDS";
            pg.Crumbs("Amazon RDS", "Bancos de dados", d.Id, "Modificar");
            pg.Header("Modificar instância de banco de dados: " + d.Id, d.Engine);
            var classes = new[] { "db.t3.micro", "db.t3.small", "db.t3.medium", "db.m5.large" };
            Panel box;
            var a = pg.Section("Configuração da instância", out box);
            var cls = a.Select("Classe da instância", classes, Array.IndexOf(classes, d.Class));
            pg.EndSection(box, a);
            var b = pg.Section("Disponibilidade e durabilidade", out box);
            var maz = b.Radios("Implantação", new[] { "Instância de banco de dados Multi-AZ (cria uma réplica em espera em outra zona)", "Instância de banco de dados Single-AZ (sem réplica em espera)" }, d.MultiAz ? 0 : 1);
            pg.EndSection(box, b);
            var c = pg.Section("Conectividade", out box);
            var pub = c.Radios("Acesso público", new[] { "Acessível publicamente", "Não acessível publicamente" }, d.Public ? 0 : 1);
            pg.EndSection(box, c);
            var opts = new[] { 0, 1, 3, 7, 14, 21, 30, 35 };
            var e2 = pg.Section("Backup", out box);
            var bk = e2.Select("Período de retenção de backup", opts.Select(x => x == 0 ? "0 dias (desativa os backups automáticos)" : x + " dia(s)").ToArray(), Array.IndexOf(opts, d.Backup));
            pg.EndSection(box, e2);
            var p = pg.Section("Proteção contra exclusão", out box);
            var prot = p.Check("Ativar a proteção contra exclusão", d.Protect);
            pg.EndSection(box, p);
            var s = pg.Section("Programação das modificações", out box);
            var when = s.Radios(null, new[] { "Aplicar durante a próxima janela de manutenção programada (dom 03:00–03:30 UTC)", "Aplicar imediatamente" }, 0);
            s.Text("Algumas mudanças (como trocar a classe) reiniciam o banco: em horário de pico, prefira a janela de manutenção — a não ser que seja urgente.", fSmall, Muted);
            pg.EndSection(box, s);
            var err = pg.Error();
            string id = d.Id;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("rds", null)), Btn("Modificar instância de banco de dados", true, () =>
            {
                var ch = new Dictionary<string, string>();
                ch["class"] = classes[Math.Max(0, cls.SelectedIndex)];
                ch["multiaz"] = maz[0].Checked ? "true" : "false";
                ch["public"] = pub[0].Checked ? "true" : "false";
                ch["backup"] = opts[Math.Max(0, bk.SelectedIndex)].ToString();
                ch["protect"] = prot.Checked ? "true" : "false";
                var e = Sim.ModifyDb(id, ch, when[1].Checked);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("rds-db", id, false);
                Flash(when[1].Checked ? "Modificações aplicadas em " + id + "." : "Modificações de " + id + " agendadas para a próxima janela de manutenção.", OkGreen);
            }));
        }

        void PageDbSnap(Flow pg)
        {
            var d = Sim.DbI(arg ?? "");
            if (d == null) { Navigate("rds", null, false); return; }
            PageTitle = "Tirar snapshot - RDS";
            pg.Crumbs("Amazon RDS", "Bancos de dados", d.Id, "Tirar snapshot");
            pg.Header("Tirar snapshot do banco de dados", d.Id + " · " + d.Engine);
            Panel box;
            var f = pg.Section("Configurações", out box);
            var name = f.Field("Nome do snapshot", "Começa com letra; só letras, números e hífens. Snapshots manuais ficam guardados até você excluí-los.");
            pg.EndSection(box, f);
            var err = pg.Error();
            string id = d.Id;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("rds", null)), Btn("Tirar snapshot", true, () =>
            {
                var e = Sim.DbSnapshot(id, name.Text);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("rds-snapshots", null, false);
                Flash("Snapshot " + name.Text.Trim() + " de " + id + " em criação.", OkGreen);
            }));
        }

        void PageDbSnaps(Flow pg)
        {
            PageTitle = "Snapshots - RDS";
            pg.Crumbs("Amazon RDS", "Snapshots");
            var list = Sim.DbSnaps.ToList();
            pg.Header("Snapshots (" + list.Count + ")", "Automatizados: criados pelo período de retenção de backup (somem com ele). Manuais: ficam até você excluir.");
            pg.Table(new[] { "Nome do snapshot", "Banco de dados", "Tipo", "Criado em" }, new[] { 2f, 1.4f, 1.2f, 1.4f }, list.Select(x => new[] { x.Id, x.Db, x.Type, x.Created.ToString("dd/MM/yyyy HH:mm") }).ToList(), null);
            pg.Text("Para criar: Bancos de dados → selecione → Ações → Tirar snapshot.", fSmall, Muted);
        }

        // ── Route 53
        void PageZones(Flow pg)
        {
            PageTitle = "Zonas hospedadas - Route 53";
            pg.Crumbs("Route 53", "Zonas hospedadas");
            pg.Header("Zonas hospedadas (1)", "Uma zona hospedada guarda os registros DNS de um domínio.");
            pg.Table(new[] { "Nome do domínio", "Tipo", "Registros", "ID da zona hospedada" }, new[] { 1.6f, 1f, 1f, 1.6f }, new List<string[]> { new[] { AwsSim.Zone, "Pública", Sim.Records.Count.ToString(), AwsSim.ZoneId } }, i => Navigate("r53-zone", AwsSim.ZoneId));
        }

        void PageZone(Flow pg)
        {
            PageTitle = AwsSim.Zone + " - Route 53";
            pg.Crumbs("Route 53", "Zonas hospedadas", AwsSim.Zone);
            var list = Sim.Records.ToList();
            var lvRef = new ListView[1];
            pg.Header(AwsSim.Zone, "ID da zona hospedada: " + AwsSim.ZoneId + " · Pública",
                Btn("Excluir registro", false, () =>
                {
                    int i = SelIdx(lvRef, "um registro"); if (i < 0) return;
                    var r = list[i];
                    if (!Confirm("Excluir o registro " + r.Name + " " + r.Type + " (" + r.Value + ")?", "Excluir registro")) return;
                    Done(Sim.ChangeRecord("DELETE", r.Name, r.Type, r.Value, r.Ttl), "Registro " + r.Name + " " + r.Type + " excluído.");
                }),
                Btn("Editar registro", false, () => { int i = SelIdx(lvRef, "um registro"); if (i >= 0) Navigate("r53-record-edit", list[i].Name + "|" + list[i].Type); }),
                Btn("Criar registro", true, () => Navigate("r53-record-create", null)));
            lvRef[0] = pg.Table(new[] { "Nome do registro", "Tipo", "Valor/rotear tráfego para", "TTL (segundos)" }, new[] { 1.8f, 0.6f, 2.8f, 0.9f }, list.Select(r => new[] { r.Name, r.Type, r.Value, r.Ttl.ToString() }).ToList(), null);
            pg.Text("TTL = por quanto tempo os resolvedores guardam a resposta em cache. Registros NS e SOA da zona não aparecem neste laboratório.", fSmall, Muted);
        }

        void PageRecordForm(Flow pg, bool edit)
        {
            AwsSim.DnsRecord rec = null;
            if (edit)
            {
                var p = (arg ?? "").Split('|');
                rec = p.Length == 2 ? Sim.Records.FirstOrDefault(r => r.Name == p[0] && r.Type == p[1]) : null;
                if (rec == null) { Navigate("r53-zone", AwsSim.ZoneId, false); return; }
            }
            PageTitle = (edit ? "Editar registro" : "Criar registro") + " - Route 53";
            pg.Crumbs("Route 53", "Zonas hospedadas", AwsSim.Zone, edit ? "Editar registro" : "Criar registro");
            pg.Header(edit ? "Editar registro" : "Criar registro", "Zona: " + AwsSim.Zone);
            var types = new[] { "A", "AAAA", "CNAME", "MX", "TXT" };
            var tdesc = new[] { "A — endereço IPv4", "AAAA — endereço IPv6", "CNAME — apelido para outro nome", "MX — servidor de e-mail", "TXT — texto (verificações, SPF...)" };
            Panel box;
            var f = pg.Section("Registro", out box);
            string sub = edit ? (rec.Name == AwsSim.Zone ? "" : rec.Name.Substring(0, rec.Name.Length - AwsSim.Zone.Length - 1)) : "";
            var name = f.Field("Nome do registro", "Só o subdomínio: 'api' vira api." + AwsSim.Zone + " (vazio = o próprio domínio).", sub);
            name.ReadOnly = edit;
            var type = f.Select("Tipo de registro", tdesc, edit ? Array.IndexOf(types, rec.Type) : 0);
            type.Enabled = !edit;
            var val = f.Field("Valor", "A: um IPv4 (ex.: 203.0.113.10); CNAME: um nome (ex.: site.exemplo.com).", edit ? rec.Value : "");
            var ttl = f.Field("TTL (segundos)", "300 = 5 minutos; 86400 = 1 dia. TTL alto = menos consultas, mas mudanças demoram a valer.", edit ? rec.Ttl.ToString() : "300");
            pg.EndSection(box, f);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("r53-zone", AwsSim.ZoneId)), Btn(edit ? "Salvar" : "Criar registros", true, () =>
            {
                int tv = ToInt(ttl.Text, -1);
                if (tv < 0) { err.Text = "✖ O TTL precisa ser um número de segundos."; return; }
                var e = Sim.ChangeRecord(edit ? "UPSERT" : "CREATE", edit ? rec.Name : name.Text, types[Math.Max(0, type.SelectedIndex)], val.Text, tv);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("r53-zone", AwsSim.ZoneId, false);
                Flash("Registro " + AwsSim.Fqdn(edit ? rec.Name : name.Text) + " " + (edit ? "atualizado." : "criado."), OkGreen);
            }));
        }

        // ── CloudFront
        static string ViewerText(string v) { var x = AwsSim.ViewerPolicies.FirstOrDefault(p => p[0] == v); return x != null ? x[1] : v; }

        void PageDists(Flow pg)
        {
            PageTitle = "Distribuições - CloudFront";
            pg.Crumbs("CloudFront", "Distribuições");
            var list = Sim.Dists.ToList();
            pg.Header("Distribuições (" + list.Count + ")", "A CloudFront guarda cópias (cache) do site em centenas de pontos de presença: o cliente baixa do mais próximo.");
            pg.Table(new[] { "ID", "Nome de domínio", "Origem", "Status" }, new[] { 1.4f, 2f, 2.6f, 1f }, list.Select(d => new[] { d.Id, d.Domain, d.Origin, "● Ativado" }).ToList(), i => Navigate("cdn-dist", list[i].Id));
        }

        void PageDist(Flow pg)
        {
            var d = Sim.Distr(arg ?? "");
            if (d == null) { Navigate("cloudfront", null, false); return; }
            PageTitle = d.Id + " - CloudFront";
            pg.Crumbs("CloudFront", "Distribuições", d.Id);
            string id = d.Id;
            pg.Header(d.Id, "Nome de domínio: " + d.Domain + " · cdn." + AwsSim.Zone);
            Panel box;
            var g = pg.Section("Geral: configurações", out box);
            g.Text("Objeto raiz padrão: " + (d.Root.Length == 0 ? "(nenhum — https://" + d.Domain + "/ responde AccessDenied)" : d.Root), fBold, d.Root.Length == 0 ? ErrRed : Ink);
            g.Text("Objeto devolvido quando alguém acessa a raiz (/) do domínio.", fSmall, Muted);
            g.Buttons(Btn("Editar", false, () => Navigate("cdn-general-edit", id)));
            pg.EndSection(box, g);
            var lvB = new ListView[1];
            pg.SubHeader("Comportamentos (1)", Btn("Editar", false, () => { if (SelIdx(lvB, "o comportamento") >= 0) Navigate("cdn-behavior-edit", id); }));
            lvB[0] = pg.Table(new[] { "Padrão do caminho", "Origem", "Política de protocolo do visualizador" }, new[] { 1.2f, 2.4f, 2f }, new List<string[]> { new[] { "Padrão (*)", d.Origin, ViewerText(d.Viewer) } }, null);
            pg.SubHeader("Invalidações (" + d.Invalidations.Count + ")", Btn("Criar invalidação", true, () => Navigate("cdn-inval-create", id)));
            pg.Table(new[] { "Caminhos", "Status" }, new[] { 3f, 1f }, d.Invalidations.Select(x => new[] { x, "● Concluído" }).ToList(), null);
            pg.Text("Invalidar tira os arquivos do cache: a próxima visita busca a versão nova na origem. Até 1000 caminhos por mês são grátis (um curinga /* conta como um caminho).", fSmall, Muted);
        }

        void PageDistGeneral(Flow pg)
        {
            var d = Sim.Distr(arg ?? "");
            if (d == null) { Navigate("cloudfront", null, false); return; }
            PageTitle = "Editar configurações - CloudFront";
            pg.Crumbs("CloudFront", "Distribuições", d.Id, "Editar configurações");
            pg.Header("Editar configurações", d.Id);
            Panel box;
            var f = pg.Section("Configurações", out box);
            var root = f.Field("Objeto raiz padrão - opcional", "Nome do objeto, sem barra inicial (ex.: index.html).", d.Root);
            pg.EndSection(box, f);
            var err = pg.Error();
            string id = d.Id;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("cdn-dist", id)), Btn("Salvar alterações", true, () =>
            {
                var e = Sim.SetRootObject(id, root.Text);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("cdn-dist", id, false);
                Flash("Distribuição " + id + " atualizada (implantando nos pontos de presença).", OkGreen);
            }));
        }

        void PageDistBehavior(Flow pg)
        {
            var d = Sim.Distr(arg ?? "");
            if (d == null) { Navigate("cloudfront", null, false); return; }
            PageTitle = "Editar comportamento - CloudFront";
            pg.Crumbs("CloudFront", "Distribuições", d.Id, "Comportamentos", "Padrão (*)");
            pg.Header("Editar comportamento: Padrão (*)", "Origem: " + d.Origin);
            Panel box;
            var f = pg.Section("Visualizador", out box);
            var vp = f.Radios("Política de protocolo do visualizador", AwsSim.ViewerPolicies.Select(v => v[1]).ToArray(), Array.FindIndex(AwsSim.ViewerPolicies, v => v[0] == d.Viewer));
            f.Text("Redirecionar: quem chega por http:// recebe um 301 para https:// (links antigos continuam funcionando). Somente HTTPS: requisições http:// recebem erro 403.", fSmall, Muted);
            pg.EndSection(box, f);
            var err = pg.Error();
            string id = d.Id;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("cdn-dist", id)), Btn("Salvar alterações", true, () =>
            {
                int i = vp.FindIndex(r => r.Checked);
                if (i < 0) { err.Text = "✖ Escolha a política."; return; }
                var e = Sim.SetViewerPolicy(id, AwsSim.ViewerPolicies[i][0]);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("cdn-dist", id, false);
                Flash("Comportamento atualizado: " + AwsSim.ViewerPolicies[i][1] + ".", OkGreen);
            }));
        }

        void PageInvalidation(Flow pg)
        {
            var d = Sim.Distr(arg ?? "");
            if (d == null) { Navigate("cloudfront", null, false); return; }
            PageTitle = "Criar invalidação - CloudFront";
            pg.Crumbs("CloudFront", "Distribuições", d.Id, "Criar invalidação");
            pg.Header("Criar invalidação", d.Id);
            Panel box;
            var f = pg.Section("Detalhes da invalidação", out box);
            f.Text("Adicionar caminhos de objeto (um por linha). Ex.: /index.html, /css/* ou /* (tudo).", fBold, Ink);
            var tb = new TextBox { Multiline = true, AcceptsReturn = true, Font = fMono, Location = new Point(f.X, f.Y), Width = Math.Min(f.W, S(460)), Height = S(90) };
            box.Controls.Add(tb);
            f.Y += tb.Height + S(10);
            pg.EndSection(box, f);
            var err = pg.Error();
            string id = d.Id;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("cdn-dist", id)), Btn("Criar invalidação", true, () =>
            {
                var paths = tb.Text.Replace("\r", "").Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                if (paths.Count == 0) { err.Text = "✖ Informe pelo menos um caminho."; return; }
                var e = Sim.Invalidate(id, paths);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("cdn-dist", id, false);
                Flash("Invalidação criada: " + string.Join(" ", paths), OkGreen);
            }));
        }

        // ── CloudTrail
        void PageTrails(Flow pg)
        {
            PageTitle = "Trilhas - CloudTrail";
            pg.Crumbs("CloudTrail", "Trilhas");
            var list = Sim.Trails.ToList();
            var lvRef = new ListView[1];
            var act = MenuBtn("Ações  ▾",
                "Iniciar o registro em log", (Action)(() => { int i = SelIdx(lvRef, "uma trilha"); if (i >= 0) Done(Sim.StartLogging(list[i].Name), "Registro em log iniciado: " + list[i].Name + "."); }),
                "Interromper o registro em log", (Action)(() => { int i = SelIdx(lvRef, "uma trilha"); if (i >= 0 && Confirm("Interromper o registro da trilha " + list[i].Name + "? As ações na conta deixam de ser registradas.", "Interromper registro")) Done(Sim.StopLogging(list[i].Name), "Registro interrompido."); }),
                "Excluir", (Action)(() => { int i = SelIdx(lvRef, "uma trilha"); if (i >= 0 && Confirm("Excluir a trilha " + list[i].Name + "?", "Excluir trilha")) Done(Sim.DeleteTrail(list[i].Name), "Trilha excluída."); }));
            pg.Header("Trilhas (" + list.Count + ")", "Uma trilha grava no S3 quem fez o quê, quando e de onde em toda a conta (console, CLI e API) — por anos, não só os 90 dias do histórico de eventos.", act, Btn("Criar trilha", true, () => Navigate("trail-create", null)));
            lvRef[0] = pg.Table(new[] { "Nome", "Região inicial", "Trilha multirregional", "Bucket do S3", "Validação de arquivos", "Status do registro em log" }, new[] { 1.1f, 1f, 1.1f, 1.5f, 1.1f, 1.3f },
                list.Select(t => new[] { t.Name, "us-east-1", t.MultiRegion ? "Sim" : "Não", t.Bucket, t.Validation ? "Ativada" : "Desativada", t.Logging ? "● Registrando" : "● Desativado" }).ToList(), null);
        }

        void PageTrailCreate(Flow pg)
        {
            PageTitle = "Criar trilha - CloudTrail";
            pg.Crumbs("CloudTrail", "Trilhas", "Criar trilha");
            pg.Header("Criar trilha", null);
            var buckets = Sim.Buckets.Select(b => b.Name).ToList();
            Panel box;
            var f = pg.Section("Atributos gerais da trilha", out box);
            var name = f.Field("Nome da trilha", "3 a 128 caracteres.");
            var multi = f.Check("Aplicar a trilha a todas as regiões", true, "Registra também o que acontece nas outras regiões (onde um invasor costuma criar recursos escondidos).");
            f.Radios("Local de armazenamento", new[] { "Usar bucket do S3 existente" }, 0);
            var bk = f.Select("Bucket do S3 da trilha", buckets.ToArray(), -1);
            var val = f.Check("Validação do arquivo de log: Ativado", true, "Gera arquivos de resumo (digest) assinados: dá para provar que ninguém alterou ou apagou um log.");
            pg.EndSection(box, f);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("cloudtrail", null)), Btn("Criar trilha", true, () =>
            {
                if (bk.SelectedIndex < 0) { err.Text = "✖ Escolha o bucket do S3."; return; }
                var e = Sim.CreateTrail(name.Text, buckets[bk.SelectedIndex], multi.Checked, val.Checked, true);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("cloudtrail", null, false);
                Flash("Trilha " + name.Text.Trim() + " criada (pelo console, o registro em log já começa ativado).", OkGreen);
            }));
        }

        // ── KMS
        void PageKeysKms(Flow pg)
        {
            PageTitle = "Chaves gerenciadas pelo cliente - KMS";
            pg.Crumbs("KMS", "Chaves gerenciadas pelo cliente");
            var list = AwsSim.KmsKeys.Where(k => k[0] != "alias/aws/s3").ToList();
            pg.Header("Chaves gerenciadas pelo cliente (" + list.Count + ")", "Chaves que a empresa controla: quem pode usar (política da chave), rotação e desativação. As gerenciadas pela AWS (alias/aws/...) a AWS alterna sozinha.");
            pg.Table(new[] { "Aliases", "ID da chave", "Status", "Rotação automática" }, new[] { 1.4f, 2.6f, 0.9f, 1.2f },
                list.Select(k => new[] { k[0], AwsSim.KmsId(k[0]), "● Ativado", Sim.Rotation(k[0]) ? "● Ativado" : "● Desativado" }).ToList(), i => Navigate("kms-key", list[i][0]));
            pg.Text("Clique no alias para ver detalhes e configurar a rotação.", fSmall, Muted);
        }

        void PageKmsKey(Flow pg)
        {
            var k = AwsSim.KmsKeys.FirstOrDefault(x => x[0] == arg);
            if (k == null) { Navigate("kms", null, false); return; }
            PageTitle = k[0] + " - KMS";
            pg.Crumbs("KMS", "Chaves gerenciadas pelo cliente", k[0]);
            string al = k[0];
            pg.Header(k[0], k[1]);
            Panel box;
            var f = pg.Section("Configuração geral", out box);
            f.Text("ID da chave: " + AwsSim.KmsId(al), fUi, Ink);
            f.Text("ARN: arn:aws:kms:us-east-1:123456789012:key/" + AwsSim.KmsId(al) + " · Tipo: simétrica · Uso: criptografar e descriptografar", fSmall, Muted);
            pg.EndSection(box, f);
            var r = pg.Section("Rotação de chaves", out box);
            r.Text("Alternar automaticamente esta chave do KMS: " + (Sim.Rotation(al) ? "● Ativado (a cada 365 dias)" : "● Desativado"), fBold, Sim.Rotation(al) ? OkGreen : ErrRed);
            r.Text("Na rotação o KMS gera material criptográfico novo; o ID, o alias e as permissões continuam iguais, e os dados antigos continuam legíveis.", fSmall, Muted);
            r.Buttons(Btn("Editar", false, () => Navigate("kms-rotation-edit", al)));
            pg.EndSection(box, r);
        }

        void PageKmsRotation(Flow pg)
        {
            var k = AwsSim.KmsKeys.FirstOrDefault(x => x[0] == arg);
            if (k == null) { Navigate("kms", null, false); return; }
            PageTitle = "Rotação de chaves - KMS";
            pg.Crumbs("KMS", "Chaves gerenciadas pelo cliente", k[0], "Rotação de chaves");
            pg.Header("Editar rotação de chaves", k[0] + " (" + AwsSim.KmsId(k[0]) + ")");
            Panel box;
            var f = pg.Section("Rotação automática de chaves", out box);
            var on = f.Check("Alternar automaticamente esta chave do KMS", Sim.Rotation(k[0]), "Período de rotação: 365 dias.");
            pg.EndSection(box, f);
            var err = pg.Error();
            string al = k[0];
            pg.Buttons(Btn("Cancelar", false, () => Navigate("kms-key", al)), Btn("Salvar", true, () =>
            {
                var e = Sim.SetRotation(al, on.Checked);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("kms-key", al, false);
                Flash("Rotação automática de " + al + ": " + (on.Checked ? "ativada" : "desativada") + ".", OkGreen);
            }));
        }

        // ── Billing e Budgets
        void PageBudgets(Flow pg)
        {
            PageTitle = "Orçamentos - Billing";
            pg.Crumbs("Billing e Gerenciamento de Custos", "Orçamentos");
            var list = Sim.Budgets.ToList();
            pg.Header("Orçamentos (" + list.Count + ")", "Custo do mês até agora: US$ 214,37 · previsão para o fim do mês: US$ 412,90. Orçamentos avisam antes da fatura assustar.", Btn("Criar orçamento", true, () => Navigate("budget-create", null)));
            pg.Table(new[] { "Nome", "Valor orçado", "Custo real", "Alertas" }, new[] { 1.6f, 1.2f, 1.2f, 2.2f },
                list.Select(b => new[] { b.Name, "US$ " + b.Amount.ToString("0.00"), "US$ 214,37", b.Alerts.Count == 0 ? "nenhum" : string.Join(", ", b.Alerts.Select(a => a[1] + "% " + (a[0] == "ACTUAL" ? "real" : "previsto"))) }).ToList(),
                i => Navigate("budget-detail", list[i].Name));
        }

        void PageBudgetCreate(Flow pg)
        {
            PageTitle = "Criar orçamento - Billing";
            pg.Crumbs("Billing e Gerenciamento de Custos", "Orçamentos", "Criar orçamento");
            pg.Header("Criar orçamento", null);
            Panel box;
            var f = pg.Section("Tipo de orçamento", out box);
            f.Radios(null, new[] { "Orçamento de custo — mensal, valor fixo (US$)" }, 0);
            pg.EndSection(box, f);
            var g = pg.Section("Detalhes", out box);
            var name = g.Field("Nome do orçamento", null);
            var val = g.Field("Valor do orçamento mensal (US$)", "Ex.: 250.00");
            g.Text("Alertas por e-mail são adicionados depois, na página do orçamento.", fSmall, Muted);
            pg.EndSection(box, g);
            var err = pg.Error();
            pg.Buttons(Btn("Cancelar", false, () => Navigate("budgets", null)), Btn("Criar orçamento", true, () =>
            {
                float v;
                if (!float.TryParse(val.Text.Trim().Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v)) { err.Text = "✖ O valor precisa ser um número."; return; }
                var e = Sim.CreateBudget(name.Text, v);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("budgets", null, false);
                Flash("Orçamento " + name.Text.Trim() + " criado.", OkGreen);
            }));
        }

        void PageBudget(Flow pg)
        {
            var b = Sim.Budgets.FirstOrDefault(x => x.Name == arg);
            if (b == null) { Navigate("budgets", null, false); return; }
            PageTitle = b.Name + " - Billing";
            pg.Crumbs("Billing e Gerenciamento de Custos", "Orçamentos", b.Name);
            string bn = b.Name;
            pg.Header(b.Name, "Orçamento de custo mensal: US$ " + b.Amount.ToString("0.00") + " · custo real até agora: US$ 214,37");
            pg.SubHeader("Alertas (" + b.Alerts.Count + ")", Btn("Adicionar limite de alerta", true, () => Navigate("budget-alert-create", bn)));
            pg.Table(new[] { "Limite", "Tipo", "Destinatário" }, new[] { 1f, 1f, 2f }, b.Alerts.Select(a => new[] { a[1] + "% do orçado", a[0] == "ACTUAL" ? "Real" : "Previsto", a[2] }).ToList(), null);
        }

        void PageBudgetAlert(Flow pg)
        {
            var b = Sim.Budgets.FirstOrDefault(x => x.Name == arg);
            if (b == null) { Navigate("budgets", null, false); return; }
            PageTitle = "Adicionar alerta - Billing";
            pg.Crumbs("Billing e Gerenciamento de Custos", "Orçamentos", b.Name, "Adicionar alerta");
            pg.Header("Adicionar limite de alerta", "Orçamento: " + b.Name + " (US$ " + b.Amount.ToString("0.00") + ")");
            Panel box;
            var f = pg.Section("Limite de alerta", out box);
            var th = f.Field("Limite (% do valor orçado)", "Ex.: 80 = avisar quando o custo passar de 80% do orçamento.");
            var tp = f.Radios("Acionar quando o custo", new[] { "Real (o que já foi gasto)", "Previsto (a projeção para o fim do mês)" }, 0);
            var mail = f.Field("Destinatários de e-mail", null);
            pg.EndSection(box, f);
            var err = pg.Error();
            string bn = b.Name;
            pg.Buttons(Btn("Cancelar", false, () => Navigate("budget-detail", bn)), Btn("Salvar", true, () =>
            {
                float v;
                if (!float.TryParse(th.Text.Trim().Replace(',', '.').TrimEnd('%'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v)) { err.Text = "✖ O limite precisa ser um número (%)."; return; }
                var e = Sim.AddBudgetAlert(bn, tp[0].Checked ? "ACTUAL" : "FORECASTED", v, mail.Text);
                if (e != null) { err.Text = "✖ " + e; return; }
                Navigate("budget-detail", bn, false);
                Flash("Alerta de " + v + "% adicionado ao orçamento " + bn + ".", OkGreen);
            }));
        }
    }
}
