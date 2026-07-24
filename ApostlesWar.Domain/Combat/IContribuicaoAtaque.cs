using ApostlesWar.Domain;

/// <summary>
/// Capacidade C do modelo de capacidades: CONTRIBUIÇÃO DE STAT (ataque).
/// O status soma (buff) ou subtrai (debuff) do ATK do portador, calculado
/// sob demanda pelo getter Combate.Ataque. A contribuição já vem COM SINAL
/// (positiva no buff, negativa no debuff), então o getter só precisa somar.
/// Implementadores: BuffAtaque (positivo), ReducaoAtaque (negativo).
/// </summary>
public interface IContribuiAtaque
{
    int ContribuicaoAtaque(Combate portador);
}
