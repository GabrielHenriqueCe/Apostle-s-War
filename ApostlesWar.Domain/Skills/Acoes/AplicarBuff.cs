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

        public override void Executar(Combate atacante, Combate alvo, List<EventoCombate> eventos)
            => _fabrica(atacante).Aplicar(alvo);
    }
}
