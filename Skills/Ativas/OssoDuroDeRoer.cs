using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Aplica ProtecaoAliado (30%) em todos os aliados exceto a Caveira por 2 turnos,
    /// depois aplica Escudo (30% HP máximo) na Caveira por 2 turnos.
    /// </summary>
    class OssoDuroDeRoer : HabilidadeAtiva
    {
        public OssoDuroDeRoer() : base("Osso Duro de Roer", "🦴", 3,
            "Protege aliados (30% do dano vai pra Caveira) e ganha Escudo de 30% HP.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                if (a == ctx.Atacante) continue;
                new ProtecaoAliado(ctx.Atacante, turnos: 2, percentual: 0.30).Aplicar(a);
            }

            int pontos = (int)(ctx.Atacante.HPMaximo * 0.30);
            new Escudo(pontos, turnos: 2).Aplicar(ctx.Atacante);

            return SemDano();
        }
    }
}