namespace Gwire.UndoRedo;

public interface IUndoableAction
{
    void Undo();
    void Redo();
}
