# ADR — PR-C: Reações Unificadas e Orquestradas

> **Tipo:** Architecture Decision Record
> **Status:** Aceito (Forma 2 / interfaces; fatiamento Strangler Fig)
> **Contexto:** Apostle's War. Implementa a Separação 2 do ADR de Sistema de
>   Efeitos. Sucede PR-A (unificar Atacar), PR-B (NaturezaDano), fix (órfãos)
>   e PR-consequencia (TipoReacao + DanoIndireto).
> **Data:** junho/2026

---

## 1. Problema

Cinco efeitos de status reagem a dano via hooks (AoSerAtacado / AoReceberDano
/ AoCausarDano) chamados DENTRO do ReceberDano/Atacar. Isso causa:

- Sem mensagem: o efeito roda longe do MenuService (ex: contra-ataque e
  reflexo não exibem o dano que causaram).
- Recursão frágil: ContraAtaque chama Atacar dentro do ReceberDano; o
  _emCooldown é band-aid contra loop.
- Contra-ataque só revida 1 atacante por turno (bug do _emCooldown).
- Operário (passiva) faz Console.WriteLine direto, furando a camada.

As PASSIVAS, em contraste, já são orquestradas pelo CombateService (eventos
do enum EventoCombate, processadas em ProcessarPassivasAlvo/Atacante, com
mensagem e controle de fluxo). O objetivo é levar os status reativos pro
mesmo modelo.

---

## 2. Os cinco efeitos reativos (mapa completo)

### Lado do ALVO (quem recebe o golpe)

| Efeito | Hook hoje | O que faz | Age sobre |
|--------|-----------|-----------|-----------|
| ContraAtaque (buff) | AoSerAtacado | revida com a1 | o atacante |
| EspinhosVenenosos (buff) | AoSerAtacado | aplica Veneno+Queima | o atacante |
| RefletirDano (buff) | AoReceberDano | reflete % do dano | o atacante |
| Sangramento (debuff) | AoReceberDano | cura o atacante em 15% | o atacante |

### Lado do ATACANTE (quem desfere o golpe)

| Efeito | Hook hoje | O que faz | Age sobre |
|--------|-----------|-----------|-----------|
| Sedento (buff) | AoCausarDano | cura o portador em % | o próprio portador |

Observação: AoSerAtacado dispara mesmo com dano 0 (reage ao ATO);
AoReceberDano só com dano > 0 (reage ao SOFRIMENTO). Essa distinção precisa
ser preservada.

---

## 3. Decisão de arquitetura

### 3.1 Buffs e passivas compartilham o ponto de orquestração

O CombateService já dispara eventos pras passivas em:
- ProcessarPassivasAlvo  -> DepoisDeSerAtacado, DepoisDeReceberDano (lado alvo)
- ProcessarPassivasAtacante -> DepoisDeAtacar (lado atacante)

A decisão: esses MESMOS pontos passam a processar TAMBÉM os status reativos
(StatusAtivos), não só as Habilidades (passivas). Buff continua buff (em
StatusAtivos, removível, com duração); passiva continua passiva. Só
compartilham o ponto onde a reação é disparada e exibida.

### 3.2 Os hooks saem do ReceberDano

Os hooks AoSerAtacado / AoReceberDano deixam de ser chamados dentro do
ReceberDano. Em vez disso, o CombateService os invoca no momento orquestrado
(depois de exibir o dano, com MenuService disponível).

O AoCausarDano (Sedento) sai do Atacar e passa a ser processado no lado do
atacante (ProcessarPassivasAtacante / equivalente).

### 3.3 Como o status declara que reage — INTERFACES (Forma 2, decidido)

DECISÃO: interfaces específicas por momento de reação, implementadas tanto
por buffs (StatusEffect) quanto por passivas (HabilidadePassiva). Substitui o
DeveAtivar(evento)+enum das passivas. Escolhida por:
- Interface Segregation (SOLID): só quem reage a um momento implementa a
  interface daquele momento. Escudo/BuffAtaque não carregam método inútil.
- Type-safe: o compilador garante o método; não existe "evento errado".
- Escala sem mexer em enum central: momento novo = interface nova.
- Alinhado ao crescimento de longo prazo do projeto (hobby, sem prazo,
  modelo reaproveitável em outros jogos).

Três interfaces, uma por momento (espelham AoSerAtacado/AoReceberDano/
AoCausarDano):

```
interface IReageAoSerAtacado   { List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx); }
interface IReageAoReceberDano  { List<ResultadoReacao> AoReceberDano(ContextoReacao ctx); }
interface IReageAoCausarDano   { List<ResultadoReacao> AoCausarDano(ContextoReacao ctx); }
```

Mapa de quem implementa o quê:
- IReageAoSerAtacado:  ContraAtaque, EspinhosVenenosos (reagem ao ATO, dano 0 ok)
- IReageAoReceberDano: RefletirDano, Sangramento (só com dano > 0)
- IReageAoCausarDano:  Sedento (lado do atacante)

### 3.3.1 ContextoReacao (entrada da reação)

Carrega tudo que a reação precisa, incluindo a NaturezaDano do golpe (pra
ContraAtaque/Reflexo consultarem TipoReacao):

```
record ContextoReacao(
    Combate Portador,        // quem tem o efeito reativo
    Combate Outro,           // o atacante (lado alvo) ou o alvo (lado atacante)
    int DanoCausado,         // dano efetivo do golpe que disparou (0 se absorvido)
    NaturezaDano Natureza    // natureza do golpe — pra consumir TipoReacao
);
```

### 3.3.2 ResultadoReacao (saída da reação)

O CombateService precisa exibir o que a reação fez. Como reações produzem
dano, cura OU só aplicação de status, o resultado é unificado:

```
record ResultadoReacao(
    string Mensagem,         // texto a exibir (ex: "Heroi contra-atacou!")
    ResultadoAtaque? Dano = null,  // se a reacao causou dano (revide/reflexo)
    int Cura = 0             // se a reacao curou (Sedento)
);
```

(Assinatura a refinar na implementação. O ponto é: a reação DECLARA o que
fez via ResultadoReacao; o CombateService EXIBE. A reação não chama
MenuService diretamente — resolve o problema do Operario furar a camada.)

ALTERNATIVA considerada: reusar ResultadoAtaque pra tudo (cura = dano
negativo). Rejeitada — cura não é dano negativo, seria gambiarra semântica.

### 3.4 Consumo do TipoReacao (loop quebrado)

O PR-consequencia preparou NaturezaDano.Reacao (Completa/SemContraAtaque/
Nenhuma). Agora ele é CONSUMIDO:

- ContraAtaque só reage se a natureza do golpe recebido for Completa
  (não SemContraAtaque, não Nenhuma). Assim um Revide (SemContraAtaque) não
  gera outro contra-ataque -> loop quebrado.
- RefletirDano reage a Completa E SemContraAtaque (reflete ataque e revide),
  mas NÃO a Nenhuma (não reflete veneno/queima/dano indireto/reflexo).
- O revide do ContraAtaque é desferido com NaturezasDano.Revide
  (SemContraAtaque) -> o alvo reflete (se tiver RefletirDano), mas não
  contra-ataca de volta.

### 3.5 Contra-ataque revida TODOS os atacantes

Como a reação passa a ser disparada por evento (uma vez por golpe recebido,
no ProcessarPassivasAlvo que já roda por golpe), o _emCooldown deixa de ser
necessário. Cada golpe recebido dispara a avaliação do contra-ataque. Três
inimigos atacando o Herói em turnos diferentes = três disparos = três
revides. Remove-se o _emCooldown.

---

## 4. O que muda em cada efeito

- ContraAtaque: deixa de sobrescrever AoSerAtacado; implementa a interface de
  reação. Revida via NaturezasDano.Revide. Só reage a golpe de natureza
  Completa. Remove _emCooldown. Resultado do revide é exibido.
- EspinhosVenenosos: deixa AoSerAtacado; implementa a interface. Reage a
  golpe (mesmo dano 0). Aplica Veneno+Queima no atacante.
- RefletirDano: deixa AoReceberDano; implementa a interface. Reage só com
  dano > 0, a naturezas Completa/SemContraAtaque. Reflete via
  NaturezasDano.DanoIndireto (já feito no PR-consequencia). Resultado exibido.
- Sangramento: deixa AoReceberDano; implementa a interface. Cura o atacante.
- Sedento: deixa AoCausarDano; implementa a interface no lado do atacante.
- Operario (passiva): para de fazer Console.WriteLine; retorna ResultadoAtaque
  e deixa o CombateService exibir (já é quase assim).

---

## 5. Riscos

- É o coração do combate. Mexe em ReceberDano, Atacar, CombateService, os 5
  status reativos E todas as passivas (migração pro modelo de interface).
  Alto risco de regressão — mitigado pelo fatiamento Strangler Fig (§6).
- A ordem das reações do alvo importa: definida em §7.3 (Reflexo -> Espinhos
  -> ContraAtaque).
- Migrar as passivas de DeveAtivar/enum pra interfaces é a maior superfície.
  O sistema velho coexiste com o novo até a última migração (C5).
- Confirmar que os efeitos do Grupo A (ModificarDanoRecebido: Escudo,
  Bloqueio, Invencivel, ProtecaoAliado, ReducaoDanoFixo) NÃO são tocados —
  eles ficam dentro do ReceberDano, são cálculo de dano, não reação.

---

## 6. Plano de sub-fatiamento (Strangler Fig — Forma 2)

A Forma 2 (interfaces) exige migrar TAMBÉM as passivas (hoje em DeveAtivar+
enum), senão ficam dois modelos. Pra não fazer big bang, usa-se o padrão
Strangler Fig: constrói o sistema novo AO LADO do velho, migra aos poucos,
remove o velho no fim. Cada sub-PR compila e o jogo roda.

- **C1 — Fundação (sem migrar ninguém).** Cria as 3 interfaces
  (IReageAoSerAtacado/ReceberDano/CausarDano), os records ContextoReacao e
  ResultadoReacao, e o dispatch no CombateService que varre StatusAtivos +
  Habilidades por essas interfaces. O dispatch novo roda EM PARALELO ao
  sistema antigo (hooks no ReceberDano + DeveAtivar das passivas). Ninguém
  implementa as interfaces ainda -> nada muda de comportamento, tudo compila.

- **C2 — Prova de conceito: migra Sedento + 1 passiva simples.** Move o
  Sedento (AoCausarDano) e uma passiva (ex: Detetive) pro modelo novo.
  Remove os hooks/DeveAtivar SÓ desses dois. Valida a fundação com 1 de cada
  tipo. Se a fundação está boa, o resto é repetição.

- **C3 — Migra RefletirDano + Sangramento** (AoReceberDano, dano > 0).

- **C4 — Migra EspinhosVenenosos** (AoSerAtacado, dano 0 ok).

- **C5 — Migra as passivas reativas restantes** (Operario, Necromancia,
  Guarda, etc) pro modelo de interface. Remove DeveAtivar/enum quando a
  última passiva migrar.

- **C6 — ContraAtaque** (o mais complexo): migra, revida todos, consome
  TipoReacao (só reage a Completa), revide com NaturezasDano.Revide, remove
  _emCooldown. Por último, com tudo maduro.

- **C7 — Limpeza:** remove os hooks mortos do StatusEffect (AoSerAtacado/
  AoReceberDano/AoCausarDano) e o DeveAtivar/EventoCombate se não sobrar uso.

Ordem das reações do alvo (decisão §7.3) aplicada no dispatch: Reflexo ->
Espinhos -> ContraAtaque.

NOTA: é mais fatias que o plano antigo, de propósito. Cada uma é segura
porque o sistema velho coexiste com o novo até a migração terminar.

---

## 7. Decisões pendentes (resolver antes/durante a implementação)

1. **RESOLVIDO** — Forma 2: três interfaces (IReageAoSerAtacado/ReceberDano/
   CausarDano) + records ContextoReacao e ResultadoReacao. Ver §3.3.
2. **RESOLVIDO** — "Houve dano?" NÃO vira um campo. A distinção vira DUAS
   interfaces distintas: IReageAoSerAtacado (disparada sempre, mesmo dano 0 —
   ContraAtaque, EspinhosVenenosos) e IReageAoReceberDano (disparada só com
   dano > 0 — RefletirDano, Sangramento). A interface implementada JÁ É a
   distinção; estado inválido (refletir sem dano) fica irrepresentável.
3. **RESOLVIDO** — Ordem das reações do lado do alvo:
   (1) RefletirDano, (2) EspinhosVenenosos, (3) ContraAtaque (SEMPRE por último).
   Efeitos passivos/defensivos antes; o revide ativo por último.
4. Onde o ContextoReacao carrega a NaturezaDano do golpe (pra consumir
   TipoReacao).
5. Confirmar que mover os hooks não quebra o ProtecaoAliado/Invencivel/etc
   (Grupo A, ModificarDanoRecebido — devem ficar no ReceberDano, intocados).
