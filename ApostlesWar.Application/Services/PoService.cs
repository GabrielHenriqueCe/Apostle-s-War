using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;

namespace ApostlesWar.Application.Services
{
    /// <summary>
    /// O saldo de PÓ do jogador — um pote só, compartilhado por todos os itens, irmão do
    /// <see cref="AlmaService"/>. O bolso é da <see cref="CarteiraDeMaterial"/>; aqui só a torneira.
    /// </summary>
    public class PoService : CarteiraDeMaterial
    {
        // O slot de save deste service. Const porque o wipe do Resetar e o carregamento o citam.
        private const string ChavePo = "po";

        public PoService(IRepositorioDeSave repo) : base(repo, ChavePo) { }

        /// <summary>
        /// O pó de uma fase VENCIDA — por fase, não por inimigo, que é onde a torneira do pó difere
        /// da alma. Ele cai junto com as peças e pela mesma regra delas: <b>derrota não dropa</b>. É
        /// a diferença contra a alma, que cai por inimigo morto mesmo na derrota.
        /// </summary>
        public void Creditar(Dificuldade dificuldade) => Depositar(Po.QuedaPorFase(dificuldade), vezes: 1);

        /// <summary>
        /// A SEGUNDA torneira do pó: o que o ⚙️ Esmeril devolveu por uma peça moída. Entra por aqui e
        /// não pelo <see cref="Creditar(Dificuldade)"/> porque a fase paga três faixas de uma vez e o
        /// esmeril paga uma só — a da peça.
        /// </summary>
        public void Creditar(Custo ganho) => Depositar(new[] { ganho }, vezes: 1);
    }
}
