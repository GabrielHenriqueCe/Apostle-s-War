using ApostlesWar.Application;
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

        private readonly PonteWebView2 _ponte;
        private readonly CombateService _combate;
        private readonly CampeoesService _campeoes;
        private readonly PerfilService _perfil;
        private readonly SessaoDoFront _sessao;

        public FluxoDoFront(PonteWebView2 ponte, CombateService combate, CampeoesService campeoes,
            PerfilService perfil, SessaoDoFront sessao)
        {
            _ponte = ponte;
            _combate = combate;
            _campeoes = campeoes;
            _perfil = perfil;
            _sessao = sessao;
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
                        case Arena:
                            // A Arena volta sozinha quando é abortada (Esc/sair); só esperamos o clique
                            // de "voltar" quando ela terminou naturalmente e mostrou o resultado.
                            if (RodarArenaRapida()) EsperarVoltarAoMenu();
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
                    new("Campanha",     "🗺️", Habilitado: false),   // próxima fatia
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

        /// <summary>Arena rápida: dois times NOVOS sorteados a cada clique (o sorteio é do service).</summary>
        /// <returns>Repassa o retorno de ExecutarArenaComTimes (true = terminou; false = abortada).</returns>
        private bool RodarArenaRapida()
        {
            _sessao.Reiniciar();   // batalha nova = tela limpa (senão os combatentes antigos acumulam)
            var (time1, time2) = _campeoes.SortearDoisTimesArena();
            return _combate.ExecutarArenaComTimes(time1, time2, bot1: false, bot2: true);
        }

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

        /// <summary>Avatar placeholder: o emoji de um dos 4 Humanos (o time inicial).</summary>
        private string AvatarAleatorioHumano()
        {
            var humanos = _campeoes.TodosOsCampeoes().Where(p => p.Faccao == Faccao.Humanos).ToList();
            return humanos[Random.Shared.Next(humanos.Count)].Simbolo;
        }
    }
}
