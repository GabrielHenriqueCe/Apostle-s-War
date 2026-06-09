using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao ser atacado, reduz em 1 turno a duração de TODOS os buffs do atacante
    /// (removendo os que expiram). Migrada para o modelo de reação
    /// (IReageAoSerAtacado). Só declara mensagem se havia buff para reduzir.
    /// </summary>
    class PassivaCientista : HabilidadePassiva, IReageAoSerAtacado
    {
        public PassivaCientista() : base("Análise Crítica", "🔬", 0,
            "Ao ser atacado, reduz em 1t a duração dos benefícios do atacante.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Outro.EstaVivo())
                return new List<ResultadoReacao>();

            var buffs = ctx.Outro.StatusAtivos.OfType<Buff>().ToList();
            if (buffs.Count == 0)
                return new List<ResultadoReacao>();

            foreach (var buff in buffs)
            {
                buff.ReduzirDuracao(1);
                if (buff.Expirou)
                    buff.Remover(ctx.Outro);
            }

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"🔬 Análise Crítica encurtou os benefícios de {ctx.Outro.Personagem.Nome}!"
                )
            };
        }
    }
}