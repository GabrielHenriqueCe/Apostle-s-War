# ADR — Unificação do Sistema de Efeitos e Dano

> **Tipo:** Architecture Decision Record (registro de decisão de arquitetura)
> **Status:** Aceito (Fase 1 concluída — decisões §5 e §6 finalizadas)
> **Contexto:** Apostle's War, alfa console. Sucede o refator de Stats em Camadas.
> **Data:** junho/2026

---

## 1. Problema

Hoje existem DOIS sistemas paralelos para "efeitos que reagem a eventos de
combate", e TRÊS caminhos diferentes para "causar dano". Essa fragmentação
gera bugs, código duplicado, e efeitos que não conseguem reportar o que
fizeram à camada de exibição.

Sintomas concretos observados:
- Contra-ataque (buff) só revida 1 atacante (deveria revidar todos) e não
  exibe mensagem do dano causado.
- Operário (passiva) faz Console.WriteLine direto, furando a camada de UI.
- Veneno/Queima/Maldição reduzem vida sem reportar quanto.
- Cura não reporta quanto curou.
- Atacar e AtacarComMultiplicador são ~90% código duplicado.

---

## 2. Como funciona hoje (mapa)

### 2.1 Caminhos de dano (três)

1. **Atacar(alvo)** — calcula crítico, chama ReceberDano, dispara AoCausarDano.
2. **AtacarComMultiplicador(alvo, mult, ignorarDefesaPct, forcaCritico, ignorarStatus)**
   — idêntico ao Atacar mas parametrizado. Atacar é um caso particular deste
   (mult=1.0).
3. **ReceberDanoDireto(dano)** — só subtrai HP, sem defesa, sem hooks. Usado
   por Veneno/Queima (dano que ignora tudo).

Ponto de convergência: **ReceberDano** processa defesa, ModificarDanoRecebido
(Escudo/Bloqueio), subtrai HP, e dispara AoSerAtacado/AoReceberDano.

### 2.2 Reação a eventos (DOIS mundos paralelos)

| Momento            | StatusEffect (override)          | HabilidadePassiva (enum)               |
|--------------------|----------------------------------|----------------------------------------|
| Início do turno    | AoIniciarTurno                   | EventoCombate.InicioDoTurno            |
| Ao ser atacado     | AoSerAtacado                     | EventoCombate.DepoisDeSerAtacado       |
| Ao receber dano    | AoReceberDano                    | EventoCombate.DepoisDeReceberDano      |
| Ao causar dano     | AoCausarDano                     | EventoCombate.DepoisDeAtacar           |
| Ao matar           | (não tem)                        | EventoCombate.DepoisDeMatar            |
| Antes de receber   | ModificarDanoRecebido            | EventoCombate.AntesDeReceberDano (*)   |

(*) AntesDeReceberDano existe no enum mas não há dispatch claro hoje.

Diferença crítica:
- **Passivas são ORQUESTRADAS** pelo CombateService: ele chama, coleta
  List<ResultadoAtaque>, exibe mensagem via MenuService, controla ordem.
- **Status são EXECUTADOS NO ESCURO**: os hooks rodam dentro do ReceberDano /
  Atacar, sem acesso ao MenuService, sem orquestração. Por isso o contra-ataque
  (status) não tem mensagem e o Operário (passiva) tem.

### 2.3 Bug latente identificado

No final de ReceberDano:

```
HPAtual -= danoFinal;
if (atacante != null)
    foreach (status in StatusAtivos)
        status.AoSerAtacado(this, atacante, danoFinal);   // sem guard de dano
        if (danoFinal > 0) status.AoReceberDano(...);
```

O contra-ataque sobrescreve AoSerAtacado e chama portador.Atacar(atacante)
DE DENTRO deste loop — recursão dentro do ReceberDano. O _emCooldown é o
band-aid que impede explodir. Frágil.

### 2.4 Anatomia comum (duplicação)

- Habilidade: Nome, Simbolo, Turnos, Descricao.
- StatusEffect: Nome, Simbolo, Turnos, Descricao, Valor, TurnosRestantes.

Os campos de identidade (Nome/Simbolo/Descricao) são duplicados entre as duas
hierarquias.

### 2.5 Buff e Debuff são classes vazias

Ambas só repassam o construtor para StatusEffect. A distinção é puramente
nominal/semântica (e uso em OfType<Buff>()). Todo o comportamento mora no
StatusEffect.

---

## 3. Hipótese validada

**"O efeito não precisa saber sobre o dano."** (proposta do Gabriel)

- **Habilidades ativas JÁ seguem isso:** AplicarDano delega para
  AtacarComMultiplicador → ReceberDano. A habilidade não calcula dano.
- **Status NÃO seguem:** contra-ataque chama Atacar direto; Veneno/Queima
  chamam ReceberDanoDireto. Executam dano dentro do hook.

A inconsistência: habilidades PEDEM dano ao orquestrador; status CAUSAM dano
sozinhos.

---

## 4. Decisão proposta — três separações

A "unificação" NÃO é fundir buff com passiva (eles são genuinamente
diferentes: buff é removível/bloqueável/temporário, passiva é inata). É
separar três responsabilidades hoje misturadas.

### Separação 1 — Identidade comum (cosmético, baixo risco)

Extrair Nome/Simbolo/Descricao para uma base comum herdada por Habilidade e
StatusEffect. Remove duplicação de campos de identidade.

Risco: baixo. Ganho: pequeno (DRY). Prioridade: última.

### Separação 2 — Reação a eventos unificada (ESSENCIAL)

Um único sistema de reação orquestrado pelo CombateService. Tanto status
quanto passivas se registram para reagir a eventos (DepoisDeSerAtacado, etc).
O CombateService dispara, coleta List<ResultadoAtaque>, exibe.

Os hooks AoSerAtacado/AoReceberDano do StatusEffect SAEM de dentro do
ReceberDano e viram reações orquestradas, igual às passivas.

Mecanismo provável: uma interface comum (ex: IReageAEvento) que define
"dado um evento e um contexto, quais reações (revides/efeitos) eu produzo".
Status e passivas implementam. O CombateService varre AMBAS as fontes
(StatusAtivos + Habilidades) no ponto de dispatch.

IMPORTANTE: buff continua sendo buff (em StatusAtivos, removível, com duração)
e passiva continua passiva (em Habilidades, inata). Só COMPARTILHAM o
mecanismo de reação. O QUE cada um é permanece distinto; o COMO reage é
unificado.

Resolve de uma vez:
- Contra-ataque revida todos (reação por evento, sem _emCooldown)
- Mensagem de contra-ataque, reflexo, veneno, queima, maldição, cura
- Operário para de furar a camada de UI
- Bug latente da recursão no ReceberDano
- Fundação do PR 8 (feedback de status)

Risco: médio-alto (mexe no coração do combate). Ganho: altíssimo. É o item
que destrava metade do backlog.

### Separação 3 — Dano centralizado (alto valor)

Um único método de causar dano, parametrizado, que todos usam: habilidades,
contra-ataque, veneno (com flag "ignora defesa/hooks"). Atacar,
AtacarComMultiplicador e ReceberDanoDireto convergem para um caminho só.

O efeito DECLARA a intenção de dano (quanto, multiplicador, o que ignora); o
orquestrador EXECUTA e produz o ResultadoAtaque (que carrega o que aconteceu,
para a UI reportar).

Resolve a hipótese do Gabriel por completo: nenhum efeito calcula dano.

Risco: médio. Ganho: alto (consistência + reporte uniforme). Habilita as
mensagens de dano de status.

---

## 5. Controle de loop (DECIDIDO — Opção A)

Quando o contra-ataque vira reação orquestrada e revida via o caminho de dano
central, o revide é um ataque que pode disparar a reação do alvo —
potencial loop A revida B revida A...

**DECISÃO: Opção A — revide não dispara reação no alvo.** Contra-ataque não
gera contra-ataque. Quebra o loop na raiz. O caminho de dano precisa distinguir
"ataque que provoca reações" de "revide que não provoca". Ver §6.1 (a flag de
dano resolve isso de forma unificada).

---

## 6. Comportamento do contra-ataque (DECIDIDO)

**Regra:** o contra-ataque dispara quando o portador é alvo de um ATAQUE,
mesmo que o dano seja totalmente absorvido (Escudo, Bloqueio Total cobriram
tudo). NÃO importa se perdeu HP — importa se foi um ataque.

**Exceção:** habilidades TipoAtaque.NaoAtaque NÃO provocam contra-ataque.
Exemplo: Inferno (Diabo) aplica Queima e explode via ReceberDanoDireto,
bypassa o fluxo de ataque — ninguém revida. Debuffs puros (sem ataque)
também não provocam. Temáticamente: revida-se um golpe, não um efeito
aplicado à distância.

Portanto a regra real é "reage a ATAQUES (com ou sem dano no HP), não a
efeitos não-ofensivos" — mais precisa que a dicotomia AoSerAtacado vs
AoReceberDano dos hooks antigos.

### 6.1 Consequência: o dano central carrega uma natureza

Para suportar §5 (revide não gera revide) e §6 (NaoAtaque não provoca), o
caminho de dano central precisa distinguir tipos de dano. Proposta de
natureza do dano:

- **Ataque** — Atacar / AtacarComMultiplicador via habilidade ofensiva.
  PROVOCA reações de "ser atacado" (contra-ataque, reflexo).
- **Revide** — contra-ataque/reflexo. NÃO provoca novas reações (quebra loop).
- **Direto** — Veneno, Queima, Inferno (ReceberDanoDireto). Ignora defesa e
  NÃO provoca reações de ataque.

Essa natureza vira um parâmetro/enum do método de dano central (Separação 3).
As reações (Separação 2) consultam a natureza para decidir se disparam.

---

## 7. Plano de fatiamento (PRs incrementais)

Cada PR compila e é testável isoladamente. Ordem proposta:

1. **PR-A — Unificar Atacar/AtacarComMultiplicador.** Atacar passa a chamar
   AtacarComMultiplicador(alvo, 1.0). Pequeno, isolado, zero risco. Aquece.

2. **PR-B — Centralizar dano (Separação 3).** Um caminho de dano parametrizado.
   ReceberDanoDireto vira caso especial (ignora defesa/hooks via flag). Médio.

3. **PR-C — Reação unificada (Separação 2).** O grande. Interface IReageAEvento,
   dispatch único no CombateService, hooks de status saem do ReceberDano.
   Aqui nascem contra-ataque-correto e as mensagens. Inclui decisões §5 e §6.

4. **PR-D — Identidade comum (Separação 1).** Cosmético. Por último.

5. **PR-E — Revisitar HP do Diabo.** Com dano centralizado, reavaliar se o
   stack de HP máximo do Diabo se beneficia da nova estrutura. (Antes decidido
   deixar como está; reabrir só se o novo desenho tornar trivial.)

Itens do backlog que CAEM junto: contra-ataque (revida todos + mensagem),
PR 8 (feedback de status), Operário (para de furar camada), item 13
(StackableDebuff encaixa no dano centralizado).

---

## 8. O que explicitamente NÃO muda

- Buff continua removível/bloqueável/temporário; passiva continua inata.
- A distinção Buff vs Debuff (mesmo sendo classes quase vazias) permanece —
  serve para semântica e para OfType<> em regras (ex: "remover buffs").
- O modelo de Stats em Camadas (ATK/DEF/Crit) fica intacto.
- HPAtual continua mutável (vida gasta), conforme decidido no refator anterior.
