using ApostlesWar.Skills;

namespace ApostlesWar
{
    /// <summary>
    /// Revive combatentes mortos com um percentual do HP máximo. Família de 7 clientes
    /// (ADR-composicao-de-acoes §9): Nigiri, Tecnology, Céu, AnjoCaído, DocesDeAbobora,
    /// Circo, Atlantis.
    ///
    /// BUFF opcional NOS REVIVIDOS (`buffNoRevivido`): fábrica genérica `Func&lt;Buff&gt;` aplicada
    /// a cada alvo que REALMENTE voltou (checa `EstaVivo()` depois de reviver — quem tinha Sentença
    /// e não voltou NÃO pega). Qualquer buff serve (Intocável/Invencível/BloqueioTotal...). Clientes:
    /// Atlantis (Sereia), Circo (Palhaço). Isso DISSOLVE o "pipeline / AfetadosPelaAcaoAnterior"
    /// (ADR §8.1) — a Reviver já sabe quem reviveu, o motor não precisa rastrear conjunto afetado.
    /// NÃO confundir com buff em TODOS os aliados (Nigiri/Tecnology usam `AplicarBuff(TodosAliados)`
    /// separado); aqui é SÓ nos revividos. Se um dia precisar de proveniência no buff, nasce o
    /// overload `Func&lt;Combate,Buff&gt;` (gêmeo do AplicarBuff) — nenhum cliente precisa hoje.
    ///
    /// REGRA DO REVIVE (decisão de Gabriel — vale pra toda a família): a ação NÃO seleciona
    /// quantos revive; a seleção é a MESMA de qualquer habilidade, pelo ResolverAlvos:
    /// - Revive-de-TODOS (Nigiri, AnjoCaído...): escopo default (TodosAliados/Mortos), sem pick.
    /// - Revive-de-N (DocesDeAbobora revive 1): a HABILIDADE declara numeroDeAlvos: N +
    ///   TipoAlvo.Aleatorio + EstadoAlvo.Mortos, e a ação usa Escopo.AlvosResolvidos — o
    ///   jogador escolhe o morto (pick real, ADR-selecao-por-estado §2.4) e os extras são
    ///   sorteados (selecionado + random, semântica do Aleatorio). Duplicata do sorteio é
    ///   inofensiva: Vivo.Reviver é no-op.
    /// Um mecanismo só de seleção pro jogo inteiro — nada de Take/contador dentro da ação.
    ///
    /// A Sentença (ImpedirRessurreicao) é checada CENTRAL no Morto.Reviver — esta ação não
    /// escreve nada disso. EstadoAlvo default Mortos — o único que faz sentido; o filtro do
    /// interpretador garante que a ação nem visita os vivos.
    /// </summary>
    class Reviver : Acao
    {
        private readonly double _percentualHP;
        private readonly Func<Buff>? _buffNoRevivido;

        public Reviver(double percentualHP, Escopo escopo = Escopo.TodosAliados,
            EstadoAlvo estadoAlvo = EstadoAlvo.Mortos, Func<Buff>? buffNoRevivido = null)
            : base(escopo, estadoAlvo)
        {
            _percentualHP = percentualHP;
            _buffNoRevivido = buffNoRevivido;
        }

        public override void Executar(Combate atacante, Combate alvo, List<EventoDano> eventos)
        {
            alvo.Reviver((int)(alvo.HPMaximo * _percentualHP));
            if (_buffNoRevivido != null && alvo.EstaVivo())   // só quem REALMENTE voltou
                _buffNoRevivido().Aplicar(alvo);
        }
    }
}
