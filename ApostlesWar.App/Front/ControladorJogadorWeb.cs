using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;
using ApostlesWar.Application.Controllers;

namespace ApostlesWar.App.Front
{
    /// <summary>
    /// O jogador humano decidindo por CLIQUE. Implementa <see cref="IControladorDeTurno"/> — o mesmo
    /// seam do ControladorJogador (console) e do ControladorBot —, então o motor não percebe diferença.
    ///
    /// Não reaproveita os laços de cursor do console de propósito: lá a navegação é "move ▶, confirma
    /// com Enter"; aqui é o fluxo que o Gabriel desenhou — clicou na habilidade, clicou no inimigo,
    /// USOU. Forçar o mesmo código nos dois exigiria o front fingir teclas, o que é pior que ter duas
    /// implementações pequenas de uma interface que já existe pra isso.
    /// </summary>
    internal class ControladorJogadorWeb : IControladorDeTurno
    {
        private readonly SessaoDoFront _sessao;
        private readonly PonteWebView2 _ponte;

        public ControladorJogadorWeb(SessaoDoFront sessao, PonteWebView2 ponte)
        {
            _sessao = sessao;
            _ponte = ponte;
        }

        public HabilidadeAtiva? EscolherAcao(Combate atacante, List<Combate> aliados, List<Combate> defensores)
        {
            // Quem age é humano ⇒ os aliados DELE são o lado esquerdo da tela, por definição.
            _sessao.DefinirLadoDoJogador(aliados, defensores);

            var habilidades = atacante.Personagem.Habilidades.OfType<HabilidadeAtiva>().ToList();

            _sessao.QuemAge = atacante;
            _sessao.Fase = FaseDaTela.EscolhendoAcao;
            _sessao.HabilidadesDoTurno = habilidades;
            _sessao.AlvosValidos = new List<Combate>();
            _sessao.Mensagem = $"Vez de {atacante.Personagem.Simbolo} {atacante.Personagem.Nome}";
            _ponte.LimparPendentes();   // ignora cliques que sobraram da animação anterior
            _sessao.Publicar();

            while (true)
            {
                MensagemDoFront msg = _ponte.Esperar();

                if (msg.Tipo == "encerrar") return null;   // janela fechou: aborta o turno

                if (msg.Tipo == "habilidade")
                {
                    if (msg.Valor < 0 || msg.Valor >= habilidades.Count) continue;
                    HabilidadeAtiva escolhida = habilidades[msg.Valor];

                    // Cooldown é regra de jogo: a tela já pinta como indisponível, mas não confiamos
                    // nela — clique em habilidade travada simplesmente não passa.
                    if (!atacante.Cooldowns[escolhida].Disponivel) continue;

                    return escolhida;
                }
            }
        }

        public Combate? EscolherAlvo(List<Combate> disponiveis, List<Combate> aliados, List<Combate> defensores)
        {
            // Alvo único e óbvio: não faz o jogador clicar por obrigação.
            if (disponiveis.Count == 1) return disponiveis[0];

            _sessao.Fase = FaseDaTela.EscolhendoAlvo;
            _sessao.AlvosValidos = disponiveis;
            _sessao.Mensagem = "Escolha o alvo";
            _ponte.LimparPendentes();
            _sessao.Publicar();

            while (true)
            {
                MensagemDoFront msg = _ponte.Esperar();

                if (msg.Tipo == "encerrar") return null;

                // Direito de arrependimento: volta pro menu de habilidades (o Esc do console).
                if (msg.Tipo == "cancelar") return null;

                if (msg.Tipo == "alvo")
                {
                    Combate? alvo = _sessao.PorId(msg.Valor);
                    if (alvo is null || !disponiveis.Contains(alvo)) continue;   // clique inválido: ignora
                    return alvo;
                }
            }
        }
    }
}
