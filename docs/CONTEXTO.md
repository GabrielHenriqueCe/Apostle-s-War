# CONTEXTO — o estado vivo entre uma sessão e a outra

> **O QUE ESTE ARQUIVO É.** O resumo comprimido da última sessão de trabalho: onde paramos, o que foi
> decidido e o que vem. Ele é **SUBSTITUÍDO** a cada sessão, não acrescentado — é uma FOTO do agora,
> não um diário. Quem quer histórico tem o `git log`, que guarda melhor e datado.
>
> **COMO USAR (Claude):** ler no início de toda sessão, logo depois do `CLAUDE.md`. E ao fim da
> sessão, reescrever este arquivo do zero com o estado novo.
>
> **O QUE NÃO ENTRA AQUI:** o que já está escrito em outro lugar. O modelo do jogo está no
> `docs/GDD-progressao.md`, a fila de execução no `docs/ROADMAP-refatoracao.md`, as regras de
> trabalho no `CLAUDE.md`, e o que foi tentado e morreu nas mensagens de commit. Aqui ficam só
> **ponteiros e o que ainda está no ar**.

---

## Onde paramos (15/ago/2026)

**Main em `18cd926`.** Sete PRs mergeados nas duas últimas sessões, todos da **PROGRESSÃO** saindo do
papel — a ordem é a do `GDD-progressao.md` §7, e ela não é negociável (status e turno ANTES de nível
e raridade):

| # | o que entrou |
|---|---|
| #232 | o handoff da progressão em doc |
| #233 | o **perfil de distância** no motor (`Arquetipos.MultiplicadorDePosicao` + `Combate.Casa`) |
| #234 | o **mapa de calor** da posição na preparação da fase |
| #235 | o aviso do RNG da bancada (as 5 linhas que oscilam são COMPORTAMENTO) |
| #236 | o **medidor de turno** — a `FilaDeTurnos` matou o `for` sobre `Equipe1 ++ Equipe2` — e a barra na tela |
| #237 | **`DEF/(DEF+5000)`** + **Precisão × Resistência** (o passo 4 do GDD) |

**PR ABERTO, esperando o merge dele:** branch `feature/fila-na-tela` (`a0f39e5` + `cd062d9`) — o
**cordão de turnos** (a ordem dos 8 próximos, flutuando no alto do campo) e o **medidor zerando nas
duas pontas a cada onda**. Ele já conferiu em jogo: *"tá ótimo"*.

## O que vem depois (na ordem do GDD §7)

1. **Nível (curva do tipo) + Raridade** nos apóstolos — sem estrela.
2. **Raridade → passiva que escala.**
3. **Item equipado no apóstolo.**

**Também em aberto, menores:** o `chance de aplicar: 75%` ao mirar (GDD §1 — e ele ficou *necessário*
depois do #237, porque o texto da habilidade agora conta só metade da história); o **empurrão de
medidor** como efeito de habilidade (a tabela está no GDD §1 — enquanto não existir, a barra quase
nunca passa de 100 e as faixas 2–5 da rampa são código dormindo); e a **9ª pele (Humanos)**, que
segue bloqueada de propósito até o fundo de facção no compêndio.

## Decisões desta sessão que NÃO se reabrem

- **A `chance` da habilidade e Precisão × Resistência são coisas SEPARADAS** e ambas ficam. Palavras
  dele: *"cada hab vai ter a sua, é pra dar o incremento diferencial no rebalanceamento, ainda mais
  em habs roubadas, e vai servir após haver diferença nas habs por raridade"*. São duas rolagens em
  sequência — o que dá no mesmo que multiplicar as probabilidades, mas separadas elas permitem
  distinguir *"a habilidade nem tentou"* de *"o alvo resistiu"* quando isso for pintado.
- **A onda nova começa EMPATADA:** os dois lados zeram o medidor a cada rodada.
- **O 2º brilho (a aura do cursor no passo de alvo) está MORTO** — aura de cursor não existe em tela
  sensível ao toque. Se voltar, volta como destaque nas CAIXAS.
- **O sinal de espera entre as fichas do cordão foi cortado**; o `Vez.Esperou` fica no motor.
- **A bancada oscila por DESENHO, não por ruído** — está documentado no cabeçalho do próprio
  relatório. Semear o RNG está descartado.

## Armadilhas que morderam nesta sessão (não repetir)

- **O `body` é um grid de TRÊS linhas** (topo · arena · painel). Peça nova entre o topo e o painel
  entra como quarto item, rouba a linha do `1fr` e o campo encolhe — o canvas do cenário fica com o
  tamanho velho e a cena aparece **cortada e subindo**. Peça nova ali é **absoluta dentro da arena**.
- **Mensagem de commit multilinha:** escrever o arquivo com a ferramenta de escrita, nunca com
  `Out-File -Encoding utf8` do PowerShell — ele grava BOM e o BOM entra no assunto do commit.
- **Squash-merge deixa a branch "não mergeada" pro git.** Conferir com `git diff main <branch>`
  vazio antes do `-D`, e recriar branch nova com `git switch -C <nome> origin/main` (o working tree
  passa junto).
- **Rodar `dotnet test` suja o `docs/bancada-dano.md`** em 5 linhas. Reverter quando a mudança do PR
  não for de balanço.

## O que o Gabriel confere, sempre

Hover, arraste, clique e tudo que acontece DURANTE a batalha. Os dois harnesses
(`ferramentas/rodar-telas.js` e `rodar-tema.js`) publicam mensagem e montam tela — **eles não clicam
em nada**. Verde deles nunca quer dizer "o jogo funciona".
