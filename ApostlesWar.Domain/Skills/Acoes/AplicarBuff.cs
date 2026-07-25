using ApostlesWar.Domain.Skills;

namespace ApostlesWar.Domain
{
    /// <summary>
    /// Aplica um buff no alvo. Espelho do AplicarDebuff — recebe uma FÁBRICA (não instância)
    /// porque cada combatente precisa da sua própria instância de status.
    ///
    /// Só serve pra buffs de valor FIXO (Provocar, Intocável, BloqueioTotal...). Buffs com valor
    /// DERIVADO (Escudo = % do HP do alvo) são operações próprias que leem um fragmento de Valor
    /// — ver ADR-composicao-de-acoes §5.5.
    ///
    /// A sobrecarga Func&lt;Combate, Buff&gt; existe pra buffs com PROVENIÊNCIA (carregam quem
    /// aplicou — ProtecaoAliado.Aplicador, Irritar.Aplicador, ver ROADMAP "Proveniência de
    /// status"). 1º cliente: OssoDuroDeRoer (ProtecaoAliado).
    /// </summary>
    public class AplicarBuff : Acao
    {
        private readonly Func<Combate, Buff> _fabrica;

        public AplicarBuff(Func<Buff> fabrica, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : this(_ => fabrica(), escopo, estadoAlvo) { }

        public AplicarBuff(Func<Combate, Buff> fabricaComAtacante, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : base(escopo, estadoAlvo) => _fabrica = fabricaComAtacante;

        public override Utilidade Utilidade => Utilidade.Reforcar;

        /// <summary>
        /// Se TODOS os alvos bloqueiam este buff, a habilidade não faz nada neles. Constrói um buff
        /// de sonda e pergunta ao próprio alvo (<see cref="Combate.PodeReceber"/>) — a mesma porta que
        /// o Aplicar usa, então a resposta não diverge. A sonda é descartada.
        /// </summary>
        public override bool TemEfeitoUtil(Combate atacante, IReadOnlyList<Combate> alvos)
            => alvos.Any(a => a.PodeReceber(_fabrica(atacante)));

        public override void Executar(Combate atacante, Combate alvo, List<EventoCombate> eventos)
            => _fabrica(atacante).Aplicar(alvo);
    }
}
