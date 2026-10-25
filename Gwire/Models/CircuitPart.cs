using Gwire.Models.Base;
using System.Text.Json.Serialization;

namespace Gwire.Models;

/// <summary>
/// All other parts than cable
/// </summary>
public sealed class CircuitPart : BaseCircuitPart, IJsonOnDeserialized
{
    /// <summary>
    /// Each group has its own independently selected active state.
    /// </summary>
    public List<PartStateGroup> StateGroups { get; set; } = [new()];

    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// if true part was loaded from repo
    /// </summary>
    [JsonIgnore]
    public bool IsFromRepo { get; set; }

    /// <summary>
    /// Use this for gerring part name
    /// </summary>
    [JsonIgnore]
    public string DisplayName => IsFromRepo ? $"*{Name}" : Name;

    /// <summary>
    /// Width of the part in pixels.
    /// </summary>
    public int Width { get; set; } = 500;

    /// <summary>
    /// Height of the part in pixels.
    /// </summary>
    public int Height { get; set; } = 500;

    /// <summary>
    /// Tags used to search and filter parts in the catalog.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// SVG markup used as the visual background of the part.
    /// </summary>
    public string SvgMarkup { get; set; } = string.Empty;

    /// <summary>
    /// Example text in the catalog and the actual text in a scheme. Null means this part has no editable text.
    /// </summary>
    public string? CustomTextValue { get; set; }

    /// <summary>
    /// Local anchor position of the horizontally centered value text.
    /// </summary>
    public Point ValuePosition { get; set; } = new(120, 80);

    /// <summary>
    /// Rotation of the SVG background in degrees around the center of the crop rectangle.
    /// </summary>
    public int SvgRotation { get; set; }

    /// <summary>
    /// Top-left position of the SVG background in the part's local coordinate system.
    /// </summary>
    public Point SvgLocalPosition { get; set; } = new();

    /// <summary>
    /// Position of the part in a circuit scheme.
    /// </summary>
    public Point SchemePosition { get; set; } = new();

    /// <summary>
    /// Moves the part and updates the stored coordinates of its connected cable points.
    /// </summary>
    public void MoveTo(Point position)
    {
        SchemePosition = position.Copy();
        UpdateConnectedCablePoints();
    }


    void IJsonOnDeserialized.OnDeserialized()
    {
        if (StateGroups.Count == 0)
        {
            StateGroups.Add(new PartStateGroup());
        }
    }

    public override IEnumerable<ConnectionPoint> GetConnectedPoints(ConnectionPoint startingPoint)
    {
        return StateGroups
            .Where(group => group.ActiveState is not null)
            .SelectMany(group => group.ActiveState!.GetConnectedPoints(startingPoint))
            .Distinct();
    }

    public void UpdateConnectedCablePoints()
    {
        var cables = new HashSet<CablePart>();
        foreach (var point in Points)
        {
            foreach (var connectedPoint in point.ConnectionPoints)
            {
                if (connectedPoint.Owner is CablePart cable)
                {
                    connectedPoint.LocalPosition = new Point(SchemePosition.X + point.LocalPosition.X,
                        SchemePosition.Y + point.LocalPosition.Y);
                    cables.Add(cable);
                }
            }
        }

        NotifyChanged();
        foreach (var cable in cables)
        {
            cable.NotifyChanged();
        }
    }


    /// <summary>
    /// Clones the shared connection graph, dimensions, and the part's tags.
    /// </summary>
    public override CircuitPart Clone()
    {
        var clone = (CircuitPart)base.Clone();
        var clonedPoints = Points.Zip(clone.Points).ToDictionary(pair => pair.First, pair => pair.Second);
        clone.StateGroups = StateGroups.Select(stateGroup =>
        {
            var states = stateGroup.States.Select(state => new PartState
            {
                Label = state.Label,
                ConnectionGroups = state.ConnectionGroups.Select(group => new ConnectionGroup
                {
                    ConnectedPints = group.ConnectedPints.Select(point => clonedPoints[point]).ToList()
                }).ToList()
            }).ToList();
            return new PartStateGroup
            {
                Label = stateGroup.Label,
                States = states,
                ActiveState = states.FirstOrDefault()
            };
        }).ToList();

        clone.SvgLocalPosition = SvgLocalPosition.Copy();
        clone.SchemePosition = SchemePosition.Copy();
        clone.ValuePosition = ValuePosition.Copy();
        clone.Width = Width;
        clone.Height = Height;
        clone.Tags = [.. Tags];
        return clone;
    }
}
