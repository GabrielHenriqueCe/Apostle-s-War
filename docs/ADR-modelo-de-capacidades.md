# ADR — Modelo de Capacidades (Interfaces por Capacidade)

> **Tipo:** Architecture Decision Record
> **Status:** Aceito (destino arquitetural; migração incremental)
> **Contexto:** Apostle's War. Consolida a direção iniciada no PR-C (reações
>   via interface) e a generaliza para TODA forma de um status/passiva
>   interagir com o combate.
> **Data:** junho/2026

---

## 1. Princípio central

> **Todo método que é "morto" (corpo vazio / sem efeito) na maioria das
> classes que o herdam deveria ser uma interface, não um método virtual na
> base.**

É o Interface Segregation Principle (o "I" do SOLID) aplicado: uma classe não
deve ser forçada a carregar métodos que não usa. Hoje, Veneno carrega um
ModificarDanoRecebido vazio; Medo carrega um ContribuicaoDefesa que retorna 0.
São métodos mortos nessas classes — sinal de que deveriam ser interfaces.

Consequência: a classe base (StatusEffect / HabilidadePassiva) fica MAGRA — só
o que é genuinamente comum a todos. Tudo que varia entre subtipos vira
interface de capacidade. A declaração da classe passa a DOCUMENTAR o que ela
faz:

```
class Escudo : Buff, IModificaDanoRecebido          // intercepta dano
class BuffDefesa : Buff, IContribuiDefesa            // contribui defesa
class EspinhosVenenosos : Buff, IReageAoSerAtacado   // reage ao ser atacado
class Veneno : Debuff                                // só tick — nenhuma capacidade reativa
```

Quem cria um status/passiva novo olha a declaração e sabe qual padrão seguir.
"É um buff que intercepta dano? implementa IModificaDanoRecebido. É uma passiva
que reage ao ataque? implementa IReageAoSerAtacado. Mexe diferente? procuro a
interface da capacidade certa (ou crio uma nova se o padrão faltar)."

---

## 2. As categorias de interação com o combate

Cada categoria é uma forma distinta de um status/passiva tocar o combate. Cada
uma tem (ou terá) seu conjunto de interfaces de capacidade.

### Categoria A — Reação após evento
Dispara DEPOIS que um evento de combate ocorreu (o dano já foi aplicado, etc).
A reação declara o que fez (ResultadoReacao); o CombateService exibe.

Interfaces (uma por MOMENTO):
- IReageAoSerAtacado   — ato de ser atacado (dispara mesmo com dano 0)
- IReageAoReceberDano  — sofrimento (só com dano > 0)
- IReageAoCausarDano   — ao causar dano (lado atacante)
- IReageAoAtacar       — ao atacar (FALTA criar — eixo de OlhoClinico, Virus,
                          Sorrateiro, Robo, Policial)
- IReageAoMorrer       — à própria morte (FALTA criar — Necromancia)
- IReageAoMatar        — ao matar alguém (FALTA criar — Fada, Vilao)

Status: IReageAoSerAtacado/ReceberDano/CausarDano já existem (PR-C). Os outros
três faltam.

### Categoria B — Intervenção no dano (durante)
Modifica o dano ENQUANTO ele acontece, antes do HP cair. Diferente da reação
(que é depois). Dois lados:
- **Defensor** (`IModificaDanoRecebido`, int→int no ReceberDano). Implementadores:
  Escudo, BloqueioTotal, Invencivel, ProtecaoAliado, ReducaoDanoFixo, Aquagirl.
- **Atacante** (`IModificaDanoCausado`, forma multiplicador, consultado pela Ação
  Dano — FILA A #10). Uma passiva/status do atacante que modifica o dano que ELE
  causa. Implementador: Piromancer (+25% vs alvo com Queima).
  NÃO confundir com fórmula-DA-HABILIDADE (`Dano(Func)`, ex: Caveira `2.0-HP%`,
  Tengu `1.0+escudo do alvo`) — aquilo é da hab, não do atacante.

### Categoria C — Modificação de stat (sob demanda)
Soma/subtrai de um stat, calculado sob demanda pelos getters (Stats em
Camadas). Buff e debuff do mesmo stat são ESPELHOS (mesmo mecanismo, sinal
trocado): compartilham a Contribuicao<Stat>, com sinal.
✅ CONSTRUÍDO pros 4 stats (FILA A #8, jul/2026): IContribuiAtaque,
IContribuiDefesa, IContribuiTaxaCrit, IContribuiDanoCrit. Cada getter
(Ataque/Defesa/TaxaCrit/DanoCrit) SOMA a interface — nenhum olha tipo concreto.
Implementadores (matriz simétrica): BuffAtaque/ReducaoAtaque, BuffDefesa/
ReducaoDefesa, BuffTaxaCrit/ReducaoTaxaCrit, BuffDanoCrit/ReducaoDanoCrit.
Nota: passivas stat-builders permanentes (Ambicao, CoroaDoSoberano, Virus,
OlhoClinico) NÃO são esta categoria — elas REAGEM a um momento (Categoria A) e,
como efeito, chamam AdicionarBonusXPermanente. O gatilho é reação; o efeito é
stat. Categoriza-se pelo gatilho.

### Categoria D — Modificação de comportamento de turno
Altera o que o portador pode fazer no próprio turno. Consultado por quem decide
a ação, não dispara sozinho. ✅ FEITO (FILA A #9, jul/2026): três capacidades por
FASE (não uma "família" só — as formas diferem), cada status dono do próprio
comportamento; o CombateService parou de decidir por tipo concreto.
- `IParalisaAcao` — Medo (`Paralisa()` — chance de cancelar a ação, após a escolha)
- `IPulaTurno` — Preso (marcador — pula o turno inteiro, antes da escolha). É a PORTA
  da família de controle-de-turno: Congelar/Stun/Enraizado/Petrificado plugam aqui sem
  tocar no fluxo (diferenças de cada um = outras capacidades compostas).
- `IForcaAcao` — Irritar (`AlvoForcado()` — o status decide o alvo da A1 forçada)

### Categoria E — Bloqueio de aplicação
Impede que outros status sejam aplicados no portador. Hoje é o método virtual
Bloqueia.
Vira: IBloqueiaStatus.
Implementadores: ImunidadeDebuffs, ImunidadeEspecifica, ImpedirBeneficios.

---

## 3. O que JÁ está correto (não confundir com "falta fazer")

- BuffAtaque/Defesa/TaxaCrit e ReducaoDefesa: corretos, via Stats em Camadas.
  ReducaoDefesa já espelha BuffDefesa (ContribuicaoDefesa com sinal trocado).
- Os buffs reativos (Sedento, Reflexo, Sangramento, Espinhos, ContraAtaque):
  migrados pra Categoria A (PR-C, fatias C2-C6).
- As passivas e debuffs funcionam corretamente HOJE. O trabalho é PADRONIZAR
  (mover pro modelo de capacidade), não consertar bugs.

---

## 4. Ordem de migração (cada categoria = tema/PR próprio)

Régua: migrar por onde DÓI. A dor atual é a Categoria A (passivas reativas
divergindo dos buffs reativos). As outras já funcionam e são consistentes —
migram quando tocadas (boy scout) ou em PRs dedicados sem urgência.

1. **Categoria A — reações** (EM CURSO):
   - Buffs reativos: feito (C2-C6).
   - Passivas reativas: PRÓXIMO. Tornar DeveAtivar/Ativar opcionais na base
     HabilidadePassiva; passivas reativas implementam as interfaces; dispatch
     varre StatusAtivos + Personagem.Habilidades; sistema velho (DeveAtivar/
     enum) aposentado quando a última passiva migrar.
   - Criar as interfaces faltantes (IReageAoAtacar/Morrer/Matar).
2. **Categoria B — intervenção no dano** (depois): IModificaDanoRecebido.
3. **Categoria E — bloqueio** (depois): IBloqueiaStatus.
4. **Categoria C — stat** ✅ FEITO (FILA A #8): IContribui* pros 4 stats, matriz simétrica.
5. **Categoria D — comportamento de turno** (baixa prioridade): avaliar se vale
   interface ou se a consulta atual basta.

---

## 5. Detalhes da migração das passivas (Categoria A — o próximo trabalho)

### 5.1 Base HabilidadePassiva
DeveAtivar e Ativar passam de abstract para virtual com corpo default (retornam
false / SemDano). Assim uma passiva migrada NÃO é obrigada a implementá-los —
usa só as interfaces de reação.

### 5.2 Coexistência sem dupla execução
Durante a migração, sistema velho (DeveAtivar em ProcessarPassivasAlvo/
Atacante) e novo (interfaces em ProcessarReacoes*) coexistem. Uma passiva
migrada NÃO pode disparar pelos dois. O loop velho pula quem implementa
interface de reação (if hab is IReageAo... continue), ou a passiva migrada
passa a ter DeveAtivar => false. Definir na implementação.

### 5.3 Dispatch varre DUAS fontes
ProcessarReacoesAlvo (e equivalentes) varrem alvo.StatusAtivos +
alvo.Personagem.Habilidades por interface. Hoje só varrem StatusAtivos
(buffs/debuffs); passivas estão em Habilidades.

### 5.4 Cooldown
Passivas têm Cooldowns[hab] (Necromancia 6, Guarda 4); buffs não tinham. O
dispatch de reação checa o cooldown quando a coisa que reage for uma Habilidade
com cooldown (igual o ExecutarPassivasReativas velho já faz). Sem novidade —
só replicar o check no dispatch novo.

### 5.5 Casos que precisam de decisão na implementação (não são dúvidas de
arquitetura, são de execução)
- **Guarda:** hoje é um HACK (revive com 1 HP). A intenção real é "impedir a
  morte": interceptar o golpe fatal e parar o HP em 1 + Invencivel. Isso é
  Categoria B (intervenção no dano via Invencivel), não reação/revive. Se for
  simples implementar do jeito certo, fazer; se for difícil, ajustar o conceito
  da passiva. Decidir ao implementar.
- **Necromancia:** reviver reativo (IReageAoMorrer) com cooldown. Único caso de
  revive reativo (os outros revives — Nigiri, AnjoCaido, Atlantis etc — são
  ATIVAS, fora do escopo). O contexto da reação precisa permitir checar
  "morreu?" e o cooldown.
- **Sushiman:** reage a crítico (precisa de FoiCritico) e aplica em aliados
  (precisa da lista de aliados). O ContextoReacao atual não tem nem FoiCritico
  nem Aliados. Resolver quando o EventoDano enriquecer o contexto, OU adicionar
  os campos ao ContextoReacao na migração das passivas.

---

## 6. Relação com o EventoDano (próximo grande tema, pós-reações)

O ContextoReacao atual (Portador, Outro, DanoCausado, Natureza) é magro. Casos
como Sushiman (FoiCritico, Aliados) pedem mais. O EventoDano — registro rico de
um evento de dano (bruto, efetivo, absorvido, crítico, atacante, alvo,
natureza) — é a evolução natural do ContextoReacao, e alimenta também o porte
Unity (a apresentação consome eventos). A fila de eventos (Forma 3, adiada no
C6) nasce junto com o EventoDano. O revide hoje usa Forma 1 (recursão de
profundidade 1, segura pela natureza Revide) e migra pra fila quando o
EventoDano vier.

---

## 7. O que NÃO é deste modelo (fora de escopo)

- Habilidades ATIVAS (74): são ações escolhidas, não reações. Não implementam
  interfaces de reação. Podem CONCEDER buffs reativos a alguém (ArvoreDoMundo,
  DragaoProtetor, DocesDeAbobora aplicam Reflexo/ContraAtaque/Proteção), mas
  isso é consumir o sistema, não fazer parte dele.
- Debuffs de tick (Veneno, Queima, Maldicao): agem no AoIniciarTurno, já usam
  as naturezas corretas (PR-B). Não são reação.
