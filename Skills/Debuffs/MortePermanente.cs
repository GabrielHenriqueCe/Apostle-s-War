using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Debuffs
{
    /// <summary>
    /// Marcador aplicado em quem foi morto pela A3 do Invasor (Barata) ou outras habilidades equivalentes.
    /// Habilidades de revive consultam Combate.TemBloqueioRessurreicao() antes de reviver.
    /// </summary>
    class MortePermanente : Debuff
    {
        public MortePermanente() : base("Morte Permanente", "⚰️", int.MaxValue, 0,
            "Não pode ser ressuscitado.")
        { }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}