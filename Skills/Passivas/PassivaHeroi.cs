using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Passiva permanente do Herói: começa o combate com ContraAtaque ativo.
    /// Aplicado via IPassivaInicial no IniciarCombate.
    /// </summary>
    class PassivaHeroi : HabilidadePassiva, IPassivaInicial
    {
        public PassivaHeroi() : base("Vigilante", "🦸", 0,
            "Sempre tem Contra-Ataque ativo.")
        { }

        public void AplicarInicial(Combate portador)
        {
            new ContraAtaque(turnos: int.MaxValue).Aplicar(portador);
        }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) => false;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}