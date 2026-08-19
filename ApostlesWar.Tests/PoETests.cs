using ApostlesWar.Application.Portas;
using ApostlesWar.Application.Services;
using ApostlesWar.Domain;
using Xunit;

namespace Tests
{
    /// <summary>
    /// O eixo do NÍVEL DO ITEM: a curva de custo, a estrela que destrava a dezena e o pedágio pago em
    /// pó. É o irmão do <see cref="AlmaTests"/>, e os números vêm do docs/GDD-itens.md §Como o nível
    /// sobe, calibrados contra a medição de ciclos de ago/2026.
    /// </summary>
    public class PoETests
    {
        private sealed class RepositorioFake : IRepositorioDeSave
        {
            private readonly Dictionary<string, object?> _dados = new();
            public void Salvar<T>(string chave, T dado) => _dados[chave] = dado;
            public T? Carregar<T>(string chave) => _dados.TryGetValue(chave, out var d) && d is T t ? t : default;
            public void Excluir(string chave) => _dados.Remove(chave);
            public bool Contem(string chave) => _dados.ContainsKey(chave);
        }

        private static ArsenalService Montar(IRepositorioDeSave repo)
            => new(new CapitulosService(repo), new PoService(repo), repo);

        // ---------- a curva ----------

        /// <summary>
        /// O custo DOBRA a cada dezena. É a escada inteira num teste só: mudar a base ou o passo faz
        /// as seis linhas se moverem juntas, e é isso que se quer conferir.
        /// </summary>
        [Theory]
        [InlineData(1, 10)]
        [InlineData(9, 10)]
        [InlineData(10, 20)]
        [InlineData(19, 20)]
        [InlineData(20, 40)]
        [InlineData(30, 80)]
        [InlineData(40, 160)]
        [InlineData(50, 320)]
        [InlineData(59, 320)]
        public void CustoDoNivel_DobraPorDezena(int nivel, int esperado)
            => Assert.Equal(esperado, Po.CustoDoNivel(nivel));

        /// <summary>
        /// Os acumulados das seis paredes. O <b>6.290</b> do fim é o número que a curva inteira
        /// produz, e é ele que a medição calibrou: uma passada do Fácil rende ~1.740 ciclos, então o
        /// item carregado chega ao 39 (1.490) dentro dela e o apóstolo termina no 29.
        /// </summary>
        [Theory]
        [InlineData(1, 0)]
        [InlineData(10, 90)]
        [InlineData(20, 290)]
        [InlineData(30, 690)]
        [InlineData(40, 1_490)]
        [InlineData(50, 3_090)]
        [InlineData(60, 6_290)]
        public void PontosParaNivel_AsSeisParedes(int nivel, int esperado)
            => Assert.Equal(esperado, Po.PontosParaNivel(nivel));

        /// <summary>
        /// Sem estrela nenhuma a peça trava no 9, e cada estrela abre a dezena seguinte — a MESMA
        /// regra do apóstolo (<see cref="Progressao.TetoPorEstrelas"/>), porque é o mesmo baile.
        /// </summary>
        [Fact]
        public void SemEstrela_OItemTravaNo9_MesmoComPontosDeSobra()
        {
            var item = new Item("Arma", "🗡️", Faccao.Reino, Fases.Fase1, TipoStat.ATKFlat)
            {
                Pontos = Po.PontosParaNivel(60)   // pontos pro jogo inteiro
            };

            Assert.Equal(9, item.Nivel);

            item.Estrelas = 1;
            Assert.Equal(19, item.Nivel);

            item.Estrelas = Material.EstrelaMaxima;
            Assert.Equal(60, item.Nivel);
        }

        // ---------- o valor ----------

        /// <summary>
        /// A magnitude é `MÁXIMO do slot × fatorNível`, e as seis dezenas caem exatamente nos seis
        /// valores da grade (25 · 40 · 55 · 70 · 85 · 100%). Um mítico nível 1 vale 11,5% — pior que
        /// um comum no teto, e isso é o desenho, não um bug.
        /// </summary>
        [Theory]
        [InlineData(1, 0.115)]
        [InlineData(10, 0.25)]
        [InlineData(20, 0.40)]
        [InlineData(30, 0.55)]
        [InlineData(40, 0.70)]
        [InlineData(50, 0.85)]
        [InlineData(60, 1.00)]
        public void FatorNivel_AsSeisDezenasCaemNaGrade(int nivel, double esperado)
            => Assert.Equal(esperado, Equipamento.FatorNivel(nivel), 6);

        /// <summary>
        /// <b>A FACÇÃO NÃO MUDA MAIS O NÚMERO.</b> Era ela que multiplicava o valor pelo capítulo, e
        /// é a mudança de maior impacto no que o jogador sente: a arma do capítulo 8 valia oito vezes
        /// a do 1. Agora a facção é o CONJUNTO e quem move magnitude é o nível, só.
        /// </summary>
        [Fact]
        public void AFaccaoNaoMexeNoValor()
        {
            var doReino = new Item("Arma", "🗡️", Faccao.Reino, Fases.Fase1, TipoStat.ATKFlat);
            var dosAscendentes = new Item("Arma", "🎄", Faccao.Ascendentes, Fases.Fase1, TipoStat.ATKFlat);

            Assert.Equal(doReino.Valor, dosAscendentes.Valor, 6);
            Assert.Equal(500 * 0.115, doReino.Valor, 6);
        }

        // ---------- o que a fase paga ----------

        /// <summary>
        /// O ponto por ciclo é o valor do enum da dificuldade, e a vitória soma o bônus fixo. O ciclo
        /// fracionário TRUNCA: meio ciclo não vira meio ponto.
        /// </summary>
        [Theory]
        [InlineData(Dificuldade.Facil, 31.0, true, 36)]      // 31×1 + 5
        [InlineData(Dificuldade.Facil, 31.0, false, 31)]     // derrota: só o acumulado
        [InlineData(Dificuldade.Pesadelo, 31.0, true, 129)]  // 31×4 + 5
        [InlineData(Dificuldade.Normal, 3.9, true, 11)]      // trunca em 3 ciclos: 3×2 + 5
        public void PontosDaFase(Dificuldade dif, double ciclos, bool venceu, int esperado)
            => Assert.Equal(esperado, Po.PontosDaFase(dif, ciclos, venceu));

        /// <summary>
        /// O teto por fase existe porque a batalha do jogo NÃO tem limite de turnos: um combate em
        /// que nenhum lado consegue matar o outro roda indefinidamente (medido em ago/2026: 169.430
        /// ciclos numa fase só). Sem o teto, ele imprimiria nível de item sem fim.
        /// </summary>
        [Fact]
        public void TetoPorFase_SeguraABatalhaQueNaoTermina()
        {
            int normal = Po.PontosDaFase(Dificuldade.Facil, Po.TetoDeCiclosPorFase, venceu: true);
            int absurdo = Po.PontosDaFase(Dificuldade.Facil, 169_430, venceu: true);

            Assert.Equal(normal, absurdo);
        }

        // ---------- o pedágio ----------

        /// <summary>
        /// A receita do pó é a MESMA FORMA da alma (muito da faixa atual + pouco da próxima), com
        /// constantes próprias — são 7 peças por apóstolo contra 1, e com o preço da alma cada parede
        /// custaria sete vezes mais.
        /// </summary>
        [Fact]
        public void Receita_MesmaFormaDaAlma_ComPrecoProprio()
        {
            Assert.Equal(new[] { new Custo(Raridade.Comum, Po.CustoDaPrimeira) }, Po.Receita(1));

            Assert.Equal(
                new[] { new Custo(Raridade.Comum, Po.CustoDaFaixa), new Custo(Raridade.Incomum, Po.CustoDaProxima) },
                Po.Receita(2));

            // A 4ª pede Épico, e Épico não cai no Fácil: é daí que sai o teto 39 do Fácil, sem uma
            // linha escrita dizendo "teto".
            Assert.Contains(Po.Receita(4), c => c.Raridade == Raridade.Epico);
            Assert.DoesNotContain(Material.FaixasQueCaem(Dificuldade.Facil), r => r == Raridade.Epico);
        }

        /// <summary>
        /// A estrela só se compra NA PAREDE, e ela move o nível de uma vez — os pontos continuam
        /// acumulando travados, que é o que faz a compra valer a pena na hora em que aparece.
        /// </summary>
        [Fact]
        public void ComprarEstrela_SoNaParede_EComPoNoBolso()
        {
            var repo = new RepositorioFake();
            var arsenal = Montar(repo);
            Item item = arsenal.DroparItens(Faccao.Reino, Fases.Fase1)[0];

            // Recém-caído: nível 1, longe da parede.
            Assert.False(arsenal.NaParede(item));
            Assert.Equal(MotivoRecusa.ForaDaParede, arsenal.ComprarEstrela(item));

            // Pontos de sobra: agora está travado no 9.
            item.Pontos = Po.PontosParaNivel(20);
            Assert.True(arsenal.NaParede(item));
            Assert.Equal(9, item.Nivel);

            // Sem pó, a compra é recusada — e o item NÃO se move.
            Assert.Equal(MotivoRecusa.SemSaldo, arsenal.ComprarEstrela(item));
            Assert.Equal(9, item.Nivel);
        }

        /// <summary>
        /// Ganhar uso só mexe em quem está VESTIDO. Peça no baú não sobe — é o "com o item equipado
        /// em alguém em campo" do GDD, e é o que dá peso a escolher o que levar.
        /// </summary>
        [Fact]
        public void CreditarUso_SoAsPecasVestidas()
        {
            var repo = new RepositorioFake();
            var arsenal = Montar(repo);
            var caidos = arsenal.DroparItens(Faccao.Reino, Fases.Fase1);

            arsenal.EquiparItem(caidos[0]);
            arsenal.CreditarUso(Dificuldade.Facil, ciclos: 31, venceu: true);

            Assert.Equal(36, caidos[0].Pontos);
            Assert.All(caidos.Skip(1), i => Assert.Equal(0, i.Pontos));
        }

        /// <summary>
        /// O PÓ é recompensa de fase e cai só na VITÓRIA, junto com as peças. O USO é outra coisa e
        /// cai dos dois jeitos — quem tentou a fase acima do próprio nível e caiu não sai de mãos
        /// vazias, e é isso que o faz continuar arriscando.
        /// </summary>
        [Fact]
        public void NaDerrota_OUsoEntra_MasOPoNao()
        {
            var repo = new RepositorioFake();
            var po = new PoService(repo);
            var arsenal = new ArsenalService(new CapitulosService(repo), po, repo);
            Item item = arsenal.DroparItens(Faccao.Reino, Fases.Fase1)[0];
            arsenal.EquiparItem(item);

            arsenal.CreditarUso(Dificuldade.Facil, ciclos: 20, venceu: false);

            Assert.Equal(20, item.Pontos);                       // o acumulado, sem o bônus de vitória
            Assert.All(Enum.GetValues<Raridade>(), r => Assert.Equal(0, po.SaldoDe(r)));
        }

        // ---------- a fórmula ----------

        /// <summary>
        /// <c>(base + cheios) × (1 + Σ%)</c>, e os percentuais SOMAM entre si.
        ///
        /// Era aqui que morava o defeito antigo: o % multiplicava só a base e IGNORAVA os valores
        /// cheios, então cheio e % viravam duas parcelas competindo pelo mesmo slot — e qual delas
        /// ganhava mudava com o nível. Multiplicando, o cheio vira o PISO sobre o qual todo % incide,
        /// e nunca é lixo.
        /// </summary>
        [Fact]
        public void AFormula_OPercentualMultiplicaOCheio_EOsPercentuaisSomam()
        {
            Personagem alguem = new PersonagemService().ObterPersonagem(Faccao.Reino, Slot.Slot1);
            var vestido = new Jogador(alguem);
            int baseAtk = vestido.Ataque;

            // Uma peça de ATK cheio e duas de ATK% — as duas de 5,75% cada no nível 1.
            vestido.AplicarItens(new[]
            {
                new Item("A", "🗡️", Faccao.Reino, Fases.Fase1, TipoStat.ATKFlat),
                new Item("B", "👔", Faccao.Reino, Fases.Fase5, TipoStat.ATKPct),
                new Item("C", "👖", Faccao.Reino, Fases.Fase6, TipoStat.ATKPct),
            });

            double cheio = Equipamento.Maximo(TipoStat.ATKFlat) * Equipamento.FatorNivel(1);
            double pct = 2 * Equipamento.Maximo(TipoStat.ATKPct) * Equipamento.FatorNivel(1);

            Assert.Equal((int)((baseAtk + cheio) * (1 + pct)), vestido.Ataque);
        }

        /// <summary>
        /// <b>Vestir não pode largar o apóstolo ferido.</b> O HPMaximo é estado mutável, então o
        /// <c>AplicarItens</c> escreve o total e o HPAtual acompanha o DELTA — sem isso o boneco
        /// entraria na luta com a barra pela metade só por ter ganhado um Elmo.
        /// </summary>
        [Fact]
        public void VestirHP_OAtualAcompanhaOMaximo()
        {
            Personagem alguem = new PersonagemService().ObterPersonagem(Faccao.Reino, Slot.Slot1);
            var vestido = new Jogador(alguem);

            vestido.AplicarItens(new[] { new Item("E", "👑", Faccao.Reino, Fases.Fase2, TipoStat.HPFlat) });

            Assert.Equal(vestido.HPMaximo, vestido.HPAtual);
        }

        // ---------- o save ----------

        /// <summary>
        /// A peça vestida e a do inventário são a MESMA, inclusive depois de recarregar. Se o save
        /// gravasse o objeto nos dois lugares, o uso subiria o nível de uma cópia só e a ficha
        /// mostraria um número diferente do que a luta usa.
        /// </summary>
        [Fact]
        public void AoRecarregar_APecaVestidaEAMesmaDoInventario()
        {
            var repo = new RepositorioFake();
            var arsenal = Montar(repo);
            Item original = arsenal.DroparItens(Faccao.Reino, Fases.Fase1)[0];
            arsenal.EquiparItem(original);
            arsenal.CreditarUso(Dificuldade.Facil, ciclos: 31, venceu: true);

            var recarregado = Montar(repo);
            recarregado.CarregarItensEquipados();

            Item? vestido = recarregado.ObterEquipados()[0];
            Assert.NotNull(vestido);
            Assert.Equal(original.Id, vestido!.Id);
            Assert.Equal(36, vestido.Pontos);
            Assert.Same(vestido, recarregado.ObterObtidos().Single(i => i.Id == original.Id));
        }
    }
}
