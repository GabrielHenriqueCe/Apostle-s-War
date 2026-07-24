using System.Text.Json;
using ApostlesWar.Application.Portas;

namespace ApostlesWar.Infrastructure
{
    /// <summary>
    /// Impl local da <see cref="IRepositorioDeSave"/>: um arquivo JSON por slot em
    /// `AppContext.BaseDirectory/Save/&lt;chave&gt;.txt` — ao lado do executável (independe da working
    /// directory) e batendo com a semente `Save/` que o csproj do App copia pra saída. Save
    /// corrompido/ilegível → carrega default (o jogo abre do zero), sem crashar e sem mensagem
    /// (decisão jul/2026). Trocável (Steam/Play/testes).
    /// </summary>
    public class SaveLocal : IRepositorioDeSave
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
