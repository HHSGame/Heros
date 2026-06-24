using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace HHSEditor.Controls;

/// <summary>
/// 游戏画布控件 - 渲染游戏地图和实体
/// </summary>
public class GameCanvasControl : Control
{
    public static readonly StyledProperty<int> MapWidthProperty =
        AvaloniaProperty.Register<GameCanvasControl, int>(nameof(MapWidth), 40);

    public static readonly StyledProperty<int> MapHeightProperty =
        AvaloniaProperty.Register<GameCanvasControl, int>(nameof(MapHeight), 25);

    public static readonly StyledProperty<char[,]> MapDataProperty =
        AvaloniaProperty.Register<GameCanvasControl, char[,]>(nameof(MapData));

    public static readonly StyledProperty<List<EntityRenderData>?> EntitiesProperty =
        AvaloniaProperty.Register<GameCanvasControl, List<EntityRenderData>?>(nameof(Entities));

    public int MapWidth
    {
        get => GetValue(MapWidthProperty);
        set => SetValue(MapWidthProperty, value);
    }

    public int MapHeight
    {
        get => GetValue(MapHeightProperty);
        set => SetValue(MapHeightProperty, value);
    }

    public char[,] MapData
    {
        get => GetValue(MapDataProperty);
        set => SetValue(MapDataProperty, value);
    }

    public List<EntityRenderData>? Entities
    {
        get => GetValue(EntitiesProperty);
        set => SetValue(EntitiesProperty, value);
    }

    private const int CellSize = 16;

    private static readonly Dictionary<char, Color> TileColors = new()
    {
        { '.', Color.Parse("#424242") },   // Floor
        { '#', Color.Parse("#795548") },   // Wall
        { '+', Color.Parse("#FF9800") },   // Door
        { '-', Color.Parse("#FFB74D") },   // Open door
        { '~', Color.Parse("#2196F3") },   // Water
        { '^', Color.Parse("#F44336") },   // Lava
        { '*', Color.Parse("#4CAF50") },   // Forest
        { 'H', Color.Parse("#8D6E63") },   // House
        { 'S', Color.Parse("#9C27B0") },   // Shop
        { '>', Color.Parse("#FFEB3B") },   // Stairs down
        { '<', Color.Parse("#FFEB3B") },   // Stairs up
        { ' ', Color.Parse("#1E1E1E") },   // Empty
    };

    private static readonly Dictionary<char, Color> EntityColors = new()
    {
        { '@', Color.Parse("#4CAF50") },   // Player
        { 'E', Color.Parse("#F44336") },   // Enemy
        { 'C', Color.Parse("#2196F3") },   // NPC
        { 'I', Color.Parse("#FF9800") },   // Item
        { 'T', Color.Parse("#9C27B0") },   // Trigger
    };

    private static readonly Color DefaultColor = Color.Parse("#333333");
    private static readonly Color GridColor = Color.Parse("#333333");
    private static readonly IBrush WhiteBrush = Brushes.White;

    public GameCanvasControl()
    {
        MapDataProperty.Changed.AddClassHandler<GameCanvasControl>((x, _) => x.InvalidateVisual());
        MapWidthProperty.Changed.AddClassHandler<GameCanvasControl>((x, _) => x.InvalidateVisual());
        MapHeightProperty.Changed.AddClassHandler<GameCanvasControl>((x, _) => x.InvalidateVisual());
        EntitiesProperty.Changed.AddClassHandler<GameCanvasControl>((x, _) => x.InvalidateVisual());

        ClipToBounds = true;
        Focusable = true;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (MapData == null || MapWidth <= 0 || MapHeight <= 0) return;

        // 绘制背景
        var bgRect = new Rect(0, 0, MapWidth * CellSize, MapHeight * CellSize);
        context.DrawRectangle(Brushes.Black, null, bgRect);

        // 绘制地图瓦片
        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                DrawTile(context, x, y, MapData[x, y]);
            }
        }

        // 绘制网格线
        var gridPen = new Pen(new SolidColorBrush(GridColor), 0.5);
        for (int x = 0; x <= MapWidth; x++)
        {
            context.DrawLine(gridPen,
                new Point(x * CellSize, 0),
                new Point(x * CellSize, MapHeight * CellSize));
        }
        for (int y = 0; y <= MapHeight; y++)
        {
            context.DrawLine(gridPen,
                new Point(0, y * CellSize),
                new Point(MapWidth * CellSize, y * CellSize));
        }

        // 绘制实体
        if (Entities != null)
        {
            foreach (var entity in Entities)
            {
                DrawEntity(context, entity);
            }
        }
    }

    private void DrawTile(DrawingContext context, int x, int y, char c)
    {
        var rect = new Rect(x * CellSize + 1, y * CellSize + 1, CellSize - 1, CellSize - 1);
        var bgColor = TileColors.TryGetValue(c, out var color) ? color : DefaultColor;
        context.DrawRectangle(new SolidColorBrush(bgColor), null, rect);

        // 绘制字符
        try
        {
            var text = new FormattedText(
                c.ToString(),
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Courier New"),
                CellSize * 0.7,
                WhiteBrush);

            var textPos = new Point(
                x * CellSize + (CellSize - text.Width) / 2,
                y * CellSize + (CellSize - text.Height) / 2);

            context.DrawText(text, textPos);
        }
        catch
        {
            // 忽略文本渲染错误
        }
    }

    private void DrawEntity(DrawingContext context, EntityRenderData entity)
    {
        var rect = new Rect(entity.X * CellSize + 1, entity.Y * CellSize + 1, CellSize - 1, CellSize - 1);
        var bgColor = EntityColors.TryGetValue(entity.Glyph, out var color) ? color : Color.Parse("#FF0000");
        context.DrawRectangle(new SolidColorBrush(bgColor), null, rect);

        try
        {
            var text = new FormattedText(
                entity.Glyph.ToString(),
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Courier New"),
                CellSize * 0.8,
                Brushes.White);

            var textPos = new Point(
                entity.X * CellSize + (CellSize - text.Width) / 2,
                entity.Y * CellSize + (CellSize - text.Height) / 2);

            context.DrawText(text, textPos);
        }
        catch
        {
            // 忽略文本渲染错误
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(MapWidth * CellSize, MapHeight * CellSize);
    }
}

/// <summary>
/// 实体渲染数据
/// </summary>
public class EntityRenderData
{
    public int X { get; set; }
    public int Y { get; set; }
    public char Glyph { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = ""; // Player, Enemy, NPC, Item
}
