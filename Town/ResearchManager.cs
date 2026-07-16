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
    [SerializeField] UnitDisplay resultUnit;
    [SerializeField] ParticleSystem particles;
    [SerializeField] AudioClip newSound, notNewSound;
    [SerializeField] TMP_Text resultHeader, resultUnitText, resultKeywordText;

    GameObject textBox;
    int unitDisplayed;
    List<UnitColour> results = new List<UnitColour>();
    List<bool> newResults = new List<bool>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        textBox = unitText.transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
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
        Master.colourActive = "red";
        lineupPanel.LoadLine(null);
        unitOptions.UpdateUnitOptions("basic", Master.colourActive, false);

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
    }

    public void SelectColour(string colour)
    {
        Master.colourActive = colour;
        unitOptions.UpdateUnitOptions("basic", colour, false);
    }

    public void SelectUnit(int index)
    {
        lineupPanel.Drop(unitOptions.GetUnit(index));
    }

    public void Drag(int index)
    {
        SquadCustomize.selectedUnit = unitOptions.GetUnit(index);
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
            newResults.Add(Master.data.NewUnitColour(result));
        Master.Save();
        RefreshMenu();

        // Start displaying the results.
        Master.OpenMenu(resultsPanel, mainPanel);
        unitDisplayed = -1;
        NextResult();
    }

    public void NextResult()
    {
        unitDisplayed++;
        if (unitDisplayed >= results.Count)
            Master.CloseMenu(resultsPanel, mainPanel);
        else
        {
            UnitColour temp = results[unitDisplayed];
            resultUnit.ChangeUnit(temp);
            resultHeader.text = temp.GetString();
            SquadCustomize.UpdateUnitDescriptions(temp.unit);
            resultUnitText.text = SquadCustomize.unitDescription;
            resultKeywordText.text = SquadCustomize.unitKeywords;

            if (newResults[unitDisplayed])
            {
                GetComponent<AudioSource>().PlayOneShot(newSound);
                particles.Play();
                resultHeader.text += "\nNEW!";
            }
            else
                GetComponent<AudioSource>().PlayOneShot(notNewSound);
        }
    }

    public void CloseMenu()
    {
        SquadCustomize.selectedUnit = null;
        Master.colourActive = "";
        gameObject.SetActive(false);
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