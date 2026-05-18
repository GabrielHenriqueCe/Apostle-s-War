using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca todos os inimigos. Multiplicador escala de 1.0 (HP cheio) até 2.0 (1 HP).
    /// Mult = 1.0 + (1.0 * % HP perdido).
    /// </summary>
    class Ossinho : HabilidadeAtiva
    {
        public Ossinho() : base("Ossinho", "🦴", 3,
            "Ataca todos os inimigos. Dano aumenta conforme o HP da Caveira diminui (até 2x).")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            double percentualPerdido = 1.0 - ((double)atacante.HPAtual / atacante.HPMaximo);
            double mult = 1.0 + percentualPerdido;

            var resultados = new List<ResultadoAtaque>();
            foreach (Combate a in ResolverAlvos(alvo, lista))
                resultados.Add(AplicarDano(atacante, a, mult));
            return resultados;
        }
    }
}
