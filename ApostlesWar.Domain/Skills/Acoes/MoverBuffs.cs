using ApostlesWar.Domain.Skills;

namespace ApostlesWar.Domain
{
    /// <summary>
    /// Move buffs do alvo PRO ATACANTE conforme um Seletor — gêmeo do RemoverBuffs, mas em vez de
    /// remover, migra a INSTÂNCIA (do StatusAtivos do alvo pro do atacante). Nome reservado no
    /// vocabulário (ADR-composicao-de-acoes §9); 1º cliente: Copiando (Mímico, Apóstolos — roubo
    /// de buff + turno extra).
    ///
    /// Mover a instância (não recriar) preserva o trade-off do Copiando original (ADR §356): buffs
    /// "transparentes" (Escudo/ProtecaoAliado/ContraAtaque) passam a valer pro atacante na hora;
    /// buffs que já mexeram no stat do alvo (BuffAtaque/BuffDefesa) não re-aplicam o efeito ao
    /// atacante — aceito. Destino é sempre o conjurador (o único caso do jogo); se um 2º destino
    /// aparecer, nasce um parâmetro `destino` (esboçado no ADR §274, omitido por YAGNI).
    /// </summary>
    public class MoverBuffs : Acao
    {
        private readonly Seletor _seletor;

        public MoverBuffs(Seletor seletor, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : base(escopo, estadoAlvo) => _seletor = seletor;

        public override void Executar(Combate atacante, Combate alvo, List<EventoCombate> eventos)
        {
            IEnumerable<Buff> candidatos = alvo.StatusAtivos.OfType<Buff>().Where(b => _seletor.Filtro(b));
            if (_seletor.Aleatorio)
                candidatos = candidatos.OrderBy(_ => Guid.NewGuid());

            foreach (var buff in candidatos.Take(_seletor.Quantos).ToList())
            {
                alvo.StatusAtivos.Remove(buff);
                atacante.StatusAtivos.Add(buff);
            }
        }
    }
}
