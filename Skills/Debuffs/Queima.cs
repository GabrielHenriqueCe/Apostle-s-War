using ApostlesWar;

namespace ApostlesWar.Skills.Debuffs
{
    /// <summary>
    /// Debuff stack-based de queimadura.
    /// - Duração = nº de stacks (perde 1 stack por turno do portador, igual Veneno)
    /// - Por turno: causa 5% do HPMaximoInicial de dano direto E reduz HPMaximo na mesma quantidade
    /// - Cap de redução acumulada (somando todas Queimas do combate): 25% do HPMaximoInicial
    /// - Após o cap: dano continua, redução para
    /// - Cap de stacks: 5 (configurável)
    /// - Reaplicar: adiciona N stacks (até o cap)
    /// </summary>
    class Queima : Debuff, IStatusComTick
    {
        public const double DanoPorTurno = 0.05;    // 5% HP inicial
        public const double CapPropio = 0.25;       // 25% redução acumulada

        public int Stacks { get; private set; }
        public int CapStacks { get; }

        public Queima(int stacks = 1)
            : base("Queima", "🔥", stacks, DanoPorTurno,
                $"Causa {DanoPorTurno * 100:F0}% HP/turno e reduz HP máx.")
        {
            Stacks = stacks;
            CapStacks = 5;
        }

        public override void Aplicar(Combate alvo)
        {
            if (!alvo.PodeReceber(this)) return;

            var existente = alvo.StatusAtivos.OfType<Queima>().FirstOrDefault();
            if (existente != null)
            {
                int novosStacks = Math.Min(existente.Stacks + this.Stacks, existente.CapStacks);
                existente.Stacks = novosStacks;
                existente.DuracaoRestante = novosStacks;
                return;
            }

            alvo.StatusAtivos.Add(this);
        }

        /// <summary>
        /// No início do turno do portador: causa dano direto e reduz HP máximo.
        /// Redução respeita o cap próprio (25% do HPMaximoInicial).
        /// </summary>
        public override void AoIniciarTurno(Combate portador)
        {
            int valor = (int)(portador.HPMaximoInicial * DanoPorTurno);
            // Dano à vida: ignora defesa e escudo, mas BloquearDano ainda bloqueia.
            portador.ReceberDano(valor, NaturezasDano.QueimaDano);
            // (a ReduzirHPMaximo abaixo continua igual — ignora tudo)

            // Redução só se cap próprio não foi atingido
            int capAbsoluto = (int)(portador.HPMaximoInicial * CapPropio);
            int aindaPodeReduzir = capAbsoluto - portador.HPMaximoReduzidoTotal;

            if (aindaPodeReduzir > 0)
            {
                int reducao = Math.Min(valor, aindaPodeReduzir);
                portador.ReduzirHPMaximo(reducao);
            }
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }

        /// <summary>
        /// IStatusComTick: aplica imediatamente todo o efeito remanescente FAZENDO O QUE A
        /// QUEIMA FAZ (dano + redução de HP máximo, respeitando o cap próprio), remove a Queima
        /// e devolve o EventoDano da detonação. Dano = stacks × 5% HPMaximoInicial.
        /// </summary>
        public EventoDano Detonar(Combate portador, Combate detonador)
        {
            int bruto = (int)(portador.HPMaximoInicial * DanoPorTurno * Stacks);
            // Dano à vida bloqueável; a redução de HP máximo (abaixo) ignora tudo.
            var (efetivo, absorvido) = portador.ReceberDano(bruto, NaturezasDano.QueimaDano);

            int capAbsoluto = (int)(portador.HPMaximoInicial * CapPropio);
            int aindaPodeReduzir = capAbsoluto - portador.HPMaximoReduzidoTotal;
            if (aindaPodeReduzir > 0)
            {
                int reducao = Math.Min(bruto, aindaPodeReduzir);
                portador.ReduzirHPMaximo(reducao);
            }

            Remover(portador);
            return new EventoDano(
                Atacante: detonador, Alvo: portador,
                DanoBruto: bruto, DanoEfetivo: efetivo, AbsorvidoPeloEscudo: absorvido,
                Critico: false, HPRestante: Math.Max(0, portador.HPAtual),
                Natureza: NaturezasDano.QueimaDano);
        }
    }
}