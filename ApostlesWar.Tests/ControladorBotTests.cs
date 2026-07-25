using ApostlesWar.Application.Controllers;
using ApostlesWar.Application.Services;
using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Ativas;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Debuffs;

namespace Tests
{
    /// <summary>
    /// Testes do cérebro tático (<see cref="ControladorBot"/>) — o que decide o turno do inimigo, do
    /// Bot×Bot e do modo automático.
    ///
    /// É o primeiro pedaço do JOGO DE VERDADE que roda headless: decidir é puro (lê o tabuleiro,
    /// devolve uma escolha) e não chama tela nenhuma, ao contrário do combate. Então aqui dá pra
    /// provar comportamento, não só mecanismo — "não cura quem está inteiro", "prefere o abate".
    ///
    /// Cada teste monta champs de mentira com as habilidades exatas que quer comparar, em vez de usar
    /// champs reais: o roster muda no rebalance e levaria os testes junto.
    /// </summary>
    public class ControladorBotTests
    {
        private static ControladorBot Bot() => new(new SelecaoDeAlvoService());

        private static Combate Champ(int hp = 1000, int atk = 100, int def = 0, params Habilidade[] habs)
        {
            var todas = new List<Habilidade> { new AtaqueBasico() };
            todas.AddRange(habs);
            var c = new Jogador(new Personagem(1, Faccao.Humanos, "T", "🧪", hp, atk, def, todas.ToArray()));
            c.IniciarCombate();
            return c;
        }

        private static HabilidadeAtiva Hab(string nome, params Acao[] acoes)
            => new(nome, "🧪", cooldown: 0, "", numeroDeAlvos: 1, TipoAlvo.Explicito,
                   TipoLista.Inimigos, EstadoAlvo.Vivos, acoes.ToList());

        /// <summary>Habilidade que mira o próprio time (cura, buff, revive).</summary>
        private static HabilidadeAtiva HabAliada(string nome, EstadoAlvo estado, params Acao[] acoes)
            => new(nome, "🧪", cooldown: 0, "", numeroDeAlvos: int.MaxValue, TipoAlvo.Explicito,
                   TipoLista.Aliados, estado, acoes.ToList());

        private static void Ferir(Combate c, int quanto) => c.ReceberDano(quanto, NaturezasDano.DanoIndireto);
        private static void Matar(Combate c) => c.ReceberDano(999_999, NaturezasDano.DanoIndireto);

        // ---------- Passo 1: a habilidade só é usada se tiver o que fazer ----------

        [Fact]
        public void NaoCura_QuandoOTimeEstaInteiro()
        {
            var curar = HabAliada("Curar", EstadoAlvo.Vivos, new Cura(Valor.Fixo(300)));
            var bot = Champ(habs: curar);
            var inimigo = Champ();

            var escolha = bot.Personagem.Habilidades.Count == 0 ? null
                : Bot().EscolherAcao(bot, new() { bot }, new() { inimigo });

            Assert.IsType<AtaqueBasico>(escolha);   // sem ninguém ferido, a cura não serve: bate
        }

        [Fact]
        public void Cura_QuandoAlguemPerdeuVidaSuficiente()
        {
            var curar = HabAliada("Curar", EstadoAlvo.Vivos, new Cura(Valor.Fixo(300)));
            var bot = Champ(habs: curar);
            var ferido = Champ();
            Ferir(ferido, 500);                     // 50% da vida — bem acima da margem
            var inimigo = Champ();

            var escolha = Bot().EscolherAcao(bot, new() { bot, ferido }, new() { inimigo });

            Assert.Same(curar, escolha);
        }

        [Fact]
        public void Cura_EIgnorada_PorArranhaoAbaixoDaMargem()
        {
            var curar = HabAliada("Curar", EstadoAlvo.Vivos, new Cura(Valor.Fixo(300)));
            var bot = Champ(habs: curar);
            var arranhado = Champ();
            Ferir(arranhado, 50);                   // 5% — não paga o turno
            var inimigo = Champ();

            var escolha = Bot().EscolherAcao(bot, new() { bot, arranhado }, new() { inimigo });

            Assert.IsType<AtaqueBasico>(escolha);
        }

        [Fact]
        public void NaoRevive_SemMortos_MasRevive_QuandoHaUm()
        {
            var reviver = HabAliada("Reviver", EstadoAlvo.Mortos, new Reviver(0.5, Escopo.AlvosResolvidos));
            var bot = Champ(habs: reviver);
            var aliado = Champ();
            var inimigo = Champ();

            Assert.IsType<AtaqueBasico>(Bot().EscolherAcao(bot, new() { bot, aliado }, new() { inimigo }));

            Matar(aliado);

            Assert.Same(reviver, Bot().EscolherAcao(bot, new() { bot, aliado }, new() { inimigo }));
        }

        [Fact]
        public void NaoLimpaDebuff_SemDebuff_MasLimpa_QuandoHaUm()
        {
            var limpar = HabAliada("Limpar", EstadoAlvo.Vivos, new RemoverDebuffs(Seletor.Todos()));
            var bot = Champ(habs: limpar);
            var aliado = Champ();
            var inimigo = Champ();

            Assert.IsType<AtaqueBasico>(Bot().EscolherAcao(bot, new() { bot, aliado }, new() { inimigo }));

            new Veneno(stacks: 1).Aplicar(aliado);

            Assert.Same(limpar, Bot().EscolherAcao(bot, new() { bot, aliado }, new() { inimigo }));
        }

        [Fact]
        public void NaoExplode_SemOStatusDetonavel()
        {
            var explodir = Hab("Explodir", new Explodir(Seletor.Tipo<Veneno>()));
            var bot = Champ(habs: explodir);
            var inimigo = Champ(hp: 100_000);

            Assert.IsType<AtaqueBasico>(Bot().EscolherAcao(bot, new() { bot }, new() { inimigo }));
        }

        /// <summary>
        /// A explosão só ganha do ataque básico se detonar MAIS vida que ele — e o bot compara os
        /// dois números de verdade. Como o veneno é % do HP máximo, um alvo grande com vários stacks
        /// vale muito mais que o A1; foi assim que este teste pegou o bot preferindo bater quando o
        /// veneno era 1 stack num alvo pequeno (ele estava certo, o teste é que pedia a coisa errada).
        /// </summary>
        /// <summary>
        /// A pergunta do Gabriel: se a habilidade de explosão faz MAIS coisas, o bot ainda prefere o
        /// A1? Não — e as duas explosões reais do jogo ganham por motivos diferentes.
        ///
        /// Putrefação (Zumbi) = Dano + Explodir + Cura, em ÁREA: mesmo sem veneno no campo (explosão
        /// prevendo 0) e com o time inteiro (cura sem serventia), ela empata com o A1 em "Ferir" e
        /// vence no desempate de alcance — área bate alvo único.
        ///
        /// Inferno (Diabo) = AplicarDebuff + Explodir: o debuff a coloca em "Enfraquecer", acima de
        /// "Ferir", então ela nem chega a ser comparada por dano.
        ///
        /// Só perderia pro A1 uma explosão SECA de alvo único — forma que não existe no roster.
        /// </summary>
        [Fact]
        public void Explosao_QueFazMaisCoisas_GanhaDoA1_MesmoSemNadaPraDetonar()
        {
            var putrefacao = new HabilidadeAtiva("Putrefação", "💀", cooldown: 0, "",
                numeroDeAlvos: int.MaxValue, TipoAlvo.Explicito, TipoLista.Inimigos, EstadoAlvo.Vivos,
                new List<Acao> { new Dano(1.0), new Explodir(Seletor.Tipo<Veneno>()) });

            var inferno = new HabilidadeAtiva("Inferno", "🔥", cooldown: 0, "",
                numeroDeAlvos: int.MaxValue, TipoAlvo.Explicito, TipoLista.Inimigos, EstadoAlvo.Vivos,
                new List<Acao>
                {
                    new AplicarDebuff(() => new Queima(stacks: 2), Escopo.TodosInimigos),
                    new Explodir(Seletor.Tipo<Queima>(), Escopo.TodosInimigos),
                });

            var inimigo = Champ(hp: 5000);   // sem veneno e sem queima: as explosões preveem 0

            var comPutrefacao = Champ(habs: putrefacao);
            Assert.Same(putrefacao, Bot().EscolherAcao(comPutrefacao, new() { comPutrefacao }, new() { inimigo }));

            var comInferno = Champ(habs: inferno);
            Assert.Same(inferno, Bot().EscolherAcao(comInferno, new() { comInferno }, new() { inimigo }));
        }

        [Fact]
        public void Explode_QuandoADetonacaoTiraMaisVidaQueOAtaque()
        {
            var explodir = Hab("Explodir", new Explodir(Seletor.Tipo<Veneno>()));
            var bot = Champ(atk: 100, habs: explodir);
            var inimigo = Champ(hp: 100_000);
            new Veneno(stacks: 5).Aplicar(inimigo);

            Assert.Same(explodir, Bot().EscolherAcao(bot, new() { bot }, new() { inimigo }));
        }

        [Fact]
        public void NaoAplicaDebuff_SeTodosOsAlvosSaoImunes()
        {
            var debuffar = Hab("Debuffar", new AplicarDebuff(() => new Veneno(stacks: 1)));
            var bot = Champ(habs: debuffar);
            var imune = Champ();
            new ImunidadeDebuffs(duracao: 5).Aplicar(imune);

            var escolha = Bot().EscolherAcao(bot, new() { bot }, new() { imune });

            Assert.IsType<AtaqueBasico>(escolha);
        }

        [Fact]
        public void NaoRoubaBuff_SeOInimigoNaoTemNenhum()
        {
            var roubar = Hab("Roubar", new MoverBuffs(Seletor.Todos()));
            var bot = Champ(habs: roubar);
            var inimigo = Champ();

            Assert.IsType<AtaqueBasico>(Bot().EscolherAcao(bot, new() { bot }, new() { inimigo }));

            new BuffAtaque(duracao: 2, percentual: 0.2).Aplicar(inimigo);

            Assert.Same(roubar, Bot().EscolherAcao(bot, new() { bot }, new() { inimigo }));
        }

        // ---------- Passo 1b: a fila de prioridade ----------

        [Fact]
        public void Prioridade_ReviverVenceCurar_QueVenceBater()
        {
            var reviver = HabAliada("Reviver", EstadoAlvo.Mortos, new Reviver(0.5, Escopo.AlvosResolvidos));
            var curar = HabAliada("Curar", EstadoAlvo.Vivos, new Cura(Valor.Fixo(300)));
            var bot = Champ(habs: new Habilidade[] { curar, reviver });
            var morto = Champ(); Matar(morto);
            var ferido = Champ(); Ferir(ferido, 500);
            var inimigo = Champ();

            var escolha = Bot().EscolherAcao(bot, new() { bot, morto, ferido }, new() { inimigo });

            Assert.Same(reviver, escolha);   // com as duas servindo, o revive vem primeiro
        }

        /// <summary>
        /// O caso do Papai Noel: entre duas habilidades da MESMA utilidade, ganha a que faz mais
        /// coisas úteis agora.
        /// </summary>
        [Fact]
        public void Desempate_AHabilidadeQueFazMaisCoisasVence()
        {
            var soDano = Hab("Só dano", new Dano(1.0));
            var danoEDebuff = Hab("Dano + debuff", new Dano(1.0), new AplicarDebuff(() => new Veneno(stacks: 1)));
            var bot = Champ(habs: new Habilidade[] { soDano, danoEDebuff });
            var inimigo = Champ();

            var escolha = Bot().EscolherAcao(bot, new() { bot }, new() { inimigo });

            // A rica entra pela utilidade mais alta que tem (Enfraquecer > Ferir), então vence direto.
            Assert.Same(danoEDebuff, escolha);
        }

        [Fact]
        public void TurnoExtra_NaoFuraAFila_MasVenceOAtaqueSeco()
        {
            var soTurnoExtra = Hab("Fôlego", new ConcederTurnoExtra());
            var curar = HabAliada("Curar", EstadoAlvo.Vivos, new Cura(Valor.Fixo(300)));
            var bot = Champ(habs: new Habilidade[] { soTurnoExtra, curar });
            var ferido = Champ(); Ferir(ferido, 500);
            var inimigo = Champ();

            // Com cura pendente, o turno extra espera.
            Assert.Same(curar, Bot().EscolherAcao(bot, new() { bot, ferido }, new() { inimigo }));

            // Sem nada melhor, ele vale mais que só bater.
            var saudavel = Champ();
            Assert.Same(soTurnoExtra, Bot().EscolherAcao(bot, new() { bot, saudavel }, new() { inimigo }));
        }

        [Fact]
        public void HabilidadeEmCooldown_NaoEConsiderada()
        {
            var forte = new HabilidadeAtiva("Forte", "🧪", cooldown: 3, "", 1, TipoAlvo.Explicito,
                TipoLista.Inimigos, EstadoAlvo.Vivos, new List<Acao> { new Dano(5.0) });
            var bot = Champ(habs: forte);
            var inimigo = Champ();
            bot.Cooldowns[forte].Usar();

            Assert.IsType<AtaqueBasico>(Bot().EscolherAcao(bot, new() { bot }, new() { inimigo }));
        }

        // ---------- Passo 2: a escolha do alvo ----------

        [Fact]
        public void Alvo_PrefereQuemMorre_AQuemPerderiaMaisVida()
        {
            var bot = Champ(atk: 300);
            var quaseMorto = Champ(hp: 1000); Ferir(quaseMorto, 900);   // 100 de vida: MORRE
            var gordo = Champ(hp: 5000);                                 // levaria mais dano, sobrevive
            var inimigos = new List<Combate> { gordo, quaseMorto };

            var controlador = Bot();
            controlador.EscolherAcao(bot, new() { bot }, inimigos);
            var alvo = controlador.EscolherAlvo(inimigos, new() { bot }, inimigos);

            Assert.Same(quaseMorto, alvo);
        }

        /// <summary>
        /// Bloqueio total não precisa de regra "evite este alvo": ele simplesmente não perde vida,
        /// então perde na comparação sozinho.
        /// </summary>
        [Fact]
        public void Alvo_EvitaOBloqueado_SemRegraEspecial()
        {
            var bot = Champ(atk: 300);
            var bloqueado = Champ(hp: 1000);
            new BloqueioTotal(duracao: 2).Aplicar(bloqueado);
            var normal = Champ(hp: 5000);
            var inimigos = new List<Combate> { bloqueado, normal };

            var controlador = Bot();
            controlador.EscolherAcao(bot, new() { bot }, inimigos);
            var alvo = controlador.EscolherAlvo(inimigos, new() { bot }, inimigos);

            Assert.Same(normal, alvo);
        }

        [Fact]
        public void Alvo_EvitaOInvencivel_PorqueEleNaoPerdeVidaDeVerdade()
        {
            var bot = Champ(atk: 300);
            var invencivel = Champ(hp: 1000); Ferir(invencivel, 999);   // 1 de vida
            new Invencivel(duracao: 2).Aplicar(invencivel);              // piso de HP = 1 → remove 0
            var normal = Champ(hp: 5000);
            var inimigos = new List<Combate> { invencivel, normal };

            var controlador = Bot();
            controlador.EscolherAcao(bot, new() { bot }, inimigos);
            var alvo = controlador.EscolherAlvo(inimigos, new() { bot }, inimigos);

            Assert.Same(normal, alvo);
        }

        /// <summary>A ordem de fuga do Gabriel: espinhos pior que contra-ataque, pior que reflexo.</summary>
        [Theory]
        [InlineData(true)]    // espinhos vs contra-ataque → foge dos espinhos
        [InlineData(false)]   // contra-ataque vs reflexo  → foge do contra-ataque
        public void Alvo_FogeDaPunicaoMaisCara_PrimeiroOsEspinhos(bool espinhosContraCA)
        {
            var bot = Champ(atk: 300);
            var pior = Champ(hp: 5000);
            var melhor = Champ(hp: 5000);

            if (espinhosContraCA)
            {
                new EspinhosVenenosos(duracao: 5).Aplicar(pior);
                new ContraAtaque(duracao: 5).Aplicar(melhor);
            }
            else
            {
                new ContraAtaque(duracao: 5).Aplicar(pior);
                new RefletirDano(duracao: 5).Aplicar(melhor);
            }

            var inimigos = new List<Combate> { pior, melhor };
            var controlador = Bot();
            controlador.EscolherAcao(bot, new() { bot }, inimigos);

            Assert.Same(melhor, controlador.EscolherAlvo(inimigos, new() { bot }, inimigos));
        }

        [Fact]
        public void Alvo_PrefereQuemNaoPuneNada()
        {
            var bot = Champ(atk: 300);
            var espinhoso = Champ(hp: 5000);
            new EspinhosVenenosos(duracao: 5).Aplicar(espinhoso);
            var limpo = Champ(hp: 5000);
            var inimigos = new List<Combate> { espinhoso, limpo };

            var controlador = Bot();
            controlador.EscolherAcao(bot, new() { bot }, inimigos);

            Assert.Same(limpo, controlador.EscolherAlvo(inimigos, new() { bot }, inimigos));
        }

        /// <summary>
        /// O motor filtra a lista de alvos (Provocar, bloqueio, estado) DEPOIS de perguntar a ação.
        /// Se o eleito não estiver mais lá, quem manda é a lista do motor — nunca a memória do bot.
        /// </summary>
        [Fact]
        public void Alvo_CedeAListaDoMotor_QuandoOEleitoNaoEstaNela()
        {
            var bot = Champ(atk: 300);
            var eleito = Champ(hp: 1000); Ferir(eleito, 900);
            var unicoPermitido = Champ(hp: 5000);
            var todos = new List<Combate> { eleito, unicoPermitido };

            var controlador = Bot();
            controlador.EscolherAcao(bot, new() { bot }, todos);

            var alvo = controlador.EscolherAlvo(new() { unicoPermitido }, new() { bot }, todos);

            Assert.Same(unicoPermitido, alvo);
        }
    }
}
