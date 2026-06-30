using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao matar um inimigo, aplica a Sentença (ImpedirRessurreicao) no morto — ele não
    /// pode ser ressuscitado. Reação (IReageAoMatar), roda antes do "ao morrer", então
    /// o debuff já está no morto quando a Necromancia/Guarda tentam reviver. O Diabo
    /// (AnjoCaido) remove a Sentença proposital.
    /// </summary>
    class PassivaVilao : HabilidadePassiva, IReageAoMatar
    {
        public PassivaVilao() : base("Sentença", "🦹", 0,
            "Inimigos mortos por ele não podem ser ressuscitados.")
        { }

        public List<ResultadoReacao> AoMatar(ContextoReacao ctx)
        {   
            new ImpedirRessurreicao().Aplicar(ctx.Contraparte);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(Mensagem: $"🦹 Sentença: {ctx.Contraparte.Personagem.Nome} não poderá ressuscitar!")
            };
        }
    }
}