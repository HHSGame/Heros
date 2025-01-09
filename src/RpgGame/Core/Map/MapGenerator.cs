using System;
using System.Collections.Generic;
using RpgGame.UI;

namespace RpgGame.Core
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
            { '#', Colors.Terrain.Stone },    // Walls
            { '.', Colors.Terrain.Grass },    // Floors
            { '~', Colors.Terrain.Water },    // Water
            { '^', Colors.Terrain.Lava },     // Lava/Hills
            { '*', Colors.Terrain.Forest },   // Vegetation
            { 'H', Colors.Terrain.Stone },    // Houses
            { 'S', Colors.Terrain.Sand },     // Shops
            { '║', Colors.Terrain.Stone },    // Town walls
            { '═', Colors.Terrain.Stone },    // Town walls
            { '╔', Colors.Terrain.Stone },    // Town walls
            { '╗', Colors.Terrain.Stone },    // Town walls
            { '╚', Colors.Terrain.Stone },    // Town walls
            { '╝', Colors.Terrain.Stone },    // Town walls
            { '▒', Colors.Terrain.Grass },    // Town streets
        };

        public static Attribute GetTerrainColor(char terrainChar)
        {
            return _terrainColors.TryGetValue(terrainChar, out var color) 
                ? color 
                : Colors.Terrain.Grass;
        }
    
        private readonly Random _random;
        private readonly int _mapWidth;
        private readonly int _mapHeight;
        private readonly MapStyle _style;
        
        public MapGenerator(int mapWidth, int mapHeight, Random random, MapStyle style = MapStyle.Cave)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _random = random;
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
            
            EventSystem.RaiseEvent($"{_style} map generated successfully");
            return map;
        }

    }
}
