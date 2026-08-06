import { aplicarTema } from './ar.js';

// O ROTEADOR DE CENAS — quem está na tela, e o Esc que decide o que "voltar" significa.
//
// `cenaAtual` e `menuRaiz` são o único estado de navegação do front, e por isso moram aqui em vez
// de numa tela. Quem está fora lê pelas funções `cenaAgora()`/`menuEhRaiz()`: em ES module um
// `export let` é lido ao vivo mas NÃO é gravável de fora, e um objeto mutável compartilhado só
// ressuscitaria a variável global com outro nome. O incômodo de escrever `definirMenuRaiz` é o que
// faz alguém pensar duas vezes antes de criar a terceira travessia.

let cenaAtual = 'menu';      // cena atual (menu, combate, criarPerfil, arenaSetup, campanha*) — o Esc depende disto
let menuRaiz = true;         // o menu na tela é o PRINCIPAL? (decide o Esc: sair do jogo × voltar)

/// Quem está na tela agora. É função e não `export let` porque um binding exportado é lido ao vivo
/// mas NÃO é gravável de fora — e porque assim existe UM lugar que escreve (o `mostrarCena`).
export const cenaAgora = () => cenaAtual;

/// O menu na tela é o principal? Quem responde é o próprio menu, ao ser montado; quem pergunta é o
/// Esc, pra saber se "voltar" significa sair do jogo ou subir um nível.
export const menuEhRaiz = () => menuRaiz;
export const definirMenuRaiz = (ehRaiz) => { menuRaiz = ehRaiz; };

/// O que rodar DEPOIS de cada troca de cena. E gancho injetado pelo composition root, e nao
/// import: quem sabe o que significa "voltar" em cada tela conhece TODAS as telas, e o nucleo nao
/// pode conhecer nenhuma. Mesma razao do registro de cenarios.
let aoTrocar = () => {};
export const aoTrocarCena = (fn) => { aoTrocar = fn; };

// ---------- cenas (menu × combate × criar/editar perfil) ----------
export function mostrarCena(cena) {
    cenaAtual = cena;
    const emCombate = cena === 'combate';
    document.getElementById('menu').hidden = cena !== 'menu';
    document.getElementById('criarPerfil').hidden = cena !== 'criarPerfil';
    document.getElementById('editarPerfil').hidden = cena !== 'editarPerfil';
    document.getElementById('arenaSetup').hidden = cena !== 'arenaSetup';
    document.getElementById('campanhaMapa').hidden = cena !== 'campanhaMapa';
    document.getElementById('campanhaFases').hidden = cena !== 'campanhaFases';
    document.getElementById('fimDeFase').hidden = cena !== 'fimDeFase';
    document.getElementById('conquista').hidden = cena !== 'conquista';
    document.getElementById('arsenal').hidden = cena !== 'arsenal';
    document.getElementById('compendio').hidden = cena !== 'compendio';
    // A ficha do champ tem UMA seção e dois donos: o compêndio e a conquista (o champ recém-ganho
    // termina na própria ficha). Copiar o HTML pra ter duas telas iguais seria duas telas pra manter.
    document.getElementById('compendioChamp').hidden = !['compendioChamp', 'conquistaChamp'].includes(cena);
    document.getElementById('arena').hidden = !emCombate;
    document.getElementById('painel').hidden = !emCombate;
    // Os controles de combate só fazem sentido na batalha.
    document.getElementById('botoesTopo').style.visibility = emCombate ? 'visible' : 'hidden';
    document.getElementById('turno').style.visibility = emCombate ? 'visible' : 'hidden';
    // O overlay de fim só existe em combate; ao trocar de cena garante que sumiu.
    if (!emCombate) document.getElementById('fimBatalha').hidden = true;
    // O tema é do CAMPO DE BATALHA: fora dele, nenhum. Quem o LIGA é o desenhar (precisa do estado
    // novo); aqui só se garante que ele não vaze pras telas de menu.
    if (!emCombate) aplicarTema('');
    aoTrocar();
}

/// Abre uma tela. É por aqui que TODO mundo abre — a mensagem do C# e também o código que abre uma
/// tela por conta (a ficha do champ que a conquista mostra depois do duplo-clique). Uma tela
/// convertida ao contrato não tem mais uma função `mostrarX` pra chamar, e um caminho de abertura
/// paralelo é como o duplo-clique da conquista parou de funcionar sem nada acusar.
///
/// Os dois parâmetros extras existem porque o jogo os pediu, não por simetria:
///   `cena`     — a MESMA tela pode aparecer em cenas diferentes. A ficha do champ é a mesma pelo
///                compêndio e pela conquista, mas a cena muda (`compendioChamp` × `conquistaChamp`)
///                porque o Esc tem de voltar pra lugares diferentes.
///   `anterior` — de onde se veio, entregue ao `montar`. Sem isto, uma tela que pergunta "eu já
///                estava aqui?" recebe sempre "sim": quem troca a cena é esta função, ANTES do
///                montar, e foi assim que o arsenal parou de zerar o slot aberto.
export function abrirTela(tela, dados, cena = tela.cena) {
    const anterior = cenaAgora();
    mostrarCena(cena);
    tela.montar(dados, anterior);
}
