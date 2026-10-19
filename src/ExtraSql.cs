using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerQuest
{
    static partial class Challenges
    {
        // ───────────────────────────── SQL: nova leva (x1 … x72) ─────────────────────────────
        // 24 desafios por nível. Tudo roda no banco cafe365 (SqlSample), que agora tem também as tabelas
        // funcionarios (gerente_id aponta para a própria tabela) e entregas (ligada a pedidos; entregador_id sem chave estrangeira de propósito).
        static partial void ExtraSql(List<Level> list)
        {
            Action<string> Cabecalhos = id => SqlTasks.All[id].Headers = true;
            Action<string> Transacao = id => SqlTasks.All[id].NeedTx = true;

            // ═════════════════════════════ Iniciante ═════════════════════════════
            list.Add(SqlL(0, "x1", "query", "Pedido número 8",
                "Um cliente ligou reclamando do pedido 8 e a Tânia, DBA, pediu para você puxar a ficha completa dele.",
                "Mostre todas as colunas do pedido cujo id é 8, na tabela pedidos.",
                "SELECT * FROM pedidos WHERE id = 8;",
                A("Comece mostrando todas as colunas da tabela e depois filtre a linha com WHERE.", "SELECT * FROM pedidos WHERE id = ...;"),
                A("SELECT * FROM pedidos", "Todas as colunas da tabela pedidos.", "WHERE id = 8", "Só a linha cuja chave primária vale 8 (número vai sem aspas)."),
                "Buscar pela chave primária (id) é a consulta mais rápida que existe: o banco vai direto na linha, sem varrer a tabela."));
            list.Add(SqlL(0, "x2", "query", "Produtos esgotados",
                "A cozinha do Café 365 precisa saber o que acabou para o app não vender o que não tem.",
                "Liste o nome dos produtos com estoque igual a 0.",
                "SELECT nome FROM produtos WHERE estoque = 0;",
                A("Filtre pela coluna estoque comparando com o número zero.", "SELECT nome FROM produtos WHERE estoque = ...;"),
                A("SELECT nome", "Só a coluna nome.", "WHERE estoque = 0", "Só os produtos com estoque zerado."),
                "Zero e NULL são coisas diferentes: estoque = 0 significa 'acabou'; NULL significaria 'ninguém sabe quanto tem'."));
            list.Add(SqlL(0, "x3", "query", "Clientes em ordem alfabética",
                "O atendimento quer a lista de clientes em ordem alfabética para achar os nomes mais rápido.",
                "Liste o nome de todos os clientes em ordem alfabética (de A a Z).",
                "SELECT nome FROM clientes ORDER BY nome;",
                A("Quem ordena o resultado é o ORDER BY. A ordem crescente (A→Z) é o padrão.", "SELECT nome FROM clientes ORDER BY ...;"),
                A("ORDER BY nome", "Ordena pelo nome.", "ASC (opcional)", "Crescente é o padrão: ORDER BY nome ASC dá no mesmo."),
                "No MySQL, a ordem de texto segue a collation do banco: normalmente ignora maiúsculas e acentos ('Álvaro' fica junto dos outros 'A').",
                true));
            list.Add(SqlL(0, "x4", "query", "Menos de 6 reais",
                "A Glória, DBA da Micorosoft, vai montar o combo 'lanche rápido' só com itens que custam menos de 6 reais.",
                "Liste o nome e o preço dos produtos com preço MENOR que 6.00 (quem custa exatamente 6.00 fica de fora).",
                "SELECT nome, preco FROM produtos WHERE preco < 6;",
                A("Use um operador de comparação no WHERE. 'Menor que' não inclui o próprio 6.", "SELECT nome, preco FROM produtos WHERE preco < 6;"),
                A("WHERE preco < 6", "Só preços menores que 6 (5.99 entra, 6.00 não).", "<  <=  >  >=", "Menor, menor ou igual, maior, maior ou igual."),
                "Preste atenção em 'menor que' (<) e 'até' (<=): com <= a Coxinha, que custa 6.00, entraria no combo."));
            list.Add(SqlL(0, "x5", "query", "Tudo menos salgados",
                "A loja do aeroporto não tem forno, então lá não se vendem salgados.",
                "Liste o nome dos produtos cuja categoria é DIFERENTE de 'salgado'.",
                "SELECT nome FROM produtos WHERE categoria <> 'salgado';",
                A("Existe um operador que significa 'diferente de'.", "SELECT nome FROM produtos WHERE categoria <> 'salgado';  (também vale !=)"),
                A("<>", "Diferente de (o MySQL também aceita !=).", "'salgado'", "Texto entre aspas simples."),
                "Cuidado: categoria <> 'salgado' NÃO traz as linhas em que a categoria é NULL. Para incluí-las: ... OR categoria IS NULL."));
            list.Add(SqlL(0, "x6", "query", "Maior pedido",
                "O Fábio, analista de BI, quer saber qual foi o maior valor de pedido já registrado no Café 365.",
                "Mostre o maior valor da coluna total da tabela pedidos.",
                "SELECT MAX(total) FROM pedidos;",
                A("Existe uma função de agregação que devolve o maior valor de uma coluna.", "SELECT MAX(...) FROM pedidos;"),
                A("MAX(total)", "O maior valor da coluna total (o contrário é MIN).", "FROM pedidos", "Considera todas as linhas da tabela."),
                "MAX e MIN também funcionam com datas e textos: MAX(data) é a data mais recente."));
            list.Add(SqlL(0, "x7", "query", "Doces baratos",
                "A Glória quer um docinho barato para dar de brinde: tem que ser da categoria 'doce' e custar menos de 5 reais.",
                "Liste o nome e o preço dos produtos da categoria 'doce' com preço menor que 5.00 (as duas condições ao mesmo tempo).",
                "SELECT nome, preco FROM produtos WHERE categoria = 'doce' AND preco < 5;",
                A("Quando as DUAS condições precisam ser verdadeiras, junte-as com AND.", "... WHERE categoria = 'doce' AND preco < 5;"),
                A("categoria = 'doce'", "Primeira condição.", "AND", "As duas condições precisam ser verdadeiras ao mesmo tempo.", "preco < 5", "Segunda condição."),
                "AND estreita o resultado (precisa cumprir tudo); OR alarga (basta cumprir uma das condições)."));
            list.Add(SqlL(0, "x8", "query", "Clientes de duas cidades",
                "O entregador novo vai cobrir Recife e Curitiba e quer saber quem mora por lá.",
                "Liste o nome dos clientes cuja cidade é 'Recife' ou 'Curitiba'.",
                "SELECT nome FROM clientes WHERE cidade IN ('Recife', 'Curitiba');",
                A("Para comparar com uma lista de valores existe um atalho mais legível do que vários OR.", "SELECT nome FROM clientes WHERE cidade IN ('Recife', 'Curitiba');"),
                A("IN ('Recife', 'Curitiba')", "Verdadeiro se a cidade for qualquer um dos valores da lista.", "cidade = 'Recife' OR cidade = 'Curitiba'", "Forma equivalente, mais longa."),
                "IN fica bem mais legível que uma fila de OR quando a lista cresce. O contrário é NOT IN."));
            list.Add(SqlL(0, "x9", "query", "Quantos pedidos entregues",
                "A diretoria quer o número de pedidos que já chegaram ao cliente.",
                "Conte quantos pedidos têm status 'entregue'.",
                "SELECT COUNT(*) FROM pedidos WHERE status = 'entregue';",
                A("Filtre com WHERE e conte as linhas que sobraram.", "SELECT COUNT(*) FROM pedidos WHERE status = ...;"),
                A("WHERE status = 'entregue'", "Primeiro o filtro: só os entregues.", "COUNT(*)", "Depois, a contagem das linhas que passaram no filtro."),
                "O WHERE roda ANTES do COUNT: a contagem só enxerga as linhas que passaram no filtro."));
            list.Add(SqlL(0, "x10", "state", "A coxinha saiu do cardápio",
                "A coxinha foi descontinuada e a Glória pediu para tirá-la do cadastro de produtos.",
                "Apague da tabela produtos o produto com nome 'Coxinha' (só ele).",
                "DELETE FROM produtos WHERE nome = 'Coxinha';",
                A("Para apagar linhas, use DELETE FROM ... WHERE. Sem WHERE, você apagaria o cardápio inteiro!", "DELETE FROM produtos WHERE nome = ...;"),
                A("DELETE FROM produtos", "Apaga linhas da tabela produtos.", "WHERE nome = 'Coxinha'", "Só a linha da coxinha (também valeria WHERE id = 8)."),
                "Antes de um DELETE, rode SELECT * FROM produtos WHERE nome = 'Coxinha'; para ver exatamente o que vai sumir.",
                false, A("produtos")));
            list.Add(SqlL(0, "x11", "query", "Preço médio do cardápio",
                "O Fábio quer comparar o preço médio do Café 365 com o da concorrência.",
                "Mostre a média de preço (coluna preco) de todos os produtos.",
                "SELECT AVG(preco) FROM produtos;",
                A("A função de média em SQL tem três letras (vem do inglês 'average').", "SELECT AVG(...) FROM produtos;"),
                A("AVG(preco)", "Soma todos os preços e divide pela quantidade de produtos (6.80).", "FROM produtos", "Usa todas as linhas da tabela."),
                "AVG ignora os NULL: a média é calculada só com as linhas que têm valor."));
            list.Add(SqlL(0, "x12", "query", "Recife ou sem e-mail",
                "A Tânia vai ligar para quem não recebe a newsletter: os clientes de Recife (o provedor de lá está bloqueando) e quem nem tem e-mail cadastrado.",
                "Liste o nome dos clientes que moram em 'Recife' OU que não têm e-mail cadastrado (email NULL).",
                "SELECT nome FROM clientes WHERE cidade = 'Recife' OR email IS NULL;",
                A("Basta UMA das condições ser verdadeira: junte-as com OR. E lembre: NULL se testa com IS NULL.", "SELECT nome FROM clientes WHERE cidade = 'Recife' OR email IS NULL;"),
                A("cidade = 'Recife'", "Primeira condição.", "OR", "Basta uma das condições ser verdadeira.", "email IS NULL", "Clientes sem e-mail (nunca use = NULL)."),
                "Misturou AND e OR na mesma condição? Use parênteses: o AND é avaliado antes do OR, como a multiplicação antes da soma."));
            list.Add(SqlL(0, "x13", "query", "Nomes em maiúsculas",
                "A gráfica vai imprimir os cartões de fidelidade e pediu os nomes dos clientes em letras maiúsculas.",
                "Liste o nome de todos os clientes em letras maiúsculas.",
                "SELECT UPPER(nome) FROM clientes;",
                A("Existe uma função de texto que converte para maiúsculas (em inglês, 'upper case').", "SELECT UPPER(...) FROM clientes;"),
                A("UPPER(nome)", "Devolve o texto em maiúsculas (o contrário é LOWER).", "FROM clientes", "Para cada cliente."),
                "Funções no SELECT mudam só o que é EXIBIDO: os dados gravados na tabela continuam iguais."));
            list.Add(SqlL(0, "x14", "state", "Cliente novo",
                "A Juliana Freitas se cadastrou hoje no balcão e o app está fora do ar. A Tânia pediu para você inserir o cadastro direto no banco.",
                "Cadastre na tabela clientes: nome 'Juliana Freitas', email 'juliana.freitas@email.com', cidade 'Salvador' e criado_em '2026-10-07' (o id é automático).",
                "INSERT INTO clientes (nome, email, cidade, criado_em) VALUES ('Juliana Freitas', 'juliana.freitas@email.com', 'Salvador', '2026-10-07');",
                A("Use INSERT INTO com a lista de colunas e, depois de VALUES, os valores na mesma ordem.", "INSERT INTO clientes (nome, email, cidade, criado_em) VALUES ('Juliana Freitas', ...);"),
                A("INSERT INTO clientes (nome, email, cidade, criado_em)", "As colunas que vão receber valores (o id fica de fora: é AUTO_INCREMENT).",
                  "VALUES (..., '2026-10-07')", "Textos e datas entre aspas; a data no formato 'AAAA-MM-DD'."),
                "Datas no MySQL usam o formato 'AAAA-MM-DD'. Escrever '07/10/2026' dá o erro 1292 (Incorrect date value).",
                false, A("clientes")));
            list.Add(SqlL(0, "x15", "query", "Pedidos a partir do dia 5",
                "O sistema de pagamentos ficou instável a partir de 5 de setembro de 2026 e o Fábio quer conferir os pedidos desde então.",
                "Liste o id, o total e a data dos pedidos com data a partir de '2026-09-05' (o dia 5 entra).",
                "SELECT id, total, data FROM pedidos WHERE data >= '2026-09-05';",
                A("Datas também podem ser comparadas com >, >=, < e <=. A data vai entre aspas, no formato 'AAAA-MM-DD'.", "SELECT id, total, data FROM pedidos WHERE data >= '2026-09-05';"),
                A("WHERE data >= '2026-09-05'", "Do dia 5 em diante (>= inclui o próprio dia).", "'AAAA-MM-DD'", "Nesse formato, a ordem do texto é a mesma do calendário."),
                "Em colunas DATETIME, '2026-09-05' vale 00:00:00 daquele dia: para pegar o dia inteiro use >= '2026-09-05' AND < '2026-09-06'."));
            list.Add(SqlL(0, "x16", "query", "Estoque total",
                "A seguradora do café pede a quantidade total de itens guardados no estoque.",
                "Mostre a soma da coluna estoque de todos os produtos.",
                "SELECT SUM(estoque) FROM produtos;",
                A("Existe uma função de agregação que soma os valores de uma coluna.", "SELECT SUM(...) FROM produtos;"),
                A("SUM(estoque)", "Soma o estoque de todas as linhas (685 itens).", "FROM produtos", "Todos os produtos."),
                "COUNT conta LINHAS; SUM soma VALORES. Quantos produtos existem é COUNT(*); quantas unidades há no estoque é SUM(estoque)."));
            list.Add(SqlL(0, "x17", "query", "Colunas com apelido",
                "O app novo do Café 365 lê o cardápio em JSON e espera os campos produto e valor, não nome e preco.",
                "Liste o nome e o preço de todos os produtos, com a coluna nome aparecendo como produto e a coluna preco aparecendo como valor.",
                "SELECT nome AS produto, preco AS valor FROM produtos;",
                A("Para dar outro nome a uma coluna no resultado, escreva AS depois dela.", "SELECT nome AS ..., preco AS ... FROM produtos;"),
                A("nome AS produto", "A coluna nome aparece no resultado como produto.", "preco AS valor", "A coluna preco aparece como valor."),
                "O apelido só muda o cabeçalho do resultado: a tabela continua com as colunas nome e preco."));
            Cabecalhos("x17");
            list.Add(SqlL(0, "x18", "state", "O e-mail que faltava",
                "O Paulo Nunes (id 6) finalmente informou o e-mail dele no balcão.",
                "Na tabela clientes, preencha o email do cliente de id 6 com 'paulo.nunes@email.com', sem mexer nos outros clientes.",
                "UPDATE clientes SET email = 'paulo.nunes@email.com' WHERE id = 6;",
                A("Use UPDATE ... SET ... e não esqueça o WHERE para mudar um cliente só.", "UPDATE clientes SET email = '...' WHERE id = 6;"),
                A("UPDATE clientes", "Altera a tabela clientes.", "SET email = 'paulo.nunes@email.com'", "Novo valor da coluna email.",
                  "WHERE id = 6", "Só o cliente 6. Filtrar pela chave primária é o jeito mais seguro."),
                "Prefira filtrar o UPDATE pela chave primária (id): nomes podem se repetir; o id, nunca.",
                false, A("clientes")));
            list.Add(SqlL(0, "x19", "query", "Preço com desconto",
                "Na Black Friday todo o cardápio terá 10% de desconto. O marketing quer ver os preços promocionais SEM mudar nada no banco.",
                "Liste o nome de cada produto e o preço com 10% de desconto (preço vezes 0.90), sem alterar a tabela.",
                "SELECT nome, preco * 0.90 FROM produtos;",
                A("No SELECT você pode fazer contas com as colunas.", "SELECT nome, preco * 0.90 FROM produtos;"),
                A("preco * 0.90", "Calcula 90% do preço de cada linha (só na exibição).", "FROM produtos", "Nenhum dado é alterado: é só uma consulta."),
                "Contas no SELECT não mudam o banco. Para gravar o desconto de verdade seria preciso um UPDATE."));
            list.Add(SqlL(0, "x20", "query", "Salário a partir de 3 mil",
                "O RH do Café 365 vai revisar o plano de cargos e quer ver quem ganha 3 mil reais ou mais. A equipe do café fica na tabela funcionarios.",
                "Liste o nome e o salário dos funcionários com salário MAIOR OU IGUAL a 3000.00.",
                "SELECT nome, salario FROM funcionarios WHERE salario >= 3000;",
                A("'A partir de' inclui o próprio valor. Qual operador faz isso? (As colunas da tabela aparecem com DESCRIBE funcionarios;)", "SELECT nome, salario FROM funcionarios WHERE salario >= 3000;"),
                A("FROM funcionarios", "A tabela da equipe do café.", "salario >= 3000", "Maior OU igual: quem ganha exatamente 3000.00 também entra."),
                "Com > 3000, a Lúcia (exatamente 3000.00) ficaria de fora. Leia o pedido com atenção: 'acima de' é >, 'a partir de' é >=."));
            list.Add(SqlL(0, "x21", "query", "Ordenando por dois critérios",
                "O cardápio impresso agrupa os produtos por categoria e, dentro de cada uma, mostra do mais barato para o mais caro.",
                "Liste categoria, nome e preço de todos os produtos, ordenados pela categoria (A→Z) e, dentro da mesma categoria, pelo preço do menor para o maior.",
                "SELECT categoria, nome, preco FROM produtos ORDER BY categoria, preco;",
                A("O ORDER BY aceita várias colunas separadas por vírgula: a segunda só desempata a primeira.", "SELECT categoria, nome, preco FROM produtos ORDER BY categoria, preco;"),
                A("ORDER BY categoria", "Primeiro critério: as categorias em ordem alfabética.", ", preco", "Segundo critério: dentro da mesma categoria, do menor preço para o maior."),
                "Cada coluna do ORDER BY tem sua própria direção: ORDER BY categoria ASC, preco DESC também é válido.",
                true));
            list.Add(SqlL(0, "x22", "query", "Etiqueta do cardápio",
                "A impressora de etiquetas do balcão recebe UM texto por produto, no formato 'Café expresso - R$ 6.50'.",
                "Liste, para cada produto, um único texto juntando o nome, o trecho ' - R$ ' e o preço (exemplo: Café expresso - R$ 6.50).",
                "SELECT CONCAT(nome, ' - R$ ', preco) FROM produtos;",
                A("Existe uma função que junta (concatena) vários textos num só.", "SELECT CONCAT(nome, ' - R$ ', preco) FROM produtos;"),
                A("CONCAT(...)", "Junta os pedaços, na ordem, num texto só.", "' - R$ '", "Texto fixo no meio (repare nos espaços dentro das aspas).", "preco", "O número vira texto automaticamente."),
                "Se qualquer pedaço do CONCAT for NULL, o resultado inteiro vira NULL. CONCAT_WS(separador, ...) pula os NULL."));
            list.Add(SqlL(0, "x23", "query", "Estoque médio arredondado",
                "O relatório mensal da Glória mostra o estoque médio com uma casa decimal só.",
                "Mostre a média da coluna estoque de todos os produtos, arredondada para 1 casa decimal.",
                "SELECT ROUND(AVG(estoque), 1) FROM produtos;",
                A("Calcule a média com AVG e passe o resultado para a função de arredondar, dizendo quantas casas você quer.", "SELECT ROUND(AVG(estoque), 1) FROM produtos;"),
                A("AVG(estoque)", "A média: 85.625.", "ROUND(..., 1)", "Arredonda para 1 casa decimal: 85.6."),
                "ROUND(x) sem o segundo número arredonda para inteiro; ROUND(x, 2) é o mais comum para dinheiro."));
            list.Add(SqlL(0, "x24", "schema", "Tabela de fornecedores",
                "O café vai cadastrar os fornecedores de grãos e de leite. A Glória pediu uma tabela simples para começar.",
                "Crie a tabela fornecedores com: id (INT, chave primária, auto incremento), nome (VARCHAR(100), obrigatório: NOT NULL) e telefone (VARCHAR(20), opcional).",
                "CREATE TABLE fornecedores (id INT PRIMARY KEY AUTO_INCREMENT, nome VARCHAR(100) NOT NULL, telefone VARCHAR(20));",
                A("CREATE TABLE nome (coluna TIPO restrições, ...). Texto usa VARCHAR(tamanho máximo).",
                  "CREATE TABLE fornecedores (id INT PRIMARY KEY AUTO_INCREMENT, nome VARCHAR(100) NOT NULL, telefone VARCHAR(20));"),
                A("id INT PRIMARY KEY AUTO_INCREMENT", "Chave primária numerada sozinha.", "nome VARCHAR(100) NOT NULL", "Texto de até 100 caracteres, obrigatório.",
                  "telefone VARCHAR(20)", "Sem NOT NULL: pode ficar vazio (NULL)."),
                "Telefone é texto (VARCHAR), não número: tem zero à esquerda, parênteses e traço, e ninguém faz conta com ele.",
                false, null, ExtraSql_CheckFornecedores));

            // ═════════════════════════════ Intermediário ═════════════════════════════
            list.Add(SqlL(1, "x25", "query", "Pedidos por status",
                "O Fábio está montando o painel de vendas e quer, para cada status, quantos pedidos existem e quanto eles somam.",
                "Para cada status da tabela pedidos, mostre o status, a quantidade de pedidos e a soma do total.",
                "SELECT status, COUNT(*), SUM(total) FROM pedidos GROUP BY status;",
                A("Agrupe por status e use duas agregações no mesmo SELECT.", "SELECT status, COUNT(*), SUM(total) FROM pedidos GROUP BY status;"),
                A("GROUP BY status", "Uma linha por status (entregue, cancelado, pendente).", "COUNT(*)", "Quantos pedidos há em cada grupo.", "SUM(total)", "A soma dos valores de cada grupo."),
                "Um GROUP BY pode calcular várias agregações de uma vez: COUNT, SUM, AVG, MIN e MAX lado a lado."));
            list.Add(SqlL(1, "x26", "query", "Segunda página do cardápio",
                "O app mostra o cardápio em páginas de 3 produtos, em ordem alfabética. Ao tocar em 'próxima', ele pede a segunda página.",
                "Liste nome e preço dos produtos em ordem alfabética de nome, pulando os 3 primeiros e mostrando os 3 seguintes (a 2ª página).",
                "SELECT nome, preco FROM produtos ORDER BY nome LIMIT 3 OFFSET 3;",
                A("Ordene primeiro. Depois, LIMIT diz quantas linhas mostrar e OFFSET quantas pular.", "SELECT nome, preco FROM produtos ORDER BY nome LIMIT 3 OFFSET 3;"),
                A("ORDER BY nome", "Sem uma ordem fixa, as páginas poderiam repetir ou pular produtos.", "LIMIT 3", "Mostra 3 linhas.",
                  "OFFSET 3", "Pula as 3 primeiras (a página 1). Atalho do MySQL: LIMIT 3, 3 (primeiro o pulo, depois a quantidade)."),
                "Página n (começando em 1) com t itens: LIMIT t OFFSET (n - 1) * t. Em tabelas enormes, um OFFSET muito grande fica lento.",
                true));
            list.Add(SqlL(1, "x27", "query", "Contato do cliente",
                "A planilha de contatos da Tânia não pode ter células vazias: quem não tem e-mail deve aparecer como 'sem e-mail'.",
                "Liste o nome de cada cliente e uma coluna chamada contato com o e-mail; quando o e-mail for NULL, mostre o texto 'sem e-mail'.",
                "SELECT nome, COALESCE(email, 'sem e-mail') AS contato FROM clientes;",
                A("Existe uma função que devolve o primeiro valor que NÃO é NULL de uma lista de argumentos.", "SELECT nome, COALESCE(email, 'sem e-mail') AS contato FROM clientes;"),
                A("COALESCE(email, 'sem e-mail')", "Se email tiver valor, usa o email; se for NULL, usa o texto.", "AS contato", "Nome da coluna no resultado.",
                  "IFNULL(email, 'sem e-mail')", "Forma equivalente do MySQL (só com dois argumentos)."),
                "COALESCE aceita vários argumentos: COALESCE(celular, telefone, email, 'sem contato') pega o primeiro que existir."));
            Cabecalhos("x27");
            list.Add(SqlL(1, "x28", "state", "Pedido com status padrão",
                "Todo pedido novo nasce 'pendente', e quem garante isso é o DEFAULT da coluna status. A Glória quer que você confie no banco e não digite o status.",
                "Insira um pedido para o cliente_id 6, com total 15.00 e data '2026-10-07', informando SÓ essas três colunas (o status deve vir do valor padrão).",
                "INSERT INTO pedidos (cliente_id, total, data) VALUES (6, 15.00, '2026-10-07');",
                A("Uma coluna que fica fora da lista do INSERT recebe o seu DEFAULT. Veja com DESCRIBE pedidos; qual é o padrão do status.",
                  "INSERT INTO pedidos (cliente_id, total, data) VALUES (6, 15.00, '2026-10-07');"),
                A("(cliente_id, total, data)", "Só as colunas informadas.", "status", "Ficou de fora: recebe o DEFAULT 'pendente'.", "id", "AUTO_INCREMENT: o banco numera sozinho (vira 11)."),
                "Também dá para pedir o padrão de forma explícita, com a palavra DEFAULT no lugar do valor: ... (cliente_id, total, status, data) VALUES (6, 15.00, DEFAULT, '2026-10-07');",
                false, A("pedidos")));
            list.Add(SqlL(1, "x29", "query", "Pedidos de São Paulo",
                "O café vai abrir uma loja física em São Paulo e o Fábio quer ver os pedidos dos clientes de lá. A cidade fica em clientes; o pedido, em pedidos.",
                "Liste o id e o total dos pedidos feitos por clientes da cidade 'São Paulo'.",
                "SELECT p.id, p.total FROM pedidos p JOIN clientes c ON c.id = p.cliente_id WHERE c.cidade = 'São Paulo';",
                A("Junte pedidos e clientes pela chave (pedidos.cliente_id = clientes.id) e depois filtre pela cidade.",
                  "SELECT p.id, p.total FROM pedidos p JOIN clientes c ON c.id = p.cliente_id WHERE c.cidade = 'São Paulo';"),
                A("JOIN clientes c ON c.id = p.cliente_id", "Cada pedido ganha os dados do seu cliente.", "WHERE c.cidade = 'São Paulo'", "Filtra por uma coluna que veio da outra tabela.",
                  "p.id", "As duas tabelas têm id: diga de qual é (senão, erro 1052: coluna ambígua)."),
                "Também funciona com subconsulta: ... FROM pedidos WHERE cliente_id IN (SELECT id FROM clientes WHERE cidade = 'São Paulo')."));
            list.Add(SqlL(1, "x30", "query", "Valor parado no estoque",
                "A Glória quer saber quanto dinheiro está 'parado' em cada produto do estoque (preço vezes quantidade), do maior para o menor.",
                "Liste o nome de cada produto e uma coluna chamada valor_estoque (preco * estoque), ordenando do maior valor_estoque para o menor.",
                "SELECT nome, preco * estoque AS valor_estoque FROM produtos ORDER BY valor_estoque DESC;",
                A("Faça a conta no SELECT, dê um apelido a ela e use o apelido no ORDER BY.", "SELECT nome, preco * estoque AS valor_estoque FROM produtos ORDER BY valor_estoque DESC;"),
                A("preco * estoque", "Conta feita linha a linha.", "AS valor_estoque", "Nome da coluna calculada.", "ORDER BY valor_estoque DESC", "O ORDER BY enxerga os apelidos do SELECT (o WHERE não!)."),
                "O WHERE roda antes do SELECT, por isso WHERE valor_estoque > 500 dá o erro 1054. Nele, repita a conta: WHERE preco * estoque > 500.",
                true));
            Cabecalhos("x30");
            list.Add(SqlL(1, "x31", "state", "Cancelar pendências antigas",
                "Pedido pendente há muito tempo é pedido abandonado. A regra da Glória: pendentes com data ANTERIOR a '2026-09-05' viram 'cancelado'.",
                "Mude para 'cancelado' o status dos pedidos que estão 'pendente' E têm data anterior a '2026-09-05'.",
                "UPDATE pedidos SET status = 'cancelado' WHERE status = 'pendente' AND data < '2026-09-05';",
                A("O WHERE precisa das duas condições juntas (AND). Ensaie antes com um SELECT usando o mesmo WHERE.",
                  "UPDATE pedidos SET status = 'cancelado' WHERE status = 'pendente' AND data < '2026-09-05';"),
                A("SET status = 'cancelado'", "Novo status.", "status = 'pendente'", "Só os pendentes (um pedido entregue não pode virar cancelado!).", "AND data < '2026-09-05'", "E só os anteriores ao dia 5."),
                "Ensaie todo UPDATE: SELECT * FROM pedidos WHERE status = 'pendente' AND data < '2026-09-05'; mostra exatamente quais linhas vão mudar.",
                false, A("pedidos")));
            list.Add(SqlL(1, "x32", "query", "Cadastros por mês",
                "O marketing quer ver em quais meses de 2026 entraram mais clientes.",
                "Mostre o número do mês de cadastro (MONTH de criado_em) e quantos clientes se cadastraram em cada mês.",
                "SELECT MONTH(criado_em), COUNT(*) FROM clientes GROUP BY MONTH(criado_em);",
                A("MONTH(data) devolve o número do mês. Dá para agrupar pelo resultado de uma função.", "SELECT MONTH(criado_em), COUNT(*) FROM clientes GROUP BY MONTH(criado_em);"),
                A("MONTH(criado_em)", "Extrai o mês (1 a 12) da data.", "GROUP BY MONTH(criado_em)", "Um grupo por mês (também vale um apelido: MONTH(criado_em) AS mes ... GROUP BY mes).",
                  "COUNT(*)", "Clientes em cada mês."),
                "Com dados de vários anos, agrupe por ano E mês (GROUP BY YEAR(criado_em), MONTH(criado_em)): senão janeiro de 2025 e de 2026 se misturam."));
            list.Add(SqlL(1, "x33", "query", "Tem estoque?",
                "A tela do app mostra 'sim' ou 'não' ao lado de cada produto, conforme ele tenha estoque.",
                "Liste o nome de cada produto e uma coluna chamada disponivel com 'sim' quando o estoque for maior que 0 e 'não' caso contrário.",
                "SELECT nome, IF(estoque > 0, 'sim', 'não') AS disponivel FROM produtos;",
                A("Para escolher entre DUAS saídas, o MySQL tem a função IF(condição, valor_se_verdadeiro, valor_se_falso).", "SELECT nome, IF(estoque > 0, 'sim', 'não') AS disponivel FROM produtos;"),
                A("IF(estoque > 0, 'sim', 'não')", "Condição verdadeira: 'sim'; falsa: 'não'.", "AS disponivel", "Nome da coluna.",
                  "CASE WHEN estoque > 0 THEN 'sim' ELSE 'não' END", "Forma do padrão SQL, que funciona em qualquer banco."),
                "IF é do MySQL; CASE WHEN é padrão SQL (funciona também no PostgreSQL e no SQL Server). Com várias faixas, CASE fica mais legível."));
            Cabecalhos("x33");
            list.Add(SqlL(1, "x34", "schema", "Tabela de cupons",
                "O marketing vai lançar cupons de desconto. O código do cupom (ex.: 'CAFE10') já é único, então a Glória quer que ele seja a própria chave primária.",
                "Crie a tabela cupons com: codigo (VARCHAR(20), chave primária), desconto (DECIMAL(5,2), NOT NULL) e validade (DATE, NOT NULL).",
                "CREATE TABLE cupons (codigo VARCHAR(20) PRIMARY KEY, desconto DECIMAL(5,2) NOT NULL, validade DATE NOT NULL);",
                A("A chave primária não precisa ser um número com AUTO_INCREMENT: um texto único também serve. Valores com centavos usam DECIMAL.",
                  "CREATE TABLE cupons (codigo VARCHAR(20) PRIMARY KEY, desconto DECIMAL(5,2) NOT NULL, validade DATE NOT NULL);"),
                A("codigo VARCHAR(20) PRIMARY KEY", "Chave 'natural': o próprio código identifica o cupom.", "desconto DECIMAL(5,2)", "Até 999.99, com 2 casas exatas.",
                  "validade DATE NOT NULL", "Data obrigatória ('AAAA-MM-DD')."),
                "Dinheiro e porcentagem: use DECIMAL, nunca FLOAT. FLOAT guarda valores aproximados (0.1 + 0.2 vira 0.30000000000000004).",
                false, null, ExtraSql_CheckCupons));
            list.Add(SqlL(1, "x35", "query", "Média por categoria arredondada",
                "No relatório do Fábio as médias de preço aparecem com 4 casas (8.1333) e a diretoria reclamou.",
                "Para cada categoria, mostre a categoria e a média de preço arredondada para 1 casa decimal.",
                "SELECT categoria, ROUND(AVG(preco), 1) FROM produtos GROUP BY categoria;",
                A("Coloque a agregação DENTRO da função de arredondar e agrupe por categoria.", "SELECT categoria, ROUND(AVG(preco), 1) FROM produtos GROUP BY categoria;"),
                A("AVG(preco)", "Média de cada grupo (bebidas: 8.1333).", "ROUND(..., 1)", "Arredonda para 1 casa: 8.1.", "GROUP BY categoria", "Uma linha por categoria."),
                "Arredonde só na apresentação: guardar valores já arredondados acumula erro nas contas seguintes."));
            list.Add(SqlL(1, "x36", "query", "Clientes com pedido pendente",
                "A Tânia vai ligar para quem tem pedido parado em 'pendente'.",
                "Liste o nome dos clientes que têm pelo menos um pedido com status 'pendente' (cada cliente uma vez só).",
                "SELECT nome FROM clientes WHERE id IN (SELECT cliente_id FROM pedidos WHERE status = 'pendente');",
                A("Primeiro descubra QUAIS cliente_id têm pedido pendente (uma consulta em pedidos). Depois use essa lista no WHERE de clientes.",
                  "SELECT nome FROM clientes WHERE id IN (SELECT cliente_id FROM pedidos WHERE status = 'pendente');"),
                A("(SELECT cliente_id FROM pedidos WHERE status = 'pendente')", "Subconsulta: a lista de clientes com pedido pendente (1 e 8).",
                  "WHERE id IN (...)", "Fica o cliente cujo id está nessa lista."),
                "Com JOIN também dá, mas um cliente com dois pendentes apareceria duas vezes (precisaria de DISTINCT). O IN nunca duplica."));
            list.Add(SqlL(1, "x37", "state", "Liquidação do estoque baixo",
                "Produtos com pouco estoque (mais de 0 e menos de 50 unidades) vão entrar em liquidação com 20% de desconto. Os esgotados não entram.",
                "Com um único UPDATE, multiplique por 0.80 o preço dos produtos com estoque maior que 0 e menor que 50.",
                "UPDATE produtos SET preco = preco * 0.80 WHERE estoque > 0 AND estoque < 50;",
                A("O novo preço é calculado a partir do preço atual. O WHERE precisa de duas condições sobre a mesma coluna.", "UPDATE produtos SET preco = preco * 0.80 WHERE estoque > 0 AND estoque < 50;"),
                A("SET preco = preco * 0.80", "20% de desconto sobre o preço atual de cada linha.", "estoque > 0 AND estoque < 50",
                  "Tira os esgotados (0) e quem tem 50 ou mais. Também vale BETWEEN 1 AND 49."),
                "A coluna preco é DECIMAL(10,2): o banco arredonda o resultado para 2 casas ao gravar (8.50 × 0.80 = 6.80).",
                false, A("produtos")));
            list.Add(SqlL(1, "x38", "query", "Nomes longos",
                "A etiqueta da vitrine só comporta 13 caracteres. A Glória quer a lista dos produtos com nome MAIOR que isso, para abreviar.",
                "Liste o nome dos produtos cujo nome tem mais de 13 caracteres (conte caracteres, não bytes).",
                "SELECT nome FROM produtos WHERE CHAR_LENGTH(nome) > 13;",
                A("No MySQL, LENGTH conta BYTES: em UTF-8, letras acentuadas como 'ã' e 'é' ocupam 2. Existe outra função que conta caracteres.",
                  "SELECT nome FROM produtos WHERE CHAR_LENGTH(nome) > 13;"),
                A("CHAR_LENGTH(nome)", "Quantidade de caracteres ('Pão de queijo' = 13).", "LENGTH(nome)", "Quantidade de BYTES ('Pão de queijo' = 14, por causa do 'ã')."),
                "Use CHAR_LENGTH para validar o tamanho de textos digitados por pessoas; LENGTH só quando importa o espaço ocupado em bytes."));
            list.Add(SqlL(1, "x39", "schema", "Renomeando uma coluna",
                "A coluna data da tabela pedidos confunde todo mundo (data de quê?). A Glória decidiu: ela passa a se chamar data_pedido.",
                "Renomeie a coluna data da tabela pedidos para data_pedido, sem perder os dados.",
                "ALTER TABLE pedidos RENAME COLUMN data TO data_pedido;",
                A("Mudança de estrutura é com ALTER TABLE. No MySQL 8 existe RENAME COLUMN antigo TO novo.", "ALTER TABLE pedidos RENAME COLUMN data TO data_pedido;"),
                A("ALTER TABLE pedidos", "Altera a estrutura da tabela pedidos.", "RENAME COLUMN data TO data_pedido", "Troca só o nome: o tipo e os dados continuam iguais."),
                "Renomear coluna quebra toda consulta antiga que usa o nome velho: em produção, combine com a equipe e atualize o código junto.",
                false, null, ExtraSql_CheckRenomear));
            list.Add(SqlL(1, "x40", "query", "Categorias caras",
                "O Fábio quer saber quais categorias têm preço médio acima de 6 reais, para revisar a margem de lucro.",
                "Mostre a categoria e a média de preço apenas das categorias cuja média é maior que 6.00.",
                "SELECT categoria, AVG(preco) FROM produtos GROUP BY categoria HAVING AVG(preco) > 6;",
                A("O filtro é sobre a MÉDIA de cada grupo, não sobre cada linha: o WHERE não serve aqui.", "SELECT categoria, AVG(preco) FROM produtos GROUP BY categoria HAVING AVG(preco) > 6;"),
                A("GROUP BY categoria", "Uma linha por categoria.", "HAVING AVG(preco) > 6", "Filtra os grupos depois de calcular a média (doce, com 5.25, sai)."),
                "WHERE preco > 6 seria outra pergunta: tiraria os produtos baratos ANTES de calcular a média, mudando o resultado."));
            list.Add(SqlL(1, "x41", "query", "Saudação personalizada",
                "O app vai mandar uma notificação 'Olá, Ana!' usando só o primeiro nome de cada cliente.",
                "Liste, para cada cliente, uma coluna chamada saudacao com o texto 'Olá, ' + o primeiro nome (tudo antes do primeiro espaço) + '!' — exemplo: Olá, Ana!",
                "SELECT CONCAT('Olá, ', SUBSTRING_INDEX(nome, ' ', 1), '!') AS saudacao FROM clientes;",
                A("SUBSTRING_INDEX(texto, separador, n) devolve o que vem antes da n-ésima ocorrência do separador. Depois é só juntar tudo com CONCAT.",
                  "SELECT CONCAT('Olá, ', SUBSTRING_INDEX(nome, ' ', 1), '!') AS saudacao FROM clientes;"),
                A("SUBSTRING_INDEX(nome, ' ', 1)", "Tudo antes do 1º espaço: 'Ana Souza' vira 'Ana'.", "CONCAT('Olá, ', ..., '!')", "Monta o texto final.", "AS saudacao", "Nome da coluna."),
                "Com n negativo, SUBSTRING_INDEX conta do fim: SUBSTRING_INDEX(nome, ' ', -1) devolve o último sobrenome."));
            Cabecalhos("x41");
            list.Add(SqlL(1, "x42", "query", "Produto mais caro",
                "A vitrine vai destacar o produto mais caro. O Fábio avisou: se dois produtos empatarem no preço máximo, os dois devem aparecer.",
                "Liste o nome e o preço do(s) produto(s) cujo preço é igual ao MAIOR preço da tabela, calculando o maior preço com uma subconsulta.",
                "SELECT nome, preco FROM produtos WHERE preco = (SELECT MAX(preco) FROM produtos);",
                A("Uma subconsulta entre parênteses pode calcular o MAX e ser comparada no WHERE.", "SELECT nome, preco FROM produtos WHERE preco = (SELECT MAX(preco) FROM produtos);"),
                A("(SELECT MAX(preco) FROM produtos)", "Calcula o maior preço (9.90).", "WHERE preco = (...)", "Traz TODOS os produtos com esse preço, inclusive empates."),
                "ORDER BY preco DESC LIMIT 1 parece igual, mas num empate mostra só um dos produtos, escolhido sem critério."));
            list.Add(SqlL(1, "x43", "schema", "Sem produto repetido",
                "Alguém cadastrou 'Cappuccino' duas vezes no sistema antigo e o estoque virou uma confusão. A Glória quer que o banco impeça nomes repetidos em produtos.",
                "Crie um índice ÚNICO chamado idx_produtos_nome na coluna nome da tabela produtos.",
                "CREATE UNIQUE INDEX idx_produtos_nome ON produtos (nome);",
                A("É o mesmo comando de criar índice, com uma palavra a mais que proíbe valores repetidos.", "CREATE UNIQUE INDEX idx_produtos_nome ON produtos (nome);"),
                A("CREATE UNIQUE INDEX", "Índice que não aceita valores repetidos.", "idx_produtos_nome", "Nome do índice.", "ON produtos (nome)", "Tabela e coluna."),
                "Teste: INSERT INTO produtos (nome, categoria, preco) VALUES ('Cappuccino', 'bebida', 1); deve dar o erro 1062 (Duplicate entry). Se já houver repetidos, o índice nem é criado.",
                false, null, ExtraSql_CheckIndiceUnico));
            list.Add(SqlL(1, "x44", "query", "Clientes por cidade",
                "O Fábio quer um ranking de cidades pelo número de clientes; nos empates, ordem alfabética.",
                "Mostre cada cidade e a quantidade de clientes dela, da cidade com mais clientes para a com menos; em caso de empate, em ordem alfabética de cidade.",
                "SELECT cidade, COUNT(*) FROM clientes GROUP BY cidade ORDER BY COUNT(*) DESC, cidade;",
                A("Agrupe por cidade, conte e ordene por dois critérios: a contagem (decrescente) e depois a cidade.", "SELECT cidade, COUNT(*) FROM clientes GROUP BY cidade ORDER BY COUNT(*) DESC, cidade;"),
                A("GROUP BY cidade", "Uma linha por cidade.", "ORDER BY COUNT(*) DESC", "Mais clientes primeiro.", ", cidade", "Desempate em ordem alfabética."),
                "Sem critério de desempate, a ordem entre as cidades empatadas pode mudar de uma execução para outra.",
                true));
            list.Add(SqlL(1, "x45", "query", "Dias de casa",
                "O programa de fidelidade dá brindes por tempo de cadastro. 'Hoje' é a data do servidor: CURDATE().",
                "Liste o nome de cada cliente e há quantos dias ele se cadastrou (diferença em dias entre CURDATE() e criado_em).",
                "SELECT nome, DATEDIFF(CURDATE(), criado_em) FROM clientes;",
                A("DATEDIFF(data_maior, data_menor) devolve a diferença em dias. A data de hoje vem de CURDATE().", "SELECT nome, DATEDIFF(CURDATE(), criado_em) FROM clientes;"),
                A("CURDATE()", "A data de hoje no servidor (neste treino, 2026-10-07).", "DATEDIFF(CURDATE(), criado_em)", "Dias entre o cadastro e hoje. Invertendo a ordem, o número sai negativo."),
                "Para diferenças em horas ou minutos, use TIMESTAMPDIFF(HOUR, inicio, fim)."));
            list.Add(SqlL(1, "x46", "schema", "Equipe sem salário à mostra",
                "O mural da recepção vai mostrar a equipe do café, mas salário é dado sensível. A Glória quer uma VIEW que exponha só o nome e o cargo.",
                "Crie a view vw_equipe mostrando apenas as colunas nome e cargo de todos os funcionários.",
                "CREATE VIEW vw_equipe AS SELECT nome, cargo FROM funcionarios;",
                A("Uma view é uma consulta guardada com nome: CREATE VIEW nome AS SELECT ...", "CREATE VIEW vw_equipe AS SELECT nome, cargo FROM funcionarios;"),
                A("CREATE VIEW vw_equipe", "Cria a view (uma 'tabela virtual').", "AS SELECT nome, cargo FROM funcionarios", "A consulta que a view executa sempre que alguém a lê."),
                "Com a view, o sistema do mural ganha permissão só nela (GRANT SELECT ON vw_equipe ...), sem enxergar os salários. Teste: SELECT * FROM vw_equipe;",
                false, null, ExtraSql_CheckVwEquipe));
            list.Add(SqlL(1, "x47", "schema", "Endereços dos clientes",
                "O delivery vai guardar o CEP de entrega de cada cliente numa tabela própria, e o banco precisa impedir CEP de cliente que não existe.",
                "Crie a tabela enderecos com: id (INT, chave primária, auto incremento), cliente_id (INT, NOT NULL, chave estrangeira para clientes.id) e cep (VARCHAR(9), NOT NULL).",
                "CREATE TABLE enderecos (id INT PRIMARY KEY AUTO_INCREMENT, cliente_id INT NOT NULL, cep VARCHAR(9) NOT NULL, FOREIGN KEY (cliente_id) REFERENCES clientes(id));",
                A("Declare as três colunas e, ainda dentro dos parênteses, a restrição FOREIGN KEY (coluna) REFERENCES tabela(coluna).",
                  "CREATE TABLE enderecos (id INT PRIMARY KEY AUTO_INCREMENT, cliente_id INT NOT NULL, cep VARCHAR(9) NOT NULL, FOREIGN KEY (cliente_id) REFERENCES clientes(id));"),
                A("cliente_id INT NOT NULL", "Todo endereço pertence a um cliente.", "FOREIGN KEY (cliente_id) REFERENCES clientes(id)", "O banco recusa cliente_id que não existe em clientes (erro 1452).",
                  "cep VARCHAR(9)", "CEP é texto: '01310-100' tem zero à esquerda e traço."),
                "Cuidado: no MySQL 8, escrever REFERENCES direto na coluna (cliente_id INT REFERENCES clientes(id)) é aceito, mas a regra é IGNORADA. Use sempre FOREIGN KEY (...) REFERENCES ...",
                false, null, ExtraSql_CheckEnderecos));
            list.Add(SqlL(1, "x48", "state", "Encomenda da festa",
                "A Micorosoft encomendou 30 brigadeiros e 2 bolos de cenoura para uma festa. As duas baixas no estoque precisam acontecer juntas: ou as duas, ou nenhuma.",
                "Numa transação (START TRANSACTION ... COMMIT), diminua em 30 o estoque do 'Brigadeiro' e em 2 o estoque do 'Bolo de cenoura'.",
                "START TRANSACTION; UPDATE produtos SET estoque = estoque - 30 WHERE nome = 'Brigadeiro'; UPDATE produtos SET estoque = estoque - 2 WHERE nome = 'Bolo de cenoura'; COMMIT;",
                A("Abra a transação, faça os dois UPDATE (cada um com seu WHERE) e confirme. Tudo pode ir numa linha só, separado por ponto e vírgula.",
                  "START TRANSACTION; UPDATE produtos SET estoque = estoque - 30 WHERE nome = 'Brigadeiro'; UPDATE produtos SET ... ; COMMIT;"),
                A("START TRANSACTION", "Abre a transação: nada fica definitivo ainda.", "UPDATE ... SET estoque = estoque - 30", "Baixa calculada a partir do valor atual.",
                  "COMMIT", "Confirma as duas alterações juntas. Se algo der errado antes, ROLLBACK desfaz tudo."),
                "Sem transação, se o servidor cair entre os dois UPDATE, o estoque fica pela metade. Transação é tudo ou nada (o 'A' de ACID: atomicidade).",
                false, A("produtos")));
            Transacao("x48");

            // ═════════════════════════════ Profissional ═════════════════════════════
            list.Add(SqlL(2, "x49", "query", "Pedidos de cada cliente",
                "O relatório de fidelidade do Fábio mostra 1 pedido para clientes que nunca compraram. O erro está na contagem feita com LEFT JOIN.",
                "Liste o nome de TODOS os clientes e quantos pedidos cada um fez — quem nunca pediu deve aparecer com 0.",
                "SELECT c.nome, COUNT(p.id) FROM clientes c LEFT JOIN pedidos p ON p.cliente_id = c.id GROUP BY c.id, c.nome;",
                A("LEFT JOIN mantém quem não tem pedido, mas a linha dele vem com p.id NULL. COUNT(*) conta essa linha; COUNT(coluna) ignora NULL.",
                  "SELECT c.nome, COUNT(p.id) FROM clientes c LEFT JOIN pedidos p ON p.cliente_id = c.id GROUP BY c.id, c.nome;"),
                A("LEFT JOIN pedidos p", "Todos os clientes, com ou sem pedido.", "COUNT(p.id)", "Conta só os pedidos de verdade (NULL não conta): quem não pediu fica com 0.",
                  "GROUP BY c.id, c.nome", "Um grupo por cliente."),
                "Regra de ouro: com LEFT JOIN, conte uma coluna da tabela da direita (COUNT(p.id)), nunca COUNT(*)."));
            list.Add(SqlL(2, "x50", "query", "Organograma",
                "O RH quer o organograma: cada funcionário ao lado do nome do seu gerente. A coluna gerente_id aponta para a PRÓPRIA tabela funcionarios; quem não tem gerente responde à diretoria.",
                "Liste o nome de cada funcionário (coluna nome) e o nome do gerente numa coluna chamada gerente; para quem não tem gerente, mostre 'diretoria'. Todos os funcionários devem aparecer.",
                "SELECT f.nome, COALESCE(g.nome, 'diretoria') AS gerente FROM funcionarios f LEFT JOIN funcionarios g ON g.id = f.gerente_id;",
                A("Junte a tabela com ela mesma usando dois apelidos (f = funcionário, g = gerente). Para não perder quem não tem gerente, o JOIN precisa ser LEFT.",
                  "SELECT f.nome, COALESCE(g.nome, 'diretoria') AS gerente FROM funcionarios f LEFT JOIN funcionarios g ON g.id = f.gerente_id;"),
                A("FROM funcionarios f LEFT JOIN funcionarios g", "Auto-relacionamento (self join): a mesma tabela duas vezes, com apelidos diferentes.",
                  "ON g.id = f.gerente_id", "O gerente (g) é a linha cujo id é o gerente_id do funcionário (f).",
                  "COALESCE(g.nome, 'diretoria')", "A Helena não tem gerente: g.nome vem NULL e vira 'diretoria'."),
                "Com JOIN comum, a Helena (gerente_id NULL) sumiria do organograma. Hierarquias (categorias, comentários, pastas) usam sempre esse padrão."));
            Cabecalhos("x50");
            list.Add(SqlL(2, "x51", "query", "Segundo maior preço",
                "Pergunta clássica de entrevista que o Fábio adora fazer: qual é o SEGUNDO maior preço do cardápio?",
                "Mostre o segundo maior preço da tabela produtos (o maior preço que é menor que o máximo), usando MAX e uma subconsulta.",
                "SELECT MAX(preco) FROM produtos WHERE preco < (SELECT MAX(preco) FROM produtos);",
                A("Tire o maior preço da disputa com uma subconsulta no WHERE; o MAX do que sobrar é o segundo.", "SELECT MAX(preco) FROM produtos WHERE preco < (SELECT MAX(preco) FROM produtos);"),
                A("(SELECT MAX(preco) FROM produtos)", "O maior preço (9.90).", "WHERE preco < (...)", "Só os preços abaixo dele.", "MAX(preco)", "O maior dos que sobraram: 8.50."),
                "ORDER BY preco DESC LIMIT 1 OFFSET 1 erra quando o maior preço se repete; a subconsulta (ou SELECT DISTINCT preco ... LIMIT 1 OFFSET 1) não erra."));
            list.Add(SqlL(2, "x52", "schema", "Backup antes de mexer",
                "Amanhã o Fábio vai rodar um reajuste grande nos preços. A Glória exige um backup da tabela produtos ANTES, para poder comparar ou voltar atrás.",
                "Crie a tabela produtos_backup como uma cópia completa (todas as colunas e todas as linhas) da tabela produtos.",
                "CREATE TABLE produtos_backup AS SELECT * FROM produtos;",
                A("Um CREATE TABLE pode nascer do resultado de um SELECT.", "CREATE TABLE produtos_backup AS SELECT * FROM produtos;"),
                A("CREATE TABLE produtos_backup", "Cria a tabela nova...", "AS SELECT * FROM produtos", "...com as colunas e as linhas do resultado do SELECT."),
                "CREATE TABLE ... AS SELECT copia os dados, mas NÃO copia chave primária, AUTO_INCREMENT nem índices. Para clonar a estrutura inteira: CREATE TABLE b LIKE a; e depois INSERT INTO b SELECT * FROM a;",
                false, null, ExtraSql_CheckBackup));
            list.Add(SqlL(2, "x53", "query", "Quantos por faixa",
                "O Fábio quer um gráfico de pizza com quantos produtos há em cada faixa de preço.",
                "Mostre uma coluna faixa ('barato' para preço menor que 6, 'normal' para preço menor que 8, 'premium' para o resto) e uma coluna quantidade com o número de produtos de cada faixa.",
                "SELECT CASE WHEN preco < 6 THEN 'barato' WHEN preco < 8 THEN 'normal' ELSE 'premium' END AS faixa, COUNT(*) AS quantidade FROM produtos GROUP BY faixa;",
                A("Monte a faixa com CASE WHEN, dê a ela o apelido faixa e agrupe por ele (o MySQL aceita agrupar pelo apelido).",
                  "SELECT CASE WHEN preco < 6 THEN 'barato' WHEN preco < 8 THEN 'normal' ELSE 'premium' END AS faixa, COUNT(*) AS quantidade FROM produtos GROUP BY faixa;"),
                A("CASE WHEN ... END AS faixa", "Classifica cada produto (o primeiro WHEN verdadeiro vence).", "GROUP BY faixa", "Agrupa pelo valor calculado (também vale GROUP BY 1).",
                  "COUNT(*) AS quantidade", "Produtos em cada faixa."),
                "Agrupar por uma expressão é como criar uma coluna nova na hora: o mesmo truque serve para faixas etárias, de salário, de horário..."));
            Cabecalhos("x53");
            list.Add(SqlL(2, "x54", "query", "Consertando o produto cartesiano",
                "O relatório do Fábio — SELECT c.nome, p.total FROM clientes c, pedidos p WHERE p.status = 'entregue'; — trouxe 48 linhas, mas só existem 6 pedidos entregues!",
                "Escreva a consulta correta: o nome do cliente e o total de cada pedido com status 'entregue' (uma linha por pedido entregue).",
                "SELECT c.nome, p.total FROM clientes c JOIN pedidos p ON p.cliente_id = c.id WHERE p.status = 'entregue';",
                A("A consulta original não diz COMO um pedido se liga a um cliente; então o banco combinou cada pedido com TODOS os clientes (8 × 6 = 48).",
                  "SELECT c.nome, p.total FROM clientes c JOIN pedidos p ON p.cliente_id = c.id WHERE p.status = 'entregue';"),
                A("FROM clientes c, pedidos p (sem ligação)", "Produto cartesiano: toda linha de uma tabela com toda linha da outra.",
                  "JOIN pedidos p ON p.cliente_id = c.id", "A condição de ligação: cada pedido só com o SEU cliente.", "WHERE p.status = 'entregue'", "Depois de ligar, o filtro."),
                "Quantidade de linhas absurda (multiplicada) quase sempre é JOIN sem ON ou com ON errado. Confira com SELECT COUNT(*) antes de confiar no relatório."));
            list.Add(SqlL(2, "x55", "state", "Reajuste por cargo",
                "O dissídio saiu: baristas ganham 5% de aumento e atendentes 8%. Os outros cargos não mudam. A Glória quer um UPDATE só, para nunca ficar com metade aplicado.",
                "Com um único UPDATE usando CASE, multiplique o salário por 1.05 para o cargo 'barista' e por 1.08 para o cargo 'atendente'. Use WHERE para tocar só nesses dois cargos.",
                "UPDATE funcionarios SET salario = CASE WHEN cargo = 'barista' THEN salario * 1.05 ELSE salario * 1.08 END WHERE cargo IN ('barista', 'atendente');",
                A("O valor do SET pode ser um CASE que escolhe a conta conforme o cargo. O WHERE limita as linhas aos dois cargos.",
                  "UPDATE funcionarios SET salario = CASE WHEN cargo = 'barista' THEN salario * 1.05 ELSE salario * 1.08 END WHERE cargo IN ('barista', 'atendente');"),
                A("SET salario = CASE ... END", "Cada linha calcula o seu novo salário.", "WHEN cargo = 'barista' THEN salario * 1.05", "Baristas: +5%.",
                  "ELSE salario * 1.08", "Pelo WHERE, quem sobra é atendente: +8%.", "WHERE cargo IN ('barista', 'atendente')", "Gerente, confeiteiro e entregadores nem são tocados."),
                "Sem o WHERE, um CASE sem ELSE devolveria NULL para os outros cargos — e salario é NOT NULL! Sempre limite o UPDATE às linhas certas.",
                false, A("funcionarios")));
            list.Add(SqlL(2, "x56", "schema", "Índice composto",
                "A tela 'meus pedidos' do app roda o tempo todo: SELECT * FROM pedidos WHERE cliente_id = ? AND status = ?. E o relatório por cliente usa só WHERE cliente_id = ?.",
                "Crie UM índice chamado idx_pedidos_cliente_status na tabela pedidos com as colunas cliente_id e status, nessa ordem, para atender as duas consultas.",
                "CREATE INDEX idx_pedidos_cliente_status ON pedidos (cliente_id, status);",
                A("Um índice pode ter várias colunas. A ORDEM importa: ele funciona como uma lista telefônica (sobrenome, depois nome).",
                  "CREATE INDEX idx_pedidos_cliente_status ON pedidos (cliente_id, status);"),
                A("(cliente_id, status)", "Índice composto: ordenado por cliente_id e, dentro de cada cliente, por status.", "WHERE cliente_id = ? AND status = ?", "Usa as duas colunas do índice.",
                  "WHERE cliente_id = ?", "Também usa (é o 'prefixo da esquerda'). Já WHERE status = ? sozinho NÃO usa."),
                "Confira com EXPLAIN SELECT * FROM pedidos WHERE cliente_id = 1 AND status = 'entregue'; — a coluna key deve mostrar idx_pedidos_cliente_status.",
                false, null, ExtraSql_CheckIndiceComposto));
            list.Add(SqlL(2, "x57", "query", "Entregas por cidade",
                "A logística quer saber para quais cidades saem mais entregas. A entrega aponta para o pedido, e o pedido aponta para o cliente, que tem a cidade.",
                "Mostre cada cidade e quantas entregas (tabela entregas) foram feitas para clientes dessa cidade.",
                "SELECT c.cidade, COUNT(*) FROM entregas e JOIN pedidos p ON p.id = e.pedido_id JOIN clientes c ON c.id = p.cliente_id GROUP BY c.cidade;",
                A("São três tabelas em corrente: entregas → pedidos (pedido_id) → clientes (cliente_id). Cada JOIN precisa do seu ON.",
                  "SELECT c.cidade, COUNT(*) FROM entregas e JOIN pedidos p ON p.id = e.pedido_id JOIN clientes c ON c.id = p.cliente_id GROUP BY c.cidade;"),
                A("JOIN pedidos p ON p.id = e.pedido_id", "Primeiro elo: a entrega ganha os dados do pedido.", "JOIN clientes c ON c.id = p.cliente_id",
                  "Segundo elo: o pedido ganha os dados do cliente (e a cidade).", "GROUP BY c.cidade", "Conta por cidade."),
                "Em JOINs encadeados, desenhe o caminho das chaves antes de escrever: cada tabela nova precisa se ligar a uma que já está na consulta."));
            list.Add(SqlL(2, "x58", "schema", "Preço sempre positivo",
                "Um bug no sistema antigo gravou um produto com preço -5.00. A Glória quer que o PRÓPRIO banco recuse isso, venha o dado de onde vier.",
                "Adicione à tabela produtos uma restrição CHECK chamada chk_preco_positivo garantindo que preco seja maior que 0.",
                "ALTER TABLE produtos ADD CONSTRAINT chk_preco_positivo CHECK (preco > 0);",
                A("Restrições novas entram com ALTER TABLE ... ADD CONSTRAINT nome ...", "ALTER TABLE produtos ADD CONSTRAINT chk_preco_positivo CHECK (preco > 0);"),
                A("ADD CONSTRAINT chk_preco_positivo", "Dá um nome à regra (ele aparece nas mensagens de erro e permite apagá-la depois).",
                  "CHECK (preco > 0)", "O banco recusa qualquer INSERT ou UPDATE que deixe a condição falsa (erro 3819)."),
                "O MySQL só passou a respeitar CHECK na versão 8.0.16; antes aceitava a sintaxe e ignorava. Teste: INSERT INTO produtos (nome, categoria, preco) VALUES ('Teste', 'doce', -1);",
                false, null, ExtraSql_CheckPrecoPositivo));
            list.Add(SqlL(2, "x59", "query", "Clientes dos últimos 6 meses",
                "A campanha de boas-vindas é para quem se cadastrou nos últimos 6 meses. A data de corte precisa ser calculada a partir de hoje, não digitada.",
                "Liste o nome dos clientes com criado_em a partir de 6 meses atrás (CURDATE() menos 6 meses), calculando a data na própria consulta.",
                "SELECT nome FROM clientes WHERE criado_em >= DATE_SUB(CURDATE(), INTERVAL 6 MONTH);",
                A("DATE_SUB(data, INTERVAL n UNIDADE) volta no tempo. A unidade vai no singular: DAY, MONTH, YEAR.", "SELECT nome FROM clientes WHERE criado_em >= DATE_SUB(CURDATE(), INTERVAL 6 MONTH);"),
                A("CURDATE()", "Hoje (2026-10-07 neste treino).", "DATE_SUB(CURDATE(), INTERVAL 6 MONTH)", "Seis meses atrás: 2026-04-07.", "criado_em >= ...", "Cadastros desse dia em diante."),
                "Equivalente: criado_em >= CURDATE() - INTERVAL 6 MONTH. Nunca deixe a data fixa num relatório que roda todo mês."));
            list.Add(SqlL(2, "x60", "state", "Direito ao esquecimento",
                "A cliente Mariana Alves (id 5) pediu, pela LGPD, a exclusão dos seus dados. Os pedidos dela apontam para o cadastro (chave estrangeira), e tudo deve sumir junto — ou nada.",
                "Numa transação, apague os pedidos do cliente_id 5 e depois o cliente de id 5; confirme com COMMIT.",
                "START TRANSACTION; DELETE FROM pedidos WHERE cliente_id = 5; DELETE FROM clientes WHERE id = 5; COMMIT;",
                A("A ordem importa: apagar o cliente primeiro dá o erro 1451, porque ainda há pedidos apontando para ele. Comece pelos 'filhos'.",
                  "START TRANSACTION; DELETE FROM pedidos WHERE cliente_id = 5; DELETE FROM clientes WHERE id = 5; COMMIT;"),
                A("START TRANSACTION", "As duas exclusões viram uma operação só.", "DELETE FROM pedidos WHERE cliente_id = 5", "Primeiro os registros dependentes (os 'filhos').",
                  "DELETE FROM clientes WHERE id = 5", "Depois o 'pai': agora nenhuma chave estrangeira impede.", "COMMIT", "Confirma tudo junto."),
                "Em produção, antes de apagar, confira se a lei não obriga a guardar algo (ex.: notas fiscais). Às vezes o certo é anonimizar em vez de excluir.",
                false, A("clientes", "pedidos")));
            Transacao("x60");
            list.Add(SqlL(2, "x61", "query", "Convidados da festa",
                "A festa de aniversário do Café 365 vai convidar os clientes de São Paulo e toda a equipe. A Marta Gomes é cliente E funcionária, e não pode receber dois convites.",
                "Liste numa única coluna os nomes dos clientes de 'São Paulo' junto com os nomes de todos os funcionários, sem nomes repetidos.",
                "SELECT nome FROM clientes WHERE cidade = 'São Paulo' UNION SELECT nome FROM funcionarios;",
                A("Para empilhar o resultado de duas consultas, use UNION. Ele já remove as linhas repetidas (UNION ALL manteria).",
                  "SELECT nome FROM clientes WHERE cidade = 'São Paulo' UNION SELECT nome FROM funcionarios;"),
                A("SELECT nome FROM clientes WHERE ...", "Primeira lista.", "UNION", "Empilha as duas listas e remove os repetidos.",
                  "SELECT nome FROM funcionarios", "Segunda lista (precisa ter o mesmo número de colunas)."),
                "UNION ALL é mais rápido (não procura repetidos): use quando a repetição é impossível ou desejada. Um ORDER BY, se houver, vai uma vez só, no final."));
            list.Add(SqlL(2, "x62", "schema", "Nome maior sem perder regras",
                "Clientes com nomes compostos longos não cabem em VARCHAR(80). A Glória pediu para aumentar para 150 — sem perder a regra de obrigatório.",
                "Altere a coluna nome da tabela clientes para VARCHAR(150), mantendo-a NOT NULL.",
                "ALTER TABLE clientes MODIFY COLUMN nome VARCHAR(150) NOT NULL;",
                A("MODIFY redefine a coluna INTEIRA: tudo o que você não repetir (NOT NULL, DEFAULT...) se perde.", "ALTER TABLE clientes MODIFY COLUMN nome VARCHAR(150) NOT NULL;"),
                A("MODIFY COLUMN nome", "Redefine a coluna nome.", "VARCHAR(150)", "Novo tamanho.", "NOT NULL", "Precisa ser repetido! Sem ele, a coluna passaria a aceitar NULL."),
                "Antes de um MODIFY, rode SHOW CREATE TABLE clientes; e copie a definição atual da coluna, mudando só o necessário. CHANGE faz o mesmo e ainda permite renomear.",
                false, null, ExtraSql_CheckModify));
            list.Add(SqlL(2, "x63", "query", "Tempo médio de entrega",
                "A logística quer comparar os entregadores pelo tempo médio de entrega, em minutos, da saída até a chegada.",
                "Para cada entregador que fez entregas, mostre o nome dele e a média, em minutos, entre saida e chegada (TIMESTAMPDIFF), juntando entregas com funcionarios.",
                "SELECT f.nome, AVG(TIMESTAMPDIFF(MINUTE, e.saida, e.chegada)) FROM entregas e JOIN funcionarios f ON f.id = e.entregador_id GROUP BY f.id, f.nome;",
                A("TIMESTAMPDIFF(MINUTE, inicio, fim) dá a duração em minutos. Junte com funcionarios para ter o nome e tire a média por entregador.",
                  "SELECT f.nome, AVG(TIMESTAMPDIFF(MINUTE, e.saida, e.chegada)) FROM entregas e JOIN funcionarios f ON f.id = e.entregador_id GROUP BY f.id, f.nome;"),
                A("TIMESTAMPDIFF(MINUTE, e.saida, e.chegada)", "Minutos de cada entrega.", "AVG(...)", "Média por grupo. A entrega ainda a caminho (chegada NULL) dá NULL e fica fora da média.",
                  "GROUP BY f.id, f.nome", "Um grupo por entregador."),
                "Repare: o Caio tem 3 entregas, mas a média dele usa só 2 (a terceira não chegou). Agregações ignoram NULL — às vezes é o que você quer, às vezes esconde um problema."));
            list.Add(SqlL(2, "x64", "schema", "Observações em cascata",
                "O atendimento quer anotar observações nos pedidos ('sem açúcar', 'portão azul'). Se um pedido for apagado, as observações dele devem sumir junto, automaticamente.",
                "Crie a tabela observacoes com: id (INT, chave primária, auto incremento), pedido_id (INT, NOT NULL) e texto (VARCHAR(200), NOT NULL), com chave estrangeira de pedido_id para pedidos(id) usando ON DELETE CASCADE.",
                "CREATE TABLE observacoes (id INT PRIMARY KEY AUTO_INCREMENT, pedido_id INT NOT NULL, texto VARCHAR(200) NOT NULL, FOREIGN KEY (pedido_id) REFERENCES pedidos(id) ON DELETE CASCADE);",
                A("A chave estrangeira aceita uma ação para quando o 'pai' for apagado: ON DELETE ..., no fim da restrição.",
                  "CREATE TABLE observacoes (id INT PRIMARY KEY AUTO_INCREMENT, pedido_id INT NOT NULL, texto VARCHAR(200) NOT NULL, FOREIGN KEY (pedido_id) REFERENCES pedidos(id) ON DELETE CASCADE);"),
                A("FOREIGN KEY (pedido_id) REFERENCES pedidos(id)", "Observação só de pedido que existe.", "ON DELETE CASCADE", "Apagou o pedido? O banco apaga as observações dele junto.",
                  "(sem ação)", "O padrão é RESTRICT: o banco recusaria apagar um pedido que tem observações (erro 1451)."),
                "Use CASCADE só para dados que não fazem sentido sozinhos (observações, itens). Para dados importantes (pagamentos, notas fiscais), prefira o padrão RESTRICT.",
                false, null, ExtraSql_CheckObservacoes));
            list.Add(SqlL(2, "x65", "state", "Bônus para entregadores",
                "Quem já fez pelo menos uma entrega ganha um bônus de 150 reais no salário. O Renan acabou de entrar e ainda não fez nenhuma, então não recebe.",
                "Aumente em 150.00 o salário dos funcionários cujo id aparece na coluna entregador_id da tabela entregas, usando uma subconsulta no WHERE.",
                "UPDATE funcionarios SET salario = salario + 150 WHERE id IN (SELECT entregador_id FROM entregas);",
                A("Filtrar por cargo = 'entregador' pegaria o Renan também. O critério é ter entrega registrada: busque os ids na tabela entregas.",
                  "UPDATE funcionarios SET salario = salario + 150 WHERE id IN (SELECT entregador_id FROM entregas);"),
                A("(SELECT entregador_id FROM entregas)", "Os entregadores que têm entrega (6 e 7).", "WHERE id IN (...)", "Só esses recebem.", "SET salario = salario + 150", "Soma ao valor atual de cada um."),
                "O MySQL não deixa a subconsulta ler a MESMA tabela que o UPDATE está alterando (erro 1093). Aqui pode, porque ela lê a tabela entregas.",
                false, A("funcionarios")));
            list.Add(SqlL(2, "x66", "schema", "View de faturamento",
                "O Fábio usa todo dia a mesma consulta de gasto por cliente e quer transformá-la numa view para o BI ler direto.",
                "Crie a view vw_gasto_clientes com o nome do cliente (coluna nome) e a soma do total dos pedidos 'entregue' dele numa coluna chamada total_gasto (só clientes com pedido entregue).",
                "CREATE VIEW vw_gasto_clientes AS SELECT c.nome, SUM(p.total) AS total_gasto FROM clientes c JOIN pedidos p ON p.cliente_id = c.id WHERE p.status = 'entregue' GROUP BY c.id, c.nome;",
                A("Escreva e teste primeiro o SELECT (JOIN + WHERE + GROUP BY). Funcionou? Coloque CREATE VIEW vw_gasto_clientes AS na frente dele.",
                  "CREATE VIEW vw_gasto_clientes AS SELECT c.nome, SUM(p.total) AS total_gasto FROM clientes c JOIN pedidos p ON p.cliente_id = c.id WHERE p.status = 'entregue' GROUP BY c.id, c.nome;"),
                A("CREATE VIEW vw_gasto_clientes AS", "Guarda a consulta com um nome.", "SUM(p.total) AS total_gasto", "O apelido vira o nome da coluna da view.",
                  "WHERE p.status = 'entregue'", "Só pedidos entregues.", "GROUP BY c.id, c.nome", "Uma linha por cliente."),
                "A view não guarda os dados: ela roda a consulta toda vez que é lida, então um pedido novo entregue aparece na hora. Para trocar a consulta: CREATE OR REPLACE VIEW.",
                false, null, ExtraSql_CheckVwGasto));
            list.Add(SqlL(2, "x67", "query", "Entregas em andamento",
                "Um cliente ligou perguntando do pedido. A Tânia precisa da lista de entregas que saíram e ainda não chegaram (chegada vazia), com quem pediu e quem está levando.",
                "Liste o pedido_id das entregas com chegada NULL, o nome do cliente numa coluna chamada cliente e o nome do entregador numa coluna chamada entregador.",
                "SELECT e.pedido_id, c.nome AS cliente, f.nome AS entregador FROM entregas e JOIN pedidos p ON p.id = e.pedido_id JOIN clientes c ON c.id = p.cliente_id " +
                "JOIN funcionarios f ON f.id = e.entregador_id WHERE e.chegada IS NULL;",
                A("São 4 tabelas: entregas → pedidos → clientes, e entregas → funcionarios. Duas colunas se chamam nome: dê apelidos a elas.",
                  "SELECT e.pedido_id, c.nome AS cliente, f.nome AS entregador FROM entregas e JOIN pedidos p ON p.id = e.pedido_id JOIN clientes c ON c.id = p.cliente_id JOIN funcionarios f ON f.id = e.entregador_id WHERE e.chegada IS NULL;"),
                A("JOIN pedidos ... JOIN clientes", "O caminho até o nome do cliente.", "JOIN funcionarios f ON f.id = e.entregador_id", "Outro ramo: o nome do entregador.",
                  "c.nome AS cliente, f.nome AS entregador", "Apelidos evitam duas colunas chamadas nome.", "WHERE e.chegada IS NULL", "Ainda não chegou."),
                "Uma tabela pode se ligar a várias outras ao mesmo tempo (entregas liga em pedidos E em funcionarios). Sempre um ON por JOIN."));
            Cabecalhos("x67");
            list.Add(SqlL(2, "x68", "schema", "Chave estrangeira esquecida",
                "Na criação da tabela entregas, alguém esqueceu a chave estrangeira de entregador_id: hoje dá para gravar entrega de um entregador que não existe.",
                "Adicione à tabela entregas uma chave estrangeira chamada fk_entregas_entregador, de entregador_id para funcionarios(id).",
                "ALTER TABLE entregas ADD CONSTRAINT fk_entregas_entregador FOREIGN KEY (entregador_id) REFERENCES funcionarios(id);",
                A("Restrição nova em tabela existente: ALTER TABLE ... ADD CONSTRAINT nome FOREIGN KEY (coluna) REFERENCES tabela(coluna).",
                  "ALTER TABLE entregas ADD CONSTRAINT fk_entregas_entregador FOREIGN KEY (entregador_id) REFERENCES funcionarios(id);"),
                A("ADD CONSTRAINT fk_entregas_entregador", "Nome da restrição (facilita achar e apagar depois).", "FOREIGN KEY (entregador_id)", "Coluna desta tabela.",
                  "REFERENCES funcionarios(id)", "O valor precisa existir em funcionarios.id. O MySQL confere as linhas que JÁ existem antes de criar a chave."),
                "Se já houver dados 'órfãos', o ALTER falha com o erro 1452. Encontre-os antes: SELECT e.* FROM entregas e LEFT JOIN funcionarios f ON f.id = e.entregador_id WHERE f.id IS NULL AND e.entregador_id IS NOT NULL;",
                false, null, ExtraSql_CheckFkEntregador));
            list.Add(SqlL(2, "x69", "query", "Ticket médio acima da casa",
                "O Fábio quer saber quais clientes gastam, por pedido entregue, mais do que a média geral dos pedidos entregues da casa.",
                "Liste o nome do cliente e a média do total dos pedidos 'entregue' dele, só para os clientes cuja média é maior que a média de TODOS os pedidos 'entregue' (calculada numa subconsulta).",
                "SELECT c.nome, AVG(p.total) FROM clientes c JOIN pedidos p ON p.cliente_id = c.id WHERE p.status = 'entregue' GROUP BY c.id, c.nome " +
                "HAVING AVG(p.total) > (SELECT AVG(total) FROM pedidos WHERE status = 'entregue');",
                A("Agrupe por cliente (só pedidos entregues) e filtre os GRUPOS com HAVING. O valor de comparação é outra média: uma subconsulta.",
                  "... GROUP BY c.id, c.nome HAVING AVG(p.total) > (SELECT AVG(total) FROM pedidos WHERE status = 'entregue');"),
                A("WHERE p.status = 'entregue'", "Só pedidos entregues entram nos grupos.", "HAVING AVG(p.total) > (...)", "Compara a média de cada cliente...",
                  "(SELECT AVG(total) FROM pedidos WHERE status = 'entregue')", "...com a média geral dos entregues (13.20)."),
                "HAVING também aceita subconsultas. Use o MESMO filtro (entregue) dos dois lados, senão a comparação fica injusta."));
            list.Add(SqlL(2, "x70", "schema", "Avaliação de 1 a 5",
                "Depois da entrega, o cliente dá uma nota de 1 a 5 pelo app. O app às vezes manda lixo, então o banco precisa barrar nota fora da faixa.",
                "Crie a tabela notas_entrega com: id (INT, chave primária, auto incremento), entrega_id (INT, NOT NULL, chave estrangeira para entregas.id) e nota (INT, NOT NULL) com um CHECK que só aceite valores de 1 a 5.",
                "CREATE TABLE notas_entrega (id INT PRIMARY KEY AUTO_INCREMENT, entrega_id INT NOT NULL, nota INT NOT NULL CHECK (nota BETWEEN 1 AND 5), FOREIGN KEY (entrega_id) REFERENCES entregas(id));",
                A("O CHECK pode ir na própria coluna: nota INT NOT NULL CHECK (condição). Uma faixa que inclui as pontas se escreve com BETWEEN.",
                  "CREATE TABLE notas_entrega (id INT PRIMARY KEY AUTO_INCREMENT, entrega_id INT NOT NULL, nota INT NOT NULL CHECK (nota BETWEEN 1 AND 5), FOREIGN KEY (entrega_id) REFERENCES entregas(id));"),
                A("nota INT NOT NULL CHECK (nota BETWEEN 1 AND 5)", "Obrigatória e só de 1 a 5 (inclusive). Nota 0 ou 6: erro 3819.",
                  "FOREIGN KEY (entrega_id) REFERENCES entregas(id)", "Só dá para avaliar entrega que existe."),
                "NOT NULL importa junto do CHECK: uma condição com NULL não é FALSA, então o CHECK sozinho deixaria passar nota NULL.",
                false, null, ExtraSql_CheckNotasEntrega));
            list.Add(SqlL(2, "x71", "query", "Participação de cada status",
                "A diretoria quer ver que porcentagem do valor total dos pedidos está em cada status, com 1 casa decimal.",
                "Mostre o status e uma coluna chamada percentual: a soma do total daquele status vezes 100, dividida pela soma do total de TODOS os pedidos (subconsulta), arredondada para 1 casa decimal.",
                "SELECT status, ROUND(SUM(total) * 100 / (SELECT SUM(total) FROM pedidos), 1) AS percentual FROM pedidos GROUP BY status;",
                A("O total geral é uma subconsulta escalar, que pode entrar na conta do SELECT junto com a agregação do grupo.",
                  "SELECT status, ROUND(SUM(total) * 100 / (SELECT SUM(total) FROM pedidos), 1) AS percentual FROM pedidos GROUP BY status;"),
                A("SUM(total)", "Soma do grupo (entregues: 79.20).", "(SELECT SUM(total) FROM pedidos)", "Total geral (141.10).", "* 100 / ...", "Vira porcentagem.",
                  "ROUND(..., 1) AS percentual", "Uma casa decimal: 56.1, 26.2 e 17.7."),
                "No MySQL, / sempre devolve número com casas decimais (5 / 2 = 2.5000); para a divisão inteira existe DIV (5 DIV 2 = 2)."));
            Cabecalhos("x71");
            list.Add(SqlL(2, "x72", "state", "Troca de gerente",
                "A Helena (id 1) vai abrir a loja nova. O Otávio (id 4) assume a gerência: vira 'gerente', deixa de ter gerente, e todos que respondiam à Helena passam a responder a ele — tudo de uma vez.",
                "Numa transação: mude o cargo do funcionário 4 para 'gerente' e o gerente_id dele para NULL; mude para 4 o gerente_id de todos que tinham gerente_id 1 (fora o próprio 4); confirme com COMMIT.",
                "START TRANSACTION; UPDATE funcionarios SET cargo = 'gerente', gerente_id = NULL WHERE id = 4; UPDATE funcionarios SET gerente_id = 4 WHERE gerente_id = 1; COMMIT;",
                A("São dois UPDATE dentro de START TRANSACTION ... COMMIT. Se o Otávio for atualizado primeiro (gerente_id = NULL), o segundo UPDATE já não pega ele.",
                  "START TRANSACTION; UPDATE funcionarios SET cargo = 'gerente', gerente_id = NULL WHERE id = 4; UPDATE funcionarios SET gerente_id = 4 WHERE gerente_id = 1; COMMIT;"),
                A("SET cargo = 'gerente', gerente_id = NULL", "Duas colunas no mesmo SET, separadas por vírgula (aqui = NULL está certo: é atribuição, não comparação).",
                  "WHERE gerente_id = 1", "Todos os subordinados da Helena (o Otávio já saiu da lista).", "COMMIT", "O organograma muda inteiro de uma vez."),
                "Enquanto a transação não termina, as outras conexões continuam vendo o organograma antigo (isolamento): ninguém enxerga o meio do caminho.",
                false, A("funcionarios")));
            Transacao("x72");
        }

        // ───────────────────────────── conferências de estrutura (modo schema) ─────────────────────────────
        static string ExtraSql_Tabela(SqlDb db, string nome, string[] cols, out SqlTable t)
        {
            if (!db.Tables.TryGetValue(nome, out t))
                return db.Views.ContainsKey(nome) ? "'" + nome + "' foi criada como VIEW, mas o pedido é uma TABELA." : "A tabela " + nome + " ainda não foi criada (confira o nome: " + nome + ").";
            var tt = t;
            var faltam = cols.Where(c => tt.ColIndex(c) < 0).ToList();
            if (faltam.Count > 0) return "Faltou a coluna " + string.Join(", ", faltam) + " na tabela " + nome + ".";
            if (t.Cols.Count != cols.Length) return "A tabela " + nome + " deve ter só as colunas " + string.Join(", ", cols) + ".";
            return null;
        }

        static SqlCol ExtraSql_Col(SqlTable t, string nome) { return t.Cols[t.ColIndex(nome)]; }

        static string ExtraSql_IdAuto(SqlTable t)
        {
            var id = ExtraSql_Col(t, "id");
            if (!id.IsInt || id.Type == "BOOLEAN") return "A coluna id precisa ser do tipo INT.";
            if (!id.Pk) return "A coluna id precisa ser a chave primária (PRIMARY KEY).";
            if (!id.Auto) return "A coluna id precisa ser AUTO_INCREMENT (numerada automaticamente).";
            return null;
        }

        static string ExtraSql_Varchar(SqlCol c, int len, bool notNull)
        {
            if (c.Type != "VARCHAR") return "A coluna " + c.Name + " precisa ser do tipo VARCHAR(" + len + ").";
            if (c.Len != len) return "A coluna " + c.Name + " precisa ser VARCHAR(" + len + ") (está com tamanho " + c.Len + ").";
            if (notNull && !c.NotNull) return "A coluna " + c.Name + " precisa ser obrigatória (NOT NULL).";
            if (!notNull && c.NotNull) return "A coluna " + c.Name + " deve ser opcional (sem NOT NULL).";
            return null;
        }

        static string ExtraSql_Fk(SqlCol c, string tabela)
        {
            if (c.RefTable != null && c.RefTable.Equals(tabela, StringComparison.OrdinalIgnoreCase) && string.Equals(c.RefCol, "id", StringComparison.OrdinalIgnoreCase)) return null;
            if (c.InlineRef != null)
                return "Você escreveu REFERENCES direto na coluna " + c.Name + ": o MySQL 8 aceita essa sintaxe, mas IGNORA a regra! Declare a restrição separada: FOREIGN KEY (" + c.Name + ") REFERENCES " + tabela + "(id).";
            return "Faltou a chave estrangeira: FOREIGN KEY (" + c.Name + ") REFERENCES " + tabela + "(id).";
        }

        // roda comandos de teste numa CÓPIA do banco do aluno: devolve o código do erro do MySQL (0 = deu certo)
        static int ExtraSql_Tenta(SqlDb copia, string sql)
        {
            try { copia.Execute(sql); return 0; }
            catch (SqlError e) { return e.Code; }
        }

        static string ExtraSql_CheckFornecedores(SqlDb db)
        {
            SqlTable t;
            string m = ExtraSql_Tabela(db, "fornecedores", new[] { "id", "nome", "telefone" }, out t);
            if (m != null) return m;
            return ExtraSql_IdAuto(t) ?? ExtraSql_Varchar(ExtraSql_Col(t, "nome"), 100, true) ?? ExtraSql_Varchar(ExtraSql_Col(t, "telefone"), 20, false);
        }

        static string ExtraSql_CheckCupons(SqlDb db)
        {
            SqlTable t;
            string m = ExtraSql_Tabela(db, "cupons", new[] { "codigo", "desconto", "validade" }, out t);
            if (m != null) return m;
            var cod = ExtraSql_Col(t, "codigo");
            if (cod.Type != "VARCHAR" || cod.Len != 20) return "A coluna codigo precisa ser VARCHAR(20).";
            if (!cod.Pk) return "A coluna codigo precisa ser a chave primária (PRIMARY KEY).";
            var d = ExtraSql_Col(t, "desconto");
            if (d.Type == "FLOAT" || d.Type == "DOUBLE") return "FLOAT/DOUBLE guardam valores APROXIMADOS: para dinheiro e porcentagem use DECIMAL(5,2).";
            if (d.Type != "DECIMAL" || d.Len != 5 || d.Scale != 2) return "A coluna desconto precisa ser DECIMAL(5,2).";
            if (!d.NotNull) return "A coluna desconto precisa ser obrigatória (NOT NULL).";
            var v = ExtraSql_Col(t, "validade");
            if (v.Type != "DATE") return "A coluna validade precisa ser do tipo DATE.";
            if (!v.NotNull) return "A coluna validade precisa ser obrigatória (NOT NULL).";
            return null;
        }

        static string ExtraSql_CheckRenomear(SqlDb db)
        {
            var t = db.Table("pedidos");
            int k = t.ColIndex("data_pedido");
            if (k < 0) return t.ColIndex("data") >= 0 ? "A tabela pedidos ainda tem a coluna data: renomeie-a para data_pedido." : "A tabela pedidos não tem a coluna data_pedido.";
            if (t.ColIndex("data") >= 0) return "A coluna data_pedido foi criada, mas a coluna data continua lá: o pedido era RENOMEAR, não criar outra.";
            if (t.Cols[k].Type != "DATE") return "A coluna data_pedido precisa continuar sendo do tipo DATE.";
            var orig = SqlSample.Fresh().Table("pedidos");
            int ko = orig.ColIndex("data");
            if (t.Cols.Count != orig.Cols.Count || t.Rows.Count != orig.Rows.Count) return "A tabela pedidos mudou além do nome da coluna.";
            for (int i = 0; i < t.Rows.Count; i++)
                if (SqlDb.Key(t.Rows[i][k]) != SqlDb.Key(orig.Rows[i][ko])) return "A coluna data_pedido perdeu as datas que estavam em data! Renomear não pode apagar os dados (use RENAME COLUMN, não DROP + ADD).";
            return null;
        }

        static string ExtraSql_CheckIndiceUnico(SqlDb db)
        {
            var t = db.Table("produtos");
            var ix = t.Indexes.FirstOrDefault(i => i.Name.Equals("idx_produtos_nome", StringComparison.OrdinalIgnoreCase));
            if (ix == null)
            {
                bool outro = t.Indexes.Any(i => i.Unique && i.Cols.Count == 1 && i.Cols[0].Equals("nome", StringComparison.OrdinalIgnoreCase)) || ExtraSql_Col(t, "nome").Unique;
                return outro ? "O índice único foi criado, mas com outro nome: o pedido é idx_produtos_nome." : "O índice idx_produtos_nome não foi criado na tabela produtos.";
            }
            if (ix.Cols.Count != 1 || !ix.Cols[0].Equals("nome", StringComparison.OrdinalIgnoreCase)) return "O índice idx_produtos_nome precisa ser só na coluna nome.";
            if (!ix.Unique) return "O índice idx_produtos_nome foi criado, mas não é ÚNICO: ele acelera a busca, mas ainda deixa repetir. Use CREATE UNIQUE INDEX.";
            return null;
        }

        static string ExtraSql_ViewIgual(SqlDb db, string view, string esperado, bool nomes)
        {
            if (!db.Views.ContainsKey(view))
            {
                if (db.Tables.ContainsKey(view)) return "'" + view + "' foi criada como TABELA (cópia parada dos dados). O pedido é uma VIEW: CREATE VIEW " + view + " AS SELECT ...";
                return db.Views.Count > 0 ? "Você criou uma view, mas com outro nome: o pedido é " + view + "." : "A view " + view + " ainda não foi criada: CREATE VIEW " + view + " AS SELECT ...";
            }
            SqlResult got;
            try { got = db.Clone().Execute("SELECT * FROM " + view); }
            catch (SqlError e) { return "A view existe, mas dá erro ao ser lida: " + e.Hint; }
            var exp = SqlSample.Fresh().Execute(esperado);
            var msg = SqlTasks.CompareResult(got, exp, false);
            if (msg == null && nomes) msg = SqlTasks.CompareHeaders(got, exp);
            return msg == null ? null : "A view " + view + " existe, mas o conteúdo dela não é o pedido. " + msg;
        }

        static string ExtraSql_CheckVwEquipe(SqlDb db)
        {
            SqlView v;
            if (db.Views.TryGetValue("vw_equipe", out v))
            {
                var r = db.Clone().Execute("SELECT * FROM vw_equipe");
                if (r.Headers.Any(h => h.Equals("salario", StringComparison.OrdinalIgnoreCase))) return "A view vw_equipe ainda mostra o salário! O objetivo era justamente esconder essa coluna: só nome e cargo.";
            }
            return ExtraSql_ViewIgual(db, "vw_equipe", "SELECT nome, cargo FROM funcionarios", true);
        }

        static string ExtraSql_CheckVwGasto(SqlDb db)
        {
            return ExtraSql_ViewIgual(db, "vw_gasto_clientes",
                "SELECT c.nome, SUM(p.total) AS total_gasto FROM clientes c JOIN pedidos p ON p.cliente_id = c.id WHERE p.status = 'entregue' GROUP BY c.id, c.nome", true);
        }

        static string ExtraSql_CheckEnderecos(SqlDb db)
        {
            SqlTable t;
            string m = ExtraSql_Tabela(db, "enderecos", new[] { "id", "cliente_id", "cep" }, out t);
            if (m != null) return m;
            m = ExtraSql_IdAuto(t);
            if (m != null) return m;
            var c = ExtraSql_Col(t, "cliente_id");
            if (!c.IsInt || c.Type == "BOOLEAN") return "A coluna cliente_id precisa ser INT (o mesmo tipo de clientes.id).";
            if (!c.NotNull) return "A coluna cliente_id precisa ser obrigatória (NOT NULL): todo endereço é de algum cliente.";
            return ExtraSql_Fk(c, "clientes") ?? ExtraSql_Varchar(ExtraSql_Col(t, "cep"), 9, true);
        }

        static string ExtraSql_CheckBackup(SqlDb db)
        {
            SqlTable b;
            if (!db.Tables.TryGetValue("produtos_backup", out b))
                return db.Views.ContainsKey("produtos_backup") ? "produtos_backup foi criada como VIEW: ela mostraria os preços JÁ reajustados. Backup é uma TABELA com uma cópia dos dados." : "A tabela produtos_backup ainda não foi criada.";
            var orig = db.Table("produtos");
            if (b.Cols.Count != orig.Cols.Count || orig.Cols.Any(c => b.ColIndex(c.Name) < 0))
                return "A tabela produtos_backup precisa ter TODAS as colunas de produtos (" + string.Join(", ", orig.Cols.Select(c => c.Name)) + ").";
            if (b.Rows.Count == 0) return SqlTasks.Pending;   // estrutura pronta (ex.: CREATE TABLE ... LIKE), falta copiar as linhas
            var copia = db.Clone();
            var msg = SqlTasks.CompareResult(copia.Execute("SELECT * FROM produtos_backup"), copia.Execute("SELECT * FROM produtos"), false);
            return msg == null ? null : "A tabela produtos_backup não é uma cópia fiel de produtos. " + msg;
        }

        static string ExtraSql_CheckIndiceComposto(SqlDb db)
        {
            var t = db.Table("pedidos");
            var ix = t.Indexes.FirstOrDefault(i => i.Name.Equals("idx_pedidos_cliente_status", StringComparison.OrdinalIgnoreCase));
            if (ix == null) return t.Indexes.Count > 0 ? "Você criou um índice, mas o nome pedido é idx_pedidos_cliente_status." : "O índice idx_pedidos_cliente_status não foi criado na tabela pedidos.";
            if (ix.Cols.Count != 2) return "O índice precisa ter exatamente duas colunas: (cliente_id, status).";
            if (ix.Cols[0].Equals("status", StringComparison.OrdinalIgnoreCase) && ix.Cols[1].Equals("cliente_id", StringComparison.OrdinalIgnoreCase))
                return "As colunas estão na ordem invertida! Com (status, cliente_id), a consulta só por cliente_id não consegue usar o índice (regra do prefixo da esquerda). Use (cliente_id, status).";
            if (!ix.Cols[0].Equals("cliente_id", StringComparison.OrdinalIgnoreCase) || !ix.Cols[1].Equals("status", StringComparison.OrdinalIgnoreCase))
                return "O índice precisa ser nas colunas (cliente_id, status), nessa ordem.";
            return null;
        }

        static string ExtraSql_CheckPrecoPositivo(SqlDb db)
        {
            var t = db.Table("produtos");
            if (t.Checks.Count == 0) return "A tabela produtos ainda não tem nenhuma restrição CHECK.";
            if (!t.Checks.Any(c => c.Key.Equals("chk_preco_positivo", StringComparison.OrdinalIgnoreCase)))
                return "O CHECK foi criado, mas com outro nome (" + string.Join(", ", t.Checks.Select(c => c.Key)) + "). O pedido é chk_preco_positivo: use ADD CONSTRAINT chk_preco_positivo CHECK (...).";
            string ins = "INSERT INTO produtos (nome, categoria, preco, estoque) VALUES ('Teste', 'doce', {0}, 1)";
            if (ExtraSql_Tenta(db.Clone(), string.Format(ins, "-5.00")) != 3819) return "O CHECK não está recusando preço negativo: a condição precisa ser preco > 0.";
            if (ExtraSql_Tenta(db.Clone(), string.Format(ins, "0")) != 3819) return "O CHECK ainda aceita preço 0. O pedido é preço MAIOR que 0: preco > 0.";
            if (ExtraSql_Tenta(db.Clone(), string.Format(ins, "0.01")) != 0 || ExtraSql_Tenta(db.Clone(), string.Format(ins, "99.90")) != 0)
                return "O CHECK está recusando preços válidos (como 0.01 ou 99.90). A condição é só preco > 0.";
            return null;
        }

        static string ExtraSql_CheckModify(SqlDb db)
        {
            var t = db.Table("clientes");
            int k = t.ColIndex("nome");
            if (k < 0) return "A tabela clientes ficou sem a coluna nome!";
            var c = t.Cols[k];
            if (c.Type != "VARCHAR" || c.Len != 150) return "A coluna nome precisa ficar VARCHAR(150) (está " + c.TypeText + ").";
            if (!c.NotNull) return "O tamanho mudou, mas a coluna nome perdeu o NOT NULL! MODIFY redefine a coluna inteira: repita NOT NULL na definição.";
            var orig = SqlSample.Fresh().Table("clientes");
            if (t.Rows.Count != orig.Rows.Count || t.Cols.Count != orig.Cols.Count) return "A tabela clientes mudou além da coluna nome.";
            return null;
        }

        static string ExtraSql_CheckObservacoes(SqlDb db)
        {
            SqlTable t;
            string m = ExtraSql_Tabela(db, "observacoes", new[] { "id", "pedido_id", "texto" }, out t);
            if (m != null) return m;
            m = ExtraSql_IdAuto(t);
            if (m != null) return m;
            var c = ExtraSql_Col(t, "pedido_id");
            if (!c.IsInt || c.Type == "BOOLEAN") return "A coluna pedido_id precisa ser INT.";
            if (!c.NotNull) return "A coluna pedido_id precisa ser obrigatória (NOT NULL).";
            m = ExtraSql_Fk(c, "pedidos") ?? ExtraSql_Varchar(ExtraSql_Col(t, "texto"), 200, true);
            if (m != null) return m;
            if (c.OnDelete != "CASCADE")
                return c.OnDelete == "SET NULL" ? "ON DELETE SET NULL deixaria observações 'soltas' (sem pedido). O pedido é ON DELETE CASCADE." :
                       "A chave estrangeira existe, mas sem a ação ON DELETE CASCADE: hoje o banco RECUSARIA apagar um pedido com observações (erro 1451).";
            var copia = db.Clone();
            if (ExtraSql_Tenta(copia, "INSERT INTO observacoes (pedido_id, texto) VALUES (4, 'teste')") != 0 || ExtraSql_Tenta(copia, "DELETE FROM pedidos WHERE id = 4") != 0 ||
                copia.Table("observacoes").Rows.Count != 0)
                return "A cascata não funcionou no teste (apagar o pedido 4 deveria apagar a observação dele).";
            return null;
        }

        static string ExtraSql_CheckFkEntregador(SqlDb db)
        {
            var t = db.Table("entregas");
            var c = ExtraSql_Col(t, "entregador_id");
            if (c.RefTable == null) return "A coluna entregador_id ainda não tem chave estrangeira.";
            if (!c.RefTable.Equals("funcionarios", StringComparison.OrdinalIgnoreCase) || !string.Equals(c.RefCol, "id", StringComparison.OrdinalIgnoreCase))
                return "A chave estrangeira de entregador_id precisa apontar para funcionarios(id).";
            if (!string.Equals(c.FkName, "fk_entregas_entregador", StringComparison.OrdinalIgnoreCase))
                return "A chave foi criada, mas sem o nome pedido. Use ADD CONSTRAINT fk_entregas_entregador FOREIGN KEY (...) REFERENCES ...";
            if (ExtraSql_Tenta(db.Clone(), "INSERT INTO entregas (pedido_id, entregador_id, saida) VALUES (5, 99, '2026-10-07 10:00:00')") != 1452)
                return "A chave não está barrando entregador inexistente.";
            return null;
        }

        static string ExtraSql_CheckNotasEntrega(SqlDb db)
        {
            SqlTable t;
            string m = ExtraSql_Tabela(db, "notas_entrega", new[] { "id", "entrega_id", "nota" }, out t);
            if (m != null) return m;
            m = ExtraSql_IdAuto(t);
            if (m != null) return m;
            var e = ExtraSql_Col(t, "entrega_id");
            if (!e.IsInt || e.Type == "BOOLEAN") return "A coluna entrega_id precisa ser INT.";
            if (!e.NotNull) return "A coluna entrega_id precisa ser obrigatória (NOT NULL).";
            m = ExtraSql_Fk(e, "entregas");
            if (m != null) return m;
            var n = ExtraSql_Col(t, "nota");
            if (!n.IsInt || n.Type == "BOOLEAN") return "A coluna nota precisa ser INT.";
            if (!n.NotNull) return "A coluna nota precisa ser NOT NULL: sem isso, uma nota NULL passaria pelo CHECK (NULL não é 'falso').";
            if (t.Checks.Count == 0) return "Faltou o CHECK que limita a nota de 1 a 5 (ex.: CHECK (nota BETWEEN 1 AND 5)).";
            string ins = "INSERT INTO notas_entrega (entrega_id, nota) VALUES (1, {0})";
            foreach (var ruim in new[] { "0", "6", "-1", "10" })
                if (ExtraSql_Tenta(db.Clone(), string.Format(ins, ruim)) != 3819) return "O CHECK deixou passar a nota " + ruim + ". Só de 1 a 5 (inclusive) pode entrar.";
            foreach (var boa in new[] { "1", "3", "5" })
                if (ExtraSql_Tenta(db.Clone(), string.Format(ins, boa)) != 0) return "O CHECK recusou a nota " + boa + ", que é válida. As pontas 1 e 5 também entram (BETWEEN inclui as pontas).";
            return null;
        }
    }

    static partial class Docs
    {
        static partial void DocsExtraSql(List<DocEntry> all)
        {
            const string Q = "SQL";
            all.Add(X(Q, "Funções", "Contas e funções de texto", "No SELECT dá para calcular e transformar valores. Só a EXIBIÇÃO muda: a tabela continua igual.",
                "SELECT coluna * 2, UPPER(coluna), CONCAT(a, ' ', b) FROM tabela;",
                A("+  -  *  /", "contas com colunas e números (/ sempre dá casas decimais; DIV é a divisão inteira)", "UPPER(t) / LOWER(t)", "maiúsculas / minúsculas",
                  "CONCAT(a, b, ...)", "junta textos (NULL em qualquer pedaço = NULL)", "CONCAT_WS(sep, a, b)", "junta com separador e pula os NULL",
                  "CHAR_LENGTH(t)", "quantidade de caracteres", "LENGTH(t)", "quantidade de BYTES (em UTF-8, 'ç' e 'ã' ocupam 2)",
                  "SUBSTRING_INDEX(t, sep, n)", "o que vem antes da n-ésima ocorrência de sep (n negativo conta do fim)", "LEFT(t, n) / RIGHT(t, n)", "os n primeiros / últimos caracteres",
                  "REPLACE(t, de, para)", "troca trechos do texto", "ROUND(x, casas)", "arredonda (ROUND(x) = inteiro)"),
                A("SELECT titulo, paginas * 2 FROM livros;", "conta linha a linha", "SELECT CONCAT(titulo, ' (', ano, ')') FROM livros;", "'Dom Casmurro (1899)'",
                  "SELECT nome FROM alunos WHERE CHAR_LENGTH(nome) > 20;", "nomes com mais de 20 caracteres",
                  "SELECT SUBSTRING_INDEX(email, '@', -1) FROM alunos;", "só o domínio do e-mail", "SELECT ROUND(AVG(nota), 1) FROM provas;", "média com 1 casa"),
                "Para um apelido de coluna calculada use AS: SELECT preco_capa * 0.8 AS preco_promo FROM livros;"));
            all.Add(X(Q, "Funções", "NULL na prática: COALESCE / IFNULL / IF", "Funções para trocar NULL por um valor e para escolher entre duas saídas.",
                "COALESCE(a, b, c, ...)\nIFNULL(a, b)\nIF(condição, se_verdadeiro, se_falso)",
                A("COALESCE(a, b, ...)", "o primeiro argumento que não é NULL", "IFNULL(a, b)", "a, ou b se a for NULL (só dois argumentos)", "IF(c, x, y)", "x se a condição c for verdadeira; senão y",
                  "NULLIF(a, b)", "NULL se a = b (útil para evitar divisão por zero)"),
                A("SELECT nome, COALESCE(telefone, 'sem telefone') AS fone FROM alunos;", "troca NULL por um texto", "SELECT titulo, IF(emprestado = 1, 'fora', 'na estante') FROM livros;", "duas saídas",
                  "SELECT total / NULLIF(qtd, 0) FROM vendas;", "dividir por zero vira NULL em vez de erro"),
                "Agregações (SUM, AVG, COUNT(coluna)) ignoram NULL. COUNT(*) conta todas as linhas."));
            all.Add(X(Q, "Funções", "Datas: CURDATE, DATEDIFF, DATE_SUB, TIMESTAMPDIFF", "Funções para pegar a data de hoje, extrair partes e fazer contas com datas.",
                "CURDATE()  NOW()\nYEAR(d)  MONTH(d)  DAY(d)\nDATEDIFF(d1, d2)\nDATE_ADD(d, INTERVAL n UNIDADE)   DATE_SUB(d, INTERVAL n UNIDADE)\nTIMESTAMPDIFF(UNIDADE, inicio, fim)",
                A("CURDATE() / NOW()", "data de hoje / data e hora de agora", "YEAR, MONTH, DAY", "partes da data (MONTH('2026-03-15') = 3)", "DATEDIFF(d1, d2)", "dias entre d2 e d1 (d1 - d2)",
                  "INTERVAL n UNIDADE", "SECOND, MINUTE, HOUR, DAY, WEEK, MONTH, YEAR (no singular)", "TIMESTAMPDIFF(MINUTE, a, b)", "diferença em minutos (ou HOUR, DAY, MONTH...)",
                  "DATE_FORMAT(d, '%d/%m/%Y')", "formata a data para exibir"),
                A("SELECT titulo FROM emprestimos WHERE devolucao < CURDATE();", "empréstimos vencidos", "SELECT nome, DATEDIFF(CURDATE(), matricula) FROM alunos;", "dias desde a matrícula",
                  "SELECT * FROM emprestimos WHERE retirada >= DATE_SUB(CURDATE(), INTERVAL 30 DAY);", "últimos 30 dias",
                  "SELECT AVG(TIMESTAMPDIFF(HOUR, retirada, devolucao)) FROM emprestimos;", "tempo médio em horas"),
                "Datas são escritas entre aspas no formato 'AAAA-MM-DD' (e 'AAAA-MM-DD hh:mm:ss' com hora). Também vale data - INTERVAL 7 DAY."));
            all.Add(X(Q, "Consultas", "Paginação e ordem com vários critérios", "ORDER BY aceita várias colunas (cada uma com ASC/DESC). LIMIT + OFFSET mostram uma 'página' do resultado.",
                "... ORDER BY col1 [DESC], col2 [DESC]\n... LIMIT quantidade OFFSET pular\n... LIMIT pular, quantidade",
                A("ORDER BY a, b", "ordena por a; b só desempata", "LIMIT 10", "só 10 linhas", "OFFSET 20", "pula as 20 primeiras", "LIMIT 20, 10", "atalho do MySQL: pula 20, mostra 10"),
                A("SELECT titulo, autor FROM livros ORDER BY autor, ano DESC;", "por autor e, dentro dele, do mais novo ao mais antigo",
                  "SELECT titulo FROM livros ORDER BY titulo LIMIT 10 OFFSET 20;", "3ª página de 10 livros"),
                "Paginação sem ORDER BY repete ou pula linhas. Página n de t itens: LIMIT t OFFSET (n - 1) * t."));
            all.Add(X(Q, "Alterar dados", "INSERT com valor padrão (DEFAULT)", "Coluna que fica fora da lista do INSERT recebe o valor DEFAULT dela (ou NULL, se não tiver).",
                "INSERT INTO tabela (col1, col2) VALUES (v1, v2);\nINSERT INTO tabela (col1, col2, col3) VALUES (v1, DEFAULT, v3);", null,
                A("INSERT INTO emprestimos (aluno_id, livro_id) VALUES (3, 12);", "devolvido fica com o DEFAULT (ex.: 0)",
                  "INSERT INTO emprestimos (aluno_id, livro_id, devolvido) VALUES (3, 12, DEFAULT);", "pede o padrão de forma explícita"),
                "Confira os padrões com DESCRIBE tabela; (coluna Default). Coluna NOT NULL sem DEFAULT e fora do INSERT dá o erro 1364."));
            all.Add(X(Q, "Alterar dados", "UPDATE com CASE ou subconsulta", "O SET pode escolher o valor conforme a linha (CASE), e o WHERE pode usar a lista de outra consulta.",
                "UPDATE t SET col = CASE WHEN cond THEN v1 ELSE v2 END WHERE ...;\nUPDATE t SET col = ... WHERE id IN (SELECT ... FROM outra);\nDELETE FROM t WHERE id IN (SELECT ... FROM outra);", null,
                A("UPDATE livros SET preco = CASE WHEN genero = 'infantil' THEN preco * 0.9 ELSE preco * 0.95 END WHERE genero IN ('infantil', 'poesia');", "descontos diferentes num comando só",
                  "UPDATE alunos SET bolsa = 1 WHERE id IN (SELECT aluno_id FROM medalhas);", "usa a lista de outra tabela",
                  "UPDATE alunos SET turma = '3A', ativo = 1 WHERE id = 7;", "várias colunas no mesmo SET"),
                "O MySQL não deixa a subconsulta ler a MESMA tabela que está sendo alterada (erro 1093). E UPDATE/DELETE sem WHERE atinge a tabela inteira!"));
            all.Add(X(Q, "Juntar tabelas", "JOIN: casos avançados", "Self join (a tabela com ela mesma), várias tabelas em sequência e contagem com LEFT JOIN.",
                "FROM t a JOIN t b ON b.id = a.pai_id\nFROM a JOIN b ON b.id = a.b_id JOIN c ON c.id = b.c_id\nSELECT x.nome, COUNT(y.id) FROM x LEFT JOIN y ON y.x_id = x.id GROUP BY x.id, x.nome",
                A("self join", "a mesma tabela duas vezes, com apelidos diferentes (hierarquias)", "JOIN encadeado", "cada tabela nova se liga a uma que já está na consulta (um ON por JOIN)",
                  "COUNT(y.id) com LEFT JOIN", "conta 0 para quem não tem par (COUNT(*) contaria 1)", "sem ON", "produto cartesiano: todas as combinações (resultado multiplicado!)"),
                A("SELECT c.nome, p.nome AS categoria_pai FROM categorias c LEFT JOIN categorias p ON p.id = c.pai_id;", "cada categoria com a categoria-pai",
                  "SELECT a.nome, l.titulo FROM emprestimos e JOIN alunos a ON a.id = e.aluno_id JOIN livros l ON l.id = e.livro_id;", "três tabelas",
                  "SELECT a.nome, COUNT(e.id) FROM alunos a LEFT JOIN emprestimos e ON e.aluno_id = a.id GROUP BY a.id, a.nome;", "empréstimos por aluno, com zeros"),
                "Ligue sempre chave estrangeira com chave primária (emprestimos.aluno_id = alunos.id). Ligar id com id 'funciona', mas junta linhas que não têm nada a ver."));
            all.Add(X(Q, "Juntar tabelas", "UNION / UNION ALL", "Empilha o resultado de dois (ou mais) SELECTs numa lista só.",
                "SELECT col FROM a\nUNION [ALL]\nSELECT col FROM b\n[ORDER BY 1];",
                A("UNION", "junta e remove as linhas repetidas", "UNION ALL", "junta e mantém tudo (mais rápido)", "mesmas colunas", "os SELECTs precisam ter a mesma quantidade de colunas (erro 1222)",
                  "ORDER BY no fim", "vale para o resultado inteiro (use o nome da 1ª consulta ou a posição)"),
                A("SELECT nome FROM alunos UNION SELECT nome FROM professores;", "todos os nomes, sem repetir", "SELECT email FROM alunos UNION ALL SELECT email FROM ex_alunos ORDER BY 1;", "com repetidos, ordenado"),
                "Os nomes das colunas do resultado vêm do PRIMEIRO SELECT."));
            all.Add(X(Q, "Estrutura", "CREATE VIEW", "Uma VIEW é uma consulta guardada com nome. Ela é lida como uma tabela, mas roda a consulta de novo a cada leitura (os dados ficam nas tabelas de origem).",
                "CREATE VIEW nome AS SELECT ...;\nCREATE OR REPLACE VIEW nome AS SELECT ...;\nDROP VIEW nome;", null,
                A("CREATE VIEW vw_acervo AS SELECT titulo, autor FROM livros;", "esconde colunas sensíveis ou internas",
                  "CREATE VIEW vw_leitores AS SELECT a.nome, COUNT(*) AS lidos FROM alunos a JOIN emprestimos e ON e.aluno_id = a.id GROUP BY a.id, a.nome;", "relatório pronto para o BI",
                  "SELECT * FROM vw_leitores WHERE lidos > 3;", "usa a view como se fosse uma tabela", "SHOW FULL TABLES;", "lista tabelas e views (Table_type)"),
                "Views simplificam consultas repetidas e servem para segurança: dê ao sistema acesso só à view, não à tabela inteira."));
            all.Add(X(Q, "Estrutura", "Copiar tabela (backup)", "Cria uma tabela nova a partir de outra: com os dados (AS SELECT) ou só com a estrutura (LIKE).",
                "CREATE TABLE nova AS SELECT * FROM original;\nCREATE TABLE nova LIKE original;\nINSERT INTO nova SELECT * FROM original;", null,
                A("CREATE TABLE livros_2025 AS SELECT * FROM livros WHERE ano = 2025;", "cópia de parte dos dados", "CREATE TABLE alunos_bkp LIKE alunos;", "mesma estrutura (com chaves e índices), vazia",
                  "INSERT INTO alunos_bkp SELECT * FROM alunos;", "copia as linhas para a tabela vazia"),
                "AS SELECT não copia PRIMARY KEY, AUTO_INCREMENT nem índices. Antes de um UPDATE/DELETE grande, um backup salva o seu dia."));
            all.Add(X(Q, "Estrutura", "ALTER TABLE: mudar colunas e restrições", "Renomear, redefinir colunas e acrescentar restrições numa tabela que já tem dados.",
                "ALTER TABLE t RENAME COLUMN antigo TO novo;\nALTER TABLE t MODIFY COLUMN col TIPO [NOT NULL] [DEFAULT ...];\nALTER TABLE t CHANGE antigo novo TIPO ...;\nALTER TABLE t ADD CONSTRAINT nome FOREIGN KEY (col) REFERENCES outra(id);\nALTER TABLE t ADD CONSTRAINT nome CHECK (condição);",
                A("RENAME COLUMN", "só troca o nome (dados e tipo ficam)", "MODIFY", "redefine a coluna INTEIRA: o que não for repetido (NOT NULL, DEFAULT) some", "CHANGE", "como MODIFY, mas também renomeia",
                  "ADD CONSTRAINT ... FOREIGN KEY", "cria a chave; as linhas que já existem precisam respeitá-la (senão, erro 1452)", "ADD CONSTRAINT ... CHECK", "cria a regra; linhas que já a violam dão erro 3819",
                  "DROP FOREIGN KEY nome / DROP CHECK nome", "remove a restrição pelo nome"),
                A("ALTER TABLE livros RENAME COLUMN autor TO autoria;", "novo nome", "ALTER TABLE alunos MODIFY COLUMN nome VARCHAR(200) NOT NULL;", "aumenta o tamanho mantendo NOT NULL",
                  "ALTER TABLE emprestimos ADD CONSTRAINT fk_emp_livro FOREIGN KEY (livro_id) REFERENCES livros(id);", "chave estrangeira com nome",
                  "ALTER TABLE provas ADD CONSTRAINT chk_nota CHECK (nota >= 0);", "regra de validação"),
                "Antes de um MODIFY, rode SHOW CREATE TABLE t; para copiar a definição atual. Dê nomes às restrições: eles aparecem nos erros e permitem apagá-las."));
            all.Add(X(Q, "Estrutura", "CHECK e ON DELETE CASCADE", "CHECK valida os valores de cada linha. ON DELETE diz o que acontece com os 'filhos' quando o 'pai' é apagado.",
                "col INT NOT NULL CHECK (col BETWEEN 1 AND 10)\nCONSTRAINT nome CHECK (condição)\nFOREIGN KEY (col) REFERENCES pai(id) ON DELETE CASCADE | SET NULL | RESTRICT",
                A("CHECK (cond)", "recusa linha com a condição FALSA (erro 3819); NULL passa", "ON DELETE CASCADE", "apagou o pai, apaga os filhos junto",
                  "ON DELETE SET NULL", "apagou o pai, a coluna do filho vira NULL (não pode ser NOT NULL)", "RESTRICT (padrão)", "recusa apagar o pai que tem filhos (erro 1451)"),
                A("CREATE TABLE provas (id INT PRIMARY KEY AUTO_INCREMENT, nota DECIMAL(4,2) NOT NULL CHECK (nota BETWEEN 0 AND 10));", "nota de 0 a 10",
                  "CREATE TABLE comentarios (id INT PRIMARY KEY AUTO_INCREMENT, livro_id INT NOT NULL, texto VARCHAR(300), FOREIGN KEY (livro_id) REFERENCES livros(id) ON DELETE CASCADE);",
                  "comentários somem junto com o livro"),
                "No MySQL 8, REFERENCES escrito na própria coluna é aceito mas IGNORADO: declare FOREIGN KEY (...) separado. CHECK só é respeitado a partir do 8.0.16."));
            all.Add(X(Q, "Estrutura", "Índices únicos e compostos", "UNIQUE impede valores repetidos. Um índice composto cobre várias colunas, na ordem em que foram listadas.",
                "CREATE UNIQUE INDEX nome ON tabela (coluna);\nCREATE INDEX nome ON tabela (col1, col2);\nDROP INDEX nome ON tabela;", null,
                A("CREATE UNIQUE INDEX idx_alunos_email ON alunos (email);", "dois alunos não podem ter o mesmo e-mail (erro 1062)",
                  "CREATE INDEX idx_emp_aluno_data ON emprestimos (aluno_id, retirada);", "acelera WHERE aluno_id = ? e WHERE aluno_id = ? AND retirada > ?",
                  "EXPLAIN SELECT * FROM emprestimos WHERE aluno_id = 3;", "a coluna key mostra o índice usado"),
                "Regra do prefixo da esquerda: o índice (a, b) serve para filtros em a, ou em a e b — mas não para filtros só em b."));
            all.Add(X(Q, "Estrutura", "Transações na prática", "Comandos que precisam acontecer juntos vão entre START TRANSACTION e COMMIT. Até o COMMIT, ROLLBACK desfaz tudo.",
                "START TRANSACTION;   (ou BEGIN;)\nUPDATE ...;\nDELETE ...;\nCOMMIT;   (ou ROLLBACK;)", null,
                A("START TRANSACTION; UPDATE contas SET saldo = saldo - 50 WHERE id = 1; UPDATE contas SET saldo = saldo + 50 WHERE id = 2; COMMIT;", "transferência: as duas mudanças ou nenhuma",
                  "START TRANSACTION; DELETE FROM emprestimos WHERE aluno_id = 9; DELETE FROM alunos WHERE id = 9; COMMIT;", "apaga primeiro os 'filhos', depois o 'pai'"),
                "Comandos de estrutura (CREATE, ALTER, DROP) fazem COMMIT automático da transação aberta: não dá para desfazê-los com ROLLBACK."));
        }
    }
}
