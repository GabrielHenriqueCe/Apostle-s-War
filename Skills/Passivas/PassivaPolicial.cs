using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Atacar um inimigo Preso adiciona +1 turno ao debuff. Migrada para IReagePorAtaque
    /// (por alvo atingido). Só declara mensagem se o alvo tinha Preso.
    /// </summary>
    class PassivaPolicial : HabilidadePassiva, IReagePorAtaque
    {
        public PassivaPolicial() : base("Algemas Reforçadas", "🔗", 0,
            "Atacar um inimigo Preso adiciona +1 turno ao debuff.")
        { }

        public List<ResultadoReacao> PorAtaque(ContextoReacao ctx)
        {
            var preso = ctx.Contraparte.StatusAtivos.OfType<Preso>().FirstOrDefault();
            if (preso == null) return new List<ResultadoReacao>();

            preso.EstenderTurno();

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(Mensagem: $"🔗 Algemas Reforçadas prolongaram o aprisionamento de {ctx.Contraparte.Personagem.Nome}!")
            };
        }
    }
}