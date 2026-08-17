using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;
using ApostlesWar.Application.Controllers;

namespace ApostlesWar.Presentation.Front
{
    /// <summary>
    /// O jogador humano decidindo por CLIQUE. Implementa <see cref="IControladorDeTurno"/> — o mesmo
    /// seam do <see cref="ControladorBot"/> —, então o motor não percebe diferença entre humano e IA.
    ///
    /// É esse seam que deixa o MODO AUTOMÁTICO barato: uma terceira impl, trocada no composition
    /// root, sem o loop de combate saber de nada.
    /// </summary>
    internal class ControladorJogadorWeb : IControladorDeTurno
    {
        private readonly SessaoDoFront _sessao;
        private readonly PonteWebView2 _ponte;

        /// <summary>
        /// O cérebro que assume quando o jogador liga o automático. É o MESMO que joga pelo inimigo —
        /// instância própria, só pra o alvo que ele memoriza entre escolher-ação e escolher-alvo não
        /// se misturar com o do adversário.
        /// </summary>
        private readonly ControladorBot _automatico;

        // Sair pedido DURANTE a escolha de alvo: lá o null significa "volta pra ação" (não aborta),
        // então marcamos aqui e a próxima EscolherAcao aborta de fato. Ver EscolherAlvo.
        private bool _sairSolicitado;

        public ControladorJogadorWeb(SessaoDoFront sessao, PonteWebView2 ponte, ControladorBot automatico)
        {
            _sessao = sessao;
            _ponte = ponte;
            _automatico = automatico;
        }

        public HabilidadeAtiva? EscolherAcao(Combate atacante, List<Combate> aliados, List<Combate> defensores)
        {
            // Sair clicado na fase de alvo só cancela o alvo (volta pra cá); é AQUI que ele vira abort.
            if (_sairSolicitado) { _sairSolicitado = false; return null; }

            // Quem age é humano ⇒ os aliados DELE são o lado esquerdo da tela, por definição.
            _sessao.DefinirLadoDoJogador(aliados, defensores);

            // O interruptor é lido AQUI, no começo de cada decisão — é o que faz ligar e desligar
            // valerem "entre turnos": o turno que o cérebro já começou vai até o fim, e o controle
            // volta na próxima pergunta.
            if (_ponte.AutoLigado)
            {
                EntregarFocoAoCerebro();
                return _automatico.EscolherAcao(atacante, aliados, defensores);
            }

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

                // Janela fechou OU o jogador pediu pra sair da batalha: null aborta a partida
                // (o motor lança BatalhaAbortada, a Arena captura e volta pro menu).
                if (msg.Tipo == "encerrar" || msg.Tipo == "sair") return null;

                // Ligou o automático enquanto este turno esperava um clique: a mensagem serviu pra
                // acordar a espera; quem decide agora é o cérebro.
                if (msg.Tipo == "auto" && _ponte.AutoLigado)
                {
                    EntregarFocoAoCerebro();
                    return _automatico.EscolherAcao(atacante, aliados, defensores);
                }

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

        /// <summary>
        /// Traduz o FOCO (o inimigo que o jogador apontou) de id pra combatente e o entrega ao
        /// cérebro, no instante em que ele vai decidir.
        ///
        /// Esta tradução mora AQUI porque é o único ponto que enxerga os dois lados: a ponte guarda
        /// um int (não conhece o domínio) e o <see cref="ControladorBot"/> vive na Application, que
        /// não pode enxergar a Presentation. Ler a cada decisão, e não uma vez, é o que faz o jogador
        /// poder trocar de alvo no meio da luta — mesma regra do interruptor do automático.
        /// </summary>
        private void EntregarFocoAoCerebro()
            => _automatico.AlvoPreferido = _sessao.PorId(_ponte.FocoDoJogador);

        public Combate? EscolherAlvo(List<Combate> disponiveis, List<Combate> aliados, List<Combate> defensores)
        {
            // No automático o alvo vem do cérebro, que já o elegeu junto com a habilidade.
            if (_ponte.AutoLigado)
                return _automatico.EscolherAlvo(disponiveis, aliados, defensores);

            // Não pôr atalho de "alvo único ⇒ escolhe sozinho" aqui: o passo de alvo é também o de
            // CONFIRMAÇÃO (o jogador vê quem vai levar e ainda pode desistir com Esc), e pulá-lo faz
            // a habilidade disparar sem direito a mudar de ideia.
            _sessao.Fase = FaseDaTela.EscolhendoAlvo;
            _sessao.AlvosValidos = disponiveis;
            _sessao.Mensagem = "Escolha o alvo";
            _ponte.LimparPendentes();
            _sessao.Publicar();

            while (true)
            {
                MensagemDoFront msg = _ponte.Esperar();

                if (msg.Tipo == "encerrar") return null;

                // Sair aqui não pode abortar direto (null = "volta pra ação"): marca e deixa a próxima
                // EscolherAcao abortar. O jogador vê o menu de ação piscar e a batalha encerra.
                if (msg.Tipo == "sair") { _sairSolicitado = true; return null; }

                // Direito de arrependimento: volta pro menu de habilidades.
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
