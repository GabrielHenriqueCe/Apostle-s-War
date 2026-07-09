using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Champs.LadoSombrio
{
    /// <summary>
    /// Detona o Veneno de cada alvo atingido (dano imediato + remove, via Veneno.Detonar —
    /// IStatusComTick) e cura o atacante pela MÉDIA dos percentuais detonados. Bespoke local —
    /// só a Putrefação (Zumbi) tem essa agregação cross-alvo hoje; é a 1ª AcaoSobreConjunto
    /// real do motor (ADR-composicao-de-acoes §3.4/§6). A MÉDIA em si (soma ÷ contador) não
    /// generaliza pra vocabulário — só Explodir/Detonar são compartilhados.
    /// </summary>
    class ExplodirVenenoECurarMedia : AcaoSobreConjunto
    {
        public ExplodirVenenoECurarMedia(Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : base(escopo, estadoAlvo) { }

        public override void Executar(Combate atacante, IReadOnlyList<Combate> conjunto, List<EventoDano> eventos)
        {
            double somaPercentual = 0;
            int comVeneno = 0;

            foreach (var alvo in conjunto)
            {
                var veneno = alvo.StatusAtivos.OfType<Veneno>().FirstOrDefault();
                if (veneno == null) continue;

                double percentual = veneno.PercentualDanoImediato;
                veneno.Detonar(alvo);
                somaPercentual += percentual;
                comVeneno++;
            }

            if (comVeneno > 0)
                atacante.Curar((int)(atacante.HPMaximo * (somaPercentual / comVeneno)));
        }
    }
}
