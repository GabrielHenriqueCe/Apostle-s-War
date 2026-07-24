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
    /// Porta de ESPERA no front: a pausa dramática entre eventos, que dá tempo do JS animar o dano
    /// pulando e o alvo tremendo.
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
        public bool AguardarAnimacao(int ms)
        {
            Thread.Sleep(ms);
            return false;
        }
    }
}
