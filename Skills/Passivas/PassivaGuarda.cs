using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao morrer (com cooldown), sobrevive com 1 HP e ganha Invencível 1 turno.
    /// Migrada pro modelo de reação (IReageAoMorrer) — pós-morte, como a Necromancia.
    ///
    /// PROVISÓRIO [sistema-morte-como-estado]: hoje é um HACK de revive (deixa morrer
    /// → Reviver(1) → Invencível). O CONCEITO certo é PREVENÇÃO de morte (pré-morte):
    /// quando o golpe seria letal, aplicar Invencível ANTES de morrer, sem nunca chegar
    /// a 0 HP. Isso exige o gancho de morte-iminente (IReageAntesDeMorrer) dentro do
    /// sistema de morte-como-estado (estado vivo/morto explícito, transições, bloquear-
    /// revive como status do morto). Quando esse sistema for feito, a Guarda nasce limpa
    /// como "previne a transição vivo→morto" e este hack sai. Ver "Estado morto" no roadmap.
    /// </summary>
    class PassivaGuarda : HabilidadePassiva, IReageAoMorrer
    {
        public PassivaGuarda() : base("Invencível", "⚜️", 4,
            "Ao receber ataque fatal, sobrevive com 1 HP e ganha invencibilidade por 1 turno.")
        { }

        public List<ResultadoReacao> AoMorrer(ContextoReacao ctx)
        {
            var portador = ctx.Portador;

            if (!portador.PodeReviver)
                return new List<ResultadoReacao>();

            portador.Reviver(1);
            new Invencivel(turnos: 1).Aplicar(portador);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{portador.Personagem.Simbolo} {portador.Personagem.Nome} sobreviveu pela passiva! Invencível por 1 turno!")
            };
        }
    }
}