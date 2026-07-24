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
    public static string unitDescription = "", unitKeywords = "";

    [SerializeField] List<Squad> squads;
    [SerializeField] List<GameObject> squadButtons;
    [SerializeField] TMP_Text errorText;
    [TextArea(1, 2), SerializeField] string notEnoughUnits, tooManyDuplicates;
    [SerializeField] TMP_InputField nameInput;
    [SerializeField] TMP_Text artifactsText;
    [SerializeField] GameObject unitsMenu;
    [SerializeField] ScrollPanel artifactsPanel;
    [SerializeField] List<LineupCustomize> lineupButtons;
    [SerializeField] UnitOptions unitOptions;
    [SerializeField] UnitDisplay dragAndDropUnit;
    [SerializeField] GameObject squadSelectUI, squadCustomizeUI;
    [SerializeField] DialogueScene dialoguePanel;
    [SerializeField] List<DialogueEvent> events;
    [SerializeField] TMP_Text unitText, keywordText;

    [HideInInspector] public int menuLayer = 0;

    GameObject textBox;

    // Start is called before the first frame update.
    void Start()
    {
        textBox = unitText.transform.parent.gameObject;
    }

    // Update is called once per frame.
    void Update()
    {
        unitText.text = unitDescription;
        keywordText.text = unitKeywords;
        textBox.SetActive(unitDescription != "");

        // Press 'Escape' to leave the current menu.
        if (Input.GetKeyDown(KeyCode.Escape)
            && DialogueScene.isDone)
        {
            if (squadActive != null)
                EndCustomize();
            else
                GotoScene("Town");
        }

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

            if (dragAndDropUnit.unit != selectedUnit
                || dragAndDropUnit.colour != Master.colours[squadActive.colour])
                dragAndDropUnit.ChangeUnit(selectedUnit, squadActive.colour, true);
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
    public static void UpdateArtifactDescriptions(ArtifactButton artifact)
    {
        unitDescription = artifact.GetDescription();
        unitKeywords = artifact.artifact.type.KeywordDescription();
    }

    public void Drag(Unit unit)
    {
        selectedUnit = unit;
        UnitHoverExit();
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
        nameInput.text = squadActive.squadName;

        // Artifacts
        Master.CloseMenu(artifactsPanel.gameObject, unitsMenu);
        List<Artifact> artifacts
            = Master.GetArtifacts(squadActive.colour);
        artifactsText.transform.parent.gameObject.SetActive
            (artifacts.Count > 0);
        if (artifacts.Count > 0)
        {
            artifactsText.text = "View Artifacts";
            List<GameObject> artifactButtons
                = artifactsPanel.Populate(artifacts.Count);
            ArtifactButton newButton;
            for (int a = 0; a < artifacts.Count; a++)
            {
                newButton = artifactButtons[a].GetComponent<ArtifactButton>();
                newButton.type = ArtifactButton.ButtonType.SquadCustomize;
                newButton.manager = gameObject;
                newButton.artifact = artifacts[a];
                newButton.colour = squadActive.colour;
            }
        }

        // Line panels
        for (int u = 0; u < lineupButtons.Count; u++)
            lineupButtons[u].LoadLine
                (u < squadActive.units.Count
                ? squadActive.units[u]
                : null);

        unitOptions.UpdateUnitOptions("basic", squadActive.colour);
        StartCoroutine(dialoguePanel.ExecuteScenes(events));
    }

    public void SquadSelected(int squadIndex)
    {
        squadActive = squads[squadIndex];
        squadSelectUI.SetActive(false);
        StartCustomize();
    }

    public void ToggleArtifacts()
    {
        if (unitsMenu.activeSelf)
        {
            Master.OpenMenu(artifactsPanel.gameObject, unitsMenu);
            artifactsText.text = "View Units";
        }
        else
        {
            Master.CloseMenu(artifactsPanel.gameObject, unitsMenu);
            artifactsText.text = "View Artifacts";
        }
    }

    public int CheckValidity()
    {
        /// <summary>Returns an integer code based on the validity of the current squad setup.
        /// 0: The squad setup is valid.
        /// 1: There are less than three lines in use.
        /// 2: Three or more lines are in use, but there is more than two of the same unit.</summary>

        Dictionary<Unit, int> unitQuantity = new Dictionary<Unit, int>();
        int activeLines = 0;
        bool tooManyDuplicates = false;
        Line line;
        foreach (LineupCustomize l in lineupButtons)
        {
            line = l.GenerateLine();
            if (line != null)
            {
                activeLines++;
                foreach (Unit u in line.units)
                {
                    if (!unitQuantity.ContainsKey(u))
                        unitQuantity[u] = 1;
                    else
                    {
                        unitQuantity[u]++;
                        if (unitQuantity[u] > 2)
                            tooManyDuplicates = true;
                    }
                }
            }
        }

        if (activeLines < 3)
            return 1;
        return tooManyDuplicates ? 2 : 0;
    }

    public void ThrowError(string message)
    {
        errorText.text = message;
        CancelInvoke("ResetError");
        Invoke("ResetError", 5);
    }
    public void ResetError()
    {
        errorText.text = "";
    }

    public void SaveChanges()
    {
        /// <summary>Check the validity of the squad being customized, and if it's valid, save the changes.</summary>

        switch (CheckValidity())
        {
            case 0:
                squadActive.units.Clear();
                if (nameInput.text != "")
                    squadActive.squadName = nameInput.text;
                foreach (LineupCustomize line in lineupButtons)
                    squadActive.units.Add(line.GenerateLine());
                while (squadActive.units.Contains(null))
                    squadActive.units.Remove(null);
                Master.Save();
                EndCustomize();
                break;
            case 1:
                ThrowError(notEnoughUnits);
                break;
            case 2:
                ThrowError(tooManyDuplicates);
                break;
            default:
                break;
        }
    }

    public void EndCustomize()
    {
        ///<summary>End the customization process and return to the select squad menu.</summary>

        squadActive = null;
        ResetError();
        Master.CloseMenu(squadCustomizeUI, squadSelectUI);
        UpdateSquadButtons();
    }

    public void SelectCollection(int index)
    {
        unitOptions.UpdateUnitOptions
            (unitOptions.collections[index],
            squadActive.colour);
    }

    public void UnitHoverEnter(Unit unit)
    {
        UpdateUnitDescriptions(unit);
    }
    public void UnitHoverEnter(ArtifactButton artifact)
    {
        UpdateArtifactDescriptions(artifact);
    }

    public void UnitHoverExit()
    {
        unitDescription = "";
        unitKeywords = "";
    }
}