using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Apostolos.Folclore
{
    /// <summary>
    /// Ogro — apóstolo como DADO. Quebrar usa PorDanoCausado no escudo (30% do dano total) e o
    /// Irritar carrega proveniência (o overload Func&lt;Combate,Debuff&gt; do AplicarDebuff — o
    /// alvo é forçado a atacar quem aplicou). Passiva: Intimidador.Passiva.cs.
    /// </summary>
    public static class Ogro
    {
        public static Personagem Definir() => new(
            1, Faccao.Folclore, "Ogro", "👹", 1400, 160, 160,
            Esmagar(), Quebrar(), new Intimidador());

        static HabilidadeAtiva Esmagar() => new(
            "Esmagar", "👊", cooldown: 3, "Se cura com 25% HP e protege os aliados por 2 turnos.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Self,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new Cura(Valor.PorHP(0.25), Escopo.ProprioAtacante),
                new AplicarBuff(atk => new ProtecaoAliado(atk, duracao: 2, percentual: 0.30), Escopo.OutrosAliados),
            });

        static HabilidadeAtiva Quebrar() => new(
            "Quebrar", "💥", cooldown: 3, "Ataca todos com 300% do ATK e aplica Irritar de 1 turno" +
            "\nEntão ganha Escudo com 30% do dano causado.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano(3.0),
                new AplicarDebuff(atk => new Irritar(atk, duracao: 1)),
                new AplicarEscudo(Valor.PorDanoCausado(0.30), duracao: 2, Escopo.ProprioAtacante),
            });
    }
}
