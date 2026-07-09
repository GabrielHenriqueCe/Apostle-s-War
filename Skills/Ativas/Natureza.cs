using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Skills.Ativas
{
    /// <summary>
    /// Ataca 1 inimigo com +50% ATK e aplica Preso 1t.
    /// </summary>
    class Natureza : HabilidadeAtiva
    {
        public Natureza() : base("Natureza", "🌿", 3,
            "Ataca 1 inimigo com +50% ATK e aplica Preso por 1 turno.")
        { }

        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<EventoDano>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                resultados.Add(AplicarDano(ctx.Atacante, a, 1.5));
                new Preso(turnos: 1).Aplicar(a);
            }
            return resultados;
        }
    }
}