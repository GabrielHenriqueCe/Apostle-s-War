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
        private readonly PersonagemService _personagens;
        private readonly IRepositorioDeSave _repo;

        // Os slots de save deste service. Os dois últimos são formatos ANTIGOS e só são citados pra
        // ser APAGADOS na migração — ver Carregar: o "itens" guardava um Item?[7] com o objeto
        // dentro, e o "equipados" guardava os 7 IDs que valiam pro jogo INTEIRO.
        private const string ChaveInventario = "inventario";
        private const string ChaveVestidos = "vestidos";
        private const string ChaveEquipadosGlobais = "equipados";
        private const string ChaveLegado = "itens";

        /// <summary>
        /// Quantas peças uma fase larga. <b>Quatro é o time inteiro armado numa corrida só</b> — é
        /// esse o motivo do número, e é ele que alimenta o sacrifício da forja mais adiante.
        /// </summary>
        public const int ItensPorFase = 4;

        private const int Slots = 7;

        // QUEM VESTE O QUÊ. A chave é `(Facção, Slot)`, a mesma identidade de apóstolo que o
        // `ProgressaoService` grava — índice no roster apontaria pra outra pessoa assim que a lista
        // crescesse. O valor é o boneco dele: 7 slots (um por fase), null = vazio.
        //
        // Em MEMÓRIA são as mesmas referências do inventário — é isso que faz o uso subir o nível de
        // UMA peça só. No DISCO vira ID (ver SalvarItens): gravar o objeto nos dois lugares faria a
        // peça existir duas vezes, e as duas cópias divergiriam no primeiro combate.
        //
        // <b>A peça é de UM apóstolo só.</b> O dicionário não garante isso sozinho (nada impede o
        // mesmo objeto em dois bonecos), e quem garante é o <see cref="EquiparItem"/>, que tira a
        // peça do portador anterior antes de vesti-la. Ver o §O ACERVO do GDD-itens: tomar a peça de
        // um aliado é o gesto, e desnudá-lo sem avisar é o defeito que a tela tem de evitar.
        private Dictionary<(Faccao, Slot), Item?[]> vestidos = new();

        private List<Item> inventario = new();

        public ArsenalService(CapitulosService capitulosService, PoService po,
            PersonagemService personagens, IRepositorioDeSave repo)
        {
            _capitulosService = capitulosService;
            _po = po;
            _personagens = personagens;
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
            foreach (VestidoPeloApostolo v in _repo.Carregar<List<VestidoPeloApostolo>>(ChaveVestidos) ?? new())
            {
                Item?[] boneco = Boneco(v.Faccao, v.Slot);
                for (int i = 0; i < v.Ids.Length && i < boneco.Length; i++)
                    boneco[i] = v.Ids[i] == null ? null : inventario.FirstOrDefault(it => it.Id == v.Ids[i]);
            }

            // <b>Save de antes do vínculo: o que estava vestido DESVESTE.</b> Lá os 7 slots valiam pro
            // jogo inteiro, e não há de quem eles eram — dar tudo a um apóstolo escolhido por nós
            // inventaria uma decisão do jogador, e num apóstolo que pode nem estar no time. O acervo
            // fica intacto: ninguém perde peça, só reescolhe quem veste o quê.
            _repo.Excluir(ChaveEquipadosGlobais);
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

        /// <summary>O boneco daquele apóstolo — 7 slots, criado vazio na primeira vez que se olha.</summary>
        private Item?[] Boneco(Faccao faccao, Slot slot)
        {
            if (!vestidos.TryGetValue((faccao, slot), out Item?[]? boneco))
                vestidos[(faccao, slot)] = boneco = new Item?[Slots];
            return boneco;
        }

        private Item?[] Boneco(Personagem apostolo) => Boneco(apostolo.Faccao, (Slot)apostolo.Slot);

        /// <summary>
        /// Veste a peça NESTE apóstolo, no slot da fase dela — e PERSISTE.
        ///
        /// <b>Tira ela de quem a estivesse usando</b>, porque a peça é uma só: é o "tomar do aliado"
        /// do §O ACERVO, e o modelo o permite de propósito. Quem tem de avisar ANTES do clique é a
        /// tela (o emoji do portador no cartão) — aqui embaixo o gesto já foi decidido.
        ///
        /// O save vem junto de propósito: "equipou, está equipado da próxima vez que abrir" é regra,
        /// não escolha de tela. Quando eram duas cascas, cada uma escolheu a sua (uma salvava só ao
        /// vencer uma fase, a outra na hora) — mesmo dado, duas políticas. Quem manda no dado é quem
        /// decide quando ele é durável.
        /// </summary>
        public void EquiparItem(Personagem apostolo, Item item)
        {
            int slot = (int)item.Fase - 1;

            foreach (Item?[] outro in vestidos.Values)
                if (outro[slot] != null && outro[slot]!.Id == item.Id) outro[slot] = null;

            Boneco(apostolo)[slot] = item;
            SalvarItens();
        }

        /// <summary>
        /// Tira do apóstolo a peça daquele slot — e PERSISTE, pela mesma regra do
        /// <see cref="EquiparItem"/>: desvestir também é estado durável.
        ///
        /// <b>A peça volta pro baú, não some.</b> Descartar peça é a Forja quem faz (o sacrifício),
        /// e nunca um botão de armaria: quem tira o elmo pra experimentar outro não está jogando o
        /// elmo fora.
        /// </summary>
        public void DesequiparItem(Personagem apostolo, Fases fase)
        {
            Boneco(apostolo)[(int)fase - 1] = null;
            SalvarItens();
        }

        /// <summary>
        /// As peças que ESTE apóstolo veste, uma por slot. Não cria boneco: perguntar o que alguém
        /// veste não pode gravar uma entrada vazia no save — e quem pergunta são as telas, o tempo
        /// todo, por 36 apóstolos.
        /// </summary>
        public Item?[] ObterEquipados(Personagem apostolo)
            => vestidos.TryGetValue((apostolo.Faccao, (Slot)apostolo.Slot), out Item?[]? boneco)
                ? boneco
                : new Item?[Slots];

        /// <summary>
        /// De quem é esta peça — nulo se ela está no baú. Casa por <see cref="Item.Id"/>, não por
        /// (facção, fase): duas Manoplas do Reino são peças DIFERENTES agora, e comparar pelo slot
        /// marcaria as duas.
        ///
        /// É ele que responde o "de quem estou tirando isso?" da tela, e por isso devolve o apóstolo
        /// e não um bool: o emoji do portador é o que impede o jogador de desnudar um aliado sem
        /// perceber e só descobrir na fase seguinte.
        /// </summary>
        public Personagem? PortadorDe(Item item)
        {
            int slot = (int)item.Fase - 1;
            foreach (var ((faccao, apostolo), boneco) in vestidos)
                if (boneco[slot] != null && boneco[slot]!.Id == item.Id)
                    return _personagens.ObterPersonagem(faccao, apostolo);
            return null;
        }

        /// <summary>Todo o acervo, na ordem em que caiu.</summary>
        public List<Item> ObterObtidos() => inventario;

        // Houve aqui um `TotaisEquipados`, que somava o conjunto POR STAT pra um painel "Bônus do
        // arsenal" ao lado do boneco. Ele morreu com o painel (ago/2026): somar item com item produz
        // "+5%", e 5% de quê só se sabe escolhendo um apóstolo. O número que interessa é o DELE, já
        // somado, e quem o produz é o mesmo caminho da luta (`FluxoDoFront.BonusDe`).

        /// <summary>
        /// Aplica ao combatente os stats das peças QUE ELE VESTE — as do apóstolo por trás do
        /// <see cref="Combate.Personagem"/>, e não um conjunto global que valia pra todo mundo.
        ///
        /// <b>Só se chama isto sobre um <see cref="Jogador"/>.</b> O inimigo da campanha é CÓPIA de um
        /// apóstolo de verdade (`Personagem.ComNivel`) e carrega a mesma `(Facção, Slot)`: chamar aqui
        /// com ele vestiria o inimigo com as peças do jogador, e a chave não teria como perceber.
        /// </summary>
        public void AplicarItens(Combate combate)
            => combate.AplicarItens(ObterEquipados(combate.Personagem).Where(i => i != null).Select(i => i!));

        #endregion

        #region O nível da peça

        /// <summary>
        /// O que a fase pagou de uso às peças VESTIDAS (docs/GDD-itens.md §Como o nível sobe).
        ///
        /// <b>Só quem estava em campo ganha</b> — peça no baú não sobe. E o ganho é por CICLO de
        /// combate, não por turno do portador: contar turno faria o apóstolo rápido subir equipamento
        /// em dobro, e a Velocidade viraria duplamente dominante (§Contar rodada, não ação).
        ///
        /// <b>"Em campo" é o TIME que lutou</b>, e passou a ser verdade com o vínculo: enquanto os
        /// slots eram globais, quem lutasse pagava as mesmas 7 peças, e a peça do banco subia junto.
        /// </summary>
        public void CreditarUso(IEnumerable<Personagem> time, Dificuldade dificuldade, double ciclos, bool venceu)
        {
            int pontos = Po.PontosDaFase(dificuldade, ciclos, venceu);

            foreach (Item? item in time.SelectMany(ObterEquipados))
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
        /// O ⚙️ ESMERIL: a peça deixa de existir e vira pó (docs/GDD-itens.md §O ESMERIL).
        ///
        /// <b>Recusa peça VESTIDA.</b> Moer o que alguém está usando desnudaria um apóstolo sem que o
        /// gesto tenha dito isso — tirar é o ✕ Remover da Armaria, e é uma decisão à parte.
        ///
        /// Quanto devolve é <see cref="Po.Esmerilhar"/>, pela faixa da peça; os pontos de nível dela
        /// não voltam, e é o que impede o esmeril de transferir progresso de uma peça pra outra.
        /// </summary>
        public MotivoRecusa Esmerilhar(Item peca)
        {
            if (PortadorDe(peca) != null) return MotivoRecusa.Vestida;
            if (!inventario.Remove(peca)) return MotivoRecusa.ForaDoAcervo;

            _po.Creditar(Po.Esmerilhar(peca.Raridade));
            SalvarItens();
            return MotivoRecusa.Nenhum;
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

        /// <summary>
        /// Persiste o acervo e quem veste o quê. Os dois andam juntos, sempre.
        ///
        /// Boneco inteiro vazio não vai pro disco: apóstolo que nunca vestiu nada não precisa de
        /// linha no save, e são 36 deles contra os 4 que costumam lutar.
        /// </summary>
        public void SalvarItens()
        {
            _repo.Salvar(ChaveInventario, inventario);
            _repo.Salvar(ChaveVestidos, vestidos
                .Where(v => v.Value.Any(i => i != null))
                .Select(v => new VestidoPeloApostolo(v.Key.Item1, v.Key.Item2, v.Value.Select(i => i?.Id).ToArray()))
                .ToList());
        }

        /// <summary>
        /// Devolve o arsenal ao estado de jogo novo — disco E memória (ver
        /// <see cref="CapitulosService.Resetar"/> pra o porquê dos dois).
        /// </summary>
        public void Resetar()
        {
            _repo.Excluir(ChaveInventario);
            _repo.Excluir(ChaveVestidos);
            _repo.Excluir(ChaveEquipadosGlobais);
            _repo.Excluir(ChaveLegado);
            vestidos = new();
            inventario.Clear();
            _po.Resetar();
        }

        #endregion
    }

    /// <summary>
    /// O boneco de um apóstolo no save: a identidade dele e os IDs das 7 peças (null = slot vazio).
    /// Mesma forma do <see cref="ApostoloProgredido"/>, e pelo mesmo motivo — `(Facção, Slot)` é o
    /// que sobrevive a um roster que cresce.
    /// </summary>
    public record VestidoPeloApostolo(Faccao Faccao, Slot Slot, Guid?[] Ids);
}
