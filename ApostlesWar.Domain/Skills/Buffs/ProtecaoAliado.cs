using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Skills.Buffs
{
    /// <summary>
    /// Aliado com este buff sofre 30% a menos de dano; esse 30% é redirecionado pro Aplicador.
    ///
    /// O redirecionamento viaja como <c>DanoIndireto</c>, que passa por defesa e escudo normalmente
    /// — então a DEF do protetor abate o que ele recebe: quanto mais tanque, mais barato sai
    /// proteger. Isso é REGRA DECIDIDA (jul/2026), não acidente da natureza escolhida: é a fantasia
    /// do tanque que se põe na frente. O que o PROTEGIDO desconta não muda com isso — são sempre os
    /// 30% cheios. Fixado em ProtecaoAliado_ADefesaDoProtetorAbateOQueEleRecebe.
    ///
    /// Se o Aplicador morrer, o status se autoremove no próximo turno via expiração natural.
    /// </summary>
    public class ProtecaoAliado : Buff, IModificaDanoRecebido
    {
        public Combate Aplicador { get; }

        public ProtecaoAliado(Combate aplicador, int duracao = 2, double percentual = 0.30)
            : base("Proteção de Aliado", "🦴", duracao, percentual,
                $"Redireciona {percentual * 100:F0}% do dano para o aplicador.")
        {
            Aplicador = aplicador;
        }

        // O recurso gasto aqui é o HP do aplicador — então roda depois de quem reduz de graça: com o
        // Bloqueio Total no portador, o aliado passa a receber o dano JÁ zerado em vez de comer 30%
        // de um golpe que nem ia acontecer.
        public OrdemDeMitigacao OrdemDeMitigacao => OrdemDeMitigacao.ConsomeRecurso;

        // Só redireciona golpes que provocam reação (ataques/revides). Quem NÃO redireciona
        // (Veneno/Queima/DanoIndireto) lista ProtecaoAliado em NaturezasDano.Ignora — e como o
        // redirecionamento abaixo usa DanoIndireto (que ignora ProtecaoAliado), isso corta o loop
        // de proteção mútua (A→B→A) estruturalmente, sem depender de disciplina.
        public int ModificarDanoRecebido(Combate portador, int dano)
        {
            if (!Aplicador.EstaVivo()) return dano;

            int redirecionado = Redirecionado(dano);
            Aplicador.ReceberDano(redirecionado, NaturezasDano.DanoIndireto);
            return dano - redirecionado;
        }

        /// <summary>
        /// Quanto deste golpe sai do portador e vai pro protetor. UMA conta só, compartilhada pelo
        /// aplicar e pelo prever — pelo mesmo motivo que fez o `OrdenarPorMitigacao` nascer: duas
        /// cópias da mesma fórmula divergem calado (basta um trocar o truncamento por arredondamento)
        /// e aí o bot avalia o alvo com um número que o golpe real não reproduz.
        /// </summary>
        private int Redirecionado(int dano) => (int)(dano * Valor);

        /// <summary>
        /// Mesma conta, SEM o redirecionamento. É o caso que mais justifica a separação: prever
        /// chamando o Modificar acima causaria dano de verdade no aplicador — a previsão feriria
        /// o aliado que ela só queria consultar.
        /// </summary>
        public int PreverDanoRecebido(Combate portador, int dano)
        {
            if (!Aplicador.EstaVivo()) return dano;
            return dano - Redirecionado(dano);
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
