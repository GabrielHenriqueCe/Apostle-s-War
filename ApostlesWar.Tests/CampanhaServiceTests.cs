using ApostlesWar.Application.Portas;
using ApostlesWar.Application.Services;
using ApostlesWar.Domain;

namespace Tests
{
    /// <summary>
    /// Testes do <see cref="CampanhaService"/> (a recompensa da vitória: unlock/drop/save) e do
    /// <see cref="ArsenalService.PreverItem"/>. É PURO (sem combate/TTY), então roda headless com um
    /// repositório fake — nenhum destes services pede tela.
    /// </summary>
    public class CampanhaServiceTests
    {
        private sealed class RepositorioFake : IRepositorioDeSave
        {
            private readonly Dictionary<string, object?> _dados = new();
            public void Salvar<T>(string chave, T dado) => _dados[chave] = dado;
            public T? Carregar<T>(string chave) => _dados.TryGetValue(chave, out var v) && v is T t ? t : default;
            public void Excluir(string chave) => _dados.Remove(chave);
            public bool Contem(string chave) => _dados.ContainsKey(chave);
        }

        private static (CampanhaService campanha, CapitulosService capitulos, CampeoesService campeoes)
            Montar()
        {
            var repo = new RepositorioFake();
            var capitulos = new CapitulosService(repo);
            var arsenal = new ArsenalService(capitulos, repo);
            var campeoes = new CampeoesService(new PersonagemService(), capitulos);
            return (new CampanhaService(arsenal, campeoes, capitulos, repo), capitulos, campeoes);
        }

        [Fact]
        public void ProcessarVitoria_Reino1_DesbloqueiaProximaFase_ChampsEItem()
        {
            var (campanha, capitulos, campeoes) = Montar();

            Assert.Equal(4, campeoes.ObterDesbloqueados().Count);              // só os 4 Humanos no começo
            Assert.False(capitulos.EstaDesbloqueado(Faccao.Reino, Fases.Fase2)); // fase 2 ainda travada

            RecompensaDaFase r = campanha.ProcessarVitoria(Faccao.Reino, Fases.Fase1);

            Assert.True(capitulos.FaseConcluida(Faccao.Reino, Fases.Fase1));   // marcou concluída
            Assert.True(capitulos.EstaDesbloqueado(Faccao.Reino, Fases.Fase2)); // liberou a próxima
            // Reino fase 1: Rodada1=[Slot1 Guarda], Rodada2=[Slot2 Ninja] → 2 champs novos
            Assert.Equal(2, r.NovosCampeoes.Count);
            Assert.NotNull(r.Item);
            Assert.Equal("Arma", r.Item!.Nome);
            Assert.Equal(6, campeoes.ObterDesbloqueados().Count);              // 4 Humanos + 2 novos
        }

        [Fact]
        public void ProcessarVitoria_Persiste_SaveEItens()
        {
            var repo = new RepositorioFake();
            var capitulos = new CapitulosService(repo);
            var arsenal = new ArsenalService(capitulos, repo);
            var campeoes = new CampeoesService(new PersonagemService(), capitulos);
            var campanha = new CampanhaService(arsenal, campeoes, capitulos, repo);

            campanha.ProcessarVitoria(Faccao.Reino, Fases.Fase1);

            Assert.True(repo.Contem("save"));
            Assert.True(repo.Contem("itens"));
        }

        [Fact]
        public void PreverItem_Deterministico_SemEfeitoColateral()
        {
            var repo = new RepositorioFake();
            var capitulos = new CapitulosService(repo);
            var arsenal = new ArsenalService(capitulos, repo);

            Item item = arsenal.PreverItem(Faccao.Reino, Fases.Fase1);

            Assert.Equal("Arma", item.Nome);
            Assert.Equal("🗡️", item.Simbolo);
            Assert.Empty(arsenal.ObterObtidos());   // preview não dropa (não mexe em obtidos)
        }

        /// <summary>
        /// O nome do SLOT e o nome do ITEM que cai nele são a mesma coisa — uma tabela só. O front
        /// tinha a própria cópia da lista e ela envelheceu (chamava a Fase 4 de "Acessório" enquanto
        /// o item nasce "Manopla"); este teste é a trava pra não acontecer de novo.
        /// </summary>
        [Fact]
        public void NomeDoSlot_CasaComONomeDoItemQueCaiNele()
        {
            var repo = new RepositorioFake();
            var arsenal = new ArsenalService(new CapitulosService(repo), repo);

            foreach (Fases fase in Enum.GetValues<Fases>())
                Assert.Equal(ArsenalService.NomeDoSlot(fase), arsenal.PreverItem(Faccao.Reino, fase).Nome);
        }

        [Fact]
        public void EquiparItem_PersisteSozinho()
        {
            var repo = new RepositorioFake();
            var arsenal = new ArsenalService(new CapitulosService(repo), repo);

            arsenal.EquiparItem(arsenal.PreverItem(Faccao.Reino, Fases.Fase1));

            // Sem ninguém chamar SalvarItens: quem manda no dado decide quando ele é durável.
            Assert.True(repo.Contem("itens"));
        }

        [Fact]
        public void TotaisEquipados_SemNada_NaoInventaLinhaDeZero()
        {
            var repo = new RepositorioFake();
            var arsenal = new ArsenalService(new CapitulosService(repo), repo);

            Assert.Empty(arsenal.TotaisEquipados());
        }

        [Fact]
        public void TotaisEquipados_UmaLinhaPorStatEquipado()
        {
            var repo = new RepositorioFake();
            var arsenal = new ArsenalService(new CapitulosService(repo), repo);

            arsenal.EquiparItem(arsenal.PreverItem(Faccao.Reino, Fases.Fase1));   // Arma → ATK 120
            arsenal.EquiparItem(arsenal.PreverItem(Faccao.Reino, Fases.Fase2));   // Elmo → HP 550

            var totais = arsenal.TotaisEquipados();

            Assert.Equal(2, totais.Count);
            Assert.Equal(120, totais.Single(b => b.Stat == TipoStat.ATKFlat).Valor);
            Assert.Equal(550, totais.Single(b => b.Stat == TipoStat.HPFlat).Valor);
        }

        /// <summary>
        /// O total é uma SOMA, não uma re-listagem dos slots. Hoje cada slot carrega um stat
        /// diferente, então a distinção não aparece jogando — é por isso que o teste força o caso:
        /// dois slots dando o MESMO stat têm que virar uma linha só, com os dois valores somados.
        /// Sem isto, uma implementação que só percorre os 7 slots passaria igual e quebraria calada
        /// no dia em que o segundo item de ATK existir.
        /// </summary>
        [Fact]
        public void TotaisEquipados_DoisItensDoMesmoStat_SomamNumaLinhaSo()
        {
            var repo = new RepositorioFake();
            var arsenal = new ArsenalService(new CapitulosService(repo), repo);

            // Fases (= slots) diferentes, mesmo TipoStat. Reino = cap 1 → 120; Lado Sombrio = cap 2 → 240.
            arsenal.EquiparItem(new Item("A", "🗡️", Faccao.Reino, Fases.Fase1, TipoStat.ATKFlat));
            arsenal.EquiparItem(new Item("B", "🏹", Faccao.LadoSombrio, Fases.Fase2, TipoStat.ATKFlat));

            var totais = arsenal.TotaisEquipados();

            Assert.Single(totais);
            Assert.Equal(TipoStat.ATKFlat, totais[0].Stat);
            Assert.Equal(360, totais[0].Valor);
        }

        /// <summary>
        /// FLAT e PCT do mesmo stat NÃO se fundem: "+550 de HP" e "+5% de HP" são coisas diferentes
        /// até haver um champ pra aplicar o percentual (o `AplicarItem` usa o HP BASE dele). O
        /// arsenal informa o que o equipamento dá; quem soma isso a alguém é a luta.
        /// </summary>
        [Fact]
        public void TotaisEquipados_FlatEPercentualDoMesmoStat_SeguemSeparados()
        {
            var repo = new RepositorioFake();
            var arsenal = new ArsenalService(new CapitulosService(repo), repo);

            arsenal.EquiparItem(arsenal.PreverItem(Faccao.Reino, Fases.Fase2));   // Elmo     → HP  550
            arsenal.EquiparItem(arsenal.PreverItem(Faccao.Reino, Fases.Fase5));   // Peitoral → HP% 0,05

            var totais = arsenal.TotaisEquipados();

            Assert.Equal(2, totais.Count);
            Assert.Equal(550, totais.Single(b => b.Stat == TipoStat.HPFlat).Valor);
            Assert.Equal(0.05, totais.Single(b => b.Stat == TipoStat.HPPct).Valor, 3);
        }

        /// <summary>
        /// A posição no mapa (o "último lugar") é PROGRESSÃO, não estado de tela — por isso mora no
        /// service e some junto com a conta, em vez de o front gravar direto na porta de save.
        /// </summary>
        [Fact]
        public void PosicaoNoMapa_ComecaNoPrimeiro_EPersiste()
        {
            var (campanha, _, _) = Montar();

            Assert.Equal(0, campanha.PosicaoNoMapa());

            campanha.SalvarPosicao(3);

            Assert.Equal(3, campanha.PosicaoNoMapa());
        }

        /// <summary>
        /// O mapa é a lista de capítulos, na ordem — não "todas as facções menos Humanos", que era
        /// como o front deduzia (acertava por coincidência da ordem do enum).
        /// </summary>
        [Fact]
        public void FaccoesDaCampanha_SaoOsCapitulos_SemOsHumanos()
        {
            var capitulos = new CapitulosService(new RepositorioFake());

            var faccoes = capitulos.FaccoesDaCampanha();

            Assert.Equal(8, faccoes.Count);
            Assert.DoesNotContain(Faccao.Humanos, faccoes);   // time inicial, não é capítulo
            Assert.Equal(Faccao.Reino, faccoes[0]);           // a campanha começa no Reino
            Assert.Equal(Faccao.Apostolos, faccoes[^1]);      // e termina nos Apóstolos
        }
    }
}
