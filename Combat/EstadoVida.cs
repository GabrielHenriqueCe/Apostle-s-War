namespace ApostlesWar
{
    /// <summary>
    /// Estado de vida de um combatente (Vivo ou Morto). State Pattern: o Combate
    /// tem um EstadoVida interno e delega a ele o COMPORTAMENTO que muda conforme
    /// vivo/morto (curar, reviver, e — no Passo 2 — os status de morto). A leitura
    /// "está vivo?" é respondida pelo estado, mantida em sincronia com o HP pela
    /// transição no ReceberDano (invariante: HP <= 0 ⟺ Morto).
    ///
    /// Passo 1: só Vivo/Morto magros (EstaVivo, Curar, Reviver). O Morto ainda não
    /// carrega status de morto — isso entra no Passo 2 (bloquear-revive como debuff).
    /// </summary>
    abstract class EstadoVida
    {
        public abstract bool EstaVivo();

        /// <summary>Cura o portador. Só o Vivo cura; o Morto ignora.</summary>
        public abstract void Curar(Combate dono, int valor);

        /// <summary>Revive o portador. Só o Morto revive (vira Vivo); o Vivo ignora.</summary>
        public abstract void Reviver(Combate dono, int hp);
    }
}