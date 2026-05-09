using HHSGame.Core.Tutorial;

namespace HHSGameTest.HHSGame
{
    [TestClass]
    public class TutorialManagerTests
    {
        [TestMethod]
        public void LoadScenario_ActivatesTutorial()
        {
            TutorialManager manager = new();
            TutorialScenario scenario = CreateScenario(3);

            manager.LoadScenario(scenario);

            Assert.IsTrue(manager.IsActive);
            Assert.AreEqual(3, manager.TotalCount);
            Assert.AreEqual(0, manager.CompletedCount);
        }

        [TestMethod]
        public void LoadScenario_FirstImmediateStep_FiresHintReady()
        {
            TutorialManager manager = new();
            TutorialScenario scenario = new()
            {
                Id = "test",
                Name = "Test",
                Steps =
                [
                    new TutorialStep { Id = "s1", Title = "Step 1", Message = "Hello", Trigger = TutorialTrigger.Immediate, Order = 0 },
                    new TutorialStep { Id = "s2", Title = "Step 2", Message = "World", Trigger = TutorialTrigger.OnMove, Order = 1 }
                ]
            };

            TutorialStep? received = null;
            manager.HintReady += (_, step) => received = step;

            manager.LoadScenario(scenario);

            Assert.IsNotNull(received);
            Assert.AreEqual("s1", received!.Id);
        }

        [TestMethod]
        public void Notify_MatchingTrigger_FiresHintReady()
        {
            TutorialManager manager = new();
            TutorialScenario scenario = new()
            {
                Id = "test",
                Name = "Test",
                Steps =
                [
                    new TutorialStep { Id = "s1", Title = "Step 1", Message = "Move", Trigger = TutorialTrigger.OnMove, Order = 0 }
                ]
            };
            manager.LoadScenario(scenario);

            TutorialStep? received = null;
            manager.HintReady += (_, step) => received = step;

            manager.Notify(TutorialTrigger.OnMove);

            Assert.IsNotNull(received);
            Assert.AreEqual("s1", received!.Id);
        }

        [TestMethod]
        public void Notify_NonMatchingTrigger_DoesNotFire()
        {
            TutorialManager manager = new();
            TutorialScenario scenario = new()
            {
                Id = "test",
                Name = "Test",
                Steps =
                [
                    new TutorialStep { Id = "s1", Title = "Step 1", Message = "Move", Trigger = TutorialTrigger.OnMove, Order = 0 }
                ]
            };
            manager.LoadScenario(scenario);

            bool fired = false;
            manager.HintReady += (_, _) => fired = true;

            manager.Notify(TutorialTrigger.OnCombatStart);

            Assert.IsFalse(fired);
        }

        [TestMethod]
        public void AdvanceStep_CompletesCurrentStep()
        {
            TutorialManager manager = new();
            TutorialScenario scenario = CreateScenario(3);
            manager.LoadScenario(scenario);

            // First step is Immediate, so it's shown on load
            manager.AdvanceStep();

            Assert.AreEqual(1, manager.CompletedCount);
            Assert.IsTrue(manager.IsStepCompleted("s1"));
        }

        [TestMethod]
        public void AdvanceStep_AllStepsComplete_FiresTutorialCompleted()
        {
            TutorialManager manager = new();
            TutorialScenario scenario = new()
            {
                Id = "test",
                Name = "Test",
                Steps =
                [
                    new TutorialStep { Id = "s1", Title = "Step 1", Message = "Go", Trigger = TutorialTrigger.Immediate, Order = 0 }
                ]
            };
            manager.LoadScenario(scenario);

            bool completed = false;
            manager.TutorialCompleted += (_, _) => completed = true;

            manager.AdvanceStep();

            Assert.IsTrue(completed);
            Assert.IsFalse(manager.IsActive);
        }

        [TestMethod]
        public void Dismiss_StopsTutorial()
        {
            TutorialManager manager = new();
            TutorialScenario scenario = CreateScenario(3);
            manager.LoadScenario(scenario);

            manager.Dismiss();

            Assert.IsFalse(manager.IsActive);
            Assert.IsTrue(manager.IsDismissed);
        }

        [TestMethod]
        public void Notify_AfterDismiss_DoesNothing()
        {
            TutorialManager manager = new();
            TutorialScenario scenario = new()
            {
                Id = "test",
                Name = "Test",
                Steps =
                [
                    new TutorialStep { Id = "s1", Title = "Step 1", Message = "Go", Trigger = TutorialTrigger.Immediate, Order = 0 }
                ]
            };
            manager.LoadScenario(scenario);
            manager.Dismiss();

            bool fired = false;
            manager.HintReady += (_, _) => fired = true;

            manager.Notify(TutorialTrigger.Immediate);

            Assert.IsFalse(fired);
        }

        [TestMethod]
        public void Notify_MarkerReached_ChecksParam()
        {
            TutorialManager manager = new();
            TutorialScenario scenario = new()
            {
                Id = "test",
                Name = "Test",
                Steps =
                [
                    new TutorialStep { Id = "s1", Title = "Step 1", Message = "Go to marker", Trigger = TutorialTrigger.OnMarkerReached, TriggerParam = ">", Order = 0 }
                ]
            };
            manager.LoadScenario(scenario);

            TutorialStep? received = null;
            manager.HintReady += (_, step) => received = step;

            // Wrong marker
            manager.Notify(TutorialTrigger.OnMarkerReached, "X");
            Assert.IsNull(received);

            // Correct marker
            manager.Notify(TutorialTrigger.OnMarkerReached, ">");
            Assert.IsNotNull(received);
        }

        [TestMethod]
        public void Steps_SortedByOrder()
        {
            TutorialManager manager = new();
            TutorialScenario scenario = new()
            {
                Id = "test",
                Name = "Test",
                Steps =
                [
                    new TutorialStep { Id = "s3", Title = "Step 3", Message = "Three", Trigger = TutorialTrigger.Immediate, Order = 2 },
                    new TutorialStep { Id = "s1", Title = "Step 1", Message = "One", Trigger = TutorialTrigger.Immediate, Order = 0 },
                    new TutorialStep { Id = "s2", Title = "Step 2", Message = "Two", Trigger = TutorialTrigger.Immediate, Order = 1 }
                ]
            };
            manager.LoadScenario(scenario);

            Assert.AreEqual("s1", manager.Steps[0].Id);
            Assert.AreEqual("s2", manager.Steps[1].Id);
            Assert.AreEqual("s3", manager.Steps[2].Id);
        }

        [TestMethod]
        public void DefaultScenario_HasSteps()
        {
            TutorialScenario scenario = TutorialScenarios.CreateDefaultScenario();

            Assert.AreEqual("default-tutorial", scenario.Id);
            Assert.IsTrue(scenario.Steps.Count > 0);
            Assert.AreEqual("welcome", scenario.Steps[0].Id);
            Assert.AreEqual(TutorialTrigger.OnMove, scenario.Steps[0].Trigger);
        }

        private static TutorialScenario CreateScenario(int count)
        {
            List<TutorialStep> steps = [];
            for (int i = 0; i < count; i++)
            {
                steps.Add(new TutorialStep
                {
                    Id = $"s{i + 1}",
                    Title = $"Step {i + 1}",
                    Message = $"Message {i + 1}",
                    Trigger = i == 0 ? TutorialTrigger.Immediate : TutorialTrigger.OnMove,
                    Order = i
                });
            }

            return new TutorialScenario
            {
                Id = "test",
                Name = "Test",
                Steps = steps
            };
        }
    }
}