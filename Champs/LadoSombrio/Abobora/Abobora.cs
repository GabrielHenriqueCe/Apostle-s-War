using ApostlesWar;
using ApostlesWar.Skills.Buffs;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Champs.LadoSombrio
{
    /// <summary>
    /// Abóbora — champ como DADO (ver ADR-composicao-de-acoes §10). DocesOuTravessuras estreia
    /// RemoverBuffs/Seletor (§5.4/§9). DocesDeAbobora é o 2º cliente de Reviver (dos 7 da
    /// família — §9), revive só 1 (quantos:1). Era EstadoAlvo.Ambos + Ativar bespoke; vira duas
    /// ações de estados diferentes (Mortos → Vivos), na ordem — segue o precedente do Nigiri
    /// (Champs/Humanos/Sushiman/). Passiva: CascaDura.Passiva.cs.
    /// </summary>
    static class Abobora
    {
        public static Personagem Definir() => new(
            3, Faccao.LadoSombrio, "Abóbora", "🎃", 600, 200, 280,
            DocesOuTravessuras(), DocesDeAbobora(), new CascaDura());

        static HabilidadeAtiva DocesOuTravessuras() => new(
            "Doces ou Travessuras", "🍬", turnos: 4, "Remove benefícios dos inimigos e bloqueia novos por 2 turnos.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new RemoverBuffs(Seletor.Removiveis()),
                new AplicarDebuff(() => new ImpedirBeneficios(turnos: 2)),
            });

        static HabilidadeAtiva DocesDeAbobora() => new(
            "Doces de Abóbora", "🍭", turnos: 4, "Revive 1 aliado e aplica Reflexo de dano em todos os aliados.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new Reviver(1.0, quantos: 1),                                            // TodosAliados/Mortos (defaults)
                new AplicarBuff(() => new RefletirDano(turnos: 2, percentual: 0.15),
                    Escopo.TodosAliados, EstadoAlvo.Vivos),                               // pega o revivido
            });
    }
}
