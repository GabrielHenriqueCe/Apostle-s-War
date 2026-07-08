using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Passiva permanente: todos os ataques do Vampiro ignoram Invencível e BloqueioTotal
    /// do alvo. Não tem evento — funciona via interface IIgnoraStatusNoAtaque consultada
    /// pelo Combate.Atacar.
    /// </summary>
    class Drenagem : HabilidadePassiva, IIgnoraStatusNoAtaque
    {
        private static readonly Type[] _tipos = new[]
        {
            typeof(Invencivel),
            typeof(BloqueioTotal)
        };

        public Drenagem() : base("Drenagem", "🧛", 0,
            "Ataques ignoram Invencível e Bloqueio Total.")
        { }

        public IEnumerable<Type> TiposIgnorados => _tipos;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();
    }
}