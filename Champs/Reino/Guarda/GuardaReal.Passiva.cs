using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Champs.Reino
{
    /// <summary>
    /// EVITA a morte (IPrevineMorte): consultada pelo Combate.ConfirmarMorte no golpe fatal. O portador
    /// NÃO morre — segue Vivo com 1 HP e ganha Invencível 1 turno, **mantendo todos os status** (não é
    /// revive, é "não morreu"). Por prevenir a morte antes de ela virar Morto, o Vilão (Sentença) e a
    /// Necromância nunca a enxergam. Cooldown 4 gerido pelo ConfirmarMorte.
    /// Nome de jogo "Guarda Real" (era "Invencível", que colidia com o buff Skills.Buffs.Invencivel).
    /// </summary>
    class GuardaReal : HabilidadePassiva, IPrevineMorte
    {
        public GuardaReal() : base("Guarda Real", "⚜️", 4,
            "Ao receber ataque fatal, sobrevive com 1 HP e ganha invencibilidade por 1 turno.")
        { }

        public void Prevenir(Combate combatente)
        {
            combatente.RestaurarVida(1);
            new Invencivel(duracao: 1).Aplicar(combatente);
        }
    }
}
