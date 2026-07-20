using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class BattleSelect : MonoBehaviour
{
    /// <summary>
    /// Manages the level selection menu.
    /// </summary>

    [SerializeField] List<GameObject> buttons;
    [SerializeField] GameObject squadMenu;
    [SerializeField, FormerlySerializedAs("dayText")] TMP_Text weekText;
    [SerializeField] List<Image> resourceDisplay;


    //Start is called before the first frame update.
    void Start()
    {
        Master.battleSelected = null;
        RenderResources();
    }

    //Update is called once per frame.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)
            && DialogueScene.isDone)
        {
            if (Master.battleSelected == null)
                BackToTown();
            else
                CancelBattle();
        }
    }

    public void RenderResources()
    {
        /// <summary>Update the resources section with the current day and resources.</summary>

        Image resourceImage;
        int index = 0;
        weekText.text = "Week " + Master.data.week.ToString();

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

    public void SelectBattle(BattleLevel battle)
    {
        /// <summary>Open the menu to set up for the selected battle, or begin immediately if it's a tutorial.</summary>
        /// <param name="battle">The battle selected.</param>

        Master.battleSelected = battle;

        // When the battle is a tutorial.
        if (Master.FinishedTutorial())
        {
            Master.OpenMenu(squadMenu, buttons);
            squadMenu.GetComponent<SquadMenu>().SetupMenu();
        }
        else
        {
            Master.leftSquads.Clear();
            for (int i = 0; i < 3; i++)
            {
                Master.data.squads[i].startMoney = Master.battleSelected.squadSize;
                Master.leftSquads.Add(Master.data.squads[i]);
            }
            SquadSelect.fairStars = 1;
            Master.GotoScene("SquadSelect");
        }
    }

    public void CancelBattle()
    {
        /// <summary>Close the setup menu and return to battle selection.</summary>

        Master.battleSelected = null;
        Master.CloseMenu(squadMenu, buttons);
    }

    public void BackToTown()
    {
        Master.GotoScene("Town");
    }
}