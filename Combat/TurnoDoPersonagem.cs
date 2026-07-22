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
    /// NÃO guarda o estado de duração/cooldown — esses moram no próprio
    /// StatusEffect e no SkillCooldown. O Turno é o MAESTRO (manda "passou o
    /// turno"), não o ARMAZÉM.
    /// </summary>
    class TurnoDoPersonagem
    {
        private readonly Combate _combatente;

        public TurnoDoPersonagem(Combate combatente)
        {
            _combatente = combatente;
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

            _combatente.LimparContraAtaques();
        }
    }
}