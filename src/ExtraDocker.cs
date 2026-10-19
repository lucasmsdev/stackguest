using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerQuest
{
    static partial class Challenges
    {
        const string ExtraDockerCap = "Capítulo 9 · Operação no Dia a Dia";

        static Level ExtraDocker_L(int tier, string title, string ctx, string obj, string[] ans, string[] hints,
                                   string[] explain, string summary, string output)
        {
            return new Level
            {
                Cat = "docker", Kind = "docker", Tier = tier, Chapter = ExtraDockerCap, Title = title, Context = ctx,
                Objective = obj, Answers = ans, Hints = hints, Explain = explain, Summary = summary, Output = output,
                Ignore = new string[0]
            };
        }

        static partial void ExtraDocker(List<Level> list)
        {
            // ══════════════════════ INICIANTE (um conceito por vez) ══════════════════════
            list.Add(ExtraDocker_L(0, "Cliente e servidor",
                "O suporte do Docker pediu as versões do cliente E do servidor (Engine, containerd, versão da API) para investigar um bug. A linha curta do 'docker --version' não serve: ela só mostra o cliente.",
                "Mostre as informações completas de versão do cliente e do servidor Docker usando o SUBCOMANDO de versão (não a flag com traços).",
                A("docker version"),
                A("Desta vez não é uma flag com traços: é um subcomando do Docker, com o nome em inglês de 'versão'.",
                  "docker version  (sem os traços)"),
                A("docker version", "Mostra duas seções: Client (a CLI que você digita) e Server (Docker Engine, containerd, runc), cada uma com versão, versão da API, versão do Go e sistema/arquitetura."),
                "Se a seção Server vier com erro ('Cannot connect to the Docker daemon'), o cliente está instalado mas o Engine não está rodando (no Windows/Mac, abra o Docker Desktop). Num bug report, mande sempre a saída do 'docker version' inteira.",
@"Client:
 Version:           27.3.1
 API version:       1.47
 Go version:        go1.22.7
 OS/Arch:           linux/amd64

Server: Docker Engine - Community
 Engine:
  Version:          27.3.1
  API version:      1.47 (minimum version 1.24)
 containerd:
  Version:          1.7.22
 runc:
  Version:          1.1.14"));

            list.Add(ExtraDocker_L(0, "Só os IDs",
                "O Jorge (SRE) está escrevendo um script que vai processar cada container em execução, um por um. Ele não quer tabela nem cabeçalho: só os IDs, um por linha.",
                "Liste apenas os IDs dos containers EM EXECUÇÃO (sem as outras colunas e sem incluir os parados).",
                A("docker ps -q", "docker ps --quiet", "docker container ls -q"),
                A("O 'docker ps' tem uma opção 'silenciosa' (em inglês, quiet) que corta todas as colunas menos uma.",
                  "docker ps -q"),
                A("docker ps", "Lista os containers em execução.",
                  "-q  (--quiet)", "Imprime só o ID curto de cada container, um por linha — perfeito para alimentar outro comando."),
                "Combinação clássica: docker stop $(docker ps -q) para todos os containers rodando. Junte -a para incluir os parados (docker ps -aq) — e pense duas vezes antes de passar isso para um 'docker rm'.",
@"d3f5a7c9e1b2
9a4c6e8b0d2f
5e7a9c1b3d5f"));

            list.Add(ExtraDocker_L(0, "Congelando o container",
                "O container 'relatorio' está gerando um relatório pesado bem na hora do pico de pedidos do Café 365. O Rafael não quer perder o que já foi processado: só congelar o container por alguns minutos e depois continuar de onde parou.",
                "Congele (suspenda) todos os processos do container 'relatorio', sem pará-lo e sem perder o estado.",
                A("docker pause relatorio", "docker container pause relatorio"),
                A("Parar (stop) encerraria o processo. Procure o comando que suspende, como o botão de pausa de um vídeo.",
                  "docker pause <container>  (para voltar: docker unpause <container>)"),
                A("docker pause", "Congela todos os processos do container (no Linux, via cgroup freezer). Memória e estado ficam intactos; os processos só deixam de receber CPU.",
                  "relatorio", "Container alvo."),
                "Para descongelar: docker unpause relatorio. No 'docker ps' o STATUS aparece como 'Up 2 hours (Paused)'. Diferente do stop, nada é encerrado — as conexões abertas apenas ficam esperando, então não deixe um container pausado por muito tempo.",
                "relatorio"));

            list.Add(ExtraDocker_L(0, "Mapa de portas",
                "A Lia (QA) não consegue abrir a 'vitrine' no navegador e quer saber em qual porta do host ela foi publicada. O 'docker ps' tem colunas demais; ela quer só os mapeamentos de porta.",
                "Mostre somente os mapeamentos de porta do container 'vitrine'.",
                A("docker port vitrine", "docker container port vitrine"),
                A("Existe um comando curtinho com o nome, em inglês, de 'porta'.",
                  "docker port <container>"),
                A("docker port", "Lista cada porta do container e para onde ela foi publicada no host (IP:porta).",
                  "vitrine", "Container alvo."),
                "Para checar uma porta específica: docker port vitrine 80. Se não aparecer nada, o container foi criado sem -p — e portas só podem ser publicadas na criação (é preciso recriar o container).",
@"80/tcp -> 0.0.0.0:8081
80/tcp -> [::]:8081"));

            list.Add(ExtraDocker_L(0, "Raio-X do volume",
                "O time de dados perguntou onde, no servidor, ficam fisicamente os arquivos do volume 'dados' do PostgreSQL e quando ele foi criado.",
                "Mostre os detalhes (driver, ponto de montagem, data de criação) do volume 'dados'.",
                A("docker volume inspect dados"),
                A("Assim como containers e redes, volumes também têm um subcomando de inspeção dentro do grupo 'docker volume'.",
                  "docker volume inspect <volume>"),
                A("docker volume inspect", "Mostra em JSON os detalhes do volume.",
                  "dados", "Volume alvo. Repare no campo Mountpoint: é a pasta real no host (/var/lib/docker/volumes/...)."),
                "Não edite arquivos direto no Mountpoint com o container rodando: use o próprio container (docker exec) ou um container temporário. No Docker Desktop (Windows/Mac), esse caminho fica dentro da máquina virtual do Docker, não no seu disco.",
@"[
    {
        ""CreatedAt"": ""2026-09-02T10:14:31Z"",
        ""Driver"": ""local"",
        ""Labels"": null,
        ""Mountpoint"": ""/var/lib/docker/volumes/dados/_data"",
        ""Name"": ""dados"",
        ""Options"": null,
        ""Scope"": ""local""
    }
]"));

            list.Add(ExtraDocker_L(0, "Último recurso: kill",
                "O container 'relogio-ponto' travou: o 'docker stop' fica esperando o tempo limite e o processo ignora o pedido de encerramento. A líder técnica autoriza encerrar na marra.",
                "Encerre IMEDIATAMENTE o container 'relogio-ponto', sem encerramento gracioso (sem esperar o tempo limite) e sem removê-lo.",
                A("docker kill relogio-ponto", "docker container kill relogio-ponto"),
                A("Existe um comando que manda o sinal de morte (SIGKILL) direto, sem esperar.",
                  "docker kill <container>"),
                A("docker kill", "Envia SIGKILL (padrão) ao processo principal: ele morre na hora, sem chance de salvar nada.",
                  "relogio-ponto", "Container alvo. Ele continua existindo (parado); dá para ver os logs e investigar o travamento."),
                "Use o kill só depois de tentar o stop: dados não salvos se perdem. Com -s você escolhe outro sinal, por exemplo docker kill -s SIGHUP para pedir que alguns servidores recarreguem a configuração.",
                "relogio-ponto"));

            list.Add(ExtraDocker_L(0, "Stack em pausa",
                "Vai ter manutenção elétrica no prédio. O Rafael quer parar a stack do Café 365 que roda com o Compose, mas sem remover containers, redes e volumes — amanhã é só ligar de novo.",
                "Na pasta do projeto, pare todos os serviços do Compose SEM remover os containers.",
                A("docker compose stop"),
                A("O 'down' para E remove. Você só quer parar — o oposto de 'start'.",
                  "docker compose stop  (para religar: docker compose start)"),
                A("docker compose stop", "Para os containers de todos os serviços do projeto, mantendo-os criados (com a mesma configuração e os mesmos dados) para um 'start' posterior."),
                "Use 'stop' para pausas e 'down' quando quiser desmontar a stack. Para parar só um serviço: docker compose stop worker.",
@"[+] Stopping 4/4
 ✔ Container cafe-api-1     Stopped
 ✔ Container cafe-worker-1  Stopped
 ✔ Container cafe-cache-1   Stopped
 ✔ Container cafe-db-1      Stopped"));

            list.Add(ExtraDocker_L(0, "Rede aposentada",
                "O teste de integração terminou e a rede 'rede-teste' ficou sobrando. Nenhum container está mais conectado a ela.",
                "Remova a rede 'rede-teste'.",
                A("docker network rm rede-teste", "docker network remove rede-teste"),
                A("Segue o mesmo padrão de 'docker volume rm', só que no grupo das redes.",
                  "docker network rm <rede>"),
                A("docker network rm", "Remove uma ou mais redes criadas pelo usuário.",
                  "rede-teste", "Rede alvo."),
                "Se algum container ainda estiver conectado, o Docker recusa ('has active endpoints'): desconecte-o (docker network disconnect) ou remova-o antes. As redes padrão (bridge, host, none) não podem ser removidas. Para apagar todas as redes sem uso: docker network prune.",
                "rede-teste"));

            list.Add(ExtraDocker_L(0, "Foto do consumo",
                "A Lia quer colar no relatório de QA quanto de CPU e memória cada container está usando agora. O 'docker stats' normal fica atualizando sem parar e não dá para copiar.",
                "Mostre o consumo de recursos dos containers em execução UMA única vez (sem ficar atualizando) e volte ao terminal.",
                A("docker stats --no-stream", "docker container stats --no-stream"),
                A("O 'docker stats' tem uma opção para NÃO ficar transmitindo (stream) as atualizações.",
                  "docker stats --no-stream"),
                A("docker stats", "Mostra CPU, memória, rede, disco e nº de processos de cada container.",
                  "--no-stream", "Faz uma única leitura, imprime e encerra — em vez do painel ao vivo."),
                "Para um container só: docker stats --no-stream api. Em scripts, junte --format para escolher as colunas (ex.: --format \"{{.Name}} {{.MemUsage}}\").",
@"CONTAINER ID   NAME      CPU %     MEM USAGE / LIMIT     MEM %     NET I/O           BLOCK I/O        PIDS
d3f5a7c9e1b2   vitrine   0.02%     9.1MiB / 7.6GiB       0.12%     1.2MB / 3.4MB     0B / 8.2kB       5
9a4c6e8b0d2f   api       3.47%     182.4MiB / 512MiB     35.63%    48.1MB / 61.7MB   12.3MB / 0B      23
5e7a9c1b3d5f   db        0.81%     96.7MiB / 7.6GiB      1.24%     22.4MB / 30.9MB   210MB / 1.1GB    11"));

            // ══════════════════════ INTERMEDIÁRIO (2–3 conceitos combinados) ══════════════════════
            list.Add(ExtraDocker_L(1, "Logs com relógio",
                "O Jorge recebeu um alerta e precisa comparar as últimas mensagens da 'api' com o horário do monitoramento. Sem a data/hora em cada linha, não dá para saber o que veio antes.",
                "Mostre as últimas 50 linhas de log do container 'api', com a data e a hora de cada linha.",
                A("docker logs -t --tail 50 api", "docker logs -t -n 50 api",
                  "docker logs --timestamps --tail 50 api", "docker logs --timestamps -n 50 api"),
                A("Combine duas opções do 'docker logs': uma limita a quantidade de linhas do fim e a outra acrescenta o horário.",
                  "--tail 50 (ou -n 50) junto com -t (--timestamps)"),
                A("docker logs", "Mostra os logs do container.",
                  "-t  (--timestamps)", "Prefixa cada linha com a data/hora em que o Docker a recebeu (RFC 3339, em UTC).",
                  "--tail 50  (-n)", "Só as últimas 50 linhas.",
                  "api", "Container alvo."),
                "O horário do -t é o do Docker, não o do app — útil quando o próprio app não registra hora. Lembre que está em UTC (São Paulo = UTC-3). Junte -f para continuar acompanhando a partir dessas 50 linhas.",
@"2026-10-15T13:41:02.118Z GET /cardapio 200 17ms
2026-10-15T13:41:07.503Z POST /pedidos 201 88ms
2026-10-15T13:41:09.990Z WARN fila de pagamentos com 120 itens
2026-10-15T13:41:12.004Z POST /pedidos 504 30001ms"));

            list.Add(ExtraDocker_L(1, "Arquivo para dentro",
                "O marketing mandou a página 'promo.html' da promoção de cappuccino e quer vê-la no ar AGORA na 'vitrine' (nginx), sem rebuild de imagem. O arquivo está na pasta atual do host.",
                "Copie o arquivo 'promo.html' (pasta atual do host) para a pasta /usr/share/nginx/html/ dentro do container 'vitrine'.",
                A("docker cp promo.html vitrine:/usr/share/nginx/html/",
                  "docker cp ./promo.html vitrine:/usr/share/nginx/html/",
                  "docker cp promo.html vitrine:/usr/share/nginx/html/promo.html",
                  "docker cp ./promo.html vitrine:/usr/share/nginx/html/promo.html"),
                A("É o mesmo comando de cópia usado para trazer arquivos do container, mas agora com a ORIGEM no host e o DESTINO no container.",
                  "docker cp <arquivo-do-host> <container>:<pasta-no-container>"),
                A("docker cp", "Copia arquivos entre host e container (nos dois sentidos).",
                  "promo.html", "Origem: arquivo na pasta atual do host.",
                  "vitrine:/usr/share/nginx/html/", "Destino: o prefixo 'vitrine:' indica que o caminho é DENTRO do container."),
                "A ordem é sempre ORIGEM DESTINO; o lado do container leva 'nome:'. Mas atenção: a cópia vive só naquele container e some quando ele é recriado. Para algo permanente, coloque o arquivo na imagem (COPY no Dockerfile) ou use um volume/bind mount.",
                "Successfully copied 4.61kB to vitrine:/usr/share/nginx/html/"));

            list.Add(ExtraDocker_L(1, "Variáveis em arquivo",
                "A API de homologação precisa de 12 variáveis de ambiente (URL do banco, chaves de pagamento de teste, nível de log...). Digitar 12 '-e' é pedir para errar; o Rafael já deixou tudo no arquivo 'hml.env', na pasta atual.",
                "Rode em segundo plano a imagem 'micorosoft/cafeapp:2.0' com o nome 'api-hml', publicando a porta 3000 do container na porta 3001 do host e carregando as variáveis de ambiente do arquivo 'hml.env'.",
                A("docker run -d --name api-hml -p 3001:3000 --env-file hml.env micorosoft/cafeapp:2.0",
                  "docker run -d --name api-hml -p 3001:3000 --env-file ./hml.env micorosoft/cafeapp:2.0"),
                A("Além de -d, --name e -p, existe uma opção do 'docker run' que lê as variáveis de um ARQUIVO em vez de uma por uma.",
                  "--env-file <arquivo>  — e no -p o formato é HOST:CONTAINER."),
                A("-d --name api-hml", "Segundo plano e nome.",
                  "-p 3001:3000", "Porta 3001 do host → porta 3000 do container.",
                  "--env-file hml.env", "Carrega todas as variáveis do arquivo (uma CHAVE=valor por linha; linhas com # são comentários).",
                  "micorosoft/cafeapp:2.0", "Imagem."),
                "No --env-file não use 'export' nem aspas: as aspas viram parte do valor. Coloque *.env no .gitignore e no .dockerignore — arquivo com segredos não vai para o repositório nem para dentro da imagem. Dá para combinar com -e, que sobrescreve uma variável do arquivo.",
                "7c1e3a5b9d2f4e6a8c0b1d3f5e7a9c2b4d6f8e0a1c3b5d7f9e2a4c6b8d0f1e3a"));

            list.Add(ExtraDocker_L(1, "Listagem sob medida",
                "O Jorge quer colar no chat do time um resumo enxuto de TODOS os containers do servidor — rodando ou parados —, mostrando só o nome e o status de cada um.",
                "Liste todos os containers (inclusive os parados) formatando a saída com o template Go: {{.Names}} {{.Status}}",
                A("docker ps -a --format \"{{.Names}} {{.Status}}\"",
                  "docker ps -a --format \"{{ .Names }} {{ .Status }}\"",
                  "docker container ls -a --format \"{{.Names}} {{.Status}}\""),
                A("Combine a flag que inclui os parados com a opção de formatação do 'docker ps', que recebe o template entre aspas.",
                  "docker ps -a --format \"<template>\""),
                A("docker ps -a", "Lista todos os containers, inclusive os parados.",
                  "--format \"{{.Names}} {{.Status}}\"", "Template Go: para cada container imprime só o nome e o status. As aspas mantêm o espaço dentro do template."),
                "Prefixe com 'table' para ter cabeçalho e colunas alinhadas: --format \"table {{.Names}}\\t{{.Status}}\". Outros campos: .ID, .Image, .Ports, .RunningFor. O mesmo --format funciona em docker images, docker stats e docker network ls.",
@"vitrine Up 3 days
api Up 5 hours
db Up 5 hours
vigia Exited (0) 9 days ago
teste-proxy Exited (137) 2 weeks ago"));

            list.Add(ExtraDocker_L(1, "Rebuild sem cache no Compose",
                "Depois de trocar a versão de uma dependência, o serviço 'api' do Compose continua com o pacote velho: o cache de camadas reaproveitou o 'npm ci' antigo. O Rafael quer reconstruir só essa imagem, do zero.",
                "Usando o Compose, reconstrua a imagem apenas do serviço 'api', sem usar o cache de build.",
                A("docker compose build --no-cache api"),
                A("O Compose tem um subcomando só para construir imagens (sem subir nada). Ele aceita a mesma opção 'sem cache' do docker build.",
                  "docker compose build --no-cache <serviço>"),
                A("docker compose build", "Constrói (ou reconstrói) as imagens dos serviços que têm 'build:' no YAML.",
                  "--no-cache", "Ignora o cache: todas as instruções do Dockerfile rodam de novo.",
                  "api", "Só o serviço 'api' (sem nome, reconstrói todos)."),
                "O build só gera a imagem; para trocar o container, rode depois docker compose up -d api. Junte --pull para baixar também a versão mais nova da imagem base. Para serviços que usam 'image:' sem 'build:', quem atualiza é o docker compose pull.",
@"[+] Building 48.2s (11/11) FINISHED
 => [api internal] load build definition from Dockerfile
 => [api 1/5] FROM docker.io/library/node:22-alpine
 => [api 4/5] RUN npm ci --omit=dev
 => [api 5/5] COPY . .
 => [api] exporting to image
 => => naming to docker.io/library/cafe-api"));

            list.Add(ExtraDocker_L(1, "Três tentativas e chega",
                "O 'importador' baixa a planilha de preços dos fornecedores todas as noites. Às vezes o servidor deles cai e o importador termina com erro; o Rafael quer que ele tente de novo sozinho, mas sem entrar num loop infinito se o problema continuar.",
                "Rode em segundo plano a imagem 'micorosoft/importador:1.0' com o nome 'importador' e uma política de reinício que reinicie o container APENAS quando ele terminar com erro, no máximo 3 vezes.",
                A("docker run -d --name importador --restart on-failure:3 micorosoft/importador:1.0"),
                A("Das políticas de --restart (no, on-failure, always, unless-stopped), uma reage só a erros — e aceita um número máximo de tentativas.",
                  "--restart on-failure:<máximo>"),
                A("-d --name importador", "Segundo plano e nome.",
                  "--restart on-failure:3", "Reinicia só se o processo terminar com código de saída diferente de 0, até 3 tentativas.",
                  "micorosoft/importador:1.0", "Imagem."),
                "Se o importador terminar com sucesso (exit 0), ele fica parado — exatamente o esperado para uma tarefa. O Docker espera cada vez mais entre as tentativas (o atraso dobra, começando em 100 ms). Confira quantas já foram com docker inspect -f '{{.RestartCount}}' importador.",
                "2d4f6e8a0c1b3d5f7e9a2c4b6d8f0e1a3c5b7d9f2e4a6c8b0d1f3e5a7c9b2d4f"));

            list.Add(ExtraDocker_L(1, "Apelido na rede",
                "O sistema legado de fidelidade roda na rede 'legado-net' e procura o Redis pelo nome 'redis', mas o nosso container se chama 'cache'. Renomear quebraria os outros serviços; o Jorge prefere dar um apelido DNS ao container só nessa rede.",
                "Conecte o container 'cache' à rede 'legado-net' com o alias de rede 'redis' (assim os containers dessa rede o encontram por 'redis').",
                A("docker network connect --alias redis legado-net cache"),
                A("É o 'docker network connect' de sempre, com uma opção que dá um nome adicional ao container naquela rede. Lembre a ordem: primeiro a REDE, depois o CONTAINER.",
                  "docker network connect --alias <apelido> <rede> <container>"),
                A("docker network connect", "Liga um container existente a mais uma rede.",
                  "--alias redis", "Nome DNS extra do container DENTRO dessa rede.",
                  "legado-net", "Rede.",
                  "cache", "Container."),
                "O alias vale só naquela rede: na cafe-net o container continua sendo apenas 'cache'. Vários containers podem ter o mesmo alias na mesma rede — o DNS do Docker devolve todos (um balanceamento simples). No 'docker run', o equivalente é --network-alias.",
                ""));

            list.Add(ExtraDocker_L(1, "Testes num container avulso",
                "Antes de cada merge, a Lia roda a suíte de testes da API. Ela não quer mexer no container do serviço 'api' que está atendendo: quer um container NOVO do mesmo serviço, que suma quando os testes acabarem.",
                "Usando o Compose, execute o comando 'npm test' em um container novo e temporário do serviço 'api', removido automaticamente ao terminar.",
                A("docker compose run --rm api npm test"),
                A("O 'exec' entra num container que já está rodando. Para criar um novo a partir da definição do serviço existe outro subcomando — e ele aceita a mesma opção de autorremoção do docker run.",
                  "docker compose run --rm <serviço> <comando>"),
                A("docker compose run", "Cria um container NOVO com a configuração do serviço (imagem, variáveis, volumes, redes) e roda um comando nele.",
                  "--rm", "Remove o container quando o comando terminar.",
                  "api", "Serviço do YAML.",
                  "npm test", "Comando executado no lugar do comando padrão do serviço."),
                "Sem --rm, cada run deixa um container parado para trás. O run sobe também as dependências (depends_on), mas por padrão NÃO publica as portas do serviço (use --service-ports se precisar).",
@"[+] Creating 1/0
 ✔ Container cafe-db-1  Running
> cafeapp@2.0.0 test
> jest
 PASS  tests/pedidos.test.js
 PASS  tests/cardapio.test.js
Tests:       42 passed, 42 total"));

            list.Add(ExtraDocker_L(1, "Duas tags num build",
                "Na hora do release, o Rafael quer que a imagem nova saia com a versão '2.1' (que nunca muda) e também como 'latest' (que sempre aponta para a mais recente) — num único build, sem precisar de 'docker tag' depois.",
                "A partir do diretório atual, construa a imagem 'cafeapp' marcada ao mesmo tempo com as tags '2.1' e 'latest'.",
                A("docker build -t cafeapp:2.1 -t cafeapp:latest ."),
                A("A opção de nome:tag do 'docker build' pode ser repetida.",
                  "docker build -t <nome:tag1> -t <nome:tag2> ."),
                A("docker build", "Constrói a imagem.",
                  "-t cafeapp:2.1", "Primeira tag: a versão fixa.",
                  "-t cafeapp:latest", "Segunda tag para o MESMO ID de imagem.",
                  ".", "Contexto de build."),
                "Em produção, faça deploy pela tag de versão (ou pelo digest), nunca pela 'latest': ela muda a cada release e dificulta saber o que está rodando e voltar atrás (rollback).",
@"[+] Building 9.8s (10/10) FINISHED
 => exporting to image
 => => naming to docker.io/library/cafeapp:2.1
 => => naming to docker.io/library/cafeapp:latest"));

            // ══════════════════════ PROFISSIONAL (cenários reais) ══════════════════════
            list.Add(ExtraDocker_L(2, "Logs que não lotam o disco",
                "O disco do servidor encheu às 3h da manhã: o arquivo de log JSON de um único container tinha 40 GB. Por padrão, o driver json-file guarda tudo para sempre. O Jorge quer que a nova 'api-v3' já suba com rotação de logs.",
                "Rode em segundo plano a imagem 'micorosoft/cafeapp:3.0' com o nome 'api-v3', usando opções do driver de log para limitar cada arquivo de log a 10m e manter no máximo 3 arquivos.",
                A("docker run -d --name api-v3 --log-opt max-size=10m --log-opt max-file=3 micorosoft/cafeapp:3.0",
                  "docker run -d --name api-v3 --log-driver json-file --log-opt max-size=10m --log-opt max-file=3 micorosoft/cafeapp:3.0"),
                A("Opções do driver de log são passadas com uma flag que pode ser repetida, no formato CHAVE=VALOR. As chaves são max-size e max-file.",
                  "--log-opt max-size=10m --log-opt max-file=3"),
                A("-d --name api-v3", "Segundo plano e nome.",
                  "--log-opt max-size=10m", "Quando o arquivo de log chega a 10 MB, ele é rotacionado.",
                  "--log-opt max-file=3", "Mantém no máximo 3 arquivos: no pior caso, 30 MB de log por container.",
                  "micorosoft/cafeapp:3.0", "Imagem."),
                "Para valer para TODOS os containers novos, configure no daemon (/etc/docker/daemon.json), com \"log-opts\": {\"max-size\": \"10m\", \"max-file\": \"3\"}, ou troque para o driver 'local', que já rotaciona e comprime. Containers existentes só pegam a configuração quando são recriados.",
                "4f6e8a0c1b3d5f7e9a2c4b6d8f0e1a3c5b7d9f2e4a6c8b0d1f3e5a7c9b2d4f6e"));

            list.Add(ExtraDocker_L(2, "IP do banco em uma linha",
                "Um firewall legado precisa do IP interno do container 'db' para liberar o acesso. O inspect completo tem centenas de linhas, e o IP fica dentro de um MAPA de redes (NetworkSettings → Networks), cuja chave é o nome de cada rede.",
                "Com --format (ou -f), use um template Go que percorra com range o mapa .NetworkSettings.Networks e imprima o .IPAddress de cada rede do container 'db' (blocos {{ }} colados, sem espaço entre eles).",
                A("docker inspect --format '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' db",
                  "docker inspect --format '{{ range .NetworkSettings.Networks }}{{ .IPAddress }}{{ end }}' db",
                  "docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' db",
                  "docker inspect -f '{{ range .NetworkSettings.Networks }}{{ .IPAddress }}{{ end }}' db",
                  "docker inspect db -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}'",
                  "docker inspect db -f '{{ range .NetworkSettings.Networks }}{{ .IPAddress }}{{ end }}'"),
                A("Para percorrer um mapa num template Go: {{range CAMINHO}} ... {{end}}. Dentro do range, o ponto (.) passa a ser cada rede.",
                  "docker inspect --format '{{range .NetworkSettings.Networks}}{{.Campo}}{{end}}' <container>"),
                A("docker inspect", "Lê a configuração e o estado do container.",
                  "{{range .NetworkSettings.Networks}}", "Percorre cada rede do container (é um mapa: nome da rede → configuração).",
                  "{{.IPAddress}}", "Dentro do range, imprime o IP do container naquela rede.",
                  "{{end}}", "Fecha o range.",
                  "db", "Container alvo."),
                "Por que range? Porque o container pode estar em várias redes, cada uma com seu IP. Se souber o nome da rede, use index: {{(index .NetworkSettings.Networks \"cafe-net\").IPAddress}}. E lembre: o IP muda quando o container é recriado — entre containers, use sempre o NOME (DNS da rede).",
                "172.18.0.3"));

            list.Add(ExtraDocker_L(2, "Quem derrubou a API?",
                "A 'api' reiniciou sozinha várias vezes na última hora e os logs não mostram nada (o processo morre antes de escrever). O Jorge quer ver os eventos do próprio Docker: quando o container morreu e com qual código de saída.",
                "Mostre os eventos do Docker da última hora (1h), filtrando apenas o container 'api' e apenas o evento 'die' (use dois filtros).",
                A("docker events --since 1h --filter container=api --filter event=die",
                  "docker events --since 1h -f container=api -f event=die",
                  "docker system events --since 1h --filter container=api --filter event=die"),
                A("O Docker registra tudo o que acontece (create, start, die, oom...) e o 'docker events' mostra esse histórico. Use a janela de tempo e o --filter repetido.",
                  "docker events --since 1h --filter container=<nome> --filter event=<evento>"),
                A("docker events", "Mostra os eventos do daemon: ciclo de vida de containers, imagens, redes e volumes.",
                  "--since 1h", "A partir de 1 hora atrás.",
                  "--filter container=api  (-f)", "Só eventos do container 'api'.",
                  "--filter event=die", "Só o evento 'die' (o processo principal terminou) — cada linha traz o exitCode."),
                "exitCode=137 é 128 + 9 (SIGKILL): quase sempre o OOM killer — confirme com docker inspect -f '{{.State.OOMKilled}}' api. Sem --until, o comando continua ouvindo ao vivo depois de mostrar o histórico (Ctrl+C para sair). Outros eventos úteis: oom, restart, health_status.",
@"2026-10-15T13:12:44.581230417Z container die 3f2a9c1e7b5d (exitCode=137, image=micorosoft/cafeapp:2.0, name=api)
2026-10-15T13:31:02.904117261Z container die 3f2a9c1e7b5d (exitCode=137, image=micorosoft/cafeapp:2.0, name=api)
2026-10-15T13:58:19.006734120Z container die 3f2a9c1e7b5d (exitCode=137, image=micorosoft/cafeapp:2.0, name=api)"));

            list.Add(ExtraDocker_L(2, "Dump sem TTY",
                "Antes da migração de sexta, o Rafael quer um dump do banco 'cafe365' gravado num arquivo do host. O 'docker compose exec' pode alocar um terminal (TTY), que troca quebras de linha e mistura códigos de controle na saída; em script (cron/CI), a regra da equipe é desligar o TTY explicitamente.",
                "Usando o Compose e SEM alocar TTY, execute no serviço 'db' o comando 'pg_dump -U postgres cafe365' e redirecione a saída para o arquivo 'backup.sql' do host.",
                A("docker compose exec -T db pg_dump -U postgres cafe365 > backup.sql",
                  "docker compose exec -T db pg_dump -U postgres cafe365 >backup.sql",
                  "docker compose exec --no-TTY db pg_dump -U postgres cafe365 > backup.sql"),
                A("É um 'docker compose exec' com uma opção (letra maiúscula) que desliga o pseudo-terminal. O '>' é do shell do HOST: o arquivo nasce fora do container.",
                  "docker compose exec -T <serviço> <comando> > <arquivo>"),
                A("docker compose exec", "Executa um comando no container do serviço que já está rodando.",
                  "-T  (--no-TTY)", "Não aloca TTY: a saída sai byte a byte, sem conversões — o certo para redirecionar ou usar em pipes.",
                  "db", "Serviço do YAML. Tudo depois dele é o comando executado lá dentro.",
                  "pg_dump -U postgres cafe365", "Gera o dump SQL do banco 'cafe365' com o usuário 'postgres'.",
                  "> backup.sql", "Redirecionamento feito pelo shell do host: grava a saída no arquivo local."),
                "Restaurar é o caminho inverso, também sem TTY: docker compose exec -T db psql -U postgres cafe365 < backup.sql. Dump lógico (pg_dump) é consistente mesmo com o banco rodando — diferente de copiar os arquivos do volume.",
                ""));

            list.Add(ExtraDocker_L(2, "Deploy que espera a saúde",
                "No pipeline, o 'docker compose up -d' devolve sucesso assim que os containers são CRIADOS — mesmo que a API ainda esteja subindo ou fique unhealthy. O Jorge quer que o comando só termine quando todos os serviços estiverem rodando/saudáveis, e que falhe se isso passar de 2 minutos.",
                "Suba a stack do Compose em segundo plano esperando os serviços ficarem em execução/saudáveis, com tempo máximo de espera de 120 segundos.",
                A("docker compose up -d --wait --wait-timeout 120", "docker compose up --wait --wait-timeout 120"),
                A("O 'docker compose up' tem uma opção que espera (wait) os serviços ficarem prontos, e outra que define o tempo limite dessa espera, em segundos.",
                  "docker compose up -d --wait --wait-timeout <segundos>"),
                A("docker compose up -d", "Sobe a stack em segundo plano.",
                  "--wait", "Espera cada serviço estar 'running' — ou 'healthy', se tiver healthcheck. Já implica -d.",
                  "--wait-timeout 120", "Desiste após 120 s e sai com erro (código diferente de 0), o que faz o pipeline falhar."),
                "Só faz sentido de verdade com healthcheck nos serviços (no YAML: healthcheck: test: [...]); sem ele, 'rodando' já conta como pronto. Melhor o pipeline vermelho do que o cliente descobrir o erro.",
@"[+] Running 4/4
 ✔ Container cafe-db-1      Healthy
 ✔ Container cafe-cache-1   Healthy
 ✔ Container cafe-api-1     Healthy
 ✔ Container cafe-worker-1  Healthy"));

            list.Add(ExtraDocker_L(2, "Zumbis e fork bomb",
                "O 'conversor' de notas fiscais dispara um processo filho por PDF. Dois problemas: filhos que terminam viram zumbis (o app é o PID 1 e não os recolhe) e, num bug da semana passada, ele criou milhares de processos e travou o servidor.",
                "Rode em segundo plano a imagem 'micorosoft/conversor:1.0' com o nome 'conversor', usando um processo init mínimo como PID 1 e limitando o container a no máximo 100 processos.",
                A("docker run -d --name conversor --init --pids-limit 100 micorosoft/conversor:1.0"),
                A("Uma opção sem valor coloca um 'init' de verdade como PID 1; outra limita o número de processos (PIDs) do container.",
                  "--init --pids-limit <máximo>"),
                A("-d --name conversor", "Segundo plano e nome.",
                  "--init", "Usa o tini como PID 1: ele recolhe processos zumbis e repassa sinais (o docker stop passa a funcionar na hora).",
                  "--pids-limit 100", "Máximo de 100 processos/threads no container: novos fork() falham lá dentro, mas o host continua de pé.",
                  "micorosoft/conversor:1.0", "Imagem."),
                "PID 1 é especial no Linux: ignora sinais sem tratador e precisa recolher os filhos órfãos. Apps comuns (Node, Python, Java) não fazem isso — daí o --init. No Compose: init: true e pids_limit: 100.",
                "6a8c0b1d3f5e7a9c2b4d6f8e0a1c3b5d7f9e2a4c6b8d0f1e3a5c7b9d2f4e6a8c"));

            list.Add(ExtraDocker_L(2, "Segredo fora da imagem",
                "Para instalar pacotes privados, o build precisa do token do registry npm da empresa, que está no arquivo '.npmrc' da pasta atual. A Lia achou um token antigo num 'docker history' — alguém tinha usado --build-arg. O Dockerfile novo já usa 'RUN --mount=type=secret,id=npmrc ...'.",
                "Construa a imagem 'cafeapp:2.2' a partir do diretório atual, entregando ao build o arquivo '.npmrc' como secret de id 'npmrc' (no formato id=...,src=...).",
                A("docker build --secret id=npmrc,src=.npmrc -t cafeapp:2.2 .",
                  "docker build --secret id=npmrc,source=.npmrc -t cafeapp:2.2 .",
                  "docker build --secret src=.npmrc,id=npmrc -t cafeapp:2.2 .",
                  "docker buildx build --secret id=npmrc,src=.npmrc -t cafeapp:2.2 ."),
                A("O BuildKit tem uma opção do 'docker build' que entrega um arquivo ao build SEM gravá-lo em nenhuma camada. O valor tem o id (o mesmo do Dockerfile) e o arquivo de origem.",
                  "--secret id=<id>,src=<arquivo>  -t <nome:tag> ."),
                A("docker build", "Constrói a imagem (com BuildKit, padrão no Docker atual).",
                  "--secret id=npmrc,src=.npmrc", "Disponibiliza o arquivo .npmrc só para os RUN que fazem --mount=type=secret,id=npmrc (por padrão em /run/secrets/npmrc).",
                  "-t cafeapp:2.2", "Nome:tag da imagem.",
                  ".", "Contexto de build."),
                "No Dockerfile: RUN --mount=type=secret,id=npmrc,target=/root/.npmrc npm ci. O secret existe só durante aquele RUN: não vai para camada nenhuma nem para o history. Também dá para ler de uma variável de ambiente: --secret id=npmrc,env=NPM_TOKEN. E coloque .npmrc no .dockerignore!",
@"[+] Building 22.6s (12/12) FINISHED
 => [3/5] RUN --mount=type=secret,id=npmrc,target=/root/.npmrc npm ci
 => exporting to image
 => => naming to docker.io/library/cafeapp:2.2"));

            list.Add(ExtraDocker_L(2, "Uma imagem, duas arquiteturas",
                "Os novos totens de autoatendimento das lojas do Café 365 usam processadores ARM, mas os servidores são x86. O Rafael quer UMA tag que funcione nos dois: cada máquina baixa automaticamente a variante certa.",
                "Com o buildx, construa a partir do diretório atual a imagem 'micorosoft/cafeapp:2.3' para as plataformas linux/amd64 e linux/arm64 (separadas por vírgula) e envie direto para o registry.",
                A("docker buildx build --platform linux/amd64,linux/arm64 -t micorosoft/cafeapp:2.3 --push .",
                  "docker buildx build --platform linux/arm64,linux/amd64 -t micorosoft/cafeapp:2.3 --push ."),
                A("São três ingredientes: a lista de plataformas, a tag e uma opção que publica no registry ao final do build.",
                  "docker buildx build --platform <p1>,<p2> -t <imagem:tag> --push ."),
                A("docker buildx build", "Build com BuildKit e recursos avançados.",
                  "--platform linux/amd64,linux/arm64", "Gera uma variante para cada arquitetura (as diferentes da sua máquina são emuladas via QEMU).",
                  "-t micorosoft/cafeapp:2.3", "Tag publicada (vira um índice multiplataforma).",
                  "--push", "Envia direto ao registry — o armazenamento clássico de imagens não guarda várias plataformas na mesma tag.",
                  ".", "Contexto de build."),
                "Pode ser preciso um builder com suporte a multiplataforma (docker buildx create --use) ou o containerd image store ativado. Confira o resultado com docker buildx imagetools inspect micorosoft/cafeapp:2.3. Emulação é lenta: em CI grande, use máquinas nativas de cada arquitetura.",
@"[+] Building 214.7s (24/24) FINISHED
 => [linux/amd64 4/5] RUN npm ci --omit=dev
 => [linux/arm64 4/5] RUN npm ci --omit=dev
 => exporting to image
 => => pushing layers
 => => pushing manifest for docker.io/micorosoft/cafeapp:2.3"));

            list.Add(ExtraDocker_L(2, "Docker em outro servidor",
                "O Jorge cansou de entrar por SSH no servidor de produção (10.0.0.20) para cada comando. Com um contexto do Docker, a CLI do seu notebook passa a comandar o Engine remoto pelo próprio SSH, com o usuário 'deploy'.",
                "Crie um contexto do Docker chamado 'producao' cujo endpoint seja o Docker do host ssh://deploy@10.0.0.20 (opção --docker com o valor host=...).",
                A("docker context create --docker host=ssh://deploy@10.0.0.20 producao",
                  "docker context create producao --docker host=ssh://deploy@10.0.0.20",
                  "docker context create producao --docker=host=ssh://deploy@10.0.0.20"),
                A("O grupo 'docker context' tem um subcomando para criar contextos. O endpoint vai na opção --docker, no formato host=ENDEREÇO.",
                  "docker context create <nome> --docker host=ssh://<usuário>@<ip>"),
                A("docker context create", "Cria um contexto: um 'perfil' com o endereço do Docker Engine que a CLI vai comandar.",
                  "producao", "Nome do contexto.",
                  "--docker host=ssh://deploy@10.0.0.20", "Endpoint do Engine: via SSH, com o usuário 'deploy' (a chave SSH dele precisa estar configurada)."),
                "Depois: docker context use producao (todos os comandos vão para o servidor) ou, pontualmente, docker --context producao ps. Volte com docker context use default. Cuidado: com o contexto de produção ativo, um 'docker system prune' apaga coisas LÁ — confira com docker context ls (o ativo tem *).",
                "producao\nSuccessfully created context \"producao\""));
        }
    }

    static partial class Docs
    {
        static partial void DocsExtraDocker(List<DocEntry> all)
        {
            const string Dk = "Docker";
            all.Add(X(Dk, "Containers", "docker pause / unpause", "Congela (pause) todos os processos de um container sem encerrá-los, e depois os descongela (unpause). Memória e estado ficam intactos.",
                "docker pause CONTAINER [CONTAINER...]\ndocker unpause CONTAINER [CONTAINER...]",
                null,
                A("docker pause backup-noturno", "suspende o container 'backup-noturno'",
                  "docker unpause backup-noturno", "continua de onde parou"),
                "Diferente do stop, nenhum processo é encerrado. No 'docker ps' o STATUS aparece com '(Paused)'."));

            all.Add(X(Dk, "Volumes e redes", "docker network rm", "Remove uma ou mais redes criadas pelo usuário.",
                "docker network rm REDE [REDE...]",
                A("-f, --force", "não reclama se a rede não existir"),
                A("docker network rm laboratorio-net", "apaga a rede 'laboratorio-net'"),
                "A rede não pode ter containers conectados (desconecte antes com docker network disconnect). As redes padrão bridge, host e none não podem ser removidas."));

            all.Add(X(Dk, "Containers", "--format em listagens (ps, images, stats)", "Escolhe quais colunas aparecem, usando um template Go entre aspas.",
                "docker ps [-a] --format \"{{.Campo}} {{.Campo}}\"\ndocker images --format \"table {{.Campo}}\\t{{.Campo}}\"",
                A("{{.ID}} {{.Names}} {{.Image}}", "(ps) ID, nome e imagem do container",
                  "{{.Status}} {{.Ports}} {{.RunningFor}}", "(ps) status, portas e há quanto tempo foi criado",
                  "{{.Repository}} {{.Tag}} {{.Size}}", "(images) repositório, tag e tamanho",
                  "table", "prefixo que acrescenta cabeçalho e alinha as colunas (use \\t entre elas)"),
                A("docker ps --format \"{{.ID}} {{.Image}}\"", "ID e imagem dos containers em execução",
                  "docker images --format \"{{.Repository}}:{{.Tag}} {{.Size}}\"", "imagens com tamanho, uma por linha",
                  "docker ps -a --format \"table {{.Names}}\\t{{.Ports}}\"", "tabela de nomes e portas, inclusive parados"),
                "Use aspas para o template inteiro (por causa dos espaços). Os nomes dos campos respeitam maiúsculas: .Names, não .names."));

            all.Add(X(Dk, "Containers", "Variáveis em arquivo (--env-file)", "Carrega várias variáveis de ambiente de um arquivo, em vez de repetir -e.",
                "docker run --env-file ARQUIVO [-e CHAVE=VALOR] IMAGEM",
                A("CHAVE=valor", "uma variável por linha",
                  "# comentário", "linhas começando com # são ignoradas",
                  "-e", "pode ser combinado; também passa variáveis avulsas"),
                A("docker run -d --env-file producao.env loja/estoque:3.0", "variáveis lidas de producao.env",
                  "docker run --rm --env-file teste.env -e LOG_LEVEL=debug loja/estoque:3.0", "arquivo + uma variável extra"),
                "Sem 'export' e sem aspas: no --env-file as aspas viram parte do valor. Nunca versione arquivos .env com segredos (coloque no .gitignore e no .dockerignore)."));

            all.Add(X(Dk, "Containers", "Políticas de reinício (--restart)", "Define o que o Docker faz quando o processo principal do container termina ou quando o servidor reinicia.",
                "docker run --restart POLÍTICA IMAGEM\ndocker update --restart POLÍTICA CONTAINER",
                A("no", "nunca reinicia (padrão)",
                  "on-failure[:N]", "reinicia só se sair com erro (código ≠ 0), no máximo N vezes",
                  "always", "sempre reinicia (inclusive após reboot), até mesmo depois de um stop manual quando o daemon reinicia",
                  "unless-stopped", "como always, mas respeita quem parou o container manualmente"),
                A("docker run -d --restart on-failure:5 --name sincronizador loja/sync:1.2", "até 5 novas tentativas em caso de erro",
                  "docker run -d --restart unless-stopped --name painel grafana/grafana", "serviço que deve estar sempre de pé"),
                "Entre tentativas o Docker espera cada vez mais (o atraso dobra a partir de 100 ms). Veja quantas já ocorreram com docker inspect -f '{{.RestartCount}}' CONTAINER."));

            all.Add(X(Dk, "Volumes e redes", "Aliases de rede (--alias / --network-alias)", "Dá um nome DNS extra a um container dentro de uma rede específica.",
                "docker network connect --alias APELIDO REDE CONTAINER\ndocker run --network REDE --network-alias APELIDO IMAGEM",
                A("--alias", "(network connect) apelido do container naquela rede",
                  "--network-alias", "(run) o mesmo, já na criação"),
                A("docker network connect --alias banco escola-net postgres-escola", "na rede 'escola-net', 'postgres-escola' também atende por 'banco'",
                  "docker run -d --network escola-net --network-alias busca opensearchproject/opensearch", "container criado já com o apelido 'busca'"),
                "O apelido vale só na rede onde foi definido. Vários containers com o mesmo alias = o DNS devolve todos (balanceamento simples)."));

            all.Add(X(Dk, "Compose", "docker compose build / pull", "build: (re)constrói as imagens dos serviços que têm 'build:'. pull: baixa as versões mais novas das imagens dos serviços que usam 'image:'.",
                "docker compose build [OPÇÕES] [SERVIÇO...]\ndocker compose pull [SERVIÇO...]",
                A("--no-cache", "(build) ignora o cache de camadas",
                  "--pull", "(build) baixa também a versão mais nova da imagem base",
                  "--build-arg CHAVE=VALOR", "(build) define um ARG"),
                A("docker compose build --no-cache frontend", "reconstrói do zero só o serviço 'frontend'",
                  "docker compose pull banco", "atualiza a imagem do serviço 'banco'"),
                "Nenhum dos dois troca os containers que estão rodando: depois rode docker compose up -d para recriá-los com as imagens novas."));

            all.Add(X(Dk, "Compose", "docker compose up --wait", "Faz o 'up' só terminar quando os serviços estiverem rodando (ou saudáveis, se tiverem healthcheck). Ideal para pipelines de deploy.",
                "docker compose up -d --wait [--wait-timeout SEGUNDOS]",
                A("--wait", "espera os serviços ficarem running/healthy (implica -d)",
                  "--wait-timeout N", "tempo máximo de espera, em segundos; estourou = erro"),
                A("docker compose up --wait", "sobe e espera, sem limite de tempo",
                  "docker compose -f homologacao.yml up -d --wait --wait-timeout 300", "homologação com até 5 minutos de espera"),
                "Se algum serviço ficar unhealthy ou o tempo acabar, o comando sai com código de erro — o pipeline para em vez de seguir com uma aplicação quebrada."));

            all.Add(X(Dk, "Containers", "Logs: driver e rotação (--log-opt)", "Controla como o Docker guarda os logs dos containers. Por padrão o driver json-file NÃO limita o tamanho dos arquivos.",
                "docker run --log-opt max-size=TAMANHO --log-opt max-file=N IMAGEM\ndocker run --log-driver DRIVER IMAGEM",
                A("--log-driver", "json-file (padrão), local, syslog, journald, none...",
                  "--log-opt max-size=", "tamanho máximo de cada arquivo (ex.: 20m)",
                  "--log-opt max-file=", "quantos arquivos rotacionados manter",
                  "--log-opt compress=true", "comprime os arquivos antigos"),
                A("docker run -d --log-opt max-size=50m --log-opt max-file=5 loja/estoque:3.0", "no máximo 250 MB de log",
                  "docker run -d --log-driver local loja/estoque:3.0", "driver 'local': já rotaciona e comprime"),
                "Para valer em todos os containers novos, configure 'log-driver'/'log-opts' em /etc/docker/daemon.json e reinicie o Docker. Containers existentes só mudam quando recriados."));

            all.Add(X(Dk, "Containers", "Templates Go: range e index", "No --format, 'range' percorre listas e mapas; 'index' pega um item de um mapa pela chave.",
                "{{range .Mapa}}{{.Campo}}{{end}}\n{{(index .Mapa \"chave\").Campo}}",
                A("{{range X}} ... {{end}}", "repete o trecho para cada item de X; dentro dele, '.' é o item atual",
                  "{{index X \"chave\"}}", "acessa um item do mapa X pela chave",
                  "{{json X}}", "mostra X em JSON (bom para descobrir os campos)"),
                A("docker inspect -f '{{range .Mounts}}{{.Source}} {{end}}' arquivista", "pastas/volumes montados no container 'arquivista'",
                  "docker inspect -f '{{json .NetworkSettings.Networks}}' arquivista", "todas as redes do container em JSON",
                  "docker inspect -f '{{(index .NetworkSettings.Networks \"escola-net\").Gateway}}' arquivista", "gateway na rede 'escola-net'"),
                "Quando um campo é um mapa ou lista (redes, mounts, portas), use range ou index — não dá para acessar direto com ponto."));

            all.Add(X(Dk, "Containers", "docker events", "Mostra os eventos do Docker Engine (create, start, die, oom, kill, health_status...), do histórico e ao vivo.",
                "docker events [--since TEMPO] [--until TEMPO] [--filter CHAVE=VALOR...]",
                A("--since / --until", "janela de tempo (10m, 2h ou data RFC 3339)",
                  "-f, --filter", "filtra; pode repetir (container=, image=, event=, type=...)",
                  "--format", "template Go para cada evento"),
                A("docker events --since 30m --filter event=oom", "quem foi morto por falta de memória na última meia hora",
                  "docker events --filter container=fila --filter event=restart", "acompanha ao vivo os reinícios do container 'fila'",
                  "docker events --since 2h --until 1h --filter type=image", "operações com imagens na hora retrasada"),
                "Sem --until, o comando fica ouvindo ao vivo (Ctrl+C para sair). No evento 'die', o exitCode ajuda no diagnóstico: 137 = SIGKILL (muitas vezes OOM), 143 = SIGTERM."));

            all.Add(X(Dk, "Containers", "--init e --pids-limit", "--init coloca um init mínimo (tini) como PID 1; --pids-limit limita quantos processos o container pode criar.",
                "docker run --init --pids-limit N IMAGEM",
                A("--init", "recolhe processos zumbis e repassa sinais ao app",
                  "--pids-limit N", "máximo de processos/threads no container (-1 = sem limite)"),
                A("docker run -d --init --name renderizador loja/thumbs:2.0", "evita zumbis num app que cria subprocessos",
                  "docker run --rm --pids-limit 50 alpine sh", "shell que não consegue criar mais de 50 processos"),
                "Proteção contra fork bomb: ao atingir o limite, o fork() falha só dentro do container. No Compose: init: true e pids_limit: N."));

            all.Add(X(Dk, "Imagens", "Segredos no build (--secret)", "Entrega um arquivo ou variável sigilosa a um RUN do Dockerfile sem gravá-lo em nenhuma camada da imagem (BuildKit).",
                "docker build --secret id=ID,src=ARQUIVO -t NOME:TAG .\n(no Dockerfile)  RUN --mount=type=secret,id=ID[,target=CAMINHO] comando",
                A("id=", "identificador usado no Dockerfile",
                  "src= / source=", "arquivo do host com o segredo",
                  "env=", "lê o segredo de uma variável de ambiente",
                  "target=", "(Dockerfile) onde o arquivo aparece; padrão /run/secrets/ID"),
                A("docker build --secret id=pipconf,src=pip.conf -t loja/relatorios:1.4 .", "entrega o pip.conf para instalar pacotes privados",
                  "docker build --secret id=chave_s3,env=AWS_SECRET_ACCESS_KEY -t loja/sync:2.0 .", "segredo vindo de variável de ambiente"),
                "Nunca use --build-arg ou ENV para senhas: os valores ficam visíveis no 'docker history'. O secret só existe durante o RUN que o monta."));

            all.Add(X(Dk, "Imagens", "docker buildx build (multiplataforma)", "Constrói uma mesma tag para várias arquiteturas (ex.: x86 e ARM); quem baixa recebe a variante certa automaticamente.",
                "docker buildx build --platform PLAT1,PLAT2 -t NOME:TAG --push CONTEXTO",
                A("--platform", "lista separada por vírgula: linux/amd64, linux/arm64, linux/arm/v7...",
                  "--push", "envia o resultado direto ao registry",
                  "--load", "carrega no Docker local (só uma plataforma no armazenamento clássico)",
                  "docker buildx create --use", "cria e ativa um builder com suporte a multiplataforma"),
                A("docker buildx build --platform linux/amd64,linux/arm/v7 -t loja/quiosque:1.0 --push .", "imagem para servidores x86 e Raspberry Pi",
                  "docker buildx imagetools inspect loja/quiosque:1.0", "lista as plataformas publicadas na tag"),
                "Plataformas diferentes da sua máquina são emuladas (QEMU) e o build fica bem mais lento."));

            all.Add(X(Dk, "Primeiros passos", "docker context", "Um contexto guarda o endereço de um Docker Engine. Trocando de contexto, a mesma CLI comanda outra máquina (por SSH, por exemplo).",
                "docker context create NOME --docker host=ENDEREÇO\ndocker context use NOME\ndocker context ls",
                A("create", "cria um contexto (ex.: host=ssh://usuario@servidor)",
                  "use", "ativa o contexto para os próximos comandos",
                  "ls", "lista os contextos (o ativo tem *)",
                  "--context NOME", "opção global: usa um contexto só naquele comando"),
                A("docker context create homologacao --docker host=ssh://ci@192.168.10.5", "contexto para o servidor de homologação",
                  "docker --context homologacao ps", "lista os containers de lá, sem trocar o contexto ativo",
                  "docker context use default", "volta para o Docker local"),
                "Cuidado com o contexto ativo: comandos destrutivos (rm, prune) vão para o servidor remoto. A variável DOCKER_HOST, se definida, tem prioridade sobre o contexto."));
        }
    }
}
