using System.Text.Json;

namespace ApostlesWar
{
    // Define as fases disponíveis e o progresso de desbloqueio de um capítulo
    class Capitulos
    {
        public Faccao Faccao { get; }
        public List<bool> FaseDesblock { get; private set; }
        public List<bool> FaseConcluida { get; private set; }
        public bool CapDesblock { get; set; }

        public Capitulos(Faccao faccao, List<bool> faseDesblock, List<bool> faseConcluida, bool capDesblock)
        {
            Faccao = faccao;
            FaseDesblock = faseDesblock;
            FaseConcluida = faseConcluida;
            CapDesblock = capDesblock;
        }
    }
}