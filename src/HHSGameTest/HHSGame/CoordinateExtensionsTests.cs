namespace HHSGame.Core
{
    [TestClass]
    public class CoordinateExtensionsTests
    {
        [TestMethod]
        public void ToMoveMapsAdjacentSteps()
        {
            Coordinate origin = new(0, 0);

            Assert.AreEqual(new Move.Forward(Direction.Right), origin.ToMove(new Coordinate(1, 0)));
            Assert.AreEqual(new Move.Forward(Direction.Left), origin.ToMove(new Coordinate(-1, 0)));
            Assert.AreEqual(new Move.Forward(Direction.Down), origin.ToMove(new Coordinate(0, 1)));
            Assert.AreEqual(new Move.Forward(Direction.Up), origin.ToMove(new Coordinate(0, -1)));
            Assert.AreEqual(new Move.Diagonal(Direction.Down, Direction.Right), origin.ToMove(new Coordinate(1, 1)));
            Assert.AreEqual(new Move.Diagonal(Direction.Up, Direction.Right), origin.ToMove(new Coordinate(1, -1)));
            Assert.AreEqual(new Move.Diagonal(Direction.Down, Direction.Left), origin.ToMove(new Coordinate(-1, 1)));
            Assert.AreEqual(new Move.Diagonal(Direction.Up, Direction.Left), origin.ToMove(new Coordinate(-1, -1)));
        }

        [TestMethod]
        public void ToMoveReturnsNoneForNonAdjacent()
        {
            Coordinate origin = new(0, 0);

            Assert.AreEqual(new Move.None(), origin.ToMove(new Coordinate(2, 0)));
            Assert.AreEqual(new Move.None(), origin.ToMove(new Coordinate(0, 2)));
            Assert.AreEqual(new Move.None(), origin.ToMove(new Coordinate(2, 2)));
        }
    }
}
