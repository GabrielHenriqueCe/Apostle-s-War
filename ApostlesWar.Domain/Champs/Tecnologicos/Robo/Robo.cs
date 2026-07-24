using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Champs.Tecnologicos
{
    /// <summary>
    /// Robô — champ como DADO. RaioX estreia o EstenderBuffs (ver EstenderBuffs.cs): a cura e a
    /// extensão de duração são AÇÕES SEPARADAS na lista — princípio do motor (ADR §3.3): quando
    /// uma habilidade faz N coisas, decompõe em N ações do vocabulário, não junta num bespoke.
    /// Tecnology é o 3º da família do Reviver (revive-de-todos, ADR §9): a Reviver pega os
    /// mortos, a CuraContinua pega os vivos — os revividos entram no buff pela ORDEM das ações.
    /// Passiva: ReparoAutomatico.Passiva.cs.
    /// </summary>
    static class Robo
    {
        public static Personagem Definir() => new(
            3, Faccao.Tecnologicos, "Robô", "🤖", 1200, 120, 240,
            RaioX(), Tecnology(), new ReparoAutomatico());

        static HabilidadeAtiva RaioX() => new(
            "Raio-X", "🩻", cooldown: 4, "Cura 15% HP e estende em 1t os benefícios de todos os aliados.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new Cura(Valor.PorHP(0.15), Escopo.TodosAliados),
                new EstenderBuffs(Seletor.Todos(), turnos: 1, Escopo.TodosAliados),
            });

        static HabilidadeAtiva Tecnology() => new(
            "Technology", "🤖", cooldown: 4, "Revive aliados (30% HP) e aplica Cura Contínua em todo o time.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new Reviver(0.30),
                new AplicarBuff(() => new CuraContinua(duracao: 1, percentual: 0.10), Escopo.TodosAliados),
            });
    }
}
