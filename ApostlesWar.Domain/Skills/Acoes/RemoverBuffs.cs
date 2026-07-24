using ApostlesWar.Domain.Skills;

namespace ApostlesWar.Domain
{
    /// <summary>
    /// Remove buffs do alvo conforme um Seletor (ADR-composicao-de-acoes §5.4/§9 — "cravada",
    /// 1º cliente: DocesOuTravessuras). Espelho futuro de RemoverDebuffs (cleanse), que ainda
    /// não tem cliente migrado.
    /// </summary>
    public class RemoverBuffs : Acao
    {
        private readonly Seletor _seletor;

        public RemoverBuffs(Seletor seletor, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : base(escopo, estadoAlvo) => _seletor = seletor;

        public override void Executar(Combate atacante, Combate alvo, List<EventoCombate> eventos)
        {
            IEnumerable<Buff> candidatos = alvo.StatusAtivos.OfType<Buff>().Where(b => _seletor.Filtro(b));
            if (_seletor.Aleatorio)
                candidatos = candidatos.OrderBy(_ => Guid.NewGuid());

            foreach (var buff in candidatos.Take(_seletor.Quantos).ToList())
                buff.Remover(alvo);
        }
    }
}
