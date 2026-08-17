# Apostle's War — guia pro Claude

RPG por turnos em C# (`net10.0`, `OutputType Exe`). Roda numa **janela webview** (WebView2), jogável de
ponta a ponta: menu, perfil, campanha, arena, arsenal. Projeto de estudo/portfólio do Gabriel.

## Abertura — o protocolo, e ele é ORÇAMENTO, não checklist
Oriente-se sozinho, sem o Gabriel pedir — e **pare quando o protocolo acabar**:

1. **`docs/CONTEXTO.md`, inteiro** (~1k tokens). É a foto da última sessão: onde paramos, o que está
   no ar, o que foi decidido e as armadilhas que acabaram de morder. É reescrito do zero ao FIM de
   cada sessão, e é o **ÚNICO dono do estado** — não existe segunda foto, nem aqui nem na memória.
2. **NÃO abrir os documentos que ele cita.** Ler o ponteiro não é ler o alvo; o alvo se abre quando
   a TAREFA for dele.
3. **O `gitStatus` da abertura já traz o branch e os commits recentes** — repetir `git log`/
   `git branch` é contexto jogado fora.
4. **Saudação sem tarefa = passo 1, responder onde estamos, e PARAR.**

A **memória** (`MEMORY.md` + `memory/`) carrega sozinha. O resto vem pela tarefa — ver a última
seção deste arquivo.

**O orçamento, medido (ago/2026).** Nada abaixo se lê inteiro sem um motivo dito em voz alta; abrir
**por seção**, com `Grep` ou `offset`/`limit`:

| arquivo | ~tokens | quando |
|---|---:|---|
| `ROADMAP-refatoracao.md` | 20k | a FILA A, quando for escolher o que fazer |
| `MANUAL-cenario.md` | 19k | só fazendo pele de facção |
| `bancada-dano.md` | 13k | é RELATÓRIO gerado — ler o `git diff`, não o arquivo |
| `GDD-itens.md` | 12k | o passo mais distante da fila |
| `ADR-composicao-de-acoes.md` | 10k | mexendo em Ação/habilidade |
| `GDD-combate.md` · `GDD-progressao.md` · `MANUAL-combate.md` | 9k · 8k · 7k | pelo § do assunto |

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
- **Fechar a sessão = reescrever o `docs/CONTEXTO.md`** do zero, com o estado do momento. Ele
  SUBSTITUI o anterior (não acumula), e só carrega ponteiro + o que ainda está no ar — o que já mora
  no GDD, no ROADMAP ou numa mensagem de commit não se repete lá.
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
  `docs/GDD-progressao.md` + `GDD-combate.md` + `GDD-itens.md` (o modelo que muda quase todo
  número), **`docs/LORE.md`**.

## A LORE, em uma frase
**Os apóstolos são peças de brinquedo dos deuses; a guerra é entre apóstolos — e o JOGADOR não é um
apóstolo, é um jogador no nível dos deuses, convidado pela deusa Cindy.** Ler `docs/LORE.md` antes de
mexer em nome de facção, de apóstolo ou em texto de tela. **Em identificador é `apostolo`; em PROSA é
`apóstolo`** — vale pra comentário, doc e string que vira documento gerado. O 🦸 Herói ficou Herói: é
nome próprio, par do 🦹 Vilão.

## O resto é carregado pela TAREFA, não por esta página
Cada assunto tem um doc próprio em `docs/`, e uma skill em `.claude/skills/` que dispara quando a
tarefa é daquele assunto. Ponteiro aqui não bastava: punha em mim a decisão de ler, e ela falhava.

| quando a tarefa for | leia |
|---|---|
| pele de facção / cenário de batalha | `docs/MANUAL-cenario.md` |
| `wwwroot/`, telas, a ponte C#↔JS | `docs/MANUAL-front.md` |
| motor de combate: dano, reações, turno, bot, bancada | `docs/MANUAL-combate.md` |
| implementar um passo da progressão | `docs/GDD-progressao.md` §7, depois o `GDD-combate.md` |
| o desenho de uma decisão grande | os `docs/ADR-*.md` |
| o que foi tentado e MORREU | `git log` |
