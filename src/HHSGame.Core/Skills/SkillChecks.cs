namespace HHSGame.Core.SkillSystem
{
    public sealed record SkillCheckResult(int Roll, int Total, int Difficulty, bool Success);

    public static class SkillChecks
    {
        public static SkillCheckResult Roll(int skillValue, int difficulty, Random random)
        {
            int roll = random.Next(1, 21);
            int total = roll + skillValue;
            bool success = total >= difficulty;
            return new SkillCheckResult(roll, total, difficulty, success);
        }
    }
}
