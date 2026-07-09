using ApostlesWar;

namespace ApostlesWar.Skills.Buffs
{
    /// <summary>
    /// Absorve dano antes de afetar o HP. Quando os pontos zeram, o buff se remove.
    /// Não acumula — se aplicado em cima de outro Escudo, mantém o de maior valor.
    /// </summary>
    class Escudo : Buff, IModificaDanoRecebido
    {
        public int PontosRestantes { get; private set; }

        public Escudo(int pontos, int turnos = 2)
            : base("Escudo", "🛡️", turnos, pontos, $"Absorve {pontos} de dano.")
        {
            PontosRestantes = pontos;
        }

        public override void Aplicar(Combate alvo)
        {
            // Mantém o maior — se já existe Escudo com mais pontos, ignora
            var existente = alvo.StatusAtivos.OfType<Escudo>().FirstOrDefault();
            if (existente != null)
            {
                if (this.PontosRestantes <= existente.PontosRestantes) return;
                alvo.StatusAtivos.Remove(existente);
            }
            alvo.StatusAtivos.Add(this);
        }

        public bool DeveAgir(NaturezaDano natureza) => !natureza.IgnoraEscudo;
        public int ModificarDanoRecebido(Combate portador, int dano)
        {
            if (PontosRestantes >= dano)
            {
                PontosRestantes -= dano;
                if (PontosRestantes == 0)
                    portador.StatusAtivos.Remove(this);
                return 0;
            }

            // Dano maior que escudo — escudo zera, excedente vai pro HP
            int excedente = dano - PontosRestantes;
            PontosRestantes = 0;
            portador.StatusAtivos.Remove(this);
            return excedente;
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
