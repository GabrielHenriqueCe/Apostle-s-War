using ApostlesWar.Application;
using ApostlesWar.Application.Portas;
using ApostlesWar.Application.Services;

namespace Tests
{
    /// <summary>
    /// Testes do <see cref="PerfilService"/> — a "conta" do jogador. É PURO (só delega pra porta de
    /// save), então roda headless com um repositório fake em memória. Cobre o boot (existe/carrega),
    /// a criação, e o "excluir conta" que limpa o perfil E o progresso de campanha.
    /// </summary>
    public class PerfilServiceTests
    {
        // Porta de save fake: um dicionário em memória (a impl real, SaveLocal, mora na Infrastructure,
        // que o projeto de teste não referencia — e nem precisa: aqui só interessa a LÓGICA de chaves).
        private sealed class RepositorioFake : IRepositorioDeSave
        {
            private readonly Dictionary<string, object?> _dados = new();

            public void Salvar<T>(string chave, T dado) => _dados[chave] = dado;
            public T? Carregar<T>(string chave) => _dados.TryGetValue(chave, out var v) && v is T t ? t : default;
            public void Excluir(string chave) => _dados.Remove(chave);

            public bool Contem(string chave) => _dados.ContainsKey(chave);
        }

        [Fact]
        public void SemPerfil_NaoExiste_ECarregaNulo()
        {
            var servico = new PerfilService(new RepositorioFake());

            Assert.False(servico.Existe());
            Assert.Null(servico.Carregar());
        }

        [Fact]
        public void CriarPerfil_PassaAExistir_EGuardaNomeEAvatar()
        {
            var servico = new PerfilService(new RepositorioFake());

            servico.CriarPerfil("Gabriel", "🕵️");

            Assert.True(servico.Existe());
            Perfil? p = servico.Carregar();
            Assert.NotNull(p);
            Assert.Equal("Gabriel", p!.Nome);
            Assert.Equal("🕵️", p.Avatar);
        }

        [Fact]
        public void Excluir_ApagaPerfil_EOProgressoDeCampanha()
        {
            var repo = new RepositorioFake();
            var servico = new PerfilService(repo);

            servico.CriarPerfil("Gabriel", "🕵️");
            repo.Salvar("save", "progresso qualquer");   // simula uma campanha em andamento
            repo.Salvar("itens", "itens quaisquer");

            servico.Excluir();

            Assert.False(servico.Existe());
            Assert.False(repo.Contem("perfil"));
            Assert.False(repo.Contem("save"));
            Assert.False(repo.Contem("itens"));
        }
    }
}
