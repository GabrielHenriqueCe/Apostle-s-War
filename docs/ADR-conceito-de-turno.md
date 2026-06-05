# ADR — Conceito de Turno (TurnoDoPersonagem)

> **Tipo:** Architecture Decision Record
> **Status:** Aceito (conceitos fechados; implementação pendente)
> **Contexto:** Apostle's War. O turno de um personagem está diluído em vários
>   métodos do CombateService, sem dono. Este ADR define o conceito de Turno,
>   extrai-o do CombateService e estabelece a fronteira com o que NÃO é turno.
> **Data:** junho/2026

---

## 1. Problema

O "turno de um personagem" não tem dono no código. Está diluído em 5 pontos do
CombateService:
- `ExecutarTurnoCompleto` — orquestra início, ação, fim
- `ExecutarInicioDeTurno` — tick de status (AoIniciarTurno) + evento InicioDoTurno
- check de `Preso` (pula a ação mas avança status/cooldown)
- `AvancarStatus` — status.PassarTurno() + remove expirados
- `AvancarCooldowns` — cd.PassarTurno() em cada cooldown

E o ESTADO do turno vive espalhado: duração nos StatusEffect, cooldown no
SkillCooldown (dentro de Combate.Cooldowns), e o futuro "1x por agressor por
turno" não tem onde morar.

Sintoma sentido: não há uma entidade que saiba "de quem é o turno e o que
acontece quando ele começa/termina". O CombateService é o God Object que
acumula isso junto com tudo mais (combate, rodada, seleção de alvo, exibição).

---

## 2. Conceito: as 3 fases de um turno

Um turno de um personagem tem três fases temporais:

1. **Início** — o que dispara "no começo da minha vez": tick de Veneno/Queima,
   cura contínua, PassivaGenio reaplica reflexo, (futuro) reset do "1x por
   agressor por turno".
2. **Ação** — o personagem faz UMA coisa: escolhe e usa uma habilidade (ou é
   forçado por Irritar, ou pula por Preso/Medo).
3. **Fim** — o que dispara "no fim da minha vez": avança duração dos status
   (perde 1 turno, remove expirados), avança cooldowns.

---

## 3. A distinção central: RELÓGIO vs VONTADE

Início e Fim têm natureza DIFERENTE da Ação:

- **Início/Fim = RELÓGIO.** Manutenção mecânica do estado temporal: "passou
  tempo, atualize os contadores". Veneno tica, duração cai, cooldown cai.
  Automático, sem decisão, IGUAL para jogador e bot. Pura consequência do tempo.

- **Ação = VONTADE.** Decisão + execução: quem escolhe (jogador via menu? bot?),
  qual alvo, qual habilidade, como exibe. Cheio de decisão e de UI. DIFERENTE
  para jogador (menu, ReadKey, seleção de alvo) e inimigo (bot).

A fronteira entre classes segue a fronteira das DEPENDÊNCIAS:
- Início/Fim dependem só de status + cooldown do combatente. Mínimas.
- Ação depende de UI/input/bot/seleção de alvo. Muitas.
Dependências diferentes → responsabilidades diferentes → classes diferentes.

---

## 4. Decisão: o Turno é dono do RELÓGIO, não da VONTADE (Opção 1.5)

O **TurnoDoPersonagem** é dono do relógio de UM combatente. Expõe duas
operações que encapsulam Início e Fim:

- `Iniciar()` — tick dos status (AoIniciarTurno), evento InicioDoTurno das
  passivas, (futuro) reset do "1x por agressor por turno".
- `Finalizar()` — avança duração dos status (PassarTurno + remove expirados),
  avança cooldowns (SkillCooldown.PassarTurno).

O **CombateService** chama essas operações EM VOLTA da Ação:

```
// loop do CombateService, por combatente:
turno.Iniciar();
if (combatente vivo && !Preso)
    ExecutarAcao(...);   // a VONTADE: fica no service (menu/bot/UI/alvo)
turno.Finalizar();
```

### Por que NÃO o Turno chamar a Ação (Opção 2 rejeitada)
Se o Turno chamasse a Ação, precisaria conhecer MenuService, input (ReadKey),
lógica de bot e seleção de alvo. Ele deixaria de ser "o relógio da minha vez" e
viraria um mini-CombateService — recriando o God Object que este ADR desfaz.
Mover o acoplamento não é resolvê-lo. O Turno é sobre o TEMPO, não sobre a
VONTADE: o relógio não decide o que você faz na sua vez, só marca início e fim.

---

## 5. O que MIGRA para o Turno vs o que FICA

### Migra (do CombateService / da invocação):
- A INVOCAÇÃO de `AoIniciarTurno` e `PassarTurno` dos status (hoje em
  ExecutarInicioDeTurno / AvancarStatus).
- O `AvancarStatus` (avança duração + remove expirados).
- O `AvancarCooldowns` (percorre Cooldowns chamando PassarTurno).
- O disparo do evento InicioDoTurno das passivas.
- (Futuro C5b) o reset do "1x por agressor por turno".

### Fica no StatusEffect (é estado/mecanismo do próprio status, NÃO do turno):
- `Turnos`, `TurnosRestantes` — a duração é DADO do status. Buffs decidem "maior
  duração prevalece"; debuffs stack (Veneno/Queima/Maldicao) sincronizam com
  Stacks. O Turno manda "passe seu turno"; o status decide o que isso significa
  pra ele.
- `AoPassarTurno` — hook interno do status pra reagir ao avanço (ex: ContraAtaque
  reseta seu HashSet). O Turno INVOCA PassarTurno; o status reage internamente.

### Fica no SkillCooldown (classe boa, intacta):
- `turnosRestantes`, `Usar`, `PassarTurno`, `Resetar`, `Disponivel`. O Turno
  ORQUESTRA (chama PassarTurno em cada cooldown no Finalizar), mas o cooldown
  continua dono do próprio estado. Foi erro ter deixado só a orquestração
  (AvancarCooldowns) solta no service — agora ela mora no Turno.

Resumo: o Turno é o MAESTRO do fluxo temporal (manda "passou o turno, atualizem-se"),
não o ARMAZÉM do estado. Cada peça (status, cooldown) continua dona do seu estado.

---

## 6. O check de Preso, Irritar e Medo (a Ação)

O check de `Preso` (pular a ação) é decisão sobre a VONTADE — fica no fluxo da
Ação no CombateService, não no Turno. Mesmo para `Irritar` (força A1) e `Medo`
(chance de paralisar). Razão: todos decidem SE/COMO a ação acontece, e a ação é
domínio do service (tem as dependências de UI/alvo). O Turno só garante que o
Início rodou antes e o Fim roda depois, INDEPENDENTE de a ação ter ocorrido —
por isso Preso ainda avança status/cooldown (Finalizar roda mesmo sem ação).

---

## 7. Fronteira com o RelógioDoCombate (conceito vizinho, FUTURO)

O TurnoDoPersonagem é o relógio de UM combatente (roda várias vezes por round).
Há um conceito VIZINHO, num nível ACIMA, que NÃO é este ADR:

**RelógioDoCombate** — o tempo GLOBAL do combate ("estamos no turno N do
combate"). Dispara eventos em marcos: limite de turnos da fase (anti-stall),
boss que no turno X mata todos (enrage timer). Mora no nível do ExecutarCombate/
Rodada, não dentro do TurnoDoPersonagem — são relógios diferentes (um por
personagem, um do combate), níveis diferentes (SRP).

**Status:** YAGNI. Não implementar agora (nenhuma fase pede ainda). Este ADR só
RESERVA o lugar: o TurnoDoPersonagem é desenhado de forma que o RelógioDoCombate
caiba acima dele depois, sem conflito. Implementa quando uma fase concreta pedir
(players otimizam até quebrar o jogo — o enrage será a rédea).

---

## 8. Escopo da implementação (quando executar)

Este é um refactor de realocação — mover o turno do CombateService para uma
entidade própria, SEM mudar a lógica (ela já funciona). Strangler-friendly:
pode-se criar o TurnoDoPersonagem e migrar o ExecutarTurnoCompleto para usá-lo
em uma fatia, validando que o combate continua idêntico.

Decisões de implementação a tomar na hora (não são dúvidas de conceito):
- O TurnoDoPersonagem é uma classe nova (onde mora: Combat/?) que recebe o
  combatente + seus aliados/inimigos (para os contextos de início).
- Como o CombateService passa a instanciar/usar o Turno por combatente por volta.
- Confirmar que o comportamento é idêntico (mesma ordem: início → ação → fim;
  Preso ainda avança; turno extra continua funcionando).

Validação: jogar uma fase completa e confirmar que ticks, durações, cooldowns,
Preso, turno extra e reações se comportam exatamente como antes do refactor.
