using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Aplica Preso 1t em 2 inimigos aleatórios.
    /// </summary>
    class Abduzir : HabilidadeAtiva
    {
        public Abduzir() : base("Abduzir", "🛸", 4,
            "Incapacita 2 inimigos aleatórios por 1 turno.")
        { }

        public override int NumeroDeAlvos => 2;
        public override TipoAlvo TipoAlvo => TipoAlvo.Aleatorio;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            foreach (Combate a in ResolverAlvos(alvo, lista))
                new Preso(turnos: 1).Aplicar(a);
            return SemDano();
        }
    }
}