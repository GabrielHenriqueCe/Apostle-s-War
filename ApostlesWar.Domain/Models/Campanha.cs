namespace ApostlesWar.Domain
{
    /// <summary>
    /// Estrutura estática da campanha: as 7 fases, cada uma com a composição de inimigos por rodada,
    /// escrita por PAPEL. `ObterFase(numero)` é 1-based (a Fase 1 é o índice 0).
    /// A tabela é a do docs/GDD-progressao.md §A COMPOSIÇÃO DAS FASES.
    /// </summary>
    public static class Campanha
    {
        // Os apelidos do GDD, pra cada rodada caber numa linha e ser conferível contra o doc.
        private const TipoDeApostolo G = TipoDeApostolo.Guardiao;
        private const TipoDeApostolo C = TipoDeApostolo.Combatente;
        private const TipoDeApostolo A = TipoDeApostolo.Atirador;
        private const TipoDeApostolo S = TipoDeApostolo.Suporte;

        // DUAS INVARIANTES governam esta tabela, e as duas quebram calado se alguém editar uma linha
        // solta (o `CampanhaTests` cobre as duas):
        //  1. a rodada 1 de uma fase é a rodada 2 da anterior — cada formação é aquecimento antes de
        //     virar o tema da fase seguinte;
        //  2. G/C só nas casas 1-2, A/S só nas 3-4. A ordem da lista É a casa, então reordenar aqui
        //     põe apóstolo fora de posição.
        // A estreia é na rodada 2 (a fase 1 é a exceção: não há rodada anterior), a ordem é
        // G → C → A → S — a mesma do roster —, e o time inimigo fecha em quatro na fase 4.
        private static readonly List<Fase> fases = new()
        {
            new Fase(new() { G },          new() { G }),
            new Fase(new() { G },          new() { G, C }),
            new Fase(new() { G, C },       new() { G, C, A }),
            new Fase(new() { G, C, A },    new() { G, C, A, S }),
            new Fase(new() { G, C, A, S }, new() { C, C, A, S }),
            new Fase(new() { C, C, A, S }, new() { G, C, S, S }),
            new Fase(new() { G, C, S, S }, new() { G, C, A, S }),
        };

        public static Fase ObterFase(int numero) => fases[numero - 1];
    }
}
