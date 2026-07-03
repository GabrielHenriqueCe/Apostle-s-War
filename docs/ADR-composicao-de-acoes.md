# ADR — Composição de Ações (habilidade como dado)

> **Tipo:** Architecture Decision Record
> **Status:** Aceito (decisões fechadas; implementação incremental a seguir)
> **Contexto:** Apostle's War. Consome a "Auditoria das habilidades ativas" que o
>   ROADMAP deixou em aberto — a auditoria encontrou dor real. É o fio predecessor
>   natural do Rebalanceamento e da reorganização pasta-por-champ.
> **Data:** julho/2026

---

## 1. Problema

Hoje cada habilidade ativa é uma **classe própria** (`Skills/Ativas/*.cs`, ~74
arquivos). Olhando o que essas classes de fato contêm, elas caem em três baldes:

- **Balde 1 (~70%) — 100% dado.** `BolaDeFogo`, `AtaqueBasico`, `Lealdade`,
  `Democracia`, `Incendio`... A classe inteira é: metadados
  (`NumeroDeAlvos`/`TipoAlvo`/`TipoLista`/`EstadoAlvo`) + um `Ativar` que faz
  `foreach` sobre `ResolverAlvos` aplicando uma **lista fixa de efeitos**. O que
  muda entre `BolaDeFogo` e `Incendio` é só: modo de alvo + lista de efeitos +
  números. Todo o resto (o `foreach`, o `ResolverAlvos`, montar a lista de
  `EventoDano`) é boilerplate copiado. **A classe não tem comportamento; tem
  configuração.**
- **Balde 2 (~20%) — dado + um tempero.** `Barata` (buff em si → ataca → *se
  matou*, aplica debuff no morto), `AnjoCaido` (revive mortos **e** cura vivos —
  dois efeitos em listas diferentes). Ainda é lista de efeitos, com um condicional
  ("ao matar") ou dois escopos.
- **Balde 3 (~10%) — bespoke de verdade.** `Copiando` (move instâncias de buff
  entre combatentes + turno extra, com trade-off de stat), `Putridao` (explode
  Veneno pra dano + cura média — agrega valor ao longo dos alvos). Isso é
  comportamento que uma lista de efeitos-padrão não expressa.

Escrever uma classe por habilidade do Balde 1 é o mesmo que escrever uma
`class Operario : Personagem` só pra guardar `HP = 1200` — dado fingindo de código.
Foi a fricção que Gabriel sentiu ("senti que reinventei o que já existe" ao criar
habilidade nova).

---

## 2. Decisão central

Uma habilidade vira **dado** (uma configuração de **Ações**) quando seu `Ativar` é
só "aplica esta lista fixa de efeitos". Ela continua **classe / Ação custom** quando
tem comportamento real que uma lista de efeitos-padrão não expressa.

O mesmo raciocínio que já se aplica ao **personagem** (que é dado — uma linha no
`PersonagemService`, não uma classe), levado um nível abaixo, pra **habilidade**.

**A unidade de reúso é a AÇÃO, não a habilidade.** Não se pergunta "essa habilidade
é única?"; decompõe-se a habilidade em ações e pergunta-se de cada ação "isso já
existe / outros vão precisar?". Quase toda habilidade "rica" decompõe em ações-padrão
+ no máximo uma ação nova pequena.

---

## 3. Anatomia de uma Ação

Uma ação combina três eixos independentes:

### 3.1 Operação (o quê)
`Dano`, `Cura`, `AplicarBuff`, `AplicarDebuff`, `RemoverDebuffs`, `RemoverBuffs`,
`MoverBuffs`, `Reviver`, `ConcederTurnoExtra`, `Transformar` (futuro)... O
vocabulário compartilhado do jogo.

### 3.2 Escopo (em quais COMBATENTES)
- **AlvosResolvidos** (default) — quem a habilidade selecionou/atingiu.
- **TodosAliados** — o time do atacante (que **inclui o próprio atacante** — ele
  faz parte do próprio time).
- **TodosInimigos** — o time adversário.
- **ProprioAtacante** — determinadamente só o conjurador (ex: `Barata` dá Intocável
  em si, sem escolher).

O escopo default herda os alvos da habilidade; ações que batem numa lista à parte
(ex: `DestruindoDia` limpa **aliados** enquanto ataca **inimigos**; `AnjoCaido`
revive mortos e cura vivos) declaram o escopo próprio.

### 3.3 Seletor (quais/quantos STATUS dentro de cada combatente)
Só para ações de manipulação de status (`Remover*`/`Mover*`/`Transformar`). É um
eixo **separado** do "quantos combatentes" (esse vem do `NumeroDeAlvos` da
habilidade). Ex: "ataca 2, rouba 1 buff aleatório de cada" = 2 combatentes
(NumeroDeAlvos) × 1 status por combatente (Seletor).

O Seletor tem três partes:
- **filtro** — todos, ou só um tipo (`BuffAtaque`), ou só removíveis.
- **quantos** — 1, 2, todos.
- **modo** — aleatório ou por ordem/prioridade.

O Seletor é a **peça compartilhada** entre `RemoverBuffs`/`MoverBuffs`/`Transformar`:
as operações são distintas (não se fundem), mas todas recebem o mesmo Seletor pra
decidir *quais/quantos*.

**Exemplos (habilidades futuras que Gabriel garante que virão):**

| Habilidade | Escopo | Seletor | Operação |
|---|---|---|---|
| Rouba só o buff de ATK | AlvosResolvidos | filtro=`BuffAtaque`, quantos=todos | Mover |
| Ataca 2, rouba 1 aleatório de cada | AlvosResolvidos | quantos=1, modo=aleatório | Mover |
| Remove 1 ou 2 buffs do inimigo | Inimigos/selecionado | quantos=1 ou 2 | Remover |
| Cleanse de 1 debuff num aliado escolhido | AlvosResolvidos (1) | filtro=Debuff, quantos=1 | Remover |
| Transforma buff no debuff correspondente | AlvosResolvidos | quantos=todos | Transformar |

---

## 4. Ações são ORDENADAS e veem o estado anterior

O interpretador roda a lista na ordem declarada, e cada ação vê o estado que as
anteriores deixaram. É **semanticamente significativo**, não uma otimização livre:
- `Barata`: aplica Intocável em si → *depois* ataca.
- `AnjoCaido`: revive os mortos → *depois* cura os vivos (a cura pega os que
  acabaram de reviver).

Reordenar quebra a semântica. Isto é invariante do interpretador.

---

## 5. `EstadoAlvo` desce para a ação — o `Ambos` morre

Hoje `EstadoAlvo` (Vivos/Mortos/**Ambos**) é propriedade da **habilidade inteira**.
O `Ambos` só existe como remendo pra dizer "essa habilidade age em dois estados,
então o pick automático se vira sozinho" — e é a origem do risco do default
silencioso no `ResolverAlvos`.

No modelo de composição, a habilidade **não tem mais um estado único** — cada
**ação** carrega o dela:

```
AnjoCaido = [ Reviver(TodosAliados, MORTOS),  Cura(TodosAliados, VIVOS) ]
BolaDeFogo = [ Dano(AlvosResolvidos, VIVOS),  AplicarDebuff(Queima 2t) ]
```

O `EstadoAlvo` vira propriedade da **ação**, não da habilidade. **O `Ambos` deixa de
existir** — não é "removido à mão", ele perde o sentido: uma habilidade que mira dois
estados é só uma habilidade com duas ações de estados diferentes.

**Pick de alvo deriva das ações:** se a habilidade tem **uma** ação que consome a
escolha do jogador, o estado+lista dela manda no menu (ataque → inimigos vivos;
revive-de-1 → aliados mortos). Se as ações batem em listas inteiras (AnjoCaido), não
há pick. Ou seja: "essa habilidade precisa de escolha, e de quem?" sai das ações,
sem nenhuma flag `EstadoAlvo` na habilidade.

**Consequência:** a ideia antiga de pôr `throw` no `EstadoAlvo.Ambos` fica sem
sentido e está descartada — não se blinda o que vai ser deletado. O `Ambos` morre
dentro deste refactor.

---

## 6. Categoria "ação condicional / de consequência" — nomeada, não resolvida agora

O Balde 2 tem um tempero que **não** é lista plana: `Barata` faz "se este golpe
**matou**, aplica `ImpedirRessurreicao` no morto". Não é "aplique efeito", é
"aplique **se** o Dano anterior matou".

Decisão: o ADR **nomeia** essa categoria (ação condicional / de consequência) em vez
de fingir que toda habilidade é lista plana — ~20% têm essa forma. **Não se resolve
agora.** As habilidades de Balde 2 continuam bespoke (`Ativar` override) até um lote
real pedir. Forma provável quando vier: uma ação-wrapper `AoMatar(acao)` que roda a
sub-ação condicionada ao resultado.

---

## 7. Bespoke e a disciplina de promoção

Ações genuinamente únicas ficam como **Ação custom / `Ativar` override**, na pasta
do champ, até ganharem um 2º cliente:
- `Copiando` (mover buffs com trade-off de stat), `Putridao` (agrega valor
  cross-target), etc.

**Promoção local → compartilhado acontece no 2º cliente REAL**, nunca especulativo
(YAGNI / "não desenhar no escuro"). Duas travas:
1. **Nomear os clientes reais** antes de extrair.
2. **Verificar-antes-de-fundir** — o grep MENTE. Ao mapear os "cleanse", 6 arquivos
   casaram na busca, mas `AnjoCaido` (remove só `ImpedirRessurreicao`, nos mortos) e
   `Putridao` (explode `Veneno` como parte de dano+cura) **não são** cleanse. Ler os
   candidatos é a regra; confiar no grep constrói a abstração errada
   ("duplicação é mais barata que a abstração errada").

**Precedente:** as `IReageAo*` e o `IAtivavelComNatureza` foram extraídos quando um
2º cliente real apareceu (o Operário no revide), não especulativamente. Mesma regra,
um nível abaixo.

---

## 8. Decisões cravadas das ações de status

Com clientes reais mapeados (e Gabriel garantindo que os padrões recorrem):

| Ação | Clientes hoje | Nasce compartilhada? |
|---|---|---|
| `RemoverDebuffs` (aliados) | Celestial, Coringa, DestruindoDia (3) | **Sim** — regra do três batida |
| `RemoverBuffs` (inimigos) | DocesOuTravessuras (+garantia de mais) | **Sim** |
| `MoverBuffs` | Copiando (+garantia de mais) | **Sim** — SEPARADA de RemoverBuffs |
| `Transformar` (buff→debuff) | nenhum ainda | **Não** — futuro |

`RemoverBuffs` e `MoverBuffs` **são operações distintas** (descartar vs transferir) —
não se fundem, só compartilham o Seletor. O `Transformar` é operação nova; quando
vier, depende dos **debuffs-contraparte existirem** (`BuffDefesa`→`ReducaoDefesa`
já existe; "reduzir ataque" talvez não) + um **mapa** buff↔debuff.

---

## 9. Contratos que o interpretador PRESERVA (invariantes)

O refactor não pode quebrar, e quebraria **em silêncio**:
1. **`TipoAtaque` continua declarado na habilidade** (AreaDeEfeito/Sequencial/
   NaoAtaque) e alimenta o dispatch de passivas-do-atacante (o `CombateService` lê
   ele pra decidir quantas vezes dispara `DepoisDeAtacar`). Manter explícito — não
   inferir das ações.
2. **O interpretador agrega os `EventoDano`** das ações de `Dano` e devolve a lista
   que as reações-do-atacante **e** a exibição consomem. Ações sem dano (Cura,
   RemoverBuffs) não entram.

---

## 10. Convivência (Strangler) e sequência de execução

**Costura de convivência:** `HabilidadeAtiva` ganha `List<Acao> Acoes` + um `Ativar`
**default** que roda as ações nos alvos resolvidos. Habilidade-dado só passa as
`Acoes` e não sobrescreve nada; habilidade bespoke **continua** sobrescrevendo
`Ativar` (ou vira uma Ação custom). Cada habilidade migra sozinha, build verde o
tempo todo.

**Sequência de PRs (um PR, um tema):**
1. **PR-1 (este fio):** modelo de `Acao` + interpretador + as ações nomeadas +
   migrar **só o Mago** (BolaDeFogo, Incendio) como piloto — é Balde 1 puro, não
   encosta em nenhuma das paredes (condicional, agregação). Build verde.
2. Depois: migrar champ-a-champ / facção-a-facção. Balde 2/3 mantêm `Ativar`
   override até a categoria condicional ser desenhada.
3. **Tema separado, depois:** champ-como-dado + reorganização pasta-por-champ
   (ver §11). Não se mistura com a composição — abre uma frente grande de cada vez.

---

## 11. Organização de pastas (destino, tema próprio)

Fim do caminho (fio separado da composição):

```
Skills/Acoes/              ← vocabulário compartilhado: Dano, Cura, AplicarBuff,
                             AplicarDebuff, RemoverDebuffs, RemoverBuffs, MoverBuffs,
                             Reviver, ConcederTurnoExtra...
Skills/Buffs/  Skills/Debuffs/   ← os StatusEffect (já existem)
Champs/<Faccao>/<Champ>/   ← <Champ>.cs (DADO: stats + habilidades como config + A1)
                             + Passiva.cs (comportamento)
                             + Acao bespoke local (ainda não promovida)
```

Criar champ novo = uma pasta nova, sem tocar arquivos de outro champ. **Descoberta:**
registro explícito de 1 linha (não reflection — magia é difícil de debugar e não
serializa). A pasta do champ mistura **dado** (a definição) com **código** só onde
há comportamento real (passiva + bespoke) — as duas coisas convivem no mesmo lugar
sem conflito.

**Nome "Acoes":** escolhido por Gabriel. Evita a colisão com `StatusEffect` — `Buff`
e `Debuff` já SÃO "efeitos", então chamar os primitivos de composição de "Efeitos"
seria ambíguo.

---

## 12. Fora de escopo deste fio (follow-ons, sem data — fazemos quando for natural)

- **`IModificaDanoCausado`** — a ação `Dano` passa a consultar modificadores do
  atacante automaticamente (a `PassivaPiromancer` para de ser fiada à mão em cada
  habilidade de fogo). Espelho do `IModificaDanoRecebido` que já existe. Desenhar a
  `Dano` de um jeito que não feche essa porta, mas não implementar no PR-1.
- **Champ-como-dado + pastas** (§11) — tema separado, depois da composição provar-se.
- **`EventoDano` por ID** — hoje carrega `Combate Atacante`/`Combate Alvo` (objetos
  vivos). Pra ser um registro limpo do golpe (log/stream desacoplado dos objetos),
  referenciaria por id/nome. Mudança real, sem data.
- **Rename do repo/namespace** — tirar o `v1` (`v1_Apostle_s_War` → `ApostlesWar`),
  deixar só "Apostle's War". Junto da faxina de nomes (`Campaingn`, PT/EN).

---

## 13. Validação

- O piloto do Mago compila e joga idêntico ao de hoje (mesmo dano, mesma Queima,
  mesmo Incêndio AoE), agora como config em vez de duas classes.
- As passivas-do-atacante disparam o mesmo nº de vezes (contrato `TipoAtaque`
  preservado).
- Nenhuma outra habilidade muda de comportamento (convivência Strangler: só o Mago
  passa pelo interpretador; o resto segue no `Ativar` velho).
