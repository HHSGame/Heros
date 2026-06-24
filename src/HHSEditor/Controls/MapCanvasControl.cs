using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace HHSEditor.Controls;

public class MapCanvasControl : Control
{
    public static readonly StyledProperty<int> MapWidthProperty =
        AvaloniaProperty.Register<MapCanvasControl, int>(nameof(MapWidth), 40);

    public static readonly StyledProperty<int> MapHeightProperty =
        AvaloniaProperty.Register<MapCanvasControl, int>(nameof(MapHeight), 25);

    public static readonly StyledProperty<char[,]> MapDataProperty =
        AvaloniaProperty.Register<MapCanvasControl, char[,]>(nameof(MapData));

    public static readonly StyledProperty<char> CurrentBrushProperty =
        AvaloniaProperty.Register<MapCanvasControl, char>(nameof(CurrentBrush), '.');

    public static readonly StyledProperty<int> CellSizeProperty =
        AvaloniaProperty.Register<MapCanvasControl, int>(nameof(CellSize), 16);

    public static readonly StyledProperty<string> CurrentToolProperty =
        AvaloniaProperty.Register<MapCanvasControl, string>(nameof(CurrentTool), "Paint");

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

    public char CurrentBrush
    {
        get => GetValue(CurrentBrushProperty);
        set => SetValue(CurrentBrushProperty, value);
    }

    public int CellSize
    {
        get => GetValue(CellSizeProperty);
        set => SetValue(CellSizeProperty, value);
    }

    public string CurrentTool
    {
        get => GetValue(CurrentToolProperty);
        set => SetValue(CurrentToolProperty, value);
    }

    public event EventHandler<(int X, int Y)>? TileAction;

    // Pan state
    private Point _panOffset;
    private Point _lastMousePosition;
    private bool _isPanning;
    private bool _isDrawing;
    private char[,]? _lastMapData;

    private static readonly Dictionary<char, Color> TileColors = new()
    {
        { '.', Color.Parse("#424242") },
        { '#', Color.Parse("#795548") },
        { '+', Color.Parse("#FF9800") },
        { '-', Color.Parse("#FFB74D") },
        { '~', Color.Parse("#2196F3") },
        { '^', Color.Parse("#F44336") },
        { '*', Color.Parse("#4CAF50") },
        { 'H', Color.Parse("#8D6E63") },
        { 'S', Color.Parse("#9C27B0") },
        { '>', Color.Parse("#FFEB3B") },
        { '<', Color.Parse("#FFEB3B") },
        { 'E', Color.Parse("#F44336") },
        { 'C', Color.Parse("#2196F3") },
        { 'P', Color.Parse("#4CAF50") },
        { 'I', Color.Parse("#FF9800") },
        { ' ', Color.Parse("#1E1E1E") },
    };

    private static readonly Color DefaultColor = Color.Parse("#333333");
    private static readonly Color GridColor = Color.Parse("#333333");
    private static readonly IBrush WhiteBrush = Brushes.White;

    public MapCanvasControl()
    {
        MapDataProperty.Changed.AddClassHandler<MapCanvasControl>((x, _) => x.InvalidateVisual());
        MapWidthProperty.Changed.AddClassHandler<MapCanvasControl>((x, _) => x.InvalidateVisual());
        MapHeightProperty.Changed.AddClassHandler<MapCanvasControl>((x, _) => x.InvalidateVisual());

        ClipToBounds = true;
        Focusable = true;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (MapData == null || MapWidth <= 0 || MapHeight <= 0) return;

        // 切换地图时重置位置
        if (_lastMapData != MapData)
        {
            _lastMapData = MapData;
            _panOffset = new Point(0, 0);
        }

        // 应用平移变换
        using (context.PushTransform(Matrix.CreateTranslation(_panOffset.X, _panOffset.Y)))
        {
            // 绘制背景
            var bgRect = new Rect(0, 0, MapWidth * CellSize, MapHeight * CellSize);
            context.DrawRectangle(Brushes.Black, null, bgRect);

            // 绘制瓦片
            for (int ty = 0; ty < MapHeight; ty++)
            {
                for (int tx = 0; tx < MapWidth; tx++)
                {
                    DrawTile(context, tx, ty, MapData[tx, ty]);
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
        }
    }

    private void DrawTile(DrawingContext context, int tx, int ty, char c)
    {
        var rect = new Rect(tx * CellSize + 1, ty * CellSize + 1, CellSize - 1, CellSize - 1);
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
                tx * CellSize + (CellSize - text.Width) / 2,
                ty * CellSize + (CellSize - text.Height) / 2);

            context.DrawText(text, textPos);
        }
        catch
        {
            // 忽略文本渲染错误
        }
    }

    private Point ScreenToMap(Point screenPos)
    {
        return new Point(
            (screenPos.X - _panOffset.X) / CellSize,
            (screenPos.Y - _panOffset.Y) / CellSize);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var position = e.GetPosition(this);
        var properties = e.GetCurrentPoint(this).Properties;

        if (CurrentTool == "Move" || properties.IsMiddleButtonPressed)
        {
            _isPanning = true;
            _lastMousePosition = position;
            e.Handled = true;
        }
        else if (properties.IsLeftButtonPressed)
        {
            _isDrawing = true;
            var mapPos = ScreenToMap(position);
            int x = (int)mapPos.X;
            int y = (int)mapPos.Y;

            if (x >= 0 && x < MapWidth && y >= 0 && y < MapHeight)
            {
                TileAction?.Invoke(this, (x, y));
            }
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var position = e.GetPosition(this);

        if (_isPanning)
        {
            var delta = position - _lastMousePosition;
            _panOffset += delta;
            _lastMousePosition = position;
            InvalidateVisual();
            e.Handled = true;
        }
        else if (_isDrawing && CurrentTool == "Paint")
        {
            var mapPos = ScreenToMap(position);
            int x = (int)mapPos.X;
            int y = (int)mapPos.Y;

            if (x >= 0 && x < MapWidth && y >= 0 && y < MapHeight)
            {
                TileAction?.Invoke(this, (x, y));
            }
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _isPanning = false;
        _isDrawing = false;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(MapWidth * CellSize, MapHeight * CellSize);
    }
}
