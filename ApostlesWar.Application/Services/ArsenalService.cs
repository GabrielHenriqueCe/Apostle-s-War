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

        // O slot de save deste service. Const porque salvar, carregar e o wipe do Resetar o citam.
        private const string ChaveItens = "itens";

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
            { Faccao.Ascendentes,  new[] { "🎄", "🧣", "🔔", "🧤", "👘", "🩲", "🪽" } },
        };

        /// <summary>
        /// Adiciona um item à lista de obtidos ao concluir uma fase
        /// </summary>
        public Item? DroparItem(Faccao faccao, Fases fase)
        {
            if (obtidos.Any(i => i.Faccao == faccao && i.Fase == fase))
                return null;

            var item = PreverItem(faccao, fase);
            obtidos.Add(item);
            return item;
        }

        /// <summary>
        /// O item que (faccao, fase) dropa, sem efeito colateral (NÃO adiciona a obtidos). Pra a tela
        /// de fase mostrar o drop antes de lutar. O DroparItem reusa isto. O item é determinístico:
        /// a fase define nome+stat, a facção define o emoji e a magnitude (Item.CalcularValor).
        /// </summary>
        public Item PreverItem(Faccao faccao, Fases fase)
        {
            string simbolo = simbolosPorFaccao[faccao][(int)fase - 1];
            return new Item(NomeDoSlot(fase), simbolo, faccao, fase, StatDoSlot(fase));
        }

        /// <summary>
        /// Como se chama o slot da fase — e, por tabela, o item que cai nela ("Arma", "Elmo"...).
        /// A tela do boneco precisa nomear os 7 slots mesmo quando estão VAZIOS (não há Item pra
        /// perguntar), e é por isso que isto é público: sem ele o front mantinha a própria cópia da
        /// lista, que envelheceu — chamava a Fase 4 de "Acessório" enquanto o item que entra nela
        /// nasce "Manopla". Uma tabela só, um nome só.
        /// </summary>
        public static string NomeDoSlot(Fases fase) => fase switch
        {
            Fases.Fase1 => "Arma",
            Fases.Fase2 => "Elmo",
            Fases.Fase3 => "Escudo",
            Fases.Fase4 => "Manopla",
            Fases.Fase5 => "Peitoral",
            Fases.Fase6 => "Calça",
            Fases.Fase7 => "Bota",
            _ => throw new ArgumentOutOfRangeException(nameof(fase))
        };

        /// <summary>Que stat o item daquele slot carrega. A magnitude é do Item (CalcularValor).</summary>
        private static TipoStat StatDoSlot(Fases fase) => fase switch
        {
            Fases.Fase1 => TipoStat.ATKFlat,
            Fases.Fase2 => TipoStat.HPFlat,
            Fases.Fase3 => TipoStat.DEFFlat,
            Fases.Fase4 => TipoStat.TaxaCritPct,
            Fases.Fase5 => TipoStat.HPPct,
            Fases.Fase6 => TipoStat.DEFPct,
            Fases.Fase7 => TipoStat.DanoCritPct,
            _ => throw new ArgumentOutOfRangeException(nameof(fase))
        };

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
            var lista = _repo.Carregar<Item?[]>(ChaveItens);
            if (lista != null)
                for (int i = 0; i < lista.Length && i < equipados.Length; i++)
                    equipados[i] = lista[i];
        }

        /// <summary>
        /// Equipa um item no slot correspondente à sua fase, substituindo o anterior — e PERSISTE.
        ///
        /// O save vem junto de propósito: "equipou, está equipado da próxima vez que abrir" é regra,
        /// não escolha de tela. Quando eram duas cascas, cada uma escolheu a sua (uma salvava só ao
        /// vencer uma fase, a outra na hora) — mesmo dado, duas políticas. Quem manda no dado é quem
        /// decide quando ele é durável.
        /// </summary>
        public void EquiparItem(Item item)
        {
            equipados[(int)item.Fase - 1] = item;
            SalvarItens();
        }

        /// <summary>
        /// Retorna os itens atualmente equipados
        /// </summary>
        public Item?[] ObterEquipados() => equipados;

        /// <summary>
        /// Este item está equipado no slot dele? Item é único por (Faccao, Fase), então casa por isso —
        /// depois de carregar o save, os objetos equipados não são a MESMA referência dos obtidos.
        /// </summary>
        public bool EstaEquipado(Item item)
        {
            Item? eq = equipados[(int)item.Fase - 1];
            return eq != null && eq.Faccao == item.Faccao && eq.Fase == item.Fase;
        }

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
        /// O que o conjunto equipado dá, somado POR STAT — a resposta à pergunta "no total, quanto
        /// meus itens estão me dando?", que antes só dava pra montar de cabeça olhando os 7 slots.
        ///
        /// Some em vez de listar porque a pergunta é sobre o CONJUNTO: hoje cada slot carrega um stat
        /// diferente (ver <see cref="StatDoSlot"/>) e a soma é de uma parcela só, mas o dia em que dois
        /// slots derem ATK a conta já está no lugar certo — e não espalhada por quem desenha.
        ///
        /// FLAT e PCT continuam SEPARADOS de propósito: "+300 de HP" e "+15% de HP" não viram um
        /// número só sem escolher um champ, porque o percentual é sobre o HP base DELE (ver
        /// <see cref="Combate.AplicarItem"/>). O arsenal informa o que o equipamento dá; quem soma
        /// isso a um personagem é a luta.
        ///
        /// Só devolve o que tem valor: slot vazio não vira uma linha de zero. Em que ORDEM se lê é da
        /// tela — aqui a lista sai por stat só pra ser determinística.
        /// </summary>
        public List<BonusDoArsenal> TotaisEquipados()
            => equipados
                .Where(item => item != null)
                .GroupBy(item => item!.TipoStat)
                .Select(g => new BonusDoArsenal(g.Key, g.Sum(item => item!.Valor)))
                .Where(b => b.Valor != 0)
                .OrderBy(b => b.Stat)
                .ToList();

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
        public void SalvarItens() => _repo.Salvar(ChaveItens, equipados);

        /// <summary>
        /// Devolve o arsenal ao estado de jogo novo — disco E memória (ver
        /// <see cref="CapitulosService.Resetar"/> pra o porquê dos dois).
        ///
        /// Os OBTIDOS entram no wipe mesmo não tendo save próprio: eles são DERIVADOS do
        /// FaseConcluida dos capítulos (ver <see cref="CarregarItens"/>), então deixá-los na memória
        /// faria o Arsenal continuar mostrando o loot de uma campanha que não existe mais.
        /// </summary>
        public void Resetar()
        {
            _repo.Excluir(ChaveItens);
            equipados = new Item?[7];
            obtidos.Clear();
        }

        #endregion
    }

    /// <summary>Um stat e quanto o arsenal equipado dá dele. Como se ESCREVE isso é da tela.</summary>
    public record BonusDoArsenal(TipoStat Stat, double Valor);
}
