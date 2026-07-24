using ApostlesWar;

namespace ApostlesWar.Skills.Debuffs
{
    /// <summary>
    /// Debuff stack-based de maldição.
    /// - Duração = nº de stacks (perde 1 stack por turno do portador)
    /// - Por turno: SÓ reduz HPMaximo em 10% do HPMaximoInicial (não causa dano)
    /// - Cap de redução acumulada: 50% do HPMaximoInicial
    /// - Após o cap: nada acontece
    /// - Sem cap de stacks
    /// </summary>
    class Maldicao : Debuff
    {
        public const double ReducaoPorTurno = 0.10;    // 10% HP inicial
        public const double CapPropio = 0.50;          // 50% redução acumulada

        public int Stacks { get; private set; }

        public Maldicao(int stacks = 1)
            : base("Maldição", "👁️‍🗨️", stacks, ReducaoPorTurno,
                $"Reduz {ReducaoPorTurno * 100:F0}% HP máximo/turno até 50%.")
        {
            Stacks = stacks;
        }

        public override void Aplicar(Combate alvo)
        {
            if (!alvo.PodeReceber(this)) return;

            var existente = alvo.StatusAtivos.OfType<Maldicao>().FirstOrDefault();
            if (existente != null)
            {
                existente.Stacks += this.Stacks;
                existente.DuracaoRestante = existente.Stacks;
                return;
            }

            alvo.StatusAtivos.Add(this);
        }

        /// <summary>
        /// No início do turno do portador: reduz HPMaximo (sem dano).
        /// </summary>
        public override EventoCombate? AoIniciarTurno(Combate portador)
        {
            int valor = (int)(portador.HPMaximoInicial * ReducaoPorTurno);

            int capAbsoluto = (int)(portador.HPMaximoInicial * CapPropio);
            int aindaPodeReduzir = capAbsoluto - portador.HPMaximoReduzidoTotal;

            if (aindaPodeReduzir > 0)
            {
                int reducao = Math.Min(valor, aindaPodeReduzir);
                portador.ReduzirHPMaximo(reducao);
            }

            return null;   // redução de HP máximo não é evento de dano/cura visível
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}