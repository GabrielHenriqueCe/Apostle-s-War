using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Champs.Folclore
{
    /// <summary>
    /// Ao ser atacado, aplica 1 stack de Maldição no atacante. Migrada para o
    /// modelo de reação (IReageAoSerAtacado).
    /// (Por-hit por enquanto; "1x por agressor por turno" vem com o Turno.)
    /// </summary>
    class PiadaDeMauGosto : HabilidadePassiva, IReageAoSerAtacado
    {
        public PiadaDeMauGosto() : base("Piada de Mau Gosto", "🤡", 0,
            "Ao ser atacado, amaldiçoa o atacante.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Contraparte.EstaVivo())
                return new List<ResultadoReacao>();

            new Maldicao(stacks: 1).Aplicar(ctx.Contraparte);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Contraparte.Personagem.Nome} foi amaldiçoado pela Piada de Mau Gosto! 🤡"
                )
            };
        }
    }
}
