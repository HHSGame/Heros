using System;
using System.Collections.Generic;
using HHSGame.UI;

namespace HHSGame.Core
{
    using Attribute = Terminal.Gui.Attribute;
    public enum MapStyle
    {
        Cave,
        Hills,
        Town,
    }

    public class MapGenerator
    {
        private static readonly Dictionary<char, Attribute> _terrainColors = new()
        {
            { '#', ColorPresets.Terrain.Stone },    // Walls
            { '.', ColorPresets.Terrain.Grass },    // Floors
            { '~', ColorPresets.Terrain.Water },    // Water
            { '^', ColorPresets.Terrain.Lava },     // Lava/Hills
            { '*', ColorPresets.Terrain.Forest },   // Vegetation
            { 'H', ColorPresets.Terrain.Stone },    // Houses
            { 'S', ColorPresets.Terrain.Sand },     // Shops
            { '║', ColorPresets.Terrain.Stone },    // Town walls
            { '═', ColorPresets.Terrain.Stone },    // Town walls
            { '╔', ColorPresets.Terrain.Stone },    // Town walls
            { '╗', ColorPresets.Terrain.Stone },    // Town walls
            { '╚', ColorPresets.Terrain.Stone },    // Town walls
            { '╝', ColorPresets.Terrain.Stone },    // Town walls
            { '│', ColorPresets.Terrain.Stone },    // House/Room walls
            { '─', ColorPresets.Terrain.Stone },    // House/Room walls
            { '┌', ColorPresets.Terrain.Stone },    // House/Room walls
            { '┐', ColorPresets.Terrain.Stone },    // House/Room walls
            { '└', ColorPresets.Terrain.Stone },    // House/Room walls
            { '┘', ColorPresets.Terrain.Stone },    // House/Room walls
            { '▒', ColorPresets.Terrain.Grass },    // Town streets
        };

        public static Attribute GetTerrainColor(char terrainChar)
        {
            return _terrainColors.TryGetValue(terrainChar, out var color) 
                ? color 
                : ColorPresets.Terrain.Grass;
        }
    
        private readonly Random _random;
        private readonly int _mapWidth;
        private readonly int _mapHeight;
        private readonly MapStyle _style;
        private readonly ItemManager _itemManager;
        private readonly ItemFactory _itemFactory;
        
        public MapGenerator(int mapWidth, int mapHeight, Random random, ItemManager itemManager, ItemFactory itemFactory, MapStyle style = MapStyle.Cave)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _random = random;
            _itemManager = itemManager;
            _itemFactory = itemFactory;
            _style = style;
        }

        public Cell[,] GenerateDungeon()
        {
            EventSystem.RaiseEvent($"Generating {_style.ToString().ToLower()} map...");
            
            BaseMapGenerator generator = _style switch
            {
                MapStyle.Hills => new HillsMapGenerator(_mapWidth, _mapHeight, _random),
                MapStyle.Town => new TownMapGenerator(_mapWidth, _mapHeight, _random),
                _ => new CaveMapGenerator(_mapWidth, _mapHeight, _random)
            };

            var map = generator.Generate();
            
            // Place random items in the dungeon
            PlaceRandomItems(map);
            
            EventSystem.RaiseEvent($"{_style} map generated successfully");
            return map;
        }

        private void PlaceRandomItems(Cell[,] map)
        {
            int itemCount = _random.Next(700, 1000); // Place some items
            for (int i = 0; i < itemCount; i++)
            {
                var item = _itemFactory.CreateRandomItem();
                int x = _random.Next(1, _mapWidth - 1);
                int y = _random.Next(1, _mapHeight - 1);
                while (!map[y, x].IsWalkable)
                {
                    x = _random.Next(1, _mapWidth - 1);
                    y = _random.Next(1, _mapHeight - 1);
                }
                item.X = x;
                item.Y = y;
                _itemManager.AddItem(item);
            }
        }
    }
}
