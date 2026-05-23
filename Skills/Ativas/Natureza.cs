using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca 1 inimigo com +50% ATK e aplica Preso 1t.
    /// </summary>
    class Natureza : HabilidadeAtiva
    {
        public Natureza() : base("Natureza", "🌿", 3,
            "+50% ATK e Preso 1t no alvo.")
        { }

        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<ResultadoAtaque>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                resultados.Add(AplicarDano(ctx.Atacante, a, 1.5));
                new Preso(turnos: 1).Aplicar(a);
            }
            return resultados;
        }
    }
}