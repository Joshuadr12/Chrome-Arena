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

    [HideInInspector] public List<UnitColour> units = new List<UnitColour>();
    UnitDisplay display;

    void RenderUnits()
    {
        /// <summary>Re-render the units on the buttons.</summary>

        for (int u = 0; u < unitImages.Count; u++)
        {
            display = unitImages[u];
            display.gameObject.SetActive(u < units.Count);
            display.ChangeUnit
                (u < units.Count ? units[u].unit : null,
                u < units.Count ? units[u].colour : "neutral");
        }
    }

    public void SetColourAll()
    {
        foreach (UnitColour u in units)
            u.colour = SquadCustomize.squadActive.colour;
    }

    public void LoadLine(Line line)
    {
        /// <summary>Load the given line for display.</summary>
        /// <param name="line">The line to load.</param>

        units.Clear();
        if (line != null)
            foreach (Unit unit in line.units)
                units.Add(new UnitColour(unit));
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
        foreach (UnitColour u in units)
            result.units.Add(u.unit);
        return result;
    }

    public void UnitHoverEnter(int index)
    {
        if (index < units.Count)
        {
            if (!SquadCustomize.chooseArtifact)
            SquadCustomize.UpdateUnitDescriptions(units[index].unit);

            if (FindAnyObjectByType<ResearchManager>())
                FindAnyObjectByType<ResearchManager>()
                    .UnitHoverEnter(units[index].unit);
        }
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
            {
                units[index].unit = SquadCustomize.selectedUnit;
                if (SquadCustomize.squadActive != null)
                    units[index].colour = SquadCustomize.squadActive.colour;
                else if (Master.colourActive != "")
                    units[index].colour = Master.colourActive;
            }
            else
                units.Add(new UnitColour(SquadCustomize.selectedUnit));

            RenderUnits();
            UnitHoverEnter(index);
        }
    }
    public void Drop(Unit unit)
    {
        units.Clear();
        units.Add(new UnitColour(unit));
        RenderUnits();
    }
}