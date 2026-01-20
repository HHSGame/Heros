using HHSGame.Core.Stats;

namespace HHSGame.Core.Combat
{
    public sealed class QueuedAction(string description, int apCost, Action execute)
    {
        public string Description { get; } = description;
        public int ApCost { get; } = Math.Max(0, apCost);
        public void Execute() => execute();
    }

    public sealed class ActionSequence
    {
        private readonly Queue<QueuedAction> actions = new();
        private int totalCost;

        public int TotalCost => totalCost;
        public int Count => actions.Count;
        public bool HasActions => actions.Count > 0;

        public IReadOnlyList<QueuedAction> Snapshot()
        {
            return actions.ToList();
        }

        public void Enqueue(QueuedAction action)
        {
            actions.Enqueue(action);
            totalCost += action.ApCost;
        }

        public void Clear()
        {
            actions.Clear();
            totalCost = 0;
        }

        public int RemainingApForTurn(int maxAp)
        {
            return Math.Max(0, maxAp - totalCost);
        }

        public int TurnsRequired(int maxAp)
        {
            if (maxAp <= 0)
            {
                return 0;
            }

            return (int)Math.Ceiling(totalCost / (double)maxAp);
        }

        public void Execute(CharacterStats stats, bool ignoreAp = false, Action<QueuedAction>? onExecuted = null)
        {
            int remainingAp = ignoreAp ? int.MaxValue : stats.CurrentAp;

            while (actions.Count > 0)
            {
                QueuedAction action = actions.Peek();
                if (!ignoreAp && action.ApCost > remainingAp)
                {
                    break;
                }

                if (!ignoreAp)
                {
                    if (!stats.TrySpendAp(action.ApCost))
                    {
                        break;
                    }
                    remainingAp = stats.CurrentAp;
                }

                action.Execute();
                onExecuted?.Invoke(action);
                actions.Dequeue();
                totalCost -= action.ApCost;
            }
        }
    }
}
