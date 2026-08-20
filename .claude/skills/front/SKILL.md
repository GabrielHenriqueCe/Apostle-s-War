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

**Verde deles não é "o jogo funciona".** O `rodar-telas.js` publica as mensagens do C#, monta cada
tela e DISPARA os gestos alcançáveis nela — clique, duplo-clique, teclado, arrastar-e-soltar e
mouse-arrasto. Continuam fora: a batalha depois do primeiro quadro (roda contra o motor C#) e o
markup ESTÁTICO do index.html, que o DOM de mentira não materializa — ouvinte registrado por
`querySelectorAll` de classe no carregamento não existe ali.

**Carga do harness é CONTRATO com o C#:** a fixture manda o DTO real (`FasesVista`,
`CompendioVista`…). Chave errada não explode, só desliga o caminho — a lista de fases nascia toda
desabilitada porque a carga dizia `liberada` e o código lê `desbloqueado`.

**Terminação de linha:** o `.gitattributes` impede (LF no repo, CRLF na cópia) e o `rodar-telas.js`
acusa. Arquivo misto não é cosmético — já grudou um `else if` num comentário.
