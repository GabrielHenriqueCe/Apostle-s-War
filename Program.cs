using System;
using System.Collections.Generic;
using System.Text.Json;

#region GerenciadorDeJogo

void Executar()
{
    bool sair = false;
    Arsenal.CarregarItensEquipados();
    Capitulos.CarregarProgresso();

    do
    {
        Menu.ExibirMenu();
        Menu.OpcoesMenu? opcao = Menu.LerOpcao<Menu.OpcoesMenu>();

        if (opcao == null)
        {
            Console.Clear();
            Console.WriteLine("Deseja sair do jogo? 1 - Sim | 2 - Não");
            Menu.SimOuNao? escolha = Menu.LerOpcao<Menu.SimOuNao>();
            if (escolha == Menu.SimOuNao.Sim)
            {
                Console.WriteLine("Obrigado por jogar! Até a próxima!");
                Console.ReadLine();
                sair = true;
            }
            continue;
        }

        switch (opcao.Value)
        {
            case Menu.OpcoesMenu.JogarCampanha:
                Faccao? faccao;
                do
                {
                    Menu.MenuCapitulos();
                    faccao = Menu.LerOpcao<Faccao>();
                    if (faccao == null) break;
                    if (Capitulos.EstaDesbloqueado(faccao.Value))
                    {
                        Fases? fases;
                        do
                        {
                            Menu.MenuFases(faccao.Value);
                            fases = Menu.LerOpcao<Fases>();
                            if (fases == null) break;
                            if (Capitulos.EstaDesbloqueado(faccao.Value, fases.Value))
                            {
                                if (GerenciadorDeJogo.ExecutarFase(faccao.Value, fases.Value))
                                {
                                    var antes = Campeoes.ObterDesbloqueados().ToList();

                                    Capitulos.DesbloquearFase(faccao.Value, fases.Value);
                                    Capitulos.ConcluirFase(faccao.Value, fases.Value);
                                    Campeoes.DesbloquearCampeoes(faccao.Value, fases.Value);
                                    Arsenal.DroparItem(faccao.Value, fases.Value);
                                    Capitulos.DesbloquearFaccao(faccao.Value, fases.Value);
                                    Capitulos.SalvarProgresso();
                                    Arsenal.SalvarItens();

                                    Console.Clear();
                                    Console.WriteLine("=====Fase Concluída!=====\n");

                                    var novos = Campeoes.ObterDesbloqueados().Except(antes).ToList();
                                    foreach (Personagem p in novos)
                                        Console.WriteLine($"Novo campeão: {p.Simbolo} {p.Nome}!");

                                    Item? item = Arsenal.ObterObtidos().LastOrDefault();
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
            case Menu.OpcoesMenu.Inventario:
                Menu.MenuInventario();
                break;
            default:
                Console.WriteLine("Opção inválida, tente novamente.");
                break;
        }
    } while (!sair);
}


#endregion
#region Program

/// <summary>
/// Ponto de entrada do jogo
/// </summary>

Console.OutputEncoding = System.Text.Encoding.UTF8;
Executar();

#endregion
