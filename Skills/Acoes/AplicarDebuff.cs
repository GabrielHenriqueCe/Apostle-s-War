using v1_Apostle_s_War.Skills;

namespace ApostlesWar
{
    /// <summary>
    /// Aplica um debuff no alvo. Recebe uma FÁBRICA (não uma instância) porque cada alvo
    /// precisa da sua própria instância de StatusEffect — ex: Queima com stacks/duração
    /// independentes por alvo. Reusar uma única instância entre alvos compartilharia estado.
    /// </summary>
    class AplicarDebuff : Acao
    {
        private readonly Func<Debuff> _fabrica;

        public AplicarDebuff(Func<Debuff> fabrica, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : base(escopo, estadoAlvo) => _fabrica = fabrica;

        public override void Executar(Combate atacante, Combate alvo, List<EventoDano> eventos)
            => _fabrica().Aplicar(alvo);
    }
}
