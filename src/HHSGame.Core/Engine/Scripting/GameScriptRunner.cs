using HHSGame.Core;

namespace HHSGame.Core.Engine.Scripting
{
    public enum ScriptRunnerStatus
    {
        Running,
        Completed,
        Failed
    }

    public sealed class ScriptRunnerOptions
    {
        public int MaxTotalSteps { get; set; } = 10000;
        public int MaxStepsPerTick { get; set; } = 100;
        public int MaxWaitTicks { get; set; } = 500;
        public int DefaultMaxRepeatIterations { get; set; } = 100;
    }

    public sealed class ScriptRunnerResult
    {
        public ScriptRunnerStatus Status { get; init; }
        public string? FailureReason { get; init; }
    }

    public readonly record struct ScriptInput(string Token, int[]? Target);

    public interface IScriptInputAdapter
    {
        ScriptGameStateSnapshot GetSnapshot();
        bool ApplyInput(ScriptInput input);
    }

    public sealed class ScriptGameStateSnapshot
    {
        public GameStateType State { get; init; }
        public bool CombatActive { get; init; }
        public bool HasVisibleEnemies { get; init; }
        public int PlayerX { get; init; }
        public int PlayerY { get; init; }
        public int PlayerAp { get; init; }
        public int TurnCount { get; init; }
    }

    public sealed class GameScriptRunner
    {
        private readonly IScriptInputAdapter adapter;
        private readonly ScriptRunnerOptions options;
        private readonly Stack<ScriptFrame> frames = new();
        private readonly Dictionary<ScriptStep, int> waitCounts = new();
        private int totalSteps;

        public ScriptRunnerStatus Status { get; private set; } = ScriptRunnerStatus.Running;
        public string? FailureReason { get; private set; }

        public GameScriptRunner(GameScript script, IScriptInputAdapter adapter, ScriptRunnerOptions? options = null)
        {
            this.adapter = adapter;
            this.options = options ?? new ScriptRunnerOptions();
            frames.Push(new ScriptFrame(script.Steps));
        }

        public ScriptRunnerStatus Step()
        {
            if (Status != ScriptRunnerStatus.Running)
            {
                return Status;
            }

            totalSteps++;
            if (totalSteps > options.MaxTotalSteps)
            {
                Fail($"Script exceeded max total steps ({options.MaxTotalSteps}).");
                return Status;
            }

            int stepsThisTick = 0;
            while (stepsThisTick < options.MaxStepsPerTick)
            {
                if (frames.Count == 0)
                {
                    Status = ScriptRunnerStatus.Completed;
                    return Status;
                }

                ScriptFrame frame = frames.Peek();
                if (!EnsureRepeatIteration(frame))
                {
                    frames.Pop();
                    continue;
                }
                if (Status != ScriptRunnerStatus.Running)
                {
                    return Status;
                }

                if (!frame.TryPeek(out ScriptStep step))
                {
                    frames.Pop();
                    continue;
                }

                if (step.WaitUntil != null)
                {
                    if (!IsConditionMet(step.WaitUntil))
                    {
                        int count = waitCounts.TryGetValue(step, out int current) ? current + 1 : 1;
                        waitCounts[step] = count;
                        if (count > options.MaxWaitTicks)
                        {
                            Fail($"Wait condition timed out after {options.MaxWaitTicks} ticks.");
                        }
                        return Status;
                    }

                    waitCounts.Remove(step);
                    frame.Advance();
                    stepsThisTick++;
                    continue;
                }

                if (step.Assert != null)
                {
                    if (!IsConditionMet(step.Assert))
                    {
                        Fail("Script assertion failed.");
                        return Status;
                    }

                    frame.Advance();
                    stepsThisTick++;
                    continue;
                }

                if (step.When != null)
                {
                    bool conditionMet = IsConditionMet(step.When);
                    if (Status != ScriptRunnerStatus.Running)
                    {
                        return Status;
                    }
                    frame.Advance();
                    if (conditionMet && step.Then != null)
                    {
                        frames.Push(new ScriptFrame(step.Then));
                    }
                    else if (!conditionMet && step.Else != null)
                    {
                        frames.Push(new ScriptFrame(step.Else));
                    }

                    stepsThisTick++;
                    continue;
                }

                if (step.Repeat != null)
                {
                    frame.Advance();
                    PushRepeatFrame(step.Repeat);
                    if (Status != ScriptRunnerStatus.Running)
                    {
                        return Status;
                    }
                    stepsThisTick++;
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(step.Input))
                {
                    frame.Advance();
                    if (!adapter.ApplyInput(new ScriptInput(step.Input, step.Target)))
                    {
                        Fail($"Input '{step.Input}' was not handled.");
                    }
                    return Status;
                }

                Fail("Script step had no executable action.");
                return Status;
            }

            Fail($"Script exceeded max steps per tick ({options.MaxStepsPerTick}).");
            return Status;
        }

        public ScriptRunnerResult RunToCompletion()
        {
            while (Status == ScriptRunnerStatus.Running)
            {
                Step();
            }

            return new ScriptRunnerResult
            {
                Status = Status,
                FailureReason = FailureReason
            };
        }

        private bool IsConditionMet(ScriptCondition condition)
        {
            ScriptGameStateSnapshot snapshot = adapter.GetSnapshot();

            if (condition.State != null)
            {
                if (!Enum.TryParse(condition.State, true, out GameStateType expectedState))
                {
                    Fail($"Unknown game state '{condition.State}'.");
                    return false;
                }

                if (snapshot.State != expectedState)
                {
                    return false;
                }
            }

            if (condition.CombatActive != null && snapshot.CombatActive != condition.CombatActive.Value)
            {
                return false;
            }

            if (condition.NoVisibleEnemies != null && condition.NoVisibleEnemies.Value == snapshot.HasVisibleEnemies)
            {
                return false;
            }

            if (condition.PlayerX != null && snapshot.PlayerX != condition.PlayerX.Value)
            {
                return false;
            }

            if (condition.PlayerY != null && snapshot.PlayerY != condition.PlayerY.Value)
            {
                return false;
            }

            if (condition.MinAp != null && snapshot.PlayerAp < condition.MinAp.Value)
            {
                return false;
            }

            if (condition.MaxAp != null && snapshot.PlayerAp > condition.MaxAp.Value)
            {
                return false;
            }

            if (condition.MinTurnCount != null && snapshot.TurnCount < condition.MinTurnCount.Value)
            {
                return false;
            }

            if (condition.MaxTurnCount != null && snapshot.TurnCount > condition.MaxTurnCount.Value)
            {
                return false;
            }

            return true;
        }

        private bool EnsureRepeatIteration(ScriptFrame frame)
        {
            if (!frame.IsRepeat)
            {
                return true;
            }

            if (frame.HasStepsRemaining)
            {
                return true;
            }

            frame.PrepareNextIteration();
            if (frame.Iteration >= frame.MaxIterations)
            {
                return false;
            }

            return IsConditionMet(frame.RepeatCondition!);
        }

        private void PushRepeatFrame(ScriptRepeat repeat)
        {
            int maxIterations = repeat.MaxIterations ?? options.DefaultMaxRepeatIterations;
            if (maxIterations <= 0 || repeat.While == null || repeat.Steps.Count == 0)
            {
                return;
            }

            if (!IsConditionMet(repeat.While))
            {
                return;
            }

            frames.Push(new ScriptFrame(repeat.Steps, repeat.While, maxIterations));
        }

        private void Fail(string message)
        {
            Status = ScriptRunnerStatus.Failed;
            FailureReason = message;
        }

        private sealed class ScriptFrame
        {
            public ScriptFrame(IReadOnlyList<ScriptStep> steps, ScriptCondition? repeatCondition = null, int maxIterations = 0)
            {
                Steps = steps;
                RepeatCondition = repeatCondition;
                MaxIterations = maxIterations;
            }

            public IReadOnlyList<ScriptStep> Steps { get; }
            public ScriptCondition? RepeatCondition { get; }
            public int MaxIterations { get; }
            public int Iteration { get; private set; }
            private int index;

            public bool IsRepeat => RepeatCondition != null;
            public bool HasStepsRemaining => index < Steps.Count;

            public bool TryPeek(out ScriptStep step)
            {
                if (index >= Steps.Count)
                {
                    step = null!;
                    return false;
                }

                step = Steps[index];
                return true;
            }

            public void Advance()
            {
                index++;
            }

            public void PrepareNextIteration()
            {
                index = 0;
                Iteration++;
            }
        }
    }
}
