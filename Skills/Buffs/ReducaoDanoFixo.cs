using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Buff permanente que reduz o dano recebido em um percentual fixo.
    /// Aplicado por passivas como PassivaSereia (15%).
    /// 
    /// Como é aplicado no IniciarCombate (antes de qualquer outro buff), processa
    /// PRIMEIRO no foreach do ReceberDano — reduz o dano antes de Escudo/BloqueioTotal/etc.
    /// </summary>
    class ReducaoDanoFixo : Buff, IModificaDanoRecebido
    {
        public ReducaoDanoFixo(double percentual = 0.15)
            : base("Couraça", "🐚", int.MaxValue, percentual,
                $"-{percentual * 100:F0}% dano recebido.")
        { }

        public bool DeveAgir(NaturezaDano natureza) => true;
        public int ModificarDanoRecebido(Combate portador, int dano)
        {
            return (int)(dano * (1 - Valor));
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}