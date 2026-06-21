using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// +25% ATK em si (2t), -30% DEF no alvo (2t), ataca com +50% ATK.
    /// </summary>
    class Glitch : HabilidadeAtiva
    {
        public Glitch() : base("Glitch", "📺", 3,
            "+25% ATK em si, -30% DEF no alvo, ataca com +50% ATK.")
        { }

        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            new BuffAtaque(turnos: 2, percentual: 0.25).Aplicar(ctx.Atacante);

            var resultados = new List<EventoDano>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                new ReducaoDefesa(turnos: 2).Aplicar(a);
                resultados.Add(AplicarDano(ctx.Atacante, a, 1.5));
            }
            return resultados;
        }
    }
}