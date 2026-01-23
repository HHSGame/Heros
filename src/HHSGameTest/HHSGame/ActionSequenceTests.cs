using HHSGame.Core.Combat;

namespace HHSGame.Core.Combat
{
    [TestClass]
    public class ActionSequenceTests
    {
        [TestMethod]
        public void CalculateTurnsNeededRoundsUp()
        {
            Assert.AreEqual(0, ActionSequence.CalculateTurnsNeeded(0, 5));
            Assert.AreEqual(0, ActionSequence.CalculateTurnsNeeded(5, 0));
            Assert.AreEqual(1, ActionSequence.CalculateTurnsNeeded(1, 5));
            Assert.AreEqual(1, ActionSequence.CalculateTurnsNeeded(5, 5));
            Assert.AreEqual(2, ActionSequence.CalculateTurnsNeeded(6, 5));
        }

        [TestMethod]
        public void TurnsRequiredUsesTotalCost()
        {
            ActionSequence sequence = new();
            sequence.Enqueue(new QueuedAction("Test", 3, () => { }));
            sequence.Enqueue(new QueuedAction("Test 2", 3, () => { }));

            Assert.AreEqual(2, sequence.TurnsRequired(5));
        }
    }
}
