using HHSGame.Core;
using HHSGame.Core.Enemies;

namespace HHSGame.Core.SkillActions
{
    public sealed record SkillActionTarget(
        Coordinate? Position = null,
        Enemy? Enemy = null,
        Npc? Npc = null,
        Player? Player = null);
}
