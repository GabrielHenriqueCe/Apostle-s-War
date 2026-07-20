using ApostlesWar;
using ApostlesWar.View;

namespace ApostlesWar.Services
{
    internal class GerenciadorDeJogoService
    {
        #region Construtor

        private readonly ArsenalService _arsenalService;
        private readonly CampeoesService _campeoesService;
        private readonly CapitulosService _capitulosService;
        private readonly MenuView _menuView;
        private readonly CombateService _combateService;
        private readonly IEntrada _entrada;

        public GerenciadorDeJogoService(ArsenalService arsenalService,
            CampeoesService campeoesService, CapitulosService capitulosService,
            MenuView menuView, CombateService combateService, IEntrada entrada)
        {
            _arsenalService = arsenalService;
            _campeoesService = campeoesService;
            _capitulosService = capitulosService;
            _menuView = menuView;
            _combateService = combateService;
            _entrada = entrada;
        }

        #endregion

        #region Entry point

        public void Executar()
        {
            CarregarSaves();

            while (true)
            {
                int opcao = EscolherOpcaoMenuPrincipal();
                if (opcao == -1) break; // saiu

                switch (opcao)
                {
                    case 1: ExecutarFluxoCampanha(); break;
                    case 2: ExecutarFluxoInventario(); break;
                }
            }
        }

        #endregion

        #region Save / load

        private void CarregarSaves()
        {
            _arsenalService.CarregarItensEquipados();
            _capitulosService.CarregarProgresso();
            _campeoesService.CarregarCampeoes();
            _arsenalService.CarregarItens();
        }

        #endregion

        #region Menu principal

        /// <summary>
        /// Retorna a opção selecionada, ou -1 se o jogador escolher sair.
        /// </summary>
        private int EscolherOpcaoMenuPrincipal()
        {
            int opcaoMenu = 1;
            while (true)
            {
                _menuView.ExibirMenu(opcaoMenu);
                Comando cmd = _entrada.Ler();

                if (cmd.Tipo == TipoComando.Confirmar) return opcaoMenu;

                if (cmd.Tipo == TipoComando.Cancelar)
                {
                    if (ConfirmarSaida()) return -1;
                    continue;
                }

                opcaoMenu = Navegacao.MoverCursor(opcaoMenu, 1, 2, cmd);
            }
        }

        private bool ConfirmarSaida()
        {
            int opcao = 1;
            while (true)
            {
                _menuView.ExibirConfirmacaoSaida(opcao);

                Comando cmd = _entrada.Ler();

                if (cmd.Tipo == TipoComando.Confirmar)
                {
                    if (opcao == 1)
                    {
                        _menuView.ExibirCreditos();
                        return true;
                    }
                    return false;
                }

                if (cmd.Tipo == TipoComando.Cancelar) return false;

                opcao = Navegacao.MoverCursor(opcao, 1, 2, cmd);
            }
        }

        #endregion

        #region Fluxo Campanha

        private void ExecutarFluxoCampanha()
        {
            int opcaoFaccao = 1;
            while (true)
            {
                _menuView.MenuCapitulos(opcaoFaccao);
                Comando cmd = _entrada.Ler();

                if (cmd.Tipo == TipoComando.Cancelar) return;

                if (cmd.Tipo == TipoComando.Confirmar)
                {
                    var faccoes = Enum.GetValues<Faccao>().Where(f => f != Faccao.Humanos).ToList();
                    Faccao faccao = faccoes[opcaoFaccao - 1];

                    if (_capitulosService.EstaCapituloDesbloqueado(faccao))
                        ExecutarFluxoFases(faccao);
                }
                else
                {
                    opcaoFaccao = Navegacao.MoverCursor(opcaoFaccao, 1, 8, cmd);
                }
            }
        }

        private void ExecutarFluxoFases(Faccao faccao)
        {
            int opcaoFase = 1;
            while (true)
            {
                _menuView.MenuFases(faccao, opcaoFase);
                Comando cmd = _entrada.Ler();

                if (cmd.Tipo == TipoComando.Cancelar) return;

                if (cmd.Tipo == TipoComando.Confirmar)
                {
                    Fases fase = (Fases)opcaoFase;
                    if (_capitulosService.EstaDesbloqueado(faccao, fase))
                        TentarExecutarFase(faccao, fase);
                }
                else
                {
                    opcaoFase = Navegacao.MoverCursor(opcaoFase, 1, 7, cmd);
                }
            }
        }

        private void TentarExecutarFase(Faccao faccao, Fases fase)
        {
            if (!_combateService.ExecutarFase(faccao, fase)) return;

            ProcessarVitoria(faccao, fase);
        }

        /// <summary>
        /// Centraliza tudo que acontece após vencer uma fase: desbloqueios, drop, save, exibição.
        /// </summary>
        private void ProcessarVitoria(Faccao faccao, Fases fase)
        {
            var antes = _campeoesService.ObterDesbloqueados().ToList();

            _capitulosService.DesbloquearFase(faccao, fase);
            _capitulosService.ConcluirFase(faccao, fase);
            _campeoesService.DesbloquearCampeoes(faccao, fase);
            Item? item = _arsenalService.DroparItem(faccao, fase);
            _capitulosService.DesbloquearFaccao(faccao, fase);
            _capitulosService.SalvarProgresso();
            _arsenalService.SalvarItens();

            ExibirTelaVitoria(antes, item);
        }

        private void ExibirTelaVitoria(List<Personagem> antesDosDesbloqueios, Item? itemDropado)
        {
            // Computa o que é NOVO (domínio) aqui; a View só desenha e o input espera aqui.
            var novos = _campeoesService.ObterDesbloqueados().Except(antesDosDesbloqueios).ToList();
            _menuView.ExibirTelaVitoria(novos, itemDropado);
            while (_entrada.Ler().Tipo != TipoComando.Confirmar) { }
        }

        #endregion

        #region Fluxo Inventário

        /// <summary>
        /// Loop do inventário: exibe, deixa jogador escolher item, equipa.
        /// A lógica de equipar saiu do MenuView (que agora só exibe e retorna).
        /// </summary>
        private void ExecutarFluxoInventario()
        {
            while (true)
            {
                int slot = _menuView.NavegarBoneco();
                if (slot == -1) return;

                Fases faseDoSlot = (Fases)(slot + 1);
                List<Item> itensDoTipo = _arsenalService.ObterObtidos()
                    .Where(i => i.Fase == faseDoSlot)
                    .ToList();

                if (itensDoTipo.Count == 0)
                {
                    _menuView.ExibirAviso("\nNenhum item desse tipo disponível.", 1200);
                    continue;
                }

                Item? escolhido = _menuView.NavegarTrocaItem(slot, itensDoTipo);
                if (escolhido != null)
                    _arsenalService.EquiparItem(escolhido);
            }
        }


        #endregion
    }
}
