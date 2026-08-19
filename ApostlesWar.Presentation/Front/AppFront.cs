using ApostlesWar.Infrastructure;
using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;
using ApostlesWar.Application.Controllers;
using ApostlesWar.Application.Services;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace ApostlesWar.Presentation.Front
{
    /// <summary>
    /// Sobe o jogo. É o composition root — monta os services e pluga as impls das portas
    /// (<see cref="TelaDeCombateWeb"/>, <see cref="ControladorJogadorWeb"/>,
    /// <see cref="ApresentacaoWebview"/>). Trocar de pele é trocar as impls montadas aqui: o motor
    /// não tem um `if` sequer sabendo que existe front.
    /// </summary>
    internal static class AppFront
    {
        public static int Rodar()
        {
            int codigo = 0;

            // A webview exige thread STA e o Application.Run toma conta dela. O JOGO, que é um laço
            // bloqueante, vai pra uma thread de fundo (ver PonteWebView2).
            var ui = new Thread(() => codigo = RodarJanela());
            ui.SetApartmentState(ApartmentState.STA);
            ui.Start();
            ui.Join();
            return codigo;
        }

        /// <summary>Largura da janela quando ela NÃO está em tela cheia. A altura não é fixa: ver
        /// <see cref="AplicarModoDeTela"/>.</summary>
        private const int LarguraDaJanela = 1280;

        /// <summary>
        /// Como a janela fica em cada modo — uma resposta só, usada no boot E no botão das
        /// configurações (via <see cref="PonteWebView2.DefinirTelaCheia"/>). Duas respostas seria a
        /// janela abrir de um jeito e voltar de outro ao alternar.
        ///
        /// TELA CHEIA = sem borda, cobrindo o monitor inteiro (barra de tarefas incluída) — é como
        /// jogo abre, e é o padrão.
        /// JANELA = a mesma largura de sempre, mas ESTICADA de cima a baixo até as bordas da área
        /// útil (pedido do Gabriel). O jogo é largo por natureza — dois times e o log no meio —, e o
        /// que faltava era altura; espremer a largura junto não ajudaria em nada.
        /// </summary>
        internal static void AplicarModoDeTela(Form janela, bool telaCheia)
        {
            // No BOOT a janela ainda não tem handle (o Screen.FromControl não teria o que consultar),
            // então cai no monitor onde está o mouse — que é onde o jogador está olhando. Depois de
            // aberta, segue o monitor em que ela mora, pra alternar não a teleportar de tela.
            Screen monitor = janela.IsHandleCreated
                ? Screen.FromControl(janela)
                : Screen.FromPoint(Cursor.Position);

            janela.WindowState = FormWindowState.Normal;   // Maximized brigaria com o Bounds explícito

            if (telaCheia)
            {
                janela.FormBorderStyle = FormBorderStyle.None;
                janela.Bounds = monitor.Bounds;             // Bounds, não WorkingArea: cobre a barra de tarefas
                return;
            }

            janela.FormBorderStyle = FormBorderStyle.Sizable;

            // Área ÚTIL (sem a barra de tarefas): em janela, a barra continua sendo do sistema.
            Rectangle util = monitor.WorkingArea;
            int largura = Math.Min(LarguraDaJanela, util.Width);
            janela.Bounds = new Rectangle(util.X + (util.Width - largura) / 2, util.Y, largura, util.Height);
        }

        private static int RodarJanela()
        {
            ApplicationConfiguration.Initialize();

            // A preferência de tela é lida ANTES da janela existir, senão ela abriria num modo e
            // pularia pro outro à vista do jogador. O mesmo repositório é passado adiante pro
            // composition root da thread do jogo — uma instância, uma verdade.
            var repositorio = new SaveLocal();
            var configuracao = new ConfiguracaoService(repositorio);

            var janela = new Form
            {
                Text = "Apostle's War",
                StartPosition = FormStartPosition.Manual,
                BackColor = System.Drawing.Color.FromArgb(20, 18, 26),
            };
            AplicarModoDeTela(janela, configuracao.Carregar().TelaCheia);

            var webview = new WebView2 { Dock = DockStyle.Fill };
            janela.Controls.Add(webview);

            // O ritmo nasce aqui pra ser COMPARTILHADO entre as duas threads: a ponte escreve nele
            // (clique no >>, thread da UI) e a ApresentacaoWebview lê (thread do jogo).
            var ritmo = new RitmoDoFront();
            var ponte = new PonteWebView2(janela, webview, ritmo);
            Thread? jogo = null;

            webview.CoreWebView2InitializationCompleted += (_, e) =>
            {
                if (!e.IsSuccess)
                {
                    // Falha aqui = tela preta. Melhor gritar do que deixar o jogador olhando o vazio.
                    MessageBox.Show($"Falha ao iniciar o WebView2:\n{e.InitializationException}",
                        "Apostle's War", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    janela.Close();
                    return;
                }

                // F5/Ctrl+R MATARIAM a partida: recarregar zera o JS, mas a thread do jogo continua
                // parada no `Take()` esperando um clique que nunca vem — a tela voltaria vazia e o
                // C# ficaria falando sozinho. Não há reconstrução barata do estado de uma batalha em
                // curso, então o certo é não deixar recarregar. Isto também leva o F12 junto; as
                // ferramentas seguem alcançáveis pelo menu do botão direito (AreDevToolsEnabled).
                webview.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;

                ponte.Conectar(webview.CoreWebView2);

                // O front é servido por um HOST VIRTUAL em vez de aberto por caminho de disco. Motivo:
                // `file://` tem origem OPACA, e ali `<script type="module">` não carrega — sem isto o
                // front inteiro teria que viver num arquivo só. Nada sai da máquina; o mapeamento
                // aponta pra própria pasta wwwroot.
                webview.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    HostVirtual, Path.Combine(AppContext.BaseDirectory, "wwwroot"),
                    CoreWebView2HostResourceAccessKind.Allow);
                // Navegação falha = tela preta com o jogo rodando atrás, falando sozinho. Grita, pelo
                // mesmo motivo do handler de inicialização acima.
                webview.CoreWebView2.NavigationCompleted += (_, nav) =>
                {
                    if (nav.IsSuccess) return;
                    MessageBox.Show($"Falha ao carregar a tela (host virtual):\n{nav.WebErrorStatus}",
                        "Apostle's War", MessageBoxButtons.OK, MessageBoxIcon.Error);
                };

                webview.CoreWebView2.Navigate($"https://{HostVirtual}/index.html");

                jogo = new Thread(() => RodarJogo(ponte, ritmo, repositorio, configuracao)) { IsBackground = true };
                jogo.Start();
            };

            janela.FormClosing += (_, _) => ponte.Encerrar();   // destrava o jogo se ele estiver esperando

            janela.Load += async (_, _) =>
            {
                // Pasta própria pros dados do WebView2 (cache/perfil). Fora do bin pra sobreviver a
                // rebuild e não sujar a saída do publish.
                string dados = Path.Combine(Path.GetTempPath(), "ApostlesWarWebView");
                Directory.CreateDirectory(dados);
                var ambiente = await CoreWebView2Environment.CreateAsync(null, dados);
                await webview.EnsureCoreWebView2Async(ambiente);
            };

            // Qualificado: "Application" cru resolveria pro namespace ApostlesWar.Application (vizinho), nao pro WinForms.
            System.Windows.Forms.Application.Run(janela);
            return 0;
        }

        /// <summary>Nome só interno ao WebView2 — não é DNS e não resolve fora do processo.</summary>
        private const string HostVirtual = "apostlesware";

        /// <summary>
        /// A thread do JOGO: monta os services (composition root do front) e entra na batalha. Roda o
        /// laço síncrono de sempre — as esperas por input viram esperas por clique.
        /// </summary>
        private static void RodarJogo(PonteWebView2 ponte, RitmoDoFront ritmo,
            IRepositorioDeSave repositorio, ConfiguracaoService configuracao)
        {
            try
            {
                ponte.EsperarTelaPronta();

                var relogio = new RelogioDoCombate();
                var sessao = new SessaoDoFront(ponte, relogio);
                var tela = new TelaDeCombateWeb(sessao, ponte);
                var apresentacao = new ApresentacaoWebview(ritmo, ponte);

                // O repositório e as configurações vêm de fora: a janela já precisou deles pra abrir
                // no modo certo (ver RodarJanela), e duas instâncias seriam duas verdades.
                var capitulos = new CapitulosService(repositorio);
                var arsenal = new ArsenalService(capitulos, repositorio);
                var personagens = new PersonagemService();
                var selecaoDeAlvo = new SelecaoDeAlvoService();
                var apostolos = new ApostolosService(personagens, capitulos);
                var progressao = new ProgressaoService(personagens, repositorio);
                // A campanha nasce ANTES do perfil: "excluir conta" delega o wipe do progresso pra ela.
                var campanha = new CampanhaService(arsenal, apostolos, capitulos, personagens, progressao, repositorio);
                var perfil = new PerfilService(repositorio, apostolos, campanha);

                // Dois ControladorBot: um é o adversário, o outro assume quando o jogador liga o
                // automático. Mesmo cérebro, instâncias separadas — cada um memoriza o próprio alvo
                // entre escolher-ação e escolher-alvo, e misturá-los seria um lado mirar pelo outro.
                var combate = new CombateService(
                    arsenal, apostolos, personagens, progressao, tela, selecaoDeAlvo,
                    controladorJogador: new ControladorJogadorWeb(sessao, ponte, new ControladorBot(selecaoDeAlvo)),
                    controladorBot: new ControladorBot(selecaoDeAlvo),
                    apresentacao, relogio);

                // Entra pelo MENU (não mais direto na batalha): o fluxo do front cuida do perfil,
                // mostra o menu principal e roteia a escolha. Ver FluxoDoFront.
                new FluxoDoFront(ponte, combate, apostolos, perfil, sessao,
                    campanha, capitulos, arsenal, personagens, progressao, configuracao).Rodar();
            }
            catch (Exception ex)
            {
                // Sem isto, exceção na thread de fundo derruba o processo em silêncio.
                Console.WriteLine($"[front] erro na thread do jogo: {ex}");
            }
        }
    }
}
