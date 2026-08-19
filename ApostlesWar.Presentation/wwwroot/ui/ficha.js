// A FICHA de um apóstolo — os dois painéis que o Compêndio e o Arsenal desenham igual.
//
// Devolvem ARRAYS de elementos em vez de escrever num id: as duas telas têm molduras diferentes
// (o compêndio abre em tela cheia, a Catedral a põe na coluna do meio) e só o conteúdo é o mesmo.

// A ordem é a da tabela do GDD §2, pra conferir a ficha contra o doc de cima a baixo.
//
// `alvo` = a ficha do nível de DESTINO, quando o jogador está montando uma evolução. Aí a linha
// que muda vira `1.240 → 1.380` no próprio lugar dela, em vez de um segundo bloco embaixo: eram
// duas listas dizendo do mesmo apóstolo, e a de baixo era a que não cabia na tela.
//
// `abrirBonus` = mostra a CONTA do equipamento (`200 +58 = 258`, em colunas alinhadas) em vez de só
// o resultado. Liga na Catedral, que é onde se está montando o boneco; em toda outra tela o jogador
// quer o número final e mais nada. Antes disso o arsenal tinha um painel de totais à parte, e ele
// mostrava "+5%" sem dizer 5% de quê.
export function painelDeStats(c, alvo = null, abrirBonus = false) {
    const b = c.bonus || {};
    const stats = [
        ['❤️', 'HP', c.hp, 'hp', b.hp],
        ['⚔️', 'Ataque', c.ataque, 'ataque', b.ataque],
        ['🛡️', 'Defesa', c.defesa, 'defesa', b.defesa],
        ['⚡', 'Velocidade', c.velocidade, 'velocidade', b.velocidade],
        ['🎯', 'Precisão', c.precisao, 'precisao', b.precisao],
        ['🧿', 'Resistência', c.resistencia, 'resistencia', b.resistencia],
        // O crítico não entra na comparação de NÍVEL: ele vem do TIPO e não anda com o nível. Mas
        // anda com item (a Manopla), então o bônus dele aparece igual.
        ['🎲', 'Taxa de crítico', c.taxaCritPct, null, b.taxaCritPct, '%'],
        ['💥', 'Dano crítico', c.danoCritPct, null, b.danoCritPct, '%'],
    ];

    return [titulo('Estatísticas'), ...stats.map(([icone, rotulo, base, campo, bonus, sufixo = '']) => {
        const depois = campo && alvo ? alvo[campo] : null;
        const mudou = depois != null && depois !== base;
        const ganho = bonus || 0;

        // A prévia de evolução e a conta do equipamento disputam a MESMA coluna, então a prévia
        // ganha e a linha volta pra grade de três colunas — senão o `1.240 → 1.380` cairia na
        // coluna estreita do "base" e vazaria por cima do resto.
        const conta = abrirBonus && !mudou;

        const linha = document.createElement('div');
        linha.className = 'apostoloStat' + (mudou ? ' subindo' : '') + (conta ? ' comConta' : '');

        const ic = document.createElement('span'); ic.className = 'csIcone'; ic.textContent = icone;
        const rot = document.createElement('span'); rot.className = 'csRotulo'; rot.textContent = rotulo;
        linha.append(ic, rot);

        const escrever = n => n.toLocaleString('pt-BR') + sufixo;

        if (mudou) {
            const val = document.createElement('span'); val.className = 'csValor';
            val.textContent = `${escrever(base)} → ${escrever(depois)}`;
            linha.append(val);
            return linha;
        }

        if (conta) {
            // O `+` e o `=` são os EIXOS da leitura, então cada um tem COLUNA PRÓPRIA em vez de vir
            // grudado no número. Escrito como `+58` o sinal viaja junto com o texto — em `+58` e
            // `+1.265` ele cai em lugares diferentes, e a coluna inteira serrilha. Separado, ele
            // fica cravado, e os números continuam alinhados à direita dentro dos deles.
            //
            // As cinco colunas existem mesmo com bônus ZERO (os dois sinais somem, o espaço fica):
            // uma linha sem bônus que colapsasse puxaria o total pra esquerda e quebraria a régua.
            const cru = document.createElement('span'); cru.className = 'csBase'; cru.textContent = escrever(base);
            const mais = document.createElement('span'); mais.className = 'csMais'; mais.textContent = ganho ? '+' : '';
            const bon = document.createElement('span'); bon.className = 'csBonus'; bon.textContent = ganho ? escrever(ganho) : '';
            const igual = document.createElement('span'); igual.className = 'csIgual'; igual.textContent = '=';
            const tot = document.createElement('span'); tot.className = 'csValor'; tot.textContent = escrever(base + ganho);
            linha.append(cru, mais, bon, igual, tot);
            return linha;
        }

        // Fora da Catedral vai o número DELE e mais nada — sem o item somado por baixo. O equipamento
        // é global hoje e apareceria igual em todo apóstolo do compêndio, inclusive nos que você nem
        // conquistou: viraria um número que não é de ninguém.
        const val = document.createElement('span'); val.className = 'csValor';
        val.textContent = escrever(base);
        linha.append(val);
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
