// A BARRA DE QUANTIDADE — `0 ———[ 47 ]——— 150`.
//
// A regra do Gabriel: **sempre que for pra aumentar número, é esta peça**. Queimar alma e fundir
// alma já são duas; a próxima (raridade, forja de item) entra aqui em vez de nascer com botõezinhos
// de ×10 próprios. Por isso ela mora em `ui/` e não dentro de uma tela.
//
// O número fica ENTRE as pontas, sobre o trilho — não numa coluna à parte. Quem arrasta está
// olhando pro polegar, e é ali que a resposta tem de aparecer.

// `pontas` = { esquerda(v), direita(v) } — quem monta os dois cantos a partir do valor escolhido.
// A Oferenda usa isso pra pôr o fogo de ORIGEM num canto e o de DESTINO no outro, cada um com a
// própria contagem: ali as pontas não são "0 e o máximo", são "o que sai" e "o que entra".
export function barraDeQuantidade({ max, valor = 0, rotuloMax, aoMudar, pontas }) {
    const cont = document.createElement('div');
    cont.className = 'quantidade' + (max > 0 ? '' : ' vazia');

    const minimo = document.createElement('span');
    minimo.className = 'qPonta'; minimo.textContent = '0';

    const maximo = document.createElement('span');
    maximo.className = 'qPonta'; maximo.textContent = (max || 0).toLocaleString('pt-BR');
    if (rotuloMax) maximo.title = rotuloMax;

    const trilho = document.createElement('div');
    trilho.className = 'qTrilho';

    const barra = document.createElement('input');
    barra.type = 'range';
    barra.className = 'qBarra';
    barra.min = 0; barra.max = Math.max(max, 0); barra.step = 1; barra.value = valor;
    barra.disabled = max <= 0;

    const bolha = document.createElement('span');
    bolha.className = 'qBolha';

    trilho.append(barra, bolha);
    cont.append(minimo, trilho, maximo);

    function pintar() {
        const v = Number(barra.value);
        bolha.textContent = v.toLocaleString('pt-BR');
        // A bolha ANDA com o polegar. Sem isso ela ficaria no centro e o número deixaria de ser a
        // leitura do gesto — que é a coisa toda.
        bolha.style.left = `${max > 0 ? (v / max) * 100 : 0}%`;
        cont.classList.toggle('cheia', max > 0 && v >= max);

        if (pontas) {
            minimo.replaceChildren(pontas.esquerda(v));
            maximo.replaceChildren(pontas.direita(v));
        }
    }

    barra.addEventListener('input', () => { pintar(); aoMudar?.(Number(barra.value)); });
    pintar();

    return {
        el: cont,
        get valor() { return Number(barra.value); },
        definir(v) { barra.value = Math.max(0, Math.min(v, max)); pintar(); },
    };
}
