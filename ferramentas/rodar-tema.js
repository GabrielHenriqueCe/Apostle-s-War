// Harness headless dos temas. Carrega o jogo.js INTEIRO num navegador de mentira, chama o
// `aplicarTema` de VERDADE e roda N segundos por tema com dt fixo.
//
// Por que a fiação e não só as camadas: uma bancada que monta os builders na mão não vê exceção
// dentro do `iniciarAr` — e foi exatamente assim que a colisão de chave `arvore` (Decaídos × Natal)
// deixou a cena em branco sem erro visível. Aqui o caminho é o mesmo que o jogo percorre.
//
// Uso:  node ferramentas/rodar-tema.js [entrada.js] [segundos]
//       node ferramentas/rodar-tema.js            (usa o wwwroot/jogo.js, 120s por tema)
'use strict';

const fs = require('fs');
const path = require('path');
const vm = require('vm');

const RAIZ = process.argv[2] || path.resolve(__dirname, '../ApostlesWar.Presentation/wwwroot/jogo.js');
const SEGUNDOS = Number(process.argv[3] || 120);
const DT = 1 / 60;

// ---------- as queixas ----------
const problemas = [];
let temaCorrente = '';
let quadroCorrente = 0;
const queixar = (msg) => {
    const chave = `${temaCorrente} · ${msg}`;
    if (!problemas.some(p => p.chave === chave)) {
        problemas.push({ chave, tema: temaCorrente, msg, quadro: quadroCorrente });
    }
};

// ---------- o ctx de mentira, que VALIDA ----------
// Não desenha: confere. Todo argumento numérico tem de ser finito; raio de arc/ellipse não pode ser
// negativo (isso LANÇA no canvas real e mata o requestAnimationFrame — a cena congela em silêncio);
// e NaN em coordenada NÃO lança no canvas real, só não desenha, que é pior porque é mudo.
const RAIO = { arc: [2], arcTo: [4], ellipse: [2, 3], createRadialGradient: [2, 5], roundRect: [4] };

function criarCtx(nome) {
    const estado = { profundidade: 0, maxProfundidade: 0 };

    const conferirArgs = (metodo, args) => {
        args.forEach((a, i) => {
            if (typeof a === 'number' && !Number.isFinite(a)) {
                queixar(`${nome}.${metodo}() recebeu ${Number.isNaN(a) ? 'NaN' : a} no argumento ${i}`);
            }
        });
        for (const i of RAIO[metodo] || []) {
            if (typeof args[i] === 'number' && args[i] < 0) {
                queixar(`${nome}.${metodo}() recebeu RAIO NEGATIVO (${args[i]}) no argumento ${i} — isso lança no canvas real`);
            }
        }
    };

    const gradiente = () => ({ addColorStop: (p, c) => {
        if (typeof p === 'number' && !Number.isFinite(p)) queixar(`addColorStop com offset ${p}`);
        if (typeof c === 'string' && /NaN|undefined/.test(c)) queixar(`addColorStop com cor inválida: ${c}`);
    } });

    const alvo = {
        canvas: null,
        save() { estado.profundidade++; estado.maxProfundidade = Math.max(estado.maxProfundidade, estado.profundidade); },
        restore() {
            estado.profundidade--;
            if (estado.profundidade < 0) { queixar(`${nome}: restore() a mais — profundidade ficou negativa`); estado.profundidade = 0; }
        },
        createLinearGradient: gradiente,
        createRadialGradient: gradiente,
        createConicGradient: gradiente,
        createPattern: () => ({}),
        measureText: (t) => ({ width: String(t ?? '').length * 6 }),
        getImageData: (x, y, w, h) => ({ data: new Uint8ClampedArray(Math.max(1, (w | 0) * (h | 0) * 4)) }),
        putImageData: () => {},
        setLineDash: () => {},
        getLineDash: () => [],
        isPointInPath: () => false,
        _estado: estado,
    };

    return new Proxy(alvo, {
        get(t, prop) {
            if (prop in t) return t[prop];
            if (typeof prop === 'symbol') return undefined;
            // Qualquer outro método de canvas: aceita, confere os argumentos, não desenha.
            return (...args) => { conferirArgs(String(prop), args); };
        },
        set(t, prop, valor) {
            if (typeof valor === 'number' && !Number.isFinite(valor)) queixar(`${nome}.${String(prop)} = ${valor}`);
            if (typeof valor === 'string' && /NaN|undefined/.test(valor)) queixar(`${nome}.${String(prop)} = "${valor}"`);
            t[prop] = valor;
            return true;
        },
    });
}

// ---------- as variáveis CSS de verdade ----------
// O `medirDoTema` lê px crus do CSS com parseFloat. Servir os valores REAIS faz o harness medir o
// que o jogo mede; sem isto tudo cairia no padrão e as âncoras ficariam no lugar errado.
function lerVariaveisDoCss() {
    const raiz = path.dirname(RAIZ);
    // TODOS os .css, não só o estilo.css: depois da separação as variáveis de ladrilho de cada tema
    // moram em cenarios/<tema>/<tema>.css, e ler só o base faria tudo cair no valor padrão — o
    // harness mediria uma cena que não é a que o jogo monta.
    const arquivos = [];
    (function varrer(dir) {
        for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
            const p = path.join(dir, e.name);
            if (e.isDirectory()) varrer(p);
            else if (e.name.endsWith('.css')) arquivos.push(p);
        }
    })(raiz);

    const vars = {};
    for (const f of arquivos) {
        const css = fs.readFileSync(f, 'utf8');
        for (const m of css.matchAll(/(--[\w-]+)\s*:\s*([^;}]+)[;}]/g)) vars[m[1]] = m[2].trim();
    }
    return vars;
}
const VARS_CSS = lerVariaveisDoCss();

// ---------- o DOM de mentira ----------
const LARGURA = 1500, ALTURA = 940;

function criarElemento(id = '') {
    const filhos = [];
    const el = {
        id, tagName: 'DIV', hidden: false, textContent: '', value: '', className: '', title: '',
        width: LARGURA, height: ALTURA - 190,
        clientWidth: LARGURA, clientHeight: ALTURA - 190,
        offsetWidth: LARGURA, offsetHeight: ALTURA - 190,
        scrollTop: 0, scrollHeight: 0, readOnly: false, disabled: false,
        dataset: {}, style: new Proxy({}, { set: (t, k, v) => (t[k] = v, true) }),
        classList: {
            _s: new Set(),
            add(...c) { c.forEach(x => this._s.add(x)); }, remove(...c) { c.forEach(x => this._s.delete(x)); },
            toggle(c, f) { const v = f ?? !this._s.has(c); v ? this._s.add(c) : this._s.delete(c); return v; },
            contains(c) { return this._s.has(c); },
        },
        addEventListener() {}, removeEventListener() {}, focus() {}, blur() {}, select() {}, click() {},
        setAttribute(k, v) { if (k.startsWith('data-')) el.dataset[k.slice(5)] = v; },
        removeAttribute(k) { if (k === 'data-tema') delete el.dataset.tema; },
        getAttribute() { return null; },
        appendChild(c) { filhos.push(c); return c; },
        append(...c) { filhos.push(...c); }, prepend(...c) { filhos.unshift(...c); },
        replaceChildren(...c) { filhos.length = 0; filhos.push(...c); },
        remove() {}, insertBefore(c) { filhos.push(c); return c; },
        querySelector() { return null; }, querySelectorAll() { return []; },
        closest() { return null; },
        getBoundingClientRect: () => ({ left: 0, top: 0, right: LARGURA, bottom: ALTURA, width: LARGURA, height: ALTURA, x: 0, y: 0 }),
        getContext: (tipo) => { const c = criarCtx(`${id || 'canvas'}.${tipo}`); c.canvas = el; return c; },
        get children() { return filhos; },
        get firstChild() { return filhos[0] ?? null; },
        get lastChild() { return filhos[filhos.length - 1] ?? null; },
    };
    return el;
}

const porId = new Map();
const document = {
    body: criarElemento('body'),
    documentElement: criarElemento('html'),
    getElementById(id) {
        if (!porId.has(id)) porId.set(id, criarElemento(id));
        return porId.get(id);
    },
    createElement: (tag) => { const e = criarElemento(); e.tagName = String(tag).toUpperCase(); return e; },
    createElementNS: (ns, tag) => document.createElement(tag),
    createDocumentFragment: () => criarElemento(),
    querySelector: () => null,
    querySelectorAll: () => [],
    addEventListener() {}, removeEventListener() {},
};

// ---------- o relógio de mentira ----------
// O rAF NÃO roda sozinho: guardamos o callback e o `avancar()` é quem o chama, com dt fixo. Assim o
// harness controla o tempo e 120s de cena custam menos de um segundo de CPU.
let proximoQuadro = null;
let idQuadro = 0;
let agora = 0;

const janela = {
    innerWidth: LARGURA, innerHeight: ALTURA,
    devicePixelRatio: 1,
    requestAnimationFrame(cb) { proximoQuadro = cb; return ++idQuadro; },
    cancelAnimationFrame() { proximoQuadro = null; },
    performance: { now: () => agora },
    getComputedStyle: () => ({ getPropertyValue: (n) => VARS_CSS[n] ?? '' }),
    addEventListener() {}, removeEventListener() {},
    setTimeout: (fn) => { try { fn(); } catch (e) { queixar(`setTimeout lançou: ${e.message}`); } return 0; },
    clearTimeout() {}, setInterval: () => 0, clearInterval() {},
    // A ponte com o C#: só engole o que a tela mandaria.
    chrome: { webview: { postMessage() {}, addEventListener() {}, removeEventListener() {} } },
    console,
};
janela.window = janela;
janela.document = document;
janela.self = janela;
janela.globalThis = janela;

// ---------- carregar o jogo.js de verdade ----------
const contexto = vm.createContext(janela);
const fonte = fs.readFileSync(RAIZ, 'utf8');

// DOIS MODOS, porque o front está no meio da separação: hoje é um script clássico, amanhã é uma
// árvore de ES modules. O harness detecta qual e entra pelo caminho certo — se ele só soubesse ler
// o monólito, morreria justamente no PR que ele existe pra vigiar.
const EH_MODULO = /^\s*(import|export)\s/m.test(fonte);

// No modo SCRIPT, `const` de topo cria binding de script e não propriedade do global — o harness não
// enxergaria nem o AR_DO_TEMA nem o aplicarTema. Este epílogo é a única linha acrescentada ao
// arquivo, e ele só LÊ o que já existe.
const EPILOGO = `
;globalThis.__exporta = {
    CENARIOS: typeof CENARIOS !== 'undefined' ? CENARIOS : (typeof AR_DO_TEMA !== 'undefined' ? AR_DO_TEMA : null),
    aplicarTema: typeof aplicarTema !== 'undefined' ? aplicarTema : null,
};`;

// DUAS FASES por causa do grafo em DIAMANTE: `jogo.js` importa `nucleo/cena.js` e `nucleo/ar.js`, e
// o `cena.js` também importa o `ar.js`. Ligando recursivamente, o segundo pedido pelo `ar.js`
// devolvia um módulo ainda no meio da própria ligação. Criar TODOS primeiro e ligar a raiz uma vez
// só é o que o `import` de verdade faz.
const cacheMod = new Map();

function criarModulo(arquivo) {
    const abs = path.resolve(arquivo);
    if (cacheMod.has(abs)) return cacheMod.get(abs);
    const fonte = fs.readFileSync(abs, 'utf8');
    const m = new vm.SourceTextModule(fonte, { identifier: abs, context: contexto });
    cacheMod.set(abs, m);
    for (const imp of fonte.matchAll(/^\s*import\s[^'"]*['"]([^'"]+)['"]/gm)) {
        criarModulo(path.resolve(path.dirname(abs), imp[1]));
    }
    return m;
}

async function carregarModulo(arquivo) {
    const raiz = criarModulo(arquivo);
    await raiz.link((spec, ref) => cacheMod.get(path.resolve(path.dirname(ref.identifier), spec)));
    return raiz;
}

async function carregar() {
    if (!EH_MODULO) {
        vm.runInContext(fonte + EPILOGO, contexto, { filename: path.basename(RAIZ) });
        return contexto.__exporta ?? {};
    }
    if (typeof vm.SourceTextModule !== 'function') {
        console.error('\n  O front virou ES module: rode com  node --experimental-vm-modules\n');
        process.exit(1);
    }
    const m = await carregarModulo(RAIZ);
    await m.evaluate();
    // No modo módulo o entry EXPORTA o que o harness precisa; o namespace já é o contrato.
    const ns = m.namespace;
    return { CENARIOS: ns.CENARIOS ?? ns.AR_DO_TEMA ?? contexto.__exporta?.CENARIOS, aplicarTema: ns.aplicarTema ?? contexto.__exporta?.aplicarTema };
}

async function principal() {
let exporta;
try {
    exporta = await carregar();
} catch (e) {
    console.error(`\n  FALHOU AO CARREGAR ${path.basename(RAIZ)} (${EH_MODULO ? 'módulo' : 'script'}):\n  ${e.stack}\n`);
    process.exit(1);
}

if (typeof exporta.aplicarTema !== 'function') {
    console.error('\n  aplicarTema não foi encontrado — o harness não tem por onde entrar.\n');
    process.exit(1);
}
const temas = Object.keys(exporta.CENARIOS ?? {});
if (temas.length === 0) {
    console.error('\n  o registro de cenários não foi encontrado — o harness não tem o que rodar.\n');
    process.exit(1);
}

// ---------- rodar ----------
const QUADROS = Math.round(SEGUNDOS / DT);
console.log(`\n  ${path.basename(RAIZ)} · ${temas.length} temas · ${SEGUNDOS}s cada (${QUADROS} quadros a dt fixo)\n`);

let falhou = false;
for (const tema of temas) {
    temaCorrente = tema;
    const antes = problemas.length;
    agora = 0;
    proximoQuadro = null;

    // O caminho REAL: aplicarTema → iniciarAr → monta as camadas. É aqui que colisão de chave e
    // config malformada aparecem, e é isto que uma bancada de builders na mão nunca vê.
    // Cada tema entra a partir do NADA (`aplicarTema('')` desliga e cancela o laço), senão o
    // `if (tema === temaAtual) return` engoliria a segunda chamada e o tema não seria montado.
    try {
        exporta.aplicarTema('');
        exporta.aplicarTema(tema);
    } catch (e) {
        console.log(`  ✗ ${tema.padEnd(14)} EXCEÇÃO NA MONTAGEM — a cena não nasce`);
        console.log(`      ${e.stack.split('\n').slice(0, 3).join('\n      ')}\n`);
        falhou = true;
        continue;
    }

    if (!proximoQuadro) {
        console.log(`  ✗ ${tema.padEnd(14)} nenhum requestAnimationFrame agendado — a cena não anima`);
        falhou = true;
        continue;
    }

    let travou = null;
    for (let q = 0; q < QUADROS; q++) {
        quadroCorrente = q;
        agora += DT * 1000;
        const cb = proximoQuadro;
        proximoQuadro = null;
        try { cb(agora); } catch (e) { travou = { q, e }; break; }
        if (!proximoQuadro) { travou = { q, e: new Error('o laço parou de se reagendar') }; break; }
    }

    if (travou) {
        console.log(`  ✗ ${tema.padEnd(14)} LANÇOU no quadro ${travou.q} (${(travou.q * DT).toFixed(1)}s) — a cena congela`);
        console.log(`      ${travou.e.stack.split('\n').slice(0, 3).join('\n      ')}\n`);
        falhou = true;
        continue;
    }

    const novos = problemas.length - antes;
    if (novos > 0) { falhou = true; console.log(`  ✗ ${tema.padEnd(14)} ${novos} problema(s)`); }
    else console.log(`  ✓ ${tema.padEnd(14)} ${QUADROS} quadros limpos`);
}

if (problemas.length) {
    console.log('\n  ---- problemas ----');
    for (const p of problemas) console.log(`  [${p.tema}] quadro ${p.quadro}: ${p.msg}`);
}

console.log(falhou ? '\n  RESULTADO: FALHOU\n' : '\n  RESULTADO: todos os temas limpos\n');
process.exit(falhou ? 1 : 0);
}

principal();
