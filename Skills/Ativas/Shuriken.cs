using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    class Shuriken : HabilidadeAtiva
    {
        public Shuriken() : base("Shuriken", "🌟", 3,
            "Ataca 1 inimigo 2x +50% ATK. Se o 1º hit for crítico, o 2º ignora 25% da DEF.")
        { }
        public override int NumeroDeAlvos => 2;
        public override TipoAlvo TipoAlvo => TipoAlvo.Aleatorio;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            var resultados = new List<ResultadoAtaque>();
            var alvos = ResolverAlvos(alvo, lista);

            var hit1 = AplicarDano(atacante, alvos[0], 1.5);
            resultados.Add(hit1);

            Combate alvo2 = alvos.Count > 1 ? alvos[1] : alvos[0];
            double ignorar = hit1.Critico ? 0.25 : 0.0;
            resultados.Add(atacante.AtacarComMultiplicador(alvo2, 1.5, ignorarDefesaPct: ignorar));

            return resultados;
        }
    }
}