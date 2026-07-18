using GHUtils;

namespace ApostlesWar.Services
{
    /// <summary>
    /// Controlador do JOGADOR humano: a ação vem do menu (navegação com cursor, pula habilidades em
    /// cooldown) e o alvo do menu de alvo. Envolve o MenuService (o I/O de console). No modo
    /// automático, o jogador troca este controlador por um automático no composition root — o loop de
    /// combate não muda.
    /// </summary>
    internal class ControladorJogador : IControladorDeTurno
    {
        private readonly MenuService _menuService;

        public ControladorJogador(MenuService menuService) => _menuService = menuService;

        public HabilidadeAtiva EscolherAcao(Combate atacante, List<Combate> aliados, List<Combate> defensores)
        {
            var habilidadesAtivas = atacante.Personagem.Habilidades.OfType<HabilidadeAtiva>().ToList();
            int totalOpcoes = habilidadesAtivas.Count;
            int acao = 1;

            while (true)
            {
                Console.Clear();
                _menuService.ExibirPartida(aliados, defensores);
                _menuService.ExibirAcoes(atacante, acao);

                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) break;

                int novaAcao = ConsoleUtils.SelecionarComCursor(acao, 1, totalOpcoes, key.Key);
                bool descendo = key.Key == ConsoleKey.S || key.Key == ConsoleKey.DownArrow;

                // Pula habilidades em cooldown. A1 (AtaqueBasico) tem cooldown 0 sempre,
                // então nunca é pulada — a A1 é sempre selecionável.
                while (novaAcao != acao)
                {
                    var hab = habilidadesAtivas[novaAcao - 1];
                    if (atacante.Cooldowns[hab].Disponivel) break;

                    int proximo = descendo ? novaAcao + 1 : novaAcao - 1;
                    if (proximo < 1 || proximo > totalOpcoes)
                    {
                        novaAcao = acao;
                        break;
                    }
                    novaAcao = proximo;
                }

                acao = novaAcao;
            }

            return habilidadesAtivas[acao - 1];
        }

        public Combate EscolherAlvo(List<Combate> disponiveis, List<Combate> aliados, List<Combate> defensores)
            => _menuService.EscolherAlvoNaTela(disponiveis, aliados, defensores);
    }
}
