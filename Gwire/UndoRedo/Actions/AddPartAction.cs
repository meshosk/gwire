using Gwire.Models;
using Gwire.Models.Base;

namespace Gwire.UndoRedo.Actions;

public sealed class AddPartAction : IUndoableAction
{
    private readonly RemovePartAction removal;

    public AddPartAction(Circuit circuit, BaseCircuitPart part)
    {
        removal = new RemovePartAction(circuit, part, circuit.Parts.Count);
    }

    public void Undo() => removal.Redo();
    public void Redo() => removal.Undo();
}
