namespace ApostlesWar
{
    #region Menu

    /// <summary>
    /// Gerencia a exibição e leitura de opções dos menus do jogo
    /// </summary>
    class Menu
    {
        // Opções de confirmação
        public enum SimOuNao { Sim = 1, Nao = 2 }
        // Opções do menu principal
        public enum OpcoesMenu { JogarCampanha = 1, Inventario = 2 }

        public static void ExibirMenu()
        {
            Console.Clear();
            Console.WriteLine("=====Apostle's War=====\n");
            Console.WriteLine("1 - Jogar Campanha");
            Console.WriteLine("2 - Inventario");
            Console.WriteLine("Esc - Sair");
            Console.WriteLine("\nDigite o número da opção desejada:");
        }

        /// <summary>
        /// Lê uma tecla do console e tenta converter para o enum informado. Retorna null se Esc for pressionado
        /// </summary>
        public static T? LerOpcao<T>() where T : struct
        {
            while (true)
            {
                ConsoleKeyInfo first = Console.ReadKey(false);

                if (first.Key == ConsoleKey.Escape)
                    return null;

                string input = first.KeyChar + Console.ReadLine();

                if (int.TryParse(input, out int opcao) && opcao > 0)
                {
                    var valor = (T)Enum.ToObject(typeof(T), opcao);
                    if (Enum.IsDefined(typeof(T), valor))
                        return valor;
                    else
                        Console.WriteLine("Opção inválida, digite uma opção válida.");
                }
                else
                {
                    Console.WriteLine("Opção inválida, digite uma opção válida.");
                }
            }
        }

        public static void MenuCapitulos()
        {
            Console.Clear();
            Console.WriteLine("=====Apostle's War=====\n");

            foreach (Faccao faccao in Enum.GetValues<Faccao>())
            {
                if (faccao == Faccao.Humanos) continue;

                Capitulos cap = Capitulos.ObterCapitulo(faccao);
                string icone;

                if (!cap.CapDesblock)
                    icone = " ❌ ";
                else if (cap.FaseConcluida.All(f => f))
                    icone = " ✅ ";
                else
                    icone = " ☑️  ";

                Console.WriteLine($"{(int)faccao} - {icone} {Faction.ObterSimbolo(faccao)}  {Faction.ObterNome(faccao)}");
            }

            Console.WriteLine("Esc - Voltar");
            Console.WriteLine("\nDigite o número da opção desejada:");
        }

        public static void MenuFases(Faccao faccao)
        {
            Console.Clear();
            Console.WriteLine("=====Apostle's War=====\n");

            Capitulos cap = Capitulos.ObterCapitulo(faccao);
            string[] nomes = { "Arma", "Elmo", "Escudo", "Manopla", "Peitoral", "Calça", "Bota" };

            foreach (Fases fase in Enum.GetValues<Fases>())
            {
                int idx = (int)fase - 1;
                string icone;

                if (!cap.FaseDesblock[idx])
                    icone = " ❌ ";
                else if (cap.FaseConcluida[idx])
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
        public static void MenuSelecaoTime(List<Personagem> desbloqueados)
        {
            Console.Clear();
            Console.WriteLine("=====Apostle's War=====\n");
            Console.WriteLine("Escolha 4 campeões para o seu time:\n");
            for (int i = 0; i < desbloqueados.Count; i++)
            {
                Jogador temp = new Jogador(desbloqueados[i]);
                Arsenal.AplicarItens(temp);

                Console.WriteLine($"{i + 1} - {desbloqueados[i].Simbolo} {desbloqueados[i].Nome} | HP:{temp.HPAtual} ATK:{temp.Ataque} DEF:{temp.Defesa}");
            }
            Console.WriteLine("\nDigite o número do campeão desejado:");
        }

        /// <summary>
        /// Exibe os itens obtidos e permite ao jogador equipar um item no slot correspondente
        /// </summary>
        public static void MenuInventario()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=====Inventário=====\n");
                Console.WriteLine("Itens equipados:");
                Item?[] equipados = Arsenal.ObterEquipados();
                for (int i = 0; i < equipados.Length; i++)
                {
                    Item? item = equipados[i];
                    if (item == null)
                        Console.WriteLine($"Slot {i + 1} - vazio");
                    else
                        Console.WriteLine($"Slot {i + 1} - {item.Simbolo} {item.Nome} ({item.Faccao}) {item.NomeStat()} {item.ValorFormatado()}");
                }

                Console.WriteLine("\nItens obtidos:");
                List<Item> obtidos = Arsenal.ObterObtidos();
                for (int i = 0; i < obtidos.Count; i++)
                    Console.WriteLine($"{i + 1} - {obtidos[i].Simbolo} {obtidos[i].Nome} ({obtidos[i].Faccao}) {obtidos[i].NomeStat()} +{obtidos[i].ValorFormatado()}");

                Console.Write("Digite o número do item para equipar ou Esc para voltar: ");
                ConsoleKeyInfo first = Console.ReadKey(false);

                if (first.Key == ConsoleKey.Escape) break;

                string input = first.KeyChar + Console.ReadLine();

                if (int.TryParse(input, out int escolha) && escolha >= 1 && escolha <= obtidos.Count)
                {
                    Arsenal.EquiparItem(obtidos[escolha - 1]);
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
    }

    #endregion
}