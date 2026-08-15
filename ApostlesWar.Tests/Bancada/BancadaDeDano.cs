using System.Text;
using ApostlesWar.Application.Controllers;
using ApostlesWar.Application.Services;
using ApostlesWar.Domain;

namespace Tests.Bancada
{
    /// <summary>
    /// A BANCADA DE DANO — o instrumento do REBALANCE (#16). Roda os 36 apóstolos contra um boneco de
    /// pancada padronizado e escreve <c>docs/bancada-dano.md</c>, que é VERSIONADO: cada tweak de
    /// número vira um `git diff` legível, que é a entrega de verdade (não "ajustar valores").
    ///
    /// **O desenho é do Gabriel, e cada escolha tem um porquê:**
    /// - **Stats iguais pra todos** — a bancada mede a HABILIDADE, não o personagem. Se um apóstolo tem
    ///   que ser mais forte, isso se resolve no ataque DELE, depois.
    /// - **Crítico 100%** — quem força crítico para de ganhar vantagem artificial, e o RNG do crítico
    ///   morre de quebra.
    /// - **100 turnos por habilidade** — é o que separa "cooldown baixo demais" de "dano forte demais".
    /// - **Cinco linhas, variando UM fator por vez.** É o que torna as subtrações legíveis: se o
    ///   isolado rodasse imune e o combinado não, o delta misturaria sinergia com malefício e não
    ///   daria pra saber de quem é o mérito.
    ///
    /// **Limitação declarada:** o boneco NUNCA AGE. Não basta ataque 0 — um golpe de dano zero ainda
    /// dispara `IReageAoSerAtacado`, e a bancada passaria a medir a passiva reagindo ao próprio
    /// andaime (o Troll terminava 25% mais forte porque a Ambição contava as pancadas do saco).
    /// Consequência: contra-ataque, espinhos, revide e passivas de apanhar medem ZERO aqui. É uma
    /// bancada de DANO CAUSADO, não de duelo — kit reativo pede outro instrumento.
    /// </summary>
    public class BancadaDeDano
    {
        private const int Turnos = 100;
        private const int Repeticoes = 10;      // média sobre o RNG que sobra (chances, paralisia)
        // HP REALISTA e IGUAL pros dois lados (a faixa dos apóstolos). É condição, não conveniência: o
        // jogo tem efeito percentual sobre o HP máximo nas DUAS pontas — a Queima tira 5% dele por
        // turno, e cura costuma ser % do HP máximo do alvo. Inflar qualquer um dos dois faz o número
        // correspondente explodir e a comparação perder o sentido.
        private const int HPPadrao = 2_000;
        private const int AtkPadrao = 200;
        private const int DefNoCap = 1000;      // 1000 de DEF = os 75% de redução, o teto da fórmula
        private const int BonecosEmArea = 4;    // a coluna que dá voz às habilidades de área

        private sealed record Linha(string Titulo, string Explica, bool PorHab, int DefBoneco, bool Imune);

        private static readonly Linha[] Linhas =
        {
            new("1 — por habilidade · boneco DEF 0 · imune a malefícios",
                "Dano cru. Sem defesa no alvo, quem \"fura defesa\" não distorce a comparação.",
                PorHab: true, DefBoneco: 0, Imune: true),
            new("2 — por habilidade · boneco DEF no cap · imune a malefícios",
                "Mesma coisa com defesa. **(2) − (1) = o que furar/ignorar defesa vale.**",
                PorHab: true, DefBoneco: DefNoCap, Imune: true),
            new("3 — apóstolo inteiro · boneco DEF no cap · imune a malefícios",
                "O apóstolo jogando com o cérebro do bot. **Sinergia = real − esperado**, onde o esperado " +
                "aplica o dano-por-uso da linha 2 às ativações que de fato aconteceram aqui. Positivo = " +
                "as habilidades valem mais juntas do que separadas.",
                PorHab: false, DefBoneco: DefNoCap, Imune: true),
            new("4 — apóstolo inteiro · boneco DEF no cap · RECEBENDO malefícios",
                "O apóstolo completo. **(4) − (3) = o que os malefícios dele valem.** A Sinergia aqui sai " +
                "do MESMO esperado da linha 2 que a linha 3 usa — é o que mantém as duas colunas " +
                "comparáveis, já que entre elas varia só a imunidade. Então **sinergia(4) − sinergia(3) " +
                "= a sinergia que passa por malefício**: raspar DEF (`ReduçãoDefesa`, −30% sobre um " +
                "boneco no cap) infla o golpe DIRETO e some do `Tick`, e a linha 3 não consegue " +
                "enxergar isso porque o boneco dela é imune.",
                PorHab: false, DefBoneco: DefNoCap, Imune: false),
            new("5 — por habilidade · boneco DEF no cap · RECEBENDO malefícios",
                "**(5) − (2) por habilidade = de quem é o mérito do malefício.** Sem esta linha, o DoT " +
                "de uma habilidade (a Queima do Mago) não aparece em número nenhum por-habilidade.",
                PorHab: true, DefBoneco: DefNoCap, Imune: false),
        };

        /// <summary>
        /// O que a linha 2 guarda de cada habilidade. São TRÊS números porque servem a contas
        /// diferentes: o POR USO alimenta o esperado da linha 3 (que precisa do mesmo orçamento de
        /// ATIVAÇÕES) e o TOTAL alimenta o Δ da linha 5 (que compara o mesmo horizonte de 100
        /// turnos). Guardar um só foi bug: o Δ saía comparando total contra dano-por-uso.
        ///
        /// O POR USO EM ÁREA é o terceiro porque a sinergia de 1 alvo SUBESTIMA o apóstolo de área — e
        /// não por pouco: o Detetive raspa DEF em área e martela em área, então os malefícios dele
        /// valem +9.504 contra 1 boneco e +30.888 contra 4, 3,2× mais. Medir isso não custa corrida
        /// nova: a corrida de área já roda no `PorHabilidade`, só era jogada fora depois de imprimir.
        /// </summary>
        private readonly record struct Isolado(int PorUso, int Total, int PorUsoArea);

        private sealed record Medicao(int DanoTotal, int CuraTotal, int DanoDeTick, int TurnosJogados,
            Dictionary<Habilidade, int> Usos, Dictionary<Habilidade, int> DanoPorHab);

        // ---------- montagem das peças ----------

        /// <summary>
        /// Mesmo apóstolo, stats padronizados. Reaproveita a MESMA lista de `Habilidades` — que é o que
        /// torna a bancada possível sem tocar nos 36 arquivos de apóstolo: a habilidade é DADO, então
        /// varrer é um `foreach`, e as PASSIVAS viajam junto (elas moram na mesma lista, e passiva é
        /// identidade do apóstolo, não algo que se mede isolado).
        /// </summary>
        private static Personagem Normalizar(Personagem original, HabilidadeAtiva espera)
        {
            // O `NuncaMorre` entra por ÚLTIMO de propósito: o `ConfirmarMorte` usa a primeira
            // IPrevineMorte disponível, então o Guarda Real de quem o tem continua respondendo antes.
            var habilidades = new List<Habilidade>(original.Habilidades) { espera, new NuncaMorre() };
            return new Personagem(original.Slot, original.Faccao, original.Nome, original.Simbolo,
                HPPadrao, AtkPadrao, def: 0, habilidades.ToArray());
        }

        private static Personagem Boneco(int defesa, bool imune, HabilidadeAtiva espera)
        {
            var habilidades = new List<Habilidade>
            {
                espera,             // ele NUNCA age — ver ControladorQueEspera
                new NuncaMorre(),   // sobrevive a habilidades que matam DENTRO de uma ativação
            };
            if (imune) habilidades.Add(new ImuneAMaleficios());
            return new Personagem(1, Faccao.Humanos, "Boneco", "🎯", HPPadrao, 0, defesa, habilidades.ToArray());
        }

        private static Medicao Rodar(Personagem original, HabilidadeAtiva? habIsolada, Linha linha,
            int quantosBonecos)
        {
            var espera = Espera.Nova();
            var esperaDoBoneco = Espera.Descanso();
            var apostolo = Normalizar(original, espera);
            var bonecos = Enumerable.Range(0, quantosBonecos)
                .Select(_ => Boneco(linha.DefBoneco, linha.Imune, esperaDoBoneco))
                .ToList();

            var tela = new TelaDeBancada(critMaximo: true);
            var selecao = new SelecaoDeAlvoService();
            var repo = new SaveEmMemoria();
            var capitulos = new CapitulosService(repo);
            var personagens = new PersonagemService();
            var controlador = new ControladorDeBancada(
                tela, new ControladorBot(selecao), habIsolada, espera, Turnos);

            var combate = new CombateService(
                new ArsenalService(capitulos, repo),
                new ApostolosService(personagens, capitulos),
                personagens, tela, selecao,
                controladorJogador: controlador,
                // Só o BONECO cai neste slot: o cérebro do bot que joga pelo apóstolo na medição de
                // apóstolo-inteiro é uma instância própria, dentro do ControladorDeBancada.
                controladorBot: new ControladorQueEspera(esperaDoBoneco),
                new SemEspera(), new RelogioDoCombate());

            // bot1: false → a equipe1 (o apóstolo) é dirigida pelo controlador da bancada.
            combate.ExecutarArenaComTimes(new List<Personagem> { apostolo }, bonecos,
                bot1: false, bot2: true);

            // Soma os bonecos TODOS: com 4 no campo, é o que dá voz à habilidade de área.
            int dano = tela.Bonecos.Sum(b => b.DanoRecebido);
            return new Medicao(dano, tela.CuraPorHab.Values.Sum(), tela.DanoDeTick, Turnos,
                controlador.Usos, tela.DanoPorHab);
        }

        /// <summary>Média sobre N repetições: mata o resto do RNG (chances de aplicar debuff,
        /// paralisia do Medo) que o crítico cravado não cobre.</summary>
        private static Medicao Media(Personagem apostolo, HabilidadeAtiva? hab, Linha linha,
            int quantosBonecos = 1)
        {
            var corridas = new List<Medicao>();
            for (int i = 0; i < Repeticoes; i++) corridas.Add(Rodar(apostolo, hab, linha, quantosBonecos));

            var usos = new Dictionary<Habilidade, int>();
            var dano = new Dictionary<Habilidade, int>();
            foreach (var c in corridas)
            {
                foreach (var (h, n) in c.Usos) usos[h] = usos.GetValueOrDefault(h) + n;
                foreach (var (h, d) in c.DanoPorHab) dano[h] = dano.GetValueOrDefault(h) + d;
            }
            foreach (var h in usos.Keys.ToList()) usos[h] /= Repeticoes;
            foreach (var h in dano.Keys.ToList()) dano[h] /= Repeticoes;

            return new Medicao(
                (int)corridas.Average(c => c.DanoTotal),
                (int)corridas.Average(c => c.CuraTotal),
                (int)corridas.Average(c => c.DanoDeTick),
                Turnos, usos, dano);
        }

        // ---------- o relatório ----------

        [Fact]
        public void GerarRelatorio()
        {
            var apostolos = new ApostolosService(new PersonagemService(), new CapitulosService(new SaveEmMemoria()));
            var todos = apostolos.TodosOsApostolos();
            Assert.Equal(36, todos.Count);

            var md = new StringBuilder();
            Cabecalho(md);

            // Guardado pra linha 3 poder subtrair a soma dos isolados da linha 2, e pra linha 5
            // poder subtrair a 2 por habilidade.
            var isoladosDaLinha2 = new Dictionary<string, Dictionary<Habilidade, Isolado>>();

            var paraRanking = new List<Registro>();

            foreach (var linha in Linhas)
            {
                md.AppendLine($"## Linha {linha.Titulo}").AppendLine();
                md.AppendLine(linha.Explica).AppendLine();

                if (linha.PorHab)
                    PorHabilidade(md, todos, linha, isoladosDaLinha2,
                        coletar: linha.Titulo.StartsWith("1") ? paraRanking : null);
                else PorApostolo(md, todos, linha, isoladosDaLinha2);

                md.AppendLine();
            }

            Rankings(md, paraRanking);

            File.WriteAllText(CaminhoDoRelatorio(), md.ToString());
        }

        /// <summary>Uma linha da tabela, guardada também pros rankings do fim do relatório.</summary>
        private sealed record Registro(string Apostolo, string Hab, int Cooldown, int Usos,
            int Dano, int DanoPorUso, int DanoEmArea, int Cura);

        private static void PorHabilidade(StringBuilder md, List<Personagem> todos, Linha linha,
            Dictionary<string, Dictionary<Habilidade, Isolado>> memoria, List<Registro>? coletar)
        {
            bool eLinha2 = linha.Titulo.StartsWith("2");
            bool eLinha5 = linha.Titulo.StartsWith("5");

            md.AppendLine(eLinha5
                ? $"| Apóstolo | Habilidade | CD | Usos | Dano | Dano/uso | Dano ({BonecosEmArea} alvos) | Cura | Tick | Δ vs linha 2 |"
                : $"| Apóstolo | Habilidade | CD | Usos | Dano | Dano/uso | Dano ({BonecosEmArea} alvos) | Cura |");
            md.AppendLine(eLinha5
                ? "|---|---|--:|--:|--:|--:|--:|--:|--:|--:|"
                : "|---|---|--:|--:|--:|--:|--:|--:|");

            foreach (var apostolo in todos)
            {
                var ativas = apostolo.Habilidades.OfType<HabilidadeAtiva>().ToList();
                if (eLinha2) memoria[apostolo.Nome] = new Dictionary<Habilidade, Isolado>();

                foreach (var hab in ativas)
                {
                    var m = Media(apostolo, hab, linha);
                    var area = Media(apostolo, hab, linha, BonecosEmArea);
                    int usos = m.Usos.GetValueOrDefault(hab);
                    int porUso = usos > 0 ? m.DanoTotal / usos : 0;
                    int usosArea = area.Usos.GetValueOrDefault(hab);
                    int porUsoArea = usosArea > 0 ? area.DanoTotal / usosArea : 0;

                    // Guarda o dano POR USO, não o total: a linha 3 precisa comparar com o mesmo
                    // orçamento de turnos, e cada isolado aqui gastou 100 turnos SÓ nesta habilidade.
                    if (eLinha2) memoria[apostolo.Nome][hab] = new Isolado(porUso, m.DanoTotal, porUsoArea);

                    coletar?.Add(new Registro($"{apostolo.Simbolo} {apostolo.Nome}", $"{hab.Simbolo} {hab.Nome}",
                        hab.Cooldown, usos, m.DanoTotal, porUso, area.DanoTotal, m.CuraTotal));

                    md.Append($"| {apostolo.Simbolo} {apostolo.Nome} | {hab.Simbolo} {hab.Nome} | {hab.Cooldown} " +
                              $"| {usos} | {m.DanoTotal} | {porUso} | {area.DanoTotal} | {m.CuraTotal} ");

                    if (eLinha5)
                    {
                        int antes = memoria.TryGetValue(apostolo.Nome, out var mapa)
                            ? mapa.GetValueOrDefault(hab).Total : 0;
                        md.Append($"| {m.DanoDeTick} | {Sinal(m.DanoTotal - antes)} ");
                    }
                    md.AppendLine("|");
                }
            }
        }

        /// <summary>
        /// Os rankings. A tabela agrupada por apóstolo acima serve pra ler UM personagem inteiro; esta
        /// serve pra achar o outlier sem ter que varrer 144 linhas com o olho. Mesmos dados, duas
        /// perguntas diferentes — por isso as duas vistas convivem em vez de uma substituir a outra.
        /// </summary>
        private static void Rankings(StringBuilder md, List<Registro> regs)
        {
            md.AppendLine("## Rankings (condições da linha 1: DEF 0, alvo imune)").AppendLine();
            md.AppendLine("A tabela por apóstolo acima responde \"como é o kit deste personagem?\".");
            md.AppendLine("Estas respondem \"quem está fora da curva?\".").AppendLine();

            Ranque(md, "Dano por uso — o BURST", regs.Where(r => r.DanoPorUso > 0)
                .OrderByDescending(r => r.DanoPorUso), r => r.DanoPorUso);

            Ranque(md, $"Dano em {Turnos} turnos, {BonecosEmArea} alvos — o SUSTENTADO com área",
                regs.Where(r => r.DanoEmArea > 0).OrderByDescending(r => r.DanoEmArea), r => r.DanoEmArea);

            Ranque(md, $"Cura em {Turnos} turnos", regs.Where(r => r.Cura > 0)
                .OrderByDescending(r => r.Cura), r => r.Cura);
        }

        private static void Ranque(StringBuilder md, string titulo, IEnumerable<Registro> ordenados,
            Func<Registro, int> valor)
        {
            var lista = ordenados.ToList();
            md.AppendLine($"### {titulo}").AppendLine();
            md.AppendLine("| # | Apóstolo | Habilidade | CD | Valor |");
            md.AppendLine("|--:|---|---|--:|--:|");
            for (int i = 0; i < lista.Count; i++)
                md.AppendLine($"| {i + 1} | {lista[i].Apostolo} | {lista[i].Hab} | {lista[i].Cooldown} | {valor(lista[i])} |");
            md.AppendLine();
        }

        /// <summary>
        /// As duas linhas de apóstolo inteiro. Elas têm as MESMAS colunas de propósito: entre a 3 e a 4
        /// varia só a imunidade do boneco, então toda coluna que sobrevive às duas vira uma subtração
        /// legível — e a Sinergia é a que mais rende, porque o esperado das duas sai da MESMA memória
        /// (a da linha 2, imune). Baseline diferente por linha faria as duas colunas medirem coisas
        /// diferentes e a comparação entre elas morreria.
        /// </summary>
        private static void PorApostolo(StringBuilder md, List<Personagem> todos, Linha linha,
            Dictionary<string, Dictionary<Habilidade, Isolado>> memoria)
        {
            md.AppendLine("| Apóstolo | Dano | Esperado | Sinergia " +
                          $"| Dano ({BonecosEmArea} alvos) | Esperado ({BonecosEmArea}) | Sinergia ({BonecosEmArea}) " +
                          "| Tick | Habilidades usadas |");
            md.AppendLine("|---|--:|--:|--:|--:|--:|--:|--:|---|");

            foreach (var apostolo in todos)
            {
                var m = Media(apostolo, null, linha);
                var area = Media(apostolo, null, linha, BonecosEmArea);
                string usadas = string.Join(", ", apostolo.Habilidades.OfType<HabilidadeAtiva>()
                    .Select(h => $"{h.Nome} {m.Usos.GetValueOrDefault(h)}×"));

                // Esperado = o que essas MESMAS ativações renderiam se cada habilidade entregasse o
                // que entrega sozinha. Comparar com a soma dos totais isolados seria laranja com
                // maçã: lá cada uma teve 100 turnos só pra si.
                // O de área usa os usos da corrida de ÁREA, não os de 1 alvo: com mais gente em campo
                // o bot escolhe outra fila, e cobrar o esperado pela fila errada inventaria sinergia.
                int esperado = 0, esperadoArea = 0;
                if (memoria.TryGetValue(apostolo.Nome, out var mapa))
                    foreach (var (h, iso) in mapa)
                    {
                        esperado += iso.PorUso * m.Usos.GetValueOrDefault(h);
                        esperadoArea += iso.PorUsoArea * area.Usos.GetValueOrDefault(h);
                    }

                md.AppendLine($"| {apostolo.Simbolo} {apostolo.Nome} " +
                              $"| {m.DanoTotal} | {esperado} | {Sinal(m.DanoTotal - esperado)} " +
                              $"| {area.DanoTotal} | {esperadoArea} | {Sinal(area.DanoTotal - esperadoArea)} " +
                              $"| {m.DanoDeTick} | {usadas} |");
            }
        }

        private static string Sinal(int v) => v > 0 ? $"+{v}" : v.ToString();

        private static void Cabecalho(StringBuilder md)
        {
            md.AppendLine("# Bancada de dano").AppendLine();
            md.AppendLine("> **Gerado por `ApostlesWar.Tests/Bancada/BancadaDeDano.cs`.** Não edite à mão —");
            md.AppendLine("> rode `dotnet test` e o arquivo se reescreve. É versionado de propósito: cada tweak");
            md.AppendLine("> de número vira um `git diff` legível.").AppendLine();
            md.AppendLine("## Condições").AppendLine();
            md.AppendLine($"- **{Turnos} turnos** por medição, média de **{Repeticoes} repetições**.");
            md.AppendLine($"- Stats IGUAIS pros dois lados: HP {HPPadrao:N0}, ATK {AtkPadrao}, DEF 0. **Crítico 100%**.");
            md.AppendLine("- **O apóstolo começa cada turno com 1 de vida.** Sem isso a coluna de cura seria toda zero");
            md.AppendLine("  (cura não cura quem está cheio), e é também a condição em que aparece quem fica mais");
            md.AppendLine("  FORTE ferido — a Caveira escala `2.0 − HP%`. Ele não morre: carrega a mesma");
            md.AppendLine("  prevenção-de-morte do boneco, que o segura quando uma habilidade de auto-dano zeraria.");
            md.AppendLine($"- A coluna **Dano ({BonecosEmArea} alvos)** repete a medição com {BonecosEmArea} bonecos no campo — é o que");
            md.AppendLine("  dá voz às habilidades de área, que contra alvo único ficam indistinguíveis de single-target.");
            md.AppendLine("- Na medição por habilidade, o apóstolo usa **só aquela** e **espera** durante o cooldown");
            md.AppendLine("  (não enche o buraco com A1 — se enchesse, o A1 dominaria e todas ficariam iguais).");
            md.AppendLine("- No apóstolo inteiro, quem decide é o **mesmo `ControladorBot`** da Arena e do modo Auto.");
            md.AppendLine($"- Boneco: DEF 0 ou {DefNoCap} (o cap de 75% de redução), e **nunca age** — ele se cura.");
            md.AppendLine("  O HP é REALISTA nos dois lados de propósito: a Queima tira 5% do HP máximo por turno e");
            md.AppendLine("  cura costuma ser % do HP máximo, então inflar qualquer um dos dois estoura o número.");
            md.AppendLine("  Ele volta ao HP cheio entre turnos e **não morre** — usa a prevenção-de-morte do Guarda");
            md.AppendLine("  Real, restaurando tudo e sem cooldown, o que também o salva de habilidades que matam");
            md.AppendLine("  DENTRO de uma ativação (o Porradeiro do Troll dá 6 hits de 480 num alvo de 2.000).");
            md.AppendLine("- **CINCO LINHAS OSCILAM A CADA EXECUÇÃO, E ISSO É COMPORTAMENTO — não ruído a consertar,");
            md.AppendLine("  não regressão.** São 🔫 Tiroteio (Policial) · 🤺 Esgrima (Guarda) · 🌟 Shuriken (Ninja) ·");
            md.AppendLine("  🥊 Porradeiro (Troll) · 👿 Vilania (Vilão): as habilidades de `TipoAlvo.Aleatorio` que");
            md.AppendLine("  causam DANO. (As outras duas de alvo aleatório do jogo — 🍭 Doces de Abóbora e 🛸 Abduzir —");
            md.AppendLine("  não dão dano, então não aparecem aqui.) O desvio fica na casa de **±0,5%**. Duas causas,");
            md.AppendLine("  as duas de desenho:");
            md.AppendLine($"  **(1) sortear alvo virou sortear MULTIPLICADOR.** Com {BonecosEmArea} bonecos as casas 1 a 4 existem e");
            md.AppendLine("  o perfil de distância paga diferente em cada uma. Antes dele os bonecos eram");
            md.AppendLine($"  intercambiáveis e o sorteio não mudava número nenhum — por isso isto atinge a coluna de");
            md.AppendLine($"  **{BonecosEmArea} alvos**, e só ela, nas cinco.");
            md.AppendLine("  **(2) encadeamento condicional por CRÍTICO**, e este é só do 🥷 Ninja: a Shuriken carrega");
            md.AppendLine("  `ignorarDefesaPctSeAnteriorCritico`, o 2º hit depende do dado do 1º, e por isso ele é o");
            md.AppendLine("  único que também oscila na coluna de **1 alvo**.");
            md.AppendLine("  **A conclusão, e ela não se reabre: o relatório varia porque a BATALHA varia.** Semear o");
            md.AppendLine("  RNG deixaria o arquivo quieto escondendo justamente o que ele mede. **O que É sinal:**");
            md.AppendLine("  qualquer OUTRO apóstolo mudando, ou uma destas cinco mudando muito além de 1%.").AppendLine();
            md.AppendLine("### O que este relatório NÃO mede").AppendLine();
            md.AppendLine("O boneco **não revida**. Contra-ataque, espinhos e revide (Herói, Operário, Zumbi)");
            md.AppendLine("medem **zero** aqui: isto é uma bancada de dano CAUSADO, não de duelo. Um apóstolo");
            md.AppendLine("com número baixo pode ser reativo, não fraco — confira o kit antes de mexer.").AppendLine();
            md.AppendLine("A coluna **Usos** é diagnóstico do BOT: se uma habilidade dispara 0× no apóstolo");
            md.AppendLine("inteiro mas tem dano alto isolada, o problema está na fila do bot, não no balanço.").AppendLine();
            md.AppendLine("Nas linhas de apóstolo inteiro, **`Habilidades usadas` descreve a corrida de 1 alvo**. A de");
            md.AppendLine($"{BonecosEmArea} alvos é uma simulação à parte — o bot escolhe outra fila com mais gente em campo —");
            md.AppendLine($"e o `Esperado ({BonecosEmArea})` é cobrado pelos usos DELA, senão a sinergia sairia inventada.").AppendLine();
            md.AppendLine($"**Por que a `Sinergia ({BonecosEmArea})` existe:** a de 1 alvo subestima o apóstolo de área. Quem raspa");
            md.AppendLine("DEF em área e martela em área colhe o malefício vezes o número de alvos — a diferença entre");
            md.AppendLine("as duas colunas é o tamanho real desse composto.").AppendLine();
            md.AppendLine($"**⚠️ Parte da `Sinergia ({BonecosEmArea})` é GEOMETRIA, não composição.** Com {BonecosEmArea} bonecos em campo as");
            md.AppendLine("casas 1 a 4 existem, e o perfil de distância multiplica o golpe (o apóstolo é Combatente, então");
            md.AppendLine("×1,00 na casa 1 e ×1,30 na 4 — média 1,15 quando o golpe pega todo mundo). A coluna de **1 alvo**");
            md.AppendLine("é imune a isso: lá os dois estão na casa 1, distância 1, ×1,00. Compare apóstolos entre si na");
            md.AppendLine("mesma coluna e o fator se cancela; ler a razão entre as duas colunas como composição, não.").AppendLine();
            md.AppendLine("---").AppendLine();
        }

        /// <summary>Sobe do bin/ até a raiz do repo (onde mora o `docs/`).</summary>
        private static string CaminhoDoRelatorio()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "docs"))) dir = dir.Parent;
            Assert.NotNull(dir);
            return Path.Combine(dir!.FullName, "docs", "bancada-dano.md");
        }
    }
}
