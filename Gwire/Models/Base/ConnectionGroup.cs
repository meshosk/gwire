using Microsoft.AspNetCore.Components;

namespace Gwire.Models.Base;

/// <summary>
/// Represents a group of some or all interconnected points within a circuit part.
/// </summary>
public class ConnectionGroup
{
    private static readonly string[] LineColors =
    [
        "#e63946",
        "#f4a261",
        "#e9c46a",
        "#2a9d8f",
        "#277da1",
        "#5e60ce",
        "#9d4edd",
        "#d65db1",
        "#43aa8b",
        "#6c757d"
    ];

    /// <summary>
    /// Interconnected points. Not all points of a part can be in the point group.
    /// </summary>
    public List<ConnectionPoint> ConnectedPints { get; } = [];

    public static string GetLineColor(int groupIndex)
    {
        var colorIndex = groupIndex % LineColors.Length;
        return LineColors[colorIndex < 0 ? colorIndex + LineColors.Length : colorIndex];
    }
}
