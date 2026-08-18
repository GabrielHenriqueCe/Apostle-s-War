# GDD — COMBATE: os stats novos, a barra de turno e o tabuleiro (§1 e §2)

> **Tipo:** MODELO (referência viva). Descreve a REGRA, não o trabalho — `DEF/(DEF+5000)` segue
>   valendo depois de implementada, e apagá-la deixaria o código como única fonte dela.
> **Função:** o que cada stat significa, como a ordem de turno é decidida e o que a posição modula.
> **Onde está a FILA:** o §7 do `GDD-progressao.md` — a ordem dos passos, que não é negociável.

> **A NUMERAÇÃO É A DO GDD ORIGINAL**, e ela vale nos três arquivos — uma referência a "§4" quer
> dizer a mesma coisa em qualquer um deles:
>
> | § | assunto | arquivo |
> |---|---|---|
> | §1 §2 | stats novos, barra de turno, posição e tipo | `GDD-combate.md` |
> | §3 §5 §6 §7 | nível e raridade, campanha, a bancada 2.0, o plano, as decisões fechadas | `GDD-progressao.md` |
> | §4 | itens | `GDD-itens.md` |

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

> **⚠️ Esta tabela DIVERGE da de §2, que é a autoritativa** (a calibrada do nv 60 pra trás). Lá o
> Suporte no nv 60 é **105**, não 110 — ele é o único dos quatro que não sobe +5. Ou é engano de
> digitação aqui, ou o +5 dele foi cortado de propósito e esta tabela ficou velha. **Pendente de
> decisão**; até lá, §2 manda. Duplicar a tabela foi o que permitiu a divergência existir.

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

#### COMO A BARRA FUNCIONA — o modelo fechado

**Cada apóstolo tem a PRÓPRIA barra**, de 0 a 100, e enche pela própria Velocidade. Não existe
relógio compartilhado que alguém precise consultar sobre os outros: se um ganha ou perde Velocidade
no meio da luta, muda só o ritmo dele.

- **Age quem cruzou 100.** Entre os prontos, o **mais cheio**.
- **Desempate:** a **posição** (a frente primeiro) e, se ainda empatar, o **lado do jogador**.
- **Ao agir, desconta 100 e a sobra CARREGA** — empurrão nunca se desperdiça.
- **A ação custa tempo**, e é durante esse tempo que cada um enche pela própria Velocidade.

**O desempate precisa ser determinístico, e o critério é arbitrário de propósito.** Empate perfeito
só acontece com Velocidade idêntica; o que importa é que o mesmo estado produza sempre a mesma
ordem, senão a prévia da fila promete o que o motor não cumpre.

> **A regra do MAIS CHEIO não se troca por velocidade.** Chegou-se a propor que, entre os prontos,
> agisse o mais RÁPIDO — assim um inimigo veloz furaria um empurrão sem depender do relógio. Está
> descartado: fazia alguém em **105% jogar antes de alguém em 133%**, e aí a barra desenhada na tela
> vira mentira, que é o defeito que este desenho inteiro existe pra consertar. Quem quiser passar na
> frente **passa no medidor**.

#### O CUSTO DA AÇÃO — e por que ele não é opcional

Sem ele o jogo congela entre uma ação e outra: quem está esperando não anda, e **nenhuma velocidade
alcança ninguém**. Um apóstolo de 1000 de Velocidade parado atrás de um empurrão fica exatamente
onde está enquanto os outros jogam. É o custo que dá à Velocidade um intervalo onde agir.

**A constante é ADIMENSIONAL**, e é isto que se guarda:

```
FRAÇÃO = 10%                              uma ação dura 10% do ciclo de um apóstolo de referência
custo  = FRAÇÃO × 100 / VEL_REFERENCIA    (com VEL_REFERENCIA = 200, dá 0,05)
```

**Gravar `0,05` é gravar o número certo na unidade errada** — ele carrega escondido a suposição
*"Velocidade típica ≈ 200"* e quebra se um dia a escala do jogo mudar. Com a fração, dobrar a escala
é trocar **uma** constante: a ordem dos turnos sai idêntica em 200, 400 ou 2000.

**O teto de estabilidade tem leitura direta:** cada ação faz o campo gerar `Σvel × custo` de medidor
e consumir 100. Passando de 100, as barras inflam pra sempre e todo mundo vive acima da linha — o
trilho de 0 a 100 perde o sentido. Em fração: **`FRAÇÃO < 100 ÷ (nº em campo)`**, ou seja **12,5%**
num 4×4. Os 10% de hoje têm um quinto de folga; se a luta virar 5×5, o teto cai pra 10%.

> **Uma versão anterior derivava o custo de `Σvel` a cada uso**, pra garantir a estabilidade sozinha.
> Descartado: obriga cada apóstolo a saber quem mais está vivo, quando a regra é justamente que cada
> um se calcula. E era desnecessário — medido, o pior caso realista (os 8 no teto de Velocidade) fica
> dentro da folga.

#### VELOCIDADE É, LITERALMENTE, QUANTOS TURNOS SE JOGA

A razão de turnos entre dois combatentes é **exatamente** a razão das Velocidades, e o custo da ação
não distorce isso — ele entrega medidor proporcional à Velocidade de cada um, então o fator sai da
divisão. **1000 contra 100 é 10 turnos para 1, cravado.** O custo muda só **onde** os turnos caem na
sequência, nunca quantos.

Isso torna o item de Velocidade o único que **multiplica todo o resto**: ATK aumenta o dano por
golpe; a Bota aumenta quantos golpes se dá, e portanto multiplica ATK, crítico, cura e aplicação de
malefício ao mesmo tempo.

**E o inimigo fica parado nesse eixo.** Ele não tem item (§5) e Velocidade não escala com nível
(§4), então a Velocidade inimiga é **85–115 no jogo inteiro** enquanto a do jogador chega a 315 (nv
60, Atirador, 6★ mítico +20 com a Bota e as subs). No fim da progressão o time joga **~3 turnos para
cada 1** do inimigo, para sempre. Vale saber ao calibrar dificuldade: é vantagem estrutural, não
curva.

#### O EMPURRÃO DE MEDIDOR — a tabela de consulta

Não há um número global: **cada habilidade tem o seu**, como multiplicador de dano. O que a medição
dá é o que cada tamanho compra, num time full mítico (178–203) com o custo em 10%:

| **em área** | turnos seguidos que compra | | **alvo único** | turnos seguidos |
|---|--:|---|---|--:|
| +10 | 1,5 | | +50 | 1,6 |
| +20 | 1,9 | | +100 | 1,8 |
| +30 | 2,3 | | | |
| +50 | 3,0 | | | |

**Em área custa ~25 de empurrão por turno seguido comprado**, e é uma reta. **Alvo único compra
muito menos tempo** — só empurra um acima de 100 — mas faz outra coisa: **escolhe quem joga**,
adiantando o nuker pra o combo sair na janela certa. São dois formatos com propósitos diferentes,
não um forte e um fraco.

> **Cortar não é mecânica — é consequência, e não há taxa a calibrar.** É só alguém ser mais rápido
> e chegar a vez dele. Medido, um inimigo entra no meio de um empurrão em **1–4% dos casos**, e isso
> não responde a tamanho de empurrão, a custo nem a Velocidade. O trabalho de design não é ajustar
> quanto o corte acontece: é deixar a **ordem legível**, pra que quando acontecer o jogador veja que
> foi a montagem dele e possa afinar. É o que a fila na tela resolve.

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
> efeito pela metade); ao **clicar** nela, cada alvo válido mostra o seu `chance de aplicar: 75%` — e
> essa linha **some** quando chega a 100%, que é o estado que o jogador persegue. **É no clique e em
> todos os alvos, nunca no hover de um só:** em touch não existe hover.
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

## 2. POSIÇÃO E TIPO — e por que uma não existe sem a outra

### A posição não PROÍBE nada — ela muda o QUANTO

Quatro casas por lado e **as frentes se olhando**: o tabuleiro é uma fila de 8, cada time de costas
pra sua borda.

```
        SEU TIME                          INIMIGO
   ┌────┬────┬────┬────┐            ┌────┬────┬────┬────┐
   │ 4  │ 3  │ 2  │ 1  │            │ 1  │ 2  │ 3  │ 4  │
   └────┴────┴────┴────┘            └────┴────┴────┴────┘
    fundo ........ frente            frente ........ fundo
```

A distância é **quantas casas separam os dois nessa fila** — frente contra frente é 1, fundo contra
fundo é 7:

```
distância = minha casa + casa do alvo − 1
```

#### Cada tipo tem uma DISTÂNCIA IDEAL

O dano é máximo nela e cai **0,10 por casa de desvio, pros dois lados**. Um número por tipo, e nada
mais:

| tipo | d\* | d1 | d2 | d3 | d4 | d5 | d6 | d7 |
|---|:--:|--:|--:|--:|--:|--:|--:|--:|
| 🛡️ Guardião | **1** | **1,30** | 1,20 | 1,10 | 1,00 | 0,90 | 0,80 | 0,70 |
| ⚔️ Combatente | **4** | 1,00 | 1,10 | 1,20 | **1,30** | 1,20 | 1,10 | 1,00 |
| 🏹 Atirador | **5** | 0,90 | 1,00 | 1,10 | 1,20 | **1,30** | 1,20 | 1,10 |
| 💗 Suporte | — | 1,00 | 1,00 | 1,00 | 1,00 | 1,00 | 1,00 | 1,00 |

**Nenhuma habilidade é proibida em lugar nenhum.** Todo mundo alcança todo mundo, sempre — a casa
decide só o quanto. **O Suporte é o único que não liga pra onde está**, o que combina com quem cura e
limpa: ele já tem com o que se preocupar.

Das casas naturais, sem ninguém andar, cada um cai em cima de um alvo **diferente**:

| quem | da casa | pica em | ou seja |
|---|:--:|:--:|---|
| 🛡️ Guardião | 1 | casa 1 | o tanque deles |
| ⚔️ Combatente | 2 | casa 3 | o suporte deles |
| 🏹 Atirador | 4 | casa 2 | o combatente deles |
| 💗 Suporte | 3 | — | igual em todo mundo |

**E sobra um buraco de propósito: ninguém pica no arqueiro deles (casa 4) parado.** Pra alcançá-lo
você avança o Combatente pra casa 1 ou o Atirador pra casa 2. Quer matar o maior ATK do campo? Expõe
alguém. É essa a decisão, e ela é toda partida.

#### O pico ANDA junto com quem se move

Como o que vale é a distância, mudar de casa **muda o alvo preferido** — não só a força:

| o arqueiro na… | o pico dele cai em… |
|---|---|
| casa 4 | casa 2 — o combatente deles |
| casa 3 | casa 3 — o suporte deles |
| casa 2 | casa 4 — **o arqueiro deles** |
| casa 1 | *fora do tabuleiro* — da frente ele alcança 4 de distância no máximo, e ele quer 5 |

**Recuar aproxima o pico da frente inimiga; avançar joga o pico pro fundo dela.** É contraintuitivo
na primeira leitura e vira segunda natureza na terceira partida — e é uma decisão real, porque
avançar o frágil pra caçar o frágil deles é pôr o frágil na frente.

> **O Atirador é o único que não atinge o próprio pico estando na casa 1**: da frente a maior
> distância possível é 4, e ele quer 5 (o Combatente, que quer 4, atinge). Isso é aceito de propósito
> — baixar o d\* dele pra 4 o empataria com o Combatente, e dois tipos com a mesma curva não são dois
> tipos.

#### Quem morre FICA na casa

**As fileiras NÃO compactam.** O corpo ocupa a casa dele até o fim da luta — e não é limitação, é o
que o jogo já pede: **existem apóstolos que revivem**, então a casa tem que estar lá esperando.

Isso fecha a pergunta que ficou aberta duas rodadas ("o que acontece com a posição quando alguém
morre") e aposenta junto o **⚔️ Atacar universal** que era o remendo proposto pra ela: ele existia pra
ninguém ficar travado sem habilidade válida, e neste modelo **ninguém trava nunca**.

> É a resposta oposta à do DD, e de graça. Lá as fileiras deslizam pra frente quando alguém morre, e
> o DD1 precisou inventar **cadáveres** justamente pra controlar o quanto elas andam — mecânica
> polêmica o bastante pra virar opção que se desliga.

#### O tamanho disso: a posição é TEMPERO, não motor

Medido no nv 60, com a `DEF/(DEF+5000)` e os dois times na formação natural:

| casa | quem está lá | bruto que chega | depois da DEF dele | HP | **rodadas até morrer** |
|---|---|--:|--:|--:|--:|
| 1 | 🛡️ Guardião | 4.758 | 3.660 | 30.000 | **8,2** |
| 2 | ⚔️ Combatente | **4.992** | 4.025 | 25.200 | **6,3** |
| 3 | 💗 Suporte | 4.926 | 4.132 | 20.100 | **4,9** |
| 4 | 🏹 Atirador | **4.590** | 4.165 | 15.000 | **3,6** |

**A casa 4 é a que recebe MENOS dano bruto de todas — e o Atirador morre lá mais que duas vezes mais
rápido que o Guardião.** A casa não protegeu nada; quem decidiu foi a ficha.

Dá pra medir o tamanho: o bruto varia **4%** entre a pior casa e a melhor; o HP efetivo varia **136%**
entre o Atirador e o Guardião. **A ficha do tipo pesa umas trinta vezes mais que a posição** — e é
assim que deve ser. Os 12 números da tabela abaixo são o esqueleto; a posição é tempero.

> **Consequência que vale lembrar ao desenhar kit:** o Guardião **não** protege o time por estar na
> casa 1 — 4% de dano desviado é nada. Ele protege **puxando o golpe pra si** (`ProtecaoAliado`), e
> isso vale 2,4× de HP efetivo. A geometria dá sabor; a proteção de verdade continua sendo do kit.
>
> Se um dia a posição precisar morder de verdade, o botão é a queda por casa (0,10 → 0,20). Mas
> calibrar isso antes de os itens existirem é calibrar contra número que ainda não tem dono.

#### Na tela: cor e multiplicador, nunca dano previsto

O mapa de calor é **um componente com três gatilhos** — passar o mouse, selecionar (o clique, que é o
que salva quem não usa hover) e arrastar entre as casas. Os três pintam a mesma coisa: as casas
inimigas tingidas pelo multiplicador de quem está em foco, **com o número escrito junto** (`×1,20`).
A escala diverge no 1,00 — abaixo esfria e apaga, acima acende e esquenta.

**O front NÃO pode ter cópia da tabela.** O C# manda a **grade 4×4 por apóstolo** (a casa onde ele
poderia estar × a casa do alvo): 16 números por apóstolo, e o arraste vira troca de linha numa grade
que já está na mão, sem ida e volta na ponte no meio do gesto. Duas cópias de uma fórmula divergem
como duas cópias de um número — e essa já custou um defeito mudo nos Decaídos.

> **NUNCA mostrar o dano que a habilidade vai causar antes de usá-la** (decisão do Gabriel). O
> multiplicador é **ficha** — é atributo do apóstolo naquela casa, igual a ATK. Dano previsto é
> simulação, e simulação tira a decisão do jogador. O mapa vai apontar pro tanque deles, que é o pior
> alvo do jogo pra matar; *"a anta que ficar focando no tank tem que se fuder"*. O `PreverDanoRecebido`
> do motor é ferramenta interna (bot, ordem de morte) e continua fora da tela.

### O TIPO é identidade — e agora também é geometria

A ficha (o que ele aguenta e o quanto bate) é **identidade**; a curva de distância é o **gesto** dele
no tabuleiro. As duas juntas é que fazem o jogador ler o time antes de montar.

| tipo | casa natural | papel |
|---|:--:|---|
| 🛡️ **Guardião** | 1 | aguenta, protege puxando o golpe, e só rende colado |
| ⚔️ **Combatente** | 2 | atravessa a linha e castiga o miolo |
| 🏹 **Atirador** | 4 | o maior alcance do campo |
| 💗 **Suporte** | 3 | cura, buff e malefício — e rende igual em qualquer casa |

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

