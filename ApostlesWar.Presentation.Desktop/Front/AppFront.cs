using ApostlesWar.Infrastructure;
using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;
using ApostlesWar.Application.Controllers;
using ApostlesWar.Application.Services;
using ApostlesWar.Presentation.ConsoleUI.Views;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace ApostlesWar.Presentation.Desktop.Front
{
    /// <summary>
    /// Sobe o jogo com a pele webview. É o composition root do front — o irmão do Program.cs, trocando
    /// os adapters de console pelos de tela: <see cref="TelaDeCombateWeb"/> no lugar da CombateView e
    /// <see cref="ControladorJogadorWeb"/> no lugar do ControladorJogador. O motor é o mesmo, sem um `if`
    /// sequer sabendo que existe front.
    ///
    /// Esta fatia cai DIRETO numa batalha da Arena com times sorteados: o pick de campeões é uma tela
    /// que ainda não existe no front, e a batalha não deveria esperar por ela pra poder ser testada.
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

        private static int RodarJanela()
        {
            ApplicationConfiguration.Initialize();

            var janela = new Form
            {
                Text = "Apostle's War",
                Width = 1280,
                Height = 820,
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = System.Drawing.Color.FromArgb(20, 18, 26),
            };
            var webview = new WebView2 { Dock = DockStyle.Fill };
            janela.Controls.Add(webview);

            var ponte = new PonteWebView2(janela, webview);
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

                ponte.Conectar(webview.CoreWebView2);
                webview.CoreWebView2.Navigate(CaminhoDaTela());

                jogo = new Thread(() => RodarJogo(ponte)) { IsBackground = true };
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

        private static string CaminhoDaTela()
            => Path.Combine(AppContext.BaseDirectory, "wwwroot", "index.html");

        /// <summary>
        /// A thread do JOGO: monta os services (composition root do front) e entra na batalha. Roda o
        /// laço síncrono de sempre — as esperas por input viram esperas por clique.
        /// </summary>
        private static void RodarJogo(PonteWebView2 ponte)
        {
            try
            {
                ponte.EsperarTelaPronta();

                var relogio = new RelogioDoCombate();
                var sessao = new SessaoDoFront(ponte, relogio);
                var tela = new TelaDeCombateWeb(sessao, ponte);
                var entrada = new EntradaWebview(ponte);
                var apresentacao = new ApresentacaoWebview();

                var repositorio = new SaveLocal();
                var capitulos = new CapitulosService(repositorio);
                var arsenal = new ArsenalService(capitulos, repositorio);
                var personagens = new PersonagemService();
                var selecaoDeAlvo = new SelecaoDeAlvoService();
                var menuView = new MenuView(arsenal, capitulos, entrada, apresentacao);
                var campeoes = new CampeoesService(personagens, menuView, capitulos);

                var combate = new CombateService(
                    arsenal, campeoes, personagens, tela, selecaoDeAlvo,
                    controladorJogador: new ControladorJogadorWeb(sessao, ponte),
                    controladorBot: new ControladorBot(selecaoDeAlvo),
                    apresentacao, relogio);

                // Times sorteados do pool completo (9 facções × 4 slots). Você joga a equipe 1.
                var pool = campeoes.TodosOsCampeoes().OrderBy(_ => Random.Shared.Next()).ToList();
                var time1 = pool.Take(4).ToList();
                var time2 = pool.Skip(4).Take(4).ToList();

                combate.ExecutarArenaComTimes(time1, time2, bot1: false, bot2: true);
            }
            catch (Exception ex)
            {
                // Sem isto, exceção na thread de fundo derruba o processo em silêncio.
                Console.WriteLine($"[front] erro na thread do jogo: {ex}");
            }
        }
    }
}
