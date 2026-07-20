namespace ApostlesWar.Controllers
{
    /// <summary>
    /// Seam de CONTROLE do turno: quem DECIDE a ação e o alvo. O loop de combate pergunta ao
    /// controlador e não sabe se por trás é humano (menu), bot (IA), ou o jogador em modo automático
    /// — é isso que deixa o auto-mode e o bot inteligente "caminho andado" (só mais uma implementação,
    /// trocada no composition root, sem tocar no loop). Ver plano do refactor do ExecutarTurno.
    ///
    /// Separado da EXECUÇÃO (que é pura: recebe ação+alvo e aplica) e da APRESENTAÇÃO (IApresentacao).
    /// </summary>
    internal interface IControladorDeTurno
    {
        /// <summary>Qual habilidade o combatente usa neste turno.</summary>
        HabilidadeAtiva EscolherAcao(Combate atacante, List<Combate> aliados, List<Combate> defensores);

        /// <summary>
        /// Qual alvo, dentre os já disponíveis (filtrados pela regra de targeting no CombateService).
        /// Só é chamado quando há pick a fazer — a cola de "qual lista / lista vazia → próprio" fica
        /// no CombateService, não aqui.
        /// </summary>
        Combate EscolherAlvo(List<Combate> disponiveis, List<Combate> aliados, List<Combate> defensores);
    }
}
