namespace HHSGame.Core
{
    [TestClass]
    public class GameStateMachineTests
    {
        [TestMethod]
        public void StartsInSetup()
        {
            GameStateMachine machine = new();
            Assert.AreEqual(GameStateType.Setup, machine.CurrentState);
        }

        [TestMethod]
        public void AllowsSetupToExploration()
        {
            GameStateMachine machine = new();
            bool changed = machine.TryChangeState(GameStateType.Exploration);
            Assert.IsTrue(changed);
            Assert.AreEqual(GameStateType.Exploration, machine.CurrentState);
        }

        [TestMethod]
        public void BlocksInvalidTransition()
        {
            GameStateMachine machine = new();
            bool changed = machine.TryChangeState(GameStateType.Combat);
            Assert.IsFalse(changed);
            Assert.AreEqual(GameStateType.Setup, machine.CurrentState);
        }

        [TestMethod]
        public void AllowsGameOverFromAnyState()
        {
            GameStateMachine machine = new();
            Assert.IsTrue(machine.TryChangeState(GameStateType.GameOver));
            Assert.AreEqual(GameStateType.GameOver, machine.CurrentState);
        }
    }
}
