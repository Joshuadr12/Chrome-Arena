using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SquadSelect : MonoBehaviour
{
    /// <summary>
    /// Manages the squad selection menu in battle.
    /// </summary>

    // Global variables.
    public static bool began = false, artifactTutorial = true;
    public static int roundsDone, fairStars;
    public static List<SquadStatus> leftArmy, rightArmy;
    public static AudioSource music;

    // Serialized variables for the editor.
    [Header("Menu")]
    [SerializeField] List<SquadStatus> leftSquads;
    [SerializeField] List<SquadStatus> rightSquads;
    [SerializeField] UnitDisplay leftDisplay, rightDisplay;
    [SerializeField] TMP_Text instructions, countdown;
    [SerializeField] Image advantageDiagram;
    [SerializeField] string cancelPrompt, winMessage, loseMessage;
    [SerializeField] KeyCode cancelKey;
    [SerializeField] AudioClip selectAudio, cancelAudio;
    [Header("Battle Results")]
    [SerializeField] GameObject retreatWarning;
    [SerializeField] Image resultsPanel;
    [SerializeField] GameObject resultsWindow, resultsBody;
    [SerializeField] TMP_Text resultsText;
    [SerializeField] AudioSource resultsAudio;
    [SerializeField] AudioClip winSound, failSound, bronzeSound, goldSound, levelSound;
    [SerializeField] List<Image> starCount;
    [SerializeField] List<Transform> starRequirements;
    [SerializeField] TMP_Text levelText;
    [SerializeField] Slider levelMeter;
    [SerializeField] GameObject starLossText;
    [SerializeField] List<Image> resourceQuantities;
    [Header("Miscellaneous")]
    [SerializeField] GameObject battleMusic;

    // Miscellaneous variables.
    bool gameOver = false;
    // countTimer is also used after the game concludes to determine which side won.
    float countTimer = 4;
    SquadStatus leftChoice, rightChoice;
    AudioSource source;
    Color bronzeFade, goldFade;

    // Start is called before the first frame update.
    void Start()
    {
        source = GetComponent<AudioSource>();
        source.volume = Master.data.sfxVolume;
        leftDisplay.ChangeUnit
            (Master.data.character,
            "neutral", true);
        rightDisplay.ChangeUnit
            (Master.levelSelected.opponentDisplay,
            "neutral", true);

        // Disable used squads.
        if (began)
        {
            gameOver = true;
            for (int n = 0; n < leftArmy.Count; n++)
            {
                // Game over if all squads have been used.
                leftArmy[n].button = leftSquads[n].button;
                if (leftArmy[n].outcome == 0)
                    gameOver = false;
                else
                    leftArmy[n].button.SetActive(false);

                rightArmy[n].button = rightSquads[n].button;
                if (rightArmy[n].outcome != 0)
                    rightArmy[n].button.SetActive(false);
            }
            if (gameOver)
            {
                countTimer = 0;
                foreach (SquadStatus s in leftArmy)
                    countTimer += s.outcome;
            }
        }
        // When the battle begins.
        else
        {
            // Music
            music = Instantiate(battleMusic).GetComponent<AudioSource>();
            if (Master.levelSelected.settings.specialMusic)
            {
                music.clip = Master
                    .levelSelected
                    .settings
                    .specialMusic;
                music.Play();
            }
            music.volume = Master.data.musicVolume;
            DontDestroyOnLoad(music.gameObject);

            // Squads
            List<Squad> squads = Master
                .levelSelected
                .ChooseSquads();
            for (int s = 0; s < squads.Count; s++)
            {
                leftSquads[s].squad = Master.leftSquads[s];
                rightSquads[s].squad = squads[s];
            }

            leftArmy = new List<SquadStatus>();
            rightArmy = new List<SquadStatus>();
            foreach (SquadStatus s in leftSquads)
                leftArmy.Add(s);
            foreach (SquadStatus s in rightSquads)
                rightArmy.Add(s);
            roundsDone = 0;
            began = true;

            // Resources
            foreach (SquadStatus squad in leftArmy)
                Master.data.AddResource(squad.squad.colour, -squad.squad.startMoney / 2);
            Master.Save();

            StarChallenges.totalScore.Reset();
        }

        if (gameOver)
        {
            // Die animation on game over.
            if (countTimer > 0)
            {
                rightDisplay.Die();
                resultsText.text = winMessage;
            }
            else
            {
                leftDisplay.Die();
                resultsText.text = loseMessage;
            }
            StartCoroutine(BattleResults());
        }
        else
        {
            advantageDiagram.gameObject.SetActive
                (Master.data.events.Contains("squad_select")
                || Master.data.events.Contains("skip_tutorial"));

            // Button colors and text
            for (int n = 0; n < leftArmy.Count; n++)
            {
                leftArmy[n].button.GetComponent<Image>().color = Master
                    .colours[leftArmy[n].squad.colour]
                    .physicalColour;
                leftArmy[n].button.transform.GetChild(0).GetComponent<TMP_Text>().text = leftArmy[n]
                    .squad
                    .squadName;
                if (Master
                    .colours[leftArmy[n].squad.colour]
                    .whiteText)
                    leftArmy[n].button.transform.GetChild(0).GetComponent<TMP_Text>().color = Color.white;
                leftArmy[n].button.transform.GetChild(0).GetComponent<TMP_Text>().text = leftArmy[n]
                    .squad
                    .squadName;
                rightArmy[n].button.GetComponent<Image>().color = Master
                    .colours[rightArmy[n].squad.colour]
                    .physicalColour;
                rightArmy[n].button.transform.GetChild(0).GetComponent<TMP_Text>().text = rightArmy[n]
                    .squad
                    .squadName;
                if (Master
                    .colours[rightArmy[n].squad.colour]
                    .whiteText)
                    rightArmy[n].button.transform.GetChild(0).GetComponent<TMP_Text>().color = Color.white;
            }

            // Right squad choice
            List<SquadStatus> temp = new List<SquadStatus>();
            foreach (SquadStatus s in rightArmy)
                if (s.outcome == 0)
                    temp.Add(s);
            rightChoice = temp[UnityEngine.Random.Range(0, temp.Count)];
        }
    }

    // Update is called once per frame.
    void Update()
    {
        // Leave the battle.
        if (Input.GetKeyDown(KeyCode.Escape)
            && (leftChoice == null)
            && !resultsPanel.gameObject.activeSelf)
            retreatWarning.SetActive(true);
        // When the battle is over.
        else if (gameOver)
        {
            countdown.enabled = true;
            countdown.text = "Escape to Play Again";
            instructions.gameObject.SetActive(false);
        }
        else
        {
            SquadManagement();
            CountdownManagement();
        }

        levelText.transform.localScale = Vector3.one * Mathf.Max(
            levelText.transform.localScale.x - Time.deltaTime * 2, 1);
    }

    void SquadManagement()
    {
        /// <summary>Manage choice canceling.</summary>

        if (Input.GetKeyDown(cancelKey))
        {
            leftChoice = null;
            PlaySound(cancelAudio);
        }
        instructions.gameObject.SetActive(leftChoice != null);
        instructions.text = cancelPrompt;
    }

    void CountdownManagement()
    {
        /// <summary>Manage the countdown timer when the players' choices have been made and move to the next scene when it's time.</summary>

        if ((leftChoice != null) && (rightChoice != null))
        {
            float oldTimer = countTimer;
            countTimer -= Time.deltaTime;

            // When the round begins.
            if (countTimer <= 0)
            {
                // In the tutorial level, right squads are chosen for specific color advantages.
                if
                    (!Master.FinishedTutorial()
                    && (roundsDone <= 1))
                    rightChoice = FindPreciseMatch(roundsDone == 1);

                Battle.leftSide = leftChoice.squad;
                Battle.rightSide = rightChoice.squad;
                SceneManager.LoadScene("Battle");
            }
            // When the countdown takes place.
            else if (countTimer <= 3)
            {
                countdown.enabled = true;
                countdown.text = $"{Mathf.CeilToInt(countTimer)}";
                // This formula is used to determine when each second passes.
                if (Mathf.Ceil(countTimer) == Mathf.Floor(oldTimer))
                    PlaySound(selectAudio);
            }
            else
                countdown.enabled = false;
        }
        else
        {
            countdown.enabled = false;
            countTimer = 4;
        }
    }

    void PlaySound
        (AudioClip sound,
        bool pitchVaries = false,
        float pitchBase = 1)
    {
        /// <summary>Play an audio clip.</summary>
        /// <param name="sound">The audio clip to play.</param>
        /// <param name="pitchVaries">If set to true, the pitch of the sound will vary randomly.</param>
        /// <param name="pitchBase">The default pitch of the sound. If pitchVaries is set to true, this is also the offset for randomization.</param>

        source.pitch = pitchBase;
        if (pitchVaries)
            source.pitch *= UnityEngine.Random.value / 2 + 0.75f;
        source.PlayOneShot(sound);
    }

    void PlayResultsSound
        (AudioClip sound,
        float pitchBase = 1)
    {
        /// <summary>Play an audio clip for the battle results.</summary>
        /// <param name="sound">The audio clip to play.</param>
        /// <param name="pitchBase">The default pitch of the sound. If pitchVaries is set to true, this is also the offset for randomization.</param>

        resultsAudio.pitch = pitchBase;
        resultsAudio.PlayOneShot(sound);
    }

    public void SetChoice(int squadIndex)
    {
        /// <summary>Set the player's choice.</summary>
        /// <param name="squadIndex">The index of the choice being made.</param>

        leftChoice = leftArmy[squadIndex];
        PlaySound(selectAudio);
    }

    public void Retreat()
    {
        leftDisplay.Die();
        resultsText.text = loseMessage;
        StartCoroutine(BattleResults(true));
    }

    public IEnumerator BattleResults(bool retreated = false)
    {
        int stars = 0;
        List<string> failedStars = new List<string>();
        void UpdateStarText(string text, bool success)
        {
            if (success)
            {
                starRequirements[stars]
                    .GetComponentInChildren<Image>(true)
                    .gameObject
                    .SetActive(true);
                starRequirements[stars].GetComponentInChildren<TMP_Text>().text = text;
                stars++;
            }
            else
                failedStars.Add(text);
        }

        // Update stars.
        int oldStars = Master.GetStars();
        if (!retreated && countTimer > 0)
        {
            UpdateStarText("FREE", true);
            UpdateStarText("FAIR MODE", fairStars >= 1);
            UpdateStarText("UNFAIR MODE", fairStars >= 2);

            // Check star challenges.
            foreach (StarChallenges.Challenge challenge
                in Master.levelSelected.starChallenges.challenges)
                UpdateStarText(challenge.GetDescription(true),
                    challenge.IsCompleted(StarChallenges.totalScore));
        }
        if (stars > oldStars)
            Master.SetStars(Master.levelSelected.levelId, stars);

        // Update level.
        int[] oldLevel = new int[2]
            { Master.data.level[0],
            Master.data.level[1] };
        int[] newLevel = Master.GetLevel();
        if ((newLevel[0] > oldLevel[0])
            || (newLevel[0] == oldLevel[0]
            && newLevel[1] > oldLevel[1]))
        {
            Master.data.level[1] = newLevel[1];
            while (Master.data.level[0] < newLevel[0])
            {
                Master.data.level[0]++;
                foreach (Player.ResourceQuantity resource in Master.data.income)
                    resource.quantity += 20;
            }
        }
        levelText.text = $"Level {oldLevel[0]}";
        float levelProgress = oldLevel[1] / (float)(oldLevel[0] / 10 + 2);
        levelMeter.value = levelProgress;

        // Refund unused squads and save.
        foreach (SquadStatus squad in leftArmy)
            if (squad.outcome != 0)
                Master.data.AddResource(squad.squad.colour, -squad.squad.startMoney / 2);
        Master.Save();

        // Panel fade
        resultsPanel.gameObject.SetActive(true);
        while (resultsPanel.color.a <= 0.5f)
        {
            resultsPanel.color += Color.black * Time.deltaTime / 2;
            music.volume -= Master.data.musicVolume * Time.deltaTime;
            yield return null;
        }
        resultsPanel.color = Color.black / 2;
        Destroy(music.gameObject);
        music = null;
        resultsWindow.SetActive(true);

        // Stars on win
        Image star;
        if (!retreated && countTimer > 0)
        {
            PlaySound(winSound);
            yield return new WaitForSeconds(0.5f);

            float lerp;
            bronzeFade = new Color
                (Master.bronzeColor.r,
                Master.bronzeColor.g,
                Master.bronzeColor.b,
                0);
            goldFade = new Color
                (Master.goldColor.r,
                Master.goldColor.g,
                Master.goldColor.b,
                0);
            int i = 0;
            while (i < stars)
            {
                star = starCount[i];
                lerp = 0;
                while (lerp < 1)
                {
                    star.transform.localScale = Vector3.one * (3 - lerp * 2);
                    star.color = (i < oldStars)
                        ? Color.Lerp(bronzeFade, Master.bronzeColor, lerp)
                        : Color.Lerp(goldFade, Master.goldColor, lerp);
                    yield return null;
                    lerp += Time.deltaTime * 8;
                }

                star.transform.localScale = Vector3.one;
                star.color = (i < oldStars)
                    ? Master.bronzeColor
                    : Master.goldColor;
                PlayResultsSound(i < oldStars ? bronzeSound : goldSound,
                    pitchBase: i / 4.0f + 1);
                starRequirements[i].gameObject.SetActive(true);
                i++;
                yield return new WaitForSeconds(0.375f);
            }
            yield return new WaitForSeconds(0.5f);

            // Animate level-up.
            float oldMeter = levelProgress, newMeter;
            if (oldLevel[0] < newLevel[0])
            {
                newMeter = 1;
                PlayResultsSound(levelSound);
                while (oldLevel[0] < newLevel[0])
                {
                    lerp = 0;
                    while (lerp < 1)
                    {
                        lerp += Time.deltaTime * 2;
                        levelMeter.value = lerp * newMeter
                            + (1 - lerp) * oldMeter;
                        yield return null;
                    }
                    oldLevel[0]++;
                    oldMeter = 0;
                    levelText.text = $"Level {oldLevel[0]}";
                    levelText.transform.localScale = Vector3.one * 2f;
                    PlayResultsSound(goldSound);
                }

                levelMeter.value = 0;
                if (newLevel[1] > 0)
                {
                    newMeter = newLevel[1] / (float)(newLevel[0] / 10 + 2);
                    lerp = 0;
                    while (lerp < 1)
                    {
                        lerp += Time.deltaTime * 2;
                        levelMeter.value = lerp * newMeter;
                        yield return null;
                    }
                    levelMeter.value = newMeter;
                }
                yield return new WaitForSeconds(1);
            }

            else if (oldLevel[0] == newLevel[0]
                && oldLevel[1] < newLevel[1])
            {
                newMeter = newLevel[1] / (float)(newLevel[0] / 10 + 2);
                PlayResultsSound(levelSound);
                lerp = 0;
                while (lerp < 1)
                {
                    lerp += Time.deltaTime * 2;
                    levelMeter.value = lerp * newMeter
                        + (1 - lerp) * oldMeter;
                    yield return null;
                }
                levelMeter.value = newMeter;
                yield return new WaitForSeconds(1);
            }

            int j = i;
            while (i < starRequirements.Count)
            {
                starRequirements[i].gameObject.SetActive(true);
                starRequirements[i].GetComponentInChildren<TMP_Text>().text = failedStars[i - j];
                starRequirements[i].GetComponentInChildren<TMP_Text>().color = Color.red;
                i++;
            }
        }

        // Stars on fail
        else
        {
            PlaySound(failSound);
            yield return new WaitForSeconds(1);
            starLossText.SetActive(true);
        }

        // Color Salvaged
        for (int i = 0; i < resourceQuantities.Count; i++)
            Master.RenderResourceQuantity(resourceQuantities[i],
                leftArmy[i].squad.colour,
                leftArmy[i].outcome == 0
                ? leftArmy[i].squad.startMoney / 2
                : Math.Min(leftArmy[i].squad.money, leftArmy[i].squad.startMoney));
        resultsBody.SetActive(true);

        levelProgress = newLevel[1] / (float)(newLevel[0] / 10 + 2);
        levelMeter.value = levelProgress;
    }

    public void Leave()
    {

        foreach (AudioSource a in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
            Destroy(a.gameObject);
        began = false;
        SceneManager.LoadScene("LevelSelect");
    }

    public SquadStatus FindPreciseMatch(bool invert)
    {
        /// <summary>Determines and returns the right squad that grants the lowest advantage/disadvantage possible, given the current left choice.</summary>
        /// <param name="invert">If set to true, the highest advantage/disadvantage will be returned instead.</param>

        SquadStatus result = null;
        float score;
        if (invert)
            score = 0;
        else
            score = 1;
        float a;
        foreach (SquadStatus s in rightArmy)
        {
            if (s.outcome == 0)
            {
                a = Mathf.Abs(Master
                    .colours[leftChoice.squad.colour]
                    .Advantage(Master.colours[s.squad.colour]));
                if (result == null)
                {
                    result = s;
                    score = a;
                }
                // Check if the current choice would grant a better score.
                else if (invert && a > score)
                {
                    result = s;
                    score = a;
                }
                else if (!invert && a < score)
                {
                    result = s;
                    score = a;
                }
            }
        }
        return result;
    }
}

[Serializable]
public class SquadStatus
{
    /// <summary>
    /// Contains metadata about a specific squad in battle, including what button corresponds to it and the outcome when it was used.
    /// </summary>

    // Variables.
    public Squad squad;
    public GameObject button;
    [HideInInspector] public int outcome = 0;
}