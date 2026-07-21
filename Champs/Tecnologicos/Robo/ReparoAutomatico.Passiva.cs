using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Champs.Tecnologicos
{
    /// <summary>
    /// Ao atacar, aplica CuraContinua 1t no aliado vivo com menor HP atual (inclui o
    /// Robô). Reage 1x por ataque (IReageAoAtacar segue o TipoAtaque). Aliados = time
    /// do portador (vem do ContextoReacao).
    /// </summary>
    class ReparoAutomatico : HabilidadePassiva, IReageAoAtacar
    {
        public ReparoAutomatico() : base("Reparo Automático", "🔧", 0,
            "Ao atacar, aplica Cura Contínua no aliado com menor HP.")
        { }

        public List<ResultadoReacao> AoAtacar(ContextoReacao ctx)
        {
            var aliadoMenorHP = ctx.Aliados
                .Where(a => a.EstaVivo())
                .OrderBy(a => a.HPAtual)
                .FirstOrDefault();

            if (aliadoMenorHP == null) return new List<ResultadoReacao>();

            new CuraContinua(duracao: 1, percentual: 0.10).Aplicar(aliadoMenorHP);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Portador.Personagem.Simbolo} reparou {aliadoMenorHP.Personagem.Simbolo}! 🔧")
            };
        }
    }
}
