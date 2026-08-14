// MENU — a lista de opções que o C# manda, e o perfil do jogador no canto.
//
// O `perfilAvatar` mora aqui porque é o menu que o recebe do C#. O mapa da campanha lê pelo
// `avatarDoJogador()`: o marcador que caminha na trilha É o avatar do jogador, e um `export let`
// seria lido ao vivo mas não gravável de fora.

import { definirMenuRaiz } from '../nucleo/cena.js';
import { mandar } from '../nucleo/ponte.js';
import { confirmar } from '../ui/modal.js';

let perfilAvatar = '🧭';   // avatar do jogador; vira o marcador que caminha no mapa

/// O avatar do jogador. Quem lê é o mapa da campanha, e é função pelo mesmo motivo do cenaAgora():
/// binding exportado nao e gravavel de fora, e um so lugar escreve.
export const avatarDoJogador = () => perfilAvatar;

export const menu = {
    cena: 'menu',
    montar(m) {
        definirMenuRaiz(!!m.raiz);
        document.getElementById('menuTitulo').textContent = m.titulo;
        document.getElementById('menuSubtitulo').textContent = m.subtitulo || '';

        // Perfil no canto (avatar + nome): só o menu principal manda. Editar é pelo botão ✏️, não pelo avatar.
        const perfil = document.getElementById('menuPerfil');
        perfil.hidden = !m.avatar;
        if (m.avatar) {
            perfilAvatar = m.avatar;
            document.getElementById('menuAvatar').textContent = m.avatar;
            document.getElementById('menuNome').textContent = m.nome || '';
        }

        const cont = document.getElementById('menuOpcoes');
        cont.replaceChildren(...m.opcoes.map((o, i) => {
            const b = document.createElement('button');
            b.type = 'button';
            b.className = 'opcaoMenu';
            b.disabled = !o.habilitado;

            const ic = document.createElement('span'); ic.className = 'opIcone'; ic.textContent = o.icone;
            const rot = document.createElement('span'); rot.className = 'opRotulo'; rot.textContent = o.rotulo;
            b.append(ic, rot);
            if (!o.habilitado) {
                const em = document.createElement('span'); em.className = 'opEmBreve'; em.textContent = 'em breve';
                b.appendChild(em);
            }
            // Opção-INTERRUPTOR (marcado != null): mostra em que estado ela está. O C# manda o estado e
            // redesenha o menu depois de alternar — a tela não guarda nada, como o botão do auto.
            if (o.marcado != null) {
                const marca = document.createElement('span');
                marca.className = 'opMarca' + (o.marcado ? ' ligado' : '');
                marca.textContent = o.marcado ? '✓' : '—';
                b.appendChild(marca);
            }

            // Opção destrutiva (ex: excluir conta): confirma no modal ANTES de mandar a escolha.
            b.addEventListener('click', () => o.confirmar
                ? confirmar(o.confirmar, () => mandar('menuEscolha', i))
                : mandar('menuEscolha', i));
            return b;
        }));
},
};
