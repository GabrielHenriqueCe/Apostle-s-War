using System.Linq;

namespace ApostlesWar
{
    /// <summary>
    /// Fragmento de valor de uma ação com magnitude (Cura, Escudo, ...). A UNIDADE de reúso é o
    /// FRAGMENTO, não a Ação: Cura e Escudo compartilham a fonte-do-valor e diferem só no verbo
    /// final (ver ADR-composicao-de-acoes §5.5). A diversidade de fonte é mantida de propósito,
    /// sem forçar um padrão único.
    /// </summary>
    delegate int ValorFn(Combate atacante, Combate alvo, List<EventoDano> eventos);

    static class Valor
    {
        /// <summary>Valor fixo em pontos.</summary>
        public static ValorFn Fixo(int pontos) => (atk, alvo, ev) => pontos;

        /// <summary>Percentual do HP máximo do ALVO (ex: cura/escudo de X% do HP do alvo).</summary>
        public static ValorFn PorHP(double percentual) => (atk, alvo, ev) => (int)(alvo.HPMaximo * percentual);

        /// <summary>Percentual do HP máximo do ATACANTE (ex: escudo baseado no próprio HP).</summary>
        public static ValorFn PorHPDoAtacante(double percentual) => (atk, alvo, ev) => (int)(atk.HPMaximo * percentual);

        /// <summary>Percentual do dano TOTAL já causado pela habilidade (agregado no eventos).</summary>
        public static ValorFn PorDanoCausado(double percentual) => (atk, alvo, ev) => (int)(ev.Sum(e => e.DanoEfetivo) * percentual);
    }
}
