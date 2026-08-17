# GDD — ITENS: slots, subestatísticas, escala e evolução (§4)

> **Tipo:** MODELO (referência viva), e o passo mais DISTANTE da fila — item equipado é o 7º e
>   último item do §7, e as subs/aprimoramento estão no DEPOIS.
> **Função:** os três eixos do item, os 9 slots e o que cada um paga, as 8 subs, a escala pela
>   estrela, o drop, a evolução e as dungeons.
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

> **O PRINCÍPIO, e ele é do Gabriel:** *"o mais fraco sempre pode virar o mais forte"*. O comum 1★ que
> caiu na primeira fase pode terminar mítico 6★ com as subestatísticas escolhidas a dedo. **O preço de
> manter isso saudável é o CUSTO:** o caminho do fraco tem de ser mais caro que vestir o forte já
> pronto, senão ninguém quer o forte. Toda trava desta seção existe por causa disso.

- **Equipados no APÓSTOLO**, não mais no jogador. É o que os torna valiosos — e é a mudança de maior
  impacto no save.
- **CONJUNTOS de 9 peças**, com bônus em **3 / 6 / 9**. E o que cada conjunto faz cresce com a
  raridade. *(O que cada conjunto FAZ ainda não foi desenhado.)*

> **O número ímpar deixou de ser problema.** O GDD antigo remendava com bônus em 2/4/6 e o acessório
> **fora** do conjunto — sobrava uma peça sem função. Com os **2 acessórios das dungeons**, são 9
> peças, três degraus iguais entre si, e **toda peça conta**.

### Os três eixos do item — e cada um com sua fonte de custo

| eixo | o que dá | como sobe | o que custa |
|---|---|---|---|
| **raridade** | QUANTAS subestatísticas (+ teto de aprimoramento) | **nível** (lento) + marco da fase + forja | sacrificar itens da raridade atual |
| **aprimoramento** | QUÃO BOAS elas são — `+0…+20` | forja, a qualquer momento | material / moeda |
| **estrela** | MAGNITUDE (principal e subs) | **nível** (rápido) + marco da fase | tempo de jogo |

> **NOMENCLATURA — e ela foi corrigida (ago/2026), porque estava invertida.** O que sobe **jogando** é
> o **NÍVEL**, igual ao apóstolo: os dois ganham nível usando, e é essa simetria que faz o nome ser
> esse. O que sobe **pagando** na forja é o **APRIMORAMENTO** (`+0…+20`), e é ele que aparece como
> `(1)`…`(5)` ao lado de cada sub. Antes o doc chamava a forja de "nível" e o uso de "barra de uso" —
> **trocado**, e a troca confundia porque nível do item e nível do apóstolo funcionavam ao contrário.

> **UM NÍVEL SÓ, e ele dá a estrela.** A raridade não tem trilha própria — ela vem de ATOS (sacrifício
> + marco + `raridade ≤ estrela`). Ver §A EVOLUÇÃO.

> **O marco é o mesmo para os dois** — vencer a fase de origem do item naquela dificuldade destrava
> estrela **e** raridade, no teto daquela dificuldade. Uma condição, dois eixos.

> **A moeda que ficou parqueada já tem dono: é o aprimoramento.** Não é preciso procurar função pra ela.

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

| slot | | forma | principal — e o valor no **6★ +20** |
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
- **Não repete no mesmo item** — além do teto, é o que faz a forja compor quatro coisas diferentes em
  vez de empilhar a mesma.
- **Valores FIXOS**, sem variação de rolagem. Não é só "somos offline, não gacha": a variação
  **brigaria com a forja**. Pagar caro, recusar três vezes, escolher a sub que queria — e ela vir no
  mínimo? A escolha comprada seria anulada por um dado. A variação em gacha existe pra criar uma
  camada extra de farm; a forja **é** essa camada, e é conteúdo.
- **A Velocidade é em valor cheio**, nunca %. Em % o Atirador (base 115) ganharia mais velocidade
  absoluta que o Guardião (base 90) com a mesma sub — o rápido ficaria mais rápido e a faixa explodiria
  sozinha.

**O teto, e ele CALCULA o valor da sub:** uma sub totalmente aprimorada são **6 rolagens** (a inicial +
5 aprimoramentos). Se ela tem de ficar abaixo do principal no topo:

```
rolagem de sub  =  principal (6★ +20) ÷ 7
```

Escolhe-se **um** número — quanto o principal dá no topo — e o da sub sai por divisão, já obedecendo
"principal > sub" por construção, em todo stat.

**A estrela multiplica principal e sub juntos**, então a hierarquia se mantém em qualquer estrela sem
regra extra: `valor = base(stat) × fator(★) × (1 + aprimoramentos)`.

> **SEM restrição temática por slot** (arma podendo ter `DEF%`, etc.). Ela existe em gacha pra afunilar
> **volume** — o jogador abre centenas de peças e precisa descartar. Aqui o volume não existe (9 slots
> × 8 conjuntos, drop garantido) e **a forja conserta a sub**: o item "errado" é matéria-prima, não
> lixo. O custo dela seria fechar builds legítimas por estética. **E a assimetria decide:**
> acrescentar depois é barato; tirar depois deixa todo item já dropado com um perfil sem sentido.
>
> **A alavanca, se ficar sem sabor:** em vez de PROIBIR `DEF%` na arma, dar a ela **peso menor no
> sorteio**. Troca uma lista de proibições por uma tabela de pesos — dado, não regra.

**O acúmulo tem freio, e ele já estava no desenho.** Um boneco full mítico tem `9 × (4 subs + 5
aprimoramentos)` rolagens, e a tentação é achar que o jogador empilha tudo num stat só. **Ele não
consegue:** a forja escolhe *quais subs existem*, mas **quem escolhe o slot do aprimoramento é o RNG**.
Os 5 aprimoramentos se espalham entre as 4 subs (~1,25 cada); concentrar os 5 numa é ~0,1%.

### Raridade → quantas subs, e até onde o APRIMORAMENTO vai

| raridade | subs | aprimoramento máx | unidades de sub |
|---|---|---|---|
| comum | 0 | **+4** | 0 |
| incomum | 1 | **+9** | 1 |
| raro | 2 | **+14** | 2 |
| épico | 3 | **+19** | 3 |
| lendário | 4 | **+20** | 4 |
| mítico | 4 | +20 | **5** *(a 5ª é o bônus de nascença)* |

**Cada degrau de raridade vale exatamente +1 sub e +5 de aprimoramento.** Escada regular, legível pro
jogador, barata de calibrar — e o mítico é o único que foge (mesmas subs do lendário, só a unidade a
mais), o que faz o último degrau ser especial **sem regra escrita pra isso**.

> **Por que o teto de aprimoramento é preso à raridade — a correção é do Gabriel.** Sem isso os dois
> fazem a MESMA coisa: um comum a `+20` acabaria com as mesmas 4 subs de um mítico. **Prendendo os
> dois, cada eixo vira uma frase:** raridade = quantas · aprimoramento = quão boas.
>
> **De quebra, um comum é um item cru de verdade** — trava em `+4`, sem sub nenhuma, valendo só o
> atributo principal. É o item que se usa e se joga fora, que é o que um comum deve ser.

### Nível → aprimoramento

**Até 20 (limitado pela raridade). A cada 5 níveis aprimora**: enquanto houver menos de 4
subestatísticas, acrescenta uma; com 4, incrementa uma das existentes — **sorteada**, mostrando
`(1)` / `(2)` ao lado e o valor que ficou.

**O aprimoramento é do SLOT, não da subestatística.** Trocar a sub de um slot não zera o aprimoramento
dele. Se zerasse, a sub que veio aprimorada viraria intocável por acidente — ninguém a trocaria nunca
— e um quarto da forja de míticos morreria em silêncio.

> **O que o endgame realmente caça:** a DISTRIBUIÇÃO dos aprimoramentos entre os 4 slots é sorteada, e
> reforjar não a redistribui. Um `(5,0,0,0)` é um achado; reforjar deixa você pôr a sub que quiser no
> slot gordo. Quem escolhe o slot é o RNG — **decisão fechada**.

### A ESCALA — a estrela dá magnitude, o aprimoramento dá subs

Os valores da tabela dos 9 slots são o **teto: 6★ +20**. Daí pra baixo:

```
principal  =  MÁXIMO  × fatorEstrela                              só a ESTRELA
sub        =  UNIDADE × fatorEstrela × aprimoramentos no slot

fatorEstrela   1★ 25%   2★ 40%   3★ 55%   4★ 70%   5★ 85%   6★ 100%
```

**O APRIMORAMENTO NÃO TOCA O PRINCIPAL** — e isso é o eixo fazendo o que a tabela dos três eixos já
dizia: magnitude é da estrela, o aprimoramento é *"quão boas são as SUBS"*. Uma versão anterior desta
seção pôs um `fatorNível` multiplicando o principal (40%→100%); estava errado, e o efeito era a forja
fazer o trabalho da estrela.

**Um `6★ +0` e um `6★ +20` têm o MESMO principal.** O que os separa são as 5 unidades de sub.

**A raridade não multiplica nada** — ela **trava** o aprimoramento, e com ele a contagem de subs:

| raridade | teto de aprimoramento | unidades de sub |
|---|--:|--:|
| comum | +4 | 0 |
| incomum | +9 | 1 |
| raro | +14 | 2 |
| épico | +19 | 3 |
| lendário | +20 | 4 |
| mítico | +20 | **5** |

> **Unidade de sub = aprimoramento**, um pra um. O `(5)` do mítico é a 5ª unidade caindo toda no mesmo
> slot — e é **a única coisa** que separa um mítico de um lendário, já que o principal empata nos dois
> em qualquer estrela. **Esse bônus não cai no drop, só se conquista evoluindo**, e é ele que dá razão
> pra rota do item fraco existir (§A EVOLUÇÃO).

**A TRAVA: a sub escala pela ESTRELA, junto com o principal.** O aprimoramento só acrescenta unidades;
quem muda o valor de cada unidade é a estrela — a mesma que move o principal. Por isso a razão entre os
dois é **fixa em qualquer estrela**:

```
              principal    sub máx (5 un.)    razão
6★               100%           50%            50%
4★                70%           35%            50%
1★                25%          12,5%           50%
```

**A sub nunca passa de METADE do principal**, e isso não é calibragem — é consequência de as duas
escalarem pelo mesmo fator. Se a sub escalasse pelo aprimoramento, um item `+20` de estrela baixa teria
sub maior que principal, e a peça deixaria de ter dono.

**A grade do principal** (para um máximo de 100%; os outros stats são esta coluna vezes o máximo deles):

| 1★ | 2★ | 3★ | 4★ | 5★ | 6★ |
|--:|--:|--:|--:|--:|--:|
| 25,00 | 40,00 | 55,00 | 70,00 | 85,00 | **100,00** |

> **A ficha mostra DUAS CASAS DECIMAIS** (decisão do Gabriel). Os números não fecham redondo — com 9
> stats e dois eixos, qualquer rampa produz decimal em algum cruzamento — e **esconder a casa é pior
> que mostrá-la**: é o defeito conhecido do Raid, onde a ficha arredonda e o jogador lê `100%` estando
> em 99,6%. Como a Taxa Crítica é o único stat com teto duro e a build inteira gira em torno de fechar
> exatamente esse teto, arredondar esconderia justo o número que precisa ser conferido.

**A jornada de uma peça, do drop ao topo** — uma Manopla de Dano Crítico (máximo 100%):

| etapa | principal | subs | o que mudou |
|---|--:|---|---|
| 1★ comum +0 | 25,00% | — | o drop |
| 1★ comum +4 | 25,00% | — | **o aprimoramento não moveu nada**: comum não tem sub |
| 3★ comum +4 | 55,00% | — | duas estrelas — e é a estrela que dobra o principal |
| 3★ raro +14 | 55,00% | Taxa (1) 2,75% · ATK% (1) 2,75% | a raridade abriu 2 subs; o principal ficou parado |
| 5★ épico +19 | 85,00% | Taxa 4,25% · ATK% 4,25% · Vel 4,25 | **as subs subiram sem aprimoramento** — só pela estrela |
| 6★ lendário +20 | 100,00% | Taxa 5% · ATK% 5% · Vel 5 · Prec 13 | principal no teto |
| 6★ mítico +20 | 100,00% | **Taxa (2) 10%** · ATK% 5% · Vel 5 · Prec 13 | só a 5ª unidade |

**O principal cresce 4× do drop ao topo, e quem faz isso é a ESTRELA sozinha.** O aprimoramento entrega
outra coisa: de zero a cinco unidades de sub. As duas linhas em que o principal não se move (`+4` e
`+14`) são o desenho aparecendo — **forja não compra magnitude**.

### O drop

- **Item sempre cai**, mas a estrela varia: 1–3★ Fácil · 2–4★ Normal · 3–5★ Difícil · 4–6★ Pesadelo.
- **O item é FIXO por fase**, como já é hoje — a arma na 1-1, e cada fase com a peça dela. O que varia
  no drop é estrela e raridade, não qual item cai.
- **CADA FASE DROPA SÓ O SLOT DELA — inclusive a 7.** Sete fases, sete peças de armadura: encaixe
  exato, e **as sete ficam vivas o jogo inteiro**. Quem quer Manopla vai na 4, e não há atalho.
- **O que a fase difícil paga é XP, não variedade.** A quantidade cresce ao longo do capítulo e a
  fase 7 dá mais XP — a 8-7 é a que mais dá no jogo. **É esse o motivo de farmar a fase difícil**, e
  ele não compete com o item: você escolhe entre *a peça que falta* e *o nível que falta*.

> **A fase 7 dropava "todos os tipos" e isso foi revogado.** Ela existia pra sustentar o sacrifício do
> mesmo conjunto (§A FORJA): a Arma do Reino só cai na Reino 1-1, e *"a 1-1 é fácil demais pra ser
> fonte de mítico"*. **Aquela justificativa estava errada** — quem gateia raridade é a **dificuldade**,
> não a fase (ver a tabela de estrelas acima). A Reino 1-1 no **Pesadelo** é fonte legítima de mítico,
> e o sacrifício se alimenta lá. Com a variedade fora da 7, ela deixa de ser a única fase que importa.
- **A raridade pode ser QUALQUER uma, em qualquer dificuldade** — o que muda é a CHANCE. No Pesadelo
  cai de mítico a comum; mítico é só muito mais raro.
- **A única trava do drop é a estrela: `raridade ≤ estrela`** (comum 1 … mítico 6).

**Sorteia a estrela pela dificuldade, depois a raridade em `[comum .. estrela]`.** Duas linhas, nenhuma
tabela — e a dificuldade continua regulando a raridade, de graça, através da estrela:

| dificuldade | estrelas | raridade possível | por quê |
|---|---|---|---|
| Fácil | 1–3★ | comum → raro | 3★ não alcança épico |
| Normal | 2–4★ | comum → épico | |
| Difícil | 3–5★ | comum → lendário | |
| Pesadelo | 4–6★ | comum → mítico | só o 6★ abre o mítico |

> **A curva tem UM botão.** Peso geométrico `r^(raridade−1)`, truncado na estrela e renormalizado. Com
> `r = 1/2`, um 6★ dá comum 51% e mítico 1,6% — e como o 6★ é 1 em 3 dos drops do Pesadelo, mítico sai
> a ~0,5% por drop. Com `r = 0,6`, ~1,1%. **Um único número calibra a economia inteira**, e o
> truncamento faz a estrela alta ser desejada duas vezes: status **e** acesso à cauda rara.

### A EVOLUÇÃO — a segunda rota, e por que ela existe

**Drop e evolução não são rotas redundantes, são moedas diferentes:**

| | o que custa | o que entrega |
|---|---|---|
| **drop** | sorte | **agora**, sem controle |
| **evolução** | tempo + sacrifício | **depois**, com as subestatísticas escolhidas |

É isso que faz o apego valer a pena: *"venho jogando com ela desde o início"* deixa de ser
sentimentalismo e vira a decisão correta.

> **Por que o drop de raridade alta NÃO vira lixo**, mesmo com a evolução dando escolha: são dezenas de
> apóstolos pra vestir, e **ninguém vai evoluir 7 itens × dezenas de apóstolos desde 1★**. A evolução é
> pros poucos itens que se carrega; o drop mítico veste todo o resto do elenco. **Por isso as
> subestatísticas são IGUAIS nos dois caminhos** — não há privilégio de nascença pro drop; o que
> segura a fantasia dele é a escassez de tempo do outro lado.
>
> **E nenhuma das duas rotas domina, porque o que a evolução entrega é o CAMINHO, não o destino:** quem
> evoluiu escolheu 1 entre 3 em **cada degrau**, gastando raro/épico/lendário (barato); quem dropou
> chegou cedo com 4 subs aleatórias e precisa da forja de **míticos** pra consertar cada uma (caro).

**UM NÍVEL SÓ, e ele entrega a ESTRELA.** A raridade **não tem trilha própria** — ela é destravada por
ATOS: o sacrifício na forja, o marco da fase, e a trava `raridade ≤ estrela`. Três condições, nenhuma
delas uma espera.

> **A versão anterior tinha DUAS barras** e o próprio doc dava o argumento contra: *"não é escolha de
> investimento — como a estrela é o teto da raridade, investir em raridade primeiro seria sempre
> errado"*. **Duas trilhas que enchem juntas, da mesma fonte, sem escolha entre elas, são uma trilha
> só**; a segunda existia pra ser lida na tela.
>
> **E a simetria com o apóstolo fecha:** ele sobe de nível **jogando** e sobe de raridade por **missão**
> — um ato, não uma barra. O item faz igual: nível jogando, raridade por sacrifício e marco.

**O nível é do ITEM, não do apóstolo** — ele viaja quando o item troca de dono. Trocar de portador pausa
o progresso, nunca destrói.

#### Como o nível sobe — e por que a rodada vale mais onde dói

O nível é de **uso**: ganha por rodada de combate, com o item equipado em alguém em campo. **E o que
uma rodada rende depende da dificuldade:**

```
ponto por RODADA      Fácil 1  ·  Normal 2  ·  Difícil 3  ·  Pesadelo 4

custo da estrela      1★→2★    100      dobra a cada degrau
                      2★→3★    200
                      3★→4★    400
                      4★→5★    800
                      5★→6★  1.600

teto por batalha      arrastar a luta não paga
derrota = o acumulado · vitória = o acumulado + BÔNUS fixo
```

> **O ponto por dificuldade fecha um furo que o teto por batalha não pega.** O teto impede *arrastar*
> a luta; não impede *repetir a luta curta*. Se a rodada valesse igual em todo lugar, o jeito ótimo de
> estrelar item seria **repetir a Fácil 1-1**, que acaba em duas rodadas. Com o ponto escalando, jogar
> onde é difícil é sempre melhor — **e é o mesmo princípio da XP**, que também paga mais na fase dura.

**Quanto cada teto custa de uma passada** (56 fases × ~12 rodadas = 672 rodadas):

| dificuldade | teto | custo do degrau | pontos na passada | % da passada |
|---|:-:|--:|--:|--:|
| Fácil | 3★ | 300 | 672 | 45% |
| Normal | 4★ | 400 | 1.344 | 30% |
| Difícil | 5★ | 800 | 2.016 | 40% |
| Pesadelo | 6★ | 1.600 | 2.688 | 60% |

**Nenhum teto exige repetir fase, e nenhum sai de graça** — o mais barato custa 30% de uma passada, o
mais caro 60%. O último degrau, que é o mais caro do jogo, cabe na metade do Pesadelo: dá pra terminar
a dificuldade **e** estrelar o conjunto que você escolheu levar.

**E os dois primeiros degraus saem em 2 e 3 fases**, o que é o que impede a coisa de ser maçante: você
vê estrela subir na primeira sessão, e como o nível é **por peça** com 9 slots, sempre há algo prestes
a estourar. O degrau caro só aparece depois que você já escolheu o item que quer levar até o fim.

> **Contar rodada, não ação do portador.** Com a barra de velocidade (§1), um apóstolo rápido age o
> dobro — se ele também evoluísse equipamento ao dobro, a Velocidade viraria duplamente dominante.
> **Rodada é a unidade que não acopla.**
>
> O teto por batalha e o bônus de vitória fecham as frestas de exploit (arrastar a luta, perder de
> propósito) **sem** tirar o progresso de quem tentou a fase acima do nível e perdeu — que é o que faz
> o jogador continuar arriscando.

#### Por que evoluir NÃO atropela o drop

Cada eixo tem fonte própria, e **só um é exclusivo de quem persiste**:

| eixo | vem do DROP? | vem de JOGAR? | vem de PAGAR? |
|---|:-:|:-:|:-:|
| **estrela** | ✅ sorteada na faixa da dificuldade | ✅ o nível | — |
| **raridade** | ✅ sorteada em `[comum..estrela]` | — | ✅ sacrifício |
| **aprimoramento** | — | — | ✅ forja |

**Estrela e raridade o drop também dá.** No Fácil você chega a 3★ raro evoluindo — e um 3★ raro pode
cair pronto ali do lado. Evoluir **não é o caminho superior; é o caminho para o item que você quer
manter.** Quem só quer status veste o que caiu.

**O item evoluído só ganha em APRIMORAMENTO**, o eixo mais lento e o único que nenhum drop entrega. É
pouco o bastante pra a caixa de surpresa continuar valendo, e o bastante pra o item antigo ter alma —
que é o *"o mais fraco sempre pode virar o mais forte"* sem matar o drop.

#### Os marcos, e as três travas em cadeia

Nível cheio é a primeira condição. A segunda é o **marco**, e ele é **por FASE, não por capítulo**:

> **O marco de um item é vencer, naquela dificuldade, a FASE DE ORIGEM dele.** Passou a fase 1 do
> capítulo 1 no Pesadelo → o item da fase 1 do capítulo 1 destrava **6★ e mítico**. No Difícil → 5★ e
> lendário. Normal → 4★ e épico. Fácil → 3★ e raro. Um marco por item, na fase que é dele.

> **É POR PEÇA, e é isso que impede o destravamento em bloco.** Terminar as 7 fases do capítulo 1 no
> Normal destrava `4★ épico` para as **7 peças do Reino** e mais nada — as do Lado Sombrio seguem
> presas no marco do Fácil até você fazer o capítulo 2 no Normal. Vencer uma fase difícil nunca
> destrava o arsenal inteiro.
>
> **E o marco é a ÚNICA coisa que a dificuldade alta dá com exclusividade.** O nível enche em qualquer
> lugar; depois que o teto subiu, dá pra completar a estrela jogando no Fácil mesmo — só que a rodada
> lá vale um ponto contra os quatro do Pesadelo. **O caminho fácil existe e é lento, que é como uma
> escolha deve ser.**

**Isso fecha a cadeia sozinho, sem regra extra.** Pra dropar um item 6★ é preciso vencer aquela fase no
Pesadelo — e vencer aquela fase **é** o marco. **Não existe item com estrela acima do próprio marco**,
por construção; o marco se satisfaz pelo mesmo ato que entrega o item. Cair um 6★ mítico de primeira
na fase 1 do Pesadelo é legítimo e não abre buraco nenhum.

```
marco da FASE do item  →  trava a ESTRELA e a RARIDADE (teto da dificuldade vencida)
ESTRELA                →  trava a RARIDADE (raridade ≤ estrela)
nível lento            →  encosta no teto e para
```

Farmar mais só leva mais rápido até uma parede que não se compra com tempo. Sem anti-cheat, sem trava
artificial.

**E é o marco por fase que entrega a fantasia inteira do sistema:** aquela arma 1★ comum da primeira
hora de jogo fica num teto baixo até o dia em que você volta e vence **a mesma fase** no Pesadelo — e
aí ela destrava até mítico 6★. *"A arma que eu venho jogando desde o início"* deixa de ser sentimento e
vira algo que o jogo mede: **você provou que domina o lugar de onde ela veio.** Nenhum item vira lixo,
e um item do capítulo 1 pode ser mais forte que um do capítulo 8 até você chegar lá.

> **O item é FIXO por fase** — a arma na 1-1, e assim por diante, como já é hoje (`Item` carrega
> `Faccao` + `Fases` + `TipoStat`).
>
> **E a fase 7 dropando TODOS os tipos não reabre o furo**, porque as fases destravam em SEQUÊNCIA
> (`CapitulosService.cs:82-88`: `DesbloquearFase` abre a seguinte, `EstaDesbloqueado` exige a
> anterior). Quem vence a 7 já venceu 1–6 naquela dificuldade, então todo item que cai lá já tem o
> marco da fase de origem cumprido. **A ordem das fases faz o trabalho sozinha.**
>
> **O save já tem a forma certa:** `Capitulo` guarda `List<bool>` de 7 fases; o marco pede a mesma
> lista **por dificuldade**.

#### A FORJA — o custo, e a escolha das subestatísticas

Subir a raridade custa **sacrificar outros itens**, e o sacrifício é o que decide as opções:

- **Consome o MESMO item, do MESMO CONJUNTO, na raridade ATUAL** — a arma do Reino sacrifica armas do
  Reino —, e a raridade sobe um degrau (raros fazem um épico; épicos fazem um lendário). Pedir a
  raridade alvo mataria a razão de existir da forja: quem tem épico não precisa fabricar épico.

> **É essa regra que decide ONDE se farma**, e ela aperta por dois lados ao mesmo tempo: o material
> tem de ser a mesma PEÇA e do mesmo CAPÍTULO. Evoluir a arma do Reino manda você de volta à **Reino
> 1-1**, que é a única fonte de arma do Reino (§O drop).
>
> **E o preço não é a fase, é a DIFICULDADE.** Chegou-se a escrever que *"a 1-1 é fácil demais pra ser
> fonte de mítico"*, e foi por isso que a fase 7 ganhou a variedade — **a justificativa estava errada**:
> quem gateia raridade é a dificuldade, e a Reino 1-1 no **Pesadelo** é fonte legítima de mítico. Fase
> curta não é fase barata.
- **As subestatísticas dos sacrificados viram o POOL**: a forja oferece **3 opções** e você escolhe 1.
  Se o pool não tiver o bastante, completa com aleatórias dentro das regras de sub.
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

**O N de uma tentativa tem de ser pequeno** (chute inicial: 2–3 no primeiro degrau, subindo pouco).
Quem gasta dez vezes isso gastou **por escolha**, perseguindo a sub — e aposta voluntária é conteúdo,
enquanto obrigação é fazenda. **O material sai da fase da própria peça, na dificuldade mais alta que
você aguenta** — é aí que ele tem preço, sem obrigar a repetir a fase 1 cem
vezes. **Se o número obrigar a repetir fase curta, virou imposto — baixar.**

> **Efeito colateral a vigiar:** com mais volume E mais variedade, a fase 7 domina e as fases 1–6 viram
> passagem única. Normal em ARPG, e não é defeito. Se um dia quiser as outras vivas, a saída barata é
> **fase 7 = volume e variedade; cada fase = o item dela, mais rápido** — dois motivos diferentes de
> repetir, sem regra nova.

#### A forja de míticos — o endgame

Mítico em cima de mítico não sobe raridade (não há acima). Pelo **mesmo custo**, ela faz uma de duas
coisas, à escolha:

1. **trocar subestatísticas** — 4 subs, 3 opções para cada, mesma regra de recusa por slot;
2. **re-sortear os aprimoramentos** — mantendo as subs.

Duas utilidades numa operação só: o material rende o dobro em decisões, sem nenhuma mecânica nova.

**Aqui nasce a primeira decisão de recurso de verdade da economia:** o mítico dropado passa a ter
destinos que **competem** — vestir mais um apóstolo da frota, virar material da forja de subs, ou pagar
um re-sorteio.

#### Resetar = voltar o APRIMORAMENTO a `+0`

Sem mecânica nova: **o aprimoramento volta a `+0` e sobe de novo**, e a distribuição das unidades
re-sorteia sozinha, porque ela é consequência da subida e não algo guardado à parte. O custo é refazer
os `+20` pagando o material outra vez — **um preço que já existe**.

> **O NÍVEL não é tocado.** Resetar mexe só no que a forja comprou; o que foi ganho jogando fica. É a
> separação dos dois eixos aparecendo na mecânica de reset sem precisar de regra escrita pra isso.

Custos adicionais, para o reset não virar re-sorteio infinito:
- **Custo CRESCENTE por item** (2ª vez 2×, 3ª 4×…, com teto pra não matar o item). É o que realmente
  freia o lunático caçando o `(5,0,0,0)`, e é **um número só**.
- **Sacrificar um item da mesma raridade**, junto da moeda.
- **Perda total** — nada volta, mesma lógica da recusa na forja: a aposta tem de doer pra valer.

> **Descartados:** perder estrela ou perder um degrau de raridade (misturam eixos que o desenho acabou
> de separar, e perder raridade tira subestatística — punição desproporcional pra quem só queria
> re-sortear). E **chance de falha** em cima: já existe RNG na distribuição, e falha sobre RNG é
> frustração dobrada sem profundidade nenhuma a mais.
>
> **O custo de subir nível é o botão que calibra o endgame inteiro** — barato demais e todo mundo tem
> o item perfeito numa semana; caro e o `(5,0,0,0)` é um troféu.

### AS DUNGEONS — a fonte dos acessórios

**Duas dungeons**, uma por acessório (colar, pulseira). É o que leva o conjunto a 9 peças.

- **Escolhe-se a FACÇÃO na entrada**, e essa escolha define **o conjunto que dropa** e **a luta que se
  enfrenta**. Deixa de ser menu e vira decisão.
- Cada facção traz **buff no boss e/ou debuff no jogador**, combináveis — mais variedade sem inflar um
  boss só. **Não removíveis.**
- **As mesmas 4 dificuldades** da campanha, **desbloqueadas pelo capítulo** correspondente, e o jogador
  escolhe qual enfrentar.
- **Os modificadores são revelados na tela de pick**, antes de entrar.
- **Marco do acessório** = vencer aquela dungeon, naquela facção, naquela dificuldade — a mesma regra
  dos outros itens, com "fase" trocada por "dungeon".

> **O motor já suporta o "não removível": custo ZERO.** `StatusEffect.Removivel` existe
> (`StatusEffect.cs:42`) e o `Seletor.Removiveis()` já a respeita — quem limpa debuff só alcança o que
> está marcado. A peça foi construída pro DocesOuTravessuras e serve aqui inteira.
>
> **Usar as mesmas 4 dificuldades foi decisão de ECONOMIA:** uma escada própria criaria um segundo
> lugar onde "quanto vale a dificuldade X" está escrito, e duas cópias de um número divergem. Se um dia
> quiser mais granularidade, ter níveis DENTRO de cada dificuldade mantém o teto vindo do bloco.

**Duas regras pros modificadores valerem a pena:**
1. **Eles têm de pedir uma RESPOSTA, não mais números.** `+50% de HP no boss` só faz a luta durar mais;
   `reflete dano em área` faz trocar o time. O segundo é conteúdo, o primeiro é tempo.
2. **Buff no boss é o padrão** (mais legível — vê-se contra o que se luta). **Debuff no jogador é a
   ferramenta de forçar COMPOSIÇÃO** (`cura recebida −50%` desliga a dependência de healer). Não
   removível **e anunciado** é quebra-cabeça; não removível **e invisível** é loteria — e a diferença
   entre as duas coisas é texto numa tela.

### Sobre o offline e o save editado

**Não existe defesa real em jogo offline single-player.** Ofuscar, assinar, checksum: uma tarde de
trabalho pra quem quer burlar, e atrito permanente pra quem não quer.

O que resolve não é impedir, é **tornar inútil**: não há ranking, não há PvP, nada que o save de um
afete o de outro. Quem editar estraga o próprio jogo.

**E isso libera o desenho.** A economia pode ser feita pelo que é DIVERTIDO, não pelo que é à prova de
fraude — o que permite fugir do "junte 100 cópias", que é design de retenção de live service.
**Por isso a ideia das MISSÕES POR APOSTOLO é melhor que duplicata:** missão é conteúdo, duplicata é
imposto de tempo.

---

