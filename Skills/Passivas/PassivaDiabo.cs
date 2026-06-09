using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// A cada hit recebido, o Diabo ganha +5% de HP máximo permanente, até 25%
    /// acumulado (5 hits). HPAtual NÃO é alterado — só o máximo aumenta. Calculado
    /// sobre HPMaximoInicial (consistente com Queima/Maldição). Migrada para o
    /// modelo de reação (IReageAoSerAtacado). O ganho vai no PORTADOR (si mesmo).
    /// </summary>
    class PassivaDiabo : HabilidadePassiva, IReageAoSerAtacado
    {
        private const double GanhoPorHit = 0.05;
        private const double Cap = 0.25;

        private class Estado
        {
            public double TotalAumentado;
        }

        public PassivaDiabo() : base("Cresce com Dor", "😈", 0,
            "Ao ser atacado, +5% HP máximo (até 25%).")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            var estado = ObterEstado<Estado>(ctx.Portador);
            if (estado.TotalAumentado >= Cap) return new List<ResultadoReacao>();

            double aumentar = Math.Min(GanhoPorHit, Cap - estado.TotalAumentado);
            int delta = (int)(ctx.Portador.HPMaximoInicial * aumentar);

            ctx.Portador.ModificarHPMaximo(delta);
            estado.TotalAumentado += aumentar;

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"😈 Cresce com Dor aumentou o HP máximo de {ctx.Portador.Personagem.Nome}!"
                )
            };
        }
    }
}