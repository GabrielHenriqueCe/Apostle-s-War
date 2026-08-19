// A BARRA DE DIFICULDADE — quatro botões, e o mesmo controle no mapa e na tela de fases.
//
// Um lugar só porque o jogador troca dos dois lugares e espera a mesma coisa nos dois. Quem decide
// o que está aberto é o C# (`desbloqueada` + `requisito`); aqui só se desenha a resposta — a travada
// CONTINUA na barra, senão a campanha depois do Fácil vira segredo.

import { mandar } from '../nucleo/ponte.js';

export function montarBarraDificuldade(alvo, dificuldades, ativa) {
    document.getElementById(alvo).replaceChildren(...(dificuldades || []).map(d => {
        const b = document.createElement('button');
        b.type = 'button';
        b.className = 'difBotao' + (d.valor === ativa ? ' ativa' : '') + (d.desbloqueada ? '' : ' travada');
        b.textContent = d.desbloqueada ? d.nome : `🔒 ${d.nome}`;
        if (d.requisito) b.title = d.requisito;
        b.disabled = !d.desbloqueada;
        if (d.desbloqueada) b.addEventListener('click', () => mandar('escolherDificuldade', d.valor));
        return b;
    }));
}
