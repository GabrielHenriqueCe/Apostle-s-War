using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Aplica Preso em 1 inimigo por 2 turnos (pula turno atual + 1 turno extra).
    /// A PassivaPolicial pode estender a duração em +1 turno.
    /// </summary>
    class Prender : HabilidadeAtiva
    {
        public Prender() : base("Prender", "⛓️", 4,
            "Inimigo pula os próximos 2 turnos.")
        { }
        public override int NumeroDeAlvos => 1;

        public override void Ativar(Combate alvo, List<Combate>? aliados = null)
        {
            var preso = new Preso(turnos: 2);
            preso.Aplicar(alvo);
        }
    }
}
