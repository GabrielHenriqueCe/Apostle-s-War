namespace ApostlesWar.App.Front
{
    /// <summary>
    /// O RETRATO da partida que vai pro JS desenhar. É o coração do contrato da ponte: o front recebe
    /// ESTADO (o que é verdade agora) e desenha do zero, em vez de receber ordens de desenho. Por isso
    /// trocar emoji por sprite depois não toca no motor — muda só quem lê este retrato.
    ///
    /// São records simples de propósito: viram JSON direto (System.Text.Json) e nenhum objeto vivo do
    /// domínio (Combate, Habilidade) atravessa a ponte. Isso é o mesmo espírito do "EventoDano por ID"
    /// registrado no ROADMAP — o front não segura referência pro motor, só um espelho.
    /// </summary>
    internal record EstadoDeBatalha(
        int Turno,
        FaseDaTela Fase,
        List<CombatenteVisto> Equipe1,
        List<CombatenteVisto> Equipe2,
        int? QuemAge,                       // índice global (Id) de quem está agindo — o contorno verde
        List<HabilidadeVista> Habilidades,  // as do QuemAge, quando é a vez de um humano
        List<int> AlvosValidos,             // Ids clicáveis agora (vazio = ninguém)
        string? Mensagem                    // linha de narração (uso de habilidade, passiva...)
    );

    /// <summary>Em que ponto do turno a tela está — o JS usa pra saber o que é clicável.</summary>
    internal enum FaseDaTela
    {
        Assistindo,        // bot agindo / animação rolando: nada clicável
        EscolhendoAcao,    // humano escolhe habilidade
        EscolhendoAlvo,    // habilidade escolhida, falta o alvo
        Fim                // batalha acabou
    }

    /// <summary>
    /// Um combatente como a tela o vê. Traz os números TODOS (ATK/DEF/crit) porque hoje o Gabriel
    /// está testando balance e precisa enxergar tudo; o botão de esconder vive no front, não aqui —
    /// mandar o dado e deixar a tela decidir o que mostrar é mais barato que ter dois formatos.
    /// </summary>
    internal record CombatenteVisto(
        int Id,
        string Nome,
        string Simbolo,
        int HPAtual,
        int HPMaximo,
        int Escudo,
        int Ataque,
        int Defesa,
        int TaxaCritPct,
        int DanoCritPct,
        bool Vivo,
        List<StatusVisto> Status
    );

    internal record StatusVisto(string Nome, string Simbolo, int DuracaoRestante, bool EhBuff);

    internal record HabilidadeVista(
        int Indice,
        string Nome,
        string Simbolo,
        string Descricao,
        int CooldownRestante,
        bool Disponivel
    );

    /// <summary>
    /// Um acontecimento pra ANIMAR (dano, cura, morte). Vai num canal separado do estado de propósito:
    /// o estado diz "como as coisas estão", o evento diz "o que acabou de acontecer" — é o segundo que
    /// faz o número pular e o alvo tremer. O motor já emite EventoDano/EventoCura desde o #7b; aqui só
    /// viram formato de tela.
    /// </summary>
    internal record EventoVisto(
        string Tipo,        // "dano" | "cura" | "morte" | "narracao"
        int? AlvoId,
        int Valor,
        bool Critico,
        int AbsorvidoPeloEscudo,
        string? Texto
    );
}
