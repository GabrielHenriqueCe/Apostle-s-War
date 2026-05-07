using System.Text.Json;

namespace ApostlesWar
{
    // Define as fases disponíveis e o progresso de desbloqueio de um capítulo
    class Capitulos
    {
        public Faccao Faccao { get; }
        public List<bool> FaseDesblock { get; private set; }
        public List<bool> FaseConcluida { get; private set; }
        public bool CapDesblock { get; private set; }

        public Capitulos(Faccao faccao, List<bool> faseDesblock, List<bool> faseConcluida, bool capDesblock)
        {
            Faccao = faccao;
            FaseDesblock = faseDesblock;
            FaseConcluida = faseConcluida;
            CapDesblock = capDesblock;
        }

        private static readonly List<Capitulos> capitulos = new List<Capitulos>
        {
            new Capitulos(Faccao.Reino, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, true),
            new Capitulos(Faccao.LadoSombrio, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
            new Capitulos(Faccao.Tecnologicos, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
            new Capitulos(Faccao.Folclore, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
            new Capitulos(Faccao.Misticos, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
            new Capitulos(Faccao.Especial, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
            new Capitulos(Faccao.Decaidos, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
            new Capitulos(Faccao.Apostolos, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
        };

        public static Capitulos ObterCapitulo(Faccao faccao)
        {
            return capitulos.First(c => c.Faccao == faccao);
        }

        public static bool EstaDesbloqueado(Faccao faccao) => ObterCapitulo(faccao).CapDesblock;

        public static void DesbloquearFase(Faccao faccao, Fases fase)
        {
            Fases ultima = Enum.GetValues<Fases>().Last();
            if (fase == ultima) return;
            Capitulos cap = ObterCapitulo(faccao);
            cap.FaseDesblock[(int)fase] = true;
        }

        public static void ConcluirFase(Faccao faccao, Fases fase)
        {
            Capitulos cap = ObterCapitulo(faccao);
            cap.FaseConcluida[(int)fase - 1] = true;
        }

        /// <summary>
        /// Desbloqueia a próxima facção se todas as fases do capítulo atual estiverem concluídas
        /// </summary>
        public static void DesbloquearFaccao(Faccao faccao, Fases fase)
        {
            Capitulos cap = ObterCapitulo(faccao);
            if (cap.FaseDesblock.All(f => f))
            {
                Faccao ultima = Enum.GetValues<Faccao>().Last();
                if (faccao == ultima) return;
                Faccao proxima = (Faccao)((int)faccao + 1);
                ObterCapitulo(proxima).CapDesblock = true;
            }
        }

        public static void SalvarProgresso()
        {
            var json = JsonSerializer.Serialize(capitulos);
            File.WriteAllText("save.txt", json);
        }



        public static List<Capitulos> ObterTodos() => capitulos;
    }
}