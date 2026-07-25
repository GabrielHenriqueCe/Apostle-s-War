namespace ApostlesWar.Domain
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
    /// Valor), nunca estado por-ativação. Valor que atravessa alvos passa pelo `eventos` (é
    /// assim que PorDanoCausado agrega), nunca por campo da instância.
    /// </summary>
    public abstract class Acao
    {
        public Escopo Escopo { get; }
        public EstadoAlvo EstadoAlvo { get; }

        protected Acao(Escopo escopo, EstadoAlvo estadoAlvo)
        {
            Escopo = escopo;
            EstadoAlvo = estadoAlvo;
        }

        /// <summary>
        /// QUE BEM esta ação faz — a classificação usada por quem AVALIA uma habilidade antes de
        /// usá-la (o controlador automático). É FATO do domínio ("isto cura", "isto revive"); a
        /// ORDEM de preferência entre as utilidades é opinião tática e mora no controlador, não aqui.
        ///
        /// Abstrata de propósito: ação nova é obrigada pelo compilador a se classificar. Sem isso, a
        /// alternativa seria o avaliador dar `switch` no tipo concreto da ação — exatamente o que o
        /// #9 tirou do CombateService (cada peça carrega a própria capacidade, ninguém pergunta "que
        /// tipo você é?").
        /// </summary>
        public abstract Utilidade Utilidade { get; }

        /// <summary>
        /// Rodar isto AGORA muda alguma coisa? Curar quem está com a vida cheia, limpar quem não tem
        /// debuff ou detonar quem não está envenenado é queimar o turno à toa.
        ///
        /// `alvos` já vem resolvido pelo Escopo e pelo EstadoAlvo desta ação, então boa parte das
        /// respostas cai sozinha: o `Reviver` mira `EstadoAlvo.Mortos`, logo lista vazia == não há
        /// quem reviver. Default `true` — quem não tem pré-condição sempre tem o que fazer.
        ///
        /// É pergunta de FATO (há trabalho a fazer?), não de estratégia (vale mais que outra coisa?).
        /// </summary>
        public virtual bool TemEfeitoUtil(Combate atacante, IReadOnlyList<Combate> alvos) => alvos.Count > 0;

        /// <summary>
        /// Quanta VIDA esta ação tiraria do alvo, sem executá-la. Zero pra quem não machuca — que é a
        /// maioria, daí o default.
        ///
        /// Mora aqui, e não num `OfType&lt;Dano&gt;()` de quem pergunta, porque dano não vem só da ação
        /// `Dano`: a explosão detona status, a Shuriken encadeia golpes. Uma varredura por tipo
        /// concreto enxergaria só a primeira e leria as outras como inofensivas — foi exatamente o
        /// que aconteceu antes desta virtual existir, e o bot preferia o ataque básico a explodir um
        /// alvo envenenado.
        /// </summary>
        public virtual int PreverVidaRemovida(Combate atacante, Combate alvo) => 0;

        /// <summary>
        /// Executa a ação sobre UM combatente já resolvido (escopo + estado filtrados pelo
        /// interpretador). Ações que causam dano acrescentam o EventoDano à lista — consumida
        /// pelas reações-do-atacante e pela exibição.
        /// </summary>
        public abstract void Executar(Combate atacante, Combate alvo, List<EventoCombate> eventos);
    }
}
