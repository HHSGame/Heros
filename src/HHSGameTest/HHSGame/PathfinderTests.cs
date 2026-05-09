using HHSGame.Core;
using HHSGame.Core.Map;
using HHSGame.UI;

namespace HHSGameTest.HHSGame
{
    [TestClass]
    public class PathfinderTests
    {
        // ── FindPath ───────────────────────────────────────────────────

        [TestMethod]
        public void FindPath_SamePosition_ReturnsSingleElement()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));
            Pathfinder pathfinder = new(mapState);

            var path = pathfinder.FindPath(new Coordinate(5, 5), new Coordinate(5, 5));

            Assert.AreEqual(1, path.Count);
            Assert.AreEqual(new Coordinate(5, 5), path[0]);
        }

        [TestMethod]
        public void FindPath_AdjacentPosition_ReturnsTwoElements()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));
            Pathfinder pathfinder = new(mapState);

            var path = pathfinder.FindPath(new Coordinate(5, 5), new Coordinate(6, 5));

            Assert.IsTrue(path.Count >= 2);
            Assert.AreEqual(new Coordinate(5, 5), path[0]);
            Assert.AreEqual(new Coordinate(6, 5), path[^1]);
        }

        [TestMethod]
        public void FindPath_StraightLine_ReturnsDirectPath()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(20, 20));
            Pathfinder pathfinder = new(mapState);

            var path = pathfinder.FindPath(new Coordinate(5, 5), new Coordinate(10, 5));

            Assert.AreEqual(6, path.Count);
            for (int i = 0; i < path.Count; i++)
            {
                Assert.AreEqual(5 + i, path[i].X);
                Assert.AreEqual(5, path[i].Y);
            }
        }

        [TestMethod]
        public void FindPath_AroundWall_FindsAlternateRoute()
        {
            MapState mapState = new();
            var data = CreateMapDataWithWall(10, 10, 5, 0, 5, 3);
            mapState.Init(data);
            Pathfinder pathfinder = new(mapState);

            var path = pathfinder.FindPath(new Coordinate(4, 2), new Coordinate(6, 2));

            Assert.IsTrue(path.Count > 2, "Path should go around the wall");
            Assert.AreEqual(new Coordinate(4, 2), path[0]);
            Assert.AreEqual(new Coordinate(6, 2), path[^1]);
        }

        [TestMethod]
        public void FindPath_UnreachableTarget_ReturnsEmpty()
        {
            MapState mapState = new();
            var data = CreateFullyWalledMap(10, 10);
            mapState.Init(data);
            Pathfinder pathfinder = new(mapState);

            var path = pathfinder.FindPath(new Coordinate(1, 1), new Coordinate(8, 8));

            Assert.AreEqual(0, path.Count);
        }

        [TestMethod]
        public void FindPath_DiagonalPath_IsAllowed()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));
            Pathfinder pathfinder = new(mapState);

            var path = pathfinder.FindPath(new Coordinate(1, 1), new Coordinate(3, 3));

            Assert.IsTrue(path.Count >= 2);
            Assert.AreEqual(new Coordinate(1, 1), path[0]);
            Assert.AreEqual(new Coordinate(3, 3), path[^1]);
        }

        [TestMethod]
        public void FindPath_OutOfBounds_ReturnsEmpty()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));
            Pathfinder pathfinder = new(mapState);

            var path = pathfinder.FindPath(new Coordinate(1, 1), new Coordinate(100, 100));

            Assert.AreEqual(0, path.Count);
        }

        // ── Helpers ────────────────────────────────────────────────────

        private static MapData CreateMapData(int width, int height)
        {
            Cell[,] cells = new Cell[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    cells[y, x] = new Cell
                    {
                        Character = '.',
                        Attribute = TilePresets.GetTerrainColor('.')
                    };
                }
            }
            return new MapData(cells, []);
        }

        private static MapData CreateMapDataWithWall(int width, int height, int wallX, int wallYStart, int wallXEnd, int wallYEnd)
        {
            Cell[,] cells = new Cell[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isWall = (x == wallX && y >= wallYStart && y <= wallYEnd);
                    char glyph = isWall ? '#' : '.';
                    cells[y, x] = new Cell
                    {
                        Character = glyph,
                        Attribute = TilePresets.GetTerrainColor(glyph)
                    };
                }
            }
            return new MapData(cells, []);
        }

        private static MapData CreateFullyWalledMap(int width, int height)
        {
            Cell[,] cells = new Cell[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    char glyph = '#';
                    cells[y, x] = new Cell
                    {
                        Character = glyph,
                        Attribute = TilePresets.GetTerrainColor(glyph)
                    };
                }
            }
            return new MapData(cells, []);
        }
    }
}