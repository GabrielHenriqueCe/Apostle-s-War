# Apostle's War — guia pro Claude

RPG por turnos em C# (`net10.0`, `OutputType Exe`). Roda numa **janela webview** (WebView2), jogável de
ponta a ponta: menu, perfil, campanha, arena, arsenal. Projeto de estudo/portfólio do Gabriel.

## Orientação — faça no início da sessão e quando o Gabriel perguntar "onde estamos"
Não precisa o Gabriel pedir; oriente-se sozinho:
- A **memória** (`MEMORY.md` + arquivos em `memory/`) carrega automática — é o ESTADO VIVO (o que está
  feito, o que vem, decisões). Leia o `project_estado.md` (topo = mais recente).
- **`docs/ROADMAP-refatoracao.md`** → a seção **FILA DE EXECUÇÃO** é a fila mestra.
- `git log --oneline -15` → os commits recentes.
- **🔄 O QUE ESTÁ ACONTECENDO AGORA (ago/2026): a PROGRESSÃO está saindo do papel.** Mergeados
  #228 (tipos + status base do tipo), #229 (o modelo da posição no GDD), #230 (o campo virou fileira
  com as frentes se olhando) e #231 (a preparação da fase virou tabuleiro). O **perfil de distância
  no motor** saiu na branch `feature/perfil-de-distancia` (`Arquetipos.MultiplicadorDePosicao` +
  `Combate.Casa`); **o próximo são OS DOIS BRILHOS** — a pintura do mapa de calor, com a grade 4×4
  vindo do C# (o front NÃO pode ter cópia da tabela). **Onde está escrito o quê:** o MODELO em
  `docs/GDD-progressao.md` §2 (a distância ideal por tipo) e §7 (a ordem dos passos); a EXECUÇÃO no
  `docs/ROADMAP-refatoracao.md` §FILA A item 19, que lista arquivo por arquivo o que fazer. Ler os
  dois antes de escrever qualquer linha — nada disso se re-deduz do código.
- **Fase atual:** FRONT feito, console REMOVIDO (#179), camadas ajustadas (#180), `PreverDano` (#181),
  **bot inteligente** (#182), **botão Auto** (#183), a **ordem do pipeline de dano** (`OrdemDeMitigacao`
  — fechou o bug do bloqueio × escudo, #185) e a **DEF do protetor** no `ProtecaoAliado` (doc mentia; a
  impl é a certa — tanque protege mais barato). A **bancada de dano ✅ está construída**
  (`ApostlesWar.Tests/Bancada/`, ~35s no `dotnet test`, escreve `docs/bancada-dano.md` versionado): 5
  linhas variando UM fator por vez — por-habilidade e apóstolo-inteiro × alvo imune/não-imune a malefícios
  × DEF 0/no cap. Zero mudança no motor. A seguir: **LER os números e rebalancear (#16)** — a bancada é
  o instrumento, o ajuste é o trabalho. Aberto: o #15 (faxina de comentários).
- **O front foi SEPARADO e está FECHADO (ago/2026): `jogo.js` 11.921 → 192 linhas.** Ver a seção
  "O FRONT, depois da separação" no fim deste arquivo — mapa de pastas, o contrato de tela, as duas
  injeções e o que os harnesses NÃO cobrem.
- **A 9ª pele (Humanos) está BLOQUEADA de propósito** — não há capítulo Humanos, então ela não teria
  onde aparecer. Quem cria a vaga é o fundo de facção no COMPÊNDIO. Ver ROADMAP §CENÁRIO POR CAPÍTULO.
  A **Arena** também vai ganhar cenário próprio — é o 1º tema que não é facção.

## Como trabalhamos
- **Design primeiro, JUNTO.** Discutir a arquitetura com o Gabriel — opinião real, trade-offs, questionar
  o próprio caminho — ANTES de codar. A execução é delegada só DEPOIS do desenho aprovado. Ele martela numa
  dúvida até entender de verdade; isso é aprendizado, não resistência.
- **Git flow:** eu crio a branch, implemento, `dotnet build` + `dotnet test`, e **PARO** — mostro o diff
  e espero o Gabriel APROVAR antes de commitar. Push só depois do ok. O commit é o ponto em que ele
  revisa o trabalho; commitar antes tira dele a etapa que é a dele. O Gabriel abre e mergeia o PR no
  GitHub web (não há `gh` CLI na máquina — e o *"Create a pull request by visiting…"* que aparece no
  output do push é texto do GitHub, não um PR criado). Depois eu limpo:
  `git checkout main && git pull && git branch -d <branch> && git fetch --prune`.
- **Nomes:** domínio em PORTUGUÊS (`Combate`, `Habilidade`, `Batalha`), andaime em INGLÊS
  (`View`/`Controller`/`Service`). Nome de capacidade = COMPORTAMENTO, nunca identidade de classe.
- **YAGNI, mas:** quando o Gabriel NOMEIA um futuro (Arena, front, medidor de velocidade), desenhar o seam
  agora vale — não é especulação. Verificar-antes-de-fundir ("o grep mente").
- **1 PR, 1 tema.** Mergeado antes do próximo começar — e **uma branch por vez**: nada de empilhar
  branch nem de adiantar o tema seguinte no working tree enquanto um PR espera. Avisar ao criar a branch.

## Comentário: cada coisa em UM lugar só (ago/2026)
Nasceu de uma medição: no PR da cura em área eu escrevi **28 linhas de comentário pra 11 de código**
(70%, contra 26% de média do repo). Esta regra é a metade da VAZÃO — **a faxina do estoque (#15) VAI
acontecer**, e a ordem é do Gabriel: primeiro separar o `jogo.js` (cada cenário que se move já sai
reduzido), **depois voltar nos outros arquivos e ajustar**. Sem a regra, a faxina se refaz sozinha;
sem a faxina, a regra só segura o que vier de novo.

| o que eu quero dizer | onde mora |
|---|---|
| o que a linha faz | **nada.** O código diz |
| por que ESTA linha é assim, o que quebra se mudar | comentário curto, na linha |
| por que este DESENHO e não o outro | ADR / ROADMAP |
| o que foi TENTADO e MORREU, o histórico, o "antes era X" | **mensagem de commit**, só |
| contrato entre camadas | `///` na superfície pública |

**O erro que a regra pega:** eu escrevo comentário no modo *"defender o PR"* — negrito, "de propósito",
"a tentação é" — e ele fica lá pra sempre no modo errado. **O lugar de defender o PR é a mensagem de
commit.** No piloto o bloco caiu de 28 pra 14 linhas sem perder nada acionável: saiu a HISTÓRIA (o git
guarda melhor, datada e grudada no diff) e a ÊNFASE; ficou a regra e a armadilha.

**O que NUNCA se corta:** armadilha ATIVA — o que quebra em silêncio se alguém "melhorar" o código.
Ex.: o `dispararReacaoPorAtaque` não pode virar dedução porque o Inferno do Diabo é `NaoAtaque` com
dano. Isso não é história, é mina. Corta-se a moral e o adjetivo, nunca o fato e a consequência.

**Razão comentário/código é métrica MENTIROSA aqui** — o topo do ranking são interfaces de capacidade
(`IPulaTurno`: 2 linhas de código, 11 de doc), onde o comentário É o produto. Faxinar por ranking
destrói o melhor primeiro.

**Como se aplica:** (1) boy-scout — mexeu no arquivo, ajusta o comentário dele no MESMO PR; (2) na
separação do `jogo.js`, cada cenário que se move **já sai reduzido** (*"vamos movendo e já
reduzindo"*); (3) **depois disso, a faxina dos OUTROS arquivos**, como trabalho próprio, fatiada por
camada/pasta — ver ROADMAP §Faxina de comentários, que já tem a medição de briefing.

## Comandos
- Build: `dotnet build`  ·  Testes: `dotnet test` (xUnit em `ApostlesWar.Tests/`).
- **Gotcha:** o jogo ABERTO trava o build (lock do `.exe`/`.dll`) — pedir pra fechar antes de buildar/testar.
- Combate NÃO roda headless (o loop chama a tela) → verificação em jogo é do Gabriel; testo só o que é
  PURO (motor, capacidades, `Batalha`, services). Com `ITelaDeCombate` injetável, uma tela no-op no
  projeto de Tests destrava testes de FLUXO (candidato: a ordem crítica de morte, #14 do ROADMAP).
- Distribuição futura: `dotnet publish -c Release -r <rid> --self-contained` → `.exe` no GitHub Releases.

## Mapa rápido — Clean Architecture, 1 PROJETO por camada (a dependência aponta pra dentro)
- `ApostlesWar.Domain/` regras do jogo, ZERO referências: `Combat/` (Combate, Batalha/Equipe,
  TurnoDoPersonagem, RelogioDoCombate, capacidades), `Skills/` (ações/buffs/debuffs/passivas),
  `Apostolos/<Faccao>/<Apostolo>/`, `Models/`, `Enum/`.
- `ApostlesWar.Application/` casos de uso: `Services/` orquestração · `Controllers/` (bot) ·
  `Portas/` (IApresentacao+Momento, ITelaDeCombate, IControladorDeTurno, IRepositorioDeSave).
- `ApostlesWar.Infrastructure/` impl das portas de dados (SaveLocal). Só a Presentation enxerga.
- `ApostlesWar.Presentation/` a ÚNICA pele: casca executável Windows (`net10.0-windows`, WinForms +
  WebView2; composition root real em `Front/AppFront.cs`, front webview em `Front/`+`wwwroot/`).
  `.exe` = `ApostlesWar.App.exe` (AssemblyName fixo), abre a janela direto — um perfil só no Play.
  - Chamava-se `Presentation.Desktop` enquanto havia uma 2ª pele (`Presentation.ConsoleUI`),
    **removida em #179** quando o front ficou jogável de ponta a ponta. Ela deixou o legado que
    importa: o motor não sabe desenhar nada. Se nascer outra pele, é só implementar as portas — e aí
    o sufixo de plataforma volta a fazer sentido nos dois nomes.
- Convenção: **pasta no disco = nome do projeto** (se divergir, o `dotnet sln add` cria uma pasta-de-solution
  fantasma no VS). Sem dependências externas ao repo — o antigo `GHUtils` foi dissolvido (jul/2026).
  **Ao renomear projeto:** editar o `.sln` À MÃO preservando o GUID (o `sln remove`+`add` gera um novo
  e mata o ponteiro do Play no `.suo`), mover só os arquivos VERSIONADOS (o `bin`/`obj` travado pelo
  VS faria o `git mv` da pasta inteira falhar no meio) e apagar a pasta velha depois — ela reaparece
  enquanto o VS estiver com a solução antiga carregada.
- Superfície pública = contrato entre camadas (sem `InternalsVisibleTo`); quebra de camada nem compila.
- Docs: `docs/ROADMAP-refatoracao.md`, `docs/ADR-*.md`, `docs/CATALOGO-de-acoes.md`, `docs/GDD-expansao.md`,
  `docs/GDD-progressao.md` (o plano que muda quase todo número), **`docs/LORE.md`**.

## A LORE, e as duas renomeações que ela obrigou — AS DUAS FEITAS (ago/2026)
**Os apóstolos são peças de brinquedo dos deuses; a guerra é entre apóstolos — e o JOGADOR não é um
apóstolo, é um jogador no nível dos deuses, convidado pela deusa Cindy.** Ler `docs/LORE.md` antes
de mexer em nome de facção, de apóstolo ou em texto de tela.

- ✅ **#17:** a facção `Apostolos` virou **`Ascendentes`** e o 🌬️ virou ❄️. O 🌬️ era o símbolo da
  **Cindy** — não da facção.
- ✅ **#18:** o herói jogável passou a se chamar **`apóstolo`** em todo o repo (904 trocas, 136
  arquivos; nasceram `Domain/Apostolos/` e o `ApostolosService`). Nesta ordem porque o inverso
  criaria pasta dentro de pasta homônima. **Os nomes velhos ficaram só na mensagem do commit.**
- **O 🦸 Herói ficou Herói** — é nome próprio, par do 🦹 Vilão. "Apóstolo" é o conceito, não o
  personagem.
- **Em identificador é `apostolo`; em PROSA é `apóstolo`.** Vale pra comentário, doc e string que
  vira documento gerado.

**Ao renomear uma facção, o que é derivado e o que não é:** a chave de tema NASCE do enum
(`FluxoDoFront.cs:431`, `faccao.ToString().ToLowerInvariant()`), então o `body[data-tema]`, a pasta
`wwwroot/cenarios/<tema>/`, os dois arquivos dela, o `<link>` do index e a entrada do `AR_DO_TEMA`
têm de mudar no MESMO commit ou o tema some sem erro nenhum. **O save NÃO quebra:** não há
`JsonStringEnumConverter`, então enum vira NÚMERO no JSON — renomear o membro é seguro desde que a
ORDEM da lista não mude.

## Fazer o CENÁRIO de uma facção (falta Humanos) — leia nesta ordem
Oito peles prontas (👑 Reino · 🌑 Lado Sombrio · ⚙️ Tecnológicos · 🪬 Folclore · 🐉 Místicos · ⭐ Especial ·
🔱 Decaídos · ❄️ Ascendentes), falta 1. **A conta das assinaturas está fechada:** dia claro (Reino), lua
(cemitério), estrelas (invasão), âmbar de fogo (Folclore), crepúsculo (praia), interior sem céu
(Especial), luz vinda de baixo (Inferno) e a paisagem vista por um RECORTE (a janela dos Ascendentes).
Pros Humanos não sobra HORA nenhuma — o caminho é o mesmo do ⭐ Especial e dos ❄️ Ascendentes: um LUGAR em
vez de uma hora, ou um enquadramento.
**Não invente processo: já existe manual, e ele é caro — cada linha dele custou uma rodada de "ficou
ruim" em jogo.**
1. **`docs/ROADMAP-refatoracao.md` → §CENÁRIO POR CAPÍTULO.** É O manual: as três camadas e o que
   decide em qual entrar, ladrilho × canvas × endereço, o MAESTRO (dado compartilhado), os motores
   já extraídos, a lista de ARMADILHAS (cada uma já custou tempo — leia todas antes de desenhar) e as
   LIÇÕES DE DESENHO. Se for contra alguma delas, seja de propósito e diga por quê.
2. **Uma pele pronta INTEIRA, das duas pontas:** o bloco `body[data-tema="misticos"]` no `estilo.css`
   e a entrada `misticos` do `AR_DO_TEMA` no `jogo.js` (a mais completa: mar, dragão em três
   distâncias, lâmpada, aparições, moldura em canvas). O ⭐ Especial é a segunda melhor referência, e
   a única com INTERIOR, com peças que se ocultam entre si (porta × sentado, anel × cocô) e com o
   `comListras` — o padrão de "monta o caminho UMA vez, usa pra preencher E recortar". O 🔱 Decaídos
   é a mais barata de imitar em peça grande: ela é montada de MEMBROS sorteados num caminho
   só (`tracarMembro`, com o sentido do traço garantido por construção), o maestro sendo LUZ e não
   vento (`inferno.pulso`), e uma HISTÓRIA amarrando as peças umas nas outras. O ❄️ Ascendentes é a mais
   NOVA, e a de imitar quando a cena tiver um ROTEIRO: a noite inteira dele é uma sequência de passos
   (`criarRoteiroDaNoite`) escrita num maestro por uma camada que **não desenha nada**, e lida por
   quatro peças em cantos diferentes da tela. É também a única em que a paisagem é vista por um
   RECORTE, e ela custou TRÊS versões — as duas que morreram estão contadas no ROADMAP, que é onde o
   que não deu certo fica registrado.
3. **`git log --oneline` dos PRs de cenário** (#195, #197, #198, #199, #200, #201, #202, #203) — as
   mensagens contam o que foi tentado e MORREU, que é a parte que o código não mostra.

**A receita, em uma linha:** o tema é `faccao.ToString().ToLowerInvariant()` → `body[data-tema]`, e
custa **zero C#** — um bloco de CSS mais uma entrada de configuração. Tema sem CSS e sem entrada no
`AR_DO_TEMA` simplesmente luta no visual padrão (foi assim que o Folclore saiu inteiro no #199).

**Depois da separação (ago/2026) isso virou:** criar `wwwroot/cenarios/<faccao>/<faccao>.js` com
`export const ar = {...}` e os builders, e pôr **uma linha** no registro `AR_DO_TEMA` do `jogo.js`.
Mais o `<faccao>.css` na mesma pasta (e o `<link>` no index.html). **E rodar
`node --experimental-vm-modules ferramentas/rodar-tema.js` antes de pedir conferência em jogo.**

**As decisões que vêm ANTES de desenhar qualquer coisa:**
- **Que assinatura sobrou.** Dia claro é do Reino, lua do cemitério, estrelas da invasão, âmbar do
  fogo, crepúsculo da praia. Escolha o que SOBROU antes de escolher o que é bonito — é o que faz o
  capítulo ser reconhecível de relance.
- **Os 4 apóstolos entram pelo SINAL, não pela figura.** Nada de corpo humano (fica esquisito em canvas);
  o gênio é a lâmpada, a sereia é a cauda, a fada é o vaga-lume maior. E o gesto tem que ser do sinal:
  cauda sozinha fazendo salto de golfinho lê como pedaço arremessado.
- **Uma peça CENTRAL, uma fonte de luz.** Fogueira, lâmpada. E o que mais acontece na cena responde a
  ela ou ao maestro.
- **Verificação:** `node --check jogo.js`, chaves do `estilo.css` batendo, todo builder do
  `noFundo`/`naFrente` com definição, nenhuma chave de config sem uso, e `dotnet build` limpo. Raio
  negativo num `arc`/`ellipse` LANÇA e mata o `requestAnimationFrame` — a cena congela em silêncio.
  **Pior ainda: NaN em coordenada NÃO lança**, só não desenha (`Math.pow(negativo, fracionário)` é a
  fonte clássica). Vale montar a **bancada headless** do tema — extrair os builders com `eval` + um
  `ctx` de mentira que VALIDA cada argumento (raio ≥ 0, tudo finito, `save`/`restore` batendo) e
  rodar ~900s de `dt` fixo. Ela já pegou bug fatal em três peles seguidas.
- **A conferência em jogo é do Gabriel**, sempre: quase todo acerto deste front veio de ele olhar
  rodando e apontar o defeito exato. E **quando ele descreve um MECANISMO, implementar LITERALMENTE**
  — no ⭐ Especial eu interpretei quatro vezes e errei as quatro; o desenho dele estava certo desde a
  primeira frase.

## O FRONT, depois da separação (ago/2026) — leia antes de tocar em `wwwroot/`

```
wwwroot/
  jogo.js            COMPOSITION ROOT (192 linhas). Imports, a tabela TELAS, o interpretador,
                     o Esc/botão-sair, o registro CENARIOS e o boot. NÃO desenha nada.
  index.html · estilo.css
  nucleo/   ponte.js (o único que sabe do C#) · cena.js (quem está na tela + abrirTela)
            ar.js (canvas, maestro, laço, aplicarTema)
  ui/       modal.js · time.js (picker+slots+arrastar) · animacao.js
  telas/    menu · perfil · arena · campanha · arsenal · compendio · combate
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
