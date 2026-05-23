using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Marcador para passivas que ignoram determinados status no cálculo de dano
    /// permanentemente (ex: PassivaVampiro ignora Invencível e BloqueioTotal).
    /// 
    /// O Combate.Atacar consulta IIgnoraStatusNoAtaque do atacante e adiciona
    /// os tipos listados ao parâmetro de ignorar no ReceberDano do alvo.
    /// </summary>
    interface IIgnoraStatusNoAtaque
    {
        IEnumerable<Type> TiposIgnorados { get; }
    }
}