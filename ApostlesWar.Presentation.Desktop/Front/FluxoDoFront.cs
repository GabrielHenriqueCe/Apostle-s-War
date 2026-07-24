using System.Text.Json;
using ApostlesWar.Application;
using ApostlesWar.Application.Portas;
using ApostlesWar.Application.Services;
using ApostlesWar.Domain;

namespace ApostlesWar.Presentation.Desktop.Front
{
    /// <summary>
    /// O "de fora da luta" do front: perfil do jogador, menu principal, configurações — e roteia a
    /// escolha. É o irmão de menu do que o <see cref="ControladorJogadorWeb"/> é pro combate:
    /// navegação por CLIQUE, não por cursor (por isso não roda o GerenciadorDeJogoService, que é o
    /// fluxo de console). Reaproveita os mesmos SERVICES; só a casca de navegação é do front.
    ///
    /// FATIA ATUAL: perfil (nome + avatar) → menu principal → Arena / Configurações. Campanha e o
    /// Novo Jogo/Continuar de progresso entram quando a Campanha existir no front (hoje "em breve").
    /// </summary>
    internal class FluxoDoFront
    {
        // Índices do MENU PRINCIPAL — casam com a ordem enviada em MostrarMenuPrincipal.
        private const int Campanha = 0;
        private const int Arena = 1;
        private const int Configuracao = 2;
        private const int Sair = 3;

        // Índices do menu de CONFIGURAÇÃO.
        private const int CfgConta = 2;

        // Índices do menu de CONTA.
        private const int ContaExcluir = 0;

        // Retornos de LerEscolha que NÃO são um índice de opção.
        private const int EscVoltar = -1;      // Esc pediu "voltar" num submenu
        private const int EditarPerfil = -2;   // clique no avatar → editar perfil

        // "Janela fechada" desenrola a navegação inteira (mesma ideia do BatalhaAbortada no motor).
        private sealed class JogoEncerrado : Exception { }

        // Chave do save da POSIÇÃO no mapa (último lugar). Ausente = 0 = Reino.
        private const string ChavePosicao = "campanha";

        // As 8 facções-capítulo, em ordem (Humanos é o time inicial, não é capítulo).
        private static readonly List<Faccao> FaccoesDaCampanha =
            Enum.GetValues<Faccao>().Where(f => f != Faccao.Humanos).ToList();

        private readonly PonteWebView2 _ponte;
        private readonly CombateService _combate;
        private readonly CampeoesService _campeoes;
        private readonly PerfilService _perfil;
        private readonly SessaoDoFront _sessao;
        private readonly CampanhaService _campanha;
        private readonly CapitulosService _capitulos;
        private readonly ArsenalService _arsenal;
        private readonly PersonagemService _personagens;
        private readonly IRepositorioDeSave _repositorio;

        public FluxoDoFront(PonteWebView2 ponte, CombateService combate, CampeoesService campeoes,
            PerfilService perfil, SessaoDoFront sessao, CampanhaService campanha, CapitulosService capitulos,
            ArsenalService arsenal, PersonagemService personagens, IRepositorioDeSave repositorio)
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
            _repositorio = repositorio;
        }

        public void Rodar()
        {
            try
            {
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

                        case Configuracao:
                            if (MostrarConfiguracao()) GarantirPerfil();   // conta excluída → pede nome de novo
                            break;

                        case EditarPerfil:
                            MostrarEditarPerfil();
                            break;

                        case Sair:
                            _ponte.FecharJanela();
                            return;

                        // Campanha vem desabilitada (fatia futura) e não chega aqui.
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
                    _perfil.CriarPerfil(nome, AvatarAleatorioHumano());
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
                    new("Configurações", "⚙️", Habilitado: true),
                    new("Sair",         "🚪", Habilitado: true),
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
                        new("Tela cheia", "🖥️", Habilitado: false),   // fatia futura
                        new("Conta",      "👤", Habilitado: true),
                        new("Voltar",     "⬅️", Habilitado: true),
                    }));

                int escolha = LerEscolha();
                if (escolha == CfgConta)
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
        /// Editar perfil: troca o nome e escolhe o avatar entre os campeões DESBLOQUEADOS (os
        /// bloqueados aparecem em cinza na grade). O front devolve o ÍNDICE na lista completa; o
        /// desbloqueio é decidido AQUI (regra de jogo), o front só pinta o que recebe.
        /// </summary>
        private void MostrarEditarPerfil()
        {
            var todos = _campeoes.TodosOsCampeoes();
            var desbloqueados = _campeoes.ObterDesbloqueados()
                .Select(p => (p.Faccao, p.Slot)).ToHashSet();

            var lista = todos
                .Select(p => new CampeaoVisto(p.Simbolo, p.Nome, desbloqueados.Contains((p.Faccao, p.Slot))))
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
                    if (!desbloqueados.Contains((escolhido.Faccao, escolhido.Slot))) continue;   // bloqueado
                    _perfil.CriarPerfil(nome, escolhido.Simbolo);   // sobrescreve (mesma chave)
                    return;
                }
            }
        }

        // ---------- Campanha ----------

        /// <summary>
        /// Campanha: carrega o progresso, mostra o mapa e roteia. O save fully-unlocked do console
        /// carrega direto; sem save, cai nos defaults (Reino fase 1, só Humanos). A posição no mapa
        /// (último lugar) persiste na chave "campanha".
        /// </summary>
        private void MostrarCampanha()
        {
            _campanha.CarregarSaves();
            int posicao = _repositorio.Carregar<int>(ChavePosicao);   // 0 (ausente) = Reino

            while (true)
            {
                _ponte.LimparPendentes();
                _ponte.EnviarMapa(MontarMapa(posicao));

                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "voltar") return;   // volta pro menu principal

                if (msg.Tipo == "selecionarCapitulo")
                {
                    int idx = msg.Valor;
                    if (idx < 0 || idx >= FaccoesDaCampanha.Count) continue;
                    Faccao faccao = FaccoesDaCampanha[idx];
                    if (!_capitulos.EstaCapituloDesbloqueado(faccao)) continue;   // bloqueado: ignora

                    posicao = idx;
                    _repositorio.Salvar(ChavePosicao, posicao);   // último lugar
                    MostrarFases(faccao);
                }
            }
        }

        private MapaVista MontarMapa(int posicao)
        {
            var capitulos = FaccoesDaCampanha.Select(f => new CapituloVista(
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
                    _sessao.Reiniciar();
                    _ponte.LimparPendentes();
                    ResultadoFase resultado = _combate.ExecutarFaseComTime(time, faccao, fase);

                    if (resultado == ResultadoFase.Venceu)
                        _ponte.EnviarVitoria(MontarRecompensa(_campanha.ProcessarVitoria(faccao, fase)));
                    else
                        _ponte.EnviarDerrota();   // Perdeu (Cancelou não acontece — o front pica o time)

                    EsperarContinuar();
                    // o while re-renderiza as fases já atualizadas
                }
            }
        }

        private FasesVista MontarFases(Faccao faccao)
        {
            var fases = Enum.GetValues<Fases>().Select(f => MontarFase(faccao, f)).ToList();
            var meus = _campeoes.ObterDesbloqueados()
                .Select(p => new CampeaoVisto(p.Simbolo, p.Nome, Desbloqueado: true)).ToList();
            return new FasesVista(faccao.Descricao(), Faccoes.Simbolo(faccao), fases, meus);
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
                new ItemVista(item.Simbolo, item.Nome, item.NomeStat(), item.ValorFormatado()));
        }

        private List<CampeaoVisto> Inimigos(Faccao faccao, List<Slot> slots) => slots
            .Select(s => _personagens.ObterPersonagem(faccao, s))
            .Select(p => new CampeaoVisto(p.Simbolo, p.Nome, Desbloqueado: true))
            .ToList();

        private static RecompensaVista MontarRecompensa(RecompensaDaFase r)
        {
            var novos = r.NovosCampeoes.Select(p => new CampeaoVisto(p.Simbolo, p.Nome, Desbloqueado: true)).ToList();
            ItemVista? item = r.Item is null ? null
                : new ItemVista(r.Item.Simbolo, r.Item.Nome, r.Item.NomeStat(), r.Item.ValorFormatado());
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

        /// <summary>Segura a tela de vitória/derrota até o jogador clicar pra continuar.</summary>
        private void EsperarContinuar()
        {
            while (true)
            {
                MensagemDoFront msg = _ponte.Esperar();
                if (msg.Tipo == "encerrar") throw new JogoEncerrado();
                if (msg.Tipo == "continuar") return;
            }
        }

        /// <summary>Avatar placeholder: o emoji de um dos 4 Humanos (o time inicial).</summary>
        private string AvatarAleatorioHumano()
        {
            var humanos = _campeoes.TodosOsCampeoes().Where(p => p.Faccao == Faccao.Humanos).ToList();
            return humanos[Random.Shared.Next(humanos.Count)].Simbolo;
        }
    }
}
