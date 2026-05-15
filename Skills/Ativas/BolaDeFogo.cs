using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;
using v1_Apostle_s_War.Skills.Passivas;

namespace v1_Apostle_s_War.Skills.Ativas
{
    class BolaDeFogo : HabilidadeAtiva
    {
        public BolaDeFogo() : base("Bola de Fogo", "🔥", 4,
            "Causa +100% ATK em 1 inimigo e aplica Queima (2t).")
        { }
        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            var resultados = new List<ResultadoAtaque>();
            foreach (Combate a in ResolverAlvos(alvo, lista))
            {
                double mult = 2.0 * PassivaPiromancer.MultExtra(atacante, a);
                resultados.Add(AplicarDano(atacante, a, mult));
                AplicarDebuff(a, new Queima(turnos: 2));
            }
            return resultados;
        }
    }
}