using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Skills.Debuffs
{
    /// <summary>
    /// Debuff stack-based. Quando o portador recebe dano, o ATACANTE é curado em
    /// 15% do dano causado. Reage via IReageAoReceberDano (só com dano > 0).
    /// - Duração = nº de stacks (perde 1 por turno, igual Veneno)
    /// - Stacks não afetam o % (sempre 15%) — só duração
    /// </summary>
    public class Sangramento : Debuff, IReageAoReceberDano
    {
        public const double PercentualCura = 0.15;

        public int Stacks { get; private set; }

        public Sangramento(int stacks = 1)
            : base("Sangramento", "🩸", stacks, PercentualCura,
                $"Atacantes curam {PercentualCura * 100:F0}% do dano causado.")
        {
            Stacks = stacks;
        }

        public override void Aplicar(Combate alvo)
        {
            if (!alvo.PodeReceber(this)) return;

            var existente = alvo.StatusAtivos.OfType<Sangramento>().FirstOrDefault();
            if (existente != null)
            {
                existente.Stacks += this.Stacks;
                existente.DuracaoRestante = existente.Stacks;
                return;
            }

            alvo.StatusAtivos.Add(this);
        }

        public List<ResultadoReacao> AoReceberDano(ContextoReacao ctx)
        {
            // ctx.Portador tem o Sangramento; ctx.Contraparte (o atacante) é quem se cura.
            int pedido = (int)(ctx.DanoCausado * PercentualCura);
            if (pedido <= 0)
                return new List<ResultadoReacao>();

            int cura = ctx.Contraparte.Curar(pedido);   // o que de FATO curou (capado no HP máximo)

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(Cura: new EventoCura(ctx.Contraparte, ctx.Contraparte, cura, ctx.Contraparte.HPAtual))
            };
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}