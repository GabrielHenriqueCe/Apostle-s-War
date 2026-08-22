using ApostlesWar.Application.Controllers;
using ApostlesWar.Application.Portas;
using ApostlesWar.Application.Services;
using ApostlesWar.Domain;
using ApostlesWar.Domain.Apostolos.Especial;
using ApostlesWar.Domain.Apostolos.LadoSombrio;
using ApostlesWar.Domain.Apostolos.Reino;
using ApostlesWar.Domain.Skills.Ativas;
using Tests.Bancada;
using Xunit;

namespace Tests
{
    /// <summary>
    /// A ORDEM CRÍTICA DE MORTE, ponta a ponta pelo FLUXO — a FILA A #14 do ROADMAP.
    ///
    /// prevent-death (<see cref="GuardaReal"/>) → <c>IReageAoMatar</c> (<see cref="Sentenca"/>) →
    /// <c>IReageAoMorrer</c> (<see cref="Necromancia"/>). <b>Trocar essa ordem não quebra o build:
    /// muda quem vive</b>, e é exatamente por isso que ela precisava de rede.
    ///
    /// <b>Por que pelo FLUXO e não por unidade.</b> Cada uma das três já funciona sozinha — o que
    /// não existia era prova de que o `CombateService` as CHAMA nesta ordem. As três chamadas moram
    /// em métodos privados (`ProcessarReacoesAtacanteMorte`, `ProcessarReacoesAoMorrer`), e a única
    /// porta pra elas é uma batalha de verdade. O que destravou isso foi o <see cref="ITelaDeCombate"/>
    /// ser injetável: a tela daqui não desenha nada e ESCUTA — as mensagens de passiva chegam nela
    /// em ordem, e a ordem da lista é a ordem das reações.
    ///
    /// <b>A batalha é determinística por construção</b>, e sem semear Random nenhum: o algoz bate
    /// muito acima do HP da vítima (o crítico não muda o desfecho), a vítima só espera, e o
    /// controlador fecha o horizonte em UM golpe — o `null` vira `BatalhaAbortada`, que é o mesmo
    /// truque da bancada de dano.
    /// </summary>
    public class OrdemDeMorteTests
    {
        private const int HpDoAlgoz = 9_999;
        private const int AtaqueDoAlgoz = 5_000;
        private const int HpDaVitima = 100;

        /// <summary>
        /// A tela que não desenha e ESCUTA. É ela a peça que a FILA A #14 esperava: o motor chama a
        /// tela a cada batida, então uma tela no-op deixa o combate rodar headless — e guardar as
        /// mensagens de passiva transforma o no-op em instrumento, porque cada reação da cadeia de
        /// morte anuncia a si mesma.
        /// </summary>
        private sealed class TelaQueEscuta : ITelaDeCombate
        {
            public List<string> Passivas { get; } = new();

            public void ExibirMensagemPassiva(string mensagem) => Passivas.Add(mensagem);

            /// <summary>Em que posição da narração esta frase apareceu; -1 = não apareceu.</summary>
            public int Onde(string trecho) => Passivas.FindIndex(m => m.Contains(trecho));

            public void LimparTela() { }
            public void ExibirPartida(List<Combate> jogadores, List<Combate> inimigos) { }
            public void ExibirFilaDeTurnos(IReadOnlyList<FilaDeTurnos.Vez> fila) { }
            public void ExibirInicioArena(List<Combate> equipe1, List<Combate> equipe2) { }
            public void ExibirResultadoAtaque(Combate atacante, Combate alvo, EventoDano r) { }
            public void ExibirDanoDeStatus(EventoDano r) { }
            public void ExibirCura(EventoCura c) { }
            public void ExibirPreparacaoAtaque(Combate atacante, List<Combate> defensores) { }
            public void ExibirUsoHabilidade(Combate atacante, Habilidade hab) { }
            public void ExibirResumoBatalha(List<Combate> jogador) { }
            public void ExibirResumoArena(List<Combate> e1, List<Combate> e2, bool venceuEquipe1) { }
            public bool ConfirmarEncerramento() => false;
        }

        /// <summary>
        /// O roteiro: o ALGOZ bate <paramref name="golpes"/> vezes e depois encerra; todo o resto
        /// espera pra sempre.
        ///
        /// Fechar o horizonte é o que torna a asserção possível — a batalha continua depois do golpe
        /// (a vítima revivida morre de novo no golpe seguinte, e aí a Sentença finalmente dispara),
        /// e uma narração de batalha inteira não distingue "não bloqueou" de "bloqueou na segunda".
        /// O `null` no <see cref="IControladorDeTurno"/> significa ENCERRAR, e o `BatalhaAbortada`
        /// que ele levanta já é caminho do motor.
        /// </summary>
        private sealed class RoteiroDeUmGolpe : IControladorDeTurno
        {
            private readonly string _algoz;
            private readonly HabilidadeAtiva _espera;
            private int _restantes;

            public RoteiroDeUmGolpe(string algoz, HabilidadeAtiva espera, int golpes = 1)
            {
                _algoz = algoz;
                _espera = espera;
                _restantes = golpes;
            }

            public HabilidadeAtiva? EscolherAcao(Combate atacante, List<Combate> aliados, List<Combate> defensores)
            {
                if (atacante.Personagem.Nome != _algoz) return _espera;
                if (_restantes-- <= 0) return null;   // horizonte fechado → BatalhaAbortada
                return atacante.Personagem.Habilidades.OfType<AtaqueBasico>().First();
            }

            /// <summary>Um alvo só nos dois lados: a lista tem exatamente um elemento.</summary>
            public Combate? EscolherAlvo(List<Combate> disponiveis, List<Combate> aliados, List<Combate> defensores)
                => disponiveis[0];
        }

        // ---------- as três provas ----------

        /// <summary>
        /// O ELO 2 ANTES DO 3: quem mata com a Sentença carimba o morto ANTES de a Necromância
        /// tentar, e o revive é negado.
        ///
        /// A prova é dupla, e as duas metades importam: as mensagens saem NA ORDEM (Sentença antes),
        /// e o desfecho é o do bloqueio. Só a ordem não bastaria — as duas poderiam sair em ordem e
        /// o revive acontecer mesmo assim.
        /// </summary>
        [Fact]
        public void VilaoMataAntesDaNecromancia_OReviveEBloqueado()
        {
            var (tela, vitima) = Lutar(algozTemSentenca: true, vitimaTemGuarda: false);

            int sentenca = tela.Onde("Sentença:");
            int necromancia = tela.Onde("não pode ser ressuscitado");

            Assert.True(sentenca >= 0, "a Sentença do Vilão não disparou");
            Assert.True(necromancia > sentenca,
                $"a Necromância tem de reagir DEPOIS da Sentença (sentença={sentenca}, necromancia={necromancia})");
            Assert.Equal(-1, tela.Onde("ressuscitado pela Necromancia"));
            Assert.False(vitima.EstaVivo());
        }

        /// <summary>
        /// O CONTROLE do teste acima: sem a Sentença no algoz, a MESMA vítima revive. Sem isto, um
        /// harness quebrado (a Necromância nunca chamada, por exemplo) daria verde lá em cima
        /// afirmando um bloqueio que nunca houve.
        /// </summary>
        [Fact]
        public void SemSentenca_ANecromanciaRevive()
        {
            var (tela, vitima) = Lutar(algozTemSentenca: false, vitimaTemGuarda: false);

            Assert.True(tela.Onde("ressuscitado pela Necromancia") >= 0,
                "sem Sentença, a Necromância tinha de reviver");
            Assert.True(vitima.EstaVivo());
            Assert.Equal(HpDaVitima / 2, vitima.HPAtual);
        }

        /// <summary>
        /// O ELO 1 ANTES DE TUDO: o Guarda EVITA a morte, então nem a Sentença nem a Necromância
        /// enxergam alguma. As duas checam `EstaVivo()` e voltam caladas.
        ///
        /// E o que separa "evitou" de "reviveu" é o <see cref="Invencivel"/> ainda no lugar: o
        /// prevent-death mantém os STATUS, e um revive (o caminho errado, que este teste tranca)
        /// entregaria um Vivo novo e limpo.
        /// </summary>
        [Fact]
        public void OGuardaEvitaAMorte_ENinguemMaisReage()
        {
            var (tela, vitima) = Lutar(algozTemSentenca: true, vitimaTemGuarda: true);

            Assert.True(vitima.EstaVivo());
            Assert.Equal(1, vitima.HPAtual);
            Assert.Contains(vitima.StatusAtivos, s => s.Nome.Contains("Invencível"));

            Assert.Equal(-1, tela.Onde("Sentença:"));
            Assert.Equal(-1, tela.Onde("ressuscitado"));
        }

        // ---------- a montagem ----------

        /// <summary>
        /// Um golpe fatal, e devolve o que a tela ouviu junto com a vítima pra inspecionar. O
        /// `ExecutarArenaComTimes` constrói os <see cref="Combate"/> por dentro, então quem dá
        /// endereço à vítima é a lista da equipe 2 — a mesma porta que a bancada de dano usa.
        /// </summary>
        private static (TelaQueEscuta Tela, Combate Vitima) Lutar(bool algozTemSentenca, bool vitimaTemGuarda)
        {
            var espera = Espera.Nova();

            var doAlgoz = new List<Habilidade> { new AtaqueBasico(), espera };
            if (algozTemSentenca) doAlgoz.Add(new Sentenca());

            var daVitima = new List<Habilidade> { espera, new Necromancia() };
            if (vitimaTemGuarda) daVitima.Add(new GuardaReal());

            var algoz = new Personagem(1, Faccao.Especial, "Algoz", "🦹",
                HpDoAlgoz, AtaqueDoAlgoz, def: 0, doAlgoz.ToArray());
            var vitima = new Personagem(1, Faccao.LadoSombrio, "Vítima", "🪦",
                HpDaVitima, 0, def: 0, daVitima.ToArray());

            // A lista da equipe 2 é a referência viva da vítima: `ExibirInicioArena` recebe os dois
            // times já como Combate, e é o único jeito de alcançá-los sem mexer no motor.
            Combate? capturada = null;
            var tela = new TelaQueEscuta();
            var roteiro = new RoteiroDeUmGolpe("Algoz", espera);
            var combate = Montar(new TelaQueEscutaECaptura(tela, e2 => capturada = e2[0]), roteiro);

            combate.ExecutarArenaComTimes(
                new List<Personagem> { algoz }, new List<Personagem> { vitima },
                bot1: false, bot2: true);

            return (tela, capturada!);
        }

        /// <summary>
        /// A mesma escuta, mais o gancho do `ExibirInicioArena`. Envolve em vez de herdar porque a
        /// <see cref="TelaQueEscuta"/> é o contrato mínimo que os testes leem — a captura é detalhe
        /// da montagem, não da leitura.
        /// </summary>
        private sealed class TelaQueEscutaECaptura : ITelaDeCombate
        {
            private readonly TelaQueEscuta _dentro;
            private readonly Action<List<Combate>> _aoIniciar;

            public TelaQueEscutaECaptura(TelaQueEscuta dentro, Action<List<Combate>> aoIniciar)
            {
                _dentro = dentro;
                _aoIniciar = aoIniciar;
            }

            public void ExibirInicioArena(List<Combate> equipe1, List<Combate> equipe2) => _aoIniciar(equipe2);
            public void ExibirMensagemPassiva(string mensagem) => _dentro.ExibirMensagemPassiva(mensagem);

            public void LimparTela() { }
            public void ExibirPartida(List<Combate> jogadores, List<Combate> inimigos) { }
            public void ExibirFilaDeTurnos(IReadOnlyList<FilaDeTurnos.Vez> fila) { }
            public void ExibirResultadoAtaque(Combate atacante, Combate alvo, EventoDano r) { }
            public void ExibirDanoDeStatus(EventoDano r) { }
            public void ExibirCura(EventoCura c) { }
            public void ExibirPreparacaoAtaque(Combate atacante, List<Combate> defensores) { }
            public void ExibirUsoHabilidade(Combate atacante, Habilidade hab) { }
            public void ExibirResumoBatalha(List<Combate> jogador) { }
            public void ExibirResumoArena(List<Combate> e1, List<Combate> e2, bool venceuEquipe1) { }
            public bool ConfirmarEncerramento() => false;
        }

        /// <summary>
        /// O `CombateService` com as portas fechadas: a tela que escuta, o roteiro nos DOIS slots de
        /// controlador (o algoz e a vítima saem do mesmo roteiro, que separa os dois pelo nome) e a
        /// espera zerada. Os services de save recebem um repositório em memória — nada deste teste
        /// toca disco.
        /// </summary>
        private static CombateService Montar(ITelaDeCombate tela, IControladorDeTurno roteiro)
        {
            var repo = new SaveEmMemoria();
            var personagens = new PersonagemService();
            var capitulos = new CapitulosService(repo);

            return new CombateService(
                new ArsenalService(capitulos, new PoService(repo), new PersonagemService(), repo),
                new ApostolosService(personagens, capitulos),
                personagens,
                new ProgressaoService(personagens, new AlmaService(repo), repo),
                new AlmaService(repo),
                tela,
                new SelecaoDeAlvoService(),
                controladorJogador: roteiro,
                controladorBot: roteiro,
                new SemEspera(),
                new RelogioDoCombate());
        }
    }
}
