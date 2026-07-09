using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Skills.Ativas
{
    /// <summary>
    /// Revive todos os aliados mortos com 30% HP (respeita PodeReviver).
    /// Aplica CuraContinua 1t em todos os aliados vivos (incluindo recém-revividos).
    /// Funciona mesmo sem ninguém pra reviver.
    /// </summary>
    class Tecnology : HabilidadeAtiva
    {
        private const double HPRevivido = 0.30;
        private const int TurnosCuraContinua = 1;
        private const double PercentualCuraContinua = 0.10;

        public Tecnology() : base("Technology", "🤖", 4,
            "Revive aliados (30% HP) e aplica Cura Contínua em todo o time.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Ambos;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var lista = ObterListaPrincipal(ctx);

            foreach (Combate a in lista)
            {
                if (!a.EstaVivo())
                    a.Reviver((int)(a.HPMaximo * HPRevivido));
            }

            foreach (Combate a in lista.Where(c => c.EstaVivo()))
                new CuraContinua(turnos: TurnosCuraContinua, percentual: PercentualCuraContinua).Aplicar(a);

            return SemDano();
        }
    }
}