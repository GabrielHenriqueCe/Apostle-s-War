using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text;

namespace v1_Apostle_s_War.Services
{
    internal class MenuService
    {
        #region Construtor

        private readonly FaccaoService _faccaoService;
        private readonly ArsenalService _arsenalService;
        private readonly CapitulosService _capitulosService;
        #endregion


        public MenuService(FaccaoService faccaoService, ArsenalService arsenalService, CapitulosService capitulosService)
        {
            _faccaoService = faccaoService;
            _arsenalService = arsenalService;
            _capitulosService = capitulosService;
        }
        #region Menu

        public void ExibirMenu()
        {
            Console.Clear();
            Console.WriteLine("=====Apostle's War=====\n");
            Console.WriteLine("1 - Jogar Campanha");
            Console.WriteLine("2 - Inventario");
            Console.WriteLine("Esc - Sair");
            Console.WriteLine("\nDigite o número da opção desejada:");
        }

        public void MenuCapitulos()
        {
            Console.Clear();
            Console.WriteLine("=====Apostle's War=====\n");

            foreach (Faccao faccao in Enum.GetValues<Faccao>())
            {
                if (faccao == Faccao.Humanos) continue;

                string icone;

                if (!_capitulosService.EstaCapituloDesbloqueado(faccao))
                    icone = " ❌ ";
                else if (_capitulosService.CapituloConcluido(faccao))
                    icone = " ✅ ";
                else
                    icone = " ☑️  ";

                Console.WriteLine($"{(int)faccao} - {icone} {_faccaoService.ObterSimbolo(faccao)}  {_faccaoService.ObterNome(faccao)}");
            }

            Console.WriteLine("Esc - Voltar");
            Console.WriteLine("\nDigite o número da opção desejada:");
        }

        public void MenuFases(Faccao faccao)
        {
            Console.Clear();
            Console.WriteLine("=====Apostle's War=====\n");

            string[] nomes = { "Arma", "Elmo", "Escudo", "Manopla", "Peitoral", "Calça", "Bota" };

            foreach (Fases fase in Enum.GetValues<Fases>())
            {
                int idx = (int)fase - 1;
                string icone;

                if (!_capitulosService.EstaDesbloqueado(faccao, fase))
                    icone = " ❌ ";
                else if (_capitulosService.FaseConcluida(faccao, fase))
                    icone = " ✅ ";
                else
                    icone = " ☑️  ";

                Console.WriteLine($"{(int)fase} - {icone}  {nomes[idx]}");
            }

            Console.WriteLine("Esc - Voltar");
            Console.WriteLine("\nDigite o número da opção desejada:");
        }

        /// <summary>
        /// Exibe a lista de campeões desbloqueados para seleção do time
        /// </summary>
        public void MenuSelecaoTime(List<Personagem> desbloqueados)
        {
            Console.Clear();
            Console.WriteLine("=====Apostle's War=====\n");
            Console.WriteLine("Escolha 4 campeões para o seu time:\n");
            for (int i = 0; i < desbloqueados.Count; i++)
            {
                Jogador temp = new Jogador(desbloqueados[i]);
                _arsenalService.AplicarItens(temp);

                Console.WriteLine($"{i + 1} - {desbloqueados[i].Simbolo} {desbloqueados[i].Nome} | HP:{temp.HPAtual} ATK:{temp.Ataque} DEF:{temp.Defesa}");
            }
            Console.WriteLine("\nDigite o número do campeão desejado:");
        }

        /// <summary>
        /// Exibe os itens obtidos e permite ao jogador equipar um item no slot correspondente
        /// </summary>
        public void MenuInventario()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=====Inventário=====\n");
                Console.WriteLine("Itens equipados:");
                Item?[] itensEquipados = _arsenalService.ObterEquipados();
                for (int i = 0; i < itensEquipados.Length; i++)
                {
                    Item? item = itensEquipados[i];
                    if (item == null)
                        Console.WriteLine($"Slot {i + 1} - vazio");
                    else
                        Console.WriteLine($"Slot {i + 1} - {item.Simbolo} {item.Nome} ({item.Faccao}) {item.NomeStat()} {item.ValorFormatado()}");
                }

                Console.WriteLine("\nItens obtidos:");
                List<Item> obtidos = _arsenalService.ObterObtidos();
                for (int i = 0; i < obtidos.Count; i++)
                    Console.WriteLine($"{i + 1} - {obtidos[i].Simbolo} {obtidos[i].Nome} ({obtidos[i].Faccao}) {obtidos[i].NomeStat()} +{obtidos[i].ValorFormatado()}");

                Console.Write("Digite o número do item para equipar ou Esc para voltar: ");
                ConsoleKeyInfo first = Console.ReadKey(false);

                if (first.Key == ConsoleKey.Escape) break;

                string input = first.KeyChar + Console.ReadLine();

                if (int.TryParse(input, out int escolha) && escolha >= 1 && escolha <= obtidos.Count)
                {
                    _arsenalService.EquiparItem(obtidos[escolha - 1]);
                    Console.WriteLine($"\n{obtidos[escolha - 1].Simbolo} {obtidos[escolha - 1].Nome} equipado!");
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("Opção inválida.");
                    Console.ReadLine();
                }
            }
        }

        #endregion
    }
}
