using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao ser atacado, aplica 1 stack de Veneno no atacante. Migrada para o
    /// modelo de reação (IReageAoSerAtacado): dispara mesmo com dano 0 (reage ao
    /// ATO de ser atacada). Não usa DeveAtivar/Ativar — herda os defaults vazios
    /// da base, então o sistema velho (ExecutarPassivasReativas) a ignora.
    /// (Por-hit por enquanto; "1x por agressor por turno" vem com o conceito de Turno.)
    /// </summary>
    class PassivaZumbi : HabilidadePassiva, IReageAoSerAtacado
    {
        public PassivaZumbi() : base("Putrefação Contagiosa", "🧟", 0,
            "Ao ser atacado, aplica Veneno no atacante.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Outro.EstaVivo())
                return new List<ResultadoReacao>();

            new Veneno(stacks: 1).Aplicar(ctx.Outro);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Outro.Personagem.Nome} foi envenenado pela Putrefação Contagiosa! 🧟"
                )
            };
        }
    }
}