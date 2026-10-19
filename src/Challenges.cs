using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DockerQuest
{
    class Category
    {
        public string Id, Name, Short, Area;   // Area: nome da área de carreira (usado em títulos)
        public int Room;                        // sala da empresa onde ficam os desafios
        public Color Color;
        public string Prompt;                   // prompt do terminal para desafios de código
    }

    static partial class Challenges
    {
        public static readonly string[] TierNames = { "Iniciante", "Intermediário", "Profissional" };
        public static readonly int[] TierXp = { 15, 30, 50 };

        public static readonly Category[] Cats =
        {
            new Category { Id = "docker", Name = "Docker", Short = "DOCKER", Area = "DevOps", Room = 1, Color = Art.Hx(0x2496ED), Prompt = "$" },
            new Category { Id = "redes", Name = "Redes", Short = "REDES", Area = "Redes", Room = 2, Color = Art.Hx(0x2A9D8F), Prompt = "$" },
            new Category { Id = "python", Name = "Python", Short = "PYTHON", Area = "Python", Room = 3, Color = Art.Hx(0x3776AB), Prompt = ">>>" },
            new Category { Id = "php", Name = "PHP", Short = "PHP", Area = "PHP", Room = 4, Color = Art.Hx(0x777BB4), Prompt = "php >" },
            new Category { Id = "front", Name = "HTML/CSS/JS", Short = "FRONTEND", Area = "Frontend", Room = 5, Color = Art.Hx(0xE34F26), Prompt = "web >" },
            new Category { Id = "react", Name = "React", Short = "REACT", Area = "React", Room = 6, Color = Art.Hx(0x149ECA), Prompt = "jsx >" },
            new Category { Id = "flutter", Name = "Flutter", Short = "FLUTTER", Area = "Mobile", Room = 7, Color = Art.Hx(0x02569B), Prompt = "dart >" },
            new Category { Id = "aws", Name = "AWS", Short = "AWS", Area = "Cloud", Room = 8, Color = Art.Hx(0xFF9900), Prompt = "$" },
            new Category { Id = "sql", Name = "SQL", Short = "SQL", Area = "Dados", Room = Story.SqlRoom, Color = Art.Hx(0x00838F), Prompt = "mysql>" },
        };

        public static Category Cat(string id) { return Cats.First(c => c.Id == id); }
        public static Category CatOfRoom(int room) { return Cats.FirstOrDefault(c => c.Room == room); }

        static string[] A(params string[] s) { return s; }

        static Level C(string cat, int tier, string kind, string title, string ctx, string obj, string[] ans, string[] hints, string[] explain, string summary)
        {
            return new Level
            {
                Cat = cat, Tier = tier, Kind = kind, Chapter = Cat(cat).Name + " · " + TierNames[tier], Title = title, Context = ctx, Objective = obj,
                Answers = ans, Hints = hints, Explain = explain, Summary = summary, Output = "", Ignore = new string[0]
            };
        }

        // Todos os desafios: os 63 de Docker primeiro (mantém o progresso salvo), depois as outras categorias.
        public static List<Level> All()
        {
            var list = Levels.All();
            for (int i = 0; i < list.Count; i++) list[i].Tier = i < 14 ? 0 : i < 40 ? 1 : 2;
            list.AddRange(Front());
            list.AddRange(React());
            list.AddRange(Flutter());
            list.AddRange(Aws());
            list.AddRange(Redes());
            list.AddRange(Python());
            list.AddRange(Php());
            list.AddRange(Sql());   // sempre no fim: o progresso salvo é guardado pela posição do desafio
            // conteúdo extra de cada área (arquivos src/Mais*.cs). Ordem FIXA e sempre no fim da lista:
            // cada método só ACRESCENTA desafios (e pode mudar o nível — Tier — dos existentes, nunca removê-los).
            MaisDocker(list); MaisRedes(list); MaisPython(list); MaisPhp(list); MaisFront(list); MaisReact(list); MaisFlutter(list); MaisAws(list); MaisSql(list);
            // segunda leva (arquivos src/Extra*.cs): mesma regra — ordem FIXA, sempre no fim, só acrescenta
            ExtraDocker(list); ExtraRedes(list); ExtraPython(list); ExtraPhp(list); ExtraFront(list); ExtraReact(list); ExtraFlutter(list); ExtraAws(list); ExtraSql(list);
            // a missão final do Docker fecha o nível Profissional
            foreach (var l in list) if (l.Cat == "docker" && l.Title.StartsWith("MISSÃO FINAL")) l.Order = 1;
            return list;
        }

        static partial void MaisDocker(List<Level> list);
        static partial void MaisRedes(List<Level> list);
        static partial void MaisPython(List<Level> list);
        static partial void MaisPhp(List<Level> list);
        static partial void MaisFront(List<Level> list);
        static partial void MaisReact(List<Level> list);
        static partial void MaisFlutter(List<Level> list);
        static partial void MaisAws(List<Level> list);
        static partial void MaisSql(List<Level> list);
        static partial void ExtraDocker(List<Level> list);
        static partial void ExtraRedes(List<Level> list);
        static partial void ExtraPython(List<Level> list);
        static partial void ExtraPhp(List<Level> list);
        static partial void ExtraFront(List<Level> list);
        static partial void ExtraReact(List<Level> list);
        static partial void ExtraFlutter(List<Level> list);
        static partial void ExtraAws(List<Level> list);
        static partial void ExtraSql(List<Level> list);

        // ───────────────────────────── FRONTEND ─────────────────────────────
        static IEnumerable<Level> Front()
        {
            const string F = "front";
            yield return C(F, 0, "html", "O título da vitrine",
                "A Camila precisa do título da nova página do Café 365, o super app da Micorosoft.",
                "Escreva o título principal (o mais importante da página) com o texto: Café 365",
                A("<h1>Café 365</h1>"),
                A("Títulos em HTML vão de h1 (mais importante) a h6.", "Abra com <h1> e feche com </h1>."),
                A("<h1>", "Título de nível 1: o principal da página (use só um por página).", "Café 365", "O conteúdo do título."),
                "Use os níveis de título em ordem (h1 → h2 → h3): leitores de tela e o Google usam isso para entender a página.");
            yield return C(F, 0, "html", "Link para o início",
                "O menu do site precisa de um link de volta para a página inicial.",
                "Crie um link para https://micorosoft.com com o texto: Início",
                A("<a href=\"https://micorosoft.com\">Início</a>"),
                A("A tag de link é a 'âncora'.", "<a href=\"...\">texto</a>"),
                A("<a>", "Âncora: cria um link.", "href=\"https://micorosoft.com\"", "Atributo com o endereço de destino.", "Início", "Texto clicável."),
                "Para abrir em nova aba use target=\"_blank\" (e rel=\"noopener\" por segurança).");
            yield return C(F, 0, "html", "Imagem acessível",
                "A Nina, de acessibilidade, lembra: toda imagem precisa de texto alternativo para quem usa leitor de tela.",
                "Insira a imagem logo.png com o texto alternativo: Logo",
                A("<img src=\"logo.png\" alt=\"Logo\">", "<img alt=\"Logo\" src=\"logo.png\">"),
                A("A tag é img e não tem fechamento.", "Use os atributos src e alt."),
                A("<img>", "Insere uma imagem (tag sem fechamento).", "src=\"logo.png\"", "Caminho do arquivo.", "alt=\"Logo\"", "Texto alternativo, lido por leitores de tela e exibido se a imagem falhar."),
                "O alt descreve a imagem para quem não pode vê-la. Imagens decorativas usam alt=\"\".");
            yield return C(F, 0, "code", "Parágrafos azuis",
                "O design novo do Café 365 usa texto azul nos parágrafos.",
                "Escreva a regra CSS que deixa todos os parágrafos (p) com a cor azul (blue).",
                A("p { color: blue; }"),
                A("Seletor + chaves + propriedade: valor;", "A propriedade de cor do texto é color."),
                A("p", "Seletor: todos os elementos <p>.", "color: blue;", "Propriedade de cor do texto com o valor azul."),
                "Cores também podem ser hexadecimais (#2496ED) ou rgb(36,150,237).");
            yield return C(F, 0, "code", "Olá no console",
                "Todo dev JavaScript começa testando no console do navegador (F12).",
                "Mostre no console a mensagem: Olá, Micorosoft!",
                A("console.log(\"Olá, Micorosoft!\")"),
                A("O objeto console tem uma função para registrar mensagens.", "console.____(\"texto\")"),
                A("console.log(...)", "Escreve uma mensagem no console do navegador (DevTools → Console)."),
                "console.error e console.warn também existem, com destaque em vermelho e amarelo.");

            yield return C(F, 1, "code", "Menu lado a lado",
                "Os itens do menu estão empilhados. A Camila quer todos lado a lado.",
                "Escreva a regra CSS que faz a classe .menu usar flexbox.",
                A(".menu { display: flex; }"),
                A("Flexbox é ligado pela propriedade display.", ".menu { display: ____; }"),
                A(".menu", "Seletor de classe.", "display: flex;", "Transforma o elemento em container flex: os filhos ficam em linha."),
                "Com flex você ainda tem justify-content (eixo principal) e align-items (eixo cruzado).");
            yield return C(F, 1, "code", "Card centralizado",
                "O texto dos cards de produto está alinhado à esquerda e ficou estranho.",
                "Escreva a regra CSS que centraliza o texto da classe .card.",
                A(".card { text-align: center; }"),
                A("A propriedade de alinhamento de texto começa com text-.", "text-align: ____;"),
                A(".card", "Seletor de classe.", "text-align: center;", "Centraliza o texto e elementos inline."),
                "Para centralizar blocos em si, use margin: 0 auto ou flexbox.");
            yield return C(F, 1, "code", "Preço constante",
                "O carrinho precisa guardar o preço do café, que não muda durante a compra.",
                "Crie uma constante JavaScript chamada preco com o valor 9.90",
                A("const preco = 9.90", "const preco = 9.9"),
                A("Valores que não mudam são declarados com const.", "const nome = valor"),
                A("const", "Declara uma constante (não pode ser reatribuída).", "preco = 9.90", "Nome e valor (número decimal com ponto)."),
                "Use let para valores que mudam e evite var em código moderno.");
            yield return C(F, 1, "code", "Pegando o carrinho",
                "Para atualizar o carrinho na tela, o JavaScript precisa encontrar o elemento dele no HTML.",
                "Guarde na constante carrinho o elemento com id carrinho.",
                A("const carrinho = document.getElementById(\"carrinho\")", "const carrinho = document.querySelector(\"#carrinho\")"),
                A("Use o objeto document.", "getElementById(...) ou querySelector(\"#...\")"),
                A("document", "A página carregada.", "getElementById(\"carrinho\")", "Busca o elemento pelo id."),
                "querySelector aceita qualquer seletor CSS: \".classe\", \"#id\", \"ul li\".");
            yield return C(F, 1, "html", "E-mail obrigatório",
                "O formulário de cadastro está aceitando e-mails vazios.",
                "Crie um campo de e-mail obrigatório (input do tipo email com required).",
                A("<input type=\"email\" required>", "<input required type=\"email\">"),
                A("O tipo do campo é definido pelo atributo type.", "required é um atributo sem valor."),
                A("<input type=\"email\">", "Campo que valida o formato de e-mail.", "required", "Impede enviar o formulário com o campo vazio."),
                "A validação do navegador ajuda, mas o servidor SEMPRE precisa validar de novo.");

            yield return C(F, 2, "code", "Clique para comprar",
                "O botão de compra existe, mas não faz nada quando clicado.",
                "Faça a variável botao chamar a função comprar quando for clicada (addEventListener).",
                A("botao.addEventListener(\"click\", comprar)"),
                A("Eventos são ligados com addEventListener(evento, função).", "O evento de clique se chama \"click\"."),
                A("addEventListener", "Registra uma função para um evento.", "\"click\"", "Nome do evento.", "comprar", "Função chamada (sem parênteses!)."),
                "Passe a função sem () — com parênteses ela seria executada na hora, e não no clique.");
            yield return C(F, 2, "code", "Buscando produtos",
                "A vitrine precisa carregar os produtos da API em vez de deixá-los fixos no HTML.",
                "Busque /api/produtos com fetch e converta a resposta em JSON usando .then com arrow function.",
                A("fetch(\"/api/produtos\").then(r => r.json())", "fetch(\"/api/produtos\").then(res => res.json())", "fetch(\"/api/produtos\").then(response => response.json())",
                  "fetch(\"/api/produtos\").then((r) => r.json())", "fetch(\"/api/produtos\").then((res) => res.json())", "fetch(\"/api/produtos\").then((response) => response.json())"),
                A("fetch(url) devolve uma Promise com a resposta.", "...then(r => r.json())"),
                A("fetch(\"/api/produtos\")", "Faz a requisição HTTP.", ".then(r => r.json())", "Quando a resposta chegar, lê o corpo como JSON."),
                "Com async/await: const r = await fetch(url); const dados = await r.json();");
            yield return C(F, 2, "code", "Galeria em grid",
                "A galeria de fotos das cafeterias precisa de 3 colunas iguais.",
                "Escreva a regra CSS da classe .galeria usando grid com 3 colunas iguais (repeat).",
                A(".galeria { display: grid; grid-template-columns: repeat(3, 1fr); }", ".galeria { grid-template-columns: repeat(3, 1fr); display: grid; }",
                  ".galeria { display: grid; grid-template-columns: 1fr 1fr 1fr; }"),
                A("Ligue o grid com display e defina as colunas com grid-template-columns.", "repeat(3, 1fr)"),
                A("display: grid;", "Liga o layout em grade.", "grid-template-columns: repeat(3, 1fr);", "Três colunas, cada uma com 1 fração do espaço."),
                "Use gap para espaçar as células: gap: 16px;");
            yield return C(F, 2, "code", "Menu no celular",
                "No celular o menu enorme cobre a tela toda.",
                "Escreva uma media query que esconde o .menu em telas de até 600px.",
                A("@media (max-width: 600px) { .menu { display: none; } }"),
                A("Media queries começam com @media.", "@media (max-width: 600px) { ... }"),
                A("@media (max-width: 600px)", "Aplica as regras só em telas de até 600px.", ".menu { display: none; }", "Esconde o menu."),
                "Mobile first: escreva o CSS do celular primeiro e use min-width para telas maiores.");
            yield return C(F, 2, "code", "Arrow function",
                "O carrinho precisa de uma função curtinha que dobra uma quantidade.",
                "Crie a constante dobro com uma arrow function que recebe n e retorna n * 2.",
                A("const dobro = n => n * 2", "const dobro = (n) => n * 2", "const dobro = (n) => { return n * 2 }", "const dobro = n => { return n * 2 }"),
                A("Arrow functions usam =>.", "const nome = (param) => expressão"),
                A("const dobro =", "Guarda a função numa constante.", "n => n * 2", "Recebe n e retorna n * 2 (retorno implícito)."),
                "Com chaves { } o retorno precisa ser explícito: (n) => { return n * 2; }");
        }

        // ───────────────────────────── REACT ─────────────────────────────
        static IEnumerable<Level> React()
        {
            const string R = "react";
            yield return C(R, 0, "cmd", "Instalando dependências",
                "Você clonou o projeto do Café 365 Web. O Thiago avisa: antes de tudo, instale as dependências.",
                "Instale as dependências listadas no package.json.",
                A("npm install", "npm i"),
                A("O gerenciador de pacotes do Node é o npm.", "npm ________"),
                A("npm install", "Lê o package.json e baixa tudo para a pasta node_modules."),
                "Nunca envie node_modules para o Git: o package-lock.json garante as mesmas versões.");
            yield return C(R, 0, "cmd", "Servidor de desenvolvimento",
                "Dependências instaladas! Hora de ver o app rodando no navegador.",
                "Rode o script dev do projeto.",
                A("npm run dev"),
                A("Scripts do package.json rodam com npm run.", "npm run ___"),
                A("npm run dev", "Executa o script \"dev\": servidor local com recarregamento automático (Vite)."),
                "Em projetos Create React App o script costuma ser npm start.");
            yield return C(R, 0, "code", "Olá, usuário",
                "O cabeçalho deve cumprimentar o usuário logado, que chega na variável nome.",
                "Escreva o JSX de um h2 com o texto Olá, seguido da variável nome entre chaves.",
                A("<h2>Olá, {nome}</h2>"),
                A("Em JSX, variáveis aparecem entre chaves.", "<h2>Olá, {...}</h2>"),
                A("<h2>", "Elemento JSX (parece HTML).", "{nome}", "Expressão JavaScript dentro do JSX."),
                "Qualquer expressão JS cabe nas chaves: {nome.toUpperCase()}, {2 + 2}...");
            yield return C(R, 0, "code", "Classe no JSX",
                "O Caio colocou class=\"destaque\" no JSX e o React reclamou.",
                "Escreva uma div vazia com a classe CSS destaque usando a forma correta do JSX.",
                A("<div className=\"destaque\"></div>", "<div className=\"destaque\" />"),
                A("Em JSX, 'class' é palavra reservada do JavaScript.", "Use className."),
                A("className", "Nome do atributo de classe CSS no JSX.", "\"destaque\"", "A classe."),
                "Outros nomes mudam também: for vira htmlFor, onclick vira onClick.");
            yield return C(R, 0, "code", "Importando o useState",
                "Para guardar estado no componente, você precisa do hook useState.",
                "Importe o hook useState do pacote react.",
                A("import { useState } from \"react\""),
                A("Hooks são importações nomeadas, entre chaves.", "import { ... } from \"react\""),
                A("import { useState }", "Importação nomeada do hook.", "from \"react\"", "Pacote de origem."),
                "Hooks só podem ser chamados no topo de componentes ou de outros hooks.");

            yield return C(R, 1, "code", "Estado do contador",
                "O botão de quantidade do carrinho precisa lembrar o número de itens.",
                "Crie o estado contador (com setContador) começando em 0.",
                A("const [contador, setContador] = useState(0)"),
                A("useState devolve um par: valor e função para atualizar.", "const [valor, setValor] = useState(inicial)"),
                A("useState(0)", "Cria o estado com valor inicial 0.", "[contador, setContador]", "Desestruturação: valor atual e função de atualização."),
                "Nunca altere o estado direto (contador++): use sempre setContador.");
            yield return C(R, 1, "code", "Botão +1",
                "Agora o botão precisa somar 1 ao contador quando clicado.",
                "Escreva o JSX de um botão com texto +1 cujo onClick chama setContador(contador + 1) numa arrow function.",
                A("<button onClick={() => setContador(contador + 1)}>+1</button>"),
                A("Eventos no JSX são camelCase: onClick.", "onClick={() => ...}"),
                A("onClick={...}", "Função chamada no clique.", "() => setContador(contador + 1)", "Arrow function que atualiza o estado."),
                "Se o novo valor depende do anterior, prefira: setContador(c => c + 1).");
            yield return C(R, 1, "code", "Lista de produtos",
                "A vitrine recebe o array produtos e precisa mostrar cada um como item de lista.",
                "Use produtos.map para gerar um <li> por produto p, com key={p.id} e o texto {p.nome} (tudo entre chaves).",
                A("{produtos.map(p => <li key={p.id}>{p.nome}</li>)}", "{produtos.map((p) => <li key={p.id}>{p.nome}</li>)}"),
                A("Listas em React são feitas com .map().", "Cada item precisa de uma key única."),
                A("produtos.map(...)", "Transforma cada produto em JSX.", "key={p.id}", "Identificador único que ajuda o React a atualizar a lista."),
                "Evite usar o índice do array como key quando a lista pode mudar de ordem.");
            yield return C(R, 1, "code", "Só para quem está logado",
                "O carrinho só deve aparecer se o usuário estiver logado (variável logado).",
                "Renderize <Carrinho /> somente se logado for verdadeiro, usando &&.",
                A("{logado && <Carrinho />}"),
                A("Em JS, a && b só avalia b se a for verdadeiro.", "{condição && <Componente />}"),
                A("logado &&", "Condição.", "<Carrinho />", "Só é renderizado se a condição for verdadeira."),
                "Para mostrar uma coisa OU outra, use o ternário: {logado ? <Carrinho /> : <Login />}.");
            yield return C(R, 1, "code", "Passando props",
                "O componente Produto precisa saber o preço, que é um número.",
                "Renderize <Produto /> passando a prop preco com o número 10.",
                A("<Produto preco={10} />", "<Produto preco={10}></Produto>"),
                A("Props são atributos do componente.", "Números vão entre chaves: {10}"),
                A("<Produto ... />", "Usa o componente.", "preco={10}", "Prop numérica (com aspas viraria texto \"10\")."),
                "Dentro de Produto, leia com: function Produto({ preco }) { ... }");

            yield return C(R, 2, "code", "Carregar ao montar",
                "A lista de pedidos precisa ser carregada uma única vez, quando a tela abre.",
                "Use useEffect para chamar carregar() apenas uma vez (array de dependências vazio).",
                A("useEffect(() => { carregar() }, [])", "useEffect(() => carregar(), [])"),
                A("useEffect recebe uma função e um array de dependências.", "Array vazio [] = só na montagem."),
                A("useEffect(..., [])", "Efeito colateral executado após a renderização.", "[]", "Sem dependências: roda uma vez só."),
                "Esquecer o array faz o efeito rodar a cada renderização (e pode virar loop!).");
            yield return C(R, 2, "code", "Contexto de tema",
                "O app terá tema claro e escuro compartilhado por todos os componentes.",
                "Crie a constante TemaContext usando createContext().",
                A("const TemaContext = createContext()", "const TemaContext = React.createContext()"),
                A("Contextos evitam passar props por vários níveis.", "const X = createContext()"),
                A("createContext()", "Cria um contexto compartilhado."),
                "Envolva a árvore com <TemaContext.Provider value={...}>.");
            yield return C(R, 2, "code", "Lendo o contexto",
                "O botão de tema precisa ler o valor atual do TemaContext.",
                "Guarde em tema o valor do TemaContext usando o hook adequado.",
                A("const tema = useContext(TemaContext)"),
                A("Existe um hook específico para ler contextos.", "useC______(TemaContext)"),
                A("useContext(TemaContext)", "Lê o valor do Provider mais próximo."),
                "Quando o valor do Provider muda, todos que usam useContext re-renderizam.");
            yield return C(R, 2, "code", "Total memorizado",
                "O cálculo do total do carrinho é pesado e está rodando a cada tecla digitada.",
                "Guarde em total o resultado de calcularTotal(itens) com useMemo, recalculando só quando itens mudar.",
                A("const total = useMemo(() => calcularTotal(itens), [itens])"),
                A("useMemo(função, dependências)", "A dependência é [itens]."),
                A("useMemo", "Memoriza o resultado.", "[itens]", "Só recalcula quando itens mudar."),
                "Não use useMemo em tudo: só em cálculos realmente caros.");
            yield return C(R, 2, "cmd", "Build de produção",
                "Vai subir para produção! O site precisa ser otimizado e minificado.",
                "Rode o script build do projeto.",
                A("npm run build"),
                A("O script que gera os arquivos finais chama-se build.", "npm run _____"),
                A("npm run build", "Gera a pasta dist/ com HTML, CSS e JS otimizados."),
                "A pasta gerada pode ir para um bucket S3, uma CDN ou um container nginx.");
        }

        // ───────────────────────────── FLUTTER ─────────────────────────────
        static IEnumerable<Level> Flutter()
        {
            const string L = "flutter";
            yield return C(L, 0, "cmd", "Novo app",
                "O Bruno quer um app novo do Café 365 para Android e iOS.",
                "Crie um projeto Flutter chamado cafe_app.",
                A("flutter create cafe_app"),
                A("O comando do Flutter para novos projetos é create.", "flutter create <nome>"),
                A("flutter create", "Gera a estrutura do projeto (lib/main.dart, pubspec.yaml...).", "cafe_app", "Nome (letras minúsculas e _)."),
                "Um único código Dart gera apps para Android, iOS, web e desktop.");
            yield return C(L, 0, "cmd", "Rodando no emulador",
                "O emulador já está aberto. Bora ver o app!",
                "Rode o app no dispositivo/emulador conectado.",
                A("flutter run"),
                A("O verbo é 'run'.", "flutter ___"),
                A("flutter run", "Compila e instala o app; aperte r para hot reload."),
                "Hot reload (r) aplica mudanças em segundos sem perder o estado da tela.");
            yield return C(L, 0, "code", "Primeiro texto",
                "A tela inicial precisa mostrar o nome do app.",
                "Crie um widget de texto com: Café 365",
                A("Text(\"Café 365\")", "const Text(\"Café 365\")"),
                A("O widget de texto se chama Text.", "Text('...')"),
                A("Text(...)", "Widget que exibe texto."),
                "Use o parâmetro style: TextStyle(fontSize: 24) para mudar a aparência.");
            yield return C(L, 0, "code", "Variável final",
                "O nome do usuário não muda depois de definido.",
                "Declare em Dart uma variável final nome com o valor 'Ana'.",
                A("final nome = \"Ana\"", "final String nome = \"Ana\""),
                A("Valores que não mudam: final (ou const).", "final nome = '...';"),
                A("final", "Atribuída uma única vez.", "nome = 'Ana'", "Nome e valor."),
                "const é para valores conhecidos em tempo de compilação; final, em tempo de execução.");
            yield return C(L, 0, "cmd", "Baixando pacotes",
                "Você editou o pubspec.yaml e o editor diz que faltam pacotes.",
                "Baixe as dependências listadas no pubspec.yaml.",
                A("flutter pub get"),
                A("O gerenciador de pacotes do Flutter é o pub.", "flutter pub ___"),
                A("flutter pub get", "Baixa os pacotes declarados no pubspec.yaml."),
                "Os pacotes ficam em pub.dev.");

            yield return C(L, 1, "code", "Botão de compra",
                "A tela de produto precisa de um botão de compra.",
                "Crie um ElevatedButton com onPressed: comprar e child: Text('Comprar').",
                A("ElevatedButton(onPressed: comprar, child: Text(\"Comprar\"))", "ElevatedButton(child: Text(\"Comprar\"), onPressed: comprar)",
                  "ElevatedButton(onPressed: comprar, child: const Text(\"Comprar\"))"),
                A("Parâmetros nomeados: onPressed e child.", "ElevatedButton(onPressed: ..., child: ...)"),
                A("onPressed: comprar", "Função chamada no toque (null = botão desativado).", "child: Text('Comprar')", "Conteúdo do botão."),
                "Também existem TextButton e OutlinedButton.");
            yield return C(L, 1, "code", "Em coluna",
                "Dois widgets, a e b, precisam ficar um embaixo do outro.",
                "Organize a e b em uma Column.",
                A("Column(children: [a, b])"),
                A("Column recebe uma lista de filhos.", "children: [ ... ]"),
                A("Column", "Empilha na vertical (Row é na horizontal).", "children: [a, b]", "Lista de widgets."),
                "mainAxisAlignment e crossAxisAlignment controlam o alinhamento.");
            yield return C(L, 1, "code", "Respiro",
                "O texto está colado na borda da tela.",
                "Envolva o widget filho com um Padding de 16 em todos os lados.",
                A("Padding(padding: EdgeInsets.all(16), child: filho)", "Padding(padding: const EdgeInsets.all(16), child: filho)",
                  "Padding(child: filho, padding: EdgeInsets.all(16))"),
                A("Espaçamento interno em Flutter é um widget: Padding.", "EdgeInsets.all(16)"),
                A("Padding", "Widget de espaçamento.", "EdgeInsets.all(16)", "16 pixels lógicos em todos os lados."),
                "EdgeInsets.symmetric(horizontal: 8) e EdgeInsets.only(top: 4) dão mais controle.");
            yield return C(L, 1, "code", "Atualizando a tela",
                "O contador aumentou mas a tela não mudou.",
                "Dentro do StatefulWidget, incremente contador avisando o Flutter com setState.",
                A("setState(() { contador++; })", "setState(() => contador++)", "setState(() { contador += 1; })"),
                A("Mudanças de estado precisam do setState.", "setState(() { ... })"),
                A("setState", "Avisa o Flutter para redesenhar.", "contador++", "A mudança em si."),
                "Para apps grandes, use gerenciadores como Provider, Riverpod ou Bloc.");
            yield return C(L, 1, "cmd", "Pacote http",
                "O app vai conversar com a API do Café 365.",
                "Adicione o pacote http ao projeto pelo terminal.",
                A("flutter pub add http"),
                A("pub tem um subcomando para adicionar pacotes.", "flutter pub add <pacote>"),
                A("flutter pub add http", "Adiciona o pacote ao pubspec.yaml e já baixa."),
                "Confira versão, popularidade e manutenção no pub.dev antes de adicionar pacotes.");

            yield return C(L, 2, "code", "Nova tela",
                "Ao tocar num produto, o app deve abrir a tela de detalhes.",
                "Navegue para DetalhesPage com Navigator.push e MaterialPageRoute (builder com context).",
                A("Navigator.push(context, MaterialPageRoute(builder: (context) => DetalhesPage()))",
                  "Navigator.push(context, MaterialPageRoute(builder: (context) => const DetalhesPage()))",
                  "Navigator.push(context, MaterialPageRoute(builder: (_) => DetalhesPage()))"),
                A("Navigator.push empilha uma nova rota.", "MaterialPageRoute(builder: (context) => ...)"),
                A("Navigator.push", "Abre uma nova tela por cima da atual.", "MaterialPageRoute", "Rota com animação padrão Android/iOS."),
                "Para voltar: Navigator.pop(context).");
            yield return C(L, 2, "code", "Função assíncrona",
                "Carregar dados da internet leva tempo e não pode travar a tela.",
                "Declare uma função assíncrona carregar que retorna Future<void> (corpo vazio).",
                A("Future<void> carregar() async {}"),
                A("Funções assíncronas retornam Future e usam async.", "Future<void> nome() async { }"),
                A("Future<void>", "Promete terminar no futuro, sem valor.", "async", "Permite usar await dentro da função."),
                "Use FutureBuilder para mostrar carregando/erro/dados na tela.");
            yield return C(L, 2, "code", "Esperando a resposta",
                "Hora de chamar a API!",
                "Guarde em resposta o resultado de http.get(url), esperando com await.",
                A("final resposta = await http.get(url)", "var resposta = await http.get(url)"),
                A("await espera o Future terminar.", "final resposta = await ..."),
                A("await", "Pausa a função (sem travar o app) até a resposta chegar.", "http.get(url)", "Requisição GET."),
                "Confira resposta.statusCode antes de usar o corpo.");
            yield return C(L, 2, "code", "JSON para mapa",
                "O corpo da resposta chegou como texto JSON.",
                "Converta resposta.body em dados com jsonDecode.",
                A("final dados = jsonDecode(resposta.body)", "var dados = jsonDecode(resposta.body)"),
                A("A função vem de dart:convert.", "jsonDecode(...)"),
                A("jsonDecode", "Transforma o texto JSON em Map/List do Dart.", "resposta.body", "O corpo da resposta."),
                "Para tipagem forte, crie classes com factory fromJson.");
            yield return C(L, 2, "cmd", "Rumo à loja",
                "O app está pronto para a Play Store!",
                "Gere o APK de release.",
                A("flutter build apk --release", "flutter build apk"),
                A("O comando de compilação é build.", "flutter build apk ..."),
                A("flutter build apk", "Compila o app Android.", "--release", "Modo otimizado (o padrão do build)."),
                "Para a Play Store, o formato recomendado é: flutter build appbundle.");
        }

        // ───────────────────────────── AWS (console simulado + CloudShell) ─────────────────────────────
        static Level AwsL(int tier, string task, string title, string ctx, string obj, string[] ans, string[] hints, string[] explain, string summary)
        {
            var l = C("aws", tier, "aws", title, ctx, obj, ans, hints, explain, summary);
            l.Task = task;
            return l;
        }

        // Answers[0] = passos no console (mostrados ao revelar); Answers[1] = alternativa pela CLI no CloudShell (quando existe)
        static IEnumerable<Level> Aws()
        {
            yield return AwsL(0, "a1", "Primeiro acesso",
                "Primeiro dia no Centro de Nuvem! A Helena abriu o Console da AWS para você e quer ver se sabe se localizar.",
                "Abra o serviço S3 (armazenamento) pelo menu Serviços ou pela barra de pesquisa do console.",
                A("Console: clique em Serviços (ou na pesquisa) → S3", "aws s3 ls"),
                A("No topo do console há o menu Serviços e uma barra de pesquisa.", "Digite 'S3' na pesquisa e aperte Enter."),
                A("Serviços / pesquisa", "Todos os serviços da AWS ficam no menu Serviços; a pesquisa é o atalho mais rápido.", "S3", "Simple Storage Service: guarda arquivos (objetos) em 'buckets'."),
                "No dia a dia, use a pesquisa e marque os serviços favoritos com a estrela para achar rápido.");
            yield return AwsL(0, "a2", "Criando o bucket",
                "O site estático do Café 365 vai morar no S3, mas ainda não existe um bucket para ele.",
                "No S3, crie o bucket cafe365-site mantendo o acesso público BLOQUEADO.",
                A("Console: S3 → Criar bucket → nome cafe365-site → deixe 'Bloquear todo o acesso público' marcado → Criar bucket", "aws s3 mb s3://cafe365-site"),
                A("No S3 há o botão laranja 'Criar bucket'.", "Nome: cafe365-site. Não desmarque o bloqueio de acesso público."),
                A("Criar bucket", "Cria um 'balde' para guardar arquivos.", "cafe365-site", "Nome único no mundo inteiro, só letras minúsculas, números e hífens.", "Bloquear acesso público", "Padrão seguro: ninguém de fora lê os arquivos sem permissão."),
                "Vazamento de dados em bucket público é um dos incidentes mais comuns na nuvem. Libere acesso só quando precisar, e com cuidado.");
            yield return AwsL(0, "a3", "Enviando a página",
                "O index.html do site está pronto na sua máquina.",
                "Envie (carregue) o arquivo index.html para o bucket cafe365-site.",
                A("Console: S3 → cafe365-site → Carregar → marque index.html → Carregar", "aws s3 cp index.html s3://cafe365-site/"),
                A("Abra o bucket clicando no nome dele.", "Use o botão 'Carregar' e escolha só o index.html."),
                A("Carregar (upload)", "Envia arquivos do seu computador para o bucket.", "index.html", "Vira um 'objeto' no S3, com chave (nome) index.html."),
                "Para pastas inteiras, a CLI tem o 'aws s3 sync', que só envia o que mudou.");
            yield return AwsL(0, "a4", "Mudando de região",
                "Os servidores do time do Brasil ficam numa região diferente. Você não está achando nada no EC2...",
                "Troque a região do console para América do Sul (São Paulo) — sa-east-1.",
                A("Console: seletor de região no canto superior direito → América do Sul (São Paulo) sa-east-1", "aws configure set region sa-east-1"),
                A("A região aparece no canto superior direito do console.", "Escolha 'América do Sul (São Paulo)'."),
                A("Região", "Cada região é um conjunto de data centers em um lugar do mundo.", "sa-east-1", "São Paulo: menor latência para usuários no Brasil e dados no país (LGPD)."),
                "Recursos como EC2 são regionais: se 'sumiu tudo', confira primeiro a região selecionada.");
            yield return AwsL(0, "a5", "Quem sou eu?",
                "Antes de mexer em produção, confirme com qual conta e usuário você está logado.",
                "Descubra o ID da conta: abra o menu da sua conta no canto superior direito OU rode no CloudShell o comando que mostra sua identidade.",
                A("Console: clique no nome da conta (canto superior direito) | CloudShell: aws sts get-caller-identity", "aws sts get-caller-identity"),
                A("No console, o menu da conta fica ao lado da região.", "Na CLI, o serviço de tokens é o STS: aws sts get-caller-..."),
                A("Menu da conta / sts get-caller-identity", "Mostra o ID da conta (Account), o usuário (UserId) e o ARN."),
                "Hábito de ouro: sempre confirme conta e região antes de apagar ou alterar qualquer coisa.");

            yield return AwsL(1, "b1", "Servidor esquecido",
                "Alguém deixou a instância de teste ligada há semanas. A fatura não perdoa!",
                "No EC2 (região Norte da Virgínia), INTERROMPA a instância teste-antigo (i-0abc123). Não mexa no web-1, que atende clientes.",
                A("Console: EC2 → Instâncias → selecione teste-antigo → Estado da instância → Interromper", "aws ec2 stop-instances --instance-ids i-0abc123"),
                A("Selecione a instância certa pelo nome na lista.", "Use 'Estado da instância' → Interromper (não Encerrar!)."),
                A("Interromper", "Desliga a instância; o disco (EBS) continua e dá para ligar de novo.", "Encerrar", "APAGA a instância para sempre. Cuidado!"),
                "Instância parada não cobra processamento (só o disco). Tags como 'Dono' e 'Projeto' ajudam a achar recursos esquecidos.");
            yield return AwsL(1, "b2", "Novo servidor",
                "A API do Café 365 precisa de um servidor de testes barato.",
                "No EC2 (Norte da Virgínia), execute uma instância chamada api-cafe, com Amazon Linux 2023 e tipo t3.micro.",
                A("Console: EC2 → Executar instâncias → nome api-cafe → Amazon Linux 2023 → tipo t3.micro → Executar instância"),
                A("O botão laranja 'Executar instâncias' abre o assistente.", "Escolha o tipo t3.micro (qualificado para o nível gratuito)."),
                A("AMI", "Imagem do sistema operacional (Amazon Linux, Ubuntu, Windows...).", "Tipo de instância", "Tamanho da máquina: CPU e memória. t3.micro é pequena e barata."),
                "Comece pequeno e aumente se precisar: escolher máquina grande 'por garantia' é dinheiro jogado fora.");
            yield return AwsL(1, "b3", "Acesso mínimo",
                "Um novo estagiário precisa ver os recursos da conta, mas não deve alterar nada.",
                "No IAM, crie o usuário estagiario no grupo Leitura (somente leitura).",
                A("Console: IAM → Usuários → Criar usuário → nome estagiario → grupo Leitura → Criar usuário", "aws iam create-user --user-name estagiario  +  aws iam add-user-to-group --user-name estagiario --group-name Leitura"),
                A("IAM controla quem pode fazer o quê.", "Escolha o grupo com ReadOnlyAccess, não o de administradores."),
                A("Usuário IAM", "Identidade de uma pessoa ou sistema.", "Grupo Leitura", "Aplica a política ReadOnlyAccess: só ver, nunca alterar."),
                "Princípio do menor privilégio: dê só a permissão necessária para o trabalho.");
            yield return AwsL(1, "b4", "MFA para todos",
                "A auditoria encontrou um administrador sem autenticação em dois fatores: o Rodrigo.",
                "No IAM, ative um dispositivo MFA (aplicativo autenticador) para o usuário rodrigo.",
                A("Console: IAM → Usuários → rodrigo → Atribuir dispositivo MFA → Aplicativo autenticador → digite os dois códigos → Adicionar MFA"),
                A("Abra o usuário rodrigo clicando no nome.", "Em 'Credenciais de segurança', use 'Atribuir dispositivo MFA' e digite os dois códigos do aplicativo."),
                A("MFA", "Senha + código temporário do celular.", "Dois códigos seguidos", "Provam que o aplicativo está sincronizado com a conta."),
                "Com MFA, uma senha vazada não basta para invadir a conta. Administradores SEMPRE com MFA.");
            yield return AwsL(1, "b5", "Sincronizando o site",
                "O time do frontend gerou a pasta site/ com vários arquivos: index.html, style.css, app.js e logo.png.",
                "Envie todos os arquivos da pasta site/ para o bucket cafe365-site (pelo console ou pelo CloudShell).",
                A("Console: S3 → cafe365-site → Carregar → Adicionar pasta site/ → Carregar | CloudShell: aws s3 sync ./site s3://cafe365-site", "aws s3 sync ./site s3://cafe365-site"),
                A("No console, o upload aceita uma pasta inteira.", "Na CLI: aws s3 sync origem destino."),
                A("aws s3 sync", "Copia só arquivos novos ou alterados.", "./site s3://cafe365-site", "Pasta local e bucket de destino."),
                "--delete remove do bucket o que não existe mais na pasta. Use com atenção!");

            yield return AwsL(2, "c1", "Testando a Lambda",
                "Corrigiram a função pedidos, que estava dando erro. Teste antes de liberar.",
                "No Lambda, abra a função pedidos e execute um teste.",
                A("Console: Lambda → pedidos → Testar → (evento pedido-teste) → Testar", "aws lambda invoke --function-name pedidos saida.json"),
                A("Abra a função clicando no nome.", "Na seção de teste, use o botão 'Testar'."),
                A("Lambda", "Executa código sem você gerenciar servidores (serverless).", "Evento de teste", "JSON de entrada que simula uma chamada real."),
                "Confira sempre o resultado e os logs no CloudWatch depois de testar.");
            yield return AwsL(2, "c2", "Registry seguro",
                "As imagens Docker da API vão para o ECR, o registry da AWS. A segurança exige verificar vulnerabilidades.",
                "No ECR, crie o repositório privado cafe365-api com a verificação de imagens ao enviar (scan on push) ATIVADA.",
                A("Console: ECR → Criar repositório → Privado → nome cafe365-api → marque 'Verificar ao enviar' → Criar repositório", "aws ecr create-repository --repository-name cafe365-api --image-scanning-configuration scanOnPush=true"),
                A("ECR = Elastic Container Registry.", "Não esqueça a opção 'Verificar ao enviar'."),
                A("Repositório ECR", "Guarda imagens Docker na AWS.", "Scan on push", "Procura vulnerabilidades conhecidas a cada docker push."),
                "Depois: docker tag, aws ecr get-login-password | docker login e docker push.");
            yield return AwsL(2, "c3", "Novo deploy no ECS",
                "A imagem nova da API foi enviada com a mesma tag. O ECS precisa subir tarefas novas.",
                "No ECS, atualize o serviço api do cluster cafe365 FORÇANDO uma nova implantação.",
                A("Console: ECS → cafe365 → serviço api → Atualizar → marque 'Forçar nova implantação' → Atualizar", "aws ecs update-service --cluster cafe365 --service api --force-new-deployment"),
                A("Abra o cluster e selecione o serviço api.", "Sem 'Forçar nova implantação', nada muda (a definição é a mesma)."),
                A("Forçar nova implantação", "Sobe tarefas novas, que baixam a imagem atualizada.", "Rolling update", "Troca as tarefas aos poucos, sem derrubar o site."),
                "Prefira tags de imagem únicas (ex.: o hash do commit): fica fácil saber o que está em produção e voltar atrás.");
            yield return AwsL(2, "c4", "Alarme de CPU",
                "O web-1 travou ontem por CPU alta e ninguém percebeu a tempo.",
                "No CloudWatch, crie o alarme cpu-alta-web: métrica CPUUtilization da instância web-1, maior que 80.",
                A("Console: CloudWatch → Alarmes → Criar alarme → CPUUtilization → web-1 → Maior que 80 → nome cpu-alta-web → Criar alarme", "aws cloudwatch put-metric-alarm --alarm-name cpu-alta-web --metric-name CPUUtilization --namespace AWS/EC2 --threshold 80 --comparison-operator GreaterThanThreshold --dimensions Name=InstanceId,Value=i-0web001"),
                A("CloudWatch guarda métricas e alarmes.", "Métrica CPUUtilization, condição 'Maior que', limite 80."),
                A("Métrica", "Número medido ao longo do tempo (CPU, rede, disco).", "Alarme", "Muda para EM ALARME quando a condição é atingida e pode avisar por SNS (e-mail, SMS)."),
                "Alarme bom é alarme acionável: avise quem pode agir, com um limite que faz sentido.");
            yield return AwsL(2, "c5", "Infraestrutura como código",
                "Toda a infra do Café 365 está descrita no arquivo infra.yaml, que cria inclusive uma role do IAM.",
                "No CloudFormation, crie a pilha cafe365 a partir de infra.yaml (o modelo cria recursos IAM).",
                A("Console: CloudFormation → Criar pilha → modelo infra.yaml → nome cafe365 → marque o reconhecimento de IAM → Enviar", "aws cloudformation deploy --template-file infra.yaml --stack-name cafe365 --capabilities CAPABILITY_IAM"),
                A("O modelo cria uma role do IAM: é preciso reconhecer isso.", "Na CLI: --capabilities CAPABILITY_IAM."),
                A("Pilha (stack)", "Conjunto de recursos criados a partir de um modelo.", "Capacidades IAM", "Confirmação explícita de que o modelo pode criar permissões."),
                "Infraestrutura como código é versionada no Git, revisada e repetível: nada de 'cliquei e esqueci'.");
        }

        // ───────────────────────────── REDES ─────────────────────────────
        static IEnumerable<Level> Redes()
        {
            const string N = "redes";
            yield return C(N, 0, "cmd", "Está vivo?",
                "Um cliente diz que o site caiu. A Sandra pede o teste mais básico de rede.",
                "Teste se o servidor micorosoft.com responde.",
                A("ping micorosoft.com"),
                A("Envia pacotes ICMP e mede a resposta.", "p___ micorosoft.com"),
                A("ping", "Envia pacotes ICMP Echo e mede o tempo de resposta.", "micorosoft.com", "Destino."),
                "Alguns servidores bloqueiam ping: sem resposta não significa necessariamente fora do ar.");
            yield return C(N, 0, "cmd", "Meu endereço",
                "Para liberar seu acesso, o Beto precisa do IP da sua máquina.",
                "Mostre o endereço IP da sua máquina (Windows ou Linux).",
                A("ipconfig", "ip addr", "ip a", "ifconfig", "ipconfig /all"),
                A("No Windows: ipconfig. No Linux: ip addr.", "Qualquer um dos dois vale."),
                A("ipconfig / ip addr", "Mostra interfaces de rede, IPs, máscara e gateway."),
                "IPs 192.168.x.x, 10.x.x.x e 172.16-31.x.x são privados (rede interna).");
            yield return C(N, 0, "text", "A porta do cadeado",
                "O navegador mostra um cadeado no site. A Clara pergunta:",
                "Qual é a porta padrão do HTTPS? (responda só o número)",
                A("443"),
                A("HTTP usa 80; HTTPS usa outra.", "É 4__."),
                A("443", "Porta padrão do HTTPS (HTTP + TLS)."),
                "Outras portas famosas: 80 (HTTP), 22 (SSH), 53 (DNS), 3306 (MySQL), 5432 (PostgreSQL).");
            yield return C(N, 0, "text", "A agenda da internet",
                "Ninguém decora IPs: digitamos nomes como micorosoft.com.",
                "Qual protocolo traduz nomes de domínio em endereços IP? (sigla)",
                A("DNS"),
                A("Domain Name System.", "Três letras: D__"),
                A("DNS", "Domain Name System: a 'lista telefônica' da internet."),
                "Problemas de DNS causam muitos 'o site caiu!' que na verdade são erro de nome.");
            yield return C(N, 0, "cmd", "Qual o IP?",
                "Precisamos confirmar para qual IP micorosoft.com aponta.",
                "Consulte no DNS o IP de micorosoft.com.",
                A("nslookup micorosoft.com", "dig micorosoft.com", "host micorosoft.com"),
                A("Ferramentas de consulta DNS: nslookup ou dig.", "nslookup <domínio>"),
                A("nslookup", "Consulta servidores DNS.", "micorosoft.com", "Nome consultado."),
                "dig +short micorosoft.com mostra só o IP.");

            yield return C(N, 1, "cmd", "O caminho dos pacotes",
                "O site está lento só para clientes de uma região. Onde está o gargalo?",
                "Mostre a rota (saltos) até micorosoft.com.",
                A("tracert micorosoft.com", "traceroute micorosoft.com"),
                A("Windows: tracert. Linux: traceroute.", "trace..."),
                A("tracert / traceroute", "Lista cada roteador (salto) e o tempo até ele."),
                "Asteriscos (*) indicam saltos que não respondem.");
            yield return C(N, 1, "text", "Máscara /24",
                "A rede do escritório é 192.168.10.0/24.",
                "Qual é a máscara de sub-rede decimal de um /24?",
                A("255.255.255.0"),
                A("/24 = 24 bits ligados.", "255.___.___.0"),
                A("/24", "24 bits de rede e 8 de hosts.", "255.255.255.0", "Mesma coisa em decimal."),
                "/16 = 255.255.0.0 e /8 = 255.0.0.0.");
            yield return C(N, 1, "text", "Porta do acesso remoto",
                "Você vai acessar um servidor Linux remotamente, com segurança.",
                "Qual é a porta padrão do SSH? (só o número)",
                A("22"),
                A("Telnet é 23; o SSH vem logo antes.", "É um número de dois dígitos."),
                A("22", "Porta padrão do SSH (shell remoto criptografado)."),
                "Mudar a porta não substitui boas práticas: use chaves e desative login de root.");
            yield return C(N, 1, "cmd", "Entrando no servidor",
                "O servidor 10.0.0.5 precisa de manutenção.",
                "Conecte-se por SSH ao 10.0.0.5 com o usuário admin.",
                A("ssh admin@10.0.0.5"),
                A("Formato: ssh usuario@host", "ssh admin@..."),
                A("ssh", "Shell remoto seguro.", "admin@10.0.0.5", "Usuário e servidor."),
                "Use -i chave.pem para escolher a chave e -p para outra porta.");
            yield return C(N, 1, "cmd", "Portas abertas",
                "A Clara suspeita de um serviço estranho escutando no servidor.",
                "Liste as conexões e portas em escuta (netstat ou ss).",
                A("netstat -an", "netstat -ano", "ss -tuln", "netstat -tuln", "ss -tulpn"),
                A("netstat existe em Windows e Linux; ss é o moderno no Linux.", "netstat -an"),
                A("netstat -an / ss -tuln", "Mostra conexões e portas em LISTEN."),
                "Feche portas que não precisam ficar abertas: menos superfície de ataque.");

            yield return C(N, 2, "text", "Hosts em /26",
                "Vamos criar uma sub-rede para as impressoras: 192.168.10.64/26.",
                "Quantos hosts utilizáveis cabem em uma rede /26?",
                A("62"),
                A("/26 deixa 6 bits para hosts: 2^6.", "2^6 - 2 (rede e broadcast)."),
                A("/26", "6 bits de host = 64 endereços.", "62", "64 menos o endereço de rede e o de broadcast."),
                "Fórmula: hosts = 2^(32 - prefixo) - 2.");
            yield return C(N, 2, "cmd", "Só os cabeçalhos",
                "Precisamos ver o código HTTP e os headers de segurança do site.",
                "Mostre apenas os cabeçalhos da resposta de https://micorosoft.com com curl.",
                A("curl -I https://micorosoft.com", "curl --head https://micorosoft.com"),
                A("curl tem uma opção para requisição HEAD.", "curl -I <url>"),
                A("curl -I", "Faz um HEAD e mostra só os cabeçalhos.", "https://micorosoft.com", "Endereço."),
                "Procure Strict-Transport-Security e Content-Security-Policy.");
            yield return C(N, 2, "text", "Camada do roteador",
                "Pergunta clássica de entrevista:",
                "Em qual camada do modelo OSI atua o roteador? (número)",
                A("3", "camada 3", "rede", "camada de rede"),
                A("Roteadores trabalham com endereços IP.", "Camada de Rede."),
                A("Camada 3 (Rede)", "Onde ficam o IP e o roteamento."),
                "Switches comuns: camada 2 (enlace, MAC). Balanceadores HTTP: camada 7.");
            yield return C(N, 2, "cmd", "Liberando o HTTPS",
                "O servidor novo bloqueia tudo. Libere só o HTTPS.",
                "Libere a porta 443/tcp no firewall UFW (Linux).",
                A("sudo ufw allow 443/tcp", "ufw allow 443/tcp", "sudo ufw allow 443", "ufw allow 443", "sudo ufw allow https"),
                A("UFW = Uncomplicated Firewall.", "ufw allow <porta>/tcp"),
                A("ufw allow", "Cria uma regra de liberação.", "443/tcp", "Porta e protocolo."),
                "Regra de ouro: bloqueie tudo e libere só o necessário.");
            yield return C(N, 2, "text", "Faixa privada classe A",
                "A nova VPC terá muitos endereços internos.",
                "Qual faixa privada (CIDR) começa em 10? Ex.: x.x.x.x/y",
                A("10.0.0.0/8"),
                A("A faixa inteira do 10.", "10.0.0.0/_"),
                A("10.0.0.0/8", "Faixa privada (RFC 1918) com cerca de 16 milhões de endereços."),
                "As outras são 172.16.0.0/12 e 192.168.0.0/16.");
        }

        // ───────────────────────────── PYTHON ─────────────────────────────
        static IEnumerable<Level> Python()
        {
            const string P = "python";
            yield return C(P, 0, "code", "Primeiro print",
                "A Yara, do Lab de Dados, quer ver seu primeiro script rodando.",
                "Mostre na tela: Olá, Micorosoft!",
                A("print(\"Olá, Micorosoft!\")"),
                A("A função de saída é print.", "print(\"...\")"),
                A("print(...)", "Escreve na saída padrão."),
                "Em Python 3 o print é uma função, por isso os parênteses.");
            yield return C(P, 0, "code", "Lista de vendas",
                "As vendas da manhã foram 10, 20 e 30 cafés.",
                "Crie a variável vendas com a lista [10, 20, 30].",
                A("vendas = [10, 20, 30]"),
                A("Listas usam colchetes.", "nome = [a, b, c]"),
                A("vendas", "Nome da variável.", "[10, 20, 30]", "Lista com três números."),
                "Listas são mutáveis; tuplas (10, 20, 30) não.");
            yield return C(P, 0, "code", "Total do dia",
                "Quanto vendemos na manhã?",
                "Guarde em total a soma da lista vendas.",
                A("total = sum(vendas)"),
                A("Existe uma função nativa de soma.", "total = s__(vendas)"),
                A("sum(vendas)", "Soma os itens da lista."),
                "Também existem len(), max() e min().");
            yield return C(P, 0, "cmd", "Instalando o pandas",
                "Para analisar planilhas de vendas, todo mundo aqui usa pandas.",
                "Instale o pacote pandas pelo terminal.",
                A("pip install pandas", "pip3 install pandas", "python -m pip install pandas", "python3 -m pip install pandas"),
                A("O instalador de pacotes do Python é o pip.", "pip install <pacote>"),
                A("pip install pandas", "Baixa o pacote do PyPI."),
                "Use requirements.txt e pip install -r requirements.txt para fixar versões.");
            yield return C(P, 0, "code", "Pergunte o nome",
                "O script de boas-vindas vai perguntar o nome do usuário.",
                "Leia uma entrada do teclado (sem mensagem) e guarde em nome.",
                A("nome = input()"),
                A("A função de entrada é input.", "nome = input()"),
                A("input()", "Lê uma linha digitada (sempre como texto)."),
                "Para número: int(input()).");

            yield return C(P, 1, "code", "Função dobro",
                "Recebemos o dobro de pedidos na promoção. Crie uma função para calcular.",
                "Crie em uma linha a função dobro(n) que retorna n * 2.",
                A("def dobro(n): return n * 2"),
                A("Funções começam com def.", "def nome(param): return ..."),
                A("def dobro(n):", "Declara a função.", "return n * 2", "Valor devolvido."),
                "Em funções maiores, o corpo vai nas linhas de baixo, indentado.");
            yield return C(P, 1, "code", "Quadrados",
                "O relatório precisa dos quadrados de 0 a 4.",
                "Crie quadrados com uma list comprehension de x**2 para x em range(5).",
                A("quadrados = [x**2 for x in range(5)]", "quadrados = [x * x for x in range(5)]", "quadrados = [i**2 for i in range(5)]"),
                A("[expressão for item in sequência]", "range(5) gera 0 a 4."),
                A("[x**2 for x in range(5)]", "Cria a lista [0, 1, 4, 9, 16]."),
                "Dá para filtrar: [x for x in nums if x > 0].");
            yield return C(P, 1, "code", "Abrindo arquivo",
                "As vendas estão no arquivo vendas.csv.",
                "Abra vendas.csv para leitura com with ... as f: (só a primeira linha do bloco).",
                A("with open(\"vendas.csv\") as f:", "with open(\"vendas.csv\", \"r\") as f:"),
                A("O with fecha o arquivo automaticamente.", "with open(\"arquivo\") as f:"),
                A("with open(...) as f:", "Abre e garante o fechamento do arquivo."),
                "Sempre prefira with a open/close manual.");
            yield return C(P, 1, "code", "CSV no pandas",
                "Bora analisar o CSV direito.",
                "Leia vendas.csv com pandas (importado como pd) e guarde em df.",
                A("df = pd.read_csv(\"vendas.csv\")"),
                A("O pandas tem read_csv.", "df = pd.read_csv(...)"),
                A("pd.read_csv", "Lê o CSV num DataFrame (tabela)."),
                "df.head() mostra as primeiras linhas.");
            yield return C(P, 1, "code", "Produto em dicionário",
                "Cada produto tem nome e preço.",
                "Crie o dicionário produto com \"nome\": \"Café\" e \"preco\": 9.9.",
                A("produto = {\"nome\": \"Café\", \"preco\": 9.9}", "produto = {\"preco\": 9.9, \"nome\": \"Café\"}"),
                A("Dicionários usam chaves e pares chave: valor.", "{\"chave\": valor, ...}"),
                A("{ }", "Dicionário.", "\"nome\": \"Café\"", "Par chave: valor."),
                "Leia com produto[\"nome\"] ou produto.get(\"nome\").");

            yield return C(P, 2, "code", "Tratando erro",
                "Alguém digitou 'abc' no campo de quantidade e o sistema quebrou.",
                "Escreva o except para ValueError imprimindo: Valor inválido (formato: except Erro: print(...)).",
                A("except ValueError: print(\"Valor inválido\")"),
                A("Erros específicos são capturados por nome.", "except ValueError: ..."),
                A("except ValueError:", "Captura só esse tipo de erro.", "print(\"Valor inválido\")", "O que fazer."),
                "Nunca use except: vazio — ele esconde bugs.");
            yield return C(P, 2, "cmd", "Ambiente virtual",
                "Cada projeto precisa das próprias versões de pacotes.",
                "Crie um ambiente virtual chamado venv.",
                A("python -m venv venv", "python3 -m venv venv", "py -m venv venv"),
                A("O módulo venv cria ambientes.", "python -m venv <nome>"),
                A("python -m venv", "Cria o ambiente isolado.", "venv", "Pasta do ambiente."),
                "Ative com venv\\Scripts\\activate (Windows) ou source venv/bin/activate (Linux).");
            yield return C(P, 2, "code", "Vendas por categoria",
                "A diretoria quer o total vendido por categoria.",
                "Some a coluna valor agrupando df por categoria.",
                A("df.groupby(\"categoria\")[\"valor\"].sum()"),
                A("groupby + coluna + agregação.", "df.groupby(\"...\")[\"...\"].sum()"),
                A("groupby(\"categoria\")", "Agrupa.", "[\"valor\"].sum()", "Soma a coluna em cada grupo."),
                "Outras agregações: mean(), count(), agg([...]).");
            yield return C(P, 2, "code", "Rota no Flask",
                "A API interna de relatórios usa Flask.",
                "Escreva o decorator da rota / no app Flask.",
                A("@app.route(\"/\")", "@app.get(\"/\")"),
                A("Rotas são decorators.", "@app.route(\"/\")"),
                A("@app.route(\"/\")", "Liga a URL / à função abaixo."),
                "Para JSON, retorne um dicionário ou use jsonify.");
            yield return C(P, 2, "code", "Construtor",
                "Vamos modelar produtos com classes.",
                "Escreva a linha do construtor da classe Produto que recebe nome.",
                A("def __init__(self, nome):"),
                A("O construtor se chama __init__.", "def __init__(self, ...):"),
                A("__init__", "Executado ao criar o objeto.", "self", "Referência ao próprio objeto."),
                "Dentro dele: self.nome = nome.");
        }

        // ───────────────────────────── PHP ─────────────────────────────
        static IEnumerable<Level> Php()
        {
            const string H = "php";
            yield return C(H, 0, "code", "Olá em PHP",
                "Boa parte do site legado da Micorosoft é PHP. O Igor quer ver seu Olá.",
                "Mostre: Olá, Micorosoft! usando echo.",
                A("echo \"Olá, Micorosoft!\";", "print \"Olá, Micorosoft!\";"),
                A("A saída mais comum em PHP é echo.", "echo \"...\";"),
                A("echo", "Envia texto para a saída (a página).", ";", "Toda instrução PHP termina com ponto e vírgula."),
                "Código PHP fica entre <?php e ?>.");
            yield return C(H, 0, "code", "Variável de preço",
                "O preço do café precisa de uma variável.",
                "Crie a variável $preco com 9.90.",
                A("$preco = 9.90;", "$preco = 9.9;"),
                A("Variáveis em PHP começam com $.", "$nome = valor;"),
                A("$preco", "Variável.", "9.90", "Valor decimal."),
                "PHP tem tipagem dinâmica, mas aceita declarações de tipo em funções.");
            yield return C(H, 0, "code", "Array de itens",
                "O pedido tem café e pão.",
                "Crie o array $itens com \"cafe\" e \"pao\".",
                A("$itens = [\"cafe\", \"pao\"];", "$itens = array(\"cafe\", \"pao\");"),
                A("Arrays usam colchetes (ou array()).", "$itens = [..., ...];"),
                A("[\"cafe\", \"pao\"]", "Array indexado (posições 0 e 1)."),
                "Arrays associativos: [\"nome\" => \"Café\"].");
            yield return C(H, 0, "cmd", "Servidor embutido",
                "Para testar rápido, sem Apache.",
                "Suba o servidor embutido do PHP em localhost:8000.",
                A("php -S localhost:8000", "php -S 127.0.0.1:8000"),
                A("O próprio php tem um servidor de testes.", "php -S host:porta"),
                A("php -S", "Servidor web de desenvolvimento.", "localhost:8000", "Endereço e porta."),
                "Nunca use o servidor embutido em produção.");
            yield return C(H, 0, "code", "Quantos itens?",
                "O carrinho precisa mostrar a quantidade de itens.",
                "Mostre com echo a quantidade de itens de $itens.",
                A("echo count($itens);"),
                A("A função que conta itens é count.", "echo count(...);"),
                A("count($itens)", "Número de elementos do array."),
                "strlen() conta caracteres de uma string.");

            yield return C(H, 1, "code", "Função dobro",
                "Promoção: dobro de pontos de fidelidade!",
                "Crie a função dobro que recebe $n e retorna $n * 2 (em uma linha).",
                A("function dobro($n) { return $n * 2; }"),
                A("Funções começam com function.", "function nome($x) { return ...; }"),
                A("function dobro($n)", "Declara a função.", "return $n * 2;", "Devolve o resultado."),
                "Com tipos: function dobro(int $n): int { ... }");
            yield return C(H, 1, "code", "Percorrendo itens",
                "Hora de listar todos os itens do pedido.",
                "Percorra $itens com foreach exibindo cada $item (em uma linha).",
                A("foreach ($itens as $item) { echo $item; }"),
                A("foreach (array as item) { ... }", "foreach ($itens as $item) { echo ...; }"),
                A("foreach", "Repete para cada elemento.", "$itens as $item", "Array e variável de cada volta."),
                "Com chaves: foreach ($arr as $chave => $valor).");
            yield return C(H, 1, "code", "Dados do formulário",
                "O formulário de cadastro envia o campo email por POST.",
                "Guarde em $email o campo email recebido via POST.",
                A("$email = $_POST[\"email\"];"),
                A("Dados de POST ficam numa superglobal.", "$_POST[\"campo\"]"),
                A("$_POST", "Array com os dados enviados por POST.", "[\"email\"]", "Nome do campo."),
                "SEMPRE valide e escape: filter_var($email, FILTER_VALIDATE_EMAIL).");
            yield return C(H, 1, "cmd", "Dependências",
                "Você clonou a API de pedidos em PHP.",
                "Instale as dependências do composer.json.",
                A("composer install"),
                A("O gerenciador de pacotes do PHP é o Composer.", "composer _______"),
                A("composer install", "Instala as versões do composer.lock na pasta vendor/."),
                "composer require <pacote> adiciona pacotes novos.");
            yield return C(H, 1, "code", "Resposta em JSON",
                "O app Flutter vai consumir essa API: ela precisa responder JSON.",
                "Mostre o array $dados convertido em JSON.",
                A("echo json_encode($dados);"),
                A("A função converte para JSON: json_...", "echo json_encode(...);"),
                A("json_encode($dados)", "Converte array/objeto em texto JSON."),
                "Envie também o header: header('Content-Type: application/json');");

            yield return C(H, 2, "code", "Conectando no banco",
                "A API precisa do MySQL do banco cafe.",
                "Crie $pdo com new PDO(\"mysql:host=localhost;dbname=cafe\", \"root\", \"\").",
                A("$pdo = new PDO(\"mysql:host=localhost;dbname=cafe\", \"root\", \"\");"),
                A("PDO recebe DSN, usuário e senha.", "new PDO(\"mysql:host=...;dbname=...\", user, senha)"),
                A("new PDO(...)", "Conexão com o banco.", "mysql:host=localhost;dbname=cafe", "DSN: driver, host e banco."),
                "Em produção nunca use root sem senha; guarde credenciais em variáveis de ambiente.");
            yield return C(H, 2, "code", "Consulta preparada",
                "Montar SQL concatenando texto abre brecha para SQL Injection!",
                "Prepare SELECT * FROM pedidos WHERE id = ? em $stmt usando $pdo.",
                A("$stmt = $pdo->prepare(\"SELECT * FROM pedidos WHERE id = ?\");"),
                A("prepared statements: ->prepare(...)", "O ? é um marcador de parâmetro."),
                A("->prepare(...)", "Prepara a consulta.", "?", "Valor enviado depois, com segurança."),
                "Prepared statements são a principal defesa contra SQL Injection.");
            yield return C(H, 2, "code", "Executando",
                "Consulta preparada. Agora execute com o id do pedido.",
                "Execute $stmt passando $id num array.",
                A("$stmt->execute([$id]);"),
                A("execute recebe um array de parâmetros.", "$stmt->execute([...]);"),
                A("->execute([$id])", "Executa trocando o ? por $id."),
                "Depois: $stmt->fetch() ou fetchAll().");
            yield return C(H, 2, "code", "Senha segura",
                "Descobrimos senhas salvas em texto puro no sistema antigo! Isso não pode.",
                "Gere em $hash o hash seguro de $senha com password_hash e PASSWORD_DEFAULT.",
                A("$hash = password_hash($senha, PASSWORD_DEFAULT);"),
                A("A função nativa é password_hash.", "password_hash($senha, PASSWORD_DEFAULT)"),
                A("password_hash", "Gera um hash com sal (bcrypt por padrão).", "PASSWORD_DEFAULT", "Algoritmo recomendado."),
                "Para conferir: password_verify($senha, $hash). Nunca use md5 para senhas!");
            yield return C(H, 2, "cmd", "Migrations",
                "O backend novo usa Laravel e o banco precisa das tabelas.",
                "Rode as migrations do Laravel.",
                A("php artisan migrate"),
                A("A CLI do Laravel é o artisan.", "php artisan _______"),
                A("php artisan migrate", "Cria/atualiza as tabelas a partir das migrations."),
                "migrate:rollback desfaz o último lote.");
        }
        // ───────────────────────────── SQL (terminal mysql com banco de verdade) ─────────────────────────────
        // O terminal executa o comando num banco simulado (cafe365). A tarefa é avaliada pelo RESULTADO:
        //   query  → o resultado do SELECT precisa bater com o da consulta de referência (qualquer consulta equivalente vale)
        //   state  → as tabelas precisam ficar como ficariam com o comando de referência
        //   schema → a estrutura (tabela/índice) é conferida por uma função
        static Level SqlL(int tier, string id, string mode, string title, string ctx, string obj, string answer, string[] hints, string[] explain, string summary,
                          bool ordered = false, string[] tables = null, Func<SqlDb, string> check = null)
        {
            var l = C("sql", tier, "sql", title, ctx, obj, A(answer), hints, explain, summary);
            l.Task = id;
            SqlTasks.All[id] = new SqlTask { Id = id, Expected = answer, Mode = mode, Ordered = ordered, Tables = tables ?? new string[0], Check = check };
            return l;
        }

        static IEnumerable<Level> Sql()
        {
            yield return SqlL(0, "s1", "query", "Conhecendo o cardápio",
                "A Glória, DBA da Micorosoft, te deu acesso ao banco cafe365. Primeiro passo de todo DBA: olhar os dados.",
                "Mostre TODAS as colunas de todos os produtos da tabela produtos.",
                "SELECT * FROM produtos;",
                A("SELECT escolhe as colunas e FROM diz de qual tabela. O asterisco (*) significa 'todas as colunas'.", "SELECT * FROM ________;"),
                A("SELECT *", "Seleciona todas as colunas.", "FROM produtos", "Da tabela produtos.", ";", "Termina o comando (no terminal mysql de verdade, é obrigatório)."),
                "Em sistemas reais evite SELECT * em tabelas enormes: peça só as colunas que você precisa.");
            yield return SqlL(0, "s2", "query", "Só o que interessa",
                "O app do Café 365 só precisa do nome e do preço para montar o cardápio.",
                "Liste apenas o nome e o preço (colunas nome e preco) de todos os produtos.",
                "SELECT nome, preco FROM produtos;",
                A("Troque o * pelos nomes das colunas.", "SELECT coluna1, coluna2 FROM tabela;"),
                A("SELECT nome, preco", "Só essas duas colunas, separadas por vírgula.", "FROM produtos", "Da tabela produtos."),
                "Não sabe o nome das colunas? DESCRIBE produtos; mostra a estrutura da tabela.");
            yield return SqlL(0, "s3", "query", "Só bebidas",
                "O cardápio de bebidas vai ganhar uma tela própria no app.",
                "Liste o nome e o preço dos produtos da categoria 'bebida'.",
                "SELECT nome, preco FROM produtos WHERE categoria = 'bebida';",
                A("Para filtrar linhas, use WHERE depois do FROM.", "... WHERE categoria = 'bebida'  (texto vai entre aspas simples)"),
                A("WHERE", "Filtra as linhas: só ficam as que satisfazem a condição.", "categoria = 'bebida'", "Comparação com um texto (entre aspas simples)."),
                "No MySQL, comparações de texto normalmente ignoram maiúsculas: 'Bebida' = 'bebida'.");
            yield return SqlL(0, "s4", "query", "Do mais caro ao mais barato",
                "A Glória quer conferir os preços, começando pelos mais caros.",
                "Liste o nome e o preço de todos os produtos, do mais caro para o mais barato.",
                "SELECT nome, preco FROM produtos ORDER BY preco DESC;",
                A("Quem ordena o resultado é o ORDER BY.", "ORDER BY coluna ASC (crescente) ou DESC (decrescente)."),
                A("ORDER BY preco", "Ordena pelo preço.", "DESC", "Em ordem decrescente (do maior para o menor). O padrão, ASC, é crescente."),
                "Sem ORDER BY, o banco não garante nenhuma ordem: nunca dependa da 'ordem natural'.", true);
            yield return SqlL(0, "s5", "query", "Quantos clientes?",
                "O marketing perguntou quantos clientes o Café 365 já tem.",
                "Conte quantos clientes existem na tabela clientes.",
                "SELECT COUNT(*) FROM clientes;",
                A("Existe uma função que conta linhas.", "SELECT COUNT(*) FROM ...;"),
                A("COUNT(*)", "Conta as linhas do resultado.", "FROM clientes", "Da tabela clientes."),
                "COUNT(coluna) conta só os valores que não são NULL. COUNT(*) conta todas as linhas.");

            yield return SqlL(1, "s6", "query", "Clientes que começam com Ma",
                "O atendimento procura uma cliente, mas só lembra que o nome começa com 'Ma'.",
                "Liste o nome dos clientes cujo nome começa com 'Ma'.",
                "SELECT nome FROM clientes WHERE nome LIKE 'Ma%';",
                A("Para buscar por padrão de texto, use LIKE.", "% significa 'qualquer coisa': LIKE 'Ma%'"),
                A("LIKE 'Ma%'", "Começa com Ma e depois qualquer coisa.", "%", "Coringa: zero ou mais caracteres. O _ vale exatamente um caractere."),
                "LIKE '%café%' procura em qualquer posição — mas é lento em tabelas grandes, porque não usa índice.");
            yield return SqlL(1, "s7", "state", "Produto novo no cardápio",
                "Chegou um produto novo e ele precisa entrar no banco hoje.",
                "Cadastre na tabela produtos o 'Chá gelado', categoria 'bebida', preço 7.50 e estoque 40 (o id é automático).",
                "INSERT INTO produtos (nome, categoria, preco, estoque) VALUES ('Chá gelado', 'bebida', 7.50, 40);",
                A("Para inserir uma linha, use INSERT INTO ... VALUES.", "INSERT INTO tabela (col1, col2, ...) VALUES (valor1, valor2, ...);"),
                A("INSERT INTO produtos (...)", "Tabela e colunas que vão receber valores (o id fica de fora: é AUTO_INCREMENT).", "VALUES ('Chá gelado', 'bebida', 7.50, 40)",
                  "Os valores, na mesma ordem das colunas. Texto entre aspas; número sem aspas e com ponto decimal."),
                "Listar as colunas no INSERT protege seu código: ele continua funcionando se alguém adicionar colunas na tabela.", false, A("produtos"));
            yield return SqlL(1, "s8", "state", "Reajuste do pão de queijo",
                "O preço do pão de queijo subiu. Cuidado: só ele pode mudar!",
                "Altere o preço do produto 'Pão de queijo' para 5.50, sem mexer nos outros produtos.",
                "UPDATE produtos SET preco = 5.50 WHERE nome = 'Pão de queijo';",
                A("Para alterar dados existentes, use UPDATE ... SET ... WHERE.", "UPDATE tabela SET coluna = valor WHERE condição;"),
                A("UPDATE produtos", "Tabela que vai ser alterada.", "SET preco = 5.50", "Novo valor da coluna.", "WHERE nome = 'Pão de queijo'",
                  "Quais linhas mudam. SEM WHERE, todas as linhas da tabela seriam alteradas!"),
                "Dica de DBA: antes de um UPDATE, rode um SELECT com o mesmo WHERE para conferir quais linhas vão mudar.", false, A("produtos"));
            yield return SqlL(1, "s9", "state", "Limpando pedidos cancelados",
                "Os pedidos cancelados estão poluindo os relatórios. A Glória autorizou a limpeza.",
                "Apague da tabela pedidos todos os pedidos com status 'cancelado'.",
                "DELETE FROM pedidos WHERE status = 'cancelado';",
                A("Para apagar linhas, use DELETE FROM ... WHERE.", "DELETE FROM tabela WHERE condição;"),
                A("DELETE FROM pedidos", "Apaga linhas da tabela pedidos.", "WHERE status = 'cancelado'", "Só as linhas que satisfazem a condição. Sem WHERE, apagaria TUDO."),
                "Em muitas empresas nada é apagado de verdade: usa-se 'exclusão lógica' (uma coluna ativo = 0) para manter o histórico.", false, A("pedidos"));
            yield return SqlL(1, "s10", "query", "Produtos por categoria",
                "O financeiro quer saber quantos produtos existem em cada categoria.",
                "Mostre cada categoria e quantos produtos ela tem.",
                "SELECT categoria, COUNT(*) FROM produtos GROUP BY categoria;",
                A("Para contar por grupo, junte COUNT(*) com GROUP BY.", "SELECT coluna, COUNT(*) FROM tabela GROUP BY coluna;"),
                A("GROUP BY categoria", "Junta as linhas que têm a mesma categoria num grupo só.", "COUNT(*)", "Conta as linhas de cada grupo."),
                "Toda coluna do SELECT que não está dentro de COUNT/SUM/AVG/MIN/MAX precisa estar no GROUP BY.");

            yield return SqlL(2, "s11", "query", "Pedidos com nome do cliente",
                "O relatório de vendas mostra só o cliente_id, e ninguém sabe quem é quem.",
                "Liste o nome do cliente e o total de cada pedido, juntando as tabelas pedidos e clientes.",
                "SELECT c.nome, p.total FROM pedidos p JOIN clientes c ON p.cliente_id = c.id;",
                A("Para juntar duas tabelas, use JOIN ... ON com a coluna que liga uma à outra.", "pedidos.cliente_id aponta para clientes.id"),
                A("FROM pedidos p", "Tabela principal, com o apelido p.", "JOIN clientes c", "Junta com a tabela clientes (apelido c).", "ON p.cliente_id = c.id",
                  "A condição que liga cada pedido ao seu cliente (chave estrangeira → chave primária)."),
                "JOIN (ou INNER JOIN) só traz linhas que têm par nas duas tabelas.");
            yield return SqlL(2, "s12", "query", "Clientes sem pedidos",
                "O marketing quer mandar um cupom para quem se cadastrou mas nunca comprou.",
                "Liste o nome dos clientes que nunca fizeram nenhum pedido.",
                "SELECT c.nome FROM clientes c LEFT JOIN pedidos p ON p.cliente_id = c.id WHERE p.id IS NULL;",
                A("LEFT JOIN mantém todos os clientes, mesmo os sem pedido (as colunas do pedido vêm NULL).", "... LEFT JOIN pedidos p ON ... WHERE p.id IS NULL"),
                A("LEFT JOIN pedidos p", "Mantém TODOS os clientes, com ou sem pedido.", "WHERE p.id IS NULL", "Fica só com quem não achou nenhum pedido (NULL se testa com IS NULL, nunca com = NULL)."),
                "Outra forma: WHERE id NOT IN (SELECT cliente_id FROM pedidos). O resultado é o mesmo.");
            yield return SqlL(2, "s13", "query", "Clientes fiéis",
                "Clientes que voltam merecem um programa de fidelidade.",
                "Mostre o cliente_id e a quantidade de pedidos dos clientes que têm MAIS DE UM pedido.",
                "SELECT cliente_id, COUNT(*) FROM pedidos GROUP BY cliente_id HAVING COUNT(*) > 1;",
                A("Agrupe por cliente e filtre os grupos. WHERE filtra linhas; quem filtra grupos é o HAVING.", "... GROUP BY cliente_id HAVING COUNT(*) > 1;"),
                A("GROUP BY cliente_id", "Um grupo por cliente.", "HAVING COUNT(*) > 1", "Filtra os GRUPOS depois de agrupar (o WHERE não enxerga o COUNT)."),
                "Ordem de execução: FROM → WHERE → GROUP BY → HAVING → SELECT → ORDER BY → LIMIT.");
            yield return SqlL(2, "s14", "schema", "Tabela de avaliações",
                "O app vai deixar os clientes darem nota aos produtos. O banco precisa de uma tabela nova.",
                "Crie a tabela avaliacoes com as colunas id (INT, chave primária, auto incremento), produto_id (INT) e nota (INT).",
                "CREATE TABLE avaliacoes (id INT PRIMARY KEY AUTO_INCREMENT, produto_id INT, nota INT);",
                A("Use CREATE TABLE nome (coluna TIPO, ...).", "id INT PRIMARY KEY AUTO_INCREMENT, produto_id INT, nota INT"),
                A("CREATE TABLE avaliacoes (...)", "Cria a tabela com as colunas entre parênteses.", "id INT PRIMARY KEY AUTO_INCREMENT", "Chave primária numerada automaticamente.",
                  "produto_id INT, nota INT", "Colunas de números inteiros."),
                "Num projeto real, produto_id ganharia uma FOREIGN KEY (produto_id) REFERENCES produtos(id) para impedir notas de produtos inexistentes.",
                false, null, CheckAvaliacoes);
            yield return SqlL(2, "s15", "schema", "Índice para acelerar",
                "O cardápio do app filtra os produtos por categoria e, com milhares de itens, toda busca varre a tabela produtos inteira.",
                "Crie um índice chamado idx_produtos_categoria na coluna categoria da tabela produtos.",
                "CREATE INDEX idx_produtos_categoria ON produtos (categoria);",
                A("Índices são criados com CREATE INDEX nome ON tabela (coluna);", "CREATE INDEX idx_produtos_categoria ON ..."),
                A("CREATE INDEX idx_produtos_categoria", "Cria um índice com esse nome.", "ON produtos (categoria)", "Na coluna categoria da tabela produtos."),
                "Compare antes e depois com EXPLAIN SELECT * FROM produtos WHERE categoria = 'doce'; — type ALL varre tudo, type ref + key mostra o índice. " +
                "Índices aceleram leituras, mas deixam escritas um pouco mais lentas. (Chaves primárias e estrangeiras já ganham índice automaticamente.)",
                false, null, CheckIndice);
        }

        static string CheckAvaliacoes(SqlDb db)
        {
            SqlTable t;
            if (!db.Tables.TryGetValue("avaliacoes", out t)) return "A tabela avaliacoes não foi criada (confira o nome: avaliacoes, sem acento).";
            var faltam = new[] { "id", "produto_id", "nota" }.Where(c => t.ColIndex(c) < 0).ToList();
            if (faltam.Count > 0) return "Faltou a coluna " + string.Join(", ", faltam) + " na tabela avaliacoes.";
            if (t.Cols.Count != 3) return "A tabela avaliacoes deve ter só as colunas id, produto_id e nota.";
            if (t.Cols.Any(c => !c.IsInt)) return "As três colunas devem ser do tipo INT.";
            var id = t.Cols[t.ColIndex("id")];
            if (!id.Pk) return "A coluna id precisa ser a chave primária (PRIMARY KEY).";
            if (!id.Auto) return "A coluna id precisa ser AUTO_INCREMENT (numerada automaticamente).";
            return null;
        }

        static string CheckIndice(SqlDb db)
        {
            var t = db.Table("produtos");
            var ix = t.Indexes.FirstOrDefault(i => i.Name.Equals("idx_produtos_categoria", StringComparison.OrdinalIgnoreCase));
            if (ix == null)
                return db.Tables.Values.Any(o => o.Indexes.Count > 0) ? "Você criou um índice, mas o pedido é idx_produtos_categoria, na tabela produtos." : "O índice idx_produtos_categoria não foi criado na tabela produtos.";
            if (!ix.Cols[0].Equals("categoria", StringComparison.OrdinalIgnoreCase)) return "O índice idx_produtos_categoria precisa ser na coluna categoria.";
            return null;
        }
    }
}
