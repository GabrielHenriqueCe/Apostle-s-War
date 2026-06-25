using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Revive todos os aliados mortos com 50% HP (respeita MortePermanente).
    /// Aplica BuffAtaque 25% 2t e BloqueioTotal 2t em todos os aliados vivos
    /// (incluindo recém-revividos).
    /// </summary>
    class Ceu : HabilidadeAtiva
    {
        private const double HPRevivido = 0.50;

        public Ceu() : base("Céu", "☁️", 4,
            "Revive aliados (50% HP), +25% ATK e Bloqueio Total (2t).")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            // Revive aliados mortos (respeita MortePermanente)
            foreach (Combate a in ObterListaPrincipal(ctx))
            {
                if (!a.EstaVivo())
                    a.Reviver((int)(a.HPMaximo * HPRevivido));
            }

            // Aplica buffs em todos os aliados vivos
            foreach (Combate a in ObterListaPrincipal(ctx).Where(c => c.EstaVivo()))
            {
                new BuffAtaque(turnos: 2, percentual: 0.25).Aplicar(a);
                new BloqueioTotal(turnos: 2).Aplicar(a);
            }

            return SemDano();
        }
    }
}