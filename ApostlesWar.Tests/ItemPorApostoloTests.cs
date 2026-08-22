using ApostlesWar.Application.Portas;
using ApostlesWar.Application.Services;
using ApostlesWar.Domain;
using Xunit;

namespace Tests
{
    /// <summary>
    /// O VÍNCULO peça↔apóstolo (docs/GDD-itens.md §O ACERVO, passo 10-b do GDD-progressao §7): cada
    /// apóstolo tem o próprio boneco de 7 slots, e a peça é de UM só.
    ///
    /// Antes disso os 7 slots eram globais e valiam pra todo mundo — quem lutasse levava as mesmas
    /// peças. Estes testes são o que impede aquilo de voltar por descuido.
    /// </summary>
    public class ItemPorApostoloTests
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
            => new(new CapitulosService(repo), new PoService(repo), new PersonagemService(), repo);

        private static readonly PersonagemService Roster = new();
        private static Personagem Guarda => Roster.ObterPersonagem(Faccao.Reino, Slot.Slot1);
        private static Personagem Ninja => Roster.ObterPersonagem(Faccao.Reino, Slot.Slot2);

        /// <summary>
        /// O boneco é DE CADA UM: vestir no Guarda não veste no Ninja. É a afirmação central do
        /// vínculo, e a que estava falsa enquanto os slots eram globais.
        /// </summary>
        [Fact]
        public void OBonecoEDeCadaApostolo()
        {
            var arsenal = Montar(new RepositorioFake());
            Item arma = arsenal.DroparItens(Faccao.Reino, Fases.Fase1)[0];

            arsenal.EquiparItem(Guarda, arma);

            Assert.Same(arma, arsenal.ObterEquipados(Guarda)[0]);
            Assert.Null(arsenal.ObterEquipados(Ninja)[0]);
            Assert.Equal("Guarda", arsenal.PortadorDe(arma)?.Nome);
        }

        /// <summary>
        /// A peça é UMA: vestir no Ninja o que estava no Guarda TIRA do Guarda. É o "tomar do aliado"
        /// do §O ACERVO — o modelo permite de propósito, e quem tem de avisar antes do clique é a
        /// tela (o emoji do portador no cartão).
        /// </summary>
        [Fact]
        public void VestirNoOutro_TiraDoPrimeiro()
        {
            var arsenal = Montar(new RepositorioFake());
            Item arma = arsenal.DroparItens(Faccao.Reino, Fases.Fase1)[0];

            arsenal.EquiparItem(Guarda, arma);
            arsenal.EquiparItem(Ninja, arma);

            Assert.Null(arsenal.ObterEquipados(Guarda)[0]);
            Assert.Same(arma, arsenal.ObterEquipados(Ninja)[0]);
            Assert.Equal("Ninja", arsenal.PortadorDe(arma)?.Nome);
        }

        /// <summary>
        /// Remover esvazia o slot e devolve a peça ao BAÚ. Descartar de vez é a Forja (o sacrifício):
        /// quem tira o elmo pra experimentar outro não está jogando o elmo fora.
        /// </summary>
        [Fact]
        public void Desequipar_EsvaziaOSlot_ESemPerderAPeca()
        {
            var arsenal = Montar(new RepositorioFake());
            Item arma = arsenal.DroparItens(Faccao.Reino, Fases.Fase1)[0];
            arsenal.EquiparItem(Guarda, arma);

            arsenal.DesequiparItem(Guarda, Fases.Fase1);

            Assert.Null(arsenal.ObterEquipados(Guarda)[0]);
            Assert.Null(arsenal.PortadorDe(arma));
            Assert.Contains(arma, arsenal.ObterObtidos());
        }

        /// <summary>
        /// O uso paga só quem ESTAVA EM CAMPO. Com os slots globais isso era aproximação: a peça do
        /// banco subia junto com a de quem lutou.
        /// </summary>
        [Fact]
        public void CreditarUso_SoAsPecasDeQuemLutou()
        {
            var arsenal = Montar(new RepositorioFake());
            var caidas = arsenal.DroparItens(Faccao.Reino, Fases.Fase1);

            arsenal.EquiparItem(Guarda, caidas[0]);
            arsenal.EquiparItem(Ninja, caidas[1]);

            arsenal.CreditarUso(new[] { Guarda }, Dificuldade.Facil, ciclos: 31, venceu: true);

            Assert.Equal(Po.PontosDaFase(Dificuldade.Facil, 31, venceu: true), caidas[0].Pontos);
            Assert.Equal(0, caidas[1].Pontos);   // ficou no banco
        }

        /// <summary>
        /// O save guarda o boneco de cada um, e o ID volta a ser a REFERÊNCIA do inventário — sem
        /// isso o slot teria uma cópia própria e o uso subiria o nível de uma só.
        /// </summary>
        [Fact]
        public void OSaveVoltaPorApostolo_EPelaMesmaReferencia()
        {
            var repo = new RepositorioFake();
            var arsenal = Montar(repo);
            var caidas = arsenal.DroparItens(Faccao.Reino, Fases.Fase1);
            arsenal.EquiparItem(Ninja, caidas[2]);

            var recarregado = Montar(repo);
            recarregado.CarregarItensEquipados();

            Item? vestida = recarregado.ObterEquipados(Ninja)[0];
            Assert.NotNull(vestida);
            Assert.Equal(caidas[2].Id, vestida!.Id);
            Assert.Same(vestida, recarregado.ObterObtidos().Single(i => i.Id == caidas[2].Id));
            Assert.Null(recarregado.ObterEquipados(Guarda)[0]);
        }

        /// <summary>
        /// <b>Save de antes do vínculo: o que estava vestido DESVESTE.</b> Lá os 7 IDs valiam pro jogo
        /// inteiro e não há de quem eles eram — dar tudo a um apóstolo escolhido por nós inventaria
        /// uma decisão do jogador. O acervo fica intacto: ninguém perde peça, só reescolhe.
        /// </summary>
        [Fact]
        public void SaveComOsSlotsGlobais_DesequipaTudo_ESemPerderPeca()
        {
            var repo = new RepositorioFake();
            var antigo = Montar(repo);
            var caidas = antigo.DroparItens(Faccao.Reino, Fases.Fase1);

            // O formato ANTIGO, escrito à mão: um Guid?[7] global, sem dono.
            var globais = new Guid?[7];
            globais[0] = caidas[0].Id;
            repo.Salvar("equipados", globais);

            var novo = Montar(repo);
            novo.CarregarItensEquipados();

            Assert.Equal(caidas.Count, novo.ObterObtidos().Count);         // o acervo não perdeu nada
            Assert.Null(novo.PortadorDe(novo.ObterObtidos()[0]));          // e ninguém veste nada
            Assert.False(repo.Contem("equipados"));                        // o formato velho morreu
        }
    }
}
