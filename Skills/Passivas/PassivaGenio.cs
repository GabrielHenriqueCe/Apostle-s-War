using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Todo início de turno, o Gênio aplica RefletirDano 2t em si mesmo.
    /// Como buffs não acumulam mas substituem se nova duração for maior, na prática
    /// o RefletirDano fica sempre renovado em 2t (perde 1 ao passar o turno → vira 1t →
    /// próxima aplicação substitui pra 2t). Se ImpedirBeneficios estiver ativo, falha
    /// silenciosamente (igual qualquer outra aplicação de buff).
    /// </summary>
    class PassivaGenio : HabilidadePassiva
    {
        public PassivaGenio() : base("Realidade", "🔮", 0,
            "Todo turno aplica Refletir Dano em si mesmo.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.InicioDoTurno && ctx.AlvoVivo;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            new RefletirDano(turnos: 2).Aplicar(ctx.Atacante);
            return SemDano();
        }

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}