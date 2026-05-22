using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Debuffs
{
    /// <summary>
    /// Reduz a DEF do alvo em 30% pelo número de turnos especificado.
    /// Ao expirar, restaura o valor reduzido.
    /// 
    /// Declara ContribuicaoDefesa negativa pra permitir ignorar via habilidade.
    /// </summary>
    class ReducaoDefesa : Debuff
    {
        private int _valorReduzido;

        public ReducaoDefesa(int turnos = 2)
            : base("Redução DEF", "🔎", turnos, 0.30, "-30% DEF.") { }

        public override void Aplicar(Combate alvo)
        {
            if (!alvo.PodeReceber(this)) return;

            _valorReduzido = (int)(alvo.Defesa * Valor);
            alvo.ModificarDefesa(-_valorReduzido);
            base.Aplicar(alvo);
        }

        public override int ContribuicaoDefesa(Combate portador) => -_valorReduzido;

        public override void Remover(Combate alvo)
        {
            alvo.ModificarDefesa(_valorReduzido);
            alvo.StatusAtivos.Remove(this);
        }
    }
}