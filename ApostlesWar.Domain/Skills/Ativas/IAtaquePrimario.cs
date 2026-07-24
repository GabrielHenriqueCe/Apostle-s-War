namespace ApostlesWar.Domain
{
    /// <summary>
    /// Marca uma habilidade como o "ataque primário" (A1) do personagem.
    /// A1 tem cooldown 0 (sempre disponível) e é injetada automaticamente pelo
    /// PersonagemService SOMENTE se o personagem não declarar a sua própria.
    /// 
    /// Hoje todos usam AtaqueBasico genérico. No futuro, um personagem pode
    /// declarar sua própria A1 (ex: class Faisca : HabilidadeAtiva, IAtaquePrimario)
    /// e a injeção respeita, não duplica.
    /// </summary>
    public interface IAtaquePrimario { }
}