using System;
using System.Collections.Generic;
using System.Text.Json;
using ApostlesWar;

// 7 slots de equipamento (um por fase), null = vazio
Item?[] equipados = new Item?[7];

#region Skill

/// <summary>
/// Retorna o emoji de relógio proporcional ao progresso do cooldown
/// </summary>
string ObterRelogio(int turnosRestantes, int cooldownTotal)
{
    if (turnosRestantes == 0) return "🕛";

    string[] relogios = { "🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙" };
    int turnosPassados = cooldownTotal - turnosRestantes;
    int indice = (int)Math.Round((double)turnosPassados * 9 / cooldownTotal) - 1;
    indice = Math.Clamp(indice, 0, 8);
    return relogios[indice];
}

#endregion

#region Capitulos

List<Capitulos> capitulos = new List<Capitulos>
        {
            new Capitulos(Faccao.Reino, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, true),
            new Capitulos(Faccao.LadoSombrio, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
            new Capitulos(Faccao.Tecnologicos, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
            new Capitulos(Faccao.Folclore, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
            new Capitulos(Faccao.Misticos, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
            new Capitulos(Faccao.Especial, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
            new Capitulos(Faccao.Decaidos, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
            new Capitulos(Faccao.Apostolos, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
        };

Capitulos ObterCapitulo(Faccao faccao)
{
    return capitulos.First(c => c.Faccao == faccao);
}

bool EstaCapituloDesbloqueado(Faccao faccao) => ObterCapitulo(faccao).CapDesblock;

void DesbloquearFase(Faccao faccao, Fases fase)
{
    Fases ultima = Enum.GetValues<Fases>().Last();
    if (fase == ultima) return;
    Capitulos cap = ObterCapitulo(faccao);
    cap.FaseDesblock[(int)fase] = true;
}

void ConcluirFase(Faccao faccao, Fases fase)
{
    Capitulos cap = ObterCapitulo(faccao);
    cap.FaseConcluida[(int)fase - 1] = true;
}

/// <summary>
/// Desbloqueia a próxima facção se todas as fases do capítulo atual estiverem concluídas
/// </summary>
void DesbloquearFaccao(Faccao faccao, Fases fase)
{
    Capitulos cap = ObterCapitulo(faccao);
    if (cap.FaseDesblock.All(f => f))
    {
        Faccao ultima = Enum.GetValues<Faccao>().Last();
        if (faccao == ultima) return;
        Faccao proxima = (Faccao)((int)faccao + 1);
        ObterCapitulo(proxima).CapDesblock = true;
    }
}

void SalvarProgresso()
{
    var json = JsonSerializer.Serialize(capitulos);
    File.WriteAllText("save.txt", json);
}

List<Capitulos> ObterTodos() => capitulos;

bool EstaDesbloqueado(Faccao faccao, Fases fase)
{
    Capitulos cap = ObterCapitulo(faccao);
    return cap.CapDesblock && cap.FaseDesblock[(int)fase - 1];
}

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
                    DesbloquearCampeoes(cap.Faccao, fase);
        CarregarItens();
    }
}

#endregion

#region SelecaoSimbolo

List<Personagem> personagens = new List<Personagem>
        {
            // Humanos
            new Personagem(1, Faccao.Humanos, "Operário", "👷",  1200, 240, 120),
            new Personagem(2, Faccao.Humanos, "Detetive", "🕵️", 1400, 160, 160),
            new Personagem(3, Faccao.Humanos, "Policial", "👮",  1000, 120, 280),
            new Personagem(4, Faccao.Humanos, "Sushiman ", "👲",  800, 280, 160),
            
            // O Reino
            new Personagem(1, Faccao.Reino, "Guarda", "💂", 1200, 160, 200, new Necromancia()),
            new Personagem(2, Faccao.Reino, "Ninja", "🥷", 600, 280, 200),
            new Personagem(3, Faccao.Reino, "Mago", "🧙", 1000, 280, 120),
            new Personagem(4, Faccao.Reino, "Rei", "🫅", 1000, 200, 200),

            // Lado Sombrio
            new Personagem(1, Faccao.LadoSombrio, "Caveira", "💀",  600, 280, 200),
            new Personagem(2, Faccao.LadoSombrio, "Fantasma", "👻", 1400, 120, 200),
            new Personagem(3, Faccao.LadoSombrio, "Abóbora", "🎃",  600, 200, 280),
            new Personagem(4, Faccao.LadoSombrio, "Zumbi", "🧟", 1400, 200, 120),

            // Tecnológicos
            new Personagem(1, Faccao.Tecnologicos, "Invasor", "👾",  600, 240, 240),
            new Personagem(2, Faccao.Tecnologicos, "Alien", "👽", 1200, 240, 120),
            new Personagem(3, Faccao.Tecnologicos, "Robô", "🤖", 1200, 120, 240),
            new Personagem(4, Faccao.Tecnologicos, "Cientista", "🧑‍🔬", 1000, 200, 200),

            // Folclore
            new Personagem(1, Faccao.Folclore, "Ogro", "👹", 1400, 160, 160),
            new Personagem(2, Faccao.Folclore, "Tengu", "👺",  800, 280, 160),
            new Personagem(3, Faccao.Folclore, "Palhaço", "🤡",  800, 160, 280),
            new Personagem(4, Faccao.Folclore, "Troll", "🧌", 1200, 160, 200),

            // Místicos
            new Personagem(1, Faccao.Misticos, "Gênio", "🧞", 1400, 120, 200),
            new Personagem(2, Faccao.Misticos, "Sereia", "🧜",  600, 280, 200),
            new Personagem(3, Faccao.Misticos, "Fada", "🧚", 1000, 280, 120),
            new Personagem(4, Faccao.Misticos, "Dragão", "🐲", 1400, 200, 120),

            // Especial
            new Personagem(1, Faccao.Especial, "Cocô", "💩", 1200, 160, 200),
            new Personagem(2, Faccao.Especial, "Herói", "🦸",  800, 240, 200),
            new Personagem(3, Faccao.Especial, "Vilão", "🦹", 1200, 200, 160),
            new Personagem(4, Faccao.Especial, "T-Rex", "🦖", 1000, 160, 240),

            // Decaídos 
            new Personagem(1, Faccao.Decaidos, "Morcego", "🦇",  800, 160, 280),
            new Personagem(2, Faccao.Decaidos, "Vampiro", "🧛",  800, 280, 160),
            new Personagem(3, Faccao.Decaidos, "Elfo", "🧝", 1400, 160, 160),
            new Personagem(4, Faccao.Decaidos, "Diabo", "😈", 1400, 160, 160),

            // Apóstolos
            new Personagem(1, Faccao.Apostolos, "Boneco de Neve", "☃️",  600, 240, 240),
            new Personagem(2, Faccao.Apostolos, "Mímico", "🎭", 1200, 240, 120),
            new Personagem(3, Faccao.Apostolos, "Anjo", "👼", 1200, 120, 240),
            new Personagem(4, Faccao.Apostolos, "Papai Noel", "🎅", 1400, 160, 160),
        };

/// <summary>
/// Retorna o personagem correspondente à facção e slot informados
/// </summary>
/// <param name="faccao">Facção do personagem</param>
/// <param name="slot">Slot do personagem dentro da facção</param>
Personagem ObterPersonagem(Faccao faccao, Slot slot)
{
    return personagens.First(p => p.Faccao == faccao && p.Slot == (int)slot);
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

        Capitulos cap = ObterCapitulo(faccao);
        string icone;

        if (!cap.CapDesblock)
            icone = " ❌ ";
        else if (cap.FaseConcluida.All(f => f))
            icone = " ✅ ";
        else
            icone = " ☑️  ";

        Console.WriteLine($"{(int)faccao} - {icone} {ObterSimbolo(faccao)}  {ObterNome(faccao)}");
    }

    Console.WriteLine("Esc - Voltar");
    Console.WriteLine("\nDigite o número da opção desejada:");
}

void MenuFases(Faccao faccao)
{
    Console.Clear();
    Console.WriteLine("=====Apostle's War=====\n");

    Capitulos cap = ObterCapitulo(faccao);
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
void MenuSelecaoTime(List<Personagem> desbloqueados)
{
    Console.Clear();
    Console.WriteLine("=====Apostle's War=====\n");
    Console.WriteLine("Escolha 4 campeões para o seu time:\n");
    for (int i = 0; i < desbloqueados.Count; i++)
    {
        Jogador temp = new Jogador(desbloqueados[i]);
        AplicarItens(temp);

        Console.WriteLine($"{i + 1} - {desbloqueados[i].Simbolo} {desbloqueados[i].Nome} | HP:{temp.HPAtual} ATK:{temp.Ataque} DEF:{temp.Defesa}");
    }
    Console.WriteLine("\nDigite o número do campeão desejado:");
}

/// <summary>
/// Exibe os itens obtidos e permite ao jogador equipar um item no slot correspondente
/// </summary>
void MenuInventario()
{
    while (true)
    {
        Console.Clear();
        Console.WriteLine("=====Inventário=====\n");
        Console.WriteLine("Itens equipados:");
        Item?[] itensEquipados = ObterEquipados();
        for (int i = 0; i < itensEquipados.Length; i++)
        {
            Item? item = itensEquipados[i];
            if (item == null)
                Console.WriteLine($"Slot {i + 1} - vazio");
            else
                Console.WriteLine($"Slot {i + 1} - {item.Simbolo} {item.Nome} ({item.Faccao}) {item.NomeStat()} {item.ValorFormatado()}");
        }

        Console.WriteLine("\nItens obtidos:");
        List<Item> obtidos = ObterObtidos();
        for (int i = 0; i < obtidos.Count; i++)
            Console.WriteLine($"{i + 1} - {obtidos[i].Simbolo} {obtidos[i].Nome} ({obtidos[i].Faccao}) {obtidos[i].NomeStat()} +{obtidos[i].ValorFormatado()}");

        Console.Write("Digite o número do item para equipar ou Esc para voltar: ");
        ConsoleKeyInfo first = Console.ReadKey(false);

        if (first.Key == ConsoleKey.Escape) break;

        string input = first.KeyChar + Console.ReadLine();

        if (int.TryParse(input, out int escolha) && escolha >= 1 && escolha <= obtidos.Count)
        {
            EquiparItem(obtidos[escolha - 1]);
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

#region Campanha

List<Fase> fases = new List<Fase>
        {
            new Fase(new List<Slot> { Slot.Slot1 }, new List<Slot> { Slot.Slot2 }),
            new Fase(new List<Slot> { Slot.Slot1, Slot.Slot1 }, new List<Slot> { Slot.Slot1, Slot.Slot2 }),
            new Fase(new List<Slot> { Slot.Slot2, Slot.Slot2 }, new List<Slot> { Slot.Slot2, Slot.Slot3 }),
            new Fase(new List<Slot> { Slot.Slot3, Slot.Slot1 }, new List<Slot> { Slot.Slot2, Slot.Slot3, Slot.Slot3 }),
            new Fase(new List<Slot> { Slot.Slot2, Slot.Slot3, Slot.Slot2 }, new List<Slot> { Slot.Slot3, Slot.Slot1, Slot.Slot3 }),
            new Fase(new List<Slot> { Slot.Slot1, Slot.Slot3, Slot.Slot1 }, new List<Slot> { Slot.Slot1, Slot.Slot3, Slot.Slot3, Slot.Slot1 }),
            new Fase(new List<Slot> { Slot.Slot2, Slot.Slot3, Slot.Slot2 }, new List<Slot> { Slot.Slot1, Slot.Slot3, Slot.Slot4, Slot.Slot2 }),
        };

Fase ObterFase(int numero)
{
    return fases[numero - 1];
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

// Itens obtidos ao longo da campanha
List<Item> obtidos = new List<Item>();

Dictionary<Faccao, string[]> simbolosPorFaccao = new Dictionary<Faccao, string[]>
        {
            { Faccao.Reino,        new[] { "🗡️", "👑", "🛡️", "📿", "👔", "👖", "👞" } },
            { Faccao.LadoSombrio,  new[] { "🏹", "🕶️", "💼", "🦯", "🥋", "🦽", "👟" } },
            { Faccao.Tecnologicos, new[] { "🔫", "🥽", "🧰", "🦾", "🥼", "🦿", "🛼" } },
            { Faccao.Folclore,     new[] { "🪃", "🧢", "🧳", "🥊", "🎽", "🩳", "🩴" } },
            { Faccao.Misticos,     new[] { "🪭", "👒", "👛", "💅", "🥻", "👙", "👠" } },
            { Faccao.Decaidos,     new[] { "🪄", "🎩", "🎒", "🩼", "🧥", "🦼", "🥾" } },
            { Faccao.Apostolos,    new[] { "🎄", "🧣", "🔔", "🧤", "👘", "🩲", "🪽" } },
        };

/// <summary>
/// Adiciona um item à lista de obtidos ao concluir uma fase
/// </summary>
void DroparItem(Faccao faccao, Fases fase)
{
    string simbolo = simbolosPorFaccao[faccao][(int)fase - 1];

    (string nome, TipoStat tipo) = fase switch
    {
        Fases.Fase1 => ("Arma", TipoStat.ATKFlat),
        Fases.Fase2 => ("Elmo", TipoStat.HPFlat),
        Fases.Fase3 => ("Escudo", TipoStat.DEFFlat),
        Fases.Fase4 => ("Manopla", TipoStat.TaxaCritPct),
        Fases.Fase5 => ("Peitoral", TipoStat.HPPct),
        Fases.Fase6 => ("Calça", TipoStat.DEFPct),
        Fases.Fase7 => ("Bota", TipoStat.DanoCritPct),
        _ => throw new ArgumentOutOfRangeException()
    };
    obtidos.Add(new Item(nome, simbolo, faccao, fase, tipo));
}

/// <summary>
/// Equipa um item no slot correspondente à sua fase, substituindo o anterior
/// </summary>
void EquiparItem(Item item)
{
    equipados[(int)item.Fase - 1] = item;
}

/// <summary>
/// Retorna os itens atualmente equipados
/// </summary>
Item?[] ObterEquipados() => equipados;

/// <summary>
/// Retorna todos os itens obtidos
/// </summary>
List<Item> ObterObtidos() => obtidos;

/// <summary>
/// Carrega itens a partir do FaseConcluida igual ao CarregarCampeoes
/// </summary>
void CarregarItens()
{
    foreach (Capitulos cap in ObterTodos())
    {
        foreach (Fases fase in Enum.GetValues<Fases>())
        {
            if (cap.FaseConcluida[(int)fase - 1])
            {
                DroparItem(cap.Faccao, fase);
            }
        }
    }
}

/// <summary>
/// Aplica os stats dos itens equipados ao combatente informado
/// </summary>
void AplicarItens(Combate combate)
{
    foreach (Item? item in equipados)
    {
        if (item == null) continue;
        combate.AplicarItem(item);
    }
}

/// <summary>
/// Serializa e persiste os itens equipados em arquivo para restauração futura
/// </summary>
void SalvarItens()
{
    var json = JsonSerializer.Serialize(equipados);
    File.WriteAllText("itens.txt", json);
}

#endregion

#region Campeoes

List<Personagem> desbloqueados = new List<Personagem>
        {
            ObterPersonagem(Faccao.Humanos, Slot.Slot1),
            ObterPersonagem(Faccao.Humanos, Slot.Slot2),
            ObterPersonagem(Faccao.Humanos, Slot.Slot3),
            ObterPersonagem(Faccao.Humanos, Slot.Slot4),
        };

void DesbloquearCampeoes(Faccao faccao, Fases fase)
{
    Fase fas = ObterFase((int)fase);

    foreach (Slot slot in fas.Rodada1)
    {
        Personagem p = ObterPersonagem(faccao, slot);
        if (!desbloqueados.Contains(p))
            desbloqueados.Add(p);
    }

    foreach (Slot slot in fas.Rodada2)
    {
        Personagem p = ObterPersonagem(faccao, slot);
        if (!desbloqueados.Contains(p))
            desbloqueados.Add(p);
    }
}

List<Personagem> ObterDesbloqueados() => desbloqueados;

/// <summary>
/// Solicita ao jogador a escolha de 4 campeões sem repetição e retorna o time selecionado
/// </summary>
List<Personagem> SelecionarTime()
{
    var time = new List<Personagem>();
    var desbloqueados = ObterDesbloqueados();

    while (time.Count < 4)
    {
        MenuSelecaoTime(desbloqueados);
        Console.WriteLine($"Slot {time.Count + 1}/4 — já selecionados: {string.Join(" ", time.Select(p => p.Simbolo))}");

        if (int.TryParse(Console.ReadLine(), out int escolha) && escolha >= 1 && escolha <= desbloqueados.Count)
        {
            Personagem selecionado = desbloqueados[escolha - 1];
            if (time.Contains(selecionado))
                Console.WriteLine("Campeão já selecionado, escolha outro.");
            else
                time.Add(selecionado);
        }
        else
        {
            Console.WriteLine("Opção inválida.");
        }
    }
    return time;
}

#endregion

#region Faction

Dictionary<Faccao, (string Nome, string Simbolo)> mapa = new Dictionary<Faccao, (string, string)>
        {
            { Faccao.Humanos, ("Humanos", "🛠️") },
            { Faccao.Reino, ("Reino", "👑") },
            { Faccao.LadoSombrio, ("Lado Sombrio", "🌑") },
            { Faccao.Tecnologicos, ("Tecnológicos", "⚙️") },
            { Faccao.Folclore, ("Folclore", "🪬") },
            { Faccao.Misticos, ("Místicos", "🐉") },
            { Faccao.Especial, ("Especial", "⭐") },
            { Faccao.Decaidos, ("Decaídos", "🔱") },
            { Faccao.Apostolos, ("Apóstolos", "🌬️") }
        };
string ObterSimbolo(Faccao faccao) => mapa[faccao].Simbolo;
string ObterNome(Faccao faccao) => mapa[faccao].Nome;

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
                    if (EstaCapituloDesbloqueado(faccao.Value))
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

                                    DesbloquearFase(faccao.Value, fases.Value);
                                    ConcluirFase(faccao.Value, fases.Value);
                                    DesbloquearCampeoes(faccao.Value, fases.Value);
                                    DroparItem(faccao.Value, fases.Value);
                                    DesbloquearFaccao(faccao.Value, fases.Value);
                                    SalvarProgresso();
                                    SalvarItens();

                                    Console.Clear();
                                    Console.WriteLine("=====Fase Concluída!=====\n");

                                    var novos = ObterDesbloqueados().Except(antes).ToList();
                                    foreach (Personagem p in novos)
                                        Console.WriteLine($"Novo campeão: {p.Simbolo} {p.Nome}!");

                                    Item? item = ObterObtidos().LastOrDefault();
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
    Capitulos cap = ObterCapitulo(capitulo);
    Fase fas = ObterFase((int)fase);
    MultiplicadorFase mult = new MultiplicadorFase
    {
        HP = (0.5f * (float)capitulo) + (0.1f * (float)fase),
        Ataque = (0.5f * (float)capitulo) + (0.1f * (float)fase),
        Defesa = (0.5f * (float)capitulo) + (0.1f * (float)fase)
    };

    var time = SelecionarTime();
    var jogador = time.Select(p => (Combate)new Jogador(p)).ToList();

    foreach (Combate c in jogador)
        AplicarItens(c);
    var inimigo = new List<Combate>();
    var combatentes = new List<Combate>();

    foreach (Slot slot in fas.Rodada1)
        inimigo.Add(new Inimigo(ObterPersonagem(cap.Faccao, slot), mult));

    combatentes.AddRange(jogador);
    combatentes.AddRange(inimigo);

    if (!ExecutarCombate(jogador, inimigo, combatentes)) return false;

    inimigo.Clear();
    combatentes.Clear();

    foreach (Slot slot in fas.Rodada2)
        inimigo.Add(new Inimigo(ObterPersonagem(cap.Faccao, slot), mult));

    combatentes.AddRange(jogador);
    combatentes.AddRange(inimigo);

    return ExecutarCombate(jogador, inimigo, combatentes);
}

Random random = new Random();

/// <summary>
/// Exibe os alvos vivos, lê a escolha do jogador ou sorteia um alvo para o inimigo, executa o ataque e exibe o HP resultante
/// </summary>
void ExecutarTurno(Combate atacante, List<Combate> defensor)
{
    Console.Clear();
    Console.WriteLine($"{atacante.Personagem.Simbolo} ataca! | HP:{atacante.HPAtual} ATK:{atacante.Ataque} DEF:{atacante.Defesa}");
    Console.WriteLine("Alvos disponíveis:");
    for (int i = 0; i < defensor.Count; i++)
    {
        if (defensor[i].EstaVivo())
            Console.WriteLine($"{i + 1} - {defensor[i].Personagem.Simbolo} | HP:{defensor[i].HPAtual} ATK:{defensor[i].Ataque} DEF:{defensor[i].Defesa}");
    }

    int alvo = 0;
    if (atacante is Jogador)
    {
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out alvo) && alvo >= 1 && alvo <= defensor.Count && defensor[alvo - 1].EstaVivo())
            {
                break;
            }
            else
            {
                Console.WriteLine($"Digite entre 1 e {defensor.Count}, o alvo precisa estar vivo");
            }
        }
    }
    else if (atacante is Inimigo)
    {
        while (true)
        {
            alvo = random.Next(1, defensor.Count + 1);
            if (alvo >= 1 && alvo <= defensor.Count && defensor[alvo - 1].EstaVivo())
            {
                break;
            }
        }
    }

    if (atacante is Inimigo)
    {
        Console.WriteLine($"{atacante.Personagem.Simbolo} prepara o ataque!");
        Thread.Sleep(1500);
    }

    atacante.Atacar(defensor[alvo - 1]);
    Console.WriteLine($"hp atual do {defensor[alvo - 1].Personagem.Simbolo} é de {defensor[alvo - 1].HPAtual}");

    var alvoAtacado = defensor[alvo - 1];
    if (!alvoAtacado.EstaVivo()
        && alvoAtacado.Personagem.Habilidade is HabilidadePassiva passiva
        && passiva.DeveAtivar(EventoCombate.DepoisDeReceberDano)
        && passiva.Cooldown.Disponivel)
    {
        passiva.Ativar(alvoAtacado);
        passiva.Cooldown.Usar();
        if (passiva.Revive())
        {
            Console.WriteLine(passiva.MensagemSobreviveu(alvoAtacado.Personagem));
            Thread.Sleep(2000);
        }
        else
        {
            Console.WriteLine(passiva.MensagemMorreu(alvoAtacado.Personagem));
            Thread.Sleep(2000);
        }
    }

    if (atacante is Inimigo)
    {
        Thread.Sleep(1500);
    }
}

/// <summary>
/// Itera pelos combatentes em ordem, alternando ataques entre jogador e inimigo, até que um dos lados seja completamente eliminado.
/// Retorna true se o jogador vencer
/// </summary>
bool ExecutarCombate(List<Combate> jogador, List<Combate> inimigo, List<Combate> combatentes)
{
    do
    {
        for (int c = 0; c < combatentes.Count; c++)
        {
            if (!combatentes[c].EstaVivo()) continue;
            if (!inimigo.Any(i => i.EstaVivo()) || !jogador.Any(j => j.EstaVivo()))
            { break; }
            if (combatentes[c] is Jogador)
            {
                ExecutarTurno(combatentes[c], inimigo);
                combatentes[c].Personagem.Habilidade?.Cooldown.PassarTurno();
            }
            else
            {
                ExecutarTurno(combatentes[c], jogador);
                combatentes[c].Personagem.Habilidade?.Cooldown.PassarTurno();
            }
        }
    } while (jogador.Any(j => j.EstaVivo()) && inimigo.Any(i => i.EstaVivo()));

    return jogador.Any(j => j.EstaVivo());
}

#endregion

#region Program

/// <summary>
/// Ponto de entrada do jogo
/// </summary>

Console.OutputEncoding = System.Text.Encoding.UTF8;
Executar();

#endregion
