using Microsoft.Extensions.Logging;

namespace HHSGame.Core.Tutorial
{
    /// <summary>
    /// Manages tutorial progression and hint display.
    /// Tracks which steps are completed and which trigger to show next.
    /// </summary>
    public sealed class TutorialManager
    {
        private readonly List<TutorialStep> steps = [];
        private readonly HashSet<string> completedSteps = new(StringComparer.OrdinalIgnoreCase);
        private int currentStepIndex;
        private bool isActive;
        private bool isDismissed;

        public event EventHandler<TutorialStep>? HintReady;
        public event EventHandler? TutorialCompleted;

        public bool IsActive => isActive && !isDismissed;
        public bool IsDismissed => isDismissed;
        public TutorialStep? CurrentStep => currentStepIndex < steps.Count ? steps[currentStepIndex] : null;
        public IReadOnlyList<TutorialStep> Steps => steps;
        public IReadOnlyCollection<string> CompletedSteps => completedSteps;

        /// <summary>
        /// Loads a tutorial scenario and begins tracking.
        /// </summary>
        public void LoadScenario(TutorialScenario scenario)
        {
            steps.Clear();
            completedSteps.Clear();
            currentStepIndex = 0;
            isDismissed = false;
            isActive = true;
            steps.AddRange(scenario.Steps.OrderBy(s => s.Order));

            // Fire the first step if it's immediate
            if (CurrentStep?.Trigger == TutorialTrigger.Immediate)
            {
                ShowCurrentHint();
            }
        }

        /// <summary>
        /// Notifies the tutorial that an event occurred. Advances steps if the trigger matches.
        /// </summary>
        public void Notify(TutorialTrigger trigger, string? param = null)
        {
            if (!isActive || isDismissed || currentStepIndex >= steps.Count)
            {
                return;
            }

            TutorialStep step = steps[currentStepIndex];
            if (step.Trigger != trigger)
            {
                return;
            }

            // For marker triggers, check the param
            if (trigger == TutorialTrigger.OnMarkerReached && !string.IsNullOrEmpty(step.TriggerParam))
            {
                if (!string.Equals(step.TriggerParam, param, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            ShowCurrentHint();
        }

        /// <summary>
        /// Marks the current step as completed and advances to the next.
        /// </summary>
        public void AdvanceStep()
        {
            if (!isActive || currentStepIndex >= steps.Count)
            {
                return;
            }

            TutorialStep step = steps[currentStepIndex];
            completedSteps.Add(step.Id);
            currentStepIndex++;

            if (currentStepIndex >= steps.Count)
            {
                isActive = false;
                TutorialCompleted?.Invoke(this, EventArgs.Empty);
                return;
            }

            // If the next step is immediate, show it
            if (CurrentStep?.Trigger == TutorialTrigger.Immediate)
            {
                ShowCurrentHint();
            }
        }

        /// <summary>
        /// Skips the entire tutorial.
        /// </summary>
        public void Dismiss()
        {
            isDismissed = true;
            isActive = false;
        }

        /// <summary>
        /// Returns true if the given step has been completed.
        /// </summary>
        public bool IsStepCompleted(string stepId)
        {
            return completedSteps.Contains(stepId);
        }

        /// <summary>
        /// Returns the number of completed steps.
        /// </summary>
        public int CompletedCount => completedSteps.Count;

        /// <summary>
        /// Returns total number of steps.
        /// </summary>
        public int TotalCount => steps.Count;

        private void ShowCurrentHint()
        {
            TutorialStep? step = CurrentStep;
            if (step != null)
            {
                HintReady?.Invoke(this, step);
            }
        }
    }
}