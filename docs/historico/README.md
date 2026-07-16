# Histórico — ADRs de planos já executados

Estes ADRs eram **planos de execução** que já foram **100% implementados**. Saíram da fila de
leitura principal (`docs/`) porque hoje a verdade é o próprio código + a seção **CONCLUÍDO** do
`ROADMAP-refatoracao.md`. Ficam aqui preservados pelo **porquê** das decisões — não deletados.

| ADR | O que era | Onde vive hoje |
|---|---|---|
| `ADR-sistema-de-efeitos.md` | Origem: unificar efeitos/dano em 3 separações (PR-A/B/C…) | Código + `ADR-modelo-de-capacidades.md` (o conceito durável). Único resíduo não-feito (identidade comum) foi pro ROADMAP boy-scout. |
| `ADR-PR-C-reacoes.md` | Plano C1–C7 das reações via interface | Código (C5 completo) + `ADR-modelo-de-capacidades.md` (Categoria A) |
| `ADR-selecao-de-alvo.md` | Extrair `SelecaoDeAlvoService` (regra) da UI e do bot | Código (`Services/SelecaoDeAlvoService.cs`) |
| `ADR-selecao-por-estado.md` | Passo 5 do Estado de Vida (EstadoAlvo, passiva-pura) | Essência absorvida em `ADR-estado-de-vida-e-atos.md §11`; forma atual em `ADR-composicao-de-acoes.md §5.3` |

> Se um destes voltar a virar trabalho vivo (improvável), traz de volta pra `docs/`.
