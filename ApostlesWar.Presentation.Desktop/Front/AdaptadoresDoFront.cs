using ApostlesWar.Application.Portas;

namespace ApostlesWar.Presentation.Desktop.Front
{
    /// <summary>
    /// O RITMO da batalha no front, controlado pelo botão de velocidade da tela.
    ///
    /// Mora AQUI, e não no motor, porque velocidade de animação é assunto de PELE: o `CombateService`
    /// não sabe que isto existe. É a porta <see cref="IApresentacao"/> fazendo o trabalho dela.
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
    /// Dorme de verdade, e isso é DE PROPÓSITO por ora. O passo natural depois é
    /// inverter: o C# manda o evento e espera o JS avisar "terminei de animar", em vez de chutar
    /// milissegundos. Aí a animação manda no ritmo, não o relógio. Fica pra quando a tela tiver
    /// animação de verdade — hoje seria complexidade sem cliente.
    ///
    /// É AQUI que o "sair" entre turnos é detectado: quando não há turno humano (Bot×Bot, ou o turno
    /// do bot) ninguém lê a fila de cliques, então a espera consulta o flag da ponte e devolve true —
    /// o CombateService.Aguardar então aborta a batalha (o ponto ÚNICO de cancelamento do motor).
    /// </summary>
    internal class ApresentacaoWebview : IApresentacao
    {
        private readonly RitmoDoFront _ritmo;
        private readonly PonteWebView2 _ponte;

        public ApresentacaoWebview(RitmoDoFront ritmo, PonteWebView2 ponte)
        {
            _ritmo = ritmo;
            _ponte = ponte;
        }

        /// <summary>
        /// Quanto tempo cada batida da narrativa fica na tela. Hoje todas valem o mesmo — é de
        /// propósito: o motor sempre esperou 1500ms em todo evento, e mudar o SENTIMENTO da batalha
        /// não é assunto deste PR. O que mudou é QUEM escolhe. Diferenciar (tick mais rápido que o
        /// anúncio de uma especial, por exemplo) agora é editar esta tabela, sem tocar no motor.
        /// </summary>
        private static int MilissegundosDe(Momento momento) => momento switch
        {
            Momento.Tick => 1500,
            Momento.Narracao => 1500,
            Momento.Golpe => 1500,
            Momento.Preparacao => 1500,
            _ => 1500,
        };

        public bool AguardarAnimacao(Momento momento)
        {
            Thread.Sleep(MilissegundosDe(momento) / _ritmo.Multiplicador);
            return _ponte.SairPedido;   // true = pediram pra sair durante a espera → aborta
        }
    }
}
