// A ALMINHA — o 🔥 pintado da cor da faixa.
//
// Foi SVG desenhado à mão antes, e o emoji ganhou: o desenho não sobrevivia aos 18px do chip, e o
// 🔥 já é legível em qualquer tamanho porque a fonte do sistema resolve isso.
//
// A cor da faixa é `filter: hue-rotate` no estilo.css, e ela só funciona porque o 🔥 já nasce
// saturado. Emoji quase branco não aceita esse filtro — ver o `sepia` do pozinho (ui/po.js).

import { FAIXAS } from './raridade.js';

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
