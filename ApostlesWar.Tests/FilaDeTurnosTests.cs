using ApostlesWar.Domain;

namespace Tests
{
    /// <summary>
    /// A FILA DE TURNOS (GDD-combate §1). Aqui se testa a REGRA da ordem, não o combate: os
    /// bonecos não agem, só ocupam a fila — o que se lê é quem ela entrega e em que sequência.
    ///
    /// O contrato de uso é `Proximo()` → a ação acontece → `Consumir(quem)`. Os testes chamam os
    /// dois na mesma ordem que o <c>CombateService</c>, senão estariam provando outra coisa.
    /// </summary>
    public class FilaDeTurnosTests
    {
        private static Combate Boneco(int velocidade, int casa, string nome = "B") =>
            new Jogador(new Personagem(1, Faccao.Humanos, nome, "🧪", 1000, 100, 0)
                .ComVelocidade(velocidade));

        /// <summary>Monta a batalha e posiciona: o índice na equipe é a casa, igual ao CombateService.</summary>
        private static (Batalha batalha, FilaDeTurnos fila) Campo(List<Combate> time1, List<Combate> time2)
        {
            for (int i = 0; i < time1.Count; i++) time1[i].IniciarCombate(casa: i + 1);
            for (int i = 0; i < time2.Count; i++) time2[i].IniciarCombate(casa: i + 1);

            var batalha = new Batalha(new Equipe(time1), new Equipe(time2));
            return (batalha, new FilaDeTurnos(batalha));
        }

        /// <summary>Roda `quantos` turnos e devolve quantas vezes cada combatente jogou.</summary>
        private static Dictionary<Combate, int> Rodar(FilaDeTurnos fila, int quantos)
        {
            var vezes = new Dictionary<Combate, int>();
            for (int i = 0; i < quantos; i++)
            {
                Combate? daVez = fila.Proximo();
                if (daVez is null) break;
                vezes[daVez] = vezes.GetValueOrDefault(daVez) + 1;
                fila.Consumir(daVez);
            }
            return vezes;
        }

        // ---------- a promessa central ----------

        /// <summary>
        /// "Velocidade é literalmente quantos turnos se joga": a razão de turnos é EXATAMENTE a razão
        /// das Velocidades, e o custo da ação não distorce isso — ele entrega medidor proporcional à
        /// Velocidade de cada um, então o fator sai na divisão.
        /// </summary>
        [Fact]
        public void RazaoDeTurnos_EhARazaoDasVelocidades()
        {
            var rapido = Boneco(velocidade: 200, casa: 1, "rápido");
            var lento = Boneco(velocidade: 100, casa: 1, "lento");
            var (_, fila) = Campo(new() { rapido }, new() { lento });

            var vezes = Rodar(fila, 300);

            // 2 pra 1, com folga de 1 turno pro corte do fim da contagem.
            Assert.InRange(vezes[rapido] / (double)vezes[lento], 1.98, 2.02);
        }

        [Fact]
        public void OPrimeiroTurnoEhDeQuemTemMaisVelocidade()
        {
            var lento = Boneco(velocidade: 100, casa: 1, "lento");
            var rapido = Boneco(velocidade: 300, casa: 1, "rápido");
            var (_, fila) = Campo(new() { lento }, new() { rapido });

            Assert.Same(rapido, fila.Proximo());
        }

        /// <summary>
        /// Com Velocidades iguais o desempate é do lado do jogador — mas por REGRA declarada, e só
        /// DEPOIS da posição. Varrer as equipes na ordem em que estão declaradas daria a abertura à
        /// equipe 1 sempre, e passaria neste teste pelo motivo errado.
        /// </summary>
        [Fact]
        public void EquipesIguais_ODesempateEhAPosicaoEDepoisOLadoDoJogador()
        {
            var meuFrente = Boneco(velocidade: 100, casa: 1, "meu 1");
            var meuFundo = Boneco(velocidade: 100, casa: 2, "meu 2");
            var deleFrente = Boneco(velocidade: 100, casa: 1, "dele 1");
            var deleFundo = Boneco(velocidade: 100, casa: 2, "dele 2");
            var (_, fila) = Campo(new() { meuFrente, meuFundo }, new() { deleFrente, deleFundo });

            // Todos cruzam 100 no mesmo instante: sobra a posição (casa 1 antes da 2) e, dentro da
            // casa, o lado do jogador.
            Assert.Same(meuFrente, fila.Proximo());
            fila.Consumir(meuFrente);
            Assert.Same(deleFrente, fila.Proximo());
        }

        // ---------- a sobra ----------

        /// <summary>
        /// A sobra acima de 100 CARREGA: é o que faz o empurrão de medidor nunca se desperdiçar,
        /// nem quando cai em quem já estava pronto.
        /// </summary>
        [Fact]
        public void ASobraCarrega_EmpurraoEmQuemJaEstaProntoNaoSeDesperdica()
        {
            var empurrado = Boneco(velocidade: 100, casa: 1, "empurrado");
            var outro = Boneco(velocidade: 100, casa: 1, "outro");
            var (_, fila) = Campo(new() { empurrado }, new() { outro });

            fila.Proximo();                        // os dois chegam a 100
            empurrado.AcumularMedidor(80);         // 180: o excedente tem de sobreviver ao turno

            Assert.Same(empurrado, fila.Proximo());
            fila.Consumir(empurrado);

            // 180 − 100 = 80 de sobra (e não zero) + o que ele mesmo encheu durante a própria ação:
            // quem age também vive o tempo que ela custa.
            Assert.Equal(80 + 100 * FilaDeTurnos.CustoDaAcao, empurrado.Medidor, 6);
        }

        /// <summary>
        /// Ninguém passa de 100 por avanço natural — o salto é EXATO até o primeiro cruzamento. Se
        /// isto quebrar, sobra passa a existir do nada e o empurrão deixa de ser a única fonte dela.
        /// </summary>
        [Fact]
        public void AvancoNatural_ParaEmCimaDoLimiar_SemPassarDireto()
        {
            var a = Boneco(velocidade: 137, casa: 1, "a");
            var b = Boneco(velocidade: 61, casa: 1, "b");
            var (_, fila) = Campo(new() { a }, new() { b });

            Combate? daVez = fila.Proximo();

            Assert.Same(a, daVez);
            Assert.Equal(FilaDeTurnos.Limiar, a.Medidor, 6);
            Assert.True(b.Medidor < FilaDeTurnos.Limiar);
        }

        [Fact]
        public void OMaisCheioAgePrimeiro_MesmoQueOOutroSejaMaisRapido()
        {
            var veloz = Boneco(velocidade: 300, casa: 1, "veloz");
            var empurrado = Boneco(velocidade: 100, casa: 1, "empurrado");
            var (_, fila) = Campo(new() { veloz }, new() { empurrado });

            fila.Proximo();                      // o veloz cruza primeiro e chega a 100
            empurrado.AcumularMedidor(140);      // ele estava atrás; o empurrão o joga pra 140+

            // Com "o mais RÁPIDO age" o veloz jogaria em 100 contra 140 — e a barra na tela viraria
            // mentira. A regra é o mais CHEIO.
            Assert.Same(empurrado, fila.Proximo());
        }

        // ---------- morte e bordas ----------

        [Fact]
        public void MortoNaoJoga_ENaoEnche()
        {
            var vivo = Boneco(velocidade: 100, casa: 1, "vivo");
            var morto = Boneco(velocidade: 900, casa: 1, "morto");
            var (_, fila) = Campo(new() { vivo }, new() { morto });

            morto.ReceberDano(99999, NaturezasDano.Ataque);
            Assert.False(morto.EstaVivo());

            Assert.Same(vivo, fila.Proximo());
            Assert.Equal(0, morto.Medidor, 6);   // o relógio andou pro vivo e não encheu o morto
        }

        /// <summary>
        /// O teto de estabilidade do GDD, como TESTE e não como comentário: acima dele o campo gera
        /// mais medidor do que uma ação consome, as barras inflam pra sempre e o trilho de 0 a 100
        /// deixa de significar alguma coisa. Num 4×4 o teto é 12,5%; se a luta virar 5×5, cai pra 10%
        /// e encosta na fração de hoje.
        /// </summary>
        [Fact]
        public void OCustoDaAcaoFicaAbaixoDoTetoDeEstabilidade()
        {
            const int EmCampo = 8;
            Assert.True(FilaDeTurnos.FracaoDoCiclo < 1.0 / EmCampo,
                $"fração {FilaDeTurnos.FracaoDoCiclo} × {EmCampo} em campo estoura o teto");
        }

        /// <summary>
        /// O custo é adimensional: dobrar a escala de Velocidade do jogo inteiro não pode mudar a
        /// ORDEM dos turnos, só a unidade em que o relógio anda.
        /// </summary>
        [Fact]
        public void DobrarAEscalaDeVelocidade_NaoMudaAOrdemDosTurnos()
        {
            var (_, normal) = Campo(
                new() { Boneco(150, 1, "a"), Boneco(90, 2, "b") },
                new() { Boneco(120, 1, "c"), Boneco(200, 2, "d") });
            var (_, dobrada) = Campo(
                new() { Boneco(300, 1, "a"), Boneco(180, 2, "b") },
                new() { Boneco(240, 1, "c"), Boneco(400, 2, "d") });

            var ordemNormal = Sequencia(normal, 40);
            var ordemDobrada = Sequencia(dobrada, 40);

            Assert.Equal(ordemNormal, ordemDobrada);
        }

        // ---------- a previsão (o cordão na tela) ----------

        /// <summary>
        /// O TESTE QUE JUSTIFICA A PREVISÃO EXISTIR: o que ela promete tem de ser exatamente o que a
        /// batalha entrega. Se um dia alguém reimplementar a regra no lado da previsão, é aqui que a
        /// divergência aparece — e não em jogo, com o jogador vendo a fila mentir.
        /// </summary>
        [Fact]
        public void Prever_DizExatamenteAOrdemQueAFilaVaiEntregar()
        {
            var (_, fila) = Campo(
                new() { Boneco(150, 1, "a"), Boneco(90, 2, "b") },
                new() { Boneco(120, 1, "c"), Boneco(203, 2, "d") });

            var previsto = fila.Prever(12).Select(v => v.Quem.Personagem.Nome).ToList();
            var aconteceu = Sequencia(fila, 12);

            Assert.Equal(previsto, aconteceu);
        }

        [Fact]
        public void Prever_NaoMexeNoEstadoDaBatalha()
        {
            var a = Boneco(150, 1, "a");
            var b = Boneco(90, 1, "b");
            var (_, fila) = Campo(new() { a }, new() { b });

            fila.Prever(20);

            Assert.Equal(0, a.Medidor, 6);
            Assert.Equal(0, b.Medidor, 6);
        }

        /// <summary>
        /// `Esperou` é o que faz o cordão dizer ONDE a ordem é frágil: false = os dois jogam na
        /// sequência, sem intervalo; true = o relógio anda até alguém cruzar 100, e é dentro desse
        /// intervalo que um terceiro pode se enfiar com um empurrão.
        /// </summary>
        [Fact]
        public void Prever_MarcaEsperouSoQuandoORelogioPrecisaAndar()
        {
            var lento = Boneco(100, 1, "lento");
            var rapido = Boneco(100, 1, "rápido");
            var (_, fila) = Campo(new() { lento }, new() { rapido });

            var previsto = fila.Prever(3);       // do zero: os dois chegam a 100 no mesmo salto

            Assert.True(previsto[0].Esperou);    // do zero até o 1º cruzamento, o relógio andou
            Assert.False(previsto[1].Esperou);   // o outro já estava pronto: mesmo instante
            Assert.True(previsto[2].Esperou);    // daí em diante volta a haver espera
        }

        private static List<string> Sequencia(FilaDeTurnos fila, int quantos)
        {
            var nomes = new List<string>();
            for (int i = 0; i < quantos; i++)
            {
                Combate? daVez = fila.Proximo();
                if (daVez is null) break;
                nomes.Add(daVez.Personagem.Nome);
                fila.Consumir(daVez);
            }
            return nomes;
        }
    }
}
