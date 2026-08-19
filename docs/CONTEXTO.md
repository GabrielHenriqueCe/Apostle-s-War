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

**Uma sessão de código, um PR grande esperando merge.** A branch **`feat/alma-e-estrela`** está no
GitHub, 31 arquivos, ~2.400 linhas. O Gabriel abre e mergeia; depois é a limpeza de sempre
(`git checkout main && git pull && git branch -d … && git fetch --prune`).

**O tema é a ALMA e a estrela comprada** — o modelo, os números e a tela. O `docs/GDD-progressao.md`
está atualizado com tudo (§A ESTRELA, §O MATERIAL, §O PEDÁGIO, §O TETO DE DIFICULDADE) e três dos
sete "números que faltam" fecharam. **Não repetir aqui o que está lá.**

O que precisa ser sabido e NÃO está no GDD:

- **A estrela agora é a COMPRA e o nível deriva dela** (`teto = 10 × estrelas + 9`). O único teto de
  nível do jogo passou a ser esse — o Fácil para no 30 porque a ★★★★ cobra Épico, sem uma linha de
  código dizendo "teto".
- **Nasceu a CATEDRAL**, a tela onde o apóstolo se aprimora, com quatro estações: 🎒 **Forja**
  (itens) · ⬆️ **Santuário** (nível, queimando alma) · ★ **Altar** (estrela) · 🔥 **Oferenda**
  (fundir alma). O antigo Arsenal virou ela; `arsenal.js` virou `catedral.js` e os ids acompanharam.
- **Peças novas de `ui/` que a próxima tela deve REUSAR em vez de recriar:** `quantidade.js` (a barra
  `0 ——[47]—— 150`, e a regra do Gabriel é *"sempre que for pra aumentar número, é esta peça"*),
  `alma.js` (o 🔥 tingido por raridade), `ficha.js` (os painéis de stat/habilidade, compartilhados
  com o compêndio) e o `contar()` do `animacao.js`.
- **O `ArsenalService` manteve o nome de propósito** — ele é o ACERVO de equipamento; a Forja é a
  estação onde ele se usa. Está comentado no código.

## O que está no ar

1. **Nada do PR foi visto rodando.** O harness monta as telas e clica os botões das estações, mas
   não julga layout, cor nem animação. **A lista de verificação em jogo é a maior dívida desta
   sessão** — ver abaixo.
2. **Os itens continuam GLOBAIS**, não por apóstolo (é o passo 10 do §7). Na Catedral, trocar o
   apóstolo do meio não muda a Forja — pode parecer bug antes de parecer "ainda não existe".
3. **A 💎 Raridade é um botão desabilitado** (passo 9). O lugar está reservado, a mecânica não existe.
4. **O pó e a forja de item não existem**: o `Item` de hoje não tem NÍVEL pra o pó pagar. É o que
   trava o outro meio do §O MATERIAL.
5. **A mudança do Mago é do Gabriel** e pegou carona no PR: a Bola de Fogo virou `AreaDeEfeito`. Está
   correta e **não muda número nenhum hoje** — o `TipoAtaque` só governa quantas vezes o
   `DepoisDeAtacar` dispara, e a única passiva dele é `IModificaDanoCausado`.

## Verificação em jogo que ainda não aconteceu

- A Catedral inteira em 1920×1080 e em janela menor: as 4 colunas (`518 · 330 · 172 · 330`), o elenco
  em grade de 4, e se ainda precisa rolar a página.
- **As seis cores da alminha** — o roxo do épico e o azul do raro são os dois suspeitos de ficarem
  parecidos no fundo escuro.
- Subir alguém até o **9**: a XP continua entrando com o nível parado, e a ★ só libera com a **barra
  cheia** (não é chegar no 9, é encher).
- A barra de XP do fim de fase atravessando nível (enche · zera · enche) e o ritmo dela — 900ms de
  enchimento e 140ms de cascata são chute meu, ficam em duas constantes no topo do bloco.
- **Reabrir com o save que já existe: ninguém pode ter caído pro nível 9** (a migração do `Estrelas`
  nulo).

## Gotchas que continuam valendo

- **`sed -i` reescreve CRLF→LF em TODO arquivo que toca**, não só nos que casam com o padrão — sujou
  39 arquivos sem uma linha de conteúdo diferente. Prefira a ferramenta de edição; se usar `sed`,
  confira com `git diff --name-only` (conteúdo) contra `git status --porcelain` (sujos) e restaure a
  diferença.
- **`dotnet test` reescreve o `docs/bancada-dano.md`** e cinco linhas oscilam entre corridas
  (Tiroteio, Esgrima, Shuriken, Porradeiro, Vilania) → `git checkout -- docs/bancada-dano.md`.
  Mudança ALÉM dessas cinco é vazamento de nível pros bonecos.
- **O harness das telas precisa da flag**: `node --experimental-vm-modules ferramentas/rodar-telas.js`.
  Ele agora também CLICA nas portas da Catedral e nos botões que o painel abre.
- O jogo ABERTO trava o build (lock do `.exe`) — pedir pra fechar antes de buildar/testar.
- **Este arquivo subiu DENTRO do PR desta vez** (o Gabriel pediu), então a `main` local não deve
  ficar à frente depois do merge.
- **Não há Python nesta máquina**, e não há `gh` CLI: o PR é o Gabriel quem abre e mergeia.
