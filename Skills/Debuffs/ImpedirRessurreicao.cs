using ApostlesWar;

namespace ApostlesWar.Skills.Debuffs
{
    /// <summary>
    /// A "Sentença": marca um morto como impossível de ressuscitar. Debuff permanente
    /// (marcador, sem efeito recorrente). Vive na lista do MORTO (StatusNoMorto) — os
    /// cleanses genéricos de vivo nunca o alcançam (filtram EstaVivo()). O revive checa
    /// este debuff (Morto.Reviver) e recusa se ele estiver presente.
    ///
    /// Aplicado por: Sentenca (ao matar) e Barata/Glitch (ao matar com a ativa).
    /// Removido por: AnjoCaido (Diabo) — cleanse específico que reabilita o revive.
    ///
    /// NOTA: por ora é Debuff. Se a mecânica de Alma fizer cleanses rodarem em mortos,
    /// reavaliar se vira tipo próprio. Ver "Vida de Alma" no roadmap.
    /// </summary>
    class ImpedirRessurreicao : Debuff
    {
        public ImpedirRessurreicao()
            : base("Sentença", "⚰️", int.MaxValue, 0, "Não pode ser ressuscitado.")
        { }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}