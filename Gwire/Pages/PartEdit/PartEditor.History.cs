using Gwire.Services;
using Gwire.UndoRedo;
using Gwire.UndoRedo.Actions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gwire.Pages.PartEdit;

public partial class PartEditor
{
    private static readonly JsonSerializerOptions snapshotOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve
    };

    [Inject]
    private UndoRedoService UndoRedoService { get; set; } = default!;

    private UndoRedoService.UndoRedoHistoryWrapper history => UndoRedoService.getHistory(HistoryIds.PartEditor);

    /// <summary>
    /// The snapshot captured before the current edit, used to create an undo action or cancel the edit.
    /// </summary>
    private PartSnapshot? pendingEdit;

    /// <summary>
    /// The root editor DOM element passed to JavaScript interop.
    /// </summary>
    private ElementReference editorElement;

    /// <summary>
    /// Allows JavaScript keyboard shortcuts to invoke the editor's .NET handlers.
    /// </summary>
    private DotNetObjectReference<PartEditor>? shortcutReference;

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

    private void BeginEdit()
    {
        if (pendingEdit is not null)
        {
            return;
        }

        pendingEdit = PartSnapshot.CaptureSnapshot(EditorStateService.PartEditor);
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
            var after = PartSnapshot.CaptureSnapshot(EditorStateService.PartEditor);
            // Compare the cloned graphs so unchanged values and clicks do not create history entries.
            if (JsonSerializer.Serialize(before.Part, snapshotOptions) != JsonSerializer.Serialize(after.Part, snapshotOptions))
            {
                history.addRedoAction(new PartSnapshotAction(EditorStateService.PartEditor, before));
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
        EditorStateService.PartEditor.RestoreSnapshot(snapshot);
        ResetPointDrag();
    }

    public async ValueTask DisposeAsync()
    {
        history.Changed -= HandleHistoryChanged;
        GwireParts.Changed -= HandleRepositoryChanged;
        CancelEdit();
        if (shortcutReference is not null)
        {
            await JS.InvokeVoidAsync("gwire.disableUndoRedoShortcuts", editorElement);
            shortcutReference.Dispose();
        }
    }
}
