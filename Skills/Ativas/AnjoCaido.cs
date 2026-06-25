using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Revive todos os aliados mortos com 50% HP e cura todos os aliados em 30% HP.
    /// QUEBRA a Sentença (remove ImpedirRessurreicao do morto) antes de reviver —
    /// "traz direto do inferno". Remove só esse debuff, não outros.
    /// </summary>
    class AnjoCaido : HabilidadeAtiva
    {
        private const double HPRevivido = 0.50;
        private const double CuraPercentual = 0.30;

        public AnjoCaido() : base("Anjo Caído", "😇", 3,
            "Revive aliados (50% HP, quebra a Sentença) e cura todos (30% HP).")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (Combate a in ObterListaPrincipal(ctx).Where(a => !a.EstaVivo()))
            {
                // Quebra a Sentença (só ela, não outros debuffs do morto), depois revive.
                foreach (var sentenca in a.StatusAtivos.OfType<ImpedirRessurreicao>().ToList())
                    sentenca.Remover(a);

                a.Reviver((int)(a.HPMaximo * HPRevivido));
            }

            // Cura todos os aliados vivos (incluindo revividos)
            foreach (Combate a in ObterListaPrincipal(ctx).Where(c => c.EstaVivo()))
                AplicarCura(a, CuraPercentual);

            return SemDano();
        }
    }
}