# GDD — ITENS: slots, subestatísticas, escala e evolução (§4)

> **Tipo:** MODELO (referência viva), e o passo mais DISTANTE da fila — item equipado é o 7º e
>   último item do §7, e as subs estão no DEPOIS.
> **Função:** os dois eixos do item, os 9 slots e o que cada um paga, as 8 subs, a escala pelo
>   nível, o drop, a evolução e as dungeons.
> **Cuidado ao ler:** quase nada aqui está implementado. É desenho fechado esperando a vez.

> **A NUMERAÇÃO É A DO GDD ORIGINAL**, e ela vale nos três arquivos — uma referência a "§4" quer
> dizer a mesma coisa em qualquer um deles:
>
> | § | assunto | arquivo |
> |---|---|---|
> | §1 §2 | stats novos, barra de turno, posição e tipo | `GDD-combate.md` |
> | §3 §5 §6 §7 | nível e raridade, campanha, a bancada 2.0, o plano, as decisões fechadas | `GDD-progressao.md` |
> | §4 | itens | `GDD-itens.md` |

---

## 4. ITENS

> **O PRINCÍPIO, e ele é do Gabriel:** *"o mais fraco sempre pode virar o mais forte"*. O comum nível 1
> que caiu na primeira fase pode terminar mítico nível 60 com as subs escolhidas a dedo. **O preço de
> manter isso saudável é o CUSTO:** o caminho do fraco tem de ser mais caro que vestir o forte já
> pronto, senão ninguém quer o forte. Toda trava desta seção existe por causa disso.

- **Equipados no APÓSTOLO**, não mais no jogador. É o que os torna valiosos — e é a mudança de maior
  impacto no save.
- **CONJUNTOS de 9 peças**, com bônus em **3 / 6 / 9**. E o que cada conjunto faz cresce com a
  raridade. *(O que cada conjunto FAZ ainda não foi desenhado.)*

> **O número ímpar deixou de ser problema.** O GDD antigo remendava com bônus em 2/4/6 e o acessório
> **fora** do conjunto — sobrava uma peça sem função. Com os **2 acessórios das dungeons**, são 9
> peças, três degraus iguais entre si, e **toda peça conta**.

### Os dois eixos do item — e cada um com sua fonte de custo

| eixo | o que dá | como sobe | o que custa |
|---|---|---|---|
| **raridade** | QUANTAS subestatísticas | forja | sacrificar peça igual + material |
| **nível** (1–60) | QUANTO cada número vale: o principal e cada sub | uso, jogando | tempo de jogo + material no pedágio |

> **Uma frase por eixo, e é assim que se explica o sistema inteiro:** **raridade = quantas · nível =
> quanto.** Os dois valem igual pro apóstolo (§3) — uma regra aprendida, usada em dois objetos.

> **OS DOIS EIXOS SÃO INDEPENDENTES.** A raridade **não** trava o nível: um **comum nível 60** existe,
> é legítimo e é caro — 100% de principal e sub nenhuma. Quem trava o nível é o **pedágio**
> (`GDD-progressao.md` §O PEDÁGIO), que pede material da dificuldade; e é por isso que o teto de 30/40/
> 50/60 por dificuldade continua de pé sem a raridade participar disso.

> **O nível do item sobe JOGANDO**, igual ao do apóstolo: os dois ganham nível sendo usados, e é essa
> simetria que dá o nome. O material **acelera** e é cobrado no pedágio a cada dezena, mas não
> substitui o uso.

> **A raridade não tem trilha própria** — ela vem de um ATO, o sacrifício na forja. Nível se enche;
> raridade se paga. Ver §A EVOLUÇÃO.

### A ESTRELA — o visor do nível, e ela não é um eixo

**A cada dezena de nível o item ganha uma estrela.** Cai no drop com **nenhuma** e chega ao nível 60
com **seis**:

| nível | 1–9 | 10 | 20 | 30 | 40 | 50 | 60 |
|---|:-:|:-:|:-:|:-:|:-:|:-:|:-:|
| **estrela** | ☆☆☆☆☆☆ | ★ | ★★ | ★★★ | ★★★★ | ★★★★★ | ★★★★★★ |
| **principal** | 11,5→23,5% | 25% | 40% | 55% | 70% | 85% | **100%** |

- **Ela não tem fonte de custo nem efeito próprio** — é leitura do nível, e por isso não compete com
  eixo nenhum. Os seis valores são os mesmos seis da grade do principal.
- **A ficha do drop já mostra os seis contornos vazios**, então a peça diz "tenho seis pra encher"
  sem tela explicando nada.
- **O pedágio senta entre as estrelas:** são 6 estrelas e 5 pedágios — ganha-se a ★, bate-se na parede,
  paga-se pra continuar. A estrela é o recibo, e o pedágio não precisa de UI própria.
- **Vale igual pro apóstolo** (§3): mesma dezena, mesma estrela, mesmo desenho.
- **A estrela do item e a raridade dele dizem coisas diferentes**, e é isso que faz as duas caberem na
  mesma ficha: `comum 6★` é "levei ao máximo o que caiu"; `mítico 0★` é "achei o troféu ontem".

### A FÓRMULA — o % multiplica o cheio, e os %s somam entre si

```
total  =  (base + valores cheios) × (1 + Σ percentuais)
```

**Duas decisões dentro de uma linha, e cada uma resolve um problema:**

**1. O % incide sobre `base + cheios`.** Hoje (`Combate.cs:98`) ele multiplica só a base e **ignora os
valores cheios** — então cheio e % viram duas parcelas somadas que **competem pelo mesmo slot**, e
qual delas ganha muda com o nível (cedo o cheio é enorme perto da base pequena; tarde a base cresceu e
o % passa na frente). Multiplicando, os dois **se amplificam**: cada ponto de cheio aumenta o quanto
vale cada % que você já tem. O cheio deixa de ser concorrente e vira o **piso** sobre o qual tudo
incide — nunca é lixo.

**2. Os percentuais SOMAM entre si; nunca compõem.** Três peças de 3% dão **9%**, não `1,03³`.

| modelo | 3 peças de 3% sobre `base 10.000 + cheios 5.000` |
|---|---|
| **soma** ← esta | 16.350 |
| compõe | 16.391 |
| % sobre a base crua (o de hoje) | 15.900 |

> Compor parece inofensivo com números pequenos (0,25% de diferença aqui), mas com 7 peças a 20%
> aditivo dá `×2,4` e composto dá `×3,58` — **50% de diferença**, e no topo os %s serão grandes. Pior:
> compor tira a **previsibilidade** (somar os %s da ficha é conta de cabeça; multiplicar sete fatores
> não) e faz **o valor de uma peça depender das outras**, o que torna comparar dois itens impossível.

**Buffs de combate continuam por cima**, como já são hoje (`base × multFase + itens` → bônus permanente
→ buff/debuff sobre esse total). Isso separa **o que você construiu** (itens, aditivo, previsível) do
**que aconteceu na luta** (buffs, temporário, multiplicativo) — e um buff de +50% valendo mais no
boneco bem-equipado é o comportamento certo.

**Calibrar o valor cheio pelo TOPO:**

```
rolagem de valor cheio  ≈  (% por rolagem) × (base típica do topo)
```

Assim os dois **empatam no full mítico** e o cheio **domina no começo** (onde a base é pequena) — que é
o desenho que se quer: nenhum dos dois é lixo em momento nenhum da curva.

### OS 9 SLOTS — 3 fixos, 4 variáveis, 2 acessórios

Os 7 de armadura **já existem e já têm nome de corpo** (`ArsenalService.cs:73-83`), um por fase.

> **OS 7 DA CAMPANHA NASCEM ABERTOS** — a regra é do Gabriel: *"item que cai na campanha tem de poder
> ser usado desde o início"*. Dropar peça que o apóstolo não pode vestir é o jogo entregando presente
> fechado, e não existe raridade nem nível que destrave slot de campanha.
>
> **Os 2 ACESSÓRIOS são a exceção, e podem ser** porque não caem na campanha: eles saem das dungeons
> (§AS DUNGEONS), então ninguém fica com item morto na mochila. Eles destravam pela **estrela do
> apóstolo**: o primeiro em **4★** (nível 40), o segundo em **6★** (nível 60).
>
> **Por que estrela e não raridade:** a raridade já entrega poder demais pela habilidade (§3) — dar os
> slots pra ela também a transformaria no único eixo que importa. Preso à estrela, o segundo acessório
> só existe no **nível 60**, que só se alcança no Pesadelo (o pedágio @50 pede material mítico): o
> portão do endgame sai de graça, sem regra nova. E o gap entre os dois é de **20 níveis**, o maior
> vão da trilha.

| slot | | forma | principal — e o valor no **mítico nível 60** |
|---|---|---|---|
| **Arma** (fase 1) | fixo | cheio | ATK **+500** |
| **Elmo** (fase 2) | fixo | cheio | HP **+11.000** |
| **Escudo** (fase 3) | fixo | cheio | DEF **+500** |
| **Manopla** (fase 4) | variável | % | ATK%·HP%·DEF% **50%** · **Taxa Crít 50%** · **Dano Crít 100%** |
| **Peitoral** (fase 5) | variável | % | ATK%·HP%·DEF% **50%** · **Resistência +125** |
| **Calça** (fase 6) | variável | % | ATK%·HP%·DEF% **50%** · **Precisão +125** |
| **Bota** (fase 7) | variável | % | ATK%·HP%·DEF% **50%** · **Velocidade +50** |
| **Pulseira** | dungeon | cheio | HP·ATK·DEF cheio · **Taxa Crít 25%** · **Dano Crít 50%** |
| **Colar** | dungeon | cheio | HP·ATK·DEF cheio · **Precisão +125** · **Resistência +125** |

**Os dois acessórios se leem numa frase cada:** a **Pulseira é o acessório do CRÍTICO**, o **Colar é o
do EFEITO**, e os dois carregam o cheio de HP/ATK/DEF — que é a pista do quem só quer status bruto.
**Nenhum dos dois dá Velocidade**, nem no principal nem em sub: ela tem uma fonte só no jogo inteiro.

**Como os valores foram derivados** (nada aqui é chute solto):

| stat | valor | de onde vem |
|---|---|---|
| ATK% · HP% · DEF% | 50% | escolha do Gabriel; é o degrau de referência de todos os outros |
| Arma · Elmo · Escudo | +500 · +11.000 · +500 | **50% da base MÉDIA dos 4 tipos** no nv 60, pela regra do `rolagem de cheio ≈ % × base típica do topo` acima |
| Velocidade | +50 | **os mesmos 50%**, aplicados à base média de Velocidade (≈100). A Bota vale exatamente o que qualquer outro principal vale — nem mais |
| Precisão · Resistência | +125 × 2 fontes | 250 no total, dividido entre a peça de armadura e o Colar |
| Taxa · Dano Crítico | 50/25 e 100/50 | proporção **2:1** entre Manopla e Pulseira — ver a escada abaixo |

> **Por que a Velocidade não é maior.** A faixa entre os arquétipos é de **30 pontos** (85→115). Uma
> Bota de +100, que chegou a ser proposta, é **mais que o triplo dessa distância** — o número do tipo
> viraria ruído e um Guardião com Bota passaria longe de um Atirador sem. Com +50 a Bota quase dobra
> a Velocidade (continua sendo o grande prêmio) e a distância entre os tipos sobrevive.

#### A ESCADA DO CRÍTICO — por que 2:1, e por que nenhuma opção domina

A Taxa Crítica é o **único stat do jogo com teto duro**. No Raid isso degenera: a luva de dano crítico
é sempre a certa, porque subs e conjuntos capam a taxa sozinhos e a luva de taxa passa a valer **zero**.
A proporção 2:1 entre Manopla e Pulseira evita isso. Combatente (base 25% taxa, 90% dano crít), variando
só o que ele põe nas duas peças:

| Manopla | Pulseira | taxa de principal | subs pra capar | dano crít | **multiplicador** |
|---|---|--:|--:|--:|--:|
| Taxa 50 | Taxa 25 | 75 | **0** | 90% | **1,90** |
| Taxa 50 | DanoCrit 50 | 50 | 5 | 140% | **2,40** |
| DanoCrit 100 | Taxa 25 | 25 | 10 | 190% | **2,90** |
| DanoCrit 100 | DanoCrit 50 | 0 | 15 | 240% | **3,40** |

**Cada degrau custa exatamente 5 rolos de sub e paga exatamente +0,50 de multiplicador** — consequência
de a proporção ser a mesma nos dois stats. Por isso **nenhuma linha domina**: a pergunta deixa de ser
*"qual é melhor?"* e vira *"o que esses 5 rolos fariam em outro lugar?"*.

**E a base do tipo aparece como rolos economizados**, que é a vantagem do Combatente em ato:

| | base | +75 de principal | ainda falta |
|---|--:|--:|--:|
| Combatente | 25% | **100%** | 0 rolos |
| Atirador | 15% | 90% | 2 rolos |
| Suporte | 10% | 85% | 3 rolos |
| Guardião | 5% | 80% | 4 rolos |

> **NÃO travar a sub de taxa** (proposta descartada). Chegou-se a propor um teto artificial na
> contribuição das subs; é desnecessário e prejudicial. **O custo já existe e é a escassez:** chegar a
> 70% de taxa por sub gasta um slot em SETE peças, slots que deixariam de ser qualquer outra coisa.
> Uma trava por cima disso mataria o item duas vezes.

#### PRECISÃO E RESISTÊNCIA — e por que 250 basta

**O crescimento do inimigo é NÍVEL (§5), e o nível só toca HP/ATK/DEF.** Velocidade, Precisão e
Resistência **não escalam** — nem do lado do jogador nem do inimigo. E como inimigo não tem item (§5),
a Resistência dele fica presa na base do tipo, **50–150, no jogo inteiro**.

Isso torna esses dois os **únicos stats que dá pra calibrar de uma vez pro jogo todo**, e a conta fecha:

```
garantir 100%  →  Precisão = 2 × Resistência do alvo
alvo mais teimoso  →  R 150  →  preciso de 300

Precisão do jogador (base + Calça 125 + Colar 125):
   Suporte 400 · Atirador 370 · Combatente 330 · Guardião 300
```

**Os quatro garantem contra qualquer inimigo comum**, e o Guardião raspando — ele consegue, mas
gastando Calça E Colar num stat que não é dele. E o eixo continua vivo do outro lado, porque a
Resistência do jogador não é infinita: contra um inimigo de Precisão 150, o Suporte é atingido 27% das
vezes, o Guardião 31% e o **Atirador 43%** — a fragilidade dele funcionando.

> **Fica em aberto, e é PENSAMENTO do Gabriel, não decisão:** talvez um dia Precisão ou Velocidade
> escalem em BOSS. É o lugar natural — o GDD já põe a trava de controle na passiva do boss (§1). Por
> ora **o inimigo evolui só o nível normal, igual aos apóstolos**, e nada mais.

**Por que metade fixa e metade variável:** se todos variassem, o jogador teria 7 loterias simultâneas e
nunca fecharia uma build; se nenhum variasse, não existiria build, só acúmulo. **Fixos = esqueleto
garantido, variáveis = espaço de escolha.** E o encaixe com as fases é feliz: os fixos são as fases
**1–3** (o esqueleto vem cedo e barato) e os variáveis as **4–7** (a build vem tarde e cara).

**Cada slot variável tem um stat próprio** mais o trio `ATK%/HP%/DEF%` — assim **nenhum drop é 100%
lixo**, e cada peça tem motivo próprio de ser farmada. A Manopla é a mão que golpeia (taxa × dano viram
escolha ali); o Peitoral é o torso que aguenta; a Calça é a perna que se move.

**Exclusivo mesmo, um só: a Velocidade.** Ela é a única com fonte ÚNICA (a Bota), e é de propósito —
sendo o stat mais forte do jogo, uma segunda fonte a tornaria barata. E a Bota **já era a fase 7**, a
mais difícil do capítulo, então ele nasce no lugar mais caro sem mexer em nada.

> **Os outros quatro especiais têm DUAS fontes**, e isso é desenho e não descuido: Taxa e Dano Crítico
> na Manopla + Pulseira; Precisão na Calça + Colar; Resistência no Peitoral + Colar. A segunda fonte é
> sempre um **acessório**, que vem de dungeon — conteúdo mais difícil, então é prêmio, não atalho.
> **A regra que governa isso: stat com TETO tem que ter fonte controlada** (a Taxa é a única com teto,
> e por isso as duas fontes dela somam 75% em vez de capar sozinhas); stat sem teto aguenta duas.

> **`ATK%` ainda não existe.** O `Combate.cs:86` já tem o `ItensAtaquePct` sendo usado no cálculo, mas
> **não há `TipoStat.ATKPct`** que o alimente — HP e DEF têm cheio *e* %, o ATK só tem cheio. É criar o
> valor no enum e ligar.
>
> **O botão da Velocidade:** a raridade dela é o **tamanho do leque da Bota**. Quatro opções = 25% de
> chance de vir Velocidade. Quer mais rara? Acrescenta opções. Um número por slot.

### AS SUBESTATÍSTICAS — 8 no pool, e nenhuma escolha aritmética

```
Arma · Elmo · Escudo          → subs CHEIAS      (a forma do principal deles)
Manopla · Peitoral · Calça · Bota → subs em %    (a forma do principal deles)
Pulseira · Colar              → subs CHEIAS      — e SEM Velocidade
```

**A divisão é por PEÇA, e a regra é: a sub tem a mesma forma que o principal daquele slot.** Uma peça
de valor cheio tem subs cheias; uma peça de percentual tem subs em percentual. Assim cada item é
internamente coerente, e é o que o jogador olha.

> **A versão anterior dizia "armadura em %, acessório em cheio"** e não fechava: obrigava a Arma, cujo
> principal é `ATK cheio`, a ter subs em percentual. O corte por forma do slot resolve isso e mantém
> a razão original intacta.

**Esta divisão existe por um motivo de INTERFACE, não de balanço.** Se `ATK` e `ATK%` pudessem sair
juntos, o jogador escolheria entre duas caras do mesmo stat — e decidir entre elas é uma **conta**, não
uma escolha. Pior: na forja, duas das três opções seriam a mesma coisa. Separando por peça, **as duas
formas nunca aparecem lado a lado** e toda opção oferecida é uma coisa diferente das outras.

> **A regra só morde em TRÊS stats.** ATK, HP e DEF são os únicos que têm duas caras. Taxa, Dano
> Crítico, Velocidade, Precisão e Resistência **existem de um jeito só** — não há "Velocidade%" pra
> escolher, então elas aparecem iguais em qualquer peça.

**O pool (8):** `ATK%` · `HP%` · `DEF%` · `Taxa Crítica` · `Dano Crítico` · `Velocidade` · `Precisão` ·
`Resistência`. Nos três slots CHEIOS o trio sai em valor cheio em vez de percentual, e **nos dois
acessórios são 7: a Velocidade não entra.**

Numa Manopla com **Dano Crítico** de principal, as opções são `Taxa Crítica · ATK% · Velocidade ·
Precisão · HP% · DEF% · Resistência` — sete coisas **diferentes**, e a decisão vira estratégica:
*acertar mais, agir mais ou bater mais?*

**As regras:**
- **`sub ≠ principal` daquele item** — a exclusão é da **forma exata**: uma Arma com `ATK cheio` de
  principal **pode** ter `ATK%` de sub, e é justamente ela que o Combatente quer.
- **A mesma sub PODE REPETIR no item**, sem teto de cópias — 5 iguais é build, não exploit. É o único
  jeito de concentrar um stat, e quem decide é a forja: o pool do sacrifício pode oferecer sub que o
  item já tem.
- **As cópias aparecem SEPARADAS na ficha**, uma linha por slot — cinco linhas de `Taxa Crítica`, não
  `Taxa Crítica (5)`. Somar num número só esconderia justamente o que a concentração custa: **os cinco
  slots foram gastos nisso**, e não sobrou nenhum pra outra coisa.
- **Valores FIXOS**, sem variação de rolagem. Não é só "somos offline, não gacha": a variação
  **brigaria com a forja**. Pagar caro, recusar três vezes, escolher a sub que queria — e ela vir no
  mínimo? A escolha comprada seria anulada por um dado. A variação em gacha existe pra criar uma
  camada extra de farm; a forja **é** essa camada, e é conteúdo.
- **A Velocidade é em valor cheio**, nunca %. Em % o Atirador (base 115) ganharia mais velocidade
  absoluta que o Guardião (base 90) com a mesma sub — o rápido ficaria mais rápido e a faixa explodiria
  sozinha.

**O teto, e ele CALCULA o valor da sub:** o pior caso de concentração são **5 cópias** da mesma sub num
mítico. Se a soma delas tem de ficar abaixo do principal no topo:

```
rolagem de sub  =  principal (mítico, nível 60) ÷ 6
```

Escolhe-se **um** número — quanto o principal dá no topo — e o da sub sai por divisão, já obedecendo
"principal > sub concentrada" por construção, em todo stat.

**A sub concentrada chega a 5/6 do principal**, e é perto de propósito: concentrar é o endgame da peça
e tem de valer a conta. O `÷ 6` ainda não passou pelos principais já calibrados dos 9 slots — está na
lista de `GDD-progressao.md` §Os números que faltam.

**O NÍVEL multiplica principal e sub juntos**, então a hierarquia se mantém em qualquer nível sem regra
extra: `valor = base(stat) × fatorNível`.

> **SEM restrição temática por slot** (arma podendo ter `DEF%`, etc.). Ela existe em gacha pra afunilar
> **volume** — o jogador abre centenas de peças e precisa descartar. Aqui o volume não existe (9 slots
> × 8 conjuntos, drop garantido) e **a forja conserta a sub**: o item "errado" é matéria-prima, não
> lixo. O custo dela seria fechar builds legítimas por estética. **E a assimetria decide:**
> acrescentar depois é barato; tirar depois deixa todo item já dropado com um perfil sem sentido.
>
> **A alavanca, se ficar sem sabor:** em vez de PROIBIR `DEF%` na arma, dar a ela **peso menor no
> sorteio**. Troca uma lista de proibições por uma tabela de pesos — dado, não regra.

**O acúmulo tem freio, e ele é CUSTO — não sorte.** Um boneco full mítico tem `9 × 5` subs, e concentrar
as cinco de uma peça no mesmo stat é **alcançável de propósito**: quem paga o sacrifício certo chega lá.
Não há dado segurando isso, há preço — e é por isso que o `N` do sacrifício e a taxa de drop do mítico
são os dois botões que calibram o endgame.

### Raridade → quantas subs

| raridade | subs |
|---|:-:|
| comum | 0 |
| incomum | 1 |
| raro | 2 |
| épico | 3 |
| lendário | 4 |
| mítico | **5** |

**Cada degrau vale exatamente +1 sub.** Escada regular, legível pro jogador, barata de calibrar.

> **Um comum é um item cru de verdade** — sub nenhuma, valendo só o atributo principal. Ele pode
> chegar ao **nível 60 e às 6 estrelas** como qualquer outro (os eixos são soltos), e aí é uma peça de
> principal cheio e nada mais: barata em sacrifício, cara em material, e legítima. O que ele nunca
> tem é opção.

> **A raridade não multiplica nada, e não trava nada.** Ela abre slot de sub; quem move número é o
> nível. Uma coisa cada, sem interseção — é por isso que não é preciso escrever regra pra separá-los.

### A ESCALA — o nível dá magnitude, a raridade dá as subs

Os valores da tabela dos 9 slots são o **teto: mítico, nível 60**. Daí pra baixo:

```
principal  =  MÁXIMO  × fatorNível
sub        =  UNIDADE × fatorNível

fatorNível  =  10 + 1,5 × nível        (em %)
```

| nível | 1 | 10 | 20 | 30 | 40 | 50 | 60 |
|---|--:|--:|--:|--:|--:|--:|--:|
| **principal** | 11,5% | 25% | 40% | 55% | 70% | 85% | **100%** |
| é o teto de | — | comum | incomum | raro | épico | lendário | mítico |

**Os seis tetos de raridade caem exatamente nos seis valores da grade**, e é isso que faz a escada do
principal e a escada da raridade serem a MESMA escada. Nenhum número dos 9 slots muda por causa disso.

> **Um mítico nível 1 tem principal 11,5%**, pior que um comum no teto. Está certo — mítico fresco é
> investimento, não upgrade instantâneo —, mas **a ficha tem de deixar isso óbvio**, senão o drop raro
> parece ter vindo quebrado.

**A TRAVA: a sub escala pelo NÍVEL, junto com o principal.** Por isso a razão entre os dois é fixa em
qualquer nível, sem calibragem:

```
             principal    5 cópias da mesma sub    razão
nível 60       100,00%            83,33%            5/6
nível 30        55,00%            45,83%            5/6
nível  1        11,50%             9,58%            5/6
```

**A sub concentrada nunca alcança o principal**, e isso não é calibragem — é consequência de as duas
escalarem pelo mesmo fator.

> **A ficha mostra DUAS CASAS DECIMAIS** (decisão do Gabriel). Os números não fecham redondo — com 9
> stats e uma rampa contínua, qualquer cruzamento produz decimal — e **esconder a casa é pior que
> mostrá-la**: é o defeito conhecido do Raid, onde a ficha arredonda e o jogador lê `100%` estando em
> 99,6%. Como a Taxa Crítica é o único stat com teto duro e a build inteira gira em torno de fechar
> exatamente esse teto, arredondar esconderia justo o número que precisa ser conferido.
**A jornada de uma peça, do drop ao topo** — uma Manopla de Dano Crítico (máximo 100%):

| etapa | principal | subs | o que mudou |
|---|--:|---|---|
| comum · nv 1 · ☆ | 11,50% | — | o drop |
| comum · nv 10 · ★ | 25,00% | — | a 1ª estrela: o principal dobrou e **sub nenhuma apareceu** |
| raro · nv 30 · ★★★ | 55,00% | Taxa 9,17% · ATK% 9,17% | dois degraus de raridade abriram 2 subs |
| lendário · nv 50 · ★★★★★ | 85,00% | Taxa · ATK% · HP% · DEF%, **14,17% cada** | 4 subs, e as três primeiras subiram sozinhas |
| mítico · nv 60 · ★★★★★★ | 100,00% | as 4 + Resistência, **16,67% cada** | a 5ª sub, e o principal no teto |
| mítico · nv 60 forjado | 100,00% | **Taxa 16,67% × 5** | 5 cópias da mesma sub — a forja concentrando |
| **comum · nv 60 · ★★★★★★** | 100,00% | — | a rota barata: mesmo principal do mítico, **zero opção** |

**O principal cresce quase 9× do drop ao topo, e quem faz isso é o NÍVEL sozinho.** A raridade entrega
outra coisa: de zero a cinco subs. As duas últimas linhas são o desenho aparecendo — **subir de nível
não compra sub, e subir de raridade não compra magnitude**.

### O drop

- **4 itens por fase**, todos do **slot da fase** — é o que arma os quatro do time numa corrida só, e é
  essa a razão do número ser 4.
- **Todo item cai no NÍVEL 1, sem estrela nenhuma.** O que o drop sorteia é a **raridade**; a magnitude
  é sempre conquistada jogando. Um mítico recém-caído é mais fraco que o comum que você já subiu — e é
  isso que impede o drop de apagar o investimento.
- **O item é FIXO por fase**, como já é hoje — a arma na 1-1, e cada fase com a peça dela.
- **CADA FASE DROPA SÓ O SLOT DELA — inclusive a 7.** Sete fases, sete peças de armadura: encaixe
  exato, e **as sete ficam vivas o jogo inteiro**. Quem quer Manopla vai na 4, e não há atalho.
- **O que a fase difícil paga é XP, não variedade.** A quantidade cresce ao longo do capítulo e a
  fase 7 dá mais XP — a 8-7 é a que mais dá no jogo. **É esse o motivo de farmar a fase difícil**, e
  ele não compete com o item: você escolhe entre *a peça que falta* e *o nível que falta*.

> **A fase 7 dropava "todos os tipos" e isso foi revogado.** Ela existia pra sustentar o sacrifício do
> mesmo conjunto (§A FORJA): a Arma do Reino só cai na Reino 1-1, e *"a 1-1 é fácil demais pra ser
> fonte de mítico"*. **Aquela justificativa estava errada** — quem gateia raridade é a **dificuldade**,
> não a fase. A Reino 1-1 no **Pesadelo** é fonte legítima de mítico, e o sacrifício se alimenta lá.
> Com a variedade fora da 7, ela deixa de ser a única fase que importa.

**A raridade é sorteada direto na faixa da dificuldade** (`GDD-progressao.md` §O TETO DE DIFICULDADE):
Fácil até raro · Normal até épico · Difícil até lendário · Pesadelo até mítico. **Qualquer raridade
abaixo do teto cai** — no Pesadelo cai de mítico a comum; mítico é só muito mais raro.

> **A curva tem UM botão.** Peso geométrico `r^(raridade−1)`, truncado no teto da dificuldade e
> renormalizado — um único número calibra a economia inteira.
>
> **E esse número CAI em relação ao chute antigo**, porque o mítico deixou de ser só o topo do drop:
> ele é o combustível do reforge (§A forja de míticos). Quanto, é um dos números que faltam.

### A EVOLUÇÃO — a segunda rota, e por que ela existe

**Drop e evolução não são rotas redundantes, são moedas diferentes:**

| | o que custa | o que entrega |
|---|---|---|
| **drop** | sorte | a **raridade**, agora e sem controle |
| **evolução** | tempo + sacrifício | a raridade **depois**, com as subestatísticas escolhidas |

É isso que faz o apego valer a pena: *"venho jogando com ela desde o início"* deixa de ser
sentimentalismo e vira a decisão correta.

> **Por que o drop de raridade alta NÃO vira lixo**, mesmo com a evolução dando escolha: são dezenas de
> apóstolos pra vestir, e **ninguém vai evoluir 7 itens × dezenas de apóstolos desde o comum**. A
> evolução é pros poucos itens que se carrega; o drop mítico veste todo o resto do elenco. **Por isso
> as subestatísticas são IGUAIS nos dois caminhos** — não há privilégio de nascença pro drop; o que
> segura a fantasia dele é a escassez de tempo do outro lado.
>
> **E nenhuma das duas rotas domina, porque o que a evolução entrega é o CAMINHO, não o destino:** quem
> evoluiu escolheu 1 entre 3 em **cada degrau**, gastando material barato; quem dropou chegou pronto na
> raridade e precisa do **reforge de míticos** pra consertar as subs, que é o mais caro do jogo.

**UMA TRILHA SÓ, e ela é o NÍVEL.** A raridade não tem barra — vem do sacrifício, que é um ato. Duas
trilhas que enchem juntas, da mesma fonte, sem escolha entre elas, seriam uma trilha só desenhada duas
vezes.

**O nível é do ITEM, não do apóstolo** — ele viaja quando o item troca de dono. Trocar de portador pausa
o progresso, nunca destrói.

#### Como o nível sobe — e por que a rodada vale mais onde dói

O nível é de **uso**: ganha por rodada de combate, com o item equipado em alguém em campo. **E o que
uma rodada rende depende da dificuldade:**

```
ponto por RODADA      Fácil 1  ·  Normal 2  ·  Difícil 3  ·  Pesadelo 4

teto por batalha      arrastar a luta não paga
derrota = o acumulado · vitória = o acumulado + BÔNUS fixo
```

**A curva de custo por nível está em aberto** — a intenção é dobrar a cada faixa de 10, o que preserva
"o primeiro degrau sai na primeira sessão, o último é o mais caro do jogo". O número está em
`GDD-progressao.md` §Os números que faltam.

> **O ponto por dificuldade fecha um furo que o teto por batalha não pega.** O teto impede *arrastar* a
> luta; não impede *repetir a luta curta*. Se a rodada valesse igual em todo lugar, o jeito ótimo de
> subir item seria **repetir a Fácil 1-1**, que acaba em duas rodadas. Com o ponto escalando, jogar
> onde é difícil é sempre melhor — **e é o mesmo princípio da XP**, que também paga mais na fase dura.

**E a cada dezena vem o PEDÁGIO** (`GDD-progressao.md` §O PEDÁGIO): material da faixa atual, mais um
pouco da próxima. Ele não é imposto — cada dezena paga **+15 pontos de principal**, que é a maior
compra do item. E como o material que mais acelera nível é o mesmo que o pedágio cobra, queimá-lo como
atalho **rouba do próprio teto**: nenhuma trava anti-abuso precisa ser escrita.

**A ⚒️ BIGORNA é onde essa queima acontece** (a Forja, ago/2026): pó vira ponto na escada
1·5·25·125·625·3.125, com o mesmo painel de barras da queima de alma. **Ela RECUSA na parede** — não
como trava anti-abuso (o parágrafo acima explica por que ela não é necessária), mas porque ali o
ponto é DESCARTADO (`GDD-progressao.md` §A ESTRELA): quem malha travado destrói o pó que a têmpera
vai cobrar e não anda um degrau. É a mesma recusa que o Santuário faz do lado do apóstolo.

**Os primeiros níveis têm de sair rápido**, e é o que impede a coisa de ser maçante: você vê número
subir na primeira sessão, e como o nível é **por peça** com 9 slots, sempre há algo prestes a estourar.
A dezena cara só aparece depois que você já escolheu o item que quer levar até o fim.

> **Contar rodada, não ação do portador.** Com a barra de velocidade (§1), um apóstolo rápido age o
> dobro — se ele também evoluísse equipamento ao dobro, a Velocidade viraria duplamente dominante.
> **Rodada é a unidade que não acopla.**
>
> O teto por batalha e o bônus de vitória fecham as frestas de exploit (arrastar a luta, perder de
> propósito) **sem** tirar o progresso de quem tentou a fase acima do nível e perdeu — que é o que faz
> o jogador continuar arriscando.

#### Por que evoluir NÃO atropela o drop

Cada eixo tem fonte própria, e **nenhum dos dois é exclusivo de um caminho só**:

| eixo | vem do DROP? | vem de JOGAR? | vem de PAGAR? |
|---|:-:|:-:|:-:|
| **raridade** | ✅ sorteada na faixa da dificuldade | — | ✅ sacrifício + material |
| **nível** | — todo item cai no nível 1 | ✅ o uso | ✅ o material acelera |

**A raridade o drop também dá.** No Fácil você chega a raro evoluindo — e um raro pode cair pronto ali
do lado. Evoluir **não é o caminho superior; é o caminho para o item que você quer manter.** Quem só
quer status veste o que caiu.

**O que a evolução dá com exclusividade são as SUBS ESCOLHIDAS.** O drop entrega as dele sorteadas, e
consertá-las custa o reforge. É pouco o bastante pra a caixa de surpresa continuar valendo, e o
bastante pra o item antigo ter alma — que é o *"o mais fraco sempre pode virar o mais forte"* sem matar
o drop.

#### O ESMERIL — a peça vira pó *(nomeado em ago/2026)*

**⚙️ Esmeril é a bancada que DESFAZ.** O rebolo é a única máquina da forja que produz pó de verdade —
faísca e limalha —, e é a única bancada que destrói: as outras três gastam material, esta gasta a
peça. Ela existe porque a fase larga 4 peças e ninguém veste 4: sem um destino, o drop vira lixo.

> **Por que "Esmeril" e não "Refusão" ou "Reforja".** *Refusão* vive do mesmo radical de FUNDIR, que já
> é o 10:1 do material (Caldeamento e Oferenda); *Reforja* descreve desfazer-pra-refazer, que é esta
> bancada e não a de raridade. E o Esmeril emparelha com a **Bigorna**: a forja fica com duas
> MÁQUINAS (bigorna, esmeril) e dois atos de FOGO (têmpera, caldeamento). O "Cadinho" foi recusado
> por ser o vaso ONDE a coisa acontece — o esmeril é a máquina que FAZ, como a bigorna.

**O que devolve, pela faixa da peça** — e devolve na PRÓPRIA faixa, que é o que faz uma peça rara
valer algo mesmo sem ser vestida:

| faixa da peça | devolve | vale em pontos |
|---|---:|---:|
| Comum | 5 | 5 |
| Incomum | 4 | 20 |
| Raro | 3 | 75 |
| Épico | 2 | 250 |
| Lendário | 1 | 625 |
| Mítico | 1 | 3.125 |

**A quantidade CAI conforme a faixa sobe** porque a escada de valor já multiplica por 5 a cada degrau
(1 · 5 · 25 · 125 · 625 · 3.125): contagem fixa faria um mítico pagar 3.125 pontos, metade da curva
1→60 de uma peça inteira.

> **Moer um mítico é permitido, e é questão de NÚMERO e não de estrutura** *(decisão do Gabriel,
> ago/2026)*: o esmeril compete com a ⚗️ Amálgama pelo mesmo item, e se moer pagar bem demais quem
> muda é a quantidade da tabela. Houve uma versão em que o mítico devolvia lendário — ela morreu por
> criar exceção onde cabia um número.

**Os `Pontos` NÃO voltam.** Devolver o nível investido faria o esmeril transferir progresso de uma
peça pra outra de graça, e a peça upada viraria banco. Perder o nível ao moer é também o que dá
motivo pra não subir lixo.

**Peça VESTIDA não entra no esmeril** — ele não desnuda apóstolo por conta própria. Tirar é o
✕ Remover da Armaria, e é uma decisão à parte.

**Calibragem:** uma fase no Fácil já derruba 260 pontos de pó; as 4 peças dela passadas no esmeril
somam 20–60. **O Esmeril é bônus, não torneira**, e é isso que ele tem de continuar sendo.

#### A AMÁLGAMA — o custo, e a escolha das subestatísticas

**⚗️ Amálgama é a bancada que FUNDE peças numa melhor** *(nomeada em ago/2026)*. Amálgama é liga
metalúrgica: muitos viram um **sem apagar o que entrou** — que é exatamente a mecânica, já que as subs
das peças sacrificadas viram o pool de 3 opções. *Fusão* seria o 10:1 do material outra vez, e
*Refino* diria "subiu de qualidade" sem dizer com o quê.

Subir a raridade custa **sacrificar outros itens mais material**, e o sacrifício é o que decide as
opções:

- **Consome o MESMO item, do MESMO CONJUNTO, na raridade ATUAL** — a arma do Reino sacrifica armas do
  Reino —, mais **material: muito da raridade atual, pouco da alvo**. Sobe um degrau, e o degrau vale
  **uma sub a mais** — nada de nível, que é outro eixo. Pedir a peça já na raridade alvo mataria a
  razão de existir da forja: quem tem épico não precisa fabricar épico.
- **O "pouco da raridade ALVO" é o que trava a dificuldade.** Sem ele, o Fácil fabricaria épico só
  sacrificando raros. Não é decoração da receita — é o portão.

> **É essa regra que decide ONDE se farma**, e ela aperta por dois lados ao mesmo tempo: o material tem
> de ser a mesma PEÇA e do mesmo CAPÍTULO. Evoluir a arma do Reino manda você de volta à **Reino
> 1-1**, que é a única fonte de arma do Reino (§O drop).
>
> **E o preço não é a fase, é a DIFICULDADE.** Chegou-se a escrever que *"a 1-1 é fácil demais pra ser
> fonte de mítico"*, e foi por isso que a fase 7 ganhou a variedade — **a justificativa estava errada**:
> quem gateia raridade é a dificuldade, e a Reino 1-1 no **Pesadelo** é fonte legítima de mítico. Fase
> curta não é fase barata.

- **As subestatísticas dos sacrificados viram o POOL**: a forja oferece **3 opções** e você escolhe 1.
  Se o pool não tiver o bastante, completa com aleatórias dentro das regras de sub. **O pool pode
  oferecer sub que o item JÁ tem** — é o que torna a concentração comprável em vez de sorteada.
- **Dá pra RECUSAR.** Recusar mantém o item exatamente como estava e **queima o material do mesmo
  jeito** — tenta-se de novo com outro sacrifício, até vir a sub desejada. É caro de propósito.
- **A recusa é por SLOT, dentro da mesma leva.** Paga-se uma vez e decide-se slot a slot "troco" ou
  "mantenho". Manter tudo é o fracasso completo (material queimado, item igual), sem obrigar ninguém a
  aceitar uma sub pior — tudo-ou-nada só acrescentaria arrependimento, não dificuldade.

> **O gasto acontece ao ABRIR a forja, não ao aceitar — e a tela tem de dizer isso ANTES do clique.**
> Sem esse aviso, a primeira recusa lê como golpe em vez de aposta, e a mecânica fica com fama de
> trapaça.
>
> **Montar o sacrifício direcionado é FEATURE, não furo.** Quem guarda peças com a sub que quer, pra
> forçar as opções, está dominando o sistema — é craft direcionado, como no PoE. Não bloquear quando
> aparecer. Pra isso funcionar, **o arsenal precisa mostrar as subs de cada item**.

**E é o sacrifício que entrega a fantasia inteira do sistema.** Aquela arma comum da primeira hora de
jogo só vira mítica quando você volta à **fase de onde ela veio**, na dificuldade que dropa mítico:
*"a arma que eu venho jogando desde o início"* deixa de ser sentimento e vira algo que o jogo mede —
você provou que domina o lugar de onde ela veio. **Nenhum item vira lixo**, porque toda peça é
matéria-prima da sua igual.

**O `N` de uma tentativa sobe junto com o drop.** Com **4 itens por fase**, o chute antigo de 2–3 peças
por degrau barateia a promoção em quatro vezes — o número tem de acompanhar, e ele está na lista dos
que faltam. O critério não muda: quem gasta dez vezes o `N` gastou **por escolha**, perseguindo a sub, e
aposta voluntária é conteúdo enquanto obrigação é fazenda. **Se o número obrigar a repetir fase curta,
virou imposto — baixar.**

> **Efeito colateral a vigiar:** com mais volume E mais variedade, a fase 7 domina e as fases 1–6 viram
> passagem única. Normal em ARPG, e não é defeito. Se um dia quiser as outras vivas, a saída barata é
> **fase 7 = volume e variedade; cada fase = o item dela, mais rápido** — dois motivos diferentes de
> repetir, sem regra nova.

#### A forja de míticos — o reforge, e o endgame da peça

Mítico em cima de mítico não sobe raridade (não há acima). Pelo mesmo custo, o **reforge re-sorteia as
5 subs**, com as mesmas 3 opções e a mesma recusa por slot da promoção. Nenhuma mecânica nova: é a
forja de sempre, apontada pra um item que já chegou no topo dos dois eixos.

**É ele que segura o endgame de pé depois do mítico 60**, e é o que torna a concentração — cinco cópias
da mesma sub — uma coisa que se persegue em vez de uma coisa que acontece.

**E é aqui que nasce a primeira decisão de recurso de verdade da economia:** o mítico dropado passa a
ter destinos que **competem** — vestir mais um apóstolo da frota, virar combustível do reforge, ou
pagar a promoção de outra peça. **Por isso a taxa de drop do mítico cai:** ele deixou de ser só troféu.

### O ACERVO — a tela por apóstolo, e o filtro *(desenho do Gabriel, ago/2026)*

**A Forja é POR APÓSTOLO, e é assim que se entra nela:** clica no apóstolo, clica em Forja, e a tela
mostra **os itens DELE** — cada apóstolo com os seus, nos 7 slots. Clicar num slot (a Arma, digamos)
abre **todas as armas disponíveis** do acervo pra escolher qual vestir.

> **É a consequência direta do item ser CUMULATIVO.** Com uma peça por (facção, fase) a lista cabia
> numa tela e o filtro era desnecessário. Com 4 drops por fase, 56 fases e 4 dificuldades, o acervo
> passa de mil peças — e escolher vira um problema de BUSCA, não de leitura.

**O FILTRO tem de ser completo.** Os eixos, todos combináveis:

| filtra por | o que faz |
|---|---|
| **nível** · **estrela** | a magnitude da peça |
| **raridade** | quantas subs ela tem |
| **facção** | de que conjunto ela é |
| **stat principal** | qual dos principais do slot ela sorteou |
| **subs, CONTENDO** | só as que têm aquela sub específica |
| **subs, SEM CONTER** | só as que NÃO têm aquela sub |
| **equipadas** | mostra também as que estão **vestidas em OUTROS apóstolos**, pra tomar a peça de um aliado |

#### O que o item POR APÓSTOLO muda nas telas *(desenho do Gabriel, ago/2026 — ✅ no ar)*

Até ago/2026 os itens eram globais e as telas mentiam um pouco por causa disso. Com o vínculo (o
passo 10-b1), três coisas mudaram juntas:

- **As setas `‹ Arma ›` da Forja passam a percorrer os itens DAQUELE apóstolo**, não o acervo do
  mundo. O gesto é o mesmo; o que ele traz é que muda.
- **A peça carrega um emoji pequeno do apóstolo num canto** — é o que responde "de quem é isso?" sem
  abrir nada. Vale no cartão do acervo e na peça do centro.
- **O filtro de EQUIPADAS existe pra roubar de aliado.** Vestir em B uma peça que está em A tira ela
  de A — isso o modelo já permite —, e **é justamente por isso que o emoji do portador é obrigatório
  ali**: sem ele o jogador desnuda um aliado sem perceber, e só descobre na fase seguinte. A tela tem
  de dizer DE QUEM está tirando antes do clique, não depois.

**E um filtro que ORDENA por stat somado, que é o que responde a pergunta real do jogador:** "qual
peça me dá mais ATAQUE?" — e a conta soma **o principal com as subs juntos**, não só o principal.
Escolhidos os stats que interessam, a lista sai pela que MAIS aumenta aquilo. **Também na ordem
inversa** (a que menos dá), pra quem está procurando o que sacrificar.

**Os conjuntos saem SEPARADOS POR FACÇÃO por padrão, nunca misturados** — é o que preserva a leitura
de conjunto, que é a razão de a facção existir no item. **Mas há um botão que MISTURA TUDO**, e ele é
parte do filtro: quem está atrás do maior (ou do menor) de um stat, sem se importar com conjunto,
liga o botão e a separação some. É preferência do jogador, não regra do jogo — por isso é botão, e
não decisão nossa.

> **O LAYOUT NÃO PODE PASSAR DA TELA.** Filtro completo é fácil de transformar em painel que estoura
> a janela e obriga a rolar — e aí ele fica feio e ninguém usa. O filtro cabe na tela, ou ele foi
> desenhado errado.

**Quando isto entra:** a tela por apóstolo é o PR do *"item vai pro apóstolo"*; os eixos de raridade e
sub do filtro só existem depois que raridade e subs existirem. O filtro de nível, estrela, facção e
principal já é implementável no primeiro dos dois.

### AS DUNGEONS — a fonte dos acessórios

**Duas dungeons**, uma por acessório (colar, pulseira). É o que leva o conjunto a 9 peças.

> **O SLOT dos dois acessórios destrava pela ESTRELA do apóstolo** — o primeiro em **4★** (nível 40),
> o segundo em **6★** (nível 60). É a única exceção ao "todo slot nasce aberto", e ela se justifica
> porque acessório não cai na campanha (§OS 9 SLOTS).

- **Escolhe-se a FACÇÃO na entrada**, e essa escolha define **o conjunto que dropa** e **a luta que se
  enfrenta**. Deixa de ser menu e vira decisão.
- Cada facção traz **buff no boss e/ou debuff no jogador**, combináveis — mais variedade sem inflar um
  boss só. **Não removíveis.**
- **As mesmas 4 dificuldades** da campanha, **desbloqueadas pelo capítulo** correspondente, e o jogador
  escolhe qual enfrentar.
- **Os modificadores são revelados na tela de pick**, antes de entrar.
- **O sacrifício do acessório é a própria dungeon** — pra promover o colar do Reino você volta à dungeon
  do Reino, na dificuldade que dropa a raridade de que precisa. A mesma regra dos outros itens, com
  "fase" trocada por "dungeon".

> **O motor já suporta o "não removível": custo ZERO.** `StatusEffect.Removivel` existe
