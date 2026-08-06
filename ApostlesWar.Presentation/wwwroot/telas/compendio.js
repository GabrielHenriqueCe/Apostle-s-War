// COMPÊNDIO — o catálogo dos 36 por facção, e a ficha de um champ.
//
// Os travados aparecem, com cadeado, e são clicáveis: é planejando contra o que ainda não se tem
// que a campanha vira escolha.
//
// Duas TELAS num arquivo só: a unidade do contrato é a MENSAGEM que o C# manda, não o arquivo.

import { mandar } from '../nucleo/ponte.js';

// ---------- compêndio ----------
// Catálogo, só leitura: nenhum clique daqui muda progresso. Por isso o champ TRAVADO é clicável
// igual ao liberado — o cadeado diz "ainda não é seu", não "não é da sua conta". Quem decide o que
// está travado é o C# (CampeoesService.EstaDesbloqueado); aqui só se pinta a resposta.
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
            grade.replaceChildren(...f.champs.map(ch => {
                const card = document.createElement('button');
                card.type = 'button';
                card.className = 'compChamp' + (ch.desbloqueado ? '' : ' travado');

                const em = document.createElement('span');
                em.className = 'ccEmoji';
                em.textContent = ch.simbolo;

                const nm = document.createElement('span');
                nm.className = 'ccNome';
                nm.textContent = ch.nome;

                card.append(em, nm);
                if (!ch.desbloqueado) {
                    const cad = document.createElement('span');
                    cad.className = 'ccCadeado';
                    cad.textContent = '🔒';
                    card.appendChild(cad);
                }

                // O índice é GLOBAL (posição na lista completa), não o da facção: a ponte carrega um int
                // só por clique, e mandar (facção, slot) exigiria dois.
                card.addEventListener('click', () => mandar('verChamp', ch.indice));
                return card;
            }));

            bloco.append(titulo, grade);
            return bloco;
        }));
},
};

export const compendioChamp = {
    cena: 'compendioChamp',
    montar(c) {
        // Hoje o emoji gigante; o #champArte é o SLOT que recebe a arte do personagem inteiro depois.
        document.getElementById('champArte').textContent = c.simbolo;
        document.getElementById('champNome').textContent = c.nome;
        document.getElementById('champFaccao').textContent =
            c.desbloqueado ? c.faccao : `${c.faccao} · 🔒 ainda não conquistado`;

        // Números de BASE: catálogo, não simulador — arsenal, itens e buffs não entram aqui.
        const stats = [
            ['❤️', 'HP', c.hp],
            ['⚔️', 'Ataque', c.ataque],
            ['🛡️', 'Defesa', c.defesa],
            ['🎯', 'Taxa de crítico', `${c.taxaCritPct}%`],
            ['💥', 'Dano crítico', `${c.danoCritPct}%`],
        ];

        const painelStats = document.getElementById('champStats');
        const tituloStats = document.createElement('h2');
        tituloStats.className = 'champSecao';
        tituloStats.textContent = 'Estatísticas';

        painelStats.replaceChildren(tituloStats, ...stats.map(([icone, rotulo, valor]) => {
            const linha = document.createElement('div');
            linha.className = 'champStat';

            const ic = document.createElement('span'); ic.className = 'csIcone'; ic.textContent = icone;
            const rot = document.createElement('span'); rot.className = 'csRotulo'; rot.textContent = rotulo;
            const val = document.createElement('span'); val.className = 'csValor'; val.textContent = valor;

            linha.append(ic, rot, val);
            return linha;
        }));

        const painelHabs = document.getElementById('champHabilidades');
        const tituloHabs = document.createElement('h2');
        tituloHabs.className = 'champSecao';
        tituloHabs.textContent = 'Habilidades';

        painelHabs.replaceChildren(tituloHabs, ...c.habilidades.map(h => {
            const card = document.createElement('div');
            card.className = 'champHab' + (h.passiva ? ' passiva' : '');

            const topo = document.createElement('div');
            topo.className = 'chTopo';

            const nome = document.createElement('span');
            nome.className = 'chNome';
            nome.textContent = `${h.simbolo} ${h.nome}`;

            // A passiva não se usa: dizer isso evita o jogador procurar o botão dela. As ativas mostram a
            // cadência DECLARADA — fora da luta não há turno correndo, e é a cadência que se compara.
            const marca = document.createElement('span');
            marca.className = 'chMarca';
            marca.textContent = h.passiva ? 'passiva' : `${h.cooldown} turnos`;

            topo.append(nome, marca);

            const desc = document.createElement('p');
            desc.className = 'chDesc';
            desc.textContent = h.descricao;

            card.append(topo, desc);
            return card;
        }));
},
};
