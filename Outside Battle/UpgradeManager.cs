using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public TMP_Text headerText, upgradePointText;
    public RectTransform panelRect;
    public GameObject upgradeButton;
    public TMP_Text requirementsText;
    public Button confirmButton;
    public AudioSource upgradeSoundSource;
    public AudioClip upgradeSound;

    string collectionName;
    List<Upgrade> upgrades = new List<Upgrade>();
    List<UpgradeButton> buttons = new List<UpgradeButton>();
    Upgrade upgradeActive;
    Upgrade.Layer layerActive;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenMenu(string collectionId)
    {
        // Update the collection and header text.
        collectionName = collectionId;
        headerText.text = $"{collectionName.FirstCharacterToUpper()} Upgrades";
        
        // Open and refresh the menu.
        gameObject.SetActive(true);
        RefreshMenu();
    }

    public void RefreshMenu()
    {
        // Display the number of upgrade points the player has.
        upgradePointText.text = $"{Master.data.upgradePoints} upgrade point";
        if (Master.data.upgradePoints != 1)
            upgradePointText.text += "s";

        // Gather upgrades under the given collection.
        upgrades.Clear();
        foreach (string collection in Master.upgrades.Keys)
            if (collection == collectionName)
                foreach (Upgrade upgrade in Master.upgrades[collection])
                    if (upgrade.revealRequirement.RequirementsMet()
                        && Master.data.TimesUpgraded(upgrade) < upgrade.layers.Count)
                        upgrades.Add(upgrade);

        // Destroy pre-existing upgrade buttons.
        foreach (UpgradeButton button in buttons)
            Destroy(button.gameObject);
        buttons.Clear();

        // Update the upgrade panel.
        UpgradeButton newButton;
        foreach (Upgrade upgrade in upgrades)
        {
            newButton = Instantiate(upgradeButton, panelRect)
                .GetComponent<UpgradeButton>();
            newButton.upgrade = upgrade;
            buttons.Add(newButton);
        }
        panelRect.sizeDelta = new Vector2(upgrades.Count * 305 + 5, 0);

        // Update the requirements text and confirm button.
        upgradeActive = null;
        requirementsText.text = upgrades.Count == 0
            ? $"No {collectionName} upgrades for now..."
            : "Click an upgrade above...";
        requirementsText.color = Color.white;
        confirmButton.interactable = false;
    }

    public void SelectUpgrade(Upgrade upgrade)
    {
        // Make the select upgrade button uninteractable.
        upgradeActive = upgrade;
        foreach (UpgradeButton button in buttons)
            button.GetComponent<Button>().interactable
                = button.upgrade != upgrade;

        // Update the requirements text.
        requirementsText.color = Color.red;
        confirmButton.interactable = false;
        layerActive = upgrade.layers
            [Master.data.TimesUpgraded(upgrade)];

        if (layerActive.requirements.RequirementsMet())
        {
            requirementsText.text = $"Costs {layerActive.upgradeCost} upgrade point";
            if (layerActive.upgradeCost != 1)
                requirementsText.text += "s";
            if (Master.data.upgradePoints >= layerActive.upgradeCost)
            {
                requirementsText.color = Color.white;
                confirmButton.interactable = true;
            }
        }
        else
            requirementsText.text = layerActive.requirements.GetDescription();
    }

    public void MakeUpgrade()
    {
        // Make the changes in the data.
        upgradeSoundSource.PlayOneShot(upgradeSound);
        Master.data.upgradePoints -= layerActive.upgradeCost;
        Master.data.MakeUpgrade(upgradeActive);
        Master.Save();

        // Close the upgrade panel or show the new building.
        if (upgradeActive.isBuilding)
        {
            gameObject.SetActive(false);
            FindFirstObjectByType<Town>()
                .RenderResources(upgradeActive.upgradeId);
        }
        else
            RefreshMenu();
    }
}

[Serializable]
public class Collection
{
    public string name;
    public List<Upgrade> upgrades;
}