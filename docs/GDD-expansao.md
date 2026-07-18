# Apostle's War — GDD da Expansão (Pós-Web)

> **Status:** Ideias registradas, NÃO implementar no console.
> Destino: expansão pós-lançamento da versão web.
> Última atualização: julho/2026.

---

## Propósito deste documento

Registrar as ideias de meta-progressão e sistemas de RPG de coleção pensadas
para uma futura expansão de Apostle's War. Nada aqui deve ser construído no
console atual — o console é o MVP jogável (montar time → jogar fase), sem
progressão persistente.

A ordem de desenvolvimento planejada é:

1. **Console jogável** — completar habilidades de todas as facções (MVP jogável)
2. **Modo auto-battle** — automação de combate no console
3. **Web port** — mesmo jogo, React + .NET API (peça principal de portfólio)
4. **Expansão (pós-web)** — tudo que está neste documento

A razão de tudo isto ser "pós-web" e não "console": cada sistema aqui exige
persistência séria (banco de dados, inventário, tracking de XP, instâncias
únicas de item) e uma interface visual rica (grids coloridos de raridade,
telas de evolução, comparação de sub-stats). O console não comporta isso sem
virar um segundo jogo.

---

## 1. Sistema de Itens (coleção)

### Instâncias de item

Itens deixam de ser stats fixos aplicados ao personagem e passam a ser
**objetos equipáveis com identidade própria**. Várias cópias do mesmo item
podem existir (ex: duas "Coroas"), cada uma equipável em um personagem
diferente. Equipar uma cópia em um personagem ocupa aquela cópia.

### Raridades

Cada item nasce com uma raridade que determina quantas sub-estatísticas ele
pode ter:

| Raridade  | Cor            | Sub-stats |
|-----------|----------------|-----------|
| Comum     | Cinza          | 0         |
| Incomum   | Verde          | 1         |
| Raro      | Azul           | 2         |
| Épico     | Roxo           | 3         |
| Lendário  | Dourado/Laranja| 4         |
| Mítico    | Vermelho       | 5 (provisório) |

### Estrelas e Níveis

- Item começa com 1 estrela, sobe até 5 estrelas.
- Cada estrela amplia o teto de nível: 1 estrela = nv 10, ..., 5 estrelas = nv 50.
  (Curva exata a definir — provisório: teto = estrelas x 10.)
- Cada nível e cada estrela aumentam o **atributo base** do item.
- Estrelas, além de aumentar o base, aumentam o **poder máximo das sub-stats**.

### Progressão de sub-stats (upgrade do item)

- A cada +5 níveis no item, ele:
  - Desbloqueia uma nova sub-stat (se ainda tiver menos do que o teto da raridade), OU
  - Melhora uma sub-stat existente (se já estiver no teto de quantidade).
- Item evolui assim até +20 (4 marcos de +5), totalizando até 4 sub-stats.

### Poder das sub-stats por estrela (exemplo)

O range aleatório de uma sub-stat depende das estrelas do item. Exemplo com
sub-stat de HP:

| Estrelas | Range de HP (random) |
|----------|----------------------|
| 1        | 8–12                 |
| 2        | 10–16                |
| 3+       | (escala a definir)   |

> Nota de design: ranges aleatórios criam variação entre cópias do mesmo item,
> incentivando "farmar" a cópia ideal — núcleo do loop de coleção.

### Conjuntos (sets) — ideia (jul/2026)
Peças de um mesmo conjunto, equipadas juntas, dão um **efeito único** (ex: *Conjunto do Inferno* —
queima quem te ataca). Substatísticas **fixas por conjunto** (sem RNG estilo Raid). Tensão a
resolver no design: isto convive com os ranges aleatórios de sub-stat acima — provavelmente
conjuntos = fixo, itens avulsos = RNG.

### Fusão de itens — ideia (jul/2026)
2 itens iguais de uma raridade = 1 item da raridade seguinte (2 Comuns → 1 Incomum → … → Mítico).
Progressão **linear e previsível, sem aleatoriedade**.

### Global → por-personagem (reforço da direção deste §1)
Hoje (console) equipamento é status GLOBAL somado no jogador; a expansão migra pra **itens
equipados individualmente em cada personagem** (é o que este §1 já assume). Em aberto: nível nos
itens + sistema de +1/+2/+3.

---

## 2. Progressão de Personagem

### Níveis e XP

- Personagens ganham XP **apenas se participarem da batalha** (estiverem no time).
- Personagens ganham níveis com XP acumulado.

### Estrelas de Personagem (ascensão)

- Personagem evolui em estrelas (mecânica de ascensão tipo gacha).
- Exemplo: ao chegar em 1 estrela nível 10, precisa de um **item de evolução**
  para ascender.
- Quanto mais estrelas o personagem já tem, mais itens de evolução são
  necessários para a próxima ascensão.

### Drop de itens de evolução

- A fase final de cada capítulo dropa item(ns) de evolução.
- Capítulos mais avançados dropam mais itens de evolução.

### Raridade de personagem + Regra de ouro (ideia jul/2026)
- Personagens têm **raridade** (mais difícil de conseguir ≠ exclusivo — **todos sempre obtíveis**).
- **Regra de ouro:** personagem fraco NUNCA é fraco pra sempre. Um Comum pode evoluir até Lendário
  com investimento — o primeiro personagem do jogador pode virar o mais forte do jogo se ele treinar.

### Maestrias por herói (ideia futura)

- Cada herói poderia ter uma árvore de maestrias desbloqueáveis.
- Algumas maestrias mexem em mecânicas permanentes (ex: "reduz DEF do inimigo
  até um cap próprio").
- **Importante:** o refator de Stats em Camadas (ver seção 4) já modela
  modificadores permanentes guardados no alvo (ReducaoDefesaPermanente),
  o que permite múltiplas fontes (Sorrateiro + maestrias) compartilharem o
  mesmo cap sem "buff invisível".

---

## 3. Campanha e Dificuldade

- A campanha começa no **Fácil**, com inimigos escalando de forma tranquila.
- Completar uma dificuldade libera a próxima: Fácil -> Normal -> Difícil -> Muito Difícil.
- Cada dificuldade aumenta a escala dos inimigos.

### Decisão em aberto: stats de inimigo por fase

Hoje (console) os inimigos têm stats calculados por multiplicador:
HP = (0.5 x capítulo) + (0.1 x fase).

Pergunta para a expansão: trocar por **tabela fixa de stats por fase/turno**
(controle de balanceamento fino) ou manter cálculo (escala automática)?

> Recomendação atual: manter cálculo até haver telemetria de jogadores reais
> indicando fases mal balanceadas. Tabela fixa é otimização que só compensa
> com dados de uso. YAGNI até lá.

---

## 4. Arquitetura de Stats em Camadas (em implementação no console)

> Esta seção descreve um refator que está sendo feito JÁ no console (não é
> futuro). Está aqui porque é a fundação técnica que torna toda a expansão
> viável.

Stats (Ataque, Defesa, etc.) deixam de ser campos mutáveis e viram
propriedades **calculadas por composição de camadas**. Princípio central:
**nada de "buff invisível"** — todo modificador é uma camada explícita e
rastreável.

### Camadas (ordem de aplicação), exemplo com Ataque

```
AtaqueComItens = AtaqueBase x MultiplicadorFase + ItensFlat + (base x ItensPct)
comStacks      = AtaqueComItens + BonusPermanente          (stack-builders, ex: Ambicao)
Ataque (final) = comStacks + (buff sobre comStacks)        (BuffAtaque temporário)
```

### Regras confirmadas (por simulação)

ATAQUE:
- base 100 + buff 25% = 125
- base 100 + stack máx 25% = 125
- base 100 + stack máx + buff 25% = 156
- base 100 + 1 stack (5%) + buff 25% = 131

DEFESA (buff e debuff incidem sobre comStacks, independentes — um anula o outro):
- base 100 + buff +30% + debuff -30% = 100
- base 100 + stack Rei +25% (comStacks 125) + buff +30% + debuff -30% = 125

### Stack-builders (modificadores permanentes da fase)

- **Ambicao** (Troll): +5% ATK por hit recebido, cap +25%. Calcula sobre
  AtaqueComItens. Mora em BonusAtaquePermanente do próprio combatente.
- **PassivaRei**: +5% DEF por hit recebido, cap +25%. Mora em BonusDefesaPermanente.
- **PassivaDiabo**: +5% HP máximo por hit recebido, cap +25%.
- **PassivaDetetive**: +5% TaxaCrit por ataque, cap +25%.
- **PassivaSorrateiro**: -5% DEF do INIMIGO por ataque, cap -25% TOTAL no alvo.
  Mora em ReducaoDefesaPermanente do alvo. Múltiplos Sorrateiros (e futuras
  maestrias) compartilham o mesmo cap porque o dado mora no alvo, não na passiva.

> Padronização: todos os stack-builders usam 5% por tick e cap de 25%.
> (Sorrateiro era 3%, ajustado para 5% para manter o padrão.)

### Critério de substituição de buffs

Buffs do mesmo tipo NÃO se somam — o mais forte prevalece:
- Maior Valor ganha (BuffAtaque 50% vence BuffAtaque 25%)
- Em empate de Valor, maior duração ganha
- Escudo: maior PontosRestantes prevalece

> Hoje todos os buffs do roster usam o mesmo valor (ex: BuffAtaque sempre 25%),
> então o critério de Valor só será exercitado quando houver buffs de valores
> diferentes (bosses, habilidades novas). O código já está preparado.

### Stack-builder como buff que "reseta no pico" (ideia futura para BOSS)

Cogitou-se um stack-builder que sobe até o máximo e então "reseta" para fraco
(janela de oportunidade). Decidido: os stack-builders atuais (Diabo/Rei/Ambicao)
ficam **permanentes e não-buffs**. A mecânica de "reset no pico" fica reservada
para um BOSS futuro.

### Por que o HP NÃO virou stat calculado por camadas

ATK, DEF, TaxaCrit e DanoCrit viraram propriedades calculadas (getters que
somam camadas). O HP ficou de fora, por decisão consciente — não por
esquecimento.

**Motivo real:** o HP tem o HPAtual, um valor de "vida gasta" com memória
própria (resultado de danos e curas ao longo do combate), que NÃO é
recalculável a partir de camadas. Além disso, o HPAtual está acoplado ao
HPMaximo (nunca pode passar dele). Quando o HPMaximo diminui (Maldição,
Queima), o HPAtual precisa ser cortado NAQUELE momento — uma ação, não um
cálculo. Um getter puro não faz ações; forçar o HPMaximo a ser getter
calculado exigiria um gatilho separado pra ajustar o HPAtual, reintroduzindo
a complexidade que o modelo mutável já resolve corretamente.

O modelo atual (HPMaximo mutável via ModificarHPMaximo / ReduzirHPMaximo /
RestaurarHPMaximo, com HPMaximoReduzidoTotal como acumulador) já trata tudo
certo e não tem o bug de timing do _valorAdicionado.

**Inconsistência consciente — o Diabo:** PassivaDiabo (+5% HP máx por hit,
cap 25%) é, conceitualmente, um stack-builder igual à Ambicao (ATK) e ao
Rei (DEF). Mas, enquanto Ambicao/Rei usam camadas de bônus permanente
(BonusAtaquePermanente etc), o Diabo modifica o HPMaximo diretamente via
ModificarHPMaximo. Essa assimetria é aceita de propósito: padronizar o Diabo
com os outros stack-builders exigiria transformar o HPMaximo em camada,
reabrindo o acoplamento HPAtual↔HPMaximo (e ainda mexendo no save/load).
Risco alto num sistema crítico (dano, cura, revive, status, save), ganho
apenas estético, e zero bug corrigido. Aceita-se a inconsistência por ser
superficial (mora num lugar diferente) mas comportamentalmente correta.

**Quando reavaliar:** se algum dia existir um buff TEMPORÁRIO de HP máximo
(que aumenta o teto por X turnos e depois volta), o modelo mutável traria de
volta o bug de timing, e aí valeria fazer o HP em camadas. Até lá, YAGNI.

---

## 5. Ganchos técnicos que o console JÁ deve respeitar

- **Stats calculados por composição**, não mutação direta. (Em implementação.)
- **Itens como lista de instâncias equipadas no combatente**, não valores
  somados direto no stat.
- **Modificadores permanentes moram no alvo afetado**, não na passiva que os
  aplica (evita "buff invisível" e permite cap compartilhado).
- **Não construir cedo demais:** não adicionar campos como Nivel ou Estrelas
  enquanto não forem usados. Objetivo é *não impedir* o futuro, não *construir*
  o futuro agora.

---

## 6. Modo Arena (Pós-Web)

> **Status:** Direção fechada; regras concretas a-definir em playtest.
> Não implementar no console — exige persistência de run, UI de draft e
> balanceamento por molde. Registrado aqui para não perder o conceito.

### Moldes de Time

Um **molde de time** é um arquétipo de composição — um esqueleto de "que tipo
de time montar", não um time fixo. Em vez de sortear personagens no RNG cru
(que gera times injogáveis ou triviais), cada molde tem regras de elegibilidade:
quais personagens podem entrar, em que proporção, com qual papel.

As 9 facções já são temas de sinergia prontos e servem como base natural para
os moldes. Exemplos de arquétipos possíveis:

- **Rush agressivo** — time de alta pressão, personagens de dano puro.
- **Controle/debuff** — time que aplica status e enrola o adversário.
- **Stall com Necromancia** — time que sustenta com revive + ImpedirRessurreicao.

> As regras concretas de cada molde (quais personagens entram, quantos, quais
> facções) são **a-definir em playtest**. Não cravar antes de ter dados reais.

### Motor Procedural

A cada run de Arena, o motor **escolhe um molde e sorteia personagens dentro
das regras dele** — gerando um time coerente e diferente a cada vez. A
variedade vem de recombinar o elenco existente (combinatória), não de criar
conteúdo novo nem de números subindo. É o "infinito horizontal" da Arena:
mesmo elenco, composições sempre novas.

### Decomposição como Enrage Timer

A mecânica de decomposição (ver Roadmap — Estado de Vida, gameplay futuro)
serve naturalmente como **enrage timer da Arena**: quando dois times entram em
loop e ninguém morre, os corpos acumulando ticks de decomposição forçam uma
resolução — o time que não conseguir reviver seus mortos perde terreno de
forma acumulativa até a explosão encerrar o combate. Anti-stall orgânico, sem
precisar de um timer externo artificial.

Esta é a conexão entre a mecânica de morte (fio técnico) e o design da Arena
(fio de jogo): o mesmo sistema resolve dois problemas ao mesmo tempo.

---

## 7. Posicionamento Tático (ideia jul/2026)

> Ideia nova e grande — muda o combate. Registrar, não implementar. **Prioridade:** depois do
> balanceamento da base E do refactor do `ExecutarTurno` (separar seleção-de-ação / seleção-de-alvo
> / execução). Interage com o targeting (`SelecaoDeAlvoService`), o motor (habilidade declara quais
> posições alcança) e o cálculo de dano (bônus por tipo de alvo).

- **Posições front/back importam:** cada habilidade define quais posições pode atingir.
- Tanque com espada só ataca o inimigo **mais próximo** (não pula posições).
- Arqueiros alcançam a **linha de trás** (magos); magos atingem a **linha da frente**.
- **Dano varia por tipo de alvo:** mago dá +dano em armadura pesada; arqueiro dá +dano em magos;
  linha de frente resiste melhor a arqueiros; magos têm +resistência mágica.

---

## 8. Sinergia de Facções (ideia jul/2026)

> As 9 facções já são temas de sinergia (ver Arena §6); isto as premia mecanicamente.

- **Bônus por quantidade** de personagens da mesma facção no time (ex: 3+ = +dano/resistência).
- **Passivas de facção amplificadas** com mais membros (ex: 3 personagens = passiva 50% mais forte).
- Possível **habilidade ativável exclusiva** ao atingir X membros da facção (ex: ataque cooperativo).

---

## 9. Modo Automático (ideia jul/2026 — NEAR-TERM, pré-web)

> Diferente do resto deste GDD (que é pós-web): o auto-battle é o **passo 2 do roadmap de dev**
> (ver topo). Fica aqui como spec, mas é near-term. **Pré-requisito PARCIALMENTE FEITO:** o refactor
> do `ExecutarTurno` separou o CONTROLE (quem decide ação/alvo) da execução via `IControladorDeTurno`
> (jul/2026) — o auto-mode é só um `ControladorAutomatico` novo trocado no composition root, sem tocar
> no loop. Falta: a implementação do controlador automático + o balanceamento da base.

- Botão **auto liga/desliga**; ao desligar, completa a ação atual e volta ao manual.
- Implementação com **async/await**: uma `Task` roda o loop de turnos com `Task.Delay()`,
  interrompida via `CancellationToken`.

---

## 10. Escopo — o que NÃO fazer agora

- NAO implementar itens com estrela/nível/raridade no console
- NAO implementar XP/níveis de personagem no console
- NAO implementar inventário persistente ou múltiplas cópias de item
- NAO implementar dificuldades destraváveis
- NAO implementar maestrias
- SIM continuar o sweep de Composição de Ações (motor de habilidades) por facção
- SIM fazer o refator de Stats em Camadas (serve ao presente E prepara o futuro)

Quando bater a empolgação com a expansão: **escrever neste documento**, não codar.
