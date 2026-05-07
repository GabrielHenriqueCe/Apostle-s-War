using System.Text.Json;

namespace ApostlesWar
{
    #region Arsenal

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


    }

    #endregion
}