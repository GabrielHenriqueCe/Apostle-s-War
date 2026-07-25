using ApostlesWar.Domain;
namespace ApostlesWar.Application.Portas
{
    /// <summary>
    /// Porta de APRESENTAÇÃO DA PARTIDA — o que o fluxo de combate (CombateService) precisa mostrar.
    /// Irmã do <see cref="IApresentacao"/> (a espera): juntas, fecham o isolamento do motor em relação
    /// à tela. Hoje a webview é a única impl; nasceu com duas (a outra era o console), e é por isso que
    /// o motor não sabe desenhar nada.
    ///
    /// O CONTRATO É UM GATILHO, NÃO UM DESENHO. Os nomes são imperativos ("Exibir...") por herança da
    /// primeira impl, mas nenhuma impl é obrigada a desenhar imperativamente: a do front traduz cada
    /// chamada em (a) um retrato do estado serializado e (b) um evento pra animar. É isso que deixa a
    /// tela declarativa — e que faz trocar emoji por sprite ser mexida só de front, sem tocar no motor.
    ///
    /// NÃO entram aqui o menu de ação e o menu de alvo: escolher ação/alvo é decisão, não exibição.
    /// Quem decide é o <see cref="Controllers.IControladorDeTurno"/>, e cada pele tem o seu (no front,
    /// clique na habilidade + clique no alvo). Botar os dois aqui obrigaria a impl a carregar método
    /// morto sempre que a forma de escolher mudasse.
    /// </summary>
    public interface ITelaDeCombate
    {
        /// <summary>Prepara a tela pro próximo quadro. Numa tela que se redesenha do estado, é no-op.</summary>
        void LimparTela();

        /// <summary>O retrato dos dois times — é o estado completo da partida.</summary>
        void ExibirPartida(List<Combate> jogadores, List<Combate> inimigos);

        /// <summary>
        /// Início da Arena (PVP): a tela recebe os dois times NA ORDEM montada pra fixar os lados
        /// (equipe1=esquerda, equipe2=direita) independente de quem controla — na Arena os lados são
        /// como o jogador montou. A campanha (PVE) não chama isto: segue "o humano à esquerda".
        /// Uma tela sem lado fixo pode ignorar (no-op).
        /// </summary>
        void ExibirInicioArena(List<Combate> equipe1, List<Combate> equipe2);

        /// <summary>Resultado de um golpe (dano, crítico, escudo aparado) — o gancho de animação.</summary>
        void ExibirResultadoAtaque(Combate atacante, Combate alvo, EventoDano resultado);

        /// <summary>Dano de tick (veneno/queima) no início do turno.</summary>
        void ExibirDanoDeStatus(EventoDano r);

        /// <summary>Cura, venha de habilidade ou de reação (mensagem única — decisão do #7b).</summary>
        void ExibirCura(EventoCura c);

        /// <summary>Mensagem de passiva (sobreviveu, reviveu, refletiu...).</summary>
        void ExibirMensagemPassiva(string mensagem);

        /// <summary>O inimigo vai agir — momento de suspense antes do golpe.</summary>
        void ExibirPreparacaoAtaque(Combate atacante, List<Combate> defensores);

        /// <summary>Anuncia a habilidade ANTES dos resultados (ordem firmada no #7b).</summary>
        void ExibirUsoHabilidade(Combate atacante, Habilidade hab);

        /// <summary>Resumo de fim de batalha da campanha (dano causado/recebido/cura por champ).</summary>
        void ExibirResumoBatalha(List<Combate> jogador);

        /// <summary>Resumo de fim de batalha da Arena (os dois times + quem venceu).</summary>
        void ExibirResumoArena(List<Combate> equipe1, List<Combate> equipe2, bool venceuEquipe1);

        /// <summary>Diálogo "encerrar a batalha?" (Esc no meio da luta). True = encerra.</summary>
        bool ConfirmarEncerramento();
    }
}
