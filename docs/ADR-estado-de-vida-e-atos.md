# ADR — Estado de Vida (Vivo/Morto) e Atos do Turno

> **Tipo:** Architecture Decision Record
> **Status:** Aceito — **Passos 1–5 IMPLEMENTADOS** (State Pattern Vivo/Morto, status no Morto,
>   Atos, Guarda limpa, seleção por estado). Doc de CONCEITO durável + gameplay futuro
>   (decomposição / Alma como 3º estado). Absorveu o antigo `ADR-selecao-por-estado` (ver §11).
> **Contexto:** Apostle's War. A morte é hoje implícita (`HPAtual <= 0`), sem um
>   momento de transição onde efeitos possam reagir/prevenir. O ExecutarHabilidade
>   resolve tudo num bloco plano, sem etapas nomeadas. Este ADR define (a) o estado
>   de vida como objeto (State Pattern), e (b) os Atos da resolução de uma habilidade.
>   Os dois são o MESMO fio: o Ato de Morte é a transição de estado.
> **Data:** junho/2026

---

## 1. Problema

Dois sintomas, um fio só.

**(a) Morte implícita.** Hoje `EstaVivo() => HPAtual > 0`. Não há um MOMENTO em que
a morte "acontece" — o HP simplesmente fica <= 0. Consequências:
- A **Guarda** (prevenir morte) não tem onde intervir — não há transição pré-morte.
- O **bloquear-revive** é uma flag (`PodeReviver`) no Combate, que persiste mesmo
  depois de reviver, e não é visível/removível como um status real.
- O **revive** e a **cura** não têm um conceito de "alvo morto" vs "alvo vivo" —
  ambos filtram `EstaVivo()` na seleção, sem distinção rica.
- Não há lugar pra status DE MORTO (mecânicas futuras: decomposição, etc).

**(b) Fluxo sem etapas.** O `ExecutarHabilidade` resolve a habilidade num foreach
plano: exibir, ao-matar, ao-morrer, reação-do-alvo, reação-do-atacante — tudo no
mesmo nível, sem estrutura que diga "esta é a etapa de reação, esta é a de morte".

A separação cálculo vs fluxo ENTRE classes já está correta (ver §6) — o problema
é fluxo sem estrutura DENTRO do CombateService, e a morte sem momento próprio.

---

## 2. Vocabulário: Ato (não "Fase")

Os passos sequenciais da resolução de uma habilidade chamam-se **Atos**. "Fase" já
é termo de campanha (Capítulo 1, Fase 1 — `ExecutarFase`/`ObterFase`); reusar criaria
ambiguidade. "Ato" casa com a encenação do combate (símbolos, mensagens, animações
futuras): cada Ato é um beat da cena.

---

## 3. Decisão A: Estado de vida como objeto (State Pattern)

O `Combate` ganha um objeto interno `EstadoVida`, que é `Vivo` ou `Morto`. O Combate
continua o MESMO objeto (todas as referências a ele permanecem válidas); o que muda é
qual estado ele segura. Interações que dependem de vivo/morto são DELEGADAS ao estado,
que decide por polimorfismo — sem `if (estaVivo)` espalhado.

```
Combate  (mesma identidade sempre)
 ├── StatusAtivos      (status DO VIVO — como hoje)
 ├── Cooldowns, HP, stats...  (como hoje)
 └── EstadoVida _estado       ← NOVO (começa Vivo)
       ├── Vivo
       └── Morto  (carrega lista de status DE MORTO)
```

É composição — o mesmo que o Combate já faz com `StatusAtivos` e `Cooldowns` (objetos
vivendo dentro dele). A novidade é que esse objeto é polimórfico e trocável.

**A regra das listas de status (resolve o histórico do MortePermanente):**
- Status de VIVO vivem na lista do Combate (onde já vivem).
- Status de MORTO vivem DENTRO do objeto Morto (lista separada).
- A transição troca o objeto de estado; os status do estado abandonado somem junto.

Isso ISOLA os status de morto dos cleanses/bloqueios do vivo. O bloquear-revive
(impedir-ressurreição) volta a ser um DEBUFF de verdade (símbolo, visível, removível)
— mas morando no Morto, fora do alcance da ImunidadeDebuffs do vivo.

> **Histórico (não repetir o erro):** o impedir-ressurreição já foi um Debuff
> (`MortePermanente`), foi PROMOVIDO a flag `PodeReviver` porque (1) não tinha
> removedor — era um bool que nunca saía, e (2) era acidentalmente bloqueado pela
> ImunidadeDebuffs (CantoDeSereia). Volta a ser Debuff AGORA porque os dois motivos
> caíram: (1) ganha removedor — o Diabo dá cleanse específico; (2) vive no Morto, em
> lista separada, fora do alcance da ImunidadeDebuffs do vivo. Quem ler isto no futuro:
> não é regressão, é a volta possibilitada pela mudança de contexto.

---

## 4. Roteamento por estado (o que cada estado responde)

O Combate delega; o estado decide. Exemplos do comportamento polimórfico:

| Interação        | Vivo responde            | Morto responde                         |
|------------------|--------------------------|----------------------------------------|
| Reviver          | ignora (já vivo)         | revive SE não tem impedir-ressurreição |
| Curar            | cura                     | ignora (morto não cura)                |
| Receber dano     | aplica/calcula           | (hipotético: corpo pode tomar dano)    |
| Bloquear-revive  | n/a                      | aplica o debuff impedir-ressurreição   |
| Cleanse do Diabo | n/a                      | remove o impedir-ressurreição          |

A seleção de alvo passa a distinguir: reviver SELECIONA mortos; curar SELECIONA vivos.
Os dois aparecem na tela (o jogador vê o vivo em pé e o morto caído); a habilidade só
ativa no estado certo. No console, resolve-se de forma simples (lista filtrada por
estado); o clique-no-alvo-certo fica pra plataforma.

---

## 5. Decisão B: os Atos da resolução de uma habilidade

O `ExecutarHabilidade` deixa de ser um foreach plano e passa a ser uma sequência de
Atos nomeados, cada um com responsabilidade clara:

1. **AtoExecucao** — a habilidade causa os efeitos (resolve alvos conforme TipoAtaque,
   chama Atacar em cada um). Produz os EventoDano.
2. **AtoReacaoDoAlvo** — o alvo reage ao que recebeu (contra-ataque, espinhos, reflexo).
3. **AtoMorte** — a transição vivo→morto de quem chegou a 0; ao-matar (Vilão bloqueia
   revive aplicando o debuff no Morto), ao-morrer (Necromancia revive), prevenção
   (Guarda intervém ANTES da transição). É AQUI que o modelo de estado encaixa.
4. **AtoReacaoDoAtacante** — o atacante reage (Sedento cura, Vírus, OlhoClinico).
5. **AtoEncerramento** — cooldown da habilidade, mensagem de uso.

O `ExecutarHabilidade` vira legível: "executo, alvo reage, resolvo morte, atacante
reage, encerro". Cada Ato é um método com nome próprio.

**O AtoMorte é o ponto de encontro dos dois fios:** ele É a transição de estado
(Decisão A) e É uma etapa do fluxo (Decisão B). Por isso estado-de-vida e atos-do-turno
são um único ADR.

---

## 6. O que NÃO muda: cálculo vs fluxo entre classes (já está certo)

Diagnóstico explícito pra evitar refactor desnecessário:

- **Combate = domínio.** Sabe CALCULAR dano (defesa, modificadores, escudo, crítico)
  e computar stats. O cálculo de dano morar no Combate está CERTO — o objeto que
  recebe o dano calcula o quanto, sobre os próprios stats/status. NÃO criar uma
  "classe de cálculos" separada (YAGNI — o Combate já é esse lar).
- **CombateService = orquestrador.** Sabe a ORDEM (quem age, quando reage, o loop do
  turno). Quase não tem cálculo. Os ProcessarReacoes* orquestram, não calculam.

A separação cálculo/fluxo ENTRE as classes já existe e é mantida. O refactor é
estrutural DENTRO do CombateService (dar Atos ao fluxo) + o modelo de estado fornecendo
o AtoMorte. Não se move cálculo; não se cria classe de cálculo.

---

## 7. Onde a transição é disparada (a tensão resolvida)

A morte (transição vivo→morto) NÃO deve ser resolvida dentro do `ReceberDano`
(Combate) — isso misturaria o cálculo de dano com a lógica de passivas/reações (o
"exagero" identificado). O `ReceberDano` CALCULA e aplica o HP; a RESOLUÇÃO da morte
(quem reage, quem previne, quem revive) acontece no **AtoMorte**, no orquestrador.

Decisão em aberto pra implementação (não é dúvida de conceito): o ponto EXATO em que
o Combate sinaliza "cheguei a 0" ao orquestrador — se o ReceberDano retorna o fato e
o AtoMorte checa, ou se há um gancho leve. O importante: o cálculo fica no Combate, a
resolução-de-morte fica no Ato. O Combate pode marcar o estado interno (Vivo→Morto)
como dado; as CONSEQUÊNCIAS (reações) rodam no Ato.

---

## 8. Validação contra os casos concretos

- **Vilão bloqueia revive:** ataca um Vivo (AtoExecucao) → alvo chega a 0 → transição
  Vivo→Morto (AtoMorte) → passiva do Vilão (IReageAoMatar, já existe) recebe o Morto e
  aplica o debuff impedir-ressurreição NELE. O debuff vive no Morto.
- **Diabo reabilita:** habilidade que dá cleanse no impedir-ressurreição (do Morto) →
  o Morto volta a poder ser revivido.
- **Revive (Necromancia):** seleciona/atua sobre um Morto → checa se tem
  impedir-ressurreição → se não, transição Morto→Vivo (descarta o Morto e seus status).
- **Guarda (prevenção):** intervém no AtoMorte ANTES da transição — aplica Invencível
  e segura o personagem como Vivo (a transição não acontece). É prevenção, não revive.
- **Cura barrada no morto:** Curar delega ao estado; o Morto ignora. Sem `if` externo.

Os cinco passam sem caso especial fora do modelo. O modelo se sustenta.

---

## 9. Fora de escopo (gameplay futuro que o modelo HABILITA)

O modelo comporta status de morto que tickam/acumulam/interagem. Registrado como
IDEIAS DE GAMEPLAY (fase de design/lore), NÃO implementadas no refactor:
- **Decomposição:** debuff no morto que, após X turnos sem reviver, aplica o
  impedir-ressurreição.
- **Gás tóxico acumulativo:** stack-builder no morto; ao atingir o limite, o corpo
  "explode" causando dano nos VIVOS inimigos e aplicando morte permanente.

O refactor entrega APENAS o impedir-ressurreição como status de morto (tem cliente:
Vilão aplica, Diabo remove). As demais validam que o desenho não fecha portas, mas
só viram código quando o balanceamento/lore pedir.

---

## 10. Escopo da implementação (quando executar)

Refactor estrutural do núcleo. Sequência sugerida (cada passo buildável, Strangler-friendly):

1. **Modelo de estado** — `EstadoVida` (Vivo/Morto), composição no Combate, delegação
   de EstaVivo/Reviver/Curar. Comportamento idêntico ao atual (morte ainda derivada do
   HP, só que agora roteada pelo estado).
2. **Status no Morto** — lista própria do Morto; migrar o bloquear-revive de flag
   `PodeReviver` pra debuff impedir-ressurreição no Morto. Vilão aplica; criar o
   cleanse do Diabo.
3. **Atos do turno** — reorganizar ExecutarHabilidade nos 5 Atos. O AtoMorte usa a
   transição do passo 1.
4. **Guarda limpa** — sai do hack [sistema-morte-como-estado]; vira prevenção real no
   AtoMorte (intervém antes da transição). Fecha o débito conceitual da Guarda.
   **(Refinado jul/2026 — fix de bug):** a prevenção migrou de reação (`IReageAntesDeMorrer`, que
   usava `AplicarRevive` e PERDIA os status) para a CAPACIDADE `IPrevineMorte`, consultada pelo
   `Combate.ConfirmarMorte()` DENTRO do `ReceberDano` (funil único de dano). O portador evita a morte
   sem nunca virar Morto → **status preservados**. `IReageAntesDeMorrer` foi removida (código morto).
5. **Seleção de alvo por estado** — reviver seleciona morto, curar seleciona vivo.

Decisões de implementação a tomar na hora (não são dúvidas de conceito):
- O que exatamente o EstadoVida controla (fronteira dos métodos delegados).
- O ponto exato de sinalização da transição (ver §7).
- Onde mora a classe EstadoVida/Vivo/Morto (Combat/?).

Validação: jogar uma fase completa e confirmar que morte, revive (Necromancia),
bloquear-revive (Vilão), cura, Guarda e seleção de alvo se comportam corretamente.

---

## 11. Passo 5 — Seleção de alvo por estado + passiva-pura (FEITO; absorve o `ADR-selecao-por-estado`)

> Este ADR e o antigo `ADR-selecao-por-estado.md` eram o MESMO fio — o Passo 5 acima. O
> selecao-por-estado foi arquivado em `historico/`; a essência durável fica aqui.

**Decisões (feitas, PR #111):**
- **`EstadoAlvo { Vivos, Mortos, Ambos }` declarado pela habilidade** — "make illegal states
  unrepresentable" (uma cura não consegue nem mirar um morto). `ResolverAlvos` passou a filtrar
  pelo estado (some o hardcode `.Where(EstaVivo())`); `SalvandoDia`/`Celestial` largaram o loop
  manual duplicado.
- **`SelecaoDeAlvoService` ganhou overload pra Aliados** (sem Provocar/Bloqueio/Intocável, que só
  fazem sentido mirando inimigos); o `CombateService` liga o pick real de alvo aliado quando
  `NumeroDeAlvos` é finito e `EstadoAlvo != Ambos`.
- **Passiva-pura (mesmo PR):** Abóbora/Dragão/Herói/Morcego/Anjo/Sereia largaram o buff-de-contorno
  (`IPassivaInicial`) e viraram a capacidade direta (interface); Fantasma virou `Removivel=false`.
- **Barata NÃO é `Ambos`:** mira Vivos (ataque) + consequência no morto resultante (Sentença) —
  é uma seleção + efeito colateral, não duas seleções.

**EVOLUÍDO PELO MOTOR:** o `EstadoAlvo` deixou de ser da habilidade e **desceu pra AÇÃO**, avaliado
na execução (`ADR-composicao-de-acoes.md §5.3`) — e o `Ambos` está morrendo (uma habilidade que
mira 2 estados vira 2 ações de estados diferentes). A forma ATUAL mora no composicao-de-acoes;
esta seção é o registro de origem.
