using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ApostlesWar.Services
{
    internal class CapitulosService
    {
        #region Capitulos

        List<Capitulo> capitulos = new List<Capitulo>
        {
            new Capitulo(Faccao.Reino, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, true),
            new Capitulo(Faccao.LadoSombrio, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
            new Capitulo(Faccao.Tecnologicos, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
            new Capitulo(Faccao.Folclore, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
            new Capitulo(Faccao.Misticos, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
            new Capitulo(Faccao.Especial, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
            new Capitulo(Faccao.Decaidos, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
            new Capitulo(Faccao.Apostolos, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
        };

        // Save "vivo": gravado na pasta Save/ ao lado do executável (AppContext.BaseDirectory),
        // não em caminho relativo — assim independe da working directory (VS x dotnet run) e
        // sempre bate com a semente Save/ que o csproj copia pra saída no build.
        private static readonly string CaminhoSave =
            Path.Combine(AppContext.BaseDirectory, "Save", "save.txt");

        Capitulo ObterCapitulo(Faccao faccao)
        {
            return capitulos.First(c => c.Faccao == faccao);
        }

        public bool EstaCapituloDesbloqueado(Faccao faccao) => ObterCapitulo(faccao).CapDesblock;

        public void DesbloquearFase(Faccao faccao, Fases fase)
        {
            Fases ultima = Enum.GetValues<Fases>().Last();
            if (fase == ultima) return;
            Capitulo cap = ObterCapitulo(faccao);
            cap.FaseDesblock[(int)fase] = true;
        }

        public void ConcluirFase(Faccao faccao, Fases fase)
        {
            Capitulo cap = ObterCapitulo(faccao);
            cap.FaseConcluida[(int)fase - 1] = true;
        }

        /// <summary>
        /// Desbloqueia a próxima facção se todas as fases do capítulo atual estiverem concluídas
        /// </summary>
        public void DesbloquearFaccao(Faccao faccao, Fases fase)
        {
            Capitulo cap = ObterCapitulo(faccao);
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
            Directory.CreateDirectory(Path.GetDirectoryName(CaminhoSave)!);
            File.WriteAllText(CaminhoSave, json);
        }

        public List<Capitulo> ObterTodos() => capitulos;

        public bool EstaDesbloqueado(Faccao faccao, Fases fase)
        {
            Capitulo cap = ObterCapitulo(faccao);
            return cap.CapDesblock && cap.FaseDesblock[(int)fase - 1];
        }

        /// <summary>
        /// Carrega o progresso salvo em arquivo, restaurando capítulos, campeões desbloqueados e itens obtidos
        /// </summary>
        public void CarregarProgresso()
        {
            if (!File.Exists(CaminhoSave)) return;

            try
            {
                var json = File.ReadAllText(CaminhoSave);
                var lista = JsonSerializer.Deserialize<List<Capitulo>>(json);

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
