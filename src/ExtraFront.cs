using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerQuest
{
    static partial class Challenges
    {
        // Todas as ordens possíveis de itens (atributos HTML, declarações CSS, opções de objeto).
        // Cada item pode ter variações separadas por "|" (ex.: "margin: 0 auto;|margin: 0px auto;").
        // O primeiro resultado é sempre a ordem e a variação escritas primeiro (serve de referência).
        static string[] ExtraFront_Perm(string before, string sep, string after, params string[] items)
        {
            var combos = new List<List<string>> { new List<string>() };
            foreach (var it in items)
            {
                var next = new List<List<string>>();
                foreach (var c in combos)
                    foreach (var v in it.Split('|'))
                    {
                        var n = new List<string>(c);
                        n.Add(v);
                        next.Add(n);
                    }
                combos = next;
            }
            var res = new List<string>();
            foreach (var c in combos)
                foreach (var p in ExtraFront_Orders(c)) res.Add(before + string.Join(sep, p) + after);
            return res.Distinct().ToArray();
        }

        static List<List<string>> ExtraFront_Orders(List<string> items)
        {
            var res = new List<List<string>>();
            if (items.Count <= 1) { res.Add(new List<string>(items)); return res; }
            for (int i = 0; i < items.Count; i++)
            {
                var rest = new List<string>(items);
                rest.RemoveAt(i);
                foreach (var r in ExtraFront_Orders(rest))
                {
                    var l = new List<string> { items[i] };
                    l.AddRange(r);
                    res.Add(l);
                }
            }
            return res;
        }

        // Regra CSS: a 1ª resposta é a forma "bonita" (várias linhas); depois, as declarações em qualquer ordem.
        static string[] ExtraFront_Css(string sel, params string[] decls)
        {
            string pretty = sel + " {\n    " + string.Join("\n    ", decls.Select(d => d.Split('|')[0])) + "\n}";
            return new[] { pretty }.Concat(ExtraFront_Perm(sel + " { ", " ", " }", decls)).Distinct().ToArray();
        }

        static partial void ExtraFront(List<Level> list)
        {
            const string F = "front";

            // ═══════════════════════════════ INICIANTE ═══════════════════════════════
            list.Add(C(F, 0, "html", "Nome na aba",
                "A aba do navegador mostra \"Sem título\" e o Google exibe o endereço do site em vez do nome do Café 365.",
                "Escreva a tag do título do documento (a que aparece na aba do navegador e fica dentro do head) com o texto: Café 365",
                A("<title>Café 365</title>"),
                A("Não é o h1: é uma tag que fica no <head> e aparece na aba do navegador.", "<title>...</title>"),
                A("<title>", "Título do documento: aparece na aba, nos favoritos e no resultado do Google.", "Café 365", "Texto curto e único para cada página."),
                "Cada página deve ter um title próprio e descritivo, ex.: \"Cardápio – Café 365\". É a primeira coisa que o leitor de tela anuncia ao abrir a página."));

            list.Add(C(F, 0, "code", "Fundo creme",
                "A Camila recebeu do time de design a cor de fundo oficial do Café 365: um creme bem claro, #fff8e7.",
                "Escreva a regra CSS que deixa o fundo do body com a cor #fff8e7, usando a propriedade background-color.",
                ExtraFront_Css("body", "background-color: #fff8e7;|background: #fff8e7;"),
                A("A propriedade de cor de fundo começa com background-.", "body { background-color: ____; }"),
                A("body", "Seletor do corpo: a página inteira.", "background-color: #fff8e7;", "Cor de fundo em hexadecimal: #RRGGBB (vermelho, verde e azul)."),
                "Confira o contraste entre texto e fundo (o DevTools mostra a taxa): texto claro em fundo creme fica ilegível para muita gente."));

            list.Add(C(F, 0, "code", "Contador de xícaras",
                "O app conta quantas xícaras o cliente já pediu hoje. O número começa em zero e vai aumentar a cada pedido.",
                "Declare a variável xicaras com o valor inicial 0, usando a palavra-chave moderna para um valor que VAI mudar.",
                A("let xicaras = 0"),
                A("const não deixa reatribuir; existe outra palavra moderna para variáveis que mudam (e não é var).", "let ______ = 0"),
                A("let", "Declara uma variável que pode receber outro valor depois (xicaras = xicaras + 1).", "= 0", "Valor inicial."),
                "Comece sempre com const e troque para let só quando precisar reatribuir. Evite var: o escopo dele é confuso."));

            list.Add(C(F, 0, "html", "Lista de bebidas",
                "O cardápio do balcão lista as bebidas uma embaixo da outra separadas com <br>. A Camila quer uma lista de verdade.",
                "Escreva uma lista não ordenada (com marcadores) com dois itens: Expresso e Latte, nesta ordem.",
                A("<ul>\n    <li>Expresso</li>\n    <li>Latte</li>\n</ul>"),
                A("Lista com marcadores é a unordered list; cada item é um list item.", "<ul><li>...</li><li>...</li></ul>"),
                A("<ul>", "Lista não ordenada (bolinhas): a ordem dos itens não importa.", "<li>", "Cada item da lista."),
                "Menus, cardápios e listas de produtos são listas: o leitor de tela anuncia \"lista, 2 itens\" e ajuda a navegar."));

            list.Add(C(F, 0, "code", "Botão arredondado",
                "Os botões do Café 365 têm cantos retos e o novo design pede cantos arredondados.",
                "Escreva a regra CSS que arredonda os cantos da classe .botao em 8px.",
                ExtraFront_Css(".botao", "border-radius: 8px;"),
                A("A propriedade de cantos arredondados tem radius (raio) no nome.", ".botao { border-______: 8px; }"),
                A(".botao", "Seletor de classe (com ponto).", "border-radius: 8px;", "Raio de 8px nos quatro cantos."),
                "border-radius: 50% num elemento quadrado vira um círculo — ótimo para fotos de perfil."));

            list.Add(C(F, 0, "code", "Lista de cafés",
                "A Yuki vai montar a vitrine com JavaScript e precisa dos nomes dos cafés guardados juntos, em ordem.",
                "Crie a constante cafes com um array de três textos: Expresso, Latte e Cappuccino (nesta ordem).",
                A("const cafes = [\"Expresso\", \"Latte\", \"Cappuccino\"]"),
                A("Arrays são escritos entre colchetes, com os itens separados por vírgula.", "const cafes = [\"...\", \"...\", \"...\"]"),
                A("[ ... ]", "Cria um array (lista ordenada).", "\"Expresso\"", "Cada texto entre aspas; a posição 0 é o primeiro item."),
                "Arrays começam no índice 0: cafes[0] é \"Expresso\" e cafes[2] é \"Cappuccino\"."));

            list.Add(C(F, 0, "html", "Aviso importante",
                "No banner \"Promoção válida só hoje\", a palavra hoje precisa ser marcada como importante — não só ficar em negrito visual.",
                "Escreva um parágrafo com o texto Promoção válida só hoje, marcando a palavra hoje como texto importante (strong).",
                A("<p>Promoção válida só <strong>hoje</strong></p>"),
                A("A tag de importância é diferente da de negrito puro (b).", "<p>... <strong>hoje</strong></p>"),
                A("<p>", "Parágrafo que envolve o texto.", "<strong>", "Importância forte: aparece em negrito e tem significado para leitores de tela e buscadores."),
                "<strong> = importante e <em> = ênfase. <b> e <i> só mudam a aparência, sem significado."));

            list.Add(C(F, 0, "code", "Espaço interno",
                "O texto dos cards está grudado na borda. A Camila explica o box model: o espaço DENTRO da borda é o padding; o de FORA, a margin.",
                "Escreva a regra CSS que dá 16px de espaço interno (padding) em todos os lados da classe .card.",
                ExtraFront_Css(".card", "padding: 16px;"),
                A("Espaço interno = entre o conteúdo e a borda do elemento.", ".card { _______: 16px; }"),
                A(".card", "Seletor de classe.", "padding: 16px;", "16px de espaço interno nos quatro lados."),
                "Box model: conteúdo → padding → border → margin. Com dois valores, padding: 8px 16px é vertical e horizontal."));

            list.Add(C(F, 0, "code", "Quantos itens?",
                "O selo do carrinho vai mostrar quantos cafés existem no array cafes. A Yuki quer conferir o número no console antes.",
                "Mostre no console a quantidade de itens do array cafes.",
                A("console.log(cafes.length)"),
                A("Todo array tem uma propriedade (não é método!) com o número de itens.", "console.log(cafes.______)"),
                A("cafes.length", "Quantidade de itens do array (sem parênteses: é uma propriedade).", "console.log(...)", "Mostra o valor no console."),
                "O último item fica sempre em cafes[cafes.length - 1] — ou, no JavaScript moderno, cafes.at(-1)."));

            list.Add(C(F, 0, "html", "Passo a passo",
                "A receita do cold brew no blog do Café 365 tem passos que precisam ser seguidos em ordem.",
                "Escreva uma lista ordenada (numerada) com dois itens: Moer o café e Esperar 12 horas, nesta ordem.",
                A("<ol>\n    <li>Moer o café</li>\n    <li>Esperar 12 horas</li>\n</ol>"),
                A("Lista numerada é a ordered list.", "<ol><li>...</li><li>...</li></ol>"),
                A("<ol>", "Lista ordenada: o navegador numera 1, 2, 3...", "<li>", "Cada passo da receita."),
                "Use <ol> quando a ordem importa (passos, ranking) e <ul> quando não importa."));

            list.Add(C(F, 0, "code", "Título por id",
                "Só o título do banner de promoção (o elemento com id destaque) precisa ficar maior. A Camila pede o tamanho em rem, que respeita a fonte escolhida no navegador.",
                "Escreva a regra CSS para o elemento de id destaque com font-size de 2rem.",
                ExtraFront_Css("#destaque", "font-size: 2rem;"),
                A("Seletor de id começa com # (o de classe é com ponto).", "#destaque { font-size: ____; }"),
                A("#destaque", "Seleciona o elemento com id=\"destaque\".", "font-size: 2rem;", "2 × o tamanho da fonte da raiz (normalmente 16px → 32px)."),
                "Prefira rem a px nas fontes: se a pessoa aumenta a fonte padrão do navegador, o site acompanha."));

            list.Add(C(F, 0, "code", "Mais um café",
                "O barista lançou o Mocha e ele precisa entrar no fim do array cafes.",
                "Acrescente o texto Mocha ao final do array cafes.",
                A("cafes.push(\"Mocha\")"),
                A("Arrays têm um método que acrescenta itens no final.", "cafes.____(\"Mocha\")"),
                A("cafes.push(...)", "Acrescenta no fim do array e devolve o novo tamanho.", "\"Mocha\"", "O item novo."),
                "push muda o array original. unshift acrescenta no início; pop e shift removem do fim e do início."));

            list.Add(C(F, 0, "html", "Menu de navegação",
                "O topo do Café 365 tem os links soltos numa <div>. A Nina explica que o leitor de tela tem atalho para pular direto ao menu — mas só se ele estiver na tag certa.",
                "Escreva a tag semântica de navegação contendo um único link para /cardapio com o texto Cardápio.",
                A("<nav>\n    <a href=\"/cardapio\">Cardápio</a>\n</nav>"),
                A("O HTML5 tem uma tag própria para blocos de navegação (abreviação de navigation).", "<nav><a href=\"...\">Cardápio</a></nav>"),
                A("<nav>", "Marca o bloco como navegação: leitores de tela listam e pulam direto para ele.", "<a href=\"/cardapio\">", "Link relativo: vai para /cardapio no mesmo site."),
                "Use <nav> para os menus principais — não para todo grupo de links (os do rodapé, por exemplo, nem sempre precisam)."));

            list.Add(C(F, 0, "code", "Link com hover",
                "A Camila quer um retorno visual: quando o mouse passa por cima de um link, ele fica laranja.",
                "Escreva a regra CSS que deixa os links (a) com a cor de texto orange quando o mouse estiver sobre eles.",
                ExtraFront_Css("a:hover", "color: orange;"),
                A("Pseudo-classes indicam um estado do elemento e vêm depois do seletor, com dois-pontos.", "a:______ { color: orange; }"),
                A("a:hover", "Vale só enquanto o mouse está sobre o link.", "color: orange;", "Cor do texto nesse estado."),
                "Hover não existe no toque do celular: nada importante pode depender só dele. Estilize :focus-visible também, para quem usa teclado."));

            list.Add(C(F, 0, "code", "Objeto do pedido",
                "Cada pedido tem o nome do café e a quantidade. A Yuki quer as duas informações juntas num objeto.",
                "Crie a constante pedido com um objeto de duas propriedades: cafe com o texto Latte e qtd com o número 2.",
                ExtraFront_Perm("const pedido = { ", ", ", " }", "cafe: \"Latte\"", "qtd: 2"),
                A("Objetos ficam entre chaves, no formato chave: valor, separados por vírgula.", "const pedido = { cafe: \"...\", qtd: ... }"),
                A("{ }", "Cria um objeto.", "cafe: \"Latte\"", "Propriedade de texto (com aspas).", "qtd: 2", "Propriedade numérica (sem aspas)."),
                "Leia as propriedades com ponto: pedido.cafe. Cuidado: número entre aspas vira texto, e \"2\" + 1 dá \"21\"!"));

            list.Add(C(F, 0, "html", "Seção de promoções",
                "A página inicial tem um bloco de promoções com título próprio. A Nina pede que ele seja uma seção temática, não uma div.",
                "Escreva uma section contendo um título h2 com o texto Promoções.",
                A("<section>\n    <h2>Promoções</h2>\n</section>"),
                A("A tag de seção temática do HTML5 é \"seção\" em inglês.", "<section><h2>...</h2></section>"),
                A("<section>", "Agrupa conteúdo de um mesmo tema; normalmente tem um título.", "<h2>", "Título da seção (abaixo do h1 da página)."),
                "Se o bloco faz sentido sozinho (um post, um card de produto), use <article>; se é uma parte temática da página, <section>."));

            list.Add(C(F, 0, "code", "Imagem que não vaza",
                "As fotos grandes da cafeteria estouram a largura da tela do celular e criam uma rolagem lateral.",
                "Escreva a regra CSS que limita a largura MÁXIMA de todas as imagens (img) a 100% do espaço disponível.",
                ExtraFront_Css("img", "max-width: 100%;").Concat(ExtraFront_Perm("img { ", " ", " }", "max-width: 100%;", "height: auto;")).ToArray(),
                A("Existe uma propriedade de largura máxima; % é relativo ao elemento pai.", "img { max-width: ____; }"),
                A("img", "Todas as imagens.", "max-width: 100%;", "Nunca fica mais larga que o pai, mas pode ser menor."),
                "Junto com height: auto, a imagem encolhe mantendo a proporção. Essa regra está em quase todo CSS base."));

            list.Add(C(F, 0, "code", "Trocando o texto",
                "O título da vitrine já está na constante titulo (pego com querySelector) e deve mudar para o nome da promoção do dia.",
                "Troque o texto do elemento titulo para Cardápio do dia, usando a propriedade textContent.",
                A("titulo.textContent = \"Cardápio do dia\""),
                A("Elementos têm uma propriedade com o texto que está dentro deles.", "titulo.textContent = \"...\""),
                A("titulo.textContent", "O texto dentro do elemento.", "= \"Cardápio do dia\"", "Substitui o texto antigo pelo novo."),
                "textContent trata tudo como texto puro — é a escolha segura sempre que o conteúdo não precisa ter tags."));

            list.Add(C(F, 0, "html", "Campo com dica",
                "No cadastro, o campo de nome está vazio e as pessoas não sabem o que escrever. A Camila quer um texto de exemplo apagadinho dentro do campo.",
                "Escreva um input do tipo text com o texto de exemplo (placeholder) Seu nome completo.",
                ExtraFront_Perm("<input ", " ", ">", "type=\"text\"", "placeholder=\"Seu nome completo\""),
                A("Existe um atributo que mostra um texto de exemplo que some quando a pessoa digita.", "<input type=\"text\" placeholder=\"...\">"),
                A("type=\"text\"", "Campo de texto comum.", "placeholder=\"Seu nome completo\"", "Texto de exemplo exibido enquanto o campo está vazio."),
                "placeholder é só um exemplo: ele não substitui o <label> (some ao digitar e costuma ter pouco contraste)."));

            list.Add(C(F, 0, "code", "Itens com respiro",
                "O .menu já usa display: flex, mas os links ficaram grudados. A Camila não quer margem em cada item: quer o espaço definido no próprio container.",
                "Escreva a regra CSS que adiciona 16px de espaço entre os itens do .menu usando a propriedade gap.",
                ExtraFront_Css(".menu", "gap: 16px;").Concat(ExtraFront_Perm(".menu { ", " ", " }", "display: flex;", "gap: 16px;")).ToArray(),
                A("No flex e no grid, uma propriedade curtinha define o espaço ENTRE os itens.", ".menu { ___: 16px; }"),
                A(".menu", "O container flex.", "gap: 16px;", "Espaço só entre os itens (não nas pontas)."),
                "gap funciona no flex e no grid e evita a gambiarra de tirar a margem do último item."));

            list.Add(C(F, 0, "code", "Botão pela classe",
                "O botão de comprar não tem id, só a classe comprar. A Yuki precisa dele no JavaScript.",
                "Guarde na constante botao o primeiro elemento com a classe comprar, usando document.querySelector.",
                A("const botao = document.querySelector(\".comprar\")"),
                A("querySelector recebe um seletor CSS — e em CSS a classe começa com...", "document.querySelector(\".______\")"),
                A("document.querySelector", "Devolve o PRIMEIRO elemento que casa com o seletor (ou null).", "\".comprar\"", "Seletor de classe, igual ao do CSS."),
                "Para pegar TODOS os elementos da classe, use querySelectorAll (devolve uma lista que aceita forEach)."));

            list.Add(C(F, 0, "html", "Botão de pedido",
                "Na tela do cardápio, alguém fez um \"botão\" com <div onclick>. A Nina mostra que ele não recebe foco pelo Tab nem funciona com Enter.",
                "Escreva um botão de verdade com o texto Pedir agora.",
                A("<button>Pedir agora</button>", "<button type=\"button\">Pedir agora</button>"),
                A("Existe uma tag própria para botões, que já funciona com o teclado.", "<button>...</button>"),
                A("<button>", "Botão nativo: recebe foco com Tab e é acionado com Enter ou Espaço.", "Pedir agora", "Texto que aparece e que o leitor de tela anuncia."),
                "Dentro de um <form>, o button é type=\"submit\" por padrão; use type=\"button\" quando ele não deve enviar o formulário."));

            list.Add(C(F, 0, "code", "Menu aberto",
                "Ao tocar no ícone, o menu (constante menu) deve aparecer. O CSS já tem a regra .aberto { display: block; }.",
                "Acrescente a classe aberto ao elemento menu usando classList.",
                A("menu.classList.add(\"aberto\")"),
                A("classList tem métodos para acrescentar, remover e alternar classes.", "menu.classList.___(\"aberto\")"),
                A("menu.classList", "A lista de classes do elemento.", ".add(\"aberto\")", "Acrescenta a classe (sem o ponto!) mantendo as outras."),
                "Deixe a aparência no CSS e use o JavaScript só para trocar classes: classList.add, remove, toggle e contains."));

            var viewport = ExtraFront_Perm("<meta ", " ", ">", "name=\"viewport\"", "content=\"width=device-width, initial-scale=1\"|content=\"width=device-width, initial-scale=1.0\"");
            list.Add(C(F, 0, "html", "Site no celular",
                "No celular, o site do Café 365 aparece minúsculo, como uma página de computador encolhida. A Camila diz que falta uma linha no <head>.",
                "Escreva a tag meta de viewport (name viewport) com o conteúdo width=device-width, initial-scale=1",
                viewport,
                A("É uma tag <meta> com name=\"viewport\" e o atributo content.", "<meta name=\"viewport\" content=\"width=..., initial-scale=1\">"),
                A("name=\"viewport\"", "Configura a área visível no celular.", "width=device-width", "A largura da página passa a ser a largura real da tela.",
                  "initial-scale=1", "Começa sem zoom."),
                "Sem essa tag as media queries de celular não funcionam direito. Nunca use user-scalable=no: impede o zoom de quem precisa."));

            // ═══════════════════════════════ INTERMEDIÁRIO ═══════════════════════════════
            list.Add(C(F, 1, "code", "Menu nas pontas",
                "No topo (classe .topo), o logo deve ficar à esquerda, o menu à direita, e os dois alinhados verticalmente ao centro.",
                "Escreva a regra CSS da classe .topo com display flex, justify-content space-between e align-items center.",
                ExtraFront_Css(".topo", "display: flex;", "justify-content: space-between;", "align-items: center;"),
                A("justify-content distribui no eixo principal (horizontal); align-items alinha no eixo cruzado (vertical).", ".topo { display: flex; justify-content: space-between; align-items: ______; }"),
                A("display: flex;", "Coloca logo e menu lado a lado.", "justify-content: space-between;", "Empurra o primeiro item para a esquerda e o último para a direita.",
                  "align-items: center;", "Centraliza verticalmente itens de alturas diferentes."),
                "Com flex-direction: column os eixos trocam: justify-content passa a ser vertical e align-items, horizontal."));

            list.Add(C(F, 1, "code", "Só os nomes",
                "O array cafes tem objetos como { nome: \"Latte\", preco: 8.5 }. O autocompletar da busca precisa só da lista de nomes.",
                "Crie a constante nomes com o nome de cada café, usando map com uma arrow function de parâmetro c.",
                FrontMix(A("const nomes = cafes.map("), A("c => ", "(c) => "), A("c.nome", "{ return c.nome }"), A(")")),
                A("map transforma cada item do array e devolve um array novo, do mesmo tamanho.", "const nomes = cafes.map(c => c.____)"),
                A("cafes.map(...)", "Cria um array novo com o resultado da função para cada item.", "c => c.nome", "De cada objeto, fica só o nome."),
                "map devolve sempre o mesmo número de itens. Para escolher itens use filter; para achar um só, find."));

            list.Add(C(F, 1, "html", "Campo de mensagem",
                "O formulário Fale Conosco precisa de um campo para textos longos, com rótulo, que não pode ser enviado vazio.",
                "Escreva o label com for mensagem e o texto Mensagem; logo depois, uma caixa de texto de várias linhas (textarea) com id mensagem e obrigatória (required).",
                ExtraFront_Perm("<label for=\"mensagem\">Mensagem</label>\n<textarea ", " ", "></textarea>", "id=\"mensagem\"", "required"),
                A("Para texto de várias linhas o input não serve: existe uma tag própria, que tem fechamento.", "<label for=\"mensagem\">Mensagem</label><textarea id=\"mensagem\" required></textarea>"),
                A("<textarea>", "Campo de várias linhas; diferente do input, tem tag de fechamento.", "id=\"mensagem\"", "Liga o campo ao for do label.", "required", "Impede o envio com o campo vazio."),
                "Limite o tamanho com maxlength e valide também no servidor: required no HTML é conforto para o usuário, não segurança."));

            list.Add(C(F, 1, "code", "Conteúdo centralizado",
                "Em monitores largos, o texto do blog ocupa a tela inteira e as linhas ficam compridas demais. A Camila quer um container de no máximo 960px, centralizado.",
                "Escreva a regra CSS da classe .container com max-width de 960px e margin 0 auto.",
                ExtraFront_Css(".container", "max-width: 960px;", "margin: 0 auto;|margin: 0px auto;"),
                A("max-width limita a largura; margin auto nas laterais divide a sobra igualmente.", ".container { max-width: 960px; margin: 0 ____; }"),
                A("max-width: 960px;", "Nunca passa de 960px, mas encolhe em telas menores.", "margin: 0 auto;", "0 em cima e embaixo, auto nas laterais: centraliza o bloco."),
                "Linhas confortáveis de ler têm de 50 a 75 caracteres; max-width: 65ch limita pela largura das letras."));

            list.Add(C(F, 1, "code", "Desmontando os pedidos",
                "A Yuki percorre o array pedidos (objetos como { cafe: \"Latte\", qtd: 2 }) escrevendo pedido.cafe e pedido.qtd a cada linha. Dá para tirar as duas propriedades já na declaração do laço.",
                "Percorra pedidos com for...of, desestruturando cafe e qtd direto na declaração (const { cafe, qtd }), e mostre no console qtd e cafe, nesta ordem: console.log(qtd, cafe).",
                A("for (const { cafe, qtd } of pedidos) {\n    console.log(qtd, cafe)\n}")
                    .Concat(FrontMix(A("for (const "), A("{ cafe, qtd }", "{ qtd, cafe }"), A(" of pedidos) "), A("console.log(qtd, cafe)", "{ console.log(qtd, cafe) }"))).ToArray(),
                A("A desestruturação de objeto usa chaves no lugar do nome da variável — e isso vale também na declaração do for...of.", "for (const { ____, ____ } of pedidos) { console.log(qtd, cafe) }"),
                A("for (... of pedidos)", "Percorre cada item do array, do primeiro ao último.", "const { cafe, qtd }", "Desestrutura cada pedido: cria cafe e qtd com os valores das propriedades de mesmo nome.",
                  "console.log(qtd, cafe)", "Mostra os dois valores, ex.: 2 Latte."),
                "A desestruturação também aceita renomear e valor padrão: const { qtd: quantidade = 1 } = pedido."));

            list.Add(C(F, 1, "html", "Card do produto",
                "Cada produto da vitrine é um bloco independente, que faria sentido sozinho até num app de entregas. A Nina sugere a tag article.",
                "Escreva um article contendo um h2 com o texto Latte e, depois, um parágrafo com o texto R$ 8,50",
                A("<article>\n    <h2>Latte</h2>\n    <p>R$ 8,50</p>\n</article>"),
                A("A tag para conteúdo independente e reutilizável é article.", "<article><h2>...</h2><p>...</p></article>"),
                A("<article>", "Conteúdo completo e independente (card, post, notícia).", "<h2>Latte</h2>", "Título do card.", "<p>R$ 8,50</p>", "Preço num parágrafo."),
                "Teste do article: se o bloco fosse copiado para outro site e ainda fizesse sentido, é um article."));

            list.Add(C(F, 1, "code", "Caixas sem surpresa",
                "A Yuki colocou width: 300px e padding: 20px num card, e ele ficou com 340px. A Camila explica: por padrão, o navegador soma padding e borda à largura.",
                "Escreva a regra que aplica box-sizing border-box a todos os elementos e aos pseudo-elementos (seletor: *, *::before, *::after).",
                ExtraFront_Css("*, *::before, *::after", "box-sizing: border-box;")
                    .Concat(ExtraFront_Perm("", ", ", " { box-sizing: border-box; }", "*", "*::before|*:before", "*::after|*:after")).Distinct().ToArray(),
                A("A propriedade é box-sizing; o valor que inclui padding e borda na largura é border-box.", "*, *::before, *::after { box-sizing: ________; }"),
                A("*", "Seletor universal: todos os elementos.", "*::before, *::after", "Pseudo-elementos (conteúdo gerado pelo CSS).",
                  "box-sizing: border-box;", "A width passa a incluir padding e borda: 300px é 300px."),
                "Essa regra abre quase todo projeto moderno (o \"reset\"): deixa as contas do layout previsíveis."));

            list.Add(C(F, 1, "code", "Abre e fecha",
                "O botão de menu (constante botao) deve abrir o menu se estiver fechado e fechar se estiver aberto. A classe aberto já existe no CSS.",
                "Registre no botao um ouvinte de click com uma arrow function sem parâmetros que alterna a classe aberto no elemento menu, usando classList.toggle.",
                FrontMix(A("botao.addEventListener(\"click\", "), A("() => menu.classList.toggle(\"aberto\")", "() => { menu.classList.toggle(\"aberto\") }"), A(")")),
                A("classList tem um método que acrescenta a classe se ela não existe e remove se existe.", "botao.addEventListener(\"click\", () => menu.classList.______(\"aberto\"))"),
                A("addEventListener(\"click\", ...)", "Executa a função a cada clique.", "() => ...", "Arrow function sem parâmetros.",
                  "classList.toggle(\"aberto\")", "Liga e desliga a classe a cada chamada."),
                "toggle devolve true quando a classe ficou ligada — útil para atualizar o aria-expanded do botão junto."));

            list.Add(C(F, 1, "html", "Menu em lista",
                "O leitor de tela anuncia o menu como um amontoado de links. A Nina explica o padrão: nav com uma lista de links dentro; aí ele diz \"lista, 2 itens\".",
                "Escreva um nav contendo uma ul com dois itens: um link para / com o texto Início e um link para /cardapio com o texto Cardápio.",
                A("<nav>\n    <ul>\n        <li><a href=\"/\">Início</a></li>\n        <li><a href=\"/cardapio\">Cardápio</a></li>\n    </ul>\n</nav>"),
                A("A estrutura é nav > ul > li > a.", "<nav><ul><li><a href=\"/\">Início</a></li><li>...</li></ul></nav>"),
                A("<nav>", "Região de navegação.", "<ul>", "Lista de links: o leitor anuncia quantos são.", "<li><a href=\"/\">Início</a></li>", "Cada item contém um link."),
                "Marque a página atual no menu com aria-current=\"page\": o leitor de tela anuncia \"página atual\"."));

            list.Add(C(F, 1, "code", "Card com sombra",
                "Os cards parecem colados no fundo. A Camila quer cantos de 12px e uma sombra suave embaixo.",
                "Escreva a regra CSS da classe .card com border-radius 12px e box-shadow 0 2px 8px rgba(0, 0, 0, 0.2).",
                ExtraFront_Css(".card", "border-radius: 12px;", "box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);|box-shadow: 0 2px 8px rgba(0, 0, 0, .2);|box-shadow: 0px 2px 8px rgba(0, 0, 0, 0.2);"),
                A("box-shadow recebe: deslocamento x, deslocamento y, desfoque e cor.", ".card { border-radius: 12px; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2); }"),
                A("border-radius: 12px;", "Cantos arredondados.", "box-shadow: 0 2px 8px", "Sombra sem deslocamento lateral, 2px para baixo e 8px de desfoque.",
                  "rgba(0, 0, 0, 0.2)", "Preto com 20% de opacidade: sombra suave."),
                "Sombras pesadas cansam a vista: prefira opacidades baixas. No tema escuro, bordas ou fundos um pouco mais claros funcionam melhor que sombra."));

            list.Add(C(F, 1, "code", "Achando o Latte",
                "Ao clicar em \"Latte\", a página de detalhes precisa do objeto completo desse café, que está no array cafes.",
                "Crie a constante latte com o PRIMEIRO item de cafes cujo nome é exatamente Latte, usando find com uma arrow function de parâmetro c e comparação estrita (===).",
                FrontMix(A("const latte = cafes.find("), A("c => ", "(c) => "),
                         A("c.nome === \"Latte\"", "\"Latte\" === c.nome", "{ return c.nome === \"Latte\" }"), A(")")),
                A("find devolve o primeiro item para o qual a função retorna true (ou undefined se nenhum servir).", "const latte = cafes.find(c => c.nome === \"...\")"),
                A("cafes.find(...)", "Para no primeiro item encontrado e devolve o próprio objeto (não um array).", "c.nome === \"Latte\"", "Comparação estrita: valor e tipo iguais."),
                "filter devolve SEMPRE um array (mesmo com um item só); find devolve o item ou undefined — trate o caso de não encontrar."));

            list.Add(C(F, 1, "html", "Senha segura",
                "O cadastro aceita senhas de 3 caracteres. Enquanto o back-end não valida, o navegador já pode barrar senhas curtas.",
                "Escreva um input do tipo password com id senha, mínimo de 8 caracteres (minlength) e obrigatório (required).",
                ExtraFront_Perm("<input ", " ", ">", "type=\"password\"", "id=\"senha\"", "minlength=\"8\"", "required"),
                A("Atributos de validação: required (obrigatório) e minlength (tamanho mínimo).", "<input type=\"password\" id=\"senha\" minlength=\"8\" required>"),
                A("type=\"password\"", "Esconde os caracteres digitados.", "minlength=\"8\"", "Bloqueia o envio com menos de 8 caracteres.", "required", "Não deixa enviar vazio."),
                "Validação no HTML ajuda o usuário, mas não é segurança: o servidor precisa validar de novo e guardar só o hash da senha."));

            list.Add(C(F, 1, "code", "Topo grudado",
                "Ao rolar o cardápio, o topo com o carrinho some. A Camila quer que o .topo grude no alto da tela quando chegar lá.",
                "Escreva a regra CSS da classe .topo com position sticky e top 0.",
                ExtraFront_Css(".topo", "position: sticky;", "top: 0;|top: 0px;"),
                A("Existe um valor de position que é \"relativo até grudar\".", ".topo { position: ______; top: 0; }"),
                A("position: sticky;", "Rola normalmente até atingir o limite e então gruda.", "top: 0;", "O limite: gruda encostado no topo da tela."),
                "sticky não funciona se algum ancestral tiver overflow: hidden. E diferente do fixed, ele não sai do fluxo da página."));

            list.Add(C(F, 1, "code", "Tema salvo",
                "O site tem tema claro e escuro, e a escolha fica salva no localStorage na chave tema. Mas na primeira visita ainda não existe nada salvo lá.",
                "Crie a constante tema lendo a chave tema do localStorage; se não houver nada salvo, use o texto claro (operador ||).",
                A("const tema = localStorage.getItem(\"tema\") || \"claro\"", "const tema = localStorage.getItem(\"tema\") ?? \"claro\""),
                A("getItem devolve null quando a chave não existe; o || escolhe o valor da direita nesse caso.", "const tema = localStorage.getItem(\"tema\") || \"...\""),
                A("localStorage.getItem(\"tema\")", "Lê o texto salvo (ou null).", "|| \"claro\"", "Valor padrão quando não há nada salvo."),
                "Aqui não precisa de JSON.parse: o valor já é um texto simples. JSON só entra quando você salva objetos ou arrays."));

            var opcoes = A("<option>P</option><option>M</option><option>G</option>", "<option value=\"P\">P</option><option value=\"M\">M</option><option value=\"G\">G</option>");
            list.Add(C(F, 1, "html", "Escolha o tamanho",
                "No pedido, o cliente escolhe o tamanho do copo. São só três opções fixas, então a Camila prefere uma lista suspensa a um campo de texto.",
                "Escreva o label com for tamanho e o texto Tamanho; depois, um select com id tamanho contendo três option com os textos P, M e G (nesta ordem).",
                A("<label for=\"tamanho\">Tamanho</label>\n<select id=\"tamanho\">\n    <option>P</option>\n    <option>M</option>\n    <option>G</option>\n</select>")
                    .Concat(FrontMix(A("<label for=\"tamanho\">Tamanho</label><select id=\"tamanho\">"), opcoes, A("</select>"))).ToArray(),
                A("Lista suspensa é select; cada escolha é um option.", "<select id=\"tamanho\"><option>P</option>...</select>"),
                A("<select id=\"tamanho\">", "Lista suspensa, ligada ao label pelo id.", "<option>P</option>", "Cada opção; sem value, o próprio texto é o valor enviado."),
                "Para 2 ou 3 opções, botões de rádio (input type=\"radio\") costumam ser mais rápidos de usar que um select: tudo fica visível."));

            list.Add(C(F, 1, "code", "Botão com transição",
                "O fundo do .botao muda de cor no hover de um jeito seco. A Camila quer a troca suave, em 0.3 segundo.",
                "Escreva duas regras: .botao com transition de background-color em 0.3s; e .botao:hover com background-color #6f4e37.",
                A(".botao {\n    transition: background-color 0.3s;\n}\n.botao:hover {\n    background-color: #6f4e37;\n}")
                    .Concat(FrontMix(A(".botao { transition: background-color "), A("0.3s", ".3s", "300ms"), A("; } .botao:hover { background-color: #6f4e37; }"))).ToArray(),
                A("A transition fica na regra normal (não no :hover): assim a volta também é suave.", ".botao { transition: background-color 0.3s; }  e  .botao:hover { background-color: ...; }"),
                A("transition: background-color 0.3s;", "Anima as mudanças de background-color durante 0.3 segundo.", ".botao:hover", "Estado com o mouse por cima.",
                  "background-color: #6f4e37;", "Novo fundo (o marrom do Café 365)."),
                "Anime de preferência transform e opacity, que são leves para o navegador. Evite transition: all."));

            list.Add(C(F, 1, "code", "Busca no cardápio",
                "O texto digitado na busca já está na constante busca, em minúsculas. Quem digita \"lat\" precisa encontrar o \"Latte\" do array cafes (objetos com nome).",
                "Crie a constante achados filtrando cafes pelos itens cujo nome, convertido para minúsculas com toLowerCase(), inclui o texto busca (includes). Use uma arrow function de parâmetro c.",
                FrontMix(A("const achados = cafes.filter("), A("c => ", "(c) => "),
                         A("c.nome.toLowerCase().includes(busca)", "{ return c.nome.toLowerCase().includes(busca) }"), A(")")),
                A("Strings têm toLowerCase() (tudo minúsculo) e includes(texto) (contém?).", "cafes.filter(c => c.nome.toLowerCase().includes(____))"),
                A("c.nome.toLowerCase()", "\"Latte\" vira \"latte\": a busca ignora maiúsculas.", ".includes(busca)", "true se o nome contém o texto em qualquer posição.",
                  "cafes.filter(...)", "Mantém só os cafés que passaram no teste."),
                "Para ignorar acentos também (\"cafe\" achar \"café\"), normalize as duas strings com normalize(\"NFD\") e remova os acentos antes de comparar."));

            list.Add(C(F, 1, "html", "Foto com legenda",
                "O blog mostra a foto da loja nova com uma legenda embaixo. A Nina quer a legenda ligada semanticamente à imagem.",
                "Escreva um figure contendo a imagem loja.jpg com alt Fachada da loja e, depois, a legenda (figcaption) com o texto Nossa loja na Paulista.",
                ExtraFront_Perm("<figure>\n    <img ", " ", ">\n    <figcaption>Nossa loja na Paulista</figcaption>\n</figure>", "src=\"loja.jpg\"", "alt=\"Fachada da loja\""),
                A("figure agrupa uma mídia e sua legenda; a legenda é figcaption.", "<figure><img src=\"...\" alt=\"...\"><figcaption>...</figcaption></figure>"),
                A("<figure>", "Agrupa imagem e legenda como uma unidade.", "<img src=\"loja.jpg\" alt=\"Fachada da loja\">", "A imagem, com texto alternativo (o que se vê nela).",
                  "<figcaption>", "A legenda (informação extra), anunciada junto com a figura."),
                "alt e figcaption têm papéis diferentes: o alt substitui a imagem; a legenda complementa. Não repita o mesmo texto nos dois."));

            list.Add(C(F, 1, "code", "Linhas listradas",
                "Na tabela de preços os olhos se perdem entre as linhas. A Camila quer as células com respiro e as linhas pares com fundo cinza.",
                "Escreva duas regras: td com padding de 8px; e as linhas pares (tr:nth-child(even)) com background-color #f2f2f2.",
                A("td {\n    padding: 8px;\n}\ntr:nth-child(even) {\n    background-color: #f2f2f2;\n}")
                    .Concat(FrontMix(A("td { padding: 8px; } "), A("tr:nth-child(even)", "tr:nth-child(2n)"), A(" { background-color: #f2f2f2; }", " { background: #f2f2f2; }")))
                    .Concat(FrontMix(A("tr:nth-child(even)", "tr:nth-child(2n)"), A(" { background-color: #f2f2f2; }", " { background: #f2f2f2; }"), A(" td { padding: 8px; }"))).ToArray(),
                A(":nth-child() escolhe elementos pela posição entre os irmãos; aceita números, odd (ímpares) e even (pares).", "td { padding: 8px; }  e  tr:nth-child(even) { background-color: #f2f2f2; }"),
                A("td { padding: 8px; }", "Espaço interno em cada célula.", "tr:nth-child(even)", "Seleciona a 2ª, a 4ª, a 6ª... linha.",
                  "background-color: #f2f2f2;", "Fundo cinza bem claro nas linhas pares."),
                ":nth-child(3n) pega de 3 em 3; :first-child e :last-child pegam as pontas."));

            list.Add(C(F, 1, "code", "Buscar com Enter",
                "No campo de busca (constante campo), o cliente aperta Enter e nada acontece: a busca só roda quando ele clica na lupa.",
                "Registre no campo um ouvinte do evento keydown com uma arrow function de parâmetro e que chama buscar() se a tecla (e.key) for estritamente igual a Enter.",
                A("campo.addEventListener(\"keydown\", (e) => {\n    if (e.key === \"Enter\") buscar()\n})")
                    .Concat(FrontMix(A("campo.addEventListener(\"keydown\", "), A("(e) => ", "e => "),
                         A("{ if (e.key === \"Enter\") buscar() }", "{ if (e.key === \"Enter\") { buscar() } }", "{ if (\"Enter\" === e.key) buscar() }", "{ if (\"Enter\" === e.key) { buscar() } }"), A(")"))).ToArray(),
                A("O objeto do evento de teclado tem a propriedade key com o nome da tecla.", "campo.addEventListener(\"keydown\", (e) => { if (e.key === \"Enter\") buscar() })"),
                A("\"keydown\"", "Disparado quando uma tecla é pressionada.", "e.key === \"Enter\"", "Confere qual tecla foi (nome legível: \"Escape\", \"ArrowUp\"...).",
                  "buscar()", "Chama a busca."),
                "Se o campo estiver dentro de um <form>, o Enter já dispara o submit — ouvir o submit costuma ser ainda melhor. Evite keyCode e which: estão obsoletos."));

            list.Add(C(F, 1, "html", "Quantidade de 1 a 10",
                "Teve cliente pedindo -3 cafés. O campo de quantidade precisa aceitar só números entre 1 e 10, começando em 1.",
                "Escreva um input do tipo number com id qtd, mínimo 1 (min), máximo 10 (max) e valor inicial 1 (value).",
                ExtraFront_Perm("<input ", " ", ">", "type=\"number\"", "id=\"qtd\"", "min=\"1\"", "max=\"10\"", "value=\"1\""),
                A("Campos number aceitam os atributos min, max e value.", "<input type=\"number\" id=\"qtd\" min=\"1\" max=\"10\" value=\"1\">"),
                A("type=\"number\"", "Campo numérico (abre o teclado numérico no celular).", "min=\"1\" max=\"10\"", "Limites das setinhas e da validação.",
                  "value=\"1\"", "Valor inicial do campo."),
                "Dá para burlar tudo isso pelo DevTools: o servidor também precisa recusar quantidades inválidas."));

            list.Add(C(F, 1, "code", "Mobile first",
                "O CSS novo é escrito primeiro para o celular, então o .menu começa empilhado. A partir de 768px de largura, ele deve ficar em linha.",
                "Escreva a media query para telas com largura MÍNIMA de 768px que aplica display flex na classe .menu.",
                A("@media (min-width: 768px) {\n    .menu {\n        display: flex;\n    }\n}", "@media screen and (min-width: 768px) { .menu { display: flex; } }"),
                A("Mobile first usa min-width (a partir de), e não max-width (até).", "@media (min-width: 768px) { .menu { ... } }"),
                A("@media (min-width: 768px)", "Vale para telas de 768px ou mais (tablets e computadores).", ".menu { display: flex; }", "Coloca os itens em linha."),
                "Mobile first: o CSS base atende o celular (o aparelho mais limitado) e as media queries só acrescentam coisas para telas maiores."));

            list.Add(C(F, 1, "code", "Promoções com await",
                "Dentro de uma função async, a Yuki precisa carregar as promoções de /api/promocoes sem encadear .then.",
                "Escreva duas linhas: a constante resposta recebe await fetch de /api/promocoes; depois, a constante promocoes recebe await resposta.json().",
                A("const resposta = await fetch(\"/api/promocoes\")\nconst promocoes = await resposta.json()"),
                A("São DUAS esperas: a resposta chegar e o corpo ser lido.", "const resposta = await fetch(\"...\")  e  const promocoes = await resposta.____()"),
                A("await fetch(\"/api/promocoes\")", "Espera a resposta HTTP chegar.", "await resposta.json()", "Espera o corpo ser lido e convertido em objeto/array."),
                "await só funciona dentro de função async (ou no topo de um módulo). Sem o segundo await, promocoes vira uma Promise, não os dados."));

            list.Add(C(F, 1, "code", "Lista na tela",
                "A lista (constante lista, uma <ul>) deve mostrar cada texto do array cafes num <li>. Os nomes vêm do próprio cardápio da empresa, não de usuários.",
                "Atribua ao innerHTML de lista o resultado de cafes.map com uma arrow function de parâmetro c que gera o template literal <li>${c}</li>, seguido de .join(\"\") para unir tudo sem vírgulas.",
                FrontMix(A("lista.innerHTML = cafes.map("), A("c => ", "(c) => "), A("`<li>${c}</li>`).join(\"\")")),
                A("map devolve um array, mas innerHTML espera um texto: é preciso juntar os pedaços.", "lista.innerHTML = cafes.map(c => `<li>${c}</li>`).____(\"\")"),
                A("cafes.map(c => `<li>${c}</li>`)", "Transforma cada nome numa string de <li>.", ".join(\"\")", "Une as strings sem separador (o padrão seria vírgula!).",
                  "lista.innerHTML =", "Coloca o HTML gerado dentro da <ul>."),
                "innerHTML só com dados confiáveis: se o texto vier de usuário, crie os elementos com createElement e textContent para evitar XSS."));

            // ═══════════════════════════════ PROFISSIONAL ═══════════════════════════════
            list.Add(C(F, 2, "html", "Idioma da página",
                "O leitor de tela da Nina lê o site do Café 365 com pronúncia de inglês, e o Chrome oferece \"traduzir do inglês\". Falta declarar o idioma do documento.",
                "Escreva só a tag de abertura do elemento html, declarando o idioma português do Brasil (atributo lang com valor pt-BR).",
                A("<html lang=\"pt-BR\">"),
                A("O idioma é um atributo do elemento raiz do documento.", "<html lang=\"...\">"),
                A("<html>", "Elemento raiz do documento.", "lang=\"pt-BR\"", "Idioma principal: leitores de tela escolhem a voz e a pronúncia certas; tradutores e buscadores também usam."),
                "Trecho em outra língua no meio do texto? Marque só ele: <span lang=\"en\">cold brew</span>."));

            list.Add(C(F, 2, "code", "Foco visível",
                "Alguém pôs outline: none em todos os botões porque \"o contorno é feio\". Agora quem navega pelo teclado não sabe onde está. A Nina quer o foco de volta, só para quem usa teclado.",
                "Escreva a regra para .botao com a pseudo-classe :focus-visible, com outline de 3px solid #1a73e8 e outline-offset de 2px.",
                ExtraFront_Css(".botao:focus-visible", "outline: 3px solid #1a73e8;", "outline-offset: 2px;"),
                A(":focus aparece até no clique do mouse; :focus-visible aparece quando o foco precisa ser visto (ex.: teclado).", ".botao:focus-visible { outline: 3px solid #1a73e8; outline-offset: 2px; }"),
                A(".botao:focus-visible", "Estilo de foco mostrado na navegação por teclado.", "outline: 3px solid #1a73e8;", "Contorno azul de 3px que não muda o tamanho do botão.",
                  "outline-offset: 2px;", "Afasta o contorno 2px da borda: fica mais fácil de ver."),
                "Nunca remova o outline sem colocar outro indicador no lugar: a WCAG exige foco visível para quem usa teclado."));

            list.Add(C(F, 2, "code", "Soma que vira texto",
                "O total do pedido mostrou R$ 520 em vez de R$ 25: o valor do campo de gorjeta (campo.value) é sempre texto, e \"5\" + 20 junta em vez de somar. O valor da compra está na constante subtotal.",
                "Crie a constante total convertendo campo.value para número com Number() e somando subtotal.",
                A("const total = Number(campo.value) + subtotal", "const total = subtotal + Number(campo.value)"),
                A("Com texto, o + concatena. Converta o texto para número antes de somar.", "const total = Number(campo.____) + subtotal"),
                A("campo.value", "Valor do input: SEMPRE string, mesmo em type=\"number\".", "Number(...)", "Converte \"5\" em 5 (ou NaN se não for número).",
                  "+ subtotal", "Agora é soma de verdade."),
                "Confira NaN com Number.isNaN(valor). Para dinheiro, trabalhe em centavos (inteiros) para fugir de 0.1 + 0.2 = 0.30000000000000004."));

            list.Add(C(F, 2, "html", "Pular para o conteúdo",
                "A Nina navegou só com o teclado: precisou apertar Tab 27 vezes para passar pelo menu antes de chegar aos produtos. O <main> da página já tem id conteudo.",
                "Escreva o link de atalho que leva ao elemento de id conteudo, com a classe pular e o texto Pular para o conteúdo.",
                ExtraFront_Perm("<a ", " ", ">Pular para o conteúdo</a>", "href=\"#conteudo\"", "class=\"pular\""),
                A("Um link interno aponta para um id usando # no href.", "<a href=\"#conteudo\" class=\"pular\">Pular para o conteúdo</a>"),
                A("href=\"#conteudo\"", "Leva ao elemento com id=\"conteudo\" na mesma página.", "class=\"pular\"", "O CSS esconde o link fora da tela e o mostra quando ele recebe foco.",
                  "Pular para o conteúdo", "Texto anunciado já no primeiro Tab."),
                "O skip link deve ser o primeiro elemento focável da página e aparecer na tela ao receber foco (.pular:focus)."));

            list.Add(C(F, 2, "code", "Foto sem distorcer",
                "As fotos dos produtos chegam em tamanhos variados e, com largura e altura fixas, ficam esticadas. A Camila quer todas na proporção 4:3, cortando o excesso em vez de deformar.",
                "Escreva a regra da classe .foto com width 100%, aspect-ratio 4 / 3 e object-fit cover.",
                ExtraFront_Css(".foto", "width: 100%;", "aspect-ratio: 4 / 3;", "object-fit: cover;"),
                A("aspect-ratio fixa a proporção da caixa; object-fit diz como a imagem preenche essa caixa.", ".foto { width: 100%; aspect-ratio: 4 / 3; object-fit: _____; }"),
                A("aspect-ratio: 4 / 3;", "A altura é calculada pela largura: sempre 4 por 3.", "object-fit: cover;", "A imagem cobre a caixa toda, cortando as sobras sem distorcer.",
                  "width: 100%;", "Ocupa a largura do card."),
                "object-fit: contain mostra a imagem inteira (com sobras). Use object-position para escolher qual parte fica visível no corte."));

            list.Add(C(F, 2, "code", "Endereço opcional",
                "O painel quebrou com \"TypeError: Cannot read properties of undefined (reading 'cidade')\": clientes que retiram no balcão não têm endereco.",
                "Crie a constante cidade lendo cliente.endereco.cidade com encadeamento opcional (?.) no endereco e, se o resultado for null ou undefined, use o texto Retirada no balcão (operador ??).",
                A("const cidade = cliente.endereco?.cidade ?? \"Retirada no balcão\"", "const cidade = cliente?.endereco?.cidade ?? \"Retirada no balcão\""),
                A("?. devolve undefined em vez de lançar erro quando o que está à esquerda não existe.", "const cidade = cliente.endereco?.cidade ?? \"...\""),
                A("cliente.endereco?.cidade", "Se endereco não existir, o resultado é undefined (sem erro).", "?? \"Retirada no balcão\"", "Usa o texto padrão só quando o valor é null ou undefined."),
                "?? é mais seguro que || para valor padrão: o || também troca 0 e \"\" (que podem ser valores válidos!) pelo padrão."));

            list.Add(C(F, 2, "html", "Script sem travar",
                "O <script src=\"app.js\"> está no <head>: a página fica em branco até ele baixar e rodar, e o código ainda não encontra os elementos, que nem existem.",
                "Escreva a tag script que carrega app.js de forma adiada (atributo defer), para continuar no head.",
                A("<script src=\"app.js\" defer></script>", "<script defer src=\"app.js\"></script>"),
                A("Existe um atributo que baixa o script em paralelo e só o executa depois que todo o HTML foi lido.", "<script src=\"app.js\" ______></script>"),
                A("src=\"app.js\"", "O arquivo do script.", "defer", "Baixa sem bloquear e executa com o HTML pronto, na ordem em que os scripts aparecem.",
                  "</script>", "A tag script sempre precisa de fechamento."),
                "async também não bloqueia, mas executa assim que baixa (fora de ordem): bom para analytics, ruim para código que mexe no DOM. type=\"module\" já é defer por padrão."));

            list.Add(C(F, 2, "code", "Vitrine que se ajusta",
                "A vitrine tem 3 colunas fixas e, no celular, os cards ficam espremidos. A Camila quer que o grid decida sozinho quantas colunas cabem, sem nenhuma media query.",
                "Escreva a regra da classe .vitrine com display grid, grid-template-columns repeat(auto-fit, minmax(200px, 1fr)) e gap de 1rem.",
                ExtraFront_Css(".vitrine", "display: grid;", "grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));", "gap: 1rem;"),
                A("repeat aceita auto-fit no lugar do número de colunas; minmax define o mínimo e o máximo de cada uma.", ".vitrine { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1rem; }"),
                A("repeat(auto-fit, ...)", "Cria quantas colunas couberem.", "minmax(200px, 1fr)", "Cada coluna tem no mínimo 200px e divide a sobra igualmente.",
                  "gap: 1rem;", "Espaço entre os cards."),
                "auto-fit estica os cards quando sobram poucos; auto-fill mantém colunas vazias reservadas. Em telas muito estreitas, minmax(min(200px, 100%), 1fr) evita rolagem lateral."));

            list.Add(C(F, 2, "code", "Ordenar sem estragar",
                "A Yuki ordenou o array cafes por preço para a vitrine e, de repente, o cardápio da cozinha (que usa a ordem original) também mudou: sort() altera o próprio array!",
                "Crie a constante ordenados com uma CÓPIA de cafes (spread: [...cafes]) ordenada por preco crescente, usando sort com os parâmetros a e b.",
                FrontMix(A("const ordenados = [...cafes].sort("), A("(a, b) => a.preco - b.preco", "(a, b) => { return a.preco - b.preco }", "function (a, b) { return a.preco - b.preco }"), A(")")),
                A("sort muda o array original. Copie antes com o operador spread (...) dentro de colchetes.", "const ordenados = [...cafes].sort((a, b) => a.preco - ____)"),
                A("[...cafes]", "Array novo com os mesmos itens: o original fica intacto.", ".sort((a, b) => a.preco - b.preco)", "Resultado negativo põe a antes de b: ordem crescente de preço."),
                "Sem função, sort compara como TEXTO: [10, 9, 1].sort() vira [1, 10, 9]. Navegadores atuais também têm toSorted(), que já devolve uma cópia."));

            list.Add(C(F, 2, "html", "Imagem que não pesa",
                "A galeria baixa 40 fotos de uma vez e, enquanto carregam, o texto pula de lugar (o Lighthouse reclamou de CLS). A foto loja-interior.jpg fica no fim da página.",
                "Escreva a img de loja-interior.jpg com alt Interior da loja, carregamento preguiçoso (loading lazy), width 800 e height 600.",
                ExtraFront_Perm("<img ", " ", ">", "src=\"loja-interior.jpg\"", "alt=\"Interior da loja\"", "loading=\"lazy\"", "width=\"800\"", "height=\"600\""),
                A("loading=\"lazy\" adia o download até a imagem chegar perto da tela; width e height reservam o espaço.", "<img src=\"loja-interior.jpg\" alt=\"Interior da loja\" loading=\"lazy\" width=\"800\" height=\"600\">"),
                A("loading=\"lazy\"", "Só baixa quando a pessoa rolar até perto da imagem.", "width=\"800\" height=\"600\"", "O navegador reserva o espaço (proporção 4:3) antes de baixar: nada pula.",
                  "alt=\"Interior da loja\"", "Texto alternativo, sempre."),
                "Não use lazy na imagem principal do topo (o LCP): ela precisa carregar o quanto antes."));

            list.Add(C(F, 2, "code", "Título fluido",
                "O h1 fica gigante no celular e pequeno demais no monitor 4K. A Camila não quer cinco media queries só para isso.",
                "Escreva a regra para h1 com font-size usando clamp: mínimo 1.5rem, valor preferido 4vw e máximo 3rem.",
                ExtraFront_Css("h1", "font-size: clamp(1.5rem, 4vw, 3rem);"),
                A("clamp(MÍNIMO, PREFERIDO, MÁXIMO) mantém o valor dentro dos limites.", "h1 { font-size: clamp(____, 4vw, ____); }"),
                A("clamp(1.5rem, 4vw, 3rem)", "Usa 4% da largura da janela, mas nunca menos que 1.5rem nem mais que 3rem.", "4vw", "vw = 1% da largura da janela."),
                "Fonte só em vw ignora o zoom do navegador (problema de acessibilidade). Misturar rem e vw no valor preferido, como 1rem + 2vw, respeita melhor o zoom."));

            list.Add(C(F, 2, "code", "Busca com caractere especial",
                "Quem busca \"café & leite\" recebe resultados de \"café\": o & quebra a URL e vira outro parâmetro. É também uma porta para injetar parâmetros na API.",
                "Crie a constante url com um template literal: /api/busca?q= seguido do valor de termo codificado com encodeURIComponent.",
                A("const url = `/api/busca?q=${encodeURIComponent(termo)}`"),
                A("Existe uma função global que codifica &, ?, espaços e acentos para caberem num parâmetro de URL.", "const url = `/api/busca?q=${encodeURIComponent(____)}`"),
                A("encodeURIComponent(termo)", "\"café & leite\" vira \"caf%C3%A9%20%26%20leite\": o & deixa de separar parâmetros.", "`/api/busca?q=${...}`", "Template literal montando a URL."),
                "encodeURI (sem Component) NÃO codifica & nem ?: serve só para URL inteira. Outra opção: new URLSearchParams({ q: termo }).toString()."));

            list.Add(C(F, 2, "html", "Link externo seguro",
                "O link para o site do fornecedor de grãos abre em nova aba. A Yuki lembra que a página aberta pode tentar mexer na aba original (tabnabbing) e que não precisamos contar a ela de onde o visitante veio.",
                "Escreva o link para https://graos-parceiro.com com o texto Nosso fornecedor, abrindo em nova aba (target _blank) e com rel noopener noreferrer.",
                ExtraFront_Perm("<a ", " ", ">Nosso fornecedor</a>", "href=\"https://graos-parceiro.com\"", "target=\"_blank\"", "rel=\"noopener noreferrer\"|rel=\"noreferrer noopener\""),
                A("O atributo rel aceita vários valores separados por espaço.", "<a href=\"https://graos-parceiro.com\" target=\"_blank\" rel=\"noopener noreferrer\">Nosso fornecedor</a>"),
                A("target=\"_blank\"", "Abre em nova aba.", "noopener", "A nova página não recebe window.opener: não consegue redirecionar a aba original.",
                  "noreferrer", "Não envia o endereço de origem (cabeçalho Referer)."),
                "Navegadores atuais já aplicam noopener ao _blank, mas deixar explícito protege navegadores antigos e documenta a intenção."));

            list.Add(C(F, 2, "code", "Tema escuro automático",
                "As cores do site já estão em variáveis no :root (--fundo e --texto). Quem usa o sistema no modo escuro reclama do clarão ao abrir o Café 365 à noite.",
                "Escreva a media query de preferência por tema escuro (prefers-color-scheme: dark) que, dentro de :root, troca --fundo para #121212 e --texto para #f5f5f5.",
                A("@media (prefers-color-scheme: dark) {\n    :root {\n        --fundo: #121212;\n        --texto: #f5f5f5;\n    }\n}")
                    .Concat(ExtraFront_Perm("@media (prefers-color-scheme: dark) { :root { ", " ", " } }", "--fundo: #121212;", "--texto: #f5f5f5;")).ToArray(),
                A("A preferência do sistema é testada com @media (prefers-color-scheme: ...). Dentro dela, basta redefinir as variáveis.", "@media (prefers-color-scheme: dark) { :root { --fundo: ...; --texto: ...; } }"),
                A("@media (prefers-color-scheme: dark)", "Vale só para quem usa tema escuro no sistema.", ":root { --fundo: #121212; --texto: #f5f5f5; }", "Redefine as variáveis: todo o CSS que usa var(--fundo) muda sozinho."),
                "Evite preto puro com branco puro: cinza bem escuro com texto quase branco cansa menos. Acrescente color-scheme: light dark para os campos nativos acompanharem."));

            list.Add(C(F, 2, "code", "Formulário em objeto",
                "O formulário de cadastro (constante form) tem 12 campos e a Yuki está lendo um por um com getElementById. Todos os campos têm o atributo name.",
                "Crie a constante dados convertendo os campos do form em objeto: Object.fromEntries recebendo new FormData(form).",
                A("const dados = Object.fromEntries(new FormData(form))"),
                A("FormData lê todos os campos que têm name; Object.fromEntries transforma pares [chave, valor] em objeto.", "const dados = Object.fromEntries(new FormData(____))"),
                A("new FormData(form)", "Coleta os campos do formulário que têm name.", "Object.fromEntries(...)", "Vira { nome: \"...\", email: \"...\" }, pronto para JSON.stringify."),
                "Campos sem name ficam de fora, e checkbox desmarcado não aparece. Para várias escolhas com o mesmo name, use formData.getAll(\"nome\")."));

            list.Add(C(F, 2, "html", "Botão só com ícone",
                "O botão de adicionar ao carrinho tem só a imagem carrinho.svg, e o leitor de tela anuncia \"botão, carrinho.svg\". A Nina quer um nome acessível no botão e a imagem tratada como decorativa.",
                "Escreva um button com type button e aria-label Adicionar ao carrinho, contendo a imagem carrinho.svg com alt vazio.",
                FrontMix(ExtraFront_Perm("<button ", " ", ">", "type=\"button\"", "aria-label=\"Adicionar ao carrinho\""),
                         ExtraFront_Perm("<img ", " ", ">", "src=\"carrinho.svg\"", "alt=\"\""), A("</button>")),
                A("aria-label dá um nome ao botão; alt=\"\" diz que a imagem é só decoração.", "<button type=\"button\" aria-label=\"Adicionar ao carrinho\"><img src=\"carrinho.svg\" alt=\"\"></button>"),
                A("aria-label=\"Adicionar ao carrinho\"", "Nome anunciado pelo leitor de tela.", "type=\"button\"", "Não envia formulário por engano.",
                  "alt=\"\"", "Imagem decorativa: o leitor a ignora (sem alt ele leria o nome do arquivo!)."),
                "Se couber, prefira texto visível no botão: ajuda todo mundo, inclusive quem usa comando de voz (\"clicar em Adicionar\")."));

            list.Add(C(F, 2, "code", "Só para leitor de tela",
                "O botão do carrinho mostra só um ícone e um número. A Nina quer um texto \"itens no carrinho\" que o leitor de tela lê, mas que não aparece na tela — e display: none não serve, porque esconde do leitor também.",
                "Escreva a regra da classe .sr-only com position absolute, width 1px, height 1px, overflow hidden e clip-path inset(50%).",
                ExtraFront_Css(".sr-only", "position: absolute;", "width: 1px;", "height: 1px;", "overflow: hidden;", "clip-path: inset(50%);"),
                A("A ideia é encolher o elemento para 1px e recortar o que sobra, sem tirá-lo da árvore de acessibilidade.", ".sr-only { position: absolute; width: 1px; height: 1px; overflow: hidden; clip-path: inset(50%); }"),
                A("position: absolute;", "Tira o elemento do fluxo: ele não empurra o layout.", "width: 1px; height: 1px; overflow: hidden;", "Encolhe a caixa e corta o conteúdo.",
                  "clip-path: inset(50%);", "Recorta o resto: some da tela, mas o leitor de tela continua lendo."),
                "display: none e visibility: hidden escondem de TODO mundo, inclusive do leitor de tela. Bibliotecas costumam incluir também white-space: nowrap na .sr-only."));

            var aberto = A("aberto", "String(aberto)", "aberto.toString()");
            list.Add(C(F, 2, "code", "Menu que avisa o estado",
                "O botão do menu tem aria-expanded=\"false\" fixo no HTML: o menu abre, mas o leitor de tela continua anunciando \"recolhido\". O estado precisa ser atualizado a cada clique.",
                "Escreva duas linhas: a constante aberto recebe o resultado de menu.classList.toggle(\"aberto\"); depois, use setAttribute no botao para definir aria-expanded com o valor de aberto.",
                A("const aberto = menu.classList.toggle(\"aberto\")\nbotao.setAttribute(\"aria-expanded\", aberto)")
                    .Concat(FrontMix(A("const aberto = menu.classList.toggle(\"aberto\")\nbotao.setAttribute(\"aria-expanded\", "), aberto, A(")"))).ToArray(),
                A("classList.toggle devolve true quando a classe ficou ligada. setAttribute(nome, valor) muda um atributo.", "const aberto = menu.classList.toggle(\"aberto\")  e  botao.setAttribute(\"aria-expanded\", ______)"),
                A("menu.classList.toggle(\"aberto\")", "Alterna a classe e devolve true (abriu) ou false (fechou).", "botao.setAttribute(\"aria-expanded\", aberto)", "Atualiza o atributo; o valor vira o texto \"true\" ou \"false\"."),
                "Feche o menu também com a tecla Escape e devolva o foco ao botão: é o que quem usa teclado espera."));

            list.Add(C(F, 2, "html", "Preenchimento automático",
                "No checkout, o celular não sugere o e-mail salvo e o gerenciador de senhas não reconhece o campo. A Nina lembra que autocomplete também é critério de acessibilidade (WCAG 1.3.5).",
                "Escreva o input do tipo email com id email, name email, autocomplete email e required.",
                ExtraFront_Perm("<input ", " ", ">", "type=\"email\"", "id=\"email\"", "name=\"email\"", "autocomplete=\"email\"", "required"),
                A("O atributo autocomplete diz ao navegador QUAL dado o campo espera (email, name, tel, street-address...).", "<input type=\"email\" id=\"email\" name=\"email\" autocomplete=\"email\" required>"),
                A("name=\"email\"", "Nome do dado enviado ao servidor.", "autocomplete=\"email\"", "O navegador oferece o e-mail salvo: menos digitação e menos erro.",
                  "required", "Campo obrigatório."),
                "Use autocomplete=\"new-password\" no cadastro e \"current-password\" no login: o gerenciador de senhas agradece. Nunca desligue o autocomplete de senha."));

            list.Add(C(F, 2, "code", "Hover só com mouse",
                "No celular, o card fica \"preso\" ampliado depois do toque, porque o navegador simula o :hover. A Camila quer o efeito só em aparelhos que têm hover de verdade.",
                "Escreva a media query (hover: hover) contendo a regra .card:hover com transform scale(1.05).",
                A("@media (hover: hover) {\n    .card:hover {\n        transform: scale(1.05);\n    }\n}"),
                A("Media queries também testam o tipo de ponteiro: (hover: hover) só vale onde dá para passar o mouse.", "@media (hover: hover) { .card:hover { transform: scale(____); } }"),
                A("@media (hover: hover)", "Aplica só em dispositivos com hover real (mouse, trackpad).", ".card:hover", "Estado com o mouse por cima.",
                  "transform: scale(1.05);", "Aumenta 5% sem empurrar o layout (transform não afeta os vizinhos)."),
                "Combine com (pointer: fine) para detectar mouse preciso; telas de toque são (pointer: coarse) e merecem alvos de toque maiores."));

            list.Add(C(F, 2, "code", "Duas APIs ao mesmo tempo",
                "A vitrine faz await buscarCafes() e só depois await buscarDoces(): uma espera a outra à toa e a página demora o dobro para aparecer.",
                "Crie, com desestruturação de array, as constantes cafes e doces a partir de await Promise.all recebendo um array com buscarCafes() e buscarDoces() (nesta ordem).",
                A("const [cafes, doces] = await Promise.all([buscarCafes(), buscarDoces()])"),
                A("Promise.all recebe um array de Promises e espera todas em paralelo; os resultados vêm na mesma ordem.", "const [cafes, doces] = await Promise.all([____, ____])"),
                A("Promise.all([...])", "Dispara as duas buscas juntas e espera ambas.", "const [cafes, doces]", "Desestruturação de array: os resultados chegam na ordem do array.",
                  "await", "Espera tudo terminar (ou o primeiro erro)."),
                "Se uma falhar, o Promise.all inteiro rejeita. Quando cada parte puder falhar sozinha, use Promise.allSettled."));

            list.Add(C(F, 2, "html", "Erro anunciado no campo",
                "Quando o e-mail é inválido, aparece um texto vermelho embaixo do campo, mas o leitor de tela não fala nada: para ele, o erro está \"solto\" na página. O texto do erro tem id erro-email.",
                "Escreva o input do tipo email com id email, marcado como inválido (aria-invalid com valor true) e ligado à mensagem de erro pelo atributo aria-describedby (valor erro-email).",
                ExtraFront_Perm("<input ", " ", ">", "type=\"email\"", "id=\"email\"", "aria-invalid=\"true\"", "aria-describedby=\"erro-email\""),
                A("aria-invalid diz que o valor está errado; aria-describedby aponta o id do texto que descreve o campo.", "<input type=\"email\" id=\"email\" aria-invalid=\"true\" aria-describedby=\"...\">"),
                A("aria-invalid=\"true\"", "O leitor anuncia \"inválido\" ao entrar no campo.", "aria-describedby=\"erro-email\"", "Lê também o texto do elemento id=\"erro-email\" (a mensagem de erro).",
                  "id=\"email\"", "Continua ligado ao label pelo for."),
                "Erro não pode ser indicado só pela cor vermelha: tenha texto, ícone e o vínculo via aria-describedby. Remova o aria-invalid quando o valor ficar certo."));

            list.Add(C(F, 2, "html", "Aviso de item adicionado",
                "Ao adicionar ao carrinho, aparece \"Latte adicionado\" no canto da tela — mas quem usa leitor de tela não fica sabendo. O JavaScript já escreve a mensagem na div com id aviso.",
                "Escreva a div vazia com id aviso e aria-live com valor polite, para que o leitor anuncie as mudanças de texto dela.",
                ExtraFront_Perm("<div ", " ", "></div>", "id=\"aviso\"", "aria-live=\"polite\""),
                A("Regiões \"vivas\" (live regions) anunciam mudanças de conteúdo sem mover o foco.", "<div id=\"aviso\" aria-live=\"______\"></div>"),
                A("aria-live=\"polite\"", "Anuncia quando o texto mudar, esperando o leitor terminar o que está falando.", "id=\"aviso\"", "O JavaScript escreve a mensagem aqui (com textContent).",
                  "<div></div>", "Fica vazia na página desde o início: a região precisa existir ANTES da mensagem."),
                "Use assertive só para urgências (ele interrompe a fala). role=\"status\" equivale a aria-live=\"polite\" e também é uma opção válida."));

            var opcoesPost = ExtraFront_Perm("{ ", ", ", " }", "method: \"POST\"", "headers: { \"Content-Type\": \"application/json\" }", "body: JSON.stringify(pedido)");
            list.Add(C(F, 2, "code", "Enviando o pedido",
                "O pedido (objeto pedido) precisa ir para a API em JSON. A Yuki usou o fetch padrão e o servidor recebeu um GET sem corpo.",
                "Crie a constante resposta com await fetch para /api/pedidos, passando as opções: method POST, headers com Content-Type application/json, e body com JSON.stringify(pedido).",
                A("const resposta = await fetch(\"/api/pedidos\", {\n    method: \"POST\",\n    headers: { \"Content-Type\": \"application/json\" },\n    body: JSON.stringify(pedido)\n})")
                    .Concat(FrontMix(A("const resposta = await fetch(\"/api/pedidos\", "), opcoesPost, A(")")))
                    .Concat(FrontMix(A("const resposta = await fetch(\"/api/pedidos\", "), opcoesPost.Select(o => o.Substring(0, o.Length - 2) + ", }").ToArray(), A(")"))).ToArray(),
                A("O segundo argumento do fetch é um objeto de opções: method, headers e body.", "await fetch(\"/api/pedidos\", { method: \"POST\", headers: { \"Content-Type\": \"application/json\" }, body: JSON.stringify(____) })"),
                A("method: \"POST\"", "Envia dados (o padrão do fetch é GET).", "headers: { \"Content-Type\": \"application/json\" }", "Avisa o servidor que o corpo é JSON.",
                  "body: JSON.stringify(pedido)", "O corpo precisa ser TEXTO: o objeto é convertido para JSON."),
                "Depois do envio, confira resposta.ok: o fetch não lança erro em respostas 400 ou 500."));
        }
    }

    static partial class Docs
    {
        static partial void DocsExtraFront(List<DocEntry> all)
        {
            const string F = "HTML/CSS/JS";
            // ── HTML
            all.Add(X(F, "HTML", "<ul> <ol> <li>  (listas)", "Listas de itens. ul tem marcadores (a ordem não importa); ol é numerada (a ordem importa).",
                "<ul>\n  <li>item</li>\n</ul>\n<ol>\n  <li>passo</li>\n</ol>",
                A("<ul>", "lista não ordenada (bolinhas)", "<ol>", "lista ordenada (1, 2, 3...)", "<li>", "cada item, dentro de ul ou ol"),
                A("<ul><li>Arroz</li><li>Feijão</li></ul>", "lista de compras", "<ol><li>Ligar o forno</li><li>Assar por 30 minutos</li></ol>", "passos de uma receita"),
                "Menus de navegação também são listas: <nav><ul><li><a href=\"...\">...</a></li></ul></nav>."));
            all.Add(X(F, "HTML", "<strong> e <em>  (ênfase)", "Marcam importância e ênfase com significado (não só aparência).",
                "<strong>texto importante</strong>\n<em>texto enfatizado</em>",
                A("<strong>", "importância forte (negrito)", "<em>", "ênfase (itálico)", "<b> / <i>", "só aparência, sem significado"),
                A("<p>Entrega dos trabalhos <strong>amanhã</strong></p>", "palavra marcada como importante"), null));
            all.Add(X(F, "HTML", "<head>: <title>, <meta viewport> e lang", "Configurações do documento: idioma, título da aba e ajuste da tela no celular.",
                "<html lang=\"IDIOMA\">\n<head>\n  <meta name=\"viewport\" content=\"width=LARGURA, initial-scale=ZOOM\">\n  <title>Título da aba</title>\n</head>",
                A("lang", "idioma do documento (pt-BR, en, es...)", "<title>", "texto da aba, dos favoritos e do Google", "width=device-width", "no viewport: a página usa a largura real da tela",
                  "initial-scale=1", "no viewport: começa sem zoom"),
                A("<html lang=\"es\">", "site em espanhol", "<title>Biblioteca Municipal</title>", "título da aba"),
                "Sem a meta viewport, o celular mostra a versão de computador encolhida e as media queries não funcionam como esperado."));
            all.Add(X(F, "HTML", "<button>  (botões)", "Botão nativo: recebe foco com Tab e funciona com Enter e Espaço, sem JavaScript extra.",
                "<button type=\"button\">Texto</button>",
                A("type=\"submit\"", "envia o formulário (padrão dentro de um form)", "type=\"button\"", "não envia nada: só para JavaScript", "aria-label", "nome acessível quando o botão só tem ícone"),
                A("<button type=\"button\">Ver mais</button>", "botão comum", "<button type=\"button\" aria-label=\"Excluir foto\"><img src=\"lixeira.svg\" alt=\"\"></button>", "botão só com ícone"),
                "Nunca faça botão com <div onclick>: ele não recebe foco nem funciona no teclado."));
            all.Add(X(F, "HTML", "<textarea> e <select>", "Campos de formulário para texto longo (textarea) e para escolher numa lista (select).",
                "<textarea id=\"ID\"></textarea>\n<select id=\"ID\">\n  <option>opção</option>\n</select>",
                A("<textarea>", "texto de várias linhas (tem fechamento)", "rows", "altura inicial em linhas", "<select>", "lista suspensa", "<option>", "cada opção",
                  "value", "valor enviado (sem ele, vale o texto da option)"),
                A("<label for=\"bio\">Biografia</label><textarea id=\"bio\" rows=\"4\"></textarea>", "campo de texto longo com rótulo",
                  "<select id=\"turno\"><option>Manhã</option><option>Tarde</option></select>", "escolha do turno"),
                "Ligue sempre um <label for> ao id do campo."));
            all.Add(X(F, "HTML", "Validação: required, minlength, min, max, value", "Atributos que o navegador confere antes de enviar o formulário.",
                "<input type=\"TIPO\" required minlength=\"N\">\n<input type=\"number\" min=\"N\" max=\"N\" value=\"N\">",
                A("required", "obrigatório", "minlength / maxlength", "tamanho mínimo / máximo do texto", "min / max", "menor / maior número aceito", "value", "valor inicial",
                  "type=\"password\"", "esconde o que é digitado"),
                A("<input type=\"text\" id=\"apelido\" minlength=\"3\" maxlength=\"20\">", "apelido de 3 a 20 caracteres",
                  "<input type=\"number\" id=\"idade\" min=\"0\" max=\"120\" value=\"18\">", "idade entre 0 e 120, começando em 18"),
                "Validação no navegador é conforto, não segurança: o servidor sempre valida de novo."));
            all.Add(X(F, "HTML", "<figure> e <figcaption>", "Agrupa uma imagem (ou gráfico, código...) com a sua legenda.",
                "<figure>\n  <img src=\"ARQUIVO\" alt=\"DESCRIÇÃO\">\n  <figcaption>Legenda</figcaption>\n</figure>",
                A("<figure>", "a unidade mídia + legenda", "<figcaption>", "a legenda (primeiro ou último filho do figure)"),
                A("<figure><img src=\"ponte.jpg\" alt=\"Ponte de ferro sobre o rio\"><figcaption>Ponte construída em 1910</figcaption></figure>", "foto com legenda"),
                "O alt substitui a imagem; a legenda complementa. Não repita o mesmo texto nos dois."));
            all.Add(X(F, "HTML", "<img> rápida: loading, width e height", "Atributos que deixam a página mais leve e estável enquanto as imagens carregam.",
                "<img src=\"ARQUIVO\" alt=\"DESCRIÇÃO\" loading=\"lazy\" width=\"L\" height=\"A\">",
                A("loading=\"lazy\"", "só baixa quando a imagem chega perto da tela", "width / height", "tamanho original: o navegador reserva o espaço (evita o layout pular, CLS)"),
                A("<img src=\"mapa.png\" alt=\"Mapa do campus\" loading=\"lazy\" width=\"640\" height=\"480\">", "imagem lá embaixo na página"),
                "Não use lazy na imagem principal do topo: ela deve carregar logo."));
            all.Add(X(F, "HTML", "Links: #id, target e rel", "Links internos (para um id da página) e links externos abertos em nova aba com segurança.",
                "<a href=\"#ID\">texto</a>\n<a href=\"URL\" target=\"_blank\" rel=\"noopener noreferrer\">texto</a>",
                A("href=\"#rodape\"", "vai até o elemento com id=\"rodape\"", "target=\"_blank\"", "abre em nova aba", "rel=\"noopener\"", "a nova aba não controla a original (window.opener)",
                  "rel=\"noreferrer\"", "não envia o endereço de origem"),
                A("<a href=\"#horarios\">Ver horários</a>", "link para outra parte da mesma página",
                  "<a href=\"https://museu.org\" target=\"_blank\" rel=\"noopener noreferrer\">Museu</a>", "site externo em nova aba"),
                "Skip link: um link \"Pular para o conteúdo\" como primeiro item da página, apontando para o id do <main>."));
            all.Add(X(F, "HTML", "<script defer> e async", "Como carregar JavaScript sem travar a página.",
                "<script src=\"ARQUIVO.js\" defer></script>",
                A("defer", "baixa em paralelo e executa depois do HTML pronto, na ordem", "async", "baixa em paralelo e executa assim que chega (fora de ordem)", "type=\"module\"", "módulo ES: já é defer"),
                A("<script src=\"graficos.js\" defer></script>", "script que mexe no DOM, sem bloquear", "<script src=\"estatisticas.js\" async></script>", "script independente (ex.: analytics)"),
                "A tag script sempre precisa do </script>, mesmo com src."));
            all.Add(X(F, "HTML", "autocomplete  (preenchimento automático)", "Diz ao navegador qual dado o campo espera, para ele sugerir o que a pessoa já salvou.",
                "<input type=\"TIPO\" name=\"NOME\" autocomplete=\"VALOR\">",
                A("email", "e-mail", "name", "nome completo", "tel", "telefone", "street-address", "endereço", "new-password", "senha nova (cadastro)", "current-password", "senha atual (login)"),
                A("<input type=\"tel\" id=\"fone\" name=\"fone\" autocomplete=\"tel\">", "telefone com sugestão automática"),
                "Também é critério de acessibilidade (WCAG 1.3.5): ajuda quem tem dificuldade para digitar."));
            all.Add(X(F, "HTML", "aria-invalid, aria-describedby e aria-live", "Atributos ARIA para formulários e mensagens dinâmicas.",
                "<input aria-invalid=\"true\" aria-describedby=\"ID_DA_MENSAGEM\">\n<div aria-live=\"polite\"></div>",
                A("aria-invalid=\"true\"", "o valor do campo está errado", "aria-describedby=\"id\"", "id do texto que descreve o campo (dica ou erro)",
                  "aria-live=\"polite\"", "anuncia mudanças de texto quando o leitor terminar de falar", "aria-live=\"assertive\"", "interrompe para anunciar (só urgências)"),
                A("<input type=\"text\" id=\"cpf\" aria-invalid=\"true\" aria-describedby=\"dica-cpf\">", "campo de CPF ligado ao texto id=\"dica-cpf\"",
                  "<p id=\"placar\" aria-live=\"polite\"></p>", "região que anuncia o placar quando ele mudar"),
                "A região aria-live precisa existir na página antes da mensagem ser escrita nela."));
            // ── CSS
            all.Add(X(F, "CSS", "Cores e fundo: background-color, hex e rgba", "Cor de fundo e formatos de cor.",
                "seletor { background-color: COR; }",
                A("#rrggbb", "hexadecimal (#ff0000 = vermelho)", "rgb(r, g, b)", "valores de 0 a 255", "rgba(r, g, b, a)", "com transparência: a vai de 0 a 1",
                  "background", "atalho: cor, imagem, posição..."),
                A(".alerta { background-color: #ffe0e0; }", "fundo rosado", ".vidro { background-color: rgba(255, 255, 255, 0.6); }", "branco 60% opaco"), null));
            all.Add(X(F, "CSS", "Unidades: px, rem, %, vw e ch", "Medidas absolutas e relativas.",
                "seletor { font-size: 1.25rem; width: 50%; }",
                A("px", "pixels (fixo)", "rem", "relativo à fonte da raiz (normalmente 16px)", "%", "relativo ao elemento pai", "vw / vh", "1% da largura / altura da janela",
                  "ch", "largura do caractere 0 (bom para linhas de texto)"),
                A(".aviso { font-size: 1.25rem; }", "20px se a raiz tiver 16px", ".coluna { width: 50%; }", "metade do pai", ".texto { max-width: 60ch; }", "linhas de até ~60 caracteres"),
                "Use rem nas fontes: elas acompanham o tamanho de letra escolhido pela pessoa no navegador."));
            all.Add(X(F, "CSS", "Box model: padding, margin, border e box-sizing", "Todo elemento é uma caixa: conteúdo → padding → border → margin.",
                "seletor { padding: VALOR; margin: VALOR; }",
                A("padding", "espaço interno (dentro da borda)", "margin", "espaço externo (fora da borda)", "margin: 0 auto", "centraliza um bloco com largura definida",
                  "max-width", "largura máxima", "box-sizing: border-box", "a width passa a incluir padding e borda"),
                A(".aviso { padding: 8px 12px; }", "8px em cima/embaixo e 12px nas laterais", ".pagina { max-width: 1200px; margin: 0 auto; }", "bloco centralizado de até 1200px",
                  ".painel { box-sizing: border-box; width: 300px; padding: 20px; }", "o painel mede 300px mesmo com o padding"),
                "Com 1 valor vale para os 4 lados; 2 valores = vertical e horizontal; 4 valores = topo, direita, baixo, esquerda."));
            all.Add(X(F, "CSS", "border-radius e box-shadow", "Cantos arredondados e sombras.",
                "seletor { border-radius: RAIO; box-shadow: X Y DESFOQUE COR; }",
                A("border-radius: 50%", "num quadrado, vira círculo", "box-shadow: x y desfoque cor", "sombra (x e y = deslocamento)", "inset", "sombra para dentro"),
                A(".avatar { border-radius: 50%; }", "foto redonda", ".painel { box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15); }", "sombra suave para baixo"), null));
            all.Add(X(F, "CSS", "Seletores: id, descendente, universal e pseudo-classes", "Formas de escolher QUAIS elementos recebem a regra.",
                "#id { }\n.pai .filho { }\nseletor:pseudo-classe { }",
                A("#id", "o elemento com aquele id", ".pai .filho", "descendente: .filho em qualquer nível dentro de .pai", "*", "universal: todos os elementos",
                  ":hover / :focus-visible", "mouse por cima / foco pelo teclado", ":first-child / :last-child", "primeiro / último filho",
                  ":nth-child(odd|even|3n)", "pela posição entre os irmãos", "::before / ::after", "pseudo-elementos (conteúdo gerado)"),
                A("#rodape a:hover { color: gold; }", "links do rodapé com o mouse por cima", ".lista li:nth-child(odd) { background: #eee; }", "itens ímpares da lista",
                  ".abas li:last-child { margin-right: 0; }", "último item sem margem"),
                "Seletores muito longos ou com id deixam a regra difícil de sobrescrever; prefira classes."));
            all.Add(X(F, "CSS", "justify-content, align-items e gap  (valores)", "Alinhamento e espaçamento dos itens de um container flex (ou grid).",
                ".container { display: flex; justify-content: VALOR; align-items: VALOR; gap: VALOR; }",
                A("flex-start / center / flex-end", "começo / centro / fim", "space-between", "primeiro e último nas pontas, sobra dividida entre eles",
                  "space-around / space-evenly", "sobra dividida também nas pontas", "align-items: center", "centraliza no eixo cruzado", "gap", "espaço entre os itens"),
                A(".barra { display: flex; justify-content: center; align-items: center; gap: 12px; }", "tudo centralizado nos dois eixos"),
                "justify-content age no eixo principal (linha, por padrão) e align-items no eixo cruzado."));
            all.Add(X(F, "CSS", "position: relative, absolute, fixed e sticky", "Controla como o elemento é posicionado.",
                "seletor { position: TIPO; top: VALOR; }",
                A("relative", "desloca a partir da posição normal; serve de referência para filhos absolute", "absolute", "sai do fluxo; posicionado em relação ao ancestral posicionado",
                  "fixed", "preso à janela (não rola)", "sticky", "rola normalmente até o limite e gruda", "z-index", "quem fica por cima"),
                A(".aviso-cookies { position: fixed; bottom: 0; }", "barra presa embaixo da janela", ".indice { position: sticky; top: 1rem; }", "índice que gruda ao rolar"),
                "sticky para de funcionar se um ancestral tiver overflow: hidden."));
            all.Add(X(F, "CSS", "@media (min-width) e (hover: hover)", "Media queries para mobile first e para o tipo de ponteiro.",
                "@media (min-width: LARGURA) { regras }\n@media (hover: hover) { regras }",
                A("min-width", "a partir dessa largura (mobile first)", "max-width", "até essa largura", "(hover: hover)", "aparelho com mouse/trackpad",
                  "(pointer: coarse)", "ponteiro impreciso (dedo)"),
                A("@media (min-width: 1024px) { .lateral { display: block; } }", "mostra a lateral em telas grandes",
                  "@media (hover: hover) { .foto:hover { opacity: 0.8; } }", "efeito só onde existe hover de verdade"),
                "Mobile first: escreva o CSS base para o celular e use min-width para acrescentar coisas em telas maiores."));
            all.Add(X(F, "CSS", "Grid automático: auto-fit e minmax()", "Colunas que se ajustam sozinhas à largura disponível, sem media query.",
                ".container { display: grid; grid-template-columns: repeat(auto-fit, minmax(MÍNIMO, 1fr)); }",
                A("auto-fit", "quantas colunas couberem (estica os itens)", "auto-fill", "quantas couberem (mantém colunas vazias)", "minmax(a, b)", "no mínimo a, no máximo b"),
                A(".albuns { display: grid; grid-template-columns: repeat(auto-fill, minmax(150px, 1fr)); gap: 8px; }", "álbuns de pelo menos 150px"), null));
            all.Add(X(F, "CSS", "clamp()  (valores fluidos)", "Escolhe um valor preferido, mas mantém entre um mínimo e um máximo.",
                "propriedade: clamp(MÍNIMO, PREFERIDO, MÁXIMO);", null,
                A(".titulo { font-size: clamp(1rem, 2.5vw, 2rem); }", "cresce com a janela entre 1rem e 2rem", ".caixa { width: clamp(280px, 50%, 600px); }", "largura flexível com limites"),
                "Também existem min() e max(): width: min(100%, 600px)."));
            all.Add(X(F, "CSS", "aspect-ratio e object-fit", "Proporção fixa da caixa e como a imagem a preenche.",
                "seletor { aspect-ratio: L / A; object-fit: cover; }",
                A("aspect-ratio: 16 / 9", "altura calculada pela largura", "object-fit: cover", "cobre a caixa, cortando as sobras", "object-fit: contain", "mostra inteira, sobrando espaço",
                  "object-position", "qual parte fica visível no corte"),
                A(".capa { width: 100%; aspect-ratio: 16 / 9; object-fit: cover; }", "capa de vídeo em 16:9 sem distorcer"), null));
            all.Add(X(F, "CSS", "outline e :focus-visible", "Indicador de foco para quem navega pelo teclado.",
                "seletor:focus-visible { outline: ESPESSURA ESTILO COR; outline-offset: DISTÂNCIA; }",
                A("outline", "contorno que não ocupa espaço no layout", "outline-offset", "distância entre o contorno e a borda", ":focus-visible", "só quando o foco precisa aparecer (teclado)"),
                A("a:focus-visible { outline: 2px dashed #222; outline-offset: 4px; }", "contorno tracejado nos links focados"),
                "Nunca use outline: none sem colocar outro indicador de foco no lugar."));
            all.Add(X(F, "CSS", "Texto só para leitores de tela  (.sr-only)", "Esconde o texto da tela sem escondê-lo das tecnologias assistivas (display: none esconde de todo mundo).",
                ".NOME { position: absolute; width: 1px; height: 1px; overflow: hidden; clip-path: inset(50%); }",
                A("position: absolute", "não ocupa espaço no layout", "width/height: 1px + overflow: hidden", "encolhe e corta o conteúdo", "clip-path: inset(50%)", "recorta o que sobrou"),
                A(".oculto-visual { position: absolute; width: 1px; height: 1px; overflow: hidden; clip-path: inset(50%); white-space: nowrap; }", "versão com white-space: nowrap (evita quebras estranhas na leitura)"),
                "Frameworks como Bootstrap e Tailwind já trazem essa classe pronta."));
            // ── JavaScript
            all.Add(X(F, "JavaScript", "Arrays: [ ], length e push", "Listas ordenadas de valores. O primeiro item fica no índice 0.",
                "const lista = [\"a\", \"b\"]\nlista.length\nlista.push(\"c\")",
                A("[ ]", "cria o array", "lista[0]", "primeiro item", "length", "quantidade de itens (propriedade, sem parênteses)", "push(x)", "acrescenta no fim",
                  "pop()", "remove do fim", "unshift(x) / shift()", "acrescenta / remove do início"),
                A("const notas = [7, 9, 10]", "array de números", "console.log(notas.length)", "mostra 3", "notas.push(8)", "agora são 4 notas"), null));
            all.Add(X(F, "JavaScript", "Objetos  { chave: valor }", "Agrupam informações com nome. Leia com ponto: objeto.chave.",
                "const obj = { chave: valor, outra: valor }",
                A("{ }", "cria o objeto", "obj.chave", "lê a propriedade", "obj[\"chave\"]", "lê por nome em texto"),
                A("const aluno = { nome: \"Rui\", idade: 15 }", "objeto com texto e número", "console.log(aluno.nome)", "mostra Rui"),
                "Número entre aspas vira texto: { idade: \"15\" } é diferente de { idade: 15 }."));
            all.Add(X(F, "JavaScript", "for...of  (percorrer arrays)", "Repete o bloco para cada item do array. Dá para desestruturar o item direto na declaração.",
                "for (const item of lista) {\n  ...\n}\nfor (const { a, b } of lista) { ... }",
                A("for (const x of lista)", "x recebe cada item, em ordem", "for (const { a, b } of lista)", "cada item (objeto) já é desestruturado em a e b",
                  "for...in", "percorre as CHAVES (índices), não os valores: evite em arrays"),
                A("for (const nota of notas) console.log(nota)", "mostra cada nota", "for (const { nome, idade } of alunos) { console.log(nome, idade) }", "nome e idade de cada aluno"),
                "Para transformar ou filtrar, prefira map e filter; o for...of é bom para executar ações (mostrar, enviar, somar)."));
            all.Add(X(F, "JavaScript", "classList: add, remove, toggle", "Liga e desliga classes CSS de um elemento sem mexer nas outras.",
                "elemento.classList.add(\"classe\")\nelemento.classList.toggle(\"classe\")",
                A("add", "acrescenta", "remove", "tira", "toggle", "alterna; devolve true se ficou ligada", "contains", "true se o elemento tem a classe"),
                A("painel.classList.toggle(\"visivel\")", "mostra/esconde o painel", "aba.classList.remove(\"ativa\")", "desativa a aba"),
                "O nome da classe vai SEM o ponto."));
            all.Add(X(F, "JavaScript", "setAttribute e getAttribute", "Lê e muda atributos HTML (inclusive aria-*) pelo JavaScript.",
                "elemento.setAttribute(\"nome\", valor)\nelemento.getAttribute(\"nome\")", null,
                A("link.setAttribute(\"href\", \"/ajuda\")", "muda o destino do link", "aba.setAttribute(\"aria-selected\", ativa)", "o valor booleano vira o texto \"true\"/\"false\""),
                "Ao abrir/fechar algo, atualize o aria-expanded do botão junto."));
            all.Add(X(F, "JavaScript", "Arrays: find", "Devolve o PRIMEIRO item para o qual a função retorna true (ou undefined).",
                "lista.find(item => condicao)", null,
                A("const ana = alunos.find(a => a.nome === \"Ana\")", "o objeto da Ana (não um array)"),
                "filter devolve sempre um array; find devolve um item só. Use === (comparação estrita)."));
            all.Add(X(F, "JavaScript", "Strings: toLowerCase, includes e trim", "Métodos úteis de texto (todos devolvem um valor novo).",
                "texto.toLowerCase()\ntexto.includes(\"trecho\")", null,
                A("\"Brasil\".toLowerCase()", "\"brasil\"", "\"Brasil\".includes(\"ras\")", "true", "\"  oi  \".trim()", "\"oi\" (sem espaços nas pontas)",
                  "livros.filter(l => l.titulo.toLowerCase().includes(termo))", "busca sem diferenciar maiúsculas"),
                "includes diferencia maiúsculas: compare as duas strings no mesmo formato."));
            all.Add(X(F, "JavaScript", "join  (array → texto)", "Une os itens de um array num texto, com o separador escolhido.",
                "lista.join(\"SEPARADOR\")", null,
                A("[\"a\", \"b\"].join(\", \")", "\"a, b\"", "tags.map(t => `<span>${t}</span>`).join(\"\")", "gera HTML sem vírgulas entre os itens"),
                "Sem argumento, join usa vírgula — por isso um array jogado direto no innerHTML aparece com vírgulas."));
            all.Add(X(F, "JavaScript", "Eventos de teclado: keydown e e.key", "Reage a teclas pressionadas. e.key traz o nome legível da tecla.",
                "elemento.addEventListener(\"keydown\", (e) => {\n  if (e.key === \"NOME\") ...\n})",
                A("\"Enter\"", "Enter", "\"Escape\"", "Esc", "\"ArrowUp\" / \"ArrowDown\"", "setas", "\" \"", "barra de espaço"),
                A("janela.addEventListener(\"keydown\", (e) => { if (e.key === \"Escape\") fechar() })", "fecha com Esc"),
                "keyCode e which estão obsoletos: use e.key."));
            all.Add(X(F, "JavaScript", "Spread [...] e sort", "O spread copia um array; sort ordena o PRÓPRIO array (muda o original).",
                "const copia = [...lista]\nlista.sort((a, b) => a - b)",
                A("[...lista]", "cópia rasa do array", "{ ...obj }", "cópia rasa do objeto", "sort((a, b) => a - b)", "crescente (números)", "sort((a, b) => b - a)", "decrescente"),
                A("const porIdade = [...alunos].sort((a, b) => a.idade - b.idade)", "cópia ordenada por idade; alunos não muda"),
                "Sem função, sort compara como texto: [10, 9, 1].sort() dá [1, 10, 9]."));
            all.Add(X(F, "JavaScript", "encodeURIComponent", "Codifica um valor para caber com segurança num parâmetro de URL (&, ?, espaços, acentos...).",
                "encodeURIComponent(valor)", null,
                A("`/api/cidades?nome=${encodeURIComponent(cidade)}`", "\"São Paulo\" vira \"S%C3%A3o%20Paulo\""),
                "encodeURI (sem Component) não codifica & e ?: não serve para valores de parâmetros."));
            all.Add(X(F, "JavaScript", "?. e ??  (encadeamento opcional e valor padrão)", "Leem valores que podem não existir sem quebrar o código.",
                "objeto.propriedade?.outra ?? padrão",
                A("a?.b", "undefined se a for null/undefined (sem erro)", "a ?? b", "b só se a for null ou undefined", "a || b", "b se a for qualquer valor \"falso\" (0, \"\", null...)"),
                A("const bairro = aluno.contato?.bairro ?? \"Não informado\"", "lê o bairro se houver contato; senão, usa o padrão"),
                "Prefira ?? quando 0 ou \"\" forem valores válidos."));
            all.Add(X(F, "JavaScript", "Number()  (texto → número)", "Converte um texto em número. Valores de inputs são sempre strings.",
                "Number(texto)", null,
                A("Number(\"42\")", "42", "Number(\"abc\")", "NaN (não é número)", "Number(idade.value) + 1", "soma de verdade, não concatenação"),
                "\"20\" + 5 dá \"205\". Confira o resultado com Number.isNaN(valor)."));
            all.Add(X(F, "JavaScript", "fetch com POST  (method, headers, body)", "Envia dados para uma API.",
                "await fetch(url, {\n  method: \"POST\",\n  headers: { \"Content-Type\": \"application/json\" },\n  body: JSON.stringify(objeto)\n})",
                A("method", "GET (padrão), POST, PUT, DELETE...", "headers", "cabeçalhos HTTP", "body", "corpo da requisição (texto)"),
                A("await fetch(\"/api/inscricoes\", { method: \"POST\", headers: { \"Content-Type\": \"application/json\" }, body: JSON.stringify(inscricao) })", "envia a inscrição em JSON"),
                "Confira resposta.ok depois: erro 400/500 não cai no catch."));
            all.Add(X(F, "JavaScript", "Promise.all  (em paralelo)", "Espera várias Promises ao mesmo tempo; os resultados vêm na mesma ordem.",
                "const [a, b] = await Promise.all([promessaA, promessaB])", null,
                A("const [turmas, alunos] = await Promise.all([buscarTurmas(), buscarAlunos()])", "as duas buscas correm juntas"),
                "Se uma falhar, Promise.all rejeita. Use Promise.allSettled para tratar cada resultado separadamente."));
            all.Add(X(F, "JavaScript", "FormData e Object.fromEntries", "Lê todos os campos (com name) de um formulário de uma vez.",
                "const dados = Object.fromEntries(new FormData(formulario))", null,
                A("const inscricao = Object.fromEntries(new FormData(formInscricao))", "{ nome: \"...\", turma: \"...\" }"),
                "Campos sem o atributo name ficam de fora."));
        }
    }
}
