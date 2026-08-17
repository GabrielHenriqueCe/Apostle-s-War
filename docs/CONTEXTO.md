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

**Branch `docs/arrumacao-de-contexto`, 7 commits, working tree limpo. NÃO mergeada — o PR é do
Gabriel.** A sessão não tocou regra de jogo: foi a arrumação dos documentos inteira, os 4 passos do
plano `~/.claude/plans/o-ctx-apenas-dando-pure-pearl.md`, que agora está **cumprido**.

**O que mudou de tamanho:**

| | antes | depois |
|---|---:|---:|
| `ROADMAP-refatoracao.md` | 2.849 l (~63k tok) | **936 l (~20k)** |
| `GDD-progressao.md` | 1.715 l | **476 l** (+ combate 584 + itens 715) |
| `CLAUDE.md` | 253 l | **137 l** |
| memória (maior arquivo) | 9,3 KB | **< 4 KB** |

**Os documentos novos:** `MANUAL-cenario.md` (como se faz uma pele) · `MANUAL-combate.md` (as regras
vivas do motor) · `MANUAL-front.md` (a ponte, o contrato de tela, as camadas) · `GDD-combate.md`
(§1+§2) · `GDD-itens.md` (§4). Mais **quatro skills** em `.claude/skills/` — cenario, front, combate,
progressao — que carregam o manual do assunto **pela tarefa**, em vez de por eu lembrar de ler.

**A regra que guiou os três cortes do ROADMAP:** pendente fica na fila · **referência viva** vai pro
doc do assunto · **histórico sai** (o `git log` guarda melhor). Cortar por `✅ feito` × `pendente`
teria perdido as armadilhas, que nasceram de itens já feitos.

## O que fica pendente desta arrumação

1. **O PR.** Sete commits esperando o Gabriel abrir e mergear.
2. **A prova do passo 4 é a PRÓXIMA sessão:** abrir com "oi" e comparar a % de contexto com os 28%
   que dispararam tudo isto.
3. **A faxina de comentários (#15) segue aberta** e agora sem desculpa: o `jogo.js` já foi separado,
   que era o pré-requisito que o próprio Gabriel pôs na frente dela.

## O trabalho de JOGO, que volta a valer agora

A ordem do `GDD-progressao.md` §7 não é negociável: **nível (curva do tipo) + raridade** → **raridade
→ passiva que escala** → **item equipado**. Menores em aberto: o `chance de aplicar: 75%` ao mirar, o
**empurrão de medidor** como efeito, a **pele da Arena** (item 20 da FILA A) e a **9ª pele, Humanos**
(item 21, bloqueada até o fundo de facção no compêndio). A dívida do
`docs/RELEITURA-backend-pendente.md` continua de pé.

## Decisões desta sessão que NÃO se reabrem

- **Instrução de assunto chega por SKILL, não por ponteiro no `CLAUDE.md`.** Ponteiro põe em mim a
  decisão de ler, e ela já falhou. Mesmo princípio das duas injeções do front: quem sabe ENTREGA.
- **O §Revogado do GDD ficou** (o plano mandava apagar). A forma dele parece história, mas a função é
  impedir que decisão morta seja reaberta — corta-se a moral, nunca o fato e a consequência.
- **A numeração dos §s do GDD ficou FURADA de propósito** (§1 §2 num arquivo, §3 §5 §6 §7 noutro, §4
  no terceiro). Renumerar quebraria dezenas de referências cruzadas, cada uma em silêncio. No lugar,
  os três arquivos abrem com a mesma bússola dizendo qual § mora onde.
- **Mapa de código escrito à mão não se mantém** — o mapa das 36 passivas por interface saiu do
  ROADMAP porque envelhece MENTINDO, e o código responde por `grep`.

## Armadilhas que morderam nesta sessão (não repetir)

- **Ponteiro quebrado não dá erro nenhum**, e quatro deles estavam em CÓDIGO (`FilaDeTurnos.cs`,
  `Enums.cs`, `Arquetipos.cs`, `FilaDeTurnosTests.cs` citavam "GDD-progressao §1/§2"). Ao mover
  qualquer seção, o `grep` pelos ponteiros vai no MESMO commit.
- **A ferramenta `Write` grava LF puro**, e este repo é CRLF na cópia de trabalho — deu **1 LF puro**
  no `CLAUDE.md`, que é exatamente a mistura que já grudou um `else if` num comentário. Conferir
  sempre: `(match /\n/) − (match /\r\n/)` tem de dar 0.
- **`python` não existe nesta máquina** (só o stub da Microsoft Store). Script de apoio é `node`.
- **O `cd` do Bash PERSISTE entre chamadas** — um `cd docs` deixou o `git add docs/CONTEXTO.md`
  falhando por caminho duplicado.
- **`dotnet test` reescreve o `docs/bancada-dano.md`**, e cinco linhas OSCILAM entre corridas (é
  comportamento, ver #235). Depois de rodar a suíte sem mexer em número, `git checkout --` nele.

## O que o Gabriel confere, sempre

Hover, arraste, clique e tudo que acontece DURANTE a batalha. Os dois harnesses
(`ferramentas/rodar-telas.js` e `rodar-tema.js`) publicam mensagem e montam tela — **eles não clicam
em nada**. Verde deles nunca quer dizer "o jogo funciona".
