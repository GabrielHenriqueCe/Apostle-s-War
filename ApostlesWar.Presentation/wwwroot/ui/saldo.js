// O SALDO ao lado da aba — o que o bolso tem daquela moeda, lado a lado.
//
// A Catedral e a Forja mostram o MESMO par de abas, e cada aba carrega a moeda dela: alma na ponta
// esquerda, pó na direita. Como as duas telas desenham o mesmo par, o desenho mora aqui e não em
// nenhuma das duas — a alternativa era a mesma função escrita duas vezes, divergindo na terceira.
//
// FAIXA ZERADA NÃO VIRA CHIP, e é aqui que essa regra mora: o C# manda as seis sempre (é o saldo
// cru), e quem joga o Fácil tem três. Mostrar seis com metade em 0 esconde as três que valem.

import { almaIcone } from './alma.js';
import { poIcone } from './po.js';

/// Pinta o saldo de ALMA no elemento de id `alvo`. `quantias` é a lista crua das seis faixas.
export const saldoDeAlma = (alvo, quantias) => pintar(alvo, quantias, almaIcone);

/// Pinta o saldo de PÓ. O irmão do saldoDeAlma, e a única diferença é quem desenha a faixa.
export const saldoDePo = (alvo, quantias) => pintar(alvo, quantias, poIcone);

function pintar(alvo, quantias, icone) {
    const el = document.getElementById(alvo);
    if (!el) return;

    el.replaceChildren(...(quantias || [])
        .filter(q => (q.quantidade ?? 0) > 0)
        .map(q => {
            const chip = document.createElement('span');
            chip.className = 'asChip';
            chip.title = q.nome;

            const n = document.createElement('span');
            n.className = 'asQtd';
            n.textContent = q.quantidade.toLocaleString('pt-BR');

            chip.append(icone(q.raridade, 17), n);
            return chip;
        }));
}
