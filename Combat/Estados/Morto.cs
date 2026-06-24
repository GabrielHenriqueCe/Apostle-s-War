namespace ApostlesWar
{
    /// <summary>
    /// Estado morto: ignora cura (morto não cura); revive transiciona de volta pra Vivo.
    /// Passo 2: passará a carregar os status de morto (bloquear-revive como debuff) e
    /// o revive checará esses status antes de permitir.
    /// </summary>
    class Morto : EstadoVida
    {
        public override bool EstaVivo() => false;

        public override void Curar(Combate dono, int valor)
        {
            // Morto não cura.
        }

        public override void Reviver(Combate dono, int hp)
        {
            dono.AplicarRevive(hp);
        }
    }
}