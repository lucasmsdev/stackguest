# Stack Quest: carreira tech na Micorosoft

![Stack Quest](logo-wordmark.png)

Jogo para Windows com visual isométrico (estilo Habbo), bonecos de blocos e o escritório visto na diagonal, para aprender tecnologia. Você é estagiário(a) da **Micorosoft**, uma paródia da Microsoft. Você anda pelo campus, conversa com as pessoas e resolve desafios na estação de cada área, digitando o comando ou o código exato que o contexto pede. Quando acerta, o jogo explica cada parte da resposta.

## Áreas, níveis e estações
| Área | Sala | Estação | Desafios |
|---|---|---|---|
| Docker | Laboratório de Containers | Terminal | 102 |
| Redes | Data Center & Redes | Terminal | 102 |
| AWS | Centro de Nuvem | Console AWS + CloudShell | 102 |
| SQL | Centro de Dados SQL | Terminal MySQL | 102 |
| Python | Laboratório de Dados | IDE | 102 |
| PHP | Squad Backend PHP | IDE | 102 |
| HTML/CSS/JS | Estúdio Frontend | IDE | 102 |
| React | Squad React | IDE | 102 |
| Flutter | Mobile Lab | IDE | 102 |

São **918 desafios** no total. Cada área tem três níveis, com 34 desafios cada: **Iniciante → Intermediário → Profissional**.
- **Iniciante:** um conceito por vez, sintaxe básica.
- **Intermediário:** combina 2 ou 3 conceitos, opções menos óbvias, situações do dia a dia de um júnior.
- **Profissional:** problemas reais de pleno/sênior: segurança (XSS, SQL Injection, menor privilégio, sessões), desempenho, boas práticas e armadilhas clássicas. Exige pesquisar na Biblioteca.

As respostas longas aceitam as variações equivalentes que um aluno escreveria (ordem de atributos e opções, parênteses opcionais, `let`/`const` quando o objetivo não define etc.). Para abrir um nível, conclua o anterior. Nenhuma sala é trancada, e basta falar com o líder da área para liberar a estação dela.

## IDE "Micorosoft Code"
As áreas de programação abrem uma IDE no estilo do VS Code:
- **Explorador de arquivos:** cada desafio é um arquivo do projeto (`titulo-vitrine.html`, `funcao_dobro.py`, `OlaUsuario.jsx`…), organizado em pastas por nível.
- **Editor:** tem números de linha, cores de sintaxe, indentação automática e Tab. O código pode ocupar várias linhas.
- **Executar:** Ctrl+Enter, F5 ou o botão ▶. O resultado aparece no painel **SAÍDA**, e os erros contam em **PROBLEMAS**.
- **Terminal integrado:** os desafios de comando dessas áreas (`npm install`, `flutter create`, `pip install`, `composer install`…) usam o terminal da própria IDE.
- **Barra de status:** mostra linha, coluna e linguagem, como no VS Code.

## Console da AWS (simulado)
A estação do Centro de Nuvem abre uma cópia do **Console de Gerenciamento da AWS**, com uma conta fictícia (buckets, instâncias, usuários, funções, alarmes etc.):
- **Serviços:** S3, EC2, IAM, Lambda, CloudWatch (alarmes e logs), ECR, ECS e CloudFormation. Dá para abri-los pelo menu **Serviços**, pela **pesquisa** ou pelos atalhos da página inicial. Há também o seletor de **região** e o menu da **conta**.
- **Tarefas reais:** o aluno faz o que se pede clicando, como na AWS de verdade. Exemplos: criar um bucket com acesso público bloqueado, enviar o `index.html`, parar a instância certa (encerrar ou parar a errada conta erro), lançar uma EC2 t3.micro, criar um usuário IAM num grupo, ativar MFA, testar uma Lambda, criar um repositório ECR com *scan on push*, forçar um deploy no ECS, criar um alarme de CPU e subir uma stack do CloudFormation.
- **Avaliação pelo estado da conta:** o jogo confere o que foi feito. Ações erradas ou perigosas (ex.: deixar um bucket público, encerrar a instância em vez de parar) contam como erro, com explicação.
- **CloudShell:** fica embaixo do console. Quem preferir pode fazer a mesma tarefa pela AWS CLI (`aws s3 mb`, `aws ec2 stop-instances`, `aws iam create-user`…). Ao concluir, o jogo mostra os dois caminhos: **pelo console** e **pela CLI**. Dá para encadear comandos numa linha com `&&` ou `;`. Cada tarefa AWS começa com a conta da empresa no estado original (o que foi apagado ou desligado em outra tarefa não atrapalha). "Novo servidor" e "MFA para todos" são feitas só pelo console.

## Terminal MySQL (área de SQL)
O Centro de Dados SQL (descendo pelo corredor sul) tem um terminal `mysql` ligado a um banco de verdade, o `cafe365`, com as tabelas `clientes`, `produtos` e `pedidos`:
- **Os comandos rodam de verdade.** SELECT (com WHERE, LIKE, ORDER BY, LIMIT, GROUP BY, HAVING, JOIN, LEFT JOIN e subconsultas), INSERT, UPDATE, DELETE, CREATE TABLE, CREATE INDEX, ALTER TABLE, SHOW TABLES, DESCRIBE, EXPLAIN e transações. O resultado aparece em tabelas, igual ao cliente do MySQL.
- **Correção pelo resultado:** qualquer consulta equivalente vale. Nas consultas, o jogo compara o resultado com o da resposta de referência. Nos comandos que alteram dados, compara como as tabelas ficaram. Nos de estrutura, confere a tabela ou o índice criado.
- **Explorar é grátis:** SHOW TABLES, DESCRIBE e `SELECT * FROM tabela` não contam erro.
- **Erros como no MySQL**, sempre com uma explicação em português. Por exemplo: coluna desconhecida, coluna ambígua no JOIN, coluna fora do GROUP BY (`only_full_group_by`), chave duplicada e chave estrangeira.
- **Segurança:** UPDATE ou DELETE sem WHERE e DROP TABLE são apontados como erro grave. Nesses casos o banco volta sozinho ao estado anterior (rollback do treino).
- TAB completa nomes de tabelas, colunas e palavras-chave, e `help` mostra os comandos.

## Tela maior
O terminal, a IDE e o console da AWS ocupam a janela inteira. **Ctrl+B** (ou o botão no topo) esconde a lista de desafios para ganhar ainda mais espaço.

## O campus
O prédio tem **19 salas** ligadas por três corredores (principal, leste e sul):
- **9 áreas de tecnologia:** Docker, Redes, Python, PHP, Frontend, React, Flutter, AWS e SQL (o Centro de Dados fica descendo pelo corredor sul).
- **Recepção & RH:** onde começa o jogo.
- **Diretoria:** onde fica a CEO Valéria.
- **Biblioteca:** documentação de graça.
- **Refeitório:** o almoço dá velocidade e reputação uma vez por sessão.
- **Auditório:** palestras sobre IA responsável, segurança e carreira.
- **Jardim de Inverno:** um lugar para fazer pausas.
- **Sala de Jogos:** pingue-pongue de verdade contra o Vini e o fliperama Invasores de Bugs.
- **SOC de Segurança:** phishing e hacking ético.
- **Help Desk & Hardware.**
- **Laboratório de Inovação:** IA e robótica.

Mais personagens circulam pelo prédio e também podem trazer as situações de ética.

## Fome, sede e moedas
- Os desafios rendem **moedas** (o saldo ao lado da estrela, no painel de carreira). Comida, bebida, roupas e móveis são comprados com elas. Gastar não reduz a pontuação do ranking.
- **Fome** e **sede** aparecem no canto inferior esquerdo e caem com o tempo de jogo: a fome vai de 100% a 0% em cerca de 24 minutos e a sede em cerca de 16 (correndo, caem 50% mais rápido; o jogo minimizado não conta).
- Abaixo de 30%, a visão **embaça**: o escritório fica borrado e escuro, e o terminal, a IDE e o console AWS também (o texto do desafio fica difícil de ler). Em 0%, o personagem também anda 25% mais devagar.
- Onde comer e beber (preços em moedas):
  - **Refeitório** (Dona Lurdes ou o bandejão): prato feito 60 (fome +70, sede +10 e +40% de velocidade por 60 s), salada com suco 45 (fome +40, sede +40), almoço com o time 80 (fome +65, sede +20 e networking), suco de laranja 20 (sede +45) e pão de queijo para levar 25.
  - Bebedouros 8 (sede +35), máquina de café 15 (sede +15) e máquina de lanches 20 (fome +18).
  - Consumíveis da lojinha também matam a fome ou a sede (pão de queijo, pizza, bolo, café, energético, chá verde).

## Sua sala
Fica no corredor sul, ao lado do Centro de Dados SQL, e começa **vazia** (piso de cimento). Na Lojinha, aba **Minha sala**, o aluno compra com moedas:
- **Mesa** (simples, grande de carvalho, gamer com LED) e **computador** (notebook velho, PC com monitor, setup com dois monitores, setup gamer RGB). O computador da sala abre o terminal de qualquer área que o aluno já conhece e dá +3%, +6% ou +10% de XP. Ele precisa de uma mesa.
- **Cadeira** (banquinho, de escritório, gamer): a de escritório faz a fome e a sede caírem 10% mais devagar, e a gamer, 20%.
- **Piso**: carpete azul, madeira, porcelanato ou grama sintética.
- **Decoração**: vaso de planta, árvore no vaso, estante de livros, TV grande, letreiro neon, quadro, tapete redondo, sofá (pausa de +40% de velocidade), pufe, luminária, frigobar (água e suco mais baratos), máquina de café, fliperama particular, aquário, troféu, caixas de som e um robozinho assistente.
- **Parede da fama:** cada badge conquistada vira um quadro na parede lateral da sala (uma coluna por área, uma fileira por nível, com Profissional em cima). O quadro aparece assim que a badge é ganha. Perto da parede, **E** abre a tela de badges.
- Itens do mesmo tipo são substituídos pelo melhor comprado. A lojinha mostra o próprio modelo 3D de cada móvel.

## Patinete
O patinete elétrico (Lojinha, Utilidades) aparece embaixo do personagem quando está em uso e deixa a caminhada 50% mais rápida. A tecla **P** (ou o botão na Lojinha) sobe e desce dele.

## Criação de personagem
No início do jogo (e depois, a qualquer momento, com a tecla **C**) o aluno monta o personagem:
- **Corpo masculino ou feminino.** O feminino tem silhueta própria, cílios e lábios. As colegas do escritório também usam esse corpo.
- **Cabelo:** curto, longo, coque, raspado, black power, rabo de cavalo, tranças ou chanel, em 8 cores.
- **Parte de baixo:** calça ou saia. **Tom de pele**, cor da camisa e da calça/saia, além dos itens da lojinha.
- **Como os textos falam com você:** no masculino ("Bem-vindo, estagiário"), no feminino ("Bem-vinda, estagiária") ou neutro ("Bem-vindo(a)"). Isso vale para as falas dos personagens e para o título de carreira (ex.: "Engenheira DevOps Jr").

## Aulas no Auditório
No **telão do Auditório** há aulas sobre temas específicos, cada uma dada por alguém da empresa: 6 slides com tópicos, exemplos de código e a fala do professor, e um quiz de 3 perguntas no final.
- **Git e GitHub do zero** (Rafael): commits, branches, conflitos e Pull Requests.
- **Como a internet funciona** (Sandra): IP, DNS, portas, HTTP e HTTPS.
- **Segurança no dia a dia** (Vítor, CISO): senhas, 2FA, phishing, atualizações e backups.
- **IA generativa com responsabilidade** (Dra. Ada): alucinação, bons prompts, privacidade e vieses.
- **Bancos de dados: do zero ao JOIN** (Glória): tabelas, chaves, CRUD, JOIN, índices e SQL Injection.
- **Carreira em tecnologia** (Renata): portfólio, currículo, entrevistas e habilidades comportamentais.

A primeira conclusão de cada aula vale pontos e XP (mais acertos, mais prêmio). Concluir 3 aulas e todas as aulas são objetivos do jogo.

## Som
O jogo tem um mixer de áudio próprio (vários sons ao mesmo tempo, tudo sintetizado, sem arquivos):
- **Passos** do personagem e dos colegas, com som diferente para piso frio, madeira, carpete, piso elevado e grama.
- **Escritório vivo:** murmúrio de conversas e teclados ao fundo, mais intensos quanto mais gente trabalhando perto (o Refeitório é barulhento, a Biblioteca é silenciosa).
- **Vozes:** nas conversas, cada personagem "fala" com um tom de voz próprio enquanto o texto aparece.
- **Teclado:** quando o aluno digita no terminal, na IDE, no CloudShell ou na pesquisa, cada tecla faz barulho de teclado mecânico (espaço, Enter e apagar têm som próprio).
- A tecla **M** liga/desliga todo o som. Com a janela em segundo plano, o ambiente fica em silêncio.

## Atividades nas salas sem desafios
Personagens com o ícone **+ roxo** oferecem atividades. Cada acerto dá pontos, XP e às vezes reputação; o prêmio vale só na primeira vez que cada pergunta é acertada.
- **Auditório (Dra. Ada):** Tech Talk com quiz de tecnologia.
- **SOC (Kátia):** detector de phishing. O aluno lê um e-mail e decide se é golpe ou legítimo.
- **Help Desk (Nando e Jéssica):** atender chamados escolhendo a solução certa.
- **Sala de Jogos:**
  - a Bel propõe a **caça ao bug** (achar o erro num trecho de código);
  - os **fliperamas** rodam **Invasores de Bugs** (estilo Space Invaders: setas movem, Espaço atira; ondas cada vez mais rápidas, escudos de firewall e um trojan bônus). A cada 50 pontos no fliperama o aluno ganha 1 ponto no jogo, até 40 por partida, e o recorde fica salvo;
  - a **mesa de pingue-pongue** é uma partida de verdade contra o Vini (W/S, setas ou mouse; primeiro a 7 pontos). Vencer vale 15 pontos e 1 de reputação. Com o tênis da lojinha, a raquete fica maior.
- **Laboratório de Inovação (Dr. Lúcio):** dilemas de IA responsável que mexem na reputação. Os robôs também conversam.
- **Jardim:**
  - missão do Seu Raimundo: regar 3 árvores, com recompensa;
  - pausa ativa no banco ou na fonte.
- **Refeitório:**
  - cardápio da Dona Lurdes, em que cada refeição dá um bônus diferente;
  - networking com colegas no Refeitório e no Auditório.

Cada atividade tem um objetivo próprio (Caçador(a) de phishing, Herói do suporte, Caça-bugs…).

As perguntas do Tech Talk, do SOC, do Help Desk e da caça ao bug têm três níveis:
- **[FÁCIL]:** disponível desde o começo.
- **[MÉDIO]:** libera depois de 3 acertos no fácil daquela atividade, ou quando o aluno chega a Pleno.
- **[DIFÍCIL]:** libera depois de 3 acertos no médio, ou ao chegar a Sênior.

As perguntas médias e difíceis têm 4 opções plausíveis (nada de alternativa absurda) e valem o dobro e o triplo de pontos e XP. Exemplos: e-mails de phishing com domínio certo, golpe da troca de conta, ransomware, RAID degradado, closures com `var`, `setState` com valor antigo, `= NULL` no SQL e XSS com `innerHTML`.

## Badges
Cada nível concluído dá uma badge: bronze para Iniciante, prata para Intermediário e ouro para Profissional. São 27 no total. A badge traz o nome do aluno e a data, e pode ser **baixada como PNG** (tecla **B** abre a galeria).

## Carreira
- Cada desafio dá **XP** na área dele. O XP total define o cargo: Estagiário(a) → Júnior (200) → Pleno (600) → Sênior (1200) → Especialista (2000) → Principal (3000).
- O **título** acompanha a combinação de áreas mais praticadas:
  - Docker + AWS: Engenheiro(a) DevOps ou Cloud Engineer
  - Docker + Redes: SRE
  - Python + PHP: Dev Backend
  - Frontend + backend: Dev Fullstack
  - Flutter: Dev Mobile
  - SQL + Python: Engenheiro(a) de Dados; só SQL: Analista de Dados
  - e outras combinações
- **Desafios relâmpago** de qualquer área aparecem de tempos em tempos. Aceitar é **opcional**. Quem resolve dentro do tempo ganha até 250 pontos e **XP em dobro**.
- **Situações do dia a dia** (ícone "…" sobre um colega) trazem dilemas de ética e colaboração: senha emprestada, crédito por trabalho alheio, software pirata, phishing, ajudar o novato, deploy na sexta e outros. Cada escolha pode dar reputação, XP, pontos, uma dica, um item ou até um desafio relâmpago, mas também pode "queimar" o aluno na empresa.
- A **reputação** muda o jogo:
  - Reputação alta dá +10% de XP.
  - Com reputação muito baixa os colegas param de dar dicas e o XP cai 25%.
- **Efetivação:** com cargo Pleno e 6 badges, fale com a CEO Valéria na Diretoria.

## Jogar
Dê dois cliques em `StackQuest.exe`. Não precisa instalar nada, porque o jogo usa o .NET Framework 4 que já vem no Windows 10/11, e o Docker também não é necessário.

| Tecla | Ação |
|---|---|
| WASD / setas | andar (Shift corre) |
| Rodinha do mouse | aproximar / afastar a câmera isométrica |
| P | subir/descer do patinete |
| E / Espaço | conversar e interagir |
| 1 a 9 | escolher uma opção nos diálogos |
| 1 a 8 | usar consumíveis fora dos diálogos |
| TAB | painel de carreira (XP e progresso por área e nível) |
| B | badges |
| O | objetivos |
| C | guarda-roupa |
| M | som (liga/desliga) |
| F1 | ajuda |
| F11 (ou Alt+Enter) | tela cheia / volta para a janela (funciona também no terminal, na IDE e no console AWS) |
| Ctrl+R | reiniciar progresso |
| Ctrl+Enter / F5 | executar o código na IDE |
| Ctrl+B | esconder/mostrar a lista de desafios (estação maior) |

No terminal, ↑/↓ recuperam os comandos anteriores e Esc volta ao escritório.

### Visão isométrica (estilo Habbo)
O escritório aparece na diagonal, desenhado pelo OpenGL na resolução da tela, com bordas suavizadas (antisserrilhamento) e textos legíveis nas placas e quadros. A câmera segue o jogador suavemente, e pernas e braços dos bonecos balançam de forma contínua ao andar. Os pisos usam as texturas do escritório (com tapetes, sombras e luz das janelas), as paredes do fundo de cada sala ficam altas, e as que ficariam na frente do jogador são cortadas automaticamente para ele nunca sumir. As divisórias de vidro são baixas e deixam ver as salas. Os personagens são bonecos de blocos, com rosto desenhado e roupas, cabelos e chapéus da lojinha. A prévia da criação de personagem e do guarda-roupa mostra o mesmo boneco.

- Na visão isométrica, W sobe na tela (noroeste no mapa) e D vai para a direita. Combine duas teclas para andar em linha reta pelos corredores.
- A rodinha do mouse aproxima e afasta a câmera (com transição suave).
- Ao abrir, o jogo prepara o chão do escritório inteiro (barra "Montando o escritório..."), para nada engasgar ao andar. Numa placa de vídeo de notebook comum, cada quadro leva cerca de 10 ms e o jogo fica em 60 quadros por segundo.
- Se o OpenGL não funcionar no computador, o jogo abre automaticamente na visão de cima em HD (o motivo fica em `%APPDATA%\StackQuest\erros.txt`). Para forçar a visão de cima, defina a variável de ambiente `STACKQUEST_2D=1`.

## Pontuação
- Cada desafio vale 100 pontos. Cada dica custa 25 e cada erro custa 5, com mínimo de 10 pontos por desafio.
- A resposta pode ser revelada depois de 5 erros, mas aí o desafio vale 0.
- A **Biblioteca** (porta no fundo da Recepção) tem a documentação das 9 áreas e consultá-la é grátis. Os exemplos usam outros nomes, para o aluno adaptar.
- A **Lojinha** do corredor vende chapéus, roupas, mascotes, consumíveis (café, pão de queijo, energético, pizza, borracha, teletransporte, chá verde, bolo) e utilidades (vale-dica, tênis, patinete, teclado mecânico +10% XP, monitor ultrawide, livro de ética e outras). Gastar na loja não reduz a pontuação do ranking.

## Correção das respostas
- **Comandos** (Docker, AWS, Redes, npm, flutter, pip, composer…): a ordem das opções não importa, e `-an` vale o mesmo que `-na`. No Docker, as formas longa e curta (`--detach` = `-d`) também são equivalentes.
- **Código** (HTML, CSS, JS, JSX, Python, PHP, Dart): o corretor ignora espaços extras, quebras de linha, indentação, a diferença entre aspas simples e duplas, letras maiúsculas e o `;` final. Quando a resposta está quase certa, ele avisa "muito perto".
- **Respostas curtas** (portas, siglas, máscaras de rede): o corretor ignora maiúsculas e acentos.
- **SQL:** vale o resultado no banco, não o texto. Maiúsculas, espaços, apelidos e a ordem das colunas não importam. A ordem das linhas só importa quando o desafio pede ORDER BY.

## Para o professor
- O progresso fica salvo em `%APPDATA%\StackQuest\progresso.txt`, separado por usuário do Windows, com uma cópia de segurança em `Documentos\StackQuest\progresso.txt` e um backup `.bak` da gravação anterior. **Trocar de versão não perde nada:** cada desafio é guardado pela área e pelo título (não pela posição), então desafios novos ou reorganizados numa versão nova não bagunçam o progresso. Se o arquivo principal estragar (por exemplo, PC desligado no meio da gravação), o jogo recupera do backup. Na primeira vez que o jogo abre, ele copia o progresso da versão antiga (Docker Quest), se existir. **Ctrl+R** reinicia o progresso.
- Para os alunos, distribua só o `StackQuest-alunos.zip` (o executável e o arquivo `COMO JOGAR.txt`). A pasta `src` contém as respostas.
- **Visão de cima em HD (reserva, usada quando o OpenGL não está disponível):** o visual é ilustrado em alta resolução, desenhado já na escala da tela: pisos com textura (porcelanato, madeira, carpete, grama), sombras suaves, luz das janelas no chão, móveis com volume e personagens com proporções realistas e luz lateral. Para continuar leve, quase tudo vem pronto de caches: o chão em blocos de 8×8 tiles, as paredes de cada fileira e cada móvel viram uma imagem só, e os personagens têm cada quadro da caminhada guardado. A cada quadro o jogo só copia essas imagens e desenha o que é animado (luzes dos servidores, telas, vapor do café). Os caches dos arredores são preparados aos poucos, na folga de cada quadro, para não engasgar ao andar. Andando pelo escritório, um quadro leva cerca de 9 ms para desenhar, e o ritmo é sincronizado com o monitor (vsync pelo DWM do Windows), com passo de tempo constante: 60 quadros por segundo estáveis enquanto algo se move (16,6 ms ± 0,8 ms medidos), 30 parado e 15 em segundo plano. O jogo usa cerca de 150 a 250 MB de memória.
- Para compilar, rode `build.bat`, que usa o `csc.exe` do .NET Framework 4.
