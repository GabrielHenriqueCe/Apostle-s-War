using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Debuffs
{
    /// <summary>
    /// Debuff stack-based. Quando o portador recebe dano, o ATACANTE é curado em
    /// 15% do dano causado. Reage via IReageAoReceberDano (só com dano > 0).
    /// - Duração = nº de stacks (perde 1 por turno, igual Veneno)
    /// - Stacks não afetam o % (sempre 15%) — só duração
    /// </summary>
    class Sangramento : Debuff, IReageAoReceberDano
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
                existente.TurnosRestantes = existente.Stacks;
                return;
            }

            alvo.StatusAtivos.Add(this);
        }

        public List<ResultadoReacao> AoReceberDano(ContextoReacao ctx)
        {
            // ctx.Portador tem o Sangramento; ctx.Contraparte (o atacante) é quem se cura.
            int cura = (int)(ctx.DanoCausado * PercentualCura);
            if (cura <= 0)
                return new List<ResultadoReacao>();

            ctx.Contraparte.Curar(cura);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Contraparte.Personagem.Nome} se cura em {cura} com o Sangramento do inimigo! 🩸",
                    Cura: cura
                )
            };
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}