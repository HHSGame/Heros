using HHSGame.Core.Map;

namespace HHSGame.Core.Rendering
{
    /// <summary>
    /// 处理游戏渲染逻辑，从 Game 类中提取。
    /// </summary>
    public sealed class GameRenderer
    {
        private readonly GameContext context;
        private readonly IApplicationHost? applicationHost;
        private readonly List<(Coordinate Position, Cell)> overlayCells = [];

        public GameRenderer(GameContext context, IApplicationHost? applicationHost = null)
        {
            this.context = context;
            this.applicationHost = applicationHost;
        }

        public void SetOverlayCells(IEnumerable<(Coordinate Position, Cell)> cells)
        {
            overlayCells.Clear();
            overlayCells.AddRange(cells);
        }

        public void ClearOverlayCells()
        {
            overlayCells.Clear();
        }

        public void RenderFrame(GameWorld world, Player activePlayer, IReadOnlyList<Player> controlledPlayers)
        {
            if (world == null || activePlayer == null)
            {
                return;
            }

            IDrawingContext drawingContext = context.DrawingContext;

            drawingContext.Viewport.UpdateViewport(activePlayer.X, activePlayer.Y, context.MapState.Width, context.MapState.Height);
            drawingContext.Clear();

            world.Draw(drawingContext);
            foreach ((Coordinate position, Cell cell) in overlayCells)
            {
                drawingContext.DrawAt((position.X, position.Y), cell);
            }
            DrawPlannedDestinations(drawingContext, controlledPlayers);
            foreach (Player player in controlledPlayers)
            {
                if (player != activePlayer)
                {
                    (player as IGameActor).Draw(drawingContext);
                }
            }
            (activePlayer as IGameActor).Draw(drawingContext);

            drawingContext.Render();
        }

        private void DrawPlannedDestinations(IDrawingContext drawingContext, IReadOnlyList<Player> controlledPlayers)
        {
            foreach (Player player in controlledPlayers)
            {
                if (player.PlannedPosition.Equals(player.Position))
                {
                    continue;
                }

                Coordinate position = player.PlannedPosition;
                drawingContext.DrawAt((position.X, position.Y), new Cell
                {
                    Character = GUISettings.PlannedDestinationGlyph,
                    Attribute = ColorPresets.PlannedDestination
                });
            }
        }

        public void AnimateStep(GameWorld world, Player activePlayer, IReadOnlyList<Player> controlledPlayers)
        {
            RenderFrame(world, activePlayer, controlledPlayers);
            if (applicationHost == null || !applicationHost.IsInitialized)
            {
                return;
            }

            applicationHost.LayoutAndDraw(true);
        }
    }
}
