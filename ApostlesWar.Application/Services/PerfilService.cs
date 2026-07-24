using ApostlesWar.Application.Portas;

namespace ApostlesWar.Application.Services
{
    /// <summary>
    /// Cria/carrega/apaga o <see cref="Perfil"/> do jogador pela porta de save. O boot do front usa
    /// <see cref="Existe"/> pra decidir entre pedir o nome (1ª vez) ou cair direto no menu.
    ///
    /// <see cref="Excluir"/> é o "excluir conta" escondido nas configurações: limpa o perfil E o
    /// progresso de campanha ("save"/"itens") — o wipe COMPLETO, de propósito difícil de achar.
    /// </summary>
    public class PerfilService
    {
        private const string ChavePerfil = "perfil";

        // O que "excluir conta" apaga além do perfil: todo o progresso de campanha. (O front ainda não
        // grava esses slots — a Campanha é fatia futura —, mas o wipe já nasce completo.)
        private static readonly string[] ChavesDoProgresso = { "save", "itens" };

        private readonly IRepositorioDeSave _repositorio;

        public PerfilService(IRepositorioDeSave repositorio) => _repositorio = repositorio;

        public Perfil? Carregar() => _repositorio.Carregar<Perfil>(ChavePerfil);

        public bool Existe() => Carregar() is not null;

        public void CriarPerfil(string nome, string avatar)
            => _repositorio.Salvar(ChavePerfil, new Perfil(nome, avatar));

        public void Excluir()
        {
            _repositorio.Excluir(ChavePerfil);
            foreach (string chave in ChavesDoProgresso) _repositorio.Excluir(chave);
        }
    }
}
