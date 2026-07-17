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

**Princípio (decorrência) — DECOMPOR, não juntar:** quando uma habilidade faz N coisas,
declare N ações do vocabulário na ordem, em vez de espremer tudo numa ação bespoke que "faz
duas coisas". A ordem + a avaliação-na-execução já entregam a semântica (a cura do RaioX é ação
separada da extensão de buffs; a cura da Putrefação é ação separada da explosão — §9). Bespoke
fica reservado a um VERBO novo e ATÔMICO (Explodir, GolpeSeguidor, EstenderBuffs), NUNCA a uma
COMBINAÇÃO de verbos que já existem. (Decisão de Gabriel, firmada no rebalance da Putrefação.)

### 3.4 `AcaoSobreConjunto` — agregação cross-alvo (CONSTRUÍDA E REMOVIDA — desenho registrado)

> **Status (jul/2026):** implementada no sweep do LadoSombrio pra média da Putrefação e
> REMOVIDA no mesmo sweep — o rebalance de Gabriel trocou a média por "cura 20% de todo o
> dano causado", que é o fragmento `PorDanoCausado` lendo o `eventos` (a agregação que o
> loop-flip §3.2 já dá de graça, sem formato novo). Zero clientes → YAGNI. O desenho fica
> registrado abaixo pra reconstrução barata se uma agregação cross-alvo REAL aparecer
> (candidata: Atlantis §8.1).

Algumas habilidades calculariam um valor que **atravessa todos os alvos** de um jeito que o
`eventos` não expressa (a antiga média da Putridão, `soma ÷ contador` de PERCENTUAIS — não de
dano). Uma `Acao` per-alvo não enxerga o conjunto todo, e guardar acumulador na instância é
frágil (ela é reusada entre combates). O formato seria um **segundo formato de ação**, que
recebe o conjunto inteiro de uma vez:

```csharp
abstract class AcaoSobreConjunto : Acao {
    public abstract void Executar(Combate atacante, IReadOnlyList<Combate> conjunto, List<EventoDano> ev);
}
```

O interpretador despacharia por tipo (ver §3.2). É modesto: uma classe a mais + um `if` no
despacho. **Antes de reconstruir, confira se o valor agregado não é derivável do `eventos`** —
foi exatamente assim que o único cliente morreu.

---

## 4. Os três níveis (todos pelo motor, nenhum com `Ativar` override)

"Motor único" **não** quer dizer "toda habilidade segue o vocabulário". São três níveis, e
os três são `new HabilidadeAtiva(..., acoes: [...])`:

| Nível | O que é | Exemplos |
|---|---|---|
| 1. **Vocabulário puro** | lista de ações compartilhadas | BolaDeFogo, Inferno, AnjoCaído, Copiando |
| 2. **Vocabulário + 1 ação custom** | quase tudo compartilhado, uma `Acao` bespoke pro pedaço único | Atlantis (revive-e-Intocável); GolpeSeguidor (Shuriken) — a Putrefação era o exemplo e SUBIU pro Nível 1 (a média virou cura-por-dano) |
| 3. **Ação custom inteira** | a habilidade é uma `Acao` bespoke que faz tudo | as genuinamente únicas |

A diferença entre os níveis é só **se as ações da lista são do vocabulário ou custom-locais
do champ**. "Único" deixa de ser uma habilidade especial (subclasse com `Ativar`) e vira uma
`Acao` especial dentro da forma padrão. O código imperativo não morre — ele muda de
recipiente (de `Ativar` override para `Acao` custom), e ganha uniformidade de graça.

---

## 5. Anatomia de uma Ação

Uma ação combina eixos independentes: **Operação × Escopo × EstadoAlvo × (Valor) × (Seletor)**.

### 5.1 Operação (o quê) — o vocabulário
> **Índice vivo (compartilhadas + bespokes + mapeadas, com caminhos): `CATALOGO-de-acoes.md`.**
> Leia-o antes de criar habilidade — evita reinventar verbo que já existe.

`Dano`, `Cura`, `Escudo`, `AplicarBuff`, `AplicarDebuff`, `Reviver`, `RemoverDebuffs`
(cleanse), `MoverBuffs` (roubo), `ConcederTurnoExtra`, `Explodir`, `Transformar` (futuro)...
O vocabulário compartilhado do jogo. **Cresce por descoberta** — quando um pedaço recorre em
2 clientes reais, ganha um nome (§9). Foi assim que `Explodir` apareceu (Inferno + Putridão).

### 5.2 Escopo (em quais COMBATENTES)
`AlvosResolvidos` (default, herda o pick da habilidade), `TodosAliados` (o time do atacante —
que **inclui o próprio atacante**), `TodosInimigos`, `ProprioAtacante`, `OutrosAliados` ✅
**IMPLEMENTADO** (aliados menos o conjurador — clientes reais: OssoDuroDeRoer ✅ (LadoSombrio),
Galáxia ✅ (Alien, Tecnológicos), Esmagar ✅ (Ogro, Folclore)).

### 5.3 EstadoAlvo por ação (Vivos/Mortos) — avaliado na execução
`EstadoAlvo` **desce da habilidade pra ação** e é avaliado no momento em que a ação roda
(não uma vez, na resolução). É isso que faz `Cura(Vivos)` pegar os recém-revividos e
`Sentença(Mortos)` pegar os recém-mortos. **O `Ambos` MORRE** — uma habilidade que mira dois
estados é só uma habilidade com duas ações de estados diferentes; o `throw` no `Ambos` ficou
sem sentido e foi descartado. **✅ O enum-value `Ambos` MORREU (jul/2026):** o 7º e último cliente
(o Céu) migrou nos Apóstolos; o value, o check no `CombateService:282` e o branch "sem filtro" do
interpretador foram removidos no PR de limpeza pós-sweep. `EstadoAlvo` hoje é só `{ Vivos, Mortos }`.

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

// PUTREFAÇÃO (como MIGROU — rebalance de Gabriel: cura 20% do dano total, não mais a média;
// a explosão registra EventoDano, então o PorDanoCausado agrega ataque + explosões, e a cura
// é um EXTRA da hab — a explosão é reutilizável a seco por clientes futuros)
acoes: {
    new Dano(1.0),
    new Explodir(Seletor.Tipo<Veneno>()),
    new Cura(Valor.PorDanoCausado(0.20), ProprioAtacante),
}
```

| Hab | Vocabulário | Mecanismo | Nível |
|---|---|---|---|
| Inferno | AplicarDebuff, Explodir | per-alvo | 1 |
| AnjoCaído | RemoverDebuffs, Reviver, Cura | estado-por-ação + ordem | 1 |
| Copiando | MoverBuffs, ConcederTurnoExtra | per-alvo + Seletor | 1 |
| Barata | AplicarBuff, Dano, AplicarDebuff | escopo próprio + estado (Mortos) | 1 |
| Quebrar | Dano, AplicarDebuff, Escudo | escopo próprio + PorDanoCausado | 1 |
| Putrefação ✅ | Dano, Explodir, Cura | per-alvo + PorDanoCausado (a média morreu) | 1 |

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

### 8.1 Pipeline / conjunto-afetado (a Atlantis) — DISSOLVIDO (jul/2026)
> **Status: DISSOLVIDO — NÃO construído.** A `Atlantis` e o Circo reviviam e aplicavam Intocável
> **só nos revividos**. A forma geral proposta era um escopo `AfetadosPelaAcaoAnterior` (a `Reviver`
> produz um conjunto, a ação seguinte consome). Não foi construído: **dissolveu-se por uma solução
> mais simples (ideia de Gabriel)** — a `Reviver` ganhou um `buffNoRevivido` (`Func<Buff>`) que
> aplica o buff a cada revivido, checando `EstaVivo()` logo após reviver. A própria `Reviver` já
> sabe quem voltou (e quem tinha Sentença e não voltou não pega) — o motor não precisa rastrear
> "conjunto afetado". É o MESMO padrão da `AcaoSobreConjunto` (§3.4): um mecanismo geral planejado
> que uma peça específica e simples dissolveu. **Reconstrói-se o pipeline SÓ se aparecer um cliente
> que precise fazer algo NÃO-buff nos afetados por uma ação anterior (ex: curar só os revividos)** —
> hoje não existe. Clientes do `buffNoRevivido`: Atlantis (Sereia), Circo (Palhaço, era bug — antes
> pegava todos os outros vivos).

### 8.2 Derivação do pick do menu
Com `EstadoAlvo` fora da habilidade, o **menu de seleção de alvo** precisa derivar da ação
que consome a escolha do jogador (a com `Escopo.AlvosResolvidos`): o estado+lista dela mandam
no menu (ataque → inimigos vivos; revive-de-1 → aliados mortos). Isso encosta no
`SelecaoDeAlvoService`/`CombateService`, não só no combate — é o lado UI da Frente 2.
**Status:** a parte pequena (matar o enum `Ambos`) FEITA no PR de limpeza pós-sweep; a **derivação
do menu** em si é tema estrutural próprio, junto do refactor do `ExecutarTurno` — NÃO feito ainda.

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

**Cravado (nascem compartilhados, clientes reais mapeados):** `RemoverDebuffs`
✅ **IMPLEMENTADA** (gêmeo do `RemoverBuffs`; 1º cliente Coringa/Folclore; próximos Celestial,
DestruindoDia, AnjoCaído), `RemoverBuffs` ✅ **IMPLEMENTADA**
(DocesOuTravessuras, LadoSombrio), `MoverBuffs` (Copiando — ainda sem cliente migrado),
`Explodir` ✅ **IMPLEMENTADA** genérica com `Seletor` + `IStatusComTick` (1º cliente: Putrefação
com `Seletor.Tipo<Veneno>()`; Inferno mapeado pra Decaídos; Gabriel planeja mais clientes de
explosão-de-veneno — a ação já serve a seco, sem a cura). `IStatusComTick` ✅ **IMPLEMENTADA**
(`Detonar(portador, detonador) → EventoDano`; Veneno e Queima). `Transformar` (buff→debuff) é
futuro (depende dos debuffs-contraparte + um mapa).

**`AcaoSobreConjunto` — REMOVIDA (jul/2026, decisão de Gabriel).** Foi construída no sweep do
LadoSombrio pra média cross-alvo da Putrefação — e a média MORREU no rebalance ("cura 20% de
todo o dano causado" é o fragmento `PorDanoCausado` lendo o `eventos`, agregação que o
loop-flip já dá de graça). O `Reviver` também chegou a nascer nela (`quantos` + `Take`) e foi
revertido pra per-alvo (regra do revive abaixo). Com ZERO clientes, saiu — YAGNI. O desenho
(§3.4) fica registrado: se uma agregação cross-alvo REAL aparecer (candidata: Atlantis §8.1,
pipeline), reconstrói-se — é 1 classe + 1 `if` de dispatch no interpretador.

**`Explodir` ✅ IMPLEMENTADA** (`Skills/Acoes/Explodir.cs`) — o molde ÚNICO das explosões
(regra de Gabriel): a ação orquestra via `Seletor` e cada status detona FAZENDO O QUE ELE FAZ
(`IStatusComTick.Detonar(portador, detonador)` devolve o `EventoDano` — Veneno só dano; Queima
dano + redução de HP máx). A detonação entra no `eventos`: aparece na exibição, conta no
`PorDanoCausado` e morte-por-explosão passa pelos Atos de morte (antes era um furo silencioso —
kill de explosão não disparava Guarda/Vilão/Necromancia). Natureza de status (`Reacao.Nenhuma`)
garante que não proca reações de ataque. Efeitos EXTRAS da habilidade (a cura da Putrefação)
NÃO moram na explosão — são ações separadas na lista, por isso ela é reutilizável a seco.
1º cliente: Putrefação (`Seletor.Tipo<Veneno>()`); Inferno usa `Seletor.Tipo<Queima>()` quando
migrar (Decaídos — até lá o shim `Queima.Explodir` chama o Detonar e descarta o evento).

**`Escopo.OutrosAliados` ✅ IMPLEMENTADO.** Clientes: OssoDuroDeRoer (Caveira, LadoSombrio) ✅,
Galáxia (Alien, Tecnológicos) ✅, Esmagar (Ogro, Folclore) ✅. (Circo saiu — virou Reviver-com-buff.)

**`EstenderBuffs` — bespoke-LOCAL do Robô (Tecnológicos).** Espelho EXATO do `RemoverBuffs`
(mesmo `Seletor`, troca só o verbo: `AumentarDuracao` no lugar de `Remover`) — segue o padrão
valência-split (`AplicarBuff`/`AplicarDebuff`, `RemoverBuffs`/`RemoverDebuffs`). RaioX é o
ÚNICO cliente ATIVO; promove pra `Skills/Acoes/` no 2º cliente ativo real (aí nasce o gêmeo
`EstenderDebuffs`). NÃO confundir com as passivas de duração (fio abaixo).

**FIO FUTURO — unir a seleção de status entre passivas e ações (decisão de Gabriel; registrar o
QUANDO e a REGRA).** Três passivas mexem em duração de status escolhendo o alvo À MÃO:
Policial/AlgemasReforçadas (estende o Preso), Repetindo (estende 1 debuff aleatório do atacante),
AnáliseCrítica (reduz TODOS os buffs do atacante). Elas compartilham com as Ações de status
(`RemoverBuffs`/`EstenderBuffs`) o PRIMITIVO (`StatusEffect.AumentarDuracao`/`ReduzirDuracao`, já
unificado na base — o `EstenderTurno` redundante foi colapsado em `AumentarDuracao(1)`) e o
CONCEITO de `Seletor` — mas **NÃO o dispatch** (passiva = `IReageAo*`, ação = `Acao`; os dois
sistemas ficam separados de propósito, não se fundem). Possível consolidação: as passivas
passarem a escolher status via o mesmo `Seletor` (unir o "pegar status", não o invólucro).
**QUANDO:** ao TOCAR cada passiva de duração no sweep da facção dela — AnáliseCrítica caiu nos
Tecnológicos (deixada como estava, DE PROPÓSITO), Repetindo cai em Apóstolos, Policial já está
em Humanos. **REGRA (Gabriel):** ao chegar nessas passivas, **PARAR e reavaliar este ponto** —
decidir juntos se vale unir a seleção; não construir especulativo antes da hora.

**RESOLUÇÃO (jul/2026 — as 3 avaliadas, fio FECHADO):** as três passivas foram deixadas como
estão, de propósito. Cada uma já usa o primitivo unificado (`AumentarDuracao`/`ReduzirDuracao`);
fundir o dispatch numa `Acao` foi rejeitado (passiva ≠ ação — o `IReageAo*` é o invólucro certo
pra "reagir a ser atacado"). Unir a seleção-de-status via `Seletor` nas 3 seria cerimônia sobre 3
usos triviais (um estende Preso, um estende 1 debuff aleatório, um reduz todos os buffs) — YAGNI.
Se um dia nascer uma 4ª passiva de duração com seleção complexa, reabrir; até lá, fechado.

**A família do revive (descoberta jul/2026, lendo os usuários de `EstadoAlvo.Ambos`):**
`Reviver` tem **7 clientes** — Nigiri ✅ (Humanos), Tecnology ✅ (Tecnológicos), Céu (Apóstolos),
AnjoCaído (Decaídos), DocesDeAbobora ✅ (LadoSombrio, revive **SÓ 1**), Circo ✅ (Folclore) e
Atlantis ✅ (Místicos). Circo e Atlantis usam `buffNoRevivido` (Intocável só nos revividos — §8.1 dissolvido).

**REGRA DO REVIVE (decisão de Gabriel, jul/2026):** a ação `Reviver` é per-alvo simples, só
`percentualHP` — a SELEÇÃO de quantos/quais revive NÃO mora na ação, mora no mecanismo único
de seleção do jogo (`ResolverAlvos`):
- **Revive-de-todos** (Nigiri, AnjoCaído...): `Reviver(pct)` com escopo default
  (`TodosAliados`/`Mortos`), sem pick.
- **Revive-de-N** (DocesDeAbobora, N=1): a HABILIDADE declara `numeroDeAlvos: N` +
  `TipoAlvo.Aleatorio` + `EstadoAlvo.Mortos`, e a ação usa `Escopo.AlvosResolvidos`. O jogador
  ESCOLHE o morto (pick real por estado — ADR-selecao-por-estado §2.4) e os extras são
  sorteados: **selecionado + random**, a semântica que o `Aleatorio` já tem. Duplicata do
  sorteio é inofensiva (`Vivo.Reviver` é no-op). Uma 1ª versão usava `quantos` + `Take` dentro
  da ação — REVERTIDA: duas formas de selecionar N alvos no mesmo motor é anti-padrão.
  O DocesDeAbobora ganhou de quebra o pick real ("primeiro da lista" era a dor apontada no
  ADR-selecao-por-estado); o `CombateService` ganhou o guard pra pick sem candidato (revive
  sem mortos: pula o pick, as demais ações rodam — o Reflexo ainda vale).

A Sentença é checada central no `Morto.Reviver` — a ação não escreve nada disso. Esses 7 são
exatamente os usuários do `Ambos`; migrados eles, o enum-value e o check do
`CombateService:282` morrem juntos. **5 de 7 feitos** (Nigiri, DocesDeAbobora, Tecnology, Circo,
Atlantis; faltam Céu, AnjoCaído).

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

### 10.2 Ordem dos temas — REVISADO (jul/2026): Nível A FUNDIDO no sweep
A 1ª versão separava o Nível A (champ-como-arquivo) do motor. **Revisado com o motor pronto:**
manter separado custaria tocar ~70 habilidades DUAS vezes (uma pra "subclasse com Acoes", outra
pra forma-construtor no arquivo do champ). Decidido: um PR de infra deixou `HabilidadeAtiva`
construível por construtor (props `virtual` com backing — as subclasses Strangler seguem
válidas), e **cada PR de facção migra os champs direto pra forma FINAL** (pasta + habilidades
como métodos + passiva movida + classes velhas deletadas). Uma passada por champ; a view chega
facção a facção; o `PersonagemService` encolhe até virar o `Roster`. O custo aceito: PRs de
facção maiores (movem passiva junto). Piloto: **Mago** (`Champs/Reino/Mago/`).

---

## 11. Sequência de implementação

- **PR #115 ✅:** modelo de `Acao` + interpretador per-alvo + `Dano`/`AplicarDebuff` +
  piloto Mago (BolaDeFogo, Incendio). Foi o formato per-alvo, não o motor.
- **PR #116 ✅ (o motor):** loop-flip (§3.2) + `Escopo` + `EstadoAlvo` por ação avaliado na
  execução + fragmentos de `Valor` + `AplicarBuff`/`Cura`. Pilotos: Furtividade (escopo
  próprio), Sushi (Cura+PorHP), Prender. Verificado em jogo.
- **Forma-construtor + champ-arquivo ✅:** `HabilidadeAtiva` concreta com dois construtores
  (dado + subclasse Strangler); Mago como piloto da forma final (`Champs/Reino/Mago/`);
  AtaqueBasico híbrido (Acoes + `AtivarComNatureza`); `Ambos` numa ação = sem filtro.
- **Testes do motor ✅:** 11 testes xUnit do interpretador (escopo, estado-na-execução,
  ordem, agregação, Aleatorio com duplicata) — feitos ANTES do sweep, o motor é infra
  load-bearing. Rede de regressão de cada PR de facção: `dotnet test`.
- **Sweep por facção, forma final (§10.2): Humanos ✅, Reino ✅, LadoSombrio ✅** (Guarda/
  Ninja/Rei; Caveira/Fantasma/Abóbora/Zumbi — `AplicarEscudo` promovido de vocabulário-mapeado
  a Ação real (Lealdade), `Dano` ganhou `ignorarDefesaPct`/`forcaCritico` opcionais (Kunai),
  Shuriken estreou Nível 3 (`GolpeSeguidor`). LadoSombrio foi o momento de design, com duas
  rodadas de revisão de Gabriel por cima do sweep: a regra do revive (per-alvo + pick do motor,
  ver §9) e o rebalance da Putrefação (cura 20% do dano total, não média) — que matou o único
  cliente da `AcaoSobreConjunto` (construída e REMOVIDA no mesmo sweep, §3.4) e fez nascer o
  **`Explodir` genérico** (`Seletor` + `IStatusComTick.Detonar(portador, detonador) →
  EventoDano`; Putrefação 1º cliente; Inferno segue no shim `Queima.Explodir` até Decaídos);
  `Escopo.OutrosAliados` real (OssoDuroDeRoer, 1º dos 2 clientes — falta Circo);
  `RemoverBuffs`/`Seletor` reais (DocesOuTravessuras).
  **Tecnológicos ✅** (Invasor/Alien/Robô/Cientista): Barata estreou o estado/ao-matar
  dissolvido em `Dano` + `AplicarDebuff(Mortos)` (sem condicional "se matou"); Tecnology é o 3º
  do Reviver (revive-de-todos); `EstenderBuffs` nasceu bespoke-local no Robô (RaioX), espelho do
  `RemoverBuffs` — ver §9; Galáxia entrou como cliente de `OutrosAliados`; consolidado
  `StatusEffect.EstenderTurno` → `AumentarDuracao(1)` (eram idênticos). Cura/extensão do RaioX e
  as ações da Barata seguem o princípio DECOMPOR (§3.3).
  **Folclore ✅** (Ogro/Tengu/Palhaço/Troll): `RemoverDebuffs` nasceu (Coringa, gêmeo do
  `RemoverBuffs`); `Dano` ganhou `ignorarStatus` (CorteDeVento/Vendaval) e `AplicarDebuff` ganhou
  `chance` (Pancada) + overload de proveniência `Func<Combate,Debuff>` (Irritar/Quebrar); Circo é o
  4º do revive e mais um cliente de `OutrosAliados` (com o Esmagar); Porradeiro = molde do Tiroteio
  + cura do Zumbi (ZERO bespoke — Gabriel cortou a ideia de bespoke que eu tinha).
  **Místicos ✅** (Gênio/Sereia/Fada/Dragão): o "pipeline §8.1" foi **DISSOLVIDO** — a `Reviver`
  ganhou `buffNoRevivido` (Intocável só nos revividos), o que fez o Atlantis (5º do revive) e
  **CONSERTOU o Circo** (bugfix: antes pegava todos os outros vivos). PoMágico virou vocabulário
  puro (o `ignorarStatus` passou a casar por tipo-BASE — `typeof(Buff)` = todos os buffs);
  `RestaurarHPMaximo` = bespoke local no Dragão. A unificação dos 3 mecanismos de ignorar fica pra
  tema próprio (na hora do Vampiro/Decaídos).
  **Especial ✅** (Cocô/Herói/Vilão/T-Rex): 100% vocabulário puro, ZERO molde novo/bespoke —
  a 1ª facção totalmente mecânica. DestruindoDia = 2º cliente do `RemoverDebuffs`, SalvandoDia =
  mais um de `OutrosAliados`. Colisão do nome "Fedorento" RESOLVIDA: o ativo do Zumbi virou "Vômito
  Tóxico" (método `VomitoToxico`) e a passiva do Cocô ficou classe `Fedorento`.
  Colisão do nome "Invencível" RESOLVIDA (PR de refactor pós-#129): a passiva do Guarda virou classe
  `GuardaReal` com nome de jogo "Guarda Real" — colidia com o buff `Skills.Buffs.Invencivel` que ela
  própria aplica. Com isso as duas colisões nomeadas no #117 (Fedorento e Invencível) estão fechadas.
  **Decaídos ✅** (Morcego/Vampiro/Elfo/Diabo): 100% vocabulário puro, ZERO bespoke. `ConcederTurnoExtra`
  **construído** (1º cliente real = Rato Voador, não o Copiando como o catálogo previa); Inferno migrou
  pro `Explodir` genérico e o shim `Queima.Explodir` MORREU (os EventoDano da explosão passam a entrar
  no pipeline — antes o Inferno os descartava); Anjo Caído (6º do revive) = `RemoverDebuffs`(Sentença,
  Mortos)+`Reviver`+`Cura` — a ordem das ações quebra a Sentença antes de reviver, sem bespoke. Renomes
  de jogo do Vampiro (pedido do Gabriel): Bat Man→"Controle de Sangue" 🩸, Cinto de Utilidades→"Vampiro
  Primordial" 🌙. Colisão "Espinhos" RESOLVIDA: a passiva do Elfo virou classe `EspinhosCorrompidos`
  ("Espinhos Corrompidos") — colidia no display com o buff `EspinhosVenenosos` (que exibe "Espinhos").
  **A unificação-do-ignorar NÃO foi feita aqui** (a Drenagem é fonte, não muda): vira PR próprio logo
  após, com desenho fechado (natureza fala a língua da lista, os `DeveAgir` morrem). Revive 6/7.
  **Apóstolos ✅ — SWEEP DAS 9 FACÇÕES COMPLETO** (Boneco de Neve/Mímico/Anjo/Papai Noel): 100%
  vocabulário puro, ZERO bespoke. `MoverBuffs` **construído** (gêmeo do `RemoverBuffs`, move a
  instância; cliente = Copiando) — com ele o vocabulário mapeado esgotou (só resta `AcaoSobreConjunto`,
  sem cliente). Imitação usa `Dano(Func)` (escala com os buffs do Mímico, molde do Tengu). Céu = **7º e
  ÚLTIMO da família do revive** (`Reviver`+2×`AplicarBuff`); era o último champ com `EstadoAlvo.Ambos` —
  agora NENHUM champ usa `Ambos`, o que desbloqueia a remoção do enum no §8.2 (pick-do-menu). **Fio §9
  fechado:** Repetindo (3ª e última passiva de duração) avaliada e deixada como está, igual a
  AnáliseCrítica e Policial — passiva ≠ ação, o dispatch não funde (só o primitivo `AumentarDuracao` e
  o conceito de Seletor são compartilhados). `PersonagemService` não instancia mais nenhum champ à mão.
  Segue: unificação-do-ignorar → pick-do-menu/§8.2 (o `Ambos` morre). Facção que estreia mecanismo =
  momento de design, não sweep.
- **Pick do menu (§8.2):** o lado UI, quando o `Ambos` morrer (pós-família-do-revive).
- **Depois:** rename do repo/namespace (§12).

Um PR, um tema. Build verde o tempo todo (Strangler). **Docs bumpam NO MESMO PR do código.**

---

## 12. Fora de escopo deste fio (follow-ons, sem data)

- **`IModificaDanoCausado`** — a ação `Dano` consulta modificadores do atacante sozinha (a
  `PassivaPiromancer` para de ser fiada à mão). Espelho do `IModificaDanoRecebido`. Desenhar
  a `Dano` sem fechar essa porta, mas não implementar agora.
- **Champ-como-dado + pastas** (§10) — tema separado, depois do motor provar-se.
- **`EventoDano` por ID** — hoje carrega objetos vivos (`Combate`); referenciar por id/nome
  desacopla o log/stream dos objetos.
- **Rename do repo/namespace ✅** — tirado o `v1` (`v1_Apostle_s_War` → `ApostlesWar`, sln/csproj junto).

---

## 13. Validação

- Cada habilidade migrada compila e joga idêntico ao de hoje (mesmo dano, mesmos status,
  mesma ordem), agora como config em vez de classe.
- As passivas-do-atacante disparam o mesmo nº de vezes (`TipoAtaque` preservado).
- Convivência Strangler: só as habilidades migradas passam pelo motor novo; o resto segue no
  `Ativar` velho, sem mudança de comportamento.
