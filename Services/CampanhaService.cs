using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApostlesWar.Services
{
    internal class CampanhaService
    {
        #region Campanha

        List<Fase> fases = new List<Fase>
        {
            new Fase(new List<Slot> { Slot.Slot1 }, new List<Slot> { Slot.Slot2 }),
            new Fase(new List<Slot> { Slot.Slot1, Slot.Slot1 }, new List<Slot> { Slot.Slot1, Slot.Slot2 }),
            new Fase(new List<Slot> { Slot.Slot2, Slot.Slot2 }, new List<Slot> { Slot.Slot2, Slot.Slot3 }),
            new Fase(new List<Slot> { Slot.Slot3, Slot.Slot1 }, new List<Slot> { Slot.Slot2, Slot.Slot3, Slot.Slot3 }),
            new Fase(new List<Slot> { Slot.Slot2, Slot.Slot3, Slot.Slot2 }, new List<Slot> { Slot.Slot3, Slot.Slot1, Slot.Slot3 }),
            new Fase(new List<Slot> { Slot.Slot1, Slot.Slot3, Slot.Slot1 }, new List<Slot> { Slot.Slot1, Slot.Slot3, Slot.Slot3, Slot.Slot1 }),
            new Fase(new List<Slot> { Slot.Slot2, Slot.Slot3, Slot.Slot2 }, new List<Slot> { Slot.Slot1, Slot.Slot3, Slot.Slot4, Slot.Slot2 }),
        };

        public Fase ObterFase(int numero)
        {
            return fases[numero - 1];
        }

        #endregion
    }
}
