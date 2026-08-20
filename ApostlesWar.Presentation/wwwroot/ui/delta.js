// A DIFERENÇA na ficha do apóstolo, já resolvida pelo C#: só as linhas que MUDAM, e o número é o
// da ficha dele — não o da peça. É ele que decide se a troca (ou o nível novo) vale.
//
// Dois donos: a troca de peça na Catedral e a prévia de nível na Forja. O `vazio` é diferente nas
// duas — trocar por uma peça igual e malhar uma peça no baú não dizem a mesma coisa —, então a
// frase entra por parâmetro em vez de nascer aqui.
export function blocoDeDelta(deltas, semNada = 'Não muda nada na ficha dele.') {
    const box = document.createElement('div');
    box.className = 'itemDelta';

    if (!deltas.length) {
        const v = document.createElement('div');
        v.className = 'catedralVazio';
        v.textContent = semNada;
        box.append(v);
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
