using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Debuffs
{
    /// <summary>
    /// Debuff de Medo. Quando o portador vai usar uma ação (a1 ou habilidade),
    /// tem uma chance de paralizar — cancela a ação e (se for habilidade) ativa cooldown.
    /// O CombateService consulta TentaParalizar() antes de executar a ação.
    /// </summary>
    class Medo : Debuff
    {
        public const double ChancePadrao = 0.50;

        private static readonly Random _random = new Random();

        public double Chance { get; }

        public Medo(int turnos = 1, double chance = ChancePadrao)
            : base("Medo", "😱", turnos, chance, $"{chance * 100:F0}% de chance de cancelar ações.")
        {
            Chance = chance;
        }

        /// <summary>
        /// Rola o dado. true = ação foi paralizada (deve ser cancelada).
        /// false = ação procede normalmente.
        /// </summary>
        public bool TentaParalizar() => _random.NextDouble() < Chance;

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}