using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Início do turno: se tem Queima, remove (cleanse) e aplica CuraContinua 2t.
    /// Só age se havia Queima — "o calor da queima vira gelo derretendo e curando".
    /// </summary>
    class Derretendo : HabilidadePassiva, IReageAoInicioTurno
    {
        private const double CuraPercentual = 0.10;
        private const int TurnosCura = 2;

        public Derretendo() : base("Derretendo", "❄️", 0,
            "Início do turno: se tem Queima, remove e aplica Cura Contínua (2t).")
        { }

        public List<ResultadoReacao> AoInicioTurno(ContextoCombate ctx)
        {
            var queimas = ctx.Atacante.StatusAtivos.OfType<Queima>().ToList();
            if (queimas.Count == 0) return new List<ResultadoReacao>();

            foreach (var q in queimas)
                q.Remover(ctx.Atacante);

            new CuraContinua(turnos: TurnosCura, percentual: CuraPercentual).Aplicar(ctx.Atacante);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Atacante.Personagem.Simbolo} derreteu a queima e começou a se curar! ❄️")
            };
        }
    }
}