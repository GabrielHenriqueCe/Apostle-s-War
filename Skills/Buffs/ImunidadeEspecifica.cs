using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Buff permanente que bloqueia a aplicação de tipos específicos de StatusEffect.
    /// Diferente de ImunidadeDebuffs (que bloqueia todos os Debuffs), este bloqueia
    /// apenas os tipos passados no construtor.
    /// 
    /// Usado pela PassivaDragao (Veneno + Queima) e por futuras passivas iniciais.
    /// </summary>
    class ImunidadeEspecifica : Buff, IBloqueiaStatus
    {
        private readonly HashSet<Type> _tipos;

        public ImunidadeEspecifica(params Type[] tipos)
            : base("Imunidade", "🛡️", int.MaxValue, 0,
                "Imune a tipos específicos de status.")
        {
            _tipos = tipos.ToHashSet();
        }

        public bool Bloqueia(StatusEffect novo) => _tipos.Contains(novo.GetType());

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}