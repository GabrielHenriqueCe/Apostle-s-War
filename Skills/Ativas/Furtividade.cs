using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;
namespace v1_Apostle_s_War.Skills.Ativas
{
    class Furtividade : HabilidadeAtiva
    {
        public Furtividade() : base("Furtividade", "🕳️", 4,
            "Intocável por 2 turnos. Ataca todos os inimigos com 100% ATK.")
        { }
        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override TipoAtaque TipoAtaque => TipoAtaque.AreaDeEfeito;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            AplicarBuff(ctx.Atacante, new Intocavel(turnos: 2));

            var resultados = new List<EventoDano>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                resultados.Add(AplicarDano(ctx.Atacante, a, 1.0));
            return resultados;
        }
    }
}