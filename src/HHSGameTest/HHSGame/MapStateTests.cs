using HHSGame.Core;
using HHSGame.Core.Map;
using HHSGame.UI;
using HHSGame.Core.Rendering;

namespace HHSGameTest.HHSGame
{
    [TestClass]
    public class MapStateTests
    {
        // ── Init ───────────────────────────────────────────────────────

        [TestMethod]
        public void Init_SetsWidthAndHeight()
        {
            MapState mapState = new();
            var data = CreateMapData(20, 15);

            mapState.Init(data);

            Assert.AreEqual(20, mapState.Width);
            Assert.AreEqual(15, mapState.Height);
        }

        [TestMethod]
        public void Init_DefaultMapState_HasPositiveOrZeroDimensions()
        {
            MapState mapState = new();
            Assert.IsTrue(mapState.Width >= 0);
            Assert.IsTrue(mapState.Height >= 0);
        }

        // ── IsInBounds ─────────────────────────────────────────────────

        [TestMethod]
        public void IsInBounds_InsideBounds_ReturnsTrue()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));

            Assert.IsTrue(mapState.IsInBounds(0, 0));
            Assert.IsTrue(mapState.IsInBounds(5, 5));
            Assert.IsTrue(mapState.IsInBounds(9, 9));
        }

        [TestMethod]
        public void IsInBounds_OutsideBounds_ReturnsFalse()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));

            Assert.IsFalse(mapState.IsInBounds(-1, 0));
            Assert.IsFalse(mapState.IsInBounds(0, -1));
            Assert.IsFalse(mapState.IsInBounds(10, 0));
            Assert.IsFalse(mapState.IsInBounds(0, 10));
        }

        [TestMethod]
        public void IsInBounds_Coordinate_Overload()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));

            Assert.IsTrue(mapState.IsInBounds(new Coordinate(5, 5)));
            Assert.IsFalse(mapState.IsInBounds(new Coordinate(15, 5)));
        }

        // ── IsWalkable ─────────────────────────────────────────────────

        [TestMethod]
        public void IsWalkable_FloorTile_ReturnsTrue()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));

            Assert.IsTrue(mapState.IsWalkable(1, 1));
        }

        [TestMethod]
        public void IsWalkable_WallTile_ReturnsFalse()
        {
            MapState mapState = new();
            var data = CreateMapDataWithWalls(10, 10);
            mapState.Init(data);

            Assert.IsFalse(mapState.IsWalkable(0, 0));
        }

        [TestMethod]
        public void IsWalkable_OutOfBounds_ReturnsFalse()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));

            Assert.IsFalse(mapState.IsWalkable(-1, -1));
            Assert.IsFalse(mapState.IsWalkable(100, 100));
        }

        [TestMethod]
        public void IsWalkable_Coordinate_Overload()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));

            Assert.IsTrue(mapState.IsWalkable(new Coordinate(1, 1)));
        }

        // ── IsTransparent ──────────────────────────────────────────────

        [TestMethod]
        public void IsTransparent_FloorTile_ReturnsTrue()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));

            Assert.IsTrue(mapState.IsTransparent(1, 1));
        }

        [TestMethod]
        public void IsTransparent_WallTile_ReturnsFalse()
        {
            MapState mapState = new();
            var data = CreateMapDataWithWalls(10, 10);
            mapState.Init(data);

            Assert.IsFalse(mapState.IsTransparent(0, 0));
        }

        // ── GetCell / SetCell ──────────────────────────────────────────

        [TestMethod]
        public void GetCell_ReturnsCorrectCell()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));

            Cell cell = mapState.GetCell(1, 1);
            Assert.AreEqual('.', cell.Character);
        }

        [TestMethod]
        public void SetCell_UpdatesCell()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));

            mapState.SetCell(1, 1, new Cell
            {
                Character = '+',
                Attribute = TilePresets.GetTerrainColor('+')
            });

            Cell cell = mapState.GetCell(1, 1);
            Assert.AreEqual('+', cell.Character);
        }

        // ── IsExplored / WasVisited / MarkExplored ─────────────────────

        [TestMethod]
        public void WasVisited_DefaultFalse()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));

            Assert.IsFalse(mapState.WasVisited(5, 5));
        }

        [TestMethod]
        public void MarkExplored_MakesTileVisited()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));

            mapState.MarkExplored(5, 5);

            Assert.IsTrue(mapState.WasVisited(5, 5));
        }

        [TestMethod]
        public void MarkExplored_MultipleTiles()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));

            mapState.MarkExplored(1, 1);
            mapState.MarkExplored(2, 2);
            mapState.MarkExplored(3, 3);

            Assert.IsTrue(mapState.WasVisited(1, 1));
            Assert.IsTrue(mapState.WasVisited(2, 2));
            Assert.IsTrue(mapState.WasVisited(3, 3));
            Assert.IsFalse(mapState.WasVisited(4, 4));
        }

        // ── MarkVisibleTiles ───────────────────────────────────────────

        [TestMethod]
        public void MarkVisibleTiles_MakesTilesVisible()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));

            HashSet<Coordinate> tiles = new()
            {
                new Coordinate(3, 3),
                new Coordinate(4, 4),
                new Coordinate(5, 5)
            };

            mapState.MarkVisibleTiles(tiles);

            Assert.IsTrue(mapState.IsVisible(3, 3));
            Assert.IsTrue(mapState.IsVisible(4, 4));
            Assert.IsTrue(mapState.IsVisible(5, 5));
            Assert.IsFalse(mapState.IsVisible(6, 6));
        }

        // ── HiddenFeatures ─────────────────────────────────────────────

        [TestMethod]
        public void AddHiddenFeature_StoresFeature()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));

            var feature = new HiddenFeature(new Coordinate(5, 5), 'C', "A hidden cache");
            mapState.AddHiddenFeature(feature);

            Assert.AreEqual(1, mapState.HiddenFeatures.Count);
        }

        [TestMethod]
        public void RevealHiddenFeatures_ReturnsFeaturesInRadius()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));

            var feature1 = new HiddenFeature(new Coordinate(5, 5), 'C', "Cache 1");
            var feature2 = new HiddenFeature(new Coordinate(6, 6), 'C', "Cache 2");
            var feature3 = new HiddenFeature(new Coordinate(15, 15), 'C', "Far away");
            mapState.AddHiddenFeature(feature1);
            mapState.AddHiddenFeature(feature2);
            mapState.AddHiddenFeature(feature3);

            var revealed = mapState.RevealHiddenFeatures(new Coordinate(5, 5), 2);

            Assert.AreEqual(2, revealed.Count);
            Assert.IsTrue(feature1.Revealed);
            Assert.IsTrue(feature2.Revealed);
            Assert.IsFalse(feature3.Revealed);
        }

        [TestMethod]
        public void RevealHiddenFeatures_AlreadyRevealed_NotReturned()
        {
            MapState mapState = new();
            mapState.Init(CreateMapData(10, 10));

            var feature = new HiddenFeature(new Coordinate(5, 5), 'C', "Cache");
            feature.Revealed = true;
            mapState.AddHiddenFeature(feature);

            var revealed = mapState.RevealHiddenFeatures(new Coordinate(5, 5), 2);

            Assert.AreEqual(0, revealed.Count);
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

        private static MapData CreateMapDataWithWalls(int width, int height)
        {
            Cell[,] cells = new Cell[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    char glyph = (x == 0 || y == 0 || x == width - 1 || y == height - 1) ? '#' : '.';
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