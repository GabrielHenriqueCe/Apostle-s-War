using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Quando o portador recebe dano direto de um ataque (a1, habilidade, etc),
    /// contra-ataca com a1 imediatamente.
    /// 
    /// Tem cooldown intrínseco de 1 turno do portador — uma vez por turno só.
    /// Isso evita loops infinitos (A contra-ataca B → B contra-ataca A → ...) e
    /// limita contra-ataques múltiplos em AoE (só o primeiro hit dispara).
    /// 
    /// NÃO dispara em dano de Veneno/Queima (status passa atacante == null no ReceberDano).
    /// Só dispara se portador estiver vivo após receber o dano.
    /// </summary>
    class ContraAtaque : Buff
    {
        private bool _emCooldown = false;

        public ContraAtaque(int turnos = 2)
            : base("Contra-Ataque", "↩️", turnos, 0, "Contra-ataca com a1 ao receber dano (1x por turno).")
        { }

        public override void AoSerAtacado(Combate portador, Combate atacante, int danoCausado)
        {
            if (_emCooldown) return;
            if (!portador.EstaVivo()) return;

            _emCooldown = true;
            portador.Atacar(atacante);
        }

        /// <summary>
        /// Reset do CD intrínseco ao passar o turno.
        /// </summary>
        protected override void AoPassarTurno()
        {
            _emCooldown = false;
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}