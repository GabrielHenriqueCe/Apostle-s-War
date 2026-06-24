using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
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

            if (!portador.PodeReviver)
                return new List<ResultadoReacao>
                {
                    new ResultadoReacao(
                        Mensagem: $"{portador.Personagem.Simbolo} {portador.Personagem.Nome} caiu em batalha e não pode ser ressuscitado.")
                };

            portador.Reviver(portador.HPMaximo / 2);
            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{portador.Personagem.Simbolo} {portador.Personagem.Nome} foi ressuscitado pela Necromancia!")
            };
        }
    }
}