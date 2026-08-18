using ApostlesWar.Domain.Skills;

namespace ApostlesWar.Domain
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
    public class AplicarDebuff : Acao
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

        public override Utilidade Utilidade => Utilidade.Enfraquecer;

        /// <summary>
        /// Se TODOS os alvos bloqueiam este debuff (Abóbora e afins), não há o que fazer neles.
        /// Mesma sonda do AplicarBuff, pela mesma porta <see cref="Combate.PodeReceber"/>.
        /// </summary>
        public override bool TemEfeitoUtil(Combate atacante, IReadOnlyList<Combate> alvos)
            => alvos.Any(a => a.PodeReceber(_fabrica(atacante)));

        /// <summary>
        /// DOIS PORTÕES, e eles são coisas diferentes de propósito (decisão do Gabriel):
        ///
        /// 1. <b>a chance da PRÓPRIA habilidade</b> (`_chance`) — o incremento diferencial que o
        ///    rebalanceamento usa pra segurar habilidade roubada, e o eixo por onde a RARIDADE vai
        ///    diferenciar kits. É identidade da habilidade e não depende de quem a usa;
        /// 2. <b>Precisão × Resistência</b> — a disputa entre quem aplica e quem apanha, que é a
        ///    mesma pra todo malefício do jogo.
        ///
        /// Elas MULTIPLICAM: o Medo de 50% num alvo que resiste metade cola em 25% das vezes.
        ///
        /// <b>Auto-malefício não rola o portão 2</b> — o que se impõe a si mesmo é escolha, não
        /// imposição. O portão 1 continua valendo: ele é identidade da habilidade, não disputa. Se um
        /// dia nascer um debuff aplicado em ALIADO, distingui-lo vai exigir a <c>Batalha</c> aqui
        /// dentro, que hoje esta ação não tem — e aí o portão 2 tem de ficar de fora dele também.
        /// </summary>
        public override double PreverChanceDeAplicar(Combate atacante, Combate alvo)
            => alvo == atacante ? _chance : _chance * atacante.ChanceDeColarEm(alvo);

        /// <summary>
        /// UM dado só, contra o número que a tela mostra. Rolar os dois portões separados dava a
        /// mesma probabilidade, mas deixava a conta do 🎲 sendo uma SEGUNDA cópia dela — e duas
        /// cópias de uma fórmula divergem como duas cópias de um número.
        /// </summary>
        public override void Executar(Combate atacante, Combate alvo, List<EventoCombate> eventos)
        {
            double chance = PreverChanceDeAplicar(atacante, alvo);
            if (chance < 1.0 && Random.Shared.NextDouble() >= chance) return;   // resistiu inteiro

            Debuff debuff = _fabrica(atacante);

            // O piso é 1 turno: já colou, então dura. Aparar um debuff de 1 turno o apagaria
            // pela porta dos fundos, e aí "colou" deixaria de querer dizer alguma coisa.
            if (alvo != atacante && debuff.DuracaoRestante > 1
                && Random.Shared.NextDouble() < atacante.ChanceDeAparaUmTurnoEm(alvo))
                debuff.ReduzirDuracao(1);

            debuff.Aplicar(alvo);
        }
    }
}
