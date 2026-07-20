using ApostlesWar;
using ApostlesWar.Services;
using GHUtils;

namespace ApostlesWar.View
{
    /// <summary>
    /// View dos MENUS: telas de escolha de opção fora da luta (menu principal, capítulos, fases,
    /// inventário/boneco, seleção de time). Separada da CombateView (renderização da partida) na
    /// quebra do antigo MenuService. Depende dos services de dado (facção/arsenal/capítulos) só pra
    /// ler o que exibir. NOTA: a navegação lê input aqui dentro; a porta de entrada (IEntrada) é
    /// tema próprio, depois.
    /// </summary>
    internal class MenuView
    {
        private readonly FaccaoService _faccaoService;
        private readonly ArsenalService _arsenalService;
        private readonly CapitulosService _capitulosService;
        private readonly IEntrada _entrada;
        private readonly IApresentacao _apresentacao;

        public MenuView(FaccaoService faccaoService, ArsenalService arsenalService, CapitulosService capitulosService,
            IEntrada entrada, IApresentacao apresentacao)
        {
            _faccaoService = faccaoService;
            _arsenalService = arsenalService;
            _capitulosService = capitulosService;
            _entrada = entrada;
            _apresentacao = apresentacao;
        }

        // ===== Telas meta (fora do combate): confirmação de saída, créditos, vitória, avisos.
        // A renderização mora aqui (View); o GerenciadorDeJogo só orquestra (decide QUANDO exibir). =====

        /// <summary>Desenha o diálogo "sair do jogo?" com o cursor na opção atual (1=Sim, 2=Não).</summary>
        public void ExibirConfirmacaoSaida(int opcao)
        {
            Console.Clear();
            Console.WriteLine("Deseja sair do jogo?\n");
            Console.WriteLine(opcao == 1 ? "▶ 1 - Sim" : "  1 - Sim");
            Console.WriteLine(opcao == 2 ? "▶ 2 - Não" : "  2 - Não");
        }

        /// <summary>Rola os créditos (com a pausa dramática entre linhas via IApresentacao).</summary>
        public void ExibirCreditos()
        {
            string[] linhas =
            {
                "",
                "    ⚔️  Apostle's War  ⚔️",
                "",
                "    Obrigado por jogar, Apóstolo...",
                "",
                "    👑  Desenvolvido por: Gabriel Henrique Cé",
                "    🛠️  Versão: 1.0",
                "    🌑  GitHub: GabrielHenriqueCe",
                "",
                "    Que a guerra dos Apóstolos nunca termine. 🌬️",
                "",
            };

            Console.Clear();
            foreach (string linha in linhas)
            {
                Console.WriteLine(linha);
                _apresentacao.AguardarAnimacao(180);
            }

            _apresentacao.AguardarAnimacao(2000);
        }

        /// <summary>Desenha a tela de vitória: campeões novos e item dropado (o GerenciadorDeJogo já computou).</summary>
        public void ExibirTelaVitoria(List<Personagem> novosCampeoes, Item? itemDropado)
        {
            Console.Clear();
            Console.WriteLine("=====Fase Concluída!=====\n");

            foreach (Personagem p in novosCampeoes)
                Console.WriteLine($"Novo campeão: {p.Simbolo} {p.Nome}!");

            if (itemDropado != null)
                Console.WriteLine($"Novo item: {itemDropado.Simbolo} {itemDropado.Nome} | {itemDropado.NomeStat()} + {itemDropado.ValorFormatado()}");

            Console.WriteLine("\nPressione Enter para continuar...");
        }

        /// <summary>Tela de derrota (perdeu a batalha ou encerrou no meio). Temática, com a Deusa.</summary>
        public void ExibirTelaDerrota()
        {
            Console.Clear();
            Console.WriteLine("═════ 🕯️ Derrota 🕯️ ═════\n");
            Console.WriteLine("Você tombou nesta batalha, Apóstolo.\n");
            Console.WriteLine("Mas a Deusa não abandona os que lutam em Seu nome. Ela viu sua coragem —");
            Console.WriteLine("e sabe que você há de se erguer, mais forte, e reivindicar a vitória.\n");
            Console.WriteLine("Levante-se. A guerra dos Apóstolos ainda não terminou. ⚜️\n");
            Console.WriteLine("Pressione Enter para continuar...");
        }

        /// <summary>Exibe um aviso curto e segura na tela por um tempo (ex: "inventário vazio").</summary>
        public void ExibirAviso(string mensagem, int ms)
        {
            Console.WriteLine(mensagem);
            _apresentacao.AguardarAnimacao(ms);
        }

        public void ExibirMenu(int selecionado)
        {
            Console.Clear();
            Console.WriteLine("=====Apostle's War=====\n");
            string[] opcoes = { "📜 - Jogar Campanha", "💰 - Inventário" };
            for (int i = 0; i < opcoes.Length; i++)
            {
                string cursor = selecionado == i + 1 ? "▶" : " ";
                Console.WriteLine($"{cursor} {i + 1} - {opcoes[i]}");
            }
            Console.WriteLine("\nEsc - Sair");
        }

        public void MenuCapitulos(int selecionado)
        {
            Console.Clear();
            Console.WriteLine("=====Apostle's War=====\n");

            var faccoes = Enum.GetValues<Faccao>().Where(f => f != Faccao.Humanos).ToList();
            for (int i = 0; i < faccoes.Count; i++)
            {
                Faccao faccao = faccoes[i];
                string icone;
                if (!_capitulosService.EstaCapituloDesbloqueado(faccao))
                    icone = " ❌ ";
                else if (_capitulosService.CapituloConcluido(faccao))
                    icone = " ✅ ";
                else
                    icone = " ☑️  ";

                string cursor = selecionado == i + 1 ? "▶" : " ";
                Console.WriteLine($"{cursor} {(int)faccao} - {icone} {_faccaoService.ObterSimbolo(faccao)}  {Helper.GetDescricao(faccao)}");
            }
            Console.WriteLine("\nEsc - Voltar");
        }

        public void MenuFases(Faccao faccao, int selecionado)
        {
            Console.Clear();
            Console.WriteLine("=====Apostle's War=====\n");

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

                string cursor = selecionado == idx + 1 ? "▶" : " ";
                Console.WriteLine($"{cursor} {(int)fase} - {icone}  {Helper.GetDescricao(fase)}");
            }
            Console.WriteLine("\nEsc - Voltar");
        }

        /// <summary>
        /// Layout do boneco. Mapeia cada slot (índice = Fases - 1) para uma posição (linha, coluna) num grid 3x3.
        ///   [0,1]=Acessório   [0,2]=Elmo
        ///   [1,0]=Arma        [1,1]=Peitoral   [1,2]=Escudo
        ///   [2,1]=Bota        [2,2]=Calça
        /// </summary>
        private static readonly (int linha, int coluna)[] _posicoesBoneco = new[]
        {
    (1, 0), // 0 Arma
    (0, 1), // 1 Elmo
    (1, 2), // 2 Escudo
    (0, 0), // 3 Acessório
    (1, 1), // 4 Peitoral
    (2, 1), // 5 Calça
    (2, 0), // 6 Bota
};

        private static readonly string[] _nomesSlot =
        {
    "Arma", "Elmo", "Escudo", "Acessório", "Peitoral", "Calça", "Bota"
};

        /// <summary>
        /// Desenha o boneco com cursor no slot selecionado.
        /// </summary>
        private void DesenharBoneco(int slotComCursor, int slotEmEdicao)
        {
            Item?[] equipados = _arsenalService.ObterEquipados();
            string[,] grid = new string[3, 3];
            for (int l = 0; l < 3; l++)
                for (int c = 0; c < 3; c++)
                    grid[l, c] = "      ";

            for (int i = 0; i < 7; i++)
            {
                var (linha, coluna) = _posicoesBoneco[i];
                string emoji = equipados[i]?.Simbolo ?? "⬜";
                string prefixo;
                if (i == slotEmEdicao) prefixo = "🔹";
                else if (i == slotComCursor) prefixo = "▶ ";
                else prefixo = "   ";
                grid[linha, coluna] = $"{prefixo}{emoji}  ";
            }

            for (int l = 0; l < 3; l++)
            {
                for (int c = 0; c < 3; c++)
                    Console.Write(grid[l, c]);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Tela inicial do inventário: boneco navegável.
        /// </summary>
        public void ExibirBonecoInventario(int slotSelecionado)
        {
            Console.Clear();
            Console.WriteLine("=====Inventário=====\n");

            DesenharBoneco(slotSelecionado, slotEmEdicao: -1);

            Console.WriteLine();
            Item? selecionado = _arsenalService.ObterEquipados()[slotSelecionado];
            if (selecionado != null)
                Console.WriteLine($"{selecionado.Simbolo} {selecionado.Nome} ({selecionado.Faccao}) {selecionado.NomeStat()} +{selecionado.ValorFormatado()}");
            else
                Console.WriteLine($"Slot {_nomesSlot[slotSelecionado]} - vazio");

            Console.WriteLine("\nWASD - Navegar | Enter - Trocar item | Esc - Voltar");
        }

        /// <summary>
        /// Tela de troca de item: boneco com slot em edição + comparação + lista horizontal.
        /// </summary>
        public void ExibirTrocaItem(int slotEmEdicao, List<Item> itensDoTipo, int idxItemSelecionado)
        {
            Console.Clear();
            Console.WriteLine("=====Inventário — Trocar Item=====\n");

            DesenharBoneco(slotComCursor: -1, slotEmEdicao: slotEmEdicao);

            Item? equipadoAgora = _arsenalService.ObterEquipados()[slotEmEdicao];
            Item itemNovo = itensDoTipo[idxItemSelecionado];

            Console.WriteLine();
            Console.WriteLine($"Equipado: {(equipadoAgora == null ? "(nenhum)" : $"{equipadoAgora.Simbolo} {equipadoAgora.Nome} ({equipadoAgora.Faccao}) {equipadoAgora.NomeStat()} +{equipadoAgora.ValorFormatado()}")}");
            Console.WriteLine($"Novo:     {itemNovo.Simbolo} {itemNovo.Nome} ({itemNovo.Faccao}) {itemNovo.NomeStat()} +{itemNovo.ValorFormatado()}");

            if (equipadoAgora != null && equipadoAgora.TipoStat == itemNovo.TipoStat)
            {
                double diff = itemNovo.Valor - equipadoAgora.Valor;
                string diffFormatado = itemNovo.TipoStat switch
                {
                    TipoStat.ATKFlat or TipoStat.HPFlat or TipoStat.DEFFlat => $"{(diff >= 0 ? "+" : "")}{(int)diff}",
                    _ => $"{(diff >= 0 ? "+" : "")}{diff * 100:F0}%"
                };
                string seta = diff > 0 ? "▲" : (diff < 0 ? "▼" : "=");
                Console.WriteLine($"Diferença: {seta} {diffFormatado}");
            }

            Console.WriteLine("\nItens disponíveis:");
            for (int i = 0; i < itensDoTipo.Count; i++)
            {
                string prefixo = i == idxItemSelecionado ? "▶" : " ";
                Console.Write($"{prefixo}{itensDoTipo[i].Simbolo}  ");
            }
            Console.WriteLine();

            Console.WriteLine("\nA/D - Navegar | Enter - Equipar | Esc - Voltar");
        }

        /// <summary>
        /// Navega o boneco. Retorna o slot escolhido para troca (0..6), ou -1 se Esc.
        /// </summary>
        public int NavegarBoneco()
        {
            int slot = 0;
            while (true)
            {
                ExibirBonecoInventario(slot);
                Comando cmd = _entrada.Ler();

                if (cmd.Tipo == TipoComando.Cancelar) return -1;
                if (cmd.Tipo == TipoComando.Confirmar) return slot;

                slot = MoverNoBoneco(slot, cmd);
            }
        }

        /// <summary>
        /// Navega a lista de itens daquele tipo. Retorna o item escolhido, ou null se Esc.
        /// </summary>
        public Item? NavegarTrocaItem(int slotEmEdicao, List<Item> itensDoTipo)
        {
            if (itensDoTipo.Count == 0) return null;

            int idx = 0;
            while (true)
            {
                ExibirTrocaItem(slotEmEdicao, itensDoTipo, idx);
                Comando cmd = _entrada.Ler();

                if (cmd.Tipo == TipoComando.Cancelar) return null;
                if (cmd.Tipo == TipoComando.Confirmar) return itensDoTipo[idx];

                // navegação horizontal
                if (cmd.Tipo == TipoComando.Esquerda)
                    idx = Math.Max(0, idx - 1);
                if (cmd.Tipo == TipoComando.Direita)
                    idx = Math.Min(itensDoTipo.Count - 1, idx + 1);
            }
        }

        /// <summary>
        /// Calcula o próximo slot no boneco com base na tecla (WASD).
        /// </summary>
        private int MoverNoBoneco(int atual, Comando cmd)
        {
            var (linha, coluna) = _posicoesBoneco[atual];
            int novaLinha = linha;
            int novaColuna = coluna;

            if (cmd.Tipo == TipoComando.Cima) novaLinha--;
            if (cmd.Tipo == TipoComando.Baixo) novaLinha++;
            if (cmd.Tipo == TipoComando.Esquerda) novaColuna--;
            if (cmd.Tipo == TipoComando.Direita) novaColuna++;

            for (int i = 0; i < _posicoesBoneco.Length; i++)
            {
                if (_posicoesBoneco[i].linha == novaLinha && _posicoesBoneco[i].coluna == novaColuna)
                    return i;
            }

            return atual;
        }

        /// <summary>
        /// Resultado de uma iteração de navegação na seleção de time.
        /// </summary>
        public record ResultadoSelecaoTime(bool Cancelou, bool Confirmar);

        private const int COLUNAS_GRID = 4;

        /// <summary>
        /// Desenha a tela de seleção de time: 4 slots em cima, grid de campeões embaixo.
        /// </summary>
        private void DesenharSelecaoTime(
    Personagem?[] time,
    List<Personagem> desbloqueados,
    bool cursorNoBotao,
    bool cursorNoTime,
    int idxTime,
    int idxGrid)
        {
            Console.Clear();
            Console.WriteLine("=====Seleção de Time=====\n");

            // Botão de iniciar fase
            int qtdSelecionados = time.Count(p => p != null);
            string prefixoBotao = cursorNoBotao ? "▶ " : "  ";
            string estadoBotao = qtdSelecionados >= 1 ? "✅" : "❌";
            Console.WriteLine($"{prefixoBotao}{estadoBotao} Iniciar Fase ({qtdSelecionados}/4)\n");

            // Linha do time
            Console.Write("Time: ");
            for (int i = 0; i < 4; i++)
            {
                string prefixo = (cursorNoTime && idxTime == i) ? "▶" : " ";
                string emoji = time[i]?.Simbolo ?? "⬜";
                Console.Write($"{prefixo}{emoji}  ");
            }
            Console.WriteLine("\n");

            // Grid
            Console.WriteLine("Campeões:");
            for (int i = 0; i < desbloqueados.Count; i++)
            {
                bool jaSelecionado = time.Any(p => p == desbloqueados[i]);
                string prefixo;
                if (jaSelecionado) prefixo = "🔹";
                else if (!cursorNoTime && !cursorNoBotao && idxGrid == i) prefixo = "▶ ";
                else prefixo = "   ";

                Console.Write($"{prefixo}{desbloqueados[i].Simbolo}  ");

                if ((i + 1) % 4 == 0) Console.WriteLine();
            }
            if (desbloqueados.Count % 4 != 0) Console.WriteLine();

            // Info do personagem sob cursor
            Console.WriteLine();
            Personagem? sob = null;
            if (cursorNoTime) sob = time[idxTime];
            else if (!cursorNoBotao && idxGrid < desbloqueados.Count) sob = desbloqueados[idxGrid];

            if (sob != null)
            {
                Jogador temp = new Jogador(sob);
                _arsenalService.AplicarItens(temp);
                Console.WriteLine($"{sob.Simbolo} {sob.Nome} ({sob.Faccao}) | HP:{temp.HPAtual} ATK:{temp.Ataque} DEF:{temp.Defesa}");
                if (sob.Habilidades.Any())
                {
                    Console.WriteLine("Habilidades:");
                    foreach (Habilidade hab in sob.Habilidades)
                        Console.WriteLine($"  {hab.Simbolo} {hab.Nome} — {hab.Descricao}");
                }
            }

            Console.WriteLine("\nWASD - Navegar | Enter - Selecionar/Remover/Iniciar | Esc - Voltar");
        }

        /// <summary>
        /// Loop de navegação da seleção de time.
        /// Retorna time preenchido ou lista vazia se cancelou.
        /// </summary>
        public List<Personagem> NavegarSelecaoTime(List<Personagem> desbloqueados)
        {
            Personagem?[] time = new Personagem?[4];

            // Estados do cursor:
            // -1 = no botão "Iniciar Fase"
            //  0..3 = nos slots do time
            //  outros = no grid (controlado por idxGrid)
            bool cursorNoBotao = false;
            bool cursorNoTime = false;
            int idxTime = 0;
            int idxGrid = 0;

            int ProximoGridValido(int inicio, int direcao)
            {
                int i = inicio;
                while (i >= 0 && i < desbloqueados.Count)
                {
                    if (!time.Contains(desbloqueados[i])) return i;
                    i += direcao;
                }
                return inicio;
            }

            while (true)
            {
                DesenharSelecaoTime(time, desbloqueados, cursorNoBotao, cursorNoTime, idxTime, idxGrid);
                Comando cmd = _entrada.Ler();

                if (cmd.Tipo == TipoComando.Cancelar) return new List<Personagem>();

                if (cmd.Tipo == TipoComando.Confirmar)
                {
                    if (cursorNoBotao)
                    {
                        // Inicia fase com quem tiver no time
                        var selecionados = time.Where(p => p != null).Select(p => p!).ToList();
                        if (selecionados.Count >= 1) return selecionados;
                    }
                    else if (cursorNoTime)
                    {
                        // Remove e desloca pra esquerda
                        if (time[idxTime] != null)
                        {
                            for (int i = idxTime; i < 3; i++)
                                time[i] = time[i + 1];
                            time[3] = null;
                        }
                    }
                    else
                    {
                        // Adiciona ao primeiro slot vazio
                        if (idxGrid < desbloqueados.Count && !time.Contains(desbloqueados[idxGrid]))
                        {
                            int vazio = Array.FindIndex(time, p => p == null);
                            if (vazio >= 0)
                            {
                                time[vazio] = desbloqueados[idxGrid];
                                // Se completou 4, pula direto pro botão
                                if (time.All(p => p != null))
                                {
                                    cursorNoBotao = true;
                                    cursorNoTime = false;
                                }
                            }
                        }
                    }
                    continue;
                }

                bool sobe = cmd.Tipo == TipoComando.Cima;
                bool desce = cmd.Tipo == TipoComando.Baixo;
                bool esq = cmd.Tipo == TipoComando.Esquerda;
                bool dir = cmd.Tipo == TipoComando.Direita;

                if (cursorNoBotao)
                {
                    if (desce)
                    {
                        cursorNoBotao = false;
                        cursorNoTime = true;
                        idxTime = 0;
                    }
                }
                else if (cursorNoTime)
                {
                    if (esq) idxTime = Math.Max(0, idxTime - 1);
                    if (dir) idxTime = Math.Min(3, idxTime + 1);
                    if (sobe)
                    {
                        cursorNoTime = false;
                        cursorNoBotao = true;
                    }
                    if (desce)
                    {
                        cursorNoTime = false;
                        idxGrid = idxTime;
                        if (idxGrid >= desbloqueados.Count) idxGrid = desbloqueados.Count - 1;
                        if (time.Contains(desbloqueados[idxGrid])) idxGrid = ProximoGridValido(idxGrid, 1);
                    }
                }
                else
                {
                    if (esq)
                    {
                        int novo = idxGrid - 1;
                        if (novo >= 0 && time.Contains(desbloqueados[novo])) novo = ProximoGridValido(novo, -1);
                        if (novo >= 0) idxGrid = novo;
                    }
                    if (dir)
                    {
                        int novo = idxGrid + 1;
                        if (novo < desbloqueados.Count && time.Contains(desbloqueados[novo])) novo = ProximoGridValido(novo, 1);
                        if (novo < desbloqueados.Count) idxGrid = novo;
                    }
                    if (sobe)
                    {
                        int novo = idxGrid - 4;
                        if (novo < 0)
                        {
                            cursorNoTime = true;
                            idxTime = Math.Min(3, idxGrid % 4);
                        }
                        else
                        {
                            if (time.Contains(desbloqueados[novo])) novo = ProximoGridValido(novo, -1);
                            if (novo >= 0) idxGrid = novo;
                        }
                    }
                    if (desce)
                    {
                        int novo = idxGrid + 4;
                        if (novo < desbloqueados.Count)
                        {
                            if (time.Contains(desbloqueados[novo])) novo = ProximoGridValido(novo, 1);
                            if (novo < desbloqueados.Count) idxGrid = novo;
                        }
                    }
                }
            }
        }
    }
}
