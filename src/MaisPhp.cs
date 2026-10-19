using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerQuest
{
    static partial class Challenges
    {
        // Gera variações de resposta: troca @0, @1, ... por cada combinação das opções de cada "slot".
        static string[] PhpMix(string template, params string[][] slots)
        {
            var res = new List<string> { template };
            for (int i = 0; i < slots.Length; i++)
            {
                var next = new List<string>();
                foreach (var r in res)
                    foreach (var opt in slots[i])
                        next.Add(r.Replace("@" + i, opt));
                res = next;
            }
            return res.Distinct().ToArray();
        }

        static void PhpTier(List<Level> list, string title, int tier)
        {
            var lv = list.First(l => l.Cat == "php" && l.Title == title);
            lv.Tier = tier;
            lv.Chapter = Cat("php").Name + " · " + TierNames[tier];
        }

        static partial void MaisPhp(List<Level> list)
        {
            const string H = "php";

            // ── 1) rebalanceamento dos desafios existentes
            // uma função só, sem combinar conceitos → Iniciante
            PhpTier(list, "Dependências", 0);
            PhpTier(list, "Dados do formulário", 0);
            PhpTier(list, "Resposta em JSON", 0);
            // uma linha, com o nome da função no próprio objetivo → Intermediário
            PhpTier(list, "Conectando no banco", 1);
            PhpTier(list, "Consulta preparada", 1);
            PhpTier(list, "Executando", 1);
            PhpTier(list, "Senha segura", 1);
            PhpTier(list, "Migrations", 1);

            // ── 2) novos — INICIANTE
            list.Add(C(H, 0, "code", "Saudação com nome",
                "O Igor quer que a api-pedidos cumprimente o cliente pelo nome, que já está na variável $nome.",
                "Mostre com echo o texto Olá, seguido do valor de $nome e de um ponto de exclamação (para Ana: Olá, Ana!). Junte os pedaços com o operador de concatenação do PHP.",
                A("echo \"Olá, \" . $nome . \"!\";",
                  "echo \"Olá, $nome!\";",
                  "echo \"Olá, {$nome}!\";",
                  "echo \"Olá, \", $nome, \"!\";",
                  "print \"Olá, \" . $nome . \"!\";"),
                A("Em PHP, textos são unidos com o ponto (.), não com +.", "echo \"Olá, \" . $nome . ...;"),
                A("\"Olá, \"", "Texto fixo (string).", ".", "Operador de concatenação: junta strings.", "$nome", "Valor da variável no meio da frase."),
                "Dentro de aspas DUPLAS o PHP também troca $nome pelo valor (interpolação); em aspas simples, não."));
            list.Add(C(H, 0, "code", "Tamanho padrão",
                "O app manda ?tamanho=G na URL, mas às vezes não manda nada — e aí aparece o aviso \"Undefined array key\". A Fê quer um valor padrão.",
                "Guarde em $tamanho o parâmetro tamanho da URL (GET) ou, se ele não existir, o texto \"M\". Use o operador de coalescência nula.",
                A("$tamanho = $_GET[\"tamanho\"] ?? \"M\";",
                  "$tamanho = isset($_GET[\"tamanho\"]) ? $_GET[\"tamanho\"] : \"M\";"),
                A("Parâmetros da URL ficam em $_GET. O operador que dá um valor padrão quando algo não existe é ??.", "$tamanho = $_GET[\"...\"] ?? \"...\";"),
                A("$_GET[\"tamanho\"]", "Valor vindo da URL (?tamanho=G).", "??", "Se o da esquerda não existir (ou for null), usa o da direita.", "\"M\"", "Valor padrão."),
                "?? não gera aviso para chave inexistente. Também existe ??= : $config[\"tema\"] ??= \"claro\";"));

            // ── novos — INTERMEDIÁRIO
            list.Add(C(H, 1, "code", "Frete com ternário",
                "Promoção do Café 365: pedidos a partir de R$ 50 têm frete grátis; os outros pagam R$ 7,50. A api-pedidos usa tipos declarados.",
                "Crie a função frete com o parâmetro $total do tipo float e retorno float: devolve 0 quando $total >= 50 e 7.5 nos outros casos, usando o operador ternário num único return (em uma linha).",
                PhpMix("function frete(float $total): float { return @0; }",
                    new[] { "$total >= 50 ? @1 : @2", "($total >= 50) ? @1 : @2", "$total < 50 ? @2 : @1", "($total < 50) ? @2 : @1" },
                    new[] { "0", "0.0", "0.00" },
                    new[] { "7.5", "7.50" }),
                A("Tipos: function nome(float $x): float { ... }. Ternário: condição ? valorSeVerdade : valorSeFalso.", "function frete(float $total): float { return $total >= 50 ? ... : ...; }"),
                A("float $total", "Parâmetro tipado.", ": float", "Tipo do retorno.", "$total >= 50 ? 0 : 7.5", "Ternário: se a condição vale, 0; senão, 7.5."),
                "Com declare(strict_types=1); no topo do arquivo, o PHP recusa tipos errados em vez de converter."));
            list.Add(C(H, 1, "code", "Copo com match",
                "Cada tamanho de copo tem um volume em ml. O switch antigo tinha um break esquecido e o pedido P saía com 300 ml.",
                "Guarde em $ml o resultado de um match sobre $tamanho: \"P\" vale 200, \"M\" vale 300, \"G\" vale 400 e qualquer outro valor vale 0 (nessa ordem).",
                A("$ml = match ($tamanho) {\n    \"P\" => 200,\n    \"M\" => 300,\n    \"G\" => 400,\n    default => 0,\n};",
                  "$ml = match ($tamanho) {\n    \"P\" => 200,\n    \"M\" => 300,\n    \"G\" => 400,\n    default => 0\n};"),
                A("match é uma expressão: devolve um valor, que pode ir direto para a variável. Cada braço é valor => resultado.", "$ml = match ($tamanho) { \"P\" => 200, ..., default => 0 };"),
                A("match ($tamanho)", "Compara $tamanho com cada braço (usando ===).", "\"P\" => 200", "Braço: se for \"P\", o resultado é 200.", "default => 0", "Qualquer outro valor."),
                "match compara com === e não precisa de break. Sem default, um valor desconhecido lança UnhandledMatchError."));
            list.Add(C(H, 1, "cmd", "Model com artisan",
                "O backend Laravel vai ganhar a tabela de pedidos. O Igor quer o model, a migration e o controller criados de uma vez.",
                "Gere o model Pedido já com a migration e o controller, numa única execução do artisan (sem outras opções).",
                A("php artisan make:model Pedido -mc", "php artisan make:model Pedido -m -c",
                  "php artisan make:model Pedido --migration --controller", "php artisan make:model Pedido -m --controller",
                  "php artisan make:model Pedido --migration -c"),
                A("O comando de gerar model é make:model. Opções: -m (migration), -c (controller)...", "php artisan make:model Pedido -__"),
                A("make:model Pedido", "Cria app/Models/Pedido.php.", "-m", "Também cria a migration da tabela pedidos.", "-c", "Também cria o PedidoController."),
                "-a (--all) cria model, migration, factory, seeder, controller e policy de uma vez."));

            // ── novos — PROFISSIONAL
            list.Add(C(H, 2, "code", "Busca por cliente",
                "O Rui (DBA) avisou: a busca de pedidos por cliente ainda concatena $_GET no SQL. Bora trocar por parâmetro nomeado e devolver arrays associativos (sem índices numéricos duplicados).",
                "Em três linhas: prepare em $stmt (com $pdo) a consulta SELECT * FROM pedidos WHERE cliente_id = :cliente_id; execute passando $clienteId para esse marcador num array associativo; guarde em $pedidos todas as linhas, cada uma como array associativo.",
                PhpMix("$stmt = $pdo->prepare(\"SELECT * FROM pedidos WHERE cliente_id = :cliente_id\");\n$stmt->execute(@0);\n$pedidos = $stmt->fetchAll(@1::FETCH_ASSOC);",
                    new[] { "[\"cliente_id\" => $clienteId]", "[\":cliente_id\" => $clienteId]", "array(\"cliente_id\" => $clienteId)", "array(\":cliente_id\" => $clienteId)" },
                    new[] { "PDO", "\\PDO" }),
                A("Marcadores nomeados começam com dois-pontos (:nome) e o execute recebe [\"nome\" => valor]. Para buscar todas as linhas: fetchAll.", "$stmt->execute([\"cliente_id\" => $clienteId]);  e  $stmt->fetchAll(PDO::FETCH_ASSOC);"),
                A(":cliente_id", "Marcador nomeado: o valor vai separado do SQL.", "[\"cliente_id\" => $clienteId]", "Liga o marcador ao valor.",
                  "fetchAll(PDO::FETCH_ASSOC)", "Todas as linhas, só com as chaves pelo nome da coluna (o padrão FETCH_BOTH duplica tudo com índices numéricos)."),
                "Defina o padrão na conexão: $pdo->setAttribute(PDO::ATTR_DEFAULT_FETCH_MODE, PDO::FETCH_ASSOC); e PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION."));
            list.Add(C(H, 2, "code", "Paginação segura",
                "A listagem paginada quebrou: com execute([$limite, $offset]) o PDO (com prepares emulados) manda os números como texto e o MySQL recebe LIMIT '10' — erro de sintaxe. O $stmt já está preparado com ... LIMIT :limite OFFSET :offset.",
                "Vincule $limite e $offset aos marcadores :limite e :offset (nessa ordem), informando ao PDO que os dois são inteiros, e depois execute $stmt sem argumentos.",
                PhpMix("$stmt->@0(\"@1limite\", $limite, @2::PARAM_INT);\n$stmt->@0(\"@1offset\", $offset, @2::PARAM_INT);\n$stmt->execute();",
                    new[] { "bindValue", "bindParam" }, new[] { ":", "" }, new[] { "PDO", "\\PDO" }),
                A("Em vez de passar os valores no execute, vincule um por um informando o tipo (constante PDO::PARAM_...).", "$stmt->bindValue(\":limite\", $limite, PDO::PARAM_INT); ... e no fim $stmt->execute();"),
                A("bindValue(\":limite\", $limite, ...)", "Liga o valor ao marcador.", "PDO::PARAM_INT", "Envia como número, não como texto entre aspas.", "execute()", "Sem array: os valores já foram vinculados."),
                "Nunca concatene LIMIT/OFFSET vindos da URL; se precisar, converta com (int) e limite o máximo (ex.: min($limite, 100))."));
            list.Add(C(H, 2, "code", "XSS no atributo",
                "O Igor achou um XSS na busca: alguém digitou \" onmouseover=\"alert(1) e o texto fechou o atributo e virou script. Atributos podem usar aspas simples ou duplas, então as duas precisam ser escapadas.",
                "Escreva a tag <input name=\"q\" value=\"...\"> em que o value mostra $busca escapado com htmlspecialchars, convertendo aspas duplas E simples, com charset UTF-8 (passe os três argumentos). Use a tag curta de echo do PHP.",
                PhpMix("<input @0 @1>",
                    new[] { "name=\"q\"" },
                    new[] { "value=\"@2\"" },
                    new[] { "<?= htmlspecialchars($busca, @3, \"UTF-8\") ?>", "<?php echo htmlspecialchars($busca, @3, \"UTF-8\"); ?>" },
                    new[] { "ENT_QUOTES", "ENT_QUOTES | ENT_HTML5", "ENT_QUOTES | ENT_SUBSTITUTE", "ENT_QUOTES | ENT_SUBSTITUTE | ENT_HTML5", "ENT_QUOTES | ENT_HTML5 | ENT_SUBSTITUTE" })
                .Concat(PhpMix("<input value=\"@0\" name=\"q\">",
                    new[] { "<?= htmlspecialchars($busca, @1, \"UTF-8\") ?>", "<?php echo htmlspecialchars($busca, @1, \"UTF-8\"); ?>" },
                    new[] { "ENT_QUOTES", "ENT_QUOTES | ENT_HTML5", "ENT_QUOTES | ENT_SUBSTITUTE", "ENT_QUOTES | ENT_SUBSTITUTE | ENT_HTML5", "ENT_QUOTES | ENT_HTML5 | ENT_SUBSTITUTE" })).ToArray(),
                A("htmlspecialchars(texto, FLAGS, charset). A flag que converte \" e ' é ENT_QUOTES. A tag curta de echo é <?= ... ?>.", "<input name=\"q\" value=\"<?= htmlspecialchars($busca, ENT_QUOTES, 'UTF-8') ?>\">"),
                A("<?= ... ?>", "Atalho para <?php echo ... ?>.", "htmlspecialchars", "Troca < > & \" por entidades HTML (&lt; &gt; &amp; &quot;).",
                  "ENT_QUOTES", "Converte também a aspa simples (&#039;). Desde o PHP 8.1 já é padrão, mas deixe explícito.", "\"UTF-8\"", "Charset do texto."),
                "Escape na SAÍDA, de acordo com o contexto (HTML, atributo, JS, URL). Em templates Blade, {{ $x }} já faz isso por você."));
            list.Add(C(H, 2, "code", "Rota protegida",
                "No Laravel, GET /pedidos/{id} deve chamar o método show do PedidoController, só para usuários logados, e só aceitar id numérico (/pedidos/abc tem que dar 404).",
                "Em routes/web.php: importe App\\Http\\Controllers\\PedidoController com use e registre a rota GET /pedidos/{id} apontando para o método \"show\" com a sintaxe de array [Classe::class, \"método\"]. Encadeie o middleware auth e depois a restrição whereNumber no parâmetro id.",
                PhpMix("use App\\Http\\Controllers\\PedidoController;\n\nRoute::get(\"@0\", [PedidoController::class, \"show\"])@1;",
                    new[] { "/pedidos/{id}", "pedidos/{id}" },
                    new[] { "->middleware(\"auth\")@2", "@2->middleware(\"auth\")" },
                    new[] { "->whereNumber(\"id\")", "->where(\"id\", \"[0-9]+\")", "->where(\"id\", \"\\d+\")" }),
                A("Rotas: Route::get(caminho, [Controller::class, \"metodo\"]). Middleware e restrições são métodos encadeados com ->.", "Route::get(\"/pedidos/{id}\", [PedidoController::class, \"show\"])->middleware(\"auth\")->whereNumber(\"id\");"),
                A("use App\\Http\\Controllers\\PedidoController;", "Importa a classe pelo namespace completo.", "PedidoController::class", "Nome completo da classe como texto.",
                  "->middleware(\"auth\")", "Só usuários autenticados passam.", "->whereNumber(\"id\")", "{id} precisa ser numérico; senão, 404."),
                "Rode php artisan route:list para ver as rotas registradas, seus middlewares e controllers."));
            list.Add(C(H, 2, "code", "Nomes disponíveis",
                "O endpoint /cardapio devolve os nomes dos produtos com estoque. Mas o app Flutter quebrou: o JSON veio como {\"1\":\"Latte\",\"3\":\"Mocha\"} em vez de [\"Latte\",\"Mocha\"]. Culpado: array_filter mantém as chaves originais.",
                "Em duas linhas, usando arrow functions (fn) com parâmetro $p: guarde em $disponiveis os itens de $produtos com $p[\"estoque\"] > 0 (array_filter) e em $nomes só o $p[\"nome\"] de cada um (array_map). $nomes precisa ser uma lista com índices 0, 1, 2... — use a função que reindexa o array.",
                A("$disponiveis = array_filter($produtos, fn($p) => $p[\"estoque\"] > 0);\n$nomes = array_values(array_map(fn($p) => $p[\"nome\"], $disponiveis));",
                  "$disponiveis = array_values(array_filter($produtos, fn($p) => $p[\"estoque\"] > 0));\n$nomes = array_map(fn($p) => $p[\"nome\"], $disponiveis);",
                  "$disponiveis = array_filter($produtos, fn($p) => $p[\"estoque\"] > 0);\n$nomes = array_map(fn($p) => $p[\"nome\"], array_values($disponiveis));",
                  "$disponiveis = array_filter($produtos, fn($p) => $p[\"estoque\"] > 0);\n$nomes = array_column($disponiveis, \"nome\");"),
                A("array_filter(array, callback) e array_map(callback, array) — repare na ordem invertida dos argumentos! Para renumerar as chaves: array_values.", "$disponiveis = array_filter($produtos, fn($p) => ...);  $nomes = array_values(array_map(fn($p) => $p[\"nome\"], $disponiveis));"),
                A("array_filter($produtos, fn($p) => ...)", "Mantém só os itens em que a função devolve true (com as chaves originais!).", "array_map(fn($p) => $p[\"nome\"], ...)", "Transforma cada item no seu nome.",
                  "array_values(...)", "Renumera as chaves para 0, 1, 2... → json_encode gera uma lista [ ].", "fn($p) =>", "Arrow function: uma expressão, e enxerga as variáveis de fora."),
                "json_encode só gera [ ] quando as chaves são 0..n-1 em sequência; qualquer buraco vira objeto { }."));
            list.Add(C(H, 2, "code", "E-mail inválido (422)",
                "Cadastro de clientes da api-pedidos. Regra do Igor: dado inválido responde 422 em JSON, e o script para ali mesmo — nada de seguir e gravar lixo no banco.",
                "Guarde em $email o campo email do POST validado com filter_var (use \"\" se o campo não vier, com ??). Se for inválido ($email === false): responda status 422, cabeçalho Content-Type: application/json, o corpo {\"erro\": \"email invalido\"} (com json_encode) e encerre o script com exit.",
                PhpMix("$email = filter_var($_POST[\"email\"] ?? \"\", FILTER_VALIDATE_EMAIL);\nif (@0) {\n    @1\n    echo json_encode([\"erro\" => \"email invalido\"]);\n    @2;\n}",
                    new[] { "$email === false", "false === $email", "!$email" },
                    new[] { "http_response_code(422);\n    header(\"Content-Type: application/json\");", "header(\"Content-Type: application/json\");\n    http_response_code(422);" },
                    new[] { "exit", "exit()", "die", "die()" }),
                A("filter_var(valor, FILTER_VALIDATE_EMAIL) devolve o e-mail ou false. O status HTTP é definido com http_response_code e o cabeçalho com header.", "if ($email === false) { http_response_code(422); header(\"Content-Type: application/json\"); echo json_encode([...]); exit; }"),
                A("filter_var(..., FILTER_VALIDATE_EMAIL)", "Valida o formato; inválido → false.", "$_POST[\"email\"] ?? \"\"", "Evita aviso se o campo não veio.",
                  "http_response_code(422)", "422 Unprocessable Content: dados com formato inválido.", "exit", "Encerra o script: nada depois disso roda."),
                "header() e http_response_code() precisam vir ANTES de qualquer echo (senão: headers already sent)."));
            list.Add(C(H, 2, "code", "Cookie de sessão",
                "O pentest apontou três problemas no cookie PHPSESSID: pode ser lido por JavaScript (roubo via XSS), trafega em HTTP puro e é enviado em requisições vindas de outros sites (CSRF).",
                "Inicie a sessão passando para session_start um array de opções que corrija os três problemas, nessa ordem: bloquear o acesso via JavaScript, só enviar via HTTPS e SameSite \"Strict\" (use true nas opções liga/desliga).",
                PhpSessionAnswers(),
                A("session_start aceita um array com as opções session.* sem o prefixo \"session.\": cookie_httponly, cookie_secure, cookie_samesite.", "session_start([\"cookie_httponly\" => true, \"cookie_secure\" => true, \"cookie_samesite\" => \"Strict\"]);"),
                A("\"cookie_httponly\" => true", "JavaScript (document.cookie) não lê o cookie.", "\"cookie_secure\" => true", "O navegador só envia o cookie por HTTPS.",
                  "\"cookie_samesite\" => \"Strict\"", "Não envia o cookie em requisições iniciadas por outros sites."),
                "Para o projeto todo, configure no php.ini: session.cookie_httponly=1, session.cookie_secure=1, session.cookie_samesite=Strict, session.use_strict_mode=1."));
            list.Add(C(H, 2, "code", "Login seguro",
                "No login, $usuario veio do banco (ou false se o e-mail não existe), com a coluna senha_hash. Risco de fixação de sessão: se o ID da sessão não mudar depois do login, quem plantou esse ID entra junto.",
                "Escreva um if que só entra quando $usuario existe E a $senha digitada confere com o hash em $usuario[\"senha_hash\"] (use && numa condição só). Dentro: primeiro gere um novo ID de sessão apagando a sessão antiga, depois guarde $usuario[\"id\"] em $_SESSION[\"usuario_id\"].",
                PhpMix("if (@0 && password_verify($senha, $usuario[\"senha_hash\"])) {\n    session_regenerate_id(true);\n    $_SESSION[\"usuario_id\"] = $usuario[\"id\"];\n}",
                    new[] { "$usuario", "$usuario !== false", "!empty($usuario)", "is_array($usuario)" }),
                A("Para conferir senha com hash: password_verify(senha, hash) — nessa ordem. Para trocar o ID da sessão: session_regenerate_id.", "if ($usuario && password_verify(...)) { session_regenerate_id(true); $_SESSION[\"usuario_id\"] = ...; }"),
                A("$usuario &&", "Se o usuário não existe, nem chega a chamar password_verify.", "password_verify($senha, $usuario[\"senha_hash\"])", "Compara a senha com o hash em tempo constante.",
                  "session_regenerate_id(true)", "Novo ID de sessão; true apaga a sessão antiga.", "$_SESSION[\"usuario_id\"]", "Guarda só o id — nunca a senha ou o hash."),
                "Mesma mensagem de erro para e-mail e senha errados (\"credenciais inválidas\") e limite de tentativas contra força bruta."));
            list.Add(C(H, 2, "code", "Pedido em JSON",
                "A api-pedidos vai devolver objetos Pedido direto com json_encode($pedido). Com propriedades private, o json_encode gera {} — a classe precisa dizer como virar JSON.",
                "Crie a classe Pedido que implementa a interface nativa JsonSerializable, com construtor promovido de duas propriedades private: $id (int) e $total (float), e corpo vazio. Implemente o método que a interface exige, com retorno array, devolvendo [\"id\" => ..., \"total\" => ...] com os valores das propriedades.",
                PhpMix("class Pedido implements @0 {\n    public function __construct(\n        private @1int $id,\n        private @1float $total@2\n    ) {}\n\n    public function jsonSerialize(): @3 {\n        return [\"id\" => $this->id, \"total\" => $this->total];\n    }\n}",
                    new[] { "JsonSerializable", "\\JsonSerializable" }, new[] { "", "readonly " }, new[] { "", "," }, new[] { "array", "mixed" }),
                A("Construtor promovido: a visibilidade vai no próprio parâmetro (private int $id) e a propriedade é criada sozinha. A interface JsonSerializable exige o método jsonSerialize.", "class Pedido implements JsonSerializable { public function __construct(private int $id, private float $total) {} public function jsonSerialize(): array { return [...]; } }"),
                A("implements JsonSerializable", "A classe promete ter o método jsonSerialize().", "private int $id", "Parâmetro promovido: declara e preenche a propriedade.",
                  "{}", "Corpo vazio: a promoção já fez as atribuições.", "jsonSerialize(): array", "json_encode($pedido) usa o que esse método devolver."),
                "Expor só o necessário no JSON (nunca dados internos) é parte do design da API."));
            list.Add(C(H, 2, "code", "Transação do pedido",
                "Pedido pago, mas o estoque não baixou: o servidor caiu entre os dois UPDATEs. O Rui quer tudo-ou-nada. As funções baixarEstoque e registrarPagamento já existem e lançam exceção em caso de erro ($pdo está em ERRMODE_EXCEPTION).",
                "Faça baixarEstoque($pdo, $pedido) e registrarPagamento($pdo, $pedido) (nessa ordem) acontecerem numa transação: dentro de um try, inicie a transação, chame as duas funções e confirme; se algo lançar exceção (capture Exception como $e), desfaça a transação e relance a mesma exceção.",
                PhpMix("@0try {\n    @1baixarEstoque($pdo, $pedido);\n    registrarPagamento($pdo, $pedido);\n    $pdo->commit();\n} catch (@2 $e) {\n    @3\n    throw $e;\n}",
                    new[] { "", "$pdo->beginTransaction();\n" }, new[] { "$pdo->beginTransaction();\n    ", "" },
                    new[] { "Exception", "\\Exception", "Throwable", "\\Throwable" },
                    new[] { "$pdo->rollBack();", "if ($pdo->inTransaction()) {\n        $pdo->rollBack();\n    }" })
                .Where(s => s.Split(new[] { "beginTransaction" }, StringSplitOptions.None).Length == 2).ToArray(),
                A("No PDO: beginTransaction() abre, commit() confirma e rollBack() desfaz. No catch, depois de desfazer, use throw para não esconder o erro.", "try { $pdo->beginTransaction(); ...; $pdo->commit(); } catch (Exception $e) { $pdo->rollBack(); throw $e; }"),
                A("$pdo->beginTransaction()", "Desliga o autocommit até o commit/rollBack.", "$pdo->commit()", "Grava as duas operações juntas.",
                  "$pdo->rollBack()", "Desfaz tudo desde o beginTransaction.", "throw $e", "Relança: quem chamou fica sabendo que falhou (e o log registra)."),
                "Engolir a exceção (catch vazio) é pior que não ter transação: o erro some e ninguém descobre. Tabelas MyISAM ignoram transações — use InnoDB."));
        }

        // session_start com as 3 opções em qualquer ordem, true ou 1, com ou sem vírgula final
        static string[] PhpSessionAnswers()
        {
            var opts = new[] { "\"cookie_httponly\" => @0", "\"cookie_secure\" => @1", "\"cookie_samesite\" => \"Strict\"" };
            var perms = new[] { new[] { 0, 1, 2 }, new[] { 0, 2, 1 }, new[] { 1, 0, 2 }, new[] { 1, 2, 0 }, new[] { 2, 0, 1 }, new[] { 2, 1, 0 } };
            var res = new List<string>();
            foreach (var p in perms)
            {
                string body = "    " + opts[p[0]] + ",\n    " + opts[p[1]] + ",\n    " + opts[p[2]];
                res.AddRange(PhpMix("session_start([\n" + body + "@2\n]);", new[] { "true", "1" }, new[] { "true", "1" }, new[] { ",", "" }));
            }
            return res.Distinct().ToArray();
        }
    }

    static partial class Docs
    {
        static partial void DocsMaisPhp(List<DocEntry> all)
        {
            const string H = "PHP";
            all.Add(X(H, "Básico", "Concatenação (.) e interpolação", "O ponto (.) junta strings. Dentro de aspas DUPLAS, variáveis são trocadas pelo valor; em aspas simples, não.",
                "\"texto \" . $variavel . \" texto\"\n\"texto $variavel texto\"\n\"texto {$variavel} texto\"",
                A(".", "concatena (não use + para textos)", ".=", "acrescenta ao fim: $msg .= \"!\";", "{$var}", "delimita a variável dentro das aspas duplas"),
                A("echo \"Turma: \" . $turma . \".\";", "concatenação", "echo \"Aluno: $aluno\";", "interpolação", "echo 'Aluno: $aluno';", "aspas simples: mostra $aluno literalmente"),
                "echo também aceita vários argumentos separados por vírgula: echo \"a\", $b;"));
            all.Add(X(H, "Básico", "?? (coalescência nula)", "Devolve o valor da esquerda se ele existir e não for null; senão, o da direita. Não gera aviso para chave inexistente.",
                "$x = $valor ?? padrao;\n$x ??= padrao;", null,
                A("$pagina = $_GET[\"pagina\"] ?? 1;", "página 1 se não vier na URL", "$apelido = $aluno[\"apelido\"] ?? $aluno[\"nome\"];", "encadeia alternativas"),
                "Diferente do ?: (Elvis), que usa o da direita também para valores \"falsos\" como 0 e \"\"."));
            all.Add(X(H, "Básico", "Operador ternário  ? :", "if/else numa expressão: condição ? valorSeVerdade : valorSeFalso.",
                "$x = condicao ? valor1 : valor2;", null,
                A("$situacao = $nota >= 6 ? \"aprovado\" : \"recuperação\";", "escolhe o texto", "return $idade < 12 ? 0 : 15.5;", "direto no return"),
                "Evite ternários aninhados: no PHP 8 eles exigem parênteses."));
            all.Add(X(H, "Básico", "Funções com tipos", "Declara o tipo de cada parâmetro e do retorno.",
                "function nome(tipo $param): tipoRetorno { ... }",
                A("int, float, string, bool, array", "tipos escalares e array", "?string", "pode ser string ou null", "void", "não retorna nada", "mixed", "qualquer tipo"),
                A("function media(float $a, float $b): float { return ($a + $b) / 2; }", "dois parâmetros float, retorno float"),
                "declare(strict_types=1); (primeira linha do arquivo) faz o PHP recusar tipos errados em vez de converter."));
            all.Add(X(H, "Básico", "match", "Expressão que compara um valor com vários braços (usando ===) e devolve o resultado do braço que casar. Não precisa de break.",
                "$x = match ($valor) {\n    opcao1 => resultado1,\n    opcao2, opcao3 => resultado2,\n    default => outro,\n};", null,
                A("$turno = match ($codigo) {\n    \"M\" => \"manhã\",\n    \"T\" => \"tarde\",\n    default => \"noite\",\n};", "texto do turno pela letra"),
                "Sem default, um valor que não casa com nenhum braço lança UnhandledMatchError."));
            all.Add(X(H, "Arrays", "in_array / array_key_exists", "in_array procura um VALOR; array_key_exists procura uma CHAVE.",
                "in_array($valor, $array, true)\narray_key_exists($chave, $array)",
                A("true (3º argumento)", "comparação estrita (===): \"1\" não é igual a 1"),
                A("in_array(\"3B\", $turmas, true)", "a turma existe na lista?", "array_key_exists(\"email\", $aluno)", "o array tem a chave email?"), null));
            all.Add(X(H, "Arrays", "array_filter / array_map / array_values", "array_filter mantém os itens em que a função devolve true (preservando as CHAVES originais). array_map transforma cada item. array_values renumera as chaves para 0, 1, 2...",
                "array_filter($array, fn($x) => condicao)\narray_map(fn($x) => novoValor, $array)\narray_values($array)",
                A("fn($x) => expr", "arrow function: uma única expressão, enxerga as variáveis de fora", "array_column($array, \"chave\")", "pega uma coluna de um array de arrays (já reindexado)"),
                A("$aprovados = array_filter($alunos, fn($a) => $a[\"nota\"] >= 6);", "filtra (chaves podem ficar 0, 2, 5...)",
                  "$titulos = array_map(fn($l) => strtoupper($l[\"titulo\"]), $livros);", "transforma",
                  "echo json_encode(array_values($aprovados));", "renumera → JSON vira lista [ ]"),
                "ATENÇÃO à ordem dos argumentos: array_filter(array, função) mas array_map(função, array). json_encode só gera [ ] se as chaves forem 0..n-1."));
            all.Add(X(H, "Web", "htmlspecialchars  (contra XSS)", "Converte caracteres especiais em entidades HTML antes de mostrar dados do usuário na página.",
                "htmlspecialchars($texto, FLAGS, \"UTF-8\")\n<?= expressao ?>   (atalho de <?php echo expressao ?>)",
                A("ENT_QUOTES", "converte aspas duplas E simples (essencial dentro de atributos)", "ENT_SUBSTITUTE", "troca bytes inválidos em vez de devolver vazio", "ENT_HTML5", "usa as entidades do HTML5", "|", "combina flags: ENT_QUOTES | ENT_HTML5"),
                A("<h2><?= htmlspecialchars($titulo, ENT_QUOTES, 'UTF-8') ?></h2>", "título escapado no HTML",
                  "<a title=\"<?= htmlspecialchars($autor, ENT_QUOTES, 'UTF-8') ?>\">Autor</a>", "escapado dentro de um atributo"),
                "Escape na SAÍDA, sempre. Validar a entrada não substitui escapar a saída."));
            all.Add(X(H, "Web", "filter_var  (validação)", "Valida (ou limpa) um valor. Com filtros FILTER_VALIDATE_*, devolve o valor válido ou false.",
                "filter_var($valor, FILTRO)",
                A("FILTER_VALIDATE_EMAIL", "e-mail", "FILTER_VALIDATE_INT", "inteiro (devolve int)", "FILTER_VALIDATE_URL", "URL", "FILTER_VALIDATE_BOOLEAN", "\"1\", \"true\", \"on\", \"yes\" → true"),
                A("$idade = filter_var($_POST[\"idade\"] ?? \"\", FILTER_VALIDATE_INT);", "inteiro ou false",
                  "if ($site === false) { ... }", "compare com === false (0 é um inteiro válido!)"), null));
            all.Add(X(H, "Web", "http_response_code / header / exit", "Define o status HTTP e os cabeçalhos da resposta; exit (ou die) encerra o script.",
                "http_response_code(CODIGO);\nheader(\"Nome: valor\");\nexit;",
                A("200 / 201", "ok / criado", "204", "sem conteúdo", "400", "requisição malformada", "401 / 403", "não autenticado / sem permissão",
                  "404", "não encontrado", "422", "dados com formato inválido", "500", "erro no servidor"),
                A("http_response_code(403);\nheader(\"Content-Type: application/json\");\necho json_encode([\"erro\" => \"sem acesso\"]);\nexit;", "nega acesso respondendo JSON"),
                "Cabeçalhos e status precisam ser enviados ANTES de qualquer saída (echo, HTML, espaço antes de <?php)."));
            all.Add(X(H, "Web", "Sessões: session_start / $_SESSION", "Guarda dados do usuário entre requisições (o navegador só leva o cookie PHPSESSID).",
                "session_start([opções]);\n$_SESSION[\"chave\"] = valor;\nsession_regenerate_id(true);\nsession_destroy();",
                A("\"cookie_httponly\" => true", "JavaScript não lê o cookie", "\"cookie_secure\" => true", "cookie só via HTTPS",
                  "\"cookie_samesite\" => \"Lax\" | \"Strict\"", "não envia o cookie em requisições de outros sites (Strict: nunca)", "\"cookie_lifetime\" => 0", "dura até fechar o navegador",
                  "session_regenerate_id(true)", "novo ID de sessão (true apaga a antiga) — use após o login"),
                A("session_start([\"cookie_lifetime\" => 3600]);", "sessão com cookie de 1 hora", "$_SESSION[\"aluno_id\"] = $aluno[\"id\"];", "guarda quem está logado",
                  "$id = $_SESSION[\"aluno_id\"] ?? null;", "lê com segurança"),
                "As opções do array são as diretivas session.* do php.ini sem o prefixo \"session.\"."));
            all.Add(X(H, "Banco e segurança", "PDO: marcadores nomeados e fetchAll", "Marcadores :nome deixam a consulta mais legível que ?. O execute recebe um array associativo.",
                "$stmt = $pdo->prepare(\"... WHERE coluna = :nome\");\n$stmt->execute([\"nome\" => $valor]);\n$linhas = $stmt->fetchAll(MODO);",
                A("fetch()", "uma linha (ou false)", "fetchAll()", "todas as linhas", "PDO::FETCH_ASSOC", "cada linha como [\"coluna\" => valor]", "PDO::FETCH_BOTH", "padrão: chaves por nome E por número (duplicado)", "fetchColumn()", "o valor da 1ª coluna"),
                A("$q = $db->prepare(\"SELECT * FROM livros WHERE autor = :autor\");", "prepara com marcador nomeado",
                  "$q->execute([\"autor\" => $autor]);", "o \":\" na chave é opcional",
                  "$livros = $q->fetchAll(PDO::FETCH_ASSOC);", "array de arrays associativos"), null));
            all.Add(X(H, "Banco e segurança", "bindValue / bindParam  (tipos)", "Vincula um valor a um marcador informando o tipo. execute([...]) envia tudo como texto.",
                "$stmt->bindValue(\":nome\", $valor, PDO::PARAM_TIPO);\n$stmt->execute();",
                A("PDO::PARAM_INT", "inteiro", "PDO::PARAM_STR", "texto (padrão)", "PDO::PARAM_BOOL", "booleano", "PDO::PARAM_NULL", "NULL",
                  "bindParam", "como bindValue, mas liga a VARIÁVEL (lida só no execute)"),
                A("$q = $db->prepare(\"SELECT * FROM livros WHERE ano = :ano\");\n$q->bindValue(\":ano\", $ano, PDO::PARAM_INT);\n$q->execute();", "ano enviado como número"),
                "Com prepares emulados (padrão do pdo_mysql), números passados em execute([...]) viram '10' entre aspas — em LIMIT isso é erro de sintaxe."));
            all.Add(X(H, "Banco e segurança", "Transações no PDO", "Agrupa operações: ou todas valem (commit) ou nenhuma (rollBack).",
                "$pdo->beginTransaction();\n$pdo->commit();\n$pdo->rollBack();",
                A("beginTransaction()", "começa (desliga o autocommit)", "commit()", "confirma", "rollBack()", "desfaz", "inTransaction()", "há transação aberta?",
                  "throw $e", "relança a exceção capturada"),
                A("try {\n    $db->beginTransaction();\n    devolverLivro($db, $id);\n    liberarReserva($db, $id);\n    $db->commit();\n} catch (Exception $e) {\n    $db->rollBack();\n    throw $e;\n}", "devolução e reserva: tudo ou nada"),
                "Use $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION) para que erros de SQL virem exceções."));
            all.Add(X(H, "Orientação a objetos", "class / construtor promovido", "Classes agrupam dados e comportamento. No construtor promovido (PHP 8), a visibilidade no parâmetro já cria e preenche a propriedade.",
                "class Nome {\n    public function __construct(\n        private tipo $prop,\n    ) {}\n}",
                A("public / protected / private", "visibilidade", "readonly", "só pode ser atribuída uma vez", "$this->prop", "acessa a propriedade dentro da classe", "new Nome(...)", "cria o objeto"),
                A("class Livro {\n    public function __construct(private string $titulo, private int $ano) {}\n    public function idade(): int { return 2025 - $this->ano; }\n}", "duas propriedades promovidas e um método"), null));
            all.Add(X(H, "Orientação a objetos", "interface / implements  (JsonSerializable)", "Uma interface lista métodos que a classe promete ter. JsonSerializable é nativa: json_encode($obj) usa o retorno de jsonSerialize().",
                "interface Nome { public function metodo(): tipo; }\nclass Classe implements Nome { ... }",
                A("implements A, B", "uma classe pode implementar várias interfaces", "jsonSerialize(): mixed", "assinatura da interface (array também é aceito, por ser mais específico)"),
                A("interface Imprimivel {\n    public function imprimir(): string;\n}", "declara a interface",
                  "class Aluno implements JsonSerializable {\n    public function __construct(private string $nome) {}\n    public function jsonSerialize(): array { return [\"nome\" => $this->nome]; }\n}", "objeto que vira JSON"),
                "Sem JsonSerializable, json_encode só enxerga as propriedades public."));
            all.Add(X(H, "Orientação a objetos", "namespace / use", "namespace organiza as classes em \"pastas\" lógicas; use importa uma classe para usá-la pelo nome curto.",
                "namespace App\\Pasta;\nuse Namespace\\Completo\\Classe;\nClasse::class   (nome completo como texto)", null,
                A("namespace App\\Models;", "a classe deste arquivo fica em App\\Models", "use App\\Models\\Livro;", "importa Livro", "Livro::class", "\"App\\Models\\Livro\""),
                "Com o autoload PSR-4 do Composer, App\\Models\\Livro fica em app/Models/Livro.php."));
            all.Add(X(H, "Laravel", "php artisan make:model", "Gera um model Eloquent (e, com opções, os arquivos relacionados).",
                "php artisan make:model Nome [opções]",
                A("-m, --migration", "cria a migration da tabela", "-c, --controller", "cria o controller", "-r, --resource", "controller com os métodos de CRUD",
                  "-f, --factory", "cria a factory", "-s, --seed", "cria o seeder", "-a, --all", "tudo isso de uma vez"),
                A("php artisan make:model Livro -m", "model + migration", "php artisan make:model Aluno -fs", "model + factory + seeder"),
                "Opções curtas podem ser juntadas: -fs = -f -s."));
            all.Add(X(H, "Laravel", "Route::  (rotas)", "Liga um método HTTP + caminho a uma ação de controller. Fica em routes/web.php (ou routes/api.php).",
                "Route::get(\"/caminho/{param}\", [Controller::class, \"metodo\"]);",
                A("Route::get / post / put / delete", "método HTTP", "{param}", "parâmetro da URL", "->middleware(\"auth\")", "exige usuário logado",
                  "->whereNumber(\"param\")", "parâmetro só numérico", "->where(\"param\", \"regex\")", "restrição por expressão regular", "->name(\"nome\")", "nomeia a rota"),
                A("use App\\Http\\Controllers\\LivroController;", "importa o controller",
                  "Route::post(\"/emprestimos\", [EmprestimoController::class, \"store\"])->middleware(\"auth\");", "rota protegida",
                  "Route::get(\"/alunos/{matricula}\", [AlunoController::class, \"show\"])->whereNumber(\"matricula\");", "só aceita número"),
                "php artisan route:list mostra todas as rotas registradas."));
        }
    }
}
