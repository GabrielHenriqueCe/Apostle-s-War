using ApostlesWar.Application.Portas;
using ApostlesWar.Application.Services;
using ApostlesWar.Domain;

namespace Tests
{
    /// <summary>
    /// Testes do <see cref="CampanhaService"/> (a recompensa da vitória: unlock/drop/save) e do
    /// <see cref="ArsenalService"/>. É PURO (sem combate/TTY), então roda headless com um
    /// repositório fake — nenhum destes services pede tela.
    /// </summary>
    public class CampanhaServiceTests
    {
        // Um apóstolo qualquer pra vestir as peças: com o vínculo, equipar exige DE QUEM.
        // A identidade é (Facção, Slot), então a instância deste roster serve pra qualquer service.
        private static readonly Personagem Portador =
            new PersonagemService().ObterPersonagem(Faccao.Reino, Slot.Slot1);

        private sealed class RepositorioFake : IRepositorioDeSave
        {
            private readonly Dictionary<string, object?> _dados = new();
            public void Salvar<T>(string chave, T dado) => _dados[chave] = dado;
            public T? Carregar<T>(string chave) => _dados.TryGetValue(chave, out var v) && v is T t ? t : default;
            public void Excluir(string chave) => _dados.Remove(chave);
            public bool Contem(string chave) => _dados.ContainsKey(chave);
        }

        private static (CampanhaService campanha, CapitulosService capitulos, ApostolosService apostolos)
            Montar()
        {
            var repo = new RepositorioFake();
            var capitulos = new CapitulosService(repo);
            var arsenal = new ArsenalService(capitulos, new PoService(repo), new PersonagemService(), repo);
            var apostolos = new ApostolosService(new PersonagemService(), capitulos);
            return (new CampanhaService(arsenal, apostolos, capitulos, new PersonagemService(), new ProgressaoService(new PersonagemService(), new AlmaService(repo), repo), repo), capitulos, apostolos);
        }

        [Fact]
        public void ProcessarVitoria_Reino1_DesbloqueiaProximaFase_ApostolosEItem()
        {
            var (campanha, capitulos, apostolos) = Montar();

            Assert.Equal(4, apostolos.ObterDesbloqueados().Count);              // só os 4 Humanos no começo
            Assert.False(capitulos.EstaDesbloqueado(Faccao.Reino, Fases.Fase2, Dificuldade.Facil)); // fase 2 ainda travada

            RecompensaDaFase r = campanha.ProcessarVitoria(Faccao.Reino, Fases.Fase1, Dificuldade.Facil);

            Assert.True(capitulos.FaseConcluida(Faccao.Reino, Fases.Fase1, Dificuldade.Facil));   // marcou concluída
            Assert.True(capitulos.EstaDesbloqueado(Faccao.Reino, Fases.Fase2, Dificuldade.Facil)); // liberou a próxima
            // Reino fase 1: as duas rodadas são o Guardião (o Guarda) → UM apóstolo novo. A fase
            // estreia um por vez, e o time do capítulo só fecha em quatro na fase 4.
            Assert.Single(r.NovosApostolos);
            Assert.Equal("Guarda", r.NovosApostolos[0].Nome);
            // A fase larga QUATRO peças do slot dela — o time inteiro armado numa corrida só.
            Assert.Equal(ArsenalService.ItensPorFase, r.Itens.Count);
            Assert.All(r.Itens, i => Assert.Equal("Arma", i.Nome));
            Assert.All(r.Itens, i => Assert.Equal(1, i.Nivel));   // todo drop nasce no nível 1
            Assert.Equal(5, apostolos.ObterDesbloqueados().Count);              // 4 Humanos + 1 novo
        }

        [Fact]
        public void ProcessarVitoria_Persiste_SaveEItens()
        {
            var repo = new RepositorioFake();
            var capitulos = new CapitulosService(repo);
            var arsenal = new ArsenalService(capitulos, new PoService(repo), new PersonagemService(), repo);
            var apostolos = new ApostolosService(new PersonagemService(), capitulos);
            var campanha = new CampanhaService(arsenal, apostolos, capitulos, new PersonagemService(), new ProgressaoService(new PersonagemService(), new AlmaService(repo), repo), repo);

            campanha.ProcessarVitoria(Faccao.Reino, Fases.Fase1, Dificuldade.Facil);

            Assert.True(repo.Contem("save"));
            Assert.True(repo.Contem("inventario"));
        }

        /// <summary>
        /// Cada peça que cai é uma INSTÂNCIA, não uma casa de catálogo: repetir a fase larga mais
        /// quatro, e nenhuma delas é a mesma da leva anterior. É o que a forja vai consumir.
        /// </summary>
        [Fact]
        public void DroparItens_CadaPecaEUmaInstancia_ERepetirAFaseLargaMais()
        {
            var repo = new RepositorioFake();
            var arsenal = new ArsenalService(new CapitulosService(repo), new PoService(repo), new PersonagemService(), repo);

            var primeira = arsenal.DroparItens(Faccao.Reino, Fases.Fase1);
            var segunda = arsenal.DroparItens(Faccao.Reino, Fases.Fase1);

            Assert.Equal(ArsenalService.ItensPorFase, primeira.Count);
            Assert.Equal(ArsenalService.ItensPorFase * 2, arsenal.ObterObtidos().Count);
            Assert.All(primeira, i => Assert.Equal("🗡️", i.Simbolo));

            // Oito peças, oito identidades — nenhuma repetida.
            var ids = primeira.Concat(segunda).Select(i => i.Id).ToList();
            Assert.Equal(ids.Count, ids.Distinct().Count());
        }

        /// <summary>
        /// O principal dos quatro slots de PERCENTUAL é sorteado; o dos três de valor cheio é fixo.
        /// É o sorteio que faz duas cópias do mesmo slot serem duas decisões diferentes.
        /// </summary>
        [Fact]
        public void DroparItens_OPrincipalSaiSempreDaListaDoSlot()
        {
            var repo = new RepositorioFake();
            var arsenal = new ArsenalService(new CapitulosService(repo), new PoService(repo), new PersonagemService(), repo);

            foreach (Fases fase in Enum.GetValues<Fases>())
            {
                var opcoes = Equipamento.OpcoesDoSlot(fase);
                Assert.All(arsenal.DroparItens(Faccao.Reino, fase), i => Assert.Contains(i.TipoStat, opcoes));
            }
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
            var arsenal = new ArsenalService(new CapitulosService(repo), new PoService(repo), new PersonagemService(), repo);

            foreach (Fases fase in Enum.GetValues<Fases>())
                Assert.All(arsenal.DroparItens(Faccao.Reino, fase),
                    i => Assert.Equal(Equipamento.NomeDoSlot(fase), i.Nome));
        }

        [Fact]
        public void EquiparItem_PersisteSozinho()
        {
            var repo = new RepositorioFake();
            var arsenal = new ArsenalService(new CapitulosService(repo), new PoService(repo), new PersonagemService(), repo);

            arsenal.EquiparItem(Portador, arsenal.DroparItens(Faccao.Reino, Fases.Fase1)[0]);

            // Sem ninguém chamar SalvarItens: quem manda no dado decide quando ele é durável.
            Assert.True(repo.Contem("inventario"));
            Assert.True(repo.Contem("vestidos"));
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

        // ---------- Onde o jogador parou: fase e time ----------

        [Fact]
        public void UltimaFase_SemSave_ComecaNaPrimeira()
        {
            var (campanha, _, _) = Montar();

            Assert.Equal(Fases.Fase1, campanha.UltimaFaseDe(Faccao.Reino, Dificuldade.Facil));
        }

        /// <summary>
        /// A memória é POR CAPÍTULO: o jogador vai e volta entre eles, e cada um tem a própria
        /// história. Uma memória global faria voltar pro Reino na fase em que ele parou no Lado
        /// Sombrio.
        /// </summary>
        [Fact]
        public void UltimaFase_EPorCapitulo()
        {
            var (campanha, capitulos, _) = Montar();
            capitulos.DesbloquearFase(Faccao.Reino, Fases.Fase1, Dificuldade.Facil);   // libera a 2

            campanha.SalvarEntradaNaFase(Faccao.Reino, Fases.Fase2, Dificuldade.Facil, new List<Personagem>());

            Assert.Equal(Fases.Fase2, campanha.UltimaFaseDe(Faccao.Reino, Dificuldade.Facil));
            Assert.Equal(Fases.Fase1, campanha.UltimaFaseDe(Faccao.LadoSombrio, Dificuldade.Facil));   // intocado
        }

        /// <summary>
        /// Lembra a fase em que ENTROU, não a que venceu: quem apanhou quer voltar naquela fase, não
        /// na seguinte. Aqui a fase 2 nunca foi vencida (segue travada) — e por isso a memória dela
        /// não vale: cai na 1, em vez de abrir numa fase que o jogador não pode jogar.
        /// </summary>
        [Fact]
        public void UltimaFase_SeATravaram_CaiNaPrimeira()
        {
            var (campanha, _, _) = Montar();

            campanha.SalvarEntradaNaFase(Faccao.Reino, Fases.Fase2, Dificuldade.Facil, new List<Personagem>());

            Assert.Equal(Fases.Fase1, campanha.UltimaFaseDe(Faccao.Reino, Dificuldade.Facil));
        }

        [Fact]
        public void UltimoTime_VoltaOsMesmosApostolos()
        {
            var (campanha, _, apostolos) = Montar();
            var time = apostolos.ObterDesbloqueados().Take(2).ToList();

            campanha.SalvarEntradaNaFase(Faccao.Reino, Fases.Fase1, Dificuldade.Facil, time);

            var voltou = campanha.UltimoTime();
            Assert.Equal(2, voltou.Count);
            Assert.Equal(time.Select(p => p.Nome), voltou.Select(p => p.Nome));
        }

        /// <summary>
        /// O time atravessa o fechar do jogo: um service NOVO lendo o mesmo save encontra o que o
        /// anterior deixou. É a diferença entre lembrar e só não ter esquecido ainda.
        /// </summary>
        [Fact]
        public void UltimoTime_SobreviveAoServiceQueOEscreveu()
        {
            var repo = new RepositorioFake();
            var capitulos = new CapitulosService(repo);
            var arsenal = new ArsenalService(capitulos, new PoService(repo), new PersonagemService(), repo);
            var apostolos = new ApostolosService(new PersonagemService(), capitulos);
            var time = apostolos.ObterDesbloqueados().Take(3).ToList();

            new CampanhaService(arsenal, apostolos, capitulos, new PersonagemService(), new ProgressaoService(new PersonagemService(), new AlmaService(repo), repo), repo)
                .SalvarEntradaNaFase(Faccao.Reino, Fases.Fase1, Dificuldade.Facil, time);

            var outro = new CampanhaService(arsenal, apostolos, capitulos, new PersonagemService(), new ProgressaoService(new PersonagemService(), new AlmaService(repo), repo), repo);
            Assert.Equal(time.Select(p => p.Nome), outro.UltimoTime().Select(p => p.Nome));
        }

        /// <summary>
        /// Apóstolo que não está liberado não volta pro time. O estrago seria a tela de fases montar um
        /// slot com alguém que o jogador não tem, e o back recusar o "Lutar" sem explicar.
        ///
        /// O teste salva um time MISTO de propósito — um Humano (liberado desde sempre) e o Guarda
        /// (que só entra ao vencer o Reino 1) — porque o wipe não serviria de cenário: ele apaga o
        /// save junto, e aí o vazio provaria só que o save sumiu, não que o filtro existe.
        /// </summary>
        [Fact]
        public void UltimoTime_IgnoraQuemNaoEstaLiberado()
        {
            var (campanha, _, apostolos) = Montar();
            var personagens = new PersonagemService();
            Personagem humano = apostolos.ObterDesbloqueados().First();
            Personagem trancado = personagens.ObterPersonagem(Faccao.Reino, Slot.Slot1);

            Assert.False(apostolos.EstaDesbloqueado(trancado));   // a premissa do teste

            campanha.SalvarEntradaNaFase(Faccao.Reino, Fases.Fase1, Dificuldade.Facil, new List<Personagem> { humano, trancado });

            Personagem sobrou = Assert.Single(campanha.UltimoTime());
            Assert.Equal(humano.Nome, sobrou.Nome);
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
            Assert.Equal(Faccao.Ascendentes, faccoes[^1]);    // e termina nos Ascendentes
        }
    }
}
