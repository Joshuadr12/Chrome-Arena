using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] TMP_Text headerText, upgradePointText;
    [SerializeField] ScrollPanel scrollPanel;
    [SerializeField] TMP_Text requirementsText;
    [SerializeField] Button confirmButton;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip upgradeSound;

    string collectionName;
    List<Upgrade> upgrades = new List<Upgrade>();
    List<UpgradeButton> buttons = new List<UpgradeButton>();
    Upgrade upgradeActive;
    Upgrade.Layer layerActive;

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

    public void OpenMenu(string collectionId)
    {
        // Update the collection and header text.
        collectionName = collectionId;
        headerText.text = $"{collectionName.FirstCharacterToUpper()} Upgrades";
        
        // Open and refresh the menu.
        gameObject.SetActive(true);
        Town.menuLayer++;
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

        // Update the upgrade panel.
        List<GameObject> newButtons = scrollPanel.Populate(upgrades.Count);
        UpgradeButton newButton;
        for (int i = 0; i < newButtons.Count; i++)
        {
            newButton = newButtons[i].GetComponent<UpgradeButton>();
            newButton.upgrade = upgrades[i];
            buttons.Add(newButton);
        }

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
        audioSource.PlayOneShot(upgradeSound);
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
            RefreshMenu();
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
        Town.menuLayer--;
    }
}

[Serializable]
public class Collection
{
    public string name;
    public List<Upgrade> upgrades;
}