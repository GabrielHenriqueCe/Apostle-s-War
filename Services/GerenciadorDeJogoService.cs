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
                _menuService.ExibirMenu();
                OpcoesMenu? opcao = LerOpcao<OpcoesMenu>();

                if (opcao == null)
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
                    continue;
                }

                switch (opcao.Value)
                {
                    case OpcoesMenu.JogarCampanha:
                        Faccao? faccao;
                        do
                        {
                            _menuService.MenuCapitulos();
                            faccao = LerOpcao<Faccao>();
                            if (faccao == null) break;
                            if (_capitulosService.EstaCapituloDesbloqueado(faccao.Value))
                            {
                                Fases? fases;
                                do
                                {
                                    _menuService.MenuFases(faccao.Value);
                                    fases = LerOpcao<Fases>();
                                    if (fases == null) break;
                                    if (_capitulosService.EstaDesbloqueado(faccao.Value, fases.Value))
                                    {
                                        if (_combateService.ExecutarFase(faccao.Value, fases.Value))
                                        {
                                            var antes = _campeoesService.ObterDesbloqueados().ToList();

                                            _capitulosService.DesbloquearFase(faccao.Value, fases.Value);
                                            _capitulosService.ConcluirFase(faccao.Value, fases.Value);
                                            _campeoesService.DesbloquearCampeoes(faccao.Value, fases.Value);
                                            _arsenalService.DroparItem(faccao.Value, fases.Value);
                                            _capitulosService.DesbloquearFaccao(faccao.Value, fases.Value);
                                            _capitulosService.SalvarProgresso();
                                            _arsenalService.SalvarItens();

                                            Console.Clear();
                                            Console.WriteLine("=====Fase Concluída!=====\n");

                                            var novos = _campeoesService.ObterDesbloqueados().Except(antes).ToList();
                                            foreach (Personagem p in novos)
                                                Console.WriteLine($"Novo campeão: {p.Simbolo} {p.Nome}!");

                                            Item? item = _arsenalService.ObterObtidos().LastOrDefault();
                                            if (item != null)
                                                Console.WriteLine($"Novo item: {item.Simbolo} {item.Nome} | {item.NomeStat()} + {item.ValorFormatado()}");

                                            Console.WriteLine("\nPressione Enter para continuar...");
                                            Console.ReadLine();
                                        }
                                    }
                                } while (true);
                            }
                        } while (true);

                        break;
                    case OpcoesMenu.Inventario:
                        _menuService.MenuInventario();
                        break;
                    default:
                        Console.WriteLine("Opção inválida, tente novamente.");
                        break;
                }
            } while (!sair);
        }

        #endregion
    }
}
