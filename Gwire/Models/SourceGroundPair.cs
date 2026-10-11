using Gwire.Models.Base;

namespace Gwire.Models;

/// <summary>
/// An editor-only power source and ground used to trace a path through the scheme.
/// </summary>
public sealed class SourceGroundPair
{
    public CircuitPart PowerSource { get; set; } = CreateTerminal("Power source", new Point(100, 160), "#d63344",
        "<circle cx=\"0\" cy=\"-44\" r=\"15\" /><path d=\"M -6 -49 H 6 M 0 -55 V -43 M -6 -36 H 6\" />");
    public CircuitPart Ground { get; set; } = CreateTerminal("Ground", new Point(220, 160), "#495057",
        "<path d=\"M 0 -59 V -42 M -14 -42 H 14 M -9 -35 H 9 M -4 -28 H 4\" />");

    public ConnectionPoint? PowerSourceAttachment { get; private set; }
    public ConnectionPoint? GroundAttachment { get; private set; }
    public string LineColor { get; set; } = "#ff0000";

    public ConnectionPoint? GetAttachment(CircuitPart terminal) => ReferenceEquals(terminal, PowerSource)
        ? PowerSourceAttachment
        : ReferenceEquals(terminal, Ground)
            ? GroundAttachment
            : throw new ArgumentException("The terminal does not belong to this pair.", nameof(terminal));

    public Point GetPosition(CircuitPart terminal, IReadOnlyList<SourceGroundPair> pairs)
    {
        if (GetAttachment(terminal) is not { } attachment)
        {
            return terminal.SchemePosition.Copy();
        }
        var position = GetPointPosition(attachment, pairs);
        return new Point(position.X - terminal.Points[0].LocalPosition.X,
            position.Y - terminal.Points[0].LocalPosition.Y);
    }

    public static Point GetPointPosition(ConnectionPoint point, IReadOnlyList<SourceGroundPair> pairs)
    {
        if (point.Owner is not CircuitPart part)
        {
            return point.LocalPosition.Copy();
        }
        var pair = pairs.FirstOrDefault(candidate => ReferenceEquals(candidate.PowerSource, part) || ReferenceEquals(candidate.Ground, part));
        var position = pair is null ? part.SchemePosition : pair.GetPosition(part, pairs);
        return new Point(position.X + point.LocalPosition.X, position.Y + point.LocalPosition.Y);
    }

    public bool CanAttachTo(CircuitPart terminal, ConnectionPoint point, IReadOnlyList<SourceGroundPair> pairs)
    {
        var visited = new HashSet<ConnectionPoint>();
        while (visited.Add(point))
        {
            if (ReferenceEquals(point.Owner, terminal))
            {
                return false;
            }
            var pair = pairs.FirstOrDefault(candidate => ReferenceEquals(candidate.PowerSource, point.Owner) ||
                ReferenceEquals(candidate.Ground, point.Owner));
            if (pair is null || pair.GetAttachment((CircuitPart)point.Owner) is not { } attachment)
            {
                return true;
            }
            point = attachment;
        }
        return false;
    }

    public void AttachTo(CircuitPart terminal, ConnectionPoint? point, IReadOnlyList<SourceGroundPair> pairs)
    {
        if (point is not null && !CanAttachTo(terminal, point, pairs))
        {
            return;
        }
        var position = GetPosition(terminal, pairs);
        if (ReferenceEquals(terminal, PowerSource))
        {
            PowerSourceAttachment = point;
        }
        else
        {
            GroundAttachment = point;
        }
        terminal.MoveTo(position);
    }

    private static CircuitPart CreateTerminal(string name, Point position, string color, string symbol)
    {
        var terminal = new CircuitPart
        {
            Name = name,
            Width = 64,
            Height = 92,
            SchemePosition = new Point(position.X - 32, position.Y - 90),
            SvgMarkup = $"""
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 92">
                    <polygon points="32,2 62,46 32,90 2,46" fill="white" stroke="{color}" stroke-width="2" />
                    <g transform="translate(32 90)" fill="none" stroke="{color}" stroke-width="2.5">{symbol}</g>
                </svg>
                """
        };
        terminal.Points.Add(new ConnectionPoint(terminal)
        {
            Label = name,
            LocalPosition = new Point(32, 90)
        });
        return terminal;
    }
}
