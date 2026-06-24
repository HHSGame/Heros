using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using HHSEditor.ViewModels;

namespace HHSEditor.Controls;

public class QuestGraphControl : Control
{
    public static readonly StyledProperty<List<QuestGraphNode>?> NodesProperty =
        AvaloniaProperty.Register<QuestGraphControl, List<QuestGraphNode>?>(nameof(Nodes));

    public List<QuestGraphNode>? Nodes
    {
        get => GetValue(NodesProperty);
        set => SetValue(NodesProperty, value);
    }

    private const int NodeWidth = 200;
    private const int NodeHeight = 90;
    private const int HorizontalSpacing = 40;
    private const int VerticalSpacing = 30;
    private const int Margin = 30;

    public QuestGraphControl()
    {
        NodesProperty.Changed.AddClassHandler<QuestGraphControl>((x, _) => x.InvalidateVisual());
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Nodes == null || Nodes.Count == 0)
        {
            var text = new FormattedText("No quests loaded",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                16, Brushes.Gray);
            context.DrawText(text, new Point(20, 20));
            return;
        }

        // Calculate grid layout
        var positions = CalculateLayout();

        // Draw connections (quest dependencies would be drawn here)
        // For now, just draw the nodes

        // Draw nodes
        foreach (var node in Nodes)
        {
            if (!positions.TryGetValue(node.Id, out var pos)) continue;

            DrawNode(context, pos, node);
        }
    }

    private Dictionary<string, Point> CalculateLayout()
    {
        var positions = new Dictionary<string, Point>();
        if (Nodes == null || Nodes.Count == 0) return positions;

        int columns = (int)Math.Ceiling(Math.Sqrt(Nodes.Count));
        int x = Margin;
        int y = Margin;
        int col = 0;

        foreach (var node in Nodes)
        {
            positions[node.Id] = new Point(x, y);

            col++;
            x += NodeWidth + HorizontalSpacing;
            if (col >= columns)
            {
                col = 0;
                x = Margin;
                y += NodeHeight + VerticalSpacing;
            }
        }

        return positions;
    }

    private void DrawNode(DrawingContext context, Point position, QuestGraphNode node)
    {
        var rect = new Rect(position.X, position.Y, NodeWidth, NodeHeight);

        // Draw shadow
        var shadowRect = new Rect(position.X + 3, position.Y + 3, NodeWidth, NodeHeight);
        context.DrawRectangle(new SolidColorBrush(Color.Parse("#E0E0E0")), null, shadowRect);

        // Draw background
        var bgColor = node.IsSelected ? "#BBDEFB" : "#F5F5F5";
        var borderColor = node.IsSelected ? "#1976D2" : "#9E9E9E";
        var brush = new SolidColorBrush(Color.Parse(bgColor));
        var pen = new Pen(new SolidColorBrush(Color.Parse(borderColor)), node.IsSelected ? 3 : 1);
        context.DrawRectangle(brush, pen, rect);

        // Draw ID header
        var idText = new FormattedText(node.Id,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial", FontStyle.Normal, FontWeight.Bold),
            11, Brushes.DarkBlue);
        context.DrawText(idText, new Point(position.X + 6, position.Y + 4));

        // Draw separator
        context.DrawLine(new Pen(Brushes.LightGray, 1),
            new Point(position.X + 4, position.Y + 20),
            new Point(position.X + NodeWidth - 4, position.Y + 20));

        // Draw name
        var nameText = new FormattedText(node.Name,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial", FontStyle.Normal, FontWeight.Bold),
            12, Brushes.Black);
        context.DrawText(nameText, new Point(position.X + 6, position.Y + 24));

        // Draw description
        var descText = new FormattedText(node.Description,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial"),
            10, Brushes.DarkGray);
        context.DrawText(descText, new Point(position.X + 6, position.Y + 42));

        // Draw stats
        var statsText = new FormattedText($"Obj: {node.ObjectiveCount} | Rew: {node.RewardCount}",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial"),
            9, Brushes.Gray);
        context.DrawText(statsText, new Point(position.X + 6, position.Y + 68));
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Nodes == null || Nodes.Count == 0)
            return new Size(600, 400);

        int columns = (int)Math.Ceiling(Math.Sqrt(Nodes.Count));
        int rows = (int)Math.Ceiling((double)Nodes.Count / columns);

        double width = columns * (NodeWidth + HorizontalSpacing) - HorizontalSpacing + Margin * 2;
        double height = rows * (NodeHeight + VerticalSpacing) - VerticalSpacing + Margin * 2;

        return new Size(Math.Max(width, 600), Math.Max(height, 400));
    }
}
