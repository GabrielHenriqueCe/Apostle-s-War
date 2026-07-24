namespace ApostlesWar
{
    /// <summary>
    /// Estado vivo: carrega os status do vivo (buffs, debuffs, escudos...).
    /// Cura normalmente; ignora revive (já está vivo).
    /// </summary>
    class Vivo : EstadoVida
    {
        public List<StatusEffect> StatusNoVivo { get; } = new List<StatusEffect>();

        public override List<StatusEffect> Status => StatusNoVivo;

        public override bool EstaVivo() => true;

        public override int Curar(Combate dono, int valor) =>
            dono.AplicarCura(valor);

        public override void Reviver(Combate dono, int hp)
        {
            // Já está vivo — reviver não faz nada.
        }
    }
}