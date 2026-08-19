// A ALMINHA — o 🔥 num anel da cor da raridade.
//
// Foi SVG desenhado à mão antes, e o emoji ganhou: o desenho não sobrevivia aos 18px do chip, e o
// 🔥 já é legível em qualquer tamanho porque a fonte do sistema resolve isso.
//
// O PREÇO, e ele é real: o emoji tem cor própria (laranja) e não dá pra pintar de seis cores —
// filtro de matiz num emoji sai lavado e diferente em cada máquina. Então a raridade não está no
// fogo, está no ANEL em volta dele. Quem lê a faixa lê o anel.
//
// A ORDEM é a do enum Raridade no C#. O índice que chega é o VALOR do enum, não a posição numa
// lista filtrada — por isso ele viaja dentro do AlmaVista.

export const FAIXAS = ['comum', 'incomum', 'raro', 'epico', 'lendario', 'mitico'];

export function almaIcone(raridade, tamanho = 18) {
    const el = document.createElement('span');
    el.className = `alminha alminha-${FAIXAS[raridade] || 'comum'}`;
    el.textContent = '🔥';
    el.style.setProperty('--alma-tam', `${tamanho}px`);
    return el;
}

/// O ícone com o número do lado — é assim que o saldo aparece em toda tela.
export function almaChip(a, tamanho = 18) {
    const chip = document.createElement('span');
    chip.className = 'almaChip';
    chip.title = a.nome;

    const n = document.createElement('span');
    n.className = 'acQtd';
    n.textContent = (a.quantidade ?? 0).toLocaleString('pt-BR');

    chip.append(almaIcone(a.raridade, tamanho), n);
    return chip;
}
