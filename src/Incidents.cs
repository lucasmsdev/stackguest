using System.Collections.Generic;

namespace DockerQuest
{
    // Desafio relâmpago: um "incidente" com tempo limitado que vale mais pontos.
    class Incident
    {
        public string Id, Title, Reporter, Alert;
        public int Chapter;          // (legado) capítulo do Docker usado para definir o nível
        public Level Level;
        public string Cat = "docker";
        public int Room = 1, Tier, LevelIndex = -1;
    }

    static class Incidents
    {
        public const int BasePoints = 150, TimeBonus = 100, WrongCost = 20, MinPoints = 50;
        public const float BaseTime = 120f;

        static Incident I(string id, int ch, string reporter, string title, string alert, string obj, string[] ans, string[] explain, string summary, string output, params string[] ignore)
        {
            return new Incident
            {
                Id = id, Chapter = ch, Reporter = reporter, Title = title, Alert = alert,
                Level = new Level
                {
                    Chapter = "Incidente", Title = title, Context = alert, Objective = obj, Answers = ans, Hints = new string[0],
                    Explain = explain, Summary = summary, Output = output, Ignore = ignore
                }
            };
        }

        static string[] A(params string[] s) { return s; }

        static Incidents()
        {
            foreach (var i in All)
            {
                i.Tier = i.Chapter <= 1 ? 0 : i.Chapter <= 3 ? 1 : 2;
                i.Level.Cat = "docker";
                i.Level.Tier = i.Tier;
                i.Reporter = new[] { "Rafael", "Lia", "Jorge" }[All.IndexOf(i) % 3];
            }
        }

        public static readonly List<Incident> All = new List<Incident>
        {
            // ── conhecimento do capítulo 1 (Recepção)
            I("painel", 0, "Marina", "O painel de monitoramento caiu",
                "SOCORRO! O painel de monitoramento saiu do ar e ninguém sabe o que está rodando!",
                "Liste rapidamente os containers que estão em execução agora.",
                A("docker ps", "docker container ls"),
                A("docker ps", "Lista os containers em execução — a forma mais rápida de saber o que está no ar."),
                "Em incidentes, 'docker ps' costuma ser o primeiro comando: confirma o que está rodando antes de qualquer ação.",
                "CONTAINER ID   IMAGE   STATUS          NAMES\n3a1f0c9d2e4b   nginx   Up 3 hours      site\n9e8d7c6b5a4f   redis   Up 3 hours      cache"),
            I("redis_pull", 0, "Paulo", "Servidor novo sem imagem",
                "O servidor novo chegou e não tem a imagem do Redis! O deploy está esperando!",
                "Baixe a imagem 'redis' (tag padrão) do Docker Hub.",
                A("docker pull redis"),
                A("docker pull redis", "Baixa a imagem do Redis para o cache local, sem criar container."),
                "Fazer o pull antes do deploy evita esperar o download na hora de subir o container.",
                "Using default tag: latest\nlatest: Pulling from library/redis\nStatus: Downloaded newer image for redis:latest"),

            // ── capítulo 2 (Laboratório)
            I("site_caiu", 1, "Rafael", "O site institucional caiu!",
                "O container do site foi apagado sem querer e o site está fora do ar!",
                "Suba de novo, em segundo plano, um container 'nginx' chamado 'site', publicando a porta 80 do host na porta 80 do container.",
                A("docker run -d --name site -p 80:80 nginx"),
                A("docker run -d", "Cria e inicia em segundo plano.", "--name site", "Nome do container.", "-p 80:80", "Porta 80 do host → 80 do container.", "nginx", "Imagem do servidor web."),
                "Com a política --restart always esse container voltaria sozinho depois de um reboot.",
                "c4f1a9e2b7d3..."),
            I("cache_caiu", 1, "Jorge", "O cache sumiu!",
                "O Redis de cache não está rodando e a API ficou lentíssima!",
                "Rode em segundo plano um container 'redis' chamado 'cache'.",
                A("docker run -d --name cache redis"),
                A("docker run -d --name cache", "Segundo plano, com o nome 'cache'.", "redis", "Imagem do Redis."),
                "Sem cache, todas as consultas vão direto ao banco. Por isso o Redis é tão importante!",
                "8b2e6f0a1c9d..."),

            // ── capítulo 3 (NOC)
            I("api_parou", 2, "Carla", "O container 'api' parou de funcionar!",
                "ALERTA VERMELHO! O container 'api' parou e os pedidos não estão chegando!",
                "Inicie novamente o container 'api', que está parado.",
                A("docker start api", "docker container start api"),
                A("docker start api", "Liga de novo o container existente 'api', mantendo a configuração dele."),
                "Depois de religar, olhe 'docker logs api' para descobrir por que ele parou.",
                "api"),
            I("pagamentos", 2, "Dani", "Pagamentos travados",
                "O container 'pagamentos' travou e os clientes não conseguem pagar!",
                "Reinicie o container 'pagamentos'.",
                A("docker restart pagamentos", "docker container restart pagamentos"),
                A("docker restart pagamentos", "Para e inicia o container de uma vez."),
                "Restart é o 'desliga e liga' dos containers. Resolve o sintoma — depois investigue a causa.",
                "pagamentos"),
            I("erro500", 2, "Otávio", "Erro 500 em produção",
                "Os clientes estão vendo erro 500! Precisamos ver o que a API está registrando!",
                "Mostre apenas as últimas 50 linhas de log do container 'api'.",
                A("docker logs --tail 50 api", "docker logs -n 50 api"),
                A("docker logs", "Mostra os logs.", "--tail 50", "Só as últimas 50 linhas.", "api", "Container investigado."),
                "Em produção os logs são enormes: --tail e --since economizam tempo.",
                "Error: connect ECONNREFUSED db:5432"),
            I("minerador", 2, "Carla", "Invasor no servidor!",
                "Tem um container estranho chamado 'minerador' usando 100% da CPU! Alguém invadiu!",
                "Remova à força o container 'minerador', mesmo ele estando em execução.",
                A("docker rm -f minerador", "docker container rm -f minerador"),
                A("docker rm -f", "Mata e remove o container de uma vez.", "minerador", "Container suspeito."),
                "Depois de um incidente de segurança: troque senhas, revise imagens e quem tem acesso ao Docker.",
                "minerador"),
            I("lentidao", 2, "Otávio", "Servidor lento",
                "Tudo está lento! Qual container está comendo CPU e memória?",
                "Acompanhe em tempo real o uso de CPU e memória de todos os containers.",
                A("docker stats", "docker container stats"),
                A("docker stats", "Painel ao vivo de CPU, memória, rede e disco por container."),
                "Achou o culpado? Limite recursos com --memory e --cpus.",
                "NAME     CPU %    MEM USAGE\napi      187.4%   1.9GiB / 2GiB"),
            I("teste_carga", 2, "Dani", "Teste de carga fora de controle",
                "O container 'teste-carga' está derrubando o banco de dados!",
                "Pare o container 'teste-carga'.",
                A("docker stop teste-carga", "docker container stop teste-carga"),
                A("docker stop teste-carga", "Encerra o container de forma graciosa."),
                "Testes de carga nunca devem apontar para o banco de produção!",
                "teste-carga"),

            // ── capítulo 4 (Fábrica de Imagens)
            I("hotfix", 3, "Thiago", "Hotfix urgente!",
                "Achamos o bug! O código já foi corrigido, falta gerar a imagem nova!",
                "Construa a imagem 'cafeapp:1.0.1' a partir do Dockerfile do diretório atual.",
                A("docker build -t cafeapp:1.0.1 ."),
                A("docker build -t cafeapp:1.0.1 .", "Gera a imagem com a tag da versão corrigida usando a pasta atual como contexto."),
                "Versione sempre os hotfixes (1.0.1, 1.0.2...) para poder voltar atrás se precisar.",
                " => => naming to docker.io/library/cafeapp:1.0.1"),
            I("push_hotfix", 3, "Nina", "As filiais precisam da correção",
                "As filiais estão esperando a versão corrigida no registry!",
                "Envie a imagem 'micorosoft/cafeapp:1.0.1' para o registry.",
                A("docker push micorosoft/cafeapp:1.0.1"),
                A("docker push", "Envia a imagem.", "micorosoft/cafeapp:1.0.1", "Imagem com o namespace da empresa."),
                "Assim cada filial só precisa de um 'docker pull' para atualizar.",
                "1.0.1: digest: sha256:7c1e... size: 1789"),

            // ── capítulo 5 (Data Center)
            I("relatorios", 4, "Sandra", "Relatórios sem banco",
                "O container 'relatorios' não consegue falar com o banco! Ele ficou fora da rede!",
                "Conecte o container 'relatorios' à rede 'cafe-net'.",
                A("docker network connect cafe-net relatorios"),
                A("docker network connect", "Liga um container existente a uma rede.", "cafe-net relatorios", "Rede e container, nessa ordem."),
                "Na mesma rede, o container acha o banco pelo nome 'db'.",
                ""),
            I("volume_pedidos", 4, "Clara", "Banco novo sem volume",
                "Vamos subir um banco novo de pedidos e ele PRECISA de armazenamento persistente!",
                "Crie um volume chamado 'dados-pedidos'.",
                A("docker volume create dados-pedidos"),
                A("docker volume create dados-pedidos", "Cria o volume nomeado."),
                "Sem volume, os pedidos sumiriam quando o container fosse recriado.",
                "dados-pedidos"),

            // ── capítulo 6 (Orquestração)
            I("queda_energia", 5, "Igor", "Queda de energia!",
                "Caiu a energia e a stack inteira está desligada! Tudo fora do ar!",
                "Suba todos os serviços do Compose em segundo plano.",
                A("docker compose up -d", "docker-compose up -d"),
                A("docker compose up -d", "Sobe todos os serviços do compose.yaml em segundo plano."),
                "Com 'restart: always' no YAML, os serviços voltariam sozinhos depois da queda.",
                "[+] Running 4/4"),
            I("worker_erro", 5, "Rui", "Worker com erro",
                "O serviço 'worker' do Compose está falhando sem parar!",
                "Acompanhe ao vivo os logs do serviço 'worker' do Compose.",
                A("docker compose logs -f worker"),
                A("docker compose logs -f worker", "Logs do serviço 'worker', acompanhando ao vivo."),
                "Use o nome do SERVIÇO do YAML, não o nome do container.",
                "worker-1  | Error: queue not found"),
            I("black_friday", 5, "Fê", "Black Friday!",
                "BLACK FRIDAY! O tráfego triplicou e a API não está aguentando!",
                "Suba a stack em segundo plano com 5 réplicas do serviço 'api'.",
                A("docker compose up -d --scale api=5"),
                A("docker compose up -d", "Sobe a stack.", "--scale api=5", "Cinco réplicas do serviço 'api'."),
                "Depois do pico, volte para menos réplicas para economizar recursos.",
                "[+] Running 7/7"),

            // ── capítulo 7 (Produção)
            I("disco_cheio", 6, "Ana", "Disco 98% cheio!",
                "O disco do servidor está quase cheio! Se encher, tudo para!",
                "Remova todos os containers parados.",
                A("docker container prune"),
                A("docker container prune", "Apaga todos os containers parados de uma vez."),
                "Agende limpezas periódicas para não chegar nesse ponto.",
                "Total reclaimed space: 2.1GB", "-f"),
            I("espaco", 6, "Helena", "Quem está comendo o disco?",
                "O alerta de disco disparou! Precisamos saber o que o Docker está usando!",
                "Mostre o uso de disco do Docker.",
                A("docker system df"),
                A("docker system df", "Resumo de espaço por imagens, containers, volumes e cache."),
                "Olhe a coluna RECLAIMABLE: é o que dá para recuperar com prune.",
                "TYPE    TOTAL   SIZE    RECLAIMABLE\nImages  31      14GB    11GB (78%)"),
            I("dangling", 6, "Vítor", "Imagens órfãs",
                "Os builds deixaram dezenas de imagens <none> lotando o disco!",
                "Remova as imagens dangling (sem tag).",
                A("docker image prune"),
                A("docker image prune", "Remove as imagens sem tag e sem uso."),
                "Imagens dangling surgem quando você reconstrói uma tag que já existia.",
                "Total reclaimed space: 3.4GB", "-f"),
        };

        public static readonly string[] Panic =
        {
            "O site caiu?!", "Socorro!", "Alguém chama o DevOps!", "Os clientes estão reclamando!", "Tá tudo vermelho no painel!",
            "Não fui eu, juro!", "Cadê o plantão?", "{nome}, corre!", "Meu café esfriou de nervoso...", "Abre um chamado!",
            "Já tentaram desligar e ligar?", "O telefone não para!"
        };

        public static readonly string[] Celebrate =
        {
            "Ufa!", "Valeu, {nome}!", "Que rapidez!", "Herói(na) do plantão!", "Voltou tudo!", "Isso aí!", "Salvou o dia!", "Bora tomar um café!"
        };

        public static readonly string[] Sad = { "Ah não...", "Dessa vez não deu.", "O plantão resolveu.", "Fica pra próxima!" };
    }
}
