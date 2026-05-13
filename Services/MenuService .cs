using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text;
using GHUtils;
using static GHUtils.ConsoleUtils;
using static GHUtils.Helper;

namespace v1_Apostle_s_War.Services
{
    internal class MenuService
    {
        #region Construtor

        private readonly FaccaoService _faccaoService;
        private readonly ArsenalService _arsenalService;
        private readonly CapitulosService _capitulosService;
        #endregion


        public MenuService(FaccaoService faccaoService, ArsenalService arsenalService, CapitulosService capitulosService)
        {
            _faccaoService = faccaoService;
            _arsenalService = arsenalService;
            _capitulosService = capitulosService;
        }
        #region Menu

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

        public void ExibirPartida(List<Combate> jogadores, List<Combate> inimigos)
        {
            Console.WriteLine("Seu time:");
            foreach (Combate j in jogadores)
            {
                string status = string.Join(" ", j.StatusAtivos.Select(s => $"{s.Nome} {ObterNumeroEmoji(s.TurnosRestantes)}"));
                Console.WriteLine($"{j.Personagem.Simbolo} {j.Personagem.Nome} | HP:{j.HPAtual} ATK:{j.Ataque} DEF:{j.Defesa} {status}");
            }

            Console.WriteLine("\nInimigos:");
            int i = 1;
            foreach (Combate inimigo in inimigos.Where(d => d.EstaVivo()))
            {
                string status = string.Join(" ", inimigo.StatusAtivos.Select(s => $"{s.Nome} {ObterNumeroEmoji(s.TurnosRestantes)}"));
                Console.WriteLine($"{i} - {inimigo.Personagem.Simbolo} {inimigo.Personagem.Nome} | HP:{inimigo.HPAtual} ATK:{inimigo.Ataque} DEF:{inimigo.Defesa} {status}");
                i++;
            }
        }

        public void ExibirAcoes(Combate atacante, int acaoSelecionada = 1)
        {
            Console.WriteLine("\nAções:");
            string cursor1 = acaoSelecionada == 1 ? "▶" : " ";
            Console.WriteLine($"{cursor1} 1 - ⚔️  Atacar");

            int i = 2;
            foreach (Habilidade hab in atacante.Personagem.Habilidades)
            {
                if (hab is HabilidadeAtiva)
                {
                    var cd = atacante.Cooldowns[hab];
                    string relogio = ObterRelogio(cd.TurnosRestantes, cd.CooldownTotal);
                    string disponivel = cd.Disponivel ? "✅" : "🟣";
                    string cursor = acaoSelecionada == i ? "▶" : " ";
                    Console.WriteLine($"{cursor} {i} - {hab.Simbolo} {hab.Nome} {disponivel} {relogio}  {hab.Descricao}");
                    i++;
                }
            }

            bool temPassiva = atacante.Personagem.Habilidades.Any(h => h is HabilidadePassiva);
            if (temPassiva)
            {
                Console.WriteLine("\nPassivas:");
                foreach (Habilidade hab in atacante.Personagem.Habilidades)
                {
                    if (hab is HabilidadePassiva)
                    {
                        var cd = atacante.Cooldowns[hab];
                        string relogio = ObterRelogio(cd.TurnosRestantes, cd.CooldownTotal);
                        string disponivel = cd.Disponivel ? "✅" : "🟣";
                        Console.WriteLine($"  {hab.Simbolo} {hab.Nome} {disponivel} {relogio}  {hab.Descricao}");
                    }
                }
            }
        }

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

        private string ObterNumeroEmoji(int numero)
        {
            string[] numeros = { "①", "②", "③", "④", "⑤", "⑥", "⑦", "⑧", "⑨", "⑩" };
            if (numero >= 1 && numero <= 10)
                return numeros[numero - 1];
            return $"({numero})";
        }


        /// <summary>
        /// Exibe o dano causado, HP restante do alvo e indicação de crítico.
        /// </summary>
        public void ExibirResultadoAtaque(Combate atacante, Combate alvo, ResultadoAtaque resultado)
        {
            string critico = resultado.Critico ? " 💥 ATAQUE CRÍTICO!" : "";
            Console.WriteLine($"{atacante.Personagem.Simbolo} causou {resultado.Dano} de dano em {alvo.Personagem.Simbolo} {alvo.Personagem.Nome}{critico}");
            Console.WriteLine($"HP de {alvo.Personagem.Simbolo}: {Math.Max(0, alvo.HPAtual)}/{alvo.HPMaximo}");
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
    (0, 2), // 1 Elmo
    (1, 2), // 2 Escudo
    (0, 1), // 3 Acessório (era Manopla)
    (1, 1), // 4 Peitoral
    (2, 2), // 5 Calça
    (2, 1), // 6 Bota
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
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Escape) return -1;
                if (key.Key == ConsoleKey.Enter) return slot;

                slot = MoverNoBoneco(slot, key.Key);
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
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Escape) return null;
                if (key.Key == ConsoleKey.Enter) return itensDoTipo[idx];

                // navegação horizontal
                if (key.Key == ConsoleKey.A || key.Key == ConsoleKey.LeftArrow)
                    idx = Math.Max(0, idx - 1);
                if (key.Key == ConsoleKey.D || key.Key == ConsoleKey.RightArrow)
                    idx = Math.Min(itensDoTipo.Count - 1, idx + 1);
            }
        }

        /// <summary>
        /// Calcula o próximo slot no boneco com base na tecla (WASD).
        /// </summary>
        private int MoverNoBoneco(int atual, ConsoleKey tecla)
        {
            var (linha, coluna) = _posicoesBoneco[atual];
            int novaLinha = linha;
            int novaColuna = coluna;

            if (tecla == ConsoleKey.W || tecla == ConsoleKey.UpArrow) novaLinha--;
            if (tecla == ConsoleKey.S || tecla == ConsoleKey.DownArrow) novaLinha++;
            if (tecla == ConsoleKey.A || tecla == ConsoleKey.LeftArrow) novaColuna--;
            if (tecla == ConsoleKey.D || tecla == ConsoleKey.RightArrow) novaColuna++;

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
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Escape) return new List<Personagem>();

                if (key.Key == ConsoleKey.Enter)
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

                bool sobe = key.Key == ConsoleKey.W || key.Key == ConsoleKey.UpArrow;
                bool desce = key.Key == ConsoleKey.S || key.Key == ConsoleKey.DownArrow;
                bool esq = key.Key == ConsoleKey.A || key.Key == ConsoleKey.LeftArrow;
                bool dir = key.Key == ConsoleKey.D || key.Key == ConsoleKey.RightArrow;

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


        #endregion
    }
}

