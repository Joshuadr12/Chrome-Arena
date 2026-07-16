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
    [HideInInspector] public List<Artifact> artifactOptions = new List<Artifact>();

    public Unit GetUnit(int index)
    {
        return unitOptions[index];
    }
    public Artifact GetArtifact(int index)
    {
        return artifactOptions[index];
    }

    public void UpdateUnitOptions
        (string collection = "all",
        string colour = "neutral",
        bool canBeBig = true)
    {
        /// <summary>Update the unit collection tabs and unit option buttons for the given collection and colour,
        /// or artifacts if the artifact selection menu is to be opened.</summary>

        // Artifact Select
        if (SquadCustomize.chooseArtifact)
        {
            // Disable collection labels.
            int buttonIndex = 0;
            foreach (TMP_Text label in collectionLabels)
            {
                label.text = "";
                label
                    .transform.parent
                        .GetComponent<Button>().interactable = false;
            }

            // Artifact options.
            artifactOptions = Master.GetArtifacts(colour);
            buttonIndex = 0;
            foreach (Artifact a in artifactOptions)
            {
                if (buttonIndex < unitOptionButtons.Count)
                {
                    unitOptionButtons[buttonIndex]
                        .SetArtifact(artifactOptions[buttonIndex], colour);
                    buttonIndex++;
                }
            }
            while (buttonIndex < unitOptionButtons.Count)
            {
                unitOptionButtons[buttonIndex].Disable();
                buttonIndex++;
            }
        }

        // Unit Customization
        else
        {
            // Unit collection labels.
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
}