using ApostlesWar;
using ApostlesWar.Skills;
using ApostlesWar.Skills.Ativas;
using ApostlesWar.Skills.Buffs;
using ApostlesWar.Skills.Debuffs;

namespace Tests
{
    /// <summary>
    /// Testes do MOTOR de habilidades (interpretador de Acoes — ADR-composicao-de-acoes §3).
    /// Cobrem o contrato de que TODAS as habilidades declarativas dependem: escopo por ação,
    /// EstadoAlvo avaliado na execução, ordem das ações, agregação via eventos e a convivência
    /// Strangler com subclasses que ainda sobrescrevem Ativar.
    ///
    /// O crítico do Atacar é aleatório — asserts de valor de dano usam os próprios EventoDano
    /// retornados (determinístico independente do crit). Cura/ferimento usam DanoIndireto com
    /// defesa 0, então os números são exatos.
    /// </summary>
    public class MotorDeHabilidadesTests
    {
        // ---------- helpers ----------

        private static Combate Novo(int hp = 1000, int atk = 200)
            => new Jogador(new Personagem(1, Faccao.Humanos, "Teste", "🧪", hp, atk, 0));

        private static HabilidadeAtiva Hab(List<Acao> acoes, int alvos,
            TipoLista lista = TipoLista.Inimigos, TipoAlvo modo = TipoAlvo.Explicito,
            EstadoAlvo estado = EstadoAlvo.Vivos)
            => new("Teste", "🧪", 3, "hab de teste", alvos, modo, lista, estado, acoes);

        private static void Ferir(Combate c, int dano)
            => c.ReceberDano(dano, NaturezasDano.DanoIndireto);

        private static void Matar(Combate c)
            => c.ReceberDano(1_000_000, NaturezasDano.DanoIndireto);

        // ---------- Escopo ----------

        [Fact]
        public void EscopoProprioAtacante_BuffCaiSoNoAtacante_EDanoNosAlvos()
        {
            var atacante = Novo(); var aliado = Novo();
            var inimigo1 = Novo(hp: 5000); var inimigo2 = Novo(hp: 5000);
            var ctx = new ContextoCombate(atacante,
                new List<Combate> { atacante, aliado },
                new List<Combate> { inimigo1, inimigo2 });

            // Furtividade-like: Intocável em SI, depois dano em todos os inimigos.
            var hab = Hab(new()
            {
                new AplicarBuff(() => new Intocavel(turnos: 2), Escopo.ProprioAtacante),
                new Dano(1.0),
            }, alvos: int.MaxValue);

            var eventos = hab.Ativar(ctx, inimigo1);

            Assert.True(atacante.StatusAtivos.OfType<Intocavel>().Any());
            Assert.False(aliado.StatusAtivos.OfType<Intocavel>().Any());
            Assert.False(inimigo1.StatusAtivos.OfType<Intocavel>().Any());
            Assert.Equal(2, eventos.Count);
            Assert.True(inimigo1.HPAtual < 5000 && inimigo2.HPAtual < 5000);
        }

        [Fact]
        public void EscopoTodosInimigos_IgnoraOPickDaHabilidade()
        {
            var atacante = Novo();
            var inimigo1 = Novo(); var inimigo2 = Novo();
            var ctx = new ContextoCombate(atacante,
                new List<Combate> { atacante },
                new List<Combate> { inimigo1, inimigo2 });

            // Habilidade de 1 alvo, mas a AÇÃO declara escopo TodosInimigos:
            // a Queima cai nos dois, apesar do pick único.
            var hab = Hab(new()
            {
                new AplicarDebuff(() => new Queima(2), Escopo.TodosInimigos),
            }, alvos: 1);

            hab.Ativar(ctx, inimigo1);

            Assert.True(inimigo1.StatusAtivos.OfType<Queima>().Any());
            Assert.True(inimigo2.StatusAtivos.OfType<Queima>().Any());
        }

        [Fact]
        public void EscopoTodosAliados_CuraPorHPDoAlvo_ComCapNoMaximo()
        {
            var atacante = Novo(hp: 1000);              // cheio — a cura capa no máximo
            var aliado = Novo(hp: 800); Ferir(aliado, 400); // 400/800 — cura usa o HP DELE
            var ctx = new ContextoCombate(atacante,
                new List<Combate> { atacante, aliado },
                new List<Combate> { Novo() });

            // Sushi-like: cura 30% do HP máximo DE CADA ALIADO (fragmento PorHP).
            var hab = Hab(new()
            {
                new Cura(Valor.PorHP(0.30), Escopo.TodosAliados),
            }, alvos: int.MaxValue, lista: TipoLista.Aliados);

            hab.Ativar(ctx, atacante);

            Assert.Equal(1000, atacante.HPAtual);        // capado no máximo
            Assert.Equal(400 + 240, aliado.HPAtual);     // 30% de 800, não de 1000
        }

        [Fact]
        public void EscopoOutrosAliados_AtingeAliadosMenosOProprioAtacante()
        {
            var atacante = Novo();
            var aliado1 = Novo(); var aliado2 = Novo();
            var ctx = new ContextoCombate(atacante,
                new List<Combate> { atacante, aliado1, aliado2 },
                new List<Combate> { Novo() });

            // OssoDuroDeRoer-like: buff em todos os aliados EXCETO quem conjurou.
            var hab = Hab(new()
            {
                new AplicarBuff(() => new Intocavel(turnos: 2), Escopo.OutrosAliados),
            }, alvos: int.MaxValue, lista: TipoLista.Aliados);

            hab.Ativar(ctx, atacante);

            Assert.False(atacante.StatusAtivos.OfType<Intocavel>().Any());
            Assert.True(aliado1.StatusAtivos.OfType<Intocavel>().Any());
            Assert.True(aliado2.StatusAtivos.OfType<Intocavel>().Any());
        }

        // ---------- EstadoAlvo avaliado na EXECUÇÃO ----------

        [Fact]
        public void EstadoNaExecucao_RecemMortoNaoRecebeDebuffDeVivos()
        {
            var atacante = Novo(atk: 200);
            var inimigo = Novo(hp: 1);                   // qualquer dano mata
            var ctx = new ContextoCombate(atacante,
                new List<Combate> { atacante },
                new List<Combate> { inimigo });

            var hab = Hab(new()
            {
                new Dano(1.0),
                new AplicarDebuff(() => new Queima(2)),  // Vivos (default) — pula o cadáver
            }, alvos: 1);

            hab.Ativar(ctx, inimigo);

            Assert.False(inimigo.EstaVivo());
            Assert.Empty(inimigo.StatusAtivos);          // a Queima NÃO caiu no morto
        }

        [Fact]
        public void EstadoMortos_AcaoPegaQuemODanoAcabouDeMatar()
        {
            var atacante = Novo(atk: 200);
            var inimigo = Novo(hp: 1);
            var ctx = new ContextoCombate(atacante,
                new List<Combate> { atacante },
                new List<Combate> { inimigo });

            // O Barata sem condicional: "se matou, Sentença" vira AplicarDebuff(Mortos).
            var hab = Hab(new()
            {
                new Dano(1.0),
                new AplicarDebuff(() => new ImpedirRessurreicao(), estadoAlvo: EstadoAlvo.Mortos),
            }, alvos: 1);

            hab.Ativar(ctx, inimigo);

            Assert.False(inimigo.EstaVivo());
            Assert.True(inimigo.StatusAtivos.OfType<ImpedirRessurreicao>().Any());

            inimigo.Reviver(500);                        // a Sentença bloqueia central (Morto.Reviver)
            Assert.False(inimigo.EstaVivo());
        }

        [Fact]
        public void EstadoAmbos_SemFiltro_AtingeVivosEMortos()
        {
            var atacante = Novo();
            var aliadoVivo = Novo();
            var aliadoMorto = Novo(); Matar(aliadoMorto);
            var ctx = new ContextoCombate(atacante,
                new List<Combate> { atacante, aliadoVivo, aliadoMorto },
                new List<Combate> { Novo() });

            var hab = Hab(new()
            {
                new AplicarBuff(() => new BuffAtaque(), Escopo.TodosAliados, EstadoAlvo.Ambos),
            }, alvos: int.MaxValue, lista: TipoLista.Aliados);

            hab.Ativar(ctx, atacante);

            Assert.True(aliadoVivo.StatusAtivos.OfType<BuffAtaque>().Any());
            Assert.True(aliadoMorto.StatusAtivos.OfType<BuffAtaque>().Any()); // na lista do Morto
        }

        // ---------- Ordem + agregação ----------

        [Fact]
        public void Agregacao_AcaoPosteriorLeODanoTotalDosEventos()
        {
            var atacante = Novo(hp: 1000); Ferir(atacante, 500);   // 500/1000
            var inimigo1 = Novo(hp: 5000); var inimigo2 = Novo(hp: 5000);
            var ctx = new ContextoCombate(atacante,
                new List<Combate> { atacante },
                new List<Combate> { inimigo1, inimigo2 });

            // Quebrar-like: o valor da ação final deriva do dano SOMADO das anteriores.
            // Ação-por-fora garante que o Dano termina TODA a passada antes da Cura ler.
            var hab = Hab(new()
            {
                new Dano(1.0),
                new Cura(Valor.PorDanoCausado(0.5), Escopo.ProprioAtacante),
            }, alvos: int.MaxValue);

            var eventos = hab.Ativar(ctx, inimigo1);

            int somaDano = eventos.Sum(e => e.DanoEfetivo);
            Assert.Equal(2, eventos.Count);
            Assert.True(somaDano > 0);
            Assert.Equal(500 + (int)(somaDano * 0.5), atacante.HPAtual);
        }

        // ---------- Resolução de alvos ----------

        [Fact]
        public void AlvosResolvidos_SingleTarget_AtingeSoOEscolhido()
        {
            var atacante = Novo();
            var inimigo1 = Novo(hp: 5000); var inimigo2 = Novo(hp: 5000);
            var ctx = new ContextoCombate(atacante,
                new List<Combate> { atacante },
                new List<Combate> { inimigo1, inimigo2 });

            var hab = Hab(new() { new Dano(1.0) }, alvos: 1);

            var eventos = hab.Ativar(ctx, inimigo1);

            Assert.Single(eventos);
            Assert.True(inimigo1.HPAtual < 5000);
            Assert.Equal(5000, inimigo2.HPAtual);
        }

        [Fact]
        public void TipoAlvoAleatorio_ComUmVivoSo_AcertaOMesmoDuasVezes()
        {
            var atacante = Novo();
            var inimigo = Novo(hp: 5000);
            var ctx = new ContextoCombate(atacante,
                new List<Combate> { atacante },
                new List<Combate> { inimigo });

            // Tiroteio-like: 2 alvos aleatórios com repetição — com 1 vivo, 2 hits nele.
            var hab = Hab(new() { new Dano(0.75) }, alvos: 2, modo: TipoAlvo.Aleatorio);

            var eventos = hab.Ativar(ctx, inimigo);

            Assert.Equal(2, eventos.Count);
            Assert.All(eventos, e => Assert.Same(inimigo, e.Alvo));
        }

        // ---------- Reviver (família do revive — ADR §9) ----------

        [Fact]
        public void Reviver_DoisEstadosEmOrdem_OsRevividosPegamOBuffDosVivos()
        {
            var atacante = Novo();
            var aliadoMorto = Novo(hp: 800); Matar(aliadoMorto);
            var ctx = new ContextoCombate(atacante,
                new List<Combate> { atacante, aliadoMorto },
                new List<Combate> { Novo() });

            // Nigiri-like (era EstadoAlvo.Ambos + Ativar bespoke): Reviver(Mortos) e depois
            // buff nos Vivos — o EstadoAlvo na EXECUÇÃO faz o recém-revivido pegar o buff.
            var hab = Hab(new()
            {
                new Reviver(0.50),
                new AplicarBuff(() => new BuffAtaque(), Escopo.TodosAliados, EstadoAlvo.Vivos),
            }, alvos: int.MaxValue, lista: TipoLista.Aliados);

            hab.Ativar(ctx, atacante);

            Assert.True(aliadoMorto.EstaVivo());
            Assert.Equal(400, aliadoMorto.HPAtual);                              // 50% de 800
            Assert.True(aliadoMorto.StatusAtivos.OfType<BuffAtaque>().Any());    // revivido pegou o buff
            Assert.True(atacante.StatusAtivos.OfType<BuffAtaque>().Any());       // vivo também
        }

        [Fact]
        public void Reviver_DeN_UsaOPickDaHabilidade_ReviveSoOSelecionado()
        {
            var atacante = Novo();
            var morto1 = Novo(hp: 1000); Matar(morto1);
            var morto2 = Novo(hp: 1000); Matar(morto2);
            var ctx = new ContextoCombate(atacante,
                new List<Combate> { atacante, morto1, morto2 },
                new List<Combate> { Novo() });

            // DocesDeAbobora-like (regra do revive): a HABILIDADE declara o pick (1 alvo,
            // estado Mortos) e a ação herda os AlvosResolvidos — a MESMA seleção de qualquer
            // habilidade (selecionado + extras sorteados), sem contador dentro da ação.
            var hab = Hab(new()
            {
                new Reviver(1.0, Escopo.AlvosResolvidos),
            }, alvos: 1, lista: TipoLista.Aliados, modo: TipoAlvo.Aleatorio, estado: EstadoAlvo.Mortos);

            hab.Ativar(ctx, morto2);   // o jogador escolheu o morto2

            Assert.True(morto2.EstaVivo());     // o SELECIONADO revive
            Assert.False(morto1.EstaVivo());    // o outro não
            Assert.Equal(1000, morto2.HPAtual); // HP cheio
        }

        // ---------- Explodir (molde único das explosões — ADR §5.1) ----------

        [Fact]
        public void Explodir_DetonaOVeneno_RegistraOEvento_EACuraPorDanoIncluiAExplosao()
        {
            var atacante = Novo(hp: 1000); Ferir(atacante, 900);   // 100/1000 — espaço pra cura
            var inimigo1 = Novo(hp: 2000, atk: 0);
            new Veneno(stacks: 2).Aplicar(inimigo1);               // detona 10% de 2000 = 200
            var inimigo2 = Novo(hp: 2000, atk: 0);                 // sem veneno — explosão pula
            var ctx = new ContextoCombate(atacante,
                new List<Combate> { atacante },
                new List<Combate> { inimigo1, inimigo2 });

            // Putrefação-like: a cura é um EXTRA da habilidade (ação separada), 20% de TODO o
            // dano causado — a explosão entra na conta porque registra seus EventoDano.
            var hab = Hab(new()
            {
                new Explodir(Seletor.Tipo<Veneno>()),
                new Cura(Valor.PorDanoCausado(0.20), Escopo.ProprioAtacante),
            }, alvos: int.MaxValue);

            var eventos = hab.Ativar(ctx, inimigo1);

            Assert.False(inimigo1.StatusAtivos.OfType<Veneno>().Any());   // detonado e removido
            Assert.Single(eventos);                                        // só quem tinha veneno gera evento
            Assert.Equal(200, eventos[0].DanoEfetivo);                     // 2 stacks × 5% de 2000
            Assert.Equal(2000 - 200, inimigo1.HPAtual);
            Assert.Equal(2000, inimigo2.HPAtual);                          // sem veneno, intocado
            Assert.Equal(100 + 40, atacante.HPAtual);                      // cura = 20% de 200
        }

        // ---------- Convivência Strangler ----------

        /// <summary>
        /// Subclasse Strangler FAKE, local ao teste — prova que uma habilidade que ainda
        /// sobrescreve Ativar (o caminho velho) continua rodando junto do motor. Local DE
        /// PROPÓSITO: não depende de um champ real, que ao migrar pra forma-construtor sumiria e
        /// quebraria o teste (foi o que aconteceu com o RaioX ao virar dado nos Tecnológicos).
        /// Cura 15% do HP máx dos aliados vivos, igual ao antigo RaioX.
        /// </summary>
        private class CuraOverrideStrangler : HabilidadeAtiva
        {
            public CuraOverrideStrangler() : base("StranglerFake", "🧪", 3, "cura 15% dos aliados vivos") { }
            public override int NumeroDeAlvos => int.MaxValue;
            public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
            public override TipoLista TipoLista => TipoLista.Aliados;
            public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
            public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

            public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
            {
                foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                    AplicarCura(a, 0.15);
                return SemDano();
            }
        }

        [Fact]
        public void Strangler_SubclasseComAtivarOverride_ContinuaFuncionando()
        {
            var atacante = Novo(hp: 1000);
            var aliado = Novo(hp: 1000); Ferir(aliado, 400);   // 600/1000
            var ctx = new ContextoCombate(atacante,
                new List<Combate> { atacante, aliado },
                new List<Combate> { Novo() });

            var raioX = new CuraOverrideStrangler();            // Ativar velho, override

            raioX.Ativar(ctx, atacante);

            Assert.Equal(1000, atacante.HPAtual);               // capado
            Assert.Equal(600 + 150, aliado.HPAtual);            // 15% de 1000
        }

        // ---------- Vocabulário novo (Folclore) ----------

        [Fact]
        public void RemoverDebuffs_LimpaTodosOsDebuffsDoAlvo()
        {
            var atacante = Novo();
            var alvo = Novo();
            new Preso(turnos: 2).Aplicar(alvo);
            new Veneno(stacks: 1).Aplicar(alvo);
            Assert.Equal(2, alvo.StatusAtivos.OfType<Debuff>().Count());

            var ctx = new ContextoCombate(atacante,
                new List<Combate> { atacante },
                new List<Combate> { alvo });
            var hab = Hab(new() { new RemoverDebuffs(Seletor.Todos()) }, alvos: 1);

            hab.Ativar(ctx, alvo);

            Assert.Empty(alvo.StatusAtivos.OfType<Debuff>());     // cleanse total (gêmeo do RemoverBuffs)
        }

        [Fact]
        public void AplicarDebuff_Chance_ZeroNuncaAplica_UmSempreAplica()
        {
            var atacante = Novo();

            var alvoNunca = Novo();
            var ctx0 = new ContextoCombate(atacante,
                new List<Combate> { atacante }, new List<Combate> { alvoNunca });
            Hab(new() { new AplicarDebuff(() => new Preso(turnos: 2), chance: 0.0) }, alvos: 1)
                .Ativar(ctx0, alvoNunca);
            Assert.Empty(alvoNunca.StatusAtivos.OfType<Preso>());  // chance 0.0 nunca aplica

            var alvoSempre = Novo();
            var ctx1 = new ContextoCombate(atacante,
                new List<Combate> { atacante }, new List<Combate> { alvoSempre });
            Hab(new() { new AplicarDebuff(() => new Preso(turnos: 2), chance: 1.0) }, alvos: 1)
                .Ativar(ctx1, alvoSempre);
            Assert.Single(alvoSempre.StatusAtivos.OfType<Preso>()); // chance 1.0 sempre aplica
        }
    }
}
