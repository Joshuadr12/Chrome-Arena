using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    /// <summary>
    /// Manages the level selection menu.
    /// </summary>

    [SerializeField] List<GameObject> buttons;
    [SerializeField] GameObject squadMenu;
    [SerializeField] TMP_Text dayText;
    [SerializeField] List<Image> resourceDisplay;

    //Start is called before the first frame update.
    void Start()
    {
        RenderResources();
    }

    public void RenderResources()
    {
        /// <summary>Update the resources section with the current day and resources.</summary>

        Image resourceImage;
        int index = 0;
        dayText.text = "Day " + Master.data.day.ToString();

        foreach (Player.ResourceQuantity resource in Master.data.resources)
        {
            if (index < resourceDisplay.Count)
            {
                resourceImage = resourceDisplay[index];
                resourceImage.gameObject.SetActive(true);
                Master.RenderResourceQuantity(resourceImage, resource);
                index++;
            }
        }

        while (index < resourceDisplay.Count)
        {
            resourceImage = resourceDisplay[index];
            resourceImage.gameObject.SetActive(false);
            index++;
        }
    }

    public void SelectLevel(Level level)
    {
        /// <summary>Open the menu to set up for the selected level, or begin immediately if it's a tutorial.</summary>
        /// <param name="level">The level selected.</param>

        Master.levelSelected = level;

        // When the level is a tutorial.
        if (Master.FinishedTutorial())
        {
            Master.OpenMenu(squadMenu, buttons);
            squadMenu.GetComponent<SquadMenu>().SetupMenu();
        }
        else
        {
            Master.leftSquads.Clear();
            for (int i = 0; i < 6; i += 2)
            {
                Master.data.squads[i].startMoney = Master.levelSelected.squadSize;
                Master.leftSquads.Add(Master.data.squads[i]);
            }
            SquadSelect.fairStars = 1;
            Master.GotoScene("SquadSelect");
        }
    }

    public void CancelLevel()
    {
        /// <summary>Close the setup menu and return to level selection.</summary>

        Master.CloseMenu(squadMenu, buttons);
    }
}