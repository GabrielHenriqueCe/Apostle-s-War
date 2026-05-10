using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    class ParedeDeTijolos : HabilidadeAtiva
    {
        public ParedeDeTijolos() : base("Parede de Tijolos", "🧱", 6, "Bloqueia 100% do dano de todos os aliados por 1 turno.") { }
        public override int NumeroDeAlvos => int.MaxValue;

        public override void Ativar(Combate alvo, List<Combate>? aliados = null)
        {
            if (aliados != null)
            {
                foreach (Combate aliado in aliados.Where(a => a.EstaVivo()))
                {
                    aliado.StatusAtivos.Add(new BloqueioTotal());
                }
            }
        }
    }
}
