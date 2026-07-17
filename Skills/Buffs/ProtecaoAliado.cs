using ApostlesWar;

namespace ApostlesWar.Skills.Buffs
{
    /// <summary>
    /// Aliado com este buff sofre 30% a menos de dano; esse 30% vai pro Aplicador (sem defesa).
    /// Se o Aplicador morrer, o status se autoremove no próximo turno via expiração natural.
    /// </summary>
    class ProtecaoAliado : Buff, IModificaDanoRecebido
    {
        public Combate Aplicador { get; }

        public ProtecaoAliado(Combate aplicador, int turnos = 2, double percentual = 0.30)
            : base("Proteção de Aliado", "🦴", turnos, percentual,
                $"Redireciona {percentual * 100:F0}% do dano para o aplicador.")
        {
            Aplicador = aplicador;
        }

        // Só redireciona golpes que provocam reação (ataques/revides). Quem NÃO redireciona
        // (Veneno/Queima/DanoIndireto) lista ProtecaoAliado em NaturezasDano.Ignora — e como o
        // redirecionamento abaixo usa DanoIndireto (que ignora ProtecaoAliado), isso corta o loop
        // de proteção mútua (A→B→A) estruturalmente, sem depender de disciplina.
        public int ModificarDanoRecebido(Combate portador, int dano)
        {
            if (!Aplicador.EstaVivo()) return dano;

            int redirecionado = (int)(dano * Valor);
            Aplicador.ReceberDano(redirecionado, NaturezasDano.DanoIndireto);
            return dano - redirecionado;
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
