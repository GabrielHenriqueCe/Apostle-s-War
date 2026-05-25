using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca todos os inimigos. O multiplicador aumenta em +25% ATK por buff
    /// ativo no Mímico, com cap em 4 buffs (+100% ATK no máximo).
    /// </summary>
    class Imitacao : HabilidadeAtiva
    {
        private const double MultiplicadorBase = 1.0;
        private const double BonusPorBuff = 0.25;
        private const int CapBuffs = 4;

        public Imitacao() : base("Imitação", "🎭", 3,
            "Ataca todos. +25% ATK por buff ativo (cap 4).")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            int buffsAtivos = ctx.Atacante.StatusAtivos.OfType<Buff>().Count();
            int buffsContados = Math.Min(buffsAtivos, CapBuffs);
            double multiplicador = MultiplicadorBase + (BonusPorBuff * buffsContados);

            var resultados = new List<ResultadoAtaque>();
            foreach (Combate i in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                resultados.Add(AplicarDano(ctx.Atacante, i, multiplicador));

            return resultados;
        }
    }
}