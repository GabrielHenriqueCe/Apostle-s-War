namespace ApostlesWar
{
    #region Item

    /// Tipos de stat que um item pode alterar
    enum TipoStat { ATKFlat, HPFlat, DEFFlat, HPPct, DEFPct, TaxaCritPct, DanoCritPct }

    /// <summary>
    /// Representa um item equipável obtido ao concluir uma fase
    /// </summary>
    class Item
    {
        public string Nome { get; init; }
        public string Simbolo { get; init; }
        public Faccao Faccao { get; init; }
        public Fases Fase { get; init; }
        public TipoStat TipoStat { get; init; }
        public double Valor { get; init; }

        public Item(string nome, string simbolo, Faccao faccao, Fases fase, TipoStat tipoStat)
        {
            Nome = nome;
            Simbolo = simbolo;
            Faccao = faccao;
            Fase = fase;
            TipoStat = tipoStat;
            Valor = CalcularValor(faccao, fase, tipoStat);
        }

        public Item()
        {
            Nome = null!;
            Simbolo = null!;
        }

        /// <summary>
        /// Calcula o valor do stat do item com base no capítulo e tipo de stat
        /// </summary>
        private static double CalcularValor(Faccao faccao, Fases fase, TipoStat tipoStat)
        {
            int cap = (int)faccao;
            return tipoStat switch
            {
                TipoStat.ATKFlat => 120 * cap,
                TipoStat.HPFlat => 550 * cap,
                TipoStat.DEFFlat => 55 * cap,
                TipoStat.HPPct => 0.05 * cap,
                TipoStat.DEFPct => 0.05 * cap,
                TipoStat.TaxaCritPct => 0.05 + 0.01 * cap,
                TipoStat.DanoCritPct => 0.15 + 0.01 * cap,
                _ => 0
            };
        }

        /// <summary>
        /// Retorna o valor formatado conforme o tipo de stat
        /// </summary>
        public string ValorFormatado()
        {
            return TipoStat switch
            {
                TipoStat.ATKFlat or TipoStat.HPFlat or TipoStat.DEFFlat
                    => $"{(int)Valor}",
                _
                    => $"{Valor * 100:F0}%"
            };
        }

        public string NomeStat() => TipoStat switch
        {
            TipoStat.ATKFlat => "ATK",
            TipoStat.HPFlat => "HP",
            TipoStat.DEFFlat => "DEF",
            TipoStat.HPPct => "HP",
            TipoStat.DEFPct => "DEF",
            TipoStat.TaxaCritPct => "Crit",
            TipoStat.DanoCritPct => "Dano Crit",
            _ => ""
        };
    }

    #endregion
}