using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text;
using GHUtils;
using static GHUtils.ConsoleUtils;

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

        #endregion

        #region GerenciadorDeJogo

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

        public void Executar()
        {
            bool sair = false;
            _arsenalService.CarregarItensEquipados();
            _capitulosService.CarregarProgresso();
            _campeoesService.CarregarCampeoes();
            _arsenalService.CarregarItens();

            do
            {
                int opcaoMenu = 1;
                while (true)
                {
                    _menuService.ExibirMenu(opcaoMenu);
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Enter) break;

                    if (key.Key == ConsoleKey.Escape)
                    {
                        Console.Clear();
                        Console.WriteLine("Deseja sair do jogo? 1 - Sim | 2 - Não");
                        SimOuNao? escolha = LerOpcao<SimOuNao>();
                        if (escolha == SimOuNao.Sim)
                        {
                            Console.WriteLine("Obrigado por jogar! Até a próxima!");
                            Console.ReadLine();
                            sair = true;
                        }
                        break;
                    }

                    opcaoMenu = ConsoleUtils.SelecionarComCursor(opcaoMenu, 1, 2, key.Key);
                }

                if (sair) break;

                switch (opcaoMenu)
                {
                    case 1:
                        int opcaoFaccao = 1;
                        bool voltouCampanha = false;
                        do
                        {
                            _menuService.MenuCapitulos(opcaoFaccao);
                            ConsoleKeyInfo key = Console.ReadKey(true);

                            if (key.Key == ConsoleKey.Escape) { voltouCampanha = true; break; }
                            if (key.Key == ConsoleKey.Enter)
                            {
                                var faccoes = Enum.GetValues<Faccao>().Where(f => f != Faccao.Humanos).ToList();
                                Faccao faccao = faccoes[opcaoFaccao - 1];

                                if (_capitulosService.EstaCapituloDesbloqueado(faccao))
                                {
                                    int opcaoFase = 1;
                                    bool voltouFase = false;
                                    do
                                    {
                                        _menuService.MenuFases(faccao, opcaoFase);
                                        ConsoleKeyInfo keyFase = Console.ReadKey(true);

                                        if (keyFase.Key == ConsoleKey.Escape) { voltouFase = true; break; }
                                        if (keyFase.Key == ConsoleKey.Enter)
                                        {
                                            Fases fase = (Fases)opcaoFase;
                                            if (_capitulosService.EstaDesbloqueado(faccao, fase))
                                            {
                                                if (_combateService.ExecutarFase(faccao, fase))
                                                {
                                                    var antes = _campeoesService.ObterDesbloqueados().ToList();

                                                    _capitulosService.DesbloquearFase(faccao, fase);
                                                    _capitulosService.ConcluirFase(faccao, fase);
                                                    _campeoesService.DesbloquearCampeoes(faccao, fase);
                                                    Item? item = _arsenalService.DroparItem(faccao, fase);
                                                    _capitulosService.DesbloquearFaccao(faccao, fase);
                                                    _capitulosService.SalvarProgresso();
                                                    _arsenalService.SalvarItens();

                                                    Console.Clear();
                                                    Console.WriteLine("=====Fase Concluída!=====\n");

                                                    var novos = _campeoesService.ObterDesbloqueados().Except(antes).ToList();
                                                    foreach (Personagem p in novos)
                                                        Console.WriteLine($"Novo campeão: {p.Simbolo} {p.Nome}!");

                                                    if (item != null)
                                                        Console.WriteLine($"Novo item: {item.Simbolo} {item.Nome} | {item.NomeStat()} + {item.ValorFormatado()}");

                                                    Console.WriteLine("\nPressione Enter para continuar...");
                                                    Console.ReadLine();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            opcaoFase = ConsoleUtils.SelecionarComCursor(opcaoFase, 1, 7, keyFase.Key);
                                        }
                                    } while (!voltouFase);
                                }
                            }
                            else
                            {
                                opcaoFaccao = ConsoleUtils.SelecionarComCursor(opcaoFaccao, 1, 8, key.Key);
                            }
                        } while (!voltouCampanha);
                        break;

                    case 2:
                        _menuService.MenuInventario();
                        break;
                }
            } while (!sair);
        }

        #endregion
    }
}
