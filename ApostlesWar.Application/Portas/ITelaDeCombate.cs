using ApostlesWar.Domain;
namespace ApostlesWar.Application.Portas
{
    /// <summary>
    /// Porta de APRESENTAÇÃO DA PARTIDA — o que o fluxo de combate (CombateService) precisa mostrar.
    /// Irmã do <see cref="IEntrada"/> (entrada) e do <see cref="IApresentacao"/> (espera): junto com
    /// elas, fecha o isolamento do motor em relação à tela. O console é uma impl
    /// (<see cref="CombateView"/>); a webview é outra.
    ///
    /// O CONTRATO É UM GATILHO, NÃO UM DESENHO. Os nomes são imperativos por herança do console
    /// ("Exibir..."), mas uma impl não é obrigada a desenhar imperativamente: a do front traduz cada
    /// chamada em (a) um retrato do estado serializado e (b) um evento pra animar. É isso que deixa a
    /// tela declarativa — e que faz trocar emoji por sprite ser mexida só de front, sem tocar no motor.
    ///
    /// NÃO entram aqui o menu de ação e o menu de alvo (`ExibirAcoes`/`EscolherAlvoNaTela`): esses são
    /// navegação com CURSOR, formato do console. Quem decide ação/alvo é o
    /// <see cref="Controllers.IControladorDeTurno"/>, e cada plataforma tem o seu (no front, clique na
    /// habilidade + clique no alvo). Botar os dois aqui obrigaria a impl web a carregar método morto.
    /// </summary>
    public interface ITelaDeCombate
    {
        /// <summary>Prepara a tela pro próximo quadro (no console, limpa; no front, pode ser no-op).</summary>
        void LimparTela();

        /// <summary>O retrato dos dois times — é o estado completo da partida.</summary>
        void ExibirPartida(List<Combate> jogadores, List<Combate> inimigos);

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
