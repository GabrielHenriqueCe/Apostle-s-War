namespace ApostlesWar.Domain
{
    /// <summary>
    /// Estrutura estática da campanha: as 7 fases, cada uma com a composição de Slots dos inimigos
    /// por rodada. Era o `CampanhaService` (tabela pura); virou dado. `ObterFase(numero)` é 1-based
    /// (a Fase 1 é o índice 0).
    /// </summary>
    public static class Campanha
    {
        private static readonly List<Fase> fases = new()
        {
            new Fase(new List<Slot> { Slot.Slot1 }, new List<Slot> { Slot.Slot2 }),
            new Fase(new List<Slot> { Slot.Slot1, Slot.Slot1 }, new List<Slot> { Slot.Slot1, Slot.Slot2 }),
            new Fase(new List<Slot> { Slot.Slot2, Slot.Slot2 }, new List<Slot> { Slot.Slot2, Slot.Slot3 }),
            new Fase(new List<Slot> { Slot.Slot3, Slot.Slot1 }, new List<Slot> { Slot.Slot2, Slot.Slot3, Slot.Slot3 }),
            new Fase(new List<Slot> { Slot.Slot2, Slot.Slot3, Slot.Slot2 }, new List<Slot> { Slot.Slot3, Slot.Slot1, Slot.Slot3 }),
            new Fase(new List<Slot> { Slot.Slot1, Slot.Slot3, Slot.Slot1 }, new List<Slot> { Slot.Slot1, Slot.Slot3, Slot.Slot3, Slot.Slot1 }),
            new Fase(new List<Slot> { Slot.Slot2, Slot.Slot3, Slot.Slot2 }, new List<Slot> { Slot.Slot1, Slot.Slot3, Slot.Slot4, Slot.Slot2 }),
        };

        public static Fase ObterFase(int numero) => fases[numero - 1];
    }
}
