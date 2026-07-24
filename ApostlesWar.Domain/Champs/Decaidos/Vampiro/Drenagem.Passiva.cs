using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Passivas;

namespace ApostlesWar.Domain.Champs.Decaidos
{
    /// <summary>
    /// Passiva permanente: todos os ataques do Vampiro ignoram Invencível e BloqueioTotal
    /// do alvo. Não tem evento — funciona via interface IIgnoraStatusNoAtaque consultada
    /// pelo Combate.Atacar.
    /// </summary>
    public class Drenagem : HabilidadePassiva, IIgnoraStatusNoAtaque
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

        public override List<EventoCombate> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();
    }
}