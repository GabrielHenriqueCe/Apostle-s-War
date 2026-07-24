using System.Text.Json;

namespace ApostlesWar.Services
{
    /// <summary>
    /// Porta de PERSISTÊNCIA: abstrai onde/como o save é guardado (par dos seams IEntrada/
    /// IApresentacao, do lado de dados). A porta é dona da serialização (JSON) + do armazenamento +
    /// do tratamento de corrupção — o service entrega/recebe o OBJETO e não toca em arquivo nem JSON.
    /// No porte Unity, troca-se `SaveLocal` por `SaveSteam`/`SavePlayGames` (SDK de plataforma) sem
    /// tocar nos services. `chave` = o "slot" do save (ex: "save", "itens").
    /// </summary>
    interface IRepositorioDeSave
    {
        void Salvar<T>(string chave, T dado);

        /// <summary>Devolve o dado do slot, ou default(T) se AUSENTE ou CORROMPIDO (silencioso).</summary>
        T? Carregar<T>(string chave);
    }

    /// <summary>
    /// Impl local: um arquivo JSON por slot em `AppContext.BaseDirectory/Save/&lt;chave&gt;.txt` — ao
    /// lado do executável (independe da working directory) e batendo com a semente `Save/` que o
    /// csproj copia pra saída. Save corrompido/ilegível → carrega default (o jogo abre do zero), sem
    /// crashar e sem mensagem (decisão jul/2026). Trocável (Steam/Play/testes).
    /// </summary>
    internal class SaveLocal : IRepositorioDeSave
    {
        private static string Caminho(string chave)
            => Path.Combine(AppContext.BaseDirectory, "Save", chave + ".txt");

        public void Salvar<T>(string chave, T dado)
        {
            string caminho = Caminho(chave);
            string json = JsonSerializer.Serialize(dado);
            Directory.CreateDirectory(Path.GetDirectoryName(caminho)!);
            File.WriteAllText(caminho, json);
        }

        public T? Carregar<T>(string chave)
        {
            string caminho = Caminho(chave);
            if (!File.Exists(caminho)) return default;

            try
            {
                return JsonSerializer.Deserialize<T>(File.ReadAllText(caminho));
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                // Save corrompido ou ilegível: mantém o default (jogo abre do zero) em vez de crashar.
                return default;
            }
        }
    }
}
