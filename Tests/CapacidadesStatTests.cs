using ApostlesWar;
using ApostlesWar.Skills.Buffs;
using ApostlesWar.Skills.Debuffs;

namespace Tests
{
    /// <summary>
    /// Testes da Capacidade C (contribuição de stat via IContribui*). Cada stat
    /// (Ataque/Defesa/TaxaCrit/DanoCrit) calcula buff/debuff somando a INTERFACE de
    /// contribuição com sinal — não por tipo concreto. Provado aqui pelo movimento dos
    /// getters, que são puros (sem Console → headless-safe).
    ///
    /// AtaqueComStacks = 200 e DefesaComStacks = 100 nos combatentes de teste (Jogador,
    /// multiplicador 1.0, sem itens). TaxaCrit base = 0.15, DanoCrit base = 0.60
    /// (Personagem.TaxaCritBase/DanoCritBase).
    /// </summary>
    public class CapacidadesStatTests
    {
        private static Combate Novo(int atk = 200, int def = 100)
            => new Jogador(new Personagem(1, Faccao.Humanos, "Teste", "🧪", 1000, atk, def));

        // ---------- Ataque ----------

        [Fact]
        public void BuffAtaque_SomaPercentualSobreOsStacks()
        {
            var c = Novo(atk: 200);
            new BuffAtaque(percentual: 0.25).Aplicar(c);
            Assert.Equal(250, c.Ataque);            // +25% de 200
        }

        [Fact]
        public void ReducaoAtaque_SubtraiPercentualSobreOsStacks()
        {
            var c = Novo(atk: 200);
            new ReducaoAtaque(percentual: 0.25).Aplicar(c);
            Assert.Equal(150, c.Ataque);            // -25% de 200
        }

        [Fact]
        public void BuffEReducaoAtaque_Convivem_SomaComSinal()
        {
            var c = Novo(atk: 200);
            new BuffAtaque(percentual: 0.25).Aplicar(c);
            new ReducaoAtaque(percentual: 0.25).Aplicar(c);
            Assert.Equal(200, c.Ataque);            // +50 −50 = base
        }

        // ---------- Defesa (o getter agora usa IContribuiDefesa, não tipo concreto) ----------

        [Fact]
        public void BuffEReducaoDefesa_ViaInterface_MantemOsNumeros()
        {
            var comBuff = Novo(def: 100);
            new BuffDefesa(percentual: 0.30).Aplicar(comBuff);
            Assert.Equal(130, comBuff.Defesa);      // +30% de 100

            var comDebuff = Novo(def: 100);
            new ReducaoDefesa().Aplicar(comDebuff);
            Assert.Equal(70, comDebuff.Defesa);     // -30% de 100
        }

        // ---------- TaxaCrit (pontos absolutos) ----------

        [Fact]
        public void BuffTaxaCrit_Soma_ReducaoTaxaCrit_Subtrai()
        {
            var c = Novo();
            double baseTaxa = c.TaxaCrit;                     // 0.15
            new BuffTaxaCrit(valor: 0.20).Aplicar(c);
            Assert.Equal(baseTaxa + 0.20, c.TaxaCrit, 3);     // 0.35

            new ReducaoTaxaCrit(valor: 0.25).Aplicar(c);      // tipo diferente: convive
            Assert.Equal(baseTaxa - 0.05, c.TaxaCrit, 3);     // 0.15 +0.20 −0.25 = 0.10
        }

        // ---------- DanoCrit (pontos absolutos) ----------

        [Fact]
        public void BuffDanoCrit_Soma_ReducaoDanoCrit_ZeraDeVolta()
        {
            var c = Novo();
            double baseDano = c.DanoCrit;                     // 0.60
            new BuffDanoCrit(valor: 0.25).Aplicar(c);
            Assert.Equal(baseDano + 0.25, c.DanoCrit, 3);     // 0.85

            new ReducaoDanoCrit(valor: 0.25).Aplicar(c);
            Assert.Equal(baseDano, c.DanoCrit, 3);            // +0.25 −0.25 = base
        }
    }
}
