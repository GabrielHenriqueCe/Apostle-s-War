using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Quando o Vilão mata um inimigo, aplica MortePermanente no morto.
    /// Habilidades que respeitam TemBloqueioRessurreicao não conseguem reviver.
    /// (AnjoCaido do Diabo ignora propositalmente.)
    /// </summary>
    class PassivaVilao : HabilidadePassiva
    {
        public PassivaVilao() : base("Sentença", "🦹", 0,
            "Inimigos mortos por ele não podem ser ressuscitados.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeMatar;

        // ctx.Atacante = Vilão (portador); alvo = inimigo que ele matou
        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            new MortePermanente().Aplicar(alvo);
            return SemDano();
        }
    }
}