namespace ApostlesWar.Domain
{
    /// <summary>
    /// Status que modifica o dano recebido durante o cálculo (Escudo, Bloqueio, Proteção, Redução).
    ///
    /// Se o modificador AGE neste golpe é decidido fora, por UMA língua só (unificação
    /// jul/2026): o ReceberDano pergunta "este status está na lista de ignorados?" — a lista
    /// unida de natureza.Ignora + ignorarStatus (golpe) + IIgnoraStatusNoAtaque (champ). Não há
    /// mais `DeveAgir` por-status lendo flags da natureza; as regras que eram flags viraram
    /// entradas na lista `NaturezasDano` (ex: QueimaDano fura Escudo; todo dano sem reação fura
    /// ProtecaoAliado — o anti-loop de proteção mútua).
    ///
    /// DOIS MÉTODOS, porque a capacidade responde a DUAS perguntas que não são a mesma:
    /// "quanto passa?" (<see cref="PreverDanoRecebido"/>) e "quanto passa E gaste o que tiver de
    /// gastar" (<see cref="ModificarDanoRecebido"/>). Elas coincidiam enquanto só o golpe real
    /// perguntava; separaram-se quando o bot passou a precisar ESPIAR o resultado sem provocá-lo —
    /// o Escudo consome pontos e se remove, e o ProtecaoAliado chega a causar dano no protetor.
    /// Prever chamando o Modificar machucaria um aliado de verdade.
    ///
    /// INVARIANTE: `Prever(dano) == Modificar(dano)` para o mesmo estado — o Prever é o valor que o
    /// Modificar devolveria, só que sem os efeitos. Há teste genérico varrendo as implementações.
    /// Sem implementação default de propósito: uma capacidade nova é obrigada pelo compilador a
    /// responder as duas — herdar silenciosamente a errada é o tipo de bug que não grita.
    /// </summary>
    public interface IModificaDanoRecebido
    {
        /// <summary>Aplica a modificação. PODE consumir recurso e ter efeito colateral.</summary>
        int ModificarDanoRecebido(Combate portador, int dano);

        /// <summary>
        /// Quanto do dano PASSARIA por este modificador — PURO: não consome nada, não toca em
        /// ninguém. É o que o bot usa pra comparar alvos sem alterar a batalha que está prevendo.
        /// </summary>
        int PreverDanoRecebido(Combate portador, int dano);
    }

    /// <summary>
    /// Capacidade E do modelo de capacidades: BLOQUEIO DE APLICAÇÃO.
    /// O status impede que outro StatusEffect seja aplicado no portador.
    /// Chamada em Combate.PodeReceber, antes de adicionar um novo status.
    /// Implementadores: ImunidadeDebuffs, ImpedirBeneficios (status); CascaDura,
    /// PeleDeDragao (passivas-pura).
    /// </summary>
    public interface IBloqueiaStatus
    {
        bool Bloqueia(StatusEffect novo);
    }

    /// <summary>
    /// Status com efeito de tick (dano periódico) que pode ser DETONADO — aplica de uma vez o
    /// efeito remanescente e se remove, em vez de esperar o próprio turno do portador. É o
    /// molde único das explosões (regra de Gabriel): a Ação Explodir orquestra igual pra todos,
    /// e cada status detona FAZENDO O QUE ELE FAZ (Veneno só dano; Queima dano + redução de HP
    /// máximo). Implementadores: Veneno (cliente: Putrefação), Queima (cliente: Inferno, migra
    /// nos Decaídos).
    /// </summary>
    public interface IStatusComTick
    {
        /// <summary>
        /// Aplica o efeito remanescente de uma vez, remove o status e devolve o EventoDano da
        /// detonação (bruto/efetivo/absorvido/natureza) — o interpretador agrega esses eventos
        /// junto dos de Dano (invariante do ADR-composicao-de-acoes §7), então a explosão
        /// aparece na exibição, conta no PorDanoCausado e a morte-por-explosão passa pelos
        /// Atos de morte. O detonador entra como Atacante do evento.
        /// </summary>
        EventoDano Detonar(Combate portador, Combate detonador);

        /// <summary>
        /// Quanta VIDA a detonação tiraria do portador — sem detonar. Espelho puro do
        /// <see cref="Detonar"/>, mesmo par que o `PreverDanoRecebido` faz com o `ReceberDano`.
        /// Existe porque quem AVALIA uma explosão antes de usá-la não pode gastar o status pra
        /// descobrir quanto ela vale.
        /// </summary>
        int PreverDetonacao(Combate portador);
    }
}