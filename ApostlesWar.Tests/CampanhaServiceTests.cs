using ApostlesWar.Application.Portas;
using ApostlesWar.Application.Services;
using ApostlesWar.Domain;

namespace Tests
{
    /// <summary>
    /// Testes do <see cref="CampanhaService"/> (a recompensa da vitória: unlock/drop/save) e do
    /// <see cref="ArsenalService.PreverItem"/>. É PURO (sem combate/TTY), então roda headless com um
    /// repositório fake e um <see cref="ITelaDeMenu"/> no-op (nenhum método de menu é chamado aqui).
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

        // ITelaDeMenu não é exercitado por ProcessarVitoria — fake no-op só pra construir o CampeoesService.
        private sealed class MenuNoOp : ITelaDeMenu
        {
            public void ExibirMenu(int selecionado) { }
            public void ExibirMenuArena(int opcao) { }
            public void ExibirConfirmacaoSaida(int opcao) { }
            public void MenuCapitulos(int selecionado) { }
            public void MenuFases(Faccao faccao, int selecionado) { }
            public void ExibirTelaVitoria(List<Personagem> novosCampeoes, Item? itemDropado) { }
            public void ExibirTelaDerrota() { }
            public void ExibirCreditos() { }
            public void ExibirAviso(string mensagem, int ms) { }
            public int NavegarBoneco() => 0;
            public Item? NavegarTrocaItem(int slotEmEdicao, List<Item> itensDoTipo) => null;
            public List<Personagem> NavegarSelecaoTime(List<Personagem> desbloqueados) => desbloqueados;
        }

        private static (CampanhaService campanha, CapitulosService capitulos, CampeoesService campeoes)
            Montar()
        {
            var repo = new RepositorioFake();
            var capitulos = new CapitulosService(repo);
            var arsenal = new ArsenalService(capitulos, repo);
            var campeoes = new CampeoesService(new PersonagemService(), new MenuNoOp(), capitulos);
            return (new CampanhaService(arsenal, campeoes, capitulos), capitulos, campeoes);
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
            var campeoes = new CampeoesService(new PersonagemService(), new MenuNoOp(), capitulos);
            var campanha = new CampanhaService(arsenal, campeoes, capitulos);

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
    }
}
