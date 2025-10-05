using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitOptions : MonoBehaviour
{
    public List<string> collections;
    public List<TMP_Text> collectionLabels;
    public List<UnitDisplay> unitOptionButtons;
    public List<Image> artifactOptionButtons;

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
            artifactOptions.Clear();
            buttonIndex = 0;
            foreach (Artifact artifact in Master.GetArtifacts(colour))
            {
                if (buttonIndex < artifactOptionButtons.Count)
                {
                    artifactOptions.Add(artifact);
                    artifactOptionButtons[buttonIndex]
                        .transform.parent
                        .gameObject.SetActive(true);
                    artifactOptionButtons[buttonIndex]
                        .gameObject.SetActive(true);
                    artifactOptionButtons[buttonIndex].sprite = artifact.sprite;
                    unitOptionButtons[buttonIndex]
                        .gameObject.SetActive(false);
                    buttonIndex++;
                }
            }
            while (buttonIndex < unitOptionButtons.Count)
            {
                artifactOptionButtons[buttonIndex]
                    .transform.parent
                    .gameObject.SetActive(false);
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
            unitOptions.Clear();
            buttonIndex = 0;
            foreach (Unit u in Master.GetUnits(collection, colour))
            {
                if (buttonIndex < unitOptionButtons.Count
                    && (u.bodySize <= 1 || canBeBig))
                {
                    unitOptions.Add(u);
                    unitOptionButtons[buttonIndex]
                        .transform.parent
                        .gameObject.SetActive(true);
                    unitOptionButtons[buttonIndex]
                        .gameObject.SetActive(true);
                    unitOptionButtons[buttonIndex].ChangeUnit(u, colour);
                    artifactOptionButtons[buttonIndex]
                        .gameObject.SetActive(false);
                    buttonIndex++;
                }
            }
            while (buttonIndex < unitOptionButtons.Count)
            {
                unitOptionButtons[buttonIndex]
                    .transform.parent
                    .gameObject.SetActive(false);
                buttonIndex++;
            }
        }
    }
}