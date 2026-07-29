using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills;
using ApostlesWar.Domain.Skills.Buffs;

namespace Tests.Bancada
{
    /// <summary>
    /// A tela da bancada: **no-op como desenho, instrumento como escuta**.
    ///
    /// Ela não desenha nada, mas o contrato do <see cref="ITelaDeCombate"/> já entrega tudo o que a
    /// medição precisa — e por dois motivos que valem registrar:
    ///
    /// 1. <c>ExibirInicioArena</c> recebe os DOIS times **já como <see cref="Combate"/>** (o
    ///    `ExecutarArenaComTimes` os constrói por dentro, então esta é a única forma de alcançá-los
    ///    sem mexer no motor). É por aqui que a bancada pega a referência do boneco pra ler os
    ///    números no fim, e onde ela crava o crítico em 100%.
    /// 2. O funil separa sozinho os dois tipos de dano que o relatório precisa distinguir:
    ///    <c>ExibirResultadoAtaque</c> = golpe direto (atribuível à habilidade que estava em
    ///    execução) e <c>ExibirDanoDeStatus</c> = tick de veneno/queima (que cai em turnos
    ///    POSTERIORES ao da habilidade que o aplicou, então não é atribuível sem inventar
    ///    proveniência — vira linha própria).
    /// </summary>
    internal sealed class TelaDeBancada : ITelaDeCombate
    {
        private readonly bool _critMaximo;

        public TelaDeBancada(bool critMaximo) => _critMaximo = critMaximo;

        public List<Combate> Champs { get; private set; } = new();
        public List<Combate> Bonecos { get; private set; } = new();

        /// <summary>Setada pelo controlador ANTES de devolver a habilidade — é o que dá endereço
        /// ao dano direto que chega logo em seguida.</summary>
        public Habilidade? HabEmExecucao { get; set; }

        public Dictionary<Habilidade, int> DanoPorHab { get; } = new();
        public Dictionary<Habilidade, int> CuraPorHab { get; } = new();
        public int DanoDeTick { get; private set; }

        public void ExibirInicioArena(List<Combate> equipe1, List<Combate> equipe2)
        {
            Champs = equipe1;
            Bonecos = equipe2;

            // Crítico 100% pra todo mundo. Duas razões: quem FORÇA crítico (Kunai) para de ganhar
            // vantagem artificial na comparação, e o RNG do crítico MORRE (`NextDouble() < 1.0` é
            // sempre verdade) — sem precisar semear o Random.Shared, que não é semeável.
            // Vai por status porque o getter TaxaCrit só soma IContribuiTaxaCrit de StatusAtivos, e
            // TaxaCritBase vem do Personagem, que não aceita crit no construtor.
            if (!_critMaximo) return;
            foreach (Combate c in equipe1)
                new BuffTaxaCrit(duracao: StatusEffect.Permanente, valor: 1.0).Aplicar(c);
        }

        public void ExibirResultadoAtaque(Combate atacante, Combate alvo, EventoDano r)
        {
            if (HabEmExecucao is null || !Bonecos.Contains(alvo)) return;
            DanoPorHab[HabEmExecucao] = DanoPorHab.GetValueOrDefault(HabEmExecucao) + r.DanoEfetivo;
        }

        public void ExibirDanoDeStatus(EventoDano r)
        {
            if (Bonecos.Contains(r.Alvo)) DanoDeTick += r.DanoEfetivo;
        }

        /// <summary>
        /// Cura CREDITADA A QUEM CUROU — e o filtro por curador não é detalhe: o boneco se cura todo
        /// turno pra não revidar, então contar por ALVO somaria o descanso dele em cima da habilidade
        /// que estava em execução.
        /// </summary>
        public void ExibirCura(EventoCura c)
        {
            if (HabEmExecucao is null || !Champs.Contains(c.Curador)) return;
            CuraPorHab[HabEmExecucao] = CuraPorHab.GetValueOrDefault(HabEmExecucao) + c.Quantidade;
        }

        public void LimparTela() { }
        public void ExibirPartida(List<Combate> jogadores, List<Combate> inimigos) { }
        public void ExibirMensagemPassiva(string mensagem) { }
        public void ExibirPreparacaoAtaque(Combate atacante, List<Combate> defensores) { }
        public void ExibirUsoHabilidade(Combate atacante, Habilidade hab) { }
        public void ExibirResumoBatalha(List<Combate> jogador) { }
        public void ExibirResumoArena(List<Combate> e1, List<Combate> e2, bool venceuEquipe1) { }

        /// <summary>Nunca chamado: só roda se a espera pedir pra sair, e a da bancada nunca pede.</summary>
        public bool ConfirmarEncerramento() => false;
    }

    /// <summary>Bancada não tem plateia: nenhuma batida respira, e ninguém pede pra sair.</summary>
    internal sealed class SemEspera : IApresentacao
    {
        public bool AguardarAnimacao(Momento momento) => false;
    }

    /// <summary>
    /// O boneco não recebe malefício nenhum. É o MESMO mecanismo do jogo (a `CascaDura` da Abóbora e
    /// a `PeleDeDragao` fazem exatamente isto), então a imunidade da bancada não é um caminho
    /// paralelo: é capacidade declarada, viajando dentro de `Habilidades` como qualquer passiva.
    /// </summary>
    internal sealed class ImuneAMaleficios : HabilidadePassiva, IBloqueiaStatus
    {
        public ImuneAMaleficios() : base("Imune (bancada)", "🛡️", 0, "O boneco não recebe malefícios.") { }

        public bool Bloqueia(StatusEffect novo) => novo is Debuff;
    }

    /// <summary>
    /// O boneco não morre: em vez disso volta ao HP CHEIO. É o mecanismo do **Guarda Real**
    /// (<see cref="IPrevineMorte"/>, consultado pelo `ConfirmarMorte` dentro do funil de dano) com
    /// duas mudanças — restaura tudo em vez de 1 de HP, e **cooldown 0**, que o `SkillCooldown`
    /// traduz em "sempre disponível" (`Usar()` faz `restante = total = 0`), inclusive ENTRE os hits
    /// de uma mesma ativação.
    ///
    /// É o que o reset entre turnos não alcançava: o Porradeiro do Troll dá 6 hits de 480 = 2880
    /// contra um boneco de 2000, matava no 5º, e a corrida parava com **1 uso em vez de 25**.
    ///
    /// E é melhor que pôr um piso de HP (`Invencivel`): com piso, o alvo ficaria em 1 de vida e o
    /// próprio bot documenta que "evitar Invencível cai sozinho de `PreverVidaRemovida`, que devolve
    /// ~0" — ele leria o boneco como inútil de bater e escolheria com a régua errada. Voltando ao HP
    /// cheio, a previsão do bot continua honesta, então isto vale pras CINCO linhas, uniforme.
    /// (Ideia do Gabriel.)
    /// </summary>
    internal sealed class NuncaMorre : HabilidadePassiva, IPrevineMorte
    {
        public NuncaMorre() : base("Nunca morre (bancada)", "♾️", cooldown: 0,
            "O boneco volta ao HP cheio em vez de morrer.")
        { }

        public void Prevenir(Combate combatente) => combatente.RestaurarVida(combatente.HPMaximo);
    }

    /// <summary>
    /// Guarda o turno enquanto o cooldown roda. Existe porque o contrato do
    /// <see cref="IControladorDeTurno"/> não tem "passar": devolver null ali significa ENCERRAR a
    /// batalha. E o buraco não pode ser preenchido com A1 — se fosse, toda habilidade carregaria ~75
    /// ataques básicos junto, o dano do A1 dominaria a soma e todas ficariam parecidas.
    /// Sem ações, mirando o próprio time: o turno passa e nada acontece, que é o ponto.
    /// </summary>
    internal static class Espera
    {
        public static HabilidadeAtiva Nova() => new(
            "Esperar", "⏳", cooldown: 0, "A bancada segura o turno enquanto o cooldown roda.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, acoes: new List<Acao>());

        /// <summary>
        /// O turno do BONECO: ele se cura em vez de revidar (ideia do Gabriel). Podia ser a casca
        /// vazia acima — o efeito na medição é o mesmo, já que o que importa é ele não ENCOSTAR no
        /// champ — mas uma ação de jogo de verdade é mais honesta que um turno oco, e de quebra
        /// devolve o alvo ao HP cheio pelo caminho do próprio motor.
        /// </summary>
        public static HabilidadeAtiva Descanso() => new(
            "Descansar", "💤", cooldown: 0, "O boneco se recompõe em vez de revidar.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new List<Acao> { new Cura(Valor.PorHP(1.0), Escopo.ProprioAtacante) });
    }

    /// <summary>
    /// Quem joga pelo champ na bancada, nos dois modos:
    /// - **isolada** (`habIsolada` != null): usa SÓ aquela habilidade, e espera quando ela está em
    ///   cooldown. Mede a habilidade.
    /// - **champ inteiro** (`habIsolada` == null): delega ao <c>ControladorBot</c>, o MESMO cérebro
    ///   que joga a Arena e o modo Auto. Mede o champ como ele é de fato jogado — e é por isso que
    ///   a contagem de usos por habilidade importa: se uma nunca é escolhida, isso é um fato sobre
    ///   a FILA DO BOT, não sobre o dano dela.
    ///
    /// O horizonte de N turnos sai sem tocar no motor: na chamada N+1 devolve null, e o
    /// `BatalhaAbortada` que já existe encerra a partida.
    /// </summary>
    internal sealed class ControladorDeBancada : IControladorDeTurno
    {
        private readonly TelaDeBancada _tela;
        private readonly IControladorDeTurno _bot;
        private readonly HabilidadeAtiva? _habIsolada;
        private readonly HabilidadeAtiva _espera;
        private readonly int _turnos;
        private int _decisoes;

        public ControladorDeBancada(TelaDeBancada tela, IControladorDeTurno bot,
            HabilidadeAtiva? habIsolada, HabilidadeAtiva espera, int turnos)
        {
            _tela = tela;
            _bot = bot;
            _habIsolada = habIsolada;
            _espera = espera;
            _turnos = turnos;
        }

        public Dictionary<Habilidade, int> Usos { get; } = new();
        public int TurnosEsperando { get; private set; }

        public HabilidadeAtiva? EscolherAcao(Combate atacante, List<Combate> aliados, List<Combate> defensores)
        {
            if (_decisoes >= _turnos) return null;   // horizonte fechado → BatalhaAbortada
            _decisoes++;

            // O boneco volta ao HP cheio antes de cada golpe. Isto NÃO é conveniência: o jogo tem
            // efeitos percentuais sobre o HP máximo (a Queima tira 5% dele por turno), então um
            // boneco com HP inflado pra "aguentar 100 turnos" faria o DoT explodir e ele se mataria.
            // Com HP realista + reset, o percentual mede o que mede em jogo, o alvo nunca morre, e o
            // `DanoRecebido` — que é o funil único — segue somando tudo.
            // Reset AQUI (entre turnos, antes de decidir) e não via Invencível de propósito: o piso
            // de HP deixaria o alvo em 1 de vida, e aí o `PreverVidaRemovida` devolveria ~0 pra tudo
            // e o BOT escolheria as habilidades com a régua errada na medição do champ inteiro.
            foreach (Combate boneco in _tela.Bonecos)
                boneco.RestaurarVida(boneco.HPMaximo);

            // E o CHAMP começa cada turno com 1 de vida. Duas razões, e a segunda foi o Gabriel quem
            // viu: (a) cura só cura quem está ferido — sem isto, toda habilidade de cura mediria 0 e
            // a coluna não existiria; (b) há champ que fica MAIS FORTE com pouca vida (a Caveira
            // escala `2.0 − HP%`), então esta é a condição em que o kit dele aparece.
            // O champ não morre disto: ele carrega a mesma prevenção-de-morte do boneco, que o
            // segura quando uma habilidade de auto-dano (o Fantasma) o levaria a zero.
            atacante.RestaurarVida(1);

            HabilidadeAtiva escolhida;
            if (_habIsolada is null)
            {
                escolhida = _bot.EscolherAcao(atacante, aliados, defensores)!;
            }
            else if (atacante.Cooldowns[_habIsolada].Disponivel)
            {
                escolhida = _habIsolada;
            }
            else
            {
                escolhida = _espera;
                TurnosEsperando++;
            }

            if (!ReferenceEquals(escolhida, _espera))
                Usos[escolhida] = Usos.GetValueOrDefault(escolhida) + 1;

            _tela.HabEmExecucao = escolhida;
            return escolhida;
        }

        public Combate? EscolherAlvo(List<Combate> disponiveis, List<Combate> aliados, List<Combate> defensores)
            => _habIsolada is null
                ? _bot.EscolherAlvo(disponiveis, aliados, defensores)
                : disponiveis.FirstOrDefault() ?? defensores.FirstOrDefault();
    }

    /// <summary>
    /// O controlador do BONECO: ele nunca age. Não basta dar-lhe ataque 0 — o golpe de dano zero
    /// AINDA dispara `IReageAoSerAtacado`, e aí a bancada mede a passiva reagindo ao próprio andaime
    /// em vez da habilidade. Foi o que inflou o Troll: a Ambição dá +5% de ATK por hit recebido até
    /// +25%, então ele terminava a corrida batendo 25% mais forte porque o saco de pancada balançava.
    /// </summary>
    internal sealed class ControladorQueEspera : IControladorDeTurno
    {
        private readonly HabilidadeAtiva _espera;

        public ControladorQueEspera(HabilidadeAtiva espera) => _espera = espera;

        public HabilidadeAtiva? EscolherAcao(Combate atacante, List<Combate> aliados, List<Combate> defensores)
            => _espera;

        public Combate? EscolherAlvo(List<Combate> disponiveis, List<Combate> aliados, List<Combate> defensores)
            => disponiveis.FirstOrDefault() ?? aliados.FirstOrDefault();
    }

    /// <summary>Save que não sai da memória — o `ArsenalService` exige a porta, a bancada não usa itens.</summary>
    internal sealed class SaveEmMemoria : IRepositorioDeSave
    {
        private readonly Dictionary<string, object?> _dados = new();

        public void Salvar<T>(string chave, T dado) => _dados[chave] = dado;
        public T? Carregar<T>(string chave) => _dados.TryGetValue(chave, out var d) && d is T t ? t : default;
        public void Excluir(string chave) => _dados.Remove(chave);
    }
}
