// PERFIL — criar na 1ª vez (só o nome) e editar depois (nome + avatar).
//
// Duas TELAS, porque são duas mensagens: o C# manda `criarPerfil` quando não há save e
// `edicaoPerfil` quando o jogador pede pra mudar.

import { mandar } from '../nucleo/ponte.js';

// ---------- criar perfil (1ª vez) ----------
export const criarPerfil = {
    cena: 'criarPerfil',
    montar(dados) {
        const input = document.getElementById('nomePerfil');
        input.value = '';
        input.focus();
},
};

function enviarPerfil() {
    const nome = document.getElementById('nomePerfil').value.trim();
    if (!nome) return;   // sem nome, não começa
    mandar('criarPerfil', 0, nome);
}

document.getElementById('confirmarPerfil').addEventListener('click', enviarPerfil);
document.getElementById('nomePerfil').addEventListener('keydown', e => {
    if (e.key === 'Enter') enviarPerfil();
});

// ---------- editar perfil (nome + avatar) ----------
// A entidade avatar+nome no menu é o gatilho inteiro.
document.getElementById('menuPerfil').addEventListener('click', () => mandar('editarPerfil'));

let avatarSelecionado = -1;   // índice na grade (= índice na lista completa que o C# mandou)

export const edicaoPerfil = {
    cena: 'editarPerfil',
    montar(dados) {
        // O nome começa TRAVADO (display); só o botão ✏️ destrava pra editar.
        const nomeInput = document.getElementById('nomeEditar');
        nomeInput.value = dados.nome || '';
        nomeInput.readOnly = true;

        const grade = document.getElementById('avatarGrade');
        avatarSelecionado = -1;

        grade.replaceChildren(...dados.campeoes.map((c, i) => {
            const cel = document.createElement('div');
            cel.className = 'avatarCelula' + (c.desbloqueado ? '' : ' bloqueado');

            const em = document.createElement('span'); em.className = 'aEmoji'; em.textContent = c.simbolo;
            const nm = document.createElement('span'); nm.className = 'aNome'; nm.textContent = c.nome;
            cel.append(em, nm);

            // Pré-seleciona o avatar atual (se desbloqueado).
            if (c.desbloqueado && c.simbolo === dados.avatar && avatarSelecionado === -1) {
                avatarSelecionado = i;
                cel.classList.add('selecionado');
            }
            if (c.desbloqueado) cel.addEventListener('click', () => selecionarAvatar(i));
            return cel;
        }));

        // Avatar atual não encontrado entre os desbloqueados: cai no 1º desbloqueado.
        if (avatarSelecionado === -1) {
            const i = dados.campeoes.findIndex(c => c.desbloqueado);
            if (i >= 0) selecionarAvatar(i);
        }
},
};

// ✏️ ao lado do nome: destrava a edição (não editável ao abrir).
document.getElementById('editarNomeBtn').addEventListener('click', () => {
    const nomeInput = document.getElementById('nomeEditar');
    nomeInput.readOnly = false;
    nomeInput.focus();
    nomeInput.select();
});

function selecionarAvatar(i) {
    avatarSelecionado = i;
    const celulas = document.getElementById('avatarGrade').children;
    for (let k = 0; k < celulas.length; k++) celulas[k].classList.toggle('selecionado', k === i);
}

function salvarEdicao() {
    const nome = document.getElementById('nomeEditar').value.trim();
    if (!nome || avatarSelecionado < 0) return;
    mandar('salvarPerfil', avatarSelecionado, nome);
}

document.getElementById('salvarEditar').addEventListener('click', salvarEdicao);
document.getElementById('nomeEditar').addEventListener('keydown', e => {
    if (e.key === 'Enter') salvarEdicao();
});
