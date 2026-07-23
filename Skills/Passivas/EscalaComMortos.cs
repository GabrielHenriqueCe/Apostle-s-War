using ApostlesWar;
using ApostlesWar.Skills;

namespace ApostlesWar.Skills.Passivas
{
    /// <summary>Quais mortos uma EscalaComMortos conta: só o próprio time, só o inimigo, ou os dois.</summary>
    enum EscopoMortos { ProprioTime, TimeInimigo, AmbosOsTimes }

    /// <summary>
    /// Passiva GENÉRICA (config-driven): o portador ganha um buff proporcional a quantos combatentes
    /// estão MORTOS no tabuleiro (escopo configurável). Molde da Ventania (Tengu) — IReageAoInicioTurno
    /// que RENOVA um buff todo turno — mas com valor DINÂMICO (nº de mortos × porMorto) em vez de fixo.
    /// Conta o tabuleiro via ContextoCombate (Aliados/Inimigos): não é estado de morto, é leitura do
    /// campo (depende da Fatia 2 do EventoDano). A fábrica de buff generaliza o STAT (BuffAtaque/
    /// BuffDefesa/… da matriz de stats).
    ///
    /// Cliente: Zumbi ("Horda" — ambos os times → ATK). Os escopos só-aliados / só-inimigos nascem
    /// prontos pra passivas futuras. IRMÃ (design pronto, não construída): EscalaComAbates, que reage
    /// ao EVENTO de matar (IReageAoMatar) e dá bônus PERMANENTE (molde da Ambição), não buff.
    /// </summary>
    class EscalaComMortos : HabilidadePassiva, IReageAoInicioTurno
    {
        private readonly EscopoMortos _escopo;
        private readonly double _porMorto;
        private readonly Func<double, Buff> _buff;

        public EscalaComMortos(string nome, string simbolo, string descricao,
            EscopoMortos escopo, double porMorto, Func<double, Buff> buff)
            : base(nome, simbolo, 0, descricao)
        {
            _escopo = escopo;
            _porMorto = porMorto;
            _buff = buff;
        }

        public List<ResultadoReacao> AoInicioTurno(ContextoCombate ctx)
        {
            int mortos = ContarMortos(ctx);
            if (mortos > 0)
                _buff(_porMorto * mortos).Aplicar(ctx.Atacante);   // renova todo turno, como a Ventania
            return new List<ResultadoReacao>();
        }

        private int ContarMortos(ContextoCombate ctx) => _escopo switch
        {
            EscopoMortos.ProprioTime => ctx.Aliados.Count(c => !c.EstaVivo()),
            EscopoMortos.TimeInimigo => ctx.Inimigos.Count(c => !c.EstaVivo()),
            _ => ctx.Aliados.Count(c => !c.EstaVivo()) + ctx.Inimigos.Count(c => !c.EstaVivo()),
        };
    }
}
