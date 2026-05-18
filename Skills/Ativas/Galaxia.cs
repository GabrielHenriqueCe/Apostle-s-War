using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// BuffDefesa 30% (2t) em todos os aliados. ProtecaoAliado 30% (2t) nos aliados exceto o Alien.
    /// </summary>
    class Galaxia : HabilidadeAtiva
    {
        public Galaxia() : base("Galáxia", "🌌", 4,
            "+30% DEF em todos. Outros aliados ficam protegidos pelo Alien.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            foreach (Combate a in ResolverAlvos(alvo, lista))
            {
                new BuffDefesa(turnos: 2, percentual: 0.30).Aplicar(a);
                if (a != atacante)
                    new ProtecaoAliado(atacante, turnos: 2, percentual: 0.30).Aplicar(a);
            }
            return SemDano();
        }
    }
}