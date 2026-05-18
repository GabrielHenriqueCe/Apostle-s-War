using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Aplica ImunidadeDebuffs permanente no início do combate.
    /// O bloqueio em si é feito pelo status (Bloqueia(novo) retorna true para Debuff).
    /// </summary>
    class PassivaAbobora : HabilidadePassiva, IPassivaInicial
    {
        public PassivaAbobora() : base("Casca Dura", "🎃", 0,
            "Imune a maleficios.")
        { }

        public void AplicarInicial(Combate portador)
        {
            new ImunidadeDebuffs().Aplicar(portador);
        }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) => false;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
            => SemDano();

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}
