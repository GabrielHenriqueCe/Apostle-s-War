namespace ApostlesWar
{
    /// <summary>
    /// Bloco de composição de uma habilidade — uma operação (Dano/Cura/AplicarBuff/...) sobre
    /// um conjunto de combatentes. A habilidade declara uma LISTA de Ações; o interpretador
    /// (HabilidadeAtiva.Ativar) roda cada uma na ordem, resolvendo o Escopo e filtrando pelo
    /// EstadoAlvo NO MOMENTO em que a ação executa (ver ADR-composicao-de-acoes §3).
    ///
    /// Cada ação carrega dois eixos:
    /// - Escopo: em quais combatentes ela cai (alvos resolvidos / aliados / inimigos / próprio).
    /// - EstadoAlvo: vivos ou mortos, avaliado na execução — é isso que faz uma ação pegar os
    ///   recém-revividos (Vivos) ou os recém-mortos (Mortos) da ação anterior, sem condicional.
    ///
    /// INVARIANTE (ADR §3.1): na forma-construtor a lista de Acoes é criada UMA vez e reusada a
    /// cada ativação — uma Acao só pode carregar CONFIG (multiplicador, fábrica, fragmento de
    /// Valor), nunca estado por-ativação. Estado que atravessa alvos vive em variável local do
    /// Executar de uma AcaoSobreConjunto, não em campo.
    /// </summary>
    abstract class Acao
    {
        public Escopo Escopo { get; }
        public EstadoAlvo EstadoAlvo { get; }

        protected Acao(Escopo escopo, EstadoAlvo estadoAlvo)
        {
            Escopo = escopo;
            EstadoAlvo = estadoAlvo;
        }

        /// <summary>
        /// Executa a ação sobre UM combatente já resolvido (escopo + estado filtrados pelo
        /// interpretador). Ações que causam dano acrescentam o EventoDano à lista — consumida
        /// pelas reações-do-atacante e pela exibição.
        /// </summary>
        public abstract void Executar(Combate atacante, Combate alvo, List<EventoDano> eventos);
    }
}
