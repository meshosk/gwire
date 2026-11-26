using Gwire.Models;

namespace Gwire.UndoRedo.Actions;

public class ChangePotentiometerPositionAction(CircuitPart part, Potentiometer potentiometer, int after) : IUndoableAction
{
    private readonly int before = potentiometer.Position;

    public void Undo() => Restore(before);

    public void Redo() => Restore(after);

    private void Restore(int position)
    {
        potentiometer.Position = position;
        part.NotifyChanged();
    }
}
