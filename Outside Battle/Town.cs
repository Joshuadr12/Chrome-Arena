using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Serialization;

public class Town : MonoBehaviour
{
    /// <summary>
    /// Manages the town.
    /// </summary>

    [SerializeField, FormerlySerializedAs("dayText")] TMP_Text weekText;
    [SerializeField] List<Image> resourceDisplay;
    [SerializeField] List<ButtonRequirement> buttonsToEnable;


    //Start is called before the first frame update.
    void Start()
    {
        RenderResources();
    }

    //Update is called once per frame.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Master.GotoScene("MainMenu");
    }

    public void RenderResources()
    {
        /// <summary>Update the conditionally active buttons, then update the resources section with the current day and resources.</summary>

        foreach (ButtonRequirement button in buttonsToEnable)
            button.RenderObject();

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

    public void NextWeek()
    {
        Master.data.NextWeek();
        Master.Save();
        RenderResources();
    }
}

[Serializable]
public class ButtonRequirement
{
    [SerializeField] GameObject obj;
    [SerializeField] RequirementSet requirements;

    public void RenderObject()
    {
        obj.SetActive(requirements.RequirementsMet());
    }
}