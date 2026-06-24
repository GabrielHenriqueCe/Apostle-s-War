namespace ApostlesWar
{
    abstract class HabilidadePassiva : Habilidade
    {
        public HabilidadePassiva(string nome, string simbolo, int turnos, string descricao = "")
            : base(nome, simbolo, turnos, descricao) { }

        protected List<EventoDano> SemDano() => new List<EventoDano>();

        // Passivas reativas não "ativam" pelo fluxo de habilidade — implementam
        // as interfaces IReageAo*. Este Ativar existe só pra satisfazer o contrato
        // abstrato de Habilidade; nunca é chamado. (Ver fio: separar hierarquia
        // Habilidade/Ativa/Passiva no roadmap.)
        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();

        protected T ObterEstado<T>(Combate combate) where T : new()
        {
            if (!combate.EstadoHabilidades.TryGetValue(this, out var estado) || estado is not T)
            {
                estado = new T();
                combate.EstadoHabilidades[this] = estado;
            }
            return (T)estado;
        }
    }
}