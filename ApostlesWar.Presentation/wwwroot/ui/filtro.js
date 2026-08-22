// O FILTRO DO ACERVO — os eixos, a ordem e a separação por conjunto.
//
// Ele mora aqui e não numa tela porque as DUAS telas mostram o mesmo acervo: a troca na Catedral e a
// escolha do que vai pra bigorna na Forja. É o mesmo motivo do `ui/peca.js` — o cartão já era
// compartilhado, e um filtro copiado seria a metade que envelhece sozinha.
//
// O que MUDA entre as duas é só o conjunto de eixos: na Catedral o "Vestidas" existe (tomar a peça
// de um aliado é gesto), e na Forja não — lá toda peça do slot é forjável, e esconder as vestidas
// esconderia justamente a peça por onde se entrou.
//
// O filtro é por SLOT, sempre. Quem troca de slot limpa o filtro: um "principal: Taxa Crítica"
// vindo da Manopla esvaziaria a lista da Bota, e a tela pareceria vazia sem dizer por quê.

export function filtroLimpo({ deAliados = false } = {}) {
    return {
        faccao: '',        // '' = todas
        stat: '',          // '' = qualquer principal (a chave crua, não o rótulo)
        nivelMin: 0,
        estrelaMin: 0,
        ordem: 'nivel',    // 'nivel' · 'estrela'
        crescente: false,  // o menor primeiro — pra achar o que sacrificar
        misturar: false,   // junta as facções numa lista só, sem separar por conjunto
        deAliados,         // mostra TAMBÉM as peças vestidas em outros apóstolos, pra tomá-las
    };
}

/// A base da lista: o que sobra depois do eixo que NÃO é preferência de leitura. Serve pra decidir
/// quais chips aparecem — oferecer "Conjunto: Reino" quando a única peça do Reino está escondida
/// daria um filtro que devolve lista vazia.
export const base = (itens, filtro) =>
    filtro.deAliados ? itens : itens.filter(o => !o.portadorSimbolo);

/// Filtra e ORDENA. As duas ordens comparam número da MESMA escada (nível e estrela), então ambas
/// dizem a verdade em qualquer lista.
export function aplicar(itens, filtro) {
    let fora = base(itens, filtro);

    if (filtro.faccao) fora = fora.filter(o => o.faccao === filtro.faccao);
    if (filtro.stat) fora = fora.filter(o => o.statChave === filtro.stat);
    if (filtro.nivelMin) fora = fora.filter(o => o.nivel >= filtro.nivelMin);
    if (filtro.estrelaMin) fora = fora.filter(o => o.estrelas >= filtro.estrelaMin);

    const chave = o => filtro.ordem === 'estrela' ? o.estrelas : o.nivel;

    fora.sort((a, b) => (chave(a) - chave(b)) * (filtro.crescente ? 1 : -1));
    return fora;
}

/// A lista em GRUPOS. Separada por facção por padrão — é o conjunto que dá sentido à facção no item,
/// e uma lista misturada apaga essa leitura. Quem não se importa liga "misturados", e aí vem um
/// grupo só, sem cabeçalho.
export function agrupar(itens, filtro) {
    if (filtro.misturar) return [{ cabecalho: null, itens }];

    const porFaccao = new Map();
    for (const o of itens) {
        if (!porFaccao.has(o.faccao)) porFaccao.set(o.faccao, []);
        porFaccao.get(o.faccao).push(o);
    }
    return [...porFaccao].map(([faccao, doGrupo]) => ({
        // O símbolo vem junto do nome: é como a facção se identifica em toda outra tela (mapa,
        // compêndio, fases), e sem ele o cabeçalho aqui seria o único lugar que a chama só pelo nome.
        cabecalho: `${doGrupo[0].faccaoSimbolo} ${faccao} · ${doGrupo.length}`,
        itens: doGrupo,
    }));
}

// O PAINEL, em CHIPS e não em formulário. `<select>` e `<checkbox>` nativos aparecem com a cara do
// Windows — caixa branca, borda cinza — e num painel escuro isso lê como planilha, não como jogo.
// Chip é o mesmo botão-placa do resto da tela, e ainda mostra as opções TODAS de uma vez, sem abrir
// menu nenhum. Cabe na coluna porque cada grupo quebra linha sozinho.
//
// `aoMudar` redesenha só a COLUNA de quem chamou: mexer no filtro é preferência de quem olha, não
// ação de jogo, então não vale uma volta à ponte — e uma volta apagaria a peça em comparação.
export function painelDeFiltro(itens, filtro, aoMudar, { comVestidas = true } = {}) {
    const box = document.createElement('div');
    box.className = 'acervoFiltro';

    const doSlot = base(itens, filtro);
    const faccoes = [...new Set(doSlot.map(o => o.faccao))].sort();
    const stats = [...new Map(doSlot.map(o => [o.statChave, o.stat])).entries()];

    const grupo = (rotulo, valor, opcoes, aplicarValor) =>
        chips(rotulo, valor, opcoes, v => { aplicarValor(v); aoMudar(); });

    // Conjunto e Principal só aparecem quando há o que escolher: com uma facção só no acervo, uma
    // fileira de um chip é ruído. O filtro encolhe com o acervo em vez de ficar sempre do tamanho do
    // pior caso — é o que o mantém dentro da coluna.
    if (faccoes.length > 1)
        box.append(grupo('Conjunto', filtro.faccao, [['', 'todos'], ...faccoes.map(f => [f, f])],
            v => filtro.faccao = v));

    if (stats.length > 1)
        box.append(grupo('Principal', filtro.stat, [['', 'qualquer'], ...stats],
            v => filtro.stat = v));

    box.append(
        grupo('Nível ≥', String(filtro.nivelMin), degraus(0, 60, 10), v => filtro.nivelMin = Number(v)),
        grupo('Estrela ≥', String(filtro.estrelaMin), degraus(0, 6, 1), v => filtro.estrelaMin = Number(v)),
        // A ordem "quanto dá" MORREU (decisão do Gabriel, ago/2026): ela comparava o valor cru do
        // principal, e 57,5 de ATK contra 0,0575 de HP% não é comparação — só significava algo com
        // o Principal já escolhido, e a tela nunca disse isso. Quem entra no lugar dela é a
        // raridade, quando o eixo existir (o passo 10-b2).
        grupo('Ordenar', filtro.ordem,
            [['nivel', 'nível'], ['estrela', 'estrela']],
            v => filtro.ordem = v),
        grupo('Sentido', filtro.crescente ? 'sim' : 'nao',
            [['nao', 'maior 1º'], ['sim', 'menor 1º']],
            v => filtro.crescente = v === 'sim'),
        grupo('Conjuntos', filtro.misturar ? 'sim' : 'nao',
            [['nao', 'separados'], ['sim', 'misturados']],
            v => filtro.misturar = v === 'sim'));

    // O chip do ROUBO, e ele é só da Catedral. Fica por último porque é o único que traz peça de
    // fora do baú, e cada uma delas chega com o emoji de quem a está usando.
    if (comVestidas)
        box.append(grupo('Vestidas', filtro.deAliados ? 'sim' : 'nao',
            [['nao', 'só no baú'], ['sim', 'incluir aliados']],
            v => filtro.deAliados = v === 'sim'));

    return box;
}

function degraus(de, ate, passo) {
    const fora = [];
    for (let n = de; n <= ate; n += passo) fora.push([String(n), n === 0 ? '—' : String(n)]);
    return fora;
}

// Um grupo do filtro: o rótulo e a fileira de chips.
function chips(rotulo, valor, opcoes, aoEscolher) {
    const bloco = document.createElement('div');
    bloco.className = 'flGrupo';

    const r = document.createElement('div'); r.className = 'flRotulo'; r.textContent = rotulo;

    const fila = document.createElement('div'); fila.className = 'flFila';
    fila.replaceChildren(...opcoes.map(([v, t]) => {
        const chip = document.createElement('button');
        chip.type = 'button';
        chip.className = 'flChip' + (v === valor ? ' ligado' : '');
        chip.textContent = t;
        chip.addEventListener('click', () => aoEscolher(v));
        return chip;
    }));

    bloco.append(r, fila);
    return bloco;
}
