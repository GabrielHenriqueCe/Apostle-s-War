using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca todos os inimigos com +100% ATK, aplica Irritar 1t em todos
    /// e ganha Escudo de 30% do dano total causado.
    /// </summary>
    class Quebrar : HabilidadeAtiva
    {
        private const double MultiplicadorAtaque = 2.0;
        private const double EscudoPercentualDano = 0.30;

        public Quebrar() : base("Quebrar", "💥", 3,
            "Ataca todos +100% ATK, Irritar 1t e ganha Escudo de 30% do dano.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override TipoAtaque TipoAtaque => TipoAtaque.AreaDeEfeito;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<EventoDano>();
            int danoTotal = 0;

            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                var r = AplicarDano(ctx.Atacante, a, MultiplicadorAtaque);
                resultados.Add(r);
                danoTotal += r.DanoEfetivo;

                if (a.EstaVivo())
                    new Irritar(ctx.Atacante, turnos: 1).Aplicar(a);
            }

            int pontosEscudo = (int)(danoTotal * EscudoPercentualDano);
            if (pontosEscudo > 0)
                new Escudo(pontosEscudo, turnos: 2).Aplicar(ctx.Atacante);

            return resultados;
        }
    }
}