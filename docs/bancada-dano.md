# Bancada de dano

> **Gerado por `ApostlesWar.Tests/Bancada/BancadaDeDano.cs`.** Não edite à mão —
> rode `dotnet test` e o arquivo se reescreve. É versionado de propósito: cada tweak
> de número vira um `git diff` legível.

## Condições

- **100 turnos** por medição, média de **10 repetições**.
- Stats IGUAIS pra todos: HP 1.000.000, ATK 200, DEF 0. **Crítico 100%**.
- Na medição por habilidade, o champ usa **só aquela** e **espera** durante o cooldown
  (não enche o buraco com A1 — se enchesse, o A1 dominaria e todas ficariam iguais).
- No champ inteiro, quem decide é o **mesmo `ControladorBot`** da Arena e do modo Auto.
- Boneco: HP 2.000, **ataque 0**, DEF 0 ou 1000 (o cap de 75% de redução).
  Ele volta ao HP cheio antes de cada golpe — o HP é REALISTA de propósito, porque a
  Queima tira 5% do HP máximo por turno e um boneco inflado faria o DoT explodir.

### O que este relatório NÃO mede

O boneco **não revida**. Contra-ataque, espinhos e revide (Herói, Operário, Zumbi)
medem **zero** aqui: isto é uma bancada de dano CAUSADO, não de duelo. Um champ
com número baixo pode ser reativo, não fraco — confira o kit antes de mexer.

A coluna **Usos** é diagnóstico do BOT: se uma habilidade dispara 0× no champ
inteiro mas tem dano alto isolada, o problema está na fila do bot, não no balanço.

---

## Linha 1 — por habilidade · boneco DEF 0 · imune a malefícios

Dano cru. Sem defesa no alvo, quem "fura defesa" não distorce a comparação.

| Champ | Habilidade | CD | Usos | Dano total | Dano por uso |
|---|---|--:|--:|--:|--:|
| 👷 Operário | ⚔️ Atacar | 0 | 100 | 36800 | 368 |
| 👷 Operário | 🧱 Parede de Tijolos | 6 | 17 | 4000 | 235 |
| 👷 Operário | 🔨 Marretada | 3 | 34 | 17240 | 507 |
| 🕵️ Detetive | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🕵️ Detetive | 🔎 Espionagem | 4 | 25 | 0 | 0 |
| 🕵️ Detetive | 🕳️ Furtividade | 4 | 25 | 8000 | 320 |
| 👮 Policial | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 👮 Policial | 🔫 Tiroteio | 4 | 25 | 12000 | 480 |
| 👮 Policial | ⛓️ Prender | 4 | 25 | 0 | 0 |
| 👲 Sushiman  | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 👲 Sushiman  | 🍣 Sushi | 4 | 25 | 0 | 0 |
| 👲 Sushiman  | 🍙 Nigiri | 4 | 25 | 0 | 0 |
| 💂 Guarda | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 💂 Guarda | 🛡️ Protetor | 4 | 25 | 0 | 0 |
| 💂 Guarda | 🤺 Esgrima | 3 | 34 | 32640 | 960 |
| 🥷 Ninja | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🥷 Ninja | 🌟 Shuriken | 3 | 34 | 32640 | 960 |
| 🥷 Ninja | 🗡️ Kunai | 4 | 25 | 12000 | 480 |
| 🧙 Mago | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🧙 Mago | 🔥 Bola de Fogo | 4 | 25 | 16000 | 640 |
| 🧙 Mago | 🌋 Incêndio | 4 | 25 | 12000 | 480 |
| 🫅 Rei | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🫅 Rei | 🗳️ Democracia | 3 | 34 | 0 | 0 |
| 🫅 Rei | 🎖️ Lealdade | 3 | 34 | 0 | 0 |
| 💀 Caveira | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 💀 Caveira | 🦴 Ossinho | 3 | 34 | 10880 | 320 |
| 💀 Caveira | 🦴 Osso Duro de Roer | 3 | 34 | 0 | 0 |
| 👻 Fantasma | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 👻 Fantasma | 👻 Assombração | 3 | 34 | 10880 | 320 |
| 👻 Fantasma | 💀 Vindo do Além | 3 | 34 | 16320 | 480 |
| 🎃 Abóbora | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🎃 Abóbora | 🍬 Doces ou Travessuras | 4 | 25 | 0 | 0 |
| 🎃 Abóbora | 🍭 Doces de Abóbora | 4 | 25 | 0 | 0 |
| 🧟 Zumbi | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🧟 Zumbi | 🤢 Vômito Tóxico | 4 | 25 | 8000 | 320 |
| 🧟 Zumbi | 💀 Putrefação | 4 | 25 | 8000 | 320 |
| 👾 Invasor | ⚔️ Atacar | 0 | 100 | 36850 | 368 |
| 👾 Invasor | 📺 Glitch | 3 | 34 | 23283 | 684 |
| 👾 Invasor | 🪳 Barata | 3 | 34 | 24860 | 731 |
| 👽 Alien | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 👽 Alien | 🛸 Abduzir | 4 | 25 | 0 | 0 |
| 👽 Alien | 🌌 Galáxia | 4 | 25 | 0 | 0 |
| 🤖 Robô | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🤖 Robô | 🩻 Raio-X | 4 | 25 | 0 | 0 |
| 🤖 Robô | 🤖 Technology | 4 | 25 | 0 | 0 |
| 🧑‍🔬 Cientista | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🧑‍🔬 Cientista | 🧪 Química | 3 | 34 | 10880 | 320 |
| 🧑‍🔬 Cientista | ⚛️ Física | 3 | 34 | 10880 | 320 |
| 👹 Ogro | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 👹 Ogro | 👊 Esmagar | 3 | 34 | 0 | 0 |
| 👹 Ogro | 💥 Quebrar | 3 | 34 | 21760 | 640 |
| 👺 Tengu | ⚔️ Atacar | 0 | 100 | 40000 | 400 |
| 👺 Tengu | 🌬️ Corte de Vento | 3 | 34 | 13600 | 400 |
| 👺 Tengu | 🌪️ Vendaval | 4 | 25 | 25000 | 1000 |
| 🤡 Palhaço | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🤡 Palhaço | 🃏 Coringa | 3 | 34 | 0 | 0 |
| 🤡 Palhaço | 🎪 Circo | 4 | 25 | 0 | 0 |
| 🧌 Troll | ⚔️ Atacar | 0 | 100 | 39570 | 395 |
| 🧌 Troll | 🤜 Pancada | 3 | 34 | 23475 | 690 |
| 🧌 Troll | 🥊 Porradeiro | 4 | 1 | 2880 | 2880 |
| 🧞 Gênio | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🧞 Gênio | 🪔 Desejo | 3 | 34 | 0 | 0 |
| 🧞 Gênio | 🔮 Profecia | 3 | 34 | 16320 | 480 |
| 🧜 Sereia | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🧜 Sereia | 🧜‍♀️ Canto de Sereia | 4 | 25 | 0 | 0 |
| 🧜 Sereia | 🌊 Atlantis | 4 | 25 | 0 | 0 |
| 🧚 Fada | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🧚 Fada | 🔔 Sininho | 3 | 34 | 32640 | 960 |
| 🧚 Fada | ✨ Pó Mágico | 4 | 25 | 12000 | 480 |
| 🐲 Dragão | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🐲 Dragão | 🔥 Sopro do Dragão | 3 | 34 | 21760 | 640 |
| 🐲 Dragão | 🐲 Dragão Protetor | 3 | 34 | 21440 | 630 |
| 💩 Cocô | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 💩 Cocô | 🚽 Descarga | 3 | 34 | 27200 | 800 |
| 💩 Cocô | 🪠 Desentupidor | 3 | 34 | 16320 | 480 |
| 🦸 Herói | ⚔️ Atacar | 0 | 100 | 64000 | 640 |
| 🦸 Herói | 🦸 Salvando o Dia | 3 | 34 | 32000 | 941 |
| 🦸 Herói | 💪 Super | 3 | 34 | 64560 | 1898 |
| 🦹 Vilão | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🦹 Vilão | 🦹 Destruindo o Dia | 3 | 34 | 21760 | 640 |
| 🦹 Vilão | 👿 Vilania | 4 | 25 | 48000 | 1920 |
| 🦖 T-Rex | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🦖 T-Rex | 🦖 Rugido | 3 | 34 | 0 | 0 |
| 🦖 T-Rex | 🦶 Pisada | 3 | 34 | 45920 | 1350 |
| 🦇 Morcego | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🦇 Morcego | 🦇 Mordida | 3 | 34 | 21760 | 640 |
| 🦇 Morcego | 🐀 Rato Voador | 4 | 25 | 0 | 0 |
| 🧛 Vampiro | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🧛 Vampiro | 🩸 Controle de Sangue | 3 | 34 | 32640 | 960 |
| 🧛 Vampiro | 🌙 Vampiro Primordial | 4 | 25 | 8000 | 320 |
| 🧝 Elfo | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🧝 Elfo | 🌳 Árvore do Mundo | 3 | 34 | 21440 | 630 |
| 🧝 Elfo | 🌿 Natureza | 3 | 34 | 16320 | 480 |
| 😈 Diabo | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 😈 Diabo | 🔥 Inferno | 3 | 34 | 0 | 0 |
| 😈 Diabo | 😇 Anjo Caído | 3 | 34 | 0 | 0 |
| ⛄ Boneco de Neve | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| ⛄ Boneco de Neve | ⛄ Bola de Neve | 3 | 34 | 29920 | 880 |
| ⛄ Boneco de Neve | ❄️ Gelado | 4 | 25 | 14000 | 560 |
| 🎭 Mímico | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🎭 Mímico | 🎭 Imitação | 3 | 34 | 13600 | 400 |
| 🎭 Mímico | 📋 Copiando | 4 | 25 | 0 | 0 |
| 😇 Anjo | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 😇 Anjo | 🌟 Celestial | 3 | 34 | 0 | 0 |
| 😇 Anjo | ☁️ Céu | 4 | 25 | 0 | 0 |
| 🎅 Papai Noel | ⚔️ Atacar | 0 | 100 | 32000 | 320 |
| 🎅 Papai Noel | 🎅 Saco de Presente | 3 | 34 | 23766 | 699 |
| 🎅 Papai Noel | 🏭 Fábrica de Presente | 3 | 34 | 19040 | 560 |

## Linha 2 — por habilidade · boneco DEF no cap · imune a malefícios

Mesma coisa com defesa. **(2) − (1) = o que furar/ignorar defesa vale.**

| Champ | Habilidade | CD | Usos | Dano total | Dano por uso |
|---|---|--:|--:|--:|--:|
| 👷 Operário | ⚔️ Atacar | 0 | 100 | 9190 | 91 |
| 👷 Operário | 🧱 Parede de Tijolos | 6 | 17 | 960 | 56 |
| 👷 Operário | 🔨 Marretada | 3 | 34 | 4320 | 127 |
| 🕵️ Detetive | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🕵️ Detetive | 🔎 Espionagem | 4 | 25 | 0 | 0 |
| 🕵️ Detetive | 🕳️ Furtividade | 4 | 25 | 2000 | 80 |
| 👮 Policial | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 👮 Policial | 🔫 Tiroteio | 4 | 25 | 3000 | 120 |
| 👮 Policial | ⛓️ Prender | 4 | 25 | 0 | 0 |
| 👲 Sushiman  | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 👲 Sushiman  | 🍣 Sushi | 4 | 25 | 0 | 0 |
| 👲 Sushiman  | 🍙 Nigiri | 4 | 25 | 0 | 0 |
| 💂 Guarda | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 💂 Guarda | 🛡️ Protetor | 4 | 25 | 0 | 0 |
| 💂 Guarda | 🤺 Esgrima | 3 | 34 | 8160 | 240 |
| 🥷 Ninja | ⚔️ Atacar | 0 | 100 | 13818 | 138 |
| 🥷 Ninja | 🌟 Shuriken | 3 | 34 | 16273 | 478 |
| 🥷 Ninja | 🗡️ Kunai | 4 | 25 | 10234 | 409 |
| 🧙 Mago | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🧙 Mago | 🔥 Bola de Fogo | 4 | 25 | 4000 | 160 |
| 🧙 Mago | 🌋 Incêndio | 4 | 25 | 3000 | 120 |
| 🫅 Rei | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🫅 Rei | 🗳️ Democracia | 3 | 34 | 0 | 0 |
| 🫅 Rei | 🎖️ Lealdade | 3 | 34 | 0 | 0 |
| 💀 Caveira | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 💀 Caveira | 🦴 Ossinho | 3 | 34 | 2720 | 80 |
| 💀 Caveira | 🦴 Osso Duro de Roer | 3 | 34 | 0 | 0 |
| 👻 Fantasma | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 👻 Fantasma | 👻 Assombração | 3 | 34 | 2720 | 80 |
| 👻 Fantasma | 💀 Vindo do Além | 3 | 34 | 16320 | 480 |
| 🎃 Abóbora | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🎃 Abóbora | 🍬 Doces ou Travessuras | 4 | 25 | 0 | 0 |
| 🎃 Abóbora | 🍭 Doces de Abóbora | 4 | 25 | 0 | 0 |
| 🧟 Zumbi | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🧟 Zumbi | 🤢 Vômito Tóxico | 4 | 25 | 2000 | 80 |
| 🧟 Zumbi | 💀 Putrefação | 4 | 25 | 2000 | 80 |
| 👾 Invasor | ⚔️ Atacar | 0 | 100 | 9164 | 91 |
| 👾 Invasor | 📺 Glitch | 3 | 34 | 5812 | 170 |
| 👾 Invasor | 🪳 Barata | 3 | 34 | 6215 | 182 |
| 👽 Alien | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 👽 Alien | 🛸 Abduzir | 4 | 25 | 0 | 0 |
| 👽 Alien | 🌌 Galáxia | 4 | 25 | 0 | 0 |
| 🤖 Robô | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🤖 Robô | 🩻 Raio-X | 4 | 25 | 0 | 0 |
| 🤖 Robô | 🤖 Technology | 4 | 25 | 0 | 0 |
| 🧑‍🔬 Cientista | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🧑‍🔬 Cientista | 🧪 Química | 3 | 34 | 2720 | 80 |
| 🧑‍🔬 Cientista | ⚛️ Física | 3 | 34 | 2720 | 80 |
| 👹 Ogro | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 👹 Ogro | 👊 Esmagar | 3 | 34 | 0 | 0 |
| 👹 Ogro | 💥 Quebrar | 3 | 34 | 5440 | 160 |
| 👺 Tengu | ⚔️ Atacar | 0 | 100 | 10000 | 100 |
| 👺 Tengu | 🌬️ Corte de Vento | 3 | 34 | 3400 | 100 |
| 👺 Tengu | 🌪️ Vendaval | 4 | 25 | 15625 | 625 |
| 🤡 Palhaço | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🤡 Palhaço | 🃏 Coringa | 3 | 34 | 0 | 0 |
| 🤡 Palhaço | 🎪 Circo | 4 | 25 | 0 | 0 |
| 🧌 Troll | ⚔️ Atacar | 0 | 100 | 9845 | 98 |
| 🧌 Troll | 🤜 Pancada | 3 | 34 | 5868 | 172 |
| 🧌 Troll | 🥊 Porradeiro | 4 | 25 | 22146 | 885 |
| 🧞 Gênio | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🧞 Gênio | 🪔 Desejo | 3 | 34 | 0 | 0 |
| 🧞 Gênio | 🔮 Profecia | 3 | 34 | 4080 | 120 |
| 🧜 Sereia | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🧜 Sereia | 🧜‍♀️ Canto de Sereia | 4 | 25 | 0 | 0 |
| 🧜 Sereia | 🌊 Atlantis | 4 | 25 | 0 | 0 |
| 🧚 Fada | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🧚 Fada | 🔔 Sininho | 3 | 34 | 8160 | 240 |
| 🧚 Fada | ✨ Pó Mágico | 4 | 25 | 3000 | 120 |
| 🐲 Dragão | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🐲 Dragão | 🔥 Sopro do Dragão | 3 | 34 | 5440 | 160 |
| 🐲 Dragão | 🐲 Dragão Protetor | 3 | 34 | 5360 | 157 |
| 💩 Cocô | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 💩 Cocô | 🚽 Descarga | 3 | 34 | 6800 | 200 |
| 💩 Cocô | 🪠 Desentupidor | 3 | 34 | 4080 | 120 |
| 🦸 Herói | ⚔️ Atacar | 0 | 100 | 16000 | 160 |
| 🦸 Herói | 🦸 Salvando o Dia | 3 | 34 | 8000 | 235 |
| 🦸 Herói | 💪 Super | 3 | 34 | 16140 | 474 |
| 🦹 Vilão | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🦹 Vilão | 🦹 Destruindo o Dia | 3 | 34 | 5440 | 160 |
| 🦹 Vilão | 👿 Vilania | 4 | 25 | 12000 | 480 |
| 🦖 T-Rex | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🦖 T-Rex | 🦖 Rugido | 3 | 34 | 0 | 0 |
| 🦖 T-Rex | 🦶 Pisada | 3 | 34 | 11480 | 337 |
| 🦇 Morcego | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🦇 Morcego | 🦇 Mordida | 3 | 34 | 5440 | 160 |
| 🦇 Morcego | 🐀 Rato Voador | 4 | 25 | 0 | 0 |
| 🧛 Vampiro | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🧛 Vampiro | 🩸 Controle de Sangue | 3 | 34 | 20400 | 600 |
| 🧛 Vampiro | 🌙 Vampiro Primordial | 4 | 25 | 2000 | 80 |
| 🧝 Elfo | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🧝 Elfo | 🌳 Árvore do Mundo | 3 | 34 | 5360 | 157 |
| 🧝 Elfo | 🌿 Natureza | 3 | 34 | 4080 | 120 |
| 😈 Diabo | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 😈 Diabo | 🔥 Inferno | 3 | 34 | 0 | 0 |
| 😈 Diabo | 😇 Anjo Caído | 3 | 34 | 0 | 0 |
| ⛄ Boneco de Neve | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| ⛄ Boneco de Neve | ⛄ Bola de Neve | 3 | 34 | 7480 | 220 |
| ⛄ Boneco de Neve | ❄️ Gelado | 4 | 25 | 3500 | 140 |
| 🎭 Mímico | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🎭 Mímico | 🎭 Imitação | 3 | 34 | 3400 | 100 |
| 🎭 Mímico | 📋 Copiando | 4 | 25 | 0 | 0 |
| 😇 Anjo | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 😇 Anjo | 🌟 Celestial | 3 | 34 | 0 | 0 |
| 😇 Anjo | ☁️ Céu | 4 | 25 | 0 | 0 |
| 🎅 Papai Noel | ⚔️ Atacar | 0 | 100 | 8000 | 80 |
| 🎅 Papai Noel | 🎅 Saco de Presente | 3 | 34 | 5916 | 174 |
| 🎅 Papai Noel | 🏭 Fábrica de Presente | 3 | 34 | 4760 | 140 |

## Linha 3 — champ inteiro · boneco DEF no cap · imune a malefícios

O champ jogando com o cérebro do bot. **Sinergia = real − esperado**, onde o esperado aplica o dano-por-uso da linha 2 às ativações que de fato aconteceram aqui. Positivo = as habilidades valem mais juntas do que separadas.

| Champ | Dano total | Esperado (isolado × usos) | Sinergia | Habilidades usadas |
|---|--:|--:|--:|---|
| 👷 Operário | 8480 | 9693 | -1213 | Atacar 50×, Parede de Tijolos 17×, Marretada 33× |
| 🕵️ Detetive | 8000 | 8000 | 0 | Atacar 75×, Espionagem 0×, Furtividade 25× |
| 👮 Policial | 9000 | 9000 | 0 | Atacar 75×, Tiroteio 25×, Prender 0× |
| 👲 Sushiman  | 7000 | 6000 | +1000 | Atacar 75×, Sushi 0×, Nigiri 25× |
| 💂 Guarda | 10000 | 10000 | 0 | Atacar 50×, Protetor 25×, Esgrima 25× |
| 🥷 Ninja | 29290 | 29075 | +215 | Atacar 50×, Shuriken 25×, Kunai 25× |
| 🧙 Mago | 11000 | 11000 | 0 | Atacar 50×, Bola de Fogo 25×, Incêndio 25× |
| 🫅 Rei | 5280 | 5280 | 0 | Atacar 66×, Democracia 0×, Lealdade 34× |
| 💀 Caveira | 5280 | 5280 | 0 | Atacar 33×, Ossinho 33×, Osso Duro de Roer 34× |
| 👻 Fantasma | 21200 | 21200 | 0 | Atacar 33×, Assombração 34×, Vindo do Além 33× |
| 🎃 Abóbora | 6000 | 6000 | 0 | Atacar 75×, Doces ou Travessuras 0×, Doces de Abóbora 25× |
| 🧟 Zumbi | 8000 | 8000 | 0 | Atacar 50×, Vômito Tóxico 25×, Putrefação 25× |
| 👾 Invasor | 17241 | 14801 | +2440 | Atacar 33×, Glitch 33×, Barata 34× |
| 👽 Alien | 6000 | 6000 | 0 | Atacar 75×, Abduzir 0×, Galáxia 25× |
| 🤖 Robô | 3235 | 4720 | -1485 | Atacar 59×, Raio-X 21×, Technology 20× |
| 🧑‍🔬 Cientista | 8000 | 8000 | 0 | Atacar 33×, Química 34×, Física 33× |
| 👹 Ogro | 10720 | 10720 | 0 | Atacar 66×, Esmagar 0×, Quebrar 34× |
| 👺 Tengu | 21550 | 21550 | 0 | Atacar 44×, Corte de Vento 34×, Vendaval 22× |
| 🤡 Palhaço | 5280 | 5280 | 0 | Atacar 66×, Coringa 34×, Circo 0× |
| 🧌 Troll | 29740 | 29630 | +110 | Atacar 44×, Pancada 34×, Porradeiro 22× |
| 🧞 Gênio | 6600 | 6600 | 0 | Atacar 33×, Desejo 34×, Profecia 33× |
| 🧜 Sereia | 7000 | 6000 | +1000 | Atacar 75×, Canto de Sereia 25×, Atlantis 0× |
| 🧚 Fada | 13000 | 13000 | 0 | Atacar 50×, Sininho 25×, Pó Mágico 25× |
| 🐲 Dragão | 13280 | 13258 | +22 | Atacar 33×, Sopro do Dragão 33×, Dragão Protetor 34× |
| 💩 Cocô | 13320 | 13320 | 0 | Atacar 33×, Descarga 33×, Desentupidor 34× |
| 🦸 Herói | 19440 | 29151 | -9711 | Atacar 33×, Salvando o Dia 33×, Super 34× |
| 🦹 Vilão | 19520 | 19520 | 0 | Atacar 44×, Destruindo o Dia 34×, Vilania 22× |
| 🦖 T-Rex | 14120 | 14098 | +22 | Atacar 33×, Rugido 33×, Pisada 34× |
| 🦇 Morcego | 9500 | 8000 | +1500 | Atacar 50×, Mordida 25×, Rato Voador 25× |
| 🧛 Vampiro | 21000 | 21000 | 0 | Atacar 50×, Controle de Sangue 25×, Vampiro Primordial 25× |
| 🧝 Elfo | 11960 | 11938 | +22 | Atacar 33×, Árvore do Mundo 34×, Natureza 33× |
| 😈 Diabo | 7920 | 7920 | 0 | Atacar 99×, Inferno 0×, Anjo Caído 1× |
| ⛄ Boneco de Neve | 13000 | 13000 | 0 | Atacar 50×, Bola de Neve 25×, Gelado 25× |
| 🎭 Mímico | 6500 | 6500 | 0 | Atacar 50×, Imitação 25×, Copiando 25× |
| 😇 Anjo | 7000 | 6000 | +1000 | Atacar 75×, Celestial 0×, Céu 25× |
| 🎅 Papai Noel | 14958 | 13176 | +1782 | Atacar 33×, Saco de Presente 34×, Fábrica de Presente 33× |

## Linha 4 — champ inteiro · boneco DEF no cap · RECEBENDO malefícios

O champ completo. **(4) − (3) = o que os malefícios dele valem.**

| Champ | Dano total | Tick | Habilidades usadas |
|---|--:|--:|---|
| 👷 Operário | 8390 | 0 | Atacar 50×, Parede de Tijolos 17×, Marretada 33× |
| 🕵️ Detetive | 9600 | 0 | Atacar 50×, Espionagem 25×, Furtividade 25× |
| 👮 Policial | 7000 | 0 | Atacar 50×, Tiroteio 25×, Prender 25× |
| 👲 Sushiman  | 7000 | 0 | Atacar 75×, Sushi 0×, Nigiri 25× |
| 💂 Guarda | 10000 | 0 | Atacar 50×, Protetor 25×, Esgrima 25× |
| 🥷 Ninja | 29290 | 0 | Atacar 50×, Shuriken 25×, Kunai 25× |
| 🧙 Mago | 19750 | 7500 | Atacar 50×, Bola de Fogo 25×, Incêndio 25× |
| 🫅 Rei | 5280 | 0 | Atacar 66×, Democracia 0×, Lealdade 34× |
| 💀 Caveira | 5280 | 0 | Atacar 33×, Ossinho 33×, Osso Duro de Roer 34× |
| 👻 Fantasma | 21200 | 0 | Atacar 33×, Assombração 34×, Vindo do Além 33× |
| 🎃 Abóbora | 4000 | 0 | Atacar 50×, Doces ou Travessuras 25×, Doces de Abóbora 25× |
| 🧟 Zumbi | 13000 | 2500 | Atacar 50×, Vômito Tóxico 25×, Putrefação 25× |
| 👾 Invasor | 32760 | 0 | Atacar 33×, Glitch 34×, Barata 33× |
| 👽 Alien | 4000 | 0 | Atacar 50×, Abduzir 25×, Galáxia 25× |
| 🤖 Robô | 3196 | 0 | Atacar 59×, Raio-X 21×, Technology 20× |
| 🧑‍🔬 Cientista | 25495 | 17495 | Atacar 33×, Química 34×, Física 33× |
| 👹 Ogro | 10720 | 0 | Atacar 66×, Esmagar 0×, Quebrar 34× |
| 👺 Tengu | 21550 | 0 | Atacar 44×, Corte de Vento 34×, Vendaval 22× |
| 🤡 Palhaço | 5280 | 0 | Atacar 66×, Coringa 34×, Circo 0× |
| 🧌 Troll | 29672 | 0 | Atacar 44×, Pancada 34×, Porradeiro 22× |
| 🧞 Gênio | 12540 | 0 | Atacar 33×, Desejo 34×, Profecia 33× |
| 🧜 Sereia | 7000 | 0 | Atacar 75×, Canto de Sereia 25×, Atlantis 0× |
| 🧚 Fada | 13000 | 0 | Atacar 50×, Sininho 25×, Pó Mágico 25× |
| 🐲 Dragão | 23180 | 9900 | Atacar 33×, Sopro do Dragão 33×, Dragão Protetor 34× |
| 💩 Cocô | 23320 | 10000 | Atacar 33×, Descarga 33×, Desentupidor 34× |
| 🦸 Herói | 19440 | 0 | Atacar 33×, Salvando o Dia 33×, Super 34× |
| 🦹 Vilão | 19520 | 0 | Atacar 44×, Destruindo o Dia 34×, Vilania 22× |
| 🦖 T-Rex | 16335 | 0 | Atacar 33×, Rugido 33×, Pisada 34× |
| 🦇 Morcego | 9500 | 0 | Atacar 50×, Mordida 25×, Rato Voador 25× |
| 🧛 Vampiro | 21000 | 0 | Atacar 50×, Controle de Sangue 25×, Vampiro Primordial 25× |
| 🧝 Elfo | 15170 | 5850 | Atacar 33×, Árvore do Mundo 34×, Natureza 33× |
| 😈 Diabo | 11880 | 0 | Atacar 66×, Inferno 33×, Anjo Caído 1× |
| ⛄ Boneco de Neve | 13000 | 0 | Atacar 50×, Bola de Neve 25×, Gelado 25× |
| 🎭 Mímico | 6500 | 0 | Atacar 50×, Imitação 25×, Copiando 25× |
| 😇 Anjo | 7000 | 0 | Atacar 75×, Celestial 0×, Céu 25× |
| 🎅 Papai Noel | 28356 | 0 | Atacar 33×, Saco de Presente 34×, Fábrica de Presente 33× |

## Linha 5 — por habilidade · boneco DEF no cap · RECEBENDO malefícios

**(5) − (2) por habilidade = de quem é o mérito do malefício.** Sem esta linha, o DoT de uma habilidade (a Queima do Mago) não aparece em número nenhum por-habilidade.

| Champ | Habilidade | CD | Usos | Dano total | Dano por uso | Tick | Δ vs linha 2 |
|---|---|--:|--:|--:|--:|--:|--:|
| 👷 Operário | ⚔️ Atacar | 0 | 100 | 9050 | 90 | 0 | -140 |
| 👷 Operário | 🧱 Parede de Tijolos | 6 | 17 | 1040 | 61 | 0 | +80 |
| 👷 Operário | 🔨 Marretada | 3 | 34 | 4310 | 126 | 0 | -10 |
| 🕵️ Detetive | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 🕵️ Detetive | 🔎 Espionagem | 4 | 25 | 0 | 0 | 0 | 0 |
| 🕵️ Detetive | 🕳️ Furtividade | 4 | 25 | 2000 | 80 | 0 | 0 |
| 👮 Policial | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 👮 Policial | 🔫 Tiroteio | 4 | 25 | 3000 | 120 | 0 | 0 |
| 👮 Policial | ⛓️ Prender | 4 | 25 | 0 | 0 | 0 | 0 |
| 👲 Sushiman  | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 👲 Sushiman  | 🍣 Sushi | 4 | 25 | 0 | 0 | 0 | 0 |
| 👲 Sushiman  | 🍙 Nigiri | 4 | 25 | 0 | 0 | 0 | 0 |
| 💂 Guarda | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 💂 Guarda | 🛡️ Protetor | 4 | 25 | 0 | 0 | 0 | 0 |
| 💂 Guarda | 🤺 Esgrima | 3 | 34 | 8160 | 240 | 0 | 0 |
| 🥷 Ninja | ⚔️ Atacar | 0 | 100 | 13818 | 138 | 0 | 0 |
| 🥷 Ninja | 🌟 Shuriken | 3 | 34 | 16273 | 478 | 0 | 0 |
| 🥷 Ninja | 🗡️ Kunai | 4 | 25 | 10234 | 409 | 0 | 0 |
| 🧙 Mago | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 🧙 Mago | 🔥 Bola de Fogo | 4 | 25 | 11500 | 460 | 7500 | +7500 |
| 🧙 Mago | 🌋 Incêndio | 4 | 25 | 3000 | 120 | 0 | 0 |
| 🫅 Rei | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 🫅 Rei | 🗳️ Democracia | 3 | 34 | 0 | 0 | 0 | 0 |
| 🫅 Rei | 🎖️ Lealdade | 3 | 34 | 0 | 0 | 0 | 0 |
| 💀 Caveira | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 💀 Caveira | 🦴 Ossinho | 3 | 34 | 2720 | 80 | 0 | 0 |
| 💀 Caveira | 🦴 Osso Duro de Roer | 3 | 34 | 0 | 0 | 0 | 0 |
| 👻 Fantasma | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 👻 Fantasma | 👻 Assombração | 3 | 34 | 2720 | 80 | 0 | 0 |
| 👻 Fantasma | 💀 Vindo do Além | 3 | 34 | 16320 | 480 | 0 | 0 |
| 🎃 Abóbora | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 🎃 Abóbora | 🍬 Doces ou Travessuras | 4 | 25 | 0 | 0 | 0 | 0 |
| 🎃 Abóbora | 🍭 Doces de Abóbora | 4 | 25 | 0 | 0 | 0 | 0 |
| 🧟 Zumbi | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 🧟 Zumbi | 🤢 Vômito Tóxico | 4 | 25 | 7000 | 280 | 5000 | +5000 |
| 🧟 Zumbi | 💀 Putrefação | 4 | 25 | 2000 | 80 | 0 | 0 |
| 👾 Invasor | ⚔️ Atacar | 0 | 100 | 9164 | 91 | 0 | 0 |
| 👾 Invasor | 📺 Glitch | 3 | 34 | 11052 | 325 | 0 | +5240 |
| 👾 Invasor | 🪳 Barata | 3 | 34 | 6215 | 182 | 0 | 0 |
| 👽 Alien | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 👽 Alien | 🛸 Abduzir | 4 | 25 | 0 | 0 | 0 | 0 |
| 👽 Alien | 🌌 Galáxia | 4 | 25 | 0 | 0 | 0 | 0 |
| 🤖 Robô | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 🤖 Robô | 🩻 Raio-X | 4 | 25 | 0 | 0 | 0 | 0 |
| 🤖 Robô | 🤖 Technology | 4 | 25 | 0 | 0 | 0 | 0 |
| 🧑‍🔬 Cientista | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 🧑‍🔬 Cientista | 🧪 Química | 3 | 34 | 12720 | 374 | 10000 | +10000 |
| 🧑‍🔬 Cientista | ⚛️ Física | 3 | 34 | 12720 | 374 | 10000 | +10000 |
| 👹 Ogro | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 👹 Ogro | 👊 Esmagar | 3 | 34 | 0 | 0 | 0 | 0 |
| 👹 Ogro | 💥 Quebrar | 3 | 34 | 5440 | 160 | 0 | 0 |
| 👺 Tengu | ⚔️ Atacar | 0 | 100 | 10000 | 100 | 0 | 0 |
| 👺 Tengu | 🌬️ Corte de Vento | 3 | 34 | 3400 | 100 | 0 | 0 |
| 👺 Tengu | 🌪️ Vendaval | 4 | 25 | 15625 | 625 | 0 | 0 |
| 🤡 Palhaço | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 🤡 Palhaço | 🃏 Coringa | 3 | 34 | 0 | 0 | 0 | 0 |
| 🤡 Palhaço | 🎪 Circo | 4 | 25 | 0 | 0 | 0 | 0 |
| 🧌 Troll | ⚔️ Atacar | 0 | 100 | 9845 | 98 | 0 | 0 |
| 🧌 Troll | 🤜 Pancada | 3 | 34 | 5865 | 172 | 0 | -3 |
| 🧌 Troll | 🥊 Porradeiro | 4 | 25 | 22146 | 885 | 0 | 0 |
| 🧞 Gênio | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 🧞 Gênio | 🪔 Desejo | 3 | 34 | 0 | 0 | 0 | 0 |
| 🧞 Gênio | 🔮 Profecia | 3 | 34 | 7752 | 228 | 0 | +3672 |
| 🧜 Sereia | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 🧜 Sereia | 🧜‍♀️ Canto de Sereia | 4 | 25 | 0 | 0 | 0 | 0 |
| 🧜 Sereia | 🌊 Atlantis | 4 | 25 | 0 | 0 | 0 | 0 |
| 🧚 Fada | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 🧚 Fada | 🔔 Sininho | 3 | 34 | 8160 | 240 | 0 | 0 |
| 🧚 Fada | ✨ Pó Mágico | 4 | 25 | 3000 | 120 | 0 | 0 |
| 🐲 Dragão | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 🐲 Dragão | 🔥 Sopro do Dragão | 3 | 34 | 15440 | 454 | 10000 | +10000 |
| 🐲 Dragão | 🐲 Dragão Protetor | 3 | 34 | 5360 | 157 | 0 | 0 |
| 💩 Cocô | ⚔️ Atacar | 0 | 100 | 17900 | 179 | 9900 | +9900 |
| 💩 Cocô | 🚽 Descarga | 3 | 34 | 16800 | 494 | 10000 | +10000 |
| 💩 Cocô | 🪠 Desentupidor | 3 | 34 | 14080 | 414 | 10000 | +10000 |
| 🦸 Herói | ⚔️ Atacar | 0 | 100 | 16000 | 160 | 0 | 0 |
| 🦸 Herói | 🦸 Salvando o Dia | 3 | 34 | 8000 | 235 | 0 | 0 |
| 🦸 Herói | 💪 Super | 3 | 34 | 16140 | 474 | 0 | 0 |
| 🦹 Vilão | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 🦹 Vilão | 🦹 Destruindo o Dia | 3 | 34 | 5440 | 160 | 0 | 0 |
| 🦹 Vilão | 👿 Vilania | 4 | 25 | 12000 | 480 | 0 | 0 |
| 🦖 T-Rex | ⚔️ Atacar | 0 | 100 | 10880 | 108 | 0 | +2880 |
| 🦖 T-Rex | 🦖 Rugido | 3 | 34 | 0 | 0 | 0 | 0 |
| 🦖 T-Rex | 🦶 Pisada | 3 | 34 | 16570 | 487 | 0 | +5090 |
| 🦇 Morcego | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 🦇 Morcego | 🦇 Mordida | 3 | 34 | 5440 | 160 | 0 | 0 |
| 🦇 Morcego | 🐀 Rato Voador | 4 | 25 | 0 | 0 | 0 | 0 |
| 🧛 Vampiro | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 🧛 Vampiro | 🩸 Controle de Sangue | 3 | 34 | 20400 | 600 | 0 | 0 |
| 🧛 Vampiro | 🌙 Vampiro Primordial | 4 | 25 | 2000 | 80 | 0 | 0 |
| 🧝 Elfo | ⚔️ Atacar | 0 | 100 | 25400 | 254 | 17400 | +17400 |
| 🧝 Elfo | 🌳 Árvore do Mundo | 3 | 34 | 22760 | 669 | 17400 | +17400 |
| 🧝 Elfo | 🌿 Natureza | 3 | 34 | 9930 | 292 | 5850 | +5850 |
| 😈 Diabo | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 😈 Diabo | 🔥 Inferno | 3 | 34 | 6800 | 200 | 0 | +6800 |
| 😈 Diabo | 😇 Anjo Caído | 3 | 34 | 0 | 0 | 0 | 0 |
| ⛄ Boneco de Neve | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| ⛄ Boneco de Neve | ⛄ Bola de Neve | 3 | 34 | 7480 | 220 | 0 | 0 |
| ⛄ Boneco de Neve | ❄️ Gelado | 4 | 25 | 3500 | 140 | 0 | 0 |
| 🎭 Mímico | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 🎭 Mímico | 🎭 Imitação | 3 | 34 | 3400 | 100 | 0 | 0 |
| 🎭 Mímico | 📋 Copiando | 4 | 25 | 0 | 0 | 0 | 0 |
| 😇 Anjo | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 😇 Anjo | 🌟 Celestial | 3 | 34 | 0 | 0 | 0 | 0 |
| 😇 Anjo | ☁️ Céu | 4 | 25 | 0 | 0 | 0 | 0 |
| 🎅 Papai Noel | ⚔️ Atacar | 0 | 100 | 8000 | 80 | 0 | 0 |
| 🎅 Papai Noel | 🎅 Saco de Presente | 3 | 34 | 5916 | 174 | 0 | 0 |
| 🎅 Papai Noel | 🏭 Fábrica de Presente | 3 | 34 | 9044 | 266 | 0 | +4284 |

