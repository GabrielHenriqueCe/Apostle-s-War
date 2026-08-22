using System.Text.Json;
using ApostlesWar.Application;
using ApostlesWar.Application.Portas;
using ApostlesWar.Application.Services;
using ApostlesWar.Domain;

namespace ApostlesWar.Presentation.Front
{
    /// <summary>
    /// O "de fora da luta": perfil do jogador, menu principal, campanha, arena, catedral,
    /// configurações — e roteia a escolha. É o irmão de menu do que o
    /// <see cref="ControladorJogadorWeb"/> é pro combate. Reaproveita os SERVICES da Application; só
    /// a casca de navegação mora aqui.
    ///
    /// Regra da casa: o que é REGRA (o que desbloqueia, o que dropa, quando salvar) fica nos
    /// services; aqui só entra o que é tela. Ver <see cref="ApostlesWar.Application.Services.CampanhaService"/>.
    /// </summary>
    internal class FluxoDoFront
    {
        // Índices do MENU PRINCIPAL — casam com a ordem enviada em MostrarMenuPrincipal.
        private const int Campanha = 0;
        private const int Arena = 1;
        private const int Catedral = 2;
        private const int Forja = 3;
        private const int Compendio = 4;
        private const int Configuracao = 5;

        // Índices do menu de CONFIGURAÇÃO.
        private const int CfgTelaCheia = 1;
        private const int CfgConta = 2;

        // Índices do menu de CONTA.
        private const int ContaExcluir = 0;

        // Retornos de LerEscolha que NÃO são um índice de opção.
        private const int EscVoltar = -1;      // Esc pediu "voltar" num submenu
        private const int EditarPerfil = -2;   // clique no avatar → editar perfil

        // "Janela fechada" desenrola a navegação inteira (mesma ideia do BatalhaAbortada no motor).
        private sealed class JogoEncerrado : Exception { }

        private readonly PonteWebView2 _ponte;
        private readonly CombateService _combate;
        private readonly ApostolosService _apostolos;
        private readonly PerfilService _perfil;
        private readonly SessaoDoFront _sessao;
        private readonly CampanhaService _campanha;
        private readonly CapitulosService _capitulos;
        private readonly ArsenalService _arsenal;
        private readonly PersonagemService _personagens;
        private readonly ProgressaoService _progressao;
        private readonly AlmaService _alma;
        private readonly PoService _po;
        private readonly ConfiguracaoService _configuracao;

        public FluxoDoFront(PonteWebView2 ponte, CombateService combate, ApostolosService apostolos,
            PerfilService perfil, SessaoDoFront sessao, CampanhaService campanha, CapitulosService capitulos,
            ArsenalService arsenal, PersonagemService personagens, ProgressaoService progressao,
            AlmaService alma, PoService po, ConfiguracaoService configuracao)
        {
            _ponte = ponte;
            _combate = combate;
            _apostolos = apostolos;
            _perfil = perfil;
            _sessao = sessao;
            _campanha = campanha;
            _capitulos = capitulos;
            _arsenal = arsenal;
            _personagens = personagens;
            _progressao = progressao;
            _alma = alma;
            _po = po;
            _configuracao = configuracao;
        }

        public void Rodar()
        {
            try
            {
                // Carrega o progresso UMA vez no boot: assim os apóstolos desbloqueados já valem em todo
                // lugar (ex: o picker de avatar do perfil libera conforme a campanha libera os apóstolos),
                // não só depois de entrar na Campanha. Depois o estado vive em memória (a vitória
                // atualiza + salva).
                _campanha.CarregarSaves();
                GarantirPerfil();

                while (true)
                {
                    switch (MostrarMenuPrincipal())
                    {
                        case Campanha:
                            MostrarCampanha();
                            break;

                        case Arena:
                            MontarArena();
                            break;

                        case Catedral:
                            MostrarCatedral();
                            break;

                        case Forja:
                            AbrirForjaDoMenu();
                            break;

                        case Compendio:
                            MostrarCompendio();
                            break;

                        case Configuracao:
                            if (MostrarConfiguracao()) GarantirPerfil();   // conta excluída → pede nome de novo
                            break;

                        case EditarPerfil:
                            MostrarEditarPerfil();
                            break;
                    }
                }
            }
            catch (JogoEncerrado)
            {
                // Janela fechada ou Esc-sair: a thread do jogo só precisa desenrolar e terminar.
            }
        }

        /// <summary>
        /// A Forja aberta DIRETO do menu. Ela é tela da PEÇA, então precisa de uma — e de um portador
        /// pra o reflexo na ficha fazer sentido.
        ///
        /// Escolhe a primeira peça VESTIDA do primeiro apóstolo do roster; sem nenhuma vestida, a
        /// primeira do acervo, e aí sem portador (a peça está no baú, e não há ficha a mexer). Com o
        /// acervo vazio a opção do menu nem está habilitada, mas a checagem fica: quem responde "dá
        /// pra abrir?" é o back, sempre de novo, e não a tela que desenhou o botão.
        /// </summary>
        private void AbrirForjaDoMenu()
        {
            var acervo = _arsenal.ObterObtidos();
            if (acervo.Count == 0) return;

            Personagem? alvo = _apostolos.ObterDesbloqueados().FirstOrDefault();
            Item? vestida = alvo == null ? null : _arsenal.ObterEquipados(alvo).FirstOrDefault(i => i != null);

            // Pediu a Catedral pela aba: quem a abre é este caminho, porque o laço da Forja aqui
            // não tem uma Catedral em volta pra onde voltar.
            if (MostrarForja(vestida ?? acervo[0], vestida == null ? null : alvo)) MostrarCatedral();
        }

        /// <summary>1ª vez (sem save de perfil): pede o nome e sorteia um avatar. Já tem perfil → volta na hora.</summary>
        private void GarantirPerfil()
        {
            if (_perfil.Existe()) return;

            _ponte.LimparPendentes();
            _ponte.PedirPerfil();   // o front abre a tela de criar nome

            while (true)
            {
                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "criarPerfil")
                {
                    string nome = (msg.Texto ?? "").Trim();
                    if (nome.Length == 0) continue;   // sem nome não cria (o front já barra, mas guardamos)
                    _perfil.CriarPerfil(nome, _perfil.AvatarInicial());
                    return;
                }
            }
        }

        private int MostrarMenuPrincipal()
        {
            Perfil? perfil = _perfil.Carregar();

            _ponte.LimparPendentes();
            _ponte.EnviarMenu(new MenuVisto(
                "Apostle's War", "RPG por turnos",
                new List<OpcaoMenuVista>
                {
                    new("Campanha",     "🗺️", Habilitado: true),
                    new("Arena",        "⚔️", Habilitado: true),
                    // A CATEDRAL abriga as quatro estações — 🎒 Armaria (vestir), ⬆️ Santuário
                    // (nível), ★ Altar (estrela) e 🔥 Oferenda (fundir alma). A ⚒️ Forja NÃO é uma
                    // delas: é tela própria, porque lá o centro é a peça e não o apóstolo. O
                    // `ArsenalService` manteve o nome de propósito: ele é o ACERVO de equipamento,
                    // e "arsenal" continua sendo a palavra certa pra isso — a Armaria é a estação
                    // onde ele se veste.
                    new("Catedral",     "⛪", Habilitado: true),
                    // A FORJA tem porta própria no menu (pedido do Gabriel, ago/2026): ela é tela
                    // irmã da Catedral, não uma estação dela, e chegar nela custava três cliques
                    // (Catedral → slot → Melhorar). Desabilitada com o acervo vazio: sem peça não
                    // há o que forjar, e a porta diria isso tarde demais.
                    new("Forja",        "⚒️", Habilitado: _arsenal.ObterObtidos().Count > 0),
                    new("Compêndio",    "📖", Habilitado: true),
                    new("Configurações", "⚙️", Habilitado: true),
                    // Não há opção "Sair" na lista: quem sai do jogo é o 🚪 do canto superior
                    // direito, o mesmo botão de todas as outras telas. Duas portas pro mesmo lugar,
                    // uma delas só aqui, é o que a padronização desfez.
                },
                Raiz: true,
                Avatar: perfil?.Avatar,
                Nome: perfil?.Nome));

            return LerEscolha();
        }

        /// <returns>true se a conta foi EXCLUÍDA aqui dentro (o chamador deve repedir o perfil).</returns>
        private bool MostrarConfiguracao()
        {
            while (true)
            {
                _ponte.LimparPendentes();
                _ponte.EnviarMenu(new MenuVisto(
                    "Configurações", "Ajustes do jogo",
                    new List<OpcaoMenuVista>
                    {
                        new("Som",        "🔊", Habilitado: false),   // fatia futura
                        new("Tela cheia", "🖥️", Habilitado: true,
                            Marcado: _configuracao.Carregar().TelaCheia),
                        new("Conta",      "👤", Habilitado: true),
                        new("Voltar",     "⬅️", Habilitado: true),
                    }));

                int escolha = LerEscolha();

                if (escolha == CfgTelaCheia)
                {
                    // Grava e aplica na hora: a preferência é o que manda, e a janela obedece. O
                    // `while` redesenha o menu, então o ✓ acompanha sem ninguém avisar a tela.
                    _ponte.DefinirTelaCheia(_configuracao.AlternarTelaCheia());
                }
                else if (escolha == CfgConta)
                {
                    if (MostrarConta()) return true;   // excluiu → sobe o sinal
                    // não excluiu: volta a mostrar as configurações
                }
                else
                {
                    return false;   // Voltar (botão) ou Esc
                }
            }
        }

        /// <returns>true se a conta foi excluída.</returns>
        private bool MostrarConta()
        {
            Perfil? perfil = _perfil.Carregar();

            _ponte.LimparPendentes();
            _ponte.EnviarMenu(new MenuVisto(
                "Conta", perfil is null ? null : $"{perfil.Avatar} {perfil.Nome}",
                new List<OpcaoMenuVista>
                {
                    new("Excluir conta", "🗑️", Habilitado: true,
                        Confirmar: "Excluir conta? Isso apaga seu perfil e TODO o progresso. Não dá pra desfazer."),
                    new("Voltar",        "⬅️", Habilitado: true),
                }));

            int escolha = LerEscolha();   // o front só manda "excluir" depois de confirmar no modal
            if (escolha == ContaExcluir)
            {
                _perfil.Excluir();
                return true;
            }
            return false;   // Voltar ou Esc
        }

        // A config da Arena chega serializada no Texto; case-insensitive porque o JS manda camelCase.
        private static readonly JsonSerializerOptions ConfigJson = new() { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Arena (PVP): manda o pool de apóstolos pro front montar os dois times + escolher o controle
        /// de cada lado, espera a config e roda a luta. Uma batalha por entrada; abortar/sair/Esc volta
        /// pro menu (o loop de Rodar redesenha).
        /// </summary>
        private void MontarArena()
        {
            var pool = _apostolos.TodosOsApostolos();
            // O card da Arena mostra o NÍVEL 1, não o do roster: é nele que os dois lados vão lutar
            // (ver a montagem dos times abaixo), e um card prometendo nv 27 seria a tela mentindo
            // sobre a luta que vem.
            var apostolos = pool.Select(p => new ApostoloVisto(p.Simbolo, Tipos.Simbolo(p.Tipo), p.Nome,
                Desbloqueado: true, Estrelas: 0, Nivel: Arquetipos.NivelMinimo)).ToList();

            _ponte.LimparPendentes();
            _ponte.EnviarMontagemArena(apostolos);

            while (true)
            {
                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "voltar") return;   // Esc na montagem → volta pro menu

                if (msg.Tipo == "iniciarArena")
                {
                    ArenaConfig? cfg = LerConfigArena(msg.Texto);
                    if (cfg is null || !ConfigValida(cfg, pool.Count)) continue;   // config inválida: ignora

                    // A Arena luta no NÍVEL 1 dos dois lados, em cópias: ela é o laboratório de
                    // balanço (já é sem item, sem cenário e sem recompensa), e um lado nivelado pela
                    // campanha responderia "quem eu treinei mais?" no lugar de "qual kit é melhor?".
                    var time1 = cfg.Time1.Select(i => pool[i].ComNivel(Arquetipos.NivelMinimo)).ToList();
                    var time2 = cfg.Time2.Select(i => pool[i].ComNivel(Arquetipos.NivelMinimo)).ToList();

                    _sessao.Reiniciar();       // batalha nova = tela limpa (senão os antigos acumulam)
                    _sessao.Modo = ModoDeBatalha.Arena;   // aqui sair é sair mesmo, sem desfecho
                    _sessao.Tema = "";                    // laboratório não tem cenário
                    _ponte.DesligarAuto();     // e controle de volta com o jogador
                    _ponte.LimparPendentes();  // dropa cliques da montagem + zera o "sair"
                    if (_combate.ExecutarArenaComTimes(time1, time2, cfg.Bot1, cfg.Bot2))
                        EsperarVoltarAoMenu();
                    return;
                }
            }
        }

        private static ArenaConfig? LerConfigArena(string? texto)
        {
            if (string.IsNullOrEmpty(texto)) return null;
            try { return JsonSerializer.Deserialize<ArenaConfig>(texto, ConfigJson); }
            catch (JsonException) { return null; }
        }

        // Cada time: de 1 a 4 apóstolos, índices válidos (o front garante ≥1 de cada lado pra dar 1x1).
        private static bool ConfigValida(ArenaConfig cfg, int total)
            => TimeValido(cfg.Time1, total) && TimeValido(cfg.Time2, total);

        private static bool TimeValido(int[]? time, int total)
            => time is { Length: >= 1 and <= 4 } && time.All(i => i >= 0 && i < total);

        private void EsperarVoltarAoMenu()
        {
            while (true)
            {
                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "voltarMenu") return;
            }
        }

        /// <summary>
        /// Lê a próxima escolha de menu. "encerrar" (janela) e "sairDoJogo" (Esc na raiz) desenrolam a
        /// navegação inteira; "voltar" (Esc num submenu) devolve <see cref="EscVoltar"/>.
        /// </summary>
        private int LerEscolha()
        {
            while (true)
            {
                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "sairDoJogo") { _ponte.FecharJanela(); throw new JogoEncerrado(); }
                if (msg.Tipo == "voltar") return EscVoltar;
                if (msg.Tipo == "editarPerfil") return EditarPerfil;
                if (msg.Tipo == "menuEscolha") return msg.Valor;
            }
        }

        /// <summary>
        /// Editar perfil: troca o nome e escolhe o avatar na grade dos 36 (os bloqueados em cinza).
        /// O front devolve o ÍNDICE na lista completa; quem diz se aquele apóstolo VALE é o
        /// <see cref="PerfilService.PodeUsarAvatar"/> — aqui só se pinta o que ele responde.
        /// </summary>
        private void MostrarEditarPerfil()
        {
            var todos = _apostolos.TodosOsApostolos();

            var lista = todos
                .Select(p => new ApostoloVisto(p.Simbolo, Tipos.Simbolo(p.Tipo), p.Nome, _perfil.PodeUsarAvatar(p),
                    _progressao.EstrelasDe(p), p.Nivel, XpPct(p)))
                .ToList();

            Perfil? perfil = _perfil.Carregar();

            _ponte.LimparPendentes();
            _ponte.EnviarEdicaoPerfil(new EdicaoPerfilVista(perfil?.Nome ?? "", perfil?.Avatar ?? "", lista));

            while (true)
            {
                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "voltar") return;
                if (msg.Tipo == "salvarPerfil")
                {
                    string nome = (msg.Texto ?? "").Trim();
                    int idx = msg.Valor;
                    if (nome.Length == 0 || idx < 0 || idx >= todos.Count) continue;   // inválido: ignora
                    Personagem escolhido = todos[idx];
                    if (!_perfil.PodeUsarAvatar(escolhido)) continue;   // bloqueado: não confiamos na tela
                    _perfil.CriarPerfil(nome, escolhido.Simbolo);   // sobrescreve (mesma chave)
                    return;
                }
            }
        }

        // ---------- Campanha ----------

        /// <summary>
        /// Campanha: mostra o mapa e roteia. O progresso já foi carregado no boot (ver Rodar) e vive em
        /// memória; sem save é o default (Reino fase 1, só Humanos). A posição no mapa (último lugar)
        /// persiste na chave "campanha".
        /// </summary>
        private void MostrarCampanha()
        {
            var faccoes = _capitulos.FaccoesDaCampanha();
            int posicao = _campanha.PosicaoNoMapa();   // 0 (sem save) = o primeiro capítulo

            // A DIFICULDADE é do fluxo, não da tela: ela atravessa daqui até o combate (que precisa
            // dela pra compor a fase, pagar a XP e nivelar o inimigo). Reabre onde o jogador estava.
            Dificuldade dificuldade = _campanha.DificuldadeAtual();

            while (true)
            {
                _ponte.LimparPendentes();
                _ponte.EnviarMapa(MontarMapa(faccoes, posicao, dificuldade));

                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "voltar") return;   // volta pro menu principal

                if (msg.Tipo == "escolherDificuldade")
                {
                    if (EscolhaDeDificuldade(msg.Valor) is not Dificuldade escolhida) continue;
                    dificuldade = escolhida;
                    _campanha.SalvarDificuldade(dificuldade);
                }

                if (msg.Tipo == "selecionarCapitulo")
                {
                    int idx = msg.Valor;
                    if (idx < 0 || idx >= faccoes.Count) continue;
                    Faccao faccao = faccoes[idx];
                    if (!_capitulos.EstaCapituloDesbloqueado(faccao, dificuldade)) continue;   // bloqueado: ignora

                    posicao = idx;
                    _campanha.SalvarPosicao(posicao);   // último lugar
                    dificuldade = MostrarFases(faccao, dificuldade);   // ele pode ter trocado lá dentro
                }
            }
        }

        /// <summary>
        /// Traduz o clique numa dificuldade, ou <c>null</c> se ela não existe ou ainda está travada.
        /// Quem responde "está liberada?" é sempre o back — a tela desenha o cadeado, não decide.
        /// </summary>
        private Dificuldade? EscolhaDeDificuldade(int valor)
        {
            if (!Enum.IsDefined(typeof(Dificuldade), valor)) return null;
            var escolhida = (Dificuldade)valor;
            return _capitulos.DificuldadeDesbloqueada(escolhida) ? escolhida : null;
        }

        /// <summary>
        /// As quatro dificuldades pra tela, com o que falta pra destravar cada uma. Vai igual no mapa e
        /// na tela de fases: é o mesmo controle, e o jogador troca dos dois lugares.
        /// </summary>
        private List<DificuldadeVista> MontarDificuldades()
            => Enum.GetValues<Dificuldade>().Select(d => new DificuldadeVista(
                d.Descricao(), (int)d, _capitulos.DificuldadeDesbloqueada(d),
                _capitulos.DificuldadeDesbloqueada(d) ? null
                    : $"feche a 8-7 no {((Dificuldade)((int)d - 1)).Descricao()}")).ToList();

        private MapaVista MontarMapa(List<Faccao> faccoes, int posicao, Dificuldade dificuldade)
        {
            var capitulos = faccoes.Select(f => new CapituloVista(
                Faccoes.Simbolo(f), f.Descricao(),
                _capitulos.EstaCapituloDesbloqueado(f, dificuldade),
                _capitulos.CapituloConcluido(f, dificuldade))).ToList();
            return new MapaVista(capitulos, posicao, MontarDificuldades(), (int)dificuldade);
        }

        /// <summary>Tela de fases de uma facção: escolhe a fase, monta o time (≤4 dos liberados) e luta.</summary>
        /// <returns>A dificuldade em que o jogador ficou — trocar aqui dentro vale pro mapa também,
        /// senão voltar pro mapa desfaria a escolha que ele acabou de fazer.</returns>
        private Dificuldade MostrarFases(Faccao faccao, Dificuldade dificuldade)
        {
            while (true)
            {
                _ponte.LimparPendentes();
                _ponte.EnviarFases(MontarFases(faccao, dificuldade));

                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "voltar") return dificuldade;   // volta pro mapa

                // Trocar de dificuldade DENTRO da fase: redesenha a mesma facção na outra trilha. A
                // fase selecionada é a última visitada LÁ (o UltimaFaseDe cai na 1 se estiver travada).
                if (msg.Tipo == "escolherDificuldade")
                {
                    if (EscolhaDeDificuldade(msg.Valor) is not Dificuldade escolhida) continue;
                    dificuldade = escolhida;
                    _campanha.SalvarDificuldade(dificuldade);
                }

                if (msg.Tipo == "iniciarFase" && ValidarFase(msg.Texto, faccao, dificuldade, out Fases fase, out var time))
                {
                    // O "Próxima" pode ter atravessado pro capítulo seguinte, então o laço continua
                    // ONDE O JOGADOR PAROU — e não onde ele entrou. Sem isto, quem virasse de
                    // capítulo lutando cairia de volta na lista de fases do capítulo antigo.
                    faccao = JogarFase(faccao, fase, dificuldade, time);
                }
            }
        }

        /// <summary>
        /// Uma fase, e o que vem DEPOIS dela. É um laço porque o fim de fase não é mais um beco:
        /// "Jogar Novamente" e "Próxima Fase" voltam pra cá com o mesmo time, sem passar pela
        /// montagem — que era o pedido (ninguém quer remontar quatro slots pra repetir uma fase).
        /// "Editar Equipe" e o Esc saem, e aí o chamador redesenha a tela de fases.
        ///
        /// Vitória e derrota terminam na MESMA tela, com as mesmas opções. A diferença é o que ela
        /// mostra em cima (troféu e recompensas × vela) e se o "Próxima" existe.
        /// </summary>
        /// <returns>O capítulo em que o jogador parou — pode não ser o que ele entrou, porque
        /// continuar depois da fase 7 atravessa pro capítulo seguinte.</returns>
        private Faccao JogarFase(Faccao faccao, Fases fase, Dificuldade dificuldade, List<Personagem> time)
        {
            while (true)
            {
                // Antes de lutar, não depois: se o jogo fechar no meio da luta, o jogador volta na
                // fase em que estava e com o time que montou.
                _campanha.SalvarEntradaNaFase(faccao, fase, dificuldade, time);

                _sessao.Reiniciar();
                _sessao.Modo = ModoDeBatalha.Campanha;   // aqui desistir é DERROTA, não saída
                // O capítulo dá o cenário da luta. O nome do enum É a chave do CSS: um capítulo sem
                // pele própria simplesmente cai no visual padrão, então acrescentar tema é só CSS.
                _sessao.Tema = faccao.ToString().ToLowerInvariant();
                _ponte.DesligarAuto();
                _ponte.LimparPendentes();

                // A foto do ANTES tem de sair daqui: quando o ExecutarFaseComTime volta, a XP já foi
                // creditada e as instâncias do roster já subiram — não há mais "antes" pra consultar.
                var antes = time.Select(p => (Apostolo: p, Nivel: p.Nivel, Xp: _progressao.XpDe(p))).ToList();
                var almaAntes = _alma.Saldo().ToList();
                var poAntes = _po.Saldo().ToList();

                ResultadoDaFase resultado = _combate.ExecutarFaseComTime(time, faccao, fase, dificuldade);
                bool venceu = resultado.Venceu;

                var ganhos = antes.Select(a => MontarGanho(a.Apostolo, a.Nivel, a.Xp)).ToList();
                var almaGanha = Enum.GetValues<Raridade>()
                    .Select(r => new AlmaVista((int)r, r.Descricao(), _alma.SaldoDe(r) - almaAntes[(int)r], Alma.XpPorAlma(r)))
                    .Where(a => a.Quantidade > 0)
                    .ToList();

                // O pó é lido pela DIFERENÇA de saldo, como a alma, e não pela tabela de queda: quem
                // decide se ele caiu é o ArsenalService (só na vitória), e refazer esse `if` aqui
                // seria a mesma regra escrita em dois lugares.
                var poGanho = Enum.GetValues<Raridade>()
                    .Select(r => new PoVista((int)r, r.Descricao(), _po.SaldoDe(r) - poAntes[(int)r], Po.PontosPorPo(r)))
                    .Where(p => p.Quantidade > 0)
                    .ToList();

                // A recompensa é processada ANTES de montar a tela: é ela que desbloqueia a fase
                // seguinte, e é isso que decide se o botão "Próxima Fase" existe.
                var novos = new List<Personagem>();
                RecompensaVista? recompensa = null;
                if (venceu)
                {
                    RecompensaDaFase r = _campanha.ProcessarVitoria(faccao, fase, dificuldade);
                    novos = r.NovosApostolos;
                    recompensa = MontarRecompensa(r);
                }

                MostrarConquistas(venceu, recompensa, faccao, fase, dificuldade, novos,
                    resultado.XpPorApostolo, ganhos, almaGanha, poGanho);

                _ponte.EnviarFimDeFase(MontarFimDeFase(venceu, recompensa, faccao, fase, dificuldade,
                    resultado.XpPorApostolo, ganhos, almaGanha, poGanho, comOpcoes: true));
                switch (EsperarDecisao())
                {
                    case DecisaoDeFim.JogarNovamente:
                        continue;

                    case DecisaoDeFim.ProximaFase:
                        // Não confiamos na tela: ela só desenha o botão quando dá, mas quem responde
                        // "a próxima existe e está liberada?" é o back, aqui, de novo.
                        var proxima = ProximaEtapa(faccao, fase, dificuldade);
                        if (proxima is null) return faccao;

                        // Virou o capítulo: o marcador do mapa vai junto, senão sair depois cairia
                        // num lugar do mapa que não é onde o jogador está.
                        if (proxima.Value.Faccao != faccao)
                            _campanha.SalvarPosicao(_capitulos.FaccoesDaCampanha().IndexOf(proxima.Value.Faccao));

                        faccao = proxima.Value.Faccao;
                        fase = proxima.Value.Fase;
                        continue;

                    default:
                        return faccao;   // Editar Equipe / Esc
                }
            }
        }

        /// <summary>
        /// A celebração do apóstolo conquistado, quando há algum. Ordem pedida pelo Gabriel: primeiro a
        /// tela de vitória com o item em destaque, e só depois cada apóstolo novo — um por vez, cada um
        /// com a própria tela. Sem apóstolo novo isto não faz nada, e a tela de decisão aparece direto.
        ///
        /// O C# conduz a sequência (manda um, espera o "continuar") em vez de despejar a lista e
        /// deixar o JS navegar, pelo mesmo motivo do compêndio: quem responde o Esc/Sair é ele, então
        /// precisa saber em qual tela o jogador está.
        /// </summary>
        private void MostrarConquistas(bool venceu, RecompensaVista? recompensa, Faccao faccao,
            Fases fase, Dificuldade dificuldade, List<Personagem> novos, int xp,
            List<GanhoVista> ganhos, List<AlmaVista> alma, List<PoVista> po)
        {
            if (novos.Count == 0) return;

            _ponte.EnviarFimDeFase(MontarFimDeFase(venceu, recompensa, faccao, fase, dificuldade,
                xp, ganhos, alma, po, comOpcoes: false));
            EsperarContinuar();

            foreach (Personagem novo in novos)
            {
                _ponte.EnviarConquista(MontarDetalhe(novo));
                EsperarContinuar();
            }
        }

        private FimDeFaseVista MontarFimDeFase(bool venceu, RecompensaVista? recompensa, Faccao faccao,
            Fases fase, Dificuldade dificuldade, int xp, List<GanhoVista> ganhos, List<AlmaVista> alma,
            List<PoVista> po, bool comOpcoes)
        {
            var proxima = ProximaEtapa(faccao, fase, dificuldade);
            return new FimDeFaseVista(venceu, recompensa, xp, ganhos, alma, po,
                PodeProxima: proxima is not null,
                ProximoECapitulo: proxima is not null && proxima.Value.Faccao != faccao,
                comOpcoes);
        }

        /// <summary>
        /// O que este apóstolo levou, já fatiado pra animação. Os trechos vão de
        /// <paramref name="nivelAntes"/> até o nível de agora, um por nível atravessado — encher,
        /// zerar, encher o próximo.
        /// </summary>
        private GanhoVista MontarGanho(Personagem p, int nivelAntes, int xpAntes)
        {
            int xpAgora = _progressao.XpDe(p);

            var trechos = Progressao.Trechos(nivelAntes, xpAntes, p.Nivel, xpAgora)
                .Select(t => new TrechoDeNivel(t.Nivel, t.De, t.Ate))
                .ToList();

            // Travado: a XP passou do teto e a faixa nunca fecha sozinha, então o último trecho vai a
            // 100 e a tela ACENDE a barra cheia. É o aviso de que a estrela passou a fazer falta.
            bool travou = _progressao.NaParede(p);
            if (travou) trechos[^1] = trechos[^1] with { Ate = 100 };

            return new GanhoVista(p.Simbolo, Tipos.Simbolo(p.Tipo), p.Nome, xpAgora - xpAntes,
                trechos, travou, DeltaDeStats(p, nivelAntes, p.Nivel));
        }

        /// <summary>
        /// A ficha do apóstolo no fim da fase — <b>os seis stats, SEMPRE</b>, subindo ou não. Quem
        /// mostra o <c>→</c> só onde mudou é a tela.
        ///
        /// Antes só entrava o que tinha MEXIDO, e o efeito era a tela dançar: a linha de um apóstolo
        /// tinha seis campos quando ele subia de nível e nenhum quando não subia, então o bloco mudava
        /// de tamanho a cada vitória. Lista de tamanho fixo se lê de relance; lista que aparece e some
        /// obriga a reler. É pedido do Gabriel (ago/2026).
        ///
        /// O crítico fica de fora porque vem do TIPO e não anda com o nível.
        /// </summary>
        private static List<DeltaStatVista> DeltaDeStats(Personagem p, int de, int ate)
        {
            Personagem antes = p.ComNivel(de);
            var linhas = new (string Icone, string Rotulo, int De, int Ate)[]
            {
                ("❤️", "HP", antes.HP, p.HP),
                ("⚔️", "Ataque", antes.Ataque, p.Ataque),
                ("🛡️", "Defesa", antes.Defesa, p.Defesa),
                ("⚡", "Velocidade", antes.Velocidade, p.Velocidade),
                ("🎯", "Precisão", antes.Precisao, p.Precisao),
                ("🧿", "Resistência", antes.Resistencia, p.Resistencia),
            };

            return linhas.Select(l => new DeltaStatVista(l.Icone, l.Rotulo, l.De, l.Ate)).ToList();
        }

        /// <summary>
        /// Pra onde o "continuar" leva, ou null se não há pra onde ir. Uma pergunta só, e é dela que
        /// caem TODOS os casos em que o botão some: derrota (a fase seguinte não foi desbloqueada) e
        /// fim do último capítulo.
        ///
        /// Depois da fase 7 ele ATRAVESSA pro capítulo seguinte, na fase 1 — jogar a campanha inteira
        /// não deveria exigir voltar ao mapa a cada sete fases. O que ele nunca faz é dar a volta:
        /// terminada a fase 7 do último capítulo, acabou, e o jogador escolhe o que fazer. Isso vai
        /// importar mais quando a DIFICULDADE existir — passar de Fácil pra Normal é decisão dele,
        /// não consequência de um clique em "continuar".
        /// </summary>
        private (Faccao Faccao, Fases Fase)? ProximaEtapa(Faccao faccao, Fases fase, Dificuldade dificuldade)
        {
            if (fase != Enum.GetValues<Fases>().Last())
                return _capitulos.EstaDesbloqueado(faccao, Proxima(fase), dificuldade)
                    ? (faccao, Proxima(fase))
                    : null;

            var capitulos = _capitulos.FaccoesDaCampanha();
            int proximo = capitulos.IndexOf(faccao) + 1;
            if (proximo <= 0 || proximo >= capitulos.Count) return null;   // era o último

            return _capitulos.EstaDesbloqueado(capitulos[proximo], Fases.Fase1, dificuldade)
                ? (capitulos[proximo], Fases.Fase1)
                : null;
        }

        private static Fases Proxima(Fases fase) => (Fases)((int)fase + 1);

        /// <summary>
        /// O perfil de distância deste apóstolo em TODAS as combinações de casa: linha = a casa onde
        /// ele estaria, coluna = a casa do alvo (as duas de 0 a 3). O front recebe a grade inteira e
        /// não a fórmula, então arrastar entre casas vira trocar de LINHA numa tabela que já está na
        /// mão — sem ida e volta pelo C# a cada quadro, e sem uma segunda cópia da regra pra divergir.
        /// </summary>
        private static List<List<double>> GradeDePosicao(Personagem p) =>
            Casas().Select(minha => Casas()
                    .Select(dele => Arquetipos.MultiplicadorDePosicao(
                        p.Tipo, Arquetipos.DistanciaEntreCasas(minha, dele)))
                    .ToList())
                .ToList();

        private static IEnumerable<int> Casas() =>
            Enumerable.Range(Arquetipos.CasaDaFrente, Arquetipos.CasaDoFundo - Arquetipos.CasaDaFrente + 1);

        private FasesVista MontarFases(Faccao faccao, Dificuldade dificuldade)
        {
            var fases = Enum.GetValues<Fases>().Select(f => MontarFase(faccao, f, dificuldade)).ToList();
            var desbloqueados = _apostolos.ObterDesbloqueados();
            var meus = desbloqueados
                .Select(p => new ApostoloVisto(p.Simbolo, Tipos.Simbolo(p.Tipo), p.Nome, Desbloqueado: true,
                    _progressao.EstrelasDe(p), p.Nivel, XpPct(p), GradeDePosicao(p)))
                .ToList();

            // O time salvo volta como ÍNDICES nesta lista, porque é isso que o clique devolve. A
            // tradução identidade→índice é do C#: o save guarda quem é o apóstolo (ver
            // CampanhaService.UltimoTime), e a posição na lista é só o endereço de hoje.
            var time = _campanha.UltimoTime()
                .Select(p => desbloqueados.FindIndex(d => d.Faccao == p.Faccao && d.Slot == p.Slot))
                .Where(i => i >= 0)
                .ToList();

            return new FasesVista(faccao.Descricao(), Faccoes.Simbolo(faccao), fases, meus,
                (int)_campanha.UltimaFaseDe(faccao, dificuldade), time,
                MontarDificuldades(), (int)dificuldade);
        }

        private FaseVista MontarFase(Faccao faccao, Fases fase, Dificuldade dificuldade)
        {
            // qualificado: o const Campanha sombreia a classe
            Fase dados = ApostlesWar.Domain.Campanha.ObterFase((int)fase, dificuldade);
            // O que cai é o SLOT, não uma peça: o principal é sorteado no drop, então prometer um
            // stat aqui mentiria em três dos sete slots. Ver DropVista.
            var drop = new DropVista(
                _arsenal.SimboloDoSlot(faccao, fase),
                Equipamento.NomeDoSlot(fase),
                string.Join(" · ", Equipamento.OpcoesDoSlot(fase).Select(NomeDoStat)),
                ArsenalService.ItensPorFase);

            // O inimigo é mostrado NO NÍVEL em que vai entrar — é a única leitura que o jogador tem
            // do quanto aquela dificuldade pesa antes de apertar Lutar.
            int nivel = Progressao.NivelDoInimigo(dificuldade, (int)faccao, (int)fase);

            return new FaseVista(
                (int)fase, fase.Descricao(),
                _capitulos.EstaDesbloqueado(faccao, fase, dificuldade),
                _capitulos.FaseConcluida(faccao, fase, dificuldade),
                Inimigos(faccao, dados.Rodada1, nivel), Inimigos(faccao, dados.Rodada2, nivel), nivel,
                drop);
        }

        // A estrela do inimigo é CAPADA em 6: ele passa de 400 no Pesadelo, e 42 estrelas não caberiam
        // na linha nem diriam nada. Seis é o topo do jogador, então "6★" lê como "no máximo", que é a
        // informação certa. O NÍVEL continua indo como número, e sem trilho de XP (o -1): inimigo não
        // acumula XP, e um trilho vazio nele leria "quase subindo".
        private List<ApostoloVisto> Inimigos(Faccao faccao, List<TipoDeApostolo> tipos, int nivel) => tipos
            .Select(t => _personagens.ObterPorTipo(faccao, t))
            .Select(p => new ApostoloVisto(p.Simbolo, Tipos.Simbolo(p.Tipo), p.Nome, Desbloqueado: true,
                Estrelas: Math.Min(Progressao.Estrelas(nivel), Material.EstrelaMaxima), Nivel: nivel))
            .ToList();

        /// <summary>
        /// O quanto da faixa do nível atual já foi paga, em 0..100 — o trilho da barrinha. No teto ela
        /// fica cheia e para, que é o que o `FaixaDoNivel` devolve.
        /// </summary>
        private int XpPct(Personagem p)
        {
            var (feito, total) = _progressao.FaixaDoNivel(p);
            return total <= 0 ? 100 : (int)(100L * feito / total);
        }

        private RecompensaVista MontarRecompensa(RecompensaDaFase r)
        {
            var novos = r.NovosApostolos.Select(p => new ApostoloVisto(p.Simbolo, Tipos.Simbolo(p.Tipo), p.Nome,
                Desbloqueado: true, _progressao.EstrelasDe(p), p.Nivel)).ToList();
            var itens = r.Itens
                .Select(i => new ItemVista(i.Simbolo, i.Nome, NomeDoStat(i.TipoStat), ValorFormatado(i)))
                .ToList();
            return new RecompensaVista(novos, itens);
        }

        /// <summary>Valida o iniciarFase: fase liberada + time de 1 a 4 dos desbloqueados. Mapeia os índices.</summary>
        private bool ValidarFase(string? texto, Faccao faccao, Dificuldade dificuldade, out Fases fase, out List<Personagem> time)
        {
            fase = default;
            time = new List<Personagem>();
            if (string.IsNullOrEmpty(texto)) return false;

            FaseConfig? cfg;
            try { cfg = JsonSerializer.Deserialize<FaseConfig>(texto, ConfigJson); }
            catch (JsonException) { return false; }
            if (cfg is null || cfg.Fase < 1 || cfg.Fase > 7) return false;

            fase = (Fases)cfg.Fase;
            if (!_capitulos.EstaDesbloqueado(faccao, fase, dificuldade)) return false;

            var pool = _apostolos.ObterDesbloqueados();
            if (cfg.Time is not { Length: >= 1 and <= 4 } || !cfg.Time.All(i => i >= 0 && i < pool.Count))
                return false;

            time = cfg.Time.Select(i => pool[i]).ToList();
            return true;
        }

        /// <summary>Segura uma tela de passagem (recompensa, conquista) até o jogador seguir em frente.</summary>
        private void EsperarContinuar()
        {
            while (true)
            {
                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "continuar") return;
            }
        }

        /// <summary>
        /// Segura a tela de fim de fase até o jogador escolher o que fazer. O Esc chega como "voltar"
        /// e vale <see cref="DecisaoDeFim.Sair"/> — que faz o mesmo que "Editar Equipe", porque sair
        /// desta tela É voltar pra montagem. Ter os dois nomes é honesto: um é gesto de teclado, o
        /// outro é um botão com um propósito escrito nele.
        /// </summary>
        private DecisaoDeFim EsperarDecisao()
        {
            while (true)
            {
                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "voltar") return DecisaoDeFim.Sair;
                if (msg.Tipo == "fimDeFase" && Enum.IsDefined(typeof(DecisaoDeFim), msg.Valor))
                    return (DecisaoDeFim)msg.Valor;
            }
        }

        // ---------- Compêndio ----------

        /// <summary>
        /// Compêndio: o catálogo dos 36, agrupado por facção, TRAVADOS INCLUÍDOS — e a ficha completa
        /// de qualquer um deles. Só lê; nada aqui muda progresso.
        ///
        /// Mostrar apóstolo travado é a decisão que dá sentido à tela: é planejando contra o que ainda
        /// não se tem que a campanha vira escolha. O cadeado diz "ainda não é seu", não "não é da sua
        /// conta" — e é por isso que a ficha não esconde nada.
        ///
        /// Duas telas em UM loop (grade → ficha → grade) pelo mesmo motivo do mapa × fases: quem
        /// responde o Esc/Sair é o C#, então ele precisa saber em qual das duas o jogador está.
        /// </summary>
        private void MostrarCompendio()
        {
            var todos = _apostolos.TodosOsApostolos();

            while (true)
            {
                _ponte.LimparPendentes();
                _ponte.EnviarCompendio(MontarCompendio(todos));

                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "voltar") return;   // volta pro menu principal

                if (msg.Tipo == "verApostolo")
                {
                    int idx = msg.Valor;
                    if (idx < 0 || idx >= todos.Count) continue;
                    MostrarApostoloDetalhe(todos[idx]);
                    // o while redesenha a grade
                }
            }
        }

        /// <summary>A ficha de um apóstolo. Sai no Esc/Sair — não há mais nada a fazer nela.</summary>
        private void MostrarApostoloDetalhe(Personagem apostolo)
        {
            _ponte.LimparPendentes();
            _ponte.EnviarApostoloDetalhe(MontarDetalhe(apostolo));

            while (true)
            {
                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "voltar") return;
            }
        }

        /// <summary>
        /// Agrupa a lista COMPLETA por facção preservando o índice global de cada apóstolo — é ele que o
        /// clique devolve. Agrupar por `GroupBy` em vez de varrer os enums de novo mantém uma ordem
        /// só: a que o <see cref="ApostolosService.TodosOsApostolos"/> definiu.
        /// </summary>
        private CompendioVista MontarCompendio(List<Personagem> todos)
        {
            var faccoes = todos
                .Select((p, indice) => (Personagem: p, Indice: indice))
                .GroupBy(x => x.Personagem.Faccao)
                .Select(g => new CompendioFaccaoVista(
                    g.Key.Descricao(),
                    Faccoes.Simbolo(g.Key),
                    g.Select(x => new CompendioApostoloVista(
                        x.Indice, x.Personagem.Simbolo, x.Personagem.Nome,
                        _apostolos.EstaDesbloqueado(x.Personagem))).ToList()))
                .ToList();

            return new CompendioVista(faccoes);
        }

        /// <summary>
        /// O que o arsenal equipado soma a ESTE apóstolo, stat a stat.
        ///
        /// Sai da DIFERENÇA entre um <see cref="Jogador"/> cru e um vestido, e não de uma conta
        /// própria: os percentuais incidem sobre a base DELE e a fórmula é
        /// `(base + cheios) × (1 + Σ%)` (<see cref="Combate.AplicarItens"/>). Qualquer segunda
        /// implementação aqui faria a ficha prometer um número que a luta não cumpre — que é o mesmo
        /// defeito que o painel de totais tinha, mostrando "+5%" solto sem dizer 5% de quê.
        /// </summary>
        private BonusDoEquipamento BonusDe(Personagem apostolo)
        {
            var cru = new Jogador(apostolo);
            var vestido = new Jogador(apostolo);
            _arsenal.AplicarItens(vestido);

            return new BonusDoEquipamento(
                vestido.HPMaximo - cru.HPMaximo,
                vestido.Ataque - cru.Ataque,
                vestido.Defesa - cru.Defesa,
                vestido.Velocidade - cru.Velocidade,
                vestido.Precisao - cru.Precisao,
                vestido.Resistencia - cru.Resistencia,
                (int)(vestido.TaxaCrit * 100) - (int)(cru.TaxaCrit * 100),
                (int)(vestido.DanoCrit * 100) - (int)(cru.DanoCrit * 100));
        }

        private ApostoloDetalheVista MontarDetalhe(Personagem apostolo) => new(
            apostolo.Nome, apostolo.Simbolo, apostolo.Faccao.Descricao(),
            _apostolos.EstaDesbloqueado(apostolo),
            apostolo.Tipo.Descricao(), Tipos.Simbolo(apostolo.Tipo), apostolo.Nivel, XpPct(apostolo),
            apostolo.HP, apostolo.Ataque, apostolo.Defesa,
            apostolo.Velocidade, apostolo.Precisao, apostolo.Resistencia,
            // O crit vem do TIPO (o Combatente é dono dos dois), não do apóstolo.
            (int)(apostolo.TaxaCrit * 100), (int)(apostolo.DanoCrit * 100),
            // Sem dono: fora da luta não há turno correndo, então o cooldown é o DECLARADO — que é
            // justamente o que se compara entre apóstolos num catálogo. Ver VistaDeHabilidade.
            apostolo.Habilidades.Select(h => VistaDeHabilidade.De(h)).ToList(),
            BonusDe(apostolo));

        // ---------- Arsenal ----------

        /// <summary>
        /// Arsenal: a tela de APRIMORAR o apóstolo. Três colunas — o roster pra escolher, o apóstolo
        /// com o que dá pra comprar nele, e os 7 slots equipados GLOBALMENTE ("em Mim", valem pra
        /// todos). Quem grava item é o <see cref="ArsenalService.EquiparItem"/>; quem grava
        /// nível/estrela é o <see cref="ProgressaoService"/>.
        ///
        /// Toda ação re-renderiza a tela inteira, e é de propósito: comprar uma estrela mexe no nível,
        /// na barra, na ficha e no saldo de alma ao mesmo tempo. Redesenhar tudo é o que impede um
        /// desses quatro de ficar velho na tela.
        /// </summary>
        private void MostrarCatedral()
        {
            int selecionado = 0;

            // A peça que o jogador está OLHANDO na troca (índice no acervo), pra a coluna do meio
            // mostrar o que ela mudaria. Só um índice: a conta é refeita a cada volta, porque ela
            // depende do apóstolo selecionado e do resto do conjunto vestido.
            int candidato = -1;

            while (true)
            {
                var roster = _apostolos.ObterDesbloqueados();
                selecionado = Math.Clamp(selecionado, 0, Math.Max(roster.Count - 1, 0));

                _ponte.LimparPendentes();
                _ponte.EnviarCatedral(MontarCatedral(roster, selecionado, candidato));

                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "voltar") return;

                Personagem? alvo = roster.Count > 0 ? roster[selecionado] : null;

                if (msg.Tipo == "equiparItem")
                {
                    var obtidos = _arsenal.ObterObtidos();
                    if (alvo == null || msg.Valor < 0 || msg.Valor >= obtidos.Count) continue;
                    // Veste no SELECIONADO. Se a peça estava em outro apóstolo, ela sai de lá — o
                    // roubo é o gesto, e quem avisa antes do clique é o emoji do portador no cartão.
                    _arsenal.EquiparItem(alvo, obtidos[msg.Valor]);
                    candidato = -1;   // vestiu: não há mais o que comparar
                }
                else if (msg.Tipo == "desequiparItem")
                {
                    // O valor é o SLOT (0..6), e não o índice no acervo: o botão nasce do boneco, e
                    // slot vazio é a única coisa que ele pode produzir.
                    if (alvo != null && msg.Valor >= 0 && msg.Valor < Enum.GetValues<Fases>().Length)
                        _arsenal.DesequiparItem(alvo, (Fases)(msg.Valor + 1));
                }
                else if (msg.Tipo == "preverItem")
                {
                    // -1 = fechou a comparação. O índice inválido também zera, em vez de continuar
                    // apontando pra uma peça que o acervo já não tem.
                    var obtidos = _arsenal.ObterObtidos();
                    candidato = msg.Valor >= 0 && msg.Valor < obtidos.Count ? msg.Valor : -1;
                }
                else if (msg.Tipo == "selecionarApostolo")
                {
                    if (msg.Valor >= 0 && msg.Valor < roster.Count) selecionado = msg.Valor;
                    candidato = -1;   // trocou de apóstolo: a comparação era com a ficha do outro
                }
                else if (msg.Tipo == "comprarEstrela")
                {
                    if (alvo != null) _progressao.ComprarEstrela(alvo);
                }
                else if (msg.Tipo == "queimarAlma")
                {
                    QueimaPedida? q = LerQueima(msg.Texto);
                    if (q != null && alvo != null)
                        _progressao.QueimarAlma(alvo,
                            q.Faixas.Select(f => new Custo((Raridade)f.Raridade, f.Quantidade)).ToList());
                }
                else if (msg.Tipo == "fundirAlma")
                {
                    // Vem a mesma forma da queima (o painel tem uma barra por faixa e um confirmar
                    // só) e cada faixa é fundida por si: a de baixo primeiro, senão fundir Comum e
                    // Incomum no mesmo gesto perderia o que acabou de nascer na de cima.
                    QueimaPedida? q = LerQueima(msg.Texto);
                    if (q != null)
                        foreach (FaixaQueimada f in q.Faixas.OrderBy(f => f.Raridade))
                            _alma.Fundir((Raridade)f.Raridade, f.Quantidade, MaiorDificuldade());
                }
                else if (msg.Tipo == "abrirForja")
                {
                    var obtidos = _arsenal.ObterObtidos();
                    if (msg.Valor >= 0 && msg.Valor < obtidos.Count) MostrarForja(obtidos[msg.Valor], alvo);
                    // Voltar da Forja cai no topo do laço, que redesenha a Catedral inteira — a peça
                    // pode ter mudado de nível lá dentro, e o boneco mostra isso.
                }
            }
        }

        /// <summary>
        /// A FORJA — a tela da peça, e a irmã da Catedral: lá o centro é o apóstolo e a moeda é alma,
        /// aqui o centro é a PEÇA e a moeda é pó. As três bancadas (⚒️ Bigorna, 💧 Têmpera,
        /// 🏺 Caldeamento) espelham Santuário, Altar e Oferenda.
        ///
        /// Ela é laço PRÓPRIO dentro do laço da Catedral: enquanto o jogador forja, quem responde o
        /// "voltar" é este `while`, e sair dele devolve o comando à Catedral — que redesenha com a
        /// peça já mudada. É o mesmo empilhamento do compêndio → ficha do apóstolo.
        ///
        /// <paramref name="portador"/> é o apóstolo selecionado na Catedral; é contra a ficha DELE
        /// que a prévia mostra o reflexo do nível novo. Nulo (ou peça no baú) apaga só esse reflexo.
        /// </summary>
        /// <returns>
        /// true quando o jogador pediu a CATEDRAL pela aba do título — quem entrou pela Catedral já
        /// volta pra lá de qualquer jeito, mas quem entrou pelo MENU precisa que alguém a abra. Sem
        /// isso a aba escrita "Catedral" devolveria ao menu, e o rótulo estaria mentindo.
        /// </returns>
        private bool MostrarForja(Item peca, Personagem? portador)
        {
            while (true)
            {
                _ponte.LimparPendentes();
                _ponte.EnviarForja(MontarForja(peca, portador));

                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "voltar") return false;
                if (msg.Tipo == "irParaCatedral") return true;

                if (msg.Tipo == "trocarSlot")
                {
                    // As setas percorrem OS ITENS DAQUELE APÓSTOLO (GDD-itens §O que o item por
                    // apóstolo muda nas telas): entrou pelo boneco dele, gira pelo boneco dele.
                    //
                    // Sem portador — peça aberta do baú — o giro é pelos slots em que o jogador tem
                    // ALGUMA peça. Nos dois casos o slot vazio fica de fora: quem ainda não achou uma
                    // bota não vê a bota vazia no meio do giro, e uma tela vazia no caminho faria a
                    // seta parecer quebrada.
                    Item?[] boneco = portador == null
                        ? new Item?[Enum.GetValues<Fases>().Length]
                        : _arsenal.ObterEquipados(portador);
                    var comPeca = (portador == null
                            ? _arsenal.ObterObtidos().Select(i => i.Fase)
                            : boneco.Where(i => i != null).Select(i => i!.Fase))
                        .Distinct().OrderBy(f => f).ToList();

                    int onde = comPeca.IndexOf(peca.Fase);
                    if (comPeca.Count > 1 && onde >= 0)
                    {
                        int passo = msg.Valor >= 0 ? 1 : -1;
                        Fases destino = comPeca[((onde + passo) % comPeca.Count + comPeca.Count) % comPeca.Count];

                        // Chega na peça VESTIDA daquele slot, e não na primeira do baú: é a que está
                        // valendo em combate, e é dela que o jogador quer partir.
                        Item? vestida = boneco[(int)destino - 1];
                        peca = vestida ?? _arsenal.ObterObtidos().First(i => i.Fase == destino);
                    }
                }
                else if (msg.Tipo == "escolherPeca")
                {
                    // A troca é DENTRO do slot: a bigorna não vira um segundo lugar de escolher arma,
                    // e uma peça de outro slot no centro não teria acervo nenhum à esquerda.
                    var doSlot = _arsenal.ObterObtidos().Where(i => i.Fase == peca.Fase).ToList();
                    if (msg.Valor >= 0 && msg.Valor < doSlot.Count) peca = doSlot[msg.Valor];
                }
                else if (msg.Tipo == "queimarPo")
                {
                    QueimaPedida? q = LerQueima(msg.Texto);
                    if (q != null)
                        _arsenal.QueimarPo(peca,
                            q.Faixas.Select(f => new Custo((Raridade)f.Raridade, f.Quantidade)).ToList());
                }
                else if (msg.Tipo == "comprarEstrelaItem")
                {
                    _arsenal.ComprarEstrela(peca);
                }
                else if (msg.Tipo == "fundirPo")
                {
                    // Faixa a faixa, da mais baixa pra mais alta, pelo motivo da fusão de alma: fundir
                    // Comum e Incomum no mesmo gesto perderia o que acabou de nascer na de cima.
                    QueimaPedida? q = LerQueima(msg.Texto);
                    if (q != null)
                        foreach (FaixaQueimada f in q.Faixas.OrderBy(f => f.Raridade))
                            _po.Fundir((Raridade)f.Raridade, f.Quantidade, MaiorDificuldade());
                }
                else if (msg.Tipo == "esmerilharPeca")
                {
                    if (_arsenal.Esmerilhar(peca) != MotivoRecusa.Nenhum) continue;

                    // A peça do centro ACABOU DE SUMIR, e a Forja não existe sem uma. Cai na peça
                    // vestida do slot (é a que está valendo), senão na primeira que sobrou dele — e
                    // se não sobrou nenhuma, sai pra Catedral: o acervo daquele slot zerou.
                    Item? proxima = portador == null ? null : _arsenal.ObterEquipados(portador)[(int)peca.Fase - 1];
                    proxima ??= _arsenal.ObterObtidos().FirstOrDefault(i => i.Fase == peca.Fase);
                    if (proxima == null) return true;
                    peca = proxima;
                }
            }
        }

        /// <summary>
        /// A queima carrega DOIS valores (faixa e quantidade) e a ponte só leva um int por clique —
        /// mesmo motivo do <c>ArenaConfig</c> vir pelo campo Texto.
        /// </summary>
        private static QueimaPedida? LerQueima(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return null;
            try
            {
                QueimaPedida? q = JsonSerializer.Deserialize<QueimaPedida>(texto, ConfigJson);
                if (q?.Faixas is null || q.Faixas.Count == 0) return null;

                int faixas = Enum.GetValues<Raridade>().Length;
                return q.Faixas.All(f => f.Raridade >= 0 && f.Raridade < faixas && f.Quantidade > 0)
                    ? q : null;
            }
            catch (JsonException) { return null; }
        }

        private AprimorarVista? MontarAprimorar(Personagem? apostolo)
        {
            if (apostolo is null) return null;

            int estrelas = _progressao.EstrelasDe(apostolo);
            int teto = _progressao.TetoDe(apostolo);
            bool naParede = _progressao.NaParede(apostolo);
            bool temProxima = estrelas < Material.EstrelaMaxima;

            var receita = temProxima ? Alma.Receita(estrelas + 1) : new List<Custo>();
            var faltando = temProxima ? _alma.Faltando(receita) : new List<Custo>();

            bool podeComprar = naParede && faltando.Count == 0;
            bool podeQueimar = !naParede && apostolo.Nivel < Arquetipos.NivelMaximo;

            // O motivo é UM só e sai pronto do C#: a tela que escolhe a frase acaba tendo de saber
            // quando cada bloqueio vale, e aí a regra estaria em dois lugares.
            string motivo =
                !temProxima ? "No topo: 6 estrelas e nível 60."
                : naParede && faltando.Count > 0 ? $"Falta {Escrever(faltando)} pra próxima estrela."
                : naParede ? $"Travado no nv {teto} — compre a estrela pra continuar."
                : "";

            // A XP que ainda cabe ANTES da parede. É ela que define o "Máximo" da queima: queimar além
            // disso não perde XP (fica guardada), mas gasta a alma que a PRÓPRIA estrela vai cobrar.
            int xpAtual = _progressao.XpDe(apostolo);
            int xpAteAParede = Math.Max(Progressao.XpParaNivel(Math.Min(teto + 1, Arquetipos.NivelMaximo + 1)) - xpAtual, 0);

            // Começa no nível ATUAL, e não no seguinte: a tela precisa do PISO da faixa de hoje pra
            // saber onde a barra está, e deduzi-lo do percentual que ela já mostra era reconstruir
            // por trás um número que o Progressao entrega de graça.
            var limiares = new List<LimiarVista>();
            for (int nivel = apostolo.Nivel; nivel <= Math.Min(teto + 1, Arquetipos.NivelMaximo + 1); nivel++)
                limiares.Add(new LimiarVista(nivel, Progressao.XpParaNivel(nivel)));

            // A ficha de cada nível alcançável, pra a PRÉVIA da barra. Vai a lista inteira e não só o
            // alvo porque o alvo muda a cada pixel arrastado — uma ida à ponte por pixel seria a
            // tela travando pra perguntar o que o C# já sabia.
            var porNivel = new List<StatsDoNivel>();
            for (int nivel = apostolo.Nivel; nivel <= teto; nivel++)
            {
                Personagem n = apostolo.ComNivel(nivel);
                porNivel.Add(new StatsDoNivel(nivel, n.HP, n.Ataque, n.Defesa,
                    n.Velocidade, n.Precisao, n.Resistencia));
            }

            return new AprimorarVista(MontarDetalhe(apostolo), estrelas, teto, naParede,
                receita.Select(VistaDeAlma).ToList(), faltando.Select(VistaDeAlma).ToList(),
                podeComprar, podeQueimar, motivo, xpAtual, xpAteAParede, limiares, porNivel);
        }

        private static AlmaVista VistaDeAlma(Custo c)
            => new((int)c.Raridade, c.Raridade.Descricao(), c.Quantidade, Alma.XpPorAlma(c.Raridade));

        private static string Escrever(IReadOnlyList<Custo> custos)
            => string.Join(" e ", custos.Select(c => $"{c.Quantidade} de {c.Raridade.Descricao()}"));

        private ForjaVista MontarForja(Item peca, Personagem? portador)
        {
            // O acervo é o do SLOT da peça, e o índice que a tela devolve é a posição NESTA lista —
            // é ela que o `escolherPeca` reabre do outro lado.
            var doSlot = _arsenal.ObterObtidos().Where(i => i.Fase == peca.Fase).ToList();
            var acervo = doSlot.Select((it, i) => Ver(it, i, Veste(portador, it))).ToList();

            int teto = Progressao.TetoPorEstrelas(peca.Estrelas);
            bool naParede = _arsenal.NaParede(peca);
            bool temProxima = peca.Estrelas < Material.EstrelaMaxima;

            var receita = temProxima ? Po.Receita(peca.Estrelas + 1) : new List<Custo>();
            var faltando = temProxima ? _po.Faltando(receita) : new List<Custo>();

            bool podeComprar = naParede && faltando.Count == 0;
            bool podeQueimar = !naParede && peca.Nivel < Arquetipos.NivelMaximo;

            string motivo =
                !temProxima && peca.Nivel >= Arquetipos.NivelMaximo ? "No topo: 6 estrelas e nível 60."
                : naParede && faltando.Count > 0 ? $"Falta {Escrever(faltando)} de pó pra próxima têmpera."
                : naParede ? $"Travada no nv {teto} — a têmpera abre a dezena seguinte."
                : "";

            // Os pontos que ainda cabem ANTES da parede — é o "Máximo" da bigorna. Malhar além disso
            // não perde ponto (ele fica guardado na peça), mas gasta o pó que a TÊMPERA vai cobrar.
            int limite = Math.Min(teto + 1, Arquetipos.NivelMaximo);
            int ateAParede = Math.Max(Po.PontosParaNivel(limite) - peca.Pontos, 0);

            var patamares = new List<PatamarVista>();
            for (int nivel = peca.Nivel; nivel <= limite; nivel++)
                patamares.Add(new PatamarVista(nivel, Po.PontosParaNivel(nivel)));

            // O `Max` por faixa é quanto dela cabe até a parede: é o que impede o jogador de torrar
            // pó mítico num nível que a estrela dele nem abriu.
            var po = Enum.GetValues<Raridade>()
                .Select(r =>
                {
                    int vale = Po.PontosPorPo(r);
                    int cabe = podeQueimar ? (ateAParede + vale - 1) / vale : 0;   // teto da divisão
                    return new PoVista((int)r, r.Descricao(), _po.SaldoDe(r), vale,
                        Math.Min(_po.SaldoDe(r), cabe),
                        PodeFundir: _po.SaldoDe(r) >= Material.PorFusao && r < Material.TetoDeFusao(MaiorDificuldade()));
                })
                .ToList();

            return new ForjaVista(Ver(peca, doSlot.FindIndex(i => i.Id == peca.Id), Veste(portador, peca)),
                Equipamento.NomeDoSlot(peca.Fase), acervo,
                // Em quantos slots a SETA tem pra onde ir, e por isso conta o mesmo conjunto que o
                // `trocarSlot` percorre: o boneco do portador, ou o acervo quando a peça é do baú.
                portador == null
                    ? _arsenal.ObterObtidos().Select(i => i.Fase).Distinct().Count()
                    : _arsenal.ObterEquipados(portador).Count(i => i != null),
                teto, naParede, peca.Pontos, ateAParede,
                po, (int)Material.TetoDeFusao(MaiorDificuldade()),
                receita.Select(VistaDePo).ToList(), faltando.Select(VistaDePo).ToList(),
                podeComprar, podeQueimar, motivo,
                patamares, PorNivelDaPeca(peca, portador, teto),
                // Quem veste a peça DE VERDADE, e não o apóstolo por onde se entrou: abrir a Forja e
                // ver o nome de outro é a resposta certa quando a peça está no aliado.
                _arsenal.PortadorDe(peca)?.Nome ?? "",
                VistaDePo(Po.Esmerilhar(peca.Raridade)), _arsenal.PortadorDe(peca) == null);
        }

        /// <summary>Este apóstolo veste ESTA peça? Nulo não veste nada — peça no baú.</summary>
        private bool Veste(Personagem? apostolo, Item peca)
            => apostolo != null && _arsenal.ObterEquipados(apostolo).Any(i => i != null && i.Id == peca.Id);

        /// <summary>
        /// O que a peça vale em cada nível que ela ainda alcança, e o que isso faz na ficha de quem a
        /// veste. O reflexo é a diferença entre o conjunto de hoje e o MESMO conjunto com esta peça
        /// no nível de destino — pelo <see cref="Jogador.AplicarItens"/> da luta, e não somando o
        /// número da peça na mão: um principal em % só vira número em cima da base de alguém.
        /// </summary>
        private List<NivelDaPecaVista> PorNivelDaPeca(Item peca, Personagem? portador, int teto)
        {
            // O reflexo é sempre contra o conjunto DO PORTADOR: subir uma peça que ele não veste não
            // muda ficha nenhuma dele, e mostrar delta ali seria promessa falsa.
            bool vestida = Veste(portador, peca);
            Item?[] hoje = portador is null ? new Item?[Enum.GetValues<Fases>().Length] : _arsenal.ObterEquipados(portador);

            List<DeltaVista> Reflexo(Item simulada)
            {
                if (!vestida || portador is null) return new List<DeltaVista>();

                var depois = hoje.ToArray();
                depois[(int)peca.Fase - 1] = simulada;

                var antes = new Jogador(portador);
                antes.AplicarItens(hoje.Where(i => i != null).Select(i => i!));

                var com = new Jogador(portador);
                com.AplicarItens(depois.Where(i => i != null).Select(i => i!));

                return new List<DeltaVista>
                {
                    Delta("HP", antes.HPMaximo, com.HPMaximo),
                    Delta("Ataque", antes.Ataque, com.Ataque),
                    Delta("Defesa", antes.Defesa, com.Defesa),
                    Delta("Velocidade", antes.Velocidade, com.Velocidade),
                    Delta("Precisão", antes.Precisao, com.Precisao),
                    Delta("Resistência", antes.Resistencia, com.Resistencia),
                    Delta("Taxa de crítico", (int)(antes.TaxaCrit * 100), (int)(com.TaxaCrit * 100), "%"),
                    Delta("Dano crítico", (int)(antes.DanoCrit * 100), (int)(com.DanoCrit * 100), "%"),
                }.Where(d => d.Delta != 0).ToList();
            }

            var fora = new List<NivelDaPecaVista>();
            for (int nivel = peca.Nivel; nivel <= teto; nivel++)
            {
                // Uma CÓPIA com os pontos daquele nível: simular mexendo na peça de verdade gravaria
                // no save um nível que o jogador só estava olhando.
                var simulada = new Item(peca.Nome, peca.Simbolo, peca.Faccao, peca.Fase, peca.TipoStat)
                {
                    Pontos = Po.PontosParaNivel(nivel),
                    Estrelas = peca.Estrelas,
                };
                fora.Add(new NivelDaPecaVista(nivel, ValorFormatado(simulada), Reflexo(simulada)));
            }
            return fora;
        }

        private static PoVista VistaDePo(Custo c)
            => new((int)c.Raridade, c.Raridade.Descricao(), c.Quantidade, Po.PontosPorPo(c.Raridade));

        /// <summary>
        /// Uma peça como a tela a lê. Mora fora do <see cref="MontarCatedral"/> porque a Forja
        /// desenha as mesmas peças — e duas cópias divergiriam no dia em que a raridade entrar.
        /// </summary>
        private ItemArsenalVista Ver(Item it, int indice, bool equipado)
        {
            var (feito, total) = _arsenal.FaixaDoNivel(it);
            return new(
                indice, it.Simbolo, it.Nome, it.Faccao.Descricao(), (int)it.Fase - 1,
                NomeDoStat(it.TipoStat), ValorFormatado(it), it.Valor, equipado,
                _arsenal.PortadorDe(it)?.Simbolo ?? "",
                it.Nivel, it.Estrelas, total <= 0 ? 100 : (int)(100L * feito / total),
                // A CHAVE do stat vai crua junto com o rótulo: "ATK" é o que se lê, mas o filtro
                // precisa distinguir ATKFlat de ATKPct, e os dois se escrevem "ATK".
                it.TipoStat.ToString(), Faccoes.Simbolo(it.Faccao));
        }

        private CatedralVista MontarCatedral(List<Personagem> roster, int selecionado, int candidato = -1)
        {
            Personagem? escolhido = roster.Count > 0 ? roster[selecionado] : null;

            // O boneco é o DELE. Trocar quem está no centro troca a coluna inteira — e é por isso que
            // a tela toda se redesenha ao selecionar outro apóstolo.
            Item?[] equipados = escolhido is null
                ? new Item?[Enum.GetValues<Fases>().Length]
                : _arsenal.ObterEquipados(escolhido);

            var acervo = _arsenal.ObterObtidos();
            // `Equipado` aqui é "vestida POR ELE": é o que marca a peça como já dele. A que está num
            // aliado não é marcada — ela leva o emoji do portador, que é o aviso antes do roubo.
            var obtidos = acervo.Select((it, i) => Ver(it, i, Veste(escolhido, it))).ToList();

            var slots = new List<SlotArsenalVista>();
            foreach (Fases fase in Enum.GetValues<Fases>())
            {
                int s = (int)fase - 1;
                Item? eq = equipados[s];
                // O nome do slot vem do ArsenalService: ele nomeia o slot E o item que cai nele, então
                // um boneco vazio e o item que o preenche não podem discordar (já discordaram).
                //
                // A peça do slot leva o índice REAL no acervo, e não -1: é por ele que o "Melhorar"
                // diz à Forja qual peça pôr na bigorna.
                slots.Add(new SlotArsenalVista(s, Equipamento.NomeDoSlot(fase),
                    eq is null ? null : Ver(eq, acervo.FindIndex(i => i.Id == eq.Id), true)));
            }

            var lista = roster
                .Select(p => new ApostoloVisto(p.Simbolo, Tipos.Simbolo(p.Tipo), p.Nome, Desbloqueado: true,
                    _progressao.EstrelasDe(p), p.Nivel, XpPct(p)))
                .ToList();

            AprimorarVista? aprimorar = MontarAprimorar(escolhido);

            // O `Max` de cada faixa é quanto dela cabe ANTES da parede — é o teto da barrinha da
            // queima, e o que impede o jogador de torrar mítico num nível que ele nem destravou.
            var saldo = Enum.GetValues<Raridade>()
                .Select(r =>
                {
                    int xp = Alma.XpPorAlma(r);
                    int cabe = aprimorar is null || !aprimorar.PodeQueimar
                        ? 0
                        : (aprimorar.XpAteAParede + xp - 1) / xp;   // teto da divisão: o último pedaço conta
                    return new AlmaVista((int)r, r.Descricao(), _alma.SaldoDe(r), xp,
                        Math.Min(_alma.SaldoDe(r), cabe),
                        PodeFundir: _alma.SaldoDe(r) >= Material.PorFusao && r < Material.TetoDeFusao(MaiorDificuldade()));
                })
                .ToList();

            return new CatedralVista(slots, obtidos, lista, selecionado, aprimorar, saldo,
                (int)Material.TetoDeFusao(MaiorDificuldade()),
                PreviaDeTroca(candidato, escolhido));
        }

        /// <summary>
        /// O que trocar a peça <paramref name="candidato"/> faria com a ficha DESTE apóstolo, stat a
        /// stat — e é a resposta à única pergunta que se faz na frente de duas armas: "essa é melhor?".
        ///
        /// A conta é a diferença entre vestir o conjunto de hoje e vestir o conjunto COM a troca, pelo
        /// mesmo <see cref="Combate.AplicarItens"/> da luta. Não dá pra comparar as duas peças
        /// isoladas: elas podem dar stats DIFERENTES (uma Manopla de Taxa contra uma de ATK%), e o %
        /// só vira número em cima da base de alguém. Comparar `valor` com `valor` mentiria nos dois
        /// casos.
        /// </summary>
        private PreviaDeTrocaVista? PreviaDeTroca(int candidato, Personagem? apostolo)
        {
            if (candidato < 0 || apostolo == null) return null;

            var obtidos = _arsenal.ObterObtidos();
            if (candidato >= obtidos.Count) return null;

            Item nova = obtidos[candidato];
            // Comparar a peça consigo mesma não diz nada — mas só se ELE já a veste. A que está num
            // aliado tem prévia sim: é exatamente a conta que o roubo pede antes do clique.
            if (Veste(apostolo, nova)) return null;

            Item?[] hoje = _arsenal.ObterEquipados(apostolo);
            var depois = hoje.ToArray();
            depois[(int)nova.Fase - 1] = nova;

            var antes = new Jogador(apostolo);
            antes.AplicarItens(hoje.Where(i => i != null).Select(i => i!));

            var comATroca = new Jogador(apostolo);
            comATroca.AplicarItens(depois.Where(i => i != null).Select(i => i!));

            var deltas = new List<DeltaVista>
            {
                Delta("HP", antes.HPMaximo, comATroca.HPMaximo),
                Delta("Ataque", antes.Ataque, comATroca.Ataque),
                Delta("Defesa", antes.Defesa, comATroca.Defesa),
                Delta("Velocidade", antes.Velocidade, comATroca.Velocidade),
                Delta("Precisão", antes.Precisao, comATroca.Precisao),
                Delta("Resistência", antes.Resistencia, comATroca.Resistencia),
                Delta("Taxa de crítico", (int)(antes.TaxaCrit * 100), (int)(comATroca.TaxaCrit * 100), "%"),
                Delta("Dano crítico", (int)(antes.DanoCrit * 100), (int)(comATroca.DanoCrit * 100), "%"),
            };

            // Só o que MUDA: uma lista de oito linhas com seis zeros esconde as duas que importam.
            return new PreviaDeTrocaVista(candidato, deltas.Where(d => d.Delta != 0).ToList());
        }

        private static DeltaVista Delta(string rotulo, int antes, int depois, string sufixo = "")
            => new(rotulo, antes, depois, depois - antes, sufixo);

        /// <summary>
        /// A dificuldade mais alta já ABERTA. É ela que trava a fusão (<see cref="Material.TetoDeFusao"/>):
        /// sem isso, 10.000 Comuns farmados no Fácil viram a alma mítica que só o Pesadelo paga.
        /// </summary>
        private Dificuldade MaiorDificuldade() => Enum.GetValues<Dificuldade>()
            .Where(_capitulos.DificuldadeDesbloqueada)
            .DefaultIfEmpty(Dificuldade.Facil)
            .Max();

        // ---------- Formatação de stat (é PELE) ----------
        //
        // Como se ESCREVE um stat na tela — rótulo curto e número com sufixo. Vive aqui, e não no
        // `Item`, porque "0.05" virar "5%" é decisão de exibição: o modelo guarda o número e o tipo,
        // e cada tela escolhe como mostrar (uma tela de comparação poderia querer "+5,0%", um
        // tooltip poderia querer por extenso). O Domain não deve ter opinião sobre casas decimais.

        /// <summary>Rótulo curto do stat, como aparece no card do item.</summary>
        private static string NomeDoStat(TipoStat stat) => stat switch
        {
            TipoStat.ATKFlat or TipoStat.ATKPct => "ATK",
            TipoStat.HPFlat or TipoStat.HPPct => "HP",
            TipoStat.DEFFlat or TipoStat.DEFPct => "DEF",
            TipoStat.TaxaCritPct => "Crit",
            TipoStat.DanoCritPct => "Dano Crit",
            TipoStat.VelocidadeFlat => "Velocidade",
            TipoStat.PrecisaoFlat => "Precisão",
            TipoStat.ResistenciaFlat => "Resistência",
            _ => ""
        };

        /// <summary>O valor como o jogador lê: inteiro cru nos stats FLAT, porcentagem nos PCT.</summary>
        private static string ValorFormatado(Item item) => ValorFormatado(item.TipoStat, item.Valor);

        /// <summary>
        /// O mesmo, a partir do par solto (stat, valor) — é o que o painel de TOTAIS tem em mãos,
        /// porque uma soma de itens não é um item. A sobrecarga acima delega pra cá, pra o card do
        /// item e a linha do total nunca escreverem o mesmo número de dois jeitos.
        /// </summary>
        /// <b>Os CHEIOS são listados um a um, e não por exclusão.</b> Este switch já teve
        /// `_ => porcentagem` como padrão, e os quatro stats cheios que chegaram com o item (Velocidade,
        /// Precisão, Resistência, e o ATK%) caíram calados nessa fresta: a Bota do Reino dá 5,75 de
        /// Velocidade no nível 1 e a tela escreveu <c>+575%</c>. Stat novo tem de aparecer aqui — no
        /// default ele volta a mentir sem quebrar nada.
        private static string ValorFormatado(TipoStat stat, double valor) => stat switch
        {
            TipoStat.ATKFlat or TipoStat.HPFlat or TipoStat.DEFFlat
                or TipoStat.VelocidadeFlat or TipoStat.PrecisaoFlat or TipoStat.ResistenciaFlat
                => $"{(int)valor}",
            TipoStat.ATKPct or TipoStat.HPPct or TipoStat.DEFPct
                or TipoStat.TaxaCritPct or TipoStat.DanoCritPct
                => $"{valor * 100:F0}%",
            _ => $"{valor:F0}"
        };
    }
}
