using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Passiva permanente da Sereia: recebe -15% de dano durante todo o combate.
    /// Aplica ReducaoDanoFixo via IPassivaInicial no IniciarCombate.
    /// </summary>
    class PassivaSereia : HabilidadePassiva, IPassivaInicial
    {
        public PassivaSereia() : base("Aquagirl", "🧜‍♀️", 0,
            "Recebe 15% menos dano.")
        { }

        public void AplicarInicial(Combate portador)
        {
            new ReducaoDanoFixo(percentual: 0.15).Aplicar(portador);
        }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) => false;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}