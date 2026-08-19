// CATEDRAL — onde o apóstolo se aprimora. Quatro colunas: o ELENCO, o escolhido, as PORTAS e a
// estação aberta.
//
// As quatro estações são LUGARES e não verbos — 🎒 Forja (itens), ⬆️ Santuário (nível), ★ Altar
// (estrela) e 🔥 Oferenda (fundir alma). A Forja é o estado de repouso.
//
// Toda ação volta pro C# e a tela inteira é redesenhada. Não há estado de jogo aqui: o que mora
// neste arquivo é só ONDE o jogador estava olhando.

import { mandar } from '../nucleo/ponte.js';
import { marcaDeTipo, marcaDeEstrelas, barraDeNivel } from '../ui/marcas.js';
import { painelDeStats } from '../ui/ficha.js';
import { almaIcone } from '../ui/alma.js';
import { barraDeQuantidade } from '../ui/quantidade.js';

const ARSENAL_AREAS = ['arma', 'elmo', 'escudo', 'acess', 'peito', 'calca', 'bota'];   // slot índice → grid-area
const ARSENAL_ICONES = ['🗡️', '⛑️', '🛡️', '📿', '🎽', '👖', '👢'];   // ícone do tipo quando o slot está vazio

let catedralDados = null;
let catedralSlotSel = -1;
let catedralEstacao = null;   // null = itens à mostra · 'estrela' · 'nivel' · 'almas'

// TROCAR ITEM: a lista de peças toma a coluna do ELENCO, e não uma tela nova. As duas listas nunca
// são úteis ao mesmo tempo — escolhendo arma ninguém está escolhendo apóstolo — e é isso que deixa
// o filtro caber sem estourar o layout, que era o risco de pôr as duas lado a lado.
let catedralTrocando = false;

// O FILTRO da troca. Mora só aqui porque é preferência de quem olha, não estado de jogo: o C# não
// precisa saber que você escondeu os comuns.
let filtro = filtroLimpo();

function filtroLimpo() {
    return {
        faccao: '',        // '' = todas
        stat: '',          // '' = qualquer principal (a chave crua, não o rótulo)
        nivelMin: 0,
        estrelaMin: 0,
        ordem: 'valor',    // 'valor' = o que mais dá do stat · 'nivel' · 'estrela'
        crescente: false,  // o menor primeiro — pra achar o que sacrificar
        misturar: false,   // junta as facções numa lista só, sem separar por conjunto
    };
}

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
        else document.getElementById('forjaDetalhe').hidden = true;

        desenharAcao();
},
};

// ---------- as ESTAÇÕES: as portas da última coluna ----------
//
// Cada uma é um LUGAR e não um verbo — Forja, Santuário, Altar, Oferenda —, e é isso que faz a tela
// inteira ter nome próprio em vez de ser um painel de ações. O 🎒 Forja é o estado de repouso.
function desenharPortas() {
    const c = catedralDados.apostolo;
    document.getElementById('catedralPortas').replaceChildren(
        botaoDeAcao('🎒', 'Forja', null, true),
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
    const nome = document.createElement('div'); nome.className = 'afNome'; nome.textContent = c.ficha.nome;

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
    const direita = document.getElementById('forja');

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
            if (s.slot !== catedralSlotSel) {
                catedralTrocando = false;
                filtro = filtroLimpo();
            }
            desenharSlot(s.slot);
            desenharColunaEsquerda();
        });
        return div;
    }));
}

// ---------- o ACERVO: a lista de peças do slot, com o filtro ----------

// As peças daquele slot já passadas pelo filtro e pela ordem. A EQUIPADA nunca entra: ela está
// desenhada em cima, no painel de comparação, e repetida na lista viraria uma troca por si mesma.
function acervoFiltrado() {
    let itens = catedralDados.obtidos.filter(o => o.slot === catedralSlotSel && !o.equipado);

    if (filtro.faccao) itens = itens.filter(o => o.faccao === filtro.faccao);
    if (filtro.stat) itens = itens.filter(o => o.statChave === filtro.stat);
    if (filtro.nivelMin) itens = itens.filter(o => o.nivel >= filtro.nivelMin);
    if (filtro.estrelaMin) itens = itens.filter(o => o.estrelas >= filtro.estrelaMin);

    const chave = o => filtro.ordem === 'nivel' ? o.nivel
        : filtro.ordem === 'estrela' ? o.estrelas
        : o.valorNum;

    // Comparar `valorNum` entre stats DIFERENTES não diz nada (57,5 de ATK contra 0,0575 de HP%), e
    // é por isso que a ordem por valor só é honesta com o filtro de stat ligado — a lista avisa isso
    // em vez de fingir um ranking. Dentro do mesmo stat ela é exata.
    itens.sort((a, b) => (chave(a) - chave(b)) * (filtro.crescente ? 1 : -1));

    return itens;
}

function desenharAcervo() {
    const col = document.getElementById('catedralElenco');
    const itens = acervoFiltrado();

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
    } else if (filtro.misturar) {
        corpo.append(...itens.map(cardDeAcervo));
    } else {
        // SEPARADO POR FACÇÃO por padrão: é o conjunto que dá sentido à facção no item, e uma lista
        // misturada apaga essa leitura. Quem não se importa liga o botão "misturar".
        const porFaccao = new Map();
        for (const o of itens) {
            if (!porFaccao.has(o.faccao)) porFaccao.set(o.faccao, []);
            porFaccao.get(o.faccao).push(o);
        }
        for (const [faccao, doGrupo] of porFaccao) {
            const cab = document.createElement('div');
            cab.className = 'acervoGrupo';
            // O símbolo vem junto do nome: é como a facção se identifica em toda outra tela (mapa,
            // compêndio, fases), e sem ele o cabeçalho aqui seria o único lugar que a chama só pelo
            // nome.
            cab.textContent = `${doGrupo[0].faccaoSimbolo} ${faccao} · ${doGrupo.length}`;
            corpo.append(cab, ...doGrupo.map(cardDeAcervo));
        }
    }

    col.replaceChildren(topo, desenharFiltro(), corpo);
}

// O cartão da peça, na mesma peça do cartão de apóstolo: emoji grande, a estrela e o nível — e MAIS
// NADA. Um acervo de centenas não se lê por ficha; se lê por grade, batendo o olho na estrela.
// Quem quer o número da peça clica nela, e ele abre na comparação ao lado.
function cardDeAcervo(o) {
    const card = document.createElement('button');
    card.type = 'button';
    const olhando = catedralDados.previa && catedralDados.previa.indice === o.indice;
    card.className = 'acervoItem' + (olhando ? ' olhando' : '');
    card.title = `${o.nome} · ${o.faccao} · ${o.stat} +${o.valor}`;   // o resto fica no hover

    const em = document.createElement('span'); em.className = 'aiEmoji'; em.textContent = o.simbolo;
    const es = document.createElement('span'); es.className = 'aiEstrelas';
    es.textContent = '★'.repeat(o.estrelas) + '☆'.repeat(6 - o.estrelas);
    const lv = document.createElement('span'); lv.className = 'aiLv'; lv.textContent = `nv ${o.nivel}`;

    card.append(em, es, lv);
    card.addEventListener('click', () => mandar('preverItem', o.indice));
    return card;
}

// O FILTRO, em CHIPS e não em formulário. `<select>` e `<checkbox>` nativos aparecem com a cara do
// Windows — caixa branca, borda cinza — e num painel escuro isso lê como planilha, não como jogo.
// Chip é o mesmo botão-placa do resto da tela, e ainda mostra as opções TODAS de uma vez, sem abrir
// menu nenhum. Cabe na coluna porque cada grupo quebra linha sozinho.
function desenharFiltro() {
    const box = document.createElement('div');
    box.className = 'acervoFiltro';

    const doSlot = catedralDados.obtidos.filter(o => o.slot === catedralSlotSel && !o.equipado);
    const faccoes = [...new Set(doSlot.map(o => o.faccao))].sort();
    const stats = [...new Map(doSlot.map(o => [o.statChave, o.stat])).entries()];

    // Conjunto e Principal só aparecem quando há o que escolher: com uma facção só no acervo, uma
    // fileira de um chip é ruído. O filtro encolhe com o acervo em vez de ficar sempre do tamanho
    // do pior caso — é o que o mantém dentro da coluna.
    if (faccoes.length > 1)
        box.append(grupo('Conjunto', filtro.faccao, [['', 'todos'], ...faccoes.map(f => [f, f])],
            v => filtro.faccao = v));

    if (stats.length > 1)
        box.append(grupo('Principal', filtro.stat, [['', 'qualquer'], ...stats],
            v => filtro.stat = v));

    box.append(
        grupo('Nível ≥', String(filtro.nivelMin), degraus(0, 60, 10), v => filtro.nivelMin = Number(v)),
        grupo('Estrela ≥', String(filtro.estrelaMin), degraus(0, 6, 1), v => filtro.estrelaMin = Number(v)),
        grupo('Ordenar', filtro.ordem,
            [['valor', 'quanto dá'], ['nivel', 'nível'], ['estrela', 'estrela']],
            v => filtro.ordem = v),
        grupo('Sentido', filtro.crescente ? 'sim' : 'nao',
            [['nao', 'maior 1º'], ['sim', 'menor 1º']],
            v => filtro.crescente = v === 'sim'),
        grupo('Conjuntos', filtro.misturar ? 'sim' : 'nao',
            [['nao', 'separados'], ['sim', 'misturados']],
            v => filtro.misturar = v === 'sim'));

    return box;
}

function degraus(de, ate, passo) {
    const fora = [];
    for (let n = de; n <= ate; n += passo) fora.push([String(n), n === 0 ? '—' : String(n)]);
    return fora;
}

// Um grupo do filtro: o rótulo e a fileira de chips. Só redesenha a COLUNA — mexer no filtro é
// preferência de quem olha, não ação de jogo, então não vale uma volta à ponte; e uma volta
// apagaria a peça que está sendo comparada ao lado.
function grupo(rotulo, valor, opcoes, aoMudar) {
    const bloco = document.createElement('div');
    bloco.className = 'flGrupo';

    const r = document.createElement('div'); r.className = 'flRotulo'; r.textContent = rotulo;

    const fila = document.createElement('div'); fila.className = 'flFila';
    fila.replaceChildren(...opcoes.map(([v, t]) => {
        const chip = document.createElement('button');
        chip.type = 'button';
        chip.className = 'flChip' + (v === valor ? ' ligado' : '');
        chip.textContent = t;
        chip.addEventListener('click', () => { aoMudar(v); desenharAcervo(); });
        return chip;
    }));

    bloco.append(r, fila);
    return bloco;
}

// ---------- o painel do SLOT: a peça de hoje, e a que você está pensando em pôr ----------
//
// Clicar num slot mostra as estatísticas DAQUELA peça e mais nada. A lista inteira de armas com a
// ficha de cada uma era ilegível — a decisão é entre DUAS peças, não entre catorze, e é assim que
// ela se desenha: a de hoje em cima, a candidata embaixo, a diferença entre as duas no meio.
function desenharSlot(slot) {
    catedralSlotSel = slot;
    desenharBoneco();
    document.getElementById('forjaDetalhe').hidden = false;
    document.getElementById('forjaSlotNome').textContent = catedralDados.slots[slot].nome;

    const cont = document.getElementById('forjaItens');
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

    const trocar = document.createElement('button');
    trocar.type = 'button';
    trocar.className = 'trocarItem' + (catedralTrocando ? ' ativo' : '');
    trocar.textContent = catedralTrocando ? 'escolhendo…' : 'Trocar item';
    trocar.addEventListener('click', () => {
        catedralTrocando = !catedralTrocando;
        if (!catedralTrocando) { mandar('preverItem', -1); return; }   // fechou: some a comparação
        desenharColunaEsquerda();
        desenharSlot(slot);
    });
    filhos.push(trocar);

    // A candidata entra ABAIXO da equipada, no mesmo lugar onde a lista morava. As duas fichas lado
    // a lado na vertical são a comparação — o delta é o que sobra de olhar as duas.
    if (candidata) {
        filhos.push(fichaDeItem(candidata, 'candidata'), blocoDeDelta(previa.deltas));

        const equipar = document.createElement('button');
        equipar.type = 'button';
        equipar.className = 'acaoConfirmar';
        equipar.textContent = 'Equipar esta';
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

// A DIFERENÇA, já resolvida contra o apóstolo selecionado pelo C#: só as linhas que mudam, e o
// número é o da FICHA dele (não o da peça), porque é ele que decide se a troca vale.
function blocoDeDelta(deltas) {
    const box = document.createElement('div');
    box.className = 'itemDelta';

    if (!deltas.length) {
        box.append(vazio('Não muda nada na ficha dele.'));
        return box;
    }

    box.replaceChildren(...deltas.map(d => {
        const linha = document.createElement('div');
        linha.className = 'idLinha ' + (d.delta > 0 ? 'sobe' : 'desce');
        const r = document.createElement('span'); r.className = 'idRotulo'; r.textContent = d.rotulo;
        const a = document.createElement('span'); a.className = 'idAntes';
        a.textContent = `${d.antes.toLocaleString('pt-BR')}${d.sufixo}`;
        const seta = document.createElement('span'); seta.className = 'idSeta'; seta.textContent = '→';
        const p = document.createElement('span'); p.className = 'idDepois';
        p.textContent = `${d.depois.toLocaleString('pt-BR')}${d.sufixo}`;
        const dl = document.createElement('span'); dl.className = 'idDelta';
        dl.textContent = `${d.delta > 0 ? '+' : ''}${d.delta.toLocaleString('pt-BR')}${d.sufixo}`;
        linha.append(r, a, seta, p, dl);
        return linha;
    }));

    return box;
}

