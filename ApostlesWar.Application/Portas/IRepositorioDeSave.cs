namespace ApostlesWar.Application.Portas
{
    /// <summary>
    /// Porta de PERSISTÊNCIA: abstrai onde/como o save é guardado (par dos seams IEntrada/
    /// IApresentacao, do lado de dados). A porta é dona da serialização + do armazenamento +
    /// do tratamento de corrupção — o service entrega/recebe o OBJETO e não toca em arquivo nem JSON.
    /// A implementação mora na Infrastructure (`SaveLocal`, arquivo JSON); no porte Unity, troca-se
    /// por `SaveSteam`/`SavePlayGames` (SDK de plataforma) sem tocar nos services.
    /// `chave` = o "slot" do save (ex: "save", "itens").
    /// </summary>
    public interface IRepositorioDeSave
    {
        void Salvar<T>(string chave, T dado);

        /// <summary>Devolve o dado do slot, ou default(T) se AUSENTE ou CORROMPIDO (silencioso).</summary>
        T? Carregar<T>(string chave);
    }
}
