using ApostlesWar;
using GHUtils;

namespace v1_Apostle_s_War.Services
{
    internal class GerenciadorDeJogoService
    {
        #region Construtor

        private readonly ArsenalService _arsenalService;
        private readonly CampeoesService _campeoesService;
        private readonly CapitulosService _capitulosService;
        private readonly MenuService _menuService;
        private readonly CombateService _combateService;

        public GerenciadorDeJogoService(ArsenalService arsenalService,
            CampeoesService campeoesService, CapitulosService capitulosService,
            MenuService menuService, CombateService combateService)
        {
            _arsenalService = arsenalService;
            _campeoesService = campeoesService;
            _capitulosService = capitulosService;
            _menuService = menuService;
            _combateService = combateService;
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
                _menuService.ExibirMenu(opcaoMenu);
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter) return opcaoMenu;

                if (key.Key == ConsoleKey.Escape)
                {
                    if (ConfirmarSaida()) return -1;
                    continue;
                }

                opcaoMenu = ConsoleUtils.SelecionarComCursor(opcaoMenu, 1, 2, key.Key);
            }
        }

        private bool ConfirmarSaida()
        {
            int opcao = 1;
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Deseja sair do jogo?\n");
                Console.WriteLine(opcao == 1 ? "▶ 1 - Sim" : "  1 - Sim");
                Console.WriteLine(opcao == 2 ? "▶ 2 - Não" : "  2 - Não");

                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter)
                {
                    if (opcao == 1)
                    {
                        ExibirCreditos();
                        return true;
                    }
                    return false;
                }

                if (key.Key == ConsoleKey.Escape) return false;

                opcao = ConsoleUtils.SelecionarComCursor(opcao, 1, 2, key.Key);
            }
        }

        private void ExibirCreditos()
        {
            string[] linhas =
            {
        "",
        "    ⚔️  Apostle's War  ⚔️",
        "",
        "    Obrigado por jogar, Apóstolo...",
        "",
        "    👑  Desenvolvido por: Gabriel Henrique Cé",
        "    🛠️  Versão: 1.0",
        "    🌑  GitHub: GabrielHenriqueCe",
        "",
        "    Que a guerra dos Apóstolos nunca termine. 🌬️",
        "",
    };

            Console.Clear();
            foreach (string linha in linhas)
            {
                Console.WriteLine(linha);
                Thread.Sleep(180);
            }

            Thread.Sleep(2000);
        }

        #endregion

        #region Fluxo Campanha

        private void ExecutarFluxoCampanha()
        {
            int opcaoFaccao = 1;
            while (true)
            {
                _menuService.MenuCapitulos(opcaoFaccao);
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Escape) return;

                if (key.Key == ConsoleKey.Enter)
                {
                    var faccoes = Enum.GetValues<Faccao>().Where(f => f != Faccao.Humanos).ToList();
                    Faccao faccao = faccoes[opcaoFaccao - 1];

                    if (_capitulosService.EstaCapituloDesbloqueado(faccao))
                        ExecutarFluxoFases(faccao);
                }
                else
                {
                    opcaoFaccao = ConsoleUtils.SelecionarComCursor(opcaoFaccao, 1, 8, key.Key);
                }
            }
        }

        private void ExecutarFluxoFases(Faccao faccao)
        {
            int opcaoFase = 1;
            while (true)
            {
                _menuService.MenuFases(faccao, opcaoFase);
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Escape) return;

                if (key.Key == ConsoleKey.Enter)
                {
                    Fases fase = (Fases)opcaoFase;
                    if (_capitulosService.EstaDesbloqueado(faccao, fase))
                        TentarExecutarFase(faccao, fase);
                }
                else
                {
                    opcaoFase = ConsoleUtils.SelecionarComCursor(opcaoFase, 1, 7, key.Key);
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
            Console.Clear();
            Console.WriteLine("=====Fase Concluída!=====\n");

            var novos = _campeoesService.ObterDesbloqueados().Except(antesDosDesbloqueios).ToList();
            foreach (Personagem p in novos)
                Console.WriteLine($"Novo campeão: {p.Simbolo} {p.Nome}!");

            if (itemDropado != null)
                Console.WriteLine($"Novo item: {itemDropado.Simbolo} {itemDropado.Nome} | {itemDropado.NomeStat()} + {itemDropado.ValorFormatado()}");

            Console.WriteLine("\nPressione Enter para continuar...");
            Console.ReadLine();
        }

        #endregion

        #region Fluxo Inventário

        /// <summary>
        /// Loop do inventário: exibe, deixa jogador escolher item, equipa.
        /// A lógica de equipar saiu do MenuService (que agora só exibe e retorna).
        /// </summary>
        private void ExecutarFluxoInventario()
        {
            while (true)
            {
                int slot = _menuService.NavegarBoneco();
                if (slot == -1) return;

                Fases faseDoSlot = (Fases)(slot + 1);
                List<Item> itensDoTipo = _arsenalService.ObterObtidos()
                    .Where(i => i.Fase == faseDoSlot)
                    .ToList();

                if (itensDoTipo.Count == 0)
                {
                    Console.WriteLine("\nNenhum item desse tipo disponível.");
                    Thread.Sleep(1200);
                    continue;
                }

                Item? escolhido = _menuService.NavegarTrocaItem(slot, itensDoTipo);
                if (escolhido != null)
                    _arsenalService.EquiparItem(escolhido);
            }
        }


        #endregion
    }
}
