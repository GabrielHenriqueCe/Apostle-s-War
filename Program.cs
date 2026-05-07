using System;
using System.Collections.Generic;
using System.Text.Json;
using ApostlesWar;

#region Capitulos

bool EstaDesbloqueado(Faccao faccao, Fases fase)
{
    Capitulos cap = ObterCapitulo(faccao);
    return cap.CapDesblock && cap.FaseDesblock[(int)fase - 1];
}

#endregion

#region Menu

void ExibirMenu()
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
T? LerOpcao<T>() where T : struct
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

void MenuCapitulos()
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

void MenuFases(Faccao faccao)
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

#endregion

#region Capitulos

/// <summary>
/// Carrega o progresso salvo em arquivo, restaurando capítulos, campeões desbloqueados e itens obtidos
/// </summary>
void CarregarProgresso()
{
    if (File.Exists("save.txt"))
    {
        var json = File.ReadAllText("save.txt");
        var lista = JsonSerializer.Deserialize<List<Capitulos>>(json);

        if (lista != null)
        {
            capitulos.Clear();
            capitulos.AddRange(lista);
        }

        foreach (Capitulos cap in capitulos)
            foreach (Fases fase in Enum.GetValues<Fases>())
                if (cap.FaseConcluida[(int)fase - 1])
                    Campeoes.DesbloquearCampeoes(cap.Faccao, fase);
        Arsenal.CarregarItens();
    }
}

#endregion
#region Arsenal

/// <summary>
/// Restaura os itens equipados a partir do arquivo salvo anteriormente
/// </summary>
void CarregarItensEquipados()
{
    if (File.Exists("itens.txt"))
    {
        var json = File.ReadAllText("itens.txt");
        var lista = JsonSerializer.Deserialize<Item?[]>(json);
        if (lista != null)
            for (int i = 0; i < lista.Length; i++)
                equipados[i] = lista[i];
    }
}

#endregion

#region GerenciadorDeJogo

void Executar()
{
    bool sair = false;
    CarregarItensEquipados();
    CarregarProgresso();

    do
    {
        ExibirMenu();
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
                    MenuCapitulos();
                    faccao = LerOpcao<Faccao>();
                    if (faccao == null) break;
                    if (EstaDesbloqueado(faccao.Value))
                    {
                        Fases? fases;
                        do
                        {
                            MenuFases(faccao.Value);
                            fases = LerOpcao<Fases>();
                            if (fases == null) break;
                            if (EstaDesbloqueado(faccao.Value, fases.Value))
                            {
                                if (ExecutarFase(faccao.Value, fases.Value))
                                {
                                    var antes = ObterDesbloqueados().ToList();

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
            case OpcoesMenu.Inventario:
                MenuInventario();
                break;
            default:
                Console.WriteLine("Opção inválida, tente novamente.");
                break;
        }
    } while (!sair);
}

/// <summary>
/// Monta e executa as duas rodadas de uma fase, retornando true se o jogador vencer ambas
/// </summary>
bool ExecutarFase(Faccao capitulo, Fases fase)
{
    Capitulos cap = Capitulos.ObterCapitulo(capitulo);
    Fase fas = Campanha.ObterFase((int)fase);
    MultiplicadorFase mult = new MultiplicadorFase
    {
        HP = (0.5f * (float)capitulo) + (0.1f * (float)fase),
        Ataque = (0.5f * (float)capitulo) + (0.1f * (float)fase),
        Defesa = (0.5f * (float)capitulo) + (0.1f * (float)fase)
    };

    var time = Campeoes.SelecionarTime();
    var jogador = time.Select(p => (Combate)new Jogador(p)).ToList();

    foreach (Combate c in jogador)
        Arsenal.AplicarItens(c);

    var inimigo = new List<Combate>();
    var combatentes = new List<Combate>();

    foreach (Slot slot in fas.Rodada1)
        inimigo.Add(new Inimigo(SelecaoSimbolo.ObterPersonagem(cap.Faccao, slot), mult));

    combatentes.AddRange(jogador);
    combatentes.AddRange(inimigo);

    if (!ExecutarCombate(jogador, inimigo, combatentes)) return false;

    inimigo.Clear();
    combatentes.Clear();

    foreach (Slot slot in fas.Rodada2)
        inimigo.Add(new Inimigo(SelecaoSimbolo.ObterPersonagem(cap.Faccao, slot), mult));

    combatentes.AddRange(jogador);
    combatentes.AddRange(inimigo);

    return ExecutarCombate(jogador, inimigo, combatentes);
}

#endregion

#region Program

/// <summary>
/// Ponto de entrada do jogo
/// </summary>

Console.OutputEncoding = System.Text.Encoding.UTF8;
Executar();

#endregion
