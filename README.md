# ⚔️ Apostle's War

> RPG por turnos em C# / .NET 10, jogável de ponta a ponta numa janela desktop (WebView2).
> Projeto de estudo e portfólio: um laboratório de design de software com um problema grande o
> bastante para que decisão ruim doa.

> **[GIF DE 5–10s DO COMBATE AQUI]** — grave com ScreenToGif, salve em `docs/media/combate.gif`
> e referencie: `![Combate](docs/media/combate.gif)`

Monte um time de 4 campeões e atravesse 8 capítulos de campanha, um por facção. 36 personagens,
combate com crítico, escudos, status com tick, contra-ataque, revive e prevenção de morte.
Detalhes do jogo mais abaixo — as três seções seguintes são sobre como ele é construído.

---

## 🏗️ Arquitetura

Clean Architecture com **um projeto por camada**. A dependência aponta sempre pra dentro, e
**quebra de camada nem compila** — a superfície pública é o contrato, não há `InternalsVisibleTo`.
A fronteira não é convenção de pasta: é verificada pelo compilador.

```
ApostlesWar.Domain/          regras do jogo, ZERO referências
  Combat/                      Combate, Batalha/Equipe, TurnoDoPersonagem, RelógioDoCombate, capacidades
  Skills/                      ações, buffs, debuffs, passivas
  Champs/<Faccao>/<Champ>/     cada campeão como dado: stats + habilidades
  Models/ · Enum/

ApostlesWar.Application/     casos de uso
  Services/                    orquestração (campanha, arsenal, perfil, combate, configuração)
  Controllers/                 o bot — decide o turno do inimigo e do modo Auto
  Portas/                      IApresentacao, ITelaDeCombate, IControladorDeTurno, IRepositorioDeSave

ApostlesWar.Infrastructure/  implementação das portas de dados (save local em JSON)

ApostlesWar.Presentation/    a única pele: WinForms + WebView2 (net10.0-windows)
  Front/                       composition root, ponte C# ⇄ JS, estado da batalha
  wwwroot/                     index.html · estilo.css · jogo.js (telas, animação e cenários)

ApostlesWar.Tests/           xUnit — motor, capacidades, services, bot, e a Bancada de dano
```

O motor não sabe desenhar nada: ele fala com a tela por portas. Foi assim que o front nasceu sem
tocar nas regras — e é assim que outra pele nasceria.

**Documentação** em `docs/`: `ROADMAP-refatoracao.md` (fila de execução e manual de cenário),
`CATALOGO-de-acoes.md`, `GDD-expansao.md` e os `ADR-*.md` com as decisões de arquitetura —
incluindo as descartadas, como o modelo gacha/live-service abandonado.

---

## 📊 Bancada de dano — teste como instrumento de design

Balancear 36 personagens à mão é chute. Então a suíte de testes virou instrumento de medição.

A cada `dotnet test`, a Bancada roda **o motor de verdade** — não um mock — mede as 36 fichas e
reescreve `docs/bancada-dano.md`. O relatório é **versionado de propósito**: cada ajuste de número
vira um `git diff` legível, e dá pra ver exatamente o que um buff em um personagem fez com a curva
dos outros.

O desenho da bancada passou por revisão antes de existir: a primeira versão media dano contra um
alvo com DEF 0, o que tornaria invisível qualquer passiva de penetração de defesa. Está registrado
em ADR.

---

## 🤖 Front-end e uso de IA

A camada de apresentação (WebView2 + HTML/CSS/JS em `wwwroot/`) foi **implementada com assistência
de IA, sob especificação e revisão minhas**. Parte dos commits é co-autorada, e está no histórico.

Foi decisão de escopo, não atalho. Meu foco de estudo é back-end e arquitetura, e eu precisava de
uma interface jogável para validar o comportamento do motor — ver o turno acontecer é diferente de
ler o log dele. HTML, CSS e JavaScript são justamente o que ainda estou aprendendo, então o código
do front fica como material de estudo para quando eu chegar nessa etapa.

O que é integralmente meu: a arquitetura, a especificação de comportamento, a crítica de cada
iteração e a decisão de descartar o que não funcionou. O contrato entre motor e pele são as portas
em `Application/Portas/` — foi ele que permitiu delegar a implementação da interface sem que uma
linha de regra de jogo fosse tocada.

---

## ✨ O jogo

Vencer uma fase dropa um item; vencer um capítulo desbloqueia a facção seguinte e os campeões dela.
Cada capítulo tem **cenário próprio, animado em canvas**: o dia claro da cidade murada do Reino, o
cemitério enluarado do Lado Sombrio, a invasão sob as estrelas dos Tecnológicos, a clareira do
Folclore, a praia no crepúsculo dos Místicos, o banheiro dos Especiais, a Árvore do Mundo escorrendo
lava dos Decaídos e a sala de Natal dos Apóstolos.

- **🧙 36 personagens** em 9 facções, cada um com kit próprio (ativas + passiva)
- **🗺️ Campanha** — 8 capítulos × 7 fases, com desbloqueio progressivo
- **⚔️ Arena** — batalha livre montando os dois times, pra testar composições
- **🤖 Modo Auto** — o mesmo cérebro que joga pelo inimigo joga por você, com foco de alvo clicável
- **📦 Arsenal** — 7 slots, itens escalados por capítulo, com totais dos bônus equipados
- **📖 Compêndio** — os 36 campeões por facção, os travados com cadeado
- **💥 Combate completo** — crítico, escudos, buffs/debuffs, status com tick, proteção de aliado,
  contra-ataque, revive e prevenção de morte
- **💾 Perfil e save** — progresso, time e itens equipados persistidos em arquivo

### Facções e personagens

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
| ✝️ Apóstolos | ☃️ Boneco de Neve · 🎭 Mímico · 👼 Anjo · 🎅 Papai Noel |

Os **Humanos** são o time inicial — a campanha começa no Reino e termina nos Apóstolos.

### Sistema de itens

Cada fase dropa o item do slot dela. A **fase** define o nome e o stat; a **facção** define o visual
e a magnitude, então o mesmo Elmo vale mais nos capítulos finais.

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

## 💡 Conceitos C# aplicados

`Classes e objetos` · `Encapsulamento` · `Herança` · `Polimorfismo` · `Classes abstratas` ·
`Interfaces` · `Generics` · `List<T>` · `Dictionary<T>` · `Enum` · `Record` · `File I/O` ·
`JSON serialização` · `LINQ` · `Pattern matching` · `Injeção de dependência` · `Testes com xUnit`

---

## 🚀 Como executar

```bash
git clone https://github.com/GabrielHenriqueCe/Apostle-s-War
cd Apostle-s-War
dotnet run --project ApostlesWar.Presentation
```

Requer **.NET 10 SDK**, **Windows** e o **runtime do WebView2** (já vem no Windows 11).

```bash
dotnet test
```

---

## 🔭 Próximos passos

- Cenário da facção **Humanos** — o último dos 9
- **Rebalanceamento** das habilidades, lendo os números da bancada
- **Níveis de dificuldade** (Fácil · Normal · Difícil · Pesadelo)
- Distribuição como `.exe` self-contained no GitHub Releases

---

## 👨‍💻 Desenvolvedor

**Gabriel Henrique Cé** — Engenharia de Software (Uniasselvi) · Blumenau, SC

[LinkedIn](https://www.linkedin.com/in/gabriel-henrique-ce) · [Portfólio](https://gabrielhenriquece.github.io/) · ga.biel.hce@gmail.com
