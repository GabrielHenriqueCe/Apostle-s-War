using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ApostlesWar
{
    #region Enums
    // Facções disponíveis no jogo
    enum Faccao { Humanos, Reino, LadoSombrio, Tecnologicos, Folclore, Misticos, Especial, Decaidos, Apostolos }
    // Posição do personagem dentro da facção
    enum Slot { Slot1 = 1, Slot2 = 2, Slot3 = 3, Slot4 = 4 }
    // Fases de cada capítulo: Arma, Elmo, Escudo, Braceletes, Armadura, Botas, Raro
    enum Fases { Fase1 = 1, Fase2 = 2, Fase3 = 3, Fase4 = 4, Fase5 = 5, Fase6 = 6, Fase7 = 7 }

    #endregion

    #region Faction

    /// <summary>
    /// Mapeia facções para nome e símbolo
    /// </summary>
    class Faction
    {
        private static readonly Dictionary<Faccao, (string Nome, string Simbolo)> mapa = new Dictionary<Faccao, (string, string)>
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
        public static string ObterSimbolo(Faccao faccao) => mapa[faccao].Simbolo;
        public static string ObterNome(Faccao faccao) => mapa[faccao].Nome;
    }

    #endregion

    #region Habilidades

    enum EventoCombate { AntesDeReceberDano, DepoisDeReceberDano }

    /// <summary>
    /// Controla o cooldown de uma habilidade — quanto tempo falta para poder usar novamente
    /// </summary>
    class SkillCooldown
    {
        public int TurnosRestantes => turnosRestantes;
        public int CooldownTotal => cooldownTotal;
        private int turnosRestantes = 0;
        private int cooldownTotal;

        public SkillCooldown(int cooldown)
        {
            cooldownTotal = cooldown;
        }

        public bool Disponivel => turnosRestantes == 0;

        public void Usar()
        {
            turnosRestantes = cooldownTotal;
        }

        public void PassarTurno()
        {
            if (turnosRestantes > 0)
                turnosRestantes--;
        }
        public void Resetar()
        {
            this.turnosRestantes = 0;
        }

        /// <summary>
        /// Retorna o emoji de relógio proporcional ao progresso do cooldown
        /// </summary>
        public static string ObterRelogio(int turnosRestantes, int cooldownTotal)
        {
            if (turnosRestantes == 0) return "🕛";

            string[] relogios = { "🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙" };
            int turnosPassados = cooldownTotal - turnosRestantes;
            int indice = (int)Math.Round((double)turnosPassados * 9 / cooldownTotal) - 1;
            indice = Math.Clamp(indice, 0, 8);
            return relogios[indice];
        }
    }

    /// <summary>
    /// Classe base para todas as habilidades — define nome, cooldown e o contrato de Ativar()
    /// </summary>
    abstract class Habilidade
    {
        public string Nome { get; }
        public int Turnos { get; }
        public string Descricao { get; }
        public SkillCooldown Cooldown { get; }

        public Habilidade(string nome, int turnos, string descricao = "")
        {
            Nome = nome;
            Turnos = turnos;
            Descricao = descricao;
            Cooldown = new SkillCooldown(turnos);
        }

        public abstract void Ativar(Combate alvo);
    }

    /// <summary>
    /// Habilidade ativada automaticamente em resposta a eventos do jogo, início de turno
    /// </summary>
    abstract class HabilidadePassiva : Habilidade
    {
        public HabilidadePassiva(string nome, int turnos, string descricao = "") : base(nome, turnos, descricao) { }
        public virtual bool Revive() => false;
        public abstract bool DeveAtivar(EventoCombate evento);
        public abstract string MensagemSobreviveu(Personagem personagem);
        public abstract string MensagemMorreu(Personagem personagem);
    }

    /// <summary>
    /// Habilidade ativada manualmente pelo jogador no seu turno
    /// </summary>
    abstract class HabilidadeAtiva : Habilidade
    {
        public HabilidadeAtiva(string nome, int turnos, string descricao = "") : base(nome, turnos, descricao) { }
    }

    #endregion

    #region Personagem

    /// <summary>
    /// Representa o Personagem
    /// </summary>
    class Personagem
    {
        public int Slot { get; private set; }
        public Faccao Faccao { get; private set; }
        public string Nome { get; private set; }
        public string Simbolo { get; private set; }
        public int HP { get; private set; }
        public int Ataque { get; private set; }
        public int Defesa { get; private set; }
        public double TaxaCrit { get; private set; } = 0.15;
        public double DanoCrit { get; private set; } = 0.60;

        public Personagem(int slot, Faccao faccao, string nome, string simbolo, int hp, int ataque, int def)
        {
            Slot = slot;
            Faccao = faccao;
            Nome = nome;
            Simbolo = simbolo;
            HP = hp;
            Ataque = ataque;
            Defesa = def;
        }
    }

    /// <summary>
    /// Armazena e fornece acesso ao roster completo de personagens do jogo
    /// </summary>
    class SelecaoSimbolo
    {
        private static readonly List<Personagem> personagens = new List<Personagem>
        {
            // Humanos
            new Personagem(1, Faccao.Humanos, "Operário", "👷",  1200, 240, 120),
            new Personagem(2, Faccao.Humanos, "Detetive", "🕵️", 1400, 160, 160),
            new Personagem(3, Faccao.Humanos, "Policial", "👮",  1000, 120, 280),
            new Personagem(4, Faccao.Humanos, "Sushiman ", "👲",  800, 280, 160),
            
            // O Reino
            new Personagem(1, Faccao.Reino, "Guarda", "💂", 1200, 160, 200),
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

        public static Personagem ObterPersonagem(int indice) => personagens[indice];

        /// <summary>
        /// Retorna o personagem correspondente à facção e slot informados
        /// </summary>
        /// <param name="faccao">Facção do personagem</param>
        /// <param name="slot">Slot do personagem dentro da facção</param>
        public static Personagem ObterPersonagem(Faccao faccao, Slot slot)
        {
            return personagens.First(p => p.Faccao == faccao && p.Slot == (int)slot);
        }
    }

    #endregion

    #region Combate

    /// <summary>
    /// Conduz o combate e status
    /// </summary>
    abstract class Combate
    {
        private static readonly Random random = new Random();
        public abstract Personagem Personagem { get; }
        public int HPAtual { get; protected set; }
        public int Ataque { get; protected set; }
        public int Defesa { get; protected set; }
        public double TaxaCrit { get; protected set; }
        public double DanoCrit { get; protected set; }

        public Combate(Personagem personagem)
        {
            HPAtual = personagem.HP;
            Ataque = personagem.Ataque;
            Defesa = personagem.Defesa;
            TaxaCrit = personagem.TaxaCrit;
            DanoCrit = personagem.DanoCrit;
        }

        /// <summary>
        /// Calcula e aplica o dano recebido descontando a redução por defesa
        /// </summary>
        /// <param name="ataque">Valor de ataque do atacante</param>
        public void ReceberDano(int ataque)
        {
            double reducao = Math.Min((Defesa / 1000.0) * 0.75, 0.75);
            int danoFinal = (int)(ataque * (1 - reducao));
            HPAtual -= danoFinal;
        }

        /// <summary>
        /// Executa um ataque contra o alvo informado
        /// </summary>
        /// <param name="alvo">Combatente que receberá o dano</param>
        public void Atacar(Combate alvo)
        {
            bool critico = random.NextDouble() < TaxaCrit;
            int dano = critico ? (int)(Ataque * (1 + DanoCrit)) : Ataque;
            alvo.ReceberDano(dano);
        }

        /// <summary>
        /// Verifica se o combatente ainda está vivo
        /// </summary>
        /// <returns>True se HP atual for maior que zero</returns>
        public bool EstaVivo()
        {
            return HPAtual > 0;
        }

        /// <summary>
        /// Aplica o stat de um item ao combatente
        /// </summary>
        public void AplicarItem(Item item)
        {
            switch (item.TipoStat)
            {
                case TipoStat.ATKFlat: Ataque += (int)item.Valor; break;
                case TipoStat.HPFlat: HPAtual += (int)item.Valor; break;
                case TipoStat.DEFFlat: Defesa += (int)item.Valor; break;
                case TipoStat.HPPct: HPAtual += (int)(HPAtual * item.Valor); break;
                case TipoStat.DEFPct: Defesa += (int)(Defesa * item.Valor); break;
                case TipoStat.TaxaCritPct: TaxaCrit += item.Valor; break;
                case TipoStat.DanoCritPct: DanoCrit += item.Valor; break;
            }
        }
    }

    #endregion

    #region Jogador

    /// <summary>
    /// Representa um jogador com nome, índice e personagem selecionado
    /// </summary>
    class Jogador : Combate
    {
        public override Personagem Personagem { get; }

        public Jogador(Personagem personagem) : base(personagem)
        {
            Personagem = personagem;
        }
    }

    /// <summary>
    /// Gerencia os campeões desbloqueados e a seleção do time do jogador
    /// </summary>
    class Campeoes
    {
        private static readonly List<Personagem> desbloqueados = new List<Personagem>
        {
            SelecaoSimbolo.ObterPersonagem(Faccao.Humanos, Slot.Slot1),
            SelecaoSimbolo.ObterPersonagem(Faccao.Humanos, Slot.Slot2),
            SelecaoSimbolo.ObterPersonagem(Faccao.Humanos, Slot.Slot3),
            SelecaoSimbolo.ObterPersonagem(Faccao.Humanos, Slot.Slot4),
        };

        public static void DesbloquearCampeoes(Faccao faccao, Fases fase)
        {
            Fase fas = Campanha.ObterFase((int)fase);

            foreach (Slot slot in fas.Rodada1)
            {
                Personagem p = SelecaoSimbolo.ObterPersonagem(faccao, slot);
                if (!desbloqueados.Contains(p))
                    desbloqueados.Add(p);
            }

            foreach (Slot slot in fas.Rodada2)
            {
                Personagem p = SelecaoSimbolo.ObterPersonagem(faccao, slot);
                if (!desbloqueados.Contains(p))
                    desbloqueados.Add(p);
            }
        }

        public static List<Personagem> ObterDesbloqueados() => desbloqueados;

        /// <summary>
        /// Solicita ao jogador a escolha de 4 campeões sem repetição e retorna o time selecionado
        /// </summary>
        public static List<Personagem> SelecionarTime()
        {
            var time = new List<Personagem>();
            var desbloqueados = ObterDesbloqueados();

            while (time.Count < 4)
            {
                Menu.MenuSelecaoTime(desbloqueados);
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
    }

    // Gerencia o arsenal de personagens do jogador
    /// <summary>
    /// Gerencia os itens obtidos e equipados pelo jogador
    /// </summary>
    class Arsenal
    {
        // Itens obtidos ao longo da campanha
        private static readonly List<Item> obtidos = new List<Item>();

        // 7 slots de equipamento (um por fase), null = vazio
        private static readonly Item?[] equipados = new Item?[7];

        private static readonly Dictionary<Faccao, string[]> simbolosPorFaccao = new Dictionary<Faccao, string[]>
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
        public static void DroparItem(Faccao faccao, Fases fase)
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
        public static void EquiparItem(Item item)
        {
            equipados[(int)item.Fase - 1] = item;
        }

        /// <summary>
        /// Retorna os itens atualmente equipados
        /// </summary>
        public static Item?[] ObterEquipados() => equipados;

        /// <summary>
        /// Retorna todos os itens obtidos
        /// </summary>
        public static List<Item> ObterObtidos() => obtidos;

        /// <summary>
        /// Carrega itens a partir do FaseConcluida igual ao CarregarCampeoes
        /// </summary>
        public static void CarregarItens()
        {
            foreach (Capitulos cap in Capitulos.ObterTodos())
                foreach (Fases fase in Enum.GetValues<Fases>())
                    if (cap.FaseConcluida[(int)fase - 1])
                        DroparItem(cap.Faccao, fase);
        }

        /// <summary>
        /// Aplica os stats dos itens equipados ao combatente informado
        /// </summary>
        public static void AplicarItens(Combate combate)
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
        public static void SalvarItens()
        {
            var json = JsonSerializer.Serialize(equipados);
            File.WriteAllText("itens.txt", json);
        }

        /// <summary>
        /// Restaura os itens equipados a partir do arquivo salvo anteriormente
        /// </summary>
        public static void CarregarItensEquipados()
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
    }

    #endregion

    #region Inimigo

    /// Define os multiplicadores de HP, Ataque e Defesa aplicados aos inimigos conforme o capítulo e fase
    struct MultiplicadorFase
    {
        public float HP;
        public float Ataque;
        public float Defesa;
    }

    class Inimigo : Combate
    {
        public override Personagem Personagem { get; }

        public Inimigo(Personagem personagem, MultiplicadorFase mult) : base(personagem)
        {
            Personagem = personagem;
            HPAtual = (int)(HPAtual * mult.HP);
            Ataque = (int)(Ataque * mult.Ataque);
            Defesa = (int)(Defesa * mult.Defesa);
        }
    }

    #endregion

    #region Item

    /// Tipos de stat que um item pode alterar
    enum TipoStat { ATKFlat, HPFlat, DEFFlat, HPPct, DEFPct, TaxaCritPct, DanoCritPct }

    /// <summary>
    /// Representa um item equipável obtido ao concluir uma fase
    /// </summary>
    class Item
    {
        public string Nome { get; init; }
        public string Simbolo { get; init; }
        public Faccao Faccao { get; init; }
        public Fases Fase { get; init; }
        public TipoStat TipoStat { get; init; }
        public double Valor { get; init; }

        public Item(string nome, string simbolo, Faccao faccao, Fases fase, TipoStat tipoStat)
        {
            Nome = nome;
            Simbolo = simbolo;
            Faccao = faccao;
            Fase = fase;
            TipoStat = tipoStat;
            Valor = CalcularValor(faccao, fase, tipoStat);
        }

        public Item()
        {
            Nome = null!;
            Simbolo = null!;
        }

        /// <summary>
        /// Calcula o valor do stat do item com base no capítulo e tipo de stat
        /// </summary>
        private static double CalcularValor(Faccao faccao, Fases fase, TipoStat tipoStat)
        {
            int cap = (int)faccao;
            return tipoStat switch
            {
                TipoStat.ATKFlat => 120 * cap,
                TipoStat.HPFlat => 550 * cap,
                TipoStat.DEFFlat => 55 * cap,
                TipoStat.HPPct => 0.05 * cap,
                TipoStat.DEFPct => 0.05 * cap,
                TipoStat.TaxaCritPct => 0.05 + 0.01 * cap,
                TipoStat.DanoCritPct => 0.15 + 0.01 * cap,
                _ => 0
            };
        }

        /// <summary>
        /// Retorna o valor formatado conforme o tipo de stat
        /// </summary>
        public string ValorFormatado()
        {
            return TipoStat switch
            {
                TipoStat.ATKFlat or TipoStat.HPFlat or TipoStat.DEFFlat
                    => $"{(int)Valor}",
                _
                    => $"{Valor * 100:F0}%"
            };
        }

        public string NomeStat() => TipoStat switch
        {
            TipoStat.ATKFlat => "ATK",
            TipoStat.HPFlat => "HP",
            TipoStat.DEFFlat => "DEF",
            TipoStat.HPPct => "HP",
            TipoStat.DEFPct => "DEF",
            TipoStat.TaxaCritPct => "Crit",
            TipoStat.DanoCritPct => "Dano Crit",
            _ => ""
        };
    }

    #endregion

    #region Campanha

    /// <summary>
    /// Define a composição de inimigos nas duas rodadas de uma fase da campanha
    /// </summary>
    class Fase
    {
        public List<Slot> Rodada1 { get; }
        public List<Slot> Rodada2 { get; }

        public Fase(List<Slot> rodada1, List<Slot> rodada2)
        {
            Rodada1 = rodada1;
            Rodada2 = rodada2;
        }
    }

    /// <summary>
    /// Representa a campanha
    /// </summary>
    class Campanha
    {
        private static readonly List<Fase> fases = new List<Fase>
        {
            new Fase(new List<Slot> { Slot.Slot1 }, new List<Slot> { Slot.Slot2 }),
            new Fase(new List<Slot> { Slot.Slot1, Slot.Slot1 }, new List<Slot> { Slot.Slot1, Slot.Slot2 }),
            new Fase(new List<Slot> { Slot.Slot2, Slot.Slot2 }, new List<Slot> { Slot.Slot2, Slot.Slot3 }),
            new Fase(new List<Slot> { Slot.Slot3, Slot.Slot1 }, new List<Slot> { Slot.Slot2, Slot.Slot3, Slot.Slot3 }),
            new Fase(new List<Slot> { Slot.Slot2, Slot.Slot3, Slot.Slot2 }, new List<Slot> { Slot.Slot3, Slot.Slot1, Slot.Slot3 }),
            new Fase(new List<Slot> { Slot.Slot1, Slot.Slot3, Slot.Slot1 }, new List<Slot> { Slot.Slot1, Slot.Slot3, Slot.Slot3, Slot.Slot1 }),
            new Fase(new List<Slot> { Slot.Slot2, Slot.Slot3, Slot.Slot2 }, new List<Slot> { Slot.Slot1, Slot.Slot3, Slot.Slot4, Slot.Slot2 }),
        };

        public static Fase ObterFase(int numero)
        {
            return fases[numero - 1];
        }

    }

    // Define as fases disponíveis e o progresso de desbloqueio de um capítulo
    class Capitulos
    {
        public Faccao Faccao { get; }
        public List<bool> FaseDesblock { get; private set; }
        public List<bool> FaseConcluida { get; private set; }
        public bool CapDesblock { get; private set; }

        public Capitulos(Faccao faccao, List<bool> faseDesblock, List<bool> faseConcluida, bool capDesblock)
        {
            Faccao = faccao;
            FaseDesblock = faseDesblock;
            FaseConcluida = faseConcluida;
            CapDesblock = capDesblock;
        }

        private static readonly List<Capitulos> capitulos = new List<Capitulos>
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

        public static Capitulos ObterCapitulo(Faccao faccao)
        {
            return capitulos.First(c => c.Faccao == faccao);
        }

        public static bool EstaDesbloqueado(Faccao faccao, Fases fase)
        {
            Capitulos cap = ObterCapitulo(faccao);
            return cap.CapDesblock && cap.FaseDesblock[(int)fase - 1];
        }

        public static bool EstaDesbloqueado(Faccao faccao) => ObterCapitulo(faccao).CapDesblock;

        public static void DesbloquearFase(Faccao faccao, Fases fase)
        {
            Fases ultima = Enum.GetValues<Fases>().Last();
            if (fase == ultima) return;
            Capitulos cap = ObterCapitulo(faccao);
            cap.FaseDesblock[(int)fase] = true;
        }

        public static void ConcluirFase(Faccao faccao, Fases fase)
        {
            Capitulos cap = ObterCapitulo(faccao);
            cap.FaseConcluida[(int)fase - 1] = true;
        }

        /// <summary>
        /// Desbloqueia a próxima facção se todas as fases do capítulo atual estiverem concluídas
        /// </summary>
        public static void DesbloquearFaccao(Faccao faccao, Fases fase)
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

        public static void SalvarProgresso()
        {
            var json = JsonSerializer.Serialize(capitulos);
            File.WriteAllText("save.txt", json);
        }

        /// <summary>
        /// Carrega o progresso salvo em arquivo, restaurando capítulos, campeões desbloqueados e itens obtidos
        /// </summary>
        public static void CarregarProgresso()
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

        public static List<Capitulos> ObterTodos() => capitulos;

    }

    /// <summary>
    /// Gerencia o loop principal do jogo, menus e execução da campanha
    /// </summary>
    class GerenciadorDeJogo
    {
        private static readonly Random random = new Random();

        /// <summary>
        /// Exibe os alvos vivos, lê a escolha do jogador ou sorteia um alvo para o inimigo, executa o ataque e exibe o HP resultante
        /// </summary>
        public static void ExecutarTurno(Combate atacante, List<Combate> defensor)
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

            if (atacante is Inimigo)
            {
                Thread.Sleep(1500);
            }
        }

        /// <summary>
        /// Itera pelos combatentes em ordem, alternando ataques entre jogador e inimigo, até que um dos lados seja completamente eliminado.
        /// Retorna true se o jogador vencer
        /// </summary>
        public static bool ExecutarCombate(List<Combate> jogador, List<Combate> inimigo, List<Combate> combatentes)
        {
            do
            {
                for (int c = 0; c < combatentes.Count; c++)
                {
                    if (!combatentes[c].EstaVivo()) continue;
                    if (!inimigo.Any(i => i.EstaVivo()) || !jogador.Any(j => j.EstaVivo()))
                    { break; }
                    if (combatentes[c] is Jogador)
                    { ExecutarTurno(combatentes[c], inimigo); }
                    else
                    { ExecutarTurno(combatentes[c], jogador); }
                }

            } while (jogador.Any(j => j.EstaVivo()) && inimigo.Any(i => i.EstaVivo()));

            return jogador.Any(j => j.EstaVivo());
        }

        /// <summary>
        /// Monta e executa as duas rodadas de uma fase, retornando true se o jogador vencer ambas
        /// </summary>
        public static bool ExecutarFase(Faccao capitulo, Fases fase)
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

        public static void Executar()
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
    }
    #endregion

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

    #region Program

    /// <summary>
    /// Ponto de entrada do jogo
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            GerenciadorDeJogo.Executar();
        }
    }

    #endregion
}
