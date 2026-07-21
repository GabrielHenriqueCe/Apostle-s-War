using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Champs.LadoSombrio
{
    /// <summary>
    /// Caveira — champ como DADO (ver ADR-composicao-de-acoes §10). OssoDuroDeRoer estreia o
    /// Escopo.OutrosAliados (2 clientes mapeados: aqui + Circo) e a fábrica de AplicarBuff
    /// ciente do atacante (ProtecaoAliado carrega o Aplicador). Passiva: Necromancia.Passiva.cs.
    /// </summary>
    static class Caveira
    {
        public static Personagem Definir() => new(
            1, Faccao.LadoSombrio, "Caveira", "💀", 600, 280, 200,
            Ossinho(), OssoDuroDeRoer(), new Necromancia());

        static HabilidadeAtiva Ossinho() => new(
            "Ossinho", "🦴", cooldown: 3, "Ataca todos os inimigos. Dano aumenta conforme o HP da Caveira diminui (até 2x).",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano((atk, alvo) => 2.0 - (double)atk.HPAtual / atk.HPMaximo),
            });

        static HabilidadeAtiva OssoDuroDeRoer() => new(
            "Osso Duro de Roer", "🦴", cooldown: 3, "Protege aliados (30% do dano vai pra Caveira) e ganha Escudo de 30% HP.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarBuff(atk => new ProtecaoAliado(atk, duracao: 2, percentual: 0.30), Escopo.OutrosAliados),
                new AplicarEscudo(Valor.PorHPDoAtacante(0.30), duracao: 2, Escopo.ProprioAtacante),
            });
    }
}
