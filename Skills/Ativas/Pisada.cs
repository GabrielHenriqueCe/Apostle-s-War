using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Aplica ContraAtaque 2t em si mesmo e ataca todos os inimigos com +125% ATK.
    /// </summary>
    class Pisada : HabilidadeAtiva
    {
        public Pisada() : base("Pisada", "🦶", 3,
            "Contra-ataque em si (2t) e ataca todos +125% ATK.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override TipoAtaque TipoAtaque => TipoAtaque.AreaDeEfeito;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            new ContraAtaque(turnos: 2).Aplicar(ctx.Atacante);

            var resultados = new List<EventoDano>();
            foreach (Combate i in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                resultados.Add(AplicarDano(ctx.Atacante, i, 2.25));

            return resultados;
        }
    }
}