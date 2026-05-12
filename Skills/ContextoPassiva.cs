namespace ApostlesWar
{
    /// <summary>
    /// Contexto passado para DeveAtivar das passivas.
    /// Cada passiva decide sozinha se deve disparar com base nesse contexto.
    /// </summary>
    record ContextoPassiva(
        bool AlvoVivo,
        bool FoiCritico,
        List<Combate> Aliados,
        Combate Atacante
    );
}
