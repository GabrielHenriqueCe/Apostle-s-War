namespace ApostlesWar
{
#region Campanha

    /// <summary>
    /// Representa a campanha\\
    /// </summary>
    class Campanha
    {
        private static readonly List<Fase> fases = new List<Fase>
        {
            new Fase(new List<Slot> { Slot.Slot1 }, new List<Slot> { Slot.Slot2 }),
            new Fase(new List<Slot> { Slot.Slot1, Slot.Slot1 }, new List<Slot> { Slot.Slot1, Slot.Slot2 }),
            new Fase(new List<Slot> { Slot.Slot2, Slot.Slot2 }, new List<Slot> { Slot.Slot2, Slot.Slot3 }),
            new Fase(new List<Slot> { Slot.Slot3, Slot.Slot1 }, new List<Slot> { Slot.Slot2, Slot.Slot3, Slot.Slot3 }),
            new Fase(new List<Slot> { Slot.Slot2, Slot.Slot3, Slot.Slot2 }, new List<Slot> { Slot.Slot3, Slot.Slot1, Slot.Slot3 }),
            new Fase(new List<Slot> { Slot.Slot1, Slot.Slot3, Slot.Slot1 }, new List<Slot> { Slot.Slot1, Slot.Slot3, Slot.Slot3, Slot.Slot1 }),
            new Fase(new List<Slot> { Slot.Slot2, Slot.Slot3, Slot.Slot2 }, new List<Slot> { Slot.Slot1, Slot.Slot3, Slot.Slot4, Slot.Slot2 }),
        };

        public static Fase ObterFase(int numero)
        {
            return fases[numero - 1];
        }
    }

#endregion
}
