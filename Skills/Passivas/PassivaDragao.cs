using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Passiva permanente do Dragão: imune a Veneno e Queima durante todo o combate.
    /// Aplica ImunidadeEspecifica via IPassivaInicial no IniciarCombate.
    /// </summary>
    class PassivaDragao : HabilidadePassiva, IPassivaInicial
    {
        public PassivaDragao() : base("Pele de Dragão", "🐉", 0,
            "Imune a Veneno e Queima.")
        { }

        public void AplicarInicial(Combate portador)
        {
            new ImunidadeEspecifica(typeof(Veneno), typeof(Queima)).Aplicar(portador);
        }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) => false;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}