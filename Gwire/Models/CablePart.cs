using Gwire.Models.Base;

namespace Gwire.Models;

/// <summary>
/// A cable joins exactly two independently movable connection points.
/// </summary>
public sealed class CablePart : CircuitPart
{
    public CablePart()
    {
        // cable has only one permanently connected state
        Points.Add(new ConnectionPoint());
        Points.Add(new ConnectionPoint());

        this.States.Add(new PartState() {
                ConnectionGroups =
                [
                    new ConnectionGroup()
                    {
                        ConnectedPints = [Points[0], Points[1]]
                    }
                ]
            }
        );
        this.ActiveState = this.States[0];
    }
}
