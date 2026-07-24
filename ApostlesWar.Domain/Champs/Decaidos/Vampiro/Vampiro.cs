using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Champs.Decaidos
{
    /// <summary>
    /// Vampiro — champ como DADO. Controle de Sangue é golpe único furando 50% da DEF (molde do
    /// Vendaval). Vampiro Primordial dá Invencível aos aliados e crita 1 inimigo. Passiva:
    /// Drenagem.Passiva.cs (ignora Invencível e Bloqueio Total no ataque — fonte permanente do
    /// "ignorar", ver ADR-composicao-de-acoes §8 / ROADMAP unificar-ignorar).
    /// </summary>
    static class Vampiro
    {
        public static Personagem Definir() => new(
            2, Faccao.Decaidos, "Vampiro", "🧛", 800, 280, 160,
            ControleDeSangue(), VampiroPrimordial(), new Drenagem());

        static HabilidadeAtiva ControleDeSangue() => new(
            "Controle de Sangue", "🩸", cooldown: 3, "+200% ATK ignorando 50% DEF.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new Dano(3.0, ignorarDefesaPct: 0.50),
            });

        static HabilidadeAtiva VampiroPrimordial() => new(
            "Vampiro Primordial", "🌙", cooldown: 4, "Invencível em todos os aliados (2t) e ataque crítico em 1 inimigo.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new AplicarBuff(() => new Invencivel(duracao: 2), Escopo.TodosAliados),
                new Dano(1.0, forcaCritico: true),
            });
    }
}
