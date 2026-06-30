using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao ser atacado, escolhe 1 debuff aleatório do atacante e aumenta sua
    /// duração em 2 turnos. Migrada para o modelo de reação (IReageAoSerAtacado).
    /// Simétrica à PassivaCientista (que reduz buffs do atacante).
    /// </summary>
    class PassivaMimico : HabilidadePassiva, IReageAoSerAtacado
    {
        private const int AumentoDuracao = 2;
        private static readonly Random _random = new Random();

        public PassivaMimico() : base("Repetindo", "🎭", 0,
            "Ao ser atacado, aumenta a duração de 1 debuff aleatório do atacante em 2 turnos.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Contraparte.EstaVivo())
                return new List<ResultadoReacao>();

            var debuffs = ctx.Contraparte.StatusAtivos.OfType<Debuff>().ToList();
            if (debuffs.Count == 0)
                return new List<ResultadoReacao>();

            var escolhido = debuffs[_random.Next(debuffs.Count)];
            escolhido.AumentarDuracao(AumentoDuracao);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"🎭 Repetindo prolongou o {escolhido.Nome} de {ctx.Contraparte.Personagem.Nome}!"
                )
            };
        }
    }
}