using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Skills.Ativas
{
    /// <summary>
    /// Ataca todos os inimigos com 100% ATK e aplica Veneno 2 stacks (= 2 turnos de duração).
    /// </summary>
    class Quimica : HabilidadeAtiva
    {
        public Quimica() : base("Química", "🧪", 3,
            "Ataca todos e aplica Veneno (5% HP/turno por stack).")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override TipoAtaque TipoAtaque => TipoAtaque.AreaDeEfeito;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<EventoDano>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                resultados.Add(AplicarDano(ctx.Atacante, a, 1.0));
                new Veneno(stacks: 2).Aplicar(a);
            }
            return resultados;
        }
    }
}