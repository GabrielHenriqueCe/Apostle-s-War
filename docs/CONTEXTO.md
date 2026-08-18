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

**Sessão inteira de design, e o modelo de progressão mudou de forma.** A branch
`docs/limpeza-nivel-raridade` carrega tudo o que foi decidido; **a fonte da raridade do apóstolo é a
única coisa que ficou de fora, de propósito** — ela continua em debate e está guardada aqui embaixo.

O que os GDDs passaram a dizer, e não diziam ontem:

- **Os dois eixos são INDEPENDENTES.** A raridade não trava mais o nível — quem trava é o **pedágio**,
  pelo material da dificuldade. **Comum nível 60 existe e é legítimo.**
- **A ESTRELA voltou, como VISOR do nível** — uma por dezena, ☆ na queda, ★★★★★★ no 60, no item e no
  apóstolo. Ela não é eixo: não tem fonte nem efeito próprio, e por isso não duplica ninguém. Os seis
  valores batem exatos com o `fatorEstrela` antigo.
- **Todo slot de campanha nasce aberto** (*"item que cai tem de poder ser usado"*). Os **2 acessórios**
  de dungeon são a exceção e destravam em **4★ e 6★**.
- **A XP é POTE dividido por quem está em campo** — solo leva tudo, quatro levam um quarto.
- **As subs iguais aparecem SEPARADAS na ficha**, uma linha por slot.
- **Nenhuma habilidade pode ser inútil numa batalha** (`GDD-combate.md`): a situacional ganha a versão
  *antes* da situação — o revive preventivo é o caso que originou a regra.
- **A alma paga nível/estrela e NÃO paga raridade.** Separar as duas fontes é o que desarma o risco dos
  36 apóstolos.

## A raridade do apóstolo — o fio que ficou aberto

**O efeito está fechado** (a raridade move a HABILIDADE) e o **teto por dificuldade** também. **A FONTE
não.** Caíram, com motivo: *alma* (foi pro nível), *missão por apóstolo* (180 instâncias), *uso de
habilidade* (o kit decide quem sobe, e dá farm de morte), *desafio-espelho 1×1* (bot esperto + cura =
empate), *matar N vezes a versão dele* (a oferta vira efeito colateral da tabela de inimigos).

**A proposta viva, e é onde a conversa parou:** a promoção vem do **capítulo**, não de atividade nova.

- **Fechar a fase 7 do capítulo da facção, numa dificuldade, pela primeira vez → N emblemas daquela
  facção.** O ícone já existe: `Faccoes.Simbolo()` (👑 Reino, 🌑 Lado Sombrio, ⚙️ Tecnológicos, 🪬
  Folclore, 🐉 Místicos, ⭐ Especial, 🔱 Decaídos, ❄️ Ascendentes, 🛠️ Humanos).
- **Custo escalonado:** 1 · 2 · 3 · 4 · 5 por degrau — 15 pra um mítico, 60 pra os quatro da facção.
  Espalhar é barato, concentrar é caro, e é aí que mora a decisão.
- **Emblema não tem tier**; quem trava é a dificuldade vencida, e o que sobra acumula.
- **Humanos:** o emblema 🛠️ vem de fechar todos os 8 capítulos de uma dificuldade — o mesmo ato que
  entrega o humano novo — e vêm junto os emblemas que faltam pros antigos alcançarem o degrau dele.
  **O gasto é na mão, um por um** (decisão do Gabriel: a tarefa é o que dá sentido ao acontecimento).
- **Onde acontece:** na ficha do apóstolo, botão *Promover*, mostrando custo, saldo e o que muda na
  habilidade. Sem tela nova.
- **Sem chance de falha, sem material extra, sem desfazer.**

**O `N` por capítulo/dificuldade é o número que decide se o elenco sobe junto ou se você tem
favoritos** — e ele não entrou na lista dos oito porque o sistema inteiro ainda pode cair.

## Pendências

1. **Voltar ao debate da raridade** (acima). É o que trava o §7.
2. **Os oito números** (`GDD-progressao.md` §Os números que faltam). O #8 é novo: a XP por fase agora
   que ela divide — ou o pote vira `4×`, ou a tabela de nível por capítulo cai por quatro.
3. **O `GDD-progressao.md` §7 volta a andar** quando a raridade fechar. A dívida do
   `docs/RELEITURA-backend-pendente.md` segue de pé.
4. Menores que continuam: o **empurrão de medidor**, a **pele da Arena** (FILA A #20) e a **9ª pele,
   Humanos** (#21, bloqueada até o fundo de facção no compêndio).

## Anotações desta sessão

- **Guerra de facção** (ideia do Gabriel, vinda do Raid): conteúdo futuro onde a facção ganha razão
  mecânica de existir. O lugar dela é **material de forja e sub de item** — nunca raridade de apóstolo.
- **Forja não FABRICA.** Materiais promovem e aceleram; criar um mítico do zero mataria o drop e a
  campanha viraria mineração.
- **As 9 facções têm exatamente um apóstolo de cada função** (9 × 4 = 36, verificado no
  `Apostolos/`). É uma grade perfeita, e vale lembrar dela antes de desenhar qualquer regra de time.
- **Regra nova no `CLAUDE.md`:** é **apóstolo**, nunca "champ"/"hero" — e quando o Gabriel escrever
  assim, corrigir.

## Gotchas que continuam valendo

- **`dotnet test` reescreve o `docs/bancada-dano.md`** e cinco linhas oscilam entre corridas. Depois de
  rodar a suíte sem ter mexido em número: `git checkout -- docs/bancada-dano.md`.
- Os harnesses `ferramentas/rodar-telas.js` e `rodar-tema.js` publicam mensagem e montam tela — **eles
  não clicam em nada**. Verde deles nunca quer dizer "o jogo funciona": hover, arraste, clique e tudo
  que acontece DURANTE a batalha quem confere é o Gabriel.
- **Splice por faixa de linha em doc CRLF:** converter o bloco novo (`sed -i 's/\r*$/\r/'`) antes de
  concatenar, e **conferir a emenda** — errar a linha final apaga meia frase sem o build reclamar.
