using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Skills.Buffs
{
    /// <summary>
    /// Buff: 1x por agressor por turno, o atacante recebe Veneno (1 stack) e Queima (1 stack).
    /// Reage via IReageAoSerAtacado (dispara mesmo com dano 0 — reage ao ATO de ser atacado, não
    /// ao dano: o espinho fere quem encosta mesmo se o golpe foi bloqueado). O gate 1x-por-agressor
    /// vem do orçamento do Turno (TentarReagir); antes era por-hit.
    /// Usado pela Espinhos (aplicado permanente via IPassivaInicial).
    /// </summary>
    class EspinhosVenenosos : Buff, IReageAoSerAtacado
    {
        public EspinhosVenenosos(int duracao = int.MaxValue)
            : base("Espinhos", "🌿", duracao, 0,
                "Atacantes recebem Veneno e Queima.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Contraparte.EstaVivo())
                return new List<ResultadoReacao>();

            if (!ctx.Portador.TentarReagir(GetType(), ctx.Contraparte, 1.0))
                return new List<ResultadoReacao>();   // 1x por agressor por turno

            new Veneno(stacks: 1).Aplicar(ctx.Contraparte);
            new Queima(stacks: 1).Aplicar(ctx.Contraparte);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Contraparte.Personagem.Nome} foi atingido pelos Espinhos! ☠️🔥"
                )
            };
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}