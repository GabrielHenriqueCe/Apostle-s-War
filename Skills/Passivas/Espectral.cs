using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Skills.Passivas
{
    /// <summary>
    /// Aplica Intocavel permanente no portador no início do combate.
    /// </summary>
    class Espectral : HabilidadePassiva, IPassivaInicial
    {
        public Espectral() : base("Espectral", "👻", 0,
            "Intocável durante todo o combate.")
        { }

        public void AplicarInicial(Combate portador)
        {
            new Intocavel(turnos: int.MaxValue, removivel: false).Aplicar(portador);
        }

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
            => SemDano();
    }
}