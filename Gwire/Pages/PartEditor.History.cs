using Gwire.Models;
using Gwire.Services;
using Gwire.UndoRedo;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gwire.Pages;

public partial class PartEditor
{
    private static readonly JsonSerializerOptions snapshotOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve
    };

    [Inject]
    private UndoRedoService UndoRedoService { get; set; } = default!;

    private UndoRedoService.UndoRedoHistoryWrapper history => UndoRedoService.getHistory(HistoryIds.PartEditor);

    private PartSnapshot? pendingEdit;
    private ElementReference editorElement;
    private DotNetObjectReference<PartEditor>? shortcutReference;

    private sealed record PartSnapshot(CircuitPart Part, int SelectedPointIndex, int ActiveGroupIndex);

    private sealed class SnapshotAction(PartEditor editor, PartSnapshot initialSnapshot) : IUndoableAction
    {
        private PartSnapshot snapshot = initialSnapshot;

        public void Undo() => SwapSnapshot();
        public void Redo() => SwapSnapshot();

        private void SwapSnapshot()
        {
            var current = editor.CaptureSnapshot();
            editor.RestoreSnapshot(snapshot);
            snapshot = current;
        }
    }

    protected override void OnInitialized() => history.Changed += HandleHistoryChanged;

    private void HandleHistoryChanged() => StateHasChanged();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            shortcutReference = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("gwire.enableUndoRedoShortcuts", editorElement, shortcutReference);
        }
    }

    [JSInvokable]
    public void HandleHistoryShortcut(bool redo)
    {
        if (redo)
        {
            history.Redo();
        }
        else
        {
            history.Undo();
        }
    }

    private PartSnapshot CaptureSnapshot() => new(
        EditedPart.Clone(),
        SelectedPointInEditor is null ? -1 : EditedPart.Points.IndexOf(SelectedPointInEditor),
        ActiveConnectionGroup is null ? -1 : ActiveStateInEditor?.ConnectionGroups.IndexOf(ActiveConnectionGroup) ?? -1);

    private void BeginEdit()
    {
        if (pendingEdit is not null)
        {
            return;
        }

        pendingEdit = CaptureSnapshot();
        history.Freeze();
    }

    private void CompleteEdit()
    {
        if (pendingEdit is not { } before)
        {
            return;
        }

        try
        {
            var after = CaptureSnapshot();
            // Compare the cloned graphs so unchanged values and clicks do not create history entries.
            if (JsonSerializer.Serialize(before.Part, snapshotOptions) != JsonSerializer.Serialize(after.Part, snapshotOptions))
            {
                history.addRedoAction(new SnapshotAction(this, before));
            }
        }
        catch
        {
            RestoreSnapshot(before);
            throw;
        }
        finally
        {
            pendingEdit = null;
            history.Unfreeze();
        }
    }

    private void CancelEdit()
    {
        if (pendingEdit is not { } before)
        {
            return;
        }

        pendingEdit = null;
        RestoreSnapshot(before);
        history.Unfreeze();
    }

    private void ExecuteEdit(Action edit)
    {
        if (pendingEdit is not null)
        {
            return;
        }

        BeginEdit();
        try
        {
            edit();
            CompleteEdit();
        }
        catch
        {
            CancelEdit();
            throw;
        }
    }

    private void RestoreSnapshot(PartSnapshot snapshot)
    {
        // Each restoration gets a fresh graph; further edits must not mutate the stored snapshot.
        EditedPart = snapshot.Part.Clone();
        SelectedPointInEditor = EditedPart.Points.ElementAtOrDefault(snapshot.SelectedPointIndex);
        ActiveStateInEditor = EditedPart.ActiveState;
        ActiveConnectionGroup = ActiveStateInEditor?.ConnectionGroups.ElementAtOrDefault(snapshot.ActiveGroupIndex);
        selectedTag = string.Empty;
        ResetPointDrag();
    }

    public async ValueTask DisposeAsync()
    {
        history.Changed -= HandleHistoryChanged;
        history.Clear();
        if (shortcutReference is not null)
        {
            await JS.InvokeVoidAsync("gwire.disableUndoRedoShortcuts", editorElement);
            shortcutReference.Dispose();
        }
    }
}
