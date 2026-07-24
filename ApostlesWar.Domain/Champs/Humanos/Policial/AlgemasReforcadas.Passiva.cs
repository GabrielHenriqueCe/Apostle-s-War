using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Champs.Humanos
{
    /// <summary>
    /// Atacar um inimigo Preso adiciona +1 turno ao debuff. Migrada para IReagePorAtaque
    /// (por alvo atingido). Só declara mensagem se o alvo tinha Preso.
    /// </summary>
    public class AlgemasReforcadas : HabilidadePassiva, IReagePorAtaque
    {
        public AlgemasReforcadas() : base("Algemas Reforçadas", "🔗", 0,
            "Atacar um inimigo Preso adiciona +1 turno ao debuff.")
        { }

        public List<ResultadoReacao> PorAtaque(ContextoReacao ctx)
        {
            var preso = ctx.Contraparte.StatusAtivos.OfType<Preso>().FirstOrDefault();
            if (preso == null) return new List<ResultadoReacao>();

            preso.AumentarDuracao(1);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(Mensagem: $"🔗 Algemas Reforçadas prolongaram o aprisionamento de {ctx.Contraparte.Personagem.Nome}!")
            };
        }
    }
}