using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;

namespace ApostlesWar.Domain.Champs.Decaidos
{
    /// <summary>
    /// Vampiro — champ como DADO. Controle de Sangue é golpe único furando 50% da DEF (molde do
    /// Vendaval). Vampiro Primordial dá Invencível aos aliados e crita 1 inimigo. Passiva:
    /// Drenagem.Passiva.cs (ignora Invencível e Bloqueio Total no ataque — fonte permanente do
    /// "ignorar", ver ADR-composicao-de-acoes §8 / ROADMAP unificar-ignorar).
    /// </summary>
    public static class Vampiro
    {
        public static Personagem Definir() => new(
            2, Faccao.Decaidos, "Vampiro", "🧛", 800, 280, 160,
            ControleDeSangue(), VampiroPrimordial(), new Drenagem());

        static HabilidadeAtiva ControleDeSangue() => new(
            "Controle de Sangue", "🩸", cooldown: 3, "Ataca com 350% do ATK ignorando 50% DEF.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new Dano(3.5, ignorarDefesaPct: 0.50),
            });

        static HabilidadeAtiva VampiroPrimordial() => new(
            "Vampiro Primordial", "🌙", cooldown: 3, "Aplica Invencível em todos os aliados por 2 turnos." +
            "\nDepois ataca 1 inimigo com 450% do ATK, esse ataque é sempre crítico.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new AplicarBuff(() => new Invencivel(duracao: 2), Escopo.TodosAliados),
                new Dano(4.5, forcaCritico: true),
            });
    }
}
