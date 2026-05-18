using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Aplica ProtecaoAliado (30%) em todos os aliados exceto a Caveira por 2 turnos,
    /// depois aplica Escudo (30% HP máximo) na Caveira por 2 turnos.
    /// </summary>
    class OssoDuroDeRoer : HabilidadeAtiva
    {
        public OssoDuroDeRoer() : base("Osso Duro de Roer", "🦴", 3,
            "Protege aliados (30% do dano vai pra Caveira) e ganha Escudo de 30% HP.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            foreach (Combate a in ResolverAlvos(alvo, lista))
            {
                if (a == atacante) continue;
                new ProtecaoAliado(atacante, turnos: 2, percentual: 0.30).Aplicar(a);
            }

            int pontos = (int)(atacante.HPMaximo * 0.30);
            new Escudo(pontos, turnos: 2).Aplicar(atacante);

            return SemDano();
        }
    }
}
