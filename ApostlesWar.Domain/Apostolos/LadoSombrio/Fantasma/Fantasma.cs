using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Apostolos.LadoSombrio
{
    /// <summary>
    /// Fantasma — apóstolo como DADO (ver ADR-composicao-de-acoes §10). VindoDoAlem usa a Ação
    /// bespoke local AutoDano.cs (Nível 2, §9). Passiva: Espectral.Passiva.cs.
    /// </summary>
    public static class Fantasma
    {
        public static Personagem Definir() => new(
            2, Faccao.LadoSombrio, "Fantasma", "👻", 1400, 120, 200,
            Assombracao(), VindoDoAlem(), new Espectral());

        // Cura AGREGADA (decisão de Gabriel): 20% da SOMA do dano causado no ataque inteiro,
        // uma vez ao fim — não 20% por hit. É o fragmento PorDanoCausado lendo o eventos completo.
        static HabilidadeAtiva Assombracao() => new(
            "Assombração", "👻", cooldown: 3, "Ataca todos os inimigos 350% ATK.\nCura 20% de todo o dano causado.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano(3.5),
                new Cura(Valor.PorDanoCausado(0.20), Escopo.ProprioAtacante),
            });

        static HabilidadeAtiva VindoDoAlem() => new(
            "Vindo do Além", "💀", cooldown: 6, "Ataca um inimigo com 300% do ATK. Esse ataque é sempre crítico e ignora 100% DEF." +
            "\nSofre 20% do dano causado em si mesmo.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new Dano(3.0, ignorarDefesaPct: 1.0, forcaCritico: true),
                new AutoDano(Valor.PorDanoCausado(0.20)),
            });
    }
}
