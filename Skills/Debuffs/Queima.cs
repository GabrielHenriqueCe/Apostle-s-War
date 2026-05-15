using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Debuffs
{
    /// <summary>
    /// Reduz o HP máximo em 5% por aplicação, até 5 stacks (25%).
    /// </summary>
    class Queima : Debuff
    {
        private int _reducaoAplicada;

        public Queima(int turnos = 2) : base("Queima", "🔥", turnos, 0.05,
            "Reduz HP máximo em 5% por aplicação, até 25%.")
        { }

        public override void Aplicar(Combate alvo)
        {
            int queimesAtivas = alvo.StatusAtivos.OfType<Queima>().Count();
            if (queimesAtivas >= 5) return;

            _reducaoAplicada = (int)(alvo.HPMaximo * Valor);
            alvo.ModificarHPMaximo(-_reducaoAplicada);

            alvo.StatusAtivos.Add(this);
        }

        public override void Remover(Combate alvo)
        {
            alvo.ModificarHPMaximo(_reducaoAplicada);
            alvo.StatusAtivos.Remove(this);
        }
    }
}