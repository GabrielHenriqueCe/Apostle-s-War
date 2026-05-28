using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Rouba todos os buffs do inimigo escolhido (migra as instâncias) e concede turno extra.
    /// 
    /// Importante: a instância de cada Buff é movida do StatusAtivos do inimigo pro do Mímico.
    /// Buffs "transparentes" (Escudo, ProtecaoAliado, ContraAtaque, etc) funcionam imediatamente.
    /// Buffs que modificaram stat direto (BuffAtaque, BuffDefesa, BuffTaxaCrit) já alteraram
    /// o stat do inimigo e não vão alterar o do Mímico — esse é o trade-off aceito.
    /// </summary>
    class Copiando : HabilidadeAtiva
    {
        public Copiando() : base("Copiando", "📋", 4,
            "Rouba todos os buffs de um inimigo e ganha turno extra.")
        { }

        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                var buffs = a.StatusAtivos.OfType<Buff>().ToList();
                foreach (var buff in buffs)
                {
                    a.StatusAtivos.Remove(buff);
                    ctx.Atacante.StatusAtivos.Add(buff);
                }
            }

            ctx.Atacante.ConcederTurnoExtra();
            return SemDano();
        }
    }
}