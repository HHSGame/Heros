using HHSGame.Core.Rendering;

namespace HHSGame.Core.Map
{
    public static class TilePresets
    {
        public static readonly Dictionary<char, GameAttribute> TERRIANCOLORS = new()
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

        public static GameAttribute GetTerrainColor(char terrainChar)
        {
            return TERRIANCOLORS.TryGetValue(terrainChar, out GameAttribute color)
                ? color
                : ColorPresets.Terrain.Grass;
        }
    }
}
