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

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<EventoDano>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                double mult = 2.0 * PassivaPiromancer.MultExtra(ctx.Atacante, a);
                resultados.Add(AplicarDano(ctx.Atacante, a, mult));
                AplicarDebuff(a, new Queima(2));
            }
            return resultados;
        }
    }
}