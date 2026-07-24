using ApostlesWar.Domain;

/// <summary>
/// Capacidade C do modelo de capacidades: CONTRIBUIÇÃO DE STAT (dano de crítico).
/// O status soma (buff) ou subtrai (debuff) do DanoCrit do portador — pontos
/// ABSOLUTOS — calculado sob demanda pelo getter Combate.DanoCrit. A contribuição
/// já vem COM SINAL.
/// Implementadores: BuffDanoCrit (positivo), ReducaoDanoCrit (negativo).
/// </summary>
public interface IContribuiDanoCrit
{
    double ContribuicaoDanoCrit(Combate portador);
}
