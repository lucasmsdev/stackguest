using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerQuest
{
    static partial class Challenges
    {
        // «p1|p2|...» com todas as ordens possíveis dos argumentos nomeados (separados por ", ")
        static string ExtraFlutter_Perm(params string[] itens)
        {
            var res = new List<string>();
            ExtraFlutter_PermRec(new List<string>(itens), new List<string>(), res);
            return "«" + string.Join("|", res) + "»";
        }

        static void ExtraFlutter_PermRec(List<string> resto, List<string> atual, List<string> res)
        {
            if (resto.Count == 0) { res.Add(string.Join(", ", atual)); return; }
            for (int i = 0; i < resto.Count; i++)
            {
                var r = new List<string>(resto); r.RemoveAt(i);
                var a = new List<string>(atual); a.Add(resto[i]);
                ExtraFlutter_PermRec(r, a, res);
            }
        }

        // comandos de terminal: expande «a|b» sem mexer em vírgulas
        static string[] ExtraFlutter_Cmd(params string[] modelos)
        {
            var res = new List<string>();
            foreach (var m in modelos)
                foreach (var e in FltAlt(m))
                    if (!res.Contains(e)) res.Add(e);
            return res.ToArray();
        }

        static partial void ExtraFlutter(List<Level> list)
        {
            const string L = "flutter";
            Func<string[], string> P = itens => ExtraFlutter_Perm(itens);

            // ═════════════════════════════ INICIANTE ═════════════════════════════
            list.Add(C(L, 0, "cmd", "Check-up do ambiente",
                "Primeiro dia no time Mobile! O Bruno instalou o Flutter na sua máquina e quer saber se está faltando alguma coisa (Android SDK, licenças, editor...).",
                "Rode o comando do Flutter que diagnostica o ambiente de desenvolvimento e lista o que está faltando.",
                A("flutter doctor", "flutter doctor -v", "flutter doctor --verbose"),
                A("A ferramenta flutter tem um comando que 'examina' a instalação, como uma consulta médica.", "flutter d_____"),
                A("flutter doctor", "Confere Flutter, Android toolchain, Xcode (no Mac), Chrome e editores: [✓] ok, [!] aviso, [✗] problema."),
                "Use flutter doctor -v para ver os detalhes. Se faltar aceitar as licenças do Android: flutter doctor --android-licenses."));

            list.Add(C(L, 0, "code", "Contador de pedidos",
                "O app do Café 365 vai contar quantos pedidos o cliente fez na sessão. Esse número muda o tempo todo.",
                "Declare com var a variável pedidos começando com o valor 0.",
                A("var pedidos = 0;"),
                A("Para valores que mudam, não use final nem const.", "var nome = valor;"),
                A("var", "O Dart deduz o tipo pelo valor: aqui, int.", "pedidos = 0", "Valor inicial; depois dá para fazer pedidos++ ou pedidos = 5."),
                "Com var o tipo fica fixo depois de deduzido: pedidos = 'cinco' dá erro. Quando o valor não muda, prefira final."));

            list.Add(C(L, 0, "code", "Saudação personalizada",
                "Quando o cliente entra no app, o Kenji quer ver no console uma saudação com o nome dele, que está na variável nome.",
                "Use print para mostrar Olá, seguido do valor de nome e de um ponto de exclamação (ex.: Olá, Ana!), usando interpolação de string com $.",
                A("print(\"Olá, $nome!\");", "print(\"Olá, ${nome}!\")"),
                A("Dentro de uma string em Dart, $variavel é trocado pelo valor da variável.", "print('Olá, $____!');"),
                A("'Olá, $nome!'", "Interpolação: $nome vira o valor da variável.", "${...}", "Com chaves cabe uma expressão: '${nome.toUpperCase()}'."),
                "Interpolação é mais legível que concatenar com +. Para expressões, use ${...}: 'Você tem ${itens.length} itens'."));

            list.Add(C(L, 0, "code", "Bem no centro",
                "A tela de boas-vindas está com o texto grudado no canto superior esquerdo. A Paula quer ele no meio da tela.",
                "Envolva um Text('Bem-vindo') com o widget Center (parâmetro child).",
                FltAns("Center(child: Text(\"Bem-vindo\"))", "«|const »Center(child: «|const »Text(\"Bem-vindo\"))"),
                A("Existe um widget cujo único trabalho é centralizar o filho.", "Center(child: ...)"),
                A("Center", "Centraliza o filho no espaço disponível (na horizontal e na vertical).", "child:", "Parâmetro nomeado com o único widget filho."),
                "Widgets de um filho só usam child; os de vários filhos (Row, Column) usam children: [...]."));

            list.Add(C(L, 0, "cmd", "Quais aparelhos?",
                "Você plugou no USB o celular de testes da Paula e quer conferir se o Flutter enxerga o aparelho (e o emulador).",
                "Liste os dispositivos conectados que o Flutter reconhece.",
                A("flutter devices"),
                A("O comando tem o nome, em inglês, do que você quer listar.", "flutter d______"),
                A("flutter devices", "Mostra nome, id e plataforma de cada celular, emulador ou navegador disponível."),
                "O id que aparece aqui é o usado em flutter run -d <id> para escolher onde rodar o app."));

            list.Add(C(L, 0, "code", "Loja aberta?",
                "O app mostra um selo 'Aberto agora' quando a loja está funcionando. O Bruno quer uma variável com tipo explícito para esse sim/não.",
                "Declare a variável aberta do tipo bool com o valor true.",
                A("bool aberta = true;"),
                A("O tipo para verdadeiro/falso em Dart tem 4 letras.", "bool nome = true;"),
                A("bool", "Só aceita true ou false.", "aberta = true", "Nome e valor inicial."),
                "Outros tipos básicos: int (inteiro), double (decimal), String (texto) e num (int ou double)."));

            list.Add(C(L, 0, "code", "Espaço entre os itens",
                "Na tela de detalhes, o título e o preço estão colados um no outro dentro da Column.",
                "Crie um SizedBox vazio (sem filho) com altura (height) 16 para separar os dois.",
                FltAns("SizedBox(height: 16)", "«|const »SizedBox(height: «16|16.0»)"),
                A("Uma caixa de tamanho fixo e sem filho serve de espaçador.", "SizedBox(height: __)"),
                A("SizedBox", "Caixa com largura e/ou altura fixas.", "height: 16", "Altura em pixels lógicos; numa Row, use width."),
                "Escreva const SizedBox(height: 16): sendo const, o Flutter reaproveita o widget em vez de recriá-lo a cada build."));

            list.Add(C(L, 0, "code", "Mais uma bebida",
                "O Bruno lançou um chá no cardápio. A lista bebidas (final bebidas = ['Espresso', 'Latte', 'Mocha'];) precisa recebê-lo.",
                "Acrescente 'Chá' no fim da lista bebidas.",
                A("bebidas.add(\"Chá\");"),
                A("Listas têm um método para acrescentar um item no fim.", "bebidas.___('Chá');"),
                A("bebidas.add('Chá')", "Coloca o item no fim: agora bebidas.length vale 4."),
                "final não impede o add: impede só trocar a lista inteira. Outros métodos úteis: insert(0, x), remove(x) e contains(x)."));

            list.Add(C(L, 0, "cmd", "Rodando os testes",
                "O Kenji escreveu testes automatizados na pasta test/. Antes de entregar qualquer mudança, rode todos eles.",
                "Execute todos os testes do projeto Flutter.",
                A("flutter test"),
                A("O nome do comando é a palavra 'teste' em inglês.", "flutter ____"),
                A("flutter test", "Roda todos os arquivos *_test.dart da pasta test/ e mostra quantos passaram."),
                "Rode os testes antes de cada entrega: descobrir um bug só depois da publicação na loja custa muito mais caro."));

            list.Add(C(L, 0, "code", "Taxa fixa",
                "A taxa de serviço do Café 365 é sempre 10% e já é conhecida antes mesmo de o app rodar.",
                "Declare uma constante de compilação (const) chamada taxaServico valendo 0.1.",
                A("const taxaServico = 0.1;", "const double taxaServico = 0.1"),
                A("Para valores fixos já na compilação, a palavra é mais forte que final.", "const nome = valor;"),
                A("const", "Valor fixo em tempo de compilação.", "0.1", "Um double (10%)."),
                "final: definido uma vez, em tempo de execução (pode ser DateTime.now()). const: conhecido na compilação (não pode)."));

            list.Add(C(L, 0, "code", "Ícone de café",
                "O botão de pedidos vai ganhar o desenho de uma xícara. O Material Design já tem esse ícone pronto, chamado coffee.",
                "Crie um widget Icon com o ícone coffee da classe Icons.",
                FltAns("Icon(Icons.coffee)", "const Icon(Icons.coffee)"),
                A("O widget é Icon e os desenhos prontos ficam na classe Icons.", "Icon(Icons.______)"),
                A("Icon(...)", "Widget que desenha um ícone.", "Icons.coffee", "Um dos milhares de ícones do Material."),
                "Mude tamanho e cor com parâmetros nomeados: Icon(Icons.coffee, size: 32, color: Colors.brown)."));

            list.Add(C(L, 0, "code", "Barra de título",
                "Toda tela do app precisa de uma barra no topo com o nome da loja.",
                "Crie uma AppBar com title: um Text com 'Café 365'.",
                FltAns("AppBar(title: Text(\"Café 365\"))", "AppBar(title: «|const »Text(\"Café 365\"))", "const AppBar(title: Text(\"Café 365\"))"),
                A("A barra do topo é a AppBar; o texto vai no parâmetro title.", "AppBar(title: Text('...'))"),
                A("AppBar", "Barra superior padrão do Material.", "title: Text('Café 365')", "Widget mostrado como título."),
                "A AppBar entra no Scaffold (appBar: AppBar(...)) e ganha o botão de voltar automaticamente quando existe uma tela anterior."));

            list.Add(C(L, 0, "cmd", "Rodar no navegador",
                "A Paula quer ver o app rodando no Chrome, sem precisar abrir o emulador.",
                "Rode o app escolhendo o dispositivo chrome.",
                A("flutter run -d chrome", "flutter run --device-id chrome", "flutter run --device-id=chrome"),
                A("O flutter run tem uma opção para escolher o dispositivo pelo id.", "flutter run -d ______"),
                A("flutter run", "Compila e abre o app.", "-d chrome", "Escolhe o dispositivo pelo id (os ids aparecem em flutter devices)."),
                "Com vários aparelhos ligados, o -d evita a pergunta 'qual dispositivo?'. flutter run -d all roda em todos de uma vez."));

            list.Add(C(L, 0, "code", "Tabela de preços",
                "O Kenji quer consultar o preço de cada bebida pelo nome, sem percorrer lista nenhuma.",
                "Declare um Map final chamado precos com a chave 'Latte' valendo 9.5 e a chave 'Mocha' valendo 12.5, nessa ordem.",
                FltAns("final precos = {\"Latte\": 9.5, \"Mocha\": 12.5};",
                       "final «Map<String, double> precos = |precos = <String, double>»{\"Latte\": 9.5, \"Mocha\": 12.5}"),
                A("Map usa chaves { } com pares chave: valor separados por vírgula.", "final precos = {'Latte': 9.5, 'Mocha': ____};"),
                A("{'Latte': 9.5, 'Mocha': 12.5}", "Literal de Map: o Dart deduz Map<String, double>.", "precos['Latte']", "Lê o valor de uma chave (9.5); chave que não existe dá null."),
                "Como precos['Chá'] pode ser null, o resultado é double?: trate com ?? (precos['Chá'] ?? 0)."));

            list.Add(C(L, 0, "code", "Tamanhos sem repetição",
                "Os tamanhos de copo do Café 365 são P, M e G, e nenhum pode aparecer duas vezes. Uma lista comum não garante isso.",
                "Declare um Set final chamado tamanhos com os textos P, M e G, nessa ordem.",
                FltAns("final tamanhos = {\"P\", \"M\", \"G\"};",
                       "final «Set<String> tamanhos = |tamanhos = <String>»{\"P\", \"M\", \"G\"}"),
                A("Set usa chaves { }, como o Map, mas só com valores (sem chave: valor).", "final tamanhos = {'P', ...};"),
                A("{'P', 'M', 'G'}", "Literal de Set: cada item aparece uma vez só.", "tamanhos.add('M')", "Não duplica: o M já existe, o Set continua com 3 itens."),
                "Cuidado: {} sozinho é um Map vazio! Para um Set vazio, escreva <String>{}."));

            list.Add(C(L, 0, "code", "Logo do app",
                "O designer mandou o arquivo logo.png. O Kenji colocou na pasta assets/ do projeto e declarou no pubspec.yaml.",
                "Mostre a imagem assets/logo.png usando o construtor Image.asset.",
                FltAns("Image.asset(\"assets/logo.png\")"),
                A("Imagens que vão dentro do app usam Image.asset; as da internet, Image.network.", "Image.asset('pasta/arquivo.png')"),
                A("Image.asset", "Carrega uma imagem empacotada no app.", "'assets/logo.png'", "Caminho a partir da raiz do projeto, igual ao declarado no pubspec.yaml."),
                "Se aparecer 'Unable to load asset', confira a seção flutter: assets: do pubspec.yaml e reinicie o app (o hot reload não pega assets novos)."));

            list.Add(C(L, 0, "cmd", "Código bem formatado",
                "A revisão de código voltou com 30 comentários sobre espaços e recuo. O Bruno diz: deixa a ferramenta formatar.",
                "Formate, com a ferramenta dart, todos os arquivos Dart da pasta atual (.).",
                A("dart format ."),
                A("O SDK do Dart tem um subcomando que aplica o estilo oficial de formatação.", "dart ______ ."),
                A("dart format", "Reescreve os arquivos no estilo oficial do Dart.", ".", "A pasta atual (e as subpastas)."),
                "Configure o editor para formatar ao salvar: o time inteiro escreve no mesmo estilo e a revisão foca no que importa."));

            list.Add(C(L, 0, "code", "Função somar",
                "O carrinho precisa somar quantidades de itens. O Kenji pediu uma função bem simples.",
                "Escreva a função somar que recebe dois int (a e b, nessa ordem) e retorna um int com a + b.",
                FltAns("int somar(int a, int b) => a + b;",
                       "int somar(int a, int b) «=> b + a|{\n  return «a + b|b + a»;\n}»"),
                A("O tipo de retorno vem antes do nome; os parâmetros, com tipo, entre parênteses.", "int somar(int a, int b) => ...;"),
                A("int somar(int a, int b)", "Retorna int e recebe dois parâmetros posicionais.", "=> a + b", "Arrow: atalho para { return a + b; }."),
                "Parâmetros posicionais dependem da ordem; quando são muitos, prefira nomeados ({required ...})."));

            list.Add(C(L, 0, "code", "Ponto de partida",
                "Todo app Flutter começa pela função main, em lib/main.dart. O Kenji apagou a dele sem querer!",
                "Escreva a função main (retorno void) que inicia o app chamando runApp com o widget CafeApp().",
                FltAns("void main() {\n  runApp(CafeApp());\n}",
                       "void main() «=> runApp(«|const »CafeApp())|{ runApp(«|const »CafeApp()); }»"),
                A("main é a porta de entrada; quem coloca o widget raiz na tela é o runApp.", "void main() { runApp(______); }"),
                A("void main()", "Função por onde todo programa Dart começa.", "runApp(CafeApp())", "Infla o widget raiz e o prende na tela."),
                "Se CafeApp tiver construtor const, escreva runApp(const CafeApp()). O CafeApp normalmente devolve um MaterialApp."));

            list.Add(C(L, 0, "code", "Aviso de esgotado",
                "Quando o estoque de um doce chega a zero, o Kenji quer um aviso no console para investigar.",
                "Escreva um if que, quando estoque for igual a 0, chame print('Esgotado').",
                FltAns("if (estoque == 0) {\n  print(\"Esgotado\");\n}",
                       "if (estoque == 0) «print(\"Esgotado\");|{ print(\"Esgotado\"); }»"),
                A("Comparação de igualdade usa dois sinais de igual.", "if (estoque == 0) { ... }"),
                A("if (estoque == 0)", "Só executa o bloco quando a condição é verdadeira.", "==", "Compara; um = só seria atribuição (e nem compila dentro do if)."),
                "Para mais casos, use else if / else. Em Dart a condição precisa ser bool: if (estoque) não compila."));

            list.Add(C(L, 0, "cmd", "Pente-fino no código",
                "Antes de abrir o pull request, o Bruno sempre procura erros, avisos e más práticas no código sem nem rodar o app.",
                "Rode a análise estática (erros, avisos e lints) do projeto Flutter.",
                A("flutter analyze", "dart analyze"),
                A("O verbo é 'analisar' em inglês.", "flutter _______"),
                A("flutter analyze", "Lê o código e aponta erros de tipo, avisos e as regras (lints) do analysis_options.yaml."),
                "Mantenha zero avisos: o analisador pega bugs reais, como usar o context depois de um await (use_build_context_synchronously)."));

            list.Add(C(L, 0, "code", "Voltar uma tela",
                "Na tela de detalhes do produto, o botão 'Fechar' precisa levar o cliente de volta ao cardápio.",
                "Feche a tela atual, voltando para a anterior, usando o Navigator e o context.",
                FltAns("Navigator.pop(context)", "Navigator.of(context).pop()"),
                A("Se o push empilha uma tela, a operação contrária desempilha.", "Navigator.___(context)"),
                A("Navigator.pop(context)", "Remove a tela do topo da pilha: a de baixo volta a aparecer."),
                "Dá para devolver um valor para quem abriu a tela: Navigator.pop(context, valor)."));

            list.Add(C(L, 0, "code", "Caixa marrom",
                "O selo de promoção é um quadrado marrom de 100 por 100 no canto do cartão do produto.",
                "Crie um Container com width 100, height 100 e color Colors.brown.",
                FltAns("Container(width: 100, height: 100, color: Colors.brown)",
                       "Container(" + P(new[] { "width: 100", "height: 100", "color: Colors.brown" }) + ")"),
                A("O Container aceita tamanho e cor como parâmetros nomeados.", "Container(width: ..., height: ..., color: Colors._____)"),
                A("width / height", "Tamanho em pixels lógicos.", "color: Colors.brown", "Cor de fundo (Colors tem a paleta do Material)."),
                "Para bordas arredondadas, use decoration: BoxDecoration(color: ..., borderRadius: BorderRadius.circular(8)) — e aí a cor sai do Container (não dá para usar color e decoration juntos)."));

            list.Add(C(L, 0, "cmd", "Faxina no build",
                "Depois de trocar de branch, o app começou a dar erros estranhos de compilação. O Kenji sugere apagar os arquivos gerados e começar do zero.",
                "Apague a pasta build e os demais arquivos gerados pelo Flutter no projeto.",
                A("flutter clean"),
                A("O comando tem o nome de 'limpar' em inglês.", "flutter _____"),
                A("flutter clean", "Apaga build/, .dart_tool/ e outros arquivos gerados; o próximo build demora mais."),
                "Depois do clean, rode flutter pub get (o .dart_tool foi apagado) e então flutter run."));

            // ═════════════════════════════ INTERMEDIÁRIO ═════════════════════════════
            list.Add(C(L, 1, "code", "Percorrendo o cardápio",
                "O Kenji quer conferir no console cada bebida da lista bebidas, com uma frase amigável.",
                "Com um for-in (variável bebida, declarada com final), percorra bebidas e, para cada uma, chame print com 'Hoje tem ' seguido da bebida, usando interpolação.",
                FltAns("for (final bebida in bebidas) {\n  print(\"Hoje tem $bebida\");\n}",
                       "for (final bebida in bebidas) «print(\"Hoje tem «$bebida|${bebida}»\");|{ print(\"Hoje tem «$bebida|${bebida}»\"); }»"),
                A("for (final item in lista) percorre a lista item a item.", "for (final bebida in bebidas) { print('Hoje tem $______'); }"),
                A("for (final bebida in bebidas)", "A cada volta, bebida recebe o próximo item.", "'Hoje tem $bebida'", "A interpolação monta a frase."),
                "Use for-in quando não precisa do índice; se precisar, for (var i = 0; i < bebidas.length; i++)."));

            list.Add(C(L, 1, "cmd", "Projeto da empresa",
                "O app de pedidos vai para as lojas e precisa do identificador da Micorosoft (com.micorosoft.pedidos_app), e não do padrão com.example.",
                "Crie o projeto pedidos_app com a organização com.micorosoft.",
                ExtraFlutter_Cmd("flutter create --org com.micorosoft pedidos_app", "flutter create pedidos_app --org com.micorosoft",
                                 "flutter create --org=com.micorosoft pedidos_app", "flutter create pedidos_app --org=com.micorosoft"),
                A("O flutter create tem uma opção para o domínio invertido da empresa.", "flutter create --org com.__________ pedidos_app"),
                A("--org com.micorosoft", "Prefixo do pacote Android / bundle iOS: com.micorosoft.pedidos_app.", "pedidos_app", "Nome do projeto (minúsculas e _)."),
                "Depois de publicado, o identificador não muda: para a loja seria outro app. Defina a --org certa desde o início."));

            list.Add(C(L, 1, "code", "Nome e preço nas pontas",
                "Em cada linha do cardápio, o nome deve ficar colado à esquerda e o preço colado à direita.",
                "Monte uma Row com mainAxisAlignment MainAxisAlignment.spaceBetween e children com os widgets nome e preco (nessa ordem).",
                FltAns("Row(\n  mainAxisAlignment: MainAxisAlignment.spaceBetween,\n  children: [nome, preco],\n)",
                       "Row(«mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [nome, preco]|children: [nome, preco], mainAxisAlignment: MainAxisAlignment.spaceBetween»)"),
                A("Na Row, o eixo principal é o horizontal; o alinhamento nele é o mainAxisAlignment.", "Row(mainAxisAlignment: MainAxisAlignment.____________, children: [nome, preco])"),
                A("mainAxisAlignment", "Distribui os filhos no eixo principal (horizontal, na Row).", "MainAxisAlignment.spaceBetween", "Primeiro no início, último no fim, espaço igual entre eles."),
                "Outros valores: start, center, end, spaceAround e spaceEvenly. O crossAxisAlignment cuida do outro eixo."));

            list.Add(C(L, 1, "code", "Classe Pedido",
                "Chega de Map solto pelo código: o Bruno quer um tipo próprio para os pedidos.",
                "Crie a classe Pedido com os campos final int numero e final double total (nessa ordem) e um construtor posicional que preencha os dois com this.numero e this.total.",
                FltAns("class Pedido {\n  final int numero;\n  final double total;\n\n  Pedido(this.numero, this.total);\n}",
                       "class Pedido { final int numero; final double total; «|const »Pedido(this.numero, this.total); }",
                       "class Pedido { «|const »Pedido(this.numero, this.total); final int numero; final double total; }"),
                A("Os campos ficam no corpo da classe; o construtor tem o mesmo nome da classe.", "class Pedido { final int numero; final double total; Pedido(this.______, this.______); }"),
                A("final int numero;", "Campo imutável.", "Pedido(this.numero, this.total);", "Atalho do Dart: recebe os valores e já guarda nos campos.", "Pedido(42, 19.9)", "Uso: cria um objeto (o new é opcional)."),
                "Com todos os campos final, o construtor pode ser const, permitindo const Pedido(1, 9.5)."));

            list.Add(C(L, 1, "code", "Pontos de fidelidade",
                "A classe Cliente tem os campos final String nome e final int pontos. Todo cliente novo começa com 0 pontos, mas o nome é obrigatório.",
                "Escreva o construtor de Cliente com parâmetros nomeados: nome obrigatório (required this.nome) e pontos com valor padrão 0 (this.pontos = 0), nessa ordem.",
                FltAns("Cliente({required this.nome, this.pontos = 0});", "«|const »Cliente({required this.nome, this.pontos = 0})"),
                A("Parâmetros nomeados ficam entre { }. required obriga; = valor define o padrão.", "Cliente({required this.nome, this.pontos = _});"),
                A("required this.nome", "Quem chama é obrigado a passar nome:.", "this.pontos = 0", "Opcional: se não vier, vale 0.", "Cliente(nome: 'Rui')", "Uso: pontos fica 0."),
                "Parâmetro nomeado sem required precisa de um valor padrão ou de um tipo anulável (int? pontos), por causa do null safety."));

            list.Add(C(L, 1, "code", "Foto do produto",
                "As fotos dos produtos vêm do servidor. O endereço de cada uma está em fotoUrl e a miniatura tem tamanho fixo.",
                "Mostre a imagem de fotoUrl com Image.network, com width 80, height 80 e fit BoxFit.cover.",
                FltAns("Image.network(\n  fotoUrl,\n  width: 80,\n  height: 80,\n  fit: BoxFit.cover,\n)",
                       "Image.network(fotoUrl, " + P(new[] { "width: 80", "height: 80", "fit: BoxFit.cover" }) + ")"),
                A("Image.network recebe o endereço como 1º argumento; o resto são parâmetros nomeados.", "Image.network(fotoUrl, width: 80, height: 80, fit: BoxFit._____)"),
                A("Image.network(fotoUrl, ...)", "Baixa e mostra a imagem do endereço.", "fit: BoxFit.cover", "Preenche os 80x80 sem distorcer (corta o que sobra)."),
                "Imagens da internet podem demorar ou falhar: o parâmetro errorBuilder mostra outra coisa no lugar quando dá erro."));

            list.Add(C(L, 1, "cmd", "Release no aparelho",
                "A Paula quer testar no emulador emulator-5554 o app otimizado, como o cliente vai receber (o modo debug é bem mais lento).",
                "Rode o app em modo release no dispositivo de id emulator-5554.",
                ExtraFlutter_Cmd("flutter run --release -d emulator-5554",
                                 "flutter run --release «--device-id emulator-5554|--device-id=emulator-5554»"),
                A("Combine duas opções do flutter run: uma escolhe o modo de compilação e a outra o dispositivo.", "flutter run --_______ -d emulator-5554"),
                A("--release", "Compila otimizado, sem as ferramentas de debug (não tem hot reload).", "-d emulator-5554", "Id do dispositivo, como aparece em flutter devices."),
                "Nunca julgue a velocidade do app pelo modo debug. Para medir desempenho com ferramentas, existe o modo --profile."));

            list.Add(C(L, 1, "code", "Esqueleto da tela",
                "A tela do cardápio precisa da estrutura padrão do Material: barra no topo e conteúdo embaixo. O conteúdo já está pronto no widget lista.",
                "Monte um Scaffold com appBar: uma AppBar com title Text('Cardápio'), e body: lista.",
                FltAns("Scaffold(\n  appBar: AppBar(title: Text(\"Cardápio\")),\n  body: lista,\n)",
                       FltSub("Scaffold(«appBar: #A, body: lista|body: lista, appBar: #A»)", "#A", "«AppBar(title: «|const »Text(\"Cardápio\"))|const AppBar(title: Text(\"Cardápio\"))»")),
                A("O Scaffold tem um parâmetro para a barra (appBar) e outro para o conteúdo (body).", "Scaffold(appBar: AppBar(title: Text('...')), body: _____)"),
                A("Scaffold", "Estrutura base de uma tela Material.", "appBar: AppBar(...)", "Barra superior.", "body: lista", "Conteúdo principal da tela."),
                "O Scaffold também tem floatingActionButton, drawer e bottomNavigationBar — e é dentro dele que as SnackBars aparecem."));

            list.Add(C(L, 1, "code", "Botão de mais um",
                "Na tela do produto (um StatefulWidget), um botão com o ícone + deve aumentar a variável quantidade e atualizar a tela.",
                "Crie um IconButton com icon: Icon(Icons.add) e onPressed: uma função anônima que chama setState incrementando quantidade.",
                FltAns("IconButton(\n  icon: Icon(Icons.add),\n  onPressed: () {\n    setState(() {\n      quantidade++;\n    });\n  },\n)",
                       FltSub("IconButton(«icon: #I, onPressed: #O|onPressed: #O, icon: #I»)",
                              "#I", "«|const »Icon(Icons.add)",
                              "#O", "«() => #S|() { #S; }»",
                              "#S", "setState(() «=> #Q|{ #Q; }»)",
                              "#Q", "«quantidade++|quantidade += 1|++quantidade|quantidade = quantidade + 1»")),
                A("onPressed recebe uma função; dentro dela, a mudança vai no callback do setState.", "IconButton(icon: Icon(Icons.add), onPressed: () { setState(() { quantidade++; }); })"),
                A("IconButton", "Botão só com ícone.", "onPressed: () { ... }", "Função anônima: só roda no toque.", "setState(() { quantidade++; })", "Muda o estado e pede para redesenhar."),
                "Escrever onPressed: setState(...) (sem o () { }) executaria na hora do build — e daria erro."));

            list.Add(C(L, 1, "code", "Só os nomes",
                "A busca precisa de uma lista apenas com os nomes dos produtos. Você tem produtos (List<Produto>, cada um com nome e preco).",
                "Crie nomes (final) transformando produtos com map — cada produto p vira p.nome — e converta o resultado para List com toList().",
                FltAns("final nomes = produtos.map((p) => p.nome).toList();",
                       "final «List<String> |»nomes = produtos.map«|<String>»((p) => p.nome).toList()"),
                A("map transforma cada item; ele devolve um Iterable, por isso o toList() no fim.", "final nomes = produtos.map((p) => p.____).toList();"),
                A("map((p) => p.nome)", "Aplica a função a cada item.", ".toList()", "O map devolve um Iterable (preguiçoso); toList cria a List de verdade."),
                "Dá para encadear: produtos.where(...).map(...).toList(). Sem o toList, o map é recalculado toda vez que alguém percorre o resultado."));

            list.Add(C(L, 1, "code", "Bebidas baratas",
                "A vitrine 'Até R$ 10' mostra só os produtos com preço menor que 10. A lista completa está em produtos (List<Produto>).",
                "Crie baratos (final) com os itens de produtos cujo preco seja menor que 10, usando where (parâmetro p) e toList().",
                FltAns("final baratos = produtos.where((p) => p.preco < 10).toList();",
                       "final «List<Produto> |»baratos = produtos.where((p) => «p.preco < 10|p.preco < 10.0|10 > p.preco»).toList()"),
                A("where recebe uma função que devolve true para os itens que devem ficar.", "produtos.where((p) => p.preco < __).toList()"),
                A("where((p) => p.preco < 10)", "Mantém só quem passa no teste.", ".toList()", "Converte o Iterable em List."),
                "Para achar só o primeiro, use firstWhere; para saber se algum passa no teste, any((p) => ...)."));

            list.Add(C(L, 1, "cmd", "Dependência de teste",
                "O Kenji quer simular a API nos testes com o pacote mocktail. Ele só é usado nos testes, então não deve ir para o app final.",
                "Adicione o pacote mocktail como dependência de desenvolvimento (dev_dependencies).",
                ExtraFlutter_Cmd("flutter pub add dev:mocktail", "«flutter|dart» pub add «dev:mocktail|--dev mocktail|mocktail --dev|-d mocktail|mocktail -d»"),
                A("O pub add tem uma forma de mandar o pacote para dev_dependencies.", "flutter pub add dev:________   (ou --dev)"),
                A("dev:mocktail", "Prefixo que coloca o pacote em dev_dependencies.", "--dev", "Forma equivalente."),
                "dev_dependencies (testes, lints, geradores de código) não entram no app publicado, que fica menor."));

            list.Add(C(L, 1, "code", "Total formatado",
                "O total do pedido (double total) aparece como 21.0 ou 9.899999. A Paula quer sempre duas casas: Total: R$ 21.00.",
                "Crie um Text que mostre 'Total: R$ ' seguido de total com 2 casas decimais (toStringAsFixed), usando interpolação ${...}. Lembre: o $ do R$ precisa ser escapado.",
                FltAns("Text(\"Total: R\\$ ${total.toStringAsFixed(2)}\")"),
                A("Dentro de ${ } cabe qualquer expressão, como a chamada de um método. Para um $ literal, use \\$.", "Text('Total: R\\$ ${total.toStringAsFixed(_)}')"),
                A("R\\$", "\\$ é um cifrão literal (sem a barra, o Dart tentaria interpolar).", "${total.toStringAsFixed(2)}", "Expressão dentro da string: o double com 2 casas, como texto."),
                "Para moeda no formato brasileiro (R$ 21,00), use o pacote intl: NumberFormat.currency(locale: 'pt_BR', symbol: 'R\\$')."));

            list.Add(C(L, 1, "code", "Widget sem estado",
                "O cabeçalho de boas-vindas nunca muda depois de desenhado. O Bruno quer um widget próprio para ele.",
                "Crie a classe Saudacao que estende StatelessWidget e sobrescreve (@override) o método build (recebe BuildContext context e retorna Widget), devolvendo Text('Bem-vindo ao Café 365').",
                FltAns("class Saudacao extends StatelessWidget {\n  @override\n  Widget build(BuildContext context) {\n    return Text(\"Bem-vindo ao Café 365\");\n  }\n}",
                       FltSub("class Saudacao extends StatelessWidget { «|const Saudacao({super.key}); |const Saudacao({Key? key}) : super(key: key); »@override Widget build(BuildContext context) «=> #T;|{ return #T; }» }",
                              "#T", "«|const »Text(\"Bem-vindo ao Café 365\")")),
                A("Widgets sem estado estendem StatelessWidget e só precisam do método build.", "class Saudacao extends StatelessWidget { @override Widget build(BuildContext context) { return ...; } }"),
                A("extends StatelessWidget", "Widget que só depende do que recebe: não muda sozinho.", "@override Widget build(BuildContext context)", "Descreve o que desenhar.", "return Text(...)", "A árvore de widgets desta parte da tela."),
                "Se o widget precisar mudar sozinho (contador, carregamento), use StatefulWidget + setState. Na dúvida, comece Stateless."));

            list.Add(C(L, 1, "code", "Latte é uma bebida",
                "Existe a classe Bebida com o método String descricao(). O Latte precisa de uma descrição própria.",
                "Crie a classe Latte que estende Bebida e sobrescreve (@override) descricao, retornando 'Latte cremoso'.",
                FltAns("class Latte extends Bebida {\n  @override\n  String descricao() => \"Latte cremoso\";\n}",
                       "class Latte extends Bebida { @override String descricao() «=> \"Latte cremoso\";|{ return \"Latte cremoso\"; }» }"),
                A("Herança usa extends; para trocar um método da classe-mãe, repita a assinatura com @override.", "class Latte extends Bebida { @override String descricao() => '...'; }"),
                A("class Latte extends Bebida", "Latte herda tudo de Bebida.", "@override", "Avisa que o método substitui o da classe-mãe (o analisador confere).", "String descricao() => 'Latte cremoso'", "A nova implementação."),
                "Para aproveitar o comportamento original dentro da versão nova, chame super.descricao()."));

            list.Add(C(L, 1, "code", "Contando pedidos",
                "Já existe a função Future<List<String>> buscarPedidos(). A tela inicial só precisa saber quantos pedidos existem.",
                "Escreva a função assíncrona contarPedidos, que retorna Future<int>: guarde em lista (final) o resultado de buscarPedidos() com await e retorne lista.length.",
                FltAns("Future<int> contarPedidos() async {\n  final lista = await buscarPedidos();\n  return lista.length;\n}",
                       "Future<int> contarPedidos() async { final «List<String> |»lista = await buscarPedidos(); return lista.length; }"),
                A("A função precisa de async para poder usar await; o valor do return vira o resultado do Future.", "Future<int> contarPedidos() async { final lista = await buscarPedidos(); return ...; }"),
                A("Future<int> ... async", "Promete devolver um int no futuro.", "await buscarPedidos()", "Espera a lista sem travar a tela.", "return lista.length", "Vira o valor do Future<int>."),
                "Quem chama também espera: final n = await contarPedidos(); — ou usa um FutureBuilder na tela."));

            list.Add(C(L, 1, "cmd", "Correções automáticas",
                "O flutter analyze mostrou 40 avisos como 'prefer_const_constructors'. Muitos deles têm correção automática.",
                "Aplique, com a ferramenta dart, as correções automáticas sugeridas pelo analisador no projeto.",
                A("dart fix --apply"),
                A("O comando dart tem um subcomando 'consertar'; para gravar de verdade, é preciso a opção de aplicar.", "dart fix --_____"),
                A("dart fix", "Corrige automaticamente avisos que têm conserto conhecido.", "--apply", "Grava as mudanças (com --dry-run, só mostra o que faria)."),
                "Rode dart fix --dry-run antes para ver o que vai mudar, e revise o diff: correção automática também merece revisão."));

            list.Add(C(L, 1, "code", "Sem faixa de debug",
                "Nas capturas de tela para a loja aparece uma faixa vermelha 'DEBUG' no canto. O Bruno quer o MaterialApp sem ela.",
                "Monte um MaterialApp com debugShowCheckedModeBanner: false e home: HomePage().",
                FltAns("MaterialApp(\n  debugShowCheckedModeBanner: false,\n  home: HomePage(),\n)",
                       FltSub("«|const »MaterialApp(«#D, #H|#H, #D»)", "#D", "debugShowCheckedModeBanner: false", "#H", "home: «|const »HomePage()")),
                A("O MaterialApp tem um parâmetro booleano para a faixa e outro para a tela inicial (home).", "MaterialApp(debugShowCheckedModeBanner: _____, home: HomePage())"),
                A("MaterialApp", "Raiz de um app Material: tema, rotas e tela inicial.", "debugShowCheckedModeBanner: false", "Esconde a faixa DEBUG (no build de release ela já não aparece).", "home: HomePage()", "A primeira tela."),
                "No MaterialApp também ficam o tema (theme: ThemeData(colorSchemeSeed: Colors.brown)) e as rotas nomeadas (routes)."));

            list.Add(C(L, 1, "code", "Cartão do produto",
                "A Paula achou as linhas do cardápio sem graça. O Bruno quer cada produto num cartão com nome e descrição.",
                "Crie um Card com child: um ListTile com title Text(nome) e subtitle Text(descricao).",
                FltAns("Card(\n  child: ListTile(\n    title: Text(nome),\n    subtitle: Text(descricao),\n  ),\n)",
                       "Card(child: ListTile(«title: Text(nome), subtitle: Text(descricao)|subtitle: Text(descricao), title: Text(nome)»))"),
                A("Card é a caixa com sombra; ListTile é a linha pronta com título e subtítulo.", "Card(child: ListTile(title: Text(nome), subtitle: ...))"),
                A("Card", "Superfície com cantos arredondados e sombra.", "ListTile(title: ..., subtitle: ...)", "Linha padrão: título em cima, subtítulo menor embaixo."),
                "O ListTile também tem leading (ícone/imagem à esquerda), trailing (à direita) e onTap."));

            list.Add(C(L, 1, "code", "Pausa de dois segundos",
                "Enquanto a API real não fica pronta, o Kenji quer simular a demora da rede dentro de uma função async.",
                "Espere (await) um Future.delayed com uma Duration de 2 segundos.",
                FltAns("await Future.delayed(const Duration(seconds: 2));",
                       "await Future.delayed(«|const »Duration(«seconds: 2|milliseconds: 2000»))"),
                A("Future tem um construtor que só termina depois de um tempo; o tempo é uma Duration.", "await Future.delayed(Duration(seconds: _));"),
                A("Future.delayed(...)", "Future que completa depois do tempo indicado.", "Duration(seconds: 2)", "Intervalo de tempo (também aceita milliseconds, minutes...).", "await", "Pausa só esta função; a tela continua respondendo."),
                "Útil para simular rede em protótipos — mas lembre de tirá-lo do código de produção."));

            list.Add(C(L, 1, "cmd", "Só o carrinho",
                "A suíte inteira de testes demora 3 minutos. Você mexeu só no carrinho e quer rodar apenas o arquivo de teste dele.",
                "Rode somente os testes do arquivo test/carrinho_test.dart.",
                A("flutter test test/carrinho_test.dart", "flutter test ./test/carrinho_test.dart", "flutter test test\\carrinho_test.dart"),
                A("O flutter test aceita o caminho de um arquivo de teste.", "flutter test test/____________.dart"),
                A("flutter test test/carrinho_test.dart", "Roda só esse arquivo: resposta rápida enquanto você trabalha."),
                "Para rodar um único teste pelo nome: flutter test --plain-name 'soma o total'. Antes de entregar, rode a suíte toda."));

            list.Add(C(L, 1, "code", "Pedido com falha",
                "Se a internet cair, enviarPedido() lança uma exceção e o app fecha. O Kenji quer capturar o erro e registrá-lo no console.",
                "Escreva um try que espera (await) enviarPedido() e um catch (e) que chama print com 'Erro: ' seguido de e (interpolação).",
                FltAns("try {\n  await enviarPedido();\n} catch (e) {\n  print(\"Erro: $e\");\n}",
                       "try { await enviarPedido(); } «catch (e)|on Exception catch (e)» { print(\"Erro: «$e|${e}»\"); }"),
                A("O código arriscado vai no bloco try; o tratamento, no catch.", "try { await enviarPedido(); } catch (e) { print('Erro: $e'); }"),
                A("try { ... }", "Tenta executar; se der exceção, pula para o catch.", "catch (e)", "e é o erro capturado.", "print('Erro: $e')", "Registra o erro em vez de derrubar o app."),
                "Na tela, mostre uma mensagem amigável (SnackBar) e use finally para desligar o 'carregando', dê certo ou errado."));

            list.Add(C(L, 1, "code", "Ouvindo os pedidos",
                "A cozinha recebe os pedidos em tempo real pelo Stream<String> pedidosStream. O Kenji quer ver cada pedido no console assim que chega.",
                "Escute pedidosStream com listen, passando uma função anônima que recebe pedido e chama print(pedido).",
                FltAns("pedidosStream.listen((pedido) {\n  print(pedido);\n});", "pedidosStream.listen((pedido) «=> print(pedido)|{ print(pedido); }»)"),
                A("Um Future entrega UM valor; um Stream entrega vários ao longo do tempo. Para reagir a cada um, use listen.", "pedidosStream.listen((pedido) { print(______); });"),
                A("Stream<String>", "Sequência de valores que chegam com o tempo.", "listen((pedido) { ... })", "A função roda para cada valor novo."),
                "O listen devolve uma StreamSubscription: guarde-a e chame cancel() no dispose, senão a escuta continua. Na tela, prefira o StreamBuilder."));

            list.Add(C(L, 1, "code", "Carrinho que avisa",
                "Várias telas mostram a quantidade do carrinho. Com o pacote provider, o Bruno quer que todas se atualizem quando um item entra.",
                "Crie a classe Carrinho que estende ChangeNotifier, com int itens = 0 e o método void adicionar() que incrementa itens e depois chama notifyListeners().",
                FltAns("class Carrinho extends ChangeNotifier {\n  int itens = 0;\n\n  void adicionar() {\n    itens++;\n    notifyListeners();\n  }\n}",
                       FltSub("class Carrinho extends ChangeNotifier { int itens = 0; void adicionar() { #I; notifyListeners(); } }",
                              "#I", "«itens++|itens += 1|++itens|itens = itens + 1»")),
                A("ChangeNotifier dá à classe o poder de avisar quem a observa; o aviso é um método chamado depois da mudança.", "class Carrinho extends ChangeNotifier { int itens = 0; void adicionar() { itens++; _______________(); } }"),
                A("extends ChangeNotifier", "A classe vira 'observável'.", "itens++", "A mudança no estado.", "notifyListeners()", "Avisa os widgets que dependem do Carrinho para redesenharem."),
                "Esqueceu o notifyListeners()? O valor muda, mas nenhuma tela atualiza. Para ligar o Carrinho às telas: ChangeNotifierProvider e context.watch/context.read."));

            // ═════════════════════════════ PROFISSIONAL ═════════════════════════════
            list.Add(C(L, 2, "code", "Provider na raiz",
                "O Carrinho (ChangeNotifier) está pronto, mas as telas não o enxergam: 'Could not find the correct Provider<Carrinho> above this widget'. Ele precisa ficar acima do app inteiro.",
                "Na main, chame runApp passando um ChangeNotifierProvider com create: (context) => Carrinho() e child: CafeApp().",
                FltAns("runApp(\n  ChangeNotifierProvider(\n    create: (context) => Carrinho(),\n    child: CafeApp(),\n  ),\n);",
                       FltSub("runApp(ChangeNotifierProvider«|<Carrinho>»(«#C, #F|#F, #C»))",
                              "#C", "create: «(context)|(_)» => Carrinho()",
                              "#F", "child: «|const »CafeApp()")),
                A("O provider precisa estar ACIMA de quem lê; o lugar mais alto é em volta do app inteiro, dentro do runApp.", "runApp(ChangeNotifierProvider(create: (context) => Carrinho(), child: ______()))"),
                A("ChangeNotifierProvider", "Cria o Carrinho e o disponibiliza para toda a árvore abaixo.", "create: (context) => Carrinho()", "Criado uma vez só (e descartado sozinho com dispose).", "child: CafeApp()", "Tudo dentro do app enxerga o Carrinho."),
                "Com vários estados globais, use MultiProvider(providers: [...], child: ...). Não crie o provider dentro de uma tela que é reconstruída com frequência."));

            list.Add(C(L, 2, "code", "Clique sem watch",
                "No botão 'Adicionar', alguém usou context.watch<Carrinho>() dentro do onPressed e o provider acusa erro: watch só pode ser usado no build. No callback, basta pegar o objeto e chamar o método.",
                "Crie um ElevatedButton com onPressed: uma função anônima que obtém o Carrinho com context.read e chama adicionar(), e child: Text('Adicionar').",
                FltAns("ElevatedButton(\n  onPressed: () => context.read<Carrinho>().adicionar(),\n  child: Text(\"Adicionar\"),\n)",
                       FltSub("ElevatedButton(«onPressed: #O, child: #T|child: #T, onPressed: #O»)",
                              "#O", "«() => #R|() { #R; }»",
                              "#R", "context.read<Carrinho>().adicionar()",
                              "#T", "«|const »Text(\"Adicionar\")")),
                A("watch = assina e redesenha quando mudar (use no build); read = só pega o objeto agora (use em callbacks).", "onPressed: () => context.____<Carrinho>().adicionar()"),
                A("context.read<Carrinho>()", "Pega o Carrinho sem registrar este widget como ouvinte.", ".adicionar()", "Muda o estado; quem usa watch redesenha.", "() => ...", "Só executa no toque."),
                "Forma equivalente mais antiga: Provider.of<Carrinho>(context, listen: false). Regra de bolso: watch no build, read nos eventos."));

            list.Add(C(L, 2, "code", "Contador no ícone",
                "O selo do carrinho na AppBar precisa mostrar o número atualizado sempre que um item é adicionado, em qualquer tela.",
                "Dentro do build, crie um Text que mostre 'Itens: ' seguido de itens do Carrinho, obtido com context.watch, usando interpolação ${...}.",
                FltAns("Text(\"Itens: ${context.watch<Carrinho>().itens}\")"),
                A("Para o widget redesenhar quando o Carrinho avisar, ele precisa assinar com watch, dentro do build.", "Text('Itens: ${context.watch<Carrinho>().______}')"),
                A("context.watch<Carrinho>()", "Lê o Carrinho E inscreve o widget: o notifyListeners() faz ele redesenhar.", "${...}", "Interpolação de uma expressão inteira."),
                "Para redesenhar só quando um campo específico muda, use context.select((Carrinho c) => c.itens): menos rebuilds."));

            list.Add(C(L, 2, "cmd", "Medindo de verdade",
                "A Paula diz que a rolagem do cardápio engasga. Antes de otimizar, o Bruno quer medir no DevTools com o app num modo próximo do release, mas ainda com rastreamento de desempenho.",
                "Rode o app no modo de compilação feito para medir desempenho.",
                A("flutter run --profile"),
                A("Existem 3 modos de compilação: debug, profile e release. Um deles é feito para medir.", "flutter run --_______"),
                A("--profile", "Compilação otimizada como a de release, mas com rastreamento para o DevTools (aba Performance)."),
                "Meça num aparelho físico e no modo profile: o debug é lento por natureza e o emulador não representa o celular do cliente (o profile nem roda em emulador)."));

            list.Add(C(L, 2, "code", "Total do carrinho",
                "O Carrinho agora guarda List<Produto> produtos. O checkout precisa do total sempre certo, sem um campo extra que pode ficar dessincronizado da lista.",
                "Crie um getter double total que soma o preco de todos os produtos com fold, começando de 0 (acumulador soma, item p).",
                FltAns("double get total => produtos.fold(0, (soma, p) => soma + p.preco);",
                       "double get total => produtos.fold«|<double>»(«0|0.0», (soma, p) => soma + p.preco)",
                       "double get total { return produtos.fold«|<double>»(«0|0.0», (soma, p) => soma + p.preco); }"),
                A("fold percorre a lista carregando um acumulador: começa com um valor inicial e soma item a item.", "double get total => produtos.fold(0, (soma, p) => soma + p._____);"),
                A("double get total", "Getter: calculado na hora e usado como campo (carrinho.total).", "fold(0, (soma, p) => soma + p.preco)", "Começa em 0 e soma o preço de cada produto."),
                "Valor derivado se calcula, não se guarda: um campo total separado cedo ou tarde fica diferente da lista. Para dinheiro de verdade, considere guardar centavos em int."));

            list.Add(C(L, 2, "code", "Teste do carrinho",
                "O Bruno exige teste automatizado para toda regra de negócio. Comece pelo adicionar() do Carrinho (itens começa em 0).",
                "Escreva um test com a descrição 'adicionar soma um item' que cria carrinho (final) = Carrinho(), chama carrinho.adicionar() e confere com expect que carrinho.itens é 1.",
                FltAns("test(\"adicionar soma um item\", () {\n  final carrinho = Carrinho();\n  carrinho.adicionar();\n  expect(carrinho.itens, 1);\n});",
                       "test(\"adicionar soma um item\", () { final carrinho = Carrinho(); carrinho.adicionar(); expect(carrinho.itens, «1|equals(1)»); })"),
                A("Um teste tem três partes: preparar, agir e conferir. A conferência é expect(valorReal, valorEsperado).", "test('adicionar soma um item', () { final carrinho = Carrinho(); carrinho.adicionar(); expect(carrinho.itens, _); });"),
                A("test('...', () { ... })", "Registra um teste com descrição.", "final carrinho = Carrinho(); carrinho.adicionar();", "Prepara e executa a ação.", "expect(carrinho.itens, 1)", "Reprova o teste se o valor real não for o esperado."),
                "Teste de unidade é rápido e não precisa de tela: cubra as regras de negócio (descontos, totais) e rode flutter test a cada mudança."));

            list.Add(C(L, 2, "cmd", "Teste com cobertura",
                "O Bruno quer saber quanto do código os testes realmente exercitam antes de aprovar a versão.",
                "Rode todos os testes gerando o relatório de cobertura de código.",
                A("flutter test --coverage"),
                A("O flutter test tem uma opção que registra quais linhas foram executadas.", "flutter test --________"),
                A("--coverage", "Gera coverage/lcov.info com as linhas executadas pelos testes."),
                "Cobertura alta não garante bons testes, mas cobertura baixa mostra o que está sem rede de proteção. O lcov.info vira relatório HTML com o genhtml ou no CI."));

            list.Add(C(L, 2, "code", "Requisição com prazo",
                "Com o 4G fraco, a tela do cardápio fica carregando para sempre. O Bruno quer desistir depois de 10 segundos e mostrar o erro.",
                "Guarde em resposta (final) o resultado, com await, de http.get(Uri.parse(url)) seguido de .timeout com uma Duration de 10 segundos.",
                FltAns("final resposta = await http\n    .get(Uri.parse(url))\n    .timeout(const Duration(seconds: 10));",
                       "final resposta = await http.get(Uri.parse(url)).timeout(«|const »Duration(seconds: 10))"),
                A("Todo Future tem o método timeout(Duration): se demorar demais, ele lança TimeoutException.", "final resposta = await http.get(Uri.parse(url)).timeout(Duration(seconds: __));"),
                A(".timeout(Duration(seconds: 10))", "Se a resposta não chegar em 10 s, lança TimeoutException.", "await", "Espera a resposta OU o fim do prazo."),
                "Capture o TimeoutException (import 'dart:async') e ofereça 'Tentar de novo'. Sem prazo, uma rede ruim deixa a tela presa."));

            list.Add(C(L, 2, "code", "Token no cabeçalho",
                "A API de pedidos passou a exigir login. O Kenji pôs o token na URL (?token=...) e a segurança vetou: URLs ficam gravadas em logs. O lugar certo é o cabeçalho Authorization.",
                "Guarde em resposta (final), com await, um http.get de Uri.parse(url) com headers contendo 'Authorization' com o valor 'Bearer ' seguido de token (interpolação).",
                FltAns("final resposta = await http.get(\n  Uri.parse(url),\n  headers: {\"Authorization\": \"Bearer $token\"},\n);",
                       "final resposta = await http.get(Uri.parse(url), headers: {\"Authorization\": \"Bearer «$token|${token}»\"})"),
                A("http.get aceita o parâmetro nomeado headers: um Map do nome do cabeçalho para o valor.", "http.get(Uri.parse(url), headers: {'Authorization': 'Bearer $_____'})"),
                A("headers: {...}", "Cabeçalhos HTTP enviados junto com a requisição.", "'Authorization': 'Bearer $token'", "Padrão para tokens (JWT/OAuth): fora da URL, longe dos logs."),
                "Guarde o token com flutter_secure_storage (Keychain/Keystore), nunca em SharedPreferences nem no código-fonte."));

            list.Add(C(L, 2, "code", "Enviando o pedido",
                "Hora de mandar o pedido (um Map chamado pedido) para a API. Ela só aceita JSON e responde 400 se o Content-Type não for application/json.",
                "Guarde em resposta (final), com await, um http.post para Uri.parse(url) com headers {'Content-Type': 'application/json'} e body: jsonEncode(pedido).",
                FltAns("final resposta = await http.post(\n  Uri.parse(url),\n  headers: {\"Content-Type\": \"application/json\"},\n  body: jsonEncode(pedido),\n);",
                       FltSub("final resposta = await http.post(Uri.parse(url), «#H, #B|#B, #H»)",
                              "#H", "headers: {\"Content-Type\": \"application/json\"}",
                              "#B", "body: jsonEncode(pedido)")),
                A("jsonEncode (dart:convert) faz o caminho inverso do jsonDecode: Map → texto JSON.", "http.post(Uri.parse(url), headers: {'Content-Type': 'application/json'}, body: jsonEncode(______))"),
                A("http.post(...)", "Envia dados ao servidor.", "'Content-Type': 'application/json'", "Diz ao servidor o formato do corpo.", "body: jsonEncode(pedido)", "O Map convertido em texto JSON."),
                "Se passar o Map direto no body, o pacote http envia como formulário (x-www-form-urlencoded), não JSON. Depois, confira o statusCode (200/201)."));

            list.Add(C(L, 2, "code", "Lista de modelos",
                "A API devolve uma lista JSON de produtos e o Produto.fromJson já existe. O Bruno não quer mais List<dynamic> espalhado pelas telas.",
                "Crie produtos (final) convertendo jsonDecode(resposta.body) para List (as List), aplicando map com (e) => Produto.fromJson(e) e terminando com toList().",
                FltAns("final produtos = (jsonDecode(resposta.body) as List)\n    .map((e) => Produto.fromJson(e))\n    .toList();",
                       "final «List<Produto> |»produtos = (jsonDecode(resposta.body) as «List|List<dynamic>»).map«|<Produto>»((e) => Produto.fromJson(e«| as Map<String, dynamic>»)).toList()"),
                A("jsonDecode devolve dynamic: diga que é uma List com as e depois transforme cada item com map.", "(jsonDecode(resposta.body) as List).map((e) => Produto.fromJson(e)).toList()"),
                A("jsonDecode(resposta.body) as List", "Cast: o JSON que chegou é uma lista.", ".map((e) => Produto.fromJson(e))", "Cada Map vira um Produto.", ".toList()", "List<Produto> pronta para a tela."),
                "Converter na borda (logo depois da requisição) mantém o resto do app tipado: erro de campo aparece num lugar só."));

            list.Add(C(L, 2, "cmd", "Formatação no CI",
                "O pipeline de CI precisa reprovar o pull request quando houver arquivo fora do padrão de formatação — sem alterar nada no servidor.",
                "Rode o dart format na pasta atual (.) sem gravar alterações (--output=none) e terminando com código de erro se algum arquivo precisar mudar (--set-exit-if-changed).",
                ExtraFlutter_CmdPerm("dart format", new[] { ".", "«--output=none|--output none|-o none»", "--set-exit-if-changed" }),
                A("O dart format tem uma opção para não gravar os arquivos e outra para falhar quando houver diferença.", "dart format --output=none --set-exit-if-changed ."),
                A("--output=none", "Não grava nada: só verifica.", "--set-exit-if-changed", "Sai com código 1 se algum arquivo precisaria mudar: o CI reprova.", ".", "O projeto todo."),
                "No CI, junte: dart format (checagem), flutter analyze e flutter test. O padrão passa a ser garantido pela máquina, não pela revisão."));

            list.Add(C(L, 2, "code", "Cópia modificada",
                "O Produto é imutável (campos final, construtor Produto({required this.nome, required this.preco})). Para aplicar uma promoção, o Kenji tentou produto.preco = 7.9, que nem compila. O padrão é criar uma cópia com o que mudou.",
                "Escreva o método copyWith de Produto com os parâmetros nomeados opcionais String? nome e double? preco, retornando um novo Produto (nome:, preco:) que usa cada parâmetro ou, se ele for null, o valor atual (this).",
                FltAns("Produto copyWith({String? nome, double? preco}) {\n  return Produto(\n    nome: nome ?? this.nome,\n    preco: preco ?? this.preco,\n  );\n}",
                       FltSub("Produto copyWith({String? nome, double? preco}) «=> #R|{ return #R; }»",
                              "#R", "Produto(«nome: nome ?? this.nome, preco: preco ?? this.preco|preco: preco ?? this.preco, nome: nome ?? this.nome»)")),
                A("Parâmetros anuláveis significam 'não quero mudar'; o ?? escolhe entre o novo valor e o atual (this.campo).", "Produto copyWith({String? nome, double? preco}) => Produto(nome: nome ?? this.nome, preco: preco ?? this._____);"),
                A("{String? nome, double? preco}", "Opcionais: quem chama passa só o que muda.", "nome ?? this.nome", "Usa o valor novo ou mantém o atual.", "produto.copyWith(preco: 7.9)", "Uso: objeto novo; o original fica intacto."),
                "Objetos imutáveis evitam bugs do tipo 'quem mudou isso?' e combinam com o Flutter: estado novo = objeto novo. Pacotes como o freezed geram o copyWith."));

            list.Add(C(L, 2, "code", "Foto quebrada",
                "Quando a foto de um produto some do servidor, o Image.network mostra um erro feio na tela. A Paula quer um ícone de imagem quebrada no lugar.",
                "Crie um Image.network(fotoUrl) com errorBuilder: uma função (context, error, stackTrace) que retorna Icon(Icons.broken_image).",
                FltAns("Image.network(\n  fotoUrl,\n  errorBuilder: (context, error, stackTrace) => Icon(Icons.broken_image),\n)",
                       FltSub("Image.network(fotoUrl, errorBuilder: «(context, error, stackTrace)|(_, __, ___)» «=> #I|{ return #I; }»)",
                              "#I", "«|const »Icon(Icons.broken_image)")),
                A("O Image.network tem um parâmetro de função chamado quando o download falha; ele recebe 3 argumentos.", "Image.network(fotoUrl, errorBuilder: (context, error, stackTrace) => Icon(Icons.____________))"),
                A("errorBuilder", "Chamado se a imagem não carregar (404, sem internet...).", "(context, error, stackTrace)", "Recebe o erro, caso você queira registrá-lo.", "Icon(Icons.broken_image)", "Substituto amigável."),
                "Para o carregamento, use loadingBuilder; para cache em disco, o pacote cached_network_image."));

            list.Add(C(L, 2, "code", "Itens com identidade",
                "Na lista de pedidos, cada PedidoTile é um StatefulWidget (guarda se está expandido). Ao remover o primeiro pedido, o estado 'pula' para o vizinho! O Flutter está casando os widgets pela posição.",
                "Crie um PedidoTile com key: ValueKey(pedido.id) e pedido: pedido.",
                FltAns("PedidoTile(key: ValueKey(pedido.id), pedido: pedido)",
                       "PedidoTile(«key: ValueKey(pedido.id), pedido: pedido|pedido: pedido, key: ValueKey(pedido.id)»)"),
                A("Uma Key estável faz o Flutter reconhecer o mesmo item mesmo quando ele muda de posição. Use algo único do pedido.", "PedidoTile(key: ValueKey(pedido.__), pedido: pedido)"),
                A("key: ValueKey(pedido.id)", "Identidade do widget baseada no id, não na posição.", "pedido: pedido", "O dado exibido."),
                "Use keys em listas que reordenam, inserem ou removem itens com estado. ValueKey(index) não resolve: a posição é justamente o problema."));

            list.Add(C(L, 2, "cmd", "APK mais leve",
                "O APK universal ficou com 48 MB porque leva o código de todas as arquiteturas de processador. Para distribuir fora da Play Store, o Bruno quer um APK por arquitetura.",
                "Gere os APKs de release separados por ABI (arquitetura de processador).",
                A("flutter build apk --split-per-abi", "flutter build apk --release --split-per-abi"),
                A("O build apk tem uma opção para dividir o resultado por ABI.", "flutter build apk --split-___-___"),
                A("--split-per-abi", "Gera um APK para cada arquitetura (arm64-v8a, armeabi-v7a, x86_64), bem menores."),
                "Na Play Store isso não é necessário: o App Bundle (flutter build appbundle) já entrega a cada aparelho só o que ele precisa."));

            list.Add(C(L, 2, "code", "Teste de widget",
                "Depois de uma refatoração, a tela CardapioPage ficou sem o título e ninguém percebeu. O Bruno quer um teste de widget que pegue isso.",
                "Escreva um testWidgets com a descrição 'mostra o título' e função async (tester): faça await tester.pumpWidget com MaterialApp(home: CardapioPage()) e confira com expect que find.text('Cardápio') encontra exatamente um widget (findsOneWidget).",
                FltAns("testWidgets(\"mostra o título\", (tester) async {\n  await tester.pumpWidget(MaterialApp(home: CardapioPage()));\n  expect(find.text(\"Cardápio\"), findsOneWidget);\n});",
                       "testWidgets(\"mostra o título\", «(tester)|(WidgetTester tester)» async { await tester.pumpWidget(«|const »MaterialApp(home: «|const »CardapioPage())); expect(find.text(\"Cardápio\"), findsOneWidget); })"),
                A("pumpWidget desenha o widget num ambiente de teste; find procura na árvore; findsOneWidget é o resultado esperado.", "testWidgets('mostra o título', (tester) async { await tester.pumpWidget(MaterialApp(home: CardapioPage())); expect(find.text('Cardápio'), _____________); });"),
                A("testWidgets(..., (tester) async { ... })", "Teste com uma tela 'virtual' controlada pelo tester.", "tester.pumpWidget(MaterialApp(home: ...))", "Monta a tela; o MaterialApp fornece tema e Navigator.", "expect(find.text('Cardápio'), findsOneWidget)", "Exige exatamente um widget com esse texto."),
                "Depois de um toque (await tester.tap(find.byIcon(Icons.add))), chame await tester.pump() para redesenhar antes do próximo expect."));

            list.Add(C(L, 2, "cmd", "URL por ambiente",
                "O endereço da API está fixo no código e cada build de teste exige editar o arquivo. O Bruno quer informar o endereço na hora de rodar.",
                "Rode o app definindo a constante de compilação API_URL com o valor https://api.cafe365.com.br.",
                A("flutter run --dart-define=API_URL=https://api.cafe365.com.br", "flutter run --dart-define API_URL=https://api.cafe365.com.br"),
                A("O flutter run aceita constantes de compilação no formato CHAVE=valor.", "flutter run --dart-define=API_URL=..."),
                A("--dart-define=API_URL=...", "Cria uma constante que o código lê com String.fromEnvironment('API_URL').", "flutter run", "A mesma opção funciona no flutter build."),
                "Com muitos valores, use --dart-define-from-file=config/dev.json. Atenção: o valor vai dentro do app — não é lugar para senhas e chaves secretas."));

            list.Add(C(L, 2, "code", "Lendo o ambiente",
                "Agora o código precisa ler o API_URL passado no --dart-define. Se ninguém informar, o app deve usar o servidor de testes.",
                "Declare a constante (const) apiUrl lendo API_URL com String.fromEnvironment e defaultValue 'https://teste.cafe365.com.br'.",
                FltAns("const apiUrl = String.fromEnvironment(\n  \"API_URL\",\n  defaultValue: \"https://teste.cafe365.com.br\",\n);",
                       "const «String |»apiUrl = String.fromEnvironment(\"API_URL\", defaultValue: \"https://teste.cafe365.com.br\")"),
                A("String.fromEnvironment lê as constantes de compilação; ele só é garantido quando usado com const.", "const apiUrl = String.fromEnvironment('API_URL', defaultValue: '...');"),
                A("String.fromEnvironment('API_URL', ...)", "Lido na compilação, a partir do --dart-define.", "defaultValue:", "Valor usado quando a chave não foi definida.", "const", "Obrigatório na prática: sem const, em builds compilados o valor pode vir vazio."),
                "Deixe como padrão um ambiente seguro (testes), nunca o de produção: assim um build sem configuração não mexe em dados reais."));

            list.Add(C(L, 2, "code", "Buscas em paralelo",
                "A tela inicial faz await buscarCardapio() e depois await buscarPromocoes(): 2 s + 2 s = 4 s de espera. As duas buscas são independentes.",
                "Guarde em resultados (final), com await, o Future.wait de uma lista com buscarCardapio() e buscarPromocoes(), nessa ordem.",
                FltAns("final resultados = await Future.wait([\n  buscarCardapio(),\n  buscarPromocoes(),\n]);",
                       "final resultados = await Future.wait([buscarCardapio(), buscarPromocoes()])"),
                A("Dispare os dois Futures juntos e espere todos de uma vez.", "final resultados = await Future.wait([buscarCardapio(), ______________()]);"),
                A("Future.wait([...])", "Começa os dois ao mesmo tempo e termina quando ambos terminam (≈ 2 s, não 4 s).", "resultados[0] / resultados[1]", "Os valores, na mesma ordem da lista."),
                "Se qualquer um falhar, o Future.wait falha. No Dart 3 também dá para usar records: final (cardapio, promos) = await (buscarCardapio(), buscarPromocoes()).wait;"));

            list.Add(C(L, 2, "code", "Status com switch",
                "O pedido tem enum Status { recebido, preparando, pronto }. Cada status precisa de uma cor, e o Bruno quer que o analisador reclame se alguém criar um status novo e esquecer dele.",
                "Declare cor (final) com uma expressão switch sobre status: Status.recebido => Colors.grey, Status.preparando => Colors.orange e Status.pronto => Colors.green (sem caso padrão _).",
                FltAns("final cor = switch (status) {\n  Status.recebido => Colors.grey,\n  Status.preparando => Colors.orange,\n  Status.pronto => Colors.green,\n};",
                       "final «Color |»cor = switch (status) { " + P(new[] { "Status.recebido => Colors.grey", "Status.preparando => Colors.orange", "Status.pronto => Colors.green" }) + "«|,» }"),
                A("O Dart 3 tem switch como expressão: switch (valor) { caso => resultado, ... }. Sem caso padrão, o analisador exige todos os valores do enum.", "final cor = switch (status) { Status.recebido => Colors.grey, Status.preparando => ..., Status.pronto => ... };"),
                A("switch (status) { ... }", "Expressão: devolve o valor direto para cor.", "Status.pronto => Colors.green", "Cada caso: padrão => resultado.", "sem _ =>", "Exaustivo: um status novo no enum vira erro de compilação até ser tratado."),
                "Evite o caso padrão (_) em switch sobre enums: ele esconde casos novos esquecidos, e o problema só aparece em produção."));

            list.Add(C(L, 2, "code", "Campo obrigatório",
                "O formulário de cadastro aceita nome vazio e a API devolve erro 400. A validação deve acontecer antes, na própria tela.",
                "Crie um TextFormField com validator: uma função (valor) que retorna 'Informe o nome' quando valor for null ou vazio (isEmpty), e null caso contrário.",
                FltAns("TextFormField(\n  validator: (valor) {\n    if (valor == null || valor.isEmpty) return \"Informe o nome\";\n    return null;\n  },\n)",
                       "TextFormField(validator: (valor) { if (valor == null || valor.isEmpty) «return \"Informe o nome\";|{ return \"Informe o nome\"; }» return null; })",
                       "TextFormField(validator: (valor) => valor == null || valor.isEmpty ? \"Informe o nome\" : null)",
                       "TextFormField(validator: (valor) => (valor == null || valor.isEmpty) ? \"Informe o nome\" : null)",
                       "TextFormField(validator: (valor) => «valor?.isEmpty ?? true|(valor?.isEmpty ?? true)» ? \"Informe o nome\" : null)"),
                A("O validator recebe o texto (String?) e devolve a mensagem de erro, ou null quando está tudo certo.", "validator: (valor) => valor == null || valor.isEmpty ? 'Informe o nome' : null"),
                A("validator: (valor) => ...", "Chamado quando o Form é validado.", "valor == null || valor.isEmpty", "Campo ausente ou vazio.", "? 'Informe o nome' : null", "Texto = erro exibido embaixo do campo; null = válido."),
                "O TextFormField fica dentro de um Form com GlobalKey<FormState>; no botão, chame formKey.currentState!.validate() antes de enviar. E valide de novo no servidor."));

            list.Add(C(L, 2, "code", "Grade responsiva",
                "No tablet, a grade do cardápio fica com 2 colunas gigantes. A Paula quer 3 colunas quando a tela tiver mais de 600 de largura.",
                "Crie um GridView.count com crossAxisCount igual a 3 se MediaQuery.sizeOf(context).width for maior que 600, senão 2 (operador ternário), e children: cartoes.",
                FltAns("GridView.count(\n  crossAxisCount: MediaQuery.sizeOf(context).width > 600 ? 3 : 2,\n  children: cartoes,\n)",
                       FltSub("GridView.count(«#C, children: cartoes|children: cartoes, #C»)",
                              "#C", "crossAxisCount: «MediaQuery.sizeOf(context).width|MediaQuery.of(context).size.width» > 600 ? 3 : 2")),
                A("O MediaQuery informa o tamanho da tela; o ternário escolhe entre dois valores: condição ? a : b.", "GridView.count(crossAxisCount: MediaQuery.sizeOf(context).width > 600 ? _ : _, children: cartoes)"),
                A("MediaQuery.sizeOf(context).width", "Largura da tela em pixels lógicos (redesenha se mudar, como ao girar o tablet).", "> 600 ? 3 : 2", "Ternário: 3 colunas em telas largas, 2 nas estreitas.", "GridView.count", "Grade com número fixo de colunas."),
                "MediaQuery.sizeOf é mais eficiente que MediaQuery.of(context).size: só redesenha quando o tamanho muda. Para o espaço do widget pai (não da tela), use LayoutBuilder."));

            list.Add(C(L, 2, "code", "Fila ao vivo",
                "O painel da cozinha mostra quantos pedidos estão na fila. O número chega pelo Stream<int> filaStream e precisa se atualizar sozinho na tela.",
                "Monte um StreamBuilder<int> com stream: filaStream e builder (context, snapshot) que: se o snapshot não tem dados (!snapshot.hasData), retorna CircularProgressIndicator(); senão, retorna um Text com 'Na fila: ' seguido de snapshot.data (interpolação ${...}).",
                FltAns("StreamBuilder<int>(\n  stream: filaStream,\n  builder: (context, snapshot) {\n    if (!snapshot.hasData) return CircularProgressIndicator();\n    return Text(\"Na fila: ${snapshot.data}\");\n  },\n)",
                       FltSub("StreamBuilder«<int>|»(«stream: filaStream, builder: #B|builder: #B, stream: filaStream»)",
                              "#B", "(context, snapshot) «{ if (!snapshot.hasData) «return #C;|{ return #C; }» «|else »«return #T;|{ return #T; }» }|=> snapshot.hasData ? #T : #C|=> !snapshot.hasData ? #C : #T»",
                              "#C", "«|const »CircularProgressIndicator()",
                              "#T", "Text(\"Na fila: «${snapshot.data}|${snapshot.data!}»\")")),
                A("O StreamBuilder funciona como o FutureBuilder, mas redesenha a cada valor novo do Stream.", "StreamBuilder<int>(stream: filaStream, builder: (context, snapshot) { if (!snapshot.hasData) return CircularProgressIndicator(); return Text('Na fila: ${snapshot.data}'); })"),
                A("stream: filaStream", "O StreamBuilder assina o Stream e cancela sozinho quando sai da tela.", "!snapshot.hasData", "Nenhum valor chegou ainda: mostra o carregando.", "'Na fila: ${snapshot.data}'", "Com chaves! $snapshot.data interpolaria só o snapshot."),
                "Trate também snapshot.hasError. E não crie o Stream dentro do build (stream: abrirFila()): cada rebuild abriria uma assinatura nova."));
        }

        // comandos com partes em qualquer ordem (cada parte pode ter «alternativas»)
        static string[] ExtraFlutter_CmdPerm(string cabeca, string[] partes)
        {
            var perm = ExtraFlutter_Perm(partes).Replace(", ", " ");
            return ExtraFlutter_Cmd(cabeca + " " + perm);
        }
    }

    static partial class Docs
    {
        static partial void DocsExtraFlutter(List<DocEntry> all)
        {
            const string L = "Flutter";
            // ───── CLI
            all.Add(X(L, "CLI do Flutter", "flutter doctor / devices / clean", "Diagnóstico do ambiente, lista de aparelhos e limpeza dos arquivos gerados.",
                "flutter doctor [-v]\nflutter devices\nflutter clean\nflutter --version",
                A("doctor", "confere a instalação ([✓] ok, [!] aviso, [✗] problema)", "devices", "lista celulares, emuladores e navegadores (com o id de cada um)", "clean", "apaga build/ e .dart_tool/ (depois rode flutter pub get)"),
                A("flutter doctor -v", "diagnóstico detalhado", "flutter doctor --android-licenses", "aceita as licenças do Android SDK"),
                "Erros estranhos de compilação depois de trocar de branch? flutter clean e flutter pub get."));
            all.Add(X(L, "CLI do Flutter", "flutter run: dispositivo e modo", "Escolhe onde rodar (-d) e o modo de compilação (debug, profile ou release).",
                "flutter run [-d ID] [--release | --profile] [--dart-define=CHAVE=valor]",
                A("-d ID / --device-id ID", "dispositivo (ids em flutter devices); all = todos", "--release", "otimizado, como o cliente recebe (sem hot reload)", "--profile", "otimizado + rastreamento para o DevTools (só em aparelho físico)",
                  "--dart-define=CHAVE=valor", "constante de compilação, lida com String.fromEnvironment"),
                A("flutter run -d windows", "roda como app desktop", "flutter run --release -d R58N123", "release num celular específico", "flutter run --dart-define=ESCOLA=Centro", "define ESCOLA"),
                "Debug é lento por natureza: meça desempenho com --profile."));
            all.Add(X(L, "CLI do Flutter", "flutter create --org", "Cria o projeto com o identificador da empresa (domínio invertido) em vez de com.example.",
                "flutter create --org com.EMPRESA NOME_DO_PROJETO",
                A("--org", "prefixo do pacote Android / bundle iOS", "NOME", "minúsculas e _"),
                A("flutter create --org br.escolaxyz agenda_app", "gera br.escolaxyz.agenda_app"),
                "O identificador não pode mudar depois que o app foi publicado."));
            all.Add(X(L, "CLI do Flutter", "flutter test (arquivo e cobertura)", "Roda os testes da pasta test/ — todos, um arquivo ou com relatório de cobertura.",
                "flutter test\nflutter test test/ARQUIVO_test.dart\nflutter test --coverage",
                A("(sem argumentos)", "todos os *_test.dart", "caminho", "só aquele arquivo", "--coverage", "gera coverage/lcov.info", "--plain-name 'texto'", "só os testes com essa descrição"),
                A("flutter test test/boletim_test.dart", "só os testes do boletim", "flutter test --coverage", "todos, medindo a cobertura"), null));
            all.Add(X(L, "CLI do Flutter", "flutter build apk --split-per-abi", "Gera um APK por arquitetura de processador, cada um bem menor que o universal.",
                "flutter build apk [--release] --split-per-abi",
                A("--split-per-abi", "um APK para arm64-v8a, armeabi-v7a e x86_64"),
                A("flutter build apk --split-per-abi", "APKs separados em build/app/outputs/flutter-apk/"),
                "Para a Play Store use flutter build appbundle: a loja já entrega só o necessário."));
            all.Add(X(L, "CLI do Flutter", "dart format / analyze / fix", "Ferramentas de qualidade: formatação oficial, análise estática e correções automáticas.",
                "dart format PASTA\nflutter analyze   (ou dart analyze)\ndart fix --apply",
                A("dart format .", "formata todos os .dart da pasta", "--output=none (-o none)", "não grava, só verifica", "--set-exit-if-changed", "código de saída 1 se algo precisar mudar (ideal para CI)",
                  "flutter analyze", "erros, avisos e lints", "dart fix --dry-run / --apply", "mostra / aplica as correções automáticas"),
                A("dart format lib", "formata só a pasta lib", "dart format -o none --set-exit-if-changed lib", "checagem de CI só na lib"),
                "No CI: checagem de formato + flutter analyze + flutter test."));
            all.Add(X(L, "CLI do Flutter", "flutter pub add (dev)", "Adiciona pacotes; com dev: (ou --dev) o pacote vai para dev_dependencies.",
                "flutter pub add PACOTE\nflutter pub add dev:PACOTE\nflutter pub remove PACOTE",
                A("dev:PACOTE / --dev", "dependência só de desenvolvimento (testes, lints, geradores)"),
                A("flutter pub add dev:build_runner", "gerador de código só para desenvolvimento", "flutter pub add dev:flutter_lints", "regras de lint"),
                "dev_dependencies não entram no app publicado."));

            // ───── Dart
            all.Add(X(L, "Dart", "Tipos básicos e var", "var deduz o tipo pelo valor; também dá para escrever o tipo explícito.",
                "var nome = valor;\nint / double / bool / String / num nome = valor;",
                A("var", "tipo deduzido (e fixo depois)", "int", "inteiro", "double", "decimal", "bool", "true ou false", "String", "texto", "num", "int ou double"),
                A("var faltas = 3;", "int deduzido", "bool matriculado = false;", "tipo explícito", "double media = 7.5;", "decimal"), null));
            all.Add(X(L, "Dart", "Interpolação de strings ($ e ${})", "Coloca valores dentro de textos: $variavel ou ${expressão}. Para um $ literal, use \\$.",
                "'Texto $variavel'\n'Texto ${expressao}'\n'Preço: R\\$ 10'",
                A("$nome", "valor de uma variável", "${...}", "qualquer expressão (método, conta, propriedade)", "\\$", "cifrão literal", "x.toStringAsFixed(2)", "double com 2 casas, como texto"),
                A("print('Oi, $aluno!')", "variável", "'Média: ${media.toStringAsFixed(1)}'", "expressão", "'Livros: ${livros.length}'", "propriedade de um objeto"),
                "Cuidado: '$obj.campo' interpola só obj; use '${obj.campo}'."));
            all.Add(X(L, "Dart", "if / for-in", "Condição e repetição sobre listas.",
                "if (condicao) { ... } else { ... }\nfor (final item in lista) { ... }",
                A("==  !=  <  >  <=  >=", "comparações", "&&  ||  !", "e, ou, não", "for (final x in lista)", "percorre item a item", "for (var i = 0; i < n; i++)", "com índice"),
                A("if (nota >= 7) print('Aprovado');", "condição simples", "for (final aluno in turma) { print(aluno); }", "percorre a turma"),
                "Em Dart, a condição do if precisa ser bool."));
            all.Add(X(L, "Dart", "Set e literais de coleção", "Set guarda itens sem repetição. List usa [ ], Map e Set usam { }.",
                "final s = {'a', 'b'};\nfinal vazio = <String>{};",
                A("{a, b}", "Set (só valores)", "{k: v}", "Map (pares chave: valor)", "<Tipo>{}", "Set vazio ({} sozinho é Map)", "s.add(x) / s.contains(x)", "acrescenta (sem duplicar) / testa"),
                A("final materias = {'Química', 'Biologia'};", "Set<String>", "final notas = <String, double>{'Ana': 8.5};", "Map tipado"), null));
            all.Add(X(L, "Dart", "Funções com parâmetros posicionais", "Tipo de retorno, nome e parâmetros entre parênteses; => para corpo de uma expressão.",
                "TIPO nome(TIPO a, TIPO b) { return ...; }\nTIPO nome(TIPO a, TIPO b) => expressão;\nvoid main() { runApp(MeuApp()); }",
                A("void", "não retorna valor", "=>", "atalho para { return ...; }", "main", "ponto de entrada do programa; runApp liga o widget raiz"),
                A("double media(double a, double b) => (a + b) / 2;", "duas notas", "void main() => runApp(const AgendaApp());", "main de um app"), null));
            all.Add(X(L, "Dart", "Classes, construtores e herança", "Campos, construtor com this., parâmetros nomeados com padrão, extends e @override.",
                "class Nome {\n  final TIPO campo;\n  Nome(this.campo);\n}\nclass Filha extends Mae {\n  @override\n  TIPO metodo() => ...;\n}",
                A("Nome(this.a, this.b)", "construtor posicional que já guarda nos campos", "Nome({required this.a, this.b = 0})", "nomeados: obrigatório e com valor padrão", "extends", "herança", "@override", "substitui um método da classe-mãe", "super.metodo()", "chama a versão da mãe"),
                A("class Aluno { final String nome; final int serie; Aluno(this.nome, this.serie); }", "classe simples", "Livro({required this.titulo, this.paginas = 100});", "nomeados", "class Romance extends Livro { @override String tipo() => 'Romance'; }", "herança"),
                "Todos os campos final? Marque o construtor como const."));
            all.Add(X(L, "Dart", "copyWith (objetos imutáveis)", "Cria uma cópia do objeto mudando só alguns campos.",
                "Classe copyWith({T1? a, T2? b}) => Classe(a: a ?? this.a, b: b ?? this.b);",
                A("T?", "parâmetro opcional: null = não mudar", "a ?? this.a", "usa o novo ou mantém o atual"),
                A("Aluno copyWith({String? nome, int? serie}) => Aluno(nome: nome ?? this.nome, serie: serie ?? this.serie);", "cópia de Aluno", "aluno.copyWith(serie: 3)", "uso"),
                "O pacote freezed gera copyWith, == e toString."));
            all.Add(X(L, "Dart", "map / where / fold", "Transformar, filtrar e acumular coleções.",
                "lista.map((x) => ...).toList()\nlista.where((x) => condição).toList()\nlista.fold(inicial, (acc, x) => ...)",
                A("map", "transforma cada item", "where", "mantém os que passam no teste", "fold", "acumula um resultado (soma, total...)", "toList()", "converte o Iterable em List", "any / firstWhere", "algum passa? / primeiro que passa"),
                A("final titulos = livros.map((l) => l.titulo).toList();", "só os títulos", "final aprovados = alunos.where((a) => a.media >= 7).toList();", "filtro", "double get soma => notas.fold(0, (s, n) => s + n);", "soma das notas"),
                "Sem toList(), map/where são preguiçosos e recalculam a cada leitura."));
            all.Add(X(L, "Dart", "switch como expressão (Dart 3) e enum", "Devolve um valor conforme o padrão; com enums, sem caso padrão, precisa cobrir todos os valores.",
                "enum Cor { azul, verde }\nfinal x = switch (valor) {\n  Cor.azul => ...,\n  Cor.verde => ...,\n};",
                A("=>", "padrão => resultado", "_ => ...", "caso padrão (esconde valores esquecidos do enum)", "exaustividade", "o analisador acusa enum sem tratamento"),
                A("final turno = switch (periodo) { Periodo.manha => 'M', Periodo.tarde => 'T', Periodo.noite => 'N' };", "sigla do turno"), null));
            all.Add(X(L, "Dart", "try / catch / finally", "Captura exceções para o app não fechar.",
                "try {\n  // código arriscado\n} catch (e) {\n  // tratamento\n} finally {\n  // sempre roda\n}",
                A("catch (e)", "e = erro capturado", "on TipoDeErro catch (e)", "captura só aquele tipo", "finally", "roda dando certo ou errado", "throw Exception('...')", "lança um erro"),
                A("try { await salvarNota(); } catch (e) { print('Falhou: $e'); }", "registra a falha"), null));
            all.Add(X(L, "Dart", "Future.delayed / Future.wait / timeout", "Utilitários de Future: esperar um tempo, rodar em paralelo e limitar a espera.",
                "await Future.delayed(Duration(seconds: N));\nfinal r = await Future.wait([f1(), f2()]);\nawait algo().timeout(Duration(seconds: N));",
                A("Duration(seconds: / milliseconds: / minutes:)", "intervalo de tempo", "Future.wait([...])", "paralelo; resultados na mesma ordem", ".timeout(...)", "lança TimeoutException se passar do prazo"),
                A("await Future.delayed(const Duration(milliseconds: 500));", "meio segundo", "final r = await Future.wait([buscarAlunos(), buscarTurmas()]);", "duas buscas juntas", "await http.get(uri).timeout(const Duration(seconds: 5));", "no máximo 5 s"), null));
            all.Add(X(L, "Dart", "Stream e listen", "Stream entrega vários valores ao longo do tempo (um Future entrega um só).",
                "final assinatura = meuStream.listen((valor) { ... });\nassinatura.cancel();",
                A("listen((v) { ... })", "roda para cada valor novo", "StreamSubscription", "o retorno do listen; cancele no dispose", "await for (final v in stream)", "percorre dentro de uma função async"),
                A("avisosStream.listen((aviso) => print(aviso));", "imprime cada aviso"), "Na tela, use StreamBuilder."));
            all.Add(X(L, "Dart", "http: headers, post e jsonEncode", "Cabeçalhos (como o token de login) e envio de JSON com o pacote http.",
                "http.get(uri, headers: {'Authorization': 'Bearer $token'})\nhttp.post(uri, headers: {'Content-Type': 'application/json'}, body: jsonEncode(mapa))",
                A("headers:", "Map de cabeçalhos", "Authorization: Bearer ...", "token no cabeçalho, nunca na URL", "jsonEncode(x)", "Map/List → texto JSON (dart:convert)", "(jsonDecode(body) as List).map(...).toList()", "lista JSON → lista de modelos"),
                A("await http.post(Uri.parse(enderecoNotas), headers: {'Content-Type': 'application/json'}, body: jsonEncode(nota))", "envia uma nota", "(jsonDecode(r.body) as List).map((e) => Aluno.fromJson(e)).toList()", "lista de Aluno"),
                "Map direto no body vira formulário (x-www-form-urlencoded), não JSON."));
            all.Add(X(L, "Dart", "String.fromEnvironment (--dart-define)", "Lê, na compilação, as constantes passadas com --dart-define.",
                "const valor = String.fromEnvironment('CHAVE', defaultValue: 'padrão');",
                A("const", "obrigatório na prática (sem const o valor pode vir vazio em builds compilados)", "defaultValue", "usado se a chave não foi definida", "bool.fromEnvironment / int.fromEnvironment", "outros tipos"),
                A("const escola = String.fromEnvironment('ESCOLA', defaultValue: 'Matriz');", "lê ESCOLA"),
                "Não guarde segredos assim: o valor fica dentro do app."));

            // ───── Widgets
            all.Add(X(L, "Widgets", "MaterialApp / Scaffold / AppBar", "A estrutura de um app Material: a raiz (MaterialApp), cada tela (Scaffold) e a barra do topo (AppBar).",
                "MaterialApp(home: Tela(), debugShowCheckedModeBanner: false)\nScaffold(appBar: AppBar(title: Text('...')), body: conteudo)",
                A("home", "primeira tela", "debugShowCheckedModeBanner", "false esconde a faixa DEBUG", "theme", "ThemeData(...)", "appBar / body", "barra e conteúdo do Scaffold", "floatingActionButton", "botão flutuante"),
                A("MaterialApp(home: const AgendaPage())", "app com uma tela", "Scaffold(appBar: AppBar(title: const Text('Agenda')), body: lista)", "tela com barra"), null));
            all.Add(X(L, "Widgets", "Center / SizedBox / Container", "Layout básico: centralizar, espaçar e desenhar caixas.",
                "Center(child: w)\nSizedBox(height: N)   SizedBox(width: N)\nContainer(width: N, height: N, color: Colors.COR, child: w)",
                A("Center", "centraliza o filho", "SizedBox", "espaço fixo (ou caixa de tamanho fixo)", "Container", "caixa com tamanho, cor, margem, padding e decoration", "decoration: BoxDecoration(...)", "bordas e cantos (aí a cor vai na decoration)"),
                A("Center(child: Text('Olá'))", "texto no meio", "const SizedBox(width: 8)", "espaço horizontal", "Container(width: 50, height: 50, color: Colors.teal)", "quadrado verde-azulado"), null));
            all.Add(X(L, "Widgets", "Icon / IconButton", "Ícones do Material e botões só com ícone.",
                "Icon(Icons.NOME, size: N, color: COR)\nIconButton(icon: Icon(Icons.NOME), onPressed: () { ... })",
                A("Icons.xxx", "milhares de ícones prontos (home, add, delete, coffee...)", "onPressed", "função chamada no toque"),
                A("Icon(Icons.school)", "ícone de escola", "IconButton(icon: const Icon(Icons.delete), onPressed: () => apagar(nota))", "botão de apagar"), null));
            all.Add(X(L, "Widgets", "Image.asset / Image.network", "Imagens empacotadas no app (asset) ou baixadas da internet (network).",
                "Image.asset('assets/ARQUIVO.png')\nImage.network(url, width: N, height: N, fit: BoxFit.cover, errorBuilder: (context, error, stackTrace) => ...)",
                A("assets", "declare a pasta no pubspec.yaml (flutter: assets:)", "fit: BoxFit.cover / contain", "preenche cortando / cabe inteira", "errorBuilder", "widget mostrado se a imagem falhar", "loadingBuilder", "widget durante o download"),
                A("Image.asset('assets/brasao.png')", "imagem local", "Image.network(fotoAluno, errorBuilder: (c, e, s) => const Icon(Icons.person))", "foto com substituto"), null));
            all.Add(X(L, "Widgets", "Row: mainAxisAlignment", "Distribuição dos filhos no eixo principal (horizontal na Row, vertical na Column).",
                "Row(mainAxisAlignment: MainAxisAlignment.VALOR, children: [...])",
                A("start / center / end", "agrupados no início, meio ou fim", "spaceBetween", "pontas coladas, espaço igual entre", "spaceAround / spaceEvenly", "espaço também nas pontas"),
                A("Row(mainAxisAlignment: MainAxisAlignment.center, children: [a, b])", "no meio"), null));
            all.Add(X(L, "Widgets", "Card e ListTile", "Cartão com sombra e a linha padrão de listas.",
                "Card(child: ListTile(leading: w, title: Text(...), subtitle: Text(...), trailing: w, onTap: () {...}))",
                A("title / subtitle", "texto principal / secundário", "leading / trailing", "widget à esquerda / à direita", "onTap", "toque na linha"),
                A("Card(child: ListTile(title: Text(aluno), subtitle: Text(turma)))", "cartão de aluno"), null));
            all.Add(X(L, "Widgets", "StatelessWidget", "Widget próprio sem estado: só o método build.",
                "class Nome extends StatelessWidget {\n  const Nome({super.key});\n  @override\n  Widget build(BuildContext context) {\n    return ...;\n  }\n}",
                A("extends StatelessWidget", "não muda sozinho", "build(BuildContext context)", "devolve a árvore de widgets", "StatefulWidget", "quando precisa de setState"),
                A("class Rodape extends StatelessWidget { @override Widget build(BuildContext context) => const Text('Escola XYZ'); }", "rodapé fixo"), null));
            all.Add(X(L, "Widgets", "Keys (ValueKey)", "Dão identidade a um widget para o Flutter não confundir itens quando a lista muda.",
                "Widget(key: ValueKey(objeto.id), ...)",
                A("ValueKey(valor)", "identidade por um valor único e estável", "ValueKey(index)", "não resolve: a posição muda"),
                A("AlunoTile(key: ValueKey(aluno.matricula), aluno: aluno)", "item identificado pela matrícula"),
                "Use em listas com itens Stateful que são removidos, inseridos ou reordenados."));
            all.Add(X(L, "Widgets", "MediaQuery e GridView.count", "Tamanho da tela e grade com número fixo de colunas.",
                "MediaQuery.sizeOf(context).width\nGridView.count(crossAxisCount: N, children: [...])\ncondicao ? a : b",
                A("MediaQuery.sizeOf(context)", "tamanho da tela (Size)", "crossAxisCount", "número de colunas", "condição ? a : b", "ternário"),
                A("GridView.count(crossAxisCount: 4, children: fotos)", "4 colunas", "final largo = MediaQuery.sizeOf(context).width > 900;", "tela larga?"),
                "Para o espaço do widget pai (e não da tela), use LayoutBuilder."));
            all.Add(X(L, "Widgets", "StreamBuilder", "Redesenha a cada valor novo de um Stream.",
                "StreamBuilder<TIPO>(stream: meuStream, builder: (context, snapshot) { ... })",
                A("snapshot.hasData / snapshot.data", "último valor recebido", "snapshot.hasError", "o Stream emitiu erro", "stream", "crie fora do build"),
                A("StreamBuilder<String>(stream: avisos, builder: (context, s) => Text(s.data ?? 'Sem avisos'))", "último aviso"), null));
            all.Add(X(L, "Widgets", "TextFormField e validator", "Campo de formulário com validação.",
                "TextFormField(validator: (valor) => CONDIÇÃO_DE_ERRO ? 'mensagem' : null)",
                A("validator", "devolve o texto do erro ou null (válido)", "Form + GlobalKey<FormState>", "agrupa os campos", "formKey.currentState!.validate()", "roda todos os validators"),
                A("TextFormField(validator: (v) => v == null || v.length < 11 ? 'CPF incompleto' : null)", "mínimo de 11 caracteres"),
                "Valide também no servidor."));

            // ───── Provider
            all.Add(X(L, "Provider", "ChangeNotifier e ChangeNotifierProvider", "Estado compartilhado (pacote provider): a classe avisa mudanças e o provider a coloca acima das telas.",
                "class Estado extends ChangeNotifier {\n  void mudar() { ...; notifyListeners(); }\n}\nrunApp(ChangeNotifierProvider(create: (context) => Estado(), child: MeuApp()));",
                A("notifyListeners()", "avisa os widgets para redesenharem", "create:", "cria o objeto uma vez", "child:", "quem pode ler o estado", "MultiProvider", "vários providers juntos"),
                A("class Placar extends ChangeNotifier { int gols = 0; void marcar() { gols++; notifyListeners(); } }", "estado do placar", "runApp(ChangeNotifierProvider(create: (_) => Placar(), child: const JogoApp()))", "placar acima do app"), null));
            all.Add(X(L, "Provider", "context.watch / context.read", "Lendo o estado: watch assina (no build); read só pega o objeto (em callbacks).",
                "context.watch<Tipo>().campo     // no build\ncontext.read<Tipo>().metodo()   // em onPressed, onTap...",
                A("watch", "redesenha quando o estado avisa", "read", "não assina (use em eventos)", "select((Tipo t) => t.campo)", "redesenha só quando aquele campo muda", "Provider.of<T>(context, listen: false)", "o mesmo que read"),
                A("Text('Gols: ${context.watch<Placar>().gols}')", "mostra e atualiza", "onPressed: () => context.read<Placar>().marcar()", "muda no toque"),
                "watch dentro de onPressed dá erro."));

            // ───── Testes
            all.Add(X(L, "Testes", "test / testWidgets / expect", "Testes automatizados (flutter_test): de unidade (lógica) e de widget (tela).",
                "test('descrição', () { ... expect(real, esperado); });\ntestWidgets('descrição', (tester) async {\n  await tester.pumpWidget(MaterialApp(home: Tela()));\n  expect(find.text('...'), findsOneWidget);\n});",
                A("expect(real, esperado)", "reprova se forem diferentes", "tester.pumpWidget(w)", "monta o widget", "tester.tap(finder) + tester.pump()", "toca e redesenha", "find.text / find.byIcon / find.byType", "procura na árvore", "findsOneWidget / findsNothing / findsNWidgets(n)", "quantos devem existir"),
                A("test('media de 6 e 8 é 7', () { expect(media(6, 8), 7); });", "teste de unidade", "expect(find.text('Agenda'), findsOneWidget);", "confere um texto na tela"), null));
        }
    }
}
