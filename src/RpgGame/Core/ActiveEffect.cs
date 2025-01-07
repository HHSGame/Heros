
namespace RpgGame.Core {

    public abstract class ActiveEffect
    {
        public int Duration { get; set; }

        public ActiveEffect(int duration)
        {
            Duration = duration;
        }

        public abstract void ApplyEffect(Player player);
    }

    public class PoisonEffect : ActiveEffect
    {
        private int _damagePerTurn;

        public PoisonEffect(int duration, int damagePerTurn = 2) : base(duration)
        {
            _damagePerTurn = damagePerTurn;
        }

        public override void ApplyEffect(Player player)
        {
            player.TakeDamage(_damagePerTurn);
        }
    }

    public class StunEffect : ActiveEffect
    {
        public StunEffect(int duration) : base(duration)
        {
        }

        public override void ApplyEffect(Player player)
        {
            // Stun prevents actions but doesn't deal damage
        }
    }
}