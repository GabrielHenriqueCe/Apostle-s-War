using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Aplica ImunidadeDebuffs permanente no início do combate.
    /// </summary>
    class PassivaAbobora : HabilidadePassiva, IPassivaInicial
    {
        public PassivaAbobora() : base("Casca Dura", "🎃", 0,
            "Imune a maleficios.")
        { }

        public void AplicarInicial(Combate portador)
        {
            new ImunidadeDebuffs().Aplicar(portador);
        }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) => false;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
            => SemDano();
    }
}