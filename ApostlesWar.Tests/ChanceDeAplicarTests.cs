using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Debuffs;

namespace Tests
{
    /// <summary>
    /// O 🎲 DA TELA — <c>HabilidadeAtiva.ChanceDeAplicarEm</c>, o número que aparece em cada alvo
    /// quando o jogador clica numa habilidade (GDD-combate §1).
    ///
    /// É função PURA, então dá pra afirmar o valor exato: o que estes testes guardam não é o sorteio,
    /// é a LEITURA — o que a tela promete tem de ser o mesmo número contra o qual o motor rola.
    /// </summary>
    public class ChanceDeAplicarTests
    {
        private static Combate Com(int precisao, int resistencia) =>
            new Jogador(new Personagem(1, Faccao.Humanos, "T", "🧪", 1000, 100, 0)
                .ComPrecisao(precisao).ComResistencia(resistencia));

        private static HabilidadeAtiva Habilidade(TipoLista tipoLista, params Acao[] acoes) =>
            new("T", "🧪", cooldown: 1, "", numeroDeAlvos: 1, TipoAlvo.Explicito, tipoLista,
                EstadoAlvo.Vivos, acoes.ToList());

        /// <summary>
        /// Sem malefício nenhum não há disputa, e 100% é justamente o estado em que a tela SOME com
        /// a linha — é assim que "habilidade que não impõe nada" não precisa de caso especial.
        /// </summary>
        [Fact]
        public void HabilidadeSemMaleficio_DaGarantia()
        {
            var hab = Habilidade(TipoLista.Inimigos, new Dano(1.0));

            double chance = hab.ChanceDeAplicarEm(Com(0, 0), Com(0, 500), alvoEhInimigo: true);

            Assert.Equal(1.0, chance, 10);
        }

        [Fact]
        public void ComMaleficio_ESoADisputa()
        {
            var hab = Habilidade(TipoLista.Inimigos, new AplicarDebuff(() => new ReducaoAtaque(2)));

            double chance = hab.ChanceDeAplicarEm(Com(500, 0), Com(0, 500), alvoEhInimigo: true);

            Assert.Equal(0.50, chance, 10);   // empate de Precisão × Resistência
        }

        /// <summary>
        /// OS DOIS PORTÕES MULTIPLICAM — é o caso do Medo do Troll (<c>chance: 0.50</c>) contra um
        /// alvo que resiste metade. Mostrar só a disputa diria 50% e colaria 25%.
        ///
        /// Este é também o campo por onde a RARIDADE vai diferenciar kits: subiu a `chance` da ação,
        /// o número da tela sobe junto, sem ninguém tocar na tela.
        /// </summary>
        [Fact]
        public void AChanceDaPropriaHabilidade_MultiplicaADisputa()
        {
            var hab = Habilidade(TipoLista.Inimigos,
                new AplicarDebuff(() => new Medo(duracao: 1), chance: 0.50));

            double chance = hab.ChanceDeAplicarEm(Com(500, 0), Com(0, 500), alvoEhInimigo: true);

            Assert.Equal(0.25, chance, 10);
        }

        /// <summary>
        /// O caso do Desejo do Gênio: buff nos aliados e Maldição nos inimigos, na MESMA habilidade.
        /// Sem o escopo filtrando, o aliado leria a chance da Maldição — um número que não vale pra
        /// ele em habilidade nenhuma.
        /// </summary>
        [Fact]
        public void OEscopoFiltra_OMaleficioNosInimigosNaoVazaProAliado()
        {
            var hab = Habilidade(TipoLista.Aliados,
                new AplicarBuff(() => new BuffDefesa(duracao: 2, percentual: 0.30), Escopo.TodosAliados),
                new AplicarDebuff(() => new Maldicao(stacks: 2), Escopo.TodosInimigos));

            var atacante = Com(500, 0);

            Assert.Equal(0.50, hab.ChanceDeAplicarEm(atacante, Com(0, 500), alvoEhInimigo: true), 10);
            Assert.Equal(1.00, hab.ChanceDeAplicarEm(atacante, Com(0, 500), alvoEhInimigo: false), 10);
        }

        /// <summary>
        /// DOIS MALEFÍCIOS, UM NÚMERO SÓ: fica o MENOR. São tentativas independentes, então nenhum
        /// número único as descreve — e o menor é o único que nunca promete mais do que a parte mais
        /// frágil entrega. Hoje nenhuma habilidade do jogo cai neste caso; quando a raridade começar
        /// a mexer no `chance` de cada ação, cai.
        /// </summary>
        [Fact]
        public void ComDoisMaleficiosDeChancesDiferentes_FicaOMenor()
        {
            var hab = Habilidade(TipoLista.Inimigos,
                new AplicarDebuff(() => new ReducaoAtaque(2)),                        // 100% × disputa
                new AplicarDebuff(() => new Medo(duracao: 1), chance: 0.50));         //  50% × disputa

            double chance = hab.ChanceDeAplicarEm(Com(1000, 0), Com(0, 500), alvoEhInimigo: true);

            Assert.Equal(0.50, chance, 10);
        }

        [Fact]
        public void AlvoSemResistencia_DaGarantiaEALinhaSome()
        {
            var hab = Habilidade(TipoLista.Inimigos, new AplicarDebuff(() => new ReducaoAtaque(2)));

            double chance = hab.ChanceDeAplicarEm(Com(1, 0), Com(0, 0), alvoEhInimigo: true);

            Assert.Equal(1.0, chance, 10);
        }

        /// <summary>
        /// Auto-malefício não passa pela disputa (a Resistência do próprio não vale contra ele), mas
        /// a `chance` declarada continua valendo: ela é identidade da habilidade, não imposição.
        /// </summary>
        [Fact]
        public void EmSiMesmo_SoAChanceDeclaradaConta()
        {
            var quem = Com(precisao: 0, resistencia: 500);
            var hab = Habilidade(TipoLista.Self,
                new AplicarDebuff(() => new Medo(duracao: 1), Escopo.ProprioAtacante, chance: 0.50));

            double chance = hab.ChanceDeAplicarEm(quem, quem, alvoEhInimigo: false);

            Assert.Equal(0.50, chance, 10);
        }
    }
}
