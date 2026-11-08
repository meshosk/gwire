using Gwire.Models;
using Gwire.Models.Base;

namespace Gwire.UndoRedo.Actions;

public sealed class AddPartAction : IUndoableAction
{
    private readonly RemovePartAction removal;

    public AddPartAction(Circuit circuit, SchemeEditorState editor, BaseCircuitPart part)
    {
        removal = new RemovePartAction(circuit, editor, part, circuit.Parts.Count);
    }

    public static AddPartAction CreateCable(Circuit circuit, SchemeEditorState editor, Point? position = null)
    {
        var cableNumber = circuit.Parts.OfType<CablePart>().Count() + 1;
        var cable = new CablePart
        {
            Name = $"Cable {cableNumber}"
        };

        if (position is { } center)
        {
            cable.Points[0].LocalPosition = new Point(center.X - 100, center.Y);
            cable.Points[1].LocalPosition = new Point(center.X + 100, center.Y);
        }
        else
        {
            cable.Points[0].LocalPosition = new Point(300, 300 + (cableNumber - 1) * 40);
            cable.Points[1].LocalPosition = new Point(500, 300 + (cableNumber - 1) * 40);
        }
        return new AddPartAction(circuit, editor, cable);
    }

    public static AddPartAction CreatePart(Circuit circuit, SchemeEditorState editor, CircuitPart selectedPart, Point? position = null)
    {
        var part = selectedPart.Clone();
        if (position is { } center)
        {
            part.SchemePosition = new Point(center.X - part.SvgLocalPosition.X - part.Width / 2f,
                center.Y - part.SvgLocalPosition.Y - part.Height / 2f);
        }
        else
        {
            var partNumber = circuit.Parts.OfType<CircuitPart>().Count();
            part.SchemePosition = new Point(150 + partNumber * 25, 150 + partNumber * 25);
        }
        return new AddPartAction(circuit, editor, part);
    }

    public void Undo() => removal.Redo();
    public void Redo() => removal.Undo();
}
