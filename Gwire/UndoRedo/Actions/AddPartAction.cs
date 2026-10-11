using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Pages;

namespace Gwire.UndoRedo.Actions;

public sealed class AddPartAction : IUndoableAction
{
    private readonly RemovePartAction removal;

    public AddPartAction(Circuit circuit, SchemeEditor editor, BaseCircuitPart part)
    {
        removal = new RemovePartAction(circuit, editor, part, circuit.Parts.Count);
    }

    public static AddPartAction CreateCable(Circuit circuit, SchemeEditor editor)
    {
        var cableNumber = circuit.Parts.OfType<CablePart>().Count() + 1;
        var cable = new CablePart
        {
            Name = $"Cable {cableNumber}"
        };

        cable.Points[0].LocalPosition = new Point(300, 300 + (cableNumber - 1) * 40);
        cable.Points[1].LocalPosition = new Point(500, 300 + (cableNumber - 1) * 40);
        return new AddPartAction(circuit, editor, cable);
    }

    public static AddPartAction CreatePart(Circuit circuit, SchemeEditor editor, CircuitPart selectedPart)
    {
        var part = selectedPart.Clone();
        var partNumber = circuit.Parts.OfType<CircuitPart>().Count();
        part.SchemePosition = new Point(150 + partNumber * 25, 150 + partNumber * 25);
        return new AddPartAction(circuit, editor, part);
    }

    public void Undo() => removal.Redo();
    public void Redo() => removal.Undo();
}
