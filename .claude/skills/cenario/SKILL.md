---
name: cenario
description: Ler ANTES de criar ou mexer numa pele de facção / cenário de batalha do Apostle's War — o fundo animado de um capítulo, `body[data-tema]`, `wwwroot/cenarios/<faccao>/`, o `AR_DO_TEMA`, canvas de fundo/frente, o maestro, ladrilho de horizonte. Também vale para a pele da Arena e a 9ª pele (Humanos).
---

# Cenário de facção

**Não invente processo: já existe manual, e cada linha dele custou uma rodada de "ficou ruim" em
jogo.** Leia `docs/MANUAL-cenario.md` INTEIRO antes de desenhar — em especial a lista de ARMADILHAS
e as LIÇÕES DE DESENHO. Se for contra alguma, seja de propósito e diga por quê.

Depois de mexer, e ANTES de pedir conferência ao Gabriel:

```
node --experimental-vm-modules ferramentas/rodar-tema.js "" 120
```

Raio negativo em `arc`/`ellipse` LANÇA e congela a cena; **NaN em coordenada não lança**, só não
desenha. É isso que o harness pega, e ele já achou bug fatal em três peles seguidas.

**A conferência em jogo é do Gabriel, sempre** — quase todo acerto deste front veio de ele olhar
rodando e apontar o defeito exato. E quando ele descrever um MECANISMO, implemente LITERALMENTE.
