using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SquadCustomize : MonoBehaviour
{
    /// <summary>
    /// Manages the squad customization menu.
    /// </summary>

    public static Squad squadActive = null;
    public static Unit selectedUnit = null;
    public static bool chooseArtifact = false;
    public static string unitDescription = "", unitKeywords = "";

    [SerializeField] List<Squad> squads;
    [SerializeField] List<GameObject> squadButtons;
    [SerializeField] List<LineupCustomize> lineupButtons;
    [SerializeField] UnitOptions unitOptions;
    [SerializeField] Image artifactImage;
    [SerializeField] UnitDisplay dragAndDropUnit;
    [SerializeField] TMP_InputField nameInput;
    [SerializeField] GameObject squadSelectUI, squadCustomizeUI, dialoguePanel;
    [SerializeField] TMP_Text unitText, keywordText;

    GameObject textBox;
    Artifact artifactActive;

    // Start is called before the first frame update.
    void Start()
    {
        textBox = unitText.transform.parent.gameObject;
        if (!Master.FinishedTutorial("basic_2"))
            artifactImage
                .transform.parent
                .gameObject.SetActive(false);
    }

    // Update is called once per frame.
    void Update()
    {
        unitText.text = unitDescription;
        keywordText.text = unitKeywords;
        textBox.SetActive(unitDescription != "");

        if (Input.GetMouseButtonUp(0))
            selectedUnit = null;
        dragAndDropUnit.gameObject.SetActive(selectedUnit);
        if (selectedUnit)
        {
            Vector3 cursorPos = Camera.main
                .ScreenToWorldPoint(Input.mousePosition);

            dragAndDropUnit.transform.position = new Vector3
                (cursorPos.x,
                cursorPos.y,
                0);

            if (dragAndDropUnit != selectedUnit)
                dragAndDropUnit.ChangeUnit(selectedUnit, squadActive.colour);
        }

        UpdateSquadButtons();
    }

    public static void UpdateUnitDescriptions(Unit unit)
    {
        unitDescription = unit.GetDescription();

        // Keyword description.
        unitKeywords = "";
        foreach (Keyword keyword in Master.keywords)
            if (unit.keywords.Contains(keyword.name))
                unitKeywords += keyword.description + "\n";
        foreach (Unit u in unit.keywordUnits)
            unitKeywords += u.KeywordDescription() + "\n";
    }
    public static void UpdateArtifactDescriptions(Artifact artifact)
    {
        unitDescription = artifact.GetDescription();

        // Keyword description.
        unitKeywords = "";
        foreach (Keyword keyword in Master.keywords)
            if (artifact.keywords.Contains(keyword.name))
                unitKeywords += keyword.description + "\n";
        foreach (Unit u in artifact.keywordUnits)
            unitKeywords += u.KeywordDescription() + "\n";
    }

    public void Drag(int index)
    {
        if (!chooseArtifact)
        {
            selectedUnit = unitOptions.GetUnit(index);
            UnitHoverExit();
        }
    }

    public void GotoScene(string sceneName)
    {
        /// <summary>Load the given scene.</summary>
        /// <param name="sceneName">The scene to load.</param>

        Master.GotoScene(sceneName);
    }

    void UpdateSquadButtons()
    {
        /// <summary>Set the button colors and names based on the corresponding squads.</summary>

        GameObject button;
        Transform buttonTransform;

        for (int b = 0; b < squadButtons.Count; b++)
        {
            button = squadButtons[b];
            button.GetComponent<Image>().color = Master
                .colours[squads[b].colour]
                .physicalColour;
            buttonTransform = button.transform.GetChild(0);
            buttonTransform.GetComponent<TMP_Text>().text = squads[b].squadName;
        }
    }

    public void StartCustomize()
    {
        ///<summary>Load the screen for squad customization.</summary>
        ///<param name="squadIndex">The index of the squad to customize.</param>

        squadCustomizeUI.SetActive(true);
        chooseArtifact = false;
        artifactActive = squadActive.artifact;
        artifactImage.sprite = artifactActive.sprite;
        nameInput.text = squadActive.squadName;

        // Line panels
        for (int u = 0; u < lineupButtons.Count; u++)
        {
            lineupButtons[u].colour = squadActive.colour;
            lineupButtons[u].LoadLine
                (u < squadActive.units.Count
                ? squadActive.units[u]
                : null);
        }

        unitOptions.UpdateUnitOptions("basic", squadActive.colour);
        dialoguePanel.SetActive(true);
    }

    public void SquadSelected(int squadIndex)
    {
        squadActive = squads[squadIndex];
        squadSelectUI.SetActive(false);
        StartCustomize();
    }

    public void ArtifactSelectMenu()
    {
        if (!chooseArtifact)
        {
            chooseArtifact = true;
            unitOptions.UpdateUnitOptions(colour: squadActive.colour);
        }
    }

    public void SelectArtifact(int index)
    {
        if (chooseArtifact)
        {
            artifactActive = unitOptions.GetArtifact(index);
            artifactImage.sprite = artifactActive.sprite;
            chooseArtifact = false;
            unitOptions.UpdateUnitOptions("basic", squadActive.colour);
        }
    }

    public void SaveChanges()
    {
        /// <summary>Save the changes to the squad being customized.</summary>

        squadActive.artifact = artifactActive;
        squadActive.units.Clear();
        if (nameInput.text != "")
            squadActive.squadName = nameInput.text;
        foreach (LineupCustomize line in lineupButtons)
            squadActive.units.Add(line.GenerateLine());
        while (squadActive.units.Contains(null))
            squadActive.units.Remove(null);
        Master.Save();
    }

    public void EndCustomize()
    {
        ///<summary>End the customization process and return to the select squad menu.</summary>

        squadActive = null;
        squadCustomizeUI.SetActive(false);
        squadSelectUI.SetActive(true);
        UpdateSquadButtons();
    }

    public void SelectCollection(int index)
    {
        unitOptions.UpdateUnitOptions
            (unitOptions.collections[index],
            squadActive.colour);
    }

    public void UnitHoverEnter(int index)
    {
        if (chooseArtifact)
            UpdateArtifactDescriptions(unitOptions.GetArtifact(index));
        else
            UpdateUnitDescriptions(unitOptions.GetUnit(index));
    }

    public void UnitHoverExit()
    {
        unitDescription = "";
        unitKeywords = "";
    }
}