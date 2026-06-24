using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Passiva permanente do Morcego: cura 15% do dano causado em qualquer ataque.
    /// Aplica Sedento via IPassivaInicial no IniciarCombate.
    /// </summary>
    class PassivaMorcego : HabilidadePassiva, IPassivaInicial
    {
        public PassivaMorcego() : base("Sedento de Sangue", "🦇", 0,
            "Cura 15% do dano causado ao atacar.")
        { }

        public void AplicarInicial(Combate portador)
        {
            new Sedento(percentual: 0.15).Aplicar(portador);
        }

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();
    }
}