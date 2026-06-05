using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace v1_Apostle_s_War.Services
{
    internal class CapitulosService
    {
        #region Capitulos

        List<Capitulos> capitulos = new List<Capitulos>
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

        Capitulos ObterCapitulo(Faccao faccao)
        {
            return capitulos.First(c => c.Faccao == faccao);
        }

        public bool EstaCapituloDesbloqueado(Faccao faccao) => ObterCapitulo(faccao).CapDesblock;

        public void DesbloquearFase(Faccao faccao, Fases fase)
        {
            Fases ultima = Enum.GetValues<Fases>().Last();
            if (fase == ultima) return;
            Capitulos cap = ObterCapitulo(faccao);
            cap.FaseDesblock[(int)fase] = true;
        }

        public void ConcluirFase(Faccao faccao, Fases fase)
        {
            Capitulos cap = ObterCapitulo(faccao);
            cap.FaseConcluida[(int)fase - 1] = true;
        }

        /// <summary>
        /// Desbloqueia a próxima facção se todas as fases do capítulo atual estiverem concluídas
        /// </summary>
        public void DesbloquearFaccao(Faccao faccao, Fases fase)
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

        public void SalvarProgresso()
        {
            var json = JsonSerializer.Serialize(capitulos);
            File.WriteAllText("save.txt", json);
        }

        public List<Capitulos> ObterTodos() => capitulos;

        public bool EstaDesbloqueado(Faccao faccao, Fases fase)
        {
            Capitulos cap = ObterCapitulo(faccao);
            return cap.CapDesblock && cap.FaseDesblock[(int)fase - 1];
        }

        /// <summary>
        /// Carrega o progresso salvo em arquivo, restaurando capítulos, campeões desbloqueados e itens obtidos
        /// </summary>
        public void CarregarProgresso()
        {
            if (!File.Exists("save.txt")) return;

            try
            {
                var json = File.ReadAllText("save.txt");
                var lista = JsonSerializer.Deserialize<List<Capitulos>>(json);

                if (lista != null)
                {
                    capitulos.Clear();
                    capitulos.AddRange(lista);
                }
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                // Save corrompido ou ilegível: mantém o progresso inicial (default em memória)
                // em vez de crashar. O jogador recomeça do zero, mas o jogo abre.
                Console.WriteLine("⚠️ Não foi possível carregar o progresso salvo (save inválido). Iniciando do começo.");
                Thread.Sleep(2000);
            }
        }
        public bool CapituloConcluido(Faccao faccao)
        {
            return ObterCapitulo(faccao).FaseConcluida.All(f => f);
        }

        public bool FaseConcluida(Faccao faccao, Fases fase)
        {
            return ObterCapitulo(faccao).FaseConcluida[(int)fase - 1];
        }

        #endregion
    }
}
