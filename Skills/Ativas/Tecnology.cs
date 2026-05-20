using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Revive todos os aliados mortos com 30% HP (respeita TemBloqueioRessurreicao).
    /// Aplica CuraContinua 1t em todos os aliados vivos (incluindo revividos).
    /// Funciona mesmo sem ninguém pra reviver.
    /// </summary>
    class Tecnology : HabilidadeAtiva
    {
        public Tecnology() : base("Technology", "🤖", 4,
            "Revive aliados (30% HP) e aplica Cura Contínua em todo o time.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var lista = ObterListaPrincipal(ctx);

            foreach (Combate a in lista)
            {
                if (!a.EstaVivo() && !a.TemBloqueioRessurreicao())
                    a.Reviver((int)(a.HPMaximo * 0.30));
            }

            foreach (Combate a in lista.Where(c => c.EstaVivo()))
                new CuraContinua(turnos: 1, percentual: 0.10).Aplicar(a);

            return SemDano();
        }
    }
}