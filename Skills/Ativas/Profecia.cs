using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Aplica ReducaoDefesa 30% (2t) em todos os inimigos e ataca todos com +50% ATK.
    /// </summary>
    class Profecia : HabilidadeAtiva
    {
        public Profecia() : base("Profecia", "🔮", 3,
            "-30% DEF em todos os inimigos (2t) e ataca todos com +50% ATK.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var alvos = ResolverAlvos(alvo, ObterListaPrincipal(ctx));

            foreach (Combate a in alvos)
                new ReducaoDefesa(turnos: 2).Aplicar(a);

            var resultados = new List<ResultadoAtaque>();
            foreach (Combate a in alvos)
                resultados.Add(AplicarDano(ctx.Atacante, a, 1.5));

            return resultados;
        }
    }
}