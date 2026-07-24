using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Skills.Passivas
{
    /// <summary>
    /// Marcador para passivas que ignoram determinados status no cálculo de dano
    /// permanentemente (ex: Drenagem ignora Invencível e BloqueioTotal).
    /// 
    /// O Combate.Atacar consulta IIgnoraStatusNoAtaque do atacante e adiciona
    /// os tipos listados ao parâmetro de ignorar no ReceberDano do alvo.
    /// </summary>
    public interface IIgnoraStatusNoAtaque
    {
        IEnumerable<Type> TiposIgnorados { get; }
    }
}