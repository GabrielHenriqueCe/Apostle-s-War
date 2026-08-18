namespace ApostlesWar.Domain
{
    /// <summary>
    /// Capacidade de INTERVENÇÃO na morte (irmã do IModificaDanoRecebido, que intercepta o dano):
    /// consultada pelo `Combate.ConfirmarMorte` no instante em que o HP chega a 0. Se o portador tem
    /// uma dessas (fora de cooldown), ela EVITA a morte — o portador segue Vivo, com os status
    /// intactos (não vira Morto). É diferente de reviver: aqui o personagem NÃO morreu.
    /// Implementador: GuardaReal (passiva do Guarda). Futuro: itens/conjuntos que previnem morte.
    /// </summary>
    public interface IPrevineMorte
    {
        /// <summary>Evita a morte do portador: restaura HP e aplica os efeitos de sobreviver.</summary>
        void Prevenir(Combate combatente);
    }
}
