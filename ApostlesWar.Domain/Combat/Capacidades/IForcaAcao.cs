using ApostlesWar.Domain;

/// <summary>
/// Capacidade D do modelo de capacidades: FORÇAR A AÇÃO. O status decide o alvo da ação
/// forçada (A1) do portador, tirando a escolha do controlador. O CombateService pergunta
/// o AlvoForcado e executa A1 (Atacar) nele — a decisão de QUEM é do status.
/// Implementadores: Irritar (força atacar quem aplicou).
/// </summary>
public interface IForcaAcao
{
    Combate AlvoForcado();
}
