using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Services;
using Gwire.UndoRedo;
using Gwire.UndoRedo.Actions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text;

namespace Gwire.Components;

public partial class CircuitPartRenderer : ComponentBase, IDisposable
{
    private ElementReference element;
    private readonly string elementReferenceId = $"part-{Guid.NewGuid():N}";
    private bool wasDragged;
    private long? dragPointerId;
    private Point dragStartClient;
    protected Point dragStartPart;

    [Parameter]
    public float Zoom { get; set; } = 1;

#region Injects & prams
        [Inject]
        private IJSRuntime JS { get; set; } = default!;
    
        [Inject]
        private UndoRedoService UndoRedoService { get; set; } = default!;

        private UndoRedoService.UndoRedoHistoryWrapper History => UndoRedoService.getHistory(HistoryIds.SchemeEditor);
    
        [Parameter]
        public EventCallback OnDelete { get; set; }

        [Parameter]
        public bool IsSelected { get; set; }
    
        [Parameter]
        public EventCallback<bool> IsSelectedChanged { get; set; }
    
        [Parameter, EditorRequired]
        public CircuitPart CircuitPart { get; set; } = default!;
#endregion

    [Parameter]
    public EventCallback<CircuitPart> OnEditText { get; set; }

    protected virtual Point PartPosition => CircuitPart.SchemePosition;

    private string SvgTransform => FormattableString.Invariant($"rotate({CircuitPart.SvgRotation} {CircuitPart.Width / 2d} {CircuitPart.Height / 2d})");

    private string PartTransform => FormattableString.Invariant($"translate({PartPosition.X} {PartPosition.Y})");

    private string? SvgImageSource => string.IsNullOrWhiteSpace(CircuitPart.SvgMarkup)
        ? null
        : $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(CircuitPart.SvgMarkup))}";

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var previousCircuitPart = CircuitPart;
        await base.SetParametersAsync(parameters);

        if (!ReferenceEquals(previousCircuitPart, CircuitPart))
        {
            if (previousCircuitPart is not null)
            {
                previousCircuitPart.Changed -= HandleChanged;
            }
            CircuitPart.Changed += HandleChanged;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("gwire.enablePointerCapture", element);
        }
    }

    private Task SelectContextTarget() => History.IsFrozen || IsSelected
        ? Task.CompletedTask
        : IsSelectedChanged.InvokeAsync(true);

    private Task Select() => wasDragged || IsSelected ? Task.CompletedTask : IsSelectedChanged.InvokeAsync(true);

    private void HandleChanged() => StateHasChanged();

    private void SetActiveState(PartState state)
    {
        if (!History.IsFrozen && !ReferenceEquals(CircuitPart.ActiveState, state))
        {
            var action = new ChangePartStateAction(CircuitPart, state);
            action.Redo();
            History.addRedoAction(action);
        }
    }


#region Dragging
        private void StartDrag(PointerEventArgs eventArgs)
    {
        if (eventArgs.Button != 0 || !eventArgs.IsPrimary || dragPointerId is not null)
        {
            return;
        }

        dragPointerId = eventArgs.PointerId;
        wasDragged = false;
        dragStartClient = new Point(eventArgs);
        dragStartPart = PartPosition.Copy();
        History.Freeze();
        OnDragStarted();
    }
        private void MoveDraggedPart(PointerEventArgs eventArgs)
        {
            if (dragPointerId != eventArgs.PointerId)
            {
                return;
            }
    
            var pointer = new Point(eventArgs);
            wasDragged |= MathF.Abs(pointer.X - dragStartClient.X) > 2 ||
                MathF.Abs(pointer.Y - dragStartClient.Y) > 2;
            CircuitPart.MoveTo(new Point(dragStartPart.X + (pointer.X - dragStartClient.X) / Zoom,
                dragStartPart.Y + (pointer.Y - dragStartClient.Y) / Zoom));
            OnDragMoved();
        }
    private void EndDrag(PointerEventArgs eventArgs)
    {
        if (dragPointerId != eventArgs.PointerId)
        {
            return;
        }

        MoveDraggedPart(eventArgs);
        OnDragEnding();
        if (HasDragChanges)
        {
            History.addRedoAction(CreateMoveAction());
        }
        ResetDrag();
    }

    private void CancelDrag(PointerEventArgs eventArgs)
    {
        if (dragPointerId == eventArgs.PointerId)
        {
            CreateMoveAction().Undo();
            ResetDrag();
        }
    }
    protected virtual void OnDragStarted() { }
    protected virtual void OnDragMoved() { }
    protected virtual void OnDragEnding() { }
    protected virtual void OnDragReset() { }

    protected virtual bool HasDragChanges => !dragStartPart.Equals(PartPosition);

    protected virtual IUndoableAction CreateMoveAction() =>
        new MovePartAction(CircuitPart, dragStartPart, CircuitPart.SchemePosition.Copy());

    private void ResetDrag()
    {
        OnDragReset();
        dragPointerId = null;
        History.Unfreeze();
    }
#endregion

    public virtual void Dispose()
    {
        CircuitPart.Changed -= HandleChanged;
        if (dragPointerId is { } pointerId)
        {
            CancelDrag(new PointerEventArgs { PointerId = pointerId });
        }
    }
}
