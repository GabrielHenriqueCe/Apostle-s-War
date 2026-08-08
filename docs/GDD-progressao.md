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

**A Velocidade quase não sobe de nível — ela vem de EQUIPAMENTO.** Num modelo de barra de turno ela é
o stat mais forte que existe; se o nível a empurrasse junto com HP/ATK/DEF, o apóstolo nivelado
dominaria os dois eixos ao mesmo tempo e não haveria calibragem que salvasse. Deixando-a no
equipamento ela vira **escolha de build**, e não consequência de investimento.

**A base vem do TIPO e mexe pouco** — **+5 no total** entre o nv 1 e o nv 60, e nada mais:

| tipo | nv 1 | nv 60 |
|---|--:|--:|
| Guardião | 85 | 90 |
| Combatente | 95 | 100 |
| Suporte | 105 | 110 |
| Atirador | 110 | 115 |

Uma faixa de ~28% — pequena o bastante pra não decidir a luta sozinha, grande o bastante pra ser
identidade. **A Velocidade fica FORA da variação por facção** (§2): sendo o stat que decide quem
joga, uma facção com bônus nela domina todas as outras.

> **CUIDADO que vem do DD, e ele inverte a intuição.** No Darkest Dungeon a ordem do turno é
> `Velocidade + d8`, **rolado a cada rodada** — por isso a faixa apertada deles funciona: o dado
> domina e o lento tem chance real toda rodada. **A nossa barra é o modelo Raid, e é
> DETERMINÍSTICA.** 90 contra 115 não é "provavelmente antes", é **sempre** antes, e a diferença se
> acumula rodada após rodada. Copiar amplitude estreita pra dentro de um sistema sem dado dá o pior
> dos dois mundos: parece pouca diferença na ficha e é diferença absoluta na luta.

> **Onde ela é balanceada:** na tabela de itens. **Principal exclusivo da Bota**, e **sub em valor
> cheio** nas outras peças — assim existe o endgame de montar velocidade, e a Bota é o grande prêmio
> em cima dele.
>
> **De graça:** como item é trocável, qualquer apóstolo pode virar rápido se você pagar por isso —
> flexibilidade sem apagar a identidade do tipo.

### Defesa — `DEF / (DEF + 5000)`

Uma curva que **nunca satura e nunca chega a 100%**: o ganho por ponto despenca conforme sobe, sem cap
e sem penhasco, e **o item de DEF nunca vira lixo por ter passado de um número**.

> **O `k` tem leitura direta: é a DEF que dá exatamente 50%.** É o botão do joelho da curva, e é o
> único número a calibrar.
>
> **Esta forma serve à DEFESA e não ao malefício**, e a diferença é de propósito: na defesa se QUER que
> nunca sature; no malefício é preciso **poder aplicar o efeito cheio**, senão nenhuma habilidade
> jamais faz o que está escrito nela. Por isso Precisão × Resistência usa uma curva que **satura**.

| DEF | redução | o que os próximos **+1000** ainda valem |
|---|---|---|
| 1.000 | 16,7% | +11,9 pontos |
| 2.500 | 33,3% | +6,3 pontos |
| **5.000** | **50%** | +4,5 pontos |
| 10.000 | 66,7% | +2,1 pontos |
| 20.000 | 80% | +0,8 ponto |

> **O que isto substitui:** `min(DEF/1000 × 0,75, 0,75)` (`Combate.cs:47-48,366`), que é **linear até
> 1000 e vale ZERO daí em diante**. Com o item de DEF valendo `55 × capítulo`, **dois deles já
> saturavam** — e com 7 slots míticos o slot de DEF morreria sozinho.

### Precisão × Resistência — o malefício cola?

**O nome é `Precisão`** (era "Aptidão", jargão de WoW pt-BR que ninguém decodifica). Sem esquiva no
jogo, nada mais pode "errar", então o nome fica livre e se explica sozinho.

**A ESCALA:** 0–1.000. Uma build balanceada fica em **400–600**; full na estatística chega a **1.000**.
Vale igual pros dois lados.

```
chance de colar            =  min( 100% , Precisão ÷ (Resistência × 2) )
chance de reduzir 1 turno  =  (100% − chance de colar) ÷ 2
piso: nunca abaixo de 1 turno — já colou, então dura
```

**Empate = 50% de chance. O DOBRO da Resistência = 100%, garantido.** Contra um alvo balanceado de
**500 de Resistência**, numa habilidade de 2 turnos:

| sua Precisão | cola | reduz 1t | o que acontece |
|---|---|---|---|
| 250 | 25% | 37,5% | 75% nada · 15,6% com 2t · 9,4% com 1t |
| **500** (empate) | **50%** | **25%** | 50% nada · 37,5% com 2t · 12,5% com 1t |
| 750 | 75% | 12,5% | 25% nada · 65,6% com 2t · 9,4% com 1t |
| **1.000** (full) | **100%** | **0%** | **sempre cola, sempre cheio** |

**A segunda rolagem morre junto com a primeira** — quem chega a 100% de Precisão cola sempre **e**
cola cheio. A build perfeita compra as duas certezas de uma vez, sem regra separada pra isso.

> **É RNG, mas o RNG é comprável.** A objeção de sempre contra chance é perder a ação inteira pra um
> dado — e ela só vale quando o dado é **inevitável**. Aqui a Precisão é o preço de sair da loteria.
> Melhor que o Raid nisso: lá sempre sobra 3% de erro, piso que existe pra impedir garantia em PvP e
> arena. **Sem PvP, não há motivo pra negar 100% a quem pagou.**
>
> **Na tela é UM número.** A habilidade continua dizendo o que sempre disse (valores íntegros, nada de
> efeito pela metade); ao mirar, aparece `chance de aplicar: 75%` — e essa linha **some** quando chega
> a 100%, que é o estado que o jogador persegue.
>
> **O `× 2` é o botão:** baixa pra 1,5 e o controle fica fácil; sobe pra 3 e Resistência vira muito
> forte. **Precisão × Evasão não existe** — a esquiva está cortada, e `Precisão` assumiu o nome.

**Por que a potência NÃO é reduzida** (a alternativa considerada): aplicar o efeito a 18% em vez de 30%
seria determinístico e granular, mas exigiria **mostrar o valor recalculado por alvo** na tela. A
habilidade passa a dizer coisas diferentes conforme quem se mira. Uma porcentagem de acerto é um número
só, e o jogador já sabe ler.

### Nada de RETORNOS DECRESCENTES globais — quem trava é o BOSS

A escada do WoW (2ª aplicação vale metade, 3ª um quarto, depois imune) **fica de fora**. Reaplicar
veneno, queima ou redução de defesa não quebra nada; o único abuso real é **controle que tira o turno**
— quatro apóstolos atordoando em sequência travam um chefe pra sempre.

**A trava não é regra global, é PASSIVA DO BOSS**, e o jogador **lê na ficha dele**:

- *"não pode ter o turno reduzido"*
- *"redução de turno limitada a 10%"*
- *"imune a atordoamento, prisão e qualquer impedimento de agir"*
- *"dano de veneno e queima acumulados têm teto"*

**Isso é melhor que a regra global por três motivos:** vira **conteúdo** (cada boss é um quebra-cabeça
diferente em vez de uma regra que vale pra todos), é **visível** (lido na ficha, não descoberto na
marra), e **não põe exceção nenhuma no motor** — que é justamente o que a Composição de Ações existe
pra permitir.

#### Metade dessas passivas já funciona; a outra metade pede um gancho novo

`IBloqueiaStatus` (`ICapacidadesStatus.cs:79`) é chamado em `Combate.PodeReceber` antes de adicionar
qualquer status, e **já tem implementadores em passiva-pura** (`CascaDura`, `PeleDeDragao`). Mas a
resposta dele é **binária** — o status entra inteiro ou não entra:

| passiva de boss | cabe no `IBloqueiaStatus`? |
|---|---|
| *"imune a atordoamento, prisão e qualquer impedimento de agir"* | ✅ sim, e custa **zero motor** |
| *"não pode ter o turno reduzido"* | ✅ sim |
| *"redução de turno limitada a **10%**"* | ❌ precisa **entrar mais fraco** |
| *"veneno e queima acumulados têm **teto**"* | ❌ idem |

**Falta o gancho de ATENUAR, e o motor já tem o espelho dele do lado do dano:**

| | dano | status |
|---|---|---|
| **barrar** | `IPrevineMorte`, bloqueios | `IBloqueiaStatus` ✅ |
| **atenuar** | `IModificaDanoRecebido` (escudo, bloqueio, proteção de aliado) | **não existe** ❌ |

Ele entra no mesmo ponto do fluxo: depois que ninguém bloqueou, os atenuadores ajustam valor e/ou
duração antes de o status ser adicionado.

> **ARMADILHA CONHECIDA, e ela já custou um bug aqui:** com dois atenuadores no mesmo status, **a ordem
> muda o resultado**. Foi exatamente o bug do bloqueio × escudo, fechado no **#185** com o
> `OrdemDeMitigacao` (`ReduzDeGraca` × `ConsomeRecurso`, **sem `default`**, pra o compilador cobrar de
> quem escrever capacidade nova). O gancho de status deve **nascer com a ordem declarada** — descobrir
> isso depois significa um boss se comportando errado em silêncio.

> **Resistência por TIPO** (Darkest Dungeon: Atordoamento, Sangramento, Praga…) custaria seis stats na
> ficha, seis fontes em item e seis números pra calibrar. **Uma Resistência única no stat**, com a
> variedade vindo de traço/passiva de apóstolo, dá a mesma decisão de time por muito menos. O encaixe é o
> `StatusEffect` (que já tem tipo), **NÃO** a `NaturezaDano` — esta descreve o golpe, não o efeito.

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

### O STAT BASE É DO TIPO, não do apóstolo

**Todo Guardião começa com os mesmos status. O que diferencia dois Guardiões é o KIT e o SET que você
monta neles.** E **cada facção/capítulo tem um de cada tipo** — quatro apóstolos, quatro papéis.

**108 números viram 12** (36 apóstolos × HP/ATK/DEF → 4 tipos × HP/ATK/DEF). E isso muda a natureza do
#16: rebalancear deixa de ser "ajustar 36 fichas" e passa a ser "ajustar 4 arquétipos + os
multiplicadores das habilidades". A bancada também fica legível, porque a variação que ela mede passa
a ser **só de kit**.

> **A grade atual foi DESCARTADA** (decisão do Gabriel, ago/2026). Chegou-se a considerar consolidar
> os degraus de hoje (`HP 600…1400`, `ATK/DEF 120…280`), mas eles foram escritos contra um jogo sem
> progressão e sem os stats novos. Os números abaixo nascem do zero, calibrados do TOPO.

#### A TABELA — calibrada do nv 60 pra trás

O método é do Gabriel: **crava o topo e deriva o resto.** O nv 60 é o teto do mítico (§3), então é a
ficha que o jogo inteiro tem de servir; o nv 1 é ela dividida por 30.

**nv 60 — mítico, ZERO item:**

| tipo | pos | HP | ATK | DEF | Vel | Precisão | Resist. | Taxa Crít. | Dano Crít. |
|---|---|--:|--:|--:|--:|--:|--:|--:|--:|
| **Guardião** | 1–2 | **30.000** | 510 | **1.500** | 90 | 50 | 120 | 5% | 60% |
| **Combatente** | 1–2 | 25.200 | 1.350 | 1.200 | 100 | 80 | 90 | **25%** | **90%** |
| **Suporte** | 3–4 | 20.100 | 810 | 960 | 105 | **150** | **150** | 10% | 70% |
| **Atirador** | 3–4 | 15.000 | **1.500** | 510 | **115** | 120 | 50 | 15% | 80% |

**nv 1 — a mesma ficha ÷ 30:**

| tipo | pos | HP | ATK | DEF | Vel | Precisão | Resist. | Taxa Crít. | Dano Crít. |
|---|---|--:|--:|--:|--:|--:|--:|--:|--:|
| Guardião | 1–2 | 1.000 | 17 | 50 | 85 | 50 | 120 | 5% | 60% |
| Combatente | 1–2 | 840 | 45 | 40 | 95 | 80 | 90 | 25% | 90% |
| Suporte | 3–4 | 670 | 27 | 32 | 105 | 150 | 150 | 10% | 70% |
| Atirador | 3–4 | 500 | 50 | 17 | 110 | 120 | 50 | 15% | 80% |

**A REGRA QUE GOVERNA A TABELA: cada tipo é PRIMEIRO em dois stats e ÚLTIMO em algum.**

| stat | dono |
|---|---|
| HP · DEF | **Guardião** |
| Taxa Crítica · Dano Crítico | **Combatente** |
| Precisão · Resistência | **Suporte** |
| ATK · Velocidade | **Atirador** |

Isso não é enfeite — é o critério de aceitação. **A primeira versão desta tabela foi rejeitada por
ser uma RAMPA**: cada stat deslizava de Guardião até Atirador, e o Suporte caía em 3º lugar de tudo,
sem vantagem nenhuma. Se uma revisão futura deixar um tipo sem primeiro lugar, ela quebrou a regra.

**Os dois tipos de DANO não são um degrau, são formatos diferentes.** O dano médio por pancada sai
quase idêntico — Atirador `1.500 × (1 + 0,15 × 0,80) = 1.680`, Combatente
`1.350 × (1 + 0,25 × 0,90) = 1.654` — mas o Atirador entrega dano REGULAR e o Combatente entrega
dano IRREGULAR com picos. O que os separa de verdade é o resto da ficha: o Combatente tem 10.000 HP
e 690 DEF a mais; o Atirador tem 15 de Velocidade a mais (≈15% mais turnos) e alcança o fundo pela
habilidade. **Um sobrevive, o outro age mais vezes e chega mais longe.**

> **Por que a Taxa Crítica é vantagem de verdade** (observação do Gabriel): ela é o **único stat com
> teto duro**. Base de taxa não vale "mais crítico", vale **menos item gasto pra chegar aos 100%** —
> e o que sobra vai pra Dano Crítico. A vantagem é economia de slot, não magnitude.
>
> **E o Dano Crítico tem PISO 60% pra todo mundo**, de propósito: abaixo disso, item de dano crítico
> vira lixo na mão do Guardião e do Suporte, e nenhum item deve ser lixo pra ninguém.

#### A CURVA DE NÍVEL — contínua, de 1× a 30×

```
stat(nv)  =  base × (1 + 29 × (nv − 1) / 59)
```

| nível | 1 | 10 | 20 | 30 | 40 | 50 | 60 |
|---|--:|--:|--:|--:|--:|--:|--:|
| multiplicador | **1×** | 5,4× | 10,3× | 15,3× | 20,2× | 25,1× | **30×** |

**As duas pontas é que são declaradas; a taxa por nível (`29/59 ≈ 0,49`) cai fora da fórmula
sozinha** — ninguém a escolhe, e por isso ela não pode divergir das pontas. São ~4,9× por década.

**A curva é CONTÍNUA, não escada** (decisão do Gabriel): sobe todo nível, e não em saltos a cada 10.
O jogador tem retorno a cada batalha em vez de ficar parado entre marcos.

**Só HP, ATK e DEF escalam com nível.** A Velocidade sobe 5 no total (§1). Precisão, Resistência e os
dois de crítico **não sobem com nível** — vêm do tipo e do item.

> **Consequência que fecha um buraco:** como o ÷30 é uniforme, a razão HP:ATK é a mesma nas duas
> pontas, e **a luta dura o mesmo tanto no nv 1 e no nv 60**. Um escalonamento desigual (HP ×20, ATK
> ×5) faria a luta de fim de jogo ficar 5× mais longa que a inicial sem ninguém ter pedido isso.

> **O que esta tabela AINDA NÃO valida:** os status dos itens não existem (o Gabriel apontou). Como o
> item multiplica MUITO, nenhum número aqui está provado até a tabela de item existir — em especial a
> DEF, que precisa dos milhares pra `DEF/(DEF+5000)` ter joelho. O nv 60 base entrega 1.500 de DEF ao
> Guardião (23% de redução); é o item que leva isso pra faixa dos 50–60%.
>
> **E o ATK 17 do Guardião no nv 1 é de propósito** — ele é inofensivo. Se algum dia ele precisar
> matar alguma coisa sozinho no começo do jogo, é esse o número a subir.

**O erro fica grande e visível, e isso é bom:** um Guardião mal calibrado estraga 9 apóstolos de uma vez
— mas é **um** erro, em **um** lugar, consertável com **um** número. Vence nove erros dispersos que só
aparecem jogando.

**Custo real, e o Gabriel já aceitou pagar:** quem decide o tipo é o KIT, e hoje há capítulos com dois
apóstolos do mesmo papel (o Reino tem Ninja e Mago, ambos de dano). Adequar significa **sobrescrever
habilidade** — trabalho de implementação, não decisão em aberto.

> **Ideia barata quando for implementar:** o `Slot` pode VIRAR o tipo (Slot1 = Guardião, Slot2 =
> Combatente…). Zero campo novo — o slot já é identidade no save (`Faccao+Slot`) e já comanda a ordem
> de drop, então o jogador ganharia os papéis sempre na mesma ordem e teria time completo cedo. O
> preço é engessar a ordem de descoberta. Não decidido.

### A VARIAÇÃO POR FACÇÃO — ±5%, soma zero

Dentro de uma facção, os Guardiões são idênticos; entre facções, cada uma torce a ficha um pouco. O
Lado Sombrio pode ser `+HP +ATK −DEF`, **ganhando e perdendo o mesmo tanto**. Assim um Guardião de uma
facção realmente aguenta mais que o de outra, e nenhuma facção é estritamente melhor.

**A matriz é 4 tipos + 9 facções = 13 conjuntos de números**, não 36 fichas soltas.

> **CUIDADO, e ele é matemático: soma zero em % NÃO é soma zero em PODER.** HP e DEF se
> **multiplicam** (o quanto se aguenta é `HP × 1/(1−redução)`), enquanto ATK é linear. `+5% ATK` e
> `−5% DEF` não se cancelam no jogo, só na planilha. **A neutralidade tem de ser MEDIDA na bancada**,
> nunca presumida da soma.
>
> **Velocidade fica FORA da matriz.** Sendo o stat que decide quem joga, uma facção com bônus nela
> domina todas as outras, e nenhum `−5% DEF` compensa isso.

### Dano que escala com DEF ou HP — somando, nunca substituindo

```
dano  =  ATK × multiplicador            (todo mundo)
      +  DEF × multiplicador2           (só quem tem essa habilidade)
```

Um Guardião que bate forte por ser durão é bom; **um apóstolo que IGNORA o ATK não é.** No Raid isso cria
duas economias de equipamento paralelas — o item de ATK vira lixo pra metade do elenco. Somando em vez
de substituir, o ATK segue sendo o eixo do dano do jogo inteiro e a parcela extra é tempero.

**Custo: um campo a mais na habilidade**, e nenhuma exceção no motor.

### O nível sobe por CURVA DO TIPO — nada de pontos distribuíveis

O nível dá ganhos diferentes por arquétipo (o Guardião ganha mais HP/DEF), e o jogador não distribui
nada.

**Por que não pontos livres**, mesmo sendo uma ideia boa em abstrato:
- **A agência já mora no ITEM** — 7 slots, subs escolhidas na forja, conjuntos. Ali a decisão é rica e
  **reversível**. Pontos no nível seriam uma segunda fonte de build, irreversível, competindo com a
  primeira, e com 36 apóstolos viraria planilha.
- **Pontos livres apagam os 4 arquétipos.** Um Guardião com tudo em ATK deixa de ser Guardião, e os 12
  números acima param de significar coisa alguma.
- **A Velocidade condena a ideia sozinha:** sendo o stat rei da barra de turno, todo jogador
  despejaria tudo nela, em todos os apóstolos.

Com a curva por tipo acontece o contrário: **o Guardião fica mais Guardião conforme sobe** — o
arquétipo define o ponto de partida E a inclinação, e a identidade se aprofunda com o investimento.

> **A válvula, se um dia faltar agência aqui:** 1 ponto livre a cada 10 níveis, sem Velocidade na
> lista. Só depois de sentir a falta, nunca antes.

---

## 3. O APÓSTOLO — NÍVEL E RARIDADE (não tem estrela)

**A separação é do Gabriel e é coerente:** número cresce por nível; **comportamento** muda por
raridade.

- **Raridade**: comum (cinza) · incomum (verde) · raro (azul) · épico (roxo) · lendário (dourado) ·
  mítico (vermelho). **Sobe** (não é fixa no drop).
- **Nível sobe por XP de batalha** — barra de progressão com marcos, igual à do item.
- **Raridade sobe por MISSÃO do apóstolo** + teto da dificuldade.
- **Subir raridade NÃO zera o nível.**

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

> **⏳ EM ABERTO — o que acontece quando alguém MORRE.** O doc define de onde se usa e quem se alcança,
> mas não o que fazer com o buraco. Sem regra, **a luta trava**: se sobra um Combatente meu (alcança
> 1–2) contra um Atirador na posição 4, ninguém encosta em ninguém. A proposta é o modelo do DD —
> **as fileiras COMPACTAM** (morreu o da 1, todos deslizam pra frente), mais o **⚔️ Atacar como piso
> universal** (usa de qualquer posição, alcança 1–2). Aí o travamento é impossível por construção, e
> de quebra derrubar a frente **arrasta o fundo** pra dentro do alcance — podendo desligar o kit dele
> no caminho, sem você ter tocado nele. **Não decidido: fica pro estudo das posições.**

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

1. **Velocidade + barra de turno + fila única.** Mexe em `Batalha`, `Equipe`, `TurnoDoPersonagem`.
2. **Precisão × Resistência** (chance de colar) + a **DEF em `DEF/(DEF+5000)`**, no lugar do cap atual.
3. **Posição na habilidade** (`posicoesDeUso`/`posicoesAlvo`) + ordenar o time na montagem.
4. **Tipos** (Guardião/Combatente/Atirador/Suporte) — **com o stat base vindo do tipo** e um de cada
   por facção. Arrasta sobrescrever habilidade nos capítulos com dois apóstolos do mesmo papel.
5. **Nível (curva do tipo) + Raridade** nos apóstolos. Sem estrela.
6. **Raridade → passiva que escala.**
7. **Item equipado no apóstolo.**

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

- A posição é **compromisso do jogador**; nada de correção automática.
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
