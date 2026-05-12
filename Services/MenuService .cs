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

        /// <summary>
        /// Exibe a lista de campeões desbloqueados para seleção do time
        /// </summary>
        public void MenuSelecaoTime(List<Personagem> desbloqueados, int selecionado)
        {
            Console.Clear();
            Console.WriteLine("=====Apostle's War=====\n");
            Console.WriteLine("Escolha 4 campeões para o seu time:\n");
            for (int i = 0; i < desbloqueados.Count; i++)
            {
                Jogador temp = new Jogador(desbloqueados[i]);
                _arsenalService.AplicarItens(temp);
                string cursor = selecionado == i + 1 ? "▶" : " ";
                Console.WriteLine($"{cursor} {i + 1} - {desbloqueados[i].Simbolo} {desbloqueados[i].Nome} | HP:{temp.HPAtual} ATK:{temp.Ataque} DEF:{temp.Defesa}");
            }
        }

        /// <summary>
        /// Exibe os itens obtidos e retorna o item escolhido pelo jogador,
        /// ou null se o jogador pressionar Esc para voltar.
        /// </summary>
        public Item? MenuInventario()
        {
            Console.Clear();
            Console.WriteLine("=====Inventário=====\n");
            Console.WriteLine("Itens equipados:");
            Item?[] itensEquipados = _arsenalService.ObterEquipados();
            for (int i = 0; i < itensEquipados.Length; i++)
            {
                Item? item = itensEquipados[i];
                if (item == null)
                    Console.WriteLine($"Slot {i + 1} - vazio");
                else
                    Console.WriteLine($"Slot {i + 1} - {item.Simbolo} {item.Nome} ({item.Faccao}) {item.NomeStat()} {item.ValorFormatado()}");
            }

            Console.WriteLine("\nItens obtidos:");
            List<Item> obtidos = _arsenalService.ObterObtidos();
            for (int i = 0; i < obtidos.Count; i++)
                Console.WriteLine($"{i + 1} - {obtidos[i].Simbolo} {obtidos[i].Nome} ({obtidos[i].Faccao}) {obtidos[i].NomeStat()} +{obtidos[i].ValorFormatado()}");

            Console.Write("Digite o número do item para equipar ou Esc para voltar: ");
            ConsoleKeyInfo first = Console.ReadKey(false);

            if (first.Key == ConsoleKey.Escape) return null;

            string input = first.KeyChar + Console.ReadLine();

            if (int.TryParse(input, out int escolha) && escolha >= 1 && escolha <= obtidos.Count)
                return obtidos[escolha - 1];

            Console.WriteLine("Opção inválida.");
            Console.ReadLine();
            return MenuInventario(); // tenta de novo
        }

        public void ExibirPartida(List<Combate> jogadores, List<Combate> inimigos)
        {
            Console.Clear();
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
            Console.WriteLine($"{cursor1} 1 - ⚔️ Atacar");

            int i = 2;
            foreach (Habilidade hab in atacante.Personagem.Habilidades)
            {
                if (hab is HabilidadeAtiva)
                {
                    var cd = atacante.Cooldowns[hab];
                    string relogio = ObterRelogio(cd.TurnosRestantes, cd.CooldownTotal);
                    string cursor = acaoSelecionada == i ? "▶" : " ";
                    Console.WriteLine($"{cursor} {i} - {hab.Simbolo} {hab.Nome} {relogio}");
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
                        Console.WriteLine($"{hab.Simbolo} {hab.Nome} {relogio}");
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
            string[] numeros = { "①","②","③","④","⑤","⑥","⑦","⑧","⑨","⑩" };
            if (numero >= 1 && numero <= 10)
                return numeros[numero - 1];
            return numero.ToString();
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


        #endregion
    }
}
