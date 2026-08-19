// AS MARCAS DO CARD — os selos que ficam nos cantos de um card de apóstolo, fora do fluxo.
//
// Existe um lugar só porque o mesmo card aparece em três telas (compêndio, picker, casa do time) e
// o selo tem que nascer no mesmo canto nas três.

// O emoji do PAPEL (🛡️ ⚔️ 🏹 💗), no canto de cima à esquerda. Quem escolhe o emoji é o C#
// (`Tipos.Simbolo`) — o front não tem cópia da tabela.
export function marcaDeTipo(simbolo) {
    const m = document.createElement('span');
    m.className = 'cantoTipo';
    m.textContent = simbolo || '';
    return m;
}

// A BARRA DE NÍVEL — `nv 12 ▬▬▬▬▬▭▭▭`, abaixo do emoji. O número diz ONDE ele está; o trilho, o
// quanto falta pro próximo. Quem calcula os dois é o C# (`ProgressaoService.FaixaDoNivel`).
//
// `pct` negativo = sem trilho, só o número: é o caso do INIMIGO, que tem nível mas não acumula XP —
// um trilho vazio nele diria "quase subindo", que é mentira.
// `previsao` = o quanto a barra ficaria SE o jogador confirmasse o que está montando (queimar alma,
// por enquanto). Ela entra em brasa por cima do âmbar do que já está lá: o mesmo gesto de "isto é
// teu" × "isto seria teu" que a barra de custo de qualquer jogo faz.
//
// Ela satura em 100 de propósito. Se o ganho atravessa nível a barra não tem como mostrar "três
// níveis à frente" — quem diz isso é o RÓTULO (`nv 7 → nv 10`), e o enche-zera-enche acontece na
// confirmação, com a mesma animação do fim de fase.
export function barraDeNivel(nivel, pct, previsao = -1) {
    const b = document.createElement('span');
    b.className = 'barraNivel';

    const rotulo = document.createElement('span');
    rotulo.className = 'bnRotulo';
    rotulo.textContent = `nv ${nivel || 1}`;
    b.append(rotulo);

    if (pct >= 0) {
        const trilho = document.createElement('span');
        trilho.className = 'bnTrilho';

        const atual = Math.max(0, Math.min(100, pct));
        const cheio = document.createElement('span');
        cheio.className = 'bnCheio';
        cheio.style.width = `${atual}%`;
        trilho.append(cheio);

        if (previsao > atual) {
            const p = document.createElement('span');
            p.className = 'bnPrevisao';
            p.style.left = `${atual}%`;
            p.style.width = `${Math.min(100, previsao) - atual}%`;
            trilho.append(p);
        }
        b.append(trilho);
    }
    return b;
}

// AS ESTRELAS — o visor do nível (uma a cada 10), ACIMA do card. Quem conta é o C#
// (`Progressao.Estrelas`), pelo mesmo motivo do emoji do tipo. Nível 1 a 9 não tem estrela nenhuma,
// e a faixa fica vazia em vez de sumir: assim o card não muda de altura quando a primeira chega.
export function marcaDeEstrelas(quantas) {
    const m = document.createElement('span');
    m.className = 'cantoEstrelas';
    m.textContent = '★'.repeat(Math.max(0, quantas || 0));
    return m;
}
