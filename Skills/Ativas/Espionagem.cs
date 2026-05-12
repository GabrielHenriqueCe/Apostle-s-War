using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Aplica -30% DEF em todos os inimigos por 2 turnos.
    /// </summary>
    class Espionagem : HabilidadeAtiva
    {
        public Espionagem() : base("Espionagem", "🔎", 4, "-30% DEF em todos os inimigos por 2 turnos.") { }
        public override int NumeroDeAlvos => int.MaxValue;

        public override void Ativar(Combate alvo, List<Combate>? aliados = null)
        {
            var alvos = aliados ?? new List<Combate> { alvo };
            foreach (Combate inimigo in alvos.Where(a => a.EstaVivo()))
            {
                new ReducaoDefesa(turnos: 2).Aplicar(inimigo);
            }
        }
    }
}
