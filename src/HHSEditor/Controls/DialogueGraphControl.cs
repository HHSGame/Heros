using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using HHSEditor.ViewModels;

namespace HHSEditor.Controls;

public class DialogueGraphControl : Control
{
    public static readonly StyledProperty<List<DialogueGraphNode>?> NodesProperty =
        AvaloniaProperty.Register<DialogueGraphControl, List<DialogueGraphNode>?>(nameof(Nodes));

    public List<DialogueGraphNode>? Nodes
    {
        get => GetValue(NodesProperty);
        set => SetValue(NodesProperty, value);
    }

    private const int NodeWidth = 180;
    private const int NodeHeight = 70;
    private const int HorizontalSpacing = 60;
    private const int VerticalSpacing = 100;
    private const int Margin = 30;

    public DialogueGraphControl()
    {
        // Subscribe to property changes to invalidate visual
        NodesProperty.Changed.AddClassHandler<DialogueGraphControl>((x, _) => x.InvalidateVisual());
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Nodes == null || Nodes.Count == 0)
        {
            var text = new FormattedText("No dialogue selected",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                16, Brushes.Gray);
            context.DrawText(text, new Point(20, 20));
            return;
        }

        // Calculate layout using topological sort
        var positions = CalculateLayout();

        // Draw edges first (behind nodes)
        foreach (var node in Nodes)
        {
            if (!positions.TryGetValue(node.Id, out var fromPos)) continue;

            foreach (var option in node.Options)
            {
                if (string.IsNullOrEmpty(option.TargetNodeId)) continue;
                if (!positions.TryGetValue(option.TargetNodeId, out var toPos)) continue;

                DrawEdge(context, fromPos, toPos, option.Text, node.Id == option.TargetNodeId);
            }
        }

        // Draw nodes on top
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

        // Build adjacency list for topological sort
        var inDegree = new Dictionary<string, int>();
        var adjacency = new Dictionary<string, List<string>>();

        foreach (var node in Nodes)
        {
            inDegree[node.Id] = 0;
            adjacency[node.Id] = [];
        }

        foreach (var node in Nodes)
        {
            foreach (var option in node.Options)
            {
                if (!string.IsNullOrEmpty(option.TargetNodeId) &&
                    adjacency.ContainsKey(option.TargetNodeId) &&
                    option.TargetNodeId != node.Id) // Skip self-loops for ordering
                {
                    adjacency[option.TargetNodeId].Add(node.Id);
                    inDegree[node.Id]++;
                }
            }
        }

        // Topological sort using BFS
        var queue = new Queue<string>();
        foreach (var kvp in inDegree)
        {
            if (kvp.Value == 0)
            {
                queue.Enqueue(kvp.Key);
            }
        }

        var layers = new List<List<string>>();
        var visited = new HashSet<string>();

        while (queue.Count > 0)
        {
            var layer = new List<string>();
            int count = queue.Count;

            for (int i = 0; i < count; i++)
            {
                var nodeId = queue.Dequeue();
                if (visited.Contains(nodeId)) continue;
                visited.Add(nodeId);
                layer.Add(nodeId);

                foreach (var neighbor in adjacency[nodeId])
                {
                    inDegree[neighbor]--;
                    if (inDegree[neighbor] == 0)
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            if (layer.Count > 0)
            {
                layers.Add(layer);
            }
        }

        // Add any remaining nodes (cycles)
        var remaining = Nodes.Where(n => !visited.Contains(n.Id)).Select(n => n.Id).ToList();
        if (remaining.Count > 0)
        {
            layers.Add(remaining);
        }

        // Position nodes by layer
        int y = Margin;
        foreach (var layer in layers)
        {
            int totalWidth = layer.Count * (NodeWidth + HorizontalSpacing) - HorizontalSpacing;
            int x = Math.Max(Margin, (600 - totalWidth) / 2); // Center horizontally

            foreach (var nodeId in layer)
            {
                positions[nodeId] = new Point(x, y);
                x += NodeWidth + HorizontalSpacing;
            }

            y += NodeHeight + VerticalSpacing;
        }

        return positions;
    }

    private void DrawNode(DrawingContext context, Point position, DialogueGraphNode node)
    {
        var rect = new Rect(position.X, position.Y, NodeWidth, NodeHeight);

        // Draw shadow
        var shadowRect = new Rect(position.X + 3, position.Y + 3, NodeWidth, NodeHeight);
        context.DrawRectangle(new SolidColorBrush(Color.Parse("#E0E0E0")), null, shadowRect);

        // Draw background with gradient effect
        var brush = new SolidColorBrush(Color.Parse("#E3F2FD"));
        var pen = new Pen(new SolidColorBrush(Color.Parse("#1976D2")), 2);
        context.DrawRectangle(brush, pen, rect);

        // Draw ID header
        var idText = new FormattedText(node.Id,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial", FontStyle.Normal, FontWeight.Bold),
            11, Brushes.DarkBlue);
        context.DrawText(idText, new Point(position.X + 6, position.Y + 4));

        // Draw separator line
        context.DrawLine(new Pen(Brushes.LightGray, 1),
            new Point(position.X + 4, position.Y + 20),
            new Point(position.X + NodeWidth - 4, position.Y + 20));

        // Draw text (truncated)
        var displayText = node.Text.Length > 35 ? node.Text[..35] + "..." : node.Text;
        var text = new FormattedText(displayText,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial"),
            10, Brushes.Black);
        context.DrawText(text, new Point(position.X + 6, position.Y + 24));

        // Draw option count badge
        var optText = new FormattedText($"{node.Options.Count} opts",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial"),
            9, Brushes.Gray);
        context.DrawText(optText, new Point(position.X + 6, position.Y + 48));

        // Draw outgoing connection count
        var connText = new FormattedText($"→{node.Options.Count(o => !string.IsNullOrEmpty(o.TargetNodeId))}",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial"),
            9, Brushes.DarkGreen);
        context.DrawText(connText, new Point(position.X + 60, position.Y + 48));
    }

    private void DrawEdge(DrawingContext context, Point from, Point to, string label, bool isSelfLoop)
    {
        var fromCenter = new Point(from.X + NodeWidth / 2, from.Y + NodeHeight);
        var toCenter = new Point(to.X + NodeWidth / 2, to.Y);

        if (isSelfLoop)
        {
            // Draw self-loop
            DrawSelfLoop(context, from, label);
            return;
        }

        // Calculate connection points on node borders
        var fromPoint = GetBorderPoint(from, NodeWidth, NodeHeight, toCenter);
        var toPoint = GetBorderPoint(to, NodeWidth, NodeHeight, fromCenter);

        // Draw curved line
        var midY = (fromPoint.Y + toPoint.Y) / 2;
        var controlPoint1 = new Point(fromPoint.X, midY);
        var controlPoint2 = new Point(toPoint.X, midY);

        var geometry = new StreamGeometry();
        using (var sg = geometry.Open())
        {
            sg.BeginFigure(fromPoint, false);
            sg.CubicBezierTo(controlPoint1, controlPoint2, toPoint);
            sg.EndFigure(false);
        }
        context.DrawGeometry(null, new Pen(Brushes.Gray, 1.5), geometry);

        // Draw arrow
        var angle = Math.Atan2(toPoint.Y - controlPoint2.Y, toPoint.X - controlPoint2.X);
        DrawArrow(context, toPoint, angle);

        // Draw label at midpoint
        var midPoint = new Point((fromPoint.X + toPoint.X) / 2, midY - 12);
        DrawEdgeLabel(context, midPoint, label);
    }

    private void DrawSelfLoop(DrawingContext context, Point nodePos, string label)
    {
        var center = new Point(nodePos.X + NodeWidth + 20, nodePos.Y + NodeHeight / 2);
        var radius = 15;

        context.DrawEllipse(null, new Pen(Brushes.Gray, 1.5),
            new Rect(center.X - radius, center.Y - radius, radius * 2, radius * 2));

        DrawEdgeLabel(context, new Point(center.X, center.Y - radius - 10), label);
    }

    private Point GetBorderPoint(Rect nodeRect, double targetX, double targetY)
    {
        return GetBorderPoint(new Point(nodeRect.X, nodeRect.Y), NodeWidth, NodeHeight, new Point(targetX, targetY));
    }

    private Point GetBorderPoint(Point nodePos, double width, double height, Point target)
    {
        var centerX = nodePos.X + width / 2;
        var centerY = nodePos.Y + height / 2;

        var dx = target.X - centerX;
        var dy = target.Y - centerY;

        if (Math.Abs(dx) < 0.001 && Math.Abs(dy) < 0.001)
            return new Point(centerX, centerY);

        var absDx = Math.Abs(dx);
        var absDy = Math.Abs(dy);

        if (absDx / width > absDy / height)
        {
            // Hit left or right border
            var sign = dx > 0 ? 1 : -1;
            return new Point(centerX + sign * width / 2, centerY + dy * width / (2 * absDx));
        }
        else
        {
            // Hit top or bottom border
            var sign = dy > 0 ? 1 : -1;
            return new Point(centerX + dx * height / (2 * absDy), centerY + sign * height / 2);
        }
    }

    private void DrawArrow(DrawingContext context, Point tip, double angle)
    {
        var arrowSize = 8;
        var p1 = new Point(tip.X - arrowSize * Math.Cos(angle - Math.PI / 6),
                          tip.Y - arrowSize * Math.Sin(angle - Math.PI / 6));
        var p2 = new Point(tip.X - arrowSize * Math.Cos(angle + Math.PI / 6),
                          tip.Y - arrowSize * Math.Sin(angle + Math.PI / 6));

        var geometry = new StreamGeometry();
        using (var sg = geometry.Open())
        {
            sg.BeginFigure(tip, true);
            sg.LineTo(p1);
            sg.LineTo(p2);
            sg.EndFigure(true);
        }
        context.DrawGeometry(Brushes.Gray, null, geometry);
    }

    private void DrawEdgeLabel(DrawingContext context, Point position, string label)
    {
        var truncatedLabel = label.Length > 20 ? label[..20] + "..." : label;

        // Draw background
        var text = new FormattedText(truncatedLabel,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial", FontStyle.Italic),
            9, Brushes.DarkSlateGray);

        var bgRect = new Rect(position.X - 2, position.Y - 2, text.Width + 4, text.Height + 4);
        context.DrawRectangle(new SolidColorBrush(Color.Parse("#FFF9C4")), null, bgRect);

        context.DrawText(text, position);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Nodes == null || Nodes.Count == 0)
            return new Size(600, 400);

        var positions = CalculateLayout();
        double maxX = 0, maxY = 0;

        foreach (var pos in positions.Values)
        {
            maxX = Math.Max(maxX, pos.X + NodeWidth);
            maxY = Math.Max(maxY, pos.Y + NodeHeight);
        }

        return new Size(Math.Max(maxX + Margin * 2, 600), Math.Max(maxY + Margin * 2, 400));
    }
}
