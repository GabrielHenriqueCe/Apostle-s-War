using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca todos os inimigos com +100% ATK e aplica Queima (2 stacks).
    /// </summary>
    class SoproDoDragao : HabilidadeAtiva
    {
        public SoproDoDragao() : base("Sopro do Dragão", "🔥", 3,
            "Ataca todos com +100% ATK e aplica Queima (2 stacks).")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<ResultadoAtaque>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                resultados.Add(AplicarDano(ctx.Atacante, a, 2.0));
                new Queima(stacks: 2).Aplicar(a);
            }
            return resultados;
        }
    }
}