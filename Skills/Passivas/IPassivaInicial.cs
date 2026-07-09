using ApostlesWar;

namespace ApostlesWar.Skills.Passivas
{
    /// <summary>
    /// Marca passivas que aplicam um efeito no setup do combate (não em resposta a evento).
    /// Ex: Espectral aplica Intocavel permanente no portador no início.
    /// </summary>
    interface IPassivaInicial
    {
        void AplicarInicial(Combate portador);
    }
}