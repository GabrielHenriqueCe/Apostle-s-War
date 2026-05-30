using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// A cada hit recebido (ataque direto), o Diabo ganha +5% de HP máximo permanente,
    /// até o cap de 25% acumulado (5 hits). HPAtual NÃO é alterado — só o máximo aumenta.
    /// 
    /// Calculado sobre HPMaximoInicial (HP cheio da fase), consistente com Queima/Maldição.
    /// Só dispara em dano de ataque direto (não Veneno/Queima).
    /// </summary>
    class PassivaDiabo : HabilidadePassiva
    {
        private const double GanhoPorHit = 0.05;
        private const double Cap = 0.25;

        private class Estado
        {
            public double TotalAumentado;
        }

        public PassivaDiabo() : base("Cresce com Dor", "😈", 0,
            "Ao ser atacado, +5% HP máximo (até 25%).")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeSerAtacado && ctx.AlvoVivo;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var estado = ObterEstado<Estado>(ctx.Atacante);
            if (estado.TotalAumentado >= Cap) return SemDano();

            double aumentar = Math.Min(GanhoPorHit, Cap - estado.TotalAumentado);
            int delta = (int)(ctx.Atacante.HPMaximoInicial * aumentar);

            ctx.Atacante.ModificarHPMaximo(delta);
            estado.TotalAumentado += aumentar;

            return SemDano();
        }
    }
}