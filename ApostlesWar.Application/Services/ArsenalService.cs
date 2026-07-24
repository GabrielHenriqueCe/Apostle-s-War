using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ApostlesWar.Application.Services
{
    public class ArsenalService
    {
        #region Construtor

        private readonly CapitulosService _capitulosService;
        private readonly IRepositorioDeSave _repo;

        // 7 slots de equipamento (um por fase), null = vazio
        private Item?[] equipados = new Item?[7];

        // Itens obtidos ao longo da campanha
        private List<Item> obtidos = new List<Item>();

        #endregion

        #region Arsenal

        Dictionary<Faccao, string[]> simbolosPorFaccao = new Dictionary<Faccao, string[]>
        {
            { Faccao.Reino,        new[] { "🗡️", "👑", "🛡️", "📿", "👔", "👖", "👞" } },
            { Faccao.LadoSombrio,  new[] { "🏹", "🕶️", "💼", "🦯", "🥋", "🦽", "👟" } },
            { Faccao.Tecnologicos, new[] { "🔫", "🥽", "🧰", "🦾", "🥼", "🦿", "🛼" } },
            { Faccao.Folclore,     new[] { "🪃", "🧢", "🧳", "🥊", "🎽", "🩳", "🩴" } },
            { Faccao.Misticos,     new[] { "🪭", "👒", "👛", "💅", "🥻", "👙", "👠" } },
            { Faccao.Especial,     new[] { "🔪", "⛑️", "🍳", "🚬", "🦺", "🛢️", "👢" } },
            { Faccao.Decaidos,     new[] { "🪄", "🎩", "🎒", "🩼", "🧥", "🦼", "🥾" } },
            { Faccao.Apostolos,    new[] { "🎄", "🧣", "🔔", "🧤", "👘", "🩲", "🪽" } },
        };

        /// <summary>
        /// Adiciona um item à lista de obtidos ao concluir uma fase
        /// </summary>
        public Item? DroparItem(Faccao faccao, Fases fase)
        {
            if (obtidos.Any(i => i.Faccao == faccao && i.Fase == fase))
                return null;

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
            var item = new Item(nome, simbolo, faccao, fase, tipo);
            obtidos.Add(item);
            return item;
        }

        public ArsenalService(CapitulosService capitulosService, IRepositorioDeSave repo)
        {
            _capitulosService = capitulosService;
            _repo = repo;
        }

        /// <summary>
        /// Restaura os itens equipados do save. Ausente/corrompido → mantém os slots vazios (a porta
        /// devolve null). A cópia é elemento-a-elemento pelo guard de tamanho.
        /// </summary>
        public void CarregarItensEquipados()
        {
            var lista = _repo.Carregar<Item?[]>("itens");
            if (lista != null)
                for (int i = 0; i < lista.Length && i < equipados.Length; i++)
                    equipados[i] = lista[i];
        }

        /// <summary>
        /// Equipa um item no slot correspondente à sua fase, substituindo o anterior
        /// </summary>
        public void EquiparItem(Item item)
        {
            equipados[(int)item.Fase - 1] = item;
        }

        /// <summary>
        /// Retorna os itens atualmente equipados
        /// </summary>
        public Item?[] ObterEquipados() => equipados;

        /// <summary>
        /// Retorna todos os itens obtidos
        /// </summary>
        public List<Item> ObterObtidos() => obtidos;

        /// <summary>
        /// Carrega itens a partir do FaseConcluida igual ao CarregarCampeoes
        /// </summary>
        public void CarregarItens()
        {
            foreach (Capitulo cap in _capitulosService.ObterTodos())
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
        public void AplicarItens(Combate combate)
        {
            foreach (Item? item in equipados)
            {
                if (item == null) continue;
                combate.AplicarItem(item);
            }
        }

        /// <summary>
        /// Persiste os itens equipados para restauração futura.
        /// </summary>
        public void SalvarItens() => _repo.Salvar("itens", equipados);

        #endregion
    }
}
