using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    class Tiroteio : HabilidadeAtiva
    {
        public Tiroteio() : base("Tiroteio", "🔫", 4, "Ataca 2 inimigos aleatórios com 75% ATK.") { }
        public override int NumeroDeAlvos => 2;

        public override void Ativar(Combate alvo, List<Combate>? aliados = null) { }

        /// <summary>
        /// Retorna lista de (alvo, resultado) para exibição de dano individual.
        /// </summary>
        public List<(Combate Alvo, ResultadoAtaque Resultado)> AtivarComAtacante(Combate atacante, List<Combate> inimigos)
        {
            var resultados = new List<(Combate, ResultadoAtaque)>();

            var alvos = inimigos
                .Where(i => i.EstaVivo())
                .OrderBy(_ => Guid.NewGuid())
                .Take(NumeroDeAlvos);

            foreach (Combate alvo in alvos)
            {
                var resultado = atacante.AtacarComMultiplicador(alvo, 0.75);
                resultados.Add((alvo, resultado));
            }

            return resultados;
        }
    }
}
