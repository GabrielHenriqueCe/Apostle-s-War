namespace ApostlesWar
{
    /// <summary>
    /// Revive combatentes mortos com um percentual do HP máximo. Família de 7 clientes
    /// (ADR-composicao-de-acoes §9): Nigiri, Tecnology, Céu, AnjoCaído, DocesDeAbobora,
    /// Circo, Atlantis.
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

        public Reviver(double percentualHP, Escopo escopo = Escopo.TodosAliados,
            EstadoAlvo estadoAlvo = EstadoAlvo.Mortos)
            : base(escopo, estadoAlvo) => _percentualHP = percentualHP;

        public override void Executar(Combate atacante, Combate alvo, List<EventoDano> eventos)
            => alvo.Reviver((int)(alvo.HPMaximo * _percentualHP));
    }
}
