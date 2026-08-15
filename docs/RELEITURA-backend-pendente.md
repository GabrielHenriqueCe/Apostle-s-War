# Releitura pendente do back-end — o que foi aprovado sem ser lido

> **POR QUE ESTE ARQUIVO EXISTE.** No front eu deixava a sessão em modo automático e só via
> acontecer — não entendo de front, então delegar fazia sentido. No back eu usava plan mode e
> revisava o código de verdade... até que, sem perceber, o hábito virou aprovar o plano e não ler
> mais o diff. O resultado é que boa parte do que está listado abaixo entrou no repo sem eu ter
> martelado nela.
>
> **A tarefa que este arquivo guarda:** em algum momento, PARAR de codar coisa nova no back e voltar
> aqui, item por item, até ENTENDER de fato o que foi feito — não só saber que existe. Só depois
> disso o back volta a ser plan-mode-martelando-de-verdade. Até lá, front continua delegado, e o back
> continua sendo tratado como o de sempre — mas esta lista é a dívida a pagar antes de confiar cega
> no próximo plano.
>
> **Como usar:** pegar um item, `git show <hash>` (ou `git log -p <hash> -1`) pra ler o diff de
> verdade, e só riscar da lista quando o "por que este desenho e não outro" fizer sentido sem
> precisar perguntar de novo.

---

## 1. Estrutura e camadas (jul)

| PR | commit | o que entrou |
|---|---|---|
| #164 | `2382396` | 1 projeto por camada — 203 arquivos migrados pra `Domain/Application/Infrastructure/Presentation/Tests` |
| #166 | `ef7db34` | nomes de projeto consistentes + dissolveu o `GHUtils` (última dependência externa) |
| #167 | `e032cbc` | `Presentation` explícito no nome dos projetos |
| #179 | `f0b07dd` | matou a pele de console — sumiram `IEntrada`, `ITelaDeMenu`, `GerenciadorDeJogoService`, `NavegacaoTests` (−521 linhas) |
| #180 | `b089244` | cada decisão na sua camada — nasceu o enum `Momento` no `IApresentacao` (o motor manda a batida, a pele escolhe a duração; antes o motor mandava `1500` e o front dividia) |
| #184 | `f8286e7` | `Presentation.Desktop` → `Presentation` (só há uma pele) |

## 2. Serviços novos nascidos pra atender o front (jul)

- **`PerfilService` + `Perfil`** (#172 `5358883`) — perfil do jogador, com `IRepositorioDeSave` e `SaveLocal` estendidos.
- **`CampanhaService`** (#174 `8ed26a0`) — mapa de facções, fases, batalha e recompensa.
- **`ConfiguracaoService`** (#191 `b77ad94`) — configuração persistida.
- **`ArsenalService`** ganhou `EstaEquipado` (#177 `85774ed`) — regra que estava no JS voltou pra Application.
- **Ciclo de fase fechado** (#194 `c79774f`) — do time montado até o "e agora?".
- **Save/compêndio/navegação** (#190 `a7796a9`) — correções atravessando os 5 services.
- `PedeAlvoDoJogadorTests`, `CampanhaServiceTests`, `PerfilServiceTests`, `ConfiguracaoServiceTests` nasceram junto.

## 3. Motor de combate — as mudanças de regra

| PR | commit | mudança |
|---|---|---|
| #181 | `423c7ca` | `PreverDano` — a fórmula de dano ganhou um espelho puro (a base do bot e, depois, do cordão) |
| #185 | `a134b9a` | `OrdemDeMitigacao` — o pipeline de dano virou ordem explícita; fechou o bug bloqueio × escudo |
| #186 | `e72bd84` | `ProtecaoAliado` usa a DEF do PROTETOR (a doc mentia; tanque protege mais barato) |
| #192 | `104c3c8` | permanente que aguenta ser estendido — `StatusEffect` + 6 buffs/debuffs |
| #207 | `cfcce6d` | cura em grupo cai de uma vez, como o golpe em área |
| #228 | `6594bb1` | os 108 números viraram 4 arquétipos — nasceu `Models/Arquetipos.cs`, os 36 apóstolos passaram a derivar status do tipo |
| #233 | `4f57d5a` | perfil de distância — `Arquetipos.MultiplicadorDePosicao` + `Combate.Casa` |
| #236 | `7d267d5` | `FilaDeTurnos` — matou o `for` sobre `Equipe1 ++ Equipe2`; o turno virou medidor |
| #237 | `18cd926` | `DEF/(DEF+5000)` (DEF virou curva, sem cap) + Precisão × Resistência (malefício passou a poder falhar) |
| #238 | `ef3dd60` | `Prever` na `FilaDeTurnos` (a ordem dos 8 próximos, rodando as MESMAS funções do combate) + `ZerarMedidor` a cada onda + gatilho novo no `ITelaDeCombate` |

## 4. Bot

- **#182 `10c68b9`** — o pacote grande (29 arquivos): `ControladorBot` inteligente, nasceram `IPuneQuemAtaca` e `Skills/Acoes/Utilidade.cs`, e toda `Acao` passou a se declarar (dano/cura/buff/debuff/reviver/explodir…) pro bot poder pontuar sem `is`.
- **#193 `528a6d2`** — o bot lê o kit de qualquer um e aponta o alvo do automático.
- **#183 `01e7e8c`** — o botão Auto (o cérebro joga no seu lugar).

## 5. Bancada de dano e rebalanceamento

- **#187 `f126175`** — nasceu `ApostlesWar.Tests/Bancada/` (`BancadaDeDano.cs` + `PecasDaBancada.cs`, 558 linhas). Roda no `dotnet test` (~35s) e escreve `docs/bancada-dano.md` versionado. Zero mudança no motor.
- **#188 `de8528e`** — colunas de cura e de 4 alvos + rankings.
- **#189 `3374a2e`** — o rebalanceamento: 42 arquivos, os 36 apóstolos + `HabilidadeAtiva` ajustados lendo os números da bancada.
- **#235 `d586749`** — o aviso do RNG corrigido: as 5 linhas que oscilam são COMPORTAMENTO, não ruído (semear o RNG está descartado).

## 6. Renomeações da lore

- **#220 `f7f9880`** — facção `Apostolos` → `Ascendentes` (18 arquivos).
- **#221 `e7feff7`** — champ/campeão → apóstolo em todo o repo: 101 arquivos, 383 trocas; `Domain/Champs/` virou `Domain/Apostolos/`, nasceu o `ApostolosService`.
- **#199 `2753d06`** / **#208 `37a7506`** — limpeza do cenário Folclore e o piloto da regra de comentário.

---

## Ordem sugerida pra releitura

Não precisa ser esta ordem, mas se ajudar: **3 → 4 → 5 → 1 → 2 → 6**. O motor de combate (3) é onde
mora a maior parte das REGRAS que mudaram sem eu ter martelado nelas — DEF virando curva, Precisão ×
Resistência, a fila de turnos. O bot (4) e a bancada (5) só fazem sentido depois de entender o motor.
Estrutura (1), serviços (2) e renomeações (6) são mais mecânicos — servem de aquecimento ou de
fechamento, não de prioridade.
