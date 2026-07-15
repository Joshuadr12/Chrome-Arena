using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Chrome Arena/Squad")]
public class Squad : ScriptableObject
{
    /// <summary>
    /// Represents a squad of units to send into battle.
    /// </summary>

    // Serialized variables for the editor.
    [FormerlySerializedAs("name")] public string squadName;
    public string colour;
    public bool isBoss;
    public List<Line> units;
    [FormerlySerializedAs("weapon")] public Artifact artifact;

    // Miscellaneous variables.
    [HideInInspector] public int startMoney, money = 0;

    public bool CanSummon(int maxCount = 5)
    {
        /// <summary>Returns whether or not the squad has a line of units that can be afforded and summoned.</summary>
        /// <param name="maxCount">The maximum number of fighters that can be summoned.</param>

        foreach (Line u in units)
            if
                ((u.TotalPrice() <= money)
                && (u.units.Count <= maxCount))
                return true;
        return false;
    }

    public Line RandomUnits(int maxCount = 5)
    {
        /// <summary>Returns a random line of units based on probability weight, space, and affordability.</summary>
        /// <param name="maxCount">The maximum number of fighters that can be summoned.</param>

        float weightPool = 0;
        List<Line> available = new List<Line>();

        // Check for available lines.
        foreach (Line u in units)
        {
            if
                ((u.TotalPrice() <= money)
                && (u.units.Count <= maxCount))
            {
                weightPool += u.TotalWeight();
                available.Add(u);
            }
        }

        // Choose a line.
        foreach (Line u in available)
        {
            if
                (UnityEngine.Random.value
                <= u.TotalWeight() / weightPool)
                return u;
            weightPool -= u.TotalWeight();
        }
        return null;
    }
}