using ApostlesWar;

/// <summary>
/// Capacidade D do modelo de capacidades: PULAR O TURNO. Marcador — a presença de um
/// status IPulaTurno em StatusAtivos faz o combatente pular o turno inteiro (o
/// ExecutarTurnoCompleto avança status/cooldowns e NÃO age). Tira do CombateService a
/// decisão por tipo concreto: o status carrega a própria capacidade.
///
/// É a porta da FAMÍLIA de controle-de-turno: Preso hoje; variantes futuras
/// (Congelar/Stun/Enraizado/Petrificado…) plugam aqui sem tocar no fluxo. O que cada
/// variante faz A MAIS é OUTRA capacidade composta por cima — não entra neste contrato.
/// Implementadores: Preso.
/// </summary>
interface IPulaTurno { }
