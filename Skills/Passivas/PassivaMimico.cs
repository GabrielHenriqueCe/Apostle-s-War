using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao ser atacado, escolhe 1 debuff aleatório do atacante e aumenta sua duração em 2 turnos.
    /// Análoga simétrica à PassivaCientista (que reduz buffs do atacante).
    /// </summary>
    class PassivaMimico : HabilidadePassiva
    {
        private const int AumentoDuracao = 2;

        private static readonly Random _random = new Random();

        public PassivaMimico() : base("Repetindo", "🎭", 0,
            "Ao ser atacado, aumenta a duração de 1 debuff aleatório do atacante em 2 turnos.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeSerAtacado && ctx.AlvoVivo;

        // ctx.Atacante = Mímico (portador); alvo = quem atacou
        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            if (!alvo.EstaVivo()) return SemDano();

            var debuffs = alvo.StatusAtivos.OfType<Debuff>().ToList();
            if (debuffs.Count == 0) return SemDano();

            var escolhido = debuffs[_random.Next(debuffs.Count)];
            escolhido.AumentarDuracao(AumentoDuracao);

            return SemDano();
        }
    }
}