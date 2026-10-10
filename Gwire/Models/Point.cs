using Microsoft.AspNetCore.Components.Web;

namespace Gwire.Models;

/// <summary>
/// A position or offset represented by single-precision coordinates.
/// </summary>
public struct Point
{
    public float X { get; set; }
    public float Y { get; set; }

    public Point()
    {
        X = 0;
        Y = 0;
    }

    public Point(float x, float y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Creates a point from the pointer's client coordinates.
    /// </summary>
    public Point(PointerEventArgs eventArgs) : this((float)eventArgs.ClientX, (float)eventArgs.ClientY)
    {
    }

    public readonly Point Copy() => new(X, Y);
}
