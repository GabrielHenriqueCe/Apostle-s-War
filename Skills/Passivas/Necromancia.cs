using ApostlesWar;
using System;

namespace v1_Apostle_s_War.Skills.Passivas
{
    #region Necromancia

    class Necromancia : HabilidadePassiva
    {
        public Necromancia() : base("Necromancia", "🪦", 6, "Revive o personagem com 50% do HP máximo ao morrer.") { }
        public override bool Revive() => true;
        public override bool DeveAtivar(EventoCombate evento) => evento == EventoCombate.DepoisDeReceberDano;
        public override string MensagemSobreviveu(Personagem personagem) => $"{personagem.Simbolo} {personagem.Nome} foi ressuscitado pela Necromancia!";
        public override string MensagemMorreu(Personagem personagem) => $"{personagem.Simbolo} {personagem.Nome} caiu em batalha e não pode ser ressuscitado.";
        public override void Ativar(Combate alvo, List<Combate>? aliados = null)
        {
            if (alvo.HPAtual <= 0)
                alvo.Reviver(alvo.HPMaximo / 2);
        }
    }

    #endregion
}