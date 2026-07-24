using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ArtifactButton : MonoBehaviour
{
    [SerializeField] GameObject infoObject;
    [SerializeField] TMP_Text headerText, requirementText,
        cooldownText;
    [SerializeField] Image image;
    [SerializeField] ParticleSystem particles;
    [Header("Forge"), SerializeField] GameObject priceObject;
    [SerializeField] Image colorImage;
    [SerializeField] TMP_Text colorText;

    [HideInInspector] public ButtonType type;
    [HideInInspector] public GameObject manager;
    [HideInInspector] public Artifact artifact;
    [HideInInspector] public string colour;
    [HideInInspector] public int index, price,
        cooldown = 0;
    [HideInInspector] public Button button;

    AudioSource audioSource;

    public enum ButtonType
    {
        None,
        Forge,
        SquadCustomize,
        Battle
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        headerText.text = artifact.type.name;
        image.sprite = artifact.type.sprite;

        switch (type)
        {
            case ButtonType.Forge:
                Master.OpenMenu(priceObject, requirementText.gameObject);
                colorImage.color = Master.colours[colour].physicalColour;
                colorText.text = price.ToString();
                break;
            case ButtonType.SquadCustomize:
                colour = SquadCustomize.squadActive.colour;
                requirementText.text = $"{artifact.uses} use";
                if (artifact.uses != 1)
                    requirementText.text += "s";
                break;
            case ButtonType.Battle:
                colour = Battle.leftSide.colour;
                break;
            default:
                break;
        }

        image.material = Master.colours[colour].material;
        button = GetComponent<Button>();
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = Master.data.sfxVolume;
    }

    // Update is called once per frame
    void Update()
    {
        if (type == ButtonType.Battle)
        {
            button.interactable = artifact.uses > 0
                && cooldown <= 0
                && Battle.leftArtifactPoints >= artifact.type.valuePerUse;

            if (Battle.leftArtifactPoints < artifact.type.valuePerUse)
            {
                requirementText.text = $"{Battle.leftArtifactPoints}A / {artifact.type.valuePerUse}A";
                requirementText.color = Color.red;
            }
            else
            {
                requirementText.text = $"{artifact.type.valuePerUse}A";
                requirementText.color = Color.black;
            }

        }
    }

    public void HoverEnter()
    {
        switch (type)
        {
            case ButtonType.Forge:
                manager.GetComponent<ForgeManager>()
                    .ArtifactHoverEnter(this);
                break;
            case ButtonType.SquadCustomize:
                manager.GetComponent<SquadCustomize>()
                    .UnitHoverEnter(this);
                break;
            case ButtonType.Battle:
                if (button.interactable)
                    manager.GetComponent<Battle>()
                        .ArtifactHoverEnter(artifact);
                break;
            default:
                break;
        }
    }
    public void HoverExit()
    {
        switch (type)
        {
            case ButtonType.Forge:
                if (ForgeManager.artifactActive != null)
                    manager.GetComponent<ForgeManager>()
                        .ArtifactHoverEnter(ForgeManager.artifactActive);
                else
                    manager.GetComponent<ForgeManager>()
                        .ArtifactHoverExit();
                break;
            case ButtonType.SquadCustomize:
                manager.GetComponent<SquadCustomize>()
                    .UnitHoverExit();
                break;
            case ButtonType.Battle:
                manager.GetComponent<Battle>()
                    .ArtifactHoverExit();
                break;
            default:
                break;
        }
    }

    public void OnClick()
    {
        switch (type)
        {
            case ButtonType.Forge:
                FindFirstObjectByType<ForgeManager>()
                    .SelectArtifact(this);
                break;
            case ButtonType.Battle:
                manager.GetComponent<Battle>()
                    .TriggerAbilities
                    (Cause.CauseType.Artifact, null,
                    Battle.leftArtifact, index);
                Battle.leftArtifactPoints -= artifact.type.valuePerUse;
                artifact.uses--;
                cooldown = 3;
                Master.OpenMenu(cooldownText.gameObject, infoObject);
                cooldownText.text = artifact.uses <= 0
                    ? "BROKEN"
                    : cooldown.ToString();
                HoverExit();

                audioSource.Play();
                particles.Play();
                break;
            default:
                break;
        }
    }

    public string GetDescription()
    {
        string result = $"{colour.FirstCharacterToUpper()} {artifact.type.name}";
        if (Master.data.events.Contains("first_purchase"))
            result += $", {artifact.type.valuePerUse}A per use";
        result += $", {artifact.uses} use";
        if (artifact.uses != 1)
            result += "s";
        result += $"\n{artifact.type.GetDescription(false)}";
        return result;
    }

    public void Cooldown()
    {
        if (artifact.uses > 0 && cooldown > 0)
        {
            cooldown--;
            if (cooldown <= 0)
            {
                Master.CloseMenu(cooldownText.gameObject, infoObject);
                Update();
            }
            else
                cooldownText.text = cooldown.ToString();
        }
    }
}