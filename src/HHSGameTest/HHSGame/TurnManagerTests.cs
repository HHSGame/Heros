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
            manager.EndPlayerTurn();
            manager.EndEnemyTurn();
            Assert.IsTrue(manager.IsPlayerTurn());
        }
    }
}
