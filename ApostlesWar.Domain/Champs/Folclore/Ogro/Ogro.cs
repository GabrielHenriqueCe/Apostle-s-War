using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Champs.Folclore
{
    /// <summary>
    /// Ogro — champ como DADO. Quebrar usa PorDanoCausado no escudo (30% do dano total) e o
    /// Irritar carrega proveniência (o overload Func&lt;Combate,Debuff&gt; do AplicarDebuff — o
    /// alvo é forçado a atacar quem aplicou). Passiva: Intimidador.Passiva.cs.
    /// </summary>
    public static class Ogro
    {
        public static Personagem Definir() => new(
            1, Faccao.Folclore, "Ogro", "👹", 1400, 160, 160,
            Esmagar(), Quebrar(), new Intimidador());

        static HabilidadeAtiva Esmagar() => new(
            "Esmagar", "👊", cooldown: 3, "Cura 50% HP em si e protege os aliados (2t).",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Self,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new Cura(Valor.PorHP(0.50), Escopo.ProprioAtacante),
                new AplicarBuff(atk => new ProtecaoAliado(atk, duracao: 2, percentual: 0.30), Escopo.OutrosAliados),
            });

        static HabilidadeAtiva Quebrar() => new(
            "Quebrar", "💥", cooldown: 3, "Ataca todos +100% ATK, Irritar 1t e ganha Escudo de 30% do dano.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano(2.0),
                new AplicarDebuff(atk => new Irritar(atk, duracao: 1)),
                new AplicarEscudo(Valor.PorDanoCausado(0.30), duracao: 2, Escopo.ProprioAtacante),
            });
    }
}
