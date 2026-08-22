using ApostlesWar.Application.Portas;
using ApostlesWar.Application.Services;
using ApostlesWar.Domain;

namespace Tests
{
    /// <summary>
    /// As quatro trilhas do <see cref="CapitulosService"/>: progresso separado por dificuldade,
    /// desbloqueio derivado da 8-7 anterior, e a XP creditada pelo <see cref="ProgressaoService"/>.
    /// Puro — nenhum destes services pede tela.
    /// </summary>
    public class DificuldadeTests
    {
        private sealed class RepositorioFake : IRepositorioDeSave
        {
            private readonly Dictionary<string, object?> _dados = new();
            public void Salvar<T>(string chave, T dado) => _dados[chave] = dado;
            public T? Carregar<T>(string chave) => _dados.TryGetValue(chave, out var v) && v is T t ? t : default;
            public void Excluir(string chave) => _dados.Remove(chave);
            public bool Contem(string chave) => _dados.ContainsKey(chave);
        }

        private static readonly Fases UltimaFase = Enum.GetValues<Fases>().Last();

        /// <summary>Fecha a 8-7 de uma dificuldade — é o único ato que abre a seguinte.</summary>
        private static void FecharACampanha(CapitulosService capitulos, Dificuldade dificuldade)
            => capitulos.ConcluirFase(Faccao.Ascendentes, UltimaFase, dificuldade);

        [Fact]
        public void OValorDoEnum_EOMultiplicadorDeXp()
        {
            Assert.Equal(1, (int)Dificuldade.Facil);
            Assert.Equal(2, (int)Dificuldade.Normal);
            Assert.Equal(3, (int)Dificuldade.Dificil);
            Assert.Equal(4, (int)Dificuldade.Pesadelo);
        }

        [Fact]
        public void SoOFacil_ComecaAberto()
        {
            var capitulos = new CapitulosService(new RepositorioFake());

            Assert.True(capitulos.DificuldadeDesbloqueada(Dificuldade.Facil));
            Assert.False(capitulos.DificuldadeDesbloqueada(Dificuldade.Normal));
            Assert.False(capitulos.DificuldadeDesbloqueada(Dificuldade.Pesadelo));
        }

        /// <summary>
        /// Fechar a 8-7 abre a PRÓXIMA, e só ela — e o Fácil continua aberto. Ninguém perde acesso ao
        /// que já destravou: voltar pra farmar é jogada legítima.
        /// </summary>
        [Fact]
        public void Fechar87_AbreAProxima_ESoEla()
        {
            var capitulos = new CapitulosService(new RepositorioFake());

            FecharACampanha(capitulos, Dificuldade.Facil);

            Assert.True(capitulos.DificuldadeDesbloqueada(Dificuldade.Facil));
            Assert.True(capitulos.DificuldadeDesbloqueada(Dificuldade.Normal));
            Assert.False(capitulos.DificuldadeDesbloqueada(Dificuldade.Dificil));
        }

        /// <summary>
        /// O progresso é POR TRILHA. Vencer no Fácil não pode liberar nada no Normal — se as quatro
        /// compartilhassem a mesma lista de capítulos, fechar o Fácil entregaria o jogo inteiro.
        /// </summary>
        [Fact]
        public void Progresso_NaoVazaEntreDificuldades()
        {
            var capitulos = new CapitulosService(new RepositorioFake());

            capitulos.DesbloquearFase(Faccao.Reino, Fases.Fase1, Dificuldade.Facil);
            capitulos.ConcluirFase(Faccao.Reino, Fases.Fase1, Dificuldade.Facil);

            Assert.True(capitulos.FaseConcluida(Faccao.Reino, Fases.Fase1, Dificuldade.Facil));
            Assert.False(capitulos.FaseConcluida(Faccao.Reino, Fases.Fase1, Dificuldade.Normal));
            Assert.False(capitulos.EstaDesbloqueado(Faccao.Reino, Fases.Fase2, Dificuldade.Normal));
        }

        /// <summary>Fase travada continua travada mesmo com a dificuldade aberta, e vice-versa.</summary>
        [Fact]
        public void EstaDesbloqueado_ExigeADificuldadeAberta()
        {
            var capitulos = new CapitulosService(new RepositorioFake());

            Assert.False(capitulos.EstaDesbloqueado(Faccao.Reino, Fases.Fase1, Dificuldade.Normal));

            FecharACampanha(capitulos, Dificuldade.Facil);

            Assert.True(capitulos.EstaDesbloqueado(Faccao.Reino, Fases.Fase1, Dificuldade.Normal));
        }

        [Fact]
        public void Save_SobreviveAoRoundTrip_ComAsQuatroTrilhas()
        {
            var repo = new RepositorioFake();
            var salvando = new CapitulosService(repo);

            FecharACampanha(salvando, Dificuldade.Facil);
            salvando.ConcluirFase(Faccao.Reino, Fases.Fase1, Dificuldade.Normal);
            salvando.SalvarProgresso();

            var carregando = new CapitulosService(repo);
            carregando.CarregarProgresso();

            Assert.True(carregando.DificuldadeDesbloqueada(Dificuldade.Normal));
            Assert.True(carregando.FaseConcluida(Faccao.Reino, Fases.Fase1, Dificuldade.Normal));
            Assert.False(carregando.FaseConcluida(Faccao.Reino, Fases.Fase1, Dificuldade.Facil));
        }

        /// <summary>
        /// As fases vencidas saem das QUATRO trilhas, com a dificuldade junto — é o que o arsenal e os
        /// apóstolos usam pra reconstruir o que já foi conquistado.
        /// </summary>
        [Fact]
        public void FasesConcluidas_TrazAsQuatroTrilhas()
        {
            var capitulos = new CapitulosService(new RepositorioFake());
            capitulos.ConcluirFase(Faccao.Reino, Fases.Fase1, Dificuldade.Facil);
            capitulos.ConcluirFase(Faccao.Reino, Fases.Fase1, Dificuldade.Normal);

            var concluidas = capitulos.FasesConcluidas().ToList();

            Assert.Equal(2, concluidas.Count);
            Assert.Contains((Faccao.Reino, Fases.Fase1, Dificuldade.Facil), concluidas);
            Assert.Contains((Faccao.Reino, Fases.Fase1, Dificuldade.Normal), concluidas);
        }

        // ---------- a XP ----------

        [Fact]
        public void Creditar_DivideOPoteEntreQuemEstaEmCampo()
        {
            var repo = new RepositorioFake();
            var personagens = new PersonagemService();
            var progressao = new ProgressaoService(personagens, new AlmaService(repo), repo);

            var solo = new List<Personagem> { personagens.ObterPersonagem(Faccao.Reino, Slot.Slot1) };
            progressao.Creditar(solo, 4_000);

            Assert.Equal(4_000, progressao.XpDe(solo[0]));

            var quatro = Enum.GetValues<Slot>().Select(s => personagens.ObterPersonagem(Faccao.LadoSombrio, s)).ToList();
            progressao.Creditar(quatro, 4_000);

            Assert.All(quatro, p => Assert.Equal(1_000, progressao.XpDe(p)));
        }

        /// <summary>
        /// A XP vira NÍVEL na hora, e o nível vira ficha: quem sobe bate mais forte. Mas ela para na
        /// parede — quem abre a dezena seguinte é a ESTRELA, e a XP que sobrou salta assim que ela é
        /// comprada.
        /// </summary>
        [Fact]
        public void Creditar_SobeONivelEAFicha()
        {
            var repo = new RepositorioFake();
            var personagens = new PersonagemService();
            var alma = new AlmaService(repo);
            var progressao = new ProgressaoService(personagens, alma, repo);

            Personagem p = personagens.ObterPersonagem(Faccao.Misticos, Slot.Slot1);
            int hpAntes = p.HP;

            progressao.Creditar(new List<Personagem> { p }, Progressao.XpParaNivel(10));

            Assert.Equal(9, p.Nivel);   // XP de sobra e nenhuma estrela: para na 1ª parede
            Assert.True(p.HP > hpAntes);
            Assert.True(repo.Contem("progressao"));

            alma.Creditar(Dificuldade.Facil, 100);
            Assert.Equal(MotivoRecusa.Nenhum, progressao.ComprarEstrela(p));
            Assert.Equal(10, p.Nivel);   // a XP guardada na parede entra de uma vez

            progressao.Resetar();   // devolve disco E memória: as instâncias são compartilhadas
            Assert.Equal(Arquetipos.NivelMinimo, p.Nivel);
            Assert.Equal(hpAntes, p.HP);
        }

        [Fact]
        public void Carregar_RestauraONivelDoRoster()
        {
            var repo = new RepositorioFake();
            var personagens = new PersonagemService();
            var alma = new AlmaService(repo);
            var salvando = new ProgressaoService(personagens, alma, repo);

            Personagem p = personagens.ObterPersonagem(Faccao.Folclore, Slot.Slot2);
            alma.Creditar(Dificuldade.Facil, 100);

            // Uma parede por vez, que é como o jogo anda: a XP para em cada uma e a estrela abre a
            // dezena seguinte ZERADA — despejar a XP do 20 de uma vez não passaria do 10.
            salvando.Creditar(new List<Personagem> { p }, Progressao.XpParaNivel(20));
            salvando.ComprarEstrela(p);   // 9 → 10
            salvando.Creditar(new List<Personagem> { p }, Progressao.XpParaNivel(20));
            salvando.ComprarEstrela(p);   // 19 → 20
            p.AplicarNivel(Arquetipos.NivelMinimo);   // finge que o jogo reabriu com o roster cru

            new ProgressaoService(personagens, new AlmaService(repo), repo).Carregar();

            Assert.Equal(20, p.Nivel);
        }

        /// <summary>
        /// O save de ANTES da estrela comprada não tinha o campo. Sem a dedução do
        /// <see cref="ProgressaoService.Carregar"/>, todo apóstolo já nivelado despencaria pro 9.
        /// </summary>
        [Fact]
        public void Carregar_SaveAntigoDeduzAEstrelaDoNivel()
        {
            var repo = new RepositorioFake();
            var personagens = new PersonagemService();

            repo.Salvar("progressao", new List<ApostoloProgredido>
            {
                new(Faccao.Reino, Slot.Slot3, Progressao.XpParaNivel(34), Estrelas: null),
            });

            new ProgressaoService(personagens, new AlmaService(repo), repo).Carregar();

            Assert.Equal(34, personagens.ObterPersonagem(Faccao.Reino, Slot.Slot3).Nivel);
        }
    }
}
