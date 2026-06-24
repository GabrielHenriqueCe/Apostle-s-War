using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Passiva permanente do Anjo: começa o combate com CuraContinua infinita 5%/turno.
    /// Aplicado via IPassivaInicial no IniciarCombate.
    /// </summary>
    class PassivaAnjo : HabilidadePassiva, IPassivaInicial
    {
        public PassivaAnjo() : base("Bênção", "😇", 0,
            "Recupera 5% HP por turno permanentemente.")
        { }

        public void AplicarInicial(Combate portador)
        {
            new CuraContinua(turnos: int.MaxValue, percentual: 0.05).Aplicar(portador);
        }

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();
    }
}