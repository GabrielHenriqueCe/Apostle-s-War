using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Aplica ProtecaoAliado 30% e BuffDefesa 30% em todos os aliados 2t.
    /// ProtecaoAliado é aplicada nos demais (não em si mesmo).
    /// </summary>
    class SalvandoDia : HabilidadeAtiva
    {
        public SalvandoDia() : base("Salvando o Dia", "🦸", 3,
            "Protege e +30% DEF em todos os aliados (2t).")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (Combate a in ObterListaPrincipal(ctx).Where(c => c.EstaVivo()))
            {
                new BuffDefesa(turnos: 2, percentual: 0.30).Aplicar(a);

                if (a != ctx.Atacante)
                    new ProtecaoAliado(ctx.Atacante, turnos: 2, percentual: 0.30).Aplicar(a);
            }
            return SemDano();
        }
    }
}