using ApostlesWar.Domain;

/// <summary>
/// Capacidade C do modelo de capacidades: CONTRIBUIÇÃO DE STAT (taxa de crítico).
/// O status soma (buff) ou subtrai (debuff) da TaxaCrit do portador — pontos
/// ABSOLUTOS, não percentual de percentual — calculado sob demanda pelo getter
/// Combate.TaxaCrit. A contribuição já vem COM SINAL.
/// Implementadores: BuffTaxaCrit (positivo), ReducaoTaxaCrit (negativo).
/// </summary>
public interface IContribuiTaxaCrit
{
    double ContribuicaoTaxaCrit(Combate portador);
}
