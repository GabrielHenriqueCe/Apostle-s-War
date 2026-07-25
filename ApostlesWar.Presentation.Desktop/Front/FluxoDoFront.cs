using System.Text.Json;
using ApostlesWar.Application;
using ApostlesWar.Application.Portas;
using ApostlesWar.Application.Services;
using ApostlesWar.Domain;

namespace ApostlesWar.Presentation.Desktop.Front
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
        private const int Configuracao = 3;
        private const int Sair = 4;

        // Índices do menu de CONFIGURAÇÃO.
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

        public FluxoDoFront(PonteWebView2 ponte, CombateService combate, CampeoesService campeoes,
            PerfilService perfil, SessaoDoFront sessao, CampanhaService campanha, CapitulosService capitulos,
            ArsenalService arsenal, PersonagemService personagens)
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

                        case Configuracao:
                            if (MostrarConfiguracao()) GarantirPerfil();   // conta excluída → pede nome de novo
                            break;

                        case EditarPerfil:
                            MostrarEditarPerfil();
                            break;

                        case Sair:
                            _ponte.FecharJanela();
                            return;
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
                    _sessao.Reiniciar();
                    _ponte.DesligarAuto();
                    _ponte.LimparPendentes();
                    ResultadoFase resultado = _combate.ExecutarFaseComTime(time, faccao, fase);

                    if (resultado == ResultadoFase.Venceu)
                        _ponte.EnviarVitoria(MontarRecompensa(_campanha.ProcessarVitoria(faccao, fase)));
                    else
                        _ponte.EnviarDerrota();

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
            return new ArsenalVista(slots, obtidos);
        }

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
        private static string ValorFormatado(Item item) => item.TipoStat switch
        {
            TipoStat.ATKFlat or TipoStat.HPFlat or TipoStat.DEFFlat => $"{(int)item.Valor}",
            _ => $"{item.Valor * 100:F0}%"
        };
    }
}
