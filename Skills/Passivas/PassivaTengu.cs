using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// O Tengu começa o combate com BuffAtaque 25% por 2 turnos.
    /// Aplicado via IPassivaInicial no IniciarCombate.
    /// </summary>
    class PassivaTengu : HabilidadePassiva, IPassivaInicial
    {
        public PassivaTengu() : base("Ventania", "👺", 0,
            "Começa o combate com +25% ATK por 2 turnos.")
        { }

        public void AplicarInicial(Combate portador)
        {
            new BuffAtaque(turnos: 2, percentual: 0.25).Aplicar(portador);
        }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) => false;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}