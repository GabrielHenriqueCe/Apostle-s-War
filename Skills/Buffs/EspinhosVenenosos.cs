using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Skills.Buffs
{
    /// <summary>
    /// Buff: cada vez que o portador é atacado, o atacante recebe Veneno (1 stack)
    /// e Queima (1 stack). Sem CD — dispara em cada hit. Reage via
    /// IReageAoSerAtacado (dispara mesmo com dano 0 — reage ao ATO de ser atacado,
    /// não ao dano: o espinho fere quem encosta mesmo se o golpe foi bloqueado).
    /// Usado pela Espinhos (aplicado permanente via IPassivaInicial).
    /// </summary>
    class EspinhosVenenosos : Buff, IReageAoSerAtacado
    {
        public EspinhosVenenosos(int turnos = int.MaxValue)
            : base("Espinhos", "🌿", turnos, 0,
                "Atacantes recebem Veneno e Queima.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Contraparte.EstaVivo())
                return new List<ResultadoReacao>();

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