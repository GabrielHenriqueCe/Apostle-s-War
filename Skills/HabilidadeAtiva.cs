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
        /// Define qual lista a habilidade considera como "principal" pra selecionar alvos.
        /// O CombateService usa isso pra mostrar a lista certa de alvos pro jogador.
        /// A habilidade ainda tem acesso às duas listas via ContextoCombate.
        /// </summary>
        public abstract TipoLista TipoLista { get; }

        /// <summary>
        /// Retorna a lista correspondente ao TipoLista da habilidade.
        /// Conveniência pra resolver alvos.
        /// </summary>
        protected List<Combate> ObterListaPrincipal(ContextoCombate ctx) => TipoLista switch
        {
            TipoLista.Aliados => ctx.Aliados,
            TipoLista.Inimigos => ctx.Inimigos,
            TipoLista.Self => new List<Combate> { ctx.Atacante },
            _ => ctx.Inimigos
        };

        /// <summary>
        /// Monta a lista de alvos com base em TipoAlvo e NumeroDeAlvos.
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