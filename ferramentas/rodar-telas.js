// Harness das TELAS. O irmão do rodar-tema.js, e existe pelo mesmo motivo: separar arquivo sem
// verificador é mover no escuro.
//
// O que ele faz: carrega o front inteiro num navegador de mentira e DIRIGE o front pelo caminho de
// verdade — publica no `chrome.webview` a mesma mensagem que o C# publicaria, uma por tela, e vê se
// alguma explode. Depois confere que todo `getElementById` citado no JS existe no index.html.
//
// Por que isso pega o que importa numa separação de arquivos:
//   · import faltando em ES module é erro de CARGA — o boot sozinho já derruba.
//   · função que ficou pra trás vira ReferenceError na hora em que a tela é montada.
//   · id trocado no meio do movimento vira tela em branco sem erro; daí a conferência de ids.
//
// Uso:  node --experimental-vm-modules ferramentas/rodar-telas.js
'use strict';

const fs = require('fs');
const path = require('path');
const vm = require('vm');

const WWW = path.resolve(__dirname, '../ApostlesWar.Presentation/wwwroot');
const ENTRADA = path.join(WWW, 'jogo.js');
const INDEX = path.join(WWW, 'index.html');

const problemas = [];
const queixar = (m) => { if (!problemas.includes(m)) problemas.push(m); };

// ---------- DOM de mentira ----------
// Só o suficiente pra montar tela: o que interessa aqui não é o pixel, é a chamada não explodir.
const LARGURA = 1500, ALTURA = 940;
const idsPedidos = new Set();

function criarElemento(id = '', tag = 'DIV') {
    const filhos = [];
    const el = {
        id, tagName: tag, hidden: false, textContent: '', value: '', className: '', title: '',
        width: LARGURA, height: 700, clientWidth: LARGURA, clientHeight: 700,
        offsetWidth: LARGURA, offsetHeight: 700, scrollTop: 0, scrollHeight: 0, scrollLeft: 0,
        readOnly: false, disabled: false, checked: false, dataset: {},
        style: new Proxy({ setProperty() {}, removeProperty() {}, getPropertyValue: () => '' },
            { set: (t, k, v) => (t[k] = v, true), get: (t, k) => (k in t ? t[k] : '') }),
        classList: {
            _s: new Set(),
            add(...c) { c.forEach(x => this._s.add(x)); }, remove(...c) { c.forEach(x => this._s.delete(x)); },
            toggle(c, f) { const v = f ?? !this._s.has(c); v ? this._s.add(c) : this._s.delete(c); return v; },
            contains(c) { return this._s.has(c); },
        },
        addEventListener() {}, removeEventListener() {}, focus() {}, blur() {}, select() {}, click() {},
        setAttribute(k, v) { if (k.startsWith('data-')) el.dataset[k.slice(5)] = v; },
        removeAttribute(k) { if (k === 'data-tema') delete el.dataset.tema; },
        getAttribute() { return null; }, hasAttribute() { return false; },
        appendChild(c) { filhos.push(c); return c; },
        append(...c) { filhos.push(...c); }, prepend(...c) { filhos.unshift(...c); },
        replaceChildren(...c) { filhos.length = 0; filhos.push(...c); },
        insertBefore(c) { filhos.push(c); return c; }, remove() {},
        // Devolve elemento em vez de null: o alvo e o CAMINHO de montagem rodar inteiro, e null
        // faria a tela parar na primeira consulta em vez de mostrar o que quebra mais adiante.
        querySelector: (sel) => criarElemento(sel), querySelectorAll: () => [], closest: () => null,
        getBoundingClientRect: () => ({ left: 0, top: 0, right: LARGURA, bottom: ALTURA, width: LARGURA, height: ALTURA, x: 0, y: 0 }),
        getContext: () => ctxDeMentira(),
        scrollTo() {}, scrollIntoView() {},
        get children() { return filhos; },
        get firstChild() { return filhos[0] ?? null; },
        get lastChild() { return filhos[filhos.length - 1] ?? null; },
        get parentElement() { return null; },
    };
    return el;
}

const ctxDeMentira = () => new Proxy({
    save() {}, restore() {},
    createLinearGradient: () => ({ addColorStop() {} }),
    createRadialGradient: () => ({ addColorStop() {} }),
    createPattern: () => ({}), measureText: () => ({ width: 10 }),
    setLineDash() {}, getLineDash: () => [],
}, { get: (t, p) => (p in t ? t[p] : () => {}), set: (t, p, v) => (t[p] = v, true) });

const porId = new Map();
const document = {
    body: criarElemento('body', 'BODY'),
    documentElement: criarElemento('html', 'HTML'),
    getElementById(id) {
        idsPedidos.add(id);
        if (!porId.has(id)) porId.set(id, criarElemento(id));
        return porId.get(id);
    },
    createElement: (tag) => criarElemento('', String(tag).toUpperCase()),
    createElementNS: (ns, tag) => criarElemento('', String(tag).toUpperCase()),
    createDocumentFragment: () => criarElemento(),
    querySelector: (sel) => criarElemento(sel), querySelectorAll: () => [],
    addEventListener() {}, removeEventListener() {},
};

// ---------- a ponte ----------
// Guardamos o ouvinte que o front registra: é por ele que as telas entram.
let ouvinte = null;
const enviados = [];

const janela = {
    innerWidth: LARGURA, innerHeight: ALTURA, devicePixelRatio: 1,
    requestAnimationFrame() { return 1; }, cancelAnimationFrame() {},
    performance: { now: () => 0 },
    getComputedStyle: () => ({ getPropertyValue: () => '' }),
    addEventListener() {}, removeEventListener() {},
    setTimeout: () => 0, clearTimeout() {}, setInterval: () => 0, clearInterval() {},
    chrome: { webview: {
        postMessage: (m) => enviados.push(m),
        addEventListener: (tipo, fn) => { if (tipo === 'message') ouvinte = fn; },
        removeEventListener() {},
    } },
    console,
};
janela.window = janela; janela.document = document; janela.self = janela;

// ---------- carregar ----------
const contexto = vm.createContext(janela);

// DUAS FASES, e a razão é o grafo em DIAMANTE: `jogo.js` importa `nucleo/cena.js` e `nucleo/ar.js`,
// e o `cena.js` também importa o `ar.js`. Ligando recursivamente, o segundo pedido pelo `ar.js`
// devolvia um módulo ainda no meio da própria ligação — "can not be resolved on module that is not
// linked". Criar TODOS primeiro e ligar a raiz uma vez só resolve, e é o que o `import` real faz.
const cache = new Map();

function criar(arquivo) {
    const abs = path.resolve(arquivo);
    if (cache.has(abs)) return cache.get(abs);
    const fonte = fs.readFileSync(abs, 'utf8');
    const m = new vm.SourceTextModule(fonte, { identifier: abs, context: contexto });
    cache.set(abs, m);
    for (const imp of fonte.matchAll(/^\s*import\s[^'"]*['"]([^'"]+)['"]/gm)) {
        criar(path.resolve(path.dirname(abs), imp[1]));
    }
    return m;
}

async function carregar(arquivo) {
    const raiz = criar(arquivo);
    await raiz.link((spec, ref) => cache.get(path.resolve(path.dirname(ref.identifier), spec)));
    return raiz;
}

// ---------- as telas, uma mensagem cada ----------
// As cargas são MÍNIMAS de propósito: o alvo é o caminho de montagem, não o conteúdo. Se a tela lê
// um campo que não veio, isso aparece como exceção — que é exatamente o que se quer saber.
const champ = { id: 1, nome: 'Teste', simbolo: '🙂', faccao: 'Reino', hp: 100, hpMax: 100, ataque: 10, defesa: 10, taxaCrit: 15, danoCrit: 60, status: [], habilidades: [], liberado: true, vivo: true };
const TELAS = [
    ['menu', { titulo: 'Apostle\'s War', subtitulo: '', opcoes: ['Jogar', 'Sair'], raiz: true, perfil: { nome: 'G', avatar: '🧭' } }],
    ['criarPerfil', {}],
    ['edicaoPerfil', { nome: 'G', avatar: '🧭', campeoes: [champ] }],
    ['montagemArena', { campeoes: [champ, { ...champ, id: 2 }] }],
    ['campanhaMapa', { capitulos: [{ nome: 'Reino', simbolo: '👑', liberado: true, fases: 7 }], atual: 0 }],
    ['campanhaFases', { faccao: 'Reino', simbolo: '👑', fases: [{ numero: 1, nome: 'Arma', liberada: true, rodada1: [champ], rodada2: [champ], item: null }], campeoes: [champ], faseSelecionada: 1, time: [], meusCampeoes: [champ] }],
    ['fimDeFase', { venceu: true, titulo: 'Vitória', faccao: 'Reino', fase: 1, recompensa: null, temProxima: false, comOpcoes: true }],
    ['conquista', champ],
    ['arsenal', { slots: [], itens: [], totais: [] }],
    ['compendio', { faccoes: [{ nome: 'Reino', simbolo: '👑', champs: [champ] }] }],
    ['compendioChamp', champ],
    ['estado', { turno: 1, fase: 'Assistindo', mensagem: '', equipe1: [champ], equipe2: [{ ...champ, id: 9 }], quemAge: null, habilidades: [], alvosValidos: [], selecionado: null, auto: false, modo: 'Campanha', tema: 'reino' }],
    ['evento', { tipo: 'dano', alvoId: 1, valor: 10, critico: false, absorvidoPeloEscudo: 0, texto: null }],
];

(async () => {
    let mod;
    try {
        mod = await carregar(ENTRADA);
        await mod.evaluate();
    } catch (e) {
        console.error(`\n  FALHOU AO CARREGAR o front:\n  ${e.stack}\n`);
        process.exit(1);
    }
    console.log(`\n  front carregado · ${enviados.length} mensagem(ns) de boot enviadas ao C#`);

    if (typeof ouvinte !== 'function') {
        console.error('\n  o front não registrou ouvinte de mensagem — o harness não tem por onde entrar.\n');
        process.exit(1);
    }

    console.log(`\n  ${TELAS.length} telas:`);
    for (const [tipo, conteudo] of TELAS) {
        try {
            ouvinte({ data: JSON.stringify({ tipo, conteudo }) });
            console.log(`  ✓ ${tipo}`);
        } catch (e) {
            console.log(`  ✗ ${tipo} — ${e.message}`);
            queixar(`${tipo}: ${e.stack.split('\n').slice(0, 3).join(' | ')}`);
        }
    }

    // ---------- os ids ----------
    // Estatico, varrendo o FONTE de todos os .js — e nao so os ids que esta corrida pediu. A
    // primeira versao conferia so os exercitados, e por isso deixou passar um id trocado numa tela
    // que so abre por clique: cobertura de corrida nao e cobertura de codigo.
    const html = fs.readFileSync(INDEX, 'utf8');
    // O `[^-\w]` antes do `id=` é o que separa `id="x"` de `data-id="x"` e de `aria-labelledby`.
    const idsNoHtml = new Set([...html.matchAll(/(?:^|[^-\w])id="([^"]+)"/g)].map(m => m[1]));

    const fontes = [];
    (function varrer(dir) {
        for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
            const p = path.join(dir, e.name);
            if (e.isDirectory()) varrer(p);
            else if (e.name.endsWith('.js')) fontes.push(p);
        }
    })(WWW);

    const idsNoCodigo = new Set();
    for (const f of fontes) {
        const src = fs.readFileSync(f, 'utf8');
        for (const m of src.matchAll(/getElementById\(\s*['"`]([^'"`]+)['"`]/g)) idsNoCodigo.add(m[1]);
    }

    const faltando = [...idsNoCodigo].filter(i => !idsNoHtml.has(i));
    console.log(`\n  ids: ${idsNoCodigo.size} citados no código (${idsPedidos.size} exercitados nesta corrida)`
        + ` · ${faltando.length} sem elemento no index.html`);
    for (const f of faltando) queixar(`getElementById("${f}") não existe no index.html`);
    if (problemas.length) {
        console.log('\n  ---- problemas ----');
        for (const p of problemas) console.log('  ' + p);
        console.log('\n  RESULTADO: FALHOU\n');
        process.exit(1);
    }
    console.log('\n  RESULTADO: todas as telas montam\n');
})();
