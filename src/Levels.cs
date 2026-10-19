using System.Collections.Generic;

namespace DockerQuest
{
    class Level
    {
        public string Chapter;
        public string Title;
        public string Context;
        public string Objective;
        public string[] Answers;   // a primeira é a forma de referência
        public string[] Hints;
        public string[] Explain;   // pares: parte do comando, explicação
        public string Summary;
        public string Output;      // saída simulada
        public string[] Ignore;    // flags opcionais que não alteram o sentido (ex.: -f no prune)
        public string Cat = "docker";   // categoria (docker, front, react, flutter, aws, redes, python, php)
        public int Tier;                // 0 iniciante, 1 intermediário, 2 profissional
        public string Kind = "docker";  // docker | cmd | code | html | text | aws (tarefa no console AWS simulado)
        public string Task;
        public int Order;               // ordem dentro do nível (maior = mais para o fim); empate = posição na lista             // aws: id da tarefa no console simulado (AwsTasks)
    }

    static class Levels
    {
        static string[] A(params string[] s) { return s; }

        static Level L(string ch, string title, string ctx, string obj, string[] ans, string[] hints,
                       string[] explain, string summary, string output, params string[] ignore)
        {
            return new Level {
                Chapter = ch, Title = title, Context = ctx, Objective = obj, Answers = ans, Hints = hints,
                Explain = explain, Summary = summary, Output = output, Ignore = ignore
            };
        }

        const string C1 = "Capítulo 1 · Primeiros Passos";
        const string C2 = "Capítulo 2 · Rodando Containers";
        const string C3 = "Capítulo 3 · Gerenciando Containers";
        const string C4 = "Capítulo 4 · Imagens & Dockerfile";
        const string C5 = "Capítulo 5 · Volumes & Redes";
        const string C6 = "Capítulo 6 · Docker Compose";
        const string C7 = "Capítulo 7 · Limpeza & Deploy";

        public static List<Level> All()
        {
            var l = new List<Level>();

            // ───────────────────────── CAPÍTULO 1 ─────────────────────────
            l.Add(L(C1, "Checando a instalação",
                "É seu primeiro dia como estagiário(a) de DevOps na Micorosoft, cuidando do Café 365, o app de cafeterias da empresa. Antes de qualquer coisa, a líder técnica pede que você confirme se o Docker está instalado nesta máquina.",
                "Mostre a versão do Docker de forma resumida, em uma única linha.",
                A("docker --version", "docker -v"),
                A("Quase toda ferramenta de linha de comando aceita uma flag que imprime a versão.",
                  "É uma flag longa (dois traços) logo depois de 'docker'. Em inglês: version."),
                A("docker", "O cliente (CLI) do Docker. Ele envia seus pedidos para o Docker Engine (o daemon 'dockerd'), que é quem realmente cria e gerencia containers.",
                  "--version  (atalho: -v)", "Imprime a versão do cliente Docker em uma única linha e encerra."),
                "Para ver detalhes do cliente E do servidor (Engine, API, containerd), use 'docker version' (sem traços). Para um panorama do ambiente, use 'docker info'.",
                "Docker version 27.3.1, build ce12230"));

            l.Add(L(C1, "Raio-X do sistema",
                "Versão confirmada! Agora a líder quer um panorama geral: quantos containers e imagens existem, qual storage driver está em uso, quantas CPUs e quanta memória o Docker enxerga.",
                "Exiba as informações gerais do sistema Docker.",
                A("docker info", "docker system info"),
                A("O comando é uma palavra curta em inglês que significa 'informação'.",
                  "docker i___"),
                A("docker info", "Consulta o daemon e mostra um resumo do ambiente: nº de containers (rodando, pausados, parados), nº de imagens, versão do servidor, storage driver (ex.: overlay2), CPUs, memória total, registries configurados etc."),
                "Ótimo para diagnóstico: se 'docker info' mostrar erro de conexão com o daemon, o Docker Engine não está rodando (no Windows/Mac, abra o Docker Desktop).",
@"Client:
 Version:    27.3.1
 Context:    default
Server:
 Containers: 0
  Running: 0
  Paused: 0
  Stopped: 0
 Images: 0
 Server Version: 27.3.1
 Storage Driver: overlay2
 CPUs: 8
 Total Memory: 15.5GiB"));

            l.Add(L(C1, "Sua primeira imagem",
                "A Micorosoft vai usar o servidor web nginx no site institucional. Antes de rodar qualquer coisa, você quer apenas baixar a imagem para a máquina.",
                "Baixe a imagem oficial 'nginx' do Docker Hub (tag padrão), sem executar nenhum container.",
                A("docker pull nginx"),
                A("O verbo em inglês para 'puxar' algo de um repositório remoto.",
                  "docker p___ nginx"),
                A("docker pull", "Baixa uma imagem de um registry (por padrão, o Docker Hub) para o cache local de imagens.",
                  "nginx", "Nome da imagem. Sem tag explícita o Docker usa ':latest', ou seja, equivale a 'nginx:latest'."),
                "Imagens são formadas por camadas (layers); o pull só baixa as camadas que você ainda não tem. Para uma versão específica: docker pull nginx:1.27-alpine.",
@"Using default tag: latest
latest: Pulling from library/nginx
a2318d6c47ec: Pull complete
095d327c79ae: Pull complete
bbfaa25db775: Pull complete
Digest: sha256:4c0fdaa8b6341bfdeca5f18f7837462c80cff90527ee35ef185571e1c327beac
Status: Downloaded newer image for nginx:latest
docker.io/library/nginx:latest"));

            l.Add(L(C1, "Inventário de imagens",
                "Você quer conferir se o download deu certo e quanto espaço a imagem ocupa.",
                "Liste todas as imagens armazenadas localmente.",
                A("docker images", "docker image ls"),
                A("O comando é o plural da palavra 'imagem' em inglês.",
                  "Também existe a forma 'docker image ls'."),
                A("docker images  (ou docker image ls)", "Lista as imagens locais com REPOSITORY, TAG, IMAGE ID, data de criação e tamanho."),
                "'docker images -q' lista apenas os IDs (útil em scripts) e 'docker images -a' inclui camadas intermediárias.",
@"REPOSITORY   TAG       IMAGE ID       CREATED       SIZE
nginx        latest    39286ab8a5e1   2 weeks ago   188MB"));

            l.Add(L(C1, "Hello, World!",
                "Tradição é tradição: todo mundo começa com um Hello World. O Docker tem uma imagem oficial só para testar se tudo funciona de ponta a ponta.",
                "Execute um container a partir da imagem 'hello-world'.",
                A("docker run hello-world"),
                A("O comando cria E inicia um container a partir de uma imagem. Em inglês: 'executar'.",
                  "docker r__ hello-world"),
                A("docker run", "Cria um novo container a partir de uma imagem e o inicia. Se a imagem não existir localmente, faz o pull automaticamente.",
                  "hello-world", "Imagem minúscula que só imprime uma mensagem de boas-vindas e encerra."),
                "'docker run' = 'docker create' + 'docker start' (+ anexar ao terminal). Quando o processo principal termina, o container PARA — mas continua existindo até ser removido.",
@"Unable to find image 'hello-world:latest' locally
latest: Pulling from library/hello-world
c1ec31eb5944: Pull complete
Status: Downloaded newer image for hello-world:latest

Hello from Docker!
This message shows that your installation appears to be working correctly."));

            l.Add(L(C1, "Quem está rodando?",
                "O hello-world imprimiu a mensagem. Mas será que ele ainda está em execução?",
                "Liste os containers que estão em execução neste momento.",
                A("docker ps", "docker container ls"),
                A("O nome vem do comando Unix que lista processos (process status).",
                  "docker p_"),
                A("docker ps  (ou docker container ls)", "Lista os containers EM EXECUÇÃO, com CONTAINER ID, IMAGE, COMMAND, CREATED, STATUS, PORTS e NAMES."),
                "A lista veio vazia! O hello-world terminou seu trabalho e parou: um container só fica 'Up' enquanto o processo principal estiver rodando.",
                "CONTAINER ID   IMAGE     COMMAND   CREATED   STATUS    PORTS     NAMES"));

            l.Add(L(C1, "Containers fantasmas",
                "A lista veio vazia... mas o container do hello-world não sumiu: ele apenas parou.",
                "Liste TODOS os containers, inclusive os que estão parados.",
                A("docker ps -a", "docker container ls -a"),
                A("Use o comando da fase anterior com uma flag a mais.",
                  "A flag significa 'all' (todos)."),
                A("docker ps", "Lista containers.",
                  "-a  (--all)", "Inclui containers parados (Exited), criados e não iniciados (Created) etc."),
                "Containers parados continuam ocupando espaço em disco. Dica: 'docker ps -aq' lista só os IDs de todos os containers.",
@"CONTAINER ID   IMAGE         COMMAND    CREATED         STATUS                     PORTS     NAMES
3f2a9c1b7d4e   hello-world   ""/hello""   2 minutes ago   Exited (0) 2 minutes ago             eager_turing"));

            // ───────────────────────── CAPÍTULO 2 ─────────────────────────
            l.Add(L(C2, "Rodando em segundo plano",
                "Hora de subir o nginx de verdade! Só que, rodando em primeiro plano, o terminal fica preso aos logs do servidor.",
                "Rode um container da imagem 'nginx' em segundo plano (detached), com o nome 'web'.",
                A("docker run -d --name web nginx"),
                A("Você precisa de uma flag para 'detached' e outra para dar nome ao container.",
                  "-d  e  --name <nome>"),
                A("docker run", "Cria e inicia um container.",
                  "-d  (--detach)", "Executa em segundo plano e apenas imprime o ID do container, liberando o terminal.",
                  "--name web", "Define um nome legível. Sem ele, o Docker inventa um (ex.: 'eager_turing'). Nomes são únicos por host.",
                  "nginx", "Imagem usada como base."),
                "Com o nome definido você pode usar 'web' em vez do ID em todos os outros comandos (stop, logs, exec...).",
                "a4c1e8f90b2d7c3e6f1a9b8d2c4e7f0a1b3c5d7e9f2a4b6c8d0e1f3a5b7c9d1e"));

            l.Add(L(C2, "Abrindo as portas",
                "O nginx está rodando, mas ninguém consegue acessá-lo: a porta 80 está isolada dentro do container. O site precisa responder em http://localhost:8080.",
                "Rode em segundo plano um novo container 'nginx' chamado 'site', publicando a porta 8080 do host na porta 80 do container.",
                A("docker run -d -p 8080:80 --name site nginx"),
                A("A flag de 'publish' recebe o formato PORTA_HOST:PORTA_CONTAINER.",
                  "-p 8080:80"),
                A("docker run -d", "Cria e inicia em segundo plano.",
                  "-p 8080:80  (--publish)", "Mapeia a porta 8080 do host para a porta 80 do container. Formato: HOST:CONTAINER.",
                  "--name site", "Nome do container.",
                  "nginx", "Imagem."),
                "Agora http://localhost:8080 abre a página do nginx. Use 'docker port site' para ver os mapeamentos. Já '-P' (maiúsculo) publica todas as portas EXPOSE em portas aleatórias do host.",
@"7b2e9d4f1c3a5e8b0d6f2a4c9e1b7d3f5a8c0e2b4d6f8a1c3e5b7d9f0a2c4e6b

CONTAINER ID   IMAGE   STATUS         PORTS                  NAMES
7b2e9d4f1c3a   nginx   Up 2 seconds   0.0.0.0:8080->80/tcp   site"));

            l.Add(L(C2, "Modo interativo",
                "Um colega quer explorar um sistema Ubuntu limpo para testar uns comandos, sem instalar nada na máquina dele.",
                "Rode um container 'ubuntu' de forma interativa e com terminal (TTY), executando o shell 'bash'.",
                A("docker run -it ubuntu bash", "docker run -it ubuntu /bin/bash"),
                A("São duas flags de uma letra, quase sempre usadas juntas: interativo + terminal.",
                  "-it, seguido da imagem e do comando a executar."),
                A("-i  (--interactive)", "Mantém a entrada padrão (STDIN) aberta para você digitar.",
                  "-t  (--tty)", "Aloca um pseudo-terminal: prompt, cores e edição de linha.",
                  "ubuntu", "Imagem.",
                  "bash", "Comando executado dentro do container, substituindo o CMD padrão da imagem."),
                "Digite 'exit' (ou Ctrl+D) para sair — o container para, porque o bash era o processo principal. Ctrl+P seguido de Ctrl+Q desanexa sem parar.",
                "root@4e8b1c2d9a7f:/# "));

            l.Add(L(C2, "Container descartável",
                "Você só quer executar um comando rápido e não deixar lixo para trás: nada de containers parados se acumulando.",
                "Execute o comando 'echo ola' em um container 'alpine' que seja removido automaticamente quando terminar.",
                A("docker run --rm alpine echo ola"),
                A("Existe uma flag do run que remove o container ao sair. O nome é a abreviação de 'remove'.",
                  "--rm"),
                A("--rm", "Remove o container automaticamente assim que ele termina.",
                  "alpine", "Imagem Linux minúscula (~8 MB), ótima para testes.",
                  "echo ola", "Comando executado no container. Tudo que vem após o nome da imagem é repassado para dentro do container."),
                "Padrão muito usado para ferramentas pontuais, por exemplo: docker run --rm -v ${PWD}:/app -w /app node npm install.",
                "ola"));

            l.Add(L(C2, "Variáveis de ambiente",
                "O sistema de pedidos precisa de um MySQL. A imagem oficial exige que a senha do root seja informada por variável de ambiente — sem ela, o container nem sobe.",
                "Rode em segundo plano um container 'mysql' chamado 'banco', definindo a variável MYSQL_ROOT_PASSWORD com o valor 'segredo'.",
                A("docker run -d --name banco -e MYSQL_ROOT_PASSWORD=segredo mysql"),
                A("A flag é a inicial de 'environment'.",
                  "-e CHAVE=VALOR"),
                A("docker run -d --name banco", "Cria e inicia em segundo plano com o nome 'banco'.",
                  "-e MYSQL_ROOT_PASSWORD=segredo  (--env)", "Define uma variável de ambiente dentro do container. Pode repetir -e quantas vezes precisar.",
                  "mysql", "Imagem oficial do MySQL."),
                "Para muitas variáveis use '--env-file arquivo.env'. Atenção: variáveis aparecem no 'docker inspect' — em produção prefira mecanismos de secrets.",
                "e91c3a7b5d2f8e4a6c0b1d9f3e7a5c2b8d4f6a0e1c3b5d7f9a2c4e6b8d0f1a3c"));

            l.Add(L(C2, "Sempre de pé",
                "O servidor reiniciou de madrugada e o site ficou fora do ar até alguém subir os containers manualmente. Isso não pode se repetir.",
                "Rode em segundo plano um container 'nginx' chamado 'vigia' com a política de reinício 'always'.",
                A("docker run -d --name vigia --restart always nginx"),
                A("O nome da flag descreve o que acontece quando o container cai: re...",
                  "--restart <política>"),
                A("--restart always", "Política de reinício: o Docker reinicia o container sempre que ele parar e também quando o daemon iniciar (ex.: após um reboot).",
                  "--name vigia / -d / nginx", "Nome, segundo plano e imagem, como você já conhece."),
                "Políticas: 'no' (padrão), 'on-failure[:N]' (só se sair com erro), 'always' e 'unless-stopped' (como always, mas respeita um 'docker stop' manual).",
                "c2a8e4f6b0d1c3e5a7b9d2f4e6a8c0b1d3f5e7a9c2b4d6f8e0a1c3b5d7f9e2a4"));

            l.Add(L(C2, "Dieta de recursos",
                "Um container sem limites pode consumir toda a memória do servidor. A equipe definiu um teto para containers de teste.",
                "Rode em segundo plano um container 'nginx' chamado 'leve', limitado a 256m de memória e 0.5 CPU.",
                A("docker run -d --name leve --memory 256m --cpus 0.5 nginx"),
                A("Uma flag para memória e outra para a quantidade de CPUs.",
                  "--memory 256m  e  --cpus 0.5"),
                A("--memory 256m  (-m)", "Limite máximo de RAM. Se ultrapassar, o processo é encerrado pelo OOM killer do kernel.",
                  "--cpus 0.5", "Limita o container a meio núcleo de CPU (50% de um core).",
                  "--name leve / -d / nginx", "Nome, segundo plano e imagem."),
                "Confira com 'docker stats leve'. Os limites podem ser alterados depois, sem recriar o container: docker update --memory 512m leve.",
                "f0e1d2c3b4a5968778695a4b3c2d1e0f1a2b3c4d5e6f708192a3b4c5d6e7f809"));

            // ───────────────────────── CAPÍTULO 3 ─────────────────────────
            l.Add(L(C3, "Pausa para o café",
                "Manutenção programada: o container 'web' precisa ser desligado de forma graciosa, sem matar o processo abruptamente.",
                "Pare o container 'web'.",
                A("docker stop web"),
                A("O verbo em inglês para 'parar'.",
                  "docker s___ web"),
                A("docker stop", "Envia SIGTERM ao processo principal e aguarda (10 s por padrão). Se ele não encerrar, envia SIGKILL.",
                  "web", "Nome (ou ID) do container."),
                "'docker kill' envia SIGKILL imediatamente, sem chance de limpeza. Use 'docker stop -t 30 web' para esperar mais tempo antes de forçar.",
                "web"));

            l.Add(L(C3, "De volta ao trabalho",
                "Manutenção concluída. O container 'web' precisa voltar a funcionar — e você NÃO quer criar um container novo.",
                "Inicie novamente o container 'web', que está parado.",
                A("docker start web"),
                A("O oposto de 'stop'.",
                  "docker s____ web"),
                A("docker start", "Inicia um container EXISTENTE que está parado, mantendo nome, portas, variáveis e sistema de arquivos.",
                  "web", "Container alvo."),
                "Diferença crucial: 'run' cria um container NOVO a partir da imagem; 'start' reaproveita um que já existe. 'docker start -a web' anexa à saída.",
                "web"));

            l.Add(L(C3, "Reinício rápido",
                "Você alterou um arquivo de configuração e o nginx do container 'site' precisa recarregar.",
                "Reinicie o container 'site' usando um único comando.",
                A("docker restart site"),
                A("É um 'stop' seguido de 'start' em um único comando.",
                  "docker re_____ site"),
                A("docker restart", "Para (stop) e inicia (start) o container novamente.",
                  "site", "Container alvo."),
                "Também aceita '-t' para definir quanto tempo esperar pelo encerramento gracioso antes do SIGKILL.",
                "site"));

            l.Add(L(C3, "Lendo os logs",
                "Um cliente reclamou de erro 404 no site. Você quer ver o que o nginx registrou.",
                "Exiba os logs do container 'web'.",
                A("docker logs web"),
                A("O plural de 'log' em inglês.",
                  "docker l___ web"),
                A("docker logs", "Mostra tudo que o processo principal escreveu em STDOUT e STDERR.",
                  "web", "Container alvo."),
                "Por isso imagens bem feitas mandam logs para a saída padrão em vez de arquivos: assim 'docker logs' e as ferramentas de monitoramento conseguem lê-los.",
@"/docker-entrypoint.sh: Configuration complete; ready for start up
172.17.0.1 - - [02/Oct/2026:12:01:44 +0000] ""GET / HTTP/1.1"" 200 615
172.17.0.1 - - [02/Oct/2026:12:01:52 +0000] ""GET /cardapio HTTP/1.1"" 404 153"));

            l.Add(L(C3, "Logs ao vivo",
                "Você quer observar as requisições chegando enquanto testa o site no navegador.",
                "Acompanhe os logs do container 'web' em tempo real (continuando a exibir as novas linhas).",
                A("docker logs -f web"),
                A("Funciona como o 'tail -f' do Linux.",
                  "A flag é -f (--follow)."),
                A("docker logs", "Mostra os logs.",
                  "-f  (--follow)", "Continua conectado e exibe as novas linhas conforme aparecem.",
                  "web", "Container alvo."),
                "Ctrl+C para de seguir (o container continua rodando). Combine com '--since 10m' ou '--timestamps'.",
@"172.17.0.1 - - [02/Oct/2026:12:03:10 +0000] ""GET / HTTP/1.1"" 200 615
172.17.0.1 - - [02/Oct/2026:12:03:11 +0000] ""GET /favicon.ico HTTP/1.1"" 404 153
█"));

            l.Add(L(C3, "Só o finalzinho",
                "O container 'site' está no ar há semanas e tem milhares de linhas de log. Você só quer ver o que aconteceu por último.",
                "Mostre apenas as últimas 20 linhas de log do container 'site'.",
                A("docker logs --tail 20 site", "docker logs -n 20 site"),
                A("O nome da flag é 'cauda' em inglês.",
                  "--tail <número>"),
                A("docker logs", "Mostra os logs.",
                  "--tail 20  (-n 20)", "Limita a saída às últimas 20 linhas.",
                  "site", "Container alvo."),
                "Muito útil combinado: docker logs -f --tail 50 site (mostra as 50 últimas e continua acompanhando).",
                "...(últimas 20 linhas do log)..."));

            l.Add(L(C3, "Entrando no container",
                "Você precisa investigar os arquivos de configuração do nginx no container 'web', que está rodando, abrindo um shell lá dentro.",
                "Abra um shell 'bash' interativo (com TTY) dentro do container 'web' em execução.",
                A("docker exec -it web bash", "docker exec -it web /bin/bash"),
                A("O comando 'executa' algo dentro de um container que já está rodando.",
                  "docker exec + as mesmas flags do modo interativo + container + shell"),
                A("docker exec", "Executa um NOVO processo dentro de um container que já está em execução.",
                  "-it", "Interativo + terminal, igual ao run.",
                  "web", "Container alvo.",
                  "bash", "Processo a executar lá dentro."),
                "Ao digitar 'exit', só o bash termina — o container continua rodando, pois o processo principal (nginx) não foi afetado. Imagens alpine geralmente têm apenas 'sh'.",
                "root@a4c1e8f90b2d:/# "));

            l.Add(L(C3, "Comando relâmpago",
                "Você só quer conferir rapidamente quais arquivos o nginx está servindo, sem abrir um shell.",
                "Sem modo interativo, liste (ls) o conteúdo do diretório /usr/share/nginx/html dentro do container 'web'.",
                A("docker exec web ls /usr/share/nginx/html"),
                A("Mesmo comando da fase anterior, mas sem -it e com outro comando no final.",
                  "docker exec <container> ls <caminho>"),
                A("docker exec", "Executa um processo no container em execução.",
                  "web", "Container alvo.",
                  "ls /usr/share/nginx/html", "Comando e argumento executados lá dentro; a saída volta para o seu terminal."),
                "Sem -it o comando roda, devolve a saída e termina. Ótimo para scripts e automações.",
                "50x.html\nindex.html"));

            l.Add(L(C3, "Raio-X do container",
                "Você precisa descobrir o IP interno, as variáveis de ambiente e os volumes do container 'web'.",
                "Exiba todos os detalhes de baixo nível (em JSON) do container 'web'.",
                A("docker inspect web", "docker container inspect web"),
                A("O verbo em inglês para 'inspecionar'.",
                  "docker i______ web"),
                A("docker inspect", "Retorna um JSON com a configuração e o estado completos do objeto: rede, IP, mounts, env, comando, política de restart etc.",
                  "web", "Objeto inspecionado (também funciona com imagens, volumes e redes)."),
                "Filtre campos com Go templates: docker inspect -f '{{.NetworkSettings.IPAddress}}' web",
@"[
    {
        ""Id"": ""a4c1e8f90b2d..."",
        ""State"": { ""Status"": ""running"", ""Running"": true },
        ""Name"": ""/web"",
        ""NetworkSettings"": { ""IPAddress"": ""172.17.0.2"" }
    }
]"));

            l.Add(L(C3, "Monitor de recursos",
                "O servidor está lento. Você suspeita que algum container esteja consumindo recursos demais.",
                "Acompanhe em tempo real o uso de CPU, memória, rede e disco de todos os containers em execução.",
                A("docker stats", "docker container stats"),
                A("Abreviação de 'statistics'.",
                  "docker st___"),
                A("docker stats", "Painel ao vivo (como o 'top') com CPU %, uso/limite de memória, I/O de rede e de disco e nº de processos de cada container."),
                "Use 'docker stats --no-stream' para uma única leitura (bom para scripts) ou informe nomes para filtrar: docker stats web site.",
@"CONTAINER ID   NAME    CPU %   MEM USAGE / LIMIT     MEM %   NET I/O         PIDS
a4c1e8f90b2d   web     0.00%   7.2MiB / 15.5GiB      0.05%   1.2kB / 0B      9
c2a8e4f6b0d1   vigia   0.01%   7.1MiB / 15.5GiB      0.04%   900B / 0B       9
f0e1d2c3b4a5   leve    0.00%   6.9MiB / 256MiB       2.70%   700B / 0B       9"));

            l.Add(L(C3, "Processos internos",
                "Dentro do container 'web' parece haver processos demais. Você quer ver quais são, sem entrar nele.",
                "Liste os processos que estão rodando dentro do container 'web'.",
                A("docker top web", "docker container top web"),
                A("Tem o mesmo nome do clássico monitor de processos do Linux.",
                  "docker t__ web"),
                A("docker top", "Mostra os processos em execução dentro do container (PID, usuário, comando), vistos a partir do host.",
                  "web", "Container alvo."),
                "Repare que os PIDs mostrados são os do HOST: containers são apenas processos isolados por namespaces do kernel.",
@"UID    PID     PPID    CMD
root   21345   21320   nginx: master process nginx -g daemon off;
101    21401   21345   nginx: worker process"));

            l.Add(L(C3, "Copiando arquivos",
                "Você quer analisar com calma a configuração do nginx no seu editor favorito.",
                "Copie o arquivo /etc/nginx/nginx.conf do container 'web' para o diretório atual do host ( . ).",
                A("docker cp web:/etc/nginx/nginx.conf ."),
                A("Tem o mesmo nome do comando Unix de cópia.",
                  "docker cp <container>:<caminho> <destino>"),
                A("docker cp", "Copia arquivos/pastas entre o container e o host.",
                  "web:/etc/nginx/nginx.conf", "Origem, no formato CONTAINER:CAMINHO.",
                  ".", "Destino: diretório atual no host."),
                "Funciona nos dois sentidos (docker cp ./index.html web:/usr/share/nginx/html/) e até com o container parado.",
                "Successfully copied 2.56kB to /home/aluno/."));

            l.Add(L(C3, "Novo nome",
                "O marketing decidiu que o container 'site' agora deve se chamar 'loja'. Recriar está fora de questão.",
                "Renomeie o container 'site' para 'loja'.",
                A("docker rename site loja", "docker container rename site loja"),
                A("O verbo em inglês para 'renomear'.",
                  "docker rename <atual> <novo>"),
                A("docker rename", "Troca o nome de um container existente (rodando ou parado).",
                  "site loja", "Nome atual e novo nome."),
                "O ID do container não muda, só o nome.",
                ""));

            l.Add(L(C3, "Faxina pontual",
                "O container 'web' já foi parado e não será mais usado.",
                "Remova o container 'web'.",
                A("docker rm web", "docker container rm web"),
                A("Abreviação de 'remove', igual ao comando Unix.",
                  "docker r_ web"),
                A("docker rm", "Remove um ou mais containers PARADOS (o sistema de arquivos dele é apagado).",
                  "web", "Container a remover."),
                "Remover o container não remove a imagem. Para remover vários: docker rm web vigia leve.",
                "web"));

            l.Add(L(C3, "Remoção forçada",
                "O container 'loja' está rodando e precisa sumir agora.",
                "Remova o container 'loja' com um único comando, forçando a parada.",
                A("docker rm -f loja"),
                A("Mesmo comando da fase anterior, com uma flag de 'força'.",
                  "-f (--force)"),
                A("docker rm", "Remove containers.",
                  "-f  (--force)", "Mata o container em execução (SIGKILL) e o remove em seguida.",
                  "loja", "Container alvo."),
                "Use com cuidado: não há encerramento gracioso. Em produção, prefira 'docker stop' antes de 'docker rm'.",
                "loja"));

            // ───────────────────────── CAPÍTULO 4 ─────────────────────────
            l.Add(L(C4, "Construindo sua imagem",
                "O time de desenvolvimento entregou o app de pedidos com um Dockerfile na raiz do projeto. Seu terminal já está nessa pasta.",
                "Construa uma imagem a partir do Dockerfile do diretório atual, com o nome/tag 'cafeapp:1.0'.",
                A("docker build -t cafeapp:1.0 ."),
                A("O verbo em inglês para 'construir'. Não esqueça do contexto (a pasta atual) no final.",
                  "docker build -t <nome:tag> ."),
                A("docker build", "Lê o Dockerfile e executa suas instruções (FROM, COPY, RUN...) gerando uma imagem em camadas.",
                  "-t cafeapp:1.0  (--tag)", "Nome e tag da imagem gerada.",
                  ".", "Contexto de build: a pasta cujos arquivos são enviados ao builder. O Dockerfile é procurado aqui por padrão."),
                "Use um arquivo .dockerignore para não enviar node_modules, .git etc. Cada instrução gera uma camada em cache, por isso a ordem das instruções afeta a velocidade do build.",
@"[+] Building 12.4s (10/10) FINISHED
 => [1/5] FROM docker.io/library/node:20-alpine
 => [2/5] WORKDIR /app
 => [3/5] COPY package*.json ./
 => [4/5] RUN npm ci --omit=dev
 => [5/5] COPY . .
 => exporting to image
 => => naming to docker.io/library/cafeapp:1.0"));

            l.Add(L(C4, "Dockerfile alternativo",
                "Para desenvolvimento existe outro arquivo, chamado 'Dockerfile.dev', com ferramentas de debug.",
                "Construa uma imagem usando o arquivo 'Dockerfile.dev', com a tag 'cafeapp:dev', usando o diretório atual como contexto.",
                A("docker build -f Dockerfile.dev -t cafeapp:dev ."),
                A("Uma flag indica qual 'file' usar.",
                  "-f Dockerfile.dev -t cafeapp:dev ."),
                A("docker build", "Constrói uma imagem.",
                  "-f Dockerfile.dev  (--file)", "Caminho do Dockerfile a usar (por padrão: ./Dockerfile).",
                  "-t cafeapp:dev", "Nome:tag.",
                  ".", "Contexto de build."),
                "Alternativa moderna: um único Dockerfile com múltiplos estágios (multi-stage) e '--target dev'.",
                " => => naming to docker.io/library/cafeapp:dev"));

            l.Add(L(C4, "Build do zero",
                "Um pacote do sistema foi atualizado, mas o build continua usando as camadas antigas do cache.",
                "Construa a imagem 'cafeapp:1.1' a partir do diretório atual, ignorando completamente o cache.",
                A("docker build --no-cache -t cafeapp:1.1 ."),
                A("Existe uma flag que desliga o uso do cache.",
                  "--no-cache"),
                A("--no-cache", "Executa todas as instruções do zero, sem reaproveitar camadas.",
                  "-t cafeapp:1.1", "Nome:tag.",
                  ".", "Contexto de build."),
                "Builds sem cache são mais lentos, use só quando necessário. '--pull' também força baixar a versão mais nova da imagem base.",
                "[+] Building 58.1s (10/10) FINISHED"));

            l.Add(L(C4, "Etiquetando para o registry",
                "Para publicar no Docker Hub, a imagem precisa ter o seu usuário como prefixo. O usuário da empresa é 'micorosoft'.",
                "Crie a tag 'micorosoft/cafeapp:1.0' apontando para a imagem 'cafeapp:1.0'.",
                A("docker tag cafeapp:1.0 micorosoft/cafeapp:1.0", "docker image tag cafeapp:1.0 micorosoft/cafeapp:1.0"),
                A("O comando é a palavra em inglês para 'etiqueta'.",
                  "docker tag <origem> <nova-tag>"),
                A("docker tag", "Cria um novo nome (referência) para uma imagem existente.",
                  "cafeapp:1.0", "Imagem de origem.",
                  "micorosoft/cafeapp:1.0", "Novo nome no formato usuário/repositório:tag."),
                "A tag não copia nada: é só outro nome para o mesmo IMAGE ID. Para outro registry inclua o host: ghcr.io/micorosoft/cafeapp:1.0.",
                ""));

            l.Add(L(C4, "Login no registry",
                "Antes de enviar a imagem, você precisa se autenticar no Docker Hub.",
                "Faça login no registry padrão (Docker Hub).",
                A("docker login"),
                A("O comando é a palavra usada em qualquer sistema para entrar com usuário e senha.",
                  "docker l____"),
                A("docker login", "Pede usuário e senha/token e guarda a credencial (no gerenciador de credenciais do sistema ou em ~/.docker/config.json)."),
                "Para outro registry: docker login ghcr.io. Em pipelines de CI use '--password-stdin' para não expor a senha no histórico.",
@"Username: micorosoft
Password:
Login Succeeded"));

            l.Add(L(C4, "Publicando",
                "Login feito! Hora de enviar a imagem para o mundo.",
                "Envie a imagem 'micorosoft/cafeapp:1.0' para o registry.",
                A("docker push micorosoft/cafeapp:1.0"),
                A("O oposto de 'pull'.",
                  "docker p___ <imagem>"),
                A("docker push", "Envia as camadas da imagem para o registry indicado no nome.",
                  "micorosoft/cafeapp:1.0", "Imagem a publicar (o prefixo define o destino)."),
                "Camadas que o registry já possui não são reenviadas ('Layer already exists').",
@"The push refers to repository [docker.io/micorosoft/cafeapp]
5f70bf18a086: Pushed
a3b9c1d2e4f5: Layer already exists
1.0: digest: sha256:9e1f... size: 1789"));

            l.Add(L(C4, "Arqueologia de camadas",
                "A imagem ficou com 900 MB! Você quer descobrir qual instrução do Dockerfile gerou a camada mais pesada.",
                "Mostre o histórico de camadas da imagem 'cafeapp:1.0'.",
                A("docker history cafeapp:1.0", "docker image history cafeapp:1.0"),
                A("A palavra em inglês para 'histórico'.",
                  "docker h______ <imagem>"),
                A("docker history", "Lista as camadas da imagem, a instrução que criou cada uma e o tamanho de cada camada.",
                  "cafeapp:1.0", "Imagem analisada."),
                "Dicas para emagrecer imagens: bases menores (alpine/slim), multi-stage build e juntar comandos RUN limpando caches na mesma camada.",
@"IMAGE          CREATED BY                         SIZE
3c9e7d1a2b4f   COPY . .                           612MB
<missing>      RUN npm ci --omit=dev              201MB
<missing>      COPY package*.json ./              1.2kB
<missing>      WORKDIR /app                       0B"));

            l.Add(L(C4, "Snapshot de container",
                "Um colega instalou ferramentas manualmente dentro do container 'prototipo' e quer guardar esse estado como imagem.",
                "Crie a imagem 'cafeapp:prototipo' a partir do estado atual do container 'prototipo'.",
                A("docker commit prototipo cafeapp:prototipo", "docker container commit prototipo cafeapp:prototipo"),
                A("O mesmo verbo usado no Git para salvar um estado.",
                  "docker commit <container> <imagem:tag>"),
                A("docker commit", "Gera uma nova imagem a partir das alterações feitas no sistema de arquivos de um container.",
                  "prototipo", "Container de origem.",
                  "cafeapp:prototipo", "Nome:tag da nova imagem."),
                "Útil para experimentos, mas em projetos reais prefira Dockerfile: ele é reprodutível e versionável.",
                "sha256:7d1c9e3a5b2f4d6e8a0c1b3d5f7e9a2c4b6d8f0e1a3c5b7d9f2e4a6c8b0d1f3e"));

            l.Add(L(C4, "Imagem no pendrive",
                "O servidor da filial não tem acesso à internet. Você vai levar a imagem em um arquivo.",
                "Salve a imagem 'cafeapp:1.0' em um arquivo chamado 'cafeapp.tar'.",
                A("docker save -o cafeapp.tar cafeapp:1.0", "docker save cafeapp:1.0 > cafeapp.tar"),
                A("O verbo em inglês para 'salvar'. Há uma flag para o arquivo de saída (output).",
                  "docker save -o <arquivo.tar> <imagem>"),
                A("docker save", "Exporta uma ou mais imagens (todas as camadas e metadados) para um arquivo tar.",
                  "-o cafeapp.tar  (--output)", "Arquivo de destino.",
                  "cafeapp:1.0", "Imagem a exportar."),
                "Não confunda com 'docker export', que exporta só o sistema de arquivos de um CONTAINER, sem camadas nem metadados.",
                ""));

            l.Add(L(C4, "Do pendrive para o servidor",
                "Chegando na filial, você precisa importar a imagem a partir do arquivo.",
                "Carregue a imagem a partir do arquivo 'cafeapp.tar'.",
                A("docker load -i cafeapp.tar", "docker load < cafeapp.tar"),
                A("O oposto de 'save'. Há uma flag para o arquivo de entrada (input).",
                  "docker load -i <arquivo>"),
                A("docker load", "Importa imagens de um arquivo tar gerado pelo 'docker save'.",
                  "-i cafeapp.tar  (--input)", "Arquivo de entrada."),
                "A imagem volta com o mesmo nome, tags e camadas. O par save/load é a forma de transportar imagens sem registry.",
                "Loaded image: cafeapp:1.0"));

            l.Add(L(C4, "Removendo imagem",
                "A imagem de desenvolvimento não é mais necessária e está ocupando espaço.",
                "Remova a imagem 'cafeapp:dev'.",
                A("docker rmi cafeapp:dev", "docker image rm cafeapp:dev"),
                A("É o 'rm' com um 'i' de image no final.",
                  "docker rmi <imagem>"),
                A("docker rmi  (ou docker image rm)", "Remove uma imagem local (ou só a tag, se outras tags apontarem para o mesmo ID).",
                  "cafeapp:dev", "Imagem a remover."),
                "Se algum container (mesmo parado) usar a imagem, a remoção falha. Remova o container antes ou use -f.",
@"Untagged: cafeapp:dev
Deleted: sha256:8b2f..."));

            // ───────────────────────── CAPÍTULO 5 ─────────────────────────
            l.Add(L(C5, "Criando um volume",
                "Dados gravados dentro do container somem quando ele é removido. O PostgreSQL precisa de armazenamento persistente gerenciado pelo Docker.",
                "Crie um volume chamado 'dados'.",
                A("docker volume create dados"),
                A("Os comandos de volume ficam no grupo 'docker volume'.",
                  "docker volume c_____ <nome>"),
                A("docker volume create", "Cria um volume nomeado, gerenciado pelo Docker (no Linux fica em /var/lib/docker/volumes/).",
                  "dados", "Nome do volume."),
                "Volumes sobrevivem à remoção dos containers e podem ser compartilhados entre vários deles.",
                "dados"));

            l.Add(L(C5, "Listando volumes",
                "Você quer confirmar que o volume foi criado e ver quais outros existem.",
                "Liste todos os volumes.",
                A("docker volume ls"),
                A("Mesmo padrão dos outros grupos: 'docker <grupo> ls'.",
                  "docker volume l_"),
                A("docker volume ls", "Lista os volumes com driver e nome."),
                "Volumes 'anônimos' aparecem com nomes enormes (hashes): são criados automaticamente quando a imagem declara VOLUME e você não informa um nome.",
@"DRIVER    VOLUME NAME
local     dados"));

            l.Add(L(C5, "Banco persistente",
                "Agora sim: o PostgreSQL vai guardar os dados no volume. O diretório de dados do Postgres é /var/lib/postgresql/data.",
                "Rode em segundo plano um container 'postgres' chamado 'db', com a variável POSTGRES_PASSWORD=senha e o volume 'dados' montado em /var/lib/postgresql/data.",
                A("docker run -d --name db -e POSTGRES_PASSWORD=senha -v dados:/var/lib/postgresql/data postgres"),
                A("Junte o que você já sabe (-d, --name, -e) com a flag de volume.",
                  "-v <volume>:<caminho-no-container>"),
                A("-d --name db", "Segundo plano, nome 'db'.",
                  "-e POSTGRES_PASSWORD=senha", "Senha do superusuário, exigida pela imagem.",
                  "-v dados:/var/lib/postgresql/data  (--volume)", "Monta o volume 'dados' no diretório de dados do Postgres. Formato: ORIGEM:DESTINO[:ro].",
                  "postgres", "Imagem oficial."),
                "Agora você pode remover e recriar o container 'db' à vontade: os dados continuam no volume. A sintaxe '--mount type=volume,src=dados,dst=...' é a forma mais explícita.",
                "b7d9f1e3a5c2b4d6f8e0a1c3b5d7f9e2a4c6b8d0f1e3a5c7b9d2f4e6a8c0b1d3"));

            l.Add(L(C5, "Edição ao vivo (bind mount)",
                "A designer edita o HTML do site em /home/aluno/site e quer ver as mudanças na hora, sem rebuild de imagem.",
                "Rode em segundo plano um container 'nginx' chamado 'vitrine', montando a pasta do host /home/aluno/site em /usr/share/nginx/html.",
                A("docker run -d --name vitrine -v /home/aluno/site:/usr/share/nginx/html nginx"),
                A("É a mesma flag de volume, mas a origem é um caminho absoluto do host.",
                  "-v /home/aluno/site:/usr/share/nginx/html"),
                A("-v /home/aluno/site:/usr/share/nginx/html", "Bind mount: espelha uma pasta do host dentro do container. Alterações aparecem nos dois lados instantaneamente.",
                  "--name vitrine / -d / nginx", "Nome, segundo plano e imagem."),
                "Quando a origem começa com '/' (ou '.'), é bind mount; quando é só um nome, é volume. Acrescente ':ro' para montar como somente leitura.",
                "d3f5a7c9e1b2d4f6a8c0e2b4d6f8a1c3e5b7d9f0a2c4e6b8d1f3a5c7e9b0d2f4"));

            l.Add(L(C5, "Criando uma rede",
                "A API, o banco e o cache precisam conversar entre si pelo nome, isolados dos outros containers.",
                "Crie uma rede chamada 'cafe-net'.",
                A("docker network create cafe-net"),
                A("Os comandos de rede ficam no grupo 'docker network'.",
                  "docker network create <nome>"),
                A("docker network create", "Cria uma rede (driver 'bridge' por padrão).",
                  "cafe-net", "Nome da rede."),
                "Em redes criadas pelo usuário, o Docker oferece DNS interno: um container encontra o outro pelo nome (ex.: 'db:5432').",
                "4f6a8c0e2b4d6f8a1c3e5b7d9f0a2c4e6b8d1f3a5c7e9b0d2f4a6c8e0b1d3f5a"));

            l.Add(L(C5, "Listando redes",
                "Você quer ver a rede nova e as redes padrão que o Docker cria.",
                "Liste todas as redes.",
                A("docker network ls"),
                A("Mesmo padrão: 'docker <grupo> ls'.",
                  "docker network l_"),
                A("docker network ls", "Lista as redes com ID, nome, driver e escopo."),
                "As redes padrão são: bridge (default), host (usa a rede do host direto) e none (sem rede).",
@"NETWORK ID     NAME       DRIVER    SCOPE
1a2b3c4d5e6f   bridge     bridge    local
4f6a8c0e2b4d   cafe-net   bridge    local
7e8f9a0b1c2d   host       host      local
3d4e5f6a7b8c   none       null      local"));

            l.Add(L(C5, "Cache na rede",
                "O cache Redis já deve nascer dentro da rede da aplicação.",
                "Rode em segundo plano um container 'redis' chamado 'cache', conectado à rede 'cafe-net'.",
                A("docker run -d --name cache --network cafe-net redis"),
                A("Existe uma flag do run para escolher a rede.",
                  "--network <rede>"),
                A("--network cafe-net  (--net)", "Conecta o container à rede indicada em vez da bridge padrão.",
                  "--name cache", "Nome — e também o hostname pelo qual os outros containers da rede vão achá-lo.",
                  "redis", "Imagem do Redis."),
                "Outros containers da cafe-net podem acessar o Redis em 'cache:6379' sem publicar nenhuma porta no host.",
                "9c1e3a5b7d2f4e6a8c0b1d3f5e7a9c2b4d6f8e0a1c3b5d7f9e2a4c6b8d0f1a3c"));

            l.Add(L(C5, "Conectando o banco",
                "O container 'db' foi criado antes da rede existir e ficou de fora.",
                "Conecte o container 'db', que já existe, à rede 'cafe-net'.",
                A("docker network connect cafe-net db"),
                A("Subcomando do grupo network que 'conecta'.",
                  "docker network connect <rede> <container>"),
                A("docker network connect", "Liga um container existente (rodando ou não) a mais uma rede.",
                  "cafe-net", "Rede.",
                  "db", "Container."),
                "Um container pode estar em várias redes ao mesmo tempo. O inverso é 'docker network disconnect cafe-net db'.",
                ""));

            l.Add(L(C5, "Raio-X da rede",
                "Você quer confirmar quais containers estão na rede e quais IPs receberam.",
                "Exiba os detalhes da rede 'cafe-net'.",
                A("docker network inspect cafe-net"),
                A("Mesmo verbo do raio-X de containers, mas dentro do grupo network.",
                  "docker network inspect <rede>"),
                A("docker network inspect", "Mostra em JSON a configuração da rede: sub-rede, gateway, driver e os containers conectados com seus IPs.",
                  "cafe-net", "Rede alvo."),
                "Útil para depurar problemas de comunicação entre containers.",
@"[
    {
        ""Name"": ""cafe-net"",
        ""Driver"": ""bridge"",
        ""IPAM"": { ""Config"": [ { ""Subnet"": ""172.18.0.0/16"" } ] },
        ""Containers"": {
            ""9c1e..."": { ""Name"": ""cache"", ""IPv4Address"": ""172.18.0.2/16"" },
            ""b7d9..."": { ""Name"": ""db"",    ""IPv4Address"": ""172.18.0.3/16"" }
        }
    }
]"));

            // ───────────────────────── CAPÍTULO 6 ─────────────────────────
            l.Add(L(C6, "Subindo a stack",
                "Chega de subir container por container! A stack completa (api, db, cache) foi descrita no arquivo compose.yaml da pasta atual.",
                "Suba todos os serviços definidos no Compose em segundo plano.",
                A("docker compose up -d"),
                A("Os comandos do Compose ficam em 'docker compose'. A ação é 'subir'.",
                  "docker compose up + flag de segundo plano"),
                A("docker compose", "Gerencia aplicações multi-container descritas em YAML (compose.yaml / docker-compose.yml).",
                  "up", "Cria redes, volumes e containers e inicia todos os serviços (fazendo build/pull se necessário).",
                  "-d  (--detach)", "Em segundo plano."),
                "O Compose v2 é um plugin: 'docker compose' (com espaço). A versão antiga era o binário separado 'docker-compose' (com hífen).",
@"[+] Running 4/4
 ✔ Network cafe_default    Created
 ✔ Container cafe-db-1     Started
 ✔ Container cafe-cache-1  Started
 ✔ Container cafe-api-1    Started"));

            l.Add(L(C6, "Status da stack",
                "Você quer ver se todos os serviços da stack subiram corretamente.",
                "Liste os containers da stack do Compose e seus status.",
                A("docker compose ps"),
                A("É o mesmo nome do comando que lista containers, só que dentro do Compose.",
                  "docker compose p_"),
                A("docker compose ps", "Lista apenas os containers do projeto Compose atual, com serviço, status e portas."),
                "O nome do projeto vem, por padrão, do nome da pasta. Mude com '-p <nome>' ou 'name:' no YAML.",
@"NAME           SERVICE   STATUS          PORTS
cafe-api-1     api       Up 30 seconds   0.0.0.0:3000->3000/tcp
cafe-cache-1   cache     Up 31 seconds   6379/tcp
cafe-db-1      db        Up 31 seconds   5432/tcp"));

            l.Add(L(C6, "Logs de um serviço",
                "A API está retornando erro 500. Você quer acompanhar só os logs dela, ao vivo.",
                "Acompanhe em tempo real os logs apenas do serviço 'api' do Compose.",
                A("docker compose logs -f api"),
                A("Igual ao 'docker logs -f', mas no Compose e informando o serviço.",
                  "docker compose logs -f <serviço>"),
                A("docker compose logs", "Mostra os logs dos serviços (de todos, se nenhum for informado), com prefixo colorido por serviço.",
                  "-f  (--follow)", "Continua acompanhando.",
                  "api", "Nome do SERVIÇO (como está no YAML), não do container."),
                "Você pode passar vários serviços: docker compose logs -f api db.",
                "api-1  | Error: connect ECONNREFUSED db:5432"));

            l.Add(L(C6, "Rebuild e sobe",
                "O time corrigiu o bug no código da API. A imagem precisa ser reconstruída antes de subir de novo.",
                "Suba a stack em segundo plano forçando o build das imagens antes.",
                A("docker compose up -d --build"),
                A("O mesmo 'up -d', com uma flag a mais que tem o nome da ação de construir.",
                  "--build"),
                A("docker compose up -d", "Sobe a stack em segundo plano.",
                  "--build", "Reconstrói as imagens dos serviços que têm 'build:' no YAML antes de iniciar."),
                "Sem --build, o Compose reutiliza a imagem antiga se ela já existir. Apenas os containers cuja configuração ou imagem mudou são recriados.",
@"[+] Building 8.2s (10/10) FINISHED
[+] Running 3/3
 ✔ Container cafe-db-1     Running
 ✔ Container cafe-cache-1  Running
 ✔ Container cafe-api-1    Started"));

            l.Add(L(C6, "Consultando o banco",
                "Você quer abrir o cliente do PostgreSQL dentro do serviço 'db' para checar as tabelas.",
                "Execute 'psql -U postgres' dentro do serviço 'db' do Compose.",
                A("docker compose exec db psql -U postgres"),
                A("Igual ao 'docker exec', mas no Compose usando o nome do serviço.",
                  "docker compose exec <serviço> <comando...>"),
                A("docker compose exec", "Executa um comando em um serviço em execução. Já aloca TTY por padrão (não precisa de -it).",
                  "db", "Nome do serviço.",
                  "psql -U postgres", "Comando executado: o cliente psql conectando como usuário 'postgres'."),
                "Para rodar um comando em um container NOVO e temporário do serviço, use 'docker compose run --rm db ...'.",
@"psql (16.4)
Type ""help"" for help.

postgres=#"));

            l.Add(L(C6, "Escalando a API",
                "Promoção de café em dobro! O tráfego explodiu e uma única instância da API não dá conta.",
                "Suba a stack em segundo plano com 3 réplicas do serviço 'api'.",
                A("docker compose up -d --scale api=3"),
                A("Existe uma flag do 'up' para 'escalar' um serviço: SERVIÇO=QUANTIDADE.",
                  "--scale api=3"),
                A("docker compose up -d", "Sobe a stack em segundo plano.",
                  "--scale api=3", "Define o número de containers do serviço 'api'."),
                "Para escalar, o serviço não pode publicar uma porta fixa no host (haveria conflito) nem ter container_name fixo. Normalmente se coloca um proxy/balanceador na frente.",
@"[+] Running 5/5
 ✔ Container cafe-api-1  Running
 ✔ Container cafe-api-2  Started
 ✔ Container cafe-api-3  Started"));

            l.Add(L(C6, "Arquivo de produção",
                "A produção usa um arquivo Compose diferente, chamado 'docker-compose.prod.yml'.",
                "Suba em segundo plano a stack definida no arquivo 'docker-compose.prod.yml'.",
                A("docker compose -f docker-compose.prod.yml up -d"),
                A("Uma flag do 'docker compose' (antes do subcomando) escolhe o arquivo.",
                  "docker compose -f <arquivo> up -d"),
                A("docker compose", "Plugin Compose.",
                  "-f docker-compose.prod.yml  (--file)", "Arquivo de configuração a usar. Pode ser repetido para mesclar arquivos.",
                  "up -d", "Sobe em segundo plano."),
                "Padrão comum: docker compose -f compose.yaml -f compose.prod.yaml up -d (o segundo arquivo sobrescreve partes do primeiro).",
                "[+] Running 4/4"));

            l.Add(L(C6, "Derrubando tudo",
                "Fim dos testes. Você quer apagar a stack inteira, INCLUINDO os volumes com os dados de teste.",
                "Pare e remova os containers e redes do Compose, removendo também os volumes.",
                A("docker compose down -v"),
                A("O oposto de 'up', com a flag de volumes.",
                  "docker compose down + -v"),
                A("docker compose down", "Para e remove containers e redes criados pelo 'up'.",
                  "-v  (--volumes)", "Remove também os volumes nomeados declarados no YAML e os anônimos."),
                "Sem -v os volumes são preservados (comportamento seguro). '--rmi all' também remove as imagens.",
@"[+] Running 5/5
 ✔ Container cafe-api-1    Removed
 ✔ Container cafe-cache-1  Removed
 ✔ Container cafe-db-1     Removed
 ✔ Volume cafe_pgdata      Removed
 ✔ Network cafe_default    Removed"));

            // ───────────────────────── CAPÍTULO 7 ─────────────────────────
            l.Add(L(C7, "Onde foi parar meu disco?",
                "Alerta: o disco do servidor está 92% cheio. Você suspeita do Docker.",
                "Mostre quanto espaço em disco o Docker está usando (imagens, containers, volumes e cache de build).",
                A("docker system df"),
                A("Está no grupo 'system'. O nome vem do comando Unix 'disk free'.",
                  "docker system d_"),
                A("docker system df", "Resumo do uso de disco por tipo de objeto, incluindo quanto é recuperável (RECLAIMABLE)."),
                "Use '-v' para ver o detalhamento de cada imagem, container e volume.",
@"TYPE            TOTAL   ACTIVE   SIZE      RECLAIMABLE
Images          14      4        6.2GB     4.9GB (79%)
Containers      9       4        120MB     98MB (81%)
Local Volumes   6       2        2.1GB     1.4GB (66%)
Build Cache     52      0        3.3GB     3.3GB"));

            l.Add(L(C7, "Containers parados",
                "Há dezenas de containers parados acumulados ao longo dos testes.",
                "Remova de uma vez todos os containers parados.",
                A("docker container prune"),
                A("'prune' significa 'podar'. Ele fica dentro do grupo do tipo de objeto.",
                  "docker container p____"),
                A("docker container prune", "Remove TODOS os containers parados. Pede confirmação (pule com -f)."),
                "Também aceita filtros, por exemplo: docker container prune --filter \"until=24h\".",
@"WARNING! This will remove all stopped containers.
Are you sure you want to continue? [y/N] y
Deleted Containers:
3f2a9c1b7d4e...
Total reclaimed space: 98MB", "-f"));

            l.Add(L(C7, "Imagens órfãs",
                "Cada rebuild deixou para trás imagens antigas sem nome, que aparecem como '<none>'.",
                "Remova as imagens 'dangling' (sem tag).",
                A("docker image prune"),
                A("Mesmo verbo da fase anterior, no grupo de imagens.",
                  "docker image p____"),
                A("docker image prune", "Remove imagens 'dangling' (sem tag e sem uso). Com -a remove TODAS as imagens não usadas por containers."),
                "Imagens dangling surgem quando você reconstrói uma tag: a nova imagem 'rouba' a tag e a antiga fica '<none>:<none>'.",
@"WARNING! This will remove all dangling images.
Are you sure you want to continue? [y/N] y
Total reclaimed space: 1.3GB", "-f"));

            l.Add(L(C7, "Volumes esquecidos",
                "Volumes de testes antigos continuam ocupando gigabytes.",
                "Remova os volumes que não estão sendo usados por nenhum container.",
                A("docker volume prune"),
                A("Mesmo verbo, no grupo de volumes.",
                  "docker volume p____"),
                A("docker volume prune", "Remove volumes locais não usados por nenhum container. Nas versões recentes, por padrão só os anônimos — use -a para incluir os nomeados."),
                "Cuidado: dados de volume removidos não voltam. Faça backup antes em produção.",
                "Total reclaimed space: 1.4GB", "-f"));

            l.Add(L(C7, "Faxina geral",
                "Fim de sprint. Você quer liberar o máximo possível de espaço de uma vez só.",
                "Remova tudo que não está em uso: containers parados, redes sem uso, cache de build e TODAS as imagens não usadas (não apenas as dangling).",
                A("docker system prune -a"),
                A("O 'prune' do grupo 'system' limpa vários tipos de uma vez. Uma flag inclui todas as imagens.",
                  "docker system prune + -a"),
                A("docker system prune", "Remove containers parados, redes não usadas, imagens dangling e cache de build.",
                  "-a  (--all)", "Inclui TODAS as imagens sem container associado, não só as dangling."),
                "Volumes NÃO são removidos, a menos que você adicione '--volumes'. Depois disso, o próximo 'run' terá que baixar as imagens novamente.",
@"WARNING! This will remove:
  - all stopped containers
  - all networks not used by at least one container
  - all images without at least one container associated to them
  - all build cache
Are you sure you want to continue? [y/N] y
Total reclaimed space: 8.7GB", "-f"));

            l.Add(L(C7, "MISSÃO FINAL: deploy em produção",
                "É o grande dia! A imagem oficial está no Docker Hub e a rede de produção 'cafe-net' está pronta. Junte tudo que você aprendeu em um único comando.",
                "Rode em segundo plano a imagem 'micorosoft/cafeapp:1.0' com o nome 'cafe-prod', na rede 'cafe-net', publicando a porta 80 do host na 3000 do container, com a variável NODE_ENV=production e política de reinício 'always'.",
                A("docker run -d --name cafe-prod --network cafe-net -p 80:3000 -e NODE_ENV=production --restart always micorosoft/cafeapp:1.0"),
                A("São 6 partes além de 'docker run' e da imagem: -d, --name, --network, -p, -e e --restart.",
                  "A ordem das opções não importa, mas todas devem vir ANTES do nome da imagem."),
                A("-d", "Segundo plano.",
                  "--name cafe-prod", "Nome do container.",
                  "--network cafe-net", "Rede onde estão o banco e o cache (acessíveis por 'db' e 'cache').",
                  "-p 80:3000", "Porta 80 do host -> porta 3000 do app.",
                  "-e NODE_ENV=production", "Variável de ambiente para o Node rodar em modo produção.",
                  "--restart always", "Volta sozinho após falhas ou reboot do servidor.",
                  "micorosoft/cafeapp:1.0", "Imagem publicada no Docker Hub (baixada automaticamente se necessário)."),
                "Parabéns! Você saiu do 'docker --version' até um deploy completo. Próximos passos: healthchecks, Docker Swarm/Kubernetes, CI/CD e segurança de imagens (docker scout).",
@"Unable to find image 'micorosoft/cafeapp:1.0' locally
1.0: Pulling from micorosoft/cafeapp
Status: Downloaded newer image for micorosoft/cafeapp:1.0
e4a6c8b0d1f3e5a7c9b2d4f6e8a0c1b3d5f7e9a2c4b6d8f0e1a3c5b7d9f2e4a6

CONTAINER ID   IMAGE                   STATUS         PORTS                  NAMES
e4a6c8b0d1f3   micorosoft/cafeapp:1.0   Up 3 seconds   0.0.0.0:80->3000/tcp   cafe-prod"));

            return l;
        }
    }
}
