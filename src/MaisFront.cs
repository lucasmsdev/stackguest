using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerQuest
{
    static partial class Challenges
    {
        // Todas as combinações de partes (produto cartesiano): FrontMix(A("a", "b"), A("1", "2")) → a1, a2, b1, b2.
        static string[] FrontMix(params string[][] parts)
        {
            var acc = new List<string> { "" };
            foreach (var p in parts)
            {
                var next = new List<string>();
                foreach (var a in acc) foreach (var b in p) next.Add(a + b);
                acc = next;
            }
            return acc.Distinct().ToArray();
        }

        // Todas as ordens possíveis de uma lista de atributos, unidos por espaço.
        static List<string> FrontPerms(List<string> items)
        {
            var res = new List<string>();
            if (items.Count <= 1) { res.Add(string.Join(" ", items)); return res; }
            for (int i = 0; i < items.Count; i++)
            {
                var rest = new List<string>(items);
                rest.RemoveAt(i);
                foreach (var r in FrontPerms(rest)) res.Add(items[i] + " " + r);
            }
            return res;
        }

        static void FrontTier(List<Level> list, string title, int tier)
        {
            var lv = list.First(l => l.Cat == "front" && l.Title == title);
            lv.Tier = tier;
            lv.Chapter = Cat("front").Name + " · " + TierNames[tier];
        }

        static partial void MaisFront(List<Level> list)
        {
            const string F = "front";

            // ── 1) rebalanceamento dos desafios existentes
            // fáceis demais para o Intermediário → Iniciante
            FrontTier(list, "Menu lado a lado", 0);
            FrontTier(list, "Card centralizado", 0);
            FrontTier(list, "Preço constante", 0);
            FrontTier(list, "Pegando o carrinho", 0);
            // eram "Profissional", mas são o dia a dia de um júnior → Intermediário
            FrontTier(list, "Clique para comprar", 1);
            FrontTier(list, "Buscando produtos", 1);
            FrontTier(list, "Galeria em grid", 1);
            FrontTier(list, "Menu no celular", 1);
            FrontTier(list, "Arrow function", 1);

            // ── 2) novos desafios
            // ───── Iniciante
            list.Add(C(F, 0, "html", "Rodapé semântico",
                "A Nina passou o leitor de tela na página do Café 365: o rodapé é só uma <div> genérica, e quem não enxerga não sabe que ali termina a página.",
                "Troque a div pela tag semântica própria de rodapé, com o texto: Micorosoft 2026",
                A("<footer>Micorosoft 2026</footer>"),
                A("O HTML5 tem tags com significado: header (topo), nav (menu), main (conteúdo principal) e uma para o rodapé.", "Rodapé em inglês é footer: <footer>...</footer>"),
                A("<footer>", "Rodapé da página (ou de uma seção): informações de autoria, direitos, contato.", "Micorosoft 2026", "O conteúdo do rodapé."),
                "Tags semânticas (header, nav, main, footer) não mudam a aparência, mas ajudam leitores de tela, o Google e quem for manter o código."));

            // ───── Intermediário
            list.Add(C(F, 1, "html", "Rótulo do campo",
                "A Nina reclamou do cadastro: o campo de e-mail não tem rótulo, então o leitor de tela anuncia só \"campo de edição\". E clicar no texto deveria levar o cursor para o campo.",
                "Escreva o rótulo (label) ligado ao campo pelo atributo for com o valor email e o texto E-mail; logo depois, um input do tipo email com id email.",
                A("<label for=\"email\">E-mail</label><input type=\"email\" id=\"email\">",
                  "<label for=\"email\">E-mail</label><input id=\"email\" type=\"email\">"),
                A("O label se liga ao campo quando o valor do for é igual ao id do input.", "<label for=\"email\">E-mail</label> e depois <input type=\"email\" id=\"email\">"),
                A("<label for=\"email\">", "Rótulo do campo; o for aponta para o id do input.", "E-mail", "Texto que aparece e que o leitor de tela anuncia.",
                  "<input type=\"email\" id=\"email\">", "O campo; o id precisa ser igual ao for do label."),
                "Placeholder NÃO substitui o label: ele some quando a pessoa começa a digitar e muitos leitores de tela o ignoram."));

            list.Add(C(F, 1, "code", "Cor em variável",
                "A Camila contou 40 lugares no CSS com o marrom do Café 365 (#6f4e37) digitado à mão. Quando o design mudar a cor, alguém vai esquecer um. Hora de usar variável CSS.",
                "Escreva duas regras: em :root, crie a variável --marrom com o valor #6f4e37; depois, faça os h2 usarem essa variável na cor do texto (color).",
                A(":root { --marrom: #6f4e37; }\nh2 { color: var(--marrom); }"),
                A("Variáveis CSS começam com dois hífens e são lidas com a função var().", ":root { --marrom: ...; }  e  h2 { color: var(--marrom); }"),
                A(":root { --marrom: #6f4e37; }", "Declara a variável na raiz do documento: vale para a página toda.",
                  "h2 { color: var(--marrom); }", "Usa o valor da variável. Mudou em :root, muda em todo lugar."),
                "var() aceita um valor reserva: color: var(--marrom, brown). Variáveis também podem ser trocadas dentro de uma media query (ex.: tema escuro)."));

            list.Add(C(F, 1, "code", "Saudação com crase",
                "A Yuki monta a mensagem de boas-vindas com \"Olá, \" + nome + \"!\" e vive esquecendo um espaço. A Camila sugere template literal.",
                "Crie a constante saudacao com um template literal que produza o texto Olá, <nome>! usando a variável nome. Exemplo: se nome for Ana, o texto fica Olá, Ana!",
                A("const saudacao = `Olá, ${nome}!`"),
                A("Template literals são escritos entre crases (`), não entre aspas.", "Variáveis entram com ${...}: `Olá, ${nome}!`"),
                A("const saudacao =", "Guarda o texto numa constante.", "`...`", "Template literal: string entre crases que aceita expressões.", "${nome}", "Insere o valor da variável no meio do texto."),
                "Dentro de ${ } cabe qualquer expressão: `Total: ${qtd * preco}`. Template literals também podem ter várias linhas."));

            list.Add(C(F, 1, "code", "Só os cafés gelados",
                "No verão, a vitrine deve mostrar só as bebidas geladas. O array cafes tem objetos assim: { nome: \"Latte\", gelado: false }.",
                "Crie a constante gelados com os itens de cafes cuja propriedade gelado é verdadeira, usando filter com uma arrow function de parâmetro c.",
                FrontMix(A("const gelados = cafes.filter("), A("c => ", "(c) => "),
                         A("c.gelado", "c.gelado === true", "c.gelado == true", "{ return c.gelado }", "{ return c.gelado === true }"), A(")")),
                A("filter cria um array novo só com os itens para os quais a função devolve true.", "const gelados = cafes.filter(c => c.____)"),
                A("cafes.filter(...)", "Percorre o array e devolve um NOVO array (o original não muda).", "c => c.gelado", "Para cada café c, mantém se gelado for verdadeiro."),
                "map transforma cada item, filter escolhe itens e reduce junta tudo num valor só. Eles podem ser encadeados: cafes.filter(...).map(...)."));

            // ───── Profissional
            list.Add(C(F, 2, "code", "Comentário sem XSS",
                "As avaliações dos clientes aparecem com comentario.innerHTML = texto. Um \"cliente\" escreveu <img src=x onerror=alert(1)> e o alerta abriu para todo mundo que visitou a página. A Yuki: \"isso é XSS!\"",
                "Reescreva a linha para colocar a variável texto dentro do elemento comentario como texto puro, sem que o navegador interprete HTML.",
                A("comentario.textContent = texto", "comentario.innerText = texto"),
                A("innerHTML interpreta o conteúdo como HTML. Existe uma propriedade que trata tudo como texto.", "comentario.textContent = ..."),
                A("textContent", "Define o conteúdo como texto: < e > aparecem na tela em vez de virar tags.", "= texto", "O texto digitado pelo usuário, agora inofensivo."),
                "Nunca coloque dado do usuário em innerHTML. Se precisar mesmo de HTML vindo de fora, sanitize antes (ex.: biblioteca DOMPurify) e use Content-Security-Policy."));

            list.Add(C(F, 2, "code", "Carrinho salvo",
                "O carrinho é salvo com localStorage.setItem(\"carrinho\", JSON.stringify(itens)). Ao reabrir o site ele precisa ser lido de volta, mas na primeira visita não há nada salvo, getItem devolve null e o código quebra no .push().",
                "Crie a constante carrinho lendo a chave carrinho do localStorage e convertendo o texto de volta com JSON.parse; se não houver nada salvo, o resultado deve ser um array vazio (use o operador ||).",
                A("const carrinho = JSON.parse(localStorage.getItem(\"carrinho\")) || []",
                  "const carrinho = JSON.parse(localStorage.getItem(\"carrinho\") || \"[]\")",
                  "const carrinho = JSON.parse(localStorage.getItem(\"carrinho\")) ?? []",
                  "const carrinho = JSON.parse(localStorage.getItem(\"carrinho\") ?? \"[]\")"),
                A("O localStorage só guarda texto: leia com getItem e converta com JSON.parse.", "JSON.parse(localStorage.getItem(\"carrinho\")) || []"),
                A("localStorage.getItem(\"carrinho\")", "Lê o texto salvo (ou null se a chave não existe).", "JSON.parse(...)", "Converte o texto JSON de volta em array/objeto.",
                  "|| []", "Se o resultado for null, usa um array vazio."),
                "localStorage guarda só strings, fica no navegador e qualquer script da página consegue ler: nunca guarde senha ou token sensível nele."));

            list.Add(C(F, 2, "code", "Menos movimento",
                "Os cards do Café 365 têm transition: transform 0.3s e crescem quando o mouse passa. A Nina explica que pessoas com labirintite ativam \"reduzir movimento\" no sistema, e o site precisa respeitar essa preferência.",
                "Escreva a media query que detecta a preferência do sistema por movimento reduzido (valor reduce) e, dentro dela, desliga a transição da classe .card (transition com o valor none).",
                A("@media (prefers-reduced-motion: reduce) { .card { transition: none; } }"),
                A("Media queries não testam só largura: também testam preferências do usuário, como prefers-color-scheme.", "@media (prefers-reduced-motion: reduce) { ... }"),
                A("@media (prefers-reduced-motion: reduce)", "Vale só para quem pediu menos movimento nas configurações do sistema.", ".card { transition: none; }", "Remove a animação dos cards."),
                "Acessibilidade também é respeitar preferências: prefers-reduced-motion, prefers-color-scheme (tema escuro) e zoom de texto sem quebrar o layout."));

            var aria = new List<string>();
            aria.AddRange(FrontPerms(new List<string> { "aria-expanded=\"false\"", "aria-controls=\"menu-principal\"" }));
            aria.AddRange(FrontPerms(new List<string> { "type=\"button\"", "aria-expanded=\"false\"", "aria-controls=\"menu-principal\"" }));
            list.Add(C(F, 2, "html", "Botão do menu acessível",
                "No celular, o menu (elemento com id menu-principal) abre por um botão \"Menu\" e começa fechado. A Nina testou com leitor de tela: ele não avisa se o menu está aberto nem diz qual elemento o botão controla.",
                "Escreva o botão com o texto Menu e dois atributos ARIA: um informando que o conteúdo controlado está recolhido (valor false) e outro indicando que ele controla o elemento menu-principal.",
                aria.Select(a => "<button " + a + ">Menu</button>").ToArray(),
                A("Procure na Biblioteca os atributos aria-* de estado (expandido ou não) e de relação (qual elemento controla).", "<button aria-expanded=\"...\" aria-controls=\"...\">Menu</button>"),
                A("<button>", "Use um botão de verdade (não uma div): já recebe foco e funciona com Enter e Espaço.", "aria-expanded=\"false\"", "Diz ao leitor de tela que o menu está fechado. O JavaScript troca para true ao abrir.",
                  "aria-controls=\"menu-principal\"", "Indica o id do elemento que o botão abre e fecha."),
                "Atributos aria-* não mudam o comportamento, só o que é anunciado: o JavaScript precisa atualizar aria-expanded toda vez que o menu abrir ou fechar."));

            string cab = "<tr><th scope=\"col\">Café</th><th scope=\"col\">Preço</th></tr>", lin = "<tr><td>Expresso</td><td>6,50</td></tr>";
            list.Add(C(F, 2, "html", "Tabela acessível",
                "A tabela de preços foi feita só com <td> e o leitor de tela lê célula por célula, sem saber qual coluna é preço. A Nina pede a tabela do jeito certo.",
                "Escreva a tabela completa: legenda Preços; uma linha de cabeçalho com as células de cabeçalho Café e Preço, cada uma indicando que é cabeçalho de coluna (scope col); e uma linha de dados com Expresso e 6,50.",
                A("<table><caption>Preços</caption>" + cab + lin + "</table>",
                  "<table><caption>Preços</caption><thead>" + cab + "</thead><tbody>" + lin + "</tbody></table>",
                  "<table><caption>Preços</caption><thead>" + cab + "</thead>" + lin + "</table>",
                  "<table><caption>Preços</caption>" + cab + "<tbody>" + lin + "</tbody></table>"),
                A("A legenda é a tag caption (primeira dentro da table); cabeçalhos são th, dados são td, e cada linha é um tr.", "<th scope=\"col\">Café</th> — o scope diz que o th é cabeçalho da coluna."),
                A("<caption>Preços</caption>", "Título da tabela, anunciado antes dos dados.", "<th scope=\"col\">", "Célula de cabeçalho de coluna: o leitor de tela repete \"Preço\" ao ler cada valor.",
                  "<td>", "Célula de dado comum.", "<tr>", "Uma linha da tabela."),
                "Tabela é para dados tabulares, nunca para montar layout. Agrupe com thead/tbody em tabelas maiores."));

            list.Add(C(F, 2, "code", "Layout com grid-areas",
                "O painel do gerente tem um topo que ocupa toda a largura, um menu lateral à esquerda e o conteúdo à direita. A Camila quer um layout legível, com áreas nomeadas.",
                "Escreva duas regras: (1) .pagina com display grid e grid-template-areas formando duas linhas — a primeira com topo topo, a segunda com menu conteudo (nesta ordem de declarações); (2) .lateral posicionada na área menu.",
                A(".pagina { display: grid; grid-template-areas: \"topo topo\" \"menu conteudo\"; }\n.lateral { grid-area: menu; }",
                  ".pagina { grid-template-areas: \"topo topo\" \"menu conteudo\"; display: grid; }\n.lateral { grid-area: menu; }"),
                A("Cada linha da grade é uma string entre aspas; repetir o nome faz a área ocupar várias colunas.", "grid-template-areas: \"topo topo\" \"menu conteudo\";  e  .lateral { grid-area: menu; }"),
                A("display: grid;", "Liga o layout em grade.", "grid-template-areas: \"topo topo\" \"menu conteudo\";", "Desenha o layout: 2 linhas × 2 colunas; topo ocupa as duas colunas da primeira linha.",
                  ".lateral { grid-area: menu; }", "Coloca o elemento na área chamada menu."),
                "Numa media query dá para redesenhar só o grid-template-areas (ex.: tudo empilhado no celular) sem mexer no HTML."));

            string tpl = "`${nome} - R$ ${preco.toFixed(2)}`";
            list.Add(C(F, 2, "code", "Cartão desestruturado",
                "A vitrine chama cartao(produto) para cada item. Hoje a função repete produto.nome e produto.preco o tempo todo, e o preço 8.5 aparece como R$ 8.5.",
                "Crie a função cartao (function declaration) que desestrutura nome e preco direto no parâmetro e retorna um template literal no formato NOME - R$ PRECO, com o preço sempre com 2 casas decimais. Ex.: cartao({ nome: \"Latte\", preco: 8.5 }) devolve Latte - R$ 8.50",
                FrontMix(A("function cartao({ nome, preco }) ", "function cartao({ preco, nome }) "), A("{ return " + tpl + " }"))
                    .Concat(FrontMix(A("const cartao = ({ nome, preco }) => ", "const cartao = ({ preco, nome }) => "), A(tpl, "{ return " + tpl + " }"))).ToArray(),
                A("Desestruturar no parâmetro: function f({ a, b }) { ... }. Números têm o método toFixed(casas).", "function cartao({ nome, preco }) { return `${nome} - R$ ${preco.____(2)}` }"),
                A("function cartao({ nome, preco })", "Recebe um objeto e já extrai as propriedades nome e preco em variáveis.", "`${nome} - R$ ...`", "Template literal montando o texto.",
                  "preco.toFixed(2)", "Formata com exatamente 2 casas decimais (8.5 → \"8.50\")."),
                "Para moeda de verdade, prefira preco.toLocaleString(\"pt-BR\", { style: \"currency\", currency: \"BRL\" }), que já usa vírgula e R$."));

            var somas = A("soma + item.preco * item.qtd", "soma + item.qtd * item.preco", "soma + (item.preco * item.qtd)", "soma + (item.qtd * item.preco)");
            var reduce = new List<string>();
            foreach (var s in somas)
            {
                reduce.Add("const total = itens.reduce((soma, item) => " + s + ", 0)");
                reduce.Add("const total = itens.reduce((soma, item) => { return " + s + " }, 0)");
                reduce.Add("const total = itens.reduce(function (soma, item) { return " + s + " }, 0)");
            }
            list.Add(C(F, 2, "code", "Total com reduce",
                "O carrinho é o array itens, com objetos como { nome: \"Latte\", preco: 8.5, qtd: 2 }. O total do pedido está sendo somado com um for cheio de bugs.",
                "Crie a constante total somando preco * qtd de todos os itens com reduce. Use os parâmetros soma e item, e informe o valor inicial 0.",
                reduce.ToArray(),
                A("reduce recebe (acumulador, itemAtual) e um valor inicial; o que a função retorna vira o acumulador da próxima volta.", "itens.reduce((soma, item) => soma + ..., 0)"),
                A("itens.reduce(...)", "Percorre o array juntando tudo num único valor.", "(soma, item) => soma + item.preco * item.qtd", "A cada item, soma o subtotal ao acumulado.",
                  ", 0", "Valor inicial. Sem ele, o primeiro ITEM (um objeto!) vira a soma inicial e o resultado sai errado."),
                "Esquecer o valor inicial é a armadilha clássica do reduce: com array vazio, ainda por cima, ele lança erro."));

            list.Add(C(F, 2, "code", "Pedido sem recarregar",
                "Ao clicar em Finalizar, o formulário (variável form) recarrega a página e o pedido some antes de enviarPedido() terminar.",
                "Registre no form um ouvinte do evento de envio do formulário com uma função de parâmetro evento que primeiro impede o comportamento padrão do navegador e depois chama enviarPedido().",
                A("form.addEventListener(\"submit\", (evento) => {\n  evento.preventDefault();\n  enviarPedido();\n})").Concat(FrontMix(A("form.addEventListener(\"submit\", "), A("(evento) => {", "evento => {", "function (evento) {"), A(" evento.preventDefault(); enviarPedido(); })"))).ToArray(),
                A("O evento de envio de formulário é \"submit\". O objeto do evento tem um método que cancela a ação padrão.", "evento.preventDefault()"),
                A("\"submit\"", "Evento disparado ao enviar o formulário (botão ou Enter).", "evento.preventDefault()", "Cancela o envio padrão, que recarregaria a página.",
                  "enviarPedido()", "Agora o JavaScript envia os dados (ex.: com fetch)."),
                "Ouça o submit no form, não o click no botão: assim o Enter também funciona e a validação do navegador (required, type=email) continua valendo."));

            var cardapio = FrontMix(
                A("async function carregarCardapio() {", "const carregarCardapio = async () => {"),
                A(" try { const resposta = await fetch(\"/api/cardapio\"); "),
                A("if (!resposta.ok) throw new Error(\"Falha ao carregar\"); ", "if (!resposta.ok) { throw new Error(\"Falha ao carregar\"); } ",
                  "if (resposta.ok === false) throw new Error(\"Falha ao carregar\"); ", "if (resposta.ok === false) { throw new Error(\"Falha ao carregar\"); } "),
                A("return await resposta.json(); ", "return resposta.json(); ", "const dados = await resposta.json(); return dados; "),
                A("} catch (erro) { console.error(erro); return []; } }"));
            list.Add(C(F, 2, "code", "Cardápio com async",
                "A versão com .then do cardápio engole erros: se a API devolve 500, a vitrine fica vazia e ninguém sabe por quê. Lembre-se: fetch só rejeita em falha de rede, não em erro HTTP.",
                "Crie a função assíncrona carregarCardapio. Dentro de um try: busque /api/cardapio com fetch e await, guardando na constante resposta; se a resposta não estiver ok (propriedade ok), lance um Error com a mensagem Falha ao carregar; senão, retorne o JSON da resposta. No catch (parâmetro erro): mostre o erro com console.error e retorne um array vazio.",
                A("async function carregarCardapio() {\n  try {\n    const resposta = await fetch(\"/api/cardapio\");\n    if (!resposta.ok) throw new Error(\"Falha ao carregar\");\n    return await resposta.json();\n  } catch (erro) {\n    console.error(erro);\n    return [];\n  }\n}").Concat(cardapio).ToArray(),
                A("async/await + try/catch; o fetch NÃO lança erro em 404/500 — confira resposta.ok e use throw.", "if (!resposta.ok) throw new Error(\"Falha ao carregar\")"),
                A("async function carregarCardapio()", "Função assíncrona: permite usar await e sempre devolve uma Promise.", "const resposta = await fetch(\"/api/cardapio\")", "Espera a resposta HTTP chegar.",
                  "if (!resposta.ok) throw new Error(...)", "ok é true só para status 200–299; em 404/500 forçamos o erro.", "return await resposta.json()", "Lê o corpo como JSON.",
                  "catch (erro) { console.error(erro); return []; }", "Qualquer falha (rede, HTTP ou JSON inválido) cai aqui: registra e devolve uma lista vazia para a tela não quebrar."),
                "Em produção, além do console.error, mostre uma mensagem amigável na tela (\"Não foi possível carregar o cardápio\") e permita tentar de novo."));
        }
    }

    static partial class Docs
    {
        static partial void DocsMaisFront(List<DocEntry> all)
        {
            const string F = "HTML/CSS/JS";
            // ── HTML
            all.Add(X(F, "HTML", "<header> <nav> <main> <footer>  (tags semânticas)", "Tags do HTML5 que dizem o PAPEL de cada parte da página. Não mudam a aparência, mas ajudam leitores de tela e buscadores.",
                "<header>topo</header>\n<nav>menu</nav>\n<main>conteúdo principal</main>\n<footer>rodapé</footer>",
                A("<header>", "topo da página ou de uma seção", "<nav>", "bloco de navegação (menus)", "<main>", "conteúdo principal (só um por página)",
                  "<section> / <article>", "seção temática / conteúdo independente", "<footer>", "rodapé: autoria, direitos, contato"),
                A("<footer>Escola Saber 2025</footer>", "rodapé com o nome da escola", "<nav><a href=\"/turmas\">Turmas</a></nav>", "menu de navegação"),
                "Prefira a tag semântica a uma <div> sempre que existir uma com o significado certo."));
            all.Add(X(F, "HTML", "<label for>  (rótulos de campos)", "Liga um texto a um campo de formulário. Clicar no rótulo foca o campo, e o leitor de tela anuncia o texto ao entrar no campo.",
                "<label for=\"ID_DO_CAMPO\">Texto</label>\n<input type=\"TIPO\" id=\"ID_DO_CAMPO\">",
                A("for", "no label: o id do campo ligado", "id", "no input: identificador único (igual ao for)"),
                A("<label for=\"telefone\">Telefone</label><input type=\"tel\" id=\"telefone\">", "rótulo ligado ao campo de telefone",
                  "<label>Nome <input type=\"text\"></label>", "outra forma: o campo dentro do label"),
                "placeholder não substitui o label: ele some ao digitar."));
            all.Add(X(F, "HTML", "<table>  (tabelas acessíveis)", "Tabelas para dados em linhas e colunas.",
                "<table>\n  <caption>Título</caption>\n  <tr><th scope=\"col\">Coluna</th></tr>\n  <tr><td>dado</td></tr>\n</table>",
                A("<caption>", "legenda/título da tabela (primeiro filho da table)", "<tr>", "linha", "<th>", "célula de cabeçalho", "<td>", "célula de dado",
                  "scope=\"col\" / \"row\"", "o th é cabeçalho da coluna / da linha", "<thead> <tbody>", "agrupam as linhas de cabeçalho e de dados (opcionais)"),
                A("<table><caption>Notas</caption><tr><th scope=\"col\">Aluno</th><th scope=\"col\">Nota</th></tr><tr><td>Bia</td><td>9</td></tr></table>", "tabela de notas com legenda e cabeçalhos"),
                "Nunca use tabela para montar layout: use flex ou grid."));
            all.Add(X(F, "HTML", "Atributos ARIA  (aria-*)", "Informam a tecnologias assistivas (leitores de tela) o estado e a relação de elementos que o HTML sozinho não expressa.",
                "<elemento aria-ATRIBUTO=\"valor\">",
                A("aria-label=\"...\"", "nome acessível (ex.: botão só com ícone)", "aria-expanded=\"true|false\"", "se o conteúdo controlado está aberto ou fechado",
                  "aria-controls=\"id\"", "id do elemento que este controla", "aria-hidden=\"true\"", "esconde do leitor de tela (ícones decorativos)",
                  "aria-live=\"polite\"", "anuncia mudanças de conteúdo (ex.: \"item adicionado\")"),
                A("<button aria-expanded=\"true\" aria-controls=\"filtros\">Filtros</button>", "botão que abre o painel id=filtros (aberto)",
                  "<button aria-label=\"Fechar janela\">X</button>", "botão de ícone com nome legível"),
                "Primeira regra do ARIA: se existe tag HTML nativa (button, nav...), use-a. aria-* só muda o que é anunciado; o JavaScript precisa atualizar os valores."));
            // ── CSS
            all.Add(X(F, "CSS", "Variáveis CSS  (--nome e var())", "Guardam um valor reutilizável (cor, espaçamento...). Declaradas com dois hífens, lidas com var().",
                ":root { --nome: valor; }\nseletor { propriedade: var(--nome); }",
                A(":root", "raiz do documento: variáveis globais", "--nome", "declara a variável (sempre com --)", "var(--nome, reserva)", "lê o valor; o segundo é opcional (usado se não existir)"),
                A(":root { --verde-escola: #2e7d32; }", "variável global de cor", ".titulo { background: var(--verde-escola); }", "usa a variável no fundo"),
                "Mudar a variável em um lugar (ou numa media query de tema escuro) muda o site todo."));
            all.Add(X(F, "CSS", "grid-template-areas  (áreas nomeadas)", "Desenha o layout da grade com nomes. Cada string é uma linha; cada palavra, uma coluna.",
                ".container { display: grid; grid-template-areas: \"a a\" \"b c\"; }\n.item { grid-area: b; }",
                A("grid-template-areas", "no container: o desenho das áreas", "grid-area: nome", "no filho: em qual área ele fica", ".", "célula vazia no desenho"),
                A(".mural { display: grid; grid-template-areas: \"capa capa capa\" \"avisos fotos fotos\"; }", "capa na linha 1 inteira; avisos e fotos na linha 2",
                  ".avisos { grid-area: avisos; }", "coloca o elemento na área avisos"),
                "Repetir o nome numa linha faz a área se estender pelas colunas. As áreas precisam formar retângulos."));
            all.Add(X(F, "CSS", "transition e :hover", ":hover aplica estilos quando o mouse está sobre o elemento; transition anima a mudança.",
                "seletor { transition: propriedade duração; }\nseletor:hover { propriedade: novo-valor; }",
                A(":hover", "mouse por cima", ":focus-visible", "elemento focado pelo teclado", "transition: transform 0.2s", "anima a propriedade transform por 0.2s",
                  "transition: none", "sem animação"),
                A(".livro { transition: transform 0.2s; }", "prepara a animação", ".livro:hover { transform: scale(1.1); }", "cresce 10% com o mouse por cima"), null));
            all.Add(X(F, "CSS", "@media e preferências do usuário", "Além da largura da tela, media queries detectam configurações do sistema da pessoa.",
                "@media (prefers-reduced-motion: reduce) { regras }\n@media (prefers-color-scheme: dark) { regras }",
                A("prefers-reduced-motion: reduce", "a pessoa pediu menos animação (labirintite, enjoo, distração)", "prefers-color-scheme: dark", "a pessoa usa tema escuro"),
                A("@media (prefers-reduced-motion: reduce) { .banner { animation: none; } }", "desliga a animação do banner",
                  "@media (prefers-color-scheme: dark) { body { background: #111; } }", "fundo escuro para quem usa tema escuro"),
                "Respeitar essas preferências faz parte da acessibilidade (WCAG)."));
            // ── JavaScript
            all.Add(X(F, "JavaScript", "Template literals  (crases)", "Strings entre crases (`) que aceitam expressões com ${ } e várias linhas.",
                "`texto ${expressao} texto`", null,
                A("const aviso = `Bem-vinda, ${aluna}!`", "insere a variável aluna", "`Total: ${qtd * valor}`", "qualquer expressão cabe dentro de ${ }"),
                "A crase fica geralmente na tecla do acento grave (` ) — no teclado ABNT, aperte e depois a barra de espaço."));
            all.Add(X(F, "JavaScript", "Arrays: map, filter e reduce", "Métodos que percorrem um array sem for e devolvem um resultado novo (o original não muda).",
                "lista.map(item => novoValor)\nlista.filter(item => condicao)\nlista.reduce((acumulado, item) => novoAcumulado, valorInicial)",
                A("map", "transforma cada item (mesmo tamanho)", "filter", "mantém só os itens cuja função devolve true", "reduce", "junta tudo num único valor (soma, objeto...)"),
                A("const aprovados = alunos.filter(a => a.nota >= 7)", "só quem tirou 7 ou mais", "const nomes = alunos.map(a => a.nome)", "array só com os nomes",
                  "const paginas = livros.reduce((acc, l) => acc + l.paginas, 0)", "soma as páginas de todos os livros (começa em 0)"),
                "Sempre passe o valor inicial no reduce: sem ele, o primeiro item vira o acumulador (e array vazio dá erro)."));
            all.Add(X(F, "JavaScript", "textContent x innerHTML  (XSS)", "Duas formas de mudar o conteúdo de um elemento. innerHTML interpreta HTML; textContent trata tudo como texto.",
                "elemento.textContent = texto\nelemento.innerHTML = html",
                A("textContent", "texto puro: seguro para dados do usuário", "innerText", "parecido, mas considera o CSS (mais lento)", "innerHTML", "interpreta tags: PERIGOSO com dados do usuário (XSS)"),
                A("recado.textContent = mensagemDoAluno", "mostra exatamente o que o aluno digitou, sem executar nada"),
                "XSS (Cross-Site Scripting): um invasor envia HTML/JS como \"comentário\" e ele roda no navegador de outras pessoas."));
            all.Add(X(F, "JavaScript", "localStorage + JSON", "Guarda dados no navegador entre visitas. Só aceita texto: objetos e arrays precisam ser convertidos com JSON.",
                "localStorage.setItem(\"chave\", JSON.stringify(valor))\nconst valor = JSON.parse(localStorage.getItem(\"chave\"))",
                A("setItem(chave, texto)", "salva", "getItem(chave)", "lê o texto (ou null se não existir)", "removeItem(chave)", "apaga",
                  "JSON.stringify(obj)", "objeto/array → texto", "JSON.parse(texto)", "texto → objeto/array"),
                A("localStorage.setItem(\"favoritos\", JSON.stringify(livros))", "salva o array livros",
                  "const favoritos = JSON.parse(localStorage.getItem(\"favoritos\")) || []", "lê de volta; se não havia nada, começa com []"),
                "Qualquer script da página lê o localStorage: não guarde senhas nem tokens sensíveis nele."));
            all.Add(X(F, "JavaScript", "Desestruturação  { } e [ ]", "Extrai propriedades de um objeto (ou posições de um array) direto em variáveis — inclusive nos parâmetros de uma função.",
                "const { a, b } = objeto\nfunction f({ a, b }) { ... }", null,
                A("const { titulo, autor } = livro", "cria as variáveis titulo e autor", "function ficha({ titulo, ano }) { return `${titulo} (${ano})` }", "desestrutura no parâmetro"),
                "A ordem dentro das chaves não importa: vale o nome da propriedade."));
            all.Add(X(F, "JavaScript", "toFixed  (casas decimais)", "Formata um número com um número fixo de casas decimais. Devolve uma string.",
                "numero.toFixed(CASAS)", null,
                A("(7.5).toFixed(2)", "\"7.50\"", "media.toFixed(1)", "uma casa decimal"),
                "Para moeda brasileira: valor.toLocaleString(\"pt-BR\", { style: \"currency\", currency: \"BRL\" }) → \"R$ 7,50\"."));
            all.Add(X(F, "JavaScript", "submit e preventDefault", "O envio de um formulário recarrega a página por padrão. preventDefault() cancela esse comportamento para o JavaScript cuidar do envio.",
                "formulario.addEventListener(\"submit\", (e) => {\n  e.preventDefault()\n  ...\n})",
                A("\"submit\"", "evento de envio (botão ou Enter)", "e.preventDefault()", "cancela a ação padrão do navegador", "e.target", "elemento que disparou o evento"),
                A("inscricao.addEventListener(\"submit\", (ev) => {\n  ev.preventDefault()\n  salvarInscricao()\n})", "envia a inscrição sem recarregar"),
                "Ouça o submit no form (não o click no botão): Enter e a validação do navegador continuam funcionando."));
            all.Add(X(F, "JavaScript", "async/await, try/catch e response.ok", "await espera uma Promise dentro de uma função async. Erros são tratados com try/catch. Atenção: fetch só rejeita em falha de REDE; erro HTTP (404, 500) chega como resposta normal com ok = false.",
                "async function nome() {\n  try {\n    const r = await fetch(url)\n    if (!r.ok) throw new Error(\"mensagem\")\n    return await r.json()\n  } catch (e) {\n    console.error(e)\n  }\n}",
                A("async", "marca a função como assíncrona (sempre devolve Promise)", "await", "espera a Promise terminar", "response.ok", "true só para status 200–299",
                  "response.status", "o código HTTP (200, 404, 500...)", "throw new Error(\"...\")", "lança um erro (vai para o catch)", "console.error", "registra o erro em vermelho no console"),
                A("async function buscarLivros() {\n  try {\n    const r = await fetch(\"/api/livros\")\n    if (!r.ok) throw new Error(\"Erro \" + r.status)\n    return await r.json()\n  } catch (e) {\n    console.error(e)\n    return []\n  }\n}", "busca livros e, em qualquer erro, devolve lista vazia"),
                "Sem checar ok, um erro 500 com corpo HTML faz o .json() falhar com uma mensagem confusa (\"Unexpected token <\")."));
        }
    }
}
