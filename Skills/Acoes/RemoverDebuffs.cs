using ApostlesWar.Skills;

namespace ApostlesWar
{
    /// <summary>
    /// Remove debuffs do alvo conforme um Seletor — gêmeo EXATO do RemoverBuffs (troca só
    /// OfType&lt;Buff&gt; por OfType&lt;Debuff&gt;). Verbo que já estava mapeado no vocabulário
    /// (ADR-composicao-de-acoes §9 / catálogo §3); 1º cliente: Coringa (Palhaço, Folclore —
    /// cleanse total dos aliados + ImunidadeDebuffs).
    /// </summary>
    class RemoverDebuffs : Acao
    {
        private readonly Seletor _seletor;

        public RemoverDebuffs(Seletor seletor, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : base(escopo, estadoAlvo) => _seletor = seletor;

        public override void Executar(Combate atacante, Combate alvo, List<EventoDano> eventos)
        {
            IEnumerable<Debuff> candidatos = alvo.StatusAtivos.OfType<Debuff>().Where(d => _seletor.Filtro(d));
            if (_seletor.Aleatorio)
                candidatos = candidatos.OrderBy(_ => Guid.NewGuid());

            foreach (var debuff in candidatos.Take(_seletor.Quantos).ToList())
                debuff.Remover(alvo);
        }
    }
}
