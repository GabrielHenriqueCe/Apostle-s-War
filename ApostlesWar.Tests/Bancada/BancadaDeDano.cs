using System.Text;
using ApostlesWar.Application.Controllers;
using ApostlesWar.Application.Services;
using ApostlesWar.Domain;

namespace Tests.Bancada
{
    /// <summary>
    /// A BANCADA DE DANO — o instrumento do REBALANCE (#16). Roda os 36 campeões contra um boneco de
    /// pancada padronizado e escreve <c>docs/bancada-dano.md</c>, que é VERSIONADO: cada tweak de
    /// número vira um `git diff` legível, que é a entrega de verdade (não "ajustar valores").
    ///
    /// **O desenho é do Gabriel, e cada escolha tem um porquê:**
    /// - **Stats iguais pra todos** — a bancada mede a HABILIDADE, não o personagem. Se um champ tem
    ///   que ser mais forte, isso se resolve no ataque DELE, depois.
    /// - **Crítico 100%** — quem força crítico para de ganhar vantagem artificial, e o RNG do crítico
    ///   morre de quebra.
    /// - **100 turnos por habilidade** — é o que separa "cooldown baixo demais" de "dano forte demais".
    /// - **Cinco linhas, variando UM fator por vez.** É o que torna as subtrações legíveis: se o
    ///   isolado rodasse imune e o combinado não, o delta misturaria sinergia com malefício e não
    ///   daria pra saber de quem é o mérito.
    ///
    /// **Limitação declarada:** o boneco NÃO revida (ataque 0). Então contra-ataque, espinhos e
    /// revide (Herói, Operário, Zumbi) medem zero aqui — isso é uma bancada de DANO CAUSADO, não de
    /// duelo. Medir kit reativo pede outro instrumento.
    /// </summary>
    public class BancadaDeDano
    {
        private const int Turnos = 100;
        private const int Repeticoes = 10;      // média sobre o RNG que sobra (chances, paralisia)
        // HP do boneco REALISTA (a faixa dos champs), porque o jogo tem efeito percentual sobre o HP
        // máximo: a Queima tira 5% dele por turno. Um boneco inflado faria o DoT explodir. Ele não
        // morre porque o controlador o devolve ao HP cheio antes de cada golpe.
        private const int HPBoneco = 2_000;
        private const int HPChamp = 1_000_000;  // sobrevive ao auto-dano (Fantasma) sem morrer no meio
        private const int AtkPadrao = 200;
        private const int DefNoCap = 1000;      // 1000 de DEF = os 75% de redução, o teto da fórmula

        private sealed record Linha(string Titulo, string Explica, bool PorHab, int DefBoneco, bool Imune);

        private static readonly Linha[] Linhas =
        {
            new("1 — por habilidade · boneco DEF 0 · imune a malefícios",
                "Dano cru. Sem defesa no alvo, quem \"fura defesa\" não distorce a comparação.",
                PorHab: true, DefBoneco: 0, Imune: true),
            new("2 — por habilidade · boneco DEF no cap · imune a malefícios",
                "Mesma coisa com defesa. **(2) − (1) = o que furar/ignorar defesa vale.**",
                PorHab: true, DefBoneco: DefNoCap, Imune: true),
            new("3 — champ inteiro · boneco DEF no cap · imune a malefícios",
                "O champ jogando com o cérebro do bot. **Sinergia = real − esperado**, onde o esperado " +
                "aplica o dano-por-uso da linha 2 às ativações que de fato aconteceram aqui. Positivo = " +
                "as habilidades valem mais juntas do que separadas.",
                PorHab: false, DefBoneco: DefNoCap, Imune: true),
            new("4 — champ inteiro · boneco DEF no cap · RECEBENDO malefícios",
                "O champ completo. **(4) − (3) = o que os malefícios dele valem.**",
                PorHab: false, DefBoneco: DefNoCap, Imune: false),
            new("5 — por habilidade · boneco DEF no cap · RECEBENDO malefícios",
                "**(5) − (2) por habilidade = de quem é o mérito do malefício.** Sem esta linha, o DoT " +
                "de uma habilidade (a Queima do Mago) não aparece em número nenhum por-habilidade.",
                PorHab: true, DefBoneco: DefNoCap, Imune: false),
        };

        /// <summary>
        /// O que a linha 2 guarda de cada habilidade. São DOIS números porque servem a contas
        /// diferentes: o POR USO alimenta o esperado da linha 3 (que precisa do mesmo orçamento de
        /// ATIVAÇÕES) e o TOTAL alimenta o Δ da linha 5 (que compara o mesmo horizonte de 100
        /// turnos). Guardar um só foi bug: o Δ saía comparando total contra dano-por-uso.
        /// </summary>
        private readonly record struct Isolado(int PorUso, int Total);

        private sealed record Medicao(int DanoTotal, int DanoDeTick, int TurnosJogados,
            Dictionary<Habilidade, int> Usos, Dictionary<Habilidade, int> DanoPorHab);

        // ---------- montagem das peças ----------

        /// <summary>
        /// Mesmo champ, stats padronizados. Reaproveita a MESMA lista de `Habilidades` — que é o que
        /// torna a bancada possível sem tocar nos 36 arquivos de champ: a habilidade é DADO, então
        /// varrer é um `foreach`, e as PASSIVAS viajam junto (elas moram na mesma lista, e passiva é
        /// identidade do champ, não algo que se mede isolado).
        /// </summary>
        private static Personagem Normalizar(Personagem original, HabilidadeAtiva espera)
        {
            var habilidades = new List<Habilidade>(original.Habilidades) { espera };
            return new Personagem(original.Slot, original.Faccao, original.Nome, original.Simbolo,
                HPChamp, AtkPadrao, def: 0, habilidades.ToArray());
        }

        private static Personagem Boneco(int defesa, bool imune)
        {
            var habilidades = new List<Habilidade> { new ApostlesWar.Domain.Skills.Ativas.AtaqueBasico() };
            if (imune) habilidades.Add(new ImuneAMaleficios());
            // Ataque 0: ele apanha, nunca bate. Ver a limitação declarada no cabeçalho da classe.
            return new Personagem(1, Faccao.Humanos, "Boneco", "🎯", HPBoneco, 0, defesa, habilidades.ToArray());
        }

        private static Medicao Rodar(Personagem original, HabilidadeAtiva? habIsolada, Linha linha)
        {
            var espera = Espera.Nova();
            var champ = Normalizar(original, espera);
            var boneco = Boneco(linha.DefBoneco, linha.Imune);

            var tela = new TelaDeBancada(critMaximo: true);
            var selecao = new SelecaoDeAlvoService();
            var repo = new SaveEmMemoria();
            var capitulos = new CapitulosService(repo);
            var personagens = new PersonagemService();
            var controlador = new ControladorDeBancada(
                tela, new ControladorBot(selecao), habIsolada, espera, Turnos);

            var combate = new CombateService(
                new ArsenalService(capitulos, repo),
                new CampeoesService(personagens, capitulos),
                personagens, tela, selecao,
                controladorJogador: controlador,
                controladorBot: new ControladorBot(selecao),
                new SemEspera(), new RelogioDoCombate());

            // bot1: false → a equipe1 (o champ) é dirigida pelo controlador da bancada.
            combate.ExecutarArenaComTimes(new List<Personagem> { champ }, new List<Personagem> { boneco },
                bot1: false, bot2: true);

            int total = tela.Bonecos.Count > 0 ? tela.Bonecos[0].DanoRecebido : 0;
            return new Medicao(total, tela.DanoDeTick, Turnos,
                controlador.Usos, tela.DanoPorHab);
        }

        /// <summary>Média sobre N repetições: mata o resto do RNG (chances de aplicar debuff,
        /// paralisia do Medo) que o crítico cravado não cobre.</summary>
        private static Medicao Media(Personagem champ, HabilidadeAtiva? hab, Linha linha)
        {
            var corridas = new List<Medicao>();
            for (int i = 0; i < Repeticoes; i++) corridas.Add(Rodar(champ, hab, linha));

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
                (int)corridas.Average(c => c.DanoDeTick),
                Turnos, usos, dano);
        }

        // ---------- o relatório ----------

        [Fact]
        public void GerarRelatorio()
        {
            var campeoes = new CampeoesService(new PersonagemService(), new CapitulosService(new SaveEmMemoria()));
            var todos = campeoes.TodosOsCampeoes();
            Assert.Equal(36, todos.Count);

            var md = new StringBuilder();
            Cabecalho(md);

            // Guardado pra linha 3 poder subtrair a soma dos isolados da linha 2, e pra linha 5
            // poder subtrair a 2 por habilidade.
            var isoladosDaLinha2 = new Dictionary<string, Dictionary<Habilidade, Isolado>>();

            foreach (var linha in Linhas)
            {
                md.AppendLine($"## Linha {linha.Titulo}").AppendLine();
                md.AppendLine(linha.Explica).AppendLine();

                if (linha.PorHab) PorHabilidade(md, todos, linha, isoladosDaLinha2);
                else PorChamp(md, todos, linha, isoladosDaLinha2);

                md.AppendLine();
            }

            File.WriteAllText(CaminhoDoRelatorio(), md.ToString());
        }

        private static void PorHabilidade(StringBuilder md, List<Personagem> todos, Linha linha,
            Dictionary<string, Dictionary<Habilidade, Isolado>> memoria)
        {
            bool eLinha2 = linha.Titulo.StartsWith("2");
            bool eLinha5 = linha.Titulo.StartsWith("5");

            md.AppendLine(eLinha5
                ? "| Champ | Habilidade | CD | Usos | Dano total | Dano por uso | Tick | Δ vs linha 2 |"
                : "| Champ | Habilidade | CD | Usos | Dano total | Dano por uso |");
            md.AppendLine(eLinha5
                ? "|---|---|--:|--:|--:|--:|--:|--:|"
                : "|---|---|--:|--:|--:|--:|");

            foreach (var champ in todos)
            {
                var ativas = champ.Habilidades.OfType<HabilidadeAtiva>().ToList();
                if (eLinha2) memoria[champ.Nome] = new Dictionary<Habilidade, Isolado>();

                foreach (var hab in ativas)
                {
                    var m = Media(champ, hab, linha);
                    int usos = m.Usos.GetValueOrDefault(hab);
                    int porUso = usos > 0 ? m.DanoTotal / usos : 0;

                    // Guarda o dano POR USO, não o total: a linha 3 precisa comparar com o mesmo
                    // orçamento de turnos, e cada isolado aqui gastou 100 turnos SÓ nesta habilidade.
                    if (eLinha2) memoria[champ.Nome][hab] = new Isolado(porUso, m.DanoTotal);

                    md.Append($"| {champ.Simbolo} {champ.Nome} | {hab.Simbolo} {hab.Nome} | {hab.Cooldown} " +
                              $"| {usos} | {m.DanoTotal} | {porUso} ");

                    if (eLinha5)
                    {
                        int antes = memoria.TryGetValue(champ.Nome, out var mapa)
                            ? mapa.GetValueOrDefault(hab).Total : 0;
                        md.Append($"| {m.DanoDeTick} | {Sinal(m.DanoTotal - antes)} ");
                    }
                    md.AppendLine("|");
                }
            }
        }

        private static void PorChamp(StringBuilder md, List<Personagem> todos, Linha linha,
            Dictionary<string, Dictionary<Habilidade, Isolado>> memoria)
        {
            bool eLinha3 = linha.Titulo.StartsWith("3");

            md.AppendLine(eLinha3
                ? "| Champ | Dano total | Esperado (isolado × usos) | Sinergia | Habilidades usadas |"
                : "| Champ | Dano total | Tick | Habilidades usadas |");
            md.AppendLine(eLinha3 ? "|---|--:|--:|--:|---|" : "|---|--:|--:|---|");

            foreach (var champ in todos)
            {
                var m = Media(champ, null, linha);
                string usadas = string.Join(", ", champ.Habilidades.OfType<HabilidadeAtiva>()
                    .Select(h => $"{h.Nome} {m.Usos.GetValueOrDefault(h)}×"));

                md.Append($"| {champ.Simbolo} {champ.Nome} | {m.DanoTotal} ");
                if (eLinha3)
                {
                    // Esperado = o que essas MESMAS ativações renderiam se cada habilidade
                    // entregasse o que entrega sozinha. Comparar com a soma dos totais isolados
                    // seria laranja com maçã: lá cada uma teve 100 turnos só pra si.
                    int esperado = 0;
                    if (memoria.TryGetValue(champ.Nome, out var mapa))
                        foreach (var (h, iso) in mapa)
                            esperado += iso.PorUso * m.Usos.GetValueOrDefault(h);
                    md.Append($"| {esperado} | {Sinal(m.DanoTotal - esperado)} ");
                }
                else md.Append($"| {m.DanoDeTick} ");

                md.AppendLine($"| {usadas} |");
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
            md.AppendLine($"- Stats IGUAIS pra todos: HP {HPChamp:N0}, ATK {AtkPadrao}, DEF 0. **Crítico 100%**.");
            md.AppendLine("- Na medição por habilidade, o champ usa **só aquela** e **espera** durante o cooldown");
            md.AppendLine("  (não enche o buraco com A1 — se enchesse, o A1 dominaria e todas ficariam iguais).");
            md.AppendLine("- No champ inteiro, quem decide é o **mesmo `ControladorBot`** da Arena e do modo Auto.");
            md.AppendLine($"- Boneco: HP {HPBoneco:N0}, **ataque 0**, DEF 0 ou {DefNoCap} (o cap de 75% de redução).");
            md.AppendLine("  Ele volta ao HP cheio antes de cada golpe — o HP é REALISTA de propósito, porque a");
            md.AppendLine("  Queima tira 5% do HP máximo por turno e um boneco inflado faria o DoT explodir.").AppendLine();
            md.AppendLine("### O que este relatório NÃO mede").AppendLine();
            md.AppendLine("O boneco **não revida**. Contra-ataque, espinhos e revide (Herói, Operário, Zumbi)");
            md.AppendLine("medem **zero** aqui: isto é uma bancada de dano CAUSADO, não de duelo. Um champ");
            md.AppendLine("com número baixo pode ser reativo, não fraco — confira o kit antes de mexer.").AppendLine();
            md.AppendLine("A coluna **Usos** é diagnóstico do BOT: se uma habilidade dispara 0× no champ");
            md.AppendLine("inteiro mas tem dano alto isolada, o problema está na fila do bot, não no balanço.").AppendLine();
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
