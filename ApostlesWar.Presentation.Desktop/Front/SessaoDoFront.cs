using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills;
using ApostlesWar.Domain.Skills.Buffs;

namespace ApostlesWar.Presentation.Desktop.Front
{
    /// <summary>
    /// O estado que a tela enxerga, num lugar só. Existe porque o retrato da partida é montado por DUAS
    /// mãos: a <see cref="TelaDeCombateWeb"/> sabe COMO os times estão, e o
    /// <see cref="ControladorJogadorWeb"/> sabe O QUE dá pra clicar agora. Sem um dono comum, cada um
    /// mandaria meio retrato e a tela piscaria.
    ///
    /// Também é aqui que os objetos do domínio viram números: o JS recebe Ids, nunca referências. E é
    /// aqui que se decide QUEM FICA NA ESQUERDA — ver <see cref="LadoDe"/>.
    /// </summary>
    internal class SessaoDoFront
    {
        private readonly PonteWebView2 _ponte;
        private readonly RelogioDoCombate _relogio;

        // Combate não sobrescreve Equals, então o dicionário casa por REFERÊNCIA — que é o que
        // queremos: dois champs iguais em ficha são combatentes distintos na luta.
        private readonly Dictionary<Combate, int> _ids = new();
        private readonly Dictionary<int, Combate> _porId = new();
        private readonly Dictionary<Combate, int> _lado = new();
        private int _proximoId = 1;
        private bool _ladoDoJogadorDefinido;

        public SessaoDoFront(PonteWebView2 ponte, RelogioDoCombate relogio)
        {
            _ponte = ponte;
            _relogio = relogio;
        }

        // --- o que a tela precisa saber pra desenhar o quadro atual ---
        public FaseDaTela Fase { get; set; } = FaseDaTela.Assistindo;
        public Combate? QuemAge { get; set; }
        public List<HabilidadeAtiva> HabilidadesDoTurno { get; set; } = new();
        public List<Combate> AlvosValidos { get; set; } = new();
        public string? Mensagem { get; set; }
        public int LadoVencedor { get; set; }   // no Fim: 1=esquerda venceu, 2=direita; 0=sem split

        /// <summary>
        /// Zera o estado pra uma batalha NOVA. A sessão vive o boot inteiro (não é recriada por luta),
        /// e os mapas de combatente casam por REFERÊNCIA — sem limpar, o Publicar mandaria os
        /// combatentes das batalhas anteriores junto (bug do "time antigo acumulando na tela"). Chamado
        /// pelo FluxoDoFront antes de cada Arena.
        /// </summary>
        public void Reiniciar()
        {
            _ids.Clear();
            _porId.Clear();
            _lado.Clear();
            _mostrado.Clear();
            _proximoId = 1;
            _ladoDoJogadorDefinido = false;

            Fase = FaseDaTela.Assistindo;
            QuemAge = null;
            HabilidadesDoTurno = new();
            AlvosValidos = new();
            Mensagem = null;
            LadoVencedor = 0;
        }

        /// <summary>
        /// Arena (PVP): fixa os lados na ORDEM montada — equipe1 à esquerda, equipe2 à direita —,
        /// independente de quem controla. Marca <c>_ladoDoJogadorDefinido</c> pra o
        /// <see cref="DefinirLadoDoJogador"/> (o "humano à esquerda" da campanha) NÃO sobrescrever.
        /// </summary>
        public void FixarLados(List<Combate> equipe1, List<Combate> equipe2)
        {
            foreach (Combate c in equipe1) { IdDe(c); _lado[c] = 1; }
            foreach (Combate c in equipe2) { IdDe(c); _lado[c] = 2; }
            _ladoDoJogadorDefinido = true;
        }

        public int IdDe(Combate c)
        {
            if (_ids.TryGetValue(c, out int id)) return id;
            id = _proximoId++;
            _ids[c] = id;
            _porId[id] = c;
            return id;
        }

        public Combate? PorId(int id) => _porId.GetValueOrDefault(id);

        /// <summary>
        /// De que lado da tela um combatente mora. O motor não tem "esquerda/direita": ele passa
        /// (aliados, inimigos) da perspectiva de QUEM ESTÁ AGINDO — ou seja, os lados se invertem
        /// quando o bot joga. Como o Gabriel quer o time dele SEMPRE à esquerda, o lado é decidido na
        /// PRIMEIRA vez que vemos cada combatente e nunca mais muda.
        /// </summary>
        private int LadoDe(Combate c) => _lado.GetValueOrDefault(c, 1);

        /// <summary>Fixa os lados na primeira visão (provisório, pela perspectiva de quem age).</summary>
        public void RegistrarLados(List<Combate> ladoEsquerdo, List<Combate> ladoDireito)
        {
            foreach (Combate c in ladoEsquerdo) { IdDe(c); if (!_lado.ContainsKey(c)) _lado[c] = 1; }
            foreach (Combate c in ladoDireito) { IdDe(c); if (!_lado.ContainsKey(c)) _lado[c] = 2; }
        }

        /// <summary>
        /// Correção AUTORITATIVA: quando o humano vai agir, os aliados DELE são, por definição, o lado
        /// esquerdo. Roda uma vez só — se o bot agiu primeiro, conserta o provisório do RegistrarLados.
        /// </summary>
        public void DefinirLadoDoJogador(List<Combate> aliadosDoHumano, List<Combate> adversarios)
        {
            if (_ladoDoJogadorDefinido) return;
            _ladoDoJogadorDefinido = true;
            foreach (Combate c in aliadosDoHumano) { IdDe(c); _lado[c] = 1; }
            foreach (Combate c in adversarios) { IdDe(c); _lado[c] = 2; }
        }

        // HP/vida COMO A TELA ESTÁ MOSTRANDO — que não é o mesmo que o modelo tem agora.
        //
        // O motor executa a habilidade INTEIRA e só depois narra (CombateService: `hab.Ativar` roda,
        // aí os eventos são exibidos um a um). Então, entre o Ativar e o evento de dano, o modelo já
        // está com o HP descontado. Publicar nesse meio entregava a vida nova cedo — a barra caía
        // antes do número subir.
        //
        // A tela é um NARRADOR: ela mostra a vida no ponto da narrativa em que está, não a do fim da
        // história. Este dicionário é esse ponto; ele avança quando o evento correspondente é narrado.
        private readonly Dictionary<Combate, (int HP, bool Vivo)> _mostrado = new();

        private (int HP, bool Vivo) Mostrado(Combate c) =>
            _mostrado.TryGetValue(c, out var v) ? v : (Math.Max(0, c.HPAtual), c.EstaVivo());

        /// <summary>Alcança a narrativa ao modelo pra TODOS: a tela passa a mostrar a vida de agora.</summary>
        public void SincronizarVida()
        {
            foreach (Combate c in _ids.Keys) _mostrado[c] = (Math.Max(0, c.HPAtual), c.EstaVivo());
        }

        /// <summary>
        /// Alcança a narrativa só de UM combatente. É o que faz a barra descer no mesmo instante em
        /// que o número dele salta: num golpe em ÁREA, o motor já baixou o HP de todos os alvos no
        /// modelo, mas cada `ExibirResultadoAtaque` narra um por vez — sincronizar só o alvo narrado
        /// segura as outras barras até chegar a vez de cada uma. (Sincronizar todos aqui era o bug do
        /// "a vida de todos desce de uma vez e os números vão pingando".)
        /// </summary>
        public void SincronizarVida(Combate alvo) => _mostrado[alvo] = (Math.Max(0, alvo.HPAtual), alvo.EstaVivo());

        /// <summary>
        /// Manda o retrato completo pra tela redesenhar.
        ///
        /// `sincronizarVida: false` publica TUDO (buffs, status, cooldown, fase) mas mantém a vida no
        /// ponto já narrado — é o que o <see cref="TelaDeCombateWeb.ExibirUsoHabilidade"/> usa pra
        /// anunciar a habilidade sem entregar o dano antes da hora.
        /// </summary>
        public void Publicar(bool sincronizarVida = true)
        {
            if (sincronizarVida) SincronizarVida();

            var todos = _ids.Keys.ToList();
            var estado = new EstadoDeBatalha(
                Turno: _relogio.NumeroDoTurno,
                Fase: Fase,
                Equipe1: todos.Where(c => LadoDe(c) == 1).Select(Ver).ToList(),
                Equipe2: todos.Where(c => LadoDe(c) == 2).Select(Ver).ToList(),
                QuemAge: QuemAge is null ? null : IdDe(QuemAge),
                Habilidades: HabilidadesDoTurno.Select(VerHabilidade).ToList(),
                AlvosValidos: AlvosValidos.Select(IdDe).ToList(),
                Mensagem: Mensagem,
                LadoVencedor: LadoVencedor);

            _ponte.EnviarEstado(estado);
        }

        private CombatenteVisto Ver(Combate c) => new(
            Id: IdDe(c),
            Nome: c.Personagem.Nome,
            Simbolo: c.Personagem.Simbolo,
            // Vida vem do ponto NARRADO (ver _mostrado), não do modelo — o resto é sempre o de agora.
            HPAtual: Mostrado(c).HP,
            HPMaximo: c.HPMaximo,
            Escudo: c.StatusAtivos.OfType<Escudo>().FirstOrDefault()?.PontosRestantes ?? 0,
            Ataque: c.Ataque,
            Defesa: c.Defesa,
            TaxaCritPct: (int)(c.TaxaCrit * 100),
            DanoCritPct: (int)(c.DanoCrit * 100),
            Vivo: Mostrado(c).Vivo,   // idem: quem morreu só "apaga" quando o golpe for narrado
            Status: c.StatusAtivos
                .Select(s => new StatusVisto(s.Nome, s.Simbolo, s.DuracaoRestante, s is Buff))
                .ToList());

        private HabilidadeVista VerHabilidade(HabilidadeAtiva h)
        {
            Combate? dono = QuemAge;
            var cooldown = dono?.Cooldowns.GetValueOrDefault(h);

            return new HabilidadeVista(
                Indice: HabilidadesDoTurno.IndexOf(h),
                Nome: h.Nome,
                Simbolo: h.Simbolo,
                Descricao: h.Descricao,
                CooldownRestante: cooldown?.CooldownRestante ?? 0,
                Disponivel: cooldown?.Disponivel ?? true,
                PedeAlvo: h.PedeAlvoDoJogador,
                Escopo: h.TipoLista.ToString());
        }
    }
}
