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

## Onde paramos (17/ago/2026)

**DUAS branches empilhadas, nenhuma mergeada — e a ordem importa:**

1. **`docs/arrumacao-de-contexto`** (7 commits) — a arrumação dos documentos. **O PR não foi aberto**;
   o Gabriel disse que estava com problema no git.
2. **`chore/faxina-de-comentarios`** (7 commits) — sai de CIMA da 1ª, de propósito e a pedido dele:
   assim, quando a de baixo mergear, esta não conflita. **Mergear na ordem.**

Empilhar branch contraria o *"uma branch por vez"* do `CLAUDE.md`. Foi decisão dele, com motivo dito.

## A faxina de comentários (#15) — FEITA, as duas metades

O front já fora fechado pela medição; **o C# nunca tinha sido faxinado**. Agora foi: 239 arquivos
medidos, **59 tocados**, 7 commits por camada, `dotnet build` limpo e **198 testes verdes** o tempo
todo. O resultado por categoria está no `ROADMAP-refatoracao.md` §Faxina — não se repete aqui.

**O que a próxima sessão precisa não reabrir:**

- **O número "138 linhas de narração histórica" era INFLADO.** O filtro conta `morto/morreu`, que
  neste jogo é vocabulário do DOMÍNIO (`EstadoAlvo.Mortos`, o estado `Morto`, a família do revive).
  Com marcadores fortes só, o estoque real era **36 no C# e 41 no `wwwroot`**.
- **`nucleo/` e `ui/` não perderam uma linha**, e as ~28 linhas restantes nos cenários FICAM: são
  mina (a esfera violeta do Reino some se virar azul) ou âncora de número (o "58% do vão").
- **O saldo é ~zero linha** (215 inserções contra 214 remoções) e isso é o esperado: o trabalho foi
  reescrever no PRESENTE o que estava no passado, não apagar.

## Pendências

1. **Os dois PRs**, na ordem acima.
2. **O trabalho de JOGO volta a valer** — `GDD-progressao.md` §7, na ordem: **nível (curva do tipo) +
   raridade** → **raridade → passiva que escala** → **item equipado**. Menores: o `chance de aplicar:
   75%` ao mirar, o **empurrão de medidor**, a **pele da Arena** (FILA A #20) e a **9ª pele, Humanos**
   (#21, bloqueada até o fundo de facção no compêndio). A dívida do
   `docs/RELEITURA-backend-pendente.md` segue de pé.

## Armadilhas que morderam nesta sessão (não repetir)

- **Carregar a skill `claude-api` custa ~100k tokens** — uma pergunta sobre preço comeu 20% da janela
  de 5 horas. Ela é o caminho certo quando a resposta precisa de número exato, mas é decisão cara e
  dá pra prever antes.
- **A ferramenta `Edit` grava LF puro em arquivo CRLF**, e o `-replace` do PowerShell reescreve o
  arquivo INTEIRO em LF. O `git diff --numstat` denuncia (arquivo todo alterado em vez de 2 linhas);
  a conferência é `(match /\n/) − (match /\r\n/) == 0`, e vale rodar em TODOS os tocados antes de
  commitar.
- **Comentário pode estar no MEMBRO ERRADO** — dois `<summary>` empilhados deixavam o vizinho sem doc
  e descreviam o método errado (`Queima.Detonar`, um teste do bot). Acha-se por grep:
  `</summary>` seguido de `<summary>`.
- **`dotnet test` reescreve o `docs/bancada-dano.md`** e cinco linhas OSCILAM entre corridas. Depois
  de rodar a suíte sem mexer em número: `git checkout -- docs/bancada-dano.md`.

## O que o Gabriel confere, sempre

Hover, arraste, clique e tudo que acontece DURANTE a batalha. Os dois harnesses
(`ferramentas/rodar-telas.js` e `rodar-tema.js`) publicam mensagem e montam tela — **eles não clicam
em nada**. Verde deles nunca quer dizer "o jogo funciona".
