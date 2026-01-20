using HHSGame.Core.Enemies;

namespace HHSGame.Core
{
    public sealed record EnemySpawn(EnemyType Type, Coordinate Position, int Count = 1);

    public sealed record MapItemSpawn(string ItemId, Coordinate Position, int Quantity = 1);
}
