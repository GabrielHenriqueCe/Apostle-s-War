using ApostlesWar.Domain;

/// <summary>
/// Capacidade C do modelo de capacidades: CONTRIBUIÇÃO DE STAT (defesa).
/// O status soma (buff) ou subtrai (debuff) da DEF do portador, calculado
/// sob demanda. Consultada em Combate.ReceberDano quando um ataque IGNORA
/// um status de defesa específico (ex: Vendaval ignora BuffDefesa) — a
/// contribuição é descontada da defesa efetiva.
/// Implementadores: BuffDefesa (positivo), ReducaoDefesa (negativo).
/// </summary>
public interface IContribuiDefesa
{
    int ContribuicaoDefesa(Combate portador);
}