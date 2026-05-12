using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Debuffs
{
    /// <summary>
    /// Reduz a DEF do alvo em 30% pelo número de turnos especificado.
    /// Ao expirar, restaura o valor reduzido.
    /// </summary>
    class ReducaoDefesa : Debuff
    {
        private int _valorReduzido;

        public ReducaoDefesa(int turnos = 2)
            : base("Redução DEF", "🔎", turnos, 0.30, "-30% DEF.") { }

        public override void Aplicar(Combate alvo)
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
