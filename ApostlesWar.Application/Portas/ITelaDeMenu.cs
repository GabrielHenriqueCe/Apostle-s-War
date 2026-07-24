using ApostlesWar.Domain;

namespace ApostlesWar.Application.Portas
{
    /// <summary>
    /// Porta de APRESENTAÇÃO DOS MENUS — o que o fluxo fora da luta (GerenciadorDeJogoService,
    /// CampeoesService) precisa mostrar/perguntar. Irmã da <see cref="ITelaDeCombate"/> (partida):
    /// juntas fecham o isolamento da Application em relação à tela. Nasceu no split por camadas:
    /// os services recebiam a MenuView CONCRETA, o que criava um ciclo Application↔Presentation —
    /// agora recebem a porta, e a MenuView (console) é uma implementação; a do front será outra.
    ///
    /// Os métodos `Exibir*` desenham um quadro (o cursor vem de fora, no `selecionado`/`opcao`);
    /// os `Navegar*` são interativos: rodam o loop de navegação inteiro e devolvem a ESCOLHA.
    /// </summary>
    public interface ITelaDeMenu
    {
        /// <summary>Menu principal com o cursor na opção `selecionado`.</summary>
        void ExibirMenu(int selecionado);

        /// <summary>Menu da Arena (times, iniciar, voltar) com o cursor em `opcao`.</summary>
        void ExibirMenuArena(int opcao);

        /// <summary>Diálogo "sair do jogo?" com o cursor em `opcao`.</summary>
        void ExibirConfirmacaoSaida(int opcao);

        /// <summary>Seleção de capítulo (facção) com o cursor em `selecionado`.</summary>
        void MenuCapitulos(int selecionado);

        /// <summary>Seleção de fase do capítulo `faccao` com o cursor em `selecionado`.</summary>
        void MenuFases(Faccao faccao, int selecionado);

        /// <summary>Tela de vitória: campeões desbloqueados e item dropado (se houver).</summary>
        void ExibirTelaVitoria(List<Personagem> novosCampeoes, Item? itemDropado);

        void ExibirTelaDerrota();

        void ExibirCreditos();

        /// <summary>Aviso passageiro (ex: "nenhum item disponível") que fica `ms` na tela.</summary>
        void ExibirAviso(string mensagem, int ms);

        /// <summary>Inventário/boneco: navega os slots e devolve o slot escolhido (ou saída).</summary>
        int NavegarBoneco();

        /// <summary>Troca de item do slot em edição; devolve o item escolhido ou null (cancelou).</summary>
        Item? NavegarTrocaItem(int slotEmEdicao, List<Item> itensDoTipo);

        /// <summary>Montagem do time entre os `desbloqueados`; devolve o time fechado.</summary>
        List<Personagem> NavegarSelecaoTime(List<Personagem> desbloqueados);
    }
}
