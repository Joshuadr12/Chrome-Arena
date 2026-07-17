using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Master : MonoBehaviour
{
    /// <summary>
    /// Oversees the gameplay and contains most global variables and methods.
    /// </summary>

    // Global variables.
    public static string saveFile = "save1";
    public static Player data, newData;
    public static Dictionary<string, Colour> colours = new Dictionary<string, Colour>();
    public static Dictionary<string, List<string>> colourSets = new Dictionary<string, List<string>>();
    public static Dictionary<string, List<Unit>> unitSets = new Dictionary<string, List<Unit>>();
    public static List<Artifact> artifacts = new List<Artifact>();
    public static Dictionary<string, List<Upgrade>> upgrades = new Dictionary<string, List<Upgrade>>();
    public static List<Cause.CauseType> abilityCauses = new List<Cause.CauseType>();
    public static List<Effect.EffectType> abilityEffects = new List<Effect.EffectType>();
    public static List<Keyword> keywords = new List<Keyword>();
    public static BattleLevel battleSelected;
    public static List<Squad> leftSquads = new List<Squad>();
    public static bool masterExecuted = false;
    public static Color bronzeColor;
    public static Color goldColor;
    public static string colourActive = "";
    public static AudioSource backgroundMusic = null;

    // Serialized variables for the editor.
    [SerializeField] bool testing;
    [SerializeField] Color bronzeStarColor, goldStarColor;
    [SerializeField] Player playerData, backupData;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Colour[] colourSet;
    [SerializeField] ColourCollection[] colourCollections;
    [SerializeField] UnitCollection[] unitCollections;
    [SerializeField] Artifact[] artifactSet;
    [SerializeField] Collection[] upgradeCollections;
    [SerializeField, Tooltip("The order in which to activate simultaneous abilities based on their causes")] List<Cause.CauseType> causeOrder;
    [SerializeField, Tooltip("The order in which to activate simultaneous abilities based on their effects")] List<Effect.EffectType> effectOrder;
    [SerializeField] List<Keyword> keywordSet;

    //Start is called before the first frame update.
    void Start()
    {
        // Initialize master settings.
        if (masterExecuted)
            audioSource.Stop();
        else
        {
            backgroundMusic = audioSource;
            DontDestroyOnLoad(audioSource.gameObject);

            bronzeColor = bronzeStarColor;
            goldColor = goldStarColor;

            // Add given colours to the static dictionary for external use.
            foreach (Colour c in colourSet)
                if (!colours.ContainsKey(c.name))
                    colours.Add(c.name, c);

            // Add given colour collections to the static dictionary.
            foreach (ColourCollection co in colourCollections)
            {
                colourSets.Add(co.name, new List<string>());
                foreach (string c in co.colours)
                    colourSets[co.name].Add(c);
            }

            // Add given unit collections to the static dictionary.
            foreach (UnitCollection co in unitCollections)
            {
                unitSets.Add(co.name, new List<Unit>());
                foreach (Unit unit in co.units)
                    unitSets[co.name].Add(unit);
            }

            // Add given artifacts to the static list.
            foreach (Artifact artifact in artifactSet)
                artifacts.Add(artifact);

            // Add given upgrade collections to the static list.
            foreach (Collection collection in upgradeCollections)
            {
                upgrades.Add(collection.name, new List<Upgrade>());
                foreach (Upgrade upgrade in collection.upgrades)
                    upgrades[collection.name].Add(upgrade);
            }

            data = playerData;
            newData = backupData;
            if (testing)
                LoadData(SaveData.Load(saveFile));

            abilityCauses = causeOrder;
            abilityEffects = effectOrder;
            keywords = keywordSet;
            masterExecuted = true;
        }

        RefreshMusic(audioSource.clip);
    }

    public static void RefreshMusic(AudioClip clip)
    {
        if (backgroundMusic != null && backgroundMusic.clip != clip)
        {
            backgroundMusic.clip = clip;
            backgroundMusic.Play();
        }
    }

    public static void Save()
    {
        SaveData.Save(data, saveFile);
    }

    public static void LoadData(PlayerData_0_3_2 saveData)
    {
        /// <summary>Load the given save data.</summary>

        data.battleSpeed = saveData.battleSpeed;
        data.musicVolume = saveData.musicVolume;
        backgroundMusic.volume = data.musicVolume;
        data.sfxVolume = saveData.sfxVolume;
        data.character = FindUnit(saveData.character);
        if (data.character == null)
            data.character = newData.character;
        data.week = saveData.week;

        data.defaultSquads.Clear();
        foreach (int i in saveData.defaultSquads)
            data.defaultSquads.Add(i);

        data.income.Clear();
        foreach (KeyValuePair<string, int> colour in saveData.income)
            data.income.Add(new Player.ResourceQuantity
                (colour.Key, colour.Value));

        data.resources.Clear();
        foreach (KeyValuePair<string, int> colour in saveData.resources)
            data.resources.Add(new Player.ResourceQuantity
                (colour.Key, colour.Value));

        data.upgrades.Clear();
        foreach (KeyValuePair<string, int> upgrade in saveData.upgrades)
            data.upgrades.Add(new Player.UpgradeQuantity
                (upgrade.Key, upgrade.Value));

        data.units.Clear();
        foreach (KeyValuePair<string, List<string>> unit in saveData.units)
            data.units.Add(new Player.UnitColours
                (unit.Key, unit.Value));

        data.stars.Clear();
        foreach (KeyValuePair<string, int> status in saveData.stars)
            data.stars.Add(new Player.BattleStars
                (status.Key, status.Value));

        data.events.Clear();
        foreach (string e in saveData.events)
            data.events.Add(e);

        data.level = saveData.level;
        data.starsLeftover = saveData.starsLeftover;
        data.upgradePoints = saveData.upgradePoints;
        data.researchPoints = saveData.researchPoints;

        Squad squad;
        PlayerData_0_3_2.SquadData squadData;
        Line line;
        for (int s = 0; s < data.squads.Count; s++)
        {
            squad = data.squads[s];
            squadData = saveData.squads[s];
            squad.squadName = squadData.title;
            squad.colour = squadData.colour;
            squad.artifact = FindArtifact(squadData.artifact);

            squad.units.Clear();
            Unit u;
            foreach (List<string> l in squadData.lines)
            {
                line = new Line();
                line.units = new List<Unit>();
                foreach (string unit in l)
                {
                    u = FindUnit(unit);
                    if (u != null)
                        line.units.Add(FindUnit(unit));
                }
                squad.units.Add(line);
            }
        }
    }

    public static void OpenMenu
        (GameObject menu,
        List<GameObject> objectsToDisable)
    {
        /// <summary>Enable and disable the given items.</summary>
        /// <param name="menu">The menu to enable.</param>
        /// <param name="objectsToDisable">The objects to disable.</param>

        foreach (GameObject g in objectsToDisable)
            g.SetActive(false);
        menu.SetActive(true);
    }
    public static void OpenMenu
        (GameObject menu,
        GameObject objectToDisable)
    {
        /// <summary>Enable and disable the given items.</summary>
        /// <param name="menu">The menu to enable.</param>
        /// <param name="objectsToDisable">The objects to disable.</param>

        objectToDisable.SetActive(false);
        menu.SetActive(true);
    }
    public static void CloseMenu
        (GameObject menu,
        List<GameObject> objectsToEnable)
    {
        /// <summary>Enable and disable the given items.</summary>
        /// <param name="menu">The menu to disable.</param>
        /// <param name="objectsToDisable">The objects to enable.</param>

        menu.SetActive(false);
        foreach (GameObject g in objectsToEnable)
            g.SetActive(true);
    }
    public static void CloseMenu
        (GameObject menu,
        GameObject objectToEnable)
    {
        /// <summary>Enable and disable the given items.</summary>
        /// <param name="menu">The menu to disable.</param>
        /// <param name="objectsToDisable">The objects to enable.</param>

        menu.SetActive(false);
        objectToEnable.SetActive(true);
    }

    public static void RenderResourceQuantity(Image obj, Player.ResourceQuantity resource)
    {
        TMP_Text resourceText = obj
            .transform
            .GetComponentInChildren<TMP_Text>();
        obj.GetComponent<Image>().color = colours[resource.colour]
            .physicalColour;
        resourceText.text = resource.quantity.ToString();
    }
    public static void RenderResourceQuantity(Image obj, string colour, int amount)
    {
        TMP_Text resourceText = obj
            .transform
            .GetComponentInChildren<TMP_Text>();
        obj.GetComponent<Image>().color = colours[colour]
            .physicalColour;
        resourceText.text = amount.ToString();
    }

    public static bool FinishedTutorial(string level = "basic_1")
    {
        return (GetStars(level) >= 1)
            || data.events.Contains("skip_tutorial");
    }

    public static int GetStars()
    {
        foreach (Player.BattleStars star in data.stars)
            if (star.battleId == battleSelected.battleId)
                return star.stars;
        return 0;
    }
    public static int GetStars(string levelId)
    {
        foreach (Player.BattleStars star in data.stars)
            if (star.battleId == levelId)
                return star.stars;
        return 0;
    }
    public static int GetStars(BattleLevel level)
    {
        foreach (Player.BattleStars star in data.stars)
            if (star.battleId == level.battleId)
                return star.stars;
        return 0;
    }

    public static void SetStars(string levelId, int stars)
    {
        foreach (Player.BattleStars star in data.stars)
            if (star.battleId == levelId)
            {
                star.stars = stars;
                return;
            }
        data.stars.Add(new Player.BattleStars(levelId, stars));
    }

    public static int[] GetLevel()
    {
        int totalStars = 0;
        foreach (Player.BattleStars star in data.stars)
            totalStars += star.stars;

        int[] result = new int[2] { 0, 0 };
        while (totalStars >= result[0] / 10 + 2)
        {
            totalStars -= result[0] / 10 + 2;
            result[0]++;
        }

        result[1] = totalStars;
        return result;
    }

    public static void NextWeek()
    {
        data.NextWeek();
        Save();
    }

    public static void RenderSquadDropdowns
        (List<TMP_Dropdown> dropdowns,
        List<string> squadsChosen,
        List<int> squadAmounts = null)
    {
        /// <summary>Update the squad dropdowns and their options according to the chosen squads, and update their sizes.</summary>

        List<string> squadNames = new List<string>();
        foreach (Squad s in data.squads)
            squadNames.Add(s.squadName);

        List<string> dropdownOptions = new List<string>();
        TMP_Dropdown dropdown;
        Squad squad;
        for (int drop = 0; drop < 3; drop++)
        {
            dropdownOptions.Clear();
            dropdownOptions.Add(squadsChosen[drop]);
            foreach (string s in squadNames)
                if (!squadsChosen.Contains(s))
                    dropdownOptions.Add(s);

            dropdown = dropdowns[drop];
            dropdown.ClearOptions();
            dropdown.AddOptions(dropdownOptions);
            dropdown.value = 0;

            squad = data
                .squads[squadNames.IndexOf(squadsChosen[drop])];
            dropdown.GetComponent<Image>().color = colours[squad.colour]
                .physicalColour;
            if (squadAmounts != null)
            {
                if (squadAmounts.Count <= 0)
                    for (int i = 0; i < squadsChosen.Count; i++)
                        squadAmounts.Add(battleSelected.squadSize);
                foreach (Player.ResourceQuantity resource in data.resources)
                    if (resource.colour == squad.colour)
                        squadAmounts[drop] = Math.Min
                            (resource.quantity,
                            squadAmounts[drop]);
                squad.startMoney = squadAmounts[drop];
            }
        }
    }

    public static float AnimationCurve
        (float state,
        bool smoothStart = false,
        bool smoothEnd = false)
    {
        /// <summary>Calculates and returns a point on an animation curve.</summary>
        /// <param name="state">The input value for the curve.</param>
        /// <param name="smoothStart">Whether the start of the curve is smooth or flat.</param>
        /// <param name="smoothEnd">Whether the end of the curve is smooth or flat.</param>

        // When the start is smooth.
        return smoothStart
            ? (smoothEnd
                ? (1 - Mathf.Cos(state * Mathf.PI)) / 2
                : 1 - Mathf.Cos(state * Mathf.PI / 2))
        // When the start is abrupt.
            : (smoothEnd
                ? Mathf.Sin(state * Mathf.PI / 2)
                : state);
    }

    public static List<string> TranslateCollections(List<string> collections, bool includeNeutral = true)
    {
        /// <summary>Returns all colours under the given list of colour collections.</summary>

        List<string> result = new List<string>();
        List<string> temp = new List<string>();
        List<string> all = colourSets["all"];
        foreach (string collection in collections)
        {
            // The inverse of a collection is indicated with a '!' at the start.
            if (collection[0] == '!')
            {
                temp.Add(collection.Substring(1));
                temp = TranslateCollections(temp);
                foreach (string colour in all)
                    if (!temp.Contains(colour)
                        && !result.Contains(colour))
                        result.Add(colour);
            }
            else if (colourSets.ContainsKey(collection))
            {
                foreach (string colour in colourSets[collection])
                    if (!result.Contains(colour))
                        result.Add(colour);
            }
            // Unrecognized collections are treated as single colours.
            else
                result.Add(collection);
        }
        // Neutral includes all units.
        if (includeNeutral && !result.Contains("neutral"))
            result.Add("neutral");
        return result;
    }

    public static List<Unit> GetUnits
        (string collection = "all",
        string colour = "all")
    {
        List<Unit> result = new List<Unit>();
        foreach (string co in unitSets.Keys)
        {
            if ((collection == "all") || (co == collection))
            {
                foreach (Unit unit in unitSets[co])
                    if (data.HasUnitColour(unit, colour))
                        result.Add(unit);
            }
        }
        return result;
    }
    public static List<Artifact> GetArtifacts(string colour = "all")
    {
        List<Artifact> result = new List<Artifact>();
        foreach (Artifact artifact in artifacts)
            if ((colour == "all")
                || TranslateCollections(artifact.colours)
                .Contains(colour))
                if (artifact.playable)
                    result.Add(artifact);
        return result;
    }

    public static Unit FindUnit(string name)
    {
        foreach (List<Unit> collection in unitSets.Values)
            foreach (Unit unit in collection)
                if (unit.name == name)
                    return unit;
        Debug.LogWarning(name + " unit not found.");
        return null;
    }

    public static Artifact FindArtifact(string name)
    {
        foreach (Artifact artifact in artifacts)
            if (artifact.name == name)
                return artifact;
        Debug.LogWarning(name + " artifact not found.");
        return null;
    }

    public static IEnumerator SetTimer
        (float duration,
        bool applyBattlespeed = true)
    {
        /// <summary>Wait the given number of seconds, applying battle speed if needed.</summary>
        /// <param name="duration">The duration in seconds.</param>
        /// <param name="applyBattleSpeed">When set to true, the timer speed is parallel to the set battle speed.</param>

        while (duration > 0)
        {
            yield return new WaitForEndOfFrame();
            duration -= Time.deltaTime * (applyBattlespeed ? data.battleSpeed : 1);
        }
    }

    public static int ChooseRandom(List<float> weights)
    {
        /// <summary>
        /// Chooses a random weight from the given list and returns its index. Higher weights are more likely to be chosen.
        /// </summary>

        float sum = 0;
        foreach (float weight in weights)
            sum += weight;

        float choice = UnityEngine.Random.value * sum;
        for (int i = 0; i < weights.Count; i++)
        {
            if (choice <= weights[i])
                return i;
            choice -= weights[i];
        }
        return 0;
    }

    public static int ChooseRandom(List<int> weights)
    {
        /// <summary>
        /// Chooses a random weight from the given list and returns its index. Higher weights are more likely to be chosen.
        /// </summary>

        int sum = 0;
        foreach (int weight in weights)
            sum += weight;
        if (sum <= 0)
            return 0;

        int choice = UnityEngine.Random.Range(0, sum);
        for (int i = 0; i < weights.Count; i++)
        {
            if (choice < weights[i])
                return i;
            choice -= weights[i];
        }
        return 0;
    }

    public static void GotoScene(string sceneName)
    {
        /// <summary>Load the given scene.</summary>
        /// <param name="sceneName">The scene to load.</param>

        SceneManager.LoadScene(sceneName);
    }
}

[Serializable]
public class Colour
{
    /// <summary>
    /// Represents a squad/unit color.
    /// </summary>

    // Variables.
    public string name;
    public Color physicalColour = Color.white;
    public Material material;
    public bool whiteText, createPaint = true;
    public float red, yellow, blue;
    //public List<Unit> playableUnits, extraUnits;

    public float Advantage(Colour other)
    {
        /// <summary>Calculates and returns this color's advantage over another color.</summary>
        /// <param name="other">The other color over which this color has the advantage.</param>

        float total = red * (other.yellow - other.blue);
        total += yellow * (other.blue - other.red);
        total += blue * (other.red - other.yellow);
        return total;
    }
}

[Serializable]
public class ColourCollection
{
    public string name;
    public List<string> colours;
}

[Serializable]
public class UnitCollection
{
    public string name;
    public List<Unit> units;
}

[Serializable]
public class Keyword
{
    /// <summary>
    /// Represents a unit keyword.
    /// </summary>

    // Variables.
    public string name;
    [TextArea(3, 3)] public string description;
}

[Serializable]
public class UnitColour
{
    public Unit unit;
    public string colour;

    public UnitColour(Unit unit)
    {
        this.unit = unit;
        if (SquadCustomize.squadActive != null)
            colour = SquadCustomize.squadActive.colour;
        else if (Master.colourActive != "")
            colour = Master.colourActive;
        else
            colour = "neutral";
    }
    public UnitColour(Unit unit, string colour)
    {
        this.unit = unit;
        this.colour = colour;
    }

    public string GetString()
    {
        return $"{colour.FirstCharacterToUpper()} {unit.name}";
    }
}

[Serializable]
public class RequirementSet
{
    /// <summary>
    /// Represents a set of requirements needed to progress or trigger something.
    /// </summary>

    public List<Requirement> requirements;
    public int requirementsNeeded;

    public bool RequirementsMet()
    {
        int result = 0;
        foreach (Requirement r in requirements)
            if (r.IsMet())
                result++;
        return result >= requirementsNeeded;
    }

    public string GetDescription()
    {
        string result = "";
        foreach (Requirement requirement in requirements)
            if (!requirement.IsMet())
                result += requirement.GetDescription() + "\n";
        return result.Trim();
    }

    [Serializable]
    public class Requirement
    {
        public enum RequireType
        {
            StarsFromBattle,
            Event,
            RoundsDone,
            BattleSelected,
            Upgrade,
            Level,
            Rank
        }

        public RequireType type;
        public string identifier;
        public int scoreNeeded = 1;
        public string overrideDescription;

        public string GetDescription()
        {
            if (overrideDescription != "")
                return overrideDescription;

            switch (type)
            {
                case RequireType.StarsFromBattle:
                    return scoreNeeded == 1
                        ? $"Clear {identifier}."
                        : $"Clear {identifier} with {scoreNeeded} stars.";
                case RequireType.Upgrade:
                    return $"Upgrade {identifier}.";
                case RequireType.Level:
                    return $"Reach Level {scoreNeeded}.";
                case RequireType.Rank:
                    return $"Reach Rank {scoreNeeded}.";
                default:
                    return "???";
            }
        }

        public bool IsMet()
        {
            switch (type)
            {
                case RequireType.StarsFromBattle:
                    return Master.GetStars(identifier) >= scoreNeeded;
                case RequireType.Event:
                    return Master.data.events.Contains(identifier);
                case RequireType.RoundsDone:
                    return SquadSelect.roundsDone == scoreNeeded;
                case RequireType.BattleSelected:
                    return Master.battleSelected.battleId == identifier;
                case RequireType.Upgrade:
                    return Master.data.TimesUpgraded(identifier) >= scoreNeeded;
                case RequireType.Level:
                    return Master.data.level >= scoreNeeded;
                case RequireType.Rank:
                    return Master.data.TimesUpgraded("rank") >= scoreNeeded;
                default:
                    Debug.LogWarning($"Unknown requirement type: {type}");
                    return false;
            }
        }
    }
}