using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Town : MonoBehaviour
{
    /// <summary>
    /// Manages the town.
    /// </summary>

    public static int menuLayer;

    [SerializeField] TMP_Text levelAndPoints;
    public Transform castle;
    [SerializeField] GameObject splashScreen, buildingParticles;
    [SerializeField] List<ButtonRequirement> buttonsToEnable;


    //Start is called before the first frame update.
    void Start()
    {
        Master.backgroundMusic.volume = Master.data.musicVolume;
        StartCoroutine(Master.DisableSplashScreen(splashScreen));
        menuLayer = 0;
        RenderResources();
    }

    //Update is called once per frame.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && menuLayer == 0
            && DialogueScene.isDone)
            StartCoroutine(Master.GotoScene("MainMenu"));
    }

    public void RenderResources(string justUpgraded = "")
    {
        /// <summary>Update the conditionally active buttons.</summary>
        /// <param name="justUpgraded">If a new building was just built, enter the upgrade ID to create particle effects at the building.</param>

        levelAndPoints.text = $"Level {Master.data.level}\n{Master.data.upgradePoints} upgrade point";
        if (Master.data.upgradePoints != 1)
            levelAndPoints.text += "s";
        levelAndPoints.text += $"\n{Master.data.researchPoints} research point";
        if (Master.data.researchPoints != 1)
            levelAndPoints.text += "s";

        foreach (ButtonRequirement button in buttonsToEnable)
            RenderObject(button, justUpgraded);
    }

    public void RenderObject(ButtonRequirement obj, string justUpgraded = "")
    {
        obj.obj.SetActive(obj.requirements.RequirementsMet());
        if (obj.obj.name == justUpgraded)
            Instantiate(buildingParticles, obj.obj.transform);
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