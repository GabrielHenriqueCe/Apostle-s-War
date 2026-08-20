// O POZINHO — o 🧂 pintado pela faixa. É o irmão da alminha (ui/alma.js), e a moeda das PEÇAS.
//
// O saleiro é o emoji que mostra pó saindo de um recipiente. O preço é ele ser quase branco: matiz
// sem saturação não roda, então `hue-rotate` sozinho (o filtro da alminha) pintaria as seis faixas
// de cinza. Por isso o estilo.css passa `sepia` ANTES — copiar o filtro da alma pra cá apaga a cor.

import { FAIXAS } from './raridade.js';

export function poIcone(raridade, tamanho = 18) {
    const el = document.createElement('span');
    el.className = `pozinho pozinho-${FAIXAS[raridade] || 'comum'}`;
    el.textContent = '🧂';
    el.style.setProperty('--po-tam', `${tamanho}px`);
    return el;
}

/// O ícone com o número do lado — é assim que o saldo de uma faixa aparece.
export function poChip(p, tamanho = 18) {
    const chip = document.createElement('span');
    chip.className = 'poChip';
    chip.title = p.nome;

    const n = document.createElement('span');
    n.className = 'acQtd';
    n.textContent = (p.quantidade ?? 0).toLocaleString('pt-BR');

    chip.append(poIcone(p.raridade, tamanho), n);
    return chip;
}
