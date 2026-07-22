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
    /// ARMAZÉM. Mas o estado TURN-SCOPED (o que nasce e morre no turno — os orçamentos
    /// de reação "1x por agressor": contra-ataque + reações de veneno) mora AQUI: é
    /// persistente (um Turno por combatente, pro combate todo, via Combate.Turno), então
    /// é o dono natural desse estado.
    /// </summary>
    class TurnoDoPersonagem
    {
        private readonly Combate _combatente;

        // Orçamento de reação "1x por agressor por turno", POR CHAVE de reação. Estado TURN-SCOPED
        // (nasce e morre no turno), por isso mora AQUI (o Turno é o dono do estado de turno) —
        // diferente de duração/cooldown, que são do combatente (persistem; o turno só avança).
        // Limpo no Finalizar. A chave separa orçamentos: o contra-ataque usa UMA chave compartilhada
        // (todas as suas fontes somam num limite só); cada reação de veneno usa a própria (GetType()).
        private readonly Dictionary<object, HashSet<Combate>> _jaReagiu = new();

        // Chave compartilhada do contra-ataque (buff ContraAtaque + Herói + Operário no mesmo limite).
        private static readonly object ChaveContraAtaque = new();

        public TurnoDoPersonagem(Combate combatente)
        {
            _combatente = combatente;
        }

        /// <summary>
        /// Orçamento de reação "1x por agressor por turno" + chance, POR CHAVE. Decide se a reação
        /// identificada por `chave` pode disparar contra `agressor` agora; registra no sucesso (trava
        /// aquele agressor naquela chave até o reset no Finalizar). chance 1.0 = sempre (se ainda não
        /// reagiu). Fontes que passam a MESMA chave compartilham o limite.
        ///
        /// As DUAS frequências são FIRST-CLASS: quem quer "1x por agressor" chama isto (OPT-IN); quem
        /// quer "1x por hit" NÃO chama — dispara direto na própria reação (com o próprio roll de chance
        /// se tiver). Regra: só se cria "método/gate" quando há ESTADO a guardar — aqui há (o registro);
        /// o por-hit não tem estado, então não vira método.
        /// </summary>
        public bool TentarReagir(object chave, Combate agressor, double chance)
        {
            if (!_jaReagiu.TryGetValue(chave, out var jaReagidos))
            {
                jaReagidos = new HashSet<Combate>();
                _jaReagiu[chave] = jaReagidos;
            }
            if (jaReagidos.Contains(agressor)) return false;
            if (Random.Shared.NextDouble() >= chance) return false;
            jaReagidos.Add(agressor);
            return true;
        }

        /// <summary>
        /// Contra-ataque: "1x por agressor por turno" + chance. Fonte única — buff ContraAtaque E as
        /// passivas (Herói, Operário) passam TODAS por aqui (via a fachada Combate.TentarContraAtacar),
        /// compartilhando a MESMA chave, então múltiplas fontes no mesmo combatente somam num limite só.
        /// </summary>
        public bool TentarContraAtacar(Combate agressor, double chance)
            => TentarReagir(ChaveContraAtaque, agressor, chance);

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
        /// avança os cooldowns das habilidades e limpa os orçamentos de reação
        /// do turno. Três estados temporais diferentes, um único dono das transições
        /// (o Turno): duração e cooldown PERSISTEM entre turnos (o turno só avança);
        /// os orçamentos de reação são do TURNO (nascem e morrem no turno) — por isso
        /// são limpos, não avançados.
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

            _jaReagiu.Clear();
        }
    }
}