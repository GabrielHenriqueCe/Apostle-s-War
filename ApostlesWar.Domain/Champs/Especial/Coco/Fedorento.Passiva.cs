using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Champs.Especial
{
    /// <summary>
    /// 1x por agressor por turno, aplica 2 stacks de Veneno no atacante. Modelo de reação
    /// (IReageAoSerAtacado). Mais agressiva que o Vômito Tóxico do Zumbi (1 stack). O gate
    /// 1x-por-agressor vem do Turno (TentarReagir); antes era por-hit.
    /// </summary>
    public class Fedorento : HabilidadePassiva, IReageAoSerAtacado
    {
        public Fedorento() : base("Fedorento", "💩", 0,
            "Ao ser atacado, aplica 2 stacks de Veneno no atacante.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Contraparte.EstaVivo())
                return new List<ResultadoReacao>();

            if (!ctx.Portador.TentarReagir(GetType(), ctx.Contraparte, 1.0))
                return new List<ResultadoReacao>();   // 1x por agressor por turno

            new Veneno(stacks: 2).Aplicar(ctx.Contraparte);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Contraparte.Personagem.Nome} foi envenenado pelo Fedorento! 💩"
                )
            };
        }
    }
}
