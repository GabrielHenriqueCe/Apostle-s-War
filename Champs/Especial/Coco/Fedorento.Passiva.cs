using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Champs.Especial
{
    /// <summary>
    /// Ao ser atacado, aplica 2 stacks de Veneno no atacante. Migrada para o
    /// modelo de reação (IReageAoSerAtacado). Mais agressiva que o Vômito Tóxico do Zumbi (1 stack).
    /// (Por-hit por enquanto; "1x por agressor por turno" vem com o Turno.)
    /// </summary>
    class Fedorento : HabilidadePassiva, IReageAoSerAtacado
    {
        public Fedorento() : base("Fedorento", "💩", 0,
            "Ao ser atacado, aplica 2 stacks de Veneno no atacante.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Contraparte.EstaVivo())
                return new List<ResultadoReacao>();

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
