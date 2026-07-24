# Apostle's War — guia pro Claude

RPG por turnos em C# (`net10.0`, `OutputType Exe`). Console hoje; o próximo grande passo é o **porte
pra um front amigável (webview)**. Projeto de estudo/portfólio do Gabriel.

## Orientação — faça no início da sessão e quando o Gabriel perguntar "onde estamos"
Não precisa o Gabriel pedir; oriente-se sozinho:
- A **memória** (`MEMORY.md` + arquivos em `memory/`) carrega automática — é o ESTADO VIVO (o que está
  feito, o que vem, decisões). Leia o `project_estado.md` (topo = mais recente).
- **`docs/ROADMAP-refatoracao.md`** → a seção **FILA DE EXECUÇÃO** é a fila mestra.
- `git log --oneline -15` → os commits recentes.
- **Fase atual:** fechar o resto da FILA A → **FRONT (webview)** → **REBALANCE (#16)**. Decisão do Gabriel:
  o front vem ANTES do rebalance (quer balancear numa interface amigável, não no console).

## Como trabalhamos
- **Design primeiro, JUNTO.** Discutir a arquitetura com o Gabriel — opinião real, trade-offs, questionar
  o próprio caminho — ANTES de codar. A execução é delegada só DEPOIS do desenho aprovado. Ele martela numa
  dúvida até entender de verdade; isso é aprendizado, não resistência.
- **Git flow:** eu crio a branch, implemento, `dotnet build` + `dotnet test`, commito, dou push. O Gabriel
  abre e mergeia o PR no GitHub web (não há `gh` CLI na máquina). Depois eu limpo:
  `git checkout main && git pull && git branch -d <branch> && git fetch --prune`.
- **1 PR, 1 tema.** Mergeado antes do próximo começar.
- **Nomes:** domínio em PORTUGUÊS (`Combate`, `Habilidade`, `Batalha`), andaime em INGLÊS
  (`View`/`Controller`/`Service`). Nome de capacidade = COMPORTAMENTO, nunca identidade de classe.
- **YAGNI, mas:** quando o Gabriel NOMEIA um futuro (Arena, front, medidor de velocidade), desenhar o seam
  agora vale — não é especulação. Verificar-antes-de-fundir ("o grep mente").

## Comandos
- Build: `dotnet build`  ·  Testes: `dotnet test` (xUnit em `ApostlesWar.Tests/`).
- **Gotcha:** o jogo ABERTO trava o build (lock do `.exe`/`.dll`) — pedir pra fechar antes de buildar/testar.
- Combate NÃO roda headless (`Console.Clear`/`ReadKey` precisam de TTY) → verificação em jogo é do Gabriel;
  testo só o que é PURO (motor, capacidades, `Batalha`).
- Distribuição futura: `dotnet publish -c Release -r <rid> --self-contained` → `.exe` no GitHub Releases.

## Mapa rápido — Clean Architecture, 1 PROJETO por camada (a dependência aponta pra dentro)
- `ApostlesWar.Domain/` regras do jogo, ZERO referências: `Combat/` (Combate, Batalha/Equipe,
  TurnoDoPersonagem, RelogioDoCombate, capacidades), `Skills/` (ações/buffs/debuffs/passivas),
  `Champs/<Faccao>/<Champ>/`, `Models/`, `Enum/`.
- `ApostlesWar.Application/` casos de uso: `Services/` orquestração · `Controllers/` (bot) ·
  `Portas/` (IEntrada, IApresentacao, ITelaDeCombate, ITelaDeMenu, IControladorDeTurno, IRepositorioDeSave).
- `ApostlesWar.Infrastructure/` impl das portas de dados (SaveLocal). Só o App enxerga.
- `ApostlesWar.ConsoleUI/` pele console: views + EntradaConsole/ApresentacaoConsole + ControladorJogador.
- `ApostlesWar.App/` casca executável Windows (composition root em `Program.cs`, front webview em
  `Front/`+`wwwroot/`). No VS, o dropdown do Play tem os perfis **Console** e **Front (webview)** (`--front`).
- Convenção: **pasta no disco = nome do projeto** (se divergir, o `dotnet sln add` cria uma pasta-de-solution
  fantasma no VS). Sem dependências externas ao repo — o antigo `GHUtils` foi dissolvido (jul/2026).
- Superfície pública = contrato entre camadas (sem `InternalsVisibleTo`); quebra de camada nem compila.
- Docs: `docs/ROADMAP-refatoracao.md`, `docs/ADR-*.md`, `docs/CATALOGO-de-acoes.md`, `docs/GDD-expansao.md`.
