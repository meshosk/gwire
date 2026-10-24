using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Gwire.Services;

/// <summary>
/// Exports the rendered scheme to a tightly cropped, single-page PDF.
/// </summary>
public sealed class SchemePdfExporter(IJSRuntime js)
{
    public async Task ExportAsync(ElementReference editor, string name)
    {
        var safeName = string.Concat(name.Select(character =>
            Path.GetInvalidFileNameChars().Contains(character) || "<>:\"/\\|?*".Contains(character)
                ? '_' : character)).Trim().TrimEnd('.');
        var fileName = string.IsNullOrWhiteSpace(safeName) ? "scheme.pdf" : $"{safeName}.pdf";
        await js.InvokeVoidAsync("gwire.exportSchemePdf", editor, fileName);
    }
}
