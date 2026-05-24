using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Passiva permanente do Morcego: cura 15% do dano causado em qualquer ataque.
    /// Aplica Sedento via IPassivaInicial no IniciarCombate.
    /// </summary>
    class PassivaMorcego : HabilidadePassiva, IPassivaInicial
    {
        public PassivaMorcego() : base("Sedento de Sangue", "🦇", 0,
            "Cura 15% do dano causado ao atacar.")
        { }

        public void AplicarInicial(Combate portador)
        {
            new Sedento(percentual: 0.15).Aplicar(portador);
        }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) => false;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}