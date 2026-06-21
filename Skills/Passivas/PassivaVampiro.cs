using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Passiva permanente: todos os ataques do Vampiro ignoram Invencível e BloqueioTotal
    /// do alvo. Não tem evento — funciona via interface IIgnoraStatusNoAtaque consultada
    /// pelo Combate.Atacar.
    /// </summary>
    class PassivaVampiro : HabilidadePassiva, IIgnoraStatusNoAtaque
    {
        private static readonly Type[] _tipos = new[]
        {
            typeof(Invencivel),
            typeof(BloqueioTotal)
        };

        public PassivaVampiro() : base("Drenagem", "🧛", 0,
            "Ataques ignoram Invencível e Bloqueio Total.")
        { }

        public IEnumerable<Type> TiposIgnorados => _tipos;

        // Passiva sem evento ativo — funciona via interface, não via DeveAtivar.
        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) => false;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();
    }
}