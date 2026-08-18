using ApostlesWar.Domain;

/// <summary>
/// Capacidade D do modelo de capacidades: PARALISAR A AÇÃO. Depois de a ação ser escolhida,
/// o status rola o próprio dado; true = a ação é cancelada (o combatente perde o turno e, se
/// era habilidade, o cooldown). O CombateService consulta após a seleção — a chance é do status.
/// Implementadores: Medo (chance de paralisar).
/// </summary>
public interface IParalisaAcao
{
    bool Paralisa();
}
