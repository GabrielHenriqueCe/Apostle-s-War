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

**O modelo de progressão fechou inteiro** — nível, raridade e as duas economias — e está escrito nos
GDDs. Foi uma sessão só de design, zero linha de código. A branch `docs/limpeza-nivel-raridade` tem
tudo, em três commits.

**Não há decisão pendente no modelo.** O que sobrou são os sete números de calibragem
(`GDD-progressao.md` §Os números que faltam) e a execução, que é o §7.

## O modelo, em cinco linhas

- **Dois eixos INDEPENDENTES nos dois objetos:** **raridade = quantas · nível = quanto.** Um não trava
  o outro — **comum nível 60 existe**. Quem trava o nível é o **pedágio** (material da dificuldade).
- **A estrela é o VISOR do nível** — uma por dezena, ☆ na queda, ★★★★★★ no 60. Não é eixo. Ela destrava
  os 2 slots de acessório, em 4★ e 6★; os 7 slots de campanha nascem abertos.
- **Item:** raridade = quantas subs (forja: sacrifício + material) · nível = magnitude
  (`fatorNível = 10 + 1,5 × nível`).
- **Apóstolo:** raridade = a HABILIDADE, e **não mexe em stat nenhum** — comum e mítico do mesmo nível
  têm os mesmos números. Nível = os números, por XP.
- **Duas moedas que não se trocam:** **alma** paga nível/estrela (cai de inimigo, sempre) · **emblema**
  paga raridade (cai de fechar capítulo, uma vez). É isso que acaba com o *"não sei em qual investir"*.

## O EMBLEMA — a peça que fechou a sessão

Fechar a **fase 7 do capítulo da facção**, em cada dificuldade, **pela primeira vez**. Ícone = o símbolo
da própria facção, que já existe em `Faccoes.cs`.

```
colheita   Fácil 4 · Normal 8 · Difícil 16 · Pesadelo 32   =  60
custo      1 · 2 · 3 · 4 · 5  =  15 por apóstolo  =  60 pelos 4 da facção
```

**Oferta = demanda.** O destino é sempre os quatro míticos; o que se escolhe é a **ORDEM**. Não é
farmável, não tem teto de raridade por dificuldade, não falha e não se desfaz.

## Pendências

1. **Os sete números** (`GDD-progressao.md` §Os números que faltam). O maior risco continua sendo o
   **#5**, a demanda de alma contra 36 apóstolos.
2. **O `GDD-progressao.md` §7 volta a andar** — ele estava parado esperando nível e raridade, e agora
   não está mais. A dívida do `docs/RELEITURA-backend-pendente.md` segue de pé.
3. Menores que continuam: o **empurrão de medidor**, a **pele da Arena** (FILA A #20) e a **9ª pele,
   Humanos** (#21, bloqueada até o fundo de facção no compêndio).

## Ideias parqueadas (não são pendência)

- **Guerra de facção** (do Raid): time mono-facção, e o prêmio é **material de forja e sub de item** —
  nunca raridade de apóstolo. É onde a facção ganharia razão mecânica de existir.
- **A Arena segue sem economia própria.** A cauda de emblemas que chegou a ser proposta foi descartada:
  a oferta já fecha em 60 e não há déficit pra ela cobrir.
- **5ª dificuldade: não.** O número 4 está costurado no jogo (ponto por rodada, XP, tetos de nível,
  `4·8·16·32`), e a identidade que fecha a economia é de quatro parcelas.
- **Se o 1º degrau parecer barato demais**, a alavanca é redistribuir dentro dos mesmos 15
  (`2·3·3·3·4`) — um número, nada mais.

## Anotações desta sessão

- **As 9 facções têm exatamente um apóstolo de cada função** (9 × 4 = 36, verificado em `Apostolos/`).
  Grade perfeita — vale lembrar antes de desenhar qualquer regra de time.
- **Regra nova no `CLAUDE.md`:** é **apóstolo**, nunca "champ"/"hero" — e quando o Gabriel escrever
  assim, corrigir.
- **Duas invenções minhas que ele teve de derrubar:** o "comum veste 3, mítico veste 9" (nunca foi
  decisão dele) e o `(5)` do aprimoramento voltando na ficha de subs. O que ele não citou, se preserva.

## Gotchas que continuam valendo

- **`dotnet test` reescreve o `docs/bancada-dano.md`** e cinco linhas oscilam entre corridas. Depois de
  rodar a suíte sem ter mexido em número: `git checkout -- docs/bancada-dano.md`.
- Os harnesses `ferramentas/rodar-telas.js` e `rodar-tema.js` publicam mensagem e montam tela — **eles
  não clicam em nada**. Verde deles nunca quer dizer "o jogo funciona": hover, arraste, clique e tudo
  que acontece DURANTE a batalha quem confere é o Gabriel.
- **Splice por faixa de linha em doc CRLF:** converter o bloco novo (`sed -i 's/\r*$/\r/'`) antes de
  concatenar e **conferir a emenda**. Errei a linha final quatro vezes nesta sessão — cada erro apaga
  meia frase em silêncio, e nenhum build reclama. Conferir com `sed -n` DEPOIS de cada splice.
