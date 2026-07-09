using ApostlesWar;
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
            TipoLista lista = TipoLista.Inimigos, TipoAlvo modo = TipoAlvo.Explicito)
            => new("Teste", "🧪", 3, "hab de teste", alvos, modo, lista, EstadoAlvo.Vivos, acoes);

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

        // ---------- Convivência Strangler ----------

        [Fact]
        public void Strangler_SubclasseComAtivarOverride_ContinuaFuncionando()
        {
            var atacante = Novo(hp: 1000);
            var aliado = Novo(hp: 1000); Ferir(aliado, 400);   // 600/1000
            var ctx = new ContextoCombate(atacante,
                new List<Combate> { atacante, aliado },
                new List<Combate> { Novo() });

            var democracia = new Democracia();                  // Ativar velho, override

            democracia.Ativar(ctx, atacante);

            Assert.Equal(1000, atacante.HPAtual);               // capado
            Assert.Equal(600 + 300, aliado.HPAtual);            // 30% de 1000
        }
    }
}
