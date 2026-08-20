// O NAVEGADOR — `‹ rótulo ›`, o gesto de "o próximo do que eu estou olhando".
//
// Dois donos com objetos diferentes: na Catedral ele troca o APÓSTOLO, na Forja o TIPO da peça. O
// gesto é o mesmo de propósito — gesto que só existe em uma tela ninguém aprende.
//
// A ARMADILHA que ele desarma, e é por isso que o rótulo vem DENTRO dele: na Forja há duas coisas
// que se trocam ao mesmo tempo — o tipo (arma → elmo) e qual peça daquele tipo (a coluna da
// esquerda). Setas soltas ao lado do emoji leriam como "próxima arma". É a posição que diz o que
// muda, então elas têm de abraçar o rótulo do que elas mudam, sempre.
//
// Com um item só não há o que percorrer, e aí as setas SOMEM em vez de ficarem mortas.
export function navegador(rotulo, { aoAnterior, aoProximo, ha = true, classe = '' }) {
    const box = document.createElement('div');
    box.className = 'navegador ' + classe;

    const texto = document.createElement('span');
    texto.className = 'navRotulo';
    texto.textContent = rotulo;

    if (!ha) {
        box.append(texto);
        return box;
    }

    box.append(seta('‹', aoAnterior), texto, seta('›', aoProximo));
    return box;
}

function seta(glifo, aoClicar) {
    const b = document.createElement('button');
    b.type = 'button';
    b.className = 'navSeta';
    b.textContent = glifo;
    b.addEventListener('click', aoClicar);
    return b;
}
