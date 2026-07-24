using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace ApostlesWar.Presentation.Desktop.Front
{
    /// <summary>Um clique vindo da tela, já com significado (não é "tecla X").</summary>
    internal record MensagemDoFront(string Tipo, int Valor);

    /// <summary>
    /// A PONTE: janela nativa + webview, e o vai-e-vem de mensagens com o JS. Não é HTTP nem servidor —
    /// JS e C# vivem no MESMO processo e trocam bilhetes direto pela webview (ADR do front).
    ///
    /// O PONTO CRÍTICO É A THREAD. A webview exige a thread de UI (STA) e o `Application.Run` toma ela
    /// pra si. O jogo, por outro lado, é um laço SÍNCRONO que bloqueia esperando input (IEntrada.Ler).
    /// Se rodassem juntos, um congelaria o outro. A solução: a UI fica na thread principal e o jogo roda
    /// numa thread de fundo; a espera do jogo vira `_mensagens.Take()`, que dorme até um clique chegar.
    /// É por isso que NADA do motor precisou virar async — o laço continua exatamente como no console.
    /// </summary>
    internal class PonteWebView2
    {
        private readonly Form _janela;
        private readonly WebView2 _webview;
        private readonly RitmoDoFront _ritmo;
        private readonly BlockingCollection<MensagemDoFront> _mensagens = new();
        private readonly TaskCompletionSource _telaPronta = new();

        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // O jogo é cheio de emoji e acento; sem isso o JSON sai com \uXXXX e polui o debug.
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        public PonteWebView2(Form janela, WebView2 webview, RitmoDoFront ritmo)
        {
            _janela = janela;
            _webview = webview;
            _ritmo = ritmo;
        }

        /// <summary>Espera o JS avisar que carregou. Sem isso, o 1º estado é enviado no vazio.</summary>
        public void EsperarTelaPronta() => _telaPronta.Task.Wait();

        /// <summary>Bloqueia até a tela mandar algo. É o coração do `IEntrada.Ler` no front.</summary>
        public MensagemDoFront Esperar() => _mensagens.Take();

        /// <summary>Descarta cliques acumulados — evita "clique fantasma" de uma fase anterior.</summary>
        public void LimparPendentes()
        {
            while (_mensagens.TryTake(out _)) { }
        }

        public void EnviarEstado(EstadoDeBatalha estado) => Enviar("estado", estado);
        public void EnviarEvento(EventoVisto evento) => Enviar("evento", evento);

        private void Enviar(string tipo, object conteudo)
        {
            string json = JsonSerializer.Serialize(new { tipo, conteudo }, Json);

            // Sempre marshalado pra thread de UI: quem chama isto é a thread do JOGO.
            if (_janela.IsDisposed) return;
            try
            {
                _janela.Invoke(() =>
                {
                    if (_webview.CoreWebView2 != null)
                        _webview.CoreWebView2.PostWebMessageAsString(json);
                });
            }
            catch (ObjectDisposedException) { }   // janela fechada no meio do turno: nada a fazer
            catch (InvalidOperationException) { } // idem (handle já foi embora)
        }

        /// <summary>Liga o recebimento. Chamado depois que o CoreWebView2 inicializa.</summary>
        public void Conectar(CoreWebView2 core)
        {
            core.WebMessageReceived += (_, e) =>
            {
                string bruto = e.TryGetWebMessageAsString();
                var msg = JsonSerializer.Deserialize<MensagemDoFront>(bruto, Json);
                if (msg is null) return;

                // Mensagens de CONTROLE são atendidas aqui e não entram na fila: a fila é o
                // `IEntrada.Ler` do jogo, e um clique de velocidade ali seria lido como escolha
                // de habilidade/alvo.
                if (msg.Tipo == "pronto") { _telaPronta.TrySetResult(); return; }
                if (msg.Tipo == "velocidade") { _ritmo.Definir(msg.Valor); return; }

                _mensagens.Add(msg);
            };
        }

        /// <summary>Destrava o jogo se a janela fechar no meio de uma espera (senão a thread vaza).</summary>
        public void Encerrar()
        {
            _telaPronta.TrySetResult();
            _mensagens.Add(new MensagemDoFront("encerrar", 0));
        }
    }
}
