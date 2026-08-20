// O CARTÃO de uma peça no acervo — o emoji, as estrelas e o nível, e o resto no hover.
//
// Dois donos: a troca na Catedral e a escolha do que vai pra bigorna na Forja. O que muda entre as
// duas é só o que o clique MANDA e o que "marcada" quer dizer (a que está sendo comparada lá, a que
// está na bigorna aqui) — a peça se desenha igual nos dois lugares, e é isso que faz o acervo ser
// reconhecível de uma tela pra outra.
export function cardDePeca(o, { marcada = false, aoClicar }) {
    const card = document.createElement('button');
    card.type = 'button';
    card.className = 'acervoItem' + (marcada ? ' olhando' : '');
    card.title = `${o.nome} · ${o.faccao} · ${o.stat} +${o.valor}`;   // o resto fica no hover

    const em = document.createElement('span'); em.className = 'aiEmoji'; em.textContent = o.simbolo;
    const es = document.createElement('span'); es.className = 'aiEstrelas';
    es.textContent = '★'.repeat(o.estrelas) + '☆'.repeat(6 - o.estrelas);
    const lv = document.createElement('span'); lv.className = 'aiLv'; lv.textContent = `nv ${o.nivel}`;

    card.append(em, es, lv);
    card.addEventListener('click', aoClicar);
    return card;
}
