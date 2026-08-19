using ApostlesWar.Application;
using ApostlesWar.Application.Portas;
using ApostlesWar.Application.Services;
using ApostlesWar.Domain;

namespace Tests
{
    /// <summary>
    /// Testes do <see cref="PerfilService"/> — a "conta" do jogador. É PURO (só delega pra porta de
    /// save e lê os desbloqueados), então roda headless com um repositório fake em memória. Cobre o
    /// boot (existe/carrega), a criação, o "excluir conta" que limpa o perfil E o progresso, e as
    /// REGRAS DE AVATAR (quem pode ser a cara do jogador) — que moravam na tela de edição do front.
    /// </summary>
    public class PerfilServiceTests
    {
        // Porta de save fake: um dicionário em memória (a impl real, SaveLocal, mora na Infrastructure,
        // que o projeto de teste não referencia — e nem precisa: aqui só interessa a LÓGICA de chaves).
        private sealed class RepositorioFake : IRepositorioDeSave
        {
            private readonly Dictionary<string, object?> _dados = new();

            public void Salvar<T>(string chave, T dado) => _dados[chave] = dado;
            public T? Carregar<T>(string chave) => _dados.TryGetValue(chave, out var v) && v is T t ? t : default;
            public void Excluir(string chave) => _dados.Remove(chave);

            public bool Contem(string chave) => _dados.ContainsKey(chave);
        }

        // O PerfilService precisa dos desbloqueados pra decidir avatar; o ApostolosService nasce com
        // os 4 Humanos e é DADO puro (não pede tela), então dá pra montar de verdade. Idem o
        // CampanhaService, que é quem o "excluir conta" chama pra zerar o progresso.
        private static PerfilService Montar(IRepositorioDeSave repo)
            => MontarCompleto(repo).Perfil;

        private static (PerfilService Perfil, CapitulosService Capitulos, ApostolosService Apostolos,
            ArsenalService Arsenal, CampanhaService Campanha) MontarCompleto(IRepositorioDeSave repo)
        {
            var capitulos = new CapitulosService(repo);
            var arsenal = new ArsenalService(capitulos, new PoService(repo), repo);
            var apostolos = new ApostolosService(new PersonagemService(), capitulos);
            var campanha = new CampanhaService(arsenal, apostolos, capitulos, new PersonagemService(), new ProgressaoService(new PersonagemService(), new AlmaService(repo), repo), repo);
            return (new PerfilService(repo, apostolos, campanha), capitulos, apostolos, arsenal, campanha);
        }

        [Fact]
        public void SemPerfil_NaoExiste_ECarregaNulo()
        {
            var servico = Montar(new RepositorioFake());

            Assert.False(servico.Existe());
            Assert.Null(servico.Carregar());
        }

        [Fact]
        public void CriarPerfil_PassaAExistir_EGuardaNomeEAvatar()
        {
            var servico = Montar(new RepositorioFake());

            servico.CriarPerfil("Gabriel", "🕵️");

            Assert.True(servico.Existe());
            Perfil? p = servico.Carregar();
            Assert.NotNull(p);
            Assert.Equal("Gabriel", p!.Nome);
            Assert.Equal("🕵️", p.Avatar);
        }

        [Fact]
        public void Excluir_ApagaPerfil_EOProgressoDeCampanha()
        {
            var repo = new RepositorioFake();
            var servico = Montar(repo);

            servico.CriarPerfil("Gabriel", "🕵️");
            repo.Salvar("save", "progresso qualquer");   // simula uma campanha em andamento
            repo.Salvar("itens", "itens quaisquer");
            repo.Salvar("campanha", 3);                  // e uma posição no mapa

            servico.Excluir();

            Assert.False(servico.Existe());
            Assert.False(repo.Contem("perfil"));
            Assert.False(repo.Contem("save"));
            Assert.False(repo.Contem("itens"));
            Assert.False(repo.Contem("campanha"));
        }

        // O bug que estes dois fixam: "excluir conta" apagava o DISCO e deixava a MEMÓRIA intacta.
        // Como o CarregarProgresso/CarregarItensEquipados só sobrescrevem quando a porta devolve
        // não-nulo, um save ausente PRESERVA o que já está carregado — então o jogador excluía a
        // conta, criava perfil novo e continuava com os 36 apóstolos e o loot, que voltavam pro disco
        // na primeira fase vencida.

        [Fact]
        public void Excluir_ZeraOProgressoEmMemoria_NaoSoNoDisco()
        {
            var repo = new RepositorioFake();
            var (servico, capitulos, apostolos, arsenal, _) = MontarCompleto(repo);

            // Uma campanha em andamento: venceu a 1ª do Reino, liberou os apóstolos e pegou o item.
            servico.CriarPerfil("Gabriel", "🕵️");
            capitulos.ConcluirFase(Faccao.Reino, Fases.Fase1, Dificuldade.Facil);
            capitulos.DesbloquearFase(Faccao.Reino, Fases.Fase1, Dificuldade.Facil);
            apostolos.DesbloquearApostolos(Faccao.Reino, Fases.Fase1, Dificuldade.Facil);
            arsenal.EquiparItem(arsenal.DroparItens(Faccao.Reino, Fases.Fase1)[0]);
            Assert.True(apostolos.ObterDesbloqueados().Count > 4);

            servico.Excluir();

            Assert.Equal(4, apostolos.ObterDesbloqueados().Count);   // só os Humanos de volta
            Assert.All(apostolos.ObterDesbloqueados(), p => Assert.Equal(Faccao.Humanos, p.Faccao));
            Assert.False(capitulos.FaseConcluida(Faccao.Reino, Fases.Fase1, Dificuldade.Facil));
            Assert.False(capitulos.EstaDesbloqueado(Faccao.Reino, Fases.Fase2, Dificuldade.Facil));
            Assert.Empty(arsenal.ObterObtidos());
            Assert.All(arsenal.ObterEquipados(), item => Assert.Null(item));
        }

        [Fact]
        public void Excluir_ERecarregar_NaoRessuscitaOProgresso()
        {
            var repo = new RepositorioFake();
            var (servico, capitulos, apostolos, arsenal, campanha) = MontarCompleto(repo);

            servico.CriarPerfil("Gabriel", "🕵️");
            capitulos.ConcluirFase(Faccao.Reino, Fases.Fase1, Dificuldade.Facil);
            capitulos.SalvarProgresso();
            campanha.CarregarSaves();

            servico.Excluir();
            campanha.CarregarSaves();   // o boot seguinte, com o save já apagado

            Assert.Equal(4, apostolos.ObterDesbloqueados().Count);
            Assert.False(capitulos.FaseConcluida(Faccao.Reino, Fases.Fase1, Dificuldade.Facil));
            Assert.Empty(arsenal.ObterObtidos());
        }

        [Fact]
        public void AvatarInicial_ESempreDeUmDosHumanos()
        {
            var repo = new RepositorioFake();
            var servico = Montar(repo);
            var humanos = new PersonagemService();

            var simbolosDosHumanos = Enum.GetValues<Slot>()
                .Select(s => humanos.ObterPersonagem(Faccao.Humanos, s).Simbolo)
                .ToHashSet();

            // Sorteado: roda várias vezes pra não passar por sorte.
            for (int i = 0; i < 20; i++)
                Assert.Contains(servico.AvatarInicial(), simbolosDosHumanos);
        }

        [Fact]
        public void PodeUsarAvatar_SoOsDesbloqueados()
        {
            var repo = new RepositorioFake();
            var servico = Montar(repo);
            var personagens = new PersonagemService();

            // Começo de jogo: os 4 Humanos liberados, o resto travado.
            Assert.True(servico.PodeUsarAvatar(personagens.ObterPersonagem(Faccao.Humanos, Slot.Slot1)));
            Assert.False(servico.PodeUsarAvatar(personagens.ObterPersonagem(Faccao.Ascendentes, Slot.Slot4)));
        }

        [Fact]
        public void PodeUsarAvatar_LiberaQuandoACampanhaDesbloqueia()
        {
            var repo = new RepositorioFake();
            var (servico, _, apostolos, _, _) = MontarCompleto(repo);
            var personagens = new PersonagemService();

            // O Guardião: é ele que a fase 1 entrega (a estreia é um por fase, GDD §5).
            Personagem doReino = personagens.ObterPorTipo(Faccao.Reino, TipoDeApostolo.Guardiao);
            Assert.False(servico.PodeUsarAvatar(doReino));

            apostolos.DesbloquearApostolos(Faccao.Reino, Fases.Fase1, Dificuldade.Facil);   // venceu a 1ª fase do Reino

            Assert.True(servico.PodeUsarAvatar(doReino));   // a cara do jogador é troféu de campanha
        }
    }
}
