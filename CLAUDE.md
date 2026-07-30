# Apostle's War — guia pro Claude

RPG por turnos em C# (`net10.0`, `OutputType Exe`). Roda numa **janela webview** (WebView2), jogável de
ponta a ponta: menu, perfil, campanha, arena, arsenal. Projeto de estudo/portfólio do Gabriel.

## Orientação — faça no início da sessão e quando o Gabriel perguntar "onde estamos"
Não precisa o Gabriel pedir; oriente-se sozinho:
- A **memória** (`MEMORY.md` + arquivos em `memory/`) carrega automática — é o ESTADO VIVO (o que está
  feito, o que vem, decisões). Leia o `project_estado.md` (topo = mais recente).
- **`docs/ROADMAP-refatoracao.md`** → a seção **FILA DE EXECUÇÃO** é a fila mestra.
- `git log --oneline -15` → os commits recentes.
- **Fase atual:** FRONT feito, console REMOVIDO (#179), camadas ajustadas (#180), `PreverDano` (#181),
  **bot inteligente** (#182), **botão Auto** (#183), a **ordem do pipeline de dano** (`OrdemDeMitigacao`
  — fechou o bug do bloqueio × escudo, #185) e a **DEF do protetor** no `ProtecaoAliado` (doc mentia; a
  impl é a certa — tanque protege mais barato). A **bancada de dano ✅ está construída**
  (`ApostlesWar.Tests/Bancada/`, ~35s no `dotnet test`, escreve `docs/bancada-dano.md` versionado): 5
  linhas variando UM fator por vez — por-habilidade e champ-inteiro × alvo imune/não-imune a malefícios
  × DEF 0/no cap. Zero mudança no motor. A seguir: **LER os números e rebalancear (#16)** — a bancada é
  o instrumento, o ajuste é o trabalho. Aberto: o #15 (faxina de comentários).
- **Frente aberta em paralelo (jul/2026): os CENÁRIOS por capítulo.** Cinco peles prontas, faltam 4, e
  a próxima é a ⭐ Especial — ver a seção "Fazer o CENÁRIO de uma facção" no fim deste arquivo. Custa
  zero C#. Anotada no ROADMAP como **dívida prevista**: o `jogo.js` passou de 7 mil linhas e o cenário
  é ~70% delas, com a fronteira (`aplicarTema`) já pronta pra virar arquivo por tema — quando o
  Gabriel decidir pagar.

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
- **1 PR, 1 tema.** Mergeado antes do próximo começar.
- **Nomes:** domínio em PORTUGUÊS (`Combate`, `Habilidade`, `Batalha`), andaime em INGLÊS
  (`View`/`Controller`/`Service`). Nome de capacidade = COMPORTAMENTO, nunca identidade de classe.
- **YAGNI, mas:** quando o Gabriel NOMEIA um futuro (Arena, front, medidor de velocidade), desenhar o seam
  agora vale — não é especulação. Verificar-antes-de-fundir ("o grep mente").

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
  `Champs/<Faccao>/<Champ>/`, `Models/`, `Enum/`.
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
- Docs: `docs/ROADMAP-refatoracao.md`, `docs/ADR-*.md`, `docs/CATALOGO-de-acoes.md`, `docs/GDD-expansao.md`.

## Fazer o CENÁRIO de uma facção (faltam 🔱 Decaídos · ✝️ Apóstolos · Humanos) — leia nesta ordem
Seis peles prontas (👑 Reino · 🌑 Lado Sombrio · ⚙️ Tecnológicos · 🪬 Folclore · 🐉 Místicos · ⭐ Especial),
faltam 3.
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
   `comListras` — o padrão de "monta o caminho UMA vez, usa pra preencher E recortar".
3. **`git log --oneline` dos PRs de cenário** (#195, #197, #198, #199, #200, #201) — as mensagens
   contam o que foi tentado e MORREU, que é a parte que o código não mostra.

**A receita, em uma linha:** o tema é `faccao.ToString().ToLowerInvariant()` → `body[data-tema]`, e
custa **zero C#** — um bloco de CSS mais uma entrada de configuração. Tema sem CSS e sem entrada no
`AR_DO_TEMA` simplesmente luta no visual padrão (foi assim que o Folclore saiu inteiro no #199).

**As decisões que vêm ANTES de desenhar qualquer coisa:**
- **Que assinatura sobrou.** Dia claro é do Reino, lua do cemitério, estrelas da invasão, âmbar do
  fogo, crepúsculo da praia. Escolha o que SOBROU antes de escolher o que é bonito — é o que faz o
  capítulo ser reconhecível de relance.
- **Os 4 champs entram pelo SINAL, não pela figura.** Nada de corpo humano (fica esquisito em canvas);
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
