using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class ResearchManager : MonoBehaviour
{
    [Header("Main Panel"), SerializeField] GameObject mainPanel;
    [SerializeField] LineupCustomize lineupPanel;
    [SerializeField] UnitOptions unitOptions;
    [SerializeField] UnitDisplay dragAndDropUnit;
    [SerializeField] TMP_Text researchPointText, researchCostText,
        unitText, keywordText;
    [SerializeField] Slider researchSlider;
    [SerializeField] Button researchButton;
    [Header("Results Panel"), SerializeField] GameObject resultsPanel;
    [SerializeField] GameObject oneResultPanel, allResultsPanel;
    [SerializeField] UnitDisplay resultUnit;
    [SerializeField] ParticleSystem particles;
    [SerializeField] AudioClip newSound, notNewSound;
    [SerializeField] TMP_Text resultHeader;
    [SerializeField] ScrollPanel allResultsScroll;

    GameObject textBox;
    bool showingAllResults = false;
    int unitDisplayed;
    List<UnitColour> results = new List<UnitColour>(),
        newResults = new List<UnitColour>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        textBox = unitText.transform.parent.gameObject;
        GetComponent<AudioSource>().volume = Master.data.sfxVolume;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && Town.menuLayer > 0)
        {
            if (Town.menuLayer == 1)
                CloseMenu();
            else if (FindFirstObjectByType<UpgradeManager>()
                && FindFirstObjectByType<UpgradeManager>()
                .gameObject.activeSelf)
                FindFirstObjectByType<UpgradeManager>()
                    .CloseMenu();
            else if (showingAllResults)
                LeaveResults();
            else
                SkipResults();
        }

        // Drag and drop unit
        if (Input.GetMouseButtonUp(0))
            SquadCustomize.selectedUnit = null;
        dragAndDropUnit.gameObject.SetActive(SquadCustomize.selectedUnit);
        if (SquadCustomize.selectedUnit)
        {
            Vector3 cursorPos = Camera.main
                .ScreenToWorldPoint(Input.mousePosition);

            dragAndDropUnit.transform.position = new Vector3
                (cursorPos.x,
                cursorPos.y,
                0);

            if (dragAndDropUnit.unit != SquadCustomize.selectedUnit
                || dragAndDropUnit.colour != Master.colours[Master.colourActive])
                dragAndDropUnit.ChangeUnit(SquadCustomize.selectedUnit, Master.colourActive);
        }

        // Update whether or not research can be done.
        researchButton.interactable = Master.data.researchPoints > 0
            && lineupPanel.units.Count > 0;
    }

    public void OpenMenu()
    {
        // Update the unit options.
        gameObject.SetActive(true);
        Town.menuLayer = 1;
        showingAllResults = false;
        Master.colourActive = "red";
        lineupPanel.LoadLine(null);

        RefreshMenu();
    }

    public void RefreshMenu()
    {
        // Display the number of research points the player has.
        researchPointText.text = $"{Master.data.researchPoints} research point";
        if (Master.data.researchPoints != 1)
            researchPointText.text += "s";

        // Adjust the slider and research button.
        researchSlider.value = 1;
        researchSlider.maxValue = Mathf.Max(Master.data.researchPoints, 1);
        researchCostText.text = "Research 1 time";

        unitOptions.UpdateUnitOptions("basic", Master.colourActive);
    }

    public void SelectColour(string colour)
    {
        Master.colourActive = colour;
        unitOptions.UpdateUnitOptions("basic", colour);
    }

    public void SelectUnit(Unit unit)
    {
        lineupPanel.Drop(unit);
    }

    public void Drag(Unit unit)
    {
        SquadCustomize.selectedUnit = unit;
        Vector3 cursorPos = Camera.main
                .ScreenToWorldPoint(Input.mousePosition);

        UnitHoverExit();
    }

    public void UnitHoverEnter(int index)
    {
        SquadCustomize.UpdateUnitDescriptions(unitOptions.GetUnit(index));
        unitText.text = SquadCustomize.unitDescription;
        keywordText.text = SquadCustomize.unitKeywords;
        textBox.SetActive(true);
    }
    public void UnitHoverEnter(Unit unit)
    {
        SquadCustomize.UpdateUnitDescriptions(unit);
        unitText.text = SquadCustomize.unitDescription;
        keywordText.text = SquadCustomize.unitKeywords;
        textBox.SetActive(true);
    }
    public void UnitHoverExit()
    {
        SquadCustomize.unitDescription = "";
        SquadCustomize.unitKeywords = "";
        textBox.SetActive(false);
    }

    public void ResearchCostChanged()
    {
        researchCostText.text = $"Research {researchSlider.value} time";
        if (researchSlider.value != 1)
            researchCostText.text += "s";
    }

    public void DoResearch()
    {
        // Execute and save the process back-end.
        Master.data.researchPoints -= (int)researchSlider.value;
        results.Clear();
        for (int i = 0; i < researchSlider.value; i++)
            results.Add(GetResearchUnit(lineupPanel.units[0]));

        newResults.Clear();
        foreach (UnitColour result in results)
            if (Master.data.NewUnitColour(result))
                newResults.Add(result);

        Master.Save();
        RefreshMenu();

        // Start displaying the results.
        Town.menuLayer = 2;
        Master.OpenMenu(resultsPanel, mainPanel);
        Master.CloseMenu(allResultsPanel, oneResultPanel);
        unitDisplayed = -1;
        NextResult();
    }

    public void SkipResults()
    {
        unitDisplayed = results.Count;
        NextResult();
    }

    public void NextResult()
    {
        unitDisplayed++;
        if (unitDisplayed >= results.Count)
        {
            UnitHoverExit();
            if (newResults.Count > 0)
            {
                Master.OpenMenu(allResultsPanel, oneResultPanel);
                showingAllResults = true;
                List<GameObject> newButtons = allResultsScroll.Populate(newResults.Count);
                CharacterButton button;
                for (int i = 0; i < newResults.Count; i++)
                {
                    button = newButtons[i].GetComponent<CharacterButton>();
                    button.SetUnit(newResults[i]);
                    button.manager = gameObject;
                }
            }
            else
                Master.CloseMenu(resultsPanel, mainPanel);
        }
        else
        {
            UnitColour temp = results[unitDisplayed];
            resultUnit.ChangeUnit(temp);
            resultHeader.text = temp.GetString();
            UnitHoverEnter(temp.unit);

            if (newResults.Contains(temp))
            {
                GetComponent<AudioSource>().PlayOneShot(newSound);
                particles.Play();
                resultHeader.text += "\nNEW!";
            }
            else
                GetComponent<AudioSource>().PlayOneShot(notNewSound);
        }
    }

    public void LeaveResults()
    {
        Master.CloseMenu(resultsPanel, mainPanel);
        showingAllResults = false;
        Town.menuLayer = 1;
    }

    public void CloseMenu()
    {
        SquadCustomize.selectedUnit = null;
        Master.colourActive = "";
        researchButton.interactable = false;
        gameObject.SetActive(false);
        Town.menuLayer = 0;
    }

    public static UnitColour GetResearchUnit(UnitColour unit)
    {
        /// <summary>
        /// Conducts research with the given unit and returns the result.
        /// </summary>

        // Generate the list of possible colours.
        List<string> colours = Master.TranslateCollections(unit.unit.colours, false);
        colours.Remove(unit.colour);
        // TODO: Allow secondary colours when support for them is unlocked.
        colours.Remove("orange");
        colours.Remove("green");
        colours.Remove("purple");
        bool canGetColour = colours.Count > 0
            && !unit.unit.isStarter;

        // Generate the list of new unit options.
        List<UnitColour> newUnits = new List<UnitColour>();
        foreach (Unit u in unit.unit.research)
            if (Master.TranslateCollections(u.colours)
                .Contains(unit.colour))
                newUnits.Add(new UnitColour(u, unit.colour));

        // Decide the output.
        if (canGetColour)
        {
            List<UnitColour> colourOptions = new List<UnitColour>();
            foreach (string colour in colours)
                colourOptions.Add(new UnitColour(unit.unit, colour));

            if (newUnits.Count > 0
                && Random.value <= 0.5)
                return newUnits[Random.Range(0, newUnits.Count)];

            return colourOptions[Random.Range(0, colourOptions.Count)];
        }

        if (newUnits.Count > 0)
            return newUnits[Random.Range(0, newUnits.Count)];

        return unit;
    }
}