using ApostlesWar.Skills;

namespace ApostlesWar.Champs.Tecnologicos
{
    /// <summary>
    /// Estende a duração de buffs do alvo conforme um Seletor — espelho EXATO do RemoverBuffs
    /// (mesmo eixo Seletor, ADR-composicao-de-acoes §5.4), trocando só o verbo: em vez de
    /// remover, chama AumentarDuracao. Segue o padrão valência-split do vocabulário
    /// (AplicarBuff/AplicarDebuff, RemoverBuffs/RemoverDebuffs) — o gêmeo EstenderDebuffs
    /// nasceria só quando houver um cliente ativo pra ele.
    ///
    /// Bespoke-LOCAL do Robô (ADR §9): o RaioX é o ÚNICO cliente ATIVO hoje. Promover pra
    /// Skills/Acoes/ no 2º cliente ATIVO real.
    ///
    /// ATENÇÃO — NÃO confundir clientes: as passivas que também mexem em duração de status
    /// (Policial/AlgemasReforçadas estende Preso; Repetindo estende debuff aleatório;
    /// AnáliseCrítica reduz buffs) NÃO são clientes desta Ação — são REAÇÕES (IReageAo*),
    /// dispatch diferente. O que elas compartilham com esta Ação é o PRIMITIVO
    /// (StatusEffect.AumentarDuracao/ReduzirDuracao, já unificado na base) e o CONCEITO de
    /// Seletor. A união do "pegar status via Seletor" entre passivas e ações é fio FUTURO —
    /// ver a nota no ADR §9 ("Unir a seleção de status entre passivas e ações"): PARAR e
    /// reavaliar quando o sweep tocar cada passiva de duração (AnáliseCrítica nos Tecnológicos;
    /// Repetindo em Apóstolos), não agora.
    /// </summary>
    class EstenderBuffs : Acao
    {
        private readonly Seletor _seletor;
        private readonly int _turnos;

        public EstenderBuffs(Seletor seletor, int turnos, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : base(escopo, estadoAlvo)
        {
            _seletor = seletor;
            _turnos = turnos;
        }

        public override void Executar(Combate atacante, Combate alvo, List<EventoDano> eventos)
        {
            IEnumerable<Buff> candidatos = alvo.StatusAtivos.OfType<Buff>().Where(b => _seletor.Filtro(b));
            if (_seletor.Aleatorio)
                candidatos = candidatos.OrderBy(_ => Guid.NewGuid());

            foreach (var buff in candidatos.Take(_seletor.Quantos).ToList())
                buff.AumentarDuracao(_turnos);
        }
    }
}
