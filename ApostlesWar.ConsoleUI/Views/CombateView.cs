using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;

namespace ApostlesWar.ConsoleUI.Views
{
    /// <summary>
    /// View da PARTIDA: renderiza a luta em andamento (times, ações, resultado de ataque, mensagens)
    /// e o menu de alvo. Separada do MenuView (telas de opção) na quebra do antigo MenuService.
    /// Sem dependência de service — só desenha combatentes. O input do menu de alvo vem da porta
    /// IEntrada (não lê tecla crua).
    ///
    /// É a impl de CONSOLE da porta <see cref="ITelaDeCombate"/>. Os dois métodos que NÃO estão na
    /// porta (`ExibirAcoes` e `EscolherAlvoNaTela`) são navegação por cursor, formato deste adapter:
    /// quem os usa é o ControladorJogador (console), não o fluxo de combate.
    /// </summary>
    public class CombateView : ITelaDeCombate
    {
        private readonly IEntrada _entrada;
        private readonly RelogioDoCombate _relogio;

        public CombateView(IEntrada entrada, RelogioDoCombate relogio)
        {
            _entrada = entrada;
            _relogio = relogio;
        }

        /// <summary>
        /// Limpa a tela da partida. Mora aqui (o adapter de console do combate) pra que nenhum
        /// Console.* vaze pro CombateService/Controlador — no porte Unity, é só uma troca de View.
        /// </summary>
        public void LimparTela() => Console.Clear();

        public void ExibirPartida(List<Combate> jogadores, List<Combate> inimigos)
        {
            Console.WriteLine($"═══ Turno {_relogio.NumeroDoTurno} ═══\n");
            Console.WriteLine("Seu time:");
            foreach (Combate j in jogadores)
            {
                string status = string.Join(" ", j.StatusAtivos.Select(s => $"{s.Simbolo}{s.Nome} {ObterNumeroEmoji(s.DuracaoRestante)}"));
                string escudo = FormatarEscudo(j);
                Console.WriteLine($"{j.Personagem.Simbolo} {j.Personagem.Nome} | HP:{j.HPAtual}{escudo} ATK:{j.Ataque} DEF:{j.Defesa} 🎯{(int)(j.TaxaCrit * 100)}% 💥x{1 + j.DanoCrit:0.0} {status}");
            }

            Console.WriteLine("\nInimigos:");
            int i = 1;
            foreach (Combate inimigo in inimigos.Where(d => d.EstaVivo()))
            {
                string status = string.Join(" ", inimigo.StatusAtivos.Select(s => $"{s.Simbolo}{s.Nome} {ObterNumeroEmoji(s.DuracaoRestante)}"));
                string escudo = FormatarEscudo(inimigo);
                Console.WriteLine($"{i} - {inimigo.Personagem.Simbolo} {inimigo.Personagem.Nome} | HP:{inimigo.HPAtual}{escudo} ATK:{inimigo.Ataque} DEF:{inimigo.Defesa} 🎯{(int)(inimigo.TaxaCrit * 100)}% 💥x{1 + inimigo.DanoCrit:0.0} {status}");
                i++;
            }
        }

        /// <summary>
        /// Tela de resumo do fim da batalha: por personagem do time, o que ele fez (dano causado/
        /// recebido, cura) + os stats finais. Só o time do jogador (os inimigos são por-rodada).
        /// Espera Confirmar pra seguir.
        /// </summary>
        public void ExibirResumoBatalha(List<Combate> jogador)
        {
            Console.Clear();
            Console.WriteLine("═══ RESUMO DA BATALHA ═══\n");
            ExibirLinhasResumo(jogador);
            Console.WriteLine("Enter - Continuar");
            while (_entrada.Ler().Tipo != TipoComando.Confirmar) { }
        }

        /// <summary>
        /// Resumo do duelo da ARENA: os DOIS times (dano causado/recebido, cura, stats finais) + quem
        /// venceu. É o instrumento de leitura do rebalance. Espera Confirmar pra seguir.
        /// </summary>
        public void ExibirResumoArena(List<Combate> equipe1, List<Combate> equipe2, bool venceuEquipe1)
        {
            Console.Clear();
            Console.WriteLine("═══ RESUMO DA ARENA ═══\n");
            Console.WriteLine($"🏆 Vencedor: Time {(venceuEquipe1 ? "1" : "2")}\n");
            Console.WriteLine("── Time 1 ──");
            ExibirLinhasResumo(equipe1);
            Console.WriteLine("── Time 2 ──");
            ExibirLinhasResumo(equipe2);
            Console.WriteLine("Enter - Continuar");
            while (_entrada.Ler().Tipo != TipoComando.Confirmar) { }
        }

        /// <summary>Linhas por combatente de um time num resumo (dano/cura + stats). Compartilhado
        /// pela batalha da campanha e pela Arena.</summary>
        private void ExibirLinhasResumo(List<Combate> combatentes)
        {
            foreach (Combate c in combatentes)
            {
                string estado = c.EstaVivo() ? "" : " ☠️";
                Console.WriteLine($"{c.Personagem.Simbolo} {c.Personagem.Nome}{estado}");
                Console.WriteLine($"   ⚔️ Dano causado: {c.DanoCausado}   🛡️ Dano recebido: {c.DanoRecebido}   💚 Cura: {c.CuraRecebida}");
                Console.WriteLine($"   HP:{c.HPAtual}/{c.HPMaximo}  ATK:{c.Ataque}  DEF:{c.Defesa}  🎯{(int)(c.TaxaCrit * 100)}%  💥x{1 + c.DanoCrit:0.0}\n");
            }
        }

        /// <summary>
        /// Retorna " 🛡️N" se o combatente tem Escudo ativo, ou string vazia.
        /// </summary>
        private string FormatarEscudo(Combate c)
        {
            var escudo = c.StatusAtivos.OfType<Domain.Skills.Buffs.Escudo>().FirstOrDefault();
            return escudo != null ? $" 🛡️{escudo.PontosRestantes}" : "";
        }

        public void ExibirAcoes(Combate atacante, int acaoSelecionada = 1)
        {
            Console.WriteLine("\nAções:");

            int i = 1;
            foreach (Habilidade hab in atacante.Personagem.Habilidades)
            {
                if (hab is HabilidadeAtiva)
                {
                    var cd = atacante.Cooldowns[hab];
                    string relogio = ObterRelogio(cd.CooldownRestante, cd.CooldownTotal);
                    string disponivel = cd.Disponivel ? "✅" : "🟣";
                    string cursor = acaoSelecionada == i ? "▶" : " ";
                    Console.WriteLine($"{cursor} {i} - {hab.Simbolo} {hab.Nome} {disponivel} {relogio}  {hab.Descricao}");
                    i++;
                }
            }

            bool temPassiva = atacante.Personagem.Habilidades.Any(h => h is HabilidadePassiva);
            if (temPassiva)
            {
                Console.WriteLine("\nPassivas:");
                foreach (Habilidade hab in atacante.Personagem.Habilidades)
                {
                    if (hab is HabilidadePassiva)
                    {
                        var cd = atacante.Cooldowns[hab];
                        string relogio = ObterRelogio(cd.CooldownRestante, cd.CooldownTotal);
                        string disponivel = cd.Disponivel ? "✅" : "🟣";
                        Console.WriteLine($"  {hab.Simbolo} {hab.Nome} {disponivel} {relogio}  {hab.Descricao}");
                    }
                }
            }
        }

        /// <summary>
        /// Retorna o emoji de relógio proporcional ao progresso do cooldown
        /// </summary>
        string ObterRelogio(int cooldownRestante, int cooldownTotal)
        {
            if (cooldownRestante == 0) return "🕛";

            string[] relogios = { "🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙" };
            int cooldownPassado = cooldownTotal - cooldownRestante;
            int indice = (int)Math.Round((double)cooldownPassado * 9 / cooldownTotal) - 1;
            indice = Math.Clamp(indice, 0, 8);
            return relogios[indice];
        }

        private string ObterNumeroEmoji(int numero)
        {
            string[] numeros = { "①", "②", "③", "④", "⑤", "⑥", "⑦", "⑧", "⑨", "⑩" };
            if (numero >= 1 && numero <= 10)
                return numeros[numero - 1];
            if (numero >= 999) return "♾️";   // permanente — mostra infinito
            return $"({numero})";
        }

        /// <summary>
        /// Exibe o dano causado, HP restante no momento do hit e indicação de crítico.
        /// </summary>
        public void ExibirResultadoAtaque(Combate atacante, Combate alvo, EventoDano resultado)
        {
            string critico = resultado.Critico ? " 💥 ATAQUE CRÍTICO!" : "";
            Console.WriteLine($"{atacante.Personagem.Simbolo} causou {resultado.DanoEfetivo} de dano em {alvo.Personagem.Simbolo} {alvo.Personagem.Nome}{critico}");
            Console.WriteLine($"HP de {alvo.Personagem.Simbolo}: {resultado.HPRestante}/{alvo.HPMaximo}");
            ExibirMorteSe(alvo);
        }

        /// <summary>Mostra "morreu" quando o alvo caiu de fato (a morte já foi confirmada no ReceberDano).
        /// Hoje é texto; no porte vira o gatilho da animação de morte.</summary>
        private void ExibirMorteSe(Combate alvo)
        {
            if (!alvo.EstaVivo())
                Console.WriteLine($"💀 {alvo.Personagem.Simbolo} {alvo.Personagem.Nome} morreu!");
        }

        /// <summary>
        /// Exibe o dano de um TICK de status (veneno/queima) — alvo-cêntrico, o portador sofrendo o
        /// dano do próprio status no início do turno. Gêmeo do ExibirResultadoAtaque.
        /// </summary>
        public void ExibirDanoDeStatus(EventoDano r)
        {
            Console.WriteLine($"☠️ {r.Alvo.Personagem.Simbolo} {r.Alvo.Personagem.Nome} sofreu {r.DanoEfetivo} de dano (HP: {r.HPRestante}/{r.Alvo.HPMaximo})");
            ExibirMorteSe(r.Alvo);
        }

        /// <summary>
        /// Exibe uma cura — "irmã" da mensagem de dano. Auto-cura (Curador == Alvo) vira "recuperou";
        /// cura em terceiro vira "curou X em Y".
        /// </summary>
        public void ExibirCura(EventoCura c)
        {
            if (c.Quantidade == 0)
                Console.WriteLine($"💚 {c.Alvo.Personagem.Simbolo} {c.Alvo.Personagem.Nome} já está com a vida cheia ({c.HPRestante}/{c.Alvo.HPMaximo})");
            else if (c.Curador == c.Alvo)
                Console.WriteLine($"💚 {c.Alvo.Personagem.Simbolo} {c.Alvo.Personagem.Nome} recuperou {c.Quantidade} de vida (HP: {c.HPRestante}/{c.Alvo.HPMaximo})");
            else
                Console.WriteLine($"💚 {c.Curador.Personagem.Simbolo} curou {c.Quantidade} de {c.Alvo.Personagem.Simbolo} {c.Alvo.Personagem.Nome} (HP: {c.HPRestante}/{c.Alvo.HPMaximo})");
        }

        /// <summary>Menu de alvo. Retorna o escolhido, ou NULL se Esc (voltar pra seleção de habilidade).</summary>
        public Combate? EscolherAlvoNaTela(List<Combate> alvosDisponiveis, List<Combate> aliados, List<Combate> defensores)
        {
            int idx = 1;
            while (true)
            {
                Console.Clear();
                ExibirPartida(aliados, defensores);
                Console.WriteLine("\nAlvos:");
                for (int i = 0; i < alvosDisponiveis.Count; i++)
                {
                    string cursor = idx == i + 1 ? "▶" : " ";
                    Console.WriteLine($"{cursor} {i + 1} - {alvosDisponiveis[i].Personagem.Simbolo} {alvosDisponiveis[i].Personagem.Nome} | HP:{alvosDisponiveis[i].HPAtual} ATK:{alvosDisponiveis[i].Ataque} DEF:{alvosDisponiveis[i].Defesa}");
                }
                Console.WriteLine("\nEsc - Voltar");

                Comando cmd = _entrada.Ler();
                if (cmd.Tipo == TipoComando.Confirmar) return alvosDisponiveis[idx - 1];
                if (cmd.Tipo == TipoComando.Cancelar) return null;   // voltar pra seleção de habilidade

                idx = Navegacao.MoverCursor(idx, 1, alvosDisponiveis.Count, cmd);
            }
        }

        /// <summary>
        /// Exibe a mensagem de preparação de ataque do inimigo.
        /// </summary>
        public void ExibirPreparacaoAtaque(Combate atacante, List<Combate> defensores)
        {
            Console.Clear();
            ExibirPartida(defensores, new List<Combate>());
            Console.WriteLine($"\n{atacante.Personagem.Simbolo} {atacante.Personagem.Nome} prepara o ataque!");
        }

        /// <summary>
        /// Exibe a mensagem de uso de habilidade.
        /// </summary>
        public void ExibirUsoHabilidade(Combate atacante, Habilidade hab)
        {
            Console.WriteLine($"{atacante.Personagem.Simbolo} usou {hab.Nome}!");
        }

        /// <summary>
        /// Exibe a mensagem de uma passiva (sobreviveu ou morreu).
        /// </summary>
        public void ExibirMensagemPassiva(string mensagem)
        {
            if (!string.IsNullOrEmpty(mensagem))
                Console.WriteLine(mensagem);
        }

        /// <summary>
        /// Diálogo "Encerrar a batalha?" (Esc no meio da luta). Auto-contido: renderiza + lê o input.
        /// Retorna true se o jogador confirmou o encerramento (aí a batalha vira derrota). Esc aqui =
        /// desistir de encerrar (volta pra luta).
        /// </summary>
        public bool ConfirmarEncerramento()
        {
            int opcao = 1; // 1 = Sim, 2 = Não
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Encerrar a batalha? Você perde a fase.\n");
                Console.WriteLine(opcao == 1 ? "▶ 1 - Sim" : "  1 - Sim");
                Console.WriteLine(opcao == 2 ? "▶ 2 - Não" : "  2 - Não");

                Comando cmd = _entrada.Ler();
                if (cmd.Tipo == TipoComando.Confirmar) return opcao == 1;
                if (cmd.Tipo == TipoComando.Cancelar) return false;   // Esc no confirm = não encerra
                opcao = Navegacao.MoverCursor(opcao, 1, 2, cmd);
            }
        }
    }
}
