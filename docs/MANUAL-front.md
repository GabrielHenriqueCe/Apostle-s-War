# Manual do front — a ponte, o contrato e as camadas

> **Tipo:** referência viva da pele (WebView2 + `wwwroot/`).
> **Função:** como o C# e o JS conversam, o que a porta de tela promete, e onde cada decisão mora.
> **Como usar:** ler antes de tocar na ponte, nas portas de apresentação ou em regra que a tela
>   parece "saber". O mapa das pastas de `wwwroot/` e o contrato de tela estão no `CLAUDE.md`.
> **Origem:** o §FRONT e o §CADA DECISÃO NA SUA CAMADA do `ROADMAP-refatoracao.md`.

---

## O mapa de `wwwroot/`, o contrato de tela e as duas injeções

```
wwwroot/
  jogo.js            COMPOSITION ROOT (192 linhas). Imports, a tabela TELAS, o interpretador,
                     o Esc/botão-sair, o registro CENARIOS e o boot. NÃO desenha nada.
  index.html · estilo.css
  nucleo/   ponte.js (o único que sabe do C#) · cena.js (quem está na tela + abrirTela)
            ar.js (canvas, maestro, laço, aplicarTema)
  ui/       13 módulos reusados entre telas: modal.js · time.js (picker+slots+arrastar) ·
            animacao.js · ficha.js · peca.js · alma.js · po.js · navegador.js · …
  telas/    menu · perfil · arena · campanha · catedral · forja · compendio · combate
  cenarios/ comum/ + 8 facções (cada pasta: <faccao>.js + <faccao>.css)
```

**O paralelo com o back, e ele é do Gabriel:** `jogo.js` = `Program.cs` · `nucleo/` =
`CombateService` · `telas/` = as habilidades (dado: carga → pinta) · `ui/` = `Acao` compartilhada.

**O CONTRATO DE TELA.** Toda tela é isto, e nada além:
```js
export const compendio = { cena: 'compendio', montar(dados, anterior) { /* preenche o DOM */ } };
```
A chave no mapa `TELAS` é o **`tipo` da mensagem** — a unidade é a MENSAGEM, não o arquivo (o
compêndio exporta duas). Tela nova = uma linha na tabela. **Abrir tela é SEMPRE `abrirTela(...)`**,
inclusive de dentro do código (a ficha do apóstolo pela conquista usa outra `cena` que a do compêndio,
porque o Esc tem de voltar pra lugares diferentes).
**`estado` e `evento` ficam FORA da tabela de propósito** — não navegam, atualizam a cena no ar.

**AS DUAS INJEÇÕES seguram a direção da dependência.** O núcleo não pode importar quem depende dele,
então o composition root ENTREGA: `registrarCenarios(CENARIOS)` e `aoTrocarCena(atualizarBotaoSair)`.
Regra: **quando um módulo interno precisa de algo do externo, o externo INJETA ou a coisa DESCE.
Nunca o interno importa pra cima.** Estado que cruza fronteira vira ACESSADOR (`cenaAgora()`,
`estadoAtual()`, `menuEhRaiz()`, `avatarDoJogador()`, `fimDeFaseTemOpcoes()`) — `export let` é lido
ao vivo mas NÃO é gravável de fora.

**Verificar SEMPRE que tocar no front** (os dois passam em ~1 min):
```
node --experimental-vm-modules ferramentas/rodar-telas.js   # as 13 mensagens montam
node --experimental-vm-modules ferramentas/rodar-tema.js "" 120   # 8 temas x 120s de cena
```
Mais `ferramentas/medir-donos.js` (grafo de donos, com `--porque <tema> <funcao>`) quando for mover
função de cenário.

**O QUE OS HARNESSES NÃO COBREM — e verde deles não é "o jogo funciona":** eles publicam mensagem,
**não clicam em nada.** Duplo-clique, clique em slot, arrastar, teclado e tudo que roda DURANTE a
batalha em resposta a isso estão fora. Na separação, QUATRO bugs saíram exatamente daí e os quatro
foram achados pelo Gabriel jogando. **Conferência em jogo continua sendo dele, sempre.**

**Terminação de linha:** o `.gitattributes` IMPEDE (LF no repo, CRLF na cópia de trabalho) e o
`rodar-telas.js` ACUSA se algo escapar. Arquivo misto não é cosmético — já grudou um `else if` num
comentário e virou código comentado.

**A armadilha do CSS por tema:** a escada de `@media` que encolhe os ladrilhos tem de vir DEPOIS do
bloco base do tema. `ferramentas/separar-css.js` prova as duas coisas (empate de especificidade e
contagem de regras) antes de escrever — CSS pode ficar válido e semanticamente MORTO.

## O método — NÃO é MVC nem REST

É uma **ponte de mensagens LOCAL, in-process, orientada a eventos.** O JS (tela) e o C# (motor) rodam
no **MESMO processo** (o `.exe`) e trocam mensagens **direto pela webview** — sem HTTP, sem servidor,
sem rede, offline. Analogia: não é "cliente e servidor pela internet", é "duas partes do mesmo programa
passando bilhetes" (padrão de app desktop com webview — VS Code / Discord / Spotify desktop por dentro).
- **REST/servidor foi DESCARTADO** (= FILA C): precisa hospedar, não é grátis/offline.
- **Encaixa nos seams já prontos:** `IEntrada` recebe o clique-do-JS → `Comando`; `IApresentacao`
  empurra o estado/eventos pro JS desenhar. O motor C# **não sabe** que é webview.
- **MVC "de leve" (só a separação natural):** View = HTML/CSS/JS; Model = domínio C#; cola = o host
  webview. NÃO é o framework ASP.NET MVC. Existe um "protocolo" (formato dos bilhetes, provavelmente
  JSON), mas é um **contrato interno do app**, não uma API.

---

## A ferramenta — WebView2, e o preço dele

O **HTML/CSS/JS mora NESTE repo** (sobe pro GitHub junto — não é site/serviço à parte).
Distribuição: `dotnet publish` self-contained → **`.exe` no GitHub Releases**. C# roda NATIVO (não
WASM) → zero carregamento.

**Photino descartado por evidência, não por preferência.** Ele sobe em `net10.0` sem problema
(o NuGet resolve `net8.0` como compatível; os binários nativos são por RID) — mas a **janela abre
PRETA**: o título é setado, o processo não crasha, e nada é renderizado. Reproduzido com
`LoadRawString` (HTML embutido, sem arquivo) E com `Load` de arquivo, com pasta de dados explícita,
na última versão existente (Photino.NET 4.0.16 + Native 4.0.22). O WebView2 Runtime da máquina está
instalado e íntegro (150.0.4078.83) — o **WebView2 direto renderizou de primeira** no mesmo runtime.
Suspeita: shim nativo do Photino velho demais pro runtime atual. **Não vale investigar mais fundo:
o Photino não expõe `CoreWebView2InitializationCompleted`/`NavigationCompleted`, então não dá pra
instrumentar a falha — e essa opacidade é, por si só, o argumento contra ele.**

**Preço do WebView2:** exige host WinForms → TFM `net10.0-windows`. Resolvido SEM contaminar o motor:
o projeto foi partido em **`ApostlesWar` (biblioteca, `net10.0` puro — motor + views de console)** e
**`App/ApostlesWar.App` (Exe, `net10.0-windows` — composition root + front)**. Os Tests seguem
`net10.0` intocados. A separação de camadas que as portas já faziam no código agora aparece também na
estrutura de projetos. *(Gotcha registrado: o pacote do WebView2 referencia as variantes WinForms E
WPF sem condição — `build/Common.targets:133` — e a WPF arrasta outro `WindowsBase`, gerando MSB3277;
removida num Target no csproj do App, já que é referência morta aqui.)*

---

## O contrato da tela é GATILHO, não desenho

É isto que faz emoji → sprite ser troca só de front, e é o que não pode ser "melhorado" sem
quebrar o porte.

  - **Descoberta que mudou o desenho:** `IApresentacao` **NÃO era** porta de render — só encapsula a
    ESPERA (`AguardarAnimacao`). O render era `Console.WriteLine` dentro da `CombateView`, concreta
    dentro do `CombateService`. A porta de verdade nasceu agora: **`View/ITelaDeCombate`**.
  - **O contrato é GATILHO, não desenho.** Os nomes são imperativos por herança do console
    (`Exibir...`), mas a impl web traduz cada chamada em (a) um RETRATO do estado serializado ou
    (b) um EVENTO pra animar. A tela se redesenha do estado; nunca recebe ordem de desenho. É isso
    que faz emoji→sprite ser troca só de front.
  - **O laço síncrono foi PRESERVADO INTEIRO** — nada virou async. A UI fica na thread principal
    (`Application.Run`) e o jogo numa thread de fundo; `IEntrada.Ler()` bloqueia num
    `BlockingCollection.Take()` que o clique do JS alimenta. Foi a `IEntrada` bloqueante (que parecia
    um problema pro porte) que salvou o motor de qualquer cirurgia.
  - **Menu de ação e de alvo ficaram FORA da porta:** são navegação por CURSOR, formato do console.
    Quem decide ação/alvo é o `IControladorDeTurno`, e o front tem o seu (`ControladorJogadorWeb`,
    clique-na-habilidade → clique-no-alvo). Botá-los na porta obrigaria a impl web a carregar
    método morto.
  - **Carona ainda em aberto (do #14):** com a apresentação agora injetável, o **teste da ordem
    crítica de morte** (Sentença antes de Necromancia) virou possível com uma tela no-op. Não feito
    neste PR (1 PR, 1 tema) — é o próximo candidato.

---

## Visual — emoji é PLACEHOLDER, não o teto

v1 = emojis + CSS (dano pulando) pra o loop andar rápido sem depender de arte. **Teto real = sprites
pixel ANIMADOS** (sprite sheets + CSS `steps()` ou `<canvas>`; `image-rendering: pixelated` mantém o
pixel nítido; arte do **Pixelab** pluga direto). O motor só EMITE eventos (o stream `EventoCombate`/
`EventoDano` = o gancho de animação); o front decide o quão rico renderiza. **Emoji → sprite = troca de
render no JS, SEM tocar no motor.** Sem susto de performance (é por turnos, não ação 60fps).

---

## Cada decisão na sua camada (#180)

Com o console fora, uma auditoria do front achou 7 decisões na camada errada. A lição geral: **o
vazamento não é teórico — ele APODRECE.** A prova foi o nome do slot do arsenal, duplicado no front
e no `ArsenalService`, que já tinha divergido (a tela dizia "Acessório", o item que cai nela nasce
"Manopla"); a tela mostrava um nome que o item não tem.

**Front decidindo regra → devolvido ao service:**
- `ArsenalService.NomeDoSlot(fase)` — nomeia o slot E o item que cai nele, uma tabela só (a tela
  precisa nomear slot VAZIO, que é por isso que ela tinha a cópia). Teste trava os dois juntos.
- `CapitulosService.FaccoesDaCampanha()` — o mapa É a lista de capítulos, na ordem. O front deduzia
  ("todas as facções menos Humanos") e acertava por coincidência da ordem do enum.
- `CampanhaService.PosicaoNoMapa()`/`SalvarPosicao()` — o "último lugar" é PROGRESSÃO. O front
  gravava direto na porta de save (única gravação do jogo fora de um service), enquanto o
  `PerfilService` já apagava a mesma chave no wipe de conta: dois donos.
- `ArsenalService.EquiparItem` **persiste sozinho** — quando havia duas cascas, cada uma escolheu
  sua política de quando salvar. Quem manda no dado decide quando ele é durável.
- `PerfilService.AvatarInicial()`/`PodeUsarAvatar()` — a cara do jogador é troféu de campanha. O
  front segue validando o clique, mas como FRONTEIRA, não como fonte da regra.

**Motor decidindo pele → devolvido à tela:**
- `Item.ValorFormatado()`/`NomeStat()` **saíram do Domain**: `:F0` e sufixo `%` são exibição. O Item
  guarda `Valor` + `TipoStat`; cada pele escreve do seu jeito.
- **`IApresentacao.AguardarAnimacao(Momento)`** no lugar de `(int ms)`. O motor mandava `1500` em 10
  lugares e a pele *dividia* o número pra corrigir — sintoma de quem não devia escolher, escolhendo.
  Agora o motor diz a BATIDA (`Tick`/`Narracao`/`Golpe`/`Preparacao`) e a pele dá a duração (todas em
  1500 hoje, de propósito: mudou o dono, não o sentimento). **É o seam do MODO AUTOMÁTICO** — uma
  pele que devolve ~0 e o motor não sabe de nada.

**NÃO mexido, porque está certo:** a validação duplicada (front valida pra UX, back valida porque não
confia na tela) e a `SessaoDoFront` inteira (ids, lado esquerdo/direito, o `_mostrado` que segura a
barra de vida até o número ser narrado) — presentation pura, no lugar certo.

**Dívida registrada, não paga — emoji no Domain.** `Personagem.Simbolo`, `Habilidade.Simbolo`,
`Item.Simbolo` e `Faccoes.simbolos` são RENDER dentro do domínio de regras. Fica como está: pagar
isso hoje custa tocar os 36 apóstolos e todas as skills por uma dor que só chega **quando os sprites
entrarem** — aí o emoji deixa de ser "o visual" e vira "um dos visuais", e o Domain vira o lugar
errado pra ele. Gatilho nomeado, sem data.
