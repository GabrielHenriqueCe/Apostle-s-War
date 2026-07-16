using ApostlesWar;

namespace ApostlesWar.Champs.Especial
{
    /// <summary>
    /// Contra-ataca com a A1 sempre que é atacado (1x por agressor por turno).
    /// Capacidade direta (IReageAoSerAtacado) — não usa mais o buff ContraAtaque via
    /// IPassivaInicial. Por ser passiva (não Buff), é imune a roubo (Copiando só leva Buffs).
    /// Mesma regra de frequência do buff ContraAtaque: ctx.Portador.TentarContraAtacar,
    /// então se o Herói também tiver o buff (ex: do Dragão), os dois não somam — o
    /// primeiro registra o agressor, o segundo vê que já contra-atacou.
    /// </summary>
    class Vigilante : HabilidadePassiva, IReageAoSerAtacado
    {
        public Vigilante() : base("Vigilante", "🦸", 0,
            "Contra-ataca sempre que é atacado.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Portador.EstaVivo()) return new List<ResultadoReacao>();
            if (!ctx.Contraparte.EstaVivo()) return new List<ResultadoReacao>();
            if (!ctx.Portador.TentarContraAtacar(ctx.Contraparte, 1.0)) return new List<ResultadoReacao>();

            var a1 = ctx.Portador.Personagem.Habilidades
                .OfType<IAtaquePrimario>()
                .OfType<IAtivavelComNatureza>()
                .First();

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Portador.Personagem.Nome} contra-ataca! ↩️",
                    Revide: new Revide(a1, ctx.Contraparte)
                )
            };
        }
    }
}
