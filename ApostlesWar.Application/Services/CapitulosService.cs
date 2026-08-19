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
        /// O jogo NOVO de UMA dificuldade: só o Reino aberto, e nele só a fase 1. É fábrica (e não um
        /// campo inicializado na declaração) porque o <see cref="Resetar"/> e as quatro trilhas
        /// precisam de cópias FRESCAS — os <see cref="Capitulo"/> são mutáveis, então reusar as mesmas
        /// instâncias faria o Pesadelo compartilhar progresso com o Fácil.
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

        /// <summary>
        /// UMA TRILHA POR DIFICULDADE, todas começando do zero. A do Fácil é a única jogável no início;
        /// as outras existem desde já porque quem decide se dá pra entrar nelas é o
        /// <see cref="DificuldadeDesbloqueada"/>, não a ausência do dado.
        /// </summary>
        private static Dictionary<Dificuldade, List<Capitulo>> TrilhasIniciais()
            => Enum.GetValues<Dificuldade>().ToDictionary(d => d, _ => EstadoInicial());

        Dictionary<Dificuldade, List<Capitulo>> trilhas = TrilhasIniciais();

        private readonly IRepositorioDeSave _repo;

        public CapitulosService(IRepositorioDeSave repo) => _repo = repo;

        /// <summary>
        /// Devolve o progresso ao estado de jogo novo — disco E memória, nesta ordem de importância:
        /// só apagar o arquivo NÃO reseta nada, porque o <see cref="CarregarProgresso"/> mantém o que
        /// já está em memória quando a porta devolve null. Era esse o buraco do "excluir conta": o
        /// wipe limpava o disco, o jogador criava um perfil novo e seguia com os 36 apóstolos liberados,
        /// que voltavam pro disco na primeira fase vencida.
        /// </summary>
        public void Resetar()
        {
            _repo.Excluir(ChaveProgresso);
            trilhas = TrilhasIniciais();
        }

        Capitulo ObterCapitulo(Faccao faccao, Dificuldade dificuldade)
        {
            return trilhas[dificuldade].First(c => c.Faccao == faccao);
        }

        /// <summary>
        /// As facções que SÃO capítulo, na ordem em que a campanha as percorre — o mapa é isto,
        /// desenhado. Note que os Humanos não estão: são o time inicial, não um capítulo.
        ///
        /// Deriva da lista de capítulos em vez de reafirmar a regra, porque a lista JÁ É a resposta.
        /// O front tinha a própria versão (`Enum.GetValues&lt;Faccao&gt;().Where(f => f != Humanos)`),
        /// que acertava por coincidência: bastava um capítulo entrar fora da ordem do enum, ou uma
        /// facção existir sem ser capítulo, pra o mapa mentir.
        ///
        /// Não depende da dificuldade: as quatro trilhas percorrem os mesmos oito capítulos.
        /// </summary>
        public List<Faccao> FaccoesDaCampanha() => trilhas[Dificuldade.Facil].Select(c => c.Faccao).ToList();

        /// <summary>
        /// A dificuldade está aberta? O Fácil sempre; as outras quando a ÚLTIMA FASE DO ÚLTIMO CAPÍTULO
        /// da anterior foi concluída (a 8-7).
        ///
        /// DERIVADO, e não um `bool` guardado no save: um segundo lugar dizendo "o Normal está aberto"
        /// é um segundo lugar pra discordar do progresso que já está aqui. E ninguém perde acesso ao
        /// que abriu — a resposta só depende de uma fase concluída, e fase concluída não desconclui.
        /// </summary>
        public bool DificuldadeDesbloqueada(Dificuldade dificuldade)
        {
            if (dificuldade == Dificuldade.Facil) return true;

            var anterior = (Dificuldade)((int)dificuldade - 1);
            return FaseConcluida(FaccoesDaCampanha().Last(), Enum.GetValues<Fases>().Last(), anterior);
        }

        public bool EstaCapituloDesbloqueado(Faccao faccao, Dificuldade dificuldade)
            => ObterCapitulo(faccao, dificuldade).CapDesblock;

        public void DesbloquearFase(Faccao faccao, Fases fase, Dificuldade dificuldade)
        {
            Fases ultima = Enum.GetValues<Fases>().Last();
            if (fase == ultima) return;
            Capitulo cap = ObterCapitulo(faccao, dificuldade);
            cap.FaseDesblock[(int)fase] = true;
        }

        public void ConcluirFase(Faccao faccao, Fases fase, Dificuldade dificuldade)
        {
            Capitulo cap = ObterCapitulo(faccao, dificuldade);
            cap.FaseConcluida[(int)fase - 1] = true;
        }

        /// <summary>
        /// Desbloqueia a próxima facção se todas as fases do capítulo atual estiverem concluídas
        /// </summary>
        public void DesbloquearFaccao(Faccao faccao, Fases fase, Dificuldade dificuldade)
        {
            Capitulo cap = ObterCapitulo(faccao, dificuldade);
            if (cap.FaseDesblock.All(f => f))
            {
                Faccao ultima = Enum.GetValues<Faccao>().Last();
                if (faccao == ultima) return;
                Faccao proxima = (Faccao)((int)faccao + 1);
                ObterCapitulo(proxima, dificuldade).CapDesblock = true;
            }
        }

        public void SalvarProgresso() => _repo.Salvar(ChaveProgresso, trilhas);

        /// <summary>
        /// O que o jogador venceu, nas quatro trilhas — com a dificuldade junto, porque a composição
        /// da fase depende dela. É a pergunta que o arsenal e os apóstolos fazem ao carregar: item e
        /// apóstolo são conquistas GLOBAIS (vencer a Reino 1-1 no Pesadelo não entrega um Ninja
        /// diferente), então quem deduplica é quem recebe.
        ///
        /// Existe pra que os dois carregadores não precisem varrer as quatro trilhas por conta própria
        /// — dois laços iguais em serviços diferentes é onde a divergência aparece.
        /// </summary>
        public IEnumerable<(Faccao Faccao, Fases Fase, Dificuldade Dificuldade)> FasesConcluidas()
            => trilhas.SelectMany(t => t.Value
                .SelectMany(cap => Enum.GetValues<Fases>()
                    .Where(f => cap.FaseConcluida[(int)f - 1])
                    .Select(f => (cap.Faccao, f, t.Key))));

        public bool EstaDesbloqueado(Faccao faccao, Fases fase, Dificuldade dificuldade)
        {
            Capitulo cap = ObterCapitulo(faccao, dificuldade);
            return DificuldadeDesbloqueada(dificuldade) && cap.CapDesblock && cap.FaseDesblock[(int)fase - 1];
        }

        /// <summary>
        /// Carrega o progresso salvo, restaurando as trilhas. Save ausente/corrompido → mantém o
        /// progresso inicial (default em memória): a porta devolve null e o jogo abre do zero. É o que
        /// acontece com o save ANTERIOR à dificuldade, que guardava uma lista solta — descartar sem
        /// migração foi decisão do Gabriel (GDD §7).
        /// </summary>
        public void CarregarProgresso()
        {
            var salvo = _repo.Carregar<Dictionary<Dificuldade, List<Capitulo>>>(ChaveProgresso);
            if (salvo == null || salvo.Count == 0) return;

            trilhas = TrilhasIniciais();
            foreach (var (dificuldade, capitulos) in salvo)
                trilhas[dificuldade] = capitulos;
        }

        public bool CapituloConcluido(Faccao faccao, Dificuldade dificuldade)
        {
            return ObterCapitulo(faccao, dificuldade).FaseConcluida.All(f => f);
        }

        public bool FaseConcluida(Faccao faccao, Fases fase, Dificuldade dificuldade)
        {
            return ObterCapitulo(faccao, dificuldade).FaseConcluida[(int)fase - 1];
        }

        #endregion
    }
}
