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

**A branch `feat/item-nivel-e-po` está no GitHub**, 31 arquivos. O Gabriel abre e mergeia; depois é a
limpeza de sempre (`git checkout main && git pull && git branch -d … && git fetch --prune`).

**O tema é o PASSO 10(a): o eixo do NÍVEL do item.** O `docs/GDD-progressao.md` §7 e o
`docs/GDD-itens.md` já estão atualizados — o passo 10 virou três PRs e o §O ACERVO nasceu. **Não
repetir aqui o que está lá.**

O que precisa ser sabido e NÃO está nos docs:

- **A ordem dos três PRs é do Gabriel:** (a) o nível ✅ · (b) o item vai pro **apóstolo** + raridade +
  subs + o filtro completo · (c) a forja. A **raridade do APÓSTOLO (passo 9) foi adiada** — ela se
  paga em emblema e não encosta em pó nem em alma, então não trava nada.
- **`Material` virou o tronco de alma e pó** ("são quase irmãos mesmo", decisão dele): a escada, a
  fusão, a diluição e a forma da receita moram lá; `Alma` e `Po` guardam só a torneira e as
  constantes. O bolso virou `CarteiraDeMaterial`, e o `AlmaService` caiu de 120 pra 25 linhas.
- **O pó tem constantes PRÓPRIAS** (50/30/20 contra 150/100 da alma) porque são **7 peças por
  apóstolo contra 1** — com o preço da alma, cada parede custaria sete vezes mais.
- **Os passos 2, 3 e 4 do §7 já estavam FEITOS em código** (a `FilaDeTurnos` roda o laço da batalha,
  o `ChanceDeColarEm` é usado pelo `AplicarDebuff`, a DEF já é `DEF/(DEF+5000)`) e o doc mostrava os
  três em aberto. Corrigido no PR.

## O que está no ar

1. **Os itens continuam GLOBAIS**, não por apóstolo — é o PR (b). Na Forja, trocar o apóstolo do meio
   não muda o equipamento.
2. **Raridade e subestatísticas não existem**, então **dois eixos do filtro faltam**: sub CONTENDO /
   SEM CONTER e raridade. Os seis que existem (conjunto · principal · nível ≥ · estrela ≥ · ordenar ·
   misturar) estão na coluna do acervo.
3. **A ordenação "quanto dá" só é honesta dentro do MESMO stat** — 57,5 de ATK contra 0,0575 de HP%
   não se comparam. Está comentado no código; a tela ainda não avisa isso ao jogador.
4. **Existe batalha que NÃO TERMINA.** Medido: 169.430 ciclos numa fase do capítulo 4, bot × bot,
   ninguém consegue matar ninguém — o laço do `ExecutarCombate` não tem limite. **Decisão do Gabriel:
   NÃO pôr limite de turnos; sair da batalha quando não dá pra vencer.** Isso é item próprio (tema de
   laço de combate) e ainda não foi feito. O teto de 60 ciclos por fase já impede o exploit de nível
   de item, mas a batalha continua pendurando o jogo.
5. **A campanha ficou mais dura**: todo item nasce valendo 11,5% do teto (a Arma dá +57 de ATK em vez
   dos +120 de antes). É o desenho, mas é a primeira coisa que se sente jogando.

## Verificação em jogo — o que JÁ foi conferido e o que não

**Conferido pelo Gabriel:** o alinhamento da ficha, a Catedral, a troca de item e o acervo.

**Não conferido:** a tela de FASE (agora promete o slot e "×4" em vez de um stat), os quatro cards de
recompensa no fim da fase, e o que acontece **ao abrir com o save antigo** — sem inventário, o acervo
se reconstrói das fases concluídas, uma peça por fase, no nível 1, e o que estava equipado se perde.

## Gotchas que continuam valendo

- **CSS: conferir o CONTAINER antes de culpar a regra.** Três bugs de layout seguidos nesta sessão
  vieram do que estava em volta, não do que eu escrevi: uma regra base depois da minha na cascata
  (`display: flex` ganhando do `grid`), o `align-items: center` do `#catedralFicha` encolhendo as
  linhas, e o `align-content: stretch` inflando as fileiras da grade.
- **Largura de grade se CONFERE NA SOMA.** A coluna do meio da Catedral tem 330px fixos (306 de
  conteúdo); colunas escritas em `em` somaram 307px, o `1fr` do rótulo colapsou e o texto transbordou
  por cima dos números. As larguras agora são px com a conta escrita no comentário.
- **Controle nativo (`<select>`, `<checkbox>`) aparece com a cara do Windows** e destoa do tema —
  usar os chips/placa do projeto. E elemento novo tem de ENTRAR na lista de seletores da placa
  (`estilo.css`), senão vira botão branco.
- **`sed -i` reescreve CRLF→LF em TODO arquivo que toca.** Prefira a ferramenta de edição; se usar
  `sed`/`perl`, confira `git diff --numstat`.
- **`dotnet test` reescreve o `docs/bancada-dano.md`** e cinco linhas oscilam entre corridas →
  `git checkout -- docs/bancada-dano.md`. Mudança ALÉM dessas cinco é vazamento de nível pros bonecos.
- O jogo ABERTO trava o build (lock do `.exe`) — pedir pra fechar antes de buildar/testar.
- **Não há Python nem `gh` CLI nesta máquina**, e a extensão do Chrome foi recusada: verificação
  visual é do Gabriel. Para renderizar front fora do jogo dá pra servir o `wwwroot` com um servidor
  node de 10 linhas (`type="module"` não roda em `file://`).
- **Este arquivo sobe DENTRO do PR**, como da última vez — a `main` local não fica à frente depois do
  merge.
