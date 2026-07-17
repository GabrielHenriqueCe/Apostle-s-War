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
| `Dano` | dano ao alvo (mult ATK; `ignorarDefesaPct`/`forcaCritico`/`ignorarStatus` opcionais; `ignorarStatus` casa por tipo-BASE — `typeof(Buff)` ignora todos os buffs; mult pode ser `Func(atk,alvo)`) | — | `Skills/Acoes/Dano.cs` |
| `Cura` | cura o alvo por um fragmento de Valor | Valor | `Skills/Acoes/Cura.cs` |
| `AplicarEscudo` | Escudo no alvo por um fragmento de Valor (+ turnos) | Valor | `Skills/Acoes/AplicarEscudo.cs` |
| `AplicarBuff` | aplica um buff (recebe FÁBRICA; sobrecarga `Func<Combate,Buff>` p/ proveniência) | — | `Skills/Acoes/AplicarBuff.cs` |
| `AplicarDebuff` | aplica um debuff (FÁBRICA; sobrecarga `Func<Combate,Debuff>` p/ proveniência; `chance` opcional) | — | `Skills/Acoes/AplicarDebuff.cs` |
| `Reviver` | revive um morto por % do HP máx (buff opcional `buffNoRevivido` — só nos revividos) | — | `Skills/Acoes/Reviver.cs` |
| `RemoverBuffs` | remove buffs do alvo conforme um Seletor | Seletor | `Skills/Acoes/RemoverBuffs.cs` |
| `RemoverDebuffs` | remove debuffs do alvo conforme um Seletor (gêmeo do RemoverBuffs) | Seletor | `Skills/Acoes/RemoverDebuffs.cs` |
| `MoverBuffs` | move buffs do alvo PRO ATACANTE conforme um Seletor (gêmeo do RemoverBuffs, move a instância) | Seletor | `Skills/Acoes/MoverBuffs.cs` |
| `Explodir` | detona status de tick (`IStatusComTick`) conforme Seletor → devolve `EventoDano` | Seletor | `Skills/Acoes/Explodir.cs` |
| `ConcederTurnoExtra` | concede um turno extra ao escopo (default `ProprioAtacante`) | — | `Skills/Acoes/ConcederTurnoExtra.cs` |

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
| `RestaurarHPMaximo` | restaura HP máximo perdido do alvo, até um cap % do HP máx inicial | 2 | DragãoProtetor (Dragão) | 2º cliente real | `Champs/Misticos/Dragao/RestaurarHPMaximo.cs` |

---

## 3. Vocabulário mapeado, mas NÃO construído (nasce quando o 1º cliente migrar)

Nomes reservados (ADR §9) — não invente sinônimo, use estes quando o cliente chegar.

| Vocabulário | O que seria | Clientes previstos |
|---|---|---|
| `AcaoSobreConjunto` | 2º formato de ação p/ agregação cross-alvo (construída e removida — ADR §3.4) | candidata: Atlantis (§8.1) |

> `ConcederTurnoExtra` saiu desta tabela — **construído** nos Decaídos (1º cliente real: Rato
> Voador do Morcego, não o Copiando como previsto). Ver tabela 1.
> `MoverBuffs` saiu desta tabela — **construído** nos Apóstolos (cliente: Copiando do Mímico).
> Ver tabela 1. Com isso o sweep das 9 facções está COMPLETO e o vocabulário mapeado esgotou-se
> (só resta `AcaoSobreConjunto`, que segue sem cliente).

---

## 4. "Ignorar status" — de quem é a perfuração? (língua única, unificada jul/2026)

Ao criar habilidade que FURA um status, pergunte **de quem vem a perfuração** — há UMA língua só
(lista de tipos), com 4 fontes conforme o dono:

| A perfuração é… | Use | Exemplo |
|---|---|---|
| da ESSÊNCIA do dano (todo dano desse tipo, sempre) | perfil de `NaturezaDano` (campo `Ignora`) | QueimaDano fura Escudo |
| só DESTE GOLPE | `ignorarStatus` na ação `Dano` | CorteDeVento fura Escudo; PóMágico fura `typeof(Buff)` |
| DESTE CHAMP, em todo ataque (identidade) | passiva `IIgnoraStatusNoAtaque` | Drenagem (Vampiro) fura Invencível+Bloqueio |
| % do stat DEFESA (não é status) | `ignorarDefesaPct` no `Dano` | Vendaval, Controle de Sangue |

As três primeiras convergem numa lista só no `ReceberDano` (match por tipo EXATO ou BASE —
`typeof(Buff)` fura todos os buffs). NÃO existe mais `DeveAgir` por-status lendo flags da natureza.
Passiva defensiva (Aquagirl) NÃO é perfurável por lista de propósito (não é status — PóMágico não a fura).
