using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LineupCustomize : MonoBehaviour
{
    /// <summary>
    /// Manages a single line of units to be customized.
    /// </summary>

    public List<UnitDisplay> unitImages;

    [HideInInspector] public string colour;
    List<Unit> units = new List<Unit>();
    UnitDisplay display;

    void RenderUnits()
    {
        /// <summary>Re-render the units on the buttons.</summary>

        for (int u = 0; u < unitImages.Count; u++)
        {
            display = unitImages[u];
            display.gameObject.SetActive(u < units.Count);
            display.ChangeUnit
                (u < units.Count ? units[u] : null,
                colour);
        }
    }

    public void LoadLine(Line line)
    {
        /// <summary>Load the given line for display.</summary>
        /// <param name="line">The line to load.</param>

        units.Clear();
        if (line != null)
            foreach (Unit unit in line.units)
                units.Add(unit);
        RenderUnits();
    }
    public void ClearLine()
    {
        /// <summary>Clear the line of units by loading a null line.</summary>

        LoadLine(null);
    }

    public Line GenerateLine()
    {
        /// <summary>Generates and returns a Line instance representing the customized line that this was managing.</summary>

        if (units.Count == 0)
            return null;
        Line result = new Line();
        result.units = new List<Unit>();
        foreach (Unit u in units)
            result.units.Add(u);
        return result;
    }

    public void UnitHoverEnter(int index)
    {
        if ((index < units.Count)
            && !SquadCustomize.chooseArtifact)
            SquadCustomize.UpdateUnitDescriptions(units[index]);
    }

    public void UnitHoverExit()
    {
        SquadCustomize.unitDescription = "";
        SquadCustomize.unitKeywords = "";
    }

    public void Drop(int index)
    {
        if (!SquadCustomize.chooseArtifact)
        {
            if (index < units.Count)
                units[index] = SquadCustomize.selectedUnit;
            else
                units.Add(SquadCustomize.selectedUnit);
            RenderUnits();
            UnitHoverEnter(index);
        }
    }
}