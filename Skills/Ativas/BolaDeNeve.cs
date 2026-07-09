using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Skills.Ativas
{
    /// <summary>
    /// Ataca 1 inimigo com +175% ATK e aplica Preso 1t.
    /// </summary>
    class BolaDeNeve : HabilidadeAtiva
    {
        public BolaDeNeve() : base("Bola de Neve", "⛄", 3,
            "+175% ATK e Preso 1t no alvo.")
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
                resultados.Add(AplicarDano(ctx.Atacante, a, 2.75));
                if (a.EstaVivo())
                    new Preso(turnos: 1).Aplicar(a);
            }
            return resultados;
        }
    }
}