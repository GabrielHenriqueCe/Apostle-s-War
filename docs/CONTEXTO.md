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

**A branch `feat/forja-e-po` está no GitHub**, 22 arquivos. O Gabriel abre e mergeia; depois é a
limpeza de sempre (`git checkout main && git pull && git branch -d … && git fetch --prune`).

**O tema foi a ⚒️ FORJA e o 🧂 pó que ela gasta.** O `docs/GDD-itens.md` já tem o que ficou decidido
(a Bigorna e a recusa na parede, no §Como o nível sobe; o item por apóstolo, no §O ACERVO). **Não
repetir aqui o que está lá.**

O que precisa ser sabido e NÃO está nos docs:

- **O PR (b) é o PRÓXIMO e o desenho dele já existe:** o item vai pro apóstolo + raridade + subs + o
  filtro completo. O que o Gabriel desenhou nesta sessão está no `GDD-itens.md` §O que o item POR
  APÓSTOLO muda nas telas — as setas por apóstolo, o emoji do portador na peça, o filtro de
  equipadas pra tomar peça de aliado. **É desenho dele, não sugestão minha.**
- **Os nomes são dele e foram escolhidos um a um:** Armaria (vestir) · Bigorna (nível) · Têmpera
  (estrela) · Caldeamento (fundir). "Cadinho" foi recusado por ser o VASO e não o ato; "Refino" e
  "Destilação" perderam para o termo de ferreiro.
- **O pó já era creditado desde o #253** — o que não existia era saída de dados: nenhum DTO o levava
  pro front. Por isso ele parecia não cair.
- **O saldo de pó NÃO fica à mostra numa faixa.** Cheguei a pôr uma na coluna das peças e o Gabriel
  cortou: *"deve mostrar onde precisa, igual a alma já faz"* — ou seja, dentro das bancadas e no fim
  da fase.

## O que está no ar

1. **Os itens continuam GLOBAIS**, não por apóstolo — é o PR (b). Na Forja, a seta `‹ Arma ›` traz a
   peça vestida do slot, que hoje é a mesma pra qualquer apóstolo.
2. **Raridade e subestatísticas não existem**, então dois eixos do filtro faltam, e a ♻️ Reforja é a
   única bancada inerte da Forja.
3. **A ordenação "quanto dá" só é honesta dentro do MESMO stat** — está comentado no código; a tela
   ainda não avisa isso ao jogador.
4. **Existe batalha que NÃO TERMINA.** 169.430 ciclos medidos numa fase do capítulo 4, bot × bot.
   **Decisão do Gabriel: NÃO pôr limite de turnos; sair da batalha quando não dá pra vencer.** Item
   próprio, não começado. O teto de 60 ciclos por fase já impede o exploit de nível de item.
5. **A campanha ficou mais dura** desde o #253: todo item nasce valendo 11,5% do teto.

## Verificação em jogo — o que ainda não foi conferido

**Nada da Forja foi visto em jogo ainda.** O que mais pede olho:

- **As seis cores do 🧂.** O saleiro é quase branco, então o filtro precisa de `sepia+saturate` antes
  do `hue-rotate` (a alminha não precisa: o 🔥 já nasce saturado). O resultado depende da fonte de
  emoji do Windows.
- A Forja inteira: a previsão da Bigorna com uma peça equipada (é onde o reflexo no apóstolo
  aparece), as setas `‹ Arma ›`, e o Esc voltando pra Catedral com a peça já mudada.
- Da sessão anterior, ainda em aberto: a tela de FASE, os quatro cards de recompensa, e o que
  acontece **ao abrir com o save antigo**.

## Gotchas que continuam valendo

- **O `rodar-telas.js` estava VERMELHO na `main`** e ninguém tinha notado: o fixture mandava
  `taxaCrit`/`danoCrit` e a ficha lê `taxaCritPct`/`danoCritPct`. Corrigido aqui. **Rodar o harness
  ANTES de mexer, pra saber o que já estava quebrado** — senão a falha herdada vira a sua.
- **Tela nova precisa da MOLDURA da seção, não só do corpo.** Criei o `#forjaCorpo` e esqueci o
  `#forja`: sem `position:absolute; inset:0` + coluna flex centrada, o `<h1>` não centraliza. A Forja
  agora entra nos MESMOS seletores do `#catedral`, e não numa cópia deles.
- **Elemento novo tem de ENTRAR na lista de seletores da placa/poço** (`estilo.css`), senão vira
  caixa branca do Windows.
- **Nada de crase (`` ` ``) dentro de `node -e "..."` no bash** — vira substituição de comando e come
  o texto. Aconteceu duas vezes nesta sessão; a segunda quebrou o comando inteiro.
- **`sed -i` reescreve CRLF→LF em TODO arquivo que toca**, e arquivo NOVO escrito pela ferramenta
  nasce LF. Converter (`perl -pi -e 's/(?<!\r)\n/\r\n/'`) — quem acusa é o `rodar-telas.js`.
- **`dotnet test` reescreve o `docs/bancada-dano.md`** e cinco linhas oscilam entre corridas →
  `git checkout -- docs/bancada-dano.md`.
- O jogo ABERTO trava o build (lock do `.exe`) — pedir pra fechar antes de buildar/testar.
- **Não há Python nem `gh` CLI nesta máquina**, e a extensão do Chrome foi recusada: verificação
  visual é do Gabriel.
- **Este arquivo sobe DENTRO do PR** — a `main` local não fica à frente depois do merge.
