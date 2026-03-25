# ⚔️ Apostle's War — C# Console RPG

> Jogo de RPG por turnos para console, desenvolvido em C# como projeto de estudo paralelo ao programa Entra21.

---

## 🎮 Sobre o Jogo

Monte um time de 4 campeões e enfrente facções inimigas em batalhas por turnos. Cada capítulo desbloqueado traz novos personagens, itens e desafios mais difíceis.

```
=====Apostle's War=====

👾 ataca! | HP:840 ATK:240 DEF:240
Alvos disponíveis:
1 - 👷 | HP:1200 ATK:240 DEF:120
2 - 👮 | HP:1000 ATK:120 DEF:280
3 - 🧙 | HP:1000 ATK:280 DEF:120
4 - 🫅 | HP:1000 ATK:200 DEF:200
```

---

## ✨ Funcionalidades

- **🧙 36 personagens** distribuídos em 9 facções
- **⚔️ Modo Campanha** — 7 fases por capítulo, com progressão e save
- **📦 Sistema de Itens** — equipamentos dropados ao concluir fases, com stats escalados por capítulo
- **💥 Combate com crítico** — taxa e dano de crítico por personagem
- **💾 Save automático** — progresso e itens equipados persistidos em arquivo

---

## 🧩 Facções e Personagens

| Facção | Personagens |
|--------|-------------|
| 🛠️ Humanos | 👷 Operário · 🕵️ Detetive · 👮 Policial · 👲 Sushiman |
| 👑 Reino | 💂 Guarda · 🥷 Ninja · 🧙 Mago · 🫅 Rei |
| 🌑 Lado Sombrio | 💀 Caveira · 👻 Fantasma · 🎃 Abóbora · 🧟 Zumbi |
| ⚙️ Tecnológicos | 👾 Invasor · 👽 Alien · 🤖 Robô · 🧑‍🔬 Cientista |
| 🪬 Folclore | 👹 Ogro · 👺 Tengu · 🤡 Palhaço · 🧌 Troll |
| 🐉 Místicos | 🧞 Gênio · 🧜 Sereia · 🧚 Fada · 🐲 Dragão |
| ⭐ Especial | 💩 Cocô · 🦸 Herói · 🦹 Vilão · 🦖 T-Rex |
| 🔱 Decaídos | 🦇 Morcego · 🧛 Vampiro · 🧝 Elfo · 😈 Diabo |
| 🌬️ Apóstolos | ☃️ Boneco de Neve · 🎭 Mímico · 👼 Anjo · 🎅 Papai Noel |

---

## 📦 Sistema de Itens

Ao concluir cada fase, um item é dropado com stats escalados pelo capítulo:

| Slot | Item | Stat |
|------|------|------|
| 1 | Arma | ATK flat |
| 2 | Elmo | HP flat |
| 3 | Escudo | DEF flat |
| 4 | Manopla | Taxa de Crítico % |
| 5 | Peitoral | HP % |
| 6 | Calça | DEF % |
| 7 | Bota | Dano Crítico % |

---

## 🏗️ Arquitetura

### Principais classes

```
Personagem       → template de stats (HP, ATK, DEF, crit)
SelecaoSimbolo   → roster completo de personagens
Combate          → classe abstrata base do combate (HPAtual, Ataque, Defesa)
  Jogador            → combatente controlado pelo jogador
  Inimigo            → combatente inimigo com multiplicadores por fase
Campeoes         → gerencia desbloqueio e seleção do time
Arsenal          → gerencia itens obtidos e equipados
Campanha         → define composição de inimigos por fase
Capitulos        → progresso de desbloqueio por capítulo
GerenciadorDeJogo → loop principal, menus e execução da campanha
```

---

## 💡 Conceitos C# Aplicados

`Classes e objetos` · `Encapsulamento` · `Herança` · `Polimorfismo` · `Classes abstratas` · `Generics` · `List<T>` · `Dictionary<T>` · `Enum` · `Struct` · `File I/O` · `JSON serialização` · `LINQ` · `Pattern matching` · `Sobrecarga de métodos`

---

## 🔭 Próximos Passos (v2)

- Habilidades únicas por personagem
- Modo automático de combate — tecla 0 alterna entre manual e automático
- Combate visual em grid 4x6 com navegação por cursor
- Seleção de personagens com navegação A/D
- Layouts de time salvos (3 slots)

---

## 🚀 Como Executar

```bash
git clone https://github.com/GabrielHenriqueCe/Apostle-s-War
cd Apostle-s-War
dotnet run
```

Requer **.NET 8+** e terminal com suporte a **UTF-8** para exibir os emojis corretamente.

---

## 👨‍💻 Desenvolvedor

**Gabriel Henriques Cé** — Engenharia de Software (3º semestre)

[![LinkedIn](https://img.shields.io/badge/LinkedIn-Gabriel_Henriques_Cé-0077B5?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/gabriel-henrique-c%C3%A9-2a97b31a0)
