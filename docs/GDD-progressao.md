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

> **Onde ela passa a ser balanceada:** na tabela de itens, não na ficha do champ. O risco é todo mundo
> montar velocidade em tudo, e o freio clássico (Raid faz assim) é **concentrá-la num slot** — as
> botas. Aí ela custa um slot, e o custo de oportunidade resolve sozinho. Decisão adiada de propósito.
>
> **De graça:** como item é trocável, qualquer champ pode virar rápido se você pagar por isso —
> flexibilidade sem apagar a identidade do tipo.

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
- **CONJUNTOS**: bônus por peças equipadas, e o que cada conjunto faz cresce com a raridade.

> **O problema do número ímpar (7 peças) tem saída barata:** bônus em **2/4/6** e o **acessório fora
> do conjunto** — um slot livre. Resolve hoje, sem mexer no boneco e sem inventar 3 acessórios (que
> segue como ideia futura, levando o conjunto a 9).

### Os três eixos do item — e cada um com sua fonte de custo

| eixo | o que dá | como sobe | o que custa |
|---|---|---|---|
| **raridade** | QUANTAS subestatísticas (+ teto de nível) | barra de uso (lenta) + marco da fase + forja | sacrificar itens da raridade atual |
| **nível** | QUÃO BOAS elas são (aprimoramento) | forja, a qualquer momento | material / moeda |
| **estrela** | MAGNITUDE (principal e subs) | barra de uso (rápida) + marco da fase | tempo de jogo |

> **O marco é o mesmo para os dois** — vencer a fase de origem do item naquela dificuldade destrava
> estrela **e** raridade, no teto daquela dificuldade. Uma condição, dois eixos.

> **A moeda que ficou parqueada já tem dono: é o nível.** Não é preciso procurar função pra ela.

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
2. **Aptidão × Resistência + retornos decrescentes.**
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
