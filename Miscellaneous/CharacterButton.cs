using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CharacterButton : MonoBehaviour
{
    [SerializeField] ButtonType buttonType;
    [SerializeField] GameObject manager;
    [SerializeField] UnitDisplay unitDisplay;
    [SerializeField] Image artifactImage;

    UnitColour unit = new UnitColour(null, null);
    Artifact artifact = null;

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
        Master.CloseMenu(artifactImage.gameObject,
            unitDisplay.gameObject);
        unitDisplay.ChangeUnit(unit);

        if (unit.unit == null)
            unitDisplay.gameObject.SetActive(false);
    }
    public void SetUnit(Unit unit, string colour)
    {
        this.unit.unit = unit;
        this.unit.colour = colour;
        gameObject.SetActive(true);
        Master.CloseMenu(artifactImage.gameObject,
            unitDisplay.gameObject);
        unitDisplay.ChangeUnit(unit, colour);

        if (unit == null)
            unitDisplay.gameObject.SetActive(false);
    }
    public void SetArtifact(Artifact artifact, string colour)
    {
        this.artifact = artifact;
        this.unit.colour = colour;
        gameObject.SetActive(true);
        Master.OpenMenu(artifactImage.gameObject,
            unitDisplay.gameObject);
        artifactImage.sprite = artifact.sprite;
        artifactImage.material = Master.colours[colour].material;
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
                case ButtonType.SquadOption:
                    if (SquadCustomize.chooseArtifact)
                        manager.GetComponent<SquadCustomize>()
                            .SelectArtifact(artifact);
                    break;
                default:
                    break;
            }
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
                        .UnitHoverEnter(unit.unit, artifact);
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
        if (unit.unit != null)
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