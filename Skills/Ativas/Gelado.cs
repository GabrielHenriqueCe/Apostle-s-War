using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Aplica Escudo de 30% do HP máximo em todos os aliados (2t)
    /// e ataca todos os inimigos com +75% ATK.
    /// </summary>
    class Gelado : HabilidadeAtiva
    {
        private const double EscudoPercentual = 0.30;
        private const double MultiplicadorAtaque = 1.75;

        public Gelado() : base("Gelado", "❄️", 4,
            "Escudo 30% HP nos aliados (2t) e ataca todos +75% ATK.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override TipoAtaque TipoAtaque => TipoAtaque.AreaDeEfeito;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            // Escudo nos aliados vivos
            foreach (Combate a in ctx.Aliados.Where(c => c.EstaVivo()))
            {
                int pontos = (int)(a.HPMaximo * EscudoPercentual);
                new Escudo(pontos, turnos: 2).Aplicar(a);
            }

            // Ataca todos inimigos
            var resultados = new List<ResultadoAtaque>();
            foreach (Combate i in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                resultados.Add(AplicarDano(ctx.Atacante, i, MultiplicadorAtaque));

            return resultados;
        }
    }
}