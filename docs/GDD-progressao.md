# GDD — PROGRESSÃO E COMBATE (ago/2026)

O plano que o Gabriel desenhou depois que a separação do front ficou pronta. **Ele muda o significado
de quase todo número do jogo**, e é por isso que o #16 (rebalance) e o PR de dificuldade estão
parados: rebalancear agora é medir uma coisa que vai deixar de existir.

> **Por que o #16 espera.** A bancada mede champ com stat FIXO contra boneco parado. Aqui o stat base
> passa a vir do **tipo**, cresce por `nível`, a habilidade muda por `raridade`, a ordem de turno passa
> a ser barra de velocidade e a posição limita quem alcança quem. Os 144 números do relatório atual
> medem um jogo que está sendo substituído.
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

**A Velocidade quase não sobe de nível — ela vem de EQUIPAMENTO.** Num modelo de barra de turno ela é
o stat mais forte que existe; se o nível a empurrasse junto com HP/ATK/DEF, o campeão nivelado
dominaria os dois eixos ao mesmo tempo e não haveria calibragem que salvasse. Deixando-a no
equipamento ela vira **escolha de build**, e não consequência de investimento.

**A base vem do TIPO e mexe pouco:** `+1 a cada 10 níveis`, do Guardião (85 → 90) ao Atirador
(110 → 115). Uma faixa de ~28% — pequena o bastante pra não decidir a luta sozinha, grande o bastante
pra ser identidade. **A Velocidade fica FORA da variação por facção** (§2): sendo o stat que decide
quem joga, uma facção com bônus nela domina todas as outras.

> **Onde ela é balanceada:** na tabela de itens. **Principal exclusivo da Bota**, e **sub em valor
> cheio** nas outras peças — assim existe o endgame de montar velocidade, e a Bota é o grande prêmio
> em cima dele.
>
> **De graça:** como item é trocável, qualquer champ pode virar rápido se você pagar por isso —
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
— quatro champs atordoando em sequência travam um chefe pra sempre.

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
> variedade vindo de traço/passiva de champ, dá a mesma decisão de time por muito menos. O encaixe é o
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

### O STAT BASE É DO TIPO, não do champ

**Todo Guardião começa com os mesmos status. O que diferencia dois Guardiões é o KIT e o SET que você
monta neles.** E **cada facção/capítulo tem um de cada tipo** — quatro champs, quatro papéis.

**108 números viram 12** (36 champs × HP/ATK/DEF → 4 tipos × HP/ATK/DEF). E isso muda a natureza do
#16: rebalancear deixa de ser "ajustar 36 fichas" e passa a ser "ajustar 4 arquétipos + os
multiplicadores das habilidades". A bancada também fica legível, porque a variação que ela mede passa
a ser **só de kit**.

> **Não é reescrita, é consolidação — os stats atuais já vivem numa grade:**
> `HP 600·800·1000·1200·1400` (passo 200) e `ATK/DEF 120·160·200·240·280` (passo 40). Cada champ já
> escolheu um degrau; a mudança é fixar 4 degraus em vez de 36 pontos soltos.

**O erro fica grande e visível, e isso é bom:** um Guardião mal calibrado estraga 9 champs de uma vez
— mas é **um** erro, em **um** lugar, consertável com **um** número. Vence nove erros dispersos que só
aparecem jogando.

**Custo real, e o Gabriel já aceitou pagar:** quem decide o tipo é o KIT, e hoje há capítulos com dois
champs do mesmo papel (o Reino tem Ninja e Mago, ambos de dano). Adequar significa **sobrescrever
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

Um Guardião que bate forte por ser durão é bom; **um champ que IGNORA o ATK não é.** No Raid isso cria
duas economias de equipamento paralelas — o item de ATK vira lixo pra metade do elenco. Somando em vez
de substituir, o ATK segue sendo o eixo do dano do jogo inteiro e a parcela extra é tempero.

**Custo: um campo a mais na habilidade**, e nenhuma exceção no motor.

### O nível sobe por CURVA DO TIPO — nada de pontos distribuíveis

O nível dá ganhos diferentes por arquétipo (o Guardião ganha mais HP/DEF), e o jogador não distribui
nada.

**Por que não pontos livres**, mesmo sendo uma ideia boa em abstrato:
- **A agência já mora no ITEM** — 7 slots, subs escolhidas na forja, conjuntos. Ali a decisão é rica e
  **reversível**. Pontos no nível seriam uma segunda fonte de build, irreversível, competindo com a
  primeira, e com 36 champs viraria planilha.
- **Pontos livres apagam os 4 arquétipos.** Um Guardião com tudo em ATK deixa de ser Guardião, e os 12
  números acima param de significar coisa alguma.
- **A Velocidade condena a ideia sozinha:** sendo o stat rei da barra de turno, todo jogador
  despejaria tudo nela, em todos os champs.

Com a curva por tipo acontece o contrário: **o Guardião fica mais Guardião conforme sobe** — o
arquétipo define o ponto de partida E a inclinação, e a identidade se aprofunda com o investimento.

> **A válvula, se um dia faltar agência aqui:** 1 ponto livre a cada 10 níveis, sem Velocidade na
> lista. Só depois de sentir a falta, nunca antes.

---

## 3. O CAMPEÃO — NÍVEL E RARIDADE (não tem estrela)

**A separação é do Gabriel e é coerente:** número cresce por nível; **comportamento** muda por
raridade.

- **Raridade**: comum (cinza) · incomum (verde) · raro (azul) · épico (roxo) · lendário (dourado) ·
  mítico (vermelho). **Sobe** (não é fixa no drop).
- **Nível sobe por XP de batalha** — barra de progressão com marcos, igual à do item.
- **Raridade sobe por MISSÃO do campeão** + teto da dificuldade.
- **Subir raridade NÃO zera o nível.**

### O CAMPEÃO NÃO TEM ESTRELA — e não se perde nada

**O eixo estava vazio.** A estrela do campeão não tinha fonte de custo que não fosse *sacrificar
cópias*, e duplicata está descartada (ver §4, offline). Eixo sem fonte própria não é eixo, é peça
esperando um sistema que não se quer construir.

**E ela já duplicava a raridade.** O que a estrela fazia era ser **teto de nível** (*1★ até 10, 2★ até
20…*) — exatamente o papel que a raridade tem no item. Passando a função pra frente, **nenhum número
muda, só o dono**:

| raridade do campeão | nível máx |
|---|---|
| comum | 10 |
| incomum | 20 |
| raro | 30 |
| épico | 40 |
| lendário | 50 |
| mítico | 60 |

Mesmo `6 × 10 = 60` que a estrela dava. E a simetria com o item fica exata: **raridade destrava nível,
nível dá os números.** De quebra, o teto de raridade por dificuldade passa a limitar o nível do
campeão **de graça** — no Fácil não se passa de raro, logo não se passa do nível 30.

> **A estrela continua existindo no jogo** — no ITEM, onde ela tem fonte própria (a barra de uso) e
> função própria (magnitude). Não se perde a sensação; para-se de pagar por ela duas vezes.

### As missões, e por que o nível avisa a hora

**Missão é conteúdo; duplicata é imposto de tempo.** Uma missão boa usa o que já existe e prova
domínio do campeão — *"passar a fase 7 do capítulo dele sozinho"*.

E o encaixe com a escada é automático: **o campeão trava no teto e a barra encosta na parede**, o que
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
> **Nada de forçar o uso deles pra passar da 8-7.** Obrigar um campeão específico na fase final é
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

> **Por que isso é o caminho barato:** 36 champs × 6 raridades, se cada degrau mexesse em TODAS as
> habilidades, seriam centenas de variantes pra escrever **e pra balancear**. Uma passiva por champ que
> ganha degraus é uma peça, não vinte. Mudar as habilidades diretamente segue como opção pra casos
> especiais.

---

## 4. ITENS

> **O PRINCÍPIO, e ele é do Gabriel:** *"o mais fraco sempre pode virar o mais forte"*. O comum 1★ que
> caiu na primeira fase pode terminar mítico 6★ com as subestatísticas escolhidas a dedo. **O preço de
> manter isso saudável é o CUSTO:** o caminho do fraco tem de ser mais caro que vestir o forte já
> pronto, senão ninguém quer o forte. Toda trava desta seção existe por causa disso.

- **Equipados no CAMPEÃO**, não mais no jogador. É o que os torna valiosos — e é a mudança de maior
  impacto no save.
- **CONJUNTOS de 9 peças**, com bônus em **3 / 6 / 9**. E o que cada conjunto faz cresce com a
  raridade. *(O que cada conjunto FAZ ainda não foi desenhado.)*

> **O número ímpar deixou de ser problema.** O GDD antigo remendava com bônus em 2/4/6 e o acessório
> **fora** do conjunto — sobrava uma peça sem função. Com os **2 acessórios das dungeons**, são 9
> peças, três degraus iguais entre si, e **toda peça conta**.

### Os três eixos do item — e cada um com sua fonte de custo

| eixo | o que dá | como sobe | o que custa |
|---|---|---|---|
| **raridade** | QUANTAS subestatísticas (+ teto de nível) | barra de uso (lenta) + marco da fase + forja | sacrificar itens da raridade atual |
| **nível** | QUÃO BOAS elas são (aprimoramento) | forja, a qualquer momento | material / moeda |
| **estrela** | MAGNITUDE (principal e subs) | barra de uso (rápida) + marco da fase | tempo de jogo |

> **O marco é o mesmo para os dois** — vencer a fase de origem do item naquela dificuldade destrava
> estrela **e** raridade, no teto daquela dificuldade. Uma condição, dois eixos.

> **A moeda que ficou parqueada já tem dono: é o nível.** Não é preciso procurar função pra ela.

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

| slot | | principal |
|---|---|---|
| **Arma** (fase 1) | fixo | ATK cheio |
| **Elmo** (fase 2) | fixo | HP cheio |
| **Escudo** (fase 3) | fixo | DEF cheio |
| **Manopla** (fase 4) | variável | ATK% · HP% · DEF% · **Taxa Crítica** · **Dano Crítico** |
| **Peitoral** (fase 5) | variável | ATK% · HP% · DEF% · **Resistência** |
| **Calça** (fase 6) | variável | ATK% · HP% · DEF% · **Precisão** |
| **Bota** (fase 7) | variável | ATK% · HP% · DEF% · **Velocidade** |
| **Colar** · **Pulseira** | dungeons | *a definir* |

**Por que metade fixa e metade variável:** se todos variassem, o jogador teria 7 loterias simultâneas e
nunca fecharia uma build; se nenhum variasse, não existiria build, só acúmulo. **Fixos = esqueleto
garantido, variáveis = espaço de escolha.** E o encaixe com as fases é feliz: os fixos são as fases
**1–3** (o esqueleto vem cedo e barato) e os variáveis as **4–7** (a build vem tarde e cara).

**Cada slot variável tem UM stat exclusivo** mais o trio `ATK%/HP%/DEF%` — assim **nenhum drop é 100%
lixo**, e cada peça tem motivo próprio de ser farmada. A Manopla é a mão que golpeia (o crítico inteiro
mora nela, e taxa × dano viram escolha); o Peitoral é o torso que aguenta; a Calça é a perna que se
move; a **Bota** é a única fonte de Velocidade como principal — e ela **já era a fase 7**, a mais
difícil do capítulo, então o stat mais forte do jogo já nasce no lugar mais caro sem mexer em nada.

> **`ATK%` ainda não existe.** O `Combate.cs:86` já tem o `ItensAtaquePct` sendo usado no cálculo, mas
> **não há `TipoStat.ATKPct`** que o alimente — HP e DEF têm cheio *e* %, o ATK só tem cheio. É criar o
> valor no enum e ligar.
>
> **O botão da Velocidade:** a raridade dela é o **tamanho do leque da Bota**. Quatro opções = 25% de
> chance de vir Velocidade. Quer mais rara? Acrescenta opções. Um número por slot.

### AS SUBESTATÍSTICAS — 8 no pool, e nenhuma escolha aritmética

```
armadura (7)   → subs em PERCENTUAL
acessórios (2) → subs em VALOR CHEIO
```

**Esta divisão existe por um motivo de INTERFACE, não de balanço.** Se `ATK` e `ATK%` pudessem sair
juntos, o jogador escolheria entre duas caras do mesmo stat — e decidir entre elas é uma **conta**, não
uma escolha. Pior: na forja, duas das três opções seriam a mesma coisa. Separando por peça, **as duas
formas nunca aparecem lado a lado** e toda opção oferecida é uma coisa diferente das outras.

**O pool (8):** `ATK%` · `HP%` · `DEF%` · `Taxa Crítica` · `Dano Crítico` · `Velocidade` · `Precisão` ·
`Resistência`.

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

### Raridade → quantas subs, e até onde o nível vai

| raridade | subs | nível máx | aprimoramentos no teto |
|---|---|---|---|
| comum | 0 | **4** | 0 |
| incomum | 1 | **9** | 1 |
| raro | 2 | **14** | 2 |
| épico | 3 | **19** | 3 |
| lendário | 4 | **20** | 4 |
| mítico | 4 (+1 de nascença) | 20 | 5 |

**Cada degrau de raridade vale exatamente +1 sub, +5 níveis e +1 aprimoramento.** Escada regular,
legível pro jogador, barata de calibrar — e o mítico é o único que foge (mesmas subs do lendário, só o
aprimoramento a mais), o que faz o último degrau ser especial **sem regra escrita pra isso**.

> **Por que o teto de nível é preso à raridade — a correção é do Gabriel.** Sem isso, nível e raridade
> fazem a MESMA coisa: um comum nível 20 acabaria com as mesmas 4 subs de um mítico, e a raridade só
> voltaria a significar algo na pontinha do aprimoramento. **Prendendo os dois, cada eixo vira uma
> frase:** raridade = quantas · nível = quão boas.
>
> **De quebra, um comum é um item cru de verdade** — trava no nível 4, sem sub nenhuma, valendo só o
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

### O drop

- **Item sempre cai**, mas a estrela varia: 1–3★ Fácil · 2–4★ Normal · 3–5★ Difícil · 4–6★ Pesadelo.
- **O item é FIXO por fase**, como já é hoje — a arma na 1-1, e cada fase com a peça dela. O que varia
  no drop é estrela e raridade, não qual item cai.
- **A quantidade CRESCE ao longo do capítulo**, e a **fase 7 dropa mais itens e de todos os tipos** —
  além de dar mais XP. É ela que dá motivo pra farmar a fase mais difícil.
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
> campeões pra vestir, e **ninguém vai evoluir 7 itens × dezenas de campeões desde 1★**. A evolução é
> pros poucos itens que se carrega; o drop mítico veste todo o resto do elenco. **Por isso as
> subestatísticas são IGUAIS nos dois caminhos** — não há privilégio de nascença pro drop; o que
> segura a fantasia dele é a escassez de tempo do outro lado.
>
> **E nenhuma das duas rotas domina, porque o que a evolução entrega é o CAMINHO, não o destino:** quem
> evoluiu escolheu 1 entre 3 em **cada degrau**, gastando raro/épico/lendário (barato); quem dropou
> chegou cedo com 4 subs aleatórias e precisa da forja de **míticos** pra consertar cada uma (caro).

**DUAS BARRAS, uma fonte.** Estrela e raridade têm barras próprias, que enchem ao mesmo tempo pelo
mesmo uso, em ritmos diferentes — **a de raridade é bem mais lenta**. Não é escolha de investimento:
como a estrela é o teto da raridade, investir em raridade primeiro seria sempre errado, e escolha em
que uma opção domina só pune quem não leu o wiki.

**A barra é do ITEM, não do campeão** — ela viaja quando o item troca de dono. Trocar de portador pausa
o progresso, nunca destrói.

#### Como a barra enche

A barra é de **uso**: ganha por rodada de combate, com o item equipado em alguém em campo.

```
por RODADA do combate, com TETO por batalha   →  arrastar a luta não paga
derrota  = o acumulado                        →  perder também evolui: você usou o item
vitória  = o acumulado + um BÔNUS fixo        →  vencer domina sempre
```

> **Contar rodada, não ação do portador.** Com a barra de velocidade (§1), um campeão rápido age o
> dobro — se ele também evoluísse equipamento ao dobro, a Velocidade viraria duplamente dominante.
> **Rodada é a unidade que não acopla.**
>
> O teto por batalha e o bônus de vitória fecham as duas frestas de exploit (arrastar a luta, perder
> de propósito) **sem** tirar o progresso de quem tentou a fase acima do nível e perdeu — que é o que
> faz o jogador continuar arriscando.

#### Os marcos, e as três travas em cadeia

Barra cheia é a primeira condição. A segunda é o **marco**, e ele é **por FASE, não por capítulo**:

> **O marco de um item é vencer, naquela dificuldade, a FASE DE ORIGEM dele.** Passou a fase 1 do
> capítulo 1 no Pesadelo → o item da fase 1 do capítulo 1 destrava **6★ e mítico**. No Difícil → 5★ e
> lendário. Normal → 4★ e épico. Fácil → 3★ e raro. Um marco por item, na fase que é dele.

**Isso fecha a cadeia sozinho, sem regra extra.** Pra dropar um item 6★ é preciso vencer aquela fase no
Pesadelo — e vencer aquela fase **é** o marco. **Não existe item com estrela acima do próprio marco**,
por construção; o marco se satisfaz pelo mesmo ato que entrega o item. Cair um 6★ mítico de primeira
na fase 1 do Pesadelo é legítimo e não abre buraco nenhum.

```
marco da FASE do item  →  trava a ESTRELA e a RARIDADE (teto da dificuldade vencida)
ESTRELA                →  trava a RARIDADE (raridade ≤ estrela)
barra lenta            →  encosta no teto e para
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
> tem de ser a mesma PEÇA e do mesmo CAPÍTULO. Evoluir a arma do Reino manda você de volta ao Reino, e
> a 1-1 é fácil demais pra ser a fonte de míticos. **Quem paga a conta é a fase 7 daquele capítulo**,
> que dropa todos os tipos dele — o volume existe, mas na fase mais difícil.
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
enquanto obrigação é fazenda. **O volume vem da fase 7**, que dropa mais itens e de todos os tipos —
então o material tem preço (é a fase mais difícil do capítulo) sem obrigar a repetir a fase 1 cem
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
destinos que **competem** — vestir mais um campeão da frota, virar material da forja de subs, ou pagar
um re-sorteio.

#### Resetar aprimoramentos = resetar o NÍVEL

Sem mecânica nova: **o nível volta a 0 e sobe de novo**; os aprimoramentos re-sorteiam sozinhos,
porque são consequência dos marcos e não algo guardado à parte. O custo é refazer os níveis, pagando o
material outra vez — **um preço que já existe**.

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
**Por isso a ideia das MISSÕES POR CHAMP é melhor que duplicata:** missão é conteúdo, duplicata é
imposto de tempo.

---

## 5. CAMPANHA

- **Ao criar a conta o jogador escolhe UM dos 4 humanos**, em vez de receber os quatro.
- **A partir do capítulo 2, toda fase é contra 4 inimigos** — acaba o crescente.
- **O campeão continua caindo GARANTIDO na fase dele.** A ordem de descoberta por fase fica como está
  — ela vale mais que a uniformidade, e sem "chance" some junto a pergunta "e se cair de novo?".
- **A tela precisa mostrar o que pode ser obtido em cada fase** (campeões e itens).

### A fase 1 entrega DOIS champs — e é só uma linha pra consertar

`ApostlesWar.Domain/Models/Campanha.cs:12` monta a fase 1 como `Rodada1 = [Slot1]` e
`Rodada2 = [Slot2]`, e o `DesbloquearCampeoes` varre as duas rodadas. **Deixar as duas iguais**
(`[Slot1]` / `[Slot1]`) resolve, e não abre buraco nenhum — os outros slots já aparecem sozinhos
adiante:

```
fase 1 → Slot1     fase 2 → Slot2     fase 3 → Slot3     fase 7 → Slot4
```

### Os inimigos não têm itens — e isso resolve a calibragem

Ideia do Gabriel: o inimigo pode ser **nível 250 com muito mais vida e status**, e o jogador vence por
**velocidade e sinergia de time**, não por status bruto.

> **Cuidado:** mostrar o nível ao jogador é bom (comunica ameaça), mas a conta deve continuar sendo
> **multiplicador de status**, não um nível de verdade. Duas fórmulas de poder pra manter em acordo
> divergem — é a mesma armadilha de "duas cópias de um número".

**A dificuldade só pode ser calibrada DEPOIS disto.** A fórmula atual (`1,75×dif + 0,5×cap + 0,1×fase`)
multiplica o inimigo contra um jogador que **não tinha progressão**. Com nível, raridade e o
equipamento do campeão, o jogador cresce também — a curva tem que ser desenhada contra as duas.

---

## 6. A BANCADA 2.0 — time contra time

A bancada atual mede **champ contra boneco parado**. Com posição, velocidade e tipos, ela fica ainda
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
   por facção. Arrasta sobrescrever habilidade nos capítulos com dois champs do mesmo papel.
5. **Nível (curva do tipo) + Raridade** nos campeões. Sem estrela.
6. **Raridade → passiva que escala.**
7. **Item equipado no campeão.**

> **O save atual é DESCARTADO** (decisão do Gabriel: *"descarta, não me importo"*). Sem migração.

### DEPOIS

Subestatísticas e aprimoramento · conjuntos 2/4/6 · drop por fase e dificuldade · a tela do "o que cai
onde" · escolher 1 champ inicial · **as barras de uso, os marcos e a forja** · **missões por campeão**
· a fase 1 entregando um champ só · **a dificuldade, agora calibrada contra a progressão** · **a
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
- **O campeão não tem estrela.** A raridade é o teto de nível dele.
- **Stat base é do tipo**, não do champ; o nível sobe por **curva do tipo**, sem pontos distribuíveis.
- **Velocidade não escala com nível** — vem de equipamento.
- **Subestatísticas iguais** no drop e na evolução; nenhum privilégio de nascença pro drop.
- **Duas barras** (estrela e raridade), mesma fonte, ritmos próprios — não é escolha de investimento.
- A barra é do **item**, não do campeão.
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
- **Campeão dropa garantido** na fase dele.
- Nada de **forçar o uso** de um campeão específico pra passar de fase.

### Revogado nesta rodada (estava escrito aqui e não vale mais)

| dizia | vale agora | por quê |
|---|---|---|
| *"na campanha só caem comuns"* | qualquer raridade cai em qualquer dificuldade | reservar raridade pra loja é desenho de gacha, e aqui não há loja |
| *"a fonte decide o teto de raridade"* (Fácil até Incomum…) | a **estrela** é o teto | virou redundante — a estrela já limita, e piso por dificuldade não acrescentava nada |
| *"fusão com semente"* | **sacrifício da raridade atual** alimentando 3 opções de sub | o sacrifício dá custo, liga o drop à evolução e ainda decide as opções |
| *"subir estrela não zera o nível"* | **subir raridade** não zera o nível | o campeão perdeu a estrela; some uma exceção, porque essa já era a regra do item |
| *"campeão tem chance de cair"* | cai **garantido** | apaga junto a pergunta "e se cair de novo?" |
| *"a ordem dos inimigos nas fases muda"* | fica como está | a descoberta por fase vale mais que a uniformidade |
