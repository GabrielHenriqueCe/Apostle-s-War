using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Champs.LadoSombrio
{
    /// <summary>
    /// Abóbora — champ como DADO (ver ADR-composicao-de-acoes §10). DocesOuTravessuras estreia
    /// RemoverBuffs/Seletor (§5.4/§9). DocesDeAbobora é o 2º cliente de Reviver (dos 7 da
    /// família — §9) e o 1º revive-de-N: o jogador ESCOLHE o morto (pick real por estado,
    /// ADR-selecao-por-estado §2.4 — antes era "primeiro da lista", que aquele ADR já apontava
    /// como dor). Era EstadoAlvo.Ambos + Ativar bespoke; vira duas ações de estados diferentes
    /// (Mortos → Vivos), na ordem — segue o precedente do Nigiri (Champs/Humanos/Sushiman/).
    /// Passiva: CascaDura.Passiva.cs.
    /// </summary>
    public static class Abobora
    {
        public static Personagem Definir() => new(
            3, Faccao.LadoSombrio, "Abóbora", "🎃", 600, 200, 280,
            DocesOuTravessuras(), DocesDeAbobora(), new CascaDura());

        static HabilidadeAtiva DocesOuTravessuras() => new(
            "Doces ou Travessuras", "🍬", cooldown: 4, "Remove benefícios dos inimigos e bloqueia novos por 2 turnos.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new RemoverBuffs(Seletor.Removiveis()),
                new AplicarDebuff(() => new ImpedirBeneficios(duracao: 2)),
            });

        // Revive-de-N canônico (regra do revive, ver Reviver.cs): a habilidade declara o pick
        // (1 morto, Aleatorio = selecionado + extras sorteados se N crescer) e a ação herda os
        // AlvosResolvidos — mesma seleção de qualquer habilidade, sem contador na ação.
        static HabilidadeAtiva DocesDeAbobora() => new(
            "Doces de Abóbora", "🍭", cooldown: 4, "Revive 1 aliado (HP cheio) e aplica Reflexo de dano em todos os aliados.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Aleatorio, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Mortos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new Reviver(1.0, Escopo.AlvosResolvidos),
                new AplicarBuff(() => new RefletirDano(duracao: 2, percentual: 0.15),
                    Escopo.TodosAliados, EstadoAlvo.Vivos),                               // pega o revivido
            });
    }
}
