using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Champs.Tecnologicos
{
    /// <summary>
    /// Alien — champ como DADO. Galáxia é mais um cliente de Escopo.OutrosAliados (a
    /// ProtecaoAliado cai em todos os aliados MENOS o próprio Alien) e usa a sobrecarga de
    /// AplicarBuff com proveniência (o buff carrega quem protege). Passiva:
    /// CarapacaAlienigena.Passiva.cs.
    /// </summary>
    public static class Alien
    {
        public static Personagem Definir() => new(
            2, Faccao.Tecnologicos, "Alien", "👽", 1200, 240, 120,
            Abduzir(), Galaxia(), new CarapacaAlienigena());

        static HabilidadeAtiva Abduzir() => new(
            "Abduzir", "🛸", cooldown: 4, "Incapacita 2 inimigos aleatórios por 1 turno.",
            numeroDeAlvos: 2, tipoAlvo: TipoAlvo.Aleatorio, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarDebuff(() => new Preso(duracao: 1)),
            });

        static HabilidadeAtiva Galaxia() => new(
            "Galáxia", "🌌", cooldown: 4, "+30% DEF em todos. Outros aliados ficam protegidos pelo Alien.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarBuff(() => new BuffDefesa(duracao: 2, percentual: 0.30), Escopo.TodosAliados),
                new AplicarBuff(atacante => new ProtecaoAliado(atacante, duracao: 2, percentual: 0.30), Escopo.OutrosAliados),
            });
    }
}
