using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerQuest
{
    static partial class Challenges
    {
        // ───────────── utilitários de respostas (Flutter) ─────────────
        // Modelos com alternativas «a|b» (podem ser aninhadas) e "#X" substituídos antes da expansão.
        // Também gera as variações com vírgula final (trailing comma), comum em Dart.
        static string[] FltAns(params string[] modelos)
        {
            var res = new List<string>();
            var vistos = new HashSet<string>();
            foreach (var m in modelos)
                foreach (var e in FltAlt(m))
                    foreach (var c in FltCommas(e))
                        if (vistos.Add(Checker.NormCode(c))) res.Add(c);
            return res.ToArray();
        }

        static string FltSub(string s, params string[] pares)
        {
            for (int i = 0; i + 1 < pares.Length; i += 2) s = s.Replace(pares[i], pares[i + 1]);
            return s;
        }

        // expande o primeiro grupo «...» (de fora para dentro), só nos ramos escolhidos
        static IEnumerable<string> FltAlt(string t)
        {
            int a = t.IndexOf('«');
            if (a < 0) { yield return t; yield break; }
            int depth = 0, ini = a + 1, b = -1;
            var opts = new List<string>();
            for (int k = a; k < t.Length; k++)
            {
                char c = t[k];
                if (c == '«') depth++;
                else if (c == '»') { depth--; if (depth == 0) { opts.Add(t.Substring(ini, k - ini)); b = k; break; } }
                else if (c == '|' && depth == 1) { opts.Add(t.Substring(ini, k - ini)); ini = k + 1; }
            }
            string pre = t.Substring(0, a), pos = t.Substring(b + 1);
            foreach (var o in opts)
                foreach (var r in FltAlt(pre + o + pos)) yield return r;
        }

        static bool FltWord(char c) { return char.IsLetterOrDigit(c) || c == '_'; }

        // há vírgula ou dois-pontos no nível 0 (argumentos nomeados ou vários argumentos)?
        static bool FltTopLevel(string s)
        {
            int d = 0; char q = '\0';
            foreach (char c in s)
            {
                if (q != '\0') { if (c == q) q = '\0'; continue; }
                if (c == '"' || c == '\'') q = c;
                else if (c == '(' || c == '[' || c == '{') d++;
                else if (c == ')' || c == ']' || c == '}') d--;
                else if (d == 0 && (c == ',' || c == ':')) return true;
            }
            return false;
        }

        static List<string> FltCommas(string s)
        {
            var pos = new List<int>();
            var estrutural = new List<bool>();
            var pilha = new Stack<int>();
            char q = '\0';
            for (int k = 0; k < s.Length; k++)
            {
                char c = s[k];
                if (q != '\0') { if (c == q) q = '\0'; continue; }
                if (c == '"' || c == '\'') { q = c; continue; }
                if (c == '(' || c == '[' || c == '{') { pilha.Push(k); continue; }
                if (c != ')' && c != ']' && c != '}') continue;
                if (pilha.Count == 0) continue;
                int o = pilha.Pop();
                int j = k - 1; while (j > o && char.IsWhiteSpace(s[j])) j--;
                if (j <= o || s[j] == ',') continue;
                int p = o - 1; while (p >= 0 && char.IsWhiteSpace(s[p])) p--;
                char antes = p >= 0 ? s[p] : ' ';
                bool ok;
                if (c == ')')
                {
                    int w = p; while (w >= 0 && FltWord(s[w])) w--;
                    string palavra = p >= 0 ? s.Substring(w + 1, p - w) : "";
                    bool chamada = FltWord(antes) || (antes == '>' && p > 0 && s[p - 1] != '=');
                    ok = chamada && palavra != "if" && palavra != "while" && palavra != "for" && palavra != "switch" && palavra != "catch"
                         && FltTopLevel(s.Substring(o + 1, k - o - 1));
                }
                else if (c == ']') ok = !(FltWord(antes) || antes == ')' || antes == ']' || antes == '!');
                else ok = antes == '(';
                if (ok) { pos.Add(j + 1); estrutural.Add(s[j] == ')' || s[j] == ']' || s[j] == '}'); }
            }
            int n = pos.Count;
            var mascaras = new List<int> { 0 };
            if (n <= 4) for (int m = 1; m < (1 << n); m++) mascaras.Add(m);
            else
            {
                for (int i = 0; i < n; i++) mascaras.Add(1 << i);
                int st = 0; for (int i = 0; i < n; i++) if (estrutural[i]) st |= 1 << i;
                mascaras.Add(st);
                mascaras.Add((1 << n) - 1);
            }
            var res = new List<string>();
            foreach (var m in mascaras.Distinct())
            {
                string r = s;
                for (int i = n - 1; i >= 0; i--) if ((m & (1 << i)) != 0) r = r.Insert(pos[i], ",");
                res.Add(r);
            }
            return res;
        }

        static partial void MaisFlutter(List<Level> list)
        {
            const string L = "flutter";

            // 1) rebalanceamento dos desafios existentes
            Action<string, int> nivel = (titulo, t) =>
            {
                var lv = list.FirstOrDefault(x => x.Cat == L && x.Title == titulo);
                if (lv == null) return;
                lv.Tier = t;
                lv.Chapter = Cat(L).Name + " · " + TierNames[t];
            };
            nivel("Em coluna", 0);             // Column(children: [a, b]) — um conceito só
            nivel("Pacote http", 0);           // flutter pub add http — comando direto
            nivel("Rumo à loja", 0);           // flutter build apk --release — comando direto
            nivel("Função assíncrona", 1);
            nivel("Esperando a resposta", 1);
            nivel("JSON para mapa", 1);

            // 2) novos desafios
            // ───────── Iniciante
            list.Add(C(L, 0, "code", "Nome opcional",
                "O Kenji está montando o perfil do cliente. Nem todo mundo cadastra um apelido, e o Dart (com null safety) não deixa uma variável comum ficar sem valor.",
                "Declare a variável apelido do tipo String que aceita null, sem valor inicial.",
                A("String? apelido;", "String? apelido = null"),
                A("Com null safety, só aceita null o tipo marcado com um sinal logo depois do nome do tipo.", "Tipo? nome;"),
                A("String?", "O ? depois do tipo diz: pode ser um texto OU null.", "apelido", "Sem valor inicial, uma variável anulável começa valendo null."),
                "Antes de usar um valor anulável, trate o null: apelido ?? 'Cliente' ou if (apelido != null) {...}. O ! (apelido!) força e derruba o app se for null."));

            list.Add(C(L, 0, "code", "Lista de bebidas",
                "O cardápio do app começa simples: três bebidas fixas que o Bruno quer ver na tela.",
                "Declare uma lista final chamada bebidas com os textos Espresso, Latte e Mocha, nessa ordem.",
                FltAns("final bebidas = [\"Espresso\", \"Latte\", \"Mocha\"];",
                       "final «List<String> bebidas = |bebidas = <String>»[\"Espresso\", \"Latte\", \"Mocha\"]"),
                A("Listas em Dart usam colchetes, com os itens separados por vírgula.", "final nome = ['a', 'b', 'c'];"),
                A("final bebidas", "A variável não pode apontar para outra lista depois.", "[\"Espresso\", \"Latte\", \"Mocha\"]", "Literal de lista: o Dart deduz o tipo List<String>."),
                "bebidas.length dá o tamanho, bebidas[0] o primeiro item e bebidas.add('Chai') acrescenta. final impede trocar a lista inteira, mas ainda deixa usar add; para uma lista imutável, use const."));

            // ───────── Intermediário
            list.Add(C(L, 1, "code", "Texto em destaque",
                "Na vitrine do app, a promoção precisa chamar atenção: letra maior e em negrito.",
                "Crie um Text com 'Promoção do dia' e um style TextStyle com tamanho de fonte 22 e peso negrito (bold).",
                FltAns("«|const »Text(\"Promoção do dia\", style: «|const »TextStyle(«fontSize: «22|22.0», fontWeight: FontWeight.bold|fontWeight: FontWeight.bold, fontSize: «22|22.0»»))"),
                A("A aparência do Text vem do parâmetro nomeado style, que recebe um TextStyle.", "Text('...', style: TextStyle(fontSize: ..., fontWeight: FontWeight.____))"),
                A("style: TextStyle(...)", "Define a aparência do texto.", "fontSize: 22", "Tamanho da fonte em pixels lógicos.", "fontWeight: FontWeight.bold", "Negrito (também existem w100 … w900)."),
                "Para manter o visual consistente, prefira os estilos do tema (Theme.of(context).textTheme.titleLarge) em vez de repetir TextStyle pelo app inteiro."));

            list.Add(C(L, 1, "code", "Tamanho do apelido",
                "A tela de perfil mostra quantas letras tem o apelido (String? apelido). Alguém escreveu apelido!.length e o app quebra para quem não tem apelido: 'Null check operator used on a null value'.",
                "Guarde em letras (final) o tamanho de apelido usando acesso seguro a null; quando apelido for null, letras deve valer 0.",
                FltAns("final letras = apelido?.length ?? 0;", "final «int |»letras = «apelido?.length|(apelido?.length)» ?? 0"),
                A("São dois operadores null-aware: um só acessa a propriedade se o valor não for null; o outro dá um valor padrão.", "final letras = apelido?.______ ?? _;"),
                A("apelido?.length", "Se apelido for null, o resultado é null (em vez de quebrar).", "?? 0", "Se a expressão da esquerda for null, usa 0."),
                "Evite o ! (apelido!) a não ser que tenha certeza absoluta: ele transforma null em crash. ?. e ?? deixam o app seguro."));

            list.Add(C(L, 1, "code", "Preço total",
                "O Kenji quer uma função de cálculo que ninguém consiga chamar trocando a ordem dos argumentos: total(preco: 8.5, qtd: 2).",
                "Escreva a função total que retorna double e recebe os parâmetros nomeados obrigatórios preco (double) e qtd (int), nessa ordem, devolvendo preco * qtd.",
                FltAns("double total({required double preco, required int qtd}) => preco * qtd;",
                       "double total({required double preco, required int qtd}) «=> qtd * preco|{\n  return «preco * qtd|qtd * preco»;\n}»"),
                A("Parâmetros nomeados ficam entre chaves { } na declaração; a palavra required torna cada um obrigatório.", "double total({required double preco, required int ___}) => ..."),
                A("{required double preco, required int qtd}", "Nomeados e obrigatórios: quem chama precisa escrever o nome de cada um.", "=> preco * qtd", "Arrow: atalho para { return preco * qtd; }."),
                "Os widgets usam o mesmo recurso: Text(..., style: ...), ElevatedButton(onPressed: ..., child: ...). Parâmetros nomeados deixam a chamada autoexplicativa."));

            list.Add(C(L, 1, "code", "Campo de busca",
                "A tela de cardápio vai ganhar uma busca. O Kenji já criou final buscaController = TextEditingController(); para ler o que o cliente digitar.",
                "Crie um TextField ligado ao controller buscaController, com uma decoration InputDecoration cujo labelText seja 'Buscar'.",
                FltAns(FltSub("TextField(«controller: buscaController, #D|#D, controller: buscaController»)",
                              "#D", "decoration: «|const »InputDecoration(labelText: \"Buscar\")")),
                A("O TextField recebe o controller e uma decoration com o rótulo.", "TextField(controller: ..., decoration: InputDecoration(labelText: '...'))"),
                A("controller: buscaController", "Liga o campo ao controller: buscaController.text tem o texto digitado.", "InputDecoration(labelText: 'Buscar')", "Rótulo que sobe quando o campo recebe o foco."),
                "Para reagir a cada letra digitada, use onChanged: (texto) { ... }. E lembre: controllers precisam ser liberados no dispose() da State."));

            // ───────── Profissional
            list.Add(C(L, 2, "code", "Texto sem estouro",
                "A Paula testou num celular pequeno e apareceu a faixa amarela e preta: 'A RenderFlex overflowed by 48 pixels on the right'. É a linha com o ícone e a descrição longa do produto.",
                "Monte uma Row com children: icone e Text(descricao). O texto deve ocupar apenas o espaço que sobra na linha (envolva-o no widget adequado) e, se não couber, terminar com reticências.",
                FltAns("Row(children: [icone, «Expanded|Flexible»(child: Text(descricao, «overflow: TextOverflow.ellipsis|maxLines: 1, overflow: TextOverflow.ellipsis|overflow: TextOverflow.ellipsis, maxLines: 1»))])"),
                A("Dentro de Row/Column, um filho envolvido em Expanded recebe só o espaço restante. E o Text tem o parâmetro overflow.", "Row(children: [icone, Expanded(child: Text(descricao, overflow: TextOverflow.________))])"),
                A("Expanded(child: ...)", "Faz o filho ocupar o espaço livre da Row, sem passar do limite.", "overflow: TextOverflow.ellipsis", "Corta o texto que não cabe e mostra '…'."),
                "Overflow é o bug de layout nº 1 em telas pequenas. Teste com fonte grande (acessibilidade) e em celulares estreitos. Flexible é a versão de Expanded que não obriga a ocupar tudo."));

            list.Add(C(L, 2, "code", "Lista de produtos",
                "O cardápio tem centenas de itens (List<String> produtos). Com uma Column e um widget por item, a rolagem trava. O Bruno pede uma lista que só constrói o que aparece na tela.",
                "Monte um ListView.builder com itemCount igual ao tamanho de produtos e itemBuilder (context, index) que devolve um ListTile com title: Text do produto da posição e onTap: uma função anônima que chama abrir com esse mesmo produto.",
                FltAns(
                    "ListView.builder(\n  itemCount: produtos.length,\n  itemBuilder: (context, index) => ListTile(\n    title: Text(produtos[index]),\n    onTap: () => abrir(produtos[index])\n  )\n)",
                    FltSub("ListView.builder(«itemCount: produtos.length, itemBuilder: #B|itemBuilder: #B, itemCount: produtos.length»)",
                        "#B", "(context, index) «=> #T|{ return #T; }»",
                        "#T", "ListTile(«title: Text(produtos[index]), onTap: #O|onTap: #O, title: Text(produtos[index])»)",
                        "#O", "«() => abrir(produtos[index])|() { abrir(produtos[index]); }»")),
                A("ListView.builder recebe itemCount e itemBuilder; o itemBuilder recebe (context, index) e devolve o widget daquela posição.", "ListView.builder(itemCount: produtos.length, itemBuilder: (context, index) => ListTile(title: ..., onTap: () => ...))"),
                A("itemCount: produtos.length", "Quantos itens existem (sem ele, a lista seria infinita).", "itemBuilder: (context, index) => ...", "Chamado só para os itens visíveis: rápido mesmo com milhares.",
                  "onTap: () => abrir(produtos[index])", "Função anônima: abrir só roda quando o item é tocado."),
                "Armadilha: onTap: abrir(produtos[index]) (sem o () =>) chama a função na hora de desenhar, não no toque. Para listas com divisórias existe ListView.separated."));

            list.Add(C(L, 2, "code", "Sem vazamento",
                "A Paula abriu e fechou a tela de busca 50 vezes e o consumo de memória só subiu. O Kenji criou buscaController = TextEditingController() na State, mas nunca o liberou.",
                "Sobrescreva o método dispose da State (com @override e retorno void): libere buscaController e, por último, chame o dispose da classe-mãe.",
                A("@override\nvoid dispose() {\n  buscaController.dispose();\n  super.dispose();\n}"),
                A("Tudo o que a State cria e que tem dispose() (controllers, streams, timers) deve ser liberado no dispose() da própria State.", "@override void dispose() { ________.dispose(); super.dispose(); }"),
                A("@override", "Indica que o método substitui o da classe State.", "buscaController.dispose();", "Libera os recursos do controller.", "super.dispose();", "Deixa o Flutter terminar a limpeza — sempre por último."),
                "Regra do ciclo de vida: crie no initState(), libere no dispose(). Vale para TextEditingController, AnimationController, ScrollController, StreamSubscription e Timer."));

            list.Add(C(L, 2, "code", "Escolha com retorno",
                "Na tela do produto, o cliente toca em 'Personalizar' e escolhe o tamanho numa outra tela, que fecha com Navigator.pop(context, tamanho). O Bruno quer receber esse tamanho de volta.",
                "Numa função async, abra EscolhaPage passando o parâmetro nomeado produto (com a variável produto) e guarde em escolha (final) o valor devolvido quando a tela fechar. Use o Navigator com MaterialPageRoute e builder (context).",
                FltAns("final escolha = await «Navigator.push(context, |Navigator.push<String>(context, |Navigator.of(context).push(»MaterialPageRoute«|<String>»(builder: «(context)|(_)» => EscolhaPage(produto: produto)))"),
                A("Navigator.push devolve um Future que termina quando a nova tela faz pop — dá para esperar com await.", "final escolha = await Navigator.push(context, MaterialPageRoute(builder: (context) => EscolhaPage(produto: ...)))"),
                A("await Navigator.push(...)", "Espera a tela fechar e recebe o valor passado no pop.", "EscolhaPage(produto: produto)", "Envia o dado para a nova tela pelo construtor.",
                  "final escolha", "Fica null se o usuário voltar sem escolher (botão ou gesto de voltar)."),
                "Sempre trate o null: o usuário pode voltar sem escolher. Para tipar o retorno, use Navigator.push<String>(...)."));

            list.Add(C(L, 2, "code", "Contexto após await",
                "Bug achado pela Paula: se o cliente sai da tela enquanto o pedido está sendo salvo, o app quebra ao tentar voltar ('Looking up a deactivated widget's ancestor is unsafe'). O analisador já avisava: use_build_context_synchronously.",
                "Escreva o corpo do onPressed (que já é async): espere salvar() terminar; depois, só se o State ainda estiver montado na tela, volte para a tela anterior com o Navigator.",
                FltAns(FltSub("await salvar();\nif («!mounted|!context.mounted») «return;|{ return; }»\n#P;",
                              "#P", "«Navigator.pop(context)|Navigator.of(context).pop()»"),
                       FltSub("await salvar();\nif («mounted|context.mounted») «#P;|{ #P; }»",
                              "#P", "«Navigator.pop(context)|Navigator.of(context).pop()»")),
                A("Depois de um await a tela pode já ter sido fechada. A State tem a propriedade mounted, que diz se ela ainda está na árvore.", "await salvar(); if (!mounted) return; Navigator.___(context);"),
                A("await salvar();", "Durante a espera, o usuário pode sair da tela.", "if (!mounted) return;", "Se o widget já saiu da árvore, para aqui: o context não vale mais.",
                  "Navigator.pop(context);", "Seguro: só roda com a tela ainda montada."),
                "Regra: nunca use o context depois de um await sem conferir mounted (dentro da State) ou context.mounted (em funções que recebem o context)."));

            list.Add(C(L, 2, "code", "Modelo fromJson",
                "A API manda {\"nome\": \"Latte\", \"preco\": 9.5}, mas às vezes o preço vem inteiro ({\"preco\": 8}) e o cast direto para double derruba o app. A classe já existe: Produto({required this.nome, required this.preco}), com final String nome e final double preco.",
                "Escreva o construtor factory Produto.fromJson, que recebe Map<String, dynamic> json e cria o Produto com nome = json['nome'] e preco = json['preco'] convertido com segurança para double (funcionando com 8 e com 9.5).",
                FltAns("factory Produto.fromJson(Map<String, dynamic> json) => Produto(nome: json[\"nome\"], preco: (json[\"preco\"] as num).toDouble());",
                    FltSub("factory Produto.fromJson(Map<String, dynamic> json) «=> Produto(#A)|{\n  return Produto(#A);\n}»",
                        "#A", "«#N, #P|#P, #N»",
                        "#N", "nome: «json[\"nome\"]|json[\"nome\"] as String»",
                        "#P", "preco: «(json[\"preco\"] as num).toDouble()|json[\"preco\"].toDouble()|double.parse(json[\"preco\"].toString())»")),
                A("Números do JSON chegam como int OU double; os dois são num, e num tem o método toDouble().", "factory Produto.fromJson(Map<String, dynamic> json) => Produto(nome: json['nome'], preco: (json['preco'] as ___).toDouble());"),
                A("factory Produto.fromJson(...)", "Construtor nomeado que monta o objeto a partir do mapa.", "Map<String, dynamic> json", "O que o jsonDecode devolve para um objeto JSON.",
                  "(json['preco'] as num).toDouble()", "Aceita 8 ou 9.5: converte qualquer número para double."),
                "Com o modelo pronto: (jsonDecode(resposta.body) as List).map((e) => Produto.fromJson(e)).toList(). Em projetos grandes, o pacote json_serializable gera esse código."));

            list.Add(C(L, 2, "code", "Buscar com status",
                "Hoje o app faz jsonDecode de qualquer resposta. Quando a API devolve 500 (uma página de erro em HTML), aparece um FormatException confuso. O Bruno quer um erro claro.",
                "Escreva a função assíncrona buscarProdutos, que retorna Future<List<dynamic>>: faça um GET para url (uma String — converta para Uri) guardando em resposta (final) com await; se o statusCode for diferente de 200, lance Exception('Falha'); senão, retorne o jsonDecode do corpo.",
                FltAns("Future<List<dynamic>> buscarProdutos() async {\n  final resposta = await http.get(Uri.parse(url));\n  if (resposta.statusCode != 200) throw Exception(\"Falha\");\n  return jsonDecode(resposta.body);\n}",
                    FltSub("Future<List<dynamic>> buscarProdutos() async {\n  final resposta = await http.get(Uri.parse(url));\n  if (resposta.statusCode != 200) «throw Exception(\"Falha\");|{ throw Exception(\"Falha\"); }»\n  «|else »«return #J;|{ return #J; }»\n}",
                        "#J", "«jsonDecode(resposta.body)|jsonDecode(resposta.body) as List<dynamic>|jsonDecode(resposta.body) as List»"),
                    FltSub("Future<List<dynamic>> buscarProdutos() async {\n  final resposta = await http.get(Uri.parse(url));\n  if (resposta.statusCode == 200) «return #J;|{ return #J; }»\n  «|else »«throw Exception(\"Falha\");|{ throw Exception(\"Falha\"); }»\n}",
                        "#J", "«jsonDecode(resposta.body)|jsonDecode(resposta.body) as List<dynamic>|jsonDecode(resposta.body) as List»")),
                A("Três passos: await http.get(Uri.parse(...)), conferir resposta.statusCode e só então jsonDecode(resposta.body).", "Future<List<dynamic>> buscarProdutos() async { final resposta = await http.get(Uri.parse(url)); if (resposta.statusCode != 200) throw Exception('Falha'); return ...; }"),
                A("Uri.parse(url)", "http.get espera um Uri, não uma String.", "if (resposta.statusCode != 200) throw ...", "Falha cedo, com mensagem clara, antes de tentar ler o corpo.",
                  "return jsonDecode(resposta.body);", "Só decodifica quando a resposta é de sucesso."),
                "Quem chama trata o erro com try/catch (ou o FutureBuilder mostra snapshot.hasError). Em produção, use também um timeout: http.get(...).timeout(Duration(seconds: 10))."));

            list.Add(C(L, 2, "code", "Carregando o cardápio",
                "A tela inicial mostra o nome da loja vindo da API. O Kenji guardou o Future no initState (nomeFuture = buscarNome();) para não refazer a requisição a cada build. Falta a parte visual.",
                "Monte um FutureBuilder<String> com future: nomeFuture e builder (context, snapshot) que: se o snapshot tem dados, retorna Text com o dado (use ! para tirar o null); se tem erro, retorna Text('Erro'); senão, retorna um CircularProgressIndicator().",
                FltAns("FutureBuilder<String>(\n  future: nomeFuture,\n  builder: (context, snapshot) {\n    if (snapshot.hasData) return Text(snapshot.data!);\n    if (snapshot.hasError) return Text(\"Erro\");\n    return CircularProgressIndicator();\n  }\n)",
                    FltSub("FutureBuilder«<String>|»(«future: nomeFuture, builder: #B|builder: #B, future: nomeFuture»)",
                        "#B", "(context, snapshot) «{ if (snapshot.hasData) #D «|else »if (snapshot.hasError) #E return #C; }|=> snapshot.hasData ? Text(snapshot.data!) : snapshot.hasError ? «|const »Text(\"Erro\") : #C»",
                        "#D", "«return Text(snapshot.data!);|{ return Text(snapshot.data!); }»",
                        "#E", "«return «|const »Text(\"Erro\");|{ return «|const »Text(\"Erro\"); }»",
                        "#C", "«|const »CircularProgressIndicator()")),
                A("O snapshot tem hasData, hasError e data. Teste nessa ordem e deixe o carregando por último.", "builder: (context, snapshot) { if (snapshot.hasData) return Text(snapshot.data!); if (snapshot.hasError) return Text('Erro'); return CircularProgressIndicator(); }"),
                A("future: nomeFuture", "Future criado fora do build (no initState): evita requisição repetida a cada redesenho.", "snapshot.hasData / snapshot.data!", "O valor chegou; o ! garante ao Dart que não é null.",
                  "snapshot.hasError", "O Future falhou (ex.: Exception('Falha')).", "CircularProgressIndicator()", "Enquanto espera: indicador de carregamento."),
                "Armadilha clássica: future: buscarNome() direto no build dispara uma requisição nova a cada setState. Guarde o Future numa variável no initState."));

            // comando de publicação: aceita --opcao=valor e --opcao valor, com ou sem --release, em qualquer ordem
            var bundle = new List<string> { "flutter build appbundle --release --build-name=1.2.0 --build-number=7 --obfuscate --split-debug-info=build/simbolos" };
            foreach (var rel in new[] { " --release", "" })
                foreach (var bn in new[] { " --build-name=1.2.0", " --build-name 1.2.0" })
                    foreach (var bu in new[] { " --build-number=7", " --build-number 7" })
                        foreach (var sd in new[] { " --split-debug-info=build/simbolos", " --split-debug-info build/simbolos", " --split-debug-info=./build/simbolos", " --split-debug-info ./build/simbolos" })
                        {
                            string cmd = "flutter build appbundle" + rel + bn + bu + " --obfuscate" + sd;
                            if (!bundle.Contains(cmd)) bundle.Add(cmd);
                        }
            list.Add(C(L, 2, "cmd", "Bundle para a loja",
                "Versão 1.2.0 aprovada pela Paula! A Play Store exige App Bundle e um código de versão maior que o anterior (o último foi 6). O Bruno também quer o código Dart ofuscado, guardando os símbolos para conseguir ler os relatórios de crash.",
                "Gere o App Bundle de release com nome de versão 1.2.0, número de build 7, código ofuscado e símbolos de depuração salvos em build/simbolos.",
                bundle.ToArray(),
                A("O formato da Play Store é o appbundle; versão e código são --build-name e --build-number; a ofuscação exige --split-debug-info.", "flutter build appbundle --build-name=... --build-number=... --obfuscate --split-debug-info=..."),
                A("flutter build appbundle", "Gera o .aab, formato exigido pela Play Store.", "--build-name=1.2.0 --build-number=7", "Versão exibida ao usuário e código interno (sempre crescente).",
                  "--obfuscate --split-debug-info=build/simbolos", "Embaralha os nomes no binário e guarda os símbolos para decifrar stack traces."),
                "Guarde a pasta de símbolos de cada versão publicada: sem ela, os crashes ofuscados ficam ilegíveis (flutter symbolize -i <trace> -d <pasta>)."));
        }
    }

    static partial class Docs
    {
        static partial void DocsMaisFlutter(List<DocEntry> all)
        {
            const string L = "Flutter";
            all.Add(X(L, "Dart", "Null safety: Tipo? / ?. / ?? / !", "Por padrão, nenhuma variável aceita null. O ? no tipo permite null; ?. e ?? tratam o null com segurança.",
                "String? nome;            // pode ser null\nobjeto?.propriedade      // dá null se objeto for null\nvalor ?? padrao          // usa padrao se valor for null\nvalor!                   // garante que não é null (quebra se for)",
                A("Tipo?", "tipo anulável (sem valor inicial, começa como null)", "?.", "acesso seguro: não quebra com null", "??", "valor padrão quando a esquerda é null", "!", "operador de certeza: evite, gera crash com null"),
                A("int? idade;", "idade pode ficar sem valor", "final turma = sala ?? \"Sem sala\"", "valor padrão", "final sigla = cidade?.toUpperCase() ?? \"N/D\"", "acesso seguro + padrão"),
                "Prefira ?. e ?? ao !: um ! aplicado a null derruba o app."));
            all.Add(X(L, "Dart", "Listas (List) e mapas (Map)", "Coleções do Dart: List é ordenada (índice a partir de 0); Map guarda pares chave: valor.",
                "final itens = [a, b, c];\nfinal List<String> nomes = ['x', 'y'];\nfinal mapa = {'chave': valor};",
                A("lista.length", "quantidade de itens", "lista[0]", "primeiro item", "lista.add(x)", "acrescenta no fim", "mapa['chave']", "lê um valor do Map"),
                A("final materias = [\"Matemática\", \"História\", \"Física\"]", "lista de textos (List<String>)", "final notas = <int>[7, 9, 10]", "tipo explícito com <int>", "final aluno = {\"nome\": \"Rita\", \"idade\": 16}", "um Map"),
                "final: a variável não troca de lista (mas a lista ainda aceita add). const: lista totalmente imutável."));
            all.Add(X(L, "Dart", "Funções e parâmetros nomeados (required)", "Parâmetros entre { } são nomeados: quem chama escreve nome: valor. required torna o parâmetro obrigatório.",
                "TIPO nome({required TIPO a, required TIPO b}) => expressão;\nTIPO nome({required TIPO a}) { return ...; }",
                A("{ }", "parâmetros nomeados", "required", "obrigatório (sem ele, o parâmetro precisa ser anulável ou ter valor padrão)", "=>", "corpo de uma expressão só (atalho para return)"),
                A("int area({required int largura, required int altura}) => largura * altura;", "declaração", "area(altura: 4, largura: 3)", "chamada: a ordem dos nomeados não importa"), null));
            all.Add(X(L, "Dart", "Modelos: factory fromJson", "Construtor factory que cria um objeto a partir do Map devolvido pelo jsonDecode.",
                "factory Classe.fromJson(Map<String, dynamic> json) => Classe(campo: json['campo']);",
                A("factory", "construtor que devolve uma instância (pode ter lógica)", "Map<String, dynamic>", "tipo de um objeto JSON decodificado", "(json['x'] as num).toDouble()", "número do JSON (int ou double) convertido para double", "json['x'] as String", "cast explícito (opcional)"),
                A("factory Livro.fromJson(Map<String, dynamic> json) => Livro(titulo: json[\"titulo\"], paginas: json[\"paginas\"])", "monta um Livro", "(json[\"nota\"] as num).toDouble()", "funciona com 7 e com 7.5"),
                "json['x'] as double quebra quando o JSON traz um inteiro (8): use num e toDouble()."));
            all.Add(X(L, "Dart", "http: Uri.parse e statusCode", "Detalhes de uma requisição com o pacote http: o endereço é um Uri e a resposta traz o código de status.",
                "final resp = await http.get(Uri.parse(\"https://...\"));\nif (resp.statusCode != 200) throw Exception('mensagem');",
                A("Uri.parse(texto)", "converte String em Uri (http.get exige Uri)", "resp.statusCode", "200 = OK; 4xx = erro do cliente; 5xx = erro do servidor", "resp.body", "corpo da resposta em texto", "throw Exception('...')", "lança um erro (tratado com try/catch)"),
                A("final r = await http.post(Uri.parse(enderecoMatricula), body: dados)", "POST", "if (r.statusCode != 201) throw Exception(\"Erro ao salvar\")", "201 = criado"),
                "Confira o status antes do jsonDecode: uma página de erro em HTML não é JSON."));
            all.Add(X(L, "Widgets", "TextStyle", "Aparência de um Text: tamanho, peso, cor e estilo da fonte.",
                "Text('texto', style: TextStyle(fontSize: N, fontWeight: FontWeight.PESO, color: COR))",
                A("fontSize", "tamanho da fonte", "fontWeight", "FontWeight.bold (negrito), FontWeight.w100 … w900", "color", "Colors.red, Colors.brown...", "fontStyle", "FontStyle.italic (itálico)"),
                A("Text(\"Aviso\", style: TextStyle(fontSize: 18, color: Colors.red))", "texto vermelho maior", "TextStyle(fontWeight: FontWeight.w600)", "seminegrito"), null));
            all.Add(X(L, "Widgets", "TextField e TextEditingController", "Campo de texto. O controller dá acesso ao que foi digitado.",
                "final meuController = TextEditingController();\nTextField(controller: meuController, decoration: InputDecoration(labelText: 'Rótulo'))",
                A("controller", "liga o campo a um TextEditingController (controller.text = texto digitado)", "decoration", "InputDecoration: labelText, hintText, prefixIcon...", "onChanged", "função chamada a cada alteração", "obscureText: true", "esconde o texto (senhas)"),
                A("TextField(controller: emailController, decoration: InputDecoration(hintText: \"seu@email.com\"))", "campo com texto de exemplo", "print(emailController.text)", "lê o que foi digitado"),
                "Libere o controller no dispose() da State."));
            all.Add(X(L, "Widgets", "Ciclo de vida da State: initState / dispose", "initState roda uma vez quando a tela nasce; dispose roda quando ela sai de vez. Recursos criados num devem ser liberados no outro.",
                "@override\nvoid initState() {\n  super.initState();\n  // criar recursos\n}\n\n@override\nvoid dispose() {\n  // liberar recursos\n  super.dispose();\n}",
                A("@override", "o método substitui o da classe State", "super.initState()", "sempre a PRIMEIRA linha do initState", "super.dispose()", "sempre a ÚLTIMA linha do dispose"),
                A("animacao.dispose();", "libera um AnimationController", "timer.cancel();", "para um Timer", "assinatura.cancel();", "encerra um StreamSubscription"),
                "Esquecer o dispose de controllers causa vazamento de memória."));
            all.Add(X(L, "Widgets", "Expanded / Flexible e overflow de texto", "Controlam quanto espaço um filho ocupa numa Row/Column e o que acontece com o texto que não cabe.",
                "Row(children: [w1, Expanded(child: w2)])\nText(texto, overflow: TextOverflow.ellipsis, maxLines: N)",
                A("Expanded", "o filho ocupa todo o espaço livre da Row/Column", "Flexible", "pode ocupar até o espaço livre (não é obrigado a preencher)", "flex: N", "proporção entre vários Expanded",
                  "TextOverflow.ellipsis", "corta o texto com …", "maxLines", "limite de linhas", "mainAxisAlignment", "(Row/Column) distribuição: start, center, spaceBetween..."),
                A("Row(children: [Expanded(child: campoNome), botaoOk])", "o campo ocupa o que sobra", "Text(resumoDoLivro, maxLines: 2, overflow: TextOverflow.fade)", "no máximo 2 linhas, com esmaecimento"),
                "'A RenderFlex overflowed' (faixa amarela e preta) = o conteúdo não coube na Row/Column."));
            all.Add(X(L, "Widgets", "ListView.builder e ListTile", "Lista rolável que constrói só os itens visíveis — ideal para listas longas. ListTile é a linha padrão de lista.",
                "ListView.builder(itemCount: N, itemBuilder: (context, index) => WIDGET_DA_POSICAO)\nListTile(title: ..., subtitle: ..., trailing: ..., onTap: () => ...)",
                A("itemCount", "quantos itens (normalmente lista.length)", "itemBuilder", "(context, index) => widget da posição index", "onTap: () => f(x)", "função anônima: f só roda no toque"),
                A("ListView.builder(itemCount: alunos.length, itemBuilder: (context, i) => Text(alunos[i]))", "lista de nomes", "ListTile(title: Text(\"Dom Casmurro\"), onTap: () => emprestar(livro))", "linha tocável"),
                "onTap: f(x) (sem o () =>) chamaria f na hora de desenhar, não no toque."));
            all.Add(X(L, "Widgets", "FutureBuilder", "Redesenha a tela conforme um Future anda: esperando, com dado ou com erro.",
                "FutureBuilder<TIPO>(future: meuFuture, builder: (context, snapshot) { ... })",
                A("future", "o Future a esperar (crie no initState, não no build)", "snapshot.hasData", "o valor chegou", "snapshot.data", "o valor (anulável: use ! depois de conferir hasData)",
                  "snapshot.hasError / snapshot.error", "o Future falhou", "CircularProgressIndicator()", "indicador de carregamento"),
                A("FutureBuilder<int>(future: totalFuture, builder: (context, snapshot) => snapshot.hasData ? Text(\"Total: ${snapshot.data}\") : LinearProgressIndicator())", "mostra o total ou uma barra de carregamento"),
                "future: buscar() direto no build refaz a requisição a cada redesenho."));
            all.Add(X(L, "Widgets", "Navigator: enviar dados e receber resultado", "Dados vão para a nova tela pelo construtor; a tela devolve um resultado no pop, que chega pelo await do push.",
                "final r = await Navigator.push(context, MaterialPageRoute(builder: (context) => Tela(param: valor)));\nNavigator.pop(context, resultado);",
                A("Navigator.push(...)", "devolve um Future com o valor do pop", "Navigator.of(context).push(...)", "forma equivalente", "Navigator.pop(context, valor)", "fecha a tela devolvendo valor"),
                A("final cor = await Navigator.push(context, MaterialPageRoute(builder: (context) => CorPage(tema: tema)))", "abre CorPage e espera a escolha", "Navigator.pop(context, \"azul\")", "fecha devolvendo \"azul\""),
                "Se o usuário voltar pelo botão/gesto, o resultado é null."));
            all.Add(X(L, "Widgets", "mounted (usar context depois de await)", "Depois de um await a tela pode ter sido fechada. mounted diz se o State ainda está na árvore de widgets.",
                "await algo();\nif (!mounted) return;\n// daqui em diante é seguro usar o context",
                A("mounted", "(State) true enquanto o widget está na tela", "context.mounted", "mesma ideia, a partir do BuildContext"),
                A("await enviarFoto();\nif (!mounted) return;\nScaffoldMessenger.of(context).showSnackBar(aviso);", "mostra o aviso só se a tela ainda existir"),
                "O lint use_build_context_synchronously aponta esse problema."));
            all.Add(X(L, "CLI do Flutter", "flutter build appbundle (publicação)", "Gera o Android App Bundle (.aab) para a Play Store, com versão e, se quiser, código ofuscado.",
                "flutter build appbundle [--release] [--build-name=X.Y.Z] [--build-number=N] [--obfuscate --split-debug-info=PASTA]",
                A("--build-name", "versão exibida (ex.: 2.0.1)", "--build-number", "código de versão inteiro, sempre maior que o anterior", "--obfuscate", "ofusca o código Dart (exige --split-debug-info)", "--split-debug-info=PASTA", "salva os símbolos de depuração na pasta"),
                A("flutter build appbundle --build-name=3.1.0 --build-number=12", "versão 3.1.0, código 12", "flutter build apk --obfuscate --split-debug-info=simbolos_apk", "APK ofuscado"),
                "As opções também aceitam a forma --opcao valor (com espaço)."));
        }
    }
}
