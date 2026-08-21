# CONTEXTO — o estado vivo entre uma sessão e a outra

> **O QUE ESTE ARQUIVO É.** O resumo comprimido da última sessão de trabalho: onde paramos, o que foi
> decidido e o que vem. Ele é **SUBSTITUÍDO** a cada sessão, não acrescentado — é uma FOTO do agora,
> não um diário. Quem quer histórico tem o `git log`, que guarda melhor e datado.
>
> **COMO USAR (Claude):** ler no início de toda sessão, logo depois do `CLAUDE.md` — e **não abrir os
> documentos que ele CITA**. Ao fim da sessão, reescrever este arquivo do zero com o estado novo.
>
> **O QUE NÃO ENTRA AQUI:** o que já está escrito em outro lugar. Aqui ficam só **ponteiros e o que
> ainda está no ar**.

---

## Onde paramos (20/ago/2026)

**A sessão inteira foi TRILHA DE FERRAMENTA**, não jogo. Três PRs mergeados — **#255** (o
`rodar-telas.js` passou a disparar gestos), **#256** (`conferir-docs.js` + a regra nova no
`CLAUDE.md`) e **#257** (a carga do harness conferida contra o DTO do C#). O que cada um fez está na
mensagem de commit; **não repetir aqui**.

O que nasceu disso e vale saber: as ferramentas acharam **~12 derivas**, e todas nasceram no mesmo
instante — rename, move, delete ou conclusão. Nenhuma nasceu de escrever mal. Daí a regra do
`CLAUDE.md` §Como trabalhamos ser *varrer no MESMO PR*, e não "escrever com mais cuidado".

**Este arquivo foi commitado DIRETO NA MAIN, sem PR** — pedido do Gabriel.

## A DECISÃO EM ABERTO — a bancada de dano

É o assunto que a sessão terminou no meio, e é design de instrumento, não encanamento.

**O que foi medido (20/ago/2026):**

| | tempo | escreve no repo? |
|---|---:|---|
| os 331 testes de verdade | **434 ms** | não |
| a bancada (1 `[Fact]`) | **83 s** | sim, 28 linhas por corrida |

Ou seja: **99,5% do `dotnet test` é gerar relatório**, e a bancada não testa nada — os dois `Assert`
dela são `36 == count` e `dir != null`. Se o motor de dano quebrasse, ela passaria verde.

**O ruído entre duas corridas:** 28 linhas mudam, 48 números — 6 acima de 1%, 4 acima de 3%, o maior
**3,92%**. Consequência: o `bancada-dano.md` commitado é UMA jogada de dado apresentada como o número
do motor, e o instrumento **não enxerga uma mudança de balanceamento de 3%** — o ruído é maior que o
sinal.

**Os três movimentos propostos, e onde cada um parou:**

1. **Tirar a bancada do `dotnet test`** — via um `FatoDaBancadaAttribute : FactAttribute` que põe
   `Skip` quando `BANCADA != 1` (xUnit aqui é 2.9.3, não tem `Assert.Skip`). `dotnet test` vira
   434 ms e árvore limpa. **Ganho puro, não depende de decisão nenhuma. NÃO COMEÇADO.**
2. **Solta, a bancada pode ficar cara** — `Repeticoes` sai de 10; 100 custam ~14 min, irrelevante
   pra algo que se roda ao mexer em balanceamento. Encolhe o ruído por 1/√N.
3. **Aí o CI pode regerar e falhar se o commitado for diferente** — o relatório fica incapaz de
   mentir, igual ao que o `conferir-docs` faz com os caminhos.

**A parte que é decisão do Gabriel, e reabre o #235:** repetição ENCOLHE o ruído, nunca ZERA — o diff
segue sujo. Zerar exige **rodízio pelas 4 casas** em vez de sortear uma, nas habilidades de alvo
aleatório. O #235 descartou semear o RNG (*"travar o dado esconderia o que ela mede"*); o argumento a
favor do rodízio é que o multiplicador por casa é propriedade do DESENHO, e a média sobre as casas é
o número procurado — sortear é só um jeito impreciso de estimá-la. **Contraria decisão dele; a
palavra é dele.**

## O que está no ar

1. **CI — combinado, não começado.** Repo é PÚBLICO → Actions de graça, inclusive `windows-latest`
   (obrigatório: a `Presentation` é `net10.0-windows`). Rodaria `dotnet build`, `dotnet test` e os
   três harnesses. **Depende do movimento 1 acima** — CI não deve rodar um relatório de 83 s que suja
   a árvore. CD tem alvo já desenhado no `CLAUDE.md` (`dotnet publish` → `.exe` na Release).
2. **Skill `depurar`** (o método de 4 fases aterrado nas armadilhas daqui) — planejada, não começada.
3. **Statusline:** `effort.level` e `fast_mode` existem no payload e a linha ignora. ~6 linhas em
   `~/.claude/statusline.js`, fora do repo. *Block timer já existe; uso semanal por modelo o payload
   não traz.*
4. **Buraco conhecido do harness:** o DOM de mentira só materializa o que o JS pede, então
   `querySelectorAll('.setupJog')` do `arena.js` (no carregamento) segue vazio e aqueles ouvintes nem
   são registrados. Fechar = construir a árvore estática do `index.html`. Está escrito no cabeçalho
   do `rodar-telas.js`.
5. **A fila do JOGO, parada desde que a trilha começou:** o **PR (b)** — item por apóstolo +
   raridade + subs + o filtro completo, desenho do Gabriel no `GDD-itens.md`; a **batalha que não
   termina** (169.430 ciclos medidos, decisão dele: sair da batalha, sem limite de turnos); e o
   **FILA A #14**, o teste da ordem crítica de morte, que destrava com uma tela no-op sobre
   `ITelaDeCombate`. Enquanto raridade e subestatísticas não existirem, a **♻️ Reforja segue inerte**
   (`forja.js:152`, botão `'em breve'`) — é a única bancada da Forja sem função.
6. **A ordenação por valor no acervo, e um comentário que MENTE sobre ela.** Comparar `valorNum`
   entre stats diferentes não diz nada (57,5 de ATK contra 0,0575 de HP%), então a ordem só é honesta
   com o filtro de stat ligado. O comentário em `catedral.js:535` afirma que *"a lista avisa isso"* —
   **e não avisa: não existe esse texto em tela nenhuma.** Duas coisas a resolver, e a segunda antes
   da primeira: apagar a mentira do comentário, e decidir se a tela passa a avisar.
7. **Dois nomes citados em doc que NUNCA foram código** — `ControladorAutomatico`
   (`GDD-expansao.md`) e `AdicionarBonusHPPermanente` (`ROADMAP-refatoracao.md`). Saem na seção de
   leitura do `conferir-docs.js`. São proposta não construída ou typo; ninguém checou qual.
8. **Os ~23 gestos ∅ do `rodar-telas.js` não foram auditados um a um.** ∅ = o handler rodou e não
   tocou no DOM nem avisou o C#. A maioria é legítima (o `mouseenter` de prévia sem nada pra prever,
   o `#setupLutar` desabilitado sem time), mas isso é uma impressão, não uma conferência.

## Verificação em jogo — o que ainda não foi conferido

**Nada da Forja foi visto em jogo ainda**, e agora tem mais: as seis cores do 🧂, a Forja inteira
(previsão da Bigorna, as setas `‹ Arma ›`, o Esc voltando pra Catedral), a tela de FASE, os quatro
cards de recompensa, e o que acontece **ao abrir com o save antigo**.

## Gotchas que continuam valendo

- **Rodar os três antes de mexer:** `node --experimental-vm-modules ferramentas/rodar-telas.js`,
  `ferramentas/rodar-tema.js "" 120` e `node ferramentas/conferir-docs.js`. O `rodar-telas` já esteve
  vermelho na `main` sem ninguém notar — falha herdada vira a sua.
- **Sabotagem para provar ferramenta: restaurar por substituição INVERSA, nunca `git checkout --`.**
  Ele volta pro HEAD e leva junto a edição não commitada do mesmo arquivo — apagou três edições do
  `CLAUDE.md` nesta sessão.
- **`split('\n')` em arquivo CRLF deixa o `\r` no fim da linha, e o `$` do regex não casa.** Foi assim
  que o strip de comentário do parser de DTO silenciosamente não fez nada. Usar `split(/\r?\n/)`.
- **Nada de crase (`` ` ``) dentro de `node -e "..."` no bash** — vira substituição de comando. Script
  que precisa de crase vai pra arquivo.
- **`sed -i` reescreve CRLF→LF em TODO arquivo que toca**, e arquivo NOVO escrito pela ferramenta
  nasce LF. Converter e conferir; quem acusa é o `rodar-telas.js`.
- **`dotnet test` reescreve o `docs/bancada-dano.md`** → `git checkout -- docs/bancada-dano.md`.
  (É exatamente o que o movimento 1 acaba.)
- O jogo ABERTO trava o build (lock do `.exe`) — pedir pra fechar antes de buildar/testar.
- **Não há Python nem `gh` CLI nesta máquina**, e a extensão do Chrome foi recusada: verificação
  visual é do Gabriel.
- **Ao reescrever ESTE arquivo, auditar item por item o que estava "no ar" antes.** Nesta sessão eu
  reescrevi do zero e sumi com três: a ordenação do acervo (que segue aberta), a Reforja inerte e o
  11,5% do teto. Só o 11,5% podia sair mesmo — ele passou a morar no `Equipamento.cs`. Foto nova não
  é licença pra esquecer: item some daqui quando FOI RESOLVIDO ou quando mudou de casa, e as duas
  coisas se PROVAM abrindo o alvo, não se presumem.
