using ApostlesWar;
using v1_Apostle_s_War.Skills.Ativas;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// 10% de chance de contra-atacar com Marretada ao ser atacado. Reage via
    /// IReageAoSerAtacado (dispara mesmo se o dano foi absorvido — reage ao ATO
    /// de ser atacado, não ao dano). Declara o revide (Revide: Marretada +
    /// Contraparte); o CombateService executa via IAtivavelComNatureza e exibe —
    /// mesmo fluxo do ContraAtaque, só troca A1 por Marretada.
    /// </summary>
    class PassivaOperario : HabilidadePassiva, IReageAoSerAtacado
    {
        private static readonly Random _random = new Random();

        public PassivaOperario() : base("Instinto do Operário", "🛠️", 0,
            "10% de chance de contra-atacar com Marretada ao ser atacado.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (_random.NextDouble() >= 0.10) return new List<ResultadoReacao>();
            if (!ctx.Contraparte.EstaVivo()) return new List<ResultadoReacao>();

            var marretada = ctx.Portador.Personagem.Habilidades.OfType<Marretada>().First();

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Portador.Personagem.Simbolo} {ctx.Portador.Personagem.Nome} contra-atacou com Marretada! 🛠️",
                    Revide: new Revide(marretada, ctx.Contraparte))
            };
        }
    }
}