using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Revive todos os aliados mortos com 50% HP e cura todos os aliados em 30% HP.
    /// IGNORA bloqueio de ressurreição (MortePermanente) — traz direto do inferno.
    /// </summary>
    class AnjoCaido : HabilidadeAtiva
    {
        private const double HPRevivido = 0.50;
        private const double CuraPercentual = 0.30;

        public AnjoCaido() : base("Anjo Caído", "😇", 3,
            "Revive aliados (50% HP, ignora bloqueio) e cura todos (30% HP).")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            // Revive todos os mortos, IGNORANDO MortePermanente (proposital).
            foreach (Combate a in ObterListaPrincipal(ctx))
            {
                if (!a.EstaVivo())
                    a.Reviver((int)(a.HPMaximo * HPRevivido));
            }

            // Cura todos os aliados vivos (incluindo revividos)
            foreach (Combate a in ObterListaPrincipal(ctx).Where(c => c.EstaVivo()))
                AplicarCura(a, CuraPercentual);

            return SemDano();
        }
    }
}