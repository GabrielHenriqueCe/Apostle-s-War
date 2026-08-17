---
name: front
description: Ler ANTES de tocar em `wwwroot/` ou na ponte C#↔JS do Apostle's War — telas (menu, perfil, arena, campanha, arsenal, compêndio, combate), `nucleo/`, `ui/`, o contrato de tela, `abrirTela`, a ponte WebView2, `IApresentacao`/`ITelaDeCombate`, ou regra que a tela parece "saber".
---

# O front

Leia `docs/MANUAL-front.md`: o mapa de `wwwroot/`, o contrato de tela (`{ cena, montar(dados,
anterior) }`), as DUAS injeções que seguram a direção da dependência, os acessadores, a ponte de
mensagens local e as 7 decisões que o #180 devolveu à camada dona.

**A regra que resume tudo:** quando um módulo interno precisa de algo do externo, o externo INJETA
ou a coisa DESCE. Nunca o interno importa pra cima.

Verificar SEMPRE (os dois passam em ~1 min):

```
node --experimental-vm-modules ferramentas/rodar-telas.js
node --experimental-vm-modules ferramentas/rodar-tema.js "" 120
```

**Verde deles não é "o jogo funciona":** eles publicam mensagem, não clicam em nada. Duplo-clique,
slot, arrastar, teclado e tudo que roda DURANTE a batalha estão fora. Na separação, quatro bugs
saíram exatamente daí, e os quatro foram achados pelo Gabriel jogando.

**Terminação de linha:** o `.gitattributes` impede (LF no repo, CRLF na cópia) e o `rodar-telas.js`
acusa. Arquivo misto não é cosmético — já grudou um `else if` num comentário.
