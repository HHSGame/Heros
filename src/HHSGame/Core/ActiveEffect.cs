
using HHSGame.Utils;

namespace HHSGame.Core
{

    public abstract class ActiveEffect(int duration)
    {
        public int Duration { get; set; } = duration;

        public abstract void ApplyEffect(Player player);
    }

    public class PoisonEffect(int duration, int damagePerTurn = 2) : ActiveEffect(duration)
    {
        public override void ApplyEffect(Player player)
        {
            Events.RaiseGameMessage(I18n.T("HHS.Core.PoisonEffect.Message", player.Name));
            player.TakeDamage(damagePerTurn);
        }
    }

    public class StunEffect(int duration) : ActiveEffect(duration)
    {
        public override void ApplyEffect(Player player)
        {
            Events.RaiseGameMessage(I18n.T("HHS.Core.StunEffect.Message", player.Name));
            // Stun prevents actions but doesn't deal damage
        }
    }
}