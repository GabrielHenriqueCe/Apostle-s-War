using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Aplica Invencivel 2t em todos os aliados e ataca 1 inimigo com crítico forçado.
    /// </summary>
    class CintoUtilidades : HabilidadeAtiva
    {
        public CintoUtilidades() : base("Cinto de Utilidades", "🦸", 4,
            "Invencível em todos os aliados (2t) e ataque crítico em 1 inimigo.")
        { }

        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            // Invencível em todos os aliados
            foreach (Combate a in ctx.Aliados.Where(c => c.EstaVivo()))
                new Invencivel(turnos: 2).Aplicar(a);

            // Ataque crítico forçado no inimigo
            var resultados = new List<EventoDano>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                var r = ctx.Atacante.Atacar(
                    a,
                    multiplicador: 1.0,
                    forcaCritico: true);
                resultados.Add(r);
            }
            return resultados;
        }
    }
}