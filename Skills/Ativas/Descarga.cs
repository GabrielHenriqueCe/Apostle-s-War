using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Skills.Ativas
{
    /// <summary>
    /// Ataca 1 inimigo com +150% ATK e aplica 5 stacks de Veneno.
    /// </summary>
    class Descarga : HabilidadeAtiva
    {
        public Descarga() : base("Descarga", "🚽", 3,
            "+150% ATK e aplica 5 stacks de Veneno.")
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
                resultados.Add(AplicarDano(ctx.Atacante, a, 2.5));
                if (a.EstaVivo())
                    new Veneno(stacks: 5).Aplicar(a);
            }
            return resultados;
        }
    }
}