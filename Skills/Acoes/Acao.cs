namespace ApostlesWar
{
    /// <summary>
    /// Bloco de composição de uma habilidade — a operação que ela executa sobre um alvo.
    /// Uma HabilidadeAtiva de "Balde 1" (que só aplica uma lista fixa de efeitos) declara
    /// suas Acoes em vez de sobrescrever Ativar; o interpretador default
    /// (HabilidadeAtiva.Ativar) roda cada ação sobre os alvos resolvidos, na ordem declarada.
    ///
    /// Escopo por ação (aliados/inimigos/próprio) e ações que agregam entre alvos entram
    /// quando uma habilidade migrada precisar — ver ADR-composicao-de-acoes.md §3.2/§6.
    /// Por ora a ação recebe UM alvo já resolvido; habilidades bespoke seguem no Ativar.
    /// </summary>
    abstract class Acao
    {
        /// <summary>
        /// Executa a ação sobre um alvo resolvido. Ações que causam dano acrescentam o
        /// EventoDano produzido à lista (consumida pelas reações-do-atacante e pela exibição).
        /// </summary>
        public abstract void Executar(Combate atacante, Combate alvo, List<EventoDano> eventos);
    }
}
