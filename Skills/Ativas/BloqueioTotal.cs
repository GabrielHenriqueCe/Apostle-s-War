using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text;

namespace v1_Apostle_s_War.Skills.Ativas
{
    #region Bloqueio Total

    class BloqueioTotal : Buff
    {
        public BloqueioTotal() : base("Bloqueio Total", 6, 1,"Bloqueia todo o dano recebido por um turno.") { }

        public override void Aplicar(Combate alvo)
        {
            alvo.StatusAtivos.Add(this);
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }

    }

    #endregion
}
