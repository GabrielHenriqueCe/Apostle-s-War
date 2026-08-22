---
name: combate
description: Ler ANTES de mexer no motor de combate do Apostle's War — pipeline de dano, `ReceberDano`, mitigação, escudo/bloqueio/proteção de aliado, natureza do dano e ignorar-status, reações e passivas (`IReageAo*`), ordem de morte e revive, turno e barra de velocidade, o `ControladorBot`, ou a bancada de dano.
---

# O motor de combate

Leia a seção do assunto em `docs/MANUAL-combate.md` — são as regras que continuam valendo depois de
implementadas, e que o código sozinho não diz. As mais caras:

- **Ordem do pipeline de dano:** quem REDUZ de graça roda antes de quem GASTA recurso
  (`OrdemDeMitigacao`, sem default de propósito). `ReceberDano` e `PreverDanoRecebido` ordenam pelo
  MESMO helper — se divergirem, o bot mira errado em silêncio.
- **A ordem crítica de morte.** Trocá-la não quebra o build: muda quem vive.
- **Ignorar status é uma língua só, a da LISTA** (`NaturezaDano.Ignora`), com 1 gate no `ReceberDano`.
- **Os 3 contextos** — `ContextoCombate` (habilidades) × `ContextoReacao` (reações) × `EventoDano`
  (o golpe). Não confundir.

O DESENHO de cada decisão está nos `docs/ADR-*.md`; o vocabulário de Ações pra reusar está no
`docs/CATALOGO-de-acoes.md` (ler antes de criar habilidade nova — verbo compartilhado primeiro,
bespoke só no 2º cliente).

**Números que mudarem: rodar a bancada** — `$env:BANCADA=1; dotnet test`. Ela é OPT-IN: o `dotnet test`
comum a pula, e sem a variável nenhum número novo sai. Escreve `docs/bancada-dano.md`, versionado, e o
`git diff` dele é o relatório. O jogo ABERTO trava o build; pedir pra fechar antes.

**Combate RODA headless** — a tela é porta, e uma no-op dá a batalha inteira em memória. O molde é o
`Tests/OrdemDeMorteTests.cs`: tela que ESCUTA (as mensagens de passiva saem na ordem das reações) +
controlador que fecha o horizonte em N golpes (`null` → `BatalhaAbortada`). Determinismo sem semear
`Random`: dano muito acima do HP do alvo. A verificação em JOGO segue sendo do Gabriel — nenhum teste
vê pixel, animação nem clique.
