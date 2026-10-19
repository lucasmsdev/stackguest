using System.Collections.Generic;

namespace DockerQuest
{
    // Documentação das outras áreas da Micorosoft (Frontend, React, Flutter, AWS, Redes, Python e PHP).
    // Como na parte de Docker, os exemplos usam outros nomes: o aluno precisa adaptar para o desafio.
    static partial class Docs
    {
        static Docs() { All.AddRange(More()); DocsMaisDocker(All); DocsMaisRedes(All); DocsMaisPython(All); DocsMaisPhp(All); DocsMaisFront(All); DocsMaisReact(All); DocsMaisFlutter(All); DocsMaisAws(All); DocsMaisSql(All);
            DocsExtraDocker(All); DocsExtraRedes(All); DocsExtraPython(All); DocsExtraPhp(All); DocsExtraFront(All); DocsExtraReact(All); DocsExtraFlutter(All); DocsExtraAws(All); DocsExtraSql(All); }
        static partial void DocsExtraDocker(List<DocEntry> all);
        static partial void DocsExtraRedes(List<DocEntry> all);
        static partial void DocsExtraPython(List<DocEntry> all);
        static partial void DocsExtraPhp(List<DocEntry> all);
        static partial void DocsExtraFront(List<DocEntry> all);
        static partial void DocsExtraReact(List<DocEntry> all);
        static partial void DocsExtraFlutter(List<DocEntry> all);
        static partial void DocsExtraAws(List<DocEntry> all);
        static partial void DocsExtraSql(List<DocEntry> all);


        static partial void DocsMaisDocker(List<DocEntry> all);
        static partial void DocsMaisRedes(List<DocEntry> all);
        static partial void DocsMaisPython(List<DocEntry> all);
        static partial void DocsMaisPhp(List<DocEntry> all);
        static partial void DocsMaisFront(List<DocEntry> all);
        static partial void DocsMaisReact(List<DocEntry> all);
        static partial void DocsMaisFlutter(List<DocEntry> all);
        static partial void DocsMaisAws(List<DocEntry> all);
        static partial void DocsMaisSql(List<DocEntry> all);

        static DocEntry X(string area, string sub, string cmd, string summary, string usage, string[] opts, string[] ex, string notes = null)
        {
            var d = D(sub, cmd, summary, usage, opts, ex, notes);
            d.Area = area;
            return d;
        }

        static IEnumerable<DocEntry> More()
        {
            // ───────────────────────── FRONTEND ─────────────────────────
            const string F = "HTML/CSS/JS";
            yield return X(F, "HTML", "<h1> … <h6>  (títulos)", "Títulos da página, do mais importante (h1) ao menos importante (h6).",
                "<h1>Texto do título</h1>\n<h2>Subtítulo</h2>", null,
                A("<h1>Padaria Pão Quente</h1>", "título principal da página", "<h3>Promoções</h3>", "título de terceiro nível"),
                "Use um único h1 por página e não pule níveis (h1 → h2 → h3).");
            yield return X(F, "HTML", "<a>  (links)", "Cria um link (âncora). O destino vai no atributo href.",
                "<a href=\"ENDEREÇO\">texto clicável</a>",
                A("href", "endereço de destino", "target=\"_blank\"", "abre em nova aba"),
                A("<a href=\"https://exemplo.com/contato\">Fale conosco</a>", "link com o texto Fale conosco"), null);
            yield return X(F, "HTML", "<img>  (imagens)", "Insere uma imagem. Não tem tag de fechamento.",
                "<img src=\"ARQUIVO\" alt=\"DESCRIÇÃO\">",
                A("src", "caminho/URL da imagem", "alt", "texto alternativo, lido por leitores de tela"),
                A("<img src=\"gato.jpg\" alt=\"Gato dormindo\">", "imagem com descrição acessível"),
                "Toda imagem deve ter alt. Imagens só decorativas usam alt=\"\".");
            yield return X(F, "HTML", "<input>  (campos de formulário)", "Campo de entrada. O tipo muda o comportamento e a validação.",
                "<input type=\"TIPO\" [required] [placeholder=\"...\"]>",
                A("type=\"text\" | \"email\" | \"number\" | \"password\"", "tipo do campo", "required", "atributo sem valor: campo obrigatório", "placeholder", "texto de exemplo"),
                A("<input type=\"number\" required>", "número obrigatório", "<input type=\"password\">", "senha (caracteres escondidos)"),
                "A validação do navegador ajuda, mas o servidor sempre precisa validar de novo.");
            yield return X(F, "CSS", "Sintaxe de uma regra CSS", "Uma regra tem seletor e um bloco de declarações propriedade: valor;",
                "seletor { propriedade: valor; }",
                A("p", "seletor de tag: todos os <p>", ".classe", "seletor de classe (ponto)", "#id", "seletor de id (cerquilha)",
                  "color", "cor do texto", "text-align", "alinhamento do texto (left, center, right)", "background", "fundo"),
                A("h2 { color: red; }", "todos os h2 vermelhos", ".aviso { text-align: right; }", "texto da classe aviso à direita"), null);
            yield return X(F, "CSS", "display: flex  (flexbox)", "Transforma o elemento num container flexível: os filhos ficam lado a lado.",
                ".container { display: flex; }",
                A("justify-content", "distribui no eixo principal (center, space-between...)", "align-items", "alinha no eixo cruzado", "gap", "espaço entre os itens"),
                A(".rodape { display: flex; gap: 8px; }", "itens do rodapé em linha"), null);
            yield return X(F, "CSS", "display: grid  (grade)", "Layout em linhas e colunas.",
                ".container { display: grid; grid-template-columns: COLUNAS; }",
                A("grid-template-columns", "define as colunas", "repeat(N, 1fr)", "N colunas iguais (fr = fração do espaço)", "gap", "espaço entre células"),
                A(".fotos { display: grid; grid-template-columns: repeat(4, 1fr); }", "quatro colunas iguais",
                  ".painel { display: grid; grid-template-columns: 200px 1fr; }", "barra fixa + conteúdo"), null);
            yield return X(F, "CSS", "@media  (responsivo)", "Aplica regras só quando a tela atende a uma condição.",
                "@media (max-width: LARGURA) { regras }",
                A("max-width", "até essa largura (telas menores)", "min-width", "a partir dessa largura (telas maiores)"),
                A("@media (max-width: 800px) { .lateral { display: none; } }", "esconde a lateral em telas de até 800px"),
                "display: none esconde o elemento.");
            yield return X(F, "JavaScript", "console.log", "Escreve uma mensagem no console do navegador (F12 → Console).",
                "console.log(valor)", null,
                A("console.log(\"Teste ok\")", "mostra o texto", "console.log(1 + 2)", "mostra 3"), "Textos vão entre aspas.");
            yield return X(F, "JavaScript", "const / let  (variáveis)", "const declara um valor que não será reatribuído; let, um que pode mudar.",
                "const nome = valor\nlet contador = 0", null,
                A("const taxa = 0.15", "constante decimal (use ponto)", "let tentativas = 3", "variável que vai mudar"),
                "Evite var em código moderno.");
            yield return X(F, "JavaScript", "document.getElementById / querySelector", "Encontra um elemento do HTML para manipular com JavaScript.",
                "document.getElementById(\"id\")\ndocument.querySelector(\"seletor CSS\")", null,
                A("const menu = document.getElementById(\"menu\")", "pega o elemento id=menu", "const btn = document.querySelector(\"#enviar\")", "mesmo efeito, com seletor CSS"), null);
            yield return X(F, "JavaScript", "addEventListener  (eventos)", "Executa uma função quando um evento acontece (clique, tecla, envio...).",
                "elemento.addEventListener(\"evento\", funcao)",
                A("\"click\"", "clique", "\"submit\"", "envio de formulário", "\"keydown\"", "tecla pressionada"),
                A("link.addEventListener(\"click\", abrirMenu)", "chama abrirMenu no clique"),
                "Passe a função SEM parênteses: com () ela seria executada na hora.");
            yield return X(F, "JavaScript", "fetch  (requisições HTTP)", "Busca dados de uma URL. Devolve uma Promise com a resposta.",
                "fetch(url).then(r => r.json())", null,
                A("fetch(\"/api/clientes\").then(res => res.json())", "busca e converte para JSON",
                  "const r = await fetch(\"/api/clientes\")", "mesma coisa com await"), null);
            yield return X(F, "JavaScript", "Arrow functions  (=>)", "Forma curta de escrever funções.",
                "const nome = (param) => expressão\nconst nome = param => expressão", null,
                A("const triplo = n => n * 3", "retorna n * 3", "const soma = (a, b) => a + b", "dois parâmetros precisam de parênteses"),
                "Com chaves { } o retorno precisa ser explícito: (n) => { return n * 3 }");

            // ───────────────────────── REACT ─────────────────────────
            const string R = "React";
            yield return X(R, "Projeto e npm", "npm install / npm i", "Instala as dependências listadas no package.json (pasta node_modules).",
                "npm install\nnpm install PACOTE", null,
                A("npm install", "instala tudo do package.json", "npm install axios", "adiciona um pacote novo"), null);
            yield return X(R, "Projeto e npm", "npm run <script>", "Executa um script definido em \"scripts\" no package.json.",
                "npm run NOME_DO_SCRIPT",
                A("dev", "servidor de desenvolvimento (Vite)", "build", "gera os arquivos otimizados de produção", "test", "roda os testes"),
                A("npm run lint", "roda o script lint"), "Em projetos Create React App o servidor é npm start.");
            yield return X(R, "JSX", "Expressões no JSX  { }", "Dentro do JSX, qualquer expressão JavaScript vai entre chaves.",
                "<tag>texto {expressao}</tag>", null,
                A("<p>Total: {quantidade}</p>", "mostra a variável", "<h1>{titulo.toUpperCase()}</h1>", "chama um método"), null);
            yield return X(R, "JSX", "className, onClick e outros atributos", "Alguns atributos mudam de nome no JSX.",
                "<div className=\"classe\"></div>\n<button onClick={funcao}>…</button>",
                A("className", "em vez de class", "htmlFor", "em vez de for", "onClick, onChange", "eventos em camelCase"),
                A("<span className=\"alerta\"></span>", "span com a classe alerta", "<button onClick={() => salvar()}>Salvar</button>", "arrow function no clique"), null);
            yield return X(R, "JSX", "Listas com .map e key", "Gera um elemento para cada item do array. Cada item precisa de uma key única.",
                "{lista.map(item => <li key={item.id}>{item.texto}</li>)}", null,
                A("{alunos.map(a => <li key={a.id}>{a.nome}</li>)}", "um <li> por aluno"), null);
            yield return X(R, "JSX", "Renderização condicional  (&& e ?:)", "Mostra algo só se uma condição for verdadeira.",
                "{condicao && <Componente />}\n{condicao ? <A /> : <B />}", null,
                A("{temErro && <Aviso />}", "Aviso só aparece se temErro for verdadeiro"), null);
            yield return X(R, "JSX", "Props", "Atributos passados para um componente. Texto vai entre aspas; números, variáveis e objetos entre chaves.",
                "<Componente prop=\"texto\" outra={valor} />", null,
                A("<Avatar tamanho={48} />", "prop numérica", "<Titulo texto=\"Bem-vindo\" />", "prop de texto"), null);
            yield return X(R, "Hooks", "import de hooks", "Hooks são importações nomeadas do pacote react.",
                "import { hook1, hook2 } from \"react\"", null,
                A("import { useEffect } from \"react\"", "importa um hook", "import { useState, useMemo } from \"react\"", "vários de uma vez"), null);
            yield return X(R, "Hooks", "useState", "Cria um estado no componente. Devolve o valor atual e a função que o altera.",
                "const [valor, setValor] = useState(inicial)", null,
                A("const [aberto, setAberto] = useState(false)", "estado booleano", "setAberto(true)", "atualiza e re-renderiza"),
                "Nunca altere o estado diretamente: use sempre a função set.");
            yield return X(R, "Hooks", "useEffect", "Executa efeitos colaterais (buscar dados, timers) depois da renderização.",
                "useEffect(() => { ... }, [dependências])",
                A("[]", "array vazio: roda uma vez, ao montar", "[x]", "roda de novo quando x mudar"),
                A("useEffect(() => { buscarClientes() }, [])", "busca ao abrir a tela"), "Sem o array, roda a cada renderização.");
            yield return X(R, "Hooks", "createContext / useContext", "Compartilha um valor com toda a árvore de componentes sem passar props.",
                "const MeuContext = createContext()\nconst valor = useContext(MeuContext)", null,
                A("const IdiomaContext = createContext()", "cria o contexto", "const idioma = useContext(IdiomaContext)", "lê o valor"), null);
            yield return X(R, "Hooks", "useMemo", "Memoriza o resultado de um cálculo caro e só recalcula quando as dependências mudam.",
                "const valor = useMemo(() => calculo(), [dependências])", null,
                A("const media = useMemo(() => calcularMedia(notas), [notas])", "recalcula só quando notas mudar"), null);

            // ───────────────────────── FLUTTER ─────────────────────────
            const string L = "Flutter";
            yield return X(L, "CLI do Flutter", "flutter create / run / build", "Comandos principais da ferramenta flutter.",
                "flutter create NOME\nflutter run\nflutter build apk [--release]\nflutter build appbundle",
                A("create", "gera um projeto novo (nome em minúsculas e _)", "run", "roda no emulador/dispositivo (r = hot reload)", "build apk", "gera o APK Android"),
                A("flutter create agenda_app", "cria o projeto agenda_app", "flutter build apk --release", "APK otimizado"), null);
            yield return X(L, "CLI do Flutter", "flutter pub get / pub add", "Gerencia pacotes do pubspec.yaml (pub.dev).",
                "flutter pub get\nflutter pub add PACOTE", null,
                A("flutter pub add provider", "adiciona o pacote provider", "flutter pub get", "baixa o que está no pubspec.yaml"), null);
            yield return X(L, "Dart", "final / const / var", "Declaração de variáveis em Dart.",
                "final nome = valor;\nconst PI = 3.14;\nvar x = 1;", null,
                A("final cidade = \"Recife\"", "atribuída uma vez", "final int idade = 20", "com tipo explícito"),
                "Textos podem usar aspas simples ou duplas.");
            yield return X(L, "Dart", "async / await / Future", "Código assíncrono: a função devolve um Future e pode esperar com await.",
                "Future<TIPO> nome() async { ... }\nfinal r = await algo();", null,
                A("Future<void> salvar() async {}", "função assíncrona sem retorno", "final dados = await buscar()", "espera o resultado"), null);
            yield return X(L, "Dart", "http.get e jsonDecode", "Requisições com o pacote http e conversão de JSON (dart:convert).",
                "final resp = await http.get(url);\nfinal mapa = jsonDecode(resp.body);", null,
                A("final r = await http.get(endereco)", "GET", "final json = jsonDecode(r.body)", "texto JSON → Map/List"), null);
            yield return X(L, "Widgets", "Text", "Exibe texto.", "Text(\"conteúdo\", style: TextStyle(...))", null,
                A("Text(\"Bem-vindo!\")", "texto simples"), null);
            yield return X(L, "Widgets", "ElevatedButton", "Botão com elevação. Parâmetros nomeados onPressed e child.",
                "ElevatedButton(onPressed: funcao, child: Widget)", null,
                A("ElevatedButton(onPressed: enviar, child: Text(\"Enviar\"))", "botão que chama enviar"), "onPressed: null deixa o botão desativado.");
            yield return X(L, "Widgets", "Column / Row", "Organizam widgets na vertical (Column) ou horizontal (Row).",
                "Column(children: [w1, w2])", null,
                A("Row(children: [icone, titulo])", "lado a lado"), null);
            yield return X(L, "Widgets", "Padding e EdgeInsets", "Espaçamento interno em volta de um widget filho.",
                "Padding(padding: EdgeInsets.all(N), child: widget)",
                A("EdgeInsets.all(8)", "todos os lados", "EdgeInsets.symmetric(horizontal: 12)", "só nas laterais"),
                A("Padding(padding: EdgeInsets.all(24), child: cartao)", "24 em volta do cartão"), null);
            yield return X(L, "Widgets", "setState", "Em um StatefulWidget, avisa o Flutter que o estado mudou e a tela precisa ser redesenhada.",
                "setState(() { mudança; })", null,
                A("setState(() { pontos += 10; })", "soma e redesenha"), null);
            yield return X(L, "Widgets", "Navigator.push / pop", "Abre uma nova tela (rota) por cima da atual, ou volta.",
                "Navigator.push(context, MaterialPageRoute(builder: (context) => Tela()))\nNavigator.pop(context)", null,
                A("Navigator.push(context, MaterialPageRoute(builder: (context) => PerfilPage()))", "abre a tela de perfil"), null);

            // ───────────────────────── AWS ─────────────────────────
            const string W = "AWS";
            yield return X(W, "Console de gerenciamento", "Navegar no console", "O Console da AWS é a interface web. No topo ficam o menu Serviços, a pesquisa, a REGIÃO (canto superior direito) e o menu da conta.",
                "Serviços ▾  ou  pesquisa → nome do serviço\nRegião: canto superior direito\nConta: menu com o seu nome (mostra o ID da conta)",
                A("Região", "Recursos como EC2 só aparecem na região onde foram criados.", "Menu da conta", "Mostra o ID da conta e o usuário logado."),
                A("Pesquisa: EC2", "abre o serviço EC2", "Região → Europa (Irlanda)", "troca a região do console"),
                "Antes de qualquer alteração, confira a conta e a região. 'Sumiu tudo' quase sempre é região errada.");
            yield return X(W, "Console de gerenciamento", "S3 no console", "No S3 você cria buckets e envia (carrega) arquivos.",
                "S3 → Criar bucket → nome + região + bloqueio de acesso público → Criar bucket\nS3 → clique no bucket → Carregar → escolha arquivos ou uma pasta → Carregar",
                A("Bloquear todo o acesso público", "Deixe marcado, a menos que tenha um motivo forte.", "Carregar", "Upload de arquivos ou de uma pasta inteira."),
                A("Criar bucket 'fotos-evento'", "bucket privado para fotos", "Carregar → convite.pdf", "envia um arquivo"), null);
            yield return X(W, "Console de gerenciamento", "EC2 no console", "Instâncias são servidores virtuais. O botão 'Estado da instância' interrompe, inicia, reinicializa ou encerra.",
                "EC2 → Instâncias → selecione a linha → Estado da instância → Interromper / Iniciar / Encerrar\nEC2 → Executar instâncias → nome, AMI, tipo, par de chaves → Executar",
                A("Interromper", "Desliga (dá para ligar de novo).", "Encerrar", "APAGA a instância.", "Tipo de instância", "Tamanho (CPU/memória) e preço por hora."),
                A("Executar 'site-teste', Ubuntu, t3.small", "cria um servidor pequeno"), null);
            yield return X(W, "Console de gerenciamento", "IAM e MFA no console", "Usuários, grupos e permissões. Grupos aplicam políticas a vários usuários de uma vez.",
                "IAM → Usuários → Criar usuário → nome → grupo → Criar usuário\nIAM → Usuários → (usuário) → Atribuir dispositivo MFA → aplicativo autenticador → 2 códigos",
                A("Administradores", "AdministratorAccess: tudo.", "Desenvolvedores", "PowerUserAccess: tudo menos IAM.", "Leitura", "ReadOnlyAccess: só visualizar."),
                A("Criar 'auditor' no grupo Leitura", "acesso só de leitura"), "Menor privilégio + MFA em contas administrativas.");
            yield return X(W, "Console de gerenciamento", "Lambda, CloudWatch, ECR, ECS e CloudFormation no console", "Resumo das telas usadas no Centro de Nuvem.",
                "Lambda → função → Testar\nCloudWatch → Alarmes → Criar alarme (métrica, instância, condição, limite, nome)\nECR → Criar repositório (privado, verificar ao enviar)\nECS → cluster → selecione o serviço → Atualizar serviço → Forçar nova implantação\nCloudFormation → Criar pilha → modelo → nome → reconhecer recursos do IAM → Enviar",
                A("Verificar ao enviar", "Scan de vulnerabilidades a cada docker push.", "Forçar nova implantação", "Sobe tarefas novas mesmo sem mudar a definição.", "Reconhecer IAM", "Obrigatório quando o modelo cria roles/políticas."),
                A("Alarme 'rede-alta': NetworkIn > 1000", "exemplo de alarme"), null);
            yield return X(W, "Básico da CLI", "iam create-user / add-user-to-group", "Cria usuários e coloca em grupos pela CLI.",
                "aws iam create-user --user-name NOME\naws iam add-user-to-group --user-name NOME --group-name GRUPO", null,
                A("aws iam create-user --user-name auditor", "cria o usuário", "aws iam add-user-to-group --user-name auditor --group-name Leitura", "dá acesso só de leitura"), null);
            yield return X(W, "Infra e monitoramento", "cloudwatch put-metric-alarm", "Cria um alarme pela CLI.",
                "aws cloudwatch put-metric-alarm --alarm-name NOME --metric-name METRICA --namespace AWS/EC2 --threshold N --comparison-operator GreaterThanThreshold --dimensions Name=InstanceId,Value=ID", null,
                A("aws cloudwatch put-metric-alarm --alarm-name rede-alta --metric-name NetworkIn --namespace AWS/EC2 --threshold 1000 --comparison-operator GreaterThanThreshold --dimensions Name=InstanceId,Value=i-0123", "alarme de rede"), null);
            yield return X(W, "Básico da CLI", "aws configure / aws sts", "Configura credenciais e confere qual identidade está em uso.",
                "aws configure\naws sts get-caller-identity", null,
                A("aws configure --profile estudos", "configura um perfil nomeado", "aws sts get-caller-identity", "mostra conta e usuário"),
                "Nunca coloque chaves de acesso no código ou no Git.");
            yield return X(W, "S3", "aws s3 ls / mb / cp / sync", "Comandos de alto nível do S3, parecidos com os do Linux.",
                "aws s3 ls [s3://bucket]\naws s3 mb s3://BUCKET\naws s3 cp ORIGEM DESTINO\naws s3 sync PASTA s3://BUCKET",
                A("ls", "lista buckets ou objetos", "mb", "make bucket: cria", "cp", "copia um arquivo (use --recursive para pastas)", "sync", "envia só o que mudou", "--delete", "(sync) apaga no destino o que sumiu na origem"),
                A("aws s3 mb s3://fotos-turma", "cria o bucket", "aws s3 cp logo.png s3://fotos-turma/", "envia um arquivo", "aws s3 sync ./dist s3://fotos-turma", "sincroniza a pasta"), null);
            yield return X(W, "EC2", "aws ec2 describe-instances / start / stop", "Consulta e liga/desliga máquinas virtuais EC2.",
                "aws ec2 describe-instances\naws ec2 stop-instances --instance-ids ID\naws ec2 start-instances --instance-ids ID",
                A("--instance-ids", "um ou mais IDs (i-...)", "--query / --output table", "filtra e formata a saída"),
                A("aws ec2 start-instances --instance-ids i-0f00ba7", "liga a instância"), "terminate-instances APAGA a instância.");
            yield return X(W, "IAM e logs", "aws iam list-users / aws logs tail", "Usuários IAM e logs do CloudWatch.",
                "aws iam list-users\naws logs tail GRUPO [--follow]",
                A("--follow", "continua acompanhando os logs novos"),
                A("aws iam list-roles", "lista roles", "aws logs tail /aws/lambda/cadastro --follow", "logs ao vivo de uma Lambda"), null);
            yield return X(W, "Serverless e containers", "aws lambda invoke", "Executa uma função Lambda e grava a resposta num arquivo.",
                "aws lambda invoke --function-name NOME ARQUIVO_SAIDA",
                A("--function-name", "nome da função", "--payload", "evento de entrada em JSON"),
                A("aws lambda invoke --function-name relatorio resposta.json", "testa a função relatorio"), null);
            yield return X(W, "Serverless e containers", "aws ecr create-repository", "Cria um repositório de imagens Docker no ECR.",
                "aws ecr create-repository --repository-name NOME", null,
                A("aws ecr create-repository --repository-name loja-web", "repositório loja-web"), null);
            yield return X(W, "Serverless e containers", "aws ecs update-service", "Altera um serviço ECS. Com --force-new-deployment, sobe tarefas novas.",
                "aws ecs update-service --cluster CLUSTER --service SERVICO [--force-new-deployment]", null,
                A("aws ecs update-service --cluster prod --service site --force-new-deployment", "reinicia o serviço site"), null);
            yield return X(W, "Infra e monitoramento", "aws cloudformation deploy", "Cria ou atualiza uma stack a partir de um template (infraestrutura como código).",
                "aws cloudformation deploy --template-file ARQUIVO --stack-name NOME",
                A("--capabilities CAPABILITY_IAM", "necessário quando o template cria recursos IAM"),
                A("aws cloudformation deploy --template-file rede.yaml --stack-name laboratorio", "stack laboratorio"), null);
            yield return X(W, "Infra e monitoramento", "aws cloudwatch describe-alarms", "Lista alarmes do CloudWatch.",
                "aws cloudwatch describe-alarms [--state-value ESTADO]",
                A("--state-value", "OK, ALARM ou INSUFFICIENT_DATA"),
                A("aws cloudwatch describe-alarms --state-value OK", "só os alarmes normais"), null);

            // ───────────────────────── REDES ─────────────────────────
            const string N = "Redes";
            yield return X(N, "Diagnóstico", "ping", "Envia pacotes ICMP e mede se (e quão rápido) um host responde.",
                "ping HOST", A("-c N (Linux) / -n N (Windows)", "quantidade de pacotes"),
                A("ping google.com", "testa a conectividade", "ping 8.8.8.8", "testa sem depender de DNS"), null);
            yield return X(N, "Diagnóstico", "ipconfig / ip addr / ifconfig", "Mostra as interfaces de rede e os endereços IP da máquina.",
                "ipconfig        (Windows)\nip addr  ou  ip a   (Linux)", A("ipconfig /all", "detalhes (MAC, DNS, DHCP)"),
                null, "Faixas privadas: 10.0.0.0/8, 172.16.0.0/12 e 192.168.0.0/16.");
            yield return X(N, "Diagnóstico", "nslookup / dig", "Consulta o DNS: descobre o IP de um nome.",
                "nslookup DOMINIO\ndig DOMINIO", A("dig +short", "mostra só o IP"),
                A("nslookup wikipedia.org", "IP da Wikipédia"), "DNS = Domain Name System, a 'lista telefônica' da internet.");
            yield return X(N, "Diagnóstico", "tracert / traceroute", "Mostra cada roteador (salto) no caminho até o destino.",
                "tracert HOST      (Windows)\ntraceroute HOST   (Linux)", null,
                A("tracert github.com", "rota até o GitHub"), "Asteriscos (*) indicam saltos que não respondem.");
            yield return X(N, "Diagnóstico", "netstat / ss", "Lista conexões e portas em escuta (LISTEN).",
                "netstat -an\nss -tuln", A("-a", "todas as conexões", "-n", "números em vez de nomes", "-t/-u/-l", "(ss) TCP / UDP / em escuta"),
                null, "Portas abertas sem necessidade aumentam a superfície de ataque.");
            yield return X(N, "Acesso e segurança", "ssh", "Abre um shell remoto criptografado (porta padrão 22).",
                "ssh USUARIO@HOST", A("-p", "outra porta", "-i", "arquivo de chave privada"),
                A("ssh maria@192.168.0.20", "entra como maria"), null);
            yield return X(N, "Acesso e segurança", "curl", "Faz requisições HTTP pelo terminal.",
                "curl [OPÇÕES] URL", A("-I, --head", "só os cabeçalhos (requisição HEAD)", "-X", "método (POST, PUT...)", "-d", "dados do corpo"),
                A("curl -I https://example.org", "vê código HTTP e headers"), null);
            yield return X(N, "Acesso e segurança", "ufw  (firewall Linux)", "Uncomplicated Firewall: libera ou bloqueia portas.",
                "sudo ufw allow PORTA[/tcp|udp]\nsudo ufw deny PORTA\nsudo ufw status", null,
                A("sudo ufw allow 80/tcp", "libera HTTP", "sudo ufw deny 23", "bloqueia telnet"), "Bloqueie tudo e libere só o necessário.");
            yield return X(N, "Conceitos", "Portas conhecidas", "Números de porta padrão dos serviços mais comuns.",
                "20/21 FTP · 22 SSH · 23 Telnet · 25 SMTP · 53 DNS · 80 HTTP · 443 HTTPS · 3306 MySQL · 5432 PostgreSQL · 3389 RDP", null, null);
            yield return X(N, "Conceitos", "CIDR e máscaras de sub-rede", "O /N indica quantos bits são de rede. O resto é para hosts.",
                "hosts utilizáveis = 2^(32 - N) - 2",
                A("/8", "255.0.0.0", "/16", "255.255.0.0", "/24", "255.255.255.0", "/25", "255.255.255.128 (126 hosts)", "/27", "255.255.255.224 (30 hosts)"),
                null, "Os 2 endereços descontados são o de rede e o de broadcast.");
            yield return X(N, "Conceitos", "Modelo OSI", "Sete camadas que organizam a comunicação em rede.",
                "7 Aplicação · 6 Apresentação · 5 Sessão · 4 Transporte · 3 Rede · 2 Enlace · 1 Física",
                A("Camada 2 (Enlace)", "switches, endereços MAC", "Camada 3 (Rede)", "IP e roteamento", "Camada 4 (Transporte)", "TCP/UDP, portas", "Camada 7 (Aplicação)", "HTTP, DNS"), null);

            // ───────────────────────── PYTHON ─────────────────────────
            const string P = "Python";
            yield return X(P, "Básico", "print / input", "Saída e entrada de texto.", "print(valor)\nvariavel = input()", null,
                A("print(\"Bom dia!\")", "mostra um texto", "idade = int(input())", "lê e converte para inteiro"), "input() sempre devolve texto.");
            yield return X(P, "Básico", "Listas e dicionários", "Listas usam [ ]; dicionários usam { } com pares chave: valor.",
                "nums = [1, 2, 3]\npessoa = {\"nome\": \"Ana\", \"idade\": 20}", null,
                A("notas = [7, 8.5, 10]", "lista", "aluno = {\"nome\": \"Rui\", \"turma\": \"2B\"}", "dicionário"), null);
            yield return X(P, "Básico", "sum, len, max, min", "Funções nativas para sequências.", "sum(lista)  len(lista)  max(lista)  min(lista)", null,
                A("soma = sum(notas)", "soma os itens", "qtd = len(notas)", "quantidade"), null);
            yield return X(P, "Básico", "def e return  (funções)", "Declara uma função. Em uma linha: def nome(p): return ...",
                "def nome(parametros):\n    return valor", null,
                A("def triplo(x): return x * 3", "função de uma linha"), null);
            yield return X(P, "Básico", "List comprehension", "Cria uma lista a partir de outra sequência numa única expressão.",
                "[expressao for item in sequencia]", A("range(n)", "números de 0 a n-1", "if", "filtro opcional no final"),
                A("cubos = [n**3 for n in range(4)]", "[0, 1, 8, 27]", "pares = [n for n in range(10) if n % 2 == 0]", "com filtro"), null);
            yield return X(P, "Arquivos e erros", "with open(...) as f:", "Abre um arquivo e garante que ele seja fechado no fim do bloco.",
                "with open(\"arquivo\", \"modo\") as f:", A("\"r\"", "leitura (padrão)", "\"w\"", "escrita", "\"a\"", "acrescentar"),
                A("with open(\"notas.txt\") as arq:", "abre para leitura"), null);
            yield return X(P, "Arquivos e erros", "try / except", "Captura erros para o programa não quebrar.",
                "try:\n    ...\nexcept TipoDoErro: acao", A("ValueError", "conversão inválida (int(\"abc\"))", "ZeroDivisionError", "divisão por zero", "FileNotFoundError", "arquivo inexistente"),
                A("except ZeroDivisionError: print(\"Não divida por zero\")", "trata um erro específico"), "Evite except: vazio — ele esconde bugs.");
            yield return X(P, "Pacotes e ambiente", "pip install", "Instala pacotes do PyPI.", "pip install PACOTE\npython -m pip install PACOTE",
                A("-r requirements.txt", "instala a lista do arquivo"), A("pip install requests", "instala o requests"), null);
            yield return X(P, "Pacotes e ambiente", "python -m venv", "Cria um ambiente virtual isolado para o projeto.", "python -m venv PASTA", null,
                A("python -m venv .env", "ambiente na pasta .env"), "Ative: PASTA\\Scripts\\activate (Windows) ou source PASTA/bin/activate (Linux).");
            yield return X(P, "Dados e web", "pandas: read_csv e groupby", "Leitura e análise de tabelas com pandas (import pandas as pd).",
                "df = pd.read_csv(\"arquivo.csv\")\ndf.groupby(\"coluna\")[\"outra\"].sum()",
                A("head()", "primeiras linhas", "groupby", "agrupa por uma coluna", "sum/mean/count", "agregações"),
                A("tabela = pd.read_csv(\"alunos.csv\")", "lê o CSV", "tabela.groupby(\"turma\")[\"nota\"].mean()", "média por turma"), null);
            yield return X(P, "Dados e web", "Flask: @app.route", "Decorator que liga uma URL a uma função.", "@app.route(\"/caminho\")\ndef funcao(): ...", null,
                A("@app.route(\"/sobre\")", "rota /sobre", "@app.get(\"/status\")", "atalho para GET"), null);
            yield return X(P, "Dados e web", "Classes e __init__", "O construtor __init__ roda ao criar o objeto. O primeiro parâmetro é self.",
                "class Nome:\n    def __init__(self, params):", null,
                A("def __init__(self, titulo, autor):", "construtor com dois parâmetros"), "Dentro dele: self.titulo = titulo");

            // ───────────────────────── PHP ─────────────────────────
            const string H = "PHP";
            yield return X(H, "Básico", "echo e variáveis", "echo envia texto para a página. Variáveis começam com $. Instruções terminam com ;",
                "echo \"texto\";\n$variavel = valor;", null,
                A("echo \"Bem-vindo!\";", "mostra o texto", "$total = 49.90;", "variável decimal"), "O código PHP fica entre <?php e ?>.");
            yield return X(H, "Básico", "Arrays e count", "Arrays indexados [ ] ou associativos [chave => valor]. count() conta os elementos.",
                "$lista = [\"a\", \"b\"];\n$assoc = [\"nome\" => \"Ana\"];\ncount($lista)", null,
                A("$cores = [\"azul\", \"verde\"];", "array", "echo count($cores);", "mostra 2"), null);
            yield return X(H, "Básico", "function", "Declara uma função.", "function nome($param) { return valor; }", null,
                A("function triplo($x) { return $x * 3; }", "função de uma linha"), null);
            yield return X(H, "Básico", "foreach", "Percorre cada elemento de um array.", "foreach ($array as $item) { ... }\nforeach ($array as $chave => $valor) { ... }", null,
                A("foreach ($cores as $cor) { echo $cor; }", "mostra cada cor"), null);
            yield return X(H, "Web", "$_POST / $_GET", "Superglobais com os dados enviados por formulários.", "$_POST[\"campo\"]\n$_GET[\"parametro\"]", null,
                A("$nome = $_POST[\"nome\"];", "campo nome via POST"), "Sempre valide e escape os dados recebidos.");
            yield return X(H, "Web", "json_encode / json_decode", "Converte arrays em JSON e vice-versa.", "json_encode($array)\njson_decode($texto, true)", null,
                A("echo json_encode($lista);", "responde JSON"), "Envie o header Content-Type: application/json.");
            yield return X(H, "Web", "php -S  (servidor embutido)", "Servidor web de desenvolvimento do próprio PHP.", "php -S HOST:PORTA", null,
                A("php -S localhost:3000", "serve a pasta atual na porta 3000"), "Não use em produção.");
            yield return X(H, "Banco e segurança", "PDO: conexão", "Conecta a bancos de dados de forma segura.",
                "$pdo = new PDO(\"mysql:host=HOST;dbname=BANCO\", \"usuario\", \"senha\");", null,
                A("$db = new PDO(\"mysql:host=127.0.0.1;dbname=escola\", \"aluno\", \"1234\");", "conecta ao banco escola"), null);
            yield return X(H, "Banco e segurança", "prepare / execute", "Consultas preparadas: os valores vão separados do SQL (evita SQL Injection).",
                "$stmt = $pdo->prepare(\"SQL com ?\");\n$stmt->execute([$valor]);", null,
                A("$q = $db->prepare(\"SELECT * FROM alunos WHERE turma = ?\");", "prepara", "$q->execute([$turma]);", "executa com o valor"),
                "Nunca concatene entrada do usuário no SQL.");
            yield return X(H, "Banco e segurança", "password_hash / password_verify", "Gera e confere hashes seguros de senha.",
                "password_hash($senha, PASSWORD_DEFAULT)\npassword_verify($senha, $hash)", null,
                A("$h = password_hash($pin, PASSWORD_DEFAULT);", "gera o hash"), "Nunca guarde senha em texto puro nem use md5.");
            yield return X(H, "Ferramentas", "composer / php artisan", "Composer gerencia dependências; artisan é a CLI do Laravel.",
                "composer install\ncomposer require PACOTE\nphp artisan migrate", null,
                A("composer require monolog/monolog", "adiciona um pacote", "php artisan migrate:rollback", "desfaz a última migration"), null);

            // ───────────────────────── SQL ─────────────────────────
            const string Q = "SQL";
            yield return X(Q, "Explorar o banco", "SHOW TABLES / DESCRIBE", "Mostra as tabelas do banco e a estrutura (colunas e tipos) de uma tabela. Explorar não altera nada.",
                "SHOW TABLES;\nDESCRIBE tabela;\nSHOW INDEX FROM tabela;", null,
                A("SHOW TABLES;", "lista as tabelas", "DESCRIBE livros;", "colunas, tipos e chaves da tabela livros"),
                "No terminal mysql, todo comando termina com ponto e vírgula (;).");
            yield return X(Q, "Consultas", "SELECT … FROM", "Lê dados de uma tabela. Depois do SELECT vêm as colunas (ou * para todas).",
                "SELECT coluna1, coluna2 FROM tabela;\nSELECT * FROM tabela;", null,
                A("SELECT * FROM livros;", "todas as colunas de todos os livros", "SELECT titulo, autor FROM livros;", "só duas colunas"),
                "Apelidos: SELECT titulo AS nome_do_livro FROM livros;");
            yield return X(Q, "Consultas", "WHERE", "Filtra as linhas. Texto vai entre aspas simples; número, sem aspas.",
                "SELECT ... FROM tabela WHERE condição;",
                A("=  <>  <  <=  >  >=", "comparações", "AND / OR / NOT", "combina condições", "IS NULL / IS NOT NULL", "testa valores vazios (nunca use = NULL)",
                  "IN (a, b, c)", "está na lista", "BETWEEN x AND y", "entre dois valores (inclusive)"),
                A("SELECT titulo FROM livros WHERE ano >= 2000;", "livros a partir de 2000", "SELECT nome FROM alunos WHERE turma = '3B' AND ativo = 1;", "duas condições"), null);
            yield return X(Q, "Consultas", "LIKE  (busca por padrão)", "Compara texto com um padrão. % = qualquer sequência de caracteres; _ = exatamente um caractere.",
                "... WHERE coluna LIKE 'padrão';", null,
                A("SELECT titulo FROM livros WHERE titulo LIKE 'O %';", "títulos que começam com 'O '", "SELECT nome FROM alunos WHERE email LIKE '%@escola.com';", "e-mails que terminam assim"),
                "No MySQL, LIKE normalmente não diferencia maiúsculas de minúsculas.");
            yield return X(Q, "Consultas", "ORDER BY / LIMIT", "Ordena o resultado e limita a quantidade de linhas.",
                "... ORDER BY coluna [ASC | DESC]\n... LIMIT n",
                A("ASC", "crescente (padrão)", "DESC", "decrescente", "LIMIT 5", "só as 5 primeiras linhas"),
                A("SELECT titulo, paginas FROM livros ORDER BY paginas DESC LIMIT 3;", "os 3 livros mais longos"),
                "Sem ORDER BY, a ordem das linhas não é garantida.");
            yield return X(Q, "Agregações", "COUNT, SUM, AVG, MIN, MAX", "Funções que resumem várias linhas num valor só.",
                "SELECT COUNT(*) FROM tabela;\nSELECT AVG(coluna) FROM tabela;",
                A("COUNT(*)", "quantidade de linhas", "SUM(col)", "soma", "AVG(col)", "média", "MIN(col) / MAX(col)", "menor / maior valor"),
                A("SELECT COUNT(*) FROM alunos;", "quantos alunos existem", "SELECT MAX(nota) FROM provas;", "a maior nota"),
                "Agregações não podem ir no WHERE. Para filtrar por elas, use HAVING.");
            yield return X(Q, "Agregações", "GROUP BY / HAVING", "GROUP BY junta linhas iguais em grupos (uma linha por grupo). HAVING filtra os grupos depois de agrupar.",
                "SELECT coluna, COUNT(*) FROM tabela GROUP BY coluna;\n... GROUP BY coluna HAVING COUNT(*) > n;", null,
                A("SELECT turma, COUNT(*) FROM alunos GROUP BY turma;", "quantos alunos por turma", "SELECT autor, COUNT(*) FROM livros GROUP BY autor HAVING COUNT(*) >= 3;", "autores com 3 livros ou mais"),
                "Toda coluna do SELECT que não está numa agregação precisa estar no GROUP BY. Ordem: WHERE → GROUP BY → HAVING → ORDER BY.");
            yield return X(Q, "Juntar tabelas", "JOIN … ON", "Junta linhas de duas tabelas que combinam pela chave (normalmente chave estrangeira = chave primária).",
                "SELECT a.col, b.col FROM tabela_a a JOIN tabela_b b ON a.chave_id = b.id;",
                A("JOIN / INNER JOIN", "só linhas com par nas duas tabelas", "LEFT JOIN", "todas as linhas da tabela da esquerda; sem par, as colunas da direita vêm NULL",
                  "apelido (a, b)", "nome curto da tabela: a.coluna"),
                A("SELECT e.titulo, a.nome FROM emprestimos e JOIN alunos a ON e.aluno_id = a.id;", "cada empréstimo com o nome do aluno",
                  "SELECT a.nome FROM alunos a LEFT JOIN emprestimos e ON e.aluno_id = a.id WHERE e.id IS NULL;", "alunos que nunca pegaram livro"),
                "Se duas tabelas têm a mesma coluna (ex.: id), diga de qual é: a.id.");
            yield return X(Q, "Juntar tabelas", "Subconsultas", "Um SELECT dentro de outro, entre parênteses.",
                "... WHERE coluna > (SELECT AVG(coluna) FROM tabela)\n... WHERE id IN (SELECT ... )\n... WHERE id NOT IN (SELECT ... )", null,
                A("SELECT titulo FROM livros WHERE paginas > (SELECT AVG(paginas) FROM livros);", "livros maiores que a média",
                  "SELECT nome FROM alunos WHERE id NOT IN (SELECT aluno_id FROM emprestimos);", "alunos sem empréstimo"), null);
            yield return X(Q, "Alterar dados", "INSERT INTO", "Insere novas linhas. Liste as colunas e depois os valores na mesma ordem.",
                "INSERT INTO tabela (col1, col2) VALUES (valor1, valor2);", null,
                A("INSERT INTO livros (titulo, autor, ano) VALUES ('Dom Casmurro', 'Machado de Assis', 1899);", "insere um livro",
                  "INSERT INTO alunos (nome, turma) VALUES ('Rita', '1A'), ('Caio', '2B');", "duas linhas de uma vez"),
                "Colunas AUTO_INCREMENT (como id) ficam de fora: o banco numera sozinho. Números decimais usam ponto: 7.50.");
            yield return X(Q, "Alterar dados", "UPDATE … SET … WHERE", "Altera valores de linhas existentes.",
                "UPDATE tabela SET coluna = valor WHERE condição;", null,
                A("UPDATE livros SET ano = 1900 WHERE id = 7;", "corrige o ano de um livro", "UPDATE alunos SET turma = '3A', ativo = 1 WHERE nome = 'Rita';", "duas colunas de uma vez"),
                "SEM WHERE, TODAS as linhas mudam! Antes, rode um SELECT com o mesmo WHERE para conferir.");
            yield return X(Q, "Alterar dados", "DELETE FROM … WHERE", "Apaga linhas.",
                "DELETE FROM tabela WHERE condição;", null,
                A("DELETE FROM emprestimos WHERE devolvido = 1;", "apaga os empréstimos já devolvidos"),
                "SEM WHERE, a tabela inteira é esvaziada! Chaves estrangeiras podem impedir apagar linhas que outras tabelas usam.");
            yield return X(Q, "Estrutura", "CREATE TABLE", "Cria uma tabela nova, com as colunas e seus tipos.",
                "CREATE TABLE nome (\n  coluna TIPO [PRIMARY KEY] [AUTO_INCREMENT] [NOT NULL],\n  ...\n);",
                A("INT", "número inteiro", "DECIMAL(10,2)", "número com 2 casas decimais (dinheiro)", "VARCHAR(100)", "texto de até 100 caracteres", "DATE", "data 'AAAA-MM-DD'",
                  "PRIMARY KEY", "identifica cada linha (não repete)", "AUTO_INCREMENT", "numera sozinho", "NOT NULL", "obrigatório",
                  "FOREIGN KEY (col) REFERENCES t(id)", "liga a coluna à chave de outra tabela"),
                A("CREATE TABLE turmas (id INT PRIMARY KEY AUTO_INCREMENT, nome VARCHAR(20) NOT NULL, ano INT);", "tabela com três colunas"),
                "ALTER TABLE tabela ADD COLUMN coluna TIPO; adiciona uma coluna a uma tabela existente.");
            yield return X(Q, "Estrutura", "CREATE INDEX / EXPLAIN", "Índices aceleram buscas por uma coluna (como o índice de um livro). EXPLAIN mostra se a consulta usa um índice.",
                "CREATE INDEX nome_do_indice ON tabela (coluna);\nEXPLAIN SELECT ... ;", null,
                A("CREATE INDEX idx_livros_autor ON livros (autor);", "índice na coluna autor", "EXPLAIN SELECT * FROM livros WHERE autor = 'Clarice';", "type ALL = varre tudo; type ref + key = usa o índice"),
                "Índices deixam as leituras rápidas, mas cada INSERT/UPDATE fica um pouco mais lento: crie só os necessários.");
            yield return X(Q, "Estrutura", "Transações", "Agrupa vários comandos: ou todos valem (COMMIT), ou nenhum (ROLLBACK).",
                "START TRANSACTION;\n... comandos ...\nCOMMIT;   (ou ROLLBACK;)", null,
                A("START TRANSACTION;", "começa", "ROLLBACK;", "desfaz tudo desde o início da transação"),
                "Transferências bancárias são o exemplo clássico: tirar de uma conta e pôr na outra precisa acontecer junto.");
        }
    }
}
