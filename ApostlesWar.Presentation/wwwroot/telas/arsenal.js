// ARSENAL — o boneco com os 7 slots, o que o conjunto dá SOMADO, e os itens pra equipar.

import { mandar } from '../nucleo/ponte.js';

const ARSENAL_AREAS = ['arma', 'elmo', 'escudo', 'acess', 'peito', 'calca', 'bota'];   // slot índice → grid-area
const ARSENAL_ICONES = ['🗡️', '⛑️', '🛡️', '📿', '🎽', '👖', '👢'];   // ícone do tipo quando o slot está vazio

let arsenalDados = null;
let arsenalSlotSel = -1;

export const arsenal = {
    cena: 'arsenal',
    montar(a, anterior) {
        if (anterior !== 'arsenal') arsenalSlotSel = -1;   // entrada fresca → nenhum slot aberto
        arsenalDados = a;
        desenharBoneco();
        desenharTotais();
        if (arsenalSlotSel >= 0) mostrarItensSlot(arsenalSlotSel);
        else document.getElementById('arsenalDetalhe').hidden = true;
},
};

// O que o conjunto equipado dá, somado. Quem soma e quem escreve o número é o C#
// (ArsenalService.TotaisEquipados + ValorFormatado) — aqui só chega texto pronto, pelo mesmo motivo
// de sempre: "0.05" virar "5%" é exibição, mas QUANTO é regra de item.
function desenharTotais() {
    const cont = document.getElementById('arsenalTotais');
    const titulo = document.createElement('div');
    titulo.className = 'atTitulo';
    titulo.textContent = 'Bônus do arsenal';

    if (!arsenalDados.totais.length) {
        const v = document.createElement('div');
        v.className = 'atVazio';
        v.textContent = 'Nada equipado ainda.';
        cont.replaceChildren(titulo, v);
        return;
    }

    cont.replaceChildren(titulo, ...arsenalDados.totais.map(b => {
        const linha = document.createElement('div'); linha.className = 'atLinha';
        const rot = document.createElement('span'); rot.className = 'atStat'; rot.textContent = b.stat;
        const val = document.createElement('span'); val.className = 'atValor'; val.textContent = b.valor;
        linha.append(rot, val);
        return linha;
    }));
}

function desenharBoneco() {
    document.getElementById('boneco').replaceChildren(...arsenalDados.slots.map(s => {
        const div = document.createElement('div');
        div.className = 'bonecoSlot' + (s.slot === arsenalSlotSel ? ' selecionado' : '') + (s.equipado ? ' preenchido' : '');
        div.style.gridArea = ARSENAL_AREAS[s.slot];
        const emoji = document.createElement('div'); emoji.className = 'bsEmoji';
        emoji.textContent = s.equipado ? s.equipado.simbolo : ARSENAL_ICONES[s.slot];
        const nome = document.createElement('div'); nome.className = 'bsNome'; nome.textContent = s.nome;
        div.append(emoji, nome);
        div.addEventListener('click', () => mostrarItensSlot(s.slot));
        return div;
    }));
}

function mostrarItensSlot(slot) {
    arsenalSlotSel = slot;
    desenharBoneco();
    document.getElementById('arsenalDetalhe').hidden = false;
    document.getElementById('arsenalSlotNome').textContent = arsenalDados.slots[slot].nome;

    const itens = arsenalDados.obtidos.filter(o => o.slot === slot);
    const equipado = itens.find(o => o.equipado);
    const cont = document.getElementById('arsenalItens');

    if (!itens.length) {
        const v = document.createElement('div'); v.className = 'arsenalVazio'; v.textContent = 'Nenhum item deste tipo ainda.';
        cont.replaceChildren(v);
        return;
    }

    cont.replaceChildren(...itens.map(o => {
        const card = document.createElement('div');
        card.className = 'itemCard' + (o.equipado ? ' equipado' : '');

        const em = document.createElement('span'); em.className = 'icEmoji'; em.textContent = o.simbolo;
        const info = document.createElement('div'); info.className = 'icInfo';
        const nm = document.createElement('div'); nm.className = 'icNome'; nm.textContent = `${o.nome} · ${o.faccao}`;
        const st = document.createElement('div'); st.className = 'icStat'; st.textContent = `${o.stat} +${o.valor}`;
        info.append(nm, st);
        card.append(em, info);

        if (equipado && !o.equipado) {   // seta de diferença vs o equipado
            const diff = o.valorNum - equipado.valorNum;
            const d = document.createElement('div');
            d.className = 'icDiff ' + (diff > 0 ? 'sobe' : diff < 0 ? 'desce' : '');
            d.textContent = diff > 0 ? '▲' : diff < 0 ? '▼' : '=';
            card.append(d);
        }
        if (o.equipado) {
            const tag = document.createElement('div'); tag.className = 'icTag'; tag.textContent = 'equipado';
            card.append(tag);
        } else {
            card.addEventListener('click', () => mandar('equiparItem', o.indice));
        }
        return card;
    }));
}
