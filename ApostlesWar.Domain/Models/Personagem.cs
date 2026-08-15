namespace ApostlesWar.Domain
{
    #region Personagem

    /// <summary>
    /// Representa o Personagem
    /// </summary>
    public class Personagem
    {
        public int Slot { get; private set; }
        public Faccao Faccao { get; private set; }
        public string Nome { get; private set; }
        public string Simbolo { get; private set; }
        public TipoDeApostolo Tipo { get; private set; }
        public int Nivel { get; private set; }
        public int HP { get; private set; }
        public int Ataque { get; private set; }
        public int Defesa { get; private set; }
        public int Velocidade { get; private set; }
        public int Precisao { get; private set; }
        public int Resistencia { get; private set; }
        public double TaxaCrit { get; private set; }
        public double DanoCrit { get; private set; }
        public List<Habilidade> Habilidades { get; private set; }

        /// <summary>
        /// O construtor dos apóstolos DE VERDADE: a ficha inteira nasce do <paramref name="tipo"/>,
        /// torcida pela facção e escalada pelo nível (ver <see cref="Arquetipos"/>). Nenhum apóstolo
        /// declara os próprios números — dois Guardiões da mesma facção são idênticos na ficha, e o
        /// que os separa é o kit e o equipamento.
        /// </summary>
        public Personagem(int slot, Faccao faccao, string nome, string simbolo,
            TipoDeApostolo tipo, params Habilidade[] habilidades)
            : this(slot, faccao, nome, simbolo, tipo, Arquetipos.NivelMinimo, habilidades) { }

        public Personagem(int slot, Faccao faccao, string nome, string simbolo,
            TipoDeApostolo tipo, int nivel, params Habilidade[] habilidades)
        {
            Slot = slot;
            Faccao = faccao;
            Nome = nome;
            Simbolo = simbolo;
            Tipo = tipo;
            Nivel = nivel;
            Habilidades = new List<Habilidade>(habilidades);

            var combate = Arquetipos.StatsDeCombate(tipo, faccao, nivel);
            HP = combate.HP;
            Ataque = combate.Ataque;
            Defesa = combate.Defesa;

            Arquetipos.Ficha ficha = Arquetipos.Base(tipo);
            Velocidade = Arquetipos.Velocidade(tipo, nivel);
            Precisao = ficha.Precisao;
            Resistencia = ficha.Resistencia;
            TaxaCrit = ficha.TaxaCrit;
            DanoCrit = ficha.DanoCrit;
        }

        /// <summary>
        /// FICHA CRUA, com os números na mão — existe pros BONECOS: os alvos de isolamento da bancada
        /// e os personagens de teste, cujo propósito é justamente ter stats arbitrários (defesa 0,
        /// HP 100.000) que nenhum arquétipo produziria.
        ///
        /// <b>Apóstolo do jogo não usa este caminho.</b> Se um dia um deles precisar de número
        /// próprio, o lugar é a tabela do <see cref="Arquetipos"/> ou a variação da facção — senão os
        /// 108 números soltos voltam pela porta dos fundos.
        /// </summary>
        public Personagem(int slot, Faccao faccao, string nome, string simbolo,
            int hp, int ataque, int def, params Habilidade[] habilidades)
        {
            Slot = slot;
            Faccao = faccao;
            Nome = nome;
            Simbolo = simbolo;
            Tipo = TipoDeApostolo.Combatente;
            Nivel = Arquetipos.NivelMinimo;
            HP = hp;
            Ataque = ataque;
            Defesa = def;
            Habilidades = new List<Habilidade>(habilidades);

            // O boneco herda Velocidade/Precisão/Resistência do Combatente (o arquétipo do meio, pra
            // não enviesar medição), mas o CRÍTICO fica nos valores históricos — 15%/60% eram as
            // constantes globais contra as quais os testes de hoje foram escritos, e herdar os 25%/90%
            // do Combatente mudaria a frequência de crítico deles em silêncio.
            Arquetipos.Ficha ficha = Arquetipos.Base(TipoDeApostolo.Combatente);
            Velocidade = ficha.VelocidadeNv1;
            Precisao = ficha.Precisao;
            Resistencia = ficha.Resistencia;
            TaxaCrit = CritCruTaxa;
            DanoCrit = CritCruDano;
        }

        /// <summary>
        /// Crava a Velocidade — <b>boneco só</b>, no mesmo espírito da ficha crua acima. Existe
        /// porque a Velocidade decide QUEM JOGA (<see cref="FilaDeTurnos"/>), e sem cravá-la não há
        /// como montar um combatente que joga o dobro dos turnos do outro.
        ///
        /// Apóstolo do jogo NÃO passa por aqui: a Velocidade dele sai do <see cref="Arquetipos"/>, e
        /// o que vai modificá-la em combate são as camadas do <c>Combate</c> quando a Bota chegar.
        /// </summary>
        public Personagem ComVelocidade(int velocidade)
        {
            Velocidade = velocidade;
            return this;
        }

        /// <summary>
        /// Crava a Resistência — <b>boneco só</b>, e o cliente principal é `0`: alvo sem Resistência
        /// nenhuma apanha TODO malefício (ver <c>Combate.ChanceDeColarEm</c>). É o mesmo espírito do
        /// crítico 100% da bancada — quem mede mecânica não pode estar medindo o dado.
        /// </summary>
        public Personagem ComResistencia(int resistencia)
        {
            Resistencia = resistencia;
            return this;
        }

        /// <summary>Crava a Precisão — <b>boneco só</b>, o gêmeo do <see cref="ComResistencia"/>.</summary>
        public Personagem ComPrecisao(int precisao)
        {
            Precisao = precisao;
            return this;
        }

        /// <summary>Crítico da ficha crua. Era a constante global, antes de o crítico virar do TIPO.</summary>
        public const double CritCruTaxa = 0.15;
        public const double CritCruDano = 0.60;
    }

    #endregion
}
