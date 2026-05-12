using v1_Apostle_s_War.Skills;

namespace ApostlesWar
{
    #region HabilidadeAtiva

    abstract class HabilidadeAtiva : Habilidade
    {
        private static readonly Random _random = new Random();

        public HabilidadeAtiva(string nome, string simbolo, int turnos, string descricao = "")
            : base(nome, simbolo, turnos, descricao) { }

        public abstract int NumeroDeAlvos { get; }
        public abstract TipoAlvo TipoAlvo { get; }

        /// <summary>
        /// Define em qual lista a habilidade age.
        /// O CombateService usa isso para passar a lista correta sem conhecer a habilidade.
        /// </summary>
        public abstract TipoLista TipoLista { get; }

        /// <summary>
        /// Monta a lista de alvos com base em TipoAlvo e NumeroDeAlvos.
        /// O alvo selecionado é sempre o primeiro.
        /// Explicito: percorre em ordem a partir do selecionado, sem repetição.
        /// Aleatorio: sorteia os demais com repetição permitida.
        /// </summary>
        protected List<Combate> ResolverAlvos(Combate alvoSelecionado, List<Combate> lista)
        {
            var vivos = lista.Where(c => c.EstaVivo()).ToList();
            var resultado = new List<Combate>();

            if (vivos.Count == 0) return resultado;

            resultado.Add(alvoSelecionado);

            int extras = NumeroDeAlvos == int.MaxValue
                ? vivos.Count - 1
                : NumeroDeAlvos - 1;

            if (extras <= 0) return resultado;

            if (TipoAlvo == TipoAlvo.Explicito)
            {
                int inicio = vivos.IndexOf(alvoSelecionado);
                for (int i = 1; i <= extras; i++)
                {
                    int idx = (inicio + i) % vivos.Count;
                    Combate proximo = vivos[idx];
                    if (!resultado.Contains(proximo))
                        resultado.Add(proximo);
                }
            }
            else
            {
                for (int i = 0; i < extras; i++)
                    resultado.Add(vivos[_random.Next(vivos.Count)]);
            }

            return resultado;
        }

        protected ResultadoAtaque AplicarDano(Combate atacante, Combate alvo, double multiplicador = 1.0)
            => atacante.AtacarComMultiplicador(alvo, multiplicador);

        protected void AplicarCura(Combate alvo, double percentual)
            => alvo.Curar((int)(alvo.HPMaximo * percentual));

        protected void AplicarBuff(Combate alvo, Buff buff)
            => buff.Aplicar(alvo);

        protected void AplicarDebuff(Combate alvo, Debuff debuff)
            => debuff.Aplicar(alvo);

        protected List<ResultadoAtaque> SemDano() => new List<ResultadoAtaque>();
    }

    #endregion
}
