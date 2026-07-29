using System.Text.Json;
using ApostlesWar.Application;
using ApostlesWar.Application.Portas;
using ApostlesWar.Application.Services;
using ApostlesWar.Domain;

namespace ApostlesWar.Presentation.Front
{
    /// <summary>
    /// O "de fora da luta": perfil do jogador, menu principal, campanha, arena, arsenal,
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
        private const int Arsenal = 2;
        private const int Compendio = 3;
        private const int Configuracao = 4;

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
        private readonly CampeoesService _campeoes;
        private readonly PerfilService _perfil;
        private readonly SessaoDoFront _sessao;
        private readonly CampanhaService _campanha;
        private readonly CapitulosService _capitulos;
        private readonly ArsenalService _arsenal;
        private readonly PersonagemService _personagens;
        private readonly ConfiguracaoService _configuracao;

        public FluxoDoFront(PonteWebView2 ponte, CombateService combate, CampeoesService campeoes,
            PerfilService perfil, SessaoDoFront sessao, CampanhaService campanha, CapitulosService capitulos,
            ArsenalService arsenal, PersonagemService personagens, ConfiguracaoService configuracao)
        {
            _ponte = ponte;
            _combate = combate;
            _campeoes = campeoes;
            _perfil = perfil;
            _sessao = sessao;
            _campanha = campanha;
            _capitulos = capitulos;
            _arsenal = arsenal;
            _personagens = personagens;
            _configuracao = configuracao;
        }

        public void Rodar()
        {
            try
            {
                // Carrega o progresso UMA vez no boot: assim os champs desbloqueados já valem em todo
                // lugar (ex: o picker de avatar do perfil libera conforme a campanha libera os champs),
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

                        case Arsenal:
                            MostrarArsenal();
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
                    new("Arsenal",      "🎒", Habilitado: true),
                    new("Compêndio",    "📖", Habilitado: true),
                    new("Configurações", "⚙️", Habilitado: true),
                    // Não há opção "Sair" na lista: quem sai do jogo é o 🚪 do canto superior direito,
                    // o mesmo botão de todas as outras telas. Duas portas pro mesmo lugar, uma delas
                    // só no menu, era exatamente o que a padronização veio desfazer.
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
        /// Arena (PVP): manda o pool de campeões pro front montar os dois times + escolher o controle
        /// de cada lado, espera a config e roda a luta. Uma batalha por entrada; abortar/sair/Esc volta
        /// pro menu (o loop de Rodar redesenha).
        /// </summary>
        private void MontarArena()
        {
            var pool = _campeoes.TodosOsCampeoes();
            var campeoes = pool.Select(p => new CampeaoVisto(p.Simbolo, p.Nome, Desbloqueado: true)).ToList();

            _ponte.LimparPendentes();
            _ponte.EnviarMontagemArena(campeoes);

            while (true)
            {
                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "voltar") return;   // Esc na montagem → volta pro menu

                if (msg.Tipo == "iniciarArena")
                {
                    ArenaConfig? cfg = LerConfigArena(msg.Texto);
                    if (cfg is null || !ConfigValida(cfg, pool.Count)) continue;   // config inválida: ignora

                    var time1 = cfg.Time1.Select(i => pool[i]).ToList();
                    var time2 = cfg.Time2.Select(i => pool[i]).ToList();

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

        // Cada time: de 1 a 4 champs, índices válidos (o front garante ≥1 de cada lado pra dar 1x1).
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
        /// O front devolve o ÍNDICE na lista completa; quem diz se aquele campeão VALE é o
        /// <see cref="PerfilService.PodeUsarAvatar"/> — aqui só se pinta o que ele responde.
        /// </summary>
        private void MostrarEditarPerfil()
        {
            var todos = _campeoes.TodosOsCampeoes();

            var lista = todos
                .Select(p => new CampeaoVisto(p.Simbolo, p.Nome, _perfil.PodeUsarAvatar(p)))
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

            while (true)
            {
                _ponte.LimparPendentes();
                _ponte.EnviarMapa(MontarMapa(faccoes, posicao));

                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "voltar") return;   // volta pro menu principal

                if (msg.Tipo == "selecionarCapitulo")
                {
                    int idx = msg.Valor;
                    if (idx < 0 || idx >= faccoes.Count) continue;
                    Faccao faccao = faccoes[idx];
                    if (!_capitulos.EstaCapituloDesbloqueado(faccao)) continue;   // bloqueado: ignora

                    posicao = idx;
                    _campanha.SalvarPosicao(posicao);   // último lugar
                    MostrarFases(faccao);
                }
            }
        }

        private MapaVista MontarMapa(List<Faccao> faccoes, int posicao)
        {
            var capitulos = faccoes.Select(f => new CapituloVista(
                Faccoes.Simbolo(f), f.Descricao(),
                _capitulos.EstaCapituloDesbloqueado(f),
                _capitulos.CapituloConcluido(f))).ToList();
            return new MapaVista(capitulos, posicao);
        }

        /// <summary>Tela de fases de uma facção: escolhe a fase, monta o time (≤4 dos liberados) e luta.</summary>
        private void MostrarFases(Faccao faccao)
        {
            while (true)
            {
                _ponte.LimparPendentes();
                _ponte.EnviarFases(MontarFases(faccao));

                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "voltar") return;   // volta pro mapa

                if (msg.Tipo == "iniciarFase" && ValidarFase(msg.Texto, faccao, out Fases fase, out var time))
                {
                    // O "Próxima" pode ter atravessado pro capítulo seguinte, então o laço continua
                    // ONDE O JOGADOR PAROU — e não onde ele entrou. Sem isto, quem virasse de
                    // capítulo lutando cairia de volta na lista de fases do capítulo antigo.
                    faccao = JogarFase(faccao, fase, time);
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
        private Faccao JogarFase(Faccao faccao, Fases fase, List<Personagem> time)
        {
            while (true)
            {
                // Antes de lutar, não depois: se o jogo fechar no meio da luta, o jogador volta na
                // fase em que estava e com o time que montou.
                _campanha.SalvarEntradaNaFase(faccao, fase, time);

                _sessao.Reiniciar();
                _sessao.Modo = ModoDeBatalha.Campanha;   // aqui desistir é DERROTA, não saída
                // O capítulo dá o cenário da luta. O nome do enum É a chave do CSS: um capítulo sem
                // pele própria simplesmente cai no visual padrão, então acrescentar tema é só CSS.
                _sessao.Tema = faccao.ToString().ToLowerInvariant();
                _ponte.DesligarAuto();
                _ponte.LimparPendentes();
                bool venceu = _combate.ExecutarFaseComTime(time, faccao, fase) == ResultadoFase.Venceu;

                // A recompensa é processada ANTES de montar a tela: é ela que desbloqueia a fase
                // seguinte, e é isso que decide se o botão "Próxima Fase" existe.
                var novos = new List<Personagem>();
                RecompensaVista? recompensa = null;
                if (venceu)
                {
                    RecompensaDaFase r = _campanha.ProcessarVitoria(faccao, fase);
                    novos = r.NovosCampeoes;
                    recompensa = MontarRecompensa(r);
                }

                MostrarConquistas(venceu, recompensa, faccao, fase, novos);

                _ponte.EnviarFimDeFase(MontarFimDeFase(venceu, recompensa, faccao, fase, comOpcoes: true));
                switch (EsperarDecisao())
                {
                    case DecisaoDeFim.JogarNovamente:
                        continue;

                    case DecisaoDeFim.ProximaFase:
                        // Não confiamos na tela: ela só desenha o botão quando dá, mas quem responde
                        // "a próxima existe e está liberada?" é o back, aqui, de novo.
                        var proxima = ProximaEtapa(faccao, fase);
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
        /// A celebração do champ conquistado, quando há algum. Ordem pedida pelo Gabriel: primeiro a
        /// tela de vitória com o item em destaque, e só depois cada champ novo — um por vez, cada um
        /// com a própria tela. Sem champ novo isto não faz nada, e a tela de decisão aparece direto.
        ///
        /// O C# conduz a sequência (manda um, espera o "continuar") em vez de despejar a lista e
        /// deixar o JS navegar, pelo mesmo motivo do compêndio: quem responde o Esc/Sair é ele, então
        /// precisa saber em qual tela o jogador está.
        /// </summary>
        private void MostrarConquistas(bool venceu, RecompensaVista? recompensa, Faccao faccao,
            Fases fase, List<Personagem> novos)
        {
            if (novos.Count == 0) return;

            _ponte.EnviarFimDeFase(MontarFimDeFase(venceu, recompensa, faccao, fase, comOpcoes: false));
            EsperarContinuar();

            foreach (Personagem novo in novos)
            {
                _ponte.EnviarConquista(MontarDetalhe(novo));
                EsperarContinuar();
            }
        }

        private FimDeFaseVista MontarFimDeFase(bool venceu, RecompensaVista? recompensa, Faccao faccao,
            Fases fase, bool comOpcoes)
        {
            var proxima = ProximaEtapa(faccao, fase);
            return new FimDeFaseVista(venceu, recompensa,
                PodeProxima: proxima is not null,
                ProximoECapitulo: proxima is not null && proxima.Value.Faccao != faccao,
                comOpcoes);
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
        private (Faccao Faccao, Fases Fase)? ProximaEtapa(Faccao faccao, Fases fase)
        {
            if (fase != Enum.GetValues<Fases>().Last())
                return _capitulos.EstaDesbloqueado(faccao, Proxima(fase))
                    ? (faccao, Proxima(fase))
                    : null;

            var capitulos = _capitulos.FaccoesDaCampanha();
            int proximo = capitulos.IndexOf(faccao) + 1;
            if (proximo <= 0 || proximo >= capitulos.Count) return null;   // era o último

            return _capitulos.EstaDesbloqueado(capitulos[proximo], Fases.Fase1)
                ? (capitulos[proximo], Fases.Fase1)
                : null;
        }

        private static Fases Proxima(Fases fase) => (Fases)((int)fase + 1);

        private FasesVista MontarFases(Faccao faccao)
        {
            var fases = Enum.GetValues<Fases>().Select(f => MontarFase(faccao, f)).ToList();
            var desbloqueados = _campeoes.ObterDesbloqueados();
            var meus = desbloqueados
                .Select(p => new CampeaoVisto(p.Simbolo, p.Nome, Desbloqueado: true)).ToList();

            // O time salvo volta como ÍNDICES nesta lista, porque é isso que o clique devolve. A
            // tradução identidade→índice é do C#: o save guarda quem é o champ (ver
            // CampanhaService.UltimoTime), e a posição na lista é só o endereço de hoje.
            var time = _campanha.UltimoTime()
                .Select(p => desbloqueados.FindIndex(d => d.Faccao == p.Faccao && d.Slot == p.Slot))
                .Where(i => i >= 0)
                .ToList();

            return new FasesVista(faccao.Descricao(), Faccoes.Simbolo(faccao), fases, meus,
                (int)_campanha.UltimaFaseDe(faccao), time);
        }

        private FaseVista MontarFase(Faccao faccao, Fases fase)
        {
            Fase dados = ApostlesWar.Domain.Campanha.ObterFase((int)fase);   // qualificado: o const Campanha sombreia a classe
            Item item = _arsenal.PreverItem(faccao, fase);
            return new FaseVista(
                (int)fase, fase.Descricao(),
                _capitulos.EstaDesbloqueado(faccao, fase),
                _capitulos.FaseConcluida(faccao, fase),
                Inimigos(faccao, dados.Rodada1), Inimigos(faccao, dados.Rodada2),
                new ItemVista(item.Simbolo, item.Nome, NomeDoStat(item.TipoStat), ValorFormatado(item)));
        }

        private List<CampeaoVisto> Inimigos(Faccao faccao, List<Slot> slots) => slots
            .Select(s => _personagens.ObterPersonagem(faccao, s))
            .Select(p => new CampeaoVisto(p.Simbolo, p.Nome, Desbloqueado: true))
            .ToList();

        private static RecompensaVista MontarRecompensa(RecompensaDaFase r)
        {
            var novos = r.NovosCampeoes.Select(p => new CampeaoVisto(p.Simbolo, p.Nome, Desbloqueado: true)).ToList();
            ItemVista? item = r.Item is null ? null
                : new ItemVista(r.Item.Simbolo, r.Item.Nome, NomeDoStat(r.Item.TipoStat), ValorFormatado(r.Item));
            return new RecompensaVista(novos, item);
        }

        /// <summary>Valida o iniciarFase: fase liberada + time de 1 a 4 dos desbloqueados. Mapeia os índices.</summary>
        private bool ValidarFase(string? texto, Faccao faccao, out Fases fase, out List<Personagem> time)
        {
            fase = default;
            time = new List<Personagem>();
            if (string.IsNullOrEmpty(texto)) return false;

            FaseConfig? cfg;
            try { cfg = JsonSerializer.Deserialize<FaseConfig>(texto, ConfigJson); }
            catch (JsonException) { return false; }
            if (cfg is null || cfg.Fase < 1 || cfg.Fase > 7) return false;

            fase = (Fases)cfg.Fase;
            if (!_capitulos.EstaDesbloqueado(faccao, fase)) return false;

            var pool = _campeoes.ObterDesbloqueados();
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
        /// Mostrar champ travado é a decisão que dá sentido à tela: é planejando contra o que ainda
        /// não se tem que a campanha vira escolha. O cadeado diz "ainda não é seu", não "não é da sua
        /// conta" — e é por isso que a ficha não esconde nada.
        ///
        /// Duas telas em UM loop (grade → ficha → grade) pelo mesmo motivo do mapa × fases: quem
        /// responde o Esc/Sair é o C#, então ele precisa saber em qual das duas o jogador está.
        /// </summary>
        private void MostrarCompendio()
        {
            var todos = _campeoes.TodosOsCampeoes();

            while (true)
            {
                _ponte.LimparPendentes();
                _ponte.EnviarCompendio(MontarCompendio(todos));

                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "voltar") return;   // volta pro menu principal

                if (msg.Tipo == "verChamp")
                {
                    int idx = msg.Valor;
                    if (idx < 0 || idx >= todos.Count) continue;
                    MostrarChampDetalhe(todos[idx]);
                    // o while redesenha a grade
                }
            }
        }

        /// <summary>A ficha de um champ. Sai no Esc/Sair — não há mais nada a fazer nela.</summary>
        private void MostrarChampDetalhe(Personagem champ)
        {
            _ponte.LimparPendentes();
            _ponte.EnviarChampDetalhe(MontarDetalhe(champ));

            while (true)
            {
                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "voltar") return;
            }
        }

        /// <summary>
        /// Agrupa a lista COMPLETA por facção preservando o índice global de cada champ — é ele que o
        /// clique devolve. Agrupar por `GroupBy` em vez de varrer os enums de novo mantém uma ordem
        /// só: a que o <see cref="CampeoesService.TodosOsCampeoes"/> definiu.
        /// </summary>
        private CompendioVista MontarCompendio(List<Personagem> todos)
        {
            var faccoes = todos
                .Select((p, indice) => (Personagem: p, Indice: indice))
                .GroupBy(x => x.Personagem.Faccao)
                .Select(g => new CompendioFaccaoVista(
                    g.Key.Descricao(),
                    Faccoes.Simbolo(g.Key),
                    g.Select(x => new CompendioChampVista(
                        x.Indice, x.Personagem.Simbolo, x.Personagem.Nome,
                        _campeoes.EstaDesbloqueado(x.Personagem))).ToList()))
                .ToList();

            return new CompendioVista(faccoes);
        }

        private ChampDetalheVista MontarDetalhe(Personagem champ) => new(
            champ.Nome, champ.Simbolo, champ.Faccao.Descricao(),
            _campeoes.EstaDesbloqueado(champ),
            champ.HP, champ.Ataque, champ.Defesa,
            // Crit é global (não vive no champ): vem das constantes-base do Personagem.
            (int)(Personagem.TaxaCritBase * 100), (int)(Personagem.DanoCritBase * 100),
            // Sem dono: fora da luta não há turno correndo, então o cooldown é o DECLARADO — que é
            // justamente o que se compara entre champs num catálogo. Ver VistaDeHabilidade.
            champ.Habilidades.Select(h => VistaDeHabilidade.De(h)).ToList());

        // ---------- Arsenal ----------

        /// <summary>
        /// Arsenal: o boneco com os 7 slots equipados GLOBALMENTE ("em Mim", valem pra todos os champs)
        /// e os itens obtidos pra escolher. Quem grava é o <see cref="ArsenalService.EquiparItem"/>.
        /// </summary>
        private void MostrarArsenal()
        {
            while (true)
            {
                _ponte.LimparPendentes();
                _ponte.EnviarArsenal(MontarArsenal());

                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "voltar") return;
                if (msg.Tipo == "equiparItem")
                {
                    var obtidos = _arsenal.ObterObtidos();
                    if (msg.Valor < 0 || msg.Valor >= obtidos.Count) continue;
                    _arsenal.EquiparItem(obtidos[msg.Valor]);
                    // o while re-renderiza o arsenal atualizado
                }
            }
        }

        private ArsenalVista MontarArsenal()
        {
            var equipados = _arsenal.ObterEquipados();

            ItemArsenalVista Ver(Item it, int indice, bool equipado) => new(
                indice, it.Simbolo, it.Nome, it.Faccao.Descricao(), (int)it.Fase - 1,
                NomeDoStat(it.TipoStat), ValorFormatado(it), it.Valor, equipado);

            var obtidos = _arsenal.ObterObtidos().Select((it, i) => Ver(it, i, _arsenal.EstaEquipado(it))).ToList();

            var slots = new List<SlotArsenalVista>();
            foreach (Fases fase in Enum.GetValues<Fases>())
            {
                int s = (int)fase - 1;
                Item? eq = equipados[s];
                // O nome do slot vem do ArsenalService: ele nomeia o slot E o item que cai nele, então
                // um boneco vazio e o item que o preenche não podem discordar (já discordaram).
                slots.Add(new SlotArsenalVista(s, ArsenalService.NomeDoSlot(fase),
                    eq is null ? null : Ver(eq, -1, true)));
            }

            var totais = _arsenal.TotaisEquipados()
                .OrderBy(b => Array.IndexOf(OrdemDeLeituraDosStats, b.Stat))
                .Select(b => new BonusVista(NomeDoStat(b.Stat), $"+{ValorFormatado(b.Stat, b.Valor)}"))
                .ToList();

            return new ArsenalVista(slots, totais, obtidos);
        }

        /// <summary>
        /// A ordem em que os totais são LIDOS — e é por isso que ela mora aqui e não no service.
        ///
        /// Não é a ordem do enum nem a dos slots: HP vem de DOIS slots (o plano e o percentual) e DEF
        /// também, e o <see cref="NomeDoStat"/> chama os dois de "HP" e "DEF" — como deve, é o mesmo
        /// stat. Longe uma da outra, as duas linhas parecem repetição; lado a lado, leem-se como uma
        /// coisa só ("HP +300, HP +15%"), que é o que elas são.
        /// </summary>
        private static readonly TipoStat[] OrdemDeLeituraDosStats =
        {
            TipoStat.ATKFlat,
            TipoStat.HPFlat, TipoStat.HPPct,
            TipoStat.DEFFlat, TipoStat.DEFPct,
            TipoStat.TaxaCritPct, TipoStat.DanoCritPct,
        };

        // ---------- Formatação de stat (é PELE) ----------
        //
        // Como se ESCREVE um stat na tela — rótulo curto e número com sufixo. Vive aqui, e não no
        // `Item`, porque "0.05" virar "5%" é decisão de exibição: o modelo guarda o número e o tipo,
        // e cada tela escolhe como mostrar (uma tela de comparação poderia querer "+5,0%", um
        // tooltip poderia querer por extenso). O Domain não deve ter opinião sobre casas decimais.

        /// <summary>Rótulo curto do stat, como aparece no card do item.</summary>
        private static string NomeDoStat(TipoStat stat) => stat switch
        {
            TipoStat.ATKFlat => "ATK",
            TipoStat.HPFlat => "HP",
            TipoStat.DEFFlat => "DEF",
            TipoStat.HPPct => "HP",
            TipoStat.DEFPct => "DEF",
            TipoStat.TaxaCritPct => "Crit",
            TipoStat.DanoCritPct => "Dano Crit",
            _ => ""
        };

        /// <summary>O valor como o jogador lê: inteiro cru nos stats FLAT, porcentagem nos PCT.</summary>
        private static string ValorFormatado(Item item) => ValorFormatado(item.TipoStat, item.Valor);

        /// <summary>
        /// O mesmo, a partir do par solto (stat, valor) — é o que o painel de TOTAIS tem em mãos,
        /// porque uma soma de itens não é um item. A sobrecarga acima delega pra cá, pra o card do
        /// item e a linha do total nunca escreverem o mesmo número de dois jeitos.
        /// </summary>
        private static string ValorFormatado(TipoStat stat, double valor) => stat switch
        {
            TipoStat.ATKFlat or TipoStat.HPFlat or TipoStat.DEFFlat => $"{(int)valor}",
            _ => $"{valor * 100:F0}%"
        };
    }
}
