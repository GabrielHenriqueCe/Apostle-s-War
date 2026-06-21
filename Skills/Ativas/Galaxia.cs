using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// BuffDefesa 30% (2t) em todos os aliados. ProtecaoAliado 30% (2t) nos aliados exceto o Alien.
    /// </summary>
    class Galaxia : HabilidadeAtiva
    {
        public Galaxia() : base("Galáxia", "🌌", 4,
            "+30% DEF em todos. Outros aliados ficam protegidos pelo Alien.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                new BuffDefesa(turnos: 2, percentual: 0.30).Aplicar(a);
                if (a != ctx.Atacante)
                    new ProtecaoAliado(ctx.Atacante, turnos: 2, percentual: 0.30).Aplicar(a);
            }
            return SemDano();
        }
    }
}