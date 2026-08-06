// Apostle's War — a tela.
//
// Contrato com o C#: recebemos ESTADO (retrato completo: como tudo está agora) e EVENTOS (o que
// acabou de acontecer). O estado redesenha; o evento anima. Essa separação é o que permite a tela
// ser burra — ela nunca calcula regra de jogo, só pinta o que chegou.
//
// Fluxo de clique (desenho do Gabriel): clica na habilidade -> clica no inimigo -> USA.
// Sem habilidade escolhida, clicar num personagem só INSPECIONA (mostra ficha e status).

'use strict';

import * as reino from './cenarios/reino/reino.js';
import * as ladosombrio from './cenarios/ladosombrio/ladosombrio.js';
import * as tecnologicos from './cenarios/tecnologicos/tecnologicos.js';
import * as folclore from './cenarios/folclore/folclore.js';
import * as misticos from './cenarios/misticos/misticos.js';
import * as especial from './cenarios/especial/especial.js';
import * as decaidos from './cenarios/decaidos/decaidos.js';
import * as apostolos from './cenarios/apostolos/apostolos.js';

import { abrirTela, aoTrocarCena, cenaAgora, menuEhRaiz, mostrarCena } from './nucleo/cena.js';
import { aplicarTema, registrarCenarios } from './nucleo/ar.js';
import { mandar, ponte } from './nucleo/ponte.js';
import { confirmar, fecharModal, modalAberto } from './ui/modal.js';

import { menu } from './telas/menu.js';
import { criarPerfil, edicaoPerfil } from './telas/perfil.js';
import { montagemArena } from './telas/arena.js';
import { campanhaFases, campanhaMapa, conquista, fimDeFase, fimDeFaseTemOpcoes } from './telas/campanha.js';
import { arsenal } from './telas/arsenal.js';
import { compendio, compendioChamp } from './telas/compendio.js';
import { aplicarEstado, aplicarEvento, aplicarVelocidade, desarmar, desenhar, estadoAtual, nomeDaFase } from './telas/combate.js';

export { aplicarTema };   // o seam por onde o harness entra (ferramentas/rodar-tema.js)




// ---------- recepção: o INTERPRETADOR de telas ----------
//
// O CONTRATO. Toda tela é um objeto com dois campos, e nada além disso:
//
//     export const compendio = {
//         cena: 'compendio',      // qual seção do index.html fica visível
//         montar(dados) { ... },  // preenche o DOM com o que o C# mandou
//     };
//
// A chave no mapa abaixo é o `tipo` da mensagem — a unidade é a MENSAGEM, não o arquivo: o
// compêndio exporta duas telas, porque o C# manda duas mensagens.
//
// É o mesmo desenho do ADR-composicao-de-acoes: a habilidade virou DADO rodada por um interpretador
// único, com zero `Ativar` override. Aqui a tela virou dado e o laço abaixo é o interpretador. Antes
// isto era uma escada de 13 `else if`, e a escada é o que permite treze jeitos diferentes de fazer a
// mesma coisa — que é exatamente a origem do "funciona nesta tela e não naquela".
//
// Tela nova = uma linha aqui, igual capítulo novo virou uma linha no CENARIOS.
const TELAS = {
    menu,
    criarPerfil, edicaoPerfil,
    montagemArena,
    campanhaMapa, campanhaFases, fimDeFase, conquista,
    arsenal,
    compendio, compendioChamp,
};


// AS DUAS QUE NÃO SÃO TELA, e ficam de fora de propósito (o "Nível 3" do ADR — quando a coisa não é
// do formato, isso se declara em vez de se torcer): `estado` e `evento` não NAVEGAM, atualizam a
// cena que já está no ar. Enfiá-las no mapa exigiria um `cena` mentiroso e um `montar` que não monta.
ponte.addEventListener('message', e => {
    let msg;
    try { msg = typeof e.data === 'string' ? JSON.parse(e.data) : e.data; }
    catch { return; }

    const tela = TELAS[msg.tipo];
    if (tela) { abrirTela(tela, msg.conteudo); return; }

    if (msg.tipo === 'estado') aplicarEstado(msg.conteudo);
    else if (msg.tipo === 'evento') aplicarEvento(msg.conteudo);
    // TEMPORÁRIO: as telas ainda não convertidas ao contrato. Cada uma que migrar some daqui e
    // aparece no mapa acima.
});














// A ficha do champ sai por CLIQUE ou Enter, além do Esc/X — vale nos dois donos da seção (o
// compêndio e a conquista), porque é a mesma tela e não deve ter dois jeitos de fechar.
document.getElementById('compendioChamp').addEventListener('click', sairDaTela);




// ---------- sair da tela ----------
// UMA função pra "voltar um nível", e os dois gestos que a disparam: a tecla Esc e o botão 🚪 Sair
// do canto superior direito. Antes cada tela tinha o próprio "Voltar" no rodapé, cada um num lugar
// diferente, e a tecla era a única coisa consistente — agora o botão é o espelho VISÍVEL dela.
//
// O que "um nível" quer dizer muda com a cena, e é aqui que a tabela mora:
//  - modal aberto → cancela o modal (ele é o nível mais raso)
//  - criar perfil → nada (não se sai no meio de digitar o nome; o botão fica escondido)
//  - fim de batalha / vitória / derrota → segue pro menu
//  - submenu e telas de conteúdo → 'voltar' (o C# desempilha; ver LerEscolha)
//  - menu raiz → confirma sair do JOGO, que é o único nível acima
//  - batalha → desarma o que estiver armado; sem nada armado, confirma sair da luta
function sairDaTela() {
    if (modalAberto) { fecharModal(); return; }
    if (cenaAgora() === 'criarPerfil') return;

    if (cenaAgora() === 'combate' && nomeDaFase(estadoAtual() || {}) === 'Fim') { mandar('voltarMenu'); return; }

    // Fim de fase: com as opções à mostra, sair é a decisão "Sair" (que faz o mesmo que Editar
    // Equipe — sair desta tela É voltar pra montagem). Na passagem da recompensa, sair é seguir.
    if (cenaAgora() === 'fimDeFase') { mandar(fimDeFaseTemOpcoes() ? 'voltar' : 'continuar'); return; }

    // A conquista e a ficha dela: sair fecha o champ e devolve o comando ao C#, que segue pro
    // próximo champ novo ou pra tela de vitória.
    if (cenaAgora() === 'conquista' || cenaAgora() === 'conquistaChamp') { mandar('continuar'); return; }

    if (cenaAgora() === 'menu') {
        if (menuEhRaiz()) confirmar('Sair do jogo?', () => mandar('sairDoJogo'));
        else mandar('voltar');
        return;
    }

    if (cenaAgora() !== 'combate') { mandar('voltar'); return; }

    if (desarmar()) { desenhar(); return; }

    // Na campanha, desistir NÃO é sair do jogo: conta derrota e cai na tela de fim de fase, de onde
    // dá pra tentar de novo. Na Arena não há desfecho nenhum, então sair é sair. O rótulo e o texto
    // seguem a consequência — duas coisas diferentes não podem ter o mesmo nome.
    if (estadoAtual()?.modo === 'campanha')
        confirmar('Encerrar a batalha? Conta como DERROTA nesta fase.', () => mandar('sair'));
    else
        confirmar('Sair da batalha? O progresso desta luta será perdido.', () => mandar('sair'));
}

// O botão só some onde sair não é opção (criar perfil). Nas demais ele existe SEMPRE no mesmo pixel —
// é isso que o torna aprendível.
// O X é sempre o mesmo desenho no mesmo pixel — o que muda é o que ele PROMETE, e isso vive no
// title (e no texto do modal). Na batalha da campanha ele encerra a luta em derrota e a tela de fim
// aparece: o jogo continua, então "sair" seria mentira.
function atualizarBotaoSair() {
    const b = document.getElementById('sairTela');
    b.hidden = cenaAgora() === 'criarPerfil';

    const encerrando = cenaAgora() === 'combate' && estadoAtual()?.modo === 'campanha'
        && nomeDaFase(estadoAtual() || {}) !== 'Fim';
    b.title = encerrando ? 'Encerrar a batalha (Esc)' : 'Sair desta tela (Esc)';
}

document.getElementById('sairTela').addEventListener('click', sairDaTela);

document.addEventListener('keydown', e => {
    // Recarregar a página MATA a partida: o JS volta do zero, mas a thread do jogo no C# continua
    // parada esperando um clique que nunca vem. O WebView2 já está com os atalhos de navegador
    // desligados (ver AppFront); isto aqui é o cinto além do suspensório — a tecla não pode passar
    // por caminho nenhum.
    if (e.key === 'F5' || ((e.ctrlKey || e.metaKey) && (e.key === 'r' || e.key === 'R'))) {
        e.preventDefault();
        return;
    }

    // Nas telas de PASSAGEM o Enter também segue em frente (é o gesto natural de "ok, continuar").
    // A tela de fim de fase COM opções fica de fora de propósito: ali cada botão faz uma coisa
    // diferente, e o Enter escolheria uma delas por conta própria.
    const passagem = (cenaAgora() === 'combate' && nomeDaFase(estadoAtual() || {}) === 'Fim')
        || (cenaAgora() === 'fimDeFase' && !fimDeFaseTemOpcoes())
        || cenaAgora() === 'conquista' || cenaAgora() === 'conquistaChamp'
        || cenaAgora() === 'compendioChamp';

    if (e.key === 'Escape' || (passagem && e.key === 'Enter')) sairDaTela();
});


// ---------- tema do campo de batalha ----------
// Cada capítulo pode ter o próprio cenário. O tema entra como `data-tema` no <body> e o CSS faz o
// resto — a ESTRUTURA da luta não muda, só a pele. É o mesmo princípio do painel de habilidades:
// uma tela, uma forma; o que varia é a roupa.
//
// Aqui em cima mora só o que o CSS não sabe fazer: as partículas do ar. Um capítulo sem entrada
// nesta tabela fica sem partículas e ainda assim ganha a pele do CSS, se ela existir — as duas
// metades do tema são independentes de propósito, pra nenhuma delas exigir a outra.
// O AR de cada cenário, em três camadas OPCIONAIS e independentes — um tema usa as que fizerem
// sentido pra ele e ignora o resto:
//   pó      · partículas pequenas. `subida` em px/s: positivo SOBE, negativo CAI. A direção é o
//             SINAL da velocidade, não um campo à parte que se pode esquecer de casar com ela.
//   névoa   · manchas grandes e translúcidas passeando devagar, coladas no chão.
//   voadores· bichos atravessando a tela, com asas.
// O REGISTRO dos capitulos. Cada valor e o MODULO do cenario, nao a configuracao dele — quem sabe
// montar a cena e o capitulo, e o nucleo so pede. Capitulo sem entrada aqui luta no visual padrao.
export const CENARIOS = {
    reino,
    ladosombrio,
    tecnologicos,
    folclore,
    misticos,
    especial,
    decaidos,
    apostolos,
};

// ---------- partida ----------

// A INJEÇÃO, e ela vem antes de qualquer coisa desenhar: é aqui que o núcleo recebe as peças
// concretas que ele não pode conhecer por import. Mesma ideia do Program.cs no back.
registrarCenarios(CENARIOS);        // quais capítulos existem e como cada um monta a cena
aoTrocarCena(atualizarBotaoSair);   // o que fazer depois de trocar de tela

aplicarVelocidade();      // sincroniza o C# com o 2x inicial
mostrarCena('menu');      // o jogo sempre abre no menu — evita o flash da arena vazia
mandar('pronto');         // destrava a thread do jogo no C#


















