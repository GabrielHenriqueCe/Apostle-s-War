using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;
using ApostlesWar.Presentation.ConsoleUI.Views;

namespace ApostlesWar.Presentation.Desktop.Front
{
    /// <summary>
    /// Porta de ENTRADA no front. Traduz clique em <see cref="Comando"/> semântico — o mesmo contrato
    /// que o console cumpre lendo tecla. Nesta fatia é pouco exercitada (a tela de combate decide tudo
    /// pelo <see cref="ControladorJogadorWeb"/>); ela existe pra fechar o composition root e é o ponto
    /// onde os MENUS vão plugar quando forem portados.
    /// </summary>
    internal class EntradaWebview : IEntrada
    {
        private readonly PonteWebView2 _ponte;

        public EntradaWebview(PonteWebView2 ponte) => _ponte = ponte;

        public Comando Ler()
        {
            MensagemDoFront msg = _ponte.Esperar();
            return msg.Tipo switch
            {
                "cancelar" => new Comando(TipoComando.Cancelar),
                "confirmar" => new Comando(TipoComando.Confirmar),
                "encerrar" => new Comando(TipoComando.Cancelar),
                _ => new Comando(TipoComando.Selecionar, msg.Valor),
            };
        }
    }

    /// <summary>
    /// O RITMO da batalha no front, controlado pelo botão de velocidade da tela.
    ///
    /// Mora AQUI, e não no motor, porque velocidade de animação é assunto de PELE: o console segue
    /// com os 1500ms de sempre e o `CombateService` não sabe que isto existe. É a porta
    /// <see cref="IApresentacao"/> fazendo o trabalho dela.
    ///
    /// `volatile` porque quem escreve é a thread da UI (o clique) e quem lê é a thread do jogo.
    /// </summary>
    internal class RitmoDoFront
    {
        private volatile int _multiplicador = 2;

        /// <summary>Divisor da espera: 2 = metade do tempo. Começa em 2x — com o log persistente,
        /// a pausa não serve mais pra ler a mensagem, só pra ver a animação.</summary>
        public int Multiplicador => _multiplicador;

        public void Definir(int valor) => _multiplicador = Math.Clamp(valor, 1, 8);
    }

    /// <summary>
    /// Porta de ESPERA no front: a pausa dramática entre eventos, que dá tempo do JS animar o dano
    /// pulando e o alvo tremendo — encurtada pelo <see cref="RitmoDoFront"/>.
    ///
    /// Dorme de verdade, como no console — e isso é DE PROPÓSITO nesta fatia. O passo natural depois é
    /// inverter: o C# manda o evento e espera o JS avisar "terminei de animar", em vez de chutar
    /// milissegundos. Aí a animação manda no ritmo, não o relógio. Fica pra quando a tela tiver
    /// animação de verdade — hoje seria complexidade sem cliente.
    ///
    /// Sempre false: no front não há Esc encerrando batalha (ver TelaDeCombateWeb.ConfirmarEncerramento).
    /// </summary>
    internal class ApresentacaoWebview : IApresentacao
    {
        private readonly RitmoDoFront _ritmo;

        public ApresentacaoWebview(RitmoDoFront ritmo) => _ritmo = ritmo;

        public bool AguardarAnimacao(int ms)
        {
            Thread.Sleep(ms / _ritmo.Multiplicador);
            return false;
        }
    }
}
