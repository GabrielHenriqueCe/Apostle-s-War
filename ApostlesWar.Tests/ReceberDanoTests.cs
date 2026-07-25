using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Debuffs;

namespace Tests
{
    /// <summary>
    /// Testes ponta-a-ponta do <c>Combate.ReceberDano</c> — o FUNIL ÚNICO por onde passa TODO dano
    /// do jogo (ataque, veneno, queima, explosão, reflexo). Cobre o pipeline na ordem em que ele
    /// roda: defesa → passiva-pura → status (escudo/bloqueio) → HP → acumuladores → piso de HP →
    /// confirmação de morte (FILA A #14).
    ///
    /// O que já vive em MotorDeHabilidadesTests (não duplicar aqui): a gramática de IGNORAR
    /// (natureza/golpe/champ), o Invencível em 1 HP e o GuardaReal preservando status. Aqui entram
    /// as partes do funil que estavam sem dono: a MATEMÁTICA da defesa (fórmula + cap +
    /// ignorarDefesaPct + IgnoraDefesa), os ACUMULADORES do resumo (#7a), a ORDEM
    /// passiva-antes-de-status e o prevent-death consumindo COOLDOWN.
    ///
    /// Determinismo: nada aqui usa Atacar (que rola crítico) — o dano entra direto pelo ReceberDano
    /// com valor fixo, então os números são exatos.
    /// </summary>
    public class ReceberDanoTests
    {
        // Constantes de balance espelhadas do Combate (lá são private const): cada 1000 de DEF
        // dá a redução MÁXIMA de 75%, proporcionalmente e com cap.
        private const double DefesaPorPontoReducao = 1000.0;
        private const double ReducaoMaximaPorDefesa = 0.75;

        private static Combate Novo(int hp = 100_000, int atk = 0, int def = 0)
            => new Jogador(new Personagem(1, Faccao.Humanos, "Teste", "🧪", hp, atk, def));

        private static Combate NovoCom(Habilidade passiva, int hp = 100_000, int def = 0)
            => new Jogador(new Personagem(1, Faccao.Humanos, "Teste", "🧪", hp, 0, def, passiva));

        /// <summary>Redução esperada pra uma defesa efetiva — a mesma fórmula do ReceberDano.</summary>
        private static int DanoAposDefesa(int bruto, int defesaEfetiva)
        {
            double reducao = Math.Min(
                (defesaEfetiva / DefesaPorPontoReducao) * ReducaoMaximaPorDefesa,
                ReducaoMaximaPorDefesa);
            return (int)(bruto * (1 - reducao));
        }

        // ---------- Etapa 1: defesa ----------

        [Fact]
        public void Defesa_AplicaAReducaoProporcional()
        {
            var alvo = Novo(def: 500);

            var (efetivo, _) = alvo.ReceberDano(1000, NaturezasDano.Ataque);

            // 500/1000 × 75% = 37,5% de redução → 1000 × 0,625
            Assert.Equal(625, efetivo);
            Assert.Equal(625, DanoAposDefesa(1000, 500));
            Assert.Equal(100_000 - 625, alvo.HPAtual);
        }

        [Fact]
        public void Defesa_TemCapDeReducao_DefesaAlemDoTetoNaoAjudaMais()
        {
            var noTeto = Novo(def: 1000);      // exatamente no cap
            var muitoAlem = Novo(def: 9999);   // muito além do cap

            var (efetivoTeto, _) = noTeto.ReceberDano(1000, NaturezasDano.Ataque);
            var (efetivoAlem, _) = muitoAlem.ReceberDano(1000, NaturezasDano.Ataque);

            Assert.Equal(250, efetivoTeto);            // 75% de redução (o máximo)
            Assert.Equal(efetivoTeto, efetivoAlem);    // defesa extra não reduz mais nada
        }

        [Fact]
        public void IgnorarDefesaPct_EncolheADefesaEfetivaAntesDaFormula()
        {
            var alvo = Novo(def: 1000);

            // Furando 50% da defesa, ela vale 500 → mesma redução do teste de 500 de DEF.
            var (efetivo, _) = alvo.ReceberDano(1000, NaturezasDano.Ataque, ignorarDefesaPct: 0.5);

            Assert.Equal(625, efetivo);
        }

        [Fact]
        public void NaturezaComIgnoraDefesa_PulaAEtapaInteira()
        {
            var alvo = Novo(def: 1000);   // reduziria 75% num ataque normal

            // Veneno é IgnoraDefesa: true — DEF é stat, não entra na lista de ignorar status.
            var (efetivo, _) = alvo.ReceberDano(1000, NaturezasDano.Veneno);

            Assert.Equal(1000, efetivo);
        }

        [Fact]
        public void BuffDefesa_EntraNaDefesaEfetiva_ComSinal()
        {
            var alvo = Novo(def: 400);
            new BuffDefesa(duracao: 2, percentual: 0.50).Aplicar(alvo);   // 400 → 600

            var (efetivo, _) = alvo.ReceberDano(1000, NaturezasDano.Ataque);

            Assert.Equal(DanoAposDefesa(1000, 600), efetivo);
        }

        // ---------- Etapa 2: ordem passiva-pura ANTES dos status ----------

        /// <summary>
        /// Passiva-pura FAKE, local ao teste (mesma razão do Strangler em MotorDeHabilidadesTests:
        /// não amarrar o teste a um champ real que pode migrar/sumir). Corta o dano pela metade.
        /// </summary>
        private class MetadeDoDano : HabilidadePassiva, IModificaDanoRecebido
        {
            public MetadeDoDano() : base("MetadeFake", "🧪", 0, "corta o dano pela metade") { }
            public OrdemDeMitigacao OrdemDeMitigacao => OrdemDeMitigacao.ReduzDeGraca;
            public int ModificarDanoRecebido(Combate portador, int dano) => dano / 2;
            public int PreverDanoRecebido(Combate portador, int dano) => dano / 2;   // puro: prever == modificar
        }

        [Fact]
        public void PassivaPura_RodaANTESDosStatus_EOEscudoVeODanoJaReduzido()
        {
            var alvo = NovoCom(new MetadeDoDano());
            new Escudo(500, duracao: 2).Aplicar(alvo);

            // Ordem real: passiva corta 1000 → 500; o escudo (500) come os 500 restantes → 0 entra.
            // Se a ordem fosse invertida, o escudo comeria 500 de 1000 e a passiva cortaria os
            // outros 500 pela metade → 250 de dano efetivo. O 0 é o que prova a ordem.
            var (efetivo, absorvido) = alvo.ReceberDano(1000, NaturezasDano.Ataque);

            Assert.Equal(0, efetivo);
            Assert.Equal(500, absorvido);
            Assert.Equal(100_000, alvo.HPAtual);   // nada entrou no HP
        }

        // ---------- Etapa 2b: ordem ENTRE os status (quem reduz de graça antes de quem gasta) ----------

        /// <summary>
        /// O bug que criou a OrdemDeMitigacao (achado em jogo, Rei + Operário): o laço de status
        /// iterava na ordem de APLICAÇÃO, então aplicar Escudo antes do Bloqueio Total fazia o escudo
        /// gastar pontos aparando um golpe que o bloqueio ia zerar de graça logo depois.
        /// </summary>
        [Fact]
        public void EscudoAplicadoAntesDoBloqueio_NaoGastaPontos()
        {
            var alvo = Novo();
            var escudo = new Escudo(500, duracao: 2);
            escudo.Aplicar(alvo);                          // aplicado PRIMEIRO — era o que quebrava
            new BloqueioTotal(duracao: 1).Aplicar(alvo);

            var (efetivo, absorvido) = alvo.ReceberDano(1000, NaturezasDano.Ataque);

            Assert.Equal(0, efetivo);
            Assert.Equal(0, absorvido);                    // o escudo não aparou nada...
            Assert.Equal(500, escudo.PontosRestantes);     // ...porque não precisou: pontos intactos
            Assert.Contains(escudo, alvo.StatusAtivos);    // e o buff sobreviveu ao turno
            Assert.Equal(100_000, alvo.HPAtual);
        }

        /// <summary>
        /// A ordem inversa (bloqueio aplicado primeiro) já funcionava antes do conserto — este teste
        /// prova que o resultado agora é o MESMO nos dois sentidos, ou seja, que a ordem de aplicação
        /// deixou de importar. Sem ele, uma troca de ordem no laço passaria batida.
        /// </summary>
        [Fact]
        public void BloqueioAplicadoAntesDoEscudo_DaOMesmoResultado()
        {
            var alvo = Novo();
            new BloqueioTotal(duracao: 1).Aplicar(alvo);
            var escudo = new Escudo(500, duracao: 2);
            escudo.Aplicar(alvo);

            var (efetivo, absorvido) = alvo.ReceberDano(1000, NaturezasDano.Ataque);

            Assert.Equal(0, efetivo);
            Assert.Equal(0, absorvido);
            Assert.Equal(500, escudo.PontosRestantes);
        }

        /// <summary>
        /// O Prever tem que ordenar igual ao Receber: se divergirem, o bot avalia o alvo com um número
        /// que o golpe real não vai reproduzir — e mira errado em silêncio.
        /// </summary>
        [Fact]
        public void Prever_UsaAMesmaOrdemDoReceber()
        {
            var alvo = Novo();
            var escudo = new Escudo(500, duracao: 2);
            escudo.Aplicar(alvo);
            new BloqueioTotal(duracao: 1).Aplicar(alvo);

            int previsto = alvo.PreverDanoRecebido(1000, NaturezasDano.Ataque);
            var (efetivo, _) = alvo.ReceberDano(1000, NaturezasDano.Ataque);

            Assert.Equal(0, previsto);
            Assert.Equal(previsto, efetivo);
            Assert.Equal(500, escudo.PontosRestantes);   // prever não gasta, receber não precisou
        }

        /// <summary>
        /// Mesmo princípio com o outro recurso em jogo: o HP de um aliado. Com o bloqueio rodando
        /// antes, o protetor recebe o dano JÁ zerado em vez de comer 30% de um golpe que não aconteceu.
        /// </summary>
        [Fact]
        public void ProtecaoAliado_NaoCobraDoProtetor_QuandoOBloqueioJaZerouODano()
        {
            var protegido = Novo();
            var protetor = Novo();
            new ProtecaoAliado(protetor, duracao: 2).Aplicar(protegido);
            new BloqueioTotal(duracao: 1).Aplicar(protegido);

            var (efetivo, _) = protegido.ReceberDano(1000, NaturezasDano.Ataque);

            Assert.Equal(0, efetivo);
            Assert.Equal(100_000, protetor.HPAtual);   // o protetor não pagou por nada
        }

        /// <summary>
        /// O aplicar e o prever da proteção fazem a MESMA conta, e nada além deste teste os obriga a
        /// concordar. Os valores são de propósito QUEBRADOS (parte fracionária ≥ 0,5): com os números
        /// redondos do resto da suíte — 1000 × 0,30 = 300 exato — trocar o truncamento por
        /// arredondamento em UM dos dois métodos passaria verde, e o bot mediria o alvo com um número
        /// que o golpe real não reproduz. Silencioso, que é o pior tipo.
        /// </summary>
        [Theory]
        [InlineData(999, 0.30)]   // 299,7 → 299 truncado (arredondado seria 300)
        [InlineData(333, 0.30)]   //  99,9 →  99
        [InlineData(105, 0.15)]   //  15,75 → 15
        [InlineData(777, 0.35)]   // 271,95 → 271
        public void ProtecaoAliado_PreverBateComOAplicar_MesmoComNumeroQuebrado(int dano, double percentual)
        {
            var protegido = Novo();
            var protetor = Novo();   // def 0: o protetor recebe o redirecionado inteiro
            new ProtecaoAliado(protetor, duracao: 2, percentual: percentual).Aplicar(protegido);

            int previsto = protegido.PreverDanoRecebido(dano, NaturezasDano.Ataque);
            var (efetivo, _) = protegido.ReceberDano(dano, NaturezasDano.Ataque);

            Assert.Equal(previsto, efetivo);
            Assert.Equal(100_000, protegido.HPAtual + efetivo);
            // e o que saiu do protegido é EXATAMENTE o que entrou no protetor: nada evapora no meio.
            Assert.Equal(dano - efetivo, 100_000 - protetor.HPAtual);
        }

        /// <summary>
        /// REGRA DECIDIDA (jul/2026): o redirecionamento passa pela defesa do protetor, então tanque
        /// protege mais barato. O doc da classe dizia "sem defesa" e contradizia a implementação; o
        /// Gabriel bateu o martelo de que a IMPLEMENTAÇÃO é a certa (DEF alta obviamente defende mais)
        /// e o doc foi corrigido. Estava invisível porque todo teste de proteção dava def 0 ao protetor.
        ///
        /// O que o PROTEGIDO desconta não muda: são sempre os 30%, independente de quem o protege.
        /// </summary>
        [Theory]
        [InlineData(0, 300)]      // sem defesa: paga os 30% cheios
        [InlineData(500, 187)]    // 37,5% de redução → 300 × 0,625 = 187,5, trunca
        [InlineData(2000, 75)]    // além do cap: 75% é o teto → 300 × 0,25
        public void ProtecaoAliado_ADefesaDoProtetorAbateOQueEleRecebe(int defesaDoProtetor, int pagoPeloProtetor)
        {
            var protegido = Novo();
            var protetor = Novo(def: defesaDoProtetor);
            new ProtecaoAliado(protetor, duracao: 2, percentual: 0.30).Aplicar(protegido);

            var (efetivo, _) = protegido.ReceberDano(1000, NaturezasDano.Ataque);

            Assert.Equal(700, efetivo);                                    // o protegido: sempre 30% a menos
            Assert.Equal(pagoPeloProtetor, 100_000 - protetor.HPAtual);
            Assert.Equal(DanoAposDefesa(300, defesaDoProtetor), pagoPeloProtetor);   // é a fórmula de sempre
        }

        /// <summary>
        /// Com o protetor MORTO a proteção não age — o portador come o golpe inteiro. É o único ramo
        /// dos dois métodos que ninguém exercitava, e ele existe porque há uma janela real no jogo:
        /// o status só se autoremove na expiração natural do próximo turno.
        /// </summary>
        [Fact]
        public void ProtecaoAliado_ComOProtetorMorto_NaoRedirecionaNemPreve()
        {
            var protegido = Novo();
            var protetor = Novo(hp: 100);
            new ProtecaoAliado(protetor, duracao: 2).Aplicar(protegido);
            protetor.ReceberDano(100, NaturezasDano.Ataque);   // mata o protetor
            Assert.False(protetor.EstaVivo());

            int previsto = protegido.PreverDanoRecebido(1000, NaturezasDano.Ataque);
            var (efetivo, _) = protegido.ReceberDano(1000, NaturezasDano.Ataque);

            Assert.Equal(1000, previsto);   // nada redirecionado: o golpe chega inteiro
            Assert.Equal(1000, efetivo);
            Assert.Equal(previsto, efetivo);
        }

        [Fact]
        public void Escudo_AbsorveParcialmente_ERelataOQueAparou()
        {
            var alvo = Novo();
            new Escudo(300, duracao: 2).Aplicar(alvo);

            var (efetivo, absorvido) = alvo.ReceberDano(1000, NaturezasDano.Ataque);

            Assert.Equal(300, absorvido);
            Assert.Equal(700, efetivo);
            Assert.Equal(100_000 - 700, alvo.HPAtual);
        }

        // ---------- Acumuladores do resumo de fim de batalha (#7a) ----------

        [Fact]
        public void DanoRecebido_SomaOEfetivo_ACadaPassadaPeloFunil()
        {
            var alvo = Novo();

            alvo.ReceberDano(300, NaturezasDano.Ataque);
            alvo.ReceberDano(200, NaturezasDano.Veneno);      // tick de status também conta
            alvo.ReceberDano(100, NaturezasDano.QueimaDano);

            Assert.Equal(600, alvo.DanoRecebido);
        }

        [Fact]
        public void DanoCausado_CreditaOAtacante_SoQuandoEleEInformado()
        {
            var atacante = Novo();
            var alvo = Novo();

            alvo.ReceberDano(400, NaturezasDano.Ataque, atacante);
            alvo.ReceberDano(600, NaturezasDano.Ataque, atacante);
            alvo.ReceberDano(999, NaturezasDano.Veneno);        // sem atacante: ninguém leva o crédito

            Assert.Equal(1000, atacante.DanoCausado);
            Assert.Equal(0, alvo.DanoCausado);
        }

        [Fact]
        public void DanoRecebido_ContaODanoCHEIO_MesmoComOHPSeguradoPeloPiso()
        {
            var alvo = Novo(hp: 1000);
            alvo.ReceberDano(999, NaturezasDano.Ataque);        // sobra 1 HP
            new Invencivel(duracao: 2).Aplicar(alvo);           // IDefineHPMinimo: piso de 1

            alvo.ReceberDano(500, NaturezasDano.Ataque);

            // O piso segura o HP, mas o dano contabilizado é integral — é o que faz o lifesteal
            // enxergar o golpe (fix do #151).
            Assert.Equal(1, alvo.HPAtual);
            Assert.Equal(999 + 500, alvo.DanoRecebido);
        }

        [Fact]
        public void CuraRecebida_SomaSoOQueDeFatoEntrou_CapadaNoMaximo()
        {
            var alvo = Novo(hp: 1000);
            alvo.ReceberDano(400, NaturezasDano.Ataque);   // 600/1000

            alvo.Curar(150);    // entra inteira
            alvo.Curar(999);    // só 250 cabem até o teto

            Assert.Equal(1000, alvo.HPAtual);
            Assert.Equal(150 + 250, alvo.CuraRecebida);
        }

        // ---------- Confirmação de morte ----------

        [Fact]
        public void HPZerado_SemPreventDeath_ConfirmaAMorte()
        {
            var alvo = Novo(hp: 500);

            alvo.ReceberDano(500, NaturezasDano.Ataque);

            Assert.False(alvo.EstaVivo());
        }

        [Fact]
        public void PreventDeath_SalvaUmaVezESOME_QuandoOCooldownAcaba()
        {
            // GuardaReal tem cooldown 4 e é consumido ao prevenir: o 2º golpe fatal no mesmo
            // combate encontra a capacidade indisponível e a morte acontece.
            var guarda = NovoCom(new ApostlesWar.Domain.Champs.Reino.GuardaReal(), hp: 1000);

            guarda.ReceberDano(5000, NaturezasDano.Ataque);
            Assert.True(guarda.EstaVivo());               // salvo pelo prevent-death
            Assert.Equal(1, guarda.HPAtual);

            // O Invencível ganho na prevenção viraria piso de HP; furamos ele pra testar a
            // segunda morte de verdade (senão o piso seguraria o HP em 1 independentemente).
            guarda.ReceberDano(5000, NaturezasDano.Ataque,
                ignorarStatus: new[] { typeof(Invencivel) });

            Assert.False(guarda.EstaVivo());              // cooldown gasto: morre
        }

        [Fact]
        public void DanoNaoLetal_NaoConfirmaMorte_EMantemOsStatus()
        {
            var alvo = Novo(hp: 1000);
            new Veneno(2).Aplicar(alvo);

            alvo.ReceberDano(999, NaturezasDano.Ataque);

            Assert.True(alvo.EstaVivo());
            Assert.Equal(1, alvo.HPAtual);
            Assert.True(alvo.StatusAtivos.OfType<Veneno>().Any());
        }
    }
}
