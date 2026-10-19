using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerQuest
{
    static partial class Challenges
    {
        // Segunda leva de React: 24 desafios novos por nível (Iniciante, Intermediário, Profissional).
        // Usa o ReactVar (src/MaisReact.cs) para expandir variações «a|b».
        static partial void ExtraReact(List<Level> list)
        {
            const string R = "react";

            // componente de função nas três formas usuais (function / arrow com retorno implícito / arrow com chaves)
            Func<string, string, string, string[]> Comp = (nome, parametros, jsx) => ReactVar(
                "«|export default |export »function " + nome + "(" + parametros + ") {\n  return «" + jsx + "|(" + jsx + ")»\n}",
                "«|export »const " + nome + " = (" + parametros + ") => «" + jsx + "|(" + jsx + ")»",
                "«|export »const " + nome + " = (" + parametros + ") => {\n  return «" + jsx + "|(" + jsx + ")»\n}");

            // comando npm com uma opção que pode vir antes ou depois do pacote
            Func<string[], string, string[], string[]> NpmFlag = (verbos, pacote, flags) =>
            {
                var r = new List<string>();
                foreach (var f in flags) foreach (var v in verbos) r.Add("npm " + v + " " + f + " " + pacote);
                foreach (var f in flags) foreach (var v in verbos) r.Add("npm " + v + " " + pacote + " " + f);
                return r.ToArray();
            };

            // ═══════════════════════ INICIANTE ═══════════════════════
            list.Add(C(R, 0, "code", "Dois hooks de uma vez",
                "O componente de pedidos vai guardar estado e também buscar dados ao abrir. O Thiago pede para importar tudo numa linha só.",
                "Importe, numa única linha de import, os hooks useState e useEffect do pacote react.",
                ReactVar("import { «useState, useEffect|useEffect, useState» } from \"react\"",
                         "import React, { «useState, useEffect|useEffect, useState» } from \"react\""),
                A("Importações nomeadas ficam entre chaves e podem ser várias, separadas por vírgula.", "import { hookA, hookB } from \"react\""),
                A("import { useState, useEffect }", "Duas importações nomeadas na mesma linha.",
                  "from \"react\"", "Os dois hooks vêm do mesmo pacote."),
                "A ordem dentro das chaves não importa. Editores como o VS Code completam o import sozinhos ao digitar o nome do hook."));

            list.Add(C(R, 0, "code", "Rótulo do campo",
                "A Mari colou um <label for=\"email\"> do protótipo em HTML e o console do React mostrou um aviso.",
                "Escreva um <label> ligado ao campo de id email, com o texto E-mail, usando o nome de atributo correto do JSX.",
                A("<label htmlFor=\"email\">E-mail</label>"),
                A("for é palavra reservada do JavaScript (laço for), assim como class.", "No JSX o atributo vira htmlFor."),
                A("htmlFor=\"email\"", "Liga o rótulo ao campo com id=\"email\" (clicar no texto foca o campo).",
                  "E-mail", "Texto que aparece para o usuário."),
                "Rótulos ligados aos campos melhoram a acessibilidade: leitores de tela anunciam o texto do label ao focar o input."));

            list.Add(C(R, 0, "cmd", "Versão do Node",
                "O npm run dev do Caio falhou dizendo que o Vite precisa de uma versão mais nova do Node.js. Antes de tudo, confira qual está instalada.",
                "Mostre a versão do Node.js instalada na máquina.",
                A("node -v", "node --version"),
                A("O programa se chama node e, como muitos CLIs, aceita uma opção de versão.", "node -_"),
                A("node -v", "Mostra a versão do Node.js (ex.: v22.12.0)."),
                "npm -v mostra a versão do npm. Para alternar entre versões do Node na mesma máquina, use um gerenciador como o nvm."));

            list.Add(C(R, 0, "code", "Foto do produto",
                "Cada cartão do cardápio mostra a foto do produto. O endereço da imagem está na variável foto e o nome do produto na variável nome.",
                "Escreva uma tag <img> autofechada com src vindo da variável foto e alt vindo da variável nome (as duas entre chaves, nessa ordem).",
                A("<img src={foto} alt={nome} />", "<img alt={nome} src={foto} />"),
                A("Atributos também aceitam expressões JavaScript: troque as aspas por chaves.", "<img src={...} alt={...} />"),
                A("src={foto}", "O valor vem da variável (sem aspas!).",
                  "alt={nome}", "Texto alternativo para leitores de tela e quando a imagem não carrega.",
                  "/>", "No JSX, tags sem conteúdo precisam ser fechadas."),
                "src=\"foto\" (com aspas) seria o texto literal foto, não a variável."));

            list.Add(C(R, 0, "code", "Contando itens",
                "O ícone do carrinho deve mostrar quantos itens já foram escolhidos. Eles estão no array itens.",
                "Escreva um <p> que mostra o tamanho do array itens entre chaves, seguido do texto itens no carrinho.",
                A("<p>{itens.length} itens no carrinho</p>"),
                A("Arrays em JavaScript têm uma propriedade com a quantidade de elementos.", "<p>{itens.length} ...</p>"),
                A("{itens.length}", "Expressão JS: quantidade de elementos do array.",
                  "itens no carrinho", "Texto fixo depois da expressão."),
                "Como itens vem do estado, o número se atualiza sozinho a cada setItens."));

            list.Add(C(R, 0, "code", "Sem div extra",
                "O Caio envolveu um título e um parágrafo numa <div> só porque o componente precisa devolver um único elemento — e essa div a mais quebrou o layout flex da página.",
                "Escreva um fragmento (<> e </>) contendo <h1>Cardápio</h1> seguido de <p>Escolha sua bebida</p>.",
                new[] { "<>\n  CORPO\n</>", "<Fragment>\n  CORPO\n</Fragment>", "<React.Fragment>\n  CORPO\n</React.Fragment>" }
                    .SelectMany(m => ReactVar(m.Replace("CORPO", "<h1>«Cardápio|Cardapio»</h1>\n  <p>Escolha sua bebida</p>"))).ToArray(),
                A("O JSX precisa de um único elemento por fora, mas ele pode ser 'invisível'.", "Fragmento: <> ... </>"),
                A("<>", "Abre o fragmento: agrupa sem criar elemento no HTML final.",
                  "<h1>…</h1> <p>…</p>", "Os dois elementos ficam lado a lado dentro dele.",
                  "</>", "Fecha o fragmento."),
                "Em listas, quando o fragmento precisa de key, use a forma longa: <Fragment key={id}>...</Fragment>."));

            list.Add(C(R, 0, "code", "Estilo inline",
                "O selo de estoque precisa ficar verde, e a Mari não quer criar uma classe CSS só para isso.",
                "Escreva um <span> com o atributo style deixando a cor do texto (color) \"green\" e o texto Em estoque.",
                A("<span style={{ color: \"green\" }}>Em estoque</span>"),
                A("No JSX, style recebe um OBJETO JavaScript, não um texto.", "style={{ propriedade: \"valor\" }}"),
                A("style={{ ... }}", "Chaves de fora: expressão JSX. Chaves de dentro: o objeto.",
                  "color: \"green\"", "Propriedade CSS como chave do objeto; o valor vai entre aspas."),
                "Propriedades com hífen viram camelCase: background-color → backgroundColor, font-size → fontSize."));

            list.Add(C(R, 0, "cmd", "Instalando o roteador",
                "O painel do Café 365 vai ganhar várias telas (cardápio, pedidos, clientes). O Thiago escolheu a biblioteca de rotas mais usada do React.",
                "Instale o pacote react-router-dom com o npm.",
                A("npm install react-router-dom", "npm i react-router-dom", "npm add react-router-dom"),
                A("Para adicionar um pacote, use o mesmo comando de instalar dependências, seguido do nome.", "npm install NOME-DO-PACOTE"),
                A("npm install", "Baixa o pacote para node_modules.", "react-router-dom", "Nome do pacote; ele entra em dependencies no package.json."),
                "O pacote fica registrado no package.json; quem clonar o projeto só precisa rodar npm install."));

            list.Add(C(R, 0, "code", "Função no clique",
                "A função pedir já está pronta. O Caio escreveu onClick={pedir()} e o pedido era feito sozinho, assim que a tela abria!",
                "Escreva um <button> com o texto Pedir cujo onClick recebe a função pedir pelo nome, sem chamá-la (sem parênteses).",
                A("<button onClick={pedir}>Pedir</button>"),
                A("Com parênteses a função é EXECUTADA na renderização; sem eles, ela é só entregue ao React.", "onClick={pedir}"),
                A("onClick={pedir}", "Entrega a função; o React a chama quando houver clique.",
                  "Pedir", "Texto do botão."),
                "Precisa passar argumento? Aí envolva numa arrow function: onClick={() => pedir(id)}."));

            list.Add(C(R, 0, "code", "Botão desabilitado",
                "Clientes apressados clicavam várias vezes em Enviar e o pedido era duplicado. O estado enviando fica true enquanto a requisição está em andamento.",
                "Escreva um <button> com o texto Enviar que fica desabilitado (atributo disabled) conforme a variável enviando.",
                A("<button disabled={enviando}>Enviar</button>"),
                A("O atributo disabled aceita um booleano vindo de uma expressão.", "disabled={...}"),
                A("disabled={enviando}", "true: botão desabilitado; false: o React remove o atributo."),
                "disabled=\"false\" (com aspas) é um texto não vazio e DESABILITARIA o botão: booleanos vão entre chaves."));

            list.Add(C(R, 0, "code", "Comentário no JSX",
                "A Mari quer deixar um lembrete dentro do JSX do topo da página, mas // apareceu escrito na tela.",
                "Escreva um comentário de JSX com o texto TODO: banner da promo.",
                A("{/* TODO: banner da promo */}"),
                A("Dentro do JSX, comentários precisam estar numa expressão (entre chaves).", "Use o comentário de bloco do JS: /* ... */"),
                A("{ }", "Abre uma expressão JavaScript no meio do JSX.",
                  "/* TODO: banner da promo */", "Comentário de bloco: não aparece na tela."),
                "Fora do JSX (no corpo da função), os comentários normais // e /* */ funcionam como sempre."));

            list.Add(C(R, 0, "code", "Carrinho vazio",
                "O carrinho começa sem nenhum produto e vai receber itens ao longo do pedido.",
                "Crie o estado itens (com setItens) começando como um array vazio.",
                A("const [itens, setItens] = useState([])"),
                A("O valor inicial do useState pode ser de qualquer tipo: número, texto, objeto ou array.", "useState([])"),
                A("[itens, setItens]", "Valor atual e função para trocá-lo.", "useState([])", "Começa com um array vazio."),
                "Com array vazio, itens.map(...) funciona desde a primeira renderização. Começar com undefined faria o map quebrar."));

            list.Add(C(R, 0, "code", "Saudação com props",
                "O topo do painel deve cumprimentar o barista do turno. O nome chega como prop.",
                "Crie o componente de função Saudacao que desestrutura a prop nome no parâmetro e retorna um <h2> com o texto Oi, seguido de {nome}.",
                Comp("Saudacao", "{ nome }", "<h2>Oi, {nome}</h2>"),
                A("As props chegam num objeto; dá para desestruturar direto no parâmetro.", "function Saudacao({ nome }) { return <h2>Oi, {nome}</h2> }"),
                A("({ nome })", "Pega só a prop nome do objeto de props.",
                  "<h2>Oi, {nome}</h2>", "Mostra o texto fixo e o valor da prop."),
                "Uso: <Saudacao nome=\"Lia\" />. Sem desestruturar seria function Saudacao(props) e {props.nome}."));

            list.Add(C(R, 0, "cmd", "Ferramenta só de desenvolvimento",
                "O time quer formatar o código automaticamente com o Prettier. Ele só é usado por quem programa, não vai para o site publicado.",
                "Instale o pacote prettier como dependência de desenvolvimento.",
                NpmFlag(new[] { "install", "i", "add" }, "prettier", new[] { "--save-dev", "-D" }),
                A("O npm separa dependencies (vão para produção) de devDependencies (só para desenvolver).", "npm install --save-dev PACOTE  (ou -D)"),
                A("npm install", "Instala o pacote.", "--save-dev / -D", "Registra em devDependencies no package.json.", "prettier", "Nome do pacote."),
                "Linters, formatadores, ferramentas de teste e o próprio Vite costumam ser devDependencies."));

            list.Add(C(R, 0, "code", "Prop booleana",
                "O componente Modal recebe a prop aberto para decidir se aparece. O Thiago quer mostrá-lo aberto.",
                "Renderize <Modal /> passando a prop aberto com valor true, na forma curta (só o nome da prop).",
                A("<Modal aberto />", "<Modal aberto={true} />"),
                A("No JSX, escrever só o nome da prop equivale a passar true.", "<Modal aberto />"),
                A("<Modal aberto />", "Mesmo que aberto={true}."),
                "Cuidado: aberto=\"false\" é um TEXTO (verdadeiro em if). Para false use aberto={false} ou simplesmente não passe a prop."));

            list.Add(C(R, 0, "code", "Exportando o componente",
                "O componente Cardapio está pronto no arquivo Cardapio.jsx, mas o App.jsx não consegue importá-lo.",
                "Escreva a linha que exporta Cardapio como exportação padrão do arquivo.",
                A("export default Cardapio"),
                A("Cada arquivo pode ter UMA exportação padrão.", "export ______ Cardapio"),
                A("export default", "Exportação padrão do módulo.", "Cardapio", "O componente exportado."),
                "Também dá para exportar na declaração: export default function Cardapio() { ... }."));

            list.Add(C(R, 0, "code", "Importando um componente",
                "Agora o App.jsx precisa usar o Cardapio, que está no arquivo Cardapio.jsx da mesma pasta.",
                "Importe a exportação padrão Cardapio do arquivo ./Cardapio.",
                A("import Cardapio from \"./Cardapio\"", "import Cardapio from \"./Cardapio.jsx\""),
                A("Exportação padrão é importada SEM chaves.", "Arquivos do projeto começam com ./ no caminho."),
                A("import Cardapio", "Nome local da exportação padrão (sem chaves).",
                  "from \"./Cardapio\"", "Caminho relativo; a extensão .jsx pode ser omitida no Vite."),
                "Sem ./ o import procuraria um pacote chamado Cardapio em node_modules."));

            list.Add(C(R, 0, "code", "Preço com duas casas",
                "O preço 12.5 aparecia como R$ 12.5 no cardápio. O cliente reclamou que parece errado.",
                "Escreva um <p> com o texto R$ seguido da expressão que mostra preco com duas casas decimais usando toFixed(2).",
                A("<p>R$ {preco.toFixed(2)}</p>"),
                A("Números em JS têm um método que fixa as casas decimais.", "{preco.toFixed(2)}"),
                A("R$", "Texto fixo.", "{preco.toFixed(2)}", "12.5 vira \"12.50\" (um texto)."),
                "Para o formato brasileiro com vírgula use preco.toLocaleString(\"pt-BR\", { style: \"currency\", currency: \"BRL\" })."));

            list.Add(C(R, 0, "cmd", "Desinstalando um pacote",
                "O projeto ainda tem a biblioteca de datas moment, que foi trocada pelo Intl do próprio navegador. Ela só deixa o bundle maior.",
                "Remova o pacote moment do projeto com o npm.",
                A("npm uninstall moment", "npm remove moment", "npm rm moment", "npm un moment", "npm r moment"),
                A("O contrário de install também é um subcomando do npm.", "npm un______ moment"),
                A("npm uninstall", "Apaga de node_modules e tira do package.json.", "moment", "Pacote removido."),
                "Antes de remover, procure no código se ninguém mais importa o pacote (busca por \"moment\")."));

            list.Add(C(R, 0, "code", "Envio do formulário",
                "O formulário de cadastro de clientes tem a função enviar pronta. Ela deve rodar quando o form for enviado (botão ou Enter).",
                "Escreva uma tag <form> (abrindo e fechando, vazia) que chama a função enviar no evento onSubmit.",
                A("<form onSubmit={enviar}></form>", "<form onSubmit={enviar} />"),
                A("O evento de envio de formulário é submit; no JSX fica em camelCase.", "<form onSubmit={...}></form>"),
                A("onSubmit={enviar}", "Chamada no clique do botão de envio ou ao apertar Enter num campo."),
                "Use onSubmit no form, não onClick no botão: assim o Enter também envia e a validação nativa do navegador funciona."));

            list.Add(C(R, 0, "code", "Link para o cardápio",
                "Com o React Router instalado, o menu usava <a href> e a página inteira recarregava a cada clique.",
                "Escreva um <Link> do React Router que leva para /cardapio, com o texto Menu.",
                A("<Link to=\"/cardapio\">Menu</Link>"),
                A("O React Router tem um componente próprio para links internos; o destino vai na prop to.", "<Link to=\"/caminho\">Texto</Link>"),
                A("<Link", "Navega sem recarregar a página (SPA).", "to=\"/cardapio\"", "Rota de destino."),
                "Use <a href> só para sites externos. Link vem de \"react-router-dom\" (import { Link } from ...)."));

            list.Add(C(R, 0, "code", "Rota do cardápio",
                "Os links já existem, mas falta dizer ao roteador qual componente mostrar em cada endereço.",
                "Escreva o <Route> autofechado que mostra <Cardapio /> no caminho /cardapio (props path e element).",
                A("<Route path=\"/cardapio\" element={<Cardapio />} />", "<Route element={<Cardapio />} path=\"/cardapio\" />"),
                A("Cada rota liga um caminho (path) a um elemento (element).", "<Route path=\"...\" element={<Componente />} />"),
                A("path=\"/cardapio\"", "Endereço da página.", "element={<Cardapio />}", "JSX que aparece nesse endereço (entre chaves)."),
                "As rotas ficam dentro de <Routes>, e o app todo dentro de <BrowserRouter>."));

            list.Add(C(R, 0, "code", "Somando com a função atualizadora",
                "O botão de fidelidade chama setPontos(pontos + 1) duas vezes seguidas, mas o cliente só ganha 1 ponto.",
                "Escreva a chamada de setPontos usando a função atualizadora com o parâmetro p, que devolve p + 1.",
                ReactVar("setPontos(«p|(p)» => «p + 1|1 + p»)"),
                A("Passe uma FUNÇÃO para o set: o React entrega o valor mais recente no parâmetro.", "setPontos(p => ...)"),
                A("setPontos(p => p + 1)", "p é sempre o valor atualizado; duas chamadas somam 2."),
                "pontos + 1 usa o valor 'fotografado' na renderização atual: as duas chamadas calculam o mesmo número."));

            list.Add(C(R, 0, "cmd", "Prévia do build",
                "O build de produção terminou. Antes de publicar, o Thiago quer abrir no navegador exatamente a versão gerada na pasta dist.",
                "Rode o script preview do projeto Vite.",
                A("npm run preview"),
                A("Scripts do package.json rodam com npm run NOME.", "npm run p______"),
                A("npm run preview", "Serve a pasta dist/ localmente (vite preview), como em produção."),
                "O preview NÃO recompila: rode npm run build antes. Ele serve para conferir, não para hospedar o site."));

            // ═══════════════════════ INTERMEDIÁRIO ═══════════════════════
            list.Add(C(R, 1, "code", "Status com cor",
                "Na lista de pedidos, cada número aparece verde se o pedido foi pago e laranja se está pendente. O CSS já tem as classes pago e pendente.",
                "Escreva um <span> mostrando {numero} cujo className é \"pago\" quando a variável pago for verdadeira e \"pendente\" quando for falsa (ternário dentro das chaves).",
                ReactVar("<span className={«pago|pago === true» ? \"pago\" : \"pendente\"}>{numero}</span>"),
                A("className também aceita uma expressão entre chaves.", "className={pago ? \"...\" : \"...\"}"),
                A("className={...}", "A classe é calculada a cada renderização.",
                  "pago ? \"pago\" : \"pendente\"", "Ternário: escolhe uma das duas classes.",
                  "{numero}", "Conteúdo do span."),
                "Para somar classes: className={\"pedido \" + (pago ? \"pago\" : \"pendente\")} ou com template string."));

            list.Add(C(R, 1, "code", "Sabores do dia",
                "O array sabores é só uma lista de textos (\"Baunilha\", \"Avelã\"...), sem id. Cada texto é único.",
                "Use sabores.map para gerar um <li> por sabor s, usando o próprio s como key e como texto (tudo entre chaves).",
                ReactVar("{sabores.map(«s|(s)» => <li key={s}>{s}</li>)}"),
                A("Sem id, um valor que seja único na lista pode servir de key.", "{sabores.map(s => <li key={s}>{s}</li>)}"),
                A("sabores.map(s => ...)", "Um <li> para cada texto.",
                  "key={s}", "O próprio texto é a chave (ele não se repete).", "{s}", "Mostra o sabor."),
                "Só use o índice (i) como key em listas que nunca são reordenadas nem filtradas."));

            list.Add(C(R, 1, "code", "Remover este item",
                "Cada linha do carrinho tem um botão para tirar aquele produto. A função remover recebe o id do item.",
                "Escreva um <button> com o texto Remover cujo onClick é uma arrow function sem parâmetros que chama remover(item.id).",
                ReactVar("<button onClick={() => «remover(item.id)|{ remover(item.id) }»}>Remover</button>"),
                A("Para passar argumento, a chamada precisa ficar DENTRO de outra função.", "onClick={() => remover(...)}"),
                A("() => remover(item.id)", "Só executa no clique, já com o id certo.",
                  "Remover", "Texto do botão."),
                "onClick={remover(item.id)} removeria TODOS os itens assim que a lista renderizasse."));

            list.Add(C(R, 1, "cmd", "Versão fixa do roteador",
                "O projeto antigo do delivery usa a API da versão 6 do React Router. Instalar sem versão traria a mais nova, com mudanças.",
                "Instale o pacote react-router-dom na versão principal 6 (use @ logo após o nome).",
                A("npm install react-router-dom@6", "npm i react-router-dom@6", "npm add react-router-dom@6"),
                A("O npm aceita PACOTE@VERSÃO.", "npm install react-router-dom@_"),
                A("react-router-dom@6", "Instala a versão 6 mais recente (ex.: 6.30.x).",
                  "npm install", "Grava a faixa ^6.x no package.json."),
                "Atualizar de versão principal (6 → 7) pode quebrar código: leia o guia de migração antes."));

            list.Add(C(R, 1, "code", "Carregando ou lista",
                "Enquanto os pedidos chegam da API, a tela deve mostrar um indicador; depois, a lista.",
                "Escreva uma expressão JSX que mostra <Spinner /> se carregando for verdadeiro e, senão, <Lista itens={itens} />.",
                ReactVar("{«carregando|carregando === true» ? <Spinner /> : <Lista itens={itens} />}"),
                A("Duas opções (isto OU aquilo) pedem o ternário.", "{carregando ? <A /> : <B ... />}"),
                A("carregando ?", "Condição.", "<Spinner />", "Enquanto carrega.", "<Lista itens={itens} />", "Depois de carregar, passando os itens como prop."),
                "Para três estados (carregando, erro, pronto), prefira ifs com retorno antecipado em vez de ternários aninhados."));

            list.Add(C(R, 1, "code", "Zero na tela",
                "O Caio escreveu {itens.length && <Carrinho />}. Com o carrinho vazio, apareceu um 0 solitário na tela!",
                "Corrija usando uma comparação: renderize <Carrinho /> com && somente quando itens.length for maior que 0.",
                ReactVar("{«itens.length > 0|0 < itens.length» && <Carrinho />}"),
                A("a && b devolve a quando a é falso — e o React mostra o número 0 (só ignora false, null e undefined).", "Faça o lado esquerdo virar booleano: itens.length > 0"),
                A("itens.length > 0", "Booleano de verdade: true ou false.",
                  "&& <Carrinho />", "Com false nada aparece (nem o 0)."),
                "Regra prática: do lado esquerdo do && use sempre um booleano (comparação ou !!valor)."));

            list.Add(C(R, 1, "code", "Rota com parâmetro",
                "Cada produto terá sua página: /produtos/1, /produtos/2... O componente Produto descobre qual mostrar pela URL.",
                "Escreva o <Route> autofechado com path /produtos/:id que mostra <Produto />.",
                A("<Route path=\"/produtos/:id\" element={<Produto />} />", "<Route element={<Produto />} path=\"/produtos/:id\" />"),
                A("Um trecho da URL com dois-pontos vira um parâmetro.", "path=\"/produtos/:id\""),
                A(":id", "Parâmetro dinâmico: casa com qualquer valor naquele trecho.",
                  "element={<Produto />}", "O mesmo componente para todos os produtos."),
                "Dentro de Produto, o valor do trecho é lido com useParams()."));

            list.Add(C(R, 1, "code", "Lendo o parâmetro",
                "A página /produtos/42 abriu o componente Produto. Agora ele precisa saber que o id é 42.",
                "Desestruture id do objeto devolvido pelo hook useParams() numa constante.",
                A("const { id } = useParams()"),
                A("useParams devolve um objeto com os parâmetros da rota.", "const { ... } = useParams()"),
                A("useParams()", "Lê os parâmetros da URL atual: { id: \"42\" }.",
                  "{ id }", "Desestruturação: o nome deve ser o mesmo do :id da rota."),
                "O valor sempre chega como TEXTO (\"42\"). Para comparar com números use Number(id)."));

            list.Add(C(R, 1, "code", "Adicionando ao carrinho",
                "O botão Adicionar fazia itens.push(novo) e a tela não atualizava. O produto escolhido está na variável novo.",
                "Chame setItens com a função atualizadora (parâmetro lista) que devolve um array novo com tudo de lista e, no fim, novo.",
                ReactVar("setItens(«lista|(lista)» => [...lista, novo])"),
                A("push altera o array original; o React compara referências e não percebe a mudança.", "[...lista, novo] cria um array novo"),
                A("setItens(lista => ...)", "Recebe a lista mais recente.",
                  "[...lista, novo]", "Copia os itens e acrescenta o novo no fim."),
                "Para colocar no início: [novo, ...lista]. Nunca use push, splice ou atribuição direta no estado."));

            list.Add(C(R, 1, "code", "Caixa de seleção controlada",
                "No cadastro, o cliente precisa aceitar os termos. O estado aceito (com setAceito) guarda se a caixa está marcada.",
                "Escreva um <input> autofechado com, nessa ordem, type=\"checkbox\", checked ligado a aceito e onChange com uma arrow (parâmetro e) que chama setAceito(e.target.checked).",
                ReactVar("<input type=\"checkbox\" checked={aceito} onChange={«e|(e)» => «setAceito(e.target.checked)|{ setAceito(e.target.checked) }»} />"),
                A("Em checkbox o estado fica em checked (não em value).", "onChange={e => setAceito(e.target.checked)}"),
                A("checked={aceito}", "A caixa mostra o estado.",
                  "e.target.checked", "true se ficou marcada, false se desmarcou."),
                "e.target.value de um checkbox é sempre \"on\": por isso se usa e.target.checked."));

            list.Add(C(R, 1, "cmd", "Pacotes desatualizados",
                "Antes da sprint de manutenção, a Mari quer saber quais dependências do painel têm versões mais novas disponíveis.",
                "Liste os pacotes desatualizados do projeto com o npm.",
                A("npm outdated"),
                A("O subcomando é uma palavra em inglês para 'desatualizado'.", "npm out_____"),
                A("npm outdated", "Mostra, para cada pacote, as versões Current (instalada), Wanted (permitida) e Latest (mais nova)."),
                "npm update sobe até a versão Wanted (respeita o ^ do package.json). Para Latest, instale com @latest e teste."));

            list.Add(C(R, 1, "code", "Cartões da vitrine",
                "A vitrine agora usa o componente CardProduto, que recebe o objeto inteiro do produto na prop produto.",
                "Use produtos.map para renderizar um <CardProduto /> autofechado por produto p, com key={p.id} e produto={p} (tudo entre chaves).",
                ReactVar("{produtos.map(«p|(p)» => <CardProduto «key={p.id} produto={p}|produto={p} key={p.id}» />)}"),
                A("O map pode devolver componentes, não só tags HTML.", "<CardProduto key={...} produto={...} />"),
                A("key={p.id}", "A key fica no elemento devolvido pelo map (não dentro do CardProduto).",
                  "produto={p}", "Passa o objeto inteiro como prop."),
                "key é usada pelo React e NÃO chega como prop: se o CardProduto precisar do id, leia produto.id."));

            list.Add(C(R, 1, "code", "Bloqueando o recarregamento",
                "Ao enviar o formulário de clientes, a página recarregava e tudo que foi digitado sumia.",
                "Escreva a função enviar que recebe o evento e, chama e.preventDefault() e, depois, salvar().",
                ReactVar("function enviar(e) {\n  e.preventDefault()\n  salvar()\n}",
                         "const enviar = «e|(e)» => {\n  e.preventDefault()\n  salvar()\n}"),
                A("O navegador recarrega a página no submit por padrão. O evento tem um método para impedir isso.", "e.preventDefault()"),
                A("enviar(e)", "Recebe o evento do onSubmit.",
                  "e.preventDefault()", "Cancela o comportamento padrão (recarregar).",
                  "salvar()", "Agora sim, sua lógica."),
                "Use no <form onSubmit={enviar}>. Para cliques que não devem 'subir' para o pai existe e.stopPropagation()."));

            list.Add(C(R, 1, "code", "Título da aba",
                "O caixa deixa o painel aberto numa aba enquanto atende. A aba deve mostrar quantos pedidos há na fila, por exemplo: 3 pedidos.",
                "Escreva um useEffect (corpo entre chaves) que define document.title como qtd seguido do texto \" pedidos\", rodando de novo sempre que qtd mudar.",
                ReactVar("useEffect(() => {\n  document.title = «qtd + \" pedidos\"|`${qtd} pedidos`»\n}, [qtd])"),
                A("Mexer no documento fora do React é um efeito colateral → useEffect.", "useEffect(() => { document.title = ... }, [qtd])"),
                A("document.title = ...", "Muda o texto da aba do navegador.",
                  "qtd + \" pedidos\"", "Número + texto (ou template string `${qtd} pedidos`).",
                  "[qtd]", "Atualiza o título a cada mudança da fila."),
                "Sem as chaves, a arrow devolveria o texto e o React avisaria: um efeito só pode retornar uma função de limpeza."));

            list.Add(C(R, 1, "code", "Link para cada produto",
                "Na vitrine, o nome de cada produto p deve levar para a página dele: /produtos/ seguido do id.",
                "Escreva um <Link> com o texto {p.nome} e a prop to montada (entre chaves) com \"/produtos/\" + p.id.",
                ReactVar("<Link to={«\"/produtos/\" + p.id|`/produtos/${p.id}`»}>{p.nome}</Link>"),
                A("Quando o destino depende de variável, to recebe uma expressão.", "to={\"/produtos/\" + p.id}"),
                A("to={...}", "Caminho calculado.",
                  "\"/produtos/\" + p.id", "Concatena texto e id (ou template string).", "{p.nome}", "Texto do link."),
                "to=\"/produtos/p.id\" (entre aspas) levaria literalmente para /produtos/p.id."));

            list.Add(C(R, 1, "code", "Botão Voltar",
                "A página de detalhes do pedido precisa de um botão que volte para a tela anterior, seja ela qual for. O navigate já foi criado com useNavigate().",
                "Escreva um <button> com o texto Voltar cujo onClick é uma arrow function que chama navigate(-1).",
                ReactVar("<button onClick={() => «navigate(-1)|{ navigate(-1) }»}>Voltar</button>"),
                A("navigate aceita um número: andar no histórico do navegador.", "Números negativos voltam."),
                A("navigate(-1)", "Volta uma página no histórico (como o botão do navegador)."),
                "navigate(1) avança. Para ir a um lugar fixo, passe o caminho: navigate(\"/pedidos\")."));

            list.Add(C(R, 1, "code", "Cartão com conteúdo",
                "A Mari quer um cartão reutilizável: título em cima e qualquer conteúdo que for colocado entre as tags <Cartao>...</Cartao>.",
                "Crie o componente de função Cartao que desestrutura titulo e children e retorna uma <section> com <h3>{titulo}</h3> seguido de {children}.",
                Comp("Cartao", "{ titulo, children }", "<section><h3>{titulo}</h3>{children}</section>")
                    .Concat(Comp("Cartao", "{ children, titulo }", "<section><h3>{titulo}</h3>{children}</section>")).Distinct().ToArray(),
                A("O que fica entre as tags do componente chega na prop especial children.", "function Cartao({ titulo, children }) { ... }"),
                A("({ titulo, children })", "Prop comum + conteúdo interno.",
                  "<h3>{titulo}</h3>", "Cabeçalho do cartão.", "{children}", "Renderiza o que foi colocado dentro."),
                "Uso: <Cartao titulo=\"Promoções\"><p>Leve 2, pague 1</p></Cartao>."));

            list.Add(C(R, 1, "cmd", "Outra porta para o dev",
                "A porta 5173 está ocupada pelo projeto de promoções. O Thiago quer subir o painel na porta 3000 sem mexer no package.json.",
                "Rode o script dev repassando ao Vite a opção --port 3000 (use o separador -- do npm).",
                A("npm run dev -- --port 3000", "npm run dev -- --port=3000", "npx vite --port 3000", "npx vite --port=3000"),
                A("Opções depois de -- não são do npm: vão para o script.", "npm run dev -- --opção valor"),
                A("npm run dev", "Roda o script.", "--", "Tudo depois é repassado ao comando do script (vite).", "--port 3000", "Opção do Vite."),
                "Sem o --, o npm tentaria interpretar --port como opção dele próprio e o Vite não receberia nada."));

            list.Add(C(R, 1, "code", "Só os doces",
                "A aba Doces deve listar apenas os produtos com categoria \"doce\".",
                "Numa expressão JSX, filtre produtos (p.categoria === \"doce\") e encadeie um map que gera <li key={p.id}>{p.nome}</li>, usando p nos dois.",
                ReactVar("{produtos.filter(«p|(p)» => p.categoria === \"doce\").map(«p|(p)» => <li key={p.id}>{p.nome}</li>)}"),
                A("filter devolve um array — e arrays têm map. Dá para encadear.", "{produtos.filter(...).map(...)}"),
                A("filter(p => p.categoria === \"doce\")", "Mantém só os doces.",
                  ".map(p => <li ...>)", "Transforma cada um em item de lista."),
                "Filtrar na renderização é o normal: não guarde a lista filtrada num estado separado."));

            list.Add(C(R, 1, "code", "Soma do carrinho",
                "O rodapé do carrinho mostra o total. Cada item do array itens tem a propriedade preco.",
                "Guarde em const total o resultado de itens.reduce com os parâmetros (soma, i), somando i.preco a soma, e valor inicial 0.",
                ReactVar("const total = itens.reduce((soma, i) => «soma + i.preco|i.preco + soma», 0)"),
                A("reduce percorre o array acumulando um único valor.", "itens.reduce((soma, i) => soma + ..., 0)"),
                A("(soma, i) =>", "Acumulador e item atual.",
                  "soma + i.preco", "Novo valor do acumulador.", ", 0", "Valor inicial (sem ele, um carrinho vazio dá erro)."),
                "É um valor derivado: calcule na renderização. Só use useMemo se o cálculo for realmente pesado."));

            list.Add(C(R, 1, "code", "Retorno antecipado",
                "A tela de detalhes quebrava com \"Cannot read properties of null\" porque pedido ainda é null enquanto a API não responde.",
                "Escreva um if que, quando !pedido, retorna <p>Carregando...</p> (antes do JSX principal).",
                ReactVar("if (!pedido) «return «<p>Carregando...</p>|(<p>Carregando...</p>)»|{ return «<p>Carregando...</p>|(<p>Carregando...</p>)» }»",
                         "if (pedido === null) «return «<p>Carregando...</p>|(<p>Carregando...</p>)»|{ return «<p>Carregando...</p>|(<p>Carregando...</p>)» }»"),
                A("Um componente pode ter mais de um return.", "if (!pedido) return <p>...</p>"),
                A("if (!pedido)", "null/undefined: ainda sem dados.",
                  "return <p>Carregando...</p>", "Sai cedo; o resto só roda com o pedido carregado."),
                "Os hooks (useState, useEffect...) precisam vir ANTES desse if: eles devem ser chamados sempre na mesma ordem."));

            list.Add(C(R, 1, "code", "Indo para a confirmação",
                "Depois de pagar, o cliente deve ir automaticamente para a página /obrigado — não é um link, é dentro da lógica.",
                "Escreva duas linhas: crie a constante navigate com useNavigate() e depois a constante finalizar, uma arrow function sem parâmetros que chama navigate(\"/obrigado\").",
                ReactVar("const navigate = useNavigate()\nconst finalizar = () => «navigate(\"/obrigado\")|{ navigate(\"/obrigado\") }»",
                         "const navigate = useNavigate()\nfunction finalizar() {\n  navigate(\"/obrigado\")\n}"),
                A("Para navegar por código, o React Router oferece um hook.", "const navigate = useNavigate(); navigate(\"/caminho\")"),
                A("useNavigate()", "Devolve a função de navegação.",
                  "navigate(\"/obrigado\")", "Troca de rota sem recarregar."),
                "Use navigate dentro de eventos ou efeitos, nunca direto no corpo da renderização."));

            list.Add(C(R, 1, "code", "Hook do tema",
                "Todo componente repete useContext(TemaContext). A Mari quer um atalho com nome claro.",
                "Crie o custom hook useTema, sem parâmetros, que retorna useContext(TemaContext).",
                ReactVar("«|export »function useTema() {\n  return useContext(TemaContext)\n}",
                         "«|export »const useTema = () => useContext(TemaContext)",
                         "«|export »const useTema = () => {\n  return useContext(TemaContext)\n}"),
                A("Custom hook é só uma função cujo nome começa com use e que chama outros hooks.", "function useTema() { return useContext(...) }"),
                A("function useTema()", "Nome com use: as regras dos hooks valem para ele.",
                  "return useContext(TemaContext)", "Devolve o valor do Provider mais próximo."),
                "Uso: const { tema, setTema } = useTema(). Quem usa nem precisa saber que existe um contexto."));

            list.Add(C(R, 1, "code", "Layout com Outlet",
                "Todas as páginas do painel têm o mesmo menu no topo. As rotas filhas ficam dentro de uma rota Layout.",
                "Crie o componente de função Layout, sem props, que retorna um fragmento com <Menu /> seguido de <Outlet />.",
                Comp("Layout", "", "<><Menu /><Outlet /></>"),
                A("Rotas aninhadas mostram a página filha no lugar de um componente especial do React Router.", "return <><Menu /><Outlet /></>"),
                A("<Menu />", "Aparece em todas as páginas.",
                  "<Outlet />", "Lugar onde a rota filha (cardápio, pedidos...) é renderizada."),
                "Configuração: <Route element={<Layout />}><Route path=\"/pedidos\" element={<Pedidos />} /></Route>."));

            // ═══════════════════════ PROFISSIONAL ═══════════════════════
            list.Add(C(R, 2, "code", "Texto do cliente seguro",
                "O mural de recados mostrava o comentário com dangerouslySetInnerHTML={{ __html: comentario }}. Alguém postou um <img onerror=...> que rodou um script na tela de todos (XSS).",
                "Reescreva o <p> mostrando comentario como texto comum, entre chaves, para o React escapar o conteúdo.",
                A("<p>{comentario}</p>"),
                A("O React já protege tudo o que é mostrado entre chaves: o HTML vira texto.", "<p>{...}</p>"),
                A("{comentario}", "Escapado: <img ...> aparece como texto, não vira tag."),
                "Se precisar MESMO renderizar HTML vindo de usuário, sanitize antes (ex.: biblioteca DOMPurify). O nome 'dangerously' é um aviso."));

            list.Add(C(R, 2, "code", "Estado que não precisa existir",
                "O perfil tinha useState para nomeCompleto e um useEffect para atualizá-lo quando nome ou sobrenome mudavam: renderização dupla e, às vezes, dados fora de sincronia.",
                "Substitua o estado por uma constante calculada na renderização: nomeCompleto igual a nome, um espaço e sobrenome.",
                ReactVar("const nomeCompleto = «nome + \" \" + sobrenome|`${nome} ${sobrenome}`»"),
                A("Se um valor pode ser calculado a partir de props/estado, ele não é estado.", "const nomeCompleto = nome + ..."),
                A("const nomeCompleto = ...", "Recalculado a cada renderização: sempre sincronizado.",
                  "nome + \" \" + sobrenome", "Concatenação com espaço (ou template string)."),
                "Estado derivado + efeito para sincronizar é um antipadrão comum. Só memorize com useMemo se o cálculo for caro."));

            list.Add(C(R, 2, "cmd", "Instalação no pipeline",
                "No GitHub Actions, o npm install às vezes atualizava pacotes e o build quebrava sem ninguém ter mudado nada.",
                "Use o comando do npm que faz uma instalação limpa e exata a partir do package-lock.json.",
                A("npm ci", "npm clean-install"),
                A("Existe um comando pensado para integração contínua (CI).", "npm c_"),
                A("npm ci", "Apaga node_modules e instala exatamente as versões do package-lock.json; falha se o lock estiver desatualizado."),
                "Faça commit do package-lock.json. No CI use npm ci; no dia a dia, npm install."));

            list.Add(C(R, 2, "code", "URL da API por ambiente",
                "A URL da API estava fixa no código: o build de homologação chamava a API de produção. O arquivo .env já tem VITE_API_URL.",
                "Guarde na constante API o valor de VITE_API_URL lido das variáveis de ambiente do Vite.",
                A("const API = import.meta.env.VITE_API_URL"),
                A("No Vite as variáveis ficam em import.meta.env (não em process.env).", "import.meta.env.NOME"),
                A("import.meta.env", "Objeto de variáveis do Vite, trocado no build.",
                  "VITE_API_URL", "Só variáveis com prefixo VITE_ chegam ao código do navegador."),
                "Tudo em import.meta.env vai parar no JavaScript público: NUNCA coloque senhas ou chaves secretas ali."));

            list.Add(C(R, 2, "code", "Tela carregada sob demanda",
                "A tela Relatorios usa uma biblioteca de gráficos enorme, e todos baixavam esse código mesmo sem abrir os relatórios.",
                "Crie a constante Relatorios com lazy, passando uma arrow function que faz o import dinâmico de \"./Relatorios\".",
                ReactVar("const Relatorios = «lazy|React.lazy»(() => import(\"./Relatorios«|.jsx»\"))"),
                A("lazy recebe uma função que devolve o import() dinâmico.", "const X = lazy(() => import(\"./X\"))"),
                A("lazy(...)", "Componente que só baixa o código na primeira vez em que aparece.",
                  "import(\"./Relatorios\")", "Import dinâmico: o Vite gera um arquivo JS separado (code splitting)."),
                "O arquivo importado precisa ter export default. Declare o lazy fora do componente, no topo do módulo."));

            list.Add(C(R, 2, "code", "Esperando o carregamento",
                "Com o lazy, o React precisa saber o que mostrar enquanto o código de Relatorios é baixado.",
                "Envolva <Relatorios /> num <Suspense> com a prop fallback igual a <p>Carregando...</p>.",
                A("<Suspense fallback={<p>Carregando...</p>}>\n  <Relatorios />\n</Suspense>"),
                A("Componentes lazy precisam de um Suspense acima deles.", "<Suspense fallback={...}> ... </Suspense>"),
                A("fallback={<p>Carregando...</p>}", "O que aparece enquanto o código chega.",
                  "<Relatorios />", "Componente lazy dentro do Suspense."),
                "Um só Suspense pode cobrir várias rotas lazy: coloque-o em volta do <Routes>."));

            list.Add(C(R, 2, "code", "Ordenar sem mutar",
                "O Caio fez produtos.sort(...) direto na prop e a lista do componente pai também ficou ordenada (e bagunçou outra tela).",
                "Crie a constante ordenados com uma CÓPIA de produtos ([...produtos]) ordenada por preço crescente com sort((a, b) => a.preco - b.preco).",
                ReactVar("const ordenados = «[...produtos].sort|produtos.toSorted|produtos.slice().sort»((a, b) => a.preco - b.preco)"),
                A("sort altera o array original. Copie antes de ordenar.", "[...produtos].sort((a, b) => ...)"),
                A("[...produtos]", "Cópia rasa: o array original fica intacto.",
                  "(a, b) => a.preco - b.preco", "Negativo: a vem antes. Ordem crescente de preço."),
                "Alternativa moderna: produtos.toSorted(...), que já devolve um array novo. Props e estado são somente leitura."));

            list.Add(C(R, 2, "code", "Objeto aninhado",
                "O estado pedido tem um objeto cliente dentro ({ id, cliente: { nome, telefone } }). Trocar o telefone com um spread só fez o nome do cliente sumir.",
                "Chame setPedido com a função atualizadora (parâmetro p) devolvendo { ...p, cliente: { ...p.cliente, telefone: novoTelefone } }.",
                ReactVar("setPedido(«p|(p)» => ({ ...p, cliente: { ...p.cliente, telefone: novoTelefone } }))"),
                A("O spread é raso: cada nível que muda precisa ser copiado.", "{ ...p, cliente: { ...p.cliente, telefone: ... } }"),
                A("...p", "Copia o pedido.", "cliente: { ...p.cliente,", "Copia o cliente (senão o nome se perde).",
                  "telefone: novoTelefone", "Só o telefone muda.", "({ ... })", "Parênteses para a arrow devolver o objeto."),
                "Estados muito aninhados são trabalhosos: prefira estruturas mais planas ou useReducer."));

            list.Add(C(R, 2, "cmd", "Duas cópias do React",
                "Depois de instalar uma biblioteca de gráficos, o app quebrou com \"Invalid hook call\". A suspeita é haver duas cópias do React no node_modules.",
                "Liste com o npm todas as instalações do pacote react na árvore de dependências.",
                A("npm ls react", "npm list react"),
                A("O npm mostra a árvore de dependências instaladas; dá para filtrar por pacote.", "npm ls PACOTE"),
                A("npm ls react", "Mostra cada caminho que leva ao react e a versão de cada cópia."),
                "Se aparecerem versões diferentes, alinhe as versões (peerDependencies) ou use \"overrides\" no package.json."));

            list.Add(C(R, 2, "code", "Rota protegida",
                "O componente RotaPrivada recebe { children } e a variável logado. Quem não fez login e digitava /admin na barra de endereço via o painel administrativo.",
                "Escreva o return: se logado, retorna children; senão, retorna <Navigate /> autofechado com to=\"/login\" e a prop replace (ternário).",
                ReactVar("return «logado|logado === true» ? children : <Navigate «to=\"/login\" replace|replace to=\"/login\"|to=\"/login\" replace={true}» />"),
                A("<Navigate /> redireciona assim que é renderizado.", "return logado ? children : <Navigate to=\"/login\" replace />"),
                A("logado ? children", "Logado: mostra a página protegida.",
                  "<Navigate to=\"/login\"", "Senão, redireciona.",
                  "replace", "Substitui a entrada no histórico: o Voltar não cai de novo na página bloqueada."),
                "Proteger no front é só experiência de uso: a API também PRECISA verificar o login em cada requisição."));

            list.Add(C(R, 2, "code", "Filtro na URL",
                "A busca de clientes ficava num useState e se perdia ao recarregar ou compartilhar o link. A Mari quer o termo na URL: /clientes?q=ana.",
                "Escreva duas linhas: desestruture params (primeiro item) de useSearchParams() e guarde em busca o valor de params.get(\"q\").",
                A("const [params] = useSearchParams()\nconst busca = params.get(\"q\")"),
                A("O React Router tem um hook parecido com useState para a query string.", "const [params] = useSearchParams(); params.get(\"q\")"),
                A("useSearchParams()", "Devolve [params, setParams], como o useState.",
                  "params.get(\"q\")", "Lê ?q=...; devolve null se não existir."),
                "Para atualizar: setParams({ q: texto }). Estado na URL sobrevive a recarregar e pode ser compartilhado."));

            list.Add(C(R, 2, "code", "Menu com link ativo",
                "O menu precisa destacar a página atual. O CSS tem a classe ativo.",
                "Escreva um <NavLink> para /pedidos, com texto Pedidos, cujo className é uma função que desestrutura { isActive } e devolve \"ativo\" quando ativo e \"\" (vazio) quando não.",
                ReactVar("<NavLink to=\"/pedidos\" className={({ isActive }) => «isActive ? \"ativo\" : \"\"|(isActive ? \"ativo\" : \"\")»}>Pedidos</NavLink>"),
                A("NavLink é um Link que sabe se está ativo; className pode ser uma função.", "className={({ isActive }) => isActive ? \"...\" : \"\"}"),
                A("<NavLink to=\"/pedidos\">", "Link que conhece a rota atual.",
                  "({ isActive }) =>", "Recebe o estado do link.", "isActive ? \"ativo\" : \"\"", "Classe só na página atual."),
                "Para a rota raiz \"/\" use a prop end, senão ela fica ativa em todas as páginas."));

            list.Add(C(R, 2, "code", "Favoritar e desfavoritar",
                "O coração de cada produto adiciona o id em favoritos (array de ids) — e, se já estiver lá, remove.",
                "Chame setFavoritos com a função atualizadora (parâmetro f): se f.includes(id), devolva f.filter(x => x !== id); senão, [...f, id] (use um ternário).",
                ReactVar("setFavoritos(«f|(f)» => f.includes(id) ? f.filter(«x|(x)» => «x !== id|id !== x») : [...f, id])"),
                A("Um ternário decide entre remover (filter) e adicionar (spread).", "f.includes(id) ? f.filter(...) : [...f, id]"),
                A("f.includes(id)", "Já é favorito?", "f.filter(x => x !== id)", "Array novo sem o id.",
                  "[...f, id]", "Array novo com o id."),
                "Os dois ramos geram arrays NOVOS. A atualizadora evita bugs com cliques rápidos seguidos."));

            list.Add(C(R, 2, "code", "Carrinho salvo",
                "Ao recarregar a página, o cliente perdia o carrinho. A Mari quer gravar no navegador a cada mudança.",
                "Escreva um useEffect que salva no localStorage, na chave \"carrinho\", o array itens convertido com JSON.stringify, rodando sempre que itens mudar.",
                ReactVar("useEffect(() => «{\n  localStorage.setItem(\"carrinho\", JSON.stringify(itens))\n}|localStorage.setItem(\"carrinho\", JSON.stringify(itens))», [itens])"),
                A("O localStorage só guarda texto: converta o array.", "localStorage.setItem(\"chave\", JSON.stringify(valor))"),
                A("localStorage.setItem(\"carrinho\", ...)", "Grava no navegador (sobrevive ao recarregar).",
                  "JSON.stringify(itens)", "Array → texto JSON.", "[itens]", "Salva toda vez que o carrinho mudar."),
                "Não guarde dados sensíveis (tokens, dados pessoais) no localStorage: qualquer script da página consegue ler."));

            list.Add(C(R, 2, "code", "Carrinho recuperado",
                "Agora o carrinho está salvo no localStorage, mas ao abrir a página ele começa vazio. Ler o localStorage em toda renderização seria desperdício.",
                "Crie o estado itens (com setItens) usando um inicializador preguiçoso: useState recebe uma arrow que devolve JSON.parse(localStorage.getItem(\"carrinho\")) || [].",
                new[] { "||", "??" }.Select(op => "const [itens, setItens] = useState(() => JSON.parse(localStorage.getItem(\"carrinho\")) " + op + " [])").ToArray(),
                A("Passar uma FUNÇÃO ao useState faz o React chamá-la só na primeira renderização.", "useState(() => JSON.parse(...) || [])"),
                A("useState(() => ...)", "Inicializador preguiçoso: roda uma vez só.",
                  "JSON.parse(localStorage.getItem(\"carrinho\"))", "Texto salvo → array (ou null se não existir).",
                  "|| []", "Sem nada salvo, começa vazio."),
                "useState(JSON.parse(...)) sem a arrow também funciona, mas executa a leitura a CADA renderização."));

            list.Add(C(R, 2, "cmd", "Auditoria para o CI",
                "O pipeline vai checar vulnerabilidades nas dependências, mas só deve falhar o build quando houver problemas graves.",
                "Rode a auditoria de segurança do npm definindo o nível mínimo para falhar como high (--audit-level).",
                A("npm audit --audit-level=high", "npm audit --audit-level high"),
                A("O npm tem um subcomando que procura vulnerabilidades conhecidas nas dependências.", "npm audit --audit-level=..."),
                A("npm audit", "Compara as versões instaladas com o banco de vulnerabilidades.",
                  "--audit-level=high", "Só termina com erro (exit code 1) para high e critical."),
                "npm audit fix tenta atualizar para versões corrigidas; revise e teste antes de usar o --force."));

            list.Add(C(R, 2, "code", "Value estável no Provider",
                "O TemaProvider passa value={{ tema, setTema }}. A cada renderização nasce um objeto novo, e todos os consumidores re-renderizam mesmo sem mudar o tema.",
                "Crie a constante valor com useMemo devolvendo o objeto { tema, setTema } (entre parênteses), recalculando só quando tema mudar.",
                ReactVar("const valor = useMemo(() => «({ tema, setTema })|{ return { tema, setTema } }», «[tema]|[tema, setTema]»)"),
                A("useMemo mantém a MESMA referência do objeto enquanto as dependências não mudam.", "useMemo(() => ({ tema, setTema }), [tema])"),
                A("() => ({ tema, setTema })", "Arrow que devolve o objeto (parênteses obrigatórios).",
                  "[tema]", "setTema é estável (o React garante), então basta tema."),
                "Depois: <TemaContext.Provider value={valor}>. Em contextos muito usados isso evita re-renders em cascata."));

            list.Add(C(R, 2, "code", "Hook com proteção",
                "Um colega usou useContext(CarrinhoContext) num componente fora do CarrinhoProvider e recebeu undefined — o erro só apareceu bem longe dali.",
                "Crie o hook useCarrinho: guarde em ctx o useContext(CarrinhoContext); se !ctx, lance new Error(\"useCarrinho fora do CarrinhoProvider\"); por fim retorne ctx.",
                ReactVar("«|export »function useCarrinho() {\n  CORPO\n}", "«|export »const useCarrinho = () => {\n  CORPO\n}")
                    .Select(s => s.Replace("CORPO", "const ctx = useContext(CarrinhoContext)\n  if (!ctx) «THROW|{ THROW }»\n  return ctx")
                                  .Replace("THROW", "throw new Error(\"useCarrinho fora do CarrinhoProvider\")"))
                    .SelectMany(s => ReactVar(s)).ToArray(),
                A("Sem Provider, useContext devolve o valor padrão do createContext() (undefined).", "if (!ctx) throw new Error(\"...\")"),
                A("useContext(CarrinhoContext)", "Lê o contexto.",
                  "if (!ctx) throw new Error(...)", "Falha cedo, com mensagem clara, no lugar certo.",
                  "return ctx", "Devolve o valor para quem chamou."),
                "Exporte só o Provider e o hook: ninguém precisa importar o contexto diretamente."));

            list.Add(C(R, 2, "code", "Reducer do carrinho",
                "O carrinho tem muitas ações (adicionar, remover, limpar). O Thiago centralizou tudo na função carrinhoReducer.",
                "Escreva duas linhas: crie [itens, dispatch] com useReducer(carrinhoReducer, []) e depois dispare a ação { type: \"limpar\" } com dispatch.",
                A("const [itens, dispatch] = useReducer(carrinhoReducer, [])\ndispatch({ type: \"limpar\" })"),
                A("useReducer recebe a função redutora e o estado inicial; devolve [estado, dispatch].", "dispatch({ type: \"...\" })"),
                A("useReducer(carrinhoReducer, [])", "O estado começa vazio; as mudanças passam pelo reducer.",
                  "dispatch({ type: \"limpar\" })", "Envia uma ação; o reducer devolve o novo estado."),
                "O reducer deve ser uma função pura: (estado, acao) => novoEstado, sem fetch nem mutação."));

            list.Add(C(R, 2, "code", "Busca com espera",
                "A busca de clientes chamava a API a cada tecla: digitar \"mariana\" gerava 7 requisições.",
                "Escreva um useEffect com dependência [termo]: guarde em const espera um setTimeout que chama buscar(termo) após 500 ms e retorne a limpeza com clearTimeout(espera).",
                ReactVar("useEffect(() => {\n  const espera = setTimeout(() => «buscar(termo)|{ buscar(termo) }», 500)\n  return () => «clearTimeout(espera)|{ clearTimeout(espera) }»\n}, [termo])"),
                A("Cada tecla muda termo; o cleanup do efeito anterior pode cancelar o timer pendente.", "const espera = setTimeout(...); return () => clearTimeout(espera)"),
                A("setTimeout(() => buscar(termo), 500)", "Agenda a busca para meio segundo depois.",
                  "return () => clearTimeout(espera)", "Se o termo mudar antes, o timer antigo é cancelado.",
                  "[termo]", "Recomeça a contagem a cada tecla."),
                "Esse padrão se chama debounce: só a última digitação, depois de uma pausa, dispara a busca."));

            list.Add(C(R, 2, "code", "Async dentro do efeito",
                "O Caio escreveu useEffect(async () => ...) e o React avisou que o efeito não pode devolver uma Promise.",
                "Num useEffect que roda só na montagem, declare a função async carregar com const resposta = await fetch(\"/api/clientes\") e setClientes(await resposta.json()); depois chame carregar().",
                new[] { "useEffect(() => {\n  async function carregar() {\n    CORPO\n  }\n  carregar()\n}, [])",
                        "useEffect(() => {\n  const carregar = async () => {\n    CORPO\n  }\n  carregar()\n}, [])" }
                    .Select(s => s.Replace("CORPO", "const resposta = await fetch(\"/api/clientes\")\n    setClientes(await resposta.json())")).ToArray(),
                A("A função do efeito não pode ser async; declare uma async DENTRO dela e chame.", "async function carregar() { ... } carregar()"),
                A("async function carregar()", "Função assíncrona interna.",
                  "await fetch(\"/api/clientes\")", "Espera a resposta HTTP.",
                  "setClientes(await resposta.json())", "Converte o corpo e guarda no estado.", "carregar()", "Dispara a busca."),
                "Uma função async sempre devolve uma Promise — e o efeito só pode retornar nada ou a função de limpeza."));

            list.Add(C(R, 2, "code", "Tecla Esc fecha o modal",
                "O modal de pedido deve fechar com a tecla Esc. Sem limpeza, cada abertura empilhava mais um ouvinte de teclado.",
                "Num useEffect que roda só na montagem, crie a função aoTeclar(e) que chama setAberto(false) se e.key === \"Escape\"; registre com window.addEventListener(\"keydown\", aoTeclar) e retorne a limpeza com window.removeEventListener.",
                ReactVar("useEffect(() => {\n  «const aoTeclar = «e|(e)» =>|function aoTeclar(e)» {\n    if (e.key === \"Escape\") «setAberto(false)|{ setAberto(false) }»\n  }\n  window.addEventListener(\"keydown\", aoTeclar)\n  return () => «window.removeEventListener(\"keydown\", aoTeclar)|{ window.removeEventListener(\"keydown\", aoTeclar) }»\n}, [])"),
                A("Para remover o ouvinte é preciso passar a MESMA função que foi registrada.", "window.addEventListener(\"keydown\", aoTeclar); return () => window.removeEventListener(\"keydown\", aoTeclar)"),
                A("e.key === \"Escape\"", "Nome da tecla Esc no evento de teclado.",
                  "addEventListener(\"keydown\", aoTeclar)", "Começa a ouvir o teclado.",
                  "return () => window.removeEventListener(...)", "Para de ouvir quando o modal desmonta."),
                "Uma arrow nova no removeEventListener não funcionaria: seria outra função. Por isso aoTeclar tem nome."));

            list.Add(C(R, 2, "code", "Cancelando a requisição",
                "Trocando rápido de pedido, a resposta lenta do pedido antigo chegava depois e sobrescrevia a do novo.",
                "Num useEffect com dependência [id]: crie const controle = new AbortController(); faça fetch(\"/api/pedidos/\" + id, { signal: controle.signal }) com .then(r => r.json()) e .then(setPedido); retorne () => controle.abort().",
                ReactVar("useEffect(() => {\n  const controle = new AbortController()\n  fetch(«\"/api/pedidos/\" + id|`/api/pedidos/${id}`», { signal: controle.signal })\n    .then(«r|(r)» => r.json())\n    .then(«setPedido|dados => setPedido(dados)|(dados) => setPedido(dados)»)\n  return () => «controle.abort()|{ controle.abort() }»\n}, [id])"),
                A("O AbortController cancela um fetch; o sinal vai nas opções do fetch.", "fetch(url, { signal: controle.signal }) e return () => controle.abort()"),
                A("new AbortController()", "Cria o controle de cancelamento.",
                  "{ signal: controle.signal }", "Liga o fetch a esse controle.",
                  "return () => controle.abort()", "Ao trocar de id (ou sair), a requisição antiga é cancelada."),
                "O fetch abortado rejeita com AbortError: num .catch, ignore esse erro (e.name === \"AbortError\") e trate os demais."));

            list.Add(C(R, 2, "code", "Fetch que falha de verdade",
                "A API do cardápio caiu e respondeu 500, mas a tela não mostrou erro nenhum: o fetch só rejeita em falha de rede, não em status HTTP de erro.",
                "Faça fetch(\"/api/cardapio\"); no primeiro .then (parâmetro r, corpo entre chaves), se !r.ok lance new Error(\"HTTP \" + r.status), senão retorne r.json(); depois .then(setCardapio) e .catch(() => setErro(true)).",
                ReactVar("fetch(\"/api/cardapio\")\n  .then(«r|(r)» => {\n    if (!r.ok) «THROW|{ THROW }»\n    return r.json()\n  })\n  .then(«setCardapio|dados => setCardapio(dados)|(dados) => setCardapio(dados)»)\n  .catch(() => «setErro(true)|{ setErro(true) }»)"
                    .Replace("THROW", "throw new Error(«\"HTTP \" + r.status|`HTTP ${r.status}`»)")),
                A("r.ok é true só para status 200–299. Se não for, lance um erro para cair no catch.", "if (!r.ok) throw new Error(\"HTTP \" + r.status); return r.json()"),
                A("if (!r.ok) throw ...", "Transforma 404/500 em erro de verdade.",
                  "return r.json()", "Só converte quando a resposta é boa.",
                  ".catch(() => setErro(true))", "Rede fora OU status de erro: a tela mostra a falha."),
                "Bibliotecas como axios já rejeitam em status de erro; com fetch puro, conferir r.ok é obrigação sua."));
        }
    }

    static partial class Docs
    {
        static partial void DocsExtraReact(List<DocEntry> all)
        {
            const string R = "React";

            // ── Projeto e npm
            all.Add(X(R, "Projeto e npm", "node -v / npm -v  (versões)", "Mostram as versões instaladas do Node.js e do npm.",
                "node -v\nnode --version\nnpm -v", null,
                A("npm -v", "versão do npm (ex.: 10.9.0)", "npx -v", "versão do npx"),
                "Ferramentas como o Vite exigem versões mínimas do Node. Gerenciadores como o nvm permitem ter várias."));
            all.Add(X(R, "Projeto e npm", "npm install -D  (devDependencies)", "Instala um pacote usado só durante o desenvolvimento (testes, lint, formatação).",
                "npm install --save-dev PACOTE\nnpm install -D PACOTE",
                A("--save-dev / -D", "grava em devDependencies", "(sem opção)", "grava em dependencies"),
                A("npm install -D eslint", "linter só para desenvolvimento", "npm i --save-dev vitest", "ferramenta de testes"),
                "devDependencies não são necessárias para o site rodar no navegador depois do build."));
            all.Add(X(R, "Projeto e npm", "npm uninstall  (remover pacote)", "Remove o pacote de node_modules e do package.json.",
                "npm uninstall PACOTE", A("remove, rm, un, r", "apelidos do mesmo comando"),
                A("npm uninstall lodash", "remove o lodash", "npm rm axios", "mesma coisa, forma curta"),
                "Confira antes se o código ainda importa o pacote."));
            all.Add(X(R, "Projeto e npm", "npm install PACOTE@VERSÃO", "Instala uma versão específica (ou a mais nova de uma versão principal).",
                "npm install PACOTE@VERSAO",
                A("@latest", "a mais recente", "@5", "a mais nova da linha 5.x", "@5.2.1", "exatamente essa versão"),
                A("npm install vue@3", "Vue na versão principal 3", "npm i axios@1.7.2", "versão exata"),
                "Trocar de versão principal costuma ter mudanças incompatíveis: leia o guia de migração."));
            all.Add(X(R, "Projeto e npm", "npm run preview", "Serve localmente a pasta dist/ gerada pelo build (projetos Vite).",
                "npm run build\nnpm run preview", null,
                A("npm run preview", "abre o site já compilado, como em produção"),
                "Não recompila nada e não serve para hospedar o site de verdade."));
            all.Add(X(R, "Projeto e npm", "npm run SCRIPT -- opções", "Tudo o que vem depois de -- é repassado ao comando do script.",
                "npm run NOME -- --opcao valor", null,
                A("npm run build -- --mode homologacao", "repassa --mode ao vite build", "npm run test -- --watch", "repassa --watch ao executor de testes",
                  "npx vite --host", "chama o vite diretamente com npx"),
                "Sem o --, o npm tenta interpretar a opção como dele próprio."));
            all.Add(X(R, "Projeto e npm", "npm outdated", "Lista dependências com versões mais novas disponíveis.",
                "npm outdated",
                A("Current", "versão instalada", "Wanted", "maior versão permitida pelo package.json (^, ~)", "Latest", "versão mais nova publicada"),
                A("npm outdated", "tabela com pacotes desatualizados", "npm update", "atualiza até a versão Wanted"), null));
            all.Add(X(R, "Projeto e npm", "npm ci  (instalação limpa)", "Instalação exata e reprodutível a partir do package-lock.json, ideal para CI.",
                "npm ci", null,
                A("npm ci", "apaga node_modules e instala exatamente o que está no lock"),
                "Falha se package.json e package-lock.json não baterem. Faça commit do lock."));
            all.Add(X(R, "Projeto e npm", "npm ls  (árvore de dependências)", "Mostra os pacotes instalados e quem depende de quem.",
                "npm ls\nnpm ls PACOTE", null,
                A("npm ls vue", "todas as cópias do vue na árvore e quem as trouxe", "npm ls --depth=0", "só as dependências diretas"),
                "Útil para achar pacotes duplicados em versões diferentes."));
            all.Add(X(R, "Projeto e npm", "npm audit", "Procura vulnerabilidades conhecidas nas dependências instaladas.",
                "npm audit [--audit-level=NIVEL]\nnpm audit fix",
                A("--audit-level", "nível mínimo que faz o comando falhar: low, moderate, high, critical", "fix", "atualiza para versões corrigidas compatíveis"),
                A("npm audit --audit-level=critical", "falha só com vulnerabilidades críticas", "npm audit --omit=dev", "ignora devDependencies"),
                "Em CI, o código de saída diferente de 0 faz o pipeline parar."));
            all.Add(X(R, "Projeto e npm", "import.meta.env  (variáveis de ambiente no Vite)", "Lê variáveis definidas em arquivos .env. Só as com prefixo VITE_ chegam ao código do navegador.",
                "import.meta.env.VITE_NOME",
                A(".env", "valores padrão", ".env.production", "usado no build de produção", "import.meta.env.MODE", "modo atual (development, production...)"),
                A("const chaveMapa = import.meta.env.VITE_MAPA_PUBLICO", "lê VITE_MAPA_PUBLICO do .env", "VITE_SITE_NOME=Biblioteca", "linha no arquivo .env"),
                "Tudo vai para o JavaScript público: nunca coloque segredos aí. Em Create React App era process.env.REACT_APP_..."));

            // ── JSX
            all.Add(X(R, "JSX", "Fragmentos  <> </>", "Agrupam vários elementos sem criar uma tag extra no HTML.",
                "<>\n  <Elemento1 />\n  <Elemento2 />\n</>", null,
                A("<>\n  <dt>Autor</dt>\n  <dd>Machado</dd>\n</>", "dois elementos irmãos", "<Fragment key={l.id}>...</Fragment>", "forma longa, aceita key"),
                "Forma longa: import { Fragment } from \"react\" (ou React.Fragment)."));
            all.Add(X(R, "JSX", "Atributos com expressão  attr={valor}", "Qualquer atributo pode receber uma expressão JavaScript no lugar do texto entre aspas.",
                "<tag atributo={expressao} />", null,
                A("<img src={capa} alt={titulo} />", "imagem vinda de variáveis", "<input readOnly={bloqueado} />", "atributo booleano vindo do estado",
                  "<button disabled={!valido}>Salvar</button>", "desabilita quando não é válido"),
                "Com aspas é texto literal: src=\"capa\" é a palavra capa."));
            all.Add(X(R, "JSX", "style={{ }}  (estilo inline)", "style recebe um objeto JavaScript com propriedades CSS em camelCase.",
                "<tag style={{ propriedade: \"valor\" }} />", null,
                A("<p style={{ fontSize: \"18px\" }}>Aviso</p>", "font-size vira fontSize", "<div style={{ backgroundColor: cor }}></div>", "valor vindo de variável"),
                "Números viram px automaticamente: { width: 200 } = 200px."));
            all.Add(X(R, "JSX", "Comentários no JSX", "Dentro do JSX, comentários vão numa expressão com /* */.",
                "{/* comentário */}", null,
                A("{/* menu lateral desativado */}", "comentário que não aparece na tela"),
                "Fora do JSX, // e /* */ funcionam normalmente."));
            all.Add(X(R, "JSX", "className condicional", "A classe pode ser calculada com um ternário ou concatenação.",
                "className={condicao ? \"classeA\" : \"classeB\"}", null,
                A("<li className={lido ? \"lido\" : \"novo\"}>{titulo}</li>", "classe conforme o estado",
                  "className={\"cartao \" + (destaque ? \"destaque\" : \"\")}", "classe fixa + condicional"), null));
            all.Add(X(R, "JSX", "Armadilha do 0 com &&", "a && b devolve a quando a é falso. Se a for 0, o React mostra o 0 na tela.",
                "{lista.length > 0 && <Componente />}", null,
                A("{alunos.length > 0 && <Tabela />}", "condição booleana: nada aparece se vazio", "{!!alunos.length && <Tabela />}", "!! também converte para booleano"),
                "O React ignora false, null e undefined, mas mostra números e textos."));
            all.Add(X(R, "JSX", "Retorno antecipado", "Um componente pode sair cedo com um return dentro de if.",
                "if (condicao) return <Algo />", null,
                A("if (!aluno) return <p>Buscando aluno...</p>", "enquanto os dados não chegam", "if (erro) return <Erro mensagem={erro} />", "tela de erro"),
                "Todos os hooks devem ser chamados ANTES do primeiro return."));
            all.Add(X(R, "JSX", "dangerouslySetInnerHTML  (cuidado: XSS)", "Insere HTML cru no elemento. Texto entre chaves é sempre escapado; este atributo não.",
                "<div dangerouslySetInnerHTML={{ __html: htmlConfiavel }} />", null,
                A("<p>{mensagem}</p>", "SEGURO: <b> vira texto", "<div dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(html) }} />", "só com HTML sanitizado"),
                "Nunca use com conteúdo digitado por usuários sem sanitizar: abre brecha para XSS."));

            // ── Componentes
            all.Add(X(R, "Componentes", "Prop booleana (forma curta)", "Escrever só o nome da prop equivale a passar true.",
                "<Componente prop />   (= prop={true})", null,
                A("<Campo obrigatorio />", "obrigatorio vale true", "<Campo obrigatorio={false} />", "false precisa de chaves"),
                "prop=\"false\" é texto (e texto não vazio é verdadeiro)."));
            all.Add(X(R, "Componentes", "lazy  (carregamento sob demanda)", "Carrega o código de um componente só quando ele for renderizado pela primeira vez.",
                "const Comp = lazy(() => import(\"./Comp\"))", null,
                A("const Mapa = lazy(() => import(\"./Mapa\"))", "Mapa vira um arquivo JS separado",
                  "import { lazy, Suspense } from \"react\"", "importações necessárias"),
                "O módulo importado precisa ter export default. Use um <Suspense> acima."));
            all.Add(X(R, "Componentes", "Suspense", "Mostra um conteúdo provisório (fallback) enquanto um componente lazy carrega.",
                "<Suspense fallback={<Provisorio />}>\n  <ComponenteLazy />\n</Suspense>", null,
                A("<Suspense fallback={<span>Abrindo mapa...</span>}>\n  <Mapa />\n</Suspense>", "texto enquanto baixa o código"),
                "Um Suspense em volta de <Routes> cobre todas as páginas lazy."));

            // ── Formulários e eventos
            all.Add(X(R, "Formulários e eventos", "onClick={funcao}  vs  onClick={funcao()}", "Passe a função (sem parênteses). Com parênteses ela é executada na renderização.",
                "onClick={funcao}\nonClick={() => funcao(arg)}", null,
                A("<button onClick={salvarNota}>Salvar</button>", "entrega a função", "<button onClick={() => salvarNota(7)}>Salvar</button>", "com argumento: arrow"),
                "onClick={salvarNota()} chama a função na hora e entrega o RESULTADO dela ao onClick."));
            all.Add(X(R, "Formulários e eventos", "onSubmit e preventDefault", "onSubmit no <form> roda ao enviar; e.preventDefault() impede o recarregamento da página.",
                "<form onSubmit={enviar}>...</form>\nfunction enviar(e) {\n  e.preventDefault()\n  // ...\n}", null,
                A("function cadastrar(ev) {\n  ev.preventDefault()\n  gravarAluno()\n}", "handler do envio",
                  "<form onSubmit={cadastrar}><button>Cadastrar</button></form>", "botão dentro do form envia (e Enter também)"),
                "Sem preventDefault o navegador recarrega a página e o estado do React se perde."));
            all.Add(X(R, "Formulários e eventos", "Checkbox controlado  (checked)", "Em caixas de seleção, o estado vai em checked e o novo valor vem de e.target.checked.",
                "<input type=\"checkbox\" checked={estado} onChange={e => setEstado(e.target.checked)} />", null,
                A("<input type=\"checkbox\" checked={presente} onChange={ev => setPresente(ev.target.checked)} />", "lista de presença"),
                "e.target.value de checkbox é \"on\": use checked."));

            // ── Estado e imutabilidade
            all.Add(X(R, "Estado e imutabilidade", "Adicionar em array  [...lista, novo]", "Cria um array novo com os itens antigos e o novo.",
                "setLista(l => [...l, novo])\nsetLista(l => [novo, ...l])", null,
                A("setLivros(ls => [...ls, livroNovo])", "adiciona no fim", "setAvisos(as => [aviso, ...as])", "adiciona no início"),
                "push altera o array do estado e o React não percebe a mudança."));
            all.Add(X(R, "Estado e imutabilidade", "reduce  (somar uma lista)", "Percorre o array acumulando um único resultado.",
                "lista.reduce((acumulador, item) => novoAcumulador, valorInicial)", null,
                A("notas.reduce((s, n) => s + n, 0)", "soma de números", "livros.reduce((t, l) => t + l.paginas, 0)", "soma de uma propriedade"),
                "Sempre informe o valor inicial: com array vazio e sem ele, o reduce lança erro."));
            all.Add(X(R, "Estado e imutabilidade", "Ordenar sem mutar", "sort altera o array original. Copie antes ou use toSorted.",
                "[...lista].sort((a, b) => a.campo - b.campo)\nlista.toSorted((a, b) => ...)", null,
                A("[...alunos].sort((a, b) => b.nota - a.nota)", "maior nota primeiro", "nomes.toSorted((a, b) => a.localeCompare(b))", "ordem alfabética"),
                "Props e estado são somente leitura: nunca chame sort direto neles."));
            all.Add(X(R, "Estado e imutabilidade", "Spread aninhado", "O spread copia só um nível; cada objeto interno que muda também precisa ser copiado.",
                "{ ...obj, interno: { ...obj.interno, campo: valor } }", null,
                A("setAluno(a => ({ ...a, endereco: { ...a.endereco, cidade: \"Recife\" } }))", "troca só a cidade"),
                "Esquecer o spread interno apaga os outros campos do objeto de dentro."));
            all.Add(X(R, "Estado e imutabilidade", "Alternar item num array", "includes + filter (remover) ou spread (adicionar), num ternário.",
                "l.includes(x) ? l.filter(y => y !== x) : [...l, x]", null,
                A("setMarcadas(m => m.includes(dia) ? m.filter(d => d !== dia) : [...m, dia])", "marca/desmarca um dia"), null));
            all.Add(X(R, "Estado e imutabilidade", "Estado derivado  (não é estado)", "Se dá para calcular a partir de props ou estado, calcule na renderização.",
                "const derivado = calculo(estado1, estado2)", null,
                A("const media = (n1 + n2) / 2", "sem useState nem useEffect", "const titulo = `${serie} - ${turma}`", "texto montado na hora"),
                "useState + useEffect para 'sincronizar' gera renderização dupla e dados fora de sincronia."));
            all.Add(X(R, "Estado e imutabilidade", "localStorage + JSON", "Guarda texto no navegador; objetos e arrays precisam de JSON.stringify / JSON.parse.",
                "localStorage.setItem(\"chave\", JSON.stringify(valor))\nJSON.parse(localStorage.getItem(\"chave\"))", null,
                A("localStorage.setItem(\"preferencias\", JSON.stringify(prefs))", "salva", "JSON.parse(localStorage.getItem(\"preferencias\")) || {}", "lê (com padrão)"),
                "getItem devolve null se a chave não existe. Não guarde dados sensíveis."));

            // ── Hooks
            all.Add(X(R, "Hooks", "useState com inicializador  useState(() => ...)", "Passe uma função para calcular o valor inicial só na primeira renderização.",
                "const [x, setX] = useState(() => calculoInicial())", null,
                A("const [prefs, setPrefs] = useState(() => JSON.parse(localStorage.getItem(\"prefs\")) || {})", "lê o navegador uma vez só"),
                "useState(calculo()) executa o cálculo a cada renderização (e descarta o resultado)."));
            all.Add(X(R, "Hooks", "useReducer", "Alternativa ao useState para estados com muitas ações: a lógica fica numa função redutora.",
                "const [estado, dispatch] = useReducer(reducer, inicial)\ndispatch({ type: \"acao\" })", null,
                A("function notasReducer(estado, acao) {\n  if (acao.type === \"zerar\") return []\n  return estado\n}", "reducer: (estado, ação) => novo estado",
                  "const [notas, despachar] = useReducer(notasReducer, [])", "cria o estado", "despachar({ type: \"zerar\" })", "envia uma ação"),
                "O reducer deve ser puro: sem fetch, sem mutar o estado recebido."));
            all.Add(X(R, "Hooks", "async/await no useEffect", "A função do efeito não pode ser async. Declare uma async dentro dele e chame-a.",
                "useEffect(() => {\n  async function carregar() { ... await ... }\n  carregar()\n}, [deps])", null,
                A("useEffect(() => {\n  async function buscarTurmas() {\n    const resp = await fetch(\"/api/turmas\")\n    setTurmas(await resp.json())\n  }\n  buscarTurmas()\n}, [])", "busca com await"),
                "useEffect(async () => ...) devolve uma Promise, e o efeito só pode devolver a função de limpeza."));
            all.Add(X(R, "Hooks", "Debounce com setTimeout", "Espera o usuário parar de digitar antes de agir; o cleanup cancela o timer anterior.",
                "useEffect(() => {\n  const t = setTimeout(() => acao(valor), MS)\n  return () => clearTimeout(t)\n}, [valor])", null,
                A("useEffect(() => {\n  const t = setTimeout(() => salvarRascunho(texto), 1000)\n  return () => clearTimeout(t)\n}, [texto])", "salva 1 s depois da última tecla"), null));
            all.Add(X(R, "Hooks", "AbortController  (cancelar fetch)", "Cancela uma requisição em andamento — ideal no cleanup do efeito.",
                "const c = new AbortController()\nfetch(url, { signal: c.signal })\nc.abort()", null,
                A("useEffect(() => {\n  const c = new AbortController()\n  fetch(\"/api/livros/\" + isbn, { signal: c.signal }).then(r => r.json()).then(setLivro)\n  return () => c.abort()\n}, [isbn])", "cancela a busca antiga ao trocar de livro"),
                "O fetch cancelado rejeita com AbortError; ignore esse erro no .catch."));
            all.Add(X(R, "Hooks", "fetch: conferindo r.ok", "fetch só rejeita em falha de rede. Respostas 404/500 precisam ser verificadas com r.ok.",
                "fetch(url)\n  .then(r => {\n    if (!r.ok) throw new Error(\"HTTP \" + r.status)\n    return r.json()\n  })\n  .catch(erro => ...)", null,
                A("fetch(\"/api/boletim\").then(r => { if (!r.ok) throw new Error(r.statusText); return r.json() })", "erro HTTP vira exceção",
                  ".catch(() => setFalhou(true))", "mostra a falha na tela"),
                "r.ok é true para status 200 a 299."));
            all.Add(X(R, "Hooks", "Hook de contexto com verificação", "Custom hook que lê o contexto e falha com mensagem clara se não houver Provider.",
                "function useAlgo() {\n  const ctx = useContext(AlgoContext)\n  if (!ctx) throw new Error(\"mensagem\")\n  return ctx\n}", null,
                A("function useSessao() {\n  const s = useContext(SessaoContext)\n  if (!s) throw new Error(\"useSessao precisa do SessaoProvider\")\n  return s\n}", "erro claro no lugar certo"),
                "Sem Provider, useContext devolve o valor padrão do createContext (undefined se nada foi passado)."));

            // ── React Router
            all.Add(X(R, "React Router", "BrowserRouter, Routes e Route", "Ligam caminhos da URL a componentes. Trechos com : são parâmetros.",
                "<BrowserRouter>\n  <Routes>\n    <Route path=\"/caminho\" element={<Componente />} />\n  </Routes>\n</BrowserRouter>",
                A("path", "endereço (\"/\", \"/alunos\", \"/alunos/:matricula\")", "element", "JSX mostrado nesse endereço", "path=\"*\"", "qualquer outro endereço (página 404)"),
                A("<Route path=\"/biblioteca\" element={<Biblioteca />} />", "rota fixa", "<Route path=\"/livros/:isbn\" element={<Livro />} />", "rota com parâmetro"),
                "Pacote: react-router-dom (import { BrowserRouter, Routes, Route } from \"react-router-dom\")."));
            all.Add(X(R, "React Router", "Link", "Link interno que troca de página sem recarregar.",
                "<Link to=\"/caminho\">Texto</Link>\n<Link to={\"/caminho/\" + id}>Texto</Link>", null,
                A("<Link to=\"/sobre\">Quem somos</Link>", "destino fixo", "<Link to={`/livros/${l.isbn}`}>{l.titulo}</Link>", "destino com variável"),
                "<a href> recarrega o app inteiro: use só para sites externos."));
            all.Add(X(R, "React Router", "NavLink", "Link que sabe se aponta para a página atual (isActive).",
                "<NavLink to=\"/x\" className={({ isActive }) => isActive ? \"classe\" : \"\"}>Texto</NavLink>", null,
                A("<NavLink to=\"/agenda\" className={({ isActive }) => isActive ? \"selecionado\" : \"\"}>Agenda</NavLink>", "destaca a página atual",
                  "<NavLink to=\"/\" end>Início</NavLink>", "end: \"/\" só fica ativo na raiz"), null));
            all.Add(X(R, "React Router", "useParams", "Lê os parâmetros (:nome) da rota atual.",
                "const { parametro } = useParams()", null,
                A("const { isbn } = useParams()", "para a rota /livros/:isbn"),
                "Os valores chegam como texto: converta com Number(...) se precisar."));
            all.Add(X(R, "React Router", "useNavigate", "Devolve uma função para trocar de página por código (depois de salvar, pagar etc.).",
                "const navigate = useNavigate()\nnavigate(\"/caminho\")\nnavigate(numero)",
                A("navigate(\"/x\")", "vai para /x", "navigate(\"/x\", { replace: true })", "substitui a entrada do histórico", "navigate(número)", "anda no histórico (negativo volta, positivo avança)"),
                A("const ir = useNavigate()\nconst concluir = () => ir(\"/boletim\")", "navega depois de uma ação"),
                "Chame navigate em eventos ou efeitos, não durante a renderização."));
            all.Add(X(R, "React Router", "Outlet  (rotas aninhadas)", "Marca onde a rota filha aparece dentro do layout da rota pai.",
                "<Route element={<Layout />}>\n  <Route path=\"/a\" element={<A />} />\n</Route>\n\nfunction Layout() { return <><Topo /><Outlet /></> }", null,
                A("function Moldura() {\n  return <><Cabecalho /><Outlet /><Rodape /></>\n}", "cabeçalho e rodapé em todas as páginas filhas"), null));
            all.Add(X(R, "React Router", "Navigate  (redirecionar)", "Componente que redireciona assim que é renderizado.",
                "<Navigate to=\"/caminho\" replace />", A("replace", "não deixa a página atual no histórico"),
                A("return autorizado ? children : <Navigate to=\"/entrar\" replace />", "rota protegida"),
                "Proteger rota no front não substitui a verificação de acesso na API."));
            all.Add(X(R, "React Router", "useSearchParams  (query string)", "Lê e altera os parâmetros depois do ? na URL.",
                "const [params, setParams] = useSearchParams()\nparams.get(\"chave\")", null,
                A("const [params] = useSearchParams()\nconst pagina = params.get(\"pagina\")", "lê ?pagina=2", "setParams({ ordem: \"nome\" })", "muda a URL para ?ordem=nome"),
                "get devolve null se o parâmetro não existir."));
        }
    }
}
