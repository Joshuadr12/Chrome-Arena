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

    [HideInInspector] public Artifact artifact;
    [HideInInspector] public string colour;
    [HideInInspector] public Battle battle = null;
    [HideInInspector] public int index, price,
        cooldown = 0;

    Button button;
    AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        headerText.text = artifact.type.name;
        image.sprite = artifact.type.sprite;

        if (battle)
            colour = Battle.leftSide.colour;
        else
        {
            Master.OpenMenu(priceObject, requirementText.gameObject);
            colorImage.color = Master.colours[colour].physicalColour;
            colorText.text = price.ToString();
        }
        image.material = Master.colours[colour].material;

        button = GetComponent<Button>();
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = Master.data.sfxVolume;
    }

    // Update is called once per frame
    void Update()
    {
        if (battle)
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

    public void HoverEnter()
    {
        if (button.interactable)
        {
            if (battle)
                battle.ArtifactHoverEnter(artifact);
            else
                FindFirstObjectByType<ForgeManager>()
                    .ArtifactHoverEnter(this);
        }

    }
    public void HoverExit()
    {
        if (battle)
            battle.ArtifactHoverExit();
        else if (ForgeManager.artifactActive != null)
            FindFirstObjectByType<ForgeManager>()
                .ArtifactHoverEnter(ForgeManager.artifactActive);
        else
            FindFirstObjectByType<ForgeManager>()
                .ArtifactHoverExit();
    }

    public void OnClick()
    {
        if (battle)
        {
            battle.TriggerAbilities
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
        }
        else
            FindFirstObjectByType<ForgeManager>()
                .SelectArtifact(this);
    }

    public string GetDescription()
    {
        string result = $"{colour.FirstCharacterToUpper()} {artifact.type.name}, costs ${price}";
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