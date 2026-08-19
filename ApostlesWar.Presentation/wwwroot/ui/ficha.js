// A FICHA de um apóstolo — os dois painéis que o Compêndio e o Arsenal desenham igual.
//
// Devolvem ARRAYS de elementos em vez de escrever num id: as duas telas têm molduras diferentes
// (o compêndio abre em tela cheia, a Catedral a põe na coluna do meio) e só o conteúdo é o mesmo.

// Números de BASE: catálogo, não simulador — arsenal, itens e buffs não entram aqui.
// A ordem é a da tabela do GDD §2, pra conferir a ficha contra o doc de cima a baixo.
//
// `alvo` = a ficha do nível de DESTINO, quando o jogador está montando uma evolução. Aí a linha
// que muda vira `1.240 → 1.380` no próprio lugar dela, em vez de um segundo bloco embaixo: eram
// duas listas dizendo do mesmo apóstolo, e a de baixo era a que não cabia na tela.
export function painelDeStats(c, alvo = null) {
    const stats = [
        ['❤️', 'HP', c.hp, 'hp'],
        ['⚔️', 'Ataque', c.ataque, 'ataque'],
        ['🛡️', 'Defesa', c.defesa, 'defesa'],
        ['⚡', 'Velocidade', c.velocidade, 'velocidade'],
        ['🎯', 'Precisão', c.precisao, 'precisao'],
        ['🧿', 'Resistência', c.resistencia, 'resistencia'],
        // O crítico não entra na comparação: ele vem do TIPO e não anda com o nível.
        ['🎲', 'Taxa de crítico', `${c.taxaCritPct}%`, null],
        ['💥', 'Dano crítico', `${c.danoCritPct}%`, null],
    ];

    return [titulo('Estatísticas'), ...stats.map(([icone, rotulo, valor, campo]) => {
        const depois = campo && alvo ? alvo[campo] : null;
        const mudou = depois != null && depois !== valor;

        const linha = document.createElement('div');
        linha.className = 'apostoloStat' + (mudou ? ' subindo' : '');

        const ic = document.createElement('span'); ic.className = 'csIcone'; ic.textContent = icone;
        const rot = document.createElement('span'); rot.className = 'csRotulo'; rot.textContent = rotulo;
        const val = document.createElement('span'); val.className = 'csValor';
        val.textContent = mudou
            ? `${valor.toLocaleString('pt-BR')} → ${depois.toLocaleString('pt-BR')}`
            : valor;

        linha.append(ic, rot, val);
        return linha;
    })];
}

export function painelDeHabilidades(c) {
    return [titulo('Habilidades'), ...c.habilidades.map(h => {
        const card = document.createElement('div');
        card.className = 'apostoloHab' + (h.passiva ? ' passiva' : '');

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
    })];
}

function titulo(texto) {
    const h = document.createElement('h2');
    h.className = 'apostoloSecao';
    h.textContent = texto;
    return h;
}
