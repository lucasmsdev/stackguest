using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerQuest
{
    static partial class Challenges
    {
        const string MaisDockerCap = "Capítulo 8 · Operação Avançada";

        static Level MaisDockerL(int tier, string title, string ctx, string obj, string[] ans, string[] hints,
                                 string[] explain, string summary, string output)
        {
            return new Level
            {
                Cat = "docker", Kind = "docker", Tier = tier, Chapter = MaisDockerCap, Title = title, Context = ctx,
                Objective = obj, Answers = ans, Hints = hints, Explain = explain, Summary = summary, Output = output,
                Ignore = new string[0]
            };
        }

        static partial void MaisDocker(List<Level> list)
        {
            // ── 1) rebalanceamento dos 63 desafios originais: 25 Iniciante / 23 Intermediário / 15 Profissional
            var iniciante = new HashSet<string> {
                "Checando a instalação", "Raio-X do sistema", "Sua primeira imagem", "Inventário de imagens", "Hello, World!",
                "Quem está rodando?", "Containers fantasmas", "Rodando em segundo plano", "Container descartável",
                "Pausa para o café", "De volta ao trabalho", "Reinício rápido", "Lendo os logs", "Logs ao vivo",
                "Raio-X do container", "Monitor de recursos", "Processos internos", "Novo nome", "Faxina pontual",
                "Remoção forçada", "Removendo imagem", "Criando um volume", "Listando volumes", "Criando uma rede", "Listando redes"
            };
            var profissional = new HashSet<string> {
                "Dieta de recursos", "Dockerfile alternativo", "Build do zero", "Snapshot de container", "Banco persistente",
                "Edição ao vivo (bind mount)", "Cache na rede", "Conectando o banco", "Rebuild e sobe", "Consultando o banco",
                "Escalando a API", "Arquivo de produção", "Derrubando tudo", "Faxina geral", "MISSÃO FINAL: deploy em produção"
            };
            foreach (var lv in list.Where(l => l.Cat == "docker"))
                lv.Tier = iniciante.Contains(lv.Title) ? 0 : profissional.Contains(lv.Title) ? 2 : 1;

            // ── 2) novos desafios — INTERMEDIÁRIO
            list.Add(MaisDockerL(1, "Exec com outro usuário",
                "A Lia, de QA, está revisando a segurança do container 'vitrine' (nginx) e quer saber se os comandos de manutenção podem rodar sem ser como root. O Rafael pede que você teste com o usuário sem privilégios que a própria imagem do nginx já traz.",
                "Sem modo interativo, execute o comando 'whoami' dentro do container 'vitrine', rodando-o como o usuário 'nginx'.",
                A("docker exec -u nginx vitrine whoami", "docker exec --user nginx vitrine whoami"),
                A("O 'docker exec' tem uma opção para escolher com qual usuário o comando roda (por padrão é o usuário da imagem, muitas vezes root).",
                  "docker exec -u <usuário> <container> <comando>  — a opção vem ANTES do nome do container."),
                A("docker exec", "Executa um novo processo dentro do container em execução.",
                  "-u nginx  (--user)", "Roda o processo como o usuário 'nginx' (também aceita UID ou UID:GID, ex.: -u 101:101).",
                  "vitrine", "Container alvo.",
                  "whoami", "Comando executado lá dentro: imprime o nome do usuário atual."),
                "Tudo que vem DEPOIS do nome do container é o comando executado lá dentro. Por isso 'docker exec vitrine -u nginx whoami' não funciona: o '-u' seria entregue ao processo, não ao Docker. O inverso também vale: 'docker exec -u root ...' entra como root num container que roda sem privilégios.",
                "nginx"));

            list.Add(MaisDockerL(1, "Caçando por imagem",
                "Depois de semanas de testes, o Jorge (SRE) suspeita que existem vários containers esquecidos criados a partir do nginx, alguns rodando e outros parados. Ele quer a lista completa antes de decidir o que apagar.",
                "Liste TODOS os containers (inclusive os parados) que foram criados a partir da imagem 'nginx', usando um filtro do próprio Docker.",
                A("docker ps -a --filter ancestor=nginx", "docker ps -a -f ancestor=nginx"),
                A("Combine a flag que mostra containers parados com a opção de filtro do 'docker ps'. O filtro tem o formato CHAVE=VALOR.",
                  "O filtro que seleciona pela imagem de origem se chama 'ancestor': --filter ancestor=<imagem>"),
                A("docker ps", "Lista containers.",
                  "-a  (--all)", "Inclui os parados; sem ele, só os em execução entrariam na lista.",
                  "--filter ancestor=nginx  (-f)", "Mantém apenas os containers criados a partir da imagem 'nginx' (ou de uma imagem derivada dela)."),
                "Filtros podem ser repetidos e combinados: docker ps -a --filter ancestor=nginx --filter status=exited. Acrescente -q para ter só os IDs e usá-los em outro comando: docker rm $(docker ps -aq --filter ancestor=nginx --filter status=exited).",
@"CONTAINER ID   IMAGE   COMMAND                  CREATED       STATUS                   PORTS                  NAMES
d3f5a7c9e1b2   nginx   ""/docker-entrypoint.…""   3 days ago    Up 3 days                80/tcp                 vitrine
c2a8e4f6b0d1   nginx   ""/docker-entrypoint.…""   2 weeks ago   Exited (0) 9 days ago                           vigia
7b2e9d4f1c3a   nginx   ""/docker-entrypoint.…""   2 weeks ago   Exited (137) 2 weeks ago                        teste-proxy"));

            // ── 3) novos desafios — PROFISSIONAL (do mais simples para o mais complexo)
            list.Add(MaisDockerL(2, "Janela do incidente",
                "Incidente! O Jorge (SRE) registrou que a API (container 'api') devolveu erros 500 hoje, 08/10/2026, entre 14:00 e 14:30 (UTC). O log tem centenas de milhares de linhas e ele quer exatamente esse intervalo para o relatório pós-incidente.",
                "Mostre os logs do container 'api' somente do intervalo entre 2026-10-08T14:00:00 e 2026-10-08T14:30:00 (escreva as datas exatamente nesse formato).",
                A("docker logs --since 2026-10-08T14:00:00 --until 2026-10-08T14:30:00 api"),
                A("O 'docker logs' aceita um início ('a partir de quando') e um fim ('até quando'). Os dois aceitam datas completas ou tempos relativos (10m, 2h).",
                  "--since <início> --until <fim>"),
                A("docker logs", "Mostra os logs do container.",
                  "--since 2026-10-08T14:00:00", "Só linhas registradas a partir desse instante (formato RFC 3339: AAAA-MM-DDTHH:MM:SS).",
                  "--until 2026-10-08T14:30:00", "Só linhas registradas até esse instante.",
                  "api", "Container alvo."),
                "Também funciona com tempos relativos: '--since 2h --until 1h' é a hora retrasada. Junte '-t' (timestamps) para ver o horário de cada linha, e lembre que o Docker usa UTC por padrão — confira o fuso antes de recortar.",
@"2026-10-08T14:02:11Z GET /pedidos 200 41ms
2026-10-08T14:07:53Z ERROR connect ECONNREFUSED db:5432
2026-10-08T14:07:53Z POST /pedidos 500 3012ms
2026-10-08T14:21:40Z WARN pool de conexões esgotado (10/10)
2026-10-08T14:29:58Z GET /cardapio 200 18ms"));

            list.Add(MaisDockerL(2, "Ajuste a quente",
                "Sexta-feira, 18h: o container 'api' está sendo morto pelo OOM killer porque o limite de memória ficou pequeno. O Rafael não quer recriar o container agora (perderia a janela de deploy) — os limites precisam mudar com ele rodando.",
                "Sem recriar o container 'api', altere seus limites para 512m de memória, 512m de memória+swap (ou seja, nada de swap extra) e 1.5 CPU.",
                A("docker update --memory 512m --memory-swap 512m --cpus 1.5 api", "docker update -m 512m --memory-swap 512m --cpus 1.5 api"),
                A("Existe um comando que ATUALIZA a configuração de recursos de um container existente. As opções têm os mesmos nomes das do 'docker run'.",
                  "docker update --memory ... --memory-swap ... --cpus ... <container>"),
                A("docker update", "Altera limites de recursos (e a política de restart) de um container existente, sem recriá-lo.",
                  "--memory 512m  (-m)", "Novo teto de RAM.",
                  "--memory-swap 512m", "Limite de memória + swap somados. Igual ao --memory significa ZERO de swap: o container não 'foge' para o disco e fica lento.",
                  "--cpus 1.5", "Até um núcleo e meio de CPU.",
                  "api", "Container alvo."),
                "Pegadinha clássica: aumentar só o --memory acima do --memory-swap atual dá erro ('Memory limit should be smaller than already set memoryswap limit'). Por isso os dois são ajustados juntos. Depois, confira com 'docker stats --no-stream api' e registre o novo limite no compose/IaC, senão ele se perde no próximo deploy.",
                "api"));

            list.Add(MaisDockerL(2, "Build parametrizado",
                "O Dockerfile do Café 365 virou multi-stage: o estágio 'dev' tem ferramentas de debug e o estágio 'producao' gera a imagem enxuta. A versão do Node é definida por 'ARG NODE_VERSION' logo no início do arquivo. O Rafael quer testar a nova versão 22 do Node.",
                "A partir do diretório atual, construa a imagem 'cafeapp:2.0' parando no estágio 'producao' e passando o argumento de build NODE_VERSION com o valor 22.",
                A("docker build --target producao --build-arg NODE_VERSION=22 -t cafeapp:2.0 ."),
                A("Duas opções do 'docker build': uma escolhe até qual estágio construir e a outra preenche um ARG do Dockerfile.",
                  "--target <estágio> --build-arg CHAVE=VALOR -t <nome:tag> ."),
                A("docker build", "Constrói a imagem.",
                  "--target producao", "Constrói só até o estágio 'FROM ... AS producao' (estágios que ele não usa são pulados).",
                  "--build-arg NODE_VERSION=22", "Define o valor do 'ARG NODE_VERSION' durante o build (ex.: FROM node:${NODE_VERSION}-alpine).",
                  "-t cafeapp:2.0", "Nome:tag da imagem gerada.",
                  ".", "Contexto de build."),
                "Nunca passe senhas ou tokens por --build-arg: o valor fica gravado no histórico da imagem ('docker history'). Para segredos de build use '--secret' do BuildKit. E um ARG declarado antes do FROM precisa ser redeclarado (ARG NODE_VERSION) dentro do estágio para ser usado lá.",
@"[+] Building 31.7s (14/14) FINISHED
 => [internal] load build definition from Dockerfile
 => [deps 1/3] FROM docker.io/library/node:22-alpine
 => [deps 3/3] RUN npm ci --omit=dev
 => [producao 2/3] COPY --from=deps /app/node_modules ./node_modules
 => [producao 3/3] COPY . .
 => exporting to image
 => => naming to docker.io/library/cafeapp:2.0"));

            list.Add(MaisDockerL(2, "Rede sob medida",
                "O time de pagamentos exige IPs previsíveis: o firewall da operadora de cartões só libera a faixa 172.30.0.0/16. A rede padrão do Docker escolhe a sub-rede sozinha, então o Rafael pede uma rede com endereçamento definido por você.",
                "Crie a rede 'pagamentos-net' (driver padrão, bridge) com a sub-rede 172.30.0.0/16 e o gateway 172.30.0.1.",
                A("docker network create --subnet 172.30.0.0/16 --gateway 172.30.0.1 pagamentos-net",
                  "docker network create --driver bridge --subnet 172.30.0.0/16 --gateway 172.30.0.1 pagamentos-net",
                  "docker network create -d bridge --subnet 172.30.0.0/16 --gateway 172.30.0.1 pagamentos-net",
                  "docker network create pagamentos-net --subnet 172.30.0.0/16 --gateway 172.30.0.1"),
                A("O 'docker network create' aceita opções de endereçamento IP (IPAM): uma para a faixa (notação CIDR) e outra para o endereço do roteador.",
                  "--subnet <faixa/CIDR> --gateway <IP> <nome-da-rede>"),
                A("docker network create", "Cria uma rede definida pelo usuário (bridge por padrão).",
                  "--subnet 172.30.0.0/16", "Faixa de IPs da rede: 172.30.x.x (65.534 endereços utilizáveis).",
                  "--gateway 172.30.0.1", "IP do gateway (a ponte no host) — precisa estar dentro da sub-rede.",
                  "pagamentos-net", "Nome da rede."),
                "Com a sub-rede definida, você pode até fixar o IP de um container: docker run --network pagamentos-net --ip 172.30.0.10 ... (o --ip só funciona em redes com --subnet). Cuidado para não escolher uma faixa que já existe na rede da empresa ou da VPN: dá conflito de rota.",
                "8e2c4a6b0d1f3e5a7c9b2d4f6e8a0c1b3d5f7e9a2c4b6d8f0e1a3c5b7d9f2e4a"));

            list.Add(MaisDockerL(2, "Container blindado",
                "O relatório de pentest da Lia apontou três problemas nos containers: processos rodando como root, sistema de arquivos gravável (um invasor poderia alterar binários) e capabilities do Linux sobrando. O novo serviço de relatórios já nasce corrigido.",
                "Rode em segundo plano a imagem 'micorosoft/relatorios:2.0' com o nome 'relatorios', executando como usuário e grupo 1000:1000, com o sistema de arquivos raiz somente leitura e removendo TODAS as capabilities do Linux.",
                A("docker run -d --name relatorios --user 1000:1000 --read-only --cap-drop ALL micorosoft/relatorios:2.0"),
                A("São três opções de segurança do 'docker run': uma escolhe o usuário, outra torna o sistema de arquivos somente leitura e a terceira descarta capabilities.",
                  "--user 1000:1000 --read-only --cap-drop ALL (além de -d e --name)"),
                A("-d --name relatorios", "Segundo plano e nome.",
                  "--user 1000:1000  (-u)", "UID:GID do processo. Sem privilégios de root, uma falha no app não vira controle total do container.",
                  "--read-only", "Monta o sistema de arquivos raiz como somente leitura: ninguém altera binários nem instala nada.",
                  "--cap-drop ALL", "Remove todas as capabilities (ex.: NET_RAW, CHOWN, SETUID). Se algo for realmente necessário, devolva só aquilo com --cap-add.",
                  "micorosoft/relatorios:2.0", "Imagem."),
                "Com --read-only, o app que precisa gravar arquivos temporários usa um tmpfs (--tmpfs /tmp) ou um volume só para os dados. O ideal é a própria imagem já ter 'USER app' no Dockerfile; '--security-opt no-new-privileges' completa o kit básico de hardening.",
                "5b7d9f1e3a5c2b4d6f8e0a1c3b5d7f9e2a4c6b8d0f1e3a5c7b9d2f4e6a8c0b1d"));

            list.Add(MaisDockerL(2, "Healthcheck na largada",
                "Na última madrugada, a API travou mas o processo continuou vivo: o Docker achou que estava tudo bem e ninguém percebeu por 2 horas. O Rafael quer que a nova versão já suba com uma verificação de saúde e com reinício automático.",
                "Rode em segundo plano a imagem 'micorosoft/cafeapp:2.0' com o nome 'api-v2', com a política de reinício que reinicia sempre, EXCETO quando alguém parou o container manualmente, e com um healthcheck que executa: curl -f http://localhost:3000/health",
                A("docker run -d --name api-v2 --restart unless-stopped --health-cmd \"curl -f http://localhost:3000/health\" micorosoft/cafeapp:2.0"),
                A("A política que respeita o 'docker stop' manual é diferente da 'always'. O healthcheck é definido por opções que começam com --health-.",
                  "--restart unless-stopped --health-cmd \"<comando entre aspas>\""),
                A("-d --name api-v2", "Segundo plano e nome.",
                  "--restart unless-stopped", "Reinicia após falhas e após reboot do servidor, mas NÃO religa um container que você parou de propósito.",
                  "--health-cmd \"curl -f http://localhost:3000/health\"", "Comando executado periodicamente DENTRO do container. Saída 0 = healthy; diferente de 0 = unhealthy. As aspas são obrigatórias, senão o '-f' seria lido como opção do Docker.",
                  "micorosoft/cafeapp:2.0", "Imagem."),
                "Ajuste o ritmo com --health-interval, --health-timeout e --health-retries (ou use a instrução HEALTHCHECK no Dockerfile). Atenção: o Docker sozinho NÃO reinicia um container 'unhealthy' — quem age sobre o status é o orquestrador (Swarm, Kubernetes) ou o 'depends_on: condition: service_healthy' do Compose.",
@"e8a0c1b3d5f7e9a2c4b6d8f0e1a3c5b7d9f2e4a6c8b0d1f3e5a7c9b2d4f6e8a0

CONTAINER ID   IMAGE                    STATUS                            NAMES
e8a0c1b3d5f7   micorosoft/cafeapp:2.0   Up 8 seconds (health: starting)   api-v2"));

            list.Add(MaisDockerL(2, "Saúde em uma linha",
                "O script de deploy do Jorge precisa esperar a 'api-v2' ficar saudável antes de liberar o tráfego. Ele não quer ler um JSON de 300 linhas: precisa de UMA palavra (starting, healthy ou unhealthy) para usar num 'if'.",
                "Usando um template Go, exiba SOMENTE o estado de saúde do container 'api-v2' — no JSON do inspect esse campo fica em State → Health → Status.",
                A("docker inspect --format '{{.State.Health.Status}}' api-v2",
                  "docker inspect --format '{{ .State.Health.Status }}' api-v2",
                  "docker inspect -f '{{.State.Health.Status}}' api-v2",
                  "docker inspect -f '{{ .State.Health.Status }}' api-v2",
                  "docker inspect api-v2 -f '{{.State.Health.Status}}'",
                  "docker inspect api-v2 -f '{{ .State.Health.Status }}'"),
                A("O 'docker inspect' tem uma opção de formatação que recebe um template Go entre aspas: {{ ... }}.",
                  "O caminho do campo vira .State.Health.Status dentro de {{ }}: docker inspect --format '{{.Campo.SubCampo}}' <container>"),
                A("docker inspect", "Lê a configuração e o estado do container.",
                  "--format '{{.State.Health.Status}}'  (-f)", "Template Go: navega no JSON com pontos (.State → .Health → .Status) e imprime só esse valor.",
                  "api-v2", "Container alvo."),
                "Num script: until [ \"$(docker inspect -f '{{.State.Health.Status}}' api-v2)\" = healthy ]; do sleep 2; done. Use '{{json .State.Health}}' para ver também o histórico das últimas verificações (útil para descobrir POR QUE está unhealthy). Se a imagem não tiver healthcheck, o campo Health nem existe e o template dá erro.",
                "healthy"));

            list.Add(MaisDockerL(2, "Profiles de produção",
                "Na produção, o Café 365 usa o compose.yaml base mais um arquivo de sobrescrita, o compose.prod.yaml (réplicas, limites, variáveis de produção). Os serviços de monitoramento (prometheus e grafana) estão marcados com 'profiles: [monitoramento]' e só sobem quando esse profile é ativado.",
                "Suba em segundo plano a stack combinando os arquivos 'compose.yaml' e 'compose.prod.yaml' (nessa ordem) e ativando o profile 'monitoramento'.",
                A("docker compose -f compose.yaml -f compose.prod.yaml --profile monitoramento up -d",
                  "docker compose --profile monitoramento -f compose.yaml -f compose.prod.yaml up -d"),
                A("Arquivo e profile são opções GLOBAIS do 'docker compose' (vêm antes do subcomando 'up'). A opção de arquivo pode ser repetida.",
                  "docker compose -f <base> -f <sobrescrita> --profile <nome> up -d"),
                A("docker compose", "Plugin Compose.",
                  "-f compose.yaml -f compose.prod.yaml  (--file)", "Mescla os arquivos na ordem: o segundo sobrescreve/complementa o primeiro.",
                  "--profile monitoramento", "Ativa os serviços marcados com esse profile (serviços sem 'profiles' sobem sempre).",
                  "up -d", "Cria e sobe tudo em segundo plano."),
                "A ordem dos -f importa: o último vence. Para conferir o resultado da mescla ANTES de subir: docker compose -f compose.yaml -f compose.prod.yaml config. O profile também pode vir da variável COMPOSE_PROFILES=monitoramento.",
@"[+] Running 7/7
 ✔ Network cafe_default         Created
 ✔ Container cafe-db-1          Started
 ✔ Container cafe-cache-1       Started
 ✔ Container cafe-api-1         Started
 ✔ Container cafe-api-2         Started
 ✔ Container cafe-prometheus-1  Started
 ✔ Container cafe-grafana-1     Started"));

            list.Add(MaisDockerL(2, "Backup de volume",
                "Antes de migrar o PostgreSQL para a versão nova, o Jorge exige um backup do volume 'dados'. Volumes ficam escondidos em /var/lib/docker, então o truque é usar um container temporário que enxerga o volume e uma pasta do host ao mesmo tempo.",
                "Em um container temporário da imagem 'alpine' (removido ao terminar), monte o volume 'dados' em /origem em modo SOMENTE LEITURA e a pasta do host /home/aluno/backup em /backup; dentro dele, execute: tar czf /backup/dados.tar.gz -C /origem .",
                A("docker run --rm -v dados:/origem:ro -v /home/aluno/backup:/backup alpine tar czf /backup/dados.tar.gz -C /origem .",
                  "docker run --rm -v dados:/origem:ro -v /home/aluno/backup:/backup alpine tar -czf /backup/dados.tar.gz -C /origem ."),
                A("São dois -v no mesmo 'docker run': um volume nomeado (com o sufixo de somente leitura) e um bind mount. O comando tar vem DEPOIS da imagem.",
                  "docker run --rm -v <volume>:<destino>:ro -v <pasta-host>:<destino> alpine tar ..."),
                A("--rm", "O container só existe durante o backup e é apagado ao terminar.",
                  "-v dados:/origem:ro", "Volume nomeado montado como somente leitura: o backup não corre risco de alterar os dados.",
                  "-v /home/aluno/backup:/backup", "Bind mount: o arquivo gerado em /backup aparece na pasta do host.",
                  "alpine", "Imagem mínima que já tem o tar.",
                  "tar czf /backup/dados.tar.gz -C /origem .", "Compacta (c=criar, z=gzip, f=arquivo) todo o conteúdo de /origem no arquivo do backup."),
                "Para restaurar, inverta: docker run --rm -v dados-novo:/destino -v /home/aluno/backup:/backup alpine tar xzf /backup/dados.tar.gz -C /destino. Em bancos de dados, pare o container (ou use o dump do próprio banco, como pg_dump) para garantir um backup consistente.",
                ""));

            list.Add(MaisDockerL(2, "Faxina com filtro",
                "O servidor de CI encheu de novo. Desta vez o Rafael quer uma limpeza seletiva: as imagens recentes ainda servem de cache para os builds da semana, só as antigas devem sair.",
                "Remova todas as imagens não usadas por containers (não apenas as dangling) que foram criadas há mais de 7 dias — informe o tempo em horas.",
                A("docker image prune -a --filter until=168h", "docker image prune -a --filter until=168h -f"),
                A("É o prune de imagens com a opção que inclui todas as não usadas, mais um filtro de tempo (CHAVE=VALOR). 7 dias = 7 × 24 horas.",
                  "docker image prune -a --filter until=<horas>h"),
                A("docker image prune", "Remove imagens sem uso.",
                  "-a  (--all)", "Inclui TODAS as imagens sem container associado, não só as <none>.",
                  "--filter until=168h", "Só as criadas há mais de 168 horas (7 dias). Atenção: no prune, '-f' é --force; o filtro precisa ser escrito como --filter."),
                "Em pipelines, use '-f' (force) para pular a confirmação. O mesmo filtro funciona em 'docker container prune' e 'docker system prune'. Outro filtro útil: --filter label=projeto=cafe365 limpa só o que tem esse rótulo.",
@"WARNING! This will remove all images without at least one container associated to them.
Are you sure you want to continue? [y/N] y
Deleted Images:
untagged: cafeapp:1.0
deleted: sha256:3c9e7d1a2b4f...
untagged: cafeapp:dev-old
deleted: sha256:8b2f0e1a3c5b...

Total reclaimed space: 2.4GB"));
        }
    }

    static partial class Docs
    {
        static partial void DocsMaisDocker(List<DocEntry> all)
        {
            const string Dk = "Docker";
            all.Add(X(Dk, "Containers", "Filtros (--filter)", "Vários comandos (ps, images, prune...) aceitam filtros no formato CHAVE=VALOR para trabalhar só com parte dos objetos. Pode repetir --filter para combinar.",
                "docker ps [-a] --filter CHAVE=VALOR [--filter CHAVE=VALOR...]\ndocker image prune [-a] --filter CHAVE=VALOR",
                A("status=", "estado do container: created, running, paused, exited...",
                  "ancestor=", "containers criados a partir de uma imagem (ou de imagens derivadas dela)",
                  "name=", "nome do container (aceita parte do nome)",
                  "label=", "objetos com um rótulo (label) específico",
                  "until=", "(prune) só objetos criados ANTES desse tempo atrás: 24h, 72h... (em horas/minutos)",
                  "dangling=true", "(images) só imagens sem tag (<none>)"),
                A("docker ps -a --filter ancestor=redis", "todos os containers (mesmo parados) criados da imagem redis",
                  "docker ps --filter name=worker --filter status=running", "containers 'worker' que estão rodando",
                  "docker container prune --filter until=48h", "remove containers parados criados há mais de 2 dias",
                  "docker image prune --filter label=time=biblioteca", "imagens dangling com esse rótulo"),
                "No 'docker ps' e no 'docker images', -f é o atalho de --filter. Já nos comandos prune, -f significa --force (sem confirmação): lá escreva sempre --filter por extenso."));

            all.Add(X(Dk, "Containers", "docker logs: janela de tempo", "Recorta os logs de um container por tempo: a partir de quando (--since) e até quando (--until).",
                "docker logs --since INÍCIO --until FIM CONTAINER",
                A("--since TEMPO", "a partir desse momento: data RFC 3339 (2026-03-01T09:00:00) ou relativo (10m, 2h)",
                  "--until TEMPO", "até esse momento (mesmos formatos)",
                  "-t, --timestamps", "mostra a data/hora de cada linha",
                  "-n, --tail N", "só as últimas N linhas do recorte"),
                A("docker logs --since 2026-03-01T09:00:00 --until 2026-03-01T09:15:00 fila", "o que o container 'fila' registrou entre 9h e 9h15 de 01/03/2026",
                  "docker logs --since 3h --until 2h -t worker", "a penúltima hora, com horário em cada linha"),
                "Datas sem fuso são interpretadas em UTC pelo daemon. Combine com 'grep' no host para achar erros: docker logs --since 1h api 2>&1 | grep ERROR."));

            all.Add(X(Dk, "Containers", "docker update", "Altera limites de recursos (e a política de reinício) de um container EXISTENTE, sem recriá-lo.",
                "docker update [OPÇÕES] CONTAINER [CONTAINER...]",
                A("-m, --memory LIMITE", "novo limite de memória (ex.: 768m, 2g)",
                  "--memory-swap LIMITE", "memória + swap somados; igual ao --memory = sem swap; -1 = swap ilimitado",
                  "--cpus N", "limite de CPUs (ex.: 0.5, 2)",
                  "--cpu-shares N", "peso relativo de CPU quando há disputa",
                  "--restart POLÍTICA", "troca a política de reinício"),
                A("docker update --memory 1g --memory-swap 1g --cpus 2 relatorio-mensal", "1 GB sem swap e 2 CPUs para o container 'relatorio-mensal'",
                  "docker update --restart on-failure:3 importador", "passa a reiniciar até 3 vezes em caso de erro"),
                "Se você aumentar o --memory para um valor MAIOR que o --memory-swap atual, o Docker recusa: ajuste os dois juntos. Mudanças feitas com update não estão no compose/Dockerfile — registre-as lá também."));

            all.Add(X(Dk, "Imagens", "Multi-stage, ARG e --target", "Um Dockerfile pode ter vários estágios (FROM ... AS nome). ARG declara variáveis de build, preenchidas com --build-arg. --target escolhe até qual estágio construir.",
                "ARG VERSAO=1\nFROM python:${VERSAO}-slim AS base\n...\nFROM base AS teste\n...\nFROM base AS final\n\ndocker build --target ESTÁGIO --build-arg CHAVE=VALOR -t NOME:TAG CONTEXTO",
                A("FROM imagem AS nome", "inicia um estágio com nome",
                  "COPY --from=estágio", "copia arquivos de um estágio anterior (só o necessário vai para a imagem final)",
                  "ARG NOME[=padrão]", "variável disponível só durante o build",
                  "--build-arg NOME=VALOR", "define o valor de um ARG na hora do build",
                  "--target ESTÁGIO", "para o build nesse estágio"),
                A("docker build --target teste --build-arg PYTHON_VERSION=3.12 -t biblioteca:teste .", "constrói só até o estágio 'teste' com Python 3.12",
                  "docker build --build-arg PORTA=8080 -t loja-livros:1.3 .", "preenche o ARG PORTA"),
                "Valores de --build-arg ficam visíveis no 'docker history': nunca use para senhas (prefira --secret do BuildKit). Um ARG declarado antes do primeiro FROM precisa ser repetido (ARG NOME) dentro do estágio que o usa."));

            all.Add(X(Dk, "Volumes e redes", "docker network create (sub-rede e gateway)", "Cria uma rede com endereçamento IP definido por você, em vez de deixar o Docker escolher a faixa.",
                "docker network create [--driver bridge] --subnet FAIXA/CIDR --gateway IP NOME",
                A("--subnet", "faixa de IPs em notação CIDR (ex.: 10.20.0.0/24)",
                  "--gateway", "IP do gateway — precisa pertencer à sub-rede",
                  "--ip-range", "sub-faixa de onde os containers recebem IP automático",
                  "-d, --driver", "bridge (padrão), overlay, macvlan..."),
                A("docker network create --subnet 10.20.0.0/24 --gateway 10.20.0.1 escola-net", "rede 'escola-net' com 254 endereços",
                  "docker run -d --network escola-net --ip 10.20.0.50 --name secretaria nginx", "container com IP fixo nessa rede"),
                "O --ip do 'docker run' só funciona em redes criadas com --subnet. Evite faixas que já existam na rede local ou na VPN."));

            all.Add(X(Dk, "Containers", "Rodando com segurança (usuário, read-only, capabilities)", "Opções do 'docker run' que reduzem o estrago caso o app seja invadido.",
                "docker run [-d] [--name NOME] --user UID:GID --read-only --cap-drop ALL [--cap-add CAP] IMAGEM",
                A("-u, --user UID[:GID]", "roda o processo sem ser root",
                  "--read-only", "sistema de arquivos raiz somente leitura",
                  "--tmpfs /caminho", "pasta temporária em memória (útil junto com --read-only)",
                  "--cap-drop CAP|ALL", "remove capabilities do Linux (ALL = todas)",
                  "--cap-add CAP", "devolve só uma capability necessária (ex.: NET_BIND_SERVICE)",
                  "--security-opt no-new-privileges", "impede ganhar privilégios via setuid"),
                A("docker run -d --name catalogo --user 2000:2000 --read-only --tmpfs /tmp livraria/catalogo:3.1", "catálogo sem root e com disco somente leitura",
                  "docker run --rm --cap-drop ALL --cap-add NET_BIND_SERVICE busybox id", "nenhuma capability, exceto abrir portas baixas"),
                "Princípio do menor privilégio: comece sem nada (--cap-drop ALL) e acrescente só o que o app realmente precisa. O melhor lugar para definir o usuário é o próprio Dockerfile (instrução USER)."));

            all.Add(X(Dk, "Containers", "Healthcheck (--health-cmd)", "Define um comando que o Docker executa periodicamente dentro do container para saber se o app está saudável (healthy) ou não (unhealthy).",
                "docker run --health-cmd \"COMANDO\" [--health-interval 30s] [--health-retries 3] IMAGEM\n(no Dockerfile: HEALTHCHECK CMD comando)",
                A("--health-cmd \"...\"", "comando de verificação (entre aspas). Saída 0 = saudável",
                  "--health-interval", "intervalo entre verificações (padrão 30s)",
                  "--health-timeout", "tempo máximo de cada verificação",
                  "--health-retries", "falhas seguidas até virar unhealthy",
                  "--no-healthcheck", "desliga o healthcheck definido na imagem"),
                A("docker run -d --name acervo --health-cmd \"wget -q -O- http://localhost:8080/ping\" biblioteca/acervo:1.0", "verifica a rota /ping",
                  "docker run -d --restart on-failure:5 --health-cmd \"pg_isready -U leitor\" postgres", "banco com healthcheck e reinício em caso de erro"),
                "O STATUS do 'docker ps' mostra (health: starting), (healthy) ou (unhealthy). O Docker puro não reinicia containers unhealthy: quem reage é o orquestrador ou o Compose (depends_on com condition: service_healthy). Políticas de --restart: no, on-failure[:N], always, unless-stopped (não religa o que você parou manualmente)."));

            all.Add(X(Dk, "Containers", "docker inspect --format (templates Go)", "Extrai só os campos que interessam do JSON do inspect, usando um template Go entre aspas.",
                "docker inspect --format '{{.Campo.SubCampo}}' OBJETO\ndocker inspect -f '{{json .Campo}}' OBJETO",
                A("{{.A.B.C}}", "navega no JSON: cada ponto entra em um nível (respeite maiúsculas)",
                  "{{json .Campo}}", "imprime o campo (ou objeto inteiro) em JSON",
                  "{{range ...}} {{end}}", "percorre listas/mapas (ex.: redes de um container)"),
                A("docker inspect --format '{{.RestartCount}}' fila", "quantas vezes o container 'fila' reiniciou",
                  "docker inspect -f '{{.HostConfig.Memory}}' relatorio", "limite de memória em bytes",
                  "docker inspect -f '{{json .State.Health}}' acervo", "healthcheck completo: status, falhas seguidas e últimas saídas",
                  "docker inspect -f '{{.Config.Image}}' acervo", "de qual imagem o container foi criado"),
                "Para descobrir o caminho de um campo, rode o inspect completo uma vez e siga as chaves do JSON: State → Status vira .State.Status."));

            all.Add(X(Dk, "Compose", "docker compose: vários arquivos e --profile", "Mescla arquivos compose (base + sobrescritas) e ativa serviços opcionais marcados com 'profiles'.",
                "docker compose -f BASE -f SOBRESCRITA [--profile NOME] SUBCOMANDO",
                A("-f, --file ARQUIVO", "pode repetir; arquivos são mesclados na ordem (o último vence)",
                  "--profile NOME", "ativa serviços com 'profiles: [NOME]' (opção global, antes do subcomando)",
                  "config", "mostra o YAML final já mesclado (ótimo para conferir)",
                  "COMPOSE_PROFILES", "variável de ambiente alternativa ao --profile"),
                A("docker compose -f compose.yaml -f compose.teste.yaml config", "confere a mescla dos dois arquivos",
                  "docker compose --profile ferramentas up -d", "sobe a stack + os serviços do profile 'ferramentas'",
                  "docker compose -f base.yml -f homologacao.yml up -d", "stack de homologação"),
                "Serviços sem 'profiles' sobem sempre. Exemplo no YAML:  adminer:  image: adminer  profiles: [ferramentas]"));

            all.Add(X(Dk, "Volumes e redes", "Backup e restauração de volumes", "Volumes não são pastas comuns do host. Para copiá-los, use um container temporário que monta o volume E uma pasta do host, e rode o tar lá dentro.",
                "docker run --rm -v VOLUME:/ORIGEM:ro -v PASTA_HOST:/DESTINO IMAGEM tar czf /DESTINO/arquivo.tar.gz -C /ORIGEM .",
                A(":ro", "monta o volume como somente leitura (o backup não altera nada)",
                  "tar czf ARQ -C DIR .", "c = cria, z = gzip, f = arquivo; -C entra no diretório antes de compactar",
                  "tar xzf ARQ -C DIR", "x = extrai (restauração)"),
                A("docker run --rm -v notas:/dados:ro -v /home/bia/copias:/copias busybox tar czf /copias/notas.tar.gz -C /dados .", "backup do volume 'notas' em /home/bia/copias",
                  "docker run --rm -v notas-novo:/dados -v /home/bia/copias:/copias busybox tar xzf /copias/notas.tar.gz -C /dados", "restaura o backup em outro volume"),
                "Em bancos de dados, pare o container antes (ou use a ferramenta de dump do banco) para não copiar arquivos no meio de uma escrita."));
        }
    }
}
