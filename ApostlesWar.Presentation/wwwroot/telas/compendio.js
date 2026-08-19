// COMPÊNDIO — o catálogo dos 36 por facção, e a ficha de um apóstolo.
//
// Os travados aparecem, com cadeado, e são clicáveis: é planejando contra o que ainda não se tem
// que a campanha vira escolha.
//
// Duas TELAS num arquivo só: a unidade do contrato é a MENSAGEM que o C# manda, não o arquivo.

import { mandar } from '../nucleo/ponte.js';
import { marcaDeTipo, marcaDeEstrelas, barraDeNivel } from '../ui/marcas.js';
import { painelDeStats, painelDeHabilidades } from '../ui/ficha.js';

// ---------- compêndio ----------
// Catálogo, só leitura: nenhum clique daqui muda progresso. Por isso o apóstolo TRAVADO é clicável
// igual ao liberado — o cadeado diz "ainda não é seu", não "não é da sua conta". Quem decide o que
// está travado é o C# (ApostolosService.EstaDesbloqueado); aqui só se pinta a resposta.
export const compendio = {
    cena: 'compendio',
    montar(c) {
        document.getElementById('compendioFaccoes').replaceChildren(...c.faccoes.map(f => {
            const bloco = document.createElement('section');
            bloco.className = 'compFaccao';

            const titulo = document.createElement('h2');
            titulo.className = 'compFaccaoNome';
            titulo.textContent = `${f.simbolo} ${f.nome}`;

            const grade = document.createElement('div');
            grade.className = 'compGrade';
            grade.replaceChildren(...f.apostolos.map(ch => {
                const card = document.createElement('button');
                card.type = 'button';
                card.className = 'compApostolo' + (ch.desbloqueado ? '' : ' travado');

                const em = document.createElement('span');
                em.className = 'ccEmoji';
                em.textContent = ch.simbolo;

                const nm = document.createElement('span');
                nm.className = 'ccNome';
                nm.textContent = ch.nome;

                card.append(marcaDeTipo(ch.tipoSimbolo), marcaDeEstrelas(ch.estrelas), em, barraDeNivel(ch.nivel, ch.xpPct), nm);
                if (!ch.desbloqueado) {
                    const cad = document.createElement('span');
                    cad.className = 'ccCadeado';
                    cad.textContent = '🔒';
                    card.appendChild(cad);
                }

                // O índice é GLOBAL (posição na lista completa), não o da facção: a ponte carrega um int
                // só por clique, e mandar (facção, slot) exigiria dois.
                card.addEventListener('click', () => mandar('verApostolo', ch.indice));
                return card;
            }));

            bloco.append(titulo, grade);
            return bloco;
        }));
},
};

export const compendioApostolo = {
    cena: 'compendioApostolo',
    montar(c) {
        // Hoje o emoji gigante; o #apostoloArte é o SLOT que recebe a arte do personagem inteiro depois.
        document.getElementById('apostoloArte').textContent = c.simbolo;
        document.getElementById('apostoloNome').textContent = c.nome;
        // O TIPO entra aqui e não na lista de stats: ele é a identidade que explica os números
        // abaixo (todo Guardião tem a mesma ficha), não mais um número entre eles.
        const identidade = `${c.faccao} · ${c.tipoSimbolo} ${c.tipo}`;
        document.getElementById('apostoloFaccao').textContent =
            c.desbloqueado ? identidade : `${identidade} · 🔒 ainda não conquistado`;

        // O nível saiu da linha de identidade e virou a BARRA: ele é a única coisa ali que muda com o
        // tempo, e como texto no meio da frase não dava pra ver o quanto falta pro próximo.
        document.getElementById('apostoloNivel').replaceChildren(barraDeNivel(c.nivel, c.xpPct));

        document.getElementById('apostoloStats').replaceChildren(...painelDeStats(c));
        document.getElementById('apostoloHabilidades').replaceChildren(...painelDeHabilidades(c));
},
};
