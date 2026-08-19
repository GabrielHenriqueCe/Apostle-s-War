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

## Onde paramos (19/ago/2026)

**Duas sessões de CÓDIGO, dois PRs mergeados** — a `main` está em `7e67053`, sem branch pendurada.

- **#250 `feat/composicao-por-tipo`** — as fases da campanha falam PAPEL (`TipoDeApostolo`) em vez de
  `Slot`, o tipo virou primeira classe (`Tipos.Simbolo`: 🛡️ ⚔️ 🏹 💗) e a fase 1 voltou a entregar
  **um** apóstolo. O `Slot` continua sendo identidade de save; ele só deixou de significar papel.
- **#251 `feat/progressao`** — o bloco 5→8 do `GDD-progressao.md` §7 INTEIRO: dificuldade, XP/nível do
  jogador e nível do inimigo. **Os passos 5 a 8 estão marcados ✅ no §7**, com o registro de por que os
  três últimos saíram num PR só.

**A ordem de estreia do capítulo mudou no #250, e foi decisão do Gabriel:** a fase 1 abre com o
**Guardião** (era o Combatente no desenho escrito), e a ordem virou `G → C → A → S` — a mesma do
roster. O GDD §5 já está corrigido.

## O que está no ar (e é sabido)

**O jogo está jogável e desbalanceado de propósito.** O Gabriel aceitou os dois buracos: *"se não
tiver como passar, o bloqueio seria 'sou fraco demais' — e vem atualização por aí"*.

1. **Não existe PEDÁGIO**, então o único teto de nível é o 60: dá pra farmar o Fácil até lá e entrar
   no Normal forte demais.
2. **Não existe a grade de itens do `GDD-itens.md`**, e o Pesadelo pede um inimigo nv 428 contra um
   jogador sem arsenal de verdade.

Os dois somem com o **material + pedágio + forja**, que é o bloco natural a seguir.

**O save de campanha foi descartado** (formato novo da chave `"save"`). Perfil e itens sobrevivem;
capítulos e apóstolos voltam do zero na primeira vez que ele abrir o jogo.

## O que vem — as duas escolhas na mesa

1. **Material, pedágio e forja** — fecha os dois buracos acima, e é o que o `GDD-progressao.md`
   §O MATERIAL / §O PEDÁGIO já descreve. Antes dele, os **sete números que faltam** (§Os números que
   faltam) precisam ser fechados; o maior risco segue o **#5**, a demanda de alma contra 36 apóstolos.
2. **Raridade + a passiva que escala** — o passo 9 do §7. O desenho está fechado (§O EMBLEMA e
   §Raridade → habilidade): emblema por fechar a fase 7 numa dificuldade, oferta = demanda = 60 por
   facção, e o efeito é **uma passiva por apóstolo** que ganha degraus, no molde do Piromancer.

Menores de sempre: os **Humanos começando com UM escolhido** (PR próprio, casa com a XP), o
**empurrão de medidor** (a `FilaDeTurnos` já tem os ganchos), a **pele da Arena** (FILA A #20) e a
**9ª pele, Humanos** (#21, bloqueada até o fundo de facção no compêndio).

## Verificação em jogo que ainda não aconteceu

Nada do #251 foi visto rodando — os harnesses publicam mensagem e montam tela, **não clicam em nada**.
O que precisa do Gabriel:

- **A barra de nível dentro do slot de 68px** durante o arraste: é o único lugar apertado de verdade.
- A troca de dificuldade **no mapa e dentro da tela da fase**, e o Normal abrindo ao fechar a 8-7.
- O card do inimigo mostrando `nv 64` sem trilho, e a XP aparecendo **também na derrota**.

## Gotchas que continuam valendo

- **A `main` local costuma ficar 1 commit à frente** por causa deste arquivo (ele é commitado local e
  sobe junto no PR seguinte). Depois do squash-merge, `git pull --ff-only` recusa. **Antes de qualquer
  reset:** `git diff main origin/main` — se o delta for só o PR mergeado, `git reset --hard
  origin/main` é seguro. Pelo mesmo motivo, `git branch -d` recusa a branch mergeada por squash;
  provar com `git diff --stat main <branch>` vazio e usar `-D`.
- **`dotnet test` reescreve o `docs/bancada-dano.md`** e cinco linhas oscilam entre corridas
  (Tiroteio, Esgrima, Shuriken, Porradeiro, Vilania) → `git checkout -- docs/bancada-dano.md`.
  **Agora que o nível existe, mudança ALÉM dessas cinco é vazamento de nível pros bonecos.**
- **As ferramentas de edição gravam LF** e o repo trabalha em CRLF; o `rodar-telas.js` acusa arquivo
  misto (só em `wwwroot/`). Depois de mexer, normalizar os arquivos tocados.
- O jogo ABERTO trava o build (lock do `.exe`) — pedir pra fechar antes de buildar/testar.
- **Não há Python nesta máquina**, e não há `gh` CLI: o PR é o Gabriel quem abre e mergeia.
