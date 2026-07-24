using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;

namespace ApostlesWar.Domain.Champs.Folclore
{
    /// <summary>
    /// Tengu — champ como DADO. CorteDeVento e Vendaval estreiam o `ignorarStatus` no Dano (a
    /// LISTA de status que o golpe pula no cálculo — ver Dano.cs): CorteDeVento fura o Escudo (e
    /// o multiplicador escala com o tamanho dele, via Func), Vendaval fura ProtecaoAliado +
    /// BuffDefesa. Passiva: Ventania.Passiva.cs.
    /// </summary>
    public static class Tengu
    {
        public static Personagem Definir() => new(
            2, Faccao.Folclore, "Tengu", "👺", 800, 280, 160,
            CorteDeVento(), Vendaval(), new Ventania());

        static HabilidadeAtiva CorteDeVento() => new(
            "Corte de Vento", "🌬️", cooldown: 3, "Ataca todos ignorando Escudo. Dano aumenta com escudo do alvo.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano(MultPorEscudoDoAlvo, ignorarStatus: new[] { typeof(Escudo) }),
            });

        static HabilidadeAtiva Vendaval() => new(
            "Vendaval", "🌪️", cooldown: 4, "+150% ATK ignorando Proteção, BuffDefesa e 50% DEF.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new Dano(2.5, ignorarDefesaPct: 0.50,
                    ignorarStatus: new[] { typeof(ProtecaoAliado), typeof(BuffDefesa) }),
            });

        // Multiplicador do Corte de Vento: 1.0 + proporção do escudo do alvo sobre o HP máx
        // inicial (cap +100%). O golpe IGNORA o Escudo (não consome), mas usa o tamanho dele
        // pra escalar o dano — anti-tanque.
        static double MultPorEscudoDoAlvo(Combate atacante, Combate alvo)
        {
            int escudo = alvo.StatusAtivos.OfType<Escudo>().Sum(e => e.PontosRestantes);
            double proporcao = escudo > 0 ? Math.Min((double)escudo / alvo.HPMaximoInicial, 1.0) : 0.0;
            return 1.0 + proporcao;
        }
    }
}
