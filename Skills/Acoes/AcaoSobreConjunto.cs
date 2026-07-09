namespace ApostlesWar
{
    /// <summary>
    /// 2º formato de Ação — pra AGREGAÇÃO CROSS-ALVO, quando o cálculo atravessa todos os
    /// combatentes do conjunto de uma vez (ex: a média da Putrefação — soma ÷ contador). Uma
    /// Acao per-alvo não enxerga o conjunto todo, e guardar acumulador na instância seria
    /// frágil (ela é reusada entre ativações). É "a única parede real" do motor
    /// (ADR-composicao-de-acoes §3.4) — o resto (escopo próprio, condição de estado) se
    /// dissolve no loop-flip comum.
    ///
    /// O interpretador (HabilidadeAtiva.Ativar) despacha por tipo: se a Ação é uma
    /// AcaoSobreConjunto, chama este Executar UMA vez com o conjunto inteiro (já resolvido por
    /// Escopo + filtrado por EstadoAlvo); senão, chama o Executar per-alvo de Acao, uma vez por
    /// combatente. O Executar per-alvo herdado de Acao nunca é chamado numa AcaoSobreConjunto —
    /// fica selado e lança, pra deixar o estado ilegal irrepresentável.
    /// </summary>
    abstract class AcaoSobreConjunto : Acao
    {
        protected AcaoSobreConjunto(Escopo escopo, EstadoAlvo estadoAlvo) : base(escopo, estadoAlvo) { }

        public sealed override void Executar(Combate atacante, Combate alvo, List<EventoDano> eventos)
            => throw new NotSupportedException(
                "AcaoSobreConjunto usa Executar(conjunto) — o interpretador despacha por tipo, isso nunca deveria ser chamado.");

        public abstract void Executar(Combate atacante, IReadOnlyList<Combate> conjunto, List<EventoDano> eventos);
    }
}
