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

**A fila de PRs zerou** — três merges no mesmo dia: `#239` a arrumação dos docs, `#240` a faxina de
comentários, `#241` a proposta de progressão. A `main` está em dia e **não há branch pendente** além
da que traz este arquivo.

**A sessão foi inteira de DESIGN — zero linha de código.** Saiu dela o
`docs/PROPOSTA-nivel-e-raridade.md`, que é o único lugar onde o modelo novo está escrito.

## O modelo novo de progressão — e a V2 já venceu

Item e apóstolo passam a ter **dois eixos só**: **raridade = quantas · nível = quanto**. Caíram por
estarem VAZIOS a **estrela** (era o nível medido em passos de 10), o **marco de fase** (a âncora de
LUGAR que ele dava sobrevive no sacrifício) e o **aprimoramento**. Entrou **material com raridade
própria** — pó pro item, alma pro apóstolo — como pedágio a cada dezena de nível e como parte do
degrau de raridade.

**O doc ainda diz "nada decidido", e isso já é drift:** no fim da sessão o Gabriel escolheu a **V2**
(sem aprimoramento; o mítico ganha 5 subs direto). Ficou só um aviso no topo do arquivo — a reescrita
de verdade é a pendência 1.

**O que a reescrita precisa levar, e que não está em lugar nenhum ainda:**

- **o argumento que decidiu** — o endgame da V1 não é o que o GDD prometia: com a estrela morta sobra
  **uma** unidade excedente, então o reset dela vira um sorteio de 1 em 4, pago com um subsistema
  inteiro;
- **o resgate do endgame na V2, sem eixo novo** — reforjar um mítico re-sorteia as **5 subs**, com as
  mesmas 3 opções e a mesma recusa por slot da promoção;
- **que o pedágio não é imposto** — cada dezena de nível compra **+15 pontos de principal**.

## Pendências

1. **Reescrever o `PROPOSTA-nivel-e-raridade.md`:** V2 promovida a modelo, V1 rebaixada a *"considerado,
   e por que não"*, o reforge do mítico como seção de endgame.
2. **Os cinco números** que o doc deixou marcados — material por pedágio, `N` do sacrifício, XP por
   faixa de material, curva de pontos por rodada, e a demanda de alma contra **36 apóstolos** (é o
   maior risco de calibragem do desenho).
3. **Aí o `GDD-progressao.md` §7 volta a andar** — ele estava parado esperando exatamente nível e
   raridade. A dívida do `docs/RELEITURA-backend-pendente.md` segue de pé.
4. Menores que continuam: o `chance de aplicar: 75%` ao mirar, o **empurrão de medidor**, a **pele da
   Arena** (FILA A #20) e a **9ª pele, Humanos** (#21, bloqueada até o fundo de facção no compêndio).

## Armadilhas desta sessão

- **Squash-merge de branch EMPILHADA gera conflito falso.** A `faxina` saía da `arrumacao`; quando a
  de baixo entrou comprimida num commit só, a de cima seguia carregando os 7 originais. Não se resolve
  à mão — **branch nova da `main` + cherry-pick** só dos commits próprios. Feito e conferido nesta
  sessão.
- **`git diff main <branch>` NÃO prova que o merge entrou** quando há outra branch em jogo: ela aparece
  como "diferença" só por não ter o trabalho da vizinha. A prova é diffar **apenas os arquivos que a
  branch toca**.
- **Devolvi como fala dele duas invenções minhas** na mesma rodada de design (rebaixar o sacrifício a
  enfeite, tirar o "evoluir jogando"). Em conversa longa isso vira premissa e o erro compõe.
- **O `GDD-itens.md` se contradiz sozinho** sobre a 5ª unidade do mítico: *"o bônus de nascença"* numa
  linha e *"não cai no drop, só se conquista evoluindo"* quinze linhas abaixo. Mítico cai no drop — a
  segunda é a velha.

## Gotchas que continuam valendo

- **`dotnet test` reescreve o `docs/bancada-dano.md`** e cinco linhas oscilam entre corridas. Depois de
  rodar a suíte sem ter mexido em número: `git checkout -- docs/bancada-dano.md`.
- Os harnesses `ferramentas/rodar-telas.js` e `rodar-tema.js` publicam mensagem e montam tela — **eles
  não clicam em nada**. Verde deles nunca quer dizer "o jogo funciona": hover, arraste, clique e tudo
  que acontece DURANTE a batalha quem confere é o Gabriel.
