# GDD — PROGRESSÃO E COMBATE (ago/2026)

O plano que o Gabriel desenhou depois que a separação do front ficou pronta. **Ele muda o significado
de quase todo número do jogo**, e é por isso que o #16 (rebalance) e o PR de dificuldade estão
parados: rebalancear agora é medir uma coisa que vai deixar de existir.

> **Por que o #16 espera.** A bancada mede champ com stat FIXO contra boneco parado. Aqui o stat vira
> função de `nível × estrela`, a habilidade muda por `raridade`, a ordem de turno passa a ser barra
> de velocidade e a posição limita quem alcança quem. Os 144 números do relatório atual medem um jogo
> que está sendo substituído.
>
> **E a bancada não responde a pergunta certa.** O Gabriel: *"um champ que ignora defesa e causa
> muito mais dano que um champ que causa menos dano e coloca um debuff — qual é mais quebrado?"* Não
> há número de dano que responda: o segundo só vale o que valer JUNTO com os outros três. A própria
> bancada já declara isso — o boneco nunca age, então contra-ataque, espinhos e revide medem ZERO.
> **Quem responde é uma bancada de TIME contra TIME.** Ver §A BANCADA 2.0.

---

## 1. OS STATS NOVOS

### Velocidade — barra de turno (modelo Raid Shadow Legends)

Cada combatente **enche uma barra pela Velocidade**. Ao passar de 100% ele age; se dois passarem no
mesmo instante, age quem tiver a barra **mais cheia**.

**Isso mata a vantagem de time que existe hoje.** Não há mais "o time A joga, depois o time B": há uma
**fila única** em que os dois lados se intercalam, e quem age primeiro é quem investiu em Velocidade.

**Consequência de graça:** encher, roubar e reduzir barra viram uma **família nova de habilidade** —
controle sem dano. É o que faltava pro 🧌 Troll e pro 👹 Ogro existirem sem serem "bate mais forte".

> **Não confundir com o `RelogioDoCombate`.** Ele conta RODADAS (enrage, limite de turnos). A barra
> decide QUEM joga. Coexistem.

### Aptidão × Resistência — o malefício cola?

Os nomes vieram do WoW pt-BR, e a **Aptidão** é a peça exata: lá ela existe **só para vencer a defesa
do alvo**. É a definição do que se quer aqui — meu malefício vence a Resistência dele.

### Precisão × Evasão — o golpe conecta?

Fica livre porque a Aptidão assumiu o malefício. **Recomendação: não implementar agora.** Raid, Epic
Seven e Summoners War tiraram a esquiva de propósito — dois "errou" diferentes na mesma luta dobram a
frustração sem dobrar a profundidade. Tirar depois é muito mais caro que acrescentar depois.

### RETORNOS DECRESCENTES — o achado do WoW

Para resistir a controle o WoW **não usa estatística**: o mesmo tipo de efeito no mesmo alvo dura
**metade** na 2ª vez, **um quarto** na 3ª, e depois o alvo fica **imune** por um tempo.

**As duas coisas se completam e não competem:**
- **Aptidão × Resistência** — o rolo do acerto: cola AGORA?
- **Retornos decrescentes** — o teto da repetição: colou de novo, mas dura menos.

Só Resistência vira loteria; só retornos faz todo mundo colar sempre. Juntos, premiam **variedade**
(quatro malefícios diferentes valem mais que o mesmo quatro vezes) — que é exatamente a estratégia de
time que a bancada atual não consegue medir.

> **Resistência por TIPO de efeito**, como no Darkest Dungeon (lá são Atordoamento, Sangramento,
> Praga, Debuff, Movimento, Golpe Fatal), é melhor que uma resistência única: o alvo pode ser durão
> contra atordoar e frágil contra veneno, e isso vira decisão de time. **Cuidado:** o encaixe é o
> `StatusEffect`, NÃO a `NaturezaDano` — esta descreve o golpe (ignora defesa, tipo de dano), não o
> efeito aplicado.

---

## 2. POSIÇÃO E TIPO — e por que são coisas DIFERENTES

### A posição mora na HABILIDADE, não no personagem (modelo Darkest Dungeon)

Quatro posições por lado (1–4, frente pro fundo). E a regra do DD é a chave: **cada habilidade
declara duas coisas separadas**.

```
posicoesDeUso: [1, 2]     // de onde dá pra usar
posicoesAlvo:  [1, 2]     // quem ela alcança
```

Por isso o mesmo herói tem uma habilidade que só funciona na frente e outra só no fundo. E por isso
**empurrar e puxar viram ataque**: tirar o atirador da posição 4 desliga metade do kit dele sem
causar um ponto de dano.

**Custo aqui: dois campos.** A habilidade já é DADO (`numeroDeAlvos`, `tipoAlvo`, `tipoLista`) — isto
são mais dois. E o `ControladorBot`, que já filtra alvo válido, ganha o filtro novo no mesmo lugar.

**O jogador ordena os 4 na montagem, e a ordem é COMPROMISSO** (Gabriel: *"se colocou em casa errada
se fudeu"*). Nada de correção automática. A tela de montar time (hoje `ui/time.js`) passa a ordenar,
não só escolher.

### O TIPO é identidade, não geometria

Separar as duas coisas é o que barateia as duas. *"Tanque não alcança o último"* é regra da
**habilidade**. *"Tanque é tanque"* é **identidade** — e é isso que o jogador lê pra montar time.

| tipo | posição natural | papel |
|---|---|---|
| **Guardião** | 1–2 | aguenta e protege; alcance curto |
| **Combatente** | 1–2 | dano corpo a corpo |
| **Atirador** | 3–4 | alcança o fundo inimigo |
| **Suporte** | 3–4 | cura, buff e malefício |

> **A MAESTRIA, ideia guardada.** No WoW ela faz **coisa diferente por especialização** — mesmo
> número, efeito distinto. Se um dia quiser um eixo a mais sem inchar a ficha: um stat só, quatro
> leituras (Guardião converte em redução, Combatente em dano, Suporte em potência de cura). Não é
> pra agora.

---

## 3. NÍVEL, ESTRELA E RARIDADE

**A separação é do Gabriel e é coerente:** número cresce por nível/estrela; **comportamento** muda por
raridade.

- **Estrelas 1★ a 6★**, padrão gacha: 1★ até nível 10, 2★ até 20, e assim por diante.
- **Subir estrela NÃO zera o nível** — decisão consciente, porque hoje há poucos campeões e zerar
  puniria justamente quem investiu.
- **Nível e estrela sobem os status base.**
- **Raridade**: comum (cinza) · incomum (verde) · raro (azul) · épico (roxo) · lendário (dourado) ·
  mítico (vermelho). **Sobe** (não é fixa no drop).

### Raridade → habilidade, e o refactor já pagou por isso

A habilidade **já é DADO** — uma lista de `Acao` com `Escopo`, `chance`, `valor`. Então raridade é um
**modificador da lista**, não um `if` espalhado nem uma classe por raridade:

- `Escopo.AlvosResolvidos` → `Escopo.TodosInimigos` = alvo único vira ÁREA
- `AplicarDebuff(chance: 0.50)` → `chance: 0.75` = malefício fraco fica forte
- acrescentar uma `Acao` na lista

> **O `ADR-composicao-de-acoes` foi escrito pra este dia** — ele só não sabia. Zero `Ativar` override,
> zero classe nova.

**A forma preferida (Gabriel): uma PASSIVA de raridade que afeta as outras habilidades**, no molde do
Piromancer do 🔮 Mago (que faz as outras habilidades renderem mais em alvo queimado). Ela escala com a
raridade, em vez de reescrever cada habilidade.

> **Por que isso é o caminho barato:** 36 champs × 6 raridades, se cada degrau mexesse em TODAS as
> habilidades, seriam centenas de variantes pra escrever **e pra balancear**. Uma passiva por champ que
> ganha degraus é uma peça, não vinte. Mudar as habilidades diretamente segue como opção pra casos
> especiais.

---

## 4. ITENS

- **Equipados no CAMPEÃO**, não mais no jogador. É o que os torna valiosos — e é a mudança de maior
  impacto no save.
- **Estrelas e raridade seguem o mesmo padrão dos campeões.**
- **Raridade decide quantas SUBESTATÍSTICAS o item já nasce tendo:** comum 0 · incomum 1 · raro 2 ·
  épico 3 · lendário 4 · mítico 4 **com uma já aprimorada**.
- **Nível até 20. A cada 5 níveis aprimora**: enquanto houver menos de 4 subestatísticas, acrescenta
  uma; com 4, incrementa uma das existentes, mostrando `(1)` / `(2)` ao lado e o valor que ficou.
  **Assim um comum nunca alcança o aprimoramento de um mítico.**
- **Estrelas afetam o quanto sobe** — no atributo principal e nas subestatísticas.
- **CONJUNTOS**: bônus por peças equipadas, e o que cada conjunto faz cresce com a raridade.

> **O problema do número ímpar (7 peças) tem saída barata:** bônus em **2/4/6** e o **acessório fora
> do conjunto** — um slot livre. Resolve hoje, sem mexer no boneco e sem inventar 3 acessórios (que
> segue como ideia futura, levando o conjunto a 9).

### O drop, e o teto por dificuldade

- **Item sempre cai**, mas a estrela varia: 1–3★ Fácil · 2–4★ Normal · 3–5★ Difícil · 4–6★ Pesadelo.
- **Mais de um item por fase**, e cada fase/dificuldade dropa coisas diferentes.
- **Na campanha só caem COMUNS.**

### A fusão sem virar fazenda de 500 itens

**O problema não é a fusão, é o TETO** — e o Gabriel já resolveu isso pras estrelas sem perceber.
**Aplicar a mesma regra na raridade:**

- **A fonte decide o teto**: Fácil até Incomum, Normal até Raro, Difícil até Épico, Pesadelo até
  Lendário/Mítico.
- **A fusão sobe DENTRO do teto**, nunca acima.

Quem farmar a fase 1 mil vezes chega no teto dela e **para**. Sem trava artificial, sem anti-cheat, e
a dificuldade passa a valer alguma coisa.

**Alternativa elegante — FUSÃO COM SEMENTE:** pra fazer um Épico, N itens **mais um Épico que você já
tenha**. Garante que toda raridade entrou pelo drop ao menos uma vez.

### Sobre o offline e o save editado

**Não existe defesa real em jogo offline single-player.** Ofuscar, assinar, checksum: uma tarde de
trabalho pra quem quer burlar, e atrito permanente pra quem não quer.

O que resolve não é impedir, é **tornar inútil**: não há ranking, não há PvP, nada que o save de um
afete o de outro. Quem editar estraga o próprio jogo.

**E isso libera o desenho.** A economia pode ser feita pelo que é DIVERTIDO, não pelo que é à prova de
fraude — o que permite fugir do "junte 100 cópias", que é design de retenção de live service.
**Por isso a ideia das MISSÕES POR CHAMP é melhor que duplicata:** missão é conteúdo, duplicata é
imposto de tempo.

---

## 5. CAMPANHA

- **Ao criar a conta o jogador escolhe UM dos 4 humanos**, em vez de receber os quatro.
- **A ordem dos inimigos enfrentados nas fases muda.**
- **A partir do capítulo 2, toda fase é contra 4 inimigos** — acaba o crescente.
- **Campeão não vem mais garantido por fase.** Ele tem CHANCE de cair nas fases em que aparece, e
  **a tela precisa mostrar o que pode ser obtido em cada fase** (campeões e itens).

### Os inimigos não têm itens — e isso resolve a calibragem

Ideia do Gabriel: o inimigo pode ser **nível 250 com muito mais vida e status**, e o jogador vence por
**velocidade e sinergia de time**, não por status bruto.

> **Cuidado:** mostrar o nível ao jogador é bom (comunica ameaça), mas a conta deve continuar sendo
> **multiplicador de status**, não um nível de verdade. Duas fórmulas de poder pra manter em acordo
> divergem — é a mesma armadilha de "duas cópias de um número".

**A dificuldade só pode ser calibrada DEPOIS disto.** A fórmula atual (`1,75×dif + 0,5×cap + 0,1×fase`)
multiplica o inimigo contra um jogador que **não tinha progressão**. Com nível e estrela, o jogador
cresce também — a curva tem que ser desenhada contra as duas.

---

## 6. A BANCADA 2.0 — time contra time

A bancada atual mede **champ contra boneco parado**. Com posição, velocidade e tipos, ela fica ainda
mais cega: não vê alcance, não vê ordem de turno, não vê sinergia.

**A que responde a pergunta do Gabriel roda TIME contra TIME**, bot contra bot, e conta vitórias.
"Quebrado" deixa de ser uma opinião sobre dano e vira o que aparece nos times que ganham. O modo
Arena já existe justamente pra isso, e o `ControladorBot` já joga os dois lados.

---

## 7. O PLANO — agora × depois × outra plataforma

### AGORA (muda o modelo e o save; tem que vir junto)

**A ordem importa e não é negociável:** status e turno ANTES de nível e estrela. Subir status antes de
mudar quem joga quando é calibrar contra uma ordem de turno que ainda vai mudar.

1. **Velocidade + barra de turno + fila única.** Mexe em `Batalha`, `Equipe`, `TurnoDoPersonagem`.
2. **Aptidão × Resistência + retornos decrescentes.**
3. **Posição na habilidade** (`posicoesDeUso`/`posicoesAlvo`) + ordenar o time na montagem.
4. **Tipos** (Guardião/Combatente/Atirador/Suporte).
5. **Nível e Estrela** nos campeões.
6. **Raridade → passiva que escala.**
7. **Item equipado no campeão.**

> **O save atual é DESCARTADO** (decisão do Gabriel: *"descarta, não me importo"*). Sem migração.

### DEPOIS, ainda nesta plataforma

Subestatísticas e aprimoramento a cada 5 níveis · conjuntos 2/4/6 · drop por fase e dificuldade ·
a tela do "o que cai onde" · escolher 1 champ inicial · a ordem nova dos inimigos · fusão com teto ·
**a dificuldade, agora calibrada contra a progressão** · **a bancada 2.0** · e só então o **#16**.

### STEAM / MOBILE / outra plataforma

Missões por campeão para subir raridade · economia de duplicatas · cloud save · os 3 acessórios (9
peças de conjunto) · conquistas e telemetria · **Precisão × Evasão**, se o combate pedir.

---

## Decisões já fechadas (não reabrir)

- Subir estrela **não** zera o nível — há poucos campeões hoje.
- A posição é **compromisso do jogador**; nada de correção automática.
- O save atual é **descartado**.
- **Não** implementar esquiva agora.
- A raridade **sobe**; não é fixa no drop.
- Não tentar impedir save editado — tornar inútil, não impossível.
