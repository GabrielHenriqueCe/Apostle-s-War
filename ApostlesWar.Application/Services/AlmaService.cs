using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;

namespace ApostlesWar.Application.Services
{
    /// <summary>
    /// O saldo de ALMA do jogador — um pote só, compartilhado por todos os apóstolos. Tudo o que é
    /// bolso (guardar, cobrar, fundir, diluir) mora na <see cref="CarteiraDeMaterial"/>; aqui fica só
    /// a torneira, que é a única coisa em que a alma difere do <see cref="PoService"/>.
    /// </summary>
    public class AlmaService : CarteiraDeMaterial
    {
        // O slot de save deste service. Const porque o wipe do Resetar e o carregamento o citam.
        private const string ChaveAlma = "alma";

        public AlmaService(IRepositorioDeSave repo) : base(repo, ChaveAlma) { }

        /// <summary>
        /// A alma dos inimigos derrubados nesta fase. Cai por INIMIGO, não por vitória — quem matou
        /// dois e perdeu leva os dois, igual à XP.
        /// </summary>
        public void Creditar(Dificuldade dificuldade, int mortos)
            => Depositar(Alma.QuedaPorInimigo(dificuldade), mortos);
    }
}
