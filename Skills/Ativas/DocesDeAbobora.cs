using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Revive 1 aliado morto com HP cheio (primeiro elegível, respeita PodeReviver).
    /// Aplica RefletirDano 2t (15%) em todos os aliados vivos.
    /// </summary>
    class DocesDeAbobora : HabilidadeAtiva
    {
        private const int TurnosRefletirDano = 2;
        private const double PercentualRefletirDano = 0.15;

        public DocesDeAbobora() : base("Doces de Abóbora", "🍭", 4,
            "Revive 1 aliado e aplica Reflexo de dano em todos os aliados.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var lista = ObterListaPrincipal(ctx);

            var morto = lista.FirstOrDefault(a => !a.EstaVivo() && a.PodeReviver);
            if (morto != null)
                morto.Reviver(morto.HPMaximo);

            foreach (Combate a in lista.Where(a => a.EstaVivo()))
                new RefletirDano(turnos: TurnosRefletirDano, percentual: PercentualRefletirDano).Aplicar(a);

            return SemDano();
        }
    }
}