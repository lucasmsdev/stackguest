using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DockerQuest
{
    class DocEntry
    {
        public string Cmd, Cat, Summary, Usage, Notes;
        public string Area = "Docker";   // área de tecnologia (nome da categoria)
        public string[] Opts;       // pares: opção, descrição
        public string[] Examples;   // pares: comando, descrição
    }

    // Documentação de referência consultada na Biblioteca. Os exemplos usam nomes diferentes dos das missões
    // de propósito: o jogador precisa entender a sintaxe e adaptar para o contexto da missão.
    static partial class Docs
    {
        public static readonly string[] Categories = { "Primeiros passos", "Containers", "Imagens", "Volumes e redes", "Compose", "Limpeza e sistema" };

        static DocEntry D(string cat, string cmd, string summary, string usage, string[] opts, string[] ex, string notes = null)
        {
            return new DocEntry { Cat = cat, Cmd = cmd, Summary = summary, Usage = usage, Opts = opts ?? new string[0], Examples = ex ?? new string[0], Notes = notes };
        }

        static string[] A(params string[] s) { return s; }

        const string P = "Primeiros passos", C = "Containers", I = "Imagens", V = "Volumes e redes", K = "Compose", L = "Limpeza e sistema";

        public static readonly List<DocEntry> All = new List<DocEntry>
        {
            // ───── Primeiros passos
            D(P, "docker --help", "Mostra a lista de todos os comandos do Docker. Qualquer comando aceita --help para mostrar suas próprias opções.",
                "docker --help\ndocker <COMANDO> --help",
                null,
                A("docker --help", "lista todos os comandos disponíveis",
                  "docker logs --help", "mostra as opções do comando logs"),
                "Esse é o atalho mais útil de todos: quando não lembrar uma opção, peça ajuda ao próprio comando."),
            D(P, "docker version / --version", "Mostra a versão do Docker instalada.",
                "docker version\ndocker --version",
                A("version (sem traços)", "subcomando que mostra detalhes do cliente E do servidor (Engine, API, containerd...)",
                  "--version  (-v)", "opção global que imprime só a versão do cliente, em uma linha"),
                null,
                "Use para conferir se o Docker está instalado. Para o estado geral do ambiente, veja 'docker info'."),
            D(P, "docker info", "Exibe informações gerais do sistema Docker: quantidade de containers e imagens, storage driver, CPUs, memória, versão do servidor etc.",
                "docker info\ndocker system info",
                A("-f, --format", "formata a saída com um template Go"),
                null,
                "Se der erro de conexão, o daemon (Docker Engine) não está rodando — no Windows/Mac, abra o Docker Desktop."),
            D(P, "docker pull", "Baixa uma imagem de um registry (por padrão, o Docker Hub) SEM criar container.",
                "docker pull [OPÇÕES] NOME[:TAG]",
                A("-a, --all-tags", "baixa todas as tags do repositório",
                  "--platform", "escolhe a plataforma (ex.: linux/arm64)"),
                A("docker pull redis", "baixa a imagem redis com a tag padrão (latest)",
                  "docker pull python:3.12-slim", "baixa uma tag específica"),
                "Sem tag, o Docker usa ':latest'."),
            D(P, "docker images / docker image ls", "Lista as imagens que existem localmente.",
                "docker images [OPÇÕES]\ndocker image ls [OPÇÕES]",
                A("-a, --all", "inclui imagens intermediárias",
                  "-q, --quiet", "mostra apenas os IDs",
                  "-f, --filter", "filtra (ex.: dangling=true)"),
                A("docker images -q", "lista só os IDs das imagens"),
                null),
            D(P, "docker run", "Cria um container NOVO a partir de uma imagem e o inicia. Se a imagem não existir localmente, faz o pull automaticamente.",
                "docker run [OPÇÕES] IMAGEM [COMANDO] [ARGUMENTOS...]",
                A("-d, --detach", "roda em segundo plano e devolve o terminal",
                  "--name NOME", "dá um nome ao container",
                  "-p, --publish HOST:CONTAINER", "publica uma porta do container numa porta do host",
                  "-P, --publish-all", "publica todas as portas EXPOSE em portas aleatórias",
                  "-i, --interactive", "mantém a entrada padrão aberta",
                  "-t, --tty", "aloca um terminal (use junto com -i: -it)",
                  "--rm", "remove o container automaticamente quando ele terminar",
                  "-e, --env CHAVE=VALOR", "define uma variável de ambiente (pode repetir)",
                  "--env-file ARQUIVO", "lê variáveis de um arquivo",
                  "--restart POLÍTICA", "no | on-failure[:N] | always | unless-stopped",
                  "-m, --memory LIMITE", "limite de memória (ex.: 512m, 1g)",
                  "--cpus N", "limite de CPUs (ex.: 1.5)",
                  "-v, --volume ORIGEM:DESTINO[:ro]", "monta um volume ou pasta do host",
                  "--network REDE", "conecta o container a uma rede",
                  "-w, --workdir DIR", "diretório de trabalho dentro do container"),
                A("docker run -d --name meu-redis redis", "Redis em segundo plano com nome",
                  "docker run -d -p 3000:80 httpd", "servidor Apache acessível em localhost:3000",
                  "docker run -it alpine sh", "abre um shell interativo num Alpine",
                  "docker run --rm busybox date", "executa 'date' e apaga o container ao terminar",
                  "docker run -d -e TZ=America/Sao_Paulo --restart unless-stopped minha-api", "variável de ambiente + política de reinício"),
                "IMPORTANTE: as opções vêm ANTES do nome da imagem. Tudo que vem depois da imagem é o comando executado DENTRO do container."),
            D(P, "docker ps / docker container ls", "Lista containers. Sem opções, mostra apenas os que estão em execução.",
                "docker ps [OPÇÕES]",
                A("-a, --all", "mostra todos, inclusive parados",
                  "-q, --quiet", "mostra apenas os IDs",
                  "-f, --filter", "filtra (ex.: status=exited, name=api)",
                  "-s, --size", "mostra o tamanho"),
                A("docker ps -q", "IDs dos containers em execução",
                  "docker ps -f status=exited", "apenas os containers parados"),
                null),

            // ───── Containers
            D(C, "docker stop", "Para um ou mais containers em execução de forma graciosa: envia SIGTERM e, após o tempo limite, SIGKILL.",
                "docker stop [OPÇÕES] CONTAINER [CONTAINER...]",
                A("-t, --timeout SEG", "segundos de espera antes de forçar (padrão 10)"),
                A("docker stop meu-redis", "para o container meu-redis",
                  "docker stop api worker", "para dois containers"),
                null),
            D(C, "docker start", "Inicia um container EXISTENTE que está parado (não cria um novo).",
                "docker start [OPÇÕES] CONTAINER [CONTAINER...]",
                A("-a, --attach", "anexa à saída do container",
                  "-i, --interactive", "anexa a entrada padrão"),
                A("docker start meu-redis", "liga de novo um container parado"),
                "Diferença: 'run' cria um container novo a partir da imagem; 'start' reaproveita um que já existe."),
            D(C, "docker restart", "Reinicia (para e inicia) um ou mais containers.",
                "docker restart [OPÇÕES] CONTAINER [CONTAINER...]",
                A("-t, --timeout SEG", "segundos de espera antes de forçar a parada"),
                A("docker restart api", "reinicia o container api"),
                null),
            D(C, "docker kill", "Mata imediatamente um container (SIGKILL por padrão), sem encerramento gracioso.",
                "docker kill [OPÇÕES] CONTAINER",
                A("-s, --signal", "sinal a enviar (ex.: SIGHUP)"),
                A("docker kill travado", "mata o container 'travado'"),
                "Prefira 'docker stop'; use kill só quando o container não responde."),
            D(C, "docker logs", "Mostra os logs (STDOUT e STDERR) de um container.",
                "docker logs [OPÇÕES] CONTAINER",
                A("-f, --follow", "continua acompanhando novas linhas (como tail -f)",
                  "-n, --tail N", "mostra só as últimas N linhas",
                  "--since TEMPO", "mostra a partir de um tempo (ex.: 10m, 2h)",
                  "-t, --timestamps", "mostra data e hora de cada linha"),
                A("docker logs api", "todos os logs do container api",
                  "docker logs -n 100 api", "as últimas 100 linhas",
                  "docker logs --follow --since 5m worker", "acompanha ao vivo desde 5 minutos atrás"),
                null),
            D(C, "docker exec", "Executa um comando dentro de um container que JÁ ESTÁ EM EXECUÇÃO.",
                "docker exec [OPÇÕES] CONTAINER COMANDO [ARGUMENTOS...]",
                A("-i, --interactive", "mantém a entrada aberta",
                  "-t, --tty", "aloca terminal (use -it para shells)",
                  "-u, --user", "executa como outro usuário",
                  "-w, --workdir", "diretório de trabalho",
                  "-e, --env", "variável de ambiente para o comando"),
                A("docker exec -it meu-redis redis-cli", "abre o cliente do Redis dentro do container",
                  "docker exec api cat /etc/hostname", "roda um comando e mostra a saída, sem modo interativo",
                  "docker exec -it app sh", "abre um shell sh"),
                "Imagens baseadas em Debian/Ubuntu costumam ter 'bash'; imagens Alpine geralmente só têm 'sh'."),
            D(C, "docker inspect", "Mostra detalhes de baixo nível (JSON) de containers, imagens, volumes ou redes: IP, mounts, variáveis, configuração...",
                "docker inspect [OPÇÕES] NOME|ID [NOME|ID...]",
                A("-f, --format", "extrai um campo com template Go",
                  "--type", "container | image | volume | network"),
                A("docker inspect api", "JSON completo do container api",
                  "docker inspect -f '{{.State.Status}}' api", "só o status"),
                null),
            D(C, "docker stats", "Painel ao vivo com uso de CPU, memória, rede e disco dos containers em execução.",
                "docker stats [OPÇÕES] [CONTAINER...]",
                A("--no-stream", "mostra uma única leitura e sai",
                  "-a, --all", "inclui containers parados"),
                A("docker stats --no-stream", "uma foto do consumo atual",
                  "docker stats api db", "acompanha só dois containers"),
                null),
            D(C, "docker top", "Lista os processos em execução dentro de um container.",
                "docker top CONTAINER [OPÇÕES DO ps]",
                null,
                A("docker top api", "processos do container api"),
                null),
            D(C, "docker cp", "Copia arquivos e pastas entre um container e o host (nos dois sentidos).",
                "docker cp [OPÇÕES] CONTAINER:ORIGEM DESTINO\ndocker cp [OPÇÕES] ORIGEM CONTAINER:DESTINO",
                A("-a, --archive", "preserva dono e permissões"),
                A("docker cp api:/app/config.json .", "copia do container para a pasta atual do host",
                  "docker cp ./index.html app:/usr/share/html/", "copia do host para o container"),
                "Funciona mesmo com o container parado."),
            D(C, "docker rename", "Renomeia um container existente.",
                "docker rename CONTAINER NOVO_NOME",
                null,
                A("docker rename teste producao", "o container 'teste' passa a se chamar 'producao'"),
                null),
            D(C, "docker rm", "Remove um ou mais containers. Por padrão só remove containers PARADOS.",
                "docker rm [OPÇÕES] CONTAINER [CONTAINER...]",
                A("-f, --force", "força a remoção de um container em execução (mata antes)",
                  "-v, --volumes", "remove também os volumes anônimos associados"),
                A("docker rm antigo", "remove o container parado 'antigo'",
                  "docker rm -f teimoso", "remove mesmo estando em execução"),
                "Remover o container não remove a imagem."),
            D(C, "docker port", "Mostra os mapeamentos de porta de um container.",
                "docker port CONTAINER [PORTA]",
                null,
                A("docker port api", "ex.: 80/tcp -> 0.0.0.0:3000"),
                null),

            // ───── Imagens
            D(I, "Dockerfile (o básico)", "Arquivo de texto com as instruções para construir uma imagem.",
                "FROM imagem-base\nWORKDIR /app\nCOPY . .\nRUN comando-de-instalação\nEXPOSE 8080\nCMD [\"programa\", \"argumento\"]",
                A("FROM", "imagem base",
                  "WORKDIR", "diretório de trabalho",
                  "COPY / ADD", "copia arquivos do contexto para a imagem",
                  "RUN", "executa um comando durante o build (gera uma camada)",
                  "ENV", "define variável de ambiente",
                  "EXPOSE", "documenta a porta usada",
                  "CMD / ENTRYPOINT", "comando executado quando o container inicia"),
                null,
                "Cada instrução gera uma camada com cache. Use .dockerignore para não enviar arquivos desnecessários."),
            D(I, "docker build", "Constrói uma imagem a partir de um Dockerfile e de um CONTEXTO (normalmente uma pasta).",
                "docker build [OPÇÕES] CONTEXTO",
                A("-t, --tag NOME:TAG", "nome e tag da imagem gerada (pode repetir)",
                  "-f, --file ARQUIVO", "usa outro Dockerfile (padrão: CONTEXTO/Dockerfile)",
                  "--no-cache", "não usa o cache de camadas",
                  "--pull", "baixa sempre a versão mais nova da imagem base",
                  "--build-arg CHAVE=VALOR", "argumento de build (ARG)",
                  "--target ESTÁGIO", "para num estágio de um multi-stage build"),
                A("docker build -t minha-api:2.0 .", "constrói a partir da pasta atual (o ponto é o contexto!)",
                  "docker build -f docker/Dockerfile.teste -t minha-api:teste .", "usa um Dockerfile em outro caminho",
                  "docker build --pull -t site:latest ./site", "contexto na pasta ./site"),
                "Não esqueça do CONTEXTO no final do comando (geralmente '.')."),
            D(I, "docker tag", "Cria um novo nome (tag) para uma imagem existente. Não copia nada: é só outro nome para o mesmo ID.",
                "docker tag ORIGEM[:TAG] DESTINO[:TAG]",
                null,
                A("docker tag minha-api:2.0 empresa/minha-api:2.0", "prepara para enviar ao usuário/organização 'empresa'",
                  "docker tag minha-api:2.0 ghcr.io/empresa/minha-api:2.0", "tag para outro registry"),
                "O prefixo antes da barra indica o usuário/organização no registry."),
            D(I, "docker login / logout", "Autentica (ou sai) em um registry. Sem endereço, usa o Docker Hub.",
                "docker login [OPÇÕES] [SERVIDOR]",
                A("-u, --username", "usuário",
                  "--password-stdin", "lê a senha/token da entrada padrão (bom para CI)"),
                A("docker login ghcr.io", "login no GitHub Container Registry"),
                null),
            D(I, "docker push", "Envia uma imagem para o registry indicado no nome dela.",
                "docker push [OPÇÕES] NOME[:TAG]",
                A("-a, --all-tags", "envia todas as tags do repositório"),
                A("docker push empresa/minha-api:2.0", "envia a imagem para o Docker Hub"),
                "É preciso estar logado e a imagem precisa ter o prefixo usuário/organização."),
            D(I, "docker history", "Mostra o histórico de camadas de uma imagem: qual instrução criou cada camada e o tamanho.",
                "docker history [OPÇÕES] IMAGEM",
                A("--no-trunc", "não corta o texto das instruções",
                  "-H, --human", "tamanhos legíveis (padrão)"),
                A("docker history python:3.12-slim", "camadas da imagem do Python"),
                null),
            D(I, "docker commit", "Cria uma nova imagem a partir das alterações feitas em um container.",
                "docker commit [OPÇÕES] CONTAINER [REPOSITÓRIO[:TAG]]",
                A("-m, --message", "mensagem do commit",
                  "-a, --author", "autor"),
                A("docker commit experimento laboratorio:v1", "salva o estado do container como imagem"),
                "Prefira Dockerfile em projetos reais: é reprodutível."),
            D(I, "docker save", "Exporta uma ou mais imagens para um arquivo .tar (com todas as camadas).",
                "docker save [OPÇÕES] IMAGEM [IMAGEM...]",
                A("-o, --output ARQUIVO", "arquivo de saída (em vez de STDOUT)"),
                A("docker save -o backup.tar redis:7", "salva a imagem redis:7 em backup.tar",
                  "docker save minha-api:2.0 > api.tar", "o mesmo usando redirecionamento"),
                "Não confunda com 'docker export', que exporta o sistema de arquivos de um CONTAINER."),
            D(I, "docker load", "Importa imagens de um arquivo .tar criado pelo docker save.",
                "docker load [OPÇÕES]",
                A("-i, --input ARQUIVO", "lê do arquivo (em vez de STDIN)",
                  "-q, --quiet", "menos saída"),
                A("docker load -i backup.tar", "carrega as imagens de backup.tar",
                  "docker load < api.tar", "o mesmo usando redirecionamento"),
                null),
            D(I, "docker rmi / docker image rm", "Remove uma ou mais imagens locais.",
                "docker rmi [OPÇÕES] IMAGEM [IMAGEM...]",
                A("-f, --force", "força a remoção"),
                A("docker rmi python:3.11", "remove essa tag/imagem"),
                "Se algum container (mesmo parado) usa a imagem, a remoção falha."),

            // ───── Volumes e redes
            D(V, "Montando volumes no docker run (-v)", "Como persistir dados ou compartilhar pastas usando a opção -v (ou --mount) do docker run.",
                "-v NOME_DO_VOLUME:/caminho/no/container\n-v /caminho/no/host:/caminho/no/container[:ro]",
                A("volume nomeado", "origem é só um NOME → volume gerenciado pelo Docker",
                  "bind mount", "origem é um CAMINHO do host (começa com / ou .) → espelha a pasta",
                  ":ro", "monta como somente leitura"),
                A("docker run -d -v dados-redis:/data redis", "volume nomeado 'dados-redis' em /data",
                  "docker run -d -v /home/ana/fotos:/srv/fotos nginx", "bind mount de uma pasta do host"),
                "Dados gravados fora de volumes somem quando o container é removido."),
            D(V, "docker volume create", "Cria um volume nomeado gerenciado pelo Docker.",
                "docker volume create [OPÇÕES] [NOME]",
                A("-d, --driver", "driver do volume (padrão: local)"),
                A("docker volume create arquivos", "cria o volume 'arquivos'"),
                null),
            D(V, "docker volume ls", "Lista os volumes.",
                "docker volume ls [OPÇÕES]",
                A("-q, --quiet", "só os nomes", "-f, --filter", "filtra (ex.: dangling=true)"),
                null, null),
            D(V, "docker volume inspect", "Mostra detalhes de um volume (ponto de montagem, driver, data).",
                "docker volume inspect VOLUME",
                null, A("docker volume inspect arquivos", "detalhes do volume 'arquivos'"), null),
            D(V, "docker volume rm", "Remove um ou mais volumes (que não estejam em uso).",
                "docker volume rm VOLUME [VOLUME...]",
                A("-f, --force", "força"), A("docker volume rm arquivos", "apaga o volume 'arquivos' e seus dados"), null),
            D(V, "docker network create", "Cria uma rede. Em redes criadas pelo usuário os containers se encontram pelo NOME (DNS interno).",
                "docker network create [OPÇÕES] NOME",
                A("-d, --driver", "bridge (padrão), overlay, macvlan...",
                  "--subnet", "sub-rede (ex.: 172.30.0.0/16)"),
                A("docker network create backend", "cria a rede 'backend'"),
                null),
            D(V, "docker network ls", "Lista as redes (inclui as padrão: bridge, host, none).",
                "docker network ls [OPÇÕES]",
                A("-q, --quiet", "só os IDs"), null, null),
            D(V, "docker network connect / disconnect", "Conecta (ou desconecta) um container EXISTENTE a uma rede.",
                "docker network connect [OPÇÕES] REDE CONTAINER\ndocker network disconnect REDE CONTAINER",
                A("--alias", "nome adicional do container nessa rede"),
                A("docker network connect backend worker", "liga o container 'worker' à rede 'backend'"),
                "Atenção à ordem: primeiro a REDE, depois o CONTAINER. Um container pode estar em várias redes."),
            D(V, "docker network inspect", "Mostra detalhes de uma rede: sub-rede, gateway e containers conectados com seus IPs.",
                "docker network inspect REDE",
                null, A("docker network inspect backend", "detalhes da rede 'backend'"), null),
            D(V, "--network no docker run", "Para criar um container já conectado a uma rede, use --network no docker run.",
                "docker run --network REDE [OUTRAS OPÇÕES] IMAGEM",
                null,
                A("docker run -d --name fila --network backend rabbitmq", "container 'fila' criado direto na rede 'backend'"),
                "Os outros containers da mesma rede acessam este pelo nome (ex.: fila:5672)."),

            // ───── Compose
            D(K, "docker compose (visão geral)", "Gerencia aplicações com vários containers descritas em um arquivo YAML (compose.yaml ou docker-compose.yml) na pasta atual.",
                "docker compose [OPÇÕES GLOBAIS] SUBCOMANDO [OPÇÕES]",
                A("-f, --file ARQUIVO", "usa outro arquivo compose (opção GLOBAL: vem antes do subcomando)",
                  "-p, --project-name", "nome do projeto"),
                A("docker compose -f compose.teste.yml ps", "usa o arquivo compose.teste.yml"),
                "Versão atual: 'docker compose' (com espaço). A antiga era 'docker-compose' (com hífen). Os comandos usam os nomes dos SERVIÇOS do YAML."),
            D(K, "docker compose up", "Cria redes, volumes e containers e inicia todos os serviços do arquivo compose.",
                "docker compose up [OPÇÕES] [SERVIÇO...]",
                A("-d, --detach", "em segundo plano",
                  "--build", "reconstrói as imagens antes de subir",
                  "--scale SERVIÇO=N", "define quantas réplicas de um serviço",
                  "--force-recreate", "recria os containers mesmo sem mudanças"),
                A("docker compose up", "sobe tudo em primeiro plano (logs na tela)",
                  "docker compose up -d web", "sobe só o serviço 'web' em segundo plano",
                  "docker compose up -d --scale worker=4", "4 réplicas do serviço 'worker'"),
                null),
            D(K, "docker compose ps", "Lista os containers do projeto compose atual e seus status.",
                "docker compose ps [OPÇÕES]",
                A("-a, --all", "inclui parados"), null, null),
            D(K, "docker compose logs", "Mostra os logs dos serviços (de todos, se nenhum for informado).",
                "docker compose logs [OPÇÕES] [SERVIÇO...]",
                A("-f, --follow", "acompanha ao vivo", "-n, --tail N", "últimas N linhas"),
                A("docker compose logs worker", "logs do serviço 'worker'",
                  "docker compose logs -n 20 -f", "últimas 20 linhas de todos e acompanha"),
                null),
            D(K, "docker compose exec / run", "exec: roda um comando num serviço EM EXECUÇÃO (já aloca terminal). run: cria um container NOVO e temporário do serviço.",
                "docker compose exec [OPÇÕES] SERVIÇO COMANDO [ARGS...]\ndocker compose run [OPÇÕES] SERVIÇO [COMANDO]",
                A("-T", "não aloca terminal (bom para scripts)", "--rm (run)", "remove o container do run ao terminar"),
                A("docker compose exec cache redis-cli", "cliente Redis dentro do serviço 'cache'",
                  "docker compose run --rm web npm test", "roda os testes num container temporário"),
                null),
            D(K, "docker compose down", "Para e remove os containers e redes criados pelo 'up'.",
                "docker compose down [OPÇÕES]",
                A("-v, --volumes", "remove também os volumes declarados no YAML",
                  "--rmi all|local", "remove também as imagens"),
                A("docker compose down", "derruba a stack mantendo os volumes (dados)"),
                "Cuidado: com -v os dados dos volumes são apagados."),
            D(K, "docker compose stop / start / restart", "Para, inicia ou reinicia serviços sem remover os containers.",
                "docker compose stop [SERVIÇO...]",
                null, A("docker compose restart web", "reinicia só o serviço 'web'"), null),

            // ───── Limpeza e sistema
            D(L, "docker system df", "Mostra quanto espaço em disco o Docker usa: imagens, containers, volumes e cache de build.",
                "docker system df [OPÇÕES]",
                A("-v, --verbose", "detalha item por item"), null, null),
            D(L, "docker system prune", "Remove de uma vez containers parados, redes sem uso, imagens dangling e cache de build.",
                "docker system prune [OPÇÕES]",
                A("-a, --all", "remove TODAS as imagens sem container, não só as dangling",
                  "--volumes", "remove também volumes sem uso",
                  "-f, --force", "não pede confirmação"),
                A("docker system prune", "limpeza básica (pede confirmação)"),
                null),
            D(L, "docker container prune", "Remove todos os containers parados.",
                "docker container prune [OPÇÕES]",
                A("-f, --force", "não pede confirmação", "--filter", "ex.: until=24h"),
                null, null),
            D(L, "docker image prune", "Remove imagens 'dangling' (sem tag, aparecem como <none>).",
                "docker image prune [OPÇÕES]",
                A("-a, --all", "remove todas as imagens sem container associado", "-f, --force", "não pede confirmação"),
                null, null),
            D(L, "docker volume prune", "Remove volumes que não estão sendo usados por nenhum container.",
                "docker volume prune [OPÇÕES]",
                A("-a, --all", "inclui volumes nomeados (versões recentes)", "-f, --force", "não pede confirmação"),
                null, "Dados apagados não voltam!"),
            D(L, "docker network prune", "Remove redes que não estão sendo usadas.",
                "docker network prune [OPÇÕES]",
                A("-f, --force", "não pede confirmação"), null, null),
        };

        public static string Fold(string s)
        {
            var d = s.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in d) if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            return sb.ToString();
        }

        public static string SearchText(DocEntry e)
        {
            return Fold(e.Cmd + " " + e.Area + " " + e.Cat + " " + e.Summary + " " + e.Usage + " " + string.Join(" ", e.Opts) + " " + string.Join(" ", e.Examples) + " " + (e.Notes ?? ""));
        }
    }

    // Tela de pesquisa da Biblioteca.
    class DocsForm : Form
    {
        readonly GameState gs;
        readonly float scale;
        readonly TextBox search;
        readonly ListBox list;
        readonly RichTextBox page;
        readonly Label lblCat;
        string category;
        readonly Font fItem, fHead, fBold, fMono, fMonoB, fText, fH1;
        int S(float v) { return (int)Math.Round(v * scale); }

        public DocsForm(GameState gs, float scale, string category)
        {
            this.gs = gs; this.scale = scale; this.category = category;
            Text = "Micorosoft — Biblioteca: documentação técnica";
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            BackColor = Theme.Panel;
            ForeColor = Theme.Text;
            var wa = Screen.PrimaryScreen.WorkingArea;
            ClientSize = new Size(Math.Min(S(1180), wa.Width - 40), Math.Min(S(760), wa.Height - 60));
            MinimumSize = new Size(S(800), S(500));
            fItem = new Font("Consolas", 10.5f, FontStyle.Bold);
            fHead = new Font("Segoe UI Semibold", 8.5f);
            fBold = new Font("Segoe UI Semibold", 11f);
            fMono = new Font("Consolas", 11f);
            fMonoB = new Font("Consolas", 11.5f, FontStyle.Bold);
            fText = new Font("Segoe UI", 11f);
            fH1 = new Font("Consolas", 18f, FontStyle.Bold);

            var header = new Panel { Dock = DockStyle.Top, Height = S(92), Padding = new Padding(S(20), S(12), S(20), 0) };
            var title = new Label { Text = "BIBLIOTECA DA MICOROSOFT · DOCUMENTAÇÃO TÉCNICA", Font = new Font("Segoe UI Black", 14f), ForeColor = Theme.Docker, AutoSize = true, Location = new Point(S(20), S(12)), UseMnemonic = false };
            var note = new Label
            {
                Text = "Pesquisar aqui é GRÁTIS. Busque por comando, função ou palavra (ex.: logs, flexbox, useState, bucket, porta, foreach). Os exemplos usam outros nomes: adapte para o seu desafio!",
                Font = new Font("Segoe UI", 9.5f), ForeColor = Theme.Muted, Location = new Point(S(22), S(48)), Size = new Size(ClientSize.Width - S(44), S(40)),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            header.Controls.Add(title);
            header.Controls.Add(note);

            var left = new Panel { Dock = DockStyle.Left, Width = S(370), Padding = new Padding(S(16), S(4), S(8), S(16)) };
            search = new TextBox { Dock = DockStyle.Top, Font = new Font("Segoe UI", 12f), BackColor = Theme.Term, ForeColor = Theme.Bright, BorderStyle = BorderStyle.FixedSingle };
            search.TextChanged += (s, e) => Fill();
            search.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Down && list.Items.Count > 0) { list.Focus(); e.Handled = true; }
                if (e.KeyCode == Keys.Escape) Close();
            };
            var searchLbl = new Label { Text = "🔍 Pesquisar:", Dock = DockStyle.Top, Height = S(26), Font = fBold, ForeColor = Theme.Bright };
            lblCat = new Label { Dock = DockStyle.Top, Height = S(30), Font = new Font("Segoe UI", 9.5f), ForeColor = Theme.Yellow, Cursor = Cursors.Hand, TextAlign = ContentAlignment.MiddleLeft };
            lblCat.Click += (s, e) => { this.category = null; Fill(); };
            list = new ListBox
            {
                Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, BackColor = Theme.Bg, ForeColor = Theme.Text, DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = S(34), IntegralHeight = false
            };
            Program.DarkScroll(list);
            list.DrawItem += DrawItem;
            list.SelectedIndexChanged += (s, e) => ShowPage();
            var gap = new Panel { Dock = DockStyle.Top, Height = S(8) };
            left.Controls.Add(list);
            left.Controls.Add(gap);
            left.Controls.Add(lblCat);
            left.Controls.Add(search);
            left.Controls.Add(searchLbl);

            var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(S(8), S(4), S(20), S(16)) };
            var pageWrap = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Term, Padding = new Padding(S(18), S(14), S(8), S(8)) };
            page = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = Theme.Term, ForeColor = Theme.Text, DetectUrls = false, ScrollBars = RichTextBoxScrollBars.Vertical };
            Program.DarkScroll(page);
            pageWrap.Controls.Add(page);
            right.Controls.Add(pageWrap);

            Controls.Add(right);
            Controls.Add(left);
            Controls.Add(header);
            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
            Shown += (s, e) => search.Focus();
            Fill();
        }

        void Fill()
        {
            var words = Docs.Fold(search.Text).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            list.BeginUpdate();
            list.Items.Clear();
            foreach (var area in Challenges.Cats.Select(ct => ct.Name))
            {
                if (category != null && area != category) continue;
                var subs = Docs.All.Where(d => d.Area == area).Select(d => d.Cat).Distinct().ToList();
                foreach (var sub in subs)
                {
                    var items = Docs.All.Where(d => d.Area == area && d.Cat == sub && words.All(w => Docs.SearchText(d).Contains(w))).ToList();
                    if (items.Count == 0) continue;
                    list.Items.Add((area + " · " + sub).ToUpperInvariant());
                    foreach (var d in items) list.Items.Add(d);
                }
            }
            list.EndUpdate();
            lblCat.Text = category != null ? "Estante: " + category + "   (clique aqui para ver tudo)" : list.Items.Count == 0 ? "Nada encontrado. Tente outra palavra." : "";
            for (int i = 0; i < list.Items.Count; i++) if (list.Items[i] is DocEntry) { list.SelectedIndex = i; break; }
            if (list.SelectedIndex < 0) page.Clear();
        }

        void DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var g = e.Graphics;
            var r = e.Bounds;
            g.FillRectangle(Art.B(Theme.Bg), r);
            var it = list.Items[e.Index];
            var d = it as DocEntry;
            if (d == null)
            {
                TextRenderer.DrawText(g, (string)it, fHead, new Rectangle(r.X + S(8), r.Y + S(8), r.Width, r.Height - S(8)), Theme.Docker, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                return;
            }
            bool sel = (e.State & DrawItemState.Selected) != 0;
            if (sel) { g.FillRectangle(Art.B(Theme.Selected), r.X + S(2), r.Y + S(1), r.Width - S(4), r.Height - S(2)); g.FillRectangle(Art.B(Theme.Docker), r.X + S(2), r.Y + S(1), S(3), r.Height - S(2)); }
            bool read = gs.Prog.DocsRead.Contains(d.Cmd);
            TextRenderer.DrawText(g, d.Cmd, fItem, new Rectangle(r.X + S(14), r.Y, r.Width - S(40), r.Height), sel ? Theme.Bright : Theme.Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            if (read) TextRenderer.DrawText(g, "✔", fHead, new Rectangle(r.Right - S(28), r.Y, S(22), r.Height), Theme.Green, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        void Put(string text, Color c, Font f)
        {
            page.SelectionStart = page.TextLength;
            page.SelectionLength = 0;
            page.SelectionColor = c;
            page.SelectionFont = f;
            page.SelectedText = text;
        }

        void ShowPage()
        {
            var d = list.SelectedItem as DocEntry;
            if (d == null)
            {
                // cabeçalho de categoria: pula para o próximo item
                if (list.SelectedIndex >= 0 && list.SelectedIndex + 1 < list.Items.Count) list.SelectedIndex++;
                return;
            }
            if (gs.Prog.DocsRead.Add(d.Cmd)) { gs.Prog.Save(); list.Invalidate(); }
            page.Clear();
            Put((d.Area + " · " + d.Cat).ToUpperInvariant() + "\n", Theme.Docker, fHead);
            Put(d.Cmd + "\n", Theme.Bright, fH1);
            Put(d.Summary + "\n\n", Theme.Text, fText);
            Put("USO\n", Theme.Yellow, fBold);
            foreach (var line in d.Usage.Split('\n')) Put("  " + line + "\n", Theme.Green, fMonoB);
            if (d.Opts.Length > 0)
            {
                Put("\nOPÇÕES PRINCIPAIS\n", Theme.Yellow, fBold);
                for (int i = 0; i + 1 < d.Opts.Length; i += 2)
                {
                    Put("  " + d.Opts[i] + "\n", Theme.Cyan, fMonoB);
                    Put("      " + d.Opts[i + 1] + "\n", Theme.Text, fText);
                }
            }
            if (d.Examples.Length > 0)
            {
                Put("\nEXEMPLOS\n", Theme.Yellow, fBold);
                for (int i = 0; i + 1 < d.Examples.Length; i += 2)
                {
                    bool shell = d.Area == "Docker" || d.Area == "AWS" || d.Area == "Redes" || d.Examples[i].StartsWith("npm ") || d.Examples[i].StartsWith("flutter ") ||
                                 d.Examples[i].StartsWith("pip ") || d.Examples[i].StartsWith("python ") || d.Examples[i].StartsWith("php ") || d.Examples[i].StartsWith("composer ");
                    Put((d.Area == "SQL" ? "  mysql> " : shell ? "  $ " : "  › ") + d.Examples[i] + "\n", Theme.Bright, fMono);
                    Put("      # " + d.Examples[i + 1] + "\n", Theme.Muted, fText);
                }
            }
            if (!string.IsNullOrEmpty(d.Notes))
            {
                Put("\nIMPORTANTE\n", Theme.Yellow, fBold);
                Put("  " + d.Notes + "\n", Theme.Text, fText);
            }
            page.SelectionStart = 0;
            page.ScrollToCaret();
        }
    }
}
