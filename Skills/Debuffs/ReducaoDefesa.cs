using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Debuffs
{
    /// <summary>
    /// Reduz a DEF do alvo em 30% pelo número de turnos especificado.
    /// Ao expirar, restaura a DEF original.
    /// </summary>
    class ReducaoDefesa : Debuff
    {
        private int _valorReduzido;

        public ReducaoDefesa(int turnos = 2)
            : base("Redução DEF", "🔎", turnos, 0.30, "-30% DEF.") { }

        /// <summary>
        /// Aplica a redução de DEF e registra o status. Chame este método, não Aplicar().
        /// </summary>
        public void AplicarEfeito(Combate alvo)
        {
            _valorReduzido = (int)(alvo.Defesa * Valor);
            alvo.ModificarDefesa(-_valorReduzido);
            base.Aplicar(alvo);
        }

        public override void Remover(Combate alvo)
        {
            alvo.ModificarDefesa(_valorReduzido);
            alvo.StatusAtivos.Remove(this);
        }
    }
}
