using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Aplica Intocavel permanente no portador no início do combate.
    /// </summary>
    class PassivaFantasma : HabilidadePassiva, IPassivaInicial
    {
        public PassivaFantasma() : base("Espectral", "👻", 0,
            "Intocável durante todo o combate.")
        { }

        public void AplicarInicial(Combate portador)
        {
            new Intocavel(turnos: int.MaxValue).Aplicar(portador);
        }

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
            => SemDano();
    }
}