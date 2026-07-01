using ApostlesWar;
using v1_Apostle_s_War.Skills.Passivas;

namespace v1_Apostle_s_War.Skills.Ativas
{
    class Incendio : HabilidadeAtiva
    {
        public Incendio() : base("Incêndio", "🌋", 4,
            "Ataca todos os inimigos com +50% ATK.")
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
                double mult = 1.5 * PassivaPiromancer.MultExtra(ctx.Atacante, a);
                resultados.Add(AplicarDano(ctx.Atacante, a, mult));
            }
            return resultados;
        }
    }
}