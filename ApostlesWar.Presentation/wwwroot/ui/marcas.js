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
