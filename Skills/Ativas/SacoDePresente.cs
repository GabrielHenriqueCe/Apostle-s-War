using ApostlesWar;
using ApostlesWar.Skills.Buffs;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Skills.Ativas
{
    /// <summary>
    /// Ataca todos os inimigos com +75% ATK, aplica Medo 1t em cada
    /// e BuffAtaque 25% 2t em todos os aliados.
    /// </summary>
    class SacoDePresente : HabilidadeAtiva
    {
        private const double MultiplicadorAtaque = 1.75;

        public SacoDePresente() : base("Saco de Presente", "🎅", 3,
            "Ataca todos +75% ATK + Medo. +25% ATK aliados (2t).")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override TipoAtaque TipoAtaque => TipoAtaque.AreaDeEfeito;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            // Buff nos aliados primeiro pra potenciar o ataque... NÃO, melhor depois:
            // o multiplicador é estatístico, ATK do Papai Noel é capturado no AplicarDano.
            // Mas pra outros aliados sentirem benefício no próximo turno, OK.
            foreach (Combate a in ctx.Aliados.Where(c => c.EstaVivo()))
                new BuffAtaque(turnos: 2, percentual: 0.25).Aplicar(a);

            var resultados = new List<EventoDano>();
            foreach (Combate i in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                resultados.Add(AplicarDano(ctx.Atacante, i, MultiplicadorAtaque));
                if (i.EstaVivo())
                    new Medo(turnos: 1).Aplicar(i);
            }

            return resultados;
        }
    }
}