using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;

namespace ApostlesWar.Application.Services
{
    /// <summary>
    /// A XP de cada apóstolo e o nível que ela paga. Guarda **só a XP** — o nível é derivado pelo
    /// <see cref="Progressao.NivelPorXp"/> na hora de aplicar. Guardar as duas pontas à mão foi
    /// exatamente o defeito que produziu a divergência da Velocidade (#247).
    ///
    /// Quem MUDA de verdade é o roster do <see cref="PersonagemService"/>: um apóstolo é uma
    /// instância só no jogo inteiro, e é ela que sobe de nível. O inimigo da campanha por isso é
    /// CÓPIA (`Personagem.ComNivel`) — nivelar a instância compartilhada vazaria pro time do jogador.
    /// </summary>
    public class ProgressaoService
    {
        private const string ChaveProgressao = "progressao";

        private readonly PersonagemService _personagens;
        private readonly IRepositorioDeSave _repo;

        private List<ApostoloProgredido> progresso = new();

        public ProgressaoService(PersonagemService personagens, IRepositorioDeSave repo)
        {
            _personagens = personagens;
            _repo = repo;
        }

        /// <summary>Lê o save e põe cada apóstolo no nível que a XP dele paga.</summary>
        public void Carregar()
        {
            progresso = _repo.Carregar<List<ApostoloProgredido>>(ChaveProgressao) ?? new();
            foreach (ApostoloProgredido p in progresso)
                _personagens.ObterPersonagem(p.Faccao, p.Slot).AplicarNivel(Progressao.NivelPorXp(p.Xp));
        }

        /// <summary>
        /// O pote da fase repartido entre QUEM ESTAVA EM CAMPO, igual pra todo mundo e independente de
        /// quem bateu. Sozinho leva tudo; em quatro, cada um leva um quarto — é isso que faz do solo o
        /// jeito rápido de subir UM e do time cheio o jeito de subir quatro, sem regra nova.
        ///
        /// A XP que passa do teto simplesmente se perde: quem concentra troca alcance por velocidade e
        /// paga em desperdício.
        /// </summary>
        public void Creditar(List<Personagem> time, int pote)
        {
            if (time.Count == 0 || pote <= 0) return;

            int porApostolo = pote / time.Count;
            foreach (Personagem p in time)
            {
                int xp = XpDe(p) + porApostolo;
                progresso.RemoveAll(a => a.Faccao == p.Faccao && (int)a.Slot == p.Slot);
                progresso.Add(new ApostoloProgredido(p.Faccao, (Slot)p.Slot, xp));
                p.AplicarNivel(Progressao.NivelPorXp(xp));
            }

            _repo.Salvar(ChaveProgressao, progresso);
        }

        /// <summary>A XP acumulada deste apóstolo. Sem registro = 0, que é o nível 1.</summary>
        public int XpDe(Personagem apostolo)
            => progresso.FirstOrDefault(a => a.Faccao == apostolo.Faccao && (int)a.Slot == apostolo.Slot)?.Xp ?? 0;

        /// <summary>
        /// O quanto falta pro próximo nível, pra a barra da ficha: (o que já entrou nesta faixa, o que
        /// a faixa inteira custa). No teto devolve a faixa cheia — a barra fica cheia e para.
        /// </summary>
        public (int Feito, int Total) FaixaDoNivel(Personagem apostolo)
        {
            int nivel = apostolo.Nivel;
            if (nivel >= Arquetipos.NivelMaximo) return (1, 1);

            int piso = Progressao.XpParaNivel(nivel);
            return (XpDe(apostolo) - piso, Progressao.XpParaNivel(nivel + 1) - piso);
        }

        /// <summary>
        /// Volta todo mundo pro nível 1 — disco E memória, como o <see cref="CapitulosService.Resetar"/>:
        /// as instâncias do roster são compartilhadas e sobrevivem ao "excluir conta", então apagar só
        /// o arquivo deixaria o jogador novo com o elenco nivelado do anterior.
        /// </summary>
        public void Resetar()
        {
            _repo.Excluir(ChaveProgressao);
            progresso = new();
            foreach (Personagem p in _personagens.Todos())
                p.AplicarNivel(Arquetipos.NivelMinimo);
        }
    }

    /// <summary>
    /// Um apóstolo no save da progressão: a identidade (facção + slot, a mesma do
    /// <see cref="ApostoloSalvo"/>) e a XP. O nível NÃO entra aqui de propósito — ele é conta, não dado.
    /// </summary>
    public record ApostoloProgredido(Faccao Faccao, Slot Slot, int Xp);
}
