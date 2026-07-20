using ApostlesWar.Skills.Ativas;
using ApostlesWar.Services;

namespace ApostlesWar.Controllers
{
    /// <summary>
    /// Controlador do INIMIGO (e base do futuro bot inteligente / modo automático). Hoje: usa sempre
    /// o Ataque Básico e escolhe o alvo pela regra do SelecaoDeAlvoService. Engordar aqui (escolher
    /// entre habilidades disponíveis, mirar o alvo mais fraco etc.) não toca o loop de combate.
    /// </summary>
    internal class ControladorBot : IControladorDeTurno
    {
        private readonly SelecaoDeAlvoService _selecaoDeAlvoService;

        public ControladorBot(SelecaoDeAlvoService selecaoDeAlvoService)
            => _selecaoDeAlvoService = selecaoDeAlvoService;

        public HabilidadeAtiva EscolherAcao(Combate atacante, List<Combate> aliados, List<Combate> defensores)
            => atacante.Personagem.Habilidades.OfType<AtaqueBasico>().First();

        public Combate EscolherAlvo(List<Combate> disponiveis, List<Combate> aliados, List<Combate> defensores)
            => _selecaoDeAlvoService.EscolherAlvoBot(disponiveis);
    }
}
