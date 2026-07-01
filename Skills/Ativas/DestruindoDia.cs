using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Remove todos os debuffs dos aliados e ataca todos os inimigos com +100% ATK.
    /// </summary>
    class DestruindoDia : HabilidadeAtiva
    {
        public DestruindoDia() : base("Destruindo o Dia", "🦹", 3,
            "Limpa maleficios dos aliados e ataca todos +100% ATK.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override TipoAtaque TipoAtaque => TipoAtaque.AreaDeEfeito;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            // Limpa debuffs dos aliados vivos
            foreach (Combate a in ctx.Aliados.Where(c => c.EstaVivo()))
            {
                var debuffs = a.StatusAtivos.OfType<Debuff>().ToList();
                foreach (var d in debuffs)
                    d.Remover(a);
            }

            // Ataca todos os inimigos
            var resultados = new List<EventoDano>();
            foreach (Combate i in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                resultados.Add(AplicarDano(ctx.Atacante, i, 2.0));

            return resultados;
        }
    }
}