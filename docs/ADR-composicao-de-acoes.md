# ADR — Composição de Ações e Motor de Habilidades (habilidade como dado)

> **Tipo:** Architecture Decision Record
> **Status:** Aceito — desenho do MOTOR fechado; implementação incremental a seguir.
> **Contexto:** Apostle's War. Consome a "Auditoria das habilidades ativas" que o
>   ROADMAP deixou em aberto. É o fio predecessor natural do Rebalanceamento e da
>   reorganização pasta-por-champ.
> **Data:** julho/2026 (revisão: motor desenhado, vocabulário e boundaries mapeados
>   lendo o roster real — Inferno, Putridão, AnjoCaído, Copiando, Atlantis, Quebrar, Barata).

---

## 0. Nota de revisão (o que mudou desde a 1ª versão)

A 1ª versão fechou o CONCEITO (habilidade como dado) e um piloto (o Mago, PR #115). Esta
revisão desenha o **motor** de verdade, depois de destrinchar as habilidades difíceis. As
correções relevantes:

- **A unidade de reúso NÃO é a Ação inteira — é o FRAGMENTO.** `Cura` e `Escudo` "parecem
  iguais" porque compartilham fragmentos (o valor `% de HP de alguém`, a iteração sobre um
  conjunto) e diferem só no verbo final. Ver §5.5.
- **As "três frentes" (escopo próprio, condição de estado, agregação) NÃO são três
  mecanismos.** Duas delas se dissolvem numa única mudança (o loop-flip, §3.2); só a
  agregação cross-alvo é uma peça real, e pequena (`AcaoSobreConjunto`, §3.4).
- **O piloto do Mago (PR #115) foi só o formato per-alvo**, não o motor. Conceitualmente
  ainda não construímos o motor — este ADR é o desenho antes de escrever.
- **`Copiando` foi recategorizado** de Balde 3 (bespoke) para vocabulário puro
  (`MoverBuffs` + `ConcederTurnoExtra`). O grep e a memória mentem; ler destravou (§9).
- **`Atlantis` revelou um boundary novo** — o conceito de "pipeline / conjunto afetado"
  (§8.1), que o eixo `EstadoAlvo` sozinho não expressa.

---

## 1. Problema

Hoje cada habilidade ativa é uma **classe própria** (`Skills/Ativas/*.cs`, ~74 arquivos).
Olhando o que essas classes de fato contêm, elas caem em três baldes:

- **Balde 1 (~70%) — 100% dado.** `BolaDeFogo`, `AtaqueBasico`, `Lealdade`, `Democracia`,
  `Incendio`... A classe inteira é metadados (`NumeroDeAlvos`/`TipoAlvo`/`TipoLista`/
  `EstadoAlvo`) + um `Ativar` que faz `foreach` sobre `ResolverAlvos` aplicando uma **lista
  fixa de efeitos**. **A classe não tem comportamento; tem configuração.**
- **Balde 2 (~20%) — dado + um tempero.** `Barata` (buff em si → ataca → se matou, Sentença
  no morto), `AnjoCaido` (revive mortos **e** cura vivos). Ainda é lista de efeitos, com um
  condicional ou dois escopos.
- **Balde 3 (~10%) — bespoke de verdade.** `Putridao` (explode Veneno pra dano + cura pela
  média dos percentuais — **agrega valor ao longo dos alvos**). Comportamento que uma lista
  de efeitos-padrão não expressa.

Escrever uma classe por habilidade do Balde 1 é o mesmo que escrever uma
`class Operario : Personagem` só pra guardar `HP = 1200` — dado fingindo de código. Foi a
fricção que Gabriel sentiu ("senti que reinventei o que já existe" ao criar habilidade nova).

---

## 2. Decisão central — dois níveis

O que se decidiu tem **dois níveis distintos**, que na 1ª versão estavam grudados:

- **Nível A — Organização (champ como classe/arquivo).** Cada champ vira uma classe/arquivo
  próprio com suas habilidades dentro, em vez de uma linha numa `List<Personagem>` gigante.
  Isso é **organização pura**, não precisa do motor, não tem parede. Benefício extra: o
  arquivo do champ vira a **view** — abre `Mago.cs` e lê todas as habilidades, símbolos,
  cooldowns e descrições **sem rodar o jogo** (§10.1).
- **Nível B — Composição (o motor).** Trocar o `Ativar` por-habilidade por uma lista
  declarativa de **Ações** rodada por um interpretador único.

Os dois são ortogonais — dá pra fazer o A sem tocar no B. Este ADR desenha o B; o A e a
reorganização pasta-por-champ têm o seu próprio momento (§10).

**Princípio-guia (corrigido):** a unidade de reúso é o **FRAGMENTO**, não a Ação inteira
(§5.5). Decompõe-se a habilidade em fragmentos e pergunta-se de cada um "isso se repete?".

---

## 3. O Motor

### 3.1 Uma forma só, um interpretador só, zero `Ativar` override

No destino, `HabilidadeAtiva` deixa de ser abstrata-subclassada e vira **concreta,
construída com dado**. Toda habilidade é `new HabilidadeAtiva(..., acoes: [...])`. **Nenhuma
habilidade sobrescreve `Ativar`.** O que some é o *override por-habilidade*; o
`HabilidadeAtiva.Ativar` — um método só — **fica**: ele É o motor.

```csharp
class HabilidadeAtiva : Habilidade {
    public int NumeroDeAlvos { get; }
    public TipoAlvo TipoAlvo { get; }
    public TipoLista TipoLista { get; }
    public TipoAtaque TipoAtaque { get; }
    private readonly List<Acao> _acoes;

    public HabilidadeAtiva(string nome, string simbolo, int turnos, string descricao,
        int numeroDeAlvos, TipoAlvo tipoAlvo, TipoLista tipoLista,
        TipoAtaque tipoAtaque, List<Acao> acoes) : base(nome, simbolo, turnos, descricao)
    { /* atribui */ }
}
```

### 3.2 A ideia única: virar o loop do avesso

Hoje o interpretador é **alvo-por-fora, ação-por-dentro** (`HabilidadeAtiva.cs:47`). Vira
**ação-por-fora, e cada ação resolve seu próprio conjunto+estado no momento em que roda**:

```csharp
foreach (var acao in _acoes) {
    var conjunto = ResolverEscopo(acao, alvosResolvidos, ctx);      // escopo + estado, AGORA
    if (acao is AcaoSobreConjunto agg)                             // §3.4
        agg.Executar(ctx.Atacante, conjunto, eventos);
    else
        foreach (var alvo in conjunto)
            acao.Executar(ctx.Atacante, alvo, eventos);
}

IEnumerable<Combate> ResolverEscopo(Acao acao, List<Combate> resolvidos, ContextoCombate ctx) {
    var conjunto = acao.Escopo switch {
        Escopo.AlvosResolvidos => resolvidos,
        Escopo.TodosAliados    => ctx.Aliados,
        Escopo.TodosInimigos   => ctx.Inimigos,
        Escopo.ProprioAtacante => new[] { ctx.Atacante },
    };
    return conjunto.Where(c => acao.EstadoAlvo == EstadoAlvo.Vivos ? c.EstaVivo() : !c.EstaVivo());
}
```

Essa virada única **dissolve** o que pareciam três problemas separados:

- **Escopo próprio** — cada ação nomeia seu conjunto. `Escudo → ProprioAtacante`.
- **Condição de estado** — cada ação filtra por estado **no momento dela**, depois das
  anteriores. `Sentença(Mortos)` só pega quem o `Dano` anterior matou. Sem wrapper condicional.
- **Agregação parcial→total** — como é ação-por-fora, o `Dano` termina TODA a passada antes
  da ação seguinte; o `Escudo` lê o `eventos` já completo.

### 3.3 Ações ordenadas veem o estado anterior (invariante)

O interpretador roda a lista na ordem declarada, e cada ação vê o estado que as anteriores
deixaram. É **semanticamente significativo**:

- `AnjoCaido`: revive os mortos → *depois* cura os vivos (a cura pega os revividos).
- `Barata`: aplica Intocável em si → *depois* ataca → *depois* Sentença nos que morreram.

Reordenar quebra a semântica. Combinado com a avaliação-na-execução (§3.2), é isso que
absorve a antiga "categoria condicional / ao-matar" para dentro do fluxo normal.

### 3.4 `AcaoSobreConjunto` — a única parede real (agregação cross-alvo)

Algumas habilidades calculam um valor que **atravessa todos os alvos** (a média da Putridão,
`soma ÷ contador`). Uma `Acao` per-alvo não enxerga o conjunto todo, e guardar acumulador na
instância é frágil (ela é reusada entre combates). Então existe um **segundo formato de
ação**, que recebe o conjunto inteiro de uma vez:

```csharp
abstract class AcaoSobreConjunto : Acao {
    public abstract void Executar(Combate atacante, IReadOnlyList<Combate> conjunto, List<EventoDano> ev);
}
```

O interpretador despacha por tipo (ver §3.2). A lógica que agrega mora numa
`AcaoSobreConjunto` (com variável local, à vontade). É a "Frente 3" — e acabou sendo
modesta: uma interface a mais + um `if` no despacho.

---

## 4. Os três níveis (todos pelo motor, nenhum com `Ativar` override)

"Motor único" **não** quer dizer "toda habilidade segue o vocabulário". São três níveis, e
os três são `new HabilidadeAtiva(..., acoes: [...])`:

| Nível | O que é | Exemplos |
|---|---|---|
| 1. **Vocabulário puro** | lista de ações compartilhadas | BolaDeFogo, Inferno, AnjoCaído, Copiando |
| 2. **Vocabulário + 1 ação custom** | quase tudo compartilhado, uma `Acao` bespoke pro pedaço único | Putridão (média), Atlantis (revive-e-Intocável) |
| 3. **Ação custom inteira** | a habilidade é uma `Acao` bespoke que faz tudo | as genuinamente únicas |

A diferença entre os níveis é só **se as ações da lista são do vocabulário ou custom-locais
do champ**. "Único" deixa de ser uma habilidade especial (subclasse com `Ativar`) e vira uma
`Acao` especial dentro da forma padrão. O código imperativo não morre — ele muda de
recipiente (de `Ativar` override para `Acao` custom), e ganha uniformidade de graça.

---

## 5. Anatomia de uma Ação

Uma ação combina eixos independentes: **Operação × Escopo × EstadoAlvo × (Valor) × (Seletor)**.

### 5.1 Operação (o quê) — o vocabulário
`Dano`, `Cura`, `Escudo`, `AplicarBuff`, `AplicarDebuff`, `Reviver`, `RemoverDebuffs`
(cleanse), `MoverBuffs` (roubo), `ConcederTurnoExtra`, `Explodir`, `Transformar` (futuro)...
O vocabulário compartilhado do jogo. **Cresce por descoberta** — quando um pedaço recorre em
2 clientes reais, ganha um nome (§9). Foi assim que `Explodir` apareceu (Inferno + Putridão).

### 5.2 Escopo (em quais COMBATENTES)
`AlvosResolvidos` (default, herda o pick da habilidade), `TodosAliados` (o time do atacante —
que **inclui o próprio atacante**), `TodosInimigos`, `ProprioAtacante`. Provável 5º:
`OutrosAliados` (aliados menos o conjurador — o OssoDuroDeRoer pede).

### 5.3 EstadoAlvo por ação (Vivos/Mortos) — avaliado na execução
`EstadoAlvo` **desce da habilidade pra ação** e é avaliado no momento em que a ação roda
(não uma vez, na resolução). É isso que faz `Cura(Vivos)` pegar os recém-revividos e
`Sentença(Mortos)` pegar os recém-mortos. **O `Ambos` MORRE** — uma habilidade que mira dois
estados é só uma habilidade com duas ações de estados diferentes; o `throw` no `Ambos` fica
sem sentido e está descartado.

### 5.4 Seletor (quais/quantos STATUS por combatente)
Só para operações de manipulação de status (`RemoverDebuffs`/`MoverBuffs`/`Explodir`/
`Transformar`). Eixo **separado** do `NumeroDeAlvos`. Três partes: **filtro** (todos / um
tipo / só removíveis), **quantos** (1, 2, todos), **modo** (aleatório / por ordem). É a peça
compartilhada entre as operações de status — as operações são distintas (não se fundem), mas
recebem o mesmo Seletor.

### 5.5 Valor — fragmentos compartilhados (o "cura = escudo")
O grão fino do reúso. Operações com magnitude (`Dano`, `Cura`, `Escudo`) recebem um
**fragmento de valor**, não um número cru:

```csharp
static class Valor {
    public static ValorFn Fixo(int v)              => (atk, alvo, ev) => v;
    public static ValorFn PorHP(double p)          => (atk, alvo, ev) => (int)(alvo.HPMaximo * p);
    public static ValorFn PorHPDoAtacante(double p)=> (atk, alvo, ev) => (int)(atk.HPMaximo * p);
    public static ValorFn PorDanoCausado(double p) => (atk, alvo, ev) => (int)(ev.Sum(e => e.DanoEfetivo) * p);
}
```

`Cura` e `Escudo` diferem **só no verbo final** — `alvo.Curar(v)` vs
`new EscudoBuff(v).Aplicar(alvo)` — e compartilham o fragmento de valor. **Diversidade de
fonte de valor é mantida de propósito, sem forçar um padrão único** (decisão de Gabriel): um
escudo ganha por dano, outro por % do HP do alvo, outro por % do HP próprio — as três formas
coexistem como fragmentos distintos, e só se fundem se a dor pedir. `Explodir` usa um
fragmento do lado do status: a interface `IStatusComTick { int Detonar(Combate portador); }`,
implementada por `Queima`/`Veneno`/`Sangramento`.

---

## 6. Exemplos trabalhados (testados no roster real)

```csharp
// INFERNO — vocabulário puro (per-alvo)
acoes: {
    new AplicarDebuff(() => new Queima(2), AlvosResolvidos, Vivos),
    new Explodir(Seletor.Tipo<Queima>(),   AlvosResolvidos, Vivos),
}

// ANJOCAÍDO — estado-por-ação + ordem (per-alvo, sem agregação)
acoes: {
    new RemoverDebuffs(Seletor.Tipo<ImpedirRessurreicao>(), TodosAliados, Mortos),
    new Reviver(0.50,           TodosAliados, Mortos),
    new Cura(Valor.PorHP(0.30), TodosAliados, Vivos),   // pega os revividos
}

// COPIANDO — vocabulário puro + Seletor (a surpresa: NÃO é Balde 3)
acoes: {
    new MoverBuffs(Seletor.Removiveis(), destino: ProprioAtacante, AlvosResolvidos, Vivos),
    new ConcederTurnoExtra(ProprioAtacante),
}

// QUEBRAR — agregação por soma (via eventos, per-alvo + PorDanoCausado)
acoes: {
    new Dano(Valor.Fixo... /* 2.0x */,      AlvosResolvidos, Vivos),
    new AplicarDebuff(atk => new Irritar(atk), AlvosResolvidos, Vivos),   // só sobreviventes
    new Escudo(Valor.PorDanoCausado(0.30),  ProprioAtacante, Vivos),      // lê o dano somado
}

// PUTRIDÃO — a única com AcaoSobreConjunto (média cross-alvo, Nível 2)
acoes: {
    new Dano(1.0,                         AlvosResolvidos, Vivos),
    new ExplodirVenenoECurarMedia(sobre: Inimigos, Vivos),  // custom, reusa Detonar
}
```

| Hab | Vocabulário | Mecanismo | Nível |
|---|---|---|---|
| Inferno | AplicarDebuff, Explodir | per-alvo | 1 |
| AnjoCaído | RemoverDebuffs, Reviver, Cura | estado-por-ação + ordem | 1 |
| Copiando | MoverBuffs, ConcederTurnoExtra | per-alvo + Seletor | 1 |
| Barata | AplicarBuff, Dano, AplicarDebuff | escopo próprio + estado (Mortos) | 1 |
| Quebrar | Dano, AplicarDebuff, Escudo | escopo próprio + PorDanoCausado | 1 |
| Putridão | Dano, Detonar (custom) | **AcaoSobreConjunto** | 2 |

---

## 7. Invariantes que o interpretador PRESERVA

Quebrariam **em silêncio** — manter explícitos:

1. **`TipoAtaque` continua declarado na habilidade** (AreaDeEfeito/Sequencial/NaoAtaque) e
   alimenta o dispatch de passivas-do-atacante (o `CombateService` lê ele pra decidir quantas
   vezes dispara `DepoisDeAtacar`). Não inferir das ações.
2. **O interpretador agrega os `EventoDano`** das ações de dano (`Dano`, e a explosão do
   `Explodir`) e devolve a lista que as reações-do-atacante **e** a exibição consomem. Ações
   sem dano não entram.

---

## 8. Boundaries e questões abertas (registradas, não resolvidas agora)

### 8.1 Pipeline / conjunto-afetado (a Atlantis)
A `Atlantis` revive os mortos e aplica Intocável **só nos que reviveu** — não em todos os
vivos. `AplicarBuff(Intocavel, TodosAliados, Vivos)` estaria errado (pegaria os que já
estavam vivos). O `EstadoAlvo` sozinho não distingue "todos os vivos agora" (AnjoCaído) de
"os que a ação anterior afetou" (Atlantis). A forma geral seria um escopo de **pipeline**
(`AfetadosPelaAcaoAnterior`): a `Reviver` produz um conjunto, a `AplicarBuff` consome. **1
cliente real hoje** (Barata parece irmão mas é resolvido por `AlvosResolvidos+Mortos`). Pela
disciplina do §9: **não construir ainda** — Atlantis fica Nível 2 (bespoke) até um 2º cliente.

### 8.2 Derivação do pick do menu
Com `EstadoAlvo` fora da habilidade, o **menu de seleção de alvo** precisa derivar da ação
que consome a escolha do jogador (a com `Escopo.AlvosResolvidos`): o estado+lista dela mandam
no menu (ataque → inimigos vivos; revive-de-1 → aliados mortos). Isso encosta no
`SelecaoDeAlvoService`/`CombateService`, não só no combate — é o lado UI da Frente 2.

### 8.3 Escopo do dano no `PorDanoCausado`
`PorDanoCausado` soma **tudo** no `eventos`. Se uma habilidade quer curar/escudar só do dano
de uma ação específica (ex: só da explosão, não do ataque também), o fragmento precisaria
distinguir "dano da ação anterior". Detalhe que aparece quando esse cliente for real.

---

## 9. Bespoke e a disciplina de promoção

Ações genuinamente únicas ficam como `Acao` custom local na pasta do champ (Nível 2/3) até
ganharem um 2º cliente. **Promoção local → compartilhado acontece no 2º cliente REAL**, nunca
especulativo (YAGNI / "não desenhar no escuro"). Duas travas:

1. **Nomear os clientes reais** antes de extrair.
2. **Verificar-antes-de-fundir** — o grep MENTE. Provas desta rodada:
   - `Copiando` estava catalogado como Balde 3 bespoke; ler mostrou que é vocabulário puro
     (`MoverBuffs` + `ConcederTurnoExtra`). O "trade-off de stat" do comentário não é lógica,
     é consequência de mover a instância.
   - `AnjoCaido`/`Putridao` "pareciam cleanse" e não eram.
   - `Atlantis` parecia igual ao `AnjoCaido` e escondia um boundary (§8.1).

**Cravado (nascem compartilhados, clientes reais mapeados):** `RemoverDebuffs` (Celestial,
Coringa, DestruindoDia, AnjoCaido), `RemoverBuffs` (DocesOuTravessuras), `MoverBuffs`
(Copiando), `Explodir` (Inferno, Putridão). `Transformar` (buff→debuff) é futuro (depende dos
debuffs-contraparte + um mapa).

---

## 10. Organização de pastas e a view do champ

### 10.1 Champ como arquivo = a view
```
Champs/<Faccao>/<Champ>/
    <Champ>.cs             ← DADO: stats + habilidades montadas como config
    <Passiva>.cs           ← CÓDIGO: comportamento (sai de Skills/Passivas/)
    <AcaoBespoke>.cs       ← Ação custom local (Nível 2/3, não promovida)
Skills/Acoes/              ← vocabulário compartilhado + os fragmentos de Valor/Seletor
Skills/Buffs/  Skills/Debuffs/   ← os StatusEffect (já existem)
Champs/Roster.cs           ← registro explícito de 1 linha por champ (NÃO reflection)
```

Criar champ novo = uma pasta nova, sem tocar arquivos de outro. E o arquivo do champ vira a
**view** que Gabriel quer: abre `Mago.cs`, lê nome/símbolo/cooldown/descrição de todas as
habilidades **sem rodar o jogo**. Uniformidade também torna trivial uma view *imprimível*
genérica (um método lê `Nome`/`Simbolo`/`Descricao` de qualquer `HabilidadeAtiva`).

### 10.2 Ordem dos temas
Nível A (champ-como-classe/arquivo) e a reorganização pasta-por-champ são um **tema separado**
do motor. Não se misturam — uma frente grande de cada vez.

---

## 11. Sequência de implementação

- **PR #115 (feito):** modelo de `Acao` + interpretador per-alvo + `Dano`/`AplicarDebuff` +
  piloto Mago (BolaDeFogo, Incendio). **Foi o formato per-alvo, não o motor.** Strangler: o
  resto segue no `Ativar` velho, build verde.
- **Motor (próximo):** o loop-flip (§3.2) + `Escopo` + `EstadoAlvo` por ação, avaliado na
  execução. Migrar 2-3 clientes de cada eixo (escopo próprio: Barata/Quebrar/ArvoreDoMundo;
  estado: AnjoCaído/Barata). O `Ambos` morre aqui.
- **Vocabulário incremental:** `Cura`, `AplicarBuff`, `Escudo` (+ fragmentos de Valor);
  depois `Reviver`, `RemoverDebuffs`, `MoverBuffs`, `ConcederTurnoExtra`, `Explodir`
  (+ `IStatusComTick`, `Seletor`) — cada um com clientes reais na mão.
- **`AcaoSobreConjunto`:** quando a Putridão for migrada (agregação).
- **Pick do menu (§8.2):** o lado UI, quando o `EstadoAlvo` sair da habilidade.
- **Tema separado, depois:** champ-como-dado + pastas (§10) + rename do repo (§12).

Um PR, um tema. Build verde o tempo todo (Strangler).

---

## 12. Fora de escopo deste fio (follow-ons, sem data)

- **`IModificaDanoCausado`** — a ação `Dano` consulta modificadores do atacante sozinha (a
  `PassivaPiromancer` para de ser fiada à mão). Espelho do `IModificaDanoRecebido`. Desenhar
  a `Dano` sem fechar essa porta, mas não implementar agora.
- **Champ-como-dado + pastas** (§10) — tema separado, depois do motor provar-se.
- **`EventoDano` por ID** — hoje carrega objetos vivos (`Combate`); referenciar por id/nome
  desacopla o log/stream dos objetos.
- **Rename do repo/namespace** — tirar o `v1` (`v1_Apostle_s_War` → `ApostlesWar`).

---

## 13. Validação

- Cada habilidade migrada compila e joga idêntico ao de hoje (mesmo dano, mesmos status,
  mesma ordem), agora como config em vez de classe.
- As passivas-do-atacante disparam o mesmo nº de vezes (`TipoAtaque` preservado).
- Convivência Strangler: só as habilidades migradas passam pelo motor novo; o resto segue no
  `Ativar` velho, sem mudança de comportamento.
