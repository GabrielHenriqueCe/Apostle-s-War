using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Passivas;

namespace ApostlesWar.Domain.Champs.LadoSombrio
{
    /// <summary>
    /// Aplica Intocavel permanente no portador no início do combate.
    /// </summary>
    public class Espectral : HabilidadePassiva, IPassivaInicial
    {
        public Espectral() : base("Espectral", "👻", 0,
            "Intocável durante todo o combate.")
        { }

        public void AplicarInicial(Combate portador)
        {
            new Intocavel(duracao: int.MaxValue, removivel: false).Aplicar(portador);
        }

        public override List<EventoCombate> Ativar(ContextoCombate ctx, Combate alvo)
            => SemDano();
    }
}
