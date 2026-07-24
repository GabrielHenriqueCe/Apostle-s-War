namespace ApostlesWar.Domain
{
    /// <summary>
    /// Capacidade de PISO DE HP (irmã do IModificaDanoRecebido, mas atua no HP, não no dano):
    /// um status declara qual o HP mínimo que o portador pode ter. Consultada pelo `Combate.ReceberDano`
    /// DEPOIS que o dano cheio já foi aplicado — o dano recebido (e o `DanoEfetivo` retornado) continua
    /// integral (lifesteal, reflexo etc. usam o valor real); só o HP é clampado pra não cair do piso.
    /// Respeita o mesmo gate de `ignorados` do ReceberDano (um golpe pode furar o status pra "matar através").
    ///
    /// NÃO é prevenir-morte: o <see cref="IPrevineMorte"/> (GuardaReal) é reativo, uma vez, com cooldown,
    /// e roda no ConfirmarMorte. Este aqui é um piso CONTÍNUO enquanto o status estiver ativo.
    /// Implementador: Invencível. Futuro: itens/conjuntos com "HP não cai de X".
    /// </summary>
    public interface IDefineHPMinimo
    {
        /// <summary>O HP mínimo que o portador pode ter enquanto este status estiver ativo.</summary>
        int HPMinimo();
    }
}
