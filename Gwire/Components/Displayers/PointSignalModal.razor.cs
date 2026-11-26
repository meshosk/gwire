using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Models.Pairs;
using Gwire.Models.Signal;
using Microsoft.AspNetCore.Components;

namespace Gwire.Components.Displayers;

public partial class PointSignalModal
{
    [Parameter, EditorRequired]
    public ConnectionPoint Point { get; set; } = default!;

    [Parameter, EditorRequired]
    public IReadOnlyDictionary<ConnectionPoint, IReadOnlyDictionary<CoilPair, SignalSpectrum>> PointSignals { get; set; } = default!;

    [Parameter, EditorRequired]
    public string Transform { get; set; } = string.Empty;

    [Parameter]
    public EventCallback OnClose { get; set; }
}
