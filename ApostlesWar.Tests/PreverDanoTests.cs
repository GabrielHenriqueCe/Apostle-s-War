using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;

namespace Tests
{
    /// <summary>
    /// Testes do PAR previsão/aplicação: <c>Combate.PreverDanoRecebido</c> e
    /// <c>Combate.PreverVidaRemovida</c> — o que o bot usa pra comparar alvos sem desferir o golpe.
    ///
    /// Duas coisas precisam ser verdade, e cada uma tem sua seção:
    /// 1. **O prever CONFERE** — o número tem que ser o mesmo que o ReceberDano de fato aplica.
    ///    Se divergirem, o bot mira errado em silêncio, que é o pior tipo de bug.
    /// 2. **O prever NÃO ACONTECE** — nada é consumido, ninguém é ferido. Esta é a razão de a
    ///    capacidade ter dois métodos: o Escudo gasta pontos e o ProtecaoAliado chega a causar dano
    ///    no protetor, então prever chamando o Modificar machucaria um aliado de verdade.
    ///
    /// Determinismo: tudo entra pelo ReceberDano/PreverDanoRecebido com valor fixo (nada de Atacar,
    /// que rola crítico), então os números são exatos.
    /// </summary>
    public class PreverDanoTests
    {
        private static Combate Novo(int hp = 100_000, int atk = 0, int def = 0)
            => new Jogador(new Personagem(1, Faccao.Humanos, "Teste", "🧪", hp, atk, def));

        // ---------- 1. O prever confere com o aplicar ----------

        [Theory]
        [InlineData(0)]
        [InlineData(300)]
        [InlineData(900)]
        [InlineData(5000)]   // além do teto de redução
        public void Prever_BateComODanoQueOReceberDanoAplica_SoDefesa(int defesa)
        {
            var previsor = Novo(def: defesa);
            var vitima = Novo(def: defesa);

            int previsto = previsor.PreverDanoRecebido(1000, NaturezasDano.Ataque);
            var (efetivo, _) = vitima.ReceberDano(1000, NaturezasDano.Ataque);

            Assert.Equal(efetivo, previsto);
        }

        [Fact]
        public void Prever_BateComOAplicar_ComEscudoParcial()
        {
            var previsor = Novo();
            var vitima = Novo();
            new Escudo(400, duracao: 2).Aplicar(previsor);
            new Escudo(400, duracao: 2).Aplicar(vitima);

            int previsto = previsor.PreverDanoRecebido(1000, NaturezasDano.Ataque);
            var (efetivo, _) = vitima.ReceberDano(1000, NaturezasDano.Ataque);

            Assert.Equal(600, previsto);        // 1000 − 400 de escudo
            Assert.Equal(efetivo, previsto);
        }

        [Fact]
        public void Prever_BateComOAplicar_ComBloqueioTotal()
        {
            var previsor = Novo();
            var vitima = Novo();
            new BloqueioTotal(duracao: 2).Aplicar(previsor);
            new BloqueioTotal(duracao: 2).Aplicar(vitima);

            int previsto = previsor.PreverDanoRecebido(1000, NaturezasDano.Ataque);
            var (efetivo, _) = vitima.ReceberDano(1000, NaturezasDano.Ataque);

            Assert.Equal(0, previsto);
            Assert.Equal(efetivo, previsto);
        }

        [Fact]
        public void Prever_RespeitaOMesmoGateDeIgnorados_QueOAplicar()
        {
            var previsor = Novo();
            var vitima = Novo();
            new Escudo(400, duracao: 2).Aplicar(previsor);
            new Escudo(400, duracao: 2).Aplicar(vitima);

            // Golpe que FURA o escudo: os dois lados têm que enxergar o mesmo furo.
            var fura = new[] { typeof(Escudo) };
            int previsto = previsor.PreverDanoRecebido(1000, NaturezasDano.Ataque, fura);
            var (efetivo, _) = vitima.ReceberDano(1000, NaturezasDano.Ataque, ignorarStatus: fura);

            Assert.Equal(1000, previsto);       // escudo ignorado, dano cheio
            Assert.Equal(efetivo, previsto);
        }

        // ---------- 2. O prever não acontece ----------

        [Fact]
        public void Prever_NaoConsomeOEscudo_NemDepoisDeVariasConsultas()
        {
            var alvo = Novo();
            var escudo = new Escudo(400, duracao: 2);
            escudo.Aplicar(alvo);

            for (int i = 0; i < 5; i++)
                Assert.Equal(600, alvo.PreverDanoRecebido(1000, NaturezasDano.Ataque));

            Assert.Equal(400, escudo.PontosRestantes);              // intacto
            Assert.Contains(escudo, alvo.StatusAtivos);             // não se removeu
            Assert.Equal(100_000, alvo.HPAtual);                    // e ninguém apanhou
        }

        /// <summary>
        /// O caso que mais justifica a capacidade ter dois métodos: o ProtecaoAliado REDIRECIONA
        /// parte do dano pro aplicador chamando `ReceberDano` nele. Se o prever passasse pelo
        /// caminho de aplicação, consultar uma jogada feriria o aliado protetor.
        /// </summary>
        [Fact]
        public void Prever_NaoFereOAplicadorDaProtecao()
        {
            var protetor = Novo();
            var protegido = Novo();
            new ProtecaoAliado(protetor, duracao: 2, percentual: 0.5).Aplicar(protegido);

            int previsto = protegido.PreverDanoRecebido(1000, NaturezasDano.Ataque);

            Assert.Equal(500, previsto);                 // metade passa pro protegido
            Assert.Equal(100_000, protetor.HPAtual);     // e o protetor NÃO levou os outros 500
            Assert.Equal(100_000, protegido.HPAtual);
        }

        // ---------- 3. PreverVidaRemovida: dano vira "quanto de vida sai" ----------

        [Fact]
        public void VidaRemovida_ECapadaPeloHPAtual_ESinalizaOAbate()
        {
            var alvo = Novo(hp: 300);

            Assert.Equal(300, alvo.PreverVidaRemovida(1000));   // não tira mais vida do que existe
            Assert.Equal(alvo.HPAtual, alvo.PreverVidaRemovida(1000));   // == HPAtual: MATA
            Assert.Equal(120, alvo.PreverVidaRemovida(120));    // golpe menor: sai o que bate
        }

        /// <summary>
        /// Invencível não reduz dano (de propósito — ver o fix do bug do lifesteal), ele põe um PISO
        /// de HP. Então quem mede "vida removida" o enxerga sozinho: sobra HPAtual − 1. É o que faz
        /// "evitar alvo invencível" não precisar de regra nenhuma no bot.
        /// </summary>
        [Fact]
        public void VidaRemovida_ParaNoPisoDeHP_DoInvencivel()
        {
            var alvo = Novo(hp: 300);
            new Invencivel(duracao: 2).Aplicar(alvo);

            Assert.Equal(299, alvo.PreverVidaRemovida(1000));   // 300 − piso 1
            Assert.NotEqual(alvo.HPAtual, alvo.PreverVidaRemovida(1000));   // logo, NÃO mata
        }

        /// <summary>
        /// Bloqueio total zera na PREVISÃO do dano (o modificador), não no cap de vida — as duas
        /// pontas juntas dão o que o bot compara: um alvo bloqueado vale 0.
        /// </summary>
        [Fact]
        public void VidaRemovida_DeAlvoBloqueado_EZero()
        {
            var alvo = Novo(hp: 300);
            new BloqueioTotal(duracao: 2).Aplicar(alvo);

            int passaria = alvo.PreverDanoRecebido(1000, NaturezasDano.Ataque);

            Assert.Equal(0, alvo.PreverVidaRemovida(passaria));
        }

        // ---------- 4. PreverAtaque: o lado do atacante ----------

        [Fact]
        public void PreverAtaque_NaoDesfereOGolpe()
        {
            var atacante = Novo(atk: 1000);
            var alvo = Novo(hp: 5000);

            int previsto = atacante.PreverAtaque(alvo, 1.0);

            Assert.True(previsto > 0);
            Assert.Equal(5000, alvo.HPAtual);       // continua inteiro
            Assert.Equal(0, atacante.DanoCausado);  // e nada foi contabilizado
        }

        /// <summary>
        /// O crítico entra como VALOR ESPERADO, não como sorteio — senão o bot não conseguiria
        /// comparar uma habilidade que crita sempre (Kunai) com uma que não crita.
        /// </summary>
        [Fact]
        public void PreverAtaque_ForcaCritico_PreveMaisQueOAtaqueNormal()
        {
            var atacante = Novo(atk: 1000);
            var alvo = Novo(hp: 100_000);

            int normal = atacante.PreverAtaque(alvo, 1.0);
            int critico = atacante.PreverAtaque(alvo, 1.0, forcaCritico: true);

            Assert.True(critico > normal);
        }

        [Fact]
        public void PreverAtaque_EDeterministico_MesmaEntradaMesmoNumero()
        {
            var atacante = Novo(atk: 1000);
            var alvo = Novo(hp: 100_000, def: 400);

            int primeira = atacante.PreverAtaque(alvo, 1.5);

            for (int i = 0; i < 10; i++)
                Assert.Equal(primeira, atacante.PreverAtaque(alvo, 1.5));
        }
    }
}
