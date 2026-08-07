using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ApostlesWar.Application.Services
{
    public class CapitulosService
    {
        #region Capitulos

        // O slot de save deste service. Const porque agora TRÊS operações o citam (salvar, carregar
        // e o wipe do Resetar) — e um wipe que erra a string apaga nada, calado.
        private const string ChaveProgresso = "save";

        /// <summary>
        /// O jogo NOVO: só o Reino aberto, e nele só a fase 1. É fábrica (e não um campo inicializado
        /// na declaração) porque o <see cref="Resetar"/> precisa de uma cópia FRESCA — os
        /// <see cref="Capitulo"/> são mutáveis, então reusar as mesmas instâncias devolveria o
        /// progresso junto.
        /// </summary>
        private static List<Capitulo> EstadoInicial() => new List<Capitulo>
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
            new Capitulo(Faccao.Ascendentes, new List<bool> { true, false, false, false, false, false, false },
            new List<bool> { false, false, false, false, false, false, false }, false),
        };

        List<Capitulo> capitulos = EstadoInicial();

        private readonly IRepositorioDeSave _repo;

        public CapitulosService(IRepositorioDeSave repo) => _repo = repo;

        /// <summary>
        /// Devolve o progresso ao estado de jogo novo — disco E memória, nesta ordem de importância:
        /// só apagar o arquivo NÃO reseta nada, porque o <see cref="CarregarProgresso"/> mantém o que
        /// já está em memória quando a porta devolve null. Era esse o buraco do "excluir conta": o
        /// wipe limpava o disco, o jogador criava um perfil novo e seguia com os 36 champs liberados,
        /// que voltavam pro disco na primeira fase vencida.
        /// </summary>
        public void Resetar()
        {
            _repo.Excluir(ChaveProgresso);
            capitulos = EstadoInicial();
        }

        Capitulo ObterCapitulo(Faccao faccao)
        {
            return capitulos.First(c => c.Faccao == faccao);
        }

        /// <summary>
        /// As facções que SÃO capítulo, na ordem em que a campanha as percorre — o mapa é isto,
        /// desenhado. Note que os Humanos não estão: são o time inicial, não um capítulo.
        ///
        /// Deriva da lista de capítulos em vez de reafirmar a regra, porque a lista JÁ É a resposta.
        /// O front tinha a própria versão (`Enum.GetValues&lt;Faccao&gt;().Where(f => f != Humanos)`),
        /// que acertava por coincidência: bastava um capítulo entrar fora da ordem do enum, ou uma
        /// facção existir sem ser capítulo, pra o mapa mentir.
        /// </summary>
        public List<Faccao> FaccoesDaCampanha() => capitulos.Select(c => c.Faccao).ToList();

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

        public void SalvarProgresso() => _repo.Salvar(ChaveProgresso, capitulos);

        public List<Capitulo> ObterTodos() => capitulos;

        public bool EstaDesbloqueado(Faccao faccao, Fases fase)
        {
            Capitulo cap = ObterCapitulo(faccao);
            return cap.CapDesblock && cap.FaseDesblock[(int)fase - 1];
        }

        /// <summary>
        /// Carrega o progresso salvo, restaurando os capítulos. Save ausente/corrompido → mantém o
        /// progresso inicial (default em memória): a porta devolve null e o jogo abre do zero.
        /// </summary>
        public void CarregarProgresso()
        {
            var lista = _repo.Carregar<List<Capitulo>>(ChaveProgresso);
            if (lista != null)
            {
                capitulos.Clear();
                capitulos.AddRange(lista);
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
