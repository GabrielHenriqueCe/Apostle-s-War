using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Champs.LadoSombrio
{
    /// <summary>
    /// 1x por agressor por turno, aplica 1 stack de Veneno no atacante. Modelo de reação
    /// (IReageAoSerAtacado): dispara mesmo com dano 0 (reage ao ATO de ser atacada). Não usa
    /// DeveAtivar/Ativar — herda os defaults vazios da base, então o sistema velho
    /// (ExecutarPassivasReativas) a ignora. O gate 1x-por-agressor vem do Turno (TentarReagir);
    /// antes era por-hit.
    /// </summary>
    class PutrefacaoContagiosa : HabilidadePassiva, IReageAoSerAtacado
    {
        public PutrefacaoContagiosa() : base("Putrefação Contagiosa", "🧟", 0,
            "Ao ser atacado, aplica Veneno no atacante.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Contraparte.EstaVivo())
                return new List<ResultadoReacao>();

            if (!ctx.Portador.TentarReagir(GetType(), ctx.Contraparte, 1.0))
                return new List<ResultadoReacao>();   // 1x por agressor por turno

            new Veneno(stacks: 1).Aplicar(ctx.Contraparte);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Contraparte.Personagem.Nome} foi envenenado pela Putrefação Contagiosa! 🧟"
                )
            };
        }
    }
}
