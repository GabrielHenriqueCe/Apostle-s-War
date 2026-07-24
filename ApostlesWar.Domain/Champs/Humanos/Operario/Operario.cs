using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;

namespace ApostlesWar.Domain.Champs.Humanos
{
    /// <summary>
    /// Operário — champ como DADO. A Marretada mora ao lado (Marretada.Ativa.cs) porque é
    /// híbrida: o InstintoDoOperario a busca por tipo pro contra-ataque.
    /// </summary>
    public static class Operario
    {
        public static Personagem Definir() => new(
            1, Faccao.Humanos, "Operário", "👷", 1200, 240, 120,
            ParedeDeTijolos(), new Marretada(), new InstintoDoOperario());

        static HabilidadeAtiva ParedeDeTijolos() => new(
            "Parede de Tijolos", "🧱", cooldown: 6, "Bloqueia 100% do dano de todos os aliados por 1 turno.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarBuff(() => new BloqueioTotal(), Escopo.TodosAliados),
            });
    }
}
