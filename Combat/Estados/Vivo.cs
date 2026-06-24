namespace ApostlesWar
{
    /// <summary>
    /// Estado vivo: cura normalmente; ignora revive (já está vivo).
    /// </summary>
    class Vivo : EstadoVida
    {
        public override bool EstaVivo() => true;

        public override void Curar(Combate dono, int valor) =>
            dono.AplicarCura(valor);

        public override void Reviver(Combate dono, int hp)
        {
            // Já está vivo — reviver não faz nada.
        }
    }
}