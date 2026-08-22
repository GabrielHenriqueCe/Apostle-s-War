# ⚔️ Apostle's War

[![CI](https://github.com/GabrielHenriqueCe/Apostle-s-War/actions/workflows/ci.yml/badge.svg)](https://github.com/GabrielHenriqueCe/Apostle-s-War/actions/workflows/ci.yml)

> RPG por turnos em C# / .NET 10, jogável de ponta a ponta numa janela desktop (WebView2).
> Projeto de estudo e portfólio: um laboratório de design de software com um problema grande o
> bastante para que decisão ruim doa.

> **[GIF DE 5–10s DO COMBATE AQUI]** — grave com ScreenToGif, salve em `docs/media/combate.gif`
> e referencie: `![Combate](docs/media/combate.gif)`

Monte um time de 4 apóstolos e atravesse 8 capítulos de campanha, um por facção. 36 personagens,
combate com crítico, escudos, status com tick, contra-ataque, revive e prevenção de morte.
Detalhes do jogo mais abaixo — as próximas seções são sobre como ele é construído, e sobre como
ele foi construído.

---

## 🏗️ Arquitetura

Clean Architecture com **um projeto por camada**. A dependência aponta sempre pra dentro, e
**quebra de camada nem compila** — a superfície pública é o contrato, não há `InternalsVisibleTo`.
A fronteira não é convenção de pasta: é verificada pelo compilador.

```
ApostlesWar.Domain/          regras do jogo, ZERO referências
  Combat/                      Combate, Batalha/Equipe, TurnoDoPersonagem, RelógioDoCombate, capacidades
  Skills/                      ações, buffs, debuffs, passivas
  Apostolos/<Faccao>/<Apostolo>/     cada apóstolo como dado: stats + habilidades
  Models/ · Enum/

ApostlesWar.Application/     casos de uso
  Services/                    orquestração (campanha, arsenal, perfil, combate, configuração)
  Controllers/                 o bot — decide o turno do inimigo e do modo Auto
  Portas/                      IApresentacao, ITelaDeCombate, IControladorDeTurno, IRepositorioDeSave

ApostlesWar.Infrastructure/  implementação das portas de dados (save local em JSON)

ApostlesWar.Presentation/    a única pele: WinForms + WebView2 (net10.0-windows)
  Front/                       composition root, ponte C# ⇄ JS, estado da batalha
  wwwroot/                     index.html · estilo.css · jogo.js (só o arranque, 193 linhas)
    nucleo/                      ponte com o C#, laço de animação, ar do tema
    telas/                       uma por tela: menu, perfil, campanha, arena, catedral, forja, compêndio, combate
    ui/                          peças reusadas entre telas: ficha, modal, alma, raridade, navegador…
    cenarios/                    o fundo animado de cada facção, um módulo por capítulo

ApostlesWar.Tests/           xUnit — motor, capacidades, services, bot, e a Bancada de dano

ferramentas/                 os verificadores do front e dos docs, e os instrumentos de medição (Node puro)
.github/workflows/ci.yml     build, testes e os três verificadores, num Windows limpo
```

O motor não sabe desenhar nada: ele fala com a tela por portas. Foi assim que a interface nasceu
sem tocar nas regras — e é assim que outra pele nasceria.

**Documentação** em `docs/`: `ROADMAP-refatoracao.md` (a fila de execução), os `MANUAL-*.md`
(cenário, front, combate) como manuais de trabalho, `CATALOGO-de-acoes.md`, os `GDD-*.md` (combate,
progressão, itens, expansão) e os `ADR-*.md` com as decisões de arquitetura — incluindo as
descartadas, como o modelo gacha/live-service abandonado.

---

## 📊 Bancada de dano — teste como instrumento de design

Balancear 36 personagens à mão é chute. Então a suíte de testes virou instrumento de medição.

A Bancada roda **o motor de verdade** — não um mock — mede as 36 fichas e reescreve
`docs/bancada-dano.md`. O relatório é **versionado de propósito**: cada ajuste de número vira um
`git diff` legível, e dá pra ver o que um buff em um personagem fez com a curva dos outros.

Ela é **opt-in** (`$env:BANCADA=1; dotnet test`), porque gerar relatório não é testar: a corrida
custa ~63 s e reescreve um arquivo versionado, enquanto a suíte de verdade leva 0,6 s e não escreve
nada.

O desenho da bancada foi corrigido antes de existir: a primeira proposta media dano contra um alvo
com DEF 0, o que tornaria invisível qualquer passiva de penetração de defesa. Está registrado em ADR.

---

## 🔧 Ferramentas — verificar o que o `dotnet test` não alcança

Boa parte deste projeto não é C#: é front em JS puro e documentação. O xUnit não alcança nenhum dos
dois, e os dois quebram **calados** — tela que monta em branco sem erro no console, documento que
descreve uma pasta que mudou de nome três PRs atrás. Então cada um ganhou um verificador em
`ferramentas/`, Node puro, sem uma única dependência instalada.

| ferramenta | o que ela responde |
|---|---|
| `rodar-telas.js` | Constrói a árvore do `index.html` num navegador de mentira, carrega o front inteiro por cima, publica em cada tela a mesma mensagem que o C# publicaria e **dispara os gestos** — clique, hover, arrasto em sequência — até parar de aparecer ouvinte novo. Confere ainda os `getElementById` do JS contra o `index.html`, as terminações de linha, e **as cargas de tela contra os DTOs do C#**: propriedade que o front lê e o C# não manda aparece na corrida. |
| `rodar-tema.js` | Roda os oito cenários animados por 120 s simulados cada — 7.200 quadros — pelo caminho de verdade, o `aplicarTema` do jogo e não um mock, e queixa de `NaN`, `Infinity` e exceção. Existe porque uma colisão de chave entre dois cenários já deixou a cena em branco **sem erro visível**. |
| `conferir-docs.js` | Confere se os 22 documentos ainda descrevem o repositório que existe: caminho que sumiu, `#NNN` citado que nunca foi mergeado, orçamento de tokens que ficou pra trás. Símbolo sem ocorrência no código sai numa lista à parte, **sem derrubar** — documento que conta o que morreu está certo. |

Os três rodam antes de cada commit e de novo no CI. O que eles **não** cobrem está escrito no
cabeçalho de cada um, em vez de virar confiança em cima de um verde: o `rodar-telas` prova que as
telas montam e que os gestos não explodem, nunca que o jogo funciona, e o `conferir-docs` pega a
deriva mecânica — a semântica, o texto que descreve um comportamento que o código não tem mais,
continua precisando de leitor.

Junto moram os instrumentos de **uma decisão só**, versionados porque a conta que eles fizeram é a
justificativa de um número que está no jogo: `calibrar-inimigo.js` acha por bisseção em que nível um
time sem equipamento nenhum empata com o time equipado do jogador (as oito âncoras de dificuldade da
campanha), `medir-donos.js` monta o grafo de chamadas que decidiu de qual facção era cada função de
cenário antes da separação, e `separar-css.js` provou que quebrar o `estilo.css` em um arquivo por
tema não mudou a cascata.

### Integração contínua

Um job em `windows-latest` — obrigatório, e não preferência: a `Presentation` é `net10.0-windows` —
roda `dotnet build`, `dotnet test` e os três verificadores a cada push na `main` e a cada pull
request. O ganho não é rodar os testes, que eu já rodo aqui: é rodá-los num Windows sem o meu
`bin/obj`, o meu cache e o meu CRLF. Metade das armadilhas deste repositório é do tipo que passa na
minha máquina e quebra num clone limpo.

---

## 🤖 Como este projeto foi construído

Este repositório foi desenvolvido em colaboração com IA, e é honesto sobre isso — parte dos commits
é co-autorada e está no histórico.

**O que é meu:** a arquitetura e o desenho das camadas, a especificação de comportamento de cada
feature (escrita como sequência causal, não como "melhora isso"), a revisão crítica de cada
iteração, o ajuste do código entregue, e a decisão de descartar o que não funcionou — inclusive
coisas já aprovadas por mim. As decisões estão registradas nos ADRs e o processo está no histórico
de PRs: um PR por tema, mergeado só depois da minha revisão.

**O que foi assistido por IA:** a maior parte da implementação, incluindo a camada de apresentação
(HTML/CSS/JS em `wwwroot/`) e a suíte de testes, ambas escritas a partir das minhas especificações
e ajustadas por mim.

Foi decisão de escopo. Meu foco de estudo atual é back-end e arquitetura, e o front usa justamente
as tecnologias que ainda estou aprendendo — o código dele fica como material de estudo para quando
eu chegar nessa etapa. O que permitiu delegar sem risco foram as portas em `Application/Portas/`:
o contrato entre motor e pele é explícito, então a implementação da interface nunca tocou uma linha
de regra de jogo.

---

## ✨ O jogo

Vencer uma fase larga quatro peças de equipamento e desbloqueia os apóstolos dela; vencer um
capítulo abre a facção seguinte. Cada capítulo tem **cenário próprio, animado em canvas**: o dia
claro da cidade murada do Reino, o cemitério enluarado do Lado Sombrio, a invasão sob as estrelas
dos Tecnológicos, a clareira do Folclore, a praia no crepúsculo dos Místicos, o banheiro dos
Especiais, a Árvore do Mundo escorrendo lava dos Decaídos e a sala de Natal dos Ascendentes.

- **🧙 36 personagens** em 9 facções, cada um com kit próprio (ativas + passiva)
- **🗺️ Campanha** — 8 capítulos × 7 fases, com desbloqueio progressivo, em **quatro dificuldades**
  (Fácil · Normal · Difícil · Pesadelo): cada uma guarda o próprio progresso, abre quando a anterior
  é zerada, e multiplica a XP pelo próprio peso
- **⚔️ Arena** — batalha livre montando os dois times, pra testar composições
- **🤖 Modo Auto** — o mesmo cérebro que joga pelo inimigo joga por você, com foco de alvo clicável
- **⛪ Catedral** — onde o apóstolo se aprimora, em estações que são LUGARES e não verbos: 🎒 Armaria
  (vestir as 7 peças, com os totais dos bônus), ⬆️ Santuário (queimar alma pra subir de nível),
  ★ Altar (a estrela que levanta o teto de nível) e 🔥 Oferenda (fundir alma)
- **⚒️ Forja** — a oficina da **peça**: ⚒️ Bigorna malha pó em ponto de nível, 💧 Têmpera paga a
  estrela e abre a dezena seguinte, 🏺 Caldeamento funde 10 de uma faixa em 1 da faixa acima
- **📖 Compêndio** — os 36 apóstolos por facção, os travados com cadeado
- **💥 Combate completo** — crítico, escudos, buffs/debuffs, status com tick, proteção de aliado,
  contra-ataque, revive e prevenção de morte
- **💾 Perfil e save** — progresso, time, itens equipados e materiais persistidos em arquivo

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
| ❄️ Ascendentes | ☃️ Boneco de Neve · 🎭 Mímico · 👼 Anjo · 🎅 Papai Noel |

Os **Humanos** são o time inicial — a campanha começa no Reino e termina nos Ascendentes.

### Sistema de itens

Cada fase larga **quatro peças**, e a peça é uma **instância**: duas Manoplas do Reino são objetos
diferentes, com níveis e principais diferentes. A **fase** define o slot e o nome; a **facção**
define o visual e o conjunto.

A magnitude vem do **nível da peça**, não do capítulo. Toda peça cai valendo 11,5% do teto do slot e
conquista o resto sendo usada — só quem entra em campo sobe, e o ganho é por ciclo de combate, não
por turno do portador, senão o apóstolo rápido subiria equipamento em dobro. É isso que impede um
drop novo de apagar o que já foi investido na peça antiga.

| Slot | Item | Principal |
|------|------|-----------|
| 1 | Arma | ATK cheio |
| 2 | Elmo | HP cheio |
| 3 | Escudo | DEF cheio |
| 4 | Manopla | ATK% · HP% · DEF% · Taxa de Crítico % · Dano Crítico % |
| 5 | Peitoral | ATK% · HP% · DEF% · Resistência |
| 6 | Calça | ATK% · HP% · DEF% · Precisão |
| 7 | Bota | ATK% · HP% · DEF% · Velocidade |

Os três primeiros têm principal **fixo**; nos quatro de percentual ele é **sorteado no drop**, e é
isso que faz duas cópias do mesmo slot serem duas decisões. A **Velocidade tem fonte única no jogo
inteiro** — a Bota —, de propósito: a faixa entre os arquétipos é de 30 pontos, e uma segunda fonte
faria o número do tipo virar ruído.

---

## 💡 Conceitos C# aplicados no projeto

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

A suíte de testes, e a Bancada de dano que fica fora dela:

```bash
dotnet test                      # ~0,6 s, e não escreve nada
$env:BANCADA=1; dotnet test      # + a Bancada: ~63 s, reescreve o docs/bancada-dano.md
```

Os três verificadores (Node 24, sem `npm install` — não há dependência a instalar):

```bash
node --experimental-vm-modules ferramentas/rodar-telas.js
node --experimental-vm-modules ferramentas/rodar-tema.js
node ferramentas/conferir-docs.js
```

---

## 🔭 Próximos passos

- Cenário da facção **Humanos** — o último dos 9
- **Raridade e subestatísticas** do item: com elas abrem a 💎 Raridade da Catedral e a ♻️ Reforja da
  Forja, as duas bancadas que hoje estão desenhadas e travadas
- **Rebalanceamento** das habilidades, lendo os números da bancada
- Distribuição como `.exe` self-contained no GitHub Releases

---

## 👨‍💻 Desenvolvedor

**Gabriel Henrique Cé** — Engenharia de Software (Uniasselvi) · Blumenau, SC

[LinkedIn](https://www.linkedin.com/in/gabriel-henrique-ce) · [Portfólio](https://gabrielhenriquece.github.io/) · ga.biel.hce@gmail.com

---

## 📄 Direitos

© 2026 Gabriel Henrique Cé. Todos os direitos reservados.

Este repositório é público para fins de portfólio e avaliação técnica. O código-fonte, a
documentação, o design do jogo, os personagens e os cenários são de minha autoria e não estão
licenciados para uso, cópia, modificação, redistribuição ou uso comercial sem autorização prévia
por escrito.

Termos completos em [LICENSE](LICENSE). Para solicitar licenciamento: ga.biel.hce@gmail.com
