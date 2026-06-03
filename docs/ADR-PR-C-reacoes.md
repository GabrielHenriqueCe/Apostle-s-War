# ADR — PR-C: Reações Unificadas e Orquestradas

> **Tipo:** Architecture Decision Record
> **Status:** Proposta (aguardando aprovação do desenho)
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

### 3.3 Como o status declara que reage — interface comum

Proposta: uma interface que os status reativos implementam.

```
interface IReageAoCombate
{
    // Chamado pelo CombateService no momento certo. Retorna os danos/curas
    // produzidos (pra orquestrador exibir). Pode ser vazio.
    List<ResultadoAtaque> Reagir(EventoCombate evento, ContextoReacao ctx);
}
```

(Nome e assinatura a refinar na implementação. O ContextoReacao carrega
portador, atacante, dano causado e a NaturezaDano que originou — essencial
pro RefletirDano/ContraAtaque consultarem TipoReacao.)

Status que NÃO reagem (Escudo, Bloqueio, BuffAtaque, etc) não implementam a
interface — o CombateService os ignora no dispatch de reação. Eles seguem
agindo via ModificarDanoRecebido (dentro do ReceberDano) ou stat calculado,
intocados.

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

- É o coração do combate. Mexe em ReceberDano, Atacar, CombateService e 5
  status. Alto risco de regressão.
- A ordem das reações importa (ex: contra-ataque antes ou depois de espinhos?).
  Precisa ser definida e documentada.
- AoSerAtacado vs AoReceberDano (dano 0 vs dano > 0) precisa virar uma
  distinção explícita no novo modelo (ex: dois eventos, ou um campo no
  contexto indicando se houve dano).

---

## 6. Plano de sub-fatiamento (cada sub-PR compila)

O PR-C é grande demais pra um commit. Sub-fatias propostas:

- **C1 — Sedento** (lado atacante, isolado, 1 efeito). Move AoCausarDano pro
  orquestrador. Menor superfície, bom primeiro passo.
- **C2 — RefletirDano + Sangramento** (lado alvo, AoReceberDano, dano > 0).
  Os dois são gêmeos (reagem a dano recebido, agem no atacante).
- **C3 — EspinhosVenenosos** (lado alvo, AoSerAtacado, mesmo com dano 0).
- **C4 — ContraAtaque** (o mais complexo: revida todos, consome TipoReacao,
  remove _emCooldown, Revide). Por último, com a fundação pronta.
- **C5 — Operario** (para de furar a camada). Pequeno, fecha.

Cada sub-PR: build verde + teste antes do push.

ALTERNATIVA: se a interface comum exigir mexer no ReceberDano de uma vez (os
hooks saem todos juntos), pode não dar pra fatiar tão limpo. Avaliar no C1: se
mover um efeito exigir mover todos, replanejar.

---

## 7. Decisões pendentes (resolver antes/durante a implementação)

1. Nome e assinatura exata da interface de reação.
2. **RESOLVIDO** — "Houve dano?" NÃO vira um campo. Usa os DOIS eventos que
   já existem (iguais aos das passivas): DepoisDeSerAtacado (dispara sempre,
   mesmo dano 0 — ContraAtaque, EspinhosVenenosos) e DepoisDeReceberDano (só
   com dano > 0 — RefletirDano, Sangramento). Cada buff reativo declara a qual
   evento reage, igual as passivas via DeveAtivar. O evento JÁ É a distinção;
   o modelo de buff reaproveita exatamente o das passivas.
3. **RESOLVIDO** — Ordem das reações do lado do alvo:
   (1) RefletirDano, (2) EspinhosVenenosos, (3) ContraAtaque (SEMPRE por último).
   Efeitos passivos/defensivos antes; o revide ativo por último.
4. Onde o ContextoReacao carrega a NaturezaDano do golpe (pra consumir
   TipoReacao).
5. Confirmar que mover os hooks não quebra o ProtecaoAliado/Invencivel/etc
   (Grupo A, ModificarDanoRecebido — devem ficar no ReceberDano, intocados).
