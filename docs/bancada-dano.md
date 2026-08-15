# Bancada de dano

> **Gerado por `ApostlesWar.Tests/Bancada/BancadaDeDano.cs`.** Não edite à mão —
> rode `dotnet test` e o arquivo se reescreve. É versionado de propósito: cada tweak
> de número vira um `git diff` legível.

## Condições

- **100 turnos** por medição, média de **10 repetições**.
- Stats IGUAIS pros dois lados: HP 2.000, ATK 200, DEF 0. **Crítico 100%**.
- **O apóstolo começa cada turno com 1 de vida.** Sem isso a coluna de cura seria toda zero
  (cura não cura quem está cheio), e é também a condição em que aparece quem fica mais
  FORTE ferido — a Caveira escala `2.0 − HP%`. Ele não morre: carrega a mesma
  prevenção-de-morte do boneco, que o segura quando uma habilidade de auto-dano zeraria.
- A coluna **Dano (4 alvos)** repete a medição com 4 bonecos no campo — é o que
  dá voz às habilidades de área, que contra alvo único ficam indistinguíveis de single-target.
- Na medição por habilidade, o apóstolo usa **só aquela** e **espera** durante o cooldown
  (não enche o buraco com A1 — se enchesse, o A1 dominaria e todas ficariam iguais).
- No apóstolo inteiro, quem decide é o **mesmo `ControladorBot`** da Arena e do modo Auto.
- Boneco: DEF 0 ou 1000 (o cap de 75% de redução), e **nunca age** — ele se cura.
  O HP é REALISTA nos dois lados de propósito: a Queima tira 5% do HP máximo por turno e
  cura costuma ser % do HP máximo, então inflar qualquer um dos dois estoura o número.
  Ele volta ao HP cheio entre turnos e **não morre** — usa a prevenção-de-morte do Guarda
  Real, restaurando tudo e sem cooldown, o que também o salva de habilidades que matam
  DENTRO de uma ativação (o Porradeiro do Troll dá 6 hits de 480 num alvo de 2.000).
- **CINCO LINHAS OSCILAM A CADA EXECUÇÃO, E ISSO É COMPORTAMENTO — não ruído a consertar,
  não regressão.** São 🔫 Tiroteio (Policial) · 🤺 Esgrima (Guarda) · 🌟 Shuriken (Ninja) ·
  🥊 Porradeiro (Troll) · 👿 Vilania (Vilão): as habilidades de `TipoAlvo.Aleatorio` que
  causam DANO. (As outras duas de alvo aleatório do jogo — 🍭 Doces de Abóbora e 🛸 Abduzir —
  não dão dano, então não aparecem aqui.) O desvio fica na casa de **±0,5%**. Duas causas,
  as duas de desenho:
  **(1) sortear alvo virou sortear MULTIPLICADOR.** Com 4 bonecos as casas 1 a 4 existem e
  o perfil de distância paga diferente em cada uma. Antes dele os bonecos eram
  intercambiáveis e o sorteio não mudava número nenhum — por isso isto atinge a coluna de
  **4 alvos**, e só ela, nas cinco.
  **(2) encadeamento condicional por CRÍTICO**, e este é só do 🥷 Ninja: a Shuriken carrega
  `ignorarDefesaPctSeAnteriorCritico`, o 2º hit depende do dado do 1º, e por isso ele é o
  único que também oscila na coluna de **1 alvo**.
  **A conclusão, e ela não se reabre: o relatório varia porque a BATALHA varia.** Semear o
  RNG deixaria o arquivo quieto escondendo justamente o que ele mede. **O que É sinal:**
  qualquer OUTRO apóstolo mudando, ou uma destas cinco mudando muito além de 1%.

### O que este relatório NÃO mede

O boneco **não revida**. Contra-ataque, espinhos e revide (Herói, Operário, Zumbi)
medem **zero** aqui: isto é uma bancada de dano CAUSADO, não de duelo. Um apóstolo
com número baixo pode ser reativo, não fraco — confira o kit antes de mexer.

A coluna **Usos** é diagnóstico do BOT: se uma habilidade dispara 0× no apóstolo
inteiro mas tem dano alto isolada, o problema está na fila do bot, não no balanço.

Nas linhas de apóstolo inteiro, **`Habilidades usadas` descreve a corrida de 1 alvo**. A de
4 alvos é uma simulação à parte — o bot escolhe outra fila com mais gente em campo —
e o `Esperado (4)` é cobrado pelos usos DELA, senão a sinergia sairia inventada.

**Por que a `Sinergia (4)` existe:** a de 1 alvo subestima o apóstolo de área. Quem raspa
DEF em área e martela em área colhe o malefício vezes o número de alvos — a diferença entre
as duas colunas é o tamanho real desse composto.

**⚠️ Parte da `Sinergia (4)` é GEOMETRIA, não composição.** Com 4 bonecos em campo as
casas 1 a 4 existem, e o perfil de distância multiplica o golpe (o apóstolo é Combatente, então
×1,00 na casa 1 e ×1,30 na 4 — média 1,15 quando o golpe pega todo mundo). A coluna de **1 alvo**
é imune a isso: lá os dois estão na casa 1, distância 1, ×1,00. Compare apóstolos entre si na
mesma coluna e o fator se cancela; ler a razão entre as duas colunas como composição, não.

---

## Linha 1 — por habilidade · boneco DEF 0 · imune a malefícios

Dano cru. Sem defesa no alvo, quem "fura defesa" não distorce a comparação.

| Apóstolo | Habilidade | CD | Usos | Dano | Dano/uso | Dano (4 alvos) | Cura |
|---|---|--:|--:|--:|--:|--:|--:|
| 👷 Operário | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 👷 Operário | 🧱 Parede de Tijolos | 6 | 17 | 0 | 0 | 0 | 0 |
| 👷 Operário | 🔨 Marretada | 2 | 50 | 40000 | 800 | 40000 | 0 |
| 🕵️ Detetive | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🕵️ Detetive | 🔎 Espionagem | 3 | 34 | 0 | 0 | 0 | 0 |
| 🕵️ Detetive | 🕳️ Furtividade | 3 | 34 | 32640 | 960 | 150144 | 0 |
| 👮 Policial | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 👮 Policial | 🔫 Tiroteio | 3 | 34 | 76160 | 2240 | 81860 | 0 |
| 👮 Policial | ⛓️ Prender | 3 | 34 | 0 | 0 | 0 | 0 |
| 👲 Sushiman  | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 👲 Sushiman  | 🍣 Sushi | 3 | 34 | 0 | 0 | 0 | 20400 |
| 👲 Sushiman  | 🍙 Nigiri | 3 | 34 | 0 | 0 | 0 | 0 |
| 💂 Guarda | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 💂 Guarda | 🛡️ Protetor | 3 | 34 | 0 | 0 | 0 | 0 |
| 💂 Guarda | 🤺 Esgrima | 3 | 34 | 76160 | 2240 | 81894 | 0 |
| 🥷 Ninja | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🥷 Ninja | 🌟 Shuriken | 3 | 34 | 43520 | 1280 | 46700 | 0 |
| 🥷 Ninja | 🗡️ Kunai | 3 | 34 | 38080 | 1120 | 38080 | 0 |
| 🧙 Mago | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🧙 Mago | 🔥 Bola de Fogo | 3 | 34 | 32640 | 960 | 150144 | 0 |
| 🧙 Mago | 🌋 Incêndio | 3 | 34 | 38080 | 1120 | 175168 | 0 |
| 🫅 Rei | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🫅 Rei | 🗳️ Democracia | 3 | 34 | 0 | 0 | 0 | 20400 |
| 🫅 Rei | 🎖️ Lealdade | 3 | 34 | 0 | 0 | 0 | 0 |
| 💀 Caveira | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 💀 Caveira | 🦴 Ossinho | 3 | 34 | 32572 | 958 | 149872 | 0 |
| 💀 Caveira | 🦴 Osso Duro de Roer | 3 | 34 | 0 | 0 | 0 | 0 |
| 👻 Fantasma | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 👻 Fantasma | 👻 Assombração | 3 | 34 | 38080 | 1120 | 175168 | 7616 |
| 👻 Fantasma | 💀 Vindo do Além | 6 | 17 | 16320 | 960 | 16320 | 0 |
| 🎃 Abóbora | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🎃 Abóbora | 🍬 Doces ou Travessuras | 3 | 34 | 0 | 0 | 0 | 0 |
| 🎃 Abóbora | 🍭 Doces de Abóbora | 3 | 34 | 0 | 0 | 0 | 0 |
| 🧟 Zumbi | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🧟 Zumbi | 🤢 Vômito Tóxico | 3 | 34 | 32640 | 960 | 150144 | 0 |
| 🧟 Zumbi | 💀 Putrefação | 3 | 34 | 32640 | 960 | 150144 | 6528 |
| 👾 Invasor | ⚔️ Atacar | 0 | 100 | 36850 | 368 | 36850 | 0 |
| 👾 Invasor | 📺 Glitch | 3 | 34 | 74580 | 2193 | 74580 | 0 |
| 👾 Invasor | 🪳 Barata | 3 | 34 | 49720 | 1462 | 49720 | 0 |
| 👽 Alien | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 👽 Alien | 🛸 Abduzir | 3 | 34 | 0 | 0 | 0 | 0 |
| 👽 Alien | 🌌 Galáxia | 3 | 34 | 0 | 0 | 0 | 0 |
| 🤖 Robô | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 10000 |
| 🤖 Robô | 🩻 Raio-X | 3 | 34 | 0 | 0 | 0 | 20400 |
| 🤖 Robô | 🤖 Technology | 3 | 34 | 0 | 0 | 0 | 6800 |
| 🧑‍🔬 Cientista | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🧑‍🔬 Cientista | 🧪 Química | 3 | 34 | 32640 | 960 | 150144 | 0 |
| 🧑‍🔬 Cientista | ⚛️ Física | 3 | 34 | 32640 | 960 | 150144 | 0 |
| 👹 Ogro | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 👹 Ogro | 👊 Esmagar | 3 | 34 | 0 | 0 | 0 | 17000 |
| 👹 Ogro | 💥 Quebrar | 3 | 34 | 32640 | 960 | 150144 | 0 |
| 👺 Tengu | ⚔️ Atacar | 0 | 100 | 48000 | 480 | 48000 | 0 |
| 👺 Tengu | 🌬️ Corte de Vento | 3 | 34 | 53040 | 1560 | 243916 | 0 |
| 👺 Tengu | 🌪️ Vendaval | 3 | 34 | 57120 | 1680 | 57120 | 0 |
| 🤡 Palhaço | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🤡 Palhaço | 🃏 Coringa | 3 | 34 | 0 | 0 | 0 | 0 |
| 🤡 Palhaço | 🎪 Circo | 3 | 34 | 0 | 0 | 0 | 0 |
| 🧌 Troll | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🧌 Troll | 🤜 Pancada | 3 | 34 | 38080 | 1120 | 175168 | 0 |
| 🧌 Troll | 🥊 Porradeiro | 3 | 34 | 48960 | 1440 | 55072 | 14688 |
| 🧞 Gênio | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🧞 Gênio | 🪔 Desejo | 3 | 34 | 0 | 0 | 0 | 0 |
| 🧞 Gênio | 🔮 Profecia | 3 | 34 | 32640 | 960 | 150144 | 0 |
| 🧜 Sereia | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🧜 Sereia | 🧜‍♀️ Canto de Sereia | 3 | 34 | 0 | 0 | 0 | 0 |
| 🧜 Sereia | 🌊 Atlantis | 3 | 34 | 0 | 0 | 0 | 0 |
| 🧚 Fada | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🧚 Fada | 🔔 Sininho | 3 | 34 | 43520 | 1280 | 43520 | 0 |
| 🧚 Fada | ✨ Pó Mágico | 3 | 34 | 38080 | 1120 | 175168 | 0 |
| 🐲 Dragão | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🐲 Dragão | 🔥 Sopro do Dragão | 3 | 34 | 32640 | 960 | 150144 | 0 |
| 🐲 Dragão | 🐲 Dragão Protetor | 3 | 34 | 0 | 0 | 0 | 17000 |
| 💩 Cocô | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 💩 Cocô | 🚽 Descarga | 3 | 34 | 48960 | 1440 | 48960 | 0 |
| 💩 Cocô | 🪠 Desentupidor | 3 | 34 | 38080 | 1120 | 175168 | 0 |
| 🦸 Herói | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🦸 Herói | 🦸 Salvando o Dia | 3 | 34 | 0 | 0 | 0 | 0 |
| 🦸 Herói | 💪 Super | 3 | 34 | 48960 | 1440 | 225216 | 0 |
| 🦹 Vilão | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🦹 Vilão | 🦹 Destruindo o Dia | 3 | 34 | 21760 | 640 | 100096 | 0 |
| 🦹 Vilão | 👿 Vilania | 3 | 34 | 54400 | 1600 | 58432 | 0 |
| 🦖 T-Rex | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🦖 T-Rex | 🦖 Rugido | 3 | 34 | 0 | 0 | 0 | 0 |
| 🦖 T-Rex | 🦶 Pisada | 3 | 34 | 35360 | 1040 | 162656 | 0 |
| 🦇 Morcego | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 4800 |
| 🦇 Morcego | 🦇 Mordida | 3 | 34 | 21760 | 640 | 100096 | 3264 |
| 🦇 Morcego | 🐀 Rato Voador | 4 | 25 | 0 | 0 | 0 | 0 |
| 🧛 Vampiro | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🧛 Vampiro | 🩸 Controle de Sangue | 3 | 34 | 38080 | 1120 | 38080 | 0 |
| 🧛 Vampiro | 🌙 Vampiro Primordial | 3 | 34 | 48960 | 1440 | 48960 | 0 |
| 🧝 Elfo | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🧝 Elfo | 🌳 Árvore do Mundo | 3 | 34 | 0 | 0 | 0 | 0 |
| 🧝 Elfo | 🌿 Natureza | 3 | 34 | 43520 | 1280 | 43520 | 0 |
| 😈 Diabo | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 😈 Diabo | 🔥 Inferno | 3 | 34 | 32640 | 960 | 150144 | 0 |
| 😈 Diabo | 😇 Anjo Caído | 3 | 34 | 0 | 0 | 0 | 20400 |
| ⛄ Boneco de Neve | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| ⛄ Boneco de Neve | ⛄ Bola de Neve | 3 | 34 | 46240 | 1360 | 46240 | 0 |
| ⛄ Boneco de Neve | ❄️ Gelado | 3 | 34 | 38080 | 1120 | 175168 | 0 |
| 🎭 Mímico | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🎭 Mímico | 🎭 Imitação | 3 | 34 | 43520 | 1280 | 200192 | 0 |
| 🎭 Mímico | 📋 Copiando | 4 | 25 | 0 | 0 | 0 | 0 |
| 😇 Anjo | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 10000 |
| 😇 Anjo | 🌟 Celestial | 3 | 34 | 0 | 0 | 0 | 30400 |
| 😇 Anjo | ☁️ Céu | 3 | 34 | 0 | 0 | 0 | 10000 |
| 🎅 Papai Noel | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🎅 Papai Noel | 🎅 Saco de Presente | 3 | 34 | 44166 | 1299 | 203184 | 0 |
| 🎅 Papai Noel | 🏭 Fábrica de Presente | 3 | 34 | 40800 | 1200 | 187680 | 0 |

## Linha 2 — por habilidade · boneco DEF no cap · imune a malefícios

Mesma coisa com defesa. **(2) − (1) = o que furar/ignorar defesa vale.**

| Apóstolo | Habilidade | CD | Usos | Dano | Dano/uso | Dano (4 alvos) | Cura |
|---|---|--:|--:|--:|--:|--:|--:|
| 👷 Operário | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 👷 Operário | 🧱 Parede de Tijolos | 6 | 17 | 0 | 0 | 0 | 0 |
| 👷 Operário | 🔨 Marretada | 2 | 50 | 10000 | 200 | 10000 | 0 |
| 🕵️ Detetive | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🕵️ Detetive | 🔎 Espionagem | 3 | 34 | 0 | 0 | 0 | 0 |
| 🕵️ Detetive | 🕳️ Furtividade | 3 | 34 | 8160 | 240 | 37536 | 0 |
| 👮 Policial | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 👮 Policial | 🔫 Tiroteio | 3 | 34 | 19040 | 560 | 20428 | 0 |
| 👮 Policial | ⛓️ Prender | 3 | 34 | 0 | 0 | 0 | 0 |
| 👲 Sushiman  | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 👲 Sushiman  | 🍣 Sushi | 3 | 34 | 0 | 0 | 0 | 20400 |
| 👲 Sushiman  | 🍙 Nigiri | 3 | 34 | 0 | 0 | 0 | 0 |
| 💂 Guarda | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 💂 Guarda | 🛡️ Protetor | 3 | 34 | 0 | 0 | 0 | 0 |
| 💂 Guarda | 🤺 Esgrima | 3 | 34 | 19040 | 560 | 20361 | 0 |
| 🥷 Ninja | ⚔️ Atacar | 0 | 100 | 13818 | 138 | 13818 | 0 |
| 🥷 Ninja | 🌟 Shuriken | 3 | 34 | 21719 | 638 | 22678 | 0 |
| 🥷 Ninja | 🗡️ Kunai | 3 | 34 | 32552 | 957 | 32552 | 0 |
| 🧙 Mago | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🧙 Mago | 🔥 Bola de Fogo | 3 | 34 | 8160 | 240 | 37536 | 0 |
| 🧙 Mago | 🌋 Incêndio | 3 | 34 | 9520 | 280 | 43792 | 0 |
| 🫅 Rei | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🫅 Rei | 🗳️ Democracia | 3 | 34 | 0 | 0 | 0 | 20400 |
| 🫅 Rei | 🎖️ Lealdade | 3 | 34 | 0 | 0 | 0 | 0 |
| 💀 Caveira | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 💀 Caveira | 🦴 Ossinho | 3 | 34 | 8126 | 239 | 37400 | 0 |
| 💀 Caveira | 🦴 Osso Duro de Roer | 3 | 34 | 0 | 0 | 0 | 0 |
| 👻 Fantasma | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 👻 Fantasma | 👻 Assombração | 3 | 34 | 9520 | 280 | 43792 | 1904 |
| 👻 Fantasma | 💀 Vindo do Além | 6 | 17 | 16320 | 960 | 16320 | 0 |
| 🎃 Abóbora | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🎃 Abóbora | 🍬 Doces ou Travessuras | 3 | 34 | 0 | 0 | 0 | 0 |
| 🎃 Abóbora | 🍭 Doces de Abóbora | 3 | 34 | 0 | 0 | 0 | 0 |
| 🧟 Zumbi | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🧟 Zumbi | 🤢 Vômito Tóxico | 3 | 34 | 8160 | 240 | 37536 | 0 |
| 🧟 Zumbi | 💀 Putrefação | 3 | 34 | 8160 | 240 | 37536 | 1632 |
| 👾 Invasor | ⚔️ Atacar | 0 | 100 | 9164 | 91 | 9164 | 0 |
| 👾 Invasor | 📺 Glitch | 3 | 34 | 18645 | 548 | 18645 | 0 |
| 👾 Invasor | 🪳 Barata | 3 | 34 | 12430 | 365 | 12430 | 0 |
| 👽 Alien | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 👽 Alien | 🛸 Abduzir | 3 | 34 | 0 | 0 | 0 | 0 |
| 👽 Alien | 🌌 Galáxia | 3 | 34 | 0 | 0 | 0 | 0 |
| 🤖 Robô | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 10000 |
| 🤖 Robô | 🩻 Raio-X | 3 | 34 | 0 | 0 | 0 | 20400 |
| 🤖 Robô | 🤖 Technology | 3 | 34 | 0 | 0 | 0 | 6800 |
| 🧑‍🔬 Cientista | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🧑‍🔬 Cientista | 🧪 Química | 3 | 34 | 8160 | 240 | 37536 | 0 |
| 🧑‍🔬 Cientista | ⚛️ Física | 3 | 34 | 8160 | 240 | 37536 | 0 |
| 👹 Ogro | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 👹 Ogro | 👊 Esmagar | 3 | 34 | 0 | 0 | 0 | 17000 |
| 👹 Ogro | 💥 Quebrar | 3 | 34 | 8160 | 240 | 37536 | 0 |
| 👺 Tengu | ⚔️ Atacar | 0 | 100 | 12000 | 120 | 12000 | 0 |
| 👺 Tengu | 🌬️ Corte de Vento | 3 | 34 | 13260 | 390 | 60928 | 0 |
| 👺 Tengu | 🌪️ Vendaval | 3 | 34 | 35700 | 1050 | 35700 | 0 |
| 🤡 Palhaço | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🤡 Palhaço | 🃏 Coringa | 3 | 34 | 0 | 0 | 0 | 0 |
| 🤡 Palhaço | 🎪 Circo | 3 | 34 | 0 | 0 | 0 | 0 |
| 🧌 Troll | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🧌 Troll | 🤜 Pancada | 3 | 34 | 9520 | 280 | 43792 | 0 |
| 🧌 Troll | 🥊 Porradeiro | 3 | 34 | 12240 | 360 | 13718 | 3672 |
| 🧞 Gênio | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🧞 Gênio | 🪔 Desejo | 3 | 34 | 0 | 0 | 0 | 0 |
| 🧞 Gênio | 🔮 Profecia | 3 | 34 | 8160 | 240 | 37536 | 0 |
| 🧜 Sereia | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🧜 Sereia | 🧜‍♀️ Canto de Sereia | 3 | 34 | 0 | 0 | 0 | 0 |
| 🧜 Sereia | 🌊 Atlantis | 3 | 34 | 0 | 0 | 0 | 0 |
| 🧚 Fada | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🧚 Fada | 🔔 Sininho | 3 | 34 | 10880 | 320 | 10880 | 0 |
| 🧚 Fada | ✨ Pó Mágico | 3 | 34 | 9520 | 280 | 43792 | 0 |
| 🐲 Dragão | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🐲 Dragão | 🔥 Sopro do Dragão | 3 | 34 | 8160 | 240 | 37536 | 0 |
| 🐲 Dragão | 🐲 Dragão Protetor | 3 | 34 | 0 | 0 | 0 | 17000 |
| 💩 Cocô | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 💩 Cocô | 🚽 Descarga | 3 | 34 | 12240 | 360 | 12240 | 0 |
| 💩 Cocô | 🪠 Desentupidor | 3 | 34 | 9520 | 280 | 43792 | 0 |
| 🦸 Herói | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🦸 Herói | 🦸 Salvando o Dia | 3 | 34 | 0 | 0 | 0 | 0 |
| 🦸 Herói | 💪 Super | 3 | 34 | 12240 | 360 | 56304 | 0 |
| 🦹 Vilão | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🦹 Vilão | 🦹 Destruindo o Dia | 3 | 34 | 5440 | 160 | 25024 | 0 |
| 🦹 Vilão | 👿 Vilania | 3 | 34 | 13600 | 400 | 14622 | 0 |
| 🦖 T-Rex | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🦖 T-Rex | 🦖 Rugido | 3 | 34 | 0 | 0 | 0 | 0 |
| 🦖 T-Rex | 🦶 Pisada | 3 | 34 | 8840 | 260 | 40664 | 0 |
| 🦇 Morcego | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 1200 |
| 🦇 Morcego | 🦇 Mordida | 3 | 34 | 5440 | 160 | 25024 | 816 |
| 🦇 Morcego | 🐀 Rato Voador | 4 | 25 | 0 | 0 | 0 | 0 |
| 🧛 Vampiro | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🧛 Vampiro | 🩸 Controle de Sangue | 3 | 34 | 23800 | 700 | 23800 | 0 |
| 🧛 Vampiro | 🌙 Vampiro Primordial | 3 | 34 | 12240 | 360 | 12240 | 0 |
| 🧝 Elfo | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🧝 Elfo | 🌳 Árvore do Mundo | 3 | 34 | 0 | 0 | 0 | 0 |
| 🧝 Elfo | 🌿 Natureza | 3 | 34 | 10880 | 320 | 10880 | 0 |
| 😈 Diabo | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 😈 Diabo | 🔥 Inferno | 3 | 34 | 8160 | 240 | 37536 | 0 |
| 😈 Diabo | 😇 Anjo Caído | 3 | 34 | 0 | 0 | 0 | 20400 |
| ⛄ Boneco de Neve | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| ⛄ Boneco de Neve | ⛄ Bola de Neve | 3 | 34 | 11560 | 340 | 11560 | 0 |
| ⛄ Boneco de Neve | ❄️ Gelado | 3 | 34 | 9520 | 280 | 43792 | 0 |
| 🎭 Mímico | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🎭 Mímico | 🎭 Imitação | 3 | 34 | 10880 | 320 | 50048 | 0 |
| 🎭 Mímico | 📋 Copiando | 4 | 25 | 0 | 0 | 0 | 0 |
| 😇 Anjo | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 10000 |
| 😇 Anjo | 🌟 Celestial | 3 | 34 | 0 | 0 | 0 | 30400 |
| 😇 Anjo | ☁️ Céu | 3 | 34 | 0 | 0 | 0 | 10000 |
| 🎅 Papai Noel | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🎅 Papai Noel | 🎅 Saco de Presente | 3 | 34 | 11016 | 324 | 50762 | 0 |
| 🎅 Papai Noel | 🏭 Fábrica de Presente | 3 | 34 | 10200 | 300 | 46920 | 0 |

## Linha 3 — apóstolo inteiro · boneco DEF no cap · imune a malefícios

O apóstolo jogando com o cérebro do bot. **Sinergia = real − esperado**, onde o esperado aplica o dano-por-uso da linha 2 às ativações que de fato aconteceram aqui. Positivo = as habilidades valem mais juntas do que separadas.

| Apóstolo | Dano | Esperado | Sinergia | Dano (4 alvos) | Esperado (4) | Sinergia (4) | Tick | Habilidades usadas |
|---|--:|--:|--:|--:|--:|--:|--:|---|
| 👷 Operário | 12640 | 12640 | 0 | 16432 | 12640 | +3792 | 0 | Atacar 33×, Parede de Tijolos 17×, Marretada 50× |
| 🕵️ Detetive | 10800 | 10800 | 0 | 40968 | 40176 | +792 | 0 | Atacar 33×, Espionagem 33×, Furtividade 34× |
| 👮 Policial | 24320 | 24320 | 0 | 30182 | 25680 | +4502 | 0 | Atacar 66×, Tiroteio 34×, Prender 0× |
| 👲 Sushiman  | 3960 | 2640 | +1320 | 5148 | 2640 | +2508 | 0 | Atacar 33×, Sushi 34×, Nigiri 33× |
| 💂 Guarda | 21120 | 21120 | 0 | 26131 | 22374 | +3757 | 0 | Atacar 33×, Protetor 34×, Esgrima 33× |
| 🥷 Ninja | 58524 | 58146 | +378 | 73359 | 59103 | +14256 | 0 | Atacar 33×, Shuriken 33×, Kunai 34× |
| 🧙 Mago | 20080 | 20080 | 0 | 83656 | 82864 | +792 | 0 | Atacar 33×, Bola de Fogo 33×, Incêndio 34× |
| 🫅 Rei | 2640 | 2640 | 0 | 3432 | 2640 | +792 | 0 | Atacar 33×, Democracia 34×, Lealdade 33× |
| 💀 Caveira | 10527 | 10527 | 0 | 39732 | 38940 | +792 | 0 | Atacar 33×, Ossinho 33×, Osso Duro de Roer 34× |
| 👻 Fantasma | 29760 | 29760 | 0 | 70104 | 64032 | +6072 | 0 | Atacar 49×, Assombração 34×, Vindo do Além 17× |
| 🎃 Abóbora | 5280 | 5280 | 0 | 6864 | 5280 | +1584 | 0 | Atacar 66×, Doces ou Travessuras 0×, Doces de Abóbora 34× |
| 🧟 Zumbi | 18720 | 18720 | 0 | 77400 | 76608 | +792 | 0 | Atacar 33×, Vômito Tóxico 33×, Putrefação 34× |
| 👾 Invasor | 41548 | 33680 | +7868 | 53999 | 33680 | +20319 | 0 | Atacar 33×, Glitch 34×, Barata 33× |
| 👽 Alien | 5280 | 5280 | 0 | 6864 | 5280 | +1584 | 0 | Atacar 66×, Abduzir 0×, Galáxia 34× |
| 🤖 Robô | 2640 | 2640 | 0 | 3432 | 2640 | +792 | 0 | Atacar 33×, Raio-X 34×, Technology 33× |
| 🧑‍🔬 Cientista | 18720 | 18720 | 0 | 77400 | 76608 | +792 | 0 | Atacar 33×, Química 34×, Física 33× |
| 👹 Ogro | 10560 | 10560 | 0 | 39864 | 39072 | +792 | 0 | Atacar 33×, Esmagar 34×, Quebrar 33× |
| 👺 Tengu | 51870 | 51870 | 0 | 111121 | 99538 | +11583 | 0 | Atacar 33×, Corte de Vento 34×, Vendaval 33× |
| 🤡 Palhaço | 5280 | 5280 | 0 | 6864 | 5280 | +1584 | 0 | Atacar 66×, Coringa 34×, Circo 0× |
| 🧌 Troll | 24120 | 24120 | 0 | 60333 | 58846 | +1487 | 0 | Atacar 33×, Pancada 33×, Porradeiro 34× |
| 🧞 Gênio | 10560 | 10560 | 0 | 39864 | 39072 | +792 | 0 | Atacar 33×, Desejo 34×, Profecia 33× |
| 🧜 Sereia | 6600 | 5280 | +1320 | 8580 | 5280 | +3300 | 0 | Atacar 66×, Canto de Sereia 34×, Atlantis 0× |
| 🧚 Fada | 22720 | 22720 | 0 | 60952 | 56992 | +3960 | 0 | Atacar 33×, Sininho 33×, Pó Mágico 34× |
| 🐲 Dragão | 10560 | 10560 | 0 | 39864 | 39072 | +792 | 0 | Atacar 33×, Sopro do Dragão 33×, Dragão Protetor 34× |
| 💩 Cocô | 24040 | 24040 | 0 | 62668 | 58312 | +4356 | 0 | Atacar 33×, Descarga 33×, Desentupidor 34× |
| 🦸 Herói | 16200 | 14880 | +1320 | 61452 | 58944 | +2508 | 0 | Atacar 33×, Salvando o Dia 33×, Super 34× |
| 🦹 Vilão | 21280 | 21280 | 0 | 44632 | 41854 | +2778 | 0 | Atacar 33×, Destruindo o Dia 34×, Vilania 33× |
| 🦖 T-Rex | 11480 | 11480 | 0 | 44096 | 43304 | +792 | 0 | Atacar 33×, Rugido 33×, Pisada 34× |
| 🦇 Morcego | 11000 | 8000 | +3000 | 34100 | 22400 | +11700 | 0 | Atacar 50×, Mordida 25×, Rato Voador 25× |
| 🧛 Vampiro | 37980 | 37980 | 0 | 49374 | 37980 | +11394 | 0 | Atacar 33×, Controle de Sangue 33×, Vampiro Primordial 34× |
| 🧝 Elfo | 13200 | 13200 | 0 | 17160 | 13200 | +3960 | 0 | Atacar 33×, Árvore do Mundo 34×, Natureza 33× |
| 😈 Diabo | 10560 | 10560 | 0 | 39864 | 39072 | +792 | 0 | Atacar 33×, Inferno 33×, Anjo Caído 34× |
| ⛄ Boneco de Neve | 23380 | 23380 | 0 | 61810 | 57652 | +4158 | 0 | Atacar 33×, Bola de Neve 33×, Gelado 34× |
| 🎭 Mímico | 12000 | 12000 | 0 | 42000 | 40800 | +1200 | 0 | Atacar 50×, Imitação 25×, Copiando 25× |
| 😇 Anjo | 3960 | 2640 | +1320 | 5148 | 2640 | +2508 | 0 | Atacar 33×, Celestial 34×, Céu 33× |
| 🎅 Papai Noel | 26658 | 23556 | +3102 | 111911 | 98942 | +12969 | 0 | Atacar 33×, Saco de Presente 34×, Fábrica de Presente 33× |

## Linha 4 — apóstolo inteiro · boneco DEF no cap · RECEBENDO malefícios

O apóstolo completo. **(4) − (3) = o que os malefícios dele valem.** A Sinergia aqui sai do MESMO esperado da linha 2 que a linha 3 usa — é o que mantém as duas colunas comparáveis, já que entre elas varia só a imunidade. Então **sinergia(4) − sinergia(3) = a sinergia que passa por malefício**: raspar DEF (`ReduçãoDefesa`, −30% sobre um boneco no cap) infla o golpe DIRETO e some do `Tick`, e a linha 3 não consegue enxergar isso porque o boneco dela é imune.

| Apóstolo | Dano | Esperado | Sinergia | Dano (4 alvos) | Esperado (4) | Sinergia (4) | Tick | Habilidades usadas |
|---|--:|--:|--:|--:|--:|--:|--:|---|
| 👷 Operário | 12640 | 12640 | 0 | 16432 | 12640 | +3792 | 0 | Atacar 33×, Parede de Tijolos 17×, Marretada 50× |
| 🕵️ Detetive | 20304 | 10800 | +9504 | 76773 | 40176 | +36597 | 0 | Atacar 33×, Espionagem 33×, Furtividade 34× |
| 👮 Policial | 21120 | 21120 | 0 | 26084 | 22440 | +3644 | 0 | Atacar 33×, Tiroteio 33×, Prender 34× |
| 👲 Sushiman  | 3960 | 2640 | +1320 | 5148 | 2640 | +2508 | 0 | Atacar 33×, Sushi 34×, Nigiri 33× |
| 💂 Guarda | 21120 | 21120 | 0 | 26210 | 22374 | +3836 | 0 | Atacar 33×, Protetor 34×, Esgrima 33× |
| 🥷 Ninja | 58524 | 58146 | +378 | 73349 | 59103 | +14246 | 0 | Atacar 33×, Shuriken 33×, Kunai 34× |
| 🧙 Mago | 33010 | 20040 | +12970 | 134890 | 82680 | +52210 | 10000 | Atacar 33×, Bola de Fogo 34×, Incêndio 33× |
| 🫅 Rei | 2640 | 2640 | 0 | 3432 | 2640 | +792 | 0 | Atacar 33×, Democracia 34×, Lealdade 33× |
| 💀 Caveira | 10527 | 10527 | 0 | 39732 | 38940 | +792 | 0 | Atacar 33×, Ossinho 33×, Osso Duro de Roer 34× |
| 👻 Fantasma | 29760 | 29760 | 0 | 70104 | 64032 | +6072 | 0 | Atacar 49×, Assombração 34×, Vindo do Além 17× |
| 🎃 Abóbora | 2640 | 2640 | 0 | 3432 | 2640 | +792 | 0 | Atacar 33×, Doces ou Travessuras 33×, Doces de Abóbora 34× |
| 🧟 Zumbi | 31920 | 18720 | +13200 | 130200 | 76608 | +53592 | 6600 | Atacar 33×, Vômito Tóxico 33×, Putrefação 34× |
| 👾 Invasor | 78935 | 33680 | +45255 | 102604 | 33680 | +68924 | 0 | Atacar 33×, Glitch 34×, Barata 33× |
| 👽 Alien | 2640 | 2640 | 0 | 3432 | 2640 | +792 | 0 | Atacar 33×, Abduzir 33×, Galáxia 34× |
| 🤖 Robô | 2640 | 2640 | 0 | 3432 | 2640 | +792 | 0 | Atacar 33×, Raio-X 34×, Technology 33× |
| 🧑‍🔬 Cientista | 36215 | 18720 | +17495 | 147380 | 76608 | +70772 | 17495 | Atacar 33×, Química 34×, Física 33× |
| 👹 Ogro | 10560 | 10560 | 0 | 39864 | 39072 | +792 | 0 | Atacar 33×, Esmagar 34×, Quebrar 33× |
| 👺 Tengu | 51870 | 51870 | 0 | 111121 | 99538 | +11583 | 0 | Atacar 33×, Corte de Vento 34×, Vendaval 33× |
| 🤡 Palhaço | 5280 | 5280 | 0 | 6864 | 5280 | +1584 | 0 | Atacar 66×, Coringa 34×, Circo 0× |
| 🧌 Troll | 24120 | 24120 | 0 | 60291 | 58846 | +1445 | 0 | Atacar 33×, Pancada 33×, Porradeiro 34× |
| 🧞 Gênio | 20064 | 10560 | +9504 | 75669 | 39072 | +36597 | 0 | Atacar 33×, Desejo 34×, Profecia 33× |
| 🧜 Sereia | 6600 | 5280 | +1320 | 8580 | 5280 | +3300 | 0 | Atacar 66×, Canto de Sereia 34×, Atlantis 0× |
| 🧚 Fada | 22720 | 22720 | 0 | 60952 | 56992 | +3960 | 0 | Atacar 33×, Sininho 33×, Pó Mágico 34× |
| 🐲 Dragão | 20460 | 10560 | +9900 | 79464 | 39072 | +40392 | 9900 | Atacar 33×, Sopro do Dragão 33×, Dragão Protetor 34× |
| 💩 Cocô | 34040 | 24040 | +10000 | 102668 | 58312 | +44356 | 10000 | Atacar 33×, Descarga 33×, Desentupidor 34× |
| 🦸 Herói | 16200 | 14880 | +1320 | 61452 | 58944 | +2508 | 0 | Atacar 33×, Salvando o Dia 33×, Super 34× |
| 🦹 Vilão | 21280 | 21280 | 0 | 44616 | 41854 | +2762 | 0 | Atacar 33×, Destruindo o Dia 34×, Vilania 33× |
| 🦖 T-Rex | 11480 | 11480 | 0 | 44096 | 43304 | +792 | 0 | Atacar 33×, Rugido 33×, Pisada 34× |
| 🦇 Morcego | 11000 | 8000 | +3000 | 34100 | 22400 | +11700 | 0 | Atacar 50×, Mordida 25×, Rato Voador 25× |
| 🧛 Vampiro | 37980 | 37980 | 0 | 49374 | 37980 | +11394 | 0 | Atacar 33×, Controle de Sangue 33×, Vampiro Primordial 34× |
| 🧝 Elfo | 13200 | 13200 | 0 | 17160 | 13200 | +3960 | 0 | Atacar 33×, Árvore do Mundo 34×, Natureza 33× |
| 😈 Diabo | 17160 | 10560 | +6600 | 66264 | 39072 | +27192 | 0 | Atacar 33×, Inferno 33×, Anjo Caído 34× |
| ⛄ Boneco de Neve | 23380 | 23380 | 0 | 61810 | 57652 | +4158 | 0 | Atacar 33×, Bola de Neve 33×, Gelado 34× |
| 🎭 Mímico | 12000 | 12000 | 0 | 42000 | 40800 | +1200 | 0 | Atacar 50×, Imitação 25×, Copiando 25× |
| 😇 Anjo | 3960 | 2640 | +1320 | 5148 | 2640 | +2508 | 0 | Atacar 33×, Celestial 34×, Céu 33× |
| 🎅 Papai Noel | 50451 | 23556 | +26895 | 211373 | 98942 | +112431 | 0 | Atacar 33×, Saco de Presente 34×, Fábrica de Presente 33× |

## Linha 5 — por habilidade · boneco DEF no cap · RECEBENDO malefícios

**(5) − (2) por habilidade = de quem é o mérito do malefício.** Sem esta linha, o DoT de uma habilidade (a Queima do Mago) não aparece em número nenhum por-habilidade.

| Apóstolo | Habilidade | CD | Usos | Dano | Dano/uso | Dano (4 alvos) | Cura | Tick | Δ vs linha 2 |
|---|---|--:|--:|--:|--:|--:|--:|--:|--:|
| 👷 Operário | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 👷 Operário | 🧱 Parede de Tijolos | 6 | 17 | 0 | 0 | 0 | 0 | 0 | 0 |
| 👷 Operário | 🔨 Marretada | 2 | 50 | 10000 | 200 | 10000 | 0 | 0 | 0 |
| 🕵️ Detetive | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🕵️ Detetive | 🔎 Espionagem | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🕵️ Detetive | 🕳️ Furtividade | 3 | 34 | 8160 | 240 | 37536 | 0 | 0 | 0 |
| 👮 Policial | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 👮 Policial | 🔫 Tiroteio | 3 | 34 | 19040 | 560 | 20608 | 0 | 0 | 0 |
| 👮 Policial | ⛓️ Prender | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 👲 Sushiman  | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 👲 Sushiman  | 🍣 Sushi | 3 | 34 | 0 | 0 | 0 | 20400 | 0 | 0 |
| 👲 Sushiman  | 🍙 Nigiri | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 💂 Guarda | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 💂 Guarda | 🛡️ Protetor | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 💂 Guarda | 🤺 Esgrima | 3 | 34 | 19040 | 560 | 20529 | 0 | 0 | 0 |
| 🥷 Ninja | ⚔️ Atacar | 0 | 100 | 13818 | 138 | 13818 | 0 | 0 | 0 |
| 🥷 Ninja | 🌟 Shuriken | 3 | 34 | 21719 | 638 | 22609 | 0 | 0 | 0 |
| 🥷 Ninja | 🗡️ Kunai | 3 | 34 | 32552 | 957 | 32552 | 0 | 0 | 0 |
| 🧙 Mago | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🧙 Mago | 🔥 Bola de Fogo | 3 | 34 | 18160 | 534 | 77536 | 0 | 10000 | +10000 |
| 🧙 Mago | 🌋 Incêndio | 3 | 34 | 9520 | 280 | 43792 | 0 | 0 | 0 |
| 🫅 Rei | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🫅 Rei | 🗳️ Democracia | 3 | 34 | 0 | 0 | 0 | 20400 | 0 | 0 |
| 🫅 Rei | 🎖️ Lealdade | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 💀 Caveira | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 💀 Caveira | 🦴 Ossinho | 3 | 34 | 8126 | 239 | 37400 | 0 | 0 | 0 |
| 💀 Caveira | 🦴 Osso Duro de Roer | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 👻 Fantasma | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 👻 Fantasma | 👻 Assombração | 3 | 34 | 9520 | 280 | 43792 | 1904 | 0 | 0 |
| 👻 Fantasma | 💀 Vindo do Além | 6 | 17 | 16320 | 960 | 16320 | 0 | 0 | 0 |
| 🎃 Abóbora | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🎃 Abóbora | 🍬 Doces ou Travessuras | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🎃 Abóbora | 🍭 Doces de Abóbora | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🧟 Zumbi | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🧟 Zumbi | 🤢 Vômito Tóxico | 3 | 34 | 18160 | 534 | 77536 | 0 | 10000 | +10000 |
| 🧟 Zumbi | 💀 Putrefação | 3 | 34 | 8160 | 240 | 37536 | 1632 | 0 | 0 |
| 👾 Invasor | ⚔️ Atacar | 0 | 100 | 9164 | 91 | 9164 | 0 | 0 | 0 |
| 👾 Invasor | 📺 Glitch | 3 | 34 | 35410 | 1041 | 35410 | 0 | 0 | +16765 |
| 👾 Invasor | 🪳 Barata | 3 | 34 | 12430 | 365 | 12430 | 0 | 0 | 0 |
| 👽 Alien | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 👽 Alien | 🛸 Abduzir | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 👽 Alien | 🌌 Galáxia | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🤖 Robô | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 10000 | 0 | 0 |
| 🤖 Robô | 🩻 Raio-X | 3 | 34 | 0 | 0 | 0 | 20400 | 0 | 0 |
| 🤖 Robô | 🤖 Technology | 3 | 34 | 0 | 0 | 0 | 6800 | 0 | 0 |
| 🧑‍🔬 Cientista | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🧑‍🔬 Cientista | 🧪 Química | 3 | 34 | 18160 | 534 | 77536 | 0 | 10000 | +10000 |
| 🧑‍🔬 Cientista | ⚛️ Física | 3 | 34 | 18160 | 534 | 77536 | 0 | 10000 | +10000 |
| 👹 Ogro | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 👹 Ogro | 👊 Esmagar | 3 | 34 | 0 | 0 | 0 | 17000 | 0 | 0 |
| 👹 Ogro | 💥 Quebrar | 3 | 34 | 8160 | 240 | 37536 | 0 | 0 | 0 |
| 👺 Tengu | ⚔️ Atacar | 0 | 100 | 12000 | 120 | 12000 | 0 | 0 | 0 |
| 👺 Tengu | 🌬️ Corte de Vento | 3 | 34 | 13260 | 390 | 60928 | 0 | 0 | 0 |
| 👺 Tengu | 🌪️ Vendaval | 3 | 34 | 35700 | 1050 | 35700 | 0 | 0 | 0 |
| 🤡 Palhaço | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🤡 Palhaço | 🃏 Coringa | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🤡 Palhaço | 🎪 Circo | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🧌 Troll | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🧌 Troll | 🤜 Pancada | 3 | 34 | 9520 | 280 | 43792 | 0 | 0 | 0 |
| 🧌 Troll | 🥊 Porradeiro | 3 | 34 | 12240 | 360 | 13728 | 3672 | 0 | 0 |
| 🧞 Gênio | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🧞 Gênio | 🪔 Desejo | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🧞 Gênio | 🔮 Profecia | 3 | 34 | 15504 | 456 | 71264 | 0 | 0 | +7344 |
| 🧜 Sereia | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🧜 Sereia | 🧜‍♀️ Canto de Sereia | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🧜 Sereia | 🌊 Atlantis | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🧚 Fada | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🧚 Fada | 🔔 Sininho | 3 | 34 | 10880 | 320 | 10880 | 0 | 0 | 0 |
| 🧚 Fada | ✨ Pó Mágico | 3 | 34 | 9520 | 280 | 43792 | 0 | 0 | 0 |
| 🐲 Dragão | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🐲 Dragão | 🔥 Sopro do Dragão | 3 | 34 | 18160 | 534 | 77536 | 0 | 10000 | +10000 |
| 🐲 Dragão | 🐲 Dragão Protetor | 3 | 34 | 0 | 0 | 0 | 17000 | 0 | 0 |
| 💩 Cocô | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 💩 Cocô | 🚽 Descarga | 3 | 34 | 22240 | 654 | 22240 | 0 | 10000 | +10000 |
| 💩 Cocô | 🪠 Desentupidor | 3 | 34 | 19520 | 574 | 83792 | 0 | 10000 | +10000 |
| 🦸 Herói | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🦸 Herói | 🦸 Salvando o Dia | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🦸 Herói | 💪 Super | 3 | 34 | 12240 | 360 | 56304 | 0 | 0 | 0 |
| 🦹 Vilão | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🦹 Vilão | 🦹 Destruindo o Dia | 3 | 34 | 5440 | 160 | 25024 | 0 | 0 | 0 |
| 🦹 Vilão | 👿 Vilania | 3 | 34 | 13600 | 400 | 14564 | 0 | 0 | 0 |
| 🦖 T-Rex | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🦖 T-Rex | 🦖 Rugido | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🦖 T-Rex | 🦶 Pisada | 3 | 34 | 8840 | 260 | 40664 | 0 | 0 | 0 |
| 🦇 Morcego | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 1200 | 0 | 0 |
| 🦇 Morcego | 🦇 Mordida | 3 | 34 | 5440 | 160 | 25024 | 1632 | 0 | 0 |
| 🦇 Morcego | 🐀 Rato Voador | 4 | 25 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🧛 Vampiro | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🧛 Vampiro | 🩸 Controle de Sangue | 3 | 34 | 23800 | 700 | 23800 | 0 | 0 | 0 |
| 🧛 Vampiro | 🌙 Vampiro Primordial | 3 | 34 | 12240 | 360 | 12240 | 0 | 0 | 0 |
| 🧝 Elfo | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🧝 Elfo | 🌳 Árvore do Mundo | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🧝 Elfo | 🌿 Natureza | 3 | 34 | 10880 | 320 | 10880 | 0 | 0 | 0 |
| 😈 Diabo | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 😈 Diabo | 🔥 Inferno | 3 | 34 | 14960 | 440 | 64736 | 0 | 0 | +6800 |
| 😈 Diabo | 😇 Anjo Caído | 3 | 34 | 0 | 0 | 0 | 20400 | 0 | 0 |
| ⛄ Boneco de Neve | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| ⛄ Boneco de Neve | ⛄ Bola de Neve | 3 | 34 | 11560 | 340 | 11560 | 0 | 0 | 0 |
| ⛄ Boneco de Neve | ❄️ Gelado | 3 | 34 | 9520 | 280 | 43792 | 0 | 0 | 0 |
| 🎭 Mímico | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🎭 Mímico | 🎭 Imitação | 3 | 34 | 10880 | 320 | 50048 | 0 | 0 | 0 |
| 🎭 Mímico | 📋 Copiando | 4 | 25 | 0 | 0 | 0 | 0 | 0 | 0 |
| 😇 Anjo | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 10000 | 0 | 0 |
| 😇 Anjo | 🌟 Celestial | 3 | 34 | 0 | 0 | 0 | 30400 | 0 | 0 |
| 😇 Anjo | ☁️ Céu | 3 | 34 | 0 | 0 | 0 | 10000 | 0 | 0 |
| 🎅 Papai Noel | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🎅 Papai Noel | 🎅 Saco de Presente | 3 | 34 | 11016 | 324 | 50762 | 0 | 0 | 0 |
| 🎅 Papai Noel | 🏭 Fábrica de Presente | 3 | 34 | 19380 | 570 | 89148 | 0 | 0 | +9180 |

## Rankings (condições da linha 1: DEF 0, alvo imune)

A tabela por apóstolo acima responde "como é o kit deste personagem?".
Estas respondem "quem está fora da curva?".

### Dano por uso — o BURST

| # | Apóstolo | Habilidade | CD | Valor |
|--:|---|---|--:|--:|
| 1 | 👮 Policial | 🔫 Tiroteio | 3 | 2240 |
| 2 | 💂 Guarda | 🤺 Esgrima | 3 | 2240 |
| 3 | 👾 Invasor | 📺 Glitch | 3 | 2193 |
| 4 | 👺 Tengu | 🌪️ Vendaval | 3 | 1680 |
| 5 | 🦹 Vilão | 👿 Vilania | 3 | 1600 |
| 6 | 👺 Tengu | 🌬️ Corte de Vento | 3 | 1560 |
| 7 | 👾 Invasor | 🪳 Barata | 3 | 1462 |
| 8 | 🧌 Troll | 🥊 Porradeiro | 3 | 1440 |
| 9 | 💩 Cocô | 🚽 Descarga | 3 | 1440 |
| 10 | 🦸 Herói | 💪 Super | 3 | 1440 |
| 11 | 🧛 Vampiro | 🌙 Vampiro Primordial | 3 | 1440 |
| 12 | ⛄ Boneco de Neve | ⛄ Bola de Neve | 3 | 1360 |
| 13 | 🎅 Papai Noel | 🎅 Saco de Presente | 3 | 1299 |
| 14 | 🥷 Ninja | 🌟 Shuriken | 3 | 1280 |
| 15 | 🧚 Fada | 🔔 Sininho | 3 | 1280 |
| 16 | 🧝 Elfo | 🌿 Natureza | 3 | 1280 |
| 17 | 🎭 Mímico | 🎭 Imitação | 3 | 1280 |
| 18 | 🎅 Papai Noel | 🏭 Fábrica de Presente | 3 | 1200 |
| 19 | 🥷 Ninja | 🗡️ Kunai | 3 | 1120 |
| 20 | 🧙 Mago | 🌋 Incêndio | 3 | 1120 |
| 21 | 👻 Fantasma | 👻 Assombração | 3 | 1120 |
| 22 | 🧌 Troll | 🤜 Pancada | 3 | 1120 |
| 23 | 🧚 Fada | ✨ Pó Mágico | 3 | 1120 |
| 24 | 💩 Cocô | 🪠 Desentupidor | 3 | 1120 |
| 25 | 🧛 Vampiro | 🩸 Controle de Sangue | 3 | 1120 |
| 26 | ⛄ Boneco de Neve | ❄️ Gelado | 3 | 1120 |
| 27 | 🦖 T-Rex | 🦶 Pisada | 3 | 1040 |
| 28 | 🕵️ Detetive | 🕳️ Furtividade | 3 | 960 |
| 29 | 🧙 Mago | 🔥 Bola de Fogo | 3 | 960 |
| 30 | 👻 Fantasma | 💀 Vindo do Além | 6 | 960 |
| 31 | 🧟 Zumbi | 🤢 Vômito Tóxico | 3 | 960 |
| 32 | 🧟 Zumbi | 💀 Putrefação | 3 | 960 |
| 33 | 🧑‍🔬 Cientista | 🧪 Química | 3 | 960 |
| 34 | 🧑‍🔬 Cientista | ⚛️ Física | 3 | 960 |
| 35 | 👹 Ogro | 💥 Quebrar | 3 | 960 |
| 36 | 🧞 Gênio | 🔮 Profecia | 3 | 960 |
| 37 | 🐲 Dragão | 🔥 Sopro do Dragão | 3 | 960 |
| 38 | 😈 Diabo | 🔥 Inferno | 3 | 960 |
| 39 | 💀 Caveira | 🦴 Ossinho | 3 | 958 |
| 40 | 👷 Operário | 🔨 Marretada | 2 | 800 |
| 41 | 🦹 Vilão | 🦹 Destruindo o Dia | 3 | 640 |
| 42 | 🦇 Morcego | 🦇 Mordida | 3 | 640 |
| 43 | 👺 Tengu | ⚔️ Atacar | 0 | 480 |
| 44 | 👾 Invasor | ⚔️ Atacar | 0 | 368 |
| 45 | 👷 Operário | ⚔️ Atacar | 0 | 320 |
| 46 | 🕵️ Detetive | ⚔️ Atacar | 0 | 320 |
| 47 | 👮 Policial | ⚔️ Atacar | 0 | 320 |
| 48 | 👲 Sushiman  | ⚔️ Atacar | 0 | 320 |
| 49 | 💂 Guarda | ⚔️ Atacar | 0 | 320 |
| 50 | 🥷 Ninja | ⚔️ Atacar | 0 | 320 |
| 51 | 🧙 Mago | ⚔️ Atacar | 0 | 320 |
| 52 | 🫅 Rei | ⚔️ Atacar | 0 | 320 |
| 53 | 💀 Caveira | ⚔️ Atacar | 0 | 320 |
| 54 | 👻 Fantasma | ⚔️ Atacar | 0 | 320 |
| 55 | 🎃 Abóbora | ⚔️ Atacar | 0 | 320 |
| 56 | 🧟 Zumbi | ⚔️ Atacar | 0 | 320 |
| 57 | 👽 Alien | ⚔️ Atacar | 0 | 320 |
| 58 | 🤖 Robô | ⚔️ Atacar | 0 | 320 |
| 59 | 🧑‍🔬 Cientista | ⚔️ Atacar | 0 | 320 |
| 60 | 👹 Ogro | ⚔️ Atacar | 0 | 320 |
| 61 | 🤡 Palhaço | ⚔️ Atacar | 0 | 320 |
| 62 | 🧌 Troll | ⚔️ Atacar | 0 | 320 |
| 63 | 🧞 Gênio | ⚔️ Atacar | 0 | 320 |
| 64 | 🧜 Sereia | ⚔️ Atacar | 0 | 320 |
| 65 | 🧚 Fada | ⚔️ Atacar | 0 | 320 |
| 66 | 🐲 Dragão | ⚔️ Atacar | 0 | 320 |
| 67 | 💩 Cocô | ⚔️ Atacar | 0 | 320 |
| 68 | 🦸 Herói | ⚔️ Atacar | 0 | 320 |
| 69 | 🦹 Vilão | ⚔️ Atacar | 0 | 320 |
| 70 | 🦖 T-Rex | ⚔️ Atacar | 0 | 320 |
| 71 | 🦇 Morcego | ⚔️ Atacar | 0 | 320 |
| 72 | 🧛 Vampiro | ⚔️ Atacar | 0 | 320 |
| 73 | 🧝 Elfo | ⚔️ Atacar | 0 | 320 |
| 74 | 😈 Diabo | ⚔️ Atacar | 0 | 320 |
| 75 | ⛄ Boneco de Neve | ⚔️ Atacar | 0 | 320 |
| 76 | 🎭 Mímico | ⚔️ Atacar | 0 | 320 |
| 77 | 😇 Anjo | ⚔️ Atacar | 0 | 320 |
| 78 | 🎅 Papai Noel | ⚔️ Atacar | 0 | 320 |

### Dano em 100 turnos, 4 alvos — o SUSTENTADO com área

| # | Apóstolo | Habilidade | CD | Valor |
|--:|---|---|--:|--:|
| 1 | 👺 Tengu | 🌬️ Corte de Vento | 3 | 243916 |
| 2 | 🦸 Herói | 💪 Super | 3 | 225216 |
| 3 | 🎅 Papai Noel | 🎅 Saco de Presente | 3 | 203184 |
| 4 | 🎭 Mímico | 🎭 Imitação | 3 | 200192 |
| 5 | 🎅 Papai Noel | 🏭 Fábrica de Presente | 3 | 187680 |
| 6 | 🧙 Mago | 🌋 Incêndio | 3 | 175168 |
| 7 | 👻 Fantasma | 👻 Assombração | 3 | 175168 |
| 8 | 🧌 Troll | 🤜 Pancada | 3 | 175168 |
| 9 | 🧚 Fada | ✨ Pó Mágico | 3 | 175168 |
| 10 | 💩 Cocô | 🪠 Desentupidor | 3 | 175168 |
| 11 | ⛄ Boneco de Neve | ❄️ Gelado | 3 | 175168 |
| 12 | 🦖 T-Rex | 🦶 Pisada | 3 | 162656 |
| 13 | 🕵️ Detetive | 🕳️ Furtividade | 3 | 150144 |
| 14 | 🧙 Mago | 🔥 Bola de Fogo | 3 | 150144 |
| 15 | 🧟 Zumbi | 🤢 Vômito Tóxico | 3 | 150144 |
| 16 | 🧟 Zumbi | 💀 Putrefação | 3 | 150144 |
| 17 | 🧑‍🔬 Cientista | 🧪 Química | 3 | 150144 |
| 18 | 🧑‍🔬 Cientista | ⚛️ Física | 3 | 150144 |
| 19 | 👹 Ogro | 💥 Quebrar | 3 | 150144 |
| 20 | 🧞 Gênio | 🔮 Profecia | 3 | 150144 |
| 21 | 🐲 Dragão | 🔥 Sopro do Dragão | 3 | 150144 |
| 22 | 😈 Diabo | 🔥 Inferno | 3 | 150144 |
| 23 | 💀 Caveira | 🦴 Ossinho | 3 | 149872 |
| 24 | 🦹 Vilão | 🦹 Destruindo o Dia | 3 | 100096 |
| 25 | 🦇 Morcego | 🦇 Mordida | 3 | 100096 |
| 26 | 💂 Guarda | 🤺 Esgrima | 3 | 81894 |
| 27 | 👮 Policial | 🔫 Tiroteio | 3 | 81860 |
| 28 | 👾 Invasor | 📺 Glitch | 3 | 74580 |
| 29 | 🦹 Vilão | 👿 Vilania | 3 | 58432 |
| 30 | 👺 Tengu | 🌪️ Vendaval | 3 | 57120 |
| 31 | 🧌 Troll | 🥊 Porradeiro | 3 | 55072 |
| 32 | 👾 Invasor | 🪳 Barata | 3 | 49720 |
| 33 | 💩 Cocô | 🚽 Descarga | 3 | 48960 |
| 34 | 🧛 Vampiro | 🌙 Vampiro Primordial | 3 | 48960 |
| 35 | 👺 Tengu | ⚔️ Atacar | 0 | 48000 |
| 36 | 🥷 Ninja | 🌟 Shuriken | 3 | 46700 |
| 37 | ⛄ Boneco de Neve | ⛄ Bola de Neve | 3 | 46240 |
| 38 | 🧚 Fada | 🔔 Sininho | 3 | 43520 |
| 39 | 🧝 Elfo | 🌿 Natureza | 3 | 43520 |
| 40 | 👷 Operário | 🔨 Marretada | 2 | 40000 |
| 41 | 🥷 Ninja | 🗡️ Kunai | 3 | 38080 |
| 42 | 🧛 Vampiro | 🩸 Controle de Sangue | 3 | 38080 |
| 43 | 👾 Invasor | ⚔️ Atacar | 0 | 36850 |
| 44 | 👷 Operário | ⚔️ Atacar | 0 | 32000 |
| 45 | 🕵️ Detetive | ⚔️ Atacar | 0 | 32000 |
| 46 | 👮 Policial | ⚔️ Atacar | 0 | 32000 |
| 47 | 👲 Sushiman  | ⚔️ Atacar | 0 | 32000 |
| 48 | 💂 Guarda | ⚔️ Atacar | 0 | 32000 |
| 49 | 🥷 Ninja | ⚔️ Atacar | 0 | 32000 |
| 50 | 🧙 Mago | ⚔️ Atacar | 0 | 32000 |
| 51 | 🫅 Rei | ⚔️ Atacar | 0 | 32000 |
| 52 | 💀 Caveira | ⚔️ Atacar | 0 | 32000 |
| 53 | 👻 Fantasma | ⚔️ Atacar | 0 | 32000 |
| 54 | 🎃 Abóbora | ⚔️ Atacar | 0 | 32000 |
| 55 | 🧟 Zumbi | ⚔️ Atacar | 0 | 32000 |
| 56 | 👽 Alien | ⚔️ Atacar | 0 | 32000 |
| 57 | 🤖 Robô | ⚔️ Atacar | 0 | 32000 |
| 58 | 🧑‍🔬 Cientista | ⚔️ Atacar | 0 | 32000 |
| 59 | 👹 Ogro | ⚔️ Atacar | 0 | 32000 |
| 60 | 🤡 Palhaço | ⚔️ Atacar | 0 | 32000 |
| 61 | 🧌 Troll | ⚔️ Atacar | 0 | 32000 |
| 62 | 🧞 Gênio | ⚔️ Atacar | 0 | 32000 |
| 63 | 🧜 Sereia | ⚔️ Atacar | 0 | 32000 |
| 64 | 🧚 Fada | ⚔️ Atacar | 0 | 32000 |
| 65 | 🐲 Dragão | ⚔️ Atacar | 0 | 32000 |
| 66 | 💩 Cocô | ⚔️ Atacar | 0 | 32000 |
| 67 | 🦸 Herói | ⚔️ Atacar | 0 | 32000 |
| 68 | 🦹 Vilão | ⚔️ Atacar | 0 | 32000 |
| 69 | 🦖 T-Rex | ⚔️ Atacar | 0 | 32000 |
| 70 | 🦇 Morcego | ⚔️ Atacar | 0 | 32000 |
| 71 | 🧛 Vampiro | ⚔️ Atacar | 0 | 32000 |
| 72 | 🧝 Elfo | ⚔️ Atacar | 0 | 32000 |
| 73 | 😈 Diabo | ⚔️ Atacar | 0 | 32000 |
| 74 | ⛄ Boneco de Neve | ⚔️ Atacar | 0 | 32000 |
| 75 | 🎭 Mímico | ⚔️ Atacar | 0 | 32000 |
| 76 | 😇 Anjo | ⚔️ Atacar | 0 | 32000 |
| 77 | 🎅 Papai Noel | ⚔️ Atacar | 0 | 32000 |
| 78 | 👻 Fantasma | 💀 Vindo do Além | 6 | 16320 |

### Cura em 100 turnos

| # | Apóstolo | Habilidade | CD | Valor |
|--:|---|---|--:|--:|
| 1 | 😇 Anjo | 🌟 Celestial | 3 | 30400 |
| 2 | 👲 Sushiman  | 🍣 Sushi | 3 | 20400 |
| 3 | 🫅 Rei | 🗳️ Democracia | 3 | 20400 |
| 4 | 🤖 Robô | 🩻 Raio-X | 3 | 20400 |
| 5 | 😈 Diabo | 😇 Anjo Caído | 3 | 20400 |
| 6 | 👹 Ogro | 👊 Esmagar | 3 | 17000 |
| 7 | 🐲 Dragão | 🐲 Dragão Protetor | 3 | 17000 |
| 8 | 🧌 Troll | 🥊 Porradeiro | 3 | 14688 |
| 9 | 🤖 Robô | ⚔️ Atacar | 0 | 10000 |
| 10 | 😇 Anjo | ⚔️ Atacar | 0 | 10000 |
| 11 | 😇 Anjo | ☁️ Céu | 3 | 10000 |
| 12 | 👻 Fantasma | 👻 Assombração | 3 | 7616 |
| 13 | 🤖 Robô | 🤖 Technology | 3 | 6800 |
| 14 | 🧟 Zumbi | 💀 Putrefação | 3 | 6528 |
| 15 | 🦇 Morcego | ⚔️ Atacar | 0 | 4800 |
| 16 | 🦇 Morcego | 🦇 Mordida | 3 | 3264 |

