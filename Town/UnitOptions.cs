using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitOptions : MonoBehaviour
{
    public List<string> collections;
    public List<TMP_Text> collectionLabels;
    public ScrollPanel unitOptionPanel;
    public CharacterButton.ButtonType buttonType;
    public GameObject manager;
    public List<CharacterButton> unitOptionButtons = new List<CharacterButton>();

    [HideInInspector] public List<Unit> unitOptions = new List<Unit>();
    List<GameObject> unitObjects;

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

        // Collections.
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
        unitOptions.Clear();
        unitOptionButtons.Clear();
        foreach (Unit u in Master.GetUnits(collection, colour))
            if (canBeBig || u.bodySize <= 1)
                unitOptions.Add(u);

        unitObjects = unitOptionPanel.Populate(unitOptions.Count);
        CharacterButton button;
        for (int u = 0; u < unitOptions.Count; u++)
        {
            button = unitObjects[u].GetComponent<CharacterButton>();
            button.buttonType = buttonType;
            button.manager = manager;
            button.SetUnit(unitOptions[u], colour, canBeBig);
            unitOptionButtons.Add(button);
        }
    }

    public void DisableUnit(Unit unit)
    {
        foreach (CharacterButton button in unitOptionButtons)
            button.GetComponent<Button>().interactable
                = button.unit.unit != unit;
    }
}