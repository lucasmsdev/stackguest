using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerQuest
{
    static partial class Challenges
    {
        // Expande trechos «a|b|c» em todas as combinações. A primeira opção de cada grupo
        // forma a resposta de referência (a primeira da lista).
        static string[] ReactVar(params string[] modelos)
        {
            var saida = new List<string>();
            foreach (var m in modelos) ReactExpandir(m, saida);
            return saida.Distinct().ToArray();
        }

        static void ReactExpandir(string m, List<string> saida)
        {
            int a = m.IndexOf('«');
            if (a < 0) { saida.Add(m); return; }
            // grupos podem ser aninhados: acha o » correspondente e separa as opções só no nível 1
            var ops = new List<string>();
            int depth = 0, b = -1, start = a + 1;
            for (int i = a; i < m.Length; i++)
            {
                char c = m[i];
                if (c == '«') depth++;
                else if (c == '»') { depth--; if (depth == 0) { ops.Add(m.Substring(start, i - start)); b = i; break; } }
                else if (c == '|' && depth == 1) { ops.Add(m.Substring(start, i - start)); start = i + 1; }
            }
            if (b < 0) throw new Exception("ReactVar: grupo sem fechamento em " + m);
            string pre = m.Substring(0, a), pos = m.Substring(b + 1);
            foreach (var op in ops) ReactExpandir(pre + op + pos, saida);
        }

        static void ReactNivel(List<Level> list, string title, int tier)
        {
            var lv = list.First(l => l.Cat == "react" && l.Title == title);
            lv.Tier = tier;
            lv.Chapter = Cat("react").Name + " · " + TierNames[tier];
        }

        static partial void MaisReact(List<Level> list)
        {
            const string R = "react";

            // 1) rebalanceamento dos desafios existentes
            ReactNivel(list, "Só para quem está logado", 0);   // um conceito só (&&)
            ReactNivel(list, "Passando props", 0);             // um conceito só (prop numérica)
            ReactNivel(list, "Build de produção", 0);          // um comando simples
            ReactNivel(list, "Carregar ao montar", 1);
            ReactNivel(list, "Contexto de tema", 1);
            ReactNivel(list, "Lendo o contexto", 1);

            // ───────────── INICIANTE ─────────────
            list.Add(C(R, 0, "code", "Loja aberta ou fechada",
                "No topo do painel, a Mari quer mostrar se a cafeteria está funcionando agora. A variável aberta vale true ou false.",
                "Escreva um parágrafo <p> que mostra o texto Aberta quando aberta for verdadeira e Fechada quando for falsa, usando o operador ternário dentro das chaves.",
                ReactVar("<p>{aberta ? \"Aberta\" : \"Fechada\"}</p>",
                         "<p>{aberta === true ? \"Aberta\" : \"Fechada\"}</p>",
                         "{aberta ? <p>Aberta</p> : <p>Fechada</p>}"),
                A("O ternário escolhe entre dois valores: condição ? seVerdadeiro : seFalso", "<p>{aberta ? \"...\" : \"...\"}</p>"),
                A("<p>{ ... }</p>", "As chaves abrem espaço para uma expressão JavaScript dentro do JSX.",
                  "aberta ?", "Condição testada.",
                  "\"Aberta\" : \"Fechada\"", "Valor usado se for verdadeira : valor usado se for falsa."),
                "Use && quando só existe o caso verdadeiro; use o ternário quando há duas opções (isto OU aquilo)."));

            list.Add(C(R, 0, "code", "Primeiro componente",
                "O Thiago explica: em React, tudo é componente — uma função que devolve JSX. O painel precisa de um rodapé.",
                "Crie o componente de função Rodape que retorna um <footer> com o texto Squad React.",
                ReactVar("«|export default |export »function Rodape() {\n  return «<footer>Squad React</footer>|(<footer>Squad React</footer>)»\n}",
                         "const Rodape = () => «<footer>Squad React</footer>|(<footer>Squad React</footer>)»",
                         "const Rodape = () => {\n  return «<footer>Squad React</footer>|(<footer>Squad React</footer>)»\n}"),
                A("Um componente é uma função com nome em MAIÚSCULA que retorna JSX.", "function Rodape() { return <footer>...</footer> }"),
                A("function Rodape()", "Componente de função (o nome começa com letra maiúscula).",
                  "return <footer>Squad React</footer>", "O JSX devolvido é o que aparece na tela."),
                "Para usar o componente em outro arquivo: export default Rodape e, lá, import Rodape from \"./Rodape\"."));

            // ───────────── INTERMEDIÁRIO ─────────────
            list.Add(C(R, 1, "cmd", "Projeto novo com Vite",
                "O Thiago quer testar uma campanha de promoções num projeto separado, sem mexer no cafe365-painel.",
                "Com o npm, crie um projeto Vite (versão latest) chamado cafe365-promo usando o template react.",
                A("npm create vite@latest cafe365-promo -- --template react", "npm create vite cafe365-promo -- --template react",
                  "npm init vite@latest cafe365-promo -- --template react", "npm init vite cafe365-promo -- --template react"),
                A("O Vite cria projetos com npm create vite@latest NOME.", "As opções do Vite vão depois de -- : ... -- --template react"),
                A("npm create vite@latest", "Baixa e roda o criador de projetos do Vite.", "cafe365-promo", "Nome da pasta do projeto.",
                  "--", "Separa as opções do npm das opções repassadas ao Vite.", "--template react", "Escolhe o modelo React (JavaScript)."),
                "Depois: cd cafe365-promo, npm install e npm run dev. Para TypeScript, use --template react-ts."));

            list.Add(C(R, 1, "code", "Estado de objeto",
                "O estado filtro é um objeto com categoria e ordem. A Mari quer trocar só a categoria para \"doces\", sem perder a ordem escolhida.",
                "Chame setFiltro passando um objeto novo que copia filtro com o operador spread (...) e muda categoria para \"doces\".",
                ReactVar("setFiltro({ ...filtro, categoria: \"doces\" })",
                         "setFiltro(«f|(f)» => ({ ...f, categoria: \"doces\" }))"),
                A("O spread ... copia todas as propriedades de um objeto para um novo.", "setFiltro({ ...filtro, categoria: ... })"),
                A("{ ...filtro", "Objeto novo com uma cópia de tudo que havia em filtro.",
                  "categoria: \"doces\" }", "Sobrescreve só essa propriedade (vem depois do spread)."),
                "setFiltro({ categoria: \"doces\" }) apagaria a ordem: o set do useState SUBSTITUI o objeto, não mescla."));

            list.Add(C(R, 1, "code", "Props com valor padrão",
                "O Caio criou o componente Selo, que mostra uma etiqueta nos produtos. Na maioria das vezes a cor é cinza, então ninguém quer repetir isso.",
                "Escreva o componente de função Selo que desestrutura as props texto e cor (nessa ordem), com cor valendo \"cinza\" por padrão, e retorna um <span> com className={cor} e o texto {texto}.",
                ReactVar("«|export default |export »function Selo({ texto, cor = \"cinza\" }) {\n  return «<span className={cor}>{texto}</span>|(<span className={cor}>{texto}</span>)»\n}",
                         "const Selo = ({ texto, cor = \"cinza\" }) => «<span className={cor}>{texto}</span>|(<span className={cor}>{texto}</span>)»",
                         "const Selo = ({ texto, cor = \"cinza\" }) => {\n  return «<span className={cor}>{texto}</span>|(<span className={cor}>{texto}</span>)»\n}"),
                A("As props chegam num objeto: dá para desestruturar direto no parâmetro { a, b }.", "Valor padrão na desestruturação: { texto, cor = \"cinza\" }"),
                A("({ texto, cor = \"cinza\" })", "Desestrutura as props; se cor não for passada, vale \"cinza\".",
                  "className={cor}", "A classe vem da prop (expressão entre chaves).", "{texto}", "Mostra o texto recebido."),
                "Uso: <Selo texto=\"Novo\" /> fica cinza; <Selo texto=\"Promo\" cor=\"vermelho\" /> muda a cor."));

            list.Add(C(R, 1, "code", "Formulário controlado",
                "O campo de e-mail do cadastro de clientes precisa ficar sempre igual ao estado email (já criado com setEmail).",
                "Escreva o <input> controlado: value ligado a email e onChange chamando setEmail com o valor digitado (e.target.value), numa arrow function com parâmetro e.",
                ReactVar("<input value={email} onChange={«e|(e)» => «setEmail(e.target.value)|{ setEmail(e.target.value) }»} />",
                         "<input onChange={«e|(e)» => «setEmail(e.target.value)|{ setEmail(e.target.value) }»} value={email} />"),
                A("Campo controlado = value vem do estado + onChange atualiza o estado.", "onChange={e => setEmail(e.target.value)}"),
                A("value={email}", "O que aparece no campo é sempre o estado.",
                  "onChange={e => ...}", "A cada tecla, recebe o evento e.",
                  "e.target.value", "Texto atual do campo, guardado com setEmail."),
                "Sem o onChange o campo fica 'travado' (só leitura). Use defaultValue se não quiser controlar o campo."));

            // ───────────── PROFISSIONAL ─────────────
            list.Add(C(R, 2, "code", "Foco na busca",
                "Quando a tela de pedidos abre, o cursor deve já estar no campo de busca. O <input> vai receber ref={buscaRef}.",
                "Escreva duas instruções: crie buscaRef com useRef começando em null; depois um useEffect que roda só na montagem e chama focus() no elemento guardado em buscaRef.current.",
                ReactVar("const buscaRef = useRef(null)\nuseEffect(() => «{\n  buscaRef.current«.|?.»focus()\n}|buscaRef.current«.|?.»focus()», [])"),
                A("useRef guarda uma referência que não causa re-render; o elemento fica em .current.", "const buscaRef = useRef(null) e, no efeito com [], buscaRef.current.focus()"),
                A("useRef(null)", "Cria a referência; o React preenche .current com o elemento depois de montar.",
                  "buscaRef.current.focus()", "Chama o método do DOM no elemento real.",
                  "[]", "O efeito roda uma vez, quando o elemento já existe na tela."),
                "Não dá para dar foco durante a renderização: o elemento só existe depois dela — por isso o useEffect."));

            list.Add(C(R, 2, "code", "Filtro memorizado",
                "A lista de produtos tem milhares de itens e o filtro da busca é recalculado a cada renderização, até quando nada mudou.",
                "Guarde em visiveis, com useMemo, os produtos (array produtos) cujo p.nome inclui (includes) o texto busca. Refaça o cálculo quando produtos ou busca mudarem (nessa ordem nas dependências).",
                ReactVar("const visiveis = useMemo(() => «produtos.filter(«p|(p)» => p.nome.includes(busca))|{ return produtos.filter(«p|(p)» => p.nome.includes(busca)) }», «[produtos, busca]|[busca, produtos]»)"),
                A("useMemo(() => cálculo, [dependências]). O cálculo é um filter com includes.", "Dependências: TUDO que o cálculo usa de fora → [produtos, busca]"),
                A("produtos.filter(p => ...)", "Novo array só com os itens que passam no teste.",
                  "p.nome.includes(busca)", "Verdadeiro se o nome contém o texto buscado.",
                  "[produtos, busca]", "Esquecer busca deixaria o filtro 'congelado' no primeiro texto."),
                "Regra de ouro dos hooks: toda variável de fora usada dentro do cálculo/efeito entra nas dependências (o ESLint avisa)."));

            list.Add(C(R, 2, "code", "Pedido pela URL",
                "A tela de detalhes recebe a prop id. Quando o usuário troca de pedido, o id muda — mas a tela continuava mostrando o pedido antigo.",
                "Escreva um useEffect que faz fetch de \"/api/pedidos/\" + id, converte com .then(r => r.json()) e guarda o resultado com .then(dados => setPedido(dados)). O efeito deve rodar de novo sempre que id mudar.",
                ReactVar("useEffect(() => «{\n  FETCH\n}|FETCH», [id])"
                    .Replace("FETCH", "fetch(«\"/api/pedidos/\" + id|`/api/pedidos/${id}`»)\n    .then(«r|(r)» => r.json())\n    .then(«dados => setPedido(dados)|(dados) => setPedido(dados)|setPedido»)")),
                A("O fetch vai dentro do useEffect; o array de dependências diz QUANDO repetir.", "useEffect(() => { fetch(...).then(...).then(...) }, [id])"),
                A("fetch(\"/api/pedidos/\" + id)", "Monta a URL com o id atual.",
                  ".then(r => r.json()).then(dados => setPedido(dados))", "Converte a resposta e guarda no estado.",
                  "[id]", "Roda na montagem E toda vez que o id mudar. Com [] ficaria preso no primeiro pedido."),
                "Em produção, cancele a requisição antiga no cleanup (AbortController) para uma resposta lenta não sobrescrever a nova."));

            list.Add(C(R, 2, "code", "Pedido pronto",
                "No painel da cozinha, o botão Pronto deve marcar o pedido daquele id como pronto, sem mexer nos outros e sem alterar o array original (o React só re-renderiza se receber um array novo).",
                "Chame setPedidos com a função atualizadora (parâmetro lista) que devolve lista.map: para o pedido p com p.id === id, um objeto novo com spread de p e pronto: true; para os demais, o próprio p.",
                ReactVar("setPedidos(«lista|(lista)» => lista.map(«p|(p)» => «TERN|(TERN)»))"
                    .Replace("TERN", "«p.id === id|id === p.id» ? «{ ...p, pronto: true }|({ ...p, pronto: true })» : p")),
                A("Nunca mude o item direto (p.pronto = true). Gere um array novo com map e um objeto novo com spread.", "lista.map(p => p.id === id ? { ...p, pronto: true } : p)"),
                A("setPedidos(lista => ...)", "Função atualizadora: recebe a lista mais recente.",
                  "lista.map(...)", "Cria um array novo com o mesmo tamanho.",
                  "p.id === id ? { ...p, pronto: true } : p", "Só o pedido certo vira um objeto novo; os outros são mantidos."),
                "Para remover use filter; para adicionar, [...lista, novo]. push, splice e atribuição direta mudam o original e o React não percebe."));

            list.Add(C(R, 2, "code", "Handler genérico",
                "O formulário de cadastro de cafeterias tem vários inputs (nome, cidade, telefone), cada um com o atributo name igual a uma chave do estado form. O Thiago não quer um onChange para cada campo.",
                "Crie a constante mudar: uma arrow function que recebe o evento e e chama setForm com a função atualizadora (parâmetro f), devolvendo um objeto novo com spread de f e a chave dinâmica e.target.name valendo e.target.value.",
                ReactVar("const mudar = «e|(e)» => «SET|{ SET }»".Replace("SET", "setForm(«f|(f)» => ({ ...f, [e.target.name]: e.target.value }))"),
                         "function mudar(e) { setForm(«f|(f)» => ({ ...f, [e.target.name]: e.target.value })) }"),
                A("Chave dinâmica num objeto usa colchetes: { [variavel]: valor }.", "Para a arrow devolver um objeto, envolva-o em parênteses: f => ({ ...f, ... })"),
                A("setForm(f => ...)", "Atualiza a partir do estado mais recente.",
                  "({ ...f, ... })", "Parênteses: sem eles as chaves seriam lidas como corpo da função.",
                  "[e.target.name]: e.target.value", "A chave vem do name do input; o valor, do que foi digitado."),
                "Em cada input: <input name=\"cidade\" value={form.cidade} onChange={mudar} />. Um handler serve para o formulário todo."));

            list.Add(C(R, 2, "code", "Remover com useCallback",
                "Cada <ItemCarrinho> usa React.memo e recebe a função remover. Mas remover é recriada a cada renderização, então o memo nunca evita nada.",
                "Crie remover com useCallback: uma arrow function que recebe id e chama setItens com a função atualizadora (parâmetro lista), filtrando os itens i com i.id !== id. Como nada de fora é usado, as dependências ficam vazias.",
                ReactVar("const remover = useCallback(«id|(id)» => «SET|{ SET }», [])"
                    .Replace("SET", "setItens(«lista|(lista)» => lista.filter(«i|(i)» => «i.id !== id|id !== i.id»))")),
                A("useCallback(função, dependências) devolve a MESMA função entre renderizações.", "useCallback(id => setItens(lista => lista.filter(...)), [])"),
                A("useCallback(..., [])", "A função é criada uma vez e mantém a mesma referência.",
                  "setItens(lista => ...)", "Graças à função atualizadora, não precisa de itens nas dependências.",
                  "lista.filter(i => i.id !== id)", "Array novo sem o item removido."),
                "useCallback só compensa junto com React.memo (ou como dependência de efeitos). Sem isso, é custo sem ganho."));

            list.Add(C(R, 2, "code", "Relógio do balcão",
                "O painel do balcão mostra há quantos segundos o pedido está aberto. Ao trocar de tela, o intervalo continuava rodando escondido e os intervalos iam se acumulando (vazamento de memória).",
                "Escreva um useEffect que roda só na montagem: guarde em const timer um setInterval que a cada 1000 ms chama setSegundos(s => s + 1), e retorne a função de limpeza que chama clearInterval(timer).",
                ReactVar("useEffect(() => {\n  const timer = setInterval(() => «setSegundos(«s|(s)» => s + 1)|{ setSegundos(«s|(s)» => s + 1) }», 1000)\n  return () => «clearInterval(timer)|{ clearInterval(timer) }»\n}, [])"),
                A("A função retornada pelo useEffect é a limpeza: roda quando o componente sai da tela.", "const timer = setInterval(...); return () => clearInterval(timer)"),
                A("setInterval(() => ..., 1000)", "Executa a cada 1 segundo e devolve um identificador.",
                  "setSegundos(s => s + 1)", "Atualizadora: com setSegundos(segundos + 1) o valor ficaria preso no 0 (closure antiga).",
                  "return () => clearInterval(timer)", "Cleanup: para o intervalo ao desmontar."),
                "Todo efeito que 'assina' algo (setInterval, addEventListener, WebSocket) deve desfazer isso no return."));

            list.Add(C(R, 2, "code", "Provider do tema",
                "O TemaContext já existe. Falta o componente que guarda o tema e o entrega, junto com a função de trocar, para todo o app.",
                "Escreva o componente de função TemaProvider que recebe { children }, cria o estado tema (com setTema) começando em \"claro\" e retorna <TemaContext.Provider> com value igual ao objeto { tema, setTema } e children dentro.",
                ReactVar("«|export default |export »function TemaProvider({ children }) {\n  const [tema, setTema] = useState(\"claro\")\n  return «PROV|(PROV)»\n}",
                         "const TemaProvider = ({ children }) => {\n  const [tema, setTema] = useState(\"claro\")\n  return «PROV|(PROV)»\n}")
                    .Select(s => s.Replace("PROV", "<TemaContext.Provider value={{ tema, setTema }}>{children}</TemaContext.Provider>")).ToArray(),
                A("children é a prop especial com tudo que fica entre as tags do componente.", "value={{ tema, setTema }} — chaves do JSX + chaves do objeto"),
                A("({ children })", "Recebe os componentes filhos.",
                  "useState(\"claro\")", "O tema mora no Provider.",
                  "value={{ tema, setTema }}", "Entrega valor e função: quem usa useContext pode ler e trocar o tema.",
                  "{children}", "Renderiza o app dentro do Provider."),
                "Uso: <TemaProvider><App /></TemaProvider>. Em apps grandes, guarde o value com useMemo para evitar re-renders."));

            list.Add(C(R, 2, "code", "Hook personalizado",
                "Menu, modal e filtro repetem a mesma lógica de abrir/fechar. A Mari quer extrair isso para um hook reutilizável.",
                "Crie o custom hook useAlternar que recebe inicial (padrão false), cria o estado ligado (com setLigado), define a constante alternar como arrow function que chama setLigado(v => !v) e retorna o array [ligado, alternar].",
                ReactVar("«|export default |export »function useAlternar(inicial = false) {\nCORPO\n}",
                         "const useAlternar = (inicial = false) => {\nCORPO\n}")
                    .Select(s => s.Replace("CORPO", "  const [ligado, setLigado] = useState(inicial)\n  const alternar = () => «setLigado(«v|(v)» => !v)|{ setLigado(«v|(v)» => !v) }»\n  return [ligado, alternar]"))
                    .SelectMany(s => ReactVar(s)).ToArray(),
                A("Custom hook é uma função cujo nome começa com use e que chama outros hooks.", "useState(inicial), alternar = () => setLigado(v => !v), return [ligado, alternar]"),
                A("function useAlternar(inicial = false)", "Hook com parâmetro de valor padrão.",
                  "setLigado(v => !v)", "Inverte o valor a partir do mais recente.",
                  "return [ligado, alternar]", "Devolve no formato do useState: const [aberto, alternarMenu] = useAlternar()."),
                "Cada componente que usa o hook ganha o SEU próprio estado: o hook reaproveita a lógica, não os dados."));
        }
    }

    static partial class Docs
    {
        static partial void DocsMaisReact(List<DocEntry> all)
        {
            const string R = "React";
            all.Add(X(R, "Projeto e npm", "npm create vite  (projeto novo)", "Cria um projeto novo com o Vite. As opções do Vite vão depois de -- (separador do npm).",
                "npm create vite@latest NOME -- --template MODELO",
                A("@latest", "usa a versão mais recente do criador", "--", "o que vem depois é repassado ao Vite (npm 7+)", "--template react", "React com JavaScript",
                  "--template react-ts", "React com TypeScript", "--template vue", "Vue"),
                A("npm create vite@latest agenda-escola -- --template vue", "projeto Vue chamado agenda-escola",
                  "npm create vite@latest loja-livros -- --template react-ts", "projeto React com TypeScript"),
                "Depois de criar: entre na pasta (cd NOME), rode npm install e npm run dev."));
            all.Add(X(R, "Componentes", "Componente de função", "Um componente é uma função com nome em MAIÚSCULA que retorna JSX.",
                "function Nome() {\n  return <tag>...</tag>\n}\nconst Nome = () => <tag>...</tag>", null,
                A("function Cabecalho() { return <header>Escola Nova</header> }", "componente com function",
                  "const Aviso = () => <strong>Atenção!</strong>", "mesma ideia com arrow function",
                  "export default Cabecalho", "exporta para usar em outro arquivo",
                  "import Cabecalho from \"./Cabecalho\"", "importa no outro arquivo"),
                "Nome em minúscula é tratado como tag HTML. JSX com várias linhas costuma ir entre parênteses: return ( ... )."));
            all.Add(X(R, "Componentes", "Props: desestruturação e valor padrão", "As props chegam num objeto; desestruture no parâmetro e defina valores padrão com =.",
                "function Comp({ prop1, prop2 = valorPadrao }) { ... }", null,
                A("function Medalha({ nome, nivel = \"bronze\" }) { return <b className={nivel}>{nome}</b> }", "nivel vale \"bronze\" se não for passado",
                  "<Medalha nome=\"Rita\" />", "usa o padrão", "<Medalha nome=\"Rita\" nivel=\"ouro\" />", "sobrescreve o padrão"),
                "O padrão só vale quando a prop não é passada (undefined)."));
            all.Add(X(R, "Componentes", "children", "Prop especial com o conteúdo colocado entre as tags do componente.",
                "function Caixa({ children }) { return <div>{children}</div> }", null,
                A("<Caixa><p>Olá</p></Caixa>", "o <p> chega como children"), null));
            all.Add(X(R, "Componentes", "React.memo", "Memoriza um componente: ele só re-renderiza quando as props mudam (comparação rasa).",
                "const Comp = memo(ComponenteOriginal)\nexport default memo(Componente)", null,
                A("export default memo(LinhaTabela)", "LinhaTabela só re-renderiza se as props mudarem"),
                "Funções e objetos criados a cada render são props 'novas' e anulam o memo: use useCallback/useMemo neles."));
            all.Add(X(R, "Formulários e eventos", "Formulário controlado  (value + onChange)", "O valor do campo vem do estado, e cada tecla atualiza o estado.",
                "<input value={estado} onChange={e => setEstado(e.target.value)} />",
                A("value", "o que o campo mostra (o estado)", "onChange", "chamado a cada alteração; recebe o evento", "e.target.value", "texto atual do campo", "e.target.name", "atributo name do campo"),
                A("<textarea value={obs} onChange={ev => setObs(ev.target.value)} />", "textarea ligada ao estado obs"),
                "Sem onChange o campo fica só leitura. Arrow com um parâmetro aceita ou não parênteses: e => ... ou (e) => ..."));
            all.Add(X(R, "Formulários e eventos", "Eventos com argumento", "Para passar um argumento, envolva a chamada numa arrow function.",
                "onClick={() => funcao(argumento)}", null,
                A("<button onClick={() => apagar(livro.id)}>Apagar</button>", "chama apagar com o id no clique"),
                "onClick={apagar(livro.id)} executaria na renderização, não no clique."));
            all.Add(X(R, "Estado e imutabilidade", "Spread em objetos  { ...obj }", "Cria um objeto novo copiando as propriedades de outro; o que vem depois sobrescreve.",
                "{ ...objeto, propriedade: novoValor }", null,
                A("setLivro({ ...livro, titulo: \"Dom Casmurro\" })", "troca só o título",
                  "const campo = \"turma\"\nconst novo = { ...aluno, [campo]: \"2B\" }", "chave dinâmica entre colchetes"),
                "O set do useState SUBSTITUI o objeto inteiro: sem o spread as outras propriedades somem."));
            all.Add(X(R, "Estado e imutabilidade", "Função atualizadora  setX(anterior => novo)", "Passe uma função ao set quando o novo valor depende do anterior: o React entrega o valor mais recente.",
                "setX(anterior => novoValor)", null,
                A("setPontos(p => p + 1)", "soma 1 ao valor mais recente", "setVisivel(v => !v)", "inverte um booleano",
                  "setAluno(a => ({ ...a, nota: 10 }))", "devolvendo objeto: envolva em parênteses"),
                "Sem os parênteses, a => { ...a } seria lido como corpo de função. Dentro de setInterval e callbacks a forma atualizadora evita valores 'velhos'."));
            all.Add(X(R, "Estado e imutabilidade", "Arrays no estado: map, filter e spread", "Nunca altere o array do estado (push, splice, item.x = ...). Gere sempre um array novo.",
                "lista.map(x => condicao ? { ...x, campo: valor } : x)\nlista.filter(x => x.id !== idRemover)\n[...lista, novoItem]",
                A("map", "troca/atualiza itens (mesmo tamanho)", "filter", "remove itens", "[...lista, novo]", "adiciona no fim"),
                A("setLivros(ls => ls.map(l => l.id === 7 ? { ...l, emprestado: false } : l))", "atualiza só o livro 7",
                  "setTurmas(ts => ts.filter(t => t.id !== 3))", "remove a turma 3"),
                "Use === para comparar ids. O React compara referências: array modificado no lugar não dispara re-render."));
            all.Add(X(R, "Estado e imutabilidade", "filter + includes  (busca em lista)", "filter mantém os itens que passam no teste; includes diz se um texto contém outro.",
                "lista.filter(item => item.campo.includes(texto))", null,
                A("alunos.filter(a => a.nome.includes(\"Ana\"))", "alunos cujo nome contém Ana",
                  "livros.filter(l => l.autor.toLowerCase().includes(termo.toLowerCase()))", "busca sem diferenciar maiúsculas"), null));
            all.Add(X(R, "Hooks", "useRef", "Guarda um valor que sobrevive às renderizações sem causar re-render. Muito usado para acessar um elemento do DOM.",
                "const ref = useRef(valorInicial)\n<input ref={ref} />\nref.current",
                A(".current", "onde fica o valor/elemento", "useRef(null)", "começa vazio; o React preenche ao montar"),
                A("const nomeRef = useRef(null)", "cria a referência", "<input ref={nomeRef} />", "liga ao elemento", "nomeRef.current.select()", "usa um método do DOM (depois de montar)"),
                "Acesse ref.current em efeitos ou eventos, não durante a renderização."));
            all.Add(X(R, "Hooks", "useEffect: limpeza (cleanup)", "A função retornada pelo efeito é executada quando o componente sai da tela (e antes de o efeito rodar de novo).",
                "useEffect(() => {\n  // assina algo\n  return () => { /* desfaz */ }\n}, [deps])",
                A("setInterval / clearInterval", "repetir a cada N ms / parar", "setTimeout / clearTimeout", "uma vez depois de N ms / cancelar",
                  "addEventListener / removeEventListener", "ouvir / parar de ouvir eventos"),
                A("useEffect(() => {\n  const aviso = setTimeout(() => setMostrar(false), 3000)\n  return () => clearTimeout(aviso)\n}, [])", "esconde um aviso após 3 s e cancela se sair antes",
                  "useEffect(() => {\n  const tecla = e => setUltima(e.key)\n  window.addEventListener(\"keydown\", tecla)\n  return () => window.removeEventListener(\"keydown\", tecla)\n}, [])", "ouve o teclado e para ao desmontar"),
                "Sem cleanup, timers e listeners continuam vivos depois que o componente some (vazamento)."));
            all.Add(X(R, "Hooks", "fetch dentro do useEffect", "Buscar dados é um efeito colateral: faça dentro do useEffect e guarde o resultado no estado.",
                "useEffect(() => {\n  fetch(url).then(r => r.json()).then(d => setDados(d))\n}, [deps])",
                A("[]", "busca uma vez, ao montar", "[codigo]", "busca de novo sempre que codigo mudar"),
                A("useEffect(() => {\n  fetch(\"/api/turmas/\" + serie).then(r => r.json()).then(t => setTurmas(t))\n}, [serie])", "recarrega quando a série muda",
                  "fetch(`/api/turmas/${serie}`)", "mesma URL com template string"),
                "Se a URL usa uma variável, ela precisa estar nas dependências; senão o efeito usa o valor antigo."));
            all.Add(X(R, "Hooks", "useMemo com várias dependências", "Inclua nas dependências TODAS as variáveis de fora usadas no cálculo.",
                "const r = useMemo(() => calculo(a, b), [a, b])", null,
                A("const aprovados = useMemo(() => alunos.filter(a => a.nota >= corte), [alunos, corte])", "refaz se alunos ou corte mudarem"),
                "Faltou uma dependência? O resultado fica 'congelado' com o valor antigo dela."));
            all.Add(X(R, "Hooks", "useCallback", "Memoriza uma FUNÇÃO: devolve a mesma referência entre renderizações enquanto as dependências não mudam.",
                "const fn = useCallback((params) => { ... }, [dependências])", null,
                A("const marcar = useCallback(id => setTarefas(ts => ts.map(t => t.id === id ? { ...t, feita: true } : t)), [])", "função estável para filhos com memo",
                  "const salvar = useCallback(() => enviar(texto), [texto])", "recria só quando texto mudar"),
                "Com a função atualizadora (setX(x => ...)) o estado não precisa entrar nas dependências."));
            all.Add(X(R, "Hooks", "Context.Provider", "Componente que fornece o valor do contexto para toda a árvore dentro dele.",
                "<MeuContext.Provider value={valor}>\n  {children}\n</MeuContext.Provider>", null,
                A("function UsuarioProvider({ children }) {\n  const [usuario, setUsuario] = useState(null)\n  return <UsuarioContext.Provider value={{ usuario, setUsuario }}>{children}</UsuarioContext.Provider>\n}", "provider com estado próprio",
                  "value={{ idioma, mudarIdioma }}", "chaves duplas: expressão do JSX + objeto"),
                "Quem chama useContext(MeuContext) recebe o value do Provider mais próximo."));
            all.Add(X(R, "Hooks", "Custom hooks  (useAlgo)", "Funções que começam com use e chamam outros hooks: reaproveitam LÓGICA entre componentes.",
                "function useNome(parametros) {\n  // useState, useEffect...\n  return valores\n}", null,
                A("function useContador(inicio = 0) {\n  const [n, setN] = useState(inicio)\n  const somar = () => setN(x => x + 1)\n  return [n, somar]\n}", "devolve no formato [valor, função]",
                  "const [cliques, clicar] = useContador()", "uso no componente"),
                "O nome precisa começar com use. Cada componente que usa o hook tem seu próprio estado."));
        }
    }
}
