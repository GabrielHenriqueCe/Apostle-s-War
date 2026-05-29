using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Debuffs
{
    /// <summary>
    /// Debuff stack-based.
    /// Quando qualquer atacante causa dano direto no portador, o atacante é curado em
    /// 15% do dano causado (cap = HP máximo do atacante).
    /// 
    /// - Duração = nº de stacks (perde 1 stack por turno do portador, igual Veneno)
    /// - Stacks NÃO afetam o % de cura (sempre 15%) — só duração
    /// - Não cura em dano de Veneno/Queima (atacante == null no ReceberDano)
    /// </summary>
    class Sangramento : Debuff
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

        public override void AoReceberDano(Combate portador, Combate atacante, int danoCausado)
        {
            atacante.Curar((int)(danoCausado * PercentualCura));
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}