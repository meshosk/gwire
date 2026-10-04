using Gwire.Models;
using Gwire.Models.Base;
using Microsoft.AspNetCore.Components;

namespace Gwire.Pages;

public partial class SchemeEditor : ComponentBase
{
    private readonly Circuit Circuit = new();
    private int selectedPartIndex = -1;

    private void AddCable()
    {
        var cableNumber = Circuit.Parts.OfType<CablePart>().Count() + 1;
        var cable = new CablePart
        {
            Name = $"Cable {cableNumber}"
        };

        cable.Points[0].LocalPosition = new Point(300, 300 + (cableNumber - 1) * 40);
        cable.Points[1].LocalPosition = new Point(500, 300 + (cableNumber - 1) * 40);
        Circuit.Parts.Add(cable);
    }

    private void AddSelectedPart()
    {
        if (selectedPartIndex < 0 || selectedPartIndex >= GwireParts.Parts.Count)
        {
            return;
        }

        if (GwireParts.Parts[selectedPartIndex] is not CircuitPart selectedPart)
        {
            return;
        }

        var part = selectedPart.Clone();
        var partNumber = Circuit.Parts.OfType<CircuitPart>().Count();
        part.SchemePosition = new Point(150 + partNumber * 25, 150 + partNumber * 25);
        Circuit.Parts.Add(part);
    }

}
