namespace ApostlesWar.Domain
{
    /// <summary>
    /// A TABELA DOS SLOTS: como cada peça se chama, o que ela pode dar de principal e quanto esse
    /// principal vale no teto. É a ficha técnica do equipamento (docs/GDD-itens.md §OS 9 SLOTS), e
    /// mora no Domain porque é regra do jogo — quem só orquestra é o <c>ArsenalService</c>.
    ///
    /// <b>Os valores da tabela são o TETO: mítico, nível 60.</b> Todo item cai no nível 1 valendo
    /// 11,5% disso e conquista o resto jogando (<see cref="FatorNivel"/>). É por isso que um mítico
    /// recém-caído é mais fraco que o comum que você já subiu — e é o que impede o drop de apagar o
    /// investimento.
    ///
    /// Os 2 acessórios das dungeons (pulseira, colar) ainda não existem: eles não caem na campanha,
    /// então não têm fase, e entram quando as dungeons entrarem.
    /// </summary>
    public static class Equipamento
    {
        /// <summary>
        /// Como se chama o slot da fase — e, por tabela, o item que cai nela.
        ///
        /// A tela do boneco precisa nomear os 7 slots mesmo quando estão VAZIOS (não há Item pra
        /// perguntar), e é por isso que isto é público: sem ele o front mantinha a própria cópia da
        /// lista, que envelheceu — chamava a Fase 4 de "Acessório" enquanto o item que entra nela
        /// nasce "Manopla". Uma tabela só, um nome só.
        /// </summary>
        public static string NomeDoSlot(Fases fase) => fase switch
        {
            Fases.Fase1 => "Arma",
            Fases.Fase2 => "Elmo",
            Fases.Fase3 => "Escudo",
            Fases.Fase4 => "Manopla",
            Fases.Fase5 => "Peitoral",
            Fases.Fase6 => "Calça",
            Fases.Fase7 => "Bota",
            _ => throw new ArgumentOutOfRangeException(nameof(fase))
        };

        // Os três de valor CHEIO são FIXOS: a Arma é sempre ATK, o Elmo sempre HP, o Escudo sempre
        // DEF. Os quatro de PERCENTUAL são VARIÁVEIS — o principal é sorteado no drop entre o trio
        // ATK%/HP%/DEF% e o especial daquela peça, e é isso que faz dois drops do mesmo slot serem
        // decisões diferentes mesmo antes de a raridade existir.
        private static readonly TipoStat[] Trio =
            { TipoStat.ATKPct, TipoStat.HPPct, TipoStat.DEFPct };

        /// <summary>
        /// O que o slot da fase pode dar de principal. Um item sorteia UM destes ao cair.
        ///
        /// <b>A Velocidade tem fonte ÚNICA no jogo inteiro</b> (a Bota), e é de propósito: a faixa
        /// entre os arquétipos é de 30 pontos, então uma segunda fonte faria o número do tipo virar
        /// ruído. Ver o §OS 9 SLOTS.
        /// </summary>
        public static IReadOnlyList<TipoStat> OpcoesDoSlot(Fases fase) => fase switch
        {
            Fases.Fase1 => new[] { TipoStat.ATKFlat },
            Fases.Fase2 => new[] { TipoStat.HPFlat },
            Fases.Fase3 => new[] { TipoStat.DEFFlat },
            Fases.Fase4 => Trio.Append(TipoStat.TaxaCritPct).Append(TipoStat.DanoCritPct).ToArray(),
            Fases.Fase5 => Trio.Append(TipoStat.ResistenciaFlat).ToArray(),
            Fases.Fase6 => Trio.Append(TipoStat.PrecisaoFlat).ToArray(),
            Fases.Fase7 => Trio.Append(TipoStat.VelocidadeFlat).ToArray(),
            _ => throw new ArgumentOutOfRangeException(nameof(fase))
        };

        /// <summary>
        /// Quanto o principal vale no TETO (mítico, nível 60). Cada número tem origem, e nenhum é
        /// chute solto (§Como os valores foram derivados):
        ///
        /// <list type="bullet">
        /// <item>ATK%/HP%/DEF% <b>50%</b> — o degrau de referência de todos os outros.</item>
        /// <item>ATK/HP/DEF cheios — <b>50% da base MÉDIA dos 4 tipos</b> no nível 60.</item>
        /// <item>Velocidade <b>+50</b> — os mesmos 50% sobre a base média (≈100). A Bota vale o que
        /// qualquer outro principal vale, nem mais.</item>
        /// <item>Precisão e Resistência <b>+125</b> — 250 no total, divididos entre a peça de
        /// armadura e o Colar de dungeon.</item>
        /// <item>Taxa e Dano Crítico <b>50/100</b> — a proporção 2:1 contra a Pulseira, que é o que
        /// faz nenhuma opção de luva dominar.</item>
        /// </list>
        /// </summary>
        public static double Maximo(TipoStat stat) => stat switch
        {
            TipoStat.ATKFlat => 500,
            TipoStat.HPFlat => 11_000,
            TipoStat.DEFFlat => 500,
            TipoStat.ATKPct => 0.50,
            TipoStat.HPPct => 0.50,
            TipoStat.DEFPct => 0.50,
            TipoStat.TaxaCritPct => 0.50,
            TipoStat.DanoCritPct => 1.00,
            TipoStat.VelocidadeFlat => 50,
            TipoStat.PrecisaoFlat => 125,
            TipoStat.ResistenciaFlat => 125,
            _ => 0
        };

        /// <summary>
        /// Quanto do teto o nível libera: <c>10 + 1,5 × nível</c>, em porcentagem. Nível 1 dá 11,5%,
        /// nível 60 dá 100%, e as seis dezenas caem exatamente nos seis valores da grade
        /// (25 · 40 · 55 · 70 · 85 · 100%).
        ///
        /// <b>Cada dezena comprada entrega +15 pontos de principal</b> — é a maior compra que existe
        /// na peça, e é o que faz o pedágio não ser imposto.
        /// </summary>
        public static double FatorNivel(int nivel)
            => (10 + 1.5 * Math.Clamp(nivel, Arquetipos.NivelMinimo, Arquetipos.NivelMaximo)) / 100.0;
    }
}
