using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace ApostlesWar.Presentation.Front
{
    /// <summary>
    /// Um clique vindo da tela, já com significado (não é "tecla X"). <see cref="Valor"/> = índice
    /// (opção de menu, habilidade, id de alvo); <see cref="Texto"/> = carga de texto quando o valor é
    /// uma string (ex.: o nome do perfil na 1ª vez) — a maioria das mensagens não usa.
    /// </summary>
    internal record MensagemDoFront(string Tipo, int Valor, string? Texto = null);

    /// <summary>
    /// A PONTE: janela nativa + webview, e o vai-e-vem de mensagens com o JS. Não é HTTP nem servidor —
    /// JS e C# vivem no MESMO processo e trocam bilhetes direto pela webview (ADR do front).
    ///
    /// O PONTO CRÍTICO É A THREAD. A webview exige a thread de UI (STA) e o `Application.Run` toma ela
    /// pra si. O jogo, por outro lado, é um laço SÍNCRONO que bloqueia esperando o input do jogador.
    /// Se rodassem juntos, um congelaria o outro. A solução: a UI fica na thread principal e o jogo roda
    /// numa thread de fundo; a espera do jogo vira `_mensagens.Take()`, que dorme até um clique chegar.
    /// É por isso que NADA do motor precisa ser async: pra ele, esperar um clique é esperar, e ponto.
    /// </summary>
    internal class PonteWebView2
    {
        private readonly Form _janela;
        private readonly WebView2 _webview;
        private readonly RitmoDoFront _ritmo;
        private readonly BlockingCollection<MensagemDoFront> _mensagens = new();
        private readonly TaskCompletionSource _telaPronta = new();

        // "Sair" pedido pela tela (já confirmado no modal do JS). Fica num flag além de entrar na fila
        // porque quando não há turno humano (Bot×Bot, ou o turno do bot) ninguém lê a fila — quem
        // observa o flag é a espera entre eventos (ApresentacaoWebview.AguardarAnimacao). volatile:
        // escrito pela thread da UI (clique), lido pela thread do jogo.
        private volatile bool _sairPedido;
        public bool SairPedido => _sairPedido;

        // Modo AUTOMÁTICO: o cérebro joga no lugar do humano. É um INTERRUPTOR, não um pedido — por
        // isso, ao contrário do _sairPedido, NÃO é zerado pelo LimparPendentes: ele tem que
        // atravessar os turnos até o jogador clicar de novo. volatile pelo mesmo motivo (a thread da
        // UI escreve no clique, a thread do jogo lê ao decidir).
        private volatile bool _autoLigado;
        public bool AutoLigado => _autoLigado;

        /// <summary>Devolve o controle ao jogador. Chamado no começo de cada batalha: entrar numa luta
        /// nova com o automático herdado da anterior seria tirar o controle de quem não pediu.</summary>
        public void DesligarAuto() => _autoLigado = false;

        // FOCO: o inimigo que o jogador APONTOU no modo automático — o cérebro passa a mirar nele.
        // Mesmo molde do _autoLigado (interruptor, não pedido; volatile porque a thread da UI escreve
        // no clique e a do jogo lê ao decidir), e pelo mesmo motivo NÃO entra no LimparPendentes: é
        // uma ordem que vale até ser desfeita, não um clique a ser consumido.
        //
        // Guarda o ID e não o Combate: quem sabe traduzir id↔combatente é a SessaoDoFront, e a ponte
        // não conhece o domínio. 0 = ninguém.
        private volatile int _focoId;
        public int FocoDoJogador => _focoId;

        /// <summary>Batalha nova começa sem alvo apontado — o inimigo de antes nem existe mais.</summary>
        public void LimparFoco() => _focoId = 0;

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

        /// <summary>Bloqueia até a tela mandar algo. É a espera por input do jogo inteiro.</summary>
        public MensagemDoFront Esperar() => _mensagens.Take();

        /// <summary>Descarta cliques acumulados — evita "clique fantasma" de uma fase anterior. Também
        /// zera o pedido de sair (nova espera/turno começa limpo).</summary>
        public void LimparPendentes()
        {
            while (_mensagens.TryTake(out _)) { }
            _sairPedido = false;
        }

        public void EnviarEstado(EstadoDeBatalha estado) => Enviar("estado", estado);
        public void EnviarEvento(EventoVisto evento) => Enviar("evento", evento);
        public void EnviarMenu(MenuVisto menu) => Enviar("menu", menu);

        /// <summary>Pede ao front a tela de criar perfil (1ª vez). A resposta volta como "criarPerfil".</summary>
        public void PedirPerfil() => Enviar("criarPerfil", new { });

        /// <summary>Abre a tela de editar perfil com os dados atuais. A resposta volta como "salvarPerfil".</summary>
        public void EnviarEdicaoPerfil(EdicaoPerfilVista edicao) => Enviar("edicaoPerfil", edicao);

        /// <summary>Abre a montagem da Arena com o pool de apóstolos. A resposta volta como "iniciarArena".</summary>
        public void EnviarMontagemArena(List<ApostoloVisto> apostolos) => Enviar("montagemArena", new { apostolos });

        // ---------- Campanha ----------
        public void EnviarMapa(MapaVista mapa) => Enviar("campanhaMapa", mapa);
        public void EnviarFases(FasesVista fases) => Enviar("campanhaFases", fases);
        /// <summary>Vitória e derrota, na mesma tela. A resposta volta como "fimDeFase" (ou "voltar", no Esc).</summary>
        public void EnviarFimDeFase(FimDeFaseVista fim) => Enviar("fimDeFase", fim);

        /// <summary>A tela de um apóstolo recém-conquistado. A resposta volta como "continuar".</summary>
        public void EnviarConquista(ApostoloDetalheVista apostolo) => Enviar("conquista", apostolo);

        public void EnviarArsenal(ArsenalVista arsenal) => Enviar("arsenal", arsenal);

        // ---------- Compêndio ----------
        // Duas telas, duas mensagens: a grade e a ficha. Poderiam ser uma só (mandar o catálogo com
        // as fichas dentro e deixar o JS navegar), mas aí o C# perderia de vista em que tela o
        // jogador está — e é ele quem responde o Esc/Sair. Mesma divisão do mapa × fases da campanha.
        public void EnviarCompendio(CompendioVista compendio) => Enviar("compendio", compendio);
        public void EnviarApostoloDetalhe(ApostoloDetalheVista apostolo) => Enviar("compendioApostolo", apostolo);

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
                // espera por input do jogo, e um clique de velocidade ali seria lido como escolha
                // de habilidade/alvo.
                if (msg.Tipo == "pronto") { _telaPronta.TrySetResult(); return; }
                if (msg.Tipo == "velocidade") { _ritmo.Definir(msg.Valor); return; }

                // "foco" é só flag, e NÃO entra na fila: ao contrário do "auto", ele não muda quem
                // decide — muda em quem o cérebro bate. Não há turno humano parado esperando isso,
                // então não há ninguém pra acordar; e enfileirá-lo faria o clique ser lido como
                // escolha de alvo no turno seguinte.
                if (msg.Tipo == "foco") { _focoId = msg.Valor; return; }

                // "auto" é flag E entra na fila. A flag porque é estado (vale nos próximos turnos); a
                // fila porque, se o humano JÁ está parado esperando um clique, é ela que o acorda —
                // sem isso, ligar o automático no meio da escolha travaria o jogo pra sempre.
                if (msg.Tipo == "auto") _autoLigado = msg.Valor != 0;

                // "sair" também vira flag: o turno humano lê da fila (EscolherAcao), mas o Bot×Bot /
                // turno do bot não lê nada — a espera entre eventos observa o flag.
                if (msg.Tipo == "sair") _sairPedido = true;

                _mensagens.Add(msg);
            };
        }

        /// <summary>Destrava o jogo se a janela fechar no meio de uma espera (senão a thread vaza).</summary>
        public void Encerrar()
        {
            _telaPronta.TrySetResult();
            _mensagens.Add(new MensagemDoFront("encerrar", 0));
        }

        /// <summary>
        /// Alterna entre tela cheia e janela. Vem da thread do JOGO (o clique nas configurações é
        /// atendido lá), e mexer em janela é coisa da thread de UI — daí o Invoke, o mesmo caminho
        /// do <see cref="FecharJanela"/>. Quem sabe COMO cada modo fica é o
        /// <see cref="AppFront.AplicarModoDeTela"/>: uma resposta só pro boot e pra troca.
        /// </summary>
        public void DefinirTelaCheia(bool telaCheia)
        {
            if (_janela.IsDisposed) return;
            try { _janela.Invoke(() => AppFront.AplicarModoDeTela(_janela, telaCheia)); }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        /// <summary>
        /// O JOGO pediu pra sair (opção Sair / Esc no menu): fecha a janela na thread de UI. O
        /// FormClosing dispara o <see cref="Encerrar"/> em seguida — este só faz a janela ir embora.
        /// </summary>
        public void FecharJanela()
        {
            if (_janela.IsDisposed) return;
            try { _janela.Invoke(() => _janela.Close()); }
            catch (ObjectDisposedException) { }   // já fechou
            catch (InvalidOperationException) { }  // handle já foi embora
        }
    }
}
