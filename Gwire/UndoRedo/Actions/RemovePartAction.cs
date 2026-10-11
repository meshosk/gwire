using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Pages;

namespace Gwire.UndoRedo.Actions;

public sealed class RemovePartAction : IUndoableAction
{
    private readonly Circuit circuit;
    private readonly SchemeEditor editor;
    private readonly BaseCircuitPart part;
    private readonly int index;
    private readonly (ConnectionPoint Point, ConnectionPoint[] Connections)[] connections;
    private readonly (ConnectionPoint Point, Point Position)[] cablePositions;
    private readonly (SourceGroundPair Pair, CircuitPart Terminal, ConnectionPoint Attachment)[] terminalAttachments;

    public RemovePartAction(Circuit circuit, SchemeEditor editor, BaseCircuitPart part)
        : this(circuit, editor, part, circuit.Parts.IndexOf(part))
    {
    }

    internal RemovePartAction(Circuit circuit, SchemeEditor editor, BaseCircuitPart part, int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        this.circuit = circuit;
        this.editor = editor;
        this.part = part;
        this.index = index;
        terminalAttachments = editor.SourceGroundPairs.SelectMany(pair => new[] { pair.PowerSource, pair.Ground }
            .Where(terminal => ReferenceEquals(pair.GetAttachment(terminal)?.Owner, part))
            .Select(terminal => (pair, terminal, pair.GetAttachment(terminal)!)))
            .ToArray();
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
        foreach (var (pair, terminal, attachment) in terminalAttachments)
        {
            pair.AttachTo(terminal, attachment, editor.SourceGroundPairs);
        }
        foreach (var owner in cablePositions.Select(entry => entry.Point.Owner).Append(part).Distinct())
        {
            owner.NotifyChanged();
        }
    }

    public void Redo()
    {
        foreach (var (pair, terminal, _) in terminalAttachments)
        {
            pair.AttachTo(terminal, null, editor.SourceGroundPairs);
        }
        foreach (var point in part.Points)
        {
            foreach (var connection in point.ConnectionPoints.ToArray())
            {
                point.Disconnect(connection);
            }
        }
        circuit.Parts.Remove(part);
        if (ReferenceEquals(editor.SelectedPart, part))
        {
            editor.SelectedPart = null;
        }
    }
}
