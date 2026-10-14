using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SquadSelect : MonoBehaviour
{
    /// <summary>
    /// Manages the squad selection menu in battle.
    /// </summary>

    // Global variables.
    public static bool began = false, artifactTutorial = true;
    public static int roundsDone, fairStars;

    // Serialized variables for the editor.
    [Header("Menu")]
    public List<SquadStatus> leftSquads;
    public List<SquadStatus> rightSquads;
    [SerializeField] GameObject splashScreen;
    [SerializeField] UnitDisplay leftDisplay, rightDisplay;
    [SerializeField] Image advantageDiagram;
    [SerializeField] string cancelPrompt, winMessage, loseMessage;
    [SerializeField] KeyCode cancelKey;
    [SerializeField] AudioClip selectAudio, cancelAudio;
    [SerializeField] Battle battle;
    [SerializeField] GameObject battleMenu;
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
    [SerializeField] TMP_Text pointsText;
    [SerializeField] GameObject starLossText;
    [SerializeField] List<Image> resourceQuantities;

    // Miscellaneous variables.
    bool gameOver = false;
    // Used after the game concludes to determine which side won.
    float totalOutcome;
    SquadStatus leftChoice, rightChoice;
    AudioSource source;
    Color bronzeFade, goldFade;

    // Start is called before the first frame update.
    void Start()
    {
        source = GetComponent<AudioSource>();
        source.volume = Master.data.sfxVolume;
        resultsAudio.volume = Master.data.sfxVolume;
        StartCoroutine(Master.DisableSplashScreen(splashScreen, 0.25f));
        leftDisplay.ChangeUnit
            (Master.data.character,
            "neutral", true);
        rightDisplay.ChangeUnit
            (Master.battleSelected.opponentDisplay,
            "neutral", true);
        advantageDiagram.gameObject.SetActive
            (Master.data.events.Contains("squad_select")
            || Master.data.events.Contains("skip_tutorial"));

        // Squads
        List<Squad> squads = Master
            .battleSelected
            .ChooseSquads();
        for (int s = 0; s < squads.Count; s++)
        {
            leftSquads[s].squad = Master.leftSquads[s];
            rightSquads[s].squad = squads[s];
        }

        roundsDone = 0;
        began = true;

        // Button colors, text, and resources
        foreach (SquadStatus squad in leftSquads)
        {
            squad.button.UpdateSquad(squad.squad);
            Master.data.AddResource(squad.squad.colour, -squad.squad.startMoney / 2);
        }
        foreach (SquadStatus squad in rightSquads)
            squad.button.UpdateSquad(squad.squad);
        Master.Save();

        Master.RefreshMusic(Master.battleSelected.settings.music);
        StarChallenges.totalScore.Reset();
        ChooseRightSquad();
    }

    // Update is called once per frame.
    void Update()
    {
        if (!Battle.inBattle)
        {
            // Leave the battle.
            if (Input.GetKeyDown(KeyCode.Escape)
                && DialogueScene.isDone)
            {
                if (leftChoice != null)
                {
                    leftChoice = null;
                    PlaySound(cancelAudio);
                }
                else if (resultsBody.activeSelf)
                    Leave();
                else if (!gameOver)
                    retreatWarning.SetActive(!retreatWarning.activeSelf);
            }

            // When the battle is over.
            levelText.transform.localScale = Vector3.one * Mathf.Max(
                levelText.transform.localScale.x - Time.deltaTime * 2, 1);
        }
    }

    void ChooseRightSquad()
    {
        // Right squad choice
        List<SquadStatus> temp = new List<SquadStatus>();
        foreach (SquadStatus s in rightSquads)
            if (s.outcome == 0)
                temp.Add(s);
        rightChoice = temp[UnityEngine.Random.Range(0, temp.Count)];
    }

    public void NextRound()
    {
        Master.OpenMenu(transform.parent.gameObject, battleMenu);
        advantageDiagram.gameObject.SetActive
            (Master.data.events.Contains("squad_select")
            || Master.data.events.Contains("skip_tutorial"));

        gameOver = true;
        for (int n = 0; n < leftSquads.Count; n++)
        {
            // Game over if all squads have been used.
            if (leftSquads[n].outcome == 0)
                gameOver = false;
            else
                leftSquads[n].button.gameObject.SetActive(false);

            if (rightSquads[n].outcome != 0)
                rightSquads[n].button.gameObject.SetActive(false);
        }

        if (gameOver)
        {
            totalOutcome = 0;
            foreach (SquadStatus s in leftSquads)
                totalOutcome += s.outcome;

            // Die animation on game over.
            if (totalOutcome > 0)
            {
                rightDisplay.SetAnimation(UnitDisplay.AnimState.Die);
                resultsText.text = winMessage;
            }
            else
            {
                leftDisplay.SetAnimation(UnitDisplay.AnimState.Die);
                resultsText.text = loseMessage;
            }
            StartCoroutine(BattleResults());
        }
        else
        {
            leftChoice = null;
            ChooseRightSquad();
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

        leftChoice = leftSquads[squadIndex];
        //PlaySound(selectAudio);
        // In the tutorial, right squads are chosen differently.
        if
            (!Master.FinishedTutorial()
            && (roundsDone <= 1))
            rightChoice = FindPreciseMatch(roundsDone == 1);
        else if (!Master.FinishedTutorial("basic_2"))
            rightChoice = FindEasiestMatch();

        Battle.leftSide = leftChoice.squad;
        Battle.rightSide = rightChoice.squad;
        Master.OpenMenu(battleMenu,
            transform.parent.gameObject);
        battle.StartBattle();
    }

    public void Retreat()
    {
        leftDisplay.SetAnimation(UnitDisplay.AnimState.Die);
        resultsText.text = loseMessage;
        gameOver = true;
        StartCoroutine(BattleResults(true));
    }

    public IEnumerator BattleResults(bool retreated = false)
    {
        // Methods
        int researchGained = 0, upgradesGained = 0;
        void UpdatePoints()
        {
            pointsText.text = $"{researchGained} research point";
            if (researchGained > 1)
                pointsText.text += "s";
            if (upgradesGained > 0)
            {
                pointsText.text += $"\n{upgradesGained} upgrade point";
                if (upgradesGained > 1)
                    pointsText.text += "s";
            }
        }

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
        if (!retreated && totalOutcome > 0)
        {
            UpdateStarText("FREE", true);
            UpdateStarText("FAIR MODE", fairStars >= 1);
            UpdateStarText("UNFAIR MODE", fairStars >= 2);

            // Check star challenges.
            foreach (StarChallenges.Challenge challenge
                in Master.battleSelected.starChallenges.challenges)
                UpdateStarText(challenge.GetDescription(true),
                    challenge.IsCompleted(StarChallenges.totalScore));
        }
        if (stars > oldStars)
        {
            Master.SetStars(Master.battleSelected.battleId, stars);
            Master.data.researchPoints += stars - oldStars;
        }

        // Update level.
        int[] oldLevel = new int[2]
            { Master.data.level,
            Master.data.starsLeftover };
        int[] newLevel = Master.GetLevel();
        if ((newLevel[0] > oldLevel[0])
            || (newLevel[0] == oldLevel[0]
            && newLevel[1] > oldLevel[1]))
        {
            Master.data.starsLeftover = newLevel[1];
            while (Master.data.level < newLevel[0])
            {
                Master.data.level++;
                Master.data.upgradePoints++;
                foreach (Player.ResourceQuantity resource in Master.data.income)
                    resource.quantity += (Master.data.level > 10) ? 40 : 20;
            }
        }
        levelText.text = $"Level {oldLevel[0]}";
        float levelProgress = oldLevel[1] / (float)(oldLevel[0] / 10 + 2);
        levelMeter.value = levelProgress;

        // Refund unused squads.
        foreach (SquadStatus squad in leftSquads)
            if (squad.outcome != 0)
                Master.data.AddResource(squad.squad.colour, -squad.squad.startMoney / 2);

        // Destroy broken artifacts.
        List<Artifact> broken = new List<Artifact>();
        foreach (ArtifactList list in Master.data.artifacts)
        {
            broken.Clear();
            foreach (Artifact artifact in list.artifacts)
                if (artifact.uses <= 0)
                    broken.Add(artifact);
            foreach (Artifact artifact in broken)
                list.artifacts.Remove(artifact);
        }

        Master.Save();

        // Panel fade
        resultsPanel.gameObject.SetActive(true);
        while (resultsPanel.color.a <= 0.5f)
        {
            resultsPanel.color += Color.black * Time.deltaTime / 2;
            Master.backgroundMusic.volume
                -= Master.data.musicVolume * Time.deltaTime;
            yield return null;
        }
        resultsPanel.color = Color.black / 2;
        Master.backgroundMusic.volume = 0;
        resultsWindow.SetActive(true);

        // Stars on win
        Image star;
        if (!retreated && totalOutcome > 0)
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

                if (i >= oldStars)
                {
                    researchGained++;
                    UpdatePoints();
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
                    upgradesGained++;
                    UpdatePoints();
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
                leftSquads[i].squad.colour,
                leftSquads[i].outcome == 0
                ? leftSquads[i].squad.startMoney / 2
                : Math.Min(leftSquads[i].squad.paint, leftSquads[i].squad.startMoney));
        resultsBody.SetActive(true);

        levelProgress = newLevel[1] / (float)(newLevel[0] / 10 + 2);
        levelMeter.value = levelProgress;
    }

    public void Leave()
    {
        began = false;
        StartCoroutine(Master.GotoScene("Town"));
    }

    public SquadStatus FindPreciseMatch(bool invert = false)
    {
        /// <summary>Determines and returns the right squad that grants the lowest advantage/disadvantage possible, given the current left choice.</summary>
        /// <param name="invert">If set to true, the highest advantage/disadvantage will be returned instead.</param>

        SquadStatus result = null;
        float score;
        score = invert ? 0 : 1;
        float a;
        foreach (SquadStatus s in rightSquads)
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

    public SquadStatus FindEasiestMatch(bool invert = false)
    {
        /// <summary>Determines and returns the right squad that grants the highest advantage possible, given the current left choice.</summary>
        /// <param name="invert">If set to true, the lowest advantage will be returned instead.</param>

        SquadStatus result = null;
        float score;
        score = invert ? 1 : -1;
        float a;
        foreach (SquadStatus s in rightSquads)
        {
            if (s.outcome == 0)
            {
                a = Master
                    .colours[leftChoice.squad.colour]
                    .Advantage(Master.colours[s.squad.colour]);
                if (result == null)
                {
                    result = s;
                    score = a;
                }
                // Check if the current choice would grant a better score.
                else if (invert && a < score)
                {
                    result = s;
                    score = a;
                }
                else if (!invert && a > score)
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
    public SquadOption button;
    [HideInInspector] public int outcome = 0;
}