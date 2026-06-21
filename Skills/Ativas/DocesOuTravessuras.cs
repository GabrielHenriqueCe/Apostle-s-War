using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Remove todos os buffs ativos dos inimigos e aplica ImpedirBeneficios (2t).
    /// </summary>
    class DocesOuTravessuras : HabilidadeAtiva
    {
        public DocesOuTravessuras() : base("Doces ou Travessuras", "🍬", 4,
            "Remove benefícios dos inimigos e bloqueia novos por 2 turnos.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                var buffs = a.StatusAtivos.OfType<Buff>().ToList();
                foreach (var b in buffs)
                    b.Remover(a);

                new ImpedirBeneficios(turnos: 2).Aplicar(a);
            }
            return SemDano();
        }
    }
}