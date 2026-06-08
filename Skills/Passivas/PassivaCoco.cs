using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao ser atacado, aplica 2 stacks de Veneno no atacante. Migrada para o
    /// modelo de reação (IReageAoSerAtacado). Mais agressiva que a Zumbi (1 stack).
    /// (Por-hit por enquanto; "1x por agressor por turno" vem com o Turno.)
    /// </summary>
    class PassivaCoco : HabilidadePassiva, IReageAoSerAtacado
    {
        public PassivaCoco() : base("Fedorento", "💩", 0,
            "Ao ser atacado, aplica 2 stacks de Veneno no atacante.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Outro.EstaVivo())
                return new List<ResultadoReacao>();

            new Veneno(stacks: 2).Aplicar(ctx.Outro);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Outro.Personagem.Nome} foi envenenado pelo Fedorento! 💩"
                )
            };
        }
    }
}