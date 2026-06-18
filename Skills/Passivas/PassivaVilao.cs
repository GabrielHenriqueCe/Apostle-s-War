using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao matar um inimigo, bloqueia a ressurreição dele (BloquearRevive). Migrada
    /// para IReageAoMatar. Roda antes do "ao morrer" — bloqueia antes da tentativa
    /// de reviver (Necromancia/Guarda). AnjoCaido (Diabo) ignora proposital.
    /// </summary>
    class PassivaVilao : HabilidadePassiva, IReageAoMatar
    {
        public PassivaVilao() : base("Sentença", "🦹", 0,
            "Inimigos mortos por ele não podem ser ressuscitados.")
        { }

        public List<ResultadoReacao> AoMatar(ContextoReacao ctx)
        {
            ctx.Outro.BloquearRevive();

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(Mensagem: $"🦹 Sentença: {ctx.Outro.Personagem.Nome} não poderá ressuscitar!")
            };
        }
    }
}