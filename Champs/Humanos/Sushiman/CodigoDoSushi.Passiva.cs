using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Champs.Humanos
{
    /// <summary>
    /// Ao receber um golpe CRÍTICO, todos os aliados ganham 15% de reflexo de dano
    /// por 2 turnos. Reage ao ser atacado (mesmo se o dano foi absorvido — o que
    /// importa é o crítico, que já vem correto do EventoDano: golpe sem dano não é
    /// crítico). Aliados = time do portador (vem do ContextoReacao).
    /// </summary>
    class CodigoDoSushi : HabilidadePassiva, IReageAoSerAtacado
    {
        public CodigoDoSushi() : base("Código do Sushi", "🥢", 0,
            "Ao receber crítico, todos os aliados ganham 15% de reflexo de dano por 2 turnos.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.FoiCritico) return new List<ResultadoReacao>();

            foreach (Combate aliado in ctx.Aliados.Where(a => a.EstaVivo()))
                new RefletirDano(duracao: 2, percentual: 0.15).Aplicar(aliado);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Portador.Personagem.Nome} ativou Código do Sushi! Aliados ganharam reflexo! 🥢")
            };
        }
    }
}