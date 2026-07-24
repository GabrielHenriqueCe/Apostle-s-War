namespace ApostlesWar.Application
{
    /// <summary>
    /// A IDENTIDADE do jogador — não é combatente (isso é o Domain.Jogador), é a "conta": o nome que
    /// as falas do jogo usam e o avatar (hoje o emoji de um campeão, PLACEHOLDER até ter picker/
    /// desbloqueio). Persistido pela porta <see cref="Portas.IRepositorioDeSave"/> na chave "perfil".
    /// Mora na Application (metadado de jogador, não regra de combate → fora do Domain).
    /// </summary>
    public record Perfil(string Nome, string Avatar);
}
