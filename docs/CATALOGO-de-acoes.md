# Catálogo de Ações — índice vivo do vocabulário do motor

> **Tipo:** Índice de referência (vivo). O "porquê" mora no **ADR-composicao-de-acoes.md**;
>   aqui é só "o quê existe e onde".
> **Leia ANTES de criar habilidade nova** — responde de uma olhada:
>   1. **Tem um verbo compartilhado que já serve?** (1ª tabela) — reaproveite, não reinvente
>      (ex: não invente um segundo jeito de reviver ou de estender duração).
>   2. **Tem um bespoke que eu deveria PROMOVER?** (2ª tabela) — se você é o 2º cliente de um
>      bespoke, é hora de movê-lo pra `Skills/Acoes/` (ADR §9), não copiá-lo.
> **Manutenção:** atualize este arquivo no MESMO PR que adiciona / promove / remove uma Ação.
>   O detalhe de cada uma está no doc-comment do próprio arquivo — o catálogo só INDEXA.

---

## 1. Vocabulário compartilhado (`Skills/Acoes/`) — alcance PRIMEIRO por estes

| Ação | O que faz | Eixos | Arquivo |
|---|---|---|---|
| `Dano` | dano ao alvo (mult ATK; `ignorarDefesaPct`/`forcaCritico`/`ignorarStatus` opcionais; mult pode ser `Func(atk,alvo)`) | — | `Skills/Acoes/Dano.cs` |
| `Cura` | cura o alvo por um fragmento de Valor | Valor | `Skills/Acoes/Cura.cs` |
| `AplicarEscudo` | Escudo no alvo por um fragmento de Valor (+ turnos) | Valor | `Skills/Acoes/AplicarEscudo.cs` |
| `AplicarBuff` | aplica um buff (recebe FÁBRICA; sobrecarga `Func<Combate,Buff>` p/ proveniência) | — | `Skills/Acoes/AplicarBuff.cs` |
| `AplicarDebuff` | aplica um debuff (FÁBRICA; sobrecarga `Func<Combate,Debuff>` p/ proveniência; `chance` opcional) | — | `Skills/Acoes/AplicarDebuff.cs` |
| `Reviver` | revive um morto por % do HP máximo | — | `Skills/Acoes/Reviver.cs` |
| `RemoverBuffs` | remove buffs do alvo conforme um Seletor | Seletor | `Skills/Acoes/RemoverBuffs.cs` |
| `RemoverDebuffs` | remove debuffs do alvo conforme um Seletor (gêmeo do RemoverBuffs) | Seletor | `Skills/Acoes/RemoverDebuffs.cs` |
| `Explodir` | detona status de tick (`IStatusComTick`) conforme Seletor → devolve `EventoDano` | Seletor | `Skills/Acoes/Explodir.cs` |

### Eixos (compõem as ações acima)
- **`Valor`** (`Skills/Acoes/Valor.cs`) — fragmento de magnitude: `Fixo(v)`, `PorHP(%)`,
  `PorHPDoAtacante(%)`, `PorDanoCausado(%)`.
- **`Seletor`** (`Skills/Acoes/Seletor.cs`) — QUAIS/QUANTOS status: `Todos()`, `Removiveis()`,
  `Tipo<T>()` — cada um com `(quantos, aleatorio)`.
- **`Escopo`** (enum) — em QUAIS combatentes a ação cai: `AlvosResolvidos` (default, o pick da
  habilidade), `TodosAliados` (inclui o próprio), `TodosInimigos`, `ProprioAtacante`, `OutrosAliados`.
- **`EstadoAlvo`** (enum) — `Vivos` / `Mortos`, avaliado NA EXECUÇÃO. (`Ambos` está em extinção —
  o estado desce pra ação; ver ADR §5.3.)

---

## 2. Bespokes por champ (Nível 2/3 — locais até o 2º cliente) — NÃO recriar, PROMOVER

Ações custom que vivem na pasta do champ porque têm só 1 cliente. Se um SEGUNDO champ precisar do
mesmo verbo, **pare e promova** pra `Skills/Acoes/` (ADR §9) em vez de duplicar.

| Ação | O que faz | Nível | Cliente | Gatilho de promoção | Arquivo |
|---|---|---|---|---|---|
| `GolpeSeguidor` | ataque cujo ignore-DEF depende do hit anterior ter sido crítico | 3 | Shuriken (Ninja) | 2º com acoplamento hit-a-hit | `Champs/Reino/Ninja/GolpeSeguidor.cs` |
| `AutoDano` | dano ao PRÓPRIO atacante por um fragmento de Valor | 2 | VindoDoAlém (Fantasma) | 2º cliente real | `Champs/LadoSombrio/Fantasma/AutoDano.cs` |
| `EstenderBuffs` | estende a duração de buffs conforme um Seletor | 2 | RaioX (Robô) | 2º cliente ATIVO (→ nasce `EstenderDebuffs`) | `Champs/Tecnologicos/Robo/EstenderBuffs.cs` |

---

## 3. Vocabulário mapeado, mas NÃO construído (nasce quando o 1º cliente migrar)

Nomes reservados (ADR §9) — não invente sinônimo, use estes quando o cliente chegar.

| Vocabulário | O que seria | Clientes previstos |
|---|---|---|
| `MoverBuffs` | roubo de buff (do alvo pro conjurador) | Copiando |
| `ConcederTurnoExtra` | concede um turno extra | Copiando |
| `AcaoSobreConjunto` | 2º formato de ação p/ agregação cross-alvo (construída e removida — ADR §3.4) | candidata: Atlantis (§8.1) |
