using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Champs.Especial
{
    /// <summary>
    /// Herói — champ como DADO. SalvandoDia é mais um cliente de OutrosAliados (ProtecaoAliado com
    /// proveniência); Super é buff-nos-aliados + ataque. Passiva: Vigilante.Passiva.cs (contra-ataque
    /// via Revide/A1 — passiva-pura, imune a roubo).
    /// </summary>
    static class Heroi
    {
        public static Personagem Definir() => new(
            2, Faccao.Especial, "Herói", "🦸", 800, 240, 200,
            SalvandoDia(), Super(), new Vigilante());

        static HabilidadeAtiva SalvandoDia() => new(
            "Salvando o Dia", "🦸", cooldown: 3, "Protege e +30% DEF em todos os aliados (2t).",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarBuff(() => new BuffDefesa(duracao: 2, percentual: 0.30), Escopo.TodosAliados),
                new AplicarBuff(atk => new ProtecaoAliado(atk, duracao: 2, percentual: 0.30), Escopo.OutrosAliados),
            });

        static HabilidadeAtiva Super() => new(
            "Super", "💪", cooldown: 3, "+25% ATK aos aliados e ataca todos +100% ATK.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new AplicarBuff(() => new BuffAtaque(duracao: 2, percentual: 0.25), Escopo.TodosAliados),
                new Dano(2.0),
            });
    }
}
