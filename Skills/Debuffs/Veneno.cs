using ApostlesWar;

namespace ApostlesWar.Skills.Debuffs
{
    /// <summary>
    /// Stack-based. Cada stack causa 5% do HP máximo no início do turno do portador (ignora defesa).
    /// Duração = nº de stacks. A cada passagem de turno, perde 1 stack.
    /// Reaplicar adiciona stacks (até o cap, se houver).
    /// </summary>
    class Veneno : Debuff
    {
        public const double DanoPorStack = 0.05;

        public int Stacks { get; private set; }
        public int? CapStacks { get; }

        public Veneno(int stacks = 1, int? capStacks = null)
            : base("Veneno", "☠️", stacks, DanoPorStack, $"Causa {DanoPorStack * 100:F0}% HP por stack/turno.")
        {
            Stacks = stacks;
            CapStacks = capStacks;
        }

        public override void Aplicar(Combate alvo)
        {
            if (!alvo.PodeReceber(this)) return;

            var existente = alvo.StatusAtivos.OfType<Veneno>().FirstOrDefault();
            if (existente != null)
            {
                int novosStacks = existente.Stacks + this.Stacks;
                if (CapStacks.HasValue) novosStacks = Math.Min(novosStacks, CapStacks.Value);
                existente.Stacks = novosStacks;
                existente.TurnosRestantes = novosStacks;
                return;
            }

            alvo.StatusAtivos.Add(this);
        }

        /// <summary>
        /// Causa o dano no início do turno do portador, antes da escolha de ação.
        /// </summary>
        public override void AoIniciarTurno(Combate portador)
        {
            int dano = (int)(portador.HPMaximo * DanoPorStack);
            portador.ReceberDano(dano, NaturezasDano.Veneno);
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }

        /// <summary>
        /// Calcula o dano total que seria causado se todos os stacks fossem aplicados de uma vez.
        /// Usado pela Putrefação.
        /// </summary>
        public int DanoTotalImediato(Combate portador) =>
            (int)(portador.HPMaximo * DanoPorStack * Stacks);

        /// <summary>
        /// % do HP máximo que o dano total representa. Usado pra calcular cura da Putrefação.
        /// </summary>
        public double PercentualDanoImediato => DanoPorStack * Stacks;
    }
}
