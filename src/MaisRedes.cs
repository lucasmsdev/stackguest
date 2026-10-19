using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerQuest
{
    static partial class Challenges
    {
        // ── auxiliares exclusivos da área Redes (nomes únicos para não colidir com outras áreas)
        // Gera as variações de ordem: head + partes em qualquer ordem, com o "alvo" (host/URL) no fim ou logo após o head.
        // A primeira variação é sempre a ordem original com o alvo no fim (vira a resposta de referência).
        static string[] RedesOrdens(string head, string alvo, params string[] partes)
        {
            var res = new List<string>();
            foreach (var p in RedesPermutar(partes.ToList()))
            {
                string meio = string.Join(" ", p);
                if (alvo == null) res.Add(head + " " + meio);
                else
                {
                    res.Add(head + " " + meio + " " + alvo);
                    res.Add(head + " " + alvo + " " + meio);
                }
            }
            return res.Distinct().ToArray();
        }

        static List<List<string>> RedesPermutar(List<string> itens)
        {
            var res = new List<List<string>>();
            if (itens.Count <= 1) { res.Add(new List<string>(itens)); return res; }
            for (int i = 0; i < itens.Count; i++)
            {
                var resto = new List<string>(itens);
                resto.RemoveAt(i);
                foreach (var r in RedesPermutar(resto))
                {
                    r.Insert(0, itens[i]);
                    res.Add(r);
                }
            }
            return res;
        }

        static string[] RedesJuntar(params string[][] listas)
        {
            return listas.SelectMany(x => x).Distinct().ToArray();
        }

        static partial void MaisRedes(List<Level> list)
        {
            const string N = "redes";
            Func<string, Level> L = t => list.First(l => l.Cat == N && l.Title == t);

            // 1) Rebalanceamento: o Profissional antigo tinha desafios de júnior.
            L("Porta do acesso remoto").Tier = 0;   // decorar a porta 22 é conceito de iniciante
            L("Hosts em /26").Tier = 1;             // aplicação direta da fórmula
            L("Só os cabeçalhos").Tier = 1;         // uma opção do curl
            L("Camada do roteador").Tier = 1;       // pergunta conceitual
            L("Liberando o HTTPS").Tier = 1;        // regra simples de firewall
            L("Faixa privada classe A").Tier = 1;   // RFC 1918 básica

            // ───────────── INICIANTE ─────────────
            list.Add(C(N, 0, "text", "Quem distribui os IPs?",
                "O Beto ligou um notebook novo na rede do Café 365 e ele recebeu IP, máscara, gateway e DNS sozinho, sem ninguém configurar nada.",
                "Qual protocolo entrega essas configurações de rede automaticamente para os dispositivos? (sigla)",
                A("DHCP"),
                A("Um servidor (normalmente o roteador) 'empresta' um IP por um tempo, o chamado lease.", "Sigla de quatro letras: começa com D e termina com P."),
                A("DHCP", "Dynamic Host Configuration Protocol: distribui IP, máscara, gateway e DNS automaticamente.",
                  "Lease", "O empréstimo do IP tem prazo; o cliente renova antes de vencer."),
                "Servidores e impressoras costumam ter IP fixo (ou reserva no DHCP) para não mudarem de endereço."));
            list.Add(C(N, 0, "text", "A própria máquina",
                "A Sandra sobe o Café 365 no notebook e diz: 'acesse pelo localhost'. Esse nome sempre aponta para a própria máquina.",
                "Qual é o endereço IPv4 de loopback (o IP do localhost)?",
                A("127.0.0.1"),
                A("Toda a faixa 127.0.0.0/8 é reservada para loopback.", "É o primeiro endereço utilizável da faixa 127: 127.0.0._"),
                A("127.0.0.1", "Endereço de loopback: o pacote nunca sai da placa de rede, volta para a própria máquina.",
                  "localhost", "Nome que aponta para 127.0.0.1 (e ::1 no IPv6)."),
                "Um serviço escutando só em 127.0.0.1 não é acessível por outras máquinas — ótimo para bancos de dados locais."));
            list.Add(C(N, 0, "cmd", "Limpando o cache DNS",
                "O site mudou de servidor e de IP, mas o seu Windows insiste no endereço antigo, que ficou guardado no cache de DNS.",
                "No Windows, limpe o cache de DNS da máquina.",
                A("ipconfig /flushdns"),
                A("O mesmo comando que mostra o seu IP no Windows tem uma opção para o cache de DNS.", "ipconfig /f____dns"),
                A("ipconfig", "Ferramenta de configuração de rede do Windows.", "/flushdns", "Apaga (flush) as respostas de DNS guardadas em cache."),
                "Para ver o que está no cache antes de limpar: ipconfig /displaydns."));
            list.Add(C(N, 0, "cmd", "Só 4 pacotes",
                "No Linux, o ping não para sozinho: fica enviando até alguém apertar Ctrl+C. A Sandra quer só uma amostra rápida.",
                "No Linux, envie exatamente 4 pacotes de ping para micorosoft.com.",
                A("ping -c 4 micorosoft.com", "ping micorosoft.com -c 4", "ping -c4 micorosoft.com"),
                A("Existe uma opção de contagem (count).", "ping -c N micorosoft.com"),
                A("ping", "Envia pacotes ICMP Echo.", "-c 4", "Count: envia só 4 pacotes e mostra o resumo.", "micorosoft.com", "Destino."),
                "No Windows o padrão já é 4 pacotes, e a opção de contagem é -n. Para ping contínuo no Windows: -t."));

            // ───────────── INTERMEDIÁRIO ─────────────
            list.Add(C(N, 1, "cmd", "Registros de e-mail",
                "Os e-mails enviados para @micorosoft.com estão voltando. A Clara quer saber quais servidores de e-mail o domínio anuncia no DNS.",
                "Consulte os registros MX de micorosoft.com (use dig, nslookup ou host).",
                RedesJuntar(
                    A("dig micorosoft.com MX", "dig MX micorosoft.com", "dig -t MX micorosoft.com", "dig micorosoft.com -t MX",
                      "dig micorosoft.com MX +short", "dig +short micorosoft.com MX", "dig MX micorosoft.com +short", "dig +short MX micorosoft.com",
                      "nslookup -type=mx micorosoft.com", "nslookup micorosoft.com -type=mx", "nslookup -query=mx micorosoft.com",
                      "nslookup micorosoft.com -query=mx", "nslookup -q=mx micorosoft.com", "nslookup -querytype=mx micorosoft.com",
                      "host -t mx micorosoft.com", "host -t MX micorosoft.com", "host micorosoft.com -t mx",
                      "Resolve-DnsName micorosoft.com -Type MX", "Resolve-DnsName -Name micorosoft.com -Type MX")),
                A("Cada registro DNS tem um tipo: A (IPv4), AAAA (IPv6), MX (e-mail), TXT (texto)...", "dig micorosoft.com MX   ou   nslookup -type=mx micorosoft.com"),
                A("dig / nslookup", "Ferramentas de consulta DNS.", "MX", "Mail eXchanger: servidores que recebem e-mail do domínio (com prioridade: menor = preferido).",
                  "micorosoft.com", "Domínio consultado."),
                "Sem registro MX correto o domínio não recebe e-mail. Confira também o TXT (SPF) para seus e-mails não caírem no spam."));

            // ───────────── PROFISSIONAL ─────────────
            list.Add(C(N, 2, "cmd", "Quem ocupa a 8080?",
                "O Café 365 não sobe no servidor Windows: 'porta 8080 já está em uso'. A Sandra quer o PID do processo culpado, sem rolar centenas de linhas.",
                "No Windows, liste todas as conexões em formato numérico mostrando o PID de cada uma (netstat) e filtre a saída só pelas linhas que contêm :8080 (findstr).",
                A("netstat -ano | findstr :8080", "netstat -ano | findstr \":8080\"", "netstat -ano | find \":8080\"", "netstat -ano | findstr 8080",
                  "netstat -ano|findstr :8080", "netstat -a -n -o | findstr :8080"),
                A("O netstat tem opções para 'todas' (-a), 'numérico' (-n) e 'dono/PID' (-o). O pipe | manda a saída para outro comando.", "netstat -ano | findstr :____"),
                A("netstat -ano", "Todas as conexões (-a), sem traduzir nomes (-n), com o PID do processo (-o).",
                  "|", "Pipe: a saída do netstat vira a entrada do próximo comando.",
                  "findstr :8080", "Mostra só as linhas que contêm :8080 (o 'grep' do Windows)."),
                "Com o PID em mãos: tasklist /FI \"PID eq 1234\" mostra o programa e taskkill /PID 1234 /F encerra. No Linux: ss -tlnp | grep :8080."));
            list.Add(C(N, 2, "text", "Prefixo para 500 hosts",
                "A rede Wi-Fi dos clientes das lojas vai ter até 500 dispositivos ao mesmo tempo. A Sandra não quer desperdiçar endereços.",
                "Qual é o MAIOR prefixo CIDR (a menor rede possível) que comporta 500 hosts utilizáveis? Responda no formato /N.",
                A("/23", "23", "255.255.254.0"),
                A("Hosts utilizáveis = 2^(bits de host) - 2. Ache a menor potência de 2 que, menos 2, chegue a 500.", "2^8 - 2 = 254 (pouco); 2^9 - 2 = 510 (cabe). 9 bits de host → 32 - 9 = ?"),
                A("9 bits de host", "2^9 - 2 = 510 endereços utilizáveis (8 bits dariam só 254).",
                  "/23", "32 - 9 = 23 bits de rede. Máscara 255.255.254.0."),
                "Planeje com folga: se a rede tende a crescer, um /22 (1022 hosts) evita renumerar tudo depois."));
            list.Add(C(N, 2, "text", "Broadcast da /27",
                "A câmera de segurança do caixa tem o IP 192.168.10.77/27. A Clara precisa configurar um alerta para o endereço de broadcast da sub-rede dela.",
                "Qual é o endereço de broadcast da sub-rede de 192.168.10.77/27?",
                A("192.168.10.95"),
                A("/27 = máscara 255.255.255.224: blocos de 32 endereços no último octeto (0, 32, 64, 96...).", "O 77 cai no bloco 64–95. O broadcast é o ÚLTIMO endereço do bloco."),
                A("/27", "Bloco de 256 - 224 = 32 endereços.",
                  "192.168.10.64", "Endereço de rede (início do bloco onde o 77 está).",
                  "192.168.10.95", "Broadcast: último endereço do bloco (64 + 32 - 1). Hosts: .65 a .94."),
                "Atalho: tamanho do bloco = 256 - (valor da máscara no octeto 'interessante'). Rede = múltiplo do bloco logo abaixo do IP."));
            list.Add(C(N, 2, "text", "Rede de um /22",
                "Um servidor de pagamentos tem o IP 172.20.13.200/22. Para criar a rota estática, o Beto precisa do endereço de rede.",
                "Qual é o endereço de rede de 172.20.13.200/22? (só o IP)",
                A("172.20.12.0", "172.20.12.0/22"),
                A("/22 = 255.255.252.0: o octeto 'interessante' é o TERCEIRO, com blocos de 4 (0, 4, 8, 12, 16...).", "O 13 está no bloco que começa em 12; o último octeto da rede é 0."),
                A("/22", "Máscara 255.255.252.0: bloco de 256 - 252 = 4 no terceiro octeto.",
                  "172.20.12.0", "Rede: o 13 cai no bloco 12–15, e o quarto octeto zera.",
                  "172.20.15.255", "Broadcast do mesmo bloco (para conferência)."),
                "Em prefixos menores que /24 a conta acontece no terceiro (ou segundo) octeto — erro clássico é calcular no último."));
            list.Add(C(N, 2, "text", "Último host da /21",
                "A rede dos quiosques de autoatendimento tem um equipamento com IP 10.8.37.19/21. O gateway vai ficar no último IP utilizável dessa sub-rede.",
                "Qual é o último endereço utilizável para hosts na sub-rede de 10.8.37.19/21?",
                A("10.8.39.254"),
                A("/21 = 255.255.248.0: blocos de 8 no terceiro octeto. Ache a rede e o broadcast primeiro.", "O 37 cai no bloco 32–39. Broadcast = 10.8.39.255; o último host vem logo antes."),
                A("/21", "Máscara 255.255.248.0: bloco de 8 no terceiro octeto.",
                  "10.8.32.0 – 10.8.39.255", "Rede e broadcast do bloco onde o 37 está.",
                  "10.8.39.254", "Último utilizável = broadcast - 1. O primeiro é 10.8.32.1."),
                "Convenção comum: gateway no primeiro ou no último IP utilizável. Documente a escolha para a equipe toda seguir."));
            list.Add(C(N, 2, "cmd", "TXT no DNS certo",
                "Os e-mails do Café 365 caem no spam. A Clara suspeita do registro SPF (que fica em um registro TXT) e não quer depender do cache do DNS local.",
                "Com o dig, consulte os registros TXT de micorosoft.com diretamente no servidor DNS 1.1.1.1, mostrando só a resposta curta.",
                RedesJuntar(
                    RedesOrdens("dig", null, "@1.1.1.1", "micorosoft.com", "TXT", "+short"),
                    RedesOrdens("dig", null, "@1.1.1.1", "micorosoft.com", "-t TXT", "+short")),
                A("No dig: @servidor escolhe o DNS consultado, o tipo do registro vai como argumento e +short enxuga a saída.", "dig @1.1.1.1 micorosoft.com ___ +short"),
                A("@1.1.1.1", "Pergunta direto para esse servidor DNS (ignora o DNS configurado na máquina).",
                  "micorosoft.com TXT", "Domínio e tipo de registro (TXT guarda SPF, verificações de domínio etc.).",
                  "+short", "Mostra só os valores, sem o cabeçalho e as seções do dig."),
                "Comparar a resposta de vários servidores (1.1.1.1, 8.8.8.8 e o DNS autoritativo) ajuda a descobrir se o problema é propagação ou configuração."));
            list.Add(C(N, 2, "cmd", "Inspecionando o TLS",
                "Alguns clientes recebem 'certificado inválido'. O servidor hospeda vários sites no mesmo IP, então ele só sabe qual certificado entregar se o cliente informar o nome (SNI).",
                "Com o openssl, abra uma conexão TLS com micorosoft.com na porta 443, informando também o nome do servidor (SNI) micorosoft.com.",
                A("openssl s_client -connect micorosoft.com:443 -servername micorosoft.com"),
                A("O subcomando do openssl que age como cliente TLS é o s_client.", "openssl s_client -connect HOST:PORTA -servername HOST"),
                A("openssl s_client", "Cliente TLS de diagnóstico: mostra a cadeia de certificados, o protocolo e a cifra negociados.",
                  "-connect micorosoft.com:443", "Host e porta, juntos com dois-pontos.",
                  "-servername micorosoft.com", "Envia o SNI: o servidor escolhe o certificado do site certo."),
                "Para ver as datas de validade: ... | openssl x509 -noout -dates. Certificado vencido é uma das causas mais comuns de 'site fora do ar'."));
            list.Add(C(N, 2, "cmd", "SSH só da rede interna",
                "A Clara viu milhares de tentativas de login por SSH vindas da internet. Decisão: a porta 22 só pode aceitar conexões da rede interna 10.0.0.0/24.",
                "Com o iptables, ACRESCENTE no fim da cadeia INPUT uma regra que aceita conexões TCP na porta de destino 22 somente quando a origem for 10.0.0.0/24.",
                RedesJuntar(
                    A("sudo iptables -A INPUT -p tcp --dport 22 -s 10.0.0.0/24 -j ACCEPT",
                      "iptables -A INPUT -p tcp --dport 22 -s 10.0.0.0/24 -j ACCEPT",
                      "sudo iptables -A INPUT -p tcp -m tcp --dport 22 -s 10.0.0.0/24 -j ACCEPT",
                      "iptables -A INPUT -p tcp -m tcp --dport 22 -s 10.0.0.0/24 -j ACCEPT",
                      "sudo iptables -A INPUT -s 10.0.0.0/24 -p tcp --destination-port 22 -j ACCEPT",
                      "iptables -A INPUT -s 10.0.0.0/24 -p tcp --destination-port 22 -j ACCEPT",
                      "sudo iptables --append INPUT --protocol tcp --dport 22 --source 10.0.0.0/24 --jump ACCEPT",
                      "iptables --append INPUT --protocol tcp --dport 22 --source 10.0.0.0/24 --jump ACCEPT")),
                A("Partes da regra: cadeia (-A INPUT), protocolo (-p), porta de destino (--dport), origem (-s) e ação (-j).", "iptables -A INPUT -p tcp --dport 22 -s REDE -j ACCEPT"),
                A("-A INPUT", "Append: acrescenta no fim da cadeia de entrada (-I inseriria no começo).",
                  "-p tcp --dport 22", "Protocolo TCP com porta de destino 22 (SSH).",
                  "-s 10.0.0.0/24", "Só quando a origem for a rede interna.",
                  "-j ACCEPT", "Ação: aceitar o pacote."),
                "Sozinha a regra não bloqueia nada: depois dela é preciso um DROP para a porta 22 (ou a política padrão DROP). E cuidado para não se trancar fora do servidor!"));
            list.Add(C(N, 2, "cmd", "Túnel até o MySQL",
                "O MySQL do servidor 10.0.0.5 só escuta em localhost (porta 3306) e o firewall não libera essa porta. O Beto precisa abrir o banco no programa gráfico da máquina dele.",
                "Crie um túnel SSH: a porta local 3307 da sua máquina deve encaminhar para localhost:3306 do lado do servidor 10.0.0.5, entrando com o usuário admin.",
                RedesJuntar(
                    RedesOrdens("ssh", "admin@10.0.0.5", "-L 3307:localhost:3306"),
                    RedesOrdens("ssh", "admin@10.0.0.5", "-L 3307:127.0.0.1:3306"),
                    RedesOrdens("ssh", "admin@10.0.0.5", "-N", "-L 3307:localhost:3306"),
                    RedesOrdens("ssh", "admin@10.0.0.5", "-N", "-L 3307:127.0.0.1:3306")),
                A("Encaminhamento de porta LOCAL no ssh usa a opção -L com o formato porta_local:destino:porta_destino.", "ssh -L 3307:localhost:____ admin@10.0.0.5"),
                A("-L 3307:localhost:3306", "Tudo que chegar na sua porta 3307 vai, criptografado, até o servidor e lá para localhost:3306.",
                  "localhost", "É resolvido do lado do SERVIDOR: ou seja, o próprio 10.0.0.5.",
                  "admin@10.0.0.5", "Usuário e servidor SSH."),
                "Depois é só conectar o cliente MySQL em 127.0.0.1:3307. Use -N para abrir só o túnel, sem shell. O banco continua fechado para a internet."));
            list.Add(C(N, 2, "cmd", "POST com JSON",
                "A API de pedidos do Café 365 recebe JSON. Antes de liberar o app, a Sandra quer testar a criação de um pedido direto do terminal.",
                "Com o curl, envie para https://api.micorosoft.com/pedidos uma requisição com o método POST explícito, o cabeçalho Content-Type: application/json e o corpo {\"item\":\"latte\"} (sem espaços, entre aspas simples).",
                RedesJuntar(
                    RedesOrdens("curl", "https://api.micorosoft.com/pedidos", "-X POST", "-H \"Content-Type: application/json\"", "-d '{\"item\":\"latte\"}'"),
                    RedesOrdens("curl", "https://api.micorosoft.com/pedidos", "--request POST", "--header \"Content-Type: application/json\"", "--data '{\"item\":\"latte\"}'"),
                    RedesOrdens("curl", "https://api.micorosoft.com/pedidos", "-X POST", "-H \"Content-Type: application/json\"", "--data-raw '{\"item\":\"latte\"}'")),
                A("Três opções do curl: método (-X), cabeçalho (-H) e corpo (-d). Valores com espaço ou aspas vão entre aspas.", "curl -X POST -H \"Content-Type: application/json\" -d '...' URL"),
                A("-X POST", "Define o método HTTP.",
                  "-H \"Content-Type: application/json\"", "Cabeçalho avisando que o corpo é JSON (sem ele, o -d envia como formulário).",
                  "-d '{\"item\":\"latte\"}'", "Corpo da requisição; aspas simples protegem as aspas duplas do JSON.",
                  "https://api.micorosoft.com/pedidos", "Endpoint da API."),
                "Some -i para ver o status (201 Created?) e os cabeçalhos da resposta. Em APIs com login: -H \"Authorization: Bearer TOKEN\"."));
        }
    }

    static partial class Docs
    {
        static partial void DocsMaisRedes(List<DocEntry> all)
        {
            const string N = "Redes";
            all.Add(X(N, "Conceitos", "DHCP", "Protocolo que entrega IP, máscara, gateway e DNS automaticamente aos dispositivos da rede.",
                "Processo DORA: Discover → Offer → Request → Acknowledge",
                A("lease", "tempo de 'empréstimo' do IP", "reserva", "o mesmo IP sempre para um MAC específico"),
                A("ipconfig /renew", "Windows: pede um novo IP ao servidor DHCP", "ipconfig /release", "Windows: devolve o IP atual"),
                "Sem resposta do DHCP, o Windows se dá um IP 169.254.x.x (APIPA) — sinal de problema na rede."));
            all.Add(X(N, "Conceitos", "Endereços especiais (loopback, APIPA...)", "Alguns endereços IPv4 têm uso reservado.",
                "127.0.0.0/8  loopback (a própria máquina)\n169.254.0.0/16  APIPA (DHCP falhou)\n0.0.0.0  'todas as interfaces' (em servidores)",
                A("localhost", "nome que aponta para o loopback (127.0.0.1; ::1 no IPv6)"),
                A("ping localhost", "testa a pilha de rede da própria máquina"),
                "Um serviço que escuta só no loopback não aceita conexões de outras máquinas."));
            all.Add(X(N, "Diagnóstico", "ipconfig  (cache de DNS e DHCP)", "No Windows, o ipconfig também gerencia o cache de DNS e a concessão do DHCP.",
                "ipconfig /OPÇÃO",
                A("/all", "todos os detalhes", "/displaydns", "mostra o cache de DNS", "/flushdns", "apaga o cache de DNS", "/release  /renew", "devolve / pede IP ao DHCP"),
                A("ipconfig /displaydns", "lista os nomes guardados em cache"), "No Linux com systemd: resolvectl flush-caches."));
            all.Add(X(N, "Diagnóstico", "dig / nslookup  (tipos de registro)", "Consulta um tipo específico de registro DNS e/ou um servidor DNS específico.",
                "dig [@SERVIDOR] DOMINIO TIPO [+short]\nnslookup -type=TIPO DOMINIO [SERVIDOR]\nhost -t TIPO DOMINIO",
                A("A / AAAA", "endereço IPv4 / IPv6", "MX", "servidores de e-mail do domínio", "TXT", "textos (SPF, verificações)", "NS", "servidores DNS do domínio",
                  "CNAME", "apelido para outro nome", "@SERVIDOR", "(dig) pergunta a esse servidor DNS", "+short", "(dig) só os valores", "-t TIPO", "(dig/host) tipo do registro"),
                A("dig escola.edu.br NS", "servidores DNS da escola", "dig @9.9.9.9 livraria.com AAAA +short", "IPv6 da livraria, perguntando ao 9.9.9.9",
                  "nslookup -type=ns escola.edu.br", "o mesmo com nslookup"),
                "No PowerShell: Resolve-DnsName DOMINIO -Type TIPO."));
            all.Add(X(N, "Diagnóstico", "netstat -ano + findstr  (quem usa a porta?)", "No Windows, mostra o PID do processo dono de cada conexão e filtra pela porta.",
                "netstat -ano | findstr :PORTA",
                A("-a", "todas as conexões e portas em escuta", "-n", "números (sem resolver nomes)", "-o", "mostra o PID (dono)", "|", "pipe: manda a saída para outro comando",
                  "findstr TEXTO", "filtra linhas que contêm o texto (como o grep)"),
                A("netstat -ano | findstr :3389", "quem usa a porta 3389", "tasklist /FI \"PID eq 4321\"", "qual programa tem o PID 4321", "taskkill /PID 4321 /F", "encerra o processo"),
                "No Linux: ss -tlnp | grep :PORTA (o -p mostra o processo)."));
            all.Add(X(N, "Conceitos", "Calculando sub-redes (rede, broadcast, faixa)", "Passo a passo para descobrir a rede, o broadcast e os hosts utilizáveis de um IP/prefixo.",
                "1) Tamanho do bloco = 256 - valor da máscara no octeto 'interessante'\n2) Rede = múltiplo do bloco igual ou logo abaixo do IP\n3) Broadcast = próxima rede - 1\n4) Hosts: da rede + 1 até o broadcast - 1\nhosts utilizáveis = 2^(32 - N) - 2",
                A("/21", "255.255.248.0 (bloco 8 no 3º octeto)", "/22", "255.255.252.0 (bloco 4 no 3º octeto)", "/23", "255.255.254.0 (bloco 2 no 3º octeto)",
                  "/26", "255.255.255.192 (bloco 64, 62 hosts)", "/27", "255.255.255.224 (bloco 32, 30 hosts)", "/28", "255.255.255.240 (bloco 16, 14 hosts)",
                  "/29", "255.255.255.248 (bloco 8, 6 hosts)", "/30", "255.255.255.252 (bloco 4, 2 hosts)"),
                A("10.0.5.150/26", "bloco 64 → rede 10.0.5.128, broadcast 10.0.5.191, hosts .129 a .190",
                  "172.16.9.10/22", "bloco 4 no 3º octeto → rede 172.16.8.0, broadcast 172.16.11.255, hosts 172.16.8.1 a 172.16.11.254",
                  "precisa de 100 hosts", "2^7 - 2 = 126 → 7 bits de host → /25"),
                "Prefixo menor que /24: a conta é feita no 3º octeto (e o 4º vai de 0 na rede a 255 no broadcast)."));
            all.Add(X(N, "Acesso e segurança", "openssl s_client  (testar TLS)", "Abre uma conexão TLS e mostra certificado, cadeia, protocolo e cifra.",
                "openssl s_client -connect HOST:PORTA [-servername NOME]",
                A("-connect HOST:PORTA", "onde conectar", "-servername NOME", "envia o SNI (nome do site) — essencial quando vários sites dividem o IP",
                  "-showcerts", "mostra toda a cadeia de certificados"),
                A("openssl s_client -connect livraria.com:443", "conecta sem SNI", "openssl s_client -connect 10.1.1.9:8443 -servername loja.exemplo.org", "conecta no IP enviando o nome do site"),
                "Validade: ... | openssl x509 -noout -dates"));
            all.Add(X(N, "Acesso e segurança", "iptables", "Firewall clássico do Linux: regras em cadeias (INPUT, OUTPUT, FORWARD).",
                "sudo iptables -A CADEIA [-p PROTOCOLO] [--dport PORTA] [-s ORIGEM] -j AÇÃO",
                A("-A CADEIA", "acrescenta a regra no fim", "-I CADEIA", "insere no começo", "-p tcp|udp", "protocolo", "--dport PORTA", "porta de destino",
                  "-s REDE", "origem (IP ou CIDR)", "-j ACCEPT|DROP|REJECT", "ação", "-L -n", "lista as regras"),
                A("sudo iptables -A INPUT -p tcp --dport 80 -j ACCEPT", "libera HTTP para todos", "sudo iptables -A INPUT -p udp --dport 161 -s 192.168.5.0/24 -j ACCEPT", "SNMP só da rede 192.168.5.0/24",
                  "sudo iptables -A INPUT -p tcp --dport 23 -j DROP", "descarta telnet"),
                "A ordem importa: a primeira regra que combinar decide. As regras somem no reboot se não forem salvas."));
            all.Add(X(N, "Acesso e segurança", "ssh -L  (túnel / encaminhamento de porta)", "Leva uma porta da sua máquina, criptografada, até um destino visto a partir do servidor SSH.",
                "ssh -L PORTA_LOCAL:DESTINO:PORTA_DESTINO USUARIO@SERVIDOR",
                A("-L", "encaminhamento local", "-N", "não abre shell, só mantém o túnel", "-p", "porta do SSH, se não for 22", "-i", "arquivo de chave privada"),
                A("ssh -L 9000:localhost:5432 ana@192.168.0.30", "Postgres do servidor acessível em localhost:9000", "ssh -N -L 8081:intranet.local:80 rui@bastion.escola.edu", "site interno via servidor de salto"),
                "O DESTINO é resolvido do lado do servidor: localhost ali é o próprio servidor."));
            all.Add(X(N, "Acesso e segurança", "curl  (APIs: método, cabeçalhos e corpo)", "Monta requisições completas para testar APIs pelo terminal.",
                "curl -X MÉTODO -H \"Nome: valor\" -d 'CORPO' URL",
                A("-X, --request", "método (GET, POST, PUT, DELETE)", "-H, --header", "acrescenta um cabeçalho", "-d, --data", "corpo da requisição", "-i", "mostra status e cabeçalhos da resposta"),
                A("curl -X PUT -H \"Content-Type: application/json\" -d '{\"titulo\":\"Dom Casmurro\"}' https://livraria.exemplo/livros/7", "atualiza um livro com JSON",
                  "curl -H \"Authorization: Bearer abc123\" https://escola.exemplo/notas", "GET autenticado"),
                "Use aspas simples em volta do JSON para as aspas duplas internas não serem 'comidas' pelo shell."));
        }
    }
}
