using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text;

namespace v1_Apostle_s_War.Skills.Buffs
{
    #region Bloqueio Total

    class BloqueioTotal : Buff
    {
        public BloqueioTotal(int turnos = 1) : base("Bloqueio Total", "🧱", turnos, 1) { }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }

    #endregion
}
