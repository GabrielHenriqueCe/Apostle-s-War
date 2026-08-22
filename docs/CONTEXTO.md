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

## Onde paramos (21–22/ago/2026)

A trilha de ferramenta fechou e a **fila do jogo voltou a andar**. Mergeados: **#260** (CI no GitHub
Actions, verde), **#261** (README ganhou o badge, a seção das ferramentas e a varredura da deriva) e
**#262** (o item vai pro apóstolo — o passo 10-b1). O que cada um fez está na mensagem de commit.

**O PR desta sessão está aberto e é o último a mergear**: o ⚙️ Esmeril, o bug do capítulo e a tela de
vitória. Ele carrega três temas de propósito, e isso está dito na mensagem dele.

## O que este PR entrega, e o que ele deixa pra conferir EM JOGO

- **⚙️ Esmeril** — 4ª bancada da Forja: a peça vira pó e deixa de existir. Única que destrói, única
  com confirmação, e recusa peça vestida. A ♻️ Reforja virou **⚗️ Amálgama** (segue `em breve`).
- **O bug do capítulo**: `DesbloquearFaccao` perguntava "todas as fases LIBERADAS" em vez de
  "CONCLUÍDAS", então o capítulo seguinte abria quando o jogador CHEGAVA na última fase. Visto em
  jogo pelo Gabriel (parado na 2-7 com o cap 3 aberto). Tem teste.
- **A tela de vitória**: alma e pó em colunas lado a lado, e os **seis stats sempre visíveis** em
  três colunas fixas, com `→` e cor viva só onde subiu (a lista filtrada fazia o bloco dançar entre
  uma fase e outra).
- **Catedral ⇄ Forja**: ⚒️ Forja tem porta no menu principal, e o título das duas telas virou um par
  de abas clicáveis. A aba "Catedral" NÃO é o `voltar` do Esc — entrando pela Forja do menu, voltar
  devolve ao menu, e o C# distingue as duas saídas (`irParaCatedral`).

## As duas tarefas que o Gabriel deixou nomeadas pra ESTA próxima sessão

1. **O consumo de material ao comprar ESTRELA parece barato demais perto do que ela entrega.** Ele
   observou o nível saltando de **9 pra ~12** ao pagar a estrela — os pontos/XP acumulados na parede
   viram vários níveis de uma vez, e o preço não acompanha esse salto. Conferir os dois lados (item e
   apóstolo) e propor se o custo passa a subir um pouco COM O NÍVEL. Vale pra `Po.Receita` e
   `Alma.Receita` (o tronco é o `Material.Receita`).
2. **Reorganizar o que a lista de vitória mostra** — provavelmente **tirar os status e deixar só o
   nível**. É a segunda passada no mesmo assunto: nesta sessão os stats passaram a aparecer sempre, e
   olhando em jogo ficou informação demais.

## O que está no ar

1. **Nada desta sessão foi visto em jogo pelo Gabriel** além do bug do capítulo e das cores: o
   vínculo item↔apóstolo (#262), o ✕ Remover, o fluxo novo da Armaria, o filtro na Forja, o Esmeril,
   as abas de título e a tela de vitória.
2. **O b2 — raridade e subestatísticas.** É o próximo passo grande do item (GDD-progressao §7,
   10-b2). `Item.Raridade` já existe com default `Comum` (o Esmeril paga por faixa); falta o drop
   sortear dentro do teto da dificuldade, as subs, a ⚗️ Amálgama e a 💎 Raridade da Catedral.
3. **A ordenação por valor no acervo.** O comentário que MENTIA sobre ela morreu no #262; falta a
   decisão: a tela passa a avisar que comparar `valorNum` entre stats diferentes não diz nada, ou
   não avisa? Hoje ela só é honesta com o Principal escolhido no filtro.
4. **A batalha que não termina** (169.430 ciclos medidos) — decisão do Gabriel: sair da batalha, sem
   limite de turnos. Não começado.
5. **FILA A #14** — o teste da ordem crítica de morte, que destrava com uma tela no-op sobre
   `ITelaDeCombate`.
6. **Skill `depurar`** (o método de 4 fases aterrado nas armadilhas daqui) — planejada, não começada.
7. **Statusline:** `effort.level` e `fast_mode` existem no payload e a linha ignora. ~6 linhas em
   `~/.claude/statusline.js`, fora do repo.
8. **Buraco conhecido do harness:** o DOM de mentira só materializa o que o JS pede, então
   `querySelectorAll('.setupJog')` do `arena.js` segue vazio e aqueles ouvintes nem são registrados.
   Fechar = construir a árvore estática do `index.html`. Está no cabeçalho do `rodar-telas.js`.
9. **Os ~23 gestos ∅ do `rodar-telas.js` não foram auditados um a um.** ∅ = o handler rodou e não
   tocou no DOM. A maioria é legítima, mas isso é impressão, não conferência.
10. **Dois nomes citados em doc que NUNCA foram código** — `ControladorAutomatico`
    (`GDD-expansao.md`) e `AdicionarBonusHPPermanente` (`ROADMAP-refatoracao.md`). Saem na seção de
    leitura do `conferir-docs.js`; ninguém checou se são proposta ou typo.

## Gotchas que continuam valendo

- **Rodar os três antes de mexer E antes de commitar** (§Comandos do `CLAUDE.md`). O CI roda os
  mesmos três num Windows limpo, e o PR mostra o resultado.
- **Arquivo NOVO escrito por ferramenta nasce LF** e o `rodar-telas` derruba por terminação mista —
  converter pra CRLF ao criar (aconteceu com o `ui/filtro.js` nesta sessão).
- **`sed -i` reescreve CRLF→LF em TODO arquivo que toca.** Preferir `node -e` com
  `replace(/\r?\n/g,'\r\n')` no fim.
- **Nada de crase (`` ` ``) dentro de `node -e "..."` no bash** — vira substituição de comando.
- **Heredoc `<<'FIM'` no bash falhou** nesta sessão ao escrever arquivo grande; o caminho que
  funciona é a ferramenta Write (e converter pra CRLF depois).
- **Sabotagem para provar ferramenta: restaurar por substituição INVERSA, nunca `git checkout --`.**
- O jogo ABERTO trava o build (lock do `.exe`) — pedir pra fechar antes de buildar/testar.
- **Não há Python nem `gh` CLI nesta máquina.** O PR o Gabriel abre e mergeia no GitHub web; depois
  eu limpo (`checkout main` → `pull` → `branch -D` → `fetch --prune`), e **confiro na `origin/main`
  antes de apagar a branch** — squash-merge sempre parece "não mergeado" pro git.
- **Ao reescrever ESTE arquivo, auditar item por item o que estava "no ar" antes.** Item só sai
  daqui quando foi PROVADO resolvido ou mudou de casa, e as duas coisas se provam abrindo o alvo.
