namespace ApostlesWar.Domain
{
    /// <summary>
    /// Aplica Escudo por um fragmento de Valor (ex: Valor.PorHP(0.30) = 30% do HP máximo do
    /// alvo). Compartilha o fragmento de Valor com Cura — difere só no verbo final (ADR-
    /// composicao-de-acoes §5.5). Já estava mapeada no vocabulário (§5.1); Lealdade (Rei) é o
    /// 1º cliente migrado. Nomeada AplicarEscudo (não "Escudo", espelhando AplicarBuff/
    /// AplicarDebuff) porque o nome cru colidiria com Skills.Buffs.Escudo: o namespace raiz
    /// ApostlesWar é ENVOLVENTE de praticamente todo o resto do código, então um tipo Escudo
    /// aqui venceria silenciosamente qualquer `using ApostlesWar.Domain.Skills.Buffs;` existente
    /// (sem erro de ambiguidade — resolução por namespace envolvente tem prioridade sobre
    /// using), quebrando todo `new Escudo(pontos, turnos)` que hoje se refere ao buff.
    /// </summary>
    public class AplicarEscudo : Acao
    {
        private readonly ValorFn _valor;
        private readonly int _duracao;

        public AplicarEscudo(ValorFn valor, int duracao, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : base(escopo, estadoAlvo)
        {
            _valor = valor;
            _duracao = duracao;
        }

        public override Utilidade Utilidade => Utilidade.Reforcar;

        /// <summary>
        /// Escudo é buff: se todos os alvos bloqueiam buff, não adianta. A sonda usa 1 ponto porque
        /// só o TIPO importa pro <see cref="Combate.PodeReceber"/> — o valor real depende do
        /// `eventos` da execução, que aqui ainda não existe.
        /// </summary>
        public override bool TemEfeitoUtil(Combate atacante, IReadOnlyList<Combate> alvos)
            => alvos.Any(a => a.PodeReceber(new ApostlesWar.Domain.Skills.Buffs.Escudo(1, _duracao)));

        public override void Executar(Combate atacante, Combate alvo, List<EventoCombate> eventos)
            => new ApostlesWar.Domain.Skills.Buffs.Escudo(_valor(atacante, alvo, eventos), _duracao).Aplicar(alvo);
    }
}
