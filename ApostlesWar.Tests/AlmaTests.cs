using ApostlesWar.Application.Portas;
using ApostlesWar.Application.Services;
using ApostlesWar.Domain;

namespace Tests
{
    /// <summary>
    /// A alma, a estrela comprada e o teto que ela produz. Ver docs/GDD-progressao.md §A ESTRELA,
    /// §O MATERIAL e §O PEDÁGIO.
    /// </summary>
    public class AlmaTests
    {
        private sealed class RepositorioFake : IRepositorioDeSave
        {
            private readonly Dictionary<string, object?> _dados = new();
            public void Salvar<T>(string chave, T dado) => _dados[chave] = dado;
            public T? Carregar<T>(string chave) => _dados.TryGetValue(chave, out var v) && v is T t ? t : default;
            public void Excluir(string chave) => _dados.Remove(chave);
        }

        private static (PersonagemService personagens, AlmaService alma, ProgressaoService progressao) Montar()
        {
            var repo = new RepositorioFake();
            var personagens = new PersonagemService();
            var alma = new AlmaService(repo);
            return (personagens, alma, new ProgressaoService(personagens, alma, repo));
        }

        // ---------- o teto ----------

        [Theory]
        [InlineData(0, 9)]
        [InlineData(1, 19)]
        [InlineData(2, 29)]
        [InlineData(3, 39)]
        [InlineData(4, 49)]
        [InlineData(5, 59)]
        [InlineData(6, 60)]
        public void TetoPorEstrelas_AsSeisParedes(int estrelas, int teto)
            => Assert.Equal(teto, Progressao.TetoPorEstrelas(estrelas));

        [Fact]
        public void NivelPorXp_ParaNoTetoDaEstrela()
        {
            Assert.Equal(9, Progressao.NivelPorXp(int.MaxValue / 2, Progressao.TetoPorEstrelas(0)));
            Assert.Equal(39, Progressao.NivelPorXp(int.MaxValue / 2, Progressao.TetoPorEstrelas(3)));

            // A sobrecarga SEM teto é a do inimigo, que chega a 428 e não pode travar no 9.
            Assert.Equal(Arquetipos.NivelMaximo, Progressao.NivelPorXp(int.MaxValue / 2));
        }

        /// <summary>
        /// O <see cref="Progressao.Estrelas"/> deixou de DEFINIR a estrela e virou a identidade que a
        /// confere. Este teste é o que protege a <see cref="Arquetipos.Velocidade"/>, que conta o
        /// degrau dela com a mesma expressão: se as duas divergirem, o degrau vira mentira.
        /// </summary>
        [Fact]
        public void Estrelas_ConfereComAsCompradas()
        {
            for (int estrelas = 0; estrelas <= Alma.EstrelaMaxima; estrelas++)
            {
                int piso = estrelas == 0 ? Arquetipos.NivelMinimo : 10 * estrelas;
                for (int nivel = piso; nivel <= Progressao.TetoPorEstrelas(estrelas); nivel++)
                    Assert.Equal(estrelas, Progressao.Estrelas(nivel));
            }
        }

        /// <summary>
        /// A barra do fim de fase, fatiada. Um trecho por nível atravessado, e o do MEIO vai sempre
        /// de 0 a 100 — é o "encher, zerar, encher" que a tela toca em ordem.
        /// </summary>
        [Fact]
        public void Trechos_UmPorNivelAtravessado()
        {
            // do 7 com a faixa pela metade até o 9 com um pedaço andado
            int xpAntes = Progressao.XpParaNivel(7) + (Progressao.XpParaNivel(8) - Progressao.XpParaNivel(7)) / 2;
            int xpDepois = Progressao.XpParaNivel(9) + (Progressao.XpParaNivel(10) - Progressao.XpParaNivel(9)) / 4;

            var trechos = Progressao.Trechos(7, xpAntes, 9, xpDepois);

            Assert.Equal(new[] { 7, 8, 9 }, trechos.Select(t => t.Nivel));
            Assert.Equal(50, trechos[0].De);
            Assert.Equal(100, trechos[0].Ate);   // o nível de partida FECHA
            Assert.Equal(0, trechos[1].De);      // o do meio atravessa inteiro
            Assert.Equal(100, trechos[1].Ate);
            Assert.Equal(0, trechos[2].De);
            Assert.Equal(25, trechos[2].Ate);

            // Sem subir de nível é um trecho só, e ele não fecha.
            var parado = Progressao.Trechos(7, xpAntes, 7, xpAntes + 10);
            Assert.Single(parado);
            Assert.Equal(50, parado[0].De);
            Assert.True(parado[0].Ate >= 50 && parado[0].Ate < 100);
        }

        // ---------- os dois verbos da alma ----------

        [Fact]
        public void Queima_NaoFuraOTeto()
        {
            var (personagens, alma, progressao) = Montar();
            Personagem p = personagens.ObterPersonagem(Faccao.Tecnologicos, Slot.Slot4);

            alma.Creditar(Dificuldade.Pesadelo, 10_000);
            progressao.QueimarAlma(p, Raridade.Mitico, alma.SaldoDe(Raridade.Mitico));

            // XP de sobra pro jogo inteiro e nenhuma estrela: ele para na 1ª parede. É esta
            // propriedade que deixa a queima ser generosa sem furar o teto de dificuldade.
            Assert.Equal(9, p.Nivel);
            Assert.True(progressao.XpDe(p) > Progressao.XpParaNivel(Arquetipos.NivelMaximo));
        }

        [Fact]
        public void Queima_RecusadaNaParede()
        {
            var (personagens, alma, progressao) = Montar();
            Personagem p = personagens.ObterPersonagem(Faccao.Reino, Slot.Slot1);

            alma.Creditar(Dificuldade.Facil, 1_000);
            progressao.QueimarAlma(p, Raridade.Raro, 200);   // 5.000 XP, e o nível 9 custa 3.600

            Assert.True(progressao.NaParede(p));

            int saldoAntes = alma.SaldoDe(Raridade.Raro);
            Assert.Equal(MotivoRecusa.NaParede, progressao.QueimarAlma(p, Raridade.Raro, 20));
            Assert.Equal(saldoAntes, alma.SaldoDe(Raridade.Raro));   // recusou sem cobrar
        }

        /// <summary>
        /// Chegar ao nível 9 NÃO é estar na parede — a parede é a barra ENCHER e o nível não passar.
        /// Sem isto, o jogador compra a estrela com a barra pela metade, o nível não se move (a XP não
        /// está lá) e o pedágio some sem entregar nada.
        /// </summary>
        [Fact]
        public void ComprarEstrela_SoDepoisQueABarraEnche()
        {
            var (personagens, alma, progressao) = Montar();
            Personagem p = personagens.ObterPersonagem(Faccao.Especial, Slot.Slot1);
            alma.Creditar(Dificuldade.Facil, 100);

            // XP que paga o 9 CRAVADO: ele está no topo da dezena, mas a faixa 9→10 está vazia.
            progressao.Creditar(new List<Personagem> { p }, Progressao.XpParaNivel(9));
            Assert.Equal(9, p.Nivel);
            Assert.False(progressao.NaParede(p));
            Assert.Equal(MotivoRecusa.ForaDaParede, progressao.ComprarEstrela(p));

            var (feito, total) = progressao.FaixaDoNivel(p);
            Assert.Equal(0, feito);        // a barra começa vazia, e não cheia
            Assert.True(total > 0);

            // agora a faixa fecha: a barra enche, o nível não passa, e AÍ a compra abre
            progressao.Creditar(new List<Personagem> { p }, Progressao.XpParaNivel(10) - Progressao.XpParaNivel(9));
            Assert.Equal(9, p.Nivel);
            Assert.True(progressao.NaParede(p));
            Assert.Equal(progressao.FaixaDoNivel(p).Total, progressao.FaixaDoNivel(p).Feito);
            Assert.Equal(MotivoRecusa.Nenhum, progressao.ComprarEstrela(p));
            Assert.Equal(10, p.Nivel);
        }

        [Fact]
        public void ComprarEstrela_SoNaParedeESoComOPrecoInteiro()
        {
            var (personagens, alma, progressao) = Montar();
            Personagem p = personagens.ObterPersonagem(Faccao.Misticos, Slot.Slot2);

            Assert.Equal(MotivoRecusa.ForaDaParede, progressao.ComprarEstrela(p));   // nível 1

            progressao.Creditar(new List<Personagem> { p }, Progressao.XpParaNivel(30));
            Assert.Equal(9, p.Nivel);
            Assert.Equal(MotivoRecusa.SemSaldo, progressao.ComprarEstrela(p));       // parede, bolso vazio

            alma.Creditar(Dificuldade.Facil, 100);
            Assert.Equal(MotivoRecusa.Nenhum, progressao.ComprarEstrela(p));
            Assert.Equal(19, p.Nivel);   // a XP guardada entra até a parede seguinte
            Assert.Equal(1, progressao.EstrelasDe(p));
        }

        /// <summary>
        /// O painel do Arsenal manda a mistura inteira de uma vez (uma barrinha por faixa, um botão
        /// de confirmar), e o débito é tudo-ou-nada: cobrar a faixa que cabe e recusar a que não cabe
        /// seria alma sumindo sem XP em troca.
        /// </summary>
        [Fact]
        public void QueimarAlma_VariasFaixasDeUmaVez()
        {
            var (personagens, alma, progressao) = Montar();
            Personagem p = personagens.ObterPersonagem(Faccao.Decaidos, Slot.Slot2);
            alma.Creditar(Dificuldade.Facil, 10);   // 150 Comum, 50 Incomum, 10 Raro

            var demais = new[] { new Custo(Raridade.Comum, 100), new Custo(Raridade.Raro, 999) };
            Assert.Equal(MotivoRecusa.SemSaldo, progressao.QueimarAlma(p, demais));
            Assert.Equal(150, alma.SaldoDe(Raridade.Comum));   // a perna que cabia não foi cobrada
            Assert.Equal(0, progressao.XpDe(p));

            var mistura = new[] { new Custo(Raridade.Comum, 100), new Custo(Raridade.Raro, 10) };
            Assert.Equal(MotivoRecusa.Nenhum, progressao.QueimarAlma(p, mistura));
            Assert.Equal(100 * 1 + 10 * 25, progressao.XpDe(p));
            Assert.Equal(50, alma.SaldoDe(Raridade.Comum));
            Assert.Equal(0, alma.SaldoDe(Raridade.Raro));
        }

        // ---------- a moeda ----------

        [Fact]
        public void Diluir_DesceUmaFaixaEACommumNaoDesce()
        {
            var (_, alma, _) = Montar();
            alma.Creditar(Dificuldade.Pesadelo, 10);   // 150 Épico, 50 Lendário, 10 Mítico

            Assert.True(alma.Diluir(Raridade.Mitico, 10));
            Assert.Equal(0, alma.SaldoDe(Raridade.Mitico));
            Assert.Equal(50 + 50, alma.SaldoDe(Raridade.Lendario));

            Assert.False(alma.Diluir(Raridade.Comum, 1));      // não há faixa abaixo
            Assert.False(alma.Diluir(Raridade.Epico, 10_000)); // sem saldo, não mexe em nada
        }

        /// <summary>
        /// A FUSÃO e a trava dela. Sem o teto, 10.000 Comuns farmados no Fácil viram a alma mítica
        /// que só o Pesadelo paga — e o teto de dificuldade inteiro cai pela porta dos fundos.
        /// </summary>
        [Theory]
        [InlineData(Dificuldade.Facil, Raridade.Raro)]
        [InlineData(Dificuldade.Normal, Raridade.Epico)]
        [InlineData(Dificuldade.Dificil, Raridade.Lendario)]
        [InlineData(Dificuldade.Pesadelo, Raridade.Mitico)]
        public void TetoDeFusao_EAFaixaQueADificuldadeDerruba(Dificuldade dificuldade, Raridade teto)
            => Assert.Equal(teto, Alma.TetoDeFusao(dificuldade));

        [Fact]
        public void Fundir_DezViramUmEOTetoBloqueia()
        {
            var (_, alma, _) = Montar();
            alma.Creditar(Dificuldade.Facil, 10);   // 150 Comum, 50 Incomum, 10 Raro

            Assert.True(alma.Fundir(Raridade.Comum, 150, Dificuldade.Facil));
            Assert.Equal(0, alma.SaldoDe(Raridade.Comum));
            Assert.Equal(50 + 15, alma.SaldoDe(Raridade.Incomum));

            // Raro → Épico é impossível pra quem só abriu o Fácil: Épico não cai lá.
            Assert.False(alma.Fundir(Raridade.Raro, 10, Dificuldade.Facil));
            Assert.Equal(10, alma.SaldoDe(Raridade.Raro));

            // A MESMA fusão passa pra quem abriu o Normal.
            Assert.True(alma.Fundir(Raridade.Raro, 10, Dificuldade.Normal));
            Assert.Equal(1, alma.SaldoDe(Raridade.Epico));
        }

        [Fact]
        public void Fundir_SoCobraOQueFechaGrupoEAMiticaNaoSobe()
        {
            var (_, alma, _) = Montar();
            alma.Creditar(Dificuldade.Pesadelo, 10);   // 150 Épico, 50 Lendário, 10 Mítico

            // 50 fecha 5 grupos exatos, e o Épico logo abaixo prova o resto que não fecha.
            Assert.True(alma.Fundir(Raridade.Lendario, 50, Dificuldade.Pesadelo));
            Assert.Equal(0, alma.SaldoDe(Raridade.Lendario));
            Assert.Equal(10 + 5, alma.SaldoDe(Raridade.Mitico));

            Assert.False(alma.Fundir(Raridade.Mitico, 15, Dificuldade.Pesadelo));   // não há faixa acima
            Assert.False(alma.Fundir(Raridade.Epico, 9, Dificuldade.Pesadelo));     // 9 não fecha grupo
            Assert.Equal(150, alma.SaldoDe(Raridade.Epico));
        }

        [Fact]
        public void Debitar_ETudoOuNada()
        {
            var (_, alma, _) = Montar();
            alma.Creditar(Dificuldade.Facil, 10);   // 150 Comum, 50 Incomum, 10 Raro

            var caro = new[] { new Custo(Raridade.Comum, 100), new Custo(Raridade.Raro, 999) };

            Assert.False(alma.Debitar(caro));
            Assert.Equal(150, alma.SaldoDe(Raridade.Comum));   // a perna que cabia NÃO foi cobrada
            Assert.Equal(new[] { new Custo(Raridade.Raro, 989) }, alma.Faltando(caro));
        }

        /// <summary>
        /// O TETO DE DIFICULDADE, e ele não é regra escrita em lugar nenhum: a estrela que abre a
        /// dezena seguinte cobra uma faixa que aquela dificuldade não derruba. Fácil para no 30,
        /// Normal no 40, Difícil no 50, Pesadelo no 60 (§O TETO DE DIFICULDADE).
        /// </summary>
        [Theory]
        [InlineData(Dificuldade.Facil, 3)]
        [InlineData(Dificuldade.Normal, 4)]
        [InlineData(Dificuldade.Dificil, 5)]
        [InlineData(Dificuldade.Pesadelo, 6)]
        public void Receita_PrendeOTetoDaDificuldade(Dificuldade dificuldade, int ultimaEstrela)
        {
            var cai = Alma.QuedaPorInimigo(dificuldade).Select(c => c.Raridade).ToHashSet();

            Assert.All(Alma.Receita(ultimaEstrela), c =>
                Assert.True(cai.Contains(c.Raridade), $"a {ultimaEstrela}ª estrela tem de ser pagável no {dificuldade}"));

            if (ultimaEstrela < Alma.EstrelaMaxima)
                Assert.Contains(Alma.Receita(ultimaEstrela + 1), c => !cai.Contains(c.Raridade));
        }

        /// <summary>
        /// A PASSADA DO FÁCIL, ponta a ponta, com o time de 4 comprando estrela assim que dá — é o
        /// teste de calibragem: se alguém mexer no <see cref="Alma.CustoDaProxima"/>, na queda por
        /// inimigo ou na composição das fases, o número da parede muda aqui.
        ///
        /// Os 328 inimigos são a soma das duas rodadas das 56 fases; o nível 27 é o mesmo que o
        /// <c>PassadaSuaveDoFacil_ParaNo27</c> afirma, e ele NÃO muda com o teto — as paredes só
        /// atrasam a aplicação da XP, não a produção dela.
        /// </summary>
        [Fact]
        public void PassadaDoFacil_ParaNa3aParedeFaltando72DeRaro()
        {
            var (personagens, alma, progressao) = Montar();
            var time = Enum.GetValues<Slot>()
                .Select(s => personagens.ObterPersonagem(Faccao.Reino, s))
                .ToList();

            int inimigos = 0;
            for (int capitulo = 1; capitulo <= 8; capitulo++)
                for (int fase = 1; fase <= 7; fase++)
                {
                    Fase f = Campanha.ObterFase(fase, Dificuldade.Facil);
                    int mortos = f.Rodada1.Count + f.Rodada2.Count;
                    inimigos += mortos;

                    alma.Creditar(Dificuldade.Facil, mortos);
                    progressao.Creditar(time, Progressao.PoteDaFase(capitulo, fase, Dificuldade.Facil));

                    // o jogador compra a estrela na hora em que a parede aparece
                    foreach (Personagem p in time)
                        while (progressao.ComprarEstrela(p) == MotivoRecusa.Nenhum) { }
                }

            Assert.Equal(328, inimigos);
            Assert.All(time, p => Assert.Equal(27, p.Nivel));
            Assert.All(time, p => Assert.Equal(2, progressao.EstrelasDe(p)));

            // A 3ª parede não chega pela XP: a passada suave morre no 27, e o 29 só sai repetindo.
            Assert.All(time, p => Assert.Equal(MotivoRecusa.ForaDaParede, progressao.ComprarEstrela(p)));

            // E quando ela chegar, falta ALMA: 72 Raro, que é 72 inimigos do Fácil (1 Raro cada).
            var paraOTime = Alma.Receita(3)
                .Select(c => new Custo(c.Raridade, c.Quantidade * time.Count))
                .ToList();

            Assert.Equal(new[] { new Custo(Raridade.Raro, 72) }, alma.Faltando(paraOTime));
        }
    }
}
