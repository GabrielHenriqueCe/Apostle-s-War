namespace ApostlesWar.Application.Portas
{
    /// <summary>
    /// Seam de apresentação do combate. Encapsula a ESPERA (as pausas dramáticas entre eventos) — e é
    /// o ponto único onde o cancelamento pluga: a espera ESCUTA o teclado e avisa se o jogador apertou
    /// Esc pra encerrar a batalha (em vez de dormir cego). Cada Presentation traz seu adaptador
    /// (`ApresentacaoConsole` no console); o combate não muda. (Forma 1 — espera interrompível.)
    /// </summary>
    public interface IApresentacao
    {
        /// <summary>
        /// Pausa dramática entre eventos. Retorna TRUE se o jogador apertou Esc durante a espera
        /// (pedido de encerrar a batalha) — quem chama decide o que fazer com isso.
        /// </summary>
        bool AguardarAnimacao(int ms);
    }
}
