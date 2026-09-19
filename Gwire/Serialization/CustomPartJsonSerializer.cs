using Gwire.Models;

namespace Gwire.Serialization;

/// <summary>
/// Serializes custom parts while preserving references from connection groups to points.
/// </summary>
public sealed class CustomPartJsonSerializer : ObjectJsonSerializer<CustomPart>
{
}
