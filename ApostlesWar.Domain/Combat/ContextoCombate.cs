namespace ApostlesWar
{
    /// <summary>
    /// Contexto passado a toda habilidade ao ativar.
    /// Sempre da perspectiva do atacante:
    /// - Atacante: quem está usando a habilidade
    /// - Aliados: time do atacante (inclui o próprio atacante)
    /// - Inimigos: time oposto
    /// 
    /// Permite habilidades que precisam interagir com os dois times
    /// (revive aliados + ataca inimigos, etc).
    /// </summary>
    record ContextoCombate(
        Combate Atacante,
        List<Combate> Aliados,
        List<Combate> Inimigos
    );
}