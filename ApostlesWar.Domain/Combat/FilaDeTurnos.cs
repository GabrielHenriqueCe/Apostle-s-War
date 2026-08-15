namespace ApostlesWar.Domain
{
    /// <summary>
    /// QUEM JOGA AGORA (GDD-progressao §1). Cada combatente enche a PRÓPRIA barra de 0 a 100 pela
    /// própria Velocidade; age quem cruzou 100 e, entre os prontos, o mais CHEIO. Ao agir desconta
    /// 100 e a sobra carrega — empurrão de medidor nunca se desperdiça.
    ///
    /// Isto substitui o <c>for</c> sobre <c>Equipe1.Membros ++ Equipe2.Membros</c>: ali a equipe 1
    /// jogava antes da equipe 2 o combate inteiro, e essa vantagem de time era estrutural, invisível
    /// e não estava no desenho de ninguém. Com a fila, quem passa na frente passa no MEDIDOR.
    ///
    /// Ninguém consulta ninguém: cada um se calcula pela própria Velocidade. É o que permite ganhar
    /// ou perder Velocidade no meio da luta mudando só o ritmo de quem mudou.
    ///
    /// <b>A previsão da fila (o cordão na tela) ainda não existe e, quando existir, tem de REUSAR
    /// estas regras sobre uma cópia do estado</b> — uma segunda implementação faria a tela prometer
    /// uma ordem que o motor não cumpre, que é o defeito que este desenho existe pra consertar.
    /// </summary>
    public class FilaDeTurnos
    {
        /// <summary>A linha que se cruza pra agir. A barra vai de 0 a 100 e a sobra acima dela é real.</summary>
        public const double Limiar = 100;

        /// <summary>
        /// O CUSTO DA AÇÃO, e ele NÃO é opcional: sem ele o relógio congela entre uma ação e outra e
        /// nenhuma Velocidade alcança ninguém — quem está atrás de um empurrão fica exatamente onde
        /// está enquanto os outros jogam. É o custo que dá à Velocidade um intervalo onde agir.
        ///
        /// A constante gravada é a FRAÇÃO, adimensional: uma ação dura 10% do ciclo de um combatente
        /// de referência. Gravar o `0,05` seria o número certo na unidade errada — ele carrega
        /// escondido a suposição "Velocidade típica ≈ 200" e quebra se a escala do jogo mudar.
        ///
        /// Teto de estabilidade: <c>FRAÇÃO &lt; 100 ÷ (nº em campo)</c>, ou 12,5% num 4×4. Acima
        /// disso o campo gera mais medidor do que uma ação consome, as barras inflam pra sempre e o
        /// trilho de 0 a 100 perde o sentido. Os 10% de hoje têm um quinto de folga; um combate 5×5
        /// derruba o teto pra 10% e encosta.
        /// </summary>
        public const double FracaoDoCiclo = 0.10;

        /// <summary>A Velocidade contra a qual a <see cref="FracaoDoCiclo"/> é escrita.</summary>
        public const int VelocidadeDeReferencia = 200;

        public static double CustoDaAcao => FracaoDoCiclo * Limiar / VelocidadeDeReferencia;

        // Comparar `double` por igualdade crua faria o desempate depender do último bit de uma
        // divisão — e o desempate PRECISA ser determinístico (ver QuemAge).
        private const double Tolerancia = 1e-9;

        private readonly Batalha _batalha;

        public FilaDeTurnos(Batalha batalha) => _batalha = batalha;

        private List<Combate> Vivos => _batalha.Combatentes.Where(c => c.EstaVivo()).ToList();

        /// <summary>
        /// De quem é a vez: adianta o relógio até o primeiro cruzar o limiar e devolve quem age.
        /// <c>null</c> só quando não há mais ninguém que possa agir (ver <see cref="AvancarAteAlguemCruzar"/>).
        /// Não consome nada — quem consome é o <see cref="Consumir"/>, DEPOIS que a ação aconteceu.
        /// </summary>
        public Combate? Proximo()
        {
            AvancarAteAlguemCruzar();
            return QuemAge();
        }

        /// <summary>
        /// Fecha o turno de quem agiu: desconta o limiar (a SOBRA carrega) e cobra o tempo da ação
        /// de todo mundo — cada um enche pela própria Velocidade durante ela.
        ///
        /// Vem DEPOIS da ação, não antes, e isso importa: um empurrão que a habilidade der durante o
        /// turno entra antes do desconto, exatamente como quem o lançou esperava.
        /// </summary>
        public void Consumir(Combate quem)
        {
            quem.DescontarUmTurnoDoMedidor();
            foreach (Combate c in Vivos)
                c.AcumularMedidor(c.Velocidade * CustoDaAcao);
        }

        /// <summary>
        /// Salta EXATO até o instante em que o primeiro cruza o limiar — ninguém passa de 100 por
        /// avanço natural, então sobra só existe por empurrão ou pelo custo da ação. Um laço de
        /// tiques daria o mesmo resultado com erro de arredondamento e N vezes o trabalho.
        ///
        /// Quem tem Velocidade ≤ 0 nunca cruza sozinho: fica de fora da conta do salto (dividir por
        /// ela daria infinito) mas continua na fila — um empurrão ainda o faz agir. Se NINGUÉM tiver
        /// Velocidade, o relógio não tem pra onde andar e a vez fica vazia.
        /// </summary>
        private void AvancarAteAlguemCruzar()
        {
            var vivos = Vivos;
            if (vivos.Any(c => c.Medidor >= Limiar - Tolerancia)) return;

            var andando = vivos.Where(c => c.Velocidade > 0).ToList();
            if (andando.Count == 0) return;

            double tempo = andando.Min(c => (Limiar - c.Medidor) / c.Velocidade);
            foreach (Combate c in andando)
                c.AcumularMedidor(c.Velocidade * tempo);
        }

        /// <summary>
        /// Entre os prontos, o MAIS CHEIO — e só isso.
        ///
        /// <b>Não trocar por "o mais rápido".</b> Chegou-se a propor que o veloz furasse a fila pela
        /// Velocidade, pra não depender do medidor; isso fazia alguém em 105% jogar antes de alguém
        /// em 133%, e aí a barra desenhada na tela vira mentira. Quem quiser passar na frente passa
        /// no medidor.
        ///
        /// O desempate é a POSIÇÃO (a frente primeiro) e depois o lado do jogador. É arbitrário de
        /// propósito e precisa ser FIXO: empate perfeito só acontece com Velocidade idêntica, e o
        /// que não pode existir é a mesma situação produzindo ordens diferentes — a prévia da fila
        /// prometeria o que o motor não cumpre.
        /// </summary>
        private Combate? QuemAge()
        {
            var prontos = Vivos.Where(c => c.Medidor >= Limiar - Tolerancia).ToList();
            if (prontos.Count == 0) return null;

            // O medidor é arredondado ANTES de ordenar: sem isso, dois que cruzaram no mesmo salto
            // se separariam pelo último bit da divisão, e o desempate por posição nunca rodaria.
            return prontos
                .OrderByDescending(c => Math.Round(c.Medidor, 6))
                .ThenBy(c => c.Casa)
                .ThenBy(c => _batalha.EquipeDe(c) == _batalha.Equipe1 ? 0 : 1)
                .First();
        }
    }
}
