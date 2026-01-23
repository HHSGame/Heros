using HHSGame.Core;
using HHSGame.UI;

namespace HHSGame.Core.Npcs
{
    public sealed class NpcManager : IDrawable
    {
        private readonly List<Npc> npcs = [];

        public IReadOnlyList<Npc> Npcs => npcs;

        public void SetNpcs(List<Npc> npcs)
        {
            this.npcs.Clear();
            this.npcs.AddRange(npcs);
        }

        public Npc? GetNpcAt(int x, int y)
        {
            return npcs.FirstOrDefault(npc => npc.X == x && npc.Y == y);
        }

        public IReadOnlyList<Npc> GetAdjacentNpcs(Coordinate position, int radius)
        {
            List<Npc> result = [];
            foreach (Npc npc in npcs)
            {
                int dx = Math.Abs(npc.X - position.X);
                int dy = Math.Abs(npc.Y - position.Y);
                if (dx <= radius && dy <= radius)
                {
                    result.Add(npc);
                }
            }

            return result;
        }

        public void Draw(IDrawingContext ctx)
        {
            Viewport viewport = ctx.Viewport;
            foreach (Npc npc in npcs)
            {
                if (viewport.Contains((npc.X, npc.Y)))
                {
                    (npc as IGameActor).Draw(ctx);
                }
            }
        }
    }
}
