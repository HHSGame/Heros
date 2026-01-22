using HHSGame.Core.Stats;

namespace HHSGame.Core.Engine.Catalogs
{
    public static class SkillMapper
    {
        public static Skills BuildSkills(Dictionary<string, int> skillRanks)
        {
            Skills skills = new();
            foreach (KeyValuePair<string, int> entry in skillRanks)
            {
                if (Enum.TryParse(entry.Key, true, out SkillType skillType))
                {
                    skills.SetRank(skillType, entry.Value);
                }
            }

            return skills;
        }
    }
}
