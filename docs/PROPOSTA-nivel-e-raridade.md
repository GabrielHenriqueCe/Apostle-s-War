# PROPOSTA — nível e raridade: item e apóstolo sem estrela (ago/2026)

> **A V2 VENCEU** — decisão do Gabriel, 17/ago/2026. **Mas o corpo deste arquivo ainda está escrito
> como se as duas estivessem em aberto**, e a reescrita é pendência: promover a V2 a modelo, rebaixar a
> V1 a *"considerado, e por que não"*, e acrescentar o endgame (reforjar um mítico re-sorteia as 5
> subs). Até isso acontecer, o `GDD-itens.md` e o `GDD-progressao.md` seguem sendo a versão oficial.
>
> O tronco (§O TRONCO) vale nas duas — **o material entra nas duas igual**. A diferença está só no
> §A BIFURCAÇÃO, e só no **ITEM**: o apóstolo é idêntico nas duas versões.

## De onde isso veio

Duas ideias do Gabriel: **(1)** subir estrela de item não custava recurso nenhum, só nível e marco —
devia custar **material**, e material com raridade própria; **(2)** o mesmo sistema devia servir pro
apóstolo, com **almas** no lugar do pó — o que resolve o *"empilhar cópias pra sacrifício"* sem
recorrer a partes de personagem, que ele não quer.

Puxando o fio, três eixos caíram. Nenhum foi cortado por gosto — os três estavam **vazios**:

- **a estrela do item** era o nível medido em passos de 10 (a tabela de §A MAGNITUDE prova);
- **o marco de fase** existia pra travar a estrela — sem estrela, não tem o que travar. A âncora de
  LUGAR que ele dava sobrevive no **sacrifício**, que já exige a mesma peça do mesmo conjunto;
- **o aprimoramento** entregava "unidades de sub", e até o lendário unidade e sub são o mesmo número —
  que a raridade já dizia. Só a **V2** mata este.

---

# O TRONCO — o que vale nas duas versões

## Os dois eixos, nos dois objetos

| | **apóstolo** | **item** |
|---|---|---|
| moeda | **alma** | **pó / metal** |
| **raridade** | teto de nível · quantos slots veste · degrau da passiva | teto de nível · quantas subs |
| **nível** (1–60) | os stats base | o principal e o valor de cada sub |
| teto de nível | comum 10 · incomum 20 · raro 30 · épico 40 · lendário 50 · mítico 60 | idem |
| sobe de nível | jogando (XP); material **acelera** | jogando (uso); material **acelera** |
| sobe de raridade | alma + (missão, opcional) | **sacrifício de peça igual** + material |

## A MAGNITUDE — a tabela da estrela cabe no nível sem recalibrar nada

```
fatorNível = 10 + 1,5 × nível
```

| nível | 10 | 20 | 30 | 40 | 50 | 60 |
|---|--:|--:|--:|--:|--:|--:|
| principal | 25% | 40% | 55% | 70% | 85% | **100%** |
| é o teto de | comum | incomum | raro | épico | lendário | mítico |

São os seis valores do antigo `fatorEstrela`, na mesma ordem, caindo exatamente nos seis tetos de
raridade. **Nenhum número do GDD muda** — a estrela era o nível o tempo todo.

> **Um mítico nível 1 tem principal 11,5%**, pior que um comum no teto (25%). Está certo: mítico
> fresco é investimento, não upgrade instantâneo. A ficha tem de deixar isso óbvio, senão o drop raro
> parece ter vindo quebrado.

## O material

- **6 raridades**, as mesmas do resto do jogo. **Pó/metal** pro item, **alma** pro apóstolo.
- **Pó cai na fase**, junto com os itens. **Alma cai por INIMIGO derrotado**, não por fase: a oferta
  escala com o quanto se joga, a mesa de drop da fase não fica com três coisas competindo pela mesma
  leitura, e a **Arena alimenta o elenco** sem regra nova. É diegético — a guerra é entre apóstolos,
  então o derrotado deixa alma.
- **Fusão 10:1 pra cima**, travada pelo **teto de raridade da dificuldade mais alta vencida** (não se
  fabrica pó mítico farmando o Fácil).
- **Diluição 1:5 pra baixo.** As duas perdem: o ida-e-volta perde metade, então **ter a moeda certa
  vale mais que ter volume**.
- **Desmontar** item devolve material da raridade dele, com perda, e nunca acima do que a peça
  poderia produzir. É o que mantém o drop comum valendo no Pesadelo.
- **Material acelera o nível** (vale XP, mais por faixa). Não precisa de trava anti-abuso: o material
  que mais acelera é o mesmo que os pedágios exigem, então queimá-lo como XP **rouba do próprio teto**.

> **Material de raridade baixa nunca vira lixo**, e é por construção: todo item novo e todo apóstolo
> novo começa no nível 1 e precisa da faixa comum. A fusão vira socorro, não plano.

## O PEDÁGIO — material a cada 10 níveis

| pedágio no nível | custo | destrava até |
|:-:|---|:-:|
| **10** | comum + incomum | 20 |
| **20** | incomum + raro | 30 |
| **30** | raro + épico | 40 |
| **40** | épico + lendário | 50 |
| **50** | lendário + mítico | **60** |

A receita é **muito da faixa atual + pouco da próxima** — a mesma frase do degrau de raridade, o que
faz o jogador aprender uma regra e usá-la em dois lugares. E o pó mítico passa a ter só dois endereços
no jogo inteiro: este pedágio e o último degrau de raridade.

> **Não trava no 59:** o pedágio do 50 compra a dezena inteira. Se o Gabriel quiser um custo próprio
> pra pisar no 60, ele cabe e não quebra nada — custaria mítico puro, e pó mítico só cai no Pesadelo,
> onde o nível 60 já é permitido. É sabor, não estrutura.

## O teto de dificuldade cai de graça

| dificuldade | pó que cai até | último pedágio pagável | teto de nível | teto de raridade |
|---|---|:-:|:-:|:-:|
| Fácil | raro | @20 | **30** | raro (**30**) |
| Normal | épico | @30 | **40** | épico (**40**) |
| Difícil | lendário | @40 | **50** | lendário (**50**) |
| Pesadelo | mítico | @50 | **60** | mítico (**60**) |

As duas últimas colunas batem nas quatro linhas. **É isto que substitui o marco por fase**: nenhuma
trava escrita à mão, o mesmo teto dito duas vezes por caminhos independentes.

## A RARIDADE — sacrifício + material

- **Sacrifica a MESMA peça, do MESMO conjunto, na raridade ATUAL**, mais material (muito da atual +
  pouco da alvo). Sobe um degrau: mais teto de nível e mais uma sub.
- **O "pouco da raridade ALVO" é o que trava a dificuldade.** Sem ele, o Fácil fabricaria épico só
  sacrificando raros. Não é decoração da receita — é o portão.
- **As subs dos sacrificados viram o pool**: 3 opções, escolhe 1, recusa por slot. **Bônus, não eixo**
  — o desenho funciona sem, e o craft direcionado (guardar peça com a sub certa pra forçar as opções)
  continua sendo feature.
- O apóstolo usa alma. **A missão** do `GDD-progressao.md` vira **opcional**: é conteúdo aditivo, não
  estrutura, e dá pra reintroduzir depois em um ou dois degraus sem mexer em modelo nenhum.

## O DROP

- **4 itens por fase**, só o slot da fase — arma os quatro do time numa corrida, que é a razão do 4.
- **Raridade sorteada direto na faixa da dificuldade.** O sorteio em dois passos (estrela, depois
  raridade em `[comum..estrela]`) existia por causa da estrela; a faixa por dificuldade já está
  escrita no GDD e sobrevive intacta.
- **O `N` do sacrifício sobe junto:** 4× de drop barateia a promoção em 4× se o número não acompanhar.

## O APÓSTOLO

- **A raridade dele é onde mora o desbloqueio de slots** — comum veste só os 3 fixos, mítico veste os
  9 (3 fixos → 4 variáveis → 2 acessórios, e os acessórios saem de dungeon, que já é conteúdo tardio).
  Fica visível por que um comum é fraco, sem número escondido.
- Alma substitui a obrigação de bancar o time: dá pra subir um recruta sem tirar o main de campo.

> **O risco de escala, e é o maior do documento:** são **36 apóstolos** contra os poucos itens que
> alguém leva até o fim. A demanda de alma é muito mais larga que a de pó. Calibrar alma com a cabeça
> de material de item congela o elenco inteiro.

## A ESTRELA

**Fora.** Chegou a ser considerada como **ícone colorido em SVG** — 6 cores, com contagem e cor
redundantes (o que também serve daltônico), e emoji não serve porque `⭐` não tem cor variável. O
Gabriel gostou da leitura, mas concluiu que, sendo só decoração, é sem. **Se voltar, volta como UI,
nunca como eixo.**

---

# A BIFURCAÇÃO — a única diferença entre as duas

## V1 — o aprimoramento vive

Os pedágios fazem dois trabalhos: destravam a dezena **e** entregam uma unidade de sub.

| pedágio | unidade | raridade que para aí | subs |
|:-:|:-:|---|:-:|
| @10 | 1ª | incomum (nível 20) | 1 |
| @20 | 2ª | raro (30) | 2 |
| @30 | 3ª | épico (40) | 3 |
| @40 | 4ª | lendário (50) | 4 |
| @50 | 5ª | mítico (60) | 4 + 1 dobrada |

O comum nunca paga pedágio nenhum (trava no 10) → 0 unidades. O mítico chega a 5 unidades em 4 subs,
então **uma sub dobra e mostra `(2)`** — é a única coisa que o separa do lendário. O **reset** do
mítico re-sorteia **em quais subs as unidades caíram, mantendo o nível** (resetar o nível apagaria o
tempo jogado, que agora é o mesmo número do aprimoramento).

**O furo que a V1 tem:** do comum ao lendário as unidades só preenchem subs vazias — **nenhuma dobra,
nenhum marcador aparece**. "Quantas unidades" e "quantas subs" são o mesmo número o caminho todo, e
quem já dizia isso era a raridade. O pedágio cobra e entrega o que a raridade já entregou.

## V2 — o aprimoramento morre

Sobram **nível e raridade**, e nada mais. A raridade dá as subs de uma vez:

| raridade | subs |
|---|:-:|
| comum | 0 |
| incomum | 1 |
| raro | 2 |
| épico | 3 |
| lendário | 4 |
| mítico | **5** |

**O mítico ganha 5 subs direto**, em vez de 4 + uma unidade dobrada. Com isso somem o `+N`, os
marcadores `(1)`/`(2)`, o reset e o conceito de "unidade". Os pedágios continuam, mas são **só
pedágio** — pagam a dezena seguinte e nada mais.

**O que a V2 custa:** morre o endgame de perseguir a **distribuição** dos aprimoramentos entre os
slots (o `(5,0,0,0)` que era achado contra o `(2,1,1,1)` que não era). Depois do mítico nível 60 não
sobra o que caçar na peça. O Gabriel decidiu deixar assim — *"se eu pensar em algo algum dia a gente
vê"*.

## Lado a lado

| | **V1** | **V2** |
|---|---|---|
| eixos | nível · raridade · aprimoramento | **nível · raridade** |
| subs do mítico | 4 + 1 dobrada `(2)` | **5** |
| o pedágio entrega | dezena **+ unidade de sub** | dezena |
| marcadores `(1)`/`(2)` | só no mítico | **não existem** |
| reset | re-sorteia a distribuição | não existe |
| endgame da peça | caçar a distribuição | acaba no mítico 60 |
| simetria com o apóstolo | quase (o apóstolo não tem aprimoramento) | **exata** |

---

# Os números que faltam (nas duas)

1. **Quanto material por pedágio**, e a curva entre eles — o @50 é o mais caro do jogo.
2. **O `N` do sacrifício** por degrau de raridade. Com 4 drops por corrida e 5 degraus, dá ~3 corridas
   por peça se o `N` ficar no chute antigo de 2–3; parece barato demais pro topo.
3. **Quanto XP cada faixa de material vale** ao ser queimada como acelerador.
4. **A curva de pontos por rodada** — a antiga era por degrau de estrela (100/200/400/800/1.600) e
   agora são 60 níveis. O formato que preserva a intenção é dobrar por faixa de 10, mantendo o ponto
   por rodada escalando com a dificuldade.
5. **A demanda de alma** contra 36 apóstolos (ver §O APÓSTOLO).

# O que isto revogaria nos docs atuais

No `GDD-itens.md`: a **estrela** como eixo e o `fatorEstrela`; o **marco por fase** e as três travas em
cadeia; a trava `raridade ≤ estrela`; a tabela **raridade → teto de aprimoramento** (`+4/+9/+14/+19/+20`);
a frase *"forja não compra magnitude"*; o **drop em dois passos**; e, só na V2, a seção inteira do
aprimoramento, a forja de subs e o reset.

No `GDD-progressao.md`: **§O APÓSTOLO NÃO TEM ESTRELA** continua com a conclusão certa (ele não tem)
mas com o argumento errado — o eixo deixa de estar vazio quando a alma existe, e quem o mata agora é o
item ter perdido a estrela também. A **missão** deixa de ser estrutura e vira conteúdo opcional.

> **E uma contradição que já existe no GDD hoje, independente desta proposta:** uma linha diz que a 5ª
> unidade do mítico é *"o bônus de nascença"* e outra, quinze linhas abaixo, diz que *"esse bônus não
> cai no drop, só se conquista evoluindo"*. As duas não podem ser verdade — mítico cai no drop. A
> segunda é a que está velha, e junto com ela cai o argumento que ela sustentava (*"é ele que dá razão
> pra rota do item fraco existir"*).
