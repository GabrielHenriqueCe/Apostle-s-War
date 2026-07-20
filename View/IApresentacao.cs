namespace ApostlesWar.View
{
    /// <summary>
    /// Seam de apresentação do combate. Encapsula a ESPERA (as pausas dramáticas entre eventos) — e é
    /// o ponto único onde o cancelamento pluga: a espera ESCUTA o teclado e avisa se o jogador apertou
    /// Esc pra encerrar a batalha (em vez de dormir cego). No porte web/Unity, troca-se `EntradaConsole`
    /// e este adapter; o combate não muda. (Forma 1 — espera interrompível; ver ROADMAP/plano.)
    /// </summary>
    interface IApresentacao
    {
        /// <summary>
        /// Pausa dramática entre eventos. Retorna TRUE se o jogador apertou Esc durante a espera
        /// (pedido de encerrar a batalha) — quem chama decide o que fazer com isso.
        /// </summary>
        bool AguardarAnimacao(int ms);
    }

    /// <summary>Console: dorme em fatias curtas escutando Esc. Trocável (web/Unity/testes).</summary>
    internal class ApresentacaoConsole : IApresentacao
    {
        public bool AguardarAnimacao(int ms)
        {
            const int fatia = 40;
            for (int passou = 0; passou < ms; passou += fatia)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    return true;
                Thread.Sleep(Math.Min(fatia, ms - passou));
            }
            return false;
        }
    }
}
