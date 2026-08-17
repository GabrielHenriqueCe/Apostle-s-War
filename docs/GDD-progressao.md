# GDD — PROGRESSÃO E COMBATE (ago/2026)

O plano que o Gabriel desenhou depois que a separação do front ficou pronta. **Ele muda o significado
de quase todo número do jogo**, e é por isso que o #16 (rebalance) e o PR de dificuldade estão
parados: rebalancear agora é medir uma coisa que vai deixar de existir.

> **Por que o #16 espera.** A bancada mede apóstolo com stat FIXO contra boneco parado. Aqui o stat base
> passa a vir do **tipo**, cresce por `nível`, a habilidade muda por `raridade`, a ordem de turno passa
> a ser barra de velocidade e a posição limita quem alcança quem. Os 144 números do relatório atual
> medem um jogo que está sendo substituído.
>
> **E a bancada não responde a pergunta certa.** O Gabriel: *"um apóstolo que ignora defesa e causa
> muito mais dano que um apóstolo que causa menos dano e coloca um debuff — qual é mais quebrado?"* Não
> há número de dano que responda: o segundo só vale o que valer JUNTO com os outros três. A própria
> bancada já declara isso — o boneco nunca age, então contra-ataque, espinhos e revide medem ZERO.
> **Quem responde é uma bancada de TIME contra TIME.** Ver §A BANCADA 2.0.

---
> **A NUMERAÇÃO É A DO GDD ORIGINAL**, e ela vale nos três arquivos — uma referência a "§4" quer
> dizer a mesma coisa em qualquer um deles:
>
> | § | assunto | arquivo |
> |---|---|---|
> | §1 §2 | stats novos, barra de turno, posição e tipo | `GDD-combate.md` |
> | §3 §5 §6 §7 | nível e raridade, campanha, a bancada 2.0, o plano, as decisões fechadas | `GDD-progressao.md` |
> | §4 | itens | `GDD-itens.md` |

---

## 1 e 2. OS STATS NOVOS · POSIÇÃO E TIPO → `docs/GDD-combate.md`

A barra de turno, a DEF em curva, Precisão × Resistência, a distância ideal por tipo e a tabela de
stats base dos 4 tipos. São 566 linhas de MODELO — o que fica valendo depois de implementado.

---

## 4. ITENS → `docs/GDD-itens.md`

696 linhas, 40% do documento original, e o passo mais distante da fila (o item equipado é o último
do §7). Slots, subs, escala, drop, evolução e dungeons.

---

## 3. O APÓSTOLO — NÍVEL E RARIDADE (não tem estrela)

**A separação é do Gabriel e é coerente:** número cresce por nível; **comportamento** muda por
raridade.

- **Raridade**: comum (cinza) · incomum (verde) · raro (azul) · épico (roxo) · lendário (dourado) ·
  mítico (vermelho). **Sobe** (não é fixa no drop).
- **Nível sobe por XP de batalha** — igual ao item, que também sobe de nível jogando.
- **Raridade sobe por MISSÃO do apóstolo** + teto da dificuldade.
- **Subir raridade NÃO zera o nível.**

### A CURVA DE XP — e ela foi calibrada pra não obrigar a farmar

```
custo do nível N → N+1   =   100 × N
XP de uma fase           =   22 × (capítulo × 7 + fase)   ×   multiplicador da dificuldade
                             Fácil 1 · Normal 2 · Difícil 3 · Pesadelo 4
```

**O critério foi um só: jogar cada fase UMA vez, em ordem, tem de bastar.** No Fácil isso fecha
cravado no teto da dificuldade:

| ao terminar o capítulo | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 |
|---|--:|--:|--:|--:|--:|--:|--:|--:|
| **nível** | 6 | 9 | 13 | 16 | 20 | 23 | 26 | **30** |

Nível **30** ao fim do capítulo 8, que é exatamente o teto do raro — o teto do Fácil. **Zero repetição
obrigatória.** E como a XP cresce dentro do capítulo e entre capítulos, a **8-7 vale 8× uma 1-1**:
quem quiser repetir tem onde, e o lugar é o mais difícil.

**O que cada dificuldade custa** (o total do jogo é `100 × (1+…+59)` = **177.000**):

| dificuldade | níveis | XP | % do jogo |
|---|--:|--:|--:|
| Fácil | 1 → 30 | 43.500 | 24,6% |
| Normal | 30 → 40 | 34.500 | 19,5% |
| Difícil | 40 → 50 | 44.500 | 25,1% |
| Pesadelo | 50 → 60 | 54.500 | 30,8% |

> **Cada dificuldade custa ~um quarto do jogo, e ninguém desenhou isso.** Cai da curva quadrática: o
> Fácil entrega **29 níveis** e o Pesadelo **10**, e mesmo assim o Pesadelo custa **mais**. As quatro
> etapas se equalizam sozinhas.

> **DO NORMAL EM DIANTE VOCÊ BATE O TETO DE NÍVEL NA METADE DA PASSADA** — por volta do capítulo 3 —
> e os cinco capítulos restantes não dão mais nível nenhum. **Isso é o desenho, não um furo:** é a
> medição do §2 aparecendo, onde o crescimento do jogador deixa de vir do nível e passa a vir do item
> (`1,11×` de nível contra `1,71×` de item no Pesadelo). O nível é o eixo do começo; o item é o do fim,
> e a XP entrega a passagem de bastão sem precisar de regra pra isso.

> **O apóstolo que entra tarde não alcança — chega perto.** Quem é descoberto no capítulo 5 do Fácil
> acumula ~30.400 e termina em **nv 25** contra os 30 do veterano. O doc promete que *"alcança rápido
> pela XP"*; sem bônus de recuperação isso é **falso por 5 níveis**. Decidir se entra um bônus ou se a
> promessa muda de texto.

### O APÓSTOLO NÃO TEM ESTRELA — e não se perde nada

**O eixo estava vazio.** A estrela do apóstolo não tinha fonte de custo que não fosse *sacrificar
cópias*, e duplicata está descartada (ver §4, offline). Eixo sem fonte própria não é eixo, é peça
esperando um sistema que não se quer construir.

**E ela já duplicava a raridade.** O que a estrela fazia era ser **teto de nível** (*1★ até 10, 2★ até
20…*) — exatamente o papel que a raridade tem no item. Passando a função pra frente, **nenhum número
muda, só o dono**:

| raridade do apóstolo | nível máx |
|---|---|
| comum | 10 |
| incomum | 20 |
| raro | 30 |
| épico | 40 |
| lendário | 50 |
| mítico | 60 |

Mesmo `6 × 10 = 60` que a estrela dava. E a simetria com o item fica exata: **raridade destrava nível,
nível dá os números.** De quebra, o teto de raridade por dificuldade passa a limitar o nível do
apóstolo **de graça** — no Fácil não se passa de raro, logo não se passa do nível 30.

> **A estrela continua existindo no jogo** — no ITEM, onde ela tem fonte própria (o nível de uso) e
> função própria (magnitude). Não se perde a sensação; para-se de pagar por ela duas vezes.

### As missões, e por que o nível avisa a hora

**Missão é conteúdo; duplicata é imposto de tempo.** Uma missão boa usa o que já existe e prova
domínio do apóstolo — *"passar a fase 7 do capítulo dele sozinho"*.

E o encaixe com a escada é automático: **o apóstolo trava no teto e a barra encosta na parede**, o que
avisa o jogador sozinho, sem tela de tutorial.

```
comum, bate no nível 10   →  parou  →  missão  →  incomum, destrava até 20
incomum, bate no nível 20 →  parou  →  missão  →  raro, destrava até 30
raro                      →  parou de vez: o Fácil não vai além
```

> Por isso **duas missões dentro do Fácil** (comum→incomum→raro), e não uma.

### Os HUMANOS — a exceção, e ela é de propósito

- **Começam INCOMUM** (teto de nível 20 contra os 10 de todo mundo). É a vantagem do time inicial, e
  ela se lê direto na escada, sem número inventado.
- **O jogador escolhe UM ao criar a conta.** Fechar **todos** os capítulos de uma dificuldade dá
  **+1 humano** para escolher, já na raridade máxima daquela dificuldade (Fácil → raro, Normal →
  épico, Difícil → lendário). No Pesadelo não vem mais nenhum: são 4 no total.
- **São os únicos que sobem raridade por progressão de campanha**, não por missão individual — e sobem
  mesmo sem serem usados.

> **Um humano *raro nível 1* é normal e não é problema:** ele tem o **teto aberto** e status nenhum.
> Quem adotar um protagonista novo lá pelo capítulo 5 alcança rápido pela XP. Os humanos são o **slot
> flexível** do elenco.
>
> **Nada de forçar o uso deles pra passar da 8-7.** Obrigar um apóstolo específico na fase final é
> gargalo: invalida o time que o jogador montou, justo onde ele mais quer usá-lo.

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

> **Por que isso é o caminho barato:** 36 apóstolos × 6 raridades, se cada degrau mexesse em TODAS as
> habilidades, seriam centenas de variantes pra escrever **e pra balancear**. Uma passiva por apóstolo que
> ganha degraus é uma peça, não vinte. Mudar as habilidades diretamente segue como opção pra casos
> especiais.

---

## 5. CAMPANHA

- **Ao criar a conta o jogador escolhe UM dos 4 humanos**, em vez de receber os quatro.
- **O apóstolo continua caindo GARANTIDO na fase dele.** Sem "chance" some junto a pergunta "e se cair
  de novo?".
- **A tela precisa mostrar o que pode ser obtido em cada fase** (apóstolos e itens).

### A COMPOSIÇÃO DAS FASES — o Fácil descobre, o resto reorganiza

**A descoberta um-por-um existe SÓ no Fácil.** Nas outras dificuldades você já conhece os quatro, então
são sempre quatro inimigos. Isso mata a regra antiga (*"a partir do capítulo 2, toda fase é contra 4
inimigos"*), que resolvia pelo capítulo o que na verdade é por **dificuldade**.

`G` Guardião · `C` Combatente (frente, posições 1–2) · `A` Atirador · `S` Suporte (fundo, 3–4).
Como **cada facção tem um de cada tipo**, a tabela se escreve por PAPEL e a posição vem do que o
apóstolo é.

**FÁCIL — a facção se junta, depois se reorganiza:**

| | | pos 1 | pos 2 | pos 3 | pos 4 | estreia |
|---|---|:-:|:-:|:-:|:-:|---|
| **FASE 1** | r1 | · | C | · | · | **C** |
| | r2 | C | · | · | · | |
| **FASE 2** | r1 | C | · | · | · | **G** |
| | r2 | G | C | · | · | |
| **FASE 3** | r1 | G | C | · | · | **A** |
| | r2 | G | C | A | · | |
| **FASE 4** | r1 | G | C | A | · | **S** |
| | r2 | G | C | A | S | |
| **FASE 5** | r1 | G | C | A | S | — |
| | r2 | C | C | A | S | |
| **FASE 6** | r1 | C | C | A | S | — |
| | r2 | G | C | S | S | |
| **FASE 7** | r1 | G | C | S | S | — |
| | r2 | G | C | A | S | |

**NORMAL · DIFÍCIL · PESADELO — sempre quatro:**

| | | pos 1 | pos 2 | pos 3 | pos 4 | a formação É |
|---|---|:-:|:-:|:-:|:-:|---|
| **FASE 1** | r1 · r2 | G | C | A | S | o padrão |
| **FASE 2** | r2 | G | C | A | A | dois atiradores |
| **FASE 3** | r2 | G | G | A | S | a muralha |
| **FASE 4** | r2 | C | C | A | S | dois combatentes |
| **FASE 5** | r2 | G | C | S | S | dois suportes |
| **FASE 6** | r2 | C | C | A | A | sem guardião — corrida |
| **FASE 7** | r2 | G | C | A | S | o time perfeito, no topo |

> **A rodada 1 de cada fase repete a rodada 2 da anterior.** Cada formação é vista duas vezes — como
> aquecimento no fim de uma fase e como tema da seguinte. Dá pra aprender a resposta antes de ela
> virar o problema, e amarra as sete fases numa sequência em vez de sete lutas soltas.

**AS DUAS REGRAS QUE GOVERNAM AS TABELAS:**

1. **Ninguém fica FORA DE POSIÇÃO, em fase nenhuma.** Todo `G`/`C` na 1 ou 2, todo `A`/`S` na 3 ou 4 —
   sempre com o kit funcionando inteiro. **A variedade vem da COMPOSIÇÃO, não do desalinho:** dois
   guardiões é uma muralha, dois combatentes é dano sem parede, dois suportes é uma luta que não
   acaba. Cada formação é um problema diferente, e nenhuma é "o mesmo inimigo mal arrumado".
2. **A estreia é sempre na rodada 2.** A rodada 1 é com quem você já conhece; a 2 apresenta o novo.

> **O time completo fecha na FASE 4, não na 7.** A primeira versão desta tabela dava o Suporte só na
> última rodada do capítulo — ele aparecia **uma vez na vida**, e as fases 5–7 eram a mesma luta com
> nível maior. Puxando as estreias pra frente, o Suporte aparece em 7 rodadas e o jogador **joga
> metade do capítulo com quatro apóstolos** em vez de receber o quarto quando o capítulo acabou.

> **Isto corrige o bug de a fase 1 entregar DOIS apóstolos.** `Campanha.cs:12` monta a fase 1 como
> `[Slot1]` / `[Slot2]` e o `DesbloquearApostolos` varre as duas rodadas — por isso saem dois.

> **✅ RESOLVIDO — o que acontece quando alguém MORRE: nada. O corpo fica na casa** (§2). A pergunta
> existia porque o modelo antigo tinha PORTÃO de alcance, e aí um buraco podia travar a luta (um
> Combatente que só alcançava 1–2 contra um Atirador na 4 nunca encostava nele). **Sem portão não há
> travamento possível**, então a compactação de fileiras e o ⚔️ Atacar universal — que eram os dois
> remendos propostos — deixaram de ter problema pra resolver. E manter a casa ocupada é o que os
> apóstolos que **revivem** precisam.

### Os inimigos não têm itens — e isso resolve a calibragem

**O inimigo escala por NÍVEL, e o multiplicador de fase MORRE.** Decisão do Gabriel: nada de
`0,5×capítulo + 0,1×fase` multiplicando status. O inimigo usa a **mesma curva de nível do jogador**
(§2) sobre a mesma tabela de tipo — **uma fórmula só pros dois lados**, e some a armadilha das "duas
cópias de um número" que o multiplicador criava.

> O doc antes alertava pra manter a conta como multiplicador e **não** como nível de verdade. Vale o
> contrário: é justamente ser nível de verdade que garante que jogador e inimigo nunca divirjam.

**A CALIBRAGEM É POR DIFICULDADE**, e tem de ser: a raridade do apóstolo trava o nível dele, então no
Fácil ele para em **30** e só no Pesadelo chega a **60**. Uma tabela só serviria a uma dificuldade.

Medido em simulação 4×4 com barra de turno (ago/2026). Lê-se *"jogador nv X enfrenta inimigo nv Y"*:

| cap | Fácil | Normal | Difícil | Pesadelo |
|--:|--:|--:|--:|--:|
| 1 | 1 → **14** | 30 → **63** | 40 → **97** | 50 → **145** |
| 2 | 4 → **17** | 31 → **67** | 41 → **107** | 51 → **157** |
| 3 | 7 → **30** | 32 → **84** | 42 → **129** | 52 → **184** |
| 4 | 10 → **37** | 33 → **86** | 43 → **130** | 53 → **186** |
| 5 | 14 → **42** | 35 → **97** | 45 → **148** | 55 → **191** |
| 6 | 17 → **63** | 36 → **120** | 46 → **171** | 56 → **225** |
| 7 | 20 → **67** | 37 → **121** | 47 → **173** | 57 → **227** |
| 8 | 23 → **71** | 38 → **122** | 48 → **175** | 58 → **228** |

**A regra que sobrevive aos números: o inimigo vale ~3× o nível do jogador, subindo pra ~4× no
Pesadelo.** A razão cresce **porque o item do jogador cresce** — o inimigo não tem item, então precisa
de nível extra pra compensar o equipamento. É o mesmo fato de outro jeito: *não tem problema o inimigo
ser muito mais forte depois que eu começar a me equipar*.

> **Trocar de dificuldade dá um respiro, e ele é de graça.** O capítulo 1 de cada dificuldade é mais
> fraco que o capítulo 8 da anterior (71 → 63, 122 → 97, 175 → 145). Ninguém desenhou isso: cai de o
> jogador ganhar 10 níveis e um degrau de raridade ao virar a página.

> **O chute do Gabriel foi "nível 250".** O 1×1 deu 254 e o 4×4 — que é o jogo de verdade — baixou pra
> 228, porque o Guardião e o Suporte puxam o dano do time pra baixo. O 1×1 acerta a FORMA da curva e
> erra a escala por um fator estável de ~`0,7`, então serve de instrumento barato pra ver o efeito de
> um número sem rodar o time inteiro.
>
> **O que a simulação NÃO tinha:** habilidade nenhuma, cura nenhuma, e as posições fora da conta. Os
> dois lados perdem o mesmo, então a forma deve sobreviver — **a escala não**. Estes níveis vão mudar
> quando as habilidades entrarem, e provavelmente pra CIMA, porque o jogador é quem escolhe o time.

---

## 6. A BANCADA 2.0 — time contra time

A bancada atual mede **apóstolo contra boneco parado**. Com posição, velocidade e tipos, ela fica ainda
mais cega: não vê alcance, não vê ordem de turno, não vê sinergia.

**A que responde a pergunta do Gabriel roda TIME contra TIME**, bot contra bot, e conta vitórias.
"Quebrado" deixa de ser uma opinião sobre dano e vira o que aparece nos times que ganham. O modo
Arena já existe justamente pra isso, e o `ControladorBot` já joga os dois lados.

---

## 7. O PLANO — agora × depois

> **Esta divisão é ORIENTAÇÃO, não lei** (Gabriel: *"não quero cravar nada de Steam ou agora, quero ir
> fazendo até me satisfazer e decidir sozinho quando trocar"*). Nada aqui adia nada por estar escrito
> numa lista de depois.

### AGORA (muda o modelo e o save; tem que vir junto)

**A ordem importa e não é negociável:** status e turno ANTES de nível e raridade. Subir status antes de
mudar quem joga quando é calibrar contra uma ordem de turno que ainda vai mudar.

1. ✅ **Tipos + o stat base vindo do tipo** — FEITO (#228). Os 108 números soltos viraram 4 fichas +
   9 torções, o crítico saiu de constante global pra vir do tipo, e a ficha inteira chegou na tela.
2. **Posição** (§2): o campo na horizontal com as frentes se olhando, a ordenação livre na montagem,
   o perfil de distância dentro do pipeline de dano e o mapa de calor. **Subiu de último pra segundo**
   *(ago/2026)* — o desempate da barra de turno é **por posição**, então ela tem que existir antes.
3. **Velocidade + barra de turno + fila única.** Mexe em `Batalha`, `Equipe`, `TurnoDoPersonagem` —
   hoje a ordem é um `for` sobre `Equipe1.Membros ++ Equipe2.Membros`, e é esse `Concat` que é a
   vantagem de time que o §1 quer matar.
4. **Precisão × Resistência** (chance de colar) + a **DEF em `DEF/(DEF+5000)`**, no lugar do cap atual.
5. **Nível (curva do tipo) + Raridade** nos apóstolos. Sem estrela.
6. **Raridade → passiva que escala.**
7. **Item equipado no apóstolo.**

> **A ordem de 3 e 4 continua não sendo negociável** em relação a 5 e 6: status e turno ANTES de nível
> e raridade. Subir status antes de mudar quem joga quando é calibrar contra uma ordem de turno que
> ainda vai mudar.

> **O save atual é DESCARTADO** (decisão do Gabriel: *"descarta, não me importo"*). Sem migração.

### DEPOIS

Subestatísticas e aprimoramento · conjuntos 2/4/6 · drop por fase e dificuldade · a tela do "o que cai
onde" · escolher 1 apóstolo inicial · **as barras de uso, os marcos e a forja** · **missões por apóstolo**
· a fase 1 entregando um apóstolo só · **a dificuldade, agora calibrada contra a progressão** · **a
bancada 2.0** · e só então o **#16**.

Sem data e sem plataforma: cloud save · os 3 acessórios (9 peças de conjunto) · conquistas e
telemetria · **Precisão × Evasão**, se o combate pedir.

---

## Decisões já fechadas (não reabrir)

- A posição é **compromisso do jogador**; nada de correção automática. A ordenação é **livre** — nada
  de travar cada tipo na casa natural dele, porque é justamente furar a casa natural que abre build.
- **A POSIÇÃO NÃO PROÍBE, ela modula** (§2): cada tipo tem uma **distância ideal** (🛡️ 1 · ⚔️ 4 ·
  🏹 5 · 💗 plano), pico 1,30 caindo 0,10 por casa de desvio pros dois lados. **Nenhuma habilidade
  fica indisponível em casa nenhuma.** Quem morre **fica na casa** — as fileiras não compactam.
- **Nunca mostrar dano previsto antes de usar a habilidade** (§2). Multiplicador é ficha e pode ser
  mostrado; dano previsto é simulação e fica fora da tela.
- **A BARRA DE TURNO está fechada** (§1): barra própria por apóstolo · age quem cruzou 100, o mais
  cheio primeiro · desempate por **posição** e depois pelo **lado do jogador** · a sobra **carrega** ·
  a ação **custa tempo**. **Não trocar "mais cheio" por "mais rápido"** — foi proposto e descartado,
  porque punha alguém em 105% na frente de alguém em 133% e transformava a barra da tela em mentira.
- **O custo da ação é ADIMENSIONAL** (§1): guarda-se **10% do ciclo de um apóstolo de referência**,
  não `0,05`. Teto de estabilidade: `FRAÇÃO < 100 ÷ (nº em campo)`. **Não** derivar de `Σvel` — obriga
  cada apóstolo a saber quem está vivo, e a medição mostrou que é desnecessário.
- **Cortar não é mecânica, é consequência** (§1) — 1–4% medidos, e não responde a nenhum botão. O que
  se projeta é a **legibilidade da ordem**, não a taxa.
- O save atual é **descartado**.
- **Não** implementar esquiva agora.
- A raridade **sobe**; não é fixa no drop.
- Não tentar impedir save editado — tornar inútil, não impossível.
- **O apóstolo não tem estrela.** A raridade é o teto de nível dele.
- **Stat base é do tipo**, não do apóstolo; o nível sobe por **curva do tipo**, sem pontos distribuíveis.
- **A TABELA DE STATS BASE dos 4 tipos está calibrada** (§2) — do nv 60 pra trás, com a grade antiga
  descartada. Critério de aceitação: **cada tipo é 1º em dois stats**. Guardião HP/DEF · Combatente
  os dois de crítico · Suporte Precisão/Resistência · Atirador ATK/Velocidade.
- **A curva de nível é CONTÍNUA e vai de 1× a 30×** (`base × (1 + 29(nv−1)/59)`). Declaram-se as
  PONTAS; a taxa por nível é consequência. Só HP/ATK/DEF escalam.
- **A CURVA DE XP é `100 × N` por nível**, e a fase dá `22 × (cap×7 + fase)` vezes o multiplicador da
  dificuldade (1·2·3·4). Calibrada pra **uma passada do Fácil fechar no nv 30** sem repetir nada.
  Consequência aceita: do Normal em diante o teto de nível chega por volta do capítulo 3, e o resto da
  passada é jogo de ITEM.
- **Dano Crítico tem piso de 60%** pra todos — nenhum item pode ser lixo pra um tipo inteiro.
- **OS PRINCIPAIS DOS 9 SLOTS ESTÃO CALIBRADOS** (§4), no 6★ +20. A **Velocidade tem fonte ÚNICA** (a
  Bota, +50); os outros quatro especiais têm DUAS, sempre com um acessório de dungeon como segunda.
- **A proporção Manopla:Pulseira é 2:1** nos dois stats de crítico (50/25 e 100/50). É ela que faz cada
  degrau custar 5 rolos de sub e pagar +0,50 de multiplicador — **nenhuma opção de luva domina**, que é
  o defeito conhecido do Raid.
- **NÃO travar a contribuição das subs de Taxa.** O custo já é a escassez de slot; trava artificial
  mataria o item duas vezes.
- **A sub tem a mesma FORMA que o principal do slot** (cheio com cheio, % com %). Acessório não dá
  Velocidade, nem principal nem sub.
- **NOMENCLATURA (corrigida, ago/2026):** o que sobe **jogando** é o **NÍVEL** — igual ao apóstolo, e é
  essa simetria que dá o nome. O que sobe **pagando** na forja é o **APRIMORAMENTO** (`+0…+20`), que é
  o `(1)`…`(5)` ao lado da sub. O doc chamava os dois ao contrário.
- **UM NÍVEL SÓ, e ele dá a ESTRELA.** A raridade não tem trilha: é destravada por ATOS (sacrifício +
  marco + `raridade ≤ estrela`). A versão de duas barras morreu pelo argumento que o próprio doc já
  dava — duas trilhas que enchem juntas, da mesma fonte, sem escolha entre elas, são uma trilha só.
- **A rodada vale mais onde dói:** 1 ponto no Fácil, 2 Normal, 3 Difícil, 4 Pesadelo. Sem isso, o jeito
  ótimo de estrelar item seria repetir a **Fácil 1-1** — o teto por batalha impede arrastar a luta, não
  repetir a luta curta.
- **A estrela custa `100 · 200 · 400 · 800 · 1.600` pontos**, dobrando. Cada teto de dificuldade cabe
  em 30–60% de uma passada dela: nenhum obriga a repetir fase, nenhum é de graça.
- **O marco é POR PEÇA** — terminar o capítulo 1 no Normal destrava as 7 peças do Reino e mais nada.
- **Evoluir não atropela o drop:** estrela e raridade o drop também dá; **só o aprimoramento é
  exclusivo de quem persiste**.
- **A ESCALA é `principal = MÁXIMO × estrela`** — só a estrela, `25→100%`. **O aprimoramento NÃO toca
  o principal**; ele entrega unidades de sub. Um `6★ +0` e um `6★ +20` têm o mesmo principal.
- **A sub escala pela ESTRELA também**, junto com o principal — é o que trava a sub em **metade** do
  principal, em qualquer estrela, sem calibragem nenhuma.
- **A raridade não multiplica nada** — ela trava o teto de aprimoramento (`+4 … +20`) e com ele a
  contagem de subs (0 a 5).
- **Unidade de sub = aprimoramento**, um pra um. O `(5)` do mítico é o bônus que **só a evolução dá**.
- **Resetar mexe só no APRIMORAMENTO** — o nível ganho jogando não é tocado.
- **A ficha mostra 2 CASAS DECIMAIS.** Arredondar esconderia justo a Taxa Crítica perto do teto, que é
  o número que o jogador mais precisa conferir — é o defeito conhecido do Raid.
- **O INIMIGO ESCALA POR NÍVEL, não por multiplicador** (§5) — mesma curva e mesma tabela de tipo do
  jogador, uma fórmula só pros dois lados. **Calibrado POR DIFICULDADE** (o teto de nível do apóstolo
  muda com ela): ~**3×** o nível do jogador, subindo pra ~**4×** no Pesadelo. O `0,5×cap + 0,1×fase`
  está morto.
- **Nível só toca HP/ATK/DEF.** Velocidade, Precisão e Resistência não escalam — e como inimigo não tem
  item, é isso que permite calibrar o eixo de efeito de uma vez pro jogo inteiro. *(Em aberto, como
  pensamento: talvez algo disso mude em BOSS.)*
- **A descoberta um-por-um existe SÓ no Fácil**; nas outras dificuldades são sempre 4 inimigos. Morre
  a regra antiga do *"4 inimigos a partir do capítulo 2"*, que resolvia por capítulo o que é por
  dificuldade. As duas tabelas de composição estão no §5.
- **Ninguém fica fora de posição em fase nenhuma** — a variedade vem da COMPOSIÇÃO (dois guardiões,
  dois suportes…), não do desalinho. E a estreia de um apóstolo é sempre na **rodada 2**.
- **Cada fase dropa SÓ o slot dela**, inclusive a 7. O que a fase difícil paga é **XP**.
- **Velocidade não escala com nível** — vem de equipamento (só +5 do nv 1 ao 60, por tipo).
- **Subestatísticas iguais** no drop e na evolução; nenhum privilégio de nascença pro drop.
- ~~Duas barras (estrela e raridade)~~ — **revogado**: virou UM nível, e a raridade sai de atos.
- A barra é do **item**, não do apóstolo.
- **Derrota também progride** a barra; a vitória dá bônus.
- **Quem escolhe o slot do aprimoramento é o RNG.**
- **O marco do item é por FASE**, não por capítulo — vencer a fase de origem dele naquela dificuldade.
  Isso fecha a cadeia sozinho: não existe item com estrela acima do próprio marco.
- **A DEF usa `DEF/(DEF+5000)`** — sem cap, sem penhasco, e o item de DEF nunca vira lixo.
- **`Aptidão` virou `Precisão`**, na escala 0–1.000 (típico 400–600). A esquiva segue cortada.
- **Malefício é CHANCE**, não potência reduzida: `min(100%, P ÷ 2R)` pra colar, mais `(1−chance)÷2` de
  perder 1 turno, com piso de 1. **O dobro da Resistência garante 100%** — o RNG é comprável.
- **Sem retornos decrescentes globais.** Quem trava controle é **passiva do BOSS**, escrita na ficha
  dele — conteúdo em vez de regra, e zero exceção no motor.
- **Percentuais SOMAM entre si** e incidem sobre `base + valores cheios`.
- **Subs em % na armadura, em valor cheio nos acessórios** — assim as duas formas do mesmo stat nunca
  aparecem lado a lado, e nenhuma escolha do jogador é aritmética.
- **Conjunto de 9 peças, bônus em 3/6/9.**
- **Variação por facção de ±5%, soma zero**, com a Velocidade fora da matriz.
- **Dano pode escalar com DEF/HP SOMANDO ao ATK**, nunca substituindo.
- **Não há restrição temática de sub por slot** (arma pode ter `DEF%`).
- **Apóstolo dropa garantido** na fase dele.
- Nada de **forçar o uso** de um apóstolo específico pra passar de fase.

### Revogado nesta rodada (estava escrito aqui e não vale mais)

| dizia | vale agora | por quê |
|---|---|---|
| *"a posição mora na HABILIDADE"* — `posicoesDeUso`/`posicoesAlvo`, o portão do DD | **a posição MODULA o dano** por distância ideal do tipo (§2) | o portão desliga metade do kit de quem está na casa errada; aqui **nenhuma habilidade morre**, e a decisão vira dial em vez de penhasco. Custo: perde-se o castigo duro do DD, ganha-se que todo apóstolo sempre tem o que fazer |
| *"empurrar e puxar viram ataque porque desligam o kit"* | empurrar e puxar **deslocam o pico**, não desligam nada | com o portão fora, tirar o arqueiro da casa 4 não o cala — muda em quem ele bate mais forte |
| *"compactar as fileiras quando alguém morre"* (proposta) | **o morto fica na casa** | existem apóstolos que revivem, então a casa tem que estar lá esperando. Some junto o **⚔️ Atacar universal**, que era o remendo pra ninguém ficar travado |
| *"o tipo é identidade, NÃO geometria"* | o tipo é identidade **e** geometria (§2) | a curva de distância é do TIPO — é o gesto dele no tabuleiro, e é o que separa dois apóstolos de fichas iguais |
| *"na campanha só caem comuns"* | qualquer raridade cai em qualquer dificuldade | reservar raridade pra loja é desenho de gacha, e aqui não há loja |
| *"a fonte decide o teto de raridade"* (Fácil até Incomum…) | a **estrela** é o teto | virou redundante — a estrela já limita, e piso por dificuldade não acrescentava nada |
| *"fusão com semente"* | **sacrifício da raridade atual** alimentando 3 opções de sub | o sacrifício dá custo, liga o drop à evolução e ainda decide as opções |
| *"subir estrela não zera o nível"* | **subir raridade** não zera o nível | o apóstolo perdeu a estrela; some uma exceção, porque essa já era a regra do item |
| *"apóstolo tem chance de cair"* | cai **garantido** | apaga junto a pergunta "e se cair de novo?" |
| *"a ordem dos inimigos nas fases muda"* | **a composição foi redesenhada** (§5) | com posição e tipo, quem está em cada casa passou a significar algo — a tabela velha era arbitrária |
| *"a partir do capítulo 2, toda fase é contra 4 inimigos"* | **só o Fácil descobre um por vez**; Normal+ é sempre 4 | resolvia por CAPÍTULO o que é por DIFICULDADE — nas outras você já achou todos |
| *"a fase 7 dropa mais itens e de TODOS os tipos"* | cada fase dropa **só o slot dela**; a 7 paga em **XP** | a variedade na 7 fazia as outras seis morrerem depois de uma passada |
| *"o inimigo é `0,5×cap + 0,1×fase` de status"* | o inimigo tem **NÍVEL**, na mesma curva do jogador | o multiplicador foi escrito contra um jogador sem progressão; e duas fórmulas de poder divergem |
| *"duas barras, uma pra estrela e uma pra raridade"* | **um nível só**; a raridade vem de ATOS (sacrifício + marco) | o próprio doc dizia que não era escolha — duas trilhas que enchem juntas, sem escolha entre elas, são uma trilha só |
| *"a barra enche por rodada"* (valor único) | a rodada vale **1·2·3·4** por dificuldade | valor único fazia o ótimo ser repetir a Fácil 1-1; o teto por batalha impede arrastar a luta, não repetir a luta curta |
