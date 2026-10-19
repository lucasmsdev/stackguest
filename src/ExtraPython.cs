using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerQuest
{
    static partial class Challenges
    {
        // Segunda leva de Python: 24 desafios por nível. Só ACRESCENTA (não remove nem reordena os existentes).
        // Usa o helper PyVar (src/MaisPython.cs) para gerar variações aceitas.
        static partial void ExtraPython(List<Level> list)
        {
            const string P = "python";
            // variações comuns de "chamar o pip" / "chamar o python"
            string[] pipVar = A("pip", "pip3", "python -m pip", "python3 -m pip", "py -m pip");
            string[] pyVar = A("python", "python3", "py");
            Func<string[], string, string[]> Cmds = (pre, rest) => pre.Select(x => x + " " + rest).ToArray();

            // ═════════════════════════════ INICIANTE ═════════════════════════════
            list.Add(C(P, 0, "code", "Tipo da variável",
                "Primeiro dia no Laboratório de Dados: a Yara mostra que o valor lido da planilha às vezes vem como texto e às vezes como número — e isso muda tudo.",
                "Mostre na tela, com print, o tipo da variável preco usando a função nativa type.",
                A("print(type(preco))"),
                A("Existe uma função nativa que devolve o tipo de qualquer valor; depois é só passar o resultado para o print.", "print(type(______))"),
                A("type(preco)", "Devolve a classe do valor: <class 'float'>, <class 'str'>...", "print(...)", "Mostra o resultado na tela."),
                "Para testar o tipo dentro de um if, prefira isinstance(preco, float) — ele também aceita subclasses."));

            list.Add(C(P, 0, "code", "Texto para número",
                "O formulário do Café 365 entrega tudo como texto: a variável qtd_texto vale \"3\", e fazer contas com ela dá TypeError.",
                "Converta qtd_texto para número inteiro e guarde o resultado na variável quantidade.",
                A("quantidade = int(qtd_texto)"),
                A("Cada tipo tem uma função de conversão com o mesmo nome do tipo.", "quantidade = ___(qtd_texto)"),
                A("int(qtd_texto)", "Converte o texto \"3\" no número inteiro 3.", "quantidade =", "Guarda o número convertido."),
                "int(\"abc\") gera ValueError. Para casas decimais use float(\"9.90\"); para voltar a texto, str(3)."));

            list.Add(C(P, 0, "code", "Novo item no pedido",
                "O Otávio está montando o pedido da mesa 4 no script. A lista pedido já tem dois itens e o cliente pediu mais um Croissant.",
                "Acrescente o texto \"Croissant\" no fim da lista pedido, usando o método de lista que adiciona um item no final.",
                A("pedido.append(\"Croissant\")"),
                A("Listas têm um método que adiciona UM item no final e altera a própria lista.", "pedido.______(\"Croissant\")"),
                A("pedido.append(", "Adiciona um item ao fim da lista (altera a própria lista).", "\"Croissant\")", "O item acrescentado."),
                "Para vários itens de uma vez use extend([...]); insert(0, item) coloca no começo."));

            list.Add(C(P, 0, "cmd", "Rodando o script",
                "A Yara deixou pronto o arquivo relatorio.py na pasta do projeto dados-vendas. Agora é com você.",
                "No terminal integrado, execute o script relatorio.py com o interpretador Python.",
                Cmds(pyVar, "relatorio.py"),
                A("O comando é o nome do interpretador; o arquivo vai logo depois.", "python <arquivo>.py"),
                A("python", "Chama o interpretador.", "relatorio.py", "Arquivo que será executado, de cima para baixo."),
                "Se o terminal não encontrar o python, confira se ele está no PATH (no Windows, o lançador py também funciona)."));

            list.Add(C(P, 0, "code", "Acumulando o caixa",
                "No laço que percorre as vendas do dia, a variável total precisa ir crescendo a cada venda.",
                "Some o valor da variável preco ao que já está em total, usando o operador de atribuição composta (sem repetir o nome total do lado direito).",
                A("total += preco"),
                A("Em vez de escrever total = total + preco, existe um atalho que soma e guarda de uma vez.", "total __ preco"),
                A("+=", "Soma e guarda na mesma variável: equivale a total = total + preco.", "preco", "Valor acrescentado ao total."),
                "Também existem -=, *= e /=. Com textos, += concatena."));

            list.Add(C(P, 0, "code", "Tamanhos do copo",
                "Os tamanhos de copo do Café 365 nunca mudam: P, M e G. A Dani quer garantir que ninguém altere essa sequência por engano.",
                "Crie a tupla tamanhos com os textos \"P\", \"M\" e \"G\", nessa ordem.",
                A("tamanhos = (\"P\", \"M\", \"G\")", "tamanhos = \"P\", \"M\", \"G\""),
                A("Tuplas são como listas que não podem ser alteradas — mas usam outro símbolo em volta dos itens.", "tamanhos = (\"P\", ___, ___)"),
                A("( ... )", "Parênteses criam uma tupla (imutável).", "\"P\", \"M\", \"G\"", "Itens em ordem fixa: tamanhos[0] é \"P\"."),
                "Tupla de um item só precisa da vírgula: (\"P\",). Tuplas podem ser chaves de dicionário; listas não."));

            list.Add(C(P, 0, "code", "Tem estoque?",
                "Antes de aceitar um pedido, o sistema confere o estoque. A Yara quer a mensagem só quando houver unidades.",
                "Escreva um if que, quando estoque for maior que 0, mostre com print o texto Em estoque (o print fica na linha de baixo, recuado).",
                PyVar("if §0:\n    print(\"Em estoque\")", A("estoque > 0", "0 < estoque", "estoque >= 1")),
                A("A condição termina com dois-pontos e o bloco de dentro fica recuado (4 espaços).", "if estoque > _:\n    print(\"...\")"),
                A("if estoque > 0:", "Testa a condição; os dois-pontos abrem o bloco.", "print(\"Em estoque\")", "Só roda se a condição for verdadeira (linha recuada)."),
                "O recuo é a sintaxe do bloco em Python. Para o caso contrário, use else: no mesmo nível do if."));

            list.Add(C(P, 0, "code", "Cupom em maiúsculas",
                "Clientes digitam o cupom de qualquer jeito (cafe10, Cafe10...) e o sistema só reconhece CAFE10.",
                "Transforme o texto da variável cupom em maiúsculas e guarde o resultado na própria variável cupom.",
                A("cupom = cupom.upper()"),
                A("Textos têm métodos para mudar maiúsculas/minúsculas — e eles devolvem um texto NOVO, que precisa ser guardado.", "cupom = cupom._____()"),
                A("cupom.upper()", "Devolve uma cópia em maiúsculas.", "cupom = ...", "Strings são imutáveis: sem guardar o resultado, nada muda."),
                "lower() faz o contrário; casefold() é ainda melhor para comparar textos sem diferenciar maiúsculas."));

            list.Add(C(P, 0, "code", "Mesas de 1 a 5",
                "O Otávio vai gerar as comandas das mesas 1 a 5 do Café 365.",
                "Com for e range, percorra os números de 1 a 5 (variável do laço: mesa) e mostre cada um com print.",
                A("for mesa in range(1, 6):\n    print(mesa)"),
                A("range(início, fim) para ANTES do fim.", "for mesa in range(_, _):\n    print(mesa)"),
                A("range(1, 6)", "Gera 1, 2, 3, 4, 5 (o 6 não entra).", "for mesa in", "A cada volta, mesa recebe o próximo número.", "print(mesa)", "Corpo do laço, recuado."),
                "range(0, 10, 2) conta de 2 em 2; range(5, 0, -1) conta para trás."));

            list.Add(C(P, 0, "code", "Categorias únicas",
                "A lista categorias tem repetições (bebida, doce, bebida, salgado...) e o filtro do painel precisa de cada categoria uma vez só.",
                "Crie o conjunto unicas a partir da lista categorias, usando a função que converte para set.",
                A("unicas = set(categorias)"),
                A("Conjuntos (set) não guardam itens repetidos.", "unicas = ___(categorias)"),
                A("set(categorias)", "Cria um conjunto e descarta as repetições.", "unicas", "Cada categoria aparece uma única vez."),
                "Set não garante ordem. Para remover repetidos mantendo a ordem: list(dict.fromkeys(categorias))."));

            list.Add(C(P, 0, "code", "Arredondando o preço",
                "Depois de aplicar o desconto, o preço ficou 8.4150000001 e o recibo saiu feio.",
                "Guarde em preco_final o valor de preco arredondado para 2 casas decimais, com a função nativa round.",
                A("preco_final = round(preco, 2)", "preco_final = round(preco, ndigits=2)"),
                A("round recebe o número e quantas casas decimais manter.", "preco_final = round(preco, _)"),
                A("round(preco, 2)", "Arredonda para 2 casas decimais.", "preco_final =", "Guarda o valor arredondado."),
                "Para só EXIBIR com 2 casas use f\"{preco:.2f}\". Para dinheiro com precisão exata, o módulo decimal é o mais indicado."));

            list.Add(C(P, 0, "cmd", "Ativando o ambiente",
                "O venv do projeto dados-vendas já foi criado na pasta venv, mas o pip continua instalando tudo no Python global do Windows.",
                "No terminal do Windows (Prompt de Comando; no PowerShell o script é o Activate.ps1), ative o ambiente virtual que está na pasta venv.",
                A("venv\\Scripts\\activate", "venv\\Scripts\\activate.bat", ".\\venv\\Scripts\\activate", ".\\venv\\Scripts\\activate.bat",
                  "venv\\Scripts\\Activate.ps1", ".\\venv\\Scripts\\Activate.ps1"),
                A("O ambiente tem um script de ativação dentro da subpasta Scripts (no Windows).", "venv\\Scripts\\________"),
                A("venv\\Scripts\\", "Pasta de executáveis do ambiente no Windows (no Linux/macOS é venv/bin).", "activate", "Script que coloca o Python do ambiente na frente do PATH."),
                "No Linux/macOS: source venv/bin/activate. Para sair: deactivate. O prompt mostra (venv) quando o ambiente está ativo."));

            list.Add(C(P, 0, "code", "Sorteio do brinde",
                "Fim do mês! O Café 365 vai sortear um brinde entre os nomes da lista clientes. O módulo random já está importado.",
                "Guarde em ganhador um item sorteado da lista clientes, usando a função do módulo random que escolhe UM elemento de uma sequência.",
                A("ganhador = random.choice(clientes)"),
                A("Funções de módulo são chamadas como modulo.funcao(...). Escolher, em inglês, é choose...", "ganhador = random.______(clientes)"),
                A("random.choice(clientes)", "Escolhe um item ao acaso.", "ganhador =", "Guarda o nome sorteado."),
                "random.sample(lista, 3) sorteia 3 sem repetir. Para senhas e tokens use o módulo secrets, nunca o random."));

            list.Add(C(P, 0, "code", "Tem no cardápio?",
                "Um cliente perguntou se ainda tem Mocha. A lista cardapio tem os nomes das bebidas do dia.",
                "Guarde em tem_mocha o resultado (True/False) de verificar se o texto \"Mocha\" está na lista cardapio, usando o operador in.",
                A("tem_mocha = \"Mocha\" in cardapio", "tem_mocha = (\"Mocha\" in cardapio)"),
                A("Existe um operador que testa se um item pertence a uma sequência e devolve um booleano.", "tem_mocha = \"Mocha\" __ cardapio"),
                A("\"Mocha\" in cardapio", "True se o item estiver na lista, False se não.", "tem_mocha =", "Guarda o booleano."),
                "not in faz o contrário. Em coleções grandes, procurar num set é bem mais rápido que numa lista."));

            list.Add(C(P, 0, "code", "Itens separados por vírgula",
                "O recibo precisa mostrar os itens numa linha só: Latte, Bolo, Croissant.",
                "Guarde em texto os itens da lista itens unidos pelo separador \", \" (vírgula e espaço), usando o método join.",
                A("texto = \", \".join(itens)"),
                A("join é um método do SEPARADOR (um texto), e recebe a lista como argumento.", "texto = \", \".____(itens)"),
                A("\", \"", "Separador colocado entre os itens.", ".join(itens)", "Une todos os itens da lista num texto só."),
                "join só aceita textos: para números use \", \".join(str(n) for n in numeros)."));

            list.Add(C(P, 0, "code", "Tirando do carrinho",
                "O cliente desistiu do Bolo. A lista carrinho tem um único \"Bolo\" que precisa sair.",
                "Remova o item \"Bolo\" da lista carrinho usando o método que remove pelo VALOR (não pela posição).",
                A("carrinho.remove(\"Bolo\")"),
                A("pop remove pela posição; outro método remove pelo valor.", "carrinho.______(\"Bolo\")"),
                A("carrinho.remove(", "Remove a primeira ocorrência do valor.", "\"Bolo\")", "Valor a remover (ValueError se ele não existir)."),
                "Para apagar pela posição use del carrinho[0] ou carrinho.pop(0)."));

            list.Add(C(P, 0, "code", "Quebrando a linha do caixa",
                "O sistema antigo do caixa exporta cada venda como \"Latte|12.50|2\", com os campos separados por barra vertical. A Dani precisa separar os campos.",
                "Guarde em partes a lista resultante de dividir o texto linha a cada barra vertical (|), usando o método split.",
                A("partes = linha.split(\"|\")"),
                A("Textos têm um método que corta pelo separador e devolve uma lista.", "partes = linha._____(\"|\")"),
                A("linha.split(\"|\")", "Corta o texto em cada | e devolve uma lista.", "partes", "[\"Latte\", \"12.50\", \"2\"] — tudo ainda como texto."),
                "Sem argumento, split() corta em qualquer espaço em branco. Para CSV de verdade, use o módulo csv (ele trata aspas e separadores dentro dos campos)."));

            list.Add(C(P, 0, "code", "Data de hoje",
                "O relatório diário precisa sair com a data do dia. No topo do script já existe from datetime import date.",
                "Guarde em hoje a data atual, usando o método da classe date que devolve a data de hoje.",
                A("hoje = date.today()"),
                A("A classe date tem um método com o nome \"hoje\" em inglês.", "hoje = date._____()"),
                A("date.today()", "Data atual (ano, mês, dia), sem horário.", "hoje =", "Guarda a data."),
                "Para data E hora use datetime.now(). hoje.year, hoje.month e hoje.day dão as partes da data."));

            list.Add(C(P, 0, "code", "Novo sabor no estoque",
                "Chegaram 12 Croissants. O dicionário estoque guarda produto → quantidade, e o Croissant ainda não está nele.",
                "Adicione ao dicionário estoque a chave \"Croissant\" com o valor 12.",
                A("estoque[\"Croissant\"] = 12", "estoque.update({\"Croissant\": 12})", "estoque.update(Croissant=12)"),
                A("Atribuir a uma chave que ainda não existe cria a chave.", "estoque[\"_________\"] = 12"),
                A("estoque[\"Croissant\"]", "Acessa a chave (e cria, se não existir).", "= 12", "Valor guardado nela."),
                "Se a chave já existir, o valor é substituído. Para somar ao que já tem: estoque[\"Croissant\"] = estoque.get(\"Croissant\", 0) + 12."));

            list.Add(C(P, 0, "code", "Quantos Espressos?",
                "A lista pedidos tem o nome de cada bebida vendida hoje. O barista quer saber quantos Espressos saíram.",
                "Guarde em vezes quantas vezes o texto \"Espresso\" aparece na lista pedidos, usando o método de lista count.",
                A("vezes = pedidos.count(\"Espresso\")"),
                A("Listas têm um método que conta as ocorrências de um valor.", "vezes = pedidos._____(\"Espresso\")"),
                A("pedidos.count(", "Conta quantas vezes o valor aparece na lista.", "\"Espresso\")", "Valor procurado (comparação exata)."),
                "Para contar TODOS os itens de uma vez, collections.Counter(pedidos) é mais eficiente do que vários count()."));

            list.Add(C(P, 0, "cmd", "Instalando pelo requirements",
                "O Otávio copiou o projeto dados-vendas para uma máquina nova. A lista de pacotes do projeto está no arquivo requirements.txt.",
                "Com o pip, instale de uma vez todos os pacotes listados em requirements.txt.",
                Cmds(pipVar, "install -r requirements.txt").Concat(A("pip install --requirement requirements.txt")).ToArray(),
                A("O pip install tem uma opção para ler os pacotes de um arquivo.", "pip install -_ requirements.txt"),
                A("pip install", "Instala pacotes.", "-r requirements.txt", "Lê a lista de pacotes (um por linha, com a versão) do arquivo."),
                "Faça isso sempre com o venv ativado, para não misturar pacotes de projetos diferentes."));

            list.Add(C(P, 0, "code", "Saudação com padrão",
                "O totem de autoatendimento cumprimenta o cliente pelo nome, mas nem todo mundo se identifica.",
                "Crie a função saudacao com o parâmetro nome valendo \"cliente\" por padrão, que retorna a f-string Oi, {nome}!",
                A("def saudacao(nome=\"cliente\"):\n    return f\"Oi, {nome}!\""),
                A("O valor padrão vai na própria assinatura da função: parametro=valor.", "def saudacao(nome=\"_______\"):\n    return f\"Oi, {____}!\""),
                A("nome=\"cliente\"", "Valor usado quando a função é chamada sem argumento.", "return f\"Oi, {nome}!\"", "Devolve o texto com o nome embutido."),
                "saudacao() devolve Oi, cliente! e saudacao(\"Ana\") devolve Oi, Ana!. Parâmetros com padrão vêm sempre depois dos obrigatórios."));

            list.Add(C(P, 0, "code", "Preços em ordem",
                "A lista precos precisa ficar em ordem crescente para o gráfico — e tudo bem alterar a própria lista.",
                "Ordene a própria lista precos em ordem crescente, usando o método de lista (sem criar uma lista nova).",
                A("precos.sort()"),
                A("Existe a função sorted (que cria outra lista) e um método da própria lista.", "precos.____()"),
                A("precos.sort()", "Ordena a lista no lugar e devolve None.", "( )", "Sem argumentos: ordem crescente."),
                "Nunca faça precos = precos.sort(): o método devolve None. Para decrescente: precos.sort(reverse=True)."));

            list.Add(C(P, 0, "code", "Raiz quadrada com math",
                "A vitrine nova é quadrada e tem area metros quadrados. O marceneiro quer saber o tamanho do lado. O módulo math já está importado.",
                "Guarde em lado a raiz quadrada de area, usando a função do módulo math.",
                A("lado = math.sqrt(area)"),
                A("Raiz quadrada em inglês é square root — a função do math tem um nome abreviado disso.", "lado = math.____(area)"),
                A("math.sqrt(area)", "Raiz quadrada (sempre devolve float).", "lado =", "Guarda o resultado."),
                "math também tem ceil (arredonda para cima), floor (para baixo) e pi. area ** 0.5 dá o mesmo resultado."));

            // ═════════════════════════════ INTERMEDIÁRIO ═════════════════════════════
            list.Add(C(P, 1, "code", "Os mais pedidos",
                "A Yara quer o top 3 de bebidas do mês. A lista pedidos tem um nome de bebida por venda, e from collections import Counter já está no topo do script.",
                "Em uma linha, guarde em top3 as 3 bebidas mais frequentes de pedidos, usando Counter e o método most_common.",
                A("top3 = Counter(pedidos).most_common(3)"),
                A("Counter conta tudo de uma vez (item → quantidade) e tem um método para os mais frequentes.", "top3 = Counter(pedidos).___________(3)"),
                A("Counter(pedidos)", "Conta cada bebida: {\"Latte\": 40, \"Mocha\": 31, ...}.", ".most_common(3)", "Lista com os 3 pares (bebida, quantidade) mais frequentes."),
                "Counter devolve 0 para itens que não existem (sem KeyError) e dois Counters podem ser somados com +."));

            list.Add(C(P, 1, "code", "E-mail limpo",
                "No cadastro de fidelidade, clientes digitam o e-mail com espaços e maiúsculas (\"  Ana@Mail.com \") e depois o login não encontra a conta.",
                "Em uma linha, guarde em email o próprio email sem espaços nas pontas e todo em minúsculas, encadeando os métodos strip e lower.",
                A("email = email.strip().lower()", "email = email.lower().strip()"),
                A("Cada método de texto devolve um texto novo — dá para chamar outro método logo em seguida.", "email = email._____()._____()"),
                A("email.strip()", "Remove espaços (e quebras de linha) do início e do fim.", ".lower()", "Converte tudo para minúsculas.", "email =", "Guarda o texto limpo."),
                "Normalize os dados na ENTRADA (cadastro) e também na busca, assim as comparações sempre batem."));

            list.Add(C(P, 1, "code", "Subtotais com zip",
                "As listas precos e quantidades estão alinhadas: precos[0] é o preço do primeiro item e quantidades[0] é quanto foi vendido dele.",
                "Crie em uma linha a lista subtotais com o preço vezes a quantidade de cada item, usando list comprehension com zip (variáveis do laço: p e q).",
                PyVar("subtotais = [§0 for §1 in zip(precos, quantidades)]", A("p * q", "q * p"), A("p, q", "(p, q)")),
                A("zip junta as duas listas em pares (preço, quantidade) — cada par pode ser desempacotado em p e q.", "subtotais = [p * q for p, q in zip(______, ___________)]"),
                A("zip(precos, quantidades)", "Pares (preço, quantidade), lado a lado.", "for p, q in", "Desempacota cada par.", "p * q", "Subtotal de cada item."),
                "zip para na lista MAIS CURTA; no Python 3.10+ use zip(a, b, strict=True) para dar erro se os tamanhos forem diferentes."));

            list.Add(C(P, 1, "code", "Clientes fiéis",
                "A Dani tem os sets clientes_jan e clientes_fev com os CPFs de quem comprou em cada mês. Quem comprou nos dois meses ganha um cupom.",
                "Guarde em fieis os clientes que estão nos dois conjuntos, usando o OPERADOR de interseção de sets.",
                A("fieis = clientes_jan & clientes_fev", "fieis = clientes_fev & clientes_jan"),
                A("Sets têm operadores da matemática: | (união), & (interseção) e - (diferença).", "fieis = clientes_jan _ clientes_fev"),
                A("&", "Interseção: só o que está nos dois conjuntos.", "clientes_jan, clientes_fev", "Os dois conjuntos comparados."),
                "clientes_jan - clientes_fev mostra quem NÃO voltou em fevereiro — ótimo para campanhas de retorno."));

            list.Add(C(P, 1, "code", "Fila do caixa",
                "No horário de pico, a lista fila guarda os nomes dos clientes na ordem de chegada. O caixa atende até ela esvaziar.",
                "Escreva um while que roda enquanto fila não estiver vazia (use a própria lista como condição). Dentro, tire o PRIMEIRO nome com pop(0), guardando em cliente, e mostre com print e f-string Atendendo seguido do nome.",
                PyVar("while §0:\n    cliente = fila.pop(0)\n    print(f\"Atendendo {cliente}\")", A("fila", "len(fila) > 0", "len(fila) != 0", "fila != []")),
                A("Lista vazia vale False numa condição; pop(0) remove e devolve o primeiro item.", "while fila:\n    cliente = fila.pop(_)\n    print(f\"Atendendo {_______}\")"),
                A("while fila:", "Repete enquanto houver alguém na lista.", "fila.pop(0)", "Remove e devolve o primeiro da fila.", "print(f\"Atendendo {cliente}\")", "Mostra quem está sendo atendido."),
                "Para filas grandes use collections.deque e popleft(): pop(0) numa lista precisa mover todos os outros itens."));

            list.Add(C(P, 1, "code", "Fechamento em arquivo",
                "No fim do expediente, o script precisa gravar o total do dia num arquivo de texto para a contabilidade.",
                "Com with open(...) as f, abra fechamento.txt para escrita (modo \"w\") com encoding=\"utf-8\" e escreva nele, com f.write e f-string, o texto Total: seguido do valor de total.",
                PyVar("with open(\"fechamento.txt\", §0) as f:\n    f.write(f\"Total: {total}§1\")",
                    A("\"w\", encoding=\"utf-8\"", "mode=\"w\", encoding=\"utf-8\"", "encoding=\"utf-8\", mode=\"w\""), A("", "\\n")),
                A("O modo \"w\" cria o arquivo (ou apaga o conteúdo antigo); o método write grava um texto.", "with open(\"fechamento.txt\", \"w\", encoding=\"utf-8\") as f:\n    f.write(f\"Total: {_____}\")"),
                A("\"w\"", "Modo escrita: cria o arquivo ou sobrescreve o que havia.", "encoding=\"utf-8\"", "Garante acentos corretos em qualquer sistema.", "f.write(...)", "Grava o texto (write não pula linha sozinho)."),
                "Use \"a\" para acrescentar ao fim sem apagar o que já existe. write só aceita str: converta números com f-string ou str()."));

            list.Add(C(P, 1, "code", "Lendo pedido a pedido",
                "O arquivo pedidos.txt tem um pedido por linha. A Yara quer mostrar cada um sem a quebra de linha que vem no final.",
                "Abra pedidos.txt com with open(\"pedidos.txt\", encoding=\"utf-8\") as f:, percorra o arquivo com for linha in f: e mostre cada linha com print, sem espaços/quebras nas pontas (método strip).",
                PyVar("with open(\"pedidos.txt\", §0) as f:\n    for linha in f:\n        print(linha.strip())",
                    A("encoding=\"utf-8\"", "\"r\", encoding=\"utf-8\"", "mode=\"r\", encoding=\"utf-8\"", "encoding=\"utf-8\", mode=\"r\"")),
                A("Um arquivo aberto pode ser percorrido com for, uma linha por vez, sem carregar tudo na memória.", "with open(...) as f:\n    for linha in f:\n        print(linha._____())"),
                A("for linha in f:", "Lê uma linha por vez (eficiente até para arquivos enormes).", "linha.strip()", "Tira o \\n do fim (e espaços nas pontas).", "print(...)", "Mostra o pedido."),
                "Evite f.readlines() em arquivos grandes: ele carrega o arquivo inteiro na memória de uma vez."));

            list.Add(C(P, 1, "code", "Clientes do CSV",
                "O CRM exporta clientes.csv com cabeçalho (nome,email,cidade). O módulo csv já está importado.",
                "Abra clientes.csv com with open(\"clientes.csv\", newline=\"\", encoding=\"utf-8\") as f:, percorra csv.DictReader(f) com for linha in ... e mostre com print o campo nome de cada linha.",
                PyVar("with open(\"clientes.csv\", §0) as f:\n    §1",
                    A("newline=\"\", encoding=\"utf-8\"", "encoding=\"utf-8\", newline=\"\""),
                    A("for linha in csv.DictReader(f):\n        print(linha[\"nome\"])",
                      "leitor = csv.DictReader(f)\n    for linha in leitor:\n        print(linha[\"nome\"])")),
                A("DictReader usa a primeira linha (o cabeçalho) como chaves do dicionário de cada linha.", "with open(...) as f:\n    for linha in csv.DictReader(f):\n        print(linha[\"____\"])"),
                A("newline=\"\"", "Recomendação da documentação do csv: o módulo cuida das quebras de linha.", "csv.DictReader(f)", "Cada linha vira um dicionário {\"nome\": ..., \"email\": ..., \"cidade\": ...}.", "linha[\"nome\"]", "Acessa a coluna pelo nome do cabeçalho."),
                "Para gravar, use csv.DictWriter com writeheader() e writerow(). Tudo que o csv lê chega como texto."));

            list.Add(C(P, 1, "code", "Pedido em JSON",
                "O app de delivery espera o pedido num arquivo JSON legível e com os acentos intactos (\"Pão de queijo\", não \"P\\u00e3o de queijo\"). O módulo json já está importado.",
                "Com with open(\"pedido.json\", \"w\", encoding=\"utf-8\") as f:, grave o dicionário pedido no arquivo usando json.dump com ensure_ascii=False e indent=2.",
                PyVar("with open(\"pedido.json\", §0) as f:\n    json.dump(pedido, f, §1)",
                    A("\"w\", encoding=\"utf-8\"", "mode=\"w\", encoding=\"utf-8\"", "encoding=\"utf-8\", mode=\"w\""),
                    A("ensure_ascii=False, indent=2", "indent=2, ensure_ascii=False")),
                A("json.dump(dados, arquivo, ...) grava direto no arquivo aberto; ensure_ascii=False mantém os acentos e indent formata.", "json.dump(pedido, f, ensure_ascii=_____, indent=_)"),
                A("json.dump(pedido, f", "Converte o dicionário em JSON e grava no arquivo.", "ensure_ascii=False", "Grava ã, é, ç como estão (sem \\u00e3).", "indent=2", "Uma chave por linha, com recuo de 2 espaços."),
                "json.dumps (com s) devolve o texto em vez de gravar. Datas e Decimal não são serializáveis direto: use default=str."));

            list.Add(C(P, 1, "code", "Pasta de relatórios",
                "O script salva PDFs em relatorios/2025/outubro, mas quebra com FileNotFoundError na máquina do Otávio, onde as pastas ainda não existem. Path já foi importado de pathlib.",
                "Em uma linha, crie a pasta relatorios/2025/outubro com Path(...).mkdir, criando também as pastas-pai que faltarem e sem dar erro se ela já existir.",
                PyVar("§0.mkdir(§1)", A("Path(\"relatorios/2025/outubro\")", "(Path(\"relatorios\") / \"2025\" / \"outubro\")"),
                    A("parents=True, exist_ok=True", "exist_ok=True, parents=True")),
                A("mkdir tem duas opções booleanas: uma cria as pastas intermediárias, a outra ignora se a pasta já existir.", "Path(\"relatorios/2025/outubro\").mkdir(parents=____, exist_ok=____)"),
                A("Path(\"relatorios/2025/outubro\")", "Caminho como objeto (funciona em Windows e Linux).", "parents=True", "Cria relatorios e 2025 se faltarem.", "exist_ok=True", "Não lança erro se a pasta já existir."),
                "Com pathlib, o operador / monta caminhos: Path(\"relatorios\") / \"2025\". O equivalente antigo é os.makedirs(caminho, exist_ok=True)."));

            list.Add(C(P, 1, "code", "Ambiente pela variável",
                "O mesmo script roda no notebook da Yara e no servidor. Quem diz onde ele está é a variável de ambiente AMBIENTE — que nem sempre foi definida. O módulo os já está importado.",
                "Guarde em ambiente o valor da variável de ambiente AMBIENTE, usando \"dev\" como padrão quando ela não existir.",
                A("ambiente = os.getenv(\"AMBIENTE\", \"dev\")", "ambiente = os.environ.get(\"AMBIENTE\", \"dev\")"),
                A("O módulo os lê variáveis de ambiente com uma função que aceita valor padrão, como o get do dicionário.", "ambiente = os.______(\"AMBIENTE\", \"dev\")"),
                A("os.getenv(\"AMBIENTE\"", "Lê a variável de ambiente (None se não existir).", "\"dev\")", "Valor usado quando ela não foi definida."),
                "Configurações e segredos ficam em variáveis de ambiente, nunca escritos no código. os.environ[\"X\"] lança KeyError se faltar — bom para o que é obrigatório."));

            list.Add(C(P, 1, "code", "Prazo de entrega",
                "Bolos personalizados ficam prontos 3 dias depois da encomenda. A variável hoje é um date e timedelta já foi importado de datetime.",
                "Guarde em pronto a data de hoje mais 3 dias, usando timedelta.",
                A("pronto = hoje + timedelta(days=3)", "pronto = timedelta(days=3) + hoje", "pronto = hoje + timedelta(3)"),
                A("Somar um date com um timedelta (uma duração) dá outra data.", "pronto = hoje + timedelta(____=3)"),
                A("timedelta(days=3)", "Uma duração de 3 dias.", "hoje + ...", "Data resultante (vira o mês ou o ano sozinha, se precisar)."),
                "Subtrair duas datas dá um timedelta: (pronto - hoje).days == 3. timedelta não tem months: para meses use dateutil.relativedelta."));

            list.Add(C(P, 1, "code", "Soma flexível",
                "O caixa às vezes soma 2 itens, às vezes 7. O Otávio quer uma função só, que aceite qualquer quantidade de valores.",
                "Crie a função somar que recebe qualquer quantidade de argumentos posicionais em *valores e retorna a soma deles com sum.",
                A("def somar(*valores):\n    return sum(valores)"),
                A("Um asterisco antes do parâmetro junta todos os argumentos posicionais numa tupla.", "def somar(*_______):\n    return sum(_______)"),
                A("*valores", "somar(1, 2, 3) chega como a tupla (1, 2, 3).", "return sum(valores)", "Soma a tupla."),
                "Para passar uma lista pronta, desempacote na chamada: somar(*minha_lista)."));

            list.Add(C(P, 1, "code", "Ficha do cliente",
                "Cada cliente do programa de fidelidade tem campos diferentes (uns têm telefone, outros aniversário). A Dani quer imprimir a ficha de qualquer um.",
                "Crie a função ficha(**dados) que percorre dados.items() com for (variáveis campo e valor) e mostra cada par com print e f-string no formato campo: valor.",
                PyVar("def ficha(**dados):\n    for §0 in dados.items():\n        print(f\"{campo}: {valor}\")", A("campo, valor", "(campo, valor)")),
                A("Dois asteriscos juntam os argumentos NOMEADOS num dicionário.", "def ficha(**dados):\n    for campo, valor in dados._____():\n        print(f\"{_____}: {_____}\")"),
                A("**dados", "ficha(nome=\"Ana\", cidade=\"Recife\") vira {\"nome\": \"Ana\", \"cidade\": \"Recife\"}.", "dados.items()", "Pares (campo, valor).", "print(f\"{campo}: {valor}\")", "Uma linha por campo."),
                "Ordem dos parâmetros: normais, *args, nomeados, **kwargs. Na chamada, **dicionario desempacota um dicionário em argumentos nomeados."));

            list.Add(C(P, 1, "code", "Preço tipado",
                "O dados-vendas cresceu e o editor não ajuda a achar erros. A Yara pediu type hints em todas as funções novas.",
                "Crie a função com_taxa com os parâmetros preco: float e taxa: float = 0.1, retorno anotado como float, que retorna preco * (1 + taxa).",
                PyVar("def com_taxa(preco: float, taxa: float = 0.1) -> float:\n    return §0", A("preco * (1 + taxa)", "preco * (taxa + 1)", "(1 + taxa) * preco", "preco + preco * taxa")),
                A("A anotação vem depois do nome do parâmetro, com dois-pontos; o tipo de retorno vem com -> antes dos dois-pontos finais.", "def com_taxa(preco: float, taxa: float = 0.1) -> _____:\n    return ..."),
                A("preco: float", "Anota o tipo esperado do parâmetro.", "taxa: float = 0.1", "Anotação com valor padrão.", "-> float", "Tipo do valor devolvido."),
                "O Python não impõe os tipos ao executar: quem confere é o editor ou ferramentas como o mypy."));

            list.Add(C(P, 1, "code", "Pedido como dataclass",
                "O Otávio cansou de escrever __init__ só para guardar atributos. Para pedidos, a Yara sugeriu dataclass (já importado de dataclasses).",
                "Crie a dataclass Pedido (decorator @dataclass) com os campos cliente: str e total: float, nessa ordem.",
                A("@dataclass\nclass Pedido:\n    cliente: str\n    total: float"),
                A("Na dataclass, os campos são declarados com anotação de tipo no corpo da classe — o __init__ é gerado sozinho.", "@dataclass\nclass Pedido:\n    cliente: ___\n    total: _____"),
                A("@dataclass", "Gera __init__, __repr__ e __eq__ automaticamente.", "cliente: str", "Primeiro campo (primeiro parâmetro do __init__).", "total: float", "Segundo campo."),
                "Pedido(\"Ana\", 25.5) já funciona e aparece bonito no print. Use @dataclass(frozen=True) para objetos imutáveis."));

            list.Add(C(P, 1, "code", "Primeiro teste com pytest",
                "A função aplicar_desconto(preco, percentual) quebrou em produção na semana passada. A Dani decidiu: agora tudo tem teste.",
                "Escreva a função de teste test_desconto (sem parâmetros) que usa assert para conferir que aplicar_desconto(100, 10) é igual a 90.",
                PyVar("def test_desconto():\n    assert §0", A("aplicar_desconto(100, 10) == 90", "90 == aplicar_desconto(100, 10)")),
                A("O pytest roda as funções cujo nome começa com test_; dentro delas, assert confere a condição.", "def test_desconto():\n    assert aplicar_desconto(___, __) == __"),
                A("def test_desconto():", "O prefixo test_ faz o pytest encontrar a função.", "assert ... == 90", "Se a condição for falsa, o teste falha e mostra os dois valores."),
                "O arquivo também precisa começar com test_ (ex.: test_precos.py). Teste os casos de borda: 0%, 100% e valores inválidos."));

            list.Add(C(P, 1, "code", "O mais caro",
                "A lista produtos tem dicionários como {\"nome\": \"Latte\", \"preco\": 12.5}. O banner da loja quer mostrar só o produto mais caro.",
                "Guarde em mais_caro o dicionário de maior preço da lista produtos, usando max com key e uma lambda de parâmetro p.",
                A("mais_caro = max(produtos, key=lambda p: p[\"preco\"])", "mais_caro = max(produtos, key=lambda p: p.get(\"preco\"))"),
                A("max, assim como sorted, aceita key= para dizer O QUE comparar em cada item.", "mais_caro = max(produtos, key=lambda p: p[\"_____\"])"),
                A("max(produtos,", "Devolve o ITEM maior (o dicionário inteiro).", "key=lambda p: p[\"preco\"]", "Compara pelo preço de cada produto."),
                "Melhor do que ordenar só para pegar o primeiro: max percorre a lista uma vez. Em lista vazia dá ValueError, a menos que use default=None."));

            list.Add(C(P, 1, "code", "Coluna de total",
                "O DataFrame df tem as colunas preco e quantidade de cada venda. A Yara quer uma coluna com o valor de cada linha.",
                "Crie em df a coluna total com preco multiplicado por quantidade (operação entre colunas, sem laço).",
                PyVar("df[\"total\"] = §0", A("df[\"preco\"] * df[\"quantidade\"]", "df[\"quantidade\"] * df[\"preco\"]", "df.preco * df.quantidade", "df.quantidade * df.preco")),
                A("Operações entre colunas são feitas linha a linha automaticamente (vetorizado).", "df[\"total\"] = df[\"_____\"] * df[\"__________\"]"),
                A("df[\"total\"] =", "Cria (ou substitui) a coluna total.", "df[\"preco\"] * df[\"quantidade\"]", "Multiplica as colunas linha a linha, sem for."),
                "Evite percorrer DataFrames com for/iterrows: operações vetorizadas são dezenas de vezes mais rápidas. E df.total = ... NÃO cria coluna nova."));

            list.Add(C(P, 1, "code", "Maiores vendas no topo",
                "Para a reunião de segunda, a Dani quer as 5 vendas de maior valor do DataFrame df.",
                "Guarde em top5 as 5 linhas de df com maior valor na coluna valor: ordene com sort_values em ordem decrescente e pegue as 5 primeiras com head.",
                PyVar("top5 = df.sort_values(§0).head(5)", A("\"valor\", ascending=False", "by=\"valor\", ascending=False", "ascending=False, by=\"valor\"")),
                A("sort_values ordena por uma coluna; por padrão a ordem é crescente.", "top5 = df.sort_values(\"valor\", ascending=_____).head(_)"),
                A("sort_values(\"valor\"", "Ordena as linhas pela coluna valor.", "ascending=False", "Maior primeiro.", ".head(5)", "As 5 primeiras linhas."),
                "df.nlargest(5, \"valor\") faz o mesmo de forma mais rápida. Para ordenar por várias colunas: sort_values([\"loja\", \"valor\"])."));

            list.Add(C(P, 1, "code", "Status da API",
                "O time de infraestrutura precisa de uma rota para monitorar se a API de relatórios (Flask, objeto app) está no ar.",
                "Crie a rota /status com @app.route e, logo abaixo, a função status() que retorna o dicionário {\"ok\": True} (o Flask converte em JSON).",
                PyVar("§0\ndef status():\n    return §1",
                    A("@app.route(\"/status\")", "@app.get(\"/status\")", "@app.route(\"/status\", methods=[\"GET\"])"),
                    A("{\"ok\": True}", "jsonify({\"ok\": True})", "jsonify(ok=True)")),
                A("Rota = decorator + função logo abaixo. No Flask 2+, retornar um dicionário já vira JSON.", "@app.route(\"/status\")\ndef status():\n    return {\"__\": ____}"),
                A("@app.route(\"/status\")", "Liga a URL /status à função.", "def status():", "Função que trata a requisição.", "return {\"ok\": True}", "Vira a resposta JSON {\"ok\": true} com status 200."),
                "Rotas de health check devem ser leves (nada de consultas pesadas): load balancers e orquestradores chamam essa URL o tempo todo."));

            list.Add(C(P, 1, "cmd", "Congelando dependências",
                "Tudo funcionando no venv do Otávio! Antes de subir o código, ele precisa registrar as versões exatas de todos os pacotes instalados.",
                "Gere o arquivo requirements.txt com a lista de pacotes instalados e suas versões, usando o pip e o redirecionamento do terminal (>).",
                Cmds(pipVar, "freeze > requirements.txt"),
                A("O pip tem um comando que lista os pacotes no formato pacote==versão; o > do terminal manda essa saída para um arquivo.", "pip ______ > requirements.txt"),
                A("pip freeze", "Lista os pacotes instalados como pacote==versão.", "> requirements.txt", "Redireciona a saída para o arquivo (sobrescreve)."),
                "Rode o freeze com o venv ATIVO; senão você congela os pacotes do Python global."));

            list.Add(C(P, 1, "cmd", "Versão fixa",
                "Uma versão nova de biblioteca quebrou o script ontem. A Yara decidiu travar o requests na versão 2.32.3, que é a testada.",
                "Instale com o pip exatamente a versão 2.32.3 do pacote requests.",
                Cmds(pipVar, "install requests==2.32.3"),
                A("O pip aceita um especificador de versão grudado no nome do pacote.", "pip install requests==_._._"),
                A("pip install", "Instala (ou troca para) a versão indicada.", "requests==2.32.3", "== fixa exatamente essa versão."),
                "Outros especificadores: >=2.31 (mínima) e ~=2.32.0 (compatível: aceita 2.32.x). No terminal, use aspas nos que têm > ou <."));

            list.Add(C(P, 1, "cmd", "Rodando os testes",
                "Os testes de preço estão no arquivo tests/test_precos.py. A suíte inteira demora, e a Dani só quer rodar esse arquivo agora.",
                "Execute com o pytest apenas os testes do arquivo tests/test_precos.py.",
                A("pytest tests/test_precos.py", "python -m pytest tests/test_precos.py", "python3 -m pytest tests/test_precos.py", "py -m pytest tests/test_precos.py",
                  "pytest tests\\test_precos.py", "python -m pytest tests\\test_precos.py", "py -m pytest tests\\test_precos.py"),
                A("O pytest aceita o caminho de um arquivo de teste como argumento.", "pytest tests/________.py"),
                A("pytest", "Executor de testes (pip install pytest).", "tests/test_precos.py", "Roda só os testes desse arquivo."),
                "pytest -v mostra o nome de cada teste; pytest tests/test_precos.py::test_desconto roda um único teste."));

            // ═════════════════════════════ PROFISSIONAL ═════════════════════════════
            list.Add(C(P, 2, "code", "Pedidos por loja",
                "A lista vendas tem tuplas (loja, valor). O relatório precisa de um dicionário loja → lista de valores, e o código antigo tinha um if \"essa loja já existe?\" a cada volta. from collections import defaultdict já está no script.",
                "Crie por_loja como defaultdict(list) e, com for loja, valor in vendas:, acrescente cada valor na lista da sua loja com append.",
                PyVar("por_loja = defaultdict(list)\nfor §0 in vendas:\n    por_loja[loja].append(valor)", A("loja, valor", "(loja, valor)")),
                A("defaultdict cria o valor padrão sozinho na primeira vez em que a chave é acessada.", "por_loja = defaultdict(____)\nfor loja, valor in vendas:\n    por_loja[____].append(_____)"),
                A("defaultdict(list)", "Chave nova começa com lista vazia, sem KeyError.", "for loja, valor in vendas:", "Desempacota cada tupla.", "por_loja[loja].append(valor)", "Acrescenta o valor na lista da loja."),
                "Sem import: por_loja.setdefault(loja, []).append(valor). Para contar, use defaultdict(int) com += 1."));

            list.Add(C(P, 2, "code", "Busca sem SQL injection",
                "Auditoria de segurança: o código fazia cur.execute(f\"SELECT * FROM clientes WHERE email = '{email}'\") e alguém entrou digitando ' OR '1'='1 no campo de e-mail. O banco é SQLite e cur é o cursor.",
                "Reescreva a chamada a cur.execute com a consulta SELECT * FROM clientes WHERE email = ?, usando o placeholder ? e passando email numa TUPLA como segundo argumento.",
                A("cur.execute(\"SELECT * FROM clientes WHERE email = ?\", (email,))"),
                A("Nunca monte SQL com f-string: o driver troca os ? pelos valores de forma segura, separados do comando.", "cur.execute(\"SELECT * FROM clientes WHERE email = ?\", (_____,))"),
                A("email = ?", "Placeholder: o valor NÃO entra no texto do SQL.", "(email,)", "Tupla de um item (a vírgula é obrigatória) com os valores, na ordem dos ?.", "cur.execute(...)", "O driver envia o SQL e os valores separadamente ao banco."),
                "Vale para qualquer banco: psycopg (PostgreSQL) e o conector do MySQL usam %s como placeholder, mas a regra é a mesma — nunca concatene entrada do usuário no SQL."));

            list.Add(C(P, 2, "code", "Soma que não fecha",
                "O teste de fechamento de caixa falhou: soma == 0.3 deu False porque soma vale 0.30000000000000004 — culpa da representação binária dos floats. O módulo math já está importado.",
                "Guarde em confere o resultado de comparar soma com 0.3 de forma segura para floats, usando a função do módulo math feita para isso (com a tolerância padrão).",
                A("confere = math.isclose(soma, 0.3)", "confere = math.isclose(0.3, soma)"),
                A("Floats não devem ser comparados com ==; o math tem uma função que diz se dois números estão \"perto o suficiente\".", "confere = math.is_____(soma, 0.3)"),
                A("math.isclose(soma, 0.3)", "True se a diferença for desprezível (tolerância relativa padrão de 1e-09).", "confere =", "Guarda o booleano."),
                "Em testes, pytest.approx faz o mesmo: assert soma == pytest.approx(0.3). Para dinheiro, a solução definitiva é o Decimal."));

            list.Add(C(P, 2, "code", "Log em vez de print",
                "Em produção ninguém vê os print do script. O Otávio configurou o logging, e agora cada módulo deve usar o próprio logger.",
                "Em duas linhas: crie logger com logging.getLogger(__name__) e registre um aviso (nível warning) com a mensagem \"Estoque baixo: %s\", passando produto como argumento separado (formatação preguiçosa, sem f-string).",
                A("logger = logging.getLogger(__name__)\nlogger.warning(\"Estoque baixo: %s\", produto)"),
                A("getLogger(__name__) dá um logger com o nome do módulo; os níveis são debug, info, warning, error e critical.", "logger = logging.getLogger(________)\nlogger.warning(\"Estoque baixo: %s\", _______)"),
                A("logging.getLogger(__name__)", "Logger com o nome do módulo (aparece no log e pode ser configurado separadamente).", "logger.warning(", "Registra no nível de aviso.", "\"Estoque baixo: %s\", produto", "O texto só é montado se a mensagem for realmente registrada."),
                "Dentro de um except, logger.exception(\"...\") registra a mensagem E o traceback completo."));

            list.Add(C(P, 2, "code", "Testes em tabela",
                "A Dani escreveu 6 funções de teste quase iguais para aplicar_desconto. Dá para trocar tudo por um único teste com uma tabela de casos. O pytest já está importado.",
                "Use @pytest.mark.parametrize com os nomes \"preco, esperado\" e os casos (100, 90) e (50, 45) numa lista; depois escreva test_desconto(preco, esperado), que confere com assert que aplicar_desconto(preco, 10) == esperado.",
                PyVar("@pytest.mark.parametrize(§0, §1)\ndef test_desconto(preco, esperado):\n    assert §2",
                    A("\"preco, esperado\"", "[\"preco\", \"esperado\"]", "(\"preco\", \"esperado\")"),
                    A("[(100, 90), (50, 45)]", "[[100, 90], [50, 45]]"),
                    A("aplicar_desconto(preco, 10) == esperado", "esperado == aplicar_desconto(preco, 10)")),
                A("parametrize recebe os nomes dos parâmetros (num texto) e a lista de casos; cada caso vira uma execução do teste.", "@pytest.mark.parametrize(\"preco, esperado\", [(100, 90), (50, 45)])\ndef test_desconto(preco, esperado):\n    assert aplicar_desconto(_____, 10) == ________"),
                A("@pytest.mark.parametrize(", "Repete o teste para cada caso da tabela.", "\"preco, esperado\"", "Nomes que viram parâmetros da função de teste.", "[(100, 90), (50, 45)]", "Os casos: (entrada, resultado esperado).", "assert aplicar_desconto(preco, 10) == esperado", "A mesma verificação, com valores diferentes."),
                "Cada caso aparece separado no relatório: se um falhar, você vê exatamente qual. Use ids=[...] para dar nomes aos casos."));

            list.Add(C(P, 2, "code", "CSV gigante em pedaços",
                "O arquivo vendas_2025.csv tem 40 milhões de linhas e o pd.read_csv derrubou o servidor por falta de memória. Só precisamos da soma da coluna valor.",
                "Comece com total = 0 e percorra o arquivo com for parte in pd.read_csv(\"vendas_2025.csv\", chunksize=100000):, somando a cada volta o sum() da coluna valor de parte em total (use +=).",
                PyVar("total = 0\nfor parte in pd.read_csv(\"vendas_2025.csv\", chunksize=§0):\n    total += §1",
                    A("100000", "100_000"), A("parte[\"valor\"].sum()", "parte.valor.sum()")),
                A("Com chunksize, o read_csv devolve um iterador de DataFrames menores em vez de um gigante.", "total = 0\nfor parte in pd.read_csv(\"vendas_2025.csv\", chunksize=100000):\n    total += parte[\"_____\"].____()"),
                A("chunksize=100000", "Lê 100 mil linhas por vez: a memória fica constante.", "for parte in ...", "Cada parte é um DataFrame comum.", "total += parte[\"valor\"].sum()", "Acumula o resultado parcial."),
                "Também ajuda ler só as colunas necessárias: usecols=[\"valor\"]. Para análises grandes, formatos colunares como Parquet são muito mais leves que CSV."));

            list.Add(C(P, 2, "cmd", "Servidor de desenvolvimento",
                "A API nova em FastAPI está no arquivo main.py, no objeto app. A porta 8000 já está ocupada pela API antiga.",
                "Suba a API com o uvicorn na porta 8001, recarregando automaticamente quando o código mudar.",
                A("uvicorn main:app --reload --port 8001", "uvicorn main:app --reload --port=8001",
                  "python -m uvicorn main:app --reload --port 8001", "python3 -m uvicorn main:app --reload --port 8001", "py -m uvicorn main:app --reload --port 8001"),
                A("O uvicorn recebe modulo:objeto (arquivo sem .py, dois-pontos, nome da variável) e opções de porta e de recarga.", "uvicorn main:app --______ --port ____"),
                A("main:app", "Arquivo main.py, variável app.", "--reload", "Reinicia sozinho quando um arquivo muda.", "--port 8001", "Porta onde o servidor escuta."),
                "--reload é só para desenvolvimento. Em produção, rode sem ele, com --host 0.0.0.0 e vários processos (--workers 4)."));

            list.Add(C(P, 2, "code", "Preço nunca negativo",
                "Um bug de importação gravou produtos com preço negativo. A classe Produto já tem a @property preco, que devolve self._preco; falta validar na escrita.",
                "Escreva o setter dentro da classe: @preco.setter e def preco(self, valor):. Se valor for menor que 0, lance ValueError(\"valor negativo\"); senão, guarde valor em self._preco.",
                PyVar("@preco.setter\ndef preco(self, valor):\n    if §0:\n        raise ValueError(\"valor negativo\")\n    §1self._preco = valor",
                    A("valor < 0", "0 > valor"), A("", "else:\n        ")),
                A("O setter usa o MESMO nome da property, decorado com @nome.setter.", "@preco.setter\ndef preco(self, valor):\n    if valor < 0:\n        raise ValueError(\"valor negativo\")\n    self.______ = valor"),
                A("@preco.setter", "produto.preco = x passa a chamar esta função.", "raise ValueError(...)", "Recusa o valor inválido na hora, antes de gravar.", "self._preco = valor", "Guarda no atributo \"interno\" (com _)."),
                "Se o setter fizer self.preco = valor (sem o _), ele chama a si mesmo até dar RecursionError."));

            list.Add(C(P, 2, "code", "Token seguro",
                "O link de \"esqueci minha senha\" usava random.randint para gerar o código — previsível para um atacante. O módulo secrets já está importado.",
                "Guarde em token um texto aleatório seguro com 32 bytes de aleatoriedade, usando a função do módulo secrets que gera texto em Base64 seguro para URL.",
                A("token = secrets.token_urlsafe(32)", "token = secrets.token_urlsafe(nbytes=32)"),
                A("secrets foi feito para segurança (senhas, tokens, links); random é para simulações e jogos. As funções dele começam com token_.", "token = secrets.token_______(32)"),
                A("secrets", "Usa a fonte de aleatoriedade criptográfica do sistema operacional.", "token_urlsafe(32)", "32 bytes aleatórios em Base64 próprio para URL (cerca de 43 caracteres)."),
                "Para conferir o token recebido use secrets.compare_digest(recebido, esperado), que evita ataques de tempo. E dê validade curta ao link."));

            list.Add(C(P, 2, "code", "Vendas por mês",
                "A coluna data de df veio do CSV como texto (\"2025-03-14\"), e df[\"data\"].dt dá AttributeError.",
                "Em duas linhas: converta a coluna data de df para datetime com pd.to_datetime (sobrescrevendo a coluna) e guarde em mensal a soma de valor agrupando por df[\"data\"].dt.month.",
                PyVar("df[\"data\"] = pd.to_datetime(df[\"data\"])\nmensal = df.groupby(§0)§1",
                    A("df[\"data\"].dt.month", "df.data.dt.month"), A("[\"valor\"].sum()", ".valor.sum()")),
                A("O acessor .dt só existe em colunas do tipo datetime; to_datetime faz a conversão.", "df[\"data\"] = pd.to_datetime(df[\"____\"])\nmensal = df.groupby(df[\"data\"].dt._____)[\"valor\"].sum()"),
                A("pd.to_datetime(df[\"data\"])", "Converte o texto em datas de verdade.", ".dt.month", "Extrai o mês (1 a 12) de cada data.", "groupby(...)[\"valor\"].sum()", "Soma as vendas de cada mês."),
                "Dá para converter já na leitura: pd.read_csv(\"vendas.csv\", parse_dates=[\"data\"]). Com mais de um ano de dados, agrupe por dt.to_period(\"M\") para não misturar março/2024 com março/2025."));

            list.Add(C(P, 2, "code", "Erro com causa",
                "A função buscar(codigo) lê catalogo[codigo] e devolvia um KeyError cru para a API. A Dani criou a exceção ProdutoNaoEncontrado e quer manter o erro original no traceback.",
                "Escreva o bloco try/except: no try, retorne catalogo[codigo]; no except KeyError as e, lance ProdutoNaoEncontrado(codigo) encadeado ao erro original com from e.",
                A("try:\n    return catalogo[codigo]\nexcept KeyError as e:\n    raise ProdutoNaoEncontrado(codigo) from e"),
                A("raise NovaExcecao(...) from erro_original guarda a causa (atributo __cause__).", "try:\n    return catalogo[codigo]\nexcept KeyError as e:\n    raise ProdutoNaoEncontrado(codigo) ____ _"),
                A("except KeyError as e:", "Captura o erro técnico e guarda em e.", "raise ProdutoNaoEncontrado(codigo)", "Lança um erro com significado para o negócio.", "from e", "Encadeia: o traceback mostra as duas exceções e quem causou quem."),
                "Exceções próprias herdam de Exception: class ProdutoNaoEncontrado(Exception): pass. Use from None só quando quiser esconder a causa de propósito."));

            list.Add(C(P, 2, "code", "Testando o erro",
                "baixar_estoque(estoque, qtd) deve lançar ValueError quando qtd for maior que o estoque. Esse comportamento também precisa de teste. O pytest já está importado.",
                "Escreva test_estoque_insuficiente() que, dentro de with pytest.raises(ValueError):, chama baixar_estoque(5, 10).",
                A("def test_estoque_insuficiente():\n    with pytest.raises(ValueError):\n        baixar_estoque(5, 10)"),
                A("pytest.raises é usado com with: o teste passa só se o bloco lançar aquele erro.", "def test_estoque_insuficiente():\n    with pytest.______(ValueError):\n        baixar_estoque(5, 10)"),
                A("with pytest.raises(ValueError):", "Espera que o bloco lance ValueError.", "baixar_estoque(5, 10)", "Pede 10 com só 5 em estoque: precisa falhar."),
                "Use match= para conferir também a mensagem: pytest.raises(ValueError, match=\"insuficiente\")."));

            list.Add(C(P, 2, "code", "Itens sem compartilhar",
                "Na dataclass Comanda, alguém escreveu itens: list = [] e o Python recusou com ValueError: mutable default. Cada comanda precisa da própria lista. dataclass e field já foram importados de dataclasses.",
                "Escreva a dataclass Comanda com os campos mesa: int e itens: list[str], este último com valor padrão field(default_factory=list).",
                PyVar("@dataclass\nclass Comanda:\n    mesa: int\n    itens: §0 = field(default_factory=list)", A("list[str]", "List[str]")),
                A("field(default_factory=...) chama a função a cada objeto novo, criando uma lista nova para cada comanda.", "@dataclass\nclass Comanda:\n    mesa: int\n    itens: list[str] = field(default_factory=____)"),
                A("mesa: int", "Campo obrigatório.", "itens: list[str]", "Lista de textos (type hint).", "field(default_factory=list)", "Chama list() para cada Comanda: nada de lista compartilhada."),
                "É o mesmo problema do argumento padrão mutável em funções: nunca compartilhe uma lista ou dicionário como valor padrão."));

            list.Add(C(P, 2, "cmd", "Onde o script demora",
                "O relatorio.py passou de 10 segundos para 4 minutos e ninguém sabe por quê. Antes de sair otimizando no chute, a Yara quer medir.",
                "Execute relatorio.py com o profiler embutido do Python (o módulo cProfile, via -m), ordenando o resultado pelo tempo acumulado (-s cumtime).",
                pyVar.SelectMany(p => A(p + " -m cProfile -s cumtime relatorio.py", p + " -m cProfile -s cumulative relatorio.py")).ToArray(),
                A("Módulos executáveis rodam com python -m modulo; o cProfile recebe a ordenação e depois o script.", "python -m cProfile -s _______ relatorio.py"),
                A("python -m cProfile", "Roda o script medindo cada função.", "-s cumtime", "Ordena pelo tempo acumulado (a função e tudo o que ela chama).", "relatorio.py", "Script analisado."),
                "Meça antes de otimizar: quase sempre 1 ou 2 funções concentram o tempo. Para trechos pequenos, o módulo timeit é mais preciso."));

            list.Add(C(P, 2, "code", "Centavos exatos",
                "Para valores em dinheiro, o financeiro exige o módulo decimal. Decimal já foi importado de decimal, e um Latte custa 19.90.",
                "Guarde em total o preço Decimal de 19.90 — criado a partir de TEXTO — multiplicado por 3.",
                A("total = Decimal(\"19.90\") * 3", "total = 3 * Decimal(\"19.90\")"),
                A("Decimal criado a partir de float já nasce com o erro binário; crie a partir de str.", "total = Decimal(\"_____\") * 3"),
                A("Decimal(\"19.90\")", "Valor exato, sem erro de arredondamento binário.", "* 3", "Resultado exato: Decimal('59.70')."),
                "Decimal(19.90) (com float) vira 19.899999999999998578... Para arredondar centavos: total.quantize(Decimal(\"0.01\"))."));

            list.Add(C(P, 2, "code", "Recebendo pedido no Flask",
                "O app do Café 365 envia pedidos em JSON para a API Flask (request já foi importado de flask). Ao criar algo, a resposta certa é 201 Created, não 200.",
                "Crie a rota POST /pedidos (use @app.post) com a função criar_pedido(): leia o corpo JSON com request.get_json() guardando em dados e retorne o dicionário {\"recebido\": dados} junto com o status 201.",
                PyVar("§0\ndef criar_pedido():\n    dados = request.get_json()\n    return {\"recebido\": dados}, 201",
                    A("@app.post(\"/pedidos\")", "@app.route(\"/pedidos\", methods=[\"POST\"])")),
                A("Uma view Flask pode devolver uma tupla (corpo, status).", "@app.post(\"/pedidos\")\ndef criar_pedido():\n    dados = request.________()\n    return {\"recebido\": dados}, ___"),
                A("@app.post(\"/pedidos\")", "Rota que só aceita POST.", "request.get_json()", "Converte o corpo JSON da requisição em dicionário.", "return {...}, 201", "Corpo em JSON + status 201 Created."),
                "Nunca confie no corpo recebido: valide os campos antes de gravar (bibliotecas como pydantic ou marshmallow ajudam)."));

            list.Add(C(P, 2, "code", "Pedidos duplicados",
                "A integração com o app reenviou alguns pedidos e o faturamento saiu inflado. Cada pedido tem um pedido_id único; quando há repetição, a ÚLTIMA versão é a correta.",
                "Em uma linha, sobrescreva df removendo as linhas com pedido_id repetido e mantendo a última ocorrência, com drop_duplicates usando subset e keep.",
                PyVar("df = df.drop_duplicates(§0)",
                    A("subset=[\"pedido_id\"], keep=\"last\"", "keep=\"last\", subset=[\"pedido_id\"]", "subset=\"pedido_id\", keep=\"last\"", "keep=\"last\", subset=\"pedido_id\"")),
                A("subset diz quais colunas definem a repetição; keep diz qual ocorrência fica (\"first\" é o padrão).", "df = df.drop_duplicates(subset=[\"_________\"], keep=\"____\")"),
                A("subset=[\"pedido_id\"]", "Só pedido_id decide se a linha é repetida.", "keep=\"last\"", "Fica a última ocorrência de cada pedido.", "df = ...", "drop_duplicates devolve um DataFrame novo."),
                "Antes de remover, investigue: df[df.duplicated(\"pedido_id\", keep=False)] mostra todas as linhas envolvidas."));

            list.Add(C(P, 2, "code", "Só as linhas de erro",
                "O log do servidor tem 8 GB. A função antiga montava uma lista com todas as linhas de erro e estourava a memória.",
                "Escreva a função geradora linhas_de_erro(caminho): abra o arquivo com with open(caminho, encoding=\"utf-8\") as f:, percorra com for linha in f: e, se \"ERROR\" estiver em linha, entregue a linha com yield.",
                A("def linhas_de_erro(caminho):\n    with open(caminho, encoding=\"utf-8\") as f:\n        for linha in f:\n            if \"ERROR\" in linha:\n                yield linha"),
                A("Uma função com yield vira geradora: entrega um item por vez, quando pedido, sem montar a lista inteira.", "def linhas_de_erro(caminho):\n    with open(caminho, encoding=\"utf-8\") as f:\n        for linha in f:\n            if \"ERROR\" in linha:\n                _____ linha"),
                A("for linha in f:", "Lê o arquivo aos poucos.", "if \"ERROR\" in linha:", "Filtra só as linhas de erro.", "yield linha", "Entrega uma linha e pausa a função até o próximo pedido."),
                "Use assim: for erro in linhas_de_erro(\"app.log\"): ... — a memória fica constante, seja o arquivo de 8 KB ou de 8 GB."));

            list.Add(C(P, 2, "code", "Cache do frete",
                "calcular_frete(cep) consulta uma API lenta e é chamada milhares de vezes com os mesmos CEPs. from functools import lru_cache já está no script.",
                "Escreva o decorator @lru_cache(maxsize=1024) e, logo abaixo, a linha def calcular_frete(cep): (só essas duas linhas).",
                PyVar("@§0(§1)\ndef calcular_frete(cep):", A("lru_cache", "functools.lru_cache"), A("maxsize=1024", "1024")),
                A("lru_cache guarda os resultados mais recentes: a mesma entrada não é calculada duas vezes.", "@lru_cache(maxsize=____)\ndef calcular_frete(cep):"),
                A("@lru_cache(", "Memoriza o resultado de cada argumento.", "maxsize=1024", "Guarda até 1024 CEPs; os menos usados recentemente saem primeiro."),
                "Os argumentos precisam ser hasháveis (str, int, tupla — não lista). calcular_frete.cache_info() mostra acertos e erros; cache_clear() limpa. Cuidado com dados que mudam: o cache não expira sozinho."));

            list.Add(C(P, 2, "code", "Horário em UTC",
                "Os servidores da Micorosoft ficam em vários fusos e os horários dos pedidos não batiam. Além disso, datetime.utcnow() está obsoleto desde o Python 3.12. datetime e timezone já foram importados de datetime.",
                "Guarde em agora a data e hora atuais com fuso horário UTC (um datetime \"aware\"), usando datetime.now com timezone.utc.",
                A("agora = datetime.now(timezone.utc)", "agora = datetime.now(tz=timezone.utc)"),
                A("datetime.now aceita um fuso horário como argumento; o módulo tem um objeto pronto para UTC.", "agora = datetime.now(timezone.___)"),
                A("datetime.now(", "Data e hora atuais.", "timezone.utc", "Fuso UTC: o datetime sabe em que fuso está (aware)."),
                "Guarde e transmita sempre em UTC; converta para o fuso do usuário só na exibição: agora.astimezone(ZoneInfo(\"America/Sao_Paulo\"))."));

            list.Add(C(P, 2, "code", "Ranking com desempate",
                "No ranking do programa de fidelidade, quem tem mais pontos vem primeiro; empates são decididos pelo nome em ordem alfabética. A lista clientes tem dicionários com nome e pontos.",
                "Em uma linha, guarde em ranking a lista clientes ordenada com sorted e key=lambda c: devolvendo uma tupla — primeiro os pontos com sinal negativo (para ficar decrescente), depois o nome.",
                A("ranking = sorted(clientes, key=lambda c: (-c[\"pontos\"], c[\"nome\"]))"),
                A("Tuplas são comparadas item a item: o segundo item só desempata quando o primeiro é igual.", "ranking = sorted(clientes, key=lambda c: (-c[\"______\"], c[\"____\"]))"),
                A("key=lambda c: (...)", "Para cada cliente, devolve a tupla usada na comparação.", "-c[\"pontos\"]", "Negativo: mais pontos vem antes (decrescente).", "c[\"nome\"]", "Desempate em ordem alfabética (crescente)."),
                "O sinal de menos só funciona com números. Para inverter um texto, ordene em duas passadas (sorted é estável): primeiro pelo critério secundário, depois pelo principal."));

            list.Add(C(P, 2, "cmd", "Só os testes do desconto",
                "A suíte tem 800 testes. A Dani está mexendo no desconto e quer rodar só os testes com desconto no nome, parando no primeiro que falhar.",
                "Rode o pytest filtrando os testes pela palavra desconto (opção -k) e parando na primeira falha (opção -x).",
                A("pytest -k desconto -x", "pytest -x -k desconto", "python -m pytest -k desconto -x", "python3 -m pytest -k desconto -x", "py -m pytest -k desconto -x",
                  "pytest --exitfirst -k desconto"),
                A("-k filtra pelo nome do teste; -x (exit first) para na primeira falha.", "pytest -k ________ -x"),
                A("-k desconto", "Roda só os testes cujo nome contém desconto.", "-x", "Para tudo na primeira falha."),
                "-k aceita expressões: -k \"desconto and not frete\". E pytest --lf roda de novo só os testes que falharam na última execução."));

            list.Add(C(P, 2, "code", "Pedido validado no FastAPI",
                "A nova API de pedidos usa FastAPI (objeto app). Pedidos com total em texto (\"abc\") estavam indo direto para o banco. BaseModel já foi importado do pydantic.",
                "Crie o modelo Pedido(BaseModel) com cliente: str e total: float; depois a rota @app.post(\"/pedidos\") com a função criar(pedido: Pedido) que retorna pedido.",
                PyVar("class Pedido(BaseModel):\n    cliente: str\n    total: float\n\n@app.post(\"/pedidos\")\n§0def criar(pedido: Pedido):\n    return pedido", A("", "async ")),
                A("No FastAPI, um parâmetro anotado com um modelo pydantic é lido do corpo JSON e validado automaticamente.", "class Pedido(BaseModel):\n    cliente: str\n    total: _____\n\n@app.post(\"/pedidos\")\ndef criar(pedido: ______):\n    return pedido"),
                A("class Pedido(BaseModel):", "Modelo com validação de tipos.", "@app.post(\"/pedidos\")", "Rota POST.", "pedido: Pedido", "O corpo JSON é convertido e validado; se for inválido, a função nem é chamada."),
                "Dados inválidos recebem resposta 422 automaticamente, com a explicação do erro. A documentação interativa da API fica em /docs."));

            list.Add(C(P, 2, "code", "Consultas em paralelo",
                "O painel busca os dados de 3 lojas com a função async buscar_loja(id), e cada chamada leva 1 segundo. Com três await seguidos, o painel demora 3 segundos.",
                "Dentro de uma função async, guarde em resultados as respostas de buscar_loja(1), buscar_loja(2) e buscar_loja(3) executadas ao mesmo tempo, com await asyncio.gather.",
                A("resultados = await asyncio.gather(buscar_loja(1), buscar_loja(2), buscar_loja(3))",
                  "resultados = await asyncio.gather(*[buscar_loja(i) for i in range(1, 4)])",
                  "resultados = await asyncio.gather(*(buscar_loja(i) for i in range(1, 4)))"),
                A("asyncio.gather recebe várias corrotinas e espera todas juntas — e ele próprio precisa de await.", "resultados = await asyncio.______(buscar_loja(1), buscar_loja(2), buscar_loja(3))"),
                A("asyncio.gather(...)", "Roda as três corrotinas de forma concorrente.", "await", "Espera todas terminarem (cerca de 1 s no total, não 3).", "resultados", "Lista com as respostas, na mesma ordem das chamadas."),
                "Use return_exceptions=True para que uma loja com erro não derrube a coleta das outras."));
        }
    }

    static partial class Docs
    {
        static partial void DocsExtraPython(List<DocEntry> all)
        {
            const string P = "Python";
            const string B = "Básico", M = "Módulos úteis", F = "Arquivos e erros", K = "Funções e classes", W = "Dados e web", E = "Pacotes e ambiente", T = "Testes e boas práticas";

            // ── Básico
            all.Add(X(P, B, "type, int, float, str, round", "Descobre o tipo de um valor e converte entre tipos. round arredonda.",
                "type(valor)\nint(texto)   float(texto)   str(numero)\nround(numero, casas)",
                A("type(x)", "classe do valor (<class 'int'>...)", "int(\"7\")", "texto → inteiro", "float(\"7.5\")", "texto → decimal", "round(x, 2)", "arredonda para 2 casas"),
                A("print(type(nota))", "mostra o tipo da variável nota", "idade = int(idade_texto)", "converte o texto para inteiro", "media = round(media, 1)", "1 casa decimal"),
                "int(\"abc\") gera ValueError. isinstance(x, int) é a forma certa de testar o tipo num if."));
            all.Add(X(P, B, "+=  -=  *=  (atribuição composta)", "Atalhos que fazem a conta e guardam o resultado na mesma variável.",
                "x += valor   (x = x + valor)\nx -= valor   x *= valor   x /= valor", null,
                A("pontos += 10", "soma 10 a pontos", "vidas -= 1", "tira 1 de vidas"), null));
            all.Add(X(P, B, "Métodos de lista", "Métodos que alteram a própria lista ou consultam seus itens.",
                "lista.append(x)  lista.remove(x)  lista.pop(i)\nlista.sort()  lista.count(x)  lista.insert(i, x)",
                A("append(x)", "adiciona no fim", "remove(x)", "remove pelo valor (1ª ocorrência)", "pop(i)", "remove e devolve pela posição", "sort()", "ordena a própria lista (devolve None)", "count(x)", "quantas vezes x aparece", "extend([...])", "adiciona vários itens"),
                A("chamada.append(\"Rui\")", "Rui entra no fim da lista", "chamada.remove(\"Ana\")", "Ana sai da lista", "notas.sort(reverse=True)", "maior nota primeiro", "faltas = presencas.count(\"F\")", "conta os F"),
                "sort() altera a lista e devolve None; sorted(lista) devolve uma lista nova."));
            all.Add(X(P, B, "Tuplas e sets", "Tupla: sequência imutável, com ( ). Set: conjunto sem repetição e sem ordem, com { } ou set().",
                "t = (a, b, c)\ns = set(lista)\na & b   a | b   a - b",
                A("( )", "tupla", "set(lista)", "remove repetidos", "a & b", "interseção (nos dois)", "a | b", "união (em qualquer um)", "a - b", "diferença (em a, mas não em b)"),
                A("dias = (\"seg\", \"ter\", \"qua\")", "tupla fixa", "turmas = set(lista_turmas)", "cada turma uma vez", "nas_duas = alunos_ingles & alunos_robotica", "alunos nos dois cursos"),
                "{} vazio é um dicionário; set vazio é set()."));
            all.Add(X(P, B, "Métodos de texto (str)", "Strings são imutáveis: os métodos devolvem um texto NOVO, que precisa ser guardado.",
                "texto.upper()  texto.lower()  texto.strip()\ntexto.split(sep)  sep.join(lista)",
                A("upper() / lower()", "maiúsculas / minúsculas", "strip()", "tira espaços e \\n das pontas", "split(sep)", "corta em lista", "sep.join(lista)", "une a lista com o separador"),
                A("nome = nome.strip().title()", "limpa e capitaliza", "campos = registro.split(\"/\")", "\"a/b/c\" → [\"a\", \"b\", \"c\"]", "frase = \" - \".join(palavras)", "une com \" - \""),
                "Métodos podem ser encadeados: texto.strip().lower()."));
            all.Add(X(P, B, "if / elif / else", "Executa um bloco só quando a condição é verdadeira. O bloco fica recuado.",
                "if condicao:\n    ...\nelif outra:\n    ...\nelse:\n    ...",
                A(">  <  >=  <=  ==  !=", "comparações", "and  or  not", "combina condições"),
                A("if nota >= 7:\n    print(\"Aprovado\")", "só imprime se a nota for 7 ou mais"),
                "Lista, texto e dicionário vazios valem False numa condição."));
            all.Add(X(P, B, "for, range e while", "for percorre uma sequência; range gera números; while repete enquanto a condição for verdadeira.",
                "for item in sequencia:\nfor i in range(inicio, fim, passo):\nwhile condicao:",
                A("range(5)", "0 a 4", "range(1, 4)", "1, 2, 3 (o fim não entra)", "range(10, 0, -2)", "10, 8, 6, 4, 2", "break / continue", "sai do laço / pula para a próxima volta"),
                A("for andar in range(1, 4):\n    print(andar)", "mostra 1, 2 e 3", "while pendentes:\n    tarefa = pendentes.pop(0)", "esvazia a lista do começo para o fim"),
                "Cuidado com while que nunca fica falso: o programa trava num laço infinito."));
            all.Add(X(P, B, "Operador in", "Testa se um item pertence a uma lista, texto, set ou às chaves de um dicionário. Devolve True ou False.",
                "item in colecao\nitem not in colecao", null,
                A("\"Ana\" in inscritos", "True se Ana estiver na lista", "\"@\" in email", "o texto contém @?", "\"nota\" in aluno", "o dicionário tem a chave nota?"),
                "Em sets e dicionários a busca é instantânea; em listas, percorre item por item."));
            all.Add(X(P, B, "dict: criar e alterar chaves", "Atribuir a uma chave cria a chave (ou substitui o valor, se ela já existir).",
                "d[chave] = valor\nd.update({chave: valor})\ndel d[chave]", null,
                A("aluno[\"turma\"] = \"3A\"", "cria/atualiza a chave turma", "boletim.update({\"historia\": 8})", "atualiza ou acrescenta pares"), null));
            // ── Módulos úteis
            all.Add(X(P, M, "math", "Funções matemáticas (import math).",
                "math.sqrt(x)  math.ceil(x)  math.floor(x)  math.pi\nmath.isclose(a, b)",
                A("sqrt", "raiz quadrada", "ceil / floor", "arredonda para cima / para baixo", "isclose(a, b)", "compara floats com tolerância"),
                A("hipotenusa = math.sqrt(a**2 + b**2)", "Pitágoras", "math.isclose(media, 7.1)", "evita o problema de 0.1 + 0.2 != 0.3"),
                "Nunca compare floats com ==: 0.1 + 0.2 vale 0.30000000000000004."));
            all.Add(X(P, M, "random", "Números e escolhas pseudoaleatórias (import random). Para simulações e jogos — não para segurança.",
                "random.choice(seq)  random.randint(a, b)\nrandom.sample(seq, k)  random.shuffle(lista)",
                A("choice", "um item ao acaso", "randint(a, b)", "inteiro entre a e b (inclusive)", "sample(seq, k)", "k itens sem repetir", "shuffle", "embaralha a própria lista"),
                A("sorteado = random.choice(alunos)", "um aluno ao acaso", "dado = random.randint(1, 6)", "joga um dado"),
                "Para senhas e tokens use o módulo secrets."));
            all.Add(X(P, M, "secrets", "Aleatoriedade criptográfica, própria para senhas, tokens e links de recuperação (import secrets).",
                "secrets.token_urlsafe(nbytes)\nsecrets.token_hex(nbytes)\nsecrets.compare_digest(a, b)",
                A("token_urlsafe(n)", "texto Base64 para URL com n bytes aleatórios", "token_hex(n)", "texto hexadecimal (2n caracteres)", "compare_digest", "compara sem vazar informação pelo tempo"),
                A("codigo = secrets.token_hex(16)", "código de convite da biblioteca", "secrets.choice(caracteres)", "um caractere seguro"), null));
            all.Add(X(P, M, "datetime: date, timedelta, timezone", "Datas e horas (from datetime import date, datetime, timedelta, timezone).",
                "date.today()\ndatetime.now(timezone.utc)\ndata + timedelta(days=n)",
                A("date.today()", "data de hoje", "datetime.now(tz)", "agora, com fuso (aware)", "timedelta(days=, hours=)", "duração para somar/subtrair", "date.fromisoformat(\"2025-01-31\")", "texto ISO → date"),
                A("prova = date(2025, 11, 20)", "data específica", "devolucao = emprestimo + timedelta(days=14)", "prazo de 14 dias", "registro = datetime.now(timezone.utc)", "momento atual em UTC"),
                "datetime.utcnow() está obsoleto desde o Python 3.12: prefira datetime.now(timezone.utc)."));
            all.Add(X(P, M, "collections: Counter e defaultdict", "Dicionários especiais (from collections import Counter, defaultdict).",
                "Counter(sequencia).most_common(n)\nd = defaultdict(list)   d = defaultdict(int)",
                A("Counter", "conta ocorrências", "most_common(n)", "os n mais frequentes", "defaultdict(list)", "chave nova começa com []", "defaultdict(int)", "chave nova começa com 0"),
                A("Counter(respostas).most_common(2)", "as 2 respostas mais comuns", "por_turma = defaultdict(list)\nfor nome, turma in alunos:\n    por_turma[turma].append(nome)", "agrupa sem if"),
                "Counter e defaultdict nunca dão KeyError ao ler uma chave nova."));
            all.Add(X(P, M, "os.getenv  (variáveis de ambiente)", "Lê configurações e segredos do ambiente, em vez de escrevê-los no código (import os).",
                "os.getenv(\"NOME\", padrao)\nos.environ.get(\"NOME\", padrao)\nos.environ[\"NOME\"]",
                A("getenv(n, p)", "valor ou p se não existir", "os.environ[n]", "valor ou KeyError (obrigatório)"),
                A("porta = int(os.getenv(\"PORTA\", \"5000\"))", "porta configurável, 5000 por padrão", "banco = os.environ[\"URL_BANCO\"]", "falha logo se faltar"),
                "Variáveis de ambiente são sempre texto: converta com int() quando precisar."));
            all.Add(X(P, M, "pathlib: Path", "Caminhos de arquivos e pastas como objetos (from pathlib import Path).",
                "Path(\"pasta\") / \"arquivo.txt\"\np.mkdir(parents=True, exist_ok=True)\np.exists()   p.glob(\"*.csv\")",
                A("/", "junta partes do caminho", "mkdir(parents=True)", "cria pastas intermediárias", "exist_ok=True", "sem erro se já existir", "glob(padrao)", "lista arquivos que combinam"),
                A("Path(\"backup/notas\").mkdir(parents=True, exist_ok=True)", "cria backup e backup/notas", "for arq in Path(\"provas\").glob(\"*.pdf\"):", "percorre os PDFs"),
                "Funciona igual no Windows e no Linux (cuida das barras)."));
            all.Add(X(P, M, "decimal: Decimal", "Números decimais exatos, ideais para dinheiro (from decimal import Decimal).",
                "Decimal(\"texto\")\nvalor.quantize(Decimal(\"0.01\"))", null,
                A("mensalidade = Decimal(\"450.75\") * 12", "conta exata", "Decimal(\"0.1\") + Decimal(\"0.2\") == Decimal(\"0.3\")", "True (com float daria False)"),
                "Crie sempre a partir de TEXTO: Decimal(0.1) herda o erro do float."));
            all.Add(X(P, M, "logging", "Registro de mensagens com nível, horário e origem — o substituto profissional do print.",
                "logger = logging.getLogger(__name__)\nlogger.info(\"texto %s\", valor)",
                A("debug / info / warning / error / critical", "níveis, do menos ao mais grave", "logger.exception(...)", "dentro do except: inclui o traceback", "%s + argumento", "formatação só quando a mensagem é registrada"),
                A("log = logging.getLogger(__name__)", "logger do módulo", "log.error(\"Falha ao importar %s\", arquivo)", "erro com o nome do arquivo"),
                "Configure uma vez no início do programa: logging.basicConfig(level=logging.INFO)."));
            all.Add(X(P, M, "functools.lru_cache", "Decorator que memoriza resultados: a mesma entrada não é recalculada.",
                "@lru_cache(maxsize=N)\ndef funcao(arg): ...",
                A("maxsize", "quantos resultados guardar (None = sem limite)", "cache_info()", "acertos e erros", "cache_clear()", "esvazia o cache"),
                A("@lru_cache(maxsize=256)\ndef media_da_turma(turma): ...", "consulta pesada memorizada"),
                "Os argumentos precisam ser hasháveis (nada de listas)."));
            all.Add(X(P, M, "asyncio: async, await e gather", "Executa tarefas de espera (rede, disco) de forma concorrente.",
                "async def f(): ...\nresultado = await f()\nresultados = await asyncio.gather(f(1), f(2))",
                A("async def", "declara uma corrotina", "await", "espera o resultado sem travar as outras tarefas", "gather", "roda várias corrotinas juntas", "asyncio.run(main())", "ponto de entrada do programa"),
                A("notas = await asyncio.gather(baixar_nota(\"ana\"), baixar_nota(\"rui\"))", "duas consultas ao mesmo tempo"),
                "await só pode ser usado dentro de async def."));
            // ── Arquivos e erros
            all.Add(X(P, F, "Ler linha a linha e gravar texto", "for linha in f lê o arquivo aos poucos; o modo \"w\" grava e \"a\" acrescenta.",
                "with open(\"arq\", encoding=\"utf-8\") as f:\n    for linha in f:\n\nwith open(\"arq\", \"w\", encoding=\"utf-8\") as f:\n    f.write(texto)",
                A("\"w\"", "escreve (apaga o conteúdo anterior)", "\"a\"", "acrescenta no fim", "encoding=\"utf-8\"", "acentos corretos em qualquer sistema", "f.write(s)", "grava um texto (sem \\n automático)"),
                A("with open(\"chamada.txt\", encoding=\"utf-8\") as arq:\n    for nome in arq:\n        print(nome.strip())", "lê nome por nome",
                  "with open(\"ata.txt\", \"a\", encoding=\"utf-8\") as arq:\n    arq.write(f\"Presentes: {qtd}\\n\")", "acrescenta uma linha"),
                "Cada linha lida termina com \\n: use strip() ou rstrip(\"\\n\")."));
            all.Add(X(P, F, "csv: DictReader e DictWriter", "Lê e grava CSV usando o cabeçalho como chaves (import csv).",
                "with open(\"arq.csv\", newline=\"\", encoding=\"utf-8\") as f:\n    for linha in csv.DictReader(f):",
                A("DictReader(f)", "cada linha vira um dicionário", "DictWriter(f, fieldnames=[...])", "grava dicionários", "newline=\"\"", "recomendado ao abrir arquivos CSV"),
                A("for linha in csv.DictReader(arq):\n    print(linha[\"turma\"])", "lê a coluna turma", "w = csv.DictWriter(arq, fieldnames=[\"nome\", \"nota\"])\nw.writeheader()", "começa um CSV novo"),
                "Os valores lidos sempre chegam como texto."));
            all.Add(X(P, F, "json.dump: indent e ensure_ascii", "Opções para gravar JSON legível e com acentos.",
                "json.dump(dados, arquivo, ensure_ascii=False, indent=2)",
                A("indent=2", "recuo de 2 espaços por nível", "ensure_ascii=False", "grava acentos como estão", "default=str", "converte tipos não suportados (datas)"),
                A("with open(\"turma.json\", \"w\", encoding=\"utf-8\") as arq:\n    json.dump(turma, arq, indent=4)", "grava formatado"),
                "Sem ensure_ascii=False, \"João\" vira \"Jo\\u00e3o\"."));
            all.Add(X(P, F, "raise, exceções próprias e from", "Lança erros de propósito, cria tipos de erro do domínio e encadeia a causa.",
                "class MeuErro(Exception):\n    pass\n\nraise MeuErro(\"mensagem\")\nraise MeuErro(...) from erro_original",
                A("raise", "lança a exceção", "from e", "guarda o erro original como causa", "from None", "esconde a causa (use com moderação)"),
                A("class MatriculaInvalida(Exception):\n    pass", "exceção do domínio", "except ValueError as e:\n    raise MatriculaInvalida(numero) from e", "traduz o erro mantendo a causa"),
                "Valide cedo: raise ValueError(\"...\") no começo da função evita dados ruins mais adiante."));
            all.Add(X(P, F, "sqlite3: consultas com parâmetros", "Banco SQLite embutido no Python (import sqlite3). Valores sempre por parâmetro, nunca por f-string.",
                "con = sqlite3.connect(\"arquivo.db\")\ncur = con.cursor()\ncur.execute(\"... WHERE coluna = ?\", (valor,))",
                A("?", "placeholder do valor", "(valor,)", "tupla de valores, na ordem dos ?", "fetchone() / fetchall()", "lê o resultado", "con.commit()", "confirma INSERT/UPDATE/DELETE"),
                A("cur.execute(\"SELECT nome FROM alunos WHERE turma = ?\", (turma,))", "consulta segura", "cur.execute(\"INSERT INTO notas VALUES (?, ?)\", (aluno_id, nota))", "dois parâmetros"),
                "Montar SQL com f-string ou + abre espaço para SQL injection."));
            // ── Funções e classes
            all.Add(X(P, K, "*args e **kwargs", "Parâmetros que aceitam qualquer quantidade de argumentos: *args vira tupla; **kwargs vira dicionário.",
                "def f(*args):\ndef f(**kwargs):\nf(*lista)   f(**dicionario)",
                A("*nome", "argumentos posicionais extras (tupla)", "**nome", "argumentos nomeados extras (dicionário)"),
                A("def maior(*notas):\n    return max(notas)", "maior(7, 9, 8) → 9", "def cadastro(**campos):\n    for k, v in campos.items():\n        print(k, v)", "cadastro(nome=\"Rui\", turma=\"2B\")"),
                "Na chamada, * e ** fazem o contrário: desempacotam lista e dicionário."));
            all.Add(X(P, K, "Type hints", "Anotações de tipo para parâmetros e retorno. Ajudam o editor e ferramentas como o mypy.",
                "def f(param: tipo, outro: tipo = padrao) -> tipo_retorno:",
                A("x: int", "parâmetro inteiro", "-> str", "devolve texto", "list[str]", "lista de textos (Python 3.9+)", "int | None", "inteiro ou None (Python 3.10+)"),
                A("def media(notas: list[float]) -> float:\n    return sum(notas) / len(notas)", "função anotada"),
                "O Python não impede tipos errados ao executar: as anotações são verificadas por ferramentas."));
            all.Add(X(P, K, "dataclass e field", "Classe para guardar dados com __init__, __repr__ e __eq__ gerados automaticamente (from dataclasses import dataclass, field).",
                "@dataclass\nclass Nome:\n    campo: tipo\n    outro: tipo = padrao\n    lista: list[str] = field(default_factory=list)",
                A("@dataclass", "gera os métodos", "field(default_factory=list)", "lista nova para cada objeto", "frozen=True", "objeto imutável"),
                A("@dataclass\nclass Livro:\n    titulo: str\n    paginas: int", "Livro(\"Iracema\", 160)", "    emprestimos: list[str] = field(default_factory=list)", "padrão mutável do jeito certo"),
                "Campos com valor padrão precisam vir depois dos campos sem padrão."));
            all.Add(X(P, K, "@property e setter", "Transforma métodos em \"atributos\" com validação na leitura e na escrita.",
                "@property\ndef x(self):\n    return self._x\n\n@x.setter\ndef x(self, valor):\n    ...\n    self._x = valor", null,
                A("@nota.setter\ndef nota(self, valor):\n    if not 0 <= valor <= 10:\n        raise ValueError(\"nota fora da faixa\")\n    self._nota = valor", "aluno.nota = 11 lança erro"),
                "Guarde o valor em outro nome (self._x); usar self.x dentro do setter causa recursão infinita."));
            all.Add(X(P, K, "Geradores (yield)", "Função com yield entrega um valor por vez, sob demanda, sem montar a lista inteira na memória.",
                "def gerador(...):\n    for item in fonte:\n        yield item", null,
                A("def pares(limite):\n    for n in range(limite):\n        if n % 2 == 0:\n            yield n", "for p in pares(10): ...", "total = sum(len(l) for l in arq)", "expressão geradora entre parênteses"),
                "Um gerador só pode ser percorrido uma vez."));
            all.Add(X(P, K, "max, min e ordenação por tupla", "max/min aceitam key= como o sorted. Uma key que devolve tupla ordena por vários critérios.",
                "max(seq, key=lambda x: ...)\nsorted(seq, key=lambda x: (criterio1, criterio2))",
                A("key=", "o que comparar em cada item", "-numero", "inverte a ordem de um critério numérico", "default=", "valor se a sequência estiver vazia"),
                A("melhor = max(alunos, key=lambda a: a[\"media\"])", "aluno com a maior média", "sorted(alunos, key=lambda a: (a[\"turma\"], -a[\"media\"]))", "por turma e, dentro dela, maior média primeiro"),
                "Tuplas são comparadas item a item: o segundo só desempata."));
            // ── Dados e web
            all.Add(X(P, W, "pandas: colunas e ordenação", "Cria colunas com operações vetorizadas e ordena linhas.",
                "df[\"nova\"] = df[\"a\"] * df[\"b\"]\ndf.sort_values(\"coluna\", ascending=False).head(n)",
                A("df[\"col\"] = ...", "cria ou substitui coluna", "sort_values(col)", "ordena (crescente por padrão)", "ascending=False", "decrescente", "head(n) / nlargest(n, col)", "primeiras n linhas / n maiores"),
                A("tabela[\"media\"] = (tabela[\"p1\"] + tabela[\"p2\"]) / 2", "coluna calculada", "tabela.sort_values(\"media\", ascending=False).head(3)", "3 melhores médias"),
                "df.coluna = ... não cria coluna nova: use sempre df[\"coluna\"] = ..."));
            all.Add(X(P, W, "pandas: read_csv em pedaços", "Lê arquivos enormes por partes, mantendo a memória sob controle.",
                "for parte in pd.read_csv(\"arq.csv\", chunksize=N):\n    ...",
                A("chunksize=N", "N linhas por parte", "usecols=[...]", "lê só as colunas indicadas"),
                A("presencas = 0\nfor bloco in pd.read_csv(\"chamadas.csv\", chunksize=50000):\n    presencas += (bloco[\"status\"] == \"P\").sum()", "conta presenças em blocos"), null));
            all.Add(X(P, W, "pandas: datas e duplicados", "Converte textos em datas para usar .dt e remove linhas repetidas.",
                "df[\"data\"] = pd.to_datetime(df[\"data\"])\ndf[\"data\"].dt.month\ndf.drop_duplicates(subset=[...], keep=\"first\")",
                A("to_datetime", "texto → data", ".dt.year / .dt.month / .dt.day", "partes da data", "subset=", "colunas que definem a repetição", "keep=\"first\" | \"last\" | False", "qual ocorrência manter"),
                A("tabela.groupby(tabela[\"prova\"].dt.year)[\"nota\"].mean()", "média por ano", "tabela.drop_duplicates(subset=[\"matricula\"])", "um registro por matrícula"),
                "read_csv(..., parse_dates=[\"data\"]) já converte na leitura."));
            all.Add(X(P, W, "Flask: JSON e status", "Uma rota pode devolver um dicionário (vira JSON) e um código de status. request.get_json() lê o corpo da requisição.",
                "@app.post(\"/caminho\")\ndef f():\n    dados = request.get_json()\n    return {...}, 201",
                A("return {...}", "resposta JSON com 200", "return {...}, 201", "JSON com status 201 Created", "request.get_json()", "corpo JSON → dicionário", "@app.post / @app.get", "atalhos para methods=[...]"),
                A("@app.get(\"/saude\")\ndef saude():\n    return {\"status\": \"ok\"}", "health check", "@app.post(\"/alunos\")\ndef novo_aluno():\n    aluno = request.get_json()\n    return aluno, 201", "cria e devolve 201"),
                "jsonify(...) também gera respostas JSON."));
            all.Add(X(P, W, "FastAPI e pydantic", "Framework de APIs que valida dados pelos type hints (pip install fastapi uvicorn).",
                "class Modelo(BaseModel):\n    campo: tipo\n\n@app.post(\"/caminho\")\ndef f(item: Modelo):\n    return item",
                A("BaseModel", "modelo com validação", "@app.get / @app.post", "rotas", "/docs", "documentação interativa automática", "422", "resposta automática para dados inválidos"),
                A("class Aluno(BaseModel):\n    nome: str\n    idade: int", "modelo", "@app.get(\"/alunos/{id}\")\ndef ver(id: int):\n    return {\"id\": id}", "parâmetro de caminho convertido para int"), null));
            // ── Pacotes e ambiente
            all.Add(X(P, E, "Executar um script", "Roda um arquivo .py com o interpretador.",
                "python arquivo.py\npython -m modulo", A("py", "lançador do Windows", "-m", "executa um módulo como programa"),
                A("python notas.py", "roda o script notas.py", "python -m http.server 9000", "servidor de arquivos na porta 9000"), null));
            all.Add(X(P, E, "Ativar o ambiente virtual", "Faz o terminal usar o Python e o pip do venv.",
                "PASTA\\Scripts\\activate          (Windows, Prompt)\n.\\PASTA\\Scripts\\Activate.ps1   (PowerShell)\nsource PASTA/bin/activate      (Linux/macOS)",
                A("deactivate", "sai do ambiente"),
                A(".env\\Scripts\\activate", "ativa o ambiente da pasta .env no Windows", "source .env/bin/activate", "o mesmo no Linux"),
                "Com o ambiente ativo, o nome dele aparece entre parênteses no prompt."));
            all.Add(X(P, E, "pip: requirements e freeze", "Registra e reinstala as dependências exatas do projeto.",
                "pip freeze > requirements.txt\npip install -r requirements.txt",
                A("freeze", "lista pacote==versão", "-r arquivo", "instala tudo o que está no arquivo", "list / show", "lista pacotes / detalhes de um pacote"),
                A("pip freeze > deps.txt", "salva as versões em deps.txt", "pip install -r deps.txt", "reinstala tudo de deps.txt"),
                "Sempre com o venv ativo."));
            all.Add(X(P, E, "pip: versões de pacotes", "Especificadores de versão na instalação.",
                "pip install pacote==X.Y.Z\npip install \"pacote>=X.Y\"",
                A("==", "versão exata", ">=", "versão mínima", "~=", "compatível (mesma série)", "--upgrade", "atualiza para a mais nova"),
                A("pip install flask==3.0.3", "versão exata do Flask", "pip install --upgrade pip", "atualiza o próprio pip"), null));
            all.Add(X(P, E, "uvicorn", "Servidor ASGI que roda APIs FastAPI.",
                "uvicorn arquivo:objeto [--reload] [--port N] [--host IP]",
                A("arquivo:objeto", "módulo (sem .py) e variável da app", "--reload", "reinicia ao salvar (só desenvolvimento)", "--port", "porta (padrão 8000)", "--host 0.0.0.0", "aceita conexões externas"),
                A("uvicorn api:aplicacao --port 9000", "arquivo api.py, objeto aplicacao, porta 9000"), null));
            all.Add(X(P, E, "python -m cProfile", "Profiler embutido: mostra quanto tempo cada função gastou.",
                "python -m cProfile -s ORDEM script.py",
                A("-s cumtime", "ordena pelo tempo acumulado", "-s tottime", "tempo gasto só dentro da própria função", "-o arquivo", "salva o resultado para análise"),
                A("python -m cProfile -s tottime boletim.py", "perfil do boletim.py"),
                "Otimize o que a medição apontar, não o que você imagina que está lento."));
            // ── Testes e boas práticas
            all.Add(X(P, T, "pytest: testes com assert", "Funções test_* em arquivos test_*.py; assert confere o resultado.",
                "def test_algo():\n    assert funcao(entrada) == esperado",
                A("test_", "prefixo de arquivos e funções de teste", "assert", "falha o teste se a condição for falsa", "pytest.approx", "compara floats com tolerância"),
                A("def test_media():\n    assert media([6, 8]) == 7", "teste simples"),
                "Um teste deve conferir um comportamento só, com nome que explique qual."));
            all.Add(X(P, T, "pytest: parametrize e raises", "parametrize roda o mesmo teste com vários casos; raises confere que um erro é lançado.",
                "@pytest.mark.parametrize(\"a, b\", [(1, 2), (3, 4)])\ndef test_x(a, b): ...\n\nwith pytest.raises(TipoErro):\n    ...",
                A("parametrize", "tabela de casos", "ids=[...]", "nomes dos casos", "raises(Erro)", "espera o erro no bloco", "match=", "confere a mensagem do erro"),
                A("@pytest.mark.parametrize(\"nota, situacao\", [(8, \"aprovado\"), (4, \"reprovado\")])\ndef test_situacao(nota, situacao):\n    assert avaliar(nota) == situacao", "dois casos",
                  "with pytest.raises(ZeroDivisionError):\n    media([])", "espera divisão por zero"), null));
            all.Add(X(P, T, "pytest na linha de comando", "Opções para escolher quais testes rodar e como.",
                "pytest [caminho] [-k EXPRESSAO] [-x] [-v]",
                A("arquivo.py", "só aquele arquivo", "arquivo.py::test_nome", "um teste só", "-k texto", "testes cujo nome contém o texto", "-x", "para na primeira falha", "-v", "mostra cada teste", "--lf", "só os que falharam da última vez"),
                A("pytest tests/test_boletim.py -v", "um arquivo, detalhado", "pytest -k media", "só testes com media no nome"), null));
        }
    }
}
