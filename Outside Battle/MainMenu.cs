using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    /// <summary>
    /// Manages the main menu.
    /// </summary>

    [Header("Background"), SerializeField] SpriteRenderer background;
    [SerializeField] GameObject backgroundFighter;
    [Header("Credits"), SerializeField] GameObject credits;
    [SerializeField] List<GameObject> objectsToDisable;
    [Header("Save Files"), SerializeField] List<string> saveFiles;
    [SerializeField] List<UnitDisplay> saveCharacters;
    [SerializeField] List<TMP_Text> saveDescriptions;
    [SerializeField] List<Button> deleteButtons;
    [Header("Miscellaneous"), SerializeField]
    AudioSource music;

    [HideInInspector] public string deleteSelection { get; set; }

    Dictionary<string, PlayerData> saves = new Dictionary<string, PlayerData>();
    List<string> unitCollections = new List<string>();
    string randomCollection;
    List<Unit> collection;
    Unit randomUnit;
    List<string> colours;
    string randomColour;
    Vector3 fighterPos;
    GameObject fighter;
    Transform displayTrans;
    UnitDisplay displayComp;
    SortingGroup displaySprite;

    // Start is called before the first frame update.
    void Start()
    {
        // Load the save files.
        music.volume = Master.data.musicVolume;
        foreach (string save in saveFiles)
            saves.Add(save, SaveData.Load(save));

        // Start spawning fighters.
        foreach (string co in Master.unitSets.Keys)
            unitCollections.Add(co);
        InvokeRepeating("SpawnFighter", 0, 0.1f);
    }

    // Update is called once per frame.
    void Update()
    {
        // Expanding the background causes an illusion of movement.
        background.size += Vector2.right * Time.deltaTime;

        // Update the fighters' properties in the background.
        for (int f = 0; f < transform.childCount; f++)
        {
            displayTrans = transform.GetChild(f);
            displayComp = displayTrans.GetComponent<UnitDisplay>();

            // Move the fighters.
            displayTrans.Translate
                (Vector3.right
                * (2 * displayComp.animSpeed - 0.5f)
                * Time.deltaTime);

            // Destroy the fighters when they are off-screen.
            if (displayTrans.position.x >= 10)
            {
                // Not detaching the child causes the game to crash.
                transform.GetChild(f).parent = null;
                Destroy(displayTrans.gameObject);
                f--;
            }
        }
    }

    void SpawnFighter()
    {
        /// <summary>Create a fighter in the background.</summary>

        // Pick a unit.
        do
        {
            randomCollection = unitCollections[Random.Range
                (0,
                unitCollections.Count)];
            collection = Master.unitSets[randomCollection];
            randomUnit = collection[Random.Range
                (0,
                collection.Count)];
        } while ((randomUnit.bodySize > 1)
        || !randomUnit.playable);

        // Pick a colour.
        colours = Master.TranslateCollections
            (randomUnit.colours);
        do
        {
            randomColour = colours[Random.Range(0, colours.Count)];
        }
        // TODO: Remove the second condition when all colors are added.
        while ((randomColour == "neutral")
        || !Master.colours.ContainsKey(randomColour));

        // Instantiate the fighter.
        fighterPos = new Vector3(-10, Random.value * 12 - 6);
        fighter = Instantiate
            (backgroundFighter,
            fighterPos,
            Quaternion.identity,
            transform);
        displayComp = fighter.GetComponent<UnitDisplay>();
        displayComp.ChangeUnit(randomUnit, randomColour);
        displayComp.SetMoveAnimation(true);

        // Arrange the sprites according to their y-position.
        displaySprite = displayComp
            .animator
            .GetComponent<SortingGroup>();
        displaySprite.sortingOrder = Mathf.RoundToInt(fighter.transform.position.y * -999);
    }

    public void UpdateSavePanels()
    {
        /// <summary>Update the information in the save file menu.</summary>

        PlayerData save;
        Unit unit;
        for (int n = 0; n < saveFiles.Count; n++)
        {
            save = saves[saveFiles[n]];
            unit = Master.FindUnit(save.character);
            saveCharacters[n].ChangeUnit(unit);
            displaySprite = saveCharacters[n].animator.GetComponent<SortingGroup>();
            displaySprite.sortingOrder = 10001;
            deleteButtons[n].interactable = save.events.Contains("intro");

            if (save.events.Contains("intro"))
                saveDescriptions[n].text = $"Day {save.day}, Level {save.level[0]}";
            else
                saveDescriptions[n].text = "New Save";
        }
    }

    public void DeleteSave()
    {
        SaveData.Save(Master.newData, deleteSelection);
        saves[deleteSelection] = SaveData.Load(deleteSelection);
        UpdateSavePanels();
    }

    public void LoadAndPlay(string saveFile)
    {
        /// <summary>Load the given save data, then proceed to the level selection menu.</summary>

        Master.LoadData(saves[saveFile]);
        Master.saveFile = saveFile;
        Master.GotoScene("LevelSelect");
    }

    public void Quit()
    {
        /// <summary>Quit the game.</summary>

        Application.Quit();
    }
}