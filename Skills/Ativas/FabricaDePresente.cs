using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Aplica ReducaoDefesa 2t em todos os inimigos e em seguida ataca todos com +75% ATK.
    /// Como a redução é aplicada ANTES do ataque, o próprio dano já se beneficia.
    /// </summary>
    class FabricaDePresente : HabilidadeAtiva
    {
        private const double MultiplicadorAtaque = 1.75;

        public FabricaDePresente() : base("Fábrica de Presente", "🏭", 3,
            "Reduz DEF dos inimigos e ataca todos +75% ATK.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override TipoAtaque TipoAtaque => TipoAtaque.AreaDeEfeito;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            // ReducaoDefesa primeiro pra potenciar o ataque
            foreach (Combate i in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                new ReducaoDefesa(turnos: 2).Aplicar(i);

            var resultados = new List<EventoDano>();
            foreach (Combate i in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                resultados.Add(AplicarDano(ctx.Atacante, i, MultiplicadorAtaque));

            return resultados;
        }
    }
}