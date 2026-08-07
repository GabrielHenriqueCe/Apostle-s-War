using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Ativas;
using ApostlesWar.Application.Services;

namespace ApostlesWar.Application.Controllers
{
    /// <summary>
    /// Decide o turno sozinho — pelo inimigo da campanha, pelos dois lados no Bot×Bot, e pelo
    /// jogador quando ele liga o modo automático. Um cérebro só; quem o pluga muda, o raciocínio não.
    ///
    /// A decisão sai em DOIS passos independentes:
    ///   1. QUAL habilidade — pelo que ela FAZ. Cada <see cref="Acao"/> diz se tem trabalho a fazer
    ///      agora (<see cref="Acao.TemEfeitoUtil"/>) e que bem ela faz (<see cref="Utilidade"/>); a
    ///      habilidade vale se QUALQUER ação dela passar, e entra na fila pela melhor delas.
    ///   2. QUAL alvo — pelo que o golpe REMOVE de vida, com abate na frente.
    /// O alvo nunca fura a fila: se a prioridade manda curar, cura, mesmo com um abate na mesa.
    ///
    /// Por que o cérebro não sabe o que é "um Dano" ou "uma Cura": porque perguntar `is Dano` seria
    /// reabrir o dispatch por tipo concreto que o modelo de capacidades fechou (#9). Ação nova
    /// classifica a si mesma e o cérebro continua igual — inclusive as bespoke dos apóstolos.
    /// </summary>
    public class ControladorBot : IControladorDeTurno
    {
        /// <summary>
        /// A ORDEM DE PREFERÊNCIA — a única opinião tática deste arquivo, e o lugar de mexer nela.
        /// Ressuscitar vale mais que curar, curar mais que limpar, e bater é o que sobra quando não
        /// há nada melhor a fazer.
        ///
        /// É ABSOLUTA: entre as habilidades que passaram, a de maior utilidade vence sempre — nada de
        /// pontuação somando pesos. O Gabriel cravou assim depois de ver, em outros jogos, estratégia
        /// degenerada emergir de score (matar o próprio aliado fraco pra negar um buff ao time). Fila
        /// fixa é previsível, explicável e depurável; e como quase toda habilidade tem cooldown, o
        /// apóstolo usa a boa, ela esfria, e no turno seguinte ele bate.
        ///
        /// TurnoExtra fica logo acima de Ferir de propósito: ganhar o turno é ótimo DE CARONA, mas não
        /// pode arrastar a habilidade pra frente da fila. Com ele no topo, o Copiando do Mímico
        /// dispararia assim que saísse do cooldown mesmo sem nenhum buff inimigo pra roubar — gastando
        /// a habilidade pelo turno extra e jogando o roubo fora. Julgado pelo resto, ele só sobe
        /// quando há o que roubar.
        ///
        /// Custo não aparece: é o preço da habilidade, nunca razão pra usá-la.
        /// </summary>
        private static readonly Utilidade[] Prioridade =
        {
            Utilidade.Reviver,
            Utilidade.Curar,
            Utilidade.LimparDebuffs,
            Utilidade.Reforcar,
            Utilidade.TirarBuffs,
            Utilidade.Enfraquecer,
            Utilidade.TurnoExtra,
            Utilidade.Ferir,
        };

        /// <summary>
        /// O quanto se foge de cada punição, da pior pra menos pior. Espinhos gruda dois DoTs no
        /// agressor (o custo PERSISTE), contra-ataque é uma A1 na cara (uma vez), reflexo devolve uma
        /// fração do dano (o mais barato — e contra alvo blindado nem dispara). Índice menor = pior.
        /// </summary>
        private static readonly TipoDePunicao[] GravidadeDaPunicao =
        {
            TipoDePunicao.AplicaStatus,
            TipoDePunicao.ContraAtaca,
            TipoDePunicao.RefleteDano,
        };

        private readonly SelecaoDeAlvoService _selecaoDeAlvoService;

        /// <summary>
        /// O alvo que a escolha da habilidade já elegeu. O motor pergunta ação e alvo em DUAS
        /// chamadas, mas escolher bem a habilidade exige saber em quem ela cairia — então o alvo é
        /// decidido junto e guardado aqui até o motor vir buscá-lo. Mesmo padrão do `_sairSolicitado`
        /// no controlador do jogador.
        /// </summary>
        private Combate? _alvoEscolhido;

        /// <summary>
        /// O inimigo que o JOGADOR apontou (o foco do modo automático). Quando setado, ele passa na
        /// frente na escolha de alvo — inclusive do abate, que é a melhor jogada automática que este
        /// arquivo conhece. É de propósito: ordem explícita de quem está jogando vale mais que
        /// heurística, senão o botão não seria um comando, seria uma sugestão.
        ///
        /// O que ele NÃO muda é a fila de <see cref="Prioridade"/>: foco diz em QUEM bater, não O QUE
        /// usar. Com o time inteiro convergindo, curar continua vindo antes de ferir.
        ///
        /// Só a instância que joga pelo humano recebe isto (ver ControladorJogadorWeb); a que joga
        /// pelo inimigo nunca — senão o jogador miraria pelos dois lados.
        /// </summary>
        public Combate? AlvoPreferido { get; set; }

        public ControladorBot(SelecaoDeAlvoService selecaoDeAlvoService)
            => _selecaoDeAlvoService = selecaoDeAlvoService;

        // O bot nunca encerra nem volta: sempre devolve algo (non-null).
        public HabilidadeAtiva? EscolherAcao(Combate atacante, List<Combate> aliados, List<Combate> defensores)
        {
            var ctx = new ContextoCombate(atacante, aliados, defensores);

            var a1 = atacante.Personagem.Habilidades.OfType<AtaqueBasico>().First();
            var disponiveis = atacante.Personagem.Habilidades.OfType<HabilidadeAtiva>()
                .Where(h => atacante.Cooldowns[h].Disponivel)
                .ToList();

            var melhor = disponiveis
                .Select(h => Avaliar(h, ctx))
                .Where(a => a is not null)
                .OrderBy(a => a!.Posicao)                       // fila absoluta
                .ThenByDescending(a => a!.Alcance)              // área > aleatório > único
                .ThenByDescending(a => a!.AcoesUteis)           // faz mais coisas
                .ThenByDescending(a => a!.VidaRemovida)         // e, no fim, quem machuca mais
                .FirstOrDefault();

            HabilidadeAtiva escolhida = melhor?.Habilidade ?? a1;   // nada serve agora → bate
            _alvoEscolhido = melhor?.Alvo ?? MelhorAlvoPara(a1, ctx, defensores);
            return escolhida;
        }

        public Combate? EscolherAlvo(List<Combate> disponiveis, List<Combate> aliados, List<Combate> defensores)
        {
            // O alvo eleito junto com a habilidade, se o motor ainda o considera legítimo. A lista
            // que chega aqui já passou por Provocar/Bloqueio e pelo estado exigido, então ela manda.
            if (_alvoEscolhido != null && disponiveis.Contains(_alvoEscolhido))
                return _alvoEscolhido;

            return _selecaoDeAlvoService.EscolherAlvoBot(disponiveis);
        }

        // ---------- Passo 1: vale a pena usar esta habilidade? ----------

        /// <summary>O que se sabe de uma habilidade depois de olhá-la sem usá-la.</summary>
        private sealed record Avaliacao(
            HabilidadeAtiva Habilidade,
            int Posicao,        // índice na fila de Prioridade (menor = melhor)
            int Alcance,        // 2 = área, 1 = aleatório, 0 = alvo único
            int AcoesUteis,     // quantas ações dela realmente fazem algo agora
            int VidaRemovida,   // quanto o melhor alvo perderia, se ela machuca
            Combate? Alvo);

        /// <summary>
        /// Olha a habilidade ação por ação. Ela vale se ALGUMA delas tem o que fazer; e vale PELA
        /// MELHOR delas — é isso que faz a A2 do Papai Noel (dano + debuff + buff) ser lida como
        /// buff, e não como mais um ataque. Devolve null se nada nela serve agora.
        /// </summary>
        private Avaliacao? Avaliar(HabilidadeAtiva hab, ContextoCombate ctx)
        {
            var uteis = hab.Acoes
                .Where(acao => acao.Utilidade != Utilidade.Custo)   // preço não é motivo pra usar
                .Where(acao => acao.TemEfeitoUtil(ctx.Atacante, hab.AlvosPossiveis(acao, ctx)))
                .ToList();

            if (uteis.Count == 0) return null;

            int posicao = uteis.Min(acao => Array.IndexOf(Prioridade, acao.Utilidade));
            Combate? alvo = MelhorAlvoPara(hab, ctx, ctx.Inimigos);

            return new Avaliacao(
                hab,
                posicao,
                Alcance: hab.NumeroDeAlvos == int.MaxValue ? 2 : hab.TipoAlvo == TipoAlvo.Aleatorio ? 1 : 0,
                AcoesUteis: uteis.Count,
                VidaRemovida: alvo is null ? 0 : VidaRemovidaPor(hab, ctx.Atacante, alvo),
                Alvo: alvo);
        }

        // ---------- Passo 2: em quem ela cai? ----------

        /// <summary>
        /// O melhor alvo pra esta habilidade, em ordem lexicográfica: primeiro quem MORRE (inimigo
        /// morto para de agir — é o maior ganho do turno), depois quem pune menos, e por fim quem
        /// perde mais vida.
        ///
        /// Duas regras que pareciam precisar de código não aparecem aqui, e é de propósito: "evitar
        /// alvo com bloqueio de dano" e "evitar Invencível" caem sozinhas de `PreverVidaRemovida`,
        /// que devolve ~0 para os dois. Quem prevê honestamente não precisa desviar à mão.
        ///
        /// O <see cref="AlvoPreferido"/> entra ANTES de tudo: é o jogador apontando, não uma
        /// heurística. E não precisa de nenhum cuidado extra pra quando ele morre ou fica intocável —
        /// nesses casos ele simplesmente não está na lista, e a ordenação segue como se nunca tivesse
        /// sido pedido.
        /// </summary>
        private Combate? MelhorAlvoPara(HabilidadeAtiva hab, ContextoCombate ctx, List<Combate> candidatos)
        {
            if (hab.TipoLista != TipoLista.Inimigos) return null;   // alvo aliado/self o motor resolve

            var vivos = candidatos.Where(c => c.EstaVivo()).ToList();
            if (vivos.Count == 0) return null;

            return vivos
                .OrderByDescending(alvo => alvo == AlvoPreferido)
                .ThenByDescending(alvo => Mata(hab, ctx.Atacante, alvo))
                .ThenByDescending(alvo => LiberdadeDe(alvo))
                .ThenByDescending(alvo => VidaRemovidaPor(hab, ctx.Atacante, alvo))
                .First();
        }

        private static bool Mata(HabilidadeAtiva hab, Combate atacante, Combate alvo)
        {
            int removida = VidaRemovidaPor(hab, atacante, alvo);
            return removida > 0 && removida >= alvo.HPAtual;
        }

        /// <summary>
        /// Quão livre de punição é bater neste alvo — maior é melhor. Sem punição alguma dá o valor
        /// máximo; com punição, vale a MENOS grave que ele carrega (não adianta somar: o que assusta
        /// é a pior). Só BUFF conta: passiva é identidade permanente do apóstolo, e fugir dela deixaria
        /// Herói, Elfo, Zumbi e Cocô praticamente inatacáveis.
        /// </summary>
        private static int LiberdadeDe(Combate alvo)
        {
            var punicoes = alvo.StatusAtivos.OfType<IPuneQuemAtaca>().ToList();
            if (punicoes.Count == 0) return GravidadeDaPunicao.Length;

            return punicoes.Max(p => Array.IndexOf(GravidadeDaPunicao, p.Punicao));
        }

        /// <summary>
        /// Quanta vida esta habilidade tiraria deste alvo. Soma o que cada ação promete, então uma
        /// habilidade de dois golpes é lida como os dois — e a explosão conta junto, porque quem
        /// responde é a AÇÃO, não uma varredura por tipo aqui.
        /// </summary>
        private static int VidaRemovidaPor(HabilidadeAtiva hab, Combate atacante, Combate alvo)
            => hab.Acoes.Sum(acao => acao.PreverVidaRemovida(atacante, alvo));
    }
}
