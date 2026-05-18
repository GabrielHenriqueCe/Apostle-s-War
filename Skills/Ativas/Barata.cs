using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Intocavel 2t em si, ataca 1 inimigo com +100% ATK (2.0x).
    /// Se este golpe matou o inimigo, aplica MortePermanente — não pode ser revivido.
    /// </summary>
    class Barata : HabilidadeAtiva
    {
        public Barata() : base("Barata", "🪳", 3,
            "Intocável 2t, ataca com +100% ATK. Matou? Não pode reviver.")
        { }

        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            new Intocavel(turnos: 2).Aplicar(atacante);

            var resultados = new List<ResultadoAtaque>();
            foreach (Combate a in ResolverAlvos(alvo, lista))
            {
                bool vivoAntes = a.EstaVivo();
                var r = AplicarDano(atacante, a, 2.0);
                resultados.Add(r);

                if (vivoAntes && !a.EstaVivo())
                    new MortePermanente().Aplicar(a);
            }
            return resultados;
        }
    }
}