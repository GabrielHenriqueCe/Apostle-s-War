namespace ApostlesWar.Domain
{
    #region Inimigo

    /// <summary>
    /// Um combatente do lado da campanha. Ele não tem nada de especial na ficha: o que o faz duro é o
    /// NÍVEL em que entrou (<see cref="Progressao.NivelDoInimigo"/>), e quem o põe nesse nível é quem
    /// monta a rodada — aqui já chega o personagem pronto.
    ///
    /// Antes ele nascia com um <c>MultiplicadorFase</c> (HP/ATK/DEF × capítulo e fase) que existia
    /// justamente porque não havia nível. As camadas `MultiplicadorAtaque`/`MultiplicadorDefesa` do
    /// <see cref="Combate"/> continuam lá, agora só como seam de item e buff.
    /// </summary>
    public class Inimigo : Combate
    {
        public override Personagem Personagem { get; }

        public Inimigo(Personagem personagem) : base(personagem)
        {
            Personagem = personagem;
        }
    }

    #endregion
}
