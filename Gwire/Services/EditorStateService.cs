using Gwire.Models;

namespace Gwire.Services;

/// <summary>
/// Keeps both editor states alive while navigating within the application.
/// </summary>
public sealed class EditorStateService
{
    public SchemeEditorState SchemeEditor { get; } = new();
    public PartEditorState PartEditor { get; } = new();
}
