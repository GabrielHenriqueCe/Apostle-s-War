// Separa os blocos de tema do estilo.css em cenarios/<tema>/<tema>.css — e PROVA que a cascata não
// mudou.
//
// A armadilha que justifica a prova: no arquivo único, a escada de `@media (max-height:...)` que
// encolhe os ladrilhos vem DEPOIS do bloco base de cada tema. Arquivos de tema carregados após o
// estilo.css invertem a posição deles em relação a todo o CSS base que hoje vem depois da região de
// temas (~490 linhas). Onde a ESPECIFICIDADE decide, ordem é irrelevante; onde ela EMPATA, a ordem é
// quem decide — e aí a inversão muda o resultado EM SILÊNCIO.
//
// Por isso o script confere EMPATES antes de mover: para cada propriedade declarada por regra de
// tema, procura regra base que venha depois da região de temas, declare a MESMA propriedade e tenha
// especificidade IGUAL. Zero empates = separar é provadamente inofensivo.
//
// Uso:  node ferramentas/separar-css.js [--conferir]
'use strict';
const fs = require('fs');
const path = require('path');

const WWW = path.resolve(__dirname, '../ApostlesWar.Presentation/wwwroot');
const CSS = path.join(WWW, 'estilo.css');
const TEMAS = ['reino', 'ladosombrio', 'tecnologicos', 'folclore', 'misticos', 'especial', 'decaidos', 'apostolos'];
const SO_CONFERIR = process.argv.includes('--conferir');

// ---------- especificidade ----------
// A primeira versão desta função tinha uma ALTERNATIVA VAZIA na regex das classes (um `|` sobrando
// no fim), e alternativa vazia casa em TODA posição do texto: a contagem virava o comprimento da
// string, dois seletores nunca empatavam, e o verificador respondia "0 empates" pra qualquer entrada.
// Verificador quebrado é pior que verificador nenhum — ele dá PERMISSÃO.
function especificidade(sel) {
    let s = ' ' + sel.trim();
    let b = 0, c = 0;

    s = s.replace(/::[\w-]+/g, () => { c++; return ' '; });                    // pseudo-elemento = tipo
    s = s.replace(/:(?:not|is|where)\(([^)]*)\)/g, (_, d) => ' ' + d + ' ');   // não contam por si

    const a = (s.match(/#[\w-]+/g) || []).length;
    s = s.replace(/#[\w-]+/g, ' ');
    b += (s.match(/\.[\w-]+/g) || []).length;
    s = s.replace(/\.[\w-]+/g, ' ');
    b += (s.match(/\[[^\]]*\]/g) || []).length;
    s = s.replace(/\[[^\]]*\]/g, ' ');
    b += (s.match(/:[\w-]+(\([^)]*\))?/g) || []).length;
    s = s.replace(/:[\w-]+(\([^)]*\))?/g, ' ');
    c += (s.match(/(?:^|[\s>+~(,])[a-zA-Z][\w-]*/g) || []).length;

    return a * 10000 + b * 100 + c;
}

// Prova da função no ARRANQUE. Se ela mentir, tudo abaixo mente junto — e a 1ª versão mentia.
for (const [sel, esperado] of [
    ['#arena', 10000],
    ['body[data-tema="reino"]', 101],
    ['body[data-tema="reino"] #arena', 10101],
    ['.combatente', 100],
    ['body[data-tema="reino"] .combatente', 201],
    ['.lado > .combatente', 200],
]) {
    const teve = especificidade(sel);
    if (teve !== esperado) {
        console.error(`  especificidade("${sel}") = ${teve}, esperado ${esperado} — verificador não confiável.`);
        process.exit(1);
    }
}

const bruto = fs.readFileSync(CSS, 'utf8');
const NL = bruto.includes('\r\n') ? '\r\n' : '\n';
const linhas = bruto.split(/\r?\n/);

// ---------- fatiar em blocos de topo ----------
const blocos = [];
let i = 0;
while (i < linhas.length) {
    if (!linhas[i].trim()) { i++; continue; }
    const inicio = i;
    let prof = 0, viuChave = false;
    while (i < linhas.length) {
        for (const ch of linhas[i]) { if (ch === '{') { prof++; viuChave = true; } else if (ch === '}') prof--; }
        i++;
        if (viuChave && prof <= 0) break;
    }
    blocos.push({ inicio, fim: i - 1, texto: linhas.slice(inicio, i).join(NL) });
}

const temaDoTexto = (txt) => TEMAS.filter(t => txt.includes(`data-tema="${t}"`));

// ---------- classificar ----------
const porTema = Object.fromEntries(TEMAS.map(t => [t, []]));
const restante = [];
let primeiroTema = Infinity;

for (const b of blocos) {
    const ts = temaDoTexto(b.texto);
    if (!ts.length) { restante.push(b); continue; }
    primeiroTema = Math.min(primeiroTema, b.inicio);

    if (ts.length === 1) { porTema[ts[0]].push(b.texto); continue; }

    // @media com vários temas dentro vira um @media por tema, com as linhas daquele tema.
    //
    // A ABERTURA é a linha do `@media`, não a primeira linha do bloco: um bloco carrega junto o
    // comentário que vem antes dele, e usar `linhas[0]` cego trocava o `@media (max-height: 1000px)`
    // pela primeira linha de um comentário de 20 linhas. O resultado compilava como CSS e era
    // MUDO: as regras ficavam dentro de um comentário nunca fechado, e o primeiro degrau da escada
    // simplesmente não existia — o ladrilho parava de encolher e nada acusava.
    const linhasB = b.texto.split(NL);
    const iAbre = linhasB.findIndex(l => /^\s*@/.test(l));
    if (iAbre === -1) { console.error(`  bloco multi-tema sem @regra na linha ${b.inicio + 1}`); process.exit(1); }
    const prefacio = linhasB.slice(0, iAbre);        // o comentário que explica a escada
    const abre = linhasB[iAbre];
    const fecha = linhasB[linhasB.length - 1];

    for (const t of ts) {
        const minhas = linhasB.slice(iAbre + 1, -1).filter(l => l.includes(`data-tema="${t}"`));
        porTema[t].push([...prefacio, abre, ...minhas, fecha].join(NL));
    }
    const orfas = linhasB.slice(iAbre + 1, -1).filter(l => l.trim() && !temaDoTexto(l).length);
    if (orfas.length) restante.push({ ...b, texto: [...prefacio, abre, ...orfas, fecha].join(NL) });
}

// ---------- conferir empates ----------
function regrasDe(texto, deLinha) {
    const semComent = texto.replace(/\/\*[\s\S]*?\*\//g, ' ');
    const regras = [];
    for (const m of semComent.matchAll(/([^{}@]+)\{([^{}]*)\}/g)) {
        const sel = m[1].trim().replace(/\s+/g, ' ');
        if (!sel || sel.startsWith('@')) continue;
        const props = [...m[2].matchAll(/([-\w]+)\s*:/g)].map(x => x[1]);
        for (const s of sel.split(',')) {
            const t = s.trim();
            if (t) regras.push({ sel: t, props, linha: deLinha, esp: especificidade(t) });
        }
    }
    return regras;
}

const regrasDeTema = [];
const regrasBaseDepois = [];
for (const b of blocos) {
    if (temaDoTexto(b.texto).length) regrasDeTema.push(...regrasDe(b.texto, b.inicio));
    else if (b.inicio > primeiroTema) regrasBaseDepois.push(...regrasDe(b.texto, b.inicio));
}

const empates = [];
for (const rt of regrasDeTema) {
    for (const rb of regrasBaseDepois) {
        if (rb.esp !== rt.esp) continue;              // especificidade decide; ordem é irrelevante
        const comuns = rt.props.filter(p => rb.props.includes(p));
        if (comuns.length) empates.push({ tema: rt.sel, base: rb.sel, props: comuns, linhaBase: rb.linha + 1 });
    }
}

console.log(`\n  estilo.css — ${blocos.length} blocos de topo, ${linhas.length} linhas`);
console.log(`  regras de tema: ${regrasDeTema.length} · regras base DEPOIS da região de temas: ${regrasBaseDepois.length}`);
console.log(`\n  EMPATES de especificidade (onde a ordem decidiria): ${empates.length}`);
for (const e of empates.slice(0, 25)) {
    console.log(`    ${e.tema}  ×  ${e.base} (linha ${e.linhaBase})  →  ${e.props.join(', ')}`);
}
if (empates.length) {
    console.log('\n  Cada um destes precisa de decisão à mão ANTES de separar.\n');
    process.exit(1);
}
console.log('  → nenhum. Mover os temas pra depois do base é provadamente inofensivo.\n');

// ---------- conferir que NADA se perdeu nem se duplicou ----------
// Esta conferência existe porque foi ela que pegou o bug do prefácio: a contagem de regras dava
// 509 antes e 510 depois, e o seletor que sobrava denunciou um `@media` que tinha virado comentário.
// Um recorte de CSS pode ficar sintaticamente válido e semanticamente morto — só a contagem acusa.
{
    const seletores = (txt) => [...txt.replace(/\/\*[\s\S]*?\*\//g, ' ').matchAll(/([^{}@]+)\{([^{}]*)\}/g)]
        .map(m => m[1].trim().replace(/\s+/g, ' '));
    const contar = (arr) => arr.reduce((m, s) => (m[s] = (m[s] || 0) + 1, m), {});

    const antes = contar(seletores(blocos.map(b => b.texto).join(NL)));
    const depois = contar(seletores([...restante.map(b => b.texto), ...Object.values(porTema).flat()].join(NL)));

    const difs = [];
    for (const k of new Set([...Object.keys(antes), ...Object.keys(depois)])) {
        if ((antes[k] || 0) !== (depois[k] || 0)) difs.push(`${JSON.stringify(k)}  antes ${antes[k] || 0}  depois ${depois[k] || 0}`);
    }
    console.log(`  REGRAS: ${Object.values(antes).reduce((a, b) => a + b, 0)} antes · ${Object.values(depois).reduce((a, b) => a + b, 0)} depois · ${difs.length} diferenças`);
    if (difs.length) {
        console.log('\n  A separação PERDEU ou DUPLICOU regra:');
        for (const d of difs.slice(0, 20)) console.log('    ' + d);
        console.log('');
        process.exit(1);
    }
}

if (SO_CONFERIR) process.exit(0);

// ---------- escrever ----------
const CABECA = {
    reino: '👑 REINO — a cidade murada sob cerco, de DIA (o único tema claro).',
    ladosombrio: '🌑 LADO SOMBRIO — o cemitério sob a lua.',
    tecnologicos: '⚙️ TECNOLÓGICOS — a noite da invasão.',
    folclore: '🪬 FOLCLORE — a clareira com a fogueira, na noite quente.',
    misticos: '🐉 MÍSTICOS — a praia no crepúsculo.',
    especial: '⭐ ESPECIAL — o banheiro público, o primeiro INTERIOR.',
    decaidos: '🔱 DECAÍDOS — a vila élfica vendida, com a luz vindo de baixo.',
    apostolos: '✝️ APÓSTOLOS — a sala de Natal, a paisagem vista por um RECORTE.',
};

for (const t of TEMAS) {
    const cab = [
        `/* ${CABECA[t]}`,
        '',
        '   Carregado DEPOIS do estilo.css (ver index.html). A escada de @media do fim deste arquivo é',
        '   a que encolhe o ladrilho em janela baixa, e ela vem depois do bloco base DESTE tema — que é',
        '   a ordem que importa. Os valores dela são px CRUS de propósito: o JS os lê com parseFloat, e',
        '   min()/clamp()/vh voltariam como texto, virariam NaN e cairiam no padrão EM SILÊNCIO. */',
        '',
        '',
    ].join(NL);
    fs.writeFileSync(path.join(WWW, `cenarios/${t}/${t}.css`), cab + porTema[t].join(NL + NL) + NL, 'utf8');
    console.log(`  + cenarios/${t}/${t}.css  (${porTema[t].length} blocos)`);
}

const novasLinhas = [];
let ultimo = -1;
for (const b of restante) {
    if (ultimo !== -1 && b.inicio > ultimo + 1) novasLinhas.push('');
    novasLinhas.push(...b.texto.split(NL));
    ultimo = b.fim;
}
fs.writeFileSync(CSS, novasLinhas.join(NL) + NL, 'utf8');
console.log(`  ~ estilo.css  ${linhas.length} → ${novasLinhas.length} linhas`);

// ---------- os <link> ----------
const idx = path.join(WWW, 'index.html');
let html = fs.readFileSync(idx, 'utf8');
if (!html.includes('cenarios/reino/reino.css')) {
    const links = TEMAS.map(t => `    <link rel="stylesheet" href="cenarios/${t}/${t}.css" />`).join(NL);
    html = html.replace('    <link rel="stylesheet" href="estilo.css" />',
        '    <link rel="stylesheet" href="estilo.css" />' + NL +
        '    <!-- Os temas vêm DEPOIS do base: é essa ordem que a separação preservou (ver separar-css.js). -->' + NL +
        links);
    fs.writeFileSync(idx, html, 'utf8');
    console.log('  ~ index.html  (8 <link> de tema)');
}
