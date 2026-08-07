using System;
using System.Linq;
using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;

namespace ApostlesWar.Application.Services
{
    /// <summary>
    /// A "conta" do jogador: cria/carrega/apaga o <see cref="Perfil"/> pela porta de save — e é dono
    /// das REGRAS de quem pode ser o seu avatar. O boot do front usa <see cref="Existe"/> pra decidir
    /// entre pedir o nome (1ª vez) ou cair direto no menu.
    ///
    /// As regras de avatar moravam na tela de edição: ela sabia que o avatar inicial é um Humano e
    /// que só apóstolo desbloqueado vale. Isso é progressão (a campanha libera avatar junto com o
    /// apóstolo), então mora aqui. O front segue validando o clique, mas como FRONTEIRA — não como a
    /// fonte da regra.
    ///
    /// <see cref="Excluir"/> é o "excluir conta" escondido nas configurações: limpa o perfil E o
    /// progresso de campanha ("save"/"itens"/"campanha") — o wipe COMPLETO, de propósito difícil de achar.
    /// </summary>
    public class PerfilService
    {
        private const string ChavePerfil = "perfil";

        private readonly IRepositorioDeSave _repositorio;
        private readonly ApostolosService _apostolos;
        private readonly CampanhaService _campanha;

        public PerfilService(IRepositorioDeSave repositorio, ApostolosService apostolos,
            CampanhaService campanha)
        {
            _repositorio = repositorio;
            _apostolos = apostolos;
            _campanha = campanha;
        }

        /// <summary>
        /// O avatar de quem acabou de criar a conta: um dos Humanos, o time com que todo mundo começa.
        /// Sorteado porque é placeholder — a 1ª tela pede só o nome, e trocar de cara é 2 cliques.
        /// </summary>
        public string AvatarInicial()
        {
            var iniciais = _apostolos.ObterDesbloqueados().Where(p => p.Faccao == Faccao.Humanos).ToList();
            return iniciais[Random.Shared.Next(iniciais.Count)].Simbolo;
        }

        /// <summary>
        /// Este apóstolo pode ser o avatar? Só os DESBLOQUEADOS — a cara do jogador é troféu de
        /// campanha, não catálogo. A grade mostra os 36 (os travados em cinza); quem recusa é isto.
        /// </summary>
        public bool PodeUsarAvatar(Personagem apostolo) => _apostolos.EstaDesbloqueado(apostolo);

        public Perfil? Carregar() => _repositorio.Carregar<Perfil>(ChavePerfil);

        public bool Existe() => Carregar() is not null;

        public void CriarPerfil(string nome, string avatar)
            => _repositorio.Salvar(ChavePerfil, new Perfil(nome, avatar));

        /// <summary>
        /// O wipe COMPLETO: o perfil é desta casa, o progresso é do
        /// <see cref="CampanhaService.ResetarProgresso"/>. Depois disto o jogo tem que estar
        /// indistinguível de uma instalação nova — inclusive em MEMÓRIA, que é o que faltava.
        /// </summary>
        public void Excluir()
        {
            _repositorio.Excluir(ChavePerfil);
            _campanha.ResetarProgresso();
        }
    }
}
