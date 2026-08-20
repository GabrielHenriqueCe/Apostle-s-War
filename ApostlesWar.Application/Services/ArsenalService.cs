using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;

namespace ApostlesWar.Application.Services
{
    /// <summary>
    /// O ACERVO de equipamento do jogador: o que ele tem, o que está vestido e o nível de cada peça.
    /// A <b>Forja</b> é a estação da Catedral onde isto se usa; o acervo é este service.
    ///
    /// <b>O inventário aceita DUPLICATA</b> (ago/2026), e é a mudança que carrega todas as outras:
    /// cada fase larga <see cref="ItensPorFase"/> peças, cada peça é uma instância com
    /// <see cref="Item.Id"/> própria, e duas Manoplas do Reino podem estar em níveis diferentes com
    /// principais diferentes. Antes era uma peça por (facção, fase), sem cópia possível.
    ///
    /// <b>O save guarda o inventário e os IDs equipados, não os itens equipados.</b> Guardar o objeto
    /// nos dois lugares faria a mesma peça existir duas vezes no disco, e o uso subiria o nível de
    /// uma cópia só.
    /// </summary>
    public class ArsenalService
    {
        #region Construtor

        private readonly CapitulosService _capitulosService;
        private readonly PoService _po;
        private readonly IRepositorioDeSave _repo;

        // Os slots de save deste service. O "itens" é o formato ANTIGO (um Item?[7] com o objeto
        // dentro) e só é citado pra ser APAGADO na migração — ver Carregar.
        private const string ChaveInventario = "inventario";
        private const string ChaveEquipados = "equipados";
        private const string ChaveLegado = "itens";

        /// <summary>
        /// Quantas peças uma fase larga. <b>Quatro é o time inteiro armado numa corrida só</b> — é
        /// esse o motivo do número, e é ele que alimenta o sacrifício da forja mais adiante.
        /// </summary>
        public const int ItensPorFase = 4;

        private const int Slots = 7;

        // 7 slots de equipamento (um por fase), null = vazio. Em MEMÓRIA são as mesmas referências do
        // inventário — é isso que faz o uso subir o nível de UMA peça só. No DISCO vira ID
        // (ver SalvarItens): gravar o objeto nos dois lugares faria a peça existir duas vezes, e as
        // duas cópias divergiriam no primeiro combate.
        private Item?[] equipados = new Item?[Slots];

        private List<Item> inventario = new();

        public ArsenalService(CapitulosService capitulosService, PoService po, IRepositorioDeSave repo)
        {
            _capitulosService = capitulosService;
            _po = po;
            _repo = repo;
        }

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
        /// O que a fase larga: <see cref="ItensPorFase"/> peças do slot dela, cada uma com o principal
        /// SORTEADO entre as opções daquele slot (<see cref="Equipamento.OpcoesDoSlot"/>).
        ///
        /// É o sorteio que faz quatro cópias do mesmo slot serem quatro decisões — nos três slots de
        /// valor cheio a lista tem uma opção só, e aí as quatro saem iguais mesmo. Todas caem no
        /// <b>nível 1, sem estrela</b>: o drop dá a peça, a magnitude se conquista jogando.
        /// </summary>
        public List<Item> DroparItens(Faccao faccao, Fases fase)
        {
            var caidos = new List<Item>();
            for (int i = 0; i < ItensPorFase; i++) caidos.Add(Forjar(faccao, fase));

            inventario.AddRange(caidos);
            SalvarItens();
            return caidos;
        }

        /// <summary>
        /// O emoji da peça daquele slot naquela facção. Público porque a tela da FASE precisa mostrar
        /// o que vai cair antes de a peça existir — e a tabela de emojis é daqui.
        /// </summary>
        public string SimboloDoSlot(Faccao faccao, Fases fase) => simbolosPorFaccao[faccao][(int)fase - 1];

        /// <summary>Uma peça nova daquele slot, com o principal sorteado. Não entra no inventário.</summary>
        private Item Forjar(Faccao faccao, Fases fase)
        {
            IReadOnlyList<TipoStat> opcoes = Equipamento.OpcoesDoSlot(fase);
            return new Item(
                Equipamento.NomeDoSlot(fase), SimboloDoSlot(faccao, fase), faccao, fase,
                opcoes[Random.Shared.Next(opcoes.Count)]);
        }

        /// <summary>
        /// Restaura o inventário e o que estava vestido.
        ///
        /// <b>Save sem inventário é save ANTIGO</b> — de quando o item era uma entrada de catálogo sem
        /// nível. Ele não se converte: as peças de lá não tinham nível nem principal sorteado, e
        /// inventar os dois seria dar de presente o que se conquista jogando. O acervo se reconstrói
        /// das fases já concluídas, <b>uma peça por fase</b> (não quatro — migração não é drop), e o
        /// slot antigo morre pra não ressuscitar na próxima abertura.
        /// </summary>
        public void CarregarItensEquipados()
        {
            _po.Carregar();

            var salvo = _repo.Carregar<List<Item>>(ChaveInventario);
            if (salvo == null)
            {
                Migrar();
                return;
            }

            inventario = salvo;

            // O ID volta a ser a REFERÊNCIA do inventário: sem isso o slot teria uma cópia própria e
            // o uso subiria o nível de uma só. ID órfão (peça que sumiu do acervo) deixa o slot vazio.
            var ids = _repo.Carregar<Guid?[]>(ChaveEquipados);
            if (ids != null)
                for (int i = 0; i < ids.Length && i < equipados.Length; i++)
                    equipados[i] = ids[i] == null ? null : inventario.FirstOrDefault(it => it.Id == ids[i]);
        }

        private void Migrar()
        {
            _repo.Excluir(ChaveLegado);
            inventario = new();

            // As quatro dificuldades caem aqui e a mesma fase pode vir mais de uma vez — a peça é uma
            // só por (facção, fase) na migração, senão quem já zerou o jogo ganharia um acervo de
            // centenas de itens só por abrir o save novo.
            foreach (var (faccao, fase, _) in _capitulosService.FasesConcluidas().DistinctBy(f => (f.Item1, f.Item2)))
                inventario.Add(Forjar(faccao, fase));

            SalvarItens();
        }

        /// <summary>
        /// Equipa a peça no slot da fase dela, substituindo a anterior — e PERSISTE.
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

        /// <summary>As peças vestidas, uma por slot.</summary>
        public Item?[] ObterEquipados() => equipados;

        /// <summary>
        /// Esta peça está vestida? Casa por <see cref="Item.Id"/>, não por (facção, fase): duas
        /// Manoplas do Reino são peças DIFERENTES agora, e comparar pelo slot marcaria as duas.
        /// </summary>
        public bool EstaEquipado(Item item) => equipados.Any(e => e != null && e.Id == item.Id);

        /// <summary>Todo o acervo, na ordem em que caiu.</summary>
        public List<Item> ObterObtidos() => inventario;

        // Houve aqui um `TotaisEquipados`, que somava o conjunto POR STAT pra um painel "Bônus do
        // arsenal" ao lado do boneco. Ele morreu com o painel (ago/2026): somar item com item produz
        // "+5%", e 5% de quê só se sabe escolhendo um apóstolo. O número que interessa é o DELE, já
        // somado, e quem o produz é o mesmo caminho da luta (`FluxoDoFront.BonusDe`).

        /// <summary>
        /// Aplica os stats dos itens equipados ao combatente informado
        /// </summary>
        public void AplicarItens(Combate combate)
            => combate.AplicarItens(ObterEquipados().Where(i => i != null).Select(i => i!));

        #endregion

        #region O nível da peça

        /// <summary>
        /// O que a fase pagou de uso às peças VESTIDAS (docs/GDD-itens.md §Como o nível sobe).
        ///
        /// <b>Só quem estava em campo ganha</b> — peça no baú não sobe. E o ganho é por CICLO de
        /// combate, não por turno do portador: contar turno faria o apóstolo rápido subir equipamento
        /// em dobro, e a Velocidade viraria duplamente dominante (§Contar rodada, não ação).
        /// </summary>
        public void CreditarUso(Dificuldade dificuldade, double ciclos, bool venceu)
        {
            int pontos = Po.PontosDaFase(dificuldade, ciclos, venceu);

            foreach (Item? item in ObterEquipados())
            {
                if (item == null) continue;
                item.Pontos += pontos;
            }

            // O PÓ é recompensa de fase, e cai só na VITÓRIA — junto com as peças, como o §O MATERIAL
            // escreve. O USO acima é outra coisa e cai dos dois jeitos: quem tentou a fase acima do
            // próprio nível e caiu não sai de mãos vazias, e é isso que o faz continuar arriscando.
            if (venceu) _po.Creditar(dificuldade);

            SalvarItens();
        }

        /// <summary>
        /// A peça bateu na parede: os pontos dela JÁ PAGAM um nível acima do teto das estrelas, e
        /// ainda há estrela a comprar. Mesmo critério do apóstolo (<see cref="ProgressaoService.NaParede"/>)
        /// e pelo mesmo motivo: estar no nível 9 com a barra pela metade é estar subindo, não travado.
        /// </summary>
        public bool NaParede(Item item)
            => item.Estrelas < Material.EstrelaMaxima
            && Po.NivelPorPontos(item.Pontos, Arquetipos.NivelMaximo) > Progressao.TetoPorEstrelas(item.Estrelas);

        /// <summary>
        /// Paga o pedágio da próxima estrela da peça com PÓ, e com ela a dezena seguinte. Só na
        /// parede, e só com o preço inteiro no bolso (<see cref="Po.Receita"/>).
        /// </summary>
        public MotivoRecusa ComprarEstrela(Item item)
        {
            if (item.Estrelas >= Material.EstrelaMaxima) return MotivoRecusa.NoTetoFinal;
            if (!NaParede(item)) return MotivoRecusa.ForaDaParede;

            if (!_po.Debitar(Po.Receita(item.Estrelas + 1))) return MotivoRecusa.SemSaldo;

            item.Estrelas++;
            SalvarItens();
            return MotivoRecusa.Nenhum;
        }

        /// <summary>
        /// Queima pó como pontos de nível da peça — a Bigorna da Forja, e o análogo exato do
        /// <see cref="ProgressaoService.QueimarAlma"/> do lado do apóstolo.
        ///
        /// <b>Na parede, recusa.</b> Ponto além do teto não se perde (ele fica guardado e vira nível
        /// quando a estrela é paga), mas o pó gasto aqui é o MESMO que a estrela vai cobrar — quem
        /// queima travado está pagando duas vezes pelo mesmo nível.
        ///
        /// O débito é tudo-ou-nada, pelo motivo da queima de alma: metade cobrada com a outra metade
        /// recusada seria pó sumindo.
        /// </summary>
        public MotivoRecusa QueimarPo(Item item, IReadOnlyList<Custo> faixas)
        {
            if (faixas.Count == 0 || faixas.Any(f => f.Quantidade <= 0)) return MotivoRecusa.SemSaldo;
            if (item.Nivel >= Arquetipos.NivelMaximo) return MotivoRecusa.NoTetoFinal;
            if (NaParede(item)) return MotivoRecusa.NaParede;

            if (!_po.Debitar(faixas)) return MotivoRecusa.SemSaldo;

            item.Pontos += faixas.Sum(f => Po.PontosPorPo(f.Raridade) * f.Quantidade);
            SalvarItens();
            return MotivoRecusa.Nenhum;
        }

        /// <summary>
        /// O quanto falta pro próximo nível da peça, pra a barra da ficha: (o que já entrou nesta
        /// faixa, o que a faixa inteira custa). Na parede a barra enche e PARA — é ela que diz ao
        /// jogador que o pedágio está liberado.
        /// </summary>
        public (int Feito, int Total) FaixaDoNivel(Item item)
        {
            int nivel = item.Nivel;
            if (nivel >= Arquetipos.NivelMaximo) return (1, 1);

            int piso = Po.PontosParaNivel(nivel);
            int total = Po.PontosParaNivel(nivel + 1) - piso;
            return (Math.Min(item.Pontos - piso, total), total);
        }

        #endregion

        #region Save

        /// <summary>Persiste o acervo e o que está vestido. Os dois andam juntos, sempre.</summary>
        public void SalvarItens()
        {
            _repo.Salvar(ChaveInventario, inventario);
            _repo.Salvar(ChaveEquipados, equipados.Select(i => i?.Id).ToArray());
        }

        /// <summary>
        /// Devolve o arsenal ao estado de jogo novo — disco E memória (ver
        /// <see cref="CapitulosService.Resetar"/> pra o porquê dos dois).
        /// </summary>
        public void Resetar()
        {
            _repo.Excluir(ChaveInventario);
            _repo.Excluir(ChaveEquipados);
            _repo.Excluir(ChaveLegado);
            equipados = new Item?[Slots];
            inventario.Clear();
            _po.Resetar();
        }

        #endregion
    }
}
