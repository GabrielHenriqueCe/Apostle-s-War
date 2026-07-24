using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills;
using ApostlesWar.Domain.Skills.Buffs;

namespace ApostlesWar.App.Front
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

        /// <summary>Manda o retrato completo pra tela redesenhar.</summary>
        public void Publicar()
        {
            var todos = _ids.Keys.ToList();
            var estado = new EstadoDeBatalha(
                Turno: _relogio.NumeroDoTurno,
                Fase: Fase,
                Equipe1: todos.Where(c => LadoDe(c) == 1).Select(Ver).ToList(),
                Equipe2: todos.Where(c => LadoDe(c) == 2).Select(Ver).ToList(),
                QuemAge: QuemAge is null ? null : IdDe(QuemAge),
                Habilidades: HabilidadesDoTurno.Select(VerHabilidade).ToList(),
                AlvosValidos: AlvosValidos.Select(IdDe).ToList(),
                Mensagem: Mensagem);

            _ponte.EnviarEstado(estado);
        }

        private CombatenteVisto Ver(Combate c) => new(
            Id: IdDe(c),
            Nome: c.Personagem.Nome,
            Simbolo: c.Personagem.Simbolo,
            HPAtual: Math.Max(0, c.HPAtual),
            HPMaximo: c.HPMaximo,
            Escudo: c.StatusAtivos.OfType<Escudo>().FirstOrDefault()?.PontosRestantes ?? 0,
            Ataque: c.Ataque,
            Defesa: c.Defesa,
            TaxaCritPct: (int)(c.TaxaCrit * 100),
            DanoCritPct: (int)(c.DanoCrit * 100),
            Vivo: c.EstaVivo(),
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
                Disponivel: cooldown?.Disponivel ?? true);
        }
    }
}
