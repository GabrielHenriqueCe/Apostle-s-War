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

## Onde paramos (18/ago/2026)

**O passo 4 do `GDD-progressao.md` §7 fechou inteiro**, e com ele a dívida de releitura do back-end.
Três PRs mergeados nesta sessão: **#245** (o 🧲 na tela), **#246** (`Combat/Capacidades/`) e **#247**
(a Velocidade por estrela). A `main` está em `fcc314f`, sem branch pendurada.

**O próximo é o passo 5.** Não há decisão de modelo travando ele.

## O passo 5, e ele são dois PRs

- **5a — nível + XP.** Metade já está no repo: `Personagem.Nivel`, `Arquetipos.FatorDoNivel` e a
  ficha na tela. Falta o que faz o número **andar** — a XP como pote dividido por quem está em campo,
  caindo por inimigo morto, sem banco — mais o save e a tela.
- **5b — raridade como DADO.** Só o eixo: campo, save, cor na ficha. O efeito é o passo 6.

**Consequência aceita:** sem o pedágio (que é material, e material é o bloco DEPOIS do §7), o nível
sobe livre até 60 — **o teto por dificuldade não existe até a forja chegar**. Está na ordem do GDD.

## Decisões desta sessão

- **A Velocidade anda na ESTRELA, +2 por estrela** (2·4·6·8·10·12, +12 do nv 1 ao 60). É o único stat
  em degrau. **O inimigo escala pela mesma regra e trava no 60** — a vantagem de ~3 turnos para 1 no
  fim da progressão é do **ITEM**, não do nível. Morreu no caminho a versão "+1 por estrela e +5 na
  sexta": a quebra de cadência custava um caso especial pra entregar o que a cadência única entrega.
- **Só o nv 1 da Velocidade é declarado; o topo é consequência.** Foi o que dissolveu a divergência
  em que o Suporte ficava parado em 105 — **duas pontas escritas à mão, e um comentário registrando o
  impasse em vez de alguém decidir**. Regra do Gabriel: *acha o errado e apaga, não comenta.*
- **O 🧲 é 🧲 e não 🎲** — o 🎲 já é a Taxa de Crítico neste front, e o 🎯 já é a Precisão.
- **`Combat/` fica em inglês.** A regra do `CLAUDE.md` (domínio em português) é sobre nome de TIPO e
  nunca chegou nas pastas; renomear as quatro (`Skills`, `Models`, `Enum`, `Combat`) é tema próprio.

## A RELEITURA do back-end — PAGA, e o arquivo morreu

O `docs/RELEITURA-backend-pendente.md` foi deletado neste PR. O Gabriel percorreu as §1, §2 e §3 (a
estrutura, os services e as regras do motor) e passou os olhos no resto, sem achar inconsistência
além do que já foi consertado — **o que a releitura pegou está nos PRs desta sessão**. Um arquivo que
ainda listasse a dívida como aberta seria o mesmo defeito que ele foi criado pra combater.

## Pendências

1. **Os sete números** (`GDD-progressao.md` §Os números que faltam). Eles **não bloqueiam o passo 5**
   — são de material, pedágio, forja, sub e drop, tudo no bloco DEPOIS. O maior risco segue o **#5**,
   a demanda de alma contra 36 apóstolos.
2. Menores que continuam: o **empurrão de medidor** (a `FilaDeTurnos` já tem os ganchos —
   `Vez.Esperou` e o "Velocidade ≤ 0 não cruza sozinho mas continua na fila"), a **pele da Arena**
   (FILA A #20) e a **9ª pele, Humanos** (#21, bloqueada até o fundo de facção no compêndio).

## Ideias parqueadas (não são pendência)

- **Guerra de facção** (do Raid): time mono-facção, prêmio em material de forja e sub de item — nunca
  raridade de apóstolo.
- **A Arena segue sem economia própria**, e **a 5ª dificuldade continua descartada**.
- **Teto de estabilidade da fila:** `FracaoDoCiclo < 100 ÷ (nº em campo)`, ou 12,5% num 4×4. Os 10%
  de hoje têm um quinto de folga — **um combate 5×5 derruba o teto pra 10% e encosta**.

## Gotchas que continuam valendo

- **`dotnet test` reescreve o `docs/bancada-dano.md`** e cinco linhas oscilam entre corridas
  (Tiroteio, Esgrima, Shuriken, Porradeiro, Vilania — todas na coluna de Sinergia 4). Depois de rodar
  a suíte sem ter mexido em número: `git checkout -- docs/bancada-dano.md`.
- Os harnesses `ferramentas/rodar-telas.js` e `rodar-tema.js` publicam mensagem e montam tela — **eles
  não clicam em nada**. Hover, arraste, clique e tudo que acontece DURANTE a batalha quem confere é o
  Gabriel.
- **Não há Python nesta máquina.** Splice em doc CRLF vai pelas ferramentas de edição, não por script.
