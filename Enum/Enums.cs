namespace ApostlesWar
{
    #region Enums

    // Facções disponíveis no jogo
    enum Faccao { Humanos, Reino, LadoSombrio, Tecnologicos, Folclore, Misticos, Especial, Decaidos, Apostolos }
    // Posição do personagem dentro da facção
    enum Slot { Slot1 = 1, Slot2 = 2, Slot3 = 3, Slot4 = 4 }
    // Fases de cada capítulo: Arma, Elmo, Escudo, Braceletes, Armadura, Botas, Raro
    enum Fases { Fase1 = 1, Fase2 = 2, Fase3 = 3, Fase4 = 4, Fase5 = 5, Fase6 = 6, Fase7 = 7 }
    
    enum EventoCombate { AntesDeReceberDano, DepoisDeReceberDano }

    #endregion
}

