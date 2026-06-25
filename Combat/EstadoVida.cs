namespace ApostlesWar
{
    /// <summary>
    /// Estado de vida de um combatente (Vivo ou Morto). State Pattern: o Combate
    /// tem um EstadoVida interno e delega a ele o COMPORTAMENTO que muda conforme
    /// vivo/morto (curar, reviver, e a lista de status daquele estado).
    ///
    /// Cada estado carrega SUA PRÓPRIA lista de status (StatusNoVivo / StatusNoMorto).
    /// O Combate expõe a do estado atual via a view StatusAtivos. Ao morrer, os status
    /// do vivo somem (a lista do Vivo é descartada com ele); o Morto começa limpo.
    /// </summary>
    abstract class EstadoVida
    {
        /// <summary>A lista de status DESTE estado. O Combate expõe a do estado atual.</summary>
        public abstract List<StatusEffect> Status { get; }

        public abstract bool EstaVivo();
        public abstract void Curar(Combate dono, int valor);
        public abstract void Reviver(Combate dono, int hp);
    }
}