using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerQuest
{
    // Pergunta de múltipla escolha usada nas atividades das salas sem desafios.
    class Quiz
    {
        public string Prompt, Explain;
        public string[] Options;   // a primeira é sempre a correta (as opções são embaralhadas na hora)
        public int[] Effect;       // dilemas: efeito na reputação de cada opção (null = quiz com uma resposta certa)
        public int Level;          // 0 fácil, 1 médio, 2 difícil (liberados aos poucos; valem mais)
        public Quiz(string prompt, string explain, params string[] options) { Prompt = prompt; Explain = explain; Options = options; }
    }

    static class ActivityBank
    {
        // ── Auditório: Tech Talk da Dra. Ada
        public static readonly Quiz[] Talk =
        {
            new Quiz("Pergunta para a plateia: o que significa o erro HTTP 404?", "404 é 'não encontrado'. 403 é 'proibido' e 500 é erro interno do servidor.",
                "Página ou recurso não encontrado", "Erro interno do servidor", "Acesso proibido"),
            new Quiz("Qual destas é uma linguagem de MARCAÇÃO, e não de programação?", "HTML descreve a estrutura da página; quem dá lógica é JavaScript, Python, PHP...",
                "HTML", "Python", "PHP"),
            new Quiz("No Git, qual comando envia seus commits para o repositório remoto?", "git push envia; git pull traz as mudanças do remoto; git clone copia o repositório inteiro.",
                "git push", "git pull", "git clone"),
            new Quiz("O que é fazer um 'deploy'?", "Deploy é publicar uma versão do sistema para os usuários usarem.",
                "Colocar o sistema no ar para os usuários", "Apagar o banco de dados", "Escrever a documentação"),
            new Quiz("Para que serve um container Docker?", "O container leva o app junto com tudo de que ele precisa, então roda igual em qualquer máquina.",
                "Empacotar o app com as dependências para rodar igual em qualquer lugar", "Deixar a internet mais rápida", "Substituir o sistema operacional do PC"),
            new Quiz("O que é autenticação em dois fatores (2FA)?", "2FA = algo que você sabe (senha) + algo que você tem (celular, chave física).",
                "Senha + uma segunda prova, como um código no celular", "Usar duas senhas iguais", "Entrar em dois computadores ao mesmo tempo"),
            new Quiz("Qual destas ferramentas NÃO é um banco de dados?", "Figma é ferramenta de design. PostgreSQL e MySQL são bancos relacionais.",
                "Figma", "PostgreSQL", "MySQL"),
            new Quiz("O que é uma API?", "API é um 'contrato' para um sistema pedir dados ou ações a outro (ex.: o app do Café 365 falando com o servidor).",
                "Uma interface para sistemas conversarem entre si", "Um tipo de vírus", "Um editor de imagens"),
            new Quiz("A LGPD é a lei brasileira sobre...", "Lei Geral de Proteção de Dados: define como empresas podem coletar e usar dados pessoais.",
                "Proteção de dados pessoais", "Impostos de software", "Velocidade mínima da internet"),
            new Quiz("Qual a diferença entre frontend e backend?", "Frontend roda no navegador/app do usuário; backend roda no servidor (regras, banco, APIs).",
                "Frontend é o que o usuário vê; backend roda no servidor", "São a mesma coisa", "Backend é só a parte de design"),
            new Quiz("O que significa 'open source'?", "Código aberto: qualquer pessoa pode ler, estudar e contribuir, seguindo a licença.",
                "O código é aberto para qualquer um ver e contribuir", "Todo software grátis, sem licença", "Software que não tem bugs"),
            new Quiz("Para que serve um teste automatizado?", "Testes conferem sozinhos se o código continua funcionando a cada mudança.",
                "Verificar automaticamente se o código faz o que deveria", "Deixar o programa mais bonito", "Substituir o programador"),
            // ── médio
            M("Qual código HTTP significa que o usuário NÃO está autenticado (precisa fazer login)?", "401 = não autenticado. 403 = autenticado, mas sem permissão. 400 = requisição malformada. 404 = não encontrado.",
                "401", "403", "400", "404"),
            M("No Git, qual a diferença entre 'git fetch' e 'git pull'?", "fetch só baixa as novidades do remoto; pull = fetch + merge na sua branch.",
                "fetch só baixa; pull baixa e já mescla na sua branch", "São exatamente iguais", "fetch envia commits; pull recebe", "pull só funciona em repositório privado"),
            M("O que é latência numa rede?", "Latência é o tempo de ida e volta. A quantidade de dados por segundo é a largura de banda.",
                "O tempo que um pacote leva para ir e voltar", "A quantidade de dados por segundo", "O número de computadores na rede", "O tamanho máximo de um arquivo"),
            M("Por que senhas são guardadas com hash (bcrypt, argon2) e não com criptografia reversível?", "Hash não tem volta: se o banco vazar, as senhas não podem ser simplesmente 'descriptografadas'.",
                "Porque o hash não pode ser revertido para a senha original", "Porque o hash ocupa menos espaço", "Porque criptografia é proibida por lei", "Porque o hash deixa o login mais rápido"),
            M("O que é um teste de unidade?", "Testa uma parte pequena (uma função, uma classe) isolada do resto. Teste de ponta a ponta é o que usa o sistema inteiro.",
                "Um teste de uma parte pequena do código, isolada", "Um teste do sistema inteiro com usuários reais", "Uma medição da velocidade do servidor", "Uma revisão de erros de digitação nos textos"),
            M("O que faz um balanceador de carga (load balancer)?", "Ele distribui as requisições entre vários servidores e tira do rodízio os que estão fora do ar.",
                "Distribui as requisições entre vários servidores", "Comprime as imagens do site", "Guarda o backup do banco de dados", "Bloqueia vírus nos e-mails"),
            M("JSON é...", "JSON é um formato de texto para trocar dados (objetos, listas, números, textos) entre sistemas — não é linguagem nem banco.",
                "Um formato de texto para trocar dados entre sistemas", "Uma linguagem de programação", "Um banco de dados", "Um protocolo de rede como o HTTP"),
            // ── difícil
            H("Buscar um item numa lista ORDENADA com busca binária tem qual complexidade?", "A busca binária descarta metade da lista a cada passo: O(log n). Percorrer a lista inteira seria O(n).",
                "O(log n)", "O(n)", "O(n²)", "O(1)"),
            H("Qual destas requisições é idempotente (repetir várias vezes tem o mesmo efeito que fazer uma vez)?", "PUT substitui o recurso pelo mesmo conteúdo, então repetir não muda nada. Cada POST normalmente cria algo novo.",
                "PUT /produtos/7 com os dados completos do produto", "POST /pedidos criando um pedido", "POST /estoque/7/somar com +1 unidade", "Nenhuma: toda requisição muda alguma coisa"),
            H("O navegador bloqueou (CORS) o fetch do site cafe365.com para api.cafe365.com. Quem precisa liberar?", "CORS é decidido pelo SERVIDOR da API, que precisa responder com o cabeçalho Access-Control-Allow-Origin.",
                "O servidor da API, com o cabeçalho Access-Control-Allow-Origin", "O usuário, desativando o antivírus", "O site, trocando fetch por XMLHttpRequest", "O DNS, apontando os dois domínios para o mesmo IP"),
            H("Qual é a troca que um índice faz num banco de dados?", "Índices aceleram buscas, mas cada INSERT/UPDATE precisa atualizá-los, e eles ocupam espaço.",
                "Consultas mais rápidas em troca de escritas mais lentas e mais espaço", "Mais segurança em troca de menos velocidade", "Menos espaço em disco em troca de consultas lentas", "Nenhuma: índice só tem vantagem"),
            H("Um ataque XSS acontece quando...", "XSS: o site exibe texto vindo do usuário sem escapar, e o navegador de outra pessoa executa o script injetado.",
                "O site mostra texto do usuário sem escapar e o navegador executa um script", "Alguém descobre a senha do banco de dados", "O servidor recebe requisições demais ao mesmo tempo", "O usuário esquece de fazer logout"),
            H("O que 'git rebase main' faz de diferente de 'git merge main'?", "rebase reaplica seus commits em cima da main (histórico linear, com novos hashes). merge cria um commit de junção.",
                "Reaplica seus commits sobre a main, deixando o histórico linear", "Apaga a branch remota", "Desfaz o último commit", "Cria uma cópia do repositório"),
            H("Escalar uma aplicação HORIZONTALMENTE significa...", "Horizontal = mais máquinas iguais atrás de um balanceador. Vertical = uma máquina mais potente.",
                "Adicionar mais servidores iguais", "Trocar o servidor por um mais potente", "Diminuir o número de usuários", "Mover o banco para um disco SSD"),
        };

        static Quiz M(string prompt, string explain, params string[] options) { return new Quiz(prompt, explain, options) { Level = 1 }; }
        static Quiz H(string prompt, string explain, params string[] options) { return new Quiz(prompt, explain, options) { Level = 2 }; }

        // ── SOC: detector de phishing (primeira opção = resposta certa)
        public static readonly Quiz[] Phishing =
        {
            new Quiz("E-mail de suporte@micor0soft-seguranca.com: \"Sua conta será BLOQUEADA em 1 hora! Clique aqui e confirme sua senha.\"",
                "Golpe! Domínio falso (zero no lugar do 'o'), urgência exagerada e pedido de senha. Nenhuma equipe séria pede senha por e-mail.",
                "É phishing", "Parece legítimo"),
            new Quiz("E-mail de rh@micorosoft.com: \"Lembrete: a palestra de sexta no Auditório começa às 15h. Não precisa responder.\"",
                "Legítimo: domínio correto, informação esperada e nenhum pedido de dados ou clique.",
                "Parece legítimo", "É phishing"),
            new Quiz("E-mail de ceo.valeria@gmail.com: \"Preciso que você compre 5 gift cards URGENTE e me mande os códigos. Não conte a ninguém.\"",
                "Golpe do falso CEO: e-mail pessoal, urgência, segredo e dinheiro. Confirme por outro canal antes de qualquer coisa.",
                "É phishing", "Parece legítimo"),
            new Quiz("E-mail de entregas@correios-rastreio.net: \"Sua encomenda está retida. Pague R$ 4,90 de taxa neste link.\"",
                "Golpe clássico: domínio que imita os Correios e cobrança pequena para roubar dados do cartão.",
                "É phishing", "Parece legítimo"),
            new Quiz("E-mail de ti@micorosoft.com: \"Manutenção programada no sábado, das 8h às 10h. Os sistemas podem ficar lentos.\"",
                "Legítimo: só informa, não pede senha nem clique, e vem do domínio oficial.",
                "Parece legítimo", "É phishing"),
            new Quiz("E-mail de seguranca@micorosoft.com.verifica-login.ru: \"Detectamos login suspeito. Entre aqui para verificar.\"",
                "Golpe! O domínio real é verifica-login.ru; o 'micorosoft.com' no começo é só disfarce. Leia o endereço da direita para a esquerda.",
                "É phishing", "Parece legítimo"),
            new Quiz("E-mail de premios@sorteio-micorosoft.com: \"Parabéns! Você ganhou um celular. Informe CPF e cartão para o frete.\"",
                "Golpe: prêmio que você não pediu + pedido de CPF e cartão. Desconfie de tudo que é bom demais.",
                "É phishing", "Parece legítimo"),
            new Quiz("Mensagem no chat interno, do Paulo (TI), que está ao seu lado: \"Vou instalar a atualização do antivírus no seu PC agora, tudo bem?\"",
                "Legítimo: você confirma pessoalmente quem é, e a TI não pede sua senha para isso.",
                "Parece legítimo", "É phishing"),
            // ── médio
            M("E-mail \"Sua senha expira hoje\". O link diz portal.micorosoft.com, mas ao passar o mouse aparece http://micorosoft.portal-login.co/reset",
                "Golpe! O texto do link pode ser qualquer coisa; o destino real (ao passar o mouse) é portal-login.co. Sempre confira para onde o link vai.",
                "É phishing", "Parece legítimo"),
            M("E-mail do financeiro de um fornecedor com quem a empresa já trabalha: \"Mudamos de banco. A partir de hoje, pague os boletos nesta nova conta.\"",
                "Golpe da troca de conta (fraude de fornecedor): mesmo vindo de um contato conhecido, confirme por telefone, pelo número que você já tinha.",
                "É phishing: confirme por outro canal antes de pagar", "Parece legítimo: o fornecedor é conhecido"),
            M("SMS: \"Micorosoft: seu código de verificação é 482913. Não compartilhe.\" Você NÃO pediu código nenhum agora.",
                "O SMS em si é real, mas alguém tem a sua senha e está tentando entrar. Troque a senha já e avise o SOC. Nunca repasse o código.",
                "Alguém está tentando entrar na sua conta: troque a senha e avise o SOC", "É phishing: responda o SMS pedindo para parar", "Foi engano, pode ignorar", "Mande o código para a Kátia conferir"),
            M("E-mail de noreply@github.com: \"Novo login na sua conta: Windows, São Paulo\". Foi você mesmo que entrou há 2 minutos, e o e-mail não pede nada.",
                "Legítimo: aviso de segurança esperado, do domínio oficial, sem pedir senha nem clique.",
                "Parece legítimo", "É phishing"),
            // ── difícil
            H("E-mail de suporte@micorosoft.com (domínio CERTO!) com o anexo \"Fatura_outubro.pdf.exe\" e o texto genérico \"Segue a fatura\".",
                "Golpe: a extensão dupla esconde um executável, e o remetente pode ter sido falsificado ou a conta invadida. Domínio certo não garante nada.",
                "Phishing: a extensão dupla esconde um programa", "Legítimo: o domínio está correto", "Legítimo: PDF é sempre seguro", "Só seria golpe se tivesse erro de português"),
            H("Ligação: \"Aqui é do suporte da Micorosoft, detectamos vírus no seu PC. Instale o AnyDesk e me passe o código para eu resolver.\"",
                "Golpe por telefone (vishing): suporte de verdade não liga do nada pedindo acesso remoto. Desligue e procure o suporte pelo canal oficial.",
                "Golpe: desligue e procure o suporte pelo canal oficial", "Legítimo: o suporte sempre liga", "Instale, mas não passe a senha", "Passe o código se ele souber seu nome"),
            H("QR code colado na mesa do refeitório: \"Cardápio novo! Escaneie e entre com sua conta Micorosoft.\"",
                "QR code também pode levar a uma página falsa de login (quishing). Cardápio não precisa da sua senha corporativa.",
                "Golpe provável: o QR pode levar a um login falso", "Legítimo: QR code é sempre do restaurante", "Só é golpe se pedir o cartão", "Legítimo, se o papel for bonito"),
            H("E-mail de rh@micorosoft.com: \"Atualize seus dados bancários para o pagamento de amanhã neste formulário\" — o link é de um formulário público na internet.",
                "Golpe: o remetente pode ser falsificado, há urgência e o pedido de dados bancários num formulário externo. Confirme com o RH pessoalmente.",
                "É phishing", "Parece legítimo"),
            H("Mensagem no chat interno, da conta real do seu líder, no estilo de sempre: \"Me manda o relatório de vendas do trimestre, por favor.\"",
                "Legítimo: pedido normal de trabalho, no canal interno, sem urgência, segredo, dinheiro ou dados pessoais. Nem tudo é golpe!",
                "Parece legítimo", "É phishing"),
        };

        // ── Help Desk: atender chamados
        public static readonly Quiz[] Tickets =
        {
            new Quiz("Chamado #4601: \"O computador não liga.\" O que você verifica primeiro?", "Comece pelo simples: energia, cabo e tomada. Metade dos chamados se resolve assim.",
                "Se o cabo de energia está conectado e a tomada funciona", "Formatar o disco", "Trocar a placa-mãe"),
            new Quiz("Chamado #4602: \"A internet sumiu só no meu PC; os colegas estão navegando.\"", "Se só um PC está sem rede, o problema é local: cabo, Wi-Fi ou configuração (ipconfig ajuda).",
                "Verificar o cabo/Wi-Fi desse PC e rodar ipconfig", "Reiniciar o roteador da empresa inteira", "Ligar para a operadora"),
            new Quiz("Chamado #4603: \"Esqueci minha senha.\"", "Confirme a identidade e use o processo oficial de reset. Nunca envie nem peça senhas.",
                "Confirmar a identidade e resetar pelo sistema oficial", "Mandar a senha antiga por e-mail", "Pedir para usar a senha de um colega"),
            new Quiz("Chamado #4604: \"Meu PC está muito lento.\"", "O Gerenciador de Tarefas mostra quem está consumindo CPU, memória ou disco.",
                "Ver no Gerenciador de Tarefas o que está consumindo recursos", "Comprar outro computador", "Apagar a pasta Windows"),
            new Quiz("Chamado #4605: \"Derramei café no teclado!\"", "Desligue/desconecte logo, vire para escorrer e deixe secar. Se não voltar, troque.",
                "Desconectar e virar para escorrer e secar", "Continuar digitando para secar", "Secar com secador ligado na tomada perto da água"),
            new Quiz("Chamado #4606: \"Recebi um anexo .exe estranho de um desconhecido.\"", "Não abra e reporte ao SOC. Executáveis inesperados são o jeito mais comum de espalhar malware.",
                "Não abrir e reportar ao SOC", "Abrir para ver o que é", "Encaminhar para o time todo"),
            new Quiz("Chamado #4607: \"Pelo IP o sistema interno abre, mas pelo nome não.\"", "Se o IP funciona e o nome não, quem falhou foi a tradução de nomes: DNS.",
                "Problema de DNS", "Problema no monitor", "Falta de memória RAM"),
            new Quiz("Chamado #4608: \"A impressora aparece como offline.\"", "Cheque energia, rede e a fila de impressão travada antes de pensar em trocar algo.",
                "Checar se está ligada, na rede e se a fila de impressão travou", "Comprar outra impressora", "Reinstalar o Windows"),
            // ── médio
            M("Chamado #4611: \"Nada da rede funciona.\" O ipconfig mostra o IP 169.254.33.7.", "169.254.x.x é o endereço que o Windows inventa quando não recebe IP do DHCP. Confira o cabo/Wi-Fi e rode ipconfig /renew.",
                "O PC não recebeu IP do DHCP: checar a conexão e renovar o IP", "A placa de vídeo queimou", "O DNS do Google caiu", "O antivírus bloqueou a impressora"),
            M("Chamado #4612: \"O disco C: está 100% cheio.\"", "Descubra o que ocupa espaço (Limpeza de Disco, arquivos grandes, Downloads) antes de apagar qualquer coisa.",
                "Ver o que está ocupando espaço e limpar com segurança", "Apagar a pasta Windows", "Formatar o computador", "Desativar a lixeira para sempre"),
            M("Chamado #4613: \"O Wi-Fi cai toda hora, mas só perto do micro-ondas do refeitório.\"", "Micro-ondas interferem na faixa de 2,4 GHz. Use a rede de 5 GHz ou mude o canal do roteador.",
                "Interferência em 2,4 GHz: usar a rede de 5 GHz ou trocar o canal", "Trocar o mouse", "Reinstalar o navegador", "Aumentar o brilho do monitor"),
            M("Chamado #4614: \"Todo site HTTPS mostra 'certificado inválido' só no meu notebook.\"", "Se só uma máquina reclama de todos os certificados, a data/hora dela provavelmente está errada.",
                "Conferir a data e a hora do notebook", "Desativar o HTTPS do navegador", "Trocar o roteador", "Instalar outro navegador e ignorar os avisos"),
            // ── difícil
            H("Chamado URGENTE: \"Os arquivos de vários colegas viraram .locked e apareceu um bilhete pedindo bitcoin.\"", "Ransomware: isole as máquinas da rede na hora (para não espalhar) e acione o SOC. Não pague e não reinicie apagando evidências.",
                "Isolar as máquinas da rede imediatamente e acionar o SOC", "Pagar o resgate o mais rápido possível", "Reiniciar todos os PCs", "Renomear os arquivos tirando o .locked"),
            H("Chamado #4621: \"O servidor de arquivos fica lento todo dia às 9h, e só nesse horário.\"", "Padrão de horário pede investigação: monitore CPU/disco/rede às 9h (backup ou antivírus agendado, todo mundo entrando ao mesmo tempo).",
                "Monitorar os recursos às 9h para achar a causa (ex.: tarefa agendada)", "Comprar outro servidor hoje mesmo", "Pedir para todos chegarem às 10h", "Desligar o antivírus para sempre"),
            H("Chamado #4622: \"Um ex-funcionário que saiu ontem ainda acessa o e-mail pelo celular.\"", "Desligamento exige desativar a conta, revogar sessões e tokens já abertos e trocar senhas compartilhadas que a pessoa conhecia.",
                "Desativar a conta, revogar as sessões abertas e trocar senhas compartilhadas", "Pedir educadamente que ele pare", "Esperar a senha expirar em 90 dias", "Bloquear só o computador antigo dele"),
            H("Chamado #4623: \"O RAID 1 do servidor avisa que um dos dois discos falhou, mas tudo continua funcionando.\"", "Funciona, mas sem redundância: se o outro disco falhar, perde tudo. Troque o disco já e confira o backup.",
                "Trocar o disco com defeito logo e conferir o backup", "Nada: se está funcionando, está ótimo", "Desligar o RAID", "Formatar o disco que ainda está bom"),
        };

        // ── Sala de Jogos: caça ao bug da Bel (QA)
        public static readonly Quiz[] Bugs =
        {
            new Quiz("Python:   if idade > 18\n                print(\"adulto\")      Onde está o bug?", "Em Python, if termina com ':' e o bloco de dentro precisa de indentação.",
                "Falta ':' depois da condição", "print está escrito errado", "idade não pode ser número"),
            new Quiz("JavaScript:   const total = 10;  total = 20;      Onde está o bug?", "const não pode ser reatribuída. Use let se o valor vai mudar.",
                "Não dá para reatribuir uma const", "Falta ponto e vírgula", "20 é um número grande demais"),
            new Quiz("HTML:   <img src=\"logo.png\">      Qual o problema?", "Toda imagem precisa de alt, o texto lido por leitores de tela.",
                "Falta o atributo alt (acessibilidade)", "img precisa de </img>", "src deveria ser href"),
            new Quiz("PHP:   $sql = \"SELECT * FROM users WHERE id = \" . $_GET[\"id\"];      Qual o problema?", "Concatenar entrada do usuário no SQL abre SQL Injection. Use prepared statements.",
                "SQL Injection: deveria usar prepared statements", "Falta ORDER BY", "SELECT * é proibido em PHP"),
            new Quiz("React:   {produtos.map(p => <li>{p.nome}</li>)}      O que está faltando?", "Cada item de lista precisa de uma key única: <li key={p.id}>.",
                "A key em cada item da lista", "map não funciona dentro de JSX", "li precisa de className"),
            new Quiz("Python:   for i in range(10):\n            print(i)   (print sem recuo)      Onde está o bug?", "O conteúdo do for precisa estar indentado (4 espaços).",
                "O print precisa estar indentado", "range não aceita 10", "Faltam parênteses no for"),
            new Quiz("JavaScript:   if (x = 5) { ... }      Onde está o bug?", "Um '=' atribui; para comparar use '===' (ou '==').",
                "Usou = (atribuição) em vez de === (comparação)", "Falta um else", "5 deveria estar entre aspas"),
            new Quiz("CSS:   .titulo { color: blue }  e no HTML: <h1 class=\"Titulo\">      Por que não fica azul?", "Nomes de classe diferenciam maiúsculas: 'titulo' ≠ 'Titulo'.",
                "A classe no HTML tem 'T' maiúsculo e não bate com o CSS", "blue não é uma cor válida", "h1 não aceita class"),
            // ── médio
            M("Python:   lista = [10, 20, 30]\n            print(lista[3])      Qual o problema?", "Os índices vão de 0 a 2: lista[3] dá IndexError. O último é lista[2] (ou lista[-1]).",
                "O índice 3 não existe: a lista vai de 0 a 2", "print não aceita listas", "Listas começam no índice 1", "Falta ponto e vírgula"),
            M("JavaScript:   const soma = \"5\" + 3;   // esperava 8      Por que deu \"53\"?", "Com um texto, o + concatena. Converta antes: Number(\"5\") + 3.",
                "O + com texto junta as strings; é preciso converter para número", "O JavaScript não sabe somar", "const não aceita contas", "Faltou ponto e vírgula"),
            M("SQL:   UPDATE produtos SET preco = 0;      O que vai acontecer?", "Sem WHERE, TODAS as linhas mudam: todo o cardápio ficaria de graça!",
                "Todos os produtos ficam com preço 0 (faltou o WHERE)", "Dá erro de sintaxe", "Só o primeiro produto muda", "Nada, porque falta o COMMIT"),
            M("PHP:   if ($idade = 18) { echo \"18 anos\"; }      Qual o problema?", "Um '=' atribui 18 (sempre verdadeiro). Para comparar use == ou ===.",
                "Atribuição (=) no lugar de comparação (== ou ===)", "Falta o else", "echo deveria ser print", "18 deveria estar entre aspas"),
            M("JavaScript:   const dados = fetch(\"/api/menu\");\n            console.log(dados.itens);      Por que sai undefined?", "fetch devolve uma Promise. É preciso await (e depois await resposta.json()) antes de usar os dados.",
                "Faltou esperar a Promise (await) e converter com .json()", "fetch só funciona em Python", "console.log não mostra objetos", "A URL precisa terminar em .html"),
            // ── difícil
            H("Python:   def adicionar(item, lista=[]):\n                lista.append(item)\n                return lista\n            A 2ª chamada devolve os itens da 1ª também. Por quê?", "O valor padrão [] é criado UMA vez e compartilhado por todas as chamadas. Use lista=None e crie a lista dentro.",
                "O valor padrão [] é o mesmo objeto em todas as chamadas", "append não existe em Python", "O return deveria vir antes do append", "Funções não podem ter valor padrão"),
            H("JavaScript:   for (var i = 0; i < 3; i++) setTimeout(() => console.log(i), 100);\n            Imprime 3, 3, 3. Por quê?", "var tem escopo de função: as três funções enxergam o mesmo i, que já vale 3. Com let, cada volta tem o seu i.",
                "var é compartilhada pelas 3 funções; com let cada volta tem seu i", "setTimeout sempre imprime 3", "Faltou o ponto e vírgula", "O for deveria começar em 1"),
            H("React:   setContador(contador + 1);\n            setContador(contador + 1);      O contador só sobe 1. Por quê?", "As duas chamadas usam o mesmo valor antigo de contador. Use a forma de função: setContador(c => c + 1).",
                "As duas usam o valor antigo; use setContador(c => c + 1)", "React só deixa chamar o set uma vez", "Faltou o useEffect", "contador deveria ser const"),
            H("SQL:   SELECT * FROM clientes WHERE email = NULL;      Nunca retorna nada. Por quê?", "Qualquer comparação com NULL dá 'desconhecido'. Para testar vazio use IS NULL.",
                "Comparação com NULL nunca é verdadeira: use IS NULL", "A tabela clientes está vazia", "Faltou o ORDER BY", "NULL precisa de aspas: 'NULL'"),
            H("React:   useEffect(() => { fetch(url).then(r => r.json()).then(setDados); });      A página trava fazendo requisições sem parar. Por quê?", "Sem o array de dependências, o efeito roda a cada renderização, e setDados provoca uma nova: loop infinito. Use [] (ou [url]).",
                "Faltou o array de dependências: cada setDados renderiza e roda o efeito de novo", "fetch não pode ficar dentro de useEffect", "then não funciona com JSON", "setDados deveria ser const"),
            H("JavaScript:   comentarios.innerHTML = textoDoUsuario;      Qual o problema de segurança?", "innerHTML interpreta o texto como HTML: um usuário pode injetar <script> (XSS). Para texto, use textContent.",
                "Abre brecha para XSS: use textContent para mostrar texto", "innerHTML é lento demais", "Faltou o ponto e vírgula", "Nenhum, innerHTML é seguro"),
        };

        // ── Laboratório de Inovação: dilemas de IA responsável (efeitos na reputação)
        public static readonly Quiz[] Ai =
        {
            new Quiz("O time quer treinar um modelo com conversas de clientes do Café 365 sem avisar ninguém.", "Dados pessoais exigem consentimento e anonimização (a LGPD diz isso).",
                "Exigir consentimento e anonimizar os dados", "Usar logo, ninguém vai saber", "Usar só metade dos dados") { Effect = new[] { 3, -4, 0 } },
            new Quiz("O modelo de crédito recusa muito mais pedidos de um bairro específico.", "Isso pode ser viés nos dados. Modelos aprendem preconceitos se ninguém conferir.",
                "Investigar o viés antes de usar o modelo", "É só matemática, segue o jogo", "Esconder o bairro e torcer para dar certo") { Effect = new[] { 3, -4, 0 } },
            new Quiz("Um colega quer colar código gerado por IA direto em produção, sem revisar.", "Código de IA é código como qualquer outro: precisa de revisão e testes.",
                "Revisar e testar como qualquer outro código", "Confiar, IA não erra", "Colar agora e revisar depois") { Effect = new[] { 3, -4, 0 } },
            new Quiz("Você usou IA para ajudar num relatório da faculdade.", "Transparência: diga que usou, revise e garanta que você entende o conteúdo.",
                "Avisar que usou IA e revisar tudo", "Entregar como se fosse 100% seu", "Não falar nada e torcer") { Effect = new[] { 3, -4, -1 } },
            new Quiz("O chatbot da empresa começou a inventar preços que não existem.", "IA também 'alucina'. Tire do ar, avise o time e corrija antes que clientes sejam enganados.",
                "Tirar do ar, avisar o time e corrigir", "Deixar, ninguém reclamou ainda", "Colocar um aviso bem pequeno") { Effect = new[] { 3, -4, 0 } },
        };

        // ── Refeitório: conversas de networking
        public static readonly string[] Network =
        {
            "Dica de carreira: monte um portfólio no GitHub com 3 projetos pequenos e bem explicados. Vale mais que 30 projetos pela metade.",
            "Aprenda a escrever bem: documentação, commits e mensagens claras fazem você ser lembrado(a) pelo time.",
            "Inglês técnico abre muitas portas. Ler documentação em inglês todo dia já ajuda muito.",
            "Não tenha medo de dizer 'não sei'. Diga 'não sei, mas vou descobrir' — e descubra!",
            "Peça feedback com frequência. Quem pede feedback cresce mais rápido.",
            "Conheça gente de outras áreas: o pessoal de Redes salva o dia do Frontend mais vezes do que você imagina.",
            "Antes de perguntar, tente 15 minutos sozinho(a). Depois disso, pergunte sem vergonha.",
            "Comunidades e eventos gratuitos são ótimos para aprender e conhecer gente. Vá a um!",
        };

        public static readonly string[] RobotLines =
        {
            "BIP BOP. ROTA RECALCULADA. ENTREGANDO CAFÉ NA SALA ERRADA COM 97% DE CONFIANÇA.",
            "OLÁ, HUMANO. MEU MODELO FOI TREINADO COM 10 MIL FOTOS DE XÍCARAS. AINDA CONFUNDO XÍCARA COM GATO.",
            "BIP. LEMBRETE: UM ROBÔ NÃO DEVE TOMAR DECISÕES IMPORTANTES SEM UM HUMANO NO CONTROLE.",
            "ERRO 418: SOU UM BULE DE CHÁ. (É um código HTTP de brincadeira de verdade!)",
        };
    }

    partial class GameView
    {
        readonly Dictionary<string, float> activityCooldown = new Dictionary<string, float>();
        readonly HashSet<int> watered = new HashSet<int>();

        bool OnCooldown(string key, float seconds)
        {
            float t;
            if (activityCooldown.TryGetValue(key, out t) && time - t < seconds) return true;
            activityCooldown[key] = time;
            return false;
        }

        static string ActivityLabel(Npc n)
        {
            switch (n.Activity)
            {
                case "talk": return "Tech Talk (quiz)";
                case "phishing": return "detector de phishing";
                case "ticket": return "atender chamado";
                case "bugs": return "caça ao bug";
                case "ia": return "dilema de IA";
                case "garden": return "missão do jardim";
                case "network": return "networking";
                case "meal": return "pedir o almoço";
                default: return null;
            }
        }

        // aula do Auditório concluída: prêmio só na primeira vez
        public void LessonDone(string id, int hits, int total)
        {
            if (id == null) return;
            bool first = gs.Prog.Stat("aula:" + id) == 0;
            gs.Prog.Stats["aula:" + id + ":acertos"] = Math.Max(gs.Prog.Stat("aula:" + id + ":acertos"), hits);
            if (!first) { gs.Prog.Save(); return; }
            gs.Prog.Stats["aula:" + id] = 1;
            var l = Lessons.Get(id);
            Reward(Lessons.BasePts + Lessons.PtsPerHit * hits, Lessons.BaseXp + Lessons.XpPerHit * hits, hits == total ? 1 : 0, "Aula concluída: " + (l != null ? l.Title : id));
        }

        void Reward(int pts, int xp, int rep, string why)
        {
            if (pts != 0) gs.Prog.ExtraPts += pts;
            if (xp > 0) gs.Prog.AddXp("soft", (int)Math.Round(xp * Career.XpMultiplier(gs.Prog)));
            if (rep != 0) gs.Prog.Reputation = Math.Max(0, Math.Min(100, gs.Prog.Reputation + rep));
            var parts = new List<string>();
            if (pts != 0) parts.Add((pts > 0 ? "+" : "") + pts + " pts");
            if (xp > 0) parts.Add("+" + xp + " XP");
            if (rep != 0) parts.Add((rep > 0 ? "+" : "") + rep + " reputação");
            if (parts.Count > 0) Toast(why + ": " + string.Join(" · ", parts), rep < 0 ? Art.Hx(0xF85149) : Art.Hx(0x3FB950));
            gs.Prog.Save();
            CheckPromotion();
        }

        // Faz uma pergunta de múltipla escolha. Prêmio só na primeira vez que cada pergunta é acertada.
        void AskQuiz(Npc n, string bank, Quiz[] qs, int pts, string stat, Action<bool> after = null)
        {
            // dificuldade: o médio libera depois de 3 acertos no fácil (ou ao chegar a Pleno); o difícil, depois de 3 no médio (ou a Sênior)
            Func<int, int> okAt = lvl => Enumerable.Range(0, qs.Length).Count(i => qs[i].Level == lvl && gs.Prog.Stat("ok:" + bank + ":" + i) > 0);
            int sen = Career.Seniority(Career.TotalXp(gs.Prog));
            int maxLvl = 0;
            if (okAt(0) >= 3 || sen >= 2) maxLvl = 1;
            if (maxLvl >= 1 && (okAt(1) >= 3 || sen >= 3)) maxLvl = 2;
            if (maxLvl > gs.Prog.Stat("lvl:" + bank))
            {
                if (gs.Prog.Stat("lvl:" + bank) >= 0 && maxLvl > 0)
                    Toast(maxLvl == 1 ? "Perguntas de nível MÉDIO liberadas aqui (valem o dobro)!" : "Perguntas de nível DIFÍCIL liberadas aqui (valem o triplo)!", Art.Hx(0xF0B429));
                gs.Prog.Stats["lvl:" + bank] = maxLvl;
            }
            var pending = Enumerable.Range(0, qs.Length).Where(i => qs[i].Level <= maxLvl && gs.Prog.Stat("ok:" + bank + ":" + i) == 0).ToList();
            bool fresh = pending.Count > 0;
            // prefere as perguntas do nível mais alto liberado
            var top = pending.Where(i => qs[i].Level == maxLvl).ToList();
            if (top.Count > 0 && rng.Next(10) < 7) pending = top;
            var pool = fresh ? pending : Enumerable.Range(0, qs.Length).Where(i => qs[i].Level <= maxLvl).ToList();
            int qi = pool[rng.Next(pool.Count)];
            var q = qs[qi];
            string tag = q.Level == 2 ? "[DIFÍCIL]  " : q.Level == 1 ? "[MÉDIO]  " : "[FÁCIL]  ";
            int mult = q.Level + 1;
            var order = Enumerable.Range(0, q.Options.Length).OrderBy(x => rng.Next()).ToList();
            var opts = new List<KeyValuePair<string, Action>>();
            foreach (var oi in order)
            {
                int pick = oi;
                opts.Add(new KeyValuePair<string, Action>(q.Options[pick], () =>
                {
                    bool ok = pick == 0;
                    if (ok)
                    {
                        Sfx.Play("ach");
                        Say(n, "Isso mesmo! " + q.Explain);
                        if (fresh)
                        {
                            gs.Prog.Stats["ok:" + bank + ":" + qi] = 1;
                            gs.Prog.AddStat(stat);
                            Reward(pts * mult, 3 * mult, q.Level == 2 ? 1 : 0, q.Level == 2 ? "Resposta certa (difícil)" : q.Level == 1 ? "Resposta certa (médio)" : "Resposta certa");
                        }
                        else Say(n, "(Você já tinha acertado essa antes, então não vale prêmio. Mas treinar é sempre bom!)");
                    }
                    else
                    {
                        Sfx.Play("err");
                        Say(n, "Não foi dessa vez. " + q.Explain);
                    }
                    if (after != null) after(ok);
                }));
            }
            pages.Enqueue(new DialogPage { Npc = n, Text = tag + Fmt(q.Prompt), Options = opts });
            if (!fresh) Toast("Você já acertou todas as perguntas daqui! Agora é só treino.", Art.Hx(0x8B949E));
        }

        void RunActivity(Npc n)
        {
            switch (n.Activity)
            {
                case "talk":
                    Say(n, n.Lines[n.LineIdx++ % n.Lines.Length]);
                    Say(n, "Vamos ver quem prestou atenção! Acertou, ganha pontos e XP.");
                    AskQuiz(n, "talk", ActivityBank.Talk, 15, "quiz");
                    break;
                case "phishing":
                    Say(n, "Bem-vindo(a) ao SOC! Eu recebo centenas de e-mails suspeitos por dia. Me ajuda a separar golpe de e-mail de verdade?");
                    AskQuiz(n, "phish", ActivityBank.Phishing, 15, "phish", ok => { if (ok && gs.Prog.Stat("phish") == 5) Say(n, "Cinco golpes desmascarados! Você tem olho de analista de segurança."); });
                    break;
                case "ticket":
                    Say(n, "Chegou chamado novo na fila do Help Desk! Me ajuda a resolver?");
                    AskQuiz(n, "ticket", ActivityBank.Tickets, 15, "tickets", ok => { if (ok) gs.Prog.Reputation = Math.Min(100, gs.Prog.Reputation + 1); });
                    break;
                case "bugs":
                    Say(n, "Caça ao bug! Eu mostro um trecho de código com problema e você aponta o erro.");
                    AskQuiz(n, "bugs", ActivityBank.Bugs, 15, "bugs");
                    break;
                case "ia":
                    RunAiDilemma(n);
                    break;
                case "garden":
                    RunGarden(n);
                    break;
                case "network":
                    Say(n, ActivityBank.Network[rng.Next(ActivityBank.Network.Length)]);
                    if (gs.Prog.Stat("net:" + n.Name) == 0)
                    {
                        gs.Prog.Stats["net:" + n.Name] = 1;
                        gs.Prog.AddStat("network");
                        Then(() => Reward(5, 2, 1, "Networking com " + n.Name));
                    }
                    break;
                case "meal":
                    MealMenu(n);
                    break;
                default:
                    TalkFlavor(n);
                    break;
            }
        }

        void RunAiDilemma(Npc n)
        {
            var qs = ActivityBank.Ai;
            var pending = Enumerable.Range(0, qs.Length).Where(i => gs.Prog.Stat("ai:" + i) == 0).ToList();
            if (pending.Count == 0)
            {
                Say(n, "Você já me ajudou com todos os dilemas do laboratório. Responsabilidade é o que diferencia quem usa IA bem!");
                Say(n, n.Lines[n.LineIdx++ % n.Lines.Length]);
                return;
            }
            int qi = pending[rng.Next(pending.Count)];
            var q = qs[qi];
            Say(n, "Preciso da sua opinião sobre um caso real do laboratório.");
            var opts = new List<KeyValuePair<string, Action>>();
            foreach (var oi in Enumerable.Range(0, q.Options.Length).OrderBy(x => rng.Next()))
            {
                int pick = oi;
                opts.Add(new KeyValuePair<string, Action>(q.Options[pick], () =>
                {
                    gs.Prog.Stats["ai:" + qi] = 1;
                    int rep = q.Effect[pick];
                    if (rep > 0) { gs.Prog.AddStat("ai_good"); gs.Prog.GoodChoices++; Sfx.Play("ach"); Say(n, "Ótima decisão. " + q.Explain); Reward(10, 4, rep, "IA responsável"); }
                    else if (rep < 0) { gs.Prog.BadChoices++; Sfx.Play("err"); Say(n, "Hum... isso pode dar muito errado. " + q.Explain); Reward(0, 0, rep, "Decisão arriscada"); }
                    else { Say(n, "Não é o pior caminho, mas dá para fazer melhor. " + q.Explain); Reward(3, 1, 0, "Meio-termo"); }
                }));
            }
            pages.Enqueue(new DialogPage { Npc = n, Text = Fmt(q.Prompt + " O que você faz?"), Options = opts });
        }

        // Missão do jardim: regar as 3 plantas marcadas e voltar ao Seu Raimundo.
        void RunGarden(Npc n)
        {
            if (gs.Prog.Stat("garden_done") > 0)
            {
                Say(n, n.Lines[n.LineIdx++ % n.Lines.Length]);
                return;
            }
            if (gs.Prog.Stat("garden_started") == 0)
            {
                gs.Prog.Stats["garden_started"] = 1;
                Say(n, "Ô, {nome}! Me dá uma mão? Minhas costas tão doendo. Rega aquelas três árvores com o regador e volta aqui.");
                Say(n, "As árvores que precisam de água estão marcadas no chão. Capricha!");
                return;
            }
            if (watered.Count < 3)
            {
                Say(n, "Faltam " + (3 - watered.Count) + " árvore(s) para regar. Elas estão marcadas!");
                return;
            }
            gs.Prog.Stats["garden_done"] = 1;
            gs.Prog.Inv["cha_verde"] = gs.Prog.Count("cha_verde") + 2;
            Say(n, "Ficou uma beleza! Obrigado, {nome}. Toma: dois chás verdes que eu mesmo plantei. Ajudar os outros é o que faz um time.");
            Then(() => Reward(40, 5, 3, "Missão do jardim"));
        }

        void RunItemActivity(Item it)
        {
            string a = it.Text ?? "";
            if (a.StartsWith("water:"))
            {
                int k = int.Parse(a.Substring(6));
                if (gs.Prog.Stat("garden_started") == 0 || gs.Prog.Stat("garden_done") > 0)
                {
                    SayIcon(it.Title, "board", "Uma árvore bonita e bem cuidada. O Seu Raimundo, jardineiro, cuida de todas.");
                    return;
                }
                if (watered.Add(k))
                {
                    Sfx.Play("use");
                    SayIcon(it.Title, "water", "Você regou a árvore. Ela parece agradecida! (" + watered.Count + "/3)" + (watered.Count == 3 ? " Volte ao Seu Raimundo." : ""));
                }
                else SayIcon(it.Title, "water", "Essa árvore já foi regada hoje.");
                return;
            }
            switch (a)
            {
                case "rest":
                    if (OnCooldown("rest", 90)) { SayIcon(it.Title, "board", "Você acabou de descansar. Bora voltar aos desafios!"); return; }
                    gs.SpeedBuff = Math.Max(gs.SpeedBuff, 60);
                    gs.Prog.AddStat("rests");
                    Sfx.Play("coffee");
                    SayIcon(it.Title, "board", "Pausa ativa: você respirou fundo, alongou o pescoço e ouviu a água da fonte. Cabeça renovada! (+40% de velocidade por 60 s)\n\nDica de bem-estar: a regra 20-20-20 descansa os olhos — a cada 20 min, olhe para algo a 20 pés (6 m) por 20 segundos.");
                    break;
                case "pingpong":
                    // partida de verdade contra o Vini (janela do minijogo)
                    keys.Clear();
                    Sfx.Play("open");
                    if (MiniGameRequested != null) MiniGameRequested("pingpong");
                    break;
                case "robot":
                    SayIcon(it.Title, "board", ActivityBank.RobotLines[rng.Next(ActivityBank.RobotLines.Length)]);
                    break;
                default:
                    SayIcon(it.Title, "board", "Nada de especial por aqui.");
                    break;
            }
        }

        // resultado dos minijogos da Sala de Jogos (pingue-pongue e fliperama)
        public void MiniGameDone(string game, bool played, int a, int b, bool won)
        {
            keys.Clear();
            if (!played) return;
            if (game == "pingpong")
            {
                gs.Prog.AddStat("pingpong_partidas");
                if (won)
                {
                    gs.Prog.AddStat("pingpong");
                    Sfx.Play("ach");
                    SayIcon("Mesa de pingue-pongue", "board", "Você venceu o Vini por " + a + " a " + b + "! Ele já está pedindo revanche.");
                    Then(() => Reward(15, 0, 1, "Vitória no pingue-pongue"));
                }
                else SayIcon("Mesa de pingue-pongue", "board", "O Vini venceu por " + b + " a " + a + ". \"Treina mais um pouco e volta!\" Pelo menos a cabeça descansou.");
            }
            else
            {
                bool record = a > gs.Prog.Stat("invaders_best");
                if (record) gs.Prog.Stats["invaders_best"] = a;
                gs.Prog.AddStat("invaders_partidas");
                int pts = Math.Min(40, a / 50);
                Sfx.Play(record ? "ach" : "ok");
                SayIcon("Fliperama · INVASORES DE BUGS", "board", "Fim de jogo: " + a + " pontos, até a onda " + b + "." + (record ? " NOVO RECORDE!" : " Recorde: " + gs.Prog.Stat("invaders_best") + ".") +
                    (pts > 0 ? " A cada 50 pontos no fliperama você ganha 1 ponto no jogo (até 40 por partida)." : ""));
                if (pts > 0) Then(() => Reward(pts, 0, 0, "Invasores de Bugs"));
                else gs.Prog.Save();
            }
            StartDialog();
        }

        // Refeitório: cardápio pago com moedas. Cada prato mata a fome e/ou a sede em quantidades diferentes.
        void MealMenu(Npc cook)
        {
            var who = cook ?? World.Npcs.FirstOrDefault(x => x.Activity == "meal");
            Action<string> say = t => { if (who != null) Say(who, t); else SayIcon("Bandejão", "coffee", t); };
            Func<int, string, Action, Action> buy = (price, what, effect) => () =>
            {
                if (!Pay(price, "Refeitório")) return;
                lunched = true;
                effect();
            };
            var opts = new List<KeyValuePair<string, Action>>
            {
                new KeyValuePair<string, Action>("Prato feito (60 moedas): fome +70, sede +10 e energia para correr", buy(60, "Prato feito", () =>
                {
                    Sfx.Play("coffee");
                    Restore(70, 10);
                    gs.SpeedBuff = Math.Max(gs.SpeedBuff, 60);
                    say("Bom apetite! Arroz, feijão, frango e salada: com essa energia você atravessa o campus voando.");
                })),
                new KeyValuePair<string, Action>("Salada com suco natural (45 moedas): fome +40, sede +40", buy(45, "Salada", () =>
                {
                    Sfx.Play("coffee");
                    Restore(40, 40);
                    say("Escolha saudável! Quem cuida da saúde cuida do trabalho.");
                    if (gs.Prog.Stat("salada") == 0) { gs.Prog.AddStat("salada"); Reward(0, 2, 1, "Almoço saudável"); }
                })),
                new KeyValuePair<string, Action>("Almoço na mesa do time (80 moedas): fome +65, sede +20 e networking", buy(80, "Almoço com o time", () =>
                {
                    Sfx.Play("coffee");
                    Restore(65, 20);
                    say("Isso! Almoço com o time é o melhor networking que existe.");
                    say(ActivityBank.Network[rng.Next(ActivityBank.Network.Length)]);
                    if (gs.Prog.Stat("almoco_time") < 3) { gs.Prog.AddStat("almoco_time"); Reward(0, 3, 2, "Almoço com o time"); }
                })),
                new KeyValuePair<string, Action>("Suco de laranja (20 moedas): sede +45", buy(20, "Suco", () =>
                {
                    Sfx.Play("coffee");
                    Restore(0, 45);
                    say("Suquinho natural, geladinho. Saúde!");
                })),
                new KeyValuePair<string, Action>("Pão de queijo para levar (25 moedas): vai para o inventário (tecla 2)", buy(25, "Pão de queijo", () =>
                {
                    gs.Prog.Inv["pao_queijo"] = gs.Prog.Count("pao_queijo") + 1; gs.Prog.Save();
                    Toast("+1 pão de queijo no inventário (tecla 2)", Art.Hx(0xF5C542));
                    say("Leva quentinho! Comer mata a fome e ainda dá um empurrãozinho no próximo desafio.");
                })),
                new KeyValuePair<string, Action>("Agora não", null),
            };
            if (who != null) talking = who;
            pages.Enqueue(new DialogPage
            {
                Npc = who, Title = who == null ? "Bandejão" : null, Icon = who == null ? "coffee" : null,
                Text = Fmt("Hoje o cardápio tá caprichado! Você está com fome " + (int)gs.Prog.Hunger + "% e sede " + (int)gs.Prog.Thirst + "%, e tem " + Math.Max(0, gs.Prog.Wallet).ToString("N0") + " moedas. O que vai ser?"),
                Options = opts
            });
        }
    }
}
