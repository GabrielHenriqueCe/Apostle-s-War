namespace ApostlesWar.Domain
{
    /// <summary>Fases disponíveis e progresso de desbloqueio de um capítulo.</summary>
    public class Capitulo
    {
        public Faccao Faccao { get; }
        public List<bool> FaseDesblock { get; private set; }
        public List<bool> FaseConcluida { get; private set; }
        public bool CapDesblock { get; set; }

        public Capitulo(Faccao faccao, List<bool> faseDesblock, List<bool> faseConcluida, bool capDesblock)
        {
            Faccao = faccao;
            FaseDesblock = faseDesblock;
            FaseConcluida = faseConcluida;
            CapDesblock = capDesblock;
        }
    }
}