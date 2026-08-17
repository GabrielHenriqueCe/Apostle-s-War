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

## Onde paramos (17/ago/2026)

**Main em `6d62f67`, working tree limpo fora este arquivo. Nenhum PR aberto.** A sessão não tocou
código do jogo: foi inteira sobre **o custo de contexto e a arrumação dos documentos**.

O gatilho: um "oi" sem tarefa nenhuma deixou o contexto em 28%. Medimos em vez de chutar, e o
diagnóstico virou um plano de 5 passos, salvo em
`~/.claude/plans/o-ctx-apenas-dando-pure-pearl.md` (aprovado, com só o Passo 1 executado).

**Feito nesta sessão (fora do repo, sem PR):**
- **A mina de 46k tokens foi desarmada.** A memória `project_estado.md` tinha virado diário de
  **165 KB / 1.829 linhas** acumulado desde o PR #110 (a main já no #238) — e era uma segunda foto
  concorrente DESTE arquivo. Apagada. O diretório de memória caiu de **216 KB → 55 KB**.
- Salvei só o que não estava em outro lugar: a **ordem crítica de morte**
  (`IReageAntesDeMorrer` → `IReageAoMatar` → `IReageAoMorrer`, em `Combat/Reacoes/IReacoes.cs`),
  verificada contra o código antes de gravar.
- Nasceu a memória do **orçamento de contexto**.
- Um **mapa visual** do projeto foi publicado como artefato e **descartado de propósito** pelo
  Gabriel — ele só queria ver uma vez. Não há mapa a manter; ver a decisão abaixo.

## O que vem — a fila desta arrumação (Passos 2 a 4 do plano)

1. **Partir o `ROADMAP-refatoracao.md` (222 KB) em TRÊS baldes, não dois.** O corte por
   `✅ feito` × `pendente` perde a parte mais cara: as ARMADILHAS e LIÇÕES DE DESENHO nasceram de
   itens **já feitos**. Os baldes: **pendente** fica na fila · **histórico** sai (o `git log` guarda
   melhor) · **referência viva** vai pro doc do assunto. O primeiro recorte é óbvio: a
   `## CENÁRIO POR CAPÍTULO` ocupa as **linhas 103–1080 — 977 linhas, ~40% do arquivo**, e é um
   manual morando dentro de uma fila → `docs/MANUAL-cenario.md`. Das 26 seções, ≥12 estão marcadas
   ✅ FEITO. A fila de verdade são 392 linhas (1121–1512).
2. **Partir o `GDD-progressao.md` (1.715 linhas) — mas por ASSUNTO + DISTÂNCIA NA FILA, não por
   feito/pendente.** O GDD é MODELO: `DEF/(DEF+5000)` segue sendo a referência depois de
   implementada, e apagá-la deixaria o código como única fonte da regra. O peso real:

   | seção | linhas | % |
   |---|---:|---:|
   | §4 ITENS | **696** | **40,6%** |
   | §2 posição e tipo | 303 | 17,7% |
   | §1 stats novos | 263 | 15,3% |
   | §3 nível e raridade | 136 | 7,9% |
   | §5 campanha | 121 | 7,1% |
   | §Decisões fechadas | 104 | 6,1% |
   | §7 o plano | 42 | 2,4% |
   | §Revogado | 20 | 1,2% |

   O corte: **`GDD-itens.md`** ← §4 (o passo mais distante da fila) · **`GDD-combate.md`** ← §1+§2
   (o modelo vivo) · o **`GDD-progressao.md`** fica com §3+§5+§6+§7+decisões (~410 l, o que está em
   curso) · §Revogado sai (é mensagem de commit por definição).
3. **Instrução por assunto, carregada pela TAREFA.** O conteúdo mora em `docs/` (seu, portátil, no
   GitHub, passa por PR); uma **skill de ~10 linhas** em `.claude/skills/` aponta pra ele e dispara
   pela descrição. Ponteiro no `CLAUDE.md` não basta: põe a decisão de ler no assistente, e ela
   falhou nesta sessão. Com isso o `CLAUDE.md` perde as 59 linhas do §CENÁRIO, as 27 do §FRONT e as
   22 da §LORE (concluída; sobram ~6 acionáveis): **224 → ~140 linhas**.
4. **A §Orientação do `CLAUDE.md` vira protocolo com orçamento** — ver "armadilhas" abaixo.

**O trabalho de JOGO segue parado onde estava**, e ele volta a valer assim que a arrumação fechar:
**nível (curva do tipo) + raridade**, depois **raridade → passiva que escala**, depois **item
equipado**. Menores em aberto: o `chance de aplicar: 75%` ao mirar, o **empurrão de medidor** como
efeito, e a **9ª pele (Humanos)**, bloqueada até o fundo de facção no compêndio. E a dívida do
`docs/RELEITURA-backend-pendente.md` continua de pé.

## Decisões desta sessão que NÃO se reabrem

- **O `docs/CONTEXTO.md` é o ÚNICO dono do estado da sessão.** Nada de uma segunda foto em memória.
  Memória é **um arquivo, um fato**; passou de ~4 KB, virou diário.
- **O mapa visual não será mantido.** Ele cumpriu o papel de orientar e foi descartado. Se um dia
  voltar, volta **GERADO por ferramenta** (como a bancada escreve o `bancada-dano.md`), nunca
  mantido à mão — mapa à mão apodrece igual ao ROADMAP, que é a doença que estamos curando.
- **No ROADMAP, item feito vence; no GDD, não.** Um descreve trabalho, o outro descreve regra.
- Os **4 ADRs já são o formato-alvo** (um assunto, um arquivo, 69 KB somados). O padrão não é novo —
  é o que o ROADMAP deixou de seguir.

## Armadilhas que morderam nesta sessão (não repetir)

- **O `gitStatus` da abertura da conversa já traz os commits recentes e o branch** — repetir
  `git log`/`git branch` é contexto jogado fora.
- **Não abrir documento inteiro por curiosidade.** `ROADMAP` = ~63k tokens (um terço da janela num
  `Read` cru); `GDD` = ~29k. **Só por seção**, com `Grep` ou `offset`/`limit`. Saudação sem tarefa =
  ler este arquivo e PARAR.
- **`Measure-Object -Line` do PowerShell engole linha vazia e subconta** — deu 1.341 num arquivo de
  1.715 linhas. Pra contar linha de verdade, contar `\n` no texto cru.
- **Ao partir GDD ou ROADMAP, mover os PONTEIROS no mesmo commit.** O `CLAUDE.md` cita
  "`GDD-progressao.md` §7" e este arquivo cita §1 e §7; é a mesma mina da renomeação de facção —
  o ponteiro quebra sem erro nenhum.
- **Memória velha mente com confiança:** a `project_estado.md` dava o Guarda como dono do
  `IReageAntesDeMorrer`, e o código mostra `Fada/Voar`. Conferir antes de gravar como fato.

## Pendência menor herdada

`memory/feedback_colaboracao.md` tem 9,3 KB — é o único arquivo de memória acima de 4 KB agora.
Vale conferir se virou diário também, na mesma passada da arrumação.

## O que o Gabriel confere, sempre

Hover, arraste, clique e tudo que acontece DURANTE a batalha. Os dois harnesses
(`ferramentas/rodar-telas.js` e `rodar-tema.js`) publicam mensagem e montam tela — **eles não clicam
em nada**. Verde deles nunca quer dizer "o jogo funciona".
