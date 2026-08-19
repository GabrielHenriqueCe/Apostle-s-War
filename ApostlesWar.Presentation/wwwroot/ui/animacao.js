// As animações que qualquer tela pode pedir: reiniciar uma classe, cuspir um número, e contar.

// O CONTADOR de cronômetro: escreve os números entre `de` e `ate` ao longo de `ms`.
//
// Escreve o `de` na hora e só então agenda o resto — assim a tela nunca aparece em branco esperando
// o primeiro quadro, e num ambiente sem requestAnimationFrame (o harness) ela fica no valor inicial
// em vez de quebrar.
//
// A desaceleração é a mesma da barra: os dois animam o MESMO ganho, e ritmos diferentes fariam o
// número chegar antes ou depois do trilho encher.
export function contar(el, de, ate, ms, escrever = (v) => `${v}`) {
    el.textContent = escrever(de);
    if (de === ate || ms <= 0) { el.textContent = escrever(ate); return Promise.resolve(); }

    return new Promise(resolve => {
        const inicio = performance.now();
        const passo = (agora) => {
            const t = Math.min((agora - inicio) / ms, 1);
            el.textContent = escrever(Math.round(de + (ate - de) * suavizar(t)));
            if (t < 1) requestAnimationFrame(passo);
            else resolve();
        };
        requestAnimationFrame(passo);
    });
}

// Começa rápido e freia no fim (ease-out). É o que faz o número "assentar" em vez de estancar.
export const suavizar = (t) => 1 - Math.pow(1 - t, 3);

// Espera `ms`, como promessa — pra encadear trecho após trecho sem aninhar setTimeout.
export const esperar = (ms) => new Promise(r => setTimeout(r, ms));


// Reinicia a animação mesmo se a classe já estiver lá (dois golpes seguidos no mesmo alvo).
export function reanimar(el, classe) {
    el.classList.remove(classe);
    void el.offsetWidth;
    el.classList.add(classe);
    setTimeout(() => el.classList.remove(classe), 520);   // acompanha o .5s do @keyframes tremer
}

// O número é CUSPIDO pra fora da caixa, na direção do seu próprio lado — a direita é o espelho
// da esquerda. O mesmo apóstolo pode lutar dos dois lados, então a animação não pode ter "lado
// certo": ela deriva de onde a caixa está, não de quem é o personagem.
export function flutuar(el, texto, classe) {
    const n = document.createElement('span');
    n.className = `flutuante ${classe}`;
    n.textContent = texto;

    const sentido = el.parentElement?.id === 'ladoDireito' ? 1 : -1;
    // Embaralha distância e giro pra dois golpes seguidos não empilharem no mesmo ponto.
    n.style.setProperty('--dx', `${sentido * (34 + Math.random() * 30)}px`);
    n.style.setProperty('--giro', `${sentido * (4 + Math.random() * 8)}deg`);

    el.appendChild(n);
    setTimeout(() => n.remove(), 1250);
}

