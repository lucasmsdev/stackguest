using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerQuest
{
    static partial class Challenges
    {
        // ───────────────────────────── SQL: conteúdo extra (s16 … s30) ─────────────────────────────
        // Rebalanceamento: 10 desafios por nível. Todos rodam no banco cafe365 (SqlSample) sem mudar os dados de exemplo.
        static partial void MaisSql(List<Level> list)
        {
            Action<string, int> Nivel = (titulo, tier) =>
            {
                var lv = list.First(l => l.Cat == "sql" && l.Title == titulo);
                lv.Tier = tier;
                lv.Chapter = Cat("sql").Name + " · " + TierNames[tier];
            };
            Nivel("Clientes que começam com Ma", 0);    // LIKE: um conceito só, uma linha
            Nivel("Pedidos com nome do cliente", 1);    // JOIN simples, de duas tabelas
            Nivel("Tabela de avaliações", 1);           // CREATE TABLE simples, sem chave estrangeira

            // ───── Iniciante
            list.Add(SqlL(0, "s16", "query", "Cidades sem repetir",
                "O Fábio, analista de BI, vai montar um mapa com as cidades onde o Café 365 tem clientes. Vários clientes moram na mesma cidade.",
                "Liste as cidades (coluna cidade) da tabela clientes, mostrando cada cidade uma única vez.",
                "SELECT DISTINCT cidade FROM clientes;",
                A("Existe uma palavra que, logo depois do SELECT, remove as linhas repetidas do resultado.", "SELECT DISTINCT coluna FROM tabela;"),
                A("SELECT DISTINCT", "Mostra cada combinação de valores uma vez só (remove as linhas repetidas).", "cidade", "A coluna que não pode repetir.",
                  "FROM clientes", "Da tabela clientes."),
                "Para saber QUANTAS cidades diferentes existem: SELECT COUNT(DISTINCT cidade) FROM clientes;"));
            list.Add(SqlL(0, "s17", "query", "Cliente sem e-mail",
                "A Tânia, DBA, vai mandar a newsletter do Café 365 e precisa saber quem ficou sem e-mail no cadastro (o campo email está vazio: NULL).",
                "Liste o nome dos clientes que não têm e-mail cadastrado.",
                "SELECT nome FROM clientes WHERE email IS NULL;",
                A("NULL significa 'sem valor'. Ele não é igual a nada, nem a outro NULL: por isso '= NULL' nunca encontra nada.", "... WHERE email IS NULL;"),
                A("WHERE email", "Filtra pela coluna email.", "IS NULL", "Verdadeiro quando a coluna não tem valor. O contrário é IS NOT NULL."),
                "Armadilha clássica: WHERE email = NULL não dá erro, mas devolve ZERO linhas. Com NULL, sempre IS NULL / IS NOT NULL."));
            list.Add(SqlL(0, "s18", "query", "Faixa de preço",
                "O app vai ter um filtro 'até 8 reais' com preço mínimo de 5 reais, e a Glória quer ver o que aparece nele.",
                "Liste o nome e o preço dos produtos com preço de 5.00 a 8.00, incluindo os dois valores (5.00 e 8.00 entram).",
                "SELECT nome, preco FROM produtos WHERE preco BETWEEN 5.00 AND 8.00;",
                A("Existe um operador que testa se um valor está ENTRE dois limites (e inclui os limites).", "... WHERE preco BETWEEN valor1 AND valor2;"),
                A("BETWEEN 5.00 AND 8.00", "Entre 5.00 e 8.00, INCLUINDO as pontas. É o mesmo que preco >= 5.00 AND preco <= 8.00."),
                "Cuidado com > e <: preco > 5 AND preco < 8 deixaria de fora o Pão de queijo (5.00) e o Suco de laranja (8.00)."));
            list.Add(SqlL(0, "s19", "query", "Top 3 mais caros",
                "A vitrine do app vai destacar só os 3 produtos mais caros do cardápio.",
                "Liste o nome e o preço dos 3 produtos mais caros, do mais caro para o menos caro.",
                "SELECT nome, preco FROM produtos ORDER BY preco DESC LIMIT 3;",
                A("Primeiro ordene do mais caro para o mais barato; depois corte o resultado nas primeiras linhas.", "... ORDER BY preco DESC LIMIT 3;"),
                A("ORDER BY preco DESC", "Ordena do maior preço para o menor.", "LIMIT 3", "Devolve só as 3 primeiras linhas (o LIMIT vem sempre no fim)."),
                "LIMIT sem ORDER BY devolve 3 linhas quaisquer! Para 'top N', ordene primeiro. Paginação: LIMIT 10 OFFSET 20 pula 20 e mostra 10.",
                true));

            // ───── Intermediário
            list.Add(SqlL(1, "s20", "state", "Duas sobremesas novas",
                "A confeitaria lançou dois doces e a Glória quer os dois no cardápio num comando só (cada comando a mais é uma ida e volta ao servidor).",
                "Com um ÚNICO comando INSERT, cadastre na tabela produtos: 'Pudim' (categoria 'doce', preço 6.50, estoque 25) e 'Torta de limão' (categoria 'doce', preço 9.00, estoque 12).",
                "INSERT INTO produtos (nome, categoria, preco, estoque) VALUES ('Pudim', 'doce', 6.50, 25), ('Torta de limão', 'doce', 9.00, 12);",
                A("Um INSERT pode receber várias linhas: depois do VALUES, coloque vários grupos de parênteses separados por vírgula.",
                  "INSERT INTO produtos (nome, categoria, preco, estoque) VALUES (...), (...);"),
                A("INSERT INTO produtos (nome, categoria, preco, estoque)", "As colunas que vão receber valores (o id é automático).",
                  "VALUES ('Pudim', ...), ('Torta de limão', ...)", "Uma linha por grupo de parênteses, separados por vírgula, na mesma ordem das colunas."),
                "Inserir em lote (várias linhas por INSERT) é muito mais rápido do que um INSERT por linha quando são milhares de registros.",
                false, A("produtos")));
            list.Add(SqlL(1, "s21", "state", "Reajuste em lote",
                "O preço do leite e do chocolate subiu. O Fábio calculou: bebidas e doces precisam ficar 10% mais caros; os salgados não mudam.",
                "Com um único UPDATE, aumente em 10% o preço (novo preço = preço atual vezes 1.10) de todos os produtos das categorias 'bebida' e 'doce'.",
                "UPDATE produtos SET preco = preco * 1.10 WHERE categoria IN ('bebida', 'doce');",
                A("O novo valor pode ser uma conta com o valor atual da própria coluna. Para filtrar duas categorias, use IN (...) ou OR.",
                  "UPDATE produtos SET preco = preco * 1.10 WHERE categoria IN (...);"),
                A("SET preco = preco * 1.10", "Cada linha usa o SEU preço atual na conta (6.50 vira 7.15, por exemplo).",
                  "WHERE categoria IN ('bebida', 'doce')", "Só as linhas cuja categoria está na lista. É o mesmo que categoria = 'bebida' OR categoria = 'doce'."),
                "Armadilha: WHERE categoria = 'bebida' AND categoria = 'doce' não altera nada (uma linha não tem duas categorias ao mesmo tempo). E sem WHERE, os salgados também subiriam!",
                false, A("produtos")));
            list.Add(SqlL(1, "s22", "query", "Mais barato e mais caro",
                "O Fábio vai montar um gráfico de faixa de preço por categoria e o sistema de BI lê as colunas PELO NOME.",
                "Para cada categoria, mostre a categoria, o menor preço numa coluna chamada mais_barato e o maior preço numa coluna chamada mais_caro.",
                "SELECT categoria, MIN(preco) AS mais_barato, MAX(preco) AS mais_caro FROM produtos GROUP BY categoria;",
                A("MIN e MAX calculam o menor e o maior valor de cada grupo. Para dar nome a uma coluna do resultado, use AS.",
                  "SELECT categoria, MIN(preco) AS ..., MAX(preco) AS ... FROM produtos GROUP BY categoria;"),
                A("MIN(preco) AS mais_barato", "Menor preço do grupo, com o nome (apelido) mais_barato.", "MAX(preco) AS mais_caro", "Maior preço do grupo, com o nome mais_caro.",
                  "GROUP BY categoria", "Uma linha por categoria."),
                "Apelidos deixam relatórios legíveis e são obrigatórios quando um sistema (BI, API, planilha) lê as colunas pelo nome. Sem AS, a coluna se chamaria 'MIN(preco)'."));
            SqlTarefa("s22").Headers = true;
            list.Add(SqlL(1, "s23", "schema", "Clientes ativos",
                "Em vez de apagar clientes, a Glória quer 'exclusão lógica': marcar quem está ativo. Todos os clientes atuais devem continuar ativos.",
                "Adicione à tabela clientes a coluna ativo, do tipo BOOLEAN, obrigatória (NOT NULL) e com valor padrão TRUE.",
                "ALTER TABLE clientes ADD COLUMN ativo BOOLEAN NOT NULL DEFAULT TRUE;",
                A("Para mudar a estrutura de uma tabela que já existe, use ALTER TABLE ... ADD COLUMN.", "ALTER TABLE clientes ADD COLUMN ativo BOOLEAN NOT NULL DEFAULT ...;"),
                A("ALTER TABLE clientes", "Altera a estrutura da tabela clientes (os dados continuam lá).", "ADD COLUMN ativo BOOLEAN", "Nova coluna verdadeiro/falso (no MySQL, BOOLEAN = TINYINT(1): 1 ou 0).",
                  "NOT NULL DEFAULT TRUE", "Obrigatória e, quando ninguém informar, vale TRUE (1). As linhas que já existiam recebem o padrão."),
                "Sem o DEFAULT, uma coluna NOT NULL nova não teria valor para as linhas antigas. Confira com DESCRIBE clientes; e SELECT nome, ativo FROM clientes;",
                false, null, SqlCheckAtivo));

            // ───── Profissional
            list.Add(SqlL(2, "s24", "query", "Acima da média",
                "A Glória quer saber quais produtos puxam o preço do cardápio para cima. A média muda toda vez que um preço muda, então ela não pode ser digitada à mão.",
                "Liste o nome e o preço dos produtos cujo preço é maior que a média de preço de todos os produtos, calculando a média dentro da própria consulta.",
                "SELECT nome, preco FROM produtos WHERE preco > (SELECT AVG(preco) FROM produtos);",
                A("AVG não pode ir direto no WHERE. Mas uma consulta inteira entre parênteses pode virar um valor (subconsulta).",
                  "... WHERE preco > (SELECT AVG(preco) FROM produtos);"),
                A("(SELECT AVG(preco) FROM produtos)", "Subconsulta escalar: roda primeiro e devolve UM valor (a média, 6.80).", "WHERE preco > (...)",
                  "Compara o preço de cada produto com esse valor."),
                "WHERE preco > AVG(preco) dá o erro 1111 (Invalid use of group function): o WHERE olha uma linha por vez e não enxerga agregações."));
            list.Add(SqlL(2, "s25", "query", "Situação do estoque",
                "A Tânia quer um relatório para a reposição. O sistema da cozinha lê a coluna pelo nome situacao e espera exatamente os textos combinados.",
                "Liste o nome de cada produto e uma coluna chamada situacao com: 'esgotado' quando o estoque for 0; 'repor' quando o estoque for menor que 50; 'ok' nos outros casos.",
                "SELECT nome, CASE WHEN estoque = 0 THEN 'esgotado' WHEN estoque < 50 THEN 'repor' ELSE 'ok' END AS situacao FROM produtos;",
                A("CASE WHEN ... THEN ... ELSE ... END funciona como um if/else dentro do SELECT. A ORDEM dos WHEN importa: vale o primeiro que for verdadeiro.",
                  "CASE WHEN estoque = 0 THEN 'esgotado' WHEN ... THEN 'repor' ELSE 'ok' END AS situacao"),
                A("CASE", "Começa a expressão condicional.", "WHEN estoque = 0 THEN 'esgotado'", "Testado primeiro: estoque zerado.",
                  "WHEN estoque < 50 THEN 'repor'", "Só é testado se o anterior falhou (0 também é < 50, por isso ele vem depois).", "ELSE 'ok' END AS situacao",
                  "Valor padrão, fim do CASE e o nome da coluna."),
                "Se o WHEN estoque < 50 viesse primeiro, a Coxinha (estoque 0) apareceria como 'repor'. Sem ELSE, os casos não previstos viram NULL."));
            SqlTarefa("s25").Headers = true;
            list.Add(SqlL(2, "s26", "query", "Ranking de clientes",
                "O programa de fidelidade vai premiar quem mais gastou. Só contam pedidos com status 'entregue' (pendente ainda não foi pago; cancelado não vale).",
                "Liste o NOME de cada cliente e a soma do total dos seus pedidos 'entregue', do cliente que mais gastou para o que menos gastou. Quem não tem pedido entregue não aparece.",
                "SELECT c.nome, SUM(p.total) FROM clientes c JOIN pedidos p ON p.cliente_id = c.id WHERE p.status = 'entregue' GROUP BY c.id, c.nome ORDER BY SUM(p.total) DESC;",
                A("Junte clientes e pedidos (JOIN), filtre as LINHAS pelo status (WHERE), agrupe por cliente e ordene pela soma.",
                  "SELECT c.nome, SUM(p.total) FROM clientes c JOIN pedidos p ON ... WHERE p.status = 'entregue' GROUP BY c.id, c.nome ORDER BY SUM(p.total) DESC;"),
                A("JOIN pedidos p ON p.cliente_id = c.id", "Cada pedido ganha o nome do seu cliente.", "WHERE p.status = 'entregue'", "Filtra os pedidos ANTES de agrupar.",
                  "GROUP BY c.id, c.nome", "Um grupo por cliente (agrupar pelo id evita juntar dois clientes com o mesmo nome).",
                  "ORDER BY SUM(p.total) DESC", "Ordena pelo valor agregado, do maior para o menor (também vale o apelido: ORDER BY gasto DESC)."),
                "Ordem lógica: FROM/JOIN → WHERE → GROUP BY → HAVING → SELECT → ORDER BY → LIMIT. Por isso o WHERE não vê o SUM, mas o ORDER BY vê.",
                true));
            list.Add(SqlL(2, "s27", "query", "Nenhuma compra entregue",
                "O marketing quer reconquistar quem nunca recebeu um pedido: quem nunca comprou e quem só tem pedidos pendentes ou cancelados.",
                "Liste o nome dos clientes que NÃO têm nenhum pedido com status 'entregue' (inclusive quem não tem pedido nenhum).",
                "SELECT c.nome FROM clientes c WHERE NOT EXISTS (SELECT 1 FROM pedidos p WHERE p.cliente_id = c.id AND p.status = 'entregue');",
                A("Pense ao contrário: para cada cliente, EXISTE algum pedido entregue dele? Fique com quem a resposta é 'não'. Uma subconsulta pode usar colunas da consulta de fora.",
                  "... FROM clientes c WHERE NOT EXISTS (SELECT 1 FROM pedidos p WHERE p.cliente_id = c.id AND p.status = 'entregue');"),
                A("NOT EXISTS (...)", "Verdadeiro quando a subconsulta não devolve nenhuma linha.", "p.cliente_id = c.id", "Subconsulta correlacionada: usa o cliente da linha de fora (c).",
                  "AND p.status = 'entregue'", "Só os pedidos entregues contam; os outros não 'salvam' o cliente."),
                "Armadilha: JOIN pedidos ... WHERE p.status <> 'entregue' traz a Ana (que tem um pedido pendente, mas também tem entregues) e esquece quem nunca comprou. " +
                "Equivalentes corretos: NOT IN (SELECT cliente_id FROM pedidos WHERE status = 'entregue') ou LEFT JOIN com o status no ON."));
            list.Add(SqlL(2, "s28", "query", "Destaques da categoria",
                "O Fábio notou que comparar um brigadeiro com um cappuccino não faz sentido. Ele quer os produtos caros DENTRO da própria categoria.",
                "Liste nome, categoria e preço dos produtos cujo preço é maior que a média de preço da PRÓPRIA categoria deles.",
                "SELECT p.nome, p.categoria, p.preco FROM produtos p WHERE p.preco > (SELECT AVG(p2.preco) FROM produtos p2 WHERE p2.categoria = p.categoria);",
                A("A média muda para cada linha: use uma subconsulta CORRELACIONADA, que usa a categoria da linha de fora. Dê apelidos diferentes para a mesma tabela.",
                  "... FROM produtos p WHERE p.preco > (SELECT AVG(p2.preco) FROM produtos p2 WHERE p2.categoria = p.categoria);"),
                A("FROM produtos p", "Consulta de fora, com o apelido p.", "(SELECT AVG(p2.preco) FROM produtos p2", "A mesma tabela de novo, com outro apelido (p2).",
                  "WHERE p2.categoria = p.categoria)", "Correlação: a média só da categoria do produto que está sendo testado.", "p.preco > (...)",
                  "Fica quem custa mais que a média da sua categoria."),
                "Subconsultas correlacionadas rodam uma vez por linha de fora: em tabelas grandes, um índice na coluna da correlação (categoria) faz muita diferença."));
            list.Add(SqlL(2, "s29", "schema", "Itens do pedido",
                "Hoje o pedido só guarda o total, sem saber O QUE foi comprado. A Glória desenhou a tabela de itens e pediu para você criá-la com todas as regras no próprio banco.",
                "Num único CREATE TABLE, crie itens_pedido com: id (INT, chave primária, auto incremento); pedido_id (INT, NOT NULL, chave estrangeira para pedidos.id); " +
                "produto_id (INT, NOT NULL, chave estrangeira para produtos.id); quantidade (INT, NOT NULL, padrão 1); e uma restrição UNIQUE na combinação (pedido_id, produto_id), " +
                "para o mesmo produto não aparecer duas vezes no mesmo pedido.",
                "CREATE TABLE itens_pedido (id INT PRIMARY KEY AUTO_INCREMENT, pedido_id INT NOT NULL, produto_id INT NOT NULL, quantidade INT NOT NULL DEFAULT 1, " +
                "FOREIGN KEY (pedido_id) REFERENCES pedidos(id), FOREIGN KEY (produto_id) REFERENCES produtos(id), UNIQUE (pedido_id, produto_id));",
                A("Declare primeiro as quatro colunas (com NOT NULL e DEFAULT). Depois, ainda dentro dos parênteses, venham as restrições: FOREIGN KEY (...) REFERENCES tabela(coluna) e UNIQUE (col1, col2).",
                  "..., quantidade INT NOT NULL DEFAULT 1, FOREIGN KEY (pedido_id) REFERENCES pedidos(id), FOREIGN KEY (produto_id) REFERENCES produtos(id), UNIQUE (pedido_id, produto_id));"),
                A("id INT PRIMARY KEY AUTO_INCREMENT", "Chave primária numerada sozinha.", "pedido_id INT NOT NULL, produto_id INT NOT NULL", "Obrigatórias: todo item pertence a um pedido e a um produto.",
                  "quantidade INT NOT NULL DEFAULT 1", "Se o INSERT não informar, vale 1.", "FOREIGN KEY (pedido_id) REFERENCES pedidos(id)",
                  "O banco recusa itens de pedidos que não existem (e impede apagar um pedido que tem itens).", "UNIQUE (pedido_id, produto_id)",
                  "Restrição composta: o PAR não pode repetir (o mesmo produto pode estar em pedidos diferentes)."),
                "Regras no banco valem para TODOS os sistemas que gravam nele (app, site, scripts). Teste: INSERT INTO itens_pedido (pedido_id, produto_id) VALUES (99, 1); deve dar o erro 1452.",
                false, null, SqlCheckItensPedido));
            list.Add(SqlL(2, "s30", "query", "Faturamento por cidade",
                "A diretoria vai decidir onde abrir a próxima cafeteria física e pediu à Glória o faturamento por cidade. Pedido cancelado não é faturamento.",
                "Para cada cidade, mostre a cidade, a quantidade de pedidos e a soma do total, considerando só os pedidos que NÃO estão 'cancelado'. " +
                "Mostre só as cidades com soma maior que 20.00, da maior soma para a menor.",
                "SELECT c.cidade, COUNT(*) AS pedidos, SUM(p.total) AS faturamento FROM clientes c JOIN pedidos p ON p.cliente_id = c.id WHERE p.status <> 'cancelado' " +
                "GROUP BY c.cidade HAVING SUM(p.total) > 20 ORDER BY faturamento DESC;",
                A("São cinco peças: JOIN (cidade fica em clientes, total em pedidos), WHERE (tira os cancelados ANTES de agrupar), GROUP BY cidade, HAVING (filtra os grupos pela soma) e ORDER BY.",
                  "SELECT c.cidade, COUNT(*), SUM(p.total) AS faturamento FROM clientes c JOIN pedidos p ON ... WHERE p.status <> 'cancelado' GROUP BY c.cidade HAVING SUM(p.total) > 20 ORDER BY faturamento DESC;"),
                A("JOIN pedidos p ON p.cliente_id = c.id", "Liga cada pedido à cidade do cliente.", "WHERE p.status <> 'cancelado'", "Filtra LINHAS: os cancelados nem entram na conta.",
                  "GROUP BY c.cidade", "Um grupo por cidade.", "HAVING SUM(p.total) > 20", "Filtra GRUPOS depois de somar (o WHERE não enxerga o SUM).",
                  "ORDER BY faturamento DESC", "Ordena pelo apelido da soma, da maior para a menor."),
                "Sem o WHERE, Belo Horizonte entraria no relatório só com um pedido cancelado (25.40). WHERE decide quais linhas entram; HAVING decide quais grupos saem.",
                true));
        }

        static SqlTask SqlTarefa(string id) { return SqlTasks.All[id]; }

        static string SqlCheckAtivo(SqlDb db)
        {
            var t = db.Table("clientes");
            int k = t.ColIndex("ativo");
            if (k < 0) return "A tabela clientes ainda não tem a coluna ativo.";
            var c = t.Cols[k];
            if (c.Type != "BOOLEAN" && c.Type != "TINYINT") return "A coluna ativo precisa ser do tipo BOOLEAN (verdadeiro/falso).";
            if (!c.NotNull) return "A coluna ativo precisa ser obrigatória (NOT NULL).";
            if (c.Default == null || !(c.Default is decimal) || (decimal)c.Default != 1m) return "O valor padrão (DEFAULT) da coluna ativo precisa ser TRUE (1): todos os clientes atuais devem ficar ativos.";
            if (t.Rows.Any(r => r[k] == null || !(r[k] is decimal) || (decimal)r[k] != 1m)) return "Todos os clientes atuais precisam ficar com ativo = TRUE.";
            return null;
        }

        static string SqlCheckItensPedido(SqlDb db)
        {
            SqlTable t;
            if (!db.Tables.TryGetValue("itens_pedido", out t)) return "A tabela itens_pedido não foi criada (confira o nome: itens_pedido).";
            var nomes = new[] { "id", "pedido_id", "produto_id", "quantidade" };
            var faltam = nomes.Where(n => t.ColIndex(n) < 0).ToList();
            if (faltam.Count > 0) return "Faltou a coluna " + string.Join(", ", faltam) + " na tabela itens_pedido.";
            if (t.Cols.Count != 4) return "A tabela itens_pedido deve ter só as colunas id, pedido_id, produto_id e quantidade.";
            if (t.Cols.Any(c => !c.IsInt || c.Type == "BOOLEAN")) return "As quatro colunas devem ser do tipo INT.";
            var id = t.Cols[t.ColIndex("id")];
            if (!id.Pk) return "A coluna id precisa ser a chave primária (PRIMARY KEY).";
            if (!id.Auto) return "A coluna id precisa ser AUTO_INCREMENT.";
            foreach (var par in new[] { new[] { "pedido_id", "pedidos" }, new[] { "produto_id", "produtos" } })
            {
                var c = t.Cols[t.ColIndex(par[0])];
                if (!c.NotNull) return "A coluna " + par[0] + " precisa ser obrigatória (NOT NULL).";
                if (c.RefTable == null || !c.RefTable.Equals(par[1], StringComparison.OrdinalIgnoreCase) || !string.Equals(c.RefCol, "id", StringComparison.OrdinalIgnoreCase))
                    return "Faltou a chave estrangeira: FOREIGN KEY (" + par[0] + ") REFERENCES " + par[1] + "(id).";
            }
            var q = t.Cols[t.ColIndex("quantidade")];
            if (!q.NotNull) return "A coluna quantidade precisa ser obrigatória (NOT NULL).";
            if (q.Default == null || !(q.Default is decimal) || (decimal)q.Default != 1m) return "A coluna quantidade precisa ter valor padrão 1 (DEFAULT 1).";
            if (t.Cols.Any(c => c.Unique && !c.Pk))
                return "UNIQUE numa coluna só impediria um produto de aparecer em dois pedidos diferentes! A restrição é na COMBINAÇÃO: UNIQUE (pedido_id, produto_id).";
            bool par2 = t.Indexes.Any(i => i.Unique && i.Cols.Count == 2 &&
                new HashSet<string>(i.Cols, StringComparer.OrdinalIgnoreCase).SetEquals(new[] { "pedido_id", "produto_id" }));
            if (!par2) return "Faltou a restrição UNIQUE na combinação (pedido_id, produto_id).";
            return null;
        }
    }

    static partial class Docs
    {
        static partial void DocsMaisSql(List<DocEntry> all)
        {
            const string Q = "SQL";
            all.Add(X(Q, "Consultas", "DISTINCT", "Remove as linhas repetidas do resultado: cada combinação de valores aparece uma vez só.",
                "SELECT DISTINCT coluna FROM tabela;\nSELECT COUNT(DISTINCT coluna) FROM tabela;", null,
                A("SELECT DISTINCT turma FROM alunos;", "cada turma uma vez só", "SELECT DISTINCT autor, editora FROM livros;", "cada PAR autor + editora uma vez só",
                  "SELECT COUNT(DISTINCT autor) FROM livros;", "quantos autores diferentes existem"),
                "DISTINCT vale para a linha inteira do SELECT, não só para a primeira coluna."));
            all.Add(X(Q, "Consultas", "IS NULL / BETWEEN / IN", "Três filtros do WHERE muito usados: valor vazio, faixa de valores e lista de valores.",
                "... WHERE coluna IS NULL\n... WHERE coluna BETWEEN minimo AND maximo\n... WHERE coluna IN (valor1, valor2, ...)",
                A("IS NULL / IS NOT NULL", "sem valor / com valor (= NULL nunca funciona: NULL não é igual a nada)", "BETWEEN a AND b", "de a até b, INCLUINDO a e b",
                  "IN (...)", "igual a qualquer item da lista (atalho para vários OR)", "NOT BETWEEN / NOT IN", "o contrário"),
                A("SELECT nome FROM alunos WHERE telefone IS NULL;", "alunos sem telefone", "SELECT titulo FROM livros WHERE ano BETWEEN 1990 AND 1999;", "livros dos anos 90 (1990 e 1999 entram)",
                  "SELECT titulo FROM livros WHERE genero IN ('poesia', 'conto');", "livros de dois gêneros"),
                "WHERE telefone = NULL não dá erro, mas devolve ZERO linhas. Use sempre IS NULL."));
            all.Add(X(Q, "Consultas", "AS  (apelidos)", "Dá um nome a uma coluna do resultado (ou um nome curto a uma tabela).",
                "SELECT expressão AS nome_da_coluna FROM tabela;\nSELECT t.coluna FROM tabela t;", null,
                A("SELECT titulo AS livro FROM livros;", "a coluna aparece como 'livro'", "SELECT turma, AVG(nota) AS media, MAX(nota) AS melhor FROM provas GROUP BY turma;", "agregações com nomes legíveis",
                  "SELECT turma, COUNT(*) AS qtd FROM alunos GROUP BY turma ORDER BY qtd DESC;", "o apelido também pode ser usado no ORDER BY e no HAVING"),
                "Sem apelido, a coluna se chama como a expressão (ex.: 'AVG(nota)'). Relatórios, APIs e ferramentas de BI leem as colunas pelo nome."));
            all.Add(X(Q, "Consultas", "CASE WHEN", "Um if/else dentro do SELECT: devolve um valor diferente conforme a condição.",
                "CASE\n  WHEN condição1 THEN valor1\n  WHEN condição2 THEN valor2\n  ELSE valor_padrão\nEND",
                A("WHEN … THEN", "testados em ordem: vale o PRIMEIRO verdadeiro", "ELSE", "quando nenhum WHEN vale (sem ELSE, o resultado é NULL)", "END", "fecha o CASE (dê um nome com AS)",
                  "IF(cond, a, b)", "atalho do MySQL para um CASE de duas saídas"),
                A("SELECT titulo, CASE WHEN paginas > 500 THEN 'longo' WHEN paginas > 200 THEN 'médio' ELSE 'curto' END AS tamanho FROM livros;", "classifica cada livro",
                  "SELECT nome, IF(faltas > 10, 'reprovado', 'ok') AS situacao FROM alunos;", "duas saídas com IF"),
                "Coloque as condições mais específicas primeiro: no exemplo, se 'paginas > 200' viesse antes, nenhum livro seria 'longo'."));
            all.Add(X(Q, "Agregações", "Consulta completa (ordem das cláusulas)", "Uma consulta pode combinar tudo, mas sempre nesta ordem: SELECT … FROM … JOIN … WHERE … GROUP BY … HAVING … ORDER BY … LIMIT.",
                "SELECT colunas, AGREGAÇÃO(...) AS apelido\nFROM tabela_a a JOIN tabela_b b ON ...\nWHERE filtro_de_linhas\nGROUP BY colunas\nHAVING filtro_de_grupos\nORDER BY ... DESC\nLIMIT n;",
                A("WHERE", "filtra LINHAS antes de agrupar (não enxerga SUM, COUNT...)", "HAVING", "filtra GRUPOS depois de agrupar (enxerga as agregações)",
                  "ORDER BY SUM(col) DESC", "ordena pelo valor agregado (ou pelo apelido dele)"),
                A("SELECT a.turma, COUNT(*) AS emprestimos FROM emprestimos e JOIN alunos a ON e.aluno_id = a.id WHERE e.devolvido = 1 GROUP BY a.turma HAVING COUNT(*) >= 5 ORDER BY emprestimos DESC;",
                  "turmas com 5 ou mais empréstimos devolvidos, da que mais leu para a que menos leu",
                  "SELECT a.nome, SUM(m.valor) FROM alunos a JOIN multas m ON m.aluno_id = a.id GROUP BY a.id, a.nome ORDER BY SUM(m.valor) DESC LIMIT 3;", "os 3 alunos com mais multas"),
                "Ordem em que o banco EXECUTA: FROM/JOIN → WHERE → GROUP BY → HAVING → SELECT → ORDER BY → LIMIT."));
            all.Add(X(Q, "Juntar tabelas", "EXISTS / NOT EXISTS  (subconsulta correlacionada)", "Uma subconsulta correlacionada usa colunas da consulta de fora e roda para cada linha. EXISTS testa se ela devolve alguma linha.",
                "... WHERE EXISTS (SELECT 1 FROM outra o WHERE o.ref_id = t.id)\n... WHERE NOT EXISTS (SELECT 1 FROM outra o WHERE o.ref_id = t.id AND ...)\n... WHERE col = (SELECT MAX(col) FROM tabela t2 WHERE t2.grupo = t.grupo)", null,
                A("SELECT a.nome FROM alunos a WHERE NOT EXISTS (SELECT 1 FROM emprestimos e WHERE e.aluno_id = a.id AND e.devolvido = 0);", "alunos sem nenhum livro pendente (inclui quem nunca pegou)",
                  "SELECT l.titulo, l.autor, l.ano FROM livros l WHERE l.ano = (SELECT MAX(l2.ano) FROM livros l2 WHERE l2.autor = l.autor);", "o livro mais recente de CADA autor (mesma tabela com dois apelidos)"),
                "Para usar a mesma tabela dentro e fora, dê apelidos diferentes (l e l2). NOT EXISTS também funciona bem quando a subconsulta poderia trazer NULL (onde NOT IN falha)."));
            all.Add(X(Q, "Alterar dados", "UPDATE com cálculo", "O novo valor pode ser uma conta usando o valor atual da própria coluna.",
                "UPDATE tabela SET coluna = coluna * fator WHERE condição;\nUPDATE tabela SET coluna = coluna + n WHERE condição;", null,
                A("UPDATE livros SET preco = preco * 0.90 WHERE genero = 'infantil';", "10% de desconto nos infantis", "UPDATE alunos SET faltas = faltas + 1 WHERE id = 12;", "soma uma falta",
                  "UPDATE livros SET preco = preco * 1.05 WHERE editora IN ('Alfa', 'Beta');", "5% de aumento em duas editoras"),
                "Aumentar 10% = multiplicar por 1.10; desconto de 10% = multiplicar por 0.90. Rode antes um SELECT com o mesmo WHERE."));
            all.Add(X(Q, "Estrutura", "ALTER TABLE", "Muda a estrutura de uma tabela que já existe, sem perder os dados.",
                "ALTER TABLE tabela ADD COLUMN coluna TIPO [NOT NULL] [DEFAULT valor];\nALTER TABLE tabela DROP COLUMN coluna;\nALTER TABLE tabela RENAME TO novo_nome;",
                A("ADD COLUMN", "acrescenta uma coluna no fim", "DEFAULT valor", "valor usado quando ninguém informa (e o valor das linhas que já existiam)",
                  "BOOLEAN", "verdadeiro/falso; no MySQL vira TINYINT(1): TRUE = 1, FALSE = 0", "DROP COLUMN", "apaga a coluna e os dados dela"),
                A("ALTER TABLE livros ADD COLUMN disponivel BOOLEAN NOT NULL DEFAULT FALSE;", "todos os livros atuais ficam com disponivel = 0",
                  "ALTER TABLE alunos ADD COLUMN apelido VARCHAR(30);", "coluna opcional: as linhas antigas ficam NULL"),
                "Em tabelas grandes, ALTER TABLE pode demorar e travar a tabela: em produção, faça fora do horário de pico."));
            all.Add(X(Q, "Estrutura", "Restrições: FOREIGN KEY, UNIQUE, DEFAULT", "Regras que o próprio banco garante, não importa qual sistema grave os dados.",
                "CREATE TABLE t (\n  ...,\n  coluna TIPO NOT NULL DEFAULT valor,\n  FOREIGN KEY (coluna) REFERENCES outra(id),\n  UNIQUE (col1, col2)\n);",
                A("FOREIGN KEY (col) REFERENCES t(id)", "só aceita valores que existem em t.id (erro 1452) e impede apagar o 'pai' (erro 1451)",
                  "UNIQUE (col)", "a coluna não repete", "UNIQUE (col1, col2)", "o PAR não repete (cada coluna sozinha pode repetir)",
                  "NOT NULL DEFAULT valor", "obrigatória, com valor padrão", "CHECK (condição)", "valida o valor (ex.: CHECK (nota >= 0))"),
                A("CREATE TABLE emprestimos (id INT PRIMARY KEY AUTO_INCREMENT, aluno_id INT NOT NULL, devolvido BOOLEAN NOT NULL DEFAULT FALSE, FOREIGN KEY (aluno_id) REFERENCES alunos(id));",
                  "empréstimo sempre de um aluno que existe",
                  "CREATE TABLE notas (aluno_id INT NOT NULL, prova_id INT NOT NULL, valor DECIMAL(4,2), UNIQUE (aluno_id, prova_id));", "um aluno não pode ter duas notas na MESMA prova"),
                "As restrições de tabela (FOREIGN KEY, UNIQUE, PRIMARY KEY) vêm depois das colunas, separadas por vírgula, ainda dentro dos parênteses."));
        }
    }
}
