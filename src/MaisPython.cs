using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerQuest
{
    static partial class Challenges
    {
        // Gera variações aceitas a partir de um modelo com marcadores §0, §1...
        // A primeira combinação (primeira opção de cada marcador) é a resposta de referência.
        static string[] PyVar(string tpl, params string[][] slots)
        {
            var res = new List<string> { tpl };
            for (int i = 0; i < slots.Length; i++)
            {
                var next = new List<string>();
                foreach (var r in res)
                    foreach (var v in slots[i]) next.Add(r.Replace("§" + i, v));
                res = next;
            }
            return res.Distinct().ToArray();
        }

        static partial void MaisPython(List<Level> list)
        {
            const string P = "python";

            // 1) Rebalanceamento dos desafios existentes (localizados pelo título)
            list.First(l => l.Cat == P && l.Title == "Produto em dicionário").Tier = 0;   // um conceito, uma linha
            list.First(l => l.Cat == P && l.Title == "Ambiente virtual").Tier = 0;        // comando único, como o pip install
            list.First(l => l.Cat == P && l.Title == "Tratando erro").Tier = 1;           // um except de uma linha
            list.First(l => l.Cat == P && l.Title == "Vendas por categoria").Tier = 1;    // groupby simples
            list.First(l => l.Cat == P && l.Title == "Rota no Flask").Tier = 1;           // um decorator
            list.First(l => l.Cat == P && l.Title == "Construtor").Tier = 1;              // só a linha do __init__

            // ───── 2) Novos — Iniciante
            list.Add(C(P, 0, "code", "Fatiando a lista",
                "No Laboratório de Dados, a Yara abriu o projeto dados-vendas. A lista vendas guarda os pedidos do dia em ordem de chegada e o painel só mostra os primeiros.",
                "Guarde na variável primeiras os 3 primeiros itens da lista vendas, usando fatiamento (slicing).",
                A("primeiras = vendas[:3]", "primeiras = vendas[0:3]"),
                A("Fatiamento usa colchetes com início:fim — o fim NÃO entra.", "primeiras = vendas[:_]"),
                A("vendas[", "Acessa a lista por posição.", ":3]", "Do início até a posição 3 (sem incluí-la): itens 0, 1 e 2."),
                "Fatias nunca dão erro de índice: vendas[:3] numa lista de 2 itens devolve os 2. E vendas[-1] é o último item."));

            list.Add(C(P, 0, "code", "Mensagem com f-string",
                "O Otávio quer que o script mostre o total do pedido de um jeito legível, sem ficar somando textos com +.",
                "Use print com uma f-string para mostrar Total do pedido: seguido do valor da variável total (ex.: Total do pedido: 25.5).",
                A("print(f\"Total do pedido: {total}\")"),
                A("Uma f-string começa com f antes das aspas, e as variáveis vão entre chaves.", "print(f\"Total do pedido: {___}\")"),
                A("f\"...\"", "f-string: texto com valores embutidos.", "{total}", "Troca pelo valor da variável total."),
                "Dá para formatar dentro das chaves: {total:.2f} mostra sempre 2 casas decimais (25.50)."));

            list.Add(C(P, 0, "code", "Desconto com get",
                "Nem todo produto tem a chave \"desconto\" no dicionário. Com produto[\"desconto\"] o script quebra com KeyError quando ela não existe.",
                "Guarde em desconto o valor da chave \"desconto\" do dicionário produto, usando o método get com valor padrão 0 quando a chave não existir.",
                A("desconto = produto.get(\"desconto\", 0)"),
                A("Dicionários têm um método que não quebra quando a chave falta.", "desconto = produto.get(\"...\", 0)"),
                A("produto.get(", "Lê uma chave sem gerar KeyError.", "\"desconto\", 0", "Chave procurada e valor devolvido se ela não existir."),
                "Sem o segundo argumento, get devolve None quando a chave não existe."));

            // ───── Novos — Intermediário
            list.Add(C(P, 1, "code", "Ranking numerado",
                "A Dani vai imprimir o ranking dos produtos mais vendidos (a lista produtos já está em ordem) e quer cada linha numerada, começando em 1.",
                "Percorra produtos com for e enumerate começando em 1 (variáveis do laço: i e nome) e, dentro do laço, mostre cada item com print e f-string no formato 1. Café",
                A("for i, nome in enumerate(produtos, start=1):\n    print(f\"{i}. {nome}\")",
                  "for i, nome in enumerate(produtos, 1):\n    print(f\"{i}. {nome}\")"),
                A("enumerate devolve pares (posição, item) e aceita o número inicial.", "for i, nome in enumerate(produtos, start=_): print(f\"...\")"),
                A("enumerate(produtos, start=1)", "Gera (1, item1), (2, item2)...", "for i, nome", "Desempacota cada par em duas variáveis.", "print(f\"{i}. {nome}\")", "Mostra no formato pedido."),
                "Evite for i in range(len(lista)): enumerate é mais legível. Para percorrer duas listas juntas, use zip(a, b)."));

            list.Add(C(P, 1, "code", "Só os caros",
                "O dicionário precos tem nome do produto → preço. A Yara quer só os itens premium para uma campanha.",
                "Crie em uma linha o dicionário caros só com os itens de precos cujo preço é maior que 10, usando dict comprehension com .items() (variáveis: nome e preco).",
                A("caros = {nome: preco for nome, preco in precos.items() if preco > 10}",
                  "caros = {nome: preco for nome, preco in precos.items() if 10 < preco}",
                  "caros = {nome: preco for (nome, preco) in precos.items() if preco > 10}"),
                A("É como a list comprehension, mas com chaves e chave: valor; o filtro if vai no fim.", "{nome: preco for nome, preco in precos.items() if ...}"),
                A("{nome: preco ...}", "Monta um novo dicionário.", "for nome, preco in precos.items()", "Percorre os pares chave/valor.", "if preco > 10", "Mantém só os caros."),
                "items() devolve pares (chave, valor); keys() só chaves; values() só valores."));

            // ───── Novos — Profissional
            list.Add(C(P, 2, "code", "Top 3 por preço",
                "A home do Café 365 vai mostrar os 3 produtos mais caros. A lista produtos tem dicionários como {\"nome\": \"Café\", \"preco\": 9.9} e NÃO pode ser alterada (outras partes do sistema dependem da ordem dela).",
                "Em uma única linha, guarde em top3 os 3 produtos de maior preço, do mais caro para o mais barato. Use a função que devolve uma NOVA lista ordenada, com uma lambda de parâmetro p como critério, e fatiamento.",
                PyVar("top3 = sorted(produtos, §0)[§1]",
                    A("key=lambda p: p[\"preco\"], reverse=True", "reverse=True, key=lambda p: p[\"preco\"]",
                      "key=lambda p: p.get(\"preco\"), reverse=True", "reverse=True, key=lambda p: p.get(\"preco\")",
                      "key=lambda p: -p[\"preco\"]"),
                    A(":3", "0:3")),
                A("list.sort() altera a lista original; sorted() devolve uma nova. Para o maior primeiro existe um parâmetro de inversão.", "sorted(produtos, key=lambda p: p[\"...\"], reverse=____)[:_]"),
                A("sorted(produtos, ...)", "Nova lista ordenada; a original fica intacta.", "key=lambda p: p[\"preco\"]", "Critério: o preço de cada dicionário.", "reverse=True", "Ordem decrescente (mais caro primeiro).", "[:3]", "Pega só os 3 primeiros."),
                "Para muitos dados, heapq.nlargest(3, produtos, key=...) evita ordenar a lista inteira."));

            var filtros = new List<string>();
            foreach (var pre in A("df[", "df.loc["))
                foreach (var v in A("(df[\"valor\"] > df[\"valor\"].mean())", "(df.valor > df.valor.mean())", "(df[\"valor\"] > df.valor.mean())", "(df.valor > df[\"valor\"].mean())"))
                    foreach (var l in A("(df[\"loja\"] == \"Centro\")", "(df.loja == \"Centro\")"))
                    {
                        filtros.Add("destaque = " + pre + v + " & " + l + "]");
                        filtros.Add("destaque = " + pre + l + " & " + v + "]");
                    }
            list.Add(C(P, 2, "code", "Destaques do Centro",
                "O DataFrame df tem as colunas loja e valor. A Dani tentou df[df[\"valor\"] > media and df[\"loja\"] == \"Centro\"] e recebeu ValueError: The truth value of a Series is ambiguous.",
                "Em uma linha, guarde em destaque as vendas de df da loja \"Centro\" com valor acima da média da coluna valor (calcule a média no próprio filtro com mean()). Combine as duas condições num único filtro booleano, do jeito certo para Series.",
                filtros.ToArray(),
                A("Em pandas, and/or não funcionam com colunas: use os operadores & (e) e | (ou).", "df[(df[\"valor\"] > df[\"valor\"].mean()) & (df[\"loja\"] == \"______\")]"),
                A("df[\"valor\"].mean()", "Média da coluna valor.", "( ... ) & ( ... )", "& combina as condições linha a linha; os parênteses são obrigatórios porque & tem precedência maior que > e ==.", "df[ ... ]", "Mantém só as linhas em que a máscara é True."),
                "Para negar uma condição use ~ (ex.: df[~(df[\"loja\"] == \"Centro\")]). Para várias lojas: df[\"loja\"].isin([...])."));

            list.Add(C(P, 2, "code", "Média sem quebrar",
                "O relatório noturno caiu com ZeroDivisionError: uma loja nova ainda não tinha vendas e a lista veio vazia.",
                "Escreva a função media(valores, padrao=0) que devolve a soma de valores dividida pela quantidade de itens. Se a lista estiver vazia, capture o ZeroDivisionError com try/except (sem usar if) e devolva padrao.",
                PyVar("def media(valores, padrao=0):\n    try:\n        return sum(valores) / len(valores)\n    except ZeroDivisionError§0:\n        return padrao",
                    A("", " as e", " as erro")),
                A("Parâmetro com valor padrão: padrao=0 na assinatura. O return pode ficar dentro do try e dentro do except.", "def media(valores, padrao=0):\n    try:\n        return sum(...) / len(...)\n    except ZeroDivisionError:\n        return ______"),
                A("padrao=0", "Valor padrão: quem chama pode omitir o argumento.", "try: return sum(valores) / len(valores)", "Tenta calcular a média.", "except ZeroDivisionError: return padrao", "Lista vazia → len é 0 → devolve o padrão em vez de quebrar."),
                "Capture só o erro que você espera: um except genérico esconderia, por exemplo, um TypeError de dados inválidos."));

            list.Add(C(P, 2, "code", "Carrinho fantasma",
                "Bug em produção: clientes diferentes estão vendo itens uns dos outros no carrinho. O culpado é def adicionar(item, carrinho=[]): o valor padrão é criado UMA vez e reaproveitado em todas as chamadas.",
                "Reescreva a função adicionar(item, carrinho=None): se carrinho for None (teste com is), crie uma lista vazia nova; depois acrescente item com append e devolva carrinho.",
                PyVar("def adicionar(item, carrinho=None):\n    if carrinho is None:\n        carrinho = §0\n    carrinho.append(item)\n    return carrinho",
                    A("[]", "list()")),
                A("Use None como padrão e crie a lista dentro da função, a cada chamada.", "def adicionar(item, carrinho=None):\n    if carrinho is None:\n        carrinho = []\n    ..."),
                A("carrinho=None", "Padrão imutável e seguro.", "if carrinho is None: carrinho = []", "Lista nova a cada chamada sem carrinho.", "carrinho.append(item)", "Acrescenta o item.", "return carrinho", "Devolve a lista."),
                "Vale para qualquer padrão mutável (lista, dicionário, set). Compare com None usando is, não ==."));

            list.Add(C(P, 2, "code", "Config com fallback",
                "O script do dados-vendas lê as configurações de config.json. Na máquina nova do Otávio o arquivo ainda não existe e o script quebra.",
                "O módulo json já está importado. Carregue config.json na variável config usando with open(...) as f e a função do json que lê de um arquivo aberto. Se o arquivo não existir, capture o erro específico e use um dicionário vazio.",
                PyVar("try:\n    with open(\"config.json\"§0) as f:\n        config = json.load(f)\nexcept FileNotFoundError:\n    config = §1",
                    A("", ", \"r\"", ", encoding=\"utf-8\"", ", \"r\", encoding=\"utf-8\""),
                    A("{}", "dict()")),
                A("O try envolve o with inteiro; json.load lê do arquivo (json.loads lê de uma string).", "try:\n    with open(\"config.json\") as f:\n        config = json.____(f)\nexcept __________:\n    config = {}"),
                A("try:", "Protege a abertura do arquivo.", "with open(\"config.json\") as f:", "Abre e garante o fechamento.", "config = json.load(f)", "Converte o JSON do arquivo em dicionário.", "except FileNotFoundError: config = {}", "Arquivo ausente → configuração vazia."),
                "Um JSON malformado gera json.JSONDecodeError: trate à parte se quiser avisar o usuário em vez de seguir com padrão."));

            list.Add(C(P, 2, "code", "Resumo por loja",
                "A diretoria quer uma tabela por loja com o total vendido e o ticket médio, com nomes de coluna claros (nada de \"sum\" e \"mean\" no cabeçalho).",
                "Em uma linha, crie resumo agrupando df por loja e usando agregação nomeada no agg: coluna total = soma de valor e coluna media = média de valor, nessa ordem.",
                A("resumo = df.groupby(\"loja\").agg(total=(\"valor\", \"sum\"), media=(\"valor\", \"mean\"))",
                  "resumo = df.groupby(\"loja\").agg(total=pd.NamedAgg(column=\"valor\", aggfunc=\"sum\"), media=pd.NamedAgg(column=\"valor\", aggfunc=\"mean\"))",
                  "resumo = df.groupby(\"loja\").agg(total=pd.NamedAgg(\"valor\", \"sum\"), media=pd.NamedAgg(\"valor\", \"mean\"))",
                  "resumo = df.groupby(by=\"loja\").agg(total=(\"valor\", \"sum\"), media=(\"valor\", \"mean\"))"),
                A("Agregação nomeada: nova_coluna=(\"coluna_origem\", \"funcao\") dentro de agg.", "df.groupby(\"loja\").agg(total=(\"valor\", \"___\"), media=(\"valor\", \"____\"))"),
                A("groupby(\"loja\")", "Um grupo por loja.", "total=(\"valor\", \"sum\")", "Coluna total: soma de valor.", "media=(\"valor\", \"mean\")", "Coluna media: média de valor."),
                "Acrescente .reset_index() para loja virar coluna normal (útil para exportar ou fazer merge)."));

            list.Add(C(P, 2, "code", "Vendas com produtos",
                "O DataFrame vendas tem produto_id e valor; o DataFrame produtos tem produto_id e nome. Algumas vendas são de produtos ainda não cadastrados — e elas não podem sumir do relatório.",
                "Em uma linha, junte vendas com produtos (vendas à esquerda) pela coluna produto_id, guardando em completo e mantendo TODAS as vendas, mesmo sem produto correspondente.",
                PyVar("completo = §0",
                    A("vendas.merge(produtos, on=\"produto_id\", how=\"left\")", "vendas.merge(produtos, how=\"left\", on=\"produto_id\")",
                      "pd.merge(vendas, produtos, on=\"produto_id\", how=\"left\")", "pd.merge(vendas, produtos, how=\"left\", on=\"produto_id\")")),
                A("merge junta tabelas como um JOIN de SQL; o padrão (inner) descarta linhas sem par.", "vendas.merge(produtos, on=\"produto_id\", how=\"____\")"),
                A("vendas.merge(produtos", "Junta as duas tabelas.", "on=\"produto_id\"", "Coluna em comum usada na junção.", "how=\"left\"", "Mantém todas as linhas da esquerda (vendas); sem par, as colunas de produtos ficam NaN."),
                "Depois do merge, completo[\"nome\"].isna() revela as vendas de produtos não cadastrados. Use validate=\"many_to_one\" para detectar ids duplicados em produtos."));

            list.Add(C(P, 2, "code", "API com timeout",
                "O script consulta a API de vendas da Micorosoft. Ontem a API travou e o script ficou parado para sempre; antes disso, um erro 500 passou despercebido e gerou um relatório vazio.",
                "O requests já está importado. Em três linhas: faça um GET em https://api.micorosoft.com/vendas com o parâmetro de query loja=7 (via params, número) e timeout de 5 segundos, guardando em resp; faça a resposta lançar exceção se o status for de erro; e guarde o corpo JSON convertido em dados.",
                PyVar("resp = requests.get(\"https://api.micorosoft.com/vendas\", §0)\nresp.raise_for_status()\ndados = resp.json()",
                    A("params={\"loja\": 7}, timeout=5", "timeout=5, params={\"loja\": 7}")),
                A("requests.get aceita params= (query string) e timeout=; a resposta tem um método que lança erro para status 4xx/5xx.", "resp = requests.get(\"...\", params={\"loja\": 7}, timeout=5)\nresp.raise_for_______()\ndados = resp.____()"),
                A("params={\"loja\": 7}", "Vira ?loja=7 na URL (já codificado).", "timeout=5", "Desiste após 5 s em vez de esperar para sempre.", "resp.raise_for_status()", "Lança HTTPError para respostas 4xx/5xx.", "resp.json()", "Converte o corpo JSON em dicionário/lista."),
                "Sem timeout, requests pode esperar indefinidamente. Em produção, envolva a chamada em try/except requests.RequestException."));

            list.Add(C(P, 2, "code", "Produto digital",
                "O Café 365 vai vender vale-presente online. A classe Produto já existe com __init__(self, nome, preco) e o método frete(self). Produto digital não tem frete.",
                "Crie a classe ProdutoDigital que herda de Produto. O __init__ recebe (self, nome, preco, link), chama o construtor da classe-mãe com super() passando nome e preco, e guarda link em self.link. Depois sobrescreva frete(self) para devolver 0.",
                PyVar("class ProdutoDigital(Produto):\n    def __init__(self, nome, preco, link):\n        §0.__init__(nome, preco)\n        self.link = link\n\n    def frete(self):\n        return 0",
                    A("super()", "super(ProdutoDigital, self)")),
                A("Herança: class Filha(Mae):. O super() chama o método da classe-mãe.", "class ProdutoDigital(Produto):\n    def __init__(self, nome, preco, link):\n        super().__init__(____, _____)\n        self.link = link\n    def frete(self): ..."),
                A("class ProdutoDigital(Produto):", "Herda atributos e métodos de Produto.", "super().__init__(nome, preco)", "Reaproveita o construtor da mãe (sem duplicar código).", "self.link = link", "Atributo novo, só da filha.", "def frete(self): return 0", "Sobrescreve o método da mãe."),
                "Esquecer o super().__init__ é um bug clássico: o objeto fica sem self.nome e self.preco."));

            list.Add(C(P, 2, "code", "Decorator de log",
                "O Otávio quer saber quais funções do dados-vendas estão sendo chamadas, sem editar uma por uma. A ideia: um decorator @log.",
                "Escreva o decorator log(func). Dentro dele, defina wrapper(*args, **kwargs) que mostra com print e f-string Chamando seguido do nome da função (atributo __name__) e depois devolve o resultado de func chamada com os mesmos argumentos. No fim, log devolve wrapper.",
                PyVar("def log(func):\n§0    def wrapper(*args, **kwargs):\n        print(f\"Chamando {func.__name__}\")\n        return func(*args, **kwargs)\n    return wrapper",
                    A("", "    @functools.wraps(func)\n", "    @wraps(func)\n")),
                A("Decorator é uma função que recebe uma função e devolve outra (a wrapper) que a envolve.", "def log(func):\n    def wrapper(*args, **kwargs):\n        print(f\"Chamando {func.________}\")\n        return func(*____, **______)\n    return _______"),
                A("def log(func):", "Recebe a função decorada.", "def wrapper(*args, **kwargs):", "Aceita quaisquer argumentos.", "print(f\"Chamando {func.__name__}\")", "Registra a chamada.", "return func(*args, **kwargs)", "Chama a original e repassa o resultado.", "return wrapper", "A função decorada passa a ser a wrapper."),
                "Use @functools.wraps(func) na wrapper para preservar o nome e a docstring da função original."));
        }
    }

    static partial class Docs
    {
        static partial void DocsMaisPython(List<DocEntry> all)
        {
            const string P = "Python";
            all.Add(X(P, "Básico", "Fatiamento (slicing)", "Pega um pedaço de uma lista ou texto: lista[inicio:fim]. O índice fim não entra.",
                "lista[inicio:fim]\nlista[:fim]   lista[inicio:]   lista[-1]   lista[::-1]", A("[:n]", "os n primeiros", "[-n:]", "os n últimos", "[::-1]", "invertida"),
                A("notas[:2]", "as 2 primeiras notas", "nome[0:4]", "os 4 primeiros caracteres do texto", "notas[-1]", "a última nota"),
                "Fatiar cria uma lista nova e nunca dá IndexError."));
            all.Add(X(P, "Básico", "f-strings", "Texto com valores embutidos: f antes das aspas e as expressões entre { }.",
                "f\"texto {variavel} texto\"", A("{x:.2f}", "2 casas decimais", "{x:>5}", "alinha à direita em 5 espaços"),
                A("print(f\"Aluno: {nome}\")", "mostra Aluno: Rui", "print(f\"Média {media:.1f}\")", "mostra Média 7.5"), null));
            all.Add(X(P, "Básico", "dict: get, items, keys, values", "Métodos de dicionário. get lê uma chave sem quebrar se ela não existir.",
                "d.get(chave)\nd.get(chave, padrao)\nfor chave, valor in d.items():", A("get(c, p)", "valor de c, ou p se não existir", "items()", "pares (chave, valor)", "keys() / values()", "só chaves / só valores"),
                A("email = aluno.get(\"email\", \"não informado\")", "padrão se faltar a chave", "for materia, nota in boletim.items(): print(materia, nota)", "percorre os pares"),
                "d[\"chave\"] gera KeyError se a chave não existir; get devolve None (ou o padrão)."));
            all.Add(X(P, "Básico", "for, enumerate e zip", "enumerate numera os itens durante o laço; zip percorre duas listas lado a lado.",
                "for i, item in enumerate(lista, start=N):\nfor a, b in zip(lista1, lista2):", A("start=", "número inicial do enumerate (padrão 0)"),
                A("for pos, livro in enumerate(livros, start=1):\n    print(f\"{pos}) {livro}\")", "1) Dom Casmurro, 2) ...", "for nome, nota in zip(nomes, notas): print(nome, nota)", "duas listas juntas"), null));
            all.Add(X(P, "Básico", "Dict comprehension", "Cria um dicionário a partir de outra sequência numa única expressão, com filtro opcional.",
                "{chave: valor for item in sequencia if condicao}", null,
                A("aprovados = {aluno: nota for aluno, nota in notas.items() if nota >= 7}", "só quem tirou 7 ou mais", "tamanhos = {p: len(p) for p in palavras}", "palavra → tamanho"), null));
            all.Add(X(P, "Funções e classes", "sorted, key= e lambda", "sorted devolve uma NOVA lista ordenada; key= diz por qual valor ordenar; reverse=True inverte.",
                "sorted(lista, key=lambda x: criterio, reverse=True)", A("key=", "função aplicada a cada item para comparar", "reverse=True", "ordem decrescente", "lista.sort()", "ordena a própria lista (altera a original)"),
                A("sorted(alunos, key=lambda a: a[\"idade\"])", "do mais novo ao mais velho", "sorted(livros, key=lambda l: l[\"paginas\"], reverse=True)[:5]", "os 5 livros mais longos"),
                "lambda x: expressao é uma função anônima de uma linha."));
            all.Add(X(P, "Funções e classes", "Parâmetros com valor padrão", "def f(a, b=valor): quem chama pode omitir b. CUIDADO: o padrão é criado uma única vez.",
                "def nome(obrigatorio, opcional=padrao):", A("x=None", "padrão seguro para listas/dicionários"),
                A("def saudar(nome, saudacao=\"Olá\"): return f\"{saudacao}, {nome}\"", "saudar(\"Ana\") → Olá, Ana",
                  "def matricular(aluno, turmas=None):\n    if turmas is None:\n        turmas = []\n    turmas.append(aluno)\n    return turmas", "jeito certo com lista"),
                "Nunca use lista ou dicionário como padrão (turmas=[]): todas as chamadas compartilhariam o mesmo objeto."));
            all.Add(X(P, "Arquivos e erros", "try / except / else / finally", "Estrutura completa de tratamento de erros. Pode ficar dentro de funções, com return em cada ramo.",
                "try:\n    ...\nexcept TipoDoErro as e:\n    ...\nelse:\n    ...   (sem erro)\nfinally:\n    ...   (sempre roda)",
                A("as e", "guarda o objeto do erro", "else", "só se não houve erro", "finally", "sempre executa (fechar recursos)"),
                A("def para_inteiro(texto, padrao=None):\n    try:\n        return int(texto)\n    except ValueError:\n        return padrao", "converte ou devolve o padrão",
                  "except (ValueError, TypeError):", "dois tipos de erro no mesmo except"),
                "Capture o erro mais específico possível."));
            all.Add(X(P, "Arquivos e erros", "json: load, loads, dump", "Lê e grava JSON (import json). load/dump trabalham com arquivo aberto; loads/dumps com texto.",
                "dados = json.load(arquivo)\ndados = json.loads(texto)\njson.dump(dados, arquivo)",
                A("json.load(f)", "arquivo → dicionário/lista", "json.loads(s)", "string → dicionário/lista", "json.dump(d, f)", "dicionário → arquivo"),
                A("with open(\"alunos.json\", encoding=\"utf-8\") as arq:\n    alunos = json.load(arq)", "lê o arquivo",
                  "try:\n    ...\nexcept FileNotFoundError:\n    alunos = []", "segue em frente se o arquivo não existir"),
                "JSON inválido gera json.JSONDecodeError."));
            all.Add(X(P, "Funções e classes", "Herança e super()", "Uma classe filha herda atributos e métodos da mãe; super() chama a versão da mãe.",
                "class Filha(Mae):\n    def __init__(self, a, b, c):\n        super().__init__(a, b)\n        self.c = c", null,
                A("class Ebook(Livro):\n    def __init__(self, titulo, autor, tamanho_mb):\n        super().__init__(titulo, autor)\n        self.tamanho_mb = tamanho_mb", "filha com atributo novo",
                  "    def emprestimo_dias(self):\n        return 30", "sobrescreve um método da mãe"),
                "Sobrescrever = definir na filha um método com o mesmo nome do da mãe."));
            all.Add(X(P, "Funções e classes", "Decorators", "Função que recebe outra função e devolve uma nova, que a envolve. Aplicado com @nome acima do def.",
                "def deco(funcao):\n    def interna(*args, **kwargs):\n        ...   (antes)\n        return funcao(*args, **kwargs)\n    return interna",
                A("*args, **kwargs", "repassa quaisquer argumentos", "funcao.__name__", "nome da função", "@functools.wraps(funcao)", "preserva nome e docstring da original"),
                A("def cronometro(funcao):\n    def interna(*args, **kwargs):\n        inicio = time.time()\n        r = funcao(*args, **kwargs)\n        print(f\"{funcao.__name__}: {time.time() - inicio:.2f}s\")\n        return r\n    return interna", "mede o tempo de qualquer função",
                  "@cronometro\ndef gerar_boletim(): ...", "aplica o decorator"), null));
            all.Add(X(P, "Dados e web", "pandas: filtro booleano", "Seleciona linhas com uma condição entre colchetes. Para combinar condições use & (e), | (ou), ~ (não) com parênteses.",
                "df[df[\"coluna\"] > valor]\ndf[(cond1) & (cond2)]", A("&  |  ~", "e / ou / não (nunca and/or/not)", "mean()", "média de uma coluna", "isin([...])", "valor dentro de uma lista"),
                A("tabela[tabela[\"nota\"] >= 7]", "só os aprovados", "tabela[(tabela[\"nota\"] < tabela[\"nota\"].mean()) & (tabela[\"turma\"] == \"2B\")]", "abaixo da média E da turma 2B"),
                "Sem os parênteses, & é calculado antes de > e == e o filtro dá erro."));
            all.Add(X(P, "Dados e web", "pandas: agg com nomes", "Agregação nomeada: cria colunas com nomes escolhidos por você.",
                "df.groupby(\"grupo\").agg(nova=(\"coluna\", \"funcao\"), outra=(\"coluna\", \"funcao\"))",
                A("\"sum\" \"mean\" \"count\" \"max\" \"min\"", "funções de agregação"),
                A("tabela.groupby(\"turma\").agg(maior=(\"nota\", \"max\"), alunos=(\"nome\", \"count\"))", "maior nota e quantidade por turma"),
                "pd.NamedAgg(column=..., aggfunc=...) é a forma longa equivalente."));
            all.Add(X(P, "Dados e web", "pandas: merge", "Junta dois DataFrames por uma coluna em comum, como um JOIN de SQL.",
                "esq.merge(dir, on=\"coluna\", how=\"tipo\")\npd.merge(esq, dir, on=\"coluna\", how=\"tipo\")",
                A("how=\"inner\"", "só linhas com par nos dois (padrão)", "how=\"left\"", "todas da esquerda", "how=\"right\"", "todas da direita", "how=\"outer\"", "todas de ambos"),
                A("alunos.merge(turmas, on=\"turma_id\", how=\"inner\")", "só alunos com turma cadastrada"),
                "Linhas sem par ficam com NaN nas colunas da outra tabela."));
            all.Add(X(P, "Dados e web", "requests: GET em APIs", "Faz requisições HTTP (pip install requests).",
                "r = requests.get(url, params={...}, timeout=segundos)\nr.raise_for_status()\nr.json()",
                A("params=", "parâmetros da query string (?a=1)", "timeout=", "tempo máximo de espera", "raise_for_status()", "lança erro se o status for 4xx/5xx", "json()", "corpo da resposta como dicionário/lista"),
                A("r = requests.get(\"https://api.escola.com/alunos\", params={\"turma\": \"2B\"}, timeout=10)", "GET com query e timeout", "r.status_code", "código HTTP (200, 404...)"),
                "Sem timeout a chamada pode ficar presa para sempre."));
        }
    }
}
