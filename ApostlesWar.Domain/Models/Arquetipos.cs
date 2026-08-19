namespace ApostlesWar.Domain
{
    /// <summary>
    /// A ficha base dos 4 arquétipos, e a variação por facção. É DADO estático, não serviço — no
    /// molde de <see cref="Faccoes"/> e <see cref="Campanha"/>: tabela pura, fora do grafo de DI.
    ///
    /// Isto substitui os 108 números que viviam soltos nos 36 apóstolos (36 × HP/ATK/DEF). Agora são
    /// 4 fichas + 9 torções, e rebalancear deixou de ser "ajustar 36 arquivos".
    ///
    /// Guarda também o PERFIL DE DISTÂNCIA (o d* de cada tipo): é ficha do tipo, igual ao HP e ao
    /// ATK — a curva de distância é o gesto dele no tabuleiro, não uma regra do motor de combate.
    ///
    /// Fonte: docs/GDD-combate.md §2. Critério de aceitação da tabela, e ele é do Gabriel:
    /// <b>cada tipo é PRIMEIRO em dois stats</b> — Guardião HP/DEF · Combatente os dois de crítico ·
    /// Suporte Precisão/Resistência · Atirador ATK/Velocidade. Uma revisão que deixe um tipo sem
    /// primeiro lugar quebrou a regra.
    /// </summary>
    public static class Arquetipos
    {
        /// <summary>
        /// A ficha de um arquétipo no NÍVEL 1. Só <see cref="HP"/>/<see cref="Ataque"/>/
        /// <see cref="Defesa"/> crescem com o nível; o resto é constante a vida toda.
        /// </summary>
        public readonly record struct Ficha(
            int HP, int Ataque, int Defesa,
            int VelocidadeNv1,
            int Precisao, int Resistencia,
            double TaxaCrit, double DanoCrit);

        /// <summary>
        /// A tabela foi calibrada do nv 60 pra trás e o nv 1 é ela dividida por 30 — o que faz o
        /// fator da curva ser exatamente 30 no topo, sem arredondamento nenhum. Os valores aqui são
        /// os do NÍVEL 1; multiplicar por 30 devolve a tabela do GDD cravada.
        /// </summary>
        private static readonly Dictionary<TipoDeApostolo, Ficha> _fichas = new()
        {
            [TipoDeApostolo.Guardiao]   = new(1000, 17, 50,  85,  50, 120, 0.05, 0.60),
            [TipoDeApostolo.Combatente] = new( 840, 45, 40,  95,  80,  90, 0.25, 0.90),
            [TipoDeApostolo.Suporte]    = new( 670, 27, 32, 105, 150, 150, 0.10, 0.70),
            [TipoDeApostolo.Atirador]   = new( 500, 50, 17, 110, 120,  50, 0.15, 0.80),
        };

        /// <summary>
        /// A torção de cada facção, em PONTOS PERCENTUAIS sobre HP/ATK/DEF, somando zero. Os
        /// 👤 Humanos são o padrão e ficam em zero — é contra eles que as outras se leem.
        ///
        /// **Velocidade fica FORA de propósito** (GDD §2): sendo o stat que decide quem joga, uma
        /// facção com bônus nela domina todas as outras, e nenhum −5% de DEF compensa isso. Precisão,
        /// Resistência e os dois de crítico também não variam — decisão do Gabriel, pra a torção ficar
        /// nos três stats que ele lê primeiro.
        ///
        /// **A soma zero é da PLANILHA, não do poder** (o aviso é do próprio GDD): HP e DEF se
        /// multiplicam entre si — o quanto se aguenta é `HP × 1/(1−redução)` — enquanto o ATK é
        /// linear. Então `+5 ATK / −5 DEF` não se cancela em jogo. A neutralidade real tem de ser
        /// MEDIDA na bancada; esta matriz é o ponto de partida, não o resultado.
        /// </summary>
        private static readonly Dictionary<Faccao, (int HP, int Ataque, int Defesa)> _variacao = new()
        {
            [Faccao.Humanos]     = (  0,   0,   0),
            [Faccao.Reino]       = (  0,  -5,  +5),
            [Faccao.LadoSombrio] = ( +5,   0,  -5),
            [Faccao.Tecnologicos]= ( -5,   0,  +5),
            [Faccao.Folclore]    = (+10,  -5,  -5),
            [Faccao.Misticos]    = ( -5,  +5,   0),
            [Faccao.Especial]    = (  0,  +5,  -5),
            [Faccao.Decaidos]    = ( -5, +10,  -5),
            [Faccao.Ascendentes] = ( +5, -10,  +5),
        };

        public const int NivelMinimo = 1;
        public const int NivelMaximo = 60;

        // === O PERFIL DE DISTÂNCIA (GDD §2) ===
        // Quatro casas por lado, as frentes se olhando: `distância = casa do atacante + casa do
        // alvo − 1`, de 1 (frente × frente) a 7 (fundo × fundo). O multiplicador é máximo na
        // distância ideal do TIPO e cai por casa de desvio, pros dois lados.

        public const int CasaDaFrente = 1;
        public const int CasaDoFundo = 4;
        public const int DistanciaMinima = CasaDaFrente * 2 - 1;
        public const int DistanciaMaxima = CasaDoFundo * 2 - 1;

        // Em CENTÉSIMOS, e é de propósito: `1.30 - 0.10 * 3` em double dá 0,9999999999999999, e o
        // (int) do dano transforma isso em 199 onde se esperava 200. A divisão por 100 no fim é
        // exata pros sete valores da tabela.
        private const int PicoEmCentesimos = 130;
        private const int QuedaPorCasaEmCentesimos = 10;

        /// <summary>
        /// A distância em que cada tipo bate mais forte. O 💗 Suporte não tem — ele rende igual em
        /// qualquer casa, e é o único assim (quem cura e limpa já tem com o que se preocupar).
        ///
        /// Mora aqui porque o d* é do TIPO: é ficha, ao lado do HP e do ATK, não regra de combate.
        /// </summary>
        private static readonly Dictionary<TipoDeApostolo, int> _distanciaIdeal = new()
        {
            [TipoDeApostolo.Guardiao] = 1,
            [TipoDeApostolo.Combatente] = 4,
            [TipoDeApostolo.Atirador] = 5,
        };

        /// <summary>A distância entre duas casas de fileiras opostas. Fora disso não há geometria.</summary>
        public static int DistanciaEntreCasas(int casaAtacante, int casaAlvo)
            => casaAtacante + casaAlvo - 1;

        /// <summary>A distância ideal do tipo, ou <c>null</c> pra quem não tem perfil (o Suporte).</summary>
        public static int? DistanciaIdeal(TipoDeApostolo tipo)
            => _distanciaIdeal.TryGetValue(tipo, out int ideal) ? ideal : null;

        /// <summary>
        /// O multiplicador de dano do tipo NAQUELA distância: 1,30 no pico, −0,10 por casa de desvio.
        /// Função pura — quem descobre as casas e aplica o resultado é o <c>Combate</c>.
        ///
        /// A distância é presa à fila de 7 porque um valor fora dela viraria multiplicador negativo,
        /// e dano negativo cura.
        /// </summary>
        public static double MultiplicadorDePosicao(TipoDeApostolo tipo, int distancia)
        {
            if (DistanciaIdeal(tipo) is not int ideal) return 1.0;

            int naFila = Math.Clamp(distancia, DistanciaMinima, DistanciaMaxima);
            return (PicoEmCentesimos - QuedaPorCasaEmCentesimos * Math.Abs(naFila - ideal)) / 100.0;
        }

        /// <summary>
        /// A curva é CONTÍNUA e vai de 1× a 30× no nv 60: declaram-se as PONTAS e a taxa por nível é
        /// consequência, em vez de uma tabela de 60 linhas pra manter.
        ///
        /// <b>Só o PISO limita.</b> O teto de 60 é do JOGADOR; o inimigo da campanha passa muito dele
        /// (428 no fim do Pesadelo, e lá o fator é 211×). Com um clamp em cima, o inimigo nv 428 seria
        /// tratado como 60 — calado, e o Pesadelo inteiro viraria trivial.
        /// </summary>
        public static double FatorDoNivel(int nivel) =>
            1 + 29.0 * (Math.Max(nivel, NivelMinimo) - 1) / (NivelMaximo - 1);

        public static Ficha Base(TipoDeApostolo tipo) => _fichas[tipo];

        public static (int HP, int Ataque, int Defesa) Variacao(Faccao faccao) => _variacao[faccao];

        /// <summary>Aplica a curva do nível e a torção da facção. Só estes três stats escalam.</summary>
        public static (int HP, int Ataque, int Defesa) StatsDeCombate(
            TipoDeApostolo tipo, Faccao faccao, int nivel)
        {
            Ficha f = Base(tipo);
            var v = Variacao(faccao);
            double fator = FatorDoNivel(nivel);
            return (
                Escalar(f.HP, fator, v.HP),
                Escalar(f.Ataque, fator, v.Ataque),
                Escalar(f.Defesa, fator, v.Defesa));
        }

        /// <summary>
        /// A Velocidade não usa a curva de 30× e não é interpolada: ela anda em DEGRAU, na ESTRELA
        /// — <b>+2 por estrela</b>, então 2·4·6·8·10·12 do nv 10 ao 60 (GDD-combate §1). O ganho é o
        /// mesmo pros quatro tipos, então a ordem entre eles é a da ficha do nv 1 e não muda nunca.
        ///
        /// Só o nv 1 é declarado; o topo é CONSEQUÊNCIA da regra. É isso que impede a divergência
        /// que existia aqui: enquanto as duas pontas eram escritas à mão, o Suporte ficou com 105
        /// nas duas e virou o único tipo que não ganhava nada — sem que nada quebrasse.
        ///
        /// O <c>nivel / 10</c> é a MESMA expressão que conta a estrela na ficha. Trocar por uma
        /// tabela de faixas seria uma segunda fonte pra "quantas estrelas ele tem".
        ///
        /// <b>O clamp de 60 aqui FICA</b> — ao contrário do <see cref="FatorDoNivel"/>, que perdeu o
        /// dele. É ele que impede o inimigo nv 428 do Pesadelo de agir quatro vezes por turno seu:
        /// HP/ATK/DEF podem subir sem teto porque só mudam quanto dói, mas Velocidade é QUANTOS TURNOS
        /// se joga. Tirar isto não quebra teste nenhum de dano — quebra o jogo.
        /// </summary>
        public static int Velocidade(TipoDeApostolo tipo, int nivel)
            => Base(tipo).VelocidadeNv1 + (Math.Clamp(nivel, NivelMinimo, NivelMaximo) / 10) * GanhoPorEstrela;

        /// <summary>O que cada estrela entrega de Velocidade. Ver <see cref="Velocidade"/>.</summary>
        public const int GanhoPorEstrela = 2;

        private static int Escalar(int baseNv1, double fatorNivel, int variacaoPct) =>
            (int)Math.Round(baseNv1 * fatorNivel * (1 + variacaoPct / 100.0));
    }
}
