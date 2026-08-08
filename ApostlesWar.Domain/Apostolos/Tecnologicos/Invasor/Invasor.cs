using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Apostolos.Tecnologicos
{
    /// <summary>
    /// Invasor — apóstolo como DADO (ver ADR-composicao-de-acoes §10). Barata estreia no sweep o
    /// padrão estado/ao-matar do motor (ADR §3.2/§3.3): ataca e, na MESMA lista de ações, aplica
    /// a Sentença num AplicarDebuff de EstadoAlvo.Mortos — a ação de Mortos, avaliada NA EXECUÇÃO,
    /// pega exatamente quem o Dano anterior acabou de matar, sem condicional "se matou". Passiva:
    /// Virus.Passiva.cs.
    /// </summary>
    public static class Invasor
    {
        public static Personagem Definir() => new(
            1, Faccao.Tecnologicos, "Invasor", "👾", TipoDeApostolo.Combatente,
            Glitch(), Barata(), new Virus());

        static HabilidadeAtiva Glitch() => new(
            "Glitch", "📺", cooldown: 3, "+50% ATK em si, -30% DEF no alvo, ataca 1 inimigo com 400% do ATK.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new AplicarBuff(() => new BuffAtaque(duracao: 2, percentual: 0.50), Escopo.ProprioAtacante),
                new AplicarDebuff(() => new ReducaoDefesa(duracao: 2)),
                new Dano(4.0),
            });

        static HabilidadeAtiva Barata() => new(
            "Barata", "🪳", cooldown: 3, "Aplica intocável em si mesmo por 2 turnos, então ataca 1 inimigo com 400% ATK." +
            "\nInimigos mortos por essa habilidade não podem reviver.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new AplicarBuff(() => new Intocavel(duracao: 2), Escopo.ProprioAtacante),
                new Dano(4.0),
                new AplicarDebuff(() => new ImpedirRessurreicao(), Escopo.AlvosResolvidos, EstadoAlvo.Mortos),
            });
    }
}
