using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public ButtonType buttonType;
    public GameObject manager;
    [SerializeField] UnitDisplay unitDisplay;
    [Tooltip("If this button is part of a lineup panel, enter its index here."), SerializeField] int lineIndex;

    UnitColour unit = new UnitColour(null, null);
    bool isLeftClicking = false;

    public enum ButtonType
    {
        None,
        DisplayCharacter,
        ResearchOption,
        ResearchSelected,
        ResearchResults,
        SquadOption,
        SquadLine
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUnit(UnitColour unit)
    {
        this.unit = unit;
        gameObject.SetActive(true);
        unitDisplay.ChangeUnit(unit);
        unitDisplay.gameObject.SetActive(unit.unit != null);
    }
    public void SetUnit(Unit unit, string colour)
    {
        this.unit.unit = unit;
        this.unit.colour = colour;
        gameObject.SetActive(true);
        unitDisplay.ChangeUnit(unit, colour);
        unitDisplay.gameObject.SetActive(unit != null);
    }
    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void OnClick()
    {
        if (unit.unit != null)
            switch (buttonType)
            {
                case ButtonType.DisplayCharacter:
                    Master.data.character = unit.unit;
                    break;
                case ButtonType.ResearchOption:
                    manager.GetComponent<ResearchManager>()
                        .SelectUnit(unit.unit);
                    break;
                default:
                    break;
            }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right
            && unit.unit != null)
        {
            switch (buttonType)
            {
                case ButtonType.ResearchSelected:
                    manager.GetComponent<ResearchManager>()
                        .RemoveSelected(lineIndex);
                    manager.GetComponent<ResearchManager>()
                        .UnitHoverExit();
                    break;
                case ButtonType.SquadLine:
                    manager.GetComponent<LineupCustomize>()
                        .RemoveUnit(lineIndex);
                    manager.GetComponent<LineupCustomize>()
                        .UnitHoverExit();
                    break;
                default:
                    break;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            isLeftClicking = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            isLeftClicking = false;
    }

    public void OnHoverEnter()
    {
        if (unit.unit != null)
            switch (buttonType)
            {
                case ButtonType.ResearchOption:
                    manager.GetComponent<ResearchManager>()
                        .UnitHoverEnter(unit.unit);
                    break;
                case ButtonType.ResearchSelected:
                    manager.GetComponent<ResearchManager>()
                        .UnitHoverEnter(unit.unit);
                    break;
                case ButtonType.ResearchResults:
                    manager.GetComponent<ResearchManager>()
                        .UnitHoverEnter(unit.unit);
                    break;
                case ButtonType.SquadOption:
                    manager.GetComponent<SquadCustomize>()
                        .UnitHoverEnter(unit.unit);
                    break;
                case ButtonType.SquadLine:
                    manager.GetComponent<LineupCustomize>()
                        .UnitHoverEnter(unit.unit);
                    break;
                default:
                    break;
            }
    }

    public void OnHoverExit()
    {
        switch (buttonType)
        {
            case ButtonType.ResearchOption:
                manager.GetComponent<ResearchManager>()
                    .UnitHoverExit();
                break;
            case ButtonType.ResearchSelected:
                manager.GetComponent<ResearchManager>()
                    .UnitHoverExit();
                break;
            case ButtonType.ResearchResults:
                manager.GetComponent<ResearchManager>()
                    .UnitHoverExit();
                break;
            case ButtonType.SquadOption:
                manager.GetComponent<SquadCustomize>()
                    .UnitHoverExit();
                break;
            case ButtonType.SquadLine:
                manager.GetComponent<LineupCustomize>()
                    .UnitHoverExit();
                break;
            default:
                break;
        }
    }

    public void OnDrag()
    {
        if (isLeftClicking && unit.unit != null)
            switch (buttonType)
            {
                case ButtonType.ResearchOption:
                    manager.GetComponent<ResearchManager>()
                        .Drag(unit.unit);
                    break;
                case ButtonType.SquadOption:
                    manager.GetComponent<SquadCustomize>()
                        .Drag(unit.unit);
                    break;
                default:
                    break;
            }
    }

    public void OnDrop()
    {
        if (SquadCustomize.selectedUnit != null)
            switch (buttonType)
            {
                case ButtonType.ResearchSelected:
                    manager.GetComponent<ResearchManager>()
                        .SelectUnit(SquadCustomize.selectedUnit);
                    break;
                // TODO: Allow for more than one button in a line.
                case ButtonType.SquadLine:
                    manager.GetComponent<LineupCustomize>()
                        .Drop(0);
                    break;
                default:
                    break;
            }
    }
}