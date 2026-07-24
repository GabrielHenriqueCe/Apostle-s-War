using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Skills.Passivas
{
    /// <summary>
    /// Marca passivas que aplicam um efeito no setup do combate (não em resposta a evento).
    /// Ex: Espectral aplica Intocavel permanente no portador no início.
    /// </summary>
    public interface IPassivaInicial
    {
        void AplicarInicial(Combate portador);
    }
}