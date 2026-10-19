using Gwire.UndoRedo;

namespace Gwire.Services;

public sealed partial class UndoRedoService
{
    /// <summary>
    /// Forwards history operations to the service using a fixed history identifier.
    /// </summary>
    public sealed class UndoRedoHistoryWrapper
    {
        private readonly UndoRedoService service;
        private readonly string historyId;
        private Action? changed;

        internal UndoRedoHistoryWrapper(UndoRedoService service, string historyId)
        {
            this.service = service;
            this.historyId = historyId;
        }

        public bool HasEntries => service.HasEntries(historyId);
        public bool IsFrozen => service.IsFrozen(historyId);
        public bool CanUndo => service.CanUndo(historyId);
        public bool CanRedo => service.CanRedo(historyId);

        public event Action? Changed
        {
            add
            {
                if (changed is null)
                {
                    service.Changed += HandleChanged;
                }
                changed += value;
            }
            remove
            {
                changed -= value;
                if (changed is null)
                {
                    service.Changed -= HandleChanged;
                }
            }
        }

        public void addRedoAction(IUndoableAction action) => service.addRedoAction(historyId, action);
        public void Undo() => service.Undo(historyId);
        public void Redo() => service.Redo(historyId);
        public void Clear() => service.Clear(historyId);
        public void Freeze() => service.Freeze(historyId);
        public void Unfreeze() => service.Unfreeze(historyId);

        private void HandleChanged(string changedHistoryId)
        {
            if (changedHistoryId == historyId)
            {
                changed?.Invoke();
            }
        }
    }
}
