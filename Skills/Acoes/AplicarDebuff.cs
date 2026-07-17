using ApostlesWar.Skills;

namespace ApostlesWar
{
    /// <summary>
    /// Aplica um debuff no alvo. Recebe uma FÁBRICA (não uma instância) porque cada alvo
    /// precisa da sua própria instância de StatusEffect — ex: Queima com stacks/duração
    /// independentes por alvo. Reusar uma única instância entre alvos compartilharia estado.
    ///
    /// A sobrecarga Func&lt;Combate, Debuff&gt; é o gêmeo da do AplicarBuff — pra debuffs com
    /// PROVENIÊNCIA (carregam quem aplicou; ex: Irritar do Quebrar, que força A1 no aplicador).
    /// O parâmetro `chance` (default 1.0) aplica só com probabilidade — 1º cliente: Pancada
    /// (Medo 50%). chance=1.0 aplica sempre (caminho comum, sem sortear).
    /// </summary>
    class AplicarDebuff : Acao
    {
        private readonly Func<Combate, Debuff> _fabrica;
        private readonly double _chance;

        public AplicarDebuff(Func<Debuff> fabrica, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos, double chance = 1.0)
            : this(_ => fabrica(), escopo, estadoAlvo, chance) { }

        public AplicarDebuff(Func<Combate, Debuff> fabricaComAtacante, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos, double chance = 1.0)
            : base(escopo, estadoAlvo)
        {
            _fabrica = fabricaComAtacante;
            _chance = chance;
        }

        public override void Executar(Combate atacante, Combate alvo, List<EventoDano> eventos)
        {
            if (_chance < 1.0 && Random.Shared.NextDouble() >= _chance) return;
            _fabrica(atacante).Aplicar(alvo);
        }
    }
}
