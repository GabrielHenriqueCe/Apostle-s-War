using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Champs.Especial
{
    /// <summary>
    /// Ao matar um inimigo, aplica a Sentença (ImpedirRessurreicao) no morto — ele não
    /// pode ser ressuscitado. Reação (IReageAoMatar), roda antes do "ao morrer", então
    /// o debuff já está no morto quando a Necromancia/Guarda tentam reviver. O Diabo
    /// (AnjoCaido) remove a Sentença proposital.
    /// </summary>
    public class Sentenca : HabilidadePassiva, IReageAoMatar
    {
        public Sentenca() : base("Sentença", "🦹", 0,
            "Inimigos mortos por ele não podem ser ressuscitados.")
        { }

        public List<ResultadoReacao> AoMatar(ContextoReacao ctx)
        {
            new ImpedirRessurreicao().Aplicar(ctx.Contraparte);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(Mensagem: $"🦹 Sentença: {ctx.Contraparte.Personagem.Nome} não poderá ressuscitar!")
            };
        }
    }
}
