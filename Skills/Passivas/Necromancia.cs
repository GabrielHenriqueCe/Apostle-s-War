using ApostlesWar;

namespace ApostlesWar.Skills.Passivas
{
    /// <summary>
    /// Ao morrer, revive o portador com 50% do HP máximo — se não estiver bloqueado
    /// (PodeReviver, ex: Vilao). Reage pós-morte (IReageAoMorrer), depois do "ao matar".
    /// Cooldown 6 (consumido ao reagir, revivendo ou não — bloqueio e cooldown são
    /// independentes).
    /// </summary>
    class Necromancia : HabilidadePassiva, IReageAoMorrer
    {
        public Necromancia() : base("Necromancia", "🪦", 6,
            "Revive o personagem com 50% do HP máximo ao morrer.")
        { }

        public List<ResultadoReacao> AoMorrer(ContextoReacao ctx)
        {
            var portador = ctx.Portador;

            portador.Reviver(portador.HPMaximo / 2);

            // Se o revive foi bloqueado pela Sentença, o Reviver não fez nada (portador
            // segue morto). A mensagem diferenciada (reviveu vs bloqueado) fica pra
            // refatoração de exibição/front — ver roadmap. Por ora, declara a tentativa.
            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: portador.EstaVivo()
                        ? $"{portador.Personagem.Simbolo} {portador.Personagem.Nome} foi ressuscitado pela Necromancia!"
                        : $"{portador.Personagem.Simbolo} {portador.Personagem.Nome} caiu em batalha e não pode ser ressuscitado.")
            };
        }
    }
}