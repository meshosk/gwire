using Gwire.Models;
using Gwire.Models.Base;

namespace Gwire.UndoRedo.Actions;

public sealed class RemovePartAction : IUndoableAction
{
    private readonly Circuit circuit;
    private readonly BaseCircuitPart part;
    private readonly int index;
    private readonly (ConnectionPoint Point, ConnectionPoint[] Connections)[] connections;
    private readonly (ConnectionPoint Point, Point Position)[] cablePositions;

    public RemovePartAction(Circuit circuit, BaseCircuitPart part)
        : this(circuit, part, circuit.Parts.IndexOf(part))
    {
    }

    internal RemovePartAction(Circuit circuit, BaseCircuitPart part, int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        this.circuit = circuit;
        this.part = part;
        this.index = index;
        connections = part.Points.Select(point => (point, point.ConnectionPoints.ToArray())).ToArray();
        cablePositions = part.Points.Concat(part.Points.SelectMany(point => point.ConnectionPoints))
            .Where(point => point.Owner is CablePart)
            .Distinct()
            .Select(point => (point, point.LocalPosition.Copy()))
            .ToArray();
    }

    public void Undo()
    {
        circuit.Parts.Insert(Math.Min(index, circuit.Parts.Count), part);
        foreach (var (point, position) in cablePositions)
        {
            point.LocalPosition = position.Copy();
        }
        foreach (var (point, connectedPoints) in connections)
        {
            foreach (var connection in connectedPoints)
            {
                point.Connect(connection);
            }
        }
        foreach (var owner in cablePositions.Select(entry => entry.Point.Owner).Append(part).Distinct())
        {
            owner.NotifyChanged();
        }
    }

    public void Redo()
    {
        foreach (var point in part.Points)
        {
            foreach (var connection in point.ConnectionPoints.ToArray())
            {
                point.Disconnect(connection);
            }
        }
        circuit.Parts.Remove(part);
        if (ReferenceEquals(circuit.SelectedPart, part))
        {
            circuit.SelectedPart = null;
        }
    }
}
