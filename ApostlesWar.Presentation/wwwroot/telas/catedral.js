// CATEDRAL — onde o apóstolo se aprimora. Quatro colunas: o ELENCO, o escolhido, as PORTAS e a
// estação aberta.
//
// As estações são LUGARES e não verbos — 🎒 Armaria (vestir), ⬆️ Santuário (nível), ★ Altar
// (estrela) e 🔥 Oferenda (fundir alma). A Armaria é o estado de repouso.
//
// A ⚒️ Forja é a única que NÃO abre aqui: ela é tela própria, porque lá o centro é a PEÇA e não o
// apóstolo. A porta dela só acende com um slot escolhido — sem peça não há o que forjar.
//
// Toda ação volta pro C# e a tela inteira é redesenhada. Não há estado de jogo aqui: o que mora
// neste arquivo é só ONDE o jogador estava olhando.

import { mandar } from '../nucleo/ponte.js';
import { marcaDeTipo, marcaDeEstrelas, barraDeNivel } from '../ui/marcas.js';
import { painelDeStats } from '../ui/ficha.js';
import { almaIcone } from '../ui/alma.js';
import { blocoDeDelta } from '../ui/delta.js';
import { cardDePeca } from '../ui/peca.js';
import { navegador } from '../ui/navegador.js';
import { barraDeQuantidade } from '../ui/quantidade.js';
import { filtroLimpo, aplicar, agrupar, painelDeFiltro } from '../ui/filtro.js';

const ARSENAL_AREAS = ['arma', 'elmo', 'escudo', 'acess', 'peito', 'calca', 'bota'];   // slot índice → grid-area
const ARSENAL_ICONES = ['🗡️', '⛑️', '🛡️', '📿', '🎽', '👖', '👢'];   // ícone do tipo quando o slot está vazio

let catedralDados = null;
let catedralSlotSel = -1;
let catedralEstacao = null;   // null = itens à mostra · 'estrela' · 'nivel' · 'almas'

// TROCAR ITEM: a lista de peças toma a coluna do ELENCO, e não uma tela nova. As duas listas nunca
// são úteis ao mesmo tempo — escolhendo arma ninguém está escolhendo apóstolo — e é isso que deixa
// o filtro caber sem estourar o layout, que era o risco de pôr as duas lado a lado.
let catedralTrocando = false;

// O FILTRO da troca. O estado mora aqui porque é preferência de quem olha, não estado de jogo (o C#
// não precisa saber que você escondeu os comuns); a MECÂNICA dele mora em `ui/filtro.js`, porque a
// Forja filtra o mesmo acervo e duas cópias divergiriam.
let filtro = filtroLimpo();

// O que o jogador está MONTANDO na coluna da direita, lido pela ficha do meio. As duas colunas
// mostram partes do mesmo gesto — a direita é o quanto, o meio é o que isso vira — e é este objeto
// que as amarra sem uma saber desenhar a outra.
let previsao = null;   // { nivel, pct, ganho }

export const catedral = {
    cena: 'catedral',
    montar(a, anterior) {
        if (anterior !== 'catedral') {   // entrada fresca
            catedralSlotSel = -1; catedralEstacao = null; catedralTrocando = false;
        }
        catedralDados = a;
        previsao = null;   // toda volta da ponte apaga o que estava sendo montado
        desenharPortas();
        desenharColunaEsquerda();
        desenharFicha();
        desenharBoneco();

        if (catedralSlotSel >= 0) desenharSlot(catedralSlotSel);
        else document.getElementById('armariaDetalhe').hidden = true;

        desenharAcao();
},
};

// ---------- as ESTAÇÕES: as portas da última coluna ----------
//
// Cada uma é um LUGAR e não um verbo — Armaria, Santuário, Altar, Oferenda —, e é isso que faz a
// tela ter nome próprio em vez de ser um painel de ações. A 🎒 Armaria é o estado de repouso.
function desenharPortas() {
    const c = catedralDados.apostolo;
    document.getElementById('catedralPortas').replaceChildren(
        botaoDeAcao('🎒', 'Armaria', null, true),
        botaoDeAcao('⬆️', 'Santuário', 'nivel', !!c?.podeQueimar),
        botaoDeAcao('★', 'Altar', 'estrela', !!c?.naParede),
        botaoDeAcao('🔥', 'Oferenda', 'almas', true),
        botaoDeAcao('💎', 'Raridade', 'raridade', false, 'em breve'));
}

// ---------- esquerda: quem eu estou aprimorando, OU as peças da troca ----------

// A coluna é UMA e o conteúdo dela depende do gesto: escolhendo apóstolo, é o elenco; escolhendo
// peça, é o acervo com o filtro. Nunca as duas.
function desenharColunaEsquerda() {
    const acervo = catedralTrocando && catedralSlotSel >= 0;
    // A classe troca a coluna de GRADE (4 cartas por linha) pra PILHA. É ela que faz o filtro e a
    // lista se empilharem em vez de caírem nas células da grade do elenco.
    document.getElementById('catedralElenco').classList.toggle('modoAcervo', acervo);
    if (acervo) desenharAcervo();
    else desenharRoster();
}

function desenharRoster() {
    document.getElementById('catedralElenco').replaceChildren(...catedralDados.roster.map((c, i) => {
        const cel = document.createElement('button');
        cel.type = 'button';
        cel.className = 'rosterCard' + (i === catedralDados.selecionado ? ' selecionado' : '');

        const em = document.createElement('span'); em.className = 'rcEmoji'; em.textContent = c.simbolo;
        const nm = document.createElement('span'); nm.className = 'rcNome'; nm.textContent = c.nome;

        cel.append(marcaDeTipo(c.tipoSimbolo), marcaDeEstrelas(c.estrelas), em, barraDeNivel(c.nivel, c.xpPct), nm);
        cel.addEventListener('click', () => mandar('selecionarApostolo', i));
        return cel;
    }));
}

// ---------- centro: o apóstolo e o que dá pra comprar nele ----------

function desenharFicha() {
    const alvo = document.getElementById('catedralFicha');
    const c = catedralDados.apostolo;

    if (!c) {
        const v = document.createElement('div');
        v.className = 'catedralVazio';
        v.textContent = 'Nenhum apóstolo conquistado ainda.';
        alvo.replaceChildren(v);
        return;
    }

    const arte = document.createElement('div'); arte.id = 'catedralArte'; arte.textContent = c.ficha.simbolo;

    // O nome COM as setas do elenco — o mesmo gesto que troca o tipo da peça na Forja (ui/navegador.js).
    // Aqui ele é atalho e não necessidade (o elenco está todo à esquerda), mas gesto que só existe em
    // uma tela ninguém aprende. Circula: do último volta pro primeiro.
    const total = catedralDados.roster.length;
    const nome = navegador(c.ficha.nome, {
        ha: total > 1,
        aoAnterior: () => mandar('selecionarApostolo', (catedralDados.selecionado - 1 + total) % total),
        aoProximo: () => mandar('selecionarApostolo', (catedralDados.selecionado + 1) % total),
        classe: 'navApostolo',
    });

    const ident = document.createElement('div');
    ident.className = 'afFaccao';
    ident.textContent = `${c.ficha.faccao} · ${c.ficha.tipoSimbolo} ${c.ficha.tipo}`;

    // O TETO vai junto do nível ("nv 24 / 29"): a parede tem de ser legível ANTES de o jogador bater
    // nela, senão a barra cheia parada parece bug. Montando uma queima, o rótulo vira o DESTINO.
    // ATRAVESSANDO NÍVEL A BARRA ZERA. Ela passa a ser a barra do nível de DESTINO, então o que já
    // estava preenchido é do nível anterior e não tem mais o que dizer ali — deixá-lo embaixo
    // escondia justamente o quanto o novo enche, que é o que o jogador quer ver.
    const p = previsao;
    const virou = p && p.nivel > c.ficha.nivel;

    const nivel = document.createElement('div');
    nivel.className = 'afNivel';
    const numero = document.createElement('span');
    numero.className = 'afNivelNumero' + (c.naParede ? ' travado' : '') + (p ? ' previsto' : '');
    numero.textContent = virou
        ? `nv ${c.ficha.nivel} → ${p.nivel} / ${c.teto}`
        : `nv ${c.ficha.nivel} / ${c.teto}`;
    nivel.append(marcaDeEstrelas(c.estrelas),
        barraDeNivel(virou ? p.nivel : c.ficha.nivel, virou ? 0 : c.ficha.xpPct, p ? p.pct : -1),
        numero);

    const filhos = [arte, nome, ident, nivel];

    if (c.motivo) {
        const m = document.createElement('div');
        m.className = 'afMotivo';
        m.textContent = c.motivo;
        filhos.push(m);
    }

    const alvoDaPrevia = previsao && previsao.nivel > c.ficha.nivel
        ? c.porNivel.find(n => n.nivel === previsao.nivel)
        : null;

    // Aqui a conta abre: é a tela em que se monta o boneco, e ver "200 +58 = 258" é o que diz se a
    // peça que você acabou de equipar valeu. Nas outras telas vai só o total.
    alvo.replaceChildren(...filhos, ...painelDeStats(c.ficha, alvoDaPrevia, true));
}


// `acao` null = mostra os ITENS (o estado de repouso da 3ª coluna). `habilitado` false com ação
// não desabilita: o painel é onde o jogador descobre O QUE falta, e botão morto não ensina nada —
// só o 💎, que ainda não existe, é de fato inerte.
function botaoDeAcao(icone, rotulo, acao, habilitado, nota) {
    const b = document.createElement('button');
    b.type = 'button';
    b.className = 'afBotao' + (catedralEstacao === acao ? ' aberto' : '');
    b.disabled = acao === 'raridade';

    const ic = document.createElement('span'); ic.className = 'abIcone'; ic.textContent = icone;
    const rot = document.createElement('span'); rot.className = 'abRotulo'; rot.textContent = rotulo;
    b.append(ic, rot);

    if (nota) {
        const n = document.createElement('span'); n.className = 'abNota'; n.textContent = nota;
        b.append(n);
    }
    if (!habilitado && !b.disabled) b.classList.add('semSaldo');

    if (!b.disabled) b.addEventListener('click', () => {
        catedralEstacao = acao;
        previsao = null; desenharPortas(); desenharFicha(); desenharAcao();
    });
    return b;
}

// ---------- direita: os itens, ou o painel da ação aberta ----------

function desenharAcao() {
    const painel = document.getElementById('catedralEstacao');
    const direita = document.getElementById('armaria');

    painel.hidden = !catedralEstacao;
    direita.hidden = !!catedralEstacao;
    if (!catedralEstacao) return;

    const c = catedralDados.apostolo;
    const nomes = { estrela: '★ Altar', nivel: '⬆️ Santuário', almas: '🔥 Oferenda' };

    const titulo = document.createElement('h2');
    titulo.className = 'apostoloSecao';
    titulo.textContent = nomes[catedralEstacao];

    const corpo = catedralEstacao === 'estrela' ? painelDaEstrela(c)
        : catedralEstacao === 'nivel' ? painelDoNivel(c)
            : painelDasAlmas();

    painel.replaceChildren(titulo, ...corpo);
}

// A FUSÃO: 10 de uma faixa viram 1 da seguinte, e o botão morre quando a faixa alvo passa do que a
// dificuldade mais alta já aberta derruba. Quem diz esse teto é o C# (`Alma.TetoDeFusao`) — é ele
// que impede fabricar mítico farmando o Fácil.
function painelDasAlmas() {
    // A barra conta GRUPOS de 10, não unidades: é o grupo que vira uma alma da faixa seguinte, e
    // arrastar em unidades deixaria o jogador parar num resto que não produz nada.
    const escolha = catedralDados.alma.map(() => 0);
    const barras = [];

    const resumo = document.createElement('div'); resumo.className = 'queimaResumo';
    const confirmar = document.createElement('button');
    confirmar.type = 'button';
    confirmar.className = 'acaoConfirmar';

    const linhas = catedralDados.alma.map((a, raridade) => {
        const grupos = a.podeFundir ? Math.floor(a.quantidade / 10) : 0;
        const seguinte = catedralDados.alma[raridade + 1];

        // As PONTAS contam a troca: à esquerda o fogo de origem com o que SOBRA dele, à direita o
        // fogo de destino com o que NASCE. Arrastar move os dois números ao mesmo tempo, e é aí que
        // a fusão deixa de precisar de legenda.
        const barra = barraDeQuantidade({
            max: grupos,
            rotuloMax: a.raridade >= catedralDados.tetoDeFusao
                ? 'vença uma dificuldade mais alta pra fundir até aqui'
                : `cada grupo consome 10 de ${a.nome}`,
            aoMudar: (v) => { escolha[raridade] = v; atualizar(); },
            // A ponta ESQUERDA não repete o fogo da linha logo acima — dois iguais empilhados liam
            // como duas coisas. Em cima fica o TOTAL que ele tem; aqui embaixo, só quanto vai
            // queimar. A ponta direita mantém o fogo porque ali a faixa é OUTRA.
            pontas: seguinte ? {
                esquerda: (v) => soNumero(v * 10),
                direita: (v) => fogoContado(seguinte, v, true),
            } : null,
        });
        barras.push(barra);

        const nota = a.raridade >= catedralDados.tetoDeFusao ? '🔒 travado'
            : seguinte ? `10 → 1 ${seguinte.nome}` : 'não sobe mais';
        return linhaDeFaixa(a, nota, barra.el);
    });

    function atualizar() {
        const total = escolha.reduce((s, g) => s + g, 0);
        resumo.textContent = total > 0
            ? `${escolha.reduce((s, g) => s + g * 10, 0)} almas viram ${total}`
            : 'Arraste uma barra pra escolher quantos grupos fundir.';
        confirmar.textContent = total > 0 ? `Fundir (${total})` : 'Fundir';
        confirmar.disabled = total <= 0;
    }

    confirmar.addEventListener('click', () => {
        const faixas = escolha
            .map((grupos, raridade) => ({ raridade, quantidade: grupos * 10 }))
            .filter(f => f.quantidade > 0);
        if (faixas.length) mandar('fundirAlma', 0, JSON.stringify({ faixas }));
    });

    const nota = document.createElement('div');
    nota.className = 'acaoAviso';
    nota.textContent = `A fusão para na faixa ${catedralDados.alma[catedralDados.tetoDeFusao].nome}: `
        + 'é até onde a dificuldade mais alta que você abriu derruba alma.';

    atualizar();
    return [rotulo('Fundir — 10 viram 1'), ...linhas, resumo, confirmar, nota];
}

// Quanto SAI: só o número, porque o fogo dessa faixa já está na linha de cima.
function soNumero(quantidade) {
    const n = document.createElement('span');
    n.className = 'fcQtd';
    n.textContent = `−${Math.max(0, quantidade).toLocaleString('pt-BR')}`;
    return n;
}

// O fogo com o número embaixo — é a ponta da barra da Oferenda.
function fogoContado(a, quantidade, comMais = false) {
    const p = document.createElement('span');
    p.className = 'fogoContado';
    const n = document.createElement('span');
    n.className = 'fcQtd';
    n.textContent = (comMais ? '+' : '') + Math.max(0, quantidade).toLocaleString('pt-BR');
    p.append(almaIcone(a.raridade, 20), n);
    return p;
}

// A linha de UMA faixa: alminha, nome, saldo, uma nota curta e a barra ocupando a largura inteira
// embaixo. As duas ações (queimar e fundir) usam a mesma forma de propósito — é o mesmo gesto.
function linhaDeFaixa(a, nota, barraEl) {
    const linha = document.createElement('div');
    linha.className = 'faixaLinha' + (a.quantidade > 0 ? '' : ' vazia');

    const topo = document.createElement('div'); topo.className = 'flTopo';
    const nome = document.createElement('span'); nome.className = 'flNome'; nome.textContent = a.nome;
    const qtd = document.createElement('span'); qtd.className = 'flQtd';
    qtd.textContent = a.quantidade.toLocaleString('pt-BR');
    const obs = document.createElement('span'); obs.className = 'flNota'; obs.textContent = nota;

    topo.append(almaIcone(a.raridade, 22), nome, qtd, obs);
    linha.append(topo, barraEl);
    return linha;
}

function painelDaEstrela(c) {
    const partes = [];

    // A EXIGÊNCIA DE NÍVEL vem primeiro, e é a que faltava: sem ela o jogador via um preço, juntava
    // a alma e o botão continuava morto sem dizer por quê. A estrela só se compra COM A BARRA CHEIA
    // no teto — juntar alma antes não adianta, e agora a tela diz isso.
    partes.push(rotulo(`★ ${c.estrelas + 1} — encha a barra no nv ${c.teto}`));

    const onde = document.createElement('div');
    onde.className = 'acaoAviso';
    onde.textContent = c.naParede
        ? `O nv ${c.teto} está cheio. A estrela abre até o nv ${c.teto + 10 > 60 ? 60 : c.teto + 10}.`
        : `Ele está no nv ${c.ficha.nivel}. Suba até o nv ${c.teto} e encha a barra pra poder comprar.`;
    partes.push(onde);

    // UMA linha por faixa, no formato `tenho/preciso` — antes eram duas listas (o preço e o que
    // faltava) dizendo a mesma coisa com números diferentes, e o jogador tinha de subtrair de cabeça.
    const receita = document.createElement('div');
    receita.className = 'acaoReceita';
    receita.replaceChildren(...c.receita.map(r => {
        const tenho = (catedralDados.alma[r.raridade] || {}).quantidade ?? 0;
        const linha = linhaDeAlma(r, `${Math.min(tenho, r.quantidade).toLocaleString('pt-BR')}/${r.quantidade.toLocaleString('pt-BR')}`);
        if (tenho < r.quantidade) linha.classList.add('faltando');
        return linha;
    }));
    partes.push(rotulo('Custo'), receita);

    const b = document.createElement('button');
    b.type = 'button';
    b.className = 'acaoConfirmar';
    b.textContent = `Comprar ★ ${c.estrelas + 1}`;
    b.disabled = !c.podeComprarEstrela;
    b.addEventListener('click', () => mandar('comprarEstrela', 0));
    partes.push(b);

    return partes;
}

function painelDoNivel(c) {
    if (!c.podeQueimar) return [aviso(c.motivo || 'Não há nível a subir agora.')];

    // O que está selecionado em cada barrinha, por faixa. Vive só enquanto o painel está aberto.
    const escolha = catedralDados.alma.map(() => 0);

    const resumo = document.createElement('div'); resumo.className = 'queimaResumo';
    const confirmar = document.createElement('button');
    confirmar.type = 'button';
    confirmar.className = 'acaoConfirmar';

    const barras = [];
    const linhas = catedralDados.alma.map((a, raridade) => {
        const barra = barraDeQuantidade({
            max: a.max,
            rotuloMax: a.max < a.quantidade
                ? `você tem ${a.quantidade}, mas só ${a.max} cabem até a parede`
                : 'tudo o que você tem desta faixa',
            aoMudar: (v) => { escolha[raridade] = v; atualizar(); },
        });
        barras.push(barra);
        return linhaDeFaixa(a, `+${a.xpPorUnidade} XP cada`, barra.el);
    });

    // MÁXIMO enche até a parede gastando da faixa MAIS BARATA pra mais cara: se Comum resolve, ele
    // não encosta no Raro — que é justamente a faixa que a próxima estrela vai cobrar.
    const max = document.createElement('button');
    max.type = 'button';
    max.className = 'qlBotao qlMaximo';
    max.textContent = 'Máximo';
    max.addEventListener('click', () => {
        let falta = c.xpAteAParede;
        catedralDados.alma.forEach((a, i) => {
            const q = falta <= 0 ? 0 : Math.min(a.max, Math.ceil(falta / a.xpPorUnidade));
            escolha[i] = q;
            falta -= q * a.xpPorUnidade;
            barras[i].definir(q);
        });
        atualizar();
    });

    function atualizar() {
        const ganho = escolha.reduce((s, q, i) => s + q * catedralDados.alma[i].xpPorUnidade, 0);
        resumo.textContent = ganho > 0
            ? `+${ganho.toLocaleString('pt-BR')} XP`
            : 'Arraste uma barra pra escolher quanto queimar.';
        confirmar.textContent = ganho > 0 ? `Queimar (+${ganho.toLocaleString('pt-BR')} XP)` : 'Queimar';
        confirmar.disabled = ganho <= 0;

        // O RESULTADO é desenhado no MEIO, na ficha: o nível de destino, a barra em brasa e o
        // `x → y` na própria linha do stat. Aqui à direita fica só quanto se está gastando.
        previsao = ganho > 0 ? projetar(c, ganho) : null;
        desenharFicha();
    }

    confirmar.addEventListener('click', () => {
        const faixas = escolha
            .map((quantidade, raridade) => ({ raridade, quantidade }))
            .filter(f => f.quantidade > 0);
        if (faixas.length) mandar('queimarAlma', 0, JSON.stringify({ faixas }));
    });

    atualizar();
    return [rotulo('Queimar alma como XP'), ...linhas, max, resumo, confirmar];
}

// Onde este ganho de XP para: o nível de destino e o quanto a barra DAQUELE nível encheria.
//
// Os dois saem da tabela de limiares que o C# manda — é busca numa lista, não a curva copiada pra
// cá. Se o ganho atravessa nível, o `pct` é o do nível de DESTINO: a barra satura em 100 no
// caminho e quem conta a travessia é o rótulo, e depois a animação do confirmar.
function projetar(c, ganho) {
    const total = c.xpAtual + ganho;
    const nivel = nivelCom(c, ganho);

    const xpDe = (n) => (c.limiares.find(l => l.nivel === n) || {}).xp;
    const piso = xpDe(nivel);
    const teto = xpDe(nivel + 1);
    if (piso == null || teto == null || teto <= piso) return { nivel, pct: 100, ganho };

    return { nivel, pct: Math.max(0, Math.min(100, ((total - piso) / (teto - piso)) * 100)), ganho };
}

// Onde a XP atual + `ganho` cai, procurando na tabela de limiares que o C# mandou. É BUSCA numa
// lista, não a curva copiada pra cá — a curva mora no Progressao e não pode ter segunda cópia.
function nivelCom(c, ganho) {
    const total = c.xpAtual + ganho;
    let nivel = c.ficha.nivel;
    for (const l of c.limiares) if (total >= l.xp && l.nivel <= c.teto) nivel = l.nivel;
    return nivel;
}

function linhaDeAlma(a, valor) {
    const linha = document.createElement('div');
    linha.className = 'almaLinha';
    const n = document.createElement('span'); n.className = 'alNome'; n.textContent = a.nome;
    const v = document.createElement('span'); v.className = 'alValor'; v.textContent = valor;
    linha.append(almaIcone(a.raridade), n, v);
    return linha;
}

function rotulo(texto) {
    const d = document.createElement('div');
    d.className = 'acaoRotulo';
    d.textContent = texto;
    return d;
}

function aviso(texto) {
    const d = document.createElement('div');
    d.className = 'acaoAviso';
    d.textContent = texto;
    return d;
}

// ---------- direita: o boneco e os itens (o que já existia) ----------

function desenharBoneco() {
    document.getElementById('boneco').replaceChildren(...catedralDados.slots.map(s => {
        const div = document.createElement('div');
        div.className = 'bonecoSlot' + (s.slot === catedralSlotSel ? ' selecionado' : '') + (s.equipado ? ' preenchido' : '');
        div.style.gridArea = ARSENAL_AREAS[s.slot];
        const emoji = document.createElement('div'); emoji.className = 'bsEmoji';
        emoji.textContent = s.equipado ? s.equipado.simbolo : ARSENAL_ICONES[s.slot];
        const nome = document.createElement('div'); nome.className = 'bsNome'; nome.textContent = s.nome;
        div.append(emoji, nome);
        // Trocar de slot começa do ZERO: fecha a troca em curso, esquece a peça que estava sendo
        // comparada e limpa o filtro. O filtro é por SLOT — um "principal: Taxa Crítica" vindo da
        // Manopla esvaziaria a lista da Bota, e a tela pareceria vazia sem dizer por quê.
        div.addEventListener('click', () => {
            // O acervo abre JUNTO com o slot: escolher a peça é a razão de clicar aqui, e pedir um
            // segundo clique ("Trocar item") só pra revelar a lista era uma etapa sem decisão dentro.
            // Quem quiser o elenco de volta tem o "← elenco" no topo da coluna.
            if (s.slot !== catedralSlotSel) filtro = filtroLimpo();
            catedralTrocando = true;
            desenharSlot(s.slot);
            desenharColunaEsquerda();
        });
        return div;
    }));
}

// ---------- o ACERVO: a lista de peças do slot, com o filtro ----------

// As peças daquele slot já passadas pelo filtro e pela ordem. A que o SELECIONADO já veste nunca
// entra: ela está desenhada em cima, no painel de comparação, e repetida na lista viraria uma troca
// por si mesma. A que está num ALIADO entra só com o filtro `deAliados` ligado — tomar a peça de um
// aliado é gesto de propósito, não descuido, e por isso se pede por ele.
const doSlotSelecionado = () =>
    catedralDados.obtidos.filter(o => o.slot === catedralSlotSel && !o.equipado);

function desenharAcervo() {
    const col = document.getElementById('catedralElenco');
    const doSlot = doSlotSelecionado();
    const itens = aplicar(doSlot, filtro);

    const topo = document.createElement('div');
    topo.className = 'acervoTopo';
    const volta = document.createElement('button');
    volta.type = 'button'; volta.className = 'acervoVoltar'; volta.textContent = '← elenco';
    volta.addEventListener('click', () => { catedralTrocando = false; mandar('preverItem', -1); });
    const titulo = document.createElement('span');
    titulo.className = 'acervoTitulo';
    titulo.textContent = `${catedralDados.slots[catedralSlotSel].nome} · ${itens.length}`;
    topo.append(volta, titulo);

    const corpo = document.createElement('div');
    corpo.className = 'acervoLista';

    if (!itens.length) {
        const v = document.createElement('div');
        v.className = 'catedralVazio';
        v.textContent = 'Nenhuma peça passa neste filtro.';
        corpo.append(v);
    } else {
        for (const g of agrupar(itens, filtro)) {
            if (g.cabecalho) {
                const cab = document.createElement('div');
                cab.className = 'acervoGrupo';
                cab.textContent = g.cabecalho;
                corpo.append(cab);
            }
            corpo.append(...g.itens.map(cardDeAcervo));
        }
    }

    col.replaceChildren(topo, painelDeFiltro(doSlot, filtro, desenharAcervo), corpo);
}

// O cartão da peça mora em `ui/peca.js`, porque a Forja mostra o MESMO acervo — desenhá-lo duas
// vezes é como as duas telas começam a discordar. Aqui fica só o que é da Catedral: quem está
// marcado é a peça sendo comparada, e o clique abre essa comparação.
const cardDeAcervo = (o) => cardDePeca(o, {
    marcada: !!catedralDados.previa && catedralDados.previa.indice === o.indice,
    aoClicar: () => mandar('preverItem', o.indice),
});


// ---------- o painel do SLOT: a peça de hoje, e a que você está pensando em pôr ----------
//
// Clicar num slot mostra as estatísticas DAQUELA peça e mais nada. A lista inteira de armas com a
// ficha de cada uma era ilegível — a decisão é entre DUAS peças, não entre catorze, e é assim que
// ela se desenha: a de hoje em cima, a candidata embaixo, a diferença entre as duas no meio.
function desenharSlot(slot) {
    catedralSlotSel = slot;
    desenharBoneco();
    document.getElementById('armariaDetalhe').hidden = false;
    document.getElementById('armariaSlotNome').textContent = catedralDados.slots[slot].nome;

    const cont = document.getElementById('armariaItens');
    const equipada = catedralDados.slots[slot].equipado;

    // A candidata só vale se for DESTE slot. Sem esta trava, sair da Arma pro Bracelete mantinha a
    // arma que estava sendo comparada desenhada embaixo do bracelete — a prévia vem do C# e ele não
    // sabe que o jogador mudou de slot, porque mudar de slot não volta pela ponte.
    const previa = catedralDados.previa;
    const candidata = previa
        ? catedralDados.obtidos.find(o => o.indice === previa.indice && o.slot === slot)
        : null;

    const filhos = [];

    filhos.push(equipada
        ? fichaDeItem(equipada, 'equipada')
        : vazio('Nenhuma peça neste slot.'));

    const acoes = document.createElement('div');
    acoes.className = 'itemAcoes';

    // Não há mais botão pra ABRIR a lista — ela já está aberta à esquerda desde o clique no slot. O
    // único botão de troca é o que TROCA, e ele nasce lá embaixo, junto da peça escolhida.

    // A porta da FORJA, e ela só existe com peça no slot: forjar o vazio não quer dizer nada. O
    // índice viaja porque é a PEÇA que abre a tela, não o slot — lá dentro dá pra pular pras outras
    // do mesmo slot, inclusive as guardadas.
    if (equipada) {
        const melhorar = document.createElement('button');
        melhorar.type = 'button';
        melhorar.className = 'trocarItem melhorarItem';
        melhorar.textContent = '⚒️ Melhorar';
        melhorar.addEventListener('click', () => mandar('abrirForja', equipada.indice));

        // TIRAR a peça. Ela volta pro baú — sacrificar peça é assunto da Forja, e um botão que
        // apagasse equipamento no meio da armaria seria a mesma tecla pra dois destinos diferentes.
        // Manda o SLOT, não o índice no acervo: o que se esvazia é o slot.
        const remover = document.createElement('button');
        remover.type = 'button';
        remover.className = 'trocarItem';
        remover.textContent = '✕ Remover';
        remover.addEventListener('click', () => mandar('desequiparItem', slot));

        acoes.append(melhorar, remover);
    }

    // Slot vazio não tem ação nenhuma agora que o "Trocar item" saiu — e uma fileira vazia abre um
    // buraco entre a ficha e a candidata.
    if (acoes.childElementCount > 0) filhos.push(acoes);

    // A candidata entra ABAIXO da equipada, no mesmo lugar onde a lista morava. As duas fichas lado
    // a lado na vertical são a comparação — o delta é o que sobra de olhar as duas.
    if (candidata) {
        filhos.push(fichaDeItem(candidata, 'candidata'), blocoDeDelta(previa.deltas));

        // A ÚNICA tecla da troca, e ela troca de fato. "Equipar" quando o slot está vazio: não há o
        // que trocar por nada.
        const equipar = document.createElement('button');
        equipar.type = 'button';
        equipar.className = 'acaoConfirmar';
        equipar.textContent = equipada ? 'Trocar' : 'Equipar';
        equipar.addEventListener('click', () => mandar('equiparItem', candidata.indice));
        filhos.push(equipar);
    }

    cont.replaceChildren(...filhos);
}

function vazio(texto) {
    const v = document.createElement('div');
    v.className = 'catedralVazio';
    v.textContent = texto;
    return v;
}

// A ficha de UMA peça: quem ela é, o principal e o eixo do nível (estrela + trilho).
function fichaDeItem(o, papel) {
    const card = document.createElement('div');
    card.className = 'itemFicha ' + papel;

    const tag = document.createElement('div');
    tag.className = 'ifTag';
    tag.textContent = papel === 'equipada' ? 'equipada' : 'candidata';

    const topo = document.createElement('div'); topo.className = 'ifTopo';
    const em = document.createElement('span'); em.className = 'ifEmoji'; em.textContent = o.simbolo;
    const nm = document.createElement('span'); nm.className = 'ifNome'; nm.textContent = `${o.nome} · ${o.faccao}`;
    topo.append(em, nm);

    const st = document.createElement('div'); st.className = 'ifStat'; st.textContent = `${o.stat} +${o.valor}`;

    const nv = document.createElement('div'); nv.className = 'ifNivel';
    const es = document.createElement('span'); es.className = 'ifEstrelas';
    es.textContent = '★'.repeat(o.estrelas) + '☆'.repeat(6 - o.estrelas);
    const lv = document.createElement('span'); lv.className = 'ifLv'; lv.textContent = `nv ${o.nivel}`;
    nv.append(es, lv);

    const trilho = document.createElement('div'); trilho.className = 'ifTrilho';
    const enche = document.createElement('div'); enche.className = 'ifTrilhoCheio';
    enche.style.width = `${o.pct}%`;
    trilho.append(enche);

    card.append(tag, topo, st, nv, trilho);
    return card;
}
