using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Champs.Folclore
{
    /// <summary>
    /// Ao ser atacado, 25% de chance de aplicar Medo 1t no atacante. Migrada para
    /// o modelo de reação (IReageAoSerAtacado). Só declara mensagem quando dispara.
    /// </summary>
    public class Intimidador : HabilidadePassiva, IReageAoSerAtacado
    {
        private const double ChanceMedo = 0.25;

        public Intimidador() : base("Intimidador", "👹", 0,
            "25% de chance de aplicar Medo no atacante ao ser atacado.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Contraparte.EstaVivo()) return new List<ResultadoReacao>();
            if (Random.Shared.NextDouble() >= ChanceMedo) return new List<ResultadoReacao>();

            new Medo(duracao: 1).Aplicar(ctx.Contraparte);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"👹 O Intimidador encheu {ctx.Contraparte.Personagem.Nome} de Medo!"
                )
            };
        }
    }
}
