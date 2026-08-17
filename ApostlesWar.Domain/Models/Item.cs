namespace ApostlesWar.Domain
{
    #region Item

    /// <summary>
    /// Representa um item equipável obtido ao concluir uma fase
    /// </summary>
    public class Item
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
        private double CalcularValor(Faccao faccao, Fases fase, TipoStat tipoStat)
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

        // COMO o valor é ESCRITO na tela ("5%" vs "0,05", o rótulo "Dano Crit") não mora aqui: o Item
        // guarda o número e o tipo, e cada pele decide a apresentação. Formatar aqui dentro põe `:F0`
        // e sufixo `%` no domínio de regras.
    }

    #endregion
}