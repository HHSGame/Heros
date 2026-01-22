namespace HHSGame.Core
{
    public sealed record PlayerSpawn(
        string Name,
        char Glyph,
        Classes.ClassConfig ClassConfig,
        Stats.Attributes? Attributes,
        Stats.Skills? Skills,
        Coordinate Position,
        List<string> StartingItems);

    public sealed record EnemySpawn(string EnemyId, Coordinate Position, int Count = 1);

    public sealed record MapItemSpawn(string ItemId, Coordinate Position, int Quantity = 1);
}
