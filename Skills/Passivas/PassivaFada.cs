using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Quando a Fada mata um inimigo, aplica BloqueioTotal 3t em si mesma.
    /// </summary>
    class PassivaFada : HabilidadePassiva
    {
        public PassivaFada() : base("Voar", "🧚", 0,
            "Após matar, fica imune a dano por 3 turnos.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeMatar && ctx.AlvoVivo;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            new BloqueioTotal(turnos: 3).Aplicar(ctx.Atacante);
            return SemDano();
        }

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}