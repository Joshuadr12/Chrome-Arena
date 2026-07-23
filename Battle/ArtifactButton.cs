using TMPro;
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
        if (battle && button.interactable)
            battle.ArtifactHoverEnter(artifact);
    }
    public void HoverExit()
    {
        if (battle)
            battle.ArtifactHoverExit();
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