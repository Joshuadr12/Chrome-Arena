using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Town : MonoBehaviour
{
    /// <summary>
    /// Manages the town.
    /// </summary>

    public static int menuLayer;

    [SerializeField, FormerlySerializedAs("dayText")] TMP_Text weekText;
    public Transform castle;
    [SerializeField] GameObject buildingParticles;
    [SerializeField] List<Image> resourceDisplay;
    [SerializeField] List<ButtonRequirement> buttonsToEnable;


    //Start is called before the first frame update.
    void Start()
    {
        Master.backgroundMusic.volume = Master.data.musicVolume;
        menuLayer = 0;
        RenderResources();
    }

    //Update is called once per frame.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && menuLayer == 0
            && DialogueScene.isDone)
            Master.GotoScene("MainMenu");
    }

    public void RenderResources(string justUpgraded = "")
    {
        /// <summary>Update the conditionally active buttons, then update the resources section with the current week and resources.</summary>
        /// <param name="justUpgraded">If a new building was just built, enter the upgrade ID to create particle effects at the building.</param>

        foreach (ButtonRequirement button in buttonsToEnable)
            RenderObject(button, justUpgraded);

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

    public void RenderObject(ButtonRequirement obj, string justUpgraded = "")
    {
        obj.obj.SetActive(obj.requirements.RequirementsMet());
        if (obj.obj.name == justUpgraded)
            Instantiate(buildingParticles, obj.obj.transform);
    }

    public void NextWeek()
    {
        Master.data.NextWeek();
        Master.Save();
        RenderResources();
    }

    public void CreateParticles(Transform location)
    {
        Instantiate(buildingParticles, location.position, Quaternion.identity);
    }
}

[Serializable]
public struct ButtonRequirement
{
    public GameObject obj;
    public RequirementSet requirements;
}