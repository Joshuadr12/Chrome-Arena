using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitOptions : MonoBehaviour
{
    public List<string> collections;
    public List<TMP_Text> collectionLabels;
    public List<CharacterButton> unitOptionButtons;

    [HideInInspector] public List<Unit> unitOptions = new List<Unit>();

    public Unit GetUnit(int index)
    {
        return unitOptions[index];
    }

    public void UpdateUnitOptions
        (string collection = "all",
        string colour = "neutral",
        bool canBeBig = true)
    {
        /// <summary>Update the unit collection tabs and unit option buttons for the given collection and colour.</summary>

        int buttonIndex = 0;
        foreach (string co in collections)
            if (buttonIndex < collectionLabels.Count)
            {
                collectionLabels[buttonIndex].text = collections[buttonIndex]
                    .ToUpper();
                collectionLabels[buttonIndex]
                    .transform.parent
                    .GetComponent<Button>().interactable = collections[buttonIndex] != collection;
                buttonIndex++;
            }
        while (buttonIndex < collectionLabels.Count)
        {
            collectionLabels[buttonIndex]
                .transform.parent
                    .GetComponent<Button>().interactable = false;
            buttonIndex++;
        }

        // Unit options.
        unitOptions = Master.GetUnits(collection, colour);
        buttonIndex = 0;
        foreach (Unit u in unitOptions)
        {
            if (buttonIndex < unitOptionButtons.Count
                && (u.bodySize <= 1 || canBeBig))
            {
                unitOptionButtons[buttonIndex].SetUnit(u, colour);
                buttonIndex++;
            }
        }
        while (buttonIndex < unitOptionButtons.Count)
        {
            unitOptionButtons[buttonIndex].Disable();
            buttonIndex++;
        }
    }
}