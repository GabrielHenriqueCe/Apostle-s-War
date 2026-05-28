using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Cura todos os aliados em 15% HP máximo.
    /// Aumenta a duração de TODOS os buffs ativos de cada aliado em 1 turno.
    /// </summary>
    class RaioX : HabilidadeAtiva
    {
        public RaioX() : base("Raio-X", "🩻", 4,
            "Cura 15% HP e estende em 1t os benefícios de todos os aliados.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                AplicarCura(a, 0.15);
                foreach (var buff in a.StatusAtivos.OfType<Buff>())
                    buff.AumentarDuracao(1);
            }
            return SemDano();
        }
    }
}