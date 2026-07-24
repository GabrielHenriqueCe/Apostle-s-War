namespace ApostlesWar.Application.Services
{
    /// <summary>
    /// Sinaliza que o jogador escolheu ENCERRAR a batalha no meio (Esc → confirmou). Lançada de dentro
    /// da espera e capturada na entrada da fase (ExecutarFase) — desenrola a cadeia profunda de combate
    /// sem threading de um `bool cancelado` por ~15 métodos. Uso legítimo de exceção pra "abortar a
    /// operação a pedido do usuário" (mapeia pro OperationCanceledException do futuro porte async).
    /// </summary>
    public sealed class BatalhaAbortada : Exception { }
}
