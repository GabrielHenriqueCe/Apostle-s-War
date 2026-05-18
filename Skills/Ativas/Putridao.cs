using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca todos com 100% ATK.
    /// Em cada inimigo com Veneno: causa dano imediato = stacks × 5% HP máx (ignora defesa) e remove o Veneno.
    /// Cura o Zumbi: (soma das % causadas) ÷ (qtd de inimigos com veneno), aplicada sobre HP máximo do Zumbi.
    /// </summary>
    class Putridao : HabilidadeAtiva
    {
        public Putridao() : base("Putrefação", "💀", 4,
            "Ataca todos. Explode Venenos causando dano imediato. Cura média.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            var resultados = new List<ResultadoAtaque>();
            double somaPercentual = 0;
            int comVeneno = 0;

            foreach (Combate a in ResolverAlvos(alvo, lista))
            {
                resultados.Add(AplicarDano(atacante, a, 1.0));

                var veneno = a.StatusAtivos.OfType<Veneno>().FirstOrDefault();
                if (veneno == null) continue;

                int danoVeneno = veneno.DanoTotalImediato(a);
                a.ReceberDanoDireto(danoVeneno);
                somaPercentual += veneno.PercentualDanoImediato;
                comVeneno++;

                veneno.Remover(a);
            }

            if (comVeneno > 0)
            {
                double percentualMedio = somaPercentual / comVeneno;
                atacante.Curar((int)(atacante.HPMaximo * percentualMedio));
            }

            return resultados;
        }
    }
}
