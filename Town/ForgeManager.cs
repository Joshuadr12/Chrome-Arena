using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForgeManager : MonoBehaviour
{
    public static ArtifactButton artifactActive = null;

    [SerializeField] ScrollPanel artifactPanel;
    [SerializeField] GameObject limitText;
    [SerializeField] List<ResourceDisplay> resources;
    [SerializeField] GameObject artifactDesc;
    [SerializeField] TMP_Text artifactText, keywordText;
    [TextArea(1, 2), SerializeField] string tooManySameColor;
    [SerializeField] Button purchaseButton;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip purchaseSound;
    [SerializeField] List<DialogueEvent> openEvents;

    string colour;
    int price;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource.volume = Master.data.sfxVolume;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && Town.menuLayer > 0)
            CloseMenu();
    }

    public void OpenMenu()
    {
        // Open and refresh the menu.
        gameObject.SetActive(true);
        Town.menuLayer++;
        RefreshMenu();
        StartCoroutine(FindFirstObjectByType<DialogueScene>()
            .ExecuteScenes(openEvents));
    }

    public void RefreshMenu()
    {
        // Display the resources the player has.
        foreach (ResourceDisplay display in resources)
        {
            display.text.color = Color.black;
            foreach (Player.ResourceQuantity quantity in Master.data.resources)
            {
                if (quantity.colour == display.colour)
                {
                    display.quantity = quantity.quantity;
                    display.text.text = display.quantity.ToString();
                }
            }
        }

        // Create the artifact buttons.
        artifactActive = null;
        ArtifactHoverExit();
        int artifactCount = 0;
        foreach (ArtifactList list in Master.data.forgeSales)
            artifactCount += list.artifacts.Count;

        if (artifactCount > 0
            && Master.data.artifactsPurchased
            < Master.data.TimesUpgraded("productivity") + 1)
        {
            Master.OpenMenu(artifactPanel.gameObject, limitText);
            List<GameObject> objects = artifactPanel.Populate(artifactCount);
            int index = 0;
            ArtifactButton newButton;
            foreach (ArtifactList list in Master.data.forgeSales)
            {
                foreach (Artifact artifact in list.artifacts)
                {
                    newButton = objects[index].GetComponent<ArtifactButton>();
                    newButton.type = ArtifactButton.ButtonType.Forge;
                    newButton.manager = gameObject;
                    newButton.artifact = artifact;
                    newButton.colour = list.colour;
                    newButton.price = Master.data.events.Contains("first_purchase")
                        ? Mathf.RoundToInt(artifact.uses * artifact.type.valuePerUse / 4f) * 5
                        : 0;
                    newButton.button = newButton.GetComponent<Button>();
                    newButton.button.interactable
                        = Master.GetArtifacts(newButton.colour).Count
                        <= Master.data.TimesUpgraded("backpack");
                    index++;
                }
            }
        }
        else
            Master.CloseMenu(artifactPanel.gameObject, limitText);

        purchaseButton.interactable = false;
    }

    public void ArtifactHoverEnter(ArtifactButton artifact)
    {
        artifactDesc.SetActive(true);
        if (artifact.button.interactable
            || artifactActive == artifact)
        {
            artifactText.text = artifact.GetDescription();
            keywordText.text = artifact.artifact.type.KeywordDescription();
        }
        else
        {
            artifactText.text = tooManySameColor;
            keywordText.text = "";
        }
    }
    public void ArtifactHoverExit()
    {
        artifactDesc.SetActive(false);
    }

    public void SelectArtifact(ArtifactButton artifact)
    {
        // Toggle interactability.
        artifactActive = artifact;
        foreach (ArtifactButton button
            in FindObjectsByType<ArtifactButton>(FindObjectsSortMode.None))
            button.button.interactable = button != artifact
                && Master.GetArtifacts(button.colour).Count
                <= Master.data.TimesUpgraded("backpack");

        // Update the resource panel to reflect the price.
        purchaseButton.interactable = true;
        foreach (ResourceDisplay display in resources)
        {
            foreach (Player.ResourceQuantity quantity in Master.data.resources)
                if (quantity.colour == display.colour)
                    display.quantity = quantity.quantity;

            if (display.colour == artifact.colour)
            {
                display.quantity -= artifact.price;
                display.text.color = new Color(0.5f, 0, 0);
                if (display.quantity < 0)
                    purchaseButton.interactable = false;
            }
            else
                display.text.color = Color.black;
            display.text.text = display.quantity.ToString();
        }
    }

    public void MakePurchase()
    {
        audioSource.PlayOneShot(purchaseSound);
        Master.data.AddResource(artifactActive.colour, -artifactActive.price);

        // Add the artifact.
        bool colourFound = false;
        foreach (ArtifactList list in Master.data.artifacts)
        {
            if (list.colour == artifactActive.colour)
            {
                list.artifacts.Add(artifactActive.artifact);
                colourFound = true;
            }
        }
        if (!colourFound)
        {
            ArtifactList newList = new ArtifactList
                (artifactActive.colour, new List<Artifact>());
            newList.artifacts.Add(artifactActive.artifact);
            Master.data.artifacts.Add(newList);
        }

        // Remove the artifact from the store.
        foreach (ArtifactList list in Master.data.forgeSales)
            if (list.colour == artifactActive.colour)
                list.artifacts.Remove(artifactActive.artifact);

        Master.data.artifactsPurchased++;
        if (artifactActive.price <= 0)
            Master.data.events.Add("first_purchase");
        Master.Save();

        FindFirstObjectByType<Town>().RenderResources();
        RefreshMenu();
    }

    public void CloseMenu()
    {
        artifactActive = null;
        ArtifactHoverExit();
        gameObject.SetActive(false);
        Town.menuLayer--;
    }

    [Serializable]
    public class ResourceDisplay
    {
        public string colour;
        public TMP_Text text;
        public int quantity;
    }
}