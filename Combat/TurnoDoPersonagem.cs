namespace ApostlesWar
{
    /// <summary>
    /// Dono do RELÓGIO de um turno de um combatente: as fases mecânicas de
    /// Início e Fim, que acontecem sem decisão (igual pra jogador e bot).
    ///
    /// Iniciar(): tick dos status (Veneno/Queima/CuraContinua agem aqui).
    /// Finalizar(): avança a duração dos status (remove expirados) e os cooldowns.
    ///
    /// NÃO é dono da AÇÃO (a "vontade": escolher/usar habilidade) — essa fica no
    /// CombateService, porque depende de UI/bot/seleção de alvo. O Turno é o
    /// relógio; o service orquestra a vontade em volta dele.
    ///
    /// Duração/cooldown NÃO moram aqui — moram no próprio StatusEffect e no
    /// SkillCooldown; pra eles o Turno é o MAESTRO (manda "passou o turno"), não o
    /// ARMAZÉM. Mas o estado TURN-SCOPED (o que nasce e morre no turno — hoje o
    /// registro de contra-ataques) mora AQUI: é persistente (um Turno por combatente,
    /// pro combate todo, via Combate.Turno), então é o dono natural desse estado.
    /// </summary>
    class TurnoDoPersonagem
    {
        private readonly Combate _combatente;

        // Agressores que este combatente já contra-atacou na janela do turno atual. Estado
        // TURN-SCOPED: nasce e morre no turno, por isso mora AQUI (o Turno é o dono do estado
        // de turno) — diferente de duração/cooldown, que são do combatente (persistem; o turno
        // só avança). Limpo no Finalizar.
        private readonly HashSet<Combate> _jaContraAtacou = new();

        public TurnoDoPersonagem(Combate combatente)
        {
            _combatente = combatente;
        }

        /// <summary>
        /// Regra ÚNICA de contra-ataque: "1x por agressor por turno" + chance. Decide se este
        /// combatente pode contra-atacar o agressor agora; registra em caso de sucesso (trava
        /// aquele agressor até o reset no Finalizar). chance 1.0 = sempre (se ainda não contra-atacou
        /// o agressor neste turno). Fonte única — o buff ContraAtaque E as passivas (Herói, Operário)
        /// passam TODAS por aqui (via a fachada Combate.TentarContraAtacar), então múltiplas fontes no
        /// mesmo combatente compartilham o mesmo limite (não somam contra-ataques).
        /// </summary>
        public bool TentarContraAtacar(Combate agressor, double chance)
        {
            if (_jaContraAtacou.Contains(agressor)) return false;
            if (Random.Shared.NextDouble() >= chance) return false;
            _jaContraAtacou.Add(agressor);
            return true;
        }

        /// <summary>
        /// Início do turno: dispara o tick dos status do combatente.
        /// (O evento InicioDoTurno das passivas é disparado pelo CombateService
        /// por enquanto — depende do sistema de passivas, que o C5 vai migrar.)
        /// </summary>
        public List<EventoCombate> Iniciar()
        {
            var eventos = new List<EventoCombate>();
            foreach (StatusEffect status in _combatente.StatusAtivos.ToList())
            {
                EventoCombate? ev = status.AoIniciarTurno(_combatente);
                if (ev != null) eventos.Add(ev);   // nulo morre na porta: a lista devolvida é sempre não-nula
            }
            return eventos;
        }

        /// <summary>
        /// Fim do turno: avança a duração dos status (removendo os expirados),
        /// avança os cooldowns das habilidades e limpa o registro de contra-ataques
        /// do turno. Três estados temporais diferentes, um único dono das transições
        /// (o Turno): duração e cooldown PERSISTEM entre turnos (o turno só avança);
        /// o registro de contra-ataques é do TURNO (nasce e morre no turno) — por isso
        /// é limpo, não avançado.
        /// </summary>
        public void Finalizar()
        {
            foreach (StatusEffect status in _combatente.StatusAtivos.ToList())
            {
                status.PassarTurno();
                if (status.Expirou)
                    status.Remover(_combatente);
            }

            foreach (var cd in _combatente.Cooldowns.Values)
                cd.PassarTurno();

            _jaContraAtacou.Clear();
        }
    }
}