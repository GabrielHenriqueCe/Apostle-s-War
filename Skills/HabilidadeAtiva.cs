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
        /// Define qual estado de vida a habilidade mira dentro da TipoLista. Sem default —
        /// toda habilidade declara conscientemente (ver ADR-selecao-por-estado.md).
        /// </summary>
        public abstract EstadoAlvo EstadoAlvo { get; }

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
        /// Filtra a lista pelo EstadoAlvo declarado pela habilidade. Ambos não usa este
        /// caminho (a habilidade resolve as duas seleções sozinha no Ativar) — cai no
        /// default (Vivos) só por segurança de compilação, nunca deveria ser chamado.
        /// </summary>
        private List<Combate> FiltrarPorEstado(List<Combate> lista) => EstadoAlvo switch
        {
            EstadoAlvo.Mortos => lista.Where(c => !c.EstaVivo()).ToList(),
            _ => lista.Where(c => c.EstaVivo()).ToList(),
        };

        /// <summary>
        /// Monta a lista de alvos com base em TipoAlvo, NumeroDeAlvos e EstadoAlvo.
        /// </summary>
        protected List<Combate> ResolverAlvos(Combate alvoSelecionado, List<Combate> lista)
        {
            var candidatos = FiltrarPorEstado(lista);
            var resultado = new List<Combate>();

            if (candidatos.Count == 0) return resultado;

            resultado.Add(alvoSelecionado);

            int extras = NumeroDeAlvos == int.MaxValue
                ? candidatos.Count - 1
                : NumeroDeAlvos - 1;

            if (extras <= 0) return resultado;

            if (TipoAlvo == TipoAlvo.Explicito)
            {
                int inicio = candidatos.IndexOf(alvoSelecionado);
                for (int i = 1; i <= extras; i++)
                {
                    int idx = (inicio + i) % candidatos.Count;
                    Combate proximo = candidatos[idx];
                    if (!resultado.Contains(proximo))
                        resultado.Add(proximo);
                }
            }
            else
            {
                for (int i = 0; i < extras; i++)
                    resultado.Add(candidatos[_random.Next(candidatos.Count)]);
            }

            return resultado;
        }

        protected EventoDano AplicarDano(Combate atacante, Combate alvo, double multiplicador = 1.0)
            => atacante.Atacar(alvo, multiplicador);

        protected void AplicarCura(Combate alvo, double percentual)
            => alvo.Curar((int)(alvo.HPMaximo * percentual));

        protected void AplicarBuff(Combate alvo, Buff buff)
            => buff.Aplicar(alvo);

        protected void AplicarDebuff(Combate alvo, Debuff debuff)
            => debuff.Aplicar(alvo);

        protected List<EventoDano> SemDano() => new List<EventoDano>();

        /// <summary>
        /// Semântica do evento de ataque. Default Sequencial (a maioria das habilidades de dano
        /// single-target ou multi-hit). Habilidades AoE e não-atacantes sobrescrevem.
        /// O CombateService usa isso pra decidir quantas vezes dispara passivas DepoisDeAtacar.
        /// </summary>
        public virtual TipoAtaque TipoAtaque => TipoAtaque.Sequencial;
    }

    #endregion
}