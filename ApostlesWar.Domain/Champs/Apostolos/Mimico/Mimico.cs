using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills;

namespace ApostlesWar.Domain.Champs.Apostolos
{
    /// <summary>
    /// Mímico — champ como DADO. Imitação escala o dano com os buffs do próprio Mímico via a
    /// sobrecarga `Dano(Func)` (molde do CorteDeVento do Tengu). Copiando estreia a ação
    /// `MoverBuffs` (roubo de buff) + reusa o `ConcederTurnoExtra` (construído nos Decaídos pro
    /// Rato Voador). Passiva: Repetindo.Passiva.cs.
    /// </summary>
    public static class Mimico
    {
        public static Personagem Definir() => new(
            2, Faccao.Apostolos, "Mímico", "🎭", 1000, 200, 200,
            Imitacao(), Copiando(), new Repetindo());

        static HabilidadeAtiva Imitacao() => new(
            "Imitação", "🎭", cooldown: 3, "Ataca todos. +25% ATK por buff ativo (cap 4).",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano(MultPorBuffsDoMimico),
            });

        static HabilidadeAtiva Copiando() => new(
            "Copiando", "📋", cooldown: 4, "Rouba todos os buffs de um inimigo e ganha turno extra.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new MoverBuffs(Seletor.Removiveis()),
                new ConcederTurnoExtra(),
            });

        // Multiplicador da Imitação: 1.0 + 25% por buff ativo NO MÍMICO, cap em 4 buffs (+100%).
        static double MultPorBuffsDoMimico(Combate atacante, Combate alvo)
        {
            int buffs = Math.Min(atacante.StatusAtivos.OfType<Buff>().Count(), 4);
            return 1.0 + 0.25 * buffs;
        }
    }
}
