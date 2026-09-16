using Gwire.Models;
using Gwire.Models.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Gwire.Pages;

public partial class PartEditor : ComponentBase
{
    /// <summary>
    /// Currently edited part
    /// </summary>
    public CustomPart Part { get; } = new();


    private ConnectionPoint? draggedPoint;
    private double dragOffsetX;
    private double dragOffsetY;
    private bool pointWasDragged;


    private ConnectionPoint? SelectedPoint { get; set; }
    private PartState? ActiveState { get; set; }
    private ConnectionGroup? ActiveConnectionGroup { get; set; }


    private void AddPoint()
    {
        var count = Part.Points.Count;
        var point = new ConnectionPoint
        {
            Label = $"Point {count + 1}",
            LocalX = 120 + (count % 5) * 140,
            LocalY = 120 + (count / 5) * 100
        };

        Part.Points.Add(point);
        SelectedPoint = point;
    }

    #region Drags

    private void StartPointDrag(ConnectionPoint point, PointerEventArgs eventArgs)
    {
        SelectedPoint = point;
        draggedPoint = point;
        dragOffsetX = eventArgs.OffsetX - point.LocalX;
        dragOffsetY = eventArgs.OffsetY - point.LocalY;
        pointWasDragged = false;
    }

    private void EndPointDrag(PointerEventArgs eventArgs)
    {
        if (draggedPoint is not null && !pointWasDragged && ActiveConnectionGroup is not null)
        {
            if (!ActiveConnectionGroup.ConnectedPints.Remove(draggedPoint))
            {
                ActiveConnectionGroup.ConnectedPints.Add(draggedPoint);
            }
        }

        CancelPointDrag(eventArgs);
    }

    private void CancelPointDrag(PointerEventArgs eventArgs)
    {
        draggedPoint = null;
        dragOffsetX = 0;
        dragOffsetY = 0;
        pointWasDragged = false;
    }

    private void MoveDraggedPoint(PointerEventArgs eventArgs)
    {
        if (draggedPoint is null)
        {
            return;
        }

        var nextX = Math.Clamp(eventArgs.OffsetX - dragOffsetX, 15, 785);
        var nextY = Math.Clamp(eventArgs.OffsetY - dragOffsetY, 15, 435);
        pointWasDragged |= Math.Abs(nextX - draggedPoint.LocalX) > 2 || Math.Abs(nextY - draggedPoint.LocalY) > 2;
        draggedPoint.LocalX = nextX;
        draggedPoint.LocalY = nextY;
    }

    private void MoveSelectedPoint(double deltaX, double deltaY)
    {
        if (SelectedPoint is null)
        {
            return;
        }

        SelectedPoint.LocalX = Math.Clamp(SelectedPoint.LocalX + deltaX, 15, 785);
        SelectedPoint.LocalY = Math.Clamp(SelectedPoint.LocalY + deltaY, 15, 435);
    }

    #endregion


    private void AddConnectionGroup(PartState state)
    {
        var group = new ConnectionGroup();
        state.ConnectionGroups.Add(group);
    }

    private void AddState()
    {
        var state = new PartState { Label = $"Stav {Part.States.Count + 1}" };
        Part.States.Add(state);
        ActiveState = state;
        ActiveConnectionGroup = null;
    }

    private void RemoveSelectedPoint()
    {
        if (SelectedPoint is null)
        {
            return;
        }

        Part.Points.Remove(SelectedPoint);
        foreach (var state in Part.States)
        {
            foreach (var group in state.ConnectionGroups)
            {
                group.ConnectedPints.Remove(SelectedPoint);
            }
        }

        if (ReferenceEquals(draggedPoint, SelectedPoint))
        {
            draggedPoint = null;
        }

        SelectedPoint = null;
    }

    private void RemoveActiveConnectionGroup(ConnectionGroup group)
    {
        if (ActiveState is null || ActiveConnectionGroup is null)
        {
            return;
        }

        ActiveState.ConnectionGroups.Remove(group);
        ActiveConnectionGroup = null;
    }

    private static T? TryGetByIndex<T>(IReadOnlyList<T> items, object? value) where T : class
    {
        return int.TryParse(value?.ToString(), out var index) && index >= 0 && index < items.Count
            ? items[index]
            : null;
    }

    private string PointClass(ConnectionPoint point) =>
        ReferenceEquals(point, SelectedPoint) ? "connection-point is-selected" : "connection-point";

    private void AddRemovePointToGroup(ConnectionGroup group, ConnectionPoint selectedPoint)
    {
        if (group.ConnectedPints.Contains(selectedPoint))
        {
            group.ConnectedPints.Remove(selectedPoint);
        }
        else
        {
            group.ConnectedPints.Add(selectedPoint);
        }
        StateHasChanged();
    }
}
