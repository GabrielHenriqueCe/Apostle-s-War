# PROPOSTA — nível e raridade: item e apóstolo sem estrela (ago/2026)

> **O modelo decidido em 17/ago/2026.** O `GDD-itens.md` e o `GDD-progressao.md` seguem sendo a versão
> oficial até serem atualizados — o que muda neles está em §O que isto revoga nos docs atuais.
>
> Só o **item** perdeu eixo; o apóstolo entra igual. O que foi considerado e cortado está no `git log`.

---

# O MODELO

## Os dois eixos, nos dois objetos

| | **apóstolo** | **item** |
|---|---|---|
| moeda | **alma** | **pó / metal** |
| **raridade** | teto de nível · quantos slots veste · degrau da passiva | teto de nível · quantas subs |
| **nível** (1–60) | os stats base | o principal e o valor de cada sub |
| teto de nível | comum 10 · incomum 20 · raro 30 · épico 40 · lendário 50 · mítico 60 | idem |
| sobe de nível | jogando (XP); material **acelera** | jogando (uso); material **acelera** |
| sobe de raridade | alma + (missão, opcional) | **sacrifício de peça igual** + material |

## A MAGNITUDE — o principal por nível

```
fatorNível = 10 + 1,5 × nível
```

| nível | 10 | 20 | 30 | 40 | 50 | 60 |
|---|--:|--:|--:|--:|--:|--:|
| principal | 25% | 40% | 55% | 70% | 85% | **100%** |
| é o teto de | comum | incomum | raro | épico | lendário | mítico |

São os seis valores do antigo `fatorEstrela`, na mesma ordem, caindo exatamente nos seis tetos de
raridade — **nenhum número do GDD muda**.

> **Um mítico nível 1 tem principal 11,5%**, pior que um comum no teto (25%). Está certo: mítico
> fresco é investimento, não upgrade instantâneo. A ficha tem de deixar isso óbvio, senão o drop raro
> parece ter vindo quebrado.

## O XP

- **Ganha-se por inimigo morto nas ondas**, não por vitória: perder a fase só deixa de ganhar o resto
  do que ainda ia cair.
- **Divide entre os 4 em campo**, igual, independente de quem bateu.
- **Não existe banco.** Pra subir, o fraco tem de **estar em campo** — mesmo atrapalhando. Carregar um
  recruta custa um dos 4 slots, e é esse o preço.
- O teto de nível continua sendo o da **raridade**: o carry acelera até o teto e para.

## O material

- **6 raridades**, as mesmas do resto do jogo. **Pó/metal** pro item, **alma** pro apóstolo.
- **Pó cai na fase**, junto com os itens. **Alma cai por INIMIGO derrotado**, não por fase: a oferta
  escala com o quanto se joga, e a **Arena alimenta o elenco** sem regra nova.
- **Fusão 10:1 pra cima**, travada pelo **teto de raridade da dificuldade mais alta vencida** (não se
  fabrica pó mítico farmando o Fácil).
- **Diluição 1:5 pra baixo.** As duas perdem: o ida-e-volta perde metade, então **ter a moeda certa
  vale mais que ter volume**.
- **Desmontar** item devolve material da raridade dele, com perda, e nunca acima do que a peça
  poderia produzir. É o que mantém o drop comum valendo no Pesadelo.
- **Material acelera o nível** (vale XP, mais por faixa). Não precisa de trava anti-abuso: o material
  que mais acelera é o mesmo que os pedágios exigem, então queimá-lo como XP **rouba do próprio teto**.

## O PEDÁGIO — material a cada 10 níveis

| pedágio no nível | custo | destrava até |
|:-:|---|:-:|
| **10** | comum + incomum | 20 |
| **20** | incomum + raro | 30 |
| **30** | raro + épico | 40 |
| **40** | épico + lendário | 50 |
| **50** | lendário + mítico | **60** |

A receita é **muito da faixa atual + pouco da próxima** — a mesma frase do degrau de raridade, o que
faz o jogador aprender uma regra e usá-la em dois lugares. O pó mítico tem só dois endereços no jogo
inteiro: este pedágio e o último degrau de raridade.

> **Não trava no 59:** o pedágio do 50 compra a dezena inteira. Um custo próprio pra pisar no 60 cabe
> e não quebra nada — custaria mítico puro, e pó mítico só cai no Pesadelo, onde o 60 já é permitido.
> É sabor, não estrutura.

## O teto de dificuldade

| dificuldade | pó que cai até | último pedágio pagável | teto de nível | teto de raridade |
|---|---|:-:|:-:|:-:|
| Fácil | raro | @20 | **30** | raro (**30**) |
| Normal | épico | @30 | **40** | épico (**40**) |
| Difícil | lendário | @40 | **50** | lendário (**50**) |
| Pesadelo | mítico | @50 | **60** | mítico (**60**) |

As duas últimas colunas batem nas quatro linhas: nenhuma trava escrita à mão, o mesmo teto dito duas
vezes por caminhos independentes.

## A RARIDADE — sacrifício + material

A raridade entrega as subs de uma vez:

| raridade | comum | incomum | raro | épico | lendário | mítico |
|---|:-:|:-:|:-:|:-:|:-:|:-:|
| subs | 0 | 1 | 2 | 3 | 4 | **5** |

- **Sacrifica a MESMA peça, do MESMO conjunto, na raridade ATUAL**, mais material (muito da atual +
  pouco da alvo). Sobe um degrau: mais teto de nível e mais uma sub.
- **O "pouco da raridade ALVO" é o que trava a dificuldade.** Sem ele, o Fácil fabricaria épico só
  sacrificando raros. Não é decoração da receita — é o portão.
- **As subs dos sacrificados viram o pool**: 3 opções, escolhe 1, recusa por slot. Guardar peça com a
  sub certa pra forçar as opções é craft direcionado, e continua sendo feature.
- **Reforjar um mítico** — sacrificar mítico em cima de mítico — **re-sorteia as 5 subs**, com as
  mesmas 3 opções e a mesma recusa por slot da promoção. É o endgame da peça depois do mítico 60.
- O apóstolo usa alma. **A missão** do `GDD-progressao.md` é **opcional**: conteúdo aditivo, não
  estrutura, e dá pra reintroduzir depois em um ou dois degraus sem mexer em modelo nenhum.

## AS SUBS

- **A mesma sub pode REPETIR no item.** Era o aprimoramento que empilhava em cima de uma sub; sem ele,
  a repetição é o único jeito de concentrar. Sem teto de cópias: 5 iguais é build, não exploit.
- **O pool do sacrifício pode oferecer sub que o item JÁ tem** — é o que torna a concentração
  comprável, com as 3 opções e a recusa por slot de sempre.
- **`sub ≠ principal` continua**, e continua sendo da forma exata (`ATK cheio` de principal aceita
  `ATK%` de sub).
- **O valor da sub tem de ser recalibrado.** O `principal ÷ 7` do GDD saía de 6 rolagens numa sub (a
  inicial + 5 aprimoramentos). O pior caso agora são **5 cópias**, então a divisão que preserva
  "principal > sub concentrada" é `principal ÷ 6` — mesmo raciocínio, número novo.

> **O freio do acúmulo mudou de natureza, e o balanço precisa saber.** O argumento velho era
> probabilístico (*"o RNG escolhe o slot do aprimoramento; concentrar os 5 numa é ~0,1%"*). Agora quem
> escolhe é a forja: 5× a mesma sub é **alcançável de propósito**, e o freio é só custo.

## O DROP

- **4 itens por fase**, só o slot da fase — arma os quatro do time numa corrida, que é a razão do 4.
- **Raridade sorteada direto na faixa da dificuldade**, que já está escrita no GDD e sobrevive intacta.
- **O `N` do sacrifício sobe junto:** 4× de drop barateia a promoção em 4× se o número não acompanhar.
- **A taxa de drop do mítico cai**, porque ele passou a ser também o combustível do reforge.

## O APÓSTOLO

- **A raridade dele é onde mora o desbloqueio de slots** — comum veste só os 3 fixos, mítico veste os
  9 (3 fixos → 4 variáveis → 2 acessórios, e os acessórios saem de dungeon, que já é conteúdo tardio).
  Fica visível por que um comum é fraco, sem número escondido.
- **Alma sobe a RARIDADE sem passar por campo.** O NÍVEL, não: esse cobra um dos 4 slots (§O XP).

> **O risco de escala, e é o maior do documento:** são **36 apóstolos** contra os poucos itens que
> alguém leva até o fim. A demanda de alma é muito mais larga que a de pó. Calibrar alma com a cabeça
> de material de item congela o elenco inteiro.

---

# Os números que faltam

1. **Quanto material por pedágio**, e a curva entre eles — o @50 é o mais caro do jogo.
2. **O `N` do sacrifício** por degrau de raridade. Com 4 drops por corrida e 5 degraus, dá ~3 corridas
   por peça se o `N` ficar no chute antigo de 2–3; parece barato demais pro topo.
3. **Quanto XP cada faixa de material vale** ao ser queimada como acelerador.
4. **A curva de pontos por rodada** — a antiga era por degrau de estrela (100/200/400/800/1.600) e
   agora são 60 níveis. O formato que preserva a intenção é dobrar por faixa de 10, mantendo o ponto
   por rodada escalando com a dificuldade.
5. **A demanda de alma** contra 36 apóstolos (ver §O APÓSTOLO).
6. **A taxa de drop do mítico**, agora que o reforge consome mítico (ver §O DROP).
7. **A rolagem da sub** — `principal ÷ 6` é a proposta, contra o `÷ 7` que o GDD calcula hoje.

# O que isto revoga nos docs atuais

No `GDD-itens.md`: a **estrela** como eixo e o `fatorEstrela`; o **marco por fase** e as três travas em
cadeia; a trava `raridade ≤ estrela`; a tabela **raridade → teto de aprimoramento** (`+4/+9/+14/+19/+20`);
a frase *"forja não compra magnitude"*; o **drop em dois passos**; a seção inteira do aprimoramento e o
reset; a regra **"não repete no mesmo item"**; a fórmula `rolagem de sub = principal ÷ 7` e o parágrafo
do freio probabilístico do acúmulo. **A forja de subs FICA** — o pool, as 3 opções e a recusa por slot
são o que sobrou de pé.

No `GDD-progressao.md`: **§O APÓSTOLO NÃO TEM ESTRELA** continua com a conclusão certa (ele não tem)
mas com o argumento errado — quem o mata agora é o item ter perdido a estrela também. A **missão**
deixa de ser estrutura e vira conteúdo opcional.

> **E uma contradição que já existe no GDD hoje, independente desta proposta:** uma linha diz que a 5ª
> unidade do mítico é *"o bônus de nascença"* e outra, quinze linhas abaixo, diz que *"esse bônus não
> cai no drop, só se conquista evoluindo"*. As duas não podem ser verdade — mítico cai no drop. A
> segunda é a que está velha.
