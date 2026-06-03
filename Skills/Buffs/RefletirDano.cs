using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Buff: reflete uma % do dano efetivamente recebido de volta ao atacante,
    /// como dano direto (ReceberDanoDireto, sem disparar hooks).
    /// 
    /// Usa AoReceberDano (so dispara com dano > 0): se Escudo/BloqueioTotal
    /// cobriram tudo, nao reflete — RefletirDano e proporcional ao sofrimento
    /// real, nao ao ato.
    /// 
    /// Usa ReceberDanoDireto pra evitar loop infinito caso o atacante
    /// tambem tenha RefletirDano (reflexo nao volta).
    /// </summary>
    class RefletirDano : Buff
    {
        public RefletirDano(int turnos = 2, double percentual = 0.15)
            : base("Reflexo", "🥢", turnos, percentual,
                $"Reflete {percentual * 100:F0}% do dano recebido.")
        { }

        public override void AoReceberDano(Combate portador, Combate atacante, int danoCausado)
        {
            if (!atacante.EstaVivo()) return;

            int danoRefletido = (int)(danoCausado * Valor);
            if (danoRefletido > 0)
                atacante.ReceberDano(danoRefletido, NaturezasDano.Direto);
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}