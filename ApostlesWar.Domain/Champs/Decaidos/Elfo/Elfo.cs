using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Champs.Decaidos
{
    /// <summary>
    /// Elfo — champ como DADO. Árvore do Mundo é tanque puro (buffs próprios, molde da
    /// Furtividade). Natureza ataca 1 e prende. Passiva: EspinhosCorrompidos.Passiva.cs (renomeada
    /// de "Espinhos" pra não colidir no display com o buff EspinhosVenenosos, que exibe "Espinhos").
    /// </summary>
    public static class Elfo
    {
        public static Personagem Definir() => new(
            3, Faccao.Decaidos, "Elfo", "🧝", 1400, 160, 160,
            ArvoreDoMundo(), Natureza(), new EspinhosCorrompidos());

        static HabilidadeAtiva ArvoreDoMundo() => new(
            "Árvore do Mundo", "🌳", cooldown: 3, "Provocar + Refletir Dano + Contra-Ataque em si mesmo (2t).",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Self,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarBuff(() => new Provocar(duracao: 2), Escopo.ProprioAtacante),
                new AplicarBuff(() => new RefletirDano(duracao: 2), Escopo.ProprioAtacante),
                new AplicarBuff(() => new ContraAtaque(duracao: 2), Escopo.ProprioAtacante),
            });

        static HabilidadeAtiva Natureza() => new(
            "Natureza", "🌿", cooldown: 3, "Ataca 1 inimigo com +50% ATK e aplica Preso por 1 turno.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new Dano(1.5),
                new AplicarDebuff(() => new Preso(duracao: 1)),
            });
    }
}
