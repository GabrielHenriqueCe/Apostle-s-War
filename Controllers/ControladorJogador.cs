using ApostlesWar.View;

namespace ApostlesWar.Controllers
{
    /// <summary>
    /// Controlador do JOGADOR humano: a ação vem do menu (navegação com cursor, pula habilidades em
    /// cooldown) e o alvo do menu de alvo. Envolve a CombateView (o I/O de console da partida). No modo
    /// automático, o jogador troca este controlador por um automático no composition root — o loop de
    /// combate não muda.
    /// </summary>
    internal class ControladorJogador : IControladorDeTurno
    {
        private readonly CombateView _combateView;
        private readonly IEntrada _entrada;

        public ControladorJogador(CombateView combateView, IEntrada entrada)
        {
            _combateView = combateView;
            _entrada = entrada;
        }

        public HabilidadeAtiva? EscolherAcao(Combate atacante, List<Combate> aliados, List<Combate> defensores)
        {
            var habilidadesAtivas = atacante.Personagem.Habilidades.OfType<HabilidadeAtiva>().ToList();
            int totalOpcoes = habilidadesAtivas.Count;
            int acao = 1;

            while (true)
            {
                Console.Clear();
                _combateView.ExibirPartida(aliados, defensores);
                _combateView.ExibirAcoes(atacante, acao);

                Comando cmd = _entrada.Ler();
                if (cmd.Tipo == TipoComando.Confirmar) break;

                // Esc no menu de ação = pedir pra encerrar a batalha. Confirmou → null (o CombateService
                // aborta). Desistiu → fica no menu.
                if (cmd.Tipo == TipoComando.Cancelar)
                {
                    if (_combateView.ConfirmarEncerramento()) return null;
                    continue;
                }

                int novaAcao = Navegacao.MoverCursor(acao, 1, totalOpcoes, cmd);
                bool descendo = cmd.Tipo == TipoComando.Baixo;

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

        public Combate? EscolherAlvo(List<Combate> disponiveis, List<Combate> aliados, List<Combate> defensores)
            => _combateView.EscolherAlvoNaTela(disponiveis, aliados, defensores);   // null = Esc = voltar
    }
}
