namespace HHSGame.Core.Combat
{
    [TestClass]
    public class TurnManagerTests
    {
        [TestMethod]
        public void StartsInPlayerTurn()
        {
            TurnManager manager = new();
            Assert.IsTrue(manager.IsPlayerTurn());
            Assert.IsFalse(manager.IsEnemyTurn());
        }

        [TestMethod]
        public void EndPlayerTurnAdvancesAndCounts()
        {
            TurnManager manager = new();
            manager.EndPlayerTurn();
            Assert.IsTrue(manager.IsEnemyTurn());
            Assert.AreEqual(1, manager.TurnCount);
        }

        [TestMethod]
        public void EndEnemyTurnReturnsToPlayer()
        {
            TurnManager manager = new();
            manager.BeginEnemyTurn();
            manager.EndEnemyTurn();
            Assert.IsTrue(manager.IsPlayerTurn());
        }

        [TestMethod]
        public void BeginEnemyTurnSwitchesState()
        {
            TurnManager manager = new();
            manager.BeginEnemyTurn();
            Assert.IsTrue(manager.IsEnemyTurn());
        }

        [TestMethod]
        public void EndPlayerTurnIgnoredWhenNotPlayerTurn()
        {
            TurnManager manager = new();
            manager.BeginEnemyTurn();
            manager.EndPlayerTurn();
            Assert.AreEqual(0, manager.TurnCount);
            Assert.IsTrue(manager.IsEnemyTurn());
        }
    }
}
