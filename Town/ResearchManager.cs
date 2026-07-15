using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ResearchManager : MonoBehaviour
{
    [SerializeField] LineupCustomize lineupPanel;
    [SerializeField] UnitOptions unitOptions;
    [SerializeField] UnitDisplay dragAndDropUnit;
    [SerializeField] TMP_Text unitText, keywordText;

    GameObject textBox;

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
    }

    public void OpenMenu()
    {
        gameObject.SetActive(true);
        Master.colourActive = "red";
        lineupPanel.LoadLine(null);
        unitOptions.UpdateUnitOptions("basic", Master.colourActive, false);
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

    public void CloseMenu()
    {
        SquadCustomize.selectedUnit = null;
        Master.colourActive = "";
        gameObject.SetActive(false);
    }
}