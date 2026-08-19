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

## Onde paramos (18/ago/2026)

**Foi uma sessão de DESENHO — zero código de jogo.** Um PR mergeado, o **#249**, só de documentação.
A `main` está em `5fd7c1e`, sem branch pendurada.

O que ela fechou: **o desenho inteiro do bloco progressão** — curva de XP, dificuldade e nível do
inimigo. Está tudo no `GDD-progressao.md`, e **as seções são a fonte, não este arquivo**:

- **§A CURVA DE XP** — o pote virou `72 × k × dificuldade`, e o critério inverteu: uma passada suave
  **não** chega ao teto; os três últimos níveis do Fácil são repetição de fase.
- **§Os inimigos não têm itens** — o nível do inimigo virou uma **reta com 2 âncoras por dificuldade**
  (8 números), calibrada por paridade de poder contra o arsenal que o `GDD-itens.md` projeta.
  `ferramentas/calibrar-inimigo.js` reproduz os oito.
- **§O TETO DE DIFICULDADE** — quem trava o nível **continua sendo o material**; não existe teto em
  código, e não é pra existir.
- **§7 O PLANO** — o passo 5 virou os **passos 5→8**, que é a ordem de codar abaixo.

**O plano de código, no detalhe de arquivo**, está em
`C:\Users\gabie\.claude\plans\sobre-a-escala-de-spicy-rossum.md` (fora do repo, sobrevive à sessão).

## O que vem — quatro PRs, nesta ordem

A ordem é load-bearing: cada estado intermediário tem de ser jogável, e **o jogador sobe antes do
inimigo** porque "ficou fácil" é intermediário aceitável e "ficou impossível" não é.

1. **`feat/composicao-por-tipo`** — as fases falam `TipoDeApostolo` em vez de `Slot`;
   `PersonagemService.ObterPorTipo`; `Tipos.Simbolo` (🛡️ ⚔️ 🏹 💗, no molde do `Faccoes.Simbolo`);
   o canto do card reservado pro emoji, com as estrelas indo ACIMA. Corrige o bug da fase 1 entregar
   dois apóstolos. Teste de guarda: toda facção tem um de cada tipo.
2. **`feat/dificuldade`** — o enum (**cujo valor É o multiplicador de XP**), a 2ª tabela de
   composição, progresso por dificuldade, desbloqueio ao fechar a 8-7, e a escolha do jogador **no
   mapa e dentro da tela da fase**. Voltar pro Fácil pra farmar é jogada legítima.
3. **`feat/xp-e-nivel`** — `Progressao` no Domain, `ProgressaoService` (chave `"progressao"`, guarda
   **só XP**; o nível é derivado), pote por inimigo morto, estrelas na ficha.
4. **`feat/nivel-do-inimigo`** — a reta e as âncoras; o `MultiplicadorFase` morre aqui.

## As minas que a sessão achou e ainda não estão em código

- **O inimigo da campanha é HOJE a mesma instância `Personagem` do roster do jogador**
  (`CombateService.ExecutarRodada` → `ObterPersonagem(capitulo, slot)`). Aplicar nível na instância
  compartilhada vaza pro inimigo **sem quebrar build**. Regra: o roster é mutado, o inimigo é CÓPIA.
- **`Arquetipos.FatorDoNivel` clampa em 60** — tem de sair, senão o inimigo nv 428 é tratado como 60,
  calado. **`Arquetipos.Velocidade` clampa também, e esse FICA**: é ele que impede o inimigo do
  Pesadelo de agir quatro vezes por turno. Os dois precisam de teste.
- **Buff de Velocidade só do lado do jogador tira as 8 âncoras do lugar** — a Bota `+50` é a única
  fonte do jogo e já está dentro da conta.

## Pendências que continuam

1. **Os sete números** (`GDD-progressao.md` §Os números que faltam) — todos de material/forja/drop, o
   bloco DEPOIS dos quatro PRs. O maior risco segue o **#5**, a demanda de alma contra 36 apóstolos.
2. **Os Humanos começando com UM escolhido** (avatar = o escolhido, é da LORE) — PR próprio, casa bem
   com a XP: começar solando é começar com o pote inteiro.
3. Menores de sempre: o **empurrão de medidor** (a `FilaDeTurnos` já tem os ganchos), a **pele da
   Arena** (FILA A #20) e a **9ª pele, Humanos** (#21, bloqueada até o fundo de facção no compêndio).

## Gotchas que continuam valendo

- **`dotnet test` reescreve o `docs/bancada-dano.md`** e cinco linhas oscilam entre corridas
  (Tiroteio, Esgrima, Shuriken, Porradeiro, Vilania). Depois de rodar a suíte sem ter mexido em
  número: `git checkout -- docs/bancada-dano.md`. **Depois dos PRs de nível, se ela mudar é vazamento
  de nível pros bonecos** — não é ruído, é bug.
- O jogo ABERTO trava o build (lock do `.exe`) — pedir pra fechar antes de buildar/testar.
- Os harnesses `ferramentas/rodar-telas.js` e `rodar-tema.js` publicam mensagem e montam tela — **eles
  não clicam em nada**. Hover, arraste e o que acontece DURANTE a batalha quem confere é o Gabriel.
- **Não há Python nesta máquina.** Splice em doc CRLF vai pelas ferramentas de edição, não por script.
