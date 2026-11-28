using Gwire.Models.Base;
using Gwire.Models.Pairs;
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

    /// <summary>
    /// Electrical elements whose terminals reference this part's Points collection.
    /// </summary>
    public List<ConnectionPair> ConnectionPairs { get; set; } = [];

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
        return GetElectricalConnections()
            .Where(connection => ReferenceEquals(connection.From, startingPoint) || ReferenceEquals(connection.To, startingPoint))
            .Select(connection => ReferenceEquals(connection.From, startingPoint) ? connection.To : connection.From)
            .Distinct();
    }

    public override IEnumerable<ElectricalConnection> GetElectricalConnections()
    {
        foreach (var group in StateGroups.Where(group => group.ActiveState is not null)
            .SelectMany(group => group.ActiveState!.ConnectionGroups))
        {
            for (var first = 0; first < group.ConnectedPints.Count - 1; first++)
            {
                for (var second = first + 1; second < group.ConnectedPints.Count; second++)
                {
                    yield return new ElectricalConnection(group.ConnectedPints[first], group.ConnectedPints[second]);
                }
            }
        }
        foreach (var connection in ConnectionPairs.SelectMany(pair => pair.GetElectricalConnections()))
        {
            yield return connection;
        }
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
    /// Switches every state group to its first state, as a part placed into a scheme starts.
    /// </summary>
    public void ResetStates()
    {
        foreach (var group in StateGroups)
        {
            group.ActiveState = group.States.FirstOrDefault();
        }
    }

    /// <summary>
    /// Adds the point to the part.
    /// </summary>
    public void AddPoint(ConnectionPoint point) => Points.Add(point);

    /// <summary>
    /// Disconnects the point, removes it from the part and from every connection group of every state.
    /// </summary>
    public void RemovePoint(ConnectionPoint point)
    {
        foreach (var connected in point.ConnectionPoints.ToArray())
        {
            point.Disconnect(connected);
        }
        Points.Remove(point);
        foreach (var group in StateGroups.SelectMany(stateGroup => stateGroup.States)
            .SelectMany(state => state.ConnectionGroups))
        {
            group.ConnectedPints.Remove(point);
        }
    }

    /// <summary>
    /// Adds the pair and all of its existing terminals to the part.
    /// </summary>
    public void AddPair(ConnectionPair pair)
    {
        foreach (var point in pair.GetPoints())
        {
            AddPoint(point);
        }
        ConnectionPairs.Add(pair);
    }

    /// <summary>
    /// Removes the pair together with all of its terminals.
    /// </summary>
    public void RemovePair(ConnectionPair pair)
    {
        foreach (var point in pair.GetPoints())
        {
            RemovePoint(point);
        }
        ConnectionPairs.Remove(pair);
    }

    /// <summary>
    /// Name of the pair that owns the point, or null for a free point.
    /// </summary>
    public string? GetElementName(ConnectionPoint point) =>
        ConnectionPairs.FirstOrDefault(pair => pair.Contains(point))?.Name;

    /// <summary>
    /// Whether the point is a terminal of a pair and so cannot be deleted on its own.
    /// </summary>
    public bool IsElementPoint(ConnectionPoint point) => ConnectionPairs.Any(pair => pair.Contains(point));

    /// <summary>
    /// Whether every potentiometer has a name and a positive total resistance.
    /// </summary>
    public bool HasValidPotentiometers() => ConnectionPairs.OfType<Potentiometer>().All(potentiometer => potentiometer.IsValid);


    /// <summary>
    /// Clones the shared connection graph, dimensions, the part's tags, and the active state of each state group.
    /// </summary>
    public override CircuitPart Clone()
    {
        var clone = (CircuitPart)base.Clone();
        var clonedPoints = Points.Zip(clone.Points).ToDictionary(pair => pair.First, pair => pair.Second);
        clone.ConnectionPairs = ConnectionPairs.Select(pair => pair.Clone(clonedPoints)).ToList();
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
                ActiveState = states.ElementAtOrDefault(stateGroup.ActiveState is null ? -1 : stateGroup.States.IndexOf(stateGroup.ActiveState))
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
