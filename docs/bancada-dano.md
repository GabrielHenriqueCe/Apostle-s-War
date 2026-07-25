# Bancada de dano

> **Gerado por `ApostlesWar.Tests/Bancada/BancadaDeDano.cs`.** Não edite à mão —
> rode `dotnet test` e o arquivo se reescreve. É versionado de propósito: cada tweak
> de número vira um `git diff` legível.

## Condições

- **100 turnos** por medição, média de **10 repetições**.
- Stats IGUAIS pros dois lados: HP 2.000, ATK 200, DEF 0. **Crítico 100%**.
- **O champ começa cada turno com 1 de vida.** Sem isso a coluna de cura seria toda zero
  (cura não cura quem está cheio), e é também a condição em que aparece quem fica mais
  FORTE ferido — a Caveira escala `2.0 − HP%`. Ele não morre: carrega a mesma
  prevenção-de-morte do boneco, que o segura quando uma habilidade de auto-dano zeraria.
- A coluna **Dano (4 alvos)** repete a medição com 4 bonecos no campo — é o que
  dá voz às habilidades de área, que contra alvo único ficam indistinguíveis de single-target.
- Na medição por habilidade, o champ usa **só aquela** e **espera** durante o cooldown
  (não enche o buraco com A1 — se enchesse, o A1 dominaria e todas ficariam iguais).
- No champ inteiro, quem decide é o **mesmo `ControladorBot`** da Arena e do modo Auto.
- Boneco: DEF 0 ou 1000 (o cap de 75% de redução), e **nunca age** — ele se cura.
  O HP é REALISTA nos dois lados de propósito: a Queima tira 5% do HP máximo por turno e
  cura costuma ser % do HP máximo, então inflar qualquer um dos dois estoura o número.
  Ele volta ao HP cheio entre turnos e **não morre** — usa a prevenção-de-morte do Guarda
  Real, restaurando tudo e sem cooldown, o que também o salva de habilidades que matam
  DENTRO de uma ativação (o Porradeiro do Troll dá 6 hits de 480 num alvo de 2.000).

### O que este relatório NÃO mede

O boneco **não revida**. Contra-ataque, espinhos e revide (Herói, Operário, Zumbi)
medem **zero** aqui: isto é uma bancada de dano CAUSADO, não de duelo. Um champ
com número baixo pode ser reativo, não fraco — confira o kit antes de mexer.

A coluna **Usos** é diagnóstico do BOT: se uma habilidade dispara 0× no champ
inteiro mas tem dano alto isolada, o problema está na fila do bot, não no balanço.

---

## Linha 1 — por habilidade · boneco DEF 0 · imune a malefícios

Dano cru. Sem defesa no alvo, quem "fura defesa" não distorce a comparação.

| Champ | Habilidade | CD | Usos | Dano | Dano/uso | Dano (4 alvos) | Cura |
|---|---|--:|--:|--:|--:|--:|--:|
| 👷 Operário | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 👷 Operário | 🧱 Parede de Tijolos | 6 | 17 | 0 | 0 | 0 | 0 |
| 👷 Operário | 🔨 Marretada | 3 | 34 | 13600 | 400 | 13600 | 0 |
| 🕵️ Detetive | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🕵️ Detetive | 🔎 Espionagem | 4 | 25 | 0 | 0 | 0 | 0 |
| 🕵️ Detetive | 🕳️ Furtividade | 4 | 25 | 8000 | 320 | 32000 | 0 |
| 👮 Policial | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 👮 Policial | 🔫 Tiroteio | 4 | 25 | 12000 | 480 | 12000 | 0 |
| 👮 Policial | ⛓️ Prender | 4 | 25 | 0 | 0 | 0 | 0 |
| 👲 Sushiman  | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 👲 Sushiman  | 🍣 Sushi | 4 | 25 | 0 | 0 | 0 | 15000 |
| 👲 Sushiman  | 🍙 Nigiri | 4 | 25 | 0 | 0 | 0 | 0 |
| 💂 Guarda | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 💂 Guarda | 🛡️ Protetor | 4 | 25 | 0 | 0 | 0 | 0 |
| 💂 Guarda | 🤺 Esgrima | 3 | 34 | 32640 | 960 | 32640 | 0 |
| 🥷 Ninja | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🥷 Ninja | 🌟 Shuriken | 3 | 34 | 32640 | 960 | 32640 | 0 |
| 🥷 Ninja | 🗡️ Kunai | 4 | 25 | 12000 | 480 | 12000 | 0 |
| 🧙 Mago | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🧙 Mago | 🔥 Bola de Fogo | 4 | 25 | 16000 | 640 | 16000 | 0 |
| 🧙 Mago | 🌋 Incêndio | 4 | 25 | 12000 | 480 | 48000 | 0 |
| 🫅 Rei | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🫅 Rei | 🗳️ Democracia | 3 | 34 | 0 | 0 | 0 | 20400 |
| 🫅 Rei | 🎖️ Lealdade | 3 | 34 | 0 | 0 | 0 | 0 |
| 💀 Caveira | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 💀 Caveira | 🦴 Ossinho | 3 | 34 | 21692 | 638 | 86768 | 0 |
| 💀 Caveira | 🦴 Osso Duro de Roer | 3 | 34 | 0 | 0 | 0 | 0 |
| 👻 Fantasma | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 👻 Fantasma | 👻 Assombração | 3 | 34 | 10880 | 320 | 43520 | 2176 |
| 👻 Fantasma | 💀 Vindo do Além | 3 | 34 | 16320 | 480 | 16320 | 0 |
| 🎃 Abóbora | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🎃 Abóbora | 🍬 Doces ou Travessuras | 4 | 25 | 0 | 0 | 0 | 0 |
| 🎃 Abóbora | 🍭 Doces de Abóbora | 4 | 25 | 0 | 0 | 0 | 0 |
| 🧟 Zumbi | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🧟 Zumbi | 🤢 Vômito Tóxico | 4 | 25 | 8000 | 320 | 32000 | 0 |
| 🧟 Zumbi | 💀 Putrefação | 4 | 25 | 8000 | 320 | 32000 | 1600 |
| 👾 Invasor | ⚔️ Atacar | 0 | 100 | 36850 | 368 | 36850 | 0 |
| 👾 Invasor | 📺 Glitch | 3 | 34 | 23283 | 684 | 23283 | 0 |
| 👾 Invasor | 🪳 Barata | 3 | 34 | 24860 | 731 | 24860 | 0 |
| 👽 Alien | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 👽 Alien | 🛸 Abduzir | 4 | 25 | 0 | 0 | 0 | 0 |
| 👽 Alien | 🌌 Galáxia | 4 | 25 | 0 | 0 | 0 | 0 |
| 🤖 Robô | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 10000 |
| 🤖 Robô | 🩻 Raio-X | 4 | 25 | 0 | 0 | 0 | 7500 |
| 🤖 Robô | 🤖 Technology | 4 | 25 | 0 | 0 | 0 | 5000 |
| 🧑‍🔬 Cientista | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🧑‍🔬 Cientista | 🧪 Química | 3 | 34 | 10880 | 320 | 43520 | 0 |
| 🧑‍🔬 Cientista | ⚛️ Física | 3 | 34 | 10880 | 320 | 43520 | 0 |
| 👹 Ogro | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 👹 Ogro | 👊 Esmagar | 3 | 34 | 0 | 0 | 0 | 34000 |
| 👹 Ogro | 💥 Quebrar | 3 | 34 | 21760 | 640 | 87040 | 0 |
| 👺 Tengu | ⚔️ Atacar | 0 | 100 | 40000 | 400 | 40000 | 0 |
| 👺 Tengu | 🌬️ Corte de Vento | 3 | 34 | 13600 | 400 | 54400 | 0 |
| 👺 Tengu | 🌪️ Vendaval | 4 | 25 | 25000 | 1000 | 25000 | 0 |
| 🤡 Palhaço | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🤡 Palhaço | 🃏 Coringa | 3 | 34 | 0 | 0 | 0 | 0 |
| 🤡 Palhaço | 🎪 Circo | 4 | 25 | 0 | 0 | 0 | 0 |
| 🧌 Troll | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🧌 Troll | 🤜 Pancada | 3 | 34 | 19040 | 560 | 76160 | 0 |
| 🧌 Troll | 🥊 Porradeiro | 4 | 25 | 72000 | 2880 | 72000 | 21600 |
| 🧞 Gênio | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🧞 Gênio | 🪔 Desejo | 3 | 34 | 0 | 0 | 0 | 0 |
| 🧞 Gênio | 🔮 Profecia | 3 | 34 | 16320 | 480 | 65280 | 0 |
| 🧜 Sereia | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🧜 Sereia | 🧜‍♀️ Canto de Sereia | 4 | 25 | 0 | 0 | 0 | 0 |
| 🧜 Sereia | 🌊 Atlantis | 4 | 25 | 0 | 0 | 0 | 0 |
| 🧚 Fada | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🧚 Fada | 🔔 Sininho | 3 | 34 | 32640 | 960 | 32640 | 0 |
| 🧚 Fada | ✨ Pó Mágico | 4 | 25 | 12000 | 480 | 48000 | 0 |
| 🐲 Dragão | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🐲 Dragão | 🔥 Sopro do Dragão | 3 | 34 | 21760 | 640 | 87040 | 0 |
| 🐲 Dragão | 🐲 Dragão Protetor | 3 | 34 | 0 | 0 | 0 | 17000 |
| 💩 Cocô | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 💩 Cocô | 🚽 Descarga | 3 | 34 | 27200 | 800 | 27200 | 0 |
| 💩 Cocô | 🪠 Desentupidor | 3 | 34 | 16320 | 480 | 65280 | 0 |
| 🦸 Herói | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🦸 Herói | 🦸 Salvando o Dia | 3 | 34 | 0 | 0 | 0 | 0 |
| 🦸 Herói | 💪 Super | 3 | 34 | 27200 | 800 | 108800 | 0 |
| 🦹 Vilão | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🦹 Vilão | 🦹 Destruindo o Dia | 3 | 34 | 21760 | 640 | 87040 | 0 |
| 🦹 Vilão | 👿 Vilania | 4 | 25 | 48000 | 1920 | 48000 | 0 |
| 🦖 T-Rex | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🦖 T-Rex | 🦖 Rugido | 3 | 34 | 0 | 0 | 0 | 0 |
| 🦖 T-Rex | 🦶 Pisada | 3 | 34 | 24480 | 720 | 97920 | 0 |
| 🦇 Morcego | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 4800 |
| 🦇 Morcego | 🦇 Mordida | 3 | 34 | 21760 | 640 | 87040 | 3264 |
| 🦇 Morcego | 🐀 Rato Voador | 4 | 25 | 0 | 0 | 0 | 0 |
| 🧛 Vampiro | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🧛 Vampiro | 🩸 Controle de Sangue | 3 | 34 | 32640 | 960 | 32640 | 0 |
| 🧛 Vampiro | 🌙 Vampiro Primordial | 4 | 25 | 8000 | 320 | 8000 | 0 |
| 🧝 Elfo | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🧝 Elfo | 🌳 Árvore do Mundo | 3 | 34 | 0 | 0 | 0 | 0 |
| 🧝 Elfo | 🌿 Natureza | 3 | 34 | 16320 | 480 | 16320 | 0 |
| 😈 Diabo | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 😈 Diabo | 🔥 Inferno | 3 | 34 | 0 | 0 | 0 | 0 |
| 😈 Diabo | 😇 Anjo Caído | 3 | 34 | 0 | 0 | 0 | 20400 |
| ⛄ Boneco de Neve | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| ⛄ Boneco de Neve | ⛄ Bola de Neve | 3 | 34 | 29920 | 880 | 29920 | 0 |
| ⛄ Boneco de Neve | ❄️ Gelado | 4 | 25 | 14000 | 560 | 56000 | 0 |
| 🎭 Mímico | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🎭 Mímico | 🎭 Imitação | 3 | 34 | 13600 | 400 | 54400 | 0 |
| 🎭 Mímico | 📋 Copiando | 4 | 25 | 0 | 0 | 0 | 0 |
| 😇 Anjo | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 10000 |
| 😇 Anjo | 🌟 Celestial | 3 | 34 | 0 | 0 | 0 | 30400 |
| 😇 Anjo | ☁️ Céu | 4 | 25 | 0 | 0 | 0 | 10000 |
| 🎅 Papai Noel | ⚔️ Atacar | 0 | 100 | 32000 | 320 | 32000 | 0 |
| 🎅 Papai Noel | 🎅 Saco de Presente | 3 | 34 | 23766 | 699 | 95064 | 0 |
| 🎅 Papai Noel | 🏭 Fábrica de Presente | 3 | 34 | 19040 | 560 | 76160 | 0 |

## Linha 2 — por habilidade · boneco DEF no cap · imune a malefícios

Mesma coisa com defesa. **(2) − (1) = o que furar/ignorar defesa vale.**

| Champ | Habilidade | CD | Usos | Dano | Dano/uso | Dano (4 alvos) | Cura |
|---|---|--:|--:|--:|--:|--:|--:|
| 👷 Operário | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 👷 Operário | 🧱 Parede de Tijolos | 6 | 17 | 0 | 0 | 0 | 0 |
| 👷 Operário | 🔨 Marretada | 3 | 34 | 3400 | 100 | 3400 | 0 |
| 🕵️ Detetive | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🕵️ Detetive | 🔎 Espionagem | 4 | 25 | 0 | 0 | 0 | 0 |
| 🕵️ Detetive | 🕳️ Furtividade | 4 | 25 | 2000 | 80 | 8000 | 0 |
| 👮 Policial | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 👮 Policial | 🔫 Tiroteio | 4 | 25 | 3000 | 120 | 3000 | 0 |
| 👮 Policial | ⛓️ Prender | 4 | 25 | 0 | 0 | 0 | 0 |
| 👲 Sushiman  | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 👲 Sushiman  | 🍣 Sushi | 4 | 25 | 0 | 0 | 0 | 15000 |
| 👲 Sushiman  | 🍙 Nigiri | 4 | 25 | 0 | 0 | 0 | 0 |
| 💂 Guarda | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 💂 Guarda | 🛡️ Protetor | 4 | 25 | 0 | 0 | 0 | 0 |
| 💂 Guarda | 🤺 Esgrima | 3 | 34 | 8160 | 240 | 8160 | 0 |
| 🥷 Ninja | ⚔️ Atacar | 0 | 100 | 13818 | 138 | 13818 | 0 |
| 🥷 Ninja | 🌟 Shuriken | 3 | 34 | 16273 | 478 | 15674 | 0 |
| 🥷 Ninja | 🗡️ Kunai | 4 | 25 | 10234 | 409 | 10234 | 0 |
| 🧙 Mago | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🧙 Mago | 🔥 Bola de Fogo | 4 | 25 | 4000 | 160 | 4000 | 0 |
| 🧙 Mago | 🌋 Incêndio | 4 | 25 | 3000 | 120 | 12000 | 0 |
| 🫅 Rei | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🫅 Rei | 🗳️ Democracia | 3 | 34 | 0 | 0 | 0 | 20400 |
| 🫅 Rei | 🎖️ Lealdade | 3 | 34 | 0 | 0 | 0 | 0 |
| 💀 Caveira | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 💀 Caveira | 🦴 Ossinho | 3 | 34 | 5406 | 159 | 21624 | 0 |
| 💀 Caveira | 🦴 Osso Duro de Roer | 3 | 34 | 0 | 0 | 0 | 0 |
| 👻 Fantasma | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 👻 Fantasma | 👻 Assombração | 3 | 34 | 2720 | 80 | 10880 | 544 |
| 👻 Fantasma | 💀 Vindo do Além | 3 | 34 | 16320 | 480 | 16320 | 0 |
| 🎃 Abóbora | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🎃 Abóbora | 🍬 Doces ou Travessuras | 4 | 25 | 0 | 0 | 0 | 0 |
| 🎃 Abóbora | 🍭 Doces de Abóbora | 4 | 25 | 0 | 0 | 0 | 0 |
| 🧟 Zumbi | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🧟 Zumbi | 🤢 Vômito Tóxico | 4 | 25 | 2000 | 80 | 8000 | 0 |
| 🧟 Zumbi | 💀 Putrefação | 4 | 25 | 2000 | 80 | 8000 | 400 |
| 👾 Invasor | ⚔️ Atacar | 0 | 100 | 9164 | 91 | 9164 | 0 |
| 👾 Invasor | 📺 Glitch | 3 | 34 | 5812 | 170 | 5812 | 0 |
| 👾 Invasor | 🪳 Barata | 3 | 34 | 6215 | 182 | 6215 | 0 |
| 👽 Alien | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 👽 Alien | 🛸 Abduzir | 4 | 25 | 0 | 0 | 0 | 0 |
| 👽 Alien | 🌌 Galáxia | 4 | 25 | 0 | 0 | 0 | 0 |
| 🤖 Robô | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 10000 |
| 🤖 Robô | 🩻 Raio-X | 4 | 25 | 0 | 0 | 0 | 7500 |
| 🤖 Robô | 🤖 Technology | 4 | 25 | 0 | 0 | 0 | 5000 |
| 🧑‍🔬 Cientista | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🧑‍🔬 Cientista | 🧪 Química | 3 | 34 | 2720 | 80 | 10880 | 0 |
| 🧑‍🔬 Cientista | ⚛️ Física | 3 | 34 | 2720 | 80 | 10880 | 0 |
| 👹 Ogro | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 👹 Ogro | 👊 Esmagar | 3 | 34 | 0 | 0 | 0 | 34000 |
| 👹 Ogro | 💥 Quebrar | 3 | 34 | 5440 | 160 | 21760 | 0 |
| 👺 Tengu | ⚔️ Atacar | 0 | 100 | 10000 | 100 | 10000 | 0 |
| 👺 Tengu | 🌬️ Corte de Vento | 3 | 34 | 3400 | 100 | 13600 | 0 |
| 👺 Tengu | 🌪️ Vendaval | 4 | 25 | 15625 | 625 | 15625 | 0 |
| 🤡 Palhaço | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🤡 Palhaço | 🃏 Coringa | 3 | 34 | 0 | 0 | 0 | 0 |
| 🤡 Palhaço | 🎪 Circo | 4 | 25 | 0 | 0 | 0 | 0 |
| 🧌 Troll | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🧌 Troll | 🤜 Pancada | 3 | 34 | 4760 | 140 | 19040 | 0 |
| 🧌 Troll | 🥊 Porradeiro | 4 | 25 | 18000 | 720 | 18000 | 5400 |
| 🧞 Gênio | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🧞 Gênio | 🪔 Desejo | 3 | 34 | 0 | 0 | 0 | 0 |
| 🧞 Gênio | 🔮 Profecia | 3 | 34 | 4080 | 120 | 16320 | 0 |
| 🧜 Sereia | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🧜 Sereia | 🧜‍♀️ Canto de Sereia | 4 | 25 | 0 | 0 | 0 | 0 |
| 🧜 Sereia | 🌊 Atlantis | 4 | 25 | 0 | 0 | 0 | 0 |
| 🧚 Fada | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🧚 Fada | 🔔 Sininho | 3 | 34 | 8160 | 240 | 8160 | 0 |
| 🧚 Fada | ✨ Pó Mágico | 4 | 25 | 3000 | 120 | 12000 | 0 |
| 🐲 Dragão | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🐲 Dragão | 🔥 Sopro do Dragão | 3 | 34 | 5440 | 160 | 21760 | 0 |
| 🐲 Dragão | 🐲 Dragão Protetor | 3 | 34 | 0 | 0 | 0 | 17000 |
| 💩 Cocô | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 💩 Cocô | 🚽 Descarga | 3 | 34 | 6800 | 200 | 6800 | 0 |
| 💩 Cocô | 🪠 Desentupidor | 3 | 34 | 4080 | 120 | 16320 | 0 |
| 🦸 Herói | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🦸 Herói | 🦸 Salvando o Dia | 3 | 34 | 0 | 0 | 0 | 0 |
| 🦸 Herói | 💪 Super | 3 | 34 | 6800 | 200 | 27200 | 0 |
| 🦹 Vilão | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🦹 Vilão | 🦹 Destruindo o Dia | 3 | 34 | 5440 | 160 | 21760 | 0 |
| 🦹 Vilão | 👿 Vilania | 4 | 25 | 12000 | 480 | 12000 | 0 |
| 🦖 T-Rex | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🦖 T-Rex | 🦖 Rugido | 3 | 34 | 0 | 0 | 0 | 0 |
| 🦖 T-Rex | 🦶 Pisada | 3 | 34 | 6120 | 180 | 24480 | 0 |
| 🦇 Morcego | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 1200 |
| 🦇 Morcego | 🦇 Mordida | 3 | 34 | 5440 | 160 | 21760 | 816 |
| 🦇 Morcego | 🐀 Rato Voador | 4 | 25 | 0 | 0 | 0 | 0 |
| 🧛 Vampiro | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🧛 Vampiro | 🩸 Controle de Sangue | 3 | 34 | 20400 | 600 | 20400 | 0 |
| 🧛 Vampiro | 🌙 Vampiro Primordial | 4 | 25 | 2000 | 80 | 2000 | 0 |
| 🧝 Elfo | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🧝 Elfo | 🌳 Árvore do Mundo | 3 | 34 | 0 | 0 | 0 | 0 |
| 🧝 Elfo | 🌿 Natureza | 3 | 34 | 4080 | 120 | 4080 | 0 |
| 😈 Diabo | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 😈 Diabo | 🔥 Inferno | 3 | 34 | 0 | 0 | 0 | 0 |
| 😈 Diabo | 😇 Anjo Caído | 3 | 34 | 0 | 0 | 0 | 20400 |
| ⛄ Boneco de Neve | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| ⛄ Boneco de Neve | ⛄ Bola de Neve | 3 | 34 | 7480 | 220 | 7480 | 0 |
| ⛄ Boneco de Neve | ❄️ Gelado | 4 | 25 | 3500 | 140 | 14000 | 0 |
| 🎭 Mímico | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🎭 Mímico | 🎭 Imitação | 3 | 34 | 3400 | 100 | 13600 | 0 |
| 🎭 Mímico | 📋 Copiando | 4 | 25 | 0 | 0 | 0 | 0 |
| 😇 Anjo | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 10000 |
| 😇 Anjo | 🌟 Celestial | 3 | 34 | 0 | 0 | 0 | 30400 |
| 😇 Anjo | ☁️ Céu | 4 | 25 | 0 | 0 | 0 | 10000 |
| 🎅 Papai Noel | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 |
| 🎅 Papai Noel | 🎅 Saco de Presente | 3 | 34 | 5916 | 174 | 23664 | 0 |
| 🎅 Papai Noel | 🏭 Fábrica de Presente | 3 | 34 | 4760 | 140 | 19040 | 0 |

## Linha 3 — champ inteiro · boneco DEF no cap · imune a malefícios

O champ jogando com o cérebro do bot. **Sinergia = real − esperado**, onde o esperado aplica o dano-por-uso da linha 2 às ativações que de fato aconteceram aqui. Positivo = as habilidades valem mais juntas do que separadas.

| Champ | Dano total | Esperado (isolado × usos) | Sinergia | Habilidades usadas |
|---|--:|--:|--:|---|
| 👷 Operário | 7300 | 7300 | 0 | Atacar 50×, Parede de Tijolos 17×, Marretada 33× |
| 🕵️ Detetive | 8000 | 8000 | 0 | Atacar 75×, Espionagem 0×, Furtividade 25× |
| 👮 Policial | 9000 | 9000 | 0 | Atacar 75×, Tiroteio 25×, Prender 0× |
| 👲 Sushiman  | 5000 | 4000 | +1000 | Atacar 50×, Sushi 25×, Nigiri 25× |
| 💂 Guarda | 10000 | 10000 | 0 | Atacar 50×, Protetor 25×, Esgrima 25× |
| 🥷 Ninja | 29290 | 29075 | +215 | Atacar 50×, Shuriken 25×, Kunai 25× |
| 🧙 Mago | 11000 | 11000 | 0 | Atacar 50×, Bola de Fogo 25×, Incêndio 25× |
| 🫅 Rei | 2640 | 2640 | 0 | Atacar 33×, Democracia 34×, Lealdade 33× |
| 💀 Caveira | 7887 | 7887 | 0 | Atacar 33×, Ossinho 33×, Osso Duro de Roer 34× |
| 👻 Fantasma | 21200 | 21200 | 0 | Atacar 33×, Assombração 34×, Vindo do Além 33× |
| 🎃 Abóbora | 6000 | 6000 | 0 | Atacar 75×, Doces ou Travessuras 0×, Doces de Abóbora 25× |
| 🧟 Zumbi | 8000 | 8000 | 0 | Atacar 50×, Vômito Tóxico 25×, Putrefação 25× |
| 👾 Invasor | 17241 | 14801 | +2440 | Atacar 33×, Glitch 33×, Barata 34× |
| 👽 Alien | 6000 | 6000 | 0 | Atacar 75×, Abduzir 0×, Galáxia 25× |
| 🤖 Robô | 2719 | 4000 | -1281 | Atacar 50×, Raio-X 25×, Technology 25× |
| 🧑‍🔬 Cientista | 8000 | 8000 | 0 | Atacar 33×, Química 34×, Física 33× |
| 👹 Ogro | 7920 | 7920 | 0 | Atacar 33×, Esmagar 34×, Quebrar 33× |
| 👺 Tengu | 21550 | 21550 | 0 | Atacar 44×, Corte de Vento 34×, Vendaval 22× |
| 🤡 Palhaço | 5280 | 5280 | 0 | Atacar 66×, Coringa 34×, Circo 0× |
| 🧌 Troll | 25500 | 25500 | 0 | Atacar 50×, Pancada 25×, Porradeiro 25× |
| 🧞 Gênio | 6600 | 6600 | 0 | Atacar 33×, Desejo 34×, Profecia 33× |
| 🧜 Sereia | 7000 | 6000 | +1000 | Atacar 75×, Canto de Sereia 25×, Atlantis 0× |
| 🧚 Fada | 13000 | 13000 | 0 | Atacar 50×, Sininho 25×, Pó Mágico 25× |
| 🐲 Dragão | 7920 | 7920 | 0 | Atacar 33×, Sopro do Dragão 33×, Dragão Protetor 34× |
| 💩 Cocô | 13320 | 13320 | 0 | Atacar 33×, Descarga 33×, Desentupidor 34× |
| 🦸 Herói | 10100 | 9440 | +660 | Atacar 33×, Salvando o Dia 33×, Super 34× |
| 🦹 Vilão | 19520 | 19520 | 0 | Atacar 44×, Destruindo o Dia 34×, Vilania 22× |
| 🦖 T-Rex | 8760 | 8760 | 0 | Atacar 33×, Rugido 33×, Pisada 34× |
| 🦇 Morcego | 9500 | 8000 | +1500 | Atacar 50×, Mordida 25×, Rato Voador 25× |
| 🧛 Vampiro | 21000 | 21000 | 0 | Atacar 50×, Controle de Sangue 25×, Vampiro Primordial 25× |
| 🧝 Elfo | 6600 | 6600 | 0 | Atacar 33×, Árvore do Mundo 34×, Natureza 33× |
| 😈 Diabo | 5280 | 5280 | 0 | Atacar 66×, Inferno 0×, Anjo Caído 34× |
| ⛄ Boneco de Neve | 13000 | 13000 | 0 | Atacar 50×, Bola de Neve 25×, Gelado 25× |
| 🎭 Mímico | 6500 | 6500 | 0 | Atacar 50×, Imitação 25×, Copiando 25× |
| 😇 Anjo | 3960 | 3520 | +440 | Atacar 44×, Celestial 34×, Céu 22× |
| 🎅 Papai Noel | 14958 | 13176 | +1782 | Atacar 33×, Saco de Presente 34×, Fábrica de Presente 33× |

## Linha 4 — champ inteiro · boneco DEF no cap · RECEBENDO malefícios

O champ completo. **(4) − (3) = o que os malefícios dele valem.**

| Champ | Dano total | Tick | Habilidades usadas |
|---|--:|--:|---|
| 👷 Operário | 7300 | 0 | Atacar 50×, Parede de Tijolos 17×, Marretada 33× |
| 🕵️ Detetive | 9600 | 0 | Atacar 50×, Espionagem 25×, Furtividade 25× |
| 👮 Policial | 7000 | 0 | Atacar 50×, Tiroteio 25×, Prender 25× |
| 👲 Sushiman  | 5000 | 0 | Atacar 50×, Sushi 25×, Nigiri 25× |
| 💂 Guarda | 10000 | 0 | Atacar 50×, Protetor 25×, Esgrima 25× |
| 🥷 Ninja | 29290 | 0 | Atacar 50×, Shuriken 25×, Kunai 25× |
| 🧙 Mago | 19750 | 7500 | Atacar 50×, Bola de Fogo 25×, Incêndio 25× |
| 🫅 Rei | 2640 | 0 | Atacar 33×, Democracia 34×, Lealdade 33× |
| 💀 Caveira | 7887 | 0 | Atacar 33×, Ossinho 33×, Osso Duro de Roer 34× |
| 👻 Fantasma | 21200 | 0 | Atacar 33×, Assombração 34×, Vindo do Além 33× |
| 🎃 Abóbora | 4000 | 0 | Atacar 50×, Doces ou Travessuras 25×, Doces de Abóbora 25× |
| 🧟 Zumbi | 13000 | 5000 | Atacar 50×, Vômito Tóxico 25×, Putrefação 25× |
| 👾 Invasor | 32760 | 0 | Atacar 33×, Glitch 34×, Barata 33× |
| 👽 Alien | 4000 | 0 | Atacar 50×, Abduzir 25×, Galáxia 25× |
| 🤖 Robô | 2683 | 0 | Atacar 50×, Raio-X 25×, Technology 25× |
| 🧑‍🔬 Cientista | 25495 | 17495 | Atacar 33×, Química 34×, Física 33× |
| 👹 Ogro | 7920 | 0 | Atacar 33×, Esmagar 34×, Quebrar 33× |
| 👺 Tengu | 21550 | 0 | Atacar 44×, Corte de Vento 34×, Vendaval 22× |
| 🤡 Palhaço | 5280 | 0 | Atacar 66×, Coringa 34×, Circo 0× |
| 🧌 Troll | 25500 | 0 | Atacar 50×, Pancada 25×, Porradeiro 25× |
| 🧞 Gênio | 12540 | 0 | Atacar 33×, Desejo 34×, Profecia 33× |
| 🧜 Sereia | 7000 | 0 | Atacar 75×, Canto de Sereia 25×, Atlantis 0× |
| 🧚 Fada | 13000 | 0 | Atacar 50×, Sininho 25×, Pó Mágico 25× |
| 🐲 Dragão | 17820 | 9900 | Atacar 33×, Sopro do Dragão 33×, Dragão Protetor 34× |
| 💩 Cocô | 23320 | 10000 | Atacar 33×, Descarga 33×, Desentupidor 34× |
| 🦸 Herói | 10100 | 0 | Atacar 33×, Salvando o Dia 33×, Super 34× |
| 🦹 Vilão | 19520 | 0 | Atacar 44×, Destruindo o Dia 34×, Vilania 22× |
| 🦖 T-Rex | 8760 | 0 | Atacar 33×, Rugido 33×, Pisada 34× |
| 🦇 Morcego | 9500 | 0 | Atacar 50×, Mordida 25×, Rato Voador 25× |
| 🧛 Vampiro | 21000 | 0 | Atacar 50×, Controle de Sangue 25×, Vampiro Primordial 25× |
| 🧝 Elfo | 6600 | 0 | Atacar 33×, Árvore do Mundo 34×, Natureza 33× |
| 😈 Diabo | 9240 | 0 | Atacar 33×, Inferno 33×, Anjo Caído 34× |
| ⛄ Boneco de Neve | 13000 | 0 | Atacar 50×, Bola de Neve 25×, Gelado 25× |
| 🎭 Mímico | 6500 | 0 | Atacar 50×, Imitação 25×, Copiando 25× |
| 😇 Anjo | 3960 | 0 | Atacar 44×, Celestial 34×, Céu 22× |
| 🎅 Papai Noel | 28356 | 0 | Atacar 33×, Saco de Presente 34×, Fábrica de Presente 33× |

## Linha 5 — por habilidade · boneco DEF no cap · RECEBENDO malefícios

**(5) − (2) por habilidade = de quem é o mérito do malefício.** Sem esta linha, o DoT de uma habilidade (a Queima do Mago) não aparece em número nenhum por-habilidade.

| Champ | Habilidade | CD | Usos | Dano | Dano/uso | Dano (4 alvos) | Cura | Tick | Δ vs linha 2 |
|---|---|--:|--:|--:|--:|--:|--:|--:|--:|
| 👷 Operário | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 👷 Operário | 🧱 Parede de Tijolos | 6 | 17 | 0 | 0 | 0 | 0 | 0 | 0 |
| 👷 Operário | 🔨 Marretada | 3 | 34 | 3400 | 100 | 3400 | 0 | 0 | 0 |
| 🕵️ Detetive | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🕵️ Detetive | 🔎 Espionagem | 4 | 25 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🕵️ Detetive | 🕳️ Furtividade | 4 | 25 | 2000 | 80 | 8000 | 0 | 0 | 0 |
| 👮 Policial | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 👮 Policial | 🔫 Tiroteio | 4 | 25 | 3000 | 120 | 3000 | 0 | 0 | 0 |
| 👮 Policial | ⛓️ Prender | 4 | 25 | 0 | 0 | 0 | 0 | 0 | 0 |
| 👲 Sushiman  | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 👲 Sushiman  | 🍣 Sushi | 4 | 25 | 0 | 0 | 0 | 15000 | 0 | 0 |
| 👲 Sushiman  | 🍙 Nigiri | 4 | 25 | 0 | 0 | 0 | 0 | 0 | 0 |
| 💂 Guarda | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 💂 Guarda | 🛡️ Protetor | 4 | 25 | 0 | 0 | 0 | 0 | 0 | 0 |
| 💂 Guarda | 🤺 Esgrima | 3 | 34 | 8160 | 240 | 8160 | 0 | 0 | 0 |
| 🥷 Ninja | ⚔️ Atacar | 0 | 100 | 13818 | 138 | 13818 | 0 | 0 | 0 |
| 🥷 Ninja | 🌟 Shuriken | 3 | 34 | 16273 | 478 | 15675 | 0 | 0 | 0 |
| 🥷 Ninja | 🗡️ Kunai | 4 | 25 | 10234 | 409 | 10234 | 0 | 0 | 0 |
| 🧙 Mago | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🧙 Mago | 🔥 Bola de Fogo | 4 | 25 | 11500 | 460 | 11500 | 0 | 7500 | +7500 |
| 🧙 Mago | 🌋 Incêndio | 4 | 25 | 3000 | 120 | 12000 | 0 | 0 | 0 |
| 🫅 Rei | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🫅 Rei | 🗳️ Democracia | 3 | 34 | 0 | 0 | 0 | 20400 | 0 | 0 |
| 🫅 Rei | 🎖️ Lealdade | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 💀 Caveira | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 💀 Caveira | 🦴 Ossinho | 3 | 34 | 5406 | 159 | 21624 | 0 | 0 | 0 |
| 💀 Caveira | 🦴 Osso Duro de Roer | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 👻 Fantasma | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 👻 Fantasma | 👻 Assombração | 3 | 34 | 2720 | 80 | 10880 | 544 | 0 | 0 |
| 👻 Fantasma | 💀 Vindo do Além | 3 | 34 | 16320 | 480 | 16320 | 0 | 0 | 0 |
| 🎃 Abóbora | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🎃 Abóbora | 🍬 Doces ou Travessuras | 4 | 25 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🎃 Abóbora | 🍭 Doces de Abóbora | 4 | 25 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🧟 Zumbi | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🧟 Zumbi | 🤢 Vômito Tóxico | 4 | 25 | 7000 | 280 | 28000 | 0 | 5000 | +5000 |
| 🧟 Zumbi | 💀 Putrefação | 4 | 25 | 2000 | 80 | 8000 | 400 | 0 | 0 |
| 👾 Invasor | ⚔️ Atacar | 0 | 100 | 9164 | 91 | 9164 | 0 | 0 | 0 |
| 👾 Invasor | 📺 Glitch | 3 | 34 | 11052 | 325 | 11052 | 0 | 0 | +5240 |
| 👾 Invasor | 🪳 Barata | 3 | 34 | 6215 | 182 | 6215 | 0 | 0 | 0 |
| 👽 Alien | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 👽 Alien | 🛸 Abduzir | 4 | 25 | 0 | 0 | 0 | 0 | 0 | 0 |
| 👽 Alien | 🌌 Galáxia | 4 | 25 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🤖 Robô | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 10000 | 0 | 0 |
| 🤖 Robô | 🩻 Raio-X | 4 | 25 | 0 | 0 | 0 | 7500 | 0 | 0 |
| 🤖 Robô | 🤖 Technology | 4 | 25 | 0 | 0 | 0 | 5000 | 0 | 0 |
| 🧑‍🔬 Cientista | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🧑‍🔬 Cientista | 🧪 Química | 3 | 34 | 12720 | 374 | 50880 | 0 | 10000 | +10000 |
| 🧑‍🔬 Cientista | ⚛️ Física | 3 | 34 | 12720 | 374 | 50880 | 0 | 10000 | +10000 |
| 👹 Ogro | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 👹 Ogro | 👊 Esmagar | 3 | 34 | 0 | 0 | 0 | 34000 | 0 | 0 |
| 👹 Ogro | 💥 Quebrar | 3 | 34 | 5440 | 160 | 21760 | 0 | 0 | 0 |
| 👺 Tengu | ⚔️ Atacar | 0 | 100 | 10000 | 100 | 10000 | 0 | 0 | 0 |
| 👺 Tengu | 🌬️ Corte de Vento | 3 | 34 | 3400 | 100 | 13600 | 0 | 0 | 0 |
| 👺 Tengu | 🌪️ Vendaval | 4 | 25 | 15625 | 625 | 15625 | 0 | 0 | 0 |
| 🤡 Palhaço | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🤡 Palhaço | 🃏 Coringa | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🤡 Palhaço | 🎪 Circo | 4 | 25 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🧌 Troll | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🧌 Troll | 🤜 Pancada | 3 | 34 | 4760 | 140 | 19040 | 0 | 0 | 0 |
| 🧌 Troll | 🥊 Porradeiro | 4 | 25 | 18000 | 720 | 18000 | 5400 | 0 | 0 |
| 🧞 Gênio | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🧞 Gênio | 🪔 Desejo | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🧞 Gênio | 🔮 Profecia | 3 | 34 | 7752 | 228 | 31008 | 0 | 0 | +3672 |
| 🧜 Sereia | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🧜 Sereia | 🧜‍♀️ Canto de Sereia | 4 | 25 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🧜 Sereia | 🌊 Atlantis | 4 | 25 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🧚 Fada | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🧚 Fada | 🔔 Sininho | 3 | 34 | 8160 | 240 | 8160 | 0 | 0 | 0 |
| 🧚 Fada | ✨ Pó Mágico | 4 | 25 | 3000 | 120 | 12000 | 0 | 0 | 0 |
| 🐲 Dragão | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🐲 Dragão | 🔥 Sopro do Dragão | 3 | 34 | 15440 | 454 | 61760 | 0 | 10000 | +10000 |
| 🐲 Dragão | 🐲 Dragão Protetor | 3 | 34 | 0 | 0 | 0 | 17000 | 0 | 0 |
| 💩 Cocô | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 💩 Cocô | 🚽 Descarga | 3 | 34 | 16800 | 494 | 16800 | 0 | 10000 | +10000 |
| 💩 Cocô | 🪠 Desentupidor | 3 | 34 | 14080 | 414 | 56320 | 0 | 10000 | +10000 |
| 🦸 Herói | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🦸 Herói | 🦸 Salvando o Dia | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🦸 Herói | 💪 Super | 3 | 34 | 6800 | 200 | 27200 | 0 | 0 | 0 |
| 🦹 Vilão | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🦹 Vilão | 🦹 Destruindo o Dia | 3 | 34 | 5440 | 160 | 21760 | 0 | 0 | 0 |
| 🦹 Vilão | 👿 Vilania | 4 | 25 | 12000 | 480 | 12000 | 0 | 0 | 0 |
| 🦖 T-Rex | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🦖 T-Rex | 🦖 Rugido | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🦖 T-Rex | 🦶 Pisada | 3 | 34 | 6120 | 180 | 24480 | 0 | 0 | 0 |
| 🦇 Morcego | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 1200 | 0 | 0 |
| 🦇 Morcego | 🦇 Mordida | 3 | 34 | 5440 | 160 | 21760 | 1632 | 0 | 0 |
| 🦇 Morcego | 🐀 Rato Voador | 4 | 25 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🧛 Vampiro | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🧛 Vampiro | 🩸 Controle de Sangue | 3 | 34 | 20400 | 600 | 20400 | 0 | 0 | 0 |
| 🧛 Vampiro | 🌙 Vampiro Primordial | 4 | 25 | 2000 | 80 | 2000 | 0 | 0 | 0 |
| 🧝 Elfo | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🧝 Elfo | 🌳 Árvore do Mundo | 3 | 34 | 0 | 0 | 0 | 0 | 0 | 0 |
| 🧝 Elfo | 🌿 Natureza | 3 | 34 | 4080 | 120 | 4080 | 0 | 0 | 0 |
| 😈 Diabo | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 😈 Diabo | 🔥 Inferno | 3 | 34 | 6800 | 200 | 27200 | 0 | 0 | +6800 |
| 😈 Diabo | 😇 Anjo Caído | 3 | 34 | 0 | 0 | 0 | 20400 | 0 | 0 |
| ⛄ Boneco de Neve | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| ⛄ Boneco de Neve | ⛄ Bola de Neve | 3 | 34 | 7480 | 220 | 7480 | 0 | 0 | 0 |
| ⛄ Boneco de Neve | ❄️ Gelado | 4 | 25 | 3500 | 140 | 14000 | 0 | 0 | 0 |
| 🎭 Mímico | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🎭 Mímico | 🎭 Imitação | 3 | 34 | 3400 | 100 | 13600 | 0 | 0 | 0 |
| 🎭 Mímico | 📋 Copiando | 4 | 25 | 0 | 0 | 0 | 0 | 0 | 0 |
| 😇 Anjo | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 10000 | 0 | 0 |
| 😇 Anjo | 🌟 Celestial | 3 | 34 | 0 | 0 | 0 | 30400 | 0 | 0 |
| 😇 Anjo | ☁️ Céu | 4 | 25 | 0 | 0 | 0 | 10000 | 0 | 0 |
| 🎅 Papai Noel | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 8000 | 0 | 0 | 0 |
| 🎅 Papai Noel | 🎅 Saco de Presente | 3 | 34 | 5916 | 174 | 23664 | 0 | 0 | 0 |
| 🎅 Papai Noel | 🏭 Fábrica de Presente | 3 | 34 | 9044 | 266 | 36176 | 0 | 0 | +4284 |

## Rankings (condições da linha 1: DEF 0, alvo imune)

A tabela por champ acima responde "como é o kit deste personagem?".
Estas respondem "quem está fora da curva?".

### Dano por uso — o BURST

| # | Champ | Habilidade | CD | Valor |
|--:|---|---|--:|--:|
| 1 | 🧌 Troll | 🥊 Porradeiro | 4 | 2880 |
| 2 | 🦹 Vilão | 👿 Vilania | 4 | 1920 |
| 3 | 👺 Tengu | 🌪️ Vendaval | 4 | 1000 |
| 4 | 💂 Guarda | 🤺 Esgrima | 3 | 960 |
| 5 | 🥷 Ninja | 🌟 Shuriken | 3 | 960 |
| 6 | 🧚 Fada | 🔔 Sininho | 3 | 960 |
| 7 | 🧛 Vampiro | 🩸 Controle de Sangue | 3 | 960 |
| 8 | ⛄ Boneco de Neve | ⛄ Bola de Neve | 3 | 880 |
| 9 | 💩 Cocô | 🚽 Descarga | 3 | 800 |
| 10 | 🦸 Herói | 💪 Super | 3 | 800 |
| 11 | 👾 Invasor | 🪳 Barata | 3 | 731 |
| 12 | 🦖 T-Rex | 🦶 Pisada | 3 | 720 |
| 13 | 🎅 Papai Noel | 🎅 Saco de Presente | 3 | 699 |
| 14 | 👾 Invasor | 📺 Glitch | 3 | 684 |
| 15 | 🧙 Mago | 🔥 Bola de Fogo | 4 | 640 |
| 16 | 👹 Ogro | 💥 Quebrar | 3 | 640 |
| 17 | 🐲 Dragão | 🔥 Sopro do Dragão | 3 | 640 |
| 18 | 🦹 Vilão | 🦹 Destruindo o Dia | 3 | 640 |
| 19 | 🦇 Morcego | 🦇 Mordida | 3 | 640 |
| 20 | 💀 Caveira | 🦴 Ossinho | 3 | 638 |
| 21 | 🧌 Troll | 🤜 Pancada | 3 | 560 |
| 22 | ⛄ Boneco de Neve | ❄️ Gelado | 4 | 560 |
| 23 | 🎅 Papai Noel | 🏭 Fábrica de Presente | 3 | 560 |
| 24 | 👮 Policial | 🔫 Tiroteio | 4 | 480 |
| 25 | 🥷 Ninja | 🗡️ Kunai | 4 | 480 |
| 26 | 🧙 Mago | 🌋 Incêndio | 4 | 480 |
| 27 | 👻 Fantasma | 💀 Vindo do Além | 3 | 480 |
| 28 | 🧞 Gênio | 🔮 Profecia | 3 | 480 |
| 29 | 🧚 Fada | ✨ Pó Mágico | 4 | 480 |
| 30 | 💩 Cocô | 🪠 Desentupidor | 3 | 480 |
| 31 | 🧝 Elfo | 🌿 Natureza | 3 | 480 |
| 32 | 👷 Operário | 🔨 Marretada | 3 | 400 |
| 33 | 👺 Tengu | ⚔️ Atacar | 0 | 400 |
| 34 | 👺 Tengu | 🌬️ Corte de Vento | 3 | 400 |
| 35 | 🎭 Mímico | 🎭 Imitação | 3 | 400 |
| 36 | 👾 Invasor | ⚔️ Atacar | 0 | 368 |
| 37 | 👷 Operário | ⚔️ Atacar | 0 | 320 |
| 38 | 🕵️ Detetive | ⚔️ Atacar | 0 | 320 |
| 39 | 🕵️ Detetive | 🕳️ Furtividade | 4 | 320 |
| 40 | 👮 Policial | ⚔️ Atacar | 0 | 320 |
| 41 | 👲 Sushiman  | ⚔️ Atacar | 0 | 320 |
| 42 | 💂 Guarda | ⚔️ Atacar | 0 | 320 |
| 43 | 🥷 Ninja | ⚔️ Atacar | 0 | 320 |
| 44 | 🧙 Mago | ⚔️ Atacar | 0 | 320 |
| 45 | 🫅 Rei | ⚔️ Atacar | 0 | 320 |
| 46 | 💀 Caveira | ⚔️ Atacar | 0 | 320 |
| 47 | 👻 Fantasma | ⚔️ Atacar | 0 | 320 |
| 48 | 👻 Fantasma | 👻 Assombração | 3 | 320 |
| 49 | 🎃 Abóbora | ⚔️ Atacar | 0 | 320 |
| 50 | 🧟 Zumbi | ⚔️ Atacar | 0 | 320 |
| 51 | 🧟 Zumbi | 🤢 Vômito Tóxico | 4 | 320 |
| 52 | 🧟 Zumbi | 💀 Putrefação | 4 | 320 |
| 53 | 👽 Alien | ⚔️ Atacar | 0 | 320 |
| 54 | 🤖 Robô | ⚔️ Atacar | 0 | 320 |
| 55 | 🧑‍🔬 Cientista | ⚔️ Atacar | 0 | 320 |
| 56 | 🧑‍🔬 Cientista | 🧪 Química | 3 | 320 |
| 57 | 🧑‍🔬 Cientista | ⚛️ Física | 3 | 320 |
| 58 | 👹 Ogro | ⚔️ Atacar | 0 | 320 |
| 59 | 🤡 Palhaço | ⚔️ Atacar | 0 | 320 |
| 60 | 🧌 Troll | ⚔️ Atacar | 0 | 320 |
| 61 | 🧞 Gênio | ⚔️ Atacar | 0 | 320 |
| 62 | 🧜 Sereia | ⚔️ Atacar | 0 | 320 |
| 63 | 🧚 Fada | ⚔️ Atacar | 0 | 320 |
| 64 | 🐲 Dragão | ⚔️ Atacar | 0 | 320 |
| 65 | 💩 Cocô | ⚔️ Atacar | 0 | 320 |
| 66 | 🦸 Herói | ⚔️ Atacar | 0 | 320 |
| 67 | 🦹 Vilão | ⚔️ Atacar | 0 | 320 |
| 68 | 🦖 T-Rex | ⚔️ Atacar | 0 | 320 |
| 69 | 🦇 Morcego | ⚔️ Atacar | 0 | 320 |
| 70 | 🧛 Vampiro | ⚔️ Atacar | 0 | 320 |
| 71 | 🧛 Vampiro | 🌙 Vampiro Primordial | 4 | 320 |
| 72 | 🧝 Elfo | ⚔️ Atacar | 0 | 320 |
| 73 | 😈 Diabo | ⚔️ Atacar | 0 | 320 |
| 74 | ⛄ Boneco de Neve | ⚔️ Atacar | 0 | 320 |
| 75 | 🎭 Mímico | ⚔️ Atacar | 0 | 320 |
| 76 | 😇 Anjo | ⚔️ Atacar | 0 | 320 |
| 77 | 🎅 Papai Noel | ⚔️ Atacar | 0 | 320 |

### Dano em 100 turnos, 4 alvos — o SUSTENTADO com área

| # | Champ | Habilidade | CD | Valor |
|--:|---|---|--:|--:|
| 1 | 🦸 Herói | 💪 Super | 3 | 108800 |
| 2 | 🦖 T-Rex | 🦶 Pisada | 3 | 97920 |
| 3 | 🎅 Papai Noel | 🎅 Saco de Presente | 3 | 95064 |
| 4 | 👹 Ogro | 💥 Quebrar | 3 | 87040 |
| 5 | 🐲 Dragão | 🔥 Sopro do Dragão | 3 | 87040 |
| 6 | 🦹 Vilão | 🦹 Destruindo o Dia | 3 | 87040 |
| 7 | 🦇 Morcego | 🦇 Mordida | 3 | 87040 |
| 8 | 💀 Caveira | 🦴 Ossinho | 3 | 86768 |
| 9 | 🧌 Troll | 🤜 Pancada | 3 | 76160 |
| 10 | 🎅 Papai Noel | 🏭 Fábrica de Presente | 3 | 76160 |
| 11 | 🧌 Troll | 🥊 Porradeiro | 4 | 72000 |
| 12 | 🧞 Gênio | 🔮 Profecia | 3 | 65280 |
| 13 | 💩 Cocô | 🪠 Desentupidor | 3 | 65280 |
| 14 | ⛄ Boneco de Neve | ❄️ Gelado | 4 | 56000 |
| 15 | 👺 Tengu | 🌬️ Corte de Vento | 3 | 54400 |
| 16 | 🎭 Mímico | 🎭 Imitação | 3 | 54400 |
| 17 | 🧙 Mago | 🌋 Incêndio | 4 | 48000 |
| 18 | 🧚 Fada | ✨ Pó Mágico | 4 | 48000 |
| 19 | 🦹 Vilão | 👿 Vilania | 4 | 48000 |
| 20 | 👻 Fantasma | 👻 Assombração | 3 | 43520 |
| 21 | 🧑‍🔬 Cientista | 🧪 Química | 3 | 43520 |
| 22 | 🧑‍🔬 Cientista | ⚛️ Física | 3 | 43520 |
| 23 | 👺 Tengu | ⚔️ Atacar | 0 | 40000 |
| 24 | 👾 Invasor | ⚔️ Atacar | 0 | 36850 |
| 25 | 💂 Guarda | 🤺 Esgrima | 3 | 32640 |
| 26 | 🥷 Ninja | 🌟 Shuriken | 3 | 32640 |
| 27 | 🧚 Fada | 🔔 Sininho | 3 | 32640 |
| 28 | 🧛 Vampiro | 🩸 Controle de Sangue | 3 | 32640 |
| 29 | 👷 Operário | ⚔️ Atacar | 0 | 32000 |
| 30 | 🕵️ Detetive | ⚔️ Atacar | 0 | 32000 |
| 31 | 🕵️ Detetive | 🕳️ Furtividade | 4 | 32000 |
| 32 | 👮 Policial | ⚔️ Atacar | 0 | 32000 |
| 33 | 👲 Sushiman  | ⚔️ Atacar | 0 | 32000 |
| 34 | 💂 Guarda | ⚔️ Atacar | 0 | 32000 |
| 35 | 🥷 Ninja | ⚔️ Atacar | 0 | 32000 |
| 36 | 🧙 Mago | ⚔️ Atacar | 0 | 32000 |
| 37 | 🫅 Rei | ⚔️ Atacar | 0 | 32000 |
| 38 | 💀 Caveira | ⚔️ Atacar | 0 | 32000 |
| 39 | 👻 Fantasma | ⚔️ Atacar | 0 | 32000 |
| 40 | 🎃 Abóbora | ⚔️ Atacar | 0 | 32000 |
| 41 | 🧟 Zumbi | ⚔️ Atacar | 0 | 32000 |
| 42 | 🧟 Zumbi | 🤢 Vômito Tóxico | 4 | 32000 |
| 43 | 🧟 Zumbi | 💀 Putrefação | 4 | 32000 |
| 44 | 👽 Alien | ⚔️ Atacar | 0 | 32000 |
| 45 | 🤖 Robô | ⚔️ Atacar | 0 | 32000 |
| 46 | 🧑‍🔬 Cientista | ⚔️ Atacar | 0 | 32000 |
| 47 | 👹 Ogro | ⚔️ Atacar | 0 | 32000 |
| 48 | 🤡 Palhaço | ⚔️ Atacar | 0 | 32000 |
| 49 | 🧌 Troll | ⚔️ Atacar | 0 | 32000 |
| 50 | 🧞 Gênio | ⚔️ Atacar | 0 | 32000 |
| 51 | 🧜 Sereia | ⚔️ Atacar | 0 | 32000 |
| 52 | 🧚 Fada | ⚔️ Atacar | 0 | 32000 |
| 53 | 🐲 Dragão | ⚔️ Atacar | 0 | 32000 |
| 54 | 💩 Cocô | ⚔️ Atacar | 0 | 32000 |
| 55 | 🦸 Herói | ⚔️ Atacar | 0 | 32000 |
| 56 | 🦹 Vilão | ⚔️ Atacar | 0 | 32000 |
| 57 | 🦖 T-Rex | ⚔️ Atacar | 0 | 32000 |
| 58 | 🦇 Morcego | ⚔️ Atacar | 0 | 32000 |
| 59 | 🧛 Vampiro | ⚔️ Atacar | 0 | 32000 |
| 60 | 🧝 Elfo | ⚔️ Atacar | 0 | 32000 |
| 61 | 😈 Diabo | ⚔️ Atacar | 0 | 32000 |
| 62 | ⛄ Boneco de Neve | ⚔️ Atacar | 0 | 32000 |
| 63 | 🎭 Mímico | ⚔️ Atacar | 0 | 32000 |
| 64 | 😇 Anjo | ⚔️ Atacar | 0 | 32000 |
| 65 | 🎅 Papai Noel | ⚔️ Atacar | 0 | 32000 |
| 66 | ⛄ Boneco de Neve | ⛄ Bola de Neve | 3 | 29920 |
| 67 | 💩 Cocô | 🚽 Descarga | 3 | 27200 |
| 68 | 👺 Tengu | 🌪️ Vendaval | 4 | 25000 |
| 69 | 👾 Invasor | 🪳 Barata | 3 | 24860 |
| 70 | 👾 Invasor | 📺 Glitch | 3 | 23283 |
| 71 | 👻 Fantasma | 💀 Vindo do Além | 3 | 16320 |
| 72 | 🧝 Elfo | 🌿 Natureza | 3 | 16320 |
| 73 | 🧙 Mago | 🔥 Bola de Fogo | 4 | 16000 |
| 74 | 👷 Operário | 🔨 Marretada | 3 | 13600 |
| 75 | 👮 Policial | 🔫 Tiroteio | 4 | 12000 |
| 76 | 🥷 Ninja | 🗡️ Kunai | 4 | 12000 |
| 77 | 🧛 Vampiro | 🌙 Vampiro Primordial | 4 | 8000 |

### Cura em 100 turnos

| # | Champ | Habilidade | CD | Valor |
|--:|---|---|--:|--:|
| 1 | 👹 Ogro | 👊 Esmagar | 3 | 34000 |
| 2 | 😇 Anjo | 🌟 Celestial | 3 | 30400 |
| 3 | 🧌 Troll | 🥊 Porradeiro | 4 | 21600 |
| 4 | 🫅 Rei | 🗳️ Democracia | 3 | 20400 |
| 5 | 😈 Diabo | 😇 Anjo Caído | 3 | 20400 |
| 6 | 🐲 Dragão | 🐲 Dragão Protetor | 3 | 17000 |
| 7 | 👲 Sushiman  | 🍣 Sushi | 4 | 15000 |
| 8 | 🤖 Robô | ⚔️ Atacar | 0 | 10000 |
| 9 | 😇 Anjo | ⚔️ Atacar | 0 | 10000 |
| 10 | 😇 Anjo | ☁️ Céu | 4 | 10000 |
| 11 | 🤖 Robô | 🩻 Raio-X | 4 | 7500 |
| 12 | 🤖 Robô | 🤖 Technology | 4 | 5000 |
| 13 | 🦇 Morcego | ⚔️ Atacar | 0 | 4800 |
| 14 | 🦇 Morcego | 🦇 Mordida | 3 | 3264 |
| 15 | 👻 Fantasma | 👻 Assombração | 3 | 2176 |
| 16 | 🧟 Zumbi | 💀 Putrefação | 4 | 1600 |

