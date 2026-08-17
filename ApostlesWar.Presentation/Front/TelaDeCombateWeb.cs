using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;

namespace ApostlesWar.Presentation.Front
{
    /// <summary>
    /// A impl WEB da porta <see cref="ITelaDeCombate"/>. Note o que ela NÃO faz: desenhar. Cada chamada
    /// imperativa da porta ("exiba isto") é traduzida em uma de duas coisas:
    ///   - um RETRATO novo do estado (a tela se redesenha sozinha a partir dele), ou
    ///   - um EVENTO (o número que pula, o alvo que treme).
    /// Essa inversão é o que torna a tela declarativa e o que permite trocar emoji por sprite depois
    /// sem encostar no motor.
    /// </summary>
    internal class TelaDeCombateWeb : ITelaDeCombate
    {
        private readonly SessaoDoFront _sessao;
        private readonly PonteWebView2 _ponte;

        public TelaDeCombateWeb(SessaoDoFront sessao, PonteWebView2 ponte)
        {
            _sessao = sessao;
            _ponte = ponte;
        }

        /// <summary>Vazio de propósito: a tela se redesenha a partir do estado, não há o que apagar.</summary>
        public void LimparTela() { }

        public void ExibirPartida(List<Combate> jogadores, List<Combate> inimigos)
        {
            _sessao.RegistrarLados(jogadores, inimigos);
            _sessao.Publicar();
        }

        // A fila não publica sozinha: ela guarda e entra no PRÓXIMO retrato, que vem logo em seguida
        // (o anúncio do turno). Publicar aqui seria um quadro a mais dizendo a mesma coisa.
        public void ExibirFilaDeTurnos(IReadOnlyList<FilaDeTurnos.Vez> fila) => _sessao.GuardarFila(fila);

        // Arena (PVP): fixa os lados na ordem montada, antes do 1º retrato.
        public void ExibirInicioArena(List<Combate> equipe1, List<Combate> equipe2)
            => _sessao.FixarLados(equipe1, equipe2);

        public void ExibirResultadoAtaque(Combate atacante, Combate alvo, EventoDano r)
        {
            _ponte.EnviarEvento(new EventoVisto(
                Tipo: "dano",
                AlvoId: _sessao.IdDe(alvo),
                Valor: r.DanoEfetivo,
                Critico: r.Critico,
                AbsorvidoPeloEscudo: r.AbsorvidoPeloEscudo,
                Texto: null));

            if (!alvo.EstaVivo())
                _ponte.EnviarEvento(new EventoVisto("morte", _sessao.IdDe(alvo), 0, false, 0, null));

            // Sincroniza SÓ este alvo: a barra dele desce junto com o número, e num golpe em área as
            // barras dos outros alvos esperam a própria vez de ser narradas.
            _sessao.SincronizarVida(alvo);
            _sessao.Publicar(sincronizarVida: false);
        }

        public void ExibirDanoDeStatus(EventoDano r)
        {
            _ponte.EnviarEvento(new EventoVisto(
                Tipo: "dano", AlvoId: _sessao.IdDe(r.Alvo), Valor: r.DanoEfetivo,
                Critico: false, AbsorvidoPeloEscudo: r.AbsorvidoPeloEscudo,
                Texto: null));

            if (!r.Alvo.EstaVivo())
                _ponte.EnviarEvento(new EventoVisto("morte", _sessao.IdDe(r.Alvo), 0, false, 0, null));

            _sessao.SincronizarVida(r.Alvo);          // barra do alvo desce junto com o tick
            _sessao.Publicar(sincronizarVida: false);
        }

        public void ExibirCura(EventoCura c)
        {
            _ponte.EnviarEvento(new EventoVisto(
                Tipo: "cura", AlvoId: _sessao.IdDe(c.Alvo), Valor: c.Quantidade,
                Critico: false, AbsorvidoPeloEscudo: 0, Texto: null));
            _sessao.SincronizarVida(c.Alvo);          // barra sobe junto com o número de cura
            _sessao.Publicar(sincronizarVida: false);
        }

        public void ExibirMensagemPassiva(string mensagem)
        {
            if (string.IsNullOrEmpty(mensagem)) return;
            _sessao.Mensagem = mensagem;
            _ponte.EnviarEvento(new EventoVisto("narracao", null, 0, false, 0, mensagem));
            _sessao.Publicar();
        }

        public void ExibirPreparacaoAtaque(Combate atacante, List<Combate> defensores)
        {
            _sessao.QuemAge = atacante;
            _sessao.Fase = FaseDaTela.Assistindo;
            _sessao.HabilidadesDoTurno = new List<HabilidadeAtiva>();
            _sessao.AlvosValidos = new List<Combate>();
            _sessao.Mensagem = $"{atacante.Personagem.Simbolo} {atacante.Personagem.Nome} prepara o ataque!";
            _sessao.Publicar();
        }

        /// <summary>
        /// A "fala" do apóstolo ao usar a habilidade — o balão que o Gabriel pediu.
        ///
        /// Publica SEM SINCRONIZAR A VIDA. Quando isto roda, o `hab.Ativar` já mexeu no modelo (o
        /// CombateService executa tudo e só depois narra): sincronizar aqui entregaria o HP já
        /// descontado e a barra cairia antes do número de dano subir. Mas PRECISA publicar — buff,
        /// debuff e escudo não geram evento nenhum, então sem este retrato eles só apareceriam no
        /// turno seguinte. Vida fica pro <see cref="ExibirResultadoAtaque"/>; o resto atualiza já.
        /// </summary>
        public void ExibirUsoHabilidade(Combate atacante, Habilidade hab)
        {
            _sessao.Mensagem = $"{atacante.Personagem.Simbolo} usou {hab.Simbolo} {hab.Nome}!";
            _ponte.EnviarEvento(new EventoVisto(
                "narracao", _sessao.IdDe(atacante), 0, false, 0, _sessao.Mensagem));
            _sessao.Publicar(sincronizarVida: false);
        }

        // Campanha: o fim é a tela de VITÓRIA/DERROTA que o FluxoDoFront manda logo depois (com apóstolos
        // novos + item). Aqui é no-op pra não piscar o overlay genérico de "Fim da batalha!" antes dela.
        public void ExibirResumoBatalha(List<Combate> jogador) { }

        // Arena (PVP): os lados foram fixados na ordem montada (equipe1=esquerda), então o vencedor
        // vira o LADO da tela — a mensagem por metade (Vitória de um lado, Derrota do outro) é decidida
        // no JS a partir disso.
        public void ExibirResumoArena(List<Combate> equipe1, List<Combate> equipe2, bool venceuEquipe1)
        {
            _sessao.LadoVencedor = venceuEquipe1 ? 1 : 2;
            Encerrar(venceuEquipe1 ? "🏆 Equipe da esquerda venceu!" : "🏆 Equipe da direita venceu!");
        }

        private void Encerrar(string texto)
        {
            _sessao.Fase = FaseDaTela.Fim;
            _sessao.QuemAge = null;
            _sessao.HabilidadesDoTurno = new List<HabilidadeAtiva>();
            _sessao.AlvosValidos = new List<Combate>();
            _sessao.Mensagem = texto;
            _sessao.Publicar();
        }

        /// <summary>
        /// Só é chamado pelo CombateService.Aguardar DEPOIS de AguardarAnimacao já ter detectado o
        /// pedido de sair (o JS confirmou no modal ANTES de mandar "sair"). Então aqui é sempre sim.
        /// </summary>
        public bool ConfirmarEncerramento() => true;
    }
}
