using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Ativas;
using ApostlesWar.Application.Services;

namespace ApostlesWar.Application.Controllers
{
    /// <summary>
    /// Controlador do INIMIGO (e base do futuro bot inteligente / modo automático). Hoje: usa sempre
    /// o Ataque Básico e escolhe o alvo pela regra do SelecaoDeAlvoService. Engordar aqui (escolher
    /// entre habilidades disponíveis, mirar o alvo mais fraco etc.) não toca o loop de combate.
    /// </summary>
    public class ControladorBot : IControladorDeTurno
    {
        private readonly SelecaoDeAlvoService _selecaoDeAlvoService;

        public ControladorBot(SelecaoDeAlvoService selecaoDeAlvoService)
            => _selecaoDeAlvoService = selecaoDeAlvoService;

        // O bot nunca encerra nem volta: sempre devolve algo (non-null).
        public HabilidadeAtiva? EscolherAcao(Combate atacante, List<Combate> aliados, List<Combate> defensores)
            => atacante.Personagem.Habilidades.OfType<AtaqueBasico>().First();

        public Combate? EscolherAlvo(List<Combate> disponiveis, List<Combate> aliados, List<Combate> defensores)
            => _selecaoDeAlvoService.EscolherAlvoBot(disponiveis);
    }
}
