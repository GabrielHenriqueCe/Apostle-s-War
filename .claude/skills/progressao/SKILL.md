---
name: progressao
description: Ler ANTES de implementar qualquer passo da progressão do Apostle's War — nível, XP, raridade, stat base por tipo/arquétipo, curva de DEF, precisão × resistência, barra de turno e velocidade, posição no tabuleiro, itens e slots, dificuldade e composição das fases.
---

# A progressão

O modelo está partido em três, e a **ordem dos passos não é negociável**:

1. **`docs/GDD-progressao.md` §7** — o PLANO. Status e turno ANTES de nível e raridade; subir status
   antes de mudar quem joga quando é calibrar contra uma ordem de turno que ainda vai mudar.
2. **`docs/GDD-combate.md`** (§1 e §2) — os stats novos, a barra de turno, a DEF em curva, precisão ×
   resistência, posição e tipo, a tabela de stats base dos 4 tipos.
3. **`docs/GDD-itens.md`** (§4) — slots, subs, escala, drop, evolução. É o passo mais DISTANTE da
   fila; quase nada ali está implementado.

E **`docs/GDD-progressao.md` §Decisões já fechadas (não reabrir)** antes de propor alternativa: quase
toda ideia "nova" já foi discutida e decidida lá, com o motivo. A tabela §Revogado diz o que já foi
tentado e por que caiu.

O GDD é MODELO, não fila: a regra continua valendo depois de implementada. A EXECUÇÃO (o que fazer
agora) está na FILA A do `docs/ROADMAP-refatoracao.md`.
