using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForgeManager : MonoBehaviour
{
    [SerializeField] ScrollPanel artifactPanel;
    [SerializeField] List<ResourceDisplay> resources;
    [SerializeField] Button purchaseButton;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip purchaseSound;

    List<ArtifactButton> buttons = new List<ArtifactButton>();
    Artifact artifactActive;
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
    }

    public void RefreshMenu()
    {
        // Display the resources the player has.
        foreach (ResourceDisplay display in resources)
        {
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
        int artifactCount = 0;
        foreach (ArtifactList list in Master.data.forgeSales)
            artifactCount += list.artifacts.Count;

        List<GameObject> objects = artifactPanel.Populate(artifactCount);
        int index = 0;
        ArtifactButton newButton;
        foreach (ArtifactList list in Master.data.forgeSales)
        {
            foreach (Artifact artifact in list.artifacts)
            {
                newButton = objects[index].GetComponent<ArtifactButton>();
                newButton.artifact = artifact;
                newButton.colour = list.colour;
                newButton.price = Mathf.RoundToInt(artifact.uses * artifact.type.valuePerUse / 4f) * 5;
                index++;
            }
        }

        purchaseButton.interactable = false;
    }

    public void SelectUpgrade(Upgrade upgrade)
    {
/*        // Make the select upgrade button uninteractable.
        upgradeActive = upgrade;
        foreach (UpgradeButton button in buttons)
            button.GetComponent<Button>().interactable
                = button.upgrade != upgrade;*/
    }

    public void MakeUpgrade()
    {
/*        // Make the changes in the data.
        audioSource.PlayOneShot(purchaseSound);
        Master.data.upgradePoints -= layerActive.upgradeCost;
        Master.data.MakeUpgrade(upgradeActive);
        Master.Save();

        // Close the upgrade panel or show the new building.
        if (upgradeActive.isBuilding)
        {
            FindFirstObjectByType<Town>()
                .RenderResources(upgradeActive.upgradeId);
            CloseMenu();
        }
        else
            RefreshMenu();*/
    }

    public void CloseMenu()
    {
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