using ApostlesWar.Application.Portas;
using ApostlesWar.Application.Services;

namespace Tests
{
    /// <summary>
    /// Testes do <see cref="ConfiguracaoService"/> — as PREFERÊNCIAS do jogador. É puro (só delega
    /// pra porta de save), então roda headless com um repositório fake.
    ///
    /// O que vale a pena travar aqui é justamente o que uma tela não prova sozinha: o padrão de
    /// instalação nova, o fato de a escolha SOBREVIVER, e o de ela NÃO morrer no "excluir conta" —
    /// esse último é o tipo de coisa que se quebra sem querer, mexendo no wipe por outro motivo.
    /// </summary>
    public class ConfiguracaoServiceTests
    {
        private sealed class RepositorioFake : IRepositorioDeSave
        {
            private readonly Dictionary<string, object?> _dados = new();
            public void Salvar<T>(string chave, T dado) => _dados[chave] = dado;
            public T? Carregar<T>(string chave) => _dados.TryGetValue(chave, out var v) && v is T t ? t : default;
            public void Excluir(string chave) => _dados.Remove(chave);
            public bool Contem(string chave) => _dados.ContainsKey(chave);
        }

        /// <summary>Instalação nova abre em tela cheia — é assim que jogo abre.</summary>
        [Fact]
        public void SemSave_AbreEmTelaCheia()
        {
            var config = new ConfiguracaoService(new RepositorioFake());

            Assert.True(config.Carregar().TelaCheia);
        }

        [Fact]
        public void AlternarTelaCheia_DevolveOEstadoNovo_EPersiste()
        {
            var repo = new RepositorioFake();
            var config = new ConfiguracaoService(repo);

            Assert.False(config.AlternarTelaCheia());   // estava cheia → vira janela
            Assert.False(config.Carregar().TelaCheia);

            Assert.True(config.AlternarTelaCheia());    // e volta
            Assert.True(config.Carregar().TelaCheia);
        }

        /// <summary>
        /// A escolha atravessa o fechar do jogo: um service novo lendo o mesmo save encontra o que o
        /// anterior deixou. É a diferença entre "preferência" e "estado de sessão".
        /// </summary>
        [Fact]
        public void Preferencia_SobreviveAoServiceQueAEscreveu()
        {
            var repo = new RepositorioFake();
            new ConfiguracaoService(repo).AlternarTelaCheia();   // desligou a tela cheia

            Assert.False(new ConfiguracaoService(repo).Carregar().TelaCheia);
        }

        /// <summary>
        /// "Excluir conta" zera o PROGRESSO, não o gosto de quem está na cadeira. A chave "config"
        /// não está na lista do <see cref="CampanhaService.ResetarProgresso"/> de propósito — este
        /// teste é o que segura essa decisão no lugar quando alguém mexer no wipe.
        /// </summary>
        [Fact]
        public void ExcluirConta_NaoApagaAPreferenciaDeTela()
        {
            var repo = new RepositorioFake();
            var capitulos = new CapitulosService(repo);
            var arsenal = new ArsenalService(capitulos, repo);
            var campeoes = new CampeoesService(new PersonagemService(), capitulos);
            var campanha = new CampanhaService(arsenal, campeoes, capitulos, new PersonagemService(), repo);
            var perfil = new PerfilService(repo, campeoes, campanha);
            var config = new ConfiguracaoService(repo);

            config.AlternarTelaCheia();          // o jogador escolheu janela
            perfil.CriarPerfil("Gabriel", "🧑‍🔧");

            perfil.Excluir();                    // wipe completo do progresso

            Assert.False(config.Carregar().TelaCheia);   // a preferência continua de pé
            Assert.False(repo.Contem("perfil"));         // e o resto foi mesmo embora
        }
    }
}
