using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca todos os inimigos. Multiplicador escala de 1.0 (HP cheio) até 2.0 (1 HP).
    /// </summary>
    class Ossinho : HabilidadeAtiva
    {
        public Ossinho() : base("Ossinho", "🦴", 3,
            "Ataca todos os inimigos. Dano aumenta conforme o HP da Caveira diminui (até 2x).")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override TipoAtaque TipoAtaque => TipoAtaque.AreaDeEfeito;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            double percentualPerdido = 1.0 - ((double)ctx.Atacante.HPAtual / ctx.Atacante.HPMaximo);
            double mult = 1.0 + percentualPerdido;

            var resultados = new List<ResultadoAtaque>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                resultados.Add(AplicarDano(ctx.Atacante, a, mult));
            return resultados;
        }
    }
}